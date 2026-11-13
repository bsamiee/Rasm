import { basename, dirname, extname, join } from 'node:path';
import { platform } from 'node:process';
import { fileURLToPath } from 'node:url';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesContext, DependencyType, hashArray, type ProjectGraphExternalNode, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { combineGlobPatterns, globWithWorkspaceContext, hashFile, hashObject, hashWithWorkspaceContext, mergeTargetConfigurations, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { printed, promised, ToolError } from '@rasm/nx-devkit/process';
import { configured, line, linked } from '@rasm/nx-devkit/project';
import { Array, Effect, Equivalence, FileSystem, Option, Record, Result, Schema, Struct } from 'effect';
import { Node, PLUGIN, targets } from '#/targets';

// --- [TYPES] ---------------------------------------------------------------------------

type Report = typeof Report.Type;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECTS = ['.csproj', '.fsproj', '.vbproj'];
const SOLUTIONS = ['.slnx', '.sln'];
const FILES = combineGlobPatterns([`*{${[...PROJECTS, ...SOLUTIONS].join(',')},.props,.targets,.globalconfig}`, 'global.json', 'Directory.Build.rsp', 'nuget.config', 'NuGet.config', 'NuGet.Config', '.editorconfig'].map((file) => `**/${file}`));
const PLATFORMS = { macOS: 'darwin', Windows: 'win32', Linux: 'linux' } as const satisfies Readonly<Record<string, NodeJS.Platform>>;
const ANALYZER = fileURLToPath(import.meta.resolve('#analyzer'));
const STATED = { metadata: { technologies: ['dotnet'] }, clean: 'dotnet' };

// --- [MODELS] --------------------------------------------------------------------------

const Report = Schema.Struct({ nodes: Schema.Array(Node), failures: Schema.Array(Schema.Struct({ file: Schema.String, message: Schema.String })), evaluated: Schema.Array(Schema.String) });
const Reported = Schema.fromJsonString(Report);
const Options = Schema.Record(
    Schema.String,
    Schema.Union([Schema.Literal(false), Schema.Struct({ options: Schema.optionalKey(Schema.Record(Schema.String, Schema.Unknown)), dependsOn: Schema.optionalKey(Schema.Array(Schema.String)), cache: Schema.optionalKey(Schema.Boolean), parallelism: Schema.optionalKey(Schema.Boolean) }), Schema.String]),
).annotate({
    description:
        'Each key naming a target the plugin infers (build--dotnet, pack--dotnet, check--dotnet-build, check--dotnet-format, fix--dotnet-format, test--dotnet, upgrade--dotnet): false removes the target from every project, an object merges its options, dependsOn, cache, and parallelism over the inferred target as Nx merges target defaults',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const external = ({ id, version }: Node['packages'][number]) => `nuget:${id}@${version}`;

const stamped = Effect.fnUntraced(function* (workspaceRoot: string, matched: readonly string[], { evaluated, nodes }: Report) {
    const fs = yield* FileSystem.FileSystem;
    const tracked = yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...matched, ...evaluated, ...nodes.flatMap(({ locks }) => locks)]));
    const folders = Array.dedupe([dirname(ANALYZER), ...nodes.flatMap(({ restored }) => restored.map((folder) => join(workspaceRoot, folder)))]);
    const held = yield* Effect.forEach(folders, (folder) => Effect.map(Effect.orElseSucceed(fs.readDirectory(folder), Array.empty<string>), (names) => names.toSorted().map((name) => join(folder, name))), { concurrency: 'unbounded' });
    return hashArray([tracked, ...held.flat().flatMap((file) => [file, ...Array.fromNullishOr(hashFile(file))])]);
});

const analyzed = Effect.fnUntraced(function* (workspaceRoot: string, matched: readonly string[]) {
    const report = yield* Effect.flatMap(printed('dotnet', [ANALYZER, workspaceRoot], { cwd: workspaceRoot, input: matched.join('\n') }), Schema.decodeEffect(Reported));
    const known = new Set(yield* Effect.promise(() => globWithWorkspaceContext(workspaceRoot, Array.dedupe(report.nodes.flatMap(({ inputs }) => inputs)))));
    return { ...report, nodes: report.nodes.map((node) => ({ ...node, inputs: node.inputs.filter((input) => known.has(input)) })) };
});

const related = (workspaceRoot: string, { nodes }: Report): readonly RawProjectGraphDependency[] => {
    const pinned = new Map(nodes.map((node) => [node.file, Record.fromIterableWith(node.packages, (reference) => [reference.id, reference] as const)] as const));
    return [
        ...nodes.flatMap((node) => node.references.map((reference): RawProjectGraphDependency => ({ source: dirname(node.file), target: reference.root, ...(reference.declared === node.file ? { type: DependencyType.static, sourceFile: node.file } : { type: DependencyType.implicit }) }))),
        ...nodes.flatMap((node) => node.packages.filter(({ direct }) => direct).map((reference): RawProjectGraphDependency => ({ source: dirname(node.file), target: external(reference), type: DependencyType.implicit }))),
        ...nodes.flatMap((node) => node.packages.flatMap((reference) => Array.getSomes(reference.dependencies.map((dependency) => Record.get(pinned.get(node.file) ?? {}, dependency))).map((target): RawProjectGraphDependency => ({ source: external(reference), target: external(target), type: DependencyType.static })))),
        ...nodes
            .filter((node) => node.modes.some(({ outputs }) => outputs.some((output) => join(workspaceRoot, output) === dirname(ANALYZER))))
            .map((node): RawProjectGraphDependency => ({ source: dirname(dirname(dirname(ANALYZER))).slice(workspaceRoot.length + 1), target: dirname(node.file), type: DependencyType.static, sourceFile: join(dirname(dirname(dirname(ANALYZER))).slice(workspaceRoot.length + 1), 'package.json') })),
    ];
};

const projected = (nodes: readonly Node[], options: typeof Options.Type, context: CreateNodesContext) => {
    const releasable = (node: Node) => node.packable || node.publishable;
    const folders = Record.fromIterableWith(nodes, (node) => [dirname(node.file), node] as const);
    const carried = (node: Node): readonly NonNullable<Node['releases']>[] => [
        ...Option.toArray(Option.fromNullOr(node.releases)),
        ...Array.getSomes(node.references.map(({ root }) => Record.get(folders, root)))
            .filter((reference) => !releasable(reference))
            .flatMap(carried),
    ];
    const merged = (inferred: Readonly<Record<string, TargetConfiguration>> = {}) => Record.filterMap(inferred, (target, name) => stated(name, target));
    const stated = (name: string, target: TargetConfiguration) => {
        const row = options[name];
        return row === false
            ? Result.failVoid
            : Result.succeed(
                  row === undefined || typeof row === 'string'
                      ? target
                      : mergeTargetConfigurations(
                            { ...(row.options !== undefined && { options: { ...row.options } }), ...(row.dependsOn !== undefined && { dependsOn: [...row.dependsOn] }), ...(row.cache !== undefined && { cache: row.cache }), ...(row.parallelism !== undefined && { parallelism: row.parallelism }) },
                            target,
                        ),
              );
    };
    return Effect.forEach(nodes, (node) => {
        const supported = Array.intersection(Record.keys(PLATFORMS), node.platforms).map((name) => PLATFORMS[name]);
        const limited = node.test && supported.length > 0 && supported.length < Record.keys(PLATFORMS).length;
        const { members, ...released } = targets({ ...node, test: limited ? supported.some((held) => held === platform) : node.test });
        const versioned = { manifest: basename(node.file), releases: Array.dedupeWith(carried(node), Equivalence.mapInput(Equivalence.String, Struct.get('unshipped'))) };
        return Effect.map(
            configured(
                members,
                {
                    ...STATED,
                    root: dirname(node.file),
                    name: node.name,
                    metadata: { technologies: ['dotnet', node.language] },
                    ...released,
                    ...(releasable(node) && { release: { version: { versionActions: `${PLUGIN}/release/version-actions`, versionActionsOptions: versioned } } }),
                    ...(limited && { tags: supported.map((held) => `os:${held}`) }),
                    ...(node.implicit.length > 0 && { implicitDependencies: [...node.implicit] }),
                },
                context,
            ),
            (project) =>
                [
                    node.file,
                    {
                        projects: { [dirname(node.file)]: { ...project, targets: merged(project.targets) } },
                        externalNodes: Record.fromIterableWith(node.packages, (reference): readonly [string, ProjectGraphExternalNode] => [external(reference), { type: 'nuget', name: external(reference), data: { version: reference.version, packageName: reference.id, hash: reference.hash } }]),
                    },
                ] as const,
        );
    });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const edges = new Map<string, readonly RawProjectGraphDependency[]>();

const createNodes: CreateNodes = [
    FILES,
    (matched, options, context) =>
        promised(
            Effect.gen(function* () {
                const fs = yield* FileSystem.FileSystem;
                const { workspaceRoot } = context;
                const stated = yield* Schema.decodeUnknownEffect(Options)(options ?? {});
                const cache = new PluginCache<{ readonly report: Report; readonly digest: string }>(join(workspaceDataDirectory, `dotnet-${hashObject(options ?? {})}.hash`));
                const held = yield* Effect.filter(Array.fromNullishOr(cache.get(PLUGIN)), ({ report, digest }) => Effect.map(stamped(workspaceRoot, matched, report), (now) => now === digest));
                const report = Array.isReadonlyArrayNonEmpty(held) ? Array.headNonEmpty(held).report : yield* analyzed(workspaceRoot, matched);
                yield* Effect.map(stamped(workspaceRoot, matched, report), (digest) => (report.failures.length === 0 ? cache.set(PLUGIN, { report, digest }) : undefined));
                yield* Effect.sync(() => cache.writeToDisk());
                edges.set(workspaceRoot, related(workspaceRoot, report));
                const solutions = yield* Effect.forEach(
                    matched.filter((file) => SOLUTIONS.includes(extname(file))),
                    Effect.fnUntraced(function* (file) {
                        const named = yield* fs.exists(join(workspaceRoot, dirname(file), 'package.json'));
                        const upgrade = {
                            command: line(['dotnet', 'package', 'update', '--project', basename(file)]),
                            metadata: { description: `Moves every package reference of ${basename(file)} to its newest release with dotnet package update`, help: { command: 'dotnet package update --help', example: { args: ['--vulnerable'] } } },
                        };
                        const { targets: inferred = {}, ...project } = yield* configured({ upgrade: { dotnet: upgrade } }, { ...STATED, root: dirname(file), ...(named ? {} : { name: basename(file, extname(file)) }) }, context);
                        const kept: Readonly<Record<string, TargetConfiguration>> = inferred;
                        return [file, { projects: { [dirname(file)]: { ...project, targets: Record.filter(kept, (_, name) => stated[name] !== false) } }, externalNodes: {} }] as const;
                    }),
                );
                const results = [...(yield* projected(report.nodes, stated, context)), ...solutions];
                const failures = report.failures.map(({ file, message }): [string, Error] => [file, ToolError.make({ subject: file, detail: message })]);
                return yield* failures.length === 0 ? Effect.succeed(results) : Effect.fail(new AggregateCreateNodesError(failures, results));
            }),
        ),
];

const createDependencies: CreateDependencies = (_, context) => linked(context, edges.get(context.workspaceRoot) ?? []);

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
