import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { env } from 'node:process';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { createNodes } from '@rasm/nx-vitest';
import { refused } from '@rasm/testing/errors';
import { taskGraph } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';
import { it } from '#/installed';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = { 'package.json': '{"name":"repo","workspaces":["libs/*","packages/*"]}' };

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('projects a Vitest configuration infers', () => {
    it.for([
        { case: 'a workspace package', files: { 'libs/a/package.json': '{"name":"a"}', 'libs/a/vitest.config.ts': 'export default {};\n' }, config: 'libs/a/vitest.config.ts' },
        { case: 'a project.json', files: { 'tools/a/project.json': '{"name":"a"}', 'tools/a/vitest.config.ts': 'export default {};\n' }, config: 'tools/a/vitest.config.ts' },
        { case: 'a Vite config stating test', files: { 'libs/a/package.json': '{"name":"a"}', 'libs/a/vite.config.ts': 'export default { test: {} };\n' }, config: 'libs/a/vite.config.ts' },
        {
            case: 'a Vitest config beside a Vite config it takes priority over',
            files: { 'libs/a/package.json': '{"name":"a"}', 'libs/a/vite.config.ts': "import 'absent-vite-plugin';\n\nexport default { test: {} };\n", 'libs/a/vitest.config.ts': 'export default {};\n' },
            config: 'libs/a/vitest.config.ts',
        },
    ])('runs vitest over the config of $case', async ({ files, config }, { workspace }) => {
        const graph = await workspace.write({ ...ROOT, ...files }).graph({ createNodes });

        expect(graph.nodes['a']?.data.targets?.['test--vitest']?.options).toStrictEqual({ config, projects: [] });
    });

    it.for([
        { case: 'a package.json the workspaces leave out', files: { 'outside/a/package.json': '{"name":"a"}', 'outside/a/vitest.config.ts': 'export default {};\n' } },
        { case: 'a Vite config stating no test', files: { 'libs/a/package.json': '{"name":"a"}', 'libs/a/vite.config.ts': 'export default {};\n' } },
    ])('infers no test target beside $case', async ({ files }, { workspace }) => {
        const graph = await workspace.write({ ...ROOT, ...files }).graph({ createNodes });

        expect(Object.values(graph.nodes).filter(({ data }) => 'test--vitest' in { ...data.targets })).toStrictEqual([]);
    });
});

describe('files Nx hands the plugin', () => {
    const WORKSPACE = {
        ...ROOT,
        'vitest.config.ts': "export default { test: { projects: [{ test: { name: 'b', root: 'libs/b' } }] } };\n",
        'libs/a/package.json': '{"name":"a"}',
        'libs/a/vitest.config.ts': 'export default {};\n',
        'libs/b/package.json': '{"name":"b"}',
        'libs/c/package.json': '{"name":"c"}',
        'libs/c/vite.config.ts': 'export default { test: {} };\n',
    };

    it('matches no Vitest config, as one marks no project at its own path, and keeps every Vite config', async ({ workspace }) => {
        workspace.write(WORKSPACE);

        const matched = await globWithWorkspaceContext(workspace.root, [createNodes[0]]);

        expect(matched.toSorted()).toStrictEqual(['libs/c/vite.config.ts']);
    });

    it('infers the projects, targets, and inputs a glob matching every Vitest config infers', async ({ workspace }) => {
        workspace.write(WORKSPACE);
        const inferred = async (glob: string) => {
            const { nodes } = await workspace.graph({ createNodes: [glob, createNodes[1]] });
            return Object.values(nodes).map(({ name, data: { root, tags, targets } }) => ({ name, root, tags, targets }));
        };

        const wide = await inferred('**/{vite,vitest}.config.{js,mjs,ts,cjs,mts,cts}');
        const narrow = await inferred(createNodes[0]);

        expect(
            wide
                .filter(({ targets = {} }) => 'test--vitest' in targets)
                .map(({ name }) => name)
                .toSorted(),
        ).toStrictEqual(['a', 'b', 'c']);
        expect(narrow).toStrictEqual(wide);
    });
});

describe('a root configuration listing projects', () => {
    it.for([
        { case: 'project glob matches no folder', config: "export default { test: { projects: ['packages/*'] } };\n", refusal: /^vitest\.config\.ts unloaded tool: vitest/u },
        { case: 'inline project roots in a folder no project holds', config: "export default { test: { projects: [{ test: { name: 'suite', root: 'tools/suite' } }] } };\n", refusal: /^vitest\.config\.ts declares a project under tools\/suite, which no project root holds$/u },
    ])('refuses a root configuration whose $case', async ({ config, refusal }, { workspace }) => {
        await expect(workspace.write({ ...ROOT, 'vitest.config.ts': config, 'tools/suite/a.test.ts': '' }).graph({ createNodes })).rejects.toSatisfy(refused(refusal));
    });
});

describe('browser suites', () => {
    it('declares on each suite only its own projects screenshot folder, and no suite the html report the whole run writes', async ({ workspace }) => {
        const graph = await workspace
            .write({
                ...ROOT,
                'libs/a/package.json': '{"name":"a"}',
                'libs/b/package.json': '{"name":"b"}',
                'vitest.config.ts': `import { playwright } from '@vitest/browser-playwright';\n\nexport default { test: { reporters: [['html', { outputFile: 'report/index.html' }]], projects: [${['a', 'b'].map((name) => `{ test: { name: '${name}', root: 'libs/${name}', browser: { enabled: true, provider: playwright(), instances: [{ browser: 'chromium' }], screenshotDirectory: 'shots/${name}' } } }`).join(', ')}] } };\n`,
            })
            .graph({ createNodes });
        const declared = (name: string) => graph.nodes[name]?.data.targets?.['test--vitest']?.outputs?.filter((output) => output.includes('shots') || output.includes('report'));

        expect([declared('a'), declared('b')]).toStrictEqual([['{projectRoot}/shots/a'], ['{projectRoot}/shots/b']]);
    });

    it.for([
        { case: 'enables a browser provider', config: "import { playwright } from '@vitest/browser-playwright';\n\nexport default { test: { browser: { enabled: true, provider: playwright(), instances: [{ browser: 'chromium' }] } } };\n", hosts: ['host:browser'] },
        { case: 'names a browser provider it leaves disabled', config: "import { playwright } from '@vitest/browser-playwright';\n\nexport default { test: { browser: { enabled: false, provider: playwright(), instances: [{ browser: 'chromium' }] } } };\n", hosts: [] },
        { case: 'runs on node alone', config: 'export default {};\n', hosts: [] },
    ])('tags the host a package needs where its Vitest config $case', async ({ config, hosts }, { workspace }) => {
        const graph = await workspace.write({ ...ROOT, 'libs/ui/package.json': '{"name":"ui"}', 'libs/ui/vitest.config.ts': config }).graph({ createNodes });

        expect(graph.nodes['ui']?.data.targets?.['test--vitest']).toBeDefined();
        expect((graph.nodes['ui']?.data.tags ?? []).filter((tag) => tag.startsWith('host:'))).toStrictEqual(hosts);
    });
});

describe('task order', () => {
    it('runs the codegen targets of the project before test--vitest', async ({ workspace }) => {
        const graph = await workspace.write({ ...ROOT, 'libs/a/package.json': '{"name":"a","nx":{"targets":{"codegen--schema":{"command":"echo"}}}}', 'libs/a/vitest.config.ts': 'export default {};\n' }).graph({ createNodes });

        const tasks = taskGraph(graph, ['a'], ['test--vitest']);

        expect(tasks.dependencies['a:test--vitest']).toStrictEqual([{ id: 'a:codegen--schema' }]);
    });
});

describe('test mode', () => {
    it.for([
        { case: 'keeping a stated NODE_ENV', stated: 'production', seen: ['true', 'true', 'production'] },
        { case: 'setting NODE_ENV test where none is stated', stated: undefined, seen: ['true', 'true', 'test'] },
    ])('loads the configuration under the TEST, VITEST, and NODE_ENV values vitest sets, $case, and restores the process environment after', async ({ stated, seen }, { workspace, onTestFinished }) => {
        vi.stubEnv('TEST', undefined);
        vi.stubEnv('VITEST', 'outer');
        vi.stubEnv('NODE_ENV', stated);
        onTestFinished(() => {
            vi.unstubAllEnvs();
        });
        workspace.write({ ...ROOT, 'libs/a/package.json': '{"name":"a"}', 'libs/a/vitest.config.ts': "import { writeFileSync } from 'node:fs';\n\nwriteFileSync(new URL('seen.json', import.meta.url), JSON.stringify([process.env.TEST, process.env.VITEST, process.env.NODE_ENV]));\n\nexport default {};\n" });

        await workspace.graph({ createNodes });

        expect([JSON.parse(readFileSync(join(workspace.root, 'libs/a/seen.json'), 'utf-8')), [env['TEST'], env['VITEST'], env['NODE_ENV']]]).toStrictEqual([seen, [undefined, 'outer', stated]]);
    });
});
