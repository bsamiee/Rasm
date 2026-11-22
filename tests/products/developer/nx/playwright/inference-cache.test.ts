import { createNodes } from '@rasm/nx-playwright';
import { it as nx } from '@rasm/testing/nx';
import { Predicate } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace, onTestFinished }) => {
    workspace.install(import.meta.resolve('@playwright/test/package.json'));
    workspace.write({ 'package.json': '{"name":"repo","workspaces":["apps/*"]}', 'package-lock.json': '{}', 'apps/e2e/package.json': '{"name":"e2e"}' });
    Reflect.set(globalThis, 'loads', 0);
    vi.stubEnv('PW_TARGET', 'serve');
    onTestFinished(() => {
        vi.unstubAllEnvs();
    });
});

describe('inferred Playwright targets across graph passes', { timeout: 60_000 }, () => {
    const counted = 'globalThis.loads = (globalThis.loads ?? 0) + 1;\n';

    it.for([
        { system: 'CommonJS', file: 'apps/e2e/playwright.config.js', exported: 'module.exports =' },
        { system: 'ES module', file: 'apps/e2e/playwright.config.mjs', exported: 'export default' },
    ])('reads an edited $system config again in the same process', async ({ file, exported }, { workspace }) => {
        await workspace.write({ [file]: `${exported} { webServer: { command: 'nx run app1:serve', reuseExistingServer: true } };\n` }).graph({ createNodes });

        const graph = await workspace.write({ [file]: `${exported} { webServer: { command: 'nx run app1:dev', reuseExistingServer: true } };\n` }).graph({ createNodes });

        expect(graph.nodes['e2e']?.data.targets?.['test--playwright']?.dependsOn?.filter(Predicate.isObject)).toStrictEqual([{ projects: ['app1'], target: 'dev', waitFor: 'ready' }]);
    });

    it('loads the config again once the environment it reads changes', async ({ workspace }) => {
        await workspace.write({ 'apps/e2e/playwright.config.js': "module.exports = { webServer: { command: 'nx run app1:' + process.env.PW_TARGET, reuseExistingServer: true } };\n" }).graph({ createNodes });
        vi.stubEnv('PW_TARGET', 'dev');

        const graph = await workspace.graph({ createNodes });

        expect(graph.nodes['e2e']?.data.targets?.['test--playwright']?.dependsOn?.filter(Predicate.isObject)).toStrictEqual([{ projects: ['app1'], target: 'dev', waitFor: 'ready' }]);
    });

    it('reuses the inferred targets while nothing the config reads changed', async ({ workspace }) => {
        await workspace.write({ 'apps/e2e/playwright.config.js': `${counted}module.exports = {};\n` }).graph({ createNodes });

        await workspace.graph({ createNodes });

        expect(Reflect.get(globalThis, 'loads')).toBe(1);
    });
});
