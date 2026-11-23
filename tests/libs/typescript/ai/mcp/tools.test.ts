import { assert, describe, it } from '@effect/vitest';
import { Array, Effect, Fiber, Option, Queue, Ref, Schema, Struct, SubscriptionRef } from 'effect';
import { McpProtocol } from 'effect/ai';
import { ASKED, Called, opened, STATE } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROTOCOL = McpProtocol.v2026_07_28;
const WEATHER = { temperature: 22.5, conditions: 'Partly cloudy' };
const INVALID_PARAMS = -32_602;

// --- [MODELS] --------------------------------------------------------------------------

const Trailer = Schema.Struct({ result: Schema.optionalKey(Schema.Unknown), handle: Schema.String, events: Schema.Array(Schema.Unknown) });
const Asked = Schema.Struct({ result: Schema.Struct({ resultType: Schema.Literal('input_required'), requestState: Schema.String }) });
const Refused = Schema.Struct({ error: Schema.Struct({ code: Schema.Int }) });
const Listed = Schema.Struct({ result: Schema.Struct({ tools: Schema.Array(Schema.Struct({ name: Schema.String })) }) });
const Owed = Schema.Struct({
    handle: Schema.String,
    early: Schema.optionalKey(Schema.Struct({ call: Schema.Int, reason: Schema.String })),
    pending: Schema.optionalKey(Schema.Array(Schema.Int)),
    finished: Schema.optionalKey(Schema.Array(Schema.Struct({ call: Schema.Int, tool: Schema.String, result: Schema.optionalKey(Schema.Unknown), error: Schema.optionalKey(Schema.String) }))),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const refusal = (message: unknown) => Effect.map(Schema.decodeUnknownEffect(Refused)(message), ({ error }) => error.code);

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('tool calls through the server', () => {
    it.live(
        'returns a new handle beside the result of a call without one, and keeps the handle a call passes back',
        Effect.fnUntraced(function* () {
            const { called } = yield* opened(PROTOCOL);
            const trailed = (call: ReturnType<typeof called>) => Effect.flatMap(call, ({ structuredContent }) => Schema.decodeUnknownEffect(Trailer)(structuredContent));
            const first = yield* trailed(called('test_structured'));
            const [again, other] = yield* Effect.all([trailed(called('test_structured', { handle: first.handle })), trailed(called('test_structured'))]);
            assert.deepStrictEqual([first.result, again.result, again.handle, other.handle === first.handle], [WEATHER, WEATHER, first.handle, false]);
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
        'returns a call at once while its ends stream holds, keeps it running, and delivers its result once in finished of a later call that waits to settle it',
        Effect.fnUntraced(function* () {
            const { probe, called } = yield* opened(PROTOCOL);
            const owed = (name: string, given: Record<string, unknown> = {}) => Effect.flatMap(called(name, given), ({ structuredContent }) => Schema.decodeUnknownEffect(Owed)(structuredContent));
            yield* SubscriptionRef.set(probe.blocked, true);
            const early = yield* owed('gated_early');
            yield* SubscriptionRef.set(probe.blocked, false);
            const settling = yield* Effect.forkScoped(owed('test_settle', { handle: early.handle }));
            const entered = [yield* Queue.take(probe.entered), yield* Queue.take(probe.entered)];
            const waiting = Option.isNone(yield* Effect.timeoutOption(Fiber.await(settling), '100 millis'));
            yield* probe.release.open;
            const [settled, after] = [yield* Fiber.join(settling), yield* owed('test_simple_text', { handle: early.handle })];
            assert.deepStrictEqual([early.early, early.pending, entered, waiting, settled.finished, after.pending, after.finished], [{ call: 1, reason: ASKED }, [1], ['gated_early', 'test_settle'], true, [{ call: 1, tool: 'gated_early', result: 'released' }], undefined, undefined]);
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
            assert.deepStrictEqual([new TextDecoder().decode(Uint8Array.fromBase64(asked.requestState, { alphabet: 'base64url' })).includes(STATE), answered.isError, answered.content[0]], [false, false, { type: 'text', text: `"${STATE}"` }]);
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
