import { join } from 'node:path';
import { workspaceRoot } from '@nx/devkit';
import { defineConfig } from '@playwright/test';
import { config, served } from '@rasm/testing/playwright';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(config(import.meta.dirname, join(workspaceRoot, '.artifacts/playwright')), served('@rasm/portfolio:serve--vite', 'http://localhost:5173'));
