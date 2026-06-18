// Field definitions for the 5-stage report wizard (BACKEND_SPEC §7).
// Field types: text | ta (textarea) | num | rupee | date | sel | seg (Yes/No)
//   | segp (Yes/No/Partial) | textrate (text value + rating good/fair/avg/poor)
import { PhotoFrames } from "./photoFrames.js";

export const STAGES = [
  { key: "intake", label: "Intake" },
  { key: "site", label: "Site & Records" },
  { key: "technical", label: "Technical & Risk" },
  { key: "valuation", label: "Valuation & Sign-off" },
  { key: "photos", label: "Photos & Evidence" },
];

const REPORT_TYPES = ["Property Inspection", "Plot Valuation", "Agri Land Valuation"];
const SOURCES = ["Bank Portal", "DSA Referral", "Branch Walk-in", "Direct Customer", "API Integration", "Tele-calling"];
const PURPOSES = ["Home Loan", "Loan Against Property", "Bank Loan — Collateral", "Agri Term Loan — Collateral", "Renewal / Review", "Auction / SARFAESI", "Other"];
const LOAN_TYPES = ["Home Loan", "Loan Against Property", "Agricultural Loan", "Business Loan", "Mortgage", "Other"];
const REPORT_STATUS = ["Open", "Assigned to Valuer", "Awaiting RO Confirmation", "In QC Review", "On Hold", "Pricing", "Verified & Issued", "Rejected"];
const FACING = ["E", "W", "N", "S", "NE", "NW", "SE", "SW"];

const f = (k, label, t = "text", options) => ({ k, label, t, options });

// ---- Stage 1: Intake (common) -----------------------------------------------
const INTAKE = [
  f("reportType", "Report Type", "sel", REPORT_TYPES),
  f("propertyType", "Property / Asset Type"),
  f("loanNo", "Loan / Prospect No."),
  f("claimNo", "Bank Claim / Ref No."),
  f("leadDate", "Lead Date", "date"),
  f("source", "Source", "sel", SOURCES),
  f("valuationPurpose", "Valuation Purpose", "sel", PURPOSES),
  f("loanType", "Loan Type", "sel", LOAN_TYPES),
  f("reportStatus", "Report Status", "sel", REPORT_STATUS),
  f("applicant", "Applicant Name"),
  f("coApplicant", "Co-Applicant Name"),
  f("contact", "Contact Number"),
  f("lender", "Lender / Bank"),
  f("branch", "Branch"),
  f("assignedRO", "Assigned RO / Valuer"),
  f("roCompany", "RO Company Name"),
  f("execName", "Bank Executive"),
  f("execPhone", "Executive Phone"),
  f("execEmail", "Executive Email"),
  f("addrDoc", "Address (As Per Document)", "ta"),
  f("addrActual", "Address (Actual / At Site)", "ta"),
  f("docsRelied", "Documents Relied Upon"),
  f("inspectionDate", "Inspection Date", "date"),
  f("issuedDate", "Issued Date", "date"),
  f("tatDue", "TAT Due", "date"),
  f("remarks", "Remarks", "ta"),
];

// ---- Stage 2: Site & Records (per family) -----------------------------------
const SITE = {
  property: [
    f("personMet", "Person Met"), f("relationship", "Relationship"), f("docsShown", "Documents Shown"),
    f("dimN", "Dimension North (ft)", "num"), f("dimS", "Dimension South (ft)", "num"),
    f("dimE", "Dimension East (ft)", "num"), f("dimW", "Dimension West (ft)", "num"), f("dimTotal", "Total Area (ft)", "num"),
    f("config", "Configuration"), f("carpet", "Carpet Area", "num"), f("builtup", "Built-up Area", "num"),
    f("superBuiltup", "Super Built-up", "num"), f("yearBuilt", "Year Built"), f("facing", "Facing", "sel", FACING),
    f("floorNo", "Floor No."), f("totalFloors", "Total Floors"),
    f("ownership", "Ownership", "sel", ["Freehold", "Leasehold", "Power of Attorney"]),
    f("paperCheck", "Paper Check Done", "seg"), f("localityEnquiry", "Locality Enquiry", "seg"),
    f("disputeRelated", "Dispute Related", "seg"), f("redevelopment", "Redevelopment", "seg"),
    f("conditionKey", "Condition", "sel", ["Good", "Fair", "Average", "Poor"]),
    f("currentStatus", "Current Status", "sel", ["Self-Occupied", "Rented", "Vacant"]),
    f("localityClass", "Locality Class"), f("roadType", "Road Type"), f("landmark", "Landmark"),
    f("distBranch", "Distance from Branch"),
    f("demand", "Demand", "sel", ["High demand", "Moderate demand", "Low demand"]),
    f("connectivity", "Connectivity", "ta"), f("structuralCondition", "Structural Condition", "ta"),
  ],
  plot: [
    f("vLocated", "Located", "seg"), f("vIdentified", "Identified", "seg"), f("vVacant", "Vacant", "seg"),
    f("vEncroach", "Encroachment", "seg"), f("vBoundary", "Boundary", "segp"),
    f("vPossession", "Possession", "sel", ["Available", "With Owner", "Disputed", "Not available"]),
    f("plotNumber", "Plot Number"), f("surveyKhasra", "Survey / Khasra"), f("villageColony", "Village / Colony"),
    f("tehsil", "Tehsil"), f("districtState", "District / State"), f("gps", "GPS"),
    f("ownerName", "Owner Name"),
    f("ownershipType", "Ownership Type", "sel", ["Freehold", "Leasehold", "Power of Attorney", "Ancestral"]),
    f("saleDeedDate", "Sale Deed Date", "date"), f("regNumber", "Reg. Number"), f("regOffice", "Reg. Office"),
    f("mutationStatus", "Mutation Status"),
    f("distMainRoad", "Dist. Main Road"), f("distMarket", "Dist. Market"), f("distSchool", "Dist. School"),
    f("distHospital", "Dist. Hospital"), f("distHighway", "Dist. Highway"),
    f("localityStatus", "Locality Status", "textrate"), f("surrDevelopment", "Surrounding Development", "textrate"),
    f("landUse", "Land Use", "textrate"), f("civicServices", "Civic Services", "textrate"),
    f("marketability", "Marketability", "textrate"),
    f("locationRemark", "Location Remark", "ta"),
  ],
  agri: [
    f("vLocated", "Located", "seg"), f("vIdentified", "Identified", "seg"), f("vCultivation", "Cultivation", "seg"),
    f("vEncroach", "Encroachment", "seg"), f("vBunds", "Bunds", "segp"),
    f("vPossession", "Possession", "sel", ["With Owner", "With Tenant", "Disputed", "Vacant"]),
    f("khasraNumber", "Khasra Number"), f("khataNumber", "Khata Number"), f("village", "Village"),
    f("tehsil", "Tehsil"), f("districtState", "District / State"), f("gps", "GPS"), f("jamabandiYear", "Jamabandi Year"),
    f("khatedarName", "Khatedar Name"),
    f("tenureType", "Tenure Type", "sel", ["Khatedari — agricultural", "Gair-Khatedari", "Government lease", "Bhumidhari"]),
    f("mutationEntry", "Mutation Entry"), f("revenueOffice", "Revenue Office"), f("landCeiling", "Land Ceiling"),
    f("distMetalledRoad", "Dist. Metalled Road"), f("distVillageAbadi", "Dist. Village Abadi"),
    f("distMandi", "Dist. Mandi"), f("distHighway", "Dist. Highway"), f("nearestTown", "Nearest Town"),
    f("soilType", "Soil Type", "textrate"), f("irrigation", "Irrigation", "textrate"),
    f("croppingPattern", "Cropping Pattern", "textrate"), f("powerConnection", "Power Connection", "textrate"),
    f("agriMarketability", "Marketability", "textrate"),
    f("locationRemark", "Location Remark", "ta"),
  ],
};

// ---- Stage 3: Technical & Risk (scores + rated params, spec §16.3) ----------
const TECHNICAL = {
  property: {
    scores: [["scInterior", "Interior"], ["scExterior", "Exterior"], ["scStructure", "Structure"], ["scFlooring", "Flooring"]],
    groups: [
      ["Interior", [["livingHall", "Living Hall"], ["kitchen", "Kitchen"], ["masterBed", "Master Bed"], ["bedrooms", "Bedrooms"], ["bathrooms", "Bathrooms"], ["electrical", "Electrical"]]],
      ["Exterior & Common", [["facade", "Facade"], ["balcony", "Balcony"], ["lobby", "Lobby"], ["terrace", "Terrace"]]],
      ["Structural", [["foundation", "Foundation"], ["rccFrame", "RCC Frame"], ["intWalls", "Internal Walls"], ["ceiling", "Ceiling"]]],
      ["Flooring", [["livingFloor", "Living Floor"], ["bedFloor", "Bedroom Floor"], ["kitchenFloor", "Kitchen Floor"], ["bathFloor", "Bathroom Floor"]]],
    ],
  },
  plot: {
    scores: [["scAccessibility", "Accessibility"], ["scMarketability", "Marketability"], ["scDevelopment", "Development"], ["scLiquidity", "Liquidity"]],
    groups: [
      ["Physical & Technical", [["plotArea", "Plot Area"], ["frontageDepth", "Frontage/Depth"], ["roadWidth", "Road Width"], ["topography", "Topography"], ["cornerAdv", "Corner Advantage"]]],
      ["Legal Docs", [["saleDeedCopy", "Sale Deed Copy"], ["mutationEntry", "Mutation Entry"], ["taxReceipt", "Tax Receipt"], ["layoutPlan", "Layout Plan"], ["encumbrance", "Encumbrance"]]],
      ["Risk", [["interiorLocation", "Interior Location"], ["htLine", "HT Line"], ["devActivity", "Dev. Activity"], ["liquidityObs", "Liquidity Obs."], ["distressObs", "Distress Obs."]]],
    ],
  },
  agri: {
    scores: [["scSoil", "Soil"], ["scIrrigation", "Irrigation"], ["scAccessibility", "Accessibility"], ["scLiquidity", "Liquidity"]],
    groups: [
      ["Land & Soil", [["landArea", "Land Area"], ["soilFertility", "Soil Fertility"], ["topoDrainage", "Topo/Drainage"], ["approachAccess", "Approach Access"]]],
      ["Irrigation & Infra", [["borewellPower", "Borewell/Power"], ["canalSupport", "Canal Support"]]],
      ["Legal & Revenue", [["jamabandiMap", "Jamabandi Map"], ["khasraGirdawari", "Khasra Girdawari"], ["mutationEntryT", "Mutation Entry"], ["encumbrance", "Encumbrance"]]],
      ["Risk", [["landUseRestriction", "Land-Use Restriction"], ["restrictedBuyer", "Restricted Buyer"], ["incomeDependence", "Income Dependence"], ["distressObs", "Distress Obs."]]],
    ],
  },
};

// ---- Stage 4: Valuation & Sign-off (common) ---------------------------------
const VALUATION = [
  f("valStatement", "Valuation Statement", "ta"),
  f("cmp1Rate", "Comparable Rate 1", "num"), f("cmp2Rate", "Comparable Rate 2", "num"), f("cmp3Rate", "Comparable Rate 3", "num"),
  f("avgRate", "Average Rate", "num"), f("adjustment", "Adjustment (%)", "num"), f("adoptedRate", "Adopted Rate", "num"),
  f("areaForVal", "Area for Valuation", "num"), f("adoptedValue", "Adopted Value", "rupee"),
  f("dlcRate", "DLC / Circle Rate", "num"), f("dlcArea", "DLC Area", "num"), f("dlcAdjust", "DLC Adjustment", "num"),
  f("dlcBasis", "DLC Basis"), f("dlcUsedFor", "DLC Used For"), f("dlcValue", "DLC Value", "rupee"),
  f("fairMarketValue", "Fair Market Value", "rupee"),
  f("realizablePct", "Realizable %", "num"), f("realizableValue", "Realizable Value", "rupee"),
  f("distressPct", "Distress %", "num"), f("distressValue", "Distress Value", "rupee"),
  f("overallRisk", "Overall Risk"),
  f("scopeLimit", "Scope & Limitations", "ta"), f("disclaimer", "Disclaimer", "ta"),
  f("inspectedBy", "Inspected By"), f("inspectedLicence", "Inspector Licence"), f("inspectedDate", "Inspected Date", "date"),
  f("reviewedBy", "Reviewed By"), f("reviewedLicence", "Reviewer Licence"), f("reviewedDate", "Reviewed Date", "date"),
  f("authorisedBy", "Authorised By"), f("authorisedDate", "Authorised Date", "date"),
];

export function stageFields(stage, family) {
  switch (stage) {
    case "intake": return INTAKE;
    case "site": return SITE[family] || [];
    case "valuation": return VALUATION;
    default: return [];
  }
}

export function technicalSchema(family) {
  return TECHNICAL[family] || { scores: [], groups: [] };
}

export { PhotoFrames };
