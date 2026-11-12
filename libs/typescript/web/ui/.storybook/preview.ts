import { preview } from '@rasm/testing/storybook';
import addonA11y from '@storybook/addon-a11y';
import addonDocs from '@storybook/addon-docs';
import { definePreview } from '@storybook/react-vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default definePreview({ ...preview, addons: [addonA11y(), addonDocs()] });
