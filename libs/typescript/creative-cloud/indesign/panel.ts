import { host } from 'adobe:uxp';
import { type Link, serve } from '@rasm/mcp/bridge';
import { Temporary } from '@rasm/uxp/runtime';
import { Effect, Layer, Option, type Scope, Semaphore, Struct } from 'effect';
import { Socket } from 'effect/socket';
import { PATH, Panel, type Peer } from './contract.ts';
import { geometry, planned } from './document.ts';
import { Model, scope } from './dom.ts';
import { enter, execute, scripted } from './execute.ts';
import { find, queries } from './find.ts';
import { face, subset } from './fonts.ts';
import { inspect, menu, preflight } from './state.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const served = Effect.fnUntraced(
    function* (origin: string, { link, working }: { readonly link: (link: Link) => Effect.Effect<void>; readonly working: <A, E, R>(description: string, work: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R> }) {
        const turn = yield* Semaphore.make(1);
        const queued =
            <P, A, E, R>(handler: (request: P) => Effect.Effect<A, E, R>) =>
            (request: P): Effect.Effect<A, E, Exclude<R, Scope.Scope>> =>
                Semaphore.withPermits(turn, 1)(Effect.scoped(Effect.andThen(scope, handler(request))));
        const warm = Effect.catchCause(Effect.asVoid(Effect.provideService(queued(inspect)({ target: Option.some('Application'), fields: Option.none() }), Model, yield* Model)), Effect.logError);
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
                    Render: queued((request) => Effect.map(scripted({ description: 'Rendering', preview: Option.some(true) }, planned(request)), Struct.get('result'))),
                    Inspect: queued((request) => Effect.flatMap(inspect(request), geometry)),
                    Face: queued(face),
                    Subset: queued(subset),
                    Find: queued(
                        shown((request) => {
                            const listed = 200;
                            return Effect.map(scripted(request, find(request, Option.some(listed))), ({ result, ...outcome }) => ({ ...result, ...outcome }));
                        }),
                    ),
                    Menu: queued(menu),
                    Preflight: queued(preflight),
                    Queries: queued(() => Effect.map(scripted({ description: 'Reading saved queries', preview: Option.some(true) }, queries()), Struct.get('result'))),
                }),
            ),
        );
    },
    Layer.effectDiscard,
    Layer.provide([Socket.layerWebSocketConstructorGlobal, Temporary.layer, Model.layer]),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { served };
