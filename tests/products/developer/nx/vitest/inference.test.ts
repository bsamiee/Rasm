import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { env } from 'node:process';
import { createNodes } from '@rasm/nx-vitest';
import { refused } from '@rasm/testing/errors';
import { taskGraph, unaggregated } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';
import { it } from '#/installed.ts';

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

describe('a root configuration listing projects', () => {
    it('gives each listed package a target running its own Vitest project through the root configuration', async ({ workspace }) => {
        const graph = await workspace.write({ ...ROOT, 'vitest.config.ts': "export default { test: { projects: ['packages/*'] } };\n", 'packages/a/package.json': '{"name":"a"}', 'packages/b/package.json': '{"name":"b"}' }).graph({ createNodes });

        expect(Object.fromEntries(Object.values(graph.nodes).flatMap(({ name, data }) => (data.targets?.['test--vitest'] === undefined ? [] : [[name, data.targets['test--vitest'].options]])))).toStrictEqual({
            a: { config: 'vitest.config.ts', projects: ['a'] },
            b: { config: 'vitest.config.ts', projects: ['b'] },
        });
    });

    it.for([
        { case: 'project glob matches no folder', config: "export default { test: { projects: ['packages/*'] } };\n", refusal: /^vitest\.config\.ts does not load as a Vitest config/u },
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
        { enabled: true, technologies: ['vitest', 'playwright'], outputs: ['{projectRoot}/shots'] },
        { enabled: false, technologies: ['vitest'], outputs: [] },
    ])('names the provider and declares the screenshot folder while the browser is enabled: $enabled', async ({ enabled, technologies, outputs }, { workspace }) => {
        const graph = await workspace
            .write({
                ...ROOT,
                'libs/ui/package.json': '{"name":"ui"}',
                'libs/ui/vitest.config.ts': `import { playwright } from '@vitest/browser-playwright';\n\nexport default { test: { browser: { enabled: ${enabled}, provider: playwright(), instances: [{ browser: 'chromium' }], screenshotDirectory: 'shots' } } };\n`,
            })
            .graph({ createNodes });
        const target = graph.nodes['ui']?.data.targets?.['test--vitest'];

        expect(graph.nodes['ui']?.data.metadata?.technologies).toStrictEqual(technologies);
        expect(target?.outputs?.filter((output) => output.endsWith('/shots'))).toStrictEqual(outputs);
    });

    it.for([
        { case: 'enables a browser provider', config: "import { playwright } from '@vitest/browser-playwright';\n\nexport default { test: { browser: { enabled: true, provider: playwright(), instances: [{ browser: 'chromium' }] } } };\n", hosts: ['host:browser'] },
        { case: 'runs on node alone', config: 'export default {};\n', hosts: [] },
    ])('tags the host a package needs where its Vitest config $case', async ({ config, hosts }, { workspace }) => {
        const graph = await workspace.write({ ...ROOT, 'libs/ui/package.json': '{"name":"ui"}', 'libs/ui/vitest.config.ts': config }).graph({ createNodes });

        expect((graph.nodes['ui']?.data.tags ?? []).filter((tag) => tag.startsWith('host:'))).toStrictEqual(hosts);
    });
});

describe('verb aggregation', () => {
    it('runs every vitest tool target through its verb, which hashes each of its inputs', async ({ workspace }) => {
        const graph = await workspace
            .write({
                ...ROOT,
                'vitest.config.ts': "export default { test: { projects: ['packages/*'], forceRerunTriggers: ['**/schema.json'] } };\n",
                'packages/a/package.json': '{"name":"a"}',
                'packages/b/package.json': '{"name":"b"}',
                'libs/c/package.json': '{"name":"c"}',
                'libs/c/vitest.config.ts': "export default { test: { setupFiles: ['../../tools/setup.mts'] } };\n",
                'tools/setup.mts': '',
                'tools/tsconfig.json': '{"compilerOptions":{}}',
            })
            .graph({ createNodes }, { outputPath: '.artifacts/vitest/{projectName}', coveragePath: '.artifacts/coverage/{projectName}' });

        expect(Object.keys(graph.nodes['c']?.data.targets ?? {})).toContain('test--vitest');
        expect(unaggregated(graph)).toStrictEqual([]);
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
    it('loads the configuration under the TEST, VITEST, and NODE_ENV values vitest sets, keeping a stated NODE_ENV, and restores the process environment after', async ({ workspace, onTestFinished }) => {
        vi.stubEnv('TEST', undefined);
        vi.stubEnv('VITEST', 'outer');
        vi.stubEnv('NODE_ENV', 'production');
        onTestFinished(() => {
            vi.unstubAllEnvs();
        });
        workspace.write({ ...ROOT, 'libs/a/package.json': '{"name":"a"}', 'libs/a/vitest.config.ts': "import { writeFileSync } from 'node:fs';\n\nwriteFileSync(new URL('seen.json', import.meta.url), JSON.stringify([process.env.TEST, process.env.VITEST, process.env.NODE_ENV]));\n\nexport default {};\n" });

        await workspace.graph({ createNodes });

        expect([JSON.parse(readFileSync(join(workspace.root, 'libs/a/seen.json'), 'utf-8')), [env['TEST'], env['VITEST'], env['NODE_ENV']]]).toStrictEqual([
            ['true', 'true', 'production'],
            [undefined, 'outer', 'production'],
        ]);
    });
});
