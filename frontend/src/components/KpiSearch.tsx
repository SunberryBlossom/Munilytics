import { useEffect, useState } from "react";
import { KPI_SEARCH_LIMIT, loadDistinct, loadKpis, type Kpi } from "../cube";

type Props = {
  selected: Kpi | null;
  onSelect: (kpi: Kpi) => void;
  codesWithData: Set<string> | null;
};

export function KpiSearch({ selected, onSelect, codesWithData }: Props) {
  const [search, setSearch] = useState("");
  const [perspective, setPerspective] = useState("");
  const [area, setArea] = useState("");
  const [perspectives, setPerspectives] = useState<string[]>([]);
  const [areas, setAreas] = useState<string[]>([]);
  const [kpis, setKpis] = useState<Kpi[]>([]);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    loadDistinct("Kpis.perspective").then(setPerspectives).catch(() => setPerspectives([]));
    loadDistinct("Kpis.operatingArea").then(setAreas).catch(() => setAreas([]));
  }, []);

  // Debounced so typing doesn't fire a query per keystroke.
  useEffect(() => {
    setBusy(true);
    const timer = setTimeout(() => {
      loadKpis(search, perspective, area)
        .then((rows) => {
          setKpis(rows);
          setError("");
        })
        .catch((e: Error) => setError(e.message))
        .finally(() => setBusy(false));
    }, 300);
    return () => clearTimeout(timer);
  }, [search, perspective, area]);

  // Hide KPIs with no measurements — picking one would only ever draw an empty chart.
  const visible = codesWithData ? kpis.filter((k) => codesWithData.has(k.code)) : kpis;

  return (
    <div className="panel">
      <h2>1. SELECT INDICATOR</h2>
      <div className="panel-body">
        <label className="field">
          <span>SEARCH TITLE</span>
          <input
            type="text"
            value={search}
            placeholder="e.g. skola, kostnad, invanare"
            onChange={(e) => setSearch(e.target.value)}
          />
        </label>

        <label className="field">
          <span>PERSPECTIVE</span>
          <select value={perspective} onChange={(e) => setPerspective(e.target.value)}>
            <option value="">(all)</option>
            {perspectives.map((p) => (
              <option key={p} value={p}>
                {p}
              </option>
            ))}
          </select>
        </label>

        <label className="field">
          <span>OPERATING AREA</span>
          <select value={area} onChange={(e) => setArea(e.target.value)}>
            <option value="">(all)</option>
            {areas.map((a) => (
              <option key={a} value={a}>
                {a}
              </option>
            ))}
          </select>
        </label>

        <div className="note">
          {error
            ? `ERROR: ${error}`
            : busy
              ? "SEARCHING..."
              : `${visible.length} INDICATOR(S)${!codesWithData ? " -- LOADING CATALOG" : ""}`}
          {/* Never let a capped result set look like the whole answer. */}
          {!busy && !error && kpis.length === KPI_SEARCH_LIMIT && (
            <>
              <br />
              CAPPED AT {KPI_SEARCH_LIMIT} MATCHES -- REFINE SEARCH TO SEE MORE
            </>
          )}
        </div>

        <div className="list">
          {visible.map((k) => (
            <button
              key={k.code}
              className="row"
              aria-selected={selected?.code === k.code}
              onClick={() => onSelect(k)}
            >
              {k.title}
              <div className="meta">
                {k.code} | {k.unit} | {k.perspective || "-"}
              </div>
            </button>
          ))}
          {!busy && visible.length === 0 && <div className="empty">NO MATCHES</div>}
        </div>
      </div>
    </div>
  );
}
