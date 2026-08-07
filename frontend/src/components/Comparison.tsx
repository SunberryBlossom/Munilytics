import { useEffect, useState } from "react";
import {
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { loadComparison, type Kpi, type Series } from "../cube";

type Props = {
  kpi: Kpi | null;
  municipalities: string[];
};

// Blue-family palette; dash patterns carry the distinction when hues get close.
const COLORS = ["#000080", "#0066cc", "#009999", "#663399", "#003366", "#3366ff"];
const DASHES = ["", "6 3", "2 2", "8 3 2 3", "4 4", "1 3"];

const GENDERS = [
  { code: "T", label: "TOTAL" },
  { code: "K", label: "FEMALE" },
  { code: "M", label: "MALE" },
];

export function Comparison({ kpi, municipalities }: Props) {
  const [gender, setGender] = useState("T");
  const [data, setData] = useState<Series[]>([]);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!kpi || municipalities.length === 0) {
      setData([]);
      return;
    }
    setBusy(true);
    loadComparison(kpi.code, municipalities, gender)
      .then((rows) => {
        setData(rows);
        setError("");
      })
      .catch((e: Error) => setError(e.message))
      .finally(() => setBusy(false));
  }, [kpi, municipalities, gender]);

  const fmt = (v: number | undefined) =>
    v == null ? "-" : v.toLocaleString("sv-SE", { maximumFractionDigits: 2 });

  return (
    <div className="panel">
      <h2>3. COMPARISON</h2>
      <div className="panel-body">
        <div className="toolbar">
          <span className="note">GENDER:</span>
          {GENDERS.map((g) => (
            <button
              key={g.code}
              onClick={() => setGender(g.code)}
              disabled={gender === g.code}
            >
              {g.label}
            </button>
          ))}
        </div>

        {kpi ? (
          <div className="note">
            <strong>{kpi.title}</strong>
            <br />
            {kpi.code} | UNIT: {kpi.unit} | {municipalities.length} MUNICIPALITY(S)
          </div>
        ) : (
          <div className="note">NO INDICATOR SELECTED</div>
        )}

        {error && <div className="empty">ERROR: {error}</div>}

        {!error && busy && <div className="empty">QUERYING...</div>}

        {!error && !busy && data.length === 0 && (
          <div className="empty">
            {!kpi
              ? "SELECT AN INDICATOR TO BEGIN"
              : municipalities.length === 0
                ? "SELECT AT LEAST ONE MUNICIPALITY"
                : "NO REPORTED DATA FOR THIS COMBINATION"}
          </div>
        )}

        {!error && !busy && data.length > 0 && (
          <>
            <div style={{ width: "100%", height: 300 }}>
              <ResponsiveContainer>
                <LineChart data={data} margin={{ top: 8, right: 16, bottom: 4, left: 8 }}>
                  <CartesianGrid stroke="#ccccdd" />
                  <XAxis
                    dataKey="year"
                    tick={{ fontSize: 11, fontFamily: "Courier New" }}
                    stroke="#000080"
                  />
                  <YAxis
                    tick={{ fontSize: 11, fontFamily: "Courier New" }}
                    stroke="#000080"
                    width={70}
                  />
                  <Tooltip
                    formatter={(v) => fmt(typeof v === "number" ? v : undefined)}
                    contentStyle={{
                      border: "2px solid #000080",
                      borderRadius: 0,
                      fontFamily: "Courier New",
                      fontSize: 11,
                    }}
                  />
                  <Legend wrapperStyle={{ fontSize: 11, fontFamily: "Courier New" }} />
                  {municipalities.map((m, i) => (
                    <Line
                      key={m}
                      type="linear"
                      dataKey={m}
                      stroke={COLORS[i % COLORS.length]}
                      strokeDasharray={DASHES[i % DASHES.length]}
                      strokeWidth={2}
                      dot={false}
                      connectNulls
                    />
                  ))}
                </LineChart>
              </ResponsiveContainer>
            </div>

            <div className="table-scroll">
              <table>
                <thead>
                  <tr>
                    <th>YEAR</th>
                    {municipalities.map((m) => (
                      <th key={m}>{m.toUpperCase()}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {data.map((row) => (
                    <tr key={row.year}>
                      <td>{row.year}</td>
                      {municipalities.map((m) => (
                        <td key={m}>{fmt(row[m])}</td>
                      ))}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
