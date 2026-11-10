import { defineMain } from '@storybook/react-vite/node';
import { mergeConfig } from 'vite';
import application from '#/vite.config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineMain({
    framework: { name: '@storybook/react-vite', options: { builder: { viteConfigPath: null } } },
    addons: ['@rasm/test-support/storybook'],
    stories: ['./*.stories.ts'],
    viteFinal: (config, { configType }) => mergeConfig(config, application({ command: configType === 'PRODUCTION' ? 'build' : 'serve', mode: 'test' })),
});
