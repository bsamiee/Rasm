import { stderr, stdout } from 'node:process';
import { pathToFileURL } from 'node:url';
import type { ExecutorContext, Task } from '@nx/devkit';
import { findAllProjectNodeDependencies, type TaskResult } from '@nx/devkit/internal';
import { batched, executor, Words } from '@rasm/nx-distribution/inference';
import { Array, Clock, Effect, Equal, FileSystem, HashSet, Option, Path, Record, Schema, Stream, Struct } from 'effect';
import { ChildProcess } from 'effect/process';
import { Crate, Unification } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Planned = Pick<Task, 'id' | 'target'>;
type Judged = (run: { readonly messages: readonly unknown[]; readonly compiled: boolean; readonly crate: Option.Option<string> }) => { readonly failed: boolean; readonly proven: boolean; readonly said: string };

// --- [MODELS] --------------------------------------------------------------------------

class Command extends Schema.Opaque<Command, unknown>()(
    Schema.Struct({
        args: Schema.NonEmptyArray(Schema.String).annotate({ description: 'Subcommand and its arguments, a JSON message format among them where each task is decided from the messages of its own package' }),
        trailing: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Arguments after --' }),
        env: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)).annotate({ description: "Environment variables the command runs with, over the caller's" }),
        packages: Schema.optionalKey(Schema.Array(Crate)).annotate({ description: 'Further packages the command selects' }),
    }),
) {}
class Cargo extends Schema.Opaque<Cargo, unknown>()(
    Schema.Struct({
        cwd: Schema.String.annotate({ description: 'Cargo workspace root, relative to the Nx workspace root' }),
        commands: Schema.NonEmptyArray(Command).annotate({ description: 'Cargo invocations in order, a task stopping at its first failure' }),
        package: Schema.optionalKey(Schema.String).annotate({ description: 'Package the task selects, passed to cargo as a package ID spec' }),
        unification: Unification.annotate({ description: 'Cargo resolver.feature-unification of the workspace, a batch running each task of selected, which unifies features over every package one invocation selects, in a cargo process of its own' }),
        __unparsed__: Words,
        outputs: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: "Paths the commands write, which the target's outputs name through {options.outputs.<n>}, each parent folder created before the run" }),
    }).annotate({
        title: 'Cargo',
        description: 'Runs cargo commands from a workspace root, one process per command for every selected package, each task decided by the compiler messages of its own package, or by the exit status where the command prints none',
    }),
) {}
const Diagnostic = Schema.Struct({ reason: Schema.Literal('compiler-message'), manifest_path: Schema.String, message: Schema.Struct({ level: Schema.String, rendered: Schema.OptionFromNullOr(Schema.String) }) });
const Artifact = Schema.Struct({ reason: Schema.Literal('compiler-artifact'), manifest_path: Schema.String, target: Schema.Struct({ kind: Schema.Array(Schema.String) }) });
const Finished = Schema.Struct({ reason: Schema.Literal('build-finished'), success: Schema.Boolean });
const Message = Schema.fromJsonString(Schema.Union([Diagnostic, Artifact, Schema.Record(Schema.String, Schema.Unknown)]));
const Compiled = Schema.Union([Diagnostic, Artifact]);
const Suited = Schema.Struct({ nextest: Schema.Struct({ crate: Schema.String }) }).annotate({ description: 'A nextest suite or test event, naming the crate of its test binary' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const ran = Effect.fnUntraced(
    function* (cwd: string, args: readonly string[], env: Readonly<Record<string, string>> = {}) {
        const handle = yield* ChildProcess.make('cargo', args, { cwd, env, extendEnv: true, detached: false, stdin: 'inherit' });
        const [lines, code] = yield* Stream.make([handle.stdout, stdout] as const, [handle.stderr, stderr] as const).pipe(
            Stream.flatMap(([bytes, sink]) => Stream.map(Stream.splitLines(Stream.decodeText(bytes)), (line) => ({ line, sink })), { concurrency: 'unbounded' }),
            Stream.mapEffect((read) => Effect.map(Clock.currentTimeMillis, (at) => ({ ...read, at, message: Schema.decodeOption(Message)(read.line) }))),
            Stream.map((read) => ({ ...read, rendered: Option.flatMap(Option.filter(read.message, Schema.is(Diagnostic)), (diagnostic) => diagnostic.message.rendered) })),
            Stream.mapAccum(HashSet.empty<string>, (seen, { rendered, line, ...read }) => [Option.reduceCompact([rendered], seen, (kept, held) => HashSet.add(kept, held)), [{ ...read, text: Option.isSome(read.message) ? Option.filter(rendered, (held) => !HashSet.has(seen, held)) : Option.some(`${line}\n`) }]]),
            Stream.tap(({ text, sink }) => Option.match(text, { onNone: () => Effect.void, onSome: (rendered) => Effect.sync(() => sink.write(rendered)) })),
            Stream.runCollect,
            Effect.zip(handle.exitCode, { concurrent: true }),
        );
        return { passed: code === 0, log: Array.getSomes(lines.map(({ text }) => text)).join(''), stamped: Array.getSomes(lines.map(({ message, at }) => Option.map(message, (held) => ({ message: held, at })))) };
    },
    Effect.scoped,
    Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed({ passed: false, log: 'cargo is not on PATH, so no Cargo task can run\n', stamped: [] })),
);

const decided = Effect.fnUntraced(function* (judged: Judged, context: ExecutorContext, inputs: Readonly<Record<string, Cargo>>, command: Command, tasks: Array.NonEmptyReadonlyArray<Planned>, earlier: Readonly<Record<string, TaskResult>>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { nodes } = context.projectGraph;
    const root = (project: string) => path.resolve(context.root, nodes[project]?.data.root ?? '.');
    const selected = (at: string, name: string) => ['--package', `${pathToFileURL(path.resolve(context.root, at)).href}#${name}`];
    const options = inputs[tasks[0].id];
    yield* Effect.orDie(
        Effect.forEach(
            tasks.flatMap(({ id }) => inputs[id]?.outputs ?? []),
            (file) => fs.makeDirectory(path.dirname(path.resolve(context.root, file)), { recursive: true }),
            { discard: true },
        ),
    );
    const [startTime, completed, endTime] = yield* Effect.all([
        Clock.currentTimeMillis,
        ran(
            path.resolve(context.root, options?.cwd ?? '.'),
            [
                ...command.args,
                ...tasks.flatMap(({ id, target: { project } }) => Option.toArray(Option.fromUndefinedOr(inputs[id]?.package)).flatMap((name) => selected(root(project), name))),
                ...(command.packages ?? []).flatMap(({ root: at, name }) => selected(at, name)),
                ...(command.trailing === undefined ? [] : ['--', ...command.trailing]),
                ...(options?.__unparsed__ ?? []),
            ],
            command.env,
        ),
        Clock.currentTimeMillis,
    ]);
    const messages = completed.stamped.map(Struct.get('message'));
    const diagnostics = Array.filter(messages, Schema.is(Diagnostic)).map((diagnostic) => ({ ...diagnostic, root: path.dirname(diagnostic.manifest_path) }));
    const errored = new Set(diagnostics.filter(({ message: { level } }) => level.startsWith('error')).map(({ root: at }) => at));
    const built = new Set(
        Array.filter(messages, Schema.is(Artifact))
            .filter(({ target: { kind } }) => !kind.includes('custom-build'))
            .map(({ manifest_path }) => path.dirname(manifest_path)),
    );
    const broken = new Set(Record.keys(Record.filter(nodes, (_, project) => errored.has(root(project)))));
    const halted = Array.filter(messages, Schema.is(Finished)).some(({ success }) => !success);
    const results = tasks.map(({ id, target: { project } }) => {
        const [at, crate] = [root(project), Option.fromUndefinedOr(inputs[id]?.package)];
        const { failed, proven, said } = judged({ messages, compiled: completed.passed || (halted && built.has(at)), crate });
        const rendered = Array.getSomes(diagnostics.filter(({ root: held }) => held === at).map(({ message }) => message.rendered));
        const last = Array.findLast(completed.stamped, ({ message }) => (Schema.is(Compiled)(message) && path.dirname(message.manifest_path) === at) || (Schema.is(Suited)(message) && Option.contains(crate, message.nextest.crate)));
        const timed = { startTime: earlier[id]?.startTime ?? startTime, endTime: Option.getOrElse(Option.map(last, Struct.get('at')), () => endTime) };
        return { id, own: errored.has(at) || failed, passed: proven && !findAllProjectNodeDependencies(project, context.projectGraph).some((name) => broken.has(name)), terminalOutput: [...rendered, said].join(''), timed };
    });
    const attributed = results.some(({ own }) => own);
    const unproven: TaskResult = attributed ? { success: false, status: 'skipped', terminalOutput: '' } : { success: false, terminalOutput: completed.log };
    return Record.fromIterableWith(results, ({ id, own, passed, terminalOutput, timed }): readonly [string, TaskResult] => [id, own || passed ? { success: !own, terminalOutput, ...timed } : { ...unproven, ...(attributed ? {} : timed) }]);
}, Effect.scoped);

const scheduled = (judged: Judged, context: ExecutorContext, inputs: Readonly<Record<string, Cargo>>, tasks: readonly Planned[]) => {
    const keyed = ({ id }: Pick<Task, 'id'>) => [inputs[id]?.cwd, inputs[id]?.commands ?? [], inputs[id]?.__unparsed__, inputs[id]?.unification === 'selected' ? id : undefined] as const;
    const steps = Array.dedupe(tasks.map(keyed)).flatMap((key) => key[1].map((command, index) => ({ group: tasks.filter((task) => Equal.equals(keyed(task), key)), command, last: index === key[1].length - 1 })));
    return Stream.fromIterable(steps).pipe(
        Stream.mapAccumEffect(
            (): Readonly<Record<string, TaskResult>> => ({}),
            Effect.fnUntraced(function* (finished, { group, command, last }) {
                const ready = group.filter(({ id }) => finished[id]?.success !== false);
                const results = Array.isReadonlyArrayNonEmpty(ready) ? yield* decided(judged, context, inputs, command, ready, finished) : {};
                const settled = Record.filter(results, ({ success }) => last || !success);
                return [{ ...finished, ...results }, Record.collect(settled, (task, result) => ({ task, result }))] as const;
            }),
        ),
    );
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const decides = (judged: Judged) => ({
    execute: executor(Cargo, function* (cargo, context) {
        const [project, target] = yield* Effect.all([Effect.fromNullishOr(context.projectName), Effect.fromNullishOr(context.targetName)]);
        const id = `${project}:${target}`;
        const results = yield* Stream.runCollect(scheduled(judged, context, { [id]: cargo }, [{ id, target: { project, target } }]));
        return { success: results.every(({ result }) => result.success) };
    }),
    batch: batched(Cargo, (taskGraph, inputs, context) => scheduled(judged, context, inputs, Object.values(taskGraph.tasks))),
});

const { execute, batch } = decides(({ compiled }) => ({ failed: false, proven: compiled, said: '' }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { batch, Cargo, type Command, decides, execute, Finished };
