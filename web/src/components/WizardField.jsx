// Renders one report field by type and reports changes via onChange(key, value).
const RATINGS = [
  ["good", "Good"], ["fair", "Fair"], ["avg", "Average"], ["poor", "Poor"],
];
const SEG_YN = ["Yes", "No"];
const SEG_YNP = ["Yes", "No", "Partial"];

export default function WizardField({ field, data, onChange }) {
  const { k, label, t, options, ph, ro } = field;
  const val = data[k] ?? "";

  const inputStyle = { width: "100%", height: 38, border: "1px solid var(--line)", borderRadius: 8, padding: "0 11px", background: "var(--slate-50)", fontSize: 13.5 };
  const roStyle = ro ? { background: "var(--slate-100)", color: "var(--slate-500)", cursor: "not-allowed" } : null;
  const set = (v) => onChange(k, v);

  let control;
  if (t === "file") {
    // Cosmetic chooser (matches the prototype). Binary upload happens later via the
    // documents endpoint in the report editor; here we just capture the chosen name.
    control = (
      <label style={{ ...inputStyle, ...roStyle, display: "flex", alignItems: "center", gap: 8, cursor: "pointer", color: "var(--slate-500)", borderStyle: "dashed" }}>
        <span style={{ fontWeight: 700, color: "var(--blue)" }}>+</span>
        <span style={{ color: val ? "var(--navy)" : "var(--slate-400)", overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>{val || "Choose file…"}</span>
        <input type="file" style={{ display: "none" }} onChange={(e) => set(e.target.files?.[0]?.name ?? "")} />
      </label>
    );
  } else if (t === "ta") {
    control = <textarea value={val} onChange={(e) => set(e.target.value)} rows={3} placeholder={ph}
      style={{ ...inputStyle, height: "auto", padding: "9px 11px", resize: "vertical" }} />;
  } else if (t === "sel") {
    control = (
      <select value={val} onChange={(e) => set(e.target.value)} style={{ ...inputStyle, ...roStyle }} disabled={ro}>
        <option value="">— select —</option>
        {(options ?? []).map((o) => <option key={o} value={o}>{o}</option>)}
      </select>
    );
  } else if (t === "seg" || t === "segp") {
    const opts = t === "segp" ? SEG_YNP : SEG_YN;
    control = (
      <div style={{ display: "flex", gap: 6 }}>
        {opts.map((o) => (
          <button type="button" key={o} onClick={() => set(o)}
            className={`btn btn-sm ${val === o ? "btn-primary" : "btn-ghost"}`}>{o}</button>
        ))}
      </div>
    );
  } else if (t === "textrate") {
    const rk = `${k}_r`;
    control = (
      <div style={{ display: "flex", gap: 6 }}>
        <input value={val} onChange={(e) => set(e.target.value)} style={{ ...inputStyle, flex: 1 }} />
        <select value={data[rk] ?? ""} onChange={(e) => onChange(rk, e.target.value)} style={{ ...inputStyle, width: 120 }}>
          <option value="">rate</option>
          {RATINGS.map(([v, l]) => <option key={v} value={v}>{l}</option>)}
        </select>
      </div>
    );
  } else {
    const type = t === "date" ? "date" : t === "num" || t === "rupee" ? "number" : "text";
    control = <input type={type} value={val} onChange={(e) => set(e.target.value)} readOnly={ro}
      style={{ ...inputStyle, ...roStyle }} placeholder={ph ?? (t === "rupee" ? "₹" : undefined)} />;
  }

  return (
    <div className="field" style={{ marginBottom: 0 }}>
      <label>{label}</label>
      {control}
    </div>
  );
}
