import { defineConfig } from 'vitest/config';
import { testPolicy } from '#/vitest';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({ plugins: [testPolicy()], test: { include: ['suite/*.test.ts'], globalSetup: ['suite/compose.setup.ts'] } });
