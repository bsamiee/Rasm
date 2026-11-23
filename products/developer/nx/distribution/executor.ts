import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { createTaskId } from '@nx/devkit/internal';
import { Array, Cause, Config, Effect, Option, Record, Schema, type Scope, String, Struct } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import { minimatch } from 'minimatch';
import { createNxReleaseConfig } from 'nx/src/command-line/release/config/config';
import { getLatestGitTagForPattern, sanitizeProjectNameForGitTag } from 'nx/src/command-line/release/utils/git';
import { getFilesForOutputsBatch } from 'nx/src/native';
import { createProjectFileMapUsingProjectGraph } from 'nx/src/project-graph/file-map-utils';
import { createTaskGraph } from 'nx/src/tasks-runner/create-task-graph';
import { spawned } from '#/inference';

// --- [MODELS] --------------------------------------------------------------------------

const Dry = Schema.Struct({ dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each change and makes none, as NX_DRY_RUN=true does' }) });
const Upstream = Schema.Struct({ dependentTasksOutputFiles: Schema.String, transitive: Schema.optionalKey(Schema.Boolean) }).annotate({ description: 'A dependentTasksOutputFiles input of a target, matching the output files of its dependency tasks, transitive ones included where transitive is true' });
const Words = Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Words after -- of the run, appended after the configured arguments', $default: { $source: 'unparsed' }, hidden: true, 'x-priority': 'internal' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const dried = ({ dryRun }: typeof Dry.Type) => Effect.map(Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false)), (stated) => dryRun === true || stated);

const forwarded = (words: readonly string[]) => {
    const owned = new RegExp(
        `^--(?:no-)?(?:${Struct.keys(Dry.fields)
            .flatMap((key) => [key, String.snakeToKebab(String.camelToSnake(key))].map(RegExp.escape))
            .join('|')})(?:=.*)?$`,
        'u',
    );
    return words.filter((word) => !owned.test(word));
};

const credentialed = (variables: string | readonly string[]) => Effect.map(Config.all(Array.ensure(variables).map((variable) => Config.option(Config.String(variable)))), Array.findFirst(Option.filter(String.isNonEmpty)));

const packaged = Effect.fnUntraced(function* ({ root, projectName, targetName, configurationName, projectGraph }: ExecutorContext) {
    const [project, target] = yield* Effect.all([Effect.fromNullishOr(projectName), Effect.fromNullishOr(targetName)]);
    const { tasks, dependencies } = yield* Effect.try({ try: () => createTaskGraph(projectGraph, {}, [project], [target], configurationName, {}), catch: (cause) => new Cause.UnknownError(cause, `${project}:${target} builds no task graph`) });
    const own = createTaskId(project, target, configurationName);
    const declared = Array.filterMap(projectGraph.nodes[project]?.data.targets?.[target]?.inputs ?? [], (input) => Schema.decodeUnknownResult(Upstream)(input));
    const upstream = (ids: ReadonlySet<string>, transitive: boolean): ReadonlySet<string> => {
        const next = new Set([...ids, ...[...ids].filter((id) => transitive || id === own).flatMap((id) => (dependencies[id] ?? []).map((edge) => edge.id))]);
        return next.size === ids.size ? ids : upstream(next, transitive);
    };
    const matched = declared.map(({ dependentTasksOutputFiles, transitive = false }) => {
        const outputs = [...upstream(new Set([own]), transitive)].filter((id) => id !== own).map((id) => tasks[id]?.outputs ?? []);
        return {
            outputs: outputs.flat(),
            files: getFilesForOutputsBatch(root, outputs)
                .flat()
                .filter((file) => minimatch(file, dependentTasksOutputFiles, { dot: true })),
        };
    });
    const [outputs, files] = [Array.dedupe(matched.flatMap(Struct.get('outputs'))), Array.dedupe(matched.flatMap(Struct.get('files'))).toSorted()];
    return yield* outputs.length > 0 && files.length === 0
        ? Effect.fail(new Cause.UnknownError(declared, `${project} holds no file its dependentTasksOutputFiles input of ${target} matches under [${outputs.join(', ')}], the outputs of the tasks ${target} depends on; run them first`))
        : Effect.succeed({ project, target, outputs, files });
});

const released = Effect.fnUntraced(function* ({ root, projectName, projectGraph, nxJsonConfiguration }: ExecutorContext) {
    const project = yield* Effect.fromNullishOr(projectName);
    const files = yield* Effect.promise(() => createProjectFileMapUsingProjectGraph(projectGraph));
    const { error, nxReleaseConfig } = yield* Effect.promise(() => createNxReleaseConfig(projectGraph, files, nxJsonConfiguration.release));
    const groups = yield* error === null ? Effect.succeed(Record.toEntries(nxReleaseConfig?.groups ?? {})) : Effect.fail(new Cause.UnknownError(error, `nx.json release refuses its configuration with ${error.code}`));
    const tags = (yield* spawned(['git', 'tag', '--points-at', 'HEAD'], { cwd: root, stdout: 'pipe' })).split('\n').filter(String.isNonEmpty);
    const found = (name: string, releaseTag: (typeof groups)[number][1]['releaseTag']) => Effect.promise(() => getLatestGitTagForPattern(releaseTag.pattern, { projectName: sanitizeProjectNameForGitTag(project), releaseGroupName: name }, () => Promise.resolve(tags), releaseTag));
    return yield* Option.match(
        Array.findFirst(groups, ([, { projects }]) => projects.includes(project)),
        { onNone: () => Effect.succeed(Option.none<string>()), onSome: ([name, { releaseTag }]) => Effect.map(found(name, releaseTag), (held) => Option.map(Option.fromNullishOr(held), ({ tag }) => tag)) },
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices | ChildProcessSpawner.ChildProcessSpawner | Scope.Scope>) => effect.pipe(Effect.scoped, Effect.provide(layer), Effect.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { credentialed, Dry, dried, forwarded, packaged, released, run, Words };
