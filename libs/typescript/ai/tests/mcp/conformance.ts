import { Server } from '@rasm/mcp';
import { Deferred, Duration, Effect, Option, Predicate, Record, Schema, String } from 'effect';
import { McpProtocol } from 'effect/ai';
import { HttpRouter } from 'effect/http';
import { Host } from 'effect/net';
import { app, Probe } from '#/fixtures';

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
const ROUTED: Record.ReadonlyRecord<string, string> = { 'resources/read': 'uri', 'tools/call': 'name', 'prompts/get': 'name' };

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);
const Meta = Schema.Struct({ _meta: Schema.Record(Schema.String, Schema.Unknown) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const hosted = Effect.gen(function* () {
    const [probe, name, address] = yield* Effect.all([Probe.make, Schema.decodeEffect(Server.Scheme)(MANIFEST.name), Effect.fromResult(Host.hostPortFromString(BOUND))]);
    return { probe, layer: Server.layerHttp({ ...MANIFEST, name }, { address, hosts: [ALLOWED], retention: Duration.minutes(1), instructions: INSTRUCTIONS }, app(probe)) };
});

const opened = Effect.fnUntraced(function* (protocol: (typeof PROTOCOLS)[number]) {
    const { probe, layer } = yield* hosted;
    const { handler, dispose } = HttpRouter.toWebHandler(layer, { disableLogger: true });
    yield* Effect.addFinalizer(() => Effect.promise(dispose));
    yield* Effect.addFinalizer(() => Deferred.succeed(probe.release, undefined));
    const stateless = Predicate.isTagged(protocol.runtime, 'Stateless');
    const raw = (body: Option.Option<string>, { method = 'POST', headers = {} }: Sent = {}) =>
        Effect.promise(() => handler(new Request(ENDPOINT, { method, headers: { accept: 'application/json, text/event-stream', 'content-type': 'application/json', ...headers }, ...Option.getOrUndefined(Option.map(body, (text) => ({ body: text }))) })));
    const post = (body: unknown, sent?: Sent) => Effect.flatMap(Effect.orDie(Schema.encodeUnknownEffect(Json)(body)), (text) => raw(Option.some(text), sent));
    const messages = (response: Response) =>
        Effect.flatMap(
            Effect.promise(() => response.text()),
            (text) => {
                const field = 'data: ';
                const records = response.headers.get('content-type')?.includes('text/event-stream') === true ? text.split('\n').filter(String.startsWith(field)).map(String.slice(field.length)) : [text].filter(String.isNonEmpty);
                return Effect.forEach(records, (record) => Schema.decodeEffect(Json)(record));
            },
        );
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { ALLOWED, BOUND, CLIENT, ENDPOINT, hosted, INSTRUCTIONS, MANIFEST, opened, PROTOCOLS, SESSION, STATEFUL, VERSION };
