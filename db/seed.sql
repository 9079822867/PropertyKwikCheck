/* ============================================================================
   PropertyKwikCheck — seed data (BACKEND_SPEC §14 / §16)
   Idempotent: every block is guarded so it can be re-run safely.
   Default password for all seeded users: Password@123
   ============================================================================ */

/* ---------- Roles ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Roles)
INSERT INTO dbo.Roles (id, role_name, remark) VALUES
    (1, 'Client',   NULL),
    (2, 'RO',       NULL),
    (3, 'Internal', NULL),
    (4, 'Cando',    NULL);

/* ---------- UserTypes ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.UserTypes)
INSERT INTO dbo.UserTypes (id, company_type_id, name) VALUES
    (1, 1, 'Client Executive'),
    (2, 1, 'Client Hub Head'),
    (3, 1, 'Client State Head'),
    (4, 1, 'Client zonal Head'),
    (5, 1, 'Client National Manager'),
    (6, 1, 'Client Admin'),
    (7, 2, 'RO admin'),
    (8, 2, 'RO Valuators'),
    (9, 3, 'State Coordinator'),
    (10, 3, 'State Head'),
    (11, 3, 'Qc Manager'),
    (12, 3, 'Pricing Manager'),
    (13, 3, 'Zonal Head'),
    (14, 3, 'National Head'),
    (15, 3, 'Business Head'),
    (16, 3, 'Admin'),
    (17, 4, 'CANDO VALUATOR'),
    (18, 4, 'Cando Admin'),
    (19, 3, 'Super Admin'),
    (20, 1, 'Cando Executive');

/* ---------- statustype (lead-stage lookup; code = leads.stage value) ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.statustype)
INSERT INTO dbo.statustype (id, code, label, sort) VALUES
    (1,  'fresh',           'Fresh',            1),
    (2,  'ro',              'RO Queue',         2),
    (3,  'assigned',        'Assigned',         3),
    (4,  'reassigned',      'Reassigned',       4),
    (5,  'ro_confirmation', 'RO Confirmation',  5),
    (6,  'qc',              'QC Review',        6),
    (7,  'qc_hold',         'QC Hold',          7),
    (8,  'pricing',         'Pricing',          8),
    (9,  'completed',       'Completed',        9),
    (10, 'out_of_tat',      'Out of TAT',       10),
    (11, 'duplicate',       'Duplicate',        11),
    (12, 'rejected',        'Rejected',         12);

/* ---------- companies ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.companies WHERE name = 'NGD Kwik Check Pvt Ltd')
    INSERT INTO dbo.companies (name, type, spoc_name, status)
    VALUES ('NGD Kwik Check Pvt Ltd', 'Valuation Agency · Owner', 'Operations Desk', 'Active');
IF NOT EXISTS (SELECT 1 FROM dbo.companies WHERE name = 'HDFC Bank Ltd')
    INSERT INTO dbo.companies (name, type, spoc_name, status)
    VALUES ('HDFC Bank Ltd', 'Lender · Bank', 'Meena Patil', 'Active');
IF NOT EXISTS (SELECT 1 FROM dbo.companies WHERE name = 'ICICI Bank Ltd')
    INSERT INTO dbo.companies (name, type, spoc_name, status)
    VALUES ('ICICI Bank Ltd', 'Lender · Bank', 'Arjun Rao', 'Active');
IF NOT EXISTS (SELECT 1 FROM dbo.companies WHERE name = 'Saraswat Co-operative Bank Ltd')
    INSERT INTO dbo.companies (name, type, spoc_name, status)
    VALUES ('Saraswat Co-operative Bank Ltd', 'Lender · Co-op Bank', 'Shivkumar Sharma', 'Active');

/* RO valuation firms — real company rows so leads can be assigned company → valuator. */
IF NOT EXISTS (SELECT 1 FROM dbo.companies WHERE name = 'Kwik Check Pvt Ltd')
    INSERT INTO dbo.companies (name, type, spoc_name, status)
    VALUES ('Kwik Check Pvt Ltd', 'RO · Valuation Firm', 'Rahul Mehta', 'Active');
IF NOT EXISTS (SELECT 1 FROM dbo.companies WHERE name = 'Apex Valuers LLP')
    INSERT INTO dbo.companies (name, type, spoc_name, status)
    VALUES ('Apex Valuers LLP', 'RO · Valuation Firm', 'Anjali Deshpande', 'Active');

/* ---------- users (password = Password@123) ---------- */
DECLARE @pwd NVARCHAR(255) = '$2a$12$JSCPVZX6UP6OG0u7pyPVhu3dmUNB25e6dMVcWAtxIFYZK.EzHkJOq';
DECLARE @ngd BIGINT  = (SELECT id FROM dbo.companies WHERE name = 'NGD Kwik Check Pvt Ltd');
DECLARE @hdfc BIGINT = (SELECT id FROM dbo.companies WHERE name = 'HDFC Bank Ltd');

IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE email = 'superadmin@kwikcheck.in')
    INSERT INTO dbo.users (name, email, password_hash, role_id, user_type_id, company_id, phone, status)
    VALUES ('Super Admin', 'superadmin@kwikcheck.in', @pwd, 3, 19, @ngd, '90000 00001', 'Active');
IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE email = 'leadmgr@kwikcheck.in')
    INSERT INTO dbo.users (name, email, password_hash, role_id, user_type_id, company_id, phone, status)
    VALUES ('Vikram Singh', 'leadmgr@kwikcheck.in', @pwd, 3, 9, @ngd, '90000 00002', 'Active');
IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE email = 'qc@kwikcheck.in')
    INSERT INTO dbo.users (name, email, password_hash, role_id, user_type_id, company_id, phone, status)
    VALUES ('Neha Joshi', 'qc@kwikcheck.in', @pwd, 3, 11, @ngd, '90000 00003', 'Active');
IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE email = 'pricing@kwikcheck.in')
    INSERT INTO dbo.users (name, email, password_hash, role_id, user_type_id, company_id, phone, status)
    VALUES ('Datta Kulkarni', 'pricing@kwikcheck.in', @pwd, 3, 12, @ngd, '90000 00004', 'Active');
IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE email = 'authoriser@kwikcheck.in')
    INSERT INTO dbo.users (name, email, password_hash, role_id, user_type_id, company_id, phone, status)
    VALUES ('Suresh Rao', 'authoriser@kwikcheck.in', @pwd, 3, 14, @ngd, '90000 00005', 'Active');
IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE email = 'rahul@kwikcheck.in')
    INSERT INTO dbo.users (name, email, password_hash, role_id, user_type_id, company_id, phone, licence_no, status)
    VALUES ('Rahul Mehta', 'rahul@kwikcheck.in', @pwd, 2, 8, @ngd, '90000 00006', 'KC-CPV-0473', 'Active');
IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE email = 'ajay@kwikcheck.in')
    INSERT INTO dbo.users (name, email, password_hash, role_id, user_type_id, company_id, phone, licence_no, status)
    VALUES ('Ajay Malviya', 'ajay@kwikcheck.in', @pwd, 2, 8, @ngd, '90000 00007', 'KC-CPV-0512', 'Active');
IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE email = 'meena.p@kwikcheck.in')
    INSERT INTO dbo.users (name, email, password_hash, role_id, user_type_id, company_id, phone, status)
    VALUES ('Meena Patil', 'meena.p@kwikcheck.in', @pwd, 1, 1, @hdfc, '99220 14785', 'Active');

/* ---------- RO valuators ↔ RO firm linkage (so company → valuator assign works) ---------- */
DECLARE @kwikRO BIGINT = (SELECT id FROM dbo.companies WHERE name = 'Kwik Check Pvt Ltd');
DECLARE @apexRO BIGINT = (SELECT id FROM dbo.companies WHERE name = 'Apex Valuers LLP');

-- Move the seeded RO valuators onto the Kwik Check RO firm (idempotent).
UPDATE dbo.users SET company_id = @kwikRO
 WHERE email IN ('rahul@kwikcheck.in', 'ajay@kwikcheck.in') AND company_id <> @kwikRO;

-- A second RO firm's valuator, so the two-step assign has more than one company.
IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE email = 'anjali@kwikcheck.in')
    INSERT INTO dbo.users (name, email, password_hash, role_id, user_type_id, company_id, phone, licence_no, status)
    VALUES ('Anjali Deshpande', 'anjali@kwikcheck.in', @pwd, 2, 8, @apexRO, '90000 00008', 'KC-CPV-0588', 'Active');

/* ---------- master_lookups ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.master_lookups WHERE category = 'banks')
INSERT INTO dbo.master_lookups (category, value, sort) VALUES
    ('banks','HDFC Bank Ltd',1),('banks','Saraswat Co-operative Bank Ltd',2),('banks','ICICI Bank Ltd',3),
    ('banks','Axis Bank Ltd',4),('banks','Bajaj Finserv',5),('banks','Kogta Financial India Ltd',6),
    ('banks','MDFC Financiers Pvt Ltd',7),('banks','State Bank of India',8),('banks','Kotak Mahindra Bank',9),
    ('banks','Tata Capital Housing Finance',10);

IF NOT EXISTS (SELECT 1 FROM dbo.master_lookups WHERE category = 'valuers')
INSERT INTO dbo.master_lookups (category, value, sort) VALUES
    ('valuers','Rahul Mehta',1),('valuers','Ajay Malviya',2),('valuers','Anjali Deshpande',3),
    ('valuers','Suresh Rao',4),('valuers','Priya Nair',5),('valuers','Datta Kulkarni',6),
    ('valuers','Vikram Singh',7),('valuers','Neha Joshi',8);

IF NOT EXISTS (SELECT 1 FROM dbo.master_lookups WHERE category = 'executives')
INSERT INTO dbo.master_lookups (category, value, meta, sort) VALUES
    ('executives','Brijesh Prajapati','{"phone":"98704 58149","email":"brijesh.p@kwikcheck.in"}',1),
    ('executives','Shivkumar Sharma','{"phone":"90040 67636","email":"shivkumar.s@kwikcheck.in"}',2),
    ('executives','Gaadi Jeeto','{"phone":"80808 08080","email":"ops@kwikcheck.in"}',3),
    ('executives','Meena Patil','{"phone":"99220 14785","email":"meena.p@kwikcheck.in"}',4),
    ('executives','Arjun Rao','{"phone":"98330 71246","email":"arjun.r@kwikcheck.in"}',5);

IF NOT EXISTS (SELECT 1 FROM dbo.master_lookups WHERE category = 'cities')
INSERT INTO dbo.master_lookups (category, value, meta, sort) VALUES
    ('cities','Jaipur, Rajasthan','{"pin":"302029"}',1),('cities','Mumbai, Maharashtra','{"pin":"400077"}',2),
    ('cities','Pune, Maharashtra','{"pin":"411001"}',3),('cities','Indore, MP','{"pin":"452001"}',4),
    ('cities','Nagpur, Maharashtra','{"pin":"440001"}',5),('cities','Nashik, Maharashtra','{"pin":"422001"}',6),
    ('cities','Surat, Gujarat','{"pin":"395003"}',7),('cities','Bhopal, MP','{"pin":"462001"}',8);

IF NOT EXISTS (SELECT 1 FROM dbo.master_lookups WHERE category = 'sources')
INSERT INTO dbo.master_lookups (category, value, sort) VALUES
    ('sources','Bank Portal',1),('sources','DSA Referral',2),('sources','Branch Walk-in',3),
    ('sources','Direct Customer',4),('sources','API Integration',5),('sources','Tele-calling',6);

IF NOT EXISTS (SELECT 1 FROM dbo.master_lookups WHERE category = 'ro_companies')
INSERT INTO dbo.master_lookups (category, value, sort) VALUES
    ('ro_companies','Kwik Check Pvt Ltd',1),('ro_companies','Apex Valuers LLP',2),
    ('ro_companies','TruEstate Surveyors',3),('ro_companies','Crest Valuation Co.',4),
    ('ro_companies','GeoVal Associates',5);

IF NOT EXISTS (SELECT 1 FROM dbo.master_lookups WHERE category = 'doc_types')
INSERT INTO dbo.master_lookups (category, value, sort) VALUES
    ('doc_types','Sale Deed',1),('doc_types','Title Deed',2),('doc_types','Tax Receipt',3),('doc_types','NOC',4),
    ('doc_types','Layout / Sanction Plan',5),('doc_types','Khasra / Girdawari',6),('doc_types','Jamabandi (RoR)',7),
    ('doc_types','Encumbrance Certificate',8),('doc_types','Mutation',9),('doc_types','Allotment Letter',10),
    ('doc_types','Other',11);

IF NOT EXISTS (SELECT 1 FROM dbo.master_lookups WHERE category = 'rejection_reasons')
INSERT INTO dbo.master_lookups (category, value, sort) VALUES
    ('rejection_reasons','Insufficient documents',1),('rejection_reasons','Property not traceable',2),
    ('rejection_reasons','Duplicate request',3),('rejection_reasons','Out of service area',4);

IF NOT EXISTS (SELECT 1 FROM dbo.master_lookups WHERE category = 'valuation_purposes')
INSERT INTO dbo.master_lookups (category, value, sort) VALUES
    ('valuation_purposes','Home Loan',1),('valuation_purposes','Loan Against Property',2),
    ('valuation_purposes','Bank Loan — Collateral',3),('valuation_purposes','Agri Term Loan — Collateral',4),
    ('valuation_purposes','Renewal / Review',5),('valuation_purposes','Auction / SARFAESI',6),
    ('valuation_purposes','Other',7);

IF NOT EXISTS (SELECT 1 FROM dbo.master_lookups WHERE category = 'asset_types')
INSERT INTO dbo.master_lookups (category, value, sort) VALUES
    ('asset_types','Residential Apartment',1),('asset_types','Residential Plot',2),
    ('asset_types','Agricultural Land',3),('asset_types','Commercial Shop',4),
    ('asset_types','Industrial Shed',5),('asset_types','Commercial Office',6),
    ('asset_types','Residential House',7),('asset_types','Warehouse',8);

/* ---------- dlc_rates ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.dlc_rates)
INSERT INTO dbo.dlc_rates (locality, unit, rate, basis) VALUES
    ('Andheri East, Mumbai','sq.ft',18500,'FY 2025-26'),
    ('Bagru, Jaipur','sq.ft',1200,'FY 2025-26'),
    ('Bassi, Jaipur','bigha',850000,'FY 2025-26'),
    ('Kothrud, Pune','sq.ft',11200,'FY 2025-26');

/* ---------- invoices ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.invoices)
INSERT INTO dbo.invoices (invoice_no, company_id, period, lead_count, amount, status)
SELECT 'INV-2026-0142', id, 'May 2026', 48, 480000, 'Paid'   FROM dbo.companies WHERE name='HDFC Bank Ltd'
UNION ALL
SELECT 'INV-2026-0143', id, 'May 2026', 31, 310000, 'Pending' FROM dbo.companies WHERE name='ICICI Bank Ltd';

/* ---------- hero leads (preserve legacy ids / req-ids) ---------- */
DECLARE @rahul BIGINT = (SELECT id FROM dbo.users WHERE email = 'rahul@kwikcheck.in');
DECLARE @ajayv BIGINT = (SELECT id FROM dbo.users WHERE email = 'ajay@kwikcheck.in');
DECLARE @hdfcId BIGINT = (SELECT id FROM dbo.companies WHERE name = 'HDFC Bank Ltd');
DECLARE @iciciId BIGINT = (SELECT id FROM dbo.companies WHERE name = 'ICICI Bank Ltd');

SET IDENTITY_INSERT dbo.leads ON;

IF NOT EXISTS (SELECT 1 FROM dbo.leads WHERE id = 4812)
INSERT INTO dbo.leads (id, req_id, asset_family, property_type, stage, report_status, applicant, pin, location,
    lender_company_id, lender_name, branch, valuator_user_id, valuator_name, ro_company, exec_name, exec_phone, exec_email,
    loan_no, source, lead_date, assigned_on, tat_due, tat_pct, tat_state, value, remarks, report_data)
VALUES (4812, '4WRP04812', 'property', 'Residential', 'qc', 'In QC Review', 'Mr. Ankit Sharma', '400077', 'Andheri East, Mumbai',
    @hdfcId, 'HDFC Bank Ltd', 'Andheri East, Mumbai', @rahul, 'Rahul Mehta', 'Kwik Check Pvt Ltd', 'Meena Patil', '99220 14785', 'meena.p@kwikcheck.in',
    'HOU52030011', 'Bank Portal', '2026-05-12', '2026-05-18', '2026-05-25', 88, 'warn', 21540000, 'Docs pending from branch',
    N'{"reportType":"Property Inspection","reportStatus":"In QC Review","leadId":"4WRP04812","reqId":"4WRP04812","propertyType":"Residential","applicant":"Mr. Ankit Sharma","lender":"HDFC Bank Ltd","branch":"Andheri East, Mumbai","loanNo":"HOU52030011","addrDoc":"Flat 1203, Sunrise Heights, Andheri East, Mumbai 400077","config":"3BHK","carpet":"980","builtup":"1180","superBuiltup":"1350","yearBuilt":"2016","facing":"E","floorNo":"12","totalFloors":"22","ownership":"Freehold","scInterior":"9.0","scExterior":"7.5","scStructure":"8.5","scFlooring":"8.0","fairMarketValue":"21540000","adoptedValue":"21540000","realizablePct":"90","realizableValue":"19386000","distressPct":"75","distressValue":"16155000","inspectedBy":"Rahul Mehta","inspectedLicence":"KC-CPV-0473","reviewedBy":"Neha Joshi"}');

IF NOT EXISTS (SELECT 1 FROM dbo.leads WHERE id = 4913)
INSERT INTO dbo.leads (id, req_id, asset_family, property_type, stage, report_status, applicant, pin, location,
    lender_company_id, lender_name, branch, valuator_user_id, valuator_name, ro_company, exec_name, exec_phone, exec_email,
    loan_no, source, lead_date, assigned_on, tat_due, tat_pct, tat_state, value, remarks, report_data)
VALUES (4913, '4WPL04913', 'plot', 'Plot', 'assigned', 'Assigned to Valuer', 'Mr. Rajesh Kumar Sharma', '302029', 'Bagru, Jaipur',
    @hdfcId, 'HDFC Bank Ltd', 'Bagru, Jaipur', @ajayv, 'Ajay Malviya', 'Kwik Check Pvt Ltd', 'Brijesh Prajapati', '98704 58149', 'brijesh.p@kwikcheck.in',
    'LAP77120034', 'DSA Referral', '2026-05-14', '2026-05-18', '2026-05-26', 40, 'ok', 2400000, 'Awaiting appointment',
    N'{"reportType":"Plot Valuation","reportStatus":"Assigned to Valuer","leadId":"4WPL04913","reqId":"4WPL04913","propertyType":"Plot","applicant":"Mr. Rajesh Kumar Sharma","lender":"HDFC Bank Ltd","branch":"Bagru, Jaipur","loanNo":"LAP77120034","plotNumber":"B-114","surveyKhasra":"245/2","villageColony":"Shanti Vihar","tehsil":"Bagru","districtState":"Jaipur, Rajasthan","ownerName":"Rajesh Kumar Sharma","ownershipType":"Freehold","scAccessibility":"7.0","scMarketability":"6.5","scDevelopment":"6.0","scLiquidity":"6.5","fairMarketValue":"2400000","adoptedValue":"2400000","realizablePct":"90","realizableValue":"2160000","distressPct":"75","distressValue":"1800000"}');

IF NOT EXISTS (SELECT 1 FROM dbo.leads WHERE id = 4927)
INSERT INTO dbo.leads (id, req_id, asset_family, property_type, stage, report_status, applicant, pin, location,
    lender_company_id, lender_name, branch, valuator_user_id, valuator_name, ro_company, exec_name, exec_phone, exec_email,
    loan_no, source, lead_date, assigned_on, tat_due, tat_pct, tat_state, value, remarks, report_data)
VALUES (4927, '4WAG04927', 'agri', 'Agricultural Land', 'ro_confirmation', 'Awaiting RO Confirmation', 'Mr. Mohan Lal Jat', '302029', 'Bassi, Jaipur',
    @hdfcId, 'HDFC Bank Ltd', 'Bassi, Jaipur', @rahul, 'Rahul Mehta', 'Kwik Check Pvt Ltd', 'Brijesh Prajapati', '98704 58149', 'brijesh.p@kwikcheck.in',
    'AGRI4480021', 'Bank Portal', '2026-05-13', '2026-05-17', '2026-05-27', 55, 'ok', 2000000, 'Site visit done — drafting',
    N'{"reportType":"Agri Land Valuation","reportStatus":"Awaiting RO Confirmation","leadId":"4WAG04927","reqId":"4WAG04927","propertyType":"Agricultural Land","applicant":"Mr. Mohan Lal Jat","lender":"HDFC Bank Ltd","branch":"Bassi, Jaipur","loanNo":"AGRI4480021","khasraNumber":"512","khataNumber":"77","village":"Bassi","tehsil":"Bassi","districtState":"Jaipur, Rajasthan","jamabandiYear":"2024-25","khatedarName":"Mohan Lal Jat","tenureType":"Khatedari — agricultural","scSoil":"7.5","scIrrigation":"7.0","scAccessibility":"6.5","scLiquidity":"6.0","fairMarketValue":"2000000","adoptedValue":"2000000","realizablePct":"90","realizableValue":"1800000","distressPct":"75","distressValue":"1500000"}');

IF NOT EXISTS (SELECT 1 FROM dbo.leads WHERE id = 4790)
INSERT INTO dbo.leads (id, req_id, asset_family, property_type, stage, report_status, applicant, pin, location,
    lender_company_id, lender_name, branch, valuator_user_id, valuator_name, ro_company, exec_name, exec_phone, exec_email,
    loan_no, source, lead_date, assigned_on, inspection_date, issued_date, tat_due, tat_pct, tat_state, value, remarks, report_data)
VALUES (4790, '4WRP04790', 'property', 'Residential', 'completed', 'Verified & Issued', 'Mrs. Sunita Verma', '411001', 'Kothrud, Pune',
    @iciciId, 'ICICI Bank Ltd', 'Kothrud, Pune', @rahul, 'Rahul Mehta', 'Kwik Check Pvt Ltd', 'Arjun Rao', '98330 71246', 'arjun.r@kwikcheck.in',
    'HOU61220098', 'Bank Portal', '2026-05-02', '2026-05-06', '2026-05-08', '2026-05-20', '2026-05-13', 100, 'ok', 9850000, 'Site visit done — drafting',
    N'{"reportType":"Property Inspection","reportStatus":"Verified & Issued","leadId":"4WRP04790","reqId":"4WRP04790","propertyType":"Residential","applicant":"Mrs. Sunita Verma","lender":"ICICI Bank Ltd","branch":"Kothrud, Pune","loanNo":"HOU61220098","config":"2BHK","carpet":"720","builtup":"860","superBuiltup":"980","yearBuilt":"2012","facing":"N","floorNo":"5","totalFloors":"11","ownership":"Freehold","scInterior":"8.0","scExterior":"7.0","scStructure":"8.0","scFlooring":"7.5","fairMarketValue":"9850000","adoptedValue":"9850000","realizablePct":"90","realizableValue":"8865000","distressPct":"75","distressValue":"7387500","inspectedBy":"Rahul Mehta","inspectedLicence":"KC-CPV-0473","reviewedBy":"Neha Joshi","authorisedBy":"Suresh Rao","authorisedDate":"2026-05-20"}');

SET IDENTITY_INSERT dbo.leads OFF;

/* ---------- site visits (yard schedule) ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.site_visits)
INSERT INTO dbo.site_visits (lead_id, valuer_user_id, scheduled_at, location, status)
VALUES
    (4913, @ajayv, '2026-05-19T09:30:00', 'Shanti Vihar, Bagru', 'Checked-in'),
    (4927, @rahul, '2026-05-19T10:30:00', 'Bassi, Jaipur', 'Scheduled');
GO
