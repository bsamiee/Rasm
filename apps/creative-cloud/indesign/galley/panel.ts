import './runtime.ts';
import { host } from 'adobe:uxp';
import { type Link, serve } from '@rasm/ai/bridge';
import { Array, Effect, Layer, Option, Schema, Semaphore } from 'effect';
import { Socket } from 'effect/socket';
import { PATH, Panel, type Peer } from './contract.ts';
import { geometry, planned } from './document.ts';
import { Model, Temporary } from './dom.ts';
import { enter, execute, scripted } from './execute.ts';
import { find, queries } from './find.ts';
import { face, subset } from './fonts.ts';
import { HostClock } from './runtime.ts';
import { inspect, menu, preflight } from './state.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const STYLE = [
    'body { padding: 12px; margin: 0; font-family: adobe-clean, sans-serif; font-size: 12px; line-height: 1.4; }',
    'p { margin: 0 0 8px; }',
    '[data-state="closed"], [data-state="unreachable"], [data-state="done"] { opacity: 0.6; }',
    '[data-state="running"] { font-weight: bold; }',
    '@media (prefers-color-scheme: dark), (prefers-color-scheme: darkest) { body { color: rgb(232 232 232); } }',
    '@media (prefers-color-scheme: light), (prefers-color-scheme: lightest) { body { color: rgb(50 50 50); } }',
].join('\n');

// --- [MODELS] --------------------------------------------------------------------------

const Origin = Schema.TemplateLiteral(['ws://localhost:', Schema.Int]);

// --- [COMPOSITION] ---------------------------------------------------------------------

const [status, activity] = [document.createElement('p'), document.createElement('p')];
const show = (element: HTMLElement, state: string, text: string): Effect.Effect<void> =>
    Effect.sync(() => {
        element.setAttribute('data-state', state);
        element.replaceChildren(text);
    });
const working =
    <P extends { readonly description: string }, A, E, R>(handler: (request: P) => Effect.Effect<A, E, R>) =>
    (request: P): Effect.Effect<A, E, R> =>
        Effect.acquireUseRelease(
            show(activity, 'running', request.description),
            () => handler(request),
            () => show(activity, 'done', request.description),
        );
document.head.append(Object.assign(document.createElement('style'), { textContent: STYLE }));
document.body.append(status, activity);
Effect.gen(function* () {
    const origin = yield* Schema.decodeUnknownEffect(Origin)(import.meta.env.ATELIER);
    const sentence = { linked: 'Linked to Atelier', closed: 'Atelier closed, redialing', unreachable: `No Atelier at ${origin}, redialing` } as const satisfies Record<Link, string>;
    const warm = Effect.catchCause(Effect.asVoid(Effect.provideService(inspect({ target: Option.some('Application'), fields: Option.none() }), Model, yield* Model)), Effect.logError);
    const turn = yield* Semaphore.make(1);
    const queued =
        <P, A, E, R>(handler: (request: P) => Effect.Effect<A, E, R>) =>
        (request: P): Effect.Effect<A, E, R> =>
            Semaphore.withPermits(turn, 1)(handler(request));
    yield* serve(Panel, {
        url: `${origin}${PATH}`,
        identity: { version: host.version } satisfies typeof Peer.Encoded,
        concurrency: 'unbounded',
        link: (link) => Effect.all([show(status, link, sentence[link]), show(activity, link, ''), link === 'linked' ? warm : Effect.void], { discard: true }),
    }).pipe(
        Effect.provide(
            Panel.toLayer({
                Enter: () => enter,
                Execute: queued(working(execute)),
                Render: queued((request) => Effect.flatMap(planned(request), (job) => Effect.map(scripted({ description: 'Rendering', preview: Option.some(true) }, job), ([shot]) => shot))),
                Inspect: queued((request) => Effect.flatMap(inspect(request), geometry)),
                Face: queued(face),
                Subset: queued(subset),
                Find: queued(
                    working((request) => {
                        const shown = 200;
                        return Effect.map(
                            Effect.flatMap(find(request, Option.some(shown)), (job) => scripted(request, job)),
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
}).pipe(Layer.effectDiscard, Layer.provide([Socket.layerWebSocketConstructorGlobal, Temporary.layer, Model.layer]), Layer.launch, Effect.provide(HostClock), Effect.runFork);
