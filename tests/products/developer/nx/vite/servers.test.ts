import type { ProjectGraph } from '@nx/devkit';
import { createNodes } from '@rasm/nx-vite';
import { it as nx, taskGraph } from '@rasm/testing/nx';
import { Order, Record } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const tools = (graph: ProjectGraph) =>
    Record.map(graph.nodes, ({ data: { targets = {} } }) =>
        Object.keys(targets)
            .filter((name) => name.endsWith('--vite') && name !== 'clean--vite')
            .toSorted(Order.String),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('vite/package.json'), import.meta.resolve('@nx/vite/package.json'));
    workspace.write({ 'package.json': '{"name":"repo"}', 'package-lock.json': '{}' });
});

describe('build, serve, and preview targets of a Vite config', { timeout: 60_000 }, () => {
    const lib = "lib: { entry: 'index.ts', name: 'lib' }";

    it('builds a config with something to build, and serves an application with an entry page or a ready signal', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/app/project.json': '{"name":"app"}',
                'apps/app/index.html': '<!doctype html>\n',
                'apps/app/vite.config.ts': 'export default {};\n',
                'apps/bare/project.json': '{"name":"bare"}',
                'apps/bare/vite.config.ts': 'export default {};\n',
                'apps/bundle/project.json': '{"name":"bundle"}',
                'apps/bundle/vite.config.ts': "export default { build: { rolldownOptions: { input: 'main.ts' } } };\n",
                'apps/builder/project.json': '{"name":"builder"}',
                'apps/builder/vite.config.ts': 'export default { builder: { buildApp: async () => {} } };\n',
                'apps/signal/project.json': '{"name":"signal"}',
                'apps/signal/vite.config.ts': "import { nxReadySignal } from '@nx/vite/plugins/nx-ready-signal.plugin';\n\nexport default { plugins: [nxReadySignal()], build: { rolldownOptions: { input: 'main.ts' } } };\n",
            })
            .graph({ createNodes });

        expect(tools(graph)).toStrictEqual({
            app: ['build--vite', 'preview--vite', 'serve--vite'],
            bare: [],
            bundle: ['build--vite'],
            builder: ['build--vite'],
            signal: ['build--vite', 'preview--vite', 'serve--vite'],
        });
    });

    it('serves a library only where its config states a server or preview block in either mode', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/plain/project.json': '{"name":"plain"}',
                'apps/plain/vite.config.ts': `export default { build: { ${lib} } };\n`,
                'apps/server/project.json': '{"name":"server"}',
                'apps/server/vite.config.ts': `export default { build: { ${lib} }, server: { port: 4300 } };\n`,
                'apps/empty/project.json': '{"name":"empty"}',
                'apps/empty/vite.config.ts': `export default { build: { ${lib} }, server: {} };\n`,
                'apps/preview/project.json': '{"name":"preview"}',
                'apps/preview/vite.config.ts': `export default { build: { ${lib} }, preview: { port: 4400 } };\n`,
                'apps/mode/project.json': '{"name":"mode"}',
                'apps/mode/vite.config.ts': `export default ({ command }) => ({ build: { ${lib} }, server: command === 'serve' ? { port: 4300 } : undefined });\n`,
                'apps/hooked/project.json': '{"name":"hooked"}',
                'apps/hooked/vite.config.ts': `export default { build: { ${lib} }, plugins: [{ name: 'serves', apply: 'serve', config: () => ({ server: { port: 4300 } }) }] };\n`,
                'apps/refusing/project.json': '{"name":"refusing"}',
                'apps/refusing/vite.config.ts': `export default ({ command }) => {\n    if (command === 'serve') {\n        throw new Error('builds only');\n    }\n    return { build: { ${lib} } };\n};\n`,
            })
            .graph({ createNodes });

        expect(tools(graph)).toStrictEqual({
            plain: ['build--vite'],
            server: ['build--vite', 'preview--vite', 'serve--vite'],
            empty: ['build--vite', 'preview--vite', 'serve--vite'],
            preview: ['build--vite', 'preview--vite', 'serve--vite'],
            mode: ['build--vite', 'preview--vite', 'serve--vite'],
            hooked: ['build--vite', 'preview--vite', 'serve--vite'],
            refusing: ['build--vite'],
        });
    });

    it('builds a project and the build--vite of each project it depends on before preview--vite serves the build', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/web/project.json': '{"name":"web","implicitDependencies":["ui"]}',
                'apps/web/index.html': '<!doctype html>\n',
                'apps/web/vite.config.ts': 'export default {};\n',
                'libs/ui/project.json': '{"name":"ui"}',
                'libs/ui/vite.config.ts': `export default { build: { ${lib} } };\n`,
            })
            .graph({ createNodes });

        const { dependencies, continuousDependencies } = taskGraph(graph, ['web'], ['preview--vite']);

        expect([dependencies['web:preview--vite'], continuousDependencies['web:preview--vite'], dependencies['web:build--vite']]).toStrictEqual([[{ id: 'web:build--vite' }], [], [{ id: 'ui:build--vite' }]]);
    });
});
