import type { ChildProcess } from 'node:child_process';
import { getBuiltinModule } from 'node:process';
import { fileServerExecutor } from '@nx/web/internal';
import { createNodes } from '@rasm/nx-storybook';
import { listen, it as nx, outputs, targetOptions, taskGraph } from '@rasm/testing/nx';
import { Effect, Schema } from 'effect';
import { FetchHttpClient, HttpClient, HttpClientResponse } from 'effect/http';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [MODELS] --------------------------------------------------------------------------

const Indexed = Schema.Struct({ entries: Schema.Record(Schema.String, Schema.Struct({ id: Schema.String })) }).annotate({ description: 'The index.json storybook build writes, one entry per story it indexed' });

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx
    .extend('modules', { auto: true }, ({ workspace }) => {
        workspace.install(import.meta.resolve('storybook/package.json'), import.meta.resolve('@storybook/react-vite/package.json'), import.meta.resolve('react/package.json'), import.meta.resolve('react-dom/package.json'), import.meta.resolve('vite/package.json'));
        workspace.write({
            'package.json': '{"name":"repo","workspaces":["apps/*"]}',
            'package-lock.json': '{}',
            'apps/site/package.json': '{"name":"site"}',
            'apps/site/label.stories.tsx': "export default { title: 'Label', component: () => 'label' };\n\nexport const Shown = {};\n",
            'apps/site/.storybook/main.ts': "export default { stories: ['../*.stories.tsx'], framework: { name: '@storybook/react-vite', options: {} }, core: { disableTelemetry: true } };\n",
        });
    })
    .extend('spawned', ({ onTestFinished }) => {
        const processes = getBuiltinModule('node:child_process');
        const [fork, served, built] = [processes.fork, Array.of<ChildProcess>(), Array.of<string>()];
        const forked = vi.spyOn(processes, 'fork').mockImplementation((...args: Parameters<typeof fork>) => {
            const child = fork(...args);
            served.push(child);
            return child;
        });
        const ran = vi.spyOn(processes, 'execFileSync').mockImplementation((file: string, args?: readonly string[]) => {
            built.push([file, ...(args ?? [])].join(' '));
            throw new Error('preview--storybook runs a build of its own');
        });
        onTestFinished(() => {
            forked.mockRestore();
            ran.mockRestore();
            served.forEach((child) => {
                child.kill();
            });
        });
        return built;
    });

describe('build--storybook and preview--storybook', { timeout: 120_000 }, () => {
    it.for([
        { case: 'in the project', options: {}, index: 'apps/site/storybook-static/index.html' },
        { case: 'under an outputPath outside the project', options: { outputPath: '.artifacts/storybook/{projectName}' }, index: '.artifacts/storybook/site/index.html' },
    ])('declares every file storybook build writes $case', async ({ options, index }, { workspace }) => {
        const graph = await workspace.graph({ createNodes }, options);
        const before = workspace.files('node_modules', 'tmp');

        const { success } = await workspace.run(graph, 'site', 'build--storybook');

        const written = workspace.files('node_modules', 'tmp').difference(before);
        expect({ success, index: written.has(index), undeclared: [...written.difference(workspace.cached(graph, 'site', 'build--storybook'))] }).toStrictEqual({ success: true, index: true, undeclared: [] });
    });

    it('declares as an output the folder each spelling of the storybook build output flag names', async ({ workspace }) => {
        const graph = await workspace.graph({ createNodes });

        expect([outputs(graph, 'site', 'build--storybook', undefined, { 'output-dir': 'kebab' }), outputs(graph, 'site', 'build--storybook', undefined, { outputDir: 'camel' }), outputs(graph, 'site', 'build--storybook', undefined, { o: 'short' })]).toStrictEqual([
            expect.arrayContaining(['apps/site/kebab']),
            expect.arrayContaining(['apps/site/camel']),
            expect.arrayContaining(['apps/site/short']),
        ]);
    });

    it('serves after build--storybook the index it wrote, running no build of its own', async ({ workspace, spawned: built }) => {
        const graph = await workspace.graph({ createNodes }, { outputPath: '.artifacts/storybook/{projectName}' });
        await workspace.run(graph, 'site', 'build--storybook');

        const [served, free] = await Promise.all([targetOptions(Schema.Struct({ staticFilePath: Schema.String, buildTarget: Schema.optionalKey(Schema.String) }), graph, 'site', 'preview--storybook'), listen()]);
        await free.close();
        const listening = await fileServerExecutor({ parallel: true, spa: false, withDeps: false, ...served, port: free.port }, workspace.context(graph, 'site', 'preview--storybook')).next();

        const { baseUrl } = await Schema.decodeUnknownPromise(Schema.Struct({ baseUrl: Schema.String }))(listening.value);
        const { entries } = await HttpClient.get(new URL('index.json', `${baseUrl}/`)).pipe(Effect.flatMap(HttpClientResponse.schemaBodyJson(Indexed)), Effect.provide(FetchHttpClient.layer), Effect.runPromise);
        expect({ after: taskGraph(graph, ['site'], ['preview--storybook']).dependencies['site:preview--storybook'], built, stories: Object.keys(entries) }).toStrictEqual({ after: [{ id: 'site:build--storybook' }], built: [], stories: ['label--shown'] });
    });
});
