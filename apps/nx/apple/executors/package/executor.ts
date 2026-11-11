import { executor } from '@rasm/nx-devkit/executor';
import { printed } from '@rasm/nx-devkit/process';
import { Array, Config, Effect, FileSystem, Option, Path, Redacted, Schema } from 'effect';
import { commands } from '#/executors/run/executor';
import { SHORT_VERSION } from '#/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Package = Schema.Struct({
    exports: Schema.Array(Schema.Struct({ archive: Schema.String, folder: Schema.String })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    products: Schema.Array(Schema.Struct({ package: Schema.String, product: Schema.String, folder: Schema.String, manifest: Schema.String })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    args: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Arguments appended to each ditto invocation that writes a zip' }),
}).annotate({
    title: 'Package',
    description:
        "Zips each archived app and each SwiftPM executable product's release build with ditto; where ASC_API_KEY_P8, ASC_API_KEY_ID, and ASC_API_ISSUER_ID are set and a keychain holds a Developer ID Application identity, exports each archive under it into a disk image, signs it, notarizes it with that App Store Connect key, and staples the ticket",
});
const Application = Schema.fromJsonString(Schema.Struct({ ApplicationPath: Schema.String, [SHORT_VERSION]: Schema.String }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Package,
    Effect.fnUntraced(function* ({ exports, products, args = [] }, context) {
        const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
        const applications = yield* Effect.forEach(exports, ({ archive }) => Effect.flatMap(printed('plutil', ['-extract', 'ApplicationProperties', 'json', '-o', '-', path.join(archive, 'Info.plist')], { cwd: context.root }), Schema.decodeEffect(Application)));
        const binaries = yield* Effect.forEach(products, ({ package: root }) => printed('swift', ['build', '--package-path', root, '--configuration', 'release', '--show-bin-path'], { cwd: context.root }));
        const versions = yield* Effect.forEach(products, ({ manifest }) => printed('plutil', ['-extract', SHORT_VERSION, 'raw', '-o', '-', manifest], { cwd: context.root }));
        const staging = yield* fs.makeTempDirectoryScoped();
        const [settings, key] = [path.join(staging, 'ExportOptions.plist'), path.join(staging, 'AuthKey.p8')];
        const identity = 'Developer ID Application';
        const identities = yield* printed('security', ['find-identity', '-v', '-p', 'codesigning']);
        const notary = Option.filter(yield* Config.option(Config.all([Config.Redacted('ASC_API_KEY_P8'), Config.String('ASC_API_KEY_ID'), Config.String('ASC_API_ISSUER_ID')])), () => identities.includes(identity));
        yield* Effect.transposeOption(Option.map(notary, ([secret]) => Effect.all([printed('plutil', ['-convert', 'xml1', '-o', settings, '-'], { input: '{"method":"developer-id"}' }), fs.writeFileString(key, Redacted.value(secret), { mode: 0o600 })])));
        const archived = Array.zip(exports, applications).flatMap(([{ archive, folder }, { ApplicationPath, [SHORT_VERSION]: version }]): readonly (readonly [string, ...string[]])[] => {
            const [bundle, name] = [path.basename(ApplicationPath), path.basename(ApplicationPath, path.extname(ApplicationPath))];
            const [volume, image] = [path.join(staging, name), path.join(folder, `${name}-${version}.dmg`)];
            return Option.match(notary, {
                onNone: () => [['ditto', '-c', '-k', '--keepParent', path.join(archive, 'Products', ApplicationPath), path.join(folder, `${name}-${version}.zip`), ...args]],
                onSome: ([, id, issuer]) => [
                    ['xcodebuild', '-exportArchive', '-archivePath', archive, '-exportPath', folder, '-exportOptionsPlist', settings],
                    ['ditto', path.join(folder, bundle), path.join(volume, bundle)],
                    ['ln', '-s', '/Applications', volume],
                    ['diskutil', 'image', 'create', 'from', '--volumeName', name, volume, image],
                    ['codesign', '--sign', identity, '--timestamp', image],
                    ['xcrun', 'notarytool', 'submit', image, '--wait', '--key', key, '--key-id', id, '--issuer', issuer],
                    ['xcrun', 'stapler', 'staple', image],
                    ['spctl', '--assess', '--type', 'open', '--context', 'context:primary-signature', '--verbose', image],
                ],
            });
        });
        const built = Array.zipWith(Array.zip(products, binaries), versions, ([{ product, folder }, bin], version): readonly [string, ...string[]] => ['ditto', '-c', '-k', path.join(bin.trim(), product), path.join(folder, `${product}-${version.trim()}.zip`), ...args]);
        return yield* commands({ runs: [...archived, ...built] }, context.root);
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
