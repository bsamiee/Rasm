import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { createProjectFileMapUsingProjectGraph, workspaceRoot } from '@nx/devkit';
import { Array, Effect, Option, Order, Schema } from 'effect';
import { createNxReleaseConfig } from 'nx/src/command-line/release/config/config';
import { createReleaseGraph } from 'nx/src/command-line/release/utils/release-graph';
import { readNxJson } from 'nx/src/config/nx-json';
import { FsTree } from 'nx/src/generators/tree';
import { nx, read, type Workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCES = {
    config: 'nrwl/nx packages/nx/src/command-line/release/config/config.spec.ts createNxReleaseConfig: a valid release configuration resolves with error null',
    selected: "README.md PRINCIPLES: Plugins tag a project they give release configuration `release`, which `nx.json` `release.projects` selects beside Nx's `npm:public`",
    graph: 'nrwl/nx packages/nx/src/command-line/release/utils/release-graph.spec.ts createReleaseGraph: resolves version actions for every project of every release group',
    version: 'nrwl/nx packages/nx/src/command-line/release/version/resolve-current-version.spec.ts: the disk resolver reads the current version from the source manifest',
    delivered: 'README.md DELIVERY: Every ecosystem and host is delivered end to end (install, package, release, publish), and none stays partly built',
    planned: 'README.md TASKS: Workflow `release.yml` runs `nx release --skip-publish` (one version, tag, and GitHub release); nrwl/nx e2e/release/src/first-release.test.ts',
} as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const tagged = ({ nodes }: Workspace) =>
    Array.sort(
        Object.values(nodes)
            .filter(({ data }) => (data.tags ?? []).some((tag) => tag === 'release' || tag === 'npm:public'))
            .map(({ name }) => name),
        Order.String,
    );

const configured = Effect.fnUntraced(function* (workspace: Workspace) {
    const files = yield* Effect.promise(() => createProjectFileMapUsingProjectGraph(workspace));
    return yield* Effect.promise(() => createNxReleaseConfig(workspace, files, readNxJson(workspaceRoot).release));
});

const graphed = Effect.fnUntraced(function* (workspace: Workspace) {
    const { nxReleaseConfig } = yield* configured(workspace);
    const tree = new FsTree(workspaceRoot, false);
    const stated = yield* Effect.fromNullishOr(nxReleaseConfig);
    const graph = yield* Effect.promise(() => createReleaseGraph({ tree, projectGraph: workspace, nxReleaseConfig: stated, filters: {}, firstRelease: true, verbose: false, skipVersionResolution: true }));
    return { tree, graph };
});

const unversioned = Effect.fnUntraced(function* (workspace: Workspace) {
    const { tree, graph } = yield* graphed(workspace);
    const stated = yield* Effect.forEach(tagged(workspace), (project) => Effect.option(Effect.tryPromise(() => Promise.resolve(graph.getVersionActionsForProject(project)?.readCurrentVersionFromSourceManifest(tree)))));
    return Array.zip(tagged(workspace), stated)
        .map(([project, manifest]) => [project, Option.getOrUndefined(manifest)?.currentVersion] as const)
        .filter(([, version]) => Option.isNone(Schema.decodeUnknownOption(Schema.SemVerFromString)(version)));
});

const undelivered = (workspace: Workspace) => tagged(workspace).filter((project) => !['nx-release-publish', 'attach'].some((target) => target in (workspace.nodes[project]?.data.targets ?? {})));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })('release configuration', (suite) => {
    suite.effect(
        'resolves nx.json release without a configuration error',
        Effect.fnUntraced(function* () {
            const { error, nxReleaseConfig } = yield* configured(yield* read('fresh'));
            assert.isNull(error, SOURCES.config);
            assert.isNotNull(nxReleaseConfig, SOURCES.config);
        }),
    );

    suite.effect(
        'places exactly the projects tagged release or npm:public in a release group',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            const { nxReleaseConfig } = yield* configured(workspace);
            const grouped = Object.values(nxReleaseConfig?.groups ?? {}).flatMap(({ projects }) => projects);
            assert.deepStrictEqual(Array.sort(grouped, Order.String), tagged(workspace), SOURCES.selected);
        }),
    );

    suite.effect(
        'resolves version actions for every project of a release group',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            const { graph } = yield* graphed(workspace);
            assert.deepStrictEqual(
                tagged(workspace).filter((project) => graph.getVersionActionsForProject(project) === undefined),
                [],
                SOURCES.graph,
            );
        }),
    );

    suite.effect(
        'reads a version from the source manifest of every project of a release group',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(yield* unversioned(yield* read('fresh')), [], SOURCES.version);
        }),
    );

    suite.effect(
        'gives every project of a release group a target that publishes it or attaches its files',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(undelivered(yield* read('fresh')), [], SOURCES.delivered);
        }),
    );

    suite.effect(
        'plans a first release as a dry run without error',
        Effect.fnUntraced(function* () {
            const { code, stdout, stderr } = yield* nx(['release', '--dry-run', '--skip-publish', '--first-release']);
            assert.strictEqual(Number(code), 0, `${SOURCES.planned}\n${stdout}${stderr}`);
        }),
    );
});
