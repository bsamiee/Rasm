import { extname, relative, resolve } from 'node:path';
import process from 'node:process';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { registerTool } from '@rasm/mcp';
import { Unmatched } from '@rasm/search';
import { Array, Cause, type Duration, Effect, Equal, flow, Iterable, Option, Order, pipe, RcMap, Record, Result, Schema, SchemaTransformation, Scope, Stream, String, Struct, Tuple } from 'effect';
import { Tool } from 'effect/ai';
import { type Definition, type DefinitionLink, DefinitionRequest, type Diagnostic, DiagnosticSeverity, type Hover, HoverRequest, type Location, LocationLink, type Range, ReferencesRequest, RequestType, type TextDocumentIdentifier, type TextDocumentItem } from 'vscode-languageserver-protocol/node';
import { LanguageServer } from '#/language-server';
import { watchWorkspace } from '#/watch-workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ClassFileContents = new RequestType<TextDocumentIdentifier, string, void>('java/classFileContents');
const DEFAULT_JAVA_LINES = 20;

// --- [MODELS] --------------------------------------------------------------------------

const File = Schema.Struct({ file: Schema.String.annotate({ description: 'Absolute or project-relative path' }) });
const Coordinate = Schema.Int.check(Schema.isGreaterThanOrEqualTo(1)).pipe(Schema.decodeTo(Schema.Int.check(Schema.isGreaterThanOrEqualTo(0)), SchemaTransformation.transform({ decode: (coordinate) => coordinate - 1, encode: (coordinate) => coordinate + 1 })));
const Position = File.mapFields(Struct.assign({ line: Coordinate.pipe(Schema.annotateEncoded({ description: '1-based line' })), character: Coordinate.pipe(Schema.annotateEncoded({ description: '1-based UTF-16 column' })) })).pipe(Schema.encodeKeys({ character: 'column' }));
const Lines = Schema.Struct({ lines: Schema.Array(Schema.String) });
const JavaSource = Schema.Struct({
    uri: Schema.String.check(Schema.isPattern(/^jdt:\/\/contents\//u)).annotate({ description: 'JDT definition URI including its query' }),
    line: Coordinate.pipe(Schema.annotateEncoded({ description: '1-based first line' })),
    count: Schema.Int.check(Schema.isBetween({ minimum: 1, maximum: 100 })).pipe(Schema.withDecodingDefaultKey(Effect.succeed(DEFAULT_JAVA_LINES))),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const location = (root: string, uri: string, { start: { line, character } }: Range) => `${URL.parse(uri)?.protocol === 'file:' ? relative(root, fileURLToPath(uri)) : uri}:${line + 1}:${character + 1}`;
const point = (item: TextDocumentItem, given: typeof Position.Type) => ({ textDocument: { uri: item.uri }, position: Struct.omit(given, ['file']) });
const locations = (root: string) => (targets: Definition | DefinitionLink[] | Location[] | null) => Array.ensure<Location | DefinitionLink>(targets ?? []).map((target) => (LocationLink.is(target) ? location(root, target.targetUri, target.targetSelectionRange) : location(root, target.uri, target.range)));
const renderText = (content: Hover['contents'] | Diagnostic['message']) => Array.ensure(content).map((part) => (typeof part === 'string' ? part : part.value));
const flattened = (message: Diagnostic['message']) => renderText(message).join('').replaceAll('\n', ' ');
const severities = new Map(Record.collect(DiagnosticSeverity, (name, value) => [value, name.toLowerCase()] as const));
const renderDiagnostics = (root: string, item: TextDocumentItem) => (reported: readonly Diagnostic[]) => {
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
            `${location(root, item.uri, range)} ${severities.get(severity ?? DiagnosticSeverity.Error)} ${source ?? ''}${code === undefined ? '' : `(${code})`}: ${flattened(message)}${codeDescription === undefined ? '' : ` ${codeDescription.href}`}`,
            ...(relatedInformation ?? []).map((entry) => `  ${location(root, entry.location.uri, entry.location.range)} ${flattened(entry.message)}`),
        ].join('\n'),
    );
};

const registerLanguageServerTools = Effect.fn('registerLanguageServerTools')(function* ({ servers, idle }: { readonly servers: Record.ReadonlyRecord<string, typeof LanguageServer.Config.Encoded>; readonly idle: Duration.Input }) {
    const root = process.cwd();
    const workspace = yield* Effect.flatMap(watchWorkspace(root), Stream.share({ capacity: 'unbounded' }));
    const entries = Record.toEntries(servers);
    const scope = yield* Scope.fork(yield* Effect.scope, 'parallel');
    const running = yield* RcMap.make({
        lookup: (entry: (typeof entries)[number]) =>
            LanguageServer.start(root, entry[1]).pipe(
                Effect.annotateLogs({ language: entry[0] }),
                Effect.tap((server): Effect.Effect<unknown, never, Scope.Scope> => server.run(workspace).pipe(Effect.ignore({ log: 'Warn', message: 'language server stopped' }), Effect.andThen(Effect.forkIn(RcMap.invalidate(running, entry), scope)), Effect.forkScoped({ startImmediately: true }))),
            ),
        idleTimeToLive: idle,
    }).pipe(Scope.provide(scope));
    const languages = entries.flatMap((entry) => Array.cartesian([entry], Record.toEntries(entry[1].extensionToLanguage)));
    const tool = <P extends Schema.Schema<typeof File.Type>>(name: string, description: string, parameters: P, handle: (server: LanguageServer, item: TextDocumentItem, given: P['Type']) => Effect.Effect<readonly string[], Error>) =>
        registerTool(
            Tool.make(name, { description, parameters, success: Lines }).annotate(Tool.Readonly, true).annotate(Tool.OpenWorld, false),
            Effect.fnUntraced(function* (given: P['Type']) {
                const [entry, [, languageId]] = yield* Unmatched.single(languages, 'language server for extension', Option.some(extname(given.file)), flow(Tuple.get(1), Tuple.get(0), Array.make));
                const server = yield* RcMap.get(running, entry);
                return { lines: yield* handle(server, yield* server.open(pathToFileURL(resolve(root, given.file)).href, languageId), given) };
            }, Effect.scoped),
        );

    yield* Effect.all(
        [
            tool('diagnostics', 'Language server diagnostics as `path:line:column severity source(code): message` lines, related locations indented', File, (server, item) => Effect.map(server.diagnose(item), renderDiagnostics(root, item))),
            tool('definition', 'Symbol definition locations as `path:line:column` lines', Position, (server, item, given) => Effect.map(server.ask(DefinitionRequest.type, point(item, given)), locations(root))),
            tool('references', 'Symbol reference and declaration locations as `path:line:column` lines', Position, (server, item, given) => Effect.map(server.ask(ReferencesRequest.type, { ...point(item, given), context: { includeDeclaration: true } }), locations(root))),
            tool('hover', 'Symbol type signature and documentation', Position, (server, item, given) => Effect.map(server.ask(HoverRequest.type, point(item, given)), flow(Option.fromNullOr, Option.map(Struct.get('contents')), Option.getOrElse(Array.empty), renderText))),
        ],
        { discard: true },
    );
    const java = entries.find(([name]) => name === 'java');
    if (java !== undefined) {
        yield* registerTool(
            Tool.make('java_source', { description: 'Read source lines attached to a Java class file definition', parameters: JavaSource, success: Lines }).annotate(Tool.Readonly, true).annotate(Tool.OpenWorld, false),
            Effect.fnUntraced(function* ({ uri, line, count }: typeof JavaSource.Type) {
                const server = yield* RcMap.get(running, java);
                const source = yield* server.ask(ClassFileContents, { uri });
                if (source === null || source === '') {
                    return yield* new Cause.NoSuchElementError('JDT LS returned no class file source');
                }
                return { lines: pipe(source, String.linesIterator, Iterable.drop(line), Iterable.take(count), Array.fromIterable) };
            }, Effect.scoped),
        );
    }
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { registerLanguageServerTools };
