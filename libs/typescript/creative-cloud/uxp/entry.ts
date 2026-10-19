import fs from 'adobe:fs';
import uxp, { entrypoints } from 'adobe:uxp';
import { Cause, Clock, Duration, Effect, Exit, Layer, ManagedRuntime, Predicate, Record, Schema } from 'effect';
import { Manifest } from './manifest.ts';
import { Plugin, Uxp } from './runtime.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const hosted = Layer.merge(
    Layer.effect(Clock.Clock)(
        Clock.clockWith((base) => {
            const monotonicTimeNanosUnsafe = (): bigint => Duration.toNanosUnsafe(performance.now());
            const currentTimeNanosUnsafe = (): bigint => Duration.toNanosUnsafe(base.currentTimeMillisUnsafe());
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
    Layer.succeed(Uxp)({
        uxp,
        fs,
        temporary: (name) =>
            Effect.acquireRelease(
                Effect.flatMap(Effect.all([Effect.fiberId, Effect.tryPromise(() => uxp.storage.localFileSystem.getTemporaryFolder())]), ([id, folder]) => Effect.tryPromise(() => folder.createFile(`${id}-${name}`, { overwrite: true }))),
                (file) => Effect.promise(() => file.delete()),
            ),
    }),
);
const plugin = <R, E, F, C extends string, P extends string>(
    layer: Layer.Layer<R, E, Uxp | Plugin>,
    entries: {
        readonly commands: Record.ReadonlyRecord<C, Effect.Effect<void, F, R | Uxp | Plugin>>;
        readonly panels: Record.ReadonlyRecord<P, Record.ReadonlyRecord<'show' | 'hide', Effect.Effect<void, F, R | Uxp | Plugin>>>;
    },
): void => {
    const runtime = ManagedRuntime.make(Layer.provideMerge(layer, Effect.try(() => fs.readFileSync('plugin:/manifest.json', 'utf-8')).pipe(Effect.flatMap(Schema.decodeEffect(Manifest.json)), Layer.effect(Plugin), Layer.merge(hosted))));
    const run = (work: Effect.Effect<void, F, R | Uxp | Plugin>) => (): Promise<void> => runtime.runPromise(Effect.tapCause(work, (cause) => Effect.sync(() => document.body.append(Object.assign(document.createElement('sp-body'), { textContent: Cause.pretty(cause) })))));
    const handlers = {
        plugin: { create: (): Promise<void> => runtime.runPromise(Effect.void), destroy: (): Promise<void> => runtime.dispose() },
        commands: Record.map(entries.commands, run),
        panels: Record.map(entries.panels, Record.map(run)),
    } satisfies Pick<Parameters<typeof entrypoints.setup>[0], 'plugin' | 'commands' | 'panels'>;
    Reflect.apply(entrypoints.setup, entrypoints, [handlers]);
};
const script = <A, E>(work: (args: readonly string[]) => Effect.Effect<A, E, Uxp>): void => {
    const target: unknown = Reflect.get(uxp, 'script');
    Effect.gen(function* () {
        const { args, setResult } = yield* Schema.decodeUnknownEffect(Schema.Struct({ args: Schema.Array(Schema.String), setResult: Schema.declare(Predicate.isFunction) }))(target);
        const exit = yield* Effect.exit(work(args));
        Reflect.apply(setResult, target, [yield* Schema.encodeUnknownEffect(Schema.fromJsonString(Schema.Unknown))(Exit.match(exit, { onSuccess: (value) => ({ value }), onFailure: (cause) => ({ failure: Cause.pretty(cause) }) }))]);
    }).pipe(Effect.provide(hosted), Effect.runSync);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { plugin, script };
