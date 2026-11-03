import { Array, Deferred, Effect, FileSystem, Match, Option, Path, Schema, String, Struct } from 'effect';
import { Encoded, Engine, fields, HostCase, type Placed, Reply, SURVEY, Survey, Written } from '#/blender/contract';
import { precondition } from '#/blender/precondition';
import type { Row } from '#/cases';
import { HarnessError } from '#/error';
import { appeared, plist, printed, type Spawned } from '#/instance';
import { replied } from '#/results';
import type { Batch, Host as Harness, Installed, Launched, Live, Rebuilt } from '#/session';
import { engineOf, prepared } from '#/source';
import type { JsonObject } from '#/values';

// --- [CONSTANTS] -----------------------------------------------------------------------

const VERSION = /^Blender (?<release>\S+) ?(?<cycle>.*)$[\s\S]*?build hash: (?<hash>\w+)/mu;
const ENGINE = new URL('engine.py', import.meta.url);
const HOST = 'host.json';
const STDERR = 'stderr.txt';
const SESSION = 'session.json';
const QUIT = 'quit';

// --- [MODELS] --------------------------------------------------------------------------

const Bundle = Schema.Struct({ CFBundleName: Schema.String, CFBundleIdentifier: Schema.String, CFBundleExecutable: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INSTALLATIONS]
const bundle = Effect.fn('bundle')(function* (app: string) {
    const path = yield* Path.Path;
    const held = yield* plist(app, Bundle);
    return { ...held, executable: path.join(app, 'Contents', 'MacOS', held.CFBundleExecutable) };
});
const identify = Effect.fn('identify')(function* (app: string) {
    const { CFBundleName, CFBundleIdentifier, executable } = yield* bundle(app);
    const version = yield* Effect.fromOption(
        Option.map(Option.fromNullishOr(VERSION.exec(yield* printed(executable, ['--version']))?.groups), ({ release = '', cycle = '', hash = '' }) => `${release}${cycle === '' ? '' : `-${cycle.toLowerCase().replaceAll(' ', '-')}`}+${hash}`),
        () => HarnessError.unavailable(app)('printed no release and build hash'),
    );
    return { identifier: CFBundleIdentifier, name: CFBundleName, app, version, watched: [CFBundleIdentifier] } satisfies Installed;
});
const installed = Effect.flatMap(printed('mdfind', ['kMDItemCFBundleIdentifier == "org.blenderfoundation.blender"']), (listed) => Effect.forEach(Array.fromIterable(String.linesIterator(listed)).filter(String.isNonEmpty), identify));

// --- [SOURCE]
const engine = engineOf({ [Engine.literal]: { extensions: ['py'] } }, Engine.literal);

// --- [CASES]
const windowed = (held: typeof JsonObject.Type, { confirms = [], files = [] }: Row) => 'window' in held || 'area' in held || confirms.length > 0 || files.length > 0;
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
const rebuilt = Effect.fn('rebuilt')(function* ({ hosted, fn, args, module }: Rebuilt<typeof HostCase.Type>, folder: string) {
    const { source, map } = yield* prepared(module, Option.none());
    const call: typeof HostCase.Type = { ...Struct.pick(hosted, ['name', 'precondition', 'read', 'keywords', 'confirms', 'files']), _tag: 'Call', folder, source, call: fn, args };
    return { hosted: call, map };
});

// --- [TRANSPORT]
const launch = Effect.fn('launch')(function* (chosen: Installed, scratch: string, shown: boolean) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const profile = path.join(scratch, 'profile');
    yield* fs.makeDirectory(profile, { recursive: true });
    yield* fs.writeFileString(path.join(scratch, STDERR), '');
    const { executable } = yield* bundle(chosen.app);
    const args = ['--log-file', path.join(scratch, 'blender.log'), '--log', 'operator', '--log-level', 'debug', '--python-exit-code', '1', '--python', yield* path.fromFileUrl(ENGINE), '--', scratch, HOST, STDERR, SESSION, QUIT];
    const environment = { BLENDER_USER_RESOURCES: profile };
    return shown ? ({ arguments: ['--no-window-focus', '--enable-event-simulate', ...args], environment } satisfies Launched) : ({ _tag: 'Spawned', command: executable, arguments: ['--background', ...args], environment } satisfies Spawned);
});
const opened = Effect.fn('opened')(function* (live: Live) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const written = yield* Effect.raceFirst(
        appeared(path.join(live.scratch, HOST)),
        Deferred.await(live.verdict).pipe(
            Effect.andThen(fs.readFileString(path.join(live.scratch, STDERR))),
            Effect.flatMap((stderr) => HarnessError.unavailable(`${live.chosen.name} ${live.chosen.version}`)([`instance ${live.pid} ended before the engine wrote ${HOST}`, stderr.trim()].filter(String.isNonEmpty).join(': '))),
        ),
    );
    return yield* Schema.decodeEffect(Written)(written);
});
const send = Effect.fn('send')(function* (live: Live, batch: Batch<typeof Survey.Type, typeof HostCase.Type>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = path.dirname(batch.records);
    const [session, reply] = [path.join(folder, SESSION), path.join(folder, 'reply.json')];
    yield* fs.writeFileString(`${session}.tmp`, yield* Schema.encodeEffect(Encoded)({ records: batch.records, reply, survey: Option.isNone(batch.survey), cases: batch.cases }));
    yield* fs.rename(`${session}.tmp`, session);
    return yield* replied(`instance ${live.pid} batch`)(yield* Schema.decodeEffect(Reply)(yield* appeared(reply)));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const Host = {
    Engine,
    survey: { question: SURVEY, Schema: Survey },
    Case: HostCase,
    fields,
    shown: ['modal', 'warnings'],
    engine,
    precondition,
    placed,
    installed,
    windowed,
    launch,
    opened,
    send,
    quit: (live) => Effect.flatMap(Effect.all([FileSystem.FileSystem, Path.Path]), ([fs, path]) => fs.writeFileString(path.join(live.scratch, QUIT), '')),
    rebuilt,
} as const satisfies Harness<typeof Survey.Type, typeof HostCase.Type, typeof Engine.Type, typeof Placed.Type, typeof fields>;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Host };
