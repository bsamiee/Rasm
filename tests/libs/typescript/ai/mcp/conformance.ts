import { layerAead, layerSecureRandom } from '@effect/platform-node/NodeCrypto';
import { Link, Server, Upstream } from '@rasm/mcp';
import { Array, Duration, Effect, Layer, MutableRef, Option, Predicate, Record, Schedule, Schema, Stream, String, Struct } from 'effect';
import { McpProtocol, type McpSchema, McpTest } from 'effect/ai';
import { FetchHttpClient, HttpRouter } from 'effect/http';
import { Host } from 'effect/net';
import { app, Echoing, Probe, UPSTREAM, upstream } from '#/fixtures.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Message {
    readonly jsonrpc: '2.0';
    readonly id?: string | number;
    readonly method?: string;
    readonly params?: Record.ReadonlyRecord<string, unknown>;
    readonly result?: unknown;
}
interface Sent {
    readonly method?: string;
    readonly headers?: Record.ReadonlyRecord<string, string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const ENDPOINT = 'http://localhost/';
const MANIFEST = { name: 'test', version: '1.2.3', description: 'Conformance fixture' } as const;
const INSTRUCTIONS = ['Specifiers come from results'] as const;
const ALLOWED = 'mcp.example';
const BOUND = '192.0.2.10:7000';
const CLIENT = { name: 'conformance-client', version: '1.0.0' } as const;
const SESSION = 'Mcp-Session-Id';
const VERSION = 'Mcp-Protocol-Version';
const STATEFUL = [McpProtocol.v2024_11_05, McpProtocol.v2025_03_26, McpProtocol.v2025_06_18, McpProtocol.v2025_11_25] as const;
const PROTOCOLS = [...STATEFUL, McpProtocol.v2026_07_28] as const;
const REVISIONS = [McpProtocol.v2026_07_28, McpProtocol.v2025_11_25, McpProtocol.v2025_06_18, McpProtocol.v2025_03_26] as const;
const SETTINGS = { retention: Duration.minutes(1), limit: 64 } as const;
const SETTLED = Duration.seconds(5);
const PEER = Link.make({ application: 'Upstream', extension: 'mcp', root: Option.none() });
const ROUTED: Record.ReadonlyRecord<string, string> = { 'resources/read': 'uri', 'tools/call': 'name', 'prompts/get': 'name' };

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);
const Meta = Schema.Struct({ _meta: Schema.Record(Schema.String, Schema.Unknown) });
const Handled = Schema.Struct({ handle: Schema.String });
const Echoed = Schema.fromJsonString(Echoing);
const Block = Schema.Struct({
    type: Schema.String,
    text: Schema.optionalKey(Schema.String),
    data: Schema.optionalKey(Schema.String),
    mimeType: Schema.optionalKey(Schema.String),
    resource: Schema.optionalKey(Schema.Struct({ uri: Schema.String, mimeType: Schema.optionalKey(Schema.String), blob: Schema.optionalKey(Schema.String), text: Schema.optionalKey(Schema.String) })),
});
const Called = Schema.Struct({ result: Schema.Struct({ content: Schema.Array(Block), isError: Schema.optionalKey(Schema.Boolean), structuredContent: Schema.optionalKey(Schema.Unknown) }) });
const Outcome = Schema.Struct({ error: Schema.optionalKey(Schema.Struct({ code: Schema.Finite, message: Schema.String })), result: Schema.optionalKey(Schema.Struct({ isError: Schema.optionalKey(Schema.Boolean) })) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const hosted = Effect.fnUntraced(function* (bound: string = BOUND) {
    const [probe, name, address] = yield* Effect.all([Probe.make, Schema.decodeEffect(Server.Scheme)(MANIFEST.name), Effect.fromResult(Host.hostPortFromString(bound))]);
    return { probe, layer: Server.layerHttp({ ...MANIFEST, name }, { ...SETTINGS, address, hosts: [ALLOWED], instructions: INSTRUCTIONS }, app(probe)).pipe(Layer.provide([layerAead, layerSecureRandom])) };
});
const messages = (response: Response) =>
    Effect.flatMap(
        Effect.promise(() => response.text()),
        (text) => {
            const field = 'data: ';
            const records = response.headers.get('content-type')?.includes('text/event-stream') === true ? text.split('\n').filter(String.startsWith(field)).map(String.slice(field.length)) : [text].filter(String.isNonEmpty);
            return Effect.forEach(records, (record) => Schema.decodeEffect(Json)(record));
        },
    );

const opened = Effect.fnUntraced(function* (protocol: (typeof PROTOCOLS)[number]) {
    const { probe, layer } = yield* hosted();
    const { handler, dispose } = HttpRouter.toWebHandler(layer, { disableLogger: true });
    yield* Effect.addFinalizer(() => Effect.promise(dispose));
    yield* Effect.addFinalizer(() => probe.release.open);
    const stateless = Predicate.isTagged(protocol.runtime, 'Stateless');
    const raw = (body: Option.Option<string>, { method = 'POST', headers = {} }: Sent = {}) =>
        Effect.promise(() => handler(new Request(ENDPOINT, { method, headers: { accept: 'application/json, text/event-stream', 'content-type': 'application/json', ...headers }, ...Option.getOrUndefined(Option.map(body, (text) => ({ body: text }))) })));
    const post = (body: unknown, sent?: Sent) => Effect.flatMap(Effect.orDie(Schema.encodeUnknownEffect(Json)(body)), (text) => raw(Option.some(text), sent));
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
    const send = (held: Record.ReadonlyRecord<string, string>, { method, ...rest }: Message, sent: Sent = {}) => {
        const modern = stateless && method !== undefined ? Option.some(enveloped({ ...rest, method })) : Option.none();
        return Option.match(modern, {
            onNone: () => post({ ...rest, ...(method === undefined ? {} : { method }) }, { ...sent, headers: { ...held, ...sent.headers } }),
            onSome: ({ body, headers }) => post(body, { ...sent, headers: { ...headers, ...sent.headers } }),
        });
    };
    const initialize = (protocolVersion: string = protocol.protocolVersion) => post({ jsonrpc: '2.0', id: 1, method: 'initialize', params: { protocolVersion, capabilities: {}, clientInfo: CLIENT } });
    const handshake = Effect.gen(function* () {
        const answered = yield* initialize();
        const held: Record.ReadonlyRecord<string, string> = { [SESSION]: yield* Effect.fromNullishOr(answered.headers.get(SESSION)), [VERSION]: protocol.protocolVersion };
        yield* post({ jsonrpc: '2.0', method: 'notifications/initialized' }, { headers: held });
        return held;
    });
    const session = stateless ? Effect.succeed<Record.ReadonlyRecord<string, string>>({}) : handshake;
    const call = (held: Record.ReadonlyRecord<string, string>, method: string, params: Record.ReadonlyRecord<string, unknown> = {}, id: string | number = 'request') => Effect.flatMap(send(held, { jsonrpc: '2.0', id, method, params }), messages);
    const answer = (held: Record.ReadonlyRecord<string, string>, method: string, params?: Record.ReadonlyRecord<string, unknown>, id?: string | number) => Effect.flatMap(call(held, method, params, id), (all) => Effect.fromNullishOr(all.at(-1)));
    return { probe, protocol, stateless, handler, raw, post, messages, send, initialize, session, call, answer };
});
const served = Effect.fnUntraced(function* (protocol: (typeof PROTOCOLS)[number]) {
    const test = yield* opened(protocol);
    const held = yield* test.session;
    const tool = (name: string, given: Record.ReadonlyRecord<string, unknown> = {}) => test.answer(held, 'tools/call', { name, arguments: given });
    return { test, held, tool, content: (name: string, given?: Record.ReadonlyRecord<string, unknown>) => Effect.map(Effect.flatMap(tool(name, given), Schema.decodeUnknownEffect(Called)), ({ result }) => result) };
});
const since = (protocol: (typeof PROTOCOLS)[number], first: (typeof PROTOCOLS)[number]) => PROTOCOLS.indexOf(protocol) >= PROTOCOLS.indexOf(first);
const outcome = (message: unknown) => Effect.map(Schema.decodeUnknownEffect(Outcome)(message), ({ error, result }) => ({ code: error?.code, failed: result?.isError === true }));

const names = (client: McpTest.Client) => Effect.map(client.listTools(), ({ tools }) => Array.map(tools, Struct.get('name')));
const settled = <A, E>(effect: Effect.Effect<A, E>, done: (value: A) => boolean) => effect.pipe(Effect.repeat({ schedule: Schedule.spaced('20 millis'), until: done }), Effect.timeout(SETTLED));
const echoed = ({ content, structuredContent }: McpSchema.CallToolResult) => ({ handled: Schema.is(Handled)(structuredContent), result: Option.getOrUndefined(Array.findFirst(content, (block) => ('text' in block ? Schema.decodeOption(Echoed)(block.text) : Option.none()))) });

const connected = Effect.fnUntraced(function* <E, R>(protocol: (typeof PROTOCOLS)[number], beside: Layer.Layer<never, E, R>) {
    const [probe, scheme] = yield* Effect.all([Probe.make, Schema.decodeEffect(Server.Scheme)(MANIFEST.name)]);
    const client = yield* McpTest.makeClient(Layer.provide(Layer.mergeAll(app(probe), beside), Server.layer({ ...SETTINGS, scheme }).pipe(Layer.provide([layerAead, layerSecureRandom]))), { protocols: [protocol] });
    yield* Effect.addFinalizer(() => probe.release.open);
    return { probe, client };
});

const proxied = Effect.fnUntraced(function* (protocol: (typeof REVISIONS)[number], { poll = Duration.millis(50), presence = Stream.succeed(true) }: { readonly poll?: Duration.Duration; readonly presence?: Stream.Stream<boolean> } = {}) {
    const [token, rejected] = [MutableRef.make('first'), MutableRef.make(0)];
    const handler = MutableRef.make((yield* upstream(protocol, token, rejected)).handler);
    const fetch: typeof globalThis.fetch = (input, init) => MutableRef.get(handler)(new Request(input, init));
    const proxy = Layer.effectDiscard(
        Upstream.serve([Upstream.mcp({ peer: PEER, enabled: true, endpoint: Effect.sync(() => ({ url: UPSTREAM, headers: { authorization: `Bearer ${MutableRef.get(token)}` } })), filters: { tools: { deny: ['secret'] } }, presence, poll })], { route: (peers) => Effect.succeed(Array.headNonEmpty(peers)) }),
    ).pipe(Layer.provide([FetchHttpClient.layer, Layer.succeed(FetchHttpClient.Fetch, fetch), layerSecureRandom]));
    const { client } = yield* connected(McpProtocol.v2025_11_25, proxy);
    yield* settled(names(client), Array.contains('echo'));
    return { client, token, rejected, restart: Effect.map(upstream(protocol, token, rejected), ({ handler: next }) => MutableRef.set(handler, next)) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { ALLOWED, BOUND, Called, CLIENT, connected, echoed, ENDPOINT, hosted, INSTRUCTIONS, MANIFEST, names, opened, outcome, PEER, PROTOCOLS, proxied, REVISIONS, served, SESSION, SETTINGS, SETTLED, settled, since, STATEFUL, VERSION };
