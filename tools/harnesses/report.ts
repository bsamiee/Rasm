import { homedir } from 'node:os';
import { fileURLToPath } from 'node:url';
import type { State } from '@rasm/automation';
import { Array, ByteSize, DateTime, Duration, Effect, FileSystem, type LogLevel, Option, Path, Predicate, Record, Schema, String, Struct, UndefinedOr } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import { HarnessError } from '#/harness-error';
import { printedLines } from '#/instance';
import { type Kind, type Messages, messageOf, record, tagOf } from '#/log';
import type { Halted } from '#/results';
import type { Harness, Instance } from '#/session';

// --- [TYPES] ---------------------------------------------------------------------------

type Lifetime = Pick<Instance, 'pid' | 'started' | 'ended'> & { readonly chosen: Pick<Instance['chosen'], 'app'> };
type Facts = Omit<(typeof Messages)['report linked']['Encoded'], 'kind' | 'file' | 'pid' | 'started' | 'case'>;
type Text = (contents: string, instance: Instance) => Option.Option<Facts>;
type Diagnosis = (contents: string, instance: Pick<Lifetime, 'pid'>) => Option.Option<Facts>;
interface Rule {
    readonly kind: typeof Kind.Type;
    readonly pattern: string;
    readonly root: string;
    readonly text?: Text;
}
interface Linked {
    readonly kind: typeof Kind.Type;
    readonly at: DateTime.Utc;
    readonly file: Option.Option<string>;
    readonly facts: Facts;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOME = homedir();
const FOLDERS = [`${HOME}/Library/Logs/DiagnosticReports`, '/Library/Logs/DiagnosticReports'];
const CLOCKS = Duration.seconds(2);
const LAUNCH = Duration.minutes(1);
const FIELD = /^(?<key>[A-Z][\w/ ]*?):\s+(?<value>.*\S)/gmu;
const SINCE = new Intl.DateTimeFormat('sv-SE', { dateStyle: 'short', timeStyle: 'medium', timeZone: 'UTC' });
const ENTRY = /^(?=Adobe InDesign Protective Shutdown Log$)/mu;
const SESSION = /^Session started up at (?<time>\d{2}:\d{2}) on \w+, (?<date>.+)$/mu;
const TABLE = /^Registered plugins:\n.*\n-+\n(?:.*\t.*\n)*/gmu;
const BREAKS = /\r\n?/gu;
const STAMP = /_(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})_(?<hour>\d{2})-(?<minute>\d{2})-(?<second>\d{2})_/u;
const BODY = /^\{\n[\s\S]*?^\}$/mu;
const HEAVIEST = /^Heaviest stack for the (?:main thread of the )?target process:\n(?<block>(?:.+\n)+)/mu;
const STACKED = /^\s*\*?\d+\s+(?<symbol>.+?)(?: \+ \d+)?(?: \((?<image>[^()]+?) \+ \d+\))?(?: \[0x[\da-f]+\])?$/u;
const FAILING = /\b(?:error|exception|fatal|traceback)\b/iu;
const TRACED = /^Traceback \(most recent call last\):$/u;
const UNREADABLE = /[\0�]/u;

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
).annotate({ description: 'ReportCrash, ExcUserFault, JetsamEvent and Adobe crash reporter report body, the pretty-printed object after the header line' });
const Diagnosed = Schema.Struct({
    PID: Schema.FiniteFromString,
    Path: Schema.String,
    Event: Schema.String,
    'Date/Time': Schema.DateTimeUtcFromString,
    'End time': Schema.DateTimeUtcFromString,
    'Time Since Fork': Schema.optionalKey(Schema.TemplateLiteralParser([Schema.Finite, 's'])),
    Duration: Schema.optionalKey(Schema.TemplateLiteralParser([Schema.Finite, 's'])),
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
const lifetime = ({ started, ended }: Lifetime) => ({
    launch: { minimum: DateTime.subtractDuration(started, CLOCKS), maximum: DateTime.min(DateTime.addDuration(started, LAUNCH), ended) },
    alive: { minimum: DateTime.subtractDuration(started, CLOCKS), maximum: DateTime.addDuration(ended, CLOCKS) },
});
const local = (at: DateTime.DateTime.Input) => Option.map(DateTime.makeZoned(at, { timeZone: DateTime.zoneMakeLocal(), adjustForTimeZone: true }), DateTime.toUtc);
const ipsOf = (report: string) => Option.flatMap(Option.fromNullishOr(BODY.exec(report)?.[0]), Schema.decodeOption(Ips));
const fieldsOf = (report: string) =>
    Schema.decodeUnknownOption(Diagnosed)(
        Object.fromEntries(
            Array.dedupeWith(
                Array.fromIterable(report.matchAll(FIELD)).map(({ groups }) => [groups?.key ?? '', groups?.value ?? ''] as const),
                ([left], [right]) => left === right,
            ),
        ),
    );
const launchedAs = (instance: Lifetime) => (report: string) => {
    const { launch, alive } = lifetime(instance);
    return Option.exists(
        ipsOf(report),
        ({ pid, procLaunch, date, processes = [] }) => (pid === instance.pid && Option.exists(Option.fromUndefinedOr(procLaunch), DateTime.between(launch))) || (Option.exists(Option.fromUndefinedOr(date), DateTime.between(alive)) && processes.some((held) => held.pid === instance.pid && held.reason !== undefined)),
    );
};
const diagnosed = (instance: Lifetime) => (report: string) => {
    const { launch, alive } = lifetime(instance);
    return Option.exists(
        fieldsOf(report),
        (fields) =>
            fields.PID === instance.pid &&
            fields.Path.startsWith(instance.chosen.app) &&
            UndefinedOr.match(fields['Time Since Fork'], {
                onUndefined: () => DateTime.between(fields['Date/Time'], alive),
                onDefined: ([seconds]) => DateTime.between(DateTime.subtractDuration(fields['End time'], Duration.seconds(seconds)), launch),
            }),
    );
};

// --- [TEXT]
const lined =
    (kept: (contents: string, instance: Instance) => readonly string[]): Text =>
    (contents, instance) =>
        Option.map(Option.liftPredicate(kept(contents, instance), Array.isReadonlyArrayNonEmpty), (lines) => ({ lines }));
const whole = (contents: string) => contents.replaceAll(BREAKS, '\n').split('\n').filter(String.isNonEmpty);
const framed =
    (images: NonNullable<(typeof Ips.Type)['usedImages']>) =>
    ({ imageIndex, imageOffset, symbol }: NonNullable<(typeof Ips.Type)['threads']>[number]['frames'][number]) =>
        `${images[imageIndex]?.name ?? imageIndex} ${symbol ?? `+${imageOffset}`}`;
const faulted: Diagnosis = (contents, { pid }) =>
    Option.flatMap(ipsOf(contents), ({ exception, faultingThread, threads, usedImages, processes = [] }) => {
        const named = Record.getSomes({
            exception: Option.map(Option.fromUndefinedOr(exception), ({ type, signal }) => [type, signal].filter(Predicate.isNotUndefined).join(' ')),
            reason: Option.flatMap(
                Array.findFirst(processes, (held) => held.pid === pid),
                ({ reason }) => Option.fromUndefinedOr(reason),
            ),
        });
        const frames = Option.map(Array.get(threads ?? [], faultingThread ?? 0), (thread) => thread.frames.map(framed(usedImages ?? [])));
        return Option.liftPredicate({ ...named, ...Record.getSomes({ frames }) } satisfies Facts, (held) => Struct.keys(held).length > 0);
    });
const hung: Diagnosis = (contents) => {
    const fields = fieldsOf(contents);
    const event = Option.map(fields, Struct.get('Event'));
    const duration = Option.flatMap(
        Option.filter(fields, ({ Event }) => Event === 'hang'),
        ({ Duration: held }) => Option.map(Option.fromUndefinedOr(held), ([seconds]) => seconds),
    );
    const frames = Option.map(Option.fromNullishOr(HEAVIEST.exec(contents)?.groups?.block), (block) => Array.reverse(Array.getSomes(block.split('\n').map((line) => Option.fromNullishOr(STACKED.exec(line)?.groups)))).map(({ image, symbol = '' }) => (image === undefined ? symbol : `${image} ${symbol}`)));
    return Option.liftPredicate({ ...Record.getSomes({ event }), ...Record.getSomes({ duration }), ...Record.getSomes({ frames }) } satisfies Facts, (held) => Struct.keys(held).length > 0);
};
const failing = (contents: string) => {
    const lines = whole(contents);
    const [, kept] = Array.mapAccum(lines, false, (tracing, line) => {
        const opened = TRACED.test(line);
        return [opened || (tracing && line.startsWith(' ')), opened || tracing || FAILING.test(line)] as const;
    });
    return lines.filter((_, index) => kept[index] === true);
};
const entries = (contents: string, instance: Instance) => {
    const { launch } = lifetime(instance);
    const ours = (entry: string) =>
        Option.exists(
            Option.flatMap(Option.fromNullishOr(SESSION.exec(entry)?.groups), ({ time, date }) => local(`${date} ${time}`)),
            DateTime.between({ ...launch, minimum: DateTime.startOf(instance.started, 'minute') }),
        );
    return whole(contents.replaceAll(BREAKS, '\n').split(ENTRY).filter(ours).join('').replaceAll(TABLE, ''));
};

// --- [LOCATORS]
const dated = ({ birthtime, mtime }: FileSystem.File.Info, ended: DateTime.Utc) =>
    Option.match(
        Option.orElse(birthtime, () => mtime),
        { onNone: () => ended, onSome: DateTime.fromDateUnsafe },
    );
const opened =
    (kind: typeof Kind.Type, at: DateTime.Utc, file: string) =>
    (facts: Facts): Linked => ({ kind, at, file: Option.some(file), facts });
const examined = Effect.fn('examined')(function* (file: string) {
    const fs = yield* FileSystem.FileSystem;
    const info = yield* fs.stat(file).pipe(
        Effect.asSome,
        Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeedNone),
        Effect.mapError(HarnessError.refused(file)),
    );
    return Option.map(info, (held) => ({ file, info: held }));
});
const read = Effect.fn('read')(function* <A extends { readonly file: string }>(held: A) {
    const fs = yield* FileSystem.FileSystem;
    const contents = yield* Effect.option(fs.readFileString(held.file));
    return { ...held, contents: Option.filter(contents, (text) => text !== '' && !UNREADABLE.test(text)) };
});
const factsOf = <I>(text: ((contents: string, instance: I) => Option.Option<Facts>) | undefined, contents: Option.Option<string>, instance: I) => Option.match(Option.all({ text: Option.fromUndefinedOr(text), contents }), { onNone: () => Option.some<Facts>({}), onSome: (held) => held.text(held.contents, instance) });
const diagnostics = Effect.fn('diagnostics')(function* (pattern: string, owned: (instance: Lifetime) => (report: string) => boolean, facts: Diagnosis, instance: Lifetime, kind: typeof Kind.Type) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const patterns = FOLDERS.map((folder) => path.join(folder, pattern));
    const globbed = yield* Effect.forEach(patterns, (glob) => Effect.mapError(fs.glob(glob), HarnessError.refused(glob)));
    const { minimum } = lifetime(instance).alive;
    const found = Array.getSomes(yield* Effect.forEach(globbed.flat(), examined, { concurrency: 'unbounded' }));
    const reports = yield* Effect.forEach(
        found.filter(({ info }) => DateTime.isGreaterThanOrEqualTo(dated(info, instance.ended), minimum)),
        read,
        { concurrency: 'unbounded' },
    );
    const ours = reports.filter(({ contents }) => Option.exists(contents, owned(instance)));
    return Array.getSomes(ours.map(({ file, info, contents }) => Option.map(factsOf(facts, contents, instance), opened(kind, dated(info, instance.ended), file))));
});
const spun = Effect.fn('spun')(function* (instance: Lifetime, kind: typeof Kind.Type) {
    const window = { start: `${SINCE.format(DateTime.toDateUtc(instance.started))}+0000`, end: `${SINCE.format(DateTime.toDateUtc(DateTime.addDuration(instance.ended, CLOCKS)))}+0000` };
    const shown = yield* printedLines('log', ['show', '--style', 'compact', '--predicate', 'subsystem == "com.apple.spindump" OR subsystem == "com.apple.libquit"', '--start', window.start, '--end', window.end]);
    const kept = shown.filter((line) => line.includes(`[${instance.pid}]`) && !line.includes('not sampling'));
    return Array.match(kept, { onEmpty: () => [], onNonEmpty: (lines): readonly Linked[] => [{ kind, at: instance.ended, file: Option.none(), facts: { lines } }] });
});
const crashed = Effect.fn('crashed')(function* (pid: number, exit: typeof State.cases.Exited.Type) {
    const report = Option.map(Option.fromUndefinedOr(exit.report), ({ url }) => fileURLToPath(url));
    const held = yield* Effect.transposeOption(Option.map(report, (file) => read({ file })));
    const facts = Option.flatMap(Option.flatMap(held, Struct.get('contents')), (contents) => faulted(contents, { pid }));
    return {
        _tag: 'Crashed',
        ...Struct.pick(exit, ['status', 'signal']),
        ...Record.getSomes({ report, exception: Option.flatMap(facts, ({ exception }) => Option.fromUndefinedOr(exception)) }),
        ...Record.getSomes({ frames: Option.flatMap(facts, ({ frames }) => Option.fromUndefinedOr(frames)) }),
    } satisfies typeof Halted.cases.Crashed.Type;
});

// --- [TABLES] --------------------------------------------------------------------------

const KINDS: { readonly [K in typeof Kind.Type]: { readonly level: LogLevel.Severity; readonly system?: (instance: Lifetime, kind: typeof Kind.Type) => Effect.Effect<readonly Linked[], Error, FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner>; readonly text?: Text } } = {
    crash: { level: 'Warn', system: (instance, kind) => diagnostics('*.ips', launchedAs, faulted, instance, kind), text: faulted },
    hang: { level: 'Warn', system: (instance, kind) => diagnostics('*.{hang,diag}', diagnosed, hung, instance, kind), text: hung },
    spin: { level: 'Warn', system: spun },
    shutdown: { level: 'Warn', text: lined(entries) },
    recovery: { level: 'Warn' },
    host: { level: 'Info' },
};
const LOCATORS = Array.getSomes(Record.collect(KINDS, (kind, row) => Option.map(Option.fromUndefinedOr(row.system), (locate) => [kind, locate] as const)));

// --- [LOCATING] ------------------------------------------------------------------------

const system = (instance: Lifetime) =>
    Effect.map(
        Effect.forEach(LOCATORS, ([kind, locate]) => locate(instance, kind), { concurrency: 'unbounded' }),
        Array.flatten,
    );
const linking = (instance: Instance, name: string, { birthtime, mtime, size }: FileSystem.File.Info) => {
    const { launch, alive } = lifetime(instance);
    const born = Option.map(birthtime, DateTime.fromDateUnsafe);
    const changed = Option.map(mtime, DateTime.fromDateUnsafe);
    return (
        ByteSize.toNumberUnsafe(size) > 0 &&
        Option.match(Option.flatMap(Schema.decodeUnknownOption(Stamp)(STAMP.exec(name)?.groups), local), {
            onSome: DateTime.between(launch),
            onNone: () => Option.exists(born, DateTime.between(launch)) || Option.exists(changed, DateTime.between(alive)),
        })
    );
};
const ruled = Effect.fn('ruled')(function* (instance: Instance, { kind, pattern, root, text = KINDS[kind].text }: Rule) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const files = yield* Effect.mapError(fs.glob(pattern, { root }), HarnessError.refused(path.join(root, pattern)));
    const found = Array.getSomes(
        yield* Effect.forEach(
            files.map((file) => path.join(root, file)),
            examined,
            { concurrency: 'unbounded' },
        ),
    );
    const kept = yield* Effect.forEach(
        found.filter(({ file, info }) => linking(instance, path.basename(file), info)),
        read,
        { concurrency: 'unbounded' },
    );
    return Array.getSomes(kept.map(({ file, info, contents }) => Option.map(factsOf(text, contents, instance), opened(kind, dated(info, instance.ended), file))));
});
const located = Effect.fn('located')(function* (host: Pick<Harness, 'logs'>, instance: Instance) {
    const rules = yield* host.logs?.(instance) ?? Effect.succeed([]);
    const [found, matched] = yield* Effect.all([system(instance), Effect.forEach(rules, (rule) => ruled(instance, rule), { concurrency: 'unbounded' })], { concurrency: 'unbounded' });
    return [...found, ...matched.flat()];
});
const linked = (host: Pick<Harness, 'logs'>, instance: Instance) =>
    located(host, instance).pipe(
        Effect.flatMap(Effect.forEach(({ kind, file, facts }) => record(KINDS[kind].level, 'report linked', { kind, ...Record.getSomes({ file }), ...facts }), { discard: true })),
        Effect.catch((error) => record('Error', 'failed', { tag: tagOf(error), error: messageOf(error) })),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { crashed, failing, HOME, KINDS, type Linked, lifetime, lined, linked, local, type Rule, system, whole };
