# [SERVICES_DURABLE_EXECUTION]

One page owns the request-driven durable work tier — `WorkflowOwner` and `ActivityOwner`, the closed durable-unit vocabulary; `ClusterEngine`, the cluster-backed `WorkflowEngine` wiring; `ActivityOwner.aiActivity` over a closed `AiProvider` `Schema.Literal` axis; the `AgentJournal` durable agent-execution ledger; and the `Resilience` primitives every outbound and durable path composes. This page is the SINGLE declaration site of the `AiProvider` literal; every other page references it as settled vocabulary. Every unit survives a process restart with exactly-once semantics. The page consumes no .NET wire and crosses no wire contract.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]          | [OWNS]                                                       |
| :-----: | :----------------- | :--------------------------------------------------------- |
|   [1]   | DURABLE_EXECUTION  | durable units, cluster-engine wiring, the AI activity, the agent journal, resilience |

## [2]-[DURABLE_EXECUTION]

- Owner: `WorkflowOwner` and `ActivityOwner`, the durable units; `ClusterEngine`, the cluster-backed `WorkflowEngine` wiring; `ActivityOwner.aiActivity`, the AI activity over the closed `AiProvider` literal axis; `AgentJournal`, the durable agent-execution ledger; and `Resilience`, the circuit-breaker/retry/JSON-patch primitive set.
- Cases: the durable-unit vocabulary is one closed family read by unit kind — a workflow is a named resumable unit with a payload schema, success and error schemas, and an execution id that is the idempotency key; an activity is a run-once unit carrying its own success and error schemas, an interrupt-retry schedule, and compensation finalizers; a durable clock pauses an in-flight workflow without holding resources; a durable deferred awaits an externally-signalled exit; a durable queue is a persisted fan-in whose worker drains payloads with the same exactly-once contract; a durable rate limiter caps a remote-lane or fan-out callout durably across restarts. The vocabulary is the closed `DurableUnit` `Data.TaggedEnum` at workflow, activity, clock, deferred, queue, and rate-limiter, dispatched by unit kind through `durableKernel` over `Match.tagsExhaustive` so the kernel-resolution fold is total and adding a unit without a kernel arm is a typecheck failure; the durable rail's own fault family is `DurableFault` (`CircuitOpen`/`Interrupted`/`Compensated`), one node-side tagged family distinct from the wire-side `@rasm/interchange` `FaultDetail`, so `Resilience.breaker` rejects with a typed `DurableFault.CircuitOpen` rather than an inline anonymous `_tag` shape. `AgentJournal` is the durable agent ledger keyed by session — `session_start`/`tool_call`/`checkpoint`/`session_complete` rows with `running`/`completed`/`failed`/`interrupted` status — persisted through the one `SqlBoundary`. `Resilience` carries the circuit-breaker (open/half-open/closed over a failure-rate window), the retry/backoff `Schedule`, and the `rfc6902` JSON-patch diff every sync and outbound path composes.
- Entry: every unit survives a process restart with exactly-once semantics; the durable tier is the node-side cluster, separate from the browser worker pool `@rasm/web` owns. `ClusterEngine` resolves the durable kernel as a closed wiring set: the cluster-backed `WorkflowEngine` layers over the shard manager and the message storage, the shard manager layers over the shard configuration with its runner, storage, and health dependencies sourced from `internal-rpc.md` `RunnerBackplane`, and a workflow `execute` or an activity body reaches the kernel through the layered `WorkflowEngine`. The `@effect/cluster` runtime plus the `@effect/workflow` algebra supplies this wiring; the merged cluster-workflow predecessor is superseded by this successor split.
- Auto: an AI provider call is one durable activity, not a parallel service set. `AiProvider` is a closed `Schema.Literal` vocabulary — anthropic, openai, google, amazon-bedrock, openrouter — selecting the provider model layer; the activity body is one language-model `generateText` (or `generateObject`) call wrapped in the activity's interrupt-retry schedule and compensation finalizers, the selected provider model provided over the activity through the unified model layer, so the five providers are one row on the activity, never five activities; each tool call and checkpoint the activity emits writes one `AgentJournal` row. This is the sole declaration site of `AiProvider`; downstream pages reference it as settled vocabulary.
- Packages: `@effect/cluster` for the runtime and shard manager, `@effect/workflow` for the workflow algebra and `WorkflowEngine`, `@effect/experimental` for the persistence substrate, the `@effect/ai*` set (`@effect/ai`, `@effect/ai-anthropic`, `@effect/ai-openai`, `@effect/ai-google`, `@effect/ai-amazon-bedrock`, `@effect/ai-openrouter`) for the unified model and the five provider layers, `rfc6902` for the JSON-patch primitive, and `@effect/platform-node` for the driver host.
- Growth: a new durable unit lands as one row on the closed unit vocabulary; a new AI provider lands as one literal on the `AiProvider` axis and one provider-model layer row, never a new activity; a new agent event lands as one `AgentJournal` row kind; a new resilience primitive lands as one `Resilience` combinator row.
- Boundary: the successor `@effect/cluster` plus standalone `@effect/workflow` split owns this concern, never the merged predecessor; `DurableUnit` is the live dispatch vocabulary `durableKernel` folds over, never a dead type; `DurableFault` is the one node-side durable fault family and an inline anonymous `{ _tag: "CircuitOpen" }` error shape is the named defect it deletes; the AI activity is the only AI surface in the branch, node-only, never browser-reachable; the SQL boundary is reached only through `ClusterEngine` over the `persistence.md` `SqlBoundary`, never a second client; this page imports `@rasm/interchange`, `@rasm/projection`, and the `persistence.md` owner, never the browser domain.

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

const durableKernel = (unit: DurableUnit): Layer.Layer<WorkflowEngine, never, Sharding | MessageStorage> =>
  Match.value(unit).pipe(
    Match.tagsExhaustive({
      Workflow: () => ClusterEngine.engine,
      Activity: () => ClusterEngine.engine,
      DurableClock: () => ClusterEngine.engine,
      DurableDeferred: () => ClusterEngine.engine,
      DurableQueue: () => ClusterEngine.engine,
      DurableRateLimiter: () => ClusterEngine.engine,
    }),
  );

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
    readonly interruptRetryPolicy: Schedule.Schedule<unknown, Cause.Cause<unknown>>;
    readonly compensate: Effect.Effect<void>;
  }) => Activity.Activity<S, E, LanguageModel.LanguageModel>;
}

class AgentJournal extends Model.Class<AgentJournal>("AgentJournal")({
  id: Model.Generated(Schema.Number),
  sessionId: Schema.String,
  kind: Schema.Literal("session_start", "tool_call", "checkpoint", "session_complete"),
  status: Schema.Literal("running", "completed", "failed", "interrupted"),
  payload: Schema.parseJson(Schema.Unknown),
  at: Schema.DateTimeUtc,
}) {}

interface Resilience {
  readonly breaker: <A, E, R>(name: string, effect: Effect.Effect<A, E, R>) => Effect.Effect<A, E | DurableFault, R>;
  readonly retry: Schedule.Schedule<unknown, unknown>;
  readonly patch: (a: unknown, b: unknown) => ReadonlyArray<unknown>;
}
```
