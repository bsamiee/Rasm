import { entrypoints } from 'adobe:uxp';
import { Clock, Duration, Effect, Layer, ManagedRuntime } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

const launch = <E>(layer: Layer.Layer<never, E>): void => {
    const runtime = ManagedRuntime.make(
        layer.pipe(
            Layer.provide(
                Layer.effect(Clock.Clock)(
                    Clock.clockWith((base) => {
                        const monotonicTimeNanosUnsafe = (): bigint => Duration.toNanosUnsafe(performance.now());
                        const start = Duration.toNanosUnsafe(base.currentTimeMillisUnsafe()) - monotonicTimeNanosUnsafe();
                        const currentTimeNanosUnsafe = (): bigint => start + monotonicTimeNanosUnsafe();
                        return Effect.succeed({
                            currentTimeMillisUnsafe: base.currentTimeMillisUnsafe.bind(base),
                            currentTimeMillis: base.currentTimeMillis,
                            sleep: base.sleep.bind(base),
                            currentTimeNanosUnsafe,
                            currentTimeNanos: Effect.sync(currentTimeNanosUnsafe),
                            monotonicTimeNanosUnsafe,
                            monotonicTimeNanos: Effect.sync(monotonicTimeNanosUnsafe),
                        });
                    }),
                ),
            ),
        ),
    );
    const handlers = { plugin: { create: (): Promise<void> => runtime.runPromise(Effect.void), destroy: (): Promise<void> => runtime.dispose() } } satisfies Pick<Parameters<typeof entrypoints.setup>[0], 'plugin'>;
    Reflect.apply(entrypoints.setup, entrypoints, [handlers]);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { launch };
