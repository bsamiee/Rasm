import { join } from 'node:path';
import { defaults } from '@rasm/test-support/vitest';
import { storybookTest } from '@storybook/addon-vitest/vitest-plugin';
import { defineConfig, defineProject, mergeConfig } from 'vitest/config';
import application from '#/vite.config.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig((env) => application(env).then((config) => mergeConfig(mergeConfig(config, defaults(import.meta.filename)), defineProject({ plugins: [storybookTest({ configDir: join(import.meta.dirname, '.storybook') })], test: { browser: { enabled: true }, api: { allowWrite: true } } }))));
