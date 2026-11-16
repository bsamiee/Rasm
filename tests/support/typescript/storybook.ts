import type { A11yTypes } from '@storybook/addon-a11y';

// --- [CONSTANTS] -----------------------------------------------------------------------

const main = { addons: ['@storybook/addon-vitest', '@storybook/addon-a11y', '@storybook/addon-docs'], core: { disableTelemetry: true } };

const preview: { readonly parameters: NonNullable<A11yTypes['parameters']>; readonly tags: string[] } = { parameters: { a11y: { test: 'error', options: { runOnly: ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa', 'best-practice'] } } }, tags: ['autodocs'] };

// --- [EXPORTS] -------------------------------------------------------------------------

export { main, preview };
