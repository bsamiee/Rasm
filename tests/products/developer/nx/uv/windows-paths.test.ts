import { execFileSync } from 'node:child_process';
import { type CreateNodesContext, DependencyType } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-uv';
import { TempFs } from '@rasm/testing/temp-fs';
import { Record } from 'effect';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('node:path'), async (actual) => {
    const { windows } = await import('@rasm/testing/path');
    return windows(await actual());
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const files = {
    'pyproject.toml': '[tool.uv.workspace]\nmembers = ["libs/python/app", "libs/python/core"]\n',
    'libs/python/app/pyproject.toml': '[project]\nname = "app"\nversion = "0.1.0"\ndependencies = ["core"]\n\n[tool.uv.sources]\ncore = { workspace = true }\n',
    'libs/python/core/pyproject.toml': '[project]\nname = "core"\nversion = "0.1.0"\n',
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', async ({}, { onCleanup }) => {
        const previous = process.cwd();
        const tempFs = new TempFs('uv-windows-paths');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(previous);
            tempFs.cleanup();
        });
        await tempFs.createFiles(files);
        execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: tempFs.tempDir });
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: {}, workspaceRoot: tempFs.tempDir }));

describe('@rasm/nx-uv under Windows path separators', { timeout: 60_000 }, () => {
    it('should hand Nx workspace-relative posix roots and lock paths', async ({ context }) => {
        const results = await createNodes[1](['uv.lock', ...Object.keys(files)], {}, context);

        const projects = results[0]?.[1].projects;
        expect(Object.keys({ ...projects }).toSorted()).toStrictEqual(['libs/python/app', 'libs/python/core']);
        expect(projects?.['libs/python/core']?.targets?.['clean']?.inputs).toContain('{workspaceRoot}/uv.lock');
    });

    it('should name the workspace-relative posix manifest as the source file of an edge', async ({ tempFs, context }) => {
        const nodes = await createNodes[1](['uv.lock', ...Object.keys(files)], {}, context);

        const result = await createDependencies(
            {},
            {
                workspaceRoot: tempFs.tempDir,
                projects: Record.fromEntries(Record.collect({ ...nodes[0]?.[1].projects }, (root, project) => [project.name ?? root, { ...project, root }] as const)),
                externalNodes: {},
                fileMap: { projectFileMap: {}, nonProjectFiles: [] },
                filesToProcess: { projectFileMap: {}, nonProjectFiles: [] },
                nxJsonConfiguration: {},
            },
        );

        expect(result).toStrictEqual([{ source: 'app', target: 'core', type: DependencyType.static, sourceFile: 'libs/python/app/pyproject.toml' }]);
    });
});
