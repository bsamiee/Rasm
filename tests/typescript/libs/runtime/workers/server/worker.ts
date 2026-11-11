import { env } from 'cloudflare:workers';
import { Worker } from '@rasm/workers';
import { Execution } from '@rasm/workers/execution';
import { Context, Effect, Layer, Record, Schema } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';

// --- [CONSTANTS] -----------------------------------------------------------------------

const keys = { kept: 'kept', scheduled: 'scheduled', built: 'built' } as const;
const refused = 'refused';
const answers = { served: 200, accepted: 202, missing: 404 } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Job = Schema.Struct({ key: Schema.String, value: Schema.String });
const Keyed = Schema.Struct({ key: Schema.String });

// --- [SERVICES] ------------------------------------------------------------------------

class Isolate extends Context.Service<Isolate, { readonly built: string; readonly results: KVNamespace; readonly producer: Queue<typeof Job.Type> }>()('@rasm/test-workers/server/worker/Isolate') {
    static readonly layer = Layer.effect(
        this,
        Effect.all({
            built: SecureRandom.SecureRandom.use(({ uuidV7 }) => uuidV7()),
            results: Effect.fromOption(Record.get(env, 'RESULTS')),
            producer: Effect.fromOption(Record.get(env, 'PRODUCER')),
        }),
    ).pipe(Layer.provide(SecureRandom.layerWebCrypto));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const routes = HttpRouter.use(
    Effect.fnUntraced(function* (router) {
        const { built, results, producer } = yield* Isolate;
        yield* router.add('GET', '/', HttpServerResponse.text(built));
        yield* router.add(
            'GET',
            '/kept',
            Effect.as(
                Execution.waitUntil(
                    Effect.andThen(
                        Effect.sleep('50 millis'),
                        Effect.promise(() => results.put(keys.kept, keys.kept)),
                    ),
                ),
                HttpServerResponse.text('Accepted', { status: answers.accepted }),
            ),
        );
        yield* router.add(
            'POST',
            '/jobs/:key',
            Effect.gen(function* () {
                const [{ key }, value] = yield* Effect.all([HttpRouter.schemaPathParams(Keyed), Effect.flatMap(HttpServerRequest.HttpServerRequest, (request) => request.text)]);
                yield* Effect.promise(() => producer.send({ key, value }));
                return HttpServerResponse.text('Accepted', { status: answers.accepted });
            }),
        );
        yield* router.add(
            'GET',
            '/jobs/:key',
            Effect.gen(function* () {
                const { key } = yield* HttpRouter.schemaPathParams(Keyed);
                const value = yield* Effect.promise(() => results.get(key));
                return value === null ? HttpServerResponse.empty({ status: answers.missing }) : HttpServerResponse.text(value);
            }),
        );
    }),
).pipe(Layer.provide(Isolate.layer));

// --- [COMPOSITION] ---------------------------------------------------------------------

export default Worker(
    Isolate.layer,
    Worker.fetch(routes),
    Worker.scheduled(({ cron, scheduledTime }) => Effect.flatMap(Isolate, ({ built, results }) => Effect.promise(() => Promise.all([results.put(keys.scheduled, `${cron}@${scheduledTime}`), results.put(`${keys.built}:${keys.scheduled}`, built)])))),
    Worker.queue(Job, ({ key, value }, { id, attempts }) => (value === refused ? Effect.fail(id) : Effect.flatMap(Isolate, ({ built, results }) => Effect.promise(() => Promise.all([results.put(key, `${value.toUpperCase()}:${attempts}`), results.put(`${keys.built}:${key}`, built)]))))),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { answers, Job, keys, refused };
