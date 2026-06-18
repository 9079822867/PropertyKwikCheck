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

// SVG donut. data = [[label, value, color], ...]
export function Donut({ data, size = 150, thickness = 24, center, sub }) {
  const total = data.reduce((s, d) => s + (Number(d[1]) || 0), 0) || 1;
  const r = (size - thickness) / 2;
  const c = size / 2;
  const C = 2 * Math.PI * r;
  let offset = 0;
  return (
    <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`}>
      <g transform={`rotate(-90 ${c} ${c})`}>
        <circle cx={c} cy={c} r={r} fill="none" stroke="var(--slate-100)" strokeWidth={thickness} />
        {data.map((d, i) => {
          const len = ((Number(d[1]) || 0) / total) * C;
          const el = (
            <circle key={i} cx={c} cy={c} r={r} fill="none" stroke={d[2] || "#1F5FAE"}
              strokeWidth={thickness} strokeDasharray={`${len} ${C - len}`} strokeDashoffset={-offset} />
          );
          offset += len;
          return el;
        })}
      </g>
      {center != null && <text x={c} y={c - 1} textAnchor="middle" fontSize="20" fontWeight="800" fill="#0C2742">{center}</text>}
      {sub && <text x={c} y={c + 15} textAnchor="middle" fontSize="9" fontWeight="700" fill="#94a3b6" letterSpacing="1">{sub}</text>}
    </svg>
  );
}

// SVG multi-line chart. series = [{ name, color, pts: [] }], xLabels = []
export function LineChart({ series, xLabels = [], width = 560, height = 210 }) {
  const pad = 28;
  const innerW = width - pad * 2, innerH = height - pad * 2;
  const max = Math.max(1, ...series.flatMap((s) => s.pts));
  const n = xLabels.length || 1;
  const xFor = (i) => pad + (n <= 1 ? 0 : (i / (n - 1)) * innerW);
  const yFor = (v) => pad + innerH - (v / max) * innerH;
  return (
    <svg width="100%" viewBox={`0 0 ${width} ${height}`}>
      {[0, 0.5, 1].map((g, i) => (
        <line key={i} x1={pad} x2={width - pad} y1={pad + innerH * g} y2={pad + innerH * g} stroke="var(--slate-100)" strokeWidth="1" />
      ))}
      {series.map((s, si) => (
        <polyline key={si} fill="none" stroke={s.color} strokeWidth="2.5" strokeLinejoin="round"
          points={s.pts.map((v, i) => `${xFor(i)},${yFor(v)}`).join(" ")} />
      ))}
      {xLabels.map((x, i) => (
        <text key={i} x={xFor(i)} y={height - 6} textAnchor="middle" fontSize="9" fill="#94a3b6">{x}</text>
      ))}
    </svg>
  );
}

// SVG stacked vertical bar chart.
// categories = ["Rajasthan", ...]; series = [{ name, color, values: [perCategory] }]
export function StackedBar({ categories, series, height = 290 }) {
  const width = 860, pad = 40, labelH = 22;
  const innerH = height - pad - labelH;
  const totals = categories.map((_, ci) => series.reduce((s, se) => s + (Number(se.values[ci]) || 0), 0));
  const max = Math.max(1, ...totals);
  const n = categories.length || 1;
  const slot = (width - pad - 8) / n;
  const barW = Math.min(48, slot * 0.6);
  const yFor = (v) => pad + innerH - (v / max) * innerH;

  return (
    <svg width="100%" viewBox={`0 0 ${width} ${height}`}>
      {[0, 0.5, 1].map((g, i) => {
        const y = pad + innerH * g;
        return (
          <g key={i}>
            <line x1={pad} x2={width - 8} y1={y} y2={y} stroke="var(--slate-100)" />
            <text x={pad - 6} y={y + 3} textAnchor="end" fontSize="9" fill="#94a3b6">{Math.round(max * (1 - g))}</text>
          </g>
        );
      })}
      {categories.map((cat, ci) => {
        const x = pad + slot * ci + (slot - barW) / 2;
        let cum = 0;
        return (
          <g key={ci}>
            {series.map((se, si) => {
              const v = Number(se.values[ci]) || 0;
              const h = (v / max) * innerH;
              const y = yFor(cum + v);
              cum += v;
              return <rect key={si} x={x} y={y} width={barW} height={Math.max(0, h)} fill={se.color} />;
            })}
            <text x={x + barW / 2} y={height - 6} textAnchor="middle" fontSize="10" fill="#516074">{cat}</text>
          </g>
        );
      })}
    </svg>
  );
}

// Inline legend chips. rows = [[label, color], ...]
export function LegendInline({ rows }) {
  return (
    <div style={{ display: "flex", gap: 16, flexWrap: "wrap", marginBottom: 12 }}>
      {rows.map((r, i) => (
        <span key={i} style={{ display: "inline-flex", alignItems: "center", gap: 6, fontSize: 11.5, color: "var(--slate-600)", fontWeight: 600 }}>
          <span style={{ width: 9, height: 9, borderRadius: 2, background: r[1] }} />{r[0]}
        </span>
      ))}
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
