import { execFileSync } from 'node:child_process';
import { chmodSync } from 'node:fs';
import { join } from 'node:path';
import type { ProjectGraph, readCachedProjectGraph, Tree } from '@nx/devkit';
import { FsTree } from '@nx/devkit/internal';
import { createTreeWithEmptyWorkspace } from '@nx/devkit/testing';
import CargoVersionActions, { afterAllProjectsVersioned } from '@rasm/nx-cargo/release/version-actions';
import { TempFs } from '@rasm/testing/temp-fs';
import { ChildProcess } from 'effect/process';
import { createNxReleaseConfig, IMPLICIT_DEFAULT_RELEASE_GROUP } from 'nx/src/command-line/release/config/config';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit'), async (actual) => ({
    ...(await actual()),
    readCachedProjectGraph: vi.fn<typeof readCachedProjectGraph>(() => ({
        nodes: { 'my-lib': { name: 'my-lib', type: 'lib', data: { root: 'packages/my-lib', release: { version: { versionActionsOptions: { lock: 'Cargo.lock' } } } } } },
        dependencies: {},
    })),
}));
vi.mock(import('effect/process'), { spy: true });

// --- [OPERATIONS] ----------------------------------------------------------------------

const createProjectGraph = (): ProjectGraph => ({
    nodes: {
        dependency: {
            name: 'dependency',
            type: 'lib',
            data: { root: 'packages/dependency' },
        },
    },
    dependencies: {},
});

const createVersionActions = async (tree: Tree, versionActionsOptions: Readonly<Record<string, unknown>> = {}) => {
    const { nxReleaseConfig } = await createNxReleaseConfig(createProjectGraph(), {}, { projects: ['my-lib'], projectsRelationship: 'independent' });
    const group = nxReleaseConfig?.groups?.[IMPLICIT_DEFAULT_RELEASE_GROUP];
    const versionActions = new CargoVersionActions(
        { ...group, projects: group?.projects ?? [], name: IMPLICIT_DEFAULT_RELEASE_GROUP, resolvedVersionPlans: false },
        { name: 'my-lib', type: 'lib', data: { root: 'packages/my-lib' } },
        {
            specifierSource: 'prompt',
            currentVersionResolver: 'disk',
            currentVersionResolverMetadata: {},
            fallbackCurrentVersionResolver: 'disk',
            versionPrefix: 'auto',
            preserveLocalDependencyProtocols: false,
            preserveMatchingDependencyRanges: false,
            adjustSemverBumpsForZeroMajorVersion: false,
            applyPreidToDependents: false,
            versionActionsOptions: { lock: 'packages/my-lib/Cargo.lock', ...versionActionsOptions },
            manifestRootsToUpdate: [],
            dockerOptions: {},
        },
    );
    await versionActions.init(tree);
    return versionActions;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const tempFs = new TempFs('cargo-version-actions');
        onCleanup(() => {
            vi.unstubAllEnvs();
            tempFs.cleanup();
        });
        return tempFs;
    })
    .extend('spawn', () => vi.mocked(ChildProcess.make).mockClear());

describe('readCurrentVersionFromRegistry', () => {
    it.for([
        { name: 'passes an injected registry to cargo search as single argv elements', options: { registry: 'r"; touch NX_PWNED; "' }, search: ['search', 'my-lib', '--limit', '1', '--color', 'never', '--registry', 'r"; touch NX_PWNED; "'] },
        { name: 'omits the registry flag when no registry was resolved', options: {}, search: ['search', 'my-lib', '--limit', '1', '--color', 'never'] },
    ])('$name', async ({ options, search }, { tempFs, spawn }) => {
        await tempFs.createFiles({ 'packages/my-lib/Cargo.toml': '[package]\nname = "my-lib"\nversion = "1.0.0"\n', 'bin/cargo': '#!/bin/sh\necho \'my-lib = "1.2.3"    # A library\'\n' });
        chmodSync(join(tempFs.tempDir, 'bin/cargo'), 0o755);
        vi.stubEnv('PATH', join(tempFs.tempDir, 'bin'));
        const tree = new FsTree(tempFs.tempDir, false);
        const versionActions = await createVersionActions(tree, options);

        const { currentVersion, logText } = await versionActions.readCurrentVersionFromRegistry(tree);

        expect(currentVersion).toBe('1.2.3');
        expect(logText).toBe(`cargo ${search.join(' ')}`);
        expect(spawn).toHaveBeenCalledWith('cargo', search, expect.objectContaining({ cwd: tempFs.tempDir }));
    });
});

describe('CargoVersionActions', () => {
    it('preserves Cargo.toml formatting when updating versions and dependencies', async () => {
        const tree = createTreeWithEmptyWorkspace();
        tree.write('packages/dependency/Cargo.toml', '[package]\nname = "dependency"\nversion = "1.0.0"\n');
        tree.write(
            'packages/my-lib/Cargo.toml',
            `# The library
[package]
name    = "my-lib"
version = "1.0.0" # released with the rest
edition = "2024"

[dependencies]
dependency = { path = "../dependency", version = "1.0.0" } # path and registry
renamed    = { package = "dependency", path = "../dependency", version = "1.0.0" }
serde      = "1"

[dependencies.tabled]
package = "dependency"
path    = "../dependency"
version = "1.0.0"

[target.'cfg(unix)'.dev-dependencies]
dependency = { path = "../dependency", version = "1.0.0" }
`,
        );
        const versionActions = await createVersionActions(tree);

        await versionActions.updateProjectVersion(tree, '1.1.0');
        await versionActions.updateProjectDependencies(tree, createProjectGraph(), {
            dependency: '2.0.0',
        });

        expect(tree.read('packages/my-lib/Cargo.toml', 'utf-8')).toBe(`# The library
[package]
name    = "my-lib"
version = "1.1.0" # released with the rest
edition = "2024"

[dependencies]
dependency = { path = "../dependency", version = "2.0.0" } # path and registry
renamed    = { package = "dependency", path = "../dependency", version = "2.0.0" }
serde      = "1"

[dependencies.tabled]
package = "dependency"
path    = "../dependency"
version = "2.0.0"

[target.'cfg(unix)'.dev-dependencies]
dependency = { path = "../dependency", version = "2.0.0" }
`);
    });

    it.each([
        {
            name: 'spaces and LF endings',
            input: '[package]\nname = "my-lib"\nedition = "2024"\n',
            expected: '[package]\nname = "my-lib"\nedition = "2024"\nversion = "1.1.0"\n',
        },
        {
            name: 'CRLF endings',
            input: '[package]\r\nname = "my-lib"\r\nedition = "2024"\r\n',
            expected: '[package]\r\nname = "my-lib"\r\nedition = "2024"\r\nversion = "1.1.0"\r\n',
        },
    ])('formats an inserted version using $name', async ({ input, expected }) => {
        const tree = createTreeWithEmptyWorkspace();
        tree.write('packages/my-lib/Cargo.toml', input);
        const versionActions = await createVersionActions(tree);

        await versionActions.updateProjectVersion(tree, '1.1.0');

        expect(tree.read('packages/my-lib/Cargo.toml', 'utf-8')).toBe(expected);
    });

    it('rejects a malformed manifest without changing it', async () => {
        const tree = createTreeWithEmptyWorkspace();
        const malformedManifest = '[package\nname = "my-lib"\nversion = "1.0.0"\n';
        tree.write('packages/my-lib/Cargo.toml', malformedManifest);
        const versionActions = await createVersionActions(tree);

        await expect(versionActions.updateProjectVersion(tree, '1.1.0')).rejects.toThrow('packages/my-lib/Cargo.toml does not read as TOML');
        expect(tree.read('packages/my-lib/Cargo.toml', 'utf-8')).toBe(malformedManifest);
    });

    it('preserves comments and trailing commas in a valid manifest', async () => {
        const tree = createTreeWithEmptyWorkspace();
        tree.write(
            'packages/my-lib/Cargo.toml',
            `[package]
# This comment should be preserved.
name = "my-lib"
version = "1.0.0"
authors = [
    "Someone",
]
`,
        );
        const versionActions = await createVersionActions(tree);

        await versionActions.updateProjectVersion(tree, '1.1.0');

        expect(tree.read('packages/my-lib/Cargo.toml', 'utf-8')).toBe(`[package]
# This comment should be preserved.
name = "my-lib"
version = "1.1.0"
authors = [
    "Someone",
]
`);
    });

    it('rejects a version update shadowed by a duplicate key', async () => {
        const tree = createTreeWithEmptyWorkspace();
        const manifest = '[package]\nname = "my-lib"\nversion = "1.0.0"\nversion = "5.5.5"\n';
        tree.write('packages/my-lib/Cargo.toml', manifest);
        const versionActions = await createVersionActions(tree);

        await expect(versionActions.updateProjectVersion(tree, '1.1.0')).rejects.toThrow('packages/my-lib/Cargo.toml does not read as TOML');
        expect(tree.read('packages/my-lib/Cargo.toml', 'utf-8')).toBe(manifest);
    });

    it('rejects a dependency update shadowed by a duplicate key', async () => {
        const tree = createTreeWithEmptyWorkspace();
        tree.write('packages/dependency/Cargo.toml', '[package]\nname = "dependency"\nversion = "1.0.0"\n');
        const manifest = '[package]\nname = "my-lib"\nversion = "1.0.0"\n\n[dependencies]\ndependency = "1.0.0"\n\n[dependencies]\ndependency = "5.0.0"\n';
        tree.write('packages/my-lib/Cargo.toml', manifest);
        const versionActions = await createVersionActions(tree);

        await expect(
            versionActions.updateProjectDependencies(tree, createProjectGraph(), {
                dependency: '2.0.0',
            }),
        ).rejects.toThrow('packages/my-lib/Cargo.toml does not read as TOML');
        expect(tree.read('packages/my-lib/Cargo.toml', 'utf-8')).toBe(manifest);
    });

    it('moves the workspace dependency row of a crate with its version and leaves an inherited requirement to it', async () => {
        const tree = createTreeWithEmptyWorkspace();
        tree.write('Cargo.toml', '[workspace]\nmembers = ["packages/*"]\n\n[workspace.dependencies]\nmy-lib = { path = "packages/my-lib", version = "1.0.0" }\n');
        tree.write('packages/my-lib/Cargo.toml', '[package]\nname = "my-lib"\nversion = "1.0.0"\n');
        const inheriting = '[package]\nname = "dependency"\nversion = "1.0.0"\n\n[dependencies]\nmy-lib.workspace = true\n';
        tree.write('packages/dependency/Cargo.toml', inheriting);
        const versionActions = await createVersionActions(tree, { lock: 'Cargo.lock' });

        await versionActions.updateProjectVersion(tree, '1.1.0');

        expect(tree.read('Cargo.toml', 'utf-8')).toBe('[workspace]\nmembers = ["packages/*"]\n\n[workspace.dependencies]\nmy-lib = { path = "packages/my-lib", version = "1.1.0" }\n');
        expect(tree.read('packages/dependency/Cargo.toml', 'utf-8')).toBe(inheriting);
    });

    it('leaves plain registry ranges unchanged', async () => {
        const tree = createTreeWithEmptyWorkspace();
        const manifest = '[package]\nname = "my-lib"\nversion = "1.0.0"\n\n[dependencies]\nserde = "2.0.0"\n';
        tree.write('packages/my-lib/Cargo.toml', manifest);
        const versionActions = await createVersionActions(tree);

        await versionActions.updateProjectDependencies(tree, createProjectGraph(), {});

        expect(tree.read('packages/my-lib/Cargo.toml', 'utf-8')).toBe(manifest);
    });
});

describe('afterAllProjectsVersioned', { timeout: 60_000 }, () => {
    it.for([
        { name: 'brings the lock in line with the versioned manifests', options: {}, changedFiles: ['Cargo.lock'], locked: 'name = "my-lib"\nversion = "1.1.0"' },
        { name: 'leaves the lock when skipLockFileUpdate is set', options: { skipLockFileUpdate: true }, changedFiles: [], locked: 'name = "my-lib"\nversion = "1.0.0"' },
    ])('$name', async ({ options, changedFiles, locked }, { tempFs }) => {
        await tempFs.createFiles({
            'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["packages/*"]\n',
            'packages/my-lib/Cargo.toml': '[package]\nname = "my-lib"\nversion = "1.0.0"\nedition = "2024"\n',
            'packages/my-lib/src/lib.rs': '',
        });
        execFileSync('cargo', ['generate-lockfile'], { cwd: tempFs.tempDir });
        tempFs.writeFile('packages/my-lib/Cargo.toml', '[package]\nname = "my-lib"\nversion = "1.1.0"\nedition = "2024"\n');

        const result = await afterAllProjectsVersioned(tempFs.tempDir, { dryRun: false, verbose: false, rootVersionActionsOptions: options });

        expect(result).toStrictEqual({ changedFiles, deletedFiles: [] });
        await expect(tempFs.readFile('Cargo.lock')).resolves.toContain(locked);
    });
});
