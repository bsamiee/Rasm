import type { Installation } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { Array, Effect, FileSystem, Option, Path, Schema, String } from 'effect';
import jsesc from 'jsesc';
import { ADOBE_CRASH_REPORTER, checked, plist, printed } from '#/instance';
import { Encoded, Engine, fields, HostCase, type Placed, type Quit, Reply, SURVEY, Survey } from '#/photoshop/contract';
import { precondition } from '#/photoshop/precondition';
import { copied } from '#/precondition';
import { replied } from '#/results';
import { type Batch, type Host as Harness, type Installed, installation, type Live } from '#/session';
import { assembled, EXTENDSCRIPT, engineOf } from '#/source';

// --- [CONSTANTS] -----------------------------------------------------------------------

const WRITABLE = /^[A-Z0-9]+$/u;
const SPACED = /\s+/u;
const ENGINE = new URL('extendscript.jsxinc', import.meta.url);

// --- [MODELS] --------------------------------------------------------------------------

const Info = Schema.Struct({
    'Adobe Product Build': Schema.String.annotate({ description: 'Build each release and channel states, 20260929.r.26' }),
    RELEASECHANNEL: Schema.String.annotate({ description: 'public or beta' }),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INSTALLATIONS]
const released = Effect.fn('released')(function* (row: Installation, rows: readonly Installation[]) {
    const path = yield* Path.Path;
    const app = yield* path.fromFileUrl(row.url);
    const [own, nested] = yield* Effect.all([plist(app, Info), printed('find', [path.join(app, 'Contents'), '-name', '*.app', '-prune'])], { concurrency: 'unbounded' });
    const held = yield* installation(row, rows, [ADOBE_CRASH_REPORTER, ...Array.fromIterable(String.linesIterator(nested)).filter(String.isNonEmpty)]);
    return { ...held, version: `${held.version}+${own['Adobe Product Build']}.${own.RELEASECHANNEL}` } satisfies Installed;
});
const installed = Effect.gen(function* () {
    const rows = yield* (yield* Automation).installed;
    return yield* Effect.forEach(
        rows.filter((row) => row.scriptable && row.identifier === 'com.adobe.Photoshop'),
        (row) => released(row, rows),
    );
});

// --- [SOURCE]
const engine = engineOf({ [Engine.literal]: { extensions: EXTENDSCRIPT } }, Engine.literal);

// --- [DIGESTS]
const writable = Effect.runSync(Effect.cached(Effect.map(printed('exiftool', ['-listwf']), (text) => new Set(text.split(SPACED).filter((token) => WRITABLE.test(token))))));
const canonical = Effect.fn('canonical')(function* (file: string, bytes: Uint8Array) {
    const path = yield* Path.Path;
    return [(yield* writable).has(path.extname(file).slice(1).toUpperCase()) ? yield* checked('exiftool', ['-XMP:all=', '-EXIF:all=', '-o', '-', file]) : bytes];
});

// --- [TRANSPORT]
const executed = Effect.fn('executed')(function* (live: Live, argument: string) {
    const [automation, fs, path] = yield* Effect.all([Automation, FileSystem.FileSystem, Path.Path]);
    const file = path.join(live.scratch, 'harness.jsx');
    yield* fs.writeFileString(file, yield* assembled(ENGINE));
    const direct = `$.evalFile(new File(${jsesc(file, { quotes: 'double', wrap: true })}))`;
    const { reply } = yield* automation.script({ pid: live.pid, direct, values: [argument] });
    return reply;
});
const send = Effect.fn('send')(function* (live: Live, batch: Batch<typeof Survey.Type, typeof HostCase.Type>) {
    const reply = yield* executed(live, yield* Schema.encodeEffect(Encoded)({ records: batch.records, label: batch.label, survey: Option.isNone(batch.survey), cases: batch.cases }));
    return yield* replied(`${Engine.literal} batch`)(yield* Schema.decodeUnknownEffect(Reply)(reply));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const Host = {
    Engine,
    survey: { question: SURVEY, Schema: Survey },
    Case: HostCase,
    fields,
    shown: ['alerts', 'activations', 'errors'],
    engine,
    precondition,
    placed: copied,
    installed,
    launch: () => Effect.succeed({}),
    send,
    quit: (live) => Effect.asVoid(executed(live, JSON.stringify({ quit: true } satisfies typeof Quit.Type))),
    canonical,
} as const satisfies Harness<typeof Survey.Type, typeof HostCase.Type, typeof Engine.Type, typeof Placed.Type, typeof fields>;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Host };
