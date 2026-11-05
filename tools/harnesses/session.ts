import { createHash } from 'node:crypto';
import { fileURLToPath } from 'node:url';
import { type Installation, State, type Terms } from '@rasm/applications';
import { Automation } from '@rasm/applications/node';
import { Array, Data, DateTime, Deferred, Duration, Effect, Equal, Fiber, FileSystem, flow, HashMap, Match, Number, Option, Order, Path, type PlatformError, Predicate, Record, Ref, Result, Schema, Stream, String, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import type { Case } from '#/cases';
import { HarnessError } from '#/harness-error';
import { type Asked, type Launched, Observation, Progress, type Spawned, stalled, starts, watched } from '#/instance';
import { Messages, record, STATES, tagOf } from '#/log';
import { crashed, linked, type Rule } from '#/report';
import { Halted, type hostCase, Outcome, Records } from '#/results';
import { phase, Run, sample } from '#/run';
import type { Mapped } from '#/source';
import type { JsonObject } from '#/values';
import npm from './package.json' with { type: 'json' };

// --- [TYPES] ---------------------------------------------------------------------------

type Decoding<P> = Schema.Codec<P, unknown, never, unknown>;
type Services = Automation | ChildProcessSpawner.ChildProcessSpawner | FileSystem.FileSystem | Path.Path | Run;
type Instance = Pick<Live, 'pid' | 'started' | 'chosen' | 'scratch'> & { readonly ended: DateTime.Utc; readonly owned: readonly string[] };
type Entry = ReturnType<typeof Records.line<typeof Outcome>>['Type'];
type Verdict = Data.TaggedEnum<{
    Halted: { readonly index: Option.Option<number>; readonly halted: typeof Halted.Type };
    Lost: { readonly index: Option.Option<number>; readonly error: Error };
}>;
type Sighting = (typeof Messages)['window opened']['Encoded'];
interface Surveyed {
    readonly host: Schema.Json;
}
type Cased = ReturnType<typeof hostCase.linked<{ readonly folder: typeof Schema.String }>>['Type'];
interface Installed {
    readonly identifier: string;
    readonly name: string;
    readonly app: string;
    readonly version: string;
    readonly helpers: readonly string[];
    readonly environment?: Record.ReadonlyRecord<string, string>;
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
    readonly since: Option.Option<DateTime.Utc>;
    readonly windows: ReadonlyMap<string, Sighting>;
}
interface Trace {
    readonly next: number;
    readonly last: Option.Option<number>;
    readonly finished: HashMap.HashMap<number, typeof Outcome.Type>;
}
interface Live {
    readonly pid: number;
    readonly started: DateTime.Utc;
    readonly chosen: Installed;
    readonly scratch: string;
    readonly passing: readonly Progress[];
    readonly assembled: (line: number) => Option.Option<Mapped>;
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
interface Finished<S> {
    readonly header: Option.Option<Header<S>>;
    readonly shared: readonly number[];
    readonly failed: Option.Option<string>;
    readonly answered: typeof Outcome.fields.answered.Type;
    readonly outcomes: readonly (typeof Outcome.Type)[];
}
interface Harness<S extends Surveyed = Surveyed, C extends Cased = Cased, E = unknown, P = unknown> {
    readonly Engine: Schema.Codec<E, unknown>;
    readonly survey: { readonly question: string; readonly Schema: Schema.Codec<S, unknown> };
    readonly Case: Schema.Codec<C, unknown>;
    readonly Outcome: Schema.Codec<typeof Outcome.Type, unknown>;
    readonly progress?: readonly Progress[];
    readonly assembled?: Effect.Effect<{ readonly code: string; readonly located: (line: number) => Option.Option<Mapped> }, HarnessError>;
    readonly bundle?: Effect.Effect<URL, HarnessError>;
    readonly installed: Effect.Effect<readonly Installed[], Error, Services>;
    readonly engine: (scripts: readonly string[]) => Result.Result<E, HarnessError>;
    readonly precondition: (survey: S) => Decoding<P>;
    readonly placed: (decoded: P, folder: string, product: string) => Effect.Effect<{ readonly precondition: unknown; readonly copies: readonly string[] }, PlatformError.PlatformError, FileSystem.FileSystem | Path.Path>;
    readonly refusal?: (engine: E, hosted: C, binary: boolean) => Option.Option<string>;
    readonly windowed?: (precondition: typeof JsonObject.Type, row: Case) => boolean;
    readonly launch: (chosen: Installed, scratch: string, windowed: boolean) => Effect.Effect<Launched | Spawned, Error, Services>;
    readonly opened?: (live: Live) => Effect.Effect<Schema.Json, Error, Services>;
    readonly send: (live: Live, batch: Batch<S, C>) => Effect.Effect<Header<S>, Error, Services>;
    readonly quit: (live: Live) => Effect.Effect<void, Error, Services>;
    readonly canonical?: (file: string, bytes: Uint8Array) => Effect.Effect<readonly (string | Uint8Array)[], Error, Services>;
    readonly rebuilt?: (call: Rebuilt<C>, folder: string) => Effect.Effect<{ readonly hosted: C; readonly map: readonly Mapped[] }, Error, Services>;
    readonly logs?: (instance: Instance) => Effect.Effect<readonly Rule[], Error, Services>;
}
const Harness = <S extends Surveyed, C extends Cased, E, P>(harness: Harness<S, C, E, P>) => Effect.map(Effect.cached(harness.installed), (installed): Harness<S, C, E, P> => ({ ...harness, installed }));
type Planner<S, L> = (survey: S) => Effect.Effect<L, Error, FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner | Run>;
type Session<S extends Surveyed, C extends Cased> = Pick<Harness<S, C>, 'progress' | 'assembled' | 'Outcome' | 'launch' | 'opened' | 'send' | 'quit' | 'canonical' | 'logs' | 'bundle'>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const VERSIONS = new Intl.Collator('en', { numeric: true });
const LABEL = npm.name.replaceAll(/\W+/gu, '-').replace(/^-/u, '');
const ENGINE_LINE = / at line (?<line>\d+)$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Verdict = Data.taggedEnum<Verdict>();
const same = Schema.toEquivalence(Schema.Json);
const newest = Order.flip(
    Order.mapInput(
        Order.make((left: string, right: string) => Number.sign(VERSIONS.compare(left, right))),
        ({ version }: Installed) => version,
    ),
);
const idle: Trace = { next: 0, last: Option.none(), finished: HashMap.empty() };

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INSTALLATIONS]
const inside = (inner: Installation, outer: Installation) => inner.url.href !== outer.url.href && inner.url.href.startsWith(outer.url.href);
const installation = Effect.fn('installation')(function* (row: Installation, rows: readonly Installation[], extra: readonly string[]) {
    const path = yield* Path.Path;
    const app = yield* path.fromFileUrl(row.url);
    const version = yield* Effect.fromOption(row.version, () => HarnessError.unavailable(row.name)('Launch Services reports no version'));
    const helpers = Array.dedupe([...rows.filter((inner) => inside(inner, row)).map(({ identifier }) => identifier), ...extra]).filter((identifier) => identifier !== row.identifier);
    return { identifier: row.identifier, name: row.name, app, version, helpers } satisfies Installed;
});
const installations = (rows: readonly Installed[], prefixes: readonly string[], subject: string) => {
    const hosts = Array.sort(rows, newest);
    const chosen = prefixes.length === 0 ? hosts.slice(0, 1) : hosts.filter(({ version }) => prefixes.some((prefix) => version.startsWith(prefix)));
    return Effect.fromOption(Option.liftPredicate(chosen, Array.isArrayNonEmpty), () => HarnessError.unavailable(`${subject} ${prefixes.join(', ')}`)(`installed versions ${hosts.map(({ version }) => version).join(', ')}`));
};

// --- [VERDICTS]
const caseOf = (batch: readonly Prepared<Cased>[], index: Option.Option<number>) =>
    Option.map(
        Option.flatMap(index, (at) => Array.get(batch, at)),
        ({ hosted }) => hosted.name,
    );
const named = (batch: readonly Prepared<Cased>[], index: Option.Option<number>) => Record.getSomes({ case: caseOf(batch, index) });
const clean = ({ signal, report, status }: { readonly signal?: number; readonly report?: unknown; readonly status?: number }) => signal === undefined && report === undefined && status === 0;
const flat = Option.match({ onNone: () => ({}), onSome: Struct.omit<typeof Halted.Type, readonly ['_tag']>(['_tag']) });
const blameOf = (limit: Duration.Duration, pid: number, batch: readonly Prepared<Cased>[]) => (verdict: Verdict) => {
    const { tag, text, halted } = Verdict.$match(verdict, {
        Halted: ({ halted: held }) => ({
            tag: held._tag,
            text: Halted.match(held, {
                Crashed: ({ status, signal, report }) => `instance ${pid} exited with ${signal === undefined ? `status ${status ?? 'unread'}` : `signal ${signal}`}${report === undefined ? '' : `, report ${report}`}`,
                Hung: ({ process, sample: file }) => `instance ${pid} hung${process === undefined ? '' : ` ${process}`} past ${Duration.format(limit)}, sample ${file}, left running`,
                Dialog: (dialog) => Observation.shown(dialog, pid),
            }),
            halted: Option.some(held),
        }),
        Lost: ({ error }) => ({ tag: tagOf(error), text: error.message, halted: Option.none<typeof Halted.Type>() }),
    });
    return { index: verdict.index, tag, halted, error: [...Option.toArray(Option.map(caseOf(batch, verdict.index), (name) => `case ${name}`)), text].join(': ') };
};
const ended = (live: Live, verdict: (trace: Trace) => Verdict) => Effect.flatMap(Ref.get(live.trace), (trace) => Effect.asVoid(Deferred.succeed(live.verdict, verdict(trace))));
const runningOf = ({ last, finished }: Trace) => Option.filter(last, (index) => !HashMap.has(finished, index));
const lost = (live: Live) => (error: Error) => ended(live, (trace) => Verdict.Lost({ index: runningOf(trace), error }));
const verdicted =
    (live: Live, subject: string) =>
    <A, R>(self: Effect.Effect<A, Error, R>) =>
        self.pipe(
            Effect.catch(lost(live)),
            Effect.catchDefect((defect) => lost(live)(HarnessError.refused(subject)(defect))),
        );
const exitedWith = (live: Live) => (halted: typeof Halted.Type) => ended(live, ({ last }) => Verdict.Halted({ index: last, halted }));

// --- [RECORDS]
const filesIn = Effect.fn('filesIn')(function* (folder: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const isFile = (file: string) => Effect.map(fs.stat(file), ({ type }) => type === 'File');
    return yield* Effect.filter(
        (yield* fs.readDirectory(folder, { recursive: true })).map((file) => path.join(folder, file)),
        isFile,
    );
});
const digest = (host: Pick<Harness, 'canonical'>) =>
    Effect.fnUntraced(function* (file: string) {
        const bytes = yield* FileSystem.FileSystem.use((fs) => fs.readFile(file));
        const parts: readonly (string | Uint8Array)[] = host.canonical === undefined ? [bytes] : yield* host.canonical(file, bytes);
        return { file, size: bytes.byteLength, sha256: parts.reduce((hash, part) => hash.update(part), createHash('sha256')).digest('hex') } satisfies (typeof Outcome.fields.written.Type)[number];
    });
const relocated = (engine: (line: number) => Option.Option<Mapped>) => (found: string, line: string) => Option.match(Option.flatMap(Number.parse(line), engine), { onNone: () => found, onSome: (held) => ` at ${held.file}:${held.line}` });
const mapped = (map: readonly Mapped[], engine: (line: number) => Option.Option<Mapped>) => (row: typeof Outcome.Type) => {
    const at = (frame: { readonly line: number; readonly file?: string }) => (frame.file === undefined ? Option.getOrElse(Array.get(map, frame.line - 1), () => ({ line: frame.line })) : frame);
    const located = (text: string) => text.replace(ENGINE_LINE, relocated(engine));
    return Outcome.fields.completion.match(row.completion, {
        Ran: () => row,
        Exited: () => row,
        Thrown: (thrown) => ({
            ...row,
            completion: {
                ...thrown,
                message: located(thrown.message),
                ...(thrown.line === undefined ? {} : at({ line: thrown.line, ...Record.getSomes({ file: Option.fromUndefinedOr(thrown.file) }) })),
                ...(thrown.stack === undefined ? {} : { stack: thrown.stack.map(at) }),
            },
        }),
        Failed: ({ reason }) => ({ ...row, completion: { _tag: 'Failed' as const, reason: located(reason) } }),
    });
};
const reported = (row: typeof Outcome.Type, tag: string = row.halted?._tag ?? row.completion._tag) => {
    const fields: Record.ReadonlyRecord<string, unknown> = row;
    const unlogged = ['name', 'completion', 'before', 'opened', 'written', 'halted'];
    const passed = row.halted === undefined && Outcome.fields.completion.isAnyOf(['Ran', 'Exited'])(row.completion);
    const facts = {
        case: row.name,
        completion: row.completion._tag,
        ...Outcome.fields.completion.match(row.completion, {
            Ran: () => ({}),
            Exited: () => ({}),
            Thrown: ({ message, file, line, number, stack }) => ({ error: message, ...Record.filter({ file, line, number }, Predicate.isNotUndefined), ...(stack === undefined ? {} : { stack: stack.map((frame) => [frame.file, frame.line].filter((part) => part !== undefined).join(':')) }) }),
            Failed: ({ reason }) => ({ error: reason }),
        }),
        ...Record.filter(fields, (held, key) => !unlogged.includes(key) && held !== undefined && (key === 'value' || !(Predicate.isObjectOrArray(held) && Object.keys(held).length === 0))),
        ...flat(Option.fromUndefinedOr(row.halted)),
        ...(tag === row.completion._tag ? {} : { tag }),
    };
    return record(passed ? 'Info' : 'Error', 'case ended', facts);
};
const notRun = Effect.fnUntraced(function* (unsent: readonly Prepared<Cased>[], reason: string) {
    const outcomes = yield* Effect.forEach(unsent, ({ hosted }) => Effect.orDie(Schema.decodeEffect(Outcome)({ name: hosted.name, completion: { _tag: 'Failed', reason } })));
    yield* Array.match(outcomes, { onEmpty: () => Effect.void, onNonEmpty: (held) => record('Warn', 'cases not run', { cases: held.map(({ name }) => name), error: reason }) });
    return outcomes;
});
const scriptOf = Match.typeTags<Cased, Option.Option<string>>()({ Probe: () => Option.none<string>(), Panel: () => Option.none<string>(), Call: ({ source }) => Option.map(Option.fromUndefinedOr(source), ({ script }) => script), Run: ({ source }) => Option.some(source.script) });
const called = ({ hosted, fn }: Prepared<Cased>) => ({
    _tag: hosted._tag,
    ...Record.getSomes({ fn, script: scriptOf(hosted) }),
    ...Match.valueTags(hosted, { Probe: () => ({}), Panel: ({ panel }) => ({ panel }), Call: ({ args }) => ({ args }), Run: ({ args }) => ({ args }) }),
});
const observe = Effect.fnUntraced(function* (live: Live, records: string, Recorded: Schema.Codec<Entry, string>) {
    const [batch, { next: from }, text] = yield* Effect.all([Ref.get(live.batch), Ref.get(live.trace), FileSystem.FileSystem.use((fs) => fs.readFileString(records))]);
    const rows = yield* Effect.forEach(text.split('\n').slice(0, -1).filter(String.isNonEmpty).slice(from), (line) => Schema.decodeEffect(Recorded)(line));
    const started = (index: number) => Option.match(Array.get(batch, index), { onNone: () => Effect.void, onSome: (item) => record('Info', 'case started', { case: item.hosted.name, call: called(item) }) });
    const step = (held: Trace, entry: Entry): readonly [Trace, Effect.Effect<void>] =>
        Match.valueTags(entry, {
            Started: ({ index }) => [{ ...held, last: Option.some(index) }, started(index)] as const,
            Finished: ({ outcome }) => {
                const map = Option.match(
                    Option.flatMap(runningOf(held), (at) => Array.get(batch, at)),
                    { onNone: () => [], onSome: (item) => item.map },
                );
                const row = mapped(map, live.assembled)(outcome);
                return [{ ...held, finished: Option.match(runningOf(held), { onNone: () => held.finished, onSome: (at) => HashMap.set(held.finished, at, row) }) }, reported(row)] as const;
            },
        });
    const logs = yield* Ref.modify(live.trace, (trace) => {
        const [next, steps] = Array.mapAccum(rows.slice(trace.next - from), trace, step);
        return [steps, { ...next, next: Math.max(trace.next, from + rows.length) }] as const;
    });
    yield* Effect.all(logs, { discard: true });
});
const writtenIn = Effect.fnUntraced(function* (host: Pick<Harness, 'canonical'>, sendable: readonly Prepared<Cased>[]) {
    const digesting = digest(host);
    const copied = new Set((yield* Effect.forEach(Array.dedupe(sendable.flatMap(({ copies }) => copies)), digesting)).map(({ sha256 }) => sha256));
    const fresh = ({ sha256 }: { readonly sha256: string }) => !copied.has(sha256);
    const written = yield* Effect.forEach(sendable, ({ hosted }) => Effect.flatMap(filesIn(hosted.folder), Effect.forEach(digesting)));
    return new Map(Array.zipWith(sendable, written, ({ hosted }, files) => [hosted.name, files.filter(fresh)] as const));
});
const instanceOf = Effect.fnUntraced(function* (host: Pick<Harness, 'bundle'>, live: Live) {
    const [now, batch, path] = yield* Effect.all([DateTime.now, Ref.get(live.batch), Path.Path]);
    const bundle = yield* Effect.orDie(Effect.transposeOption(Option.fromUndefinedOr(host.bundle)));
    return {
        ...Struct.pick(live, ['pid', 'started', 'chosen', 'scratch']),
        ended: now,
        owned: [live.scratch, ...Option.toArray(Option.map(bundle, (url) => path.basename(fileURLToPath(url)))), ...batch.flatMap(({ hosted }) => [hosted.name, hosted.folder, ...Option.toArray(scriptOf(hosted))])],
    } satisfies Instance;
});
const reportsOf = (host: Pick<Harness, 'logs' | 'bundle'>, live: Live) => Effect.flatMap(instanceOf(host, live), (instance) => linked(host, instance));

// --- [SCRIPTS]
const scripted = Effect.fn('scripted')(function* ({ pid, chosen: { app } }: Pick<Live, 'pid' | 'chosen'>, script: Parameters<typeof Terms.script>[1]) {
    const [automation, path] = yield* Effect.all([Automation, Path.Path]);
    return yield* automation.script({ pid, url: yield* path.toFileUrl(app), ...script });
});

// --- [WATCHER]
const usageOf = Match.type<typeof State.Type>().pipe(
    Match.when({ running: ({ status }: typeof State.Running.Type) => status === 'unresponsive' }, ({ running }) => ({ ...Struct.pick(running, ['process']), ...UndefinedOr.match(running.usage, { onUndefined: () => ({}), onDefined: Struct.pick(['cpu', 'footprint', 'threads']) }) })),
    Match.orElse(() => ({})),
);
const factsOf = (state: typeof State.Type) =>
    State.match<object>(state, {
        running: () => usageOf(state),
        failed: ({ reason }) => ({ reason }),
        exited: (exit) => ({
            ...Struct.pick(exit, ['status', 'signal', 'detail', 'statusFailure', 'reportFailure']),
            ...UndefinedOr.match(exit.report, { onUndefined: () => ({}), onDefined: ({ url, exception }) => ({ report: fileURLToPath(url), ...Record.getSomes({ exception: Option.fromUndefinedOr(exception) }) }) }),
        }),
    });
const sightingOf = ({ number, kind, asking, title, element }: typeof State.Window.Type): Sighting => ({
    number,
    kind,
    asking,
    ...Record.getSomes({ title: Option.fromUndefinedOr(element?.title ?? title) }),
    ...UndefinedOr.match(element, {
        onUndefined: () => ({}),
        onDefined: (held) => ({
            ...Struct.pick(held, ['role', 'subrole', 'edited', 'modal', 'identifier']),
            ...UndefinedOr.match(held.document, { onUndefined: () => ({}), onDefined: (url) => ({ document: fileURLToPath(url) }) }),
            ...UndefinedOr.match(held.detail, { onUndefined: () => ({}), onDefined: ({ text, controls }) => ({ text, controls: controls.map(Struct.pick(['label', 'role', 'value', 'enabled', 'attribute'])) }) }),
        }),
    }),
});
const windowsOf = (state: typeof State.Type, held: ReadonlyMap<string, Sighting>, ours: boolean): ReadonlyMap<string, Sighting> =>
    State.match<ReadonlyMap<string, Sighting>>(state, { running: ({ windows }) => new Map(windows.filter(({ presented, asking }) => presented && (ours || asking)).map((window) => [`${window.number}`, sightingOf(window)] as const)), failed: () => held, exited: () => held });
const seenOf = (prior: Option.Option<Seen>, row: typeof Observation.Type, ours: boolean): Seen => ({
    state: row.state,
    since: Observation.tag(row.state) === 'unresponsive' ? Option.orElseSome(Option.flatMap(prior, Struct.get('since')), () => row.at) : Option.none(),
    windows: windowsOf(row.state, Option.match(prior, { onNone: () => new Map(), onSome: Struct.get('windows') }), ours),
});
const unchanged = Schema.toEquivalence(Messages.instance);
const alike = Schema.toEquivalence(Messages['window changed']);
const sighted = (row: typeof Observation.Type, before: Option.Option<Seen>, seen: Seen, ours: boolean, asked: Option.Option<Asked>) => {
    const prior = Option.match(before, { onNone: () => new Map<string, Sighting>(), onSome: (held) => held.windows });
    const tag = Observation.tag(row.state);
    const spell = Option.filter(Option.flatMap(before, Struct.get('since')), () => tag !== 'unresponsive');
    const facts = { state: tag, ...factsOf(row.state), ...Record.getSomes({ duration: Option.map(spell, (since) => Duration.toSeconds(DateTime.distance(since, row.at))) }) };
    const kept = Option.exists(before, (held) => Observation.tag(held.state) === tag && ((tag !== 'failed' && tag !== 'exited') || unchanged({ state: tag, ...factsOf(held.state) }, { state: tag, ...factsOf(row.state) })));
    const dialog = Option.map(asked, ({ window }) => `${window.number}`);
    const level = (key: string) => (Option.contains(dialog, key) ? 'Warn' : 'Info');
    const opened = Array.fromIterable(seen.windows).filter(([key]) => !prior.has(key));
    const changed = Array.fromIterable(seen.windows).filter(([key, held]) => Option.exists(Option.fromUndefinedOr(prior.get(key)), (was) => !alike(was, held)));
    const closed = Array.fromIterable(prior).filter(([key]) => !seen.windows.has(key));
    return Effect.all(
        [
            (ours || (Predicate.hasProperty(row.state, 'exited') && !clean(row.state.exited))) && !kept ? record(STATES[tag], 'instance', facts) : Effect.void,
            Effect.forEach(opened, ([key, held]) => record(level(key), 'window opened', held), { discard: true }),
            Effect.forEach(changed, ([key, held]) => record(level(key), 'window changed', held), { discard: true }),
            Effect.forEach(closed, ([, held]) => record('Info', 'window closed', held), { discard: true }),
        ],
        { discard: true },
    );
};
const within = (live: Live) => Effect.provideService(Run.Instance, Option.some(live));
const owned = (live: Live, row: typeof Observation.Type) => row.pid === live.pid;
const noteOf = ({ element, number }: typeof State.Window.Type) => element?.id ?? `${number}`;
const stacked = Effect.fnUntraced(function* (pid: number) {
    const file = yield* sample(pid);
    return { sample: file, frames: yield* stalled(file) };
});
const blocked = Effect.fnUntraced(function* (live: Live, window: typeof State.Window.Type, dialog: typeof Halted.cases.Dialog.Type) {
    const stack = yield* stacked(live.pid);
    return stack.frames.some(String.includes('Modal')) ? Option.some(dialog) : yield* Effect.as(record('Info', 'window busy', { number: window.number, ...stack }), Option.none<typeof Halted.cases.Dialog.Type>());
});
const answering = (live: Live, ours: boolean) =>
    Effect.fnUntraced(function* ({ window, dialog }: Asked, pid: number) {
        const [trace, batch, notes] = yield* Effect.all([Ref.get(live.trace), Ref.get(live.batch), Ref.get(live.notes)]);
        const running = runningOf(trace);
        const used = notes.filter(({ index }) => Equal.equals(index, running)).length;
        const control = running.pipe(
            Option.filter(() => ours),
            Option.flatMap((index) => Array.get(batch, index)),
            Option.flatMap(({ presses }) => Array.get(presses, used)),
            Option.flatMap((press) => Progress.pressed(window, press)),
        );
        const halting = (halted: typeof Halted.Type) => ended(live, (held) => Verdict.Halted({ index: runningOf(held), halted }));
        const answered = (press: string) => ({ ...Record.getSomes({ title: Option.fromUndefinedOr(dialog.title) }), text: dialog.text, pressed: press });
        yield* Option.match(control, {
            onNone: () => Effect.flatMap(window.kind === 'blocking' && ours ? blocked(live, window, dialog) : Effect.succeedSome(dialog), Option.match({ onNone: () => Effect.void, onSome: halting })),
            onSome: ({ press, id }) => Effect.catch(Effect.andThen(Effect.all([Ref.update(live.notes, Array.append({ index: running, element: noteOf(window), answered: answered(press) })), record('Info', 'dialog answered', answered(press))]), Observation.perform(pid, id)), lost(live)),
        });
    });
const exitOf = (seen: SubscriptionRef.SubscriptionRef<HashMap.HashMap<number, Seen>>, pid: number) =>
    SubscriptionRef.changes(seen).pipe(
        Stream.filter((map) => Option.exists(HashMap.get(map, pid), ({ state }) => Predicate.hasProperty(state, 'exited'))),
        Stream.runHead,
    );
const observed =
    (live: Live, row: typeof Observation.Type) =>
    <A, E, R>(effect: Effect.Effect<A, E, R>) =>
        owned(live, row) ? Effect.flatMap(Effect.all([Ref.get(live.trace), Ref.get(live.batch)]), ([trace, batch]) => within(live)(Effect.annotateLogs(effect, named(batch, runningOf(trace))))) : Effect.annotateLogs(effect, { bundle: row.identifier, pid: row.pid });
const arrived = (live: Live, seen: SubscriptionRef.SubscriptionRef<HashMap.HashMap<number, Seen>>) =>
    Effect.fn('arrived')(
        function* (row: typeof Observation.Type) {
            const ours = owned(live, row);
            const [before, held] = yield* SubscriptionRef.modify(seen, (map) => {
                const prior = HashMap.get(map, row.pid);
                const next = seenOf(prior, row, ours);
                return [[prior, next] as const, HashMap.set(map, row.pid, next)] as const;
            });
            yield* ours ? SubscriptionRef.set(live.state, row.state) : Effect.void;
            yield* sighted(row, before, held, ours, Progress.asked(live.passing, row.state));
        },
        (effect, row) => observed(live, row)(effect),
    );
const handled = (host: Pick<Harness, 'logs' | 'bundle'>, live: Live, seen: SubscriptionRef.SubscriptionRef<HashMap.HashMap<number, Seen>>) =>
    Effect.fn('handled')(
        function* (row: typeof Observation.Type) {
            const ours = owned(live, row);
            const noted = (yield* Ref.get(live.notes)).map(Struct.get('element'));
            yield* Option.match(
                Option.filter(Progress.asked(live.passing, row.state), ({ window }) => !noted.includes(noteOf(window))),
                { onNone: () => Effect.void, onSome: (found) => answering(live, ours)(found, row.pid) },
            );
            yield* Match.value({ ours, state: row.state }).pipe(
                Match.when({ ours: true, state: { failed: Match.defined } }, ({ state: { failed } }) => lost(live)(HarnessError.unavailable(`process ${row.pid}`)(failed.reason))),
                Match.orElse(() => Effect.void),
            );
            yield* ours && Observation.settled(row.state) ? Effect.uninterruptible(Effect.andThen(Effect.flatMap(crashed(live.pid, row.state.exited), exitedWith(live)), reportsOf(host, live))) : Effect.void;
        },
        (effect, row) => (Predicate.hasProperty(row.state, 'exited') ? effect : Effect.raceFirst(effect, Effect.asVoid(exitOf(seen, row.pid)))).pipe(verdicted(live, `instance ${row.pid} row`), observed(live, row)),
    );
const progress = Effect.fnUntraced(function* (live: Live, records: string, Recorded: Schema.Codec<Entry, string>, limit: Duration.Duration) {
    yield* watched(records).pipe(
        Stream.mapEffect(() => observe(live, records, Recorded)),
        Stream.debounce(limit),
        Stream.runHead,
    );
    const [stack, state] = yield* Effect.all([stacked(live.pid), SubscriptionRef.get(live.state)]);
    yield* ended(live, (trace) => Verdict.Halted({ index: runningOf(trace), halted: { _tag: 'Hung', ...usageOf(state), ...stack } }));
});

// --- [SESSION]
const acquired = Effect.fn('acquired')(function* (host: Pick<Harness, 'progress' | 'assembled' | 'launch' | 'logs' | 'bundle'>, chosen: Installed, root: string, limit: Duration.Duration, windowed: boolean, release: Ref.Ref<Option.Option<Effect.Effect<void, Error, Services>>>) {
    const fs = yield* FileSystem.FileSystem;
    const refused = HarnessError.unavailable(chosen.name);
    const passing = host.progress ?? [];
    const scratch = yield* fs.makeTempDirectoryScoped({ directory: root, prefix: 'instance-' });
    const spec = yield* host.launch(chosen, scratch, windowed);
    const instance = yield* Effect.timeoutOrElse(
        spec.start(chosen, passing, (held) => Ref.set(release, Option.some(held))),
        { duration: limit, orElse: () => Effect.fail(refused(`launch past ${Duration.format(limit)}`)) },
    );
    yield* Observation.settled(instance.state) ? refused(`instance ${instance.pid} ${Observation.described(instance.state)}`) : Effect.void;
    const starting = yield* starts(['-p', `${instance.pid}`]);
    const started = yield* Effect.fromOption(Option.fromUndefinedOr(starting.get(instance.pid)), () => HarnessError.unavailable(`process ${instance.pid}`)('ps reports no start time'));
    const engine = yield* Effect.transposeOption(Option.fromUndefinedOr(host.assembled));
    const live: Live = {
        pid: instance.pid,
        started,
        chosen,
        scratch,
        passing,
        assembled: Option.match(engine, { onNone: () => () => Option.none(), onSome: ({ located }) => located }),
        verdict: yield* Deferred.make<Verdict>(),
        notes: yield* Ref.make<readonly Note[]>([]),
        trace: yield* Ref.make(idle),
        batch: yield* Ref.make<readonly Prepared<Cased>[]>([]),
        state: yield* SubscriptionRef.make<typeof State.Type>(instance.state),
    };
    yield* within(live)(
        record('Info', 'instance launched', {
            app: chosen.app,
            created: instance.created,
            state: Observation.tag(instance.state),
            ...Array.match(instance.shared, { onEmpty: () => ({}), onNonEmpty: (shared) => ({ shared }) }),
        }),
    );
    const seen = yield* SubscriptionRef.make(HashMap.set(HashMap.empty<number, Seen>(), instance.pid, { state: instance.state, since: Option.none(), windows: new Map() }));
    const [arriving, handling] = [arrived(live, seen), handled(host, live, seen)];
    const first = Struct.pick(instance, ['identifier', 'pid', 'state', 'at']);
    yield* Effect.andThen(arriving(first), handling(first));
    yield* instance.rows.pipe(
        Stream.tap(arriving),
        Stream.groupByKey(Struct.get('pid')),
        Stream.mapEffect(([, rows]) => rows.pipe(Stream.buffer({ capacity: 1, strategy: 'sliding' }), Stream.runForEach(handling)), { concurrency: 'unbounded', unordered: true }),
        Stream.runDrain,
        Effect.andThen(refused(`instance ${instance.pid} watch ended`)),
        verdicted(live, `instance ${instance.pid} watch`),
        Effect.forkScoped({ startImmediately: true }),
    );
    yield* Ref.update(
        release,
        Option.map((quit) => within(live)(quitted(host, live, limit, quit))),
    );
    yield* Effect.raceFirst(
        SubscriptionRef.changes(live.state).pipe(
            Stream.filter((state) => Observation.tag(state) === 'responding' && Array.isReadonlyArrayEmpty(State.asking(state))),
            Stream.runHead,
        ),
        Effect.flatMap(Deferred.await(live.verdict), (over) => refused(blameOf(limit, instance.pid, [])(over).error)),
    ).pipe(
        Effect.timeoutOrElse({
            duration: limit,
            orElse: () => Effect.flatMap(Effect.all([sample(live.pid), SubscriptionRef.get(live.state), Ref.set(release, Option.none())]), ([file, state]) => refused(`instance ${instance.pid} ${Observation.described(state)} past ${Duration.format(limit)}, sample ${file}, left running`)),
        }),
        phase('ready'),
        within(live),
    );
    return { live, shared: instance.shared };
}, phase('launch'));
const quitted = Effect.fn('quitted')(
    function* (host: Pick<Harness, 'logs' | 'bundle'>, live: Live, limit: Duration.Duration, quit: Effect.Effect<void, Error, Services>) {
        yield* record('Info', 'quit sent', {});
        const waited = SubscriptionRef.changes(live.state).pipe(Stream.filter(Observation.settled), Stream.runHead, Effect.timeoutOption(limit), Effect.map(Option.flatten));
        const watching = yield* Effect.forkChild(waited, { startImmediately: true });
        const stopped = yield* Effect.result(Effect.raceFirst(Fiber.join(watching), Effect.andThen(quit, Fiber.join(watching))));
        const exited = Option.flatten(Result.getSuccess(stopped));
        const silent = Option.filter(Result.getSuccess(stopped), Option.isNone);
        const running = yield* Effect.transposeOption(Option.as(silent, stacked(live.pid)));
        const refused = HarnessError.unavailable(`process ${live.pid}`);
        const failed = Result.match(stopped, {
            onFailure: Option.some,
            onSuccess: () =>
                Option.firstSomeOf([
                    Option.map(
                        Option.filter(exited, (value) => !clean(value.exited)),
                        flow(Observation.described, refused),
                    ),
                    Option.map(running, ({ sample: file }) => refused(`quit sent, running past ${Duration.format(limit)}, sample ${file}, left running`)),
                ]),
        });
        yield* Option.isSome(exited) ? Effect.void : reportsOf(host, live);
        yield* Option.match(failed, { onNone: () => Effect.void, onSome: (error) => Effect.andThen(record('Warn', 'failed', { tag: tagOf(error), error: error.message, ...Option.getOrElse(running, () => ({})) }), Effect.fail(error)) });
    },
    phase('quit'),
    (effect, _, live) => Effect.flatMap(Deferred.isDone(live.verdict), (halted) => (halted ? Effect.void : effect)),
);
const openedIn = (host: Pick<Harness, 'opened'>, live: Live, limit: Duration.Duration) =>
    host.opened === undefined
        ? Effect.succeedNone
        : Effect.asSome(host.opened(live)).pipe(
              Effect.timeoutOrElse({
                  duration: limit,
                  orElse: () => Effect.flatMap(sample(live.pid), (file) => HarnessError.unavailable(live.chosen.name)(`instance ${live.pid} opened nothing past ${Duration.format(limit)}, sample ${file}`)),
              }),
              phase('open'),
              within(live),
          );
const batchIn = Effect.fn('batchIn')(function* <S extends Surveyed, C extends Cased>(host: Pick<Harness<S, C>, 'Outcome' | 'send'>, live: Live, batch: (records: string) => Batch<S, C>, sendable: readonly Prepared<C>[], limit: Duration.Duration) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const Recorded = Records.line(host.Outcome);
    const records = path.join(yield* fs.makeTempDirectoryScoped({ directory: live.scratch, prefix: 'batch-' }), 'records.jsonl');
    yield* fs.writeFileString(records, '');
    yield* Effect.all([Ref.set(live.batch, sendable), Ref.set(live.notes, []), Ref.set(live.trace, idle)]);
    yield* Effect.forkScoped(verdicted(live, records)(progress(live, records, Recorded, limit)));
    const reply = yield* Effect.result(
        Effect.raceFirst(
            host.send(live, batch(records)),
            Effect.flatMap(Deferred.await(live.verdict), (over) => HarnessError.unavailable(`process ${live.pid}`)(blameOf(limit, live.pid, sendable)(over).error)),
        ),
    );
    yield* observe(live, records, Recorded);
    const [trace, notes, verdict] = yield* Effect.all([Ref.get(live.trace), Ref.get(live.notes), Effect.flatMap(Deferred.poll(live.verdict), Effect.transposeOption)]);
    return { reply, trace, notes, verdict };
}, Effect.scoped);
const surveyIn = Effect.fn('surveyIn')(
    function* <S extends Surveyed, C extends Cased>(host: Pick<Harness<S, C>, 'Outcome' | 'send'>, live: Live, opened: Option.Option<Schema.Json>, limit: Duration.Duration) {
        const { reply } = yield* batchIn(host, live, (records) => ({ records, label: LABEL, survey: Option.none(), opened, cases: [] }), [], limit);
        const header = yield* Effect.fromResult(reply);
        return yield* Effect.fromOption(Option.fromUndefinedOr(header.survey), () => HarnessError.unavailable(`process ${live.pid}`)('survey reply holds no survey'));
    },
    phase('survey'),
    (effect, _, live) => within(live)(effect),
);
const finishedIn = Effect.fnUntraced(function* <S extends Surveyed, C extends Cased>(host: Session<S, C>, live: Live, survey: S, opened: Option.Option<Schema.Json>, rest: readonly Prepared<C>[], limit: Duration.Duration) {
    const cases = rest.map(({ hosted }) => hosted);
    const ran = yield* batchIn(host, live, (records) => ({ records, label: LABEL, survey: Option.some(survey), opened, cases }), rest, limit);
    const { finished } = ran.trace;
    const running = runningOf(ran.trace);
    const replied = Option.map(Result.getFailure(ran.reply), (error) => Verdict.Lost({ index: running, error }));
    const blamed = Option.map(Option.firstSomeOf([ran.verdict, replied]), blameOf(limit, live.pid, rest));
    const stopped = Option.flatMap(blamed, Struct.get('index'));
    const halting = Option.flatMap(blamed, Struct.get('halted'));
    const failing = Option.filter(stopped, (index) => !HashMap.has(finished, index));
    const on = (index: number) => ran.notes.filter(({ index: at }) => Option.contains(at, index)).map(Struct.get('answered'));
    const kept = Array.fromIterable(finished).map(([index, row]) => ({ ...row, answered: [...row.answered, ...on(index)], ...(Option.contains(stopped, index) ? Record.getSomes({ halted: halting }) : {}) }));
    const halt = ({ index, item, blame }: { readonly index: number; readonly item: Prepared<C>; readonly blame: { readonly tag: string; readonly error: string } }) =>
        Effect.orDie(Schema.decodeEffect(Outcome)({ name: item.hosted.name, answered: on(index), completion: { _tag: 'Failed', reason: blame.error }, ...Record.getSomes({ halted: halting }) })).pipe(Effect.tap((row) => reported(row, blame.tag)));
    const halts = yield* Effect.transposeOption(Option.map(Option.all({ index: failing, item: Option.flatMap(failing, (index) => Array.get(rest, index)), blame: blamed }), halt));
    yield* Option.match(
        Option.filter(blamed, () => Option.isNone(failing)),
        { onNone: () => Effect.void, onSome: ({ tag, error }) => record('Error', 'failed', { tag, error, ...named(rest, stopped), ...flat(halting) }) },
    );
    const skipped = yield* notRun(
        rest.filter((_, index) => !(HashMap.has(finished, index) || Option.contains(failing, index))),
        `not run, ${Option.match(blamed, { onNone: () => 'engine replied without recording the case', onSome: Struct.get('error') })}`,
    );
    return { header: Result.getSuccess(ran.reply), failed: Option.map(blamed, Struct.get('error')), answered: ran.notes.filter(({ index }) => Option.isNone(index)).map(({ answered }) => answered), outcomes: [...kept, ...Option.toArray(halts), ...skipped] };
}, phase('batch'));
const instanced = <S extends Surveyed, C extends Cased, L extends Plan<C, unknown>>(host: Session<S, C>, chosen: Installed, scratch: string, limit: Duration.Duration, mode: boolean, known: Option.Option<S>, draft: Option.Option<L>, planned: Planner<S, L>) =>
    Effect.acquireUseRelease(
        Ref.make(Option.none<Effect.Effect<void, Error, Services>>()),
        Effect.fn('instanced')(function* (release: Ref.Ref<Option.Option<Effect.Effect<void, Error, Services>>>) {
            const { live, shared } = yield* acquired(host, chosen, scratch, limit, mode, release);
            const opened = yield* openedIn(host, live, limit);
            yield* Ref.update(
                release,
                Option.map(() => within(live)(quitted(host, live, limit, host.quit(live)))),
            );
            const confirmed = Option.filter(known, (held) => Option.isNone(opened) || Option.containsWith(same)(opened, held.host));
            const survey = yield* Option.match(confirmed, { onSome: Effect.succeed, onNone: () => surveyIn(host, live, opened, limit) });
            const plan = yield* Option.match(
                Option.filter(draft, () => Option.isSome(confirmed)),
                { onSome: Effect.succeed, onNone: () => planned(survey) },
            );
            const finished = yield* within(live)(
                finishedIn(
                    host,
                    live,
                    survey,
                    opened,
                    plan.sendable.filter((item) => item.windowed === mode),
                    limit,
                ),
            );
            const quit = yield* Effect.transposeOption(Option.map(yield* Ref.getAndSet(release, Option.none()), Effect.result));
            const ran: Finished<S> = { ...finished, failed: Option.orElse(finished.failed, () => Option.map(Option.flatMap(quit, Result.getFailure), Struct.get('message'))), shared };
            return { survey, opened, plan, ran };
        }),
        (release) =>
            Effect.flatMap(
                Ref.get(release),
                Option.getOrElse(() => Effect.void),
            ),
    ).pipe(Effect.scoped);
const execute = Effect.fn('execute')(function* <S extends Surveyed, C extends Cased, L extends Plan<C, unknown>>(host: Session<S, C>, chosen: Installed, scratch: string, limit: Duration.Duration, windowed: boolean, known: Option.Option<S>, planned: Planner<S, L>) {
    const idled: Finished<S> = { header: Option.none(), shared: [], failed: Option.none(), answered: [], outcomes: [] };
    const draft = yield* Effect.transposeOption(Option.map(known, planned));
    const mode = Option.match(
        Option.flatMap(draft, ({ sendable }) => Array.head(sendable)),
        { onNone: () => windowed, onSome: Struct.get('windowed') },
    );
    const waiting = Option.filter(
        Option.zipWith(known, draft, (survey, plan) => ({ survey, opened: Option.none<Schema.Json>(), plan, ran: idled })),
        ({ plan }) => plan.sendable.length === 0,
    );
    const first = yield* Option.match(waiting, { onSome: Effect.succeed, onNone: () => instanced(host, chosen, scratch, limit, mode, known, draft, planned) });
    const far: Plan<C, unknown> = { engine: first.plan.engine, sendable: first.plan.sendable.filter((item) => item.windowed !== mode), settled: [] };
    const relaunched = Option.filter(
        Option.liftPredicate(far, ({ sendable }) => sendable.length > 0),
        () => Option.isNone(first.ran.failed),
    );
    const later = yield* Option.match(relaunched, {
        onNone: () => Effect.succeed(idled),
        onSome: (held) =>
            Effect.map(
                instanced(host, chosen, scratch, limit, !mode, Option.some(first.survey), Option.some(held), () => Effect.succeed(held)),
                Struct.get('ran'),
            ),
    });
    const skipped = yield* Option.match(first.ran.failed, { onNone: () => Effect.succeed([]), onSome: (failed) => notRun(far.sendable, `not run, ${failed}`) });
    const ran = [first.ran, later, { ...idled, outcomes: skipped }];
    const written = yield* writtenIn(host, first.plan.sendable);
    return {
        survey: first.survey,
        opened: first.opened,
        plan: first.plan,
        header: Option.firstSomeOf(ran.map(Struct.get('header'))),
        shared: Array.dedupe(ran.flatMap(Struct.get('shared'))),
        failed: Option.liftPredicate(Array.getSomes(ran.map(Struct.get('failed'))).join('; '), String.isNonEmpty),
        answered: ran.flatMap(Struct.get('answered')),
        outcomes: [...first.plan.settled, ...ran.flatMap(Struct.get('outcomes'))].map((row) => ({ ...row, written: written.get(row.name) ?? [] })),
    };
});
const rerun = <S extends Surveyed, C extends Cased>(host: Session<S, C>, rebuilt: NonNullable<Harness<S, C>['rebuilt']>, chosen: Installed, scratch: string, limit: Duration.Duration, engine: unknown, requested: readonly Rebuilt<C>[]) =>
    Array.match(requested, {
        onEmpty: () => Effect.succeed([]),
        onNonEmpty: Effect.fn('rerun')(function* (calls: Array.NonEmptyReadonlyArray<Rebuilt<C>>) {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const folders = Array.map(calls, (_, index) => path.join(scratch, 'rebuilt', `${index}`));
            const made = yield* Effect.forEach(Array.zip(calls, folders), ([call, folder]) => Effect.andThen(fs.makeDirectory(folder, { recursive: true }), rebuilt(call, folder)));
            const sendable = Array.zipWith(calls, made, (call, { hosted, map }): Prepared<C> => ({ hosted, fn: Option.some(call.fn), map, presses: [], copies: [], windowed: call.windowed }));
            return (yield* execute(host, chosen, scratch, limit, Array.headNonEmpty(sendable).windowed, Option.none(), () => Effect.succeed({ engine, sendable, settled: [] }))).outcomes;
        }),
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Batch, type Cased, type Decoding, execute, filesIn, Harness, type Installed, type Instance, inside, installation, installations, LABEL, type Live, type Plan, type Prepared, type Rebuilt, rerun, type Services, type Session, type Surveyed, scripted };
