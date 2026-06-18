import { useAuth } from "../lib/auth.jsx";

function Row({ label, value }) {
  return (
    <div style={{ display: "flex", justifyContent: "space-between", padding: "11px 0", borderBottom: "1px solid var(--slate-100)" }}>
      <span className="muted" style={{ fontSize: 13 }}>{label}</span>
      <span style={{ fontWeight: 600, color: "var(--navy)" }}>{value || "—"}</span>
    </div>
  );
}

export default function Settings() {
  const { user, logout } = useAuth();
  return (
    <>
      <div className="page-head"><div><h1>Settings</h1><div className="sub">Your account and session.</div></div></div>
      <div className="grid" style={{ gridTemplateColumns: "1fr 1fr", gap: 16, maxWidth: 820 }}>
        <div className="card">
          <div className="card-head"><h3>Profile</h3></div>
          <div className="card-pad">
            <Row label="Name" value={user?.name} />
            <Row label="Email" value={user?.email} />
            <Row label="Role" value={user?.role} />
            <Row label="User Type" value={user?.userType} />
            <Row label="Company" value={user?.company} />
          </div>
        </div>
        <div className="card">
          <div className="card-head"><h3>Session</h3></div>
          <div className="card-pad">
            <p className="muted" style={{ marginTop: 0, fontSize: 13 }}>
              Signed in with a JWT access token (auto-refreshed). Sign out to clear your session on this device.
            </p>
            <button className="btn btn-danger" onClick={logout}>Sign out</button>
          </div>
        </div>
      </div>
    </>
  );
}
