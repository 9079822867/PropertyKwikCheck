import { useState } from "react";
import { useLeadAction, useMasterItems } from "../lib/queries.js";
import Icon from "./Icon.jsx";

const REJECT_REASONS = ["Insufficient documents", "Property not traceable", "Duplicate request", "Out of service area", "Customer not reachable", "Bank withdrew request"];
const NOTE_TYPES = ["Internal note", "Bank communication", "Site observation", "Follow-up reminder"];

function Shell({ icon, tone, title, sub, onClose, children, foot }) {
  const tones = { good: ["#E7F6EE", "#1E9D5B"], poor: ["#FBE7E7", "#D33B3B"], fair: ["#FBF2DD", "#C7890F"], blue: ["#EAF2FB", "#1F5FAE"] };
  const c = tones[tone] || tones.blue;
  return (
    <div className="overlay" onClick={(e) => { if (e.target.classList.contains("overlay")) onClose(); }}>
      <div className="modal">
        <div className="modal-head">
          <div className="mh-ic" style={{ background: c[0], color: c[1] }}><Icon name={icon} /></div>
          <div><h3>{title}</h3><p>{sub}</p></div>
          <button className="x" onClick={onClose}><Icon name="x" /></button>
        </div>
        <div className="modal-body">{children}</div>
        <div className="modal-foot">{foot}</div>
      </div>
    </div>
  );
}

export default function LeadActionModals({ modal, onClose }) {
  const action = useLeadAction();
  const isReassign = modal?.type === "reassign";
  const { data: valuers } = useMasterItems("valuers", isReassign);

  const [noteText, setNoteText] = useState("");
  const [valuer, setValuer] = useState(modal?.lead?.valuator || "");

  if (!modal) return null;
  const l = modal.lead;
  const sub = `${l.applicant} · ${l.reqId}`;
  const done = () => { onClose(); };

  if (modal.type === "note") {
    return (
      <Shell icon="note" tone="fair" title="Add Note" sub={sub} onClose={onClose}
        foot={<>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button className="btn btn-primary" disabled={action.isPending}
            onClick={() => action.mutate({ id: l.id, body: { remarks: noteText } }, { onSuccess: done })}>
            <Icon name="check" size={16} />Save note
          </button>
        </>}>
        <div className="field"><label>Note type</label><select>{NOTE_TYPES.map((t) => <option key={t}>{t}</option>)}</select></div>
        <div className="field"><label>Note</label><textarea autoFocus placeholder="Type your note for this lead…" value={noteText} onChange={(e) => setNoteText(e.target.value)} /></div>
      </Shell>
    );
  }

  if (modal.type === "reassign") {
    return (
      <Shell icon="reassign" tone="good" title="Reassign Lead" sub={sub} onClose={onClose}
        foot={<>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button className="btn btn-primary" disabled={action.isPending || !valuer}
            onClick={() => action.mutate({ id: l.id, body: { action: "reassign", valuator: valuer } }, { onSuccess: done })}>
            <Icon name="check" size={16} />Reassign
          </button>
        </>}>
        <div className="field"><label>Reassign to valuer</label>
          <select value={valuer} onChange={(e) => setValuer(e.target.value)}>
            <option value="">— select valuer —</option>
            {(valuers || []).map((v) => <option key={v.id} value={v.value}>{v.value}</option>)}
          </select>
        </div>
        <div className="field"><label>Reason</label><select><option>Workload balancing</option><option>Geographic proximity</option><option>Specialisation match</option><option>Original valuer unavailable</option></select></div>
        <div className="field"><label>Note (optional)</label><textarea placeholder="Add a handover note…" /></div>
      </Shell>
    );
  }

  if (modal.type === "reject") {
    return (
      <Shell icon="reject" tone="poor" title="Reject Lead" sub={sub} onClose={onClose}
        foot={<>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button className="btn btn-danger" disabled={action.isPending}
            onClick={() => action.mutate({ id: l.id, body: { action: "reject" } }, { onSuccess: done })}>
            <Icon name="reject" size={16} />Reject lead
          </button>
        </>}>
        <div className="field"><label>Rejection reason *</label><select>{REJECT_REASONS.map((r) => <option key={r}>{r}</option>)}</select></div>
        <div className="field"><label>Remark</label><textarea placeholder="Explain the rejection for the audit trail…" /></div>
      </Shell>
    );
  }

  if (modal.type === "delete") {
    return (
      <Shell icon="del" tone="poor" title="Delete Lead" sub="This cannot be undone" onClose={onClose}
        foot={<>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button className="btn btn-danger" disabled={action.isPending}
            onClick={() => action.mutate({ id: l.id, method: "delete" }, { onSuccess: done })}>
            <Icon name="del" size={16} />Delete permanently
          </button>
        </>}>
        <p style={{ margin: 0, color: "var(--slate-600)", fontSize: 13.5, lineHeight: 1.6 }}>
          You are about to delete <b>{l.reqId}</b> — {l.applicant}. The record and its audit trail will be removed.
        </p>
      </Shell>
    );
  }

  return null;
}
