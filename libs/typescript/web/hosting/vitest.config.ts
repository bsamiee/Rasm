import { cloudflareTest } from '@cloudflare/vitest-plugin';
import { createVitestConfig } from '../../../../vitest.config.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default createVitestConfig(import.meta.dirname, {
    test: {
        projects: [{ plugins: [cloudflareTest({ experimental: { newConfig: true } })], test: { name: 'r2', include: ['r2.test.ts'] } }, { test: { name: 's3', include: ['s3.test.ts'] } }],
    },
});
