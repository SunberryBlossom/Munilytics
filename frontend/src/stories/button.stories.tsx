import type { Meta, StoryObj } from '@storybook/react-vite'
import { Button } from '../components/ui/button'

const meta: Meta<typeof Button> = {
  title: 'UI/Button',
  component: Button,
  tags: ['autodocs'],
}

export default meta
type Story = StoryObj<typeof Button>

export const Primary: Story = {
  args: {
    children: 'Click Me',
    variant: 'default',
  },
}

export const Secondary: Story = {
  args: {
    children: 'Cancel',
    variant: 'outline',
  },
}