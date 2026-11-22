import { createNodes } from '@rasm/nx-vite';
import { it as nx, unaggregated } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const rich = {
    'nx.json': '{"namedInputs":{"default":["{projectRoot}/**/*"],"production":["default","!{projectRoot}/**/*.spec.ts"]}}',
    'tsconfig.base.json': '{}',
    'tsconfig.shared.json': '{"extends":"./tsconfig.base.json"}',
    'tools/vite/base.ts': 'export const port = 4300;\n',
    'apps/web/project.json': '{"name":"web"}',
    'apps/web/tsconfig.json': '{"extends":"../../tsconfig.shared.json"}',
    'apps/web/index.html': '<!doctype html>\n',
    'apps/web/vite.config.ts': "import { port } from '../../tools/vite/base';\n\nexport default { server: { port } };\n",
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('vite/package.json'), import.meta.resolve('@nx/vite/package.json'));
    workspace.write({ 'package.json': '{"name":"repo","workspaces":["apps/*"]}', 'package-lock.json': '{}' });
});

describe('projects the Vite targets join', { timeout: 60_000 }, () => {
    it('infers build--vite from every config file name vite loads', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/js/project.json': '{"name":"js"}',
                'apps/js/vite.config.js': "module.exports = { build: { lib: { entry: 'index.js' } } };\n",
                'apps/mjs/project.json': '{"name":"mjs"}',
                'apps/mjs/vite.config.mjs': "export default { build: { lib: { entry: 'index.js' } } };\n",
                'apps/ts/project.json': '{"name":"ts"}',
                'apps/ts/vite.config.ts': "export default { build: { lib: { entry: 'index.js' } } };\n",
                'apps/cjs/project.json': '{"name":"cjs"}',
                'apps/cjs/vite.config.cjs': "module.exports = { build: { lib: { entry: 'index.js' } } };\n",
                'apps/mts/project.json': '{"name":"mts"}',
                'apps/mts/vite.config.mts': "export default { build: { lib: { entry: 'index.js' } } };\n",
                'apps/cts/project.json': '{"name":"cts"}',
                'apps/cts/vite.config.cts': "module.exports = { build: { lib: { entry: 'index.js' } } };\n",
            })
            .graph({ createNodes });

        expect(Object.fromEntries(Object.values(graph.nodes).map(({ name, data: { targets = {} } }) => [name, 'build--vite' in targets]))).toStrictEqual({ cjs: true, cts: true, js: true, mjs: true, mts: true, ts: true });
    });

    it('infers no project where neither a project.json nor a package.json the workspaces list sits beside the config', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/listed/package.json': '{"name":"listed"}',
                'apps/listed/vite.config.ts': "export default { build: { lib: { entry: 'index.ts' } } };\n",
                'tools/marked/project.json': '{"name":"marked"}',
                'tools/marked/vite.config.ts': "export default { build: { lib: { entry: 'index.ts' } } };\n",
                'tools/unlisted/package.json': '{"name":"unlisted"}',
                'tools/unlisted/vite.config.ts': "export default { build: { lib: { entry: 'index.ts' } } };\n",
            })
            .graph({ createNodes });

        expect(Object.fromEntries(Object.values(graph.nodes).map(({ data: { root, targets = {} } }) => [root, 'build--vite' in targets]))).toStrictEqual({ 'apps/listed': true, 'tools/marked': true });
    });

    it('types a library config a library and any other buildable config an application', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/lib/project.json': '{"name":"lib"}',
                'apps/lib/vite.config.ts': "export default { build: { lib: { entry: 'index.ts' } } };\n",
                'apps/app/project.json': '{"name":"app"}',
                'apps/app/index.html': '<!doctype html>\n',
                'apps/app/vite.config.ts': 'export default {};\n',
                'apps/bundle/project.json': '{"name":"bundle"}',
                'apps/bundle/vite.config.ts': "export default { build: { rolldownOptions: { input: 'main.ts' } } };\n",
            })
            .graph({ createNodes });

        expect([graph.nodes['lib']?.type, graph.nodes['app']?.type, graph.nodes['bundle']?.type]).toStrictEqual(['lib', 'app', 'app']);
    });

    it('runs every Vite target through its verb, hashing every input of each', async ({ workspace }) => {
        const graph = await workspace.write(rich).graph({ createNodes }, { outputPath: '.artifacts/vite/{projectName}' });

        expect(unaggregated(graph)).toStrictEqual([]);
    });

    it.for([
        { case: 'its project.json', home: 'apps/web', manifests: { 'apps/web/project.json': '{"name":"web","targets":{"build--vite":{"inputs":["...","{projectRoot}/stated.txt"]}}}' } },
        { case: 'a package.json the workspaces leave out beside its project.json', home: 'tools/web', manifests: { 'tools/web/project.json': '{"name":"web"}', 'tools/web/package.json': '{"name":"web","nx":{"targets":{"build--vite":{"inputs":["...","{projectRoot}/stated.txt"]}}}}' } },
    ])('hashes in build every input $case states for build--vite', async ({ home, manifests }, { workspace }) => {
        const graph = await workspace.write({ ...manifests, [`${home}/index.html`]: '<!doctype html>\n', [`${home}/vite.config.ts`]: 'export default {};\n' }).graph({ createNodes });

        expect(graph.nodes['web']?.data.targets?.['build--vite']?.inputs).toContain('{projectRoot}/stated.txt');
        expect(unaggregated(graph)).toStrictEqual([]);
    });

    it('names every vite target that declares outputs in the targets clean--vite removes', async ({ workspace }) => {
        const graph = await workspace.write(rich).graph({ createNodes });
        const targets = graph.nodes['web']?.data.targets ?? {};

        expect(targets['clean--vite']).toHaveProperty(
            ['options', 'targets'],
            Object.keys(targets).filter((name) => name.endsWith('--vite') && (targets[name]?.outputs ?? []).length > 0),
        );
    });
});
