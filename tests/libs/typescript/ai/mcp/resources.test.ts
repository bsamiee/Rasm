import { assert, describe, it } from '@effect/vitest';
import { Array, Effect, Layer, Option, Order, Queue, Schema, String, SubscriptionRef } from 'effect';
import { McpProtocol } from 'effect/ai';
import { connected, outcome, PROTOCOLS, served, since, STATEFUL } from '#/conformance';
import { IDS, PNG, STATIC } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ABSENT = { modern: -32_602, legacy: -32_002 } as const;
const COMPLETED = { most: 100 } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Resources = Schema.Struct({ result: Schema.Struct({ resources: Schema.Array(Schema.Struct({ uri: Schema.String, name: Schema.String, description: Schema.optionalKey(Schema.String), mimeType: Schema.optionalKey(Schema.String) })) }) });
const Templates = Schema.Struct({ result: Schema.Struct({ resourceTemplates: Schema.Array(Schema.Struct({ uriTemplate: Schema.String, name: Schema.String })) }) });
const Read = Schema.Struct({ result: Schema.Struct({ contents: Schema.Tuple([Schema.Struct({ uri: Schema.String, mimeType: Schema.optionalKey(Schema.String), text: Schema.optionalKey(Schema.String), blob: Schema.optionalKey(Schema.String) })]) }) });
const Completed = Schema.Struct({ result: Schema.Struct({ completion: Schema.Struct({ values: Schema.Array(Schema.String), total: Schema.optionalKey(Schema.Finite) }) }) });

// --- [COMPOSITION] ---------------------------------------------------------------------

describe.each(PROTOCOLS)('$protocolVersion resources', (protocol) => {
    const absent = since(protocol, McpProtocol.v2026_07_28) ? ABSENT.modern : ABSENT.legacy;

    it.live(
        'lists each registered resource under the server scheme with its name, description, and media type',
        Effect.fnUntraced(function* () {
            const { test, held } = yield* served(protocol);
            const { result } = yield* Effect.flatMap(test.answer(held, 'resources/list'), Schema.decodeUnknownEffect(Resources));
            assert.includeDeepMembers(
                result.resources.map(({ uri, name, description, mimeType }) => ({ uri, name, description, mimeType })),
                [
                    { uri: 'test://static-text', name: 'static-text', description: 'A static text resource for testing', mimeType: 'text/plain' },
                    { uri: 'test://static-binary', name: 'static-binary', description: 'A static binary resource for testing', mimeType: 'image/png' },
                    { uri: 'test://watched-resource', name: 'watched-resource', description: 'A resource whose value changes', mimeType: 'text/plain' },
                ],
            );
        }, Effect.scoped),
    );

    it.live.each([
        { uri: 'test://static-text', content: { mimeType: 'text/plain', text: STATIC } },
        { uri: 'test://static-binary', content: { mimeType: 'image/png', blob: PNG } },
        { uri: 'test://template/alpha/data', content: { mimeType: 'application/json', text: '{"id":"alpha","templateTest":true,"data":"Data for ID: alpha"}' } },
    ] as const)(
        'reads $uri as one content entry holding that URI, its media type, and its text or base64 bytes',
        Effect.fnUntraced(function* ({ uri, content }) {
            const { test, held } = yield* served(protocol);
            const { result } = yield* Effect.flatMap(test.answer(held, 'resources/read', { uri }), Schema.decodeUnknownEffect(Read));
            assert.deepStrictEqual(result.contents, [{ uri, ...content }]);
        }, Effect.scoped),
    );

    it.live(
        'answers a read of a resource that does not exist with the resource-not-found error of the revision: -32002 through 2025-11-25, -32602 from 2026-07-28',
        Effect.fnUntraced(function* () {
            const { test, held } = yield* served(protocol);
            assert.deepStrictEqual(yield* Effect.flatMap(test.answer(held, 'resources/read', { uri: 'test://nonexistent-resource-for-conformance-testing' }), outcome), { code: absent, failed: false });
        }, Effect.scoped),
    );

    it.live(
        'lists a resource template by its URI template and name',
        Effect.fnUntraced(function* () {
            const { test, held } = yield* served(protocol);
            const { result } = yield* Effect.flatMap(test.answer(held, 'resources/templates/list'), Schema.decodeUnknownEffect(Templates));
            assert.deepInclude(result.resourceTemplates, { uriTemplate: 'test://template/{id}/data', name: 'template' });
        }, Effect.scoped),
    );

    it.live(
        'completes a template argument with the values nearest the typed text first, at most 100 of them, and their count as total',
        Effect.fnUntraced(function* () {
            const { test, held } = yield* served(protocol);
            const { result } = yield* Effect.flatMap(test.answer(held, 'completion/complete', { ref: { type: 'ref/resource', uri: 'test://template/{id}/data' }, argument: { name: 'id', value: 'al' } }), Schema.decodeUnknownEffect(Completed));
            const [{ values, total }, nearest] = [result.completion, IDS.filter(String.startsWith('al'))];
            assert.deepStrictEqual({ nearest: Array.sort(values.slice(0, nearest.length), Order.String), bounded: values.length <= COMPLETED.most, total }, { nearest: Array.sort(nearest, Order.String), bounded: true, total: values.length });
        }, Effect.scoped),
    );
});

describe.each(STATEFUL)('$protocolVersion resource subscription', (protocol) => {
    it.live(
        'notifies a subscriber of a resource with its URI when the value of the resource changes',
        Effect.fnUntraced(function* () {
            const { probe, client } = yield* connected(protocol, Layer.empty);
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
