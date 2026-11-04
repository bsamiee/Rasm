import { constants } from 'node:os';
import { State } from '@rasm/automation';
import { Array, ByteSize, Data, DateTime, Duration, Effect, Equivalence, FileSystem, identity, Match, Option, Path, type PlatformError, PubSub, Queue, Record, Result, Schema, Sink, Stream, Struct } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/process';
import { HarnessError } from '#/harness-error';
import type { Halted } from '#/results';
import type { Installed } from '#/session';

// --- [TYPES] ---------------------------------------------------------------------------

type Next = (typeof State.Type)['next'];
type Answer = Extract<NonNullable<Next>, { readonly answer: unknown }>['answer'];
type Chosen = Pick<Installed, 'identifier' | 'name' | 'app' | 'helpers'>;
type Releasing = (release: Effect.Effect<void, Error, ChildProcessSpawner.ChildProcessSpawner>) => Effect.Effect<void>;
interface Asked {
    readonly answer: Answer;
    readonly dialog: typeof Halted.cases.Dialog.Type;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPORTING = (['SIGQUIT', 'SIGILL', 'SIGTRAP', 'SIGABRT', 'SIGFPE', 'SIGBUS', 'SIGSEGV', 'SIGSYS'] as const).map((name) => constants.signals[name]);
const SIGNALS: Record.ReadonlyRecord<string, number> = constants.signals;
const MAIN_THREAD = /^\s*\d+ Thread_\S+\s+DispatchQueue_1: com\.apple\.main-thread\b/u;
const FRAME = /^(?<depth>[ +!:|]*)\d+ (?<symbol>.+?) {2}(?:\(in (?<image>[^)]+)\)|\[)/u;
const SPACES = /\s+/u;
const LABELLED = Array.makeEquivalence(Equivalence.String);

// --- [MODELS] --------------------------------------------------------------------------

const Interrupted = Schema.Struct({ message: Schema.TemplateLiteralParser(["Process interrupted due to receipt of signal: '", Schema.String, "'"]) });
const Instances = Schema.Record(Schema.String, Schema.Record(Schema.String, State));
const Failure = Schema.Record(Schema.String, Schema.Struct({ fail: Schema.Struct({ reason: Schema.String }) }));
const Shown = Schema.Record(Schema.String, Schema.Struct({ fail: Schema.Struct({ reason: Schema.Literal('visibility') }) })).annotate({ description: 'Hide watch-apps could not hold, the next state line stating it through hidden' });
const System = Schema.Struct({ system: Schema.Struct({ memory: Schema.Literals(['normal', 'warning', 'critical']), thermal: Schema.Literals(['nominal', 'fair', 'serious', 'critical']) }) });
const Line = Schema.fromJsonString(Schema.toCodecJson(Schema.Union([Instances, Failure, System])));
const Choice = Schema.Literals(['accept', 'discard', 'cancel']).annotate({ description: 'Answer control watch-apps marks on a dialog, a case press naming any other text names a button label' });

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
const startOf = (line: string) =>
    Array.matchLeft(line.trim().split(SPACES), {
        onEmpty: Option.none,
        onNonEmpty: (pid, started) => Option.zipWith(Schema.decodeOption(Schema.FiniteFromString)(pid), Option.map(DateTime.makeZoned(started.join(' '), { timeZone: DateTime.zoneMakeLocal(), adjustForTimeZone: true }), DateTime.toUtc), (held, at) => [held, at] as const),
    });
const starts = (selected: readonly string[]) => Effect.map(printed('ps', ['-o', 'pid=,lstart=', ...selected]), (text) => new Map(Array.getSomes(text.split('\n').map(startOf))));
const plist = Effect.fn('plist')(function* <S extends Schema.Top>(app: string, schema: S) {
    const path = yield* Path.Path;
    return yield* Schema.decodeEffect(Schema.fromJsonString(schema))(yield* printed('plutil', ['-convert', 'json', '-o', '-', path.join(app, 'Contents', 'Info.plist')]));
});
const sampled = (pid: number, file: string) => Effect.as(checked('sample', [String(pid), '1', '-file', file]), file);
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
const listed = (line: typeof Instances.Type, at: DateTime.Utc) => Record.collect(line, (identifier, instances) => Record.collect(instances, (pid, state) => ({ identifier, pid: Number(pid), state, at }))).flat();
const stamped = (line: typeof Instances.Type) => Effect.map(DateTime.now, (at) => listed(line, at));
const decoded = Schema.decodeEffect(Line);
const routed = Match.type<typeof Line.Type>().pipe(
    Match.when(Schema.is(System), ({ system }) => Effect.as(Effect.annotateLogs(Effect.logWithLevel(system.memory === 'normal' && (system.thermal === 'nominal' || system.thermal === 'fair') ? 'Info' : 'Warn')('system pressure'), system), [])),
    Match.when(Schema.is(Shown), () => Effect.succeed([])),
    Match.when(Schema.is(Failure), (failed) => HarnessError.unavailable('watch-apps')(Record.collect(failed, (key, { fail }) => `${key}: ${fail.reason}`).join(', '))),
    Match.orElse((instances) => Effect.succeed([instances])),
);
const answerOf = Match.type<Next>().pipe(
    Match.when({ answer: Match.defined }, ({ answer }) => answer),
    Match.option,
);
const lines = (command: readonly string[]) =>
    Effect.map(ChildProcess.make('watch-apps', command, { stdin: 'ignore', stderr: 'inherit' }), (handle) => ({
        pid: handle.pid,
        lines: handle.stdout.pipe(
            Stream.decodeText,
            Stream.splitLines,
            Stream.mapEffect((text) => Effect.flatMap(decoded(text), routed)),
            Stream.flattenIterable,
        ),
    }));
const drained = (command: readonly string[]) => Effect.scoped(Effect.flatMap(lines(command), ({ lines: held }) => Stream.runDrain(held)));

// --- [INSTANCES] -----------------------------------------------------------------------

const Observation = Object.assign(Schema.Struct({ identifier: Schema.String, pid: Schema.Int, state: State, at: Schema.DateTimeUtc.annotate({ description: 'Instant the line holding the state arrived' }) }), {
    perform: (id: string) => drained(['perform', id, 'AXPress']),
    settled: (state: typeof State.Type): state is typeof State.cases.Exited.Type => State.isAnyOf(['Exited'])(state) && (state.status !== undefined || state.report !== undefined || state.reportFailure !== undefined || (state.signal !== undefined && !REPORTING.includes(state.signal))),
    described: (state: typeof State.Type): string =>
        State.match(state, {
            Launching: () => 'launching',
            Responding: ({ answering }) => `responding${answering ? ' with a dialog requiring an answer' : ''}`,
            Unresponsive: ({ process, usage, windows }) =>
                `unresponsive ${process}${usage === undefined ? '' : ` cpu ${Duration.format(usage.cpu)} footprint ${ByteSize.format(usage.footprint)}`}${windows.length === 0 ? '' : `, windows ${windows.map(({ number, title }) => (title === undefined ? String(number) : `${number} "${title}"`)).join(', ')}`}`,
            Failed: ({ reason }) => `status failed: ${reason}`,
            Exited: ({ signal, status, report, reportFailure }) => `exited with ${signal === undefined ? `status ${status ?? 'unread'}` : `signal ${signal}`}${report === undefined ? '' : `, report ${report.url.href}`}${reportFailure === undefined ? '' : `, ${reportFailure}`}`,
        }),
    shown: (dialog: Pick<typeof Halted.cases.Dialog.Type, 'title' | 'text' | 'buttons'>, pid: number): string => `instance ${pid} shows ${dialog.title ?? 'a dialog'}${dialog.text.length === 0 ? '' : ` "${dialog.text.join(' ')}"`}${dialog.buttons.length === 0 ? '' : ` with ${dialog.buttons.join(', ')}`}, left running`,
});
class Progress extends Data.Class<{ readonly title: string; readonly buttons: readonly string[]; readonly enabled?: boolean }> {
    static readonly asked = (passing: readonly Progress[], state: typeof State.Type): Option.Option<Asked> => {
        const answer = answerOf(state.next);
        const id = Option.map(answer, Struct.get('id'));
        const element = Array.findFirst(State.isAnyOf(['Responding'])(state) ? state.elements : [], (held) => Option.contains(id, held.id));
        const title = Option.flatMap(element, (held) => Option.fromUndefinedOr(held.title));
        const text = Option.match(element, { onNone: () => [], onSome: ({ detail }) => detail?.text ?? [] });
        const controls = new Map(Option.match(element, { onNone: () => [], onSome: ({ detail }) => detail?.controls ?? [] }).map((control) => [control.id, control.enabled]));
        const buttons = Option.match(answer, { onNone: () => [], onSome: Struct.get('buttons') });
        const labels = Array.map(buttons, Struct.get('label'));
        const progressing = passing.some((named) => [...Option.toArray(title), ...text].includes(named.title) && LABELLED(named.buttons, labels) && (named.enabled === undefined || buttons.every((button) => controls.get(button.id) === named.enabled)));
        return Option.map(
            Option.filter(answer, () => !progressing),
            (held) => ({ answer: held, dialog: { _tag: 'Dialog', ...Record.getSomes({ title }), text, buttons: labels } }),
        );
    };
    static readonly pressed = (answer: Answer, press: string) =>
        Option.map(
            Option.orElse(
                Option.flatMap(Schema.decodeUnknownOption(Choice)(press), (role) => Option.fromUndefinedOr(answer[role])),
                () =>
                    Option.map(
                        Array.findFirst(answer.buttons, ({ label }) => label === press),
                        Struct.get('id'),
                    ),
            ),
            (id) => ({ press, id }),
        );
    static readonly shown = (passing: readonly Progress[], row: typeof Observation.Type) => Option.map(Progress.asked(passing, row.state), ({ dialog }) => Observation.shown(dialog, row.pid));
}
const launching = Effect.fn('launching')(function* (spec: Launched, chosen: Chosen, passing: readonly Progress[], releasing: Releasing) {
    const refused = HarnessError.unavailable(chosen.name);
    const rowsOf = <E, R>(held: Stream.Stream<typeof Instances.Type, E, R>) => Stream.flattenIterable(Stream.mapEffect(held, stamped));
    const watch = yield* lines(['watch', chosen.app, ...chosen.helpers]);
    const [first, rest] = yield* Stream.peel(watch.lines, Sink.find(Record.has(chosen.identifier)));
    const running = yield* stamped(yield* Effect.fromOption(first, () => refused(`watch ${chosen.app} ended before its first line`)));
    yield* Array.match(Array.getSomes(running.map((row) => Progress.shown(passing, row))), { onEmpty: () => Effect.void, onNonEmpty: (held) => refused(held.join('; ')) });
    const flags = [
        ...((spec.fresh ?? true) ? ['--new'] : []),
        ...(spec.documents ?? []).map((file) => `--document=${file}`),
        ...(spec.arguments ?? []).map((value) => `--arguments=${value}`),
        ...Record.collect(spec.defaults ?? {}, (key, value) => [`--arguments=-${key}`, `--arguments=${value}`]).flat(),
        ...Record.collect(spec.environment ?? {}, (name, value) => `--environment=${name}=${value}`),
    ];
    const subscribed = yield* PubSub.subscribe(yield* Stream.toPubSubTake(rowsOf(rest), { capacity: 'unbounded' }));
    const retain = ({ identifier, pid, state }: typeof Observation.Type) => (identifier === chosen.identifier && State.isAnyOf(['Launching', 'Responding', 'Unresponsive'])(state) && state.created === true ? releasing(drained(['quit', '--pid', `${pid}`])) : Effect.void);
    const launched = yield* Effect.scoped(Effect.flatMap(lines(['launch', chosen.app, ...flags]), ({ lines: held }) => Stream.runCollect(Stream.tap(rowsOf(held), retain))));
    const last = Array.last(launched);
    const ended = Option.filter(last, ({ identifier, state }) => identifier === chosen.identifier && !State.isAnyOf(['Launching'])(state));
    const settled = yield* Effect.fromOption(ended, () => refused(`launch ended ${Option.match(last, { onNone: () => 'with no state', onSome: ({ identifier, pid, state }) => `at ${identifier} ${pid} ${Observation.described(state)}` })}`));
    return {
        ...settled,
        shared: running.filter(({ pid, state }) => pid !== settled.pid && !State.isAnyOf(['Exited'])(state)).map(({ pid }) => pid),
        watch: Option.some(watch.pid),
        rows: Stream.filter(Stream.flattenTake(Stream.fromSubscription(subscribed)), (row) => row.identifier !== chosen.identifier || (row.pid === settled.pid && DateTime.isGreaterThan(row.at, settled.at))),
    };
});
const spawning = Effect.fn('spawning')(function* (spec: Spawned, chosen: Chosen, releasing: Releasing) {
    const handle = yield* ChildProcess.make(spec.command, spec.arguments, { stdin: 'ignore', stdout: 'ignore', stderr: 'inherit', env: spec.environment, extendEnv: true });
    yield* releasing(handle.kill());
    const exited = Effect.map(
        Effect.result(handle.exitCode),
        Result.match({
            onSuccess: (status): typeof State.cases.Exited.Type => ({ _tag: 'Exited', status }),
            onFailure: (error): typeof State.cases.Exited.Type => ({
                _tag: 'Exited',
                ...Record.getSomes({ signal: Option.flatMap(Schema.decodeUnknownOption(Interrupted)(error.cause), ({ message: [, name] }) => Record.get(SIGNALS, name)) }),
            }),
        }),
    );
    const alive: typeof State.Type = { _tag: 'Responding', elements: [], windows: [], answering: false, created: true };
    return { identifier: chosen.identifier, pid: handle.pid, state: alive, at: yield* DateTime.now, shared: [], watch: Option.none<number>(), rows: Stream.fromEffect(Effect.zipWith(exited, DateTime.now, (state, at) => ({ identifier: chosen.identifier, pid: handle.pid, state, at }))) };
});
class Launched extends Data.TaggedClass('Launched')<{
    readonly fresh?: boolean;
    readonly documents?: readonly string[];
    readonly arguments?: readonly string[];
    readonly defaults?: Record.ReadonlyRecord<string, string>;
    readonly environment?: Record.ReadonlyRecord<string, string>;
}> {
    readonly start = (chosen: Chosen, passing: readonly Progress[], releasing: Releasing) => launching(this, chosen, passing, releasing);
}
class Spawned extends Data.TaggedClass('Spawned')<{ readonly command: string; readonly arguments: readonly string[]; readonly environment: Record.ReadonlyRecord<string, string> }> {
    readonly start = (chosen: Chosen, _: readonly Progress[], releasing: Releasing) => spawning(this, chosen, releasing);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Asked, appeared, checked, Launched, Observation, output, Progress, plist, printed, Spawned, System, sampled, stalled, starts, watched };
