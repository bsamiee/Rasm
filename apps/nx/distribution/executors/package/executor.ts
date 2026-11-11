import type { ExecutorContext } from '@nx/devkit';
import { executor } from '@rasm/nx-devkit/executor';
import { ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, FileSystem, Option, Path, Schema, UndefinedOr } from 'effect';
import { sourced } from '#/host-packages';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({
    outputPath: Schema.optionalKey(Schema.String).annotate({ description: 'Folder, relative to the workspace root, receiving the package; unset, the target packs nothing' }),
    timestampAuthority: Schema.String.annotate({ description: 'Timestamp server ZXPSignCmd -tsa signs against, so a CEP package outlives its certificate', default: 'http://timestamp.digicert.com' }),
}).annotate({ title: 'Package', description: "Packs the Adobe UXP plugin or CEP extension the project's other builds wrote, else its source folder: a .ccx zip for a manifest.json, a ZXPSignCmd-signed .zxp for a CSXS/manifest.xml" });

// --- [OPERATIONS] ----------------------------------------------------------------------

const packed = Effect.fnUntraced(function* (context: ExecutorContext, outputPath: string, timestampAuthority: string) {
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
    yield* Effect.forEach(manifests, ({ pack, folder, names, version, archive }) => pack(folder, names, path.join(directory, `${path.basename(project)}-${version}${archive}`), Option.some(timestampAuthority)), { discard: true });
    return true;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Options, ({ outputPath, timestampAuthority }, context) => UndefinedOr.match(outputPath, { onUndefined: () => Effect.succeed(true), onDefined: (at) => packed(context, at, timestampAuthority) }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
