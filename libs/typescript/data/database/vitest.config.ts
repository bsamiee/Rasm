import { join } from 'node:path';
import { vitestProject } from '@rasm/test-support/vitest';
import { searchForWorkspaceRoot } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default vitestProject(import.meta.dirname, {
    portable: () =>
        import('@cloudflare/vitest-plugin').then(({ cloudflareTest }) => ({
            test: {
                projects: [{ test: { name: 'node', include: ['node.test.ts'] } }, { plugins: [cloudflareTest({ config: true })], test: { name: 'workers', include: ['d1.test.ts'] } }],
            },
        })),
    integration: { test: { projects: [{ test: { name: 'node', provide: { squawkConfig: join(searchForWorkspaceRoot(import.meta.dirname), '.config', 'squawk.toml') } } }] } },
});
