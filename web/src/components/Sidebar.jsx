import { useState } from "react";
import { useLocation, useNavigate, useSearchParams } from "react-router-dom";
import { BUCKETS } from "../lib/constants.js";
import { useMeta } from "../lib/queries.js";
import Icon from "./Icon.jsx";

// Sidebar information architecture mirrors the KwikCheck prototype.
export default function Sidebar() {
  const { pathname } = useLocation();
  const [params] = useSearchParams();
  const navigate = useNavigate();
  const { data } = useMeta();
  const counts = data?.bucketCounts ?? {};

  const onLeadList = pathname === "/leads";
  const activeBucket = onLeadList ? params.get("bucket") || "assigned" : null;

  // Which collapsible groups are open — auto-open the one matching the route.
  const [open, setOpen] = useState({});
  const isOpen = (key, auto) => open[key] ?? auto;
  const toggle = (key) => setOpen((o) => ({ ...o, [key]: !(o[key] ?? false) }));

  const Item = ({ to, icon, label, active, onClick }) => (
    <button className={`nav-item ${active ? "active" : ""}`} onClick={onClick || (() => navigate(to))}>
      <Icon name={icon} className="ni-ic" />
      <span className="nlab">{label}</span>
    </button>
  );

  const Parent = ({ k, icon, label, auto }) => (
    <button className={`nav-item ${isOpen(k, auto) ? "open" : ""} ${auto ? "active" : ""}`} onClick={() => toggle(k)}>
      <Icon name={icon} className="ni-ic" />
      <span className="nlab">{label}</span>
      <Icon name="chevron" className="chev" />
    </button>
  );

  const Sub = ({ to, label, count, alert, active }) => (
    <button className={`subnav-item ${active ? "active" : ""}`} onClick={() => navigate(to)}>
      <span>{label}</span>
      {count != null && <span className={`cnt ${alert ? "alert" : ""}`}>{count}</span>}
    </button>
  );

  return (
    <nav className="sidebar">
      <Item icon="home" label="Dashboard" active={pathname === "/"} onClick={() => navigate("/")} />

      <Parent k="buckets" icon="buckets" label="Lead Buckets" auto={onLeadList} />
      <div className={`subnav ${isOpen("buckets", onLeadList) ? "open" : ""}`}>
        {BUCKETS.map((b) => {
          const c = counts[b.key] ?? 0;
          const alert = (b.key === "qc_hold" || b.key === "out_of_tat" || b.key === "rejected") && c > 0;
          return <Sub key={b.key} to={`/leads?bucket=${b.key}`} label={b.label} count={c} alert={alert} active={activeBucket === b.key} />;
        })}
      </div>

      <Parent k="leadmgmt" icon="folderadd" label="Lead Management" auto={pathname === "/leads/new"} />
      <div className={`subnav ${isOpen("leadmgmt", pathname === "/leads/new") ? "open" : ""}`}>
        <Sub to="/leads/new" label="Create New Lead" active={pathname === "/leads/new"} />
        <Sub to="/leads?bucket=fresh" label="All Leads" />
      </div>

      <div className="nav-group-label">Administration</div>
      <Parent k="users" icon="user" label="User Management" auto={pathname.startsWith("/users")} />
      <div className={`subnav ${isOpen("users", pathname.startsWith("/users")) ? "open" : ""}`}>
        <Sub to="/users" label="All Users" active={pathname === "/users"} />
        <Sub to="/users/new" label="Create New User" active={pathname === "/users/new"} />
      </div>
      <Item icon="billing" label="Billing" active={pathname === "/screens/billing"} onClick={() => navigate("/screens/billing")} />
      <Parent k="company" icon="company" label="Company" auto={pathname.startsWith("/companies")} />
      <div className={`subnav ${isOpen("company", pathname.startsWith("/companies")) ? "open" : ""}`}>
        <Sub to="/companies" label="All Companies" active={pathname === "/companies"} />
        <Sub to="/companies/new" label="Create Company" active={pathname === "/companies/new"} />
      </div>
      <Item icon="company" label="Yard" active={pathname === "/screens/yard"} onClick={() => navigate("/screens/yard")} />
      <Item icon="doc" label="Document Center" active={pathname === "/screens/documents"} onClick={() => navigate("/screens/documents")} />

      <div className="nav-group-label">Reports &amp; Analytics</div>
      <Item icon="trend" label="Analytics" active={pathname === "/analytics"} onClick={() => navigate("/analytics")} />
      <Item icon="layers" label="MIS Reports" active={pathname === "/screens/mis"} onClick={() => navigate("/screens/mis")} />
      <Item icon="doc" label="Reports Issued" active={pathname === "/screens/reports"} onClick={() => navigate("/screens/reports")} />

      <div className="nav-group-label">Configuration</div>
      <Item icon="master" label="Master" active={pathname === "/screens/master"} onClick={() => navigate("/screens/master")} />
      <Item icon="master" label="Settings" active={pathname === "/settings"} onClick={() => navigate("/settings")} />
    </nav>
  );
}
