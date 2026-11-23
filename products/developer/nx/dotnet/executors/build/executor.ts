import type { TaskGraph } from '@nx/devkit';
import type { TaskResult } from '@nx/devkit/internal';
import { batched, executor, ran, spawned, Words } from '@rasm/nx-distribution/inference';
import { Array, Console, Effect, Equal, FileSystem, Option, Path, Record, Schema, Stream, String, Struct } from 'effect';
import { Xml } from 'effect/encoding';
import { ANALYZER, ENVIRONMENT } from '#/targets';

// --- [TYPES] ---------------------------------------------------------------------------

type Planned = readonly [string, Build];

// --- [CONSTANTS] -----------------------------------------------------------------------

const ENTRY_TARGETS = ['Build', 'Rebuild', 'Clean', 'Publish', 'Pack'] as const;

// --- [MODELS] --------------------------------------------------------------------------

class Build extends Schema.Opaque<Build, unknown>()(
    Schema.Struct({
        project: Schema.String.annotate({ description: 'Project file to build' }),
        command: Schema.NonEmptyArray(Schema.String).annotate({ description: 'dotnet verb and its switches for the configuration, which the plugin infers' }),
        outputs: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: "Folders and files the configuration writes, which the target's outputs name through {options.outputs.<n>}" }),
        solution: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)).annotate({
            description: 'Configuration|Platform of each project file the build reaches, passed as CurrentSolutionConfigurationContents, which the plugin infers where a reached project lacks the configuration',
        }),
        __unparsed__: Words,
    }).annotate({
        title: 'Build',
        description: "Runs dotnet build, publish, or pack once over a generated traversal project of every batched project, as the MSBuild target named for the verb or the -t a run passes, restoring inside the run, letting MSBuild order the projects, and reporting each project's own result from the run's binary log",
    }),
) {}
const Outcomes = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Struct({ succeeded: Schema.Boolean, lines: Schema.Array(Schema.String) }))).annotate({
    description: 'Success and diagnostics per project file the binary log of a run records, which MsbuildAnalyzer results prints',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const escaped = (text: string) => text.replaceAll(/[%*?@$();',"]/gu, (character) => `%${character.codePointAt(0)?.toString(16)}`);

const built = Effect.fnUntraced(function* (root: string, group: Array.NonEmptyReadonlyArray<Planned>) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const [[, { command, solution = {}, __unparsed__: words = [] }]] = group;
    const assigned = Record.collect(solution, (file, configuration): Xml.Element => ({ name: 'ProjectConfiguration', attributes: { Project: file, AbsolutePath: path.resolve(root, file) }, children: [configuration] }));
    const projects = Array.map(group, ([, { project }]) => path.resolve(root, project));
    const folder = yield* fs.makeTempDirectoryScoped();
    const [file, log] = [path.join(folder, 'batch.proj'), path.join(folder, 'batch.binlog')];
    yield* Stream.fromIterable(
        Xml.toEvents({
            name: 'Project',
            attributes: { DefaultTargets: String.capitalize(Array.headNonEmpty(command)) },
            children: [
                { name: 'Import', attributes: { Project: '$(MSBuildToolsPath)/NuGet.targets' }, children: [] },
                {
                    name: 'ItemGroup',
                    attributes: {},
                    children: [
                        ...projects.flatMap((project) => ['RestoreGraphProjectInputItems', 'ProjectReference'].map((name) => ({ name, attributes: { Include: escaped(project) }, children: [] }))),
                        ...ENTRY_TARGETS.map((name) => ({ name: 'ProjectReferenceTargets', attributes: { Include: name, Targets: name }, children: [] })),
                    ],
                },
                ...ENTRY_TARGETS.map((name) => ({ name: 'Target', attributes: { Name: name }, children: [{ name: 'MSBuild', attributes: { Projects: '@(ProjectReference)', Targets: name, BuildInParallel: 'true' }, children: [] }] })),
            ],
        }),
    ).pipe(Stream.pipeThroughChannel(Xml.encode()), Stream.run(fs.sink(file)));
    const contents = yield* Stream.fromIterable(Xml.toEvents({ name: 'SolutionConfiguration', attributes: {}, children: assigned })).pipe(Stream.pipeThroughChannel(Xml.encodeString()), Stream.mkString);
    const { invoked, stdout, said } = yield* ran(['dotnet', Array.headNonEmpty(command), file, ...Array.tailNonEmpty(command), ...(assigned.length > 0 ? [`-p:CurrentSolutionConfigurationContents=${escaped(contents)}`] : []), `-bl:${log}`, ...words], { cwd: root });
    const outcomes = yield* Effect.flatMap(spawned(['dotnet', ANALYZER, 'results', root, log], { cwd: root, env: ENVIRONMENT }), Schema.decodeEffect(Outcomes));
    return Array.map(Array.zip(group, projects), ([[task], project]) => {
        const outcome = Record.get(outcomes, project);
        const success = Option.exists(outcome, Struct.get('succeeded'));
        const own = Option.match(outcome, { onNone: () => [], onSome: ({ lines }) => lines });
        const result: TaskResult = { success, terminalOutput: (success ? own : [invoked, ...(own.length > 0 ? own : [said, stdout])]).join('\n') };
        return { task, result };
    });
});

const grouped = <A extends { readonly project: string; readonly outputs?: readonly string[] }>(planned: readonly (readonly [string, A])[]) => {
    const keyed = ([, options]: readonly [string, A]) => Struct.omit(options, ['project', 'outputs']);
    return Array.dedupe(planned.map(keyed)).flatMap((key) =>
        Array.match(
            planned.filter((each) => Equal.equals(keyed(each), key)),
            { onEmpty: () => [], onNonEmpty: (group) => [group] },
        ),
    );
};

const ordered = (graph: TaskGraph, root: string, planned: readonly Planned[]) =>
    Stream.paginate(
        planned,
        Effect.fnUntraced(function* (pending: readonly Planned[]) {
            const waiting = new Set(pending.map(([id]) => id));
            const runnable = (group: readonly Planned[]): readonly Planned[] => {
                const held = new Set(group.map(([id]) => id));
                const ready = group.filter(([task]) => (graph.dependencies[task] ?? []).every(({ id }) => !waiting.has(id) || held.has(id)));
                return ready.length === group.length ? group : runnable(ready);
            };
            const next = Array.findFirst(grouped(pending).map(runnable), Array.isReadonlyArrayNonEmpty);
            const cycled: TaskResult = { success: false, terminalOutput: `Task cycle among ${[...waiting].join(', ')}, as each waits on a task of another batch, drop one dependsOn entry of it` };
            const results = yield* Option.match(next, { onNone: () => Effect.succeed(pending.map(([task]) => ({ task, result: cycled }))), onSome: (group) => Effect.scoped(built(root, group)) });
            const done = new Set(Option.match(next, { onNone: () => waiting, onSome: (group) => group.map(([id]) => id) }));
            return [
                results,
                Option.liftPredicate(
                    pending.filter(([task]) => !done.has(task)),
                    Array.isReadonlyArrayNonEmpty,
                ),
            ] as const;
        }),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Build, function* (build, context) {
    const [{ result }] = yield* built(context.root, [[context.root, build]]);
    yield* Console.log(result.terminalOutput);
    return { success: result.success };
});

const batch = batched(Build, (graph, inputs, context) => ordered(graph, context.root, Record.toEntries(inputs)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Build, batch, execute, grouped };
