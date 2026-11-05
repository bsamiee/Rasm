import type { Installation } from '@rasm/applications';
import { Automation } from '@rasm/applications/node';
import { Effect, FileSystem, Option, Path, Schema } from 'effect';
import { copied } from '#/cases/precondition';
import { Contract, type Quit } from '#/hosts/adobe/photoshop/contract';
import { precondition } from '#/hosts/adobe/photoshop/precondition';
import { ADOBE, crashes, REPORTER, uxp } from '#/hosts/adobe/reports';
import { type Batch, Harness, type Installed, installation, type Live, scripted } from '#/session/harness';
import { checked, Launched, plist, printed } from '#/session/instance';
import { assembled } from '#/session/source';

// --- [CONSTANTS] -----------------------------------------------------------------------

const WRITABLE = /^[A-Z0-9]+$/u;
const SPACED = /\s+/u;
const ENGINE = Effect.runSync(Effect.cached(assembled(new URL('engine/main.ts', import.meta.url))));

// --- [MODELS] --------------------------------------------------------------------------

const Info = Schema.Struct({
    'Adobe Product Build': Schema.String.annotate({ description: 'Build each release and channel states, 20260929.r.26' }),
    RELEASECHANNEL: Schema.String.annotate({ description: 'public or beta' }),
    CFBundleDisplayName: Schema.String.annotate({ description: 'Adobe Photoshop 2026 or Adobe Photoshop (Beta), naming the folder its UXP logs sit in' }),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INSTALLATIONS]
const released = Effect.fn('released')(function* (row: Installation, rows: readonly Installation[]) {
    const held = yield* installation(row, rows, [REPORTER]);
    const own = yield* plist(held.app, Info);
    return { ...held, version: `${held.version}+${own['Adobe Product Build']}.${own.RELEASECHANNEL}` } satisfies Installed;
});
const installed = Effect.gen(function* () {
    const rows = yield* Automation.use((automation) => automation.installed(ADOBE));
    return yield* Effect.forEach(
        rows.filter((row) => row.scriptable && row.identifier === 'com.adobe.Photoshop'),
        (row) => released(row, rows),
    );
});

// --- [DIGESTS]
const writable = Effect.runSync(Effect.cached(Effect.map(printed('exiftool', ['-listwf']), (text) => new Set(text.split(SPACED).filter((token) => WRITABLE.test(token))))));
const canonical = Effect.fn('canonical')(function* (file: string, bytes: Uint8Array) {
    const path = yield* Path.Path;
    return [(yield* writable).has(path.extname(file).slice(1).toUpperCase()) ? yield* checked('exiftool', ['-XMP:all=', '-EXIF:all=', '-o', '-', file]) : bytes];
});

// --- [TRANSPORT]
const executed = Effect.fn('executed')(function* (live: Live, argument: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const file = path.join(live.scratch, 'harness.jsx');
    yield* fs.writeFileString(file, (yield* ENGINE).code);
    const { reply } = yield* scripted(live, { direct: '$.evalFile(new File(arguments[1]))', values: [argument, file] });
    return reply;
});
const send = Effect.fn('send')(function* (live: Live, batch: Batch<typeof Contract.Survey.Type, typeof Contract.Case.Type>) {
    const reply = yield* executed(live, yield* Schema.encodeEffect(Contract.Encoded)({ records: batch.records, label: batch.label, survey: Option.isNone(batch.survey), cases: batch.cases }));
    return yield* Contract.Reply.replied(`instance ${live.pid} batch`)(reply);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const photoshop = Harness({
    ...Contract,
    assembled: ENGINE,
    precondition,
    placed: copied,
    installed,
    launch: () => Effect.succeed(new Launched({})),
    send,
    quit: (live) => Effect.asVoid(executed(live, JSON.stringify({ quit: true } satisfies typeof Quit.Type))),
    canonical,
    logs: (instance) => Effect.zipWith(plist(instance.chosen.app, Info), crashes(instance), ({ CFBundleDisplayName: folder }, found) => [uxp(folder), ...found]),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { photoshop };
