import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Array, ConfigProvider, Effect, FileSystem, Path, Record, Schema } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [MODELS] --------------------------------------------------------------------------

const Invocation = Schema.Struct({
    command: Schema.NonEmptyArray(Schema.String),
    env: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)),
    results: Schema.optionalKey(Schema.Array(Schema.String)),
});
const Run = Schema.Struct({ runs: Schema.Array(Invocation), args: Schema.optionalKey(Schema.Array(Schema.String)) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const variables = (provider: ConfigProvider.ConfigProvider, at: readonly string[]): Effect.Effect<readonly string[], ConfigProvider.SourceError> =>
    Effect.flatMap(provider.load(at), (node) =>
        node === undefined
            ? Effect.succeed([])
            : node._tag === 'Value'
              ? Effect.succeed([at.join('_')])
              : Effect.map(Effect.forEach(node._tag === 'Record' ? [...node.keys] : Array.range(0, node.length - 1).map(String), (key) => variables(provider, [...at, key])), (nested) => [
                    ...(node.value === undefined ? [] : [at.join('_')]),
                    ...nested.flat(),
                ]),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) =>
    Effect.runPromise(
        Effect.gen(function* () {
            const [path, fs, provider, run] = yield* Effect.all([Path.Path, FileSystem.FileSystem, ConfigProvider.ConfigProvider, Schema.decodeUnknownEffect(Run)(options)]);
            const removed = Record.fromIterableWith([...(yield* variables(provider, ['NX'])), 'LERNA_PACKAGE_NAME'], (name) => [name, undefined] as const);
            return yield* Effect.forEach(run.runs, ({ command: [program, ...args], env, results }) =>
                Effect.andThen(
                    Effect.forEach(results ?? [], (at) => Effect.andThen(fs.remove(path.resolve(context.root, at), { recursive: true, force: true }), fs.makeDirectory(path.dirname(path.resolve(context.root, at)), { recursive: true }))),
                    Effect.scoped(
                        Effect.flatMap(
                            ChildProcess.make(program, [...args, ...(run.args ?? [])], { cwd: context.root, extendEnv: true, env: { ...removed, ...env }, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' }),
                            (handle) => handle.exitCode,
                        ),
                    ),
                ).pipe(Effect.filterOrFail((code) => code === 0)),
            ).pipe(
                Effect.as({ success: true }),
                Effect.catchTag('NoSuchElementError', () => Effect.succeed({ success: false })),
            );
        }).pipe(Effect.provide(layer)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Invocation };
