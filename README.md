# PropertyKwikCheck

Backend for the **KwikCheck Valuation Workflow Console** — an operations console for a
property-valuation agency performing collateral valuations for banks/NBFCs.

- **API:** ASP.NET Core (.NET 10) Web API, **Dapper** + hand-written T-SQL, **SQL Server**.
- **Auth:** JWT access tokens + rotating refresh tokens; RBAC over the project's own
  **Roles (4)** + **UserTypes (20)** identity model.
- **Frontend:** React (Phase 2 — see the design prototype). Not yet in this repo.
- Contract is defined by `BACKEND_SPEC.md` (the §-references throughout the code point to it).

## Layout

```
db/                      schema.sql + seed.sql (T-SQL)
src/
  PropertyKwikCheck.Core            domain, DTOs, enums, RBAC policy, stage machine, mappers, interfaces
  PropertyKwikCheck.Infrastructure  Dapper repositories + services (lead/auth/directory/analytics/screen)
  PropertyKwikCheck.Api             controllers, JWT auth, RBAC, error envelope, Swagger, DI
  web/                              (placeholder — React app, Phase 2)
tests/
  PropertyKwikCheck.Tests           xUnit + Moq + FluentAssertions
```

## RBAC — how the workflow maps onto Roles/UserTypes

The spec describes 9 abstract functional roles; this project keeps its own **Role + UserType**
identity tables and maps each `UserType` to a set of workflow **capabilities** + a row **scope**
in [`RbacPolicy`](src/PropertyKwikCheck.Core/Rbac/RbacPolicy.cs). Summary:

| UserType | Acts as | Scope |
|---|---|---|
| Super Admin (19), Admin (16) | full access | all |
| State Coordinator (9) | Lead Manager + Data Entry | all |
| State Head (10), Zonal Head (13) | Lead Manager + analytics | all |
| Qc Manager (11) | QC approve/hold | all |
| Pricing Manager (12) | Pricing sign-off + billing | all |
| National/Business Head (14,15) | Authoriser | all |
| RO admin (7), Cando Admin (18) | assign/yard | all |
| RO Valuators (8), CANDO VALUATOR (17) | field valuer | own leads |
| Client* / Cando Executive (1–6,20) | bank coordinator | own company |

Capability failure → `403 { error, code: "RBAC_DENIED" }`; single-resource scope failure → `404`.

## Setup

### 1. Configure secrets (never committed)
Put the real connection string + a 32+ char JWT key in
`src/PropertyKwikCheck.Api/appsettings.Development.json` (gitignored) — a template is already there.

### 2. Create the database schema + seed
Pass credentials via the environment (don't hard-code them in scripts or docs):
```powershell
$env:SQLCMDPASSWORD = "<db-password>"
sqlcmd -S <server> -d PropertyDB -U <db-user> -C -i db/schema.sql
sqlcmd -S <server> -d PropertyDB -U <db-user> -C -i db/seed.sql
```
Both scripts are idempotent (safe to re-run). Seeds the 4 Roles, 20 UserTypes, lookup lists,
sample companies/users, and the four "hero" leads (`4812`, `4913`, `4927`, `4790`).

### 3. Run
```powershell
dotnet run --project src/PropertyKwikCheck.Api
```
Swagger UI at `/swagger` (Development). Health at `/api/health`.

### 4. Test
```powershell
dotnet test
```

## Seeded logins (password: `Password@123`)

| Email | UserType |
|---|---|
| superadmin@kwikcheck.in | Super Admin |
| qc@kwikcheck.in | Qc Manager |
| pricing@kwikcheck.in | Pricing Manager |
| rahul@kwikcheck.in | RO Valuators |
| meena.p@kwikcheck.in | Client Executive |

## API (Phase 1)

`POST /api/auth/{login,refresh,logout}` · `GET /api/auth/me`
`GET/POST /api/leads` · `GET/PATCH/DELETE /api/leads/{id}`
`GET/POST /api/users` · `PATCH/DELETE /api/users/{id}` · `GET /api/roles` · `GET /api/usertypes`
`GET/POST /api/companies` · `PATCH/DELETE /api/companies/{id}`
`GET /api/meta` · `GET /api/analytics` · `GET /api/screens/{name}` · `GET /api/health`

## Not yet implemented (later phases)
- Document/photo uploads + object storage (spec §8.11, §10)
- Report PDF generation (spec §8.12, §11)
- TAT background job (computed at read time for now, spec §12)
- React frontend (Phase 2)
- Some analytics series (state/district/trend) use representative data pending richer aggregation.
- The hero leads' `report_data` is representative; sync verbatim from the frontend `lib/sampleData.js` when available so issued PDFs match the reference designs.
