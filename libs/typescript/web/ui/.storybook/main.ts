import { main } from '@rasm/test-support/main';
import { defineMain } from '@storybook/react-vite/node';
import { mergeConfig } from 'vite';
import { react } from '../../../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineMain({ ...main, stories: ['./*.stories.tsx'], viteFinal: (config) => mergeConfig(config, { plugins: [react()] }) });
