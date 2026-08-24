# [RUNTIME_PROFILE]

`Profile` owns continuous wall and heap profiling from the node lane to the Pyroscope backend, labeled by the `Identity.App` every span, metric, and log carries. Pyroscope transport bypasses OTLP: `@pyroscope/nodejs` samples through the native pprof engine and pushes on its own cadence, while this owner brackets the complete lifecycle as one policy value and one Layer.

`Profile.live(policy)` seats identity, backend, auth, symbolication, the armed profiler roster, and path posture once at the node root; its ranked `Life` row drains the final profile and its diagnostic bridge folds the engine's own log sink into the process rail. `Profile.banded(vocabulary, labels, work)` admits bounded channel and step values, projects them onto the profile-store region label, and closes the span-profile join both directions: the live span takes the correlation attribute and the samples take its id and trace id back. Module: `runtime/src/otel/profile.ts`.

## [01]-[INDEX]

- [02]-[POLICY] — the one `Profile.Policy` row: identity, backend, auth, armed profilers, sampling, symbolication; `Profile`.
- [03]-[LIFECYCLE] — the init/arm/drain bracket as one Layer with the ranked drain row and the log bridge; `Profile`.
- [04]-[BANDS] — the region-label band and the span-profile correlation stamp over both work modalities; `Profile`.

## [02]-[POLICY]

- Law: identity is projected through the profile dialect, never restated — `appName` is `identity.app` and `tags` is `Convention.profiled(identity)`, the one profile-store selector projection whose `service_name` label the board `profile` pack and every flamegraph query key on. Folding the full `Convention.identity` attribute record here pushes OTLP resource spellings (`service.name`, `deployment.environment.name`) into a store whose label grammar is `service_name`/`span_name`, minting a second identity vocabulary the pack cannot query; a free-string label beside the projection is the same defect.
- Law: the credential is a closed tagged family over the package's two auth shapes, not a bearer field with an unreachable twin — `Token` mints `authToken` for a self-hosted push gateway and `Basic` mints the `basicAuthUser`/`basicAuthPassword` pair a hosted tenant authenticates with, so a hosted backend needs no second policy and no init-site escape. Both arms seal their secret in `Redacted` end-to-end, unwrapped exactly once inside the `_credential` projection, and the exhaustive tag match decides the field set rather than a nullable pair a caller half-fills.
- Law: the armed roster is policy, not a fan — `profilers` names which native samplers run, so a deployment carrying wall attribution without heap retention pressure arms one row and pays one sampler's overhead. Selection is the point rather than breadth: the package's aggregate `start`/`stop` arms and drains both native samplers together and can express no subset, which is the whole reason a per-row roster exists.
- Law: the roster holds the two NATIVE samplers and no third — the package's `cpu` pair is a backwards-compatibility alias whose start and stop resolve the wall profiler itself, so a `cpu` row beside `wall` is one sampler counted twice: its arm is a logged no-op against an already-started engine and its release either steals the wall drain or finds nothing to drain, by arrival order. Both arms of that pair are the same engine, so the roster names the engine.
- Law: the posture rows are compliance data — `strip` selects `"all" | "dependencies"` path stripping where a deployment's posture forbids source paths on the wire, `shorten` collapses surviving paths to their tail for a store whose label budget the full path exceeds, and `roots` names the sourcemap search directories `SourceMapper.create` walks so transpiled frames resolve to source; all three are policy values, never init-site literals.
- Law: every package type this owner spells reads off the surface that carries it, because the package root re-exports its three config interfaces alone — the strip mode indexes `PyroscopeConfig`, the symbolicator takes `InstanceType` of the default object's class, and the log sink takes `Parameters` of the setter — so an upstream field or signature change breaks the policy row rather than leaving a name that resolves nowhere.
- Law: `backend.origin` is a SELF-EGRESS coordinate, so the root that arms this lane hands the same origin to `Export.Policy.egress` — the push rides `@datadog/pprof`'s own HTTP on `flushIntervalMs`, outside any Effect region and outside every `SpanProcessor`, `LogRecordProcessor`, and metric reader, so the `suppressTracing` wrap that fences the OTLP legs reaches it never and `server#REGISTRATION`'s hostname roster is its only exclusion. Unrostered, an armed profiler under an SDK lane mints a traced outbound span per flush whose export mints another — continuous noise proportional to `flush`, and it is the profiler's own transport reading as application egress.
- Boundary: arming is the composition root's decision — `Setting.otel.profile` resolving no origin means the root composes no `Profile.live` and contributes no `egress` row, so an unarmed deployment loads zero profiler code and this owner carries no unarmed branch.
- Growth: a new profiling decision is one policy field consumed by the lifecycle bracket; a new native sampler is one `_PROFILERS` row; a new auth shape is one `Credential` case with its `_credential` arm; a new backend speaking this dialect is an origin value, and the one lane this owner ever admits is the OTLP profiles transport the swap row names.

```typescript signature
import Pyroscope, { init, wrapWithLabels, type PyroscopeConfig } from "@pyroscope/nodejs"
import { Duration, Effect, Exit, Layer, Match, Option, type ParseResult, Record, Redacted, Runtime, Schema, Scope, type Tracer } from "effect"
import { type Identity, Convention } from "@rasm/core"
import { Life } from "../proc/life.ts"

// this package root re-exports its three config interfaces and nothing else, so every other type reads off the
// record or default-export object carrying it — an import of the bare name resolves nowhere
type _Strip = NonNullable<PyroscopeConfig["stripFilenames"]>
type _Mapper = InstanceType<typeof Pyroscope.SourceMapper>
type _Logger = Parameters<typeof Pyroscope.setLogger>[0]

// One row per NATIVE sampler. The package's `cpu` pair resolves the wall profiler under an older name, so seating it
// here would arm one engine twice and hand its drain to whichever row released first; the aggregate `start`/`stop`
// takes both rows together and can express no subset, which is what this roster buys.
const _PROFILERS = {
  heap: { start: Pyroscope.startHeapProfiling, stop: Pyroscope.stopHeapProfiling },
  wall: { start: Pyroscope.startWallProfiling, stop: Pyroscope.stopWallProfiling },
} as const

declare namespace Profile {
  type BandVocabulary = {
    readonly channel: readonly [string, ...ReadonlyArray<string>]
    readonly step: readonly [string, ...ReadonlyArray<string>]
  }
  type Credential =
    | { readonly _tag: "Basic"; readonly secret: Redacted.Redacted<string>; readonly user: string }
    | { readonly _tag: "Token"; readonly secret: Redacted.Redacted<string> }
  type Sampler = keyof typeof _PROFILERS
  type Policy = {
    readonly identity: Identity.App
    readonly backend: {
      readonly credential: Credential
      readonly origin: string
      readonly tenant: Option.Option<string>
    }
    readonly flush: Duration.Duration
    readonly profilers: ReadonlyArray<Sampler>
    readonly wall: { readonly durationMs: number; readonly intervalMicros: number; readonly cpuTime: boolean }
    readonly heap: { readonly intervalBytes: number; readonly stackDepth: number }
    readonly strip: Option.Option<_Strip>
    readonly shorten: boolean
    readonly roots: ReadonlyArray<string>
    readonly drain: Duration.Duration
  }
}
```

## [03]-[LIFECYCLE]

- Owner: `Profile.live(policy)` — one `Layer.scopedDiscard` bracketing the whole push lifecycle: the acquisition installs the log bridge, resolves `SourceMapper.create(policy.roots)` when roots are declared, folds the policy into one `PyroscopeConfig` (`appName`, `serverAddress`, the credential arm's own fields, `tenantID`, `flushIntervalMs`, the `wall`/`heap` sampling rows, `tags` from the profile projection, `stripFilenames`, `shortenPaths`, `sourceMapper`), calls `init(config)`, then arms each rostered sampler as its OWN `acquireRelease` extended into the child scope, so start and stop are one row and the outer scope owns that child before construction starts; the same close action registers as a ranked `Life` drain row under the policy's drain budget.
- Law: the seat is the node composition root alone — one `init` per process; a library arming the profiler double-samples the native engine, and the browser and worker lanes carry no profiler by construction (the module ships on the `./server` subpath).
- Law: symbolication is acquisition material — `SourceMapper.create` is asynchronous and completes before `init` seats it, so the first pushed profile already resolves transpiled frames; an empty `roots` roster skips the mint and frames ship as built.
- Law: the engine's own diagnostics ride the process rail — `setLogger` seats a six-level sink forwarding into `Effect.log*`, so a rejected push, an expired token, or a sourcemap fault reads beside every other process record instead of vanishing into the package's null logger. Level mapping is the table: `fatal` and `error` land as errors, `warn` as warning, `info` as info, `debug` and `trace` as debug and trace; this mirrors the OTel `diag` bridge at `emit#LANES`, so both foreign SDK sinks reach one rail under one shape.
- Law: drain rank sits beside telemetry — the profile row registers at rank 91, one step after the rank-90 telemetry scope, so spans and metrics flush first and the final profile still lands inside the drain window; each armed sampler's stop promise settles before the process exits because the drain fold awaits the row's budget.
- Law: this bracket publishes whether the engine is SEATED, because the label surface `[04]` bands with resolves the singleton profiler and throws where `init` never ran — an unarmed deployment composes nothing here, so without the seat a banded kernel dies on a lane the deployment deliberately declined. Seating writes where `init` runs and clears where the scope closes, so it never outlives the engine it reports.
- Law: arming is per row and stopping is its release, so partial failure is total — a fault after two of three samplers start releases exactly those two, where a roster-wide finalizer calls stop on an engine that never ran and turns a construction failure into a shutdown defect. Each stop lands its rejection on the log rail rather than as a defect, because one stubborn native sampler must not poison the ordered drain every ranked row behind it depends on.
- Law: this owner is the branch's profiles SWAP POINT, and the swap is a lane row rather than an origin value — a peer push store speaking the same dialect is one `Setting.otel.profile` origin, while the swap off vendor push onto the OTLP profiles signal replaces this push bracket with a profiles lane row beside the `Export.live` exporters, armed only once that signal reaches stable across the three SDK trains. `Profile.banded`'s correlation stamp, the region-label projection, and every flamegraph query survive that replacement untouched, which is the invariant that makes it a row swap: the transport changes and the label vocabulary does not.
- Entry: `Profile.live(policy)` merged at the node root beside `Export.live`; an unarmed deployment (absent `Setting.otel.profile` origin) composes nothing.
- Growth: a per-profiler toggle is one `profilers` entry; a new sampling axis is one policy field folded into the same config mint.
- Packages: `@pyroscope/nodejs` (`init`, the per-sampler start/stop rows, `SourceMapper`, `setLogger` and the `Logger` six-level contract via the default export), `effect` (`Effect`, `Exit`, `Layer`, `Match`, `Record`, `Runtime`, `Scope`), `../proc/life.ts` (`Life`).

```typescript signature
const _credential = (row: Profile.Credential): Partial<PyroscopeConfig> =>
  Match.value(row).pipe(
    // secrets unwrap exactly once, here: the tag fixes the field set the backend actually reads
    Match.tag("Basic", ({ secret, user }) => ({ basicAuthPassword: Redacted.value(secret), basicAuthUser: user })),
    Match.tag("Token", ({ secret }) => ({ authToken: Redacted.value(secret) })),
    Match.exhaustive,
  )

const _config = (policy: Profile.Policy, mapper: Option.Option<_Mapper>): PyroscopeConfig => ({
  appName: policy.identity.app,
  serverAddress: policy.backend.origin,
  ..._credential(policy.backend.credential),
  flushIntervalMs: Duration.toMillis(policy.flush),
  shortenPaths: policy.shorten,
  tags: Convention.profiled(policy.identity), // the profile-store dialect: service_name, never the OTLP resource spelling
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

const _LOG = {
  // this table maps the engine six-level sink onto the process rail, so a rejected push reads beside every other record
  debug: Effect.logDebug,
  error: Effect.logError,
  fatal: Effect.logError,
  info: Effect.logInfo,
  trace: Effect.logTrace,
  warn: Effect.logWarning,
} as const

// Release seat for the process-global sink: the engine's own null logger sits behind its package root, so the
// same table spells a no-op of the shape its interface fixes.
const _silent = Record.map(_LOG, () => (..._args: Array<{}>): void => {}) satisfies _Logger

// This sink stays PROCESS-GLOBAL and closes over this runtime, so it BRACKETS: an unbracketed install outlives its
// scope and keeps forking records onto a runtime the graph already disposed, and the next composition then
// inherits a logger nothing owns. Release re-seats the no-op, so the engine falls back to silence rather than
// to a dead handle.
const _bridged = <R>(runtime: Runtime.Runtime<R>): Effect.Effect<void, never, Scope.Scope> =>
  Effect.asVoid(Effect.acquireRelease(
    // BOUNDARY ADAPTER: the engine's Logger declares mutable rest arrays, so the bridge spells the parameter its interface fixes
    Effect.sync(() =>
      Pyroscope.setLogger(
        Record.map(_LOG, (run) => (...args: Array<{}>): void => {
          Runtime.runFork(runtime)(Effect.annotateLogs(run("<pyroscope>"), { detail: args }))
        }) satisfies _Logger,
      )),
    () => Effect.sync(() => Pyroscope.setLogger(_silent)),
  ))

// `[04]`'s band reads this one bit before it touches the engine. Every label op resolves the singleton profiler and
// THROWS when `init` never ran, so an unarmed deployment — the boundary above, where the root composes nothing here —
// would turn a banded kernel into a defect. The cell is this bracket's own fact: written where `init` seats the
// engine, cleared where the scope closes, and never a policy a caller passes.
let _seated = false

const _armed = (policy: Profile.Policy): Effect.Effect<void, never, Scope.Scope> =>
  Effect.gen(function* () {
    yield* _bridged(yield* Effect.runtime<never>())
    const mapper = yield* policy.roots.length === 0
      ? Effect.succeedNone
      : Effect.map(Effect.promise(() => Pyroscope.SourceMapper.create([...policy.roots])), Option.some) // symbolication completes before init seats it
    yield* Effect.sync(() => init(_config(policy, mapper)))
    yield* Effect.acquireRelease(
      Effect.sync(() => {
        _seated = true
      }),
      () =>
        Effect.sync(() => {
          _seated = false
        }),
    )
    // arm and drain are ONE row: only a sampler this fold actually started ever stops, so a fault partway through the
    // roster releases exactly what it armed instead of calling stop on an engine that never ran
    yield* Effect.forEach(
      policy.profilers,
      (sampler) =>
        Effect.acquireRelease(
          Effect.sync(() => _PROFILERS[sampler].start()),
          () =>
            // a wedged sampler must not poison the ranked drain: the fault reads on the rail and the fold continues
            Effect.catchAll(
              Effect.tryPromise(() => _PROFILERS[sampler].stop()),
              (fault) => Effect.annotateLogs(Effect.logWarning("<profile-drain>"), { detail: String(fault), sampler }),
            ),
        ),
      { discard: true },
    )
  })

const _live = (policy: Profile.Policy): Layer.Layer<never, never, Life> =>
  Layer.scopedDiscard(
    Effect.gen(function* () {
      const scope = yield* Effect.acquireRelease(
        Scope.make(),
        (held) => Scope.close(held, Exit.void),
      )
      yield* Scope.extend(_armed(policy), scope) // the armed rows release with the child scope the ranked drain closes
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

- Owner: `Profile.banded(vocabulary, labels, work)` — the one correlation member folding both work modalities off the input shape: it admits a workload's bounded channel and step values, projects them onto the single `Convention.profile.span` region label, stamps `Convention.profile.id` on the live span, and — for a synchronous thunk alone — scopes `wrapWithLabels` around the region so its samples group under the band on the backend.
- Law: the label key set closes at the profile-store dialect — `Convention.profile.span` is the one region label and `Convention.profile.service` rides the config `tags`, so the caller's `channel`/`step` vocabulary is INPUT that projects into one dotted region value rather than two free-string label keys. Bands writing their own key mint a store dimension no board or pack can query, exactly as a free-string metric name mints an unqueryable series.
- Law: `Convention.profile.id` takes the live span's own id, so the C# root-span stamp and this one spell one attribute.
- Law: `Convention.profile.spanId` and `Convention.profile.traceId` ride the sample labels, closing the join the store queries from.
- Law: one span read feeds both ends, so the attribute and the labels can never name different spans.
- Law: stamping is unconditional across both modalities and silent outside a span region, so a caller never gates on span presence.
- Law: the package ships no span processor, so the branch writes both label halves itself through the label bag `wrapWithLabels` admits.
- Law: the band is synchronous by the engine's contract — `wrapWithLabels` tags every sample taken during the callback, and the ambient label set is thread-global, so an effectful region whose fibers interleave cannot hold a band. Effectful work therefore carries the span stamp alone as its whole contract, since banding it attributes a peer fiber's samples to this region — worse than no band. Long-lived anchors (a machine actor, a gateway duplex) take that arm and join on region name and time window.
- Law: the identifier labels ride the synchronous arm alone, sharing the band's thread-global limit; the effectful arm joins by name and window.
- Law: the branch's long-lived anchors are `work/entity#ACTOR_MINT`'s `actor/<name>` instance span and `serve/live#SSE_ROW`'s `realtime/sse` and `realtime/socket` endpoint spans — each opens where its own scope ends it, takes the effectful arm, and publishes the vocabulary the kernels beneath it band with, so the attribute opens the store join from the trace side and a synchronous kernel's `span_id` label closes it from the sample side.
- Law: the synchronous arm is reachable ONLY while the lifecycle bracket reports the engine seated — every label op resolves the singleton profiler and throws where `init` never ran, so on an unarmed deployment the band runs its kernel bare rather than converting a declined lane into a defect; the decode and the span stamp run identically either way, so a caller's contract does not change with the deployment's posture.
- Law: the engine restores the prior label set AFTER its callback returns and outside any `finally`, so a kernel throwing through an unguarded band leaks that band's labels onto every later sample on the thread with nothing to rewrite them; the arm therefore catches inside the band and re-raises outside it, which keeps the restore on the engine's own path and the defect unchanged for the caller.
- Law: every distinct label value mints a profile series exactly as a metric tag mints a metric series, so band values decode through the caller's non-empty literal roster with excess keys rejected before the engine sees them, and an all-absent band OMITS the region key rather than writing the empty string — an empty label is a series named nothing that every unbanded region joins, which is the store's version of the zero a producer never measured.
- Law: the vocabulary's parser is minted once per vocabulary and held weakly — a schema compiles its parser on first decode and caches it on the instance, so re-minting the struct inside the entry throws that cache away on every banded region and pays an AST compile at a workload seam; the table keys on the caller's own vocabulary value, so the entry stays one member and the parser dies with the roster that owns it.
- Entry: `Profile.banded(vocabulary, { channel }, () => kernel())` at a synchronous workload seam; `Profile.banded(vocabulary, { channel, step }, effect)` at a long-lived scoped span.
- Packages: `@pyroscope/nodejs` (`wrapWithLabels`), `effect` (`Effect`, `Option`, `Schema`), `@rasm/core` (`Convention`).

```typescript signature
type _Held<A> = { readonly ok: true; readonly value: A } | { readonly ok: false; readonly cause: unknown }

const _bandSchema = (vocabulary: Profile.BandVocabulary) =>
  Schema.partial(
    Schema.Struct({
      channel: Schema.Literal(...vocabulary.channel),
      step: Schema.Literal(...vocabulary.step),
    }),
  )

// Schema compiles its parser once per instance, so a per-call mint throws that cache away on every banded region;
// vocabularies are the caller's own values, so this table keys on them weakly and dies with the roster
const _SCHEMAS = new WeakMap<Profile.BandVocabulary, ReturnType<typeof _bandSchema>>()

const _admits = (vocabulary: Profile.BandVocabulary): ReturnType<typeof _bandSchema> =>
  Option.getOrElse(Option.fromNullable(_SCHEMAS.get(vocabulary)), () => {
    const minted = _bandSchema(vocabulary)
    _SCHEMAS.set(vocabulary, minted)
    return minted
  })

const _region = (admitted: { readonly channel?: string; readonly step?: string }): string =>
  [admitted.channel, admitted.step].filter((part): part is string => part !== undefined).join(".")

const _labels = (
  admitted: { readonly channel?: string; readonly step?: string },
  live: Option.Option<Tracer.Span>,
): Record<string, string> => ({
  // one store-dialect key: the caller's bounded vocabulary is input, the region label is the projection. An all-absent
  // band OMITS the key on the same conditional spread `_config` uses, rather than writing "": both spellings are a
  // distinct series to the store, and one of them is a dimension named nothing every unbanded region joins.
  ...(_region(admitted).length > 0 && { [Convention.profile.span]: _region(admitted) }),
  // `spanId` and `traceId` carry the identifier half a region label cannot, so a profile query joins its span on them
  ...Option.match(live, {
    onNone: () => ({}),
    onSome: (span) => ({ [Convention.profile.spanId]: span.spanId, [Convention.profile.traceId]: span.traceId }),
  }),
})

const _banded: {
  <A>(vocabulary: Profile.BandVocabulary, labels: unknown, work: () => A): Effect.Effect<A, ParseResult.ParseError>
  <A, E, R>(vocabulary: Profile.BandVocabulary, labels: unknown, work: Effect.Effect<A, E, R>): Effect.Effect<A, E | ParseResult.ParseError, R>
} = <A, E, R>(
  vocabulary: Profile.BandVocabulary,
  labels: unknown,
  work: (() => A) | Effect.Effect<A, E, R>,
): Effect.Effect<A, E | ParseResult.ParseError, R> =>
  Effect.flatMap(
    Schema.decodeUnknown(_admits(vocabulary), { errors: "all", onExcessProperty: "error" })(labels),
    (admitted) =>
      // ONE span read feeds both ends of the join; outside a span region it answers none and correlation stays silent
      Effect.flatMap(Effect.option(Effect.currentSpan), (live) =>
        Effect.zipRight(
          Option.match(live, {
            onNone: () => Effect.void,
            onSome: (span) => Effect.annotateCurrentSpan(Convention.profile.id, span.spanId),
          }),
          Effect.isEffect(work)
            ? work // the ambient label set is thread-global: an interleaving region takes the stamp alone, never a band
            : !_seated
              ? Effect.sync(work) // unarmed: the engine throws on any label op, so the kernel runs bare and the stamp above still lands
              : Effect.sync(() => {
                // BOUNDARY ADAPTER: the engine callback returns void, so the thunk's OUTCOME crosses through this cell.
                // Pprof restores the prior label set after the callback returns and outside any `finally`, so a
                // thunk throwing through the band would leak these labels onto every later sample on the thread —
                // permanently, since nothing else rewrites them. Catching inside and re-raising outside keeps the
                // restore on the engine's own path while the defect still reaches the caller unchanged.
                let held: _Held<A> = { ok: false, cause: new Error("<band-callback-unreached>") }
                wrapWithLabels(_labels(admitted, live), () => {
                  try {
                    held = { ok: true, value: work() }
                  } catch (cause) {
                    held = { ok: false, cause }
                  }
                })
                if (!held.ok) {
                  throw held.cause
                }
                return held.value
              }),
        )),
  )

const Profile: {
  readonly banded: typeof _banded
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
