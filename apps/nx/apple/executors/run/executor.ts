import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Array, ConfigProvider, Effect, Match, Record, Schema } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [MODELS] --------------------------------------------------------------------------

const Run = Schema.Struct({ runs: Schema.Array(Schema.NonEmptyArray(Schema.String)), args: Schema.optionalKey(Schema.Array(Schema.String)) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const variables = (provider: ConfigProvider.ConfigProvider, path: readonly string[]): Effect.Effect<readonly string[], ConfigProvider.SourceError> =>
    Effect.flatMap(provider.load(path), (node) =>
        node === undefined
            ? Effect.succeed([])
            : Effect.map(
                  Effect.forEach(Match.valueTags(node, { Value: () => [], Record: ({ keys }) => [...keys], Array: ({ length }) => Array.makeBy(length, String) }), (key) => variables(provider, [...path, key])),
                  (nested) => [...(node.value === undefined ? [] : [path.join('_')]), ...nested.flat()],
              ),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) =>
    Effect.runPromise(
        Effect.gen(function* () {
            const [run, names] = yield* Effect.all([Schema.decodeUnknownEffect(Run)(options), variables(ConfigProvider.fromEnv(), ['NX'])]);
            const env = Record.fromIterableWith([...names, 'LERNA_PACKAGE_NAME'], (name) => [name, undefined]);
            return yield* Effect.forEach(run.runs, ([program, ...args]) =>
                Effect.scoped(Effect.flatMap(ChildProcess.make(program, [...args, ...(run.args ?? [])], { cwd: context.root, env, extendEnv: true, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' }), (handle) => handle.exitCode)).pipe(Effect.filterOrFail((code) => code === 0)),
            ).pipe(
                Effect.as({ success: true }),
                Effect.catchTag('NoSuchElementError', () => Effect.succeed({ success: false })),
            );
        }).pipe(Effect.provide(layer)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Run };
