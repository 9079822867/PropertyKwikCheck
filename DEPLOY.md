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

The committed `src\PropertyKwikCheck.Api\web.config` is **secret-free** (it only removes the
WebDAV module and wires the ASP.NET Core handler), so a fresh `publish\web.config` will **not**
contain the DB password or JWT key. Supply them in production via one of:

- add an `<environmentVariables>` block to `publish\web.config` after publishing (keys
  `ConnectionStrings__PropertyDb`, `Jwt__SigningKey`, optionally `ASPNETCORE_ENVIRONMENT`), **or**
- a server-side **`appsettings.Production.json`** (bundled, gitignored), **or**
- IIS app-pool environment variables.

`ASPNETCORE_ENVIRONMENT` defaults to `Production` — no need to set it explicitly.

> **WebDAV / 405 on DELETE & PUT** — IIS's WebDAVModule answers `DELETE`/`PUT` with
> `405 Method Not Allowed` before they reach the app. The committed `web.config` removes it
> (`<remove name="WebDAVModule"/>` + handler removal). If you ever hand-edit the deployed
> `web.config`, keep those removals or photo/user/company deletes will 405 again.

## Verify after deploy

```
GET  https://property.kwikcheck.in/             -> the SPA loads
GET  https://property.kwikcheck.in/api/health   -> {"status":"ok"}
POST https://property.kwikcheck.in/api/auth/login {"email":"superadmin@kwikcheck.in","password":"Password@123"} -> token
GET  https://property.kwikcheck.in/swagger      -> API explorer
DELETE https://property.kwikcheck.in/api/photos/{id}  (with bearer) -> {"ok":true}, NOT 405
```

## Notes
- `src/PropertyKwikCheck.Api/wwwroot/` is gitignored — it's regenerated from `web/dist` at build time.
- For local dev you don't need this: run the API (`dotnet run`) and `cd web; npm start` (proxy handles `/api`).
