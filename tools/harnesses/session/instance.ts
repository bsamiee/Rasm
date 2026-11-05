import { constants } from 'node:os';
import { type Launching, type Pressure, type RunningInstance, State } from '@rasm/applications';
import { Automation } from '@rasm/applications/node';
import { Array, ByteSize, Data, DateTime, Duration, Effect, Equivalence, FileSystem, identity, Match, Option, Path, type PlatformError, Predicate, PubSub, Queue, Record, Result, Schema, type Scope, Sink, Stream, String, Struct } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/process';
import type { Halted } from '#/cases/results';
import { HarnessError } from '#/harness-error';
import type { Installed } from '#/session/harness';
import { messageOf, record, tagOf } from '#/trace/records';

// --- [TYPES] ---------------------------------------------------------------------------

type Chosen = Pick<Installed, 'identifier' | 'name' | 'app' | 'helpers'>;
type Releasing = (release: Effect.Effect<void, Error, Automation>) => Effect.Effect<void>;
type Starting = Automation | ChildProcessSpawner.ChildProcessSpawner | Path.Path | Scope.Scope;
interface Started {
    readonly identifier: string;
    readonly pid: number;
    readonly state: typeof State.Type;
    readonly at: DateTime.Utc;
    readonly created: boolean;
    readonly shared: readonly number[];
    readonly rows: Stream.Stream<typeof Observation.Type, Error>;
}
interface Asked {
    readonly window: typeof State.Window.Type;
    readonly dialog: typeof Halted.cases.Dialog.Type;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SIGNALS: Record.ReadonlyRecord<string, number> = constants.signals;
const MAIN_THREAD = /^\s*\d+ Thread_\S+\s+DispatchQueue_1: com\.apple\.main-thread\b/u;
const FRAME = /^(?<depth>[ +!:|]*)\d+ (?<symbol>.+?) {2}(?:\(in (?<image>[^)]+)\)|\[)/u;
const SPACES = /\s+/u;
const LABELLED = Array.makeEquivalence(Equivalence.String);

// --- [MODELS] --------------------------------------------------------------------------

const Interrupted = Schema.Struct({ message: Schema.TemplateLiteralParser(["Process interrupted due to receipt of signal: '", Schema.String, "'"]) });
const Choice = Schema.Literals(['accept', 'discard', 'cancel']).annotate({ description: 'Answer control marked on a dialog, a case press naming any other text names a button label' });
const sameState = Schema.toEquivalence(State);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PROCESSES]
const output = Effect.fn('output')(function* (command: string, args: readonly string[], env: Record.ReadonlyRecord<string, string> = {}) {
    const handle = yield* ChildProcess.make(command, args, { stdin: 'ignore', env, extendEnv: true });
    const [stdout, stderr, code] = yield* Effect.all([Stream.mkUint8Array(handle.stdout), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return { stdout, stderr, code };
}, Effect.scoped);
const checked = Effect.fn('checked')(function* (command: string, args: readonly string[], env: Record.ReadonlyRecord<string, string> = {}) {
    const { stdout, stderr, code } = yield* output(command, args, env);
    return code === 0 ? stdout : yield* HarnessError.unavailable([command, ...args].join(' '))(`exited with code ${code}: ${stderr}`);
});
const printed = (command: string, args: readonly string[], env: Record.ReadonlyRecord<string, string> = {}) => Effect.map(checked(command, args, env), (bytes) => new TextDecoder().decode(bytes));
const printedLines = (command: string, args: readonly string[], env: Record.ReadonlyRecord<string, string> = {}) => Effect.map(printed(command, args, env), (text) => Array.fromIterable(String.linesIterator(text)).filter(String.isNonEmpty));
const startOf = (line: string) =>
    Array.matchLeft(line.trim().split(SPACES), {
        onEmpty: Option.none,
        onNonEmpty: (pid, started) => Option.zipWith(Schema.decodeOption(Schema.FiniteFromString)(pid), Option.map(DateTime.makeZoned(started.join(' '), { timeZone: DateTime.zoneMakeLocal(), adjustForTimeZone: true }), DateTime.toUtc), (held, at) => [held, at] as const),
    });
const starts = (selected: readonly string[]) => Effect.map(printedLines('ps', ['-o', 'pid=,lstart=', ...selected]), (lines) => new Map(Array.getSomes(lines.map(startOf))));
const converted = <S extends Schema.Top>(file: string, schema: S) => Effect.flatMap(printed('plutil', ['-convert', 'json', '-o', '-', file]), Schema.decodeEffect(Schema.fromJsonString(schema)));
const plist = Effect.fn('plist')(function* <S extends Schema.Top>(app: string, schema: S) {
    const path = yield* Path.Path;
    return yield* converted(path.join(app, 'Contents', 'Info.plist'), schema);
});
const entitlements = Effect.fn('entitlements')(function* <S extends Schema.Top>(app: string, file: string, schema: S) {
    yield* checked('codesign', ['-d', '--entitlements', file, '--xml', app]);
    return yield* converted(file, schema);
});
const sampled = (pid: number, file: string) => Effect.as(checked('sample', [globalThis.String(pid), '1', '-file', file]), file);
const stalled = Effect.fn('stalled')(function* (sample: string) {
    const lines = (yield* FileSystem.FileSystem.use((fs) => fs.readFileString(sample))).split('\n');
    const block = Array.takeWhile(
        Array.drop(
            Array.dropWhile(lines, (line) => !MAIN_THREAD.test(line)),
            1,
        ),
        (line) => line.trim() !== '',
    );
    const frames = Array.getSomes(block.map((line) => Option.fromNullishOr(FRAME.exec(line)?.groups)));
    const heaviest = Array.takeWhile(frames, ({ depth = '' }, at) => Option.match(Array.get(frames, at - 1), { onNone: () => true, onSome: (above) => depth.length > (above.depth ?? '').length }));
    return Array.reverse(heaviest).map(({ image, symbol = '' }) => (image === undefined ? symbol : `${image} ${symbol}`));
});

// --- [FILES]
const watched = (path: string) =>
    Stream.callback<void, PlatformError.PlatformError, FileSystem.FileSystem>((queue) => {
        const onReady = () => Queue.offerUnsafe(queue, undefined);
        return FileSystem.FileSystem.use((fs) => Stream.runIntoQueue(Stream.as(fs.watch(path, { onReady }), undefined), queue));
    });
const appeared = Effect.fn('appeared')(function* (file: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    yield* Stream.runHead(
        Stream.filter(
            Stream.mapEffect(watched(path.dirname(file)), () => fs.exists(file)),
            identity,
        ),
    );
    return yield* fs.readFileString(file);
});

// --- [ROWS]
const stamped = Stream.mapEffect(({ identifier, pid, state }: RunningInstance) => Effect.map(DateTime.now, (at): typeof Observation.Type => ({ identifier, pid, state, at })));
const changed = Stream.mapAccum(
    () => new Map<number, typeof State.Type>(),
    (seen, rows: readonly RunningInstance[]) => {
        const fresh = rows.filter(({ pid, state }) => {
            const held = seen.get(pid);
            return held === undefined || !sameState(held, state);
        });
        return [new Map(rows.map(({ pid, state }) => [pid, state] as const)), fresh] as const;
    },
);
const pressures = Result.match({
    onSuccess: (pressure: typeof Pressure.Type) => record(pressure.memory === 'normal' && (pressure.thermal === 'nominal' || pressure.thermal === 'fair') ? 'Info' : 'Warn', 'system pressure', pressure),
    onFailure: (error: Error) => record('Warn', 'failed', { tag: tagOf(error), error: messageOf(error) }),
});

// --- [INSTANCES] -----------------------------------------------------------------------

const Observation = Object.assign(Schema.Struct({ identifier: Schema.String, pid: Schema.Int, state: State, at: Schema.DateTimeUtc.annotate({ description: 'Instant the state arrived' }) }), {
    tag: (state: typeof State.Type) => State.match<(typeof State.Running.Type)['status'] | 'failed' | 'exited'>(state, { running: ({ status }) => status, failed: () => 'failed', exited: () => 'exited' }),
    perform: (pid: number, element: string) => Automation.use((automation) => automation.perform({ pid, actions: [{ perform: { element, action: 'AXPress' } }] })),
    settled: (state: typeof State.Type): state is Extract<typeof State.Type, { readonly exited: unknown }> => Predicate.hasProperty(state, 'exited') && state.exited.reporting !== true,
    described: (state: typeof State.Type): string =>
        State.match(state, {
            running: (running) =>
                Match.value(running).pipe(
                    Match.when(
                        ({ status }: typeof State.Running.Type) => status === 'unresponsive',
                        ({ process, usage, windows }) =>
                            `unresponsive ${process ?? ''}${usage === undefined ? '' : ` cpu ${Duration.format(Duration.seconds(usage.cpu))} footprint ${ByteSize.format(ByteSize.bytes(BigInt(usage.footprint)))}`}${windows.length === 0 ? '' : `, windows ${windows.map(({ number, title }) => (title === undefined ? globalThis.String(number) : `${number} "${title}"`)).join(', ')}`}`,
                    ),
                    Match.orElse(({ status }) => `${status}${Array.isReadonlyArrayNonEmpty(State.asking(state)) ? ' with a dialog requiring an answer' : ''}`),
                ),
            failed: ({ reason }) => `status failed: ${reason}`,
            exited: ({ signal, status, report, reportFailure }) => `exited with ${signal === undefined ? `status ${status ?? 'unread'}` : `signal ${signal}`}${report === undefined ? '' : `, report ${report.url}`}${reportFailure === undefined ? '' : `, ${reportFailure}`}`,
        }),
    shown: ({ kind, number, title, text, buttons }: typeof Halted.cases.Dialog.Type, pid: number): string => `instance ${pid} shows ${kind ?? 'dialog'} ${number ?? ''} ${title ?? ''}${text.length === 0 ? '' : ` "${text.join(' ')}"`}${buttons.length === 0 ? '' : ` with ${buttons.join(', ')}`}, left running`,
});
class Progress extends Data.Class<{ readonly title: string; readonly buttons: readonly string[] }> {
    static readonly asked = (passing: readonly Progress[], state: typeof State.Type): Option.Option<Asked> =>
        Array.findFirst(
            State.asking(state).map((window): Asked => {
                const title = window.element?.title ?? window.title;
                const buttons = Array.getSomes((window.element?.answers?.buttons ?? []).map(({ label }) => Option.fromUndefinedOr(label)));
                return { window, dialog: { _tag: 'Dialog', kind: window.kind, number: window.number, ...Record.getSomes({ title: Option.fromUndefinedOr(title) }), text: window.element?.detail?.text ?? [], buttons } };
            }),
            ({ dialog }) => !passing.some((named) => [...Option.toArray(Option.fromUndefinedOr(dialog.title)), ...dialog.text].includes(named.title) && LABELLED(named.buttons, dialog.buttons)),
        );
    static readonly pressed = ({ element }: typeof State.Window.Type, press: string) =>
        Option.map(
            Option.orElse(
                Option.flatMap(Schema.decodeUnknownOption(Choice)(press), (role) => Option.fromUndefinedOr(element?.answers?.[role]?.id)),
                () =>
                    Option.map(
                        Array.findFirst(element?.answers?.buttons ?? [], ({ label }) => label === press),
                        Struct.get('id'),
                    ),
            ),
            (id) => ({ press, id }),
        );
    static readonly shown = (passing: readonly Progress[], row: Pick<typeof Observation.Type, 'pid' | 'state'>) => Option.map(Progress.asked(passing, row.state), ({ dialog }) => Observation.shown(dialog, row.pid));
}
const launching = Effect.fn('launching')(function* (spec: Launched, chosen: Chosen, passing: readonly Progress[], releasing: Releasing) {
    const [automation, path] = yield* Effect.all([Automation, Path.Path]);
    const refused = HarnessError.unavailable(chosen.name);
    const refusing = (rows: readonly Pick<typeof Observation.Type, 'pid' | 'state'>[]) => Array.match(Array.getSomes(rows.map((row) => Progress.shown(passing, row))), { onEmpty: () => Effect.void, onNonEmpty: (held) => Effect.fail(refused(held.join('; '))) });
    yield* Effect.forkScoped(Stream.runForEach(automation.pressure(), pressures));
    const [first, rest] = yield* Stream.peel(automation.instances({ identifiers: [chosen.identifier, ...chosen.helpers] }), Sink.head());
    const running = yield* Effect.fromOption(first, () => refused('instance listing ended before its first value'));
    yield* refusing(running);
    const carried = [...(spec.arguments ?? []), ...Record.collect(spec.defaults ?? {}, (key, value) => [`-${key}`, value]).flat()];
    const environment = spec.environment ?? {};
    const reused = spec.several === true ? [] : running.filter(({ identifier, state }) => identifier === chosen.identifier && !Predicate.hasProperty(state, 'exited'));
    yield* (Array.isArrayNonEmpty(carried) || !Record.isEmptyRecord(environment)) && Array.isArrayNonEmpty(reused) ? refused(`instance ${reused.map(Struct.get('pid')).join(', ')} is running, launch arguments and environment reach only an instance launch creates`) : Effect.void;
    const subscribed = yield* PubSub.subscribe(yield* rest.pipe(changed, stamped, Stream.toPubSubTake({ capacity: 'unbounded' })));
    const located = yield* Effect.all({ url: path.toFileUrl(chosen.app), documents: Effect.forEach(spec.documents ?? [], (file) => path.toFileUrl(file)) });
    const stepped = Match.type<typeof Launching.Type>().pipe(
        Match.when({ launched: { created: true } }, ({ launched: { pid } }) => releasing(Effect.asVoid(automation.quit({ pid })))),
        Match.when({ prompts: Match.defined }, ({ prompts }) => refusing(prompts)),
        Match.when({ failed: Match.defined }, ({ failed: { reason, subject } }) => Effect.fail(refused(`launch failed ${reason} ${subject}`))),
        Match.orElse(() => Effect.void),
    );
    const steps = yield* Stream.runCollect(Stream.tap(automation.launch({ ...located, arguments: carried, environment, new: spec.several === true }), stepped));
    const last = Array.findLast(
        steps,
        Match.type<typeof Launching.Type>().pipe(
            Match.when({ state: Match.defined }, ({ state }) => state),
            Match.option,
        ),
    );
    const ended = Option.filter(last, ({ identifier, state }) => identifier === chosen.identifier && (Observation.tag(state) !== 'launching' || Array.isReadonlyArrayNonEmpty(State.asking(state))));
    const settled = yield* Effect.fromOption(ended, () => refused(`launch ended ${Option.match(last, { onNone: () => 'with no state', onSome: ({ identifier, pid, state }) => `at ${identifier} ${pid} ${Observation.described(state)}` })}`));
    const at = yield* DateTime.now;
    return {
        identifier: settled.identifier,
        pid: settled.pid,
        state: settled.state,
        at,
        created: steps.some((step) => Predicate.hasProperty(step, 'launched') && step.launched.created),
        shared: running.filter(({ pid, state }) => pid !== settled.pid && !Predicate.hasProperty(state, 'exited')).map(Struct.get('pid')),
        rows: Stream.filter(Stream.flattenTake(Stream.fromSubscription(subscribed)), (row) => row.identifier !== chosen.identifier || (row.pid === settled.pid && DateTime.isGreaterThan(row.at, at))),
    };
});
const spawning = Effect.fn('spawning')(function* (spec: Spawned, chosen: Chosen, releasing: Releasing) {
    const handle = yield* ChildProcess.make(spec.command, spec.arguments, { stdin: 'ignore', stdout: 'ignore', stderr: 'inherit', env: spec.environment, extendEnv: true });
    yield* releasing(handle.kill());
    const exited = Effect.map(
        Effect.result(handle.exitCode),
        Result.match({
            onSuccess: (status): typeof State.Type => ({ exited: { status } }),
            onFailure: (error): typeof State.Type => ({ exited: Record.getSomes({ signal: Option.flatMap(Schema.decodeUnknownOption(Interrupted)(error.cause), ({ message: [, name] }) => Record.get(SIGNALS, name)) }) }),
        }),
    );
    const alive: typeof State.Type = { running: { active: false, status: 'responding', windows: [] } };
    return { identifier: chosen.identifier, pid: handle.pid, state: alive, at: yield* DateTime.now, created: true, shared: [], rows: Stream.fromEffect(Effect.zipWith(exited, DateTime.now, (state, at): typeof Observation.Type => ({ identifier: chosen.identifier, pid: handle.pid, state, at }))) };
});
class Launched extends Data.TaggedClass('Launched')<{
    readonly several?: boolean;
    readonly documents?: readonly string[];
    readonly arguments?: readonly string[];
    readonly defaults?: Record.ReadonlyRecord<string, string>;
    readonly environment?: Record.ReadonlyRecord<string, string>;
}> {
    readonly start = (chosen: Chosen, passing: readonly Progress[], releasing: Releasing): Effect.Effect<Started, Error, Starting> => launching(this, chosen, passing, releasing);
}
class Spawned extends Data.TaggedClass('Spawned')<{ readonly command: string; readonly arguments: readonly string[]; readonly environment: Record.ReadonlyRecord<string, string> }> {
    readonly start = (chosen: Chosen, _: readonly Progress[], releasing: Releasing): Effect.Effect<Started, Error, Starting> => spawning(this, chosen, releasing);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Asked, appeared, checked, entitlements, Launched, Observation, output, Progress, plist, printed, printedLines, Spawned, sampled, stalled, starts, watched };
