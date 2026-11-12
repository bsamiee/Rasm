import { main } from '@rasm/testing/storybook';
import { defineMain } from '@storybook/react-vite/node';
import react from '@vitejs/plugin-react';
import { mergeConfig } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineMain({
    ...main,
    framework: '@storybook/react-vite',
    stories: ['../*.stories.@(ts|tsx)'],
    viteFinal: (config) => mergeConfig(config, { plugins: [react({ compiler: { panicThreshold: 'all_errors' } })] }),
});
