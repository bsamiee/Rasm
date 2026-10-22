import { addons } from '@rasm/test-support/preview.ts';
import { definePreview } from '@storybook/react-vite';
import '../site/theme.css';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default definePreview({
    addons,
    tags: ['autodocs'],
    parameters: { layout: 'fullscreen' },
});
