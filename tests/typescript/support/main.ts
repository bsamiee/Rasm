import { dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

// --- [COMPOSITION] ---------------------------------------------------------------------

const main = {
    framework: '@storybook/react-vite',
    addons: ['@storybook/addon-vitest', '@storybook/addon-a11y', '@storybook/addon-docs'].map((name) => dirname(fileURLToPath(import.meta.resolve(`${name}/package.json`)))),
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { main };
