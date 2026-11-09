import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Array, Effect, Option, Path, Schema } from 'effect';
import { execute as run } from '#/executors/run/executor';
import { AppleError, spawned } from '#/plugins/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Zip = Schema.Struct({
    exports: Schema.Array(Schema.Struct({ archive: Schema.String, folder: Schema.String, method: Schema.optionalKey(Schema.String) })),
    profile: Schema.optionalKey(Schema.String),
    args: Schema.optionalKey(Schema.Array(Schema.String)),
});
const Application = Schema.fromJsonString(Schema.Struct({ ApplicationPath: Schema.String, CFBundleShortVersionString: Schema.String }));
const ExportOptions = Schema.fromJsonString(Schema.Struct({ method: Schema.String }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) =>
    Effect.runPromise(
        Effect.gen(function* () {
            const [{ exports, profile, args = [] }, path] = yield* Effect.all([Schema.decodeUnknownEffect(Zip)(options), Path.Path]);
            const notarized = exports.flatMap(({ folder, method }) => (method === undefined ? [] : [{ settings: path.join(folder, 'ExportOptions.plist'), method }]));
            const keychain = yield* Option.match(Option.fromUndefinedOr(profile), {
                onNone: () => (notarized.length === 0 ? Effect.succeed('') : AppleError.make({ subject: 'notaryProfile', detail: 'is unset in the @rasm/nx-apple row of nx.json, and notarytool reads its credentials from the keychain profile notarytool store-credentials saved' })),
                onSome: Effect.succeed,
            });
            const applications = yield* Effect.forEach(exports, ({ archive }) => Effect.flatMap(spawned('plutil', ['-extract', 'ApplicationProperties', 'json', '-o', '-', path.join(archive, 'Info.plist')], { cwd: context.root }), Schema.decodeEffect(Application)));
            const encoded = yield* Effect.forEach(notarized, ({ method }) => Schema.encodeEffect(ExportOptions)({ method }));
            yield* Effect.forEach(Array.zip(notarized, encoded), ([{ settings }, input]) => spawned('plutil', ['-convert', 'xml1', '-o', settings, '-'], { cwd: context.root, input }));
            const runs = Array.zip(exports, applications).flatMap(([{ archive, folder, method }, { ApplicationPath, CFBundleShortVersionString }]): readonly (readonly [string, ...string[]])[] => {
                const app = method === undefined ? path.join(archive, 'Products', ApplicationPath) : path.join(folder, path.basename(ApplicationPath));
                const zip = path.join(folder, `${path.basename(ApplicationPath, path.extname(ApplicationPath))}-${CFBundleShortVersionString}.zip`);
                const zipped = ['ditto', '-c', '-k', '--keepParent', app, zip] as const;
                return method === undefined
                    ? [[...zipped, ...args]]
                    : [['xcodebuild', '-exportArchive', '-archivePath', archive, '-exportPath', folder, '-exportOptionsPlist', path.join(folder, 'ExportOptions.plist')], zipped, ['xcrun', 'notarytool', 'submit', zip, '--wait', '--keychain-profile', keychain, ...args], ['xcrun', 'stapler', 'staple', app], zipped];
            });
            return yield* Effect.promise(() => run({ runs }, context));
        }).pipe(Effect.provide(layer)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
