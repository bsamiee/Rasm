import { main } from '@rasm/testing/storybook';
import { defineMain } from '@storybook/react-vite/node';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineMain({ ...main, framework: '@storybook/react-vite', stories: ['../*.stories.@(ts|tsx)'] });
