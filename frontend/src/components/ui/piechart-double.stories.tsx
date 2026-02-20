import type { Meta, StoryObj } from '@storybook/react-vite';
import { PieChartDouble } from './piechart-double';

const meta: Meta<typeof PieChartDouble> = {
    title: 'Charts/PieChart - Double',
    component: PieChartDouble,
};

export default meta;

type Story = StoryObj<typeof PieChartDouble>;

export const Default: Story = {
    args: {
        isAnimationActive: true,
    },
};