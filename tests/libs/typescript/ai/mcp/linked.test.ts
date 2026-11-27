import { assert, describe, it } from '@effect/vitest';
import { Link } from '@rasm/mcp/link';
import { Upstream } from '@rasm/mcp/upstream';
import { Array, Effect, Layer, Schema, Stream } from 'effect';
import { Tool, Toolkit } from 'effect/ai';
import { RpcTest } from 'effect/rpc';
import { connected, lines, names, relisted } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LINKED = Link.make({ application: 'Host', plugin: 'ext' });

// --- [MODELS] --------------------------------------------------------------------------

const Plugin = Toolkit.make(Tool.make('unique', { description: 'Tool of the plugin alone', success: Schema.String }).annotate(Tool.Strict, true));

// --- [OPERATIONS] ----------------------------------------------------------------------

const linked = Effect.fnUntraced(function* (silent: readonly Stream.Stream<readonly Upstream[]>[] = []) {
    const plugin = yield* RpcTest.makeClient(Link.Rpcs, { flatten: true }).pipe(Effect.provide(Link.serve(Plugin, Stream.never).pipe(Layer.provide(Plugin.toLayer({ unique: () => Effect.succeed('own') })))));
    const links = { changes: Stream.concat(Stream.succeed([LINKED]), Stream.never), call: () => Effect.succeed(plugin) };
    const proxy = Upstream.serve(Array.append(silent, Upstream.fromLinks(links)), { route: (peers) => Effect.succeed(Array.headNonEmpty(peers)) }).pipe(Layer.effectDiscard);
    const { client } = yield* connected(proxy);
    const offers = (name: string) => relisted(client, names(client), Array.contains(name));
    yield* offers('unique');
    const [issued] = lines(yield* client.callTool({ name: 'test_simple_text', arguments: {} }));
    const handle = yield* Effect.fromNullishOr(issued?.split(' ')[1]);
    return { client, handle, offers };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('linked plugin', () => {
    it.live(
        'answers a key the strict plugin tool does not take with the refusal the plugin words, as an error result',
        Effect.fnUntraced(function* () {
            const { client, handle } = yield* linked();
            const refused = yield* client.callTool({ name: 'unique', arguments: { handle, extra: 'kept' } });
            assert.deepStrictEqual([refused.isError, lines(refused)], [true, ['extra: not a parameter, this takes none']]);
        }, Effect.scoped),
    );

    it.live(
        'lists the tools of a plugin while another source has listed nothing yet',
        Effect.fnUntraced(function* () {
            const { offers } = yield* linked([Stream.never]);
            assert.isTrue(Array.contains(yield* offers('unique'), 'unique'));
        }, Effect.scoped),
    );
});
