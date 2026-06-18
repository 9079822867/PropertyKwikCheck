import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAnalytics } from "../lib/queries.js";
import { Spinner, ErrorBox, Pill } from "../components/ui.jsx";
import { Donut, LineChart, StackedBar, LegendInline } from "../components/Viz.jsx";
import Icon from "../components/Icon.jsx";

// Stage segments for the stacked distribution (aligns to stateData's 5 value columns).
const DIST_STAGES = [
  ["Fresh Leads", "#1F5FAE"], ["Assigned for Inspection", "#DD6F1E"], ["Verification Completed", "#1E9D5B"],
  ["QC Stage Leads", "#0C2742"], ["Pricing Stage Leads", "#C7890F"],
];
const abbrev = (s) => (s.length > 10 ? s.split(/\s+/).map((w) => w[0]).join("") : s);
import { BUCKET_MAP, TAT_TONE } from "../lib/constants.js";
import { inr } from "../lib/format.js";

const COLORS = {
  blue: ["#1F5FAE", "#EAF2FB"], good: ["#1E9D5B", "#E7F6EE"], poor: ["#D33B3B", "#FBE7E7"],
  amber: ["#C7890F", "#FBF2DD"], slate: ["#6b7a8d", "#eef2f7"], navy: ["#0C2742", "#EAF2FB"],
};
const col = (k) => COLORS[k] || COLORS.blue;

function CardHead({ icon, title, meta }) {
  return (
    <div className="card-head">
      <div className="ch-ic"><Icon name={icon} /></div>
      <h3>{title}</h3>
      {meta && <span className="ch-meta">{meta}</span>}
    </div>
  );
}

export default function Dashboard() {
  const navigate = useNavigate();
  const [dashState, setDashState] = useState(null);
  const { data, isLoading, error } = useAnalytics();

  if (isLoading) return <Spinner label="Loading dashboard…" />;
  if (error) return (<><div className="page-head"><div><h1>Dashboard</h1></div></div><ErrorBox error={error} /></>);

  const stats = data.stats ?? [];
  const kpi = data.kpi ?? [];
  const stageDefs = data.stageDefs ?? [];
  const ptype = data.ptypeDonut ?? [];
  const pipeline = data.pipeline ?? [];
  const states = data.stateData ?? [];
  const districtData = data.districtData ?? {};
  const recent = data.recent ?? [];
  const activities = data.activities ?? [];
  const siteVisits = data.siteVisits ?? [];
  const co = data.casesOverview ?? { x: [], created: [], approved: [], issued: [] };

  const stageTotal = stageDefs.reduce((s, d) => s + (Number(d[1]) || 0), 0);
  const ptypeTotal = ptype.reduce((s, d) => s + (Number(d[1]) || 0), 0);
  const pipeMax = Math.max(1, ...pipeline.map((p) => Number(p[1]) || 0));

  return (
    <>
      <div className="page-head">
        <div>
          <div className="eyebrow">Valuation Workflow Console</div>
          <h1>Dashboard</h1>
          <div className="sub">Overview of leads &amp; reports — live across all banks and states.</div>
        </div>
        <button className="btn btn-primary" onClick={() => navigate("/leads/new")}><Icon name="plus" size={16} />Create New Lead</button>
      </div>

      {/* stat grid */}
      <div className="stat-grid">
        {stats.map((s, i) => {
          const [bucket, label, count, colorKey, icon, tr, trl] = s;
          const c = col(colorKey);
          return (
            <div key={i} className="dstat" onClick={() => bucket && navigate(`/leads?bucket=${bucket}`)} style={{ cursor: bucket ? "pointer" : "default" }}>
              <div className="accent" style={{ background: c[0] }} />
              <div className="st-top">
                <div className="st-ic" style={{ background: c[1], color: c[0] }}><Icon name={icon} size={19} /></div>
              </div>
              <div className="num">{count}</div>
              <div className="lab">{label}</div>
              <div className={`trend ${tr}`}>{trl}</div>
            </div>
          );
        })}
      </div>

      {/* kpi strip */}
      <div className="kpi-strip" style={{ marginTop: 18 }}>
        {kpi.map((k, i) => {
          const c = col(k[2]);
          return (
            <div className="kpi" key={i}>
              <div className="kpi-ic" style={{ background: c[1], color: c[0] }}><Icon name={k[3]} size={18} /></div>
              <div><div className="kpi-v">{k[1]}</div><div className="kpi-l">{k[0]}</div></div>
            </div>
          );
        })}
      </div>

      {/* cases overview + overall status donut */}
      <div className="dash-2" style={{ gridTemplateColumns: "1.6fr 1fr" }}>
        <div className="card">
          <CardHead icon="trend" title="Cases Overview" meta="This week" />
          <div className="card-pad">
            <LineChart xLabels={co.x} series={[
              { name: "Created", color: "#1F5FAE", pts: co.created || [] },
              { name: "Approved", color: "#1E9D5B", pts: co.approved || [] },
              { name: "Issued", color: "#C7890F", pts: co.issued || [] },
            ]} />
          </div>
        </div>
        <div className="card">
          <CardHead icon="layers" title="Overall Leads Status" />
          <div className="card-pad" style={{ display: "flex", gap: 18, alignItems: "center", flexWrap: "wrap" }}>
            <Donut data={stageDefs} size={158} thickness={26} center={stageTotal.toLocaleString("en-IN")} sub="TOTAL" />
            <div style={{ flex: 1, minWidth: 180, display: "flex", flexDirection: "column", gap: 6 }}>
              {stageDefs.map((d, i) => (
                <div key={i} style={{ display: "flex", alignItems: "center", gap: 8, fontSize: 12 }}>
                  <span style={{ width: 10, height: 10, borderRadius: 3, background: d[2] }} />
                  <span style={{ flex: 1, color: "var(--slate-700)" }}>{d[0]}</span>
                  <span className="mono" style={{ fontWeight: 600 }}>{d[1]}</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* pipeline + property type donut */}
      <div className="dash-2" style={{ gridTemplateColumns: "1fr 1fr" }}>
        <div className="card">
          <CardHead icon="buckets" title="Lead Pipeline" meta="Live stages" />
          <div className="card-pad">
            {pipeline.map((p, i) => (
              <div key={i} style={{ display: "flex", alignItems: "center", gap: 12, marginBottom: 11 }}>
                <div style={{ width: 118, fontSize: 12.5, fontWeight: 600, color: "var(--slate-600)" }}>{p[0]}</div>
                <div style={{ flex: 1, height: 26, background: "var(--slate-50)", borderRadius: 7, overflow: "hidden" }}>
                  <div style={{ height: "100%", width: `${Math.max(6, (Number(p[1]) / pipeMax) * 100)}%`, background: p[2], borderRadius: 7, display: "flex", alignItems: "center", justifyContent: "flex-end", paddingRight: 9, color: "#fff", fontSize: 11.5, fontWeight: 700 }}>{p[1]}</div>
                </div>
              </div>
            ))}
          </div>
        </div>
        <div className="card">
          <CardHead icon="building" title="Cases by Property Type" meta={`${ptypeTotal} total`} />
          <div className="card-pad" style={{ display: "flex", gap: 18, alignItems: "center", flexWrap: "wrap" }}>
            <Donut data={ptype} size={150} thickness={24} center={ptypeTotal} sub="TOTAL" />
            <div style={{ flex: 1, minWidth: 170, display: "flex", flexDirection: "column", gap: 7 }}>
              {ptype.map((d, i) => (
                <div key={i} style={{ display: "flex", alignItems: "center", gap: 8, fontSize: 13 }}>
                  <span style={{ width: 11, height: 11, borderRadius: 3, background: d[2] }} />
                  <span style={{ flex: 1, color: "var(--slate-700)" }}>{d[0]}</span>
                  <span className="mono" style={{ fontWeight: 600 }}>{d[1]}%</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* state summary */}
      <div className="card" style={{ marginTop: 18 }}>
        <CardHead icon="map" title="State Wise Summary" />
        <table className="table">
          <thead><tr><th>State</th><th>M1</th><th>M2</th><th>M3</th><th>M4</th><th>M5</th><th>Total</th></tr></thead>
          <tbody>
            {states.map((s, i) => (
              <tr key={i} style={{ cursor: "default" }}>
                <td style={{ fontWeight: 600, color: "var(--navy)" }}>{s[0]}</td>
                {s.slice(1).map((v, j) => <td key={j} className="mono">{v}</td>)}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* district breakup + leads-by-state */}
      {Object.keys(districtData).length > 0 && (() => {
        const stateKeys = Object.keys(districtData);
        const active = dashState && districtData[dashState] ? dashState : stateKeys[0];
        const rows = districtData[active] || [];
        const ranked = [...states].map((r) => [r[0], r[6] ?? r.slice(1).reduce((a, b) => a + (Number(b) || 0), 0)]).sort((a, b) => b[1] - a[1]);
        const rmax = Math.max(1, ...ranked.map((r) => r[1]));
        return (
          <div className="dash-2" style={{ gridTemplateColumns: "1.7fr 1fr" }}>
            <div className="card">
              <div className="card-head">
                <div className="ch-ic"><Icon name="pin" /></div>
                <h3>District Wise Breakup</h3>
                <span style={{ marginLeft: "auto" }}>
                  <select value={active} onChange={(e) => setDashState(e.target.value)}
                    style={{ height: 32, border: "1px solid var(--line)", borderRadius: 8, padding: "0 9px", background: "var(--slate-50)", fontSize: 12.5 }}>
                    {stateKeys.map((s) => <option key={s}>{s}</option>)}
                  </select>
                </span>
              </div>
              <table className="table">
                <thead><tr><th>District</th><th>M1</th><th>M2</th><th>M3</th><th>M4</th><th>M5</th><th>Total</th></tr></thead>
                <tbody>
                  {rows.map((r, i) => (
                    <tr key={i} style={{ cursor: "default" }}>
                      <td style={{ fontWeight: 600, color: "var(--navy)" }}>{r[0]}</td>
                      {r.slice(1).map((v, j) => <td key={j} className="mono">{v}</td>)}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div className="card">
              <div className="card-head"><div className="ch-ic"><Icon name="map" /></div><h3>Leads by State</h3><span className="ch-meta">Ranked</span></div>
              <div className="card-pad" style={{ display: "flex", flexDirection: "column", gap: 10 }}>
                {ranked.map((r, i) => (
                  <div key={i} style={{ display: "flex", alignItems: "center", gap: 10 }}>
                    <span style={{ width: 18, color: "var(--slate-400)", fontWeight: 700, fontSize: 11 }}>{i + 1}</span>
                    <span style={{ width: 90, fontSize: 12, color: "var(--slate-700)" }}>{r[0]}</span>
                    <span style={{ flex: 1, height: 14, background: "var(--slate-100)", borderRadius: 5, overflow: "hidden" }}>
                      <i style={{ display: "block", height: "100%", width: `${(r[1] / rmax) * 100}%`, background: "var(--blue)", borderRadius: 5 }} />
                    </span>
                    <span className="mono" style={{ width: 52, textAlign: "right", fontSize: 12, fontWeight: 600 }}>{r[1].toLocaleString("en-IN")}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        );
      })()}

      {/* state-wise stacked distribution */}
      {states.length > 0 && (
        <div className="card" style={{ marginTop: 18 }}>
          <div className="card-head"><div className="ch-ic"><Icon name="trend" /></div><h3>State Wise Leads Distribution</h3><span className="ch-meta">Stacked by stage</span></div>
          <div className="card-pad">
            <LegendInline rows={DIST_STAGES} />
            <StackedBar
              categories={states.map((r) => abbrev(r[0]))}
              series={DIST_STAGES.map(([name, color], si) => ({ name, color, values: states.map((r) => Number(r[si + 1]) || 0) }))}
            />
          </div>
        </div>
      )}

      {/* recent leads + activity / visits */}
      <div className="dash-2" style={{ gridTemplateColumns: "1.5fr 1fr" }}>
        <div className="card">
          <CardHead icon="leads" title="Recent Leads" />
          <table className="table">
            <thead><tr><th>Customer</th><th>Case No.</th><th>Type</th><th>Bank</th><th>Status</th><th>TAT</th></tr></thead>
            <tbody>
              {recent.map((l) => (
                <tr key={l.id} onClick={() => navigate(`/leads/${l.id}`)}>
                  <td style={{ fontWeight: 600, color: "var(--navy)" }}>{l.applicant}</td>
                  <td className="mono">{l.reqId}</td><td>{l.ptype}</td><td>{l.lender}</td>
                  <td><Pill tone={BUCKET_MAP[l.stage]?.tone || "info"}>{BUCKET_MAP[l.stage]?.status || l.stage}</Pill></td>
                  <td><Pill tone={TAT_TONE[l.tatState] || "info"}>{l.tatPct}%</Pill></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        <div style={{ display: "flex", flexDirection: "column", gap: 18 }}>
          <div className="card">
            <CardHead icon="layers" title="Quick Actions" />
            <div className="card-pad" style={{ display: "flex", flexDirection: "column", gap: 8 }}>
              {[
                ["New Valuation Case", "Create a new case", "plus", () => navigate("/leads/new")],
                ["My Assignments", "Cases assigned to me", "user", () => navigate("/leads?bucket=assigned")],
                ["Site Visit Scheduler", "Schedule a site visit", "pin", () => navigate("/screens/yard")],
                ["Issued Reports", "Browse completed reports", "doc", () => navigate("/screens/reports")],
              ].map(([t, d, icon, fn], i) => (
                <button key={i} onClick={fn} style={{ display: "flex", alignItems: "center", gap: 11, padding: "10px 12px", border: "1px solid var(--line)", borderRadius: 10, background: "#fff", textAlign: "left", cursor: "pointer" }}>
                  <span style={{ width: 32, height: 32, borderRadius: 8, background: "var(--blue-tint)", color: "var(--blue)", display: "grid", placeItems: "center", flex: "0 0 auto" }}><Icon name={icon} size={16} /></span>
                  <span style={{ flex: 1 }}>
                    <span style={{ display: "block", fontWeight: 600, fontSize: 13, color: "var(--navy)" }}>{t}</span>
                    <span className="muted" style={{ fontSize: 11.5 }}>{d}</span>
                  </span>
                  <Icon name="chevron" size={15} />
                </button>
              ))}
            </div>
          </div>
          <div className="card">
            <CardHead icon="clock" title="Recent Activities" />
            <div className="card-pad" style={{ display: "flex", flexDirection: "column", gap: 12 }}>
              {activities.length ? activities.map((a, i) => (
                <div key={i} style={{ display: "flex", gap: 10, alignItems: "flex-start" }}>
                  <span style={{ width: 8, height: 8, borderRadius: "50%", background: "var(--blue)", marginTop: 5, flex: "0 0 auto" }} />
                  <div style={{ fontSize: 13 }}><div style={{ color: "var(--navy)" }}>{a[1]}</div><div className="muted" style={{ fontSize: 11.5 }}>{a[2]} · {a[3]}</div></div>
                </div>
              )) : <span className="muted">No recent activity</span>}
            </div>
          </div>
          {siteVisits.length > 0 && (
            <div className="card">
              <CardHead icon="map" title="Upcoming Site Visits" />
              <div className="card-pad">
                {siteVisits.slice(0, 4).map((s, i) => (
                  <div key={i} style={{ display: "flex", alignItems: "center", gap: 12, padding: "9px 0", borderBottom: "1px solid var(--slate-100)" }}>
                    <div style={{ width: 44, textAlign: "center", flex: "0 0 auto" }}>
                      <div style={{ fontSize: 18, fontWeight: 800, color: "var(--navy)", lineHeight: 1 }}>{s[0]}</div>
                      <div className="eyebrow" style={{ color: "var(--slate-400)" }}>{s[1]}</div>
                    </div>
                    <div style={{ flex: 1 }}>
                      <div className="mono" style={{ fontSize: 11.5, color: "var(--blue)", fontWeight: 700 }}>{s[2]}</div>
                      <div className="muted" style={{ fontSize: 11.5 }}>{s[4]}</div>
                    </div>
                    <div style={{ fontSize: 11.5, fontWeight: 600, color: "var(--slate-600)" }}>{s[5]}</div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>
    </>
  );
}
