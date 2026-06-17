// The 12 pipeline buckets, in sidebar order (BACKEND_SPEC §4).
export const BUCKETS = [
  { key: "fresh", label: "Fresh Leads", status: "Fresh", tone: "info" },
  { key: "ro", label: "RO Leads", status: "RO Queue", tone: "info" },
  { key: "assigned", label: "Assigned Leads", status: "Assigned", tone: "info" },
  { key: "reassigned", label: "Reassigned Leads", status: "Reassigned", tone: "fair" },
  { key: "ro_confirmation", label: "RO Confirmation Stage", status: "Site Visit Done", tone: "info" },
  { key: "qc", label: "QC Stage", status: "Under Review", tone: "fair" },
  { key: "qc_hold", label: "QC Hold Stage", status: "On Hold", tone: "poor" },
  { key: "pricing", label: "Pricing Stage", status: "Pricing", tone: "fair" },
  { key: "completed", label: "Completed Stage", status: "Approved · Issued", tone: "good" },
  { key: "out_of_tat", label: "Out of TAT Leads", status: "Overdue", tone: "poor" },
  { key: "duplicate", label: "Duplicate Leads", status: "Duplicate", tone: "slate" },
  { key: "rejected", label: "Rejected Leads", status: "Rejected", tone: "poor" },
];

export const BUCKET_MAP = Object.fromEntries(BUCKETS.map((b) => [b.key, b]));

export const TAT_TONE = { ok: "good", warn: "fair", over: "poor" };

export const ASSET_LABEL = { property: "Property", plot: "Plot", agri: "Agri Land" };
