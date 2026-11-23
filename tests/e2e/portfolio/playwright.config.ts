import { defineConfig } from '@playwright/test';
import { config } from '@rasm/testing/playwright';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(config(import.meta.dirname));
