import { basename, extname, relative, resolve } from 'node:path';
import process from 'node:process';
import { PassThrough } from 'node:stream';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { NodeStream } from '@effect/platform-node';
import { type Event, getEventsSince, subscribe, writeSnapshot } from '@parcel/watcher';
import { main } from '@rasm/mcp/main';
import { READING, register, Unmatched } from '@rasm/mcp/server';
import { Array, type Cause, Config, Deferred, Duration, Effect, Equal, Fiber, FileSystem, Function, flow, HashMap, HashSet, Layer, Match, Number, Option, Order, Predicate, Queue, RcMap, Record, Schema, SchemaTransformation, Scope, Stream, Struct, SubscriptionRef } from 'effect';
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
    LSPErrorCodes,
    MarkupKind,
    type MessageConnection,
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
import plugin from './.codex-plugin/plugin.json' with { type: 'json' };
import lsp from './.lsp.json' with { type: 'json' };

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

const Timeout = Schema.Int.check(Schema.isGreaterThan(0)).pipe(Schema.decodeTo(Schema.Duration, SchemaTransformation.durationFromMillis));
const Server = Schema.Struct({
    command: Schema.String,
    args: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    env: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)),
    extensionToLanguage: Schema.Record(Schema.String, Schema.String),
    initializationOptions: Schema.optionalKey(Schema.Json),
    settings: Schema.optionalKey(Schema.Json),
    workspaceFolder: Schema.String.pipe(Schema.withDecodingDefaultKey(Effect.succeed('.'))),
    startupTimeout: Timeout.pipe(Schema.withDecodingDefaultTypeKey(Effect.succeed(Duration.infinity))),
    shutdownTimeout: Timeout.pipe(Schema.withDecodingDefaultTypeKey(Effect.succeed(Duration.infinity))),
    requestTimeout: Timeout.pipe(Schema.withDecodingDefaultTypeKey(Effect.succeed(Duration.minutes(1)))),
});
const File = Schema.Struct({ file: Schema.String.annotate({ description: 'File path, absolute or relative to project root' }) });
const Coordinate = Schema.Int.check(Schema.isGreaterThanOrEqualTo(1));
const Position = Schema.Struct({ ...File.fields, line: Coordinate.annotate({ description: '1-based line' }), column: Coordinate.annotate({ description: '1-based UTF-16 column' }) });
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
    const record = Effect.tryPromise(() => writeSnapshot(root, snapshot, scanned));
    return Stream.unwrap(
        Effect.as(
            record,
            Stream.callback<readonly [Error | null, readonly Event[]], Cause.UnknownError>((queue) =>
                Effect.acquireRelease(
                    Effect.tryPromise(() => subscribe(root, (error, events) => Queue.offerUnsafe(queue, [error, events]), { ignore })),
                    (subscription) => Effect.promise(() => subscription.unsubscribe()),
                ),
            ),
        ),
    ).pipe(Stream.mapEffect(([error, events]) => (error === null ? Effect.succeed(events) : Effect.tryPromise(() => getEventsSince(root, snapshot, scanned)).pipe(Effect.tap(() => record), Effect.map((missed) => [...events, ...missed])))));
};

const listen = (connection: MessageConnection, settings: Schema.Json | undefined, listening: Deferred.Deferred<void>) => {
    const setting = (section: string | undefined): unknown => (section?.split('.') ?? []).reduce((value: unknown, key) => (Predicate.hasProperty(value, key) ? value[key] : undefined), settings) ?? null;
    return Stream.callback<(current: typeof INITIAL) => typeof INITIAL>((queue) => {
        connection.onRequest(ConfigurationRequest.type, ({ items }) => items.map(flow(Struct.get('section'), setting)));
        connection.onRequest(RegistrationRequest.type, ({ registrations }) => {
            Queue.offerUnsafe(queue, (current) => ({ ...current, invalidated: current.invalidated + 1, registrations: HashMap.setMany(current.registrations, Array.zip(registrations.map(Struct.get('id')), registrations)) }));
        });
        connection.onRequest(UnregistrationRequest.type, ({ unregisterations }) => {
            Queue.offerUnsafe(queue, (current) => ({ ...current, invalidated: current.invalidated + 1, registrations: HashMap.removeMany(current.registrations, unregisterations.map(Struct.get('id'))) }));
        });
        connection.onRequest(DiagnosticRefreshRequest.type, () => {
            Queue.offerUnsafe(queue, (current) => ({ ...current, invalidated: current.invalidated + 1 }));
        });
        connection.onRequest(WorkDoneProgressCreateRequest.type, Function.constVoid);
        connection.onUnhandledProgress(({ token, value: { kind } }) => {
            Queue.offerUnsafe(queue, (current) =>
                kind === 'report' ? current : { ...current, invalidated: current.invalidated + 1, begun: current.begun + (kind === 'begin' ? 1 : 0), progress: kind === 'begin' ? HashSet.add(current.progress, token) : HashSet.remove(current.progress, token) },
            );
        });
        connection.onNotification(PublishDiagnosticsNotification.type, (params) => {
            Queue.offerUnsafe(queue, (current) => ({ ...current, published: HashMap.set(current.published, params.uri, params) }));
        });
        connection.listen();
        return Deferred.succeed(listening, undefined);
    });
};

const start = Effect.fn('start')(function* (root: string, config: typeof Server.Type, workspace: Stream.Stream<readonly Event[], Error>) {
    const fs = yield* FileSystem.FileSystem;
    const folder = resolve(root, config.workspaceFolder);
    const input = new PassThrough();
    const child = yield* ChildProcess.make(config.command, config.args, { cwd: folder, env: config.env, extendEnv: true, stdin: NodeStream.fromReadable({ evaluate: () => input }).pipe(Stream.orDie) });
    const exited = Effect.flatMap(child.exitCode, (code) => Effect.fail(Exited.make({ command: config.command, args: config.args, code })));
    yield* child.stderr.pipe(Stream.decodeText(), Stream.splitLines, Stream.runForEach(Effect.logInfo), Effect.ignore({ log: 'Warn', message: 'stderr read failed' }), Effect.forkScoped);
    const connection = yield* Effect.acquireRelease(
        Effect.sync(() => createMessageConnection(NodeStream.toReadableNever(child.stdout), input)),
        (opened) => Effect.sync(() => opened.dispose()),
    );
    const [state, listening] = yield* Effect.all([SubscriptionRef.make(INITIAL), Deferred.make<void>()]);
    yield* listen(connection, config.settings, listening).pipe(
        Stream.runForEach((update) => SubscriptionRef.update(state, update)),
        Effect.forkScoped,
    );
    yield* Deferred.await(listening);
    const registered = <RO>({ method }: RegistrationType<RO>, current: typeof INITIAL): readonly RO[] => Array.fromIterable(HashMap.values(current.registrations)).filter((entry) => entry.method === method).map(({ registerOptions }): RO => registerOptions);
    const classify = ({ cause }: Cause.UnknownError) =>
        Match.value(cause).pipe(
            Match.when(Match.instanceOf(ResponseError), (error) => Effect.fail(error)),
            Match.when(Match.instanceOf(ConnectionError), () => exited),
            Match.orElse((defect) => Effect.die(defect)),
        );
    const notify = <P>(type: NotificationType<P>, params: RequestParam<P>) => Effect.tryPromise(() => connection.sendNotification(type, params)).pipe(Effect.catchTag('UnknownError', classify));
    const announce = <P, RO>(type: ProtocolNotificationType<P, RO>, current: typeof INITIAL, params: RequestParam<P>) => (registered(type, current).length > 0 ? notify(type, params) : Effect.void);
    const ask = <P, R>(type: RequestType<P, R, unknown>, params: RequestParam<P>, timeout: Duration.Duration) =>
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
    const declared = Object.entries({
        [DidOpenTextDocumentNotification.method]: sync.openClose === true && {},
        [DidCloseTextDocumentNotification.method]: sync.openClose === true && {},
        [DidChangeTextDocumentNotification.method]: sync.change !== undefined && sync.change !== TextDocumentSyncKind.None && { syncKind: sync.change },
        [DidSaveTextDocumentNotification.method]: sync.save === true ? {} : sync.save,
        [DocumentDiagnosticRequest.method]: capabilities.diagnosticProvider,
    }).flatMap(([method, registerOptions]) => (Predicate.isObject(registerOptions) ? [[method, { id: method, method, registerOptions }] as const] : []));
    yield* SubscriptionRef.update(state, (current) => ({ ...current, registrations: HashMap.setMany(current.registrations, declared) }));
    yield* notify(InitializedNotification.type, {});
    yield* config.settings === undefined ? Effect.void : notify(DidChangeConfigurationNotification.type, { settings: config.settings });
    yield* Effect.addFinalizer(() =>
        Effect.tryPromise(() => connection.sendRequest(ShutdownRequest.type)).pipe(
            Effect.andThen(Effect.tryPromise(() => connection.sendNotification(ExitNotification.type))),
            Effect.andThen(child.exitCode),
            Effect.timeout(config.shutdownTimeout),
            Effect.ignore({ log: 'Warn', message: 'shutdown did not complete' }),
        ),
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
    const close = (uri: string) =>
        SubscriptionRef.modifyEffect(state, (current) =>
            Effect.as(announce(DidCloseTextDocumentNotification.type, current, { textDocument: { uri } }), [undefined, { ...current, documents: HashMap.remove(current.documents, uri), published: HashMap.remove(current.published, uri) }] as const),
        );
    const settled = (uri: string, current: typeof INITIAL) =>
        HashSet.isEmpty(current.progress) &&
        (registered(DocumentDiagnosticRequest.type, current).length > 0 ||
            Option.exists(HashMap.get(current.documents, uri), ({ item, begun }) => (current.begun === 0 || current.begun > begun) && Option.exists(HashMap.get(current.published, uri), ({ version }) => version === undefined || version === item.version)));
    const pull = (uri: string, current: typeof INITIAL) =>
        Stream.fromEffect(
            Effect.forEach(
                registered(DocumentDiagnosticRequest.type, current),
                ({ identifier }) =>
                    ask(DocumentDiagnosticRequest.type, { textDocument: { uri }, ...(identifier === undefined ? {} : { identifier }) }, config.requestTimeout).pipe(
                        Effect.flatMap((report) => (report.kind === DocumentDiagnosticReportKind.Full ? Effect.succeed(report.items) : Effect.die(report))),
                    ),
                { concurrency: 'unbounded' },
            ),
        ).pipe(
            Stream.mapEffect((pulled) => Effect.map(SubscriptionRef.get(state), (latest) => [...pulled.flat(), ...Option.match(HashMap.get(latest.published, uri), { onNone: Array.empty, onSome: Struct.get('diagnostics') })])),
            Stream.catchIf((error) => error instanceof ResponseError && error.code === LSPErrorCodes.ServerCancelled, () => Stream.empty),
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
        ask: <P, R>(type: RequestType<P, R, unknown>, params: RequestParam<P>) => ask(type, params, config.requestTimeout),
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

const query = <const Name extends string, P extends Schema.Struct.Fields>(name: Name, description: string, parameters: Schema.Struct<P>) => Tool.make(name, { description, parameters, success: Lines }).annotateMerge(READING);

const initialize = Effect.fn('initialize')(function* () {
    const fs = yield* FileSystem.FileSystem;
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
        Array.sortWith(outermost, (path) => [path.endsWith('/') ? 0 : 1, path.split('/').length] as const, Order.Tuple([Order.Number, Order.Number])),
        exclusionPaths,
    );
    const workspace = yield* Stream.share(
        watch(root, [...excluded.map((path) => resolve(root, path)), ...matched.map((path) => `${escape(path.replace(/\/$/u, ''), { magicalBraces: true })}{,/**}`)], resolve(yield* fs.makeTempDirectoryScoped(), 'snapshot')),
        { capacity: 'unbounded' },
    );
    const reference = /\$\{(?<name>\w+)\}/gu;
    const names = [...(yield* Schema.encodeEffect(Schema.fromJsonString(Schema.Json))(lsp)).matchAll(reference)].flatMap(({ groups }) => groups?.name ?? []);
    const variables = Record.set(Record.getSomes(yield* Config.all(Record.fromIterableWith(names, (name) => [name, Config.option(Config.String(name))]))), 'CLAUDE_PROJECT_DIR', root);
    const substitute = (value: Schema.Json): Schema.Json =>
        Match.value(value).pipe(
            Match.when(Match.string, (template) => template.replace(reference, (match, name: string) => Option.getOrElse(Record.get(variables, name), () => match))),
            Match.when(Array.isArray<Schema.Json>, (items) => items.map(substitute)),
            Match.when(Predicate.isObjectKeyword, Record.map(substitute)),
            Match.orElse(() => value),
        );
    const servers = Object.entries(yield* Schema.decodeUnknownEffect(Schema.Record(Schema.String, Server))(substitute(lsp)));
    const scope = yield* Scope.fork(yield* Effect.scope, 'parallel');
    const running = yield* RcMap.make({
        lookup: (entry: (typeof servers)[number]) =>
            start(root, entry[1], workspace).pipe(
                Effect.annotateLogs({ language: entry[0] }),
                Effect.tap((server): Effect.Effect<unknown, never, Scope.Scope> => server.stopped.pipe(Effect.ignore({ log: 'Warn', message: 'language server stopped' }), Effect.andThen(Effect.forkIn(RcMap.invalidate(running, entry), scope)), Effect.forkScoped)),
            ),
        idleTimeToLive: '30 minutes',
    }).pipe(Scope.provide(scope));
    const languages = new Map(servers.flatMap((entry) => Object.entries(entry[1].extensionToLanguage).map(([extension, languageId]) => [extension, { entry, languageId }] as const)));
    const serve = Effect.fn('serve')(function* (file: string) {
        const language = languages.get(extname(file));
        if (language === undefined) {
            return yield* Unmatched.make({ subject: 'language server', query: file, candidates: [...languages.keys()] });
        }
        const server = yield* RcMap.get(running, language.entry);
        return { server, item: yield* server.open(pathToFileURL(resolve(root, file)).href, language.languageId) };
    });
    const location = (uri: string, { start: { line, character } }: Range) => `${URL.parse(uri)?.protocol === 'file:' ? relative(root, fileURLToPath(uri)) : uri}:${line + 1}:${character + 1}`;
    const locations = (targets: Definition | DefinitionLink[] | Location[] | null) => ({ lines: [targets ?? []].flat().map((target) => ('targetUri' in target ? location(target.targetUri, target.targetSelectionRange) : location(target.uri, target.range))) });
    const point = (item: TextDocumentItem, { line, column }: typeof Position.Type) => ({ textDocument: { uri: item.uri }, position: { line: line - 1, character: column - 1 } });
    const flattened = (message: Diagnostic['message']) => (typeof message === 'string' ? message : message.value).replaceAll('\n', ' ');
    const severities = new Map(Object.entries(DiagnosticSeverity).map(([name, value]) => [value, name.toLowerCase()]));
    yield* Effect.all(
        [
            register(
                query('diagnostics', 'Language server diagnostics of a file, one `path:line:column severity source(code): message` line each with indented related locations', File),
                Effect.fnUntraced(function* ({ file }: typeof File.Type) {
                    const { server, item } = yield* serve(file);
                    const diagnostics = Array.dedupe(yield* server.diagnose(item));
                    const related = diagnostics.flatMap(({ relatedInformation }) => relatedInformation ?? []).flatMap(({ location: { uri, range }, message }) => (uri === item.uri ? [{ range, message }] : []));
                    return {
                        lines: Array.sortWith(
                            diagnostics.filter(({ range, message }) => !related.some(Equal.equals({ range, message }))),
                            ({ range: { start: position } }) => [position.line, position.character] as const,
                            Order.Tuple([Order.Number, Order.Number]),
                        ).map(({ severity, range, source, code, codeDescription, message, relatedInformation }) =>
                            [
                                `${location(item.uri, range)} ${severities.get(severity ?? DiagnosticSeverity.Error)} ${source ?? ''}${code === undefined ? '' : `(${code})`}: ${flattened(message)}${codeDescription === undefined ? '' : ` ${codeDescription.href}`}`,
                                ...(relatedInformation ?? []).map((entry) => `  ${location(entry.location.uri, entry.location.range)} ${flattened(entry.message)}`),
                            ].join('\n'),
                        ),
                    };
                }, Effect.scoped),
            ),
            register(
                query('definition', 'Definition locations of the symbol at a line and column, one `path:line:column` line each', Position),
                Effect.fnUntraced(function* (position: typeof Position.Type) {
                    const { server, item } = yield* serve(position.file);
                    return locations(yield* server.ask(DefinitionRequest.type, point(item, position)));
                }, Effect.scoped),
            ),
            register(
                query('references', 'Reference locations of the symbol at a line and column, declaration included, one `path:line:column` line each', Position),
                Effect.fnUntraced(function* (position: typeof Position.Type) {
                    const { server, item } = yield* serve(position.file);
                    return locations(yield* server.ask(ReferencesRequest.type, { ...point(item, position), context: { includeDeclaration: true } }));
                }, Effect.scoped),
            ),
            register(
                query('hover', 'Type signature and documentation of the symbol at a line and column', Position),
                Effect.fnUntraced(function* (position: typeof Position.Type) {
                    const { server, item } = yield* serve(position.file);
                    const hover = yield* server.ask(HoverRequest.type, point(item, position));
                    return { lines: [hover?.contents ?? []].flat().map((content) => (typeof content === 'string' ? content : content.value)) };
                }, Effect.scoped),
            ),
        ],
        { discard: true },
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

main(plugin, Layer.effectDiscard(initialize()));
