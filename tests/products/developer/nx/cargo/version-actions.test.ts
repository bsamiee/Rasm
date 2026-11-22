import { execFileSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import type { ProjectGraph, readCachedProjectGraph, Tree } from '@nx/devkit';
import { FsTree } from '@nx/devkit/internal';
import { createTreeWithEmptyWorkspace } from '@nx/devkit/testing';
import CargoVersionActions, { afterAllProjectsVersioned } from '@rasm/nx-cargo/release/version-actions';
import { it, pathed } from '@rasm/testing/nx';
import { assert, asyncProperty, nat, option, stringMatching, tuple } from 'fast-check';
import type { ReleaseGroupWithName } from 'nx/src/command-line/release/config/filter-release-groups';
import type { FinalConfigForProject } from 'nx/src/command-line/release/utils/release-graph';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit'), async (actual) => ({
    ...(await actual()),
    readCachedProjectGraph: vi.fn<typeof readCachedProjectGraph>(() => ({
        nodes: { 'my-lib': { name: 'my-lib', type: 'lib', data: { root: 'packages/my-lib', release: { version: { versionActionsOptions: { lock: 'Cargo.lock' } } } } } },
        dependencies: {},
    })),
}));

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'packages/my-lib/Cargo.toml';
const graph: ProjectGraph = { nodes: { dependency: { name: 'dependency', type: 'lib', data: { root: 'packages/dependency' } } }, dependencies: {} };
const independent: ReleaseGroupWithName = { name: 'crates', projects: ['my-lib'], projectsRelationship: 'independent', resolvedVersionPlans: false };

// --- [OPERATIONS] ----------------------------------------------------------------------

const actioned = async (tree: Tree, versionActionsOptions: Readonly<Record<string, unknown>>) => {
    const settled: FinalConfigForProject = {
        specifierSource: 'prompt',
        currentVersionResolver: 'disk',
        currentVersionResolverMetadata: {},
        fallbackCurrentVersionResolver: 'disk',
        versionPrefix: 'auto',
        preserveLocalDependencyProtocols: false,
        preserveMatchingDependencyRanges: false,
        adjustSemverBumpsForZeroMajorVersion: false,
        applyPreidToDependents: false,
        versionActionsOptions,
        manifestRootsToUpdate: [],
        dockerOptions: {},
    };
    const actions = new CargoVersionActions(independent, { name: 'my-lib', type: 'lib', data: { root: 'packages/my-lib' } }, settled);
    await actions.init(tree);
    return actions;
};

const written = (files: Readonly<Record<string, string>>) => {
    const tree = createTreeWithEmptyWorkspace();
    Object.entries(files).forEach(([file, text]) => tree.write(file, text));
    return tree;
};

describe('manifest edits', () => {
    it('moves the package version and every requirement on a released crate, in each table form, keeping the rest of the text', async () => {
        const tree = written({
            'packages/dependency/Cargo.toml': '[package]\nname = "dependency"\nversion = "1.0.0"\n',
            [MANIFEST]:
                '# The library\n[package]\nname    = "my-lib"\nversion = "1.0.0" # released with the rest\nedition = "2024"\n\n[dependencies]\ndependency = { path = "../dependency", version = "1.0.0" } # path and registry\nrenamed    = { package = "dependency", path = "../dependency", version = "1.0.0" }\nserde      = "1"\n\n[dependencies.tabled]\npackage = "dependency"\npath    = "../dependency"\nversion = "1.0.0"\n\n[target.\'cfg(unix)\'.dev-dependencies]\ndependency = { path = "../dependency", version = "1.0.0" }\n',
        });
        const actions = await actioned(tree, { lock: 'packages/my-lib/Cargo.lock' });

        await actions.updateProjectVersion(tree, '1.1.0');
        await actions.updateProjectDependencies(tree, graph, { dependency: '2.0.0' });

        expect(tree.read(MANIFEST, 'utf-8')).toBe(
            '# The library\n[package]\nname    = "my-lib"\nversion = "1.1.0" # released with the rest\nedition = "2024"\n\n[dependencies]\ndependency = { path = "../dependency", version = "2.0.0" } # path and registry\nrenamed    = { package = "dependency", path = "../dependency", version = "2.0.0" }\nserde      = "1"\n\n[dependencies.tabled]\npackage = "dependency"\npath    = "../dependency"\nversion = "2.0.0"\n\n[target.\'cfg(unix)\'.dev-dependencies]\ndependency = { path = "../dependency", version = "2.0.0" }\n',
        );
    });

    it.for([
        { case: 'LF', text: '[package]\nname = "my-lib"\nedition = "2024"\n', moved: '[package]\nname = "my-lib"\nedition = "2024"\nversion = "1.1.0"\n' },
        { case: 'CRLF', text: '[package]\r\nname = "my-lib"\r\nedition = "2024"\r\n', moved: '[package]\r\nname = "my-lib"\r\nedition = "2024"\r\nversion = "1.1.0"\r\n' },
        { case: 'a multi-line array', text: '[package]\n# kept\nname = "my-lib"\nversion = "1.0.0"\nauthors = [\n    "Someone",\n]\n', moved: '[package]\n# kept\nname = "my-lib"\nversion = "1.1.0"\nauthors = [\n    "Someone",\n]\n' },
    ])('writes the version into a manifest with $case in its own form', async ({ text, moved }) => {
        const tree = written({ [MANIFEST]: text });
        const actions = await actioned(tree, { lock: 'packages/my-lib/Cargo.lock' });

        await actions.updateProjectVersion(tree, '1.1.0');

        expect(tree.read(MANIFEST, 'utf-8')).toBe(moved);
    });

    it('writes any version, prerelease included, into the package table and reads that version back', async () => {
        expect.hasAssertions();
        await assert(
            asyncProperty(tuple(nat(), nat(), nat()), option(stringMatching(/^[a-z][a-z\d]{0,7}$/u), { nil: undefined }), async (parts, prerelease) => {
                const version = [parts.join('.'), ...(prerelease === undefined ? [] : [prerelease])].join('-');
                const tree = written({ [MANIFEST]: '[package]\nname = "my-lib"\nversion = "1.0.0"\n' });
                const actions = await actioned(tree, { lock: 'packages/my-lib/Cargo.lock' });

                await actions.updateProjectVersion(tree, version);

                expect(tree.read(MANIFEST, 'utf-8')).toBe(`[package]\nname = "my-lib"\nversion = "${version}"\n`);
                await expect(actions.readCurrentVersionFromSourceManifest(tree)).resolves.toStrictEqual({ manifestPath: MANIFEST, currentVersion: version });
            }),
        );
    });

    it('moves the workspace dependency row of the crate with its version, leaving a member that inherits the row untouched', async () => {
        const inheriting = '[package]\nname = "dependency"\nversion = "1.0.0"\n\n[dependencies]\nmy-lib.workspace = true\n';
        const tree = written({
            'Cargo.toml': '[workspace]\nmembers = ["packages/*"]\n\n[workspace.dependencies]\nmy-lib = { path = "packages/my-lib", version = "1.0.0" }\n',
            [MANIFEST]: '[package]\nname = "my-lib"\nversion = "1.0.0"\n',
            'packages/dependency/Cargo.toml': inheriting,
        });
        const actions = await actioned(tree, { lock: 'Cargo.lock' });

        await actions.updateProjectVersion(tree, '1.1.0');

        expect([tree.read('Cargo.toml', 'utf-8'), tree.read('packages/dependency/Cargo.toml', 'utf-8')]).toStrictEqual(['[workspace]\nmembers = ["packages/*"]\n\n[workspace.dependencies]\nmy-lib = { path = "packages/my-lib", version = "1.1.0" }\n', inheriting]);
    });

    it.for([
        { case: 'a manifest that is not TOML', text: '[package\nname = "my-lib"\nversion = "1.0.0"\n' },
        { case: 'a version a duplicate key shadows', text: '[package]\nname = "my-lib"\nversion = "1.0.0"\nversion = "5.5.5"\n' },
    ])('refuses a version move into $case, naming the file and leaving it as it was', async ({ text }) => {
        const tree = written({ [MANIFEST]: text });
        const actions = await actioned(tree, { lock: 'packages/my-lib/Cargo.lock' });

        await expect(actions.updateProjectVersion(tree, '1.1.0')).rejects.toThrow(`${MANIFEST} does not read as TOML`);
        expect(tree.read(MANIFEST, 'utf-8')).toBe(text);
    });

    it('refuses a requirement move into a table a duplicate table shadows, naming the file and leaving it as it was', async () => {
        const text = '[package]\nname = "my-lib"\nversion = "1.0.0"\n\n[dependencies]\ndependency = "1.0.0"\n\n[dependencies]\ndependency = "5.0.0"\n';
        const tree = written({ 'packages/dependency/Cargo.toml': '[package]\nname = "dependency"\nversion = "1.0.0"\n', [MANIFEST]: text });
        const actions = await actioned(tree, { lock: 'packages/my-lib/Cargo.lock' });

        await expect(actions.updateProjectDependencies(tree, graph, { dependency: '2.0.0' })).rejects.toThrow(`${MANIFEST} does not read as TOML`);
        expect(tree.read(MANIFEST, 'utf-8')).toBe(text);
    });
});

describe('workspace-inherited versions', () => {
    it('reads the current version of a crate inheriting version.workspace from the workspace manifest', async () => {
        const tree = written({ 'Cargo.toml': '[workspace]\nmembers = ["packages/*"]\n\n[workspace.package]\nversion = "3.2.1"\n', [MANIFEST]: '[package]\nname = "my-lib"\nversion.workspace = true\n' });
        const actions = await actioned(tree, { lock: 'Cargo.lock' });

        await expect(actions.readCurrentVersionFromSourceManifest(tree)).resolves.toStrictEqual({ manifestPath: 'Cargo.toml', currentVersion: '3.2.1' });
    });

    it('refuses an independent release group for a crate inheriting its version, naming the group to change', async () => {
        const tree = written({ 'Cargo.toml': '[workspace]\nmembers = ["packages/*"]\n\n[workspace.package]\nversion = "3.2.1"\n', [MANIFEST]: '[package]\nname = "my-lib"\nversion.workspace = true\n' });
        const actions = await actioned(tree, { lock: 'Cargo.lock' });

        await expect(actions.validate(tree)).rejects.toThrow(`${MANIFEST}: inherits the version in Cargo.toml, which every inheriting member shares, so my-lib needs a fixed release group in place of the independent group crates`);
    });

    it('reads the requirement a crate inherits from the workspace dependency table as its current dependency version', async () => {
        const tree = written({
            'Cargo.toml': '[workspace]\nmembers = ["packages/*"]\n\n[workspace.dependencies]\ndependency = { path = "packages/dependency", version = "1.4.0" }\n',
            'packages/dependency/Cargo.toml': '[package]\nname = "dependency"\nversion = "1.4.0"\n',
            [MANIFEST]: '[package]\nname = "my-lib"\nversion = "1.0.0"\n\n[dev-dependencies]\ndependency.workspace = true\n',
        });
        const actions = await actioned(tree, { lock: 'Cargo.lock' });

        await expect(actions.readCurrentVersionOfDependency(tree, graph, 'dependency')).resolves.toStrictEqual({ currentVersion: '1.4.0', dependencyCollection: 'dev-dependencies' });
    });
});

describe('registry version', () => {
    it.for([
        { case: 'a registry the crate names, as one argument however it reads', options: { lock: 'Cargo.lock', registry: 'r"; touch NX_PWNED; "' }, argv: ['search', 'my-lib', '--limit', '1', '--color', 'never', '--registry', 'r"; touch NX_PWNED; "'] },
        { case: 'no registry, leaving cargo its default', options: { lock: 'Cargo.lock' }, argv: ['search', 'my-lib', '--limit', '1', '--color', 'never'] },
    ])('reads the newest published version from cargo search with $case', async ({ options, argv }, { workspace }) => {
        pathed(join(workspace.root, '.bin'), { cargo: 'printf \'%s\\n\' "$@" > argv\necho \'my-lib = "1.2.3"    # A library\'' });
        workspace.write({ [MANIFEST]: '[package]\nname = "my-lib"\nversion = "1.0.0"\n' });
        const tree = new FsTree(workspace.root, false);
        const actions = await actioned(tree, options);

        const { currentVersion } = await actions.readCurrentVersionFromRegistry(tree);

        expect(currentVersion).toBe('1.2.3');
        expect(readFileSync(join(workspace.root, 'argv'), 'utf-8').split('\n').slice(0, -1)).toStrictEqual(argv);
    });
});

describe('lock refresh', { timeout: 120_000 }, () => {
    it.for([
        { case: 'brings the lock in line with the versioned manifests', options: {}, changedFiles: ['Cargo.lock'], locked: 'name = "my-lib"\nversion = "1.1.0"' },
        { case: 'leaves the lock where skipLockFileUpdate is set', options: { skipLockFileUpdate: true }, changedFiles: [], locked: 'name = "my-lib"\nversion = "1.0.0"' },
    ])('$case', async ({ options, changedFiles, locked }, { workspace }) => {
        workspace.write({ 'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["packages/*"]\n', [MANIFEST]: '[package]\nname = "my-lib"\nversion = "1.0.0"\nedition = "2024"\n', 'packages/my-lib/src/lib.rs': '' });
        execFileSync('cargo', ['generate-lockfile'], { cwd: workspace.root });
        workspace.write({ [MANIFEST]: '[package]\nname = "my-lib"\nversion = "1.1.0"\nedition = "2024"\n' });

        const result = await afterAllProjectsVersioned(workspace.root, { dryRun: false, verbose: false, rootVersionActionsOptions: options });

        expect(result).toStrictEqual({ changedFiles, deletedFiles: [] });
        expect(readFileSync(join(workspace.root, 'Cargo.lock'), 'utf-8')).toContain(locked);
    });
});
