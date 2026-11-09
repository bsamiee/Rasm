import { matchesGlob } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, pipe, Schema, UndefinedOr } from 'effect';
import { built, DistributionError, HOSTS } from '#/plugins/host-packages';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String), timestampAuthority: Schema.optionalKey(Schema.String) }).annotate({ description: 'package executor options, schema.json defaults applied' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const packed = Effect.fnUntraced(function* (context: ExecutorContext, outputPath: string, timestampAuthority: Option.Option<string>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const target = yield* Effect.fromNullishOr(context.targetName);
    const { project, root, written } = yield* built(context, Option.fromUndefinedOr(context.configurationName), target);
    const files = yield* Option.match(written, {
        onSome: Effect.succeed,
        onNone: () =>
            Effect.map(
                Effect.promise(() => globWithWorkspaceContext(context.root, [path.join(root, '**')])),
                Array.map((file) => path.join(context.root, file)),
            ),
    });
    const packers = Array.getSomes(HOSTS.map((host) => Option.all({ host: Option.some(host), archive: Option.fromUndefinedOr(host.archive), pack: Option.fromUndefinedOr(host.build?.pack) })));
    const candidates = files.flatMap((file) => packers.map((packer) => ({ ...packer, file, folder: path.join(file, path.relative(packer.host.manifest, '.')) })).filter(({ host, folder }) => matchesGlob(path.relative(folder, file), host.manifest)));
    const read = yield* Effect.forEach(candidates, ({ host, file }) => Effect.flatMap(fs.readFileString(file), (text) => host.read(text, file)));
    const manifests = yield* Array.match(
        Array.getSomes(
            Array.zipWith(candidates, read, (candidate, fields) =>
                pipe(
                    fields,
                    Option.flatMap(({ version }) => Option.fromUndefinedOr(version)),
                    Option.map((version) => ({ ...candidate, version })),
                ),
            ),
        ),
        {
            onEmpty: () => DistributionError.make({ subject: project, detail: `finds no ${packers.map(({ host }) => host.manifest).join(' or ')} in what its builds wrote` }),
            onNonEmpty: Effect.succeed,
        },
    );
    const directory = path.join(context.root, outputPath);
    yield* fs.makeDirectory(directory, { recursive: true });
    const stale = (yield* fs.readDirectory(directory)).filter((name) => manifests.some(({ archive }) => name.endsWith(archive)));
    yield* Effect.forEach(stale, (name) => fs.remove(path.join(directory, name)), { discard: true });
    yield* Effect.forEach(
        manifests,
        ({ pack, folder, version, archive }) =>
            pack(
                folder,
                files.filter((other) => other.startsWith(`${folder}/`)).map((other) => path.relative(folder, other)),
                path.join(directory, `${path.basename(project)}-${version}${archive}`),
                timestampAuthority,
            ),
        { discard: true },
    );
    return { success: true };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) =>
    Schema.decodeUnknownEffect(Options)(options).pipe(
        Effect.flatMap(({ outputPath, timestampAuthority }) => UndefinedOr.match(outputPath, { onUndefined: () => Effect.succeed({ success: true }), onDefined: (at) => packed(context, at, Option.fromUndefinedOr(timestampAuthority)) })),
        Effect.provide(layer),
        Effect.runPromise,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
