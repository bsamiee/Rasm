import { assert, describe, expect, it } from '@effect/vitest';
import type { ProjectGraph, Tree } from '@nx/devkit';
import { createTreeWithEmptyWorkspace } from '@nx/devkit/testing';
import UvVersionActions from '@rasm/nx-uv/release/version-actions';
import { Arbitrary, Effect, Option, Schema, Struct } from 'effect';

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

// --- [MODELS] --------------------------------------------------------------------------

const Release = Schema.Struct({
    release: Schema.Tuple([Schema.Natural, Schema.Natural, Schema.Natural]),
    pre: Schema.UndefinedOr(Schema.Tuple([Schema.Literals(Struct.keys(SPELLED)), Schema.Natural])),
});
const RELEASES = Arbitrary.schema(Release).pipe(
    Arbitrary.map(({ release, pre }) => ({
        semver: [release.join('.'), ...Option.toArray(Option.map(Option.fromUndefinedOr(pre), (held) => held.join('.')))].join('-'),
        pep440: [release.join('.'), ...Option.toArray(Option.map(Option.fromUndefinedOr(pre), ([id, number]) => `${SPELLED[id]}${number}`))].join(''),
    })),
);

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
    it.effect.prop(
        'writes any SemVer release or alpha, beta, or rc prerelease in its PEP 440 spelling into the version field alone and reads the same version back',
        { version: RELEASES, ending: Schema.Literals(['\n', '\r\n']) },
        Effect.fnUntraced(function* ({ version: { semver, pep440 }, ending }) {
            const before = ['# manifest', '[project]', 'name    = "my-lib"', 'version = "1.0.0" # moved by nx release', 'dependencies = ["requests>=2.31"]', ''].join(ending);
            const { tree, actions } = yield* Effect.promise(() => versioned({ [MANIFEST]: before }));

            yield* Effect.promise(() => actions.updateProjectVersion(tree, semver));

            assert.deepStrictEqual([tree.read(MANIFEST, 'utf-8'), yield* Effect.promise(() => actions.readCurrentVersionFromSourceManifest(tree))], [before.replace('"1.0.0"', `"${pep440}"`), { currentVersion: semver, manifestPath: MANIFEST }]);
        }),
    );

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
