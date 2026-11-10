import { previewAddons } from '@rasm/test-support/storybook';
import { definePreview } from '@storybook/react-vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default definePreview({ addons: previewAddons, tags: ['autodocs'] });
