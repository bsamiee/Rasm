# [AI_MEMORY]

Agent memory is one durable session row and three total operations: `Session` is the `Schema.Class` owner — branded key, exported history, turn count, revision instant, optional digest — persisted through a `KeyValueStore.SchemaStore` so the backing is a runtime row (memory for specs, filesystem on node, Web Storage in the browser) selected at the root, never named here. `Memory.recall` hydrates a `Prompt` from the stored history or returns the empty prompt for a fresh session; `Memory.persist` exports the live prompt back through the Chat serializer and owns the turn count; `Memory.compact` holds history under a token budget by trimming or by digesting old turns into one note through the gate. History serialization rides `Chat`'s own export/restore pair — the one shipped codec for prompt state — so this page owns session policy, not a second history format.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                             |
| :-----: | :-------- | :------------------------------------------------------------------ |
|  [01]   | [SESSION] | the `Session` row, the schema store, recall and persist              |
|  [02]   | [COMPACT] | the budget-driven history fold — trim and digest lanes               |

## [2]-[SESSION]

[SESSION]:
- Owner: `Session` + the `Memory` operations over one `KeyValueStore.layerSchema(Session, "ai/agent/Session")` store — `get` yields `Option` of the decoded row, `set` encodes through the same declaration, decode skew rides `ParseError` and store I/O rides `PlatformError`, the two families the platform store already types.
- Law: history serializes through the shipped pair — `persist` runs `Chat.fromPrompt(prompt)` then `exportJson`; `recall` runs `Chat.fromJson(history)` then reads the service's `history` Ref — so the stored string is the package's own export format and survives package evolution as a decode concern, never a hand-rolled message codec; `Chat.fromJson` carries the `LanguageModel` requirement its restore path declares, which is why `recall`'s `R` names it.
- Law: absence is a fresh session — `recall`'s `onNone` arm returns the empty prompt, so a first turn and a hundredth turn are one code path distinguished by the store's `Option`, and no `SessionNotFound` fault exists; history stores TURNS ONLY — the system line and retrieval context are woven fresh per turn by `model/token`'s assembly, which is what keeps stored history model-portable and retrieval current.
- Law: the session key is branded at the field — `Session["key"]` is the one spelling every consumer types against (the `agent/actor` protocol reuses the field schema itself), and a raw string cannot address a session.
- Law: the turn count is the owner's fact — `persist` reads the prior row and increments inside, so no caller threads a counter and a lost update cannot mis-count under the entity's single-writer law.
- Law: `Chat.Persistence`/`Chat.layerPersisted` is the shipped durable-session Tag over the experimental `Persistence.BackingPersistence` tree — named here as the alternative lane for apps already provisioning that tree; its option surface beyond `id` is a RESEARCH row, so this page's settled lane is the schema store above, and the two never run side by side for one session space.
- Boundary: which `KeyValueStore` backing satisfies the layer is the app root's runtime row; session RETIREMENT rides the store surface's own removal member — a catalog gap recorded as RESEARCH, not fenced.
- Entry: `Memory.recall(key)`; `Memory.persist(key, prompt, digest)`.
- Growth: a new session fact (labels, actor mode, spend accumulation) is one `Session` field; every consumer re-decodes it for free.
- Packages: `@effect/ai` (`Chat`, `Prompt`), `@effect/platform` (`KeyValueStore`, `PlatformError`), `effect` (`DateTime`, `Effect`, `Layer`, `Option`, `Ref`, `Schema`).

```typescript
import { type AiError, Chat, type LanguageModel, Prompt, type Tokenizer, Toolkit } from "@effect/ai"
import { KeyValueStore, type PlatformError } from "@effect/platform"
import { DateTime, Effect, type Layer, Option, type ParseResult, Ref, Schema } from "effect"

class _Session extends Schema.Class<_Session>("Session")({
  key: Schema.NonEmptyString.pipe(Schema.brand("SessionKey")),
  history: Schema.String,
  turns: Schema.Int.pipe(Schema.nonNegative()),
  revised: Schema.DateTimeUtc,
  digest: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
}) {}

const _store = KeyValueStore.layerSchema(_Session, "ai/agent/Session")

declare namespace Memory {
  type Session = _Session
  type Key = _Session["key"]
  type Store = Layer.Layer.Success<typeof _store.layer>
  type Recalled = Effect.Effect<
    Prompt.Prompt,
    ParseResult.ParseError | PlatformError.PlatformError,
    LanguageModel.LanguageModel | Store
  >
  type Persisted = Effect.Effect<
    void,
    AiError.MalformedOutput | ParseResult.ParseError | PlatformError.PlatformError,
    Store
  >
}

const _recall = (key: Memory.Key): Memory.Recalled =>
  Effect.flatMap(_store.tag, (store) =>
    Effect.flatMap(
      store.get(key),
      Option.match({
        onNone: () => Effect.succeed(Prompt.fromMessages([])),
        onSome: (session) => Effect.flatMap(Chat.fromJson(session.history), (chat) => Ref.get(chat.history)),
      }),
    ))

const _persist = (key: Memory.Key, prompt: Prompt.Prompt, digest: Option.Option<string>): Memory.Persisted =>
  Effect.gen(function* () {
    const chat = yield* Chat.fromPrompt(prompt)
    const history = yield* chat.exportJson
    const revised = yield* DateTime.now
    const store = yield* _store.tag
    const prior = yield* store.get(key)
    const turns = Option.match(prior, { onNone: () => 1, onSome: (row) => row.turns + 1 })
    yield* store.set(key, new _Session({ key, history, turns, revised, digest }))
  })
```

## [3]-[COMPACT]

[COMPACT]:
- Owner: `Memory.compact(prompt, fold)` — the history fold applied after every persisted turn: under the budget's ceiling the prompt passes untouched; over it, the `trim` lane truncates through the one `Tokenizer` enforcement (`Budget.fit`), and the `digest` lane folds the whole history into one summary note through the gate — one model call whose instruction is the fold's `charge` — returning the compacted prompt and the digest as evidence for the session row.
- Law: the digest lane is a gate call like any other — it passes the fold's own `Gate.Policy` and `Toolkit.empty`, so moderation applies to compaction output exactly as to user-facing output, and the summarizer can never call tools.
- Law: digest replaces, trim preserves — the digest lane's output prompt is one digest note (a user-role line, because the system slot belongs to the per-turn weave and a stored system would collide with it), byte-cheap and semantically whole; preserving a recent-turn tail through the digest lane needs a message-level prompt projection the catalogue does not yet verify, a RESEARCH row recorded here — `trim` is the settled lane meanwhile, and lane choice is the fold value's tag.
- Law: compaction is idempotent under the ceiling — a compacted prompt re-entering `compact` gauges under the ceiling and passes untouched, so the fold composes safely into every turn loop with no generation-count guard.
- Boundary: the turn loop that calls this after persisting is `agent/actor`'s; the budget row's window/reserve semantics are `model/token`'s.
- Entry: `Memory.compact(prompt, fold)`.
- Receipt: `{ prompt, digest }` — the compacted prompt and the `Option` digest line.
- Growth: a new compaction lane (windowed digests, salience-ranked retention) is one fold arm.
- Packages: `@effect/ai` (`Prompt`, `Tokenizer`, `Toolkit`), `effect` (`Effect`, `Option`).

```typescript
import { Gate } from "../model/provider.ts"
import { Budget } from "../model/token.ts"

declare namespace Memory {
  type Fold =
    | { readonly lane: "trim"; readonly row: Budget.Row }
    | { readonly lane: "digest"; readonly row: Budget.Row; readonly gate: Gate.Policy; readonly charge: string }
  type Compacted = { readonly prompt: Prompt.Prompt; readonly digest: Option.Option<string> }
  type Shape = {
    readonly Session: typeof _Session
    readonly layer: typeof _store.layer
    readonly recall: (key: Key) => Recalled
    readonly persist: (key: Key, prompt: Prompt.Prompt, digest: Option.Option<string>) => Persisted
    readonly compact: (prompt: Prompt.Prompt, fold: Fold) => Effect.Effect<
      Compacted,
      AiError.AiError | Budget.Fault | Gate.Fault,
      LanguageModel.LanguageModel | Tokenizer.Tokenizer
    >
  }
}

const _compact = (prompt: Prompt.Prompt, fold: Memory.Fold): Effect.Effect<
  Memory.Compacted,
  AiError.AiError | Budget.Fault | Gate.Fault,
  LanguageModel.LanguageModel | Tokenizer.Tokenizer
> =>
  Effect.gen(function* () {
    const need = yield* Budget.gauge(prompt)
    const ceiling = fold.row.window - fold.row.reply
    return need <= ceiling
      ? { prompt, digest: Option.none<string>() }
      : fold.lane === "trim"
        ? { prompt: yield* Budget.fit(prompt, fold.row), digest: Option.none<string>() }
        : yield* Effect.map(
            Gate.text(fold.gate)({ prompt, turn: fold.charge, toolkit: Toolkit.empty }),
            (folded) => ({
              prompt: Prompt.make(`<digest> ${folded.text}`),
              digest: Option.some(folded.text),
            }),
          )
  })

const Memory: Memory.Shape = {
  Session: _Session,
  layer: _store.layer,
  recall: _recall,
  persist: _persist,
  compact: _compact,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Memory }
```
