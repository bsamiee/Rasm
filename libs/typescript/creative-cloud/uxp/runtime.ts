import { host } from 'adobe:uxp';
import { type Link, serve } from '@rasm/mcp/link';
import { Activity } from '@rasm/mcp/server';
import { Clock, Context, Duration, Effect, Layer, Option, Schema, Stream } from 'effect';
import { type Rpc, type RpcGroup, RpcMiddleware } from 'effect/rpc';
import { Socket } from 'effect/socket';

// --- [SERVICES] ------------------------------------------------------------------------

class Panel extends Context.Service<Panel, { readonly origin: string; readonly link: (state: Link) => Effect.Effect<void> }>()('@rasm/uxp/runtime/Panel') {}
class Working extends RpcMiddleware.Service<Working>()('@rasm/uxp/runtime/Working') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const linked = <Rpcs extends Rpc.Any, E, R>(group: RpcGroup.RpcGroup<Rpcs>, path: string, handlers: Layer.Layer<Rpc.ToHandler<Rpcs> | Rpc.Middleware<Rpcs>, E, R>) =>
    Layer.effectDiscard(Effect.flatMap(Panel, ({ origin, link }) => serve(group.middleware(Working), { url: `${origin}${path}`, identity: { version: host.version }, concurrency: 1 }).pipe(Stream.runForEach(link), Effect.forkScoped))).pipe(Layer.provide(handlers));
const launch = <E>(layer: Layer.Layer<never, E, Panel | Working | Socket.WebSocketConstructor>, { peer, origin }: { readonly peer: string; readonly origin: string }): void => {
    const [status, activity] = [document.createElement('sp-body'), document.createElement('sp-label')];
    const show = (element: HTMLElement, text: string): Effect.Effect<void> => Effect.sync(() => element.replaceChildren(text));
    document.body.append(status, activity);
    Layer.launch(layer).pipe(
        Effect.provide([
            Layer.succeed(Panel)({
                origin,
                link: (state) => Effect.andThen(show(status, ({ linked: `Linked to ${peer}`, closed: `${peer} closed, redialing`, unreachable: `No ${peer} at ${origin}, redialing` } satisfies Record<Link, string>)[state]), show(activity, '')),
            }),
            Layer.succeed(Working)((effect, { payload }) =>
                Option.match(Schema.decodeUnknownOption(Activity)(payload), {
                    onNone: () => effect,
                    onSome: ({ description }) =>
                        Effect.acquireUseRelease(
                            show(activity, description),
                            () => effect,
                            () => show(activity, ''),
                        ),
                }),
            ),
            Socket.layerWebSocketConstructorGlobal,
            Layer.effect(Clock.Clock)(
                Clock.clockWith((base) => {
                    const monotonicTimeNanosUnsafe = (): bigint => Duration.toNanosUnsafe(performance.now());
                    const start = Duration.toNanosUnsafe(base.currentTimeMillisUnsafe()) - monotonicTimeNanosUnsafe();
                    const currentTimeNanosUnsafe = (): bigint => start + monotonicTimeNanosUnsafe();
                    return Effect.succeed({
                        currentTimeMillisUnsafe: () => base.currentTimeMillisUnsafe(),
                        currentTimeMillis: base.currentTimeMillis,
                        currentTimeNanosUnsafe,
                        currentTimeNanos: Effect.sync(currentTimeNanosUnsafe),
                        monotonicTimeNanosUnsafe,
                        monotonicTimeNanos: Effect.sync(monotonicTimeNanosUnsafe),
                        sleep: (duration: Duration.Duration) => base.sleep(duration),
                    });
                }),
            ),
        ]),
        Effect.runFork,
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { launch, linked };
