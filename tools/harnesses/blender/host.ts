import { constants } from 'node:os';
import process from 'node:process';
import { State } from '@rasm/automation/applications';
import { Array, Data, Duration, Effect, Equal, Fiber, FileSystem, Filter, flow, identity, Match, Number, Option, Order, Path, type PlatformError, Record, Ref, Result, Schema, type Scope, Stream, String, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/process';
import { Batch, Engine, Entry, HostCase, Outcome, type Results, Session } from '#/blender/contract';
import { Vector } from '#/cases';
import { HarnessError } from '#/error';
import { crashed, type Exited, Launch, ended, settled, watch } from '#/instance';
import type { Answered } from '#/results';

// --- [TYPES] ---------------------------------------------------------------------------

type Element = Extract<typeof State.Type, { readonly _tag: 'Responding' }>['elements'][number];
type Row = (typeof Batch.Type.cases)[number];
type Notes = Ref.Ref<readonly (readonly [Option.Option<string>, typeof Answered.Type])[]>;
type End = Data.TaggedEnum<{ Exited: { readonly state: Exited }; Hung: { readonly cpu: 'busy' | 'blocked'; readonly sample: string; readonly python: string } }>;
interface Install {
    readonly path: string;
    readonly name: string;
    readonly identifier: string;
    readonly executable: string;
    readonly version: string;
}
type States = SubscriptionRef.SubscriptionRef<Option.Option<typeof State.Type>>;
interface Watched {
    readonly pid: number;
    readonly states: States;
    readonly limit: Duration.Duration;
    readonly results: string;
}
interface Started {
    readonly pid: number;
    readonly exited: Effect.Effect<Exited, HarnessError>;
    readonly early: Effect.Effect<string, PlatformError.PlatformError, FileSystem.FileSystem>;
    readonly halted: Effect.Effect<void, PlatformError.PlatformError>;
}
interface Mode {
    readonly started: (install: Install, args: readonly string[], environment: Record.ReadonlyRecord<string, string>, states: States, stderr: string) => Effect.Effect<Started, HarnessError | PlatformError.PlatformError | Schema.SchemaError, Scope.Scope | ChildProcessSpawner.ChildProcessSpawner>;
    readonly stalled: (watched: Watched) => Effect.Effect<'busy' | 'blocked', HarnessError | PlatformError.PlatformError, FileSystem.FileSystem | ChildProcessSpawner.ChildProcessSpawner>;
}
interface Hosted {
    readonly host: typeof HostCase.Type;
    readonly presses: readonly string[];
}
interface Paging {
    readonly remaining: readonly Hosted[];
    readonly asked: Option.Option<string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const BUNDLE = 'org.blenderfoundation.blender';
const VERSION = /^Blender (?<release>\S+) ?(?<cycle>.*)$[\s\S]*?build hash: (?<hash>\w+)/mu;
const VERSIONS = new Intl.Collator('en', { numeric: true });
const DISMISS = 'Ignore';
const RUNNING = 'R';
const SIGNALS: Record.ReadonlyRecord<string, number> = constants.signals;

// --- [MODELS] --------------------------------------------------------------------------

const End = Data.taggedEnum<End>();
const Bundle = Schema.fromJsonString(Schema.Struct({ CFBundleName: Schema.String, CFBundleIdentifier: Schema.String, CFBundleExecutable: Schema.String }));
const Encoded = Schema.fromJsonString(Session);
const Line = Schema.fromJsonString(Entry);
const Interrupted = Schema.Struct({ message: Schema.TemplateLiteralParser(["Process interrupted due to receipt of signal: '", Schema.String, "'"]) });
const newest = Order.mapInput(
    Order.make((left: string, right: string) => Number.sign(VERSIONS.compare(left, right))),
    ({ version }: Install) => version,
);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INSTALLS]
const lines = (text: string) => Array.fromIterable(String.linesIterator(text)).filter(String.isNonEmpty);
const output = Effect.fn('output')(function* (command: string, args: readonly string[]) {
    const handle = yield* ChildProcess.make(command, args, { stdin: 'ignore' });
    const [text, errors, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? text.trim() : yield* HarnessError.make({ reason: { _tag: 'Unavailable', because: `exited with code ${code}: ${errors}${text}` }, subject: [command, ...args].join(' ') });
}, Effect.scoped);
const identify = Effect.fn('identify')(function* (app: string) {
    const path = yield* Path.Path;
    const bundle = yield* Effect.flatMap(output('plutil', ['-convert', 'json', '-o', '-', path.join(app, 'Contents', 'Info.plist')]), Schema.decodeEffect(Bundle));
    const executable = path.join(app, 'Contents', 'MacOS', bundle.CFBundleExecutable);
    const printed = yield* output(executable, ['--version']);
    const version = yield* Effect.fromOption(
        Option.map(Option.fromNullishOr(VERSION.exec(printed)?.groups), ({ release = '', cycle = '', hash = '' }) => `${release}${cycle === '' ? '' : `-${cycle.toLowerCase().replaceAll(' ', '-')}`}+${hash}`),
        () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'printed no release and build hash' }, subject: app }),
    );
    return { path: app, name: bundle.CFBundleName, identifier: bundle.CFBundleIdentifier, executable, version } satisfies Install;
});
const installations = Effect.fn('installations')(function* (releases: readonly string[]) {
    const rows = Array.sort(yield* Effect.forEach(lines(yield* output('mdfind', [`kMDItemCFBundleIdentifier == "${BUNDLE}"`])), identify), Order.flip(newest));
    const chosen = releases.length === 0 ? rows : rows.filter(({ version }) => releases.some((prefix) => version.startsWith(prefix)));
    return yield* Effect.fromOption(Array.match(chosen, { onEmpty: Option.none, onNonEmpty: Option.some }), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: `installed releases ${rows.map(({ path, version }) => `${path} ${version}`).join(', ')}` }, subject: `Blender ${releases.join(', ')}` }));
});

// --- [CASES]
const vectored =
    ({ calls, script }: NonNullable<typeof Batch.Type.vector>) =>
    ({ name, fn, args }: (typeof Vector.Type.cases)[number]): Option.Option<Row> =>
        Option.map(Record.get(calls, fn), (call) => ({ name, fn, call, args, ...(script === undefined ? {} : { script }) }));
const bodyOf = (located: (relative: string) => string, batch: typeof Batch.Type) =>
    Match.type<Row>().pipe(
        Match.when({ evaluate: Match.string }, ({ evaluate }) => Effect.succeed({ _tag: 'Probe' as const, evaluate, expression: evaluate })),
        Match.when({ call: Match.string }, ({ call, args, keywords, script }) => Effect.succeed({ _tag: 'Call' as const, call, args, keywords: keywords ?? {}, ...UndefinedOr.match(script ?? batch.script, { onUndefined: () => ({}), onDefined: (held) => ({ script: located(held) }) }), expression: call })),
        Match.orElse(({ name, script }) =>
            Effect.fromOption(
                Option.map(Option.fromUndefinedOr(script ?? batch.script), (relative) => ({ _tag: 'Run' as const, script: located(relative), expression: relative })),
                () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'names no evaluate, call, or script' }, subject: name }),
            ),
        ),
    );
const hostedOf = Effect.fn('hostedOf')(function* (product: string, batch: typeof Batch.Type, row: Row) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const located = (relative: string) => path.join(product, relative);
    const precondition = { ...batch.precondition, ...row.precondition };
    const { expression, ...body } = yield* bodyOf(located, batch)(row);
    const files = yield* Effect.forEach(row.files ?? [], (held) => (held === null ? Effect.succeed(null) : Effect.map(fs.exists(located(held)), (found) => ({ path: held, ...(found ? { source: located(held) } : {}) }))));
    const document = Match.value(precondition.document).pipe(
        Match.when({ open: Match.string }, (opened) => ({ document: { ...opened, open: located(opened.open) } })),
        Match.orElse(() => ({})),
    );
    const host = yield* Schema.decodeUnknownEffect(HostCase)({ ...body, name: row.name, precondition: { ...precondition, ...document }, read: { ...batch.read, ...row.read }, confirms: row.confirms ?? [], files });
    return { row, host, presses: row.presses ?? [], key: { expression, precondition } };
});
const prepare = Effect.fn('prepare')(function* (product: string, file: string, only: readonly string[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const batch = yield* Schema.decodeEffect(Schema.fromJsonString(Batch))(yield* fs.readFileString(file), { onExcessProperty: 'error' });
    const expanded: readonly Row[] = yield* UndefinedOr.match(batch.vector, {
        onUndefined: () => Effect.succeed([]),
        onDefined: (vector) => Effect.map(Effect.flatMap(fs.readFileString(path.join(product, vector.file)), Schema.decodeEffect(Vector)), ({ cases }) => Array.getSomes(cases.map(vectored(vector)))),
    });
    const rows = [...batch.cases, ...expanded].filter(({ name }) => only.length === 0 || only.includes(name));
    return { batch, items: yield* Effect.forEach(rows, (row) => hostedOf(product, batch, row)) };
});
const entries = Effect.fn('entries')(function* (file: string) {
    const fs = yield* FileSystem.FileSystem;
    const text = yield* fs.readFileString(file);
    return yield* Effect.forEach(lines(text.slice(0, text.lastIndexOf('\n') + 1)), (line) => Schema.decodeEffect(Line)(line));
});
const flightOf = (rows: readonly (typeof Entry.Type)[]) =>
    Option.filter(
        Option.map(Array.findLast(rows, Entry.guards.Started), ({ name }) => name),
        (name) => !rows.some((row) => Entry.guards.Finished(row) && row.outcome.name === name),
    );

// --- [WATCH]
const pressable = (element: Element, wanted: Option.Option<string>) => {
    const pressing = (element.detail?.controls ?? []).filter(({ actions }) => actions.includes('AXPress'));
    return Option.firstSomeOf([
        Option.flatMap(wanted, (label) => Array.findFirst(pressing, (control) => control.label === label)),
        Array.findFirst(pressing, ({ attribute }) => attribute === 'AXCancelButton'),
        Array.findFirst(pressing, ({ attribute }) => attribute === 'AXDefaultButton'),
        Array.head(pressing.length === 1 ? pressing : []),
    ]);
};
const dismissal = (element: Element) => {
    const pressing = (element.detail?.controls ?? []).filter(({ actions }) => actions.includes('AXPress'));
    return Option.firstSomeOf([Array.findFirst(pressing, ({ label }) => label === DISMISS), Array.findFirst(pressing, ({ attribute }) => attribute === 'AXCancelButton')]);
};
const answering = (install: Install, hosted: readonly Hosted[], results: string, notes: Notes, done: Ref.Ref<ReadonlySet<string>>, crashes: Ref.Ref<number>) =>
    Effect.fn('answering')(function* (identifier: string, state: typeof State.Type) {
        const own = identifier === install.identifier;
        yield* own && State.guards.Exited(state) && state.signal !== undefined && state.report === undefined && state.reportFailure === undefined ? Ref.update(crashes, Number.increment) : Effect.void;
        const pressed = yield* Ref.get(done);
        const waiting = State.guards.Responding(state) ? state.elements.filter(({ requiresAnswer, id }) => requiresAnswer && !pressed.has(id)) : [];
        yield* Effect.forEach(
            waiting,
            Effect.fnUntraced(function* (element: Element) {
                const flight = flightOf(yield* entries(results));
                const used = (yield* Ref.get(notes)).filter(([name]) => Equal.equals(name, flight)).length;
                const wanted = Option.flatMap(
                    Array.findFirst(hosted, ({ host }) => Option.contains(flight, host.name)),
                    ({ presses }) => Array.get(presses, used),
                );
                const crashing = yield* Ref.get(crashes);
                const chosen = own ? pressable(element, wanted) : Option.filter(dismissal(element), () => crashing > 0);
                const label = yield* Option.match(chosen, { onNone: () => Effect.succeedNone, onSome: (held) => Effect.as(output('watch-apps', ['perform', held.id, 'AXPress']), Option.some(held.label ?? held.id)) });
                yield* own || Option.isNone(label) ? Effect.void : Ref.update(crashes, Number.decrement);
                yield* Ref.update(done, (held) => new Set([...held, element.id]));
                const shown = { ...(element.title === undefined ? {} : { title: element.title }), text: element.detail?.text ?? [] };
                yield* Ref.update(notes, Array.append([flight, { ...shown, ...Option.match(label, { onNone: () => ({}), onSome: (text) => ({ pressed: text }) }) }] as const));
            }),
            { discard: true },
        );
    });
const background = Effect.fnUntraced(function* (install: Install, args: readonly string[], environment: Record.ReadonlyRecord<string, string>) {
    const handle = yield* ChildProcess.make(install.executable, ['--background', ...args], { stdin: 'ignore', env: environment, extendEnv: true });
    const early = yield* Effect.forkScoped(Stream.mkString(Stream.decodeText(handle.all)));
    return {
        pid: handle.pid,
        exited: Effect.map(
            Effect.result(handle.exitCode),
            Result.match({
                onSuccess: (status): Exited => ({ _tag: 'Exited', status }),
                onFailure: ({ reason }): Exited => ({
                    _tag: 'Exited',
                    ...Option.match(
                        Option.flatMap(Schema.decodeUnknownOption(Interrupted)(reason.cause), ({ message: [, name] }) => Record.get(SIGNALS, name)),
                        { onNone: () => ({}), onSome: (signal) => ({ signal }) },
                    ),
                }),
            }),
        ),
        early: Fiber.join(early),
        halted: Effect.asVoid(handle.kill({ killSignal: 'SIGKILL' })),
    } satisfies Started;
});
const launched = Effect.fnUntraced(function* (install: Install, args: readonly string[], environment: Record.ReadonlyRecord<string, string>, states: States, stderr: string) {
    const { pid, state } = yield* Launch({ app: install.path, fresh: true, arguments: ['--no-window-focus', '--enable-event-simulate', ...args], environment });
    yield* SubscriptionRef.set(states, Option.some(state));
    return {
        pid,
        exited: SubscriptionRef.changes(states).pipe(
            Stream.filterMap(Filter.fromPredicateOption(identity)),
            Stream.filter(settled),
            Stream.runHead,
            Effect.flatMap(Effect.fromOption(() => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'watch ended before the instance exited' }, subject: `process ${pid}` }))),
        ),
        early: Effect.flatMap(FileSystem.FileSystem, (fs) => fs.readFileString(stderr)),
        halted: Effect.void,
    } satisfies Started;
});
const hang = Effect.fn('hang')(function* (started: Started, stderr: string, captures: string, stalled: Effect.Effect<'busy' | 'blocked', HarnessError | PlatformError.PlatformError, FileSystem.FileSystem | ChildProcessSpawner.ChildProcessSpawner>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const cpu = yield* stalled;
    yield* fs.makeDirectory(captures, { recursive: true });
    const offset = (yield* fs.readFileString(stderr)).length;
    yield* Effect.sync(() => process.kill(started.pid, 'SIGUSR1'));
    const sample = yield* ended(started.pid, path.join(captures, `${started.pid}.sample.txt`));
    yield* started.halted;
    const python = (yield* fs.readFileString(stderr)).slice(offset);
    return End.Hung({ cpu, sample, python });
});
const Modes = {
    headless: {
        started: background,
        stalled: Effect.fnUntraced(function* ({ pid, limit, results }: Watched) {
            const fs = yield* FileSystem.FileSystem;
            yield* fs.watch(results).pipe(
                Stream.map(() => undefined),
                Stream.prepend([undefined]),
                Stream.switchMap(() => Stream.fromEffect(Effect.sleep(limit))),
                Stream.runHead,
            );
            return (yield* output('ps', ['-o', 'stat=', '-p', `${pid}`])).startsWith(RUNNING) ? 'busy' : 'blocked';
        }),
    },
    windowed: {
        started: launched,
        stalled: ({ pid, states, limit }: Watched) =>
            SubscriptionRef.changes(states).pipe(
                Stream.filterMap(Filter.fromPredicateOption(identity)),
                Stream.changesWith((left, right) => left._tag === right._tag && (!(State.guards.Unresponsive(left) && State.guards.Unresponsive(right)) || left.process === right.process)),
                Stream.switchMap((state) => (State.guards.Unresponsive(state) ? Stream.fromEffect(Effect.as(Effect.sleep(limit), state)) : Stream.empty)),
                Stream.filter(State.guards.Unresponsive),
                Stream.runHead,
                Effect.flatMap(Effect.fromOption(() => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'watch ended' }, subject: `process ${pid}` }))),
                Effect.map(({ process: run }) => (run === 'running' ? 'busy' : 'blocked')),
            ),
    },
} as const satisfies Record.ReadonlyRecord<string, Mode>;
const windowed = ({ precondition, confirms, files }: typeof HostCase.Type) => precondition.window !== undefined || precondition.area !== undefined || confirms.length > 0 || files.length > 0;

// --- [SESSIONS]
const session = Effect.fn('session')(function* (install: Install, mode: Mode, folder: string, hosted: readonly Hosted[], dictionary: Option.Option<string>, limit: Duration.Duration, captures: string, crashes: Ref.Ref<number>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const run = yield* fs.makeTempDirectoryScoped({ directory: folder, prefix: 'session-' });
    const [file, profile] = [path.join(run, 'session.json'), path.join(run, 'profile')];
    const written = Session.make({
        limit: Duration.toSeconds(limit),
        results: path.join(run, 'results.jsonl'),
        stdout: path.join(run, 'stdout.txt'),
        stderr: path.join(run, 'stderr.txt'),
        log: path.join(run, 'blender.log'),
        ...Option.match(dictionary, { onNone: () => ({}), onSome: (held) => ({ dictionary: held }) }),
        cases: hosted.map(({ host }) => host),
    });
    yield* Effect.all([fs.makeDirectory(profile), fs.writeFileString(written.results, ''), Effect.flatMap(Schema.encodeEffect(Encoded)(written), (text) => fs.writeFileString(file, text))]);
    const states = yield* SubscriptionRef.make(Option.none<typeof State.Type>());
    const [notes, done] = yield* Effect.all([Ref.make<readonly (readonly [Option.Option<string>, typeof Answered.Type])[]>([]), Ref.make<ReadonlySet<string>>(new Set())]);
    const started = yield* mode.started(install, ['--log-file', written.log, '--log', 'operator', '--log-level', 'debug', '--python-exit-code', '1', '--python', path.join(import.meta.dirname, 'engine.py'), '--', file], { TMPDIR: run, BLENDER_USER_RESOURCES: profile }, states, written.stderr);
    const subject = `${install.identifier} ${started.pid}`;
    const watching = Effect.andThen(
        Stream.runForEach(watch([install.identifier], started.pid), ({ identifier, state }) => Effect.andThen(identifier === install.identifier ? SubscriptionRef.set(states, Option.some(state)) : Effect.void, answering(install, hosted, written.results, notes, done, crashes)(identifier, state))),
        HarnessError.make({ reason: { _tag: 'Unavailable', because: 'watch ended before the instance exited' }, subject }),
    );
    const end = yield* Effect.raceFirst(
        watching,
        Effect.raceFirst(
            Effect.map(started.exited, (state) => End.Exited({ state })),
            hang(started, written.stderr, captures, mode.stalled({ pid: started.pid, states, limit, results: written.results })),
        ),
    );
    const rows = yield* entries(written.results);
    yield* Option.match(Array.findFirst(rows, Entry.guards.Failed), { onNone: () => Effect.void, onSome: ({ reason }) => HarnessError.make({ reason: { _tag: 'Unavailable', because: reason }, subject }) });
    const backtraces = yield* Effect.forEach(
        (yield* fs.readDirectory(run)).filter((name) => name.endsWith('.crash.txt')),
        (name) => fs.readFileString(path.join(run, name)),
    );
    const finished = rows.filter(Entry.guards.Finished).map(({ outcome }) => outcome);
    const flight = flightOf(rows);
    const interrupted = yield* End.$match(end, {
        Exited: ({ state }) => Effect.map(crashed(state), (crash) => ({ crashed: crash, backtraces })),
        Hung: ({ cpu, sample, python }) => Effect.succeed({ hung: { cpu, sample }, stacks: python, backtraces }),
    });
    const lost = Option.map(flight, (name) => Outcome.make({ name, completion: { _tag: 'Failed', reason: `process ${started.pid} ${End.$match(end, { Exited: () => 'ended', Hung: ({ cpu }) => `hung ${cpu}` })}` }, ...interrupted }));
    const placed = Option.isNone(flight) && finished.length < hosted.length ? Array.matchRight(finished, { onEmpty: () => finished, onNonEmpty: (init, last) => [...init, { ...last, ...interrupted }] }) : finished;
    const outcomes = [...placed, ...Option.toArray(lost)];
    const answered = yield* Ref.get(notes);
    const annotated = outcomes.map((outcome) =>
        Array.match(
            answered.filter(([name]) => Option.contains(name, outcome.name)),
            { onEmpty: () => outcome, onNonEmpty: (held) => ({ ...outcome, answered: held.map(([, note]) => note) }) },
        ),
    );
    const reported = Option.map(Array.findFirst(rows, Entry.guards.Host), ({ host }) => host);
    yield* outcomes.length === 0 && (hosted.length > 0 || Option.isNone(reported)) ? Effect.flatMap(started.early, (text) => HarnessError.make({ reason: { _tag: 'Unavailable', because: `ended before any case: ${JSON.stringify(interrupted)} ${text}` }, subject })) : Effect.void;
    const reached = new Set(outcomes.map(({ name }) => name));
    return {
        host: reported,
        outcomes: annotated,
        answered: answered.filter(([name]) => Option.isNone(name)).map(([, note]) => note),
        rest: hosted.filter(({ host: held }) => !reached.has(held.name)),
    };
}, Effect.scoped);
const execute = Effect.fn('execute')(function* (install: Install, hosted: readonly Hosted[], limit: Duration.Duration, captures: string, dictionary: Option.Option<string>) {
    const fs = yield* FileSystem.FileSystem;
    const folder = yield* fs.makeTempDirectoryScoped({ prefix: 'blender-harness-' });
    const crashes = yield* Ref.make(0);
    const [shown, headless] = Array.partition(
        hosted,
        Filter.fromPredicate(({ host }: Hosted) => windowed(host)),
    );
    const groups = Array.match(
        [
            { mode: Modes.headless, items: headless },
            { mode: Modes.windowed, items: shown },
        ].filter(({ items }) => items.length > 0),
        { onEmpty: () => [{ mode: Modes.headless, items: [] }], onNonEmpty: identity },
    );
    const pages = (yield* Effect.forEach(groups, ({ mode, items }, index) => {
        const start: Paging = { remaining: items, asked: index === 0 ? dictionary : Option.none() };
        return Stream.runCollect(
            Stream.paginate(
                start,
                Effect.fnUntraced(function* ({ remaining, asked }: Paging) {
                    const page = yield* session(install, mode, folder, remaining, asked, limit, captures, crashes);
                    const next: Paging = { remaining: page.rest, asked: Option.none() };
                    return [[page], Option.liftPredicate(next, flow(Struct.get('remaining'), Array.isReadonlyArrayNonEmpty))] as const;
                }),
            ),
        );
    })).flat();
    return {
        application: install.name,
        identifier: install.identifier,
        version: install.version,
        engine: Engine.literal,
        ...Option.match(
            Array.findFirst(pages, ({ host }) => host),
            { onNone: () => ({}), onSome: (host) => ({ host }) },
        ),
        answered: pages.flatMap(({ answered }) => answered),
        cases: pages.flatMap(({ outcomes }) => outcomes),
    } satisfies typeof Results.Type;
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, installations, prepare };
