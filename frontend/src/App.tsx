import { useEffect, useState } from "react";
import { Comparison } from "./components/Comparison";
import { KpiSearch } from "./components/KpiSearch";
import { MunicipalityPicker } from "./components/MunicipalityPicker";
import { loadKpiCodesWithData, type Kpi } from "./cube";

function App() {
  const [kpi, setKpi] = useState<Kpi | null>(null);
  const [municipalities, setMunicipalities] = useState<string[]>([]);
  const [codesWithData, setCodesWithData] = useState<Set<string> | null>(null);
  const [catalogError, setCatalogError] = useState("");

  useEffect(() => {
    loadKpiCodesWithData()
      .then(setCodesWithData)
      .catch((e: Error) => setCatalogError(e.message));
  }, []);

  return (
    <>
      <div className="masthead">
        <span className="title">MUNILYTICS</span>
        <span className="sub">KOMMUN KPI ANALYSIS TERMINAL</span>
      </div>

      <div className="layout">
        <div className="col-left">
          <KpiSearch selected={kpi} onSelect={setKpi} codesWithData={codesWithData} />
          <MunicipalityPicker selected={municipalities} onChange={setMunicipalities} />
        </div>
        <div className="col-right">
          <Comparison kpi={kpi} municipalities={municipalities} />
        </div>
      </div>

      <div className="status">
        SOURCE: KOLADA VIA CUBE |{" "}
        {catalogError
          ? `CATALOG ERROR: ${catalogError}`
          : codesWithData
            ? `${codesWithData.size} INDICATORS WITH DATA`
            : "LOADING CATALOG..."}{" "}
        | INDICATOR: {kpi?.code ?? "NONE"} | MUNICIPALITIES: {municipalities.length}
      </div>
    </>
  );
}

export default App;
