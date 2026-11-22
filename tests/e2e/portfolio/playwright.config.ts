import { defineConfig } from '@playwright/test';
import { config, served } from '@rasm/testing/playwright';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(config(import.meta.dirname), served('@rasm/portfolio:preview--vite', 'http://localhost:4173'));
