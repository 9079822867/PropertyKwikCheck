import { useNavigate } from "react-router-dom";
import { useAnalytics } from "../lib/queries.js";
import { Spinner, ErrorBox, Pill } from "../components/ui.jsx";
import { BarChart } from "../components/Viz.jsx";
import { BUCKET_MAP, TAT_TONE } from "../lib/constants.js";
import { inr } from "../lib/format.js";

export default function Dashboard() {
  const navigate = useNavigate();
  const { data, isLoading, error } = useAnalytics();

  if (isLoading) return <Spinner label="Loading dashboard…" />;
  if (error) {
    return (
      <>
        <div className="page-head"><div><h1>Dashboard</h1></div></div>
        <ErrorBox error={error} />
      </>
    );
  }

  const stats = data.stats ?? [];
  const kpi = data.kpi ?? [];
  const recent = data.recent ?? [];
  const pipeline = (data.pipeline ?? []).map((r) => [r[0], r[1], r[2]]);
  const activities = data.activities ?? [];
  const siteVisits = data.siteVisits ?? [];

  return (
    <>
      <div className="page-head">
        <div>
          <h1>Dashboard</h1>
          <div className="sub">Operational overview of the valuation pipeline.</div>
        </div>
      </div>

      <div className="grid grid-4" style={{ marginBottom: 16 }}>
        {stats.map((s, i) => {
          const [stageKey, label, count] = s;
          return (
            <div
              key={i}
              className="card stat"
              style={{ cursor: stageKey ? "pointer" : "default" }}
              onClick={() => stageKey && navigate(`/leads?bucket=${stageKey}`)}
            >
              <div className="v">{count}</div>
              <div className="l">{label}</div>
            </div>
          );
        })}
      </div>

      <div className="grid grid-4" style={{ marginBottom: 24 }}>
        {kpi.map((k, i) => {
          const [label, value] = k;
          return (
            <div key={i} className="card stat">
              <div className="v">{value}</div>
              <div className="l">{label}</div>
            </div>
          );
        })}
      </div>

      <div className="grid" style={{ gridTemplateColumns: "1fr 1fr", marginBottom: 16 }}>
        <div className="card">
          <div className="card-head"><h3>Pipeline by Stage</h3></div>
          <div className="card-pad">{pipeline.length ? <BarChart rows={pipeline} /> : <span className="muted">No data</span>}</div>
        </div>
        <div className="card">
          <div className="card-head"><h3>Recent Activity</h3></div>
          <div className="card-pad" style={{ display: "flex", flexDirection: "column", gap: 12 }}>
            {activities.length ? activities.map((a, i) => (
              <div key={i} style={{ display: "flex", gap: 10, alignItems: "flex-start" }}>
                <span style={{ width: 8, height: 8, borderRadius: "50%", background: "var(--blue)", marginTop: 5, flex: "0 0 auto" }} />
                <div style={{ fontSize: 13 }}>
                  <div style={{ color: "var(--navy)" }}>{a[1]}</div>
                  <div className="muted" style={{ fontSize: 11.5 }}>{a[2]} · {a[3]}</div>
                </div>
              </div>
            )) : <span className="muted">No recent activity</span>}
          </div>
        </div>
      </div>

      {siteVisits.length > 0 && (
        <div className="card" style={{ marginBottom: 16 }}>
          <div className="card-head"><h3>Upcoming Site Visits</h3></div>
          <table className="table">
            <thead><tr><th>Date</th><th>Req ID</th><th>Type</th><th>Location</th><th>Time</th></tr></thead>
            <tbody>
              {siteVisits.map((s, i) => (
                <tr key={i} style={{ cursor: "default" }}>
                  <td className="mono">{s[0]} {s[1]}</td><td className="mono">{s[2]}</td><td>{s[3]}</td><td>{s[4]}</td><td>{s[5]}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <div className="card">
        <div className="card-head"><h3>Recent Leads</h3></div>
        <table className="table">
          <thead>
            <tr>
              <th>Req ID</th><th>Applicant</th><th>Type</th><th>Lender</th>
              <th>Stage</th><th>Value</th><th>TAT</th>
            </tr>
          </thead>
          <tbody>
            {recent.map((l) => (
              <tr key={l.id} onClick={() => navigate(`/leads/${l.id}`)}>
                <td className="mono">{l.reqId}</td>
                <td>{l.applicant}</td>
                <td>{l.ptype}</td>
                <td>{l.lender}</td>
                <td><Pill tone={BUCKET_MAP[l.stage]?.tone || "info"}>{BUCKET_MAP[l.stage]?.status || l.stage}</Pill></td>
                <td>{inr(l.value)}</td>
                <td><Pill tone={TAT_TONE[l.tatState] || "info"}>{l.tatPct}%</Pill></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </>
  );
}
