import { type CreateNodesContext, DependencyType } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-cargo';
import { TempFs } from '@rasm/testing/temp-fs';
import { Predicate, Record } from 'effect';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('node:path'), async (actual) => {
    const { windows } = await import('@rasm/testing/path');
    return windows(await actual());
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', async ({}, { onCleanup }) => {
        const cwd = process.cwd();
        const tempFs = new TempFs('cargo-windows-paths');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(cwd);
            tempFs.cleanup();
        });
        await tempFs.createFiles({
            'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/core", "crates/cli"]\n',
            'crates/core/Cargo.toml': '[package]\nname = "core-kit"\nversion = "0.1.0"\nedition = "2024"\n',
            'crates/core/src/lib.rs': '',
            'crates/cli/Cargo.toml': '[package]\nname = "cli"\nversion = "0.1.0"\nedition = "2024"\n\n[dependencies]\ncore-kit = { path = "../core" }\n',
            'crates/cli/src/main.rs': 'fn main() {}\n',
        });
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: {}, workspaceRoot: tempFs.tempDir }));

describe('@rasm/nx-cargo under Windows path separators', { timeout: 60_000 }, () => {
    const manifests = ['Cargo.toml', 'crates/core/Cargo.toml', 'crates/cli/Cargo.toml'];

    it('should hand Nx workspace-relative posix roots, inputs, outputs, and lock paths', async ({ context }) => {
        const results = await createNodes[1](manifests, {}, context);

        const projects = results[0]?.[1].projects;
        expect(Object.keys({ ...projects })).toStrictEqual(['crates/core', 'crates/cli']);
        expect(projects?.['crates/core']?.namedInputs?.['default']?.filter(Predicate.isString)).toStrictEqual(['{projectRoot}/Cargo.toml', '{projectRoot}/src/**/*', '{workspaceRoot}/Cargo.lock', '{workspaceRoot}/Cargo.toml']);
        expect(projects?.['crates/cli']?.targets?.['build--cargo']).toMatchObject({ options: { outputs: ['{workspaceRoot}/target/debug/cli', '{workspaceRoot}/target/debug/cli.d'] } });
        expect(projects?.['crates/core']?.release?.version?.versionActionsOptions).toStrictEqual({ lock: 'Cargo.lock' });
    });

    it('should name the workspace-relative posix manifest as the source file of an edge', async ({ tempFs, context }) => {
        const nodes = await createNodes[1](manifests, {}, context);

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

        expect(result).toStrictEqual([{ source: 'cli', target: 'core_kit', type: DependencyType.static, sourceFile: 'crates/cli/Cargo.toml' }]);
    });
});
