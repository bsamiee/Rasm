import { spawn } from 'node:child_process';
import { once } from 'node:events';
import consumers from 'node:stream/consumers';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import type { ExecutorContext, TaskGraph } from '@nx/devkit';
import type { TaskResult } from '@nx/devkit/internal';
import { Array, Console, Effect, Equal, FileSystem, Option, Path, Predicate, Record, Schema, type Scope, Stream, String, Struct } from 'effect';
import { Xml } from 'effect/encoding';

// --- [TYPES] ---------------------------------------------------------------------------

type Planned = readonly [string, typeof Build.Type];

// --- [CONSTANTS] -----------------------------------------------------------------------

const HEX = 16;

// --- [MODELS] --------------------------------------------------------------------------

const Build = Schema.Struct({
    project: Schema.String.annotate({ description: 'Project file to build' }),
    command: Schema.NonEmptyArray(Schema.String).annotate({ description: 'dotnet verb and its switches for the configuration, which the plugin infers' }),
    outputs: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: "Folders and files the configuration writes, which the target's outputs name through {options.outputs.<n>}" }),
    args: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Arguments appended to the dotnet command' }),
}).annotate({ title: 'Build', description: "Runs dotnet build, publish, or pack once over a generated traversal project of every batched project, as the MSBuild target named for the verb, restoring inside the run, letting MSBuild order the projects, and reporting each project's own result" });
const Results = Schema.fromJsonString(Schema.Struct({ Items: Schema.Struct({ Built: Schema.Array(Schema.Struct({ Identity: Schema.String })) }) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const traversal = (projects: readonly string[], target: string) => {
    const element = (name: string, attributes: Readonly<Record<string, string>>, ...children: readonly (readonly Xml.Event[])[]): readonly Xml.Event[] => [{ _tag: 'StartElement', name, attributes }, ...children.flat(), { _tag: 'EndElement', name }];
    const escaped = (project: string) => project.replaceAll(/[%*?@$();']/gu, (character) => `%${character.codePointAt(0)?.toString(HEX)}`);
    return Stream.fromIterable(
        element(
            'Project',
            {},
            element('Import', { Project: '$(MSBuildToolsPath)/NuGet.targets' }),
            element('ItemGroup', {}, ...projects.flatMap((project) => [element('RestoreGraphProjectInputItems', { Include: escaped(project) }), element('Selected', { Include: '$(MSBuildThisFileFullPath)', AdditionalProperties: `Selected=${escaped(project)}` })])),
            element('Target', { Name: target }, element('MSBuild', { Projects: '@(Selected)', Targets: 'Selected', BuildInParallel: 'true' }, element('Output', { TaskParameter: 'TargetOutputs', ItemName: 'Built' }))),
            element('Target', { Name: 'Selected', Returns: '@(Built)' }, element('MSBuild', { Projects: '$(Selected)', Targets: target, RemoveProperties: 'Selected', ContinueOnError: 'true' }), element('ItemGroup', {}, element('Built', { Include: '$(Selected)', Condition: "'$(MSBuildLastTaskResult)' == 'true'" }))),
        ),
    ).pipe(Stream.pipeThroughChannel(Xml.encodeString()), Stream.mkString);
};

const built = Effect.fnUntraced(function* (root: string, group: Array.NonEmptyReadonlyArray<Planned>) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const [[, { command, args }]] = group;
    const target = String.capitalize(Array.headNonEmpty(command));
    const projects = Array.map(group, ([, { project }]) => path.resolve(root, project));
    const file = path.join(yield* fs.makeTempDirectoryScoped(), 'batch.proj');
    yield* fs.writeFileString(file, yield* traversal(projects, target));
    const argv = [Array.headNonEmpty(command), file, ...Array.tailNonEmpty(command), `-t:${target}`, '-getItem:Built', ...(args ?? [])];
    const child = spawn('dotnet', argv, { cwd: root, stdio: ['ignore', 'pipe', 'pipe'] });
    const [out, err] = yield* Effect.all([Effect.fromNullishOr(child.stdout), Effect.fromNullishOr(child.stderr)]);
    const [stdout, stderr] = yield* Effect.tryPromise(() => Promise.all([consumers.text(out), consumers.text(err), once(child, 'close')]));
    const succeeded = Option.map(Schema.decodeOption(Results)(stdout), ({ Items }) => new Set(Items.Built.map(({ Identity }) => Identity)));
    const lines = stderr.split('\n');
    return Array.map(Array.zip(group, projects), ([[task], project]) => {
        const success = Option.exists(succeeded, (set) => set.has(project));
        const own = lines.filter((text) => text.includes(` [${project}`));
        const result: TaskResult = { success, terminalOutput: (success ? own : [child.spawnargs.join(' '), ...(own.length > 0 ? own : [stderr, stdout])]).join('\n') };
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

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices | Scope.Scope>) => Effect.runPromise(Effect.provide(Effect.scoped(effect), layer));

const streamed = <A, E>(results: Stream.Stream<A, E, NodeServices>) => Stream.toReadableStream(Stream.provide(results, layer)).values();

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* (build: typeof Build.Type, context: ExecutorContext) {
    const [{ result }] = yield* built(context.root, [[context.root, build]]);
    yield* Console.log(result.terminalOutput);
    return { success: result.success };
}, run);

const batch = (graph: TaskGraph, inputs: Readonly<Record<string, typeof Build.Type>>, _: unknown, context: ExecutorContext) => streamed(ordered(graph, context.root, grouped(Record.toEntries(inputs))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Build, batch, execute, grouped, run, streamed };
