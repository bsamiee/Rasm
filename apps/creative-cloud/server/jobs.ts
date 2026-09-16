// --- [IMPORTS] -------------------------------------------------------------------------

import { Cause, Clock, Context, Crypto, Effect, Encoding, Exit, Fiber, FileSystem, flow, identity, Option, Path, type PlatformError, Ref, Result, Schema } from 'effect';
import { type BridgeError, bridgeError } from './errors.ts';
import { AbsolutePath, type HostId, JobId, PROBE_MS } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface InFlight {
    readonly jobId: JobId;
    readonly startedAt: number;
}

interface Activity {
    readonly inFlight: Option.Option<InFlight>;
    readonly lastSuccessAt: Option.Option<number>;
    readonly lastError: Option.Option<{ readonly error: BridgeError; readonly count: number; readonly at: number }>;
}

type Jobs = Readonly<Record<HostId, Ref.Ref<Activity>>>;

type Spilled = { readonly kind: 'value'; readonly value: Schema.Json } | { readonly kind: 'file'; readonly path: AbsolutePath; readonly bytes: number; readonly sha256: string };

// --- [CONSTANTS] -----------------------------------------------------------------------

const _SPILL_CHARACTERS = 200_000;

// --- [MODELS] --------------------------------------------------------------------------

const InFlight: Schema.Codec<InFlight, unknown> = Schema.Struct({ jobId: JobId, startedAt: Schema.Number });

const Spilled: Schema.Codec<Spilled, unknown> = Schema.Union([
    Schema.Struct({ kind: Schema.Literal('value'), value: Schema.Json }),
    Schema.Struct({ kind: Schema.Literal('file'), path: AbsolutePath, bytes: Schema.Int, sha256: Schema.String }),
]);

// --- [SERVICES] ------------------------------------------------------------------------

const Jobs: Context.Service<Jobs, Jobs> = Context.Service<Jobs>('Jobs');

// --- [STATE] ---------------------------------------------------------------------------

const _failed =
    (error: BridgeError, at: number) =>
    (jobs: Activity): Activity => ({
        ...jobs,
        lastError: Option.some({ error, at, count: Option.match(jobs.lastError, { onNone: () => 1, onSome: (last) => (last.error._tag === error._tag ? last.count + 1 : 1) }) }),
    });

const _recorded =
    (exit: Exit.Exit<unknown, BridgeError>, at: number) =>
    (jobs: Activity): Activity =>
        Exit.match(exit, {
            onSuccess: () => ({ ...jobs, lastSuccessAt: Option.some(at) }),
            onFailure: (cause) => Option.match(Cause.findErrorOption(cause), { onNone: () => jobs, onSome: (error) => _failed(error, at)(jobs) }),
        });

const _released = (jobs: Activity): Activity => ({ ...jobs, inFlight: Option.none() });

const _stamped = (jobs: Ref.Ref<Activity>, transition: (at: number) => (jobs: Activity) => Activity): Effect.Effect<void> =>
    Effect.flatMap(Clock.currentTimeMillis, (at) => Ref.update(jobs, transition(at)));

// --- [RUN] -----------------------------------------------------------------------------

const _job = <A, R>(
    host: HostId,
    jobs: Ref.Ref<Activity>,
    timeoutMs: number,
    settle: typeof _recorded,
    work: (jobId: JobId) => Effect.Effect<A, BridgeError, R>,
): Effect.Effect<A, BridgeError, R | Crypto.Crypto> =>
    Effect.gen(function* () {
        const jobId = yield* Effect.orDie(Crypto.Crypto.use((crypto) => Effect.flatMap(crypto.randomUUIDv4, Schema.decodeEffect(JobId))));
        const startedAt = yield* Clock.currentTimeMillis;
        yield* Effect.fromResult(
            yield* Ref.modify(jobs, (state): readonly [Result.Result<void, BridgeError>, Activity] =>
                Option.match(state.inFlight, {
                    onNone: () => [Result.void, { ...state, inFlight: Option.some({ jobId, startedAt }) }],
                    onSome: (running) => [Result.fail(bridgeError.hostBusy({ host, jobId: running.jobId, startedAt: running.startedAt })), state],
                }),
            ),
        );
        const fiber = yield* Effect.forkDetach(Effect.onExit(work(jobId), (exit) => _stamped(jobs, (at) => flow(settle(exit, at), _released))));
        const deadline = bridgeError.deadlineExceeded({ host, jobId });
        return yield* Effect.timeoutOrElse(Fiber.join(fiber), {
            duration: timeoutMs,
            orElse: () =>
                Effect.andThen(
                    _stamped(jobs, (at) => settle(Exit.fail(deadline), at)),
                    Effect.fail(deadline),
                ),
        });
    });

const run = <A, R>(host: HostId, jobs: Ref.Ref<Activity>, timeoutMs: number, work: (jobId: JobId) => Effect.Effect<A, BridgeError, R>): Effect.Effect<A, BridgeError, R | Crypto.Crypto> =>
    _job(host, jobs, timeoutMs, _recorded, work);

const probe = <A, R>(host: HostId, jobs: Ref.Ref<Activity>, work: (jobId: JobId) => Effect.Effect<A, BridgeError, R>): Effect.Effect<Result.Result<A, BridgeError>, never, R | Crypto.Crypto> =>
    Effect.result(_job(host, jobs, PROBE_MS, () => identity, work));

// --- [SPILL] ---------------------------------------------------------------------------

const spill = (host: HostId, jobId: JobId, value: Schema.Json): Effect.Effect<Spilled, PlatformError.PlatformError, Crypto.Crypto | FileSystem.FileSystem | Path.Path> =>
    Effect.gen(function* () {
        const text = JSON.stringify(value);
        if (text.length <= _SPILL_CHARACTERS) {
            return { kind: 'value', value };
        }
        const fs = yield* FileSystem.FileSystem;
        const path = yield* Path.Path;
        const crypto = yield* Crypto.Crypto;
        const directory = path.resolve(import.meta.dirname, '..', '..', '..', '.artifacts', 'creative-cloud', host, 'results');
        const file = path.join(directory, `${jobId}.json`);
        const bytes = new TextEncoder().encode(text);
        yield* fs.makeDirectory(directory, { recursive: true });
        yield* fs.writeFile(file, bytes);
        return { kind: 'file', path: yield* Effect.orDie(Schema.decodeEffect(AbsolutePath)(file)), bytes: bytes.byteLength, sha256: Encoding.encodeHex(yield* crypto.digest('SHA-256', bytes)) };
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Activity };
export { InFlight, Jobs, probe, run, Spilled, spill };
