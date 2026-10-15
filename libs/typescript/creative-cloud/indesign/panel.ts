import { host } from 'adobe:uxp';
import { type Link, serve } from '@rasm/mcp/bridge';
import { Temporary } from '@rasm/uxp/runtime';
import { Array, Effect, Layer, Option, Semaphore } from 'effect';
import { Socket } from 'effect/socket';
import { PATH, Panel, type Peer } from './contract.ts';
import { geometry, planned } from './document.ts';
import { Model } from './dom.ts';
import { enter, execute, scripted } from './execute.ts';
import { find, queries } from './find.ts';
import { face, subset } from './fonts.ts';
import { inspect, menu, preflight } from './state.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const served = Effect.fnUntraced(
    function* (origin: string, { link, working }: { readonly link: (link: Link) => Effect.Effect<void>; readonly working: <A, E, R>(description: string, work: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R> }) {
        const warm = Effect.catchCause(Effect.asVoid(Effect.provideService(inspect({ target: Option.some('Application'), fields: Option.none() }), Model, yield* Model)), Effect.logError);
        const turn = yield* Semaphore.make(1);
        const queued =
            <P, A, E, R>(handler: (request: P) => Effect.Effect<A, E, R>) =>
            (request: P): Effect.Effect<A, E, R> =>
                Semaphore.withPermits(turn, 1)(handler(request));
        const shown =
            <P extends { readonly description: string }, A, E, R>(handler: (request: P) => Effect.Effect<A, E, R>) =>
            (request: P): Effect.Effect<A, E, R> =>
                working(request.description, handler(request));
        yield* serve(Panel, {
            url: `${origin}${PATH}`,
            identity: { version: host.version } satisfies typeof Peer.Encoded,
            concurrency: 'unbounded',
            link: (state) => Effect.andThen(link(state), state === 'linked' ? warm : Effect.void),
        }).pipe(
            Effect.provide(
                Panel.toLayer({
                    Enter: () => enter,
                    Execute: queued(shown(execute)),
                    Render: queued((request) => Effect.flatMap(planned(request), (job) => Effect.map(scripted({ description: 'Rendering', preview: Option.some(true) }, job), ([shot]) => shot))),
                    Inspect: queued((request) => Effect.flatMap(inspect(request), geometry)),
                    Face: queued(face),
                    Subset: queued(subset),
                    Find: queued(
                        shown((request) => {
                            const listed = 200;
                            return Effect.map(
                                Effect.flatMap(find(request, Option.some(listed)), (job) => scripted(request, job)),
                                ([found, corrections, preview]) => ({ ...found, corrections: Option.liftPredicate(corrections, Array.isReadonlyArrayNonEmpty), preview }),
                            );
                        }),
                    ),
                    Menu: queued(menu),
                    Preflight: queued(preflight),
                    Queries: queued(() =>
                        Effect.map(
                            Effect.flatMap(queries(), (job) => scripted({ description: 'Reading saved queries', preview: Option.some(true) }, job)),
                            ([rows]) => rows,
                        ),
                    ),
                }),
            ),
        );
    },
    Layer.effectDiscard,
    Layer.provide([Socket.layerWebSocketConstructorGlobal, Temporary.layer, Model.layer]),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { served };
