import { layerSecureRandom } from '@effect/platform-node/NodeCrypto';
import { assert, describe, it } from '@effect/vitest';
import { Upstream } from '@rasm/mcp';
import { Array, Duration, Effect, Function, HashSet, MutableRef, Option, Queue, Result, Stream, Struct, SubscriptionRef } from 'effect';
import { McpProtocol, type McpTest } from 'effect/ai';
import { FetchHttpClient } from 'effect/http';
import { echoed, names, PEER, proxied, SETTLED, settled, TEXT } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROTOCOL = McpProtocol.v2025_11_25;
const CHANGED = ['notifications/tools/list_changed', 'notifications/resources/list_changed', 'notifications/prompts/list_changed'] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const heardAll = (seen: HashSet.HashSet<string>) => CHANGED.every((method) => HashSet.has(seen, method));
const heard = (client: McpTest.Client) =>
    Stream.fromQueue(client.notifications).pipe(
        Stream.map(Struct.get('method')),
        Stream.scan(Function.constant(HashSet.empty<string>()), (seen, method) => HashSet.add(seen, method)),
        Stream.takeUntil(heardAll),
        Stream.runLast,
        Effect.timeout(SETTLED),
        Effect.map(Option.exists(heardAll)),
    );
const listed = (client: McpTest.Client) =>
    Effect.all([
        Effect.map(names(client), Array.contains('echo')),
        Effect.map(client.listResources(), ({ resources }) => resources.some(({ uri }) => uri === 'upstream://note')),
        Effect.map(client.listResourceTemplates(), ({ resourceTemplates }) => resourceTemplates.some(({ name }) => name === 'item')),
        Effect.map(client.listPrompts(), ({ prompts }) => prompts.some(({ name }) => name === 'greet')),
    ]);

// --- [COMPOSITION] ---------------------------------------------------------------------

describe.each([McpProtocol.v2025_11_25, McpProtocol.v2026_07_28])('upstream speaking $protocolVersion', (protocol) => {
    it.live(
        'forwards a tool call with its result beside the handle, a resource read, template reads and completions, and a prompt with its completion',
        Effect.fnUntraced(function* () {
            const { client } = yield* proxied(protocol);
            const called = echoed(yield* client.callTool({ name: 'echo', arguments: { value: 'hi' } }));
            const read = yield* Effect.forEach(['upstream://note', 'upstream://items/apple', 'upstream://files/docs/guide.md'], (uri) => client.readResource({ uri }));
            const completed = yield* Effect.all([
                client.complete({ ref: { type: 'ref/resource', uri: 'upstream://items/{id}' }, argument: { name: 'id', value: 'ap' } }),
                client.complete({ ref: { type: 'ref/resource', uri: 'upstream://files/{+path}' }, argument: { name: 'path', value: 'gu' } }),
                client.complete({ ref: { type: 'ref/prompt', name: 'greet' }, argument: { name: 'who', value: 'wo' } }),
            ]);
            const greeted = yield* client.getPrompt({ name: 'greet', arguments: { who: 'there' } });
            assert.deepStrictEqual<unknown>(
                {
                    called,
                    read: Array.map(Array.flatMap(read, Struct.get('contents')), (content) => [content.uri, 'text' in content ? content.text : undefined]),
                    completed: Array.map(completed, ({ completion }) => completion.values.toSorted()),
                    greeted: Array.map(greeted.messages, ({ content }) => ('text' in content ? content.text : undefined)),
                },
                {
                    called: { handled: true, result: { echoed: 'hi' } },
                    read: [
                        ['upstream://note', 'upstream note'],
                        ['upstream://items/apple', 'item apple'],
                        ['upstream://files/docs/guide.md', 'file upstream://files/docs/guide.md'],
                    ],
                    completed: [['apple', 'apricot'], ['docs/guide.md'], ['world']],
                    greeted: ['hello there'],
                },
            );
        }),
    );
});

describe('upstream session', () => {
    it.live.each([
        { label: 'no filter', filter: {}, listed: ['echo', 'secret'], secret: { type: 'text', text: 'secret' } },
        { label: 'a denied name', filter: { deny: ['secret'] }, listed: ['echo'], secret: -32_602 },
        { label: 'an allowed pattern', filter: { allow: [/^ec/u] }, listed: ['echo'], secret: -32_602 },
        { label: 'a denied pattern over an allowed name', filter: { allow: ['echo', 'secret'], deny: [/^s/u] }, listed: ['echo'], secret: -32_602 },
    ] as const)(
        'lists the upstream tools its filter admits under $label, and answers a call to the tool secret with what its filter allows',

        Effect.fnUntraced(function* ({ filter, listed: admitted, secret }) {
            const { client } = yield* proxied(PROTOCOL, { filters: { tools: filter } });
            const shown = yield* settled(names(client), Array.contains<string>(Array.headNonEmpty(admitted)));
            const answer = yield* Effect.result(client.callTool({ name: 'secret', arguments: {} }));
            assert.deepStrictEqual([Array.filter(shown, (name) => name === 'echo' || name === 'secret').toSorted(), Result.match(answer, { onFailure: Struct.get('code'), onSuccess: ({ content }) => content[0] })], [admitted, secret]);
        }),
    );

    it.live(
        'answers a tool sharing a name with a tool of the server from the server, never the upstream',
        Effect.fnUntraced(function* () {
            const { client } = yield* proxied(PROTOCOL);
            yield* settled(names(client), Array.contains('echo'));
            const { content } = yield* client.callTool({ name: 'test_simple_text', arguments: {} });
            assert.deepStrictEqual(content[0], { type: 'text', text: `"${TEXT}"` });
        }),
    );

    it.live(
        'hides the tool, resource, template, and prompt an upstream offers while it is absent, with each kind of list_changed, and lists them again when it returns',
        Effect.fnUntraced(function* () {
            const present = yield* SubscriptionRef.make(true);
            const { client } = yield* proxied(PROTOCOL, { presence: SubscriptionRef.changes(present) });
            yield* Queue.clear(client.notifications);
            yield* SubscriptionRef.set(present, false);
            const hidden = yield* heard(client);
            const gone = yield* listed(client);
            yield* Queue.clear(client.notifications);
            yield* SubscriptionRef.set(present, true);
            const shown = yield* heard(client);
            const back = yield* settled(listed(client), Array.every(Boolean));
            assert.deepStrictEqual([hidden, gone, shown, back], [true, [false, false, false, false], true, [true, true, true, true]]);
        }),
    );

    it.live(
        'reconnects with the current header and answers the call when the upstream refuses the old token with 401',
        Effect.fnUntraced(function* () {
            const { client, token, rejected } = yield* proxied(PROTOCOL, { poll: Duration.minutes(1) });
            MutableRef.set(token, 'second');
            const called = echoed(yield* client.callTool({ name: 'echo', arguments: { value: 'again' } }));
            assert.deepStrictEqual([called.result, MutableRef.get(rejected) > 0], [{ echoed: 'again' }, true]);
        }),
    );

    it.live(
        'opens a new session and answers the call when the upstream restarted and no longer holds its session',
        Effect.fnUntraced(function* () {
            const { client, restart } = yield* proxied(PROTOCOL, { poll: Duration.minutes(1) });
            yield* restart;
            const called = echoed(yield* client.callTool({ name: 'echo', arguments: { value: 'after' } }));
            assert.deepStrictEqual(called.result, { echoed: 'after' });
        }),
    );

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
});
