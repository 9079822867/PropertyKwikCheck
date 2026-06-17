import { useNavigate } from "react-router-dom";

export default function NotFound() {
  const navigate = useNavigate();
  return (
    <div className="card card-pad" style={{ textAlign: "center", padding: 60 }}>
      <h1 style={{ marginTop: 0 }}>404</h1>
      <p className="muted">This screen doesn’t exist yet.</p>
      <button className="btn btn-primary" onClick={() => navigate("/")}>Back to Dashboard</button>
    </div>
  );
}
