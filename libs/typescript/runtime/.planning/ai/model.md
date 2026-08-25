# [RUNTIME_MODEL]

The intelligence spine: five provider families fold onto one capability-asymmetry table whose rows are `Model.make` values, fallback is the core `ExecutionPlan` engine driven over interchangeable `Model` layers through an overloaded pair — `Effect.withExecutionPlan` and `Stream.withExecutionPlan` sharing one compiled plan — so the streaming modality routes tiers exactly like the buffered one, and the removed provider-plan abstraction has no successor here because the core engine IS the mechanism. Every generation crosses ONE guardrail gate: input screen, structural tool admission compiled from the `tool#SAFETY` partition into `toolChoice`, output sweep over text, object, and streaming modalities, and a typed refusal arm. The gate is carrier-parametric — its three arms take the generation trio as a value, defaulting to the free `LanguageModel` functions and taking a persisted `Chat` wherever a session owns the history, so appending, saving, and gating are one call rather than a gated call beside a hand-written history. The token economy lives on the same page because budget and gate are one admission: meter-relative window/reply budgets bound at the `Tokenizer` Tag (the Anthropic bare-value service and the model-keyed OpenAI factory are the two shipped meters), enforcement is `truncate` before the wire, and context assembly is a measured, rank-ordered greedy weave over app-passed retrieval values — retrieval is data, never a data-wave import. Cost is exact: a `BigDecimal` spend fold over the response `Usage` against per-row rates, with the aggregator's settled per-response cost (`FinishPart.metadata.openrouter.usage.cost`, USD) taking precedence where the row carries it. Business logic depends only on the `LanguageModel`/`Tokenizer` Tags; a provider is a row, never a fork. The module is `runtime/src/ai/model.ts`.

## [01]-[INDEX]

- [02]-[PROVIDER_ROWS]: the asymmetry table, client construction, the one transport requirement; `Providers`.
- [03]-[LADDER]: tier routing and fault-gated failover over both rails on one plan; `Ladder`.
- [04]-[GATE]: the one guardrail — screen, admit, sweep (all three modalities), carrier, refusal, spend; `Guardrail`.
- [05]-[TOKENS]: meter-relative budgets, truncation enforcement, the measured context weave; `Tokens`.

## [02]-[PROVIDER_ROWS]

[PROVIDER_ROWS]:
- Owner: `Providers` — the capability-asymmetry table as data: one row per family carrying its `Model.make` entry, its divergent asymmetry cells, and its client Layer. Cells are facts, not code paths: `openai` populates native embeddings, the model-keyed tokenizer factory, and a namespaced telemetry module; `anthropic` populates the bare `Tokenizer.Service` value and the `cacheControl` breakpoint; `google` reaches embeddings, token counting, and caching through its raw client alone; `bedrock` carries SigV4 credentials and the `cachePoint` block; `openrouter` carries aggregator routing and a settled per-response cost. Each row also carries the semantic-convention `system` identity (`gemini` and `aws.bedrock`, not local provider aliases) used by accounting. A consumer reads a cell; a `switch` over provider names is unspellable.
- Law: a cell earns its column by DIVERGING — a value every row repeats is deleted, and a roster tally is never a cell because no consumer spends a count.
- Law: every row states its forfeit on `degrade`, so a caller reads what the row gives up against the reference row instead of discovering it at an absent tag.
- Law: prompt caching is a `cache` cell, not a provider branch — the breakpoint mechanism diverges four ways across five rows, and the spend fold already prices `cachedInputTokens` at its own rate, so a plane pricing cache hits while stamping no breakpoint prices a discount it never earns.
- Law: a breakpoint is STAMPED, never appended — no `Prompt` combinator carries the provider slot, so the only seat is a message's own `options` and the only way to reach it is a message-array rebuild; `Providers.stamp` performs that rebuild once against the trailing system message, and a row whose cell marks nothing rebuilds nothing.
- Law: construction is uniform — every client is `layerConfig` with `Config.redacted` credentials over the `HttpClient` requirement `net/client`'s default-policy rows satisfy; a provider Layer never dials its own transport policy.
- Law: `Model.make(name, layer)` is both a `Layer` and an `Effect` and carries the provider name Tag, so the ladder and the spend fold read the active row by yielding one Tag — provider identity is ambient, never threaded.
- Law: per-request steering is the provider's `Config` Tag written through `withConfigOverride` — the OpenAI `strict`/`verbosity` knobs, the Anthropic parallel-tool toggle — scoped per effect, never baked into a row.
- Growth: a sixth provider is one row with its cells; an asymmetry axis (a batching endpoint, a reasoning-continuity carrier) is one column every row answers.
- Packages: `@effect/ai` (`Model`, `LanguageModel`); `@effect/ai-openai`, `@effect/ai-anthropic`, `@effect/ai-google`, `@effect/ai-amazon-bedrock`, `@effect/ai-openrouter`; `../net/client.ts` (`Client` — the `HttpClient` policy row).

```typescript
import { AiError, type Chat, LanguageModel, Prompt, Response, Telemetry, Tokenizer, type Tool } from "@effect/ai"
import { AnthropicClient, AnthropicLanguageModel, AnthropicTokenizer } from "@effect/ai-anthropic"
import { AmazonBedrockClient, AmazonBedrockLanguageModel } from "@effect/ai-amazon-bedrock"
import { GoogleClient, GoogleLanguageModel } from "@effect/ai-google"
import { OpenAiClient, OpenAiLanguageModel, OpenAiTelemetry, OpenAiTokenizer } from "@effect/ai-openai"
import { OpenRouterClient, OpenRouterLanguageModel } from "@effect/ai-openrouter"
import { Array, BigDecimal, Chunk, Config, Duration, Effect, ExecutionPlan, Layer, Match, Number, Option, Order, Schedule, Schema, Stream, Struct, type Tracer } from "effect"
import { Fault } from "@rasm/core"
import { Safety } from "./tool.ts"

declare namespace Providers {
  type Descriptor = {
    readonly fits: string
    readonly admit: "api-key" | "sigv4"
    readonly tenancy: string
    readonly lifetime: string
    readonly degrade: string
  }
  type Capability = Descriptor & {
    readonly embed: "native" | "raw" | "none"
    readonly tokenizer: "keyed" | "value" | "fallback"
    readonly cache: "cacheControl" | "cachePoint" | "implicit" | "client"
    readonly telemetry: "namespaced" | "core"
    readonly routing: "direct" | "aggregate"
    readonly cost: "aggregate" | "metered"
    readonly system: NonNullable<Telemetry.BaseAttributes["system"]>
  }
}

const _providers = {
  openai: {
    model: OpenAiLanguageModel.model,
    client: OpenAiClient.layerConfig({ apiKey: Config.redacted("OPENAI_API_KEY") }),
    cells: {
      fits: "<reference-row-native-embeddings-keyed-exact-tokenizer-namespaced-telemetry>",
      admit: "api-key", tenancy: "<per-credential-one-api-key-resolving-to-one-organization-and-project>",
      lifetime: "<does-not-decide-the-provider-evicts-its-own-implicit-prefix-cache>",
      degrade: "<no-cache-breakpoint-and-no-system-fingerprint-through-the-part-algebra>",
      embed: "native", tokenizer: "keyed", cache: "implicit", telemetry: "namespaced", routing: "direct",
      cost: "metered", system: "openai",
    },
  },
  anthropic: {
    model: AnthropicLanguageModel.model,
    client: AnthropicClient.layerConfig({ apiKey: Config.redacted("ANTHROPIC_API_KEY") }),
    cells: {
      fits: "<exact-tokenizer-value-and-a-caller-placed-cache-breakpoint>",
      admit: "api-key", tenancy: "<per-credential-one-api-key-resolving-to-one-workspace>",
      lifetime: "<does-not-decide-the-provider-expires-the-breakpoint-this-row-only-stamps>",
      degrade: "<no-embedding-model>",
      embed: "none", tokenizer: "value", cache: "cacheControl", telemetry: "core", routing: "direct",
      cost: "metered", system: "anthropic",
    },
  },
  google: {
    model: GoogleLanguageModel.model,
    client: GoogleClient.layerConfig({ apiKey: Config.redacted("GOOGLE_API_KEY") }),
    cells: {
      fits: "<generation-through-the-shared-tags-with-every-other-capability-behind-the-raw-client>",
      admit: "api-key", tenancy: "<per-credential-one-api-key-resolving-to-one-cloud-project>",
      lifetime: "<does-not-decide-the-provider-owns-every-cache-entry-the-raw-client-creates>",
      degrade: "<embeddings-token-count-and-caching-reach-the-raw-client-alone>",
      embed: "raw", tokenizer: "fallback", cache: "client", telemetry: "core", routing: "direct",
      cost: "metered", system: "gemini",
    },
  },
  bedrock: {
    model: AmazonBedrockLanguageModel.model,
    client: AmazonBedrockClient.layerConfig({
      accessKeyId: Config.string("AWS_ACCESS_KEY_ID"),
      secretAccessKey: Config.redacted("AWS_SECRET_ACCESS_KEY"),
    }),
    cells: {
      fits: "<claude-under-an-aws-account-role-with-native-guardrail-traces-on-the-finish-part>",
      admit: "sigv4", tenancy: "<per-credential-one-account-role-resolving-to-one-account-and-region>",
      lifetime: "<does-not-decide-the-provider-expires-the-cache-point-this-row-only-places>",
      degrade: "<no-embedding-model-and-no-first-party-tokenizer>",
      embed: "none", tokenizer: "fallback", cache: "cachePoint", telemetry: "core", routing: "direct",
      cost: "metered", system: "aws.bedrock",
    },
  },
  openrouter: {
    model: OpenRouterLanguageModel.model,
    client: OpenRouterClient.layerConfig({ apiKey: Config.redacted("OPENROUTER_API_KEY") }),
    cells: {
      fits: "<one-key-reaching-many-upstreams-with-a-settled-per-response-cost>",
      admit: "api-key", tenancy: "<per-credential-one-aggregator-key-fronting-many-upstream-accounts-it-never-separates>",
      lifetime: "<does-not-decide-the-resolved-upstream-expires-the-breakpoint>",
      degrade: "<no-embedding-tokenizer-telemetry-or-provider-tool-binding>",
      embed: "none", tokenizer: "fallback", cache: "cacheControl", telemetry: "core", routing: "aggregate",
      cost: "aggregate", system: "openrouter",
    },
  },
} as const satisfies Record<string, { readonly model: unknown; readonly client: unknown; readonly cells: Providers.Capability }>

declare namespace Providers {
  type Name = keyof typeof _providers
  type Row = (typeof _providers)[Name]
  type Ttl = "5m" | "1h"
  type _Rows<T extends Record<Name, { readonly cells: Capability }> = typeof _providers> = T
}

const _breakpoints = {
  cacheControl: (provider: Providers.Name, ttl: Providers.Ttl) =>
    Option.some({ [provider]: { cacheControl: { type: "ephemeral", ttl } } }),
  cachePoint: (provider: Providers.Name) => Option.some({ [provider]: { cachePoint: { type: "default" } } }),
  implicit: Option.none,
  client: Option.none,
} as const satisfies Record<
  Providers.Capability["cache"],
  (provider: Providers.Name, ttl: Providers.Ttl) => Option.Option<Prompt.ProviderOptions>
>

const _stamped = (prompt: Prompt.Prompt, provider: Providers.Name, ttl: Providers.Ttl = "5m"): Prompt.Prompt =>
  Option.match(_breakpoints[_providers[provider].cells.cache](provider, ttl), {
    onNone: () => prompt,
    onSome: (options) =>
      Option.match(Array.findLastIndex(prompt.content, (message) => message.role === "system"), {
        onNone: () => prompt,
        onSome: (at) =>
          Prompt.fromMessages(Array.map(prompt.content, (message, index) =>
            index === at && message.role === "system"
              ? Prompt.makeMessage("system", { content: message.content, options })
              : message)),
      }),
  })

const Providers = {
  ..._providers,
  names: Struct.keys(_providers),
  breakpoint: (provider: Providers.Name, ttl: Providers.Ttl = "5m"): Option.Option<Prompt.ProviderOptions> =>
    _breakpoints[_providers[provider].cells.cache](provider, ttl),
  stamp: _stamped,
}
```

## [03]-[LADDER]

[LADDER]:
- Owner: `_graded` grades the `AiError` union onto the branch fault lattice, and both ladder rungs read that grade.
- Law: an unbranded provider failure grades `defect` by default, so an ungraded ladder retries nothing and fails over nowhere — the grade is what arms `while` and the tier schedule, and both take the same predicate so one call cannot retry under a policy the other refuses.
- Law: `HttpResponseError` grades on STATUS, never on its tag — one tag covers a rate limit, a revoked key, and a malformed request, so a tag-level verdict either replays a caller error through every tier or strands a throttle on one attempt.
- Law: `Ladder.after` reads the provider's own published wait off the refusal it already holds; a schedule ignoring it either hammers the window or over-waits it.
- Law: the published wait REPLACES one compiled delay and composes no second curve — `Ladder.paced` re-points the budget schedule's output at the retry input so the refusal becomes readable, then replaces that single decision's delay; the core budget owns every other attempt, and a summed delay would multiply the curve the branch retry ruling pins.
- Law: the streaming arm refuses partial-stream fallback — `preventFallbackOnPartialStream: true` — because a gated stream has already released swept text deltas by the time a tier trips, and a silent re-run would replay them past the sweep window the gate's release rule holds.
- Law: the plan's own requirement tail is derived, never restated — `Ladder.Needs` reads the compiled plan's `requirements` channel, so the provider-client set each tier drags behind it is a projection of the table and a sixth provider widens it with no signature edit.
- Law: tier selection evidence rides the span — the settled step's provider name and attempt count annotate the generation span so cost attribution and failover health are queryable per call.
- Law: sampling is tier policy, not a call knob — `sampling` carries the package's own `RequestAttributes` band minus `model` (the tier already anchors that), so temperature, top-p, penalties, stop sequences, and seed travel with the row the gate prices and annotate the span from the same value.
- Growth: a new tier is one table row; a per-tenant ladder is a table value selected by the caller's context.

```typescript
const _statusRows = [
  { at: (status: number) => status === 408 || status === 429, class: "exhausted" },
  { at: (status: number) => status === 401 || status === 403, class: "denied" },
  { at: (status: number) => status === 404, class: "absent" },
  { at: (status: number) => status === 409, class: "conflicted" },
  { at: (status: number) => status >= 500, class: "unavailable" },
] as const satisfies ReadonlyArray<{ readonly at: (status: number) => boolean; readonly class: Fault.Class.Kind }>

const _status = (status: number): Fault.Class.Kind =>
  Option.match(Array.findFirst(_statusRows, (row) => row.at(status)), {
    onNone: () => "invalid" as const,
    onSome: (row) => row.class,
  })

const _graded = Match.type<AiError.AiError>().pipe(
  Match.tag("HttpRequestError", (fault) => fault.reason === "Transport" ? "unavailable" as const : "invalid" as const),
  Match.tag("HttpResponseError", (fault) =>
    fault.reason === "StatusCode" ? _status(fault.response.status) : "malformed" as const),
  Match.tag("MalformedInput", () => "invalid" as const),
  Match.tag("MalformedOutput", () => "malformed" as const),
  Match.tag("UnknownError", () => "defect" as const),
  Match.exhaustive,
)

const _classOf = (fault: unknown): Fault.Class.Kind =>
  AiError.isAiError(fault) ? _graded(fault) : Fault.Class.of(fault)

const _yields = (fault: unknown): boolean => Fault.Class.retryable(_classOf(fault))

const _after = (fault: unknown): Option.Option<Duration.Duration> =>
  AiError.isAiError(fault) && fault._tag === "HttpResponseError"
    ? Option.map(Option.flatMap(Option.fromNullable(fault.response.headers["retry-after"]), Number.parse), Duration.seconds)
    : Option.none()

const _paced = (budget: Fault.Budget.Kind) =>
  Schedule.passthrough(Fault.Budget.schedule(budget, _yields)).pipe(
    Schedule.modifyDelay((fault, delay) => Option.getOrElse(_after(fault), () => delay)),
  )

const _step = (tier: Ladder.Tier) => ({
  provide: _providers[tier.provider].model(tier.model),
  attempts: tier.attempts,
  schedule: _paced(tier.budget),
  while: (fault: unknown) => Effect.succeed(_yields(fault)),
})

const _plan = (table: Ladder.Table) => ExecutionPlan.make(...Array.map(table, _step))

declare namespace Ladder {
  type Tier = {
    readonly name: string
    readonly provider: Providers.Name
    readonly model: string
    readonly attempts: number
    readonly budget: Fault.Budget.Kind
    readonly sampling: Omit<Telemetry.RequestAttributes, "model">
    readonly rate: {
      readonly input: BigDecimal.BigDecimal
      readonly cachedInput: BigDecimal.BigDecimal
      readonly output: BigDecimal.BigDecimal
      readonly reasoning: BigDecimal.BigDecimal
    }
  }
  type Table = readonly [Tier, ...Array<Tier>]
  type Plan = ReturnType<typeof _plan>
  type Needs = Plan extends ExecutionPlan.ExecutionPlan<infer Types> ? Types["requirements"] : never
  type Bound<R> = Exclude<R, LanguageModel.LanguageModel> | Needs
}

function _tiered<A, E, R>(table: Ladder.Table, call: Effect.Effect<A, E, R>): Effect.Effect<A, E, Ladder.Bound<R>>
function _tiered<A, E, R>(table: Ladder.Table, call: Stream.Stream<A, E, R>): Stream.Stream<A, E, Ladder.Bound<R>>
function _tiered<A, E, R>(table: Ladder.Table, call: Effect.Effect<A, E, R> | Stream.Stream<A, E, R>) {
  const plan = _plan(table)
  return Effect.isEffect(call)
    ? Effect.withExecutionPlan(call, plan)
    : Stream.withExecutionPlan(call, plan, { preventFallbackOnPartialStream: true })
}

const Ladder = { drive: _tiered, yields: _yields, grade: _classOf, after: _after, paced: _paced }
```

## [04]-[GATE]

[GATE]:
- Owner: `Guardrail.generate(policy, request, carrier?)` — one request-shape-discriminated entry over `Text`, `Object`, and `Stream` modalities. Every arm screens the prompt, validates forced tool choice against the admitted roster, spreads `toolChoice` and `disableToolCallResolution` into the provider request, detects provider refusal, and sweeps output; the request tag changes the modality and the carrier changes WHO executes it, never the guardrail surface.
- Law: the carrier is a value, and its shape is the substrate's own — `Guardrail.Carrier` is `Pick<Chat.Service, "generateText" | "generateObject" | "streamText">`, so the persisted chat IS a carrier by construction and the free `LanguageModel` trio proves conformance at its `satisfies` seam. The default carrier is the free trio; handing a `Chat.Persisted` instead makes the same gated call append prompt and response to the conversation and write both to the backing store, so a session's history is generated, never assembled. A gated call beside a hand-written history append is the split this parameter deletes.
- Law: the stream sweep retains withheld OUTPUT and its source id. Each text delta appends, sweeps the whole held window, and releases only the prefix older than `policy.window`; before every non-text part it sweeps and flushes the residual text first, preserving source order instead of allowing tool or metadata parts to overtake withheld text. `text-end` is the same boundary rule, and a match fails before any byte in the matched span emits.
- Law: admission modes are policy rows — `Safety.admit` partitions the graded roster into executable `allowed` tools and visible-but-unresolved `held` tools, and the provider's `oneOf` receives their union while `disableToolCallResolution` prevents local execution whenever `held` is non-empty. An empty union compiles to `"none"`; mandatory choice compiles to `{ mode: "required", oneOf: visible }`; and a forced tool outside the visible union is a policy defect, never a silent escalation. This preserves held-call evidence for the agent approval loop without making the held tool executable.
- Law: the provider's verdict reads the WHOLE finish roster through one table — `content-filter` and `error` are refusals, `pause` is unfinished work, and everything else settles — so a gate matching a single literal hands back a filtered, faulted, or truncated turn as a clean reply; that one table serves both buffered arms and the stream's finish part.
- Law: spend and telemetry fold per call as one accounting — `Spend.accounted(tier, response)` reads the settled cost through the tier row's `cost` cell (`FinishPart.metadata.openrouter.usage.cost`, USD, admitted through `BigDecimal.safeFromNumber`) and otherwise multiplies `Usage` (input, output, reasoning, cached tokens) against the tier's `BigDecimal` rate rows; `Telemetry.addGenAIAnnotations(span, { system, operation, request, usage })` writes the standard `gen_ai.*` attribute set onto the generation span — the request band spreads the tier's whole sampling row, so a cost anomaly attributes to a config change rather than to traffic — and float arithmetic on money and hand-named span attributes are both unspellable.
- Law: the `telemetry` capability cell is a code path, not a fact — one annotation record keyed by that cell runs after the core fold: the `namespaced` row folds `OpenAiTelemetry.addGenAIAnnotations(span, { openai: { response: { serviceTier } } })` off the finish part's own `openai` metadata slot, the `core` row is a no-op, and the lookup replaces the provider `switch` the column exists to make unspellable. `_finish` is the one finish-part read both the exact-cost admission and this fold consume.
- Law: tool-call ids are pluggable — the `IdGenerator` Tag rides the requirement set, and a durable agent supplies a deterministic generator Layer at its root so replayed turns mint identical tool-call ids and the workflow journal stays byte-stable across replay.
- Growth: a screen or sweep policy is a predicate row on the gate's policy table; a new modality inherits the fold by construction; a provider gaining a namespaced telemetry module is one annotation row.

```typescript
const _LEG = "gate"

const _refusals = Fault.Class.family(["screened", "swept", "provider", "stalled", "policy"] as const, {
  screened: Fault.Class.row({
    class: "denied",
    leg: _LEG,
    detail: Schema.Struct({ rule: Schema.NonEmptyString }),
    render: ({ rule }) => `the prompt matched screen rule ${rule}`,
  }),
  swept: Fault.Class.row({
    class: "denied",
    leg: _LEG,
    detail: Schema.Struct({ span: Schema.NonEmptyString }),
    render: ({ span }) => `the answer carried a swept span — ${span}`,
  }),
  provider: Fault.Class.row({
    class: "denied",
    leg: _LEG,
    detail: Schema.Struct({ finish: Schema.Literal("content-filter", "error") }),
    render: ({ finish }) => `the provider refused this turn and finished ${finish}`,
  }),
  stalled: Fault.Class.row({
    class: "unavailable",
    leg: _LEG,
    detail: Schema.Struct({ finish: Schema.Literal("pause") }),
    render: () => "the provider paused this turn mid-tool and it is unfinished, never settled",
  }),
  policy: Fault.Class.row({
    class: "invalid",
    leg: _LEG,
    detail: Schema.Struct({ choice: Schema.NonEmptyString }),
    render: ({ choice }) => `tool choice ${choice} names nothing this gate made visible`,
  }),
})

class GuardrailFault extends Schema.TaggedError<GuardrailFault>()("GuardrailFault", {
  case: _refusals.payload,
}) {
  get class(): Fault.Class.Kind {
    return _refusals.classOf(this.case.reason)
  }
  override get message(): string {
    return _refusals.render(this.case)
  }
}

const _finishes = {
  "content-filter": Option.some({ reason: "provider", finish: "content-filter" } as const),
  error: Option.some({ reason: "provider", finish: "error" } as const),
  pause: Option.some({ reason: "stalled", finish: "pause" } as const),
  stop: Option.none(),
  length: Option.none(),
  "tool-calls": Option.none(),
  other: Option.none(),
  unknown: Option.none(),
} as const satisfies Record<Response.FinishReason, Option.Option<Guardrail.Issue>>

const _refused = (reason: Response.FinishReason): Option.Option<GuardrailFault> =>
  Option.map(_finishes[reason], (issue) => new GuardrailFault({ case: issue }))

const _free = {
  generateText: LanguageModel.generateText,
  generateObject: LanguageModel.generateObject,
  streamText: LanguageModel.streamText,
} satisfies Guardrail.Carrier

declare namespace Guardrail {
  type Carrier = Pick<Chat.Service, "generateText" | "generateObject" | "streamText">
  type Issue = typeof _refusals.payload.Type
  type Reason = (typeof _refusals.kinds)[number]
  type Policy = {
    readonly screen: (prompt: Prompt.Prompt) => Option.Option<string>
    readonly sweep: (text: string) => Option.Option<string>
    readonly mode: Safety.Mode
    readonly graded: ReadonlyArray<{ readonly name: string; readonly clazz: Safety.Class }>
    readonly window: number
    readonly choice: Option.Option<"required" | { readonly tool: string }>
  }
  type Request =
    | { readonly _tag: "Text"; readonly options: Parameters<typeof LanguageModel.generateText>[0] }
    | { readonly _tag: "Object"; readonly options: Parameters<typeof LanguageModel.generateObject>[0] }
    | { readonly _tag: "Stream"; readonly options: Parameters<typeof LanguageModel.streamText>[0] }
}

const _admitted = (policy: Guardrail.Policy) => {
  const admission = Safety.admit(policy.graded, policy.mode)
  const visible = [...admission.allowed, ...admission.held]
  const disabled = admission.held.length > 0
  return Option.match(policy.choice, {
    onNone: () => Effect.succeed({
      toolChoice: visible.length === 0 ? "none" as const : { mode: "auto" as const, oneOf: visible },
      disableToolCallResolution: disabled,
      held: admission.held,
    } as const),
    onSome: (forced) => {
      if (forced === "required") {
        return visible.length === 0
          ? Effect.fail(new GuardrailFault({ case: { reason: "policy", choice: "required" } }))
          : Effect.succeed({
            toolChoice: { mode: "required" as const, oneOf: visible },
            disableToolCallResolution: disabled,
            held: admission.held,
          } as const)
      }
      return Array.contains(visible, forced.tool)
        ? Effect.succeed({ toolChoice: forced, disableToolCallResolution: disabled, held: admission.held } as const)
        : Effect.fail(new GuardrailFault({ case: { reason: "policy", choice: forced.tool } }))
    },
  })
}

const _screened = (policy: Guardrail.Policy, prompt: Prompt.Prompt) =>
  Option.match(policy.screen(prompt), {
    onNone: () => Effect.void,
    onSome: (rule) => Effect.fail(new GuardrailFault({ case: { reason: "screened", rule } })),
  })

const _split = (window: string, width: number): readonly [kept: string, freed: string] => [
  window.slice(Number.max(0, window.length - width)),
  window.slice(0, Number.max(0, window.length - width)),
]

const _sweepStream = (policy: Guardrail.Policy) =>
<Tools extends Record<string, Tool.Any>, E, R>(parts: Stream.Stream<Response.StreamPart<Tools>, E, R>) =>
  parts.pipe(
    Stream.mapAccumEffect({ held: "", id: "" }, (state, part) =>
      part.type === "text-delta"
        ? Option.match(policy.sweep(state.held + part.delta), {
          onNone: () => {
            const [kept, freed] = _split(state.held + part.delta, policy.window)
            return Effect.succeed([
              { held: kept, id: part.id },
              freed.length > 0
                ? Chunk.of<Response.StreamPart<Tools>>(Response.makePart("text-delta", { id: part.id, delta: freed }))
                : Chunk.empty<Response.StreamPart<Tools>>(),
            ] as const)
          },
          onSome: (span) => Effect.fail(new GuardrailFault({ case: { reason: "swept", span } })),
        })
        : Option.match(part.type === "finish" ? _refused(part.reason) : Option.none<GuardrailFault>(), {
          onSome: Effect.fail,
          onNone: () =>
            Option.match(policy.sweep(state.held), {
              onNone: () => Effect.succeed([
                { held: "", id: "" },
                state.held.length > 0
                  ? Chunk.make<Response.StreamPart<Tools>>(
                    Response.makePart("text-delta", { id: state.id, delta: state.held }),
                    part,
                  )
                  : Chunk.of<Response.StreamPart<Tools>>(part),
              ] as const),
              onSome: (span) => Effect.fail(new GuardrailFault({ case: { reason: "swept", span } })),
            }),
        })),
    Stream.flattenChunks,
  )

const _request = <Options extends object>(policy: Guardrail.Policy, options: Options) =>
  Effect.map(_admitted(policy), (gate) => ({
    ...options,
    toolChoice: gate.toolChoice,
    disableToolCallResolution: gate.disableToolCallResolution,
  } as const))

const _swept = (policy: Guardrail.Policy) =>
<Settled extends { readonly finishReason: Response.FinishReason; readonly text: string }>(settled: Settled) =>
  Option.match(_refused(settled.finishReason), {
    onSome: Effect.fail,
    onNone: () =>
      Option.match(policy.sweep(settled.text), {
        onNone: () => Effect.succeed(settled),
        onSome: (span) => Effect.fail(new GuardrailFault({ case: { reason: "swept", span } })),
      }),
  })

const _text = <Options extends Parameters<typeof LanguageModel.generateText>[0]>(
  policy: Guardrail.Policy,
  options: Options,
  carrier: Guardrail.Carrier = _free,
) =>
  _screened(policy, Prompt.make(options.prompt)).pipe(
    Effect.zipRight(_request(policy, options)),
    Effect.flatMap(carrier.generateText),
    Effect.flatMap(_swept(policy)),
  )

const _object = <Options extends Parameters<typeof LanguageModel.generateObject>[0]>(
  policy: Guardrail.Policy,
  options: Options,
  carrier: Guardrail.Carrier = _free,
) =>
  _screened(policy, Prompt.make(options.prompt)).pipe(
    Effect.zipRight(_request(policy, options)),
    Effect.flatMap(carrier.generateObject),
    Effect.flatMap(_swept(policy)),
  )

const _stream = <Options extends Parameters<typeof LanguageModel.streamText>[0]>(
  policy: Guardrail.Policy,
  options: Options,
  carrier: Guardrail.Carrier = _free,
) =>
  Stream.unwrap(_screened(policy, Prompt.make(options.prompt)).pipe(
    Effect.zipRight(_request(policy, options)),
    Effect.map((admitted) => carrier.streamText(admitted).pipe(_sweepStream(policy))),
  ))

function _generate<const Options extends Parameters<typeof LanguageModel.generateText>[0]>(
  policy: Guardrail.Policy,
  request: { readonly _tag: "Text"; readonly options: Options },
  carrier?: Guardrail.Carrier,
): ReturnType<typeof _text<Options>>
function _generate<const Options extends Parameters<typeof LanguageModel.generateObject>[0]>(
  policy: Guardrail.Policy,
  request: { readonly _tag: "Object"; readonly options: Options },
  carrier?: Guardrail.Carrier,
): ReturnType<typeof _object<Options>>
function _generate<const Options extends Parameters<typeof LanguageModel.streamText>[0]>(
  policy: Guardrail.Policy,
  request: { readonly _tag: "Stream"; readonly options: Options },
  carrier?: Guardrail.Carrier,
): ReturnType<typeof _stream<Options>>
function _generate(policy: Guardrail.Policy, request: Guardrail.Request, carrier: Guardrail.Carrier = _free) {
  return Match.value(request).pipe(
    Match.tag("Text", ({ options }) => _text(policy, options, carrier)),
    Match.tag("Object", ({ options }) => _object(policy, options, carrier)),
    Match.tag("Stream", ({ options }) => _stream(policy, options, carrier)),
    Match.exhaustive,
  )
}

const Guardrail = { generate: _generate, admitted: _admitted }

const _spend = (tier: Ladder.Tier, usage: Response.Usage): BigDecimal.BigDecimal =>
  BigDecimal.sumAll([
    BigDecimal.multiply(BigDecimal.fromNumber(usage.inputTokens ?? 0), tier.rate.input),
    BigDecimal.multiply(BigDecimal.fromNumber(usage.cachedInputTokens ?? 0), tier.rate.cachedInput),
    BigDecimal.multiply(BigDecimal.fromNumber(usage.outputTokens ?? 0), tier.rate.output),
    BigDecimal.multiply(BigDecimal.fromNumber(usage.reasoningTokens ?? 0), tier.rate.reasoning),
  ])

const _finish = (content: ReadonlyArray<Response.AnyPart>): Option.Option<Response.FinishPart> =>
  Array.findFirst(content, (part): part is Response.FinishPart => part.type === "finish")

const _settled = {
  aggregate: (content: ReadonlyArray<Response.AnyPart>) =>
    _finish(content).pipe(
      Option.flatMapNullable((part) => part.metadata.openrouter?.usage?.cost),
      Option.flatMap(BigDecimal.safeFromNumber),
    ),
  metered: () => Option.none<BigDecimal.BigDecimal>(),
} as const satisfies Record<
  Providers.Capability["cost"],
  (content: ReadonlyArray<Response.AnyPart>) => Option.Option<BigDecimal.BigDecimal>
>

const _exact = (
  tier: Ladder.Tier,
  content: ReadonlyArray<Response.AnyPart>,
): Option.Option<BigDecimal.BigDecimal> => _settled[_providers[tier.provider].cells.cost](content)

const _annotations = {
  namespaced: (span: Tracer.Span, content: ReadonlyArray<Response.AnyPart>) =>
    Option.match(Option.flatMapNullable(_finish(content), (part) => part.metadata.openai?.serviceTier), {
      onNone: () => Effect.void,
      onSome: (serviceTier) => Effect.sync(() => OpenAiTelemetry.addGenAIAnnotations(span, { openai: { response: { serviceTier } } })),
    }),
  core: () => Effect.void,
} as const satisfies Record<
  Providers.Capability["telemetry"],
  (span: Tracer.Span, content: ReadonlyArray<Response.AnyPart>) => Effect.Effect<void>
>

const _accounted = <Tools extends Record<string, Tool.Any>>(
  tier: Ladder.Tier,
  response: LanguageModel.GenerateTextResponse<Tools>,
): Effect.Effect<BigDecimal.BigDecimal> =>
  Effect.as(
    Effect.flatMap(
      Effect.optionFromOptional(Effect.currentSpan),
      Option.match({
        onNone: () => Effect.void,
        onSome: (span) =>
          Effect.sync(() =>
            Telemetry.addGenAIAnnotations(span, {
              system: _providers[tier.provider].cells.system,
              operation: { name: "chat" },
              request: { model: tier.model, ...tier.sampling },
              usage: { inputTokens: response.usage.inputTokens, outputTokens: response.usage.outputTokens },
            })).pipe(Effect.zipRight(_annotations[_providers[tier.provider].cells.telemetry](span, response.content))),
      }),
    ),
    Option.getOrElse(_exact(tier, response.content), () => _spend(tier, response.usage)),
  )

const Spend = { of: _spend, exact: _exact, accounted: _accounted, finish: _finish }
```

## [05]-[TOKENS]

[TOKENS]:
- Owner: `Tokens` — the token economy bound at the `Tokenizer` Tag. A total provider roster returns an exact Anthropic/OpenAI meter or an explicitly supplied exact fallback for Google, Bedrock, and OpenRouter; `Tokens.Budget` admits positive integer `{ window, reply }` values only when `reply < window`, `Tokens.gauge` measures a prompt, and `Tokens.fit` truncates to the admitted `window - reply` capacity before the wire.
- Law: assembly is measured greedy selection, and MEASURE is split from FOLD — `Tokens.weave(system, passages, budget)` prices every rank-ordered passage in one unbounded-concurrency `Effect.forEach` because measurement is independent per passage, then a pure `Array.reduce` admits passages while the running total fits the window's retrieval share, because only the total is sequential; a serialized meter walk pays one round-trip per passage on the critical path of every turn against a keyed or remote-metered tokenizer, and greedy admission semantics are identical either way. Admitted passages fold into origin-attributed system blocks as TEXT and seat through ONE `Prompt.setSystem` write, because the appending combinators keep the message they merged from and a per-passage call would leave one duplicated system message per admission; passages arrive as app-passed values from the caller's retrieval read, so this fold never imports the data wave.
- Law: the tokenizer is ambient — every gauge and fit yields the Tag; the row that satisfies it is the tier's provider cell, folded into the requirement set by the `*WithTokenizer` model arms where the provider ships one.
- Growth: a per-provider exact meter for an unmetered row is one roster entry; a budget shape change (per-tool reply reserves) is one field on the pair.
- Packages: `@effect/ai` (`Tokenizer`, `Prompt`); `@effect/ai-anthropic` (`AnthropicTokenizer`); `@effect/ai-openai` (`OpenAiTokenizer`); `effect` (`Effect`, `Array`, `Order`).

```typescript
const TokenBudget = Schema.Struct({
  window: Schema.Int.pipe(Schema.positive()),
  reply: Schema.Int.pipe(Schema.positive()),
}).pipe(Schema.filter((budget) => budget.reply < budget.window, { identifier: "ReplyWithinWindow" }))

declare namespace Tokens {
  type Pair = Schema.Schema.Type<typeof TokenBudget>
  type Passage = { readonly origin: string; readonly rank: number; readonly body: string }
  type Exact = Exclude<Providers.Capability["tokenizer"], "fallback">
  type ExactMeter = { readonly meter: Exact; readonly model: string }
}

const _exactMeters = {
  value: (_model: string) => Layer.succeed(Tokenizer.Tokenizer, AnthropicTokenizer.make),
  keyed: (model: string) => OpenAiTokenizer.layer({ model }),
} as const satisfies Record<Tokens.Exact, (model: string) => Layer.Layer<Tokenizer.Tokenizer>>

const _meters = {
  value: _exactMeters.value,
  keyed: _exactMeters.keyed,
  fallback: (_model: string, fallback: Tokens.ExactMeter) => _exactMeters[fallback.meter](fallback.model),
} as const satisfies Record<
  Providers.Capability["tokenizer"],
  (model: string, fallback: Tokens.ExactMeter) => Layer.Layer<Tokenizer.Tokenizer>
>

const _gauge = (prompt: Prompt.RawInput) =>
  Tokenizer.Tokenizer.pipe(Effect.flatMap((meter) => meter.tokenize(prompt)), Effect.map((tokens) => tokens.length))

const _fit = (prompt: Prompt.RawInput, pair: Tokens.Pair) =>
  Tokenizer.Tokenizer.pipe(Effect.flatMap((meter) => meter.truncate(prompt, pair.window - pair.reply)))

const _weave = (system: string, passages: ReadonlyArray<Tokens.Passage>, pair: Tokens.Pair) =>
  Effect.gen(function* () {
    const meter = yield* Tokenizer.Tokenizer
    const base = yield* meter.tokenize(system)
    const priced = yield* Effect.forEach(
      Array.sortBy(passages, Order.mapInput(Order.number, (passage: Tokens.Passage) => passage.rank)),
      (passage) => {
        const block = `[${passage.origin}] ${passage.body}`
        return Effect.map(meter.tokenize(block), (tokens) => ({ block, cost: tokens.length }))
      },
      { concurrency: "unbounded" },
    )
    const admitted = Array.reduce(priced, { blocks: [system], spent: base.length }, (held, { block, cost }) =>
      held.spent + cost > pair.window - pair.reply
        ? held
        : { blocks: Array.append(held.blocks, block), spent: held.spent + cost })
    return Prompt.setSystem(Prompt.empty, Array.join(admitted.blocks, "\n"))
  })

const Tokens = {
  Budget: TokenBudget,
  meter: (provider: Providers.Name, model: string, fallback: Tokens.ExactMeter) =>
    _meters[_providers[provider].cells.tokenizer](model, fallback),
  gauge: _gauge,
  fit: _fit,
  weave: _weave,
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Guardrail, GuardrailFault, Ladder, Providers, Spend, Tokens }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
