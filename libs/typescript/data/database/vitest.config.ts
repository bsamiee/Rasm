import { fileURLToPath } from 'node:url';
import { located } from '../../../../vite.shared.ts';
import { vitestProject } from '../../../../vitest.config.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default vitestProject(import.meta.dirname, {
    portable: () =>
        import('@cloudflare/vitest-plugin').then(({ cloudflareTest }) => ({
            test: {
                projects: [{ test: { name: 'node', include: ['node.test.ts'] } }, { plugins: [cloudflareTest({ config: true })], test: { name: 'workers', include: ['d1.test.ts'] } }],
            },
        })),
    integration: { test: { projects: [{ test: { name: 'node', provide: { artifacts: located('.artifacts/typescript/vitest', import.meta.dirname), squawkConfig: fileURLToPath(new URL('../../../../.config/squawk.toml', import.meta.url)) } } }] } },
});
