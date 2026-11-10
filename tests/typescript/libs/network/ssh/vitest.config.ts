import { testPolicy } from '@rasm/test-support/vitest';
import { defineConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({ plugins: [testPolicy()], test: { include: ['sshd.test.ts'] } });
