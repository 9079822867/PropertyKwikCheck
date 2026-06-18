# Deploy — single domain (API + SPA together)

The React SPA and the .NET API run as **one app on one domain** (`https://property.kwikcheck.in`):

- `/api/*` → API controllers
- `/swagger` → API docs
- everything else → the React SPA (client-side routing falls back to `index.html`)

No CORS, no second host. The SPA calls same-origin `/api`.

## Build & publish

From the repo root (`D:\Development\PropertyKwikCheck`):

```powershell
# 1. Build the SPA (same-origin /api)
cd web
npm install
npm run build

# 2. Copy the build into the API's wwwroot
cd ..
New-Item -ItemType Directory -Force src\PropertyKwikCheck.Api\wwwroot | Out-Null
Copy-Item web\dist\* src\PropertyKwikCheck.Api\wwwroot -Recurse -Force

# 3. Publish the API (includes wwwroot + appsettings.Production.json)
dotnet publish src\PropertyKwikCheck.Api -c Release -o publish
```

`publish\` now contains the API **and** the SPA. Deploy that folder to the IIS site
(overwrite the app folder, next to `PropertyKwikCheck.Api.dll`) and **recycle the app pool**.

## Server config (one of)

The DB connection + JWT key must be present in production. Either:

- the bundled **`web.config`** has them as `<environmentVariables>` (already set in `publish\web.config`), **or**
- a server-side **`appsettings.Production.json`** (also bundled), **or**
- IIS app-pool environment variables: `ConnectionStrings__PropertyDb`, `Jwt__SigningKey`.

`ASPNETCORE_ENVIRONMENT` defaults to `Production` — no need to set it explicitly.

## Verify after deploy

```
GET  https://property.kwikcheck.in/             -> the SPA loads
GET  https://property.kwikcheck.in/api/health   -> {"status":"ok"}
POST https://property.kwikcheck.in/api/auth/login {"email":"superadmin@kwikcheck.in","password":"Password@123"} -> token
GET  https://property.kwikcheck.in/swagger      -> API explorer
```

## Notes
- `src/PropertyKwikCheck.Api/wwwroot/` is gitignored — it's regenerated from `web/dist` at build time.
- For local dev you don't need this: run the API (`dotnet run`) and `cd web; npm start` (proxy handles `/api`).
