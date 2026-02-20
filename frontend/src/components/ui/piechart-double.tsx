import { PieChart, Pie, Tooltip, ResponsiveContainer } from 'recharts';

const innerDataSet = [
    { name: 'Skolor', value: 400 },
    { name: 'Kebabförsäljning', value: 300 },
    { name: 'Betyg', value: 300 },
    { name: 'Lärarpoäng', value: 200 },
];
const outerDataSet = [
    { name: 'Jag', value: 100 },
    { name: 'Testar', value: 300 },
    { name: 'att', value: 100 },
    { name: 'ha', value: 80 },
    { name: 'olika', value: 40 },
    { name: 'namn', value: 30 },
    { name: 'på', value: 50 },
    { name: 'de', value: 100 },
    { name: 'separata', value: 200 },
    { name: 'elementen', value: 150 },
    { name: 'liksom', value: 50 },
];

type Props = {
    isAnimationActive?: boolean;
};

export const PieChartDouble = ({ isAnimationActive = true }: Props) => {
    return (
        <ResponsiveContainer width="100%" height={400} >
            <PieChart>
                <Pie
                    data={innerDataSet}
                    dataKey="value"
                    cx="50%"
                    cy="50%"
                    outerRadius={80}
                    fill="#abfb57"
                    isAnimationActive={isAnimationActive}
                />
                <Pie
                    data={outerDataSet}
                    dataKey="value"
                    cx="50%"
                    cy="50%"
                    innerRadius={90}
                    outerRadius={120}
                    fill="#f59555"
                    label
                    isAnimationActive={isAnimationActive}
                />
                <Tooltip />
            </PieChart>
        </ResponsiveContainer>
    );
};