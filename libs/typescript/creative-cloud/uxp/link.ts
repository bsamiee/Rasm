import { Context, Effect, Layer, Option, Schedule, Schema, Stream, SubscriptionRef } from 'effect';
import { UrlParams } from 'effect/http';
import { NetAddress } from 'effect/net';
import { type Rpc, type RpcGroup, type RpcSerialization, RpcServer } from 'effect/rpc';
import { Socket, SocketServer } from 'effect/socket';

// --- [MODELS] --------------------------------------------------------------------------

const State = Schema.TaggedUnion({ Dialing: {}, Linked: {}, Down: { cause: Socket.SocketError } });

// --- [SERVICES] ------------------------------------------------------------------------

class Link extends Context.Service<Link, { readonly state: SubscriptionRef.SubscriptionRef<typeof State.Type>; readonly call: SubscriptionRef.SubscriptionRef<Option.Option<string>> }>()('@rasm/uxp/link') {
    static readonly layer = Layer.effect(Link)(Effect.all({ state: SubscriptionRef.make<typeof State.Type>(State.cases.Dialing.make({})), call: SubscriptionRef.make(Option.none<string>()) }));
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const linked = <Rpcs extends Rpc.Any, H, EH, RH, E, R>(
    group: RpcGroup.RpcGroup<Rpcs>,
    handlers: Layer.Layer<H, EH, RH>,
    { peer, url, identity, serialization }: { readonly peer: string; readonly url: string; readonly identity: Effect.Effect<UrlParams.Input, E, R>; readonly serialization: Layer.Layer<RpcSerialization.RpcSerialization> },
) => {
    const dialed = Layer.effect(SocketServer.SocketServer)(
        Effect.gen(function* () {
            const [{ state }, query] = yield* Effect.all([Link, identity]);
            const { reader, writer } = yield* Socket.makeWebSocket(`${url}?${UrlParams.toString(UrlParams.fromInput(query))}`);
            const down = (cause: Socket.SocketError) => SubscriptionRef.set(state, State.cases.Down.make({ cause }));
            return {
                address: NetAddress.inetAddressUnsafe(NetAddress.ipv4Unspecified, 0),
                run: (handler) =>
                    reader.pipe(
                        Effect.tapError(down),
                        Effect.tap(() => SubscriptionRef.set(state, State.cases.Linked.make({}))),
                        Effect.flatMap(({ pull, upgrade }) => handler(Socket.make({ reader: Effect.succeed({ pull: Effect.tapError(pull, down), upgrade }), writer }))),
                        Effect.scoped,
                        Effect.retry(Schedule.spaced('2 seconds')),
                        Effect.forever,
                        Effect.mapError((cause) => new SocketServer.SocketServerError({ reason: new SocketServer.SocketServerUnknownError({ cause }) })),
                    ),
            };
        }),
    );
    const shown = Layer.effectDiscard(
        Effect.gen(function* () {
            const [{ state, call }, element] = yield* Effect.all([
                Link,
                Effect.acquireRelease(
                    Effect.sync(() => document.body.appendChild(document.createElement('div'))),
                    (node) => Effect.sync(() => node.remove()),
                ),
            ]);
            yield* SubscriptionRef.changes(state).pipe(
                Stream.zipLatestWith(SubscriptionRef.changes(call), (link, running) => [
                    Object.assign(document.createElement('sp-body'), { textContent: State.match(link, { Dialing: () => `Dialing ${peer} at ${url}`, Linked: () => `Linked to ${peer}`, Down: ({ cause }) => `No ${peer} at ${url} (${cause.message}), redialing` }) }),
                    ...Option.toArray(Option.map(running, (label) => Object.assign(document.createElement('sp-label'), { textContent: label }))),
                ]),
                Stream.runForEach((children) => Effect.sync(() => element.replaceChildren(...children))),
                Effect.forkScoped,
            );
        }),
    );
    return Layer.mergeAll(RpcServer.layer(group, { concurrency: 1 }).pipe(Layer.provide(handlers), Layer.provide(RpcServer.layerProtocolSocketServer), Layer.provide(dialed), Layer.provide(serialization)), shown).pipe(Layer.provideMerge(Link.layer), Layer.provide(Socket.layerWebSocketConstructorGlobal));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Link, linked };
