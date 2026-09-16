// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Cause, Clock, Context, Crypto, Deferred, Duration, Effect, Exit, flow, identity, Layer, Match, Option, Path, Queue, Record, Ref, Result, Schema, String } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/unstable/process';
import { BridgeError } from './errors.ts';
import { Hosts } from './hosts.ts';
import { reply } from './osascript.ts';
import { type HostId, JobId, LOAD_CEILING, PROBE_MS, QUEUE_DEPTH } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface InFlight {
    readonly jobId: JobId;
    readonly startedAt: number;
}

interface Process {
    readonly pid: number;
    readonly cpu: number;
    readonly command: string;
}

interface Activity {
    readonly inFlight: Option.Option<InFlight>;
    readonly wedged: Option.Option<{ readonly jobId: JobId; readonly at: number }>;
    readonly lastSuccessAt: Option.Option<number>;
    readonly lastError: Option.Option<{ readonly error: BridgeError; readonly count: number; readonly at: number }>;
}

interface Entry {
    readonly jobId: JobId;
    readonly deadlineAt: number;
    readonly settle: typeof _recorded;
    readonly abandoned: Effect.Effect<boolean>;
    readonly refuse: (error: BridgeError) => Effect.Effect<boolean>;
    readonly perform: (remaining: Duration.Duration) => Effect.Effect<Exit.Exit<unknown, BridgeError>>;
}

interface Host {
    readonly id: HostId;
    readonly processName: string;
    readonly queue: Queue.Queue<Entry>;
    readonly activity: Ref.Ref<Activity>;
}

type Jobs = Readonly<Record<HostId, Host>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _IDLE: Activity = { inFlight: Option.none(), wedged: Option.none(), lastSuccessAt: Option.none(), lastError: Option.none() };

const _PROCESSES = ChildProcess.make('ps', ['-A', '-o', 'pid=,%cpu=,comm=']);

// --- [MODELS] --------------------------------------------------------------------------

const InFlight: Schema.Codec<InFlight, unknown> = Schema.Struct({ jobId: JobId, startedAt: Schema.Number });

const Process: Schema.Codec<Process, unknown> = Schema.Struct({ pid: Schema.Int, cpu: Schema.Number, command: Schema.String });

const _Column = Schema.Struct({ pid: Schema.NumberFromString, cpu: Schema.NumberFromString, command: Schema.String });
const _column: (line: unknown) => Option.Option<Process> = Schema.decodeUnknownOption(_Column);

// --- [SERVICES] ------------------------------------------------------------------------

const Jobs: Context.Service<Jobs, Jobs> = Context.Service<Jobs>('Jobs');

// --- [STATE] ---------------------------------------------------------------------------

const _counted = (previous: Activity['lastError'], error: BridgeError): number => Option.match(previous, { onNone: () => 1, onSome: (last) => (last.error._tag === error._tag ? last.count + 1 : 1) });

const _recorded = (jobs: Activity, exit: Exit.Exit<unknown, BridgeError>, at: number): Activity =>
    Exit.match(exit, {
        onSuccess: () => ({ ...jobs, lastSuccessAt: Option.some(at) }),
        onFailure: (cause) =>
            Option.match(Cause.findErrorOption(cause), {
                onNone: () => jobs,
                onSome: (error) => ({ ...jobs, lastError: Option.some({ error, at, count: _counted(jobs.lastError, error) }) }),
            }),
    });

const _wedged = (jobs: Activity, exit: Exit.Exit<unknown, BridgeError>, jobId: JobId, at: number): Activity['wedged'] =>
    Option.match(Exit.match(exit, { onSuccess: Option.none, onFailure: Cause.findErrorOption }), {
        onNone: () => Exit.match(exit, { onSuccess: Option.none, onFailure: () => jobs.wedged }),
        onSome: (error) =>
            Match.value(error).pipe(
                Match.withReturnType<Activity['wedged']>(),
                Match.tag('deadlineExceeded', 'hostUnresponsive', () => Option.some({ jobId, at })),
                Match.tag('hostThrew', 'hostRejected', 'scriptNotCompiled', 'resultNotDecodable', () => Option.none()),
                Match.orElse(() => jobs.wedged),
            ),
    });

const _transition = (jobs: Ref.Ref<Activity>, step: (state: Activity, at: number) => Activity): Effect.Effect<void> =>
    Effect.flatMap(Clock.currentTimeMillis, (at) => Ref.update(jobs, (state) => step(state, at)));

// --- [LIVENESS] ------------------------------------------------------------------------

const _split = (text: string): readonly [string, string] =>
    Option.match(String.indexOf(' ')(text), { onNone: () => [text, ''], onSome: (index) => [text.slice(0, index), String.trim(text.slice(index))] });

const _process = (line: string): Option.Option<Process> => {
    const [pid, rest] = _split(String.trim(line));
    const [cpu, command] = _split(rest);
    return _column({ pid, cpu, command });
};

const liveness: (host: Host) => Effect.Effect<Process, BridgeError, ChildProcessSpawner.ChildProcessSpawner | Path.Path> = Effect.fnUntraced(function* (host: Host) {
    const path = yield* Path.Path;
    const listing = yield* Effect.orDie(reply(_PROCESSES));
    const found = Array.findFirst(String.linesIterator(listing), (line) => Option.filter(_process(line), (process) => path.basename(process.command) === host.processName));
    return yield* Option.match(found, {
        onNone: () => Effect.fail(BridgeError.cases.hostNotRunning.make({ host: host.id })),
        onSome: (process) => (process.cpu >= LOAD_CEILING ? Effect.fail(BridgeError.cases.hostSaturated.make({ host: host.id, pid: process.pid, cpu: process.cpu })) : Effect.succeed(process)),
    });
});

// --- [WORKER] --------------------------------------------------------------------------

const _dispatch = Effect.fnUntraced(function* (host: Host, entry: Entry) {
    const abandoned = yield* entry.abandoned;
    if (abandoned) {
        return;
    }
    const process = yield* Effect.result(liveness(host));
    const at = yield* Clock.currentTimeMillis;
    const remaining = entry.deadlineAt - at;
    const admitted = Result.flatMap(process, () => (remaining > 0 ? Result.void : Result.fail(BridgeError.cases.deadlineExceeded.make({ host: host.id, jobId: entry.jobId }))));
    const exit = yield* Result.match(admitted, {
        onFailure: (error) => Effect.as(entry.refuse(error), Exit.fail(error)),
        onSuccess: () =>
            Effect.andThen(
                Ref.update(host.activity, (state) => ({ ...state, inFlight: Option.some({ jobId: entry.jobId, startedAt: at }) })),
                entry.perform(Duration.millis(remaining)),
            ),
    });
    yield* _transition(host.activity, (state, now) => ({ ...entry.settle(state, exit, now), inFlight: Option.none(), wedged: _wedged(state, exit, entry.jobId, now) }));
});

const _worker = (host: Host): Effect.Effect<never, never, ChildProcessSpawner.ChildProcessSpawner | Path.Path> =>
    Effect.forever(Effect.flatMap(Queue.take(host.queue), (entry) => _dispatch(host, entry)));

const _host = (id: HostId, processName: string): Effect.Effect<Host> =>
    Effect.map(Effect.all([Queue.bounded<Entry>(QUEUE_DEPTH), Ref.make<Activity>(_IDLE)]), ([queue, activity]) => ({ id, processName, queue, activity }));

const layer: Layer.Layer<Jobs, never, Hosts | ChildProcessSpawner.ChildProcessSpawner | Path.Path> = Layer.effect(
    Jobs,
    Effect.gen(function* () {
        const hosts = yield* Hosts;
        const jobs = yield* Effect.all(Record.map(hosts, (row) => _host(row.id, row.processName)));
        yield* Effect.forEach(Record.values(jobs), flow(_worker, Effect.forkScoped));
        return jobs;
    }),
);

// --- [RUN] -----------------------------------------------------------------------------

const _job = Effect.fnUntraced(function* <A, R>(host: Host, timeoutMs: number, settle: typeof _recorded, work: (jobId: JobId) => Effect.Effect<A, BridgeError, R>) {
    const jobId = yield* Effect.orDie(Crypto.Crypto.use((crypto) => Effect.flatMap(crypto.randomUUIDv4, Schema.decodeEffect(JobId))));
    const context = yield* Effect.context<R>();
    const settled = yield* Deferred.make<A, BridgeError>();
    const startedAt = yield* Clock.currentTimeMillis;
    const deadline = BridgeError.cases.deadlineExceeded.make({ host: host.id, jobId });
    const entry: Entry = {
        jobId,
        settle,
        deadlineAt: startedAt + timeoutMs,
        abandoned: Deferred.isDone(settled),
        refuse: (error) => Deferred.fail(settled, error),
        perform: (remaining) =>
            Effect.flatMap(Effect.exit(Effect.timeoutOrElse(Effect.provideContext(work(jobId), context), { duration: remaining, orElse: () => Effect.fail(deadline) })), (exit) =>
                Effect.as(Deferred.done(settled, exit), exit),
            ),
    };
    return yield* Effect.timeoutOrElse(Effect.andThen(Queue.offer(host.queue, entry), Deferred.await(settled)), {
        duration: timeoutMs,
        orElse: () => Effect.andThen(Deferred.fail(settled, deadline), Effect.fail(deadline)),
    }).pipe(Effect.onInterrupt(() => Deferred.interrupt(settled)));
});

const run = <A, R>(host: Host, timeoutMs: number, work: (jobId: JobId) => Effect.Effect<A, BridgeError, R>): Effect.Effect<A, BridgeError, R | Crypto.Crypto> => _job(host, timeoutMs, _recorded, work);

const probe = <A, R>(host: Host, work: (jobId: JobId) => Effect.Effect<A, BridgeError, R>): Effect.Effect<Result.Result<A, BridgeError>, never, R | Crypto.Crypto> =>
    Effect.flatMap(Ref.get(host.activity), (state) =>
        Option.match(state.inFlight, {
            onNone: () => Effect.result(_job(host, PROBE_MS, identity, work)),
            onSome: (running) => Effect.succeed(Result.fail(BridgeError.cases.hostBusy.make({ host: host.id, jobId: running.jobId, startedAt: running.startedAt }))),
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { InFlight, Jobs, layer, liveness, Process, probe, run };
