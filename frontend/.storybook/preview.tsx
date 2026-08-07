import type { Preview } from '@storybook/react-vite'
import type { Decorator } from '@storybook/react'
import '../src/styles/globals.css'
import { ThemeProvider } from '@/components/theme-provider'

const withTheme: Decorator = (Story, context) => {
  return (
    <ThemeProvider defaultTheme="dark" storageKey="vite-ui-theme">
      <Story {...context} />
    </ThemeProvider>
  )
}

const preview: Preview = {
  parameters: {
    controls: {
      matchers: {
        color: /(background|color)$/i,
        date: /Date$/i,
      },
    },
    a11y: {
      test: 'todo',
    },
  },
  decorators: [withTheme],
}

export default preview
