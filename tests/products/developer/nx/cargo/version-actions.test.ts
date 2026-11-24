import { execFileSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { type ProjectGraph, type readCachedProjectGraph, type Tree, workspaceRoot } from '@nx/devkit';
import { createTreeWithEmptyWorkspace } from '@nx/devkit/testing';
import CargoVersionActions, { afterAllProjectsVersioned } from '@rasm/nx-cargo/release/version-actions';
import { it } from '@rasm/testing/nx';
import { array, assert, asyncProperty, constantFrom, nat, option, stringMatching, tuple } from 'fast-check';
import type { ReleaseGroupWithName } from 'nx/src/command-line/release/config/filter-release-groups';
import type { FinalConfigForProject } from 'nx/src/command-line/release/utils/release-graph';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit'), async (actual) => ({
    ...(await actual()),
    readCachedProjectGraph: vi.fn<typeof readCachedProjectGraph>(() => ({
        nodes: {
            'my-lib': { name: 'my-lib', type: 'lib', data: { root: 'packages/my-lib', release: { version: { versionActions: '@rasm/nx-cargo/release/version-actions', versionActionsOptions: { lock: 'Cargo.lock' } } } } },
            'py-lib': { name: 'py-lib', type: 'lib', data: { root: 'python/py-lib', release: { version: { versionActions: '@rasm/nx-uv/release/version-actions', versionActionsOptions: { lock: 'python/uv.lock' } } } } },
        },
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

const requiring = (padding: string, comment: string) =>
    [
        '[package]',
        'name = "my-lib"',
        'version = "1.0.0"',
        'edition = "2024"',
        '',
        '[dependencies]',
        `dependency${padding}= { path = "../dependency", version = "1.0.0" }${comment}`,
        `renamed${padding}= { package = "dependency", path = "../dependency", version = "1.0.0" }`,
        `serde${padding}= "1.0.0"${comment}`,
        '',
        '[dev-dependencies.tabled]',
        'package = "dependency"',
        'path    = "../dependency"',
        'version = "1.0.0"',
        '',
        "[target.'cfg(unix)'.build-dependencies]",
        `dependency${padding}= { path = "../dependency", version = "1.0.0" }`,
        '',
    ].join('\n');

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('requirement edits', () => {
    it('moves only the version literal of each requirement on the released crate, whatever its spacing and comments', async () => {
        expect.hasAssertions();
        await assert(
            asyncProperty(constantFrom(' ', '  ', '    '), option(stringMatching(/^ # [a-z ]{0,12}$/u), { nil: '' }), async (padding, comment) => {
                const before = requiring(padding, comment);
                const tree = written({ 'packages/dependency/Cargo.toml': '[package]\nname = "dependency"\nversion = "1.0.0"\n', [MANIFEST]: before });
                const actions = await actioned(tree, { lock: 'Cargo.lock' });

                await actions.updateProjectDependencies(tree, graph, { dependency: '2.0.0' });

                const after = tree.read(MANIFEST, 'utf-8') ?? '';
                expect([after.split('"2.0.0"').length - 1, after.replaceAll('"2.0.0"', '"1.0.0"'), after.includes(`serde${padding}= "1.0.0"`)]).toStrictEqual([4, before, true]);
            }),
        );
    });
});

describe('package versions', () => {
    it('writes any version, prerelease included, into the package table alone and reads that version back', async () => {
        expect.hasAssertions();
        await assert(
            asyncProperty(tuple(nat(), nat(), nat()), array(stringMatching(/^[a-z][a-z\d]{0,7}$/u), { maxLength: 2 }), async (parts, prerelease) => {
                const version = [parts.join('.'), ...(prerelease.length === 0 ? [] : [prerelease.join('.')])].join('-');
                const tree = written({ 'Cargo.toml': '[workspace]\nmembers = ["packages/*"]\n', [MANIFEST]: '# kept\n[package]\nname = "my-lib"\nversion = "1.0.0"\n\n[dependencies]\nserde = "1.0.0"\n' });
                const actions = await actioned(tree, { lock: 'Cargo.lock' });

                await actions.updateProjectVersion(tree, version);

                expect(tree.read(MANIFEST, 'utf-8')).toBe(`# kept\n[package]\nname = "my-lib"\nversion = "${version}"\n\n[dependencies]\nserde = "1.0.0"\n`);
                await expect(actions.readCurrentVersionFromSourceManifest(tree)).resolves.toStrictEqual({ manifestPath: MANIFEST, currentVersion: version });
            }),
        );
    });

    it('reads the version a crate inherits from the workspace manifest, and refuses an independent release group for it', async () => {
        const tree = written({ 'Cargo.toml': '[workspace]\nmembers = ["packages/*"]\n\n[workspace.package]\nversion = "3.2.1"\n', [MANIFEST]: '[package]\nname = "my-lib"\nversion.workspace = true\n' });
        const actions = await actioned(tree, { lock: 'Cargo.lock' });

        await expect(actions.readCurrentVersionFromSourceManifest(tree)).resolves.toStrictEqual({ manifestPath: 'Cargo.toml', currentVersion: '3.2.1' });
        await expect(actions.validate(tree)).rejects.toThrow(`${MANIFEST}: inherits the version in Cargo.toml, which every inheriting member shares, so my-lib needs a fixed release group in place of the independent group crates`);
    });
});

describe('lock refresh', { timeout: 120_000 }, () => {
    it('brings the lock of each project these actions version in line with its manifests, leaving the locks of other version actions alone', async ({ workspace }) => {
        workspace.write({ 'rust-toolchain.toml': readFileSync(join(workspaceRoot, 'rust-toolchain.toml'), 'utf-8'), 'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["packages/*"]\n', [MANIFEST]: '[package]\nname = "my-lib"\nversion = "1.0.0"\nedition = "2024"\n', 'packages/my-lib/src/lib.rs': '' });
        execFileSync('cargo', ['generate-lockfile'], { cwd: workspace.root });
        workspace.write({ [MANIFEST]: '[package]\nname = "my-lib"\nversion = "1.1.0"\nedition = "2024"\n' });

        const result = await afterAllProjectsVersioned(workspace.root, { dryRun: false, verbose: false, rootVersionActionsOptions: {} });

        expect(result).toStrictEqual({ changedFiles: ['Cargo.lock'], deletedFiles: [] });
        expect(readFileSync(join(workspace.root, 'Cargo.lock'), 'utf-8')).toContain('name = "my-lib"\nversion = "1.1.0"');
    });
});
