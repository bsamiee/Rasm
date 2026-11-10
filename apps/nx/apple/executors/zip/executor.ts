import type { ExecutorContext } from '@nx/devkit';
import { printed, runtime, ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, Option, Path, Result, Schema } from 'effect';
import { commands } from '#/executors/run/executor';
import { SHORT_VERSION } from '#/plugins/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Zip = Schema.Struct({
    exports: Schema.Array(Schema.Struct({ archive: Schema.String, folder: Schema.String, method: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    products: Schema.Array(Schema.Struct({ package: Schema.String, product: Schema.String, folder: Schema.String, manifest: Schema.String })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    profile: Schema.optionalKey(Schema.String),
    args: Schema.optionalKey(Schema.Array(Schema.String)),
});
const Application = Schema.fromJsonString(Schema.Struct({ ApplicationPath: Schema.String, [SHORT_VERSION]: Schema.String }));
const ExportOptions = Schema.fromJsonString(Schema.Struct({ method: Schema.String }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const executed = Effect.fnUntraced(function* (options: unknown, context: ExecutorContext) {
    const [{ exports, products, profile, args = [] }, path] = yield* Effect.all([Schema.decodeUnknownEffect(Zip)(options), Path.Path]);
    const notarized = Array.filterMap(exports, ({ folder, method }) =>
        Result.map(
            Result.fromNullishOr(method, () => folder),
            (exported) => ({ settings: path.join(folder, 'ExportOptions.plist'), method: exported }),
        ),
    );
    const keychain = Option.fromUndefinedOr(profile);
    yield* Option.isNone(keychain) && notarized.length > 0 ? ToolError.make({ subject: 'notaryProfile', detail: "is unset in nx.json's @rasm/nx-apple row; notarytool needs the keychain profile `notarytool store-credentials` saved" }) : Effect.void;
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
});

const execute = (options: unknown, context: ExecutorContext) => executed(options, context).pipe(runtime.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
