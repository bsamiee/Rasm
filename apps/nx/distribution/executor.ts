import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { createTaskId } from '@nx/devkit/internal';
import { Array, Config, Effect, Formatter, Match, Option, Schema, type Scope, Stream, String } from 'effect';
import { ChildProcess, type ChildProcessSpawner, CommandLine } from 'effect/process';
import { getFilesForOutputsBatch } from 'nx/src/native/index';

// --- [MODELS] --------------------------------------------------------------------------

const Dry = Schema.Struct({ dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each change and makes none, as NX_DRY_RUN=true does' }) });
const Words = Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Words after -- of the run, appended after the configured arguments', $default: { $source: 'unparsed' }, hidden: true, 'x-priority': 'internal' });

// --- [ERRORS] --------------------------------------------------------------------------

class DistributionError extends Schema.TaggedError<DistributionError, { readonly brand: unique symbol }>()('DistributionError', {
    reason: Schema.TaggedUnion({ Exited: { code: Schema.Int }, Refused: { detail: Schema.String } }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
}) {
    static readonly of = (reason: DistributionError['reason'], subject: string) => (cause?: unknown) => DistributionError.make({ reason, subject, ...(cause === undefined ? {} : { cause }) });

    static readonly refused = (subject: string, detail: string) => DistributionError.of({ _tag: 'Refused', detail }, subject);

    override get message() {
        const stated = Match.valueTags(this.reason, { Exited: ({ code }) => `${this.subject} exits ${code}`, Refused: ({ detail }) => `${this.subject}: ${detail}` });
        return [stated, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ');
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const dried = ({ dryRun }: typeof Dry.Type) => Effect.map(Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false)), (stated) => dryRun === true || stated);

const credentialed = (variable: string) => Effect.map(Config.option(Config.String(variable)), Option.filter(String.isNonEmpty));

const spawned = Effect.fnUntraced(
    function* (argv: Array.NonEmptyReadonlyArray<string>, options: ChildProcess.CommandOptions = {}) {
        const handle = yield* ChildProcess.make(Array.headNonEmpty(argv), Array.tailNonEmpty(argv), { stdin: 'ignore', stdout: 'inherit', stderr: 'inherit', ...options, extendEnv: true, detached: false });
        const [printed, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), handle.exitCode], { concurrency: 'unbounded' });
        return code === 0 ? printed : yield* DistributionError.of({ _tag: 'Exited', code }, CommandLine.display(argv))();
    },
    Effect.scoped,
    (effect, ...[argv]: readonly [Array.NonEmptyReadonlyArray<string>, ChildProcess.CommandOptions?]) => Effect.catchTag(effect, 'PlatformError', (cause) => Effect.fail(DistributionError.refused(Array.headNonEmpty(argv), 'does not start')(cause))),
);

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

const packaged = Effect.fnUntraced(function* (context: ExecutorContext) {
    const [project, target] = yield* Effect.all([Effect.fromNullishOr(context.projectName), Effect.fromNullishOr(context.targetName)]);
    const task = createTaskId(project, target, context.configurationName);
    const own = yield* Effect.fromNullishOr(context.taskGraph?.tasks[task]);
    const [{ getTaskRawInputs }, { getInputs }] = yield* Effect.promise(() => Promise.all([import('nx/src/hasher/check-task-files'), import('nx/src/hasher/task-hasher')]));
    const read = getInputs(own, context.projectGraph, context.nxJsonConfiguration).depsOutputs;
    const files = yield* Effect.tryPromise({ try: () => getTaskRawInputs(task, { projectGraph: context.projectGraph, nxJson: context.nxJsonConfiguration }), catch: DistributionError.refused(task, 'has no hash plan to read its dependent task output files from') }).pipe(Effect.map((inputs) => inputs?.depOutputs ?? []));
    return yield* read.length > 0 && files.length === 0 ? DistributionError.refused(project, `holds no file its dependentTasksOutputFiles input of ${target} matches; run the targets ${target} depends on first`)() : Effect.succeed(files.toSorted());
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices | ChildProcessSpawner.ChildProcessSpawner | Scope.Scope>) => effect.pipe(Effect.tapCause(Effect.logError), Effect.scoped, Effect.provide(layer), Effect.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { credentialed, DistributionError, Dry, dried, packaged, run, spawned, Words, written };
