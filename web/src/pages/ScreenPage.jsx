import { useParams, useNavigate } from "react-router-dom";
import { useScreen } from "../lib/queries.js";
import { Spinner, ErrorBox } from "../components/ui.jsx";
import Icon from "../components/Icon.jsx";

const COLORS = {
  blue: ["#1F5FAE", "#EAF2FB"], good: ["#1E9D5B", "#E7F6EE"], poor: ["#D33B3B", "#FBE7E7"],
  amber: ["#C7890F", "#FBF2DD"], slate: ["#6b7a8d", "#eef2f7"], navy: ["#0C2742", "#EAF2FB"],
};
const col = (k) => COLORS[k] || COLORS.blue;
const TYPE_TAG = { property: ["#EAF2FB", "#1F5FAE"], plot: ["#FBF2DD", "#C7890F"], agri: ["#E7F6EE", "#1E9D5B"] };
const PTYPE_FAMILY = { Residential: "property", Commercial: "property", Industrial: "property", Plot: "plot", "Plot / Land": "plot", "Agricultural Land": "agri" };

const META = {
  billing: ["Administration", "Billing & Invoices", "Bank-wise invoicing, payments and payment requests."],
  yard: ["Administration", "Site-Visit Yard", "Field-operations board — today’s inspections and valuer load."],
  mis: ["Reports & Analytics", "MIS & Reports", "Operational analytics across the valuation pipeline."],
  reports: ["Reports & Analytics", "Reports Issued", "Finalised, verified reports — available for download."],
  documents: ["Administration", "Document Center", "Central repository for every document attached to a case."],
  master: ["Configuration", "Master Data", "Lookup lists that power every dropdown across the app."],
};

function AdminHead({ name, navigate }) {
  const [group, title, sub] = META[name] || ["", name, ""];
  return (
    <>
      <div className="crumbs"><a onClick={() => navigate("/")}>Home</a><span className="sep">/</span><span>{group}</span><span className="sep">/</span><span style={{ color: "var(--navy)" }}>{title}</span></div>
      <div className="page-head"><div><h1>{title}</h1><div className="sub">{sub}</div></div></div>
    </>
  );
}

function StatCards({ rows }) {
  return (
    <div className="stat-grid" style={{ marginBottom: 18 }}>
      {rows.map((r, i) => {
        const c = col(r[2]);
        return (
          <div key={i} className="dstat" style={{ cursor: "default" }}>
            <div className="accent" style={{ background: c[0] }} />
            <div className="st-top"><div className="st-ic" style={{ background: c[1], color: c[0] }}><Icon name={r[3]} size={19} /></div></div>
            <div className="num">{r[1]}</div><div className="lab">{r[0]}</div>
          </div>
        );
      })}
    </div>
  );
}

function TypeTag({ value }) {
  const fam = TYPE_TAG[value] ? value : PTYPE_FAMILY[value] || "property";
  const c = TYPE_TAG[fam] || TYPE_TAG.property;
  return <span className="type-tag" style={{ background: c[0], color: c[1] }}>{value}</span>;
}

const Pill = ({ tone, children }) => <span className={`pill pill-${tone || "info"}`}><span className="pdot" />{children}</span>;
const Card = ({ icon, title, meta, children, pad = true }) => (
  <div className="card">
    <div className="card-head"><div className="ch-ic"><Icon name={icon} /></div><h3>{title}</h3>{meta != null && <span className="ch-meta">{meta}</span>}</div>
    {pad ? <div className="card-pad">{children}</div> : children}
  </div>
);

export default function ScreenPage() {
  const { name } = useParams();
  const navigate = useNavigate();
  const { data: d, isLoading, error } = useScreen(name);

  if (isLoading) return <Spinner label={`Loading ${name}…`} />;
  if (error) return <ErrorBox error={error} />;

  return (
    <>
      <AdminHead name={name} navigate={navigate} />
      {render(name, d, navigate)}
    </>
  );
}

function render(name, d, navigate) {
  switch (name) {
    case "billing":
      return (
        <>
          <StatCards rows={d.stats} />
          <div className="card">
            <div className="card-head"><h3>Invoices</h3><span style={{ marginLeft: "auto" }}><button className="btn btn-primary btn-sm"><Icon name="plus" size={14} />Raise Invoice</button></span></div>
            <table className="table">
              <thead><tr><th>Invoice #</th><th>Bank</th><th>Period</th><th style={{ textAlign: "center" }}>Cases</th><th>Amount</th><th>Status</th></tr></thead>
              <tbody>{d.invoices.map((r, i) => (
                <tr key={i} style={{ cursor: "default" }}>
                  <td className="mono">{r[0]}</td><td>{r[1]}</td><td>{r[2]}</td><td style={{ textAlign: "center" }} className="mono">{r[3]}</td>
                  <td style={{ fontWeight: 700, color: "var(--navy)" }}>{r[4]}</td><td><Pill tone={r[6]}>{r[5]}</Pill></td>
                </tr>
              ))}</tbody>
            </table>
          </div>
        </>
      );

    case "yard":
      return (
        <>
          <div className="grid grid-4" style={{ marginBottom: 18 }}>
            {d.valuers.map((v, i) => (
              <div className="card card-pad" key={i}>
                <div style={{ display: "flex", alignItems: "center", gap: 10, marginBottom: 12 }}>
                  <div style={{ width: 38, height: 38, borderRadius: "50%", background: "linear-gradient(140deg,#2f6fbe,#0C2742)", color: "#fff", fontWeight: 700, fontSize: 13, display: "grid", placeItems: "center" }}>{v[0].split(" ").map((x) => x[0]).join("")}</div>
                  <div><div style={{ fontWeight: 700, color: "var(--navy)", fontSize: 13 }}>{v[0]}</div><div className="muted" style={{ fontSize: 11 }}>{v[2]}</div></div>
                </div>
                <div style={{ display: "flex", alignItems: "baseline", gap: 6 }}><div style={{ fontSize: 28, fontWeight: 800, color: "var(--navy)" }}>{v[1]}</div><div className="muted" style={{ fontSize: 12 }}>visits today</div></div>
                <div style={{ marginTop: 10 }}><TypeTag value={v[3]} /></div>
              </div>
            ))}
          </div>
          <Card icon="clock" title="Today’s Schedule" meta={`${d.schedule.length} visits`} pad={false}>
            <table className="table">
              <thead><tr><th>Time</th><th>Valuer</th><th>Case</th><th>Type</th><th>Location</th><th>Status</th></tr></thead>
              <tbody>{d.schedule.map((r, i) => (
                <tr key={i} style={{ cursor: "default" }}>
                  <td className="mono">{r[0]}</td><td>{r[1]}</td><td className="mono" style={{ fontSize: 11 }}>{r[2]}</td>
                  <td><TypeTag value={r[3]} /></td><td>{r[4]}</td><td><Pill tone={r[6]}>{r[5]}</Pill></td>
                </tr>
              ))}</tbody>
            </table>
          </Card>
        </>
      );

    case "mis": {
      const bmax = Math.max(1, ...d.weekly.map((b) => Number(b[1]) || 0));
      return (
        <>
          <div className="dash-2" style={{ gridTemplateColumns: "2fr 1fr", marginTop: 0, marginBottom: 18 }}>
            <Card icon="layers" title="Weekly Completed Volume" meta="Last 7 days">
              <div style={{ display: "flex", alignItems: "flex-end", gap: 16, height: 150, paddingTop: 10 }}>
                {d.weekly.map((b, i) => (
                  <div key={i} style={{ flex: 1, display: "flex", flexDirection: "column", alignItems: "center", gap: 7, height: "100%", justifyContent: "flex-end" }}>
                    <div style={{ fontSize: 11, fontWeight: 700, color: "var(--navy)" }}>{b[1]}</div>
                    <div style={{ width: "100%", maxWidth: 46, height: `${(Number(b[1]) / bmax) * 100}%`, background: "linear-gradient(180deg,#2f6fbe,#1F5FAE)", borderRadius: "7px 7px 0 0" }} />
                    <div className="muted" style={{ fontSize: 11 }}>{b[0]}</div>
                  </div>
                ))}
              </div>
            </Card>
            <Card icon="trend" title="Snapshot">
              {d.snapshot.map((s, i) => (
                <div key={i} style={{ display: "flex", justifyContent: "space-between", padding: "9px 0", borderBottom: "1px solid var(--slate-100)" }}>
                  <span className="muted" style={{ fontSize: 12.5 }}>{s[0]}</span><b style={{ color: "var(--navy)", fontSize: 13 }}>{s[1]}</b>
                </div>
              ))}
            </Card>
          </div>
          <div className="grid grid-3">
            {d.reports.map((r, i) => {
              const c = col(r[5]);
              return (
                <div className="card card-pad" key={i}>
                  <div style={{ display: "flex", alignItems: "center", gap: 10, marginBottom: 11 }}>
                    <div style={{ width: 38, height: 38, borderRadius: 10, background: c[1], color: c[0], display: "grid", placeItems: "center" }}><Icon name={r[4]} size={18} /></div>
                    <div style={{ fontWeight: 700, color: "var(--navy)", fontSize: 13.5 }}>{r[0]}</div>
                  </div>
                  <div className="muted" style={{ fontSize: 12, marginBottom: 12, minHeight: 32 }}>{r[1]}</div>
                  <div style={{ display: "flex", alignItems: "baseline", gap: 7 }}><div style={{ fontSize: 22, fontWeight: 800, color: "var(--navy)" }}>{r[2]}</div><div className="eyebrow" style={{ color: "var(--slate-400)" }}>{r[3]}</div></div>
                </div>
              );
            })}
          </div>
        </>
      );
    }

    case "reports":
      return (
        <>
          <StatCards rows={d.stats} />
          <div className="card">
            <div className="table-toolbar" style={{ display: "flex", gap: 12, alignItems: "center", padding: "14px 18px", borderBottom: "1px solid var(--line)" }}>
              <div className="tt-search"><input placeholder="Search reports…" /></div>
              <button className="filter-chip"><Icon name="bank" />Bank</button>
              <span style={{ marginLeft: "auto" }}><button className="btn btn-soft btn-sm"><Icon name="doc" size={14} />Download All</button></span>
            </div>
            <table className="table">
              <thead><tr><th>Case No.</th><th>Applicant</th><th>Property Type</th><th>Bank</th><th>Issued On</th><th>Status</th></tr></thead>
              <tbody>{d.rows.map((r, i) => (
                <tr key={i} style={{ cursor: "default" }}>
                  <td className="mono" style={{ color: "var(--blue)", fontWeight: 700 }}>{r[0]}</td>
                  <td style={{ fontWeight: 600, color: "var(--navy)" }}>{r[1]}</td><td><TypeTag value={r[2]} /></td>
                  <td>{r[3]}</td><td>{r[4]}</td><td><Pill tone="good">Issued</Pill></td>
                </tr>
              ))}</tbody>
            </table>
          </div>
        </>
      );

    case "documents":
      return (
        <>
          <div className="grid grid-3" style={{ marginBottom: 18 }}>
            {d.folders.map((f, i) => {
              const c = col(f[3]);
              return (
                <div className="card card-pad" key={i} style={{ cursor: "pointer" }}>
                  <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <div style={{ width: 42, height: 42, borderRadius: 11, background: c[1], color: c[0], display: "grid", placeItems: "center" }}><Icon name={f[2]} /></div>
                    <div><div style={{ fontWeight: 700, color: "var(--navy)", fontSize: 13.5 }}>{f[0]}</div><div className="muted" style={{ fontSize: 11.5 }}>{Number(f[1]).toLocaleString("en-IN")} files</div></div>
                    <span style={{ marginLeft: "auto", color: "var(--slate-300)" }}><Icon name="chevron" /></span>
                  </div>
                </div>
              );
            })}
          </div>
          <Card icon="doc" title="Recent Documents" pad>
            {(d.recent || []).length === 0 ? <span className="muted">No documents yet.</span> : d.recent.map((r, i) => (
              <div key={i} style={{ display: "flex", alignItems: "center", gap: 12, padding: "10px 0", borderBottom: "1px solid var(--slate-100)" }}>
                <div style={{ width: 34, height: 34, borderRadius: 8, background: "var(--blue-tint)", color: "var(--blue)", display: "grid", placeItems: "center" }}><Icon name="doc" size={16} /></div>
                <div style={{ flex: 1 }}><div style={{ fontWeight: 600, color: "var(--navy)", fontSize: 12.5 }}>{r[0]}</div><div className="muted" style={{ fontSize: 11 }}>{r[1]} · {r[2]} · {r[3]}</div></div>
              </div>
            ))}
          </Card>
        </>
      );

    case "master":
      return (
        <>
          <div className="placeholder-note"><Icon name="master" />
            <div>Edit a master list once and it updates everywhere — banks, asset types, valuation purposes, rejection reasons, localities and the DLC rate master all feed the forms.</div>
          </div>
          <div className="grid grid-3">
            {d.map((s, i) => (
              <Card key={i} icon={s[2] || "layers"} title={s[0]} meta={s[1]}>
                {(s[3] || []).map((it, j) => (
                  <div key={j} style={{ display: "flex", alignItems: "center", justifyContent: "space-between", padding: "8px 0", borderBottom: "1px solid var(--slate-100)", fontSize: 12.5 }}>
                    <span style={{ color: "var(--slate-700)" }}>{it}</span>
                  </div>
                ))}
                <button className="btn btn-soft btn-sm" style={{ marginTop: 12, width: "100%" }}><Icon name="plus" size={14} />Add item</button>
              </Card>
            ))}
          </div>
        </>
      );

    default:
      return <ErrorBox error={{ error: "Unknown screen" }} />;
  }
}
