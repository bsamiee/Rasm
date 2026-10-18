import { globSync } from 'node:fs';
import { resolve } from 'node:path';
import { defineConfig } from '@playwright/test';
import { config } from './support/playwright.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(config(import.meta.dirname), { projects: globSync('e2e-*', { cwd: import.meta.dirname }).map((name) => ({ name, testDir: resolve(import.meta.dirname, name) })) });
