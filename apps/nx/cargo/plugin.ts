import { basename, dirname, join } from 'node:path';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, DependencyType, hashArray, type RawProjectGraphDependency } from '@nx/devkit';
import { combineGlobPatterns, getNamedInputs, globWithWorkspaceContext, hashObject, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { promised } from '@rasm/nx-devkit/process';
import { configured, linked, placed } from '@rasm/nx-devkit/project';
import { Array, Effect, FileSystem, Option, Record, Result, String } from 'effect';
import { CLIPPY, CONFIG, crateInputs, crateTargets, NEXTEST, PLUGIN, released, workspaceTargets } from '#/targets';
import { MANIFEST, type Reading, resolved, workspaces } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = combineGlobPatterns([MANIFEST, ...CONFIG, ...CLIPPY, NEXTEST, '{,.}deny{,.exceptions}.toml'].map((file) => `**/${file}`));
const LAYOUT = ['src/{main,lib}.rs', '{src/bin,examples,tests,benches}/{*.rs,*/main.rs}', 'build.rs'] as const;
const STATED = { metadata: { technologies: ['rust'] }, clean: 'cargo' };

// --- [COMPOSITION] ---------------------------------------------------------------------

const edges = new Map<string, readonly RawProjectGraphDependency[]>();

const createNodes: CreateNodes = [
    FILES,
    (matched, options, context) =>
        promised(
            Effect.gen(function* () {
                const fs = yield* FileSystem.FileSystem;
                const { workspaceRoot } = context;
                const files = new Set(matched);
                const manifests = matched.filter((file) => basename(file) === MANIFEST);
                const cache = new PluginCache<readonly Reading[]>(join(workspaceDataDirectory, `cargo-${hashObject(options ?? {})}.hash`));
                const [content, laid] = yield* Effect.promise(() => Promise.all([hashWithWorkspaceContext(workspaceRoot, [FILES]), globWithWorkspaceContext(workspaceRoot, Array.cartesianWith(manifests.map(dirname), LAYOUT, join))]));
                const key = hashArray([content, ...laid]);
                const [read, failures] = Array.partition(yield* Option.match(Option.fromNullishOr(cache.get(key)), { onNone: () => workspaces(workspaceRoot, manifests, files), onSome: (held) => Effect.succeed(held.map((reading) => [MANIFEST, Result.succeed(reading)] as const)) }), ([file, reading]) =>
                    Result.mapError(reading, (error): [string, Error] => [file, error]),
                );
                yield* Effect.sync(() => (failures.length === 0 ? cache.set(key, read) : undefined));
                yield* Effect.sync(() => cache.writeToDisk());
                const reported = Option.map(placed(options, context)('llvm-cov'), (at) => join(at, 'lcov.info'));
                const found = read.map((reading) => resolved(workspaceRoot, reading));
                edges.set(
                    workspaceRoot,
                    found.flatMap((workspace) =>
                        workspace.members.flatMap(({ crate, root, manifest: sourceFile }) =>
                            workspace.members.filter((other) => crate.dependencies.some(({ path: at }) => at === dirname(other.crate.manifest_path))).map((other): RawProjectGraphDependency => ({ source: root, target: other.root, type: DependencyType.static, sourceFile })),
                        ),
                    ),
                );
                const results = yield* Effect.forEach(
                    found,
                    Effect.fnUntraced(function* (workspace) {
                        const crates = Record.fromIterableWith(workspace.members, (member) => {
                            const implicit = member.crate.metadata?.nx?.implicitDependencies;
                            const { default: sources } = getNamedInputs(member.root, context);
                            const { release, publish } = released(member, workspace.root);
                            return [
                                member.root,
                                {
                                    members: crateTargets(workspace, files, reported, member),
                                    stated: {
                                        ...STATED,
                                        root: member.root,
                                        name: String.kebabToSnake(member.crate.name),
                                        namedInputs: { default: crateInputs(workspace, files, member, sources) },
                                        ...(implicit !== undefined && { implicitDependencies: [...implicit] }),
                                        ...(release !== undefined && { release: { version: { versionActions: `${PLUGIN}/release/version-actions`, versionActionsOptions: release } }, publish }),
                                    },
                                },
                            ] as const;
                        });
                        const own = crates[workspace.root];
                        const rooted = workspaceTargets(workspace);
                        const top = { members: { ...own?.members, check: { ...own?.members.check, ...rooted.check }, clean: rooted.clean, upgrade: rooted.upgrade }, stated: own?.stated ?? { ...STATED, root: workspace.root } };
                        const held = own !== undefined || (yield* fs.exists(join(workspaceRoot, workspace.root, 'package.json')));
                        const projects = yield* Effect.all(Record.map({ ...crates, ...(held && { [workspace.root]: top }) }, ({ members, stated }) => configured(members, stated, context)));
                        return [join(workspace.root, MANIFEST), { projects }] as const;
                    }),
                );
                return yield* failures.length === 0 ? Effect.succeed(results) : Effect.fail(new AggregateCreateNodesError(failures, results));
            }),
        ),
];

const createDependencies: CreateDependencies = (_, context) => linked(context, edges.get(context.workspaceRoot) ?? []);

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
