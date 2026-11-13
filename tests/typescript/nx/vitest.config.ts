import { configDefaults, defineConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    test: {
        name: '@rasm/test-nx',
        chaiConfig: { includeStack: true, truncateThreshold: 0 },
        reporters: [...configDefaults.reporters, 'junit'],
        globalSetup: ['setup.ts'],
        fileParallelism: false,
        testTimeout: 120_000,
        forceRerunTriggers: ['**/*'],
    },
});
