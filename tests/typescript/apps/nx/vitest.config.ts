import { testPolicy } from '@rasm/test-support/vitest';
import { defineConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({ plugins: [testPolicy()], test: { include: ['inference/*.test.ts', '*.test.ts'], setupFiles: ['@rasm/test-support/nx/setup'], testTimeout: 60_000, unstubEnvs: true } });
