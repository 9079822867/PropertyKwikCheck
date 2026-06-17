/* ============================================================================
   PropertyKwikCheck — SQL Server schema (T-SQL translation of BACKEND_SPEC §6.2)
   Identity model uses the project's own Roles + UserTypes tables.
   Safe to re-run: each table is created only if absent (no destructive drops).
   Run against the PropertyDB database.
   ============================================================================ */

/* ---------- Roles (4 rows, fixed ids) ---------- */
IF OBJECT_ID('dbo.Roles', 'U') IS NULL
CREATE TABLE dbo.Roles (
    id          INT           NOT NULL PRIMARY KEY,
    role_name   NVARCHAR(64)  NOT NULL,
    remark      NVARCHAR(256) NULL
);

/* ---------- UserTypes (20 rows, fixed ids; company_type_id aligns with Roles.id) ---------- */
IF OBJECT_ID('dbo.UserTypes', 'U') IS NULL
CREATE TABLE dbo.UserTypes (
    id              INT          NOT NULL PRIMARY KEY,
    company_type_id INT          NOT NULL,
    name            NVARCHAR(96) NOT NULL,
    CONSTRAINT fk_usertype_companytype FOREIGN KEY (company_type_id) REFERENCES dbo.Roles(id)
);

/* ---------- companies (lenders + the owning agency) ---------- */
IF OBJECT_ID('dbo.companies', 'U') IS NULL
CREATE TABLE dbo.companies (
    id           BIGINT IDENTITY(1,1) PRIMARY KEY,
    name         NVARCHAR(160) NOT NULL,
    type         NVARCHAR(64)  NOT NULL,
    spoc_name    NVARCHAR(120) NULL,
    spoc_user_id BIGINT NULL,
    status       NVARCHAR(16)  NOT NULL CONSTRAINT df_company_status DEFAULT 'Active'
                 CONSTRAINT ck_company_status CHECK (status IN ('Active','Inactive')),
    created_at   DATETIME2 NOT NULL CONSTRAINT df_company_created DEFAULT SYSUTCDATETIME(),
    updated_at   DATETIME2 NOT NULL CONSTRAINT df_company_updated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT uq_company_name UNIQUE (name)
);

/* ---------- users ---------- */
IF OBJECT_ID('dbo.users', 'U') IS NULL
CREATE TABLE dbo.users (
    id            BIGINT IDENTITY(1,1) PRIMARY KEY,
    name          NVARCHAR(120) NOT NULL,
    email         NVARCHAR(160) NOT NULL,
    password_hash NVARCHAR(255) NOT NULL,
    role_id       INT NOT NULL,
    user_type_id  INT NOT NULL,
    company_id    BIGINT NULL,
    phone         NVARCHAR(32)  NULL,
    licence_no    NVARCHAR(48)  NULL,
    status        NVARCHAR(16)  NOT NULL CONSTRAINT df_user_status DEFAULT 'Active'
                  CONSTRAINT ck_user_status CHECK (status IN ('Active','Inactive')),
    last_login_at DATETIME2 NULL,
    created_at    DATETIME2 NOT NULL CONSTRAINT df_user_created DEFAULT SYSUTCDATETIME(),
    updated_at    DATETIME2 NOT NULL CONSTRAINT df_user_updated DEFAULT SYSUTCDATETIME(),
    deleted_at    DATETIME2 NULL,
    CONSTRAINT uq_user_email UNIQUE (email),
    CONSTRAINT fk_user_role     FOREIGN KEY (role_id)      REFERENCES dbo.Roles(id),
    CONSTRAINT fk_user_usertype FOREIGN KEY (user_type_id) REFERENCES dbo.UserTypes(id),
    CONSTRAINT fk_user_company  FOREIGN KEY (company_id)   REFERENCES dbo.companies(id)
);

/* ---------- leads (central entity) ---------- */
IF OBJECT_ID('dbo.leads', 'U') IS NULL
CREATE TABLE dbo.leads (
    id              BIGINT IDENTITY(1,1) PRIMARY KEY,
    req_id          NVARCHAR(40) NOT NULL,
    asset_family    NVARCHAR(16) NOT NULL CONSTRAINT ck_lead_family CHECK (asset_family IN ('property','plot','agri')),
    property_type   NVARCHAR(48) NOT NULL,
    stage           NVARCHAR(24) NOT NULL CONSTRAINT df_lead_stage DEFAULT 'fresh'
                    CONSTRAINT ck_lead_stage CHECK (stage IN
                       ('fresh','ro','assigned','reassigned','ro_confirmation','qc','qc_hold',
                        'pricing','completed','out_of_tat','duplicate','rejected')),
    report_status   NVARCHAR(40) NOT NULL CONSTRAINT df_lead_rstatus DEFAULT 'Open',

    applicant       NVARCHAR(160) NULL,
    co_applicant    NVARCHAR(160) NULL,
    contact         NVARCHAR(40)  NULL,
    pin             NVARCHAR(12)  NULL,
    location        NVARCHAR(200) NULL,

    lender_company_id BIGINT NULL,
    lender_name       NVARCHAR(160) NULL,
    branch            NVARCHAR(160) NULL,

    valuator_user_id  BIGINT NULL,
    valuator_name     NVARCHAR(120) NULL,
    ro_company        NVARCHAR(120) NULL,

    exec_name       NVARCHAR(120) NULL,
    exec_phone      NVARCHAR(40)  NULL,
    exec_email      NVARCHAR(160) NULL,

    loan_no         NVARCHAR(48) NULL,
    claim_no        NVARCHAR(48) NULL,
    source          NVARCHAR(48) NULL,
    reg_no          NVARCHAR(48) NULL,

    lead_date       DATE NULL,
    assigned_on     DATE NULL,
    inspection_date DATE NULL,
    issued_date     DATE NULL,
    tat_due         DATE NULL,
    tat_pct         SMALLINT NOT NULL CONSTRAINT df_lead_tatpct DEFAULT 0,
    tat_state       NVARCHAR(8) NOT NULL CONSTRAINT df_lead_tatstate DEFAULT 'ok'
                    CONSTRAINT ck_lead_tatstate CHECK (tat_state IN ('ok','warn','over')),

    value           BIGINT NULL,
    remarks         NVARCHAR(400) NULL,
    hold_remarks    NVARCHAR(400) NULL,

    report_data     NVARCHAR(MAX) NULL,

    created_by      BIGINT NULL,
    created_at      DATETIME2 NOT NULL CONSTRAINT df_lead_created DEFAULT SYSUTCDATETIME(),
    updated_at      DATETIME2 NOT NULL CONSTRAINT df_lead_updated DEFAULT SYSUTCDATETIME(),
    deleted_at      DATETIME2 NULL,

    CONSTRAINT uq_lead_reqid UNIQUE (req_id),
    CONSTRAINT fk_lead_lender   FOREIGN KEY (lender_company_id) REFERENCES dbo.companies(id),
    CONSTRAINT fk_lead_valuator FOREIGN KEY (valuator_user_id)  REFERENCES dbo.users(id)
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_lead_stage')
    CREATE INDEX idx_lead_stage    ON dbo.leads(stage);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_lead_lender')
    CREATE INDEX idx_lead_lender   ON dbo.leads(lender_company_id);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_lead_valuator')
    CREATE INDEX idx_lead_valuator ON dbo.leads(valuator_user_id);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_lead_created')
    CREATE INDEX idx_lead_created  ON dbo.leads(created_at);

/* ---------- lead stage history ---------- */
IF OBJECT_ID('dbo.lead_stage_history', 'U') IS NULL
CREATE TABLE dbo.lead_stage_history (
    id            BIGINT IDENTITY(1,1) PRIMARY KEY,
    lead_id       BIGINT NOT NULL,
    from_stage    NVARCHAR(24) NULL,
    to_stage      NVARCHAR(24) NOT NULL,
    actor_user_id BIGINT NULL,
    note          NVARCHAR(400) NULL,
    created_at    DATETIME2 NOT NULL CONSTRAINT df_hist_created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT fk_hist_lead FOREIGN KEY (lead_id) REFERENCES dbo.leads(id) ON DELETE CASCADE
);

/* ---------- documents ---------- */
IF OBJECT_ID('dbo.documents', 'U') IS NULL
CREATE TABLE dbo.documents (
    id          BIGINT IDENTITY(1,1) PRIMARY KEY,
    lead_id     BIGINT NOT NULL,
    doc_type    NVARCHAR(48)  NOT NULL,
    file_name   NVARCHAR(255) NOT NULL,
    storage_key NVARCHAR(512) NOT NULL,
    mime        NVARCHAR(96)  NULL,
    size_bytes  BIGINT NULL,
    uploaded_by BIGINT NULL,
    uploaded_at DATETIME2 NOT NULL CONSTRAINT df_doc_uploaded DEFAULT SYSUTCDATETIME(),
    CONSTRAINT fk_doc_lead FOREIGN KEY (lead_id) REFERENCES dbo.leads(id) ON DELETE CASCADE
);

/* ---------- photos & videos ---------- */
IF OBJECT_ID('dbo.photos', 'U') IS NULL
CREATE TABLE dbo.photos (
    id          BIGINT IDENTITY(1,1) PRIMARY KEY,
    lead_id     BIGINT NOT NULL,
    kind        NVARCHAR(8) NOT NULL CONSTRAINT df_photo_kind DEFAULT 'photo'
                CONSTRAINT ck_photo_kind CHECK (kind IN ('photo','video')),
    frame_label NVARCHAR(80) NULL,
    storage_key NVARCHAR(512) NOT NULL,
    mime        NVARCHAR(96) NULL,
    size_bytes  BIGINT NULL,
    lat         DECIMAL(10,7) NULL,
    lng         DECIMAL(10,7) NULL,
    captured_at DATETIME2 NULL,
    uploaded_by BIGINT NULL,
    uploaded_at DATETIME2 NOT NULL CONSTRAINT df_photo_uploaded DEFAULT SYSUTCDATETIME(),
    CONSTRAINT fk_photo_lead FOREIGN KEY (lead_id) REFERENCES dbo.leads(id) ON DELETE CASCADE
);

/* ---------- master lookups ---------- */
IF OBJECT_ID('dbo.master_lookups', 'U') IS NULL
CREATE TABLE dbo.master_lookups (
    id       BIGINT IDENTITY(1,1) PRIMARY KEY,
    category NVARCHAR(48)  NOT NULL,
    value    NVARCHAR(160) NOT NULL,
    meta     NVARCHAR(MAX) NULL,
    sort     INT NOT NULL CONSTRAINT df_lookup_sort DEFAULT 0,
    active   BIT NOT NULL CONSTRAINT df_lookup_active DEFAULT 1,
    CONSTRAINT uq_lookup UNIQUE (category, value)
);

/* ---------- DLC / circle-rate master ---------- */
IF OBJECT_ID('dbo.dlc_rates', 'U') IS NULL
CREATE TABLE dbo.dlc_rates (
    id         BIGINT IDENTITY(1,1) PRIMARY KEY,
    locality   NVARCHAR(160) NOT NULL,
    unit       NVARCHAR(24)  NOT NULL,
    rate       BIGINT NOT NULL,
    basis      NVARCHAR(48) NULL,
    active     BIT NOT NULL CONSTRAINT df_dlc_active DEFAULT 1,
    updated_at DATETIME2 NOT NULL CONSTRAINT df_dlc_updated DEFAULT SYSUTCDATETIME()
);

/* ---------- invoices ---------- */
IF OBJECT_ID('dbo.invoices', 'U') IS NULL
CREATE TABLE dbo.invoices (
    id          BIGINT IDENTITY(1,1) PRIMARY KEY,
    invoice_no  NVARCHAR(32) NOT NULL,
    company_id  BIGINT NOT NULL,
    period      NVARCHAR(24) NOT NULL,
    lead_count  INT NOT NULL CONSTRAINT df_invoice_count DEFAULT 0,
    amount      BIGINT NOT NULL CONSTRAINT df_invoice_amount DEFAULT 0,
    status      NVARCHAR(12) NOT NULL CONSTRAINT df_invoice_status DEFAULT 'Pending'
                CONSTRAINT ck_invoice_status CHECK (status IN ('Paid','Pending','Overdue')),
    created_at  DATETIME2 NOT NULL CONSTRAINT df_invoice_created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT uq_invoice_no UNIQUE (invoice_no),
    CONSTRAINT fk_invoice_company FOREIGN KEY (company_id) REFERENCES dbo.companies(id)
);

/* ---------- site visits (yard schedule) ---------- */
IF OBJECT_ID('dbo.site_visits', 'U') IS NULL
CREATE TABLE dbo.site_visits (
    id             BIGINT IDENTITY(1,1) PRIMARY KEY,
    lead_id        BIGINT NOT NULL,
    valuer_user_id BIGINT NULL,
    scheduled_at   DATETIME2 NOT NULL,
    location       NVARCHAR(200) NULL,
    status         NVARCHAR(16) NOT NULL CONSTRAINT df_visit_status DEFAULT 'Scheduled'
                   CONSTRAINT ck_visit_status CHECK (status IN ('Scheduled','En route','Checked-in','Completed','Cancelled')),
    created_at     DATETIME2 NOT NULL CONSTRAINT df_visit_created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT fk_visit_lead FOREIGN KEY (lead_id) REFERENCES dbo.leads(id) ON DELETE CASCADE
);

/* ---------- audit log (append-only) ---------- */
IF OBJECT_ID('dbo.audit_log', 'U') IS NULL
CREATE TABLE dbo.audit_log (
    id            BIGINT IDENTITY(1,1) PRIMARY KEY,
    actor_user_id BIGINT NULL,
    action        NVARCHAR(48) NOT NULL,
    entity_type   NVARCHAR(32) NOT NULL,
    entity_id     NVARCHAR(40) NULL,
    before_json   NVARCHAR(MAX) NULL,
    after_json    NVARCHAR(MAX) NULL,
    ip            NVARCHAR(64) NULL,
    user_agent    NVARCHAR(255) NULL,
    created_at    DATETIME2 NOT NULL CONSTRAINT df_audit_created DEFAULT SYSUTCDATETIME()
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_audit_entity')
    CREATE INDEX idx_audit_entity ON dbo.audit_log(entity_type, entity_id);

/* ---------- refresh tokens ---------- */
IF OBJECT_ID('dbo.refresh_tokens', 'U') IS NULL
CREATE TABLE dbo.refresh_tokens (
    id         BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id    BIGINT NOT NULL,
    token_hash NVARCHAR(255) NOT NULL,
    expires_at DATETIME2 NOT NULL,
    revoked_at DATETIME2 NULL,
    created_at DATETIME2 NOT NULL CONSTRAINT df_rt_created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT fk_rt_user FOREIGN KEY (user_id) REFERENCES dbo.users(id) ON DELETE CASCADE
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_rt_hash')
    CREATE INDEX idx_rt_hash ON dbo.refresh_tokens(token_hash);
GO
