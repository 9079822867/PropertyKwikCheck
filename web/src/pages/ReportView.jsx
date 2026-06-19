import { Fragment } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { usePublicReport } from "../lib/queries.js";
import { useAuth } from "../lib/auth.jsx";
import "./report.css";

const ratingClass = (r) => `kc-rating ${String(r || "").toLowerCase()}`;

function Brandbar({ cover, meta, pageNo, pageLabel }) {
  return (
    <div className="kc-brandbar">
      <div>
        <div className="b-name">{cover?.brand || "KWIKCHECK"}</div>
        <div className="b-tag">{cover?.tagline}</div>
      </div>
      <div className="b-right">
        <b>{meta?.reportNo}</b>
        {pageLabel} · Page {pageNo} / 05
      </div>
    </div>
  );
}

const Foot = ({ label }) => (
  <div className="kc-foot">
    <span>NGD Kwik Check Pvt. Ltd.</span>
    <span>{label}</span>
    <span>Confidential</span>
  </div>
);

function KV({ items, cols, render }) {
  return (
    <div className={`kc-kv ${cols === 2 ? "cols-2" : cols === 4 ? "cols-4" : ""}`}>
      {(items || []).map((it, i) => (
        <div key={i}>
          <div className="k">{it.label}</div>
          <div className="v">{render ? render(it) : it.value}</div>
        </div>
      ))}
    </div>
  );
}

export default function ReportView() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();
  const { data: r, isLoading, error } = usePublicReport(id);

  if (isLoading) return <div className="kc-report"><div className="kc-loading">Loading report…</div></div>;
  if (error) return <div className="kc-report"><div className="kc-error">{error?.error || "Report not found."}</div></div>;

  const { meta = {}, cover = {}, customer = {}, plot = {}, snapshot = [], inspection = {},
    claim = {}, conditions = {}, photos = {}, summary = {} } = r || {};
  const val = summary.valuation || {};

  return (
    <div className="kc-report">
      {/* toolbar — hidden on print */}
      <div className="kc-toolbar">
        <div>
          <div className="tb-title">{cover.propertyName || "Property Inspection Report"}</div>
          <div className="tb-no">{meta.reportNo}</div>
        </div>
        <div className="spacer" />
        {meta.verified && <span className="kc-badge-verified">● Verified &amp; Issued</span>}
        {user && <button className="kc-btn kc-btn-ghost" onClick={() => navigate(-1)}>← Back</button>}
        <button className="kc-btn kc-btn-primary" onClick={() => window.print()}>⬇ Download PDF</button>
      </div>

      {/* ───────── PAGE 1 — Cover + Customer + Plot + Snapshot + Inspection ───────── */}
      <section className="kc-page">
        <Brandbar cover={cover} meta={meta} pageNo="01" pageLabel="Property Inspection Report" />

        <div className="kc-hero">
          <div className="kicker">{cover.kicker || "PROPERTY INSPECTION REPORT"}</div>
          <h1>{cover.propertyName}</h1>
          <div className="subtitle">{cover.subtitle}</div>
          {cover.address && <div className="addr">{cover.address}</div>}
          {cover.addressAsPerDoc && <div className="addr-doc">As per document — {cover.addressAsPerDoc}</div>}
        </div>

        <div className="kc-hero-meta">
          <div><div className="l">Inspection Date</div><div className="v">{cover.inspectionDate}</div></div>
          <div><div className="l">Property Class</div><div className="v">{cover.propertyClass}</div></div>
          <div><div className="l">Inspection Type</div><div className="v">{cover.inspectionType}</div></div>
          <div><div className="l">Report Status</div><div className="v">{cover.reportStatus}</div></div>
        </div>

        <div className="kc-valuation-hero">
          <div className="l">{cover.valuationLabel || "Fair Market Value — adopted"}</div>
          <div className="amt">{cover.valuationAmount}</div>
          <div className="w">{cover.valuationWords}</div>
        </div>

        <div className="kc-block-title">Customer Information</div>
        <KV cols={3} items={[
          { label: "Applicant Name", value: customer.applicantName },
          { label: "Co-Applicant Name", value: customer.coApplicantName },
          { label: "Contact Number", value: customer.contactNumber },
          { label: "Person Met at Site", value: customer.personMet },
          { label: "Relationship", value: customer.relationship },
          { label: "Documents Shown", value: customer.documentsShown },
        ]} />

        <div className="kc-block-title">Plot Dimensions <span className="hint">N · S · E · W</span></div>
        <div className="kc-plot">
          <div><div className="k">North</div><div className="v">{plot.north}</div></div>
          <div><div className="k">South</div><div className="v">{plot.south}</div></div>
          <div><div className="k">East</div><div className="v">{plot.east}</div></div>
          <div><div className="k">West</div><div className="v">{plot.west}</div></div>
          <div className="total"><div className="k">Total Plot Area</div><div className="v">{plot.totalArea}</div></div>
        </div>

        <div className="kc-block-title">Property Snapshot</div>
        <div className="kc-cards">
          {snapshot.map((c, i) => (
            <div className="c" key={i}>
              <div className="l">{c.label}</div><div className="v">{c.value}</div><div className="n">{c.note}</div>
            </div>
          ))}
        </div>

        <div className="kc-block-title">Inspection &amp; Verification</div>
        <KV cols={3} items={[
          { label: "Inspected By", value: `${inspection.inspectedBy || ""}${inspection.inspectedByTitle ? " · " + inspection.inspectedByTitle : ""}` },
          { label: "Licence ID", value: inspection.licenceId },
          { label: "Field Date", value: inspection.fieldDate },
          { label: "Requested By", value: inspection.requestedBy },
          { label: "Branch", value: inspection.branch },
          { label: "Claim Number", value: inspection.claimNumber },
          { label: "Lead ID", value: inspection.leadId },
          { label: "Verify At", value: inspection.verifyUrl },
        ]} />

        <Foot label="Property Inspection Report" />
      </section>

      {/* ───────── PAGE 2 — Claim Report ───────── */}
      <section className="kc-page">
        <Brandbar cover={cover} meta={meta} pageNo="02" pageLabel="Claim Report" />
        <div className="kc-section-head">
          <div className="eyebrow">Section 02 — Claim Assessment</div>
          <h2>Claim Report</h2>
          <p>Verified facilities, community amenities, locality connectivity and structural condition recorded against the lending claim. Claim No. {claim.claimNo}</p>
        </div>

        <KV cols={3} items={claim.flags} />

        <div className="kc-block-title">Facilities <span className="hint">In-Unit &amp; Allotted</span></div>
        <KV cols={2} items={claim.facilities} render={(it) => (
          <>{it.value}{it.tag && <span className="tag">{it.tag}</span>}</>
        )} />

        <div className="kc-block-title">Community Amenities <span className="hint">{claim.amenitiesSummary}</span></div>
        <div className="kc-amenities">
          {(claim.amenities || []).map((a, i) => (
            <div className={`a ${a.available ? "yes" : "no"}`} key={i}>
              <span className="dot" />{a.name}
              <span className="st">{a.available ? "Available" : "Not Provided"}</span>
            </div>
          ))}
        </div>

        <div className="kc-block-title">Locality <span className="hint">Classification</span></div>
        <KV cols={3} items={claim.locality} />

        <div className="kc-block-title">Connectivity <span className="hint">Civic Infrastructure</span></div>
        <KV cols={3} items={claim.connectivity} render={(it) => (
          <>{it.value}{it.dist && <span className="dist">{it.dist}</span>}</>
        )} />

        <div className="kc-block-title">Structural Condition <span className="hint">Building &amp; Finishes</span></div>
        <KV cols={3} items={claim.structural} render={(it) => (
          <>{it.value} <span className={ratingClass(it.rating)}>{it.rating}</span></>
        )} />

        <Foot label="Claim Report" />
      </section>

      {/* ───────── PAGE 3 — Condition Assessment ───────── */}
      <section className="kc-page">
        <Brandbar cover={cover} meta={meta} pageNo="03" pageLabel="Condition Assessment" />
        <div className="kc-section-head">
          <div className="eyebrow">Section 03 — Component Inspection</div>
          <h2>Condition Assessment</h2>
          <p>{conditions.summary} — {conditions.summaryNote}. Each rated against Kwik Check's 142-point inspection protocol.</p>
        </div>

        <div className="kc-scorebar">
          {(conditions.scores || []).map((s, i) => (
            <div className="s" key={i}>
              <div className="l">{s.label}</div>
              <div className="v">{s.score}</div>
              <span className={ratingClass(s.rating)}>{s.rating}</span>
            </div>
          ))}
        </div>

        <table className="kc-table">
          <tbody>
            {(conditions.categories || []).map((cat, ci) => (
              <Fragment key={`c${ci}`}>
                <tr className="cat">
                  <td colSpan={3}>{cat.name}<span className="sc">Category {cat.score}</span></td>
                </tr>
                {cat.components.map((cmp, i) => (
                  <tr key={`c${ci}-${i}`}>
                    <td className="cmp">{cmp.name}</td>
                    <td className="obs">{cmp.observation}</td>
                    <td className="rt"><span className={ratingClass(cmp.rating)}>{cmp.rating}</span></td>
                  </tr>
                ))}
              </Fragment>
            ))}
          </tbody>
        </table>

        {conditions.priorityNote && (
          <div className="kc-priority">
            <div className="t">Priority Attention</div>
            <p>{conditions.priorityNote}</p>
          </div>
        )}

        <Foot label="Condition Assessment" />
      </section>

      {/* ───────── PAGE 4 — Photo Documentation ───────── */}
      <section className="kc-page">
        <Brandbar cover={cover} meta={meta} pageNo="04" pageLabel="Photo Documentation" />
        <div className="kc-section-head">
          <div className="eyebrow">Section 04 — Visual Evidence</div>
          <h2>Photo Documentation</h2>
          <p>{photos.summary} · Captured {photos.capturedOn}. Date-stamped photographs supporting the condition findings recorded in this report.</p>
        </div>

        {(photos.groups || []).map((g, gi) => (
          <div key={gi}>
            <div className="kc-block-title">{g.title} <span className="hint">{g.count}</span></div>
            <div className="kc-photos">
              {g.items.map((p, i) => (
                <div className="p" key={i}>
                  <div className="frame">
                    <span className="ph-tag">{p.tag}</span>
                    <span className="ph-no">{p.no}</span>
                    <span className="ph-stamp">{p.stamp}</span>
                  </div>
                  <div className="meta"><div className="t">{p.no} · {p.title}</div><div className="n">{p.note}</div></div>
                </div>
              ))}
            </div>
          </div>
        ))}

        {photos.footnote && <div className="kc-note">{photos.footnote}</div>}
        <Foot label="Photo Documentation" />
      </section>

      {/* ───────── PAGE 5 — Summary & Sign-off ───────── */}
      <section className="kc-page">
        <Brandbar cover={cover} meta={meta} pageNo="05" pageLabel="Summary &amp; Sign-off" />
        <div className="kc-section-head">
          <div className="eyebrow">Section 05 — Conclusion &amp; Certification</div>
          <h2>Summary &amp; Sign-off</h2>
          <p>Inspector's conclusion, prioritised recommendations and formal certification of this inspection. Issued {summary.issuedOn}.</p>
        </div>

        <div className="kc-final">
          <div className="f fmv">
            <div className="l">Fair Market Value — Adopted</div>
            <div className="amt">{val.fairMarketValue}</div>
            <div className="n">{val.fairMarketWords}</div>
          </div>
          <div className="f alt">
            <div className="l">{val.distressLabel || "Distress Value"}</div>
            <div className="amt">{val.distressValue}</div>
            <div className="n">{val.distressNote}</div>
          </div>
          <div className="f alt">
            <div className="l">Govt. DLC Value</div>
            <div className="amt">{val.govtDlcValue}</div>
            <div className="n">{val.govtDlcNote}</div>
          </div>
        </div>

        <div className="kc-block-title">Valuation Arrival <span className="hint">DLC / Circle Rate · Market Value</span></div>
        <div className="kc-kv cols-2" style={{ alignItems: "start" }}>
          {[val.dlcMethod, val.marketMethod].filter(Boolean).map((m, i) => (
            <div className="kc-method" key={i}>
              <div className="mh"><span className="t">{m.title}</span><span className="tag">{m.tag}</span></div>
              {(m.rows || []).map((row, j) => (
                <div className="row" key={j}><span className="lbl">{row.label}</span><span className="val">{row.value}</span></div>
              ))}
              <div className="row total"><span className="lbl">Valuation</span><span className="val">{m.total}</span></div>
            </div>
          ))}
        </div>

        <div className="kc-block-title">Inspector's Summary</div>
        <div className="kc-summary-box">
          <p>{summary.inspectorSummary}</p>
          {summary.finalGrade && <span className="kc-grade">Final Assessment — {summary.finalGrade}</span>}
          <div style={{ marginTop: 10, fontSize: 12, fontWeight: 700, color: "var(--navy)" }}>{summary.summaryBy}</div>
          <div style={{ fontSize: 11, color: "var(--muted)" }}>{summary.summaryByNote}</div>
        </div>

        {(summary.scope || summary.disclaimer) && (
          <div style={{ marginTop: 16 }}>
            <div className="kc-prose" style={{ marginBottom: 10 }}><b>Scope &amp; Methodology — </b>{summary.scope}</div>
            <div className="kc-prose"><b>Disclaimer — </b>{summary.disclaimer}</div>
          </div>
        )}

        <div className="kc-block-title">Certification &amp; Authorisation</div>
        <div className="kc-cert">
          {(summary.certification || []).map((c, i) => (
            <div className="c" key={i}>
              <div className="role">{c.role}</div>
              <div className="sigline" />
              <div className="nm">{c.name}</div>
              <div className="ti">{c.title}</div>
              <div className="lic">{c.licence}</div>
            </div>
          ))}
        </div>

        {summary.contact && (
          <div className="kc-contact">
            <div className="c"><div className="l">Call</div><div className="v">{summary.contact.call}</div></div>
            <div className="c"><div className="l">Email</div><div className="v">{summary.contact.email}</div></div>
            <div className="c"><div className="l">Web</div><div className="v">{summary.contact.web}</div></div>
            <div className="c"><div className="l">Office</div><div className="v">{summary.contact.office}</div></div>
          </div>
        )}

        <Foot label="End of Report" />
      </section>
    </div>
  );
}
