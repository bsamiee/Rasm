# [PROJECTION_STREAM_POLICY]

`StreamPolicy` is the one bounded reconnect-and-back-pressure vocabulary every fold composes through `withPolicy`, the single `Stream` decorator that applies the full operator set to every fold source. A fold that pipes a source any other way — a bare `Stream.retry`, an improvised reconnect loop, an unbounded buffer — is the deleted form; `withPolicy` is the one site the policy fields become behavior. The domain dials no transport and depends only on the `interchange` decoded shapes.

## [1]-[INDEX]

One cluster: `[2]-[STREAM_POLICY]` owns the `StreamPolicy` field set, its default row, and the `withPolicy` decorator.

## [2]-[STREAM_POLICY]

- Owner: `StreamPolicy`, the four-field operator vocabulary (reconnect `Schedule`, back-pressure buffer, rate throttle, grouped-within batch), and `withPolicy`, the single decorator every fold pipes its source through. `defaultStreamPolicy` is the one default row every store inherits.
- Packages: `effect` for `Stream`, `Schedule`, and `Duration`.
- Growth: a new reconnect or back-pressure posture lands as one `StreamPolicy` field every fold inherits through `withPolicy`, never a per-fold improvisation.
- Boundary: reconnect, buffer, throttle, and batch are owned once and never re-derived per fold; the staleness-forward retry value the `availability` gate carries grounds in this `Schedule`; the domain imports no `@connectrpc/*` and dials no transport.

```ts contract
import { Duration, Schedule, Stream } from "effect";

interface StreamPolicy {
  readonly reconnect: Schedule.Schedule<Duration.Duration, unknown>;
  readonly buffer: { readonly capacity: number; readonly strategy: "dropping" | "sliding" | "suspend" };
  readonly throttle: { readonly cost: number; readonly units: number; readonly duration: Duration.Duration };
  readonly groupedWithin: { readonly chunkSize: number; readonly window: Duration.Duration };
}

const defaultStreamPolicy: StreamPolicy = {
  reconnect: Schedule.exponential(Duration.millis(250)).pipe(Schedule.union(Schedule.spaced(Duration.seconds(30)))),
  buffer: { capacity: 256, strategy: "sliding" },
  throttle: { cost: 1, units: 1, duration: Duration.millis(16) },
  groupedWithin: { chunkSize: 64, window: Duration.millis(100) },
};

const withPolicy = <In, R>(source: Stream.Stream<In, never, R>, policy: StreamPolicy): Stream.Stream<In, never, R> =>
  source.pipe(
    Stream.retry(policy.reconnect),
    Stream.buffer({ capacity: policy.buffer.capacity, strategy: policy.buffer.strategy }),
    Stream.throttle({ cost: () => policy.throttle.cost, units: policy.throttle.units, duration: policy.throttle.duration, strategy: "shape" }),
    Stream.groupedWithin(policy.groupedWithin.chunkSize, policy.groupedWithin.window),
    Stream.flattenIterables,
  );
```
