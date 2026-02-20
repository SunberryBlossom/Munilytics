import {
    LineChart,
    Line,
    CartesianGrid,
    XAxis,
    YAxis,
    Tooltip,
    ResponsiveContainer,
} from "recharts";

type LineChartSingularProps = {
    data: { name: string; value: number }[];
};

export const LineChartSingular = ({ data }: LineChartSingularProps) => {
    return (
        <ResponsiveContainer width="100%" height={300}>
            <LineChart data={data}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" />
                <YAxis />
                <Tooltip />
                <Line type="monotone" dataKey="value" stroke="#abfb57" />
            </LineChart>
        </ResponsiveContainer>
    );
};