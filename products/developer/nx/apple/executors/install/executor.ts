import { installed } from '@rasm/nx/executor';
import { executor, spawned, Words } from '@rasm/nx/inference';
import { Array, Config, Effect, Path, Schema } from 'effect';
import { commands, configured } from '#/executors/run/executor';
import { SIGNED, SWIFTPM } from '#/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Install = Schema.Struct({
    package: Schema.String.annotate({ description: 'Package folder the plugin infers, passed as --package-path' }),
    product: Schema.String.annotate({ description: 'Executable product the plugin infers from swift package describe' }),
    entitlements: Schema.optionalKey(Schema.String).annotate({ description: 'Entitlements file <module>.entitlements among the sources of the module the product builds, signed into the product where present' }),
    __unparsed__: Words,
}).annotate({
    title: 'Install',
    description: `Builds a SwiftPM executable product in release, signs a copy staged beside ~/.local/bin/<product>, the per-user folder launchd's PATH holds, under the ${SIGNED.development} identity with the hardened runtime, and puts it there by one renamex_np, RENAME_SWAP exchanging it with the executable installed there, or where the volume refuses that rename with ENOTSUP, EINVAL, or EXDEV by two renames moving the installed executable aside first, leaving the path empty between them`,
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Install, function* ({ package: root, product, entitlements, __unparsed__: words = [] }, context) {
    const [path, stated, home] = yield* Effect.all([Path.Path, configured, Config.String('HOME')]);
    const at = path.join(home, '.local', 'bin', product);
    const build = Array.make('swift', 'build', '--package-path', root, ...SWIFTPM, '--configuration', 'release', '--product', product, ...words);
    const built = yield* commands({ runs: [build] }, context.root);
    return yield* built.success
        ? spawned(Array.append(build, '--show-bin-path'), { cwd: context.root, env: stated }).pipe(
              Effect.flatMap((bin) => installed([[path.join(bin.trim(), product), at]], (staged) => spawned(['codesign', '--force', '--sign', SIGNED.development, '--options', 'runtime', ...(entitlements === undefined ? [] : ['--entitlements', entitlements]), staged], { cwd: context.root, env: stated }))),
              Effect.andThen(Effect.logInfo(`Executable product \`${product}\` was signed and replaced at ${at} by rename, so a running process keeps the image it started from`)),
              Effect.as({ success: true }),
          )
        : Effect.succeed(built);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Install };
