import { execFileSync } from 'node:child_process';
import { type CreateDependenciesContext, DependencyType } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-uv';
import { TempFs } from '@rasm/testing/temp-fs';
import { Record } from 'effect';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const previous = process.cwd();
        const tempFs = new TempFs('uv-dependencies');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(previous);
            tempFs.cleanup();
        });
        return tempFs;
    })
    .extend('locked', ({ tempFs }) => async (files: Readonly<Record<string, string>>) => {
        await tempFs.createFiles(files);
        execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: tempFs.tempDir });
        await createNodes[1](['uv.lock', ...Object.keys(files)], undefined, { workspaceRoot: tempFs.tempDir, nxJsonConfiguration: {} });
    })
    .extend('contextWith', ({ tempFs }) => (projects: Readonly<Record<string, string>>): CreateDependenciesContext => ({
        workspaceRoot: tempFs.tempDir,
        projects: Record.map(projects, (root, name) => ({ name, root })),
        externalNodes: {},
        fileMap: { projectFileMap: {}, nonProjectFiles: [] },
        filesToProcess: { projectFileMap: {}, nonProjectFiles: [] },
        nxJsonConfiguration: {},
    }));

describe('@rasm/nx-uv/dependencies', { timeout: 60_000 }, () => {
    it('records an edge as static when the source project owns the manifest', async ({ locked, contextWith }) => {
        await locked({
            'pyproject.toml': '[project]\nname = "root"\nversion = "0.1.0"\ndependencies = ["clients"]\n\n[tool.uv.sources]\nclients = { workspace = true }\n\n[tool.uv.workspace]\nmembers = ["clients"]\n',
            'clients/pyproject.toml': '[project]\nname = "clients"\nversion = "0.1.0"\n',
        });

        const result = await createDependencies({}, contextWith({ root: '.', clients: 'clients' }));

        expect(result).toStrictEqual([
            expect.objectContaining({
                source: 'root',
                target: 'clients',
                type: DependencyType.static,
                sourceFile: 'pyproject.toml',
            }),
        ]);
    });

    it('records a nested project that owns its manifest as static', async ({ locked, contextWith }) => {
        await locked({
            'pyproject.toml': '[tool.uv.workspace]\nmembers = ["api-checker/core", "clients"]\n',
            'api-checker/core/pyproject.toml': '[project]\nname = "api-checker-core"\nversion = "0.1.0"\ndependencies = ["clients"]\n\n[tool.uv.sources]\nclients = { workspace = true }\n',
            'clients/pyproject.toml': '[project]\nname = "clients"\nversion = "0.1.0"\n',
        });

        const result = await createDependencies({}, contextWith({ 'api-checker-core': 'api-checker/core', clients: 'clients' }));

        expect(result[0]).toStrictEqual(expect.objectContaining({ type: DependencyType.static, sourceFile: 'api-checker/core/pyproject.toml' }));
    });

    it('records each dependency uv resolves for a member, and none it reaches through another member', async ({ locked, contextWith }) => {
        await locked({
            'pyproject.toml': '[tool.uv.workspace]\nmembers = ["app", "middle", "core"]\n',
            'app/pyproject.toml': '[project]\nname = "app"\nversion = "0.1.0"\ndependencies = ["middle"]\n\n[tool.uv.sources]\nmiddle = { workspace = true }\n',
            'middle/pyproject.toml': '[project]\nname = "middle"\nversion = "0.1.0"\ndependencies = ["core"]\n\n[tool.uv.sources]\ncore = { workspace = true }\n',
            'core/pyproject.toml': '[project]\nname = "core"\nversion = "0.1.0"\n',
        });

        const result = await createDependencies({}, contextWith({ app: 'app', middle: 'middle', core: 'core' }));

        expect(result).toHaveLength(2);
        expect(result).toStrictEqual(
            expect.arrayContaining([
                { source: 'app', target: 'middle', type: DependencyType.static, sourceFile: 'app/pyproject.toml' },
                { source: 'middle', target: 'core', type: DependencyType.static, sourceFile: 'middle/pyproject.toml' },
            ]),
        );
    });
});
