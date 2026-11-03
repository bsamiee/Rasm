import { basename, extname, relative, resolve } from 'node:path';
import process from 'node:process';
import { PassThrough } from 'node:stream';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { NodeStream } from '@effect/platform-node';
import { type Event, getEventsSince, subscribe, writeSnapshot } from '@parcel/watcher';
import { main } from '@rasm/mcp/node';
import { registerTool } from '@rasm/mcp/tool';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, type Cause, Config, Duration, Effect, Equal, Fiber, FileSystem, Function, flow, HashMap, HashSet, Layer, Match, Option, Order, PlatformError, Predicate, Queue, RcMap, Record, Result, Schema, SchemaTransformation, Scope, Stream, Struct, SubscriptionRef } from 'effect';
import { Tool } from 'effect/ai';
import { ChildProcess } from 'effect/process';
import { escape, minimatch } from 'minimatch';
import {
    CancellationTokenSource,
    ConfigurationRequest,
    ConnectionError,
    createMessageConnection,
    type Definition,
    type DefinitionLink,
    DefinitionRequest,
    type Diagnostic,
    type Hover,
    DiagnosticRefreshRequest,
    DiagnosticSeverity,
    DidChangeConfigurationNotification,
    DidChangeTextDocumentNotification,
    DidChangeWatchedFilesNotification,
    DidCloseTextDocumentNotification,
    DidOpenTextDocumentNotification,
    DidSaveTextDocumentNotification,
    DocumentDiagnosticReportKind,
    DocumentDiagnosticRequest,
    ExitNotification,
    FileChangeType,
    HoverRequest,
    InitializedNotification,
    InitializeRequest,
    type Location,
    LocationLink,
    LSPErrorCodes,
    MarkupKind,
    type NotificationType,
    type ProgressToken,
    type ProtocolNotificationType,
    PublishDiagnosticsNotification,
    type PublishDiagnosticsParams,
    type Range,
    ReferencesRequest,
    type Registration,
    RegistrationRequest,
    type RegistrationType,
    type RequestParam,
    type RequestType,
    ResponseError,
    ShutdownRequest,
    type TextDocumentItem,
    TextDocumentSyncKind,
    UnregistrationRequest,
    WatchKind,
    WorkDoneProgressCreateRequest,
} from 'vscode-languageserver-protocol/node';
import plugin from '#/.codex-plugin/plugin.json' with { type: 'json' };
import lsp from '#/.lsp.json' with { type: 'json' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const INITIAL = {
    registrations: HashMap.empty<string, Registration>(),
    progress: HashSet.empty<ProgressToken>(),
    begun: 0,
    invalidated: 0,
    documents: HashMap.empty<string, { readonly item: TextDocumentItem; readonly begun: number }>(),
    published: HashMap.empty<string, PublishDiagnosticsParams>(),
};

// --- [MODELS] --------------------------------------------------------------------------

const Timeout = (fallback: Duration.Duration) => Schema.Int.check(Schema.isGreaterThan(0)).pipe(Schema.decodeTo(Schema.DurationFromMillis), Schema.withDecodingDefaultTypeKey(Effect.succeed(fallback)));
const Server = Schema.Struct({
    command: Schema.String,
    args: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    env: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)),
    extensionToLanguage: Schema.Record(Schema.String, Schema.String),
    initializationOptions: Schema.optionalKey(Schema.Json),
    settings: Schema.OptionFromOptionalKey(Schema.Json),
    workspaceFolder: Schema.String.pipe(Schema.withDecodingDefaultKey(Effect.succeed('.'))),
    startupTimeout: Timeout(Duration.infinity),
    shutdownTimeout: Timeout(Duration.infinity),
    requestTimeout: Timeout(Duration.minutes(1)),
});
const File = Schema.Struct({ file: Schema.String.annotate({ description: 'Absolute or project-relative path' }) });
const Coordinate = Schema.Int.check(Schema.isGreaterThanOrEqualTo(1)).pipe(Schema.decodeTo(Schema.Int.check(Schema.isGreaterThanOrEqualTo(0)), SchemaTransformation.transform({ decode: (coordinate) => coordinate - 1, encode: (coordinate) => coordinate + 1 })));
const Position = Schema.Struct({ ...File.fields, line: Coordinate.pipe(Schema.annotateEncoded({ description: '1-based line' })), character: Coordinate.pipe(Schema.annotateEncoded({ description: '1-based UTF-16 column' })) }).pipe(Schema.encodeKeys({ character: 'column' }));
const Lines = Schema.Struct({ lines: Schema.Array(Schema.String) });

// --- [ERRORS] --------------------------------------------------------------------------

class Exited extends Schema.TaggedError<Exited, { readonly brand: unique symbol }>()('Exited', { command: Schema.String, args: Schema.Array(Schema.String), code: Schema.Int }) {
    override get message(): string {
        return `${[this.command, ...this.args].join(' ')} exited with code ${this.code}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const watch = (root: string, ignore: string[], snapshot: string) => {
    const scanned = { ignore, backend: 'brute-force' as const };
    const watched = <A>(run: () => Promise<A>) => Effect.tryPromise({ try: run, catch: (cause) => PlatformError.systemError({ _tag: 'Unknown', module: 'FileSystem', method: 'watch', pathOrDescriptor: root, cause }) });
    const record = watched(() => writeSnapshot(root, snapshot, scanned));
    return Stream.unwrap(
        Effect.as(
            record,
            Stream.callback<readonly [Error | null, readonly Event[]], PlatformError.PlatformError>((queue) =>
                Effect.acquireRelease(
                    watched(() => subscribe(root, (error, events) => Queue.offerUnsafe(queue, [error, events]), { ignore })),
                    (subscription) => Effect.promise(() => subscription.unsubscribe()),
                ),
            ),
        ),
    ).pipe(
        Stream.mapEffect(([error, events]) =>
            error === null
                ? Effect.succeed(events)
                : watched(() => getEventsSince(root, snapshot, scanned)).pipe(
                      Effect.tap(() => record),
                      Effect.map((missed) => [...events, ...missed]),
                  ),
        ),
    );
};

const start = Effect.fn('start')(function* (root: string, config: typeof Server.Type, workspace: Stream.Stream<readonly Event[], Error>) {
    const fs = yield* FileSystem.FileSystem;
    const folder = resolve(root, config.workspaceFolder);
    const input = new PassThrough();
    const child = yield* ChildProcess.make(config.command, config.args, { cwd: folder, env: config.env, extendEnv: true, stdin: NodeStream.fromReadable({ evaluate: () => input }).pipe(Stream.orDie) });
    const exited = Effect.flatMap(child.exitCode, (code) => Exited.make({ command: config.command, args: config.args, code }));
    yield* child.stderr.pipe(Stream.decodeText(), Stream.splitLines, Stream.runForEach(Effect.logInfo), Effect.ignore({ log: 'Warn', message: 'stderr read failed' }), Effect.forkScoped);
    const connection = yield* Effect.acquireRelease(
        Effect.sync(() => createMessageConnection(NodeStream.toReadableNever(child.stdout), input)),
        (opened) => Effect.sync(() => opened.dispose()),
    );
    const [state, updates] = yield* Effect.all([SubscriptionRef.make(INITIAL), Queue.unbounded<(current: typeof INITIAL) => typeof INITIAL>()]);
    const offer = (update: (current: typeof INITIAL) => typeof INITIAL) => {
        Queue.offerUnsafe(updates, update);
    };
    const invalidated = (current: typeof INITIAL) => ({ ...current, invalidated: current.invalidated + 1 });
    const setting = (section: string | undefined) => (section?.split('.') ?? []).reduce((value, key) => Option.flatMap(value, flow(Option.liftPredicate(Schema.is(Schema.JsonObject)), Option.flatMap(Record.get(key)))), config.settings);
    connection.onRequest(ConfigurationRequest.type, ({ items }) => items.map(flow(Struct.get('section'), setting, Option.getOrNull)));
    connection.onRequest(RegistrationRequest.type, ({ registrations }) => offer((current) => invalidated({ ...current, registrations: HashMap.setMany(current.registrations, Array.zip(registrations.map(Struct.get('id')), registrations)) })));
    connection.onRequest(UnregistrationRequest.type, ({ unregisterations }) => offer((current) => invalidated({ ...current, registrations: HashMap.removeMany(current.registrations, unregisterations.map(Struct.get('id'))) })));
    connection.onRequest(DiagnosticRefreshRequest.type, () => offer(invalidated));
    connection.onRequest(WorkDoneProgressCreateRequest.type, Function.constVoid);
    connection.onUnhandledProgress(({ token, value: { kind } }) => offer((current) => (kind === 'report' ? current : invalidated(kind === 'begin' ? { ...current, begun: current.begun + 1, progress: HashSet.add(current.progress, token) } : { ...current, progress: HashSet.remove(current.progress, token) }))));
    connection.onNotification(PublishDiagnosticsNotification.type, (params) => offer((current) => ({ ...current, published: HashMap.set(current.published, params.uri, params) })));
    connection.listen();
    yield* Stream.fromQueue(updates).pipe(
        Stream.runForEach((update) => SubscriptionRef.update(state, update)),
        Effect.forkScoped,
    );
    const registered = <RO>({ method }: RegistrationType<RO>, current: typeof INITIAL): readonly RO[] =>
        Array.fromIterable(HashMap.values(current.registrations))
            .filter((entry) => entry.method === method)
            .map(({ registerOptions }): RO => registerOptions);
    const classify = ({ cause }: Cause.UnknownError) =>
        Match.value(cause).pipe(
            Match.when(Match.instanceOf(ResponseError), Effect.fail<ResponseError<unknown>>),
            Match.when(Match.instanceOf(ConnectionError), () => exited),
            Match.orElse(Effect.die),
        );
    const notify = <P>(type: NotificationType<P>, params: RequestParam<P>) => Effect.tryPromise(() => connection.sendNotification(type, params)).pipe(Effect.catchTag('UnknownError', classify));
    const announce = <P, RO>(type: ProtocolNotificationType<P, RO>, current: typeof INITIAL, params: RequestParam<P>) => (registered(type, current).length > 0 ? notify(type, params) : Effect.void);
    const ask = <P, R>(type: RequestType<P, R, unknown>, params: RequestParam<P>, timeout = config.requestTimeout) =>
        Effect.acquireUseRelease(
            Effect.sync(() => new CancellationTokenSource()),
            (source) => Effect.tryPromise(() => connection.sendRequest(type, params, source.token)),
            (source) =>
                Effect.sync(() => {
                    source.cancel();
                    source.dispose();
                }),
        ).pipe(Effect.catchTag('UnknownError', classify), Effect.timeout(timeout), Effect.raceFirst(exited));
    const workspaceFolder = { uri: pathToFileURL(folder).href, name: basename(folder) };
    const { capabilities } = yield* ask(
        InitializeRequest.type,
        {
            processId: process.pid,
            rootUri: workspaceFolder.uri,
            workspaceFolders: [workspaceFolder],
            initializationOptions: config.initializationOptions,
            capabilities: {
                window: { workDoneProgress: true },
                workspace: { configuration: true, didChangeWatchedFiles: { dynamicRegistration: true, relativePatternSupport: true }, diagnostics: { refreshSupport: true } },
                textDocument: {
                    synchronization: { dynamicRegistration: true, didSave: true },
                    diagnostic: { dynamicRegistration: true },
                    publishDiagnostics: { versionSupport: true, relatedInformation: true, codeDescriptionSupport: true },
                    hover: { contentFormat: [MarkupKind.PlainText] },
                },
            },
        },
        config.startupTimeout,
    );
    const sync = typeof capabilities.textDocumentSync === 'number' ? { openClose: capabilities.textDocumentSync !== TextDocumentSyncKind.None, change: capabilities.textDocumentSync } : (capabilities.textDocumentSync ?? {});
    const declared = Record.filterMap(
        {
            [DidOpenTextDocumentNotification.method]: sync.openClose === true && {},
            [DidCloseTextDocumentNotification.method]: sync.openClose === true && {},
            [DidChangeTextDocumentNotification.method]: sync.change !== undefined && sync.change !== TextDocumentSyncKind.None && { syncKind: sync.change },
            [DidSaveTextDocumentNotification.method]: sync.save === true ? {} : sync.save,
            [DocumentDiagnosticRequest.method]: capabilities.diagnosticProvider,
        },
        (registerOptions, method) => (Predicate.isObject(registerOptions) ? Result.succeed({ id: method, method, registerOptions }) : Result.failVoid),
    );
    yield* SubscriptionRef.update(state, (current) => ({ ...current, registrations: HashMap.setMany(current.registrations, Record.toEntries(declared)) }));
    yield* notify(InitializedNotification.type, {});
    yield* Option.match(config.settings, { onNone: () => Effect.void, onSome: (settings) => notify(DidChangeConfigurationNotification.type, { settings }) });
    yield* Effect.addFinalizer(() =>
        Effect.tryPromise(() => connection.sendRequest(ShutdownRequest.type)).pipe(Effect.andThen(Effect.tryPromise(() => connection.sendNotification(ExitNotification.type))), Effect.andThen(child.exitCode), Effect.timeout(config.shutdownTimeout), Effect.ignore({ log: 'Warn', message: 'shutdown failed' })),
    );
    const synchronize = Effect.fn('synchronize')(function* (uri: string, languageId: string) {
        const text = yield* fs.readFileString(fileURLToPath(uri));
        return yield* SubscriptionRef.modifyEffect(
            state,
            Effect.fnUntraced(function* (current) {
                const previous = HashMap.get(current.documents, uri);
                if (Option.isSome(previous) && previous.value.item.text === text) {
                    return [previous.value.item, current] as const;
                }
                const item = { uri, languageId, text, version: Option.isSome(previous) ? previous.value.item.version + 1 : 1 };
                yield* Option.isSome(previous) ? announce(DidChangeTextDocumentNotification.type, current, { textDocument: { uri, version: item.version }, contentChanges: [{ text }] }) : announce(DidOpenTextDocumentNotification.type, current, { textDocument: item });
                return [item, { ...current, documents: HashMap.set(current.documents, uri, { item, begun: current.begun }), published: HashMap.remove(current.published, uri) }] as const;
            }),
        );
    });
    const close = (uri: string) => SubscriptionRef.modifyEffect(state, (current) => Effect.as(announce(DidCloseTextDocumentNotification.type, current, { textDocument: { uri } }), [undefined, { ...current, documents: HashMap.remove(current.documents, uri), published: HashMap.remove(current.published, uri) }] as const));
    const settled = (uri: string, current: typeof INITIAL) =>
        HashSet.isEmpty(current.progress) &&
        (registered(DocumentDiagnosticRequest.type, current).length > 0 || Option.exists(HashMap.get(current.documents, uri), ({ item, begun }) => (current.begun === 0 || current.begun > begun) && Option.exists(HashMap.get(current.published, uri), ({ version }) => version === undefined || version === item.version)));
    const pull = (uri: string, current: typeof INITIAL) =>
        Stream.fromEffect(
            Effect.forEach(
                registered(DocumentDiagnosticRequest.type, current),
                ({ identifier }) => ask(DocumentDiagnosticRequest.type, { textDocument: { uri }, ...(identifier === undefined ? {} : { identifier }) }).pipe(Effect.flatMap((report) => (report.kind === DocumentDiagnosticReportKind.Full ? Effect.succeed(report.items) : Effect.die(report)))),
                { concurrency: 'unbounded' },
            ),
        ).pipe(
            Stream.mapEffect((pulled) => Effect.map(SubscriptionRef.get(state), (latest) => [...pulled.flat(), ...Option.match(HashMap.get(latest.published, uri), { onNone: Array.empty, onSome: Struct.get('diagnostics') })])),
            Stream.catchIf(
                (error) => error instanceof ResponseError && error.code === LSPErrorCodes.ServerCancelled,
                () => Stream.empty,
            ),
        );
    const forward = Effect.fn('forward')(function* (events: readonly Event[]) {
        const kinds = { create: [FileChangeType.Created, WatchKind.Create], update: [FileChangeType.Changed, WatchKind.Change], delete: [FileChangeType.Deleted, WatchKind.Delete] } as const;
        const changes = Array.dedupe(
            Array.cartesian(events, registered(DidChangeWatchedFilesNotification.type, yield* SubscriptionRef.get(state)).flatMap(Struct.get('watchers'))).flatMap(([{ path, type }, { globPattern, kind }]) => {
                const [change, mask] = kinds[type];
                const [base, pattern] = typeof globPattern === 'string' ? [folder, globPattern] : [fileURLToPath(typeof globPattern.baseUri === 'string' ? globPattern.baseUri : globPattern.baseUri.uri), globPattern.pattern];
                return (kind === undefined || (kind & mask) !== 0) && minimatch(path, resolve(escape(base, { magicalBraces: true }), pattern), { dot: true }) ? [{ uri: pathToFileURL(path).href, type: change }] : [];
            }),
        );
        yield* changes.length === 0 ? Effect.void : notify(DidChangeWatchedFilesNotification.type, { changes });
    });
    const watching = yield* workspace.pipe(Stream.runForEach(forward), Effect.forkScoped);
    return {
        stopped: Effect.raceFirst(exited, Fiber.join(watching)),
        ask,
        open: (uri: string, languageId: string) =>
            Stream.fromIterableEffect(Effect.map(SubscriptionRef.get(state), (current) => HashMap.values(current.documents))).pipe(
                Stream.mapEffect(({ item }) => synchronize(item.uri, item.languageId).pipe(Effect.catchTag('PlatformError', () => close(item.uri))), { concurrency: 'unbounded' }),
                Stream.runDrain,
                Effect.andThen(synchronize(uri, languageId)),
                Effect.raceFirst(exited),
            ),
        diagnose: Effect.fn('diagnose')(function* (item: TextDocumentItem) {
            const [saved] = registered(DidSaveTextDocumentNotification.type, yield* SubscriptionRef.get(state));
            yield* saved === undefined ? Effect.void : notify(DidSaveTextDocumentNotification.type, { textDocument: { uri: item.uri }, ...(saved.includeText === true ? { text: item.text } : {}) });
            return yield* SubscriptionRef.changes(state).pipe(
                Stream.changesWith((left, right) => left.invalidated === right.invalidated && settled(item.uri, left) === settled(item.uri, right)),
                Stream.switchMap((current) => (settled(item.uri, current) ? pull(item.uri, current) : Stream.empty)),
                Stream.take(1),
                Stream.flattenIterable,
                Stream.runCollect,
            );
        }, Effect.raceFirst(exited)),
    };
});

const initialize = Effect.fn('initialize')(function* () {
    const root = process.cwd();
    const listing = ['ls-files', '--others', '--ignored', '--exclude-standard', '--directory', '-z'];
    const ignored = yield* Effect.scoped(
        Effect.gen(function* () {
            const git = yield* ChildProcess.make('git', listing, { cwd: root });
            const [text, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(git.stdout)), git.exitCode], { concurrency: 'unbounded' });
            return code === 0 ? [...text.split('\0').filter((path) => path.length > 0), '.git/'] : yield* Exited.make({ command: 'git', args: listing, code });
        }),
    );
    const outermost = ignored.filter((path) => !ignored.some((other) => other !== path && other.endsWith('/') && path.startsWith(other)));
    const exclusionPaths = 8;
    const [excluded, matched] = Array.splitAt(
        Array.sortWith(outermost, (path) => [!path.endsWith('/'), path.split('/').length] as const, Order.Tuple([Order.Boolean, Order.Number])),
        exclusionPaths,
    );
    const workspace = yield* Stream.share(watch(root, [...excluded.map((path) => resolve(root, path)), ...matched.map((path) => `${escape(path.endsWith('/') ? path.slice(0, -1) : path, { magicalBraces: true })}{,/**}`)], resolve(yield* (yield* FileSystem.FileSystem).makeTempDirectoryScoped(), 'snapshot')), {
        capacity: 'unbounded',
    });
    const reference = /\$\{(?<name>\w+)\}/gu;
    const json = yield* Schema.encodeEffect(Schema.fromJsonString(Schema.Json))(lsp);
    const names = [...json.matchAll(reference)].flatMap(({ groups }) => groups?.name ?? []);
    const variables = Record.set(Record.getSomes(yield* Config.all(Record.fromIterableWith(names, (name) => [name, Config.option(Config.String(name))]))), 'CLAUDE_PROJECT_DIR', root);
    const servers = Record.toEntries(yield* Schema.decodeEffect(Schema.fromJsonString(Schema.Record(Schema.String, Server)))(json.replace(reference, (match, name: string) => Option.match(Record.get(variables, name), { onNone: () => match, onSome: (value) => JSON.stringify(value).slice(1, -1) }))));
    const scope = yield* Scope.fork(yield* Effect.scope, 'parallel');
    const running = yield* RcMap.make({
        lookup: (entry: (typeof servers)[number]) =>
            start(root, entry[1], workspace).pipe(
                Effect.annotateLogs({ language: entry[0] }),
                Effect.tap((server): Effect.Effect<unknown, never, Scope.Scope> => server.stopped.pipe(Effect.ignore({ log: 'Warn', message: 'language server stopped' }), Effect.andThen(Effect.forkIn(RcMap.invalidate(running, entry), scope)), Effect.forkScoped)),
            ),
        idleTimeToLive: '30 minutes',
    }).pipe(Scope.provide(scope));
    const languages = servers.flatMap((entry) => Record.collect(entry[1].extensionToLanguage, (extension, languageId) => ({ extension, entry, languageId })));
    const tool = <P extends Schema.Top & { readonly Type: { readonly file: string } }>(name: string, description: string, parameters: P, handle: (server: Effect.Success<ReturnType<typeof start>>, item: TextDocumentItem, given: P['Type']) => Effect.Effect<readonly string[], Error>) =>
        registerTool(
            Tool.make(name, { description, parameters, success: Lines }).annotate(Tool.Readonly, true).annotate(Tool.OpenWorld, false),
            Effect.fnUntraced(function* (given: P['Type']) {
                const language = yield* Unmatched.single(languages, 'language server for extension', Option.some(extname(given.file)), ({ extension }) => [extension]);
                const server = yield* RcMap.get(running, language.entry);
                return { lines: yield* handle(server, yield* server.open(pathToFileURL(resolve(root, given.file)).href, language.languageId), given) };
            }, Effect.scoped),
        );
    const location = (uri: string, { start: { line, character } }: Range) => `${URL.parse(uri)?.protocol === 'file:' ? relative(root, fileURLToPath(uri)) : uri}:${line + 1}:${character + 1}`;
    const point = (item: TextDocumentItem, given: typeof Position.Type) => ({ textDocument: { uri: item.uri }, position: Struct.omit(given, ['file']) });
    const locations = (targets: Definition | DefinitionLink[] | Location[] | null) => Array.ensure<Location | DefinitionLink>(targets ?? []).map((target) => (LocationLink.is(target) ? location(target.targetUri, target.targetSelectionRange) : location(target.uri, target.range)));
    const text = (content: Hover['contents'] | Diagnostic['message']) => Array.ensure(content).map((part) => (typeof part === 'string' ? part : part.value));
    const flattened = (message: Diagnostic['message']) => text(message).join('').replaceAll('\n', ' ');
    const severities = new Map(Record.collect(DiagnosticSeverity, (name, value) => [value, name.toLowerCase()] as const));
    const report = (item: TextDocumentItem) => (reported: readonly Diagnostic[]) => {
        const diagnostics = Array.dedupe(reported);
        const related = Array.filterMap(
            diagnostics.flatMap(({ relatedInformation }) => relatedInformation ?? []),
            ({ location: { uri, range }, message }) => (uri === item.uri ? Result.succeed({ range, message }) : Result.failVoid),
        );
        return Array.sortWith(
            diagnostics.filter(({ range, message }) => !related.some(Equal.equals({ range, message }))),
            ({ range }) => range.start,
            Order.Struct({ line: Order.Number, character: Order.Number }),
        ).map(({ severity, range, source, code, codeDescription, message, relatedInformation }) =>
            [
                `${location(item.uri, range)} ${severities.get(severity ?? DiagnosticSeverity.Error)} ${source ?? ''}${code === undefined ? '' : `(${code})`}: ${flattened(message)}${codeDescription === undefined ? '' : ` ${codeDescription.href}`}`,
                ...(relatedInformation ?? []).map((entry) => `  ${location(entry.location.uri, entry.location.range)} ${flattened(entry.message)}`),
            ].join('\n'),
        );
    };
    yield* Effect.all(
        [
            tool('diagnostics', 'Language server diagnostics as `path:line:column severity source(code): message` lines, related locations indented', File, (server, item) => Effect.map(server.diagnose(item), report(item))),
            tool('definition', 'Symbol definition locations as `path:line:column` lines', Position, (server, item, given) => Effect.map(server.ask(DefinitionRequest.type, point(item, given)), locations)),
            tool('references', 'Symbol reference and declaration locations as `path:line:column` lines', Position, (server, item, given) => Effect.map(server.ask(ReferencesRequest.type, { ...point(item, given), context: { includeDeclaration: true } }), locations)),
            tool('hover', 'Symbol type signature and documentation', Position, (server, item, given) => Effect.map(server.ask(HoverRequest.type, point(item, given)), (hover) => text(hover?.contents ?? []))),
        ],
        { discard: true },
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

main(plugin, { retention: Duration.hours(1), instructions: [] }, Layer.effectDiscard(initialize()));
