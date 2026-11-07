import { addons, framework } from '@rasm/test-support/storybook/node';
import { defineMain } from '@storybook/react-vite/node';
import { mergeConfig } from 'vite';
import { react } from '#/vite.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineMain({ framework, addons, stories: ['./*.stories.@(ts|tsx)'], viteFinal: (config) => mergeConfig(config, { plugins: [react()] }) });
