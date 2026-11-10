import addonA11y, { type A11yTypes } from '@storybook/addon-a11y';
import addonDocs from '@storybook/addon-docs';
import { definePreviewAddon } from 'storybook/internal/csf';

// --- [COMPOSITION] ---------------------------------------------------------------------

const previewAddons = [addonA11y(), addonDocs(), definePreviewAddon<A11yTypes>({ parameters: { a11y: { test: 'error' } } })];

// --- [EXPORTS] -------------------------------------------------------------------------

export { previewAddons };
