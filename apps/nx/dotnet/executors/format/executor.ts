import type { ExecutorContext } from '@nx/devkit';
import type { TaskResult } from '@nx/devkit/internal';
import { executor } from '@rasm/nx-devkit/executor';
import { iterated, ran } from '@rasm/nx-devkit/process';
import { Array, Console, Effect, FileSystem, flow, Option, Order, Path, pipe, Record, Schema, Stream, Struct } from 'effect';
import { Xml } from 'effect/encoding';
import { CommandLine } from 'effect/process';
import { grouped } from '#/executors/build/executor';

// --- [TYPES] ---------------------------------------------------------------------------

type Planned = readonly [string, typeof Format.Type];

// --- [CONSTANTS] -----------------------------------------------------------------------

const UNFORMATTED = 2;

// --- [MODELS] --------------------------------------------------------------------------

const Format = Schema.Struct({
    commands: Schema.NonEmptyArray(Schema.NonEmptyArray(Schema.String)).annotate({ description: 'dotnet format subcommands and their switches, run in order, which the plugin infers' }),
    project: Schema.String.annotate({ description: 'Project file the commands run on' }),
    args: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Arguments appended to every command' }),
}).annotate({ title: 'Format', description: 'Runs dotnet format commands in order on one project, or once each over a solution of every batched project' });
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
    const [[, { commands, args }], ...rest] = group;
    const members = Array.map(group, ([task, { project }]) => ({ task, project: path.resolve(root, project), folder: `${path.dirname(path.resolve(root, project))}${path.sep}` }));
    const nested = Array.sort(
        members,
        Order.mapInput(Order.flip(Order.Number), (member: (typeof members)[number]) => member.folder.length),
    );
    const owner = ({ FilePath }: (typeof Report.Type)[number]) => Array.findFirst(nested, (member) => FilePath.startsWith(member.folder));
    const folder = yield* fs.makeTempDirectoryScoped();
    const target = rest.length === 0 ? Array.headNonEmpty(members).project : path.join(folder, 'batch.slnx');
    yield* rest.length === 0 ? Effect.void : Effect.flatMap(solution(members.map(({ project }) => project)), (text) => fs.writeFileString(target, text));
    const runs = yield* Effect.forEach(
        commands,
        Effect.fnUntraced(function* (command, index) {
            const report = path.join(folder, `${index}.json`);
            const argv = [...command, target, '--report', report, ...(args ?? [])];
            const { code, stdout, stderr } = yield* ran('dotnet', argv, { cwd: root });
            const changed = yield* Effect.option(Effect.flatMap(fs.readFileString(report), Schema.decodeEffect(Report)));
            const owners = Option.flatMap(
                Option.filter(changed, () => code === UNFORMATTED),
                flow(Array.map(owner), Option.all),
            );
            const blamed = code === 0 ? Option.some(new Set<string>()) : Option.map(owners, (found) => new Set(found.map(Struct.get('task'))));
            return { command: CommandLine.display(['dotnet', ...argv]), lines: [stdout, stderr].join('').split('\n'), blamed };
        }),
    );
    const said = (project: string, { command, lines }: (typeof runs)[number]) => {
        const own = lines.filter((text) => text.endsWith(`[${project}]`));
        return [command, ...(own.length > 0 ? own : lines)];
    };
    return Array.map(members, ({ task, project }) => {
        const failed = runs.filter(({ blamed }) => Option.match(blamed, { onNone: () => true, onSome: (tasks) => tasks.has(task) }));
        const result: TaskResult = { success: failed.length === 0, terminalOutput: failed.flatMap((run) => said(project, run)).join('\n') };
        return { task, result };
    });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Format,
    Effect.fnUntraced(function* (format, context) {
        const [{ result }] = yield* formatted(context.root, [[context.root, format]]);
        yield* Console.log(result.terminalOutput);
        return result.success;
    }),
);

const batch = iterated((_: unknown, inputs: Readonly<Record<string, unknown>>, __: unknown, context: ExecutorContext) =>
    pipe(
        Record.toEntries(inputs),
        Effect.forEach(([task, options]) => Effect.map(Schema.decodeUnknownEffect(Format)(options), (format): Planned => [task, format])),
        Effect.map((planned) => Stream.mapEffect(Stream.fromIterable(grouped(planned)), (group) => Effect.scoped(formatted(context.root, group)))),
        Stream.unwrap,
        Stream.flattenIterable,
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { batch, execute };
