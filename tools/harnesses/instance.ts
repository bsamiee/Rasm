import { constants } from 'node:os';
import { State } from '@rasm/automation/applications';
import { Array, ByteSize, Data, DateTime, Duration, Effect, Equivalence, FileSystem, flow, identity, Match, Option, Path, Record, Result, Schema, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { HarnessError } from '#/error';
import type { Halted } from '#/results';
import type { Installed } from '#/session';

// --- [TYPES] ---------------------------------------------------------------------------

type Element = (typeof State.cases.Responding.Type)['elements'][number];
type Chosen = Pick<Installed, 'identifier' | 'app' | 'watched'>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPORTING = (['SIGQUIT', 'SIGILL', 'SIGTRAP', 'SIGABRT', 'SIGFPE', 'SIGBUS', 'SIGSEGV', 'SIGSYS'] as const).map((name) => constants.signals[name]);
const SIGNALS: Record.ReadonlyRecord<string, number> = constants.signals;
const RUNNABLE = 'R';
const MAIN_THREAD = /^\s*\d+ Thread_\S+\s+DispatchQueue_1: com\.apple\.main-thread\b/u;
const FRAME = /^(?<depth>[ +!:|]*)\d+ (?<symbol>.+?) {2}(?:\(in (?<image>[^)]+)\)|\[)/u;
const SPACES = /\s+/u;
const LABELLED = Array.makeEquivalence(Equivalence.String);

// --- [MODELS] --------------------------------------------------------------------------

const Interrupted = Schema.Struct({ message: Schema.TemplateLiteralParser(["Process interrupted due to receipt of signal: '", Schema.String, "'"]) });
const Instances = Schema.Record(Schema.String, Schema.Record(Schema.String, State));
const Failure = Schema.Record(Schema.String, Schema.Struct({ fail: Schema.Struct({ reason: Schema.String }) }));
const System = Schema.Struct({ system: Schema.Struct({ memory: Schema.Literals(['normal', 'warning', 'critical']), thermal: Schema.Literals(['nominal', 'fair', 'serious', 'critical']) }) });
const Line = Schema.fromJsonString(Schema.toCodecJson(Schema.Union([Instances, Failure, System])));

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
    const lines = (yield* (yield* FileSystem.FileSystem).readFileString(sample)).split('\n');
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
const busy = (pid: number) =>
    Effect.map(
        ChildProcessSpawner.ChildProcessSpawner.use((spawner) => spawner.lines(ChildProcess.make('ps', ['-o', 'stat=', '-p', String(pid)]))),
        (lines) => (lines.some((line) => line.startsWith(RUNNABLE)) ? ('busy' as const) : ('blocked' as const)),
    );

// --- [FILES]
const watched = (path: string) => Stream.unwrap(Effect.map(FileSystem.FileSystem, (fs) => Stream.prepend(Stream.as(fs.watch(path), undefined), [undefined])));
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
const listed = (line: typeof Instances.Type) => Record.collect(line, (identifier, instances) => Record.collect(instances, (pid, state) => ({ identifier, pid: Number(pid), state }))).flat();
const decoded = (text: string) =>
    Effect.flatMap(Schema.decodeEffect(Line)(text), (line) =>
        Match.value(line).pipe(
            Match.when(Schema.is(System), ({ system }) => Effect.as(Effect.annotateLogs(Effect.logWithLevel(system.memory === 'normal' && (system.thermal === 'nominal' || system.thermal === 'fair') ? 'Info' : 'Warn')('system pressure'), system), [])),
            Match.when(Schema.is(Failure), (failed) => HarnessError.unavailable('watch-apps')(Record.collect(failed, (key, { fail }) => `${key}: ${fail.reason}`).join(', '))),
            Match.orElse(flow(listed, Effect.succeed)),
        ),
    );
const rows = (command: readonly string[]) => Effect.map(ChildProcess.make('watch-apps', command, { stdin: 'ignore', stderr: 'inherit' }), (handle) => handle.stdout.pipe(Stream.decodeText, Stream.splitLines, Stream.mapEffect(decoded), Stream.flattenIterable));
const pressing = (element: Element) => (element.detail?.controls ?? []).filter(({ actions }) => actions.includes('AXPress'));
const labels = (element: Element) => Array.getSomes(pressing(element).map(({ label }) => Option.fromUndefinedOr(label)));

// --- [INSTANCES] -----------------------------------------------------------------------

const Row = Object.assign(Schema.Struct({ identifier: Schema.String, pid: Schema.Int, state: State }), {
    read: (names: Array.NonEmptyReadonlyArray<string>) => Effect.scoped(Effect.flatMap(rows(['read', ...names]), Stream.runCollect)),
    perform: (id: string) => Effect.scoped(Effect.flatMap(rows(['perform', id, 'AXPress']), Stream.runDrain)),
    settled: (state: typeof State.Type): state is typeof State.cases.Exited.Type => State.guards.Exited(state) && (state.status !== undefined || state.report !== undefined || state.reportFailure !== undefined || (state.signal !== undefined && !REPORTING.includes(state.signal))),
    described: (state: typeof State.Type): string =>
        State.match(state, {
            Launching: () => 'launching',
            Responding: ({ answering }) => `responding${answering ? ' with a dialog requiring an answer' : ''}`,
            Unresponsive: ({ process, usage, windows }) =>
                `unresponsive ${process}${usage === undefined ? '' : ` cpu ${Duration.format(usage.cpu)} footprint ${ByteSize.format(usage.footprint)}`}${windows.length === 0 ? '' : `, windows ${windows.map(({ number, title }) => (title === undefined ? String(number) : `${number} "${title}"`)).join(', ')}`}`,
            Failed: ({ reason }) => `status failed: ${reason}`,
            Exited: ({ signal, status, report, reportFailure }) => `exited with ${signal === undefined ? `status ${status ?? 'unread'}` : `signal ${signal}`}${report === undefined ? '' : `, report ${report.url.href}`}${reportFailure === undefined ? '' : `, ${reportFailure}`}`,
        }),
    pressed: (element: Element, label: string) => Array.findFirst(pressing(element), (control) => control.label === label),
    dialog: (element: Element): typeof Halted.cases.Dialog.Type => ({ _tag: 'Dialog', ...(element.title === undefined ? {} : { title: element.title }), text: element.detail?.text ?? [], buttons: labels(element) }),
    shown: (dialog: Pick<typeof Halted.cases.Dialog.Type, 'title' | 'text' | 'buttons'>, pid: number): string => `instance ${pid} shows ${dialog.title ?? 'a dialog'}${dialog.text.length === 0 ? '' : ` "${dialog.text.join(' ')}"`}${dialog.buttons.length === 0 ? '' : ` with ${dialog.buttons.join(', ')}`}, left running`,
});
const launching = Effect.fn('Launched.start')(function* (spec: Launched, chosen: Chosen) {
    const watching = yield* rows(['watch', ...chosen.watched]);
    const flags = [
        ...((spec.fresh ?? true) ? ['--new'] : []),
        ...(spec.documents ?? []).map((file) => `--document=${file}`),
        ...(spec.arguments ?? []).map((value) => `--arguments=${value}`),
        ...Record.collect(spec.defaults ?? {}, (key, value) => [`--arguments=-${key}`, `--arguments=${value}`]).flat(),
        ...Record.collect(spec.environment ?? {}, (name, value) => `--environment=${name}=${value}`),
    ];
    const states = yield* Effect.scoped(Effect.flatMap(rows(['launch', chosen.app, ...flags]), (launch) => Stream.runCollect(Stream.takeUntil(launch, ({ state }) => !State.guards.Launching(state) && (!State.guards.Exited(state) || Row.settled(state))))));
    const last = yield* Option.match(
        Option.filter(Array.last(states), ({ state }) => !State.guards.Launching(state)),
        {
            onNone: () => HarnessError.unavailable(chosen.app)(`launch ended at ${states.map(({ pid, state }) => `${pid} ${state._tag}`).join(', ') || 'no state'}`),
            onSome: Effect.succeed,
        },
    );
    return { ...last, rows: Stream.filter(watching, (row) => row.identifier !== chosen.identifier || (row.pid === last.pid && !State.guards.Launching(row.state))) };
});
const spawning = Effect.fn('Spawned.start')(function* (spec: Spawned, chosen: Chosen) {
    const handle = yield* ChildProcess.make(spec.command, spec.arguments, { stdin: 'ignore', stdout: 'ignore', stderr: 'inherit', env: spec.environment, extendEnv: true });
    const exited = Effect.map(
        Effect.result(handle.exitCode),
        Result.match({
            onSuccess: (status): typeof State.cases.Exited.Type => ({ _tag: 'Exited', status }),
            onFailure: (error): typeof State.cases.Exited.Type => ({
                _tag: 'Exited',
                ...Option.match(
                    Option.flatMap(Schema.decodeUnknownOption(Interrupted)(error.cause), ({ message: [, name] }) => Record.get(SIGNALS, name)),
                    { onNone: () => ({}), onSome: (signal) => ({ signal }) },
                ),
            }),
        }),
    );
    const alive: typeof State.Type = { _tag: 'Responding', elements: [], windows: [], answering: false };
    return { identifier: chosen.identifier, pid: handle.pid, state: alive, rows: Stream.fromEffect(Effect.map(exited, (state) => ({ identifier: chosen.identifier, pid: handle.pid, state }))) };
});
class Progress extends Data.Class<{ readonly title: string; readonly buttons: readonly string[] }> {
    static readonly requiring = (passing: readonly Progress[]) => (element: Element) => element.requiresAnswer && !passing.some((named) => [element.title, ...(element.detail?.text ?? [])].includes(named.title) && LABELLED(named.buttons, labels(element)));
}
class Launched extends Data.TaggedClass('Launched')<{
    readonly fresh?: boolean;
    readonly documents?: readonly string[];
    readonly arguments?: readonly string[];
    readonly defaults?: Record.ReadonlyRecord<string, string>;
    readonly environment?: Record.ReadonlyRecord<string, string>;
}> {
    readonly start = (chosen: Chosen) => launching(this, chosen);
}
class Spawned extends Data.TaggedClass('Spawned')<{ readonly command: string; readonly arguments: readonly string[]; readonly environment: Record.ReadonlyRecord<string, string> }> {
    readonly start = (chosen: Chosen) => spawning(this, chosen);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { appeared, busy, checked, Launched, output, Progress, plist, printed, Row, Spawned, sampled, stalled, starts, watched };
