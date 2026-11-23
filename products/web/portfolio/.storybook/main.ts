import { main } from '@rasm/testing/storybook';
import { defineMain } from '@storybook/react-vite/node';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineMain({
    ...main,
    framework: { name: '@storybook/react-vite', options: { builder: { viteConfigPath: null } } },
    stories: ['../site/*.stories.ts'],
});
