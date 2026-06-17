// Small presentational primitives shared across pages.

export function Pill({ tone = "info", children }) {
  return (
    <span className={`pill pill-${tone}`}>
      <span className="pdot" />
      {children}
    </span>
  );
}

export function Spinner({ label }) {
  return (
    <div style={{ display: "flex", flexDirection: "column", alignItems: "center", gap: 12, padding: 40 }}>
      <div className="spinner" />
      {label && <span className="muted" style={{ fontSize: 13 }}>{label}</span>}
    </div>
  );
}

export function ErrorBox({ error }) {
  const msg = error?.error || error?.message || "Something went wrong.";
  return <div className="error-banner">{msg}</div>;
}

export function EmptyState({ children }) {
  return (
    <div className="card card-pad muted" style={{ textAlign: "center", padding: 48 }}>
      {children}
    </div>
  );
}
