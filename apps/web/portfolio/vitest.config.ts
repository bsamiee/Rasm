import { join } from 'node:path';
import { defaults } from '@rasm/test-support/vitest';
import { storybookTest } from '@storybook/addon-vitest/vitest-plugin';
import { defineConfig, defineProject, mergeConfig } from 'vitest/config';
import application from '#/vite.config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig((env) => mergeConfig(mergeConfig(application(env), defaults(import.meta.filename)), defineProject({ plugins: [storybookTest({ configDir: join(import.meta.dirname, '.storybook') })], test: { browser: { enabled: true, screenshotFailures: true }, api: { allowWrite: true } } })));
