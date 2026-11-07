import { basename, resolve } from 'node:path';
import process from 'node:process';
import { PassThrough } from 'node:stream';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { fromReadable, toReadableNever } from '@effect/platform-node/NodeStream';
import type { Event } from '@parcel/watcher';
import { Array, type Cause, Config, Duration, Effect, Exit, Fiber, FileSystem, Function, flow, HashMap, HashSet, Iterable, Match, Number, Option, Predicate, Queue, Record, Result, Schedule, Schema, Stream, Struct, SubscriptionRef } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/process';
import { escape, minimatch } from 'minimatch';
import {
    CancellationTokenSource,
    ConfigurationRequest,
    ConnectionError,
    createMessageConnection,
    DiagnosticRefreshRequest,
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
    InitializedNotification,
    InitializeRequest,
    LSPErrorCodes,
    MarkupKind,
    type NotificationType,
    type ProgressToken,
    type ProtocolNotificationType,
    PublishDiagnosticsNotification,
    type PublishDiagnosticsParams,
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
import { Exited } from '#/exited';

// --- [TYPES] ---------------------------------------------------------------------------

interface Resources {
    readonly config: typeof LanguageServer.Config.Type;
    readonly folder: string;
    readonly fs: FileSystem.FileSystem;
    readonly child: ChildProcessSpawner.ChildProcessHandle;
    readonly connection: ReturnType<typeof createMessageConnection>;
    readonly state: SubscriptionRef.SubscriptionRef<typeof INITIAL>;
    readonly stopped: Effect.Effect<never, Exited | Effect.Error<ChildProcessSpawner.ChildProcessHandle['exitCode']>>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const INITIAL = {
    registrations: HashMap.empty<string, Registration>(),
    progress: HashSet.empty<ProgressToken>(),
    begun: 0,
    invalidated: 0,
    documents: HashMap.empty<string, { readonly item: TextDocumentItem; readonly begun: number }>(),
    published: HashMap.empty<string, PublishDiagnosticsParams>(),
};
const REFERENCE = /\$\{(?<name>\w+)\}/gu;
const SHUTDOWN_GRACE_SECONDS = 3;

// --- [MODELS] --------------------------------------------------------------------------

const Timeout = (fallback: Duration.Duration) => Schema.Int.check(Schema.isGreaterThan(0)).pipe(Schema.decodeTo(Schema.DurationFromMillis), Schema.withDecodingDefaultTypeKey(Effect.succeed(fallback)));
const JsonText = Schema.fromJsonString(Schema.Json);

// --- [SERVICES] ------------------------------------------------------------------------

class LanguageServer {
    // --- [CONNECTION]

    readonly #resources: Resources;

    constructor(resources: Resources) {
        this.#resources = resources;
    }

    static readonly Config = Schema.Struct({
        command: Schema.String,
        args: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        env: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)),
        extensionToLanguage: Schema.Record(Schema.String, Schema.String),
        initializationOptions: Schema.optionalKey(Schema.Json),
        settings: Schema.OptionFromOptionalKey(Schema.Json),
        workspaceFolder: Schema.String.pipe(Schema.withDecodingDefaultKey(Effect.succeed('.'))),
        startupTimeout: Timeout(Duration.infinity),
        shutdownTimeout: Timeout(Duration.seconds(SHUTDOWN_GRACE_SECONDS)),
        requestTimeout: Timeout(Duration.minutes(1)),
    });

    static readonly start = Effect.fn('LanguageServer.start')(function* (root: string, declared: typeof LanguageServer.Config.Encoded) {
        const text = yield* Schema.encodeEffect(JsonText)(declared);
        const names = [...text.matchAll(REFERENCE)].flatMap(({ groups }) => groups?.name ?? []);
        const variables = yield* Config.all(Record.fromIterableWith(names, (name) => [name, name === 'CLAUDE_PROJECT_DIR' ? Config.succeed(root) : Config.String(name)] as const));
        const config = yield* Schema.decodeEffect(Schema.fromJsonString(LanguageServer.Config, { reviver: (_key, value: unknown) => (typeof value === 'string' ? Record.reduce(variables, value, (expanded, variable, name) => expanded.replaceAll(`\${${name}}`, variable)) : value) }))(text);
        const fs = yield* FileSystem.FileSystem;
        const folder = resolve(root, config.workspaceFolder);
        const input = new PassThrough();
        const child = yield* ChildProcess.make(config.command, config.args, {
            cwd: folder,
            env: config.env,
            extendEnv: true,
            stdin: fromReadable({ evaluate: () => input }).pipe(Stream.orDie),
            forceKillAfter: config.shutdownTimeout,
        });
        yield* child.stderr.pipe(Stream.decodeText(), Stream.splitLines, Stream.runForEach(Effect.logInfo), Effect.ignore({ log: 'Warn', message: 'stderr read failed' }), Effect.forkScoped);
        const connection = yield* Effect.acquireRelease(
            Effect.sync(() => createMessageConnection(toReadableNever(child.stdout), input)),
            (opened) => Effect.sync(opened.dispose.bind(opened)),
        );
        const stopped = Effect.flatMap(child.exitCode, (code) => Exited.make({ command: config.command, args: config.args, code }));
        const server = new LanguageServer({ config, folder, fs, child, connection, state: yield* SubscriptionRef.make(INITIAL), stopped });
        yield* server.#listen().pipe(
            Stream.runForEach((update) => SubscriptionRef.update(server.#resources.state, update)),
            Effect.forkScoped({ startImmediately: true }),
        );
        yield* server.#initialize();
        return server;
    });

    readonly run = Effect.fn('LanguageServer.run')({ self: this }, function* (this: LanguageServer, workspace: Stream.Stream<readonly Event[], Error>) {
        const watching = yield* workspace.pipe(Stream.runForEach(this.#forward), Effect.forkScoped({ startImmediately: true }));
        return yield* Effect.raceFirst(this.#resources.stopped, Fiber.join(watching));
    });

    #setting(section: string | undefined): Option.Option<Schema.Json> {
        return (section?.split('.') ?? []).reduce((value, key) => Option.flatMap(value, flow(Option.liftPredicate(Schema.is(Schema.JsonObject)), Option.flatMap(Record.get(key)))), this.#resources.config.settings);
    }

    #listen() {
        const { connection } = this.#resources;
        const invalidate: (current: typeof INITIAL) => typeof INITIAL = Struct.evolve({ invalidated: Number.sum(1) });
        const setting = this.#setting.bind(this);
        return Stream.callback<(current: typeof INITIAL) => typeof INITIAL>((updates) => {
            const offer = (update: (current: typeof INITIAL) => typeof INITIAL) => {
                Queue.offerUnsafe(updates, update);
            };
            connection.onRequest(ConfigurationRequest.type, ({ items }) => items.map(flow(Struct.get('section'), setting, Option.getOrNull)));
            connection.onRequest(RegistrationRequest.type, ({ registrations }) => offer(flow(invalidate, Struct.evolve({ registrations: HashMap.setMany(Array.zip(registrations.map(Struct.get('id')), registrations)) }))));
            connection.onRequest(UnregistrationRequest.type, ({ unregisterations }) => offer(flow(invalidate, Struct.evolve({ registrations: HashMap.removeMany(unregisterations.map(Struct.get('id')))<Registration> }))));
            connection.onRequest(DiagnosticRefreshRequest.type, () => offer(invalidate));
            connection.onRequest(WorkDoneProgressCreateRequest.type, Function.constVoid);
            connection.onUnhandledProgress(({ token, value: { kind } }) => {
                if (kind !== 'report') {
                    offer(
                        flow(
                            invalidate,
                            Struct.evolve({
                                begun: kind === 'begin' ? Number.sum(1) : Function.identity,
                                progress: kind === 'begin' ? HashSet.add(token) : HashSet.remove(token),
                            }),
                        ),
                    );
                }
            });
            connection.onNotification(PublishDiagnosticsNotification.type, (params) => offer(Struct.evolve({ published: HashMap.set(params.uri, params) })));
            connection.listen();
            return Effect.void;
        });
    }

    readonly #initialize = Effect.fn('LanguageServer.initialize')({ self: this }, function* (this: LanguageServer) {
        const { config, folder, state, connection, child } = this.#resources;
        const workspaceFolder = { uri: pathToFileURL(folder).href, name: basename(folder) };
        const { capabilities } = yield* this.ask(
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
        yield* SubscriptionRef.update(state, Struct.evolve({ registrations: HashMap.setMany<string, Registration>(Record.toEntries(declared)) }));
        yield* this.#notify(InitializedNotification.type, {});
        yield* Option.match(config.settings, { onNone: () => Effect.void, onSome: (settings) => this.#notify(DidChangeConfigurationNotification.type, { settings }) });
        const exited = Effect.asVoid(child.exitCode);
        const shutdown = Effect.tryPromise(() => connection.sendRequest(ShutdownRequest.type)).pipe(
            Effect.andThen(Effect.tryPromise(() => connection.sendNotification(ExitNotification.type))),
            Effect.as(exited),
            Effect.raceFirst(Effect.as(exited, Effect.void)),
            Effect.flatten,
            Effect.timeout(config.shutdownTimeout),
            Effect.ignore({ log: 'Warn', message: 'shutdown failed' }),
            Effect.annotateLogs({ pid: child.pid }),
        );
        yield* Effect.addFinalizer(Function.constant(shutdown));
    });

    // --- [REQUESTS]

    ask<P, R>(type: RequestType<P, R, unknown>, params: RequestParam<P>, timeout = this.#resources.config.requestTimeout) {
        const { connection, stopped } = this.#resources;
        return Effect.acquireUseRelease(
            Effect.sync(() => new CancellationTokenSource()),
            (source) => Effect.tryPromise(() => connection.sendRequest(type, params, source.token)).pipe(Effect.catchTag('UnknownError', this.#classify), Effect.raceFirst(stopped)),
            (source, exit) =>
                Effect.sync(() => {
                    if (Exit.hasInterrupts(exit)) {
                        source.cancel();
                    }
                    source.dispose();
                }),
        ).pipe(Effect.retry({ while: (error) => error instanceof ResponseError && error.code === LSPErrorCodes.ContentModified, schedule: Schedule.spaced('100 millis') }), Effect.timeout(timeout));
    }

    readonly #classify = ({ cause }: Cause.UnknownError) =>
        Match.value(cause).pipe(
            Match.when(Match.instanceOf(ResponseError), Effect.fail<ResponseError<unknown>>),
            Match.when(Match.instanceOf(ConnectionError), () => this.#resources.stopped),
            Match.orElse(Effect.die),
        );

    #notify<P>(type: NotificationType<P>, params: RequestParam<P>) {
        return Effect.tryPromise(() => this.#resources.connection.sendNotification(type, params)).pipe(Effect.catchTag('UnknownError', this.#classify));
    }

    #announce<P, RO>(type: ProtocolNotificationType<P, RO>, current: typeof INITIAL, params: RequestParam<P>) {
        return Iterable.isEmpty(registered(type, current.registrations)) ? Effect.void : this.#notify(type, params);
    }

    // --- [DOCUMENTS]

    #updateDocument(document: Omit<TextDocumentItem, 'version'>) {
        return SubscriptionRef.modifyEffect(this.#resources.state, (current) => {
            const previous = HashMap.get(current.documents, document.uri);
            if (Option.isSome(previous) && previous.value.item.text === document.text) {
                return Effect.succeed([previous.value.item, current] as const);
            }
            const item = { ...document, version: Option.match(previous, { onNone: () => 1, onSome: ({ item: { version } }) => version + 1 }) };
            const announced = Option.match(previous, {
                onNone: () => this.#announce(DidOpenTextDocumentNotification.type, current, { textDocument: item }),
                onSome: () => this.#announce(DidChangeTextDocumentNotification.type, current, { textDocument: { uri: item.uri, version: item.version }, contentChanges: [{ text: item.text }] }),
            });
            return Effect.as(announced, [item, { ...current, documents: HashMap.set(current.documents, item.uri, { item, begun: current.begun }), published: HashMap.remove(current.published, item.uri) }] as const);
        });
    }

    open(uri: string, languageId: string) {
        return Stream.fromIterableEffect(Effect.map(SubscriptionRef.get(this.#resources.state), (current) => HashMap.values(current.documents))).pipe(
            Stream.mapEffect(({ item }) => this.#synchronize(item.uri, item.languageId).pipe(Effect.catchTag('PlatformError', () => this.#close(item.uri))), { concurrency: 'unbounded' }),
            Stream.runDrain,
            Effect.andThen(this.#synchronize(uri, languageId)),
            Effect.raceFirst(this.#resources.stopped),
        );
    }

    #synchronize(uri: string, languageId: string) {
        return this.#resources.fs.readFileString(fileURLToPath(uri)).pipe(Effect.flatMap((text) => this.#updateDocument({ uri, languageId, text })));
    }

    #close(uri: string) {
        return SubscriptionRef.modifyEffect(this.#resources.state, (current) =>
            Effect.as(this.#announce(DidCloseTextDocumentNotification.type, current, { textDocument: { uri } }), [undefined, { ...current, documents: HashMap.remove(current.documents, uri), published: HashMap.remove(current.published, uri) }] as const),
        );
    }

    // --- [DIAGNOSTICS]

    readonly diagnose = Effect.fn('LanguageServer.diagnose')({ self: this }, function* (this: LanguageServer, item: TextDocumentItem) {
        const [saved] = registered(DidSaveTextDocumentNotification.type, (yield* SubscriptionRef.get(this.#resources.state)).registrations);
        yield* saved === undefined ? Effect.void : this.#notify(DidSaveTextDocumentNotification.type, { textDocument: { uri: item.uri }, ...(saved.includeText === true ? { text: item.text } : {}) });
        return yield* SubscriptionRef.changes(this.#resources.state).pipe(
            Stream.changesWith((left, right) => left.invalidated === right.invalidated && LanguageServer.#settled(item.uri, left) === LanguageServer.#settled(item.uri, right)),
            Stream.switchMap((current) => (LanguageServer.#settled(item.uri, current) ? this.#pull(item.uri, current) : Stream.empty)),
            Stream.take(1),
            Stream.flattenIterable,
            Stream.runCollect,
            Effect.raceFirst(this.#resources.stopped),
        );
    });

    static #settled(uri: string, current: typeof INITIAL) {
        return (
            HashSet.isEmpty(current.progress) &&
            (!Iterable.isEmpty(registered(DocumentDiagnosticRequest.type, current.registrations)) ||
                Option.exists(HashMap.get(current.documents, uri), ({ item, begun }) => (current.begun === 0 || current.begun > begun) && Option.exists(HashMap.get(current.published, uri), ({ version }) => version === undefined || version === item.version)))
        );
    }

    #pull(uri: string, current: typeof INITIAL) {
        return Stream.fromEffect(
            Effect.forEach(
                registered(DocumentDiagnosticRequest.type, current.registrations),
                ({ identifier }) => this.ask(DocumentDiagnosticRequest.type, { textDocument: { uri }, ...(identifier === undefined ? {} : { identifier }) }).pipe(Effect.flatMap((report) => (report.kind === DocumentDiagnosticReportKind.Full ? Effect.succeed(report.items) : Effect.die(report)))),
                { concurrency: 'unbounded' },
            ),
        ).pipe(
            Stream.mapEffect((pulled) => Effect.map(SubscriptionRef.get(this.#resources.state), (latest) => [...pulled.flat(), ...Option.match(HashMap.get(latest.published, uri), { onNone: Array.empty, onSome: Struct.get('diagnostics') })])),
            Stream.catchIf(
                (error) => error instanceof ResponseError && error.code === LSPErrorCodes.ServerCancelled,
                () => Stream.empty,
            ),
        );
    }

    // --- [FILES]

    readonly #forward = Effect.fn('LanguageServer.forward')({ self: this }, function* (this: LanguageServer, events: readonly Event[]) {
        const kinds = { create: [FileChangeType.Created, WatchKind.Create], update: [FileChangeType.Changed, WatchKind.Change], delete: [FileChangeType.Deleted, WatchKind.Delete] } as const;
        const changes = Array.dedupe(
            Array.filterMap(Iterable.cartesian(events, Iterable.flatMap(registered(DidChangeWatchedFilesNotification.type, (yield* SubscriptionRef.get(this.#resources.state)).registrations), Struct.get('watchers'))), ([{ path, type }, { globPattern, kind }]) => {
                const [change, mask] = kinds[type];
                const [base, pattern] = typeof globPattern === 'string' ? [this.#resources.folder, globPattern] : [fileURLToPath(typeof globPattern.baseUri === 'string' ? globPattern.baseUri : globPattern.baseUri.uri), globPattern.pattern];
                return (kind === undefined || (kind & mask) !== 0) && minimatch(path, resolve(escape(base, { magicalBraces: true }), pattern), { dot: true }) ? Result.succeed({ uri: pathToFileURL(path).href, type: change }) : Result.failVoid;
            }),
        );
        yield* changes.length === 0 ? Effect.void : this.#notify(DidChangeWatchedFilesNotification.type, { changes });
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const registered = <RO>({ method }: RegistrationType<RO>, registrations: (typeof INITIAL)['registrations']) => Iterable.filterMap(registrations, ([, entry]) => (entry.method === method ? Result.succeed<RO>(entry.registerOptions) : Result.failVoid));

// --- [EXPORTS] -------------------------------------------------------------------------

export { LanguageServer };
