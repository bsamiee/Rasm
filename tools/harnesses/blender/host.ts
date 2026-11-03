import { createHash } from 'node:crypto';
import { Array, Deferred, type Duration, Effect, FileSystem, identity, Match, Option, Path, Result, Schema, Stream, String, Struct } from 'effect';
import { ChildProcess } from 'effect/process';
import { Encoded, Engine, HostCase, type Placed, Recorded, Reply, SURVEY, Survey, Written } from '#/blender/contract';
import { precondition } from '#/blender/precondition';
import type { Batch as Authored, Row } from '#/cases';
import { HarnessError } from '#/error';
import type { Spawned } from '#/instance';
import { merged } from '#/precondition';
import { type Batch, execute, type Host as Harness, type Installed, type Launched, type Live, type Prepared, type Rebuilt, recorded } from '#/session';
import { prepared } from '#/source';

// --- [TYPES] ---------------------------------------------------------------------------

type Hosting = Harness<typeof Survey, typeof HostCase, typeof Engine, typeof Placed.Type>;
type Rerun = Extract<Hosting['compare'], { readonly _tag: 'Hosted' }>['run'];

// --- [CONSTANTS] -----------------------------------------------------------------------

const VERSION = /^Blender (?<release>\S+) ?(?<cycle>.*)$[\s\S]*?build hash: (?<hash>\w+)/mu;
const ENGINE = new URL('engine.py', import.meta.url);
const HOST = 'host.json';
const STDERR = 'stderr.txt';
const REBUILT = 'rebuilt';
const EXTENSION = '.py';

// --- [MODELS] --------------------------------------------------------------------------

const Bundle = Schema.fromJsonString(Schema.Struct({ CFBundleName: Schema.String, CFBundleIdentifier: Schema.String, CFBundleExecutable: Schema.String }));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INSTALLATIONS]
const output = Effect.fn('output')(function* (command: string, args: readonly string[]) {
    const handle = yield* ChildProcess.make(command, args, { stdin: 'ignore' });
    const [text, errors, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? text.trim() : yield* HarnessError.make({ reason: { _tag: 'Unavailable', because: `exited with code ${code}: ${errors}${text}` }, subject: [command, ...args].join(' ') });
}, Effect.scoped);
const bundle = Effect.fn('bundle')(function* (app: string) {
    const path = yield* Path.Path;
    const plist = yield* Effect.flatMap(output('plutil', ['-convert', 'json', '-o', '-', path.join(app, 'Contents', 'Info.plist')]), Schema.decodeEffect(Bundle));
    return { ...plist, executable: path.join(app, 'Contents', 'MacOS', plist.CFBundleExecutable) };
});
const identify = Effect.fn('identify')(function* (app: string) {
    const { CFBundleName, CFBundleIdentifier, executable } = yield* bundle(app);
    const printed = yield* output(executable, ['--version']);
    const version = yield* Effect.fromOption(
        Option.map(Option.fromNullishOr(VERSION.exec(printed)?.groups), ({ release = '', cycle = '', hash = '' }) => `${release}${cycle === '' ? '' : `-${cycle.toLowerCase().replaceAll(' ', '-')}`}+${hash}`),
        () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'printed no release and build hash' }, subject: app }),
    );
    return { identifier: CFBundleIdentifier, name: CFBundleName, app, version, watched: [CFBundleIdentifier] } satisfies Installed;
});
const installed = Effect.flatMap(output('mdfind', ['kMDItemCFBundleIdentifier == "org.blenderfoundation.blender"']), (listed) => Effect.forEach(Array.fromIterable(String.linesIterator(listed)).filter(String.isNonEmpty), identify));

// --- [SOURCE]
const engine = (scripts: readonly string[]): Result.Result<typeof Engine.Type, HarnessError> => {
    const unknown = scripts.filter((script) => !script.endsWith(EXTENSION));
    return unknown.length === 0 ? Result.succeed(Engine.literal) : Result.fail(HarnessError.make({ reason: { _tag: 'Unavailable', because: `holds no ${EXTENSION} extension` }, subject: unknown.join(', ') }));
};

// --- [CASES]
const windowed = ({ precondition: shared }: typeof Authored.Type, row: Row) => {
    const held = merged(shared ?? {}, row.precondition ?? {});
    return 'window' in held || 'area' in held || (row.confirms ?? []).length > 0 || (row.files ?? []).length > 0;
};
const placed = Effect.fn('placed')(function* (decoded: typeof Placed.Type, folder: string, product: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    return yield* Match.value(decoded.document).pipe(
        Match.when({ open: Match.string }, (document) => {
            const source = path.join(product, document.open);
            const copy = path.join(folder, path.basename(document.open));
            return Effect.as(fs.copyFile(source, copy), { precondition: { ...decoded, document: { ...document, open: copy } }, copies: [source] });
        }),
        Match.orElse(() => Effect.succeed({ precondition: decoded, copies: [] })),
    );
});
const digest = Effect.fn('digest')(function* (file: string) {
    const bytes = yield* (yield* FileSystem.FileSystem).readFile(file);
    return { file, size: bytes.byteLength, sha256: createHash('sha256').update(bytes).digest('hex') };
});

// --- [TRANSPORT]
const appeared = (folder: string, file: string) =>
    Effect.flatMap(FileSystem.FileSystem, (fs) =>
        Stream.concat(
            Stream.make(undefined),
            Stream.map(fs.watch(folder), () => undefined),
        ).pipe(
            Stream.mapEffect(() => fs.exists(file)),
            Stream.filter(identity),
            Stream.runHead,
        ),
    );
const launch = Effect.fn('launch')(function* (chosen: Installed, scratch: string, shown: boolean) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const profile = path.join(scratch, 'profile');
    yield* fs.makeDirectory(profile, { recursive: true });
    const { executable } = yield* bundle(chosen.app);
    const args = ['--log-file', path.join(scratch, 'blender.log'), '--log', 'operator', '--log-level', 'debug', '--python-exit-code', '1', '--python', yield* path.fromFileUrl(ENGINE), '--', scratch];
    const environment = { BLENDER_USER_RESOURCES: profile };
    return shown ? ({ arguments: ['--no-window-focus', '--enable-event-simulate', ...args], environment } satisfies Launched) : ({ _tag: 'Spawned', command: executable, arguments: ['--background', ...args], environment } satisfies Spawned);
});
const opened = Effect.fn('opened')(function* (live: Live) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const file = path.join(live.scratch, HOST);
    yield* Effect.raceFirst(
        appeared(live.scratch, file),
        Effect.flatMap(Deferred.await(live.verdict), () =>
            Effect.flatMap(fs.readFileString(path.join(live.scratch, STDERR)), (printed) => HarnessError.make({ reason: { _tag: 'Unavailable', because: `instance ${live.pid} ended before the engine wrote ${HOST}: ${printed.trim()}` }, subject: `${live.chosen.name} ${live.chosen.version}` })),
        ),
    );
    return Option.some(yield* Schema.decodeEffect(Written)(yield* fs.readFileString(file)));
});
const send = Effect.fn('send')(function* (live: Live, batch: Batch<typeof Survey.Type, typeof HostCase.Type>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = path.dirname(batch.records);
    const [session, reply] = [path.join(folder, 'session.json'), path.join(folder, 'reply.json')];
    yield* fs.writeFileString(`${session}.tmp`, yield* Schema.encodeEffect(Encoded)({ records: batch.records, reply, survey: Option.isNone(batch.survey), cases: batch.cases }));
    yield* fs.rename(`${session}.tmp`, session);
    yield* appeared(folder, reply);
    const answer = yield* Schema.decodeEffect(Reply)(yield* fs.readFileString(reply));
    return yield* 'failed' in answer ? HarnessError.make({ reason: { _tag: 'Unavailable', because: answer.failed }, subject: `instance ${live.pid} batch` }) : Effect.succeed(answer);
});
// --- [COMPARING]
const run: Rerun = Effect.fn('run')(function* (chosen: Installed, calls: readonly Rebuilt<typeof HostCase.Type>[], scratch: string, limit: Duration.Duration, authored: typeof Authored.Type) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const root = path.join(scratch, REBUILT);
    const sendable = yield* Effect.forEach(calls, ({ hosted, fn, args, module }, index) =>
        Effect.gen(function* () {
            const folder = path.join(root, `${index}`);
            yield* fs.makeDirectory(folder, { recursive: true });
            const { source, map } = yield* prepared(module, Option.none());
            const rebuilt: typeof HostCase.Type = { ...Struct.pick(hosted, ['name', 'precondition', 'read', 'keywords', 'confirms', 'files']), _tag: 'Call', folder, source, call: fn, args };
            return { hosted: rebuilt, name: hosted.name, folder, map, presses: [], copies: [] } satisfies Prepared<typeof HostCase.Type>;
        }),
    );
    const done = yield* execute(Host, chosen, root, REBUILT, limit, Option.none(), () => Effect.succeed({ engine: Engine.literal, sendable, settled: [] }), authored);
    return done.outcomes;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const Host = {
    name: 'blender',
    Engine,
    survey: { question: SURVEY, Schema: Survey, host: (survey) => Option.some(survey.host) },
    Case: HostCase,
    shown: ['modal', 'warnings'],
    progress: [],
    engine,
    precondition,
    placed,
    refusal: () => Option.none(),
    installed,
    windowed,
    launch,
    opened,
    send,
    records: (_, file) => recorded(Recorded)(file),
    quit: (live) => Effect.flatMap(Effect.all([FileSystem.FileSystem, Path.Path]), ([fs, path]) => fs.writeFileString(path.join(live.scratch, 'quit'), '')),
    digest,
    compare: { _tag: 'Hosted', run },
} as const satisfies Hosting;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Host };
