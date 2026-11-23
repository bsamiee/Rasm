import type { TaskResult } from '@nx/devkit/internal';
import { batched, executor, ran, Words } from '@rasm/nx-distribution/inference';
import { Array, Console, Effect, FileSystem, flow, Option, Order, Path, Record, Schema, Stream, Struct } from 'effect';
import { Xml } from 'effect/encoding';
import { grouped } from '#/executors/build/executor';

// --- [TYPES] ---------------------------------------------------------------------------

type Planned = readonly [string, Format];

// --- [CONSTANTS] -----------------------------------------------------------------------

const UNFORMATTED = 2;

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
    const [[, { commands, __unparsed__: words = [] }], ...rest] = group;
    const members = Array.map(group, ([task, { project }]) => ({ task, project: path.resolve(root, project), folder: `${path.dirname(path.resolve(root, project))}${path.sep}` }));
    const nested = Array.sort(
        members,
        Order.mapInput(Order.flip(Order.Number), (member: (typeof members)[number]) => member.folder.length),
    );
    const owner = ({ FilePath }: { readonly FilePath: string }) => Array.findFirst(nested, (member) => FilePath.startsWith(member.folder));
    const folder = yield* fs.makeTempDirectoryScoped();
    const target = rest.length === 0 ? Array.headNonEmpty(members).project : path.join(folder, 'batch.slnx');
    yield* rest.length === 0 ? Effect.void : Effect.flatMap(solution(members.map(({ project }) => project)), (text) => fs.writeFileString(target, text));
    const runs = yield* Effect.forEach(
        commands,
        Effect.fnUntraced(function* (command, index) {
            const report = path.join(folder, `${index}.json`);
            const { invoked, stdout, said: stderr, code } = yield* ran(['dotnet', ...command, target, '--report', report, ...words], { cwd: root });
            const changed = yield* Effect.option(Effect.flatMap(fs.readFileString(report), Schema.decodeEffect(Report)));
            const owners = Option.flatMap(
                Option.filter(changed, () => code === UNFORMATTED),
                flow(Array.map(owner), Option.all),
            );
            const blamed = code === 0 ? Option.some(new Set<string>()) : Option.map(owners, (found) => new Set(found.map(Struct.get('task'))));
            return { command: invoked, lines: [stdout, stderr].join('').split('\n'), blamed };
        }),
    );
    const said = (project: string, { command, lines }: (typeof runs)[number]) => {
        const own = lines.filter((text) => text.endsWith(`[${project}]`));
        return [command, ...(own.length > 0 ? own : lines)];
    };
    return Array.map(members, ({ task, project }) => {
        const failed = runs.filter(({ blamed }) => Option.match(blamed, { onNone: () => true, onSome: (tasks) => tasks.has(task) }));
        const result: TaskResult = { success: failed.length === 0, terminalOutput: failed.flatMap((each) => said(project, each)).join('\n') };
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
