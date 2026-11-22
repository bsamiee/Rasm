import { assert, describe, it } from '@effect/vitest';
import { Array, Effect, Fiber, Option, Queue, Ref, Schema, Struct, SubscriptionRef } from 'effect';
import { McpProtocol } from 'effect/ai';
import { Called, outcome, PROTOCOLS, served, since } from '#/conformance.ts';
import { ASKED, EMBEDDED, FAILURE, PNG, STATE } from '#/fixtures.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const WEATHER = { temperature: 22.5, conditions: 'Partly cloudy' };

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);
const Trailer = Schema.Struct({ result: Schema.optionalKey(Schema.Unknown), handle: Schema.String, events: Schema.Array(Schema.Unknown) });
const Asked = Schema.Struct({ result: Schema.Struct({ resultType: Schema.Literal('input_required'), requestState: Schema.String }) });
const Owed = Schema.Struct({
    handle: Schema.String,
    early: Schema.optionalKey(Schema.Struct({ call: Schema.Int, reason: Schema.String })),
    pending: Schema.optionalKey(Schema.Array(Schema.Int)),
    finished: Schema.optionalKey(Schema.Array(Schema.Struct({ call: Schema.Int, tool: Schema.String, result: Schema.optionalKey(Schema.Unknown), error: Schema.optionalKey(Schema.String) }))),
});

// --- [COMPOSITION] ---------------------------------------------------------------------

describe.each(PROTOCOLS)('$protocolVersion tools', (protocol) => {
    const [structured, validating] = [since(protocol, McpProtocol.v2025_06_18), since(protocol, McpProtocol.v2025_11_25)];

    it.live(
        'returns image bytes as an image block holding base64 data and the media type',
        Effect.fnUntraced(function* () {
            const { content } = yield* served(protocol);
            const { content: blocks } = yield* content('test_image_content');
            assert.deepStrictEqual(
                blocks.filter(({ type }) => type === 'image').map(({ data, mimeType }) => ({ data, mimeType })),
                [{ data: PNG, mimeType: 'image/png' }],
            );
        }, Effect.scoped),
    );

    it.live(
        'returns a file as an embedded resource block holding its URI, media type, and its content as text or base64 bytes',
        Effect.fnUntraced(function* () {
            const { content } = yield* served(protocol);
            const { content: blocks } = yield* content('test_embedded_resource');
            assert.deepStrictEqual(
                blocks.filter(({ type }) => type === 'resource').map(({ resource }) => ({ uri: resource?.uri, mimeType: resource?.mimeType, held: resource?.text === EMBEDDED || resource?.blob === new TextEncoder().encode(EMBEDDED).toBase64() })),
                [{ uri: 'test://embedded-resource', mimeType: 'text/plain', held: true }],
            );
        }, Effect.scoped),
    );

    it.live(
        'returns a struct serialized in a text block, and as structured content from 2025-06-18',
        Effect.fnUntraced(function* () {
            const { content } = yield* served(protocol);
            const { content: blocks, structuredContent } = yield* content('test_structured');
            assert.deepStrictEqual([Option.getOrUndefined(Option.map(Schema.decodeUnknownOption(Trailer)(structuredContent), Struct.get('result'))), Array.getSomes(blocks.map(({ text }) => Schema.decodeUnknownOption(Json)(text)))], [structured ? WEATHER : undefined, [WEATHER]]);
        }, Effect.scoped),
    );

    it.live.runIf(structured)(
        'returns the handle and the events since the last call as structured content beside the result, and the handle a call passes back unchanged',
        Effect.fnUntraced(function* () {
            const { tool } = yield* served(protocol);
            const first = yield* Effect.flatMap(Effect.flatMap(tool('test_structured'), Schema.decodeUnknownEffect(Called)), ({ result }) => Schema.decodeUnknownEffect(Trailer)(result.structuredContent));
            const again = yield* Effect.flatMap(Effect.flatMap(tool('test_structured', { handle: first.handle }), Schema.decodeUnknownEffect(Called)), ({ result }) => Schema.decodeUnknownEffect(Trailer)(result.structuredContent));
            assert.deepStrictEqual([first.result, first.events, again.handle], [WEATHER, [], first.handle]);
        }, Effect.scoped),
    );

    it.live.runIf(structured)(
        'returns a call at once while its ends stream holds, keeps it running, and delivers its result once in finished of the call that settles it',
        Effect.fnUntraced(function* () {
            const { test, tool } = yield* served(protocol);
            const owed = (name: string, given: Record<string, unknown> = {}) => Effect.flatMap(Effect.flatMap(tool(name, given), Schema.decodeUnknownEffect(Called)), ({ result }) => Schema.decodeUnknownEffect(Owed)(result.structuredContent));
            yield* SubscriptionRef.set(test.probe.blocked, true);
            const early = yield* owed('gated_early');
            yield* SubscriptionRef.set(test.probe.blocked, false);
            const settling = yield* Effect.forkScoped(owed('test_settle', { handle: early.handle }));
            yield* test.probe.release.open;
            const [settled, after] = [yield* Fiber.join(settling), yield* owed('test_simple_text', { handle: early.handle })];
            assert.deepStrictEqual([early.early, early.pending, settled.finished, after.pending, after.finished], [{ call: 1, reason: ASKED }, [1], [{ call: 1, tool: 'gated_early', result: 'released' }], undefined, undefined]);
        }, Effect.scoped),
    );

    it.live(
        'answers a handle it never issued with an error result naming expiry and starting a new one by omitting it',
        Effect.fnUntraced(function* () {
            const { content } = yield* served(protocol);
            const { content: blocks, isError } = yield* content('test_value', { value: 'kept', handle: 'no-such-handle' });
            assert.deepStrictEqual([isError, blocks.some(({ text }) => text?.includes('unknown or expired') === true && text.includes('without handle'))], [true, true]);
        }, Effect.scoped),
    );

    it.live(
        'releases a scoped resource the tool acquires when the call ends',
        Effect.fnUntraced(function* () {
            const { test, content } = yield* served(protocol);
            const { isError } = yield* content('test_scoped');
            assert.deepStrictEqual([isError, yield* Effect.timeoutOption(Queue.take(test.probe.released), '2 seconds')], [false, Option.some('test_scoped')]);
        }, Effect.scoped),
    );

    it.live(
        'returns a failure of the tool as a result with isError and the failure text, never as a JSON-RPC error',
        Effect.fnUntraced(function* () {
            const { content } = yield* served(protocol);
            const { content: blocks, isError } = yield* content('test_error_handling');
            assert.deepStrictEqual([isError, blocks.some(({ text }) => text?.includes(FAILURE) === true)], [true, true]);
        }, Effect.scoped),
    );

    it.live(
        'answers a call to a tool it does not hold with JSON-RPC error -32602',
        Effect.fnUntraced(function* () {
            const { tool } = yield* served(protocol);
            assert.deepStrictEqual(yield* Effect.flatMap(tool('no_such_tool'), outcome), { code: -32_602, failed: false });
        }, Effect.scoped),
    );

    it.live(
        'reports a defect of a tool as an error holding no text of the defect',
        Effect.fnUntraced(function* () {
            const { tool } = yield* served(protocol);
            const message = yield* tool('test_defect');
            const { code, failed } = yield* outcome(message);
            assert.deepStrictEqual([code !== undefined || failed, (yield* Schema.encodeUnknownEffect(Json)(message)).includes('db.internal')], [true, false]);
        }, Effect.scoped),
    );

    it.live(
        'refuses a call to a hidden tool and never runs it',
        Effect.fnUntraced(function* () {
            const { test, tool } = yield* served(protocol);
            const { code, failed } = yield* Effect.flatMap(tool('test_hidden'), outcome);
            assert.deepStrictEqual([code !== undefined || failed, yield* Ref.get(test.probe.calls)], [true, []]);
        }, Effect.scoped),
    );

    it.live(
        'refuses arguments of the wrong type without running the tool: JSON-RPC error -32602 through 2025-06-18, a result with isError from 2025-11-25',
        Effect.fnUntraced(function* () {
            const { test, tool } = yield* served(protocol);
            const refusal = yield* Effect.flatMap(tool('test_value', { value: 123 }), outcome);
            assert.deepStrictEqual([refusal, yield* Ref.get(test.probe.calls)], [validating ? { code: undefined, failed: true } : { code: -32_602, failed: false }, []]);
        }, Effect.scoped),
    );
});

describe('2026-07-28 client input', () => {
    it.live(
        'seals the state of an input request so the client cannot read it, opens it on the retry for the tool, and refuses a state it did not seal',
        Effect.fnUntraced(function* () {
            const { test, held, tool } = yield* served(McpProtocol.v2026_07_28);
            const { result: asked } = yield* Effect.flatMap(tool('test_input'), Schema.decodeUnknownEffect(Asked));
            const retried = (requestState: string) => test.answer(held, 'tools/call', { name: 'test_input', arguments: {}, requestState });
            const { result: answered } = yield* Effect.flatMap(retried(asked.requestState), Schema.decodeUnknownEffect(Called));
            const forged = yield* Effect.flatMap(retried(`${asked.requestState}x`), outcome);
            assert.deepStrictEqual([asked.requestState.includes(STATE), answered.isError, answered.content.some(({ text }) => text?.includes(STATE) === true), forged.code !== undefined || forged.failed], [false, false, true, true]);
        }, Effect.scoped),
    );
});
