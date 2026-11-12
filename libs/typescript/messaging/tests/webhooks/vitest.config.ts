import { cloudflareTest } from '@cloudflare/vitest-plugin';
import { testPolicy } from '@rasm/testing/vitest';
import { defineConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    plugins: [testPolicy()],
    test: {
        projects: [
            { extends: true, test: { name: 'node', include: ['node.test.ts'] } },
            { extends: true, plugins: [cloudflareTest({ config: true })], test: { name: 'workers', include: ['workers.test.ts'] } },
        ],
    },
});
