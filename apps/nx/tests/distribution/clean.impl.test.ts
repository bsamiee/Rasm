import { existsSync } from 'node:fs';
import { join } from 'node:path';
import type { ExecutorContext } from '@nx/devkit';
import { execute } from '@rasm/nx-distribution/executors/clean';
import { TempFs } from '@rasm/testing/temp-fs';
import { describe, expect, test, vi } from 'vitest';

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('log', { auto: true }, ({}, { onCleanup }) => {
        const log = vi.spyOn(console, 'log').mockReturnValue(undefined);
        onCleanup(() => log.mockRestore());
        return log;
    })
    .extend('tempFs', ({}, { onCleanup }) => {
        const tempFs = new TempFs('clean');
        onCleanup(() => tempFs.cleanup());
        tempFs.createFilesSync({
            '.gitignore': 'packed\nbuilt.txt\n.artifacts\ncleaned.txt\n',
            'tools/record.js': "import { writeFileSync } from 'node:fs';\n\nconst [file, ...argv] = process.argv.slice(2);\nwriteFileSync(file, argv.join('\\n'));\n",
            'lib/index.js': 'export const lib = 1;\n',
            'lib/packed/lib.tgz': 'archive',
            'lib/staged/built.txt': 'built',
            'lib/staged/tracked.txt': 'source',
            '.artifacts/distribution/lib/lib.zip': 'archive',
        });
        return tempFs;
    })
    .extend(
        'present',
        ({ tempFs }) =>
            (...files: readonly string[]) =>
                files.filter((file) => existsSync(join(tempFs.tempDir, file))),
    )
    .extend('context', ({ tempFs }): ExecutorContext => ({
        root: tempFs.tempDir,
        cwd: tempFs.tempDir,
        isVerbose: false,
        projectName: 'lib',
        targetName: 'clean',
        nxJsonConfiguration: {},
        projectsConfigurations: { version: 2, projects: { lib: { root: 'lib' } } },
        projectGraph: {
            nodes: {
                lib: {
                    name: 'lib',
                    type: 'lib',
                    data: {
                        root: 'lib',
                        targets: {
                            pack: { outputs: ['{projectRoot}/packed/lib.tgz'] },
                            stage: { outputs: ['{projectRoot}/staged'] },
                            archive: { outputs: ['{options.outputPath}'], options: { outputPath: '.artifacts/distribution/lib' }, configurations: { release: { outputPath: '.artifacts/release/lib' } } },
                        },
                    },
                },
            },
            dependencies: {},
        },
    }));

describe('clean executor', () => {
    it("removes each declared output, keeps each file the workspace tracks under one, and runs the tool's own clean command", async ({ tempFs, present, context }) => {
        await expect(execute({ targets: ['pack', 'stage'], commands: [['node', join(tempFs.tempDir, 'tools/record.js'), '../cleaned.txt', 'swept']], cwd: 'lib' }, context)).resolves.toStrictEqual({ success: true });

        expect(present('lib/packed/lib.tgz', 'lib/staged/built.txt', 'lib/staged/tracked.txt', 'lib/index.js')).toStrictEqual(['lib/staged/tracked.txt', 'lib/index.js']);
        await expect(tempFs.readFile('cleaned.txt')).resolves.toBe('swept');
    });

    it('removes a declared output folder under a dot folder whole, with the outputs of every configuration', async ({ tempFs, present, context }) => {
        await tempFs.createFile('.artifacts/release/lib/lib.zip', 'archive');

        await expect(execute({ targets: ['archive'], commands: [], cwd: '.' }, context)).resolves.toStrictEqual({ success: true });

        expect(present('.artifacts/distribution/lib', '.artifacts/release/lib', '.artifacts')).toStrictEqual(['.artifacts']);
    });

    it('removes nothing and runs nothing under the defaults its schema states', async ({ present, context }) => {
        await expect(execute({ targets: [], commands: [], cwd: '.' }, context)).resolves.toStrictEqual({ success: true });

        expect(present('lib/packed/lib.tgz', 'lib/staged/built.txt', '.artifacts/distribution/lib/lib.zip', 'cleaned.txt')).toStrictEqual(['lib/packed/lib.tgz', 'lib/staged/built.txt', '.artifacts/distribution/lib/lib.zip']);
    });
});
