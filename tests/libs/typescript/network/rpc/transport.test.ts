import { once } from 'node:events';
import { createServer, request } from 'node:http';
import { layer as served } from '@effect/platform-node/NodeHttpServer';
import { assert, describe, it } from '@effect/vitest';
import { dial, listen } from '@rasm/rpc';
import { Array, Cause, Context, Deferred, Duration, Effect, Exit, Fiber, Function, Layer, MutableRef, Number, Option, Queue, Ref, Schema, Scope, Stream, Struct } from 'effect';
import { HttpRouter, HttpServer } from 'effect/http';
import { Rpc, RpcGroup } from 'effect/rpc';
import { Socket } from 'effect/socket';
import { TestClock } from 'effect/testing';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ORIGIN = 'http://suite.test';
const PATH = '/link';
const REDIAL = Duration.millis(1);
const ROUND_TRIPS = 5;

// --- [MODELS] --------------------------------------------------------------------------

class Peer extends Schema.Opaque<Peer>()(Schema.Struct({ name: Schema.String, instance: Schema.String })) {}
const Asked = RpcGroup.make(Rpc.make('instance', { success: Schema.String }), Rpc.make('notices', { success: Schema.String, stream: true }), Rpc.make('crash'));
const Bound = Schema.Struct({ port: Schema.Int });
const Closed = Schema.Tuple([Schema.Struct({ code: Schema.Int })]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const linked = listen(Asked, { path: PATH, origin: ORIGIN, peer: Peer, key: Struct.get('name') });
const listening = Effect.fnUntraced(function* (port: number) {
    const ready = yield* Deferred.make<Effect.Success<typeof linked>>();
    const routes = Layer.effectDiscard(Effect.flatMap(linked, (links) => Deferred.succeed(ready, links)));
    const services = yield* Layer.build(Layer.provideMerge(HttpRouter.serve(routes, { disableLogger: true, disableListenLog: true }), served(createServer, { port })));
    const bound = yield* Schema.decodeUnknownEffect(Bound)(Context.get(services, HttpServer.HttpServer).address);
    return { links: yield* Deferred.await(ready), port: bound.port };
});
const dialing = Effect.fnUntraced(function* (port: number, instance: string, identity: Effect.Effect<Peer, Cause.UnknownError> = Effect.succeed({ name: 'extension', instance })) {
    const [dials, read, opened, notices, closes] = [yield* Ref.make(0), yield* Deferred.make<void>(), yield* Deferred.make<void>(), yield* Queue.unbounded<string>(), MutableRef.make<readonly Promise<unknown>[]>([])];
    const sockets = Layer.succeed(Socket.WebSocketConstructor)((url) => {
        const socket = new globalThis.WebSocket(url, { headers: { origin: ORIGIN } });
        MutableRef.update(closes, Array.append(once(socket, 'close')));
        return socket;
    });
    const peer = Ref.update(dials, Number.increment).pipe(Effect.andThen(Deferred.succeed(read, undefined)), Effect.andThen(identity));
    const handlers = Asked.toLayer({ instance: () => Effect.succeed(instance), notices: () => Rpc.fork(Deferred.succeed(opened, undefined).pipe(Effect.as(Stream.fromQueue(notices)), Stream.unwrap)), crash: () => Effect.die('handler died') });
    yield* Layer.build(dial(Asked, handlers, { url: new URL(PATH, `ws://127.0.0.1:${port}`), redial: REDIAL, peer }).pipe(Layer.provide(sockets)));
    const closed = Effect.promise(() => Promise.all(MutableRef.get(closes))).pipe(Effect.flatMap(Effect.forEach((event) => Schema.decodeUnknownEffect(Closed)(event))), Effect.map(Array.map(([{ code }]) => code)));
    return { dials: Ref.get(dials), read: Deferred.await(read), opened: Deferred.await(opened), notices, closed };
});
const upgraded = (port: number, origin: Readonly<Record<string, string>>) =>
    Effect.callback<number | undefined>((resume) => {
        const handshake = { connection: 'Upgrade', upgrade: 'websocket', 'sec-websocket-version': '13', 'sec-websocket-key': 'dGhlIHNhbXBsZSBub25jZQ==', ...origin };
        const sent = request({ host: '127.0.0.1', port, path: `${PATH}?name=extension&instance=probe`, headers: handshake });
        sent.once('response', (response) => resume(Effect.succeed(response.resume().statusCode)));
        sent.once('upgrade', (response, socket) => resume(Effect.sync(() => socket.destroy()).pipe(Effect.as(response.statusCode))));
        sent.once('error', (cause) => resume(Effect.die(cause)));
        sent.end();
        return Effect.sync(() => sent.destroy());
    }).pipe(Effect.timeoutOrElse({ duration: '5 seconds', orElse: () => Effect.fail(new Cause.UnknownError(origin, 'the WebSocket upgrade drew no answer within 5 seconds')) }));
const asked = (links: Effect.Success<typeof linked>) => links.call(Function.constTrue, 'peer').pipe(Effect.flatMap((client) => client('instance', undefined)));
const answering = (links: Effect.Success<typeof linked>, instance: string) =>
    links.changes.pipe(
        Stream.mapEffect(() => Effect.option(asked(links))),
        Stream.filter(Option.contains(instance)),
        Stream.runHead,
        Effect.timeoutOrElse({ duration: '5 seconds', orElse: () => Effect.fail(new Cause.UnknownError(instance, `peer ${instance} never answered over the link within 5 seconds`)) }),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('link', () => {
    it.live(
        'closes the socket of a peer a newer peer of its key replaces with private code 4000, and the replaced peer dials no more',
        Effect.fnUntraced(function* () {
            const { links, port } = yield* listening(0);
            const first = yield* dialing(port, 'first');
            yield* answering(links, 'first');
            const second = yield* dialing(port, 'second');
            yield* answering(links, 'second');
            const codes = yield* Effect.timeoutOrElse(first.closed, { duration: '5 seconds', orElse: () => Effect.fail(new Cause.UnknownError(undefined, 'socket of the replaced peer stays open')) });
            const answers = yield* Effect.forEach(Array.range(1, ROUND_TRIPS), () => asked(links));
            assert.deepStrictEqual([codes, answers, yield* first.dials, yield* second.dials, yield* links.peers], [[4000], Array.replicate('second', ROUND_TRIPS), 1, 1, [{ name: 'extension', instance: 'second' }]]);
        }),
    );

    it.live(
        'dials again, reading its identity again, after its server closes and another listens on the port',
        Effect.fnUntraced(function* () {
            const first = yield* Effect.flatMap(Effect.scope, Scope.fork);
            const { links, port } = yield* Scope.provide(listening(0), first);
            const peer = yield* dialing(port, 'kept');
            yield* answering(links, 'kept');
            yield* Scope.close(first, Exit.void);
            const second = yield* listening(port);
            const answered = yield* answering(second.links, 'kept');
            assert.deepStrictEqual([answered, Number.isGreaterThan(yield* peer.dials, 1)], [Option.some(Option.some('kept')), true]);
        }),
    );

    it.live(
        'refuses a WebSocket upgrade from a foreign or absent origin with 403, and upgrades one from its own origin',
        Effect.fnUntraced(function* () {
            const { port } = yield* listening(0);
            const statuses = yield* Effect.all({ foreign: upgraded(port, { origin: 'http://foreign.test' }), absent: upgraded(port, {}), own: upgraded(port, { origin: ORIGIN }) });
            assert.deepStrictEqual(statuses, { foreign: 403, absent: 403, own: 101 });
        }),
    );

    it.live(
        'keeps a notice stream of a link open when another call on that link dies',
        Effect.fnUntraced(function* () {
            const { links, port } = yield* listening(0);
            const peer = yield* dialing(port, 'notified');
            yield* answering(links, 'notified');
            const client = yield* links.call(Function.constTrue, 'peer');
            const heard = yield* client('notices', undefined).pipe(Stream.take(1), Stream.runCollect, Effect.forkScoped);
            yield* Effect.timeoutOrElse(peer.opened, { duration: '5 seconds', orElse: () => Effect.fail(new Cause.UnknownError(undefined, 'the notice stream never opened on the dialing peer')) });
            const died = yield* Effect.exit(client('crash', undefined));
            yield* Queue.offer(peer.notices, 'after');
            const notices = yield* Effect.timeoutOrElse(Fiber.join(heard), { duration: '5 seconds', orElse: () => Effect.fail(new Cause.UnknownError(undefined, 'the notice offered after the dead call never arrived')) });
            assert.deepStrictEqual([Exit.isFailure(died) && Cause.hasDies(died.cause), notices], [true, ['after']]);
        }),
    );

    it.effect(
        'reads a failing peer identity once and dials no more',
        Effect.fnUntraced(function* () {
            const peer = yield* dialing(1, 'broken', Effect.fail(new Cause.UnknownError(undefined, 'host module absent')));
            yield* peer.read;
            yield* TestClock.adjust(Duration.times(REDIAL, 1000));
            assert.strictEqual(yield* peer.dials, 1);
        }),
    );
});
