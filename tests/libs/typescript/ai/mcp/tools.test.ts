import { assert, describe, it } from '@effect/vitest';
import { Array, Effect, Record, Ref, Schema, Struct } from 'effect';
import { McpProtocol } from 'effect/ai';
import { Called, lines, opened, STATE } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROTOCOL = McpProtocol.v2026_07_28;
const ISSUED = /^handle [\da-f]{8}-[\da-f]{4}-7[\da-f]{3}-[89ab][\da-f]{3}-[\da-f]{12} holds your state, pass it on later calls$/u;
const INVALID_PARAMS = -32_602;
const CALLS = {
    test_simple_text: [{}, ['text']],
    test_value: [{ value: 'kept' }, ['text']],
    test_image_content: [{}, ['text', 'image']],
    test_embedded_resource: [{}, ['text', 'resource']],
    test_structured: [{}, ['text']],
    test_error_handling: [{}, ['text']],
    test_multiple_content_types: [{}, ['text', 'image', 'resource']],
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const Asked = Schema.Struct({ result: Schema.Struct({ resultType: Schema.Literal('input_required'), requestState: Schema.String }) });
const Refused = Schema.Struct({ error: Schema.Struct({ code: Schema.Int }) });
const Listed = Schema.Struct({ result: Schema.Struct({ tools: Schema.Array(Schema.Struct({ name: Schema.String, outputSchema: Schema.optionalKey(Schema.Unknown) })) }) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const refusal = (message: unknown) => Effect.map(Schema.decodeUnknownEffect(Refused)(message), ({ error }) => error.code);
const named = (line: string | undefined) => line?.split(' ')[1] ?? '';

// --- [COMPOSITION] ---------------------------------------------------------------------

describe.each([McpProtocol.v2025_11_25, McpProtocol.v2026_07_28])('results speaking $protocolVersion', (protocol) => {
    it.live(
        'lists no tool with an output schema, and answers every call with no structured content and one text block leading the blocks the tool adds',
        Effect.fnUntraced(function* () {
            const { called, answer } = yield* opened(protocol);
            const { result } = yield* Effect.flatMap(answer('tools/list'), Schema.decodeUnknownEffect(Listed));
            const results = yield* Effect.all(Record.map(CALLS, ([given], name) => called(name, given)));
            assert.deepStrictEqual([Array.filter(result.tools, (tool) => tool.outputSchema !== undefined), Record.map(results, ({ content, structuredContent }) => ({ structuredContent, types: Array.map(content, Struct.get('type')) }))], [[], Record.map(CALLS, ([, types]) => ({ structuredContent: undefined, types }))]);
        }),
    );
});

describe('tool calls through the server', () => {
    it.live(
        'answers a handle it never issued with an error result and never runs the tool, which runs under a handle it issued',
        Effect.fnUntraced(function* () {
            const { probe, called } = yield* opened(PROTOCOL);
            const issued = yield* called('test_value', { value: 'kept' });
            const refused = yield* called('test_value', { value: 'kept', handle: 'no-such-handle' });
            assert.deepStrictEqual([issued.isError, refused.isError, yield* Ref.get(probe.calls)], [false, true, ['test_value']]);
        }),
    );

    it.live(
        'answers a key its strict tool does not take with an error result naming the nearest parameter, and never runs the tool',
        Effect.fnUntraced(function* () {
            const { probe, called } = yield* opened(PROTOCOL);
            const refused = yield* called('test_value', { value: 'kept', valu: 'kept' });
            assert.deepStrictEqual([refused.isError, lines(refused).at(-1), yield* Ref.get(probe.calls)], [true, 'valu: not a parameter, nearest value', []]);
        }),
    );

    it.live(
        'lists no hidden tool, refuses a call to it as an unknown tool, and never runs it',
        Effect.fnUntraced(function* () {
            const { probe, tool, answer } = yield* opened(PROTOCOL);
            const { result } = yield* Effect.flatMap(answer('tools/list'), Schema.decodeUnknownEffect(Listed));
            const code = yield* Effect.flatMap(tool('test_hidden'), refusal);
            assert.deepStrictEqual([Array.map(result.tools, Struct.get('name')).includes('test_hidden'), code, yield* Ref.get(probe.calls)], [false, INVALID_PARAMS, []]);
        }),
    );
});

describe('client input of a stateless client', () => {
    it.live(
        'seals the state of an input request so the client cannot read it, opens it on the retry of the same tool, and names on the retry the handle the first call started',
        Effect.fnUntraced(function* () {
            const { tool, called } = yield* opened(PROTOCOL);
            const { result: asked } = yield* Effect.flatMap(tool('test_input'), Schema.decodeUnknownEffect(Asked));
            const { result: answered } = yield* Effect.flatMap(tool('test_input', {}, { requestState: asked.requestState }), Schema.decodeUnknownEffect(Called));
            const [line, , body] = lines(answered);
            const resumed = yield* called('test_value', { value: 'kept', handle: named(line) });
            assert.deepStrictEqual([asked.requestState.split('.').some((part) => new TextDecoder().decode(Uint8Array.fromBase64(part, { alphabet: 'base64url' })).includes(STATE)), answered.isError, ISSUED.test(line ?? ''), body, resumed.isError, lines(resumed)], [false, false, true, STATE, false, ['kept']]);
        }),
    );

    it.live.each([
        { label: 'altered in one sealed byte', tool: 'test_input', altered: (state: string) => state.replace(/(?<=\.)[^.](?=[^.]*$)/u, (sealed) => (sealed === 'A' ? 'B' : 'A')) },
        { label: 'sealed for another tool', tool: 'test_value', altered: (state: string) => state },
    ] as const)(
        'answers a request state $label with an error result naming the refusal, without running the tool',
        Effect.fnUntraced(function* ({ tool: target, altered }) {
            const { probe, tool } = yield* opened(PROTOCOL);
            const { result: asked } = yield* Effect.flatMap(tool('test_input'), Schema.decodeUnknownEffect(Asked));
            const { result: refused } = yield* Effect.flatMap(tool(target, { value: 'replayed' }, { requestState: altered(asked.requestState) }), Schema.decodeUnknownEffect(Called));
            assert.deepStrictEqual([refused.isError, lines(refused), yield* Ref.get(probe.calls)], [true, ['requestState came from no input request of this server and tool, call again without it'], ['test_input']]);
        }),
    );
});
