import { LineChartSingular } from "./linechart-singular";
import type { Meta, StoryObj } from '@storybook/react-vite';

const meta: Meta<typeof LineChartSingular> = {
    title: "Charts/LineChart - Singular",
    component: LineChartSingular,
};

export default meta;

type Story = StoryObj<typeof LineChartSingular>;

const testData = [
    { name: "First", value: 10 },
    { name: "Andra", value: 25 },
    { name: "Sanbanme", value: 45 },
    { name: "Vierte", value: 40 },
    { name: "Quinta", value: 65 },
];

export const Default: Story = {
    args: {
        data: testData,
    },
};