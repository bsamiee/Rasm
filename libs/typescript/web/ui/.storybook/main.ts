import { defineMain } from '@storybook/react-vite/node';
import { mergeConfig } from 'vite';
import { react } from '../../../../../vite.config.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineMain({ framework: '@storybook/react-vite', stories: ['./*.stories.tsx'], addons: ['@rasm/test-support/preset.ts'], viteFinal: (config) => mergeConfig(config, { plugins: [react()] }) });
