using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using PropertyKwikCheck.Api.Background;
using Microsoft.IdentityModel.Tokens;
using PropertyKwikCheck.Api.Middleware;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Infrastructure.Data;
using PropertyKwikCheck.Infrastructure.Security;
using PropertyKwikCheck.Infrastructure.Services;
using PropertyKwikCheck.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);

// ---- configuration ----------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("PropertyDb")
    ?? throw new InvalidOperationException("ConnectionStrings:PropertyDb is not configured.");

var jwt = new JwtOptions();
builder.Configuration.GetSection(JwtOptions.SectionName).Bind(jwt);
if (string.IsNullOrWhiteSpace(jwt.SigningKey))
    throw new InvalidOperationException("Jwt:SigningKey is not configured.");

// ---- services ---------------------------------------------------------------
builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never);

builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IDbConnectionFactory>(_ => new SqlConnectionFactory(connectionString));

var storageOptions = new StorageOptions();
builder.Configuration.GetSection(StorageOptions.SectionName).Bind(storageOptions);
builder.Services.AddSingleton(storageOptions);
builder.Services.AddSingleton<IFileStorage>(_ => new LocalFileStorage(storageOptions));

builder.Services.AddScoped<ILeadRepository, LeadRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IReportingRepository, ReportingRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IPhotoRepository, PhotoRepository>();

builder.Services.AddScoped<ILeadService, LeadService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDirectoryService, DirectoryService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IScreenService, ScreenService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IReportPdfService, PropertyKwikCheck.Infrastructure.Pdf.ReportPdfService>();

// Background TAT recompute (spec §12).
builder.Services.AddHostedService<TatRecalculationService>();

// Rate limiting — protect the auth endpoints from brute force (spec §14).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

// ---- auth -------------------------------------------------------------------
JwtSecurityTokenHandler.DefaultMapInboundClaims = false; // keep raw claim names (sub, roleId, ...)
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Everything requires auth unless explicitly [AllowAnonymous].
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// ---- CORS -------------------------------------------------------------------
var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
    ?? ["http://localhost:3000", "http://localhost:5173"];
builder.Services.AddCors(o => o.AddPolicy("frontend", p => p
    .WithOrigins(corsOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

// ---- Swagger ----------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ---- pipeline ---------------------------------------------------------------
app.UseMiddleware<ErrorHandlingMiddleware>();

// Swagger enabled in all environments (registered before auth so the UI is reachable).
app.UseSwagger();
app.UseSwaggerUI();

// Serve the React SPA from wwwroot (single-domain hosting): static assets + index.html.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SPA client-side routing: any non-API, non-file path serves index.html. Anonymous so
// deep links (e.g. /leads) aren't caught by the fallback auth policy.
app.MapFallbackToFile("index.html").AllowAnonymous();

app.Run();

/// <summary>Exposed for WebApplicationFactory-based integration tests.</summary>
public partial class Program;
