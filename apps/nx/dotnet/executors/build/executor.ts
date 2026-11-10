import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext, TaskGraph } from '@nx/devkit';
import type { TaskResult } from '@nx/devkit/internal';
import { Array, Console, Effect, Equal, FileSystem, flow, Option, Path, Predicate, Record, Schema, type Scope, Stream, Struct } from 'effect';
import { Xml } from 'effect/encoding';
import { dotnet } from '#/plugins/targets';

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
    const run = yield* dotnet([Array.headNonEmpty(command), file, ...Array.tailNonEmpty(command), '-t:Dispatch', '-getItem:Built', ...(args ?? [])], root);
    const succeeded = Option.map(Schema.decodeOption(Results)(run.stdout), ({ Items }) => new Set(Items.Built.map(({ Identity }) => Identity)));
    const lines = run.stderr.split('\n');
    return Array.map(Array.zip(group, projects), ([[id], project]): readonly [string, TaskResult] => {
        const success = Option.exists(succeeded, (set) => set.has(project));
        const own = lines.filter((line) => line.includes(` [${project}`));
        return [id, { success, terminalOutput: (success || own.length > 0 ? own : [run.stderr, run.stdout]).join('\n') }];
    });
});

const ordered = (graph: TaskGraph, root: string, pending: readonly Array.NonEmptyReadonlyArray<Planned>[], decided: Readonly<Record<string, TaskResult>>): Effect.Effect<Readonly<Record<string, TaskResult>>, Effect.Error<ReturnType<typeof built>>, Exclude<Effect.Services<ReturnType<typeof built>>, Scope.Scope>> => {
    const waiting = (group: readonly Planned[]) => {
        const elsewhere = new Set(pending.filter((other) => other !== group).flatMap((other) => other.map(([id]) => id)));
        return group.some(([task]) => (graph.dependencies[task] ?? []).some(({ id }) => elsewhere.has(id)));
    };
    return Option.match(Array.head(pending), {
        onNone: () => Effect.succeed(decided),
        onSome: (first) => {
            const next = Option.getOrElse(Array.findFirst(pending, Predicate.not(waiting)), () => first);
            return Effect.flatMap(Effect.scoped(built(root, next)), (results) =>
                ordered(
                    graph,
                    root,
                    pending.filter((group) => group !== next),
                    { ...decided, ...Record.fromEntries(results) },
                ),
            );
        },
    });
};

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
        const [[, result]] = yield* Effect.scoped(built(context.root, [[context.root, build]]));
        yield* Console.log(result.terminalOutput);
        return { success: result.success };
    }, Effect.provide(layer)),
    Effect.runPromise,
);

const batch = flow(
    Effect.fnUntraced(function* (graph: TaskGraph, inputs: Readonly<Record<string, unknown>>, _: unknown, context: ExecutorContext) {
        const planned = yield* Effect.forEach(Record.toEntries(inputs), ([task, options]) => Effect.map(Schema.decodeUnknownEffect(Build)(options), (build): Planned => [task, build]));
        return yield* ordered(graph, context.root, grouped(planned), {});
    }, Effect.provide(layer)),
    Effect.runPromise,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { batch, execute, grouped, traversal };
