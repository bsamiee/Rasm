import { posix } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesResultArray, DependencyType, hashArray, parseJson, type RawProjectGraphDependency } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, getNamedInputs, hashObject, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Schema, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import manifest from '../package.json' with { type: 'json' };
import { configured, type Options, targets } from './targets.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Node = typeof Node.Type;
type Report = typeof Report.Type;

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = '**/{*.{csproj,fsproj,vbproj,props,targets,globalconfig},global.json,Directory.Build.rsp,nuget.config,NuGet.config,NuGet.Config,.editorconfig}';
const { name } = manifest;

// --- [MODELS] --------------------------------------------------------------------------

const Node = Schema.Struct({
    file: Schema.String,
    name: Schema.String,
    language: Schema.String,
    test: Schema.Boolean,
    packable: Schema.Boolean,
    frameworks: Schema.Array(Schema.String),
    configurations: Schema.Array(Schema.String),
    runtimes: Schema.Array(Schema.String),
    references: Schema.Array(Schema.Struct({ root: Schema.String, declared: Schema.String })),
    inputs: Schema.Array(Schema.String),
    outputs: Schema.Array(Schema.String),
    bases: Schema.Array(Schema.String),
    restored: Schema.String,
    results: Schema.NullOr(Schema.String),
});
const Report = Schema.Struct({ nodes: Schema.Array(Node), failures: Schema.Array(Schema.Struct({ file: Schema.String, message: Schema.String })), evaluated: Schema.Array(Schema.String) });
const Runner = Schema.Struct({ test: Schema.optionalKey(Schema.Struct({ runner: Schema.optionalKey(Schema.String) })) });
const Platform = Schema.Literal('Microsoft.Testing.Platform');
const Built = Schema.fromJsonString(Schema.Struct({ TargetResults: Schema.Struct({ Build: Schema.Struct({ Items: Schema.Array(Schema.Struct({ Identity: Schema.String })) }) }) }));

// --- [ERRORS] --------------------------------------------------------------------------

class DotnetError extends Schema.TaggedError<DotnetError, { readonly brand: unique symbol }>()('DotnetError', { subject: Schema.String, detail: Schema.String }) {
    override get message() {
        return `${this.subject}: ${this.detail}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const spawned = (args: readonly string[], cwd: string, input?: string) =>
    Effect.scoped(
        Effect.gen(function* () {
            const handle = yield* ChildProcess.make('dotnet', args, { cwd, extendEnv: true, stdin: input === undefined ? 'ignore' : Stream.encodeText(Stream.succeed(input)) });
            const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
            return code === 0 ? stdout : yield* DotnetError.make({ subject: ['dotnet', ...args].join(' '), detail: `exited ${code}: ${stderr.trim()}` });
        }),
    );

const analyzed = Effect.fnUntraced(function* (workspaceRoot: string, analyzer: string, files: readonly string[]) {
    const path = yield* Path.Path;
    const assembly =
        path.extname(analyzer) === '.dll'
            ? analyzer
            : yield* Effect.flatMap(Effect.flatMap(spawned(['build', analyzer, '--configuration', 'Release', '-getTargetResult:Build'], workspaceRoot), Schema.decodeEffect(Built)), ({ TargetResults }) =>
                  Effect.fromOption(Array.head(TargetResults.Build.Items)).pipe(Effect.mapBoth({ onSuccess: ({ Identity }) => Identity, onFailure: () => DotnetError.make({ subject: analyzer, detail: 'built no assembly' }) })),
              );
    return yield* Effect.flatMap(spawned([assembly, workspaceRoot], workspaceRoot, files.join('\n')), (stdout) =>
        Schema.decodeEffect(Schema.fromJsonString(Report))(stdout).pipe(Effect.mapError((error) => DotnetError.make({ subject: assembly, detail: error.message }))),
    );
});

const stamped = (fs: FileSystem.FileSystem, files: readonly string[]) =>
    Effect.map(
        Effect.forEach(files, (file) =>
            Effect.map(Effect.option(fs.stat(file)), (held) => Option.match(held, { onNone: () => `${file}:`, onSome: ({ mtime, size }) => `${file}:${Option.getOrElse(Option.map(mtime, (at) => at.valueOf()), () => 0)}:${size}` })),
        ),
        (stamps) => stamps.join(','),
    );

const reports = new Map<string, Report>();

const graph = Effect.fnUntraced(function* (workspaceRoot: string, options: Options, matched: readonly string[]) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const files = matched.toSorted();
    const analyzer = yield* path.fromFileUrl(new URL(import.meta.resolve('#analyzer')));
    const folder = path.dirname(analyzer);
    const modules = [path.join(import.meta.dirname, '..', 'package.json'), ...(yield* fs.readDirectory(import.meta.dirname)).toSorted().map((module) => path.join(import.meta.dirname, module))];
    const hash = hashArray([
        yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, files)),
        ...(yield* Effect.forEach(modules, (module) => fs.readFileString(module))),
        yield* stamped(fs, (yield* fs.readDirectory(folder)).toSorted().map((entry) => path.join(folder, entry))),
        hashObject(options),
        ...files,
    ]);
    const restored = (report: Report) => stamped(fs, report.nodes.map(({ restored: at }) => path.join(workspaceRoot, at)));
    const evaluated = (report: Report) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...report.evaluated]));
    const cache = new PluginCache<{ readonly report: Report; readonly evaluated: string; readonly restored: string }>(path.join(workspaceDataDirectory, `${path.basename(name)}-${hashObject(options)}.hash`));
    const held = yield* Option.match(Option.fromNullishOr(cache.get(hash)), {
        onNone: () => Effect.succeedNone,
        onSome: (entry) => Effect.map(Effect.all([evaluated(entry.report), restored(entry.report)]), ([inputs, stamps]) => (inputs === entry.evaluated && stamps === entry.restored ? Option.some(entry.report) : Option.none())),
    });
    const report = Option.isSome(held) ? held.value : yield* analyzed(workspaceRoot, analyzer, files);
    yield* Option.isSome(held) || report.failures.length > 0
        ? Effect.void
        : Effect.flatMap(Effect.all([evaluated(report), restored(report)]), ([inputs, stamps]) => Effect.sync(() => (cache.set(hash, { report, evaluated: inputs, restored: stamps }), cache.writeToDisk())));
    reports.set(hashArray([workspaceRoot, hashObject(options)]), report);
    return report;
});

const platform = Effect.fnUntraced(function* (workspaceRoot: string, files: readonly string[]) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    return files.includes('global.json')
        ? Option.exists(Option.fromUndefinedOr((yield* Schema.decodeEffect(Runner)(parseJson(yield* fs.readFileString(path.join(workspaceRoot, 'global.json'))))).test?.runner), Schema.is(Platform))
        : false;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<Options> = [
    FILES,
    (files, options = {}, context) =>
        Effect.runPromise(
            Effect.gen(function* () {
                const path = yield* Path.Path;
                const [report, testing] = yield* Effect.all([graph(context.workspaceRoot, options, files), platform(context.workspaceRoot, files)]);
                const nodes: CreateNodesResultArray = report.nodes.map((node) => {
                    const root = path.dirname(node.file);
                    return [
                        node.file,
                        {
                            projects: {
                                [root]: {
                                    name: node.name,
                                    metadata: { technologies: ['dotnet', node.language] },
                                    targets: configured(targets(path, node, 'production' in getNamedInputs(root, context) ? 'production' : 'default', testing), options),
                                    ...(node.packable ? { release: { version: { versionActions: `${name}/release/version-actions`, versionActionsOptions: { projectFile: path.basename(node.file) } } } } : {}),
                                },
                            },
                        },
                    ] as const;
                });
                return report.failures.length === 0 ? nodes : yield* Effect.fail(new AggregateCreateNodesError(report.failures.map(({ file, message }) => [file, new Error(message)]), nodes));
            }).pipe(Effect.provide(layer)),
        ),
];

const createDependencies: CreateDependencies<Options> = (options = {}, { workspaceRoot, projects }) => {
    const names = createProjectRootMappingsFromProjectConfigurations(projects);
    return (reports.get(hashArray([workspaceRoot, hashObject(options)]))?.nodes ?? []).flatMap((node) =>
        node.references.flatMap((reference): readonly RawProjectGraphDependency[] =>
            Option.toArray(Option.all([Option.fromUndefinedOr(names.get(posix.dirname(node.file))), Option.fromUndefinedOr(names.get(reference.root))])).map(([source, target]) =>
                reference.declared === node.file ? { source, target, type: DependencyType.static, sourceFile: node.file } : { source, target, type: DependencyType.implicit },
            ),
        ),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes, DotnetError, name, type Node, type Options, spawned };
