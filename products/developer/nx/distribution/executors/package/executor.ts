import { Array, Cause, Effect, FileSystem, Option, Path, Schema } from 'effect';
import { located } from '#/host-packages';
import { executor, Words } from '#/inference';

// --- [MODELS] --------------------------------------------------------------------------

const Package = Schema.Struct({
    outputPath: Schema.String.annotate({ description: 'Folder, relative to the workspace root, receiving the package, each earlier package of the project in it removed first' }),
    __unparsed__: Words,
}).annotate({
    title: 'Package',
    description: "Packs each host package the project's other builds wrote, else its tracked source folder: a .ccx zip for an Adobe UXP manifest.json, a .zip for a Chrome Manifest V3 manifest.json, a .yak through yak build for a manifest.yml",
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Package, function* ({ outputPath, __unparsed__: words = [] }, context) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { project, folders } = yield* located(context);
    const packed = Array.getSomes(folders.map((found) => Option.map(Option.all({ pack: Option.fromUndefinedOr(found.host.build?.pack), archive: Option.fromUndefinedOr(found.host.archive) }), ({ pack, archive }) => ({ found, pack, file: path.join(context.root, outputPath, `${found.name}${archive}`) }))));
    yield* fs.makeDirectory(path.join(context.root, outputPath), { recursive: true });
    const archives = new Set(packed.map(({ file }) => path.extname(file)));
    yield* Effect.forEach(
        (yield* fs.readDirectory(path.join(context.root, outputPath))).filter((name) => archives.has(path.extname(name))),
        (name) => fs.remove(path.join(context.root, outputPath, name)),
        { discard: true },
    );
    yield* Array.match(packed, {
        onEmpty: () => Effect.fail(new Cause.UnknownError(folders, `${project} finds no manifest.json or manifest.yml in what its builds wrote`)),
        onNonEmpty: Effect.forEach(({ found, pack, file }) => pack(found, file, words), { discard: true }),
    });
    return { success: true };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Package };
