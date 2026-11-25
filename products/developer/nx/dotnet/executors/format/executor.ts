import type { TaskResult } from '@nx/devkit/internal';
import { batched, executor, inside, ran, Words } from '@rasm/nx';
import { Array, Clock, Console, Effect, FileSystem, flow, Option, Order, Path, Record, Schema, Stream, Struct } from 'effect';
import { Xml } from 'effect/encoding';
import { grouped } from '#/executors/build/executor';

// --- [TYPES] ---------------------------------------------------------------------------

type Planned = readonly [string, Format];

// --- [MODELS] --------------------------------------------------------------------------

class Format extends Schema.Opaque<Format, unknown>()(
    Schema.Struct({
        commands: Schema.NonEmptyArray(Schema.NonEmptyArray(Schema.String)).annotate({ description: 'dotnet format subcommands and their switches, run in order, which the plugin infers' }),
        project: Schema.String.annotate({ description: 'Project file the commands run on' }),
        __unparsed__: Words,
    }).annotate({ title: 'Format', description: 'Runs dotnet format commands in order on one project, or once each over a solution of every batched project' }),
) {}
const Report = Schema.fromJsonString(Schema.Array(Schema.Struct({ FilePath: Schema.String }))).annotate({ description: 'The file dotnet format --report writes, one entry per document it changed or would change' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const formatted = Effect.fnUntraced(function* (root: string, group: Array.NonEmptyReadonlyArray<Planned>) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const [, { commands, __unparsed__: words = [] }] = Array.headNonEmpty(group);
    const members = Array.map(group, ([task, { project }]) => ({ task, project: path.resolve(root, project), folder: path.dirname(path.resolve(root, project)) }));
    const nested = Array.sortWith(members, (member) => member.folder.length, Order.flip(Order.Number));
    const owner = ({ FilePath }: { readonly FilePath: string }) => Array.findFirst(nested, (member) => inside(member.folder)(FilePath));
    const [startTime, folder] = yield* Effect.all([Clock.currentTimeMillis, fs.makeTempDirectoryScoped()]);
    const target = group.length === 1 ? Array.headNonEmpty(members).project : path.join(folder, 'batch.slnx');
    yield* group.length === 1 ? Effect.void : Stream.fromIterable(Xml.toEvents({ name: 'Solution', attributes: {}, children: members.map(({ project }) => ({ name: 'Project', attributes: { Path: project }, children: [] })) })).pipe(Stream.pipeThroughChannel(Xml.encode()), Stream.run(fs.sink(target)));
    const runs = yield* Effect.forEach(
        commands,
        Effect.fnUntraced(function* (command, index) {
            const report = path.join(folder, `${index}.json`);
            const { invoked, stdout, said: stderr, code } = yield* ran(['dotnet', ...command, target, '--report', report, ...words], { cwd: root });
            const changed = yield* Effect.option(Effect.flatMap(fs.readFileString(report), Schema.decodeEffect(Report)));
            const owners = changed.pipe(
                Option.filter(() => code === 2),
                Option.flatMap(flow(Array.map(owner), Option.all)),
            );
            const blamed = code === 0 ? Option.some(new Set<string>()) : Option.map(owners, (found) => new Set(found.map(Struct.get('task'))));
            return { command: invoked, lines: [stdout, stderr].join('').split('\n'), blamed };
        }),
    );
    const endTime = yield* Clock.currentTimeMillis;
    const said = (project: string, { command, lines }: (typeof runs)[number]) => {
        const own = lines.filter((text) => text.endsWith(`[${project}]`));
        return [command, ...(own.length > 0 ? own : lines)];
    };
    return Array.map(members, ({ task, project }) => {
        const failed = runs.filter(({ blamed }) => Option.match(blamed, { onNone: () => true, onSome: (tasks) => tasks.has(task) }));
        const result: TaskResult = { success: failed.length === 0, terminalOutput: failed.flatMap((each) => said(project, each)).join('\n'), startTime, endTime };
        return { task, result };
    });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Format, function* (format, context) {
    const [{ result }] = yield* formatted(context.root, [[context.root, format]]);
    yield* Console.log(result.terminalOutput);
    return { success: result.success };
});

const batch = batched(Format, (_, inputs, context) => Stream.flattenIterable(Stream.mapEffect(Stream.fromIterable(grouped(Record.toEntries(inputs))), (group) => Effect.scoped(formatted(context.root, group)))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { batch, execute, Format };
