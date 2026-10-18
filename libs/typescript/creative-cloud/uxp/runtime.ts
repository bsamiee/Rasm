import { host, storage } from 'adobe:uxp';
import { Activity, Enter, type Link, serve } from '@rasm/mcp/link';
import { Clock, Context, Deferred, Duration, Effect, Layer, Option, Ref, Schema, type Scope } from 'effect';
import { Rpc, RpcGroup, RpcMiddleware } from 'effect/rpc';
import { Socket } from 'effect/socket';

// --- [SERVICES] ------------------------------------------------------------------------

class Panel extends Context.Service<
    Panel,
    {
        readonly origin: string;
        readonly link: (state: Link) => Effect.Effect<void>;
        readonly working: <A, E, R>(description: string, work: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>;
    }
>()('@rasm/uxp/runtime/Panel') {}
class Temporary extends Context.Service<Temporary, (name: string) => { readonly native: string; readonly url: string }>()('@rasm/uxp/runtime/Temporary') {
    static readonly layer = Layer.effect(this)(
        Effect.map(
            Effect.promise(() => storage.localFileSystem.getTemporaryFolder()),
            ({ nativePath }) =>
                (name: string) => ({ native: `${nativePath}${name}`, url: `plugin-temp:/${name}` }),
        ),
    );
}
class Handoff extends Context.Service<Handoff>()('@rasm/uxp/runtime/Handoff', {
    make: Effect.gen(function* () {
        const [entry, phase] = yield* Effect.all([Ref.make(Option.none<Effect.Effect<void>>()), Ref.make<typeof Enter.successSchema.Type>('Idle')]);
        return {
            enter: Effect.flatMap(Ref.getAndSet(entry, Option.none()), Option.match({ onNone: () => Ref.get(phase), onSome: Effect.as('Entered' as const) })),
            entered: Ref.set(phase, 'Entered'),
            handed: Effect.fnUntraced(
                function* <A, E, R, W>(wait: Effect.Effect<void, W>, step: Effect.Effect<A, E, R>) {
                    const [done, services] = yield* Effect.all([Deferred.make<A, E>(), Effect.context<R>()]);
                    yield* Ref.set(entry, Option.some(Effect.andThen(Ref.set(phase, 'Held'), Effect.asVoid(Deferred.into(Effect.provideContext(step, services), done)))));
                    yield* Ref.set(phase, 'Missed');
                    yield* wait.pipe(Effect.repeat({ until: () => Deferred.isDone(done) }));
                    return yield* Deferred.await(done);
                },
                Effect.ensuring(Effect.andThen(Ref.set(entry, Option.none()), Ref.set(phase, 'Idle'))),
            ),
        };
    }),
}) {
    static readonly layer = Layer.effect(this, this.make);
}
class Turn extends RpcMiddleware.Service<Turn>()('@rasm/uxp/runtime/Turn') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const linked = <Rpcs extends Rpc.Any, E, R>(group: RpcGroup.RpcGroup<Rpcs>, { path, hold }: { readonly path: string; readonly hold: Effect.Effect<void, never, Scope.Scope> }, handlers: Layer.Layer<Rpc.ToHandler<Exclude<Rpcs, typeof Enter>>, E, R>) =>
    Layer.effectDiscard(Effect.flatMap(Panel, ({ origin, link }) => serve(group.omit(Enter._tag).middleware(Turn).add(Enter), { url: `${origin}${path}`, identity: { version: host.version }, concurrency: 1, link }))).pipe(
        Layer.provide([
            handlers,
            RpcGroup.make(Enter).toLayer(Effect.map(Handoff, ({ enter }) => ({ enter: () => Rpc.fork(enter) }))),
            Layer.effect(Turn)(
                Effect.map(
                    Panel,
                    ({ working }) =>
                        (effect, { payload }) =>
                            Effect.andThen(hold, Option.match(Schema.decodeUnknownOption(Activity)(payload), { onNone: () => effect, onSome: ({ description }) => working(description, effect) })),
                ),
            ),
        ]),
    );
const launch = <E>(layer: Layer.Layer<never, E, Panel | Temporary | Handoff | Socket.WebSocketConstructor>, { peer, origin }: { readonly peer: string; readonly origin: string }): void => {
    const [style, status, activity] = [document.createElement('style'), document.createElement('p'), document.createElement('p')];
    const told = { linked: `Linked to ${peer}`, closed: `${peer} closed, redialing`, unreachable: `No ${peer} at ${origin}, redialing` } satisfies Record<Link, string>;
    const show = (element: HTMLElement, state: string, text: string): Effect.Effect<void> =>
        Effect.sync(() => {
            element.setAttribute('data-state', state);
            element.replaceChildren(text);
        });
    style.textContent = [
        'body { padding: 12px; margin: 0; font-family: adobe-clean, sans-serif; font-size: 12px; line-height: 1.4; }',
        'p { margin: 0 0 8px; }',
        '[data-state="closed"], [data-state="unreachable"], [data-state="done"] { opacity: 0.6; }',
        '[data-state="running"] { font-weight: bold; }',
        '@media (prefers-color-scheme: dark), (prefers-color-scheme: darkest) { body { color: rgb(232 232 232); } }',
        '@media (prefers-color-scheme: light), (prefers-color-scheme: lightest) { body { color: rgb(50 50 50); } }',
    ].join('\n');
    document.head.append(style);
    document.body.append(status, activity);
    Layer.launch(layer).pipe(
        Effect.provide([
            Layer.succeed(Panel)({
                origin,
                link: (state) => Effect.andThen(show(status, state, told[state]), show(activity, state, '')),
                working: (description, work) =>
                    Effect.acquireUseRelease(
                        show(activity, 'running', description),
                        () => work,
                        () => show(activity, 'done', description),
                    ),
            }),
            Temporary.layer,
            Handoff.layer,
            Socket.layerWebSocketConstructorGlobal,
        ]),
        Effect.provide(
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
        ),
        Effect.runFork,
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Handoff, launch, linked, Temporary };
