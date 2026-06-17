import { useState } from "react";
import { useNavigate, useLocation, Navigate } from "react-router-dom";
import { useAuth } from "../lib/auth.jsx";
import { ErrorBox } from "../components/ui.jsx";

export default function Login() {
  const { user, login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [email, setEmail] = useState("superadmin@kwikcheck.in");
  const [password, setPassword] = useState("");
  const [error, setError] = useState(null);
  const [busy, setBusy] = useState(false);

  if (user) return <Navigate to="/" replace />;

  async function onSubmit(e) {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      await login(email, password);
      const to = location.state?.from?.pathname || "/";
      navigate(to, { replace: true });
    } catch (err) {
      setError(err);
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="center-screen">
      <div className="card card-pad" style={{ width: 380 }}>
        <div style={{ display: "flex", alignItems: "center", gap: 11, marginBottom: 6 }}>
          <div className="brand-badge">KC</div>
          <div className="brand-name">
            <span className="k">Kwik</span>
            <span className="c">Check</span>
          </div>
        </div>
        <p className="muted" style={{ marginTop: 0, marginBottom: 20, fontSize: 13 }}>
          Sign in to the valuation workflow console.
        </p>

        {error && <ErrorBox error={error} />}

        <form onSubmit={onSubmit}>
          <div className="field">
            <label>Email</label>
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required autoFocus />
          </div>
          <div className="field">
            <label>Password</label>
            <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
          </div>
          <button className="btn btn-primary" style={{ width: "100%" }} disabled={busy}>
            {busy ? "Signing in…" : "Sign in"}
          </button>
        </form>

        <p className="muted" style={{ fontSize: 11.5, marginTop: 16, marginBottom: 0 }}>
          Seeded demo: superadmin@kwikcheck.in · password <code>Password@123</code>
        </p>
      </div>
    </div>
  );
}
