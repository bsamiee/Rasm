import { cloudflareTest } from '@cloudflare/vitest-plugin';
import { project } from '../../../../vitest.config.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default project(import.meta.dirname, {
    test: {
        projects: [
            { test: { name: 'node', include: ['node.test.ts', 'postgres.test.ts'] } },
            {
                plugins: [cloudflareTest({ experimental: { newConfig: true } })],
                test: { name: 'workers', include: ['d1.test.ts'] },
            },
        ],
    },
});
