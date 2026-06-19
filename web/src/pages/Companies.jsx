import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useCompanies } from "../lib/queries.js";
import { Spinner, ErrorBox } from "../components/ui.jsx";
import Icon from "../components/Icon.jsx";
import { toast } from "../lib/toast.js";

export default function Companies() {
  const navigate = useNavigate();
  const { data: companies, isLoading, error } = useCompanies();
  const [q, setQ] = useState("");
  const rows = (companies || []).filter((c) => !q || `${c.name} ${c.type} ${c.spoc}`.toLowerCase().includes(q.toLowerCase()));

  return (
    <>
      <div className="crumbs"><a onClick={() => navigate("/")}>Home</a><span className="sep">/</span><span>Company</span><span className="sep">/</span><span style={{ color: "var(--navy)" }}>All Companies</span></div>
      <div className="page-head"><div><h1>All Companies</h1><div className="sub">Lenders, NBFCs and partners who raise valuation requests.</div></div></div>

      <div className="card">
        <div className="table-toolbar" style={{ display: "flex", gap: 12, alignItems: "center", padding: "14px 18px", borderBottom: "1px solid var(--line)" }}>
          <div className="tt-search"><input placeholder="Search companies…" value={q} onChange={(e) => setQ(e.target.value)} /></div>
          <span style={{ marginLeft: "auto" }}><button className="btn btn-primary btn-sm" onClick={() => navigate("/companies/new")}><Icon name="plus" size={14} />Create Company</button></span>
        </div>
        {isLoading ? <Spinner /> : error ? <div style={{ padding: 16 }}><ErrorBox error={error} /></div> : (
          <table className="table">
            <thead><tr><th>Company</th><th>Type</th><th>SPOC</th><th style={{ textAlign: "center" }}>Total Leads</th><th style={{ textAlign: "center" }}>Active</th><th>Status</th><th style={{ textAlign: "right" }}>Actions</th></tr></thead>
            <tbody>
              {rows.map((c) => (
                <tr key={c.id} style={{ cursor: "default" }}>
                  <td><div style={{ display: "flex", alignItems: "center", gap: 10 }}><div className="co-ic"><Icon name="bank" /></div><div style={{ fontWeight: 700, color: "var(--navy)", fontSize: 13 }}>{c.name}</div></div></td>
                  <td style={{ fontSize: 12.5 }}>{c.type}</td><td style={{ fontSize: 12.5 }}>{c.spoc || "—"}</td>
                  <td style={{ textAlign: "center" }} className="mono">{c.leads}</td><td style={{ textAlign: "center" }} className="mono">{c.active}</td>
                  <td><span className={`pill ${c.status === "Active" ? "pill-good" : "pill-slate"}`}><span className="pdot" />{c.status}</span></td>
                  <td><div className="row-actions">
                    <button className="act" title="View" onClick={() => toast(`Open ${c.name}`)}><Icon name="view" /></button>
                    <button className="act" title="Edit" onClick={() => navigate(`/companies/${c.id}/edit`)}><Icon name="note" /></button>
                  </div></td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </>
  );
}
