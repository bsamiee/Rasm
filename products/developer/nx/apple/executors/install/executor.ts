import { pid } from 'node:process';
import type { ExecutorContext } from '@nx/devkit';
import { Array, Config, Effect, FileSystem, Path, Schema } from 'effect';
import { commands, configured } from '#/executors/run/executor';
import { run, SWIFTPM, spawned } from '#/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Install = Schema.Struct({
    package: Schema.String.annotate({ description: 'Package folder the plugin infers, passed as --package-path' }),
    product: Schema.String.annotate({ description: 'Executable product the plugin infers from swift package describe' }),
    __unparsed__: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Words after -- of the run, appended after the configured arguments', $default: { $source: 'unparsed' }, hidden: true, 'x-priority': 'internal' }),
}).annotate({ title: 'Install', description: "Builds a SwiftPM executable product in release and renames it over the copy SwiftPM's install folder holds, or installs it through experimental-install when the folder holds none" });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ package: root, product, __unparsed__: words = [] }: typeof Install.Type, context: ExecutorContext) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const stated = yield* configured;
    const swiftpm = yield* Config.String('XDG_CONFIG_HOME').pipe(
        Config.map((configuration) => path.join(configuration, 'swiftpm')),
        Config.orElse(() => Config.map(Config.String('HOME'), (home) => path.join(home, '.swiftpm'))),
    );
    const at = path.join(swiftpm, 'bin', product);
    const build = Array.make('swift', 'build', '--package-path', root, ...SWIFTPM, '--configuration', 'release', '--product', product, ...words);
    const held = yield* fs.exists(at);
    const built = yield* commands({ runs: [build, ...(held ? [] : [Array.make('swift', 'package', '--package-path', root, ...SWIFTPM, ...words, 'experimental-install', '--product', product)])] }, context.root);
    return yield* held && built.success
        ? Effect.flatMap(spawned(Array.append(build, '--show-bin-path'), { cwd: context.root, env: stated }), (folder) =>
              fs.copyFile(path.join(folder.trim(), product), `${at}.${pid}`).pipe(
                  Effect.andThen(fs.rename(`${at}.${pid}`, at)),
                  Effect.onError(() => Effect.ignore(fs.remove(`${at}.${pid}`, { force: true }))),
                  Effect.andThen(Effect.logInfo(`Executable product \`${product}\` was replaced at ${at} by rename, so a running process keeps the image it started from`)),
                  Effect.as({ success: true }),
              ),
          )
        : Effect.succeed(built);
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Install };
