import { vitestProject } from '@rasm/test-support/vitest';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default vitestProject(import.meta.dirname, {
    portable: () => import('@cloudflare/vitest-plugin').then(({ cloudflareTest }) => ({ test: { projects: [{ plugins: [cloudflareTest({ config: true })], test: { name: 'r2', include: ['r2.test.ts'] } }] } })),
    integration: { test: { projects: [{ test: { name: 's3' } }] } },
});
