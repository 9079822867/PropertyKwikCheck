import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useCreateLead } from "../lib/queries.js";
import { stageFields } from "../lib/wizardSchema.js";
import WizardField from "../components/WizardField.jsx";
import Icon from "../components/Icon.jsx";
import { ErrorBox } from "../components/ui.jsx";

// Asset types → family + reqId prefix + report label (mirrors backend §16.2).
const PTYPES = [
  { ptype: "Residential", family: "property", prefix: "KC-RESI", rep: "Property Inspection", icon: "building" },
  { ptype: "Commercial", family: "property", prefix: "KC-COMM", rep: "Property Inspection", icon: "building" },
  { ptype: "Industrial", family: "property", prefix: "KC-IND", rep: "Property Inspection", icon: "building" },
  { ptype: "Plot", family: "plot", prefix: "KC-PLOT", rep: "Plot Valuation", icon: "map" },
  { ptype: "Agricultural Land", family: "agri", prefix: "KC-AGRI", rep: "Agri Land Valuation", icon: "map" },
];

export default function NewLead() {
  const navigate = useNavigate();
  const create = useCreateLead();
  const [ptype, setPtype] = useState("Residential");
  const [data, setData] = useState({});
  const [err, setErr] = useState(null);

  const meta = PTYPES.find((p) => p.ptype === ptype);
  const onChange = (k, v) => setData((d) => ({ ...d, [k]: v }));

  async function submit(openEditor) {
    setErr(null);
    const payload = { ...data, reportType: meta.rep, propertyType: ptype };
    try {
      const lead = await create.mutateAsync({ ptype, data: payload });
      navigate(openEditor ? `/leads/${lead.id}/edit` : `/leads/${lead.id}`);
    } catch (e) { setErr(e); }
  }

  return (
    <>
      <div className="crumbs"><a onClick={() => navigate("/")}>Home</a><span className="sep">/</span><span>Lead Management</span><span className="sep">/</span><span style={{ color: "var(--navy)" }}>Create New Lead</span></div>
      <div className="page-head">
        <div><h1>Create New Lead</h1><div className="sub">Choose the asset type, then capture intake details — the full report is completed stage-by-stage.</div></div>
        <button className="btn btn-ghost" onClick={() => navigate("/leads?bucket=fresh")}>Cancel</button>
      </div>

      {err && <ErrorBox error={err} />}

      <div className="card card-pad" style={{ marginBottom: 18 }}>
        <div className="fs-title"><div className="n"><Icon name="layers" size={13} /></div><h3>Asset Type</h3></div>
        <div className="fs-sub">Determines which fields and which report template apply.</div>
        <div style={{ display: "flex", gap: 14, flexWrap: "wrap" }}>
          {PTYPES.map((p) => {
            const on = p.ptype === ptype;
            return (
              <div key={p.ptype} onClick={() => setPtype(p.ptype)}
                style={{ flex: "1 1 210px", border: `2px solid ${on ? "var(--blue)" : "var(--line)"}`, background: on ? "var(--blue-tint)" : "#fff", borderRadius: 13, padding: 15, cursor: "pointer", display: "flex", gap: 12, alignItems: "center" }}>
                <div style={{ width: 42, height: 42, borderRadius: 11, background: on ? "#fff" : "var(--blue-tint)", display: "grid", placeItems: "center", color: "var(--blue)", flex: "0 0 auto" }}><Icon name={p.icon} /></div>
                <div><div style={{ fontWeight: 700, color: "var(--navy)", fontSize: 13.5 }}>{p.ptype}</div><div style={{ fontSize: 11, color: "var(--slate-500)" }}>{p.prefix} · {p.rep}</div></div>
                <div style={{ marginLeft: "auto", width: 20, height: 20, borderRadius: "50%", border: `2px solid ${on ? "var(--blue)" : "var(--slate-300)"}`, display: "grid", placeItems: "center" }}>
                  {on && <div style={{ width: 10, height: 10, borderRadius: "50%", background: "var(--blue)" }} />}
                </div>
              </div>
            );
          })}
        </div>
      </div>

      <div className="card card-pad">
        <div className="fs-title"><div className="n">1</div><h3>Intake Details</h3></div>
        <div className="fs-sub">Applicant, lender and case basics. Remaining stages are filled in the report editor.</div>
        <div className="grid grid-3" style={{ gap: 16 }}>
          {stageFields("intake", meta.family).map((f) => <WizardField key={f.k} field={f} data={data} onChange={onChange} />)}
        </div>
      </div>

      <div className="wizard-foot">
        <button className="btn btn-ghost" onClick={() => navigate("/")}>Discard</button>
        <div style={{ display: "flex", gap: 10 }}>
          <button className="btn btn-soft" disabled={create.isPending} onClick={() => submit(false)}>Save as Draft</button>
          <button className="btn btn-primary" disabled={create.isPending} onClick={() => submit(true)}><Icon name="plus" size={16} />{create.isPending ? "Creating…" : "Create & Open Editor"}</button>
        </div>
      </div>
    </>
  );
}
