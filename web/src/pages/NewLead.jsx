import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useCreateLead } from "../lib/queries.js";
import { ASSET_META, createIntakeSections } from "../lib/wizardSchema.js";
import WizardField from "../components/WizardField.jsx";
import Icon from "../components/Icon.jsx";
import { ErrorBox } from "../components/ui.jsx";

// Asset types → family + reqId prefix + report label (mirrors backend §16.2).
const PTYPES = Object.entries(ASSET_META).map(([ptype, m]) => ({ ptype, ...m }));

export default function NewLead() {
  const navigate = useNavigate();
  const create = useCreateLead();
  const [ptype, setPtype] = useState("Residential");
  const [data, setData] = useState({});
  const [err, setErr] = useState(null);

  const meta = ASSET_META[ptype];
  const sections = useMemo(() => createIntakeSections(ptype), [ptype]);
  // Report Type + Property / Asset Type default from the chosen asset type but remain editable
  // (Residential/Commercial/Industrial → Property Inspection, Plot → Plot Valuation, Agri → Agri Land Valuation).
  useEffect(() => {
    setData((d) => ({ ...d, reportType: meta.report, propertyType: ptype }));
  }, [ptype]); // eslint-disable-line react-hooks/exhaustive-deps
  const view = data;
  const onChange = (k, v) => setData((d) => ({ ...d, [k]: v }));

  async function submit(openEditor) {
    setErr(null);
    // leadId is a display-only preview; the backend assigns the real reqId.
    const { leadId, ...rest } = data;
    const payload = { ...rest, reportType: data.reportType || meta.report, propertyType: data.propertyType || ptype };
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
                <div><div style={{ fontWeight: 700, color: "var(--navy)", fontSize: 13.5 }}>{p.ptype}</div><div style={{ fontSize: 11, color: "var(--slate-500)" }}>{p.prefix} · {p.report}</div></div>
                <div style={{ marginLeft: "auto", width: 20, height: 20, borderRadius: "50%", border: `2px solid ${on ? "var(--blue)" : "var(--slate-300)"}`, display: "grid", placeItems: "center" }}>
                  {on && <div style={{ width: 10, height: 10, borderRadius: "50%", background: "var(--blue)" }} />}
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {sections.map((sec, i) => (
        <div className="card card-pad" key={sec.t} style={{ marginBottom: 18 }}>
          <div className="fs-title"><div className="n">{i + 1}</div><h3>{sec.t}</h3></div>
          <div className="fs-sub">{sec.s}</div>
          <div className="grid" style={{ gridTemplateColumns: `repeat(${sec.c}, 1fr)`, gap: 16 }}>
            {sec.f.map((f) => (
              <div key={f.k} style={{ gridColumn: f.c ? `span ${Math.min(f.c, sec.c)}` : "auto" }}>
                <WizardField field={f} data={view} onChange={onChange} />
              </div>
            ))}
          </div>
        </div>
      ))}

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
