import { assert, describe, it } from '@effect/vitest';
import { Link, Server, Upstream } from '@rasm/mcp';
import { Array, Effect, Layer, Number, Option, Queue, Ref, Schema, Stream } from 'effect';
import { Tool, Toolkit } from 'effect/ai';
import { RpcTest } from 'effect/rpc';
import { connected, lines, names, settled } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LINKED = Link.make({ application: 'Host', plugin: 'ext', root: Option.none() });
const JOINED = Link.make({ application: 'Host', plugin: 'joined', root: Option.none() });
const HOST_EVENT = { name: 'documentOpened', target: Option.some('/document[1]'), detail: Option.none() };

// --- [MODELS] --------------------------------------------------------------------------

const Plugin = Toolkit.make(Tool.make('unique', { description: 'Tool of the plugin alone', success: Schema.String }));
const Joined = Toolkit.make(Tool.make('late', { description: 'Tool of the plugin joining later', success: Schema.String }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const linked = Effect.fnUntraced(function* (silent: readonly Stream.Stream<readonly Upstream[]>[] = []) {
    const [hostEvents, late, own, lists, subscribed, seen] = yield* Effect.all([Queue.unbounded<Stream.Success<Upstream['events']>>(), Queue.unbounded<Stream.Success<Upstream['events']>>(), Queue.unbounded<string>(), Queue.unbounded<Link[]>(), Ref.make(0), Ref.make<readonly string[]>([])]);
    const counted = Stream.unwrap(Effect.as(Ref.update(subscribed, Number.increment), Stream.fromQueue(hostEvents)));
    const [plugin, joined] = yield* Effect.all([
        RpcTest.makeClient(Link.Rpcs, { flatten: true }).pipe(Effect.provide(Link.serve(Plugin, { events: counted }).pipe(Layer.provide(Plugin.toLayer({ unique: () => Effect.succeed('own') }))))),
        RpcTest.makeClient(Link.Rpcs, { flatten: true }).pipe(Effect.provide(Link.serve(Joined, { events: Stream.fromQueue(late) }).pipe(Layer.provide(Joined.toLayer({ late: () => Effect.succeed('joined') }))))),
    ]);
    const links = { changes: Stream.concat(Stream.succeed([LINKED]), Stream.fromQueue(lists)), call: (holds: (peer: Link) => boolean) => Effect.succeed(holds(JOINED) ? joined : plugin) };
    const proxy = Server.use((server) => server.events({ stream: Stream.fromQueue(own), render: Effect.succeed })).pipe(Effect.andThen(Upstream.serve(Array.append(silent, Upstream.fromLinks(links)), { route: (peers) => Effect.succeed(Array.headNonEmpty(peers)) })), Layer.effectDiscard);
    const { client } = yield* connected(proxy);
    yield* settled(names(client), Array.contains('unique'));
    const [issued] = lines(yield* client.callTool({ name: 'test_simple_text', arguments: {} }));
    const read = client.callTool({ name: 'test_simple_text', arguments: { handle: issued?.split(' ')[1] } }).pipe(Effect.flatMap((result) => Ref.updateAndGet(seen, Array.appendAll(Array.filter(lines(result), (line) => line.startsWith('! #'))))));
    return { hostEvents, late, own, lists, subscribed: Ref.get(subscribed), offers: (name: string) => settled(names(client), Array.contains(name)), events: (count: number) => settled(read, (events) => events.length >= count) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('linked plugin', () => {
    it.live(
        'reports an event the plugin sends as an event of the next result, naming the plugin, its host, the host event, and its target',
        Effect.fnUntraced(function* () {
            const { hostEvents, events } = yield* linked();
            yield* Queue.offer(hostEvents, HOST_EVENT);
            assert.deepStrictEqual(yield* events(1), ['! #1 ext in Host documentOpened /document[1]']);
        }, Effect.scoped),
    );

    it.live(
        'numbers a plugin event after an event of another source in one sequence',
        Effect.fnUntraced(function* () {
            const { hostEvents, own, events } = yield* linked();
            yield* Queue.offer(own, 'own event');
            yield* events(1);
            yield* Queue.offer(hostEvents, HOST_EVENT);
            assert.deepStrictEqual(yield* events(2), ['! #1 own event', '! #2 ext in Host documentOpened /document[1]']);
        }, Effect.scoped),
    );

    it.live(
        'keeps the one event subscription of a plugin, and its later events, when another plugin joins the list',
        Effect.fnUntraced(function* () {
            const { hostEvents, late, lists, subscribed, offers, events } = yield* linked();
            yield* Queue.offer(lists, [LINKED, JOINED]);
            yield* offers('late');
            yield* Queue.offer(late, HOST_EVENT);
            yield* events(1);
            yield* Queue.offer(hostEvents, HOST_EVENT);
            assert.deepStrictEqual([yield* events(2), yield* subscribed], [['! #1 joined in Host documentOpened /document[1]', '! #2 ext in Host documentOpened /document[1]'], 1]);
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
