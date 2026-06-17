import { useState, useEffect } from "react";
import { useSearchParams, useNavigate } from "react-router-dom";
import { useLeads } from "../lib/queries.js";
import { Spinner, ErrorBox, Pill, EmptyState } from "../components/ui.jsx";
import { BUCKET_MAP, TAT_TONE } from "../lib/constants.js";
import { inr } from "../lib/format.js";

export default function Leads() {
  const [params, setParams] = useSearchParams();
  const navigate = useNavigate();
  const bucket = params.get("bucket") || "assigned";
  const q = params.get("q") || "";
  const [search, setSearch] = useState(q);

  useEffect(() => setSearch(q), [q]);

  const { data, isLoading, error, isFetching } = useLeads(bucket, q);
  const meta = BUCKET_MAP[bucket];

  function applySearch(e) {
    e.preventDefault();
    const next = { bucket };
    if (search.trim()) next.q = search.trim();
    setParams(next);
  }

  return (
    <>
      <div className="page-head">
        <div>
          <h1>{meta?.label || "Leads"}</h1>
          <div className="sub">
            {data ? `${data.total} lead${data.total === 1 ? "" : "s"} in this bucket` : "Loading…"}
            {isFetching && " · refreshing"}
          </div>
        </div>
      </div>

      <form className="toolbar" onSubmit={applySearch}>
        <input
          className="search"
          placeholder="Search applicant, req id, lender, valuer…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <button className="btn btn-ghost btn-sm" type="submit">Search</button>
        {q && (
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={() => setParams({ bucket })}
          >
            Clear
          </button>
        )}
      </form>

      {isLoading ? (
        <Spinner label="Loading leads…" />
      ) : error ? (
        <ErrorBox error={error} />
      ) : data.rows.length === 0 ? (
        <EmptyState>No leads in this bucket{q ? ` matching “${q}”` : ""}.</EmptyState>
      ) : (
        <div className="card">
          <table className="table">
            <thead>
              <tr>
                <th>Req ID</th><th>Applicant</th><th>Type</th><th>Lender</th>
                <th>Valuator</th><th>Stage</th><th>Value</th><th>TAT</th>
              </tr>
            </thead>
            <tbody>
              {data.rows.map((l) => (
                <tr key={l.id} onClick={() => navigate(`/leads/${l.id}`)}>
                  <td className="mono">{l.reqId}</td>
                  <td>{l.applicant}</td>
                  <td>{l.ptype}</td>
                  <td>{l.lender}</td>
                  <td>{l.valuator || "—"}</td>
                  <td><Pill tone={BUCKET_MAP[l.stage]?.tone || "info"}>{BUCKET_MAP[l.stage]?.status || l.stage}</Pill></td>
                  <td>{inr(l.value)}</td>
                  <td><Pill tone={TAT_TONE[l.tatState] || "info"}>{l.tatPct}%</Pill></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  );
}
