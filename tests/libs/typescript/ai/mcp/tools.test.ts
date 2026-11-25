import { assert, describe, it } from '@effect/vitest';
import { Array, Effect, Fiber, Option, Queue, Record, Ref, Schema, String, Struct, SubscriptionRef } from 'effect';
import { McpProtocol } from 'effect/ai';
import { ASKED, Called, lines, opened, STATE, settled, TEXT } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROTOCOL = McpProtocol.v2026_07_28;
const WEATHER = '{"temperature":22.5,"conditions":"Partly cloudy"}';
const MINTED = /^handle [\da-f]{8}(?:-[\da-f]{4}){3}-[\da-f]{12}$/u;
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
        'leads the JSON of a structured result with a new handle for a call without one, and with no handle line for a call passing it back',
        Effect.fnUntraced(function* () {
            const { called } = yield* opened(PROTOCOL);
            const [minted, body] = lines(yield* called('test_structured'));
            const handle = minted?.slice('handle '.length);
            const [again, other] = yield* Effect.all([Effect.map(called('test_structured', { handle }), lines), Effect.map(called('test_structured'), lines)]);
            assert.deepStrictEqual([MINTED.test(minted ?? ''), body, again, MINTED.test(other[0] ?? ''), other[0] === minted], [true, WEATHER, [WEATHER], true, false]);
        }),
    );

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
        'leads results with the minted handle, the events its handle owns in one sequence, an early return with its pending call, and its finished result once in a later call that waits to settle it',
        Effect.fnUntraced(function* () {
            const { probe, called } = yield* opened(PROTOCOL);
            const text = (name: string, given: Record.ReadonlyRecord<string, unknown> = {}) => Effect.map(called(name, given), (result) => lines(result).join('\n'));
            const seen = (handle: string) =>
                settled(
                    Effect.map(called('test_simple_text', { handle }), (result) => Array.filter(lines(result), String.startsWith('! '))),
                    Array.isReadonlyArrayNonEmpty,
                );
            const [owner, other] = [yield* text('test_claim'), yield* text('test_simple_text')];
            const [claimer, bystander] = [owner.slice('handle '.length, owner.indexOf('\n')), other.slice('handle '.length, other.indexOf('\n'))];
            yield* Queue.offer(probe.owned, 'owned one');
            const owned = yield* seen(claimer);
            yield* Queue.offer(probe.shared, 'shared one');
            const shared = yield* seen(bystander);
            yield* SubscriptionRef.set(probe.blocked, true);
            const early = yield* text('gated_early', { handle: claimer });
            yield* SubscriptionRef.set(probe.blocked, false);
            const pending = yield* text('test_simple_text', { handle: claimer });
            yield* Queue.clear(probe.entered);
            const settling = yield* Effect.forkScoped(text('test_settle', { handle: claimer }));
            const entered = yield* Queue.take(probe.entered);
            const waiting = Option.isNone(yield* Effect.timeoutOption(Fiber.await(settling), '100 millis'));
            yield* probe.release.open;
            const [finished, after] = [yield* Fiber.join(settling), yield* text('test_simple_text', { handle: claimer })];
            assert.deepStrictEqual(
                { minted: [MINTED.test(owner.split('\n')[0] ?? ''), MINTED.test(other.split('\n')[0] ?? ''), claimer === bystander], bodies: [owner.split('\n')[1], other.split('\n')[1]], owned, shared, early, pending, entered, waiting, finished, after },
                {
                    minted: [true, true, false],
                    bodies: ['claimed', TEXT],
                    owned: ['! #1 owned one'],
                    shared: ['! #2 shared one'],
                    early: `! #2 shared one\nearly: call 1 keeps running, its result arrives under finished\n${ASKED}`,
                    pending: `pending: calls 1\n${TEXT}`,
                    entered: 'test_settle',
                    waiting: true,
                    finished: 'finished: call 1 gated_early released\nsettled',
                    after: TEXT,
                },
            );
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
        'seals the state of an input request so the client cannot read it, and opens it on the retry of the same tool',
        Effect.fnUntraced(function* () {
            const { tool } = yield* opened(PROTOCOL);
            const { result: asked } = yield* Effect.flatMap(tool('test_input'), Schema.decodeUnknownEffect(Asked));
            const { result: answered } = yield* Effect.flatMap(tool('test_input', {}, { requestState: asked.requestState }), Schema.decodeUnknownEffect(Called));
            assert.deepStrictEqual([new TextDecoder().decode(Uint8Array.fromBase64(asked.requestState, { alphabet: 'base64url' })).includes(STATE), answered.isError, answered.content], [false, false, [{ type: 'text', text: STATE }]]);
        }),
    );

    it.live.each([
        { label: 'altered in one sealed byte', tool: 'test_input', altered: (state: string) => `${state.slice(0, 40)}${state.at(40) === 'A' ? 'B' : 'A'}${state.slice(41)}` },
        { label: 'sealed for another tool', tool: 'test_value', altered: (state: string) => state },
    ] as const)(
        'refuses a request state $label as invalid params without running the tool',
        Effect.fnUntraced(function* ({ tool: target, altered }) {
            const { probe, tool } = yield* opened(PROTOCOL);
            const { result: asked } = yield* Effect.flatMap(tool('test_input'), Schema.decodeUnknownEffect(Asked));
            const code = yield* Effect.flatMap(tool(target, { value: 'replayed' }, { requestState: altered(asked.requestState) }), refusal);
            assert.deepStrictEqual([code, yield* Ref.get(probe.calls)], [INVALID_PARAMS, ['test_input']]);
        }),
    );
});
