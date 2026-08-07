import { useEffect, useMemo, useState } from "react";
import { loadMunicipalities, type Municipality } from "../cube";

type Props = {
  selected: string[];
  onChange: (titles: string[]) => void;
};

export function MunicipalityPicker({ selected, onChange }: Props) {
  const [all, setAll] = useState<Municipality[]>([]);
  const [filter, setFilter] = useState("");
  const [error, setError] = useState("");

  // 312 rows total, so fetch once and filter in the browser.
  useEffect(() => {
    loadMunicipalities()
      .then(setAll)
      .catch((e: Error) => setError(e.message));
  }, []);

  const visible = useMemo(() => {
    const q = filter.trim().toLowerCase();
    if (!q) return all;
    return all.filter(
      (m) => m.title.toLowerCase().includes(q) || m.county.toLowerCase().includes(q)
    );
  }, [all, filter]);

  const toggle = (title: string) =>
    onChange(
      selected.includes(title) ? selected.filter((t) => t !== title) : [...selected, title]
    );

  return (
    <div className="panel">
      <h2>2. SELECT MUNICIPALITIES</h2>
      <div className="panel-body">
        <label className="field">
          <span>FILTER NAME / COUNTY</span>
          <input
            type="text"
            value={filter}
            placeholder="e.g. Stockholm, Skane"
            onChange={(e) => setFilter(e.target.value)}
          />
        </label>

        <div className="chips">
          {selected.length === 0 && <span className="note">NONE SELECTED</span>}
          {selected.map((t) => (
            <span key={t} className="chip">
              {t}
              <button onClick={() => toggle(t)} title={`Remove ${t}`}>
                x
              </button>
            </span>
          ))}
        </div>

        <div className="toolbar">
          <button onClick={() => onChange([])} disabled={selected.length === 0}>
            CLEAR
          </button>
          <span className="note">
            {error ? `ERROR: ${error}` : `${visible.length} OF ${all.length} SHOWN`}
          </span>
        </div>

        <div className="list">
          {visible.map((m) => (
            <label key={m.title} className="checkrow">
              <input
                type="checkbox"
                aria-label={m.title}
                checked={selected.includes(m.title)}
                onChange={() => toggle(m.title)}
              />
              <span>
                {m.title}
                <span className="meta"> -- {m.county || "-"}</span>
                {m.type === "L" && <span className="meta"> [REGION]</span>}
              </span>
            </label>
          ))}
          {visible.length === 0 && <div className="empty">NO MATCHES</div>}
        </div>
      </div>
    </div>
  );
}
