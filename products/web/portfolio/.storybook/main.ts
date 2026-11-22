import { main } from '@rasm/testing/storybook';
import { defineMain } from '@storybook/react-vite/node';
import tailwindcss from '@tailwindcss/vite';
import { mergeConfig } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineMain({
    ...main,
    framework: { name: '@storybook/react-vite', options: { builder: { viteConfigPath: null } } },
    stories: ['../site/*.stories.ts'],
    viteFinal: (config) => mergeConfig(main.viteFinal(config), { plugins: [tailwindcss()] }),
});
