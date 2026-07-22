# [RUNTIME_PROFILE]

`Profile` owns continuous wall and heap profiling from the node lane to the Pyroscope backend, labeled by the `AppIdentity` every span, metric, and log carries. Pyroscope transport bypasses OTLP: `@pyroscope/nodejs` samples through the native pprof engine and pushes on its own cadence, while this owner brackets the complete lifecycle as one policy value and one Layer.

`Profile.live(policy)` seats identity, backend, auth, symbolication, sampling, and path posture once at the node root; its ranked `Life` row drains the final profile. `Profile.banded(vocabulary, labels, work)` admits bounded channel and step labels before cpu attribution joins the span identity. Module: `runtime/src/otel/profile.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]   | [OWNS]                                                                         | [PUBLIC]  |
| :-----: | :---------- | :----------------------------------------------------------------------------- | :-------- |
|  [01]   | `POLICY`    | the one `Profile.Policy` row: identity, backend, auth, sampling, symbolication | `Profile` |
|  [02]   | `LIFECYCLE` | the init/start/stop bracket as one Layer with the ranked drain row             | `Profile` |
|  [03]   | `BANDS`     | the `wrapWithLabels` label-band member over work-plane workloads               | `Profile` |

## [02]-[POLICY]

[POLICY]:
- Owner: `Profile.Policy` — one typed row carrying every profiling decision: the `AppIdentity` (its settled dimensions mint `appName` and the label tags, so profile identity and resource identity are one projection), the backend origin and sealed credential, the optional tenant coordinate, the flush cadence, the wall and heap sampling rows, the `StripFilenamesMode` posture, and the sourcemap search roots. Transport rows — backend origin, sealed token — home in `Setting.otel` (`config#ADMISSION_ROWS`' `profile` rows), so the app root assembles the policy from the boot-validated `Setting` and its own identity value, and no profiling decision exists outside the one row.
- Law: identity is projected, never restated — `appName` is `identity.app` and the tag band folds `Convention.identity(identity)` so profiles join traces and metrics on the one `service.name` coordinate; a free-string label beside the projection is the split-identity defect.
- Law: the backend credential rides `Redacted` end-to-end — sealed at config admission, unwrapped exactly once inside `init`; an absent origin on `Setting.otel.profile` leaves the lane unarmed by construction, so a deployment without a profiling backend composes zero profiler code.
- Law: the posture rows are compliance data — `strip` selects `"all" | "dependencies"` path stripping where a deployment's posture forbids source paths on the wire, and `roots` names the sourcemap search directories `SourceMapper.create` walks so transpiled frames resolve to source; both are policy values, never init-site literals.
- Growth: a new profiling decision is one policy field consumed by the lifecycle bracket; a new backend is an origin value, never a lane.
- Packages: `effect` (`Duration`, `Option`, `Redacted`), `@rasm/ts/core` (`AppIdentity`, `Convention`).

```typescript
import Pyroscope, { init, start, stop, wrapWithLabels, type PyroscopeConfig, type StripFilenamesMode } from "@pyroscope/nodejs"
import { Duration, Effect, Exit, Layer, Option, type ParseResult, Record, Redacted, Schema, Scope } from "effect"
import { type AppIdentity, Convention } from "@rasm/ts/core"
import { Life } from "../proc/life.ts"

declare namespace Profile {
  type BandVocabulary = {
    readonly channel: readonly [string, ...ReadonlyArray<string>]
    readonly step: readonly [string, ...ReadonlyArray<string>]
  }
  type Policy = {
    readonly identity: AppIdentity
    readonly backend: {
      readonly origin: string
      readonly token: Redacted.Redacted<string>
      readonly tenant: Option.Option<string>
    }
    readonly flush: Duration.Duration
    readonly wall: { readonly durationMs: number; readonly intervalMicros: number; readonly cpuTime: boolean }
    readonly heap: { readonly intervalBytes: number; readonly stackDepth: number }
    readonly strip: Option.Option<StripFilenamesMode>
    readonly roots: ReadonlyArray<string>
    readonly drain: Duration.Duration
  }
}

const _labels = (identity: AppIdentity): Record<string, string> =>
  Record.filterMap(Convention.identity(identity), (value) => (typeof value === "string" ? Option.some(value) : Option.none()))
```

## [03]-[LIFECYCLE]

[LIFECYCLE]:
- Owner: `Profile.live(policy)` — one `Layer.scopedDiscard` bracketing the whole push lifecycle: the acquisition resolves `SourceMapper.create(policy.roots)` when roots are declared, folds the policy into one `PyroscopeConfig` (`appName`, `serverAddress`, `authToken` unwrapped once, `tenantID`, `flushIntervalMs`, the `wall`/`heap` sampling rows, `tags` from the identity projection, `stripFilenames`, `sourceMapper`), calls `init(config)` then `start()`, and the child scope's release runs `stop()` so the final profile flushes; the outer scope owns that child before construction starts, and the same close action registers as a ranked `Life` drain row under the policy's drain budget.
- Law: the seat is the node composition root alone — one `init`/`start` per process; a library arming the profiler double-samples the native engine, and the browser and worker lanes carry no profiler by construction (the module ships on the `./server` subpath).
- Law: symbolication is acquisition material — `SourceMapper.create` is asynchronous and completes before `init` seats it, so the first pushed profile already resolves transpiled frames; an empty `roots` roster skips the mint and frames ship as built.
- Law: drain rank sits beside telemetry — the profile row registers at rank 91, one step after the rank-90 telemetry scope, so spans and metrics flush first and the final profile still lands inside the drain window; `stop()`'s promise settles before the process exits because the drain fold awaits the row's budget.
- Entry: `Profile.live(policy)` merged at the node root beside `Export.live`; an unarmed deployment (absent `Setting.otel.profile` origin) composes nothing.
- Growth: a per-profiler toggle or a new sampling axis is one policy field folded into the same config mint.
- Packages: `@pyroscope/nodejs` (`init`, `start`, `stop`, `SourceMapper` via the default export), `effect` (`Effect`, `Exit`, `Layer`, `Scope`), `../proc/life.ts` (`Life`).

```typescript
const _config = (policy: Profile.Policy, mapper: Option.Option<Pyroscope.SourceMapper>): PyroscopeConfig => ({
  appName: policy.identity.app,
  serverAddress: policy.backend.origin,
  authToken: Redacted.value(policy.backend.token), // unwrapped exactly once, inside the config mint
  flushIntervalMs: Duration.toMillis(policy.flush),
  tags: _labels(policy.identity), // profile identity IS resource identity: one projection, one service.name
  wall: {
    samplingDurationMs: policy.wall.durationMs,
    samplingIntervalMicros: policy.wall.intervalMicros,
    collectCpuTime: policy.wall.cpuTime,
  },
  heap: { samplingIntervalBytes: policy.heap.intervalBytes, stackDepth: policy.heap.stackDepth },
  ...(Option.isSome(policy.backend.tenant) && { tenantID: policy.backend.tenant.value }),
  ...(Option.isSome(policy.strip) && { stripFilenames: policy.strip.value }),
  ...(Option.isSome(mapper) && { sourceMapper: mapper.value }),
})

const _armed = (policy: Profile.Policy): Effect.Effect<void> =>
  Effect.gen(function* () {
    const mapper = yield* policy.roots.length === 0
      ? Effect.succeedNone
      : Effect.map(Effect.promise(() => Pyroscope.SourceMapper.create([...policy.roots])), Option.some) // symbolication completes before init seats it
    yield* Effect.sync(() => {
      init(_config(policy, mapper))
      start()
    })
  })

const _live = (policy: Profile.Policy): Layer.Layer<never, never, Life> =>
  Layer.scopedDiscard(
    Effect.gen(function* () {
      const scope = yield* Effect.acquireRelease(
        Scope.make(),
        (held) => Scope.close(held, Exit.void),
      )
      yield* Scope.addFinalizer(scope, Effect.promise(() => stop())) // the final profile flushes inside the ordered drain
      yield* _armed(policy)
      yield* Life.register({
        label: "profile",
        rank: 91, // one step after the rank-90 telemetry scope: spans flush first, the last profile still lands in-window
        budget: Option.some(policy.drain),
        run: Scope.close(scope, Exit.void),
      }).pipe(Effect.orDie)
    }),
  )
```

## [04]-[BANDS]

[BANDS]:
- Owner: `Profile.banded(vocabulary, labels, work)` — the label-band member admitting a workload's bounded channel and step values before scoping `wrapWithLabels` around a synchronous region, so its samples group under the band on the backend; the ambient thread labels stay the package's own.
- Law: the band is synchronous by the engine's contract — `wrapWithLabels` bands the wall profiler across the callback's execution, so the member accepts a thunk and effect-shaped work bands its own measured kernel, never an async closure whose samples escape the band.
- Law: band keys close at `channel | step`, and each value decodes through the caller's non-empty literal roster with excess keys rejected before `wrapWithLabels`; every distinct label mints a profile series exactly as a metric tag mints a metric series.
- Entry: `Profile.banded(vocabulary, { channel }, () => kernel())` at the owning workload seam.
- Packages: `@pyroscope/nodejs` (`wrapWithLabels`), `effect` (`Effect`).

```typescript signature
const _bandSchema = (vocabulary: Profile.BandVocabulary) =>
  Schema.partial(
    Schema.Struct({
      channel: Schema.Literal(...vocabulary.channel),
      step: Schema.Literal(...vocabulary.step),
    }),
  )

const _banded = <A>(
  vocabulary: Profile.BandVocabulary,
  labels: unknown,
  work: () => A,
): Effect.Effect<A, ParseResult.ParseError> =>
  Effect.flatMap(
    Schema.decodeUnknown(_bandSchema(vocabulary), { errors: "all", onExcessProperty: "error" })(labels),
    (admitted) =>
      Effect.sync(() => {
        let held: A | undefined
        // BOUNDARY ADAPTER: decoded optional fields contain no undefined entries; the engine callback returns void.
        wrapWithLabels(admitted as Record<string, string | number>, () => {
          held = work()
        })
        return held as A
      }),
  )

const Profile: {
  readonly banded: <A>(
    vocabulary: Profile.BandVocabulary,
    labels: unknown,
    work: () => A,
  ) => Effect.Effect<A, ParseResult.ParseError>
  readonly live: (policy: Profile.Policy) => Layer.Layer<never, never, Life>
} = {
  banded: _banded,
  live: _live,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Profile }
```

## [05]-[RESEARCH]

(none)
