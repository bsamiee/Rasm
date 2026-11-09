import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Array, Effect, Path, Result, Schema } from 'effect';
import { execute as run } from '#/executors/run/executor';
import { AppleError, spawned } from '#/plugins/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Zip = Schema.Struct({
    exports: Schema.Array(Schema.Struct({ archive: Schema.String, folder: Schema.String, method: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    products: Schema.Array(Schema.Struct({ package: Schema.String, product: Schema.String, folder: Schema.String })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    profile: Schema.optionalKey(Schema.String),
    args: Schema.optionalKey(Schema.Array(Schema.String)),
});
const Application = Schema.fromJsonString(Schema.Struct({ ApplicationPath: Schema.String, CFBundleShortVersionString: Schema.String }));
const ExportOptions = Schema.fromJsonString(Schema.Struct({ method: Schema.String }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const executed = Effect.fnUntraced(function* (options: unknown, context: ExecutorContext) {
    const [{ exports, products, profile, args = [] }, path] = yield* Effect.all([Schema.decodeUnknownEffect(Zip)(options), Path.Path]);
    const notarized = Array.filterMap(exports, ({ folder, method }) =>
        Result.map(
            Result.fromNullishOr(method, () => folder),
            (held) => ({ settings: path.join(folder, 'ExportOptions.plist'), method: held }),
        ),
    );
    const keychain = yield* profile === undefined && notarized.length > 0 ? AppleError.make({ subject: 'notaryProfile', detail: 'is unset in the @rasm/nx-apple row of nx.json, and notarytool reads its credentials from the keychain profile notarytool store-credentials saved' }) : Effect.succeed(profile ?? '');
    const applications = yield* Effect.forEach(exports, ({ archive }) => Effect.flatMap(spawned('plutil', ['-extract', 'ApplicationProperties', 'json', '-o', '-', path.join(archive, 'Info.plist')], { cwd: context.root }), Schema.decodeEffect(Application)));
    const binaries = yield* Effect.forEach(products, ({ package: root }) => spawned('swift', ['build', '--package-path', root, '--configuration', 'release', '--show-bin-path'], { cwd: context.root }));
    const encoded = yield* Effect.forEach(notarized, ({ method }) => Schema.encodeEffect(ExportOptions)({ method }));
    yield* Effect.forEach(Array.zip(notarized, encoded), ([{ settings }, input]) => spawned('plutil', ['-convert', 'xml1', '-o', settings, '-'], { cwd: context.root, input }));
    const archived = Array.zip(exports, applications).flatMap(([{ archive, folder, method }, { ApplicationPath, CFBundleShortVersionString }]): readonly (readonly [string, ...string[]])[] => {
        const app = method === undefined ? path.join(archive, 'Products', ApplicationPath) : path.join(folder, path.basename(ApplicationPath));
        const zip = path.join(folder, `${path.basename(ApplicationPath, path.extname(ApplicationPath))}-${CFBundleShortVersionString}.zip`);
        const zipped = ['ditto', '-c', '-k', '--keepParent', app, zip] as const;
        return method === undefined
            ? [[...zipped, ...args]]
            : [['xcodebuild', '-exportArchive', '-archivePath', archive, '-exportPath', folder, '-exportOptionsPlist', path.join(folder, 'ExportOptions.plist')], zipped, ['xcrun', 'notarytool', 'submit', zip, '--wait', '--keychain-profile', keychain, ...args], ['xcrun', 'stapler', 'staple', app], zipped];
    });
    const built = Array.zip(products, binaries).map(([{ product, folder }, bin]): readonly [string, ...string[]] => ['ditto', '-c', '-k', path.join(bin.trim(), product), path.join(folder, `${product}.zip`), ...args]);
    return yield* Effect.promise(() => run({ runs: [...archived, ...built] }, context));
}, Effect.provide(layer));

const execute = (options: unknown, context: ExecutorContext) => executed(options, context).pipe(Effect.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
