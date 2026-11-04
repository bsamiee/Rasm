import { storybookMain } from '@rasm/test-support/main';
import { defineMain } from '@storybook/react-vite/node';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineMain({ ...storybookMain, stories: ['./*.stories.ts'] });
