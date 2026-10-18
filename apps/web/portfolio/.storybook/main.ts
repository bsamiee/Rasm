import { defineMain } from '@storybook/react-vite/node';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineMain({ framework: '@storybook/react-vite', stories: ['../*/*.stories.tsx'], addons: ['@rasm/test-support/preset'] });
