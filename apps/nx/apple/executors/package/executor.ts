import type { ExecutorContext } from '@nx/devkit';
import { Array, Cause, Config, Effect, FileSystem, Option, Path, Redacted, Schema, Stream, Struct } from 'effect';
import { commands } from '#/executors/run/executor';
import { GIT_STATUS, run, SWIFTPM, spawned } from '#/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Package = Schema.Struct({
    exports: Schema.Array(Schema.Struct({ archive: Schema.String, folder: Schema.String, run: Schema.NonEmptyArray(Schema.String).annotate({ description: 'The xcodebuild archive invocation that writes the archive' }) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    products: Schema.Array(Schema.Struct({ package: Schema.String, product: Schema.String, folder: Schema.String })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    notary: Schema.optionalKey(
        Schema.Struct({
            key: Schema.String.annotate({ description: 'Variable holding the App Store Connect .p8 key' }),
            keyId: Schema.String.annotate({ description: 'Variable holding the App Store Connect key id' }),
            issuer: Schema.String.annotate({ description: 'Variable holding the App Store Connect issuer id' }),
        }),
    ).annotate({ description: 'Variables naming the App Store Connect key that notarizes a disk image, absent zipping each app alone' }),
    __unparsed__: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Words after -- of the run, appended after the configured arguments', $default: { $source: 'unparsed' }, hidden: true, 'x-priority': 'internal' }),
}).annotate({
    title: 'Package',
    description:
        'Archives each app with its xcodebuild invocation and builds each SwiftPM executable product in release, then zips each under its bundle or product name with ditto; where the variables notary names are set, exports each archive under the Developer ID Application identity into a disk image in place of the zip, signs it, notarizes it with that App Store Connect key, and staples the ticket',
});
const Application = Schema.fromJsonString(Schema.Struct({ ApplicationPath: Schema.String }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ exports = [], products = [], notary: stated, __unparsed__: words = [] }: typeof Package.Encoded, context: ExecutorContext) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    yield* Effect.filterOrFail(commands({ runs: exports.map((exported) => exported.run), env: GIT_STATUS }, context.root), Struct.get('success'), () => new Cause.UnknownError(exports, 'xcodebuild archive does not exit 0'));
    const applications = yield* Effect.forEach(exports, ({ archive }) => Effect.flatMap(spawned(['plutil', '-extract', 'ApplicationProperties', 'json', '-o', '-', path.join(archive, 'Info.plist')], { cwd: context.root }), Schema.decodeEffect(Application)));
    const binaries = yield* Effect.forEach(products, ({ package: root }) => spawned(['swift', 'build', '--package-path', root, '--configuration', 'release', '--show-bin-path'], { cwd: context.root }));
    const staging = yield* fs.makeTempDirectoryScoped();
    const [settings, key] = [path.join(staging, 'ExportOptions.plist'), path.join(staging, 'AuthKey.p8')];
    const notary = Option.flatten(yield* Effect.transposeOption(Option.map(Option.fromUndefinedOr(stated), ({ key: secret, keyId, issuer }) => Config.option(Config.all([Config.Redacted(secret), Config.String(keyId), Config.String(issuer)])))));
    yield* Effect.transposeOption(Option.map(notary, ([secret]) => Effect.all([spawned(['plutil', '-convert', 'xml1', '-o', settings, '-'], { cwd: context.root, stdin: Stream.encodeText(Stream.make('{"method":"developer-id"}')) }), fs.writeFileString(key, Redacted.value(secret), { mode: 0o600 })])));
    const archived = Array.zip(exports, applications).flatMap(([{ archive, folder }, { ApplicationPath }]): readonly (readonly [string, ...string[]])[] => {
        const [bundle, name] = [path.basename(ApplicationPath), path.basename(ApplicationPath, path.extname(ApplicationPath))];
        const [volume, image, zip] = [path.join(staging, name), path.join(folder, `${name}.dmg`), path.join(folder, `${name}.zip`)];
        return Option.match(notary, {
            onNone: () => [
                ['rm', '-f', image],
                ['ditto', '-c', '-k', '--keepParent', path.join(archive, 'Products', ApplicationPath), zip, ...words],
            ],
            onSome: ([, id, issuer]) => [
                ['rm', '-f', image, zip],
                ['xcodebuild', '-exportArchive', '-archivePath', archive, '-exportPath', folder, '-exportOptionsPlist', settings],
                ['ditto', path.join(folder, bundle), path.join(volume, bundle)],
                ['ln', '-s', '/Applications', volume],
                ['diskutil', 'image', 'create', 'from', '--volumeName', name, volume, image],
                ['codesign', '--sign', 'Developer ID Application', '--timestamp', image],
                ['xcrun', 'notarytool', 'submit', image, '--wait', '--key', key, '--key-id', id, '--issuer', issuer],
                ['xcrun', 'stapler', 'staple', image],
                ['spctl', '--assess', '--type', 'open', '--context', 'context:primary-signature', '--verbose', image],
            ],
        });
    });
    const built = Array.zipWith(products, binaries, ({ package: root, product, folder }, bin): readonly (readonly [string, ...string[]])[] => [
        ['swift', 'build', '--package-path', root, '--configuration', 'release', '--product', product, ...SWIFTPM],
        ['ditto', '-c', '-k', path.join(bin.trim(), product), path.join(folder, `${product}.zip`), ...words],
    ]);
    return yield* commands({ runs: [...archived, ...built.flat()] }, context.root);
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Package };
