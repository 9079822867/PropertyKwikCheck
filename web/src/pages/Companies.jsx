import { useState } from "react";
import { useCompanies, useCreateCompany } from "../lib/queries.js";
import { Spinner, ErrorBox, Pill } from "../components/ui.jsx";

const TYPES = ["Lender · Bank", "Lender · NBFC", "Lender · Co-op Bank", "Valuation Agency · Owner"];
const blank = { name: "", type: TYPES[0], spoc: "", status: "Active" };

export default function Companies() {
  const { data: companies, isLoading, error } = useCompanies();
  const create = useCreateCompany();
  const [open, setOpen] = useState(false);
  const [form, setForm] = useState(blank);
  const [err, setErr] = useState(null);

  const set = (k, v) => setForm((f) => ({ ...f, [k]: v }));
  const inp = { width: "100%", height: 38, border: "1px solid var(--line)", borderRadius: 8, padding: "0 11px", background: "var(--slate-50)" };

  async function submit(e) {
    e.preventDefault(); setErr(null);
    try { await create.mutateAsync(form); setForm(blank); setOpen(false); } catch (e2) { setErr(e2); }
  }

  return (
    <>
      <div className="page-head">
        <div><h1>Companies</h1><div className="sub">{companies?.length ?? 0} companies</div></div>
        <button className="btn btn-primary" onClick={() => setOpen((o) => !o)}>{open ? "Close" : "New Company"}</button>
      </div>

      {open && (
        <form className="card card-pad" style={{ marginBottom: 16 }} onSubmit={submit}>
          {err && <ErrorBox error={err} />}
          <div className="grid grid-3" style={{ gap: 14 }}>
            <div className="field" style={{ margin: 0 }}><label>Name</label><input style={inp} value={form.name} onChange={(e) => set("name", e.target.value)} required /></div>
            <div className="field" style={{ margin: 0 }}><label>Type</label>
              <select style={inp} value={form.type} onChange={(e) => set("type", e.target.value)}>{TYPES.map((t) => <option key={t}>{t}</option>)}</select>
            </div>
            <div className="field" style={{ margin: 0 }}><label>SPOC</label><input style={inp} value={form.spoc} onChange={(e) => set("spoc", e.target.value)} /></div>
          </div>
          <button className="btn btn-primary" style={{ marginTop: 16 }} disabled={create.isPending}>{create.isPending ? "Creating…" : "Create Company"}</button>
        </form>
      )}

      {isLoading ? <Spinner /> : error ? <ErrorBox error={error} /> : (
        <div className="card">
          <table className="table">
            <thead><tr><th>Name</th><th>Type</th><th>SPOC</th><th>Leads</th><th>Active</th><th>Status</th></tr></thead>
            <tbody>
              {companies.map((c) => (
                <tr key={c.id} style={{ cursor: "default" }}>
                  <td style={{ fontWeight: 600, color: "var(--navy)" }}>{c.name}</td>
                  <td>{c.type}</td><td>{c.spoc || "—"}</td><td className="mono">{c.leads}</td><td className="mono">{c.active}</td>
                  <td><Pill tone={c.status === "Active" ? "good" : "slate"}>{c.status}</Pill></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  );
}
