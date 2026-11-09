import { join } from 'node:path';
import { defaults } from '@rasm/test-support/vitest';
import { react } from '@rasm/ui/vite';
import { storybookTest } from '@storybook/addon-vitest/vitest-plugin';
import { defineConfig, mergeConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default mergeConfig(
    defaults(import.meta.filename),
    defineConfig({
        test: {
            api: { allowWrite: true },
            browser: { enabled: true },
            projects: [
                { extends: true, plugins: [react()], test: { name: 'components' } },
                { extends: true, plugins: [storybookTest({ configDir: join(import.meta.dirname, '.storybook') })], test: { name: 'stories', browser: { screenshotFailures: true } } },
            ],
        },
    }),
);
