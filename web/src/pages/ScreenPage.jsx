import { useParams } from "react-router-dom";
import { useScreen } from "../lib/queries.js";
import { Spinner, ErrorBox, Pill } from "../components/ui.jsx";

// Titles + renderers for each /api/screens/{name} dataset (positional tuples, spec §8.8).
const META = {
  billing: "Billing & Invoices",
  yard: "Site-Visit Yard",
  mis: "MIS & Analytics",
  reports: "Issued Reports",
  documents: "Document Store",
  master: "Master Data",
};

function StatCards({ rows }) {
  return (
    <div className="grid grid-4" style={{ marginBottom: 16 }}>
      {rows.map((r, i) => (
        <div className="card stat" key={i}>
          <div className="v">{r[1]}</div>
          <div className="l">{r[0]}</div>
        </div>
      ))}
    </div>
  );
}

function Table({ head, rows, render }) {
  return (
    <div className="card">
      <table className="table">
        <thead><tr>{head.map((h) => <th key={h}>{h}</th>)}</tr></thead>
        <tbody>{rows.map((r, i) => <tr key={i} style={{ cursor: "default" }}>{render(r)}</tr>)}</tbody>
      </table>
    </div>
  );
}

export default function ScreenPage() {
  const { name } = useParams();
  const { data, isLoading, error } = useScreen(name);

  if (isLoading) return <Spinner label={`Loading ${name}…`} />;
  if (error) return <ErrorBox error={error} />;

  return (
    <>
      <div className="page-head"><div><h1>{META[name] || name}</h1></div></div>
      {renderScreen(name, data)}
    </>
  );
}

function renderScreen(name, d) {
  switch (name) {
    case "billing":
      return (
        <>
          <StatCards rows={d.stats} />
          <Table head={["Invoice", "Company", "Period", "Leads", "Amount", "Status"]} rows={d.invoices}
            render={(r) => <><td className="mono">{r[0]}</td><td>{r[1]}</td><td>{r[2]}</td><td>{r[3]}</td><td>{r[4]}</td><td><Pill tone={r[6]}>{r[5]}</Pill></td></>} />
        </>
      );
    case "yard":
      return (
        <>
          <Table head={["Time", "Valuer", "Req ID", "Type", "Location", "Status"]} rows={d.schedule}
            render={(r) => <><td className="mono">{r[0]}</td><td>{r[1]}</td><td className="mono">{r[2]}</td><td>{r[3]}</td><td>{r[4]}</td><td><Pill tone={r[6]}>{r[5]}</Pill></td></>} />
        </>
      );
    case "mis":
      return (
        <>
          <div className="grid grid-3" style={{ marginBottom: 16 }}>
            {d.reports.map((r, i) => (
              <div className="card card-pad" key={i}>
                <div style={{ fontWeight: 700, color: "var(--navy)" }}>{r[0]}</div>
                <div className="muted" style={{ fontSize: 12, margin: "4px 0 10px" }}>{r[1]}</div>
                <div className="stat" style={{ padding: 0 }}><span className="v">{r[2]}</span> <span className="muted">{r[3]}</span></div>
              </div>
            ))}
          </div>
          <Table head={["Day", "Leads"]} rows={d.weekly} render={(r) => <><td>{r[0]}</td><td className="mono">{r[1]}</td></>} />
        </>
      );
    case "reports":
      return (
        <>
          <StatCards rows={d.stats} />
          <Table head={["Req ID", "Applicant", "Type", "Lender", "Issued"]} rows={d.rows}
            render={(r) => <><td className="mono">{r[0]}</td><td>{r[1]}</td><td>{r[2]}</td><td>{r[3]}</td><td>{r[4]}</td></>} />
        </>
      );
    case "documents":
      return (
        <div className="grid grid-4">
          {d.folders.map((r, i) => (
            <div className="card stat" key={i}><div className="v">{r[1]}</div><div className="l">{r[0]}</div></div>
          ))}
        </div>
      );
    case "master":
      return (
        <Table head={["Category", "Count", "Sample values"]} rows={d}
          render={(r) => <><td style={{ fontWeight: 600, color: "var(--navy)" }}>{r[0]}</td><td className="mono">{r[1]}</td><td className="muted">{(r[3] || []).join(" · ")}</td></>} />
      );
    default:
      return <pre>{JSON.stringify(d, null, 2)}</pre>;
  }
}
