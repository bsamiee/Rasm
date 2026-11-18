import 'core-js/modules/web.url.can-parse.js';
import { HostError } from '@rasm/object-model';
import { layer } from '@rasm/telemetry';
import { parse } from 'acorn';
import fs from 'adobe:fs';
import uxp, { entrypoints } from 'adobe:uxp';
import { formatHex8 } from 'culori/fn';
import {
    Array,
    Cause,
    Clock,
    Console,
    Context,
    Deferred,
    Duration,
    Effect,
    Exit,
    Fiber,
    FiberHandle,
    FiberSet,
    Function,
    flow,
    Inspectable,
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
import { FetchHttpClient, Url } from 'effect/http';
import { RpcMessage, RpcSerialization, RpcServer } from 'effect/rpc';
import { imageSize } from 'image-size';
import { UxpManifest } from '#/manifest';

// --- [TYPES] ---------------------------------------------------------------------------

type Panel<F> = (root: HTMLElement, fork: <A, X>(work: Effect.Effect<A, X, Uxp | Plugin>) => void) => Effect.Effect<void, F, Scope.Scope | Uxp | Plugin>;

interface ThemeEvents {
    readonly getCurrent: () => unknown;
    readonly onUpdated: { readonly addListener: (listener: (theme: unknown) => void) => void; readonly removeListener: (listener: (theme: unknown) => void) => void };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const EVALUATED = 'evaluated.js';
const IDENTIFIER = /^(?!(?:await|break|case|catch|class|const|continue|debugger|default|delete|do|else|enum|export|extends|false|finally|for|function|if|import|in|instanceof|let|new|null|return|static|super|switch|this|throw|true|try|typeof|var|void|while|with|yield)$)[$_\p{ID_Start}](?:[$\p{ID_Continue}]|‌|‍)*$/u;
const MANIFEST = `plugin:/${UxpManifest.file}`;
const BUNDLE = /(?<=\.app)(?:\/.*)?$|(?<=[^/])$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Thrown = Schema.Struct({ message: Schema.Unknown });
const Measured = Schema.Struct({ width: Schema.Int, height: Schema.Int }).annotate({ description: 'Image size in pixels' });
const Chunk = Schema.Union([Schema.String, Schema.Uint8Array.pipe(Schema.decodeTo(Schema.String, { decode: SchemaGetter.transform((bytes) => new TextDecoder().decode(bytes)), encode: SchemaGetter.forbiddenEncoding }))]);
const Parameter = Schema.Struct({ type: Schema.String, start: Schema.Int, end: Schema.Int, left: Schema.OptionFromOptionalKey(Schema.Struct({ start: Schema.Int, end: Schema.Int })) });
const Declared = Schema.Struct({ body: Schema.Tuple([Schema.Struct({ expression: Schema.Struct({ body: Schema.Struct({ body: Schema.Tuple([Schema.Struct({ value: Schema.Struct({ params: Schema.Array(Parameter) }) })]) }) }) })]) });
const Callable = Schema.declare((value): value is (...args: readonly unknown[]) => unknown => Predicate.isFunction(value));
const Script = Schema.Struct({ script: Schema.declare(Schema.is(Schema.Struct({ args: Schema.Array(Schema.String), setResult: Callable }))) });
const Entrypoints = Schema.Struct({ setup: Callable });
const ThemeEvents = Schema.Struct({ getCurrent: Callable, onUpdated: Schema.Struct({ addListener: Callable, removeListener: Callable }) });
const Themed = Schema.Struct({ theme: Schema.declare((value): value is ThemeEvents => Schema.is(ThemeEvents)(value)) });

// --- [SERVICES] ------------------------------------------------------------------------

class Uxp extends Context.Service<Uxp, { readonly uxp: typeof uxp; readonly fs: typeof fs }>()('@rasm/uxp/plugin/Uxp') {
    static readonly Entry = Schema.Struct({ isEntry: Schema.Literal(true), url: Schema.Union([Schema.String, Schema.URL]) });
    static readonly Application = Uxp.Entry.mapFields(
        Struct.assign({
            url: Schema.Union([Schema.URLFromString, Schema.URL])
                .pipe(Schema.decode({ decode: SchemaGetter.transform((url: URL) => Url.setPathname(url, url.pathname.replace(BUNDLE, '/'))), encode: SchemaGetter.passthrough() }))
                .annotate({ description: 'Host application bundle URL with trailing slash' }),
        }),
    );
    static readonly layer = (modules: Uxp['Service']) =>
        Layer.mergeAll(
            Layer.effect(Clock.Clock)(
                Effect.map(Clock.Clock, (base) => {
                    const nanos = {
                        currentTimeNanosUnsafe: (): bigint => Duration.toNanosUnsafe(base.currentTimeMillisUnsafe()),
                        monotonicTimeNanosUnsafe: (): bigint => Duration.toNanosUnsafe(performance.now()),
                    };
                    return {
                        ...nanos,
                        currentTimeMillisUnsafe: base.currentTimeMillisUnsafe.bind(base),
                        currentTimeMillis: base.currentTimeMillis,
                        sleep: base.sleep.bind(base),
                        currentTimeNanos: Effect.sync(nanos.currentTimeNanosUnsafe),
                        monotonicTimeNanos: Effect.sync(nanos.monotonicTimeNanosUnsafe),
                    };
                }),
            ),
            Layer.succeed(Uxp)(modules),
            Layer.succeed(Logger.LogToStderr)(true),
            Path.layer,
        );
    static readonly thrown = (cause: unknown): HostError => {
        const prepare: unknown = Reflect.get(Error, 'prepareStackTrace');
        const lines: number[] = [];
        Error.prepareStackTrace = (error, frames) => {
            lines.push(...frames.filter((frame) => frame.getScriptNameOrSourceURL() === EVALUATED).flatMap((frame) => frame.getLineNumber() ?? []));
            // oxlint-disable-next-line typescript/no-base-to-string, typescript/restrict-template-expressions -- NodeJS.CallSite omits the toString V8 call sites carry
            return Schema.is(Callable)(prepare) ? prepare(error, frames) : [String(error), ...frames.map((frame) => `    at ${frame}`)].join('\n');
        };
        try {
            const line = Predicate.hasProperty(cause, 'stack') && Predicate.isString(cause.stack) ? Array.head(lines) : Option.none();
            return HostError.thrown(Inspectable.toStringUnknown(Option.match(Schema.decodeUnknownOption(Thrown)(cause), { onNone: () => cause, onSome: Struct.get('message') }), 0), line)(cause);
        } finally {
            Reflect.set(Error, 'prepareStackTrace', prepare);
        }
    };
    static readonly evaluate = (scope: Record.ReadonlyRecord<string, unknown>, code: string) =>
        Effect.try({
            // oxlint-disable-next-line typescript/no-implied-eval, typescript/no-unsafe-call -- execute_script evaluates caller source in the host
            try: (): unknown => new globalThis.Function(`{ ${Record.keys(Record.filter(scope, (_, key) => IDENTIFIER.test(key))).join(', ')} }`, 'return eval(arguments[1])')(scope, `${code}\n//# sourceURL=${EVALUATED}`),
            catch: Uxp.thrown,
        }).pipe(
            Effect.filterOrFail(Schema.is(Callable), (value) => new Cause.IllegalArgumentError(`code evaluates to ${typeof value}, not a body function`)),
            Effect.map((body) => Effect.try({ try: (): unknown => body(), catch: Uxp.thrown })),
        );
    static readonly rendered = Effect.fnUntraced(function* <E, R>(write: (file: Awaited<ReturnType<Awaited<ReturnType<typeof uxp.storage.localFileSystem.getTemporaryFolder>>['createFile']>>) => Effect.Effect<void, E, R>, name: string) {
        const { uxp: host, fs: files } = yield* Uxp;
        const folder = yield* Effect.tryPromise({ try: () => host.storage.localFileSystem.getTemporaryFolder(), catch: HostError.refused('temporary folder') });
        const file = yield* Effect.acquireRelease(Effect.tryPromise({ try: () => folder.createFile(name, { overwrite: true }), catch: HostError.refused(`${name} in ${folder.nativePath}`) }), (entry) =>
            Effect.orDie(Effect.tryPromise({ try: () => entry.delete(), catch: HostError.refused(`delete of ${entry.nativePath}`) })),
        );
        yield* write(file);
        const image = yield* Effect.try({ try: () => new Uint8Array(files.readFileSync(file.nativePath)), catch: HostError.refused(`read of ${file.nativePath}`) });
        const size = yield* Effect.try({ try: () => imageSize(image), catch: HostError.refused(`image size of ${file.nativePath}`) });
        return { image, ...(yield* Schema.decodeEffect(Measured)(size)) };
    }, Effect.scoped);
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
    static readonly members = (prototype: unknown) => Array.unfold(prototype, (each) => (Predicate.isObject(each) && each !== Object.prototype ? Option.some([declared(each), Object.getPrototypeOf(each)] as const) : Option.none()));
    static readonly script = <E>(modules: Uxp['Service'], server: Layer.Layer<never, E, RpcServer.Protocol | Uxp>): void => {
        Effect.gen(function* () {
            const [{ script }, context, done] = yield* Effect.all([Schema.decodeUnknownEffect(Script)(modules.uxp), Effect.context<Uxp>(), Deferred.make<undefined>()]);
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
                Schema.decodeUnknownEffect(Entrypoints)(entrypoints),
            ]);
            const refused = HostError.refused('UXP plugin folder');
            const root = Effect.all([Effect.tryPromise({ try: () => uxp.storage.localFileSystem.getPluginFolder(), catch: refused }), Path.Path]).pipe(Effect.flatMap(([{ nativePath }, path]) => Effect.mapError(path.toFileUrl(nativePath), refused)));
            const linked = link === undefined ? Layer.empty : Layer.launch(link(manifest.id, root)).pipe(reported, Effect.forkScoped, Layer.effectDiscard);
            const runtime = ManagedRuntime.make(linked.pipe(Layer.provideMerge(Layer.succeed(Plugin)(manifest)), Layer.provideMerge(Layer.provide(layer({ serviceName: manifest.id }), FetchHttpClient.layer)), Layer.provideMerge(hosted)));
            const [handled, shown] = yield* Effect.all([
                Effect.all(
                    Record.map(
                        commands,
                        Effect.fnUntraced(function* (command) {
                            const running = yield* FiberHandle.make<unknown, F>();
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

const declared = (each: object) => {
    const descriptors = Record.toEntries(Object.getOwnPropertyDescriptors(each));
    return {
        name: each.constructor.name,
        fields: Array.filterMap(descriptors, ([name, descriptor]) => (descriptor.get === undefined ? Result.failVoid : Result.succeed({ name, settable: descriptor.set !== undefined }))),
        methods: Array.filterMap(descriptors, ([name, { value }]: readonly [string, { readonly value?: unknown }]) => (Schema.is(Callable)(value) && name !== 'constructor' ? Result.succeed({ name, parameters: parameters(value) }) : Result.failVoid)),
    };
};
const parameters = (method: { readonly length: number }) => {
    const source = `(class { ${globalThis.Function.prototype.toString.call(method)} })`;
    return Option.liftThrowable(parse)(source, { ecmaVersion: 'latest' }).pipe(
        Option.flatMap(Schema.decodeUnknownOption(Declared)),
        Option.match({
            onNone: () => Array.makeBy(method.length, (at) => ({ name: `arguments[${at}]`, optional: false })),
            onSome: ({ body }) => Array.map(body[0].expression.body.body[0].value.params, ({ type, start, end, left }) => ({ name: Option.match(left, { onNone: () => source.slice(start, end), onSome: (bound) => source.slice(bound.start, bound.end) }), optional: Option.isSome(left) || type === 'RestElement' })),
        }),
    );
};
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
            Object.assign(root.style, { color: formatHex8(UxpManifest.fills[theme]) });
        }),
    );
const panel = Effect.fnUntraced(function* <E, F>(runtime: ManagedRuntime.ManagedRuntime<Uxp | Plugin, E>, body: Panel<F>) {
    const held = yield* ScopedRef.fromAcquire(Effect.void);
    const run = <X>(effect: Effect.Effect<unknown, X, Uxp | Plugin>): Promise<void> => runtime.runPromise(Effect.asVoid(reported(effect)));
    const fork = Effect.fnUntraced(
        function* (element: HTMLElement) {
            const forked = yield* FiberSet.makeRuntime<Uxp | Plugin, unknown, never>();
            yield* Effect.forkScoped(reported(themed(element)));
            return yield* body(element, (work) => {
                forked(Effect.ignore(reported(work)));
            });
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
