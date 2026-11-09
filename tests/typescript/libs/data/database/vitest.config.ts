import { cloudflareTest } from '@cloudflare/vitest-plugin';
import { defaults } from '@rasm/test-support/vitest';
import { defineConfig, mergeConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default mergeConfig(
    defaults(import.meta.filename),
    defineConfig({
        test: {
            projects: [
                { extends: true, test: { name: 'node', include: ['node.test.ts'] } },
                { extends: true, plugins: [cloudflareTest({ config: true })], test: { name: 'workers', include: ['d1.test.ts'] } },
                { extends: true, test: { name: 'postgres', include: ['postgres.test.ts'] } },
            ],
        },
    }),
);
