# PropertyKwikCheck — Web (React)

React SPA for the KwikCheck valuation console. Talks to the .NET API at the same-origin
`/api` path (BACKEND_SPEC §5); in dev, Vite proxies `/api` to the backend.

**Stack:** Vite + React 18 + React Router 6 + TanStack Query 5 + Axios.

> Node.js is **not installed** on the build machine, so dependencies were not installed and
> the app was not run/verified here. Install Node 18+ and run the steps below to start it.

## Run

```powershell
# 1. Start the backend first (from the repo root), so /api is reachable:
dotnet run --project src/PropertyKwikCheck.Api      # serves http://localhost:5080

# 2. Then the frontend:
cd web
copy .env.example .env        # optional; defaults to http://localhost:5080
npm install
npm run dev                   # http://localhost:5173
```

Open http://localhost:5173 and sign in with a seeded account
(e.g. `superadmin@kwikcheck.in` / `Password@123`).

## What's implemented (Phase 2, first cut)

- **Auth**: login, JWT bearer on every request, automatic refresh-on-401 with retry,
  session restore via `/api/auth/me`, sign-out ([src/lib/api.js](src/lib/api.js), [src/lib/auth.jsx](src/lib/auth.jsx)).
- **App shell**: topbar + sidebar with live bucket counts from `/api/meta`.
- **Dashboard**: stat cards, KPIs, and recent leads from `/api/analytics`.
- **Leads**: bucket list with server-side search (`/api/leads?bucket&q`), row → detail.
- **Lead detail**: header, key fields, and the full report `data` from `/api/leads/{id}`.

RBAC is enforced by the backend — the UI surfaces `{error}` envelopes (e.g. a 403 on
`/api/analytics` for roles without analytics access) inline.

## Structure

```
src/
  lib/        api.js (axios + interceptors), auth.jsx, queries.js, constants.js, format.js
  components/ Layout, Topbar, Sidebar, Icon, ProtectedRoute, ui (Pill/Spinner/…)
  pages/      Login, Dashboard, Leads, LeadDetail, NotFound
```

## Not yet built (next)
- 5-stage report wizard (intake → site → technical → valuation → photos)
- Users / Companies / Billing / Yard / MIS / Reports / Documents / Master screens
- Lead create + assign/reassign/reject actions, document & photo upload, PDF download
