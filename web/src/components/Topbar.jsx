import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../lib/auth.jsx";
import { initials } from "../lib/format.js";
import Icon from "./Icon.jsx";

export default function Topbar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [q, setQ] = useState("");

  function submitSearch(e) {
    e.preventDefault();
    navigate(`/leads?bucket=assigned&q=${encodeURIComponent(q)}`);
  }

  return (
    <header className="topbar">
      <div className="brand">
        <div className="brand-badge">KC</div>
        <div className="brand-name">
          <span className="k">Kwik</span>
          <span className="c">Check</span>
        </div>
      </div>

      <form className="topbar-search" onSubmit={submitSearch}>
        <input
          placeholder="Search leads by applicant, req id, lender…"
          value={q}
          onChange={(e) => setQ(e.target.value)}
        />
      </form>

      <div className="topbar-right">
        <div className="tb-user">
          <div className="av">{initials(user?.name)}</div>
          <div className="who">
            <b>{user?.name}</b>
            <span>{user?.userType || user?.role}</span>
          </div>
        </div>
        <button className="btn btn-ghost btn-sm" onClick={logout} title="Sign out">
          <Icon name="logout" size={16} />
          Sign out
        </button>
      </div>
    </header>
  );
}
