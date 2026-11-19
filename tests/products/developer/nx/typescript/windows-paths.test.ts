import { symlinkSync } from 'node:fs';
import { dirname, join, matchesGlob } from 'node:path';
import { fileURLToPath } from 'node:url';
import type { CreateNodesContext } from '@nx/devkit';
import { setupWorkspaceContext } from '@nx/devkit/internal';
import { createNodes } from '@rasm/nx-typescript';
import { TempFs } from '@rasm/testing/temp-fs';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('node:path'), async (actual) => {
    const { windows } = await import('@rasm/testing/path');
    return windows(await actual());
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const files = {
    'package-lock.json': '',
    'package.json': JSON.stringify({ workspaces: ['libs/*'] }),
    'tsconfig.base.json': '{"compilerOptions":{}}',
    'libs/my-lib/package.json': JSON.stringify({ name: 'my-lib', version: '1.0.0' }),
    'libs/my-lib/tsconfig.json': JSON.stringify({ extends: '../../tsconfig.base.json', files: ['index.ts'] }),
    'libs/my-lib/index.ts': 'export {};',
    'node_modules/knip/package.json': JSON.stringify({ name: 'knip', type: 'module', exports: './index.js' }),
    'node_modules/knip/index.js': 'export {};',
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', async ({}, { onCleanup }) => {
        const cwd = process.cwd();
        const tempFs = new TempFs('typescript-windows-paths');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(cwd);
            tempFs.cleanup();
        });
        await tempFs.createFiles(files);
        symlinkSync(dirname(fileURLToPath(import.meta.resolve('@typescript/native/package.json'))), join(tempFs.tempDir, 'node_modules/typescript'), 'dir');
        setupWorkspaceContext(tempFs.tempDir);
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: { namedInputs: { default: ['{projectRoot}/**/*'] } }, workspaceRoot: tempFs.tempDir }));

describe('@rasm/nx-typescript under Windows path separators', { timeout: 60_000 }, () => {
    it('should hand Nx posix project roots, manifest and tsconfig inputs, and tarball paths', async ({ context }) => {
        const results = await createNodes[1](
            Object.keys(files).filter((file) => matchesGlob(file, createNodes[0])),
            { outputPath: '.artifacts/npm/{projectName}' },
            context,
        );

        const projects = Object.groupBy(
            results.flatMap(([, nodes]) => Object.entries({ ...nodes.projects })),
            ([root]) => root,
        );
        const targets = Object.fromEntries((projects['libs/my-lib'] ?? []).flatMap(([, project]) => Object.entries({ ...project.targets })));
        expect(Object.keys(projects).toSorted()).toStrictEqual(['.', 'libs/my-lib']);
        expect(targets['build--tsc']?.inputs).toStrictEqual(expect.arrayContaining(['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{workspaceRoot}/tsconfig.base.json', '{projectRoot}/index.ts']));
        expect(targets['pack--npm']).toMatchObject({
            command: "npm pack --pack-destination '../../.artifacts/npm/{projectName}'",
            inputs: ['default', '^{projectRoot}/package.json', { json: '{workspaceRoot}/package.json', fields: ['workspaces'] }, { dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
            outputs: ['{workspaceRoot}/.artifacts/npm/{projectName}/*.tgz'],
        });
        expect(projects['.']?.[0]?.[1].targets?.['check--knip']?.inputs).toStrictEqual(['{workspaceRoot}/**/*', { externalDependencies: ['knip'] }]);
    });
});
