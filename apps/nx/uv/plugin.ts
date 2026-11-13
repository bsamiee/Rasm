import { basename, dirname, join } from 'node:path';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type RawProjectGraphDependency } from '@nx/devkit';
import { combineGlobPatterns, hashObject, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { printed, promised, ToolError } from '@rasm/nx-devkit/process';
import { configured, linked, placed } from '@rasm/nx-devkit/project';
import { Effect, Option, Record, Schema } from 'effect';
import { CONFIGS, documents, targets } from '#/targets';
import { folders, LOCK, located, Metadata, report } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Reading = readonly (readonly [lock: string, metadata: typeof Metadata.Type])[];

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = combineGlobPatterns([LOCK, 'uv.toml', '.python-version', ...CONFIGS].map((file) => `**/${file}`));

// --- [COMPOSITION] ---------------------------------------------------------------------

const edges = new Map<string, readonly RawProjectGraphDependency[]>();

const createNodes: CreateNodes = [
    FILES,
    (matched, options, context) =>
        promised(
            Effect.gen(function* () {
                const { workspaceRoot } = context;
                const cache = new PluginCache<Reading>(join(workspaceDataDirectory, `uv-${hashObject(options ?? {})}.hash`));
                const key = yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [FILES]));
                const [read, unlocked] = yield* Option.match(Option.fromNullishOr(cache.get(key)), {
                    onSome: (held) => Effect.succeed([held, []] as const),
                    onNone: () =>
                        Effect.partition(
                            matched.filter((file) => basename(file) === LOCK),
                            (lock) =>
                                printed('uv', ['workspace', 'metadata', '--frozen', '--preview-features', 'workspace-metadata', '--quiet'], { cwd: join(workspaceRoot, dirname(lock)) }).pipe(
                                    Effect.flatMap(Schema.decodeEffect(Metadata)),
                                    Effect.mapBoth({ onFailure: (error): [string, Error] => [lock, ToolError.of(`uv workspace metadata for ${lock}`, 'does not decode')(error)], onSuccess: (metadata) => [lock, metadata] as const }),
                                ),
                            { concurrency: 'unbounded' },
                        ),
                });
                yield* Effect.sync(() => (unlocked.length === 0 ? cache.set(key, read) : undefined));
                yield* Effect.sync(() => cache.writeToDisk());
                const { documented, unparsed } = yield* documents(
                    workspaceRoot,
                    read.flatMap(([, metadata]) => folders(workspaceRoot, metadata)),
                );
                const workspaces = yield* Effect.forEach(read, ([lock, metadata]) => located(workspaceRoot, report(workspaceRoot, lock, metadata, documented)), { concurrency: 'unbounded' });
                edges.set(
                    workspaceRoot,
                    workspaces.flatMap((workspace) => workspace.edges),
                );
                const results = yield* Effect.forEach(workspaces, ({ lock, members }) =>
                    Effect.map(
                        Effect.all(
                            Record.fromIterableWith(members, (member) => {
                                const { members: tools, ...released } = targets(member, placed(options, context));
                                return [
                                    member.root,
                                    configured(
                                        tools,
                                        {
                                            root: member.root,
                                            name: member.name,
                                            metadata: { technologies: ['python'] },
                                            clean: 'uv',
                                            ...released,
                                            ...(member.published && { release: { version: { versionActions: '@rasm/nx-uv/release/version-actions', versionActionsOptions: { lock: member.lock } } } }),
                                            ...(member.implicit !== undefined && { implicitDependencies: [...member.implicit] }),
                                        },
                                        context,
                                    ),
                                ] as const;
                            }),
                        ),
                        (projects) => [lock, { projects }] as const,
                    ),
                );
                const failures: readonly [string, Error][] = [...unlocked, ...unparsed.map(([file, error]): [string, Error] => [file, error]), ...workspaces.flatMap((workspace) => workspace.failures.map(([file, error]): [string, Error] => [file, error]))];
                return yield* failures.length === 0 ? Effect.succeed(results) : Effect.fail(new AggregateCreateNodesError([...failures], results));
            }),
        ),
];

const createDependencies: CreateDependencies = (_, context) => linked(context, edges.get(context.workspaceRoot) ?? []);

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
