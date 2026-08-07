import cube, { type BinaryFilter } from "@cubejs-client/core";

// Aspire injects VITE_CUBE_URL; the fallback is the port AppHost publishes for local `vite dev`.
const API_URL = import.meta.env.VITE_CUBE_URL ?? "http://localhost:4000";

// Cube runs in dev mode (checkAuth is a no-op), so the token only has to be present.
export const cubeApi = cube("mysupersecret", {
  apiUrl: `${API_URL}/cubejs-api/v1`,
});

export type Kpi = {
  code: string;
  title: string;
  unit: string;
  perspective: string;
  operatingArea: string;
};

export type Municipality = {
  title: string;
  county: string;
  type: string;
};

export type Series = {
  year: number;
  [municipality: string]: number;
};

const str = (v: unknown) => (v == null ? "" : String(v));

export const KPI_SEARCH_LIMIT = 500;

export async function loadKpis(search: string, perspective: string, area: string): Promise<Kpi[]> {
  const filters: BinaryFilter[] = [];
  if (search.trim()) {
    filters.push({ member: "Kpis.title", operator: "contains", values: [search.trim()] });
  }
  if (perspective) {
    filters.push({ member: "Kpis.perspective", operator: "equals", values: [perspective] });
  }
  if (area) {
    filters.push({ member: "Kpis.operatingArea", operator: "equals", values: [area] });
  }

  const rs = await cubeApi.load({
    dimensions: [
      "Kpis.kpiCode",
      "Kpis.title",
      "Kpis.unit",
      "Kpis.perspective",
      "Kpis.operatingArea",
    ],
    filters,
    order: { "Kpis.title": "asc" },
    limit: KPI_SEARCH_LIMIT,
  });

  return rs.rawData().map((r) => ({
    code: str(r["Kpis.kpiCode"]),
    title: str(r["Kpis.title"]),
    unit: str(r["Kpis.unit"]),
    perspective: str(r["Kpis.perspective"]),
    operatingArea: str(r["Kpis.operatingArea"]),
  }));
}

export async function loadDistinct(dimension: string): Promise<string[]> {
  const rs = await cubeApi.load({ dimensions: [dimension], limit: 500 });
  return rs
    .rawData()
    .map((r) => str(r[dimension]))
    .filter(Boolean)
    .sort();
}

export async function loadMunicipalities(): Promise<Municipality[]> {
  const rs = await cubeApi.load({
    dimensions: ["Municipalities.title", "Municipalities.county", "Municipalities.type"],
    order: { "Municipalities.title": "asc" },
    limit: 1000,
  });
  return rs.rawData().map((r) => ({
    title: str(r["Municipalities.title"]),
    county: str(r["Municipalities.county"]),
    type: str(r["Municipalities.type"]),
  }));
}

/**
 * Only ~2968 of the 6138 KPIs have any measurements. Fetched once so search never
 * offers a KPI that would render an empty chart. Costs one ~2s scan on startup.
 */
export async function loadKpiCodesWithData(): Promise<Set<string>> {
  const rs = await cubeApi.load({
    dimensions: ["Kpis.kpiCode"],
    measures: ["KpiMeasurements.rows"],
    limit: 5000,
  });
  return new Set(rs.rawData().map((r) => str(r["Kpis.kpiCode"])));
}

export async function loadComparison(
  kpiCode: string,
  municipalities: string[],
  genderCode: string
): Promise<Series[]> {
  if (!kpiCode || municipalities.length === 0) return [];

  const rs = await cubeApi.load({
    measures: ["KpiMeasurements.avgValue"],
    dimensions: ["Time.year", "Municipalities.title"],
    segments: ["KpiMeasurements.reportedOnly"],
    filters: [
      { member: "Kpis.kpiCode", operator: "equals", values: [kpiCode] },
      { member: "Genders.code", operator: "equals", values: [genderCode] },
      { member: "Municipalities.title", operator: "equals", values: municipalities },
    ],
    order: { "Time.year": "asc" },
    limit: 5000,
  });

  // Cube returns one row per (year, municipality); the chart and table both want one row per year.
  const byYear = new Map<number, Series>();
  for (const r of rs.rawData()) {
    const year = Number(r["Time.year"]);
    const name = str(r["Municipalities.title"]);
    const value = Number(r["KpiMeasurements.avgValue"]);
    if (!byYear.has(year)) byYear.set(year, { year } as Series);
    byYear.get(year)![name] = value;
  }
  return [...byYear.values()].sort((a, b) => a.year - b.year);
}
