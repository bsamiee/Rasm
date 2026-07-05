# [RUNTIME_MODEL]

The intelligence spine: five provider families fold onto one capability-asymmetry table whose rows are `Model.make` values, fallback is the core `ExecutionPlan` engine driven through `Effect.withExecutionPlan` over interchangeable `Model` layers — the removed provider-plan abstraction has no successor here because the core engine IS the mechanism — and every generation crosses ONE guardrail gate: input screen, structural tool admission compiled from the `tool#SAFETY` partition into `toolChoice`, output sweep over text, object, and streaming modalities, and a typed refusal arm. The token economy lives on the same page because budget and gate are one admission: meter-relative window/reply budgets bound at the `Tokenizer` Tag (the Anthropic bare-value service and the model-keyed OpenAI factory are the two shipped meters), enforcement is `truncate` before the wire, and context assembly is a measured, rank-ordered greedy weave over app-passed retrieval values — retrieval is data, never a data-wave import. Cost is exact: a `BigDecimal` spend fold over the response `Usage` against per-row rates, with the aggregator's per-response cost slot preferred where its accessor settles. Business logic depends only on the `LanguageModel`/`Tokenizer` Tags; a provider is a row, never a fork. The module is `runtime/src/ai/model.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                       | [PUBLIC]    |
| :-----: | :-------------- | :------------------------------------------------------------------------------ | :---------- |
|  [01]   | `PROVIDER_ROWS` | the asymmetry table, client construction, the one transport requirement          | `Providers` |
|  [02]   | `LADDER`        | tier routing and fault-gated failover on the core execution-plan engine          | `Ladder`    |
|  [03]   | `GATE`          | the one guardrail — screen, admit, sweep (all three modalities), refusal, spend  | `Gate`      |
|  [04]   | `TOKENS`        | meter-relative budgets, truncation enforcement, the measured context weave       | `Tokens`    |

## [2]-[PROVIDER_ROWS]

[PROVIDER_ROWS]:
- Owner: `Providers` — the capability-asymmetry table as data: one row per family carrying its `Model.make` entry, its populated asymmetry cells, and its client Layer. The cells are facts, not code paths: `openai` is the reference row (language model on Responses, embeddings ×2 modalities, `OpenAiTokenizer.make({ model })`, four provider tools, a namespaced telemetry module), `anthropic` populates tokenizer (`AnthropicTokenizer.make` — a bare `Tokenizer.Service` value, not a factory) and five tool families, `google` carries raw-client embeddings only, `amazon-bedrock` carries SigV4 credentials and native guardrail traces, `openrouter` carries aggregator routing and per-response cost metadata. A consumer reads a cell; a `switch` over provider names is unspellable.
- Law: construction is uniform — every client is `layerConfig` with `Config.redacted` credentials over the `HttpClient` requirement `net/client`'s default-policy rows satisfy; a provider Layer never dials its own transport policy.
- Law: `Model.make(name, layer)` is both a `Layer` and an `Effect` and auto-provides the provider name Tag, so the ladder and the spend fold read the active row by yielding one Tag — provider identity is ambient, never threaded.
- Law: per-request steering is the provider's `Config` Tag written through `withConfigOverride` — the OpenAI `strict`/`verbosity` knobs, the Anthropic parallel-tool toggle — scoped per effect, never baked into a row.
- Growth: a sixth provider is one row with its cells; an asymmetry axis (caching, batching endpoint) is one column every row answers.
- Packages: `@effect/ai` (`Model`, `LanguageModel`); `@effect/ai-openai`, `@effect/ai-anthropic`, `@effect/ai-google`, `@effect/ai-amazon-bedrock`, `@effect/ai-openrouter`; `../net/client.ts` (`Client` — the `HttpClient` policy row).

```typescript
import { LanguageModel, Prompt, Response, Tokenizer } from "@effect/ai"
import { AnthropicClient, AnthropicLanguageModel, AnthropicTokenizer } from "@effect/ai-anthropic"
import { AmazonBedrockClient, AmazonBedrockLanguageModel } from "@effect/ai-amazon-bedrock"
import { GoogleClient, GoogleLanguageModel } from "@effect/ai-google"
import { OpenAiClient, OpenAiLanguageModel, OpenAiTokenizer } from "@effect/ai-openai"
import { OpenRouterClient, OpenRouterLanguageModel } from "@effect/ai-openrouter"
import { Array, BigDecimal, Config, Data, Effect, type ExecutionPlan, Layer, Option, Order, Stream } from "effect"
import { Budget, FaultClass } from "@rasm/ts/core"
import { Safety } from "./tool.ts"

const _providers = {
  openai: {
    model: OpenAiLanguageModel.model,
    client: OpenAiClient.layerConfig({ apiKey: Config.redacted("OPENAI_API_KEY") }),
    cells: { embed: "native", tokenizer: "keyed", tools: 4, telemetry: "namespaced" },
  },
  anthropic: {
    model: AnthropicLanguageModel.model,
    client: AnthropicClient.layerConfig({ apiKey: Config.redacted("ANTHROPIC_API_KEY") }),
    cells: { embed: "none", tokenizer: "value", tools: 5, telemetry: "core" },
  },
  google: {
    model: GoogleLanguageModel.model,
    client: GoogleClient.layerConfig({ apiKey: Config.redacted("GOOGLE_API_KEY") }),
    cells: { embed: "raw", tokenizer: "none", tools: 4, telemetry: "core" },
  },
  bedrock: {
    model: AmazonBedrockLanguageModel.model,
    client: AmazonBedrockClient.layerConfig({
      accessKeyId: Config.string("AWS_ACCESS_KEY_ID"),
      secretAccessKey: Config.redacted("AWS_SECRET_ACCESS_KEY"),
    }),
    cells: { embed: "none", tokenizer: "none", tools: 8, telemetry: "core" },
  },
  openrouter: {
    model: OpenRouterLanguageModel.model,
    client: OpenRouterClient.layerConfig({ apiKey: Config.redacted("OPENROUTER_API_KEY") }),
    cells: { embed: "none", tokenizer: "none", tools: 0, telemetry: "core" },
  },
} as const

declare namespace Providers {
  type Name = keyof typeof _providers
  type Row = (typeof _providers)[Name]
}

const Providers = { ..._providers, names: Object.keys(_providers) as ReadonlyArray<Providers.Name> }
```

## [3]-[LADDER]

[LADDER]:
- Owner: `Ladder` — tier routing as an execution plan: an ordered tier table (`fast` — the cheap high-volume row, `deep` — the reasoning row, `fallback` — the aggregator row that answers when a primary is down) compiles into one plan value whose steps each carry a provider `Model` layer, an attempt bound, a `Budget`-compiled pacing schedule, and the failover predicate — a step yields to the next only on a retryable `FaultClass` (`unavailable`, `exhausted`, `expired` folded from the provider's error union); an `invalid` request or a `denied` key fails immediately through every tier because retrying a deterministic rejection spends money to learn nothing.
- Law: the drive is `Effect.withExecutionPlan(effect, plan)` over an effect that depends only on the `LanguageModel` Tag — the plan swaps WHICH row satisfies it per step, so tier routing composes around any gated call without the call knowing tiers exist; `Stream.withExecutionPlan` is the streaming twin.
- Law: tier selection evidence rides the span — the settled step's provider name and attempt count annotate the generation span so cost attribution and failover health are queryable per call.
- RESEARCH: the plan-constructor step-record spelling (`ExecutionPlan.make` field names for the per-step layer, attempt count, schedule, and predicate) settles from the shipped `effect/ExecutionPlan` declaration before this fence's constructor literals leave RESEARCH; `Effect.withExecutionPlan` and the tier-table shape are settled law.
- Growth: a new tier is one table row; a per-tenant ladder is a table value selected by the caller's context.
- Packages: `effect` (`ExecutionPlan`, `Effect.withExecutionPlan`, `Schedule`); `@rasm/ts/core` (`Budget`, `FaultClass`).

```typescript
declare namespace Ladder {
  type Tier = {
    readonly name: "fast" | "deep" | "fallback"
    readonly provider: Providers.Name
    readonly model: string
    readonly attempts: number
    readonly budget: Budget.Kind
    readonly rate: { readonly input: BigDecimal.BigDecimal; readonly output: BigDecimal.BigDecimal }
  }
  type Table = readonly [Tier, ...Array<Tier>]
}

const _yields = (fault: unknown): boolean => FaultClass.retryable(fault)

declare const _plan: (table: Ladder.Table) => ExecutionPlan.ExecutionPlan<{ provides: LanguageModel.LanguageModel; input: unknown }>

const _tiered = <A, E, R>(table: Ladder.Table, call: Effect.Effect<A, E, R | LanguageModel.LanguageModel>) =>
  Effect.withExecutionPlan(call, _plan(table))

const Ladder = { drive: _tiered, yields: _yields }
```

## [4]-[GATE]

[GATE]:
- Owner: `Gate` — the one guardrail over every generation modality. `Gate.text`, `Gate.object`, and `Gate.stream` wrap `LanguageModel.generateText`, `generateObject`, and `streamText` in the same admission fold: the input screen runs first (a policy predicate over the assembled prompt — length, injection heuristics, tenant policy — refusing before any token is bought), tool admission compiles the `Safety.admit` partition into the call's `toolChoice` (`{ mode: "auto", oneOf: allowed }` — a held or denied tool is structurally uncallable, not merely discouraged) with `disableToolCallResolution: true` whenever held names exist so no handler runs before a supervisor releases the call, and the output sweep runs last. The sweep is modality-uniform: the text arm sweeps the settled response, the object arm validates through the caller's `Schema` (structured output IS the sweep's strongest form), and the streaming arm scans a sliding window over `TextDeltaPart` deltas — the window buffers the last unswept span, emits only swept prefixes, and a tripped window fails the stream into the refusal arm, so streaming is no longer the unswept modality.
- Law: refusal is typed — `Refusal` is a tagged union (`screened` — input refused; `swept` — output tripped; `provider` — the model's own content-filter finish reason) riding the error channel as a `denied`-classed fault; a caller distinguishes refusal from failure by tag, never by message text.
- Law: spend is folded per call — `Usage` (input, output, reasoning, cached tokens) multiplies against the settled tier's `BigDecimal` rate rows into an exact spend receipt annotated onto the span and metered as a fact; float arithmetic on money is unspellable.
- RESEARCH: the aggregator's per-response exact cost (the openrouter finish-part metadata slot) is preferred over the rate fold when its accessor path from a settled response settles from the shipped declaration; until then the rate fold is the one cost source.
- Growth: a screen or sweep policy is a predicate row on the gate's policy table; a new modality inherits the fold by construction.
- Packages: `@effect/ai` (`LanguageModel`, `Response`, `Prompt`); `effect` (`Stream`, `BigDecimal`, `Data`); `./tool.ts` (`Safety`).

```typescript
type Refusal = Data.TaggedEnum<{
  Screened: { readonly rule: string }
  Swept: { readonly span: string }
  Provider: { readonly reason: Response.FinishReason }
}>
const Refusal = Data.taggedEnum<Refusal>()

class GateFault extends Data.TaggedError("ModelRefusal")<{ readonly refusal: Refusal }> {
  readonly class: FaultClass.Kind = "denied"
}

declare namespace Gate {
  type Policy = {
    readonly screen: (prompt: Prompt.Prompt) => Option.Option<string>
    readonly sweep: (text: string) => Option.Option<string>
    readonly mode: Safety.Mode
    readonly window: number
  }
}

const _admitted = <Tools extends Record<string, unknown>>(policy: Gate.Policy, toolkit: { readonly graded: ReadonlyArray<{ readonly name: string; readonly clazz: Safety.Class }> }) => {
  const admission = Safety.admit(toolkit.graded, policy.mode)
  return {
    toolChoice: { mode: "auto" as const, oneOf: admission.allowed },
    disableToolCallResolution: admission.held.length > 0,
    held: admission.held,
  }
}

const _screened = (policy: Gate.Policy, prompt: Prompt.Prompt) =>
  Option.match(policy.screen(prompt), {
    onNone: () => Effect.void,
    onSome: (rule) => Effect.fail(new GateFault({ refusal: Refusal.Screened({ rule }) })),
  })

const _sweepStream = (policy: Gate.Policy) => <E, R>(parts: Stream.Stream<Response.StreamPart<any>, E, R>) =>
  parts.pipe(
    Stream.mapAccumEffect("", (window, part) =>
      part.type === "text-delta"
        ? Option.match(policy.sweep(window + part.delta), {
          onNone: () => Effect.succeed([(window + part.delta).slice(-policy.window), Option.some(part)] as const),
          onSome: (span) => Effect.fail(new GateFault({ refusal: Refusal.Swept({ span }) })),
        })
        : Effect.succeed([window, Option.some(part)] as const)),
    Stream.filterMap((part) => part),
  )

const Gate = {
  text: (policy: Gate.Policy) => <Options extends Parameters<typeof LanguageModel.generateText>[0]>(options: Options) =>
    _screened(policy, Prompt.make(options.prompt)).pipe(
      Effect.zipRight(LanguageModel.generateText(options)),
      Effect.filterOrFail(
        (response) => Option.isNone(policy.sweep(response.text)),
        (response) => new GateFault({ refusal: Refusal.Swept({ span: response.text.slice(0, 80) }) }),
      ),
    ),
  object: (policy: Gate.Policy) => <Options extends Parameters<typeof LanguageModel.generateObject>[0]>(options: Options) =>
    _screened(policy, Prompt.make(options.prompt)).pipe(Effect.zipRight(LanguageModel.generateObject(options))),
  stream: (policy: Gate.Policy) => <Options extends Parameters<typeof LanguageModel.streamText>[0]>(options: Options) =>
    Stream.unwrap(_screened(policy, Prompt.make(options.prompt)).pipe(
      Effect.as(LanguageModel.streamText(options).pipe(_sweepStream(policy))),
    )),
  admitted: _admitted,
}

const _spend = (tier: Ladder.Tier, usage: Response.Usage): BigDecimal.BigDecimal =>
  BigDecimal.sum(
    BigDecimal.multiply(BigDecimal.fromNumber(usage.inputTokens ?? 0), tier.rate.input),
    BigDecimal.multiply(BigDecimal.fromNumber(usage.outputTokens ?? 0), tier.rate.output),
  )

const Spend = { of: _spend }
```

## [5]-[TOKENS]

[TOKENS]:
- Owner: `Tokens` — the token economy bound at the `Tokenizer` Tag: the meter roster is a row table (`anthropic` provides the bare `AnthropicTokenizer.make` service value, `openai` the model-keyed `OpenAiTokenizer.make({ model })`, and the unmetered rows — google, bedrock, openrouter — meter through the default row as a stated approximation), the budget is `{ window, reply }` token pairs per tier, `Tokens.gauge` measures a prompt against its window, and `Tokens.fit` enforces — `Tokenizer.truncate(prompt, window - reply)` before the wire so a call never buys a context overflow.
- Law: assembly is measured greedy selection — `Tokens.weave(system, passages, budget)` measures each rank-ordered retrieval passage, admits passages while the running total fits the window's retrieval share, and folds admitted passages into origin-attributed system blocks through `Prompt.appendSystem`; passages arrive as app-passed values from the caller's retrieval read, so this fold never imports the data wave.
- Law: the tokenizer is ambient — every gauge and fit yields the Tag; the row that satisfies it is the tier's provider cell, folded into the provides set by the `*WithTokenizer` model arms where the provider ships one.
- Growth: a per-provider exact meter for an unmetered row is one roster entry; a budget shape change (per-tool reply reserves) is one field on the pair.
- Packages: `@effect/ai` (`Tokenizer`, `Prompt`); `@effect/ai-anthropic` (`AnthropicTokenizer`); `@effect/ai-openai` (`OpenAiTokenizer`); `effect` (`Effect`, `Array`, `Order`).

```typescript
declare namespace Tokens {
  type Pair = { readonly window: number; readonly reply: number }
  type Passage = { readonly origin: string; readonly rank: number; readonly body: string }
}

const _meters = {
  anthropic: Layer.succeed(Tokenizer.Tokenizer, AnthropicTokenizer.make),
  openai: (model: string) => OpenAiTokenizer.layer({ model }),
} as const

const _fit = (prompt: Prompt.RawInput, pair: Tokens.Pair) =>
  Tokenizer.Tokenizer.pipe(Effect.flatMap((meter) => meter.truncate(prompt, pair.window - pair.reply)))

const _weave = (system: string, passages: ReadonlyArray<Tokens.Passage>, pair: Tokens.Pair) =>
  Tokenizer.Tokenizer.pipe(
    Effect.flatMap((meter) =>
      Effect.reduce(
        Array.sortBy(passages, Order.mapInput(Order.number, (passage: Tokens.Passage) => passage.rank)),
        { prompt: Prompt.make(system), spent: 0 },
        (state, passage) =>
          meter.tokenize(passage.body).pipe(
            Effect.map((tokens) =>
              state.spent + tokens.length > pair.window - pair.reply
                ? state
                : {
                  prompt: Prompt.appendSystem(state.prompt, `[${passage.origin}] ${passage.body}`),
                  spent: state.spent + tokens.length,
                }
            ),
          ),
      )
    ),
    Effect.map((state) => state.prompt),
  )

const Tokens = { meters: _meters, fit: _fit, weave: _weave }

// --- [EXPORTS] --------------------------------------------------------------------------

export { Gate, GateFault, Ladder, Providers, Refusal, Spend, Tokens }
```
