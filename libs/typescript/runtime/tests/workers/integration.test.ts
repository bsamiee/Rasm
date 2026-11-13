import { env, exports } from 'cloudflare:workers';
import { assert, it } from '@effect/vitest';
import { Array, DateTime, Duration, Effect, Schedule } from 'effect';
import { answers, type Job, refused } from '#/server/worker';

// --- [CONSTANTS] -----------------------------------------------------------------------

const interval = Duration.toMillis('1 second');

// --- [OPERATIONS] ----------------------------------------------------------------------

const running = Effect.fromNullishOr(exports.default);
const fetched = Effect.fnUntraced(function* (path: string, init?: RequestInit) {
    const self = yield* running;
    const response = yield* Effect.promise(() => self.fetch(`https://example.com${path}`, init));
    return { status: response.status, text: yield* Effect.promise(() => response.text()) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.effect(
    'dispatches fetch events to the running Worker, on one layer build',
    Effect.fnUntraced(function* () {
        const [first, second, missing] = yield* Effect.all([fetched('/'), fetched('/'), fetched('/absent')]);
        assert.deepStrictEqual({ first: first.status, second, missing: missing.status }, { first: answers.served, second: first, missing: answers.missing });
    }),
);

it.effect(
    'dispatches a scheduled event to the running Worker',
    Effect.fnUntraced(function* () {
        const self = yield* running;
        const result = yield* Effect.promise(() => self.scheduled({ scheduledTime: DateTime.toDateUtc(DateTime.makeUnsafe(interval)), cron: '30 * * * *' }));
        assert.strictEqual(result.outcome, 'ok');
    }),
);

it.effect(
    'consumes queue messages sent to the running Worker, retrying the message of failed work alone',
    Effect.fnUntraced(function* () {
        const self = yield* running;
        const bodies: Array.NonEmptyReadonlyArray<typeof Job.Type> = [
            { key: '/6', value: 'six' },
            { key: '/7', value: refused },
        ];
        const messages: ServiceBindingQueueMessage<typeof Job.Type>[] = Array.map(bodies, (body, index) => ({ id: `${body.key}-${index}`, timestamp: DateTime.toDateUtc(DateTime.makeUnsafe(interval * (index + 1))), attempts: 1, body }));
        const { outcome, retryBatch, ackAll, retryMessages, explicitAcks } = yield* Effect.promise(() => self.queue(env.QUEUE, messages));
        assert.deepStrictEqual({ outcome, retryBatch: retryBatch.retry, ackAll, retryMessages: Array.map(retryMessages, ({ msgId }) => msgId), explicitAcks }, { outcome: 'ok', retryBatch: false, ackAll: false, retryMessages: ['/7-1'], explicitAcks: ['/6-0'] });
    }),
);

it.live(
    'produces a queue message from a fetch event and consumes it',
    Effect.fnUntraced(function* () {
        const accepted = yield* fetched('/jobs/key', { method: 'POST', body: 'value' });
        const worked = yield* Effect.repeat(fetched('/jobs/key'), { until: ({ status }) => status === answers.served, schedule: Schedule.spaced('50 millis') });
        assert.deepStrictEqual(
            [accepted, worked],
            [
                { status: answers.accepted, text: 'Accepted' },
                { status: answers.served, text: 'VALUE:1' },
            ],
        );
    }),
);
