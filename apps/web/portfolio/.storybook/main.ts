import { main } from '@rasm/test-support/main';
import { defineMain } from '@storybook/react-vite/node';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineMain({ ...main, stories: ['./*.stories.ts'] });
