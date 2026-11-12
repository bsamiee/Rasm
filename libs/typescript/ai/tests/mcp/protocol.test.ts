import { assert, describe, it } from '@effect/vitest';
import { Array, Effect, Fiber, Option, Queue, Schema } from 'effect';
import { McpProtocol } from 'effect/ai';
import { CLIENT, INSTRUCTIONS, MANIFEST, opened, PROTOCOLS, SESSION, STATEFUL, VERSION } from '#/conformance';
import { STEPS } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PARAMS = { protocolVersion: STATEFUL[3].protocolVersion, capabilities: {}, clientInfo: CLIENT } as const;
const INITIALIZE = { jsonrpc: '2.0', id: 1, method: 'initialize', params: PARAMS } as const;
const PING = { jsonrpc: '2.0', id: 'ping', method: 'ping' } as const;
const CANCELLED = { jsonrpc: '2.0', method: 'notifications/cancelled', params: { requestId: 'gated' } } as const;
const DELETED = { ended: 404, declined: 405 } as const;
const VISIBLE = /^[\x21-\x7e]+$/u;
const REFUSED = [
    { label: 'an unknown method', body: '{"jsonrpc":"2.0","id":7,"method":"no/such","params":{}}', code: -32_601, id: 7 },
    { label: 'params that break the method schema', body: '{"jsonrpc":"2.0","id":"seven","method":"tools/call","params":{"name":5}}', code: -32_602, id: 'seven' },
    { label: 'a message with neither method nor result', body: '{"jsonrpc":"2.0","id":7}', code: -32_600, id: 7 },
    { label: 'text that is not JSON', body: '{"jsonrpc":', code: -32_700, id: null },
] as const;
const HOSTS = [
    { host: 'attacker.example', served: false },
    { host: 'localhost', served: true },
    { host: '127.0.0.1', served: true },
] as const;
const MEDIA = [
    { headers: { 'content-type': 'text/plain' }, status: 415 },
    { headers: { accept: 'application/json' }, status: 406 },
    { headers: { accept: 'text/event-stream' }, status: 406 },
] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Initialized = Schema.Struct({ result: Schema.Struct({ protocolVersion: Schema.String, instructions: Schema.String, capabilities: Schema.Record(Schema.String, Schema.Unknown) }) });
const Refusal = Schema.Struct({ id: Schema.NullOr(Schema.Union([Schema.String, Schema.Finite])), error: Schema.Struct({ code: Schema.Finite }) });
const Progress = Schema.Struct({ method: Schema.Literal('notifications/progress'), params: Schema.Struct({ progressToken: Schema.Union([Schema.String, Schema.Finite]), progress: Schema.Finite, total: Schema.Finite }) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const refused = (message: unknown) => Effect.map(Schema.decodeUnknownEffect(Refusal)(message), ({ id, error }) => ({ id, code: error.code }));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe.each(STATEFUL)('$protocolVersion lifecycle', (protocol) => {
    it.live(
        'answers initialize with the requested version, the server name and version, and the stated instructions',
        Effect.fnUntraced(function* () {
            const test = yield* opened(protocol);
            const [message] = yield* Effect.flatMap(test.initialize(), test.messages);
            const { result } = yield* Schema.decodeUnknownEffect(Initialized)(message);
            assert.deepNestedInclude(message, { 'result.protocolVersion': protocol.protocolVersion, 'result.serverInfo.name': MANIFEST.name, 'result.serverInfo.version': MANIFEST.version });
            assert.isTrue(INSTRUCTIONS.every((line) => result.instructions.includes(line)));
        }, Effect.scoped),
    );

    it.live(
        'refuses a request other than ping sent before initialize with 400',
        Effect.fnUntraced(function* () {
            const test = yield* opened(protocol);
            const { status } = yield* test.post({ jsonrpc: '2.0', id: 1, method: 'tools/list', params: {} });
            assert.deepStrictEqual({ status }, { status: 400 });
        }, Effect.scoped),
    );

    it.live.each(['protocolVersion', 'capabilities', 'clientInfo'] as const)(
        'refuses initialize params without %s as invalid params and opens no session',
        Effect.fnUntraced(function* (absent) {
            const test = yield* opened(protocol);
            const { [absent]: _, ...params } = PARAMS;
            const response = yield* test.post({ ...INITIALIZE, params });
            const [message] = yield* test.messages(response);
            assert.deepStrictEqual([yield* refused(message), response.headers.get(SESSION)], [{ id: 1, code: -32_602 }, null]);
        }, Effect.scoped),
    );

    it.live(
        'answers a requested version it does not serve with a version it serves',
        Effect.fnUntraced(function* () {
            const test = yield* opened(protocol);
            const [message] = yield* Effect.flatMap(test.initialize('1999-01-01'), test.messages);
            const { result } = yield* Schema.decodeUnknownEffect(Initialized)(message);
            assert.include<string>(
                STATEFUL.map(({ protocolVersion }) => protocolVersion),
                result.protocolVersion,
            );
        }, Effect.scoped),
    );

    it.live(
        'advertises the tools and resources capabilities its registrations use, and completions from the revision that defines that capability',
        Effect.fnUntraced(function* () {
            const test = yield* opened(protocol);
            const [message] = yield* Effect.flatMap(test.initialize(), test.messages);
            assert.containsAllKeys((yield* Schema.decodeUnknownEffect(Initialized)(message)).result.capabilities, protocol === McpProtocol.v2024_11_05 ? ['tools', 'resources'] : ['tools', 'resources', 'completions']);
        }, Effect.scoped),
    );
});

describe.each(STATEFUL)('$protocolVersion transport', (protocol) => {
    it.live(
        'accepts a notification with 202 and no body',
        Effect.fnUntraced(function* () {
            const test = yield* opened(protocol);
            const response = yield* test.send(yield* test.session, CANCELLED);
            assert.deepStrictEqual({ status: response.status, body: yield* Effect.promise(() => response.text()) }, { status: 202, body: '' });
        }, Effect.scoped),
    );

    it.live.each(MEDIA)(
        'answers a POST sent with $headers with $status',
        Effect.fnUntraced(function* ({ headers, status }) {
            const test = yield* opened(protocol);
            assert.strictEqual((yield* test.post(INITIALIZE, { headers })).status, status);
        }, Effect.scoped),
    );

    it.live(
        'issues a session id of visible ASCII, answers a later request without it with 400, and answers an unknown id with 404',
        Effect.fnUntraced(function* () {
            const test = yield* opened(protocol);
            const held = yield* test.session;
            const [missing, unknown] = yield* Effect.all([test.post(PING, { headers: { [VERSION]: protocol.protocolVersion } }), test.post(PING, { headers: { ...held, [SESSION]: 'no-such-session' } })]);
            assert.match(yield* Effect.fromNullishOr(held[SESSION]), VISIBLE);
            assert.deepStrictEqual({ missing: missing.status, unknown: unknown.status }, { missing: 400, unknown: 404 });
        }, Effect.scoped),
    );

    it.live(
        'answers DELETE for a session by ending it, its id answering 404 afterwards, or by 405 with the session still served, and keeps another session live either way',
        Effect.fnUntraced(function* () {
            const test = yield* opened(protocol);
            const [ended, kept] = yield* Effect.all([test.session, test.session]);
            const deleted = yield* test.raw(Option.none(), { method: 'DELETE', headers: ended });
            const [after, other] = yield* Effect.all([test.send(ended, PING), test.send(kept, PING)]);
            assert.deepStrictEqual({ ended: deleted.ok ? after.status : deleted.status, kept: deleted.ok || after.ok, other: other.ok }, { ended: deleted.ok ? DELETED.ended : DELETED.declined, kept: true, other: true });
        }, Effect.scoped),
    );

    it.live(
        'refuses an Origin it does not allow with 403',
        Effect.fnUntraced(function* () {
            const test = yield* opened(protocol);
            const { status } = yield* test.post(INITIALIZE, { headers: { origin: 'https://attacker.example' } });
            assert.deepStrictEqual({ status }, { status: 403 });
        }, Effect.scoped),
    );

    it.live.each(HOSTS)(
        'serves initialize addressed to host $host: $served, a loopback host is served and a host that only resolves there is refused',
        Effect.fnUntraced(function* ({ host, served }) {
            const test = yield* opened(protocol);
            assert.strictEqual((yield* test.post(INITIALIZE, { headers: { host } })).ok, served);
        }, Effect.scoped),
    );

    it.live.each(REFUSED)(
        'answers $label with JSON-RPC error $code under the id of the request',
        Effect.fnUntraced(function* ({ body, code, id }) {
            const test = yield* opened(protocol);
            const [message] = yield* Effect.flatMap(test.raw(Option.some(body), { headers: yield* test.session }), test.messages);
            assert.deepStrictEqual(yield* refused(message), { id, code });
        }, Effect.scoped),
    );
});

describe.each([McpProtocol.v2025_06_18, McpProtocol.v2025_11_25])('$protocolVersion protocol version header', (protocol) => {
    it.live(
        'refuses a version the server does not serve with 400 and uses the negotiated version when the header is absent',
        Effect.fnUntraced(function* () {
            const test = yield* opened(protocol);
            const { [VERSION]: _, ...headers } = yield* test.session;
            const [unserved, absent] = yield* Effect.all([test.post(PING, { headers: { ...headers, [VERSION]: '2099-01-01' } }), test.post(PING, { headers })]);
            assert.deepStrictEqual({ unserved: unserved.status, absent: absent.status }, { unserved: 400, absent: 200 });
        }, Effect.scoped),
    );
});

describe.each(STATEFUL)('$protocolVersion utilities', (protocol) => {
    it.live(
        'answers ping with an empty result under the id of the request',
        Effect.fnUntraced(function* () {
            const test = yield* opened(protocol);
            assert.deepInclude(yield* test.answer(yield* test.session, 'ping', {}, PING.id), { id: PING.id, result: {} });
        }, Effect.scoped),
    );

    it.live.each(['gated_read', 'gated_write'] as const)(
        'stops a call to %s on cancellation and writes no response for it',
        Effect.fnUntraced(function* (name) {
            const test = yield* opened(protocol);
            const held = yield* test.session;
            const pending = yield* Effect.forkChild(test.call(held, 'tools/call', { name, arguments: {} }, CANCELLED.params.requestId));
            const entered = yield* Queue.take(test.probe.entered);
            const cancelled = yield* test.send(held, CANCELLED);
            const interrupted = yield* Effect.timeoutOption(Queue.take(test.probe.interrupted), '2 seconds');
            const written = yield* Effect.timeoutOption(Fiber.join(pending), '2 seconds');
            assert.deepStrictEqual({ entered, cancelled: cancelled.status, interrupted, written }, { entered: name, cancelled: 202, interrupted: Option.some(name), written: Option.some([]) });
        }, Effect.scoped),
    );
});

describe.each(PROTOCOLS)('$protocolVersion progress', (protocol) => {
    it.live(
        'reports progress under the token of the request, increasing, before the result, and reports none for a request without a token',
        Effect.fnUntraced(function* () {
            const test = yield* opened(protocol);
            const held = yield* test.session;
            const tokened = yield* test.call(held, 'tools/call', { name: 'test_tool_with_progress', arguments: {}, _meta: { progressToken: 'progress-1' } }, 'tokened');
            const plain = yield* test.call(held, 'tools/call', { name: 'test_tool_with_progress', arguments: {} }, 'plain');
            const reported = Array.getSomes(tokened.slice(0, -1).map((message) => Schema.decodeUnknownOption(Progress)(message)));
            assert.deepStrictEqual([reported.map(({ params }) => params), tokened.length, plain.length], [STEPS.map((progress) => ({ progressToken: 'progress-1', progress, total: 100 })), STEPS.length + 1, 1]);
            assert.nestedInclude(tokened.at(-1), { id: 'tokened', 'result.content[0].type': 'text' });
        }, Effect.scoped),
    );
});
