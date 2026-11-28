import { Buffer } from 'node:buffer';
import { hash } from 'node:crypto';
import { availableParallelism, homedir } from 'node:os';
import { basename, dirname, extname, join, relative, resolve } from 'node:path';
import process from 'node:process';
import { PassThrough } from 'node:stream';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { fromReadable, toReadable } from '@effect/platform-node/NodeStream';
import { getTouchingPropertyName } from '@typescript/native/ast';
import { API, type NodeHandle, type Project } from '@typescript/native/sync';
import { Array, Cause, Console, Effect, Fiber, FileSystem, HashSet, Match, Number, Option, PlatformError, Queue, Record, Result, Schema, Stream, String, Struct } from 'effect';
import { ChildProcess } from 'effect/process';
import { ConfigurationRequest, createMessageConnection, DidOpenTextDocumentNotification, InitializedNotification, type InitializeParams, InitializeRequest, type InitializeResult, type MessageConnection, NotificationType, ReferencesRequest, RequestType } from 'vscode-languageserver-protocol/node';

// --- [TYPES] ---------------------------------------------------------------------------

interface Site {
    readonly file: string;
    readonly line: number;
}
interface Declaration {
    readonly label: string;
    readonly name: string;
    readonly line: number;
    readonly character: number;
}
interface Target {
    readonly root: string;
    readonly file: string;
    readonly text: string;
    readonly declarations: Array.NonEmptyReadonlyArray<Declaration>;
}
interface Server {
    readonly command: Array.NonEmptyReadonlyArray<string>;
    readonly language: string;
    readonly opened?: (connection: MessageConnection) => Promise<unknown>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const DEADLINE = '10 minutes';
const QUIET = '1 second';
const INITIALIZE = new RequestType<Omit<InitializeParams, 'rootUri'>, InitializeResult, unknown>(InitializeRequest.method);
const SOLUTION = new NotificationType<{ readonly solution: string }>('solution/open');
const SYNCHRONIZE = new RequestType<{ readonly index: boolean }, unknown, unknown>('workspace/synchronize');
const BUILD = new RequestType<{ readonly displayName: string; readonly version: string; readonly bspVersion: string; readonly rootUri: string; readonly capabilities: { readonly languageIds: readonly string[] } }, unknown, unknown>('build/initialize');
const OPTIONS = new RequestType<{ readonly textDocument: { readonly uri: string }; readonly language: string }, unknown, unknown>('textDocument/sourceKitOptions');

// --- [MODELS] --------------------------------------------------------------------------

const Arguments = Schema.TupleWithRest(Schema.Tuple([Schema.NonEmptyString]), [Schema.NonEmptyString]);
const Member = Schema.Struct({
    name: Schema.String,
    symbolType: Schema.String,
    range: Schema.Struct({ byteOffset: Schema.Struct({ start: Schema.Int, end: Schema.Int }), start: Schema.Struct({ line: Schema.Int, column: Schema.Int }), end: Schema.Struct({ line: Schema.Int }) }),
});
const Item = Member.pipe(Schema.fieldsAssign({ isImport: Schema.Boolean, members: Member.pipe(Schema.Array, Schema.optionalKey) }));
const Outline = Schema.fromJsonString(Schema.Struct({ path: Schema.String, items: Schema.Array(Item) }));
const Progress = Schema.Struct({ token: Schema.Union([Schema.String, Schema.Int]), value: Schema.Struct({ kind: Schema.Literals(['begin', 'report', 'end']) }) });
const Position = Schema.Struct({ line: Schema.Int, character: Schema.Int });
const Lsif = Schema.fromJsonString(
    Schema.Union([
        Schema.Struct({ label: Schema.Literal('document'), id: Schema.Int, uri: Schema.String }),
        Schema.Struct({ label: Schema.Literal('range'), id: Schema.Int, start: Position, end: Position }),
        Schema.Struct({ label: Schema.Literal('contains'), outV: Schema.Int, inVs: Schema.Array(Schema.Int) }),
        Schema.Struct({ label: Schema.Literals(['next', 'textDocument/references']), outV: Schema.Int, inV: Schema.Int }),
        Schema.Struct({ label: Schema.Literal('item'), outV: Schema.Int, inVs: Schema.Array(Schema.Int), document: Schema.Int, property: Schema.Literal('references') }),
    ]),
);
const Presets = Schema.fromJsonString(
    Schema.Struct({
        configurePresets: Schema.Array(Schema.Struct({ name: Schema.String, inherits: Schema.optionalKey(Schema.Union([Schema.String, Schema.Array(Schema.String)])), binaryDir: Schema.optionalKey(Schema.String) })),
        workflowPresets: Schema.NonEmptyArray(Schema.Struct({ steps: Schema.NonEmptyArray(Schema.Struct({ type: Schema.String, name: Schema.String })) })),
    }),
);
const Store = Schema.Struct({ data: Schema.Struct({ indexStorePath: Schema.String }) });
const Flags = Schema.NullOr(Schema.Struct({ compilerArguments: Schema.Array(Schema.String) }));

// --- [ERRORS] --------------------------------------------------------------------------

class ReadersError extends Schema.TaggedError<ReadersError, { readonly brand: unique symbol }>()('ReadersError', {
    reason: Schema.TaggedUnion({ Unsupported: {}, Empty: {}, Unbuilt: {}, Refused: {} }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
    message: Schema.String,
}) {
    static readonly of = (reason: ReadersError['reason'], subject: string) => (cause?: unknown) => ReadersError.make({ reason, subject, ...(cause === undefined ? {} : { cause }), message: `${subject} ${String.toLowerCase(reason._tag)}` });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const spawned = Effect.fnUntraced(function* (root: string, argv: Array.NonEmptyReadonlyArray<string>, { input, accepted = [0] }: { readonly input?: string; readonly accepted?: readonly number[] } = {}) {
    const handle = yield* ChildProcess.make(Array.headNonEmpty(argv), Array.tailNonEmpty(argv), { cwd: root, ...(input === undefined ? {} : { stdin: Stream.make(new TextEncoder().encode(input)) }) });
    const [stdout, stderr, code] = yield* Effect.all([handle.stdout.pipe(Stream.decodeText, Stream.mkString), handle.stderr.pipe(Stream.decodeText, Stream.mkString), handle.exitCode], { concurrency: 'unbounded' });
    return accepted.includes(code) ? stdout : yield* ReadersError.of({ _tag: 'Refused' }, `${argv.join(' ')} exited ${code}: ${stderr.trim()}`)();
}, Effect.scoped);
const outlined = Effect.fnUntraced(function* (root: string, files: readonly string[]) {
    const text = yield* Array.match(files, { onEmpty: () => Effect.succeed(''), onNonEmpty: (listed) => spawned(root, ['ast-grep', 'outline', ...listed, '--items', 'all', '--view', 'expanded', '--json=stream']) });
    return yield* Effect.forEach(text.split('\n').filter(String.isNonEmpty), (line) => Schema.decodeEffect(Outline)(line));
});
const declared = (bytes: Buffer, outline: typeof Outline.Type): readonly Declaration[] =>
    outline.items
        .filter(({ isImport, symbolType }) => !isImport && symbolType !== 'namespace')
        .flatMap((item) => [{ entry: item, label: item.name }, ...(item.members ?? []).map((member) => ({ entry: member, label: `${item.name}.${member.name}` }))])
        .flatMap(({ entry: { name, range }, label }) => {
            const span = bytes.subarray(range.byteOffset.start, range.byteOffset.end).toString();
            const at = span.search(new RegExp(`(?<![\\p{ID_Continue}$])${RegExp.escape(name)}(?![\\p{ID_Continue}$])`, 'u'));
            const lines = span.slice(0, Math.max(at, 0) + name.length - (name.split(/::|\./u).at(-1) ?? name).length).split('\n');
            const indent = bytes.subarray(range.byteOffset.start - range.start.column, range.byteOffset.start).toString().length;
            return at < 0 ? [] : [{ label, name, line: range.start.line + lines.length - 1, character: (lines.length === 1 ? indent : 0) + (lines.at(-1)?.length ?? 0) }];
        });
const enclosing = ({ items }: typeof Outline.Type) =>
    new Map(
        items
            .filter(({ symbolType }) => symbolType !== 'namespace')
            .flatMap((item) => [{ range: item.range, label: item.isImport ? 'import' : item.name }, ...(item.members ?? []).map(({ range, name }) => ({ range, label: `${item.name}.${name}` }))])
            .flatMap(({ range, label }) => Array.range(range.start.line + 1, range.end.line + 1).map((line) => [line, label] as const)),
    );
const requesting =
    (connection: MessageConnection, exited: Effect.Effect<never, ReadersError | PlatformError.PlatformError>, refused: (step: string) => (cause?: unknown) => ReadersError) =>
    <P, R>(type: RequestType<P, R, unknown>, params: P) =>
        Effect.raceFirst(Effect.tryPromise({ try: () => connection.sendRequest<R>(type.method, params), catch: refused(type.method) }), exited);
const notifying =
    (connection: MessageConnection, refused: (step: string) => (cause?: unknown) => ReadersError) =>
    <P>(type: NotificationType<P>, params: P) =>
        Effect.tryPromise({ try: () => connection.sendNotification(type.method, params), catch: refused(type.method) });
const connected = Effect.fnUntraced(function* (root: string, command: Array.NonEmptyReadonlyArray<string>) {
    const input = new PassThrough();
    const refused = (step: string) => ReadersError.of({ _tag: 'Refused' }, `${command.join(' ')} ${step}`);
    const handle = yield* ChildProcess.make(Array.headNonEmpty(command), Array.tailNonEmpty(command), {
        cwd: root,
        stdin: fromReadable({ evaluate: () => input, onError: (cause) => PlatformError.systemError({ _tag: 'Unknown', module: 'Stream', method: 'read', cause }) }),
        forceKillAfter: '1 second',
    });
    const died = Effect.flatMap(Effect.all([handle.exitCode, handle.stderr.pipe(Stream.decodeText, Stream.mkString)], { concurrency: 'unbounded' }), ([code, stderr]) => refused(`exited ${code}: ${stderr.trim()}`)());
    const exited = Fiber.join(yield* Effect.forkScoped(died));
    const connection = yield* Effect.acquireRelease(
        Effect.map(toReadable(handle.stdout), (output) => createMessageConnection(output, input)),
        (opened) => Effect.sync(() => opened.dispose()),
    );
    const progress = yield* Queue.unbounded<unknown>();
    connection.onUnhandledProgress((event) => {
        Queue.offerUnsafe(progress, event);
    });
    connection.onRequest(ConfigurationRequest.type, ({ items }) => items.map(() => null));
    connection.onRequest(() => null);
    connection.listen();
    return { connection, progress, exited, requested: requesting(connection, exited, refused), notified: notifying(connection, refused) };
});
const served = Effect.fnUntraced(function* (server: Server, { root, file, text, declarations }: Target) {
    const { connection, progress, exited, requested, notified } = yield* connected(root, server.command);
    const uri = pathToFileURL(file).href;
    yield* requested(INITIALIZE, { processId: process.pid, workspaceFolders: [{ uri: pathToFileURL(root).href, name: basename(root) }], capabilities: { window: { workDoneProgress: true }, workspace: { configuration: true, workspaceFolders: true } } });
    yield* notified(InitializedNotification.type, {});
    yield* notified(DidOpenTextDocumentNotification.type, { textDocument: { uri, languageId: server.language, version: 1, text } });
    yield* Effect.raceFirst(Effect.tryPromise({ try: () => server.opened?.(connection) ?? Promise.resolve(), catch: ReadersError.of({ _tag: 'Refused' }, `${server.command.join(' ')} opened`) }), exited);
    const settled = Stream.fromQueue(progress).pipe(
        Stream.mapEffect((event) => Schema.decodeUnknownEffect(Progress)(event)),
        Stream.scan(HashSet.empty<string | number>, (active, { token, value }) =>
            Match.value(value.kind).pipe(
                Match.when('begin', () => HashSet.add(active, token)),
                Match.when('end', () => HashSet.remove(active, token)),
                Match.orElse(() => active),
            ),
        ),
        Stream.debounce(QUIET),
        Stream.filter(HashSet.isEmpty),
        Stream.runHead,
    );
    yield* Effect.raceFirst(settled, exited);
    const found = yield* Effect.forEach(declarations, ({ line, character }) => requested(ReferencesRequest.type, { textDocument: { uri }, position: { line, character }, context: { includeDeclaration: false } }), { concurrency: 'unbounded' });
    return found.map((locations) => (locations ?? []).map((location): Site => ({ file: fileURLToPath(location.uri), line: location.range.start.line + 1 })));
}, Effect.scoped);
const located = (file: string, project: Project, position: number) => (reference: NodeHandle) =>
    Option.flatMap(Option.fromUndefinedOr(reference.resolve(project)), (node) => {
        const source = node.getSourceFile();
        const start = node.getStart(source);
        return source.fileName === file && start === position ? Option.none() : Option.some<Site>({ file: source.fileName, line: source.getLineAndCharacterOfPosition(start).line + 1 });
    });
const typescript = Effect.fnUntraced(function* ({ root, file, declarations }: Target) {
    const configs = yield* spawned(root, ['git', 'ls-files', '--', '*tsconfig*.json']);
    const api = yield* Effect.acquireDisposable(Effect.sync(() => new API({ cwd: root })));
    const snapshot = yield* Effect.acquireDisposable(
        Effect.sync(() =>
            api.createSnapshot({
                openProjects: configs
                    .split('\n')
                    .filter(String.isNonEmpty)
                    .map((config) => join(root, config)),
            }),
        ),
    );
    const holding = Array.filterMap(snapshot.getProjects(), (project) =>
        Result.fromOption(
            Option.map(Option.fromUndefinedOr(project.program.getSourceFile(file)), (source) => ({ project, source })),
            () => project,
        ),
    );
    const referenced = ({ line, character }: Declaration) =>
        holding.flatMap(({ project, source }) => {
            const position = source.getPositionOfLineAndCharacter(line, character);
            return Array.getSomes(
                project.languageService
                    .getReferencedSymbolsForNode(getTouchingPropertyName(source, position), position)
                    .flatMap(Struct.get('references'))
                    .map(located(file, project, position)),
            );
        });
    return yield* Effect.try({
        try: () => declarations.map(referenced),
        catch: ReadersError.of({ _tag: 'Refused' }, 'tsgo references'),
    });
}, Effect.scoped);
const rust = Effect.fnUntraced(function* ({ root, file, declarations }: Target) {
    const fs = yield* FileSystem.FileSystem;
    const version = yield* spawned(root, ['rust-analyzer', '--version']);
    const listed = yield* spawned(root, ['git', 'ls-files', '-co', '--exclude-standard', '--', '*.rs', '*Cargo.toml', 'Cargo.lock']);
    const blobs = yield* spawned(root, ['git', 'hash-object', '--stdin-paths'], { input: listed });
    const folder = join(root, '.cache', 'rust-analyzer', 'lsif');
    const dump = join(folder, `${hash('sha256', `${version}${blobs}`)}.lsif`);
    const partial = `${dump}.${process.pid}.partial`;
    const written = Effect.gen(function* () {
        yield* fs.makeDirectory(folder, { recursive: true });
        const handle = yield* ChildProcess.make('rust-analyzer', ['lsif', root, '--exclude-vendored-libraries'], { cwd: root });
        const [, stderr, code] = yield* Effect.all([handle.stdout.pipe(Stream.run(fs.sink(partial))), handle.stderr.pipe(Stream.decodeText, Stream.mkString), handle.exitCode], { concurrency: 'unbounded' });
        yield* Effect.when(ReadersError.of({ _tag: 'Refused' }, `rust-analyzer lsif exited ${code}: ${stderr.trim().split('\n').at(-1) ?? ''}`)(), Effect.succeed(code !== 0));
        yield* fs.rename(partial, dump);
        const stale = (yield* fs.readDirectory(folder)).filter((name) => name.endsWith('.lsif') && join(folder, name) !== dump);
        yield* Effect.forEach(stale, (name) => fs.remove(join(folder, name), { force: true }), { discard: true });
    }).pipe(Effect.ensuring(fs.remove(partial, { force: true }).pipe(Effect.ignore)), Effect.scoped);
    yield* Effect.when(
        written,
        Effect.map(fs.exists(dump), (held) => !held),
    );
    const rows = Array.getSomes((yield* fs.readFileString(dump)).split('\n').map((line) => Schema.decodeOption(Lsif)(line)));
    const uris = new Map(rows.flatMap((row) => (row.label === 'document' ? [[row.id, row.uri] as const] : [])));
    const ranges = new Map(rows.flatMap((row) => (row.label === 'range' ? [[row.id, row] as const] : [])));
    const held = new Map(rows.flatMap((row) => (row.label === 'contains' ? Option.toArray(Option.map(Option.fromUndefinedOr(uris.get(row.outV)), (uri) => [uri, row.inVs] as const)) : [])));
    const following = new Map(rows.flatMap((row) => (row.label === 'next' ? [[row.outV, row.inV] as const] : [])));
    const referencing = new Map(rows.flatMap((row) => (row.label === 'textDocument/references' ? [[row.outV, row.inV] as const] : [])));
    const items = Map.groupBy(
        rows.flatMap((row) => (row.label === 'item' ? [row] : [])),
        ({ outV }) => outV,
    );
    const site = (at: number) => (id: number) => Option.zipWith(Option.fromUndefinedOr(uris.get(at)), Option.fromUndefinedOr(ranges.get(id)), (uri, { start }): Site => ({ file: fileURLToPath(uri), line: start.line + 1 }));
    const covering = (line: number, character: number) => (id: number) => Option.exists(Option.fromUndefinedOr(ranges.get(id)), ({ start, end }) => start.line === line && start.character <= character && character < end.character);
    return declarations.map(({ line, character }) =>
        Option.fromUndefinedOr(held.get(pathToFileURL(file).href)).pipe(
            Option.flatMap((ids) => Array.findFirst(ids, covering(line, character))),
            Option.flatMapNullishOr((id) => following.get(id)),
            Option.flatMapNullishOr((set) => referencing.get(set)),
            Option.flatMapNullishOr((result) => items.get(result)),
            Option.getOrElse(() => []),
            Array.flatMap(({ inVs, document: at }) => Array.getSomes(inVs.map(site(at)))),
        ),
    );
});
const compiled = Effect.fnUntraced(function* (root: string) {
    const fs = yield* FileSystem.FileSystem;
    const stated = yield* fs.readFileString(join(root, 'CMakePresets.json')).pipe(
        Effect.asSome,
        Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeedNone),
    );
    const presets = yield* Effect.transposeOption(Option.map(stated, Schema.decodeEffect(Presets)));
    const named = new Map(Option.match(presets, { onNone: () => [], onSome: ({ configurePresets }) => configurePresets.map((preset) => [preset.name, preset] as const) }));
    const tree = (name: string): Option.Option<string> => Option.flatMap(Option.fromUndefinedOr(named.get(name)), (preset) => Option.orElse(Option.fromUndefinedOr(preset.binaryDir), () => Array.findFirst(Array.ensure(preset.inherits ?? []), tree)));
    const configure = Option.map(presets, ({ workflowPresets }) => Array.headNonEmpty(Array.headNonEmpty(workflowPresets).steps).name);
    const dir = Option.flatMap(configure, (name) => Option.map(tree(name), (folder) => resolve(root, folder.replaceAll('${sourceDir}', root).replaceAll('${presetName}', name))));
    const built = yield* Option.match(dir, { onNone: () => Effect.succeed(true), onSome: (at) => fs.exists(join(at, 'compile_commands.json')) });
    yield* Effect.when(ReadersError.of({ _tag: 'Unbuilt' }, `compile_commands.json before cmake --preset ${Option.getOrElse(configure, () => '')}`)(), Effect.succeed(Option.isSome(configure) && !built));
    return Option.match(dir, { onNone: () => [], onSome: (at) => ['--compile-commands-dir', at] });
});
const configured = Effect.fnUntraced(function* (root: string, workspace: string) {
    const fs = yield* FileSystem.FileSystem;
    const custom = (yield* spawned(root, ['defaults', 'read', 'com.apple.dt.Xcode', 'IDECustomDerivedDataLocation'], { accepted: [0, 1] })).trim();
    const derived = String.isNonEmpty(custom) ? custom : join(homedir(), 'Library', 'Developer', 'Xcode', 'DerivedData');
    const owned = (entry: string) => Effect.map(spawned(root, ['plutil', '-extract', 'WorkspacePath', 'raw', '-o', '-', join(derived, entry, 'info.plist')], { accepted: [0, 1] }), (text) => text.trim() === join(root, workspace));
    const build = yield* Effect.flatMap(
        Effect.flatMap(fs.readDirectory(derived), (entries) => Effect.findFirst(entries, owned)),
        Effect.fromOption(ReadersError.of({ _tag: 'Unbuilt' }, `${derived} build folder of ${workspace}`)),
    );
    yield* Effect.acquireRelease(spawned(root, ['xcode-build-server', 'config', '-workspace', workspace, '--build_root', join(derived, build)]), () => Effect.ignore(fs.remove(join(root, 'buildServer.json'))));
});
const indexed = Effect.fnUntraced(function* (target: Target) {
    const fs = yield* FileSystem.FileSystem;
    const { root, file } = target;
    const workspace = yield* Effect.flatMap(fs.readDirectory(root), (entries) =>
        Effect.fromOption(
            Array.findFirst(entries, (entry) => extname(entry) === '.xcworkspace'),
            ReadersError.of({ _tag: 'Unbuilt' }, `${root} xcworkspace`),
        ),
    );
    yield* Effect.when(
        configured(root, workspace),
        Effect.map(fs.exists(join(root, 'buildServer.json')), (held) => !held),
    );
    const { requested } = yield* connected(root, ['xcode-build-server']);
    const { data } = yield* Effect.flatMap(requested(BUILD, { displayName: 'readers', version: '1', bspVersion: '2.2.0', rootUri: pathToFileURL(root).href, capabilities: { languageIds: ['swift'] } }), Schema.decodeUnknownEffect(Store));
    const flags = (source: string) =>
        requested(OPTIONS, { textDocument: { uri: pathToFileURL(source).href }, language: 'swift' }).pipe(
            Effect.flatMap(Schema.decodeUnknownEffect(Flags)),
            Effect.map((found) => Option.filter(Option.fromNullOr(found), ({ compilerArguments }) => compilerArguments.includes('-module-name'))),
        );
    const absent = Effect.flatMap(fs.readDirectory(join(root, workspace, 'xcshareddata', 'xcschemes')), (schemes) => ReadersError.of({ _tag: 'Unbuilt' }, `${relative(root, file)} module in the newest ${workspace} build log (nx build ${schemes.map((scheme) => basename(scheme, extname(scheme))).join(' or ')})`)());
    yield* Effect.when(absent, Effect.map(flags(file), Option.isNone));
    const unit = (path: string) => Effect.all({ text: Effect.map(fs.readFile(join(data.indexStorePath, path)), (bytes) => Buffer.from(bytes).toString('latin1')), stat: fs.stat(join(data.indexStorePath, path)) });
    const units = yield* Effect.flatMap(fs.glob('*/units/*', { root: data.indexStorePath }), (listed) => Effect.forEach(listed, unit, { concurrency: availableParallelism() }));
    const listed = yield* spawned(root, ['git', 'ls-files', '--', '*.swift']);
    const sources = yield* Effect.forEach(listed.split('\n').filter(String.isNonEmpty), (source) => Effect.map(fs.stat(join(root, source)), ({ mtime }) => ({ source, mtime: Option.getOrUndefined(mtime)?.getTime() ?? 0 })), {
        concurrency: availableParallelism(),
    });
    const recorded =
        ({ source, mtime }: (typeof sources)[number]) =>
        (held: (typeof units)[number]) =>
            (held.text.includes(`${dirname(source)}${basename(source)}`) || held.text.includes(`${dirname(join(root, source))}${basename(source)}`)) && (Option.getOrUndefined(held.stat.mtime)?.getTime() ?? 0) >= mtime;
    const refilled = (source: string) => (compilerArguments: readonly string[]) =>
        Effect.map(Effect.result(spawned(root, ['xcrun', 'swiftc', ...compilerArguments.filter((argument) => argument !== '-cache-compile-job'), '-index-file', '-index-file-path', join(root, source), '-index-store-path', data.indexStorePath, '-index-ignore-system-modules'])), (outcome) =>
            Result.isSuccess(outcome) ? Result.succeed(source) : Result.fail(source),
        );
    const filled = yield* Effect.forEach(
        sources.filter((source) => !units.some(recorded(source))),
        ({ source }) => Effect.flatMap(flags(join(root, source)), Option.match({ onNone: () => Effect.succeed(Result.fail(source)), onSome: ({ compilerArguments }) => refilled(source)(compilerArguments) })),
        { concurrency: availableParallelism() },
    );
    const [now, missed] = Array.partition(filled, (outcome) => outcome);
    return {
        sites: yield* served({ command: ['xcrun', 'sourcekit-lsp'], language: 'swift', opened: (connection) => connection.sendRequest(SYNCHRONIZE, { index: true }) }, target),
        notes: [...Array.match(now, { onEmpty: () => [], onNonEmpty: (names) => [`indexed now: ${names.join(', ')}`] }), ...Array.match(missed, { onEmpty: () => [], onNonEmpty: (names) => [`unindexed: ${names.join(', ')}`] })],
    };
}, Effect.scoped);
const solved = Effect.fnUntraced(function* (target: Target) {
    const fs = yield* FileSystem.FileSystem;
    const solution = yield* Effect.flatMap(fs.readDirectory(target.root), (entries) =>
        Effect.fromOption(
            Array.findFirst(entries, (entry) => ['.slnx', '.sln'].includes(extname(entry))),
            ReadersError.of({ _tag: 'Unbuilt' }, `${target.root} solution`),
        ),
    );
    const opened = (connection: MessageConnection) => connection.sendNotification(SOLUTION, { solution: pathToFileURL(join(target.root, solution)).href });
    return { sites: yield* served({ command: ['roslyn-language-server', '--stdio', '--telemetryLevel', 'off', `--clientProcessId=${process.pid}`], language: 'csharp', opened }, target), notes: [] };
});
const BACKENDS = {
    tsgo: { extensions: ['ts', 'tsx', 'mts', 'cts', 'js', 'jsx', 'mjs', 'cjs'], run: (target: Target) => Effect.map(typescript(target), (sites) => ({ sites, notes: [] })) },
    clangd: {
        extensions: ['c', 'cc', 'cpp', 'cxx', 'h', 'hh', 'hpp', 'hxx', 'm', 'mm'],
        run: (target: Target) => Effect.flatMap(compiled(target.root), (flags) => Effect.map(served({ command: ['clangd', '--background-index', ...flags], language: 'cpp' }, target), (sites) => ({ sites, notes: [] }))),
    },
    'ty server': { extensions: ['py', 'pyi'], run: (target: Target) => Effect.map(served({ command: ['uv', 'run', '--locked', 'ty', 'server'], language: 'python' }, target), (sites) => ({ sites, notes: [] })) },
    'rust-analyzer lsif': { extensions: ['rs'], run: (target: Target) => Effect.map(rust(target), (sites) => ({ sites, notes: [] })) },
    'roslyn-language-server': { extensions: ['cs'], run: solved },
    'sourcekit-lsp': { extensions: ['swift'], run: indexed },
};
const EXTENSIONS = new Map(Record.collect(BACKENDS, (tool, { extensions, run }) => extensions.map((extension) => [extension, { tool, run }] as const)).flat());
const readers = Effect.fnUntraced(function* (file: string, symbols: readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    const root = process.cwd();
    const named = relative(root, file);
    const backend = yield* Effect.fromOption(Option.fromUndefinedOr(EXTENSIONS.get(extname(file).slice(1))), ReadersError.of({ _tag: 'Unsupported' }, `${extname(file)} outside ${[...EXTENSIONS.keys()].join(' ')}`));
    const bytes = Buffer.from(yield* fs.readFile(file));
    const declarations = (yield* outlined(root, [named])).flatMap((outline) => declared(bytes, outline)).filter(({ label, name }) => Array.isReadonlyArrayEmpty(symbols) || symbols.includes(label) || symbols.includes(name));
    const listed = yield* Array.match(declarations, { onEmpty: () => ReadersError.of({ _tag: 'Empty' }, `${named} declarations ${symbols.join(' ')}`)(), onNonEmpty: Effect.succeed });
    const answered = yield* backend.run({ root, file, text: bytes.toString(), declarations: listed });
    const relocated = answered.sites.map((held) => held.map((site) => ({ file: relative(root, site.file), line: site.line })));
    const ignored = new Set((yield* spawned(root, ['git', 'check-ignore', '--stdin'], { input: relocated.flat().map(Struct.get('file')).join('\n'), accepted: [0, 1, 128] })).split('\n'));
    const found = Array.zip(listed, relocated).map(([declaration, held]) => ({
        label: `${declaration.label}:${declaration.line + 1}`,
        sites: Array.dedupeWith(
            held.filter((site) => !ignored.has(site.file)),
            (left, right) => left.file === right.file && left.line === right.line,
        ),
    }));
    const labels = new Map((yield* outlined(root, Array.dedupe(found.flatMap(({ sites }) => sites.map(Struct.get('file')))))).map((outline) => [outline.path, enclosing(outline)] as const));
    const rows = join('.artifacts', 'code', 'readers', `${named}.tsv`);
    const [read, unread] = Array.partition(found, (row) => (Array.isReadonlyArrayNonEmpty(row.sites) ? Result.succeed(row) : Result.fail(row.label)));
    const lines = read.flatMap(({ label, sites }) => sites.map((site) => [label, `${site.file}:${site.line}`, labels.get(site.file)?.get(site.line) ?? '-'].join('\t')));
    yield* fs.makeDirectory(dirname(rows), { recursive: true });
    yield* fs.writeFileString(rows, `${lines.join('\n')}\n`);
    return [
        `readers: ${named}`,
        `backend: ${backend.tool}`,
        `rows: ${rows}`,
        `declarations: ${found.length}`,
        `sites: ${Number.sumAll(read.map(({ sites }) => sites.length))}`,
        ...read.map(({ label, sites }) => `    ${[label, sites.length, Array.dedupe(sites.map(Struct.get('file'))).length].join('\t')}`),
        ...Array.match(unread, { onEmpty: () => [], onNonEmpty: (names) => [`unread: ${names.join(', ')}`] }),
        ...answered.notes,
    ].join('\n');
});

// --- [COMPOSITION] ---------------------------------------------------------------------

Schema.decodeUnknownEffect(Arguments)(process.argv.slice(2)).pipe(
    Effect.flatMap(([file, ...symbols]) => readers(resolve(file), symbols)),
    Effect.timeoutOrElse({ duration: DEADLINE, orElse: () => Effect.fail(new Cause.TimeoutError(`readers ran past ${DEADLINE}`)) }),
    Effect.flatMap(Console.log),
    Effect.provide(layer),
    runMain(),
);
