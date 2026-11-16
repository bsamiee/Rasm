import { main } from '@rasm/testing/storybook';
import { defineMain } from '@storybook/react-vite/node';
import tailwindcss from '@tailwindcss/vite';
import react from '@vitejs/plugin-react';
import { mergeConfig } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineMain({
    ...main,
    framework: { name: '@storybook/react-vite', options: { builder: { viteConfigPath: null } } },
    stories: ['../site/*.stories.ts'],
    viteFinal: (config) => mergeConfig(config, { plugins: [react({ compiler: { panicThreshold: 'all_errors' } }), tailwindcss()] }),
});
