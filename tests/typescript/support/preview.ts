import addonA11y, { type A11yTypes } from '@storybook/addon-a11y';
import addonDocs from '@storybook/addon-docs';

// --- [COMPOSITION] ---------------------------------------------------------------------

const addons = [addonA11y(), addonDocs(), { parameters: { a11y: { test: 'error' } } satisfies A11yTypes['parameters'] }];

// --- [EXPORTS] -------------------------------------------------------------------------

export { addons };
