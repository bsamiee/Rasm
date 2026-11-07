import { storybookPreview } from '@rasm/test-support/storybook';
import { definePreview } from '@storybook/react-vite';
import '#/site/theme.css';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default definePreview({ ...storybookPreview, parameters: { ...storybookPreview.parameters, layout: 'fullscreen' } });
