import { assert, describe, it } from '@effect/vitest';
import { Array, Effect, Layer, Option, Result, Schema, Stream, SubscriptionRef } from 'effect';
import type { McpTest } from 'effect/ai';
import { connected, SETTLED } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TEMPLATE = 'test://template/{id}/data';
const URIS = { alpha: 'test://template/alpha/data', beta: 'test://template/beta/data', opal: 'test://template/opal/data', watched: 'test://watched-resource' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Updated = Schema.Struct({ uri: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const updated = (client: McpTest.Client, until: string) =>
    Stream.fromQueue(client.notifications).pipe(
        Stream.filterMap(({ method, payload }) =>
            Result.fromOption(
                Option.filter(
                    Option.map(Schema.decodeUnknownOption(Updated)(payload), ({ uri }) => uri),
                    () => method === 'notifications/resources/updated',
                ),
                () => undefined,
            ),
        ),
        Stream.takeUntil((uri) => uri === until),
        Stream.runCollect,
        Effect.timeout(SETTLED),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('resources of the server', () => {
    it.live(
        'completes a template argument with only the values matching the typed text, the value it starts first, and their count as total',
        Effect.fnUntraced(function* () {
            const { client } = yield* connected(Layer.empty);
            const { completion } = yield* client.complete({ ref: { type: 'ref/resource', uri: TEMPLATE }, argument: { name: 'id', value: 'al' } });
            assert.deepStrictEqual([completion.values, completion.total], [['alpha', 'opal'], 2]);
        }),
    );

    it.live(
        'notifies a subscriber of a resource it never read with its URI when the value of the resource changes',
        Effect.fnUntraced(function* () {
            const { probe, client } = yield* connected(Layer.empty);
            yield* client.subscribe({ uri: URIS.watched });
            yield* SubscriptionRef.set(probe.watched, 'moved');
            assert.deepStrictEqual(yield* updated(client, URIS.watched), [URIS.watched]);
        }),
    );

    it.live(
        'notifies the URI of each read template entry whose value changed, never one whose value held',
        Effect.fnUntraced(function* () {
            const { probe, client } = yield* connected(Layer.empty);
            yield* Effect.forEach([URIS.alpha, URIS.beta, URIS.opal], (uri) => Effect.andThen(client.subscribe({ uri }), client.readResource({ uri })), { discard: true });
            yield* SubscriptionRef.set(probe.versions, { alpha: 1 });
            const first = yield* updated(client, URIS.alpha);
            yield* SubscriptionRef.set(probe.versions, { alpha: 1, opal: 1 });
            assert.deepStrictEqual(Array.appendAll(first, yield* updated(client, URIS.opal)), [URIS.alpha, URIS.opal]);
        }),
    );
});
