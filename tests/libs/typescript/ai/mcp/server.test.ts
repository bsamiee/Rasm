import { layerAead, layerSecureRandom } from '@effect/platform-node/NodeCrypto';
import { assert, describe, it } from '@effect/vitest';
import { Link, Server, Upstream } from '@rasm/mcp';
import { Array, Duration, Effect, HashSet, Layer, MutableRef, Option, Queue, Result, Schedule, Schema, Stream, Struct, SubscriptionRef } from 'effect';
import { McpProtocol, McpSchema, McpServer, McpTest, Tool, Toolkit } from 'effect/ai';
import { FetchHttpClient, HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { MANIFEST, SETTINGS, STATEFUL } from '#/conformance';
import { app, Probe, TEXT } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PATH = '/mcp';
const UPSTREAM = new URL(PATH, 'http://upstream.test');
const PEER = Link.make({ application: 'Upstream', extension: 'mcp', root: Option.none() });
const REVISIONS = [McpProtocol.v2026_07_28, McpProtocol.v2025_11_25, McpProtocol.v2025_06_18, McpProtocol.v2025_03_26] as const;
const CHANGED = ['notifications/tools/list_changed', 'notifications/resources/list_changed', 'notifications/prompts/list_changed'] as const;
const ITEMS = ['apple', 'apricot', 'banana'] as const;
const SETTLED = Duration.seconds(5);

// --- [MODELS] --------------------------------------------------------------------------

const Echoing = Schema.Struct({ echoed: Schema.String });
const Handled = Schema.Struct({ handle: Schema.String });
const Text = Schema.fromJsonString(Echoing);
const Tools = Toolkit.make(
    Tool.make('echo', { description: 'Echoes its value', parameters: Schema.Struct({ value: Schema.String }), success: Echoing }),
    Tool.make('secret', { description: 'Refused by the filter', success: Schema.String }),
    Tool.make('test_simple_text', { description: 'Shares a name with a tool of the server', success: Schema.String }),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const registrations = Layer.mergeAll(
    McpServer.toolkit(Tools).pipe(Layer.provide(Tools.toLayer({ echo: ({ value }) => Effect.succeed({ echoed: value }), secret: () => Effect.succeed('secret'), test_simple_text: () => Effect.succeed('upstream') }))),
    Layer.effectDiscard(McpServer.registerResource({ uri: 'upstream://note', name: 'note', mimeType: 'text/plain', content: Effect.succeed('upstream note') })),
    Layer.effectDiscard(McpServer.registerResource`upstream://items/${McpSchema.param('id', Schema.String)}`({ name: 'item', mimeType: 'text/plain', completion: { id: () => Effect.succeed([...ITEMS]) }, content: (_, id) => Effect.succeed(`item ${id}`) })),
    Layer.effectDiscard(McpServer.registerPrompt({ name: 'greet', parameters: { who: Schema.String }, completion: { who: () => Effect.succeed(['world', 'wide']) }, content: ({ who }) => Effect.succeed(`hello ${who}`) })),
);
const echoed = ({ content, structuredContent }: McpSchema.CallToolResult) => ({ handled: Schema.is(Handled)(structuredContent), result: Option.getOrUndefined(Array.findFirst(content, (block) => ('text' in block ? Schema.decodeOption(Text)(block.text) : Option.none()))) });
const names = (client: McpTest.Client) => Effect.map(client.listTools(), ({ tools }) => Array.map(tools, Struct.get('name')));
const settled = <A, E>(effect: Effect.Effect<A, E>, done: (value: A) => boolean) => effect.pipe(Effect.repeat({ schedule: Schedule.spaced('20 millis'), until: done }), Effect.timeout(SETTLED));
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
const guarded = (token: MutableRef.MutableRef<string>, rejected: MutableRef.MutableRef<number>) =>
    HttpRouter.middleware(
        (next) =>
            Effect.flatMap(HttpServerRequest.HttpServerRequest, ({ headers }) =>
                headers['authorization'] === `Bearer ${MutableRef.get(token)}`
                    ? next
                    : Effect.as(
                          Effect.sync(() => MutableRef.increment(rejected)),
                          HttpServerResponse.empty({ status: 401 }),
                      ),
            ),
        { global: true },
    );
const upstream = (protocol: (typeof REVISIONS)[number], token: MutableRef.MutableRef<string>, rejected: MutableRef.MutableRef<number>) =>
    Effect.acquireRelease(
        Effect.sync(() => HttpRouter.toWebHandler(Layer.merge(McpServer.layerHttp({ name: 'upstream', version: '1.0.0', path: PATH, protocols: [protocol], registrations }).pipe(Layer.provideMerge(McpServer.McpServer.layer)), guarded(token, rejected)), { disableLogger: true })),
        ({ dispose }) => Effect.promise(dispose),
    );

const proxied = Effect.fnUntraced(function* (protocol: (typeof REVISIONS)[number], { poll = Duration.millis(50), presence = Stream.succeed(true) }: { readonly poll?: Duration.Duration; readonly presence?: Stream.Stream<boolean> } = {}) {
    const [token, rejected] = [MutableRef.make('first'), MutableRef.make(0)];
    const [probe, scheme, serving] = yield* Effect.all([Probe.make, Schema.decodeEffect(Server.Scheme)(MANIFEST.name), upstream(protocol, token, rejected)]);
    const handler = MutableRef.make(serving.handler);
    const fetch: typeof globalThis.fetch = (input, init) => MutableRef.get(handler)(new Request(input, init));
    const proxy = Layer.effectDiscard(
        Upstream.serve(
            [
                Upstream.mcp({
                    peer: PEER,
                    enabled: true,
                    url: UPSTREAM,
                    headers: Effect.sync(() => ({ authorization: `Bearer ${MutableRef.get(token)}` })),
                    filters: { tools: { deny: ['secret'] } },
                    presence,
                    poll,
                }),
            ],
            { route: (peers) => Effect.succeed(Array.headNonEmpty(peers)) },
        ),
    ).pipe(Layer.provide([FetchHttpClient.layer, Layer.succeed(FetchHttpClient.Fetch, fetch), layerSecureRandom]));
    const client = yield* McpTest.makeClient(Layer.provide(Layer.mergeAll(app(probe), proxy), Server.layer({ ...SETTINGS, scheme }).pipe(Layer.provide([layerAead, layerSecureRandom]))), { protocols: [McpProtocol.v2025_11_25] });
    yield* Effect.addFinalizer(() => probe.release.open);
    yield* settled(names(client), Array.contains('echo'));
    return { client, token, rejected, restart: Effect.map(upstream(protocol, token, rejected), ({ handler: next }) => MutableRef.set(handler, next)) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

describe.each(STATEFUL)('$protocolVersion server', (protocol) => {
    it.live(
        'notifies a subscriber of a resource with its URI when the value of the resource changes',
        Effect.fnUntraced(function* () {
            const [probe, scheme] = yield* Effect.all([Probe.make, Schema.decodeEffect(Server.Scheme)(MANIFEST.name)]);
            const client = yield* McpTest.makeClient(Layer.provide(app(probe), Server.layer({ ...SETTINGS, scheme }).pipe(Layer.provide([layerAead, layerSecureRandom]))), { protocols: [protocol] });
            yield* Effect.addFinalizer(() => probe.release.open);
            yield* client.subscribe({ uri: 'test://watched-resource' });
            yield* SubscriptionRef.set(probe.watched, 'moved');
            const updated = yield* Queue.take(client.notifications).pipe(
                Effect.repeat({ until: ({ method }) => method === 'notifications/resources/updated' }),
                Effect.map(({ payload }) => payload),
                Effect.timeoutOption('2 seconds'),
            );
            assert.deepStrictEqual(updated, Option.some({ uri: 'test://watched-resource' }));
        }, Effect.scoped),
    );
});

describe.each(REVISIONS)('$protocolVersion upstream', (protocol) => {
    it.live(
        'proxies a tool call with its result beside the handle, a resource read, a template read and completion, and a prompt with its completion',
        Effect.fnUntraced(function* () {
            const { client } = yield* proxied(protocol);
            const called = echoed(yield* client.callTool({ name: 'echo', arguments: { value: 'hi' } }));
            const [note, item] = yield* Effect.all([client.readResource({ uri: 'upstream://note' }), client.readResource({ uri: 'upstream://items/apple' })]);
            const [items, people] = yield* Effect.all([client.complete({ ref: { type: 'ref/resource', uri: 'upstream://items/{id}' }, argument: { name: 'id', value: 'ap' } }), client.complete({ ref: { type: 'ref/prompt', name: 'greet' }, argument: { name: 'who', value: 'wo' } })]);
            const greeted = yield* client.getPrompt({ name: 'greet', arguments: { who: 'there' } });
            assert.deepStrictEqual(
                [
                    called,
                    Array.map(note.contents, Struct.get('uri')),
                    Array.map([...note.contents, ...item.contents], (content) => ('text' in content ? content.text : undefined)),
                    items.completion.values.slice(0, 2).toSorted(),
                    people.completion.values[0],
                    greeted.messages.some(({ content }) => 'text' in content && content.text === 'hello there'),
                ],
                [{ handled: true, result: { echoed: 'hi' } }, ['upstream://note'], ['upstream note', 'item apple'], ['apple', 'apricot'], 'world', true],
            );
        }, Effect.scoped),
    );

    it.live(
        'lists no tool its filter denies and refuses a call to it',
        Effect.fnUntraced(function* () {
            const { client } = yield* proxied(protocol);
            const refused = yield* Effect.result(client.callTool({ name: 'secret', arguments: {} }));
            assert.deepStrictEqual([(yield* names(client)).includes('secret'), Result.match(refused, { onFailure: () => true, onSuccess: ({ isError }) => isError === true })], [false, true]);
        }, Effect.scoped),
    );

    it.live(
        'answers a tool sharing a name with a tool of the server from the server, never the upstream',
        Effect.fnUntraced(function* () {
            const { client } = yield* proxied(protocol);
            const { content } = yield* client.callTool({ name: 'test_simple_text', arguments: {} });
            assert.isTrue(content.some((block) => 'text' in block && block.text.includes(TEXT)));
        }, Effect.scoped),
    );

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
        'opens a new session and answers the call when the upstream restarted and no longer holds its session',
        Effect.fnUntraced(function* () {
            const { client, restart } = yield* proxied(McpProtocol.v2025_11_25, { poll: Duration.minutes(1) });
            yield* restart;
            const called = echoed(yield* client.callTool({ name: 'echo', arguments: { value: 'after' } }));
            assert.deepStrictEqual(called.result, { echoed: 'after' });
        }, Effect.scoped),
    );
});
