import { existsSync, readFileSync, rmSync, symlinkSync } from 'node:fs';
import { join } from 'node:path';
import { env } from 'node:process';
import type { ProjectGraph } from '@nx/devkit';
import { createNodes } from '@rasm/nx-vite';
import { refused } from '@rasm/testing/errors';
import { it as nx, taskGraph, type Workspace } from '@rasm/testing/nx';
import { readNxJson } from 'nx/src/config/nx-json';
import { createTaskHasher } from 'nx/src/hasher/create-task-hasher';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const INSTALLED = { vite: '8.0.0', dep: '1.0.0', unread: '1.0.0' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const hashed = (workspace: Workspace, graph: ProjectGraph, versions: Readonly<Record<string, string>> = INSTALLED, variables: Readonly<Record<string, string>> = {}) => {
    const planned = taskGraph(graph, ['web'], ['build--vite']);
    const installed: ProjectGraph = { ...graph, externalNodes: Object.fromEntries(Object.entries(versions).map(([name, version]) => [`npm:${name}`, { type: 'npm', name: `npm:${name}`, data: { packageName: name, version, hash: `${name}@${version}` } }])) };
    const task = planned.tasks['web:build--vite'];
    return task === undefined
        ? Promise.resolve(undefined)
        : createTaskHasher(installed, readNxJson(workspace.root))
              .hashTask(task, planned, { ...env, ...variables })
              .then(({ value }) => value);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('vite/package.json'));
    workspace.write({
        'package.json': '{"name":"repo","workspaces":["apps/*","libs/*"]}',
        'package-lock.json': '{}',
        'nx.json': '{"namedInputs":{"default":["{projectRoot}/**/*"],"production":["default","!{projectRoot}/**/*.spec.ts"]}}',
        'tsconfig.base.json': '{"compilerOptions":{"target":"es2022"}}',
        'tsconfig.shared.json': '{"extends":"./tsconfig.base.json","compilerOptions":{"jsx":"react-jsx"}}',
        '.gitignore': '.env*\n',
        'docs/guide.md': '# Guide\n',
        'tools/vite/base.ts': "export const outDir = 'dist';\n",
        '..tools/vite/served.ts': "export const base = '/';\n",
        'libs/plugin/package.json': '{"name":"plugin","type":"module","exports":"./index.js"}',
        'libs/plugin/index.js': "export const named = 'plugin';\n",
        'node_modules/dep/package.json': '{"name":"dep","type":"module","exports":"./index.js"}',
        'node_modules/dep/index.js': "export const define = { DEP: '1' };\n",
        'apps/web/package.json': '{"name":"web","type":"module"}',
        'apps/web/tsconfig.json': '{"extends":"../../tsconfig.shared.json"}',
        'apps/web/index.html': '<!doctype html>\n',
        'apps/web/main.ts': 'export {};\n',
        'apps/web/main.spec.ts': 'export {};\n',
        'apps/web/vite.config.ts': "import { define } from 'dep';\nimport { named } from 'plugin';\nimport { outDir } from '../../tools/vite/base';\nimport { base } from '../../..tools/vite/served';\n\nexport default { base, define, build: { outDir }, plugins: [{ name: named }] };\n",
    });
    symlinkSync(join(workspace.root, 'libs/plugin'), join(workspace.root, 'node_modules/plugin'), 'dir');
});

describe('what build--vite hashes', () => {
    it.for([
        { case: 'a project source file', edit: { 'apps/web/main.ts': 'export const changed = 1;\n' }, changes: true },
        { case: 'a project spec file the production input leaves out', edit: { 'apps/web/main.spec.ts': 'export const changed = 1;\n' }, changes: false },
        { case: 'a workspace file the config imports from outside the project', edit: { 'tools/vite/base.ts': "export const outDir = 'dist';\nexport const unused = 1;\n" }, changes: true },
        { case: 'a workspace file the config imports from a two-dot folder outside the project', edit: { '..tools/vite/served.ts': "export const base = '/';\nexport const unused = 1;\n" }, changes: true },
        { case: 'the source of a workspace package the config imports', edit: { 'libs/plugin/index.js': "export const named = 'plugin';\nexport const unused = 1;\n" }, changes: true },
        { case: 'the compilerOptions of a tsconfig outside the project its extends chain reaches', edit: { 'tsconfig.shared.json': '{"extends":"./tsconfig.base.json","compilerOptions":{"jsx":"preserve"}}' }, changes: true },
        { case: 'the include list of that tsconfig, which vite build never reads', edit: { 'tsconfig.shared.json': '{"extends":"./tsconfig.base.json","compilerOptions":{"jsx":"react-jsx"},"include":["src"]}' }, changes: false },
        { case: 'a docs file no project owns', edit: { 'docs/guide.md': '# Guide\n\nMore.\n' }, changes: false },
        { case: 'a gitignored dotenv file of the envDir', edit: { 'apps/web/.env.production.local': 'VITE_API=https://two.test\n' }, changes: true },
    ])('hashes again after an edit to $case: $changes', async ({ edit, changes }, { workspace }) => {
        const before = await hashed(workspace, await workspace.graph({ createNodes }));

        const after = await hashed(workspace, await workspace.write(edit).graph({ createNodes }));

        expect({ planned: before !== undefined && after !== undefined, changed: before !== after }).toStrictEqual({ planned: true, changed: changes });
    });

    it.for([
        { case: 'a module it imports from inside the folder of another project', edit: { 'libs/plugin/index.js': "export const named = 'plugin';\nexport const unused = 1;\n" } },
        { case: 'the compilerOptions of a tsconfig inside the folder of another project the root tsconfig extends', edit: { 'libs/plugin/tsconfig.json': '{"compilerOptions":{"jsx":"preserve"}}' } },
        { case: 'a module it imports from a folder the root project holds', edit: { 'tools/vite/base.ts': "export const outDir = 'dist';\nexport const unused = 1;\n" } },
    ])('hashes a config of the root project again after an edit to $case', async ({ edit }, { workspace }) => {
        workspace.write({
            'package.json': '{"name":"web","type":"module","workspaces":["apps/*","libs/*"],"nx":{}}',
            '.gitignore': '.env*\ntmp\n',
            'apps/web/package.json': '{"name":"app","type":"module"}',
            'tsconfig.json': '{"extends":"./libs/plugin/tsconfig.json"}',
            'libs/plugin/tsconfig.json': '{"compilerOptions":{"jsx":"react-jsx"}}',
            'index.html': '<!doctype html>\n',
            'vite.config.ts': "import { named } from './libs/plugin/index.js';\nimport { outDir } from './tools/vite/base';\n\nexport default { build: { outDir }, plugins: [{ name: named }] };\n",
        });
        const before = await hashed(workspace, await workspace.graph({ createNodes }));

        const after = await hashed(workspace, await workspace.write(edit).graph({ createNodes }));

        expect({ planned: before !== undefined && after !== undefined, changed: before !== after }).toStrictEqual({ planned: true, changed: true });
    });

    it.for([
        { case: 'a module it imports from inside the folder of a project nested in its own', edit: { 'apps/web/sub/index.js': "export const named = 'sub';\nexport const unused = 1;\n" } },
        { case: 'the compilerOptions of a tsconfig inside the folder of a project nested in its own that its tsconfig extends', edit: { 'apps/web/sub/tsconfig.json': '{"extends":"../../../tsconfig.shared.json","compilerOptions":{"jsx":"preserve"}}' } },
        { case: 'a module it imports from its own folder outside the nested project', edit: { 'apps/web/base.ts': "export const outDir = 'dist';\nexport const unused = 1;\n" } },
    ])('hashes a config of a project holding another project again after an edit to $case', async ({ edit }, { workspace }) => {
        workspace.write({
            'apps/web/sub/project.json': '{"name":"sub"}',
            'apps/web/sub/index.js': "export const named = 'sub';\n",
            'apps/web/sub/tsconfig.json': '{"extends":"../../../tsconfig.shared.json","compilerOptions":{"jsx":"react-jsx"}}',
            'apps/web/tsconfig.json': '{"extends":"./sub/tsconfig.json"}',
            'apps/web/base.ts': "export const outDir = 'dist';\n",
            'apps/web/vite.config.ts': "import { named } from './sub/index.js';\nimport { outDir } from './base';\n\nexport default { build: { outDir }, plugins: [{ name: named }] };\n",
        });
        const before = await hashed(workspace, await workspace.graph({ createNodes }));

        const after = await hashed(workspace, await workspace.write(edit).graph({ createNodes }));

        expect({ planned: before !== undefined && after !== undefined, changed: before !== after }).toStrictEqual({ planned: true, changed: true });
    });

    it.for([
        { moved: 'vite', changes: true },
        { moved: 'dep', changes: true },
        { moved: 'unread', changes: false },
    ])('hashes again after a new release of the installed package $moved: $changes', async ({ moved, changes }, { workspace }) => {
        const graph = await workspace.graph({ createNodes });

        const [before, after] = await Promise.all([hashed(workspace, graph), hashed(workspace, graph, { ...INSTALLED, [moved]: '9.0.0' })]);

        expect({ planned: before !== undefined && after !== undefined, changed: before !== after }).toStrictEqual({ planned: true, changed: changes });
    });

    it.for([
        { case: 'a variable under the envPrefix', variable: 'VITE_API', changes: true },
        { case: 'a variable under a custom envPrefix', variable: 'PUBLIC_API', config: "export default { envPrefix: 'PUBLIC_' };\n", changes: true },
        { case: 'a variable outside the envPrefix', variable: 'API', changes: false },
    ])('hashes again after a change to $case: $changes', async ({ variable, config, changes }, { workspace }) => {
        const graph = await (config === undefined ? workspace : workspace.write({ 'apps/web/vite.config.ts': config })).graph({ createNodes });

        const [before, after] = await Promise.all([hashed(workspace, graph, INSTALLED, { [variable]: 'https://one.test' }), hashed(workspace, graph, INSTALLED, { [variable]: 'https://two.test' })]);

        expect({ planned: before !== undefined && after !== undefined, changed: before !== after }).toStrictEqual({ planned: true, changed: changes });
    });

    it('states its variable digest in words cmd.exe quoting accepts, which refuses a double quote or a line break', async ({ workspace }) => {
        const graph = await workspace.write({ 'apps/web/vite.config.ts': "export default { envPrefix: ['VITE_', 'PUBLIC_'] };\n" }).graph({ createNodes });

        const digests = (graph.nodes['web']?.data.targets?.['build--vite']?.inputs ?? []).flatMap((input) => (typeof input === 'object' && 'runtime' in input ? [input.runtime] : []));

        expect({ stated: digests.some((command) => command.includes('PUBLIC_')), refused: digests.filter((command) => /["\n]/u.test(command)) }).toStrictEqual({ stated: true, refused: [] });
    });

    it('builds after the vite build of each dependency and never after its tsc or storybook build', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/web/package.json': '{"name":"web","type":"module","nx":{"implicitDependencies":["shared"]}}',
                'libs/shared/package.json': '{"name":"shared","type":"module","nx":{"targets":{"build--vite":{"executor":"nx:noop"},"build--tsc":{"executor":"nx:noop"},"build--storybook":{"executor":"nx:noop"}}}}',
            })
            .graph({ createNodes });

        const { dependencies } = taskGraph(graph, ['web'], ['build--vite']);

        expect(dependencies['web:build--vite']?.map(({ id }) => id).toSorted()).toStrictEqual(['shared:build--vite']);
    });
});

describe('resolving the config across graph passes', () => {
    it('refuses a config that throws in build mode, naming the config', async ({ workspace }) => {
        const graphed = workspace.write({ 'apps/web/vite.config.ts': "throw new Error('config refused');\n" }).graph({ createNodes });

        await expect(graphed).rejects.toSatisfy(refused(/^apps\/web\/vite\.config\.ts does not load for vite build/u));
    });

    it("never lets one config's env writes reach another config or the process inferring them", async ({ workspace }) => {
        const before = { ...env };
        const graph = await workspace
            .write({
                'apps/early/package.json': '{"name":"early","type":"module"}',
                'apps/early/index.html': '<!doctype html>\n',
                'apps/early/vite.config.ts': "process.env.VITE_OUT = 'leaked';\n\nexport default {};\n",
                'apps/late/package.json': '{"name":"late","type":"module"}',
                'apps/late/index.html': '<!doctype html>\n',
                'apps/late/vite.config.ts': "export default { build: { outDir: process.env.VITE_OUT ?? 'dist' } };\n",
            })
            .graph({ createNodes });

        expect({ outputs: graph.nodes['late']?.data.targets?.['build--vite']?.outputs, written: env['VITE_OUT'], mode: env['NODE_ENV'] }).toStrictEqual({ outputs: ['{projectRoot}/dist'], written: undefined, mode: before['NODE_ENV'] });
    });

    it('loads the config once while edits reach no file it loads, and again with the new value after one does', async ({ workspace, onTestFinished }) => {
        const counter = `${workspace.root}.loads`;
        onTestFinished(() => rmSync(counter, { force: true }));
        const loads = () => (existsSync(counter) ? readFileSync(counter, 'utf-8').length : 0);
        await workspace.write({ 'apps/web/vite.config.ts': `import { appendFileSync } from 'node:fs';\nimport { outDir } from '../../tools/vite/base';\n\nappendFileSync(${JSON.stringify(counter)}, 'x');\n\nexport default { build: { outDir } };\n` }).graph({ createNodes });

        await workspace.write({ 'docs/guide.md': '# Guide\n\nMore.\n', 'apps/web/main.ts': 'export const changed = 1;\n' }).graph({ createNodes });
        const reused = loads();
        const graph = await workspace.write({ 'tools/vite/base.ts': "export const outDir = 'out';\n" }).graph({ createNodes });

        expect({ reused, reloaded: loads(), outputs: graph.nodes['web']?.data.targets?.['build--vite']?.outputs }).toStrictEqual({ reused: 1, reloaded: 2, outputs: ['{projectRoot}/out'] });
    });
});
