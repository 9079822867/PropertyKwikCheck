import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useCreateLead } from "../lib/queries.js";
import { ErrorBox } from "../components/ui.jsx";

const PTYPES = ["Residential", "Commercial", "Industrial", "Plot", "Agricultural Land"];
const SOURCES = ["Bank Portal", "DSA Referral", "Branch Walk-in", "Direct Customer", "API Integration", "Tele-calling"];

export default function NewLead() {
  const navigate = useNavigate();
  const create = useCreateLead();
  const [err, setErr] = useState(null);
  const [form, setForm] = useState({
    ptype: "Residential", applicant: "", lender: "", branch: "",
    loanNo: "", contact: "", source: "Bank Portal", leadDate: "",
  });
  const set = (k, v) => setForm((f) => ({ ...f, [k]: v }));
  const inp = { width: "100%", height: 40, border: "1px solid var(--line)", borderRadius: 8, padding: "0 11px", background: "var(--slate-50)" };

  async function submit(e) {
    e.preventDefault(); setErr(null);
    const { ptype, ...data } = form;
    try {
      const lead = await create.mutateAsync({ ptype, data });
      navigate(`/leads/${lead.id}`);
    } catch (e2) { setErr(e2); }
  }

  return (
    <>
      <div className="page-head">
        <div>
          <button className="btn btn-ghost btn-sm" onClick={() => navigate(-1)} style={{ marginBottom: 8 }}>← Back</button>
          <h1>New Lead</h1>
          <div className="sub">Create a fresh valuation request.</div>
        </div>
      </div>

      <form className="card card-pad" onSubmit={submit} style={{ maxWidth: 760 }}>
        {err && <ErrorBox error={err} />}
        <div className="grid grid-3" style={{ gap: 14 }}>
          <div className="field" style={{ margin: 0 }}><label>Property / Asset Type</label>
            <select style={inp} value={form.ptype} onChange={(e) => set("ptype", e.target.value)}>{PTYPES.map((p) => <option key={p}>{p}</option>)}</select>
          </div>
          <div className="field" style={{ margin: 0 }}><label>Applicant Name</label><input style={inp} value={form.applicant} onChange={(e) => set("applicant", e.target.value)} required /></div>
          <div className="field" style={{ margin: 0 }}><label>Contact Number</label><input style={inp} value={form.contact} onChange={(e) => set("contact", e.target.value)} /></div>
          <div className="field" style={{ margin: 0 }}><label>Lender / Bank</label><input style={inp} value={form.lender} onChange={(e) => set("lender", e.target.value)} /></div>
          <div className="field" style={{ margin: 0 }}><label>Branch</label><input style={inp} value={form.branch} onChange={(e) => set("branch", e.target.value)} /></div>
          <div className="field" style={{ margin: 0 }}><label>Loan / Prospect No.</label><input style={inp} value={form.loanNo} onChange={(e) => set("loanNo", e.target.value)} /></div>
          <div className="field" style={{ margin: 0 }}><label>Source</label>
            <select style={inp} value={form.source} onChange={(e) => set("source", e.target.value)}>{SOURCES.map((s) => <option key={s}>{s}</option>)}</select>
          </div>
          <div className="field" style={{ margin: 0 }}><label>Lead Date</label><input type="date" style={inp} value={form.leadDate} onChange={(e) => set("leadDate", e.target.value)} /></div>
        </div>
        <button className="btn btn-primary" style={{ marginTop: 18 }} disabled={create.isPending}>{create.isPending ? "Creating…" : "Create Lead"}</button>
      </form>
    </>
  );
}
