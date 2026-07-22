# [RUNTIME_MODEL]

The intelligence spine: five provider families fold onto one capability-asymmetry table whose rows are `Model.make` values, fallback is the core `ExecutionPlan` engine driven through `Effect.withExecutionPlan` over interchangeable `Model` layers — the removed provider-plan abstraction has no successor here because the core engine IS the mechanism — and every generation crosses ONE guardrail gate: input screen, structural tool admission compiled from the `tool#SAFETY` partition into `toolChoice`, output sweep over text, object, and streaming modalities, and a typed refusal arm. The token economy lives on the same page because budget and gate are one admission: meter-relative window/reply budgets bound at the `Tokenizer` Tag (the Anthropic bare-value service and the model-keyed OpenAI factory are the two shipped meters), enforcement is `truncate` before the wire, and context assembly is a measured, rank-ordered greedy weave over app-passed retrieval values — retrieval is data, never a data-wave import. Cost is exact: a `BigDecimal` spend fold over the response `Usage` against per-row rates, with the aggregator's settled per-response cost (`FinishPart.metadata.openrouter.usage.cost`, USD) taking precedence where the row carries it. Business logic depends only on the `LanguageModel`/`Tokenizer` Tags; a provider is a row, never a fork. The module is `runtime/src/ai/model.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                          | [PUBLIC]    |
| :-----: | :-------------- | :------------------------------------------------------------------------------ | :---------- |
|  [01]   | `PROVIDER_ROWS` | the asymmetry table, client construction, the one transport requirement         | `Providers` |
|  [02]   | `LADDER`        | tier routing and fault-gated failover on the core execution-plan engine         | `Ladder`    |
|  [03]   | `GATE`          | the one guardrail — screen, admit, sweep (all three modalities), refusal, spend | `Guardrail` |
|  [04]   | `TOKENS`        | meter-relative budgets, truncation enforcement, the measured context weave      | `Tokens`    |

## [02]-[PROVIDER_ROWS]

[PROVIDER_ROWS]:
- Owner: `Providers` — the capability-asymmetry table as data: one row per family carrying its `Model.make` entry, its populated asymmetry cells, and its client Layer. The cells are facts, not code paths: `openai` is the reference row (language model on Responses, embeddings ×2 modalities, `OpenAiTokenizer.make({ model })`, four provider tools, a namespaced telemetry module), `anthropic` populates tokenizer (`AnthropicTokenizer.make` — a bare `Tokenizer.Service` value, not a factory) and five tool families, `google` carries raw-client embeddings only, `bedrock` carries SigV4 credentials and native guardrail traces, and `openrouter` carries aggregator routing and per-response cost metadata. Each row also carries the semantic-convention `system` identity (`gemini` and `aws.bedrock`, not local provider aliases) used by accounting. A consumer reads a cell; a `switch` over provider names is unspellable.
- Law: construction is uniform — every client is `layerConfig` with `Config.redacted` credentials over the `HttpClient` requirement `net/client`'s default-policy rows satisfy; a provider Layer never dials its own transport policy.
- Law: `Model.make(name, layer)` is both a `Layer` and an `Effect` and carries the provider name Tag, so the ladder and the spend fold read the active row by yielding one Tag — provider identity is ambient, never threaded.
- Law: per-request steering is the provider's `Config` Tag written through `withConfigOverride` — the OpenAI `strict`/`verbosity` knobs, the Anthropic parallel-tool toggle — scoped per effect, never baked into a row.
- Growth: a sixth provider is one row with its cells; an asymmetry axis (caching, batching endpoint) is one column every row answers.
- Packages: `@effect/ai` (`Model`, `LanguageModel`); `@effect/ai-openai`, `@effect/ai-anthropic`, `@effect/ai-google`, `@effect/ai-amazon-bedrock`, `@effect/ai-openrouter`; `../net/client.ts` (`Client` — the `HttpClient` policy row).

```typescript
import { LanguageModel, Prompt, Response, Telemetry, Tokenizer, type Tool } from "@effect/ai"
import { AnthropicClient, AnthropicLanguageModel, AnthropicTokenizer } from "@effect/ai-anthropic"
import { AmazonBedrockClient, AmazonBedrockLanguageModel } from "@effect/ai-amazon-bedrock"
import { GoogleClient, GoogleLanguageModel } from "@effect/ai-google"
import { OpenAiClient, OpenAiLanguageModel, OpenAiTokenizer } from "@effect/ai-openai"
import { OpenRouterClient, OpenRouterLanguageModel } from "@effect/ai-openrouter"
import { Array, BigDecimal, Chunk, Config, Data, Effect, ExecutionPlan, Layer, Match, Number, Option, Order, Schema, Stream, Struct } from "effect"
import { Budget, FaultClass } from "@rasm/ts/core"
import { Safety } from "./tool.ts"

declare namespace Providers {
  type Capability = {
    readonly generation: readonly ["text", "object", "stream"]
    readonly embed: "native" | "raw" | "none"
    readonly tokenizer: "keyed" | "value" | "fallback"
    readonly tools: number
    readonly telemetry: "namespaced" | "core"
    readonly routing: "direct" | "aggregate"
    readonly exactCost: boolean
    readonly auth: "api-key" | "sigv4"
    readonly configOverride: boolean
    readonly system: NonNullable<Telemetry.BaseAttributes["system"]>
  }
}

const _providers = {
  openai: {
    model: OpenAiLanguageModel.model,
    client: OpenAiClient.layerConfig({ apiKey: Config.redacted("OPENAI_API_KEY") }),
    cells: {
      generation: ["text", "object", "stream"], embed: "native", tokenizer: "keyed", tools: 4,
      telemetry: "namespaced", routing: "direct", exactCost: false, auth: "api-key", configOverride: true, system: "openai",
    },
  },
  anthropic: {
    model: AnthropicLanguageModel.model,
    client: AnthropicClient.layerConfig({ apiKey: Config.redacted("ANTHROPIC_API_KEY") }),
    cells: {
      generation: ["text", "object", "stream"], embed: "none", tokenizer: "value", tools: 5,
      telemetry: "core", routing: "direct", exactCost: false, auth: "api-key", configOverride: true, system: "anthropic",
    },
  },
  google: {
    model: GoogleLanguageModel.model,
    client: GoogleClient.layerConfig({ apiKey: Config.redacted("GOOGLE_API_KEY") }),
    cells: {
      generation: ["text", "object", "stream"], embed: "raw", tokenizer: "fallback", tools: 4,
      telemetry: "core", routing: "direct", exactCost: false, auth: "api-key", configOverride: true, system: "gemini",
    },
  },
  bedrock: {
    model: AmazonBedrockLanguageModel.model,
    client: AmazonBedrockClient.layerConfig({
      accessKeyId: Config.string("AWS_ACCESS_KEY_ID"),
      secretAccessKey: Config.redacted("AWS_SECRET_ACCESS_KEY"),
    }),
    cells: {
      generation: ["text", "object", "stream"], embed: "none", tokenizer: "fallback", tools: 8,
      telemetry: "core", routing: "direct", exactCost: false, auth: "sigv4", configOverride: true, system: "aws.bedrock",
    },
  },
  openrouter: {
    model: OpenRouterLanguageModel.model,
    client: OpenRouterClient.layerConfig({ apiKey: Config.redacted("OPENROUTER_API_KEY") }),
    cells: {
      generation: ["text", "object", "stream"], embed: "none", tokenizer: "fallback", tools: 0,
      telemetry: "core", routing: "aggregate", exactCost: true, auth: "api-key", configOverride: true, system: "openrouter",
    },
  },
} as const satisfies Record<string, { readonly model: unknown; readonly client: unknown; readonly cells: Providers.Capability }>

declare namespace Providers {
  type Name = keyof typeof _providers
  type Row = (typeof _providers)[Name]
}

const Providers = { ..._providers, names: Struct.keys(_providers) }
```

## [03]-[LADDER]

[LADDER]:
- Owner: `Ladder` — an arbitrary non-empty ordered tier table compiles into one execution plan; tier names are data, while every row carries a provider `Model` layer, attempt bound, `Budget` pacing schedule, failover predicate, and four-band token rates. `fast`, `deep`, and `fallback` are ordinary seed values rather than a closed type roster, so tenant-specific ladders and five-times-larger plans require no surface edit.
- Law: the drive is `Effect.withExecutionPlan(effect, plan)` over an effect that depends only on the `LanguageModel` Tag — the plan swaps WHICH row satisfies it per step, so tier routing composes around any gated call without the call knowing tiers exist; `Stream.withExecutionPlan` is the streaming twin.
- Law: tier selection evidence rides the span — the settled step's provider name and attempt count annotate the generation span so cost attribution and failover health are queryable per call.
- Law: the plan compiles from the tier table alone — `ExecutionPlan.make` receives one step record per tier (`provide` takes the provider row's `Model` value directly because a `Model` IS a `Layer`, `attempts` the tier bound, `schedule` the `Budget.schedule(kind)` compiled pacing value, `while` the retryability probe over `FaultClass`), and `Array.map` over the non-empty tier tuple preserves the non-empty step arity `make` demands.
- Growth: a new tier is one table row; a per-tenant ladder is a table value selected by the caller's context.
- Packages: `effect` (`ExecutionPlan`, `Effect.withExecutionPlan`, `Schedule`); `@rasm/ts/core` (`Budget`, `FaultClass`).

```typescript
declare namespace Ladder {
  type Tier = {
    readonly name: string
    readonly provider: Providers.Name
    readonly model: string
    readonly attempts: number
    readonly budget: Budget.Kind
    readonly rate: {
      readonly input: BigDecimal.BigDecimal
      readonly cachedInput: BigDecimal.BigDecimal
      readonly output: BigDecimal.BigDecimal
      readonly reasoning: BigDecimal.BigDecimal
    }
  }
  type Table = readonly [Tier, ...Array<Tier>]
}

const _yields = (fault: unknown): boolean => FaultClass[FaultClass.of(fault)].retryable

const _step = (tier: Ladder.Tier) => ({
  provide: _providers[tier.provider].model(tier.model),
  attempts: tier.attempts,
  schedule: Budget.schedule(tier.budget),
  while: (fault: unknown) => Effect.succeed(_yields(fault)),
})

const _plan = (table: Ladder.Table) => ExecutionPlan.make(...Array.map(table, _step))

const _tiered = <A, E, R>(table: Ladder.Table, call: Effect.Effect<A, E, R | LanguageModel.LanguageModel>) =>
  Effect.withExecutionPlan(call, _plan(table))

const Ladder = { drive: _tiered, yields: _yields }
```

## [04]-[GATE]

[GATE]:
- Owner: `Guardrail.generate(policy, request)` — one request-shape-discriminated entry over `Text`, `Object`, and `Stream` modalities. Every arm screens the prompt, validates forced tool choice against the admitted roster, spreads `toolChoice` and `disableToolCallResolution` into the provider request, detects provider refusal, and sweeps output; modality changes the carrier, never the guardrail surface.
- Law: the stream sweep retains withheld OUTPUT and its source id. Each text delta appends, sweeps the whole held window, and releases only the prefix older than `policy.window`; before every non-text part it sweeps and flushes the residual text first, preserving source order instead of allowing tool or metadata parts to overtake withheld text. `text-end` is the same boundary rule, and a match fails before any byte in the matched span emits.
- Law: admission modes are policy rows — `Safety.admit` partitions the graded roster into executable `allowed` tools and visible-but-unresolved `held` tools, and the provider's `oneOf` receives their union while `disableToolCallResolution` prevents local execution whenever `held` is non-empty. An empty union compiles to `"none"`; mandatory choice compiles to `{ mode: "required", oneOf: visible }`; and a forced tool outside the visible union is a policy defect, never a silent escalation. This preserves held-call evidence for the agent approval loop without making the held tool executable.
- Law: refusal is typed — `Refusal` is a tagged union (`screened` — input refused; `swept` — output tripped; `provider` — the model's own content-filter finish reason) riding the error channel as a `denied`-classed fault; a caller distinguishes refusal from failure by tag, never by message text.
- Law: spend and telemetry fold per call as one accounting — `Spend.accounted(tier, response)` takes the aggregator's settled exact cost where the row carries one (`FinishPart.metadata.openrouter.usage.cost`, USD, admitted through `BigDecimal.safeFromNumber`) and otherwise multiplies `Usage` (input, output, reasoning, cached tokens) against the tier's `BigDecimal` rate rows; `Telemetry.addGenAIAnnotations(span, { system, operation, request, usage })` writes the standard `gen_ai.*` attribute set onto the generation span — the provider `*Telemetry` modules extend the same semconv, so every exporter reads standard attributes with zero bespoke mapping; float arithmetic on money and hand-named span attributes are both unspellable.
- Law: tool-call ids are pluggable — the `IdGenerator` Tag rides the requirement set, and a durable agent supplies a deterministic generator Layer at its root so replayed turns mint identical tool-call ids and the workflow journal stays byte-stable across replay.
- Growth: a screen or sweep policy is a predicate row on the gate's policy table; a new modality inherits the fold by construction.
- Packages: `@effect/ai` (`LanguageModel`, `Response`, `Prompt`); `effect` (`Stream`, `Chunk`, `BigDecimal`, `Data`); `./tool.ts` (`Safety`).

```typescript
type Refusal = Data.TaggedEnum<{
  Screened: { readonly rule: string }
  Swept: { readonly span: string }
  Provider: { readonly reason: Response.FinishReason }
}>
const Refusal = Data.taggedEnum<Refusal>()

class GuardrailFault extends Data.TaggedError("GuardrailFault")<{ readonly refusal: Refusal }> {
  readonly class: FaultClass.Kind = "denied"
}

declare namespace Guardrail {
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
          ? Effect.fail(new GuardrailFault({ refusal: Refusal.Screened({ rule: "tool:required" }) }))
          : Effect.succeed({
            toolChoice: { mode: "required" as const, oneOf: visible },
            disableToolCallResolution: disabled,
            held: admission.held,
          } as const)
      }
      return Array.contains(visible, forced.tool)
        ? Effect.succeed({ toolChoice: forced, disableToolCallResolution: disabled, held: admission.held } as const)
        : Effect.fail(new GuardrailFault({ refusal: Refusal.Screened({ rule: `tool:${forced.tool}` }) }))
    },
  })
}

const _screened = (policy: Guardrail.Policy, prompt: Prompt.Prompt) =>
  Option.match(policy.screen(prompt), {
    onNone: () => Effect.void,
    onSome: (rule) => Effect.fail(new GuardrailFault({ refusal: Refusal.Screened({ rule }) })),
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
          onSome: (span) => Effect.fail(new GuardrailFault({ refusal: Refusal.Swept({ span }) })),
        })
        : part.type === "finish" && part.reason === "content-filter"
          ? Effect.fail(new GuardrailFault({ refusal: Refusal.Provider({ reason: part.reason }) }))
          : Option.match(policy.sweep(state.held), {
            onNone: () => Effect.succeed([
              { held: "", id: "" },
              state.held.length > 0
                ? Chunk.make<Response.StreamPart<Tools>>(
                  Response.makePart("text-delta", { id: state.id, delta: state.held }),
                  part,
                )
                : Chunk.of<Response.StreamPart<Tools>>(part),
            ] as const),
            onSome: (span) => Effect.fail(new GuardrailFault({ refusal: Refusal.Swept({ span }) })),
          })),
    Stream.flattenChunks,
  )

const _request = <Options extends object>(policy: Guardrail.Policy, options: Options) =>
  Effect.map(_admitted(policy), (gate) => ({
    ...options,
    toolChoice: gate.toolChoice,
    disableToolCallResolution: gate.disableToolCallResolution,
  } as const))

const _swept = <Tools extends Record<string, Tool.Any>>(policy: Guardrail.Policy) =>
(response: LanguageModel.GenerateTextResponse<Tools>) =>
  response.finishReason === "content-filter"
    ? Effect.fail(new GuardrailFault({ refusal: Refusal.Provider({ reason: response.finishReason }) }))
    : Option.match(policy.sweep(response.text), {
      onNone: () => Effect.succeed(response),
      onSome: (span) => Effect.fail(new GuardrailFault({ refusal: Refusal.Swept({ span }) })),
    })

const _text = <Options extends Parameters<typeof LanguageModel.generateText>[0]>(policy: Guardrail.Policy, options: Options) =>
  _screened(policy, Prompt.make(options.prompt)).pipe(
    Effect.zipRight(_request(policy, options)),
    Effect.flatMap(LanguageModel.generateText),
    Effect.flatMap(_swept(policy)),
  )

const _object = <Options extends Parameters<typeof LanguageModel.generateObject>[0]>(policy: Guardrail.Policy, options: Options) =>
  _screened(policy, Prompt.make(options.prompt)).pipe(
    Effect.zipRight(_request(policy, options)),
    Effect.flatMap(LanguageModel.generateObject),
    Effect.flatMap((response) =>
      response.finishReason === "content-filter"
        ? Effect.fail(new GuardrailFault({ refusal: Refusal.Provider({ reason: response.finishReason }) }))
        : Option.match(policy.sweep(response.text), {
          onNone: () => Effect.succeed(response),
          onSome: (span) => Effect.fail(new GuardrailFault({ refusal: Refusal.Swept({ span }) })),
        })),
  )

const _stream = <Options extends Parameters<typeof LanguageModel.streamText>[0]>(policy: Guardrail.Policy, options: Options) =>
  Stream.unwrap(_screened(policy, Prompt.make(options.prompt)).pipe(
    Effect.zipRight(_request(policy, options)),
    Effect.map((admitted) => LanguageModel.streamText(admitted).pipe(_sweepStream(policy))),
  ))

function _generate<const Options extends Parameters<typeof LanguageModel.generateText>[0]>(
  policy: Guardrail.Policy,
  request: { readonly _tag: "Text"; readonly options: Options },
): ReturnType<typeof _text<Options>>
function _generate<const Options extends Parameters<typeof LanguageModel.generateObject>[0]>(
  policy: Guardrail.Policy,
  request: { readonly _tag: "Object"; readonly options: Options },
): ReturnType<typeof _object<Options>>
function _generate<const Options extends Parameters<typeof LanguageModel.streamText>[0]>(
  policy: Guardrail.Policy,
  request: { readonly _tag: "Stream"; readonly options: Options },
): ReturnType<typeof _stream<Options>>
function _generate(policy: Guardrail.Policy, request: Guardrail.Request) {
  return Match.value(request).pipe(
    Match.tag("Text", ({ options }) => _text(policy, options)),
    Match.tag("Object", ({ options }) => _object(policy, options)),
    Match.tag("Stream", ({ options }) => _stream(policy, options)),
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

const _exact = (content: ReadonlyArray<Response.AnyPart>): Option.Option<BigDecimal.BigDecimal> =>
  Array.findFirst(content, (part): part is Response.FinishPart => part.type === "finish").pipe(
    Option.flatMapNullable((part) => part.metadata.openrouter?.usage?.cost),
    Option.flatMap(BigDecimal.safeFromNumber), // the float-to-exact admission: an unrepresentable provider float refuses instead of drifting
  )

const _accounted = (tier: Ladder.Tier, response: LanguageModel.GenerateTextResponse<Record<string, Tool.Any>>): Effect.Effect<BigDecimal.BigDecimal> =>
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
              request: { model: tier.model },
              usage: { inputTokens: response.usage.inputTokens, outputTokens: response.usage.outputTokens },
            })),
      }),
    ),
    Option.getOrElse(_exact(response.content), () => _spend(tier, response.usage)),
  )

const Spend = { of: _spend, exact: _exact, accounted: _accounted }
```

## [05]-[TOKENS]

[TOKENS]:
- Owner: `Tokens` — the token economy bound at the `Tokenizer` Tag. A total provider roster returns an exact Anthropic/OpenAI meter or an explicitly supplied exact fallback for Google, Bedrock, and OpenRouter; `Tokens.Budget` admits positive integer `{ window, reply }` values only when `reply < window`, `Tokens.gauge` measures a prompt, and `Tokens.fit` truncates to the admitted `window - reply` capacity before the wire.
- Law: assembly is measured greedy selection — `Tokens.weave(system, passages, budget)` measures each rank-ordered retrieval passage, admits passages while the running total fits the window's retrieval share, and folds admitted passages into origin-attributed system blocks through `Prompt.appendSystem`; passages arrive as app-passed values from the caller's retrieval read, so this fold never imports the data wave.
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
  type ExactMeter = { readonly provider: "anthropic" | "openai"; readonly model: string }
}

const _approximate = (fallback: Tokens.ExactMeter): Layer.Layer<Tokenizer.Tokenizer> =>
  fallback.provider === "anthropic"
    ? Layer.succeed(Tokenizer.Tokenizer, AnthropicTokenizer.make)
    : OpenAiTokenizer.layer({ model: fallback.model })

const _meters = {
  anthropic: (_model: string, _fallback: Tokens.ExactMeter) => Layer.succeed(Tokenizer.Tokenizer, AnthropicTokenizer.make),
  openai: (model: string, _fallback: Tokens.ExactMeter) => OpenAiTokenizer.layer({ model }),
  google: (_model: string, fallback: Tokens.ExactMeter) => _approximate(fallback),
  bedrock: (_model: string, fallback: Tokens.ExactMeter) => _approximate(fallback),
  openrouter: (_model: string, fallback: Tokens.ExactMeter) => _approximate(fallback),
} as const satisfies Record<Providers.Name, (model: string, fallback: Tokens.ExactMeter) => Layer.Layer<Tokenizer.Tokenizer>>

const _gauge = (prompt: Prompt.RawInput) =>
  Tokenizer.Tokenizer.pipe(Effect.flatMap((meter) => meter.tokenize(prompt)), Effect.map((tokens) => tokens.length))

const _fit = (prompt: Prompt.RawInput, pair: Tokens.Pair) =>
  Tokenizer.Tokenizer.pipe(Effect.flatMap((meter) => meter.truncate(prompt, pair.window - pair.reply)))

const _weave = (system: string, passages: ReadonlyArray<Tokens.Passage>, pair: Tokens.Pair) =>
  Effect.gen(function* () {
    const meter = yield* Tokenizer.Tokenizer
    const base = yield* meter.tokenize(system)
    const state = yield* Effect.reduce(
      Array.sortBy(passages, Order.mapInput(Order.number, (passage: Tokens.Passage) => passage.rank)),
      { prompt: Prompt.make(system), spent: base.length },
      (held, passage) => {
        const block = `[${passage.origin}] ${passage.body}`
        return Effect.map(meter.tokenize(block), (tokens) =>
          held.spent + tokens.length > pair.window - pair.reply
            ? held
            : { prompt: Prompt.appendSystem(held.prompt, block), spent: held.spent + tokens.length })
      },
    )
    return state.prompt
  })

const Tokens = {
  Budget: TokenBudget,
  meter: (provider: Providers.Name, model: string, fallback: Tokens.ExactMeter) => _meters[provider](model, fallback),
  gauge: _gauge,
  fit: _fit,
  weave: _weave,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Guardrail, GuardrailFault, Ladder, Providers, Refusal, Spend, Tokens }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
