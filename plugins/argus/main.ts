import { basename, extname, relative, resolve } from 'node:path';
import process from 'node:process';
import { PassThrough } from 'node:stream';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { NodeRuntime, NodeServices, NodeStream } from '@effect/platform-node';
import { Array, Duration, Effect, FileSystem, Layer, Logger, Option, Pool, PubSub, Record, Schema, Stream, String, Struct } from 'effect';
import { McpProtocol, McpServer, Tool, Toolkit } from 'effect/ai';
import { ChildProcess } from 'effect/process';
import {
    ConfigurationRequest,
    createProtocolConnection,
    type Definition,
    type DefinitionLink,
    DefinitionRequest,
    type Diagnostic,
    DiagnosticSeverity,
    DidChangeConfigurationNotification,
    DidCloseTextDocumentNotification,
    DidOpenTextDocumentNotification,
    DocumentDiagnosticReportKind,
    DocumentDiagnosticRequest,
    ExitNotification,
    HoverRequest,
    InitializedNotification,
    InitializeRequest,
    MarkupContent,
    type ProtocolConnection,
    PublishDiagnosticsNotification,
    type PublishDiagnosticsParams,
    type Range,
    ReferencesRequest,
    RegistrationRequest,
    ShutdownRequest,
    type TextDocumentPositionParams,
    WorkDoneProgressCreateRequest,
} from 'vscode-languageserver-protocol/node';
import plugin from './.claude-plugin/plugin.json' with { type: 'json' };
import lsp from './.lsp.json' with { type: 'json' };

// --- [TYPES] ---------------------------------------------------------------------------

interface Workspace {
    readonly fs: FileSystem.FileSystem;
    readonly languages: ReadonlyMap<string, { readonly pool: Pool.Pool<Effect.Success<ReturnType<typeof start>>, ServerExited | RequestFailed>; readonly languageId: string }>;
    readonly root: string;
}

// --- [MODELS] --------------------------------------------------------------------------

const LanguageServer = Schema.Struct({
    command: Schema.String,
    args: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    extensionToLanguage: Schema.Record(Schema.String, Schema.String),
    initializationOptions: Schema.optionalKey(Schema.Json),
    settings: Schema.Json.pipe(Schema.withDecodingDefaultKey(Effect.succeed(null))),
    workspaceFolder: Schema.String.pipe(Schema.withDecodingDefaultKey(Effect.succeed('.'))),
});
const Document = Schema.Struct({ file: Schema.String.annotate({ description: 'File path, absolute or relative to project root' }) });
const Position = Schema.Struct({ ...Document.fields, line: Schema.Int.annotate({ description: '1-based line' }), column: Schema.Int.annotate({ description: '1-based UTF-16 column' }) });
const Patch = Schema.Struct({ patch: Schema.String.annotate({ description: 'apply_patch text a PostToolUse hook received' }) });
const Lines = Schema.Struct({ lines: Schema.Array(Schema.String) });
const HookOutput = Schema.Struct({ hookSpecificOutput: Schema.optionalKey(Schema.Struct({ hookEventName: Schema.Literal('PostToolUse'), additionalContext: Schema.String })) });

// --- [ERRORS] --------------------------------------------------------------------------

class NoLanguageServer extends Schema.TaggedError<NoLanguageServer>()('NoLanguageServer', { file: Schema.String }) {}
class ReadFailed extends Schema.TaggedError<ReadFailed>()('ReadFailed', { cause: Schema.Defect() }) {}
class ServerExited extends Schema.TaggedError<ServerExited>()('ServerExited', { cause: Schema.Defect() }) {}
class RequestFailed extends Schema.TaggedError<RequestFailed>()('RequestFailed', { cause: Schema.Defect() }) {}
const Failure = Schema.Union([NoLanguageServer, ReadFailed, ServerExited, RequestFailed]);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SERVERS]

const send = <R>(message: () => Promise<R>): Effect.Effect<R, RequestFailed> => Effect.tryPromise({ try: message, catch: (cause) => new RequestFailed({ cause }) });
const start = Effect.fn('start')(function* (config: typeof LanguageServer.Type, root: string) {
    const folder = resolve(root, config.workspaceFolder);
    const uri = pathToFileURL(folder).href;
    const input = new PassThrough();
    const child = yield* ChildProcess.make(config.command, config.args, { cwd: folder, stdin: NodeStream.fromReadable({ evaluate: () => input }).pipe(Stream.orDie), stderr: 'inherit' }).pipe(Effect.mapError((cause) => new ServerExited({ cause })));
    const connection = createProtocolConnection(NodeStream.toReadableNever(child.stdout), input);
    const exited = Effect.exit(child.exitCode).pipe(Effect.flatMap((cause) => Effect.fail(new ServerExited({ cause }))));
    const published = yield* PubSub.unbounded<PublishDiagnosticsParams>();
    connection.onNotification(PublishDiagnosticsNotification.type, (params) => {
        PubSub.publishUnsafe(published, params);
    });
    connection.onRequest(ConfigurationRequest.type, ({ items }) =>
        items.map(({ section }) =>
            Option.getOrNull(
                Array.fromNullishOr(section)
                    .flatMap(String.split('.'))
                    .reduce((value, key) => value.pipe(Option.filter(Schema.is(Schema.JsonObject)), Option.flatMap(Record.get(key))), Option.some(config.settings)),
            ),
        ),
    );
    connection.onRequest(RegistrationRequest.type, () => undefined);
    connection.onRequest(WorkDoneProgressCreateRequest.type, () => undefined);
    connection.listen();
    const { capabilities } = yield* Effect.raceFirst(
        send(() =>
            connection.sendRequest(InitializeRequest.type, {
                processId: process.pid,
                rootUri: uri,
                workspaceFolders: [{ uri, name: basename(folder) }],
                initializationOptions: config.initializationOptions,
                capabilities: { workspace: { configuration: true }, textDocument: { diagnostic: {}, publishDiagnostics: {}, hover: { contentFormat: ['plaintext'] } } },
            }),
        ),
        exited,
    );
    yield* send(() => connection.sendNotification(InitializedNotification.type, {}));
    yield* send(() => connection.sendNotification(DidChangeConfigurationNotification.type, { settings: config.settings }));
    yield* Effect.addFinalizer(() => send(() => connection.sendRequest(ShutdownRequest.type)).pipe(Effect.andThen(send(() => connection.sendNotification(ExitNotification.type))), Effect.ignore));
    return { connection, exited, published, pull: capabilities.diagnosticProvider !== undefined };
});
const languages = Effect.fn('languages')(function* (config: typeof LanguageServer.Type, root: string) {
    const pool: Pool.Pool<Effect.Success<ReturnType<typeof start>>, ServerExited | RequestFailed> = yield* Pool.makeWithTTL({
        acquire: start(config, root).pipe(Effect.tap((server) => server.exited.pipe(Effect.ignore, Effect.andThen(Pool.invalidate(pool, server)), Effect.forkDetach))),
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
    (root: string, uri: string, threshold: DiagnosticSeverity) =>
    (diagnostic: Diagnostic): readonly string[] =>
        Object.entries(DiagnosticSeverity)
            .filter(([, severity]) => severity === (diagnostic.severity ?? DiagnosticSeverity.Error) && severity <= threshold)
            .map(
                ([name]) =>
                    `${location(root, uri, diagnostic.range)} ${name.toLowerCase()} ${[...Array.fromNullishOr(diagnostic.source), ...Array.fromNullishOr(diagnostic.code).map((code) => `(${code})`)].join('')}: ${(MarkupContent.is(diagnostic.message) ? diagnostic.message.value : diagnostic.message).replaceAll('\n', ' ')}`,
            );

// --- [QUERIES]

const open = Effect.fn('open')(function* (workspace: Workspace, file: string, use: (connection: ProtocolConnection, uri: string, diagnostics: Effect.Effect<readonly Diagnostic[], RequestFailed>) => Effect.Effect<readonly string[], RequestFailed>) {
    const path = resolve(workspace.root, file);
    const { pool, languageId } = yield* Effect.fromNullishOr(workspace.languages.get(extname(path))).pipe(Effect.mapError(() => new NoLanguageServer({ file })));
    const text = yield* workspace.fs.readFileString(path).pipe(Effect.mapError((cause) => new ReadFailed({ cause })));
    const { connection, exited, published, pull } = yield* Pool.get(pool);
    const uri = pathToFileURL(path).href;
    const document = Effect.gen(function* () {
        const subscription = yield* PubSub.subscribe(published);
        yield* Effect.acquireRelease(
            send(() => connection.sendNotification(DidOpenTextDocumentNotification.type, { textDocument: { uri, languageId, version: 1, text } })),
            () => send(() => connection.sendNotification(DidCloseTextDocumentNotification.type, { textDocument: { uri } })).pipe(Effect.ignore),
        );
        return yield* use(
            connection,
            uri,
            pull
                ? send(() => connection.sendRequest(DocumentDiagnosticRequest.type, { textDocument: { uri } })).pipe(Effect.map((report) => (report.kind === DocumentDiagnosticReportKind.Full ? report.items : [])))
                : Stream.fromSubscription(subscription).pipe(
                      Stream.filter((params) => params.uri === uri),
                      Stream.debounce(Duration.seconds(1)),
                      Stream.runHead,
                      Effect.map((head) => Option.getOrThrow(head).diagnostics),
                  ),
        );
    });
    return { lines: yield* Effect.raceFirst(Effect.scoped(document), exited) };
}, Effect.scoped);
const diagnose = Effect.fn('diagnose')((workspace: Workspace, file: string, threshold: DiagnosticSeverity) => open(workspace, file, (_connection, uri, diagnostics) => Effect.map(diagnostics, Array.flatMap(render(workspace.root, uri, threshold)))));
const request = Effect.fn('request')(<R>(workspace: Workspace, { file, line, column }: typeof Position.Type, call: (connection: ProtocolConnection, params: TextDocumentPositionParams) => Promise<R>, lines: (result: R) => readonly string[]) =>
    open(workspace, file, (connection, uri) => send(() => call(connection, { textDocument: { uri }, position: { line: line - 1, character: column - 1 } })).pipe(Effect.map(lines))),
);
const diagnosePatch = Effect.fn('diagnosePatch')(function* (workspace: Workspace, patch: string) {
    const files = Array.dedupe(patch.match(/(?<=^\*\*\* (?:Add File|Update File|Move to): ).+?(?=\s*$)/gmu) ?? []).filter((file) => workspace.languages.has(extname(file)));
    const existing = yield* Effect.filter(files, (file) => workspace.fs.exists(resolve(workspace.root, file)).pipe(Effect.mapError((cause) => new ReadFailed({ cause }))));
    const lines = (yield* Effect.forEach(existing, (file) => diagnose(workspace, file, DiagnosticSeverity.Warning), { concurrency: 'unbounded' })).flatMap(Struct.get('lines'));
    return lines.length === 0 ? {} : { hookSpecificOutput: { hookEventName: 'PostToolUse' as const, additionalContext: lines.join('\n') } };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const query = <const Name extends string, P extends Schema.Constraint, S extends Schema.Constraint>(name: Name, description: string, parameters: P, success: S): Tool.Tool<Name, { readonly parameters: P; readonly success: S; readonly failure: typeof Failure; readonly failureMode: 'error' }> =>
    Tool.make(name, { description, parameters, success, failure: Failure }).annotate(Tool.Readonly, true).annotate(Tool.Destructive, false).annotate(Tool.OpenWorld, false);
const tools = Toolkit.make(
    query('diagnostics', 'Errors, warnings, information, and hints the language server reports for a file, one `path:line:column severity source(code): message` line each', Document, Lines),
    query('definition', 'Definition locations of the symbol at a line and column, one `path:line:column` line each', Position, Lines),
    query('references', 'Reference locations of the symbol at a line and column, declaration included, one `path:line:column` line each', Position, Lines),
    query('hover', 'Type signature and documentation of the symbol at a line and column', Position, Lines),
    query('patched', 'PostToolUse hook output with the errors and warnings of each file an apply_patch adds, updates, or moves', Patch, HookOutput),
);
const handlers = tools.toLayer(
    Effect.gen(function* () {
        const root = process.cwd();
        const variables: NodeJS.Dict<string> = {
            ...process.env,
            ...Record.singleton('CLAUDE_PROJECT_DIR', root),
            ...Record.mapEntries(plugin.userConfig, (option, key) => [`user_config.${key}`, option.default]),
        };
        const configs = yield* Schema.decodeEffect(Schema.fromJsonString(Schema.Record(Schema.String, LanguageServer)))(JSON.stringify(lsp, (_key, value: unknown) => (typeof value === 'string' ? value.replace(/\$\{(?<name>[\w.]+)\}/gu, (match, name: string) => variables[name] ?? match) : value)));
        const workspace: Workspace = {
            fs: yield* FileSystem.FileSystem,
            languages: new Map((yield* Effect.forEach(Object.values(configs), (config) => languages(config, root))).flat()),
            root,
        };
        return {
            diagnostics: ({ file }) => diagnose(workspace, file, DiagnosticSeverity.Hint),
            definition: (position) => request(workspace, position, (connection, params) => connection.sendRequest(DefinitionRequest.type, params), locations(root)),
            references: (position) => request(workspace, position, (connection, params) => connection.sendRequest(ReferencesRequest.type, { ...params, context: { includeDeclaration: true } }), locations(root)),
            hover: (position) =>
                request(
                    workspace,
                    position,
                    (connection, params) => connection.sendRequest(HoverRequest.type, params),
                    (hover) => Array.fromNullishOr(hover?.contents).flat().filter(MarkupContent.is).map(Struct.get('value')),
                ),
            patched: ({ patch }) => diagnosePatch(workspace, patch),
        };
    }),
);

McpServer.toolkit(tools).pipe(
    Layer.provide([
        handlers,
        McpServer.layerStdio({
            instructions: "Read a symbol's definition, references, and type signature from its language server before changing it, and a file's diagnostics after",
            name: plugin.name,
            protocols: [McpProtocol.v2026_07_28, McpProtocol.v2025_11_25],
            version: plugin.version,
        }),
    ]),
    Layer.provide(NodeServices.layer),
    Layer.launch,
    Effect.provideService(Logger.LogToStderr, true),
    NodeRuntime.runMain,
);
