import { type CreateDependenciesContext, DependencyType } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-cargo';
import { TempFs } from '@rasm/testing/temp-fs';
import { Record } from 'effect';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const cwd = process.cwd();
        const tempFs = new TempFs('cargo-dependencies');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(cwd);
            tempFs.cleanup();
        });
        return tempFs;
    })
    .extend('contextWith', ({ tempFs }) => async (manifests: readonly string[], projects: Record.ReadonlyRecord<string, string>): Promise<CreateDependenciesContext> => {
        await createNodes[1](manifests, undefined, { nxJsonConfiguration: {}, workspaceRoot: tempFs.tempDir });
        return {
            workspaceRoot: tempFs.tempDir,
            projects: Record.map(projects, (root, name) => ({ name, root })),
            externalNodes: {},
            fileMap: { projectFileMap: {}, nonProjectFiles: [] },
            filesToProcess: { projectFileMap: {}, nonProjectFiles: [] },
            nxJsonConfiguration: {},
        };
    });

describe('@rasm/nx-cargo/dependencies', { timeout: 60_000 }, () => {
    it('records an edge as static when the source project owns the manifest', async ({ tempFs, contextWith }) => {
        await tempFs.createFiles({
            'Cargo.toml': '[package]\nname = "root"\nedition = "2024"\n\n[workspace]\nmembers = ["clients"]\n\n[dependencies]\nclients = { path = "clients" }\n',
            'src/lib.rs': '',
            'clients/Cargo.toml': '[package]\nname = "clients"\nedition = "2024"\n',
            'clients/src/lib.rs': '',
        });

        const result = await createDependencies({}, await contextWith(['Cargo.toml', 'clients/Cargo.toml'], { root: '.', clients: 'clients' }));

        expect(result).toStrictEqual([
            {
                source: 'root',
                target: 'clients',
                type: DependencyType.static,
                sourceFile: 'Cargo.toml',
            },
        ]);
    });

    it('records a nested project that owns its manifest as static', async ({ tempFs, contextWith }) => {
        await tempFs.createFiles({
            'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["api-checker/core", "clients"]\n',
            'api-checker/core/Cargo.toml': '[package]\nname = "api-checker-core"\nedition = "2024"\n\n[dependencies]\nclients = { path = "../../clients" }\n',
            'api-checker/core/src/lib.rs': '',
            'clients/Cargo.toml': '[package]\nname = "clients"\nedition = "2024"\n',
            'clients/src/lib.rs': '',
        });

        const result = await createDependencies({}, await contextWith(['Cargo.toml', 'api-checker/core/Cargo.toml', 'clients/Cargo.toml'], { api_checker_core: 'api-checker/core', clients: 'clients' }));

        expect(result).toStrictEqual([
            {
                source: 'api_checker_core',
                target: 'clients',
                type: DependencyType.static,
                sourceFile: 'api-checker/core/Cargo.toml',
            },
        ]);
    });
});
