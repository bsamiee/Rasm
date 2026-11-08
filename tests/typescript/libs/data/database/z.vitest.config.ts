import { cloudflareTest } from '@cloudflare/vitest-plugin';
import { defaults } from '@rasm/test-support/vitest';
import { defineProject, mergeConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default mergeConfig(defaults(import.meta.filename), defineProject({ test: { projects: [{ test: { name: 'node', include: ['node.test.ts'] } }, { plugins: [cloudflareTest({ config: true })], test: { name: 'workers', include: ['d1.test.ts'] } }] } }));
