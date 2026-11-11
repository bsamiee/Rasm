import { executor } from '@rasm/nx-devkit/executor';
import { ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, FileSystem, Option, Path, Schema } from 'effect';
import { located } from '#/host-packages';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({
    outputPath: Schema.String.annotate({ description: 'Folder, relative to the workspace root, receiving the package' }),
    timestampAuthority: Schema.String.annotate({ description: 'Timestamp server ZXPSignCmd -tsa signs against, so a CEP package outlives its certificate', default: 'http://timestamp.digicert.com' }),
}).annotate({ title: 'Package', description: "Packs the Adobe UXP plugin or CEP extension the project's other builds wrote, else its tracked source folder: a .ccx zip for a manifest.json, a .zxp ZXPSignCmd signs with a certificate it creates for the build for a CSXS/manifest.xml" });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Options,
    Effect.fnUntraced(function* ({ outputPath, timestampAuthority }, context, project) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const { folders } = yield* located(context, yield* Effect.fromNullishOr(context.targetName));
        const packed = Array.getSomes(
            folders.map((found) => Option.map(Option.all({ pack: Option.fromUndefinedOr(found.host.build?.pack), archive: Option.fromUndefinedOr(found.host.archive) }), ({ pack, archive }) => ({ found, pack, file: path.join(context.root, outputPath, `${path.basename(project)}-${found.version}${archive}`) }))),
        );
        yield* fs.makeDirectory(path.join(context.root, outputPath), { recursive: true });
        yield* Effect.forEach(
            (yield* fs.readDirectory(path.join(context.root, outputPath))).filter((name) => packed.some(({ file }) => path.extname(name) === path.extname(file))),
            (name) => fs.remove(path.join(context.root, outputPath, name)),
            { discard: true },
        );
        yield* Array.match(packed, {
            onEmpty: () => ToolError.make({ subject: project, detail: 'finds no manifest.json or CSXS/manifest.xml in what its builds wrote' }),
            onNonEmpty: Effect.forEach(({ found, pack, file }) => pack(found, file, timestampAuthority), { discard: true }),
        });
        return true;
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
