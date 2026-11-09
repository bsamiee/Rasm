import { stderr, stdout } from 'node:process';
import { pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext, Task, TaskGraph } from '@nx/devkit';
import type { TaskResult } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, HashSet, Match, Option, Path, Record, Schema, Stream } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FORMAT = 'json-diagnostic-rendered-ansi';
const REPORTS = {
    messages: { flags: ['--message-format', FORMAT], env: {} },
    tests: { flags: ['--cargo-message-format', FORMAT, '--message-format', 'libtest-json-plus', '--message-format-version', '0.1'], env: { NEXTEST_EXPERIMENTAL_LIBTEST_JSON: '1' } },
    status: { flags: [], env: {} },
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const Command = Schema.Struct({ args: Schema.NonEmptyArray(Schema.String), trailing: Schema.optionalKey(Schema.Array(Schema.String)), report: Schema.Literals(['messages', 'tests', 'status']), packages: Schema.optionalKey(Schema.Array(Schema.Struct({ root: Schema.String, name: Schema.String }))) });
const Cargo = Schema.Struct({ cwd: Schema.String, commands: Schema.NonEmptyArray(Command), package: Schema.optionalKey(Schema.String), args: Schema.optionalKey(Schema.Array(Schema.String)), outputs: Schema.optionalKey(Schema.Array(Schema.String)) });
const Diagnostic = Schema.Struct({ reason: Schema.Literal('compiler-message'), manifest_path: Schema.String, message: Schema.Struct({ level: Schema.String, rendered: Schema.NullOr(Schema.String) }) });
const Artifact = Schema.Struct({ reason: Schema.Literal('compiler-artifact'), manifest_path: Schema.String, target: Schema.Struct({ kind: Schema.Array(Schema.String) }) });
const Finished = Schema.Struct({ reason: Schema.Literal('build-finished'), success: Schema.Boolean });
const Suite = Schema.Struct({ type: Schema.Literal('suite'), event: Schema.Literals(['ok', 'failed']), nextest: Schema.Struct({ crate: Schema.String }) });
const Failed = Schema.Struct({ type: Schema.Literal('test'), event: Schema.Literal('failed'), name: Schema.String, stdout: Schema.optionalKey(Schema.String) });
const Message = Schema.fromJsonString(Schema.Union([Diagnostic, Artifact, Finished, Suite, Failed, Schema.Record(Schema.String, Schema.Unknown)]));

type Command = typeof Command.Type;

// --- [OPERATIONS] ----------------------------------------------------------------------

const ran = (cwd: string, args: readonly string[], env: Readonly<Record<string, string>> = {}) =>
    Effect.scoped(
        Effect.gen(function* () {
            const handle = yield* ChildProcess.make('cargo', args, { cwd, extendEnv: true, stdin: 'inherit', env });
            const [lines, diagnosed, code] = yield* Effect.all(
                [
                    Stream.splitLines(Stream.decodeText(handle.stdout)).pipe(
                        Stream.mapAccum(HashSet.empty<string>, (seen, line) => {
                            const message = Schema.decodeOption(Message)(line);
                            const shown = Option.match(message, { onNone: () => `${line}\n`, onSome: (held) => (Schema.is(Diagnostic)(held) ? (held.message.rendered ?? '') : '') });
                            return [HashSet.add(seen, shown), [{ text: HashSet.has(seen, shown) ? '' : shown, message }]];
                        }),
                        Stream.tap(({ text }) => Effect.sync(() => stdout.write(text))),
                        Stream.runCollect,
                    ),
                    Stream.mkString(Stream.tap(Stream.decodeText(handle.stderr), (chunk) => Effect.sync(() => stderr.write(chunk)))),
                    handle.exitCode,
                ],
                { concurrency: 'unbounded' },
            );
            return { code, log: [...lines.map(({ text }) => text), diagnosed].join(''), messages: lines.flatMap(({ message }) => Option.toArray(message)) };
        }),
    );

const decided = Effect.fnUntraced(function* (context: ExecutorContext, inputs: Readonly<Record<string, typeof Cargo.Type>>, command: Command, tasks: Array.NonEmptyReadonlyArray<Pick<Task, 'id' | 'target'>>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { nodes, dependencies } = context.projectGraph;
    const root = (project: string) => path.resolve(context.root, nodes[project]?.data.root ?? '.');
    const selected = (at: string, name: string) => ['--package', `${pathToFileURL(path.resolve(context.root, at)).href}#${name}`];
    const options = inputs[tasks[0].id];
    const { flags, env } = REPORTS[command.report];
    yield* Effect.orDie(
        Effect.forEach(
            tasks.flatMap(({ id }) => inputs[id]?.outputs ?? []),
            (file) => fs.makeDirectory(path.dirname(path.resolve(context.root, file)), { recursive: true }),
            { discard: true },
        ),
    );
    const run = yield* Effect.orDie(
        ran(
            path.resolve(context.root, options?.cwd ?? '.'),
            [
                ...command.args,
                ...tasks.flatMap(({ id, target: { project } }) => Option.toArray(Option.fromUndefinedOr(inputs[id]?.package)).flatMap((name) => selected(root(project), name))),
                ...(command.packages ?? []).flatMap(({ root: at, name }) => selected(at, name)),
                ...(command.trailing === undefined ? flags : ['--', ...command.trailing, ...flags]),
                ...(options?.args ?? []),
            ],
            env,
        ),
    );
    const diagnostics = run.messages.filter(Schema.is(Diagnostic)).map((held) => ({ ...held, root: path.dirname(held.manifest_path) }));
    const errored = new Set(diagnostics.filter(({ message: { level } }) => level.startsWith('error')).map(({ root: at }) => at));
    const built = new Set(run.messages.filter(Schema.is(Artifact)).flatMap(({ manifest_path, target: { kind } }) => (kind.includes('custom-build') ? [] : [path.dirname(manifest_path)])));
    const suites = Array.groupBy(run.messages.filter(Schema.is(Suite)), ({ nextest }) => nextest.crate);
    const completed = run.messages.some((held) => Schema.is(Finished)(held) && held.success) && !run.messages.some(Schema.is(Failed));
    const output = (at: string, name: string) => [...diagnostics.filter(({ root: held }) => held === at).map(({ message: { rendered } }) => rendered ?? ''), ...run.messages.filter(Schema.is(Failed)).flatMap((held) => (held.name.startsWith(`${name}::`) ? [`${held.name}\n${held.stdout ?? ''}`] : []))].join('');
    const broken = new Set(Record.keys(Record.filter(nodes, (_, project) => errored.has(root(project)))));
    const reaches = (project: string, seen: ReadonlySet<string>): boolean => (dependencies[project] ?? []).some(({ target }) => !seen.has(target) && (broken.has(target) || reaches(target, new Set([...seen, target]))));
    const results = tasks.map(({ id, target: { project } }) => {
        const [at, name] = [root(project), inputs[id]?.package ?? ''];
        const reported = suites[name] ?? [];
        const [proven, empty] = ({ messages: [built.has(at), false], tests: [reported.length > 0, completed && reported.length === 0], status: [run.code === 0, false] } as const)[command.report];
        return { id, own: errored.has(at) || empty || reported.some(({ event }) => event === 'failed'), blocked: reaches(project, new Set([project])), proven, terminalOutput: empty ? `${name}: no tests to run\n` : output(at, name) };
    });
    const attributed = results.some(({ own }) => own);
    return Record.fromIterableWith(results, ({ id, own, blocked, proven, terminalOutput }): readonly [string, TaskResult] => [
        id,
        Match.value({ passed: proven && !(own || blocked), own, held: blocked || attributed }).pipe(
            Match.when({ passed: true }, (): TaskResult => ({ success: true, terminalOutput })),
            Match.when({ own: true }, (): TaskResult => ({ success: false, terminalOutput })),
            Match.when({ held: true }, (): TaskResult => ({ success: false, status: 'skipped', terminalOutput: '' })),
            Match.orElse((): TaskResult => ({ success: false, terminalOutput: run.log })),
        ),
    ]);
});

const decoded = (inputs: Readonly<Record<string, unknown>>) =>
    Effect.map(
        Effect.forEach(Record.toEntries(inputs), ([id, options]) => Effect.map(Effect.orDie(Schema.decodeUnknownEffect(Cargo)(options)), (held) => [id, held] as const)),
        Record.fromEntries,
    );

const scheduled = (context: ExecutorContext, inputs: Readonly<Record<string, typeof Cargo.Type>>, tasks: readonly Pick<Task, 'id' | 'target'>[]) =>
    Effect.reduce(
        Record.values(Array.groupBy(tasks, ({ id }) => JSON.stringify([inputs[id]?.cwd, inputs[id]?.commands, inputs[id]?.args]))),
        () => Record.empty<string, TaskResult>(),
        (done, group) =>
            Effect.reduce(
                inputs[group[0].id]?.commands ?? [],
                () => done,
                (held, command) =>
                    Array.match(
                        group.filter(({ id }) => held[id]?.success !== false),
                        {
                            onEmpty: () => Effect.succeed(held),
                            onNonEmpty: (ready) => Effect.map(decided(context, inputs, command, ready), (results) => ({ ...held, ...results })),
                        },
                    ),
            ),
    ).pipe(Effect.provide(layer));

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) => {
    const id = `${context.projectName}:${context.targetName}`;
    return Effect.runPromise(Effect.flatMap(decoded({ [id]: options }), (inputs) => scheduled(context, inputs, [{ id, target: { project: context.projectName ?? '', target: context.targetName ?? '' } }])).pipe(Effect.map((results) => ({ success: Record.values(results).every(({ success }) => success) }))));
};

const batch = (taskGraph: TaskGraph, inputs: Record<string, unknown>, _overrides: unknown, context: ExecutorContext) => Effect.runPromise(Effect.flatMap(decoded(inputs), (held) => scheduled(context, held, Record.values(taskGraph.tasks))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { batch, Cargo, type Command, execute, ran };
