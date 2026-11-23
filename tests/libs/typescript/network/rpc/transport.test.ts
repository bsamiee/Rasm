import { once } from 'node:events';
import { createServer } from 'node:http';
import { layer as served } from '@effect/platform-node/NodeHttpServer';
import { assert, describe, it } from '@effect/vitest';
import { dial, listen } from '@rasm/rpc';
import { Array, Cause, Context, Deferred, Duration, Effect, Exit, Layer, MutableRef, Number, Option, Ref, Schema, Scope, Stream, Struct } from 'effect';
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

class Peer extends Schema.Opaque<Peer, unknown>()(Schema.Struct({ name: Schema.String, instance: Schema.String })) {}
const Asked = RpcGroup.make(Rpc.make('instance', { success: Schema.String }));
const Bound = Schema.Struct({ port: Schema.Int });
const Closed = Schema.Tuple([Schema.Struct({ code: Schema.Int })]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const linked = listen(Asked, { path: PATH, origin: ORIGIN, peer: Peer, key: Struct.get('name'), middleware: Layer.empty });
const listening = Effect.fnUntraced(function* (port: number) {
    const ready = yield* Deferred.make<Effect.Success<typeof linked>>();
    const routes = Layer.effectDiscard(Effect.flatMap(linked, (links) => Deferred.succeed(ready, links)));
    const services = yield* Layer.build(Layer.provideMerge(HttpRouter.serve(routes, { disableLogger: true, disableListenLog: true }), served(createServer, { port })));
    const bound = yield* Schema.decodeUnknownEffect(Bound)(Context.get(services, HttpServer.HttpServer).address);
    return { links: yield* Deferred.await(ready), port: bound.port };
});
const dialing = Effect.fnUntraced(function* (port: number, instance: string, identity: Effect.Effect<Peer, Cause.UnknownError> = Effect.succeed({ name: 'extension', instance })) {
    const [dials, read, closes] = [yield* Ref.make(0), yield* Deferred.make<void>(), MutableRef.make<readonly Promise<unknown>[]>([])];
    const sockets = Layer.succeed(Socket.WebSocketConstructor)((url) => {
        const socket = new globalThis.WebSocket(url, { headers: { origin: ORIGIN } });
        MutableRef.update(closes, Array.append(once(socket, 'close')));
        return socket;
    });
    const peer = Effect.andThen(Ref.update(dials, Number.increment), Effect.andThen(Deferred.succeed(read, undefined), identity));
    yield* Layer.build(dial(Asked, Asked.toLayer({ instance: () => Effect.succeed(instance) }), { url: new URL(PATH, `ws://127.0.0.1:${port}`), redial: REDIAL, peer }).pipe(Layer.provide(sockets)));
    const closed = Effect.promise(() => Promise.all(MutableRef.get(closes))).pipe(Effect.flatMap(Effect.forEach((event) => Schema.decodeUnknownEffect(Closed)(event))), Effect.map(Array.map(([{ code }]) => code)));
    return { dials: Ref.get(dials), read: Deferred.await(read), closed };
});
const asked = (links: Effect.Success<typeof linked>) =>
    Effect.flatMap(
        links.call(() => true, 'peer'),
        (client) => client('instance', undefined),
    );
const answering = (links: Effect.Success<typeof linked>, instance: string) =>
    Stream.runHead(
        Stream.filter(
            Stream.mapEffect(links.changes, () => Effect.option(asked(links))),
            Option.contains(instance),
        ),
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
        }, Effect.scoped),
        { timeout: 20_000 },
    );

    it.live(
        'dials again, reading its identity again, after its server closes and another listens on the port',
        Effect.fnUntraced(function* () {
            const first = yield* Scope.make();
            const { links, port } = yield* Scope.provide(listening(0), first);
            const peer = yield* dialing(port, 'kept');
            yield* answering(links, 'kept');
            yield* Scope.close(first, Exit.void);
            const second = yield* listening(port);
            const answered = yield* answering(second.links, 'kept');
            assert.deepStrictEqual([answered, Number.isGreaterThan(yield* peer.dials, 1)], [Option.some(Option.some('kept')), true]);
        }, Effect.scoped),
        { timeout: 20_000 },
    );

    it.effect(
        'reads a failing peer identity once and dials no more',
        Effect.fnUntraced(function* () {
            const peer = yield* dialing(1, 'broken', Effect.fail(new Cause.UnknownError(undefined, 'host module absent')));
            yield* peer.read;
            yield* TestClock.adjust(Duration.times(REDIAL, 1000));
            assert.strictEqual(yield* peer.dials, 1);
        }, Effect.scoped),
    );
});
