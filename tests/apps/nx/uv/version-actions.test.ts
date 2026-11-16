import { execFileSync } from 'node:child_process';
import { join } from 'node:path';
import { type ProjectGraph, readCachedProjectGraph, type Tree } from '@nx/devkit';
import { FsTree } from '@nx/devkit/internal';
import { createTreeWithEmptyWorkspace } from '@nx/devkit/testing';
import UvVersionActions, { afterAllProjectsVersioned } from '@rasm/nx-uv/release/version-actions';
import { TempFs } from '@rasm/testing/temp-fs';
import { ChildProcess } from 'effect/process';
import { createNxReleaseConfig, IMPLICIT_DEFAULT_RELEASE_GROUP } from 'nx/src/command-line/release/config/config';
import type { FinalConfigForProject } from 'nx/src/command-line/release/utils/release-graph';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit'), async (actual) => ({
    ...(await actual()),
    readCachedProjectGraph: vi.fn<() => ProjectGraph>(),
}));
vi.mock(import('effect/process'), { spy: true });

// --- [CONSTANTS] -----------------------------------------------------------------------

const compile = ['pip', 'compile', '-', '--no-deps', '--no-header', '--no-annotate', '--color', 'never', '--quiet'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const manifest = (dependencies: string) => `[project]\nname = "my-lib"\nversion = "1.0.0"\ndependencies = ${dependencies}\n`;

const createProjectGraph = (): ProjectGraph => ({
    nodes: {
        'my-lib': { name: 'my-lib', type: 'lib', data: { root: 'packages/my-lib', metadata: { technologies: ['python'] } } },
        dependency: { name: 'dependency', type: 'lib', data: { root: 'packages/dependency', metadata: { technologies: ['python'] } } },
    },
    dependencies: {},
});

const createVersionActions = async (tree: Tree, config: Partial<FinalConfigForProject> = {}): Promise<UvVersionActions> => {
    const { nxReleaseConfig } = await createNxReleaseConfig(createProjectGraph(), {}, { projects: ['my-lib'], projectsRelationship: 'independent' });
    const group = nxReleaseConfig?.groups?.[IMPLICIT_DEFAULT_RELEASE_GROUP];
    const versionActions = new UvVersionActions(
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
            versionActionsOptions: { lock: 'uv.lock' },
            manifestRootsToUpdate: [],
            dockerOptions: {},
            ...config,
        },
    );
    await versionActions.init(tree);
    return versionActions;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const previous = process.cwd();
        const tempFs = new TempFs('uv-version-actions');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(previous);
            tempFs.cleanup();
        });
        return tempFs;
    })
    .extend('spawn', () => vi.mocked(ChildProcess.make).mockClear())
    .extend('locked', async ({ tempFs }) => {
        await tempFs.createFiles({ 'pyproject.toml': '[tool.uv.workspace]\nmembers = ["packages/my-lib"]\n', 'packages/my-lib/pyproject.toml': manifest('[]') });
        execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: tempFs.tempDir });
        vi.mocked(readCachedProjectGraph).mockReturnValue(createProjectGraph());
        return tempFs;
    });

describe('readCurrentVersionFromRegistry', () => {
    it('passes the member name to uv pip compile as one requirement on standard input, never through a shell', async ({ tempFs, spawn }) => {
        await tempFs.createFiles({ 'packages/my-lib/pyproject.toml': manifest('[]') });
        spawn.mockReturnValueOnce(ChildProcess.make('echo', ['my-lib==1.2.3']));
        const tree = new FsTree(tempFs.tempDir, false);
        const versionActions = await createVersionActions(tree);

        const { currentVersion, logText } = await versionActions.readCurrentVersionFromRegistry(tree);

        expect(currentVersion).toBe('1.2.3');
        expect(logText).toBe('uv pip compile - --no-deps --no-header --no-annotate --color never --quiet');
        expect(spawn).toHaveBeenCalledWith('uv', compile, expect.objectContaining({ cwd: tempFs.tempDir }));
    });

    it('runs uv pip compile in the folder of the lock, whose uv settings name the index', async ({ tempFs, spawn }) => {
        await tempFs.createFiles({ 'packages/my-lib/pyproject.toml': manifest('[]'), 'nested/proj/uv.toml': '[[index]]\nurl = "https://r.example.com/simple"\ndefault = true\n' });
        spawn.mockReturnValueOnce(ChildProcess.make('echo', ['my-lib==1.2.3']));
        const tree = new FsTree(tempFs.tempDir, false);
        const versionActions = await createVersionActions(tree, { versionActionsOptions: { lock: 'nested/proj/uv.lock' } });

        await versionActions.readCurrentVersionFromRegistry(tree);

        expect(spawn).toHaveBeenCalledWith('uv', compile, expect.objectContaining({ cwd: join(tempFs.tempDir, 'nested/proj') }));
    });
});

describe('UvVersionActions', () => {
    it('preserves pyproject.toml formatting when updating versions and dependencies', async () => {
        const tree = createTreeWithEmptyWorkspace();
        tree.write('packages/dependency/pyproject.toml', '[project]\nname = "dependency"\nversion = "1.0.0"\n');
        tree.write(
            'packages/my-lib/pyproject.toml',
            `# Library manifest
[project]
name         = "my-lib"
version      = "1.0.0" # moved by nx release
dependencies = [
    "dependency>=1.0.0", # sibling
    "requests>=2.31",
]
`,
        );
        const versionActions = await createVersionActions(tree);

        await versionActions.updateProjectVersion(tree, '1.1.0');
        await versionActions.updateProjectDependencies(tree, createProjectGraph(), { dependency: '2.0.0' });

        expect(tree.read('packages/my-lib/pyproject.toml', 'utf-8')).toBe(`# Library manifest
[project]
name         = "my-lib"
version      = "1.1.0" # moved by nx release
dependencies = [
    "dependency>=2.0.0", # sibling
    "requests>=2.31",
]
`);
    });

    it.each([
        { name: 'LF endings', input: '[project]\nname = "my-lib"\nversion = "1.0.0"\n', expected: '[project]\nname = "my-lib"\nversion = "1.1.0"\n' },
        { name: 'CRLF endings', input: '[project]\r\nname = "my-lib"\r\nversion = "1.0.0"\r\n', expected: '[project]\r\nname = "my-lib"\r\nversion = "1.1.0"\r\n' },
        { name: 'tab indentation', input: '[project]\n\tname = "my-lib"\n\tversion = "1.0.0"\n', expected: '[project]\n\tname = "my-lib"\n\tversion = "1.1.0"\n' },
    ])('formats an updated version using $name', async ({ input, expected }) => {
        const tree = createTreeWithEmptyWorkspace();
        tree.write('packages/my-lib/pyproject.toml', input);
        const versionActions = await createVersionActions(tree);

        await versionActions.updateProjectVersion(tree, '1.1.0');

        expect(tree.read('packages/my-lib/pyproject.toml', 'utf-8')).toBe(expected);
    });

    it.each([
        { version: '1.1.0-alpha.1', written: '1.1.0a1' },
        { version: '1.1.0-beta.2', written: '1.1.0b2' },
        { version: '1.1.0-rc.3', written: '1.1.0rc3' },
    ])('writes $version in its PEP 440 form $written', async ({ version, written }) => {
        const tree = createTreeWithEmptyWorkspace();
        tree.write('packages/my-lib/pyproject.toml', manifest('[]'));
        const versionActions = await createVersionActions(tree);

        await versionActions.updateProjectVersion(tree, version);

        expect(tree.read('packages/my-lib/pyproject.toml', 'utf-8')).toBe(manifest('[]').replace('1.0.0', written));
        await expect(versionActions.readCurrentVersionFromSourceManifest(tree)).resolves.toStrictEqual({ currentVersion: version, manifestPath: 'packages/my-lib/pyproject.toml' });
    });

    it('rejects a malformed manifest without changing it', async () => {
        const tree = createTreeWithEmptyWorkspace();
        const malformedManifest = '[project\nname = "my-lib"\nversion = "1.0.0"\n';
        tree.write('packages/my-lib/pyproject.toml', malformedManifest);
        const versionActions = await createVersionActions(tree);

        await expect(versionActions.updateProjectVersion(tree, '1.1.0')).rejects.toThrow('packages/my-lib/pyproject.toml does not read as TOML');
        expect(tree.read('packages/my-lib/pyproject.toml', 'utf-8')).toBe(malformedManifest);
    });

    it('rejects a version update shadowed by a duplicate key', async () => {
        const tree = createTreeWithEmptyWorkspace();
        const duplicated = '[project]\nname = "my-lib"\nversion = "1.0.0"\nversion = "5.5.5"\n';
        tree.write('packages/my-lib/pyproject.toml', duplicated);
        const versionActions = await createVersionActions(tree);

        await expect(versionActions.updateProjectVersion(tree, '1.1.0')).rejects.toThrow('packages/my-lib/pyproject.toml does not read as TOML');
        expect(tree.read('packages/my-lib/pyproject.toml', 'utf-8')).toBe(duplicated);
    });

    it('rejects a dependency update shadowed by a duplicate key', async () => {
        const tree = createTreeWithEmptyWorkspace();
        tree.write('packages/dependency/pyproject.toml', '[project]\nname = "dependency"\nversion = "1.0.0"\n');
        const duplicated = `${manifest('["dependency>=1.0.0"]')}dependencies = ["dependency>=5.0.0"]\n`;
        tree.write('packages/my-lib/pyproject.toml', duplicated);
        const versionActions = await createVersionActions(tree);

        await expect(versionActions.updateProjectDependencies(tree, createProjectGraph(), { dependency: '2.0.0' })).rejects.toThrow('packages/my-lib/pyproject.toml does not read as TOML');
        expect(tree.read('packages/my-lib/pyproject.toml', 'utf-8')).toBe(duplicated);
    });

    it('rejects a dependency version outside the requirement once its lower bound moves, without changing the manifest', async () => {
        const tree = createTreeWithEmptyWorkspace();
        tree.write('packages/dependency/pyproject.toml', '[project]\nname = "dependency"\nversion = "1.0.0"\n');
        const bounded = manifest('["dependency>=1.0,<2"]');
        tree.write('packages/my-lib/pyproject.toml', bounded);
        const versionActions = await createVersionActions(tree);

        await expect(versionActions.updateProjectDependencies(tree, createProjectGraph(), { dependency: '2.0.0' })).rejects.toThrow('dependency>=1.0,<2: excludes the released 2.0.0 once each lower bound moves to it');
        expect(tree.read('packages/my-lib/pyproject.toml', 'utf-8')).toBe(bounded);
    });

    it('moves the lower bound of each requirement on the dependency in dependencies, optional dependencies, and dependency groups', async () => {
        const tree = createTreeWithEmptyWorkspace();
        tree.write('packages/dependency/pyproject.toml', '[project]\nname = "dependency"\nversion = "1.0.0"\n');
        tree.write(
            'packages/my-lib/pyproject.toml',
            '[project]\nname = "my-lib"\nversion = "1.0.0"\ndependencies = ["dependency==1.0.0"]\n\n[project.optional-dependencies]\nfast = ["Dependency[speed]~=1.0 ; python_version >= \'3.13\'"]\n\n[dependency-groups]\ndev = ["dependency>=1.0.0,<3", { include-group = "lint" }]\nlint = ["dependency"]\n',
        );
        const versionActions = await createVersionActions(tree);

        const logs = await versionActions.updateProjectDependencies(tree, createProjectGraph(), { dependency: '2.5.0' });

        expect(tree.read('packages/my-lib/pyproject.toml', 'utf-8')).toBe(
            '[project]\nname = "my-lib"\nversion = "1.0.0"\ndependencies = ["dependency==2.5.0"]\n\n[project.optional-dependencies]\nfast = ["Dependency[speed]~=2.5 ; python_version >= \'3.13\'"]\n\n[dependency-groups]\ndev = ["dependency>=2.5.0,<3", { include-group = "lint" }]\nlint = ["dependency"]\n',
        );
        expect(logs).toStrictEqual(['Lower bound of dependency moved to 2.5.0 in packages/my-lib/pyproject.toml']);
    });

    it('leaves requirements on packages outside the update unchanged', async () => {
        const tree = createTreeWithEmptyWorkspace();
        const unrelated = manifest('["requests>=2.0.0", "other-dependency==1.0.0"]');
        tree.write('packages/my-lib/pyproject.toml', unrelated);
        const versionActions = await createVersionActions(tree);

        await versionActions.updateProjectDependencies(tree, createProjectGraph(), {});

        expect(tree.read('packages/my-lib/pyproject.toml', 'utf-8')).toBe(unrelated);
    });
});

describe('afterAllProjectsVersioned', { timeout: 60_000 }, () => {
    it('updates uv.lock when the version it holds for a member is stale', async ({ locked }) => {
        locked.writeFile('packages/my-lib/pyproject.toml', manifest('[]').replace('1.0.0', '1.1.0'));

        const { changedFiles } = await afterAllProjectsVersioned(locked.tempDir, { dryRun: false, verbose: false, rootVersionActionsOptions: {} });

        expect(changedFiles).toStrictEqual(['uv.lock']);
        await expect(locked.readFile('uv.lock')).resolves.toContain('name = "my-lib"\nversion = "1.1.0"');
    });

    it('skips the uv.lock update when the versions it holds are already in sync', async ({ locked }) => {
        const { changedFiles } = await afterAllProjectsVersioned(locked.tempDir, { dryRun: false, verbose: false, rootVersionActionsOptions: {} });

        expect(changedFiles).toStrictEqual([]);
    });

    it('leaves uv.lock unchanged for a dry run', async ({ locked }) => {
        const before = await locked.readFile('uv.lock');
        locked.writeFile('packages/my-lib/pyproject.toml', manifest('[]').replace('1.0.0', '1.1.0'));

        const { changedFiles } = await afterAllProjectsVersioned(locked.tempDir, { dryRun: true, verbose: false, rootVersionActionsOptions: {} });

        expect(changedFiles).toStrictEqual([]);
        await expect(locked.readFile('uv.lock')).resolves.toBe(before);
    });

    it('leaves uv.lock unchanged when skipLockFileUpdate is set', async ({ locked }) => {
        const before = await locked.readFile('uv.lock');
        locked.writeFile('packages/my-lib/pyproject.toml', manifest('[]').replace('1.0.0', '1.1.0'));

        const { changedFiles } = await afterAllProjectsVersioned(locked.tempDir, { dryRun: false, verbose: false, rootVersionActionsOptions: { skipLockFileUpdate: true } });

        expect(changedFiles).toStrictEqual([]);
        await expect(locked.readFile('uv.lock')).resolves.toBe(before);
    });

    it('does not create a uv.lock when none exists', async ({ locked }) => {
        locked.removeFileSync('uv.lock');

        const { changedFiles } = await afterAllProjectsVersioned(locked.tempDir, { dryRun: false, verbose: false, rootVersionActionsOptions: {} });

        expect(changedFiles).toStrictEqual([]);
    });
});
