# [SERVICES_FUSED_RANK]

The fused hybrid-retrieval owner — `HybridSearch`, the single Reciprocal-Rank-Fusion surface fusing semantic (vector/HNSW), lexical (BM25), trigram, and phonetic retrieval over the one `SqlBoundary` Postgres client, then re-scoring the fused candidate set through a terminal `rerank` stage. It is a first-class search domain distinct from raw persistence: the four retrieval signals are per-signal ranked arms on one RRF owner, never four parallel search services and never folded into persistence. RRF is scale-invariant across the heterogeneous signal scores, so each arm contributes a rank rather than a raw score and the fragile per-signal normalization the weighted-sum needed is dropped. The owner rides the persistence `PgClient` and crosses no .NET wire.

## [1]-[INDEX]

One cluster: `[2]-[FUSED_RANK]` owns the RRF-over-BM25 semantic+lexical+trigram+phonetic fused-rank surface and the post-fusion `rerank` stage axis.

RESEARCH: the BM25 lexical arm rests on a Postgres BM25 extension (`pg_search`/ParadeDB `@@@` + `paradedb.score` or Tiger `pg_textsearch`) that the `pgvector`/`pg_trgm`/`fuzzystrmatch` set does not carry; the arm is authored against the `pg_search` `@@@`/`paradedb.score` surface, the extension admission and the exact score-function spelling are the open probe gated on `provisioning/contract#PROVISIONING` provisioning the BM25-bearing Postgres image.

RESEARCH: the `RerankModel.CrossEncoder` arm's `endpoint` is a live off-process cross-encoder model-host the design cannot settle branch-side; the Effect body is authored against the host surface, the host URL is the one open probe.

## [2]-[FUSED_RANK]

- Owner: `HybridSearch`, the single RRF owner over four retrieval signals — `semantic` (vector cosine distance over an embedding column with HNSW `efSearch` tuning), `lexical` (BM25 over a `pg_search` index), `trigram` (`pg_trgm` similarity), and `phonetic` (a phonetic-key match) — each one ranked arm on the one owner, never a parallel search service; the fused candidate set then passes through one ordered two-stage pipeline whose terminal `rerank` stage is a `RerankModel` row, never a second retriever.
- Cases: one SQL round-trip fans the four signals out through the `hybridQuery` per-signal `RANK() OVER (ORDER BY ...)` scaffold (`pgvector` `<=>` cosine, BM25 `paradedb.score` over the `@@@` match, `pg_trgm` `similarity`, `fuzzystrmatch` `dmetaphone`), every id carrying all four per-signal rank columns so a candidate strong in one modality fuses cleanly; the fused rank is the Reciprocal Rank Fusion `SUM(weight / (rrfK + rank))` over a `SearchWeights` row, scale-invariant so a per-signal score normalization is unnecessary; the trigram and phonetic signals catch the typo and homophone matches the BM25 lexical signal misses. The `rerank` stage then re-scores the bounded candidate set the SQL narrowed — a `RerankModel` row over a `RerankFeatureVector` (the four signal contributions plus rank-margin and staleness), collect-then-`Order`-reorder so the sort is GLOBAL across the candidate set, never chunk-local, promoting a candidate strong on a cross-feature interaction the linear RRF sum cannot express; the result is one re-ordered `SearchHit` stream.
- Entry: the search rides the one `persistence/store-boundary#STORE_BOUNDARY` `PgClient` surface — the vector, BM25, trigram, and phonetic columns are columns on the searched entity's `Model.Class`, and the fused query is one set-algebraic SQL the boundary issues with the embedding profile, the weights, and the `rrfK` constant bound as parameters; the embedding profile dimensions the vector column and a re-embed job (a `persistence/work-and-signals#WORK_AND_SIGNALS` `WorkQueue` row) refreshes a stale embedding; the search is tenant-scoped through the same `app.current_tenant` GUC every query reads (`persistence/tenancy#TENANCY`); the `rerank` stage is a pure post-fusion projection over the bounded fused top-k candidate set the SQL already narrowed, so it adds no second round-trip and no second index — it re-orders the candidates the one query returned.
- Wire: the owner crosses no .NET wire — the C# `federation#FUSION_RANK` is a different retrieval algebra (a three-branch `vector`/`spatial`/`lexical` reciprocal-rank fusion over `federated_entity`, with a `spatial` branch this owner lacks and without this owner's `trigram`/`phonetic` branches), so the two cannot share a fused-candidate wire; this branch owns its fusion in `hybridQuery` and decodes no server RRF. The post-fusion `rerank` projection re-orders this branch's OWN fused candidates, not a re-fusion of the server's.
- Packages: `@effect/sql` and `@effect/sql-pg` for the set-algebraic query over the vector/BM25/trigram/phonetic columns through the one `PgClient` (the `pgvector`, `pg_search` (BM25), `pg_trgm`, and `fuzzystrmatch` Postgres extensions provisioned by `provisioning/contract#PROVISIONING`), and `effect` for the `Stream` result, the RRF fold, the `RerankModel` `Data.TaggedEnum` dispatch, and the `Order`-driven reorder fold.
- Growth: a new retrieval signal lands as one ranked arm on `HybridSearch` with its weight, never a parallel search service; a new embedding profile lands as one profile row; a new HNSW tuning lands as one `efSearch` value; a new fusion strategy lands as one rank arm; a new reranking strategy lands as one `RerankModel` case on the one tagged axis (one `Match.tagsExhaustive` arm), never a parallel reranker service or a second pipeline.
- Boundary: the named defects — four parallel search services instead of one fused owner; a second SQL surface beside the one `PgClient` (acquired from the `SqlClient` context tag in the `HybridSearch.Default` Layer); four sequential branch-side queries instead of the one round-trip; a free-floating `set_config` CTE Postgres prunes instead of the non-prunable cross join that pins `efSearch` to the executing connection; a branch-side score normalization instead of the scale-invariant RRF rank fold; a recomputed branch-side staleness heuristic instead of the column flag; a parallel reranker beside the one tagged axis; an unchecked embedding width poisoning the `::vector` cast instead of the pre-query `feature`-stage arity assert; a weighted-sum over `ts_rank_cd` instead of RRF over BM25. This is a node-only surface, never browser-reachable.

```ts owner
import type { PgClient } from "@effect/sql-pg"
import type { SqlError } from "@effect/sql"
import { SqlClient } from "@effect/sql"
import { Array as A, Data, Effect, Layer, Match, Order, Record as R, Schema as S, Stream } from "effect"

const SEARCH_SIGNALS = ["semantic", "lexical", "trigram", "phonetic"] as const
type SearchSignal = (typeof SEARCH_SIGNALS)[number]

type RerankModel = Data.TaggedEnum<{
  readonly Identity:     {}
  readonly Linear:       { readonly bias: number; readonly weights: RerankFeatureVector }
  readonly CrossEncoder: { readonly endpoint: string; readonly model: string; readonly batch: number }
}>
const RerankModel = Data.taggedEnum<RerankModel>()

interface SearchWeights { readonly semantic: number; readonly lexical: number; readonly trigram: number; readonly phonetic: number }
interface EmbeddingProfile { readonly model: string; readonly dimensions: number; readonly efSearch: number }
type RerankFeatureVector = Record<SearchSignal, number> & { readonly margin: number; readonly staleness: number }

interface SearchHit {
  readonly id: string
  readonly fusedScore: number
  readonly rerankedScore: number
  readonly signals: Record<SearchSignal, number>
  readonly stale: boolean
}

class RerankFault extends S.TaggedError<RerankFault>()("RerankFault", {
  stage: S.Literal("feature", "score", "host"),
  model: S.String,
  cause: S.Unknown,
}) {}

type QueryOptions = {
  readonly tenant: string
  readonly text: string
  readonly embedding: ReadonlyArray<number>
  readonly weights: SearchWeights
  readonly profile: EmbeddingProfile
  readonly rerank: RerankModel
  readonly rrfK: number
  readonly candidates: number
  readonly limit: number
}

class HybridSearch extends Effect.Service<HybridSearch>()("services/HybridSearch", {
  accessors: true,
  effect: Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const query = (o: QueryOptions): Stream.Stream<SearchHit, SqlError.SqlError | RerankFault, never> =>
      Stream.fromIterableEffect(
        assertArity(o.embedding, o.profile).pipe(Effect.flatMap(() => hybridQuery(sql, o))),
      ).pipe(rerankStage(o.text, o.rerank))
    return { query } as const
  }),
}) {}

const assertArity = (embedding: ReadonlyArray<number>, profile: EmbeddingProfile): Effect.Effect<void, RerankFault> =>
  embedding.length === profile.dimensions
    ? Effect.void
    : Effect.fail(new RerankFault({ stage: "feature", model: profile.model, cause: `embedding arity ${embedding.length} != profile dimensions ${profile.dimensions}` }))

const featuresOf = (hit: SearchHit): RerankFeatureVector => ({
  ...R.fromEntries(A.map(SEARCH_SIGNALS, (signal) => [signal, hit.signals[signal] ?? 0] as const)),
  margin:    hit.fusedScore,
  staleness: hit.stale ? 1 : 0,
})

const linearScore = (bias: number, w: RerankFeatureVector) => (f: RerankFeatureVector): number =>
  bias +
  f.semantic * w.semantic + f.lexical * w.lexical + f.trigram * w.trigram + f.phonetic * w.phonetic +
  f.margin * w.margin - f.staleness * w.staleness

const crossEncoderScore = (text: string, endpoint: string, model: string, batch: number) =>
  (chunk: A.NonEmptyReadonlyArray<SearchHit>): Effect.Effect<ReadonlyArray<number>, RerankFault> =>
    Effect.tryPromise({
      try: (signal) =>
        fetch(endpoint, { method: "POST", signal, headers: { "content-type": "application/json" },
          body: JSON.stringify({ model, query: text, documents: A.map(chunk, (h) => h.id), batch }) })
          .then((r) => r.json() as Promise<{ readonly scores: ReadonlyArray<number> }>)
          .then((j) => j.scores),
      catch: (cause) => new RerankFault({ stage: "host", model, cause }),
    })

const rerankStage = (text: string, model: RerankModel) =>
  (fused: Stream.Stream<SearchHit, SqlError.SqlError, never>): Stream.Stream<SearchHit, SqlError.SqlError | RerankFault, never> =>
    Match.value(model).pipe(Match.tagsExhaustive({
      Identity: () => fused,
      Linear: ({ bias, weights }) =>
        Stream.unwrap(Stream.runCollect(fused).pipe(Effect.map((chunk) => {
          const score = linearScore(bias, weights)
          return Stream.fromIterable(A.sortBy(byReranked)(A.map(A.fromIterable(chunk), (hit) => ({ ...hit, rerankedScore: score(featuresOf(hit)) }))))
        }))),
      CrossEncoder: ({ endpoint, model: name, batch }) =>
        Stream.unwrap(Stream.runCollect(fused).pipe(Effect.flatMap((chunk) =>
          A.match(A.fromIterable(chunk), {
            onEmpty:    () => Effect.succeed(Stream.empty),
            onNonEmpty: (hits) =>
              crossEncoderScore(text, endpoint, name, batch)(hits).pipe(Effect.map((scores) =>
                Stream.fromIterable(A.sortBy(byReranked)(A.zipWith(hits, scores, (hit, rerankedScore) => ({ ...hit, rerankedScore })))))),
          })))),
    }))

const byReranked: Order.Order<SearchHit> = Order.reverse(Order.mapInput(Order.number, (hit: SearchHit) => hit.rerankedScore))

const hybridQuery = (sql: SqlClient.SqlClient, o: QueryOptions) => sql<SearchHit>`
  WITH cfg AS (SELECT set_config('hnsw.ef_search', ${o.profile.efSearch}::text, true)),
  semantic AS (
    SELECT s.id, RANK() OVER (ORDER BY s.embedding <=> ${o.embedding}::vector) AS rnk,
           s.embedding_model <> ${o.profile.model} AS stale
    FROM searchable s, cfg
    ORDER BY s.embedding <=> ${o.embedding}::vector LIMIT ${o.candidates}
  ),
  lexical AS (
    SELECT s.id, RANK() OVER (ORDER BY paradedb.score(s.id) DESC) AS rnk
    FROM searchable s WHERE s.bm25 @@@ ${o.text}
    ORDER BY paradedb.score(s.id) DESC LIMIT ${o.candidates}
  ),
  trigram AS (
    SELECT s.id, RANK() OVER (ORDER BY similarity(s.label, ${o.text}) DESC) AS rnk
    FROM searchable s WHERE s.label %% ${o.text}
    ORDER BY similarity(s.label, ${o.text}) DESC LIMIT ${o.candidates}
  ),
  phonetic AS (
    SELECT s.id, RANK() OVER (ORDER BY s.id) AS rnk
    FROM searchable s WHERE dmetaphone(s.label) = dmetaphone(${o.text}) LIMIT ${o.candidates}
  ),
  ids AS (
    SELECT id FROM semantic UNION SELECT id FROM lexical UNION SELECT id FROM trigram UNION SELECT id FROM phonetic
  )
  SELECT i.id,
         ${o.weights.semantic}::float8 / (${o.rrfK}::float8 + COALESCE(se.rnk, ${o.candidates} + 1))
       + ${o.weights.lexical}::float8  / (${o.rrfK}::float8 + COALESCE(lx.rnk, ${o.candidates} + 1))
       + ${o.weights.trigram}::float8  / (${o.rrfK}::float8 + COALESCE(tg.rnk, ${o.candidates} + 1))
       + ${o.weights.phonetic}::float8 / (${o.rrfK}::float8 + COALESCE(ph.rnk, ${o.candidates} + 1)) AS "fusedScore",
         ${o.weights.semantic}::float8 / (${o.rrfK}::float8 + COALESCE(se.rnk, ${o.candidates} + 1))
       + ${o.weights.lexical}::float8  / (${o.rrfK}::float8 + COALESCE(lx.rnk, ${o.candidates} + 1))
       + ${o.weights.trigram}::float8  / (${o.rrfK}::float8 + COALESCE(tg.rnk, ${o.candidates} + 1))
       + ${o.weights.phonetic}::float8 / (${o.rrfK}::float8 + COALESCE(ph.rnk, ${o.candidates} + 1)) AS "rerankedScore",
         jsonb_build_object(
           'semantic', ${o.weights.semantic}::float8 / (${o.rrfK}::float8 + COALESCE(se.rnk, ${o.candidates} + 1)),
           'lexical',  ${o.weights.lexical}::float8  / (${o.rrfK}::float8 + COALESCE(lx.rnk, ${o.candidates} + 1)),
           'trigram',  ${o.weights.trigram}::float8  / (${o.rrfK}::float8 + COALESCE(tg.rnk, ${o.candidates} + 1)),
           'phonetic', ${o.weights.phonetic}::float8 / (${o.rrfK}::float8 + COALESCE(ph.rnk, ${o.candidates} + 1))
         ) AS signals,
         COALESCE(se.stale, false) AS stale
  FROM ids i
  LEFT JOIN semantic se ON se.id = i.id
  LEFT JOIN lexical  lx ON lx.id = i.id
  LEFT JOIN trigram  tg ON tg.id = i.id
  LEFT JOIN phonetic ph ON ph.id = i.id
  ORDER BY "fusedScore" DESC
  LIMIT ${o.limit}
`

const HybridSearchLayer: Layer.Layer<HybridSearch, never, SqlClient.SqlClient | PgClient.PgClient> = HybridSearch.Default
```
