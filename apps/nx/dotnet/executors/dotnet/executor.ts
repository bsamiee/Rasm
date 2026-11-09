import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext, TaskGraph } from '@nx/devkit';
import { Array, Effect, FileSystem, Path, Record, Schema, type Scope, Stream } from 'effect';
import { Xml } from 'effect/encoding';
import { ChildProcess } from 'effect/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Planned = readonly [string, typeof Command.Type];

// --- [MODELS] --------------------------------------------------------------------------

const Command = Schema.Struct({ commands: Schema.NonEmptyArray(Schema.NonEmptyArray(Schema.String)), project: Schema.String, args: Schema.optionalKey(Schema.Array(Schema.String)) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const dotnet = (args: readonly string[], cwd: string) => Effect.scoped(Effect.flatMap(ChildProcess.make('dotnet', args, { cwd, extendEnv: true, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' }), (handle) => handle.exitCode)).pipe(Effect.map((code) => code === 0));

const ran = (command: typeof Command.Type, target: string, cwd: string) =>
    Effect.map(
        Effect.forEach(command.commands, (invocation) => dotnet([...invocation, target, ...(command.args ?? [])], cwd)),
        (outcomes) => outcomes.every(Boolean),
    );

const solution = (projects: readonly string[]) =>
    Stream.fromIterable<Xml.Event>([
        { _tag: 'StartElement', name: 'Solution', attributes: {} },
        ...projects.flatMap((project): readonly Xml.Event[] => [
            { _tag: 'StartElement', name: 'Project', attributes: { Path: project } },
            { _tag: 'EndElement', name: 'Project' },
        ]),
        { _tag: 'EndElement', name: 'Solution' },
    ]).pipe(Stream.pipeThroughChannel(Xml.encodeString()), Stream.mkString);

const grouped = Effect.fnUntraced(function* (root: string, group: Array.NonEmptyReadonlyArray<Planned>) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const [[, first], ...rest] = group;
    const target = rest.length === 0 ? first.project : yield* Effect.flatMap(Effect.all([fs.makeTempDirectoryScoped(), solution(group.map(([, command]) => path.resolve(root, command.project)))]), ([folder, text]) => Effect.as(fs.writeFileString(path.join(folder, 'batch.slnx'), text), path.join(folder, 'batch.slnx')));
    return yield* ran(first, target, root);
});

const waves = (graph: TaskGraph, root: string, pending: readonly Planned[], decided: Readonly<Record<string, boolean>>): Effect.Effect<Readonly<Record<string, boolean>>, Effect.Error<ReturnType<typeof grouped>>, Exclude<Effect.Services<ReturnType<typeof grouped>>, Scope.Scope>> => {
    const waiting = ([task]: Planned) => (graph.dependencies[task] ?? []).some(({ id }) => pending.some(([other]) => other === id));
    const clear = ([task]: Planned) => (graph.dependencies[task] ?? []).every(({ id }) => decided[id] !== false);
    const ready = pending.filter((planned) => !waiting(planned));
    const later = pending.filter(waiting);
    const runnable = ready.filter(clear);
    const blocked = ready.filter((planned) => !clear(planned));
    return Array.isReadonlyArrayNonEmpty(ready)
        ? Effect.flatMap(
              Effect.forEach(Record.values(Array.groupBy(runnable, ([, command]) => JSON.stringify([command.commands, command.args ?? []]))), (group) => Effect.map(Effect.scoped(grouped(root, group)), (success) => group.map(([task]) => [task, success] as const))),
              (outcomes) => waves(graph, root, later, { ...decided, ...Record.fromEntries([...outcomes.flat(), ...blocked.map(([task]) => [task, false] as const)]) }),
          )
        : Effect.succeed(decided);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) =>
    Effect.runPromise(
        Effect.flatMap(Schema.decodeUnknownEffect(Command)(options), (command) => ran(command, command.project, context.root)).pipe(
            Effect.map((success) => ({ success })),
            Effect.provide(layer),
        ),
    );

const batch = (graph: TaskGraph, inputs: Readonly<Record<string, unknown>>, _: unknown, context: ExecutorContext) =>
    Effect.runPromise(
        Effect.flatMap(
            Effect.forEach(Record.toEntries(inputs), ([task, options]) => Effect.map(Schema.decodeUnknownEffect(Command)(options), (command): Planned => [task, command])),
            (decoded) => Effect.map(waves(graph, context.root, decoded, {}), (decided) => Record.map(decided, (success) => ({ success, terminalOutput: '' }))),
        ).pipe(Effect.provide(layer)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { batch, execute };
