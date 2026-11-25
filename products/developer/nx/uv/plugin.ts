import { join, posix } from 'node:path';
import { AggregateCreateNodesError, type CreateNodes, type ProjectConfiguration } from '@nx/devkit';
import { emitPluginWorkerLog, globWithWorkspaceContext } from '@nx/devkit/internal';
import { anchored, fingerprinted, inferring, manifested, run, spawned } from '@rasm/nx';
import { Array, Effect, FileSystem, Glob, Option, type Record, Schema, String } from 'effect';
import which from 'which';
import { Options, PLUGIN, targets } from '#/targets';
import { LOCK, located, Metadata, SCHEMA } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const OWN = fingerprinted(import.meta.dirname);
const METADATA = ['uv', 'workspace', 'metadata', '--frozen', '--preview-features', 'workspace-metadata', '--output-version', SCHEMA, '--quiet'] as const;
const LISTED = ['uv', 'workspace', 'list'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Named = Schema.fromJsonString(Schema.Struct({ name: Schema.String })).annotate({ description: 'The name field of a package.json, which Nx gives a project no plugin names' });

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
    return { ...(yield* located(workspaceRoot, lock, metadata)), refused };
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
        const listed = yield* Array.match(patterns, { onEmpty: () => Effect.succeed([]), onNonEmpty: (globs) => Effect.promise(() => globWithWorkspaceContext(context.workspaceRoot, globs)) });
        const held = new Set(patterns.filter((pattern) => listed.some((file) => Glob.match(Glob.make(pattern, { dot: true }), file))));
        const [roots, anchor] = [new Set(members.map(({ root }) => root)), anchored(context.workspaceRoot)];
        const stated = yield* Effect.forEach(members, ({ root }) => manifested(context.workspaceRoot, root), { concurrency: 'unbounded' });
        const supplied = yield* Effect.filter(
            members.filter(({ listed: uv }) => !uv),
            ({ root }) => Effect.zipWith(fs.exists(join(context.workspaceRoot, root, 'project.json')), Effect.option(Effect.flatMap(fs.readFileString(join(context.workspaceRoot, root, 'package.json')), Schema.decodeEffect(Named))), (project, manifest) => project || Option.isSome(manifest)),
        );
        const results = Array.zipWith(members, stated, (member, manifest): [string, { readonly projects: Record<string, ProjectConfiguration> }] => [
            member.lock,
            {
                projects: {
                    [member.root]: {
                        root: member.root,
                        ...(!supplied.includes(member) && { name: member.name }),
                        metadata: { technologies: ['python'] },
                        ...(member.packaged && { tags: ['release'], release: { version: { versionActions: `${PLUGIN}/release/version-actions`, versionActionsOptions: { lock: member.lock } } } }),
                        implicitDependencies: member.linked.filter((root) => roots.has(root)).map((root) => `directory:${root}`),
                        targets: targets({ ...member, checks: member.checks.filter((check) => !held.isDisjointFrom(new Set(check.patterns))) }, settings, manifest, anchor),
                    },
                },
            },
        ]);
        yield* Effect.forEach(
            models.flatMap(({ lock, refused: refusals }) => refusals.map((refusal) => `${lock} gives uv no workspace metadata, so its members hold no checker, test, or build target until \`uv lock\` rewrites it: ${refusal}`)),
            (warning) => Effect.sync(() => emitPluginWorkerLog('warn', warning)),
        );
        return yield* Array.match(refused, { onEmpty: () => Effect.succeed(results), onNonEmpty: (errors) => Effect.fail(new AggregateCreateNodesError(errors, results)) });
    }),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
