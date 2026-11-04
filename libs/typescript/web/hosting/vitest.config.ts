import { cloudflareTest } from '@cloudflare/vitest-plugin';
import { vitestProject } from '../../../../vitest.config.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default vitestProject(import.meta.dirname, {
    test: {
        projects: [{ plugins: [cloudflareTest({ config: true })], test: { name: 'r2', include: ['r2.test.ts'] } }, { test: { name: 's3', include: ['s3.test.ts'] } }],
    },
});
