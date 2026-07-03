# [AI_TOKEN]

Token budgets bind at one Tag and context assembles from values: every budget read and truncation rides `Tokenizer.Tokenizer` — which concrete meter satisfies it is a roster row, with `AnthropicTokenizer` as the canonical default and the OpenAI meter as the model-keyed row — and retrieval context arrives as app-passed `Passage` values the weave folds into a `Prompt` under a token allotment, never through a `store` import. The two owners split cleanly: `Budget` decides how many tokens a prompt may spend and enforces the ceiling through `truncate`; `Assembly` decides which passages earn their tokens and where they sit in the prompt. Both are consumed by `model/provider`'s gate before any call and by `agent/memory`'s compaction after every turn, so the token economy has exactly one vocabulary across the folder.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]  | [OWNS]                                                                     |
| :-----: | :--------- | :-------------------------------------------------------------------------- |
|  [01]   | [BUDGET]   | the meter roster, the window/reserve budget rows, gauge and fit enforcement  |
|  [02]   | [CONTEXT]  | the `Passage` value, ranked greedy selection, and the system-block weave     |

## [2]-[BUDGET]

[BUDGET]:
- Owner: `Budget` — the meter roster plus the enforcement fold. A budget row is `{ window, reply }`: the model's context window and the completion reserve, so the prompt ceiling is `window - reply` and a row whose reserve swallows its window is the typed breach, not a silent zero. `Budget.gauge` reads a prompt's token count; `Budget.fit` returns the prompt untouched when it fits and `truncate`s it to the ceiling when it does not.
- Law: budgets bind at the Tag — every read yields `Tokenizer.Tokenizer`, so a flow inside a stood provider graph (the `anthropic`/`openai` rows stand through `modelWithTokenizer`) pays zero extra wiring, and a flow outside one (chunk calibration, memory compaction) provides a roster row at the root; the count is meter-relative by construction, and cross-meter counts are never compared.
- Law: the roster is two rows — `anthropic`, the bare-value Layer and the branch default (the pruned standalone tokenizer's replacement), and `openai`, keyed by model id; providers with empty tokenizer cells (`google`, `bedrock`, `openrouter`) route through the default row as the conservative meter, a stated approximation, never a hidden one.
- Law: `Budget.Fault` carries the evidence — `ceiling` and `need` — so a breach routes as data; truncation below the ceiling is not a fault, because `truncate` is the enforcement the budget exists to apply.
- Boundary: which window a given model actually has is app policy passed as the row — model catalogs drift too fast for a literal table to be law; the shape is the law.
- Entry: `Budget.gauge(input)`, `Budget.fit(prompt, row)`.
- Growth: a third meter is one roster row; a new budget axis (a tool-call reserve, a system floor) is one row field consumed inside `fit`.
- Packages: `@effect/ai` (`Tokenizer`, `Prompt`), `@effect/ai-anthropic` (`AnthropicTokenizer`), `@effect/ai-openai` (`OpenAiTokenizer`), `effect` (`Data`, `Effect`).

```typescript
import { type AiError, Prompt, Tokenizer } from "@effect/ai"
import { AnthropicTokenizer } from "@effect/ai-anthropic"
import { OpenAiTokenizer } from "@effect/ai-openai"
import { Data, Effect } from "effect"

const _meters = {
  anthropic: AnthropicTokenizer.layer,
  openai: (model: string) => OpenAiTokenizer.layer({ model }),
} as const

class _BudgetFault extends Data.TaggedError("BudgetFault")<{
  readonly ceiling: number
  readonly need: number
}> {}

declare namespace Budget {
  type Fault = _BudgetFault
  type Meter = keyof typeof _meters
  type Row = { readonly window: number; readonly reply: number }
  type Shape = {
    readonly Fault: typeof _BudgetFault
    readonly meters: typeof _meters
    readonly gauge: (input: Prompt.RawInput) => Effect.Effect<number, AiError.AiError, Tokenizer.Tokenizer>
    readonly fit: (prompt: Prompt.Prompt, row: Row) => Effect.Effect<Prompt.Prompt, Fault | AiError.AiError, Tokenizer.Tokenizer>
  }
}

const Budget: Budget.Shape = {
  Fault: _BudgetFault,
  meters: _meters,
  gauge: (input) =>
    Effect.flatMap(Tokenizer.Tokenizer, (meter) => Effect.map(meter.tokenize(input), (tokens) => tokens.length)),
  fit: (prompt, row) =>
    Effect.gen(function* () {
      const ceiling = row.window - row.reply
      const need = yield* Budget.gauge(prompt)
      yield* Effect.when(Effect.fail(new _BudgetFault({ ceiling, need })), () => ceiling <= 0)
      const meter = yield* Tokenizer.Tokenizer
      return need <= ceiling ? prompt : yield* meter.truncate(prompt, ceiling)
    }),
}
```

## [3]-[CONTEXT]

[CONTEXT]:
- Owner: `Assembly` — the context weave over app-passed retrieval values. `Assembly.Passage` is the folder's one retrieval-value shape: `body`, non-negative `rank`, and `origin` provenance; `Assembly.weave` ranks passages descending, greedily keeps rows while their measured token cost fits the passage allotment, renders the kept rows as one origin-attributed system block, and returns the assembled `Prompt` — system line first, context block appended.
- Law: retrieval is data, never an edge — `store/retrieve` results cross into this fold as already-typed `Passage` values the app passes; `ai` imports no `store` code, and the join between the hybrid retrieval lane and this weave is one `Array.map` at the app seam.
- Law: selection is measured, not guessed — each candidate's cost is a real `tokenize` read (fanned with inherited concurrency), and the greedy fold spends the allotment in rank order, so the weave's output is deterministic in (passages, allotment, meter) and a starved allotment yields the system line alone rather than a truncated mid-passage fragment.
- Law: provenance survives assembly — each kept passage renders as `<origin> body`, so the model's context is attributable line-by-line and a citation surface downstream reads origins straight off the prompt text.
- Law: the weave owns context only — the untrusted user turn enters at `model/provider`'s gate, and the final window fit is `Budget.fit` applied by the caller over the woven prompt plus turn; assembly never re-screens and never re-fits.
- Boundary: passage production (chunking, embedding, retrieval ranking) is `embed/*` and the store's lane; digest-based history compaction that PRODUCES a digest note is `agent/memory`'s — this fold consumes values, never mints them.
- Entry: `Assembly.weave(spec)`.
- Receipt: the assembled `Prompt.Prompt` — the one value every downstream call consumes.
- Growth: a new context source (tool digests, pinned instructions) is one `Passage` row at the caller; a new placement policy is one field on the weave spec.
- Packages: `@effect/ai` (`Prompt`, `Tokenizer`), `effect` (`Array`, `Effect`, `Order`, `Schema`).

```typescript
import { Array, Order, Schema } from "effect"

class _Passage extends Schema.Class<_Passage>("Passage")({
  body: Schema.NonEmptyString,
  rank: Schema.NonNegative,
  origin: Schema.NonEmptyString,
}) {}

const _byRank: Order.Order<_Passage> = Order.mapInput(Order.reverse(Order.number), (passage: _Passage) => passage.rank)

declare namespace Assembly {
  type Passage = _Passage
  type Spec = {
    readonly system: string
    readonly passages: ReadonlyArray<Passage>
    readonly allot: number
  }
  type Shape = {
    readonly Passage: typeof _Passage
    readonly weave: (spec: Spec) => Effect.Effect<Prompt.Prompt, AiError.AiError, Tokenizer.Tokenizer>
  }
}

const Assembly: Assembly.Shape = {
  Passage: _Passage,
  weave: (spec) =>
    Effect.gen(function* () {
      const meter = yield* Tokenizer.Tokenizer
      const ranked = Array.sort(spec.passages, _byRank)
      const costs = yield* Effect.forEach(
        ranked,
        (passage) => Effect.map(meter.tokenize(passage.body), (tokens) => tokens.length),
        { concurrency: "inherit" },
      )
      const kept = Array.reduce(
        Array.zip(ranked, costs),
        { spent: 0, rows: Array.empty<_Passage>() },
        (acc, [passage, cost]) =>
          acc.spent + cost <= spec.allot
            ? { spent: acc.spent + cost, rows: Array.append(acc.rows, passage) }
            : acc,
      )
      const base = Prompt.setSystem(Prompt.fromMessages([]), spec.system)
      return Array.isNonEmptyReadonlyArray(kept.rows)
        ? Prompt.appendSystem(base, Array.join(Array.map(kept.rows, (row) => `<${row.origin}> ${row.body}`), "\n"))
        : base
    }),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Assembly, Budget }
```
