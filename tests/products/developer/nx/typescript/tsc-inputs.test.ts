import { mkdirSync, symlinkSync } from 'node:fs';
import { join } from 'node:path';
import { createNodes } from '@rasm/nx-typescript';
import { it, typescript } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIB = { 'package.json': '{"name":"root","workspaces":["libs/*"]}', 'package-lock.json': '', 'libs/lib/package.json': '{"name":"lib"}' };

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('tsc target inputs', () => {
    it.for([
        {
            case: 'include and exclude globs select the files',
            files: {
                'libs/lib/tsconfig.json': '{"include":["src/**/*.ts"],"exclude":["src/**/*.spec.ts"],"compilerOptions":{"noEmit":true}}',
                'libs/lib/src/index.ts': 'export {};',
                'libs/lib/src/index.spec.ts': 'export {};',
                'libs/lib/tools/gen.ts': 'export {};',
            },
            hashed: ['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{projectRoot}/src/index.ts'],
        },
        {
            case: 'an include of the parent folder reaches files outside the project',
            files: { 'libs/lib/tsconfig.json': '{"include":[".."],"compilerOptions":{"noEmit":true}}', 'libs/lib/index.ts': 'export {};', 'libs/shared/units.ts': 'export {};' },
            hashed: ['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{projectRoot}/index.ts', '{workspaceRoot}/libs/shared/units.ts'],
        },
        {
            case: 'a listed file imports a file no files entry names',
            files: { 'libs/lib/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"noEmit":true,"module":"preserve"}}', 'libs/lib/index.ts': "export * from './helper';", 'libs/lib/helper.ts': 'export const helper = 1;', 'libs/lib/unused.ts': 'export {};' },
            hashed: ['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{projectRoot}/index.ts', '{projectRoot}/helper.ts'],
        },
        {
            case: 'a listed file imports a json file under resolveJsonModule',
            files: {
                'libs/lib/package.json': '{"name":"lib","type":"module"}',
                'libs/lib/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"noEmit":true,"module":"nodenext","resolveJsonModule":true}}',
                'libs/lib/index.ts': "import data from './data.json' with { type: 'json' };\nexport const value = data.value;",
                'libs/lib/data.json': '{"value":1}',
            },
            hashed: ['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{projectRoot}/index.ts', '{projectRoot}/data.json'],
        },
        {
            case: 'the tsconfig extends a chain of workspace files',
            files: {
                'tsconfig.base.json': '{"compilerOptions":{"strict":true}}',
                'tsconfig.foo.json': '{"extends":"./tsconfig.base.json"}',
                'tsconfig.bar.json': '{"compilerOptions":{"noEmit":true}}',
                'libs/lib/tsconfig.json': '{"extends":["../../tsconfig.foo.json","../../tsconfig.bar.json"],"files":["index.ts"]}',
                'libs/lib/index.ts': 'export {};',
            },
            hashed: ['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{projectRoot}/index.ts', '{workspaceRoot}/tsconfig.foo.json', '{workspaceRoot}/tsconfig.base.json', '{workspaceRoot}/tsconfig.bar.json'],
        },
        {
            case: 'a solution tsconfig references programs beside it',
            files: {
                'libs/lib/tsconfig.json': '{"files":[],"references":[{"path":"./tsconfig.lib.json"},{"path":"./tsconfig.spec.json"}]}',
                'libs/lib/tsconfig.lib.json': '{"include":["src/**/*.ts"],"exclude":["src/**/*.spec.ts"],"compilerOptions":{"noEmit":true}}',
                'libs/lib/tsconfig.spec.json': '{"include":["src/**/*.spec.ts"],"compilerOptions":{"noEmit":true}}',
                'libs/lib/src/index.ts': 'export {};',
                'libs/lib/src/index.spec.ts': 'export {};',
            },
            hashed: ['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{projectRoot}/tsconfig.lib.json', '{projectRoot}/tsconfig.spec.json', '{projectRoot}/src/index.ts', '{projectRoot}/src/index.spec.ts'],
        },
        {
            case: 'the tsconfig references programs of another folder, through a cycle and a missing path',
            files: {
                'libs/lib/tsconfig.json': '{"files":["index.ts"],"references":[{"path":"../other/tsconfig.lib.json"},{"path":"../missing"}]}',
                'libs/lib/index.ts': 'export {};',
                'libs/other/tsconfig.lib.json': '{"extends":"./tsconfig.shared.json","include":["src"],"references":[{"path":"./tsconfig.core.json"}]}',
                'libs/other/tsconfig.shared.json': '{"compilerOptions":{"composite":true}}',
                'libs/other/tsconfig.core.json': '{"include":["core"],"references":[{"path":"../lib"}]}',
                'libs/other/tsconfig.spec.json': '{"include":["src"]}',
                'libs/other/src/index.ts': 'export {};',
                'libs/other/core/index.ts': 'export {};',
            },
            hashed: ['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{projectRoot}/index.ts', '{workspaceRoot}/libs/other/tsconfig.lib.json', '{workspaceRoot}/libs/other/tsconfig.shared.json', '{workspaceRoot}/libs/other/tsconfig.core.json'],
        },
    ])('hashes the files tsc reads when $case', async ({ files, hashed }, { workspace }) => {
        const graph = await workspace
            .install(typescript)
            .write({ ...LIB, ...files })
            .graph({ createNodes });

        expect(graph.nodes['lib']?.data.targets?.['check--tsc']?.inputs?.filter((input) => typeof input === 'string' && input.startsWith('{')).toSorted()).toStrictEqual(hashed.toSorted());
    });

    it('hashes a tsconfig a workspace package provides through node_modules at its workspace path, and drops an installed one', async ({ workspace }) => {
        workspace.install(typescript).write({
            ...LIB,
            'libs/preset/package.json': '{"name":"@scope/preset"}',
            'libs/preset/tsconfig.preset.json': '{"compilerOptions":{"strict":true}}',
            'node_modules/@tsconfig/strictest/package.json': '{"name":"@tsconfig/strictest"}',
            'node_modules/@tsconfig/strictest/tsconfig.json': '{"compilerOptions":{"noEmit":true}}',
            'libs/lib/tsconfig.json': '{"extends":["@scope/preset/tsconfig.preset.json","@tsconfig/strictest/tsconfig.json"],"files":["index.ts"]}',
            'libs/lib/index.ts': 'export {};',
        });
        mkdirSync(join(workspace.root, 'node_modules/@scope'));
        symlinkSync(join(workspace.root, 'libs/preset'), join(workspace.root, 'node_modules/@scope/preset'), 'dir');

        const graph = await workspace.graph({ createNodes });

        expect(graph.nodes['lib']?.data.targets?.['check--tsc']?.inputs?.filter((input) => typeof input === 'string' && input.startsWith('{')).toSorted()).toStrictEqual(['{projectRoot}/index.ts', '{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{workspaceRoot}/libs/preset/tsconfig.preset.json']);
    });
});
