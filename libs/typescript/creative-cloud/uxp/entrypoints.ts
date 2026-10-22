import fs from 'adobe:fs';
import uxp, { entrypoints } from 'adobe:uxp';
import { Cause, Clock, Console, Duration, Effect, flow, Layer, ManagedRuntime, Option, Predicate, Record, Schema, Sink, Stdio, Stream } from 'effect';
import { RpcMessage, RpcSerialization, RpcServer } from 'effect/rpc';
import { Manifest } from './manifest.ts';
import { Plugin, Uxp } from './runtime.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const hosted = Layer.merge(
    Layer.effect(Clock.Clock)(
        Effect.map(Clock.Clock, (base) => {
            const monotonicTimeNanosUnsafe = (): bigint => Duration.toNanosUnsafe(performance.now());
            const currentTimeNanosUnsafe = (): bigint => Duration.toNanosUnsafe(base.currentTimeMillisUnsafe());
            return {
                currentTimeMillisUnsafe: base.currentTimeMillisUnsafe.bind(base),
                currentTimeMillis: base.currentTimeMillis,
                sleep: base.sleep.bind(base),
                currentTimeNanosUnsafe,
                currentTimeNanos: Effect.sync(currentTimeNanosUnsafe),
                monotonicTimeNanosUnsafe,
                monotonicTimeNanos: Effect.sync(monotonicTimeNanosUnsafe),
            };
        }),
    ),
    Layer.succeed(Uxp)({ uxp, fs }),
);
const plugin = <R, E, F>(
    layer: Layer.Layer<R, E, Uxp | Plugin>,
    entries: {
        readonly commands: Record.ReadonlyRecord<string, Effect.Effect<void, F, R | Uxp | Plugin>>;
        readonly panels: Record.ReadonlyRecord<string, Record.ReadonlyRecord<'show' | 'hide', Effect.Effect<void, F, R | Uxp | Plugin>>>;
    },
): void => {
    const logged = flow(Cause.pretty, Console.error);
    const runtime = ManagedRuntime.make(layer.pipe(Layer.provideMerge(Effect.try(() => fs.readFileSync('plugin:/manifest.json', 'utf-8')).pipe(Effect.flatMap(Schema.decodeEffect(Manifest.json)), Layer.effect(Plugin), Layer.merge(hosted))), Layer.tapCause(logged)));
    const run = (work: Effect.Effect<void, F, R | Uxp | Plugin>) => (): Promise<void> => runtime.runPromise(Effect.tapCause(work, logged));
    const handlers = {
        plugin: { create: (): Promise<void> => runtime.runPromise(Effect.void), destroy: (): Promise<void> => runtime.dispose() },
        commands: Record.map(entries.commands, run),
        panels: Record.map(entries.panels, Record.map(run)),
    } satisfies Pick<Parameters<typeof entrypoints.setup>[0], 'plugin' | 'commands' | 'panels'>;
    Reflect.apply(entrypoints.setup, entrypoints, [handlers]);
};
const script = <E>(server: Layer.Layer<never, E, RpcServer.Protocol | Uxp>): void => {
    const target: unknown = Reflect.get(uxp, 'script');
    Effect.gen(function* () {
        const [{ args, setResult }, serialization] = yield* Effect.all([Schema.decodeUnknownEffect(Schema.Struct({ args: Schema.Array(Schema.String), setResult: Schema.declare(Predicate.isFunction) }))(target), RpcSerialization.RpcSerialization]);
        const answer = (chunk: string | Uint8Array) => Effect.sync(() => Reflect.apply(setResult, target, [Predicate.isString(chunk) ? chunk : new TextDecoder().decode(chunk)]));
        const stdio = Stdio.make({ args: Effect.succeed(args), stdin: Stream.encodeText(Stream.fromIterable(args)), stdout: () => Sink.forEach(answer), stderr: () => Sink.forEach(Console.error) });
        yield* Layer.launch(server).pipe(
            Effect.provide(RpcServer.layerProtocolStdio.pipe(Layer.provide(Layer.succeed(Stdio.Stdio)(stdio)))),
            Effect.catchCauseIf(Predicate.not(Cause.hasInterruptsOnly), (cause) => Effect.flatMap(Schema.encodeEffect(Schema.Defect())(Cause.squash(cause)), (defect) => Effect.transposeOption(Option.map(Option.fromUndefinedOr(serialization.makeUnsafe().encode(RpcMessage.ResponseDefectEncoded(defect))), answer)))),
        );
    }).pipe(Effect.provide([hosted, RpcSerialization.layerJson]), Effect.runSyncExit);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { plugin, script };
