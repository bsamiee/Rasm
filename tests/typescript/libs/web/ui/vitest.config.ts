import { join } from 'node:path';
import { testPolicy } from '@rasm/testing/vitest';
import { storybookTest } from '@storybook/addon-vitest/vitest-plugin';
import react from '@vitejs/plugin-react';
import { defineConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    plugins: [testPolicy()],
    test: {
        api: { allowWrite: true },
        browser: { enabled: true },
        projects: [
            { extends: true, plugins: [react({ compiler: { panicThreshold: 'all_errors' } })], test: { name: 'components', include: ['*.test.{ts,tsx}'] } },
            { extends: true, plugins: [storybookTest({ configDir: join(import.meta.dirname, '.storybook') })], test: { name: 'stories', browser: { screenshotFailures: true } } },
        ],
    },
});
