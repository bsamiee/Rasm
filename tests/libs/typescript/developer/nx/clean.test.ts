import { existsSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import { execPath } from 'node:process';
import { execute } from '@rasm/nx/executors/clean';
import { it } from '@rasm/testing/nx';
import { describe, expect } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const lib = {
    '.gitignore': 'packed\nbuilt.txt\n.artifacts\ncleaned.txt\n',
    'tools/record.mjs': "import { writeFileSync } from 'node:fs';\n\nconst [file, ...argv] = process.argv.slice(2);\nwriteFileSync(file, argv.join('\\n'));\n",
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
            sign: { executor: 'nx:noop', outputs: ['{projectRoot}/packed', '!{projectRoot}/packed/lib.tgz'] },
            stage: { executor: 'nx:noop', outputs: ['{projectRoot}/staged'] },
            archive: { executor: 'nx:noop', outputs: ['{options.outputPath}'], options: { outputPath: '.artifacts/distribution/lib' }, configurations: { release: { outputPath: '.artifacts/release/lib' } } },
        },
    }),
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('clean executor', () => {
    it('removes each declared output and keeps each file the workspace tracks under one', async ({ workspace }) => {
        const graph = await workspace.write(lib).graph({});

        await execute({ targets: ['pack', 'stage'], commands: [], cwd: '.', folders: [] }, workspace.context(graph, 'lib', 'clean'));

        expect(['lib/packed/lib.tgz', 'lib/staged/built.txt', 'lib/staged/tracked.txt', 'lib/index.js'].filter((file) => existsSync(join(workspace.root, file)))).toStrictEqual(['lib/staged/tracked.txt', 'lib/index.js']);
    });

    it('keeps each file a negated output excludes from a declared output', async ({ workspace }) => {
        const graph = await workspace.write({ ...lib, 'lib/packed/lib.tgz.sig': 'signature' }).graph({});

        await execute({ targets: ['sign'], commands: [], cwd: '.', folders: [] }, workspace.context(graph, 'lib', 'clean'));

        expect(['lib/packed/lib.tgz', 'lib/packed/lib.tgz.sig'].filter((file) => existsSync(join(workspace.root, file)))).toStrictEqual(['lib/packed/lib.tgz']);
    });

    it('removes each folder under the project root a removal left empty', async ({ workspace }) => {
        const graph = await workspace.write(lib).graph({});

        await execute({ targets: ['pack'], commands: [], cwd: '.', folders: [] }, workspace.context(graph, 'lib', 'clean'));

        expect(['lib/packed', 'lib'].filter((file) => existsSync(join(workspace.root, file)))).toStrictEqual(['lib']);
    });

    it('removes a folder under the project root a removal left empty whose name starts with two dots', async ({ workspace }) => {
        const graph = await workspace.write({ ...lib, '.gitignore': `${lib['.gitignore']}..cache\n`, 'lib/..cache/out/built.txt': 'built', 'lib/project.json': JSON.stringify({ name: 'lib', targets: { cache: { executor: 'nx:noop', outputs: ['{projectRoot}/..cache/out'] } } }) }).graph({});

        await execute({ targets: ['cache'], commands: [], cwd: '.', folders: [] }, workspace.context(graph, 'lib', 'clean'));

        expect(['lib/..cache/out', 'lib/..cache', 'lib'].filter((file) => existsSync(join(workspace.root, file)))).toStrictEqual(['lib']);
    });

    it('removes the outputs of every configuration of a target', async ({ workspace }) => {
        const graph = await workspace.write(lib).graph({});

        await execute({ targets: ['archive'], commands: [], cwd: '.', folders: [] }, workspace.context(graph, 'lib', 'clean'));

        expect(['.artifacts/distribution/lib', '.artifacts/release/lib', '.artifacts'].filter((file) => existsSync(join(workspace.root, file)))).toStrictEqual(['.artifacts']);
    });

    it('removes each match of a workspace pattern, and keeps a same-named folder outside the pattern and one holding a file the workspace tracks', async ({ workspace }) => {
        const scattered = { 'lib/pkg/__pycache__/a.pyc': '', 'lib/pkg/deep/__pycache__/b.pyc': '', 'other/__pycache__/c.pyc': '', 'lib/kept/__pycache__/tracked.py': 'source' };
        const graph = await workspace.write({ ...lib, ...scattered, '.gitignore': `${lib['.gitignore']}*.pyc\n` }).graph({});

        await execute({ targets: [], commands: [], cwd: '.', folders: [], patterns: ['lib/**/__pycache__'] }, workspace.context(graph, 'lib', 'clean'));

        expect(['lib/pkg/__pycache__', 'lib/pkg/deep/__pycache__', 'other/__pycache__', 'lib/kept/__pycache__/tracked.py'].filter((file) => existsSync(join(workspace.root, file)))).toStrictEqual(['other/__pycache__', 'lib/kept/__pycache__/tracked.py']);
    });

    it("runs the tool's own clean commands in the folder cwd names", async ({ workspace }) => {
        const graph = await workspace.write(lib).graph({});

        await execute({ targets: [], commands: [[execPath, join(workspace.root, 'tools/record.mjs'), 'cleaned.txt', 'swept']], cwd: 'lib', folders: [] }, workspace.context(graph, 'lib', 'clean'));

        expect(readFileSync(join(workspace.root, 'lib/cleaned.txt'), 'utf-8')).toBe('swept');
    });
});
