import type { ExecutorContext, TaskGraph } from '@nx/devkit';
import type { TaskResult } from '@nx/devkit/internal';
import { Array, Console, Effect, Equal, FileSystem, Option, Path, Predicate, Record, Schema, Stream, String, Struct } from 'effect';
import { Xml } from 'effect/encoding';
import { ran, run, streamed } from '#/targets';

// --- [TYPES] ---------------------------------------------------------------------------

type Planned = readonly [string, typeof Build.Type];

// --- [CONSTANTS] -----------------------------------------------------------------------

const HEX = 16;
const BUILT = 'built.txt';
const ENTRY_TARGETS = ['Build', 'Rebuild', 'Clean', 'Publish', 'Pack'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Build = Schema.Struct({
    project: Schema.String.annotate({ description: 'Project file to build' }),
    command: Schema.NonEmptyArray(Schema.String).annotate({ description: 'dotnet verb and its switches for the configuration, which the plugin infers' }),
    outputs: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: "Folders and files the configuration writes, which the target's outputs name through {options.outputs.<n>}" }),
    __unparsed__: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Words after -- of the run, appended after the configured arguments', $default: { $source: 'unparsed' }, hidden: true, 'x-priority': 'internal' }),
}).annotate({
    title: 'Build',
    description: "Runs dotnet build, publish, or pack once over a generated traversal project of every batched project, as the MSBuild target named for the verb or the -t a run passes, restoring inside the run, letting MSBuild order the projects, and reporting each project's own result",
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const traversal = (projects: readonly string[], target: string) => {
    const element = (name: string, attributes: Readonly<Record<string, string>>, ...children: readonly (readonly Xml.Event[])[]): readonly Xml.Event[] => [{ _tag: 'StartElement', name, attributes }, ...children.flat(), { _tag: 'EndElement', name }];
    const escaped = (project: string) => project.replaceAll(/[%*?@$();']/gu, (character) => `%${character.codePointAt(0)?.toString(HEX)}`);
    const [outer, inner] = ["'$(Selected)' == ''", "'$(Selected)' != ''"];
    return Stream.fromIterable(
        element(
            'Project',
            { DefaultTargets: target },
            element('Import', { Project: '$(MSBuildToolsPath)/NuGet.targets' }),
            element(
                'ItemGroup',
                { Condition: outer },
                ...projects.flatMap((project) => [element('RestoreGraphProjectInputItems', { Include: escaped(project) }), element('Selected', { Include: '$(MSBuildThisFileFullPath)', AdditionalProperties: `Selected=${escaped(project)}` })]),
                element('ProjectReference', { Include: '@(Selected)' }),
            ),
            element('ItemGroup', { Condition: inner }, element('ProjectReference', { Include: '$(Selected)', GlobalPropertiesToRemove: 'Selected' })),
            element('ItemGroup', {}, ...ENTRY_TARGETS.map((name) => element('ProjectReferenceTargets', { Include: name, Targets: name }))),
            ...ENTRY_TARGETS.map((name) =>
                element(
                    'Target',
                    { Name: name, Returns: '@(Built)' },
                    element('MSBuild', { Condition: outer, Projects: '@(Selected)', Targets: name, BuildInParallel: 'true' }, element('Output', { TaskParameter: 'TargetOutputs', ItemName: 'Built' })),
                    element('WriteLinesToFile', { Condition: outer, File: `$(MSBuildThisFileDirectory)${BUILT}`, Lines: '@(Built)', Overwrite: 'true' }),
                    element('MSBuild', { Condition: inner, Projects: '@(ProjectReference)', Targets: name, RemoveProperties: 'Selected', ContinueOnError: 'true' }),
                    element('ItemGroup', { Condition: inner }, element('Built', { Include: '$(Selected)', Condition: "'$(MSBuildLastTaskResult)' == 'true'" })),
                ),
            ),
        ),
    ).pipe(Stream.pipeThroughChannel(Xml.encodeString()), Stream.mkString);
};

const built = Effect.fnUntraced(function* (root: string, group: Array.NonEmptyReadonlyArray<Planned>) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const [[, { command, __unparsed__: words = [] }]] = group;
    const projects = Array.map(group, ([, { project }]) => path.resolve(root, project));
    const folder = yield* fs.makeTempDirectoryScoped();
    const file = path.join(folder, 'batch.proj');
    yield* fs.writeFileString(file, yield* traversal(projects, String.capitalize(Array.headNonEmpty(command))));
    const { invoked, stdout, said: stderr } = yield* ran('dotnet', [Array.headNonEmpty(command), file, ...Array.tailNonEmpty(command), ...words], { cwd: root });
    const succeeded = Option.map(yield* Effect.option(fs.readFileString(path.join(folder, BUILT))), (text) => new Set(text.split('\n')));
    const lines = stdout.split('\n');
    return Array.map(Array.zip(group, projects), ([[task], project]) => {
        const success = Option.exists(succeeded, (set) => set.has(project));
        const own = lines.filter((text) => text.includes(` [${project}`));
        const result: TaskResult = { success, terminalOutput: (success ? own : [invoked, ...(own.length > 0 ? own : [stderr, stdout])]).join('\n') };
        return { task, result };
    });
});

const ordered = (graph: TaskGraph, root: string, groups: readonly Array.NonEmptyReadonlyArray<Planned>[]) =>
    Stream.paginate(groups, (pending) => {
        const waiting = (group: readonly Planned[]) => {
            const elsewhere = new Set(pending.filter((other) => other !== group).flatMap((other) => other.map(([id]) => id)));
            return group.some(([task]) => (graph.dependencies[task] ?? []).some(({ id }) => elsewhere.has(id)));
        };
        return Option.match(Array.head(pending), {
            onNone: () => Effect.succeed([[], Option.none()] as const),
            onSome: (first) => {
                const next = Option.getOrElse(Array.findFirst(pending, Predicate.not(waiting)), () => first);
                return Effect.map(Effect.scoped(built(root, next)), (results) => [results, Option.some(pending.filter((group) => group !== next))] as const);
            },
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

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* (build: typeof Build.Type, context: ExecutorContext) {
    const [{ result }] = yield* built(context.root, [[context.root, build]]);
    yield* Console.log(result.terminalOutput);
    return { success: result.success };
}, run);

const batch = (graph: TaskGraph, inputs: Readonly<Record<string, typeof Build.Type>>, _: unknown, context: ExecutorContext) => streamed(ordered(graph, context.root, grouped(Record.toEntries(inputs))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Build, batch, execute, grouped };
