import { stderr, stdout } from 'node:process';
import { pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext, Task, TaskGraph } from '@nx/devkit';
import type { TaskResult } from '@nx/devkit/internal';
import { Array, Effect, Equal, FileSystem, HashSet, Option, Path, Record, Schema, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import { run } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Planned = Pick<Task, 'id' | 'target'>;
type Judged = (run: { readonly messages: readonly unknown[]; readonly compiled: boolean; readonly crate: Option.Option<string> }) => { readonly failed: boolean; readonly proven: boolean; readonly said: string };

// --- [MODELS] --------------------------------------------------------------------------

const Command = Schema.Struct({
    args: Schema.NonEmptyArray(Schema.String).annotate({ description: 'Subcommand and its arguments, a JSON message format among them where each task is decided from the messages of its own package' }),
    trailing: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Arguments after --' }),
    env: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)).annotate({ description: "Environment variables the command runs with, over the caller's" }),
    packages: Schema.optionalKey(Schema.Array(Schema.Struct({ root: Schema.String, name: Schema.String }))).annotate({ description: 'Further packages the command selects' }),
});
const Cargo = Schema.Struct({
    cwd: Schema.String.annotate({ description: 'Cargo workspace root, relative to the Nx workspace root' }),
    commands: Schema.NonEmptyArray(Command).annotate({ description: 'Cargo invocations in order, a task stopping at its first failure' }),
    package: Schema.optionalKey(Schema.String).annotate({ description: 'Package the task selects, passed to cargo as a package ID spec' }),
    args: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Arguments appended to every command' }),
    outputs: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: "Paths the commands write, which the target's outputs name through {options.outputs.<n>}, each parent folder created before the run" }),
}).annotate({
    title: 'Cargo',
    description: 'Runs cargo commands from a workspace root, one process per command for every selected package, each task decided by the compiler messages of its own package, or by the exit status where the command prints none',
});
const Diagnostic = Schema.Struct({ reason: Schema.Literal('compiler-message'), manifest_path: Schema.String, message: Schema.Struct({ level: Schema.String, rendered: Schema.OptionFromNullOr(Schema.String) }) });
const Artifact = Schema.Struct({ reason: Schema.Literal('compiler-artifact'), manifest_path: Schema.String, target: Schema.Struct({ kind: Schema.Array(Schema.String) }) });
const Message = Schema.fromJsonString(Schema.Union([Diagnostic, Artifact, Schema.Record(Schema.String, Schema.Unknown)]));

type Command = typeof Command.Type;

// --- [OPERATIONS] ----------------------------------------------------------------------

const ran = Effect.fnUntraced(
    function* (cwd: string, args: readonly string[], env: Readonly<Record<string, string>> = {}) {
        const handle = yield* ChildProcess.make('cargo', args, { cwd, env, extendEnv: true, detached: false, stdin: 'inherit' });
        const [lines, diagnosed, code] = yield* Effect.all(
            [
                Stream.splitLines(Stream.decodeText(handle.stdout)).pipe(
                    Stream.mapAccum(HashSet.empty<string>, (seen, line) => {
                        const message = Schema.decodeOption(Message)(line);
                        const fresh = Option.filter(
                            Option.flatMap(message, (parsed) => (Schema.is(Diagnostic)(parsed) ? parsed.message.rendered : Option.none())),
                            (rendered) => !HashSet.has(seen, rendered),
                        );
                        return [Option.match(fresh, { onNone: () => seen, onSome: (rendered) => HashSet.add(seen, rendered) }), [{ text: Option.isSome(message) ? fresh : Option.some(`${line}\n`), message }]];
                    }),
                    Stream.tap(({ text }) => Option.match(text, { onNone: () => Effect.void, onSome: (rendered) => Effect.sync(() => stdout.write(rendered)) })),
                    Stream.runCollect,
                ),
                Stream.mkString(Stream.tap(Stream.decodeText(handle.stderr), (chunk) => Effect.sync(() => stderr.write(chunk)))),
                handle.exitCode,
            ],
            { concurrency: 'unbounded' },
        );
        return { passed: code === 0, log: [...Array.getSomes(lines.map(({ text }) => text)), diagnosed].join(''), messages: Array.getSomes(lines.map(({ message }) => message)) };
    },
    Effect.scoped,
    Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed({ passed: false, log: 'cargo is not on PATH, so no Cargo task can run\n', messages: Array.empty<typeof Message.Type>() })),
);

const decided = Effect.fnUntraced(function* (judged: Judged, context: ExecutorContext, inputs: Readonly<Record<string, typeof Cargo.Type>>, command: Command, tasks: Array.NonEmptyReadonlyArray<Planned>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { nodes, dependencies } = context.projectGraph;
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
    const completed = yield* ran(
        path.resolve(context.root, options?.cwd ?? '.'),
        [
            ...command.args,
            ...tasks.flatMap(({ id, target: { project } }) => Option.toArray(Option.fromUndefinedOr(inputs[id]?.package)).flatMap((name) => selected(root(project), name))),
            ...(command.packages ?? []).flatMap(({ root: at, name }) => selected(at, name)),
            ...(command.trailing === undefined ? [] : ['--', ...command.trailing]),
            ...(options?.args ?? []),
        ],
        command.env,
    );
    const diagnostics = completed.messages.filter(Schema.is(Diagnostic)).map((diagnostic) => ({ ...diagnostic, root: path.dirname(diagnostic.manifest_path) }));
    const errored = new Set(diagnostics.filter(({ message: { level } }) => level.startsWith('error')).map(({ root: at }) => at));
    const built = new Set(
        completed.messages
            .filter(Schema.is(Artifact))
            .filter(({ target: { kind } }) => !kind.includes('custom-build'))
            .map(({ manifest_path }) => path.dirname(manifest_path)),
    );
    const broken = new Set(Record.keys(Record.filter(nodes, (_, project) => errored.has(root(project)))));
    const reaches = (project: string, seen: ReadonlySet<string>): boolean => (dependencies[project] ?? []).some(({ target }) => !seen.has(target) && (broken.has(target) || reaches(target, new Set([...seen, target]))));
    const results = tasks.map(({ id, target: { project } }) => {
        const at = root(project);
        const { failed, proven, said } = judged({ messages: completed.messages, compiled: built.size + diagnostics.length === 0 ? completed.passed : built.has(at), crate: Option.fromUndefinedOr(inputs[id]?.package) });
        const rendered = Array.getSomes(diagnostics.filter(({ root: crate }) => crate === at).map(({ message }) => message.rendered));
        return { id, own: errored.has(at) || failed, passed: proven && !reaches(project, new Set([project])), terminalOutput: [...rendered, said].join('') };
    });
    const attributed = results.some(({ own }) => own);
    const unproven: TaskResult = attributed ? { success: false, status: 'skipped', terminalOutput: '' } : { success: false, terminalOutput: completed.log };
    return Record.fromIterableWith(results, ({ id, own, passed, terminalOutput }): readonly [string, TaskResult] => [id, own || passed ? { success: !own, terminalOutput } : unproven]);
});

const scheduled = (judged: Judged, context: ExecutorContext, inputs: Readonly<Record<string, typeof Cargo.Type>>, tasks: readonly Planned[]) => {
    const keyed = ({ id }: Pick<Task, 'id'>) => [inputs[id]?.cwd, inputs[id]?.commands ?? [], inputs[id]?.args] as const;
    const steps = Array.dedupe(tasks.map(keyed)).flatMap((key) => key[1].map((command, index) => ({ group: tasks.filter((task) => Equal.equals(keyed(task), key)), command, last: index === key[1].length - 1 })));
    return Stream.fromIterable(steps).pipe(
        Stream.mapAccumEffect(
            (): Readonly<Record<string, TaskResult>> => ({}),
            Effect.fnUntraced(function* (finished, { group, command, last }) {
                const ready = group.filter(({ id }) => finished[id]?.success !== false);
                const results = Array.isReadonlyArrayNonEmpty(ready) ? yield* decided(judged, context, inputs, command, ready) : {};
                const settled = Record.filter(results, ({ success }) => last || !success);
                return [{ ...finished, ...results }, Record.collect(settled, (task, result) => ({ task, result }))] as const;
            }),
        ),
    );
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const decides = (judged: Judged) => ({
    execute: Effect.fnUntraced(function* (cargo: typeof Cargo.Type, context: ExecutorContext) {
        const [project, target] = yield* Effect.all([Effect.fromNullishOr(context.projectName), Effect.fromNullishOr(context.targetName)]);
        const id = `${project}:${target}`;
        const results = yield* Stream.runCollect(scheduled(judged, context, { [id]: cargo }, [{ id, target: { project, target } }]));
        return { success: results.every(({ result }) => result.success) };
    }, run),
    batch: (taskGraph: TaskGraph, inputs: Readonly<Record<string, typeof Cargo.Type>>, _: unknown, context: ExecutorContext) => Stream.toReadableStream(Stream.provide(scheduled(judged, context, inputs, Object.values(taskGraph.tasks)), layer)).values(),
});

const { execute, batch } = decides(({ compiled }) => ({ failed: false, proven: compiled, said: '' }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { batch, Cargo, type Command, decides, execute };
