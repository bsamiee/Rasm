import { it as workspaced } from '@rasm/testing/nx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = workspaced.extend('installed', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('vitest/package.json'), import.meta.resolve('vite/package.json'), import.meta.resolve('@vitest/coverage-v8/package.json'), import.meta.resolve('@vitest/browser-playwright/package.json'));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { it };
