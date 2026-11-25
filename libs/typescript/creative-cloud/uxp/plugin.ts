import 'core-js/modules/web.url.can-parse.js';
import { Link, type Upstream } from '@rasm/mcp';
import { type Evaluation, EvaluationRpcs, HostError, type Render, RenderRpcs, Specifier } from '@rasm/object-model';
import { type Containment, encode, reference, resolve } from '@rasm/object-model/host';
import { Unmatched } from '@rasm/search';
import { layer } from '@rasm/telemetry';
import fs from 'adobe:fs';
import uxp, { entrypoints, type File } from 'adobe:uxp';
import { formatHex8 } from 'culori/fn';
import {
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
    Queue,
    Record,
    Result,
    Schema,
    SchemaGetter,
    Scope,
    ScopedRef,
    Semaphore,
    Sink,
    Stdio,
    Stream,
    Struct,
} from 'effect';
import { Toolkit } from 'effect/ai';
import { FetchHttpClient, Url } from 'effect/http';
import { RpcMessage, RpcSerialization, RpcServer } from 'effect/rpc';
import { Socket } from 'effect/socket';
import { imageSize } from 'image-size';
import { UxpManifest } from '#/manifest';

// --- [TYPES] ---------------------------------------------------------------------------

type Panel<F> = (root: HTMLElement, fork: <A, X>(work: Effect.Effect<A, X, Uxp | Plugin>) => void) => Effect.Effect<void, F, Scope.Scope | Uxp | Plugin>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = `plugin:/${UxpManifest.file}`;
const BUNDLE = /(?<=\.app)(?:\/.*)?$|(?<=[^/])$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Thrown = Schema.Struct({ message: Schema.Unknown });
const ImageSize = Schema.Struct({ width: Schema.Int, height: Schema.Int }).annotate({ description: 'Image size in pixels' });
const Chunk = Schema.Union([Schema.String, Schema.Uint8Array.pipe(Schema.decodeTo(Schema.String, { decode: SchemaGetter.transform((bytes) => new TextDecoder().decode(bytes)), encode: SchemaGetter.forbiddenEncoding }))]);
const Callable = Schema.declare((value): value is (...args: readonly unknown[]) => unknown => Predicate.isFunction(value));
const Scripting = Schema.Struct({ script: Schema.declare(Schema.is(Schema.Struct({ args: Schema.Array(Schema.String), setResult: Callable }))) });
const Entrypoints = Schema.Struct({ setup: Callable });
const ThemeDocument = Schema.Struct({ theme: Schema.declare(Schema.is(Schema.Struct({ getCurrent: Callable, onUpdated: Schema.Struct({ addListener: Callable, removeListener: Callable }) }))) });
const GlobalRequire = Schema.Struct({ require: Callable }).annotate({ description: 'Global require a UXP host resolves its own modules through by name' });
const HostInfo = Schema.Struct({ applicationPath: Schema.NonEmptyString.annotate({ description: 'Bundle path of the running installation' }) });
const Requests = EvaluationRpcs.merge(RenderRpcs);

// --- [SERVICES] ------------------------------------------------------------------------

class Uxp extends Context.Service<Uxp, { readonly uxp: typeof uxp; readonly fs: typeof fs }>()('@rasm/uxp/plugin/Uxp') {
    static readonly Entry = Schema.Struct({ isEntry: Schema.Literal(true), url: Schema.Union([Schema.String, Schema.URL]) });
    static readonly Missing = Schema.Literal(-2).annotate({ description: 'Value UXP fs throws for a path it finds no entry at' });
    static readonly Application = Uxp.Entry.mapFields(
        Struct.assign({
            url: Schema.Union([Schema.URLFromString, Schema.URL])
                .pipe(Schema.decode({ decode: SchemaGetter.transform((url: URL) => Url.setPathname(url, url.pathname.replace(BUNDLE, '/'))), encode: SchemaGetter.passthrough() }))
                .annotate({ description: 'Host application bundle URL with trailing slash' }),
        }),
    );
    static readonly layer = (modules: Uxp['Service']) =>
        Layer.mergeAll(
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
            }).pipe(Layer.effect(Clock.Clock)),
            Layer.succeed(Uxp)(modules),
            Layer.succeed(Logger.LogToStderr)(true),
            Path.layer,
            Socket.layerWebSocketConstructorGlobal,
        );
    static readonly toHostError = Effect.acquireRelease(
        Effect.sync(() => {
            const prepare: unknown = Reflect.get(Error, 'prepareStackTrace');
            const lines = new WeakMap<object, number>();
            Error.prepareStackTrace = (error, frames) => {
                const line = frames.find((frame) => frame.getScriptNameOrSourceURL() === UxpManifest.body)?.getLineNumber();
                if (Predicate.isNumber(line)) {
                    lines.set(error, line);
                }
                return Schema.is(Callable)(prepare) ? prepare(error, frames) : [String(error), ...frames.map((frame) => `    at ${frame.getFunctionName() ?? '<anonymous>'} (${frame.getScriptNameOrSourceURL()}:${frame.getLineNumber()}:${frame.getColumnNumber()})`)].join('\n');
            };
            return { prepare, lines };
        }),
        ({ prepare }) => Effect.sync(() => Reflect.set(Error, 'prepareStackTrace', prepare)),
    ).pipe(
        Effect.map(({ lines }) => (cause: unknown): HostError => {
            const line = Predicate.hasProperty(cause, 'stack') && Predicate.isString(cause.stack) ? Option.fromUndefinedOr(lines.get(cause)) : Option.none();
            return HostError.thrown(Inspectable.toStringUnknown(Option.match(Schema.decodeUnknownOption(Thrown)(cause), { onNone: () => cause, onSome: Struct.get('message') }), 0), line)(cause);
        }),
    );
    static readonly require = (module: string) => {
        const refused = HostError.refused(`require('${module}')`);
        return Schema.decodeUnknownEffect(GlobalRequire)(globalThis).pipe(
            Effect.mapError(refused),
            Effect.flatMap(({ require: resolved }) => Effect.try({ try: (): unknown => resolved(module), catch: refused })),
        );
    };
    static readonly application = Uxp.use(({ uxp: modules }) => Schema.decodeUnknownEffect(HostInfo)(modules.host)).pipe(Effect.flatMap(({ applicationPath }) => Path.Path.use((path) => path.toFileUrl(`${applicationPath}/`))));
    static readonly locate = (held: Containment, specifier: Pick<Specifier, 'steps' | 'end'>): Effect.Effect<unknown, Unmatched> => {
        const found = resolve(held, specifier);
        return Option.match(Schema.decodeUnknownOption(Unmatched.stated)(found), { onNone: () => Effect.succeed(found), onSome: Effect.fail });
    };
    static readonly evaluate = Effect.fnUntraced(function* (scope: Record.ReadonlyRecord<string, unknown>, code: string) {
        const toHostError = yield* Uxp.toHostError;
        const body = yield* Effect.try({
            try: (): unknown => new globalThis.Function(`{ ${Record.keys(scope).join(', ')} }`, 'return eval(arguments[1])')(scope, `${code}\n//# sourceURL=${UxpManifest.body}`),
            catch: toHostError,
        }).pipe(Effect.filterOrFail(Schema.is(Callable), (value) => new Cause.IllegalArgumentError(`code evaluates to ${typeof value}, not a body function`)));
        return {
            ran: Effect.try({ try: (): unknown => body(), catch: toHostError }),
            settled: (value: unknown) => Effect.tryPromise({ try: (): Promise<unknown> => Promise.resolve(value), catch: toHostError }),
        };
    });
    static readonly execute = Effect.fnUntraced(function* <E = never, R = never>(held: Containment, code: string, step: (call: Effect.Effect<unknown, HostError>) => Effect.Effect<unknown, HostError | E, R> = Function.identity) {
        const services = yield* Effect.context<never>();
        const resolveSpecifier = flow(
            Schema.decodeEffect(Specifier.FromString),
            Effect.flatMap((specifier) => Uxp.locate(held, specifier)),
            Effect.runSyncWith(services),
        );
        const { ran, settled } = yield* Uxp.evaluate({ resolve: resolveSpecifier }, code);
        const result = yield* settled(yield* step(ran));
        return yield* Effect.transposeOption(Option.map(Option.fromUndefinedOr(result), (each) => Schema.decodeUnknownEffect(Schema.Json)(encode(each, { reference: (object) => reference(held, object) }))));
    }, Effect.scoped);
    static readonly renderFile = Effect.fnUntraced(function* <E, R>(write: (file: File) => Effect.Effect<void, E, R>, name: string) {
        const { uxp: host, fs: files } = yield* Uxp;
        const folder = yield* Effect.tryPromise({ try: () => host.storage.localFileSystem.getTemporaryFolder(), catch: HostError.refused('temporary folder') });
        const file = yield* Effect.acquireRelease(Effect.tryPromise({ try: () => folder.createFile(name, { overwrite: true }), catch: HostError.refused(`${name} in ${folder.nativePath}`) }), (entry) =>
            Effect.try({ try: () => files.lstatSync(entry.nativePath), catch: HostError.refused(`stat of ${entry.nativePath}`) }).pipe(
                Effect.andThen(Effect.tryPromise({ try: () => entry.delete(), catch: HostError.refused(`delete of ${entry.nativePath}`) })),
                Effect.catchIf((error) => Schema.is(Uxp.Missing)(error.cause), Function.constant(Effect.void)),
                Effect.orDie,
            ),
        );
        yield* write(file);
        const image = yield* Effect.try({ try: () => new Uint8Array(files.readFileSync(file.nativePath)), catch: HostError.refused(`read of ${file.nativePath}`) });
        const size = yield* Effect.try({ try: () => imageSize(image), catch: HostError.refused(`image size of ${file.nativePath}`) });
        return { image, ...(yield* Schema.decodeEffect(ImageSize)(size)) };
    }, Effect.scoped);
    static readonly restore = (target: object, saved: Record.ReadonlyRecord<string, unknown>) => Effect.validate(Record.toEntries(saved), ([key, value]) => Effect.try({ try: () => Object.assign(target, { [key]: value }), catch: HostError.refused(`restore of ${key}`) }), { discard: true }).pipe(Effect.orDie);
    static readonly preserve = <T extends object>(target: T, replacements: Partial<T>, keys: readonly string[] = Struct.keys(replacements)) => {
        const refused = HostError.refused(`preferences ${keys.join(', ')}`);
        return Effect.acquireRelease(Effect.try({ try: () => Record.fromIterableWith(keys, (key): readonly [string, unknown] => [key, Reflect.get(target, key)]), catch: refused }), (saved) => Uxp.restore(target, saved)).pipe(Effect.tap(Effect.try({ try: () => Object.assign(target, replacements), catch: refused })));
    };
    static readonly serveScript = <E>(modules: Uxp['Service'], server: Layer.Layer<never, E, RpcServer.Protocol | Uxp>): void => {
        Effect.gen(function* () {
            const [{ script }, context, done] = yield* Effect.all([Schema.decodeUnknownEffect(Scripting)(modules.uxp), Effect.context<Uxp>(), Deferred.make<undefined>()]);
            const answer = flow(
                Schema.decodeEffect(Chunk),
                Effect.orDie,
                Effect.flatMap((text) => Effect.sync(script.setResult.bind(script, text))),
                Effect.andThen(Deferred.succeed(done, undefined)),
            );
            const reportDefect = Effect.fnUntraced(function* (cause: Cause.Cause<E>) {
                yield* Effect.logError(cause);
                const defect = yield* Effect.orDie(Schema.encodeEffect(Schema.Defect())(Cause.squash(cause)));
                const encoded = RpcSerialization.json.makeUnsafe().encode(RpcMessage.ResponseDefectEncoded(defect));
                return yield* encoded === undefined ? Effect.void : answer(encoded);
            });
            const stdio = Layer.succeed(Stdio.Stdio)(Stdio.make({ args: Effect.succeed(script.args), stdin: Stream.encodeText(Stream.fromIterable(script.args)), stdout: () => Sink.forEach(answer), stderr: () => Sink.forEach(Console.error) }));
            const exit = Layer.launch(server).pipe(Effect.provide(RpcServer.layerProtocolStdio.pipe(Layer.provide([stdio, RpcSerialization.layerJson]))), Effect.catchCauseIf(Predicate.not(Cause.hasInterruptsOnly), reportDefect), Effect.runSyncExitWith(context));
            const pending = Option.filter(Result.getSuccess(Exit.findDefect(exit)), Cause.isAsyncFiberError);
            yield* Effect.sync(() => Option.getOrUndefined(pending)?.fiber.interruptUnsafe());
            return yield* Option.isSome(pending) && !(yield* Deferred.isDone(done)) ? reportDefect(Cause.die(pending.value)) : Effect.void;
        }).pipe(Effect.tapCause(Effect.logError), Effect.provide(Uxp.layer(modules)), Effect.runSyncExit);
    };
}
class Plugin extends Context.Service<Plugin, Pick<UxpManifest, 'id'> & { readonly host: Pick<UxpManifest['host'], 'app'> }>()('@rasm/uxp/plugin') {
    static readonly theme = Stream.callback<unknown, Schema.SchemaError>(
        Effect.fnUntraced(function* (queue) {
            const listener = (current: unknown): void => {
                Queue.offerUnsafe(queue, current);
            };
            const { theme } = yield* Schema.decodeUnknownEffect(ThemeDocument)(document);
            yield* Effect.acquireRelease(
                Effect.sync(() => {
                    theme.onUpdated.addListener(listener);
                    listener(theme.getCurrent());
                }),
                Function.constant(Effect.sync(() => theme.onUpdated.removeListener(listener))),
            );
        }),
    ).pipe(Stream.mapEffect((theme) => Schema.decodeUnknownEffect(UxpManifest.Theme)(theme)));
    static readonly serve = <E, F>({
        link,
        commands = {},
        panels = {},
    }: {
        readonly link?: (plugin: string, root: Effect.Effect<URL, HostError, Path.Path>) => Layer.Layer<never, E, Uxp | Plugin | Path.Path | Socket.WebSocketConstructor>;
        readonly commands?: Record.ReadonlyRecord<string, Effect.Effect<void, F, Uxp | Plugin>>;
        readonly panels?: Record.ReadonlyRecord<string, Panel<F>>;
    }): void => {
        const hostLayer = Uxp.layer({ uxp, fs });
        Effect.gen(function* () {
            const [manifest, { setup }] = yield* Effect.all([
                Effect.try({ try: () => fs.readFileSync(MANIFEST, 'utf-8'), catch: (cause) => PlatformError.systemError({ _tag: 'Unknown', module: 'FileSystem', method: 'readFileString', pathOrDescriptor: MANIFEST, cause }) }).pipe(Effect.flatMap(Schema.decodeEffect(UxpManifest.Identity))),
                Schema.decodeUnknownEffect(Entrypoints)(entrypoints),
            ]);
            const refused = HostError.refused('UXP plugin folder');
            const root = Effect.all([Effect.tryPromise({ try: () => uxp.storage.localFileSystem.getPluginFolder(), catch: refused }), Path.Path]).pipe(Effect.flatMap(([{ nativePath }, path]) => Effect.mapError(path.toFileUrl(nativePath), refused)));
            const linkLayer = link === undefined ? Layer.empty : Layer.launch(link(manifest.id, root)).pipe(logFailure, Effect.forkScoped, Layer.effectDiscard);
            const rejections = Stream.runForEach(Stream.fromEventListener<PromiseRejectionEvent>(globalThis, 'unhandledrejection'), ({ reason }) => Effect.logError('Unhandled rejection', Cause.die(reason))).pipe(Effect.forkScoped, Layer.effectDiscard);
            const runtime = ManagedRuntime.make(Layer.merge(linkLayer, rejections).pipe(Layer.provideMerge(Layer.succeed(Plugin)(manifest)), Layer.provideMerge(Layer.provide(layer({ serviceName: manifest.id }), FetchHttpClient.layer)), Layer.provideMerge(hostLayer)));
            const [commandHandlers, panelHandlers] = yield* Effect.all([
                Effect.all(Record.map(commands, (command) => Effect.map(FiberHandle.make<unknown, F>(), (running) => ({ run: () => runtime.runPromise(Effect.flatMap(FiberHandle.run(running, logFailure(command)), Fiber.join)), cancel: () => runtime.runPromise(FiberHandle.clear(running)) })))),
                Effect.all(Record.map(panels, (each) => panel(runtime, each))),
            ]).pipe(Scope.provide(runtime.scope));
            yield* Effect.sync(() => setup.call(entrypoints, { plugin: { create: () => runtime.runPromise(Effect.void) }, commands: commandHandlers, panels: panelHandlers } satisfies Pick<Parameters<typeof entrypoints.setup>[0], 'plugin' | 'commands' | 'panels'>));
        }).pipe(Effect.tapCause(Effect.logError), Effect.provide(hostLayer), Effect.runFork);
    };
    static readonly link = <EA, EX extends Error, ER extends Error, EN = never, F = never>({
        application,
        execute,
        render,
        events = Stream.empty,
        commands = {},
    }: {
        readonly application: Effect.Effect<Link['application'], EA, Uxp | Path.Path | Scope.Scope>;
        readonly execute: (code: string, label: string) => Effect.Effect<Option.Option<Schema.Json>, EX, Uxp | Path.Path | Scope.Scope>;
        readonly render: (request: Render, file: File) => Effect.Effect<void, ER, Uxp | Path.Path | Scope.Scope>;
        readonly events?: Stream.Stream<Stream.Success<Upstream['events']>, EN>;
        readonly commands?: Record.ReadonlyRecord<string, Effect.Effect<void, F, Uxp | Plugin>>;
    }): void => {
        const permit = Semaphore.makeUnsafe(1);
        const hostScope = Effect.map(
            Effect.context<Uxp | Path.Path>(),
            (services) =>
                <A, E>(effect: Effect.Effect<A, E, Uxp | Path.Path | Scope.Scope>) =>
                    Effect.scoped(effect).pipe(Semaphore.withPermit(permit), Effect.provideContext(services)),
        );
        const requests = Requests.toLayer(
            Effect.map(hostScope, (hosted) => ({
                evaluate: ({ code, label }: Evaluation) => hosted(Effect.map(execute(code, label), (result) => ({ result }))),
                render: (request: Render) => hosted(Uxp.renderFile((file) => render(request, file), 'rendered.png')),
            })),
        );
        const hostEvents = Stream.catchCause(events, (cause) => Stream.drain(Stream.fromEffect(Effect.logError('Host events ended', cause))));
        Plugin.serve({
            commands,
            link: (plugin, root) =>
                Link.dial(
                    Requests,
                    Layer.merge(Link.serve(Toolkit.empty, { events: hostEvents }), requests),
                    new URL(UxpManifest.link),
                    Effect.flatMap(hostScope, (hosted) => Effect.all([hosted(application), root])).pipe(Effect.map(([named, folder]) => Link.make({ application: named, plugin, root: Option.some(folder) }))),
                ),
        });
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const logFailure = <A, X, R>(effect: Effect.Effect<A, X, R>) => effect.pipe(Effect.tapCauseIf(Predicate.not(Cause.hasInterruptsOnly<X>), Effect.logError));
const applyTheme = Effect.fnUntraced(function* (root: HTMLElement) {
    const { host } = yield* Plugin;
    yield* Stream.runForEach(Plugin.theme, (theme) => Effect.sync(() => Object.assign(root.style, { color: formatHex8(UxpManifest.fills[host.app][theme]) })));
});
const panel = Effect.fnUntraced(function* <E, F>(runtime: ManagedRuntime.ManagedRuntime<Uxp | Plugin, E>, body: Panel<F>) {
    const held = yield* ScopedRef.fromAcquire(Effect.void);
    const run = <X>(effect: Effect.Effect<unknown, X, Uxp | Plugin>): Promise<void> => runtime.runPromise(Effect.asVoid(logFailure(effect)));
    const fork = Effect.fnUntraced(
        function* (element: HTMLElement) {
            const runFork = yield* FiberSet.makeRuntime<Uxp | Plugin, unknown, never>();
            yield* Effect.forkScoped(logFailure(applyTheme(element)));
            return yield* body(element, (work) => {
                runFork(Effect.ignore(logFailure(work)));
            });
        },
        (effect) => Effect.asVoid(Effect.forkScoped(logFailure(effect))),
    );
    return {
        show: (root?: unknown): Promise<void> => run(Effect.flatMap(Schema.decodeUnknownEffect(Schema.instanceOf(HTMLElement))(root), (element) => ScopedRef.set(held, fork(element)))),
        hide: (): Promise<void> => run(ScopedRef.set(held, Effect.void)),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Plugin, Uxp };
