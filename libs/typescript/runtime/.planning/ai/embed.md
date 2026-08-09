# [RUNTIME_EMBED]

The embedding corpus pipeline and the retrieval port's satisfying side: deterministic chunking (one normalization anchor, three cut lanes as policy rows folding into `Piece` receipts), embedding capability rows on the native engine, each taking ONE admitted window policy rather than freezing a posture in its name (`EmbeddingModel.make` with its built-in batch-and-cache, `makeDataLoader` with wall-clock window coalescing, the OpenAI row as the shipped reference, the Google row over the raw `BatchEmbedContents` client, a `custom` row for any remaining raw provider), a two-tier cache whose durable band survives restart through the persisted request-resolver family, and the `Embedder` Layer that satisfies the data wave's retrieval port at app composition — publishing the admitted `Search.Embedding.fingerprint`, batching through the data wave's `Batch.Engine` on both postures, and folding the provider error union into the port's typed fault through one total tag table. The optional `Reranker` port is satisfied here too: a gated structured-output scoring fold over the window the retrieval fusion hands across, its answer re-admitted against the presented cells before it leaves. Determinism is the spine: the NFC scrub is the identity anchor — equal text yields equal pieces, equal pieces yield equal cache hits and equal fingerprint rows in every process and every language. The module is `runtime/src/ai/embed.ts`.

## [01]-[INDEX]

- [02]-[CUT]: the normalization anchor, the cut-lane policy rows, the `Piece` receipt; `Cut`.
- [03]-[ROWS]: embedding capability rows — openai, google, custom — under one window policy, and the cache; `Embedding`.
- [04]-[PORT]: the `Embedder`/`Reranker` satisfying Layers — fingerprint, fault fold, batching; `Embedding`.

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
import { Array, Effect, Exit, HashSet, Layer, Match, Option, PrimaryKey, Schema } from "effect"
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
- Owner: `Embedding.rows` — three capability rows over the native engine, all taking their window geometry as the SAME admitted policy value: `openai` (the curated rows — `Embedding.Custom` selects `layerBatched` for request coalescing plus the hot in-memory tier, or `layerDataLoader` for wall-clock coalescing across unrelated fibers, the bulk-ingest posture), `google` (the Gemini row — no curated `EmbeddingModel` ships, so the row lifts the raw `GoogleClient` `BatchEmbedContents` rail through `EmbeddingModel.make`: one wire call per engine batch, each request `{ model, content: { parts: [{ text }] }, taskType, outputDimensionality }`, vectors read off `embeddings[].values` positionally, and the `taskType`/`outputDimensionality` policy is row data so query-versus-document asymmetry and truncated dims are declaration facts), and `custom` (`EmbeddingModel.make`/`makeDataLoader` over any raw `embedMany` — the row a local ONNX model lands on without a new surface).
- Law: the window posture is a POLICY VALUE, never a row name — one `Embedding.Custom` union states batch width, hot-cache geometry, and wall-clock window, and EVERY row reads it, so a batched and a windowed deployment of one provider are one row taking two values rather than two rows freezing two literals. A provider row differing from its sibling only by a frozen option record is the shape this collapse deletes, and a new posture is one union member every row inherits.
- Law: a raw provider states its `embedMany` and NOTHING about geometry — one `_raw` builder holds both engine arms and every non-curated row composes it, so a row body cannot carry a batch-width literal at all. A hand-frozen width inside a row denies that provider the wall-clock window and the hot tier the policy union grants its siblings, and it reads as a row-shaped fact while being a posture the deployment was never asked about; the curated OpenAI row keeps its own builder because the package ships the pair.
- Law: an absent cache is an ABSENT KEY, not a written `undefined` — the hot-tier option is spread conditionally out of the `Option`, because the engine's option record is exact-optional and writing the key with no value is a different fact from omitting it.
- Law: the cache is two tiers with distinct owners — the hot tier is the engine's own bounded `cache` option; the durable tier is the `_Embedded` persisted request family riding the data wave's engine values (`Batch.windowed` under the wall-clock window, then `Batch.durable` over `Persistence.ResultPersistence`, backed at the root by the data-wave cache lane with its tenant partition), primary-keyed `<fingerprint>:<body>` on the scrubbed body — so a re-embedding of unchanged corpus text after restart is a durable hit, not a provider call.
- Law: the task roster transcribes the provider's published vocabulary WHOLE, carving only the unset sentinel — a sampled subset denies this corpus a posture the engine already answers, and the carve states its reason rather than leaving the gap unnamed.
- Law: every row answers the descriptor floor on `_rowCells` — `fits`, `admit`, `tenancy`, `lifetime`, `degrade` — as the `providers` descriptors a `proc/config#ADMISSION_ROWS` `Profile` selects; `tenancy` names the mechanism a row separates tenants BY, since the closed axis selects the row and the cell explains the separation it performs.
- Law: `degrade` carries what a row GIVES UP against its siblings and nothing else — a coordinate a row never owned is stated as undecided with its owner named, because a forfeit and an absent claim read the same to a caller only when both are wrong.
- Law: provider identity is the admitted `Search.Embedding` value — model, dimensions, revision, and derived fingerprint travel as one fact through every provider row, while `Search.Corpus` composes the distinct relation identity at the data owner; a parallel identity tuple cannot drift, and a revision can never hide behind a constructor-local `"1"` default.
- Growth: a new provider row is one table entry over `custom`; a cache or window policy change is a field on the one policy union; a Google task-type posture is a row field, never a call knob.
- Packages: `@effect/ai` (`EmbeddingModel`, `AiError`); `@effect/ai-openai` (`OpenAiEmbeddingModel`, type `OpenAiClient`); `@effect/ai-google` (`GoogleClient` — the raw `BatchEmbedContents` rail); `@rasm/ts/data` (`Batch.Engine`, `Batch.Persistence`, `Batch.tagged`, `Batch.windowed`, `Batch.durable`); `@effect/experimental` (`Persistence.ResultPersistence` — the durable band's requirement); `effect` (`Exit`, `Layer`, `Option`, `PrimaryKey`, `Schema`).

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
  type Task = (typeof _tasks)[number]
  type Custom = typeof _Custom.Type
  type _Cells<T extends Record<keyof typeof _rows, Embedding.Descriptor> = typeof _rowCells> = T
  type Descriptor = {
    readonly fits: string
    readonly admit: string
    readonly tenancy: string
    readonly lifetime: string
    readonly degrade: string
  }
}

// Providers publish this task vocabulary as a generated literal whose emitted symbol is mangled and cannot be named,
// so the roster transcribes WHOLE rather than sampling it — a five-value subset silently denies this corpus every
// code-retrieval, question-answering, and fact-verification posture the engine already answers. One carve stands:
// `TASK_TYPE_UNSPECIFIED` means "the caller set nothing", so admitting it as a policy value would spell an unmade
// decision as a made one.
const _tasks = [
  "RETRIEVAL_QUERY",
  "RETRIEVAL_DOCUMENT",
  "SEMANTIC_SIMILARITY",
  "CLASSIFICATION",
  "CLUSTERING",
  "QUESTION_ANSWERING",
  "FACT_VERIFICATION",
  "CODE_RETRIEVAL_QUERY",
] as const

// Rows answer the descriptor floor as data a `proc/config#ADMISSION_ROWS` `Profile` SELECTS: `admit` names what a
// caller hands in, `tenancy` the MECHANISM the row separates tenants by, `lifetime` the owner that ends a cached
// vector, and `degrade` the forfeit. Selection reads the closed axis and the cell explains the separation, so a
// `none|single|multi` value here re-mints the roster `core/value/identity#IDENTITY_OWNER` already publishes. Every
// row decides its own vector lifetime because the durable band is this package's own store, and none defers it out.
const _rowCells = {
  openai: {
    fits: "<curated-engine-with-package-owned-batching-and-hot-cache>",
    admit: "<embedding-identity-and-window-policy>",
    tenancy: "<per-credential-one-api-key-is-the-whole-boundary-and-it-resolves-to-one-organization-and-project>",
    lifetime: "<durable-band-ttl-this-package-sets-hot-tier-evicts-on-the-capacity-it-was-given>",
    degrade: "<none-beyond-the-credential-boundary-this-row-is-the-reference>",
  },
  google: {
    fits: "<gemini-vectors-with-a-declared-task-posture-and-truncated-dimensionality>",
    admit: "<embedding-identity-task-posture-and-window-policy>",
    tenancy: "<per-credential-one-api-key-is-the-whole-boundary-and-it-resolves-to-one-cloud-project>",
    lifetime: "<durable-band-ttl-this-package-sets-the-provider-caches-nothing-across-calls>",
    degrade: "<no-curated-engine-ships-so-a-package-side-batching-gain-never-reaches-this-row>",
  },
  custom: {
    fits: "<any-raw-embedMany-including-an-in-process-model-with-no-provider-at-all>",
    admit: "<embedding-identity-window-policy-and-the-caller-s-own-embedMany>",
    tenancy: "<whatever-the-caller-s-own-embedMany-dials-separates-by-and-an-in-process-model-separates-by-nothing>",
    lifetime: "<durable-band-ttl-this-package-sets-and-nothing-else-is-reachable-from-here>",
    degrade: "<the-declared-identity-is-a-caller-assertion-this-row-cannot-verify-against-the-vectors-it-returns>",
  },
} as const satisfies Record<keyof typeof _rows, Embedding.Descriptor>

const _cached = (policy: Extract<Embedding.Custom, { readonly _tag: "Batched" }>) =>
  Option.match(policy.cache, { onNone: () => ({}), onSome: (cache) => ({ cache }) }) // exact-optional: an absent hot tier omits the key, never writes undefined

// The ONE raw-engine builder every non-curated row composes: the window posture is the policy union's on both arms, so
// no raw provider freezes a batch width in its own body and a windowed deployment of any of them is another value.
const _raw = (
  policy: Embedding.Custom,
  embedMany: Parameters<typeof EmbeddingModel.make>[0]["embedMany"],
): Layer.Layer<EmbeddingModel.EmbeddingModel> =>
  policy._tag === "DataLoader"
    ? Layer.scoped(
        EmbeddingModel.EmbeddingModel,
        EmbeddingModel.makeDataLoader({ embedMany, window: policy.engine.window, maxBatchSize: policy.engine.width }),
      )
    : Layer.effect(
        EmbeddingModel.EmbeddingModel,
        EmbeddingModel.make({ embedMany, maxBatchSize: policy.maxBatchSize, ..._cached(policy) }),
      )

const _googleEmbed = (
  google: GoogleClient.GoogleClient,
  embedding: Search.Embedding,
  task: Embedding.Task,
): Parameters<typeof EmbeddingModel.make>[0]["embedMany"] =>
(bodies) =>
  google.client.BatchEmbedContents(embedding.model, {
    requests: Array.map(bodies, (text) => ({
      model: `models/${embedding.model}`,
      content: { parts: [{ text }] },
      taskType: task,
      outputDimensionality: embedding.dims,
    })),
  }).pipe(
    Effect.map((response) => Array.map(response.embeddings ?? [], (row, index) => ({ index, embeddings: [...(row.values ?? [])] }))),
    // the raw client's transport and decode faults join the engine's own AiError rail before make() sees them
    Effect.mapError((cause) => new AiError.UnknownError({ module: "GoogleEmbedding", method: "BatchEmbedContents", cause })),
  )

const _rows = {
  openai: (embedding: Search.Embedding, policy: Embedding.Custom): Embedding.Row<OpenAiClient.OpenAiClient> => ({
    embedding,
    layer: policy._tag === "DataLoader"
      ? OpenAiEmbeddingModel.layerDataLoader({
        model: embedding.model,
        config: { window: policy.engine.window, maxBatchSize: policy.engine.width },
      })
      : OpenAiEmbeddingModel.layerBatched({
        model: embedding.model,
        config: { maxBatchSize: policy.maxBatchSize, ..._cached(policy) },
      }),
  }),
  google: (
    embedding: Search.Embedding,
    task: Embedding.Task,
    policy: Embedding.Custom,
  ): Embedding.Row<GoogleClient.GoogleClient> => ({
    embedding,
    layer: Layer.unwrapEffect(Effect.map(GoogleClient.GoogleClient, (google) => _raw(policy, _googleEmbed(google, embedding, task)))),
  }),
  custom: (
    embedding: Search.Embedding,
    policy: Embedding.Custom,
    embedMany: Parameters<typeof EmbeddingModel.make>[0]["embedMany"],
  ): Embedding.Row => ({ embedding, layer: _raw(policy, embedMany) }),
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
- Law: a refusal blames the side that caused it — `_blamed` maps each guardrail reason onto the port's own vocabulary, so a screened query and a swept answer reach different operators.
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

const _blamed = {
  policy: "shape",
  screened: "shape",
  swept: "provider",
  provider: "provider",
} as const satisfies Record<Guardrail.Reason, EmbedFault["reason"]>

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
        // Blame crosses with the refusal, and each refusal blames a different party: misconfiguration and a screened
        // query are both this caller's own, which the port spells `shape`, while a swept answer and a provider filter
        // belong to whoever operates the model. Collapsing four reasons onto one boolean misroutes a rerank bug.
        Effect.mapError((fault) => new EmbedFault({ reason: _blamed[fault.reason], detail: fault.reason })),
      ),
  })

const Embedding = {
  Custom: _Custom,
  rows: _rows,
  cells: _rowCells,
  tasks: _tasks,
  embedder: _embedder,
  reranker: _reranker,
  fingerprint: _fingerprint,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Cut, Embedding, Piece }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

- [DIMS_TRUNCATION_SUPPORT]-[OPEN]: which embedding rows honour a truncated `outputDimensionality`/dimension request, since the port refuses a vector whose length misses the declared `dims` and a row silently ignoring the request fails every call; verify against `@effect/ai-openai/OpenAiEmbeddingModel` and the Google `BatchEmbedContents` request shape on the member rail.
- [PORT_REFUSAL_REASON]-[OPEN]: whether `EmbedFault` admits a refusal reason distinct from `provider`, so a moderation verdict on the rerank path stops borrowing the transport cell; verify against the `Embedder`/`EmbedFault` owner in `@rasm/ts/data`.
