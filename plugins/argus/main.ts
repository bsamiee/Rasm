import { execFile } from 'node:child_process';
import { createServer } from 'node:http';
import { basename, extname, relative, resolve } from 'node:path';
import process from 'node:process';
import { PassThrough } from 'node:stream';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { promisify } from 'node:util';
import { NodeHttpServer, NodeRuntime, NodeServices, NodeStream } from '@effect/platform-node';
import { subscribe } from '@parcel/watcher';
import { Array, Cache, Deferred, Duration, Effect, FileSystem, Layer, Logger, Match, MutableRef, Option, Pool, Predicate, Record, Schema, Semaphore, Stream, String, Struct } from 'effect';
import { McpProtocol, McpServer, Tool, Toolkit } from 'effect/ai';
import { Command, Flag } from 'effect/cli';
import { HttpRouter } from 'effect/http';
import { ChildProcess } from 'effect/process';
import { readJsonFile } from 'nx/src/devkit-exports.js';
import {
    type CancellationToken,
    CancellationTokenSource,
    ConfigurationRequest,
    createProtocolConnection,
    type Definition,
    type DefinitionLink,
    DefinitionRequest,
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
    type ProtocolConnection,
    PublishDiagnosticsNotification,
    type PublishDiagnosticsParams,
    type Range,
    ReferencesRequest,
    type RelatedFullDocumentDiagnosticReport,
    ResponseError,
    ShutdownRequest,
    type TextDocumentPositionParams,
    TextDocumentSyncKind,
    WorkDoneProgressCreateRequest,
} from 'vscode-languageserver-protocol/node';
import { TextDocument } from 'vscode-languageserver-textdocument';
import plugin from './.claude-plugin/plugin.json' with { type: 'json' };
import lsp from './.lsp.json' with { type: 'json' };
import { EditContext, nativeEdits, TranscriptFailed } from './edits.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Server = Effect.Success<ReturnType<typeof start>>;
interface Change {
    readonly event: FileEvent;
    readonly languageId: string;
}
interface Language {
    readonly pool: Pool.Pool<Server, ServerExited | RequestFailed>;
    readonly languageId: string;
}
interface Workspace {
    readonly fs: FileSystem.FileSystem;
    readonly root: string;
    readonly ignore: string[];
    readonly languages: Map<string, Language>;
    readonly reports: Map<string, { readonly pushed?: PublishDiagnosticsParams | undefined; readonly pulled?: PublishDiagnosticsParams | undefined }>;
    readonly stale: Set<string>;
    readonly edits: Map<string, string>;
    readonly sessions: Cache.Cache<string, Session>;
}
interface Session {
    readonly pending: Map<string, Change & Language>;
    readonly formatting: Map<string, FileEvent>;
    readonly delivered: Map<string, ReadonlySet<string>>;
    readonly delivery: Semaphore.Semaphore;
}

// --- [MODELS] --------------------------------------------------------------------------

const LanguageServer = Schema.Struct({
    command: Schema.String,
    args: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    extensionToLanguage: Schema.Record(Schema.String, Schema.String),
    initializationOptions: Schema.optionalKey(Schema.Json),
    settings: Schema.OptionFromOptionalKey(Schema.Json),
    workspaceFolder: Schema.OptionFromOptionalKey(Schema.String),
});
const Document = Schema.Struct({ file: Schema.String.annotate({ description: 'File path, absolute or relative to project root' }) });
const Coordinate = Schema.Int.check(Schema.isGreaterThanOrEqualTo(1));
const Position = Schema.Struct({ ...Document.fields, line: Coordinate.annotate({ description: '1-based line' }), column: Coordinate.annotate({ description: '1-based UTF-16 column' }) });
const Lines = Schema.Struct({ lines: Schema.Array(Schema.String) });

// --- [ERRORS] --------------------------------------------------------------------------

class NoLanguageServer extends Schema.TaggedError<NoLanguageServer>()('NoLanguageServer', { file: Schema.String }) {}
class ReadFailed extends Schema.TaggedError<ReadFailed>()('ReadFailed', { cause: Schema.Defect() }) {}
class ServerExited extends Schema.TaggedError<ServerExited>()('ServerExited', { cause: Schema.Defect() }) {}
class RequestFailed extends Schema.TaggedError<RequestFailed>()('RequestFailed', { cause: Schema.Defect() }) {}
const Failure = Schema.Union([NoLanguageServer, ReadFailed, ServerExited, RequestFailed, TranscriptFailed]);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SERVERS]

const send = <R>(message: (token: CancellationToken) => Promise<R>): Effect.Effect<R, RequestFailed> =>
    Effect.acquireUseRelease(
        Effect.sync(() => new CancellationTokenSource()),
        (source) => Effect.tryPromise({ try: () => message(source.token), catch: (cause) => new RequestFailed({ cause }) }).pipe(Effect.onInterrupt(() => Effect.sync(() => source.cancel()))),
        (source) => Effect.sync(() => source.dispose()),
    );
const start = Effect.fn('start')(function* (config: typeof LanguageServer.Type, workspace: Workspace) {
    const folder = resolve(workspace.root, ...Option.toArray(config.workspaceFolder));
    const input = new PassThrough();
    const child = yield* ChildProcess.make(config.command, config.args, { cwd: folder, stdin: NodeStream.fromReadable({ evaluate: () => input }).pipe(Stream.orDie), stderr: 'inherit' }).pipe(Effect.mapError((cause) => new ServerExited({ cause })));
    const connection = createProtocolConnection(NodeStream.toReadableNever(child.stdout), input);
    const documents = new Map<string, TextDocument>();
    const results = new Map<string, RelatedFullDocumentDiagnosticReport>();
    const dirty = new Map<string, Change>();
    const versions = MutableRef.make(0);
    const watchFailed = yield* Deferred.make<never, RequestFailed>();
    yield* Effect.addFinalizer(() =>
        Effect.sync(() => {
            connection.dispose();
            Array.forEach(documents.keys(), (uri) => workspace.reports.delete(uri));
        }),
    );
    yield* Effect.acquireRelease(
        Effect.tryPromise({
            try: () =>
                subscribe(
                    workspace.root,
                    (error, events) =>
                        error === null
                            ? Array.forEach(events, ({ path, type }) => {
                                  const opened = documents.get(pathToFileURL(path).href);
                                  if (opened !== undefined) {
                                      workspace.stale.add(opened.uri);
                                      dirty.set(opened.uri, { event: { uri: opened.uri, type: ({ create: FileChangeType.Created, update: FileChangeType.Changed, delete: FileChangeType.Deleted } as const)[type] }, languageId: opened.languageId });
                                  }
                              })
                            : Deferred.doneUnsafe(watchFailed, Effect.fail(new RequestFailed({ cause: error }))),
                    { ignore: workspace.ignore },
                ),
            catch: (cause) => new RequestFailed({ cause }),
        }),
        (subscription) => Effect.promise(() => subscription.unsubscribe()),
    );
    const exited = Effect.raceFirst(
        Effect.flatMap(Effect.exit(child.exitCode), (cause) => Effect.fail(new ServerExited({ cause }))),
        Deferred.await(watchFailed),
    );
    connection.onRequest(ConfigurationRequest.type, ({ items }) =>
        items.map(({ section }) =>
            Option.getOrNull(Array.reduce(Array.fromNullishOr(section).flatMap(String.split('.')), config.settings, (value, key) => Option.flatMap(value, (entry) => (Array.isArray<Schema.Json>(entry) || typeof entry !== 'object' || entry === null ? Option.none() : Record.get<string, Schema.Json>(entry, key))))),
        ),
    );
    connection.onRequest(WorkDoneProgressCreateRequest.type, () => undefined);
    connection.onNotification(PublishDiagnosticsNotification.type, (pushed) => {
        const opened = documents.get(pushed.uri);
        if (opened !== undefined && !workspace.stale.has(pushed.uri) && (pushed.version === undefined || pushed.version === opened.version)) {
            workspace.reports.set(pushed.uri, { ...workspace.reports.get(pushed.uri), pushed });
        }
    });
    connection.listen();
    const { capabilities } = yield* Effect.raceFirst(
        send((token) =>
            connection.sendRequest(
                InitializeRequest.type,
                {
                    processId: process.pid,
                    rootUri: pathToFileURL(folder).href,
                    workspaceFolders: [{ uri: pathToFileURL(folder).href, name: basename(folder) }],
                    initializationOptions: config.initializationOptions,
                    capabilities: { workspace: { configuration: true }, textDocument: { synchronization: { didSave: true }, diagnostic: {}, publishDiagnostics: { versionSupport: true }, hover: { contentFormat: ['plaintext'] } } },
                },
                token,
            ),
        ),
        exited,
    );
    yield* send(() => connection.sendNotification(InitializedNotification.type, {}));
    yield* Effect.transposeOption(Option.map(config.settings, (settings) => send(() => connection.sendNotification(DidChangeConfigurationNotification.type, { settings }))));
    yield* Effect.addFinalizer(() => send((token) => connection.sendRequest(ShutdownRequest.type, token)).pipe(Effect.andThen(send(() => connection.sendNotification(ExitNotification.type))), Effect.ignore));
    const { openClose = false, change = TextDocumentSyncKind.None, save = false } = typeof capabilities.textDocumentSync === 'number' ? { openClose: capabilities.textDocumentSync !== TextDocumentSyncKind.None, change: capabilities.textDocumentSync } : (capabilities.textDocumentSync ?? {});
    const diagnosticProvider = Option.fromUndefinedOr(capabilities.diagnosticProvider);
    const synchronize = Effect.fn('synchronize')(function* (uri: string, languageId: string) {
        const stale = workspace.stale.delete(uri);
        const text = yield* workspace.fs.readFileString(fileURLToPath(uri)).pipe(
            Effect.mapError((cause) => new ReadFailed({ cause })),
            Effect.tapError(() => Effect.sync(() => stale && workspace.stale.add(uri))),
        );
        const previous = documents.get(uri);
        if (previous?.getText() === text) {
            return previous;
        }
        const document = TextDocument.create(uri, languageId, MutableRef.incrementAndGet(versions), text);
        documents.set(uri, document);
        workspace.reports.delete(uri);
        if (previous === undefined ? openClose : change !== TextDocumentSyncKind.None) {
            yield* send(() =>
                previous === undefined
                    ? connection.sendNotification(DidOpenTextDocumentNotification.type, { textDocument: { uri, languageId, version: document.version, text } })
                    : connection.sendNotification(DidChangeTextDocumentNotification.type, {
                          textDocument: { uri, version: document.version },
                          contentChanges: change === TextDocumentSyncKind.Incremental ? [{ range: { start: previous.positionAt(0), end: previous.positionAt(previous.getText().length) }, text }] : [{ text }],
                      }),
            );
        }
        if (save !== false) {
            yield* send(() => connection.sendNotification(DidSaveTextDocumentNotification.type, { textDocument: { uri }, ...(save !== true && save.includeText === true ? { text } : {}) }));
        }
        return document;
    }, Effect.uninterruptible);
    return {
        connection,
        exited,
        documents,
        diagnosticProvider,
        synchronize,
        refresh: Effect.fn('refresh')(function* (extra: readonly Change[]) {
            const changes = [...new Map([...dirty, ...extra.map((pending) => [pending.event.uri, pending] as const)]).values()];
            if (changes.length > 0) {
                yield* send(() => connection.sendNotification(DidChangeWatchedFilesNotification.type, { changes: changes.map(({ event }) => event) }));
            }
            return Array.getSomes(
                yield* Effect.validate(
                    changes,
                    Effect.fnUntraced(function* ({ event: { uri, type }, languageId }: Change) {
                        const marked = dirty.get(uri);
                        const document = type === FileChangeType.Deleted ? Option.none() : Option.some(yield* synchronize(uri, languageId));
                        if (Option.isNone(document)) {
                            if (documents.has(uri) && openClose) {
                                yield* send(() => connection.sendNotification(DidCloseTextDocumentNotification.type, { textDocument: { uri } }));
                            }
                            documents.delete(uri);
                            results.delete(uri);
                            workspace.reports.delete(uri);
                        }
                        if (dirty.get(uri) === marked) {
                            dirty.delete(uri);
                        }
                        return document;
                    }, Effect.uninterruptible),
                    { concurrency: 'unbounded' },
                ).pipe(Effect.mapError((cause) => new RequestFailed({ cause }))),
            );
        }),
        pull: Effect.fn('pull')(function* (document: TextDocument) {
            const previous = results.get(document.uri);
            const result = yield* Option.match(diagnosticProvider, {
                onNone: () => Effect.succeedNone,
                onSome: ({ identifier }) =>
                    send((token) => connection.sendRequest(DocumentDiagnosticRequest.type, { textDocument: { uri: document.uri }, ...(identifier === undefined ? {} : { identifier }), ...(previous?.resultId === undefined ? {} : { previousResultId: previous.resultId }) }, token)).pipe(
                        Effect.flatMap((report) => (report.kind === DocumentDiagnosticReportKind.Full ? Effect.succeed(report) : Effect.fromNullishOr(previous).pipe(Effect.mapBoth({ onFailure: () => new RequestFailed({ cause: report }), onSuccess: (full) => ({ ...full, resultId: report.resultId }) })))),
                        Effect.tap((full) => Effect.sync(() => results.set(document.uri, full))),
                        Effect.map(Option.some),
                        Effect.catchIf(
                            ({ cause }) => cause instanceof ResponseError && cause.code === LSPErrorCodes.ServerCancelled,
                            () => Effect.succeedNone,
                        ),
                    ),
            });
            workspace.reports.set(document.uri, { ...workspace.reports.get(document.uri), pulled: Option.getOrUndefined(Option.map(result, ({ items }) => ({ uri: document.uri, version: document.version, diagnostics: items }))) });
        }),
    };
});

// --- [RENDERING]

const location = (root: string, uri: string, { start: { line, character } }: Range): string => `${URL.parse(uri)?.protocol === 'file:' ? relative(root, fileURLToPath(uri)) : uri}:${line + 1}:${character + 1}`;
const locations =
    (root: string) =>
    (targets: Definition | DefinitionLink[] | null): readonly string[] =>
        Array.fromNullishOr(targets)
            .flat()
            .map((target) => ('targetUri' in target ? location(root, target.targetUri, target.targetSelectionRange) : location(root, target.uri, target.range)));
const render =
    (root: string, threshold: DiagnosticSeverity) =>
    ({ uri, version, diagnostics }: PublishDiagnosticsParams): readonly string[] =>
        diagnostics.flatMap(({ severity, range, source, code, message }) =>
            Object.entries(DiagnosticSeverity)
                .filter(([, value]) => value === (severity ?? DiagnosticSeverity.Error) && value <= threshold)
                .map(([name]) => `${location(root, uri, range)} ${name.toLowerCase()} ${source ?? ''}${code === undefined ? '' : `(${code})`}: ${(typeof message === 'string' ? message : message.value).replaceAll('\n', ' ')}${version === undefined ? ' [server publication without document version]' : ''}`),
        );

// --- [QUERIES]

const open = Effect.fn('open')(function* (workspace: Workspace, file: string, use: (server: Server, document: TextDocument) => Effect.Effect<readonly string[], RequestFailed>) {
    const { pool, languageId } = yield* Effect.fromNullishOr(workspace.languages.get(extname(file))).pipe(Effect.mapError(() => new NoLanguageServer({ file })));
    const server = yield* Pool.get(pool);
    return {
        lines: yield* server.refresh([]).pipe(
            Effect.andThen(server.synchronize(pathToFileURL(resolve(workspace.root, file)).href, languageId)),
            Effect.flatMap((document) => use(server, document)),
            Effect.raceFirst(server.exited),
        ),
    };
}, Effect.scoped);
const request = Effect.fn('request')(<R>(workspace: Workspace, { file, line, column }: typeof Position.Type, call: (connection: ProtocolConnection, params: TextDocumentPositionParams, token: CancellationToken) => Promise<R>, lines: (result: R) => readonly string[]) =>
    open(workspace, file, (server, { uri }) => send((token) => call(server.connection, { textDocument: { uri }, position: { line: line - 1, character: column - 1 } }, token)).pipe(Effect.map(lines))),
);

// --- [HOOKS]

const feedback = Effect.fn('feedback')(function* (workspace: Workspace, sessionId: string, owned: Session, events: readonly FileEvent[]) {
    const owns = (uri: string): boolean => workspace.edits.get(uri) === sessionId;
    Array.forEach(events, (event) => {
        const language = workspace.languages.get(extname(event.uri));
        if (language !== undefined) {
            owned.pending.set(event.uri, { event, ...language });
        }
    });
    yield* Effect.validate(
        Map.groupBy(
            Array.filter(owned.pending.values(), ({ event }) => owns(event.uri)),
            ({ pool }) => pool,
        ),
        ([pool, queued]) =>
            Effect.flatMap(Pool.get(pool), (server) => {
                const changes = queued.filter(({ event }) => owns(event.uri));
                return server.refresh(changes).pipe(
                    Effect.flatMap((refreshed) =>
                        Effect.validate(
                            (Option.exists(server.diagnosticProvider, ({ interFileDependencies }) => interFileDependencies) && changes.length + refreshed.length > 0 ? [...server.documents.values()] : refreshed).filter(({ uri }) => owns(uri)),
                            server.pull,
                            { concurrency: 'unbounded', discard: true },
                        ),
                    ),
                    Effect.andThen(Effect.sync(() => Array.forEach(changes, ({ event }) => owned.pending.delete(event.uri)))),
                    Effect.raceFirst(server.exited),
                );
            }).pipe(Effect.scoped),
        { concurrency: 'unbounded', discard: true },
    ).pipe(Effect.mapError((cause) => new RequestFailed({ cause })));
    return [...workspace.reports].flatMap(([uri, { pushed, pulled }]) => {
        const found = owns(uri) && !workspace.stale.has(uri) ? [pushed, pulled].filter((report) => report !== undefined).filter(({ version, diagnostics }) => version !== undefined || diagnostics.length > 0) : [];
        const lines = new Set(found.flatMap(render(workspace.root, DiagnosticSeverity.Warning)));
        const previous = owned.delivered.get(uri);
        if (found.length > 0) {
            owned.delivered.set(uri, lines);
        }
        return [...lines].filter((line) => previous?.has(line) !== true);
    });
});
const format = Effect.fn('format')(function* (workspace: Workspace, project: string, sessionId: string, owned: Session) {
    const events = Array.filter(owned.formatting.values(), ({ uri }) => workspace.edits.get(uri) === sessionId);
    const failures: readonly string[] =
        events.length === 0
            ? []
            : yield* Effect.tryPromise({
                  try: (signal) =>
                      promisify(execFile)('nx', ['run', `${project}:format:files`, '--output-style=static', '--', ...events.map(({ uri }) => fileURLToPath(uri))], { cwd: workspace.root, signal, maxBuffer: Number.POSITIVE_INFINITY }).then(
                          (): readonly string[] => [],
                          (error: { readonly code?: unknown; readonly stdout: string; readonly stderr: string }) => (typeof error.code === 'number' ? [`${error.stdout}${error.stderr}`.trim()] : Promise.reject(error)),
                      ),
                  catch: (cause) => new RequestFailed({ cause }),
              });
    if (failures.length === 0) {
        Array.forEach(events, ({ uri }) => owned.formatting.delete(uri));
    }
    return yield* feedback(workspace, sessionId, owned, events).pipe(
        Effect.map((diagnostics) => Array.match([...failures, ...diagnostics], { onEmpty: () => ({}), onNonEmpty: (lines) => ({ decision: 'block' as const, reason: lines.join('\n') }) })),
        Effect.catchIf(
            () => failures.length > 0,
            (error) => Effect.succeed({ decision: 'block' as const, reason: failures.join('\n'), systemMessage: `Diagnostic feedback failed: ${Schema.toFormatter(Failure)(error)}` }),
        ),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const query = <const Name extends string, P extends Schema.Constraint, S extends Schema.Constraint>(name: Name, description: string, parameters: P, success: S): Tool.Tool<Name, { readonly parameters: P; readonly success: S; readonly failure: typeof Failure; readonly failureMode: 'error' }> =>
    Tool.make(name, { description, parameters, success, failure: Failure }).annotate(Tool.Readonly, true).annotate(Tool.Destructive, false).annotate(Tool.OpenWorld, false);
const tools = Toolkit.make(
    query('diagnostics', 'Errors, warnings, information, and hints the language server reports for a file, one `path:line:column severity source(code): message` line each', Document, Lines),
    query('definition', 'Definition locations of the symbol at a line and column, one `path:line:column` line each', Position, Lines),
    query('references', 'Reference locations of the symbol at a line and column, declaration included, one `path:line:column` line each', Position, Lines),
    query('hover', 'Type signature and documentation of the symbol at a line and column', Position, Lines),
    query('feedback', 'PostToolUse hook records native session edits and delivers new errors and warnings for that session', EditContext, Schema.Struct({ hookSpecificOutput: Schema.optionalKey(Schema.Struct({ hookEventName: Schema.Literal('PostToolUse'), additionalContext: Schema.String })) })),
    query(
        'format',
        'Stop hook formats native session edits through Nx writer targets and delivers new diagnostic facts',
        EditContext.mapFields(Struct.pick(['sessionId'])),
        Schema.Union([Schema.Struct({ decision: Schema.Literal('block'), reason: Schema.String, systemMessage: Schema.optionalKey(Schema.String) }), Schema.Record(Schema.String, Schema.Never)]),
    ).annotate(Tool.Readonly, false),
);
const initialize = Effect.fn('initialize')(function* () {
    const root = process.cwd();
    const ignored = yield* Effect.validate(
        [
            { args: ['ls-files', '--others', '--ignored', '--exclude-standard', '--directory', '-z'], separator: '\0' },
            { args: ['rev-parse', '--path-format=absolute', '--git-dir', '--git-common-dir'], separator: '\n' },
        ],
        ({ args, separator }) => Effect.tryPromise((signal) => promisify(execFile)('git', args, { cwd: root, signal, maxBuffer: Number.POSITIVE_INFINITY })).pipe(Effect.map(({ stdout }) => stdout.split(separator).filter((path) => path.length > 0))),
        { concurrency: 'unbounded' },
    ).pipe(Effect.mapError((cause) => new RequestFailed({ cause })));
    const { name: project } = readJsonFile<{ readonly name: string }>(resolve(root, 'package.json'));
    const variables: NodeJS.Dict<string> = { ...process.env, ...Record.singleton('CLAUDE_PROJECT_DIR', root) };
    const substitute = (value: Schema.Json): Schema.Json =>
        Match.value(value).pipe(
            Match.when(Match.string, (text) => text.replace(/\$\{(?<name>\w+)\}/gu, (match, name: string) => variables[name] ?? match)),
            Match.when(Array.isArray<Schema.Json>, (values) => values.map(substitute)),
            Match.when(Predicate.isObjectKeyword, Record.map(substitute)),
            Match.orElse(() => value),
        );
    const workspace: Workspace = {
        fs: yield* FileSystem.FileSystem,
        root,
        ignore: ignored.flat().map((path) => resolve(root, path)),
        languages: new Map(),
        reports: new Map(),
        stale: new Set(),
        edits: new Map(),
        sessions: yield* Cache.make<string, Session>({ capacity: Number.POSITIVE_INFINITY, lookup: () => Effect.map(Semaphore.make(1), (delivery) => ({ pending: new Map(), formatting: new Map(), delivered: new Map(), delivery })) }),
    };
    const scope = yield* Effect.scope;
    yield* Effect.forEach(
        Object.values(yield* Schema.decodeUnknownEffect(Schema.Record(Schema.String, LanguageServer))(substitute(lsp))),
        Effect.fnUntraced(function* (config: typeof LanguageServer.Type) {
            const pool: Language['pool'] = yield* Pool.makeWithTTL({
                acquire: start(config, workspace).pipe(Effect.tap((server) => server.exited.pipe(Effect.ignore, Effect.andThen(Pool.invalidate(pool, server)), Effect.forkIn(scope)))),
                min: 0,
                max: 1,
                timeToLive: Duration.infinity,
            });
            Array.forEach(Record.toEntries(config.extensionToLanguage), ([extension, languageId]) => workspace.languages.set(extension, { pool, languageId }));
        }),
    );
    return tools.of({
        diagnostics: ({ file }) =>
            open(workspace, file, (server, document) =>
                server.pull(document).pipe(
                    Effect.map(() => {
                        const { pushed, pulled } = workspace.stale.has(document.uri) ? {} : (workspace.reports.get(document.uri) ?? {});
                        const lines = [pushed, pulled].filter((report) => report !== undefined).flatMap(render(root, DiagnosticSeverity.Hint));
                        const path = relative(root, fileURLToPath(document.uri));
                        return Option.match(Option.fromUndefinedOr(Option.isSome(server.diagnosticProvider) ? pulled : pushed), {
                            onNone: () => [`${path}: awaiting diagnostic publication for document version ${document.version}`, ...lines],
                            onSome: ({ version, diagnostics }) => (version === undefined && diagnostics.length === 0 ? [`${path}: empty server publication without document version; current document diagnostics unknown`] : lines),
                        });
                    }),
                ),
            ),
        definition: (position) => request(workspace, position, (connection, params, token) => connection.sendRequest(DefinitionRequest.type, params, token), locations(root)),
        references: (position) => request(workspace, position, (connection, params, token) => connection.sendRequest(ReferencesRequest.type, { ...params, context: { includeDeclaration: true } }, token), locations(root)),
        hover: (position) =>
            request(
                workspace,
                position,
                (connection, params, token) => connection.sendRequest(HoverRequest.type, params, token),
                (hover) =>
                    Array.fromNullishOr(hover?.contents)
                        .flat()
                        .map((content) => (typeof content === 'string' ? content : content.value)),
            ),
        feedback: (context) =>
            Effect.flatMap(Cache.get(workspace.sessions, context.sessionId), (owned) =>
                nativeEdits(workspace.fs, context).pipe(
                    Effect.tap((events) =>
                        Effect.sync(() =>
                            Array.forEach(events, (event) => {
                                workspace.stale.add(event.uri);
                                workspace.edits.set(event.uri, context.sessionId);
                                if (event.type === FileChangeType.Deleted) {
                                    owned.formatting.delete(event.uri);
                                    owned.delivered.delete(event.uri);
                                } else {
                                    owned.formatting.set(event.uri, event);
                                }
                            }),
                        ),
                    ),
                    Effect.flatMap((events) => feedback(workspace, context.sessionId, owned, events)),
                    owned.delivery.withPermit,
                    Effect.map((lines) => (lines.length === 0 ? {} : { hookSpecificOutput: { hookEventName: 'PostToolUse' as const, additionalContext: `Workspace diagnostics:\n${lines.join('\n')}` } })),
                ),
            ),
        format: ({ sessionId }) => Effect.flatMap(Cache.get(workspace.sessions, sessionId), (owned) => format(workspace, project, sessionId, owned).pipe(owned.delivery.withPermit)),
    });
});

const endpoint = McpServer.toolkit(tools).pipe(
    Layer.provide(tools.toLayer(initialize())),
    Layer.provide(
        McpServer.layerHttp({
            instructions: "Read a symbol's definition, references, and type signature from its language server before changing it, and a file's diagnostics after",
            name: plugin.name,
            path: '/',
            protocols: [McpProtocol.v2026_07_28, McpProtocol.v2025_06_18],
            version: plugin.version,
        }),
    ),
);

Command.make(plugin.name, { port: Flag.Int('port').pipe(Flag.withSchema(Schema.Number.check(Schema.isBetween({ minimum: 1, maximum: 65_535 }))), Flag.withDescription('Loopback port the Streamable HTTP endpoint listens on')) }, ({ port }) =>
    HttpRouter.serve(endpoint, { disableLogger: true }).pipe(Layer.provide(NodeHttpServer.layerServer(createServer, { host: '127.0.0.1', port })), Layer.launch),
).pipe(Command.run({ version: plugin.version }), Effect.provide(NodeServices.layer), Effect.provideService(Logger.LogToStderr, true), NodeRuntime.runMain);
