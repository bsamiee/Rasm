import { readJson } from '@nx/devkit';
import { FsTree, flushChanges } from '@nx/devkit/internal';
import { generate } from '@rasm/nx/generators/executor-schemas';
import { it as workspaced } from '@rasm/testing/nx';
import { combineOptionsForExecutor } from 'nx/src/utils/params';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit'), async (actual) => ({ ...(await actual()), formatFiles: vi.fn<() => Promise<void>>().mockResolvedValue(undefined) }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = workspaced.extend('tree', ({ workspace }) => {
    workspace.write({
        'package.json': '{ "name": "repo", "private": true, "workspaces": ["packages/*"] }',
        'nx.json': '{}',
        'packages/plugin/package.json': '{ "name": "plugin", "type": "module", "executors": "./executors.json" }',
        'packages/plugin/executors.json': '{ "executors": { "run": { "implementation": "./executor.js#execute", "schema": "./schema.json", "description": "Stale" } } }',
        'packages/plugin/executor.js':
            "import { Schema } from 'effect';\n\nexport const Run = Schema.Struct({ cwd: Schema.String.annotate({ description: 'Folder the command runs in', default: '.' }) }).annotate({ title: 'Run', description: 'Runs the command in its folder' });\nexport const execute = () => Promise.resolve({ success: true });\n",
        'packages/other/package.json': '{ "name": "other" }',
    });
    workspace.install(import.meta.resolve('effect/package.json'));
    return new FsTree(workspace.root, false);
});

describe('executor-schemas sync generator', () => {
    it('writes a schema.json Nx applies the declared default and type of each option from', async ({ tree }) => {
        await generate(tree);

        const schema = readJson<Parameters<typeof combineOptionsForExecutor>[3]>(tree, 'packages/plugin/schema.json');

        expect(combineOptionsForExecutor({}, '', { options: {} }, schema, 'plugin', null)).toStrictEqual({ cwd: '.' });
        expect(() => {
            combineOptionsForExecutor({ cwd: 3 }, '', { options: {} }, schema, 'plugin', null);
        }).toThrow(/cwd/u);
    });

    it('writes the description the options schema declares into executors.json', async ({ tree }) => {
        await generate(tree);

        expect(readJson<{ readonly executors: Readonly<Record<string, { readonly description: string }>> }>(tree, 'packages/plugin/executors.json').executors['run']?.description).toBe('Runs the command in its folder');
    });

    it('names each missing or stale file in the details Nx prints', async ({ tree }) => {
        const { outOfSyncDetails } = await generate(tree);

        expect(outOfSyncDetails.toSorted()).toStrictEqual(['packages/plugin/executors.json', 'packages/plugin/schema.json']);
    });

    it('changes nothing once every schema.json and description is in sync', async ({ workspace, tree }) => {
        await generate(tree);
        flushChanges(workspace.root, tree.listChanges());
        const synced = new FsTree(workspace.root, false);

        await generate(synced);

        expect(synced.listChanges()).toStrictEqual([]);
    });
});
