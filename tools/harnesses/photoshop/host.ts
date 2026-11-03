import { createHash } from 'node:crypto';
import type { Installation } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { Array, Effect, FileSystem, Option, Path, type PlatformError, Result, Schema, Stream, String } from 'effect';
import { ChildProcess } from 'effect/process';
import jsesc from 'jsesc';
import { HarnessError } from '#/error';
import { Document, Encoded, Engine, HostCase, type Placed, type Quit, Recorded, Reply, SURVEY, Survey } from '#/photoshop/contract';
import { precondition } from '#/photoshop/precondition';
import { type Batch, type Host as Harness, type Installed, type Live, recorded } from '#/session';
import { assembled } from '#/source';

// --- [CONSTANTS] -----------------------------------------------------------------------

const JAVASCRIPT = { command: 'miscDjxM', argument: 'JArg' } as const;
const EXTENSIONS: readonly string[] = ['jsx', 'jsxbin', 'js'];
const WRITABLE = /^[A-Z0-9]+$/u;
const SPACED = /\s+/u;
const ENGINE = new URL('extendscript.jsxinc', import.meta.url);

// --- [MODELS] --------------------------------------------------------------------------

const Info = Schema.fromJsonString(
    Schema.Struct({
        'Adobe Product Build': Schema.String.annotate({ description: 'Build each release and channel states, 20260929.r.26' }),
        RELEASECHANNEL: Schema.String.annotate({ description: 'public or beta' }),
    }),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PROCESSES]
const piped = Effect.fn('piped')(function* (command: string, args: readonly string[]) {
    const handle = yield* ChildProcess.make(command, args, { stdin: 'ignore', stderr: 'inherit' });
    const [bytes, code] = yield* Effect.all([Stream.mkUint8Array(handle.stdout), handle.exitCode], { concurrency: 'unbounded' });
    return yield* code === 0 ? Effect.succeed(bytes) : HarnessError.make({ reason: { _tag: 'Unavailable', because: `exited with code ${code}` }, subject: [command, ...args].join(' ') });
}, Effect.scoped);
const output = (command: string, args: readonly string[]) => Effect.map(piped(command, args), (bytes) => new TextDecoder().decode(bytes));

// --- [INSTALLATIONS]
const released = Effect.fn('released')(function* (installation: Installation) {
    const path = yield* Path.Path;
    const app = yield* path.fromFileUrl(installation.url);
    const [own, nested] = yield* Effect.all([Effect.flatMap(output('plutil', ['-convert', 'json', '-o', '-', path.join(app, 'Contents', 'Info.plist')]), Schema.decodeEffect(Info)), output('find', [path.join(app, 'Contents'), '-name', '*.app', '-prune'])], { concurrency: 'unbounded' });
    const version = yield* Effect.fromOption(installation.version, () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'Launch Services reports no version' }, subject: installation.name }));
    return { identifier: installation.identifier, name: installation.name, app, version: `${version}+${own['Adobe Product Build']}.${own.RELEASECHANNEL}`, watched: [installation.identifier, ...Array.fromIterable(String.linesIterator(nested)).filter((line) => line !== '')] } satisfies Installed;
});
const installed = Effect.gen(function* () {
    const automation = yield* Automation;
    const rows = yield* automation.installed;
    return yield* Effect.forEach(
        rows.filter((row) => row.scriptable && row.identifier === 'com.adobe.Photoshop'),
        released,
    );
});

// --- [SOURCE]
const engine = (scripts: readonly string[]): Result.Result<typeof Engine.Type, HarnessError> => {
    const unknown = scripts.filter((script) => !EXTENSIONS.some((extension) => script.endsWith(`.${extension}`)));
    return unknown.length > 0 ? Result.fail(HarnessError.make({ reason: { _tag: 'Unavailable', because: `holds no extension of ${EXTENSIONS.join(' ')}` }, subject: unknown.join(', ') })) : Result.succeed(Engine.literal);
};

// --- [CASES]
const placed = Effect.fn('placed')(function* (decoded: typeof Placed.Type, folder: string, product: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const copy = (open: string) => path.join(folder, path.basename(open));
    const documents = yield* Effect.forEach(
        decoded.documents,
        (document): Effect.Effect<typeof Document.Type, PlatformError.PlatformError> =>
            Document.match(document, {
                Made: Effect.succeed<typeof Document.cases.Made.Type>,
                Opened: ({ open }) => Effect.as(fs.copyFile(path.join(product, open), copy(open)), Document.cases.Opened.make({ open: copy(open) })),
            }),
    );
    return { precondition: { ...decoded, documents }, copies: decoded.documents.flatMap(Document.match({ Made: () => [], Opened: ({ open }) => [path.join(product, open)] })) };
});

// --- [DIGESTS]
const writable = Effect.runSync(Effect.cached(Effect.map(output('exiftool', ['-listwf']), (text) => new Set(text.split(SPACED).filter((token) => WRITABLE.test(token))))));
const digest = Effect.fn('digest')(function* (file: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const bytes = yield* fs.readFile(file);
    const content = (yield* writable).has(path.extname(file).slice(1).toUpperCase()) ? yield* piped('exiftool', ['-XMP:all=', '-EXIF:all=', '-o', '-', file]) : bytes;
    return { file, size: bytes.byteLength, sha256: createHash('sha256').update(content).digest('hex') };
});

// --- [TRANSPORT]
const executed = Effect.fn('executed')(function* (live: Live, argument: string) {
    const [automation, fs, path] = yield* Effect.all([Automation, FileSystem.FileSystem, Path.Path]);
    const file = path.join(live.scratch, 'harness.jsx');
    yield* fs.writeFileString(file, yield* assembled(ENGINE));
    const direct = `$.evalFile(new File(${jsesc(file, { quotes: 'double', wrap: true })}))`;
    const { reply } = yield* automation.execute({ pid: live.pid, command: JAVASCRIPT.command, arguments: { direct, [JAVASCRIPT.argument]: [argument] }, artifacts: [] });
    return reply;
});
const send = Effect.fn('send')(function* (live: Live, batch: Batch<typeof Survey.Type, typeof HostCase.Type>) {
    const reply = yield* executed(live, yield* Schema.encodeEffect(Encoded)({ records: batch.records, label: batch.label, survey: Option.isNone(batch.survey), cases: batch.cases }));
    const answer = yield* Schema.decodeUnknownEffect(Reply)(reply);
    return yield* 'failed' in answer ? HarnessError.make({ reason: { _tag: 'Unavailable', because: answer.failed }, subject: `${Engine.literal} batch` }) : Effect.succeed(answer);
});
// --- [COMPOSITION] ---------------------------------------------------------------------

const Host = {
    name: 'photoshop',
    Engine,
    survey: { question: SURVEY, Schema: Survey, host: (survey) => Option.some(survey.host) },
    Case: HostCase,
    shown: ['alerts', 'activations', 'errors'],
    progress: [],
    engine,
    precondition,
    placed,
    refusal: () => Option.none(),
    installed,
    windowed: () => true,
    launch: () => Effect.succeed({}),
    opened: () => Effect.succeedNone,
    send,
    records: (_, file) => recorded(Recorded)(file),
    quit: (live) => Effect.asVoid(executed(live, JSON.stringify({ quit: true } satisfies typeof Quit.Type))),
    digest,
    compare: { _tag: 'Imported' },
} as const satisfies Harness<typeof Survey, typeof HostCase, typeof Engine, typeof Placed.Type>;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Host };
