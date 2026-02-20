import {
    ComposedChart,
    Line,
    Area,
    Bar,
    Scatter,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    Legend,
    ResponsiveContainer,
} from 'recharts';

type SeriesType = 'line' | 'bar' | 'area' | 'scatter';

type SeriesConfig = {
    type: SeriesType;
    dataKey: string;
    label: string;
    color: string;
};

type ChartComposedProps<T> = {
    data: T[];
    xKey: keyof T;
    series: SeriesConfig[];
};

export function ChartComposed<T extends Record<string, string | number>>({
    data,
    xKey,
    series,
}: ChartComposedProps<T>) {
    return (
        <ResponsiveContainer width="100%" height={400}>
            <ComposedChart data={data}>
                <CartesianGrid stroke="#f5f5f5" />
                <XAxis dataKey={xKey as string} />
                <YAxis />
                <Tooltip />
                <Legend />

                {series.map((s) => {
                    switch (s.type) {
                        case 'line':
                            return (
                                <Line
                                    key={s.dataKey}
                                    type="monotone"
                                    dataKey={s.dataKey}
                                    name={s.label}
                                    stroke={s.color}
                                />
                            );

                        case 'bar':
                            return (
                                <Bar
                                    key={s.dataKey}
                                    dataKey={s.dataKey}
                                    name={s.label}
                                    fill={s.color}
                                />
                            );

                        case 'area':
                            return (
                                <Area
                                    key={s.dataKey}
                                    type="monotone"
                                    dataKey={s.dataKey}
                                    name={s.label}
                                    stroke={s.color}
                                    fill={s.color}
                                />
                            );

                        case 'scatter':
                            return (
                                <Scatter
                                    key={s.dataKey}
                                    dataKey={s.dataKey}
                                    name={s.label}
                                    fill={s.color}
                                />
                            );

                        default:
                            return null;
                    }
                })}
            </ComposedChart>
        </ResponsiveContainer>
    );
}