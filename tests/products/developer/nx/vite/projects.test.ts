import { createNodes } from '@rasm/nx-vite';
import { Clean, execute } from '@rasm/nx/executors/clean';
import { it as nx, taskGraph } from '@rasm/testing/nx';
import { Record, Schema } from 'effect';
import { assert, describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIB = "lib: { entry: 'index.ts', name: 'lib' }";

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('vite/package.json'), import.meta.resolve('@nx/vite/package.json'));
    workspace.write({ 'package.json': '{"name":"repo","workspaces":["apps/*"]}', 'package-lock.json': '{}' });
});

describe('projects the Vite targets join', () => {
    it('infers a project only where a project.json or a package.json the workspaces list sits beside the config', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/listed/package.json': '{"name":"listed"}',
                'apps/listed/vite.config.ts': `export default { build: { ${LIB} } };\n`,
                'tools/marked/project.json': '{"name":"marked"}',
                'tools/marked/vite.config.mts': `export default { build: { ${LIB} } };\n`,
                'tools/unlisted/package.json': '{"name":"unlisted"}',
                'tools/unlisted/vite.config.ts': `export default { build: { ${LIB} } };\n`,
            })
            .graph({ createNodes });

        expect(Record.mapEntries(graph.nodes, ({ data }) => [data.root, 'build--vite' in (data.targets ?? {})])).toStrictEqual({ 'apps/listed': true, 'tools/marked': true });
    });

    it.for([
        { case: 'an entry page', files: { 'apps/web/index.html': '<!doctype html>\n', 'apps/web/vite.config.ts': 'export default {};\n' }, targets: ['build--vite', 'preview--vite', 'serve--vite'], type: 'app' },
        { case: 'nothing to build', files: { 'apps/web/vite.config.ts': 'export default {};\n' }, targets: [], type: 'lib' },
        { case: 'a server block and nothing to build', files: { 'apps/web/vite.config.ts': 'export default { server: { port: 4300 } };\n' }, targets: [], type: 'lib' },
        { case: 'a rolldown input alone', files: { 'apps/web/vite.config.ts': "export default { build: { rolldownOptions: { input: 'index.ts' } } };\n" }, targets: ['build--vite'], type: 'app' },
        { case: 'a top-level input alone', files: { 'apps/web/vite.config.ts': "export default { input: 'index.ts' };\n" }, targets: ['build--vite'], type: 'app' },
        { case: 'an ssr entry alone', files: { 'apps/web/vite.config.ts': "export default { build: { ssr: 'index.ts' } };\n" }, targets: ['build--vite'], type: 'app' },
        { case: 'an entry page in the root folder the config states', files: { 'apps/web/src/index.html': '<!doctype html>\n', 'apps/web/vite.config.ts': "export default { root: 'src' };\n" }, targets: ['build--vite', 'preview--vite', 'serve--vite'], type: 'app' },
        { case: 'a buildApp builder alone', files: { 'apps/web/vite.config.ts': 'export default { builder: { buildApp: async () => {} } };\n' }, targets: ['build--vite'], type: 'app' },
        {
            case: 'a rolldown input and a ready signal',
            files: { 'apps/web/vite.config.ts': "import { nxReadySignal } from '@nx/vite/plugins/nx-ready-signal.plugin';\n\nexport default { plugins: [nxReadySignal()], build: { rolldownOptions: { input: 'index.ts' } } };\n" },
            targets: ['build--vite', 'preview--vite', 'serve--vite'],
            type: 'app',
        },
        { case: 'a library alone', files: { 'apps/web/vite.config.ts': `export default { build: { ${LIB} } };\n` }, targets: ['build--vite'], type: 'lib' },
        { case: 'a library and an entry page', files: { 'apps/web/index.html': '<!doctype html>\n', 'apps/web/vite.config.ts': `export default { build: { ${LIB} } };\n` }, targets: ['build--vite'], type: 'lib' },
        { case: 'a library and an empty server block', files: { 'apps/web/vite.config.ts': `export default { build: { ${LIB} }, server: {} };\n` }, targets: ['build--vite', 'preview--vite', 'serve--vite'], type: 'lib' },
        { case: 'a library and a preview block', files: { 'apps/web/vite.config.ts': `export default { build: { ${LIB} }, preview: { port: 4400 } };\n` }, targets: ['build--vite', 'preview--vite', 'serve--vite'], type: 'lib' },
        {
            case: 'a library whose config function states a server in serve mode alone',
            files: { 'apps/web/vite.config.ts': `export default ({ command }) => ({ build: { ${LIB} }, server: command === 'serve' ? { port: 4300 } : undefined });\n` },
            targets: ['build--vite', 'preview--vite', 'serve--vite'],
            type: 'lib',
        },
        {
            case: 'a library whose serve-only plugin states a server',
            files: { 'apps/web/vite.config.ts': `export default { build: { ${LIB} }, plugins: [{ name: 'serves', apply: 'serve', config: () => ({ server: { port: 4300 } }) }] };\n` },
            targets: ['build--vite', 'preview--vite', 'serve--vite'],
            type: 'lib',
        },
        {
            case: 'a library whose config refuses serve mode',
            files: { 'apps/web/vite.config.ts': `export default ({ command }) => {\n    if (command === 'serve') {\n        throw new Error('builds only');\n    }\n    return { build: { ${LIB} } };\n};\n` },
            targets: ['build--vite'],
            type: 'lib',
        },
    ])('infers $targets for a config with $case', async ({ files, targets, type }, { workspace }) => {
        const graph = await workspace.write({ 'apps/web/project.json': '{"name":"web"}', ...files }).graph({ createNodes });
        const { type: inferred, data } = graph.nodes['web'] ?? {};

        expect({
            targets: Object.keys(data?.targets ?? {})
                .filter((name) => /^(?:build|serve|preview)--vite$/u.test(name))
                .toSorted(),
            type: inferred,
        }).toStrictEqual({ targets, type });
    });

    it('builds the project and the build--vite of each project it depends on before preview--vite serves the build', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/web/project.json': '{"name":"web","implicitDependencies":["ui"]}',
                'apps/web/index.html': '<!doctype html>\n',
                'apps/web/vite.config.ts': 'export default {};\n',
                'libs/ui/project.json': '{"name":"ui"}',
                'libs/ui/vite.config.ts': `export default { build: { ${LIB} } };\n`,
            })
            .graph({ createNodes });

        const { dependencies, continuousDependencies } = taskGraph(graph, ['web'], ['preview--vite']);

        expect([dependencies['web:preview--vite'], continuousDependencies['web:preview--vite'], dependencies['web:build--vite']]).toStrictEqual([[{ id: 'web:build--vite' }], [], [{ id: 'ui:build--vite' }]]);
    });

    it('removes with clean--vite every file vite build wrote, and nothing else', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/web/package.json': '{"name":"web","type":"module"}',
                'apps/web/index.ts': 'export const value = 1;\n',
                'apps/web/vite.config.ts': `export default { build: { ${LIB} }, plugins: [{ name: 'routes', configEnvironment: () => ({ build: { outDir: '.output/client' } }) }] };\n`,
            })
            .graph({ createNodes });
        const before = workspace.files('node_modules', 'tmp');
        await workspace.run(graph, 'web', 'build--vite');
        const built = workspace.files('node_modules', 'tmp').difference(before);
        const stated: unknown = { commands: [], cwd: '.', folders: [], ...graph.nodes['web']?.data.targets?.['clean--vite']?.options };
        assert(Schema.is(Clean)(stated), 'clean--vite states clean options');

        await execute(stated, workspace.context(graph, 'web', 'clean--vite'));

        expect({ built: built.size > 0, left: [...workspace.files('node_modules', 'tmp').symmetricDifference(before)] }).toStrictEqual({ built: true, left: [] });
    });
});
