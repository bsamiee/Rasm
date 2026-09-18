// --- [IMPORTS] -------------------------------------------------------------------------

import {
    Array,
    Boolean,
    Cause,
    Clock,
    Context,
    Crypto,
    Deferred,
    Duration,
    Effect,
    Encoding,
    Exit,
    FileSystem,
    Filter,
    flow,
    Layer,
    Match,
    Option,
    Path,
    Queue,
    Record,
    Ref,
    Result,
    Schema,
    String,
    Struct,
} from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/unstable/process';
import { BridgeError, exited, inaccessible } from './errors.ts';
import { Hosts, installed } from './hosts.ts';
import { reply } from './osascript.ts';
import { AbsolutePath, ARTIFACTS, type HostId, JobId, LOAD_CEILING, PROBE_MS, QUEUE_DEPTH } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type InFlight = (typeof InFlight)['Type'];
type Process = (typeof Process)['Type'];
type Spilled = (typeof Spilled)['Type'];

interface Activity {
    readonly inFlight: Option.Option<InFlight>;
    readonly wedged: Option.Option<InFlight>;
    readonly lastSuccessAt: Option.Option<number>;
    readonly lastError: Option.Option<{ readonly error: BridgeError; readonly count: number; readonly at: number }>;
}

interface Entry {
    readonly jobId: JobId;
    readonly deadlineAt: number;
    readonly pending: Effect.Effect<boolean>;
    readonly refuse: (error: BridgeError) => Effect.Effect<Exit.Exit<unknown, BridgeError>>;
    readonly perform: (remaining: Duration.Duration) => Effect.Effect<Exit.Exit<unknown, BridgeError>>;
}

interface Host {
    readonly id: HostId;
    readonly resolved: Hosts[HostId];
    readonly queue: Queue.Queue<Entry>;
    readonly activity: Ref.Ref<Activity>;
}

type Jobs = Readonly<Record<HostId, Host>>;

type Settle = (state: Activity, exit: Exit.Exit<unknown, BridgeError>, jobId: JobId, at: number) => Activity;

// --- [CONSTANTS] -----------------------------------------------------------------------

const SPILL_CHARS = 200_000;
const _PROCESS = /^\s*(?<pid>\d+)\s+(?<cpu>\d+(?:\.\d+)?)\s+(?<command>.+)$/u;

// --- [MODELS] --------------------------------------------------------------------------

const InFlight: Schema.Struct<{ readonly jobId: Schema.Codec<JobId, string>; readonly startedAt: Schema.Number }> = Schema.Struct({ jobId: JobId, startedAt: Schema.Number });

const Spilled: Schema.Struct<{ readonly kind: Schema.Literal<'file'>; readonly path: typeof AbsolutePath; readonly bytes: Schema.Int; readonly sha256: Schema.String }> = Schema.Struct({
    kind: Schema.Literal('file'),
    path: AbsolutePath,
    bytes: Schema.Int,
    sha256: Schema.String,
});

const Process: Schema.Struct<{ readonly pid: Schema.Int; readonly cpu: Schema.Number; readonly command: Schema.String }> = Schema.Struct({
    pid: Schema.Int,
    cpu: Schema.Number,
    command: Schema.String,
});

const _listing = Schema.decodeUnknownEffect(Schema.Array(Schema.Struct({ pid: Schema.NumberFromString, cpu: Schema.NumberFromString, command: Schema.String })));

// --- [SERVICES] ------------------------------------------------------------------------

const Jobs: Context.Service<Jobs, Jobs> = Context.Service<Jobs>('Jobs');

// --- [PLACES] --------------------------------------------------------------------------

const root: Effect.Effect<string, never, Path.Path> = Effect.map(Path.Path, (path) => path.resolve(import.meta.dirname, '..', '..', '..'));

const artifacts = (...segments: readonly string[]): Effect.Effect<string, never, Path.Path> =>
    Effect.map(Effect.all([Path.Path, root]), ([path, base]) => path.join(base, ARTIFACTS, 'creative-cloud', ...segments));

// --- [STATE] ---------------------------------------------------------------------------

const _settled: Settle = (state, exit, jobId, at) =>
    Exit.match(exit, {
        onSuccess: () => ({ ...state, inFlight: Option.none(), wedged: Option.none(), lastSuccessAt: Option.some(at) }),
        onFailure: flow(
            Cause.findErrorOption,
            Option.match({
                onNone: () => ({ ...state, inFlight: Option.none() }),
                onSome: (error) => ({
                    ...state,
                    inFlight: Option.none(),
                    wedged: Match.value(error).pipe(
                        Match.withReturnType<Option.Option<InFlight>>(),
                        Match.tag('deadlineExceeded', 'hostUnresponsive', () => Option.some({ jobId, startedAt: at })),
                        Match.tag('hostThrew', 'hostRejected', 'scriptNotCompiled', 'resultNotDecodable', () => Option.none()),
                        Match.tag(
                            'hostNotInstalled',
                            'hostNotRunning',
                            'automationDenied',
                            'hostNotAttached',
                            'portNotBound',
                            'hostBusy',
                            'hostSaturated',
                            'pluginDetached',
                            'fileNotAccessible',
                            'commandFailed',
                            () => state.wedged,
                        ),
                        Match.exhaustive,
                    ),
                    lastError: Option.some({ error, at, count: Option.match(state.lastError, { onNone: () => 1, onSome: (last) => (last.error._tag === error._tag ? last.count + 1 : 1) }) }),
                }),
            }),
        ),
    });

const _probed: Settle = (state, exit, jobId, at) => ({ ...state, inFlight: Option.none(), wedged: _settled(state, exit, jobId, at).wedged });

// --- [LIVENESS] ------------------------------------------------------------------------

const liveness: (host: Pick<Host, 'id' | 'resolved'>) => Effect.Effect<Process, BridgeError, ChildProcessSpawner.ChildProcessSpawner | Path.Path> = Effect.fnUntraced(function* (
    host: Pick<Host, 'id' | 'resolved'>,
) {
    const path = yield* Path.Path;
    const { executable } = yield* installed(host.id, host.resolved);
    const listing = yield* Effect.mapError(reply(ChildProcess.make('ps', ['-A', '-o', 'pid=,%cpu=,comm='])), exited(host.id));
    const processes = yield* Effect.orDie(_listing(Array.filterMap(String.linesIterator(listing), Filter.fromPredicateOption(flow(String.match(_PROCESS), Option.map(Struct.get('groups')))))));
    return yield* Array.findFirst(processes, (process) => path.basename(process.command) === executable).pipe(
        Effect.fromOption(() => BridgeError.cases.hostNotRunning.make({ host: host.id })),
        Effect.filterOrFail(
            (process) => process.cpu < LOAD_CEILING,
            (process) => BridgeError.cases.hostSaturated.make({ host: host.id, pid: process.pid, cpu: process.cpu }),
        ),
    );
});

const processId = (host: Pick<Host, 'id' | 'resolved'>): Effect.Effect<Option.Option<number>, BridgeError, ChildProcessSpawner.ChildProcessSpawner | Path.Path> =>
    liveness(host).pipe(
        Effect.map((found) => Option.some(found.pid)),
        Effect.catchTag('hostNotRunning', () => Effect.succeedNone),
        Effect.catchTag('hostSaturated', ({ pid }) => Effect.succeed(Option.some(pid))),
    );

// --- [WORKER] --------------------------------------------------------------------------

const _dispatch = Effect.fnUntraced(function* (host: Host, entry: Entry) {
    const admitted = yield* Effect.result(
        Effect.filterOrFail(
            Effect.andThen(liveness(host), Clock.currentTimeMillis),
            (at) => at < entry.deadlineAt,
            () => BridgeError.cases.deadlineExceeded.make({ host: host.id, jobId: entry.jobId }),
        ),
    );
    yield* Result.match(admitted, {
        onFailure: entry.refuse,
        onSuccess: (at) =>
            Effect.andThen(
                Ref.update(host.activity, (state) => ({ ...state, inFlight: Option.some({ jobId: entry.jobId, startedAt: at }) })),
                entry.perform(Duration.millis(entry.deadlineAt - at)),
            ),
    });
});

const layer: Layer.Layer<Jobs, never, Hosts | ChildProcessSpawner.ChildProcessSpawner | Path.Path> = Layer.effect(
    Jobs,
    Effect.gen(function* () {
        const hosts = yield* Hosts;
        const jobs = yield* Effect.all(
            Record.map(hosts, (resolved, id) =>
                Effect.map(
                    Effect.all([Queue.bounded<Entry>(QUEUE_DEPTH), Ref.make<Activity>({ inFlight: Option.none(), wedged: Option.none(), lastSuccessAt: Option.none(), lastError: Option.none() })]),
                    ([queue, activity]): Host => ({ id, resolved, queue, activity }),
                ),
            ),
        );
        yield* Effect.forEach(Record.values(jobs), (host) => Effect.forkScoped(Effect.forever(Effect.flatMap(Queue.take(host.queue), (entry) => Effect.when(_dispatch(host, entry), entry.pending)))), {
            discard: true,
        });
        return jobs;
    }),
);

// --- [RUN] -----------------------------------------------------------------------------

const _job = Effect.fnUntraced(function* <A, R>(host: Host, timeoutMs: number, settle: Settle, work: (jobId: JobId) => Effect.Effect<A, BridgeError, R>) {
    const jobId = yield* Effect.orDie(Crypto.Crypto.use((crypto) => Effect.map(crypto.randomUUIDv4, JobId.make)));
    const context = yield* Effect.context<R>();
    const settled = yield* Deferred.make<A, BridgeError>();
    const startedAt = yield* Clock.currentTimeMillis;
    const deadline = BridgeError.cases.deadlineExceeded.make({ host: host.id, jobId });
    const conclude = (exit: Exit.Exit<A, BridgeError>): Effect.Effect<Exit.Exit<A, BridgeError>> =>
        Effect.andThen(
            Effect.flatMap(Clock.currentTimeMillis, (at) => Ref.update(host.activity, (state) => settle(state, exit, jobId, at))),
            Effect.as(Deferred.done(settled, exit), exit),
        );
    const entry: Entry = {
        jobId,
        deadlineAt: startedAt + timeoutMs,
        pending: Effect.map(Deferred.isDone(settled), Boolean.not),
        refuse: flow(Exit.fail, conclude),
        perform: (remaining) =>
            work(jobId).pipe(Effect.provideContext(context), Effect.timeoutOrElse({ duration: remaining, orElse: () => Effect.fail(deadline) }), Effect.exit, Effect.flatMap(conclude)),
    };
    return yield* Effect.timeoutOrElse(Effect.andThen(Queue.offer(host.queue, entry), Deferred.await(settled)), {
        duration: timeoutMs,
        orElse: () => Effect.andThen(Deferred.fail(settled, deadline), Effect.fail(deadline)),
    }).pipe(Effect.onInterrupt(() => Deferred.interrupt(settled)));
});

const run = <A, R>(host: Host, timeoutMs: number, work: (jobId: JobId) => Effect.Effect<A, BridgeError, R>): Effect.Effect<A, BridgeError, R | Crypto.Crypto> => _job(host, timeoutMs, _settled, work);

const probe = <A, R>(host: Host, work: (jobId: JobId) => Effect.Effect<A, BridgeError, R>): Effect.Effect<Result.Result<A, BridgeError>, never, R | Crypto.Crypto> =>
    Effect.result(
        Effect.flatMap(Ref.get(host.activity), (state) =>
            Option.match(state.inFlight, {
                onNone: () => _job(host, PROBE_MS, _probed, work),
                onSome: (running) => Effect.fail(BridgeError.cases.hostBusy.make({ host: host.id, jobId: running.jobId, startedAt: running.startedAt })),
            }),
        ),
    );

// --- [SPILL] ---------------------------------------------------------------------------

const _spilled = Effect.fnUntraced(function* (host: HostId, jobId: JobId, text: string) {
    const path = yield* Path.Path;
    const fs = yield* FileSystem.FileSystem;
    const directory = yield* artifacts(host, 'results');
    const file = AbsolutePath.make(path.join(directory, `${jobId}.json`));
    const bytes = new TextEncoder().encode(text);
    yield* Effect.andThen(fs.makeDirectory(directory, { recursive: true }), fs.writeFile(file, bytes)).pipe(Effect.mapError(inaccessible(host)));
    const digest = yield* Crypto.Crypto.use((crypto) => crypto.digest('SHA-256', bytes)).pipe(Effect.mapError(inaccessible(host)));
    return Spilled.make({ kind: 'file', path: file, bytes: bytes.length, sha256: Encoding.encodeHex(digest) });
});

const spill = <A>(host: HostId, jobId: JobId, value: A): Effect.Effect<A | Spilled, BridgeError, Crypto.Crypto | FileSystem.FileSystem | Path.Path> =>
    Option.match(
        Option.liftPredicate(JSON.stringify(value), (text) => text.length > SPILL_CHARS),
        { onNone: () => Effect.succeed(value), onSome: (text) => _spilled(host, jobId, text) },
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Host };
export { artifacts, InFlight, Jobs, layer, liveness, Process, probe, processId, root, run, Spilled, spill };
