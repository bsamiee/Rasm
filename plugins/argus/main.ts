import { createServer } from 'node:http';
import { basename, dirname, extname, isAbsolute, relative, resolve } from 'node:path';
import process from 'node:process';
import { PassThrough } from 'node:stream';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { NodeHttpServer, NodeRuntime, NodeServices, NodeStream } from '@effect/platform-node';
import { type Event, getEventsSince, subscribe, writeSnapshot } from '@parcel/watcher';
import { Array, Cause, Duration, Effect, Equal, Exit, Fiber, FileSystem, HashMap, HashSet, Latch, Layer, Logger, Match, Option, Order, Pool, Predicate, Queue, Record, Runtime, Schema, Scope, Stream, Struct, SubscriptionRef } from 'effect';
import { McpProtocol, McpServer, Tool, Toolkit } from 'effect/ai';
import { Command, Flag } from 'effect/cli';
import { identity } from 'effect/Function';
import { HttpRouter } from 'effect/http';
import { ChildProcess } from 'effect/process';
import { minimatch } from 'minimatch';
import {
    type CancellationToken,
    CancellationTokenSource,
    ConfigurationRequest,
    createProtocolConnection,
    type Definition,
    type DefinitionLink,
    DefinitionRequest,
    type Diagnostic,
    DiagnosticRefreshRequest,
    type DiagnosticRelatedInformation,
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
    type FileEvent,
    HoverRequest,
    InitializedNotification,
    InitializeRequest,
    LSPErrorCodes,
    MarkupKind,
    type ProgressToken,
    PublishDiagnosticsNotification,
    type Range,
    ReferencesRequest,
    RegistrationRequest,
    type RelatedFullDocumentDiagnosticReport,
    ResponseError,
    ShutdownRequest,
    type TextDocumentItem,
    type TextDocumentPositionParams,
    TextDocumentSyncKind,
    UnregistrationRequest,
    WatchKind,
    WorkDoneProgress,
    WorkDoneProgressCreateRequest,
} from 'vscode-languageserver-protocol/node';
import plugin from './.codex-plugin/plugin.json' with { type: 'json' };
import lsp from './.lsp.json' with { type: 'json' };

// --- [TYPES] ---------------------------------------------------------------------------

type Server = Effect.Success<ReturnType<typeof start>>;
interface Language {
    readonly pool: Pool.Pool<Server, ServerExited | RequestFailed>;
    readonly languageId: string;
}
interface Workspace {
    readonly fs: FileSystem.FileSystem;
    readonly root: string;
    readonly observers: Set<(batch: Batch) => Effect.Effect<void, RequestFailed>>;
    readonly current: Latch.Latch;
    readonly watching: Effect.Effect<never, RequestFailed>;
    readonly languages: Map<string, Language>;
}
interface Batch {
    readonly events: readonly FileEvent[];
    readonly stale: boolean;
}
interface Opened extends TextDocumentItem {
    readonly synced: number;
}
interface Finding extends Diagnostic {
    readonly severity: DiagnosticSeverity;
    readonly relatedInformation: DiagnosticRelatedInformation[];
}
interface Publication {
    readonly sequence: number;
    readonly version: number | undefined;
    readonly findings: readonly Finding[];
}
interface Status {
    readonly sequence: number;
    readonly invalidated: number;
    readonly began: number;
    readonly active: HashSet.HashSet<ProgressToken>;
    readonly registrations: HashMap.HashMap<string, typeof Registration.Type>;
    readonly published: HashMap.HashMap<string, Publication>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const requestTimeout = 60_000;
const changeTypes = { create: FileChangeType.Created, update: FileChangeType.Changed, delete: FileChangeType.Deleted } as const;
const watchKinds = { [FileChangeType.Created]: WatchKind.Create, [FileChangeType.Changed]: WatchKind.Change, [FileChangeType.Deleted]: WatchKind.Delete } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Timeout = Schema.Int.check(Schema.isGreaterThan(0));
const LanguageServer = Schema.Struct({
    command: Schema.String,
    args: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    env: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)),
    extensionToLanguage: Schema.Record(Schema.String, Schema.String),
    initializationOptions: Schema.optionalKey(Schema.Json),
    settings: Schema.OptionFromOptionalKey(Schema.Json),
    workspaceFolder: Schema.OptionFromOptionalKey(Schema.String),
    startupTimeout: Schema.OptionFromOptionalKey(Timeout),
    shutdownTimeout: Schema.OptionFromOptionalKey(Timeout),
    requestTimeout: Timeout.pipe(Schema.withDecodingDefaultKey(Effect.succeed(requestTimeout))),
});
const Registration = Schema.Union([
    Schema.Struct({
        id: Schema.String,
        method: Schema.Literal(DidChangeWatchedFilesNotification.method),
        registerOptions: Schema.Struct({
            watchers: Schema.Array(
                Schema.Struct({
                    globPattern: Schema.Union([Schema.String, Schema.Struct({ baseUri: Schema.Union([Schema.String, Schema.Struct({ uri: Schema.String })]), pattern: Schema.String })]),
                    kind: Schema.OptionFromOptionalKey(Schema.Int),
                }),
            ),
        }),
    }),
    Schema.Struct({ id: Schema.String, method: Schema.Literal(DocumentDiagnosticRequest.method), registerOptions: Schema.Struct({ identifier: Schema.optionalKey(Schema.String) }) }),
    Schema.Struct({ id: Schema.String, method: Schema.Literal(DidSaveTextDocumentNotification.method), registerOptions: Schema.Struct({ includeText: Schema.optionalKey(Schema.Boolean) }) }),
]);
const Document = Schema.Struct({ file: Schema.String.annotate({ description: 'File path, absolute or relative to project root' }) });
const Coordinate = Schema.Int.check(Schema.isGreaterThanOrEqualTo(1));
const Position = Schema.Struct({ ...Document.fields, line: Coordinate.annotate({ description: '1-based line' }), column: Coordinate.annotate({ description: '1-based UTF-16 column' }) });
const Lines = Schema.Struct({ lines: Schema.Array(Schema.String) });

// --- [ERRORS] --------------------------------------------------------------------------

class NoLanguageServer extends Schema.TaggedError<NoLanguageServer, { readonly brand: unique symbol }>()('NoLanguageServer', { file: Schema.String }) {}
class ReadFailed extends Schema.TaggedError<ReadFailed, { readonly brand: unique symbol }>()('ReadFailed', { cause: Schema.Defect() }) {}
class ServerExited extends Schema.TaggedError<ServerExited, { readonly brand: unique symbol }>()('ServerExited', { cause: Schema.Defect() }) {}
class RequestFailed extends Schema.TaggedError<RequestFailed, { readonly brand: unique symbol }>()('RequestFailed', { cause: Schema.Defect() }) {}
const Failure = Schema.Union([NoLanguageServer, ReadFailed, ServerExited, RequestFailed]);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PROTOCOL]

const send = <R>(message: (token: CancellationToken) => Promise<R>): Effect.Effect<R, RequestFailed> =>
    Effect.acquireUseRelease(
        Effect.sync(() => new CancellationTokenSource()),
        (source) => Effect.tryPromise({ try: () => message(source.token), catch: (cause) => RequestFailed.make({ cause }) }).pipe(Effect.onInterrupt(() => Effect.sync(() => source.cancel()))),
        (source) => Effect.sync(() => source.dispose()),
    );
const fileEvent = ({ path, type }: Event): FileEvent => ({ uri: pathToFileURL(path).href, type: changeTypes[type] });
const finding = (diagnostic: Diagnostic): Finding => ({ ...diagnostic, severity: diagnostic.severity ?? DiagnosticSeverity.Error, relatedInformation: diagnostic.relatedInformation ?? [] });
const deadline = (duration: Duration.Input): (<A, E, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, E | RequestFailed, R>) =>
    Effect.timeoutOrElse({ duration, orElse: () => Effect.fail(RequestFailed.make({ cause: new Cause.TimeoutError(`no answer within ${Duration.format(Duration.fromInputUnsafe(duration))}`) })) });

// --- [SERVERS]

const start = Effect.fn('start')(function* (config: typeof LanguageServer.Type, workspace: Workspace) {
    const folder = resolve(workspace.root, ...Option.toArray(config.workspaceFolder));
    const workspaceFolder = { uri: pathToFileURL(folder).href, name: basename(folder) };
    const input = new PassThrough();
    const child = yield* ChildProcess.make(config.command, config.args, { cwd: folder, env: config.env, extendEnv: true, stdin: NodeStream.fromReadable({ evaluate: () => input }).pipe(Stream.orDie), detached: false, killSignal: 'SIGKILL' }).pipe(Effect.mapError((cause) => ServerExited.make({ cause })));
    yield* Stream.runForEach(child.stderr.pipe(Stream.decodeText(), Stream.splitLines), Effect.logInfo).pipe(Effect.ignore({ log: 'Warn', message: 'stderr read failed' }), Effect.forkScoped);
    const connection = yield* Effect.acquireRelease(
        Effect.sync(() => createProtocolConnection(NodeStream.toReadableNever(child.stdout), input)),
        (acquired) => Effect.sync(() => acquired.dispose()),
    );
    const status = yield* SubscriptionRef.make<Status>({ sequence: 0, invalidated: 0, began: 0, active: HashSet.empty(), registrations: HashMap.empty(), published: HashMap.empty() });
    const advance = (next: (current: Status) => Status): Effect.Effect<number> =>
        SubscriptionRef.modify(status, (current) => {
            const advanced = next({ ...current, sequence: current.sequence + 1 });
            return [advanced.sequence, advanced];
        });
    const run = Effect.runSyncWith(yield* Effect.context<never>());
    const observe = (next: (current: Status) => Status): void => run(Effect.asVoid(advance(next)));
    const registered = <M extends (typeof Registration.Type)['method']>(current: Status, method: M): Extract<typeof Registration.Type, { readonly method: M }>[] => [...HashMap.values(current.registrations)].filter((entry): entry is Extract<typeof Registration.Type, { readonly method: M }> => entry.method === method);
    const ask = <R>(message: (token: CancellationToken) => Promise<R>): Effect.Effect<R, RequestFailed> => send(message).pipe(deadline(config.requestTimeout));
    const documents = new Map<string, Opened>();
    const dirty = new Map<string, FileEvent & Pick<Opened, 'languageId'>>();
    const results = new Map<string, Map<string | undefined, RelatedFullDocumentDiagnosticReport>>();
    const receive = ({ events, stale }: Batch): Effect.Effect<void, RequestFailed> =>
        Effect.flatMap(SubscriptionRef.get(status), (current) => {
            [...(stale ? [...documents.keys()].map((uri): FileEvent => ({ uri, type: FileChangeType.Changed })) : []), ...events].forEach((event) => {
                const opened = documents.get(event.uri);
                if (opened !== undefined) {
                    dirty.set(event.uri, { ...event, languageId: opened.languageId });
                }
            });
            const watchers = registered(current, DidChangeWatchedFilesNotification.method).flatMap(({ registerOptions }) => registerOptions.watchers);
            const changes = events.filter(({ uri, type }) => {
                const path = fileURLToPath(uri);
                return watchers.some(
                    ({ globPattern, kind }) =>
                        Option.match(kind, { onNone: () => true, onSome: (mask) => (mask & watchKinds[type]) !== 0 }) &&
                        (typeof globPattern === 'string'
                            ? minimatch(isAbsolute(globPattern) ? path : relative(folder, path), globPattern, { dot: true })
                            : minimatch(relative(fileURLToPath(typeof globPattern.baseUri === 'string' ? globPattern.baseUri : globPattern.baseUri.uri), path), globPattern.pattern, { dot: true })),
                );
            });
            return changes.length === 0 ? Effect.void : send(() => connection.sendNotification(DidChangeWatchedFilesNotification.type, { changes }));
        });
    yield* Effect.acquireRelease(
        Effect.sync(() => workspace.observers.add(receive)),
        () => Effect.sync(() => workspace.observers.delete(receive)),
    );
    const exited = Effect.raceFirst(
        Effect.flatMap(Effect.exit(child.exitCode), (cause) => Effect.fail(ServerExited.make({ cause }))),
        workspace.watching,
    );
    connection.onRequest(ConfigurationRequest.type, ({ items }) =>
        items.map(({ section }) => Option.getOrNull((section?.split('.') ?? []).reduce((value, key) => Option.flatMap(value, (entry) => (Array.isArray<Schema.Json>(entry) || typeof entry !== 'object' || entry === null ? Option.none() : Record.get<string, Schema.Json>(entry, key))), config.settings))),
    );
    connection.onRequest(RegistrationRequest.type, ({ registrations }) =>
        observe((current) => ({ ...current, invalidated: current.sequence, registrations: Array.getSomes(registrations.map((entry) => Schema.decodeUnknownOption(Registration)(entry))).reduce((all, entry) => HashMap.set(all, entry.id, entry), current.registrations) })),
    );
    connection.onRequest(UnregistrationRequest.type, ({ unregisterations }) => observe((current) => ({ ...current, invalidated: current.sequence, registrations: unregisterations.reduce((all, { id }) => HashMap.remove(all, id), current.registrations) })));
    connection.onRequest(DiagnosticRefreshRequest.type, () => observe((current) => ({ ...current, invalidated: current.sequence })));
    connection.onRequest(WorkDoneProgressCreateRequest.type, ({ token }) => {
        const progress = connection.onProgress(WorkDoneProgress.type, token, ({ kind }): void => {
            observe((current) =>
                Match.value(kind).pipe(
                    Match.when('begin', () => ({ ...current, invalidated: current.sequence, began: current.sequence, active: HashSet.add(current.active, token) })),
                    Match.when('report', () => current),
                    Match.when('end', () => ({ ...current, invalidated: current.sequence, active: HashSet.remove(current.active, token) })),
                    Match.exhaustive,
                ),
            );
            if (kind === 'end') {
                progress.dispose();
            }
        });
    });
    connection.onNotification(PublishDiagnosticsNotification.type, ({ uri, version, diagnostics }) => observe((current) => ({ ...current, published: HashMap.set(current.published, uri, { sequence: current.sequence, version, findings: diagnostics.map(finding) }) })));
    connection.listen();
    const { capabilities } = yield* send((token) =>
        connection.sendRequest(
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
            token,
        ),
    ).pipe(deadline(Option.getOrElse(config.startupTimeout, () => Duration.infinity)), Effect.raceFirst(exited));
    const { openClose = false, change = TextDocumentSyncKind.None, save = false } = typeof capabilities.textDocumentSync === 'number' ? { openClose: capabilities.textDocumentSync !== TextDocumentSyncKind.None, change: capabilities.textDocumentSync } : (capabilities.textDocumentSync ?? {});
    yield* advance((current) => ({
        ...current,
        registrations: HashMap.fromIterable(
            (
                [
                    ...(capabilities.diagnosticProvider === undefined ? [] : [capabilities.diagnosticProvider]).map(({ identifier }) => ({ id: DocumentDiagnosticRequest.method, method: DocumentDiagnosticRequest.method, registerOptions: identifier === undefined ? {} : { identifier } })),
                    ...(save === false ? [] : [{ id: DidSaveTextDocumentNotification.method, method: DidSaveTextDocumentNotification.method, registerOptions: save === true || save.includeText === undefined ? {} : { includeText: save.includeText } }]),
                ] satisfies (typeof Registration.Type)[]
            ).map((entry) => [entry.id, entry] as const),
        ),
    }));
    yield* send(() => connection.sendNotification(InitializedNotification.type, {}));
    yield* Effect.transposeOption(Option.map(config.settings, (settings) => send(() => connection.sendNotification(DidChangeConfigurationNotification.type, { settings }))));
    yield* Effect.addFinalizer(() =>
        send((token) => connection.sendRequest(ShutdownRequest.type, token)).pipe(
            Effect.andThen(send(() => connection.sendNotification(ExitNotification.type))),
            Effect.andThen(child.exitCode),
            deadline(Option.getOrElse(config.shutdownTimeout, () => Duration.infinity)),
            Effect.ignore({ log: 'Warn', message: 'shutdown did not complete' }),
        ),
    );
    const close = Effect.fn('close')(function* (uri: string) {
        if (documents.has(uri) && openClose) {
            yield* send(() => connection.sendNotification(DidCloseTextDocumentNotification.type, { textDocument: { uri } }));
        }
        documents.delete(uri);
        dirty.delete(uri);
        results.delete(uri);
    });
    const synchronize = Effect.fn('synchronize')(function* (uri: string, languageId: string) {
        const text = yield* workspace.fs.readFileString(fileURLToPath(uri)).pipe(Effect.mapError((cause) => ReadFailed.make({ cause })));
        dirty.delete(uri);
        const previous = documents.get(uri);
        if (previous?.text === text) {
            return previous;
        }
        const document: Opened = { uri, languageId, version: (previous?.version ?? 0) + 1, text, synced: yield* advance(identity) };
        documents.set(uri, document);
        if (previous === undefined ? openClose : change !== TextDocumentSyncKind.None) {
            yield* send(() =>
                previous === undefined ? connection.sendNotification(DidOpenTextDocumentNotification.type, { textDocument: Struct.omit(document, ['synced']) }) : connection.sendNotification(DidChangeTextDocumentNotification.type, { textDocument: { uri, version: document.version }, contentChanges: [{ text }] }),
            );
        }
        yield* Effect.forEach(registered(yield* SubscriptionRef.get(status), DidSaveTextDocumentNotification.method).slice(0, 1), ({ registerOptions: { includeText } }) =>
            send(() => connection.sendNotification(DidSaveTextDocumentNotification.type, { textDocument: { uri }, ...(includeText === true ? { text } : {}) })),
        );
        return document;
    }, Effect.uninterruptible);
    const pull = (document: Opened, identifier: string | undefined): Effect.Effect<Option.Option<readonly Finding[]>, RequestFailed> => {
        const previous = results.get(document.uri)?.get(identifier);
        return ask((token) => connection.sendRequest(DocumentDiagnosticRequest.type, { textDocument: { uri: document.uri }, ...(identifier === undefined ? {} : { identifier }), ...(previous?.resultId === undefined ? {} : { previousResultId: previous.resultId }) }, token)).pipe(
            Effect.filterOrElse(
                (report): report is RelatedFullDocumentDiagnosticReport => report.kind === DocumentDiagnosticReportKind.Full,
                (report) => Effect.fromNullishOr(previous).pipe(Effect.mapBoth({ onFailure: () => RequestFailed.make({ cause: report }), onSuccess: (full) => ({ ...full, resultId: report.resultId }) })),
            ),
            Effect.tap((full) => Effect.sync(() => results.getOrInsertComputed(document.uri, () => new Map()).set(identifier, full))),
            Effect.map(({ items }) => Option.some(items.map(finding))),
            Effect.catchIf(
                ({ cause }) => cause instanceof ResponseError && cause.code === LSPErrorCodes.ServerCancelled,
                () => Effect.succeedNone,
            ),
        );
    };
    const collect = Effect.fnUntraced(function* (document: Opened, invalidated: number): Effect.fn.Return<readonly Finding[], RequestFailed> {
        const pushed = (current: Status): Option.Option<Publication> => Option.filter(HashMap.get(current.published, document.uri), ({ sequence, version }) => (version === undefined ? sequence > document.synced : version === document.version));
        yield* SubscriptionRef.changes(status).pipe(
            Stream.takeUntil((current) => current.invalidated >= invalidated && HashSet.isEmpty(current.active) && (registered(current, DocumentDiagnosticRequest.method).length > 0 || ((current.began === 0 || current.began > document.synced) && Option.isSome(pushed(current))))),
            Stream.runDrain,
        );
        const before = yield* SubscriptionRef.get(status);
        const pulled = Option.all(yield* Effect.forEach(registered(before, DocumentDiagnosticRequest.method), ({ registerOptions: { identifier } }) => pull(document, identifier), { concurrency: 'unbounded' }));
        const after = yield* SubscriptionRef.get(status);
        return yield* Option.match(
            Option.filter(pulled, () => after.invalidated === before.invalidated),
            {
                onNone: () => collect(document, Math.max(before.invalidated + 1, after.invalidated)),
                onSome: (reports) => Effect.succeed([...reports.flat(), ...Option.toArray(pushed(after)).flatMap(({ findings }) => findings)]),
            },
        );
    });
    return {
        connection,
        exited,
        ask,
        synchronize,
        refresh: Latch.await(workspace.current).pipe(
            Effect.andThen(Effect.suspend(() => Effect.validate([...dirty.values()], ({ uri, type, languageId }) => (type === FileChangeType.Deleted ? close(uri) : Effect.asVoid(synchronize(uri, languageId)))))),
            Effect.asVoid,
            Effect.mapError((cause) => RequestFailed.make({ cause })),
        ),
        diagnose: Effect.fn('diagnose')(function* (document: Opened) {
            const current = Array.dedupe(yield* collect(document, 0));
            const referenced = current.flatMap(({ relatedInformation }) => relatedInformation.filter(({ location: { uri } }) => uri === document.uri).map(({ location: { range }, message }) => ({ range, message })));
            return current
                .filter(({ range, message }) => !referenced.some(Equal.equals({ range, message })))
                .toSorted(Order.mapInput(Order.Tuple([Order.Number, Order.Number, Order.Number]), ({ range: { start: position }, severity }: Finding) => [position.line, position.character, severity] as const))
                .flatMap(render(workspace.root, document.uri));
        }),
    };
});

// --- [RENDERING]

const oneLine = (message: Diagnostic['message']): string => (typeof message === 'string' ? message : message.value).replaceAll('\n', ' ');
const render =
    (root: string, uri: string) =>
    ({ severity, range, source, code, codeDescription, message, relatedInformation }: Finding): readonly string[] =>
        Object.entries(DiagnosticSeverity)
            .filter(([, value]) => value === severity)
            .map(([name]) =>
                [
                    `${location(root, uri, range)} ${name.toLowerCase()} ${source ?? ''}${code === undefined ? '' : `(${code})`}: ${oneLine(message)}${codeDescription === undefined ? '' : ` ${codeDescription.href}`}`,
                    ...relatedInformation.map((related) => `  ${location(root, related.location.uri, related.location.range)} ${oneLine(related.message)}`),
                ].join('\n'),
            );
const location = (root: string, uri: string, { start: { line, character } }: Range): string => `${URL.parse(uri)?.protocol === 'file:' ? relative(root, fileURLToPath(uri)) : uri}:${line + 1}:${character + 1}`;
const locations =
    (root: string) =>
    (targets: Definition | DefinitionLink[] | null): readonly string[] =>
        [targets ?? []].flat().map((target) => ('targetUri' in target ? location(root, target.targetUri, target.targetSelectionRange) : location(root, target.uri, target.range)));

// --- [QUERIES]

const open = Effect.fn('open')(function* (workspace: Workspace, file: string, use: (server: Server, document: Opened) => Effect.Effect<readonly string[], RequestFailed>) {
    const { pool, languageId } = yield* Effect.fromNullishOr(workspace.languages.get(extname(file))).pipe(Effect.mapError(() => NoLanguageServer.make({ file })));
    const server = yield* Pool.get(pool);
    return {
        lines: yield* server.refresh.pipe(
            Effect.andThen(server.synchronize(pathToFileURL(resolve(workspace.root, file)).href, languageId)),
            Effect.flatMap((document) => use(server, document)),
            Effect.raceFirst(server.exited),
        ),
    };
}, Effect.scoped);
const request = Effect.fn('request')(<R>(workspace: Workspace, { file, line, column }: typeof Position.Type, call: (server: Server, params: TextDocumentPositionParams, token: CancellationToken) => Promise<R>, lines: (result: R) => readonly string[]) =>
    open(workspace, file, (server, { uri }) => server.ask((token) => call(server, { textDocument: { uri }, position: { line: line - 1, character: column - 1 } }, token)).pipe(Effect.map(lines))),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const query = <const Name extends string, P extends Schema.Constraint, S extends Schema.Constraint>(name: Name, description: string, parameters: P, success: S): Tool.Tool<Name, { readonly parameters: P; readonly success: S; readonly failure: typeof Failure; readonly failureMode: 'error' }> =>
    Tool.make(name, { description, parameters, success, failure: Failure }).annotate(Tool.Readonly, true).annotate(Tool.Destructive, false).annotate(Tool.Idempotent, true).annotate(Tool.OpenWorld, false);
const tools = Toolkit.make(
    query('diagnostics', 'Language server diagnostics of a file, one `path:line:column severity source(code): message` line each with indented related locations', Document, Lines),
    query('definition', 'Definition locations of the symbol at a line and column, one `path:line:column` line each', Position, Lines),
    query('references', 'Reference locations of the symbol at a line and column, declaration included, one `path:line:column` line each', Position, Lines),
    query('hover', 'Type signature and documentation of the symbol at a line and column', Position, Lines),
);
const initialize = Effect.fn('initialize')(function* () {
    const root = process.cwd();
    const fs = yield* FileSystem.FileSystem;
    const ignored = yield* Effect.validate(
        [
            { args: ['ls-files', '--others', '--ignored', '--exclude-standard', '--directory', '-z'], separator: '\0' },
            { args: ['rev-parse', '--path-format=absolute', '--git-dir', '--git-common-dir'], separator: '\n' },
        ],
        ({ args, separator }) =>
            Effect.scoped(Effect.flatMap(ChildProcess.make('git', args, { cwd: root }), (handle) => Effect.all({ text: Stream.mkString(Stream.decodeText(handle.stdout)), code: handle.exitCode }, { concurrency: 'unbounded' }))).pipe(
                Effect.filterOrFail(
                    ({ code }) => code === 0,
                    (cause) => RequestFailed.make({ cause }),
                ),
                Effect.map(({ text }) => text.split(separator).filter((path) => path.length > 0)),
            ),
        { concurrency: 'unbounded' },
    ).pipe(Effect.mapError((cause) => RequestFailed.make({ cause })));
    const variables: NodeJS.Dict<string> = { ...process.env, CLAUDE_PROJECT_DIR: root };
    const substitute = (value: Schema.Json): Schema.Json =>
        Match.value(value).pipe(
            Match.when(Match.string, (template) => template.replace(/\$\{(?<name>\w+)\}/gu, (match, name: string) => variables[name] ?? match)),
            Match.when(Array.isArray<Schema.Json>, (items) => items.map(substitute)),
            Match.when(Predicate.isObjectKeyword, Record.map(substitute)),
            Match.orElse(() => value),
        );
    const ignore = ignored.flat().map((path) => resolve(root, path));
    const snapshot = resolve(root, '.cache', plugin.name, 'watcher.snapshot');
    const scanned = { ignore: [...ignore, snapshot], backend: 'brute-force' as const };
    const record = fs.makeDirectory(dirname(snapshot), { recursive: true }).pipe(
        Effect.mapError((cause) => RequestFailed.make({ cause })),
        Effect.andThen(Effect.tryPromise({ try: () => writeSnapshot(root, snapshot, scanned), catch: (cause) => RequestFailed.make({ cause }) })),
    );
    const observers: Workspace['observers'] = new Set();
    const current = yield* Latch.make(true);
    const scope = yield* Scope.fork(yield* Effect.scope, 'parallel');
    const notify = (batch: Batch): Effect.Effect<void, RequestFailed> => Effect.forEach(observers, (receive) => receive(batch), { discard: true });
    yield* record;
    const watcher = yield* Stream.callback<Batch, RequestFailed>((queue) =>
        Effect.acquireRelease(Effect.tryPromise({ try: () => subscribe(root, (error, events) => Queue.offerUnsafe(queue, { events: events.map(fileEvent), stale: error !== null }), { ignore }), catch: (cause) => RequestFailed.make({ cause }) }), (subscription) => Effect.promise(() => subscription.unsubscribe())),
    ).pipe(
        Stream.runForEach((batch) =>
            batch.stale
                ? Latch.close(current).pipe(
                      Effect.andThen(Effect.tryPromise({ try: () => getEventsSince(root, snapshot, scanned), catch: (cause) => RequestFailed.make({ cause }) })),
                      Effect.tap(() => record),
                      Effect.flatMap((missed) => notify({ events: [...batch.events, ...missed.map(fileEvent)], stale: true })),
                      Effect.ensuring(Latch.open(current)),
                  )
                : notify(batch),
        ),
        Effect.forkIn(scope),
    );
    const workspace: Workspace = {
        fs,
        root,
        observers,
        current,
        watching: Effect.flatMap(Fiber.await(watcher), (cause) => Effect.fail(RequestFailed.make({ cause }))),
        languages: new Map(),
    };
    yield* Effect.forEach(
        Object.entries(yield* Schema.decodeUnknownEffect(Schema.Record(Schema.String, LanguageServer))(substitute(lsp))),
        Effect.fnUntraced(function* ([language, config]: readonly [string, typeof LanguageServer.Type]) {
            const pool: Language['pool'] = yield* Pool.makeWithTTL({
                acquire: start(config, workspace).pipe(
                    Effect.annotateLogs({ language }),
                    Effect.tap((server) => server.exited.pipe(Effect.ignore, Effect.andThen(Pool.invalidate(pool, server)), Effect.forkIn(scope))),
                ),
                min: 0,
                max: 1,
                timeToLive: '30 minutes',
            }).pipe(Scope.provide(scope));
            Object.entries(config.extensionToLanguage).forEach(([extension, languageId]) => {
                workspace.languages.set(extension, { pool, languageId });
            });
        }),
    );
    return tools.of({
        diagnostics: ({ file }) => open(workspace, file, (server, document) => server.diagnose(document)),
        definition: (position) => request(workspace, position, ({ connection }, params, token) => connection.sendRequest(DefinitionRequest.type, params, token), locations(root)),
        references: (position) => request(workspace, position, ({ connection }, params, token) => connection.sendRequest(ReferencesRequest.type, { ...params, context: { includeDeclaration: true } }, token), locations(root)),
        hover: (position) =>
            request(
                workspace,
                position,
                ({ connection }, params, token) => connection.sendRequest(HoverRequest.type, params, token),
                (hover) => [hover?.contents ?? []].flat().map((content) => (typeof content === 'string' ? content : content.value)),
            ),
    });
});

const endpoint = McpServer.toolkit(tools).pipe(
    Layer.provide(tools.toLayer(initialize())),
    Layer.provide(
        McpServer.layerHttp({
            name: plugin.name,
            path: '/',
            protocols: [McpProtocol.v2026_07_28, McpProtocol.v2025_11_25, McpProtocol.v2025_06_18],
            version: plugin.version,
        }),
    ),
);

Command.make(plugin.name, { port: Flag.Int('port').pipe(Flag.withSchema(Schema.Int.check(Schema.isBetween({ minimum: 1, maximum: 65_535 }))), Flag.withDescription('Loopback port the Streamable HTTP endpoint listens on')) }, ({ port }) =>
    HttpRouter.serve(endpoint, { disableLogger: true }).pipe(Layer.provide(NodeHttpServer.layerServer(createServer, { host: '127.0.0.1', port, disablePreemptiveShutdown: true })), Layer.launch),
).pipe(Command.run({ version: plugin.version }), Effect.provide(NodeServices.layer), Effect.provideService(Logger.LogToStderr, true), NodeRuntime.runMain({ teardown: (exit, onExit) => Runtime.defaultTeardown<unknown, unknown>(Exit.isFailure(exit) && Cause.hasInterruptsOnly(exit.cause) ? Exit.void : exit, onExit) }));
