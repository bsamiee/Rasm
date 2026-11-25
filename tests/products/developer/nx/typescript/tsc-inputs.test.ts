import { spawnSync } from 'node:child_process';
import { mkdirSync, mkdtempSync, readFileSync, realpathSync, rmSync, symlinkSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { basename, dirname, join, relative } from 'node:path';
import { env } from 'node:process';
import type { CreateNodes, ProjectGraph } from '@nx/devkit';
import { createNodes } from '@rasm/nx-typescript';
import { it, taskGraph, typescript, type Workspace } from '@rasm/testing/nx';
import { readNxJson } from 'nx/src/config/nx-json';
import { createTaskHasher } from 'nx/src/hasher/create-task-hasher';
import { HashPlanInspector } from 'nx/src/hasher/hash-plan-inspector';
import { assert, describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIB = { 'package.json': '{"name":"root","workspaces":["libs/*"]}', 'package-lock.json': '', 'libs/lib/package.json': '{"name":"lib"}', 'libs/lib/decoy.ts': 'export {};\n' };
const LAYERED = {
    '.gitignore': 'dist\n',
    'package.json': '{"name":"root","workspaces":["libs/*"]}',
    'package-lock.json': '',
    'libs/up/package.json': '{"name":"up"}',
    'libs/up/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"composite":true,"outDir":"dist","module":"preserve"}}',
    'libs/up/index.ts': 'export const up = (value: number): number => value + 1;\n',
    'libs/up/notes.md': '# up\n',
    'libs/side/package.json': '{"name":"side"}',
    'libs/side/index.ts': 'export const side = 2;\n',
    'libs/down/package.json': '{"name":"down","nx":{"implicitDependencies":["up"]}}',
    'libs/down/tsconfig.json': '{"files":["index.ts"],"references":[{"path":"../up"}],"compilerOptions":{"noEmit":true,"module":"preserve"}}',
    'libs/down/index.ts': "import { side } from '../side/index';\nimport { up } from '../up/index';\nexport const down: string = up(side);\n",
};
const TYPESCRIPT = { 'npm:typescript': { type: 'npm', name: 'npm:typescript', data: { packageName: 'typescript', version: '7.0.0' } } } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const compiled = (workspace: Workspace, folder: string, ...args: readonly string[]) => spawnSync(join(workspace.root, 'node_modules/.bin/tsc'), args, { cwd: join(workspace.root, folder), encoding: 'utf-8' }).stdout;

const listed = (workspace: Workspace, folder: string, program: string) =>
    compiled(workspace, folder, '-p', program, '--listFilesOnly')
        .split('\n')
        .filter((file) => file.length > 0)
        .map((file) => relative(workspace.root, file))
        .filter((file) => !(file.startsWith('..') || file.includes('node_modules')));

const graphed = async (workspace: Workspace): Promise<ProjectGraph> => ({ ...(await workspace.graph({ createNodes })), externalNodes: TYPESCRIPT });

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('check--tsc hashes the files tsc reads', () => {
    it.for([
        {
            case: 'include and exclude globs select the files',
            programs: ['tsconfig.json'],
            configs: ['libs/lib/tsconfig.json'],
            unread: ['libs/lib/src/index.spec.ts'],
            files: { 'libs/lib/tsconfig.json': '{"include":["src/**/*.ts"],"exclude":["src/**/*.spec.ts"],"compilerOptions":{"noEmit":true}}', 'libs/lib/src/index.ts': 'export {};', 'libs/lib/src/index.spec.ts': 'export {};' },
        },
        { case: 'an include of the parent folder reaches files of another folder', programs: ['tsconfig.json'], configs: ['libs/lib/tsconfig.json'], unread: [], files: { 'libs/lib/tsconfig.json': '{"include":["../shared"],"compilerOptions":{"noEmit":true}}', 'libs/shared/units.ts': 'export {};' } },
        {
            case: 'a listed file imports a file no files entry names',
            programs: ['tsconfig.json'],
            configs: ['libs/lib/tsconfig.json'],
            unread: [],
            files: { 'libs/lib/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"noEmit":true,"module":"preserve"}}', 'libs/lib/index.ts': "export * from './helper';", 'libs/lib/helper.ts': 'export const helper = 1;' },
        },
        {
            case: 'a listed file imports json under resolveJsonModule',
            programs: ['tsconfig.json'],
            configs: ['libs/lib/tsconfig.json'],
            unread: [],
            files: {
                'libs/lib/package.json': '{"name":"lib","type":"module"}',
                'libs/lib/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"noEmit":true,"module":"nodenext","resolveJsonModule":true}}',
                'libs/lib/index.ts': "import data from './data.json' with { type: 'json' };\nexport const value = data.value;",
                'libs/lib/data.json': '{"value":1}',
            },
        },
        {
            case: 'a solution tsconfig references programs beside it',
            programs: ['tsconfig.lib.json', 'tsconfig.spec.json'],
            configs: ['libs/lib/tsconfig.json', 'libs/lib/tsconfig.lib.json', 'libs/lib/tsconfig.spec.json'],
            unread: [],
            files: {
                'libs/lib/tsconfig.json': '{"files":[],"references":[{"path":"./tsconfig.lib.json"},{"path":"./tsconfig.spec.json"}]}',
                'libs/lib/tsconfig.lib.json': '{"include":["src/**/*.ts"],"exclude":["src/**/*.spec.ts"],"compilerOptions":{"composite":true,"noEmit":true}}',
                'libs/lib/tsconfig.spec.json': '{"include":["src/**/*.spec.ts"],"compilerOptions":{"composite":true,"noEmit":true}}',
                'libs/lib/src/index.ts': 'export {};',
                'libs/lib/src/index.spec.ts': 'export {};',
            },
        },
        {
            case: 'the tsconfig extends a chain of workspace files and a workspace package preset',
            programs: ['tsconfig.json'],
            configs: ['libs/lib/tsconfig.json', 'tsconfig.foo.json', 'tsconfig.base.json', 'libs/preset/tsconfig.preset.json'],
            unread: [],
            files: {
                'tsconfig.base.json': '{"compilerOptions":{"strict":true}}',
                'tsconfig.foo.json': '{"extends":"./tsconfig.base.json"}',
                'libs/preset/package.json': '{"name":"@scope/preset"}',
                'libs/preset/tsconfig.preset.json': '{"compilerOptions":{"noEmit":true}}',
                'libs/lib/tsconfig.json': '{"extends":["../../tsconfig.foo.json","@scope/preset/tsconfig.preset.json"],"files":["index.ts"]}',
                'libs/lib/index.ts': 'export {};',
            },
        },
    ])('hashes every workspace file tsc --listFilesOnly lists and every config it reads, and no other source, when $case', async ({ programs, configs, unread, files }, { workspace }) => {
        workspace.install(typescript).write({ ...LIB, ...files });
        mkdirSync(join(workspace.root, 'node_modules/@scope'));
        symlinkSync(join(workspace.root, 'libs/preset'), join(workspace.root, 'node_modules/@scope/preset'), 'dir');
        const inspector = new HashPlanInspector(await graphed(workspace), workspace.root);
        await inspector.init();

        const read = programs.flatMap((program) => listed(workspace, 'libs/lib', program));
        const hashed = new Set(inspector.inspectTaskInputs({ project: 'lib', target: 'check--tsc' })['lib:check--tsc']?.files);

        expect(read.length).toBeGreaterThan(0);
        expect([...read, ...configs].filter((file) => !hashed.has(file))).toStrictEqual([]);
        expect(['libs/lib/decoy.ts', ...unread].filter((file) => hashed.has(file))).toStrictEqual([]);
    });

    it('hashes a declaration file tsc reads through a gitignored directory link to a folder outside the workspace, and the task hash follows its content', async ({ workspace, onTestFinished }) => {
        const outside = realpathSync(mkdtempSync(join(tmpdir(), 'nx-outside-')));
        onTestFinished(() => {
            rmSync(outside, { recursive: true, force: true });
        });
        writeFileSync(join(outside, 'outside.d.ts'), "declare module 'outside' {\n    export const value: number;\n}\n");
        workspace.install(typescript).write({
            ...LIB,
            '.gitignore': 'libs/lib/types/outside\n',
            'libs/lib/tsconfig.json': '{"include":["*.ts","types/**/*.d.ts"],"compilerOptions":{"noEmit":true,"module":"preserve"}}',
            'libs/lib/index.ts': "import { value } from 'outside';\nexport const doubled: number = value * 2;\n",
        });
        mkdirSync(join(workspace.root, 'libs/lib/types'));
        symlinkSync(outside, join(workspace.root, 'libs/lib/types/outside'), 'dir');
        const hashed = async () => {
            const graph = await graphed(workspace);
            const tasks = taskGraph(graph, ['lib'], ['check--tsc']);
            const task = tasks.tasks['lib:check--tsc'];
            assert(task !== undefined, 'lib:check--tsc is a task of the graph');
            const inspector = new HashPlanInspector(graph, workspace.root);
            await inspector.init();
            const { value } = await createTaskHasher(graph, readNxJson(workspace.root)).hashTask(task, tasks, env);
            return { files: inspector.inspectTaskInputs({ project: 'lib', target: 'check--tsc' })['lib:check--tsc']?.files ?? [], hash: value };
        };

        const before = await hashed();
        writeFileSync(join(outside, 'outside.d.ts'), "declare module 'outside' {\n    export const value: string;\n}\n");
        const after = await hashed();

        expect(listed(workspace, 'libs/lib', 'tsconfig.json')).toContain('libs/lib/types/outside/outside.d.ts');
        expect(before.files).toContain('libs/lib/types/outside/outside.d.ts');
        expect(after.hash).not.toBe(before.hash);
    });

    it('hashes in check--tsc of the root project the tsconfig of each nested project its program references', async ({ workspace }) => {
        workspace.install(typescript).write({
            ...LIB,
            'tsconfig.json': '{"files":[],"references":[{"path":"./tsconfig.node.json"}]}',
            'tsconfig.node.json': '{"files":["tool.ts"],"references":[{"path":"./libs/lib"}],"compilerOptions":{"composite":true,"outDir":"dist/node","module":"preserve"}}',
            'tool.ts': 'export const tool = 1;\n',
            'libs/lib/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"composite":true,"outDir":"dist","module":"preserve"}}',
            'libs/lib/index.ts': 'export const lib = 1;\n',
        });
        const inspector = new HashPlanInspector(await graphed(workspace), workspace.root);
        await inspector.init();

        const built = [...compiled(workspace, '.', '--build', '--dry', '--verbose').matchAll(/^\s+\* (?<config>\S+)$/gmu)].map(({ groups }) => groups?.['config'] ?? '');
        const hashed = new Set(inspector.inspectTaskInputs({ project: 'root', target: 'check--tsc' })['root:check--tsc']?.files);

        expect(built).toContain('libs/lib/tsconfig.json');
        expect(built.filter((config) => !hashed.has(config))).toStrictEqual([]);
    });

    it.for([
        { case: 'a source inside the folder of a project.json project nested in its own', nested: { 'libs/lib/sub/project.json': '{"name":"sub"}' }, edit: 'libs/lib/sub/index.ts' },
        {
            case: 'a source inside the folder of a workspace package with its own program nested in its own',
            nested: { 'package.json': '{"name":"root","workspaces":["libs/*","libs/lib/sub"]}', 'libs/lib/sub/package.json': '{"name":"sub"}', 'libs/lib/sub/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"noEmit":true}}' },
            edit: 'libs/lib/sub/index.ts',
        },
        { case: 'a source inside the folder of a project another plugin infers nested in its own', nested: { 'libs/lib/sub/Cargo.toml': '[package]\nname = "sub"\n' }, edit: 'libs/lib/sub/index.ts' },
        { case: 'a source in a folder of its own outside the nested project', nested: { 'libs/lib/sub/project.json': '{"name":"sub"}' }, edit: 'libs/lib/src/index.ts' },
        { case: 'a source directly in its own folder', nested: { 'libs/lib/sub/project.json': '{"name":"sub"}' }, edit: 'libs/lib/index.ts' },
    ])('hashes check--tsc of a project holding another project again after an edit to $case', async ({ nested, edit }, { workspace }) => {
        const crated: CreateNodes = [
            '**/{tsconfig.json,package.json,project.json,Cargo.toml}',
            async (files, options, context) => [
                ...(await createNodes[1](
                    files.filter((file) => basename(file) !== 'Cargo.toml'),
                    options,
                    context,
                )),
                ...files.filter((file) => basename(file) === 'Cargo.toml').map((file) => [file, { projects: { [dirname(file)]: { root: dirname(file), name: 'crate' } } }] as const),
            ],
        ];
        workspace.install(typescript).write({
            ...LIB,
            'libs/lib/tsconfig.json': '{"include":["**/*.ts"],"compilerOptions":{"noEmit":true}}',
            'libs/lib/index.ts': 'export const own = 1;\n',
            'libs/lib/src/index.ts': 'export const src = 1;\n',
            'libs/lib/sub/index.ts': 'export const sub = 1;\n',
            ...nested,
        });
        const hashed = async () => {
            const graph = { ...(await workspace.graph({ createNodes: crated })), externalNodes: TYPESCRIPT };
            const tasks = taskGraph(graph, ['lib'], ['check--tsc']);
            const task = tasks.tasks['lib:check--tsc'];
            assert(task !== undefined, 'lib:check--tsc is a task of the graph');
            const { value } = await createTaskHasher(graph, readNxJson(workspace.root)).hashTask(task, tasks, env);
            return value;
        };
        const before = await hashed();

        workspace.write({ [edit]: 'export const changed = 2;\n' });
        const after = await hashed();

        expect(listed(workspace, 'libs/lib', 'tsconfig.json')).toContain(edit);
        expect(after).not.toBe(before);
    });

    it("hashes the declarations tsc reads in place of a referenced project's sources through its build outputs, each upstream manifest, and sources imported without a reference, and no other upstream file", async ({ workspace }) => {
        workspace.install(typescript).write(LAYERED);
        compiled(workspace, 'libs/up', '-b');
        const inspector = new HashPlanInspector(await graphed(workspace), workspace.root);
        await inspector.init();

        const read = listed(workspace, 'libs/down', 'tsconfig.json');
        const { files = [], depOutputs = [] } = inspector.inspectTaskInputs({ project: 'down', target: 'check--tsc' })['down:check--tsc'] ?? {};

        expect(read).toContain('libs/side/index.ts');
        expect(read.filter((file) => !files.includes(file))).toStrictEqual(['libs/up/dist/index.d.ts']);
        expect(depOutputs).toContain('libs/up/dist/index.d.ts');
        expect(['libs/up/tsconfig.json', 'libs/up/package.json'].filter((file) => !files.includes(file))).toStrictEqual([]);
        expect(['libs/up/index.ts', 'libs/up/notes.md'].filter((file) => files.includes(file))).toStrictEqual([]);
    });

    it('keeps the check--tsc hash of a referencing project through an upstream body edit tsc emits identical declarations for, and changes it once the declarations change', async ({ workspace }) => {
        workspace.install(typescript).write(LAYERED);
        const state = async (edited: Readonly<Record<string, string>>) => {
            compiled(workspace.write(edited), 'libs/up', '-b');
            const graph = await graphed(workspace);
            const tasks = taskGraph(graph, ['down'], ['check--tsc']);
            const task = tasks.tasks['down:check--tsc'];
            assert(task !== undefined, 'down:check--tsc is a task of the graph');
            const { value } = await createTaskHasher(graph, readNxJson(workspace.root)).hashTask(task, tasks, env);
            return { hash: value, declarations: readFileSync(join(workspace.root, 'libs/up/dist/index.d.ts'), 'utf-8'), built: readFileSync(join(workspace.root, 'libs/up/dist/tsconfig.tsbuildinfo'), 'utf-8'), diagnostics: compiled(workspace, 'libs/down', '-p', 'tsconfig.json') };
        };
        const before = await state({});

        const body = await state({ 'libs/up/index.ts': 'export const up = (value: number): number => value + 2;\n' });
        const signature = await state({ 'libs/up/index.ts': 'export const up = (value: number): string => String(value + 2);\n' });

        expect(before.diagnostics).toContain('error TS2322');
        expect(body.built).not.toBe(before.built);
        expect({ ...body, built: before.built }).toStrictEqual(before);
        expect({ declarations: signature.declarations === body.declarations, hash: signature.hash === body.hash, diagnostics: signature.diagnostics }).toStrictEqual({ declarations: false, hash: false, diagnostics: '' });
    });
});
