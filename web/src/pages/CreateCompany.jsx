import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useCreateCompany } from "../lib/queries.js";
import { ErrorBox } from "../components/ui.jsx";
import Icon from "../components/Icon.jsx";

const TYPES = ["Lender · Bank", "Lender · Co-op Bank", "Lender · NBFC", "Valuation Agency · Owner", "Channel Partner"];
const inp = { width: "100%", height: 40, border: "1px solid var(--line)", borderRadius: 8, padding: "0 11px", background: "var(--slate-50)", fontSize: 13.5 };

const blank = { name: "", type: TYPES[0], gstin: "", pan: "", spoc: "", spocEmail: "", spocPhone: "", defaultTat: "", address: "", status: "Active" };

export default function CreateCompany() {
  const navigate = useNavigate();
  const create = useCreateCompany();
  const [form, setForm] = useState(blank);
  const [err, setErr] = useState(null);
  const set = (k, v) => setForm((f) => ({ ...f, [k]: v }));

  async function submit() {
    setErr(null);
    if (!form.name) { setErr({ error: "Company name is required." }); return; }
    try {
      // Backend persists name/type/spoc/status; extra fields are sent for forward-compat.
      await create.mutateAsync({
        name: form.name, type: form.type, spoc: form.spoc, status: form.status,
        gstin: form.gstin, pan: form.pan, spocEmail: form.spocEmail, spocPhone: form.spocPhone,
        defaultTat: form.defaultTat, address: form.address,
      });
      navigate("/companies");
    } catch (e) { setErr(e); }
  }

  const Field = ({ label, k, type = "text", col2 }) => (
    <div className={`field ${col2 ? "col2" : ""}`} style={{ margin: 0 }}>
      <label>{label}</label>
      <input type={type} style={inp} value={form[k]} onChange={(e) => set(k, e.target.value)} />
    </div>
  );

  return (
    <>
      <div className="crumbs"><a onClick={() => navigate("/")}>Home</a><span className="sep">/</span><span>Company</span><span className="sep">/</span><span style={{ color: "var(--navy)" }}>Create Company</span></div>
      <div className="page-head">
        <div><h1>Create Company</h1><div className="sub">Onboard a new lender or partner.</div></div>
        <button className="btn btn-ghost" onClick={() => navigate("/companies")}>Back</button>
      </div>

      {err && <ErrorBox error={err} />}

      <div className="card card-pad" style={{ maxWidth: 860 }}>
        <div className="fs-title"><div className="n">1</div><h3>Company Details</h3></div>
        <div className="fs-sub">Billing entity and point of contact.</div>
        <div className="g2">
          <Field label="Company Name" k="name" />
          <div className="field" style={{ margin: 0 }}><label>Type</label>
            <select style={inp} value={form.type} onChange={(e) => set("type", e.target.value)}>{TYPES.map((t) => <option key={t}>{t}</option>)}</select>
          </div>
          <Field label="GSTIN" k="gstin" />
          <Field label="PAN" k="pan" />
          <Field label="SPOC Name" k="spoc" />
          <Field label="SPOC Email" k="spocEmail" type="email" />
          <Field label="SPOC Phone" k="spocPhone" />
          <Field label="Default TAT (days)" k="defaultTat" type="number" />
          <div className="field col2" style={{ margin: 0 }}>
            <label>Registered Address</label>
            <textarea value={form.address} onChange={(e) => set("address", e.target.value)} rows={3}
              style={{ ...inp, height: "auto", padding: "9px 11px", resize: "vertical" }} placeholder="Company address…" />
          </div>
          <div className="field col2" style={{ margin: 0 }}>
            <label>Status</label>
            <div className="seg">
              <button type="button" className={form.status === "Active" ? "on" : ""} onClick={() => set("status", "Active")}>Active</button>
              <button type="button" className={form.status === "Inactive" ? "on" : ""} onClick={() => set("status", "Inactive")}>Inactive</button>
            </div>
          </div>
        </div>
      </div>

      <div className="wizard-foot">
        <button className="btn btn-ghost" onClick={() => navigate("/companies")}>Cancel</button>
        <button className="btn btn-primary" disabled={create.isPending} onClick={submit}><Icon name="check" size={16} />{create.isPending ? "Creating…" : "Create Company"}</button>
      </div>
    </>
  );
}
