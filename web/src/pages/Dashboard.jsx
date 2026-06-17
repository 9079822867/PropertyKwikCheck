import { useNavigate } from "react-router-dom";
import { useAnalytics } from "../lib/queries.js";
import { Spinner, ErrorBox, Pill } from "../components/ui.jsx";
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
