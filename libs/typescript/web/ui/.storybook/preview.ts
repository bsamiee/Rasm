import { storybookAddons } from '@rasm/test-support/preview';
import { definePreview } from '@storybook/react-vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default definePreview({ addons: storybookAddons, tags: ['autodocs'] });
