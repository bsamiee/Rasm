import { testPolicy } from '@rasm/testing/vitest';
import { configDefaults, defineConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    plugins: [testPolicy()],
    test: {
        include: ['inference/*.test.ts', 'delivery/*.test.ts', '*.test.ts'],
        exclude: [...configDefaults.exclude, '**/*.e2e.test.ts'],
        setupFiles: ['@rasm/testing/nx/setup'],
        provide: { driven: false },
        testTimeout: 60_000,
        unstubEnvs: true,
    },
});
