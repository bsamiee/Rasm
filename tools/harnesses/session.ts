import { createHash } from 'node:crypto';
import { type Installation, State } from '@rasm/automation/applications';
import type { Automation } from '@rasm/automation/automation';
import { Array, Data, type DateTime, Deferred, Duration, Effect, Equal, Equivalence, FileSystem, identity, Number, Option, Order, Path, type PlatformError, Predicate, Record, Ref, Result, Schema, Stream, String, SubscriptionRef } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import type { Row as Authored } from '#/cases';
import { HarnessError } from '#/error';
import { busy, crashed, described, dialogOf, type Element, type Exited, Launch, perform, pressed, type Row, read, type Spawned, settled, shown, spawned, stalled, starts, watch, watched } from '#/instance';
import { phase, Run, type Sighting, sample, sightings, stateOf, tagOf } from '#/log';
import { type Answered, Completion, type Dialog, type Hang, Outcome, outcome, record, type Written } from '#/results';
import type { Mapped } from '#/source';
import type { JsonObject } from '#/values';
import npm from './package.json' with { type: 'json' };

// --- [TYPES] ---------------------------------------------------------------------------

type Any = Schema.Codec<unknown, unknown>;
type Decoding<P> = Schema.Codec<P, unknown, never, unknown>;
type Services = Automation | ChildProcessSpawner.ChildProcessSpawner | FileSystem.FileSystem | Path.Path;
type Entry = ReturnType<typeof record<typeof Outcome>>['Type'];
type Digest = typeof Written.Type;
type Launched = Omit<Launch, 'app'>;
type Verdict = Data.TaggedEnum<{
    Exited: { readonly exit: Exited };
    Hung: { readonly index: Option.Option<number>; readonly hang: typeof Hang.Type };
    Dialog: { readonly index: Option.Option<number>; readonly dialog: typeof Dialog.Type };
    Lost: { readonly error: Error };
}>;
interface Surveyed {
    readonly host: Schema.Json;
}
interface Cased {
    readonly name: string;
    readonly folder: string;
}
interface Fields {
    readonly [key: PropertyKey]: Schema.Top & Schema.Decoder<unknown>;
}
interface Progress {
    readonly title: string;
    readonly buttons: readonly string[];
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
interface Records {
    readonly finished: ReadonlyMap<number, typeof Outcome.Type>;
    readonly running: Option.Option<number>;
}
interface Note {
    readonly index: Option.Option<number>;
    readonly element: string;
    readonly answered: typeof Answered.Type;
}
interface Live {
    readonly pid: number;
    readonly started: DateTime.Utc;
    readonly sighted: Option.Option<Sighting>;
    readonly launched: boolean;
    readonly chosen: Installed;
    readonly scratch: string;
    readonly verdict: Deferred.Deferred<Verdict>;
    readonly notes: Ref.Ref<readonly Note[]>;
    readonly inFlight: Ref.Ref<Option.Option<number>>;
    readonly presses: Ref.Ref<readonly (readonly string[])[]>;
    readonly state: SubscriptionRef.SubscriptionRef<typeof State.Type>;
}
interface Prepared<C extends Cased> {
    readonly hosted: C;
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
interface Ran<S> extends Records {
    readonly reply: Result.Result<Header<S>, Error>;
    readonly notes: readonly Note[];
    readonly verdict: Option.Option<Verdict>;
}
interface Finished<S> {
    readonly header: Option.Option<Header<S>>;
    readonly shared: readonly number[];
    readonly failed: Option.Option<string>;
    readonly answered: readonly (typeof Answered.Type)[];
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
    readonly shown: readonly (keyof F & string)[];
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
}
type Session<S extends Surveyed, C extends Cased> = Pick<Host<S, C>, 'progress' | 'fields' | 'shown' | 'launch' | 'opened' | 'send' | 'quit' | 'canonical'>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const VERSIONS = new Intl.Collator('en', { numeric: true });
const LABEL = npm.name.replaceAll(/\W+/gu, '-').replace(/^-/u, '');

// --- [MODELS] --------------------------------------------------------------------------

const Verdict = Data.taggedEnum<Verdict>();
const same = Schema.toEquivalence(Schema.Json);
const labelled = Array.makeEquivalence(Equivalence.String);
const newest = Order.flip(
    Order.mapInput(
        Order.make((left: string, right: string) => Number.sign(VERSIONS.compare(left, right))),
        ({ version }: Installed) => version,
    ),
);

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
    return Effect.fromOption(Array.match(chosen, { onEmpty: Option.none, onNonEmpty: Option.some }), () => HarnessError.unavailable(`${subject} ${prefixes.join(', ')}`)(`installed versions ${hosts.map(({ version }) => version).join(', ')}`));
};

// --- [VERDICTS]
const verdictText = (limit: Duration.Duration, pid: number) =>
    Verdict.$match({
        Exited: ({ exit }) => `instance ${pid} ${described(exit)}`,
        Hung: ({ hang }) => `instance ${hang.pid} hung ${hang.cpu} past ${Duration.format(limit)}, sample ${hang.sample}, left running`,
        Dialog: ({ dialog }) => shown(dialog),
        Lost: ({ error }) => error.message,
    });
const ended = (live: Live, verdict: Verdict) => Effect.asVoid(Deferred.succeed(live.verdict, verdict));
const fault = (limit: Duration.Duration, pid: number, verdict: Option.Option<Verdict>, message: string) => {
    const completion = { _tag: 'Failed' as const, reason: Option.match(verdict, { onNone: () => message, onSome: verdictText(limit, pid) }) };
    return Option.match(verdict, {
        onNone: () => Effect.succeed({ completion }),
        onSome: Verdict.$match({
            Exited: ({ exit }) => Effect.map(crashed(exit), (crash) => ({ completion, crashed: crash })),
            Hung: ({ hang }) => Effect.succeed({ completion, hung: hang }),
            Dialog: ({ dialog }) => Effect.succeed({ completion, dialog }),
            Lost: () => Effect.succeed({ completion }),
        }),
    });
};

// --- [RECORDS]
const startedBefore = (rows: readonly Entry[], at: number) =>
    Option.map(
        Array.findLast(rows.slice(0, at), (row): row is Extract<Entry, { readonly _tag: 'Started' }> => row._tag === 'Started'),
        ({ index }) => index,
    );
const filesIn = Effect.fn('filesIn')(function* (folder: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const isFile = (file: string) => Effect.map(fs.stat(file), ({ type }) => type === 'File');
    return yield* Effect.filter(
        (yield* fs.readDirectory(folder, { recursive: true })).map((file) => path.join(folder, file)),
        isFile,
    );
});
const recordsOf = (Line: Schema.Codec<Entry, string, never, unknown>) =>
    Effect.fn('recordsOf')(function* (file: string) {
        const fs = yield* FileSystem.FileSystem;
        const rows = yield* Effect.forEach(
            Array.fromIterable(String.linesIterator(yield* fs.readFileString(file))).filter((line) => line.trim() !== ''),
            (line) => Schema.decodeEffect(Line)(line),
        );
        const finished = new Map(rows.flatMap((row, at) => (row._tag === 'Finished' ? Option.toArray(Option.map(startedBefore(rows, at), (index) => [index, row.outcome] as const)) : [])));
        return { finished, running: Option.filter(startedBefore(rows, rows.length), (index) => !finished.has(index)) } satisfies Records;
    });
const mapped =
    (map: readonly Mapped[]) =>
    (row: typeof Outcome.Type): typeof Outcome.Type => {
        const at = (frame: { readonly line: number; readonly file?: string }) => (frame.file === undefined ? Option.getOrElse(Array.get(map, frame.line - 1), () => ({ line: frame.line })) : frame);
        return row.completion._tag === 'Thrown'
            ? { ...row, completion: { ...row.completion, ...(row.completion.line === undefined ? {} : at({ line: row.completion.line, ...(row.completion.file === undefined ? {} : { file: row.completion.file }) })), ...(row.completion.stack === undefined ? {} : { stack: row.completion.stack.map(at) }) } }
            : row;
    };
const writtenIn = Effect.fnUntraced(function* (host: Pick<Host, 'canonical'>, sendable: readonly Prepared<Cased>[]) {
    const fs = yield* FileSystem.FileSystem;
    const digest = Effect.fnUntraced(function* (file: string) {
        const bytes = yield* fs.readFile(file);
        const parts: readonly (string | Uint8Array)[] = host.canonical === undefined ? [bytes] : yield* host.canonical(file, bytes);
        return { file, size: bytes.byteLength, sha256: parts.reduce((hash, part) => hash.update(part), createHash('sha256')).digest('hex') } satisfies Digest;
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
const requiring = (passing: readonly Progress[], pid: number) => (element: Element) => {
    const { title, text, buttons } = dialogOf(element, pid);
    return element.requiresAnswer && !passing.some((named) => [title, ...text].includes(named.title) && labelled(named.buttons, buttons));
};
const holding =
    (passing: readonly Progress[]) =>
    ({ pid, state }: Row): Option.Option<string> =>
        State.match(state, {
            Launching: () => Option.none(),
            Responding: ({ elements }) => Option.map(Array.findFirst(elements, requiring(passing, pid)), (element) => shown(dialogOf(element, pid))),
            Unresponsive: () => Option.some(`instance ${pid} ${described(state)}, left running`),
            Failed: () => Option.none(),
            Exited: () => Option.none(),
        });
const unanswered = (notes: readonly Note[], required: (element: Element) => boolean) => (element: Element) => required(element) && notes.every(({ element: id }) => id !== element.id);
const sightingOf = (pid: number, state: typeof State.Type) => ({
    detail: described(state),
    ...(State.guards.Responding(state)
        ? Array.match(
              state.elements.filter(({ requiresAnswer }) => requiresAnswer),
              { onEmpty: () => ({}), onNonEmpty: (held) => ({ dialogs: held.map((element) => dialogOf(element, pid)) }) },
          )
        : {}),
});
const within = (live: Live) => Effect.provideService(Run.Instance, Option.some(live));
const sighted = (live: Live, state: typeof State.Type, message: string) => {
    const levels = { Launching: 'Info', Responding: 'Info', Dialog: 'Warn', Unresponsive: 'Warn', Failed: 'Warn', Exited: 'Info' } as const;
    const logged = Effect.logWithLevel(levels[stateOf(state)])(message).pipe(Effect.annotateLogs(sightingOf(live.pid, state)), within(live));
    return Option.match(live.sighted, { onNone: () => logged, onSome: (into) => into(logged) });
};
const watcher = (live: Live, passing: readonly Progress[]) =>
    Effect.fn('watcher')(function* (row: Row) {
        const ours = row.identifier === live.chosen.identifier && row.pid === live.pid;
        const before = ours ? yield* SubscriptionRef.getAndSet(live.state, row.state) : row.state;
        yield* same(sightingOf(row.pid, before), sightingOf(row.pid, row.state)) ? Effect.void : sighted(live, row.state, `instance ${stateOf(row.state)}`);
        const answer = Effect.fnUntraced(function* (element: Element) {
            const [at, presses, notes] = yield* Effect.all([Ref.get(live.inFlight), Ref.get(live.presses), Ref.get(live.notes)]);
            const used = notes.filter(({ index }) => Equal.equals(index, at)).length;
            const wanted = Option.flatMap(
                Option.filter(at, () => ours),
                (index) => Option.flatMap(Array.get(presses, index), Array.get(used)),
            );
            const control = Option.zipWith(
                wanted,
                Option.flatMap(wanted, (label) => pressed(element, label)),
                (label, { id }) => ({ label, id }),
            );
            yield* Option.match(control, {
                onNone: () => ended(live, Verdict.Dialog({ index: at, dialog: dialogOf(element, row.pid) })),
                onSome: ({ label, id }) => {
                    const answered = { ...(element.title === undefined ? {} : { title: element.title }), text: element.detail?.text ?? [], pressed: label };
                    return Effect.catch(Effect.andThen(Effect.all([Ref.update(live.notes, Array.append({ index: at, element: element.id, answered })), Effect.annotateLogs(Effect.logInfo('dialog answered'), { dialog: answered })]), perform(id)), (error) => ended(live, Verdict.Lost({ error })));
                },
            });
        });
        yield* State.match(row.state, {
            Launching: () => Effect.void,
            Responding: ({ elements }) => Effect.flatMap(Ref.get(live.notes), (notes) => Effect.forEach(elements.filter(unanswered(notes, requiring(passing, row.pid))), answer, { discard: true })),
            Unresponsive: () => Effect.void,
            Failed: ({ reason }) => (ours ? ended(live, Verdict.Lost({ error: HarnessError.unavailable(`process ${row.pid}`)(reason) })) : Effect.void),
            Exited: (exit) => (ours && settled(exit) ? ended(live, Verdict.Exited({ exit })) : Effect.void),
        });
    });
const advanced = (live: Live, names: readonly string[]) => (running: Option.Option<number>) =>
    Effect.flatMap(Ref.getAndSet(live.inFlight, running), (before) =>
        Option.match(
            Option.filter(
                Option.flatMap(running, (index) => Array.get(names, index)),
                () => !Equal.equals(before, running),
            ),
            { onNone: () => Effect.void, onSome: (name) => Effect.annotateLogs(Effect.logInfo('case started'), { case: name }) },
        ),
    );
const progress = Effect.fnUntraced(function* (live: Live, records: string, names: readonly string[], Recorded: Schema.Codec<Entry, string, never, unknown>, limit: Duration.Duration) {
    yield* watched(records).pipe(
        Stream.mapEffect(() => Effect.flatMap(recordsOf(Recorded)(records), ({ running }) => advanced(live, names)(running))),
        Stream.debounce(limit),
        Stream.runHead,
    );
    const index = yield* Ref.get(live.inFlight);
    const [cpu, file] = yield* Effect.all([busy(live.pid), sample(live)]);
    yield* ended(live, Verdict.Hung({ index, hang: { cpu, sample: file, frames: yield* stalled(file), pid: live.pid } }));
});

// --- [SESSION]
const acquired = Effect.fn('acquired')(function* (host: Pick<Host, 'progress' | 'launch'>, chosen: Installed, root: string, limit: Duration.Duration, windowed: boolean) {
    const fs = yield* FileSystem.FileSystem;
    const refused = HarnessError.unavailable(chosen.name);
    const passing = host.progress ?? [];
    const dialog = (row: Row) => (State.guards.Unresponsive(row.state) ? Option.none<string>() : holding(passing)(row));
    const { running, scratch, rows, instance } = yield* Effect.gen(function* () {
        const before = (yield* read(chosen.watched)).filter((row) => row.identifier === chosen.identifier);
        yield* Array.match(Array.getSomes(before.map(dialog)), { onEmpty: () => Effect.void, onNonEmpty: (held) => refused(held.join('; ')) });
        const folder = yield* fs.makeTempDirectoryScoped({ directory: root, prefix: 'instance-' });
        const spec = yield* host.launch(chosen, folder, windowed);
        const { rows: watching, ...opened } = yield* Predicate.isTagged('Spawned')(spec) ? spawned(chosen.identifier, spec) : Effect.map(Launch({ app: chosen.app, fresh: true, ...spec }), (held) => ({ ...held, rows: watch(chosen.watched, held.pid) }));
        yield* Option.match(dialog(opened), { onNone: () => Effect.void, onSome: refused });
        yield* settled(opened.state) ? refused(`instance ${opened.pid} ${described(opened.state)}`) : Effect.void;
        return { running: before, scratch: folder, rows: watching, instance: opened };
    }).pipe(phase('launch'));
    const shared = running.filter(({ pid, state }) => pid !== instance.pid && !State.guards.Exited(state)).map(({ pid }) => pid);
    const launched = running.every(({ pid }) => pid !== instance.pid);
    const started = yield* Effect.flatMap(starts(['-p', `${instance.pid}`]), (held) => Effect.fromOption(Option.fromUndefinedOr(held.get(instance.pid)), () => HarnessError.unavailable(`process ${instance.pid}`)('ps reports no start time')));
    const live: Live = {
        pid: instance.pid,
        started,
        sighted: launched ? Option.some(yield* sightings({ pid: instance.pid, started })) : Option.none(),
        launched,
        chosen,
        scratch,
        verdict: yield* Deferred.make<Verdict>(),
        notes: yield* Ref.make<readonly Note[]>([]),
        inFlight: yield* Ref.make(Option.none<number>()),
        presses: yield* Ref.make<readonly (readonly string[])[]>([]),
        state: yield* SubscriptionRef.make<typeof State.Type>(instance.state),
    };
    yield* Effect.annotateLogs(sighted(live, instance.state, launched ? 'instance launched' : 'instance attached'), { shared });
    yield* Effect.forkScoped(
        Stream.runForEach(rows, watcher(live, passing)).pipe(
            Effect.andThen(refused(`instance ${instance.pid} watch ended`)),
            Effect.catch((error) => ended(live, Verdict.Lost({ error }))),
        ),
    );
    yield* Effect.raceFirst(
        SubscriptionRef.changes(live.state).pipe(
            Stream.filter((state) => State.guards.Responding(state) && !state.answering),
            Stream.runHead,
        ),
        Effect.flatMap(Deferred.await(live.verdict), (over) => refused(verdictText(limit, instance.pid)(over))),
    ).pipe(Effect.timeoutOrElse({ duration: limit, orElse: () => Effect.flatMap(Effect.all([sample(live), SubscriptionRef.get(live.state)]), ([file, state]) => refused(`instance ${instance.pid} ${described(state)} past ${Duration.format(limit)}, sample ${file}, left running`)) }), phase('ready'), within(live));
    return { live, shared };
});
const openedIn = (host: Pick<Host, 'opened'>, live: Live, limit: Duration.Duration) =>
    host.opened === undefined
        ? Effect.succeedNone
        : Effect.asSome(host.opened(live)).pipe(
              Effect.timeoutOrElse({
                  duration: limit,
                  orElse: () => Effect.flatMap(sample(live), (file) => HarnessError.unavailable(live.chosen.name)(`instance ${live.pid} opened nothing past ${Duration.format(limit)}, sample ${file}, left running`)),
              }),
              phase('open'),
              within(live),
          );
const batchIn = Effect.fn('batchIn')(function* <S extends Surveyed, C extends Cased>(host: Pick<Host<S, C>, 'fields' | 'send'>, live: Live, batch: (records: string) => Batch<S, C>, presses: readonly (readonly string[])[], limit: Duration.Duration) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const Recorded: Schema.Codec<Entry, string, never, unknown> = record(outcome(host.fields));
    const records = path.join(yield* fs.makeTempDirectoryScoped({ directory: live.scratch, prefix: 'batch-' }), 'records.jsonl');
    const sent = batch(records);
    yield* fs.writeFileString(records, '');
    yield* Effect.all([Ref.set(live.presses, presses), Ref.set(live.notes, []), Ref.set(live.inFlight, Option.none())]);
    yield* Effect.forkScoped(
        Effect.catch(
            progress(
                live,
                records,
                sent.cases.map(({ name }) => name),
                Recorded,
                limit,
            ),
            (error) => ended(live, Verdict.Lost({ error })),
        ),
    );
    const reply = yield* Effect.result(
        Effect.raceFirst(
            host.send(live, sent),
            Effect.flatMap(Deferred.await(live.verdict), (over) => HarnessError.unavailable(`process ${live.pid}`)(verdictText(limit, live.pid)(over))),
        ),
    );
    const [held, notes, verdict] = yield* Effect.all([recordsOf(Recorded)(records), Ref.get(live.notes), Effect.flatMap(Deferred.poll(live.verdict), Effect.transposeOption)]);
    return { reply, ...held, notes, verdict } satisfies Ran<S>;
}, Effect.scoped);
const surveyIn = Effect.fn('surveyIn')(function* <S extends Surveyed, C extends Cased>(host: Pick<Host<S, C>, 'fields' | 'send'>, live: Live, opened: Option.Option<Schema.Json>, limit: Duration.Duration) {
    const { reply } = yield* batchIn(host, live, (records) => ({ records, label: LABEL, survey: Option.none(), opened, cases: [] }), [], limit);
    const header = yield* Effect.fromResult(reply);
    return yield* Effect.fromOption(Option.fromUndefinedOr(header.survey), () => HarnessError.unavailable(`process ${live.pid}`)('survey reply holds no survey'));
}, phase('survey'));
const reported = (keys: readonly string[]) => (row: typeof Outcome.Type) => {
    const passed = Completion.guards.Ran(row.completion) || Completion.guards.Exited(row.completion);
    const facts = {
        case: row.name,
        completion: row.completion._tag,
        ...Completion.match(row.completion, { Ran: () => ({}), Exited: () => ({}), Thrown: ({ message, file, line }) => ({ error: message, ...Record.filter({ file, line }, Predicate.isNotUndefined) }), Failed: ({ reason }) => ({ error: reason }) }),
        ...Record.filter({ answered: row.answered, leaked: row.leaked, ...Record.fromIterableWith(keys, (key) => [key, row[key]] as const) }, (items) => Array.isArray(items) && items.length > 0),
        ...Record.filter({ crashed: row.crashed, hung: row.hung, dialog: row.dialog }, Predicate.isNotUndefined),
    };
    const state = Option.firstSomeOf([Option.as(Option.fromUndefinedOr(row.hung), 'Unresponsive'), Option.as(Option.fromUndefinedOr(row.dialog), 'Dialog'), Option.as(Option.fromUndefinedOr(row.crashed), 'Exited')]);
    const failed = { tag: row.completion._tag, ...Option.match(state, { onNone: () => ({}), onSome: (held) => ({ state: held }) }) };
    return passed ? Effect.annotateLogs(Effect.logInfo(`case ${row.completion._tag}`), facts) : Effect.annotateLogs(Effect.logError(`case ${row.completion._tag}`), { ...facts, ...failed });
};
const finishedIn = Effect.fnUntraced(function* <S extends Surveyed, C extends Cased>(host: Session<S, C>, live: Live, survey: S, opened: Option.Option<Schema.Json>, rest: readonly Prepared<C>[], limit: Duration.Duration) {
    const ran = yield* batchIn(
        host,
        live,
        (records) => ({ records, label: LABEL, survey: Option.some(survey), opened, cases: rest.map(({ hosted }) => hosted) }),
        rest.map(({ presses }) => presses),
        limit,
    );
    const on = (index: number) => ran.notes.filter(({ index: at }) => Option.contains(at, index)).map(({ answered }) => answered);
    const kept = Array.getSomes(rest.map(({ map }, index) => Option.map(Option.fromUndefinedOr(ran.finished.get(index)), (row) => ({ ...mapped(map)(row), answered: [...row.answered, ...on(index)] }))));
    const stop = Option.getOrElse(ran.running, () => ran.finished.size);
    const reason = Option.match(ran.verdict, { onNone: () => 'not run, batch ended before the case', onSome: (over) => `not run, ${verdictText(limit, live.pid)(over)}` });
    const stopped = Option.zipWith(Result.getFailure(ran.reply), Array.get(rest, stop), (error, { hosted }) => ({ name: hosted.name, message: error.message }));
    const interrupted = yield* Effect.transposeOption(Option.map(stopped, ({ name, message }) => Effect.map(fault(limit, live.pid, ran.verdict, message), (parts) => ({ name, answered: on(stop), ...parts }))));
    const unsent = (Result.isFailure(ran.reply) ? rest.slice(stop + 1) : []).map(({ hosted }) => ({ name: hosted.name, completion: { _tag: 'Failed' as const, reason } }));
    const decoded = (row: typeof Outcome.Encoded) => Effect.orDie(Schema.decodeEffect(Outcome)(row));
    const [halted, skipped] = yield* Effect.all([Effect.forEach(Option.toArray(interrupted), decoded), Effect.forEach(unsent, decoded)]);
    yield* Effect.forEach([...kept, ...halted], reported(host.shown), { discard: true });
    yield* Effect.forEach(skipped, ({ name }) => Effect.annotateLogs(Effect.logWarning('case not run'), { case: name, error: reason }), { discard: true });
    const failure = Option.firstSomeOf([Option.map(ran.verdict, (over) => ({ tag: over._tag, error: verdictText(limit, live.pid)(over) })), Option.map(Result.getFailure(ran.reply), (error) => ({ tag: tagOf(error), error: error.message }))]);
    yield* Option.match(failure, { onNone: () => Effect.void, onSome: (held) => Effect.annotateLogs(Effect.logError('batch failed'), Option.isSome(interrupted) ? { error: held.error } : held) });
    return { ran, outcomes: [...kept, ...halted, ...skipped] };
}, phase('batch'));
const quitted = Effect.fn('quitted')(
    function* (host: Pick<Host, 'quit'>, live: Live, limit: Duration.Duration) {
        const refused = yield* Ref.make(Option.none<Error>());
        yield* Effect.forkScoped(Effect.catch(host.quit(live), (error) => Ref.set(refused, Option.some(error))));
        const verdict = yield* Effect.timeoutOption(Deferred.await(live.verdict), limit);
        const quit = Option.match(yield* Ref.get(refused), { onNone: () => 'quit sent', onSome: (error) => `quit refused: ${error.message}` });
        const clean = (over: Verdict) => Verdict.$is('Exited')(over) && over.exit.signal === undefined && over.exit.report === undefined && (over.exit.status ?? 0) === 0;
        const left = Option.match(verdict, {
            onNone: () => Option.some(`${quit}, instance ${live.pid} running past ${Duration.format(limit)}, left running`),
            onSome: (over) => Option.liftPredicate(`${quit}, ${verdictText(limit, live.pid)(over)}`, () => !clean(over)),
        });
        yield* Option.match(left, { onNone: () => Effect.void, onSome: (error) => Effect.annotateLogs(Effect.logWarning('quit unclean'), { tag: Option.match(verdict, { onNone: () => 'Running', onSome: ({ _tag }) => _tag }), error }) });
        return left;
    },
    Effect.scoped,
    phase('quit'),
);
const ranIn = Effect.fnUntraced(function* <S extends Surveyed, C extends Cased>(host: Session<S, C>, live: Live, survey: S, opened: Option.Option<Schema.Json>, rest: readonly Prepared<C>[], limit: Duration.Duration) {
    const { ran, outcomes } = yield* within(live)(finishedIn(host, live, survey, opened, rest, limit));
    const failed = yield* Option.match(ran.verdict, {
        onSome: (over) => Effect.succeedSome(verdictText(limit, live.pid)(over)),
        onNone: () => Option.match(Result.getFailure(ran.reply), { onSome: (error) => Effect.succeedSome(error.message), onNone: () => (live.launched ? within(live)(quitted(host, live, limit)) : Effect.succeedNone) }),
    });
    return { header: Result.getSuccess(ran.reply), failed, answered: ran.notes.filter(({ index }) => Option.isNone(index)).map(({ answered }) => answered), outcomes };
});
const execute = Effect.fn('execute')(function* <S extends Surveyed, C extends Cased, L extends Plan<C, unknown>>(
    host: Session<S, C>,
    chosen: Installed,
    scratch: string,
    limit: Duration.Duration,
    windowed: boolean,
    known: Option.Option<S>,
    planned: (survey: S) => Effect.Effect<L, Error, FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner>,
) {
    const idle: Finished<S> = { header: Option.none(), shared: [], failed: Option.none(), answered: [], outcomes: [] };
    const draft = yield* Effect.transposeOption(Option.map(known, planned));
    const waiting = Option.filter(
        Option.zipWith(known, draft, (survey, plan): Done<S, C, unknown, L> => ({ ...idle, survey, opened: Option.none(), plan })),
        ({ plan }) => plan.sendable.length === 0,
    );
    const launched = Effect.fnUntraced(function* () {
        const mode = Option.match(
            Option.flatMap(draft, ({ sendable }) => Array.head(sendable)),
            { onNone: () => windowed, onSome: (item) => item.windowed },
        );
        const { live, shared } = yield* acquired(host, chosen, scratch, limit, mode);
        const opened = yield* openedIn(host, live, limit);
        const confirmed = Option.filter(known, (held) => Option.match(opened, { onNone: () => true, onSome: (state) => same(held.host, state) }));
        const survey = yield* Option.match(confirmed, { onSome: Effect.succeed, onNone: () => within(live)(surveyIn(host, live, opened, limit)) });
        const plan = yield* Option.match(
            Option.filter(draft, () => Option.isSome(confirmed)),
            { onSome: Effect.succeed, onNone: () => planned(survey) },
        );
        const nearby = ({ windowed: held }: Prepared<C>) => held === mode;
        const [near, far] = Array.partition(plan.sendable, Result.liftPredicate(nearby, identity));
        const first: Finished<S> = { ...(yield* ranIn(host, live, survey, opened, near, limit)), shared };
        const unrun = (failed: string) =>
            Effect.map(
                Effect.forEach(far, ({ hosted }) => Effect.orDie(Schema.decodeEffect(Outcome)({ name: hosted.name, completion: { _tag: 'Failed', reason: `not run, ${failed}` } }))),
                (outcomes) => ({ ...idle, outcomes }),
            );
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
            Effect.map(rebuilt(call, folder), ({ hosted, map }): Prepared<C> => ({ hosted, map, presses: [], copies: [], windowed: call.windowed })),
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

export {
    type Any,
    type Batch,
    type Cased,
    type Decoding,
    type Digest,
    type Done,
    type Entry,
    execute,
    type Fields,
    filesIn,
    type Header,
    type Host,
    type Installed,
    inside,
    installation,
    installations,
    type Launched,
    type Live,
    type Plan,
    type Prepared,
    type Rebuilt,
    type Records,
    rerun,
    type Services,
    type Session,
    type Surveyed,
};
