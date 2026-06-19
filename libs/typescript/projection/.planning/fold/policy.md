# [PROJECTION_POLICY]

`StreamPolicy` is the one bounded reconnect-and-back-pressure vocabulary every fold composes through `withPolicy`, the single `Stream` decorator that applies the full operator set to every fold source. A fold that pipes a source any other way — a bare `Stream.retry`, an improvised reconnect loop, an unbounded buffer — is the deleted form; `withPolicy` is the one site the policy fields become behavior. The domain dials no transport and depends only on the `interchange` decoded shapes.

## [1]-[INDEX]

- [1]-[STREAM_POLICY]: Owns the `StreamPolicy` field set, the `PolicyProfile` posture table, the profile selector, and the `withPolicy` decorator.

## [2]-[STREAM_POLICY]

- Owner: `StreamPolicy`, the four-field operator vocabulary (reconnect `Schedule`, back-pressure buffer, rate throttle, grouped-within batch), and `withPolicy`, the single decorator every fold pipes its source through. The reconnect-and-back-pressure posture is not a literal baked into a single default object — it is `PolicyProfile`, the one `as const satisfies Record<PolicyProfileKey, PolicyProfileRow>` posture table whose every row carries the cadence, depth, throttle window, and batch span of one fold class, and `streamPolicy` materializes a row into a `StreamPolicy` so a new posture is a row value rather than a re-baked object. `defaultStreamPolicy` is the `interactive` row materialized — the one default every store inherits, recovered from the table rather than re-declared beside it.
- Cases: `PolicyProfile` carries three posture rows — `interactive` (the live editing surface: a `250ms` exponential reconnect whose per-attempt delay is capped at a `30s` ceiling through `Schedule.modifyDelay` taking the `Duration.min` of the exponential delay and the ceiling, a 256-cell sliding back-pressure buffer that drops oldest under a stalled consumer, a single-cell-per-frame `16ms` shaping throttle pacing one render frame, and a 64-row `100ms` batch window); `bulk` (the resumable large-history replay: a longer reconnect cadence, a deeper suspend-strategy buffer that back-pressures the producer rather than dropping, a wider throttle window, and a larger batch span amortizing the fold over fewer wake-ups); and `idle` (the background feed: the slowest reconnect, the smallest buffer, and the coarsest batch). `streamPolicy` reads a row through indexed access and lifts its scalar `reconnectBaseMs`/`reconnectCeilingMs`/`throttleFrameMs`/`batchWindowMs` numbers into the `Duration` and `Schedule` values `withPolicy` consumes, so the cadence is a table number every profile shares the construction of, never a per-row `Schedule.exponential` re-spelled inline. `keyof typeof PolicyProfile` projects the `PolicyProfileKey` so a fourth posture breaks every `streamPolicy` selection site at compile time rather than defaulting silently.
- Packages: `effect` for `Stream`, `Schedule`, and `Duration`.
- Growth: a new reconnect or back-pressure posture lands as one `PolicyProfile` row carrying its own cadence, depth, and batch numbers — every fold inherits it through `streamPolicy(key)`, never a per-fold `Schedule` improvisation; a new operator dimension lands as one `StreamPolicy` field threaded once through `withPolicy`.
- Boundary: reconnect, buffer, throttle, and batch are owned once as table rows and never re-derived per fold; the staleness-forward retry value the `availability` gate carries grounds in the `interactive` row `Schedule`; anything that varies between fold classes is a `PolicyProfile` row number, never a literal re-baked into the fold body; the domain imports no `@connectrpc/*` and dials no transport.

```ts contract
import { Duration, Schedule, Stream } from "effect";

interface StreamPolicy {
  readonly reconnect: Schedule.Schedule<Duration.Duration, unknown>;
  readonly buffer: { readonly capacity: number; readonly strategy: "dropping" | "sliding" | "suspend" };
  readonly throttle: { readonly cost: number; readonly units: number; readonly duration: Duration.Duration };
  readonly groupedWithin: { readonly chunkSize: number; readonly window: Duration.Duration };
}

interface PolicyProfileRow {
  readonly reconnectBaseMs: number;
  readonly reconnectCeilingMs: number;
  readonly bufferCapacity: number;
  readonly bufferStrategy: StreamPolicy["buffer"]["strategy"];
  readonly throttleFrameMs: number;
  readonly batchChunkSize: number;
  readonly batchWindowMs: number;
}

const PolicyProfile = {
  interactive: { reconnectBaseMs: 250, reconnectCeilingMs: 30_000, bufferCapacity: 256, bufferStrategy: "sliding", throttleFrameMs: 16, batchChunkSize: 64, batchWindowMs: 100 },
  bulk: { reconnectBaseMs: 500, reconnectCeilingMs: 60_000, bufferCapacity: 4_096, bufferStrategy: "suspend", throttleFrameMs: 64, batchChunkSize: 512, batchWindowMs: 250 },
  idle: { reconnectBaseMs: 1_000, reconnectCeilingMs: 120_000, bufferCapacity: 64, bufferStrategy: "sliding", throttleFrameMs: 250, batchChunkSize: 16, batchWindowMs: 1_000 },
} as const satisfies Record<string, PolicyProfileRow>;

type PolicyProfileKey = keyof typeof PolicyProfile;

const streamPolicy = (key: PolicyProfileKey): StreamPolicy => {
  const row = PolicyProfile[key];
  return {
    reconnect: Schedule.exponential(Duration.millis(row.reconnectBaseMs)).pipe(Schedule.modifyDelay((_out, delay) => Duration.min(delay, Duration.millis(row.reconnectCeilingMs)))),
    buffer: { capacity: row.bufferCapacity, strategy: row.bufferStrategy },
    throttle: { cost: 1, units: 1, duration: Duration.millis(row.throttleFrameMs) },
    groupedWithin: { chunkSize: row.batchChunkSize, window: Duration.millis(row.batchWindowMs) },
  };
};

const defaultStreamPolicy: StreamPolicy = streamPolicy("interactive");

const withPolicy = <In, R>(source: Stream.Stream<In, never, R>, policy: StreamPolicy): Stream.Stream<In, never, R> =>
  source.pipe(
    Stream.retry(policy.reconnect),
    Stream.buffer({ capacity: policy.buffer.capacity, strategy: policy.buffer.strategy }),
    Stream.throttle({ cost: () => policy.throttle.cost, units: policy.throttle.units, duration: policy.throttle.duration, strategy: "shape" }),
    Stream.groupedWithin(policy.groupedWithin.chunkSize, policy.groupedWithin.window),
    Stream.flattenIterables,
  );
```
