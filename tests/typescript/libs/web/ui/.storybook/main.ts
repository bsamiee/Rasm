import { addons, core, framework } from '@rasm/test-support/storybook/node';
import { react } from '@rasm/ui/vite';
import { defineMain } from '@storybook/react-vite/node';
import { mergeConfig } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineMain({ framework, addons, core, stories: ['./*.stories.@(ts|tsx)'], viteFinal: (config) => mergeConfig(config, { plugins: [react()] }) });
