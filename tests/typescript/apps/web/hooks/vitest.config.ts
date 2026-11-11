import { testPolicy } from '@rasm/testing/vitest';
import { defineConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({ plugins: [testPolicy()], test: { include: ['*.test.ts'] } });
