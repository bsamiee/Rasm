import { dirname, extname, relative, resolve } from 'node:path';
import process from 'node:process';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { type Event, getEventsSince, subscribe, writeSnapshot } from '@parcel/watcher';
import { registerTool } from '@rasm/mcp';
import { Unmatched } from '@rasm/search';
import { Array, Config, Effect, Equal, FileSystem, Function, flow, Option, Order, PlatformError, Queue, RcMap, Record, Result, Schema, SchemaTransformation, Scope, Stream, String, Struct, Trie, Tuple } from 'effect';
import { Tool } from 'effect/ai';
import { ChildProcess } from 'effect/process';
import { escape } from 'minimatch';
import { type Definition, type DefinitionLink, DefinitionRequest, type Diagnostic, DiagnosticSeverity, type Hover, HoverRequest, type Location, LocationLink, type Range, ReferencesRequest, type TextDocumentItem } from 'vscode-languageserver-protocol/node';
import lsp from '#/.lsp.json' with { type: 'json' };
import { Exited, LanguageServer } from '#/language-server';

// --- [MODELS] --------------------------------------------------------------------------

const File = Schema.Struct({ file: Schema.String.annotate({ description: 'Absolute or project-relative path' }) });
const Coordinate = Schema.Int.check(Schema.isGreaterThanOrEqualTo(1)).pipe(Schema.decodeTo(Schema.Int.check(Schema.isGreaterThanOrEqualTo(0)), SchemaTransformation.transform({ decode: (coordinate) => coordinate - 1, encode: (coordinate) => coordinate + 1 })));
const Position = File.mapFields(Struct.assign({ line: Coordinate.pipe(Schema.annotateEncoded({ description: '1-based line' })), character: Coordinate.pipe(Schema.annotateEncoded({ description: '1-based UTF-16 column' })) })).pipe(Schema.encodeKeys({ character: 'column' }));
const Lines = Schema.Struct({ lines: Schema.Array(Schema.String) });
const JsonText = Schema.fromJsonString(Schema.Json);

// --- [OPERATIONS] ----------------------------------------------------------------------

const watch = (root: string, ignore: string[], snapshot: string) => {
    const scanned = { ignore, backend: 'brute-force' as const };
    const watched = <A>(run: () => Promise<A>) => Effect.tryPromise({ try: run, catch: (cause) => PlatformError.systemError({ _tag: 'Unknown', module: 'FileSystem', method: 'watch', pathOrDescriptor: root, cause }) });
    const record = watched(() => writeSnapshot(root, snapshot, scanned));
    return Stream.unwrap(
        Effect.as(
            record,
            Stream.callback<readonly [Error | null, readonly Event[]], PlatformError.PlatformError>((queue) =>
                Effect.acquireRelease(watched(subscribe.bind(undefined, root, (error, events) => Queue.offerUnsafe(queue, [error, events]), { ignore })), (subscription) => Effect.promise(subscription.unsubscribe.bind(subscription))),
            ),
        ),
    ).pipe(
        Stream.mapEffect(([error, events]) =>
            error === null
                ? Effect.succeed(events)
                : watched(() => getEventsSince(root, snapshot, scanned)).pipe(
                      Effect.tap(record),
                      Effect.map((missed) => [...events, ...missed]),
                  ),
        ),
    );
};

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

const registerLanguageServerTools = Effect.fn('registerLanguageServerTools')(function* () {
    const root = process.cwd();
    const listing = ['ls-files', '--others', '--ignored', '--exclude-standard', '--directory', '-z'];
    const ignored = yield* Effect.scoped(
        Effect.gen(function* () {
            const git = yield* ChildProcess.make('git', listing, { cwd: root });
            const [text, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(git.stdout)), git.exitCode], { concurrency: 'unbounded' });
            return code === 0 ? [...text.split('\0').filter(String.isNonEmpty), '.git/'] : yield* Exited.make({ command: 'git', args: listing, code });
        }),
    );
    const parents = Trie.fromIterable(ignored.filter(String.endsWith('/')).map((path) => [path, undefined] as const));
    const outermost = ignored.filter(flow(dirname, String.concat('/'), (parent) => Trie.longestPrefixOf(parents, parent), Option.isNone));
    const exclusionPaths = 8;
    const [excluded, matched] = Array.splitAt(
        Array.sortWith(outermost, (path) => [!path.endsWith('/'), path.split('/').length] as const, Order.Tuple([Order.Boolean, Order.Number])),
        exclusionPaths,
    );
    const workspace = yield* Stream.share(watch(root, [...excluded.map((path) => resolve(root, path)), ...matched.map((path) => `${escape(path.endsWith('/') ? path.slice(0, -1) : path, { magicalBraces: true })}{,/**}`)], resolve(yield* FileSystem.FileSystem.use((fs) => fs.makeTempDirectoryScoped()), 'snapshot')), {
        capacity: 'unbounded',
    });
    const reference = /\$\{(?<name>\w+)\}/gu;
    const json = yield* Schema.encodeEffect(JsonText)(lsp);
    const names = [...json.matchAll(reference)].flatMap(({ groups }) => groups?.name ?? []);
    const variables = Record.set(Record.getSomes(yield* Config.all(Record.fromIterableWith(names, (name) => [name, Config.option(Config.String(name))]))), 'CLAUDE_PROJECT_DIR', root);
    const servers = Record.toEntries(
        yield* Schema.decodeEffect(Schema.fromJsonString(Schema.Record(Schema.String, LanguageServer.Config), { reviver: (_key, value: unknown) => (typeof value === 'string' ? value.replace(reference, (match, name: string) => Option.getOrElse(Record.get(variables, name), Function.constant(match))) : value) }))(json),
    );
    const scope = yield* Scope.fork(yield* Effect.scope, 'parallel');
    const running = yield* RcMap.make({
        lookup: (entry: (typeof servers)[number]) =>
            LanguageServer.start(root, entry[1]).pipe(
                Effect.annotateLogs({ language: entry[0] }),
                Effect.tap((server): Effect.Effect<unknown, never, Scope.Scope> => server.run(workspace).pipe(Effect.ignore({ log: 'Warn', message: 'language server stopped' }), Effect.andThen(Effect.forkIn(RcMap.invalidate(running, entry), scope)), Effect.forkScoped({ startImmediately: true }))),
            ),
        idleTimeToLive: '30 minutes',
    }).pipe(Scope.provide(scope));
    const languages = servers.flatMap((entry) => Array.cartesian([entry], Record.toEntries(entry[1].extensionToLanguage)));
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
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { registerLanguageServerTools };
