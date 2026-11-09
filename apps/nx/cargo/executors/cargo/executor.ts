import { pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext, Task, TaskGraph } from '@nx/devkit';
import type { TaskResult } from '@nx/devkit/internal';
import { Array, Effect, HashSet, Option, Path, Record, Result, Schema, Stream } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/process';

// --- [MODELS] --------------------------------------------------------------------------

const Command = Schema.Struct({ args: Schema.NonEmptyArray(Schema.String), trailing: Schema.optionalKey(Schema.Array(Schema.String)) });
const Cargo = Schema.Struct({
    cwd: Schema.String,
    commands: Schema.NonEmptyArray(Command),
    package: Schema.optionalKey(Schema.String),
    warnings: Schema.optionalKey(Schema.Literals(['allow', 'warn', 'deny'])),
    args: Schema.optionalKey(Schema.Array(Schema.String)),
});
const Diagnostic = Schema.Struct({
    reason: Schema.Literal('compiler-message'),
    manifest_path: Schema.String,
    message: Schema.Struct({ level: Schema.String, rendered: Schema.NullOr(Schema.String), code: Schema.NullOr(Schema.Struct({ code: Schema.String })) }),
});
const Message = Schema.fromJsonString(Schema.Union([Diagnostic, Schema.Struct({ reason: Schema.String })]));

type Command = typeof Command.Type;

// --- [OPERATIONS] ----------------------------------------------------------------------

const ran = (cwd: string, args: readonly string[], warnings?: typeof Cargo.Type.warnings) =>
    Effect.scoped(
        Effect.gen(function* () {
            const path = yield* Path.Path;
            const handle = yield* ChildProcess.make('cargo', args, { cwd, extendEnv: true, stdin: 'inherit', ...(warnings === undefined ? {} : { env: { CARGO_BUILD_WARNINGS: warnings } }) });
            const [lines, stderr, code] = yield* Effect.all(
                [
                    Stream.splitLines(Stream.decodeText(handle.stdout)).pipe(
                        Stream.mapAccum(HashSet.empty<string>, (seen, line) => {
                            const message = Schema.decodeOption(Message)(line);
                            const diagnostic = Option.map(Option.filter(message, Schema.is(Diagnostic)), (held) => ({ ...held, root: path.dirname(held.manifest_path) }));
                            const shown = Option.match(diagnostic, { onNone: () => (Option.isSome(message) ? '' : `${line}\n`), onSome: ({ message: { rendered } }) => rendered ?? '' });
                            return [HashSet.add(seen, shown), [{ text: HashSet.has(seen, shown) ? '' : shown, diagnostic }]];
                        }),
                        Stream.tap(({ text }) => Effect.sync(() => process.stdout.write(text))),
                        Stream.runCollect,
                    ),
                    Stream.mkString(Stream.tap(Stream.decodeText(handle.stderr), (chunk) => Effect.sync(() => process.stderr.write(chunk)))),
                    handle.exitCode,
                ],
                { concurrency: 'unbounded' },
            );
            const diagnostics = lines.flatMap(({ diagnostic, text }) => Option.toArray(Option.map(diagnostic, (held) => ({ ...held, text }))));
            return {
                code,
                log: [...lines.map(({ text }) => text), stderr].join(''),
                rendered: Record.map(Array.groupBy(diagnostics, ({ root }) => root), (held) => held.map(({ text }) => text).join('')),
                errored: new Set(diagnostics.filter(({ message: { level } }) => level.startsWith('error')).map(({ root }) => root)),
                failing: new Set(diagnostics.filter(({ message: { level, code: lint } }) => level.startsWith('error') || (warnings === 'deny' && level === 'warning' && lint !== null)).map(({ root }) => root)),
            };
        }),
    );

const decided = (
    context: ExecutorContext,
    inputs: Readonly<Record<string, typeof Cargo.Type>>,
    command: Command,
    tasks: Array.NonEmptyReadonlyArray<Pick<Task, 'id' | 'target'>>,
): Effect.Effect<Record<string, TaskResult>, never, Path.Path | ChildProcessSpawner.ChildProcessSpawner> =>
    Effect.gen(function* () {
        const path = yield* Path.Path;
        const { nodes, dependencies } = context.projectGraph;
        const root = (project: string) => path.resolve(context.root, nodes[project]?.data.root ?? '.');
        const options = inputs[tasks[0].id];
        const run = yield* Effect.orDie(
            ran(
                path.resolve(context.root, options?.cwd ?? '.'),
                [
                    ...command.args,
                    ...tasks.flatMap(({ id, target: { project } }) => Option.toArray(Option.fromUndefinedOr(inputs[id]?.package)).flatMap((name) => ['--package', `${pathToFileURL(root(project)).href}#${name}`])),
                    ...(command.trailing === undefined ? [] : ['--', ...command.trailing]),
                    ...(options?.args ?? []),
                ],
                options?.warnings,
            ),
        );
        const broken = new Set(Record.keys(Record.filter(nodes, (_, project) => run.errored.has(root(project)))));
        const reaches = (project: string, seen: ReadonlySet<string>): boolean =>
            (dependencies[project] ?? []).some(({ target }) => !seen.has(target) && (broken.has(target) || reaches(target, new Set([...seen, target]))));
        const [settled, rest] = Array.partition(tasks, (task) =>
            run.failing.has(root(task.target.project))
                ? Result.succeed([task.id, { success: false, terminalOutput: run.rendered[root(task.target.project)] ?? '' }] as const)
                : reaches(task.target.project, new Set([task.target.project]))
                  ? Result.succeed([task.id, { success: false, status: 'skipped', terminalOutput: '' }] as const)
                  : Result.fail(task),
        );
        const remaining = Array.match(rest, {
            onEmpty: () => Effect.succeed(Record.empty<string, TaskResult>()),
            onNonEmpty: (held) => {
                const [left, right] = Array.splitAtNonEmpty(held, Math.ceil(held.length / 2));
                return run.code === 0
                    ? Effect.succeed(Record.fromIterableWith(held, (task): readonly [string, TaskResult] => [task.id, { success: true, terminalOutput: run.rendered[root(task.target.project)] ?? '' }]))
                    : settled.length > 0
                      ? decided(context, inputs, command, held)
                      : Array.match(right, {
                            onEmpty: () => Effect.succeed({ [held[0].id]: { success: false, terminalOutput: run.log } }),
                            onNonEmpty: (other) => Effect.map(Effect.all([decided(context, inputs, command, left), decided(context, inputs, command, other)]), ([first, second]) => ({ ...first, ...second })),
                        });
            },
        });
        return { ...Record.fromEntries(settled), ...(yield* remaining) };
    });

const decoded = (inputs: Readonly<Record<string, unknown>>) =>
    Effect.map(
        Effect.forEach(Record.toEntries(inputs), ([id, options]) => Effect.map(Effect.orDie(Schema.decodeUnknownEffect(Cargo)(options)), (held) => [id, held] as const)),
        Record.fromEntries,
    );

const scheduled = (context: ExecutorContext, inputs: Readonly<Record<string, typeof Cargo.Type>>, tasks: readonly Pick<Task, 'id' | 'target'>[]) =>
    Effect.reduce(
        Record.values(Array.groupBy(tasks, ({ id }) => JSON.stringify([inputs[id]?.cwd, inputs[id]?.commands, inputs[id]?.warnings, inputs[id]?.args]))),
        () => Record.empty<string, TaskResult>(),
        (done, group) =>
            Effect.reduce(inputs[group[0].id]?.commands ?? [], () => done, (held, command) =>
                Array.match(group.filter(({ id }) => held[id]?.success !== false), {
                    onEmpty: () => Effect.succeed(held),
                    onNonEmpty: (ready) => Effect.map(decided(context, inputs, command, ready), (results) => ({ ...held, ...results })),
                }),
            ),
    ).pipe(Effect.provide(layer));

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) => {
    const id = `${context.projectName}:${context.targetName}`;
    return Effect.runPromise(
        Effect.flatMap(decoded({ [id]: options }), (inputs) => scheduled(context, inputs, [{ id, target: { project: context.projectName ?? '', target: context.targetName ?? '' } }])).pipe(
            Effect.map((results) => ({ success: Record.values(results).every(({ success }) => success) })),
        ),
    );
};

const batch = (taskGraph: TaskGraph, inputs: Record<string, unknown>, _overrides: unknown, context: ExecutorContext) =>
    Effect.runPromise(Effect.flatMap(decoded(inputs), (held) => scheduled(context, held, Record.values(taskGraph.tasks))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { batch, Cargo, type Command, execute, ran };
