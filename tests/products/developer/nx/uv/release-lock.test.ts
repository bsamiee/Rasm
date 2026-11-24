import { execFileSync, spawnSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import { join, normalize } from 'node:path';
import { type ProjectGraph, readCachedProjectGraph } from '@nx/devkit';
import { FsTree } from '@nx/devkit/internal';
import { createNodes } from '@rasm/nx-uv';
import UvVersionActions, { afterAllProjectsVersioned } from '@rasm/nx-uv/release/version-actions';
import { it } from '@rasm/testing/nx';
import { assert, describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));
vi.mock(import('@nx/devkit'), async (actual) => ({ ...(await actual()), readCachedProjectGraph: vi.fn<() => ProjectGraph>() }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const BUILT = '\n[build-system]\nrequires = ["uv_build>=0.13,<0.14"]\nbuild-backend = "uv_build"\n';
const WORKSPACE = {
    'pyproject.toml': '[tool.uv.workspace]\nmembers = ["packages/my-lib"]\n',
    'packages/my-lib/pyproject.toml': `[project]\nname = "My_Lib"\nversion = "1.0.0"\nrequires-python = ">=3.13"\n${BUILT}`,
    'packages/my-lib/src/my_lib/__init__.py': '',
};
const LINKED = {
    'pyproject.toml': '[tool.uv.workspace]\nmembers = ["packages/*"]\n',
    'packages/library/pyproject.toml': `[project]\nname = "library"\nversion = "1.0.0"\nrequires-python = ">=3.13"\n${BUILT}`,
    'packages/internal/pyproject.toml': `[project]\nname = "internal"\nversion = "1.0.0"\nrequires-python = ">=3.13"\n${BUILT}`,
    'packages/application/pyproject.toml': `[project]\nname = "application"\nversion = "1.0.0"\nrequires-python = ">=3.13"\ndependencies = ["library>=1.0.0"]\n\n[tool.uv.sources]\nlibrary = { workspace = true }\n${BUILT}`,
    'packages/application/project.json': '{ "implicitDependencies": ["internal", "library"] }',
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('readDependencies', { timeout: 60_000 }, () => {
    it('reports the members the manifest of a member names, as uv export lists them, and none a project.json alone declares', async ({ workspace }) => {
        execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: workspace.write(LINKED).root });
        const graph = await workspace.graph({ createNodes });
        const node = graph.nodes['application'];
        assert(node !== undefined, 'application is a node of the graph');
        const tree = new FsTree(workspace.root, false);
        const actions = new UvVersionActions({ name: 'default', projects: ['application'], projectsRelationship: 'independent', resolvedVersionPlans: false, version: {}, changelog: false, releaseTag: {}, versionPlans: false }, node, {
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
        });
        await actions.init(tree);

        const dependencies = await actions.readDependencies(tree, graph);

        const exported = execFileSync('uv', ['export', '--frozen', '--package', 'application', '--no-hashes', '--no-emit-project', '--quiet'], { cwd: workspace.root, encoding: 'utf-8' });
        const editable = exported.split('\n').flatMap((row) => (row.startsWith('-e ') ? [normalize(row.slice('-e '.length))] : []));
        expect([dependencies.map(({ target }) => graph.nodes[target]?.data.root), graph.dependencies['application']?.map(({ target }) => target).toSorted()]).toStrictEqual([editable, ['internal', 'library']]);
        expect(editable).toStrictEqual(['packages/library']);
    });
});

describe('readCurrentVersionFromRegistry', { timeout: 60_000 }, () => {
    it('reads the newest release the index settings of the lock folder serve, in its SemVer form', async ({ workspace }) => {
        workspace.write({
            'nested/proj/pyproject.toml': '[tool.uv.workspace]\nmembers = ["packages/my-lib"]\n\n[tool.uv]\nno-index = true\nfind-links = ["wheels"]\n',
            'nested/proj/packages/my-lib/pyproject.toml': `[project]\nname = "My_Lib"\nversion = "1.0.0"\nrequires-python = ">=3.13"\n${BUILT}`,
            'released/pyproject.toml': `[project]\nname = "my-lib"\nversion = "1.2.3rc1"\nrequires-python = ">=3.13"\n${BUILT}`,
            'released/src/my_lib/__init__.py': '',
        });
        execFileSync('uv', ['build', '--wheel', '--quiet', '--out-dir', join(workspace.root, 'nested/proj/wheels')], { cwd: join(workspace.root, 'released') });
        const tree = new FsTree(workspace.root, false);
        const actions = new UvVersionActions(
            { name: 'default', projects: ['my-lib'], projectsRelationship: 'independent', resolvedVersionPlans: false, version: {}, changelog: false, releaseTag: {}, versionPlans: false },
            { name: 'my-lib', type: 'lib', data: { root: 'nested/proj/packages/my-lib' } },
            {
                specifierSource: 'prompt',
                currentVersionResolver: 'registry',
                currentVersionResolverMetadata: {},
                fallbackCurrentVersionResolver: 'disk',
                versionPrefix: 'auto',
                preserveLocalDependencyProtocols: false,
                preserveMatchingDependencyRanges: false,
                adjustSemverBumpsForZeroMajorVersion: false,
                applyPreidToDependents: false,
                versionActionsOptions: { lock: 'nested/proj/uv.lock' },
                manifestRootsToUpdate: [],
                dockerOptions: {},
            },
        );
        await actions.init(tree);

        const { currentVersion } = await actions.readCurrentVersionFromRegistry(tree);

        expect(currentVersion).toBe('1.2.3-rc.1');
    });
});

describe(afterAllProjectsVersioned, { timeout: 60_000 }, () => {
    it.for([
        { case: 'a stale lock', version: '1.1.0', options: { dryRun: false }, changed: ['uv.lock'] },
        { case: 'skipLockFileUpdate', version: '1.1.0', options: { dryRun: false, rootVersionActionsOptions: { skipLockFileUpdate: true } }, changed: [] },
    ])('relocks the uv workspace of each Python project and reports the lock it changed, given $case', async ({ version, options, changed }, { workspace }) => {
        execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: workspace.write(WORKSPACE).root });
        vi.mocked(readCachedProjectGraph).mockReturnValue(await workspace.graph({ createNodes }));
        const before = readFileSync(join(workspace.root, 'uv.lock'), 'utf-8');
        workspace.write({ 'packages/my-lib/pyproject.toml': WORKSPACE['packages/my-lib/pyproject.toml'].replace('1.0.0', version) });

        const { changedFiles } = await afterAllProjectsVersioned(workspace.root, { verbose: false, ...options });

        expect(changedFiles).toStrictEqual(changed);
        expect([readFileSync(join(workspace.root, 'uv.lock'), 'utf-8') === before, spawnSync('uv', ['lock', '--check', '--offline', '--quiet'], { cwd: workspace.root }).status === 0]).toStrictEqual([changed.length === 0, changed.length > 0]);
    });
});
