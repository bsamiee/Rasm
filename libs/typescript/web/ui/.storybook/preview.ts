import { addons } from '@rasm/test-support/preview.ts';
import { definePreview } from '@storybook/react-vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default definePreview({ addons, tags: ['autodocs'] });
