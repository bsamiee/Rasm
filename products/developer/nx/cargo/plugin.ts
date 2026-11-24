import { dirname, join, posix } from 'node:path';
import { arch, platform } from 'node:process';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesContext, DependencyType, hashArray, type ProjectConfiguration } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, getNamedInputs, globWithWorkspaceContext, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { anchored, fingerprinted, inferring, manifested } from '@rasm/nx-distribution/inference';
import { Array, Config, Effect, identity, Option, Record, String } from 'effect';
import { crateInputs, Options, projectTargets, released } from '#/targets';
import { CLIPPY, CONFIG, DENY, Links, MANIFEST, type Member, NEXTEST, placed, type Reading, resolved, run, type Workspace, workspaces } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = combineGlobPatterns([MANIFEST, ...CONFIG, ...CLIPPY, NEXTEST, ...Object.values(DENY).flat()].map((file) => `**/${file}`));
const LAYOUT = ['src/{main,lib}.rs', '{src/bin,examples,tests,benches}/{*.rs,*/main.rs}', 'build.rs', '{README,README.md,README.txt}'] as const;
const PLACED = ['CARGO_TARGET_DIR', 'CARGO_BUILD_TARGET_DIR'] as const;
const OWN = fingerprinted(import.meta.dirname);

// --- [OPERATIONS] ----------------------------------------------------------------------

const inferred = Effect.fnUntraced(function* (context: CreateNodesContext, { coveragePath }: Options, listed: (root: string) => boolean, workspace: Workspace) {
    const coverage = Option.map(Option.fromUndefinedOr(coveragePath), (at) => posix.join(at, 'lcov.info'));
    const roots = [...new Map<string, Option.Option<Member>>([[workspace.root, Option.none()], ...workspace.members.map((member) => [member.root, Option.some(member)] as const)])];
    const manifests = yield* Effect.forEach(roots, ([root]) => Effect.transposeOption(Option.liftPredicate(manifested(context.workspaceRoot, root), () => listed(root))));
    const present = Array.zip(roots, manifests).filter(([[, member], stated]) => Option.isSome(member) || Option.isSome(stated));
    const projects = Record.fromIterableWith(present, ([[root, member], manifest]): [string, ProjectConfiguration] => {
        const stated = Option.getOrElse(manifest, () => ({}));
        const targets = projectTargets(workspace, coverage, member, stated);
        const crate = Option.map(member, (held) => {
            const { default: sources } = getNamedInputs(root, context);
            const implicit = held.crate.metadata?.nx?.implicitDependencies;
            const delivery = released(held, workspace.root);
            return {
                name: String.kebabToSnake(held.crate.name),
                namedInputs: { default: crateInputs(workspace, held, sources) },
                ...(implicit !== undefined && { implicitDependencies: [...implicit] }),
                ...delivery,
                targets: { ...targets, ...delivery.targets },
            };
        });
        return [root, { root, metadata: { technologies: ['rust'] }, targets, ...Option.getOrUndefined(crate) }];
    });
    return [posix.join(workspace.root, MANIFEST), { projects }] as const;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    FILES,
    inferring(Options, run, function* (matched, settings, context) {
        const { workspaceRoot } = context;
        const files = new Set(matched);
        const listed = anchored(workspaceRoot);
        const manifests = matched.filter((file) => posix.basename(file) === MANIFEST);
        const cache = new PluginCache<{ readonly digest: string; readonly readings: readonly Reading[] }>(join(workspaceDataDirectory, 'cargo.hash'));
        const { stored, loaded } = placed(workspaceRoot);
        const [content, laid, targeted] = yield* Effect.all(
            [Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...matched])), Effect.promise(() => globWithWorkspaceContext(workspaceRoot, Array.cartesianWith(manifests.map(posix.dirname), LAYOUT, posix.join))), Effect.forEach(PLACED, (name) => Config.String(name).pipe(Config.withDefault('')))],
            { concurrency: 'unbounded' },
        );
        const digest = hashArray([content, ...laid, ...targeted, platform, arch, yield* OWN]);
        const hit = Option.filter(Option.fromNullishOr(cache.get(FILES)), (held) => held.digest === digest);
        const [readings, failures] = yield* Option.match(hit, {
            onNone: () => Effect.map(workspaces(workspaceRoot, manifests, files), (read) => Array.partition(read, identity)),
            onSome: (held) => Effect.succeed([held.readings.map(loaded), Array.empty<[string, Error]>()] as const),
        });
        yield* failures.length > 0 || Option.isSome(hit)
            ? Effect.void
            : Effect.sync(() => {
                  cache.set(FILES, { digest, readings: readings.map(stored) });
                  cache.writeToDisk();
              });
        const resolutions = readings.map((reading) => resolved(workspaceRoot, reading, files));
        (yield* Links).set(workspaceRoot, new Map(resolutions.flatMap(({ members }) => members.map((member) => [dirname(member.crate.manifest_path), member] as const))));
        const results = yield* Effect.forEach(resolutions, (workspace) => inferred(context, settings, listed, workspace));
        return yield* failures.length === 0 ? Effect.succeed(results) : Effect.fail(new AggregateCreateNodesError(failures, results));
    }),
];

const createDependencies: CreateDependencies = (_, { projects, workspaceRoot }) =>
    run(
        Effect.map(Links, (links) => {
            const names = createProjectRootMappingsFromProjectConfigurations(projects);
            const named = ({ root }: Member) => Option.fromUndefinedOr(names.get(root));
            const linked = links.get(workspaceRoot) ?? new Map<string, Member>();
            const stated = linked
                .values()
                .flatMap((member) => Array.getSomes(member.crate.dependencies.map(({ path }) => Option.fromUndefinedOr(path))).map((at) => ({ member, to: Option.fromUndefinedOr(linked.get(at)) })))
                .toArray();
            return Array.getSomes(stated.map(({ member, to }) => Option.map(Option.all({ source: named(member), target: Option.flatMap(to, named) }), (edge) => ({ ...edge, type: DependencyType.static, sourceFile: member.manifest }))));
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
