namespace PropertyKwikCheck.Core.Mapping;

/// <summary>
/// Canonical list of report-payload field keys. This is the single source of truth that
/// maps 1:1 to the columns of <c>dbo.leadreportdata</c> (each key is a column of the same
/// name). Used by the repository to read (FOR JSON) and write (column-wise upsert) the
/// report payload without a JSON blob. Keep in sync with db/schema.sql and the frontend
/// wizard schema (web/src/lib/wizardSchema.js).
/// </summary>
public static class ReportFields
{
    public static readonly string[] All =
    [
        "addrActual", "addrDoc", "adjustment", "adoptedRate", "adoptedValue", "agriMarketability",
        "agriMarketability_r", "applicant", "approachAccess", "areaForVal", "assignedRO", "authorisedBy",
        "authorisedDate", "avgRate", "balcony", "bathFloor", "bathrooms", "bedFloor",
        "bedrooms", "borewellPower", "branch", "builtup", "canalSupport", "carpet",
        "ceiling", "civicServices", "civicServices_r", "claimNo", "cmp1Rate", "cmp2Rate",
        "cmp3Rate", "coApplicant", "conditionKey", "config", "connectivity", "contact",
        "cornerAdv", "croppingPattern", "croppingPattern_r", "currentStatus", "demand", "devActivity",
        "dimE", "dimN", "dimS", "dimTotal", "dimW", "disclaimer",
        "disputeRelated", "distBranch", "distHighway", "distHospital", "distMainRoad", "distMandi",
        "distMarket", "distMetalledRoad", "distSchool", "distVillageAbadi", "distressObs", "distressPct",
        "distressValue", "districtState", "dlcAdjust", "dlcArea", "dlcBasis", "dlcRate",
        "dlcUsedFor", "dlcValue", "docType", "docsRelied", "docsShown", "electrical",
        "encumbrance", "execEmail", "execName", "execPhone", "facade", "facing",
        "fairMarketValue", "floorNo", "foundation", "frontageDepth", "gps", "htLine",
        "incomeDependence", "inspectedBy", "inspectedDate", "inspectedLicence", "inspectionDate", "intWalls",
        "interiorLocation", "irrigation", "irrigation_r", "issuedDate", "jamabandiMap", "jamabandiYear",
        "khasraGirdawari", "khasraNumber", "khataNumber", "khatedarName", "kitchen", "kitchenFloor",
        "landArea", "landCeiling", "landUse", "landUseRestriction", "landUse_r", "landmark",
        "layoutPlan", "leadDate", "leadId", "lender", "liquidityObs", "livingFloor",
        "livingHall", "loanNo", "loanType", "lobby", "localityClass", "localityEnquiry",
        "localityStatus", "localityStatus_r", "locationRemark", "marketability", "marketability_r", "masterBed",
        "mutationEntry", "mutationEntryT", "mutationStatus", "nearestTown", "overallRisk", "ownerName",
        "ownership", "ownershipType", "paperCheck", "personMet", "plotArea", "plotNumber",
        "powerConnection", "powerConnection_r", "propertyType", "rccFrame", "realizablePct", "realizableValue",
        "redevelopment", "regNumber", "regOffice", "relationship", "remarks", "reportStatus",
        "reportType", "reqId", "restrictedBuyer", "revenueOffice", "reviewedBy", "reviewedDate",
        "reviewedLicence", "roCompany", "roadType", "roadWidth", "saleDeedCopy", "saleDeedDate",
        "scAccessibility", "scDevelopment", "scExterior", "scFlooring", "scInterior", "scIrrigation",
        "scLiquidity", "scMarketability", "scSoil", "scStructure", "scopeLimit", "soilFertility",
        "soilType", "soilType_r", "source", "structuralCondition", "superBuiltup", "surrDevelopment",
        "surrDevelopment_r", "surveyKhasra", "tatDue", "taxReceipt", "tehsil", "tenureType",
        "terrace", "topoDrainage", "topography", "totalFloors", "vBoundary", "vBunds",
        "vCultivation", "vEncroach", "vIdentified", "vLocated", "vPossession", "vVacant",
        "valStatement", "valuationPurpose", "village", "villageColony", "yearBuilt",
    ];

    /// <summary>Fast membership set for write-time whitelisting.</summary>
    public static readonly HashSet<string> Set = new(All, StringComparer.Ordinal);
}
