// Lightweight CSS bar chart (no chart library). rows = [[label, value, color?], ...]
export function BarChart({ rows, unit = "" }) {
  const max = Math.max(1, ...rows.map((r) => Number(r[1]) || 0));
  return (
    <div style={{ display: "flex", flexDirection: "column", gap: 10 }}>
      {rows.map((r, i) => {
        const v = Number(r[1]) || 0;
        const color = r[2] || "var(--blue)";
        return (
          <div key={i} style={{ display: "flex", alignItems: "center", gap: 10 }}>
            <div style={{ width: 130, fontSize: 12, color: "var(--slate-600)", flex: "0 0 auto", whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis" }}>{r[0]}</div>
            <div style={{ flex: 1, background: "var(--slate-100)", borderRadius: 6, height: 16, overflow: "hidden" }}>
              <div style={{ width: `${(v / max) * 100}%`, height: "100%", background: color, borderRadius: 6 }} />
            </div>
            <div className="mono" style={{ width: 48, textAlign: "right", fontSize: 12 }}>{v}{unit}</div>
          </div>
        );
      })}
    </div>
  );
}

// Colored legend list. rows = [[label, percent, color], ...]
export function Legend({ rows }) {
  return (
    <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
      {rows.map((r, i) => (
        <div key={i} style={{ display: "flex", alignItems: "center", gap: 8, fontSize: 13 }}>
          <span style={{ width: 11, height: 11, borderRadius: 3, background: r[2] || "var(--blue)", flex: "0 0 auto" }} />
          <span style={{ flex: 1, color: "var(--slate-700)" }}>{r[0]}</span>
          <span className="mono" style={{ fontWeight: 600 }}>{r[1]}%</span>
        </div>
      ))}
    </div>
  );
}
