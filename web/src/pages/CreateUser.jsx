import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useRoles, useUserTypes, useCompanies, useCreateUser, useUpdateUser, useUsers } from "../lib/queries.js";
import { ErrorBox } from "../components/ui.jsx";
import Icon from "../components/Icon.jsx";

const blank = { name: "", email: "", phone: "", roleId: "", userTypeId: "", companyId: "", licenceNo: "", password: "", status: "Active" };
const inp = { width: "100%", height: 40, border: "1px solid var(--line)", borderRadius: 8, padding: "0 11px", background: "var(--slate-50)", fontSize: 13.5 };

// Workflow stages shown in the permissions panel, mapped to the user types that own them.
const STAGES = [
  ["Intake & Records", "State Coordinator, Client", (t) => /Coordinator|Client|Admin|Super/.test(t)],
  ["Technical & Risk", "RO Valuators, Cando Valuator", (t) => /Valuator|VALUATOR|Admin|Super/.test(t)],
  ["QC Review", "Qc Manager", (t) => /Qc Manager|Admin|Super/.test(t)],
  ["Pricing", "Pricing Manager", (t) => /Pricing|Admin|Super/.test(t)],
  ["Valuation Sign-off", "National / Business Head", (t) => /National|Business|Super/.test(t)],
  ["All stages", "Super Admin / Admin", (t) => /Super Admin|^Admin$/.test(t)],
];

export default function CreateUser() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEdit = id != null;
  const { data: roles } = useRoles();
  const { data: types } = useUserTypes();
  const { data: companies } = useCompanies();
  const { data: users } = useUsers();
  const create = useCreateUser();
  const update = useUpdateUser(id);
  const saving = isEdit ? update.isPending : create.isPending;
  const [form, setForm] = useState(blank);
  const [err, setErr] = useState(null);
  const set = (k, v) => setForm((f) => ({ ...f, [k]: v }));

  // On edit, prefill from the list query. The list DTO carries names, so map
  // role/userType/company back to their ids via the lookup queries.
  useEffect(() => {
    if (!isEdit || !users || !roles || !types) return;
    const u = users.find((x) => String(x.id) === String(id));
    if (!u) return;
    setForm({
      name: u.name || "", email: u.email || "", phone: u.phone || "",
      roleId: String(roles.find((r) => r.roleName === u.role)?.id ?? ""),
      userTypeId: String(types.find((t) => t.name === u.userType)?.id ?? ""),
      companyId: String((companies || []).find((c) => c.name === u.company)?.id ?? ""),
      licenceNo: u.licenceNo || "", password: "", status: u.status || "Active",
    });
  }, [isEdit, id, users, roles, types, companies]);

  const typeName = (types || []).find((t) => String(t.id) === String(form.userTypeId))?.name || "";

  async function submit() {
    setErr(null);
    if (!form.name || !form.email || !form.roleId || !form.userTypeId) { setErr({ error: "Name, email, role and user type are required." }); return; }
    try {
      if (isEdit) {
        await update.mutateAsync({
          name: form.name, phone: form.phone || null,
          roleId: Number(form.roleId), userTypeId: Number(form.userTypeId),
          companyId: form.companyId ? Number(form.companyId) : null,
          licenceNo: form.licenceNo || null, status: form.status,
          password: form.password || null,
        });
      } else {
        await create.mutateAsync({
          name: form.name, email: form.email, phone: form.phone || null,
          roleId: Number(form.roleId), userTypeId: Number(form.userTypeId),
          companyId: form.companyId ? Number(form.companyId) : null,
          licenceNo: form.licenceNo || null, status: form.status, password: form.password || null,
        });
      }
      navigate("/users");
    } catch (e) { setErr(e); }
  }

  const Field = ({ label, k, type = "text", col2, disabled }) => (
    <div className={`field ${col2 ? "col2" : ""}`} style={{ margin: 0 }}>
      <label>{label}</label>
      <input type={type} style={{ ...inp, ...(disabled ? { opacity: 0.6, cursor: "not-allowed" } : {}) }}
        value={form[k]} disabled={disabled} onChange={(e) => set(k, e.target.value)} />
    </div>
  );

  return (
    <>
      <div className="crumbs"><a onClick={() => navigate("/")}>Home</a><span className="sep">/</span><span>User Management</span><span className="sep">/</span><span style={{ color: "var(--navy)" }}>{isEdit ? "Edit User" : "Create New User"}</span></div>
      <div className="page-head">
        <div><h1>{isEdit ? "Edit User" : "Create New User"}</h1><div className="sub">{isEdit ? "Update this team member's role and access." : "Add a team member and grant a workflow role."}</div></div>
        <button className="btn btn-ghost" onClick={() => navigate("/users")}>Back</button>
      </div>

      {err && <ErrorBox error={err} />}

      <div className="dash-2" style={{ gridTemplateColumns: "1.6fr 1fr", marginTop: 0 }}>
        {/* step 1 — user details */}
        <div className="card card-pad">
          <div className="fs-title"><div className="n">1</div><h3>User Details</h3></div>
          <div className="fs-sub">Identity, role and login.</div>
          <div className="g2">
            <Field label="Full Name" k="name" />
            <Field label="Email" k="email" type="email" disabled={isEdit} />
            <Field label="Phone" k="phone" />
            <Field label="Licence No." k="licenceNo" />
            <div className="field" style={{ margin: 0 }}><label>Role</label>
              <select style={inp} value={form.roleId} onChange={(e) => set("roleId", e.target.value)}>
                <option value="">— role —</option>{(roles || []).map((r) => <option key={r.id} value={r.id}>{r.roleName}</option>)}
              </select>
            </div>
            <div className="field" style={{ margin: 0 }}><label>Workflow User Type</label>
              <select style={inp} value={form.userTypeId} onChange={(e) => set("userTypeId", e.target.value)}>
                <option value="">— type —</option>{(types || []).map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
              </select>
            </div>
            <div className="field" style={{ margin: 0 }}><label>Company</label>
              <select style={inp} value={form.companyId} onChange={(e) => set("companyId", e.target.value)}>
                <option value="">— none —</option>{(companies || []).map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
            </div>
            <Field label={isEdit ? "Reset Password (optional)" : "Initial Password (optional)"} k="password" type="password" />
            <div className="field col2" style={{ margin: 0 }}>
              <label>Status</label>
              <div className="seg">
                <button type="button" className={form.status === "Active" ? "on" : ""} onClick={() => set("status", "Active")}>Active</button>
                <button type="button" className={form.status === "Inactive" ? "on" : ""} onClick={() => set("status", "Inactive")}>Inactive</button>
              </div>
            </div>
          </div>
        </div>

        {/* step 2 — stage permissions (role-derived) */}
        <div className="card card-pad">
          <div className="fs-title"><div className="n">2</div><h3>Stage Permissions</h3></div>
          <div className="fs-sub">Auto-set by the selected user type.</div>
          {STAGES.map(([stage, def, test]) => (
            <label key={stage} className="perm-row">
              <input type="checkbox" readOnly checked={typeName ? test(typeName) : stage === "Intake & Records"} />
              <div><div className="pt">{stage}</div><div className="pd">Default: {def}</div></div>
            </label>
          ))}
        </div>
      </div>

      <div className="wizard-foot">
        <button className="btn btn-ghost" onClick={() => navigate("/users")}>Cancel</button>
        <button className="btn btn-primary" disabled={saving} onClick={submit}><Icon name="check" size={16} />{saving ? "Saving…" : isEdit ? "Save Changes" : "Create User"}</button>
      </div>
    </>
  );
}
