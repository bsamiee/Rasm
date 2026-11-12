import { platform } from 'node:process';
import { type CreateDependenciesContext, DependencyType, type ProjectGraphExternalNode, type RawProjectGraphDependency } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { inferred, type Project, type Request, submodules, type Unit } from '@rasm/nx-devkit/plugin';
import { printed, ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, Equivalence, Option, Path, Record, Schema, Struct } from 'effect';
import { analyzer, Node, targets } from '#/targets';

// --- [TYPES] ---------------------------------------------------------------------------

type Report = typeof Report.Type;
interface Evaluated extends Unit {
    readonly core: string;
    readonly held: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECTS = ['csproj', 'fsproj', 'vbproj'];
const FILES = [`*.{${PROJECTS.join(',')},props,targets,globalconfig}`, 'global.json', 'Directory.Build.rsp', 'nuget.config', 'NuGet.config', 'NuGet.Config', '.editorconfig'] as const;
const PLATFORMS = { macOS: 'darwin', Windows: 'win32', Linux: 'linux' } as const satisfies Readonly<Record<string, NodeJS.Platform>>;

// --- [MODELS] --------------------------------------------------------------------------

const Report = Schema.Struct({ nodes: Schema.Array(Node), failures: Schema.Array(Schema.Struct({ file: Schema.String, message: Schema.String })), evaluated: Schema.Array(Schema.String) });
const Reported = Schema.fromJsonString(Report);

// --- [OPERATIONS] ----------------------------------------------------------------------

const external = ({ id, version }: Node['packages'][number]) => `nuget:${id}@${version}`;

const edges = Effect.fnUntraced(function* (reports: readonly (readonly [Evaluated, Report])[], _: CreateDependenciesContext, { plugin, context }: Request<unknown>) {
    const path = yield* Path.Path;
    const nodes = reports.flatMap(([, report]) => report.nodes);
    const compiled = reports.map(([{ core }]) => path.relative(context.workspaceRoot, core));
    const folder = path.relative(context.workspaceRoot, plugin.folder);
    const built = nodes.flatMap((node) => node.modes.flatMap(Struct.get('outputs')).map((output) => ({ file: node.file, output })));
    const pinned = new Map(nodes.map((node) => [node.file, Record.fromIterableWith(node.packages, (reference) => [reference.id, reference] as const)] as const));
    const locked = nodes.flatMap((node) => node.packages.map((reference) => ({ reference, held: pinned.get(node.file) ?? {} })));
    return [
        ...nodes.flatMap((node) => node.references.map((reference) => ({ source: path.dirname(node.file), target: reference.root, ...(reference.declared === node.file ? { type: DependencyType.static, sourceFile: node.file } : { type: DependencyType.implicit }) }) satisfies RawProjectGraphDependency)),
        ...nodes.flatMap((node) => node.packages.filter(({ direct }) => direct).map((reference) => ({ source: path.dirname(node.file), target: external(reference), type: DependencyType.implicit }) satisfies RawProjectGraphDependency)),
        ...locked.flatMap(({ reference, held }) => Array.getSomes(reference.dependencies.map((dependency) => Record.get(held, dependency))).map((target) => ({ source: external(reference), target: external(target), type: DependencyType.static }) satisfies RawProjectGraphDependency)),
        ...Array.dedupe(built.filter(({ output }) => compiled.some((core) => core.startsWith(`${output}/`))).map(({ file }) => file)).map((file) => ({ source: folder, target: path.dirname(file), type: DependencyType.static, sourceFile: path.join(folder, 'package.json') }) satisfies RawProjectGraphDependency),
    ];
});

const analyzed = Effect.fnUntraced(function* ({ core, held }: Evaluated, { files, context: { workspaceRoot } }: Request<unknown>) {
    const report = yield* Effect.flatMap(printed('dotnet', [core, workspaceRoot, ...held], { cwd: workspaceRoot, input: files.join('\n') }), Schema.decodeEffect(Reported));
    const stated = Array.dedupe(report.nodes.flatMap(({ inputs }) => inputs));
    const known = new Set(yield* Effect.promise(() => globWithWorkspaceContext(workspaceRoot, stated)));
    const read = (input: string) => known.has(input) || held.some((root) => input === root || input.startsWith(`${root}/`));
    return {
        report: { ...report, nodes: report.nodes.map((node) => ({ ...node, inputs: node.inputs.filter(read) })) },
        failures: report.failures.map(({ file, message }): readonly [string, Error] => [file, ToolError.make({ subject: file, detail: message })]),
    };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes, createDependencies } = inferred({
    module: import.meta.url,
    files: FILES,
    options: Schema.Struct({}),
    units: ({ files, path, context: { workspaceRoot } }: Request<unknown>) =>
        Effect.map(Effect.all([analyzer, submodules(workspaceRoot)]), ([core, gitlinks]) => {
            const held = gitlinks.map(({ root }) => root);
            return Array.take(
                files.filter((file) => PROJECTS.includes(path.extname(file).slice(1))),
                1,
            ).map((file): Evaluated => ({ key: 'msbuild', file, core, held, salt: [...files, ...held].join('\n') }));
        }),
    read: (missing: Array.NonEmptyReadonlyArray<Evaluated>, request: Request<unknown>) => Effect.map(Effect.result(analyzed(Array.headNonEmpty(missing), request)), (reading) => missing.map(() => reading)),
    sources: ({ evaluated, nodes }: Report, { core }: Evaluated, { files, path, context: { workspaceRoot } }) => ({
        tracked: [...files, ...evaluated, ...nodes.flatMap(({ locks }) => locks)],
        untracked: [path.dirname(core), ...nodes.flatMap(({ restored }) => restored.map((folder) => path.join(workspaceRoot, folder)))],
    }),
    projects: ({ nodes }: Report, _, { path, plugin }) => {
        const releasable = (node: Node) => node.packable || node.publishable;
        const folders = Record.fromIterableWith(nodes, (node) => [path.dirname(node.file), node] as const);
        const carried = (node: Node): readonly NonNullable<Node['releases']>[] => [
            ...Option.toArray(Option.fromNullOr(node.releases)),
            ...Array.getSomes(node.references.map(({ root }) => Record.get(folders, root)))
                .filter((reference) => !releasable(reference))
                .flatMap(carried),
        ];
        return nodes.map((node): Project => {
            const root = path.dirname(node.file);
            const supported = Array.intersection(Record.keys(PLATFORMS), node.platforms).map((name) => PLATFORMS[name]);
            const limited = node.test && supported.length > 0 && supported.length < Record.keys(PLATFORMS).length;
            return {
                file: node.file,
                root,
                name: node.name,
                technologies: ['dotnet', node.language],
                ...targets(plugin, path, { ...node, test: limited ? supported.some((held) => held === platform) : node.test }),
                ...(releasable(node) && { release: { projectFile: path.basename(node.file), releases: Array.dedupeWith(carried(node), Equivalence.mapInput(Equivalence.String, Struct.get('unshipped'))) } }),
                externalNodes: Record.fromIterableWith(node.packages, (reference): readonly [string, ProjectGraphExternalNode] => [external(reference), { type: 'nuget', name: external(reference), data: { version: reference.version, packageName: reference.id, hash: reference.hash } }]),
                configuration: {
                    ...(limited && { tags: supported.map((held) => `os:${held}`) }),
                    ...(node.implicit.length > 0 && { implicitDependencies: [...node.implicit] }),
                },
            };
        });
    },
    edges,
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
