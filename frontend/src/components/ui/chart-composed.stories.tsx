import type { Meta, StoryObj } from '@storybook/react-vite';
import { ChartComposed } from './chart-composed';

type SampleRow = {
    category: string;
    metric1: number;
    metric2: number;
    metric3: number;
    metric4: number;
};

const testData: SampleRow[] = [
    { category: 'En kommun', metric1: 400, metric2: 240, metric3: 240, metric4: 200 },
    { category: 'Annan', metric1: 300, metric2: 139, metric3: 221, metric4: 180 },
    { category: 'Tredje', metric1: 200, metric2: 523, metric3: 229, metric4: 430 },
    { category: 'Fjärde', metric1: 278, metric2: 390, metric3: 200, metric4: 50 },
];

const TypedChart = ChartComposed<SampleRow>;

const meta: Meta<typeof TypedChart> = {
    title: 'Charts/Chart - Composed',
    component: TypedChart,
};

export default meta;

type Story = StoryObj<typeof TypedChart>;

export const Default: Story = {
    args: {
        data: testData,
        xKey: 'category',
        series: [
            {
                type: 'area',
                dataKey: 'metric1',
                label: 'Snittbetyg',
                color: '#8884d8',
            },
            {
                type: 'bar',
                dataKey: 'metric2',
                label: 'Elevantal',
                color: '#413ea0',
            },
            {
                type: 'line',
                dataKey: 'metric3',
                label: 'Rikets snittbetyg',
                color: '#ff7300',
            },
            {
                type: 'scatter',
                dataKey: 'metric4',
                label: 'Jag vet inte ens',
                color: 'red',
            },
        ],
    },
};