import fs from 'adobe:fs';
import uxp, { entrypoints } from 'adobe:uxp';
import { Cause, Clock, Console, Duration, Effect, flow, Layer, ManagedRuntime, Option, PlatformError, Predicate, Record, Schema, Scope, ScopedRef, Sink, Stdio, Stream, UndefinedOr } from 'effect';
import { RpcMessage, RpcSerialization, RpcServer } from 'effect/rpc';
import type { Link } from '#/link';
import { Manifest } from '#/manifest';
import { Plugin, Uxp } from '#/runtime';

// --- [MODELS] --------------------------------------------------------------------------

const Root = Schema.declare((value: unknown): value is HTMLElement => value instanceof HTMLElement);

// --- [COMPOSITION] ---------------------------------------------------------------------

const logged = flow(Cause.pretty, Console.error);
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
const plugin = <E, F>(
    dial: (link: Pick<Parameters<typeof Link.dial>[2], 'origin' | 'extension'>) => Layer.Layer<never, E, Uxp | Plugin>,
    entries: {
        readonly commands: Record.ReadonlyRecord<string, Effect.Effect<void, F, Uxp | Plugin>>;
        readonly panels: Record.ReadonlyRecord<string, (root: HTMLElement) => Effect.Effect<void, F, Scope.Scope | Uxp | Plugin>>;
    } = { commands: {}, panels: {} },
): void => {
    const manifest = 'plugin:/manifest.json';
    const runtime = ManagedRuntime.make(
        Effect.gen(function* () {
            const decoded = yield* Effect.try({ try: () => fs.readFileSync(manifest, 'utf-8'), catch: (cause) => PlatformError.systemError({ _tag: 'Unknown', module: 'FileSystem', method: 'readFileString', pathOrDescriptor: manifest, cause }) }).pipe(Effect.flatMap(Schema.decodeEffect(Manifest.json)));
            const origin = yield* Effect.fromOption(
                Option.map(decoded.requiredPermissions.network, ({ domains: [first] }) => first),
                () => new Cause.NoSuchElementError(`${decoded.name} manifest lacks requiredPermissions.network domains`),
            );
            return Layer.provideMerge(dial({ origin, extension: decoded.name }), Layer.succeed(Plugin)(decoded));
        }).pipe(Layer.unwrap, Layer.provideMerge(hosted), Layer.tapCause(logged)),
    );
    const run = <X>(work: Effect.Effect<unknown, X, Uxp | Plugin>): Promise<void> => runtime.runPromise(Effect.asVoid(Effect.tapCause(work, logged)));
    const panels = Record.map(entries.panels, (shown) => {
        const held = Effect.runSync(Scope.provide(ScopedRef.fromAcquire(Effect.void), runtime.scope));
        const forked = flow(shown, Effect.tapCauseIf(Predicate.not(Cause.hasInterruptsOnly), logged), Effect.forkScoped, Effect.asVoid);
        return {
            show: (root?: unknown): Promise<void> => run(Effect.flatMap(Schema.decodeUnknownEffect(Root)(root), (element) => ScopedRef.set(held, forked(element)))),
            hide: (): Promise<void> => run(ScopedRef.set(held, Effect.void)),
        };
    });
    Schema.decodeEffect(Schema.Struct({ setup: Schema.declare(Predicate.isFunction) }))(entrypoints).pipe(
        Effect.flatMap(({ setup }) =>
            Effect.sync(() =>
                setup.call(entrypoints, {
                    plugin: { create: () => runtime.runPromise(Effect.void), destroy: () => runtime.dispose() },
                    commands: Record.map(entries.commands, (work) => () => run(work)),
                    panels,
                } satisfies Pick<Parameters<typeof entrypoints.setup>[0], 'plugin' | 'commands' | 'panels'>),
            ),
        ),
        Effect.tapCause(logged),
        Effect.runFork,
    );
};
const script = <E>(server: Layer.Layer<never, E, RpcServer.Protocol | Uxp>): void => {
    Effect.gen(function* () {
        const [{ script: host }, serialization] = yield* Effect.all([Schema.decodeUnknownEffect(Schema.Struct({ script: Schema.declare(Schema.is(Schema.Struct({ args: Schema.Array(Schema.String), setResult: Schema.declare(Predicate.isFunction) }))) }))(uxp), RpcSerialization.RpcSerialization]);
        const answer = (chunk: string | Uint8Array) => Effect.sync(() => host.setResult(Predicate.isString(chunk) ? chunk : new TextDecoder().decode(chunk)));
        yield* Layer.launch(server).pipe(
            Effect.provide(RpcServer.layerProtocolStdio.pipe(Layer.provide(Layer.succeed(Stdio.Stdio)(Stdio.make({ args: Effect.succeed(host.args), stdin: Stream.encodeText(Stream.fromIterable(host.args)), stdout: () => Sink.forEach(answer), stderr: () => Sink.forEach(Console.error) }))))),
            Effect.catchCauseIf(
                Predicate.not(Cause.hasInterruptsOnly),
                flow(
                    Cause.squash,
                    Schema.encodeEffect(Schema.Defect()),
                    Effect.flatMap((defect) => UndefinedOr.match(serialization.makeUnsafe().encode(RpcMessage.ResponseDefectEncoded(defect)), { onUndefined: () => Effect.void, onDefined: answer })),
                ),
            ),
        );
    }).pipe(Effect.tapCauseIf(Predicate.not(Cause.hasInterruptsOnly), logged), Effect.provide([hosted, RpcSerialization.layerJson]), Effect.runSyncExit);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { plugin, script };
