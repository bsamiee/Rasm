import { env } from 'node:process';
import { fileURLToPath } from 'node:url';
import { createNodes } from '@rasm/nx-playwright';
import { refused } from '@rasm/testing/errors';
import { it as nx } from '@rasm/testing/nx';
import { Predicate } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const internal = JSON.stringify(fileURLToPath(import.meta.resolve('@nx/devkit/internal')));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace, onTestFinished }) => {
    workspace.install(import.meta.resolve('@playwright/test/package.json'));
    workspace.write({ 'package.json': '{"name":"repo","workspaces":["apps/*"]}', 'package-lock.json': '{}', 'apps/e2e/package.json': '{"name":"e2e"}' });
    ['PW_ADDED', 'PW_CLIENT', 'PW_TARGET'].forEach((name) => {
        vi.stubEnv(name, undefined);
    });
    vi.stubEnv('PW_KEPT', 'ambient');
    onTestFinished(() => {
        vi.unstubAllEnvs();
    });
});

describe('loading a Playwright config', { timeout: 60_000 }, () => {
    it('refuses a config that throws, naming the config', async ({ workspace }) => {
        const graphed = workspace.write({ 'apps/e2e/playwright.config.js': "throw new Error('config refused');\n" }).graph({ createNodes });

        await expect(graphed).rejects.toSatisfy(refused(/^apps\/e2e\/playwright\.config\.js does not load as a Playwright config/u));
    });

    it.for([
        { case: 'loads', tail: 'module.exports = {};' },
        { case: 'throws', tail: "throw new Error('config refused');" },
    ])('leaves process.env as it found it after a config that writes to it $case', async ({ tail }, { workspace }) => {
        workspace.write({ 'apps/e2e/playwright.config.js': `process.env.PW_ADDED = 'config';\nprocess.env.PW_KEPT = 'config';\n${tail}\n` });

        await workspace.graph({ createNodes }).catch(() => undefined);

        expect([env['PW_ADDED'], env['PW_KEPT']]).toStrictEqual([undefined, 'ambient']);
    });

    it.for([
        { case: 'after the config wrote it', config: `process.env.PW_CLIENT = 'config';\nrequire(${internal}).applyDaemonEnvFromClient({ ...process.env });\n`, kept: 'config' },
        { case: 'before the config overwrote it', config: `require(${internal}).applyDaemonEnvFromClient({ ...process.env, PW_CLIENT: 'client' });\nprocess.env.PW_CLIENT = 'config';\n`, kept: 'client' },
    ])('keeps the client env the daemon applied during the load $case', async ({ config, kept }, { workspace }) => {
        workspace.write({ 'apps/e2e/playwright.config.js': `${config}module.exports = {};\n` });

        await workspace.graph({ createNodes });

        expect(env['PW_CLIENT']).toBe(kept);
    });

    it("loads configs one at a time, so one config's env writes never reach another's webServer rows", async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/early/package.json': '{"name":"early"}',
                'apps/early/playwright.config.js': "process.env.PW_TARGET = 'leaked';\nmodule.exports = {};\n",
                'apps/late/package.json': '{"name":"late"}',
                'apps/late/playwright.config.js': "module.exports = { webServer: { command: 'nx run app1:' + (process.env.PW_TARGET ?? 'serve'), reuseExistingServer: true } };\n",
            })
            .graph({ createNodes });

        expect(graph.nodes['late']?.data.targets?.['test--playwright']?.dependsOn?.filter(Predicate.isObject)).toStrictEqual([{ projects: ['app1'], target: 'serve', waitFor: 'ready' }]);
    });
});
