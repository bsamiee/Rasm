import { assert, describe, it } from '@effect/vitest';
import { Array, Effect, Option, Schema, Struct } from 'effect';
import { McpProtocol } from 'effect/ai';
import { INSTRUCTIONS, opened } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROTOCOL = McpProtocol.v2025_11_25;
const INITIALIZE = { jsonrpc: '2.0', id: 1, method: 'initialize', params: { protocolVersion: PROTOCOL.protocolVersion, capabilities: {}, clientInfo: { name: 'gate', version: '1.0.0' } } } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Instructed = Schema.Struct({ result: Schema.Struct({ instructions: Schema.String }) });
const Answered = Schema.Struct({ id: Schema.String, result: Schema.Struct({ content: Schema.NonEmptyArray(Schema.Unknown) }) });
const Progress = Schema.Struct({ method: Schema.Literal('notifications/progress'), params: Schema.Struct({ progressToken: Schema.String, progress: Schema.Finite, total: Schema.Finite }) });

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('DNS-rebinding gate of an endpoint bound to 192.0.2.10:7000 allowing mcp.example', () => {
    it.live.each([
        { host: 'attacker.example', status: 403 },
        { host: 'attacker.example:7000', status: 403 },
        { host: 'localhost', status: 200 },
        { host: '127.0.0.1:9', status: 200 },
        { host: '[::1]:7000', status: 200 },
        { host: 'mcp.example:8080', status: 200 },
        { host: '192.0.2.10:7000', status: 200 },
    ] as const)(
        'answers a request addressed to Host $host with $status',
        Effect.fnUntraced(function* ({ host, status }) {
            const { post } = yield* opened(PROTOCOL);
            assert.strictEqual((yield* post(INITIALIZE, { headers: { host } })).status, status);
        }),
    );

    it.live.each([
        { origin: 'https://attacker.example', status: 403 },
        { origin: 'http://localhost:7001', status: 403 },
        { origin: 'http://localhost:7000', status: 200 },
        { origin: 'https://mcp.example', status: 200 },
        { origin: 'http://192.0.2.10:7000', status: 200 },
    ] as const)(
        'answers a request from Origin $origin with $status',
        Effect.fnUntraced(function* ({ origin, status }) {
            const { post } = yield* opened(PROTOCOL);
            assert.strictEqual((yield* post(INITIALIZE, { headers: { origin } })).status, status);
        }),
    );
});

describe('session of a stateful client', () => {
    it.live(
        'states the instructions of the server and of each visible registration at initialize, never those of a hidden one',
        Effect.fnUntraced(function* () {
            const { initialize, messages } = yield* opened(PROTOCOL);
            const [message] = yield* Effect.flatMap(initialize, messages);
            const lines = (yield* Schema.decodeUnknownEffect(Instructed)(message)).result.instructions.split('\n');
            assert.deepStrictEqual(
                [INSTRUCTIONS.stated, INSTRUCTIONS.shown, INSTRUCTIONS.hidden].map((line) => lines.includes(line)),
                [true, true, false],
            );
        }),
    );

    it.live(
        'reports each progress step under the token of its request before the result, and none to a request without a token',
        Effect.fnUntraced(function* () {
            const { call } = yield* opened(PROTOCOL);
            const tokened = yield* call('tools/call', { name: 'test_tool_with_progress', arguments: {}, _meta: { progressToken: 'progress-1' } }, 'tokened');
            const plain = yield* call('tools/call', { name: 'test_tool_with_progress', arguments: {} }, 'plain');
            assert.deepStrictEqual(
                {
                    before: Array.map(tokened.slice(0, -1), (message) => Schema.decodeUnknownOption(Progress)(message)),
                    answered: Array.map([tokened.at(-1), ...plain], (message) => Option.map(Schema.decodeUnknownOption(Answered)(message), Struct.get('id'))),
                },
                { before: [0, 50, 100].map((progress) => Option.some({ method: 'notifications/progress', params: { progressToken: 'progress-1', progress, total: 100 } })), answered: [Option.some('tokened'), Option.some('plain')] },
            );
        }),
    );
});
