import { useState, useEffect } from "react";
import { useSearchParams, useNavigate } from "react-router-dom";
import { useLeads, useLeadAction } from "../lib/queries.js";
import { Spinner, ErrorBox } from "../components/ui.jsx";
import Icon from "../components/Icon.jsx";
import api from "../lib/api.js";
import { BUCKET_MAP } from "../lib/constants.js";

const DESC = {
  fresh: "Newly created leads not yet assigned to a valuer.",
  ro: "Leads queued for the Registered Officer / field valuer.",
  assigned: "Leads currently assigned to a field valuer for site visit.",
  reassigned: "Leads re-routed to a different valuer.",
  ro_confirmation: "Site visit done — awaiting RO confirmation of findings.",
  qc: "Under quality-control review before pricing.",
  qc_hold: "Flagged by QC — on hold pending correction.",
  pricing: "Awaiting valuation fee / pricing sign-off.",
  completed: "Verified, issued and available for download.",
  out_of_tat: "Breached turnaround time — needs escalation.",
  duplicate: "Detected as duplicate of an existing case.",
  rejected: "Rejected leads with recorded reason.",
};

const TYPE_TAG = { property: ["#EAF2FB", "#1F5FAE"], plot: ["#FBF2DD", "#C7890F"], agri: ["#E7F6EE", "#1E9D5B"] };
function TypeTag({ l }) {
  const c = TYPE_TAG[l.type] || TYPE_TAG.property;
  return <span className="type-tag" style={{ background: c[0], color: c[1] }}>{l.ptype}</span>;
}
function StatusPill({ l }) {
  const m = BUCKET_MAP[l.stage];
  return <span className={`pill pill-${m?.tone || "info"}`}><span className="pdot" />{m?.status || l.stage}</span>;
}
function TatBar({ l }) {
  const st = l.tatState === "over" ? "over" : l.tatState === "warn" ? "warn" : "ok";
  const lab = st === "over" ? "Overdue" : st === "warn" ? `${Math.max(0, 100 - l.tatPct)}h left` : "On track";
  return (
    <div className={`tat ${st}`}>
      <span className="tt-lab">{lab}</span>
      <div className="tt-bar"><i style={{ width: `${Math.min(100, l.tatPct)}%` }} /></div>
    </div>
  );
}
function Cust({ l }) {
  return (
    <div className="cust">
      <span className="phone">{l.contact || l.pin || "—"}</span>
      <div className="nm">{l.applicant}</div>
      <div className="pin">PIN {l.pin || "—"}</div>
    </div>
  );
}
function BankExec({ l }) {
  return (
    <div style={{ fontSize: 11.5 }}>
      <b style={{ fontWeight: 600, color: "var(--navy)" }}>{l.exec || "—"}</b>
      <div className="muted">{l.execPhone}</div>
      <div className="muted" style={{ fontSize: 10.5 }}>{l.execEmail}</div>
    </div>
  );
}

const COLDEF = {
  cust: ["Customer Details", (l) => <td key="cust"><Cust l={l} /></td>],
  caseno: ["Case No.", (l) => <td key="caseno"><span className="mono" style={{ fontSize: 11.5 }}>{l.reqId}</span></td>],
  loanno: ["Loan / Prospect No.", (l) => <td key="loanno"><span className="mono" style={{ fontSize: 11.5 }}>{l.loanNo || "—"}</span></td>],
  leaddate: ["Lead Date", (l) => <td key="leaddate" style={{ fontSize: 12.5, whiteSpace: "nowrap" }}>{l.leadDate || "—"}</td>],
  source: ["Source", (l) => <td key="source"><span className="pill pill-slate" style={{ fontWeight: 600 }}>{l.source || "—"}</span></td>],
  ptype: ["Property Type", (l) => <td key="ptype"><TypeTag l={l} /></td>],
  bank: ["Client / Bank", (l) => <td key="bank" style={{ fontSize: 12.5, maxWidth: 150 }}>{l.lender}<div className="muted" style={{ fontSize: 11 }}>{l.branch}</div></td>],
  bankexec: ["Bank Executive", (l) => <td key="bankexec"><BankExec l={l} /></td>],
  rocompany: ["RO Company", (l) => <td key="rocompany" style={{ fontSize: 12.5 }}>{l.roCompany || "—"}</td>],
  valuer: ["Valuer", (l) => <td key="valuer" style={{ fontSize: 12.5 }}>{l.valuator || <span className="muted">Unassigned</span>}</td>],
  remarks: ["Remarks", (l) => <td key="remarks" style={{ fontSize: 12, maxWidth: 170 }}><div className="muted" style={{ lineHeight: 1.4 }}>{l.remarks || "—"}</div></td>],
  holdremarks: ["Hold Remarks", (l) => <td key="holdremarks" style={{ fontSize: 12, maxWidth: 200 }}><div style={{ color: "var(--poor)", lineHeight: 1.4 }}>{l.holdRemarks || "—"}</div></td>],
  location: ["Location", (l) => <td key="location" style={{ fontSize: 12 }}>{l.location || "—"}</td>],
  tat: ["TAT", (l) => <td key="tat"><TatBar l={l} /></td>],
  status: ["Status", (l) => <td key="status"><StatusPill l={l} /></td>],
};
const STAGE_COLS = {
  fresh: ["cust", "caseno", "loanno", "leaddate", "source", "ptype", "bank", "bankexec", "tat"],
  ro: ["cust", "caseno", "loanno", "leaddate", "source", "rocompany", "ptype", "bank", "bankexec", "tat"],
  assigned: ["cust", "caseno", "loanno", "leaddate", "source", "valuer", "remarks", "bankexec", "ptype", "status", "tat"],
  reassigned: ["cust", "caseno", "loanno", "leaddate", "source", "valuer", "remarks", "bankexec", "ptype", "status", "tat"],
  qc_hold: ["cust", "caseno", "ptype", "bank", "valuer", "holdremarks", "status", "tat"],
  completed: ["cust", "caseno", "ptype", "bank", "location", "tat", "status"],
  _default: ["cust", "caseno", "ptype", "bank", "valuer", "location", "tat", "status"],
};
const colsFor = (b) => STAGE_COLS[b] || STAGE_COLS._default;

export default function Leads() {
  const [params, setParams] = useSearchParams();
  const navigate = useNavigate();
  const bucket = params.get("bucket") || "assigned";
  const q = params.get("q") || "";
  const [search, setSearch] = useState(q);
  useEffect(() => setSearch(q), [q]);

  const { data, isLoading, error, isFetching } = useLeads(bucket, q);
  const action = useLeadAction();
  const meta = BUCKET_MAP[bucket];
  const cols = colsFor(bucket);

  function applySearch(e) {
    e.preventDefault();
    const next = { bucket };
    if (search.trim()) next.q = search.trim();
    setParams(next);
  }

  function reassign(l) {
    const v = window.prompt("Reassign to valuator (name):", l.valuator || "");
    if (v) action.mutate({ id: l.id, body: { action: "reassign", valuator: v } });
  }
  function reject(l) {
    if (window.confirm(`Reject lead ${l.reqId}?`)) action.mutate({ id: l.id, body: { action: "reject" } });
  }
  async function download(l) {
    try {
      const res = await api.get(`/leads/${l.id}/report.pdf`, { responseType: "blob" });
      const url = URL.createObjectURL(res.data); window.open(url, "_blank");
      setTimeout(() => URL.revokeObjectURL(url), 60_000);
    } catch (e) { alert(e?.error || "Report not available."); }
  }

  return (
    <>
      <div className="crumbs"><a onClick={() => navigate("/")}>Home</a><span className="sep">/</span><span>Lead Buckets</span><span className="sep">/</span><span style={{ color: "var(--navy)" }}>{meta?.label}</span></div>
      <div className="page-head">
        <div><h1>{meta?.label || "Leads"}</h1><div className="sub">{DESC[bucket] || ""}</div></div>
        <button className="btn btn-primary" onClick={() => navigate("/leads/new")}><Icon name="plus" size={16} />Create New Lead</button>
      </div>

      <div className="card">
        <div className="table-toolbar" style={{ display: "flex", gap: 12, alignItems: "center", padding: "14px 18px", borderBottom: "1px solid var(--line)", flexWrap: "wrap" }}>
          <form className="tt-search" onSubmit={applySearch}>
            <input placeholder="Search all columns…" value={search} onChange={(e) => setSearch(e.target.value)} />
          </form>
          <button type="button" className="filter-chip"><Icon name="layers" />Asset Type</button>
          <button type="button" className="filter-chip"><Icon name="bank" />Bank</button>
          <span style={{ marginLeft: "auto", fontSize: 12, color: "var(--slate-500)", fontWeight: 600 }}>
            {data ? `${data.total.toLocaleString("en-IN")} total · showing ${data.rows.length}` : "…"}{isFetching && " · refreshing"}
          </span>
        </div>

        {isLoading ? <Spinner label="Loading leads…" /> : error ? <div style={{ padding: 16 }}><ErrorBox error={error} /></div> : (
          <>
            <div style={{ overflowX: "auto" }}>
              <table className="table">
                <thead><tr>{cols.map((k) => <th key={k}>{COLDEF[k][0]}</th>)}<th style={{ textAlign: "right" }}>Actions</th></tr></thead>
                <tbody>
                  {data.rows.length === 0 ? (
                    <tr><td colSpan={cols.length + 1}><div className="empty-state"><Icon name="buckets" size={38} /><h3>No leads in this bucket</h3><div>Leads will appear here as they reach the “{meta?.label}” stage.</div></div></td></tr>
                  ) : data.rows.map((l) => {
                    const done = l.stage === "completed", terminal = done || l.stage === "rejected";
                    return (
                      <tr key={l.id} style={{ cursor: "default" }}>
                        {cols.map((k) => COLDEF[k][1](l))}
                        <td>
                          <div className="row-actions">
                            <button className="act" title="View" onClick={() => navigate(`/leads/${l.id}`)}><Icon name="view" /></button>
                            <button className="act" title="Edit" onClick={() => navigate(`/leads/${l.id}/edit`)}><Icon name="reassign" /></button>
                            {!terminal && <button className="act" title="Reassign" onClick={() => reassign(l)}><Icon name="reassign" /></button>}
                            {!terminal && <button className="act reject" title="Reject" onClick={() => reject(l)}><Icon name="reject" /></button>}
                            <button className="act" title={done ? "Download" : "Available when completed"} disabled={!done} onClick={() => download(l)}><Icon name="doc" /></button>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
            <div className="table-foot">
              <span>Page 1 of {Math.max(1, Math.ceil((data.total || 1) / 50))}</span>
              <div className="pager"><button>‹</button><button className="on">1</button><button>›</button></div>
            </div>
          </>
        )}
      </div>
    </>
  );
}
