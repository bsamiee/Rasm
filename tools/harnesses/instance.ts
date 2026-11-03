import { constants } from 'node:os';
import { fileURLToPath } from 'node:url';
import { State } from '@rasm/automation/applications';
import { Array, ByteSize, DateTime, Duration, Effect, FileSystem, flow, identity, Match, Option, Path, Record, Result, Schema, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { HarnessError } from '#/error';
import type { Crash, Dialog } from '#/results';

// --- [TYPES] ---------------------------------------------------------------------------

type Exited = Extract<typeof State.Type, { readonly _tag: 'Exited' }>;
type Element = Extract<typeof State.Type, { readonly _tag: 'Responding' }>['elements'][number];
type Control = NonNullable<Element['detail']>['controls'][number];
interface Row {
    readonly identifier: string;
    readonly pid: number;
    readonly state: typeof State.Type;
}
interface Launch {
    readonly app: string;
    readonly fresh?: boolean;
    readonly documents?: readonly string[];
    readonly arguments?: readonly string[];
    readonly defaults?: Record.ReadonlyRecord<string, string>;
    readonly environment?: Record.ReadonlyRecord<string, string>;
}
interface Spawned {
    readonly _tag: 'Spawned';
    readonly command: string;
    readonly arguments: readonly string[];
    readonly environment: Record.ReadonlyRecord<string, string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPORTING = (['SIGQUIT', 'SIGILL', 'SIGTRAP', 'SIGABRT', 'SIGFPE', 'SIGBUS', 'SIGSEGV', 'SIGSYS'] as const).map((name) => constants.signals[name]);
const SIGNALS: Record.ReadonlyRecord<string, number> = constants.signals;
const RUNNABLE = 'R';
const ADOBE_CRASH_REPORTER = 'com.adobe.crashreporter';
const MAIN_THREAD = /^\s*\d+ Thread_\S+\s+DispatchQueue_1: com\.apple\.main-thread\b/u;
const FRAME = /^(?<depth>[ +!:|]*)\d+ (?<symbol>.+?) {2}(?:\(in (?<image>[^)]+)\)|\[)/u;
const SPACES = /\s+/u;

// --- [MODELS] --------------------------------------------------------------------------

const Interrupted = Schema.Struct({ message: Schema.TemplateLiteralParser(["Process interrupted due to receipt of signal: '", Schema.String, "'"]) });
const Instances = Schema.Record(Schema.String, Schema.Record(Schema.String, State));
const Failure = Schema.Record(Schema.String, Schema.Struct({ fail: Schema.Struct({ reason: Schema.String }) }));
const System = Schema.Struct({ system: Schema.Struct({ memory: Schema.Literals(['normal', 'warning', 'critical']), thermal: Schema.Literals(['nominal', 'fair', 'serious', 'critical']) }) });
const Line = Schema.fromJsonString(Schema.toCodecJson(Schema.Union([Instances, Failure, System])));
const Ips = Schema.fromJsonString(
    Schema.Struct({
        exception: Schema.optionalKey(Schema.Struct({ type: Schema.String, signal: Schema.optionalKey(Schema.String) })),
        faultingThread: Schema.optionalKey(Schema.Int),
        threads: Schema.optionalKey(Schema.Array(Schema.Struct({ frames: Schema.Array(Schema.Struct({ imageIndex: Schema.Int, imageOffset: Schema.Int, symbol: Schema.optionalKey(Schema.String) })) }))),
        usedImages: Schema.optionalKey(Schema.Array(Schema.Struct({ name: Schema.optionalKey(Schema.String) }))),
    }),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PROCESSES]
const output = Effect.fn('output')(function* (command: string, args: readonly string[]) {
    const handle = yield* ChildProcess.make(command, args, { stdin: 'ignore' });
    const [stdout, stderr, code] = yield* Effect.all([Stream.mkUint8Array(handle.stdout), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return { stdout, stderr, code };
}, Effect.scoped);
const checked = Effect.fn('checked')(function* (command: string, args: readonly string[]) {
    const { stdout, stderr, code } = yield* output(command, args);
    return code === 0 ? stdout : yield* HarnessError.unavailable([command, ...args].join(' '))(`exited with code ${code}: ${stderr}`);
});
const printed = (command: string, args: readonly string[]) => Effect.map(checked(command, args), (bytes) => new TextDecoder().decode(bytes));
const startOf = (line: string) =>
    Array.matchLeft(line.trim().split(SPACES), {
        onEmpty: Option.none,
        onNonEmpty: (pid, started) => Option.zipWith(Schema.decodeOption(Schema.FiniteFromString)(pid), DateTime.make(started.join(' ')), (held, at) => [held, at] as const),
    });
const starts = (selected: readonly string[]) => Effect.map(printed('ps', ['-o', 'pid=,lstart=', ...selected]), (text) => new Map(Array.getSomes(text.split('\n').map(startOf))));
const plist = Effect.fn('plist')(function* <S extends Schema.Top>(app: string, schema: S) {
    const path = yield* Path.Path;
    return yield* Schema.decodeEffect(Schema.fromJsonString(schema))(yield* printed('plutil', ['-convert', 'json', '-o', '-', path.join(app, 'Contents', 'Info.plist')]));
});

// --- [FILES]
const watched = (path: string) =>
    Stream.unwrap(
        Effect.map(FileSystem.FileSystem, (fs) =>
            Stream.concat(
                Stream.make(undefined),
                Stream.map(fs.watch(path), () => undefined),
            ),
        ),
    );
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

// --- [LINES]
const settled = (state: typeof State.Type): state is Exited => State.guards.Exited(state) && (state.status !== undefined || state.report !== undefined || state.reportFailure !== undefined || (state.signal !== undefined && !REPORTING.includes(state.signal)));
const listed = (line: typeof Instances.Type): readonly Row[] => Record.collect(line, (identifier, instances) => Record.collect(instances, (pid, state) => ({ identifier, pid: Number(pid), state }))).flat();
const decoded = (text: string): Effect.Effect<readonly Row[], HarnessError | Schema.SchemaError> =>
    Effect.flatMap(Schema.decodeEffect(Line)(text), (line) =>
        Match.value(line).pipe(
            Match.when(Schema.is(System), () => Effect.succeed([])),
            Match.when(Schema.is(Failure), (failed) => HarnessError.unavailable('watch-apps')(Record.collect(failed, (key, { fail }) => `${key}: ${fail.reason}`).join(', '))),
            Match.orElse(flow(listed, Effect.succeed)),
        ),
    );
const rows = (command: readonly string[]) => Effect.map(ChildProcess.make('watch-apps', command, { stdin: 'ignore', stderr: 'inherit' }), (handle) => handle.stdout.pipe(Stream.decodeText, Stream.splitLines, Stream.mapEffect(decoded), Stream.flattenIterable));

// --- [INSTANCES]
const Launch = Effect.fn('Launch')(function* (spec: Launch) {
    const flags = [
        ...(spec.fresh === true ? ['--new'] : []),
        ...(spec.documents ?? []).map((file) => `--document=${file}`),
        ...(spec.arguments ?? []).map((value) => `--arguments=${value}`),
        ...Record.collect(spec.defaults ?? {}, (key, value) => [`--arguments=-${key}`, `--arguments=${value}`]).flat(),
        ...Record.collect(spec.environment ?? {}, (name, value) => `--environment=${name}=${value}`),
    ];
    const states = yield* Stream.runCollect(Stream.takeUntil(yield* rows(['launch', spec.app, ...flags]), ({ state }) => !State.guards.Launching(state) && (!State.guards.Exited(state) || settled(state))));
    return yield* Option.match(
        Option.filter(Array.last(states), ({ state }) => !State.guards.Launching(state)),
        {
            onNone: () => HarnessError.unavailable(spec.app)(`launch ended at ${states.map(({ pid, state }) => `${pid} ${state._tag}`).join(', ') || 'no state'}`),
            onSome: Effect.succeed,
        },
    );
}, Effect.scoped);
const spawned = Effect.fn('spawned')(function* (identifier: string, spec: Spawned) {
    const handle = yield* ChildProcess.make(spec.command, spec.arguments, { stdin: 'ignore', stdout: 'ignore', stderr: 'inherit', env: spec.environment, extendEnv: true });
    const exited = Effect.map(
        Effect.result(handle.exitCode),
        Result.match({
            onSuccess: (status): Exited => ({ _tag: 'Exited', status }),
            onFailure: (error): Exited => ({
                _tag: 'Exited',
                ...Option.match(
                    Option.flatMap(Schema.decodeUnknownOption(Interrupted)(error.cause), ({ message: [, name] }) => Record.get(SIGNALS, name)),
                    { onNone: () => ({}), onSome: (signal) => ({ signal }) },
                ),
            }),
        }),
    );
    const alive: typeof State.Type = { _tag: 'Responding', elements: [], windows: [], answering: false };
    return { identifier, pid: handle.pid, state: alive, rows: Stream.fromEffect(Effect.map(exited, (state): Row => ({ identifier, pid: handle.pid, state }))) };
});
const read = (names: Array.NonEmptyReadonlyArray<string>) => Effect.scoped(Effect.flatMap(rows(['read', ...names]), Stream.runCollect));
const watch = (names: Array.NonEmptyReadonlyArray<string>, pid: number) => Stream.filter(Stream.unwrap(rows(['watch', ...names])), (row) => row.identifier !== names[0] || row.pid === pid);
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

// --- [DIALOGS]
const pressing = (element: Element): readonly Control[] => (element.detail?.controls ?? []).filter(({ actions }) => actions.includes('AXPress'));
const pressed = (element: Element, label: string): Option.Option<Control> => Array.findFirst(pressing(element), (control) => control.label === label);
const perform = (element: Control['id']) => Effect.scoped(Effect.flatMap(rows(['perform', element, 'AXPress']), Stream.runDrain));
const dialogOf = (element: Element, pid: number): typeof Dialog.Type => ({
    ...(element.title === undefined ? {} : { title: element.title }),
    text: element.detail?.text ?? [],
    buttons: Array.getSomes(pressing(element).map(({ label }) => Option.fromUndefinedOr(label))),
    pid,
});
const shown = (dialog: typeof Dialog.Type): string => `instance ${dialog.pid} shows ${dialog.title ?? 'a dialog'}${dialog.text.length === 0 ? '' : ` "${dialog.text.join(' ')}"`}${dialog.buttons.length === 0 ? '' : ` with ${dialog.buttons.join(', ')}`}, left running`;

// --- [CRASHES]
const crashed = Effect.fn('crashed')(function* (exit: Exited) {
    const fs = yield* FileSystem.FileSystem;
    const report = Option.map(Option.fromUndefinedOr(exit.report), ({ url }) => fileURLToPath(url));
    const ips = yield* Option.match(report, {
        onNone: () => Effect.succeedNone,
        onSome: (file) => Effect.option(Effect.flatMap(fs.readFileString(file), (text) => Schema.decodeEffect(Ips)(text.slice(text.indexOf('\n') + 1)))),
    });
    const thread = Option.flatMap(ips, ({ faultingThread, threads }) => Array.get(threads ?? [], faultingThread ?? 0));
    return {
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
    } satisfies typeof Crash.Type;
});
const described = (state: typeof State.Type): string =>
    State.match(state, {
        Launching: () => 'launching',
        Responding: ({ answering }) => `responding${answering ? ' with a dialog requiring an answer' : ''}`,
        Unresponsive: ({ process, usage, windows }) =>
            `unresponsive ${process}${usage === undefined ? '' : ` cpu ${Duration.format(usage.cpu)} footprint ${ByteSize.format(usage.footprint)}`}${windows.length === 0 ? '' : `, windows ${windows.map(({ number, title }) => (title === undefined ? String(number) : `${number} "${title}"`)).join(', ')}`}`,
        Failed: ({ reason }) => `status failed: ${reason}`,
        Exited: ({ signal, status, report, reportFailure }) => `exited with ${signal === undefined ? `status ${status ?? 'unread'}` : `signal ${signal}`}${report === undefined ? '' : `, report ${report.url.href}`}${reportFailure === undefined ? '' : `, ${reportFailure}`}`,
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { ADOBE_CRASH_REPORTER, appeared, busy, type Control, checked, crashed, described, dialogOf, type Element, type Exited, Launch, output, perform, plist, pressed, printed, type Row, read, type Spawned, sampled, settled, shown, spawned, stalled, starts, watch, watched };
