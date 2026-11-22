import type { ProjectGraph, Tree } from '@nx/devkit';
import { createTreeWithEmptyWorkspace } from '@nx/devkit/testing';
import UvVersionActions from '@rasm/nx-uv/release/version-actions';
import { describe, expect, it } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'packages/my-lib/pyproject.toml';
const DEPENDENCY = { 'packages/dependency/pyproject.toml': '[project]\nname = "dependency"\nversion = "1.0.0"\n' };
const GRAPH: ProjectGraph = {
    nodes: {
        'my-lib': { name: 'my-lib', type: 'lib', data: { root: 'packages/my-lib' } },
        dependency: { name: 'dependency', type: 'lib', data: { root: 'packages/dependency' } },
    },
    dependencies: {},
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const versioned = async (files: Readonly<Record<string, string>>) => {
    const tree: Tree = createTreeWithEmptyWorkspace();
    Object.entries(files).forEach(([path, text]) => tree.write(path, text));
    const actions = new UvVersionActions(
        { name: 'default', projects: ['my-lib'], projectsRelationship: 'independent', resolvedVersionPlans: false, version: {}, changelog: false, releaseTag: {}, versionPlans: false },
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
        },
    );
    await actions.init(tree);
    return { tree, actions };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('project version', () => {
    it.for([
        { case: 'comments and aligned keys', before: '# manifest\n[project]\nname    = "my-lib"\nversion = "1.0.0" # moved by nx release\n', after: '# manifest\n[project]\nname    = "my-lib"\nversion = "1.1.0" # moved by nx release\n' },
        { case: 'CRLF endings', before: '[project]\r\nname = "my-lib"\r\nversion = "1.0.0"\r\n', after: '[project]\r\nname = "my-lib"\r\nversion = "1.1.0"\r\n' },
        { case: 'tab indentation', before: '[project]\n\tname = "my-lib"\n\tversion = "1.0.0"\n', after: '[project]\n\tname = "my-lib"\n\tversion = "1.1.0"\n' },
    ])('writes the new version and keeps $case', async ({ before, after }) => {
        const { tree, actions } = await versioned({ [MANIFEST]: before });

        await actions.updateProjectVersion(tree, '1.1.0');

        expect(tree.read(MANIFEST, 'utf-8')).toBe(after);
    });

    it.for([
        { version: '1.1.0-alpha.1', written: '1.1.0a1' },
        { version: '1.1.0-beta.2', written: '1.1.0b2' },
        { version: '1.1.0-rc.3', written: '1.1.0rc3' },
    ])('writes $version in its PEP 440 form $written and reads it back', async ({ version, written }) => {
        const { tree, actions } = await versioned({ [MANIFEST]: '[project]\nname = "my-lib"\nversion = "1.0.0"\n' });

        await actions.updateProjectVersion(tree, version);

        expect(tree.read(MANIFEST, 'utf-8')).toBe(`[project]\nname = "my-lib"\nversion = "${written}"\n`);
        await expect(actions.readCurrentVersionFromSourceManifest(tree)).resolves.toStrictEqual({ currentVersion: version, manifestPath: MANIFEST });
    });

    it.for([
        { case: 'a malformed manifest', manifest: '[project\nname = "my-lib"\nversion = "1.0.0"\n', version: '1.1.0', refusal: `${MANIFEST} does not read as TOML` },
        { case: 'a duplicate version key', manifest: '[project]\nname = "my-lib"\nversion = "1.0.0"\nversion = "5.5.5"\n', version: '1.1.0', refusal: `${MANIFEST} does not read as TOML` },
        { case: 'a prerelease id PEP 440 lacks', manifest: '[project]\nname = "my-lib"\nversion = "1.0.0"\n', version: '1.1.0-next.1', refusal: '1.1.0-next.1: has no PEP 440 form; use a prerelease id of alpha, beta, or rc' },
    ])('refuses $case and leaves the manifest unchanged', async ({ manifest, version, refusal }) => {
        const { tree, actions } = await versioned({ [MANIFEST]: manifest });

        await expect(actions.updateProjectVersion(tree, version)).rejects.toThrow(refusal);
        expect(tree.read(MANIFEST, 'utf-8')).toBe(manifest);
    });

    it.for(['1!1.0.0', '1.0.0.post1', '1.0.0.dev1', '1.0.0+local', '1.0.0.1'])('refuses to read a source version %s that SemVer cannot state', async (version) => {
        const { tree, actions } = await versioned({ [MANIFEST]: `[project]\nname = "my-lib"\nversion = "${version}"\n` });

        await expect(actions.readCurrentVersionFromSourceManifest(tree)).rejects.toThrow(`${version}: has an epoch, post, dev, or local part, or more than three release numbers, which SemVer cannot state`);
    });
});

describe('dependency requirements', () => {
    it('moves the lower bound of each requirement on the dependency in dependencies, optional dependencies, and dependency groups', async () => {
        const { tree, actions } = await versioned({
            ...DEPENDENCY,
            [MANIFEST]:
                '[project]\nname = "my-lib"\nversion = "1.0.0"\ndependencies = [\n    "dependency==1.0.0", # sibling\n    "requests>=2.31",\n]\n\n[project.optional-dependencies]\nfast = ["Dependency[speed]~=1.0 ; python_version >= \'3.13\'"]\n\n[dependency-groups]\ndev = ["dependency>=1.0.0,<3", { include-group = "lint" }]\nlint = ["dependency"]\n',
        });

        const logs = await actions.updateProjectDependencies(tree, GRAPH, { dependency: '2.5.0' });

        expect(tree.read(MANIFEST, 'utf-8')).toBe(
            '[project]\nname = "my-lib"\nversion = "1.0.0"\ndependencies = [\n    "dependency==2.5.0", # sibling\n    "requests>=2.31",\n]\n\n[project.optional-dependencies]\nfast = ["Dependency[speed]~=2.5 ; python_version >= \'3.13\'"]\n\n[dependency-groups]\ndev = ["dependency>=2.5.0,<3", { include-group = "lint" }]\nlint = ["dependency"]\n',
        );
        expect(logs).toStrictEqual([`Lower bound of dependency moved to 2.5.0 in ${MANIFEST}`]);
    });

    it('leaves a manifest requiring no updated dependency unchanged', async () => {
        const manifest = '[project]\nname = "my-lib"\nversion = "1.0.0"\ndependencies = ["requests>=2.0.0", "other-dependency==1.0.0"]\n';
        const { tree, actions } = await versioned({ ...DEPENDENCY, [MANIFEST]: manifest });

        await actions.updateProjectDependencies(tree, GRAPH, { dependency: '2.0.0' });

        expect(tree.read(MANIFEST, 'utf-8')).toBe(manifest);
    });

    it.for([
        { case: 'a requirement the moved bound excludes', manifest: '[project]\nname = "my-lib"\nversion = "1.0.0"\ndependencies = ["dependency>=1.0,<2"]\n', refusal: 'dependency>=1.0,<2: excludes the released 2.0.0 once each lower bound moves to it' },
        { case: 'a duplicate dependencies key', manifest: '[project]\nname = "my-lib"\nversion = "1.0.0"\ndependencies = ["dependency>=1.0.0"]\ndependencies = ["dependency>=5.0.0"]\n', refusal: `${MANIFEST} does not read as TOML` },
    ])('refuses $case and leaves the manifest unchanged', async ({ manifest, refusal }) => {
        const { tree, actions } = await versioned({ ...DEPENDENCY, [MANIFEST]: manifest });

        await expect(actions.updateProjectDependencies(tree, GRAPH, { dependency: '2.0.0' })).rejects.toThrow(refusal);
        expect(tree.read(MANIFEST, 'utf-8')).toBe(manifest);
    });

    it.for([
        { case: 'an optional dependency', table: '\n[project.optional-dependencies]\nfast = ["dependency>=1.0"]\n', read: { currentVersion: '1.0.0', dependencyCollection: 'optional-dependencies.fast' } },
        { case: 'a dependency group', table: '\n[dependency-groups]\ndev = ["requests", "dependency~=1.0"]\n', read: { currentVersion: '1.0.0', dependencyCollection: 'dependency-groups.dev' } },
        { case: 'no requirement', table: '', read: { currentVersion: null, dependencyCollection: null } },
    ])('reads the version and collection of a dependency the manifest requires through $case', async ({ table, read }) => {
        const { tree, actions } = await versioned({ ...DEPENDENCY, [MANIFEST]: `[project]\nname = "my-lib"\nversion = "1.0.0"\ndependencies = ["requests"]\n${table}` });

        await expect(actions.readCurrentVersionOfDependency(tree, GRAPH, 'dependency')).resolves.toStrictEqual(read);
    });
});
