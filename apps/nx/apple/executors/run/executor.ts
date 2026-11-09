import { env, platform } from 'node:process';
import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Effect, Record, Schema } from 'effect';
import { ChildProcess } from 'effect/process';
import { AppleError } from '#/plugins/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Run = Schema.Struct({ runs: Schema.Array(Schema.NonEmptyArray(Schema.String)), args: Schema.optionalKey(Schema.Array(Schema.String)) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const commands = Effect.fnUntraced(function* ({ runs, args = [] }: typeof Run.Type, root: string) {
    const cleared = Record.fromIterableWith([...Object.keys(env).filter((name) => name.startsWith('NX_')), 'LERNA_PACKAGE_NAME'], (name) => [name, undefined]);
    yield* Effect.forEach(
        runs,
        ([program, ...given]) =>
            Effect.scoped(Effect.flatMap(ChildProcess.make(program, [...given, ...args], { cwd: root, env: cleared, extendEnv: true, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' }), (handle) => handle.exitCode)).pipe(
                Effect.catchReason('PlatformError', 'NotFound', () => AppleError.make({ subject: program, detail: `is absent on this ${platform} host; projects tagged os:darwin run on macOS with Xcode` })),
                Effect.filterOrFail(
                    (code) => code === 0,
                    (code) => AppleError.make({ subject: program, detail: `exited ${code}` }),
                ),
            ),
        { discard: true },
    );
    return { success: true };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) =>
    Schema.decodeUnknownEffect(Run)(options).pipe(
        Effect.flatMap((run) => commands(run, context.root)),
        Effect.provide(layer),
        Effect.runPromise,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { commands, execute, Run };
