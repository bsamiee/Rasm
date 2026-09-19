// --- [IMPORTS] -------------------------------------------------------------------------

import {
    Array,
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
    type Result,
    Schema,
    String,
    Struct,
    SubscriptionRef,
    Tuple,
} from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/unstable/process';
import { BridgeError, exited, inaccessible } from './errors.ts';
import type { Activity, InFlight, Request } from './frames.ts';
import { Hosts, installed } from './hosts.ts';
import { reply } from './osascript.ts';
import { AbsolutePath, ARTIFACTS, type HostId, JobId, QUEUE_DEPTH } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Host {
    readonly id: HostId;
    readonly resolved: Hosts[HostId];
    readonly queue: Queue.Queue<Effect.Effect<void, never, ChildProcessSpawner.ChildProcessSpawner | Path.Path>>;
    readonly activity: SubscriptionRef.SubscriptionRef<(typeof Activity)['Type']>;
}

type Jobs = Readonly<Record<HostId, Host>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const SPILL_CHARS = 200_000;
const _PROCESS = /^\s*(?<pid>\d+)\s+(?<cpu>\d+(?:\.\d+)?)\s+(?<command>.+)$/u;

// --- [MODELS] --------------------------------------------------------------------------

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

const _settled = (state: (typeof Activity)['Type'], exit: Exit.Exit<unknown, BridgeError>, jobId: JobId, at: number): (typeof Activity)['Type'] => {
    const running = Option.filter(state.inFlight, (entry) => entry.jobId === jobId);
    const owns = Option.isSome(running);
    const next = { ...state, queueDepth: state.queueDepth - (owns ? 0 : 1), inFlight: owns ? Option.none() : state.inFlight };
    if (Exit.isSuccess(exit)) {
        return { ...next, wedged: Option.none(), lastSuccessAt: Option.some(at) };
    }
    const failure = Cause.findErrorOption(exit.cause);
    if (Option.isNone(failure)) {
        return next;
    }
    const error = failure.value;
    const wedged = Match.value(error).pipe(
        Match.withReturnType<Option.Option<(typeof InFlight)['Type']>>(),
        Match.tag('deadlineExceeded', 'hostUnresponsive', 'scriptTimedOut', () => (owns ? running : state.wedged)),
        Match.tag('hostThrew', 'hostRejected', 'scriptNotCompiled', 'resultNotDecodable', () => Option.none()),
        Match.tag('hostNotInstalled', 'hostNotRunning', 'automationDenied', 'hostNotAttached', 'portNotBound', 'hostBusy', 'pluginDetached', 'fileNotAccessible', 'commandFailed', () => state.wedged),
        Match.exhaustive,
    );
    return {
        ...next,
        wedged,
        lastError: Option.some({ error, at, count: Option.match(state.lastError, { onNone: () => 1, onSome: (last) => (last.error._tag === error._tag ? last.count + 1 : 1) }) }),
    };
};

const _probed: typeof _settled = (state, exit, jobId, at) => ({ ..._settled(state, exit, jobId, at), lastSuccessAt: state.lastSuccessAt, lastError: state.lastError });

// --- [LIVENESS] ------------------------------------------------------------------------

const liveness: (host: Pick<Host, 'id' | 'resolved'>) => Effect.Effect<(typeof Process)['Type'], BridgeError, ChildProcessSpawner.ChildProcessSpawner | Path.Path> = Effect.fnUntraced(function* (
    host: Pick<Host, 'id' | 'resolved'>,
) {
    const path = yield* Path.Path;
    const { bundlePath, executable } = yield* installed(host.id, host.resolved);
    const listing = yield* Effect.mapError(reply(ChildProcess.make('ps', ['-A', '-o', 'pid=,%cpu=,comm='])), exited(host.id));
    const processes = yield* Effect.orDie(_listing(Array.filterMap(String.linesIterator(listing), Filter.fromPredicateOption(flow(String.match(_PROCESS), Option.map(Struct.get('groups')))))));
    return yield* Effect.fromOption(
        Array.findFirst(processes, (process) => process.command === path.join(bundlePath, 'Contents', 'MacOS', executable)),
        () => BridgeError.cases.hostNotRunning.make({ host: host.id }),
    );
});

const processId = (host: Pick<Host, 'id' | 'resolved'>): Effect.Effect<Option.Option<number>, BridgeError, ChildProcessSpawner.ChildProcessSpawner | Path.Path> =>
    liveness(host).pipe(
        Effect.map((found) => Option.some(found.pid)),
        Effect.catchTag('hostNotRunning', () => Effect.succeedNone),
    );

// --- [WORKER] --------------------------------------------------------------------------

const layer: Layer.Layer<Jobs, never, Hosts | ChildProcessSpawner.ChildProcessSpawner | Path.Path> = Layer.effect(
    Jobs,
    Effect.gen(function* () {
        const hosts = yield* Hosts;
        const jobs = yield* Effect.all(
            Record.map(hosts, (resolved, id) =>
                Effect.map(
                    Effect.all([
                        Queue.bounded<Effect.Effect<void, never, ChildProcessSpawner.ChildProcessSpawner | Path.Path>>(QUEUE_DEPTH),
                        SubscriptionRef.make<(typeof Activity)['Type']>({ queueDepth: 0, inFlight: Option.none(), wedged: Option.none(), lastSuccessAt: Option.none(), lastError: Option.none() }),
                    ]),
                    ([queue, activity]): Host => ({ id, resolved, queue, activity }),
                ),
            ),
        );
        yield* Effect.forEach(Record.values(jobs), (host) => Effect.forkScoped(Effect.forever(Effect.flatten(Queue.take(host.queue)))), { discard: true });
        return jobs;
    }),
);

// --- [RUN] -----------------------------------------------------------------------------

const _queue = Effect.fnUntraced(function* <A, R>(host: Host, { jobId, deadlineAt }: (typeof Request)['Type'], settle: typeof _settled, work: Effect.Effect<A, BridgeError, R>) {
    const now = yield* Clock.currentTimeMillis;
    const context = yield* Effect.context<R>();
    const settled = yield* Deferred.make<A, BridgeError>();
    const deadline = BridgeError.cases.deadlineExceeded.make({ host: host.id, jobId });
    const execution = Effect.gen(function* () {
        if (yield* Deferred.isDone(settled)) {
            return yield* Deferred.await(settled);
        }
        yield* liveness(host).pipe(Effect.timeoutOrElse({ duration: Duration.millis(deadlineAt - (yield* Clock.currentTimeMillis)), orElse: () => Effect.fail(deadline) }));
        const startedAt = yield* Clock.currentTimeMillis.pipe(
            Effect.filterOrFail(
                (at) => at < deadlineAt,
                () => deadline,
            ),
        );
        const dispatched = yield* SubscriptionRef.modify(host.activity, (state) =>
            Deferred.isDoneUnsafe(settled)
                ? Tuple.make(Deferred.await(settled), state)
                : Tuple.make(Effect.provideContext(work, context), { ...state, queueDepth: state.queueDepth - 1, inFlight: Option.some({ jobId, startedAt }) }),
        );
        return yield* dispatched;
    });
    const entry = Clock.currentTimeMillis.pipe(
        Effect.filterOrFail(
            (at) => at < deadlineAt,
            () => deadline,
        ),
        Effect.andThen(execution),
        Effect.onExit(
            Effect.fnUntraced(function* (exit: Exit.Exit<A, BridgeError>) {
                const at = yield* Clock.currentTimeMillis;
                yield* SubscriptionRef.update(host.activity, (state) => (Deferred.doneUnsafe(settled, exit) ? settle(state, exit, jobId, at) : state));
            }),
        ),
        Effect.exit,
        Effect.asVoid,
    );
    return yield* Effect.acquireUseRelease(
        SubscriptionRef.update(host.activity, (state) => ({ ...state, queueDepth: state.queueDepth + 1 })),
        () =>
            Effect.gen(function* () {
                if (deadlineAt <= now) {
                    return yield* Effect.fail(deadline);
                }
                yield* Queue.offer(host.queue, entry);
                return yield* Deferred.await(settled);
            }).pipe(Effect.timeoutOrElse({ duration: Duration.millis(deadlineAt - now), orElse: () => Effect.fail(deadline) })),
        (_, exit) =>
            Effect.gen(function* () {
                const at = yield* Clock.currentTimeMillis;
                const completed = at >= deadlineAt && Exit.hasInterrupts(exit) ? Exit.fail(deadline) : exit;
                yield* SubscriptionRef.update(host.activity, (state) =>
                    Option.contains(Option.map(state.inFlight, Struct.get('jobId')), jobId) || !Deferred.doneUnsafe(settled, completed) ? state : settle(state, completed, jobId, at),
                );
            }),
    );
});

const request: (timeoutMs: number) => Effect.Effect<(typeof Request)['Type'], never, Crypto.Crypto> = Effect.fnUntraced(function* (timeoutMs: number) {
    const startedAt = yield* Clock.currentTimeMillis;
    const jobId = yield* Effect.orDie(Crypto.Crypto.use((crypto) => Effect.map(crypto.randomUUIDv4, JobId.make)));
    return { jobId, deadlineAt: startedAt + timeoutMs };
});

const submit = <A, R>(host: Host, entry: (typeof Request)['Type'], work: Effect.Effect<A, BridgeError, R>): Effect.Effect<A, BridgeError, R> => _queue(host, entry, _settled, work);

const run = <A, R>(host: Host, timeoutMs: number, work: (jobId: JobId) => Effect.Effect<A, BridgeError, R>): Effect.Effect<A, BridgeError, R | Crypto.Crypto> =>
    Effect.flatMap(request(timeoutMs), (entry) => submit(host, entry, work(entry.jobId)));

const probe = <A, R>(host: Host, entry: (typeof Request)['Type'], work: Effect.Effect<A, BridgeError, R>): Effect.Effect<Result.Result<A, BridgeError>, never, R> =>
    Effect.result(
        Effect.flatMap(SubscriptionRef.get(host.activity), (state) =>
            Option.match(state.inFlight, {
                onNone: () => _queue(host, entry, _probed, work),
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

const spill = <A>(host: HostId, jobId: JobId, value: A): Effect.Effect<A | (typeof Spilled)['Type'], BridgeError, Crypto.Crypto | FileSystem.FileSystem | Path.Path> =>
    Option.match(
        Option.liftPredicate(JSON.stringify(value), (text) => text.length > SPILL_CHARS),
        { onNone: () => Effect.succeed(value), onSome: (text) => _spilled(host, jobId, text) },
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Host };
export { artifacts, Jobs, layer, liveness, Process, probe, processId, request, root, run, Spilled, spill, submit };
