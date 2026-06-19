import { useParams, useNavigate } from "react-router-dom";
import { useLead, useUpdateLead } from "../lib/queries.js";
import api from "../lib/api.js";
import { Spinner, ErrorBox, Pill } from "../components/ui.jsx";
import { BUCKET_MAP, TAT_TONE, ASSET_LABEL } from "../lib/constants.js";
import { inr } from "../lib/format.js";

function Field({ label, value }) {
  return (
    <div style={{ marginBottom: 12 }}>
      <div className="eyebrow" style={{ marginBottom: 3 }}>{label}</div>
      <div style={{ fontSize: 14, fontWeight: 600, color: "var(--navy)" }}>{value ?? "—"}</div>
    </div>
  );
}

export default function LeadDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { data: lead, isLoading, error } = useLead(id);
  const update = useUpdateLead(id);

  function reassign() {
    const valuator = window.prompt("Assign / reassign to valuator (name):", lead?.valuator || "");
    if (valuator) update.mutate({ action: "reassign", valuator },
      { onError: (e) => alert(e?.error || "Failed to reassign.") });
  }

  function reject() {
    if (window.confirm("Reject this lead? This is terminal."))
      update.mutate({ action: "reject" }, { onError: (e) => alert(e?.error || "Failed to reject.") });
  }

  async function downloadPdf() {
    try {
      const res = await api.get(`/leads/${id}/report.pdf`, { responseType: "blob" });
      const url = URL.createObjectURL(res.data);
      window.open(url, "_blank");
      setTimeout(() => URL.revokeObjectURL(url), 60_000);
    } catch (e) {
      alert(e?.error || "Report not available before QC stage.");
    }
  }

  if (isLoading) return <Spinner label="Loading lead…" />;
  if (error) return <ErrorBox error={error} />;

  const meta = BUCKET_MAP[lead.stage];
  // Render report data as plain string fields (skip nested objects for now).
  const dataEntries = Object.entries(lead.data ?? {}).filter(
    ([, v]) => v != null && typeof v !== "object"
  );

  return (
    <>
      <div className="page-head">
        <div>
          <button className="btn btn-ghost btn-sm" onClick={() => navigate(-1)} style={{ marginBottom: 8 }}>
            ← Back
          </button>
          <h1>{lead.applicant}</h1>
          <div className="sub">
            <span className="mono">{lead.reqId}</span> · {ASSET_LABEL[lead.type] || lead.type} · {lead.ptype}
          </div>
        </div>
        <div style={{ display: "flex", flexDirection: "column", gap: 10, alignItems: "flex-end" }}>
          <div style={{ display: "flex", gap: 8, alignItems: "center" }}>
            <Pill tone={meta?.tone || "info"}>{meta?.status || lead.stage}</Pill>
            <Pill tone={TAT_TONE[lead.tatState] || "info"}>TAT {lead.tatPct}%</Pill>
          </div>
          <div style={{ display: "flex", gap: 8 }}>
            <button className="btn btn-primary btn-sm" onClick={() => navigate(`/leads/${id}/edit`)}>Edit Report</button>
            <button className="btn btn-ghost btn-sm" onClick={() => window.open(`/report/${id}`, "_blank")}>View Report</button>
            <button className="btn btn-ghost btn-sm" onClick={reassign} disabled={update.isPending}>Assign / Reassign</button>
            <button className="btn btn-ghost btn-sm" onClick={downloadPdf}>Download PDF</button>
            <button className="btn btn-danger btn-sm" onClick={reject} disabled={update.isPending}>Reject</button>
          </div>
        </div>
      </div>

      <div className="grid grid-3" style={{ marginBottom: 16 }}>
        <div className="card card-pad">
          <Field label="Lender" value={lead.lender} />
          <Field label="Branch" value={lead.branch} />
          <Field label="Loan / Prospect No." value={lead.loanNo} />
          <Field label="Source" value={lead.source} />
        </div>
        <div className="card card-pad">
          <Field label="Assigned Valuator" value={lead.valuator} />
          <Field label="RO Company" value={lead.roCompany} />
          <Field label="Bank Executive" value={lead.exec} />
          <Field label="Exec Phone" value={lead.execPhone} />
        </div>
        <div className="card card-pad">
          <Field label="Report Status" value={lead.reportStatus} />
          <Field label="Adopted / Fair Value" value={inr(lead.value)} />
          <Field label="Lead Date" value={lead.leadDate} />
          <Field label="Assigned On" value={lead.assignedOn} />
        </div>
      </div>

      <div className="card">
        <div className="card-head"><h3>Report Data</h3></div>
        <div className="card-pad">
          {dataEntries.length === 0 ? (
            <span className="muted">No report data captured yet.</span>
          ) : (
            <div className="grid grid-4">
              {dataEntries.map(([k, v]) => (
                <Field key={k} label={k} value={String(v)} />
              ))}
            </div>
          )}
        </div>
      </div>
    </>
  );
}
