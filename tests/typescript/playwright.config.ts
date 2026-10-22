import { globSync } from 'node:fs';
import { resolve } from 'node:path';
import { defineConfig } from '@playwright/test';
import { config } from '@rasm/test-support/playwright';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(config(import.meta.dirname), { projects: globSync('e2e-*', { cwd: import.meta.dirname }).map((name) => ({ name, testDir: resolve(import.meta.dirname, name) })) });
