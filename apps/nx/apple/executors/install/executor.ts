import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Config, Effect, FileSystem, Path, Schema } from 'effect';
import { commands } from '#/executors/run/executor';

// --- [MODELS] --------------------------------------------------------------------------

const Install = Schema.Struct({ package: Schema.String, product: Schema.String, args: Schema.optionalKey(Schema.Array(Schema.String)) });

// --- [COMPOSITION] ---------------------------------------------------------------------

const installed = Effect.fnUntraced(function* (options: unknown, context: ExecutorContext) {
    const [{ package: root, product, args = [] }, fs, path] = yield* Effect.all([Schema.decodeUnknownEffect(Install)(options), FileSystem.FileSystem, Path.Path]);
    const swiftpm = yield* Config.String('XDG_CONFIG_HOME').pipe(
        Config.map((at) => path.join(at, 'swiftpm')),
        Config.orElse(() => Config.map(Config.String('HOME'), (home) => path.join(home, '.swiftpm'))),
    );
    const held = yield* fs.exists(path.join(swiftpm, 'bin', product));
    const runs: readonly (readonly [string, ...string[]])[] = [
        ['swift', 'build', '--package-path', root, '--configuration', 'release', '--product', product, ...args],
        ...(held ? [['swift', 'package', '--package-path', root, 'experimental-uninstall', product] as const] : []),
        ['swift', 'package', '--package-path', root, ...args, 'experimental-install', '--product', product],
    ];
    return yield* commands({ runs }, context.root);
});

const execute = (options: unknown, context: ExecutorContext) => installed(options, context).pipe(Effect.provide(layer), Effect.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
