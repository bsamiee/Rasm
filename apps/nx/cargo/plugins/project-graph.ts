import { type CreateDependencies, type CreateNodes, type CreateNodesContext, type CreateNodesResultArray, hashArray, logger, type RawProjectGraphDependency } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, getNamedInputs, hashObject, hashWithWorkspaceContext } from '@nx/devkit/internal';
import { aggregated, cached, identity, inferred, reported } from '@rasm/nx-devkit/inference';
import { present } from '@rasm/nx-devkit/process';
import { Array, Effect, Option, Path, Record, Result } from 'effect';
import { crateTargets, released, TECHNOLOGIES, unresolved, workspaceTargets } from '#/plugins/targets';
import { CONFIG, DENY, MANIFEST, Manifest, TOOLCHAIN, toml, workspaces } from '#/plugins/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

interface Graph {
    readonly nodes: CreateNodesResultArray;
    readonly edges: readonly RawProjectGraphDependency[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = `**/{${[MANIFEST, ...TOOLCHAIN, ...CONFIG, 'clippy.toml', '.clippy.toml', '.config/nextest.toml', ...Object.values(DENY).flat()].join(',')}}`;

// --- [OPERATIONS] ----------------------------------------------------------------------

const analyze = Effect.fnUntraced(function* (context: CreateNodesContext, plugin: Parameters<typeof released>[0], matched: readonly string[]) {
    const path = yield* Path.Path;
    const files = new Set(matched);
    const manifests = matched.filter((file) => path.basename(file) === MANIFEST).toSorted((left, right) => left.split('/').length - right.split('/').length || left.localeCompare(right));
    const installed = Option.isSome(yield* present('cargo', ['--version']));
    const [resolved, failures] = Array.partition(installed ? yield* workspaces(context.workspaceRoot, manifests, files) : [], ([file, result]) => Result.mapError(result, (error) => [file, error] as const));
    const parts = resolved.map((workspace) => {
        const crates = Record.fromIterableWith(workspace.members, ({ crate, root }) => {
            const named = getNamedInputs(root, context);
            const { targets, ...configured } = released(plugin, path, crate, workspace.root);
            return [
                root,
                {
                    name: crate.name,
                    metadata: { technologies: TECHNOLOGIES },
                    ...configured,
                    members: crateTargets(plugin, path, workspace, files, 'production' in named ? ['production', '^production'] : ['default', '^default'], crate, root),
                    published: targets,
                },
            ] as const;
        });
        const own = crates[workspace.root];
        const rooted = workspaceTargets(plugin, path, workspace);
        const projects = Record.map({ ...crates, [workspace.root]: { metadata: { technologies: TECHNOLOGIES }, published: {}, ...own, members: { ...own?.members, check: { ...own?.members.check, ...rooted.check }, upgrade: rooted.upgrade } } }, ({ members, published, ...project }) => ({
            ...project,
            targets: { ...aggregated(members, TECHNOLOGIES, { executor: plugin.cleaner, tool: 'cargo' }), ...published },
        }));
        return { node: [path.join(workspace.root, MANIFEST), { projects }] as const, edges: workspace.edges };
    });
    const fallbacks = yield* Effect.forEach(installed ? [] : manifests, (file) =>
        toml(Manifest, path.join(context.workspaceRoot, file)).pipe(
            Effect.map(({ package: declared }) => (declared === undefined ? {} : { name: declared.name })),
            Effect.orElseSucceed(() => ({})),
            Effect.map((named) => [file, { projects: { [path.dirname(file)]: { ...named, metadata: { technologies: TECHNOLOGIES }, targets: aggregated(unresolved(plugin, path.dirname(file)), TECHNOLOGIES) } } }] as const),
        ),
    );
    yield* Effect.sync(() => {
        if (!installed && manifests.length > 0) {
            logger.warn(`cargo is absent on this host, so ${manifests.length} Cargo manifests keep check--cargo alone, and its runs report the missing tool`);
        }
    });
    return { graph: { nodes: [...parts.map(({ node }) => node), ...fallbacks], edges: parts.flatMap(({ edges }) => edges) } satisfies Graph, failures, complete: installed && failures.length === 0 };
});

const reports = new Map<string, Graph>();

const keyed = (workspaceRoot: string, options: unknown) => hashArray([workspaceRoot, hashObject(options ?? {})]);

const graph = Effect.fnUntraced(function* (context: CreateNodesContext, matched: readonly string[], options: unknown) {
    const plugin = yield* identity(import.meta.url, context.workspaceRoot);
    const hash = hashArray([yield* Effect.promise(() => hashWithWorkspaceContext(context.workspaceRoot, [FILES])), plugin.digest, hashObject(context.nxJsonConfiguration.namedInputs ?? {})]);
    const cache = yield* cached<Graph>(plugin, options);
    const built = yield* Option.match(yield* cache.get(FILES, () => Effect.succeed(hash)), {
        onSome: (held) => Effect.succeed({ graph: held, failures: Array.empty<readonly [string, Error]>() }),
        onNone: () => Effect.tap(analyze(context, plugin, matched.toSorted()), ({ graph: report, complete }) => (complete ? cache.set(FILES, report, hash) : Effect.void)),
    });
    reports.set(keyed(context.workspaceRoot, options), built.graph);
    return yield* reported(built.graph.nodes, built.failures);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [FILES, (files, options, context) => inferred(graph(context, files, options))];

const createDependencies: CreateDependencies = (options, context) => {
    const names = createProjectRootMappingsFromProjectConfigurations(context.projects);
    return Array.getSomes((reports.get(keyed(context.workspaceRoot, options))?.edges ?? []).map((edge) => Option.map(Option.all([Option.fromUndefinedOr(names.get(edge.source)), Option.fromUndefinedOr(names.get(edge.target))]), ([source, target]) => ({ ...edge, source, target }))));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
