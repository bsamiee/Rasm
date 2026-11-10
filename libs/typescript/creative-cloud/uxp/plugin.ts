import fs from 'adobe:fs';
import uxp, { entrypoints } from 'adobe:uxp';
import { HostError } from '@rasm/automation';
import { fillColor } from '@rasm/mark';
import { errorReporter, layerOtlp } from '@rasm/telemetry';
import {
    Array,
    Cause,
    Clock,
    Console,
    Context,
    Deferred,
    Duration,
    Effect,
    ErrorReporter,
    Exit,
    Fiber,
    FiberHandle,
    FiberSet,
    Function,
    flow,
    Layer,
    Logger,
    ManagedRuntime,
    Option,
    Path,
    PlatformError,
    Predicate,
    pipe,
    Queue,
    Record,
    Result,
    Schema,
    SchemaGetter,
    Scope,
    ScopedRef,
    Sink,
    Stdio,
    Stream,
    Struct,
} from 'effect';
import { FetchHttpClient } from 'effect/http';
import { RpcMessage, RpcSerialization, RpcServer } from 'effect/rpc';
import { UxpManifest } from '#/index';

// --- [TYPES] ---------------------------------------------------------------------------

type Panel<F> = (root: HTMLElement, fork: <A, X>(work: Effect.Effect<A, X, Uxp | Plugin>) => void) => Effect.Effect<void, F, Scope.Scope | Uxp | Plugin>;

interface ThemeEvents {
    readonly getCurrent: () => unknown;
    readonly onUpdated: { readonly addListener: (listener: (theme: unknown) => void) => void; readonly removeListener: (listener: (theme: unknown) => void) => void };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const EVALUATED = 'evaluated.js';
const IDENTIFIER = /^[$_\p{ID_Start}](?:[$\p{ID_Continue}]|‌|‍)*$/u;
const MANIFEST = `plugin:/${UxpManifest.file}`;

// --- [MODELS] --------------------------------------------------------------------------

const Thrown = Schema.Struct({ message: Schema.Unknown });
const Chunk = Schema.Union([Schema.String, Schema.Uint8Array.pipe(Schema.decodeTo(Schema.String, { decode: SchemaGetter.transform((bytes) => new TextDecoder().decode(bytes)), encode: SchemaGetter.forbiddenEncoding }))]);
const Script = Schema.Struct({ script: Schema.declare(Schema.is(Schema.Struct({ args: Schema.Array(Schema.String), setResult: Schema.declare(Predicate.isFunction) }))) });
const Entrypoints = Schema.Struct({ setup: Schema.declare(Predicate.isFunction) });
const ThemeEvents = Schema.Struct({ getCurrent: Schema.declare(Predicate.isFunction), onUpdated: Schema.Struct({ addListener: Schema.declare(Predicate.isFunction), removeListener: Schema.declare(Predicate.isFunction) }) });
const Themed = Schema.Struct({ theme: Schema.declare((value): value is ThemeEvents => Schema.is(ThemeEvents)(value)) });

// --- [SERVICES] ------------------------------------------------------------------------

class Uxp extends Context.Service<Uxp, { readonly uxp: typeof uxp; readonly fs: typeof fs }>()('@rasm/uxp/plugin/Uxp') {
    static readonly Entry = Schema.Struct({ isEntry: Schema.Literal(true), url: Schema.Union([Schema.String, Schema.URL]) });
    static readonly layer = (modules: Uxp['Service']) =>
        Layer.mergeAll(
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
            Layer.succeed(Uxp)(modules),
            Layer.succeed(Logger.LogToStderr)(true),
            ErrorReporter.layer([errorReporter], { mergeWithExisting: true }),
            Path.layer,
        );
    static readonly thrown = (cause: unknown): HostError => {
        const prepare = Error.prepareStackTrace;
        const lines: number[] = [];
        Error.prepareStackTrace = (error, frames) => {
            lines.push(...frames.filter((frame) => frame.getScriptNameOrSourceURL() === EVALUATED).flatMap((frame) => frame.getLineNumber() ?? []));
            return prepare?.(error, frames) ?? [String(error), ...frames.map((frame) => `    at ${frame}`)].join('\n');
        };
        try {
            const line = Predicate.hasProperty(cause, 'stack') && Predicate.isString(cause.stack) ? Array.head(lines) : Option.none();
            return HostError.thrown(String(Option.match(Schema.decodeUnknownOption(Thrown)(cause), { onNone: () => cause, onSome: Struct.get('message') })), line)(cause);
        } finally {
            Error.prepareStackTrace = prepare;
        }
    };
    static readonly evaluate = (scope: Record.ReadonlyRecord<string, unknown>, code: string) =>
        Effect.try({
            // biome-ignore lint/nursery/noImpliedEval: execute_script evaluates caller source in the host
            try: (): unknown => new globalThis.Function(`{ ${Record.keys(Record.filter(scope, (_, key) => IDENTIFIER.test(key))).join(', ')} }`, 'return eval(arguments[1])')(scope, `${code}\n//# sourceURL=${EVALUATED}`),
            catch: Uxp.thrown,
        }).pipe(
            Effect.filterOrFail(Predicate.isFunction, (value) => new Cause.IllegalArgumentError(`code evaluates to ${typeof value}, not a body function`)),
            Effect.map((body) => Effect.try({ try: (): unknown => body(), catch: Uxp.thrown })),
        );
    static readonly restore = (target: object, saved: Record.ReadonlyRecord<string, unknown>) =>
        pipe(
            saved,
            Record.toEntries,
            Effect.validate(([key, value]) => Effect.try({ try: () => Object.assign(target, { [key]: value }), catch: HostError.refused(`restore of ${key}`) }), { discard: true }),
            Effect.orDie,
        );
    static readonly preserve = <T extends object>(target: T, replacements: Partial<T>, keys: readonly string[] = Struct.keys(replacements)) => {
        const refused = HostError.refused(`preferences ${keys.join(', ')}`);
        const current = (key: string): unknown => Reflect.get(target, key);
        return Effect.acquireRelease(Effect.try({ try: () => Record.fromIterableWith(keys, (key) => [key, current(key)] as const), catch: refused }), (saved) => Uxp.restore(target, saved)).pipe(Effect.tap(Effect.try({ try: () => Object.assign(target, replacements), catch: refused })));
    };
    static readonly members = (prototype: object) => {
        const declared = (each: object) => {
            const descriptors = Record.toEntries(Object.getOwnPropertyDescriptors(each));
            return {
                name: each.constructor.name,
                fields: Array.filterMap(descriptors, ([name, { get, set }]) => (get === undefined ? Result.failVoid : Result.succeed({ name, settable: set !== undefined }))),
                methods: Array.filterMap(descriptors, ([name, { value }]) => (Predicate.isFunction(value) && name !== 'constructor' ? Result.succeed(name) : Result.failVoid)),
            };
        };
        const above = (each: unknown) => (Predicate.isObject(each) && each !== Object.prototype ? Option.some([declared(each), Object.getPrototypeOf(each)] as const) : Option.none());
        return Array.prepend(Array.unfold(Object.getPrototypeOf(prototype), above), declared(prototype));
    };
    static readonly script = <E>(modules: Uxp['Service'], server: Layer.Layer<never, E, RpcServer.Protocol | Uxp>): void => {
        Effect.gen(function* () {
            const [{ script }, context, done] = yield* Effect.all([Schema.decodeUnknownEffect(Script)(modules.uxp), Effect.context<Uxp>(), Deferred.make<void>()]);
            const answered = flow(
                Schema.decodeEffect(Chunk),
                Effect.orDie,
                Effect.flatMap((text) => Effect.sync(script.setResult.bind(script, text))),
                Effect.andThen(Deferred.succeed(done, undefined)),
            );
            const failed = Effect.fnUntraced(function* (cause: Cause.Cause<E>) {
                yield* Effect.logError(cause);
                const defect = yield* Effect.orDie(Schema.encodeEffect(Schema.Defect())(Cause.squash(cause)));
                const encoded = RpcSerialization.json.makeUnsafe().encode(RpcMessage.ResponseDefectEncoded(defect));
                return yield* encoded === undefined ? Effect.void : answered(encoded);
            });
            const stdio = Layer.succeed(Stdio.Stdio)(Stdio.make({ args: Effect.succeed(script.args), stdin: Stream.encodeText(Stream.fromIterable(script.args)), stdout: () => Sink.forEach(answered), stderr: () => Sink.forEach(Console.error) }));
            const exit = Layer.launch(server).pipe(Effect.provide(RpcServer.layerProtocolStdio.pipe(Layer.provide([stdio, RpcSerialization.layerJson]))), Effect.catchCauseIf(Predicate.not(Cause.hasInterruptsOnly), failed), Effect.runSyncExitWith(context));
            const pending = Option.filter(Result.getSuccess(Exit.findDefect(exit)), Cause.isAsyncFiberError);
            yield* Effect.sync(() => Option.getOrUndefined(pending)?.fiber.interruptUnsafe());
            return yield* Option.isSome(pending) && !(yield* Deferred.isDone(done)) ? failed(Cause.die(pending.value)) : Effect.void;
        }).pipe(Effect.tapCause(Effect.logError), Effect.provide(Uxp.layer(modules)), Effect.runSyncExit);
    };
}
class Plugin extends Context.Service<Plugin, typeof UxpManifest.identity.Type>()('@rasm/uxp/plugin') {
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
        readonly link?: (extension: string, root: Effect.Effect<URL, HostError, Path.Path>) => Layer.Layer<never, E, Uxp | Plugin | Path.Path>;
        readonly commands?: Record.ReadonlyRecord<string, Effect.Effect<void, F, Uxp | Plugin>>;
        readonly panels?: Record.ReadonlyRecord<string, Panel<F>>;
    }): void => {
        const hosted = Uxp.layer({ uxp, fs });
        Effect.gen(function* () {
            const [manifest, { setup }] = yield* Effect.all([
                Effect.try({ try: () => fs.readFileSync(MANIFEST, 'utf-8'), catch: (cause) => PlatformError.systemError({ _tag: 'Unknown', module: 'FileSystem', method: 'readFileString', pathOrDescriptor: MANIFEST, cause }) }).pipe(Effect.flatMap(Schema.decodeEffect(UxpManifest.identity))),
                Schema.decodeEffect(Entrypoints)(entrypoints),
            ]);
            const root = Effect.all([Effect.tryPromise(() => uxp.storage.localFileSystem.getPluginFolder()), Path.Path]).pipe(
                Effect.flatMap(([{ nativePath }, path]) => path.toFileUrl(nativePath)),
                Effect.mapError(HostError.refused('UXP plugin folder')),
            );
            const linked = link === undefined ? Layer.empty : Layer.launch(link(manifest.id, root)).pipe(reported, Effect.forkScoped, Layer.effectDiscard);
            const runtime = ManagedRuntime.make(linked.pipe(Layer.provideMerge(Layer.succeed(Plugin)(manifest)), Layer.provideMerge(Layer.provide(layerOtlp({ serviceName: manifest.id }), FetchHttpClient.layer)), Layer.provideMerge(hosted)));
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
const themed = (root: HTMLElement) =>
    Stream.runForEach(Plugin.theme, (theme) =>
        Effect.sync(() => {
            Object.assign(root.style, { color: fillColor(UxpManifest.fills[theme]) });
        }),
    );
const panel = Effect.fnUntraced(function* <E, F>(runtime: ManagedRuntime.ManagedRuntime<Uxp | Plugin, E>, body: Panel<F>) {
    const held = yield* ScopedRef.fromAcquire(Effect.void);
    const run = <X>(effect: Effect.Effect<unknown, X, Uxp | Plugin>): Promise<void> => runtime.runPromise(Effect.asVoid(reported(effect)));
    const fork = Effect.fnUntraced(
        function* (element: HTMLElement) {
            const forked = yield* FiberSet.makeRuntime<Uxp | Plugin, void, never>();
            yield* Effect.forkScoped(reported(themed(element)));
            return yield* body(element, flow(reported, Effect.ignore(), forked));
        },
        (effect) => Effect.asVoid(Effect.forkScoped(reported(effect))),
    );
    return {
        show: (root?: unknown): Promise<void> => run(Effect.flatMap(Schema.decodeUnknownEffect(Schema.instanceOf(HTMLElement))(root), (element) => ScopedRef.set(held, fork(element)))),
        hide: (): Promise<void> => run(ScopedRef.set(held, Effect.void)),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Plugin, Uxp };
