import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useUsers, useDeleteUser } from "../lib/queries.js";
import { Spinner, ErrorBox } from "../components/ui.jsx";
import Icon from "../components/Icon.jsx";
import { initials } from "../lib/format.js";
import { toast } from "../lib/toast.js";

export default function Users() {
  const navigate = useNavigate();
  const { data: users, isLoading, error } = useUsers();
  const del = useDeleteUser();
  const [q, setQ] = useState("");

  const rows = (users || []).filter((u) =>
    !q || `${u.name} ${u.email} ${u.role} ${u.userType} ${u.company}`.toLowerCase().includes(q.toLowerCase()));

  return (
    <>
      <div className="crumbs"><a onClick={() => navigate("/")}>Home</a><span className="sep">/</span><span>User Management</span><span className="sep">/</span><span style={{ color: "var(--navy)" }}>All Users</span></div>
      <div className="page-head"><div><h1>All Users</h1><div className="sub">Every role that touches a valuation — permissions are role-based.</div></div></div>

      <div className="placeholder-note"><Icon name="user" />
        <div><b>4 roles · 20 user types</b> map to the workflow: Client · RO · Internal · Cando. Internal (Super Admin) sees and edits all stages; each user type unlocks specific lead stages and actions.</div>
      </div>

      <div className="card">
        <div className="table-toolbar" style={{ display: "flex", gap: 12, alignItems: "center", padding: "14px 18px", borderBottom: "1px solid var(--line)" }}>
          <div className="tt-search"><input placeholder="Search users…" value={q} onChange={(e) => setQ(e.target.value)} /></div>
          <span style={{ marginLeft: "auto" }}><button className="btn btn-primary btn-sm" onClick={() => navigate("/users/new")}><Icon name="plus" size={14} />Create New User</button></span>
        </div>
        {isLoading ? <Spinner /> : error ? <div style={{ padding: 16 }}><ErrorBox error={error} /></div> : (
          <table className="table">
            <thead><tr><th>User</th><th>Role / Type</th><th>Company</th><th style={{ textAlign: "center" }}>Leads</th><th>Status</th><th style={{ textAlign: "right" }}>Actions</th></tr></thead>
            <tbody>
              {rows.map((u) => {
                const admin = /Super Admin|Admin/.test(u.userType || "");
                return (
                  <tr key={u.id} style={{ cursor: "default" }}>
                    <td><div style={{ display: "flex", alignItems: "center", gap: 10 }}>
                      <div className="avatar">{initials(u.name)}</div>
                      <div><div style={{ fontWeight: 700, color: "var(--navy)", fontSize: 13 }}>{u.name}</div><div className="muted" style={{ fontSize: 11.5 }}>{u.email}</div></div>
                    </div></td>
                    <td><span className={`pill ${admin ? "pill-navy" : "pill-info"}`}>{u.userType || u.role}</span></td>
                    <td style={{ fontSize: 12.5 }}>{u.company || "—"}</td>
                    <td style={{ textAlign: "center" }} className="mono">{u.leads}</td>
                    <td><span className={`pill ${u.status === "Active" ? "pill-good" : "pill-slate"}`}><span className="pdot" />{u.status}</span></td>
                    <td><div className="row-actions">
                      <button className="act" title="Edit" onClick={() => navigate(`/users/${u.id}/edit`)}><Icon name="note" /></button>
                      <button className="act" title="Permissions" onClick={() => toast("Manage permissions")}><Icon name="shield" /></button>
                      <button className="act reject" title="Disable" onClick={() => { if (window.confirm(`Disable ${u.name}?`)) del.mutate(u.id); }}><Icon name="del" /></button>
                    </div></td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>
    </>
  );
}
