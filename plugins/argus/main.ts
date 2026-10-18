import { basename, dirname, extname, isAbsolute, relative, resolve } from 'node:path';
import process from 'node:process';
import { PassThrough } from 'node:stream';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { NodeStream } from '@effect/platform-node';
import { type Event, getEventsSince, subscribe, writeSnapshot } from '@parcel/watcher';
import { READING } from '@rasm/mcp/link';
import { main, register } from '@rasm/mcp/server';
import { Array, Cause, Config, Duration, Effect, Equal, Fiber, FileSystem, Function, HashMap, HashSet, Latch, Layer, Match, Option, Order, Predicate, Queue, RcRef, Record, Schema, Scope, Stream, Struct, SubscriptionRef } from 'effect';
import { Tool } from 'effect/ai';
import { ChildProcess } from 'effect/process';
import { escape, minimatch } from 'minimatch';
import {
    type CancellationToken,
    CancellationTokenSource,
    ConfigurationRequest,
    ConnectionError,
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
    type Hover,
    HoverRequest,
    InitializedNotification,
    InitializeRequest,
    LSPErrorCodes,
    MarkupKind,
    type ProgressToken,
    type ProtocolConnection,
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
    type WorkDoneProgressCreateParams,
    WorkDoneProgressCreateRequest,
} from 'vscode-languageserver-protocol/node';
import plugin from './.codex-plugin/plugin.json' with { type: 'json' };
import lsp from './.lsp.json' with { type: 'json' };

// --- [TYPES] ---------------------------------------------------------------------------

interface Workspace {
    readonly fs: FileSystem.FileSystem;
    readonly root: string;
    readonly observers: Set<(batch: Batch) => Effect.Effect<void>>;
    readonly current: Latch.Latch;
    readonly watching: Effect.Effect<never>;
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
interface Status {
    readonly sequence: number;
    readonly invalidated: number;
    readonly began: number;
    readonly active: HashSet.HashSet<ProgressToken>;
    readonly registrations: HashMap.HashMap<string, typeof Registration.Type>;
    readonly published: HashMap.HashMap<string, { readonly sequence: number; readonly version: number | undefined; readonly findings: readonly Finding[] }>;
}

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
    requestTimeout: Timeout.pipe(Schema.withDecodingDefaultKey(Effect.succeed(Duration.toMillis(Duration.minutes(1))))),
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

class LanguageError extends Schema.TaggedError<LanguageError, { readonly brand: unique symbol }>()('LanguageError', {
    reason: Schema.TaggedUnion({
        Unsupported: { file: Schema.String },
        Unreadable: { file: Schema.String },
        Unstarted: { language: Schema.String },
        Exited: { language: Schema.String, code: Schema.OptionFromOptionalKey(Schema.Int) },
        Rejected: { code: Schema.Int, message: Schema.String, data: Schema.optionalKey(Schema.Json) },
        Unanswered: { timeout: Timeout },
    }),
}) {
    override get message(): string {
        return LanguageError.fields.reason.match(this.reason, {
            Unsupported: ({ file }) => `No language server reads ${file}`,
            Unreadable: ({ file }) => `${file} cannot be read`,
            Unstarted: ({ language }) => `The ${language} language server did not start`,
            Exited: ({ language, code }) => `The ${language} language server exited${Option.match(code, { onNone: () => '', onSome: (status) => ` with code ${status}` })}`,
            Rejected: ({ code, message }) => `The language server refused the request (${code}): ${message}`,
            Unanswered: ({ timeout }) => `The language server sent no answer within ${timeout} ms`,
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const location = (root: string, uri: string, { start: { line, character } }: Range): string => `${URL.parse(uri)?.protocol === 'file:' ? relative(root, fileURLToPath(uri)) : uri}:${line + 1}:${character + 1}`;

const start = Effect.fn('start')(function* (language: string, config: typeof LanguageServer.Type, workspace: Workspace) {
    const folder = resolve(workspace.root, ...Option.toArray(config.workspaceFolder));
    const workspaceFolder = { uri: pathToFileURL(folder).href, name: basename(folder) };
    const input = new PassThrough();
    const child = yield* ChildProcess.make(config.command, config.args, { cwd: folder, env: config.env, extendEnv: true, stdin: NodeStream.fromReadable({ evaluate: () => input }).pipe(Stream.orDie) }).pipe(Effect.mapError(() => LanguageError.make({ reason: { _tag: 'Unstarted', language } })));
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
    const exited = Effect.raceFirst(
        Effect.flatMap(Effect.option(child.exitCode), (code) => Effect.fail(LanguageError.make({ reason: { _tag: 'Exited', language, code } }))),
        workspace.watching,
    );
    const send = <R>(message: (token: CancellationToken) => Promise<R>) =>
        Effect.acquireUseRelease(
            Effect.sync(() => new CancellationTokenSource()),
            (source) => Effect.tryPromise({ try: () => message(source.token), catch: (cause) => (cause instanceof ResponseError ? LanguageError.make({ reason: { _tag: 'Rejected', ...cause.toJson() } }) : new Cause.UnknownError(cause)) }),
            (source) =>
                Effect.sync(() => {
                    source.cancel();
                    source.dispose();
                }),
        ).pipe(Effect.catchTag('UnknownError', ({ cause }) => (cause instanceof ConnectionError ? exited : Effect.die(cause))));
    const deadline = (timeout: number) => Effect.timeoutOrElse({ duration: timeout, orElse: () => Effect.fail(LanguageError.make({ reason: { _tag: 'Unanswered', timeout } })) });
    const ask = <R>(message: (token: CancellationToken) => Promise<R>) => send(message).pipe(deadline(config.requestTimeout));
    const finding = (diagnostic: Diagnostic): Finding => ({ ...diagnostic, severity: diagnostic.severity ?? DiagnosticSeverity.Error, relatedInformation: diagnostic.relatedInformation ?? [] });
    const watchKinds = { [FileChangeType.Created]: WatchKind.Create, [FileChangeType.Changed]: WatchKind.Change, [FileChangeType.Deleted]: WatchKind.Delete };
    const documents = new Map<string, Opened>();
    const dirty = new Map<string, Opened['languageId']>();
    const results = new Map<string, Map<string | undefined, RelatedFullDocumentDiagnosticReport>>();
    const receive = ({ events, stale }: Batch) => {
        const current = SubscriptionRef.getUnsafe(status);
        (stale ? [...documents.values()] : events.flatMap(({ uri }) => documents.get(uri) ?? [])).forEach(({ uri, languageId }) => {
            dirty.set(uri, languageId);
        });
        const watchers = registered(current, DidChangeWatchedFilesNotification.method).flatMap(({ registerOptions }) => registerOptions.watchers);
        const changes = events.filter(({ uri, type }) =>
            watchers.some(({ globPattern, kind }) => {
                const [base, pattern] = typeof globPattern === 'string' ? [folder, globPattern] : [fileURLToPath(typeof globPattern.baseUri === 'string' ? globPattern.baseUri : globPattern.baseUri.uri), globPattern.pattern];
                return (Option.isNone(kind) || (kind.value & watchKinds[type]) !== 0) && minimatch(fileURLToPath(uri), resolve(escape(base), pattern), { dot: true });
            }),
        );
        return changes.length === 0 ? Effect.void : Effect.ignore(send(() => connection.sendNotification(DidChangeWatchedFilesNotification.type, { changes })));
    };
    yield* Effect.acquireRelease(
        Effect.sync(() => workspace.observers.add(receive)),
        () => Effect.sync(() => workspace.observers.delete(receive)),
    );
    const setting = (section: string | undefined) => Option.getOrNull((section?.split('.') ?? []).reduce((value: Option.Option<unknown>, key) => value.pipe(Option.filter(Predicate.isObject), Option.flatMap(Record.get(key))), config.settings));
    const created = ({ token }: WorkDoneProgressCreateParams) => {
        const progress = connection.onProgress(WorkDoneProgress.type, token, ({ kind }) => {
            observe(
                (current) =>
                    ({
                        begin: { ...current, invalidated: current.sequence, began: current.sequence, active: HashSet.add(current.active, token) },
                        report: current,
                        end: { ...current, invalidated: current.sequence, active: HashSet.remove(current.active, token) },
                    })[kind],
            );
            if (kind === 'end') {
                progress.dispose();
            }
        });
    };
    connection.onRequest(ConfigurationRequest.type, ({ items }) => items.map(({ section }) => setting(section)));
    connection.onRequest(RegistrationRequest.type, ({ registrations }) => {
        const added = HashMap.fromIterable(registrations.flatMap((entry) => Option.toArray(Schema.decodeUnknownOption(Registration)(entry))).map((entry) => [entry.id, entry] as const));
        observe((current) => ({ ...current, invalidated: current.sequence, registrations: HashMap.union(current.registrations, added) }));
    });
    connection.onRequest(UnregistrationRequest.type, ({ unregisterations }) => {
        const removed = unregisterations.map(({ id }) => id);
        observe((current) => ({ ...current, invalidated: current.sequence, registrations: HashMap.removeMany(current.registrations, removed) }));
    });
    connection.onRequest(DiagnosticRefreshRequest.type, () => observe((current) => ({ ...current, invalidated: current.sequence })));
    connection.onRequest(WorkDoneProgressCreateRequest.type, created);
    connection.onNotification(PublishDiagnosticsNotification.type, ({ uri, version, diagnostics }) => observe((current) => ({ ...current, published: HashMap.set(current.published, uri, { sequence: current.sequence, version, findings: diagnostics.map(finding) }) })));
    connection.listen();
    const handshake = (token: CancellationToken) =>
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
        );
    const { capabilities } = yield* Option.match(config.startupTimeout, { onNone: () => send(handshake), onSome: (timeout) => send(handshake).pipe(deadline(timeout)) }).pipe(Effect.raceFirst(exited));
    const { openClose = false, change = TextDocumentSyncKind.None, save = false } = typeof capabilities.textDocumentSync === 'number' ? { openClose: capabilities.textDocumentSync !== TextDocumentSyncKind.None, change: capabilities.textDocumentSync } : (capabilities.textDocumentSync ?? {});
    const declared = HashMap.fromIterable(
        (
            [
                ...(capabilities.diagnosticProvider === undefined ? [] : [capabilities.diagnosticProvider]).map(({ identifier }) => ({ id: DocumentDiagnosticRequest.method, method: DocumentDiagnosticRequest.method, registerOptions: identifier === undefined ? {} : { identifier } })),
                ...(save === false ? [] : [{ id: DidSaveTextDocumentNotification.method, method: DidSaveTextDocumentNotification.method, registerOptions: save === true || save.includeText === undefined ? {} : { includeText: save.includeText } }]),
            ] satisfies (typeof Registration.Type)[]
        ).map((entry) => [entry.id, entry] as const),
    );
    yield* advance((current) => ({ ...current, registrations: declared }));
    yield* send(() => connection.sendNotification(InitializedNotification.type, {}));
    yield* Effect.transposeOption(Option.map(config.settings, (settings) => send(() => connection.sendNotification(DidChangeConfigurationNotification.type, { settings }))));
    yield* Effect.addFinalizer(() =>
        send((token) => connection.sendRequest(ShutdownRequest.type, token)).pipe(
            Effect.andThen(send(() => connection.sendNotification(ExitNotification.type))),
            Effect.andThen(child.exitCode),
            Effect.timeout(Option.getOrElse(config.shutdownTimeout, () => Duration.infinity)),
            Effect.ignore({ log: 'Warn', message: 'shutdown did not complete' }),
        ),
    );
    const close = Effect.fn('close')(function* (uri: string) {
        if (openClose) {
            yield* send(() => connection.sendNotification(DidCloseTextDocumentNotification.type, { textDocument: { uri } }));
        }
        documents.delete(uri);
        dirty.delete(uri);
        results.delete(uri);
    });
    const synchronize = Effect.fn('synchronize')(function* (uri: string, languageId: string) {
        const text = yield* workspace.fs.readFileString(fileURLToPath(uri));
        dirty.delete(uri);
        const previous = documents.get(uri);
        if (previous?.text === text) {
            return previous;
        }
        const document: Opened = { uri, languageId, version: (previous?.version ?? 0) + 1, text, synced: yield* advance(Function.identity) };
        documents.set(uri, document);
        if (previous === undefined ? openClose : change !== TextDocumentSyncKind.None) {
            yield* send(() =>
                previous === undefined ? connection.sendNotification(DidOpenTextDocumentNotification.type, { textDocument: Struct.omit(document, ['synced']) }) : connection.sendNotification(DidChangeTextDocumentNotification.type, { textDocument: { uri, version: document.version }, contentChanges: [{ text }] }),
            );
        }
        return document;
    }, Effect.uninterruptible);
    const pull = (document: Opened, identifier: string | undefined) => {
        const previous = results.get(document.uri)?.get(identifier);
        return ask((token) => connection.sendRequest(DocumentDiagnosticRequest.type, { textDocument: { uri: document.uri }, ...(identifier === undefined ? {} : { identifier }), ...(previous?.resultId === undefined ? {} : { previousResultId: previous.resultId }) }, token)).pipe(
            Effect.filterOrElse(
                (report): report is RelatedFullDocumentDiagnosticReport => report.kind === DocumentDiagnosticReportKind.Full,
                (report) => (previous === undefined ? Effect.die(report) : Effect.succeed({ ...previous, resultId: report.resultId })),
            ),
            Effect.tap((full) => Effect.sync(() => results.getOrInsert(document.uri, new Map()).set(identifier, full))),
            Effect.map(({ items }) => Option.some(items.map(finding))),
            Effect.catchIf(
                (failure) => failure.reason._tag === 'Rejected' && failure.reason.code === LSPErrorCodes.ServerCancelled,
                () => Effect.succeedNone,
            ),
        );
    };
    const collect = Effect.fnUntraced(function* (document: Opened, invalidated: number): Effect.fn.Return<readonly Finding[], LanguageError> {
        const pushed = (current: Status) => Option.filter(HashMap.get(current.published, document.uri), ({ sequence, version }) => (version === undefined ? sequence > document.synced : version === document.version));
        yield* SubscriptionRef.changes(status).pipe(
            Stream.takeUntil((current) => current.invalidated >= invalidated && HashSet.isEmpty(current.active) && (registered(current, DocumentDiagnosticRequest.method).length > 0 || ((current.began === 0 || current.began > document.synced) && Option.isSome(pushed(current))))),
            Stream.runDrain,
        );
        const before = yield* SubscriptionRef.get(status);
        const pulled = Option.all(yield* Effect.forEach(registered(before, DocumentDiagnosticRequest.method), ({ registerOptions: { identifier } }) => pull(document, identifier), { concurrency: 'unbounded' }));
        const after = yield* SubscriptionRef.get(status);
        if (Option.isNone(pulled) || after.invalidated !== before.invalidated) {
            return yield* collect(document, Math.max(before.invalidated + 1, after.invalidated));
        }
        return [...pulled.value.flat(), ...Option.toArray(Option.map(pushed(after), Struct.get('findings'))).flat()];
    });
    const severities = new Map(Object.entries(DiagnosticSeverity).map(([name, value]) => [value, name.toLowerCase()]));
    const oneLine = (message: Diagnostic['message']): string => (typeof message === 'string' ? message : message.value).replaceAll('\n', ' ');
    return {
        connection,
        exited,
        ask,
        synchronize,
        refresh: Latch.await(workspace.current).pipe(Effect.andThen(Effect.forEach(dirty, ([uri, languageId]) => synchronize(uri, languageId).pipe(Effect.catchTag('PlatformError', () => close(uri))), { concurrency: 'unbounded', discard: true }))),
        diagnose: Effect.fn('diagnose')(function* (document: Opened) {
            const [saved] = registered(yield* SubscriptionRef.get(status), DidSaveTextDocumentNotification.method);
            if (saved !== undefined) {
                yield* send(() => connection.sendNotification(DidSaveTextDocumentNotification.type, { textDocument: { uri: document.uri }, ...(saved.registerOptions.includeText === true ? { text: document.text } : {}) }));
            }
            const current = Array.dedupe(yield* collect(document, 0));
            const referenced = current
                .flatMap(({ relatedInformation }) => relatedInformation)
                .filter(({ location: { uri } }) => uri === document.uri)
                .map(({ location: { range }, message }) => ({ range, message }));
            const render = ({ severity, range, source, code, codeDescription, message, relatedInformation }: Finding): string =>
                [
                    `${location(workspace.root, document.uri, range)} ${severities.get(severity)} ${source ?? ''}${code === undefined ? '' : `(${code})`}: ${oneLine(message)}${codeDescription === undefined ? '' : ` ${codeDescription.href}`}`,
                    ...relatedInformation.map((related) => `  ${location(workspace.root, related.location.uri, related.location.range)} ${oneLine(related.message)}`),
                ].join('\n');
            return current
                .filter(({ range, message }) => !referenced.some(Equal.equals({ range, message })))
                .toSorted(Order.mapInput(Order.Tuple([Order.Number, Order.Number, Order.Number]), ({ range: { start: position }, severity }: Finding) => [position.line, position.character, severity] as const))
                .map(render);
        }),
    };
});

const query = <const Name extends string, P extends Schema.Struct.Fields>(name: Name, description: string, parameters: Schema.Struct<P>) => Tool.make(name, { description, parameters, success: Lines }).annotateMerge(READING);

const initialize = Effect.fn('initialize')(function* () {
    const fs = yield* FileSystem.FileSystem;
    const root = process.cwd();
    const ignored = yield* Effect.validate(
        [
            { args: ['ls-files', '--others', '--ignored', '--exclude-standard', '--directory', '-z'], separator: '\0' },
            { args: ['rev-parse', '--path-format=absolute', '--git-dir', '--git-common-dir'], separator: '\n' },
        ],
        ({ args, separator }) =>
            Effect.scoped(Effect.flatMap(ChildProcess.make('git', args, { cwd: root }), (handle) => Effect.all({ text: Stream.mkString(Stream.decodeText(handle.stdout)), code: handle.exitCode }, { concurrency: 'unbounded' }))).pipe(
                Effect.filterOrFail(
                    ({ code }) => code === 0,
                    ({ code }) => new Cause.UnknownError(code, `git ${args.join(' ')} exited with code ${code}`),
                ),
                Effect.map(({ text }) => text.split(separator)),
            ),
        { concurrency: 'unbounded' },
    );
    const reference = /\$\{(?<name>\w+)\}/gu;
    const references = [...(yield* Schema.encodeEffect(Schema.fromJsonString(Schema.Json))(lsp)).matchAll(reference)];
    const variables = new Map(Object.entries({ ...(yield* Config.all(Object.fromEntries(references.flatMap(({ 0: match, groups }) => (groups?.name === undefined ? [] : [[groups.name, Config.withDefault(Config.String(groups.name), match)] as const]))))), CLAUDE_PROJECT_DIR: root }));
    const substitute = (value: Schema.Json): Schema.Json =>
        Match.value(value).pipe(
            Match.when(Match.string, (template) => template.replace(reference, (match, name: string) => variables.get(name) ?? match)),
            Match.when(Array.isArray<Schema.Json>, (items) => items.map(substitute)),
            Match.when(Predicate.isObjectKeyword, Record.map(substitute)),
            Match.orElse(() => value),
        );
    const entries = Array.dedupeWith(
        ignored
            .flat()
            .filter((path) => path.length > 0)
            .map((path) => ({ path: relative(root, resolve(root, path)), directory: path.endsWith('/') || isAbsolute(path) })),
        (left, right) => left.path === right.path,
    );
    const outermost = entries.filter(({ path }) => !entries.some((other) => other.directory && path.startsWith(`${other.path}/`)));
    const exclusionLimit = 8;
    const [excluded, filtered] = Array.splitAt(
        Array.sortWith(outermost, ({ directory, path }) => [directory ? 0 : 1, path.split('/').length] as const, Order.Tuple([Order.Number, Order.Number])),
        exclusionLimit,
    );
    const ignore = [...excluded.map(({ path }) => resolve(root, path)), ...filtered.map(({ path }) => `${path.replaceAll(/[*?[\]{}()!@+|,\\]/gu, '\\$&')}{,/**}`)];
    const snapshot = resolve(root, '.cache', plugin.name, 'watcher.snapshot');
    const scanned = { ignore: [...ignore, snapshot], backend: 'brute-force' as const };
    const record = fs.makeDirectory(dirname(snapshot), { recursive: true }).pipe(Effect.andThen(Effect.tryPromise(() => writeSnapshot(root, snapshot, scanned))));
    const fileEvent = ({ path, type }: Event): FileEvent => ({ uri: pathToFileURL(path).href, type: ({ create: FileChangeType.Created, update: FileChangeType.Changed, delete: FileChangeType.Deleted } as const)[type] });
    const observers: Workspace['observers'] = new Set();
    const [current, scope, batches] = yield* Effect.all([Latch.make(true), Scope.fork(yield* Effect.scope, 'parallel'), Queue.unbounded<Batch>()]);
    const notify = (batch: Batch) => Effect.forEach(observers, Function.apply(batch), { concurrency: 'unbounded', discard: true });
    yield* record;
    yield* Effect.acquireRelease(
        Effect.tryPromise(() => subscribe(root, (error, events) => Queue.offerUnsafe(batches, { events: events.map(fileEvent), stale: error !== null }), { ignore })),
        (subscription) => Effect.promise(() => subscription.unsubscribe()),
    );
    const watcher = yield* Stream.fromQueue(batches).pipe(
        Stream.runForEach((batch) =>
            batch.stale
                ? Latch.close(current).pipe(
                      Effect.andThen(Effect.tryPromise(() => getEventsSince(root, snapshot, scanned))),
                      Effect.tap(() => record),
                      Effect.flatMap((missed) => notify({ events: [...batch.events, ...missed.map(fileEvent)], stale: true })),
                      Effect.ensuring(Latch.open(current)),
                  )
                : notify(batch),
        ),
        Effect.orDie,
        Effect.forkIn(scope),
    );
    const workspace = { fs, root, observers, current, watching: Fiber.join(watcher).pipe(Effect.andThen(Effect.interrupt)) };
    const languages = new Map(
        (yield* Effect.forEach(
            Object.entries(yield* Schema.decodeUnknownEffect(Schema.Record(Schema.String, LanguageServer))(substitute(lsp))),
            Effect.fnUntraced(function* ([language, config]: readonly [string, typeof LanguageServer.Type]) {
                const server: RcRef.RcRef<Effect.Success<ReturnType<typeof start>>, Effect.Error<ReturnType<typeof start>>> = yield* RcRef.make({
                    acquire: start(language, config, workspace).pipe(
                        Effect.annotateLogs({ language }),
                        Effect.tap((started) => started.exited.pipe(Effect.ignore, Effect.andThen(RcRef.invalidate(server)), Effect.forkIn(scope))),
                    ),
                    idleTimeToLive: '30 minutes',
                }).pipe(Scope.provide(scope));
                return Object.entries(config.extensionToLanguage).map(([extension, languageId]) => [extension, { server, languageId }] as const);
            }),
        )).flat(),
    );
    const open = Effect.fn('open')(function* (file: string, use: (server: Effect.Success<ReturnType<typeof start>>, document: Opened) => Effect.Effect<readonly string[], LanguageError>) {
        const { server: shared, languageId } = yield* Effect.fromNullishOr(languages.get(extname(file))).pipe(Effect.mapError(() => LanguageError.make({ reason: { _tag: 'Unsupported', file } })));
        const server = yield* RcRef.get(shared);
        return {
            lines: yield* server.refresh.pipe(
                Effect.andThen(server.synchronize(pathToFileURL(resolve(root, file)).href, languageId)),
                Effect.catchTag('PlatformError', () => Effect.fail(LanguageError.make({ reason: { _tag: 'Unreadable', file } }))),
                Effect.flatMap((document) => use(server, document)),
                Effect.raceFirst(server.exited),
            ),
        };
    }, Effect.scoped);
    const request = <R>({ file, line, column }: typeof Position.Type, call: (connection: ProtocolConnection, params: TextDocumentPositionParams, token: CancellationToken) => Promise<R>, lines: (result: R) => readonly string[]) =>
        open(file, (server, { uri }) => server.ask((token) => call(server.connection, { textDocument: { uri }, position: { line: line - 1, character: column - 1 } }, token)).pipe(Effect.map(lines)));
    const locations = (targets: Definition | DefinitionLink[] | null): readonly string[] => [targets ?? []].flat().map((target) => ('targetUri' in target ? location(root, target.targetUri, target.targetSelectionRange) : location(root, target.uri, target.range)));
    const hovered = (hover: Hover | null): readonly string[] => [hover?.contents ?? []].flat().map((content) => (typeof content === 'string' ? content : content.value));
    yield* Effect.all(
        [
            register(query('diagnostics', 'Language server diagnostics of a file, one `path:line:column severity source(code): message` line each with indented related locations', Document), ({ file }) => open(file, (server, document) => server.diagnose(document))),
            register(query('definition', 'Definition locations of the symbol at a line and column, one `path:line:column` line each', Position), (position) => request(position, (connection, params, token) => connection.sendRequest(DefinitionRequest.type, params, token), locations)),
            register(query('references', 'Reference locations of the symbol at a line and column, declaration included, one `path:line:column` line each', Position), (position) =>
                request(position, (connection, params, token) => connection.sendRequest(ReferencesRequest.type, { ...params, context: { includeDeclaration: true } }, token), locations),
            ),
            register(query('hover', 'Type signature and documentation of the symbol at a line and column', Position), (position) => request(position, (connection, params, token) => connection.sendRequest(HoverRequest.type, params, token), hovered)),
        ],
        { discard: true },
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

main(plugin, Layer.effectDiscard(initialize()));
