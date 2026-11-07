import { dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

// --- [OPERATIONS] ----------------------------------------------------------------------

const located = (name: string) => dirname(fileURLToPath(import.meta.resolve(`${name}/package.json`)));

// --- [COMPOSITION] ---------------------------------------------------------------------

const framework = located('@storybook/react-vite');
const addons = ['@storybook/addon-vitest', '@storybook/addon-a11y', '@storybook/addon-docs'].map(located);

// --- [EXPORTS] -------------------------------------------------------------------------

export { addons, framework };
