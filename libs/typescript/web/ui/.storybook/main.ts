import { storybookMain } from '@rasm/test-support/main';
import { defineMain } from '@storybook/react-vite/node';
import { mergeConfig } from 'vite';
import { react } from '#/vite.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineMain({ ...storybookMain, stories: ['./*.stories.@(ts|tsx)'], viteFinal: (config) => mergeConfig(config, { plugins: [react()] }) });
