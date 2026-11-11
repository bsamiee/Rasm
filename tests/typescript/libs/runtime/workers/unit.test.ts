import { createExecutionContext, createMessageBatch, createScheduledController, getQueueResult, type MessageBatchMessage, waitOnExecutionContext } from 'cloudflare:test';
import { env } from 'cloudflare:workers';
import { assert, it } from '@effect/vitest';
import { Array, DateTime, Duration, Effect, Record } from 'effect';
import worker, { answers, type Job, keys, refused } from '#/server/worker';

// --- [CONSTANTS] -----------------------------------------------------------------------

const IncomingRequest = Request<unknown, IncomingRequestCfProperties>;
const interval = Duration.toMillis('1 second');
const cron = { scheduledTime: DateTime.toDateUtc(DateTime.makeUnsafe(interval)), cron: '30 * * * *' };
const uuidV7 = /^[0-9a-f]{8}-[0-9a-f]{4}-7[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const fetched = Effect.fnUntraced(function* (path: string) {
    const [fetch, ctx] = [yield* Effect.fromNullishOr(worker.fetch), createExecutionContext()];
    const response = yield* Effect.promise(() => Promise.resolve(fetch(new IncomingRequest(`http://example.com${path}`), env, ctx)));
    return { ctx, status: response.status, text: yield* Effect.promise(() => response.text()) };
});
const read = (key: string) => Effect.flatMap(Effect.fromOption(Record.get(env, 'RESULTS')), (results: KVNamespace) => Effect.promise(() => results.get(key)));
const consumed = Effect.fnUntraced(function* (...bodies: Array.NonEmptyReadonlyArray<typeof Job.Type>) {
    const sent: MessageBatchMessage<typeof Job.Type>[] = Array.map(bodies, (body, index) => ({ id: `${body.key}-${index}`, timestamp: DateTime.toDateUtc(DateTime.makeUnsafe(interval * (index + 1))), attempts: 1, body }));
    const [consume, ctx] = [yield* Effect.fromNullishOr(worker.queue), createExecutionContext()];
    const name: string = yield* Effect.fromOption(Record.get(env, 'QUEUE'));
    const batch = createMessageBatch(name, sent);
    yield* Effect.promise(() => Promise.resolve(consume(batch, env, ctx)));
    const { outcome, retryBatch, ackAll, retryMessages, explicitAcks } = yield* Effect.promise(() => getQueueResult(batch, ctx));
    return { outcome, retryBatch: retryBatch.retry, ackAll, retryMessages: Array.map(retryMessages, ({ msgId }) => msgId), explicitAcks };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.effect(
    'dispatches a fetch event, and builds the layer once for two events',
    Effect.fnUntraced(function* () {
        const [first, second] = yield* Effect.all([fetched('/'), fetched('/')]);
        yield* Effect.promise(() => Promise.all([waitOnExecutionContext(first.ctx), waitOnExecutionContext(second.ctx)]));
        assert.deepStrictEqual({ first: first.status, second: second.status, built: second.text }, { first: answers.served, second: answers.served, built: first.text });
        assert.match(first.text, uuidV7);
    }),
);

it.effect(
    'answers before work the handler leaves running ends, and the execution context waits for that work',
    Effect.fnUntraced(function* () {
        const { ctx, status, text } = yield* fetched('/kept');
        const before = yield* read(keys.kept);
        yield* Effect.promise(() => waitOnExecutionContext(ctx));
        assert.deepStrictEqual({ status, text, before, after: yield* read(keys.kept) }, { status: answers.accepted, text: 'Accepted', before: null, after: keys.kept });
    }),
);

it.effect(
    'dispatches a scheduled event with its controller, on the layer the fetch handler reads',
    Effect.fnUntraced(function* () {
        const [scheduled, ctx] = [yield* Effect.fromNullishOr(worker.scheduled), createExecutionContext()];
        yield* Effect.promise(() => Promise.resolve(scheduled(createScheduledController(cron), env, ctx)));
        yield* Effect.promise(() => waitOnExecutionContext(ctx));
        const { text: built } = yield* fetched('/');
        assert.deepStrictEqual([yield* read(keys.scheduled), yield* read(`${keys.built}:${keys.scheduled}`)], [`${cron.cron}@${cron.scheduledTime.getTime()}`, built]);
    }),
);

it.effect(
    'consumes queue messages, acknowledging each worked message alone, on the layer the fetch handler reads',
    Effect.fnUntraced(function* () {
        const result = yield* consumed({ key: '/1', value: 'one' }, { key: '/2', value: 'two' });
        const { text: built } = yield* fetched('/');
        assert.deepStrictEqual(result, { outcome: 'ok', retryBatch: false, ackAll: false, retryMessages: [], explicitAcks: ['/1-0', '/2-1'] });
        assert.deepStrictEqual(yield* Effect.all([read('/1'), read('/2'), read(`${keys.built}:/1`)]), ['ONE:1', 'TWO:1', built]);
    }),
);

it.effect(
    'retries the message of failed work alone, and acknowledges the worked messages around it',
    Effect.fnUntraced(function* () {
        const result = yield* consumed({ key: '/3', value: 'three' }, { key: '/4', value: refused }, { key: '/5', value: 'five' });
        assert.deepStrictEqual(result, { outcome: 'ok', retryBatch: false, ackAll: false, retryMessages: ['/4-1'], explicitAcks: ['/3-0', '/5-2'] });
        assert.deepStrictEqual(yield* Effect.all([read('/3'), read('/4'), read('/5')]), ['THREE:1', null, 'FIVE:1']);
    }),
);
