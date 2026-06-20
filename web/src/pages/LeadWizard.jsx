import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useLead, useUpdateLead, usePhotos, useUploadPhoto, useDeletePhoto, useStatusTypes } from "../lib/queries.js";
import { STAGES, stageFields, technicalSchema } from "../lib/wizardSchema.js";
import { PhotoFrames } from "../lib/photoFrames.js";
import WizardField from "../components/WizardField.jsx";
import AuthImage from "../components/AuthImage.jsx";
import { Spinner, ErrorBox, Pill } from "../components/ui.jsx";

const RATINGS = [["good", "Good"], ["fair", "Fair"], ["avg", "Average"], ["poor", "Poor"]];

// The primary forward edge out of each stage — drives the "move to next stage" action.
// Assigned/Reassigned advance to RO Confirmation; the rest follow the linear pipeline.
const NEXT_STAGE = {
  fresh: "assigned",
  ro: "assigned",
  assigned: "ro_confirmation",
  reassigned: "ro_confirmation",
  ro_confirmation: "qc",
  qc: "pricing",
  qc_hold: "qc",
  pricing: "completed",
};

function stageKeys(stage, family) {
  if (stage === "technical") {
    const t = technicalSchema(family);
    return [...t.scores.map((s) => s[0]), ...t.groups.flatMap((g) => g[1].map((i) => i[0]))];
  }
  const keys = [];
  for (const fl of stageFields(stage, family)) {
    keys.push(fl.k);
    if (fl.t === "textrate") keys.push(`${fl.k}_r`);
  }
  return keys;
}

export default function LeadWizard() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { data: lead, isLoading, error } = useLead(id);
  const { data: statusTypes } = useStatusTypes();
  const update = useUpdateLead(id);

  const [active, setActive] = useState(0);
  const [data, setData] = useState({});
  const [msg, setMsg] = useState(null);

  useEffect(() => {
    if (lead?.data) setData({ ...lead.data });
  }, [lead]);

  if (isLoading) return <Spinner label="Loading report…" />;
  if (error) return <ErrorBox error={error} />;

  const family = lead.type;
  // Fresh leads only capture the intake / lead-table data; the deeper report stages
  // unlock once the lead is assigned to a valuator.
  const visibleStages = lead.stage === "fresh" ? STAGES.slice(0, 1) : STAGES;
  const activeIdx = Math.min(active, visibleStages.length - 1);
  const stage = visibleStages[activeIdx];
  const onChange = (k, v) => setData((d) => ({ ...d, [k]: v }));

  const nextCode = NEXT_STAGE[lead.stage];
  const nextLabel = (statusTypes || []).find((s) => s.code === nextCode)?.label || nextCode;

  async function save(advance) {
    const keys = stageKeys(stage.key, family);
    const subset = {};
    for (const k of keys) if (data[k] !== undefined && data[k] !== "") subset[k] = data[k];
    try {
      await update.mutateAsync({ data: subset });
      setMsg(`Saved ${stage.label}.`);
      if (advance && activeIdx < visibleStages.length - 1) setActive(activeIdx + 1);
    } catch (e) {
      setMsg(e?.error || "Save failed.");
    }
  }

  async function moveNext() {
    if (!nextCode) return;
    try {
      await update.mutateAsync({ stage: nextCode });
      setMsg(`Moved to ${nextLabel}.`);
    } catch (e) {
      setMsg(e?.error || "Stage change not allowed.");
    }
  }

  return (
    <>
      <div className="page-head">
        <div>
          <button className="btn btn-ghost btn-sm" onClick={() => navigate(`/leads/${id}`)} style={{ marginBottom: 8 }}>← Back to lead</button>
          <h1>Edit Report</h1>
          <div className="sub"><span className="mono">{lead.reqId}</span> · {lead.applicant} · {lead.ptype}</div>
        </div>
        <Pill tone="info">{lead.reportStatus}</Pill>
      </div>

      {/* stepper */}
      <div className="card card-pad" style={{ display: "flex", gap: 8, marginBottom: 16, flexWrap: "wrap", alignItems: "center" }}>
        {visibleStages.map((s, i) => (
          <button key={s.key} onClick={() => setActive(i)}
            className={`btn btn-sm ${i === activeIdx ? "btn-primary" : "btn-ghost"}`}>
            {i + 1}. {s.label}
          </button>
        ))}
        {nextCode && (
          <button className="btn btn-sm btn-soft" style={{ marginLeft: "auto" }}
            disabled={update.isPending} onClick={moveNext} title={`Move this lead to ${nextLabel}`}>
            Move to next stage → {nextLabel}
          </button>
        )}
      </div>

      {msg && <div className="error-banner" style={{ background: "var(--good-bg)", color: "var(--good)" }}>{msg}</div>}

      <div className="card card-pad">
        {stage.key === "technical" ? (
          <TechnicalStage family={family} data={data} onChange={onChange} />
        ) : stage.key === "photos" ? (
          <PhotosStage leadId={id} family={family} />
        ) : (
          <div className="grid grid-3" style={{ gap: 16 }}>
            {stageFields(stage.key, family).map((fl) => (
              <WizardField key={fl.k} field={fl} data={data} onChange={onChange} />
            ))}
          </div>
        )}

        {stage.key !== "photos" && (
          <div style={{ display: "flex", gap: 10, marginTop: 20 }}>
            <button className="btn btn-primary" disabled={update.isPending} onClick={() => save(true)}>
              {update.isPending ? "Saving…" : "Save & Next"}
            </button>
            <button className="btn btn-ghost" disabled={update.isPending} onClick={() => save(false)}>Save</button>
          </div>
        )}
      </div>
    </>
  );
}

function TechnicalStage({ family, data, onChange }) {
  const schema = technicalSchema(family);
  const num = { width: "100%", height: 38, border: "1px solid var(--line)", borderRadius: 8, padding: "0 11px", background: "var(--slate-50)" };
  return (
    <>
      <h3 style={{ marginTop: 0, color: "var(--navy)" }}>Score Cards (0–10)</h3>
      <div className="grid grid-4" style={{ marginBottom: 20 }}>
        {schema.scores.map(([k, label]) => (
          <div className="field" key={k} style={{ marginBottom: 0 }}>
            <label>{label}</label>
            <input type="number" min="0" max="10" step="0.5" style={num}
              value={data[k] ?? ""} onChange={(e) => onChange(k, e.target.value)} />
          </div>
        ))}
      </div>
      {schema.groups.map(([title, items]) => (
        <div key={title} style={{ marginBottom: 16 }}>
          <div className="eyebrow" style={{ marginBottom: 8 }}>{title}</div>
          <div className="grid grid-4">
            {items.map(([k, label]) => (
              <div className="field" key={k} style={{ marginBottom: 0 }}>
                <label>{label}</label>
                <select style={num} value={data[k] ?? ""} onChange={(e) => onChange(k, e.target.value)}>
                  <option value="">— rate —</option>
                  {RATINGS.map(([v, l]) => <option key={v} value={v}>{l}</option>)}
                </select>
              </div>
            ))}
          </div>
        </div>
      ))}
    </>
  );
}

function PhotosStage({ leadId, family }) {
  const { data: photos, isLoading } = usePhotos(leadId);
  const upload = useUploadPhoto(leadId);
  const remove = useDeletePhoto(leadId);
  const frames = PhotoFrames[family]?.photos ?? [];

  if (isLoading) return <Spinner label="Loading photos…" />;
  const byFrame = Object.fromEntries((photos ?? []).map((p) => [p.frameLabel, p]));

  return (
    <>
      <h3 style={{ marginTop: 0, color: "var(--navy)" }}>Site Photographs</h3>
      <div className="grid grid-4">
        {frames.map((frame) => {
          const existing = byFrame[frame];
          return (
            <div key={frame} className="card card-pad" style={{ padding: 12 }}>
              <div style={{ fontSize: 12, fontWeight: 600, marginBottom: 8, color: "var(--navy)" }}>{frame}</div>
              {existing ? (
                <div>
                  <AuthImage path={`/photos/${existing.id}/download`} alt={frame}
                    style={{ width: "100%", height: 90, objectFit: "cover", borderRadius: 6, marginBottom: 6, display: "block" }} />
                  <button className="btn btn-danger btn-sm" onClick={() => remove.mutate(existing.id)}>Remove</button>
                </div>
              ) : (
                <input type="file" accept="image/jpeg,image/png" style={{ fontSize: 11 }}
                  onChange={(e) => {
                    const file = e.target.files?.[0];
                    if (file) upload.mutate({ file, frameLabel: frame, kind: "photo" });
                  }} />
              )}
            </div>
          );
        })}
      </div>
      {upload.isError && <ErrorBox error={upload.error} />}
    </>
  );
}
