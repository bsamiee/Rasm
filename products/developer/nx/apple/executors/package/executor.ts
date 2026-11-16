import type { ExecutorContext } from '@nx/devkit';
import { spawned } from '@rasm/nx-distribution/inference';
import { Array, Cause, Config, Effect, FileSystem, Option, Path, Redacted, Schema, Stream, Struct } from 'effect';
import { commands } from '#/executors/run/executor';
import { NOTARY, run, SIGNED, SWIFTPM } from '#/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Package = Schema.Struct({
    exports: Schema.Array(Schema.Struct({ archive: Schema.String, folder: Schema.String, run: Schema.NonEmptyArray(Schema.String).annotate({ description: 'The xcodebuild archive invocation that writes the archive' }) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    products: Schema.Array(
        Schema.Struct({
            package: Schema.String,
            product: Schema.String,
            folder: Schema.String,
            bin: Schema.String.annotate({ description: 'Release bin folder swift build --show-bin-path names for the package' }),
            entitlements: Schema.optionalKey(Schema.String).annotate({ description: 'Entitlements file <product>.entitlements beside the target sources, signed into the product where present' }),
        }),
    ).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    __unparsed__: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Words after -- of the run, appended after the configured arguments', $default: { $source: 'unparsed' }, hidden: true, 'x-priority': 'internal' }),
}).annotate({
    title: 'Package',
    description: `Archives each app with its xcodebuild invocation and builds each SwiftPM executable product in release, signing each product under the ${SIGNED.development} identity with the hardened runtime, then zips each under its bundle or product name with ditto; where ${NOTARY.key}, ${NOTARY.keyId}, and ${NOTARY.issuer} are set, exports each archive under the ${SIGNED.distribution} identity into a disk image in place of the zip, signs it, notarizes it with that App Store Connect key, and staples the ticket, and signs each product under that identity with a secure timestamp and notarizes its zip`,
});
const Application = Schema.fromJsonString(Schema.Struct({ ApplicationPath: Schema.String }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ exports = [], products = [], __unparsed__: words = [] }: typeof Package.Encoded, context: ExecutorContext) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    yield* Effect.filterOrFail(commands({ runs: exports.map((exported) => exported.run) }, context.root), Struct.get('success'), () => new Cause.UnknownError(exports, 'xcodebuild archive does not exit 0'));
    const applications = yield* Effect.forEach(exports, ({ archive }) => Effect.flatMap(spawned(['plutil', '-extract', 'ApplicationProperties', 'json', '-o', '-', path.join(archive, 'Info.plist')], { cwd: context.root }), Schema.decodeEffect(Application)));
    const staging = yield* fs.makeTempDirectoryScoped();
    const [settings, key] = [path.join(staging, 'ExportOptions.plist'), path.join(staging, 'AuthKey.p8')];
    const notary = yield* Config.option(Config.all([Config.Redacted(NOTARY.key), Config.String(NOTARY.keyId), Config.String(NOTARY.issuer)]));
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
                ['codesign', '--sign', SIGNED.distribution, '--timestamp', image],
                ['xcrun', 'notarytool', 'submit', image, '--wait', '--key', key, '--key-id', id, '--issuer', issuer],
                ['xcrun', 'stapler', 'staple', image],
                ['spctl', '--assess', '--type', 'open', '--context', 'context:primary-signature', '--verbose', image],
            ],
        });
    });
    const built = products.flatMap(({ package: root, product, folder, bin, entitlements }): readonly (readonly [string, ...string[]])[] => {
        const [signed, zip] = [path.join(staging, product), path.join(folder, `${product}.zip`)];
        return [
            ['swift', 'build', '--package-path', root, '--configuration', 'release', '--product', product, ...SWIFTPM],
            ['ditto', path.join(bin, product), signed],
            ['codesign', '--force', '--options', 'runtime', ...(entitlements === undefined ? [] : ['--entitlements', entitlements]), ...Option.match(notary, { onNone: () => ['--sign', SIGNED.development], onSome: () => ['--sign', SIGNED.distribution, '--timestamp'] }), signed],
            ['ditto', '-c', '-k', signed, zip, ...words],
            ...Option.match(notary, { onNone: () => [], onSome: ([, id, issuer]) => [['xcrun', 'notarytool', 'submit', zip, '--wait', '--key', key, '--key-id', id, '--issuer', issuer] as const] }),
        ];
    });
    return yield* commands({ runs: [...archived, ...built] }, context.root);
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Package };
