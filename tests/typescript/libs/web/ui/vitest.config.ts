import { join } from 'node:path';
import { defaults } from '@rasm/test-support/vitest';
import { storybookTest } from '@storybook/addon-vitest/vitest-plugin';
import react from '@vitejs/plugin-react';
import { defineConfig, mergeConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default mergeConfig(
    defaults(import.meta.filename),
    defineConfig({
        test: {
            api: { allowWrite: true },
            browser: { enabled: true },
            projects: [
                { extends: true, plugins: [react({ compiler: { panicThreshold: 'all_errors' } })], test: { name: 'components' } },
                { extends: true, plugins: [storybookTest({ configDir: join(import.meta.dirname, '.storybook') })], test: { name: 'stories', browser: { screenshotFailures: true } } },
            ],
        },
    }),
);
