import { previewAddons } from '@rasm/test-support/storybook';
import { definePreview } from '@storybook/react-vite';
import '#/site/theme.css';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default definePreview({ addons: previewAddons, tags: ['autodocs'], parameters: { layout: 'fullscreen' } });
