import type { ProjectGraph } from '@nx/devkit';
import { createNodes } from '@rasm/nx-playwright';
import { it as nx } from '@rasm/testing/nx';
import { Order, Predicate, Record } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const projects = (servers: Record.ReadonlyRecord<string, string>) =>
    Record.fromEntries(
        Record.collect(servers, (name, server): readonly (readonly [string, string])[] => [
            [`apps/${name}/project.json`, JSON.stringify({ name })],
            [`apps/${name}/playwright.config.js`, `module.exports = { webServer: ${server} };\n`],
        ]).flat(),
    );

const waits = (graph: ProjectGraph) =>
    Record.map(
        Record.filter(graph.nodes, ({ data: { targets = {} } }) => 'test--playwright' in targets),
        ({ data: { targets = {} } }) =>
            (targets['test--playwright']?.dependsOn ?? [])
                .flatMap((on) => (Predicate.isString(on) ? [] : [on]))
                .flatMap(({ projects: named = [], target, waitFor = 'started' }) => [named].flat().map((project) => `${project}:${target}:${waitFor}`))
                .toSorted(Order.String),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('@playwright/test/package.json'));
    workspace.write({ 'package.json': '{"name":"repo"}', 'package-lock.json': '{}' });
});

describe('test--playwright dependencies on webServer tasks', { timeout: 60_000 }, () => {
    it('waits until ready for the Nx task a reused webServer row runs, in each form a package manager spells it', async ({ workspace }) => {
        const graph = await workspace
            .write(
                projects({
                    npx: "{ command: 'npx nx run app1:serve', reuseExistingServer: true }",
                    bare: "{ command: 'nx run api:dev', reuseExistingServer: true }",
                    pnpm: "{ command: 'pnpm exec nx run app1:serve', reuseExistingServer: true }",
                    bunx: "{ command: 'bunx nx serve app1', reuseExistingServer: true }",
                    bunx2: "{ command: 'bun x nx serve app1', reuseExistingServer: true }",
                    yarn: "{ command: 'yarn nx dev api', reuseExistingServer: true }",
                    emptyEnv: "{ command: 'nx run app1:serve', reuseExistingServer: true, env: {} }",
                    emptyWait: "{ command: 'nx run app1:serve', reuseExistingServer: true, wait: {} }",
                }),
            )
            .graph({ createNodes });

        expect(waits(graph)).toStrictEqual({
            npx: ['app1:serve:ready'],
            bare: ['api:dev:ready'],
            pnpm: ['app1:serve:ready'],
            bunx: ['app1:serve:ready'],
            bunx2: ['app1:serve:ready'],
            yarn: ['api:dev:ready'],
            emptyEnv: ['app1:serve:ready'],
            emptyWait: ['app1:serve:ready'],
        });
    });

    it('leaves to Playwright each webServer row it launches itself', async ({ workspace }) => {
        const graph = await workspace
            .write(
                projects({
                    launched: "{ command: 'nx run app1:serve', reuseExistingServer: false }",
                    unstated: "{ command: 'nx run app1:serve' }",
                    stdout: "{ command: 'nx run app1:serve', reuseExistingServer: true, wait: { stdout: /ready/ } }",
                    stderr: "{ command: 'nx run app1:serve', reuseExistingServer: true, wait: { stderr: /ready/ } }",
                    env: "{ command: 'nx run app1:serve', reuseExistingServer: true, env: { PORT: '4300' } }",
                    commandless: "{ url: 'http://localhost:4300', reuseExistingServer: true }",
                    foreign: "{ command: 'npm run start', reuseExistingServer: true }",
                }),
            )
            .graph({ createNodes });

        expect(waits(graph)).toStrictEqual({ launched: [], unstated: [], stdout: [], stderr: [], env: [], commandless: [], foreign: [] });
    });

    it('waits for every reused row of a webServer list and for no row Playwright launches', async ({ workspace }) => {
        const graph = await workspace
            .write(
                projects({
                    listed: "[{ command: 'nx run app1:serve', reuseExistingServer: true }, { command: 'nx run api1:serve', reuseExistingServer: true }, { command: 'nx run api2:dev', reuseExistingServer: true }, { command: 'nx run app1:serve', reuseExistingServer: true }, { command: 'nx run api3:serve', reuseExistingServer: false }]",
                }),
            )
            .graph({ createNodes });

        expect(waits(graph)).toStrictEqual({ listed: ['api1:serve:ready', 'api2:dev:ready', 'app1:serve:ready'] });
    });

    it('keeps pack--playwright waiting on the reused server after test--playwright, as Nx stops a continuous task no remaining task names', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/e2e/project.json': '{"name":"e2e"}',
                'apps/e2e/smoke.spec.js': 'module.exports = {};\n',
                'apps/e2e/store.media.js': 'module.exports = {};\n',
                'apps/e2e/playwright.config.js': "module.exports = { webServer: { command: 'nx run app1:serve', reuseExistingServer: true }, projects: [{ name: 'suite', testIgnore: '**/*.media.js' }, { name: 'media', testMatch: '**/*.media.js', outputDir: '../../.artifacts/playwright/e2e/media' }] };\n",
            })
            .graph({ createNodes }, { outputPath: '.artifacts/playwright/{projectName}' });

        expect(graph.nodes['e2e']?.data.targets?.['pack--playwright']?.dependsOn?.filter(Predicate.isObject)).toStrictEqual([{ projects: ['app1'], target: 'serve', waitFor: 'ready' }]);
    });

    it('warns, naming the command and config, about a reused row whose nx command names no task or a configuration dependsOn cannot state', async ({ workspace, onTestFinished }) => {
        const warn = vi.spyOn(console, 'warn').mockReturnValue(undefined);
        onTestFinished(() => warn.mockRestore());
        const graph = await workspace
            .write(
                projects({
                    flagged: "{ command: 'npx nx run app1:serve --port=4200', reuseExistingServer: true }",
                    versioned: "{ command: 'npx nx@latest serve app1', reuseExistingServer: true }",
                    configured: "{ command: 'npx nx run app1:serve:production', reuseExistingServer: true }",
                    foreign: "{ command: 'npm run start', reuseExistingServer: true }",
                }),
            )
            .graph({ createNodes });

        expect(waits(graph)).toStrictEqual({ flagged: [], versioned: [], configured: [], foreign: [] });
        expect(warn.mock.calls.flat().toSorted(Order.String)).toStrictEqual([
            expect.stringMatching(/"npx nx run app1:serve --port=4200" in apps\/flagged\/playwright\.config\.js/u),
            expect.stringMatching(/"npx nx run app1:serve:production" in apps\/configured\/playwright\.config\.js/u),
            expect.stringMatching(/"npx nx@latest serve app1" in apps\/versioned\/playwright\.config\.js/u),
        ]);
    });
});
