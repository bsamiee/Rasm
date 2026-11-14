import { platform } from 'node:process';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { createProjectFileMapUsingProjectGraph, workspaceRoot } from '@nx/devkit';
import { Array, Effect, FileSystem, Option, Order, Path, Schema } from 'effect';
import { createNxReleaseConfig } from 'nx/src/command-line/release/config/config';
import { createReleaseGraph } from 'nx/src/command-line/release/utils/release-graph';
import { readNxJson } from 'nx/src/config/nx-json';
import { FsTree } from 'nx/src/generators/tree';
import { declared, nx, read, type Workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCES = {
    config: 'nrwl/nx packages/nx/src/command-line/release/config/config.spec.ts createNxReleaseConfig: a valid release configuration resolves with error null',
    selected: 'README.md PRINCIPLES: Plugins tag a project they give release configuration `release`, the one selector of `nx.json` `release.projects`',
    graph: 'nrwl/nx packages/nx/src/command-line/release/utils/release-graph.spec.ts createReleaseGraph: resolves version actions for every project of every release group',
    version: 'nrwl/nx packages/nx/src/command-line/release/version/resolve-current-version.spec.ts: the disk resolver reads the current version from the source manifest; packages/nx/src/command-line/release/config/config.ts: projectsRelationship defaults to fixed, which gives every project of the group one version',
    delivered:
        'README.md DELIVERY: Every ecosystem and host is delivered end to end (install, package, release, publish), and none stays partly built; PRINCIPLES: Plugins present a target that runs a host or needs a platform only on a machine holding it, and tag a project a platform limits `os:<platform>`; Unreal targets exist where the engine a `.uproject` names resolves',
    destination: "README.md DELIVERY: Publishing delivers through each destination's own command (`pnpm publish`, `cargo publish`, `dotnet nuget push`, `uv publish`, `yak push`)",
    defaulted:
        'README.md PRINCIPLES: `nx.json` `targetDefaults` holds one row, keyed `@nx/js:release-publish`; nrwl/nx packages/nx/src/project-graph/utils/project-configuration/target-defaults.ts: a default keyed by an executor reaches the targets running that executor, and Nx records `nx/core/package-json` as the plugin of the target it infers for a public package',
    planned:
        'README.md TASKS: Workflow `release.yml` runs `nx release --skip-publish` (one version, tag, and GitHub release); nrwl/nx e2e/release/src/first-release.test.ts nx release first run with --first-release: should not error; packages/nx/src/command-line/release/config/config.ts defaultFixedReleaseTagPattern: `v{version}`',
} as const;
const TAGGED = /^git tag --annotate (?<tag>\S+)/gmu;
const WRITTEN = /^(?<project>\S+) .*New version (?<version>\S+) written to /gmu;
const COMMAND = /`(?<command>[^`]+)`/gu;

// --- [MODELS] --------------------------------------------------------------------------

const Published = Schema.Struct({ publish: Schema.Array(Schema.String) }).annotate({ description: 'Options of a publish target that states its command: the publish argv' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const captured = (text: string, pattern: RegExp, name: string) => Array.fromIterable(text.matchAll(pattern)).flatMap(({ groups = {} }) => Array.fromNullishOr(groups[name]));

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

const versions = Effect.fnUntraced(function* (workspace: Workspace) {
    const { tree, graph } = yield* graphed(workspace);
    const stated = yield* Effect.forEach(tagged(workspace), (project) => Effect.option(Effect.tryPromise(() => Promise.resolve(graph.getVersionActionsForProject(project)?.readCurrentVersionFromSourceManifest(tree)))));
    return Array.zip(tagged(workspace), stated).map(([project, manifest]) => [project, Option.getOrUndefined(manifest)?.currentVersion] as const);
});

const undelivered = (workspace: Workspace) =>
    tagged(workspace).filter((project) => {
        const { tags = [], targets = {} } = workspace.nodes[project]?.data ?? {};
        const limits = tags.filter((tag) => tag.startsWith('os:'));
        const held = limits.length === 0 || (limits.includes(`os:${platform}`) && Object.keys(targets).length > 0);
        return held && !['nx-release-publish', 'attach'].some((target) => target in targets);
    });

const destinations = Effect.gen(function* () {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const text = yield* fs.readFileString(path.join(workspaceRoot, 'README.md'));
    const stated = yield* Effect.fromOption(Array.findFirst(text.split('\n'), (line) => line.startsWith('- Publishing delivers')));
    return captured(stated, COMMAND, 'command').map((command) => `${command} `);
});

const published = (workspace: Workspace) =>
    Array.getSomes(
        declared(workspace)
            .filter(({ name }) => name === 'nx-release-publish')
            .map(({ task, target }) => Option.map(Schema.decodeUnknownOption(Published)(target.options), ({ publish }) => ({ task, command: `${publish.join(' ')} ` }))),
    );

const foreign = (workspace: Workspace, commands: readonly string[]) => published(workspace).filter(({ command }) => !commands.some((stated) => command.startsWith(stated)));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })((suite) => {
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
        'reads one version from the source manifest of every project of the fixed release group',
        Effect.fnUntraced(function* () {
            const stated = yield* versions(yield* read('fresh'));
            assert.deepStrictEqual(
                stated.filter(([, version]) => Option.isNone(Schema.decodeUnknownOption(Schema.SemVerFromString)(version))),
                [],
                SOURCES.version,
            );
            assert.lengthOf(Array.dedupe(stated.map(([, version]) => version)), 1, SOURCES.version);
        }),
    );

    suite.effect(
        'gives every project of a release group whose host this machine holds a target that publishes it or attaches its files',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(undelivered(yield* read('fresh')), [], SOURCES.delivered);
        }),
    );

    suite.effect(
        'starts every publish command a target states with a command README DELIVERY names for a destination',
        Effect.fnUntraced(function* () {
            const [workspace, commands] = yield* Effect.all([read('fresh'), destinations]);
            assert.isNotEmpty(commands, SOURCES.destination);
            assert.isNotEmpty(published(workspace), SOURCES.destination);
            assert.deepStrictEqual(foreign(workspace, commands), [], SOURCES.destination);
        }),
    );

    suite.effect(
        'runs each publish target Nx core infers for a public package through the nx.json row, and leaves each publish target a plugin infers as that plugin states it',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            const stated = declared(workspace)
                .filter(({ name }) => name === 'nx-release-publish')
                .map(({ task, target, plugin }) => ({ task, executor: `${target.executor}`, plugin: Option.getOrUndefined(plugin), keyed: Schema.is(Published)(target.options) }));
            const core = stated.filter(({ plugin }) => plugin === 'nx/core/package-json');
            assert.isNotEmpty(core, SOURCES.defaulted);
            assert.deepStrictEqual(
                core.filter(({ keyed, executor }) => !keyed || executor === '@nx/js:release-publish'),
                [],
                SOURCES.defaulted,
            );
            assert.deepStrictEqual(
                stated.filter(({ plugin, executor, keyed }) => plugin !== 'nx/core/package-json' && !(executor.startsWith(`${plugin}:`) && (!keyed || executor.endsWith(':publish')))),
                [],
                SOURCES.defaulted,
            );
        }),
    );

    suite.effect(
        'plans a first release of a stated version as a dry run that tags v<version> once and writes that version to the manifest of every project of a release group',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            const { code, stdout, stderr } = yield* nx(['release', '1.0.0', '--skip-publish', '--first-release', '--dry-run', '--verbose']);
            const output = `${stdout}${stderr}`;
            assert.strictEqual(Number(code), 0, `${SOURCES.planned}\n${output}`);
            assert.deepStrictEqual(captured(output, TAGGED, 'tag'), ['v1.0.0'], SOURCES.planned);
            assert.deepStrictEqual(Array.sort(captured(output, WRITTEN, 'project'), Order.String), tagged(workspace), SOURCES.planned);
            assert.deepStrictEqual(Array.dedupe(captured(output, WRITTEN, 'version')), ['1.0.0'], SOURCES.planned);
        }),
    );
});
