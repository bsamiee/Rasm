import { defineConfig } from '@playwright/test';
import { config, served } from '@rasm/test-support/playwright';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(config, served('@rasm/portfolio:serve--vite', 'http://localhost:5173'));
