import { fileURLToPath } from 'node:url';
import { testPolicy } from '@rasm/testing/vitest';
import { defineConfig } from 'vitest/config';

// --- [TYPES] ---------------------------------------------------------------------------

declare module 'vitest' {
    interface ProvidedContext {
        readonly driven: boolean;
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    plugins: [testPolicy()],
    test: {
        dir: fileURLToPath(new URL('..', import.meta.url)),
        include: ['inference/*.test.ts', 'workspace/*.test.ts', 'delivery/*.e2e.test.ts'],
        setupFiles: ['@rasm/testing/nx/setup'],
        provide: { driven: true },
        testTimeout: 300_000,
        unstubEnvs: true,
    },
});
