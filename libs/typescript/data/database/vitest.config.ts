import { cloudflareTest } from '@cloudflare/vitest-plugin';
import { createVitestConfig } from '../../../../vitest.config.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default createVitestConfig(import.meta.dirname, {
    test: {
        projects: [
            { test: { name: 'node', include: ['node.test.ts'] } },
            { test: { name: 'postgres', include: ['postgres.test.ts'] } },
            {
                plugins: [cloudflareTest({ experimental: { newConfig: true } })],
                test: { name: 'workers', include: ['d1.test.ts'] },
            },
        ],
    },
});
