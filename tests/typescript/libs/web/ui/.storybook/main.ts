import { defineMain } from '@storybook/react-vite/node';
import react from '@vitejs/plugin-react';
import { mergeConfig } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineMain({
    framework: '@storybook/react-vite',
    addons: ['@rasm/test-support/storybook', '@rasm/nx-storybook/preset'],
    stories: ['./*.stories.@(ts|tsx)'],
    viteFinal: (config) => mergeConfig(config, { plugins: [react({ compiler: { panicThreshold: 'all_errors' } })] }),
});
