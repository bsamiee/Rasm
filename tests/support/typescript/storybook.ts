import { join } from 'node:path';
import viteReact from '@vitejs/plugin-react';
import { resolvePathInStorybookCache } from 'storybook/internal/common';
import { type InlineConfig, mergeConfig } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const react = viteReact({ exclude: [/\/node_modules\//u, join(resolvePathInStorybookCache(''), '..', '**')], compiler: { panicThreshold: 'all_errors' } });

const main = { addons: ['@storybook/addon-vitest', '@storybook/addon-a11y', '@storybook/addon-docs'], core: { disableTelemetry: true }, viteFinal: (config: InlineConfig) => mergeConfig(config, { plugins: [react], build: { chunkSizeWarningLimit: 900 } }) };

// --- [EXPORTS] -------------------------------------------------------------------------

export { main, react };
