import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useCreateCompany } from "../lib/queries.js";
import { ErrorBox } from "../components/ui.jsx";

const TYPES = ["Lender · Bank", "Lender · Co-op Bank", "Lender · NBFC", "Valuation Agency · Owner", "Channel Partner"];
const inp = { width: "100%", height: 40, border: "1px solid var(--line)", borderRadius: 8, padding: "0 11px", background: "var(--slate-50)", fontSize: 13.5 };

export default function CreateCompany() {
  const navigate = useNavigate();
  const create = useCreateCompany();
  const [form, setForm] = useState({ name: "", type: TYPES[0], spoc: "", status: "Active" });
  const [err, setErr] = useState(null);
  const set = (k, v) => setForm((f) => ({ ...f, [k]: v }));

  async function submit() {
    setErr(null);
    if (!form.name) { setErr({ error: "Company name is required." }); return; }
    try { await create.mutateAsync(form); navigate("/companies"); } catch (e) { setErr(e); }
  }

  return (
    <>
      <div className="crumbs"><a onClick={() => navigate("/")}>Home</a><span className="sep">/</span><span>Company</span><span className="sep">/</span><span style={{ color: "var(--navy)" }}>Create Company</span></div>
      <div className="page-head"><div><h1>Create Company</h1><div className="sub">Onboard a new lender or partner.</div></div>
        <button className="btn btn-ghost" onClick={() => navigate("/companies")}>Back</button></div>

      {err && <ErrorBox error={err} />}

      <div className="card card-pad" style={{ maxWidth: 720 }}>
        <div className="fs-title"><div className="n">1</div><h3>Company Details</h3></div>
        <div className="fs-sub">Billing entity and point of contact.</div>
        <div className="g2">
          <div className="field" style={{ margin: 0 }}><label>Company Name</label><input style={inp} value={form.name} onChange={(e) => set("name", e.target.value)} /></div>
          <div className="field" style={{ margin: 0 }}><label>Type</label><select style={inp} value={form.type} onChange={(e) => set("type", e.target.value)}>{TYPES.map((t) => <option key={t}>{t}</option>)}</select></div>
          <div className="field col2" style={{ margin: 0 }}><label>SPOC Name</label><input style={inp} value={form.spoc} onChange={(e) => set("spoc", e.target.value)} /></div>
        </div>
        <div className="wizard-foot">
          <button className="btn btn-ghost" onClick={() => navigate("/companies")}>Cancel</button>
          <button className="btn btn-primary" disabled={create.isPending} onClick={submit}>{create.isPending ? "Creating…" : "Create Company"}</button>
        </div>
      </div>
    </>
  );
}
