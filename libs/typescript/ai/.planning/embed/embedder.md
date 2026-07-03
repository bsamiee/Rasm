# [AI_EMBEDDER]

Embedding is one Tag and a policy value: every consumer reads `EmbeddingModel.EmbeddingModel`, and which row satisfies it — the OpenAI batched row with its bounded input cache, the OpenAI window row that coalesces calls across fibers, or a custom row over the core constructor — is `Embedder.live(policy)` dispatch on a tagged policy, selected at the app root. Batching, caching, and window coalescing are package capability (`maxBatchSize`, `cache`, `makeDataLoader`) surfaced as policy fields, never re-implemented; the piece pipeline zips `Chunker.Piece` rows with their vectors positionally in one call; and the `store/retrieve` `Embedder` port is satisfied at app composition by projecting this Layer — `ai` produces the capability, the store declares the port, the root joins them, and no import edge exists in either direction.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]  | [OWNS]                                                              |
| :-----: | :--------- | :------------------------------------------------------------------- |
|  [01]   | [ROWS]     | the policy union and the Layer rows that satisfy the one Tag          |
|  [02]   | [PIPELINE] | the piece-to-vector zip and the port-satisfaction law                 |

## [2]-[ROWS]

[ROWS]:
- Owner: `Embedder.live(policy)` — total dispatch over the tagged policy: `Batched` selects `OpenAiEmbeddingModel.layerBatched` (call coalescing up to `batch`, an optional bounded `{ capacity, timeToLive }` input cache), `Windowed` selects `layerDataLoader` (a `Duration` window batching across unrelated fibers, scope owned inside the Layer). Both rows require `OpenAiClient` — the root satisfies it with `Provider.openai.client`, the same hoisted transport const the provider row stands on, so language-model and embedding traffic share one client construction.
- Law: OpenAI is the only curated embedding provider — the asymmetry table's one populated `embed: "curated"` cell; the Google cell is `raw` (embeddings exist only through `GoogleClient.Service.client` operations `EmbedContent`/`BatchEmbedContents` against the `EmbedContentRequest` anchor), and wiring that lane through `Embedder.custom` is a RESEARCH row until the request payload members verify — the gap is recorded, never bridged with invented fields.
- Law: `Embedder.custom(options)` is the own-row constructor — the core `EmbeddingModel.make` when no `window` is given (batching plus optional cache), `makeDataLoader` when one is (the constructor's `Scope` is absorbed by `Layer.scoped`), so a self-hosted or sixth-provider embedder is one options value satisfying the same Tag, and the modality discriminates on the options shape, never a second entrypoint.
- Law: the input cache keys on the input string — `embed/chunk`'s normalization is what makes those keys deterministic, so re-embedding an unchanged corpus is a cache read; cache capacity and TTL are policy fields with no defaults asserted here, because staleness tolerance is an app fact.
- Boundary: dimension and encoding knobs ride the package's own per-request `Config` surface; vector persistence, index selection, and fingerprint keys are the store's vector-row law.
- Entry: `Embedder.live(policy)`; `Embedder.custom(options)`.
- Growth: a new provider embedding row is one policy arm plus its dispatch line; a new batching modality is one arm.
- Packages: `@effect/ai` (`EmbeddingModel`), `@effect/ai-openai` (`OpenAiEmbeddingModel`), `effect` (`Data`, `Duration`, `Effect`, `Layer`).

```typescript
import { type AiError, EmbeddingModel } from "@effect/ai"
import { type OpenAiClient, OpenAiEmbeddingModel } from "@effect/ai-openai"
import { Array, Data, type Duration, Effect, Layer, type Types } from "effect"

type _Policy = Data.TaggedEnum<{
  Batched: {
    readonly model: string
    readonly batch?: number
    readonly cache?: { readonly capacity: number; readonly timeToLive: Duration.DurationInput }
  }
  Windowed: {
    readonly model: string
    readonly window: Duration.DurationInput
    readonly batch?: number
  }
}>
const _Policy: Data.TaggedEnum.Constructor<_Policy> = Data.taggedEnum<_Policy>()

declare namespace Embedder {
  type Policy = _Policy
  type Custom = {
    readonly embedMany: (input: ReadonlyArray<string>) => Effect.Effect<Array<EmbeddingModel.Result>, AiError.AiError>
    readonly batch?: number
    readonly cache?: { readonly capacity: number; readonly timeToLive: Duration.DurationInput }
    readonly window?: Duration.DurationInput
  }
  type Live = Layer.Layer<EmbeddingModel.EmbeddingModel, never, OpenAiClient.OpenAiClient>
  type Own = Layer.Layer<EmbeddingModel.EmbeddingModel>
}

const _live = (policy: _Policy): Embedder.Live =>
  _Policy.$match(policy, {
    Batched: ({ batch, cache, model }) =>
      OpenAiEmbeddingModel.layerBatched({
        model,
        config: {
          ...(batch !== undefined && { maxBatchSize: batch }),
          ...(cache !== undefined && { cache }),
        },
      }),
    Windowed: ({ batch, model, window }) =>
      OpenAiEmbeddingModel.layerDataLoader({
        model,
        config: { window, ...(batch !== undefined && { maxBatchSize: batch }) },
      }),
  })

const _custom = (options: Embedder.Custom): Embedder.Own =>
  options.window === undefined
    ? Layer.effect(
        EmbeddingModel.EmbeddingModel,
        EmbeddingModel.make({
          embedMany: options.embedMany,
          ...(options.batch !== undefined && { maxBatchSize: options.batch }),
          ...(options.cache !== undefined && { cache: options.cache }),
        }),
      )
    : Layer.scoped(
        EmbeddingModel.EmbeddingModel,
        EmbeddingModel.makeDataLoader({
          embedMany: options.embedMany,
          window: options.window,
          ...(options.batch !== undefined && { maxBatchSize: options.batch }),
        }),
      )
```

## [3]-[PIPELINE]

[PIPELINE]:
- Owner: `Embedder.vectors(pieces, options?)` — the one piece pipeline: it reads the Tag, embeds every `Piece.body` in one `embedMany` call under the declared concurrency, and zips vectors back onto their pieces positionally, so the receipt is `{ piece, values }` rows whose provenance (seq, span, estimate) survives embedding intact.
- Law: positional integrity is the package's contract — `embedMany` answers index-for-index, the zip truncates nothing because both operands are the same length by construction, and the built-in batching means one pipeline call may fan into several wire requests without this fold caring.
- Law: single-string reads ride the Tag directly — a consumer with one string yields `EmbeddingModel.EmbeddingModel` and calls `embed`; a wrapper renaming that member is the deleted hop.
- Law: port satisfaction is composition, not import — the app root projects this page's Layer onto the `store/Embedder` port with `Layer.project`: the port's `embed` delegates to the Tag's `embedMany` (non-empty in, non-empty out), `AiError` folds into the port's own `EmbedFault` reasons at the projection, and the port's `fingerprint` field is the app's embedding-space identity minted from the store's index vocabulary — `ai` never names a store symbol, and retrieval-side concerns (fingerprints, index rows, RRF fusion) never appear here.
- Entry: `Embedder.vectors(pieces, options?)`.
- Receipt: `ReadonlyArray<Embedder.Vector>` — piece plus its vector, ready for the store's vector-row intake at the app seam.
- Growth: a reranking or dimension-projection step is one fold over the receipt rows at the caller, never a second pipeline.
- Packages: `@effect/ai` (`EmbeddingModel`), `effect` (`Array`, `Effect`, `Types`).

```typescript
import { Piece } from "./chunk.ts"

declare namespace Embedder {
  type Vector = { readonly piece: Piece; readonly values: ReadonlyArray<number> }
  type Shape = {
    readonly Policy: typeof _Policy
    readonly custom: (options: Custom) => Own
    readonly live: (policy: Policy) => Live
    readonly vectors: (
      pieces: ReadonlyArray<Piece>,
      options?: { readonly concurrency?: Types.Concurrency },
    ) => Effect.Effect<ReadonlyArray<Vector>, AiError.AiError, EmbeddingModel.EmbeddingModel>
  }
}

const Embedder: Embedder.Shape = {
  Policy: _Policy,
  custom: _custom,
  live: _live,
  vectors: (pieces, options) =>
    Effect.flatMap(EmbeddingModel.EmbeddingModel, (model) =>
      Effect.map(
        model.embedMany(Array.map(pieces, (piece) => piece.body), options),
        (rows) => Array.zipWith(pieces, rows, (piece, values) => ({ piece, values })),
      )),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Embedder }
```
