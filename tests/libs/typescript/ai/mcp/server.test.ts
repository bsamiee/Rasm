import { layerAead, layerSecureRandom } from '@effect/platform-node/NodeCrypto';
import { assert, describe, it } from '@effect/vitest';
import { Link, Server, Upstream } from '@rasm/mcp';
import { Array, Duration, Effect, Function, HashSet, Layer, MutableRef, Number, Option, Queue, Ref, Result, Schedule, Schema, Stream, Struct, SubscriptionRef } from 'effect';
import { McpProtocol, McpSchema, McpServer, McpTest, Tool, Toolkit } from 'effect/ai';
import { FetchHttpClient, HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { RpcTest } from 'effect/rpc';
import { MANIFEST, SETTINGS, STATEFUL } from '#/conformance';
import { app, Probe, TEXT } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PATH = '/mcp';
const UPSTREAM = new URL(PATH, 'http://upstream.test');
const PEER = Link.make({ application: 'Upstream', extension: 'mcp', root: Option.none() });
const LINKED = Link.make({ application: 'Host', extension: 'ext', root: Option.none() });
const JOINED = Link.make({ application: 'Host', extension: 'joined', root: Option.none() });
const NOTICE = { name: 'documentOpened', target: Option.some('/document[1]'), detail: Option.none() };
const REVISIONS = [McpProtocol.v2026_07_28, McpProtocol.v2025_11_25, McpProtocol.v2025_06_18, McpProtocol.v2025_03_26] as const;
const CHANGED = ['notifications/tools/list_changed', 'notifications/resources/list_changed', 'notifications/prompts/list_changed'] as const;
const ITEMS = ['apple', 'apricot', 'banana'] as const;
const SETTLED = Duration.seconds(5);

// --- [MODELS] --------------------------------------------------------------------------

const Echoing = Schema.Struct({ echoed: Schema.String });
const Handled = Schema.Struct({ handle: Schema.String });
const Trailed = Schema.Struct({ handle: Schema.String, events: Schema.Array(Schema.Struct({ sequence: Schema.Int, text: Schema.String })) });
const Extension = Toolkit.make(Tool.make('unique', { description: 'Tool of the extension alone', success: Schema.String }));
const Joined = Toolkit.make(Tool.make('late', { description: 'Tool of the extension joining later', success: Schema.String }));
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
                    endpoint: Effect.sync(() => ({ url: UPSTREAM, headers: { authorization: `Bearer ${MutableRef.get(token)}` } })),
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
const linked = Effect.fnUntraced(function* (silent: readonly Stream.Stream<readonly Upstream[]>[] = []) {
    const [probe, scheme, notices, late, own, lists, subscribed, seen] = yield* Effect.all([
        Probe.make,
        Schema.decodeEffect(Server.Scheme)(MANIFEST.name),
        Queue.unbounded<typeof Link.Notice.Type>(),
        Queue.unbounded<typeof Link.Notice.Type>(),
        Queue.unbounded<string>(),
        Queue.unbounded<Link[]>(),
        Ref.make(0),
        Ref.make<readonly (typeof Trailed.Type.events)[number][]>([]),
    ]);
    const counted = Stream.unwrap(Effect.as(Ref.update(subscribed, Number.increment), Stream.fromQueue(notices)));
    const [extension, joined] = yield* Effect.all([
        RpcTest.makeClient(Link.Rpcs, { flatten: true }).pipe(Effect.provide(Link.serve(Extension, { notices: counted }).pipe(Layer.provide(Extension.toLayer({ unique: () => Effect.succeed('own') }))))),
        RpcTest.makeClient(Link.Rpcs, { flatten: true }).pipe(Effect.provide(Link.serve(Joined, { notices: Stream.fromQueue(late) }).pipe(Layer.provide(Joined.toLayer({ late: () => Effect.succeed('joined') }))))),
    ]);
    const links = { changes: Stream.concat(Stream.succeed([LINKED]), Stream.fromQueue(lists)), call: (holds: (peer: Link) => boolean) => Effect.succeed(holds(JOINED) ? joined : extension) };
    const proxy = Layer.effectDiscard(
        Effect.andThen(
            Server.use((server) => server.events({ text: Function.identity, stream: Stream.fromQueue(own) })),
            Upstream.serve(Array.append(silent, Upstream.linked(links)), { route: (peers) => Effect.succeed(Array.headNonEmpty(peers)) }),
        ),
    );
    const client = yield* McpTest.makeClient(Layer.provide(Layer.mergeAll(app(probe), proxy), Server.layer({ ...SETTINGS, scheme }).pipe(Layer.provide([layerAead, layerSecureRandom]))), { protocols: [McpProtocol.v2025_11_25] });
    yield* Effect.addFinalizer(() => probe.release.open);
    yield* settled(names(client), Array.contains('unique'));
    const { handle } = yield* Effect.flatMap(client.callTool({ name: 'test_simple_text', arguments: {} }), ({ structuredContent }) => Schema.decodeUnknownEffect(Trailed)(structuredContent));
    const read = client.callTool({ name: 'test_simple_text', arguments: { handle } }).pipe(
        Effect.flatMap(({ structuredContent }) => Schema.decodeUnknownEffect(Trailed)(structuredContent)),
        Effect.flatMap(({ events }) => Ref.updateAndGet(seen, Array.appendAll(events))),
    );
    return { notices, late, own, lists, subscribed: Ref.get(subscribed), offers: (name: string) => settled(names(client), Array.contains(name)), events: (count: number) => settled(read, (events) => events.length >= count) };
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

describe('linked extension', () => {
    it.live(
        'reports a notice the extension sends as an event of the next result, naming the extension, its host, the host event, and its target',
        Effect.fnUntraced(function* () {
            const { notices, events } = yield* linked();
            yield* Queue.offer(notices, NOTICE);
            assert.deepStrictEqual(yield* events(1), [{ sequence: 1, text: 'ext in Host documentOpened /document[1]' }]);
        }, Effect.scoped),
    );

    it.live(
        'numbers a notice after an event of another source in one sequence',
        Effect.fnUntraced(function* () {
            const { notices, own, events } = yield* linked();
            yield* Queue.offer(own, 'own event');
            yield* events(1);
            yield* Queue.offer(notices, NOTICE);
            assert.deepStrictEqual(Array.map(yield* events(2), Struct.pick(['sequence', 'text'])), [
                { sequence: 1, text: 'own event' },
                { sequence: 2, text: 'ext in Host documentOpened /document[1]' },
            ]);
        }, Effect.scoped),
    );

    it.live(
        'keeps the one notice subscription of an extension, and its later notices, when another extension joins the list',
        Effect.fnUntraced(function* () {
            const { notices, late, lists, subscribed, offers, events } = yield* linked();
            yield* Queue.offer(lists, [LINKED, JOINED]);
            yield* offers('late');
            yield* Queue.offer(late, NOTICE);
            yield* events(1);
            yield* Queue.offer(notices, NOTICE);
            assert.deepStrictEqual([Array.map(yield* events(2), Struct.get('text')), yield* subscribed], [['joined in Host documentOpened /document[1]', 'ext in Host documentOpened /document[1]'], 1]);
        }, Effect.scoped),
    );

    it.live(
        'lists the tools of an extension while another source has listed nothing yet',
        Effect.fnUntraced(function* () {
            const { offers } = yield* linked([Stream.never]);
            assert.isTrue(Array.contains(yield* offers('unique'), 'unique'));
        }, Effect.scoped),
    );
});

describe('MCP upstream', () => {
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
