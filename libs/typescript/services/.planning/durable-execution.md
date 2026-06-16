# [SERVICES_DURABLE_EXECUTION]

One page owns the request-driven durable work tier — `WorkflowOwner` and `ActivityOwner`, the closed durable-unit vocabulary; `ClusterEngine`, the cluster-backed `WorkflowEngine` wiring; `ActivityOwner.aiActivity` over a closed `AiProvider` `Schema.Literal` axis; the `AgentJournal` durable agent-execution ledger; and the `Resilience` primitives every outbound and durable path composes. This page is the SINGLE declaration site of the `AiProvider` literal; every other page references it as settled vocabulary. Every unit survives a process restart with exactly-once semantics. The page consumes no .NET wire and crosses no wire contract.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                               |
| :-----: | :---------------- | :----------------------------------------------------------------------------------- |
|   [1]   | DURABLE_EXECUTION | durable units, cluster-engine wiring, the AI activity, the agent journal, resilience |

## [2]-[DURABLE_EXECUTION]

- Owner: `WorkflowOwner` and `ActivityOwner`, the durable units; `ClusterEngine`, the cluster-backed `WorkflowEngine` wiring; `ActivityOwner.aiActivity`, the AI activity over the closed `AiProvider` literal axis; `AgentJournal`, the durable agent-execution ledger; and `Resilience`, the circuit-breaker/retry/JSON-patch primitive set.
- Cases: the durable-unit vocabulary is one closed family read by unit kind — a workflow is a named resumable unit with a payload schema, success and error schemas, and an execution id that is the idempotency key; an activity is a run-once unit carrying its own success and error schemas, an interrupt-retry schedule, and compensation finalizers; a durable clock pauses an in-flight workflow without holding resources; a durable deferred awaits an externally-signalled exit; a durable queue is a persisted fan-in whose worker drains payloads with the same exactly-once contract; a durable rate limiter caps a remote-lane or fan-out callout durably across restarts. The vocabulary is the closed `DurableUnit` `Data.TaggedEnum` at workflow, activity, clock, deferred, queue, and rate-limiter, dispatched by unit kind through `durableKernel` over `Match.tagsExhaustive` so the kernel-resolution fold is total and adding a unit without a kernel arm is a typecheck failure; the durable rail's own fault family is `DurableFault` (`CircuitOpen`/`Interrupted`/`Compensated`), one node-side tagged family distinct from the wire-side `interchange` `FaultDetail`, so `Resilience.breaker` rejects with a typed `DurableFault.CircuitOpen` rather than an inline anonymous `_tag` shape. `AgentJournal` is the durable agent ledger keyed by session — `session_start`/`tool_call`/`checkpoint`/`session_complete` rows with `running`/`completed`/`failed`/`interrupted` status — persisted through the one `SqlBoundary`. `Resilience` carries the circuit-breaker (open/half-open/closed over a failure-rate window), the retry/backoff `Schedule`, and the `rfc6902` JSON-patch diff every sync and outbound path composes.
- Saga: a multi-step durable transaction is one workflow folded from a closed `SagaStep` chain, not a parallel saga type family. Each `SagaStep` is a forward/compensating pair — the forward effect is one `Activity` carrying its own `interruptRetryPolicy`, timeout, and `Activity.idempotencyKey` so a re-attempt after a restart never re-applies a side effect, and the compensating effect is registered with that same activity through `Workflow.withCompensation` so it runs in reverse declaration order ONLY for steps that committed, automatically, on any failure or interruption of a later step. The fold is `sagaFold` over the closed `StepOutcome` union (`Forward`/`Compensating`/`Skipped`), reduced by `StepOutcome.$match`/`SagaTerminal.$match` (the generated total `Data.TaggedEnum` matchers) so the saga's terminal outcome — `committed` when every forward arm succeeds, `rolled-back` when a forward arm fails and the committed prefix compensates, `aborted` when a compensation finalizer itself faults — is total and adding an outcome or terminal without a fold arm is a typecheck failure. The `StepOutcome` stream is a REAL mixed stream: `runStep`'s `Effect.matchCauseEffect` rail emits `Forward` on commit, `Compensating({ name, cause })` for the failing step before the engine unwinds, and the body fills `Skipped` for steps never reached after a prior fault; `sagaFold` then reduces that stream to the `SagaTerminal` the workflow's success `Schema` carries. The saga's success/error/payload schemas are the workflow's OWN schemas (success encodes `SagaTerminal`), so `WorkflowProxy.toRpcGroup` (internal-rpc#INTERNAL_RPC) derives the start/`Discard`/`Resume` procedures over the SAME wire `Schema` the saga workflow defines — a second `RpcGroup` or a hand-built saga DTO is the named defect. Every step boundary writes one `AgentJournal` row through the `persistence.md` `SqlBoundary` (`checkpoint` on forward commit, `failed` on compensation) so the saga's progress is durably replayable, never a parallel ledger.
- Entry: every unit survives a process restart with exactly-once semantics; the durable tier is the node-side cluster, separate from the browser worker pool `platform` owns. `ClusterEngine` resolves the durable kernel as a closed wiring set: the cluster-backed `WorkflowEngine` layers over the shard manager and the message storage, the shard manager layers over the shard configuration with its runner, storage, and health dependencies sourced from `internal-rpc.md` `RunnerBackplane`, and a workflow `execute` or an activity body reaches the kernel through the layered `WorkflowEngine`. Every `DurableUnit` kind resolves the SAME cluster-backed `WorkflowEngine`: the durable clock, deferred, queue, and rate-limiter are `@effect/workflow` durable PRIMITIVES (`DurableClock.make`, `DurableDeferred.make`, `DurableQueue.makeWorker`, `DurableRateLimiter.make`) a workflow body composes over that one engine, not distinct engine layers — so `durableKernel` is one direct projection to `ClusterEngine.engine`, not a synthetic kind-dispatch that would return the identical layer from six arms. `DurableUnit` stays the closed vocabulary growth lands on; the kernel does not branch on it because the engine resolution does not. The `@effect/cluster` runtime plus the `@effect/workflow` algebra supplies this wiring; the merged cluster-workflow predecessor is superseded by this successor split.
- Auto: an AI provider call is one durable activity, not a parallel service set. `AiProvider` is a closed `Schema.Literal` vocabulary — anthropic, openai, google, amazon-bedrock, openrouter — selecting the provider model layer; the activity body is one language-model `generateText` (or `generateObject`) call wrapped in the activity's interrupt-retry schedule and compensation finalizers, the selected provider model provided over the activity through the unified model layer, so the five providers are one row on the activity, never five activities; each tool call and checkpoint the activity emits writes one `AgentJournal` row. This is the sole declaration site of `AiProvider`; downstream pages reference it as settled vocabulary.
- Packages: `@effect/cluster` for the runtime and shard manager, `@effect/workflow` for the workflow algebra and `WorkflowEngine`, `@effect/experimental` for the persistence substrate, the `@effect/ai*` set (`@effect/ai`, `@effect/ai-anthropic`, `@effect/ai-openai`, `@effect/ai-google`, `@effect/ai-amazon-bedrock`, `@effect/ai-openrouter`) for the unified model and the five provider layers, `rfc6902` for the JSON-patch primitive, and `@effect/platform-node` for the driver host.
- Growth: a new durable unit lands as one row on the closed unit vocabulary; a new AI provider lands as one literal on the `AiProvider` axis and one provider-model layer row, never a new activity; a new agent event lands as one `AgentJournal` row kind; a new resilience primitive lands as one `Resilience` combinator row; a new saga step lands as one `SagaStep` row appended to the chain (forward `Activity` + compensating finalizer), never a new saga workflow type, and a new saga terminal lands as one case on the `StepOutcome` union that `sagaFold` must then handle.
- Boundary: the successor `@effect/cluster` plus standalone `@effect/workflow` split owns this concern, never the merged predecessor; `DurableUnit` is the live dispatch vocabulary `durableKernel` folds over, never a dead type; `DurableFault` is the one node-side durable fault family and an inline anonymous `{ _tag: "CircuitOpen" }` error shape is the named defect it deletes; the AI activity is the only AI surface in the branch, node-only, never browser-reachable; the SQL boundary is reached only through `ClusterEngine` over the `persistence.md` `SqlBoundary`, never a second client; this page imports `interchange`, `projection`, and the `persistence.md` owner, never the browser domain. The saga is one workflow whose compensation rides `Workflow.withCompensation` (the engine-owned reverse-order finalizer), never a hand-rolled try/rollback or a parallel `SagaCoordinator` service; rollback is driven by the engine on `Cause` of a later step, so the fold NEVER catches a defect to ROLL BACK — actual compensation runs as the engine's exit finalizer. `runStep`'s `Effect.matchCauseEffect` only OBSERVES the failing step's `Cause` into a `Compensating` `StepOutcome` (for the journal `failed` row and the `sagaFold` terminal); it re-emits no recovery effect and triggers no compensation, so the engine's exit-finalizer model remains the sole rollback driver. `runStep` is invoked only at the workflow body's TOP LEVEL (`sagaBody`'s `Effect.forEach`), because `Workflow.withCompensation` registers finalizers for top-level effects only — nesting a step inside an outer activity would silently void its rollback guarantee. The saga workflow is callable only through the ONE `WorkflowProxy`-derived `RpcGroup` over its own `Schema`, and authoring a second `InternalRpc`/saga DTO is the named defect that internal-rpc#INTERNAL_RPC GAP_LEDGER [5] closes.

```ts contract
type DurableUnit = Data.TaggedEnum<{
  readonly Workflow: { readonly name: string };
  readonly Activity: { readonly name: string };
  readonly DurableClock: { readonly name: string };
  readonly DurableDeferred: { readonly name: string };
  readonly DurableQueue: { readonly name: string };
  readonly DurableRateLimiter: { readonly name: string };
}>;
const DurableUnit = Data.taggedEnum<DurableUnit>();

type DurableFault = Data.TaggedEnum<{
  readonly CircuitOpen: { readonly name: string; readonly openedAt: DateTime.Utc };
  readonly Interrupted: { readonly name: string; readonly attempt: number };
  readonly Compensated: { readonly name: string };
}>;
const DurableFault = Data.taggedEnum<DurableFault>();

// Every DurableUnit resolves the SAME cluster-backed WorkflowEngine: clock/deferred/queue/rate-limiter are
// `@effect/workflow` durable PRIMITIVES composed by a workflow body over that engine (DurableClock.make,
// DurableDeferred.make, DurableQueue.makeWorker, DurableRateLimiter.make), NOT distinct engine layers — so the
// engine resolution does not discriminate by kind. The kernel is one direct projection; a synthetic 6-arm match
// over a discriminant that yields the identical layer would be dead ceremony, so it is collapsed here.
const durableKernel = (_: DurableUnit): Layer.Layer<WorkflowEngine, never, Sharding | MessageStorage> => ClusterEngine.engine;

interface ClusterEngine {
  readonly engine: Layer.Layer<WorkflowEngine, never, Sharding | MessageStorage>;
  readonly execute: <P, S, E>(
    workflow: Workflow.Workflow<string, P, S, E>,
    options: { readonly executionId: string; readonly payload: P },
  ) => Effect.Effect<S, E, WorkflowEngine>;
}

const AiProvider = Schema.Literal("anthropic", "openai", "google", "amazon-bedrock", "openrouter");
type AiProvider = Schema.Schema.Type<typeof AiProvider>;

interface ActivityOwner {
  readonly providerModel: (provider: AiProvider, model: string) => Layer.Layer<LanguageModel.LanguageModel>;
  readonly aiActivity: <S extends Schema.Schema.Any, E extends Schema.Schema.All>(options: {
    readonly name: string;
    readonly provider: AiProvider;
    readonly model: string;
    readonly success: S;
    readonly error: E;
    readonly prompt: Prompt.RawInput;
    // Retry schedules key to the effect's FAILURE value (Schema.Schema.Type<E>), not a Cause — one convention page-wide.
    readonly interruptRetryPolicy: Schedule.Schedule<unknown, Schema.Schema.Type<E>>;
    readonly compensate: Effect.Effect<void>;
  }) => Activity.Activity<S, E, LanguageModel.LanguageModel>;
}

class AgentJournal extends Model.Class<AgentJournal>("AgentJournal")({
  id: Model.Generated(Schema.Number),
  sessionId: Schema.String,
  kind: Schema.Literal("session_start", "tool_call", "checkpoint", "session_complete"),
  status: Schema.Literal("running", "completed", "failed", "interrupted"),
  payload: Schema.parseJson(Schema.Unknown),
  at: Model.DateTimeInsert,
}) {}

interface Resilience {
  readonly breaker: <A, E, R>(name: string, effect: Effect.Effect<A, E, R>) => Effect.Effect<A, E | DurableFault, R>;
  readonly retry: Schedule.Schedule<unknown, unknown>;
  readonly patch: (a: unknown, b: unknown) => ReadonlyArray<unknown>;
}

// --- [MODELS] --------------------------------------------------------------------------

// One row on the durable family: a saga step is a forward/compensating pair, never a parallel saga type.
// `forward` is one Activity with its own retry+timeout+idempotency; `compensate` is the reverse-order finalizer.
interface SagaStep<S extends Schema.Schema.Any, E extends Schema.Schema.All, R> {
  readonly name: string;
  readonly forward: Activity.Activity<S, E, R>;
  readonly compensate: (value: Schema.Schema.Type<S>, cause: Cause.Cause<Schema.Schema.Type<E>>) => Effect.Effect<void, never, R>;
  // Keys to the forward Activity's failure value, matching `Effect.retry({ schedule })`'s error-input convention.
  readonly retryPolicy: Schedule.Schedule<unknown, Schema.Schema.Type<E>>;
  readonly timeout: Duration.Duration;
}

// The closed saga step-outcome union: every forward arm resolves to exactly one case, folded by `sagaFold`.
type StepOutcome = Data.TaggedEnum<{
  readonly Forward: { readonly name: string; readonly attempt: number };          // committed; compensation is now armed for this prefix
  readonly Compensating: { readonly name: string; readonly cause: Cause.Cause<unknown> }; // a later step failed; this committed step is rolling back
  readonly Skipped: { readonly name: string };                                    // not yet reached when a prior forward failed
}>;
const StepOutcome = Data.taggedEnum<StepOutcome>();

// The saga terminal: total over the closed StepOutcome stream, no parallel result types.
type SagaTerminal = Data.TaggedEnum<{
  readonly Committed: { readonly steps: ReadonlyArray<string> };
  readonly RolledBack: { readonly failedAt: string; readonly compensated: ReadonlyArray<string> };
  readonly Aborted: { readonly failedAt: string; readonly compensationFault: DurableFault };
}>;
const SagaTerminal = Data.taggedEnum<SagaTerminal>();

// --- [OPERATIONS] ----------------------------------------------------------------------

// The compensation fold: reduce the closed StepOutcome stream to one SagaTerminal via the generated total matchers
// `StepOutcome.$match`/`SagaTerminal.$match`. Every case is REACHABLE — runStep emits Forward/Compensating and
// sagaBody fills Skipped — so all three fold arms and the RolledBack/Aborted machinery are live. A new StepOutcome
// case or SagaTerminal case without an arm is a typecheck failure; no distributed `switch (_tag)`.
const sagaFold = (outcomes: ReadonlyArray<StepOutcome>): SagaTerminal =>
  Array.reduce(outcomes, SagaTerminal.Committed({ steps: [] }) as SagaTerminal, (acc, outcome) =>
    StepOutcome.$match(outcome, {
      Forward: ({ name }) =>
        SagaTerminal.$match(acc, {
          Committed: ({ steps }) => SagaTerminal.Committed({ steps: [...steps, name] }),
          RolledBack: (t) => SagaTerminal.RolledBack(t),
          Aborted: (t) => SagaTerminal.Aborted(t),
        }),
      Compensating: ({ name, cause }) =>
        SagaTerminal.$match(acc, {
          // First failing step: the committed prefix rolls back. If the compensation finalizer itself faulted
          // (a `Die` in the captured Cause), the prefix cannot be undone cleanly -> Aborted with the fault.
          Committed: () =>
            Cause.isDie(cause)
              ? SagaTerminal.Aborted({ failedAt: name, compensationFault: DurableFault.Compensated({ name }) })
              : SagaTerminal.RolledBack({ failedAt: name, compensated: [name] }),
          RolledBack: ({ failedAt, compensated }) => SagaTerminal.RolledBack({ failedAt, compensated: [...compensated, name] }),
          Aborted: (t) => SagaTerminal.Aborted(t),
        }),
      // A step never reached after a prior fault contributes nothing to the terminal but keeps the stream total.
      Skipped: () => acc,
    }),
  );

// Lower one SagaStep into a durable, idempotent, compensation-armed effect that SUCCEEDS with a `Forward`
// outcome or FAILS on its own Cause. It MUST keep failing (rather than absorbing the Cause) so two contracts
// hold: `Effect.forEach` short-circuits the chain at the failing step, and the ENGINE observes the workflow
// failure to run the registered `Workflow.withCompensation` exit finalizers in reverse over the committed prefix.
// MUST be invoked only at the workflow body's TOP LEVEL: `Workflow.withCompensation` registers finalizers for
// top-level effects only, so nesting runStep inside an outer Activity silently drops the rollback guarantee.
// The compensation finalizer's OWN faults are captured via `Effect.catchAllCause` so a fault in undoing a step
// is logged and surfaced (the `Aborted.compensationFault` the fold derives from a `Die` Cause), never swallowed.
const runStep = <S extends Schema.Schema.Any, E extends Schema.Schema.All, R>(
  step: SagaStep<S, E, R>,
): Effect.Effect<StepOutcome, Schema.Schema.Type<E>, R | WorkflowEngine | WorkflowInstance | Scope.Scope> =>
  Activity.idempotencyKey(step.name, { includeAttempt: false }).pipe(
    Effect.zipRight(step.forward),
    // A timeout is a Cause-level interruption the activity's own interruptRetryPolicy gates; it never invents a
    // step-error value, so the bound stays in the engine's retry contract rather than an `as never` escape.
    Effect.timeoutTo({ duration: step.timeout, onTimeout: () => Effect.interrupt, onSuccess: Effect.succeed }),
    Effect.flatten,
    Effect.retry({ schedule: step.retryPolicy }),
    // Compensation's own fault is captured here so `SagaTerminal.Aborted.compensationFault` is observable.
    Workflow.withCompensation((value, cause) =>
      step.compensate(value, cause).pipe(
        Effect.catchAllCause((compCause) => Effect.logError("saga.compensate.fault", compCause)),
      ),
    ),
    // On commit emit Forward; the failure rail propagates unchanged so the engine drives the rollback.
    Effect.flatMap(() => Activity.CurrentAttempt.pipe(Effect.map((attempt) => StepOutcome.Forward({ name: step.name, attempt })))),
  );

// --- [SERVICES] ------------------------------------------------------------------------

interface SagaOwner {
  // Build the saga workflow from a non-empty step chain over the saga's OWN payload/success(SagaTerminal)/error Schema.
  // The forward chain is one Effect.forEach (sequential commit); compensation is engine-owned reverse order.
  // Returns BOTH the Workflow (for execute/poll/interrupt/resume) and its registration Layer (body wired via toLayer).
  readonly make: <const Name extends string, P extends Schema.Struct.Fields, S extends Schema.Schema.Any, E extends Schema.Schema.All>(options: {
    readonly name: Name;
    readonly payload: P;
    readonly success: S;
    readonly error: E;
    readonly idempotencyKey: (payload: Schema.Struct.Type<P>) => string;
    readonly steps: (payload: Schema.Struct.Type<P>) => NonEmptyReadonlyArray<SagaStep<Schema.Schema.Any, Schema.Schema.All, never>>;
  }) => {
    readonly workflow: Workflow.Workflow<Name, Schema.Struct<P>, S, E>;
    readonly layer: Layer.Layer<never, never, WorkflowEngine | SqlBoundary>;
  };
  // Derive the start/Discard/Resume procedures over the SAME Schema the saga defines — the ONE RpcGroup.
  // internal-rpc#INTERNAL_RPC owns the group; this is the single derivation point, not a parallel surface.
  readonly proxy: <const W extends NonEmptyReadonlyArray<Workflow.Any>>(
    sagas: W,
  ) => RpcGroup.RpcGroup<WorkflowProxy.ConvertRpcs<W[number], "">>;
}

// COMPOSITION: the saga workflow body — forward-commit the chain, journal each step boundary, fold to terminal.
// On a later-step failure the engine unwinds the registered compensations in reverse; the body never catches it.
//   producer (runStep) -> journal each boundary -> sagaFold -> SagaTerminal (the workflow's success Schema).
// The body returns the SagaTerminal directly so the producer -> fold -> terminal path is wired in one place; the
// `journal` AgentJournal session is the live ledger row each step boundary writes through the SqlBoundary.
// `Ref<ReadonlyArray<StepOutcome>>` accumulates the committed `Forward` prefix so the failure-observation rail can
// fold the real mixed stream (committed Forwards + the failing Compensating + the unreached Skipped tail) before
// re-raising. The success path folds the pure-Forward stream to `Committed`.
const sagaBody = (sessionId: string) =>
  (steps: NonEmptyReadonlyArray<SagaStep<Schema.Schema.Any, Schema.Schema.All, never>>): Effect.Effect<SagaTerminal, Schema.Schema.Type<Schema.Schema.All>, WorkflowEngine | WorkflowInstance | Scope.Scope | SqlBoundary> =>
    Ref.make<ReadonlyArray<StepOutcome>>([]).pipe(
      Effect.flatMap((committed) =>
        // Run the chain sequentially; runStep commits or FAILS so forEach short-circuits and the engine rolls back.
        Effect.forEach(
          steps,
          (step) =>
            runStep(step).pipe(
              Effect.zipLeft(Effect.annotateCurrentSpan("saga.step", step.name)),
              Effect.tap((outcome) => Ref.update(committed, (xs) => [...xs, outcome])),
              // Commit boundary: one `checkpoint`/`completed` AgentJournal row through the SqlBoundary.
              Effect.tap(() => SqlBoundary.insert(AgentJournal, { sessionId, kind: "checkpoint", status: "completed", payload: { step: step.name } })),
            ),
          { concurrency: 1 },
        ).pipe(
          // Success: every forward committed, fold the pure-Forward stream to `Committed`.
          Effect.map((forwards) => sagaFold(forwards)),
          // Failure-observation rail: journal the failing step `failed`, fold committed-prefix + Compensating +
          // Skipped tail to the RolledBack/Aborted terminal (logged), then RE-RAISE so the engine compensates.
          Effect.tapErrorCause((cause) =>
            Ref.get(committed).pipe(
              Effect.flatMap((prefix) => {
                const failedAt = steps[prefix.length]?.name ?? steps[steps.length - 1].name;
                const stream: ReadonlyArray<StepOutcome> = [
                  ...prefix,
                  StepOutcome.Compensating({ name: failedAt, cause }),
                  ...Array.drop(steps, prefix.length + 1).map((s) => StepOutcome.Skipped({ name: s.name })),
                ];
                return SqlBoundary.insert(AgentJournal, { sessionId, kind: "tool_call", status: "failed", payload: { step: failedAt, cause: Cause.pretty(cause) } }).pipe(
                  Effect.zipRight(Effect.logWarning("saga.rolled-back", sagaFold(stream))),
                );
              }),
            ),
          ),
        ),
      ),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

// The SagaOwner implementation: assemble runStep + sagaBody + sagaFold into the workflow the contract names.
// `make` builds the durable Workflow over the saga's OWN payload/success(SagaTerminal)/error Schema and registers
// its body via `Workflow.toLayer`; `proxy` derives the ONE RpcGroup via WorkflowProxy.toRpcGroup — no parallel surface.
const SagaOwnerLive: SagaOwner = {
  make: (options) => {
    const workflow = Workflow.make({
      name: options.name,
      payload: options.payload,
      success: options.success,
      error: options.error,
      idempotencyKey: options.idempotencyKey,
    });
    // toLayer's body returns Success["Type"]; the saga's success Schema IS the SagaTerminal, so the folded
    // terminal is the workflow's success value directly. The failure rail re-raises the step error (= the
    // workflow error Schema) so the engine runs the registered compensation finalizers in reverse.
    const layer = workflow.toLayer((payload, _executionId) =>
      sagaBody(_executionId)(options.steps(payload)) as Effect.Effect<Schema.Schema.Type<S>, Schema.Schema.Type<E>, never>,
    );
    return { workflow, layer } as ReturnType<SagaOwner["make"]>;
  },
  proxy: (sagas) => WorkflowProxy.toRpcGroup(sagas),
};
```
