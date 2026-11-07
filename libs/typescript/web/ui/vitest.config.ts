import { join } from 'node:path';
import { defaults } from '@rasm/test-support/vitest';
import { storybookTest } from '@storybook/addon-vitest/vitest-plugin';
import { defineProject, mergeConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default mergeConfig(defaults(import.meta.filename), defineProject({ plugins: [storybookTest({ configDir: join(import.meta.dirname, '.storybook') })], test: { browser: { enabled: true, screenshotFailures: true }, api: { allowWrite: true } } }));
