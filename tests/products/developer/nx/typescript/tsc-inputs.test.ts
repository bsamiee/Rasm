import { spawnSync } from 'node:child_process';
import { mkdirSync, symlinkSync } from 'node:fs';
import { join, relative } from 'node:path';
import { createNodes } from '@rasm/nx-typescript';
import { it, typescript } from '@rasm/testing/nx';
import { HashPlanInspector } from 'nx/src/hasher/hash-plan-inspector';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIB = { 'package.json': '{"name":"root","workspaces":["libs/*"]}', 'package-lock.json': '', 'libs/lib/package.json': '{"name":"lib"}', 'libs/lib/decoy.ts': 'export {};\n' };

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
        const graph = await workspace.graph({ createNodes });
        const inspector = new HashPlanInspector({ ...graph, externalNodes: { 'npm:typescript': { type: 'npm', name: 'npm:typescript', data: { packageName: 'typescript', version: '7.0.0' } } } }, workspace.root);
        await inspector.init();

        const read = programs
            .flatMap((program) => spawnSync(join(workspace.root, 'node_modules/.bin/tsc'), ['-p', program, '--listFilesOnly'], { cwd: join(workspace.root, 'libs/lib'), encoding: 'utf-8' }).stdout.split('\n'))
            .filter((file) => file.length > 0)
            .map((file) => relative(workspace.root, file))
            .filter((file) => !(file.startsWith('..') || file.includes('node_modules')));
        const hashed = new Set(inspector.inspectTaskInputs({ project: 'lib', target: 'check--tsc' })['lib:check--tsc']?.files);

        expect(read.length).toBeGreaterThan(0);
        expect([...read, ...configs].filter((file) => !hashed.has(file))).toStrictEqual([]);
        expect(['libs/lib/decoy.ts', ...unread].filter((file) => hashed.has(file))).toStrictEqual([]);
    });
});
