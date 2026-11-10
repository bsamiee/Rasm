import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import type { TaskResult } from '@nx/devkit/internal';
import { Array, Console, Effect, FileSystem, flow, Path, Record, Schema, Stream } from 'effect';
import { Xml } from 'effect/encoding';
import { grouped } from '#/executors/build/executor';
import { dotnet } from '#/plugins/targets';

// --- [TYPES] ---------------------------------------------------------------------------

type Planned = readonly [string, typeof Format.Type];

// --- [MODELS] --------------------------------------------------------------------------

const Format = Schema.Struct({ commands: Schema.NonEmptyArray(Schema.NonEmptyArray(Schema.String)), project: Schema.String, args: Schema.optionalKey(Schema.Array(Schema.String)) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const solution = (projects: readonly string[]) =>
    Stream.fromIterable<Xml.Event>([
        { _tag: 'StartElement', name: 'Solution', attributes: {} },
        ...projects.flatMap((project): readonly Xml.Event[] => [
            { _tag: 'StartElement', name: 'Project', attributes: { Path: project } },
            { _tag: 'EndElement', name: 'Project' },
        ]),
        { _tag: 'EndElement', name: 'Solution' },
    ]).pipe(Stream.pipeThroughChannel(Xml.encodeString()), Stream.mkString);

const formatted = Effect.fnUntraced(function* (root: string, group: Array.NonEmptyReadonlyArray<Planned>) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const [[, { commands, project, args }], ...rest] = group;
    const target =
        rest.length === 0
            ? path.resolve(root, project)
            : yield* Effect.flatMap(Effect.all([fs.makeTempDirectoryScoped(), solution(group.map(([, planned]) => path.resolve(root, planned.project)))]), ([folder, text]) => Effect.as(fs.writeFileString(path.join(folder, 'batch.slnx'), text), path.join(folder, 'batch.slnx')));
    const runs = yield* Effect.forEach(commands, (command) => dotnet([...command, target, ...(args ?? [])], root));
    const result: TaskResult = { success: runs.every(({ code }) => code === 0), terminalOutput: runs.flatMap(({ stdout, stderr }) => [stdout, stderr]).join('') };
    return Array.map(group, ([id]) => [id, result] as const);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = flow(
    Effect.fnUntraced(function* (options: unknown, context: ExecutorContext) {
        const format = yield* Schema.decodeUnknownEffect(Format)(options);
        const [[, result]] = yield* Effect.scoped(formatted(context.root, [[context.root, format]]));
        yield* Console.log(result.terminalOutput);
        return { success: result.success };
    }, Effect.provide(layer)),
    Effect.runPromise,
);

const batch = flow(
    Effect.fnUntraced(function* (_: unknown, inputs: Readonly<Record<string, unknown>>, __: unknown, context: ExecutorContext) {
        const planned = yield* Effect.forEach(Record.toEntries(inputs), ([task, options]) => Effect.map(Schema.decodeUnknownEffect(Format)(options), (format): Planned => [task, format]));
        return Record.fromEntries((yield* Effect.forEach(grouped(planned), (group) => Effect.scoped(formatted(context.root, group)))).flat());
    }, Effect.provide(layer)),
    Effect.runPromise,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { batch, execute };
