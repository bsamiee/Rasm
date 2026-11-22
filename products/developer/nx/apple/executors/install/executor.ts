import { pid } from 'node:process';
import type { ExecutorContext } from '@nx/devkit';
import { spawned } from '@rasm/nx-distribution/inference';
import { Array, Config, Effect, FileSystem, Path, Schema } from 'effect';
import { commands, configured } from '#/executors/run/executor.ts';
import { run, SIGNED, SWIFTPM } from '#/targets.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Install = Schema.Struct({
    package: Schema.String.annotate({ description: 'Package folder the plugin infers, passed as --package-path' }),
    product: Schema.String.annotate({ description: 'Executable product the plugin infers from swift package describe' }),
    entitlements: Schema.optionalKey(Schema.String).annotate({ description: 'Entitlements file <module>.entitlements among the sources of the module the product builds, signed into the product where present' }),
    __unparsed__: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Words after -- of the run, appended after the configured arguments', $default: { $source: 'unparsed' }, hidden: true, 'x-priority': 'internal' }),
}).annotate({
    title: 'Install',
    description: `Builds a SwiftPM executable product in release, signs a copy under the ${SIGNED.development} identity with the hardened runtime, and renames it over ~/.local/bin/<product>, the per-user folder launchd's PATH holds`,
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ package: root, product, entitlements, __unparsed__: words = [] }: typeof Install.Type, context: ExecutorContext) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const stated = yield* configured;
    const folder = path.join(yield* Config.String('HOME'), '.local', 'bin');
    const [at, staged] = [path.join(folder, product), path.join(folder, `${product}.${pid}`)];
    const build = Array.make('swift', 'build', '--package-path', root, ...SWIFTPM, '--configuration', 'release', '--product', product, ...words);
    const built = yield* commands({ runs: [build] }, context.root);
    return yield* built.success
        ? Effect.flatMap(spawned(Array.append(build, '--show-bin-path'), { cwd: context.root, env: stated }), (bin) =>
              fs.makeDirectory(folder, { recursive: true }).pipe(
                  Effect.andThen(fs.copyFile(path.join(bin.trim(), product), staged)),
                  Effect.andThen(spawned(['codesign', '--force', '--sign', SIGNED.development, '--options', 'runtime', ...(entitlements === undefined ? [] : ['--entitlements', entitlements]), staged], { cwd: context.root, env: stated })),
                  Effect.andThen(fs.rename(staged, at)),
                  Effect.onError(() => Effect.ignore(fs.remove(staged, { force: true }))),
                  Effect.andThen(Effect.logInfo(`Executable product \`${product}\` was signed and replaced at ${at} by rename, so a running process keeps the image it started from`)),
                  Effect.as({ success: true }),
              ),
          )
        : Effect.succeed(built);
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Install };
