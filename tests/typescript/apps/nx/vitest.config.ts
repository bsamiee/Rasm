import { testPolicy } from '@rasm/testing/vitest';
import { defineConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({ plugins: [testPolicy()], test: { include: ['inference/*.test.ts', 'workspace/*.test.ts', '*.test.ts'], setupFiles: ['@rasm/testing/nx/setup'], testTimeout: 60_000, unstubEnvs: true } });
