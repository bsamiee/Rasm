import { dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import type { StorybookConfig } from 'storybook/internal/types';

// --- [COMPOSITION] ---------------------------------------------------------------------

const addons = ['@storybook/addon-vitest', '@storybook/addon-a11y', '@storybook/addon-docs'].map((name) => dirname(fileURLToPath(import.meta.resolve(`${name}/package.json`)))) satisfies StorybookConfig['addons'];
const core = { disableTelemetry: true } as const satisfies StorybookConfig['core'];

// --- [EXPORTS] -------------------------------------------------------------------------

export { addons, core };
