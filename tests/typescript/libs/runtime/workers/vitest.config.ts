import { cloudflareTest } from '@cloudflare/vitest-plugin';
import { testPolicy } from '@rasm/testing/vitest';
import { defineConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    plugins: [testPolicy()],
    test: {
        projects: [
            { extends: true, test: { name: 'node', include: ['deploy.test.ts'] } },
            { extends: true, plugins: [cloudflareTest({ config: true })], test: { name: 'workers', include: ['unit.test.ts', 'integration.test.ts'] } },
        ],
    },
});
