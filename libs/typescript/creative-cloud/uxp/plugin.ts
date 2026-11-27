import { Description, type Source } from '@rasm/applications/description';
import 'core-js/modules/web.url.can-parse.js';
import { Link } from '@rasm/mcp/link';
import { type Containment, encode, reference, resolve } from '@rasm/object-model/host';
import { HostError, Specifier } from '@rasm/object-model/model';
import { Dialogs, type Evaluation, EvaluationRpcs, Failure, type Render, RenderRpcs } from '@rasm/object-model/rpcs';
import { Unmatched } from '@rasm/search/unmatched';
import { layer } from '@rasm/telemetry/otlp';
import fs from 'adobe:fs';
import uxp, { entrypoints, type File } from 'adobe:uxp';
import { formatHex8 } from 'culori/fn';
import { Array, Cache, Cause, Clock, Context, Duration, Effect, Exit, Fiber, FiberHandle, FiberSet, Function, flow, Inspectable, Layer, Logger, ManagedRuntime, Option, Path, PlatformError, Predicate, Queue, Record, Schema, SchemaGetter, Scope, ScopedRef, Stream, Struct } from 'effect';
import { type Tool, Toolkit } from 'effect/ai';
import { FetchHttpClient, Url } from 'effect/http';
import { type Rpc, RpcGroup } from 'effect/rpc';
import { Socket } from 'effect/socket';
import { imageSize } from 'image-size';
import { UxpManifest } from '#/manifest';

// --- [TYPES] ---------------------------------------------------------------------------

type Panel<F> = (root: HTMLElement, fork: <A, X>(work: Effect.Effect<A, X, Uxp | Plugin>) => void) => Effect.Effect<void, F, Scope.Scope | Uxp | Plugin>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = `plugin:/${UxpManifest.file}`;
const BUNDLE = /(?<=\.app)(?:\/.*)?$|(?<=[^/])$/u;
const RENDERED = 'rendered.png';
const BODY = 'body.js';

// --- [MODELS] --------------------------------------------------------------------------

const Thrown = Schema.Struct({ message: Schema.Unknown });
const ImageSize = Schema.Struct({ width: Schema.Int, height: Schema.Int });
const Callable = Schema.declare((value): value is (...args: readonly unknown[]) => unknown => Predicate.isFunction(value));
const HostObject = <const Fields extends Schema.Struct.Fields, Kind extends object = object>(fields: Fields, kind?: Predicate.Refinement<object, Kind>) => {
    const members = Schema.Struct(fields);
    return Schema.declare(
        (value: unknown): value is Kind & typeof members.Type =>
            Predicate.isObjectKeyword(value) &&
            (kind === undefined || kind(value)) &&
            Schema.is(members)(
                Record.fromIterableWith(
                    Array.filter(Struct.keys(fields), (key) => Reflect.has(value, key)),
                    (key) => [key, Reflect.get(value, key)],
                ),
            ),
    );
};
const Entrypoints = Schema.Struct({ setup: Callable });
const ThemeDocument = Schema.Struct({ theme: HostObject({ getCurrent: Callable, onUpdated: Schema.Struct({ addListener: Callable, removeListener: Callable }) }) });
const GlobalRequire = Schema.Struct({ require: Callable });
const HostInfo = Schema.Struct({ applicationPath: Schema.NonEmptyString });
const Requests = EvaluationRpcs.merge(RenderRpcs);
const DescriptionRpcs = Description.Rpcs.failing(Failure);
const PointerJson = Schema.toCodecJson(Description.fields.pointer.schema);
const WebSocketUrl = Schema.URLFromString.check(Schema.makeFilter(({ protocol }) => protocol === 'ws:' || protocol === 'wss:'));

// --- [SERVICES] ------------------------------------------------------------------------

class Uxp extends Context.Service<Uxp, { readonly uxp: typeof uxp; readonly fs: typeof fs }>()('@rasm/uxp/plugin/Uxp') {
    static readonly Entry = Schema.Struct({ isEntry: Schema.Literal(true), url: Schema.Union([Schema.String, Schema.URL]) });
    static readonly Collection = Schema.Struct({ length: Schema.Int });
    static readonly Missing = Schema.Literal(-2);
    static readonly Application = Uxp.Entry.mapFields(
        Struct.assign({
            url: Schema.Union([Schema.URLFromString, Schema.URL]).pipe(Schema.decode({ decode: SchemaGetter.transform((url: URL) => Url.setPathname(url, url.pathname.replace(BUNDLE, '/'))), encode: SchemaGetter.passthrough() })),
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
            Error.prepareStackTrace = (error, frames: readonly (NodeJS.CallSite & { readonly toString: () => string })[]) => {
                const line = frames.find((frame) => frame.getScriptNameOrSourceURL() === BODY)?.getLineNumber();
                if (Predicate.isNumber(line)) {
                    lines.set(error, line);
                }
                return Schema.is(Callable)(prepare) ? prepare(error, frames) : [String(error), ...frames.map((frame) => `    at ${frame.toString()}`)].join('\n');
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
    static readonly require = <S extends Schema.Top>(module: string, schema: S) => {
        const refused = HostError.refused(`require('${module}')`);
        return Effect.suspend(() => Schema.decodeUnknownEffect(GlobalRequire)(globalThis)).pipe(
            Effect.mapError(refused),
            Effect.flatMap(({ require: resolved }) => Effect.try({ try: (): unknown => resolved(module), catch: refused })),
            Effect.flatMap((loaded) => Schema.decodeUnknownEffect(schema)(loaded)),
        );
    };
    static readonly listen = <Args extends readonly unknown[]>(subject: string, { add, remove }: Record.ReadonlyRecord<'add' | 'remove', (listener: (...args: Args) => void) => unknown>) => {
        const refused = HostError.refused(subject);
        return Stream.callback<Args, HostError>((queue) => {
            const listener = (...args: Args): void => {
                Queue.offerUnsafe(queue, args);
            };
            return Effect.acquireRelease(Effect.tryPromise({ try: () => Promise.resolve(add(listener)), catch: refused }), () => Effect.orDie(Effect.tryPromise({ try: () => Promise.resolve(remove(listener)), catch: refused })));
        });
    };
    static readonly application = Uxp.use(({ uxp: modules }) => Schema.decodeUnknownEffect(HostInfo)(modules.host)).pipe(Effect.flatMap(({ applicationPath }) => Path.Path.use((path) => path.toFileUrl(`${applicationPath}/`))));
    static readonly locate = (held: Containment, specifier: Pick<Specifier, 'steps' | 'end'>): Effect.Effect<unknown, Unmatched> => {
        const found = resolve(held, specifier);
        return Option.match(Schema.decodeUnknownOption(Unmatched.stated)(found), { onNone: () => Effect.succeed(found), onSome: Effect.fail });
    };
    static readonly evaluate = Effect.fnUntraced(function* (scope: Record.ReadonlyRecord<string, unknown>, code: string) {
        const toHostError = yield* Uxp.toHostError;
        const body = yield* Effect.try({
            try: (): unknown => new globalThis.Function(`{ ${Record.keys(scope).join(', ')} }`, 'return eval(arguments[1])')(scope, `${code}\n//# sourceURL=${BODY}`),
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
    static readonly renderFile = Effect.fnUntraced(function* <E, R>(write: (file: File) => Effect.Effect<void, E, R>) {
        const { uxp: host, fs: files } = yield* Uxp;
        const folder = yield* Effect.tryPromise({ try: () => host.storage.localFileSystem.getTemporaryFolder(), catch: HostError.refused('temporary folder') });
        const file = yield* Effect.acquireRelease(Effect.tryPromise({ try: () => folder.createFile(RENDERED, { overwrite: true }), catch: HostError.refused(`${RENDERED} in ${folder.nativePath}`) }), (entry) =>
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
}
class Plugin extends Context.Service<Plugin, typeof UxpManifest.Identity.Type>()('@rasm/uxp/plugin') {
    static readonly theme = Effect.suspend(() => Schema.decodeUnknownEffect(ThemeDocument)(document)).pipe(
        Effect.map(({ theme }) =>
            Stream.concat(
                Stream.fromEffect(Effect.try({ try: (): unknown => theme.getCurrent(), catch: HostError.refused('current theme') })),
                Stream.map(Uxp.listen<readonly [unknown]>('theme updates', { add: (listener) => theme.onUpdated.addListener(listener), remove: (listener) => theme.onUpdated.removeListener(listener) }), Array.headNonEmpty),
            ),
        ),
        Stream.unwrap,
        Stream.mapEffect((theme) => Schema.decodeUnknownEffect(UxpManifest.Theme)(theme)),
    );
    static readonly scripting = <EX extends Error, ER extends Error, RX, RR>({ execute, render }: { readonly execute: (code: string, label: string) => Effect.Effect<Option.Option<Schema.Json>, EX, RX>; readonly render: (request: Render, file: File) => Effect.Effect<void, ER, RR> }) => ({
        group: Requests,
        handlers: Requests.toLayer({ evaluate: ({ code, label }: Evaluation) => Effect.map(execute(code, label), (result) => ({ result })), render: (request: Render) => Uxp.renderFile((file) => render(request, file)) }),
    });
    static readonly serve = <E, F>({
        link,
        commands = {},
        panels = {},
    }: {
        readonly link: Layer.Layer<never, E, Uxp | Plugin | Path.Path | Socket.WebSocketConstructor>;
        readonly commands?: Record.ReadonlyRecord<string, Effect.Effect<void, F, Uxp | Plugin>>;
        readonly panels?: Record.ReadonlyRecord<string, Panel<F>>;
    }): void => {
        const hostLayer = Uxp.layer({ uxp, fs });
        Effect.gen(function* () {
            const [manifest, { setup }] = yield* Effect.all([
                Effect.try({ try: () => fs.readFileSync(MANIFEST, 'utf-8'), catch: (cause) => PlatformError.systemError({ _tag: 'Unknown', module: 'FileSystem', method: 'readFileString', pathOrDescriptor: MANIFEST, cause }) }).pipe(Effect.flatMap(Schema.decodeEffect(UxpManifest.Identity))),
                Schema.decodeUnknownEffect(Entrypoints)(entrypoints),
            ]);
            const rejections = Stream.runForEach(Stream.fromEventListener<PromiseRejectionEvent>(globalThis, 'unhandledrejection'), ({ reason }) => Effect.logError('Unhandled rejection', Cause.die(reason))).pipe(Effect.forkScoped, Layer.effectDiscard);
            const runtime = ManagedRuntime.make(Layer.merge(link, rejections).pipe(Layer.provideMerge(Layer.succeed(Plugin)(manifest)), Layer.provideMerge(Layer.provide(layer({ serviceName: manifest.id }), FetchHttpClient.layer)), Layer.provideMerge(hostLayer)));
            const [commandHandlers, panelHandlers] = yield* Effect.all([
                Effect.all(Record.map(commands, (command) => Effect.map(FiberHandle.make<unknown, F>(), (running) => ({ run: () => runtime.runPromise(Effect.flatMap(FiberHandle.run(running, logFailure(command)), Fiber.join)), cancel: () => runtime.runPromise(FiberHandle.clear(running)) })))),
                Effect.all(Record.map(panels, (each) => panel(runtime, each))),
            ]).pipe(Scope.provide(runtime.scope));
            yield* Effect.sync(() => setup.call(entrypoints, { plugin: { create: () => runtime.runPromise(Effect.void) }, commands: commandHandlers, panels: panelHandlers } satisfies Pick<Parameters<typeof entrypoints.setup>[0], 'plugin' | 'commands' | 'panels'>));
        }).pipe(Effect.tapCause(Effect.logError), Effect.provide(hostLayer), Effect.runFork);
    };
    static readonly dial = <
        Rpcs extends Rpc.Any,
        EA,
        RA,
        EH,
        RH,
        RD,
        ES extends Error = never,
        RS = never,
        EP extends Error = never,
        RP = never,
        EE extends Error = never,
        RE = never,
        Tools extends Record.ReadonlyRecord<string, Tool.Any & { readonly parametersSchema: Schema.Decoder<unknown> }> = Toolkit.Tools<typeof Toolkit.empty>,
    >({
        application,
        group,
        handlers,
        toolkit,
        source,
        events = Stream.empty,
        dialogs,
    }: {
        readonly application: Effect.Effect<Link['application'], EA, RA>;
        readonly group: RpcGroup.RpcGroup<Rpcs>;
        readonly handlers: Layer.Layer<Rpc.ToHandler<Rpcs> | Tool.HandlersFor<Tools>, EH, RH>;
        readonly toolkit?: Toolkit.Toolkit<Tools>;
        readonly source?: Omit<Source<ES, RS, EP, RP>, 'roots'>;
        readonly events?: Stream.Stream<typeof Link.Event.Type, EE, RE>;
        readonly dialogs: Effect.Effect<unknown, HostError, Scope.Scope | RD>;
    }) => {
        const peer = Cache.makeWith(() => Effect.all({ application, plugin: Plugin.useSync(Struct.get('id')) }), { capacity: 1, timeToLive: (exit) => (Exit.isSuccess(exit) ? Duration.infinity : Duration.zero) });
        const suppressed = Layer.effect(Dialogs)(
            Effect.map(Effect.context<RD>(), (services) => {
                const held = Effect.updateContext(dialogs, (scope: Context.Context<Scope.Scope>) => Context.merge(services, scope));
                return (request) => Effect.scoped(Effect.andThen(held, request));
            }),
        );
        const description = source === undefined ? { group: RpcGroup.make(), handlers: Layer.empty } : { group: DescriptionRpcs, handlers: DescriptionRpcs.toLayer(Struct.pick(source, ['describe', 'operate'])) };
        const pointers = Stream.mapEffect(source?.pointer ?? Stream.empty, (pointer) => Effect.map(Effect.orDie(Schema.encodeEffect(PointerJson)(pointer)), (detail) => ({ name: 'pointer' satisfies keyof typeof Description.fields, target: Option.none(), detail: Option.some(detail) })));
        const served = Link.serve(Toolkit.merge(Toolkit.empty, ...Array.fromNullishOr(toolkit)), Stream.merge(pointers, events)).pipe(Layer.provideMerge(Layer.mergeAll(handlers, description.handlers, suppressed)));
        const url = Plugin.use(({ id, requiredPermissions: { network } }) =>
            Effect.fromOption(
                Array.findFirst(Array.ensure(network?.domains ?? []), (domain) => Schema.decodeOption(WebSocketUrl)(domain)),
                () => new Cause.NoSuchElementError(`${id} manifest permits no WebSocket domain to link through`),
            ),
        );
        return Effect.all([url, peer]).pipe(
            Effect.flatMap(([at, identity]) => Layer.launch(Link.dial(group.merge(description.group, Link.Rpcs.omit('tools/list', 'events')).middleware(Dialogs), served, at, Cache.get(identity, undefined)))),
            logFailure,
            Effect.forkScoped,
            Layer.effectDiscard,
        );
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
    const run = <X>(effect: Effect.Effect<unknown, X, Uxp | Plugin>): Promise<void> => runtime.runPromise(effect.pipe(logFailure, Effect.asVoid));
    const fork = Effect.fnUntraced(
        function* (element: HTMLElement) {
            const runFork = yield* FiberSet.makeRuntime<Uxp | Plugin, unknown, never>();
            yield* applyTheme(element).pipe(logFailure, Effect.forkScoped);
            return yield* body(element, (work) => {
                work.pipe(logFailure, Effect.ignore, runFork);
            });
        },
        (effect) => effect.pipe(logFailure, Effect.forkScoped, Effect.asVoid),
    );
    return {
        show: (root?: unknown): Promise<void> => run(Effect.flatMap(Schema.decodeUnknownEffect(Schema.instanceOf(HTMLElement))(root), (element) => ScopedRef.set(held, fork(element)))),
        hide: (): Promise<void> => run(ScopedRef.set(held, Effect.void)),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Callable, HostObject, Plugin, Uxp };
