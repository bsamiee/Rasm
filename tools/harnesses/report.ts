import { homedir } from 'node:os';
import { fileURLToPath } from 'node:url';
import type { State } from '@rasm/automation/applications';
import { Array, ByteSize, DateTime, Duration, Effect, FileSystem, type LogLevel, Option, Path, Schema, String, UndefinedOr } from 'effect';
import { HarnessError } from '#/error';
import { printed } from '#/instance';
import { Kind, Run, tagOf } from '#/log';
import type { Halted } from '#/results';
import type { Host, Instance, Live, Services } from '#/session';

// --- [TYPES] ---------------------------------------------------------------------------

type Text = (contents: string, instance: Instance) => string;
interface Rule {
    readonly kind: typeof Kind.Type;
    readonly pattern: string;
    readonly root: string;
    readonly text?: Text;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const FOLDERS = [`${homedir()}/Library/Logs/DiagnosticReports`, '/Library/Logs/DiagnosticReports'];
const CLOCKS = Duration.seconds(2);
const LAUNCH = Duration.minutes(1);
const FIELD = /^(?<key>[A-Z][\w/ ]*?):\s+(?<value>.*\S)/gmu;
const SINCE = new Intl.DateTimeFormat('sv-SE', { dateStyle: 'short', timeStyle: 'medium', timeZone: 'UTC' });
const ENTRY = /^(?=Adobe InDesign Protective Shutdown Log$)/mu;
const SESSION = /^Session started up at (?<time>\d{2}:\d{2}) on \w+, (?<date>.+)$/mu;
const LOADED = /^.*\tLoaded\n/gmu;
const BREAKS = /\r\n?/gu;
const STAMP = /_(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})_(?<hour>\d{2})-(?<minute>\d{2})-(?<second>\d{2})_/u;
const UNSAMPLED = 'not sampling';

// --- [MODELS] --------------------------------------------------------------------------

const Ips = Schema.fromJsonString(
    Schema.Struct({
        pid: Schema.optionalKey(Schema.Int),
        procLaunch: Schema.optionalKey(Schema.DateTimeUtcFromString),
        date: Schema.optionalKey(Schema.DateTimeUtcFromString),
        processes: Schema.optionalKey(Schema.Array(Schema.Struct({ pid: Schema.Int, reason: Schema.optionalKey(Schema.String) }))),
        exception: Schema.optionalKey(Schema.Struct({ type: Schema.String, signal: Schema.optionalKey(Schema.String) })),
        faultingThread: Schema.optionalKey(Schema.Int),
        threads: Schema.optionalKey(Schema.Array(Schema.Struct({ frames: Schema.Array(Schema.Struct({ imageIndex: Schema.Int, imageOffset: Schema.Int, symbol: Schema.optionalKey(Schema.String) })) }))),
        usedImages: Schema.optionalKey(Schema.Array(Schema.Struct({ name: Schema.optionalKey(Schema.String) }))),
    }),
).annotate({ description: 'ReportCrash, ExcUserFault and JetsamEvent report body after its header line' });
const Diagnosed = Schema.Struct({
    PID: Schema.FiniteFromString,
    Path: Schema.String,
    'Date/Time': Schema.DateTimeUtcFromString,
    'End time': Schema.DateTimeUtcFromString,
    'Time Since Fork': Schema.optionalKey(Schema.TemplateLiteralParser([Schema.Finite, 's'])),
}).annotate({ description: 'Header fields of a spindump hang, cpu resource or disk writes report' });
const Stamp = Schema.Struct({
    year: Schema.FiniteFromString,
    month: Schema.FiniteFromString,
    day: Schema.FiniteFromString,
    hour: Schema.FiniteFromString,
    minute: Schema.FiniteFromString,
    second: Schema.FiniteFromString,
}).annotate({ description: 'Local launch time a log name states, UXPLogs_<stamp>_<us>.log and Illustrator*Session_<stamp>_PID<pid>.log' });

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [LINKS]
const lifetime = ({ started, ended }: Instance) => ({
    launch: { minimum: DateTime.subtractDuration(started, CLOCKS), maximum: DateTime.addDuration(started, LAUNCH) },
    alive: { minimum: DateTime.subtractDuration(started, CLOCKS), maximum: DateTime.addDuration(ended, CLOCKS) },
});
const local = (at: DateTime.DateTime.Input) => Option.map(DateTime.makeZoned(at, { timeZone: DateTime.zoneMakeLocal(), adjustForTimeZone: true }), DateTime.toUtc);
const ipsOf = (report: string) => Schema.decodeOption(Ips)(report.slice(report.indexOf('\n') + 1));
const launchedAs = (instance: Instance) => (report: string) => {
    const { launch, alive } = lifetime(instance);
    return Option.exists(
        ipsOf(report),
        ({ pid, procLaunch, date, processes = [] }) =>
            (pid === instance.pid && Option.exists(Option.fromUndefinedOr(procLaunch), (at) => DateTime.between(at, launch))) || (Option.exists(Option.fromUndefinedOr(date), (at) => DateTime.between(at, alive)) && processes.some((held) => held.pid === instance.pid && held.reason !== undefined)),
    );
};
const diagnosed = (instance: Instance) => (report: string) => {
    const { launch, alive } = lifetime(instance);
    return Option.exists(
        Schema.decodeUnknownOption(Diagnosed)(
            Object.fromEntries(
                Array.dedupeWith(
                    Array.fromIterable(report.matchAll(FIELD)).map(({ groups }) => [groups?.key ?? '', groups?.value ?? ''] as const),
                    ([left], [right]) => left === right,
                ),
            ),
        ),
        (fields) =>
            fields.PID === instance.pid &&
            fields.Path.startsWith(instance.chosen.app) &&
            UndefinedOr.match(fields['Time Since Fork'], {
                onUndefined: () => DateTime.between(fields['Date/Time'], alive),
                onDefined: ([seconds]) => DateTime.between(DateTime.subtractDuration(fields['End time'], Duration.seconds(seconds)), launch),
            }),
    );
};
const diagnostics = Effect.fn('diagnostics')(function* (suffixes: readonly string[], owned: (instance: Instance) => (report: string) => boolean, instance: Instance) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const files = (yield* Effect.forEach(FOLDERS, (folder) => fs.glob(path.join(folder, `*{${suffixes.join(',')}}`)))).flat();
    const { minimum } = lifetime(instance).alive;
    const recent = ({ mtime }: FileSystem.File.Info) => Option.exists(Option.map(mtime, DateTime.fromDateUnsafe), (at) => DateTime.isGreaterThanOrEqualTo(at, minimum));
    const fresh = yield* Effect.filter(files, (file) => Effect.map(fs.stat(file), recent), { concurrency: 'unbounded' });
    return yield* Effect.filter(fresh, (file) => Effect.map(fs.readFileString(file), owned(instance)), { concurrency: 'unbounded' });
});
const spun = Effect.fn('spun')(function* (instance: Instance) {
    const [fs, path, { folder }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Run]);
    const file = path.join(folder, `${instance.pid}.spin.log`);
    const shown = yield* printed('log', ['show', '--style', 'compact', '--predicate', 'subsystem == "com.apple.spindump" OR subsystem == "com.apple.libquit"', '--start', `${SINCE.format(DateTime.toDateUtc(instance.started))}+0000`]);
    const lines = Array.fromIterable(String.linesIterator(shown)).filter((line) => line.includes(`[${instance.pid}]`) && !line.includes(UNSAMPLED));
    return yield* Array.match(lines, { onEmpty: () => Effect.succeed([]), onNonEmpty: (held) => Effect.as(fs.writeFileString(file, `${held.join('\n')}\n`), [file]) });
});
const crashed = Effect.fn('crashed')(function* (exit: typeof State.cases.Exited.Type) {
    const fs = yield* FileSystem.FileSystem;
    const report = Option.map(Option.fromUndefinedOr(exit.report), ({ url }) => fileURLToPath(url));
    const ips = yield* Option.match(report, { onNone: () => Effect.succeedNone, onSome: (file) => Effect.map(Effect.option(fs.readFileString(file)), Option.flatMap(ipsOf)) });
    const thread = Option.flatMap(ips, ({ faultingThread, threads }) => Array.get(threads ?? [], faultingThread ?? 0));
    return {
        _tag: 'Crashed',
        ...(exit.status === undefined ? {} : { status: exit.status }),
        ...(exit.signal === undefined ? {} : { signal: exit.signal }),
        ...Option.match(report, { onNone: () => ({}), onSome: (held) => ({ report: held }) }),
        ...Option.match(
            Option.flatMap(ips, ({ exception }) => Option.fromUndefinedOr(exception)),
            { onNone: () => ({}), onSome: ({ type, signal }) => ({ type: [type, signal].filter((part) => part !== undefined).join(' ') }) },
        ),
        ...Option.match(
            Option.zipWith(thread, ips, ({ frames }, { usedImages }) => frames.map(({ imageIndex, imageOffset, symbol }) => `${usedImages?.[imageIndex]?.name ?? imageIndex} ${symbol ?? `+${imageOffset}`}`)),
            { onNone: () => ({}), onSome: (frames) => ({ frames }) },
        ),
    } satisfies typeof Halted.cases.Crashed.Type;
});

// --- [TEXT]
const whole: Text = (contents) => contents.replaceAll(BREAKS, '\n');
const entries: Text = (contents, instance) => {
    const { launch } = lifetime(instance);
    const ours = (entry: string) =>
        Option.exists(
            Option.flatMap(Option.fromNullishOr(SESSION.exec(entry)?.groups), ({ time, date }) => local(`${date} ${time}`)),
            (at) => DateTime.between(at, { ...launch, minimum: DateTime.startOf(instance.started, 'minute') }),
        );
    return whole(contents, instance).split(ENTRY).filter(ours).join('').replaceAll(LOADED, '');
};

// --- [TABLES] --------------------------------------------------------------------------

const KINDS: { readonly [K in typeof Kind.Type]: { readonly level: LogLevel.Severity; readonly system?: (instance: Instance) => Effect.Effect<readonly string[], Error, Services>; readonly text?: Text } } = {
    crash: { level: 'Warn', system: (instance) => diagnostics(['.ips'], launchedAs, instance), text: whole },
    hang: { level: 'Warn', system: (instance) => diagnostics(['.hang', '.diag'], diagnosed, instance), text: whole },
    spin: { level: 'Warn', system: spun, text: whole },
    shutdown: { level: 'Warn', text: entries },
    recovery: { level: 'Warn' },
    host: { level: 'Info', text: whole },
};

// --- [LOCATING] ------------------------------------------------------------------------

const located = Effect.fn('located')(function* (host: Pick<Host, 'logs'>, instance: Instance) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { launch, alive } = lifetime(instance);
    const systems = Array.getSomes(Kind.literals.map((kind) => Option.map(Option.fromUndefinedOr(KINDS[kind].system), (system) => [kind, system] as const)));
    const links = (file: string) =>
        Effect.mapBoth(fs.stat(file), {
            onFailure: HarnessError.refused(file),
            onSuccess: ({ birthtime, mtime, size }) =>
                ByteSize.toNumberUnsafe(size) > 0 &&
                Option.match(Option.flatMap(Schema.decodeUnknownOption(Stamp)(STAMP.exec(path.basename(file))?.groups), local), {
                    onSome: (at) => DateTime.between(at, launch),
                    onNone: () => Option.exists(Option.map(birthtime, DateTime.fromDateUnsafe), (at) => DateTime.between(at, launch)) || Option.exists(Option.map(mtime, DateTime.fromDateUnsafe), (at) => DateTime.between(at, alive)),
                }),
        });
    const rules = yield* host.logs?.(instance) ?? Effect.succeed([]);
    const [found, matched] = yield* Effect.all(
        [
            Effect.forEach(
                systems,
                ([kind, system]) =>
                    Effect.map(
                        system(instance),
                        Array.map((file) => ({ kind, file, text: KINDS[kind].text })),
                    ),
                { concurrency: 'unbounded' },
            ),
            Effect.forEach(rules, ({ kind, pattern, root, text = KINDS[kind].text }) => Effect.mapBoth(fs.glob(pattern, { root }), { onFailure: HarnessError.refused(pattern), onSuccess: Array.map((file) => ({ kind, file: path.join(root, file), text })) }), { concurrency: 'unbounded' }),
        ],
        { concurrency: 'unbounded' },
    );
    return [...found.flat(), ...(yield* Effect.filter(matched.flat(), ({ file }) => links(file), { concurrency: 'unbounded' }))];
});
const linked = (host: Pick<Host, 'logs'>, live: Pick<Live, 'pid' | 'started' | 'chosen'>) =>
    DateTime.now.pipe(
        Effect.flatMap((ended) => located(host, { ...live, ended })),
        Effect.flatMap(Effect.forEach(({ kind, file }) => Effect.annotateLogs(Effect.logWithLevel(KINDS[kind].level)('report linked'), { kind, file }), { discard: true })),
        Effect.catch((error) => Effect.annotateLogs(Effect.logWarning('report failed'), { tag: tagOf(error), error: error.message })),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { crashed, linked, local, located, type Rule, type Text };
