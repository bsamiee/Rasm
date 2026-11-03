import { State } from '@rasm/automation/applications';
import type { Automation } from '@rasm/automation/automation';
import { Array, Data, Deferred, Duration, Effect, Equal, FileSystem, Number, Option, Order, Path, Predicate, type Record, Ref, Result, Schema, Stream, String, SubscriptionRef } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import type { Batch as Authored } from '#/cases';
import { HarnessError } from '#/error';
import { busy, crashed, described, dialogOf, type Element, type Exited, Launch, perform, pressed, type Row, read, type Spawned, sampled, settled, shown, spawned, watch } from '#/instance';
import { type Answered, type Dialog, type Hang, Outcome } from '#/results';
import type { Mapped } from '#/source';

// --- [TYPES] ---------------------------------------------------------------------------

type Any = Schema.Codec<unknown, unknown>;
type Decoding<P> = Schema.Codec<P, unknown, never, unknown>;
type Services = Automation | ChildProcessSpawner.ChildProcessSpawner | FileSystem.FileSystem | Path.Path;
type Entry = { readonly _tag: 'Started'; readonly index: number } | { readonly _tag: 'Finished'; readonly outcome: typeof Outcome.Type };
type Verdict = Data.TaggedEnum<{
    Exited: { readonly exit: Exited };
    Hung: { readonly index: Option.Option<number>; readonly hang: typeof Hang.Type };
    Dialog: { readonly index: Option.Option<number>; readonly dialog: typeof Dialog.Type };
    Lost: { readonly error: Error };
}>;
interface Installed {
    readonly identifier: string;
    readonly name: string;
    readonly app: string;
    readonly version: string;
    readonly watched: Array.NonEmptyReadonlyArray<string>;
}
interface Launched {
    readonly fresh?: boolean;
    readonly documents?: readonly string[];
    readonly arguments?: readonly string[];
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
    readonly launched: boolean;
    readonly chosen: Installed;
    readonly scratch: string;
    readonly verdict: Deferred.Deferred<Verdict>;
    readonly notes: Ref.Ref<readonly Note[]>;
    readonly inFlight: Ref.Ref<Option.Option<number>>;
    readonly presses: Ref.Ref<readonly (readonly string[])[]>;
    readonly state: SubscriptionRef.SubscriptionRef<typeof State.Type>;
}
interface Prepared<C> {
    readonly hosted: C;
    readonly name: string;
    readonly folder: string;
    readonly map: readonly Mapped[];
    readonly presses: readonly string[];
    readonly copies: readonly string[];
}
interface Plan<C, E> {
    readonly engine: E;
    readonly sendable: readonly Prepared<C>[];
    readonly settled: readonly (typeof Outcome.Type)[];
}
interface Digest {
    readonly file: string;
    readonly size: number;
    readonly sha256: string;
}
interface Rebuilt<C> {
    readonly hosted: C;
    readonly fn: string;
    readonly args: readonly Schema.Json[];
    readonly module: string;
}
interface Ran<S> extends Records {
    readonly reply: Result.Result<Header<S>, Error>;
    readonly notes: readonly Note[];
    readonly verdict: Option.Option<Verdict>;
}
interface Done<S, C, E, P extends Plan<C, E>> {
    readonly survey: S;
    readonly opened: Option.Option<Schema.Json>;
    readonly plan: P;
    readonly header: Option.Option<Header<S>>;
    readonly shared: readonly number[];
    readonly failed: Option.Option<string>;
    readonly answered: readonly (typeof Answered.Type)[];
    readonly outcomes: readonly (typeof Outcome.Type)[];
}
interface Host<S extends Any, C extends Any, E extends Any, P = unknown> {
    readonly name: string;
    readonly Engine: E;
    readonly survey: { readonly question: string; readonly Schema: S; readonly host: (survey: S['Type']) => Option.Option<Schema.Json> };
    readonly Case: C;
    readonly shown: readonly string[];
    readonly engine: (scripts: readonly string[]) => Result.Result<E['Type'], HarnessError>;
    readonly precondition: (survey: S['Type']) => Decoding<P>;
    readonly placed: (decoded: P, folder: string, product: string) => Effect.Effect<{ readonly precondition: unknown; readonly copies: readonly string[] }, Error, FileSystem.FileSystem | Path.Path>;
    readonly refusal: (engine: E['Type'], hosted: C['Type'], binary: boolean) => Option.Option<string>;
    readonly installed: Effect.Effect<readonly Installed[], Error, Services>;
    readonly launch: (chosen: Installed, scratch: string, authored: typeof Authored.Type) => Effect.Effect<Launched | Spawned, Error, Services>;
    readonly opened: (live: Live) => Effect.Effect<Option.Option<Schema.Json>, Error, Services>;
    readonly send: (live: Live, batch: Batch<S['Type'], C['Type']>) => Effect.Effect<Header<S['Type']>, Error, Services>;
    readonly records: (live: Live, file: string, reply: Option.Option<Header<S['Type']>>) => Effect.Effect<Records, Error, FileSystem.FileSystem | Path.Path>;
    readonly quit: (live: Live) => Effect.Effect<void, Error, Services>;
    readonly digest: (file: string) => Effect.Effect<Digest, Error, Services>;
    readonly compare: { readonly _tag: 'Imported' } | { readonly _tag: 'Hosted'; readonly run: (chosen: Installed, calls: readonly Rebuilt<C['Type']>[], scratch: string, limit: Duration.Duration, authored: typeof Authored.Type) => Effect.Effect<readonly (typeof Outcome.Type)[], Error, Services> };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const VERSIONS = new Intl.Collator('en', { numeric: true });

// --- [MODELS] --------------------------------------------------------------------------

const Verdict = Data.taggedEnum<Verdict>();
const newest = Order.flip(
    Order.mapInput(
        Order.make((left: string, right: string) => Number.sign(VERSIONS.compare(left, right))),
        ({ version }: Installed) => version,
    ),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INSTALLATIONS]
const installations = (rows: readonly Installed[], prefixes: readonly string[], subject: string) => {
    const hosts = Array.sort(rows, newest);
    const chosen = prefixes.length === 0 ? hosts : hosts.filter(({ version }) => prefixes.some((prefix) => version.startsWith(prefix)));
    return Effect.fromOption(Array.match(chosen, { onEmpty: Option.none, onNonEmpty: Option.some }), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: `installed versions ${hosts.map(({ version }) => version).join(', ')}` }, subject: `${subject} ${prefixes.join(', ')}` }));
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
const fault = (limit: Duration.Duration, verdict: Option.Option<Verdict>, message: string) =>
    Option.match(verdict, {
        onNone: () => Effect.succeed({ completion: { _tag: 'Failed' as const, reason: message } }),
        onSome: Verdict.$match({
            Exited: ({ exit }) => Effect.map(crashed(exit), (crash) => ({ completion: { _tag: 'Failed' as const, reason: described(exit) }, crashed: crash })),
            Hung: ({ hang }) => Effect.succeed({ completion: { _tag: 'Failed' as const, reason: `hung ${hang.cpu} past ${Duration.format(limit)}` }, hung: hang }),
            Dialog: ({ dialog }) => Effect.succeed({ completion: { _tag: 'Failed' as const, reason: shown(dialog) }, dialog }),
            Lost: ({ error }) => Effect.succeed({ completion: { _tag: 'Failed' as const, reason: error.message } }),
        }),
    });

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
const recorded = (Line: Schema.Codec<Entry, string, never, unknown>) =>
    Effect.fn('recorded')(function* (file: string) {
        const fs = yield* FileSystem.FileSystem;
        const rows = yield* Effect.forEach(
            Array.fromIterable(String.linesIterator(yield* fs.readFileString(file))).filter((line) => line.trim() !== ''),
            (line) => Schema.decodeEffect(Line)(line),
        );
        const finished = new Map(rows.flatMap((row, at) => (row._tag === 'Finished' ? Option.toArray(Option.map(startedBefore(rows, at), (index) => [index, row.outcome] as const)) : [])));
        return { finished, running: Option.filter(startedBefore(rows, rows.length), (index) => !finished.has(index)) };
    });
const mapped =
    (map: readonly Mapped[]) =>
    (row: typeof Outcome.Type): typeof Outcome.Type => {
        const at = (frame: { readonly line: number; readonly file?: string }) => (frame.file === undefined ? Option.getOrElse(Array.get(map, frame.line - 1), () => ({ line: frame.line })) : frame);
        return row.completion._tag === 'Thrown'
            ? { ...row, completion: { ...row.completion, ...(row.completion.line === undefined ? {} : at({ line: row.completion.line, ...(row.completion.file === undefined ? {} : { file: row.completion.file }) })), ...(row.completion.stack === undefined ? {} : { stack: row.completion.stack.map(at) }) } }
            : row;
    };
const writtenIn = Effect.fnUntraced(function* <S extends Any, C extends Any, E extends Any, P>(host: Host<S, C, E, P>, sendable: readonly Prepared<C['Type']>[]) {
    const copied = new Set((yield* Effect.forEach(Array.dedupe(sendable.flatMap(({ copies }) => copies)), host.digest)).map(({ sha256 }) => sha256));
    const digested = Effect.fnUntraced(function* ({ name, folder }: Prepared<C['Type']>) {
        return [name, (yield* Effect.forEach(yield* filesIn(folder), host.digest)).filter(({ sha256 }) => !copied.has(sha256))] as const;
    });
    return new Map(yield* Effect.forEach(sendable, digested));
});

// --- [WATCHER]
const holding = ({ pid, state }: Row): Option.Option<string> =>
    State.match(state, {
        Launching: () => Option.none(),
        Responding: ({ elements }) =>
            Option.map(
                Array.findFirst(elements, ({ requiresAnswer }) => requiresAnswer),
                (element) => shown(dialogOf(element, pid)),
            ),
        Unresponsive: () => Option.some(`instance ${pid} ${described(state)}, left running`),
        Failed: () => Option.none(),
        Exited: () => Option.none(),
    });
const unanswered = (notes: readonly Note[]) => (element: Element) => element.requiresAnswer && notes.every(({ element: id }) => id !== element.id);
const controlOf = (element: Element, label: string) => Option.map(pressed(element, label), ({ id }) => ({ label, id }));
const watcher = (live: Live) =>
    Effect.fn('watcher')(function* (row: Row) {
        const ours = row.identifier === live.chosen.identifier && row.pid === live.pid;
        yield* ours ? SubscriptionRef.set(live.state, row.state) : Effect.void;
        const answer = Effect.fnUntraced(function* (element: Element) {
            const [at, presses, notes] = yield* Effect.all([Ref.get(live.inFlight), Ref.get(live.presses), Ref.get(live.notes)]);
            const used = notes.filter(({ index }) => Equal.equals(index, at)).length;
            const wanted = Option.flatMap(
                Option.filter(at, () => ours),
                (index) => Option.flatMap(Array.get(presses, index), Array.get(used)),
            );
            yield* Option.match(
                Option.flatMap(wanted, (label) => controlOf(element, label)),
                {
                    onNone: () => ended(live, Verdict.Dialog({ index: at, dialog: dialogOf(element, row.pid) })),
                    onSome: ({ label, id }) =>
                        Ref.update(live.notes, Array.append({ index: at, element: element.id, answered: { ...(element.title === undefined ? {} : { title: element.title }), text: element.detail?.text ?? [], pressed: label } })).pipe(
                            Effect.andThen(perform(id)),
                            Effect.catch((error) => ended(live, Verdict.Lost({ error }))),
                        ),
                },
            );
        });
        yield* State.match(row.state, {
            Launching: () => Effect.void,
            Responding: ({ elements }) => Effect.flatMap(Ref.get(live.notes), (notes) => Effect.forEach(elements.filter(unanswered(notes)), answer, { discard: true })),
            Unresponsive: () => Effect.void,
            Failed: ({ reason }) => (ours ? ended(live, Verdict.Lost({ error: HarnessError.make({ reason: { _tag: 'Unavailable', because: reason }, subject: `process ${row.pid}` }) })) : Effect.void),
            Exited: (exit) => (ours && settled(exit) ? ended(live, Verdict.Exited({ exit })) : Effect.void),
        });
    });
const progress = Effect.fnUntraced(function* <S extends Any, C extends Any, E extends Any, P>(host: Host<S, C, E, P>, live: Live, records: string, folders: readonly string[], limit: Duration.Duration) {
    const fs = yield* FileSystem.FileSystem;
    yield* Stream.concat(
        Stream.make(undefined),
        Stream.map(fs.watch(records), () => undefined),
    ).pipe(
        Stream.mapEffect(() => Effect.flatMap(host.records(live, records, Option.none()), ({ running: at }) => Ref.set(live.inFlight, at))),
        Stream.switchMap(() => Stream.fromEffect(Effect.sleep(limit))),
        Stream.runHead,
    );
    const running = yield* Ref.get(live.inFlight);
    const folder = Option.getOrElse(
        Option.flatMap(running, (index) => Array.get(folders, index)),
        () => live.scratch,
    );
    const [cpu, sample] = yield* Effect.all([busy(live.pid), sampled(live.pid, `${folder}/sample.txt`)]);
    yield* ended(live, Verdict.Hung({ index: running, hang: { cpu, sample, pid: live.pid } }));
});

// --- [SESSION]
const acquired = Effect.fn('acquired')(function* <S extends Any, C extends Any, E extends Any, P>(host: Host<S, C, E, P>, chosen: Installed, scratch: string, limit: Duration.Duration, authored: typeof Authored.Type) {
    const path = yield* Path.Path;
    const refused = (because: string) => HarnessError.make({ reason: { _tag: 'Unavailable', because }, subject: chosen.name });
    const running = (yield* read(chosen.watched)).filter((row) => row.identifier === chosen.identifier);
    const dialog = (row: Row) => (State.guards.Unresponsive(row.state) ? Option.none<string>() : holding(row));
    yield* Array.match(Array.getSomes(running.map(dialog)), { onEmpty: () => Effect.void, onNonEmpty: (held) => refused(held.join('; ')) });
    const spec = yield* host.launch(chosen, scratch, authored);
    const { rows, ...instance } = yield* Predicate.isTagged('Spawned')(spec) ? spawned(chosen.identifier, spec) : Effect.map(Launch({ app: chosen.app, fresh: true, ...spec }), (opened) => ({ ...opened, rows: watch(chosen.watched, opened.pid) }));
    const shared = running.filter(({ pid, state }) => pid !== instance.pid && !State.guards.Exited(state)).map(({ pid }) => pid);
    yield* Option.match(dialog(instance), { onNone: () => Effect.void, onSome: refused });
    yield* settled(instance.state) ? refused(`instance ${instance.pid} ${described(instance.state)}`) : Effect.void;
    const live: Live = {
        pid: instance.pid,
        launched: instance.launched,
        chosen,
        scratch,
        verdict: yield* Deferred.make<Verdict>(),
        notes: yield* Ref.make<readonly Note[]>([]),
        inFlight: yield* Ref.make(Option.none<number>()),
        presses: yield* Ref.make<readonly (readonly string[])[]>([]),
        state: yield* SubscriptionRef.make<typeof State.Type>(instance.state),
    };
    yield* Effect.forkScoped(
        Stream.runForEach(rows, watcher(live)).pipe(
            Effect.andThen(refused(`instance ${instance.pid} watch ended`)),
            Effect.catch((error) => ended(live, Verdict.Lost({ error }))),
        ),
    );
    yield* Effect.raceFirst(
        Stream.concat(Stream.fromEffect(SubscriptionRef.get(live.state)), SubscriptionRef.changes(live.state)).pipe(
            Stream.filter((state) => State.guards.Responding(state) && state.elements.every(({ requiresAnswer }) => !requiresAnswer)),
            Stream.runHead,
        ),
        Effect.flatMap(Deferred.await(live.verdict), (over) => refused(verdictText(limit, instance.pid)(over))),
    ).pipe(Effect.timeoutOrElse({ duration: limit, orElse: () => Effect.flatMap(sampled(instance.pid, path.join(scratch, `${instance.pid}.sample.txt`)), (sample) => refused(`instance ${instance.pid} shows no responding state within ${Duration.format(limit)}, sample ${sample}, left running`)) }));
    return { live, shared };
});
const batchIn = Effect.fn('batchIn')(function* <S extends Any, C extends Any, E extends Any, P>(host: Host<S, C, E, P>, live: Live, batch: (records: string) => Batch<S['Type'], C['Type']>, folders: readonly string[], presses: readonly (readonly string[])[], limit: Duration.Duration) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const records = path.join(yield* fs.makeTempDirectoryScoped({ directory: live.scratch, prefix: 'batch-' }), 'records.jsonl');
    yield* fs.writeFileString(records, '');
    yield* Effect.all([Ref.set(live.presses, presses), Ref.set(live.notes, []), Ref.set(live.inFlight, Option.none())]);
    yield* Effect.forkScoped(Effect.catch(progress(host, live, records, folders, limit), (error) => ended(live, Verdict.Lost({ error }))));
    const reply = yield* Effect.result(
        Effect.raceFirst(
            host.send(live, batch(records)),
            Effect.flatMap(Deferred.await(live.verdict), (over) => HarnessError.make({ reason: { _tag: 'Unavailable', because: verdictText(limit, live.pid)(over) }, subject: `process ${live.pid}` })),
        ),
    );
    const [held, notes, verdict] = yield* Effect.all([host.records(live, records, Result.getSuccess(reply)), Ref.get(live.notes), Effect.flatMap(Deferred.poll(live.verdict), Effect.transposeOption)]);
    return { reply, ...held, notes, verdict } satisfies Ran<S['Type']>;
}, Effect.scoped);
const surveyIn = Effect.fn('surveyIn')(function* <S extends Any, C extends Any, E extends Any, P>(host: Host<S, C, E, P>, live: Live, label: string, opened: Option.Option<Schema.Json>, limit: Duration.Duration) {
    const { reply } = yield* batchIn(host, live, (records) => ({ records, label, survey: Option.none(), opened, cases: [] }), [], [], limit);
    const header = yield* Effect.fromResult(reply);
    return yield* Effect.fromOption(Option.fromUndefinedOr(header.survey), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'survey reply holds no survey' }, subject: `process ${live.pid}` }));
});
const finishedIn = Effect.fnUntraced(function* <C, S>(live: Live, rest: readonly Prepared<C>[], ran: Ran<S>, limit: Duration.Duration) {
    const on = (index: number) => ran.notes.filter(({ index: at }) => Option.contains(at, index)).map(({ answered }) => answered);
    const kept = rest.flatMap(({ map }, index) => Option.toArray(Option.map(Option.fromUndefinedOr(ran.finished.get(index)), (row) => ({ ...mapped(map)(row), answered: [...row.answered, ...on(index)] }))));
    const stop = Option.getOrElse(ran.running, () => ran.finished.size);
    const reason = Option.match(ran.verdict, {
        onNone: () => 'not run, batch ended before the case',
        onSome: Verdict.$match({
            Exited: () => `not run, instance ${live.pid} exited`,
            Hung: ({ hang }) => `not run, instance ${hang.pid} hung`,
            Dialog: ({ dialog }) => `not run, instance ${dialog.pid} shows a dialog`,
            Lost: ({ error }) => `not run, ${error.message}`,
        }),
    });
    const stopped = Option.zipWith(Result.getFailure(ran.reply), Array.get(rest, stop), (error, { name }) => ({ name, message: error.message }));
    const interrupted = yield* Effect.transposeOption(Option.map(stopped, ({ name, message }) => Effect.map(fault(limit, ran.verdict, message), (parts) => ({ name, answered: on(stop), ...parts }))));
    const unsent = (Result.isFailure(ran.reply) ? rest.slice(stop + 1) : []).map(({ name }) => ({ name, completion: { _tag: 'Failed' as const, reason } }));
    return [...kept, ...(yield* Effect.forEach([...Option.toArray(interrupted), ...unsent], (row) => Effect.orDie(Schema.decodeEffect(Outcome)(row))))];
});
const quitted = Effect.fn('quitted')(function* <S extends Any, C extends Any, E extends Any, P>(host: Host<S, C, E, P>, live: Live, limit: Duration.Duration) {
    const refused = yield* Ref.make(Option.none<Error>());
    yield* Effect.forkScoped(Effect.catch(host.quit(live), (error) => Ref.set(refused, Option.some(error))));
    const verdict = yield* Effect.timeoutOption(Deferred.await(live.verdict), limit);
    const quit = Option.match(yield* Ref.get(refused), { onNone: () => 'quit sent', onSome: (error) => `quit refused: ${error.message}` });
    return Option.match(verdict, {
        onNone: () => Option.some(`${quit}, instance ${live.pid} running past ${Duration.format(limit)}, left running`),
        onSome: Verdict.$match({
            Exited: ({ exit }) => (exit.signal === undefined && exit.report === undefined && (exit.status ?? 0) === 0 ? Option.none<string>() : Option.some(`${quit}, instance ${live.pid} ${described(exit)}`)),
            Hung: (held) => Option.some(verdictText(limit, live.pid)(held)),
            Dialog: (held) => Option.some(verdictText(limit, live.pid)(held)),
            Lost: (held) => Option.some(verdictText(limit, live.pid)(held)),
        }),
    });
}, Effect.scoped);
const execute = Effect.fn('execute')(function* <S extends Any, C extends Any, E extends Any, P, L extends Plan<C['Type'], E['Type']>>(
    host: Host<S, C, E, P>,
    chosen: Installed,
    scratch: string,
    label: string,
    limit: Duration.Duration,
    known: Option.Option<S['Type']>,
    planned: (survey: S['Type']) => Effect.Effect<L, Error, FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner>,
    authored: typeof Authored.Type = { cases: [] },
) {
    const same = Schema.toEquivalence(Schema.Json);
    const draft = yield* Effect.transposeOption(Option.map(known, planned));
    const idle = Option.filter(
        Option.zipWith(known, draft, (survey, plan): Done<S['Type'], C['Type'], E['Type'], L> => ({ survey, opened: Option.none(), plan, header: Option.none(), shared: [], failed: Option.none(), answered: [], outcomes: [] })),
        ({ plan }) => plan.sendable.length === 0,
    );
    const launched = Effect.fnUntraced(function* () {
        const { live, shared } = yield* acquired(host, chosen, scratch, limit, authored);
        const opened = yield* host.opened(live);
        const confirming = (held: S['Type']) => Option.match(opened, { onNone: () => true, onSome: (state) => Option.exists(host.survey.host(held), (fact) => same(fact, state)) });
        const confirmed = Option.filter(known, confirming);
        const survey = yield* Option.match(confirmed, { onSome: Effect.succeed, onNone: () => surveyIn(host, live, label, opened, limit) });
        const plan = yield* Option.match(
            Option.filter(draft, () => Option.isSome(confirmed)),
            { onSome: Effect.succeed, onNone: () => planned(survey) },
        );
        const rest = plan.sendable;
        const cases = rest.map(({ hosted }) => hosted);
        const ran = yield* batchIn(
            host,
            live,
            (records) => ({ records, label, survey: Option.some(survey), opened, cases }),
            rest.map(({ folder }) => folder),
            rest.map(({ presses }) => presses),
            limit,
        );
        const outcomes = yield* finishedIn(live, rest, ran, limit);
        const failed = yield* Option.match(ran.verdict, {
            onSome: (over) => Effect.succeedSome(verdictText(limit, live.pid)(over)),
            onNone: () => Option.match(Result.getFailure(ran.reply), { onSome: (error) => Effect.succeedSome(error.message), onNone: () => (live.launched ? quitted(host, live, limit) : Effect.succeedNone) }),
        });
        return { survey, opened, plan, header: Result.getSuccess(ran.reply), shared, failed, answered: ran.notes.filter(({ index }) => Option.isNone(index)).map(({ answered }) => answered), outcomes } satisfies Done<S['Type'], C['Type'], E['Type'], L>;
    }, Effect.scoped);
    const done = yield* Option.match(idle, { onSome: Effect.succeed, onNone: launched });
    const written = yield* writtenIn(host, done.plan.sendable);
    return { ...done, outcomes: [...done.plan.settled, ...done.outcomes].map((row) => ({ ...row, written: written.get(row.name) ?? [] })) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Any, type Batch, type Decoding, type Digest, type Done, type Entry, execute, type Header, type Host, type Installed, installations, type Launched, type Live, type Plan, type Prepared, type Rebuilt, type Records, recorded, type Services };
