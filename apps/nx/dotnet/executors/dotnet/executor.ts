import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext, TaskGraph } from '@nx/devkit';
import { Array, Effect, FileSystem, Order, Path, Record, Schema } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Group = Array.NonEmptyReadonlyArray<readonly [string, typeof Command.Type]>;

// --- [MODELS] --------------------------------------------------------------------------

const Command = Schema.Struct({ command: Schema.NonEmptyArray(Schema.String), project: Schema.String, args: Schema.optionalKey(Schema.Array(Schema.String)) });

// --- [CONSTANTS] -----------------------------------------------------------------------

const ENTITIES = { '&': '&amp;', '<': '&lt;', '"': '&quot;' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const dotnet = (args: readonly string[], cwd: string) =>
    Effect.scoped(Effect.flatMap(ChildProcess.make('dotnet', args, { cwd, extendEnv: true, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' }), (handle) => handle.exitCode)).pipe(Effect.map((code) => code === 0));

const solution = (projects: readonly string[]) =>
    ['<Solution>', ...projects.map((project) => `    <Project Path="${project.replace(/[&<"]/g, (character) => ENTITIES[character as keyof typeof ENTITIES])}" />`), '</Solution>', ''].join('\n');

const grouped = Effect.fnUntraced(function* (root: string, group: Group) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const [[, first], ...rest] = group;
    const target =
        rest.length === 0
            ? first.project
            : yield* Effect.flatMap(fs.makeTempDirectoryScoped(), (folder) =>
                  Effect.as(fs.writeFileString(path.join(folder, 'batch.slnx'), solution(group.map(([, options]) => path.resolve(root, options.project)))), path.join(folder, 'batch.slnx')),
              );
    return yield* dotnet([...first.command, target, ...(first.args ?? [])], root);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) =>
    Effect.runPromise(
        Effect.flatMap(Schema.decodeUnknownEffect(Command)(options), (held) => dotnet([...held.command, held.project, ...(held.args ?? [])], context.root)).pipe(
            Effect.map((success) => ({ success })),
            Effect.provide(layer),
        ),
    );

const batch = (graph: TaskGraph, inputs: Readonly<Record<string, unknown>>, _: unknown, context: ExecutorContext) =>
    Effect.runPromise(
        Effect.gen(function* () {
            const decoded = yield* Effect.forEach(Record.toEntries(inputs), ([task, options]) => Effect.map(Schema.decodeUnknownEffect(Command)(options), (held) => [task, held] as const));
            const groups = Record.values(Array.groupBy(decoded, ([, held]) => JSON.stringify([held.command, held.args ?? []])));
            const after = (one: Group, other: Group) => one.some(([task]) => (graph.dependencies[task] ?? []).some((dependency) => other.some(([id]) => id === dependency.id)));
            const ordered = Array.sort(groups, Order.make<Group>((left, right) => (after(left, right) ? 1 : after(right, left) ? -1 : 0)));
            const results = yield* Effect.reduce(ordered, () => Record.empty<string, boolean>(), (held, group) =>
                Effect.map(
                    group.every(([task]) => (graph.dependencies[task] ?? []).every((dependency) => held[dependency.id] !== false)) ? Effect.scoped(grouped(context.root, group)) : Effect.succeed(false),
                    (success) => ({ ...held, ...Record.fromEntries(group.map(([task]) => [task, success] as const)) }),
                ),
            );
            return Record.map(results, (success) => ({ success, terminalOutput: '' }));
        }).pipe(Effect.provide(layer)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { batch };
export default execute;
