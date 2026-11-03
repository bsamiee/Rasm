import fs from 'adobe:fs';
import uxp, { entrypoints } from 'adobe:uxp';
import { Cause, Context, Effect, Fiber, FiberHandle, FiberSet, flow, Layer, ManagedRuntime, type Path, PlatformError, Predicate, Record, Schema, Scope, ScopedRef } from 'effect';
import { Manifest } from '#/manifest';
import { Uxp } from '#/runtime';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = `plugin:/${Manifest.file}`;

// --- [SERVICES] ------------------------------------------------------------------------

class Plugin extends Context.Service<Plugin, Manifest>()('@rasm/uxp/plugin') {
    static readonly serve = <E, F>({
        link,
        commands = {},
        panels = {},
    }: {
        readonly link?: (extension: string) => Layer.Layer<never, E, Uxp | Plugin | Path.Path>;
        readonly commands?: Record.ReadonlyRecord<string, Effect.Effect<void, F, Uxp | Plugin>>;
        readonly panels?: Record.ReadonlyRecord<string, (root: HTMLElement, fork: <A, X>(work: Effect.Effect<A, X, Uxp | Plugin>) => void) => Effect.Effect<void, F, Scope.Scope | Uxp | Plugin>>;
    }): void => {
        const reported = <A, X, R>(effect: Effect.Effect<A, X, R>) => effect.pipe(Effect.tapCauseIf(Predicate.not(Cause.hasInterruptsOnly<X>), Uxp.logged));
        Effect.gen(function* () {
            const [manifest, { setup }] = yield* Effect.all([
                Effect.try({ try: () => fs.readFileSync(MANIFEST, 'utf-8'), catch: (cause) => PlatformError.systemError({ _tag: 'Unknown', module: 'FileSystem', method: 'readFileString', pathOrDescriptor: MANIFEST, cause }) }).pipe(Effect.flatMap(Schema.decodeEffect(Manifest.json))),
                Schema.decodeEffect(Schema.Struct({ setup: Schema.declare(Predicate.isFunction) }))(entrypoints),
            ]);
            const linked = link === undefined ? Layer.empty : Layer.launch(link(manifest.id)).pipe(reported, Effect.forkScoped, Layer.effectDiscard);
            const runtime = ManagedRuntime.make(linked.pipe(Layer.provideMerge(Layer.succeed(Plugin)(manifest)), Layer.provideMerge(Uxp.layer({ uxp, fs }))));
            const run = <X>(effect: Effect.Effect<unknown, X, Uxp | Plugin>): Promise<void> => runtime.runPromise(Effect.asVoid(reported(effect)));
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
                Effect.all(
                    Record.map(
                        panels,
                        Effect.fnUntraced(function* (panel) {
                            const held = yield* ScopedRef.fromAcquire(Effect.void);
                            const fork = (element: HTMLElement) =>
                                FiberSet.makeRuntime<Uxp | Plugin, void, never>().pipe(
                                    Effect.flatMap((forked) => panel(element, flow(reported, Effect.ignore(), forked))),
                                    reported,
                                    Effect.forkScoped,
                                    Effect.asVoid,
                                );
                            return {
                                show: (root?: unknown): Promise<void> => run(Effect.flatMap(Schema.decodeUnknownEffect(Schema.instanceOf(HTMLElement))(root), (element) => ScopedRef.set(held, fork(element)))),
                                hide: (): Promise<void> => run(ScopedRef.set(held, Effect.void)),
                            };
                        }),
                    ),
                ),
            ]).pipe(Scope.provide(runtime.scope));
            yield* Effect.sync(() => setup.call(entrypoints, { plugin: { create: () => runtime.runPromise(Effect.void), destroy: () => runtime.dispose() }, commands: handled, panels: shown } satisfies Pick<Parameters<typeof entrypoints.setup>[0], 'plugin' | 'commands' | 'panels'>));
        }).pipe(Effect.tapCause(Uxp.logged), Effect.runFork);
    };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Plugin };
