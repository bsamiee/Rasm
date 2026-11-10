import type { ExecutorContext, TaskGraph } from '@nx/devkit';
import type { TaskResult } from '@nx/devkit/internal';
import { line } from '@rasm/nx-devkit/inference';
import { iterated, ran, runtime } from '@rasm/nx-devkit/process';
import { Array, Console, Effect, Equal, FileSystem, flow, Option, Path, Predicate, pipe, Record, Schema, Stream, Struct } from 'effect';
import { Xml } from 'effect/encoding';

// --- [TYPES] ---------------------------------------------------------------------------

type Planned = readonly [string, typeof Build.Type];

// --- [CONSTANTS] -----------------------------------------------------------------------

const HEX = 16;

// --- [MODELS] --------------------------------------------------------------------------

const Build = Schema.Struct({ project: Schema.String, command: Schema.NonEmptyArray(Schema.String), args: Schema.optionalKey(Schema.Array(Schema.String)) });
const Results = Schema.fromJsonString(Schema.Struct({ Items: Schema.Struct({ Built: Schema.Array(Schema.Struct({ Identity: Schema.String })) }) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const traversal = (projects: readonly string[]) => {
    const element = (name: string, attributes: Readonly<Record<string, string>>, ...children: readonly (readonly Xml.Event[])[]): readonly Xml.Event[] => [{ _tag: 'StartElement', name, attributes }, ...children.flat(), { _tag: 'EndElement', name }];
    const escaped = (project: string) => project.replaceAll(/[%*?@$();']/gu, (character) => `%${character.codePointAt(0)?.toString(HEX)}`);
    return Stream.fromIterable(
        element(
            'Project',
            {},
            element('Import', { Project: '$(MSBuildToolsPath)/NuGet.targets' }),
            element('ItemGroup', {}, ...projects.flatMap((project) => [element('RestoreGraphProjectInputItems', { Include: escaped(project) }), element('Selected', { Include: '$(MSBuildThisFileFullPath)', AdditionalProperties: `Selected=${escaped(project)}` })])),
            element('PropertyGroup', {}, element('Requested', {}, [{ _tag: 'Text', text: 'Build' }]), element('Requested', { Condition: "'$(_IsPublishing)' == 'true'" }, [{ _tag: 'Text', text: 'Publish' }])),
            element('Target', { Name: 'Publish', DependsOnTargets: 'Dispatch' }),
            element('Target', { Name: 'Dispatch' }, element('MSBuild', { Projects: '@(Selected)', Targets: 'Selected', BuildInParallel: 'true' }, element('Output', { TaskParameter: 'TargetOutputs', ItemName: 'Built' }))),
            element(
                'Target',
                { Name: 'Selected', Returns: '@(Built)' },
                element('MSBuild', { Projects: '$(Selected)', Targets: '$(Requested)', RemoveProperties: 'Selected', ContinueOnError: 'true' }),
                element('ItemGroup', {}, element('Built', { Include: '$(Selected)', Condition: "'$(MSBuildLastTaskResult)' == 'true'" })),
            ),
        ),
    ).pipe(Stream.pipeThroughChannel(Xml.encodeString()), Stream.mkString);
};

const built = Effect.fnUntraced(function* (root: string, group: Array.NonEmptyReadonlyArray<Planned>) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const [[, { command, args }]] = group;
    const projects = Array.map(group, ([, { project }]) => path.resolve(root, project));
    const file = path.join(yield* fs.makeTempDirectoryScoped(), 'batch.proj');
    yield* fs.writeFileString(file, yield* traversal(projects));
    const argv = [Array.headNonEmpty(command), file, ...Array.tailNonEmpty(command), '-t:Dispatch', '-getItem:Built', ...(args ?? [])];
    const run = yield* ran('dotnet', argv, { cwd: root });
    const succeeded = Option.map(Schema.decodeOption(Results)(run.stdout), ({ Items }) => new Set(Items.Built.map(({ Identity }) => Identity)));
    const lines = run.stderr.split('\n');
    return Array.map(Array.zip(group, projects), ([[task], project]) => {
        const success = Option.exists(succeeded, (set) => set.has(project));
        const own = lines.filter((text) => text.includes(` [${project}`));
        const result: TaskResult = { success, terminalOutput: (success ? own : [line(['dotnet', ...argv]), ...(own.length > 0 ? own : [run.stderr, run.stdout])]).join('\n') };
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

const grouped = <A extends { readonly project: string }>(planned: readonly (readonly [string, A])[]) => {
    const keyed = ([, options]: readonly [string, A]) => Struct.omit(options, ['project']);
    return Array.dedupe(planned.map(keyed)).flatMap((key) =>
        Array.match(
            planned.filter((each) => Equal.equals(keyed(each), key)),
            { onEmpty: () => [], onNonEmpty: (group) => [group] },
        ),
    );
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = flow(
    Effect.fnUntraced(function* (options: unknown, context: ExecutorContext) {
        const build = yield* Schema.decodeUnknownEffect(Build)(options);
        const [{ result }] = yield* Effect.scoped(built(context.root, [[context.root, build]]));
        yield* Console.log(result.terminalOutput);
        return { success: result.success };
    }),
    runtime.runPromise,
);

const batch = iterated((graph: TaskGraph, inputs: Readonly<Record<string, unknown>>, _: unknown, context: ExecutorContext) =>
    pipe(
        Record.toEntries(inputs),
        Effect.forEach(([task, options]) => Effect.map(Schema.decodeUnknownEffect(Build)(options), (build): Planned => [task, build])),
        Effect.map((planned) => ordered(graph, context.root, grouped(planned))),
        Stream.unwrap,
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { batch, execute, grouped, traversal };
