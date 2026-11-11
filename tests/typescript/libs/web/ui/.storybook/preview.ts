import { previewAddons } from '@rasm/testing/storybook';
import { definePreview } from '@storybook/react-vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default definePreview({ addons: previewAddons, tags: ['autodocs'] });
