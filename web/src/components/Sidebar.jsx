import { useLocation, useNavigate, useSearchParams } from "react-router-dom";
import { BUCKETS } from "../lib/constants.js";
import { useMeta } from "../lib/queries.js";
import Icon from "./Icon.jsx";

export default function Sidebar() {
  const { pathname } = useLocation();
  const [params] = useSearchParams();
  const navigate = useNavigate();
  const { data } = useMeta();
  const counts = data?.bucketCounts ?? {};

  const activeBucket = pathname.startsWith("/leads") ? params.get("bucket") || "assigned" : null;

  return (
    <nav className="sidebar">
      <button
        className={`nav-item ${pathname === "/" ? "active" : ""}`}
        onClick={() => navigate("/")}
      >
        <Icon name="dashboard" className="ni-ic" />
        Dashboard
      </button>

      <div className="nav-group-label">Lead Pipeline</div>
      {BUCKETS.map((b) => {
        const count = counts[b.key] ?? 0;
        return (
          <button
            key={b.key}
            className={`nav-item ${activeBucket === b.key ? "active" : ""}`}
            onClick={() => navigate(`/leads?bucket=${b.key}`)}
          >
            <Icon name="leads" className="ni-ic" />
            {b.label}
            <span className={`nav-badge ${b.tone === "poor" && count > 0 ? "alert" : ""}`}>{count}</span>
          </button>
        );
      })}
    </nav>
  );
}
