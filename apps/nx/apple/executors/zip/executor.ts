import { executor } from '@rasm/nx-devkit/executor';
import { printed, ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, Option, Path, Result, Schema } from 'effect';
import { commands } from '#/executors/run/executor';
import { SHORT_VERSION } from '#/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Zip = Schema.Struct({
    exports: Schema.Array(Schema.Struct({ archive: Schema.String, folder: Schema.String, method: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    products: Schema.Array(Schema.Struct({ package: Schema.String, product: Schema.String, folder: Schema.String, manifest: Schema.String })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    profile: Schema.optionalKey(Schema.String).annotate({ description: "notarytool keychain profile from the plugin's notaryProfile option, required by archives with a method" }),
    args: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Arguments appended to notarytool submit, else to ditto' }),
}).annotate({ title: 'Zip', description: "Zips each archived app and each SwiftPM executable product's release build with ditto; an archive with a method is exported with it first, notarized with notarytool, stapled, and zipped again" });
const Application = Schema.fromJsonString(Schema.Struct({ ApplicationPath: Schema.String, [SHORT_VERSION]: Schema.String }));
const ExportOptions = Schema.fromJsonString(Schema.Struct({ method: Schema.String }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Zip,
    Effect.fnUntraced(function* ({ exports, products, profile, args = [] }, context) {
        const path = yield* Path.Path;
        const notarized = Array.filterMap(exports, ({ folder, method }) =>
            Result.map(
                Result.fromNullishOr(method, () => folder),
                (exported) => ({ settings: path.join(folder, 'ExportOptions.plist'), method: exported }),
            ),
        );
        const keychain = Option.fromUndefinedOr(profile);
        yield* Option.isNone(keychain) && notarized.length > 0 ? ToolError.make({ subject: 'notaryProfile', detail: 'is unset in the plugin options, notarytool needs the keychain profile `notarytool store-credentials` saved' }) : Effect.void;
        const applications = yield* Effect.forEach(exports, ({ archive }) => Effect.flatMap(printed('plutil', ['-extract', 'ApplicationProperties', 'json', '-o', '-', path.join(archive, 'Info.plist')], { cwd: context.root }), Schema.decodeEffect(Application)));
        const binaries = yield* Effect.forEach(products, ({ package: root }) => printed('swift', ['build', '--package-path', root, '--configuration', 'release', '--show-bin-path'], { cwd: context.root }));
        const versions = yield* Effect.forEach(products, ({ manifest }) => printed('plutil', ['-extract', SHORT_VERSION, 'raw', '-o', '-', manifest], { cwd: context.root }));
        const encoded = yield* Effect.forEach(notarized, ({ method }) => Schema.encodeEffect(ExportOptions)({ method }));
        yield* Effect.forEach(Array.zip(notarized, encoded), ([{ settings }, input]) => printed('plutil', ['-convert', 'xml1', '-o', settings, '-'], { cwd: context.root, input }));
        const archived = Array.zip(exports, applications).flatMap(([{ archive, folder, method }, { ApplicationPath, [SHORT_VERSION]: version }]): readonly (readonly [string, ...string[]])[] => {
            const app = method === undefined ? path.join(archive, 'Products', ApplicationPath) : path.join(folder, path.basename(ApplicationPath));
            const zip = path.join(folder, `${path.basename(ApplicationPath, path.extname(ApplicationPath))}-${version}.zip`);
            const zipped = ['ditto', '-c', '-k', '--keepParent', app, zip] as const;
            return method === undefined
                ? [[...zipped, ...args]]
                : [
                      ['xcodebuild', '-exportArchive', '-archivePath', archive, '-exportPath', folder, '-exportOptionsPlist', path.join(folder, 'ExportOptions.plist')],
                      zipped,
                      ['xcrun', 'notarytool', 'submit', zip, '--wait', ...Option.toArray(keychain).flatMap((name) => ['--keychain-profile', name]), ...args],
                      ['xcrun', 'stapler', 'staple', app],
                      zipped,
                  ];
        });
        const built = Array.zipWith(Array.zip(products, binaries), versions, ([{ product, folder }, bin], version): readonly [string, ...string[]] => ['ditto', '-c', '-k', path.join(bin.trim(), product), path.join(folder, `${product}-${version.trim()}.zip`), ...args]);
        return yield* commands({ runs: [...archived, ...built] }, context.root);
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
