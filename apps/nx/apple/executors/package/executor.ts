import { execFileSync } from 'node:child_process';
import type { ExecutorContext } from '@nx/devkit';
import { Array, Config, Effect, FileSystem, Option, Path, Redacted, Schema, Struct } from 'effect';
import { commands, run } from '#/executors/run/executor';
import { GIT_STATUS, refused } from '#/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Package = Schema.Struct({
    exports: Schema.Array(Schema.Struct({ archive: Schema.String, folder: Schema.String, run: Schema.NonEmptyArray(Schema.String).annotate({ description: 'The xcodebuild archive invocation that writes the archive' }) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    products: Schema.Array(Schema.Struct({ package: Schema.String, product: Schema.String, folder: Schema.String })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    args: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Arguments appended to each ditto invocation that writes a zip' }),
}).annotate({
    title: 'Package',
    description:
        'Archives each app with its xcodebuild invocation and builds each SwiftPM executable product in release, then zips each under its bundle or product name with ditto; where ASC_API_KEY_P8, ASC_API_KEY_ID, and ASC_API_ISSUER_ID are set, exports each archive under the Developer ID Application identity into a disk image in place of the zip, signs it, notarizes it with that App Store Connect key, and staples the ticket',
});
const Application = Schema.fromJsonString(Schema.Struct({ ApplicationPath: Schema.String }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ exports = [], products = [], args = [] }: typeof Package.Encoded, context: ExecutorContext) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const printed = (command: string, given: readonly string[], input?: string) => Effect.try({ try: () => execFileSync(command, given, { encoding: 'utf8', cwd: context.root, input }), catch: refused(command, 'does not exit 0') });
    yield* Effect.filterOrFail(commands({ runs: exports.map((exported) => exported.run), env: GIT_STATUS }, context.root), Struct.get('success'), refused('xcodebuild archive', 'does not exit 0'));
    const applications = yield* Effect.forEach(exports, ({ archive }) => Effect.flatMap(printed('plutil', ['-extract', 'ApplicationProperties', 'json', '-o', '-', path.join(archive, 'Info.plist')]), Schema.decodeEffect(Application)));
    const binaries = yield* Effect.forEach(products, ({ package: root }) => printed('swift', ['build', '--package-path', root, '--configuration', 'release', '--show-bin-path']));
    const staging = yield* fs.makeTempDirectoryScoped();
    const [settings, key] = [path.join(staging, 'ExportOptions.plist'), path.join(staging, 'AuthKey.p8')];
    const notary = yield* Config.option(Config.all([Config.Redacted('ASC_API_KEY_P8'), Config.String('ASC_API_KEY_ID'), Config.String('ASC_API_ISSUER_ID')]));
    yield* Effect.transposeOption(Option.map(notary, ([secret]) => Effect.all([printed('plutil', ['-convert', 'xml1', '-o', settings, '-'], '{"method":"developer-id"}'), fs.writeFileString(key, Redacted.value(secret), { mode: 0o600 })])));
    const archived = Array.zip(exports, applications).flatMap(([{ archive, folder }, { ApplicationPath }]): readonly (readonly [string, ...string[]])[] => {
        const [bundle, name] = [path.basename(ApplicationPath), path.basename(ApplicationPath, path.extname(ApplicationPath))];
        const [volume, image, zip] = [path.join(staging, name), path.join(folder, `${name}.dmg`), path.join(folder, `${name}.zip`)];
        return Option.match(notary, {
            onNone: () => [
                ['rm', '-f', image],
                ['ditto', '-c', '-k', '--keepParent', path.join(archive, 'Products', ApplicationPath), zip, ...args],
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
        ['swift', 'build', '--package-path', root, '--configuration', 'release', '--product', product, '--force-resolved-versions'],
        ['ditto', '-c', '-k', path.join(bin.trim(), product), path.join(folder, `${product}.zip`), ...args],
    ]);
    return yield* commands({ runs: [...archived, ...built.flat()] }, context.root);
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Package };
