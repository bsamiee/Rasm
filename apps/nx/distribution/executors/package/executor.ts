import type { ExecutorContext } from '@nx/devkit';
import { runtime, ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, FileSystem, Option, Path, Schema, UndefinedOr } from 'effect';
import { sourced } from '#/plugins/host-packages';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String), timestampAuthority: Schema.optionalKey(Schema.String) }).annotate({ description: 'package executor options, schema.json defaults applied' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const packed = Effect.fnUntraced(function* (context: ExecutorContext, outputPath: string, timestampAuthority: Option.Option<string>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const target = yield* Effect.fromNullishOr(context.targetName);
    const { project, manifests: found } = yield* sourced(context, Option.fromUndefinedOr(context.configurationName), target);
    const manifests = yield* Array.match(
        Array.getSomes(
            found.map(({ host, folder, names, fields }) => Option.all({ pack: Option.fromUndefinedOr(host.build?.pack), archive: Option.fromUndefinedOr(host.archive), version: Option.flatMap(fields, ({ version }) => Option.fromUndefinedOr(version)), folder: Option.some(folder), names: Option.some(names) })),
        ),
        {
            onEmpty: () => ToolError.make({ subject: project, detail: 'finds no manifest.json or CSXS/manifest.xml in what its builds wrote' }),
            onNonEmpty: Effect.succeed,
        },
    );
    const directory = path.join(context.root, outputPath);
    yield* fs.makeDirectory(directory, { recursive: true });
    const stale = (yield* fs.readDirectory(directory)).filter((name) => manifests.some(({ archive }) => name.endsWith(archive)));
    yield* Effect.forEach(stale, (name) => fs.remove(path.join(directory, name)), { discard: true });
    yield* Effect.forEach(manifests, ({ pack, folder, names, version, archive }) => pack(folder, names, path.join(directory, `${path.basename(project)}-${version}${archive}`), timestampAuthority), { discard: true });
    return { success: true };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) =>
    Schema.decodeUnknownEffect(Options)(options).pipe(
        Effect.flatMap(({ outputPath, timestampAuthority }) => UndefinedOr.match(outputPath, { onUndefined: () => Effect.succeed({ success: true }), onDefined: (at) => packed(context, at, Option.fromUndefinedOr(timestampAuthority)) })),
        runtime.runPromise,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
