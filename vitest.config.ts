import { defaults } from '@rasm/test-support/vitest';
import { defineConfig, mergeConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default mergeConfig(defaults(import.meta.filename), defineConfig({ test: { projects: ['*/**/vitest.config.ts'] } }));
