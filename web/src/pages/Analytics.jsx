import { useAnalytics } from "../lib/queries.js";
import { Spinner, ErrorBox } from "../components/ui.jsx";
import { BarChart, Legend } from "../components/Viz.jsx";

export default function Analytics() {
  const { data, isLoading, error } = useAnalytics();
  if (isLoading) return <Spinner label="Loading analytics…" />;
  if (error) return <ErrorBox error={error} />;

  const kpi = data.kpi ?? [];
  const pipeline = (data.pipeline ?? []).map((r) => [r[0], r[1], r[2]]);
  const ptype = data.ptypeDonut ?? [];
  const valuers = (data.valuerProductivity ?? []).map((r) => [r[0], r[1]]);
  const states = data.stateData ?? [];
  const tat = data.tatTrend ?? { weeks: [], pts: [] };
  const tatRows = (tat.weeks || []).map((w, i) => [w, tat.pts?.[i] ?? 0]);

  return (
    <>
      <div className="page-head"><div><h1>Analytics</h1><div className="sub">Pipeline, productivity & regional breakdown.</div></div></div>

      <div className="grid grid-4" style={{ marginBottom: 16 }}>
        {kpi.map((k, i) => <div className="card stat" key={i}><div className="v">{k[1]}</div><div className="l">{k[0]}</div></div>)}
      </div>

      <div className="grid" style={{ gridTemplateColumns: "2fr 1fr", marginBottom: 16 }}>
        <div className="card">
          <div className="card-head"><h3>Pipeline by Stage</h3></div>
          <div className="card-pad"><BarChart rows={pipeline} /></div>
        </div>
        <div className="card">
          <div className="card-head"><h3>Asset Type Split</h3></div>
          <div className="card-pad"><Legend rows={ptype} /></div>
        </div>
      </div>

      <div className="grid" style={{ gridTemplateColumns: "1fr 1fr", marginBottom: 16 }}>
        <div className="card">
          <div className="card-head"><h3>Valuer Productivity</h3></div>
          <div className="card-pad">{valuers.length ? <BarChart rows={valuers} /> : <span className="muted">No data</span>}</div>
        </div>
        <div className="card">
          <div className="card-head"><h3>Avg TAT Trend (days)</h3></div>
          <div className="card-pad"><BarChart rows={tatRows} /></div>
        </div>
      </div>

      <div className="card">
        <div className="card-head"><h3>By State</h3></div>
        <table className="table">
          <thead><tr><th>State</th><th>M1</th><th>M2</th><th>M3</th><th>M4</th><th>M5</th><th>Total</th></tr></thead>
          <tbody>
            {states.map((s, i) => (
              <tr key={i} style={{ cursor: "default" }}>
                <td style={{ fontWeight: 600, color: "var(--navy)" }}>{s[0]}</td>
                <td className="mono">{s[1]}</td><td className="mono">{s[2]}</td><td className="mono">{s[3]}</td>
                <td className="mono">{s[4]}</td><td className="mono">{s[5]}</td><td className="mono">{s[6]}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </>
  );
}
