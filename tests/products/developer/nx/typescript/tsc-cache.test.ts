import { rmSync } from 'node:fs';
import { join } from 'node:path';
import type { ProjectGraph } from '@nx/devkit';
import { createNodes } from '@rasm/nx-typescript';
import { it, typescript } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKSPACE = {
    'package.json': '{"name":"root","workspaces":["libs/*","apps/*"]}',
    'package-lock.json': '',
    'tsconfig.base.json': '{"compilerOptions":{"strict":true,"noEmit":true}}',
    'apps/app/package.json': '{"name":"app"}',
    'apps/app/tsconfig.json': '{"extends":"../../tsconfig.base.json","files":["main.ts"],"references":[{"path":"../../libs/shared"}],"compilerOptions":{"module":"preserve"}}',
    'apps/app/main.ts': "export * from './helper';\n",
    'apps/app/helper.ts': 'export const helper = 1;\n',
    'libs/shared/tsconfig.json': '{"files":[],"references":[{"path":"./tsconfig.one.json"}]}',
    'libs/shared/tsconfig.one.json': '{"files":["index.ts"]}',
    'libs/shared/tsconfig.two.json': '{"files":["index.ts"]}',
    'libs/shared/index.ts': 'export {};\n',
    'libs/lib/package.json': '{"name":"lib"}',
    'libs/lib/tsconfig.json': '{"include":["src"],"compilerOptions":{"noEmit":true}}',
    'libs/lib/src/index.ts': 'export {};\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const targets = (graph: ProjectGraph) => Object.fromEntries(Object.values(graph.nodes).map(({ name, data }) => [name, data.targets]));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('tsc inference reused across graph runs', () => {
    it.for([
        { case: 'a missing tsconfig another folder references appears', before: { 'libs/shared/tsconfig.json': '{"files":[],"references":[{"path":"./tsconfig.one.json"},{"path":"./tsconfig.four.json"}]}' }, files: { 'libs/shared/tsconfig.four.json': '{"files":["index.ts"]}' } },
        { case: 'a file appears in a new folder the include covers', before: {}, files: { 'libs/lib/src/nested/added.ts': 'export {};\n' } },
        { case: 'a listed file starts importing a file no files entry names', before: {}, files: { 'apps/app/helper.ts': "export * from './extra';\n", 'apps/app/extra.ts': 'export const extra = 2;\n' } },
        { case: 'the tsconfig another folder holds references another program', before: {}, files: { 'libs/shared/tsconfig.json': '{"files":[],"references":[{"path":"./tsconfig.two.json"}]}' } },
        {
            case: 'a gitignored source starts importing a file outside the project',
            before: { '.gitignore': 'libs/lib/src/generated.ts\n', 'libs/lib/src/generated.ts': 'export {};\n', 'libs/other/x.ts': 'export const x = 1;\n' },
            files: { 'libs/lib/src/generated.ts': "export * from '../../other/x';\n" },
        },
        { case: 'the extended base config turns emitting on', before: {}, files: { 'tsconfig.base.json': '{"compilerOptions":{"strict":true,"outDir":"dist"}}' } },
    ])('infers from a warm cache what a cold read infers after $case', async ({ before, files }, { workspace }) => {
        workspace.install(typescript).write({ ...WORKSPACE, ...before });
        const initial = targets(await workspace.graph({ createNodes }));

        const warm = targets(await workspace.write(files).graph({ createNodes }));
        rmSync(join(workspace.root, 'tmp/project-graph-cache'), { recursive: true, force: true });
        const cold = targets(await workspace.graph({ createNodes }));

        expect(cold).not.toStrictEqual(initial);
        expect(warm).toStrictEqual(cold);
    });

    it('infers the targets it read before once the compiler stops resolving, while every config and source is unchanged', async ({ workspace }) => {
        workspace.install(typescript).write(WORKSPACE);
        const fresh = targets(await workspace.graph({ createNodes }));

        const cached = targets(await workspace.remove('node_modules/typescript').graph({ createNodes }));

        expect([fresh['app']?.['check--tsc'], fresh['lib']?.['check--tsc']]).not.toContain(undefined);
        expect(cached).toStrictEqual(fresh);
    });
});
