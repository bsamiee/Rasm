// biome-ignore-all lint/correctness/noNodejsModules: The Node LSP transport and HTTP server require native streams, server factories, process state, and file URL conversion.
import { execFile } from 'node:child_process';
import { createServer } from 'node:http';
import { basename, extname, relative, resolve } from 'node:path';
import process from 'node:process';
import { PassThrough } from 'node:stream';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { promisify } from 'node:util';
import { NodeHttpServer, NodeRuntime, NodeServices, NodeStream } from '@effect/platform-node';
import { subscribe } from '@parcel/watcher';
import { Array, Deferred, Duration, Effect, FileSystem, flow, Layer, Logger, Match, Option, Pool, Predicate, Record, Schema, Semaphore, Stream, String } from 'effect';
import { McpProtocol, McpServer, Tool, Toolkit } from 'effect/ai';
import { Command, Flag } from 'effect/cli';
import { HttpRouter } from 'effect/http';
import { ChildProcess } from 'effect/process';
import { readJsonFile } from 'nx/src/devkit-exports.js';
import {
    type CancellationToken,
    CancellationTokenSource,
    type ConfigurationParams,
    ConfigurationRequest,
    createProtocolConnection,
    type Definition,
    type DefinitionLink,
    DefinitionRequest,
    type Diagnostic,
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
    type ProtocolConnection,
    PublishDiagnosticsNotification,
    type Range,
    ReferencesRequest,
    type RelatedFullDocumentDiagnosticReport,
    ShutdownRequest,
    type TextDocumentPositionParams,
    TextDocumentSyncKind,
    WorkDoneProgressCreateRequest,
} from 'vscode-languageserver-protocol/node';
import { TextDocument } from 'vscode-languageserver-textdocument';
import { formatEdited, type WriterProject } from '../function-hooks/repository/format.ts';
import plugin from './.claude-plugin/plugin.json' with { type: 'json' };
import lsp from './.lsp.json' with { type: 'json' };
import { EditContext, nativeEdits, TranscriptFailed } from './edits.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface LanguageServer {
    readonly command: string;
    readonly args?: readonly string[] | undefined;
    readonly extensionToLanguage: Record.ReadonlyRecord<string, string>;
    readonly initializationOptions?: Schema.Json | undefined;
    readonly settings?: Schema.Json | undefined;
    readonly workspaceFolder?: string | undefined;
}
interface Language {
    readonly pool: Pool.Pool<Effect.Success<ReturnType<typeof start>>, ServerExited | RequestFailed>;
    readonly languageId: string;
}
interface Workspace {
    readonly fs: FileSystem.FileSystem;
    readonly languages: ReadonlyMap<string, Language>;
    readonly root: string;
    readonly reports: Map<string, { readonly version: Option.Option<number>; readonly revision: number; readonly diagnostics: readonly Diagnostic[] }>;
    readonly revisions: Map<string, number>;
    readonly edits: Map<string, typeof EditContext.Type>;
    readonly sessions: Map<string, Session>;
}
interface Session {
    readonly pending: Map<string, { readonly event: FileEvent; readonly language: Language }>;
    readonly formatting: Map<string, FileEvent>;
    readonly owned: Map<string, typeof EditContext.Type>;
    readonly delivered: Map<string, ReadonlySet<string>>;
    readonly delivery: Semaphore.Semaphore;
}

// --- [MODELS] --------------------------------------------------------------------------

const Document = Schema.Struct({ file: Schema.String.annotate({ description: 'File path, absolute or relative to project root' }) });
const Coordinate = Schema.Int.check(Schema.isGreaterThanOrEqualTo(1));
const Position = Schema.Struct({ ...Document.fields, line: Coordinate.annotate({ description: '1-based line' }), column: Coordinate.annotate({ description: '1-based UTF-16 column' }) });
const Feedback = Schema.Struct({ sessionId: Schema.NonEmptyString });
const Lines = Schema.Struct({ lines: Schema.Array(Schema.String) });
const HookOutput = Schema.Struct({ hookSpecificOutput: Schema.optionalKey(Schema.Struct({ hookEventName: Schema.Literal('PostToolUse'), additionalContext: Schema.String })) });
const StopOutput = Schema.Union([Schema.Struct({ decision: Schema.Literal('block'), reason: Schema.String, systemMessage: Schema.optionalKey(Schema.String) }), Schema.Record(Schema.String, Schema.Never)]);
const Port = Schema.Number.check(Schema.isBetween({ minimum: 1, maximum: 65_535 }));

// --- [ERRORS] --------------------------------------------------------------------------

class NoLanguageServer extends Schema.TaggedError<NoLanguageServer>()('NoLanguageServer', { file: Schema.String }) {}
class ReadFailed extends Schema.TaggedError<ReadFailed>()('ReadFailed', { cause: Schema.Defect() }) {}
class ServerExited extends Schema.TaggedError<ServerExited>()('ServerExited', { cause: Schema.Defect() }) {}
class RequestFailed extends Schema.TaggedError<RequestFailed>()('RequestFailed', { cause: Schema.Defect() }) {}
const Failure = Schema.Union([NoLanguageServer, ReadFailed, ServerExited, RequestFailed, TranscriptFailed]);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SERVERS]

const send = <R>(message: (token: CancellationToken) => Promise<R>): Effect.Effect<R, RequestFailed> =>
    Effect.tryPromise({
        try: async (signal) => {
            const source = new CancellationTokenSource();
            const cancel = (): void => source.cancel();
            signal.addEventListener('abort', cancel, { once: true });
            try {
                return await message(source.token);
            } finally {
                signal.removeEventListener('abort', cancel);
                source.dispose();
            }
        },
        catch: (cause) => new RequestFailed({ cause }),
    });
const start = Effect.fn('start')(function* (config: LanguageServer, root: string, reports: Workspace['reports'], revisions: Workspace['revisions'], ignore: string[]) {
    const folder = resolve(root, ...Array.fromNullishOr(config.workspaceFolder));
    const workspaceUri = pathToFileURL(folder).href;
    const input = new PassThrough();
    const options: ChildProcess.CommandOptions = { cwd: folder, stdin: NodeStream.fromReadable({ evaluate: () => input }).pipe(Stream.orDie), stderr: 'inherit' };
    const child = yield* (config.args === undefined ? ChildProcess.make(config.command, options) : ChildProcess.make(config.command, config.args, options)).pipe(Effect.mapError((cause) => new ServerExited({ cause })));
    const connection = createProtocolConnection(NodeStream.toReadableNever(child.stdout), input);
    yield* Effect.addFinalizer(() => Effect.sync(() => connection.dispose()));
    const documents = new Map<string, { readonly document: TextDocument; readonly revision: number }>();
    const results = new Map<string, RelatedFullDocumentDiagnosticReport>();
    const dirty = new Map<string, { readonly event: FileEvent; readonly languageId: string }>();
    const watchFailed = yield* Deferred.make<never, RequestFailed>();
    yield* Effect.acquireRelease(
        Effect.tryPromise({
            try: subscribe.bind(
                undefined,
                root,
                (error, events) => {
                    if (error !== null) {
                        Deferred.doneUnsafe(watchFailed, Effect.fail(new RequestFailed({ cause: error })));
                        return;
                    }
                    Array.forEach(events, (event) => {
                        const uri = pathToFileURL(event.path).href;
                        const opened = documents.get(uri);
                        if (opened !== undefined) {
                            revisions.set(event.path, (revisions.get(event.path) ?? 0) + 1);
                            dirty.set(uri, {
                                event: {
                                    uri,
                                    type: Match.value(event.type).pipe(
                                        Match.withReturnType<FileChangeType>(),
                                        Match.when('delete', () => FileChangeType.Deleted),
                                        Match.when('create', () => FileChangeType.Created),
                                        Match.when('update', () => FileChangeType.Changed),
                                        Match.exhaustive,
                                    ),
                                },
                                languageId: opened.document.languageId,
                            });
                        }
                    });
                },
                { ignore },
            ),
            catch: (cause) => new RequestFailed({ cause }),
        }),
        (subscription) => Effect.promise(() => subscription.unsubscribe()),
    );
    const exited = Effect.raceFirst(Effect.exit(child.exitCode).pipe(Effect.flatMap((cause) => Effect.fail(new ServerExited({ cause })))), Deferred.await(watchFailed));
    let version = 0;
    connection.onRequest(
        ConfigurationRequest.type,
        flow(
            ({ items }: ConfigurationParams) => items,
            Array.map(({ section }) =>
                Option.getOrNull(
                    Array.fromNullishOr(section)
                        .flatMap(String.split('.'))
                        .reduce((value, key) => value.pipe(Option.flatMap((entry) => (Array.isArray<Schema.Json>(entry) || typeof entry !== 'object' || entry === null ? Option.none() : Record.get<string, Schema.Json>(entry, key)))), Option.fromUndefinedOr(config.settings)),
                ),
            ),
        ),
    );
    connection.onRequest(WorkDoneProgressCreateRequest.type, () => undefined);
    connection.listen();
    const { capabilities } = yield* Effect.raceFirst(
        send((token) =>
            connection.sendRequest(
                InitializeRequest.type,
                {
                    processId: process.pid,
                    rootUri: workspaceUri,
                    workspaceFolders: [{ uri: workspaceUri, name: basename(folder) }],
                    initializationOptions: config.initializationOptions,
                    capabilities: { workspace: { configuration: true }, textDocument: { synchronization: { didSave: true }, diagnostic: {}, publishDiagnostics: { versionSupport: true }, hover: { contentFormat: ['plaintext'] } } },
                },
                token,
            ),
        ),
        exited,
    );
    const pull = Option.fromNullishOr(capabilities.diagnosticProvider);
    connection.onNotification(PublishDiagnosticsNotification.type, (params) => {
        const owned = documents.get(params.uri);
        if (Option.isNone(pull) && owned !== undefined && owned.revision === (revisions.get(fileURLToPath(params.uri)) ?? 0) && (params.version === undefined || params.version === owned.document.version)) {
            reports.set(params.uri, { version: Option.fromNullishOr(params.version), revision: owned.revision, diagnostics: params.diagnostics });
        }
    });
    yield* send(() => connection.sendNotification(InitializedNotification.type, {}));
    if (config.settings !== undefined) {
        yield* send(() => connection.sendNotification(DidChangeConfigurationNotification.type, { settings: config.settings }));
    }
    yield* Effect.addFinalizer(() => send((token) => connection.sendRequest(ShutdownRequest.type, token)).pipe(Effect.andThen(send(() => connection.sendNotification(ExitNotification.type))), Effect.ignore));
    const sync = typeof capabilities.textDocumentSync === 'number' ? { openClose: capabilities.textDocumentSync !== TextDocumentSyncKind.None, change: capabilities.textDocumentSync } : capabilities.textDocumentSync;
    return {
        connection,
        exited,
        documents,
        results,
        dirty,
        sync,
        pull,
        nextVersion: (): number => {
            version += 1;
            return version;
        },
    };
});
const languages = Effect.fn('languages')(function* (config: LanguageServer, root: string, reports: Workspace['reports'], revisions: Workspace['revisions'], ignore: string[]) {
    const scope = yield* Effect.scope;
    const pool: Pool.Pool<Effect.Success<ReturnType<typeof start>>, ServerExited | RequestFailed> = yield* Pool.makeWithTTL({
        acquire: start(config, root, reports, revisions, ignore).pipe(Effect.tap((server) => server.exited.pipe(Effect.ignore, Effect.andThen(Effect.sync(() => Array.forEach(server.documents.keys(), (uri) => reports.delete(uri)))), Effect.andThen(Pool.invalidate(pool, server)), Effect.forkIn(scope)))),
        min: 0,
        max: 1,
        timeToLive: Duration.infinity,
    });
    return Object.entries(config.extensionToLanguage).map(([extension, languageId]) => [extension, { pool, languageId }] as const);
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
    (root: string, uri: string, threshold: DiagnosticSeverity, version: Option.Option<number>) =>
    ({ severity, range, source, code, message }: Diagnostic): readonly string[] =>
        Object.entries(DiagnosticSeverity)
            .filter(([, value]) => value === (severity ?? DiagnosticSeverity.Error) && value <= threshold)
            .map(([name]) => `${location(root, uri, range)} ${name.toLowerCase()} ${source ?? ''}${code === undefined ? '' : `(${code})`}: ${(typeof message === 'string' ? message : message.value).replaceAll('\n', ' ')}${Option.isNone(version) ? ' [server publication without document version]' : ''}`);

// --- [QUERIES]

const synchronize = Effect.fn('synchronize')(function* (workspace: Workspace, server: Effect.Success<ReturnType<typeof start>>, file: string, languageId: string) {
    const path = resolve(workspace.root, file);
    const revision = workspace.revisions.get(path) ?? 0;
    const uri = pathToFileURL(path).href;
    const dirty = server.dirty.get(uri);
    const text = yield* workspace.fs.readFileString(path).pipe(Effect.mapError((cause) => new ReadFailed({ cause })));
    const previous = server.documents.get(uri)?.document;
    const document = previous?.getText() === text ? previous : TextDocument.create(uri, languageId, server.nextVersion(), text);
    server.documents.set(uri, { document, revision });
    if (previous !== document) {
        workspace.reports.delete(uri);
        if (previous === undefined && server.sync?.openClose === true) {
            yield* send(() => server.connection.sendNotification(DidOpenTextDocumentNotification.type, { textDocument: { uri, languageId, version: document.version, text } }));
        } else if (previous !== undefined && server.sync?.change !== undefined && server.sync.change !== TextDocumentSyncKind.None) {
            const contentChanges = server.sync.change === TextDocumentSyncKind.Incremental ? [{ range: { start: previous.positionAt(0), end: previous.positionAt(previous.getText().length) }, text }] : [{ text }];
            yield* send(() => server.connection.sendNotification(DidChangeTextDocumentNotification.type, { textDocument: { uri, version: document.version }, contentChanges }));
        }
        if (server.sync?.save !== undefined && server.sync.save !== false) {
            const contents = typeof server.sync.save === 'object' && server.sync.save.includeText === true ? { text } : {};
            yield* send(() => server.connection.sendNotification(DidSaveTextDocumentNotification.type, { textDocument: { uri }, ...contents }));
        }
    }
    const report = workspace.reports.get(uri);
    if (previous === document && report !== undefined) {
        workspace.reports.set(uri, { ...report, revision });
    }
    if (server.dirty.get(uri) === dirty) {
        server.dirty.delete(uri);
    }
    return { document, revision };
}, Effect.uninterruptible);
const close = Effect.fn('close')(function* (workspace: Workspace, server: Effect.Success<ReturnType<typeof start>>, uri: string) {
    const document = server.documents.get(uri);
    const dirty = server.dirty.get(uri);
    if (document !== undefined && server.sync?.openClose === true) {
        yield* send(() => server.connection.sendNotification(DidCloseTextDocumentNotification.type, { textDocument: { uri } }));
    }
    server.documents.delete(uri);
    server.results.delete(uri);
    if (server.dirty.get(uri) === dirty) {
        server.dirty.delete(uri);
    }
    workspace.reports.delete(uri);
}, Effect.uninterruptible);
const refresh = Effect.fn('refresh')(function* (workspace: Workspace, server: Effect.Success<ReturnType<typeof start>>, pending: ReadonlySet<string>) {
    const changes = [...server.dirty.values()].filter(({ event: { uri } }) => !pending.has(uri));
    if (changes.length === 0) {
        return [];
    }
    yield* send(() => server.connection.sendNotification(DidChangeWatchedFilesNotification.type, { changes: changes.map(({ event }) => event) }));
    return Array.getSomes(
        yield* Effect.validate(changes, ({ event: { uri, type }, languageId }) => (type === FileChangeType.Deleted ? close(workspace, server, uri).pipe(Effect.as(Option.none())) : synchronize(workspace, server, fileURLToPath(uri), languageId).pipe(Effect.map(Option.some))), { concurrency: 'unbounded' }).pipe(
            Effect.mapError((cause) => new RequestFailed({ cause })),
        ),
    );
});
const open = Effect.fn('open')(function* (workspace: Workspace, file: string, use: (server: Effect.Success<ReturnType<typeof start>>, document: TextDocument, revision: number) => Effect.Effect<readonly string[], RequestFailed>) {
    const { pool, languageId } = yield* Effect.fromNullishOr(workspace.languages.get(extname(file))).pipe(Effect.mapError(() => new NoLanguageServer({ file })));
    const server = yield* Pool.get(pool);
    const operation = refresh(workspace, server, new Set([pathToFileURL(resolve(workspace.root, file)).href])).pipe(
        Effect.andThen(synchronize(workspace, server, file, languageId)),
        Effect.flatMap(({ document, revision }) => use(server, document, revision)),
    );
    return { lines: yield* Effect.raceFirst(operation, server.exited) };
}, Effect.scoped);
const pull = Effect.fn('pull')(function* (workspace: Workspace, server: Effect.Success<ReturnType<typeof start>>, document: TextDocument, revision: number) {
    if (Option.isSome(server.pull)) {
        const previous = server.results.get(document.uri);
        const { identifier } = server.pull.value;
        const report = yield* send((token) =>
            server.connection.sendRequest(
                DocumentDiagnosticRequest.type,
                {
                    textDocument: { uri: document.uri },
                    ...(identifier === undefined ? {} : { identifier }),
                    ...(previous?.resultId === undefined ? {} : { previousResultId: previous.resultId }),
                },
                token,
            ),
        );
        const result = report.kind === DocumentDiagnosticReportKind.Full ? report : { ...(yield* Effect.fromNullishOr(previous).pipe(Effect.mapError(() => new RequestFailed({ cause: report })))), resultId: report.resultId };
        server.results.set(document.uri, result);
        if (revision === (workspace.revisions.get(fileURLToPath(document.uri)) ?? 0)) {
            workspace.reports.set(document.uri, { version: Option.some(document.version), revision, diagnostics: result.items });
        }
    }
});
const diagnose = Effect.fn('diagnose')((workspace: Workspace, file: string, threshold: DiagnosticSeverity) =>
    open(workspace, file, (server, document, revision) =>
        pull(workspace, server, document, revision).pipe(
            Effect.map(() =>
                Option.match(Option.fromNullishOr(workspace.reports.get(document.uri)).pipe(Option.filter((report) => report.revision === (workspace.revisions.get(fileURLToPath(document.uri)) ?? 0))), {
                    onNone: () => [`${relative(workspace.root, fileURLToPath(document.uri))}: awaiting diagnostic publication for document version ${document.version}`],
                    onSome: ({ version, diagnostics }) =>
                        Option.isNone(version) && diagnostics.length === 0 ? [`${relative(workspace.root, fileURLToPath(document.uri))}: empty server publication without document version; current document diagnostics unknown`] : diagnostics.flatMap(render(workspace.root, document.uri, threshold, version)),
                }),
            ),
        ),
    ),
);
const request = Effect.fn('request')(<R>(workspace: Workspace, { file, line, column }: typeof Position.Type, call: (connection: ProtocolConnection, params: TextDocumentPositionParams, token: CancellationToken) => Promise<R>, lines: (result: R) => readonly string[]) =>
    open(workspace, file, (server, document) => send((token) => call(server.connection, { textDocument: { uri: document.uri }, position: { line: line - 1, character: column - 1 } }, token)).pipe(Effect.map(lines))),
);

// --- [HOOKS]

const session = (workspace: Workspace, sessionId: string): Session => {
    const existing = workspace.sessions.get(sessionId);
    if (existing !== undefined) {
        return existing;
    }
    const created: Session = { pending: new Map(), formatting: new Map(), owned: new Map(), delivered: new Map(), delivery: Semaphore.makeUnsafe(1) };
    workspace.sessions.set(sessionId, created);
    return created;
};
const recordEdits = Effect.fn('recordEdits')(function* (workspace: Workspace, owned: Session, context: typeof EditContext.Type) {
    const events = yield* nativeEdits(workspace.fs, context);
    Array.forEach(events, (event) => {
        const path = fileURLToPath(event.uri);
        const revision = (workspace.revisions.get(path) ?? 0) + 1;
        workspace.revisions.set(path, revision);
        workspace.edits.set(event.uri, context);
        owned.owned.set(event.uri, context);
        if (event.type === FileChangeType.Deleted) {
            owned.formatting.delete(event.uri);
        } else {
            owned.formatting.set(event.uri, event);
        }
        const language = workspace.languages.get(extname(path));
        if (language !== undefined) {
            owned.pending.set(event.uri, { event, language });
        }
    });
});
const feedback = Effect.fn('feedback')(function* (workspace: Workspace, owned: Session) {
    const current = (uri: string): boolean => owned.owned.has(uri) && owned.owned.get(uri) === workspace.edits.get(uri);
    const pending = Array.filter(owned.pending.values(), ({ event }) => current(event.uri));
    const groups = Map.groupBy(pending, ({ language }) => language.pool);
    yield* Effect.validate(
        groups,
        Effect.fn(function* ([pool, queued]: [Language['pool'], readonly NonNullable<ReturnType<Session['pending']['get']>>[]]) {
            const server = yield* Pool.get(pool);
            const changes = queued.filter(({ event }) => current(event.uri));
            const refreshed = yield* refresh(workspace, server, new Set(changes.map(({ event }) => event.uri)));
            yield* send(() =>
                server.connection.sendNotification(DidChangeWatchedFilesNotification.type, {
                    changes: changes.map(({ event }) => event),
                }),
            ).pipe(
                Effect.andThen(
                    Effect.validate(
                        changes,
                        Effect.fn(function* ({ event, language }: NonNullable<ReturnType<Session['pending']['get']>>) {
                            const { uri } = event;
                            if (!current(uri)) {
                                return Option.none();
                            }
                            if (event.type === FileChangeType.Deleted) {
                                yield* close(workspace, server, uri);
                                owned.delivered.delete(uri);
                                return Option.none();
                            }
                            return Option.some(yield* synchronize(workspace, server, fileURLToPath(uri), language.languageId));
                        }, Effect.uninterruptible),
                        { concurrency: 'unbounded' },
                    ),
                ),
                Effect.flatMap((synchronized) => {
                    const affected =
                        Option.isSome(server.pull) && server.pull.value.interFileDependencies && changes.length + refreshed.length > 0 ? [...server.documents.values()].filter(({ document }) => current(document.uri)) : [...Array.getSomes(synchronized), ...refreshed.filter(({ document }) => current(document.uri))];
                    if (Option.isSome(server.pull)) {
                        Array.forEach(affected, ({ document }) => workspace.reports.delete(document.uri));
                    }
                    return Effect.validate(affected, ({ document, revision }) => pull(workspace, server, document, revision), { concurrency: 'unbounded', discard: true });
                }),
                Effect.andThen(
                    Effect.sync(() =>
                        Array.forEach(changes, ({ event }) => {
                            owned.pending.delete(event.uri);
                        }),
                    ),
                ),
                Effect.raceFirst(server.exited),
            );
        }, Effect.scoped),
        { concurrency: 'unbounded', discard: true },
    ).pipe(Effect.mapError((cause) => new RequestFailed({ cause })));
    const reports = Array.filter(workspace.reports, ([uri, report]) => current(uri) && report.revision === (workspace.revisions.get(fileURLToPath(uri)) ?? 0) && (Option.isSome(report.version) || report.diagnostics.length > 0));
    return reports.flatMap(([uri, { diagnostics, version }]) => {
        const lines = new Set(diagnostics.flatMap(render(workspace.root, uri, DiagnosticSeverity.Warning, version)));
        const previous = owned.delivered.get(uri);
        const added = [...lines].filter((line) => previous?.has(line) !== true);
        owned.delivered.set(uri, lines);
        return added;
    });
});
const format = Effect.fn('format')(function* (workspace: Workspace, project: WriterProject, owned: Session) {
    const events = Array.filter(owned.formatting.values(), ({ uri }) => owned.owned.get(uri) === workspace.edits.get(uri));
    const failures = yield* Effect.tryPromise({
        try: (signal) =>
            formatEdited(
                workspace.root,
                events.map(({ uri }) => fileURLToPath(uri)),
                signal,
                project,
            ),
        catch: (cause) => new RequestFailed({ cause }),
    });
    Array.forEach(events, (event) => {
        if (failures.length === 0) {
            owned.formatting.delete(event.uri);
        }
        const language = workspace.languages.get(extname(fileURLToPath(event.uri)));
        if (language !== undefined) {
            owned.pending.set(event.uri, { event, language });
        }
    });
    return yield* feedback(workspace, owned).pipe(
        Effect.matchEffect({
            onFailure: (error) =>
                Array.match(failures, {
                    onEmpty: () => Effect.fail(error),
                    onNonEmpty: (lines) => Effect.succeed({ decision: 'block' as const, reason: lines.join('\n'), systemMessage: `Diagnostic feedback failed: ${Schema.toFormatter(Failure)(error)}` }),
                }),
            onSuccess: (diagnostics) =>
                Effect.succeed(
                    Array.match([...failures, ...diagnostics], {
                        onEmpty: () => ({}),
                        onNonEmpty: (lines) => ({ decision: 'block' as const, reason: lines.join('\n') }),
                    }),
                ),
        }),
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
    query('feedback', 'PostToolUse hook records native session edits and delivers new errors and warnings for that session', EditContext, HookOutput),
    query('format', 'Stop hook formats native session edits through Nx writer targets and delivers new diagnostic facts', Feedback, StopOutput).annotate(Tool.Readonly, false),
);
const initialize = Effect.fn('initialize')(function* () {
    const root = process.cwd();
    const execute = promisify(execFile);
    const ignored = yield* Effect.validate(
        [
            { args: ['ls-files', '--others', '--ignored', '--exclude-standard', '--directory', '-z'], separator: '\0' },
            { args: ['rev-parse', '--path-format=absolute', '--git-dir', '--git-common-dir'], separator: '\n' },
        ],
        ({ args, separator }) => Effect.tryPromise((signal) => execute('git', args, { cwd: root, signal, maxBuffer: Number.POSITIVE_INFINITY })).pipe(Effect.map(({ stdout }) => stdout.split(separator).filter((path) => path.length > 0))),
        { concurrency: 'unbounded' },
    ).pipe(Effect.mapError((cause) => new RequestFailed({ cause })));
    const ignore = ignored.flat().map((path) => resolve(root, path));
    const {
        name: projectName,
        nx: {
            targets: {
                format: {
                    metadata: { writers },
                },
            },
        },
    } = readJsonFile<{ readonly name: string; readonly nx: { readonly targets: { readonly format: { readonly metadata: { readonly writers: WriterProject['writers'] } } } } }>(resolve(root, 'package.json'));
    const project: WriterProject = { name: projectName, writers };
    const variables: NodeJS.Dict<string> = {
        ...process.env,
        ...Record.singleton('CLAUDE_PROJECT_DIR', root),
        ...Record.mapEntries(plugin.userConfig, (option, key) => [`user_config.${key}`, option.default]),
    };
    const replace = (value: string): string => value.replace(/\$\{(?<name>[\w.]+)\}/gu, (match, name: string) => variables[name] ?? match);
    const substitute = (value: Schema.Json): Schema.Json =>
        Match.value(value).pipe(
            Match.when(Match.string, replace),
            Match.when(Array.isArray<Schema.Json>, (values) => values.map(substitute)),
            Match.when(Predicate.isObjectKeyword, Record.map(substitute)),
            Match.orElse(() => value),
        );
    const configs = Object.values(lsp).map((config: LanguageServer) => ({
        ...config,
        command: replace(config.command),
        args: config.args?.map(replace),
        extensionToLanguage: Record.map(config.extensionToLanguage, replace),
        initializationOptions: config.initializationOptions === undefined ? undefined : substitute(config.initializationOptions),
        settings: config.settings === undefined ? undefined : substitute(config.settings),
        workspaceFolder: config.workspaceFolder === undefined ? undefined : replace(config.workspaceFolder),
    }));
    const reports: Workspace['reports'] = new Map();
    const revisions: Workspace['revisions'] = new Map();
    const workspace: Workspace = {
        fs: yield* FileSystem.FileSystem,
        languages: new Map((yield* Effect.forEach(configs, (config) => languages(config, root, reports, revisions, ignore))).flat()),
        root,
        reports,
        revisions,
        edits: new Map(),
        sessions: new Map(),
    };
    return {
        diagnostics: ({ file }: typeof Document.Type) => diagnose(workspace, file, DiagnosticSeverity.Hint),
        definition: (position: typeof Position.Type) => request(workspace, position, (connection, params, token) => connection.sendRequest(DefinitionRequest.type, params, token), locations(root)),
        references: (position: typeof Position.Type) => request(workspace, position, (connection, params, token) => connection.sendRequest(ReferencesRequest.type, { ...params, context: { includeDeclaration: true } }, token), locations(root)),
        hover: (position: typeof Position.Type) =>
            request(
                workspace,
                position,
                (connection, params, token) => connection.sendRequest(HoverRequest.type, params, token),
                (hover) =>
                    Array.fromNullishOr(hover?.contents)
                        .flat()
                        .map((content) => (typeof content === 'string' ? content : content.value)),
            ),
        feedback: (context: typeof EditContext.Type) => {
            const owned = session(workspace, context.sessionId);
            return recordEdits(workspace, owned, context).pipe(
                Effect.andThen(feedback(workspace, owned)),
                owned.delivery.withPermit,
                Effect.map((lines) => (lines.length === 0 ? {} : { hookSpecificOutput: { hookEventName: 'PostToolUse' as const, additionalContext: `Workspace diagnostics:\n${lines.join('\n')}` } })),
            );
        },
        format: ({ sessionId }: typeof Feedback.Type) => {
            const owned = session(workspace, sessionId);
            return format(workspace, project, owned).pipe(owned.delivery.withPermit);
        },
    };
});
const handlers = tools.toLayer(initialize());

const endpoint = McpServer.toolkit(tools).pipe(
    Layer.provide(handlers),
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

Command.make(plugin.name, { port: Flag.Int('port').pipe(Flag.withSchema(Port), Flag.withDescription('Loopback port the Streamable HTTP endpoint listens on')) }, ({ port }) =>
    HttpRouter.serve(endpoint, { disableLogger: true }).pipe(Layer.provide(NodeHttpServer.layerServer(createServer, { host: '127.0.0.1', port })), Layer.launch),
).pipe(Command.run({ version: plugin.version }), Effect.provide(NodeServices.layer), Effect.provideService(Logger.LogToStderr, true), NodeRuntime.runMain);
