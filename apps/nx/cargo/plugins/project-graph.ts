import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesContext, type CreateNodesResultArray, DependencyType, hashArray, type RawProjectGraphDependency } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, getNamedInputs, hashObject, hashWithWorkspaceContext, isCI, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Effect, Option, Path, Record, Result } from 'effect';
import manifest from '../package.json' with { type: 'json' };
import { aggregated, crateTargets, released, TECHNOLOGIES, workspaceTargets } from './targets.ts';
import { externals, MANIFEST, workspaces } from './workspace.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Graph {
    readonly nodes: CreateNodesResultArray;
    readonly edges: readonly RawProjectGraphDependency[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = `**/{${[MANIFEST, 'Cargo.lock', 'rust-toolchain.toml', 'rust-toolchain', '.cargo/config.toml', '.cargo/config', 'clippy.toml', '.clippy.toml', '.config/nextest.toml', 'deny.toml', '.deny.toml', '.cargo/deny.toml'].join(',')}}`;
const { name } = manifest;

// --- [OPERATIONS] ----------------------------------------------------------------------

const analyze = Effect.fnUntraced(function* (context: CreateNodesContext, matched: readonly string[]) {
    const path = yield* Path.Path;
    const files = new Set(matched);
    const manifests = matched.filter((file) => path.basename(file) === MANIFEST).toSorted((left, right) => left.split('/').length - right.split('/').length || left.localeCompare(right));
    const results = yield* workspaces(context.workspaceRoot, manifests, files);
    const found = results.flatMap(([, held]) => (Result.isSuccess(held) ? [held.success] : []));
    const parts = found.map((workspace) => {
        const rooted = workspace.metadata.packages.map((crate) => ({ crate, root: path.relative(context.workspaceRoot, path.dirname(crate.manifest_path)) || '.' }));
        const byName = new Map(rooted.map(({ crate, root }) => [crate.name, root] as const));
        const lock = Option.map(workspace.lock, externals);
        const crates = Record.fromIterableWith(rooted, ({ crate, root }) => {
            const named = getNamedInputs(root, context);
            const { release, targets } = released(crate, workspace.root);
            return [
                root,
                {
                    name: crate.name,
                    metadata: { technologies: TECHNOLOGIES },
                    ...(release === undefined ? {} : { release }),
                    targets: { ...crateTargets(path, workspace, files, 'production' in named ? ['production', '^production'] : ['default', '^default'], crate, root), ...targets },
                },
            ] as const;
        });
        const own = crates[workspace.root];
        const projects = Record.map(
            { ...crates, [workspace.root]: { name: path.basename(path.resolve(context.workspaceRoot, workspace.root)), metadata: { technologies: TECHNOLOGIES }, ...own, targets: { ...own?.targets, ...workspaceTargets(path, workspace) } } },
            (project) => ({ ...project, targets: aggregated(project.targets) }),
        );
        const edges: readonly RawProjectGraphDependency[] = [
            ...rooted.flatMap(({ crate, root }) =>
                crate.dependencies.flatMap(({ path: at }) => Option.toArray(Option.fromUndefinedOr(at)).map((dependency): RawProjectGraphDependency => ({ source: root, target: path.relative(context.workspaceRoot, dependency) || '.', type: DependencyType.implicit }))),
            ),
            ...Option.match(lock, { onNone: () => [], onSome: ({ edges: locked }) => locked.flatMap((edge): readonly RawProjectGraphDependency[] => (edge.type === DependencyType.implicit ? Option.toArray(Option.map(Option.fromUndefinedOr(byName.get(edge.source)), (root) => ({ ...edge, source: root }))) : [edge])) }),
        ];
        return {
            node: [path.join(workspace.root, MANIFEST), { projects, ...Option.match(lock, { onNone: () => ({}), onSome: ({ nodes }) => ({ externalNodes: nodes }) }) }] as const,
            edges,
        };
    });
    return {
        graph: { nodes: parts.map(({ node }) => node), edges: parts.flatMap(({ edges }) => edges) } satisfies Graph,
        failures: results.flatMap(([file, held]) => (Result.isFailure(held) ? [[file, held.failure] as const] : [])),
    };
});

const reports = new Map<string, Graph>();

const keyed = (workspaceRoot: string, options: unknown) => `${workspaceRoot}#${hashObject(options ?? {})}`;

const graph = Effect.fnUntraced(function* (context: CreateNodesContext, matched: readonly string[], options: unknown) {
    const path = yield* Path.Path;
    const hash = hashArray([yield* Effect.promise(() => hashWithWorkspaceContext(context.workspaceRoot, [FILES, path.join(path.relative(context.workspaceRoot, path.dirname(import.meta.dirname)), '**/*.{ts,json}')])), manifest.version, hashObject(context.nxJsonConfiguration.namedInputs ?? {}), String(Boolean(isCI()))]);
    const cache = new PluginCache<Graph>(path.join(workspaceDataDirectory, `${path.basename(name)}-${hashObject(options ?? {})}.hash`));
    const built = yield* Option.match(Option.fromNullishOr(cache.get(hash)), {
        onSome: (held) => Effect.succeed({ graph: held, failures: [] }),
        onNone: () => Effect.tap(analyze(context, matched.toSorted()), ({ graph: report, failures }) => (failures.length === 0 ? Effect.sync(() => (cache.set(hash, report), cache.writeToDisk())) : Effect.void)),
    });
    reports.set(keyed(context.workspaceRoot, options), built.graph);
    return built;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    FILES,
    (files, options, context) =>
        Effect.runPromise(
            Effect.flatMap(graph(context, files, options), ({ graph: built, failures }) =>
                failures.length === 0 ? Effect.succeed(built.nodes) : Effect.fail(new AggregateCreateNodesError(failures.map(([file, error]) => [file, new Error(error.message)]), [...built.nodes])),
            ).pipe(Effect.provide(layer)),
        ),
];

const createDependencies: CreateDependencies = (options, { workspaceRoot, projects, externalNodes }) => {
    const names = createProjectRootMappingsFromProjectConfigurations(projects);
    const resolved = (at: string) => Option.orElse(Option.fromUndefinedOr(names.get(at)), () => Option.liftPredicate(at, (node) => node in (externalNodes ?? {})));
    return Record.values(
        Record.fromIterableWith(
            (reports.get(keyed(workspaceRoot, options))?.edges ?? []).flatMap((edge) =>
                Option.toArray(Option.all([resolved(edge.source), resolved(edge.target)])).flatMap(([source, target]) => (source === target ? [] : [{ ...edge, source, target }])),
            ),
            (edge) => [`${edge.source} ${edge.target}`, edge] as const,
        ),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes, name };
