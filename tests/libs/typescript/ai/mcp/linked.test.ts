import { assert, describe, it } from '@effect/vitest';
import { Link, Server, Upstream } from '@rasm/mcp';
import { Array, Effect, Layer, Number, Option, Queue, Ref, Schema, Stream } from 'effect';
import { Tool, Toolkit } from 'effect/ai';
import { RpcTest } from 'effect/rpc';
import { connected, lines, names, settled } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LINKED = Link.make({ application: 'Host', extension: 'ext', root: Option.none() });
const JOINED = Link.make({ application: 'Host', extension: 'joined', root: Option.none() });
const NOTICE = { name: 'documentOpened', target: Option.some('/document[1]'), detail: Option.none() };

// --- [MODELS] --------------------------------------------------------------------------

const Extension = Toolkit.make(Tool.make('unique', { description: 'Tool of the extension alone', success: Schema.String }));
const Joined = Toolkit.make(Tool.make('late', { description: 'Tool of the extension joining later', success: Schema.String }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const linked = Effect.fnUntraced(function* (silent: readonly Stream.Stream<readonly Upstream[]>[] = []) {
    const [notices, late, own, lists, subscribed, seen] = yield* Effect.all([Queue.unbounded<Stream.Success<Upstream['notices']>>(), Queue.unbounded<Stream.Success<Upstream['notices']>>(), Queue.unbounded<string>(), Queue.unbounded<Link[]>(), Ref.make(0), Ref.make<readonly string[]>([])]);
    const counted = Stream.unwrap(Effect.as(Ref.update(subscribed, Number.increment), Stream.fromQueue(notices)));
    const [extension, joined] = yield* Effect.all([
        RpcTest.makeClient(Link.Rpcs, { flatten: true }).pipe(Effect.provide(Link.serve(Extension, { notices: counted }).pipe(Layer.provide(Extension.toLayer({ unique: () => Effect.succeed('own') }))))),
        RpcTest.makeClient(Link.Rpcs, { flatten: true }).pipe(Effect.provide(Link.serve(Joined, { notices: Stream.fromQueue(late) }).pipe(Layer.provide(Joined.toLayer({ late: () => Effect.succeed('joined') }))))),
    ]);
    const links = { changes: Stream.concat(Stream.succeed([LINKED]), Stream.fromQueue(lists)), call: (holds: (peer: Link) => boolean) => Effect.succeed(holds(JOINED) ? joined : extension) };
    const proxy = Server.use((server) => server.events({ stream: Stream.fromQueue(own), render: Effect.succeed })).pipe(Effect.andThen(Upstream.serve(Array.append(silent, Upstream.linked(links)), { route: (peers) => Effect.succeed(Array.headNonEmpty(peers)) })), Layer.effectDiscard);
    const { client } = yield* connected(proxy);
    yield* settled(names(client), Array.contains('unique'));
    const [minted] = lines(yield* client.callTool({ name: 'test_simple_text', arguments: {} }));
    const read = client.callTool({ name: 'test_simple_text', arguments: { handle: minted?.split(' ')[1] } }).pipe(Effect.flatMap((result) => Ref.updateAndGet(seen, Array.appendAll(Array.filter(lines(result), (line) => line.startsWith('! #'))))));
    return { notices, late, own, lists, subscribed: Ref.get(subscribed), offers: (name: string) => settled(names(client), Array.contains(name)), events: (count: number) => settled(read, (events) => events.length >= count) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('linked extension', () => {
    it.live(
        'reports a notice the extension sends as an event of the next result, naming the extension, its host, the host event, and its target',
        Effect.fnUntraced(function* () {
            const { notices, events } = yield* linked();
            yield* Queue.offer(notices, NOTICE);
            assert.deepStrictEqual(yield* events(1), ['! #1 ext in Host documentOpened /document[1]']);
        }, Effect.scoped),
    );

    it.live(
        'numbers a notice after an event of another source in one sequence',
        Effect.fnUntraced(function* () {
            const { notices, own, events } = yield* linked();
            yield* Queue.offer(own, 'own event');
            yield* events(1);
            yield* Queue.offer(notices, NOTICE);
            assert.deepStrictEqual(yield* events(2), ['! #1 own event', '! #2 ext in Host documentOpened /document[1]']);
        }, Effect.scoped),
    );

    it.live(
        'keeps the one notice subscription of an extension, and its later notices, when another extension joins the list',
        Effect.fnUntraced(function* () {
            const { notices, late, lists, subscribed, offers, events } = yield* linked();
            yield* Queue.offer(lists, [LINKED, JOINED]);
            yield* offers('late');
            yield* Queue.offer(late, NOTICE);
            yield* events(1);
            yield* Queue.offer(notices, NOTICE);
            assert.deepStrictEqual([yield* events(2), yield* subscribed], [['! #1 joined in Host documentOpened /document[1]', '! #2 ext in Host documentOpened /document[1]'], 1]);
        }, Effect.scoped),
    );

    it.live(
        'lists the tools of an extension while another source has listed nothing yet',
        Effect.fnUntraced(function* () {
            const { offers } = yield* linked([Stream.never]);
            assert.isTrue(Array.contains(yield* offers('unique'), 'unique'));
        }, Effect.scoped),
    );
});
