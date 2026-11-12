import { cloudflareTest } from '@cloudflare/vitest-plugin';
import { testPolicy } from '@rasm/testing/vitest';
import { defineConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    plugins: [testPolicy()],
    test: {
        projects: [
            { extends: true, plugins: [cloudflareTest({ config: true })], test: { name: 'workers', include: ['r2.test.ts'] } },
            { extends: true, test: { name: 's3', include: ['s3.test.ts'] } },
        ],
    },
});
