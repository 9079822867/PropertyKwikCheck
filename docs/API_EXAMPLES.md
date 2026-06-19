# PropertyKwikCheck — API Examples

Every endpoint with an example request and response.

- **Base URL** — `https://property.kwikcheck.in/api` (prod) · `http://localhost:5107/api` (local). All routes below are relative to `/api`.
- **Auth** — JWT Bearer. Send `Authorization: Bearer <token>` on every endpoint except `auth/login`, `auth/refresh`, and `health`.
- **Content type** — `application/json` for bodies, except the file uploads (`multipart/form-data`).
- **Error envelope** — any 4xx/5xx returns:
  ```json
  { "error": "Lead not found", "code": "NOT_FOUND", "details": null }
  ```
  Codes: `VALIDATION` (400), `UNAUTHORIZED` (401), `FORBIDDEN` (403), `NOT_FOUND` (404), `INTERNAL` (500).

---

## Auth

### POST `/auth/login`  — _anonymous, rate-limited_
```http
POST /api/auth/login
Content-Type: application/json

{ "email": "superadmin@kwikcheck.in", "password": "Password@123" }
```
```json
200 OK
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "b9f3c0e2-5a1d-4f7e-9c2a-1e6d8b0a3f44",
  "user": {
    "id": 1, "name": "Super Admin", "email": "superadmin@kwikcheck.in",
    "role": "Internal", "userType": "Super Admin", "company": null,
    "phone": null, "licenceNo": null, "status": "Active", "leads": "—"
  }
}
```
Bad credentials → `401 { "error": "Invalid credentials", "code": "UNAUTHORIZED" }`.

### POST `/auth/refresh`  — _anonymous, rate-limited_
```http
POST /api/auth/refresh
Content-Type: application/json

{ "refreshToken": "b9f3c0e2-5a1d-4f7e-9c2a-1e6d8b0a3f44" }
```
```json
200 OK
{ "token": "eyJhbGciOiJIUzI1...", "refreshToken": "c1a4d2f6-..." }
```

### POST `/auth/logout`
```http
POST /api/auth/logout
Authorization: Bearer <token>
Content-Type: application/json

{ "refreshToken": "b9f3c0e2-5a1d-4f7e-9c2a-1e6d8b0a3f44" }
```
```json
200 OK
{ "ok": true }
```

### GET `/auth/me`
```http
GET /api/auth/me
Authorization: Bearer <token>
```
```json
200 OK
{
  "id": 1, "name": "Super Admin", "email": "superadmin@kwikcheck.in",
  "role": "Internal", "userType": "Super Admin", "company": null,
  "phone": null, "licenceNo": null, "status": "Active", "leads": "—"
}
```

---

## Leads

### GET `/leads`  — list / search, scoped to the caller
Query params: `bucket` (default `assigned`), `q`, `page` (1), `pageSize`, `sort`.
```http
GET /api/leads?bucket=qc&q=4812&page=1&pageSize=20
Authorization: Bearer <token>
```
```json
200 OK
{
  "rows": [
    {
      "id": 4812, "reqId": "REQ-4812", "type": "Bank Valuation", "ptype": "residential",
      "stage": "qc", "reportStatus": "in_review",
      "applicant": "Ramesh Kumar", "coApplicant": null, "contact": "98xxxxxx10",
      "pin": "560001", "location": "Bengaluru, KA",
      "lender": "HDFC Bank", "branch": "MG Road", "valuator": "Rahul Mehta",
      "roCompany": "Acme Valuers", "tatPct": 60, "tatState": "ok",
      "value": 7500000, "leadDate": "2026-06-10", "assignedOn": "2026-06-11",
      "data": null
    }
  ],
  "total": 1,
  "counts": { "assigned": 12, "qc": 4, "ro_confirmation": 3, "completed": 27, "rejected": 2 }
}
```

### GET `/leads/{id}`  — full detail (includes the `data` report payload)
```http
GET /api/leads/4812
Authorization: Bearer <token>
```
```json
200 OK
{
  "id": 4812, "reqId": "REQ-4812", "type": "Bank Valuation", "ptype": "residential",
  "stage": "qc", "reportStatus": "in_review",
  "applicant": "Ramesh Kumar", "contact": "98xxxxxx10", "pin": "560001",
  "location": "Bengaluru, KA", "lender": "HDFC Bank", "valuator": "Rahul Mehta",
  "tatPct": 60, "tatState": "ok", "value": 7500000,
  "remarks": null, "holdRemarks": null,
  "data": { "fairMarketValue": 7500000, "landArea": 1200, "builtUpArea": 1850 }
}
```
Not found → `404 { "error": "Lead not found", "code": "NOT_FOUND" }`.

### POST `/leads`  — create
```http
POST /api/leads
Authorization: Bearer <token>
Content-Type: application/json

{
  "ptype": "residential",
  "data": { "applicant": "Sita Sharma", "pin": "411001", "lender": "ICICI Bank" }
}
```
```json
201 Created
Location: /api/leads/4951
{ "id": 4951, "reqId": "REQ-4951", "ptype": "residential", "stage": "assigned",
  "reportStatus": "draft", "applicant": "Sita Sharma", "pin": "411001",
  "lender": "ICICI Bank", "data": { ... } }
```

### PATCH `/leads/{id}`  — edit / reassign / reject
`action` discriminates: omit it for an edit-save (deep-merges `data`, assigns columns); `"reassign"` sets `valuator`; `"reject"` moves to rejected.
```http
PATCH /api/leads/4812
Authorization: Bearer <token>
Content-Type: application/json

{ "action": "reassign", "valuator": "Meena Patil" }
```
```json
200 OK
{ "id": 4812, "stage": "qc", "valuator": "Meena Patil", ... }
```
Edit-save example:
```json
{ "value": 7600000, "reportStatus": "approved",
  "data": { "fairMarketValue": 7600000 } }
```

### DELETE `/leads/{id}`
```http
DELETE /api/leads/4951
Authorization: Bearer <token>
```
```json
200 OK
{ "ok": true }
```

---

## Users

### GET `/users`
```http
GET /api/users
Authorization: Bearer <token>
```
```json
200 OK
[
  { "id": 5, "name": "Rahul Mehta", "email": "rahul@kwikcheck.in",
    "role": "RO", "userType": "RO Valuator", "company": "Acme Valuers",
    "phone": "99xxxxxx01", "licenceNo": "VAL-2021-118", "status": "Active", "leads": "8" }
]
```

### POST `/users`  — create
```http
POST /api/users
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Anita Rao", "email": "anita@kwikcheck.in",
  "roleId": 2, "userTypeId": 7, "companyId": 3,
  "phone": "98xxxxxx22", "licenceNo": "VAL-2024-009",
  "status": "Active", "password": "Password@123"
}
```
```json
201 Created
{ "id": 21, "name": "Anita Rao", "email": "anita@kwikcheck.in",
  "role": "RO", "userType": "RO Valuator", "company": "Acme Valuers",
  "phone": "98xxxxxx22", "licenceNo": "VAL-2024-009", "status": "Active", "leads": "—" }
```

### PATCH `/users/{id}`  — update (all fields optional; no `email`)
```http
PATCH /api/users/21
Authorization: Bearer <token>
Content-Type: application/json

{ "name": "Anita Rao Kulkarni", "userTypeId": 8, "status": "Inactive", "password": "NewPass@123" }
```
```json
200 OK
{ "id": 21, "name": "Anita Rao Kulkarni", "email": "anita@kwikcheck.in",
  "role": "RO", "userType": "Cando Valuator", "status": "Inactive", ... }
```

### DELETE `/users/{id}`  — disable
```http
DELETE /api/users/21
Authorization: Bearer <token>
```
```json
200 OK
{ "ok": true }
```

### GET `/roles`  — lookup for the user form
```http
GET /api/roles
Authorization: Bearer <token>
```
```json
200 OK
[ { "id": 1, "roleName": "Client" }, { "id": 2, "roleName": "RO" },
  { "id": 3, "roleName": "Internal" }, { "id": 4, "roleName": "Cando" } ]
```

### GET `/usertypes`  — lookup for the user form
```http
GET /api/usertypes
Authorization: Bearer <token>
```
```json
200 OK
[ { "id": 1, "name": "Super Admin" }, { "id": 7, "name": "RO Valuator" },
  { "id": 8, "name": "Cando Valuator" }, ... ]
```

---

## Companies

### GET `/companies`
```http
GET /api/companies
Authorization: Bearer <token>
```
```json
200 OK
[ { "id": 3, "name": "Acme Valuers", "type": "Valuation Agency · Owner",
    "spoc": "Vikram Shah", "leads": 42, "active": 9, "status": "Active" } ]
```

### POST `/companies`  — create
```http
POST /api/companies
Authorization: Bearer <token>
Content-Type: application/json

{ "name": "Sunrise NBFC", "type": "Lender · NBFC", "spoc": "Priya N", "status": "Active" }
```
```json
201 Created
{ "id": 11, "name": "Sunrise NBFC", "type": "Lender · NBFC",
  "spoc": "Priya N", "leads": 0, "active": 0, "status": "Active" }
```
> Note: `gstin`, `pan`, `spocEmail`, `spocPhone`, `defaultTat`, `address` may be sent but are not yet persisted.

### PATCH `/companies/{id}`  — update (all fields optional)
```http
PATCH /api/companies/11
Authorization: Bearer <token>
Content-Type: application/json

{ "name": "Sunrise Finance Ltd", "status": "Inactive" }
```
```json
200 OK
{ "id": 11, "name": "Sunrise Finance Ltd", "type": "Lender · NBFC",
  "spoc": "Priya N", "status": "Inactive" }
```

### DELETE `/companies/{id}`
```http
DELETE /api/companies/11
Authorization: Bearer <token>
```
```json
200 OK
{ "ok": true }
```

---

## Documents  _(multipart upload)_

### POST `/leads/{leadId}/documents`  — max 10 MB
```http
POST /api/leads/4812/documents
Authorization: Bearer <token>
Content-Type: multipart/form-data

file=<binary>; docType=title_deed
```
```json
201 Created
{ "id": 88, "leadId": 4812, "docType": "title_deed",
  "fileName": "title_deed.pdf", "mime": "application/pdf",
  "sizeBytes": 482113, "uploadedAt": "2026-06-19T10:22:00Z",
  "downloadUrl": "/api/documents/88/download" }
```

### GET `/leads/{leadId}/documents`
```http
GET /api/leads/4812/documents
Authorization: Bearer <token>
```
```json
200 OK
[ { "id": 88, "leadId": 4812, "docType": "title_deed", "fileName": "title_deed.pdf",
    "mime": "application/pdf", "sizeBytes": 482113,
    "uploadedAt": "2026-06-19T10:22:00Z", "downloadUrl": "/api/documents/88/download" } ]
```

### GET `/documents/{docId}/download`
Returns the raw file bytes with its content type (binary, not JSON).

### DELETE `/documents/{docId}`
```json
200 OK
{ "ok": true }
```

---

## Photos / site-visit frames  _(multipart upload)_

### POST `/leads/{leadId}/photos`  — max 100 MB
Form fields: `file`, `frameLabel`, `kind` (default `photo`), `lat`, `lng`, `capturedAt` (ISO).
```http
POST /api/leads/4812/photos
Authorization: Bearer <token>
Content-Type: multipart/form-data

file=<binary>; frameLabel=Front Elevation; kind=photo; lat=12.9716; lng=77.5946; capturedAt=2026-06-18T14:05:00Z
```
```json
201 Created
{ "id": 301, "leadId": 4812, "kind": "photo", "frameLabel": "Front Elevation",
  "mime": "image/jpeg", "sizeBytes": 1840221, "lat": 12.9716, "lng": 77.5946,
  "capturedAt": "2026-06-18T14:05:00Z", "uploadedAt": "2026-06-19T09:10:00Z",
  "downloadUrl": "/api/photos/301/download" }
```

### GET `/leads/{leadId}/photos`
```json
200 OK
[ { "id": 301, "leadId": 4812, "kind": "photo", "frameLabel": "Front Elevation",
    "mime": "image/jpeg", "lat": 12.9716, "lng": 77.5946,
    "downloadUrl": "/api/photos/301/download" } ]
```

### GET `/photos/{photoId}/download`
Returns the raw image bytes (binary).

### DELETE `/photos/{photoId}`
```json
200 OK
{ "ok": true }
```

---

## Reports

### GET `/leads/{id}/report.pdf`
Renders the valuation report as a PDF (served inline). Binary `application/pdf`, not JSON.
```http
GET /api/leads/4812/report.pdf
Authorization: Bearer <token>
```

---

## Master data

### GET `/master/{category}`  — e.g. `banks`, `branches`, `propertyTypes`
```http
GET /api/master/banks
Authorization: Bearer <token>
```
```json
200 OK
[ { "id": 1, "value": "HDFC Bank" }, { "id": 2, "value": "ICICI Bank" } ]
```

### POST `/master`  — add (requires `ManageMasters`)
```http
POST /api/master
Authorization: Bearer <token>
Content-Type: application/json

{ "category": "banks", "value": "Axis Bank" }
```
```json
201 Created
{ "id": 3, "category": "banks", "value": "Axis Bank" }
```
Missing field → `400 { "error": "category and value are required", "code": "VALIDATION" }`.

### DELETE `/master/{id}`  — requires `ManageMasters`
```json
200 OK
{ "ok": true }
```

---

## Dashboard / meta / analytics / screens

### GET `/meta`  — sidebar badge counts (scoped to user)
```http
GET /api/meta
Authorization: Bearer <token>
```
```json
200 OK
{ "bucketCounts": { "assigned": 12, "qc": 4, "ro_confirmation": 3, "completed": 27, "rejected": 2 } }
```

### GET `/analytics`  — requires `ViewAnalytics`
```http
GET /api/analytics
Authorization: Bearer <token>
```
```json
200 OK
{
  "kpis": { "totalLeads": 142, "completed": 27, "avgTatDays": 3.4, "slaBreaches": 5 },
  "trend": [ { "date": "2026-06-12", "count": 8 }, { "date": "2026-06-13", "count": 11 } ],
  "byStatus": [ { "status": "completed", "count": 27 }, { "status": "qc", "count": 4 } ],
  "byDistrict": [ { "district": "Bengaluru", "count": 34 } ]
}
```
Forbidden → `403 { "error": "Forbidden", "code": "FORBIDDEN" }`.

### GET `/screens/{name}`  — server-driven screen payload (e.g. `dashboard`, `mis`, `master`, `documents`)
```http
GET /api/screens/dashboard
Authorization: Bearer <token>
```
```json
200 OK
{ "name": "dashboard", "stats": [ ... ], "charts": { ... }, "tables": { ... } }
```
> The exact shape varies per screen; it is consumed verbatim by `ScreenPage.jsx`.

---

## Health  — _anonymous_

### GET `/health`
```http
GET /api/health
```
```json
200 OK
{ "status": "ok", "time": "2026-06-19T10:30:00Z" }
```
