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

/* ---------- lead report data (column-per-field, 1:1 to a lead) ----------
   The report payload is stored COLUMN-WISE (no JSON blob): each column name matches a
   report field key (web/src/lib/wizardSchema.js / PropertyKwikCheck.Core.Mapping.ReportFields).
   Reads re-assemble the JSON via FOR JSON PATH. */

/* Fold any earlier JSON-shaped leadreportdata back into leads.report_data, then drop it so
   the column-wise table can be (re)created and re-populated below. */
IF OBJECT_ID('dbo.leadreportdata','U') IS NOT NULL AND COL_LENGTH('dbo.leadreportdata','report_data') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.leads','report_data') IS NULL
        ALTER TABLE dbo.leads ADD report_data NVARCHAR(MAX) NULL;
    EXEC sp_executesql N'UPDATE l SET report_data = r.report_data FROM dbo.leads l JOIN dbo.leadreportdata r ON r.lead_id = l.id WHERE r.report_data IS NOT NULL;';
    DROP TABLE dbo.leadreportdata;
END

IF OBJECT_ID('dbo.leadreportdata', 'U') IS NULL
CREATE TABLE dbo.leadreportdata (
    id         BIGINT IDENTITY(1,1) PRIMARY KEY,
    lead_id    BIGINT NOT NULL,
    [addrActual] NVARCHAR(MAX) NULL, [addrDoc] NVARCHAR(MAX) NULL, [adjustment] NVARCHAR(MAX) NULL, [adoptedRate] NVARCHAR(MAX) NULL,
    [adoptedValue] NVARCHAR(MAX) NULL, [agriMarketability] NVARCHAR(MAX) NULL, [agriMarketability_r] NVARCHAR(MAX) NULL, [applicant] NVARCHAR(MAX) NULL,
    [approachAccess] NVARCHAR(MAX) NULL, [areaForVal] NVARCHAR(MAX) NULL, [assignedRO] NVARCHAR(MAX) NULL, [authorisedBy] NVARCHAR(MAX) NULL,
    [authorisedDate] NVARCHAR(MAX) NULL, [avgRate] NVARCHAR(MAX) NULL, [balcony] NVARCHAR(MAX) NULL, [bathFloor] NVARCHAR(MAX) NULL,
    [bathrooms] NVARCHAR(MAX) NULL, [bedFloor] NVARCHAR(MAX) NULL, [bedrooms] NVARCHAR(MAX) NULL, [borewellPower] NVARCHAR(MAX) NULL,
    [branch] NVARCHAR(MAX) NULL, [builtup] NVARCHAR(MAX) NULL, [canalSupport] NVARCHAR(MAX) NULL, [carpet] NVARCHAR(MAX) NULL,
    [ceiling] NVARCHAR(MAX) NULL, [civicServices] NVARCHAR(MAX) NULL, [civicServices_r] NVARCHAR(MAX) NULL, [claimNo] NVARCHAR(MAX) NULL,
    [cmp1Rate] NVARCHAR(MAX) NULL, [cmp2Rate] NVARCHAR(MAX) NULL, [cmp3Rate] NVARCHAR(MAX) NULL, [coApplicant] NVARCHAR(MAX) NULL,
    [conditionKey] NVARCHAR(MAX) NULL, [config] NVARCHAR(MAX) NULL, [connectivity] NVARCHAR(MAX) NULL, [contact] NVARCHAR(MAX) NULL,
    [cornerAdv] NVARCHAR(MAX) NULL, [croppingPattern] NVARCHAR(MAX) NULL, [croppingPattern_r] NVARCHAR(MAX) NULL, [currentStatus] NVARCHAR(MAX) NULL,
    [demand] NVARCHAR(MAX) NULL, [devActivity] NVARCHAR(MAX) NULL, [dimE] NVARCHAR(MAX) NULL, [dimN] NVARCHAR(MAX) NULL,
    [dimS] NVARCHAR(MAX) NULL, [dimTotal] NVARCHAR(MAX) NULL, [dimW] NVARCHAR(MAX) NULL, [disclaimer] NVARCHAR(MAX) NULL,
    [disputeRelated] NVARCHAR(MAX) NULL, [distBranch] NVARCHAR(MAX) NULL, [distHighway] NVARCHAR(MAX) NULL, [distHospital] NVARCHAR(MAX) NULL,
    [distMainRoad] NVARCHAR(MAX) NULL, [distMandi] NVARCHAR(MAX) NULL, [distMarket] NVARCHAR(MAX) NULL, [distMetalledRoad] NVARCHAR(MAX) NULL,
    [distSchool] NVARCHAR(MAX) NULL, [distVillageAbadi] NVARCHAR(MAX) NULL, [distressObs] NVARCHAR(MAX) NULL, [distressPct] NVARCHAR(MAX) NULL,
    [distressValue] NVARCHAR(MAX) NULL, [districtState] NVARCHAR(MAX) NULL, [dlcAdjust] NVARCHAR(MAX) NULL, [dlcArea] NVARCHAR(MAX) NULL,
    [dlcBasis] NVARCHAR(MAX) NULL, [dlcRate] NVARCHAR(MAX) NULL, [dlcUsedFor] NVARCHAR(MAX) NULL, [dlcValue] NVARCHAR(MAX) NULL,
    [docType] NVARCHAR(MAX) NULL, [docsRelied] NVARCHAR(MAX) NULL, [docsShown] NVARCHAR(MAX) NULL, [electrical] NVARCHAR(MAX) NULL,
    [encumbrance] NVARCHAR(MAX) NULL, [execEmail] NVARCHAR(MAX) NULL, [execName] NVARCHAR(MAX) NULL, [execPhone] NVARCHAR(MAX) NULL,
    [facade] NVARCHAR(MAX) NULL, [facing] NVARCHAR(MAX) NULL, [fairMarketValue] NVARCHAR(MAX) NULL, [floorNo] NVARCHAR(MAX) NULL,
    [foundation] NVARCHAR(MAX) NULL, [frontageDepth] NVARCHAR(MAX) NULL, [gps] NVARCHAR(MAX) NULL, [htLine] NVARCHAR(MAX) NULL,
    [incomeDependence] NVARCHAR(MAX) NULL, [inspectedBy] NVARCHAR(MAX) NULL, [inspectedDate] NVARCHAR(MAX) NULL, [inspectedLicence] NVARCHAR(MAX) NULL,
    [inspectionDate] NVARCHAR(MAX) NULL, [intWalls] NVARCHAR(MAX) NULL, [interiorLocation] NVARCHAR(MAX) NULL, [irrigation] NVARCHAR(MAX) NULL,
    [irrigation_r] NVARCHAR(MAX) NULL, [issuedDate] NVARCHAR(MAX) NULL, [jamabandiMap] NVARCHAR(MAX) NULL, [jamabandiYear] NVARCHAR(MAX) NULL,
    [khasraGirdawari] NVARCHAR(MAX) NULL, [khasraNumber] NVARCHAR(MAX) NULL, [khataNumber] NVARCHAR(MAX) NULL, [khatedarName] NVARCHAR(MAX) NULL,
    [kitchen] NVARCHAR(MAX) NULL, [kitchenFloor] NVARCHAR(MAX) NULL, [landArea] NVARCHAR(MAX) NULL, [landCeiling] NVARCHAR(MAX) NULL,
    [landUse] NVARCHAR(MAX) NULL, [landUseRestriction] NVARCHAR(MAX) NULL, [landUse_r] NVARCHAR(MAX) NULL, [landmark] NVARCHAR(MAX) NULL,
    [layoutPlan] NVARCHAR(MAX) NULL, [leadDate] NVARCHAR(MAX) NULL, [leadId] NVARCHAR(MAX) NULL, [lender] NVARCHAR(MAX) NULL,
    [liquidityObs] NVARCHAR(MAX) NULL, [livingFloor] NVARCHAR(MAX) NULL, [livingHall] NVARCHAR(MAX) NULL, [loanNo] NVARCHAR(MAX) NULL,
    [loanType] NVARCHAR(MAX) NULL, [lobby] NVARCHAR(MAX) NULL, [localityClass] NVARCHAR(MAX) NULL, [localityEnquiry] NVARCHAR(MAX) NULL,
    [localityStatus] NVARCHAR(MAX) NULL, [localityStatus_r] NVARCHAR(MAX) NULL, [locationRemark] NVARCHAR(MAX) NULL, [marketability] NVARCHAR(MAX) NULL,
    [marketability_r] NVARCHAR(MAX) NULL, [masterBed] NVARCHAR(MAX) NULL, [mutationEntry] NVARCHAR(MAX) NULL, [mutationEntryT] NVARCHAR(MAX) NULL,
    [mutationStatus] NVARCHAR(MAX) NULL, [nearestTown] NVARCHAR(MAX) NULL, [overallRisk] NVARCHAR(MAX) NULL, [ownerName] NVARCHAR(MAX) NULL,
    [ownership] NVARCHAR(MAX) NULL, [ownershipType] NVARCHAR(MAX) NULL, [paperCheck] NVARCHAR(MAX) NULL, [personMet] NVARCHAR(MAX) NULL,
    [plotArea] NVARCHAR(MAX) NULL, [plotNumber] NVARCHAR(MAX) NULL, [powerConnection] NVARCHAR(MAX) NULL, [powerConnection_r] NVARCHAR(MAX) NULL,
    [propertyType] NVARCHAR(MAX) NULL, [rccFrame] NVARCHAR(MAX) NULL, [realizablePct] NVARCHAR(MAX) NULL, [realizableValue] NVARCHAR(MAX) NULL,
    [redevelopment] NVARCHAR(MAX) NULL, [regNumber] NVARCHAR(MAX) NULL, [regOffice] NVARCHAR(MAX) NULL, [relationship] NVARCHAR(MAX) NULL,
    [remarks] NVARCHAR(MAX) NULL, [reportStatus] NVARCHAR(MAX) NULL, [reportType] NVARCHAR(MAX) NULL, [reqId] NVARCHAR(MAX) NULL,
    [restrictedBuyer] NVARCHAR(MAX) NULL, [revenueOffice] NVARCHAR(MAX) NULL, [reviewedBy] NVARCHAR(MAX) NULL, [reviewedDate] NVARCHAR(MAX) NULL,
    [reviewedLicence] NVARCHAR(MAX) NULL, [roCompany] NVARCHAR(MAX) NULL, [roadType] NVARCHAR(MAX) NULL, [roadWidth] NVARCHAR(MAX) NULL,
    [saleDeedCopy] NVARCHAR(MAX) NULL, [saleDeedDate] NVARCHAR(MAX) NULL, [scAccessibility] NVARCHAR(MAX) NULL, [scDevelopment] NVARCHAR(MAX) NULL,
    [scExterior] NVARCHAR(MAX) NULL, [scFlooring] NVARCHAR(MAX) NULL, [scInterior] NVARCHAR(MAX) NULL, [scIrrigation] NVARCHAR(MAX) NULL,
    [scLiquidity] NVARCHAR(MAX) NULL, [scMarketability] NVARCHAR(MAX) NULL, [scSoil] NVARCHAR(MAX) NULL, [scStructure] NVARCHAR(MAX) NULL,
    [scopeLimit] NVARCHAR(MAX) NULL, [soilFertility] NVARCHAR(MAX) NULL, [soilType] NVARCHAR(MAX) NULL, [soilType_r] NVARCHAR(MAX) NULL,
    [source] NVARCHAR(MAX) NULL, [structuralCondition] NVARCHAR(MAX) NULL, [superBuiltup] NVARCHAR(MAX) NULL, [surrDevelopment] NVARCHAR(MAX) NULL,
    [surrDevelopment_r] NVARCHAR(MAX) NULL, [surveyKhasra] NVARCHAR(MAX) NULL, [tatDue] NVARCHAR(MAX) NULL, [taxReceipt] NVARCHAR(MAX) NULL,
    [tehsil] NVARCHAR(MAX) NULL, [tenureType] NVARCHAR(MAX) NULL, [terrace] NVARCHAR(MAX) NULL, [topoDrainage] NVARCHAR(MAX) NULL,
    [topography] NVARCHAR(MAX) NULL, [totalFloors] NVARCHAR(MAX) NULL, [vBoundary] NVARCHAR(MAX) NULL, [vBunds] NVARCHAR(MAX) NULL,
    [vCultivation] NVARCHAR(MAX) NULL, [vEncroach] NVARCHAR(MAX) NULL, [vIdentified] NVARCHAR(MAX) NULL, [vLocated] NVARCHAR(MAX) NULL,
    [vPossession] NVARCHAR(MAX) NULL, [vVacant] NVARCHAR(MAX) NULL, [valStatement] NVARCHAR(MAX) NULL, [valuationPurpose] NVARCHAR(MAX) NULL,
    [village] NVARCHAR(MAX) NULL, [villageColony] NVARCHAR(MAX) NULL, [yearBuilt] NVARCHAR(MAX) NULL,
    updated_at DATETIME2 NOT NULL CONSTRAINT df_lrd_updated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT uq_lrd_lead UNIQUE (lead_id),
    CONSTRAINT fk_lrd_lead FOREIGN KEY (lead_id) REFERENCES dbo.leads(id) ON DELETE CASCADE
);

/* Explode any legacy leads.report_data JSON into the column-wise table, then drop it.
   Dynamic SQL: the column is absent on fresh installs (would fail to compile inline). */
IF COL_LENGTH('dbo.leads', 'report_data') IS NOT NULL
EXEC sp_executesql N'
    INSERT INTO dbo.leadreportdata (lead_id,
            [addrActual], [addrDoc], [adjustment], [adoptedRate], [adoptedValue], [agriMarketability], [agriMarketability_r], [applicant],
            [approachAccess], [areaForVal], [assignedRO], [authorisedBy], [authorisedDate], [avgRate], [balcony], [bathFloor],
            [bathrooms], [bedFloor], [bedrooms], [borewellPower], [branch], [builtup], [canalSupport], [carpet],
            [ceiling], [civicServices], [civicServices_r], [claimNo], [cmp1Rate], [cmp2Rate], [cmp3Rate], [coApplicant],
            [conditionKey], [config], [connectivity], [contact], [cornerAdv], [croppingPattern], [croppingPattern_r], [currentStatus],
            [demand], [devActivity], [dimE], [dimN], [dimS], [dimTotal], [dimW], [disclaimer],
            [disputeRelated], [distBranch], [distHighway], [distHospital], [distMainRoad], [distMandi], [distMarket], [distMetalledRoad],
            [distSchool], [distVillageAbadi], [distressObs], [distressPct], [distressValue], [districtState], [dlcAdjust], [dlcArea],
            [dlcBasis], [dlcRate], [dlcUsedFor], [dlcValue], [docType], [docsRelied], [docsShown], [electrical],
            [encumbrance], [execEmail], [execName], [execPhone], [facade], [facing], [fairMarketValue], [floorNo],
            [foundation], [frontageDepth], [gps], [htLine], [incomeDependence], [inspectedBy], [inspectedDate], [inspectedLicence],
            [inspectionDate], [intWalls], [interiorLocation], [irrigation], [irrigation_r], [issuedDate], [jamabandiMap], [jamabandiYear],
            [khasraGirdawari], [khasraNumber], [khataNumber], [khatedarName], [kitchen], [kitchenFloor], [landArea], [landCeiling],
            [landUse], [landUseRestriction], [landUse_r], [landmark], [layoutPlan], [leadDate], [leadId], [lender],
            [liquidityObs], [livingFloor], [livingHall], [loanNo], [loanType], [lobby], [localityClass], [localityEnquiry],
            [localityStatus], [localityStatus_r], [locationRemark], [marketability], [marketability_r], [masterBed], [mutationEntry], [mutationEntryT],
            [mutationStatus], [nearestTown], [overallRisk], [ownerName], [ownership], [ownershipType], [paperCheck], [personMet],
            [plotArea], [plotNumber], [powerConnection], [powerConnection_r], [propertyType], [rccFrame], [realizablePct], [realizableValue],
            [redevelopment], [regNumber], [regOffice], [relationship], [remarks], [reportStatus], [reportType], [reqId],
            [restrictedBuyer], [revenueOffice], [reviewedBy], [reviewedDate], [reviewedLicence], [roCompany], [roadType], [roadWidth],
            [saleDeedCopy], [saleDeedDate], [scAccessibility], [scDevelopment], [scExterior], [scFlooring], [scInterior], [scIrrigation],
            [scLiquidity], [scMarketability], [scSoil], [scStructure], [scopeLimit], [soilFertility], [soilType], [soilType_r],
            [source], [structuralCondition], [superBuiltup], [surrDevelopment], [surrDevelopment_r], [surveyKhasra], [tatDue], [taxReceipt],
            [tehsil], [tenureType], [terrace], [topoDrainage], [topography], [totalFloors], [vBoundary], [vBunds],
            [vCultivation], [vEncroach], [vIdentified], [vLocated], [vPossession], [vVacant], [valStatement], [valuationPurpose],
            [village], [villageColony], [yearBuilt])
    SELECT l.id,
            JSON_VALUE(l.report_data, ''$.addrActual''), JSON_VALUE(l.report_data, ''$.addrDoc''), JSON_VALUE(l.report_data, ''$.adjustment''),
            JSON_VALUE(l.report_data, ''$.adoptedRate''), JSON_VALUE(l.report_data, ''$.adoptedValue''), JSON_VALUE(l.report_data, ''$.agriMarketability''),
            JSON_VALUE(l.report_data, ''$.agriMarketability_r''), JSON_VALUE(l.report_data, ''$.applicant''), JSON_VALUE(l.report_data, ''$.approachAccess''),
            JSON_VALUE(l.report_data, ''$.areaForVal''), JSON_VALUE(l.report_data, ''$.assignedRO''), JSON_VALUE(l.report_data, ''$.authorisedBy''),
            JSON_VALUE(l.report_data, ''$.authorisedDate''), JSON_VALUE(l.report_data, ''$.avgRate''), JSON_VALUE(l.report_data, ''$.balcony''),
            JSON_VALUE(l.report_data, ''$.bathFloor''), JSON_VALUE(l.report_data, ''$.bathrooms''), JSON_VALUE(l.report_data, ''$.bedFloor''),
            JSON_VALUE(l.report_data, ''$.bedrooms''), JSON_VALUE(l.report_data, ''$.borewellPower''), JSON_VALUE(l.report_data, ''$.branch''),
            JSON_VALUE(l.report_data, ''$.builtup''), JSON_VALUE(l.report_data, ''$.canalSupport''), JSON_VALUE(l.report_data, ''$.carpet''),
            JSON_VALUE(l.report_data, ''$.ceiling''), JSON_VALUE(l.report_data, ''$.civicServices''), JSON_VALUE(l.report_data, ''$.civicServices_r''),
            JSON_VALUE(l.report_data, ''$.claimNo''), JSON_VALUE(l.report_data, ''$.cmp1Rate''), JSON_VALUE(l.report_data, ''$.cmp2Rate''),
            JSON_VALUE(l.report_data, ''$.cmp3Rate''), JSON_VALUE(l.report_data, ''$.coApplicant''), JSON_VALUE(l.report_data, ''$.conditionKey''),
            JSON_VALUE(l.report_data, ''$.config''), JSON_VALUE(l.report_data, ''$.connectivity''), JSON_VALUE(l.report_data, ''$.contact''),
            JSON_VALUE(l.report_data, ''$.cornerAdv''), JSON_VALUE(l.report_data, ''$.croppingPattern''), JSON_VALUE(l.report_data, ''$.croppingPattern_r''),
            JSON_VALUE(l.report_data, ''$.currentStatus''), JSON_VALUE(l.report_data, ''$.demand''), JSON_VALUE(l.report_data, ''$.devActivity''),
            JSON_VALUE(l.report_data, ''$.dimE''), JSON_VALUE(l.report_data, ''$.dimN''), JSON_VALUE(l.report_data, ''$.dimS''),
            JSON_VALUE(l.report_data, ''$.dimTotal''), JSON_VALUE(l.report_data, ''$.dimW''), JSON_VALUE(l.report_data, ''$.disclaimer''),
            JSON_VALUE(l.report_data, ''$.disputeRelated''), JSON_VALUE(l.report_data, ''$.distBranch''), JSON_VALUE(l.report_data, ''$.distHighway''),
            JSON_VALUE(l.report_data, ''$.distHospital''), JSON_VALUE(l.report_data, ''$.distMainRoad''), JSON_VALUE(l.report_data, ''$.distMandi''),
            JSON_VALUE(l.report_data, ''$.distMarket''), JSON_VALUE(l.report_data, ''$.distMetalledRoad''), JSON_VALUE(l.report_data, ''$.distSchool''),
            JSON_VALUE(l.report_data, ''$.distVillageAbadi''), JSON_VALUE(l.report_data, ''$.distressObs''), JSON_VALUE(l.report_data, ''$.distressPct''),
            JSON_VALUE(l.report_data, ''$.distressValue''), JSON_VALUE(l.report_data, ''$.districtState''), JSON_VALUE(l.report_data, ''$.dlcAdjust''),
            JSON_VALUE(l.report_data, ''$.dlcArea''), JSON_VALUE(l.report_data, ''$.dlcBasis''), JSON_VALUE(l.report_data, ''$.dlcRate''),
            JSON_VALUE(l.report_data, ''$.dlcUsedFor''), JSON_VALUE(l.report_data, ''$.dlcValue''), JSON_VALUE(l.report_data, ''$.docType''),
            JSON_VALUE(l.report_data, ''$.docsRelied''), JSON_VALUE(l.report_data, ''$.docsShown''), JSON_VALUE(l.report_data, ''$.electrical''),
            JSON_VALUE(l.report_data, ''$.encumbrance''), JSON_VALUE(l.report_data, ''$.execEmail''), JSON_VALUE(l.report_data, ''$.execName''),
            JSON_VALUE(l.report_data, ''$.execPhone''), JSON_VALUE(l.report_data, ''$.facade''), JSON_VALUE(l.report_data, ''$.facing''),
            JSON_VALUE(l.report_data, ''$.fairMarketValue''), JSON_VALUE(l.report_data, ''$.floorNo''), JSON_VALUE(l.report_data, ''$.foundation''),
            JSON_VALUE(l.report_data, ''$.frontageDepth''), JSON_VALUE(l.report_data, ''$.gps''), JSON_VALUE(l.report_data, ''$.htLine''),
            JSON_VALUE(l.report_data, ''$.incomeDependence''), JSON_VALUE(l.report_data, ''$.inspectedBy''), JSON_VALUE(l.report_data, ''$.inspectedDate''),
            JSON_VALUE(l.report_data, ''$.inspectedLicence''), JSON_VALUE(l.report_data, ''$.inspectionDate''), JSON_VALUE(l.report_data, ''$.intWalls''),
            JSON_VALUE(l.report_data, ''$.interiorLocation''), JSON_VALUE(l.report_data, ''$.irrigation''), JSON_VALUE(l.report_data, ''$.irrigation_r''),
            JSON_VALUE(l.report_data, ''$.issuedDate''), JSON_VALUE(l.report_data, ''$.jamabandiMap''), JSON_VALUE(l.report_data, ''$.jamabandiYear''),
            JSON_VALUE(l.report_data, ''$.khasraGirdawari''), JSON_VALUE(l.report_data, ''$.khasraNumber''), JSON_VALUE(l.report_data, ''$.khataNumber''),
            JSON_VALUE(l.report_data, ''$.khatedarName''), JSON_VALUE(l.report_data, ''$.kitchen''), JSON_VALUE(l.report_data, ''$.kitchenFloor''),
            JSON_VALUE(l.report_data, ''$.landArea''), JSON_VALUE(l.report_data, ''$.landCeiling''), JSON_VALUE(l.report_data, ''$.landUse''),
            JSON_VALUE(l.report_data, ''$.landUseRestriction''), JSON_VALUE(l.report_data, ''$.landUse_r''), JSON_VALUE(l.report_data, ''$.landmark''),
            JSON_VALUE(l.report_data, ''$.layoutPlan''), JSON_VALUE(l.report_data, ''$.leadDate''), JSON_VALUE(l.report_data, ''$.leadId''),
            JSON_VALUE(l.report_data, ''$.lender''), JSON_VALUE(l.report_data, ''$.liquidityObs''), JSON_VALUE(l.report_data, ''$.livingFloor''),
            JSON_VALUE(l.report_data, ''$.livingHall''), JSON_VALUE(l.report_data, ''$.loanNo''), JSON_VALUE(l.report_data, ''$.loanType''),
            JSON_VALUE(l.report_data, ''$.lobby''), JSON_VALUE(l.report_data, ''$.localityClass''), JSON_VALUE(l.report_data, ''$.localityEnquiry''),
            JSON_VALUE(l.report_data, ''$.localityStatus''), JSON_VALUE(l.report_data, ''$.localityStatus_r''), JSON_VALUE(l.report_data, ''$.locationRemark''),
            JSON_VALUE(l.report_data, ''$.marketability''), JSON_VALUE(l.report_data, ''$.marketability_r''), JSON_VALUE(l.report_data, ''$.masterBed''),
            JSON_VALUE(l.report_data, ''$.mutationEntry''), JSON_VALUE(l.report_data, ''$.mutationEntryT''), JSON_VALUE(l.report_data, ''$.mutationStatus''),
            JSON_VALUE(l.report_data, ''$.nearestTown''), JSON_VALUE(l.report_data, ''$.overallRisk''), JSON_VALUE(l.report_data, ''$.ownerName''),
            JSON_VALUE(l.report_data, ''$.ownership''), JSON_VALUE(l.report_data, ''$.ownershipType''), JSON_VALUE(l.report_data, ''$.paperCheck''),
            JSON_VALUE(l.report_data, ''$.personMet''), JSON_VALUE(l.report_data, ''$.plotArea''), JSON_VALUE(l.report_data, ''$.plotNumber''),
            JSON_VALUE(l.report_data, ''$.powerConnection''), JSON_VALUE(l.report_data, ''$.powerConnection_r''), JSON_VALUE(l.report_data, ''$.propertyType''),
            JSON_VALUE(l.report_data, ''$.rccFrame''), JSON_VALUE(l.report_data, ''$.realizablePct''), JSON_VALUE(l.report_data, ''$.realizableValue''),
            JSON_VALUE(l.report_data, ''$.redevelopment''), JSON_VALUE(l.report_data, ''$.regNumber''), JSON_VALUE(l.report_data, ''$.regOffice''),
            JSON_VALUE(l.report_data, ''$.relationship''), JSON_VALUE(l.report_data, ''$.remarks''), JSON_VALUE(l.report_data, ''$.reportStatus''),
            JSON_VALUE(l.report_data, ''$.reportType''), JSON_VALUE(l.report_data, ''$.reqId''), JSON_VALUE(l.report_data, ''$.restrictedBuyer''),
            JSON_VALUE(l.report_data, ''$.revenueOffice''), JSON_VALUE(l.report_data, ''$.reviewedBy''), JSON_VALUE(l.report_data, ''$.reviewedDate''),
            JSON_VALUE(l.report_data, ''$.reviewedLicence''), JSON_VALUE(l.report_data, ''$.roCompany''), JSON_VALUE(l.report_data, ''$.roadType''),
            JSON_VALUE(l.report_data, ''$.roadWidth''), JSON_VALUE(l.report_data, ''$.saleDeedCopy''), JSON_VALUE(l.report_data, ''$.saleDeedDate''),
            JSON_VALUE(l.report_data, ''$.scAccessibility''), JSON_VALUE(l.report_data, ''$.scDevelopment''), JSON_VALUE(l.report_data, ''$.scExterior''),
            JSON_VALUE(l.report_data, ''$.scFlooring''), JSON_VALUE(l.report_data, ''$.scInterior''), JSON_VALUE(l.report_data, ''$.scIrrigation''),
            JSON_VALUE(l.report_data, ''$.scLiquidity''), JSON_VALUE(l.report_data, ''$.scMarketability''), JSON_VALUE(l.report_data, ''$.scSoil''),
            JSON_VALUE(l.report_data, ''$.scStructure''), JSON_VALUE(l.report_data, ''$.scopeLimit''), JSON_VALUE(l.report_data, ''$.soilFertility''),
            JSON_VALUE(l.report_data, ''$.soilType''), JSON_VALUE(l.report_data, ''$.soilType_r''), JSON_VALUE(l.report_data, ''$.source''),
            JSON_VALUE(l.report_data, ''$.structuralCondition''), JSON_VALUE(l.report_data, ''$.superBuiltup''), JSON_VALUE(l.report_data, ''$.surrDevelopment''),
            JSON_VALUE(l.report_data, ''$.surrDevelopment_r''), JSON_VALUE(l.report_data, ''$.surveyKhasra''), JSON_VALUE(l.report_data, ''$.tatDue''),
            JSON_VALUE(l.report_data, ''$.taxReceipt''), JSON_VALUE(l.report_data, ''$.tehsil''), JSON_VALUE(l.report_data, ''$.tenureType''),
            JSON_VALUE(l.report_data, ''$.terrace''), JSON_VALUE(l.report_data, ''$.topoDrainage''), JSON_VALUE(l.report_data, ''$.topography''),
            JSON_VALUE(l.report_data, ''$.totalFloors''), JSON_VALUE(l.report_data, ''$.vBoundary''), JSON_VALUE(l.report_data, ''$.vBunds''),
            JSON_VALUE(l.report_data, ''$.vCultivation''), JSON_VALUE(l.report_data, ''$.vEncroach''), JSON_VALUE(l.report_data, ''$.vIdentified''),
            JSON_VALUE(l.report_data, ''$.vLocated''), JSON_VALUE(l.report_data, ''$.vPossession''), JSON_VALUE(l.report_data, ''$.vVacant''),
            JSON_VALUE(l.report_data, ''$.valStatement''), JSON_VALUE(l.report_data, ''$.valuationPurpose''), JSON_VALUE(l.report_data, ''$.village''),
            JSON_VALUE(l.report_data, ''$.villageColony''), JSON_VALUE(l.report_data, ''$.yearBuilt'')
    FROM dbo.leads l
    WHERE l.report_data IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM dbo.leadreportdata r WHERE r.lead_id = l.id);
    ALTER TABLE dbo.leads DROP COLUMN report_data;';

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

/* ---------- statustype (lead-stage lookup; code matches leads.stage) ---------- */
IF OBJECT_ID('dbo.statustype', 'U') IS NULL
CREATE TABLE dbo.statustype (
    id     INT          NOT NULL PRIMARY KEY,
    code   NVARCHAR(24) NOT NULL,   -- matches the leads.stage enum value
    label  NVARCHAR(64) NOT NULL,   -- human label shown in lists / dropdowns
    sort   INT          NOT NULL CONSTRAINT df_statustype_sort   DEFAULT 0,
    active BIT          NOT NULL CONSTRAINT df_statustype_active DEFAULT 1,
    CONSTRAINT uq_statustype_code UNIQUE (code)
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
