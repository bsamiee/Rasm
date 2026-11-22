import { layerSecureRandom } from '@effect/platform-node/NodeCrypto';
import { assert, describe, it } from '@effect/vitest';
import { Upstream } from '@rasm/mcp';
import { Array, Duration, Effect, HashSet, MutableRef, Option, Queue, Stream, Struct, SubscriptionRef } from 'effect';
import { McpProtocol, type McpTest } from 'effect/ai';
import { FetchHttpClient } from 'effect/http';
import { echoed, names, PEER, proxied, REVISIONS, SETTLED, settled } from '#/conformance.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CHANGED = ['notifications/tools/list_changed', 'notifications/resources/list_changed', 'notifications/prompts/list_changed'] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const heardAll = (seen: HashSet.HashSet<string>) => CHANGED.every((method) => HashSet.has(seen, method));
const heard = (client: McpTest.Client) =>
    Stream.fromQueue(client.notifications).pipe(
        Stream.map(Struct.get('method')),
        Stream.scan(
            () => HashSet.empty<string>(),
            (seen, method) => HashSet.add(seen, method),
        ),
        Stream.takeUntil(heardAll),
        Stream.runLast,
        Effect.timeout(SETTLED),
        Effect.map(Option.exists(heardAll)),
    );
const listed = (client: McpTest.Client) =>
    Effect.map(Effect.all([names(client), client.listResources(), client.listResourceTemplates(), client.listPrompts()]), ([tools, { resources }, { resourceTemplates }, { prompts }]) => [
        tools.includes('echo'),
        resources.some(({ uri }) => uri === 'upstream://note'),
        resourceTemplates.some(({ name }) => name === 'item'),
        prompts.some(({ name }) => name === 'greet'),
    ]);

// --- [COMPOSITION] ---------------------------------------------------------------------

describe.each(REVISIONS)('$protocolVersion upstream session', (protocol) => {
    it.live(
        'hides every tool, resource, template, and prompt of an upstream that goes absent with each kind of list_changed, and lists them again when it returns',
        Effect.fnUntraced(function* () {
            const present = yield* SubscriptionRef.make(true);
            const { client } = yield* proxied(protocol, { presence: SubscriptionRef.changes(present) });
            yield* Queue.clear(client.notifications);
            yield* SubscriptionRef.set(present, false);
            const hidden = yield* heard(client);
            const gone = yield* listed(client);
            yield* Queue.clear(client.notifications);
            yield* SubscriptionRef.set(present, true);
            const shown = yield* heard(client);
            const back = yield* settled(listed(client), Array.every(Boolean));
            assert.deepStrictEqual([hidden, gone, shown, back], [true, [false, false, false, false], true, [true, true, true, true]]);
        }, Effect.scoped),
    );

    it.live(
        'reconnects with the current header and answers the call when the upstream refuses the old token with 401',
        Effect.fnUntraced(function* () {
            const { client, token, rejected } = yield* proxied(protocol, { poll: Duration.minutes(1) });
            MutableRef.set(token, 'second');
            const called = echoed(yield* client.callTool({ name: 'echo', arguments: { value: 'again' } }));
            assert.deepStrictEqual([called.result, MutableRef.get(rejected) > 0], [{ echoed: 'again' }, true]);
        }, Effect.scoped),
    );
});

describe('upstream session', () => {
    it.live(
        'lists nothing at once while its endpoint has not answered',
        Effect.fnUntraced(
            function* () {
                const first = yield* Stream.runHead(Upstream.mcp({ peer: PEER, enabled: true, endpoint: Effect.never, poll: SETTLED })).pipe(Effect.timeout(SETTLED));
                assert.deepStrictEqual(first, Option.some([]));
            },
            Effect.provide([FetchHttpClient.layer, layerSecureRandom]),
        ),
    );

    it.live(
        'opens a new session and answers the call when the upstream restarted and no longer holds its session',
        Effect.fnUntraced(function* () {
            const { client, restart } = yield* proxied(McpProtocol.v2025_11_25, { poll: Duration.minutes(1) });
            yield* restart;
            const called = echoed(yield* client.callTool({ name: 'echo', arguments: { value: 'after' } }));
            assert.deepStrictEqual(called.result, { echoed: 'after' });
        }, Effect.scoped),
    );
});
