import { cloudflareTest } from '@cloudflare/vitest-plugin';
import { defaults } from '@rasm/test-support/vitest';
import { defineProject, mergeConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default mergeConfig(defaults(import.meta.filename), defineProject({ plugins: [cloudflareTest({ config: true })], test: { include: ['r2.test.ts'] } }));
