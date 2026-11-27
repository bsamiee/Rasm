import { assert, describe, it } from '@effect/vitest';
import { Link } from '@rasm/mcp/link';
import { Server } from '@rasm/mcp/server';
import { Upstream } from '@rasm/mcp/upstream';
import { Array, Cause, Duration, Effect, Layer, Number, Option, Queue, Ref, Schedule, Schema, Stream } from 'effect';
import { Tool, Toolkit } from 'effect/ai';
import { RpcTest } from 'effect/rpc';
import { connected, lines, names, relisted, SETTLED } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LINKED = Link.make({ application: 'Host', plugin: 'ext' });
const JOINED = Link.make({ application: 'Host', plugin: 'joined' });
const HOST_EVENT = { name: 'documentOpened', target: Option.some('/document[1]'), detail: Option.none() };

// --- [MODELS] --------------------------------------------------------------------------

const Plugin = Toolkit.make(Tool.make('unique', { description: 'Tool of the plugin alone', success: Schema.String }).annotate(Tool.Strict, true));
const Joined = Toolkit.make(Tool.make('late', { description: 'Tool of the plugin joining later', success: Schema.String }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const linked = Effect.fnUntraced(function* (silent: readonly Stream.Stream<readonly Upstream[]>[] = []) {
    const [hostEvents, late, own, lists, subscribed, seen] = yield* Effect.all([Queue.unbounded<Stream.Success<Upstream['events']>, Error>(), Queue.unbounded<Stream.Success<Upstream['events']>>(), Queue.unbounded<string>(), Queue.unbounded<Link[]>(), Ref.make(0), Ref.make<readonly string[]>([])]);
    const counted = Stream.unwrap(Effect.as(Ref.update(subscribed, Number.increment), Stream.fromQueue(hostEvents)));
    const [plugin, joined] = yield* Effect.all([
        RpcTest.makeClient(Link.Rpcs, { flatten: true }).pipe(Effect.provide(Link.serve(Plugin, counted).pipe(Layer.provide(Plugin.toLayer({ unique: () => Effect.succeed('own') }))))),
        RpcTest.makeClient(Link.Rpcs, { flatten: true }).pipe(Effect.provide(Link.serve(Joined, Stream.fromQueue(late)).pipe(Layer.provide(Joined.toLayer({ late: () => Effect.succeed('joined') }))))),
    ]);
    const links = { changes: Stream.concat(Stream.succeed([LINKED]), Stream.fromQueue(lists)), call: (holds: (peer: Link) => boolean) => Effect.succeed(holds(JOINED) ? joined : plugin) };
    const proxy = Server.use((server) => server.events({ stream: Stream.fromQueue(own), render: Effect.succeed })).pipe(Effect.andThen(Upstream.serve(Array.append(silent, Upstream.fromLinks(links)), { route: (peers) => Effect.succeed(Array.headNonEmpty(peers)) })), Layer.effectDiscard);
    const { client } = yield* connected(proxy);
    const offers = (name: string) => relisted(client, names(client), Array.contains(name));
    yield* offers('unique');
    const [issued] = lines(yield* client.callTool({ name: 'test_simple_text', arguments: {} }));
    const handle = yield* Effect.fromNullishOr(issued?.split(' ')[1]);
    const read = client.callTool({ name: 'test_simple_text', arguments: { handle } }).pipe(Effect.flatMap((result) => Ref.updateAndGet(seen, Array.appendAll(Array.filter(lines(result), (line) => line.startsWith('! #'))))));
    const events = (count: number) =>
        read.pipe(Effect.repeat({ schedule: Schedule.spaced('20 millis'), until: (held) => held.length >= count }), Effect.timeoutOrElse({ duration: SETTLED, orElse: () => Effect.fail(new Cause.UnknownError(handle, `handle ${handle} led no ${count} events after ${Duration.format(SETTLED)}`)) }));
    return { client, handle, hostEvents, late, own, lists, subscribed: Ref.get(subscribed), offers, events };
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
        'reports the failure ending the event stream of the plugin as an event naming the plugin and its host',
        Effect.fnUntraced(function* () {
            const { hostEvents, events } = yield* linked();
            yield* Queue.fail(hostEvents, new Error('the host quit'));
            assert.deepStrictEqual(yield* events(1), ['! #1 ext in Host stopped sending events: the host quit, they resume once it links again']);
        }, Effect.scoped),
    );

    it.live(
        'answers a key the strict plugin tool does not take with the refusal the plugin words, as an error result',
        Effect.fnUntraced(function* () {
            const { client, handle } = yield* linked();
            const refused = yield* client.callTool({ name: 'unique', arguments: { handle, extra: 'kept' } });
            assert.deepStrictEqual([refused.isError, lines(refused)], [true, ['extra: not a parameter, this takes none']]);
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
