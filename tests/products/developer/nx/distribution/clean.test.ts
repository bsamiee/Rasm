import { existsSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import { execPath } from 'node:process';
import { createNodes } from '@rasm/nx-distribution';
import { Clean, execute } from '@rasm/nx-distribution/executors/clean';
import { it } from '@rasm/testing/nx';
import { Schema } from 'effect';
import { assert, describe, expect, vi } from 'vitest';

vi.mock(import('node:process'), async (actual) => {
    const platform: NodeJS.Platform = 'darwin';
    return { ...(await actual()), platform };
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const lib = {
    '.gitignore': 'packed\nbuilt.txt\n.artifacts\ncleaned.txt\n',
    'tools/record.js': "import { writeFileSync } from 'node:fs';\n\nconst [file, ...argv] = process.argv.slice(2);\nwriteFileSync(file, argv.join('\\n'));\n",
    'lib/index.js': 'export const lib = 1;\n',
    'lib/packed/lib.tgz': 'archive',
    'lib/staged/built.txt': 'built',
    'lib/staged/tracked.txt': 'source',
    '.artifacts/distribution/lib/lib.zip': 'archive',
    '.artifacts/release/lib/lib.zip': 'archive',
    'lib/project.json': JSON.stringify({
        name: 'lib',
        targets: {
            pack: { executor: 'nx:noop', outputs: ['{projectRoot}/packed/lib.tgz'] },
            stage: { executor: 'nx:noop', outputs: ['{projectRoot}/staged'] },
            archive: { executor: 'nx:noop', outputs: ['{options.outputPath}'], options: { outputPath: '.artifacts/distribution/lib' }, configurations: { release: { outputPath: '.artifacts/release/lib' } } },
        },
    }),
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('clean executor', () => {
    it('removes what the inferred build wrote and keeps the manifest it read', async ({ workspace }) => {
        const graph = await workspace.write({ '.gitignore': '*.yak\n', 'rhino/manifest.yml': 'name: marker\nversion: 1.2.3\nauthors: [Suite]\ndescription: Marks points\n', 'rhino/marker-1.2.3-rh8-any.yak': 'archive' }).graph({ createNodes });

        const stated: unknown = { commands: [], cwd: '.', ...graph.nodes['marker']?.data.targets?.['clean--yak']?.options };
        assert(Schema.is(Clean)(stated), 'clean--yak states clean options');

        await execute(stated, workspace.context(graph, 'marker', 'clean--yak'));

        expect(['rhino/marker-1.2.3-rh8-any.yak', 'rhino/manifest.yml'].filter((file) => existsSync(join(workspace.root, file)))).toStrictEqual(['rhino/manifest.yml']);
    });

    it('removes each declared output and keeps each file the workspace tracks under one', async ({ workspace }) => {
        const graph = await workspace.write(lib).graph({ createNodes });

        await execute({ targets: ['pack', 'stage'], commands: [], cwd: '.' }, workspace.context(graph, 'lib', 'clean'));

        expect(['lib/packed/lib.tgz', 'lib/staged/built.txt', 'lib/staged/tracked.txt', 'lib/index.js'].filter((file) => existsSync(join(workspace.root, file)))).toStrictEqual(['lib/staged/tracked.txt', 'lib/index.js']);
    });

    it('removes the outputs of every configuration of a target', async ({ workspace }) => {
        const graph = await workspace.write(lib).graph({ createNodes });

        await execute({ targets: ['archive'], commands: [], cwd: '.' }, workspace.context(graph, 'lib', 'clean'));

        expect(['.artifacts/distribution/lib', '.artifacts/release/lib', '.artifacts'].filter((file) => existsSync(join(workspace.root, file)))).toStrictEqual(['.artifacts']);
    });

    it("runs the tool's own clean commands in the folder cwd names", async ({ workspace }) => {
        const graph = await workspace.write(lib).graph({ createNodes });

        await execute({ targets: [], commands: [[execPath, join(workspace.root, 'tools/record.js'), 'cleaned.txt', 'swept']], cwd: 'lib' }, workspace.context(graph, 'lib', 'clean'));

        expect(readFileSync(join(workspace.root, 'lib/cleaned.txt'), 'utf-8')).toBe('swept');
    });
});
