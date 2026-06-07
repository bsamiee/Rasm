# [TYPESCRIPT_EFFECT_RESEARCH]

Effect source comments should document the observable contract around `Effect<A, E, R>` and its terminal boundary, not repeat the carrier shape. The current TypeScript capsule in `docs/standards/reference/code-documentation.md` already names the right surfaces; this report supplies the Effect-specific semantics that should decide whether that capsule needs refinement.

## [1][SOURCE_BASIS]

Research scope: `Effect<A, E, R>`, `Scope`, `Layer`, `Stream`, `Exit`, `Cause`, interruption, resources, schedules, and terminal runners.
Source of truth: Effect official documentation and npm package metadata.
Last verified: 2026-06-05.
Review trigger: Effect `latest` dist-tag changes, Effect docs restructure these pages, or Rasm changes its TypeScript code-documentation capsule.

Primary source set:
- Effect package metadata: `npm view effect version time.modified dist-tags --json`; `latest` is `3.21.3`, modified `2026-06-05T13:29:44.268Z`, with `beta` at `4.0.0-beta.78`.
- Effect docs, The Effect Type: `https://effect.website/docs/getting-started/the-effect-type/`.
- Effect docs, Running Effects: `https://effect.website/docs/getting-started/running-effects/`.
- Effect docs, Scope: `https://effect.website/docs/resource-management/scope/`.
- Effect docs, Managing Layers: `https://effect.website/docs/requirements-management/layers/`.
- Effect docs, Layer Memoization: `https://effect.website/docs/requirements-management/layer-memoization/`.
- Effect docs, Stream Introduction: `https://effect.website/docs/stream/introduction/`.
- Effect docs, Resourceful Streams: `https://effect.website/docs/stream/resourceful-streams/`.
- Effect docs, Consuming Streams: `https://effect.website/docs/stream/consuming-streams/`.
- Effect docs, Repetition: `https://effect.website/docs/scheduling/repetition/`.
- Effect docs, Retrying: `https://effect.website/docs/error-management/retrying/`.
- Effect docs, Cause: `https://effect.website/docs/data-types/cause/`.
- Effect docs, Exit: `https://effect.website/docs/data-types/exit/`.

Context7 lookup used the official Effect documentation library `/websites/effect_website` for three focused queries: Effect channels and `Cause`, resource and `Layer` scope, and schedules plus terminal runners.

## [2][CHANNEL_SEMANTICS]

`Effect<A, E, R>` is lazy source truth for an executable workflow. The official docs define `A` as success, `E` as expected error, and `R` as required context; they also state that an Effect value models work until interpreted by a runtime. A source comment should therefore add only the caller-visible semantics that those three type arguments cannot carry.

[COMMENT_FIELDS]:
- `A`: name the committed success meaning, including receipt semantics, returned unit, observable side effect, or stream collection meaning.
- `E`: name expected tagged failures and recovery boundaries, not defects. If retry exhausts, document the terminal expected failure that propagates.
- `R`: name required services, layer-provided dependencies, caller-owned configuration, `Scope`, clock, randomness, telemetry, or platform runtime requirements when the signature does not make their operational consequence obvious.
- Deferred execution: document when the effect performs no work until a terminal runner or runtime executes it, especially when a returned effect starts IO, writes artifacts, or allocates resources later.

Accepted: `@returns` says the effect commits one import receipt or fails with `ImportPayloadError | ImportStorageError`; it requires `ImportStore | Scope` and performs no writes until run.
Rejected: `@returns Effect<ImportReceipt, ImportError, ImportStore | Scope>`.
Reason: the rejected form restates machine shape and hides the semantic contract.

## [3][CAUSE_EXIT]

`Exit<A, E>` is the all-outcome result of running an Effect: success carries `A`, and failure carries a `Cause<E>`. `Cause` distinguishes expected `Fail<E>`, unexpected `Die`, interruption, and composed causes. Comments should mention `Exit` or `Cause` only when callers inspect all outcomes, sandbox causes, attach finalizers that receive an exit, or translate terminal failures.

[DOCUMENT_WHEN]:
- The public API returns `Exit`, uses `runPromiseExit`, stores an exit receipt, or branches on `Exit.Success` and `Exit.Failure`.
- The public API exposes full causes through `Effect.sandbox`, `catchAllCause`, `matchCause`, or terminal runner diagnostics.
- Defects or interruption are observable to the caller as cause data, logged telemetry, process exit mapping, or cleanup behavior.

[OMIT_WHEN]:
- `E` already names expected failure variants and callers cannot observe `Cause`.
- The API uses Effect internally and exposes only a typed `E` rail or successful value.
- A broad `@throws` would imply typed `E` escapes as native exceptions.

Accepted: a terminal runner comment says `runPromiseExit` preserves `Cause` so callers can distinguish `Fail`, `Die`, and `Interrupt`.
Rejected: a typed rail comment adds `@throws` for every `E` variant.
Reason: expected Effect failures are data in `E`; thrown exceptions belong only to actual JavaScript exceptions or terminal runner rejection edges.

## [4][INTERRUPTION_SCOPE]

Interruption is observable when it changes cleanup, external cancellation, finalizer execution, telemetry, or the terminal shape. Official Scope docs show finalizers receiving `Exit` and running when the scope closes on success, failure, or interruption. Comments for scoped resources should state the acquisition owner, release owner, and whether release observes success, failure, and interruption.

[RESOURCE_FIELDS]:
- Acquisition: service, file handle, network connection, transaction, stream resource, runtime fiber, or external subscription opened by the effect.
- Scope owner: caller-provided `Scope`, `Effect.scoped`, layer scope, stream scope, or runtime-managed scope.
- Release: finalizer action and whether it runs on success, expected failure, defect, and interruption.
- Interruption bridge: `AbortSignal`, platform cancellation, child fiber interruption, process signal, external request abort, or no external cancellation propagation.

Accepted: `@remarks` says the stream opens the source cursor inside the caller's `Scope`; the finalizer closes it on success, failure, or interruption, and interruption forwards the caller's abort signal to the remote query.
Rejected: `@returns scoped stream`.
Reason: the rejected form does not tell the caller who closes the resource or what interruption does.

## [5][LAYER_SERVICES]

`Layer` is dependency construction with lifecycle and provision semantics. Official docs show scoped services acquiring and releasing resources, and the memoization docs state that globally provided layers are shared by default and memoized by reference equality. Source comments should document layer construction only where it affects caller obligations, side effects, sharing, teardown, or configuration ownership.

[LAYER_FIELDS]:
- Provides: service tag or capability delivered to `R`.
- Requires: upstream services or configuration not obvious from exported type aliases.
- Construction side effect: connection, pool, cache, telemetry exporter, filesystem state, worker, or subscription started by layer construction.
- Sharing: whether reuse depends on passing the same layer instance, whether fresh allocation is required, or whether local provision avoids global sharing.
- Teardown: scope or finalizer that closes the service.

Accepted: `@remarks` says `Live` opens one shared pool when the same layer value is provided globally; use `fresh` to allocate an isolated pool per run.
Rejected: `@remarks provides Database`.
Reason: the service tag already carries the provided capability; the comment must carry sharing and lifecycle semantics.

## [6][STREAM_SCHEDULE_RUNNER]

`Stream<A, E, R>` has item, failure, and requirement channels, and stream consumers such as `runCollect` convert the stream into an Effect. Resourceful stream docs state that resourceful constructors acquire before stream usage and close after usage, and schedule docs state that repeat and retry policies control recurrence, stop conditions, and failure propagation. Terminal runner docs show `runPromise` returns a Promise and rejects on failure, while `runPromiseExit` preserves `Exit`.

[STREAM_FIELDS]:
- Items: item meaning, ordering, chunking, cardinality, end condition, and whether collection is bounded.
- Failure: expected stream failure variants and whether partial output can be observed before failure.
- Consumption: whether callers must run, drain, collect, or interrupt the stream to release resources.
- Resource: acquisition and finalization for `Stream.acquireRelease`, `Stream.finalizer`, or scoped stream constructors.

[SCHEDULE_FIELDS]:
- Policy: retry or repeat, recurrence count, delay, backoff, jitter, cron, or conditional stop.
- Failure class: which `E` variants are retryable, which fail immediately, and what propagates after exhaustion.
- Requirements: clock, randomness, scheduler, telemetry, or services introduced by the schedule.

[RUNNER_FIELDS]:
- Runner owner: `runSync`, `runPromise`, `runPromiseExit`, `runFork`, custom runtime, platform main, or test runtime.
- Boundary result: direct value, Promise resolution or rejection, `Exit`, fiber handle, process exit status, log span, metric, or runtime defect reporting.
- Cancellation: `AbortSignal`, fiber interruption, signal handler, child-fiber lifetime, or no supported cancellation after the boundary.

Accepted: `@remarks` says `runPromise` is the HTTP boundary: success resolves with the response receipt, expected `HttpFailure` rejects as `FiberFailure`, and the provided `AbortSignal` interrupts the fiber and closes the scoped client.
Rejected: `@returns Promise<ResponseReceipt>`.
Reason: the return type omits typed failure translation, interruption, and resource cleanup.

## [7][INTEGRATION]

The current `docs/standards/reference/code-documentation.md` TypeScript capsule already covers the needed major headings: `Effect<A, E, R>`, interruption, resource scope, retry, terminal runner, `Exit`, `Cause`, `Stream`, `Layer`, and services. A standards edit should prefer small tightening over expansion unless another worker finds missing cases.

Suggested durable additions:
- Add `deferred execution` to the TypeScript `Effect` channel wording because official docs make laziness and runtime interpretation central to the carrier.
- Clarify that `E` is expected error data, while `Cause` is the route for defects and interruption when exposed through sandboxing, matching, `Exit`, or terminal diagnostics.
- Clarify that `Layer` comments should document reference-sharing, fresh allocation, construction side effects, and teardown only when those affect callers.
- Clarify that terminal runner comments should name the translation from Effect outcomes into Promise resolution or rejection, `Exit`, fiber handle, process status, signal handling, logging, spans, or metrics.

Do not add a generated Effect API catalog to the standards page. The standards page should tell authors which semantic fields comments own; official Effect docs and generated TypeDoc remain the API lookup source.

## [8][VALIDATION]

- [x] Read `CLAUDE.md`, root `AGENTS.md`, `docs/standards/AGENTS.md`, `docs/standards/README.md`, the full target `docs/standards/reference/code-documentation.md`, and shared governing standards.
- [x] Used current primary sources from official Effect documentation and npm registry metadata.
- [x] Edited only `.reports/code-documentation-050626/track-typescript/03-typescript-effect.md`.
- [x] Left active standards untouched.
