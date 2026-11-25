import { existsSync, readFileSync, rmSync, statSync, utimesSync } from 'node:fs';
import { join } from 'node:path';
import { env } from 'node:process';
import { createNodes } from '@rasm/nx-playwright';
import { refused } from '@rasm/testing/errors';
import { it as nx } from '@rasm/testing/nx';
import { Predicate } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace, onTestFinished }) => {
    workspace.install(import.meta.resolve('@playwright/test/package.json'));
    workspace.write({ 'package.json': '{"name":"repo","workspaces":["apps/*"]}', 'package-lock.json': '{}', 'docs/guide.md': '# Guide\n', 'apps/e2e/package.json': '{"name":"e2e"}' });
    vi.stubEnv('PW_ADDED', undefined);
    vi.stubEnv('PW_TARGET', 'serve');
    vi.stubEnv('PW_KEPT', 'ambient');
    onTestFinished(() => {
        vi.unstubAllEnvs();
    });
});

describe('loading a Playwright config', () => {
    it('refuses a config that throws, naming the config', async ({ workspace }) => {
        const graphed = workspace.write({ 'apps/e2e/playwright.config.js': "throw new Error('config refused');\n" }).graph({ createNodes });

        await expect(graphed).rejects.toSatisfy(refused(/^apps\/e2e\/playwright\.config\.js does not load as a Playwright config/u));
    });

    it.for([
        { case: 'loads', tail: 'module.exports = {};', settled: 'fulfilled' },
        { case: 'throws', tail: "throw new Error('config refused');", settled: 'rejected' },
    ])('leaves process.env as it found it after a config that writes to it $case', async ({ tail, settled }, { workspace }) => {
        workspace.write({ 'apps/e2e/playwright.config.js': `process.env.PW_ADDED = 'config';\nprocess.env.PW_KEPT = 'config';\n${tail}\n` });

        const [graphed] = await Promise.allSettled([workspace.graph({ createNodes })]);

        expect({ settled: graphed.status, added: env['PW_ADDED'], kept: env['PW_KEPT'] }).toStrictEqual({ settled, added: undefined, kept: 'ambient' });
    });

    it("never lets one config's env writes reach another's webServer rows", async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/early/package.json': '{"name":"early"}',
                'apps/early/playwright.config.js': "process.env.PW_TARGET = 'leaked';\nmodule.exports = {};\n",
                'apps/late/package.json': '{"name":"late"}',
                'apps/late/playwright.config.js': "module.exports = { webServer: { command: 'nx run app1:' + process.env.PW_TARGET, reuseExistingServer: true } };\n",
            })
            .graph({ createNodes });

        expect(graph.nodes['late']?.data.targets?.['test--playwright']?.dependsOn?.filter(Predicate.isObject)).toStrictEqual([{ projects: ['app1'], target: 'serve', waitFor: 'ready' }]);
    });
});

describe('inferred Playwright targets across graph passes', () => {
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

    it('reuses the config while only environment it never reads changes, as NX_HEAD does in every CI job', async ({ workspace, onTestFinished }) => {
        const counter = `${workspace.root}.loads`;
        onTestFinished(() => rmSync(counter, { force: true }));
        vi.stubEnv('NX_HEAD', 'one');
        await workspace.write({ 'apps/e2e/playwright.config.js': `require('node:fs').appendFileSync(${JSON.stringify(counter)}, 'x');\nmodule.exports = { webServer: { command: 'nx run app1:' + process.env.PW_TARGET, reuseExistingServer: true } };\n` }).graph({ createNodes });
        vi.stubEnv('NX_HEAD', 'two');

        await workspace.graph({ createNodes });

        expect(readFileSync(counter, 'utf-8')).toBe('x');
    });

    it('loads the config and writes its cache once while edits reach neither a file it loads nor the test files its projects list, and again after either does', async ({ workspace, onTestFinished }) => {
        const [counter, cache] = [`${workspace.root}.loads`, join(workspace.root, 'tmp/project-graph-cache/playwright.hash')];
        onTestFinished(() => rmSync(counter, { force: true }));
        const loads = () => (existsSync(counter) ? readFileSync(counter, 'utf-8').length : 0);
        await workspace.write({ 'tools/pw/base.js': 'module.exports = {};\n', 'apps/e2e/smoke.spec.js': 'module.exports = {};\n', 'apps/e2e/playwright.config.js': `require('node:fs').appendFileSync(${JSON.stringify(counter)}, 'x');\nrequire('../../tools/pw/base.js');\nmodule.exports = {};\n` }).graph({ createNodes });
        utimesSync(cache, 0, 0);

        await workspace.write({ 'docs/guide.md': '# Guide\n\nMore.\n', 'apps/e2e/smoke.spec.js': 'module.exports = { changed: true };\n' }).graph({ createNodes });
        const reused = { loads: loads(), rewritten: statSync(cache).mtimeMs !== 0 };
        await workspace.write({ 'apps/e2e/added.spec.js': 'module.exports = {};\n' }).graph({ createNodes });
        const listed = loads();
        await workspace.write({ 'tools/pw/base.js': 'module.exports = { changed: true };\n' }).graph({ createNodes });

        expect({ reused, listed, imported: loads() }).toStrictEqual({ reused: { loads: 1, rewritten: false }, listed: 2, imported: 3 });
    });
});
