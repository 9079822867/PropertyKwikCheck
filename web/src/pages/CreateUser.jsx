import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useRoles, useUserTypes, useCompanies, useCreateUser } from "../lib/queries.js";
import { ErrorBox } from "../components/ui.jsx";

const blank = { name: "", email: "", phone: "", roleId: "", userTypeId: "", companyId: "", licenceNo: "", password: "", status: "Active" };
const inp = { width: "100%", height: 40, border: "1px solid var(--line)", borderRadius: 8, padding: "0 11px", background: "var(--slate-50)", fontSize: 13.5 };

export default function CreateUser() {
  const navigate = useNavigate();
  const { data: roles } = useRoles();
  const { data: types } = useUserTypes();
  const { data: companies } = useCompanies();
  const create = useCreateUser();
  const [form, setForm] = useState(blank);
  const [err, setErr] = useState(null);
  const set = (k, v) => setForm((f) => ({ ...f, [k]: v }));

  async function submit() {
    setErr(null);
    if (!form.name || !form.email || !form.roleId || !form.userTypeId) { setErr({ error: "Name, email, role and user type are required." }); return; }
    try {
      await create.mutateAsync({
        name: form.name, email: form.email, phone: form.phone || null,
        roleId: Number(form.roleId), userTypeId: Number(form.userTypeId),
        companyId: form.companyId ? Number(form.companyId) : null,
        licenceNo: form.licenceNo || null, status: form.status, password: form.password || null,
      });
      navigate("/users");
    } catch (e) { setErr(e); }
  }

  const Field = ({ label, k, type = "text", col2 }) => (
    <div className={`field ${col2 ? "col2" : ""}`} style={{ margin: 0 }}>
      <label>{label}</label>
      <input type={type} style={inp} value={form[k]} onChange={(e) => set(k, e.target.value)} />
    </div>
  );

  return (
    <>
      <div className="crumbs"><a onClick={() => navigate("/")}>Home</a><span className="sep">/</span><span>User Management</span><span className="sep">/</span><span style={{ color: "var(--navy)" }}>Create New User</span></div>
      <div className="page-head"><div><h1>Create New User</h1><div className="sub">Add a team member and grant a workflow role.</div></div>
        <button className="btn btn-ghost" onClick={() => navigate("/users")}>Back</button></div>

      {err && <ErrorBox error={err} />}

      <div className="card card-pad" style={{ maxWidth: 760 }}>
        <div className="fs-title"><div className="n">1</div><h3>User Details</h3></div>
        <div className="fs-sub">Identity, role and login.</div>
        <div className="g2">
          <Field label="Full Name" k="name" />
          <Field label="Email" k="email" type="email" />
          <Field label="Phone" k="phone" />
          <Field label="Licence No." k="licenceNo" />
          <div className="field" style={{ margin: 0 }}><label>Role</label>
            <select style={inp} value={form.roleId} onChange={(e) => set("roleId", e.target.value)}>
              <option value="">— role —</option>{(roles || []).map((r) => <option key={r.id} value={r.id}>{r.roleName}</option>)}
            </select>
          </div>
          <div className="field" style={{ margin: 0 }}><label>User Type</label>
            <select style={inp} value={form.userTypeId} onChange={(e) => set("userTypeId", e.target.value)}>
              <option value="">— type —</option>{(types || []).map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
            </select>
          </div>
          <div className="field" style={{ margin: 0 }}><label>Company</label>
            <select style={inp} value={form.companyId} onChange={(e) => set("companyId", e.target.value)}>
              <option value="">— none —</option>{(companies || []).map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
          </div>
          <Field label="Initial Password (optional)" k="password" type="password" />
        </div>
        <div className="wizard-foot">
          <button className="btn btn-ghost" onClick={() => navigate("/users")}>Cancel</button>
          <button className="btn btn-primary" disabled={create.isPending} onClick={submit}>{create.isPending ? "Creating…" : "Create User"}</button>
        </div>
      </div>
    </>
  );
}
