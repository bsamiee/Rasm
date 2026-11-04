import { cloudflareTest } from '@cloudflare/vitest-plugin';
import { vitestProject } from '../../../../vitest.config.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default vitestProject(import.meta.dirname, {
    test: {
        projects: [
            { test: { name: 'node', include: ['node.test.ts', 'postgres.test.ts'] } },
            {
                plugins: [cloudflareTest({ config: true })],
                test: { name: 'workers', include: ['d1.test.ts'] },
            },
        ],
    },
});
