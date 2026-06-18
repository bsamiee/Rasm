# [SERVICES_AI_ACTIVITY]

The AI activity over the one `AiProvider` literal axis, the `AgentJournal` durable agent-execution ledger, and the `Resilience` primitives every outbound and durable path composes. An AI provider call is one durable activity, not a parallel service set; the five providers are one row on the activity. This page is the SINGLE declaration site of the `AiProvider` literal; every other page references it as settled vocabulary. The AI activity is node-only, never browser-reachable, and crosses no .NET wire.

## [1]-[INDEX]

One cluster: `[2]-[AI_ACTIVITY]` owns the `AiProvider` literal axis, the single AI activity, the `AgentJournal` ledger, and the `Resilience` primitives.

## [2]-[AI_ACTIVITY]

- Owner: `ActivityOwner.aiActivity`, the AI activity over the closed `AiProvider` literal axis; `AgentJournal`, the durable agent-execution ledger; `Resilience`, the circuit-breaker/retry/JSON-patch primitive set.
- Auto: an AI provider call is one durable activity, not a parallel service set. `AiProvider` is a closed `Schema.Literal` vocabulary — anthropic, openai, google, amazon-bedrock, openrouter — selecting the provider model layer; the activity body is one language-model `generateText` (or `generateObject`) call wrapped in the activity's interrupt-retry schedule and compensation finalizers, the selected provider model provided over the activity through the unified model layer, so the five providers are one row on the activity, never five activities; each tool call and checkpoint the activity emits writes one `AgentJournal` row. This is the sole declaration site of `AiProvider`; downstream pages reference it as settled vocabulary.
- Cases: `AgentJournal` is the durable agent ledger keyed by session — `session_start`/`tool_call`/`checkpoint`/`session_complete` rows with `running`/`completed`/`failed`/`interrupted` status — persisted through the one `SqlBoundary` (`persistence/store-boundary#STORE_BOUNDARY`). `Resilience` carries the circuit-breaker (open/half-open/closed over a failure-rate window), the retry/backoff `Schedule`, and the `rfc6902` JSON-patch diff every sync and outbound path composes; `Resilience.breaker` rejects with a typed `DurableFault.CircuitOpen` (`durable-execution/engine#ENGINE`), never an inline anonymous `_tag` shape.
- Entry: the AI activity reaches the cluster engine through `ClusterEngine` and persists its ledger through the one `SqlBoundary`. A retry schedule keys to the effect's FAILURE value (`Schema.Schema.Type<E>`), not a `Cause` — one convention page-wide.
- Packages: the `@effect/ai*` set (`@effect/ai`, `@effect/ai-anthropic`, `@effect/ai-openai`, `@effect/ai-google`, `@effect/ai-amazon-bedrock`, `@effect/ai-openrouter`) for the unified model and the five provider layers, `rfc6902` for the JSON-patch primitive, `@effect/sql` and `@effect/sql-pg` for the journal rows, `@effect/platform-node` for the driver host.
- Growth: a new AI provider lands as one literal on the `AiProvider` axis and one provider-model layer row, never a new activity; a new agent event lands as one `AgentJournal` row kind; a new resilience primitive lands as one `Resilience` combinator row.
- Boundary: the AI activity is the only AI surface in the branch, node-only, never browser-reachable; the SQL boundary is reached only through the one `SqlBoundary`, never a second client; the `Resilience.breaker` rejection is the `durable-execution/engine#ENGINE` `DurableFault.CircuitOpen`, never an inline anonymous `_tag` shape minted here.

```ts contract
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
```
