import { symlinkSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { readJson } from '@nx/devkit';
import { FsTree, flushChanges } from '@nx/devkit/internal';
import { generate } from '@rasm/nx-distribution/generators/executor-schemas';
import { TempFs } from '@rasm/testing/temp-fs';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit'), async (actual) => ({
    ...(await actual()),
    formatFiles: vi.fn<() => Promise<void>>().mockResolvedValue(undefined),
}));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const tempFs = new TempFs('executor-schemas');
        onCleanup(() => tempFs.cleanup());
        tempFs.createFilesSync({
            'package.json': JSON.stringify({ name: 'repo', private: true, workspaces: ['packages/*'] }),
            'nx.json': '{}',
            'packages/plugin/package.json': JSON.stringify({ name: 'plugin', type: 'module', executors: './executors.json' }),
            'packages/plugin/executors.json': JSON.stringify({ executors: { run: { implementation: './executor.js#execute', schema: './schema.json', description: 'Stale' } } }),
            'packages/plugin/executor.js':
                "import { Schema } from 'effect';\n\nexport const Run = Schema.Struct({ cwd: Schema.String.annotate({ description: 'Folder the command runs in', default: '.' }) }).annotate({ title: 'Run', description: 'Runs the command in its folder' });\nexport const execute = () => Promise.resolve({ success: true });\n",
            'packages/other/package.json': JSON.stringify({ name: 'other' }),
        });
        tempFs.createDirSync('node_modules');
        symlinkSync(dirname(fileURLToPath(import.meta.resolve('effect/package.json'))), join(tempFs.tempDir, 'node_modules/effect'), 'dir');
        return tempFs;
    })
    .extend('tree', ({ tempFs }) => new FsTree(tempFs.tempDir, false));

describe('generate()', () => {
    it('should not make changes when every schema.json and executors.json description is in sync', async ({ tempFs, tree }) => {
        await generate(tree);
        flushChanges(tempFs.tempDir, tree.listChanges());
        const synced = new FsTree(tempFs.tempDir, false);

        await generate(synced);

        expect(synced.listChanges()).toStrictEqual([]);
    });

    it('should return an out of sync message naming each missing and stale file', async ({ tree }) => {
        const result = await generate(tree);

        expect(result.outOfSyncMessage).toBe('Executor schema.json files and executors.json descriptions differ from the options schemas their executors declare: packages/plugin/schema.json, packages/plugin/executors.json');
    });

    it('should sync each executor schema.json and executors.json description from the options schema its implementation declares', async ({ tree }) => {
        await generate(tree);

        expect(readJson(tree, 'packages/plugin/schema.json')).toMatchInlineSnapshot(`
          {
            "$schema": "http://json-schema.org/draft-07/schema#",
            "additionalProperties": true,
            "description": "Runs the command in its folder",
            "properties": {
              "cwd": {
                "default": ".",
                "description": "Folder the command runs in",
                "type": "string",
              },
            },
            "required": [
              "cwd",
            ],
            "title": "Run",
            "type": "object",
            "version": 2,
          }
        `);
        expect(readJson(tree, 'packages/plugin/executors.json')).toStrictEqual({ executors: { run: { implementation: './executor.js#execute', schema: './schema.json', description: 'Runs the command in its folder' } } });
    });
});
