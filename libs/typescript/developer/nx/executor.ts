import { createProjectFileMapUsingProjectGraph, type ExecutorContext } from '@nx/devkit';
import { createTaskId } from '@nx/devkit/internal';
import { Array, Cause, Config, Effect, Function, Option, Record, Schema, String, Struct } from 'effect';
import { minimatch } from 'minimatch';
import { createNxReleaseConfig } from 'nx/src/command-line/release/config/config';
import { getLatestGitTagForPattern, sanitizeProjectNameForGitTag } from 'nx/src/command-line/release/utils/git';
import { RepoGitTags } from 'nx/src/command-line/release/utils/repository-git-tags';
import { getFilesForOutputsBatch } from 'nx/src/native';
import { createTaskGraph } from 'nx/src/tasks-runner/create-task-graph';
import { spawned } from '#/inference';

// --- [MODELS] --------------------------------------------------------------------------

const Dry = Schema.Struct({ dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each change and makes none, as NX_DRY_RUN=true does' }) });
const Upstream = Schema.Struct({ dependentTasksOutputFiles: Schema.String, transitive: Schema.optionalKey(Schema.Boolean) }).annotate({ description: 'A dependentTasksOutputFiles input of a target, matching the output files of its dependency tasks, transitive ones included where transitive is true' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const forwarded = (words: readonly string[]) => {
    const owned = new RegExp(`^--(?:no-)?(?:${Array.flatMap(Struct.keys(Dry.fields), (key) => [key, String.kebabCase(key)].map(RegExp.escape)).join('|')})(?:=.*)?$`, 'u');
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
    return outputs.length > 0 && files.length === 0 ? yield* new Cause.UnknownError(declared, `${project} holds no file its dependentTasksOutputFiles input of ${target} matches under [${outputs.join(', ')}], the outputs of the tasks ${target} depends on; run them first`) : { project, target, outputs, files };
});

const released = Effect.fnUntraced(function* ({ root, projectName, projectGraph, nxJsonConfiguration }: ExecutorContext) {
    const project = yield* Effect.fromNullishOr(projectName);
    const files = yield* Effect.promise(() => createProjectFileMapUsingProjectGraph(projectGraph));
    const { error, nxReleaseConfig } = yield* Effect.promise(() => createNxReleaseConfig(projectGraph, files, nxJsonConfiguration.release));
    const group = error === null ? Array.findFirst(Record.toEntries(nxReleaseConfig?.groups ?? {}), ([, { projects }]) => projects.includes(project)) : yield* new Cause.UnknownError(error, `nx.json release refuses its configuration with ${error.code}`);
    const [head, ordered] = yield* Effect.all([spawned(['git', 'tag', '--points-at', 'HEAD'], { cwd: root }), Effect.promise(() => RepoGitTags.create().resolveTags())]);
    const tags = Array.intersection(ordered, head.split('\n'));
    const held = yield* Effect.transposeOption(Option.map(group, ([name, { releaseTag }]) => Effect.promise(() => getLatestGitTagForPattern(releaseTag.pattern, { projectName: sanitizeProjectNameForGitTag(project), releaseGroupName: name }, Function.constant(Promise.resolve(tags)), releaseTag))));
    return Option.flatMapNullishOr(held, (found) => found?.tag);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { credentialed, Dry, forwarded, packaged, released };
