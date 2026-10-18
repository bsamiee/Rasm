import { cloudflareTest } from '@cloudflare/vitest-plugin';
import { createVitestConfig } from '../../../vitest.config.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default createVitestConfig(import.meta.dirname, { test: { projects: [{ plugins: [cloudflareTest({ experimental: { newConfig: true } })], test: { name: 'worker', setupFiles: ['@rasm/test-support/workers'] } }] } });
