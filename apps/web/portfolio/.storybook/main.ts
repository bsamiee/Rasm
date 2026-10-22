import { defineMain } from '@storybook/react-vite/node';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineMain({ framework: '@storybook/react-vite', stories: ['./*.stories.ts'], addons: ['@rasm/test-support/preset'] });
