import type { ProjectGraph, Tree } from '@nx/devkit';
import { createTreeWithEmptyWorkspace } from '@nx/devkit/testing';
import UvVersionActions from '@rasm/nx-uv/release/version-actions';
import { Option } from 'effect';
import { assert, asyncProperty, constantFrom, nat, option, tuple } from 'fast-check';
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
const SPELLED = { alpha: 'a', beta: 'b', rc: 'rc' } as const;
const RELEASES = tuple(tuple(nat(99), nat(99), nat(99)), option(tuple(constantFrom('alpha', 'beta', 'rc'), nat(9)), { nil: undefined })).map(([release, pre]) => ({
    semver: [release.join('.'), ...Option.toArray(Option.map(Option.fromUndefinedOr(pre), (held) => held.join('.')))].join('-'),
    pep440: [release.join('.'), ...Option.toArray(Option.map(Option.fromUndefinedOr(pre), ([id, number]) => `${SPELLED[id]}${number}`))].join(''),
}));

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

const read = async (version: string): Promise<unknown> => {
    const { tree, actions } = await versioned({ [MANIFEST]: `[project]\nname = "my-lib"\nversion = "${version}"\n` });
    return actions.readCurrentVersionFromSourceManifest(tree).catch((error: unknown) => error);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('project version', () => {
    it('writes any SemVer release or alpha, beta, or rc prerelease in its PEP 440 spelling into the version field alone and reads the same version back', async () => {
        expect.hasAssertions();
        await assert(
            asyncProperty(RELEASES, constantFrom('\n', '\r\n'), async ({ semver, pep440 }, ending) => {
                const before = ['# manifest', '[project]', 'name    = "my-lib"', 'version = "1.0.0" # moved by nx release', 'dependencies = ["requests>=2.31"]', ''].join(ending);
                const { tree, actions } = await versioned({ [MANIFEST]: before });

                await actions.updateProjectVersion(tree, semver);

                expect(tree.read(MANIFEST, 'utf-8')).toBe(before.replace('"1.0.0"', `"${pep440}"`));
                await expect(actions.readCurrentVersionFromSourceManifest(tree)).resolves.toStrictEqual({ currentVersion: semver, manifestPath: MANIFEST });
            }),
        );
    });

    it('refuses to read a source version SemVer cannot state, naming it', async () => {
        expect.hasAssertions();
        const versions = ['1!1.0.0', '1.0.0.post1', '1.0.0.dev1', '1.0.0+local', '1.0.0.1'];

        const refusals = await Promise.all(versions.map(read));

        expect(refusals).toMatchObject(versions.map((version) => ({ message: expect.stringContaining(version) as unknown })));
    });
});

describe('dependency requirements', () => {
    it('moves the lower bound of each requirement on the dependency in dependencies, optional dependencies, and dependency groups, keeping the rest of the text', async () => {
        const { tree, actions } = await versioned({
            ...DEPENDENCY,
            [MANIFEST]:
                '[project]\nname = "my-lib"\nversion = "1.0.0"\ndependencies = [\n    "dependency==1.0.0", # sibling\n    "requests>=2.31",\n]\n\n[project.optional-dependencies]\nfast = ["Dependency[speed]~=1.0 ; python_version >= \'3.13\'"]\n\n[dependency-groups]\ndev = ["dependency>=1.0.0,<3", { include-group = "lint" }]\nlint = ["dependency"]\n',
        });

        await actions.updateProjectDependencies(tree, GRAPH, { dependency: '2.5.0' });

        expect(tree.read(MANIFEST, 'utf-8')).toBe(
            '[project]\nname = "my-lib"\nversion = "1.0.0"\ndependencies = [\n    "dependency==2.5.0", # sibling\n    "requests>=2.31",\n]\n\n[project.optional-dependencies]\nfast = ["Dependency[speed]~=2.5 ; python_version >= \'3.13\'"]\n\n[dependency-groups]\ndev = ["dependency>=2.5.0,<3", { include-group = "lint" }]\nlint = ["dependency"]\n',
        );
    });

    it('refuses a requirement the moved bound excludes, leaving the manifest unchanged', async () => {
        const manifest = '[project]\nname = "my-lib"\nversion = "1.0.0"\ndependencies = ["dependency>=1.0,<2"]\n';
        const { tree, actions } = await versioned({ ...DEPENDENCY, [MANIFEST]: manifest });

        await expect(actions.updateProjectDependencies(tree, GRAPH, { dependency: '2.0.0' })).rejects.toThrow('dependency>=1.0,<2: excludes the released 2.0.0 once each lower bound moves to it');
        expect(tree.read(MANIFEST, 'utf-8')).toBe(manifest);
    });
});
