import { join } from 'node:path';
import { testPolicy } from '@rasm/testing/vitest';
import { storybookTest } from '@storybook/addon-vitest/vitest-plugin';
import { defineConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    plugins: [testPolicy(), storybookTest({ configDir: join(import.meta.dirname, '.storybook') })],
    test: { name: 'storybook', api: { allowWrite: true }, browser: { enabled: true, screenshotFailures: true } },
});
