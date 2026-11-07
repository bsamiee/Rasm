import { storybookAddons } from '@rasm/test-support/preview';
import { definePreview } from '@storybook/react-vite';
import '#/site/theme.css';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default definePreview({
    addons: storybookAddons,
    tags: ['autodocs'],
    parameters: { layout: 'fullscreen' },
});
