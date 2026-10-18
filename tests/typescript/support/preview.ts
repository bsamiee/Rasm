import addonA11y from '@storybook/addon-a11y';
import addonDocs from '@storybook/addon-docs';

// --- [COMPOSITION] ---------------------------------------------------------------------

const addons = [addonA11y(), addonDocs(), { parameters: { a11y: { test: 'error' } } }];

// --- [EXPORTS] -------------------------------------------------------------------------

export { addons };
