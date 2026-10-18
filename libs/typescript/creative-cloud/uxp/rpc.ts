import { type Link, serve } from '@rasm/mcp/link';
import { Activity } from '@rasm/mcp/server';
import { Effect, Layer, Option, Schema, Stream } from 'effect';
import type { UrlParams } from 'effect/http';
import { type Rpc, type RpcGroup, RpcMiddleware } from 'effect/rpc';
import { Socket } from 'effect/socket';

// --- [SERVICES] ------------------------------------------------------------------------

class Working extends RpcMiddleware.Service<Working>()('@rasm/uxp/rpc/Working') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const linked = <Rpcs extends Rpc.Any, E, R>(group: RpcGroup.RpcGroup<Rpcs>, { peer, url, identity }: { readonly peer: string; readonly url: string; readonly identity: Effect.Effect<UrlParams.Input, E, R> }) =>
    Layer.effectDiscard(
        Effect.gen(function* () {
            const panel = yield* Effect.acquireRelease(
                Effect.sync(() => {
                    const element = document.createElement('div');
                    document.body.append(element);
                    return element;
                }),
                (element) => Effect.sync(() => element.remove()),
            );
            const [status, activity] = [document.createElement('sp-body'), document.createElement('sp-label')];
            panel.append(status, activity);
            const show = (element: HTMLElement, text: string) => Effect.sync(() => element.replaceChildren(text));
            yield* serve(group.middleware(Working), { url, identity: yield* identity, concurrency: 1 }).pipe(
                Stream.runForEach((state) => Effect.andThen(show(status, ({ linked: `Linked to ${peer}`, closed: `${peer} closed, redialing`, unreachable: `No ${peer} at ${url}, redialing` } satisfies Record<Link, string>)[state]), show(activity, ''))),
                Effect.provideService(Working, (effect, { payload }) =>
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
                Effect.forkScoped,
            );
        }),
    ).pipe(Layer.provide(Socket.layerWebSocketConstructorGlobal));

// --- [EXPORTS] -------------------------------------------------------------------------

export { linked };
