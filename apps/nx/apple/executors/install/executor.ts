import { pid } from 'node:process';
import type { ExecutorContext } from '@nx/devkit';
import { Completed } from '@rasm/process';
import { Config, Effect, FileSystem, Path, Schema } from 'effect';
import { ChildProcess } from 'effect/process';
import { commands, run } from '#/executors/run/executor';

// --- [MODELS] --------------------------------------------------------------------------

const Install = Schema.Struct({
    package: Schema.String.annotate({ description: 'Package folder the plugin infers, passed as --package-path' }),
    product: Schema.String.annotate({ description: 'Executable product the plugin infers from swift package describe' }),
    env: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)).annotate({ description: "Environment variables every swift invocation runs with, over the caller's" }),
    args: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Arguments appended to the release build and placed ahead of experimental-install' }),
}).annotate({ title: 'Install', description: "Builds a SwiftPM executable product in release and renames it over the copy SwiftPM's install folder holds, or installs it through experimental-install when the folder holds none" });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ package: root, product, env = {}, args = [] }: typeof Install.Type, context: ExecutorContext) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const swiftpm = yield* Config.String('XDG_CONFIG_HOME').pipe(
        Config.map((configuration) => path.join(configuration, 'swiftpm')),
        Config.orElse(() => Config.map(Config.String('HOME'), (home) => path.join(home, '.swiftpm'))),
    );
    const at = path.join(swiftpm, 'bin', product);
    const build = ['build', '--package-path', root, '--configuration', 'release', '--product', product, ...args];
    const held = yield* fs.exists(at);
    yield* commands({ runs: [['swift', ...build]], env }, context.root);
    return yield* held
        ? Effect.flatMap(Completed.output(ChildProcess.make('swift', [...build, '--show-bin-path'], { cwd: context.root, env, extendEnv: true, stdin: 'ignore' })), (folder) =>
              fs.copyFile(path.join(folder.trim(), product), `${at}.${pid}`).pipe(
                  Effect.andThen(fs.rename(`${at}.${pid}`, at)),
                  Effect.onError(() => Effect.ignore(fs.remove(`${at}.${pid}`, { force: true }))),
                  Effect.andThen(Effect.logInfo(`Executable product \`${product}\` was replaced at ${at} by rename, so a running process keeps the image it started from`)),
                  Effect.as({ success: true }),
              ),
          )
        : commands({ runs: [['swift', 'package', '--package-path', root, ...args, 'experimental-install', '--product', product]], env }, context.root);
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Install };
