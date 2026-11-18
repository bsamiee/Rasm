import { dirname, join, posix, relative } from 'node:path';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesContext, DependencyType, hashArray, normalizePath, type ProjectConfiguration } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, getNamedInputs, globWithWorkspaceContext, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { anchored, fingerprinted, manifested } from '@rasm/nx-distribution/inference';
import { Array, Effect, Option, Record, Result, Schema, String } from 'effect';
import { crateInputs, Options, projectTargets, released } from '#/targets';
import { CLIPPY, CONFIG, DENY, INSTALL, Links, MANIFEST, type Member, NEXTEST, placed, type Reading, resolved, run, workspaces } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = combineGlobPatterns([MANIFEST, ...CONFIG, ...CLIPPY, NEXTEST, ...Object.values(DENY).flat()].map((file) => `**/${file}`));
const LAYOUT = ['src/{main,lib}.rs', '{src/bin,examples,tests,benches}/{*.rs,*/main.rs}', 'build.rs'] as const;
const OWN = fingerprinted(import.meta.dirname);

// --- [OPERATIONS] ----------------------------------------------------------------------

const inferred = Effect.fnUntraced(function* (context: CreateNodesContext, files: ReadonlySet<string>, settings: typeof Options.Type, listed: (root: string) => boolean, reading: Reading) {
    const links = yield* Links;
    const workspace = resolved(context.workspaceRoot, reading, files, yield* INSTALL);
    const coverage = Option.map(Option.fromUndefinedOr(settings.coveragePath), (at) => posix.join(at, 'lcov.info'));
    workspace.members.forEach((member) => {
        links.set(dirname(member.crate.manifest_path), member);
    });
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
    return { projects };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = [
    FILES,
    (matched, options, context) =>
        run(
            Effect.gen(function* () {
                const { workspaceRoot } = context;
                const settings = yield* Schema.decodeEffect(Options)(options ?? {});
                const files = new Set(matched);
                const listed = anchored(workspaceRoot);
                const manifests = matched.filter((file) => posix.basename(file) === MANIFEST);
                const cache = new PluginCache<{ readonly digest: string; readonly readings: readonly Reading[] }>(join(workspaceDataDirectory, 'cargo.hash'));
                const { stored, loaded } = placed(workspaceRoot);
                const [content, laid] = yield* Effect.promise(() => Promise.all([hashWithWorkspaceContext(workspaceRoot, [FILES]), globWithWorkspaceContext(workspaceRoot, Array.cartesianWith(manifests.map(posix.dirname), LAYOUT, posix.join))]));
                const digest = hashArray([content, ...laid, yield* OWN]);
                const hit = Option.filter(Option.fromNullishOr(cache.get(FILES)), (held) => held.digest === digest);
                const read = yield* Option.match(hit, {
                    onNone: () => workspaces(workspaceRoot, manifests, files),
                    onSome: ({ readings }) => Effect.succeed(readings.map(loaded).map((reading) => [posix.join(normalizePath(relative(workspaceRoot, reading.metadata.workspace_root)), MANIFEST), Result.succeed(reading)] as const)),
                });
                const failures = Array.filterMap(read, ([file, reading]) => Result.map(Result.flip(reading), (error): [string, Error] => [file, error]));
                yield* failures.length > 0 || Option.isSome(hit)
                    ? Effect.void
                    : Effect.sync(() => {
                          cache.set(FILES, { digest, readings: Array.getSuccesses(read.map(([, reading]) => reading)).map(stored) });
                          cache.writeToDisk();
                      });
                const results = yield* Effect.forEach(
                    Array.filterMap(read, ([file, reading]) => Result.map(reading, (held) => [file, held] as const)),
                    ([file, held]) => Effect.map(inferred(context, files, settings, listed, held), (result) => [file, result] as const),
                );
                return yield* failures.length === 0 ? Effect.succeed(results) : Effect.fail(new AggregateCreateNodesError(failures, results));
            }),
        ),
];

const createDependencies: CreateDependencies = (_, { projects, workspaceRoot }) =>
    run(
        Effect.map(Links, (links) => {
            const names = createProjectRootMappingsFromProjectConfigurations(projects);
            const named = ({ root }: Member) => Option.fromUndefinedOr(names.get(root));
            const members = links
                .values()
                .filter(({ crate }) => !relative(workspaceRoot, crate.manifest_path).startsWith('..'))
                .toArray();
            const stated = members.flatMap((member) => Array.getSomes(member.crate.dependencies.map(({ path }) => Option.fromUndefinedOr(path))).map((at) => ({ member, to: Option.fromUndefinedOr(links.get(at)) })));
            return Array.getSomes(stated.map(({ member, to }) => Option.map(Option.all({ source: named(member), target: Option.flatMap(to, named) }), (edge) => ({ ...edge, type: DependencyType.static, sourceFile: member.manifest }))));
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
