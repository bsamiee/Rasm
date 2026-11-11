import { printed, runtime } from '@rasm/nx-devkit/process';
import { Cause, Effect, Schedule, Schema, String } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Listed = Schema.Array(Schema.fromJsonString(Schema.Struct({ Service: Schema.String, State: Schema.String, Health: Schema.String }))).annotate({ description: 'The lines of docker compose ps --format json' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const waitForDevServer = (root: string, ...services: readonly string[]) =>
    printed('docker', ['compose', '--project-directory', root, 'ps', '--format', 'json', ...services]).pipe(
        Effect.flatMap((stdout) => Schema.decodeEffect(Listed)(stdout.split('\n').filter(String.isNonEmpty))),
        Effect.filterOrFail(
            (listed) => services.every((service) => listed.some(({ Service, State, Health }) => Service === service && State === 'running' && ['', 'healthy'].includes(Health))),
            (listed) => new Cause.IllegalArgumentError(`Compose reports ${services.join(', ')} of ${root} as ${listed.map(({ Service, State, Health }) => [Service, State, Health].filter(String.isNonEmpty).join(' ')).join(', ')}`),
        ),
        Effect.retry(Schedule.spaced('250 millis')),
        runtime.runPromise,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { waitForDevServer };
