import { Array, Config, Deferred, Effect, FileSystem, Match, Option, Path, Schema, String, Struct } from 'effect';
import { Encoded, Engine, HostCase, HostOutcome, type Placed, Reply, SURVEY, Survey, Written } from '#/blender/contract';
import { precondition } from '#/blender/precondition';
import type { Case } from '#/cases';
import { HarnessError } from '#/harness-error';
import { appeared, Launched, plist, printed, Spawned } from '#/instance';
import { Run } from '#/log';
import { failing, lined, whole } from '#/report';
import { type Batch, Harness, type Installed, type Live, type Rebuilt } from '#/session';
import { engineOf, prepared } from '#/source';
import type { JsonObject } from '#/values';

// --- [CONSTANTS] -----------------------------------------------------------------------

const VERSION = /^Blender (?<release>\S+) ?(?<cycle>.*)$[\s\S]*?build hash: (?<hash>\w+)/mu;
const ENGINE = new URL('engine.py', import.meta.url);
const HOST = 'host.json';
const LOG = '.blender.log';
const ERRORS = '.stderr.txt';
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
    return { identifier: CFBundleIdentifier, name: CFBundleName, app, version, helpers: [] } satisfies Installed;
});
const installed = Effect.gen(function* () {
    const [listed, userbase] = yield* Effect.all([printed('mdfind', ['kMDItemCFBundleIdentifier == "org.blenderfoundation.blender"']), Config.option(Config.String('PYTHONUSERBASE'))]);
    const startup = Option.getOrUndefined(Option.map(userbase, (PYTHONUSERBASE) => ({ environment: { PYTHONUSERBASE } })));
    return (yield* Effect.forEach(Array.fromIterable(String.linesIterator(listed)).filter(String.isNonEmpty), identify)).map((chosen) => ({ ...chosen, ...startup }));
});

// --- [SOURCE]
const engine = engineOf({ [Engine.literal]: { extensions: ['py'] } }, Engine.literal);

// --- [CASES]
const windowed = (held: typeof JsonObject.Type, { until, confirms = [], files = [] }: Case) => until !== undefined || 'window' in held || 'area' in held || confirms.length > 0 || files.length > 0;
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
    const call: typeof HostCase.Type = { ...Struct.pick(hosted, ['name', 'precondition', 'read', 'keywords', 'until', 'confirms', 'files']), _tag: 'Call', folder, source, call: fn, args };
    return { hosted: call, map };
});

// --- [TRANSPORT]
const stemmed = (scratch: string) => Effect.map(Effect.all([Run, Path.Path]), ([{ folder }, path]) => path.join(folder, path.basename(scratch)));
const launch = Effect.fn('launch')(function* (chosen: Installed, scratch: string, shown: boolean) {
    const [fs, path, { folder }, stem] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Run, stemmed(scratch)]);
    const profile = path.join(scratch, 'profile');
    yield* fs.makeDirectory(profile, { recursive: true });
    yield* fs.writeFileString(`${stem}${ERRORS}`, '');
    const { executable } = yield* bundle(chosen.app);
    const args = [...(chosen.environment === undefined ? [] : ['--python-use-system-env']), '--log-file', `${stem}${LOG}`, '--log', 'operator', '--log-level', 'debug', '--python-exit-code', '1', '--python', yield* path.fromFileUrl(ENGINE), '--', scratch, HOST, `${stem}${ERRORS}`, SESSION, QUIT];
    const environment = { ...chosen.environment, BLENDER_USER_RESOURCES: profile, TMPDIR: folder };
    return shown ? new Launched({ arguments: ['--no-window-focus', '--enable-event-simulate', ...args], environment }) : new Spawned({ command: executable, arguments: ['--background', ...args], environment });
});
const opened = Effect.fn('opened')(function* (live: Live) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const written = yield* Effect.raceFirst(
        appeared(path.join(live.scratch, HOST)),
        Deferred.await(live.verdict).pipe(
            Effect.andThen(Effect.flatMap(stemmed(live.scratch), (stem) => fs.readFileString(`${stem}${ERRORS}`))),
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
    return yield* Reply.replied(`instance ${live.pid} batch`)(yield* appeared(reply));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const blender = Harness({
    Engine,
    survey: { question: SURVEY, Schema: Survey },
    Case: HostCase,
    Outcome: HostOutcome,
    engine,
    precondition,
    placed,
    installed,
    windowed,
    launch,
    opened,
    send,
    logs: (instance) =>
        Effect.map(Effect.all([Run, Path.Path]), ([{ folder }, path]) => [
            { kind: 'host', pattern: `${path.basename(instance.scratch)}{${LOG},${ERRORS}}`, root: folder, text: lined(failing) },
            { kind: 'crash', pattern: '*.crash.txt', root: folder, text: lined(whole) },
        ]),
    quit: (live) => Effect.flatMap(Effect.all([FileSystem.FileSystem, Path.Path]), ([fs, path]) => fs.writeFileString(path.join(live.scratch, QUIT), '')),
    rebuilt,
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { blender };
