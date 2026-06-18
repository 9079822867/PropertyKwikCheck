import { useState } from "react";
import { useUsers, useRoles, useUserTypes, useCompanies, useCreateUser, useDeleteUser } from "../lib/queries.js";
import { Spinner, ErrorBox, Pill } from "../components/ui.jsx";

const blank = { name: "", email: "", roleId: "", userTypeId: "", companyId: "", phone: "", licenceNo: "", password: "" };

export default function Users() {
  const { data: users, isLoading, error } = useUsers();
  const { data: roles } = useRoles();
  const { data: types } = useUserTypes();
  const { data: companies } = useCompanies();
  const create = useCreateUser();
  const del = useDeleteUser();

  const [open, setOpen] = useState(false);
  const [form, setForm] = useState(blank);
  const [err, setErr] = useState(null);

  const set = (k, v) => setForm((f) => ({ ...f, [k]: v }));
  const inp = { width: "100%", height: 38, border: "1px solid var(--line)", borderRadius: 8, padding: "0 11px", background: "var(--slate-50)" };

  async function submit(e) {
    e.preventDefault();
    setErr(null);
    try {
      await create.mutateAsync({
        name: form.name, email: form.email,
        roleId: Number(form.roleId), userTypeId: Number(form.userTypeId),
        companyId: form.companyId ? Number(form.companyId) : null,
        phone: form.phone || null, licenceNo: form.licenceNo || null,
        password: form.password || null,
      });
      setForm(blank); setOpen(false);
    } catch (e2) { setErr(e2); }
  }

  return (
    <>
      <div className="page-head">
        <div><h1>Users</h1><div className="sub">{users?.length ?? 0} users</div></div>
        <button className="btn btn-primary" onClick={() => setOpen((o) => !o)}>{open ? "Close" : "New User"}</button>
      </div>

      {open && (
        <form className="card card-pad" style={{ marginBottom: 16 }} onSubmit={submit}>
          {err && <ErrorBox error={err} />}
          <div className="grid grid-3" style={{ gap: 14 }}>
            <div className="field" style={{ margin: 0 }}><label>Name</label><input style={inp} value={form.name} onChange={(e) => set("name", e.target.value)} required /></div>
            <div className="field" style={{ margin: 0 }}><label>Email</label><input type="email" style={inp} value={form.email} onChange={(e) => set("email", e.target.value)} required /></div>
            <div className="field" style={{ margin: 0 }}><label>Password</label><input type="password" style={inp} value={form.password} onChange={(e) => set("password", e.target.value)} placeholder="optional" /></div>
            <div className="field" style={{ margin: 0 }}><label>Role</label>
              <select style={inp} value={form.roleId} onChange={(e) => set("roleId", e.target.value)} required>
                <option value="">— role —</option>{(roles || []).map((r) => <option key={r.id} value={r.id}>{r.roleName}</option>)}
              </select>
            </div>
            <div className="field" style={{ margin: 0 }}><label>User Type</label>
              <select style={inp} value={form.userTypeId} onChange={(e) => set("userTypeId", e.target.value)} required>
                <option value="">— type —</option>{(types || []).map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
              </select>
            </div>
            <div className="field" style={{ margin: 0 }}><label>Company</label>
              <select style={inp} value={form.companyId} onChange={(e) => set("companyId", e.target.value)}>
                <option value="">— none —</option>{(companies || []).map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
            </div>
            <div className="field" style={{ margin: 0 }}><label>Phone</label><input style={inp} value={form.phone} onChange={(e) => set("phone", e.target.value)} /></div>
            <div className="field" style={{ margin: 0 }}><label>Licence No.</label><input style={inp} value={form.licenceNo} onChange={(e) => set("licenceNo", e.target.value)} /></div>
          </div>
          <button className="btn btn-primary" style={{ marginTop: 16 }} disabled={create.isPending}>{create.isPending ? "Creating…" : "Create User"}</button>
        </form>
      )}

      {isLoading ? <Spinner /> : error ? <ErrorBox error={error} /> : (
        <div className="card">
          <table className="table">
            <thead><tr><th>Name</th><th>Email</th><th>Role</th><th>User Type</th><th>Company</th><th>Status</th><th>Leads</th><th></th></tr></thead>
            <tbody>
              {users.map((u) => (
                <tr key={u.id} style={{ cursor: "default" }}>
                  <td style={{ fontWeight: 600, color: "var(--navy)" }}>{u.name}</td>
                  <td>{u.email}</td><td>{u.role}</td><td>{u.userType}</td><td>{u.company || "—"}</td>
                  <td><Pill tone={u.status === "Active" ? "good" : "slate"}>{u.status}</Pill></td>
                  <td className="mono">{u.leads}</td>
                  <td><button className="btn btn-danger btn-sm" onClick={() => del.mutate(u.id)}>Disable</button></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  );
}
