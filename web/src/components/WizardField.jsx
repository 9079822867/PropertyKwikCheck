// Renders one report field by type and reports changes via onChange(key, value).
const RATINGS = [
  ["good", "Good"], ["fair", "Fair"], ["avg", "Average"], ["poor", "Poor"],
];
const SEG_YN = ["Yes", "No"];
const SEG_YNP = ["Yes", "No", "Partial"];

export default function WizardField({ field, data, onChange }) {
  const { k, label, t, options } = field;
  const val = data[k] ?? "";

  const inputStyle = { width: "100%", height: 38, border: "1px solid var(--line)", borderRadius: 8, padding: "0 11px", background: "var(--slate-50)", fontSize: 13.5 };
  const set = (v) => onChange(k, v);

  let control;
  if (t === "ta") {
    control = <textarea value={val} onChange={(e) => set(e.target.value)} rows={3}
      style={{ ...inputStyle, height: "auto", padding: "9px 11px", resize: "vertical" }} />;
  } else if (t === "sel") {
    control = (
      <select value={val} onChange={(e) => set(e.target.value)} style={inputStyle}>
        <option value="">— select —</option>
        {options.map((o) => <option key={o} value={o}>{o}</option>)}
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
    control = <input type={type} value={val} onChange={(e) => set(e.target.value)}
      style={inputStyle} placeholder={t === "rupee" ? "₹" : undefined} />;
  }

  return (
    <div className="field" style={{ marginBottom: 0 }}>
      <label>{label}</label>
      {control}
    </div>
  );
}
