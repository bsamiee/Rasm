# [RUNTIME_EMBED]

The embedding corpus pipeline and the retrieval port's satisfying side: deterministic chunking (one normalization anchor, three cut lanes as policy rows folding into `Piece` receipts), embedding capability rows on the native engine (`EmbeddingModel.make` with its built-in batch-and-cache, `makeDataLoader` with wall-clock window coalescing, the OpenAI rows as the shipped reference, the Google row over the raw `BatchEmbedContents` client, a `custom` row for any remaining raw provider), a two-tier cache whose durable band survives restart through the persisted request-resolver family, and the `Embedder` Layer that satisfies the data wave's retrieval port at app composition — publishing the admitted `Search.Embedding.fingerprint`, batching through the data wave's `Batch.Engine` on both postures, and folding the provider error union into the port's typed fault through one total tag table. The optional `Reranker` port is satisfied here too: a gated structured-output scoring fold over the window the retrieval fusion hands across, its answer re-admitted against the presented cells before it leaves. Determinism is the spine: the NFC scrub is the identity anchor — equal text yields equal pieces, equal pieces yield equal cache hits and equal fingerprint rows in every process and every language. The module is `runtime/src/ai/embed.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER] | [OWNS]                                                                          | [PUBLIC]    |
| :-----: | :-------- | :------------------------------------------------------------------------------ | :---------- |
|  [01]   | `CUT`     | the normalization anchor, the cut-lane policy rows, the `Piece` receipt         | `Cut`       |
|  [02]   | `ROWS`    | embedding capability rows — batched, windowed, google, custom — and the cache   | `Embedding` |
|  [03]   | `PORT`    | the `Embedder`/`Reranker` satisfying Layers — fingerprint, fault fold, batching | `Embedding` |

## [02]-[CUT]

[CUT]:
- Owner: `Cut` — corpus chunking as admitted policy rows: `Cut.Lane` is the union schema, and `Cut.pieces(text, lane)` normalizes ONCE (`String.normalize("NFC")`, line-ending canonicalization, horizontal-space collapse, and bounded blank-line collapse — the determinism anchor every cache key and store fingerprint inherits), then dispatches the lane row through one handler record: `fixed` (span-windowed with overlap columns), `sentence` (`Intl.Segmenter` sentence granularity packed to the span ceiling), `markdown` (heading-bounded sections re-packed under the same ceiling). Every lane answers `Piece` receipts — `seq` (position), `span` (start/length into the normalized text), `body`, and a token estimate — so downstream zips vectors to pieces positionally and provenance survives to the citation layer.
- Law: normalization happens exactly once and only here — a consumer that re-normalizes or re-trims shifts spans and silently forks identity; the piece's `span` indexes the normalized text, and the normalized text is the byte source the store fingerprint hashes.
- Law: the lane is a value on the corpus row, never a caller decision per call — one corpus chunks one way for its lifetime, and a lane change is a re-embedding event by construction (new pieces, new cache keys).
- Law: lane admission proves every `span` positive and every fixed `overlap` non-negative and strictly below its `span`; the fixed stride therefore remains positive and `Array.makeBy` cannot receive an infinite or negative extent.
- Law: the token estimate is a heuristic column for packing, never a budget claim — real metering is the model page's `Tokens` concern.
- Exemption: `_packed` is the measured packing kernel — the single-pass ledger mutation is the sanctioned statement seam, because an immutable rebuild re-copies the packed prefix on every span; the mark rides its first line and the draft detaches at the return.
- Growth: a new cut lane (code-aware, table-aware) is one handler row; an overlap or ceiling change is a lane field.
- Packages: `effect` (`Array`, `Schema`, `Option`); the `Intl.Segmenter` platform seam (universal runtime intrinsic).

```typescript
import { AiError, EmbeddingModel, type LanguageModel } from "@effect/ai"
import { GoogleClient } from "@effect/ai-google"
import { type OpenAiClient, OpenAiEmbeddingModel } from "@effect/ai-openai"
import type { Persistence } from "@effect/experimental"
import { Array, Duration, Effect, Exit, HashSet, Layer, Match, Option, PrimaryKey, Schema } from "effect"
import { Batch, Embedder, EmbedFault, Reranker, Search } from "@rasm/ts/data"
import { Guardrail } from "./model.ts"

const _packed = (spans: ReadonlyArray<{ readonly start: number; readonly body: string }>, ceiling: number) => {
  // BOUNDARY ADAPTER: measured packing kernel — the ledger mutates in place, the packed array detaches immutable at the return
  const packed: Array<{ start: number; body: string }> = []
  for (const span of spans) {
    const last = packed[packed.length - 1]
    if (last !== undefined && last.body.length + span.body.length <= ceiling) {
      packed[packed.length - 1] = { start: last.start, body: last.body + span.body }
    } else {
      packed.push({ start: span.start, body: span.body })
    }
  }
  return packed
}

class Piece extends Schema.Class<Piece>("Piece")({
  seq: Schema.Int,
  span: Schema.Struct({ start: Schema.Int, length: Schema.Int }),
  body: Schema.NonEmptyString,
  estimate: Schema.Int,
}) {}

const _Lane = Schema.Union(
  Schema.Struct({
    kind: Schema.Literal("fixed"),
    span: Schema.Int.pipe(Schema.positive()),
    overlap: Schema.Int.pipe(Schema.nonNegative()),
  }).pipe(Schema.filter((lane) => lane.overlap < lane.span, { identifier: "OverlapWithinSpan" })),
  Schema.Struct({ kind: Schema.Literal("sentence"), span: Schema.Int.pipe(Schema.positive()), locale: Schema.NonEmptyString }),
  Schema.Struct({ kind: Schema.Literal("markdown"), span: Schema.Int.pipe(Schema.positive()) }),
)

declare namespace Cut {
  type Lane = typeof _Lane.Type
}

const _scrubbed = (text: string): string =>
  text.normalize("NFC").replace(/\r\n?/g, "\n").replace(/[^\S\n]+/g, " ").replace(/\n{3,}/g, "\n\n").trim()

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

const Cut = { Lane: _Lane, pieces: _pieces, scrub: _scrubbed }
```

## [03]-[ROWS]

[ROWS]:
- Owner: `Embedding.rows` — the capability rows over the native engine: `batched` (the OpenAI polymorphic `model` under `{ mode: "batched", maxBatchSize, cache: { capacity, timeToLive } }` — request coalescing plus the hot in-memory tier in one shipped option), `windowed` (`{ mode: "data-loader", window, maxBatchSize }` — wall-clock coalescing across unrelated fibers, the bulk-ingest row), `google` (the Gemini row — no curated `EmbeddingModel` ships, so the row lifts the raw `GoogleClient` `BatchEmbedContents` rail through `EmbeddingModel.make`: one wire call per engine batch, each request `{ model, content: { parts: [{ text }] }, taskType, outputDimensionality }`, vectors read off `embeddings[].values` positionally, and the `taskType`/`outputDimensionality` policy is row data so query-versus-document asymmetry and truncated dims are declaration facts), and `custom` (`EmbeddingModel.make`/`makeDataLoader` over any raw `embedMany` — the row a local ONNX model lands on without a new surface).
- Law: the cache is two tiers with distinct owners — the hot tier is the engine's own bounded `cache` option; the durable tier is the `_Embedded` persisted request family riding the data wave's engine values (`Batch.windowed` under the wall-clock window, then `Batch.durable` over `Persistence.ResultPersistence`, backed at the root by the data-wave cache lane with its tenant partition), primary-keyed `<fingerprint>:<body>` on the scrubbed body — so a re-embedding of unchanged corpus text after restart is a durable hit, not a provider call.
- Law: provider identity is the admitted `Search.Embedding` value — model, dimensions, revision, and derived fingerprint travel as one fact through every provider row, while `Search.Corpus` composes the distinct relation identity at the data owner; a parallel identity tuple cannot drift, and a revision can never hide behind a constructor-local `"1"` default.
- Growth: a new provider row is one table entry over `custom`; a cache policy change is an option field; a Google task-type posture is a row field, never a call knob.
- Packages: `@effect/ai` (`EmbeddingModel`, `AiError`); `@effect/ai-openai` (`OpenAiEmbeddingModel`, type `OpenAiClient`); `@effect/ai-google` (`GoogleClient` — the raw `BatchEmbedContents` rail); `@rasm/ts/data` (`Batch.Engine`, `Batch.Persistence`, `Batch.tagged`, `Batch.windowed`, `Batch.durable`); `@effect/experimental` (`Persistence.ResultPersistence` — the durable band's requirement); `effect` (`Duration`, `Exit`, `Layer`, `Option`, `PrimaryKey`, `Schema`).

```typescript
const _Custom = Schema.Union(
  Schema.TaggedStruct("Batched", {
    maxBatchSize: Schema.Int.pipe(Schema.positive()),
    cache: Schema.optionalWith(Schema.Struct({
      capacity: Schema.Int.pipe(Schema.positive()),
      timeToLive: Schema.DurationFromSelf,
    }), { as: "Option" }),
  }),
  Schema.TaggedStruct("DataLoader", { engine: Batch.Engine }),
)

declare namespace Embedding {
  type Row<R = never> = {
    readonly embedding: Search.Embedding
    readonly layer: Layer.Layer<EmbeddingModel.EmbeddingModel, never, R>
  }
  type Durable = {
    readonly engine: Batch.Engine
    readonly persistence: Batch.Persistence
  }
  type Task =
    | "RETRIEVAL_DOCUMENT"
    | "RETRIEVAL_QUERY"
    | "SEMANTIC_SIMILARITY"
    | "CLASSIFICATION"
    | "CLUSTERING"
  type Custom = typeof _Custom.Type
}

const _rows = {
  batched: (embedding: Search.Embedding): Embedding.Row<OpenAiClient.OpenAiClient> => ({
    embedding,
    layer: OpenAiEmbeddingModel.layerBatched({
      model: embedding.model,
      config: { maxBatchSize: 64, cache: { capacity: 4096, timeToLive: Duration.minutes(30) } },
    }),
  }),
  windowed: (embedding: Search.Embedding): Embedding.Row<OpenAiClient.OpenAiClient> => ({
    embedding,
    layer: OpenAiEmbeddingModel.layerDataLoader({
      model: embedding.model,
      config: { window: Duration.millis(80), maxBatchSize: 256 },
    }),
  }),
  google: (
    embedding: Search.Embedding,
    task: Embedding.Task,
  ): Embedding.Row<GoogleClient.GoogleClient> => ({
    embedding,
    layer: Layer.effect(
      EmbeddingModel.EmbeddingModel,
      Effect.flatMap(GoogleClient.GoogleClient, (google) =>
        EmbeddingModel.make({
          maxBatchSize: 64,
          embedMany: (bodies) =>
            google.client.BatchEmbedContents(embedding.model, {
              requests: Array.map(bodies, (text) => ({
                model: `models/${embedding.model}`,
                content: { parts: [{ text }] },
                taskType: task,
                outputDimensionality: embedding.dims,
              })),
            }).pipe(
              Effect.map((response) =>
                Array.map(response.embeddings ?? [], (embedding, index) => ({ index, embeddings: [...(embedding.values ?? [])] })),
              ),
              Effect.mapError((cause) =>
                new AiError.UnknownError({ module: "GoogleEmbedding", method: "BatchEmbedContents", cause }),
              ), // the raw client's transport and decode faults join the engine's own AiError rail before make() sees them
            ),
        })),
    ),
  }),
  custom: (
    embedding: Search.Embedding,
    policy: Embedding.Custom,
    embedMany: Parameters<typeof EmbeddingModel.make>[0]["embedMany"],
  ): Embedding.Row => ({
    embedding,
    layer: policy._tag === "DataLoader"
      ? Layer.scoped(
          EmbeddingModel.EmbeddingModel,
          EmbeddingModel.makeDataLoader({
            embedMany,
            window: policy.engine.window,
            maxBatchSize: policy.engine.width,
          }),
        )
      : Layer.effect(
          EmbeddingModel.EmbeddingModel,
          EmbeddingModel.make({
            embedMany,
            maxBatchSize: policy.maxBatchSize,
            cache: Option.getOrUndefined(policy.cache),
          }),
        ),
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
  engine: Batch.Engine,
  durable: Option.Option<Batch.Persistence>,
) =>
  Effect.flatMap(
    Batch.windowed(
      Batch.tagged<_Embedded>()({
        Embedded: (batch) => embed(Array.map(batch, (request) => request.body)),
      }),
      engine,
    ),
    (windowed) =>
      Option.match(durable, {
        onNone: () => Effect.succeed(windowed),
        onSome: (policy) => Batch.durable(windowed, {
          storeId: policy.storeId,
          timeToLive: (_request, exit) => Exit.isSuccess(exit) ? policy.hit : policy.miss,
        }),
      }),
  )
```

## [04]-[PORT]

[PORT]:
- Owner: the port satisfaction — `Embedding.embedder(row)` builds the Layer that satisfies the data wave's `Embedder` Tag at app composition: `fingerprint` publishes `Search.Embedding.fingerprint` (the brand the vector table's primary key carries, so a model migration is a new fingerprint and old vectors stay queryable under theirs), `embed` scrubs and routes each singular port request through `Effect.request` over the resolver band, and every provider fault folds into the port's own family through the total `_folded` tag table — decode skew to `shape`, transport and unknown failures to `provider`, a `429`/`413` provider rejection to `budget` off the carried response status — so retrieval's lane-exclusion fold reads one vocabulary and no tag falls through an untyped default.
- Law: batching identity is the resolver value on BOTH postures — one `_band` resolver mints inside the Layer scope for the plain and the durable overload alike (`Batch.windowed` alone, or `Batch.windowed` under `Batch.durable`), identity stable, windows grouping across the whole scope; a resolver minted per call, or a plain path that bypasses the window by dialing the provider directly, defeats the coalescing the law exists to guarantee.
- Law: the two Tags are the whole seam — this page imports the port types and nothing else data-owned; retrieval results flow back as app-passed values through the model page's `Tokens.weave`, never through an import edge.
- Growth: a scope-selected second model is a second `embedder(row)` Layer against the same Tag at the root; a cross-encoder reranker is a `Reranker` implementation swap.
- Packages: `@rasm/ts/data` (`Embedder`, `EmbedFault`, `Reranker`, `Search`); `effect` (`Layer`, `Effect`, `Array`, `HashSet`, `Schema`); `./model.ts` (`Guardrail`).

```typescript
const _fingerprint = <R>(row: Embedding.Row<R>): Search.Fingerprint => Search.fingerprint(row.embedding)

const _folded = (fault: AiError.AiError): EmbedFault =>
  Match.value(fault).pipe(
    Match.tag("HttpRequestError", (held) => new EmbedFault({ reason: "provider", detail: held.message })),
    Match.tag("HttpResponseError", (held) =>
      new EmbedFault({
        reason: held.response.status === 429 || held.response.status === 413 ? "budget" : "provider",
        detail: held.message,
      })),
    Match.tags({
      MalformedInput: (held) => new EmbedFault({ reason: "shape", detail: held.message }),
      MalformedOutput: (held) => new EmbedFault({ reason: "shape", detail: held.message }),
    }),
    Match.tag("UnknownError", (held) => new EmbedFault({ reason: "provider", detail: held.message })),
    Match.exhaustive,
  )

function _embedder<R>(row: Embedding.Row<R>): Layer.Layer<Embedder, never, R>
function _embedder<R>(
  row: Embedding.Row<R>,
  durable: Embedding.Durable,
): Layer.Layer<Embedder, never, R | Persistence.ResultPersistence>
function _embedder<R>(row: Embedding.Row<R>, durable?: Embedding.Durable) {
  return Layer.scoped(
    Embedder,
    Effect.gen(function* () {
      const engine = yield* EmbeddingModel.EmbeddingModel
      const print = _fingerprint(row)
      const provider = (bodies: ReadonlyArray<string>) => Effect.mapError(engine.embedMany(bodies), _folded)
      const band = yield* _band(
        provider,
        durable === undefined ? Batch.defaults : durable.engine,
        Option.map(Option.fromNullable(durable), (policy) => policy.persistence),
      )
      return {
        fingerprint: print,
        embed: (body: string) =>
          Effect.request(new _Embedded({ fingerprint: print, body: Cut.scrub(body) }), band).pipe(
            Effect.filterOrFail(
              (vector) => vector.length === row.embedding.dims,
              () => new EmbedFault({ reason: "shape", detail: `dims!=${row.embedding.dims}` }),
            ),
          ),
      }
    }),
  ).pipe(Layer.provide(row.layer))
}

const _Order = Schema.Struct({ order: Schema.NonEmptyArray(Schema.String) })

const _permuted = (presented: ReadonlyArray<string>, answered: ReadonlyArray<string>): ReadonlyArray<string> => {
  const known = HashSet.fromIterable(presented)
  const kept = Array.filter(Array.dedupe(answered), (cell) => HashSet.has(known, cell))
  const named = HashSet.fromIterable(kept)
  return [...kept, ...Array.filter(presented, (cell) => !HashSet.has(named, cell))]
}

const _reranker = (policy: Guardrail.Policy): Layer.Layer<Reranker, never, LanguageModel.LanguageModel> =>
  Layer.succeed(Reranker, {
    rerank: (query, hits) =>
      Guardrail.generate(policy, {
        _tag: "Object",
        options: {
          prompt: `rank cells for: ${query}\n${Array.join(Array.map(hits, (hit) => `${hit.cell}: ${hit.body}`), "\n")}`,
          schema: _Order,
        },
      }).pipe(
        Effect.map((response) => _permuted(Array.map(hits, (hit) => hit.cell), response.value.order)),
        Effect.mapError((fault) => new EmbedFault({ reason: "provider", detail: fault._tag })),
      ),
  })

const Embedding = {
  Custom: _Custom,
  rows: _rows,
  embedder: _embedder,
  reranker: _reranker,
  fingerprint: _fingerprint,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Cut, Embedding, Piece }
```
