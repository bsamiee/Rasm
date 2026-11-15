import { matchesGlob } from 'node:path';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { createTaskId } from '@nx/devkit/internal';
import { Array, Cause, Config, Effect, Option, Schema, type Scope, Stream, String } from 'effect';
import { ChildProcess, type ChildProcessSpawner, CommandLine } from 'effect/process';
import { getFilesForOutputsBatch } from 'nx/src/native/index';

// --- [MODELS] --------------------------------------------------------------------------

const Dry = Schema.Struct({ dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each change and makes none, as NX_DRY_RUN=true does' }) });
const Upstream = Schema.Struct({ dependentTasksOutputFiles: Schema.String, transitive: Schema.optionalKey(Schema.Boolean) }).annotate({ description: 'A dependentTasksOutputFiles input of a target, matching the output files of its dependency tasks, transitive ones included where transitive is true' });
const Words = Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Words after -- of the run, appended after the configured arguments', $default: { $source: 'unparsed' }, hidden: true, 'x-priority': 'internal' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const dried = ({ dryRun }: typeof Dry.Type) => Effect.map(Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false)), (stated) => dryRun === true || stated);

const credentialed = (variable: string) => Effect.map(Config.option(Config.String(variable)), Option.filter(String.isNonEmpty));

const spawned = Effect.fnUntraced(function* (argv: Array.NonEmptyReadonlyArray<string>, options: ChildProcess.CommandOptions = {}) {
    const handle = yield* ChildProcess.make(Array.headNonEmpty(argv), Array.tailNonEmpty(argv), { stdin: 'ignore', ...options, extendEnv: true, detached: false });
    const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? stdout : yield* new Cause.UnknownError(code, [`${CommandLine.display(argv)} exits ${code}`, ...[stderr.trim()].filter(String.isNonEmpty)].join(': '));
}, Effect.scoped);

const written = Effect.fnUntraced(function* ({ root, projectName, targetName, configurationName, taskGraph }: ExecutorContext) {
    const [{ tasks, dependencies }, project, target] = yield* Effect.all([Effect.fromNullishOr(taskGraph), Effect.fromNullishOr(projectName), Effect.fromNullishOr(targetName)]);
    const own = createTaskId(project, target, configurationName);
    const reached = (ids: ReadonlySet<string>): ReadonlySet<string> => {
        const through = [...ids].filter((id) => id === own || tasks[id]?.outputs.length === 0);
        const next = new Set([...ids, ...through.flatMap((id) => (dependencies[id] ?? []).map((edge) => edge.id)).filter((id) => tasks[id]?.target.project === project)]);
        return next.size === ids.size ? ids : reached(next);
    };
    const selected = [...reached(new Set([own]))].filter((id) => id !== own);
    const outputs = selected.map((id) => tasks[id]?.outputs ?? []);
    return { project, target, selected, outputs: outputs.flat(), files: Array.dedupe(getFilesForOutputsBatch(root, outputs).flat()).toSorted() };
});

const packaged = Effect.fnUntraced(function* ({ root, projectName, targetName, configurationName, projectGraph, taskGraph }: ExecutorContext) {
    const [{ tasks, dependencies }, project, target] = yield* Effect.all([Effect.fromNullishOr(taskGraph), Effect.fromNullishOr(projectName), Effect.fromNullishOr(targetName)]);
    const own = createTaskId(project, target, configurationName);
    const declared = Array.filterMap(projectGraph.nodes[project]?.data.targets?.[target]?.inputs ?? [], (input) => Schema.decodeUnknownResult(Upstream)(input));
    const upstream = (ids: ReadonlySet<string>, transitive: boolean): ReadonlySet<string> => {
        const next = new Set([...ids, ...[...ids].filter((id) => transitive || id === own).flatMap((id) => (dependencies[id] ?? []).map((edge) => edge.id))]);
        return next.size === ids.size ? ids : upstream(next, transitive);
    };
    const files = declared.flatMap(({ dependentTasksOutputFiles, transitive = false }) => {
        const ids = [...upstream(new Set([own]), transitive)].filter((id) => id !== own);
        return getFilesForOutputsBatch(
            root,
            ids.map((id) => tasks[id]?.outputs ?? []),
        )
            .flat()
            .filter((file) => matchesGlob(file, dependentTasksOutputFiles));
    });
    return yield* declared.length > 0 && files.length === 0 ? Effect.fail(new Cause.UnknownError(declared, `${project} holds no file its dependentTasksOutputFiles input of ${target} matches; run the targets ${target} depends on first`)) : Effect.succeed(Array.dedupe(files).toSorted());
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices | ChildProcessSpawner.ChildProcessSpawner | Scope.Scope>) => effect.pipe(Effect.scoped, Effect.provide(layer), Effect.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { credentialed, Dry, dried, packaged, run, spawned, Words, written };
