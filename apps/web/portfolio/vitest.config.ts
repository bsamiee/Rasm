import { join } from 'node:path';
import { testPolicy } from '@rasm/testing/vitest';
import { storybookTest } from '@storybook/addon-vitest/vitest-plugin';
import { defineConfig, mergeConfig } from 'vitest/config';
import application from '#/vite.config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig((env) =>
    mergeConfig(application(env), {
        plugins: [testPolicy()],
        test: { projects: [{ extends: true, plugins: [storybookTest({ configDir: join(import.meta.dirname, '.storybook') })], test: { name: 'storybook', browser: { enabled: true, screenshotFailures: true }, api: { allowWrite: true } } }] },
    }),
);
