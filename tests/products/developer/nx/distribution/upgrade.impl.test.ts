import { join } from 'node:path';
import { env } from 'node:process';
import type { ExecutorContext } from '@nx/devkit';
import { execute } from '@rasm/nx-distribution/executors/upgrade';
import { TempFs } from '@rasm/testing/temp-fs';
import { describe, expect, test } from 'vitest';

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const tempFs = new TempFs('upgrade');
        onCleanup(() => tempFs.cleanup());
        tempFs.createFilesSync({ 'tools/record.js': "import { writeFileSync } from 'node:fs';\n\nconst [file, name] = process.argv.slice(2);\nwriteFileSync(file, process.env[name] ?? 'unset');\n" });
        return tempFs;
    })
    .extend(
        'record',
        ({ tempFs }) =>
            (file: string) =>
                ['node', join(tempFs.tempDir, 'tools/record.js'), file, 'UPGRADE_DUMPS'] as const,
    )
    .extend('context', ({ tempFs }): ExecutorContext => ({
        root: tempFs.tempDir,
        cwd: tempFs.tempDir,
        isVerbose: false,
        projectName: 'lib',
        targetName: 'upgrade--tool',
        nxJsonConfiguration: {},
        projectsConfigurations: { version: 2, projects: { lib: { root: 'lib' } } },
        projectGraph: { nodes: {}, dependencies: {} },
    }));

describe('upgrade executor', () => {
    it('runs a command with its env row, Nx tokens resolved over the executor context, and leaves the next command and the process without it', async ({ tempFs, record, context }) => {
        await expect(execute({ commands: [{ apply: record('moved.txt'), env: { UPGRADE_DUMPS: '{root}/dumps/{projectName}-%p.dmp' } }, { apply: record('locked.txt') }], cwd: '.' }, context)).resolves.toStrictEqual({ success: true });

        await expect(tempFs.readFile('moved.txt')).resolves.toBe(`${tempFs.tempDir}/dumps/lib-%p.dmp`);
        await expect(tempFs.readFile('locked.txt')).resolves.toBe('unset');
        expect(env['UPGRADE_DUMPS']).toBeUndefined();
    });

    it("runs a command's preview with its env row under a dry run", async ({ tempFs, record, context }) => {
        await expect(execute({ dryRun: true, commands: [{ apply: record('moved.txt'), preview: record('previewed.txt'), env: { UPGRADE_DUMPS: '{root}/dumps' } }], cwd: '.' }, context)).resolves.toStrictEqual({ success: true });

        await expect(tempFs.readFile('previewed.txt')).resolves.toBe(`${tempFs.tempDir}/dumps`);
    });
});
