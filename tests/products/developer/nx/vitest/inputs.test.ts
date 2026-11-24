import { readFileSync, rmSync, statSync, symlinkSync } from 'node:fs';
import { join } from 'node:path';
import type { ProjectGraph } from '@nx/devkit';
import { createNodes } from '@rasm/nx-vitest';
import { HashPlanInspector } from 'nx/src/hasher/hash-plan-inspector';
import { describe, expect, vi } from 'vitest';
import { it } from '#/installed';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = { 'package.json': '{"name":"repo","workspaces":["libs/*","packages/*"]}' };
const LIB = { ...ROOT, 'libs/lib/package.json': '{"name":"lib"}' };
const LOADS = 'tmp/loads';
const LISTED =
    "import { globSync, readFileSync } from 'node:fs';\nimport { dirname } from 'node:path';\n\nexport default { test: { projects: globSync('packages/*/package.json', { cwd: import.meta.dirname }).map((file) => ({ test: { name: JSON.parse(readFileSync(new URL(file, import.meta.url), 'utf-8')).name, root: dirname(file) } })) } };\n";

// --- [OPERATIONS] ----------------------------------------------------------------------

const counted = (up: string) => `import { appendFileSync } from 'node:fs';\n\nappendFileSync(new URL('${up}/${LOADS}', import.meta.url), '+');\n\n`;

const loads = (root: string) => readFileSync(join(root, LOADS), 'utf-8').length;

const targets = (graph: ProjectGraph) => Object.fromEntries(Object.values(graph.nodes).map(({ name, data }) => [name, data.targets]));

const hashed = async (graph: ProjectGraph, root: string, project: string) => {
    const inspector = new HashPlanInspector({ ...graph, externalNodes: Object.fromEntries(['vitest', 'vite', '@vitest/coverage-v8'].map((name) => [`npm:${name}`, { type: 'npm' as const, name: `npm:${name}`, data: { packageName: name, version: '5.0.0' } }])) }, root);
    await inspector.init();
    return inspector.inspectTaskInputs({ project, target: 'test--vitest' })[`${project}:test--vitest`]?.files ?? [];
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('test--vitest inference reused across graph runs', () => {
    it.for([
        {
            case: 'a module the config imports starts naming a setup file',
            before: { 'tools/setup.mts': '', 'shared/settings.ts': 'export const setupFiles: string[] = [];\n', 'libs/lib/vitest.config.ts': "import { setupFiles } from '../../shared/settings.ts';\n\nexport default { test: { setupFiles } };\n" },
            files: { 'shared/settings.ts': "export const setupFiles = ['../../tools/setup.mts'];\n" },
            options: {},
        },
        { case: 'a manifest joins the workspace a root config lists projects from', before: { 'vitest.config.ts': LISTED, 'packages/a/package.json': '{"name":"a"}' }, files: { 'packages/b/package.json': '{"name":"b"}' }, options: {} },
        { case: 'the plugin options change', before: { 'libs/lib/vitest.config.ts': 'export default {};\n' }, files: {}, options: { outputPath: '.artifacts/second' } },
    ])('infers from a warm cache what a cold read infers after $case', async ({ before, files, options }, { workspace }) => {
        workspace.write({ ...LIB, ...before });
        const initial = targets(await workspace.graph({ createNodes }, { outputPath: '.artifacts/first' }));

        const warm = targets(await workspace.write(files).graph({ createNodes }, { outputPath: '.artifacts/first', ...options }));
        rmSync(join(workspace.root, 'tmp/project-graph-cache'), { recursive: true, force: true });
        const cold = targets(await workspace.graph({ createNodes }, { outputPath: '.artifacts/first', ...options }));

        expect(cold).not.toStrictEqual(initial);
        expect(warm).toStrictEqual(cold);
    });

    it('reuses the inferred targets of a root configuration while a file it never reads changes', async ({ workspace }) => {
        await workspace.write({ ...ROOT, [LOADS]: '', 'vitest.config.ts': `${counted('.')}export default { test: { projects: ['packages/*'] } };\n`, 'packages/a/package.json': '{"name":"a"}' }).graph({ createNodes });
        expect(loads(workspace.root)).toBeGreaterThan(0);

        const graph = await workspace.write({ [LOADS]: '', 'packages/a/notes.md': '# a\n' }).graph({ createNodes });

        expect([loads(workspace.root), graph.nodes['a']?.data.targets?.['test--vitest']?.options]).toStrictEqual([0, { config: 'vitest.config.ts', projects: [] }]);
    });

    it('describes a configuration again after a source of a linked workspace package it imports changes', async ({ workspace }) => {
        workspace.write({
            ...LIB,
            [LOADS]: '',
            'libs/helper/package.json': '{"name":"helper","type":"module","exports":"./index.js"}',
            'libs/helper/index.js': "export const include = ['*.test.ts'];\n",
            'libs/lib/vitest.config.ts': `import { include } from 'helper';\n${counted('../..')}export default { test: { include } };\n`,
        });
        symlinkSync(join(workspace.root, 'libs/helper'), join(workspace.root, 'node_modules/helper'), 'dir');
        await workspace.graph({ createNodes });

        const graph = await workspace.write({ [LOADS]: '', 'libs/helper/index.js': "export const include = ['*.spec.ts'];\n" }).graph({ createNodes });

        expect([loads(workspace.root), graph.nodes['lib']?.data.targets?.['test--vitest']?.inputs]).toStrictEqual([1, expect.arrayContaining(['{workspaceRoot}/libs/helper/index.js'])]);
    });

    it('leaves the plugin cache file unwritten while every configuration is reused', async ({ workspace }) => {
        await workspace.write({ ...LIB, 'libs/lib/vitest.config.ts': 'export default {};\n' }).graph({ createNodes });
        const written = statSync(join(workspace.root, 'tmp/project-graph-cache/vitest.hash'), { bigint: true }).mtimeNs;

        await workspace.graph({ createNodes });

        expect(statSync(join(workspace.root, 'tmp/project-graph-cache/vitest.hash'), { bigint: true }).mtimeNs).toBe(written);
    });
});

describe('test--vitest hashes the files a run loads outside its project', () => {
    it.for([
        { case: 'a setup file', config: "export default { test: { setupFiles: ['../../tools/vitest/setup.mts'] } };\n", loaded: 'tools/vitest/setup.mts' },
        { case: 'a globalSetup string', config: "export default { test: { globalSetup: '../../tools/vitest/global.mts' } };\n", loaded: 'tools/vitest/global.mts' },
        { case: 'a setup file under an authored root above the project', config: "export default { root: '..', test: { setupFiles: ['../tools/vitest/setup.mts'] } };\n", loaded: 'tools/vitest/setup.mts' },
        { case: 'a file forceRerunTriggers names beyond the Vitest defaults', config: "export default { test: { forceRerunTriggers: ['**/shared/schema.json'] } };\n", loaded: 'shared/schema.json' },
        { case: 'the tsconfig the setup file compiles under', config: "export default { test: { setupFiles: ['../../tools/vitest/setup.mts'] } };\n", loaded: 'tools/tsconfig.json' },
    ])('hashes $case', async ({ config, loaded }, { workspace }) => {
        const graph = await workspace.write({ ...LIB, 'tools/vitest/setup.mts': '', 'tools/vitest/global.mts': '', 'tools/vitest/unused.mts': '', 'tools/tsconfig.json': '{"compilerOptions":{}}', 'shared/schema.json': '{}', 'libs/lib/vitest.config.ts': config }).graph({ createNodes });
        const files = await hashed(graph, workspace.root, 'lib');

        expect([files.includes(loaded), files.includes('tools/vitest/unused.mts')]).toStrictEqual([true, false]);
    });

    it('hashes the modules a config loads from a linked workspace package and no other file of that package', async ({ workspace }) => {
        workspace.write({
            ...LIB,
            'libs/helper/package.json': '{"name":"helper","type":"module","exports":{"./settings":"./settings.js"}}',
            'libs/helper/settings.js': "export { include } from './include.js';\n",
            'libs/helper/include.js': "export const include = ['*.test.ts'];\n",
            'libs/helper/unused.js': 'export const unused = true;\n',
            'libs/lib/vitest.config.ts': "import { include } from 'helper/settings';\n\nexport default { test: { include } };\n",
        });
        symlinkSync(join(workspace.root, 'libs/helper'), join(workspace.root, 'node_modules/helper'), 'dir');
        const graph = await workspace.graph({ createNodes });
        const files = await hashed(graph, workspace.root, 'lib');

        expect(['libs/helper/settings.js', 'libs/helper/include.js', 'libs/helper/unused.js'].map((at) => files.includes(at))).toStrictEqual([true, true, false]);
    });

    it('hashes a root configuration from the package whose project it lists', async ({ workspace }) => {
        const graph = await workspace.write({ ...ROOT, 'vitest.config.ts': "export default { test: { projects: ['packages/*'] } };\n", 'packages/a/package.json': '{"name":"a"}' }).graph({ createNodes });

        await expect(hashed(graph, workspace.root, 'a')).resolves.toContain('vitest.config.ts');
    });

    it('leaves out of a suite the module a plugin of another suite loads from inside that suite', async ({ workspace }) => {
        const graph = await workspace
            .write({
                ...ROOT,
                'vitest.config.ts': "export default { test: { projects: [{ test: { name: 'a', root: 'packages/a' } }, { test: { name: 'b', root: 'packages/b' }, plugins: [{ name: 'loads', config: async () => { await import('./packages/b/loaded.ts'); } }] }] } };\n",
                'packages/a/package.json': '{"name":"a"}',
                'packages/b/package.json': '{"name":"b"}',
                'packages/b/loaded.ts': 'export {};\n',
            })
            .graph({ createNodes });

        const [a, b] = await Promise.all([hashed(graph, workspace.root, 'a'), hashed(graph, workspace.root, 'b')]);

        expect([a.includes('vitest.config.ts'), a.includes('packages/b/loaded.ts'), b.includes('packages/b/loaded.ts')]).toStrictEqual([true, false, true]);
    });
});
