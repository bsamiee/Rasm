import { existsSync, readFileSync, rmSync, symlinkSync } from 'node:fs';
import { join } from 'node:path';
import type { ProjectGraph } from '@nx/devkit';
import { createNodes } from '@rasm/nx-storybook';
import { refused } from '@rasm/testing/errors';
import { it as nx, taskGraph, type Workspace } from '@rasm/testing/nx';
import { Predicate, Record, UndefinedOr } from 'effect';
import { readNxJson } from 'nx/src/config/nx-json';
import { createTaskHasher } from 'nx/src/hasher/create-task-hasher';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const INSTALLED = { storybook: '11.0.0', '@storybook/react-vite': '11.0.0', unread: '1.0.0' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const hashed = (workspace: Workspace, graph: ProjectGraph, target: string, versions: Record.ReadonlyRecord<string, string> = INSTALLED) => {
    const planned = taskGraph(graph, ['site'], [target]);
    const installed: ProjectGraph = { ...graph, externalNodes: Record.mapEntries(versions, (version, name) => [`npm:${name}`, { type: 'npm', name: `npm:${name}`, data: { packageName: name, version, hash: `${name}@${version}` } }]) };
    return UndefinedOr.map(planned.tasks[`site:${target}`], (task) =>
        createTaskHasher(installed, readNxJson(workspace.root))
            .hashTask(task, planned, {})
            .then(({ value }) => value),
    );
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('@storybook/react-vite/package.json'));
    workspace.write({
        'package.json': '{"name":"repo","workspaces":["apps/*","libs/*"]}',
        'package-lock.json': '{}',
        'nx.json': '{"namedInputs":{"default":["{projectRoot}/**/*"],"production":["default","!{projectRoot}/**/*.stories.tsx","!{projectRoot}/.storybook/**/*"]}}',
        'tsconfig.base.json': '{"compilerOptions":{"target":"es2022"}}',
        'tsconfig.shared.json': '{"extends":"./tsconfig.base.json","compilerOptions":{"jsx":"react-jsx"}}',
        'docs/guide.md': '# Guide\n',
        'tools/storybook/shared.ts': "export const stories = ['../src', '../../../libs/ui/**/*.stories.tsx', '../../../..stories/**/*.stories.tsx'];\n",
        'libs/addon/package.json': '{"name":"addon","type":"module","exports":"./preset.js"}',
        'libs/addon/preset.js': 'export {};\n',
        'libs/ui/button.stories.tsx': 'export default {};\n',
        'libs/ui/button.tsx': 'export const Button = 1;\n',
        '..stories/card.stories.tsx': 'export default {};\n',
        'apps/site/package.json': '{"name":"site"}',
        'apps/site/tsconfig.json': '{"extends":"../../tsconfig.shared.json"}',
        'apps/site/src/label.stories.tsx': 'export default {};\n',
        'apps/site/.storybook/main.ts': "import { stories } from '../../../tools/storybook/shared.ts';\n\nexport default { stories, framework: '@storybook/react-vite', addons: ['addon'] };\n",
    });
    symlinkSync(join(workspace.root, 'libs/addon'), join(workspace.root, 'node_modules/addon'), 'dir');
});

describe('what the Storybook targets hash', () => {
    it.for([
        { case: 'a story in the stories folder the main config names', edit: { 'apps/site/src/label.stories.tsx': 'export default { title: "Label" };\n' }, changes: true },
        { case: 'a story under a stories glob outside the project', edit: { 'libs/ui/button.stories.tsx': 'export default { title: "Button" };\n' }, changes: true },
        { case: 'a story under a stories glob in a two-dot folder outside the project', edit: { '..stories/card.stories.tsx': 'export default { title: "Card" };\n' }, changes: true },
        { case: 'a workspace file the main config imports from outside the project', edit: { 'tools/storybook/shared.ts': "export const stories = ['../src', '../../../libs/ui/**/*.stories.tsx', '../../../..stories/**/*.stories.tsx'];\nexport const unused = 1;\n" }, changes: true },
        { case: 'the source of a workspace addon the main config names', edit: { 'libs/addon/preset.js': 'export const unused = 1;\n' }, changes: true },
        { case: 'the compilerOptions of a tsconfig outside the project its extends chain reaches', edit: { 'tsconfig.shared.json': '{"extends":"./tsconfig.base.json","compilerOptions":{"jsx":"preserve"}}' }, changes: true },
        { case: 'a file outside the project that no stories glob matches', edit: { 'libs/ui/button.tsx': 'export const Button = 2;\n' }, changes: false },
        { case: 'the main config the production input leaves out', edit: { 'apps/site/.storybook/main.ts': "import { stories } from '../../../tools/storybook/shared.ts';\n\nexport default { stories, framework: '@storybook/react-vite', addons: ['addon'], docs: {} };\n" }, changes: true },
    ])('hashes build--storybook again after an edit to $case: $changes', async ({ edit, changes }, { workspace }) => {
        const before = await hashed(workspace, await workspace.graph({ createNodes }), 'build--storybook');

        const after = await hashed(workspace, await workspace.write(edit).graph({ createNodes }), 'build--storybook');

        expect({ planned: before !== undefined && after !== undefined, changed: before !== after }).toStrictEqual({ planned: true, changed: changes });
    });

    it.for([
        { case: 'a module the main config imports from inside the folder of a project nested in its own', edit: { 'apps/site/sub/shared.ts': "export const stories = ['../src', '../sub/**/*.stories.tsx'];\nexport const unused = 1;\n" } },
        { case: 'the compilerOptions of a tsconfig inside the folder of a project nested in its own that its tsconfig extends', edit: { 'apps/site/sub/tsconfig.json': '{"extends":"../../../tsconfig.shared.json","compilerOptions":{"jsx":"preserve"}}' } },
        { case: 'a story under a stories glob inside the folder of a project nested in its own', edit: { 'apps/site/sub/card.stories.tsx': 'export default { title: "Card" };\n' } },
        { case: 'a module the main config imports from its own folder outside the nested project', edit: { 'apps/site/addons.ts': "export const addons = ['addon'];\nexport const unused = 1;\n" } },
    ])('hashes build--storybook of a project holding another project again after an edit to $case', async ({ edit }, { workspace }) => {
        workspace.write({
            'apps/site/sub/project.json': '{"name":"sub"}',
            'apps/site/sub/shared.ts': "export const stories = ['../src', '../sub/**/*.stories.tsx'];\n",
            'apps/site/sub/tsconfig.json': '{"extends":"../../../tsconfig.shared.json","compilerOptions":{"jsx":"react-jsx"}}',
            'apps/site/sub/card.stories.tsx': 'export default {};\n',
            'apps/site/tsconfig.json': '{"extends":"./sub/tsconfig.json"}',
            'apps/site/addons.ts': "export const addons = ['addon'];\n",
            'apps/site/.storybook/main.ts': "import { stories } from '../sub/shared.ts';\nimport { addons } from '../addons.ts';\n\nexport default { stories, framework: '@storybook/react-vite', addons };\n",
        });
        const before = await hashed(workspace, await workspace.graph({ createNodes }), 'build--storybook');

        const after = await hashed(workspace, await workspace.write(edit).graph({ createNodes }), 'build--storybook');

        expect({ planned: before !== undefined && after !== undefined, changed: before !== after }).toStrictEqual({ planned: true, changed: true });
    });

    it.for([
        { case: 'the module of a workspace package the main config imports', edit: { 'libs/helper/index.js': "export const stories = ['../src'];\nexport const unused = 1;\n" }, changes: true },
        { case: 'a file of that package the main config never loads', edit: { 'libs/helper/other.js': 'export const unused = 2;\n' }, changes: false },
    ])('hashes build--storybook again after an edit to $case: $changes', async ({ edit, changes }, { workspace }) => {
        workspace.write({
            'libs/helper/package.json': '{"name":"helper","type":"module","exports":"./index.js"}',
            'libs/helper/index.js': "export const stories = ['../src'];\n",
            'libs/helper/other.js': 'export const unused = 1;\n',
            'apps/site/.storybook/main.ts': "import { stories } from 'helper';\n\nexport default { stories, framework: '@storybook/react-vite' };\n",
        });
        symlinkSync(join(workspace.root, 'libs/helper'), join(workspace.root, 'node_modules/helper'), 'dir');
        const before = await hashed(workspace, await workspace.graph({ createNodes }), 'build--storybook');

        const after = await hashed(workspace, await workspace.write(edit).graph({ createNodes }), 'build--storybook');

        expect({ planned: before !== undefined && after !== undefined, changed: before !== after }).toStrictEqual({ planned: true, changed: changes });
    });

    it.for([
        { moved: 'storybook', changes: true },
        { moved: '@storybook/react-vite', changes: true },
        { moved: 'unread', changes: false },
    ])('hashes build--storybook again after a new release of the installed package $moved: $changes', async ({ moved, changes }, { workspace }) => {
        const graph = await workspace.graph({ createNodes });

        const [before, after] = await Promise.all([hashed(workspace, graph, 'build--storybook'), hashed(workspace, graph, 'build--storybook', { ...INSTALLED, [moved]: '12.0.0' })]);

        expect({ planned: before !== undefined && after !== undefined, changed: before !== after }).toStrictEqual({ planned: true, changed: changes });
    });
});

describe('loading the main config across graph passes', () => {
    it('refuses a main config that throws, naming the config', async ({ workspace }) => {
        const graphed = workspace.write({ 'apps/site/.storybook/main.ts': "throw new Error('main refused');\n" }).graph({ createNodes });

        await expect(graphed).rejects.toSatisfy(refused(/^apps\/site\/\.storybook\/main\.ts does not load as a Storybook main config/u));
    });

    it('reads an edited main config again in the same process', async ({ workspace }) => {
        await workspace.graph({ createNodes });

        const graph = await workspace.write({ 'apps/site/.storybook/main.ts': "export default { stories: ['../docs/**/*.mdx'] };\n" }).graph({ createNodes });

        expect(graph.nodes['site']?.data.targets?.['build--storybook']?.inputs?.filter((input) => Predicate.isString(input) && input.startsWith('{workspaceRoot}/apps/site/'))).toStrictEqual(['{workspaceRoot}/apps/site/docs/**/*.mdx', '{workspaceRoot}/apps/site/.storybook/main.ts']);
    });

    it('loads the main config once while edits reach no file it loads, and again after one does', async ({ workspace, onTestFinished }) => {
        const counter = `${workspace.root}.loads`;
        onTestFinished(() => rmSync(counter, { force: true }));
        const loads = () => (existsSync(counter) ? readFileSync(counter, 'utf-8').length : 0);
        await workspace.write({ 'apps/site/.storybook/main.ts': `import { appendFileSync } from 'node:fs';\nimport { stories } from '../../../tools/storybook/shared.ts';\n\nappendFileSync(${JSON.stringify(counter)}, 'x');\n\nexport default { stories };\n` }).graph({ createNodes });

        await workspace.write({ 'docs/guide.md': '# Guide\n\nMore.\n', 'apps/site/src/label.stories.tsx': 'export default { title: "Label" };\n' }).graph({ createNodes });
        const reused = loads();
        await workspace.write({ 'tools/storybook/shared.ts': "export const stories = ['../src'];\n" }).graph({ createNodes });

        expect([reused, loads()]).toStrictEqual([1, 2]);
    });
});
