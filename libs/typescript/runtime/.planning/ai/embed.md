# [RUNTIME_EMBED]

The embedding corpus pipeline and the retrieval port's satisfying side: deterministic chunking (one normalization anchor, three cut lanes as policy rows folding into `Piece` receipts), embedding capability rows on the native engine (`EmbeddingModel.make` with its built-in batch-and-cache, `makeDataLoader` with wall-clock window coalescing, the OpenAI rows as the shipped reference, a `custom` row for any raw provider), a two-tier cache whose durable band survives restart through the persisted request-resolver family, and the `Embedder` Layer that satisfies the data wave's retrieval port at app composition — publishing the `<model>:<dims>:<revision>` fingerprint the vector rows key on, batching through the data wave's request-batching engine values, and folding the provider error union into the port's typed fault. The optional `Reranker` port is satisfied here too: a gated structured-output scoring fold over the window the retrieval fusion hands across. Determinism is the spine: the NFC scrub is the identity anchor — equal text yields equal pieces, equal pieces yield equal cache hits and equal fingerprint rows in every process and every language. The module is `runtime/src/ai/embed.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER] | [OWNS]                                                                          | [PUBLIC]    |
| :-----: | :-------- | :------------------------------------------------------------------------------ | :---------- |
|  [01]   | `CUT`     | the normalization anchor, the cut-lane policy rows, the `Piece` receipt         | `Cut`       |
|  [02]   | `ROWS`    | embedding capability rows — batched, windowed, custom — and the two-tier cache  | `Embedding` |
|  [03]   | `PORT`    | the `Embedder`/`Reranker` satisfying Layers — fingerprint, fault fold, batching | `Embedding` |

## [02]-[CUT]

[CUT]:
- Owner: `Cut` — corpus chunking as policy rows: `Cut.pieces(text, lane)` normalizes ONCE (`String.normalize("NFC")` plus whitespace collapse — the determinism anchor every cache key and store fingerprint inherits), then dispatches the lane row through one handler record: `fixed` (span-windowed with overlap columns), `sentence` (`Intl.Segmenter` sentence granularity packed to the span ceiling), `markdown` (heading-bounded sections re-packed under the same ceiling). Every lane answers `Piece` receipts — `seq` (position), `span` (start/length into the normalized text), `body`, and a token estimate — so downstream zips vectors to pieces positionally and provenance survives to the citation layer.
- Law: normalization happens exactly once and only here — a consumer that re-normalizes or re-trims shifts spans and silently forks identity; the piece's `span` indexes the normalized text, and the normalized text is the byte source the store fingerprint hashes.
- Law: the lane is a value on the corpus row, never a caller decision per call — one corpus chunks one way for its lifetime, and a lane change is a re-embedding event by construction (new pieces, new cache keys).
- Law: the token estimate is a heuristic column for packing, never a budget claim — real metering is the model page's `Tokens` concern.
- Exemption: `_packed` is the measured packing kernel — the single-pass ledger mutation is the sanctioned statement seam, because an immutable rebuild re-copies the packed prefix on every span.
- Growth: a new cut lane (code-aware, table-aware) is one handler row; an overlap or ceiling change is a lane field.
- Packages: `effect` (`Array`, `Schema`, `Option`); the `Intl.Segmenter` platform seam (universal runtime intrinsic).

```typescript
import { EmbeddingModel, type LanguageModel } from "@effect/ai"
import { OpenAiEmbeddingModel } from "@effect/ai-openai"
import type { Persistence } from "@effect/experimental"
import { Array, Duration, Effect, Layer, PrimaryKey, Schema } from "effect"
import { Batch, Embedder, EmbedFault, Reranker, Search } from "@rasm/ts/data"
import { Guardrail } from "./model.ts"

const _packed = (spans: ReadonlyArray<{ readonly start: number; readonly body: string }>, ceiling: number) =>
  Array.reduce(spans, Array.empty<{ start: number; body: string }>(), (packed, span) => {
    const last = packed[packed.length - 1]
    if (last !== undefined && last.body.length + span.body.length <= ceiling) {
      packed[packed.length - 1] = { start: last.start, body: last.body + span.body }
      return packed
    }
    packed.push({ start: span.start, body: span.body })
    return packed
  })

class Piece extends Schema.Class<Piece>("Piece")({
  seq: Schema.Int,
  span: Schema.Struct({ start: Schema.Int, length: Schema.Int }),
  body: Schema.NonEmptyString,
  estimate: Schema.Int,
}) {}

declare namespace Cut {
  type Lane =
    | { readonly kind: "fixed"; readonly span: number; readonly overlap: number }
    | { readonly kind: "sentence"; readonly span: number; readonly locale: string }
    | { readonly kind: "markdown"; readonly span: number }
}

const _scrubbed = (text: string): string => text.normalize("NFC").replace(/\s+/g, " ").trim()

const _lanes = {
  fixed: (text: string, lane: Extract<Cut.Lane, { kind: "fixed" }>) =>
    Array.makeBy(Math.ceil(text.length / (lane.span - lane.overlap)), (index) => {
      const start = index * (lane.span - lane.overlap)
      return { start, body: text.slice(start, start + lane.span) }
    }),
  sentence: (text: string, lane: Extract<Cut.Lane, { kind: "sentence" }>) =>
    _packed(Array.map(Array.fromIterable(new Intl.Segmenter(lane.locale, { granularity: "sentence" }).segment(text)), (seg) => ({ start: seg.index, body: seg.segment })), lane.span),
  markdown: (text: string, lane: Extract<Cut.Lane, { kind: "markdown" }>) =>
    _packed(
      Array.mapAccum(text.split(/(?=^#{1,6} )/m), 0, (start, body) => [start + body.length, { start, body }] as const)[1],
      lane.span,
    ),
} as const

const _pieces = (raw: string, lane: Cut.Lane): ReadonlyArray<Piece> => {
  const text = _scrubbed(raw)
  const spans = lane.kind === "fixed" ? _lanes.fixed(text, lane) : lane.kind === "sentence" ? _lanes.sentence(text, lane) : _lanes.markdown(text, lane)
  return Array.map(
    Array.filter(spans, (span) => span.body.length > 0),
    (span, seq) =>
      new Piece({
        seq,
        span: { start: span.start, length: span.body.length },
        body: span.body,
        estimate: Math.ceil(span.body.length / 4),
      }),
  )
}

const Cut = { pieces: _pieces, scrub: _scrubbed }
```

## [03]-[ROWS]

[ROWS]:
- Owner: `Embedding.rows` — the capability rows over the native engine: `batched` (the OpenAI polymorphic `model` under `{ mode: "batched", maxBatchSize, cache: { capacity, timeToLive } }` — request coalescing plus the hot in-memory tier in one shipped option), `windowed` (`{ mode: "data-loader", window, maxBatchSize }` — wall-clock coalescing across unrelated fibers, the bulk-ingest row), and `custom` (`EmbeddingModel.make`/`makeDataLoader` over any raw `embedMany` — the row a non-shipped provider or a local ONNX model lands on without a new surface).
- Law: the cache is two tiers with distinct owners — the hot tier is the engine's own bounded `cache` option; the durable tier is the `_Embedded` persisted request family riding the data wave's engine values (`Batch.windowed` under the wall-clock window, then `Batch.durable({ storeId })` over `Persistence.BackingPersistence`, backed at the root by the data-wave `KeyValueStore` scope with `CacheLane.scoped` interposed per tenant), primary-keyed `<fingerprint>:<body>` on the scrubbed body — so a re-embedding of unchanged corpus text after restart is a durable hit, not a provider call.
- Law: dimensions are row facts — a row declares its `dims` and `revision`, and the fingerprint below derives from them; mixing rows in one corpus is unrepresentable because the fingerprint is the vector table's key.
- RESEARCH: the Google raw-embedding row (`EmbedContent`/`BatchEmbedContents` over the raw client) lands as a `custom` row when its request payload members settle from the shipped declaration; the row slot and the `custom` constructor are settled law.
- Growth: a new provider row is one table entry over `custom`; a cache policy change is an option field.
- Packages: `@effect/ai` (`EmbeddingModel`); `@effect/ai-openai` (`OpenAiEmbeddingModel`); `@rasm/ts/data` (`Batch.tagged`, `Batch.windowed`, `Batch.durable`); `@effect/experimental` (`Persistence.BackingPersistence` — the band's requirement); `effect` (`Duration`, `Layer`, `PrimaryKey`, `Schema`).

```typescript
declare namespace Embedding {
  type Row<R = never> = {
    readonly model: string
    readonly dims: number
    readonly revision: string
    readonly layer: Layer.Layer<EmbeddingModel.EmbeddingModel, never, R>
  }
  type Durable = {
    readonly storeId: string
    readonly window: Duration.DurationInput
    readonly maxBatchSize: number
    readonly timeToLive: Duration.DurationInput
  }
}

const _rows = {
  batched: (model: string, dims: number) => ({
    model,
    dims,
    revision: "1",
    layer: OpenAiEmbeddingModel.layerBatched({
      model,
      config: { maxBatchSize: 64, cache: { capacity: 4096, timeToLive: Duration.minutes(30) } },
    }),
  }),
  windowed: (model: string, dims: number) => ({
    model,
    dims,
    revision: "1",
    layer: OpenAiEmbeddingModel.layerDataLoader({
      model,
      config: { window: Duration.millis(80), maxBatchSize: 256 },
    }),
  }),
  custom: (
    row: { readonly model: string; readonly dims: number; readonly revision: string },
    embedMany: Parameters<typeof EmbeddingModel.make>[0]["embedMany"],
  ): Embedding.Row => ({
    ...row,
    layer: Layer.effect(EmbeddingModel.EmbeddingModel, EmbeddingModel.make({ embedMany, maxBatchSize: 64 })),
  }),
} as const

class _Embedded extends Schema.TaggedRequest<_Embedded>()("Embedded", {
  payload: { fingerprint: Schema.String, body: Schema.String },
  success: Schema.Array(Schema.Number),
  failure: EmbedFault,
}) {
  [PrimaryKey.symbol]() {
    return `${this.fingerprint}:${this.body}`
  }
}

const _band = (
  embed: (bodies: ReadonlyArray<string>) => Effect.Effect<ReadonlyArray<ReadonlyArray<number>>, EmbedFault>,
  policy: Embedding.Durable,
) =>
  Effect.flatMap(
    Batch.windowed(
      Batch.tagged<_Embedded>()({
        Embedded: (window) => embed(Array.map(window, (request) => request.body)),
      }),
      { window: policy.window, maxBatchSize: policy.maxBatchSize },
    ),
    (windowed) => Batch.durable(windowed, { storeId: policy.storeId, timeToLive: () => policy.timeToLive }),
  )
```

## [04]-[PORT]

[PORT]:
- Owner: the port satisfaction — `Embedding.embedder(row)` builds the Layer that satisfies the data wave's `Embedder` Tag at app composition: `fingerprint` publishes `<model>:<dims>:<revision>` (the brand the vector table's primary key carries, so a model migration is a new fingerprint and old vectors stay queryable under theirs), `embed` delegates to the `EmbeddingModel` Tag the row's layer provides, and every provider fault folds into the port's own family — a token-ceiling rejection to `budget`, a transport or provider failure to `provider`, a dimension mismatch to `shape` — so retrieval's lane-exclusion fold reads one vocabulary.
- Law: batching identity is the resolver value — the port's provider side coalesces through the data wave's request-batching engine law: one resolver minted at Layer construction, identity stable, windows grouping across the whole scope; a resolver minted per call defeats the window and is the structural defect that law exists to kill. The durable posture is the second `embedder(row, durable)` overload — the `_band` resolver mints once inside the Layer scope and `embed` routes through `Effect.request` under `{ batching: true }`, so restart-surviving hits and window coalescing ride the same identity.
- Law: `Embedding.reranker(policy)` satisfies the optional `Reranker` Tag — one gated structured-output call (`Guardrail.object` with `Toolkit`-free options and a `Schema`-typed score array) over the fusion window's `{ cell, body }` pairs, answering re-ordered cell keys; rerank presence is `Effect.serviceOption` on the retrieval side, so shipping without a reranker is a composition choice, never a knob.
- Law: the two Tags are the whole seam — this page imports the port types and nothing else data-owned; retrieval results flow back as app-passed values through the model page's `Tokens.weave`, never through an import edge.
- Growth: a scope-selected second model is a second `embedder(row)` Layer against the same Tag at the root; a cross-encoder reranker is a `Reranker` implementation swap.
- Packages: `@rasm/ts/data` (`Embedder`, `EmbedFault`, `Reranker`, `Search`); `effect` (`Layer`, `Effect`, `Array`, `Schema`); `./model.ts` (`Guardrail`).

```typescript
const _fingerprint = <R>(row: Embedding.Row<R>): Search.Fingerprint =>
  Search.fingerprint(row.model, row.dims, row.revision)

const _bridge: { readonly [tag: string]: "shape" | "provider" | "budget" } = {
  MalformedInput: "shape",
  MalformedOutput: "shape",
  HttpResponseError: "provider",
}

const _folded = (fault: { readonly _tag: string; readonly description?: string }): EmbedFault =>
  new EmbedFault({
    reason: _bridge[fault._tag] ?? "budget",
    detail: fault.description ?? fault._tag,
  })

function _embedder<R>(row: Embedding.Row<R>): Layer.Layer<Embedder, never, R>
function _embedder<R>(
  row: Embedding.Row<R>,
  durable: Embedding.Durable,
): Layer.Layer<Embedder, never, R | Persistence.BackingPersistence>
function _embedder<R>(row: Embedding.Row<R>, durable?: Embedding.Durable) {
  return Layer.scoped(
    Embedder,
    Effect.gen(function* () {
      const engine = yield* EmbeddingModel.EmbeddingModel
      const print = _fingerprint(row)
      const provider = (bodies: ReadonlyArray<string>) => Effect.mapError(engine.embedMany(bodies), _folded)
      const band = durable === undefined ? undefined : yield* _band(provider, durable)
      return {
        fingerprint: print,
        embed: (texts: Array.NonEmptyReadonlyArray<string>) =>
          (band === undefined
            ? provider(texts)
            : Effect.forEach(texts, (body) => Effect.request(new _Embedded({ fingerprint: print, body }), band), {
                batching: true,
              })
          ).pipe(
            Effect.filterOrFail(
              (vectors): vectors is Array.NonEmptyArray<ReadonlyArray<number>> =>
                Array.isNonEmptyArray(vectors) && Array.every(vectors, (vector) => vector.length === row.dims),
              () => new EmbedFault({ reason: "shape", detail: `dims!=${row.dims}` }),
            ),
          ),
      }
    }),
  ).pipe(Layer.provide(row.layer))
}

const _Scores = Schema.Struct({ order: Schema.NonEmptyArray(Schema.String) })

const _reranker = (policy: Parameters<typeof Guardrail.object>[0]): Layer.Layer<Reranker, never, LanguageModel.LanguageModel> =>
  Layer.succeed(Reranker, {
    rerank: (query, hits) =>
      Guardrail.object(policy)({
        prompt: `rank cells for: ${query}\n${Array.join(Array.map(hits, (hit) => `${hit.cell}: ${hit.body}`), "\n")}`,
        schema: _Scores,
      }).pipe(
        Effect.map((response) => response.value.order),
        Effect.mapError((fault) => new EmbedFault({ reason: "provider", detail: fault._tag })),
      ),
  })

const Embedding = {
  rows: _rows,
  embedder: _embedder,
  reranker: _reranker,
  fingerprint: _fingerprint,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Cut, Embedding, Piece }
```
