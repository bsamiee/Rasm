import { previewAddons } from '@rasm/testing/storybook';
import { definePreview } from '@storybook/react-vite';
import '#/site/theme.css';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default definePreview({ addons: previewAddons, tags: ['autodocs'], parameters: { layout: 'fullscreen' } });
