import fs from 'adobe:fs';
import uxp, { entrypoints } from 'adobe:uxp';
import { inkColor } from '@rasm/mark';
import { Cause, Context, Effect, Fiber, FiberHandle, FiberSet, Function, flow, Layer, ManagedRuntime, type Path, PlatformError, Predicate, Queue, Record, Schema, Scope, ScopedRef, Stream } from 'effect';
import { Uxp } from '#/uxp/uxp';
import { UxpManifest } from '#/uxp-manifest';

// --- [TYPES] ---------------------------------------------------------------------------

type Panel<F> = (root: HTMLElement, fork: <A, X>(work: Effect.Effect<A, X, Uxp | UxpPlugin>) => void) => Effect.Effect<void, F, Scope.Scope | Uxp | UxpPlugin>;

interface ThemeEvents {
    readonly getCurrent: () => unknown;
    readonly onUpdated: { readonly addListener: (listener: (theme: unknown) => void) => void; readonly removeListener: (listener: (theme: unknown) => void) => void };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = `plugin:/${UxpManifest.file}`;

// --- [MODELS] --------------------------------------------------------------------------

const Entrypoints = Schema.Struct({ setup: Schema.declare(Predicate.isFunction) });
const Themed = Schema.Struct({ theme: Schema.declare((held): held is ThemeEvents => Schema.is(Schema.Struct({ getCurrent: Schema.declare(Predicate.isFunction), onUpdated: Schema.Struct({ addListener: Schema.declare(Predicate.isFunction), removeListener: Schema.declare(Predicate.isFunction) }) }))(held)) });

// --- [SERVICES] ------------------------------------------------------------------------

class UxpPlugin extends Context.Service<UxpPlugin, UxpManifest>()('@rasm/uxp/uxp/uxp-plugin/UxpPlugin') {
    static readonly theme = Stream.callback<unknown, Schema.SchemaError>((queue) =>
        Effect.flatMap(
            Effect.suspend(() => Schema.decodeUnknownEffect(Themed)(document)),
            ({ theme }) => followed(theme, queue),
        ),
    ).pipe(Stream.mapEffect((theme) => Schema.decodeUnknownEffect(UxpManifest.Theme)(theme)));
    static readonly serve = <E, F>({
        link,
        commands = {},
        panels = {},
    }: {
        readonly link?: (extension: string) => Layer.Layer<never, E, Uxp | UxpPlugin | Path.Path>;
        readonly commands?: Record.ReadonlyRecord<string, Effect.Effect<void, F, Uxp | UxpPlugin>>;
        readonly panels?: Record.ReadonlyRecord<string, Panel<F>>;
    }): void => {
        const hosted = Uxp.layer({ uxp, fs });
        Effect.gen(function* () {
            const [manifest, { setup }] = yield* Effect.all([
                Effect.try({ try: () => fs.readFileSync(MANIFEST, 'utf-8'), catch: (cause) => PlatformError.systemError({ _tag: 'Unknown', module: 'FileSystem', method: 'readFileString', pathOrDescriptor: MANIFEST, cause }) }).pipe(Effect.flatMap(Schema.decodeEffect(UxpManifest.json))),
                Schema.decodeEffect(Entrypoints)(entrypoints),
            ]);
            const linked = link === undefined ? Layer.empty : Layer.launch(link(manifest.id)).pipe(reported, Effect.forkScoped, Layer.effectDiscard);
            const runtime = ManagedRuntime.make(linked.pipe(Layer.provideMerge(Layer.succeed(UxpPlugin)(manifest)), Layer.provideMerge(hosted)));
            const [handled, shown] = yield* Effect.all([
                Effect.all(
                    Record.map(
                        commands,
                        Effect.fnUntraced(function* (command) {
                            const running = yield* FiberHandle.make<void, F>();
                            return { run: () => runtime.runPromise(Effect.flatMap(FiberHandle.run(running, reported(command)), Fiber.join)), cancel: () => runtime.runPromise(FiberHandle.clear(running)) };
                        }),
                    ),
                ),
                Effect.all(Record.map(panels, (each) => panel(runtime, each))),
            ]).pipe(Scope.provide(runtime.scope));
            yield* Effect.sync(() => setup.call(entrypoints, { plugin: { create: () => runtime.runPromise(Effect.void), destroy: () => runtime.dispose() }, commands: handled, panels: shown } satisfies Pick<Parameters<typeof entrypoints.setup>[0], 'plugin' | 'commands' | 'panels'>));
        }).pipe(Effect.tapCause(Effect.logError), Effect.provide(hosted), Effect.runFork);
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const reported = <A, X, R>(effect: Effect.Effect<A, X, R>) => effect.pipe(Effect.tapCauseIf(Predicate.not(Cause.hasInterruptsOnly<X>), Effect.logError));
const followed = <E>(theme: ThemeEvents, queue: Queue.Queue<unknown, E>) => {
    const listener = (current: unknown): void => {
        Queue.offerUnsafe(queue, current);
    };
    return Effect.acquireRelease(
        Effect.sync(() => {
            theme.onUpdated.addListener(listener);
            listener(theme.getCurrent());
        }),
        Function.constant(Effect.sync(() => theme.onUpdated.removeListener(listener))),
    );
};
const inked = (root: HTMLElement) =>
    Stream.runForEach(UxpPlugin.theme, (theme) =>
        Effect.sync(() => {
            Object.assign(root.style, { color: inkColor(UxpManifest.inks[UxpManifest.themes[theme]]) });
        }),
    );
const panel = Effect.fnUntraced(function* <E, F>(runtime: ManagedRuntime.ManagedRuntime<Uxp | UxpPlugin, E>, body: Panel<F>) {
    const held = yield* ScopedRef.fromAcquire(Effect.void);
    const run = <X>(effect: Effect.Effect<unknown, X, Uxp | UxpPlugin>): Promise<void> => runtime.runPromise(Effect.asVoid(reported(effect)));
    const fork = (element: HTMLElement) =>
        FiberSet.makeRuntime<Uxp | UxpPlugin, void, never>().pipe(
            Effect.flatMap((forked) => Effect.andThen(Effect.forkScoped(reported(inked(element))), body(element, flow(reported, Effect.ignore(), forked)))),
            reported,
            Effect.forkScoped,
            Effect.asVoid,
        );
    return {
        show: (root?: unknown): Promise<void> => run(Effect.flatMap(Schema.decodeUnknownEffect(Schema.instanceOf(HTMLElement))(root), (element) => ScopedRef.set(held, fork(element)))),
        hide: (): Promise<void> => run(ScopedRef.set(held, Effect.void)),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { UxpPlugin };
