import type { ExecutorContext } from '@nx/devkit';
import { Array, Cause, Effect, FileSystem, Option, Path, Schema } from 'effect';
import { run, Words } from '#/executor';
import { Identity, located } from '#/host-packages';

// --- [MODELS] --------------------------------------------------------------------------

const Package = Schema.Struct({
    outputPath: Schema.String.annotate({ description: 'Folder, relative to the workspace root, receiving the package, each earlier package of the project in it removed first' }),
    timestampAuthority: Schema.String.annotate({ description: 'Timestamp server ZXPSignCmd -tsa signs against, so a CEP package outlives its certificate', default: 'http://timestamp.digicert.com' }),
    identity: Schema.optionalKey(Identity).annotate({
        description: 'certificate: environment variable holding the base64 PKCS#12 certificate ZXPSignCmd signs a CEP package with; password: the variable holding its password; absent, or certificate unset or empty, signs with a certificate ZXPSignCmd -selfSignedCert creates for the pack',
    }),
    __unparsed__: Words,
}).annotate({
    title: 'Package',
    description: "Packs the Adobe UXP plugin or CEP extension the project's other builds wrote, else its tracked source folder: a .ccx zip for a manifest.json, a .zxp ZXPSignCmd signs for a CSXS/manifest.xml with the certificate its identity variables hold, else with one it creates for the pack",
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ outputPath, __unparsed__: words = [], ...signing }: typeof Package.Type, context: ExecutorContext) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { project, folders } = yield* located(context);
    const packed = Array.getSomes(folders.map((found) => Option.map(Option.all({ pack: Option.fromUndefinedOr(found.host.build?.pack), archive: Option.fromUndefinedOr(found.host.archive) }), ({ pack, archive }) => ({ found, pack, file: path.join(context.root, outputPath, `${found.name}${archive}`) }))));
    yield* fs.makeDirectory(path.join(context.root, outputPath), { recursive: true });
    const held = yield* fs.readDirectory(path.join(context.root, outputPath));
    yield* Effect.forEach(
        packed,
        ({ found, file }) =>
            Effect.forEach(
                held.filter((name) => name.startsWith(found.name) && path.extname(name) === path.extname(file)),
                (name) => fs.remove(path.join(context.root, outputPath, name)),
                { discard: true },
            ),
        { discard: true },
    );
    yield* Array.match(packed, {
        onEmpty: () => Effect.fail(new Cause.UnknownError(folders, `${project} finds no manifest.json, CSXS/manifest.xml, or manifest.yml in what its builds wrote`)),
        onNonEmpty: Effect.forEach(({ found, pack, file }) => pack(found, file, signing, words), { discard: true }),
    });
    return { success: true };
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Package };
