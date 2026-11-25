import { layerAead, layerSecureRandom } from '@effect/platform-node/NodeCrypto';
import { Content, Handle, Link, registerResource, registerResourceTemplate, Server, Upstream } from '@rasm/mcp';
import { Array, Cause, Context, Deferred, Duration, Effect, Latch, Layer, MutableRef, Option, Predicate, Queue, Record, Ref, Result, Schedule, Schema, Stream, String, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { McpProtocol, McpSchema, McpServer, McpTest, Tool, Toolkit } from 'effect/ai';
import { FetchHttpClient, HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { Host } from 'effect/net';

// --- [TYPES] ---------------------------------------------------------------------------

interface Message {
    readonly jsonrpc: '2.0';
    readonly id?: string | number;
    readonly method?: string;
    readonly params?: Record.ReadonlyRecord<string, unknown>;
}
interface Sent {
    readonly method?: string;
    readonly headers?: Record.ReadonlyRecord<string, string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PNG = 'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8DwHwAFBQIAX8jx0gAAAABJRU5ErkJggg==';
const TEXT = 'This is a simple text response for testing.';
const MIXED = { text: 'Multiple content types test:', uri: 'test://mixed-content-resource', json: '{"test":"data","value":123}' } as const;
const STATIC = 'This is the content of the static text resource.';
const EMBEDDED = 'This is an embedded resource content.';
const FAILURE = 'This tool intentionally returns an error for testing';
const STEPS = [0, 50, 100] as const;
const IDS = ['beta', 'opal', 'alpha'] as const;
const STATE = 'asked once';
const ASKED = 'a dialog asks';
const RAISED = { owned: 'raised during the claim', judged: 'judged after it' } as const;
const PATH = '/mcp';
const UPSTREAM = new URL(PATH, 'http://upstream.test');
const ENDPOINT = 'http://localhost/';
const MANIFEST = { name: 'test', version: '1.2.3', description: 'Conformance fixture' } as const;
const INSTRUCTIONS = { stated: 'Specifiers come from results', shown: 'Simple text answers at once', hidden: 'Hidden tools stay unmentioned' } as const;
const CLIENT = { name: 'conformance-client', version: '1.0.0' } as const;
const SESSION = 'Mcp-Session-Id';
const SETTINGS = { retention: Duration.minutes(1), limit: 64, budget: 8000 } as const;
const SETTLED = Duration.seconds(5);
const PEER = Link.make({ application: 'Upstream', plugin: 'mcp', root: Option.none() });
const ROUTED: Record.ReadonlyRecord<string, string> = { 'resources/read': 'uri', 'tools/call': 'name', 'prompts/get': 'name' };

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);
const Meta = Schema.Struct({ _meta: Schema.Record(Schema.String, Schema.Unknown) });
const Weather = Schema.Struct({ temperature: Schema.Finite, conditions: Schema.String });
const Attachment = Schema.Struct({ uri: Schema.String, mimeType: Schema.OptionFromOptionalKey(Schema.String), data: Schema.Uint8Array });
const Templated = Schema.Struct({ id: Schema.String, templateTest: Schema.Literal(true), data: Schema.String, version: Schema.Int });
const Valued = Schema.Struct({ value: Schema.String });
const Echoing = Schema.Struct({ echoed: Schema.String });
const Lined = Schema.Struct({ type: Schema.Literal('text'), text: Schema.String });
const Block = Schema.Struct({ type: Schema.String, text: Schema.optionalKey(Schema.String) });
const Called = Schema.Struct({ result: Schema.Struct({ content: Schema.Array(Block), isError: Schema.optionalKey(Schema.Boolean), structuredContent: Schema.optionalKey(Schema.Unknown) }) });
const Tools = Toolkit.make(
    Tool.make('echo', { description: 'Echoes its value', parameters: Valued, success: Echoing }),
    Tool.make('secret', { description: 'Refused by the filter', success: Schema.String }),
    Tool.make('test_simple_text', { description: 'Shares a name with a tool of the server', success: Schema.String }),
);

// --- [SERVICES] ------------------------------------------------------------------------

const Claimed = Context.Reference<boolean>('@rasm/test-mcp/fixtures/Claimed', { defaultValue: () => false });

class Probe extends Context.Service<Probe>()('@rasm/test-mcp/fixtures/Probe', {
    make: Effect.gen(function* () {
        const [calls, entered, interrupted, release, watched, blocked, versions, shared, owned, judged] = yield* Effect.all([
            Ref.make<readonly string[]>([]),
            Queue.unbounded<string>(),
            Queue.unbounded<string>(),
            Latch.make(),
            SubscriptionRef.make('open'),
            SubscriptionRef.make(false),
            SubscriptionRef.make<Record.ReadonlyRecord<string, number>>({}),
            Queue.unbounded<string>(),
            Queue.unbounded<string>(),
            Deferred.make<void>(),
        ]);
        return { calls, entered, interrupted, release, watched, blocked, versions, shared, owned, judged };
    }),
}) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const registered = Effect.fnUntraced(function* ({ calls, entered, interrupted, release, watched, blocked, versions, shared, owned, judged }: Probe['Service']) {
    const called = <A, E, R>(name: string, answer: Effect.Effect<A, E, R>) => Ref.update(calls, (held) => [...held, name]).pipe(Effect.andThen(Queue.offer(entered, name)), Effect.andThen(answer));
    const gated = (name: string) =>
        called(
            name,
            release.await.pipe(
                Effect.as('released'),
                Effect.onInterrupt(() => Queue.offer(interrupted, name)),
            ),
        );
    yield* Server.registerTool(Tool.make('test_simple_text', { description: 'Returns simple text', success: Schema.String }).annotate(Tool.Readonly, true).annotate(Server.Instructions, [INSTRUCTIONS.shown]), () => called('test_simple_text', Effect.succeed(TEXT)));
    yield* Server.registerTool(Tool.make('test_value', { description: 'Returns the value it is given', parameters: Valued, success: Schema.String }).annotate(Tool.Readonly, true), ({ value }) => called('test_value', Effect.succeed(value)));
    yield* Server.registerTool(Tool.make('test_image_content', { description: 'Returns an image', success: Content.image('image/png') }).annotate(Tool.Readonly, true), () => called('test_image_content', Effect.succeed(Uint8Array.fromBase64(PNG))));
    yield* Server.registerTool(Tool.make('test_embedded_resource', { description: 'Returns an embedded resource', success: Content.embeddedResource(Attachment) }).annotate(Tool.Readonly, true), () =>
        called('test_embedded_resource', Effect.succeed({ uri: 'test://embedded-resource', mimeType: Option.some('text/plain'), data: new TextEncoder().encode(EMBEDDED) })),
    );
    yield* Server.registerTool(
        Tool.make('test_multiple_content_types', {
            description: 'Returns text, an image, and an embedded resource',
            success: Content(Schema.Uint8Array, MIXED.text, (data) => [[McpSchema.ImageContent.make({ data, mimeType: 'image/png' }), McpSchema.EmbeddedResource.make({ resource: McpSchema.TextResourceContents.make({ uri: MIXED.uri, mimeType: 'application/json', text: MIXED.json }) })], MIXED.text]),
        }).annotate(Tool.Readonly, true),
        () => called('test_multiple_content_types', Effect.succeed(Uint8Array.fromBase64(PNG))),
    );
    yield* Server.registerTool(Tool.make('test_structured', { description: 'Returns a structured result', success: Weather }).annotate(Tool.Readonly, true), () => called('test_structured', Effect.succeed({ temperature: 22.5, conditions: 'Partly cloudy' })));
    yield* Server.registerTool(Tool.make('test_error_handling', { description: 'Fails on every call', success: Schema.String }).annotate(Tool.Readonly, true), () => called('test_error_handling', Effect.fail(new Cause.IllegalArgumentError(FAILURE))));
    yield* Server.registerTool(Tool.make('test_hidden', { description: 'Hidden from every caller', success: Schema.String }).annotate(Tool.Readonly, true).annotate(Server.Visibility, Effect.succeed(false)).annotate(Server.Instructions, [INSTRUCTIONS.hidden]), () => called('test_hidden', Effect.succeed(TEXT)));
    yield* Server.registerTool(Tool.make('test_tool_with_progress', { description: 'Reports progress in three steps', success: Schema.String }).annotate(Tool.Readonly, true), () =>
        called(
            'test_tool_with_progress',
            Stream.fromIterable(STEPS).pipe(
                Server.progress((progress) => ({ progress, total: 100 })),
                Stream.runDrain,
                Effect.as('done'),
            ),
        ),
    );
    yield* Server.registerTool(
        Tool.make('test_input', { description: 'Asks for client input, then answers with the state it sealed', success: Schema.String }).annotate(Tool.Readonly, true),
        Effect.fnUntraced(function* () {
            const [{ requestState }, { input }] = yield* Effect.all([McpSchema.McpRequestContext, Server.ToolCall]);
            return yield* called('test_input', UndefinedOr.match(requestState, { onUndefined: () => input(new McpSchema.InputRequired({ requestState: STATE })), onDefined: Effect.succeed }));
        }),
    );
    yield* Server.registerTool(Tool.make('gated_write', { description: 'Waits for release, changes state', success: Schema.String }), () => gated('gated_write'));
    yield* Server.registerTool(
        Tool.make('gated_early', { description: 'Waits for release, returning early while blocked', success: Schema.String }).annotate(
            Server.EarlyReturn,
            Stream.filterMap(SubscriptionRef.changes(blocked), (held) => (held ? Result.succeed(Effect.succeed(ASKED)) : Result.failVoid)),
        ),
        () => gated('gated_early'),
    );
    yield* Server.registerTool(Tool.make('test_claim', { description: 'Raises two events of the owned source, and claims the source for its handle once the server judged the second, which it does after placing the first for every handle, and no dialog blocks', success: Schema.String }), () =>
        called(
            'test_claim',
            Queue.offerAll(owned, [RAISED.owned, RAISED.judged]).pipe(Effect.andThen(Deferred.await(judged)), Effect.andThen(Stream.runHead(Stream.filter(SubscriptionRef.changes(blocked), (held) => !held))), Effect.andThen(Effect.flatMap(Handle, (handle) => handle.set(Claimed, true))), Effect.as('claimed')),
        ),
    );
    yield* Server.use((server) =>
        Effect.all([server.events({ stream: Stream.fromQueue(shared), render: Effect.succeed }), server.events({ stream: Stream.fromQueue(owned), render: Effect.succeed, owns: (event, state) => (event === RAISED.judged ? Deferred.doneUnsafe(judged, Effect.void) && false : Context.get(state, Claimed)) })], {
            discard: true,
        }),
    );
    yield* Server.registerTool(Tool.make('test_await_running', { description: 'Awaits the calls of its handle that returned early', success: Schema.String }), () => called('test_await_running', Effect.as(Server.ToolCall.use(Struct.get('awaitRunning')), 'settled')));
    yield* registerResource({ name: 'static-text', description: 'A static text resource for testing', mimeType: 'text/plain', schema: Schema.String, changes: Stream.empty, content: Effect.succeed(STATIC) });
    yield* registerResource({ name: 'static-binary', description: 'A static binary resource for testing', mimeType: 'image/png', schema: Schema.Uint8Array, changes: Stream.empty, content: Effect.succeed(Uint8Array.fromBase64(PNG)) });
    yield* registerResource({ name: 'watched-resource', description: 'A resource whose value changes', mimeType: 'text/plain', schema: Schema.String, changes: SubscriptionRef.changes(watched), content: SubscriptionRef.get(watched) });
    yield* registerResourceTemplate`template/${McpSchema.param('id', Schema.String)}/data`({
        name: 'template',
        description: 'A resource template with one variable',
        schema: Templated,
        changes: SubscriptionRef.changes(versions),
        completion: { id: () => Effect.succeed([...IDS]) },
        content: (_, id) => Effect.map(SubscriptionRef.get(versions), (held) => ({ id, templateTest: true as const, data: `Data for ID: ${id}`, version: held[id] ?? 0 })),
    });
});

const app = (probe: Probe['Service']) => Layer.effectDiscard(registered(probe));

const registrations = Layer.mergeAll(
    McpServer.toolkit(Tools).pipe(Layer.provide(Tools.toLayer({ echo: ({ value }) => Effect.succeed({ echoed: value }), secret: () => Effect.succeed('secret'), test_simple_text: () => Effect.succeed('upstream') }))),
    Layer.effectDiscard(McpServer.registerResource({ uri: 'upstream://note', name: 'note', mimeType: 'text/plain', content: Effect.succeed('upstream note') })),
    Layer.effectDiscard(McpServer.registerResource`upstream://items/${McpSchema.param('id', Schema.String)}`({ name: 'item', mimeType: 'text/plain', completion: { id: () => Effect.succeed(['apple', 'apricot', 'banana']) }, content: (_, id) => Effect.succeed(`item ${id}`) })),
    Layer.effectDiscard(
        McpServer.McpServer.use((server) =>
            server.addResourceTemplate({
                template: McpSchema.ResourceTemplate.make({ uriTemplate: 'upstream://files/{+path}', name: 'file', mimeType: 'text/plain' }),
                annotations: Context.empty(),
                routerPath: 'upstream:://files/*',
                completions: { path: () => Effect.succeed(McpSchema.CompleteResult.make({ completion: { values: ['docs/guide.md'] } })) },
                handle: (uri) => Effect.succeed(McpSchema.ReadResourceResult.make({ contents: [McpSchema.TextResourceContents.make({ uri, mimeType: 'text/plain', text: `file ${uri}` })] })),
            }),
        ),
    ),
    Layer.effectDiscard(McpServer.registerPrompt({ name: 'greet', parameters: { who: Schema.String }, completion: { who: () => Effect.succeed(['world', 'wide']) }, content: ({ who }) => Effect.succeed(`hello ${who}`) })),
);

const upstream = (protocol: McpProtocol.ProtocolAdapter, token: MutableRef.MutableRef<string>, rejected: MutableRef.MutableRef<number>) => {
    const guarded = HttpRouter.middleware((next) => Effect.flatMap(HttpServerRequest.HttpServerRequest, ({ headers }) => (headers['authorization'] === `Bearer ${MutableRef.get(token)}` ? next : Effect.sync(() => MutableRef.increment(rejected)).pipe(Effect.as(HttpServerResponse.empty({ status: 401 }))))), {
        global: true,
    });
    return Effect.acquireRelease(
        Effect.sync(() => HttpRouter.toWebHandler(Layer.merge(McpServer.layerHttp({ name: 'upstream', version: '1.0.0', path: PATH, protocols: [protocol], registrations }).pipe(Layer.provideMerge(McpServer.McpServer.layer)), guarded), { disableLogger: true })),
        ({ dispose }) => Effect.promise(dispose),
    );
};

const hosted = Effect.fnUntraced(function* (bound: string) {
    const [probe, name, address] = yield* Effect.all([Probe.make, Server.scheme(MANIFEST.name), Effect.fromResult(Host.hostPortFromString(bound))]);
    return { probe, layer: Server.layerHttp({ ...MANIFEST, name }, { ...SETTINGS, address, hosts: ['mcp.example'], instructions: [INSTRUCTIONS.stated] }, app(probe)).pipe(Layer.provide([layerAead, layerSecureRandom])) };
});
const messages = Effect.fnUntraced(function* (response: Response) {
    const text = yield* Effect.promise(() => response.text());
    const field = 'data: ';
    const records = response.headers.get('content-type')?.includes('text/event-stream') === true ? text.split('\n').filter(String.startsWith(field)).map(String.slice(field.length)) : [text].filter(String.isNonEmpty);
    return yield* Effect.forEach(records, (record) => Schema.decodeEffect(Json)(record));
});

const opened = Effect.fnUntraced(function* (protocol: McpProtocol.ProtocolAdapter) {
    const { probe, layer } = yield* hosted('192.0.2.10:7000');
    const { handler, dispose } = HttpRouter.toWebHandler(layer, { disableLogger: true });
    yield* Effect.addFinalizer(() => Effect.promise(dispose));
    yield* Effect.addFinalizer(() => probe.release.open);
    const stateless = Predicate.isTagged(protocol.runtime, 'Stateless');
    const post = (body: unknown, { method = 'POST', headers = {} }: Sent = {}) =>
        Effect.flatMap(Effect.orDie(Schema.encodeUnknownEffect(Json)(body)), (text) => Effect.promise(() => handler(new Request(ENDPOINT, { method, headers: { accept: 'application/json, text/event-stream', 'content-type': 'application/json', ...headers }, body: text }))));
    const enveloped = ({ method, params = {}, ...rest }: Message & { readonly method: string }) => {
        const meta = Option.getOrUndefined(Option.map(Schema.decodeUnknownOption(Meta)(params), ({ _meta }) => _meta));
        const named = Record.get(ROUTED, method).pipe(
            Option.flatMap((key) => Record.get(params, key)),
            Option.filter(Predicate.isString),
            Option.map((target) => ({ 'Mcp-Name': target })),
        );
        return {
            body: { ...rest, method, params: { ...params, _meta: { ...meta, 'io.modelcontextprotocol/protocolVersion': protocol.protocolVersion, 'io.modelcontextprotocol/clientCapabilities': {}, 'io.modelcontextprotocol/clientInfo': CLIENT } } },
            headers: { 'MCP-Protocol-Version': protocol.protocolVersion, 'Mcp-Method': method, ...Option.getOrUndefined(named) },
        };
    };
    const send = (held: Record.ReadonlyRecord<string, string>, { method, ...rest }: Message) => {
        const { body, headers } = stateless && method !== undefined ? enveloped({ ...rest, method }) : { body: { ...rest, ...(method === undefined ? {} : { method }) }, headers: held };
        return post(body, { headers });
    };
    const initialize = post({ jsonrpc: '2.0', id: 1, method: 'initialize', params: { protocolVersion: protocol.protocolVersion, capabilities: {}, clientInfo: CLIENT } });
    const handshake = Effect.gen(function* () {
        const answered = yield* initialize;
        const held: Record.ReadonlyRecord<string, string> = { [SESSION]: yield* Effect.fromNullishOr(answered.headers.get(SESSION)), 'Mcp-Protocol-Version': protocol.protocolVersion };
        yield* post({ jsonrpc: '2.0', method: 'notifications/initialized' }, { headers: held });
        return held;
    });
    const held = yield* stateless ? Effect.succeed<Record.ReadonlyRecord<string, string>>({}) : handshake;
    const call = (method: string, params: Record.ReadonlyRecord<string, unknown> = {}, id: string | number = 'request') => Effect.flatMap(send(held, { jsonrpc: '2.0', id, method, params }), messages);
    const answer = (method: string, params?: Record.ReadonlyRecord<string, unknown>, id?: string | number) => Effect.flatMap(call(method, params, id), (all) => Effect.fromNullishOr(all.at(-1)));
    const tool = (name: string, given: Record.ReadonlyRecord<string, unknown> = {}, extra: Record.ReadonlyRecord<string, unknown> = {}) => answer('tools/call', { name, arguments: given, ...extra });
    return { probe, post, messages, initialize, send: (message: Message) => send(held, message), call, answer, tool, called: (name: string, given?: Record.ReadonlyRecord<string, unknown>) => Effect.map(Effect.flatMap(tool(name, given), Schema.decodeUnknownEffect(Called)), Struct.get('result')) };
});

const names = (client: McpTest.Client) => Effect.map(client.listTools(), ({ tools }) => Array.map(tools, Struct.get('name')));
const settled = <A, E>(effect: Effect.Effect<A, E>, done: (value: A) => boolean) => effect.pipe(Effect.repeat({ schedule: Schedule.spaced('20 millis'), until: done }), Effect.timeout(SETTLED));
const lines = ({ content }: { readonly content: readonly unknown[] }) => Array.head(content).pipe(Option.flatMap(Schema.decodeUnknownOption(Lined)), Option.match({ onNone: () => [], onSome: ({ text }) => text.split('\n') }));

const connected = Effect.fnUntraced(function* <E, R>(beside: Layer.Layer<never, E, R>) {
    const [probe, scheme] = yield* Effect.all([Probe.make, Server.scheme(MANIFEST.name)]);
    const client = yield* McpTest.makeClient(Layer.provide(Layer.mergeAll(app(probe), beside), Server.layer({ ...SETTINGS, scheme }).pipe(Layer.provide([layerAead, layerSecureRandom]))), { protocols: [McpProtocol.v2025_11_25] });
    yield* Effect.addFinalizer(() => probe.release.open);
    return { probe, client };
});

const proxied = Effect.fnUntraced(function* (
    protocol: McpProtocol.ProtocolAdapter,
    { poll = Duration.millis(50), presence = Stream.succeed(true), filters = { tools: { deny: ['secret'] } } }: { readonly poll?: Duration.Duration; readonly presence?: Stream.Stream<boolean>; readonly filters?: Parameters<typeof Upstream.fromHttp>[0]['filters'] } = {},
) {
    const [token, rejected] = [MutableRef.make('first'), MutableRef.make(0)];
    const handler = MutableRef.make((yield* upstream(protocol, token, rejected)).handler);
    const fetch: typeof globalThis.fetch = (input, init) => MutableRef.get(handler)(new Request(input, init));
    const proxy = Layer.effectDiscard(Upstream.serve([Upstream.fromHttp({ peer: PEER, enabled: true, endpoint: Effect.sync(() => ({ url: UPSTREAM, headers: { authorization: `Bearer ${MutableRef.get(token)}` } })), filters, presence, poll })], { route: (peers) => Effect.succeed(Array.headNonEmpty(peers)) })).pipe(
        Layer.provide([FetchHttpClient.layer, Layer.succeed(FetchHttpClient.Fetch, fetch), layerSecureRandom]),
    );
    const { client } = yield* connected(proxy);
    yield* settled(client.listResources(), ({ resources }) => resources.some(({ uri }) => uri === 'upstream://note'));
    return { client, token, rejected, restart: Effect.map(upstream(protocol, token, rejected), ({ handler: next }) => MutableRef.set(handler, next)) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { ASKED, Called, connected, hosted, INSTRUCTIONS, lines, names, opened, PEER, proxied, RAISED, SETTINGS, SETTLED, settled, STATE, TEXT };
