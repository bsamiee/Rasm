import { addons, core, framework } from '@rasm/test-support/storybook/node';
import { defineMain } from '@storybook/react-vite/node';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineMain({ framework, addons, core, stories: ['./*.stories.ts'] });
