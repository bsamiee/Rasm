import { layer } from '@effect/platform-node/NodeServices';
import { runReadyCheck } from '@nx/devkit/internal';
import { Completed } from '@rasm/process';
import { Array, Cause, Effect, Schedule, Schema, String } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [MODELS] --------------------------------------------------------------------------

const Running = Schema.Array(Schema.fromJsonString(Schema.Struct({ Service: Schema.String, State: Schema.Literal('running'), Health: Schema.Literals(['', 'healthy']) }))).annotate({
    description: 'The lines of docker compose ps --format json where every listed container runs and passes its healthcheck or states none',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const waitForDevServer = (...services: readonly string[]) =>
    runReadyCheck(({ root }) =>
        Completed.output(ChildProcess.make('docker', ['compose', 'ps', '--format', 'json', ...services], { cwd: root, stdin: 'ignore' })).pipe(
            Effect.flatMap((stdout) => Schema.decodeEffect(Running)(stdout.split('\n').filter(String.isNonEmpty))),
            Effect.map((running) =>
                Array.difference(
                    services,
                    Array.map(running, ({ Service }) => Service),
                ),
            ),
            Effect.filterOrFail(Array.isReadonlyArrayEmpty, (absent) => new Cause.NoSuchElementError(`Compose lists no running container of ${absent.join(', ')} in ${root}`)),
            Effect.retry(Schedule.spaced('250 millis')),
            Effect.as(null),
            Effect.provide(layer),
            Effect.runPromise,
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { waitForDevServer };
