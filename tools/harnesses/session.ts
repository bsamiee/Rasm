import { createHash } from 'node:crypto';
import { fileURLToPath } from 'node:url';
import { type Installation, State } from '@rasm/automation/applications';
import type { Automation } from '@rasm/automation/automation';
import { Array, ByteSize, Data, type DateTime, Deferred, Duration, Effect, Equal, FileSystem, HashMap, identity, Match, Number, Option, Order, Path, type PlatformError, Predicate, Record, Ref, Result, Schema, Stream, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import type { Row as Authored } from '#/cases';
import { HarnessError } from '#/error';
import { busy, type Launched, Progress, Row, type Spawned, stalled, starts, watched } from '#/instance';
import { LAUNCHED, phase, Run, STATES, sample, stateOf, tagOf } from '#/log';
import { crashed, linked, type Rule } from '#/report';
import { Halted, type hostCase, Outcome, Records } from '#/results';
import type { Mapped } from '#/source';
import type { JsonObject } from '#/values';
import npm from './package.json' with { type: 'json' };

// --- [TYPES] ---------------------------------------------------------------------------

type Any = Schema.Codec<unknown, unknown>;
type Decoding<P> = Schema.Codec<P, unknown, never, unknown>;
type Services = Automation | ChildProcessSpawner.ChildProcessSpawner | FileSystem.FileSystem | Path.Path | Run;
type Instance = Pick<Live, 'pid' | 'started' | 'chosen'> & { readonly ended: DateTime.Utc };
type Entry = ReturnType<typeof Records.line<typeof Outcome>>['Type'];
type Verdict = Data.TaggedEnum<{
    Halted: { readonly index: Option.Option<number>; readonly halted: typeof Halted.Type };
    Lost: { readonly index: Option.Option<number>; readonly error: Error };
}>;
type Sighting = typeof JsonObject.Type & { readonly state: 'Dialog' | 'Responding' };
interface Surveyed {
    readonly host: Schema.Json;
}
type Cased = ReturnType<typeof hostCase<{ readonly folder: typeof Schema.String }>>['Type'];
interface Fields {
    readonly [key: PropertyKey]: Schema.Top & Schema.Decoder<unknown>;
}
interface Installed {
    readonly identifier: string;
    readonly name: string;
    readonly app: string;
    readonly version: string;
    readonly watched: Array.NonEmptyReadonlyArray<string>;
}
interface Header<S> {
    readonly application: string;
    readonly version: string;
    readonly state?: Schema.Json;
    readonly survey?: S;
}
interface Batch<S, C> {
    readonly records: string;
    readonly label: string;
    readonly survey: Option.Option<S>;
    readonly opened: Option.Option<Schema.Json>;
    readonly cases: readonly C[];
}
interface Note {
    readonly index: Option.Option<number>;
    readonly element: string;
    readonly answered: (typeof Outcome.fields.answered.Type)[number];
}
interface Seen {
    readonly state: typeof State.Type;
    readonly windows: ReadonlyMap<string, Sighting>;
}
interface Trace {
    readonly next: number;
    readonly running: Option.Option<number>;
    readonly last: Option.Option<number>;
    readonly finished: HashMap.HashMap<number, typeof Outcome.Type>;
}
interface Live {
    readonly pid: number;
    readonly started: DateTime.Utc;
    readonly launched: boolean;
    readonly chosen: Installed;
    readonly scratch: string;
    readonly passing: readonly Progress[];
    readonly verdict: Deferred.Deferred<Verdict>;
    readonly notes: Ref.Ref<readonly Note[]>;
    readonly trace: Ref.Ref<Trace>;
    readonly batch: Ref.Ref<readonly Prepared<Cased>[]>;
    readonly state: SubscriptionRef.SubscriptionRef<typeof State.Type>;
}
interface Prepared<C extends Cased> {
    readonly hosted: C;
    readonly fn: Option.Option<string>;
    readonly map: readonly Mapped[];
    readonly presses: readonly string[];
    readonly copies: readonly string[];
    readonly windowed: boolean;
}
interface Plan<C extends Cased, E> {
    readonly engine: E;
    readonly sendable: readonly Prepared<C>[];
    readonly settled: readonly (typeof Outcome.Type)[];
}
interface Rebuilt<C> {
    readonly hosted: C;
    readonly windowed: boolean;
    readonly fn: string;
    readonly args: readonly Schema.Json[];
    readonly module: string;
}
interface Ran<S> {
    readonly reply: Result.Result<Header<S>, Error>;
    readonly trace: Trace;
    readonly notes: readonly Note[];
    readonly verdict: Option.Option<Verdict>;
}
interface Finished<S> {
    readonly header: Option.Option<Header<S>>;
    readonly shared: readonly number[];
    readonly failed: Option.Option<string>;
    readonly answered: typeof Outcome.fields.answered.Type;
    readonly outcomes: readonly (typeof Outcome.Type)[];
}
interface Done<S, C extends Cased, E, P extends Plan<C, E>> extends Finished<S> {
    readonly survey: S;
    readonly opened: Option.Option<Schema.Json>;
    readonly plan: P;
}
interface Host<S extends Surveyed = Surveyed, C extends Cased = Cased, E = unknown, P = unknown, F extends Fields = Fields> {
    readonly Engine: Schema.Codec<E, unknown>;
    readonly survey: { readonly question: string; readonly Schema: Schema.Codec<S, unknown> };
    readonly Case: Schema.Codec<C, unknown>;
    readonly fields: F;
    readonly progress?: readonly Progress[];
    readonly installed: Effect.Effect<readonly Installed[], Error, Services>;
    readonly engine: (scripts: readonly string[]) => Result.Result<E, HarnessError>;
    readonly precondition: (survey: S) => Decoding<P>;
    readonly placed: (decoded: P, folder: string, product: string) => Effect.Effect<{ readonly precondition: unknown; readonly copies: readonly string[] }, PlatformError.PlatformError, FileSystem.FileSystem | Path.Path>;
    readonly refusal?: (engine: E, hosted: C, binary: boolean) => Option.Option<string>;
    readonly windowed?: (precondition: typeof JsonObject.Type, row: Authored) => boolean;
    readonly launch: (chosen: Installed, scratch: string, windowed: boolean) => Effect.Effect<Launched | Spawned, Error, Services>;
    readonly opened?: (live: Live) => Effect.Effect<Schema.Json, Error, Services>;
    readonly send: (live: Live, batch: Batch<S, C>) => Effect.Effect<Header<S>, Error, Services>;
    readonly quit: (live: Live) => Effect.Effect<void, Error, Services>;
    readonly canonical?: (file: string, bytes: Uint8Array) => Effect.Effect<readonly (string | Uint8Array)[], Error, Services>;
    readonly rebuilt?: (call: Rebuilt<C>, folder: string) => Effect.Effect<{ readonly hosted: C; readonly map: readonly Mapped[] }, Error, Services>;
    readonly logs?: (instance: Instance) => Effect.Effect<readonly Rule[], Error, Services>;
}
type Session<S extends Surveyed, C extends Cased> = Pick<Host<S, C>, 'progress' | 'fields' | 'launch' | 'opened' | 'send' | 'quit' | 'canonical' | 'logs'>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const VERSIONS = new Intl.Collator('en', { numeric: true });
const LABEL = npm.name.replaceAll(/\W+/gu, '-').replace(/^-/u, '');
const HALTED = { Crashed: 'Exited', Hung: 'Unresponsive', Dialog: 'Dialog' } as const satisfies { readonly [K in (typeof Halted.Type)['_tag']]: keyof typeof STATES };
const UNLOGGED = new Set(['name', 'completion', 'before', 'opened', 'written', 'halted']);

// --- [MODELS] --------------------------------------------------------------------------

const Verdict = Data.taggedEnum<Verdict>();
const same = Schema.toEquivalence(Schema.Json);
const newest = Order.flip(
    Order.mapInput(
        Order.make((left: string, right: string) => Number.sign(VERSIONS.compare(left, right))),
        ({ version }: Installed) => version,
    ),
);
const idle: Trace = { next: 0, running: Option.none(), last: Option.none(), finished: HashMap.empty() };

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INSTALLATIONS]
const inside = (outer: Installation, inner: Installation) => inner.url.href !== outer.url.href && inner.url.href.startsWith(outer.url.href);
const installation = Effect.fn('installation')(function* (row: Installation, rows: readonly Installation[], extra: readonly string[]) {
    const path = yield* Path.Path;
    const app = yield* path.fromFileUrl(row.url);
    const version = yield* Effect.fromOption(row.version, () => HarnessError.unavailable(row.name)('Launch Services reports no version'));
    const companions = Array.dedupe([...rows.filter((inner) => inside(row, inner)).map(({ identifier }) => identifier), ...extra]).filter((identifier) => identifier !== row.identifier);
    return { identifier: row.identifier, name: row.name, app, version, watched: [row.identifier, ...companions] } satisfies Installed;
});
const installations = (rows: readonly Installed[], prefixes: readonly string[], subject: string) => {
    const hosts = Array.sort(rows, newest);
    const chosen = prefixes.length === 0 ? hosts : hosts.filter(({ version }) => prefixes.some((prefix) => version.startsWith(prefix)));
    return Effect.fromOption(Option.liftPredicate(chosen, Array.isArrayNonEmpty), () => HarnessError.unavailable(`${subject} ${prefixes.join(', ')}`)(`installed versions ${hosts.map(({ version }) => version).join(', ')}`));
};

// --- [VERDICTS]
const caseOf = (batch: readonly Prepared<Cased>[], index: Option.Option<number>) =>
    Option.map(
        Option.flatMap(index, (at) => Array.get(batch, at)),
        ({ hosted }) => hosted.name,
    );
const named = (batch: readonly Prepared<Cased>[], index: Option.Option<number>) => Record.getSomes({ case: caseOf(batch, index) });
const flat = (halted: Option.Option<typeof Halted.Type>) => Option.match(halted, { onNone: () => ({}), onSome: (held) => ({ ...Struct.omit(held, ['_tag']), state: HALTED[held._tag] }) });
const verdictTag = Verdict.$match({ Halted: ({ halted }) => halted._tag, Lost: ({ error }) => tagOf(error) });
const verdictText = (limit: Duration.Duration, pid: number, batch: readonly Prepared<Cased>[]) => (verdict: Verdict) =>
    [
        ...Option.toArray(Option.map(caseOf(batch, verdict.index), (name) => `case ${name}`)),
        Verdict.$match(verdict, {
            Halted: ({ halted }) =>
                Halted.match(halted, {
                    Crashed: ({ status, signal, report }) => `instance ${pid} exited with ${signal === undefined ? `status ${status ?? 'unread'}` : `signal ${signal}`}${report === undefined ? '' : `, report ${report}`}`,
                    Hung: ({ cpu, sample: file }) => `instance ${pid} hung ${cpu} past ${Duration.format(limit)}, sample ${file}, left running`,
                    Dialog: (dialog) => Row.shown(dialog, pid),
                }),
            Lost: ({ error }) => error.message,
        }),
    ].join(': ');
const ended = (live: Live, verdict: (trace: Trace) => Verdict) => Effect.flatMap(Ref.get(live.trace), (trace) => Effect.asVoid(Deferred.succeed(live.verdict, verdict(trace))));
const lost = (live: Live) => (error: Error) => ended(live, ({ running }) => Verdict.Lost({ index: running, error }));

// --- [RECORDS]
const filesIn = Effect.fn('filesIn')(function* (folder: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const isFile = (file: string) => Effect.map(fs.stat(file), ({ type }) => type === 'File');
    return yield* Effect.filter(
        (yield* fs.readDirectory(folder, { recursive: true })).map((file) => path.join(folder, file)),
        isFile,
    );
});
const entriesOf = (Line: Schema.Codec<Entry, string, never, unknown>) =>
    Effect.fn('entriesOf')(function* (file: string) {
        const lines = (yield* (yield* FileSystem.FileSystem).readFileString(file)).split('\n').slice(0, -1);
        return yield* Effect.forEach(
            lines.filter((line) => line.trim() !== ''),
            (line) => Schema.decodeEffect(Line)(line),
        );
    });
const mapped =
    (map: readonly Mapped[]) =>
    (row: typeof Outcome.Type): typeof Outcome.Type => {
        const at = (frame: { readonly line: number; readonly file?: string }) => (frame.file === undefined ? Option.getOrElse(Array.get(map, frame.line - 1), () => ({ line: frame.line })) : frame);
        return row.completion._tag === 'Thrown'
            ? { ...row, completion: { ...row.completion, ...(row.completion.line === undefined ? {} : at({ line: row.completion.line, ...(row.completion.file === undefined ? {} : { file: row.completion.file }) })), ...(row.completion.stack === undefined ? {} : { stack: row.completion.stack.map(at) }) } }
            : row;
    };
const reported = (row: typeof Outcome.Type, tag: string = row.halted?._tag ?? row.completion._tag) => {
    const fields: Record.ReadonlyRecord<string, unknown> = row;
    const passed = row.halted === undefined && (Outcome.fields.completion.guards.Ran(row.completion) || Outcome.fields.completion.guards.Exited(row.completion));
    const facts = {
        case: row.name,
        completion: row.completion._tag,
        ...Outcome.fields.completion.match(row.completion, {
            Ran: () => ({}),
            Exited: () => ({}),
            Thrown: ({ message, file, line, number, stack }) => ({ error: message, ...Record.filter({ file, line, number }, Predicate.isNotUndefined), ...(stack === undefined ? {} : { stack: stack.map((frame) => [frame.file, frame.line].filter(Predicate.isNotUndefined).join(':')) }) }),
            Failed: ({ reason }) => ({ error: reason }),
        }),
        ...Record.filter(fields, (held, key) => !UNLOGGED.has(key) && held !== undefined && (key === 'value' || !(Predicate.isObject(held) && Object.keys(held).length === 0))),
        ...flat(Option.fromUndefinedOr(row.halted)),
    };
    return passed ? Effect.annotateLogs(Effect.logInfo(`case ${row.completion._tag}`), facts) : Effect.annotateLogs(Effect.logError(`case ${row.completion._tag}`), { ...facts, tag });
};
const notRun = Effect.fnUntraced(function* (unsent: readonly Prepared<Cased>[], reason: string) {
    const outcomes = yield* Effect.forEach(unsent, ({ hosted }) => Effect.orDie(Schema.decodeEffect(Outcome)({ name: hosted.name, completion: { _tag: 'Failed', reason } })));
    yield* Array.match(outcomes, { onEmpty: () => Effect.void, onNonEmpty: (held) => Effect.annotateLogs(Effect.logWarning('cases not run'), { cases: held.map(({ name }) => name), error: reason }) });
    return outcomes;
});
const called = ({ hosted, fn }: Prepared<Cased>) => ({
    _tag: hosted._tag,
    ...Match.valueTags(hosted, {
        Probe: ({ evaluate }) => ({ evaluate }),
        Call: ({ call, args, source }) => ({ call, args, ...UndefinedOr.match(source, { onUndefined: () => ({}), onDefined: ({ script }) => ({ script }) }) }),
        Run: ({ source, args }) => ({ script: source.script, args }),
    }),
    ...Record.getSomes({ fn }),
});
const observe = Effect.fnUntraced(function* (live: Live, rows: readonly Entry[]) {
    const batch = yield* Ref.get(live.batch);
    const step = (held: Trace, entry: Entry): readonly [Trace, Effect.Effect<void>] =>
        Match.valueTags(entry, {
            Started: ({ index }) => [{ ...held, running: Option.some(index), last: Option.some(index) }, Option.match(Array.get(batch, index), { onNone: () => Effect.void, onSome: (item) => Effect.annotateLogs(Effect.logInfo('case started'), { case: item.hosted.name, call: called(item) }) })] as const,
            Finished: ({ outcome }) => {
                const row = Option.match(
                    Option.flatMap(held.running, (at) => Array.get(batch, at)),
                    { onNone: () => outcome, onSome: ({ map }) => mapped(map)(outcome) },
                );
                return [{ ...held, running: Option.none(), finished: Option.match(held.running, { onNone: () => held.finished, onSome: (at) => HashMap.set(held.finished, at, row) }) }, reported(row)] as const;
            },
        });
    const logs = yield* Ref.modify(live.trace, (trace) => {
        const [next, steps] = Array.mapAccum(rows.slice(trace.next), trace, step);
        return [steps, { ...next, next: Math.max(trace.next, rows.length) }] as const;
    });
    yield* Effect.all(logs, { discard: true });
});
const writtenIn = Effect.fnUntraced(function* (host: Pick<Host, 'canonical'>, sendable: readonly Prepared<Cased>[]) {
    const fs = yield* FileSystem.FileSystem;
    const digest = Effect.fnUntraced(function* (file: string) {
        const bytes = yield* fs.readFile(file);
        const parts: readonly (string | Uint8Array)[] = host.canonical === undefined ? [bytes] : yield* host.canonical(file, bytes);
        return { file, size: bytes.byteLength, sha256: parts.reduce((hash, part) => hash.update(part), createHash('sha256')).digest('hex') } satisfies (typeof Outcome.fields.written.Type)[number];
    });
    const copied = new Set((yield* Effect.forEach(Array.dedupe(sendable.flatMap(({ copies }) => copies)), digest)).map(({ sha256 }) => sha256));
    const listed = ({ hosted }: Prepared<Cased>) =>
        Effect.map(
            Effect.flatMap(filesIn(hosted.folder), (files) => Effect.forEach(files, digest)),
            (written) => [hosted.name, written.filter(({ sha256 }) => !copied.has(sha256))] as const,
        );
    return new Map(yield* Effect.forEach(sendable, listed));
});

// --- [WATCHER]
const processOf = (state: typeof State.Type) =>
    State.match(state, {
        Launching: () => ({}),
        Responding: () => ({}),
        Unresponsive: ({ process, usage, windows }) => ({
            process,
            ...UndefinedOr.match(usage, { onUndefined: () => ({}), onDefined: ({ cpu, footprint, threads }) => ({ cpu: Duration.toSeconds(cpu), footprint: ByteSize.toNumberUnsafe(footprint), threads }) }),
            windows: windows.map(Struct.pick(['number', 'title'])),
        }),
        Failed: (failed) => Struct.omit(failed, ['_tag', 'next']),
        Exited: ({ report, ...exit }) => ({ ...Struct.omit(exit, ['_tag', 'next']), ...UndefinedOr.match(report, { onUndefined: () => ({}), onDefined: ({ url, exception }) => ({ report: fileURLToPath(url), ...Record.filter({ exception }, Predicate.isNotUndefined) }) }) }),
    });
const keyOf = (element: (typeof State.cases.Responding.Type)['elements'][number]) => `${element.window ?? element.id}`;
const windowsOf = (state: typeof State.Type, held: ReadonlyMap<string, Sighting>, passing: readonly Progress[], ours: boolean): ReadonlyMap<string, Sighting> =>
    State.guards.Responding(state)
        ? new Map<string, Sighting>([
              ...state.elements
                  .filter((element) => ours || element.requiresAnswer)
                  .map(
                      (element) =>
                          [
                              keyOf(element),
                              {
                                  ...(element.window === undefined ? { id: element.id } : { window: element.window }),
                                  ...Struct.pick(element, ['role', 'subrole', 'title', 'edited', 'modal', 'identifier', 'requiresAnswer']),
                                  ...UndefinedOr.match(element.document, { onUndefined: () => ({}), onDefined: (url) => ({ document: fileURLToPath(url) }) }),
                                  ...UndefinedOr.match(element.detail, { onUndefined: () => ({}), onDefined: ({ text, controls }) => ({ text, controls: controls.map(Struct.pick(['label', 'role', 'value', 'enabled', 'attribute'])) }) }),
                                  state: Progress.requiring(passing)(element) ? 'Dialog' : 'Responding',
                              },
                          ] as const,
                  ),
              ...state.windows.filter(({ title }) => ours && title !== undefined).map((window) => [`${window.number}`, { ...Struct.pick(window, ['number', 'title']), state: 'Responding' }] as const),
          ])
        : held;
const sighted = (row: typeof Row.Type, before: Option.Option<Seen>, windows: ReadonlyMap<string, Sighting>, ours: boolean) => {
    const prior = Option.match(before, { onNone: () => new Map<string, Sighting>(), onSome: (held) => held.windows });
    const shown = (state: typeof State.Type): Schema.Json =>
        State.match(state, { Launching: () => ({}), Responding: () => ({}), Unresponsive: (held) => ({ process: held.process, titles: held.windows.map(({ title }) => title ?? null) }), Failed: ({ reason }) => ({ reason }), Exited: (exit) => ({ ...processOf(exit) }) });
    const instance = (ours || State.guards.Exited(row.state)) && !Option.exists(before, (held) => held.state._tag === row.state._tag && same(shown(held.state), shown(row.state))) ? Effect.annotateLogs(Effect.logWithLevel(STATES[row.state._tag])(`instance ${row.state._tag}`), processOf(row.state)) : Effect.void;
    const opened = Array.fromIterable(windows).filter(([key]) => !prior.has(key));
    const changed = Array.fromIterable(windows).filter(([key, facts]) => Option.exists(Option.fromUndefinedOr(prior.get(key)), (held) => !same(held, facts)));
    const closed = Array.fromIterable(prior).filter(([key]) => !windows.has(key));
    return Effect.all(
        [
            instance,
            Effect.forEach(opened, ([, facts]) => Effect.annotateLogs(Effect.logWithLevel(STATES[facts.state])('window opened'), facts), { discard: true }),
            Effect.forEach(changed, ([, facts]) => Effect.annotateLogs(Effect.logWithLevel(STATES[facts.state])('window changed'), facts), { discard: true }),
            Effect.forEach(closed, ([, facts]) => Effect.annotateLogs(Effect.logInfo('window closed'), facts), { discard: true }),
        ],
        { discard: true },
    );
};
const holding =
    (passing: readonly Progress[]) =>
    ({ pid, state }: typeof Row.Type): Option.Option<string> =>
        State.match(state, {
            Launching: () => Option.none(),
            Responding: ({ elements }) => Option.map(Array.findFirst(elements, Progress.requiring(passing)), (element) => Row.shown(Row.dialog(element), pid)),
            Unresponsive: () => Option.some(`instance ${pid} ${Row.described(state)}, left running`),
            Failed: () => Option.none(),
            Exited: () => Option.none(),
        });
const within = (live: Live) => Effect.provideService(Run.Instance, Option.some(live));
const owned = (live: Live, row: typeof Row.Type) => row.identifier === live.chosen.identifier && row.pid === live.pid;
const watcher = (host: Pick<Host, 'logs'>, live: Live, seen: Ref.Ref<HashMap.HashMap<number, Seen>>) =>
    Effect.fn('watcher')(
        function* (row: typeof Row.Type) {
            const ours = owned(live, row);
            const [before, windows] = yield* Ref.modify(seen, (held) => {
                const prior = HashMap.get(held, row.pid);
                const next = windowsOf(row.state, Option.match(prior, { onNone: () => new Map(), onSome: ({ windows: kept }) => kept }), live.passing, ours);
                return [[prior, next] as const, HashMap.set(held, row.pid, { state: row.state, windows: next })] as const;
            });
            yield* ours ? SubscriptionRef.set(live.state, row.state) : Effect.void;
            yield* sighted(row, before, windows, ours);
            const answer = Effect.fnUntraced(function* (element: (typeof State.cases.Responding.Type)['elements'][number]) {
                const [{ running }, batch, notes] = yield* Effect.all([Ref.get(live.trace), Ref.get(live.batch), Ref.get(live.notes)]);
                const used = notes.filter(({ index }) => Equal.equals(index, running)).length;
                const wanted = running.pipe(
                    Option.filter(() => ours),
                    Option.flatMap((index) => Array.get(batch, index)),
                    Option.flatMap(({ presses }) => Array.get(presses, used)),
                );
                const control = Option.zipWith(
                    wanted,
                    Option.flatMap(wanted, (label) => Row.pressed(element, label)),
                    (label, { id }) => ({ label, id }),
                );
                yield* Option.match(control, {
                    onNone: () => ended(live, (trace) => Verdict.Halted({ index: trace.running, halted: Row.dialog(element) })),
                    onSome: ({ label, id }) => {
                        const answered = { ...(element.title === undefined ? {} : { title: element.title }), text: element.detail?.text ?? [], pressed: label };
                        return Effect.catch(Effect.andThen(Effect.all([Ref.update(live.notes, Array.append({ index: running, element: element.id, answered })), Effect.annotateLogs(Effect.logInfo('dialog answered'), { dialog: answered, ...named(batch, running) })]), Row.perform(id)), lost(live));
                    },
                });
            });
            const noted = yield* Ref.get(live.notes);
            yield* State.match(row.state, {
                Launching: () => Effect.void,
                Responding: ({ elements }) =>
                    Effect.forEach(
                        elements.filter((element) => Progress.requiring(live.passing)(element) && noted.every((note) => note.element !== element.id)),
                        answer,
                        { discard: true },
                    ),
                Unresponsive: () => Effect.void,
                Failed: ({ reason }) => (ours ? lost(live)(HarnessError.unavailable(`process ${row.pid}`)(reason)) : Effect.void),
                Exited: (exit) =>
                    ours && Row.settled(exit)
                        ? Effect.andThen(
                              Effect.flatMap(crashed(exit), (halted) => ended(live, ({ last }) => Verdict.Halted({ index: last, halted }))),
                              linked(host, live),
                          )
                        : Effect.void,
            });
        },
        (effect, row) =>
            owned(live, row) ? Effect.flatMap(Effect.all([Ref.get(live.trace), Ref.get(live.batch)]), ([{ running }, batch]) => within(live)(Effect.annotateLogs(effect, named(batch, running)))) : Effect.annotateLogs(effect, { identifier: row.identifier, pid: row.pid, state: stateOf(row.state, live.passing) }),
    );
const progress = Effect.fnUntraced(function* (live: Live, records: string, Recorded: Schema.Codec<Entry, string, never, unknown>, limit: Duration.Duration) {
    yield* watched(records).pipe(
        Stream.mapEffect(() => Effect.flatMap(entriesOf(Recorded)(records), (rows) => observe(live, rows))),
        Stream.debounce(limit),
        Stream.runHead,
    );
    const [cpu, file] = yield* Effect.all([busy(live.pid), sample(live.pid)]);
    const frames = yield* stalled(file);
    yield* ended(live, ({ running }) => Verdict.Halted({ index: running, halted: { _tag: 'Hung', cpu, sample: file, frames } }));
});

// --- [SESSION]
const acquired = Effect.fn('acquired')(function* (host: Pick<Host, 'progress' | 'launch' | 'logs'>, chosen: Installed, root: string, limit: Duration.Duration, windowed: boolean) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const refused = HarnessError.unavailable(chosen.name);
    const passing = host.progress ?? [];
    const dialog = (row: typeof Row.Type) => (State.guards.Unresponsive(row.state) ? Option.none<string>() : holding(passing)(row));
    const { running, scratch, rows, instance } = yield* Effect.gen(function* () {
        const before = yield* Row.read([path.resolve(chosen.app)]);
        yield* Array.match(Array.getSomes(before.map(dialog)), { onEmpty: () => Effect.void, onNonEmpty: (held) => refused(held.join('; ')) });
        const folder = yield* fs.makeTempDirectoryScoped({ directory: root, prefix: 'instance-' });
        const spec = yield* host.launch(chosen, folder, windowed);
        const { rows: watching, ...opened } = yield* spec.start(chosen);
        yield* Option.match(dialog(opened), { onNone: () => Effect.void, onSome: refused });
        yield* Row.settled(opened.state) ? refused(`instance ${opened.pid} ${Row.described(opened.state)}`) : Effect.void;
        return { running: before, scratch: folder, rows: watching, instance: opened };
    }).pipe(phase('launch'));
    const shared = running.filter(({ pid, state }) => pid !== instance.pid && !State.guards.Exited(state)).map(({ pid }) => pid);
    const launched = running.every(({ pid }) => pid !== instance.pid);
    const started = yield* Effect.flatMap(starts(['-p', `${instance.pid}`]), (held) => Effect.fromOption(Option.fromUndefinedOr(held.get(instance.pid)), () => HarnessError.unavailable(`process ${instance.pid}`)('ps reports no start time')));
    const live: Live = {
        pid: instance.pid,
        started,
        launched,
        chosen,
        scratch,
        passing,
        verdict: yield* Deferred.make<Verdict>(),
        notes: yield* Ref.make<readonly Note[]>([]),
        trace: yield* Ref.make(idle),
        batch: yield* Ref.make<readonly Prepared<Cased>[]>([]),
        state: yield* SubscriptionRef.make<typeof State.Type>(instance.state),
    };
    yield* within(live)(Effect.annotateLogs(Effect.logInfo(launched ? LAUNCHED : 'instance attached'), { shared, app: chosen.app, ...processOf(instance.state) }));
    const seeded: Seen = { state: instance.state, windows: new Map() };
    const seen = yield* Ref.make(HashMap.make([instance.pid, seeded]));
    yield* Effect.forkScoped(Stream.runForEach(Stream.concat(Stream.make(instance), rows), watcher(host, live, seen)).pipe(Effect.andThen(refused(`instance ${instance.pid} watch ended`)), Effect.catch(lost(live))));
    yield* Effect.raceFirst(
        SubscriptionRef.changes(live.state).pipe(
            Stream.filter((state) => State.guards.Responding(state) && !state.answering),
            Stream.runHead,
        ),
        Effect.flatMap(Deferred.await(live.verdict), (over) => refused(verdictText(limit, instance.pid, [])(over))),
    ).pipe(Effect.timeoutOrElse({ duration: limit, orElse: () => Effect.flatMap(Effect.all([sample(live.pid), SubscriptionRef.get(live.state)]), ([file, state]) => refused(`instance ${instance.pid} ${Row.described(state)} past ${Duration.format(limit)}, sample ${file}, left running`)) }), phase('ready'), within(live));
    return { live, shared };
});
const quitted = Effect.fn('quitted')(
    function* (host: Pick<Host, 'quit' | 'logs'>, live: Live, limit: Duration.Duration) {
        const refused = yield* Ref.make(Option.none<Error>());
        yield* Effect.logInfo('quit sent');
        yield* Effect.forkScoped(Effect.catch(host.quit(live), (error) => Ref.set(refused, Option.some(error))));
        const [verdict, batch] = yield* Effect.all([Effect.timeoutOption(Deferred.await(live.verdict), limit), Ref.get(live.batch)]);
        const quit = Option.match(yield* Ref.get(refused), { onNone: () => 'quit sent', onSome: (error) => `quit refused: ${error.message}` });
        const exited = Option.flatMap(verdict, (over) => (Verdict.$is('Halted')(over) && over.halted._tag === 'Crashed' ? Option.some(over.halted) : Option.none()));
        const clean = Option.exists(exited, ({ signal, report, status }) => signal === undefined && report === undefined && (status ?? 0) === 0);
        const left = Option.match(verdict, {
            onNone: () => Option.some(`${quit}, instance ${live.pid} running past ${Duration.format(limit)}, left running`),
            onSome: (over) => Option.liftPredicate(`${quit}, ${verdictText(limit, live.pid, batch)(over)}`, () => !clean),
        });
        yield* Option.match(left, {
            onNone: () => Effect.void,
            onSome: (error) =>
                Effect.annotateLogs(Effect.logWarning('quit unclean'), {
                    tag: Option.match(verdict, { onNone: () => 'Running', onSome: verdictTag }),
                    error,
                    ...named(
                        batch,
                        Option.flatMap(verdict, ({ index }) => index),
                    ),
                }),
        });
        yield* Option.isSome(exited) ? Effect.void : linked(host, live);
        return left;
    },
    Effect.scoped,
    phase('quit'),
);
const abandoned = (host: Pick<Host, 'quit' | 'logs'>, live: Live, limit: Duration.Duration) => Effect.flatMap(Deferred.isDone(live.verdict), (done) => (live.launched && !done ? Effect.asVoid(within(live)(quitted(host, live, limit))) : Effect.void));
const openedIn = (host: Pick<Host, 'opened' | 'quit' | 'logs'>, live: Live, limit: Duration.Duration) =>
    host.opened === undefined
        ? Effect.succeedNone
        : Effect.asSome(host.opened(live)).pipe(
              Effect.timeoutOrElse({
                  duration: limit,
                  orElse: () => Effect.flatMap(sample(live.pid), (file) => HarnessError.unavailable(live.chosen.name)(`instance ${live.pid} opened nothing past ${Duration.format(limit)}, sample ${file}`)),
              }),
              phase('open'),
              within(live),
              Effect.onError(() => abandoned(host, live, limit)),
          );
const batchIn = Effect.fn('batchIn')(function* <S extends Surveyed, C extends Cased>(host: Pick<Host<S, C>, 'fields' | 'send'>, live: Live, batch: (records: string) => Batch<S, C>, sendable: readonly Prepared<C>[], limit: Duration.Duration) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const Recorded: Schema.Codec<Entry, string, never, unknown> = Records.line(Schema.Struct({ ...Outcome.fields, ...host.fields }));
    const records = path.join(yield* fs.makeTempDirectoryScoped({ directory: live.scratch, prefix: 'batch-' }), 'records.jsonl');
    yield* fs.writeFileString(records, '');
    yield* Effect.all([Ref.set(live.batch, sendable), Ref.set(live.notes, []), Ref.set(live.trace, idle)]);
    yield* Effect.forkScoped(Effect.catch(progress(live, records, Recorded, limit), lost(live)));
    const reply = yield* Effect.result(
        Effect.raceFirst(
            host.send(live, batch(records)),
            Effect.flatMap(Deferred.await(live.verdict), (over) => HarnessError.unavailable(`process ${live.pid}`)(verdictText(limit, live.pid, sendable)(over))),
        ),
    );
    yield* Effect.flatMap(entriesOf(Recorded)(records), (rows) => observe(live, rows));
    const [trace, notes, verdict] = yield* Effect.all([Ref.get(live.trace), Ref.get(live.notes), Effect.flatMap(Deferred.poll(live.verdict), Effect.transposeOption)]);
    return { reply, trace, notes, verdict } satisfies Ran<S>;
}, Effect.scoped);
const surveyIn = Effect.fn('surveyIn')(function* <S extends Surveyed, C extends Cased>(host: Pick<Host<S, C>, 'fields' | 'send'>, live: Live, opened: Option.Option<Schema.Json>, limit: Duration.Duration) {
    const { reply } = yield* batchIn(host, live, (records) => ({ records, label: LABEL, survey: Option.none(), opened, cases: [] }), [], limit);
    const header = yield* Effect.fromResult(reply);
    return yield* Effect.fromOption(Option.fromUndefinedOr(header.survey), () => HarnessError.unavailable(`process ${live.pid}`)('survey reply holds no survey'));
}, phase('survey'));
const finishedIn = Effect.fnUntraced(function* <S extends Surveyed, C extends Cased>(host: Session<S, C>, live: Live, survey: S, opened: Option.Option<Schema.Json>, rest: readonly Prepared<C>[], limit: Duration.Duration) {
    const ran = yield* batchIn(host, live, (records) => ({ records, label: LABEL, survey: Option.some(survey), opened, cases: rest.map(({ hosted }) => hosted) }), rest, limit);
    const { finished } = ran.trace;
    const on = (index: number) => ran.notes.filter(({ index: at }) => Option.contains(at, index)).map(({ answered }) => answered);
    const blamed = Option.firstSomeOf([
        Option.map(ran.verdict, (over) => ({
            index: over.index,
            tag: verdictTag(over),
            error: verdictText(limit, live.pid, rest)(over),
            halted: Option.map(Option.liftPredicate(over, Verdict.$is('Halted')), ({ halted }) => halted),
        })),
        Option.map(Result.getFailure(ran.reply), (error) => ({ index: ran.trace.running, tag: tagOf(error), error: error.message, halted: Option.none<typeof Halted.Type>() })),
    ]);
    const stopped = Option.flatMap(blamed, ({ index }) => index);
    const halting = Option.flatMap(blamed, ({ halted }) => halted);
    const failing = Option.filter(stopped, (index) => !HashMap.has(finished, index));
    const kept = Array.getSomes(rest.map((_, index) => Option.map(HashMap.get(finished, index), (row) => ({ ...row, answered: [...row.answered, ...on(index)], ...(Option.contains(stopped, index) ? Record.getSomes({ halted: halting }) : {}) }))));
    const halts = yield* Effect.forEach(Option.toArray(Option.all({ index: failing, item: Option.flatMap(failing, (index) => Array.get(rest, index)), blame: blamed })), ({ index, item, blame }) =>
        Effect.orDie(Schema.decodeEffect(Outcome)({ name: item.hosted.name, answered: on(index), completion: { _tag: 'Failed', reason: blame.error }, ...Record.getSomes({ halted: halting }) })).pipe(Effect.tap((row) => reported(row, blame.tag))),
    );
    yield* Option.match(
        Option.filter(blamed, () => Option.isNone(failing)),
        { onNone: () => Effect.void, onSome: ({ tag, error }) => Effect.annotateLogs(Effect.logError('batch failed'), { tag, error, ...named(rest, stopped), ...flat(halting) }) },
    );
    const skipped = yield* notRun(
        rest.filter((_, index) => !(HashMap.has(finished, index) || Option.contains(halted, index))),
        `not run, ${Option.match(blamed, { onNone: () => 'engine replied without recording the case', onSome: ({ error }) => error })}`,
    );
    return { ran, failed: Option.map(blamed, ({ error }) => error), outcomes: [...kept, ...halts, ...skipped] };
}, phase('batch'));
const ranIn = Effect.fnUntraced(function* <S extends Surveyed, C extends Cased>(host: Session<S, C>, live: Live, survey: S, opened: Option.Option<Schema.Json>, rest: readonly Prepared<C>[], limit: Duration.Duration) {
    const { ran, failed, outcomes } = yield* within(live)(finishedIn(host, live, survey, opened, rest, limit));
    const left = yield* Option.isNone(failed) && live.launched ? within(live)(quitted(host, live, limit)) : Effect.succeedNone;
    return { header: Result.getSuccess(ran.reply), failed: Option.orElse(failed, () => left), answered: ran.notes.filter(({ index }) => Option.isNone(index)).map(({ answered }) => answered), outcomes };
});
const execute = Effect.fn('execute')(function* <S extends Surveyed, C extends Cased, L extends Plan<C, unknown>>(
    host: Session<S, C>,
    chosen: Installed,
    scratch: string,
    limit: Duration.Duration,
    windowed: boolean,
    known: Option.Option<S>,
    planned: (survey: S) => Effect.Effect<L, Error, FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner | Run>,
) {
    const idled: Finished<S> = { header: Option.none(), shared: [], failed: Option.none(), answered: [], outcomes: [] };
    const draft = yield* Effect.transposeOption(Option.map(known, planned));
    const waiting = Option.filter(
        Option.zipWith(known, draft, (survey, plan): Done<S, C, unknown, L> => ({ ...idled, survey, opened: Option.none(), plan })),
        ({ plan }) => plan.sendable.length === 0,
    );
    const surveyed = (live: Live, opened: Option.Option<Schema.Json>) => Effect.onError(within(live)(surveyIn(host, live, opened, limit)), () => abandoned(host, live, limit));
    const launched = Effect.fnUntraced(function* () {
        const mode = Option.match(
            Option.flatMap(draft, ({ sendable }) => Array.head(sendable)),
            { onNone: () => windowed, onSome: (item) => item.windowed },
        );
        const { live, shared } = yield* acquired(host, chosen, scratch, limit, mode);
        const opened = yield* openedIn(host, live, limit);
        const confirmed = Option.filter(known, (held) => Option.match(opened, { onNone: () => true, onSome: (state) => same(held.host, state) }));
        const survey = yield* Option.match(confirmed, { onSome: Effect.succeed, onNone: () => surveyed(live, opened) });
        const plan = yield* Option.match(
            Option.filter(draft, () => Option.isSome(confirmed)),
            { onSome: Effect.succeed, onNone: () => planned(survey) },
        );
        const nearby = ({ windowed: held }: Prepared<C>) => held === mode;
        const [near, far] = Array.partition(plan.sendable, Result.liftPredicate(nearby, identity));
        const first: Finished<S> = { ...(yield* ranIn(host, live, survey, opened, near, limit)), shared };
        const unrun = (failed: string) => Effect.map(notRun(far, `not run, ${failed}`), (outcomes): Finished<S> => ({ ...idled, outcomes }));
        const relaunched = Effect.scoped(
            Effect.gen(function* () {
                const other = yield* acquired(host, chosen, scratch, limit, !mode);
                return { ...(yield* ranIn(host, other.live, survey, yield* openedIn(host, other.live, limit), far, limit)), shared: other.shared } satisfies Finished<S>;
            }),
        );
        const ran: readonly Finished<S>[] = [first, ...(far.length === 0 ? [] : [yield* Option.match(first.failed, { onSome: unrun, onNone: () => relaunched })])];
        return {
            survey,
            opened,
            plan,
            header: Option.firstSomeOf(ran.map(({ header }) => header)),
            shared: Array.dedupe(ran.flatMap(({ shared: held }) => held)),
            failed: Array.match(Array.getSomes(ran.map(({ failed }) => failed)), { onEmpty: Option.none, onNonEmpty: (held) => Option.some(held.join('; ')) }),
            answered: ran.flatMap(({ answered }) => answered),
            outcomes: ran.flatMap(({ outcomes }) => outcomes),
        } satisfies Done<S, C, unknown, L>;
    }, Effect.scoped);
    const done = yield* Option.match(waiting, { onSome: Effect.succeed, onNone: launched });
    const written = yield* writtenIn(host, done.plan.sendable);
    return { ...done, outcomes: [...done.plan.settled, ...done.outcomes].map((row) => ({ ...row, written: written.get(row.name) ?? [] })) };
});
const rerun = Effect.fn('rerun')(function* <S extends Surveyed, C extends Cased>(host: Session<S, C>, rebuilt: NonNullable<Host<S, C>['rebuilt']>, chosen: Installed, scratch: string, limit: Duration.Duration, engine: unknown, calls: readonly Rebuilt<C>[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const sendable = yield* Effect.forEach(calls, (call, index) => {
        const folder = path.join(scratch, 'rebuilt', `${index}`);
        return Effect.andThen(
            fs.makeDirectory(folder, { recursive: true }),
            Effect.map(rebuilt(call, folder), ({ hosted, map }): Prepared<C> => ({ hosted, fn: Option.some(call.fn), map, presses: [], copies: [], windowed: call.windowed })),
        );
    });
    return (yield* execute(
        host,
        chosen,
        scratch,
        limit,
        Option.exists(Array.head(sendable), (item) => item.windowed),
        Option.none(),
        () => Effect.succeed({ engine, sendable, settled: [] }),
    )).outcomes;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Any, type Batch, type Cased, type Decoding, type Done, type Entry, execute, type Fields, filesIn, type Header, type Host, type Installed, type Instance, inside, installation, installations, type Live, type Plan, type Prepared, type Rebuilt, rerun, type Services, type Session, type Surveyed };
