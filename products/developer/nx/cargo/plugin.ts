import { dirname, join, posix } from 'node:path';
import { arch, platform } from 'node:process';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesContext, DependencyType, hashArray, type ProjectConfiguration } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, getNamedInputs, globWithWorkspaceContext, hashWithWorkspaceContext, mergeTargetConfigurations, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { type Anchor, anchored, fingerprinted, inferring, manifested } from '@rasm/nx/inference';
import { Array, Config, Effect, identity, Option, Record, Schema, String, Struct } from 'effect';
import { crateInputs, projectTargets, released } from '#/targets';
import { CLIPPY, CONFIG, DENY, Links, MANIFEST, type Member, NEXTEST, placed, type Reading, resolved, run, type Workspace, workspaces } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONFIGURED = [...CONFIG, ...CLIPPY, NEXTEST, ...Object.values(DENY).flat()].map((file) => `**/${file}`);
const LAYOUT = ['src/{main,lib}.rs', '{src/bin,examples,tests,benches}/{*.rs,*/main.rs}', 'build.rs', '{README,README.md,README.txt}'] as const;
const VARIABLES = ['CARGO_TARGET_DIR', 'CARGO_BUILD_TARGET_DIR', 'CARGO_BUILD_TARGET', 'CARGO_RESOLVER_FEATURE_UNIFICATION', 'CARGO_UNSTABLE_FEATURE_UNIFICATION', 'CLIPPY_CONF_DIR'] as const;
const OWN = fingerprinted(import.meta.dirname);

// --- [OPERATIONS] ----------------------------------------------------------------------

const inferred = Effect.fnUntraced(function* (context: CreateNodesContext, anchor: Anchor, workspace: Workspace) {
    const roots = [...new Map<string, Option.Option<Member>>([[workspace.root, Option.none()], ...workspace.members.map((member) => [member.root, Option.some(member)] as const)])];
    const manifests = yield* Effect.forEach(roots, ([root]) => Effect.transposeOption(Option.liftPredicate(manifested(context.workspaceRoot, root), () => anchor.holds(root))));
    const present = Array.zip(roots, manifests).filter(([[, member], stated]) => Option.isSome(member) || Option.isSome(stated));
    const projects = Record.fromIterableWith(present, ([[root, member], manifest]): [string, ProjectConfiguration] => {
        const declared = Option.flatMapNullishOr(member, (held) => held.crate.metadata?.nx?.targets).pipe(Option.getOrElse(Record.empty));
        const stated = Record.union(declared, Option.getOrElse(manifest, Record.empty), (own, written) => mergeTargetConfigurations(written, own));
        const delivery = Option.map(member, (held) => released(held, workspace.root));
        const targets = Record.union(projectTargets(workspace, member, stated, anchor, Option.match(delivery, { onNone: () => ({}), onSome: Struct.get('targets') })), declared, (derived, own) => mergeTargetConfigurations(own, derived));
        const crate = Option.zipWith(member, delivery, (held, shipped) => {
            const { default: sources } = getNamedInputs(root, context);
            const implicit = held.crate.metadata?.nx?.implicitDependencies;
            return {
                name: String.kebabToSnake(held.crate.name),
                namedInputs: { default: crateInputs(workspace, held, sources) },
                ...(implicit !== undefined && { implicitDependencies: [...implicit] }),
                ...shipped,
                targets,
            };
        });
        return [root, { root, metadata: { technologies: ['rust'] }, targets, ...Option.getOrUndefined(crate) }];
    });
    return [posix.join(workspace.root, MANIFEST), { projects }] as const;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    `**/${MANIFEST}`,
    inferring(Schema.Struct({}), run, function* (manifests, _, context) {
        const { workspaceRoot } = context;
        const anchor = anchored(workspaceRoot);
        const cache = new PluginCache<{ readonly digest: string; readonly readings: readonly Reading[] }>(join(workspaceDataDirectory, 'cargo.hash'));
        const { stored, loaded } = placed(workspaceRoot);
        const [content, configured, laid, stated] = yield* Effect.all(
            [
                Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...manifests, ...CONFIGURED])),
                Effect.promise(() => globWithWorkspaceContext(workspaceRoot, CONFIGURED)),
                Effect.promise(() => globWithWorkspaceContext(workspaceRoot, Array.cartesianWith(manifests.map(posix.dirname), LAYOUT, posix.join))),
                Effect.forEach(VARIABLES, (name) => Config.String(name).pipe(Config.withDefault(''))),
            ],
            { concurrency: 'unbounded' },
        );
        const files = new Set([...manifests, ...configured]);
        const digest = hashArray([content, ...laid, ...stated, platform, arch, yield* OWN]);
        const hit = Option.filter(Option.fromNullishOr(cache.get(MANIFEST)), (held) => held.digest === digest);
        const [readings, failures] = yield* Option.match(hit, {
            onNone: () => Effect.map(workspaces(workspaceRoot, manifests, files), (read) => Array.partition(read, identity)),
            onSome: (held) => Effect.succeed([held.readings.map(loaded), Array.empty<[string, Error]>()] as const),
        });
        if (failures.length === 0 && Option.isNone(hit)) {
            cache.set(MANIFEST, { digest, readings: readings.map(stored) });
            cache.writeToDisk();
        }
        const resolutions = readings.map((reading) => resolved(workspaceRoot, reading, files));
        (yield* Links).set(workspaceRoot, new Map(resolutions.flatMap(({ members }) => members.map((member) => [dirname(member.crate.manifest_path), member] as const))));
        const results = yield* Effect.forEach(resolutions, (workspace) => inferred(context, anchor, workspace));
        return yield* failures.length === 0 ? Effect.succeed(results) : Effect.fail(new AggregateCreateNodesError(failures, results));
    }),
];

const createDependencies: CreateDependencies = (_, { projects, workspaceRoot }) =>
    run(
        Effect.map(Links, (links) => {
            const names = createProjectRootMappingsFromProjectConfigurations(projects);
            const linked = links.get(workspaceRoot) ?? new Map<string, Member>();
            const named = (at: string | undefined) => Option.flatMapNullishOr(Option.fromUndefinedOr(at), (path) => linked.get(path)?.root).pipe(Option.flatMapNullishOr((root) => names.get(root)));
            return Array.getSomes(linked.values().flatMap((member) => member.crate.dependencies.map(({ path }) => Option.map(Option.all({ source: Option.fromUndefinedOr(names.get(member.root)), target: named(path) }), Struct.assign({ type: DependencyType.static, sourceFile: member.manifest })))));
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
