import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Effect, Record, Schema } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [MODELS] --------------------------------------------------------------------------

const Run = Schema.Struct({ runs: Schema.Array(Schema.NonEmptyArray(Schema.String)), args: Schema.optionalKey(Schema.Array(Schema.String)) });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) =>
    Effect.runPromise(
        Effect.gen(function* () {
            const [run, env] = yield* Effect.all([Schema.decodeUnknownEffect(Run)(options), Effect.sync(() => Record.filter(process.env, (_, name) => !name.startsWith('NX_') && name !== 'LERNA_PACKAGE_NAME'))]);
            return yield* Effect.forEach(run.runs, ([program, ...args]) =>
                Effect.scoped(Effect.flatMap(ChildProcess.make(program, [...args, ...(run.args ?? [])], { cwd: context.root, env, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' }), (handle) => handle.exitCode)).pipe(
                    Effect.filterOrFail((code) => code === 0),
                ),
            ).pipe(
                Effect.as({ success: true }),
                Effect.catchTag('NoSuchElementError', () => Effect.succeed({ success: false })),
            );
        }).pipe(Effect.provide(layer)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Run };
