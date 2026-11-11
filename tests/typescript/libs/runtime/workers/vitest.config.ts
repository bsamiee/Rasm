import { cloudflareTest } from '@cloudflare/vitest-plugin';
import { testPolicy } from '@rasm/testing/vitest';
import { defineConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({ plugins: [testPolicy(), cloudflareTest({ config: true })], test: { include: ['*.test.ts'] } });
