import { join, posix } from 'node:path';
import { AggregateCreateNodesError, type CreateNodes, type ProjectConfiguration } from '@nx/devkit';
import { emitPluginWorkerLog } from '@nx/devkit/internal';
import { anchored, fingerprinted, inferring, manifested, Named, run, spawned } from '@rasm/nx/inference';
import { Array, Effect, FileSystem, Option, type Record, Schema, String, Struct } from 'effect';
import { multiGlobWithWorkspaceContext } from 'nx/src/utils/workspace-context';
import which from 'which';
import plugin from '#/package.json' with { type: 'json' };
import { Options, targets } from '#/targets';
import { LOCK, located, Metadata, SCHEMA } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const OWN = fingerprinted(import.meta.dirname);
const METADATA = ['uv', '--color', 'never', 'workspace', 'metadata', '--frozen', '--preview-features', 'workspace-metadata', '--output-version', SCHEMA, '--quiet'] as const;
const LISTED = ['uv', '--color', 'never', 'workspace', 'list'] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const asked = (cwd: string) =>
    Effect.matchEffect(spawned(METADATA, { cwd, env: { UV_PROJECT_ENVIRONMENT: posix.join(LOCK, 'environment') } }), {
        onSuccess: (reply) => Effect.map(Schema.decodeEffect(Metadata)(reply), (metadata) => ({ metadata, refused: Array.empty<string>() })),
        onFailure: (refusal) =>
            Effect.mapBoth(Effect.all([spawned(LISTED, { cwd }), spawned([...LISTED, '--paths'], { cwd })], { concurrency: 'unbounded' }), {
                onFailure: () => refusal,
                onSuccess: ([names, paths]) => ({
                    metadata: { workspace: { path: cwd, id: cwd }, members: Array.zipWith(names.split('\n').filter(String.isNonEmpty), paths.split('\n').filter(String.isNonEmpty), (name, path) => ({ name, path, id: name })), resolution: {} },
                    refused: [refusal.message],
                }),
            }),
    });

const read = Effect.fnUntraced(function* (workspaceRoot: string, lock: string) {
    const { metadata, refused } = yield* asked(join(workspaceRoot, posix.dirname(lock)));
    return { ...(yield* located(workspaceRoot, lock, metadata)), refused: refused.map((refusal) => `${lock} gives uv no workspace metadata, so its members hold no checker, test, or build target until \`uv lock\` rewrites it: ${refusal}`) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    `**/${LOCK}`,
    inferring(Options, run, function* (locks, settings, context, cached) {
        const fs = yield* FileSystem.FileSystem;
        const found = yield* Effect.forEach(Array.fromNullishOr(yield* Effect.promise(() => which('uv', { nothrow: true }))), (at) => fs.realPath(at));
        const stats = yield* Effect.forEach(found, (at) => fs.stat(at));
        const binary = [...found, ...stats.flatMap(({ size, mtime }) => [`${size}`, ...Option.toArray(mtime).map(globalThis.String)])];
        const pass = yield* cached<Effect.Success<ReturnType<typeof read>>>('uv.hash', yield* OWN, () => Effect.succeed(binary));
        const [models, refused] = yield* Effect.partition(locks, (lock) => Effect.mapError(pass.recall(lock, read(context.workspaceRoot, lock)), (error): [string, Error] => [lock, error]), { concurrency: 'unbounded' });
        const members = models.flatMap((model) => model.members);
        const patterns = Array.dedupe(members.flatMap(({ checks }) => checks.flatMap((check) => check.patterns)));
        const listed = yield* Effect.promise(() => multiGlobWithWorkspaceContext(context.workspaceRoot, patterns));
        const held = patterns.filter((_, index) => Array.isReadonlyArrayNonEmpty(listed[index] ?? []));
        const [roots, anchor] = [new Set(members.map(({ root }) => root)), anchored(context.workspaceRoot)];
        const stated = yield* Effect.forEach(members, ({ root }) => manifested(context.workspaceRoot, root), { concurrency: 'unbounded' });
        const supplied = yield* Effect.filter(
            members.filter(({ listed: uv }) => !uv),
            ({ root }) => Effect.zipWith(fs.exists(join(context.workspaceRoot, root, 'project.json')), Effect.isSuccess(Effect.flatMap(fs.readFileString(join(context.workspaceRoot, root, 'package.json')), Schema.decodeEffect(Named))), (project, manifest) => project || manifest),
        );
        const results = Array.zipWith(members, stated, (member, manifest): [string, { readonly projects: Record<string, ProjectConfiguration> }] => {
            const checks = member.checks.filter((check) => Array.isReadonlyArrayNonEmpty(Array.intersection(check.patterns, held)));
            return [
                member.lock,
                {
                    projects: {
                        [member.root]: {
                            root: member.root,
                            ...(!supplied.includes(member) && { name: member.name }),
                            metadata: { technologies: ['python'] },
                            ...(member.packaged && { tags: ['release'], release: { version: { versionActions: `${plugin.name}/release/version-actions`, versionActionsOptions: { lock: member.lock } } } }),
                            implicitDependencies: Array.dedupe([...member.linked, ...checks.flatMap(Struct.get('paths'))])
                                .filter((root) => root !== member.root && roots.has(root))
                                .map((root) => `directory:${root}`),
                            targets: targets({ ...member, checks }, settings, manifest, anchor),
                        },
                    },
                },
            ];
        });
        yield* Effect.forEach(models.flatMap(Struct.get('refused')), (warning) => Effect.sync(() => emitPluginWorkerLog('warn', warning)));
        return yield* Array.match(refused, { onEmpty: () => Effect.succeed(results), onNonEmpty: (errors) => Effect.fail(new AggregateCreateNodesError(errors, results)) });
    }),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
