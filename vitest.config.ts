import { join, resolve } from 'node:path';
import { cloudflareTest } from '@cloudflare/vitest-plugin';
import { storybookTest } from '@storybook/addon-vitest/vitest-plugin';
import react from '@vitejs/plugin-react';
import { playwright } from '@vitest/browser-playwright';
import { configDefaults, defineConfig, type Plugin } from 'vitest/config';
import type { BrowserConfigOptions } from 'vitest/node';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TRACE = { mode: 'retain-on-failure' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const browser = (): BrowserConfigOptions => ({ enabled: true, provider: playwright(), headless: true, instances: [{ browser: 'chromium' }], locators: { errorFormat: 'aria' }, trace: TRACE });

const stories = (name: string, folder: string) => ({ plugins: [storybookTest({ configDir: join(import.meta.dirname, folder, '.storybook') })], test: { name, api: { allowWrite: true }, browser: browser() } });

const reports = (): Plugin => ({
    name: 'reports',
    config({ test }) {
        const folder = test?.attachmentsDir === undefined ? undefined : resolve(import.meta.dirname, test.attachmentsDir, '..');
        return folder === undefined ? { test: { reporters: ['html'] } } : { test: { reporters: [['html', { outputDir: join(folder, 'html') }]], browser: { trace: { ...TRACE, tracesDir: join(folder, 'traces') }, screenshotDirectory: join(folder, 'screenshots') } } };
    },
});

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    plugins: [reports()],
    test: {
        chaiConfig: { includeStack: true, truncateThreshold: 0 },
        coverage: { reporter: ['text', 'html'], reportOnFailure: true, skipFull: true },
        reporters: [...configDefaults.reporters, 'junit'],
        silent: 'passed-only',
        projects: [
            { root: 'apps/web/tests/hooks', test: { name: '@rasm/test-hooks' } },
            stories('@rasm/portfolio', 'apps/web/portfolio'),
            { root: 'libs/typescript/ai/tests/mcp', test: { name: '@rasm/test-mcp' } },
            { root: 'libs/typescript/data/tests/database', test: { name: '@rasm/test-database node', include: ['node.test.ts'] } },
            { root: 'libs/typescript/data/tests/database', plugins: [cloudflareTest({ config: true })], test: { name: '@rasm/test-database workers', include: ['d1.test.ts'] } },
            { root: 'libs/typescript/data/tests/object-storage', plugins: [cloudflareTest({ config: true })], test: { name: '@rasm/test-object-storage' } },
            { root: 'libs/typescript/macos/tests/applications', test: { name: '@rasm/test-applications' } },
            { root: 'libs/typescript/messaging/tests/webhooks', test: { name: '@rasm/test-webhooks node', include: ['node.test.ts'] } },
            { root: 'libs/typescript/messaging/tests/webhooks', plugins: [cloudflareTest({ config: true })], test: { name: '@rasm/test-webhooks workers', include: ['workers.test.ts'] } },
            { root: 'libs/typescript/network/tests/ssh', test: { name: '@rasm/test-ssh' } },
            { root: 'libs/typescript/runtime/tests/workers', test: { name: '@rasm/test-workers node', include: ['deploy.test.ts'] } },
            { root: 'libs/typescript/runtime/tests/workers', plugins: [cloudflareTest({ config: true })], test: { name: '@rasm/test-workers workers', include: ['unit.test.ts', 'integration.test.ts'] } },
            { root: 'libs/typescript/web/tests/hosting', test: { name: '@rasm/test-hosting' } },
            { root: 'libs/typescript/web/tests/ui', plugins: [react({ compiler: { panicThreshold: 'all_errors' } })], test: { name: '@rasm/test-ui', browser: browser() } },
            stories('@rasm/ui', 'libs/typescript/web/ui'),
        ],
    },
});
