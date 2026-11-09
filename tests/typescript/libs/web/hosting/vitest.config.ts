import { cloudflareTest } from '@cloudflare/vitest-plugin';
import { defaults } from '@rasm/test-support/vitest';
import { defineConfig, mergeConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default mergeConfig(
    defaults(import.meta.filename),
    defineConfig({
        test: {
            projects: [
                { extends: true, plugins: [cloudflareTest({ config: true })], test: { name: 'workers', include: ['r2.test.ts'] } },
                { extends: true, test: { name: 's3', include: ['s3.test.ts'] } },
            ],
        },
    }),
);
