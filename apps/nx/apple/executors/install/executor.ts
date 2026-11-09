import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Config, Effect, FileSystem, Path, Schema } from 'effect';
import { execute as run } from '#/executors/run/executor';

// --- [MODELS] --------------------------------------------------------------------------

const Install = Schema.Struct({ package: Schema.String, product: Schema.String, args: Schema.optionalKey(Schema.Array(Schema.String)) });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) =>
    Effect.runPromise(
        Effect.gen(function* () {
            const [{ package: root, product, args = [] }, fs, path] = yield* Effect.all([Schema.decodeUnknownEffect(Install)(options), FileSystem.FileSystem, Path.Path]);
            const swiftpm = yield* Config.String('XDG_CONFIG_HOME').pipe(
                Config.map((at) => path.join(at, 'swiftpm')),
                Config.orElse(() => Config.map(Config.String('HOME'), (home) => path.join(home, '.swiftpm'))),
            );
            const installed = yield* fs.exists(path.join(swiftpm, 'bin', product));
            const runs: readonly (readonly [string, ...string[]])[] = [
                ['swift', 'build', '--package-path', root, '--configuration', 'release', '--product', product, ...args],
                ...(installed ? [['swift', 'package', '--package-path', root, 'experimental-uninstall', product] as const] : []),
                ['swift', 'package', '--package-path', root, 'experimental-install', '--product', product],
            ];
            return yield* Effect.promise(() => run({ runs }, context));
        }).pipe(Effect.provide(layer)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
