import { pid } from 'node:process';
import type { ExecutorContext } from '@nx/devkit';
import { line } from '@rasm/nx-devkit/inference';
import { printed, runtime } from '@rasm/nx-devkit/process';
import { Config, Effect, FileSystem, Path, Schema } from 'effect';
import { commands } from '#/executors/run/executor';

// --- [MODELS] --------------------------------------------------------------------------

const Install = Schema.Struct({ package: Schema.String, product: Schema.String, env: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)), args: Schema.optionalKey(Schema.Array(Schema.String)) });

// --- [COMPOSITION] ---------------------------------------------------------------------

const installed = Effect.fnUntraced(function* (options: unknown, context: ExecutorContext) {
    const [{ package: root, product, env = {}, args = [] }, fs, path, dry] = yield* Effect.all([Schema.decodeUnknownEffect(Install)(options), FileSystem.FileSystem, Path.Path, Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false))]);
    const swiftpm = yield* Config.String('XDG_CONFIG_HOME').pipe(
        Config.map((configuration) => path.join(configuration, 'swiftpm')),
        Config.orElse(() => Config.map(Config.String('HOME'), (home) => path.join(home, '.swiftpm'))),
    );
    const at = path.join(swiftpm, 'bin', product);
    const build = ['build', '--package-path', root, '--configuration', 'release', '--product', product, ...args];
    const held = yield* fs.exists(at);
    if (dry) {
        return yield* Effect.as(Effect.logInfo([`Would run ${line(['swift', ...build])}, then`, held ? `rename the built ${product} over ${at}` : `install it to ${path.dirname(at)} through swift package experimental-install`].join(' ')), { success: true });
    }
    yield* commands({ runs: [['swift', ...build]], env }, context.root);
    return yield* held
        ? Effect.flatMap(printed('swift', [...build, '--show-bin-path'], { cwd: context.root, env }), (folder) =>
              fs.copyFile(path.join(folder.trim(), product), `${at}.${pid}`).pipe(
                  Effect.andThen(fs.rename(`${at}.${pid}`, at)),
                  Effect.onError(() => Effect.ignore(fs.remove(`${at}.${pid}`, { force: true }))),
                  Effect.andThen(Effect.logInfo(`Executable product \`${product}\` was replaced at ${at} by rename, so a running process keeps the image it started from`)),
                  Effect.as({ success: true }),
              ),
          )
        : commands({ runs: [['swift', 'package', '--package-path', root, ...args, 'experimental-install', '--product', product]], env }, context.root);
});

const execute = (options: unknown, context: ExecutorContext) => installed(options, context).pipe(runtime.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
