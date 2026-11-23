import { assert, describe, it } from '@effect/vitest';
import { Array, Effect, Result, Struct } from 'effect';
import { echoed, names, proxied, REVISIONS } from '#/conformance';
import { TEXT } from '#/fixtures';

// --- [COMPOSITION] ---------------------------------------------------------------------

describe.each(REVISIONS)('$protocolVersion upstream proxy', (protocol) => {
    it.live(
        'proxies a tool call with its result beside the handle, a resource read, a template read and completion, and a prompt with its completion',
        Effect.fnUntraced(function* () {
            const { client } = yield* proxied(protocol);
            const called = echoed(yield* client.callTool({ name: 'echo', arguments: { value: 'hi' } }));
            const [note, item] = yield* Effect.all([client.readResource({ uri: 'upstream://note' }), client.readResource({ uri: 'upstream://items/apple' })]);
            const [items, people] = yield* Effect.all([client.complete({ ref: { type: 'ref/resource', uri: 'upstream://items/{id}' }, argument: { name: 'id', value: 'ap' } }), client.complete({ ref: { type: 'ref/prompt', name: 'greet' }, argument: { name: 'who', value: 'wo' } })]);
            const greeted = yield* client.getPrompt({ name: 'greet', arguments: { who: 'there' } });
            assert.deepStrictEqual(
                [
                    called,
                    Array.map(note.contents, Struct.get('uri')),
                    Array.map([...note.contents, ...item.contents], (content) => ('text' in content ? content.text : undefined)),
                    items.completion.values.slice(0, 2).toSorted(),
                    people.completion.values[0],
                    greeted.messages.some(({ content }) => 'text' in content && content.text === 'hello there'),
                ],
                [{ handled: true, result: { echoed: 'hi' } }, ['upstream://note'], ['upstream note', 'item apple'], ['apple', 'apricot'], 'world', true],
            );
        }, Effect.scoped),
    );

    it.live(
        'lists no tool its filter denies and refuses a call to it',
        Effect.fnUntraced(function* () {
            const { client } = yield* proxied(protocol);
            const refused = yield* Effect.result(client.callTool({ name: 'secret', arguments: {} }));
            assert.deepStrictEqual([(yield* names(client)).includes('secret'), Result.match(refused, { onFailure: () => true, onSuccess: ({ isError }) => isError === true })], [false, true]);
        }, Effect.scoped),
    );

    it.live(
        'answers a tool sharing a name with a tool of the server from the server, never the upstream',
        Effect.fnUntraced(function* () {
            const { client } = yield* proxied(protocol);
            const { content } = yield* client.callTool({ name: 'test_simple_text', arguments: {} });
            assert.isTrue(content.some((block) => 'text' in block && block.text.includes(TEXT)));
        }, Effect.scoped),
    );
});
