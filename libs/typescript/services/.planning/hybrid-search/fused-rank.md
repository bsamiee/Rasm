# [SERVICES_FUSED_RANK]

The fused hybrid-retrieval owner — `HybridSearch`, the single weighted-rank surface fusing semantic (vector/HNSW), lexical (tsvector/regconfig), trigram, and phonetic retrieval over the one `SqlBoundary` Postgres client, then re-scoring the fused candidate set through a terminal `rerank` stage. It is a first-class search domain distinct from raw persistence: the four retrieval signals are rows on one weighted-rank owner, never four parallel search services and never folded into persistence. The owner rides the persistence `PgClient` and crosses no .NET wire.

## [1]-[INDEX]

One cluster: `[2]-[FUSED_RANK]` owns the fused semantic+lexical+trigram+phonetic weighted-rank surface and the post-fusion `rerank` stage axis.

RESEARCH: the `RerankModel.CrossEncoder` arm's `endpoint` is a live off-process cross-encoder model-host the design cannot settle branch-side; the Effect body is authored against the host surface, the host URL is the one open probe.

## [2]-[FUSED_RANK]

- Owner: `HybridSearch`, the single fused weighted-rank owner over four retrieval signals — `semantic` (vector cosine distance over an embedding column with HNSW `efSearch` tuning), `lexical` (`tsvector`/`regconfig` full-text rank), `trigram` (`pg_trgm` similarity), and `phonetic` (a phonetic-key match) — each one signal row on the one owner with its weight, never a parallel search service; the fused candidate set then passes through one ordered two-stage pipeline whose terminal `rerank` stage is a `RerankModel` row, never a second retriever.
- Cases: one SQL round-trip fans the four signals out through the `hybridQuery` `LATERAL` per-id scaffold (`pgvector` `<=>` cosine, `ts_rank_cd` over `websearch_to_tsquery`, `pg_trgm` `similarity`, `fuzzystrmatch` `dmetaphone`), every id carrying all four signal rows so a candidate strong in one modality fuses cleanly; the fused rank is the weighted sum over a `SearchWeights` row; the trigram and phonetic signals catch the typo and homophone matches the lexical signal misses. The `rerank` stage then re-scores the bounded candidate set the SQL narrowed — a `RerankModel` row over a `RerankFeatureVector` (the four scores plus rank-margin and staleness), collect-then-`Order`-reorder so the sort is GLOBAL across the candidate set, never chunk-local, promoting a candidate strong on a cross-feature interaction the linear sum cannot express; the result is one re-ordered `SearchHit` stream.
- Entry: the search rides the one `persistence/store-boundary#STORE_BOUNDARY` `PgClient` surface — the vector, full-text, trigram, and phonetic columns are columns on the searched entity's `Model.Class`, and the fused query is one set-algebraic SQL the boundary issues with the embedding profile and the weights bound as parameters; the embedding profile dimensions the vector column and a re-embed job (a `persistence/work-and-signals#WORK_AND_SIGNALS` `WorkQueue` row) refreshes a stale embedding; the search is tenant-scoped through the same `app.current_tenant` GUC every query reads (`persistence/tenancy#TENANCY`); the `rerank` stage is a pure post-fusion projection over the bounded fused top-k candidate set the SQL already narrowed, so it adds no second round-trip and no second index — it re-orders the candidates the one query returned.
- Wire: the owner crosses no .NET wire — the C# `federation#FUSION_RANK` is a different retrieval algebra (a three-branch `vector`/`spatial`/`lexical` reciprocal-rank fusion over `federated_entity`, with a `spatial` branch this owner lacks and without this owner's `trigram`/`phonetic` branches), so the two cannot share a fused-candidate wire; this branch owns its fusion in `hybridQuery` and decodes no server RRF. The post-fusion `rerank` projection re-orders this branch's OWN fused candidates, not a re-fusion of the server's.
- Packages: `@effect/sql` and `@effect/sql-pg` for the set-algebraic query over the vector/tsvector/trigram/phonetic columns through the one `PgClient` (the `pgvector`, `pg_trgm`, and `fuzzystrmatch` Postgres extensions provisioned by `provisioning/contract#PROVISIONING`), and `effect` for the `Stream` result, the weighted-rank fold, the `RerankModel` `Data.TaggedEnum` dispatch, and the `Order`-driven reorder fold.
- Growth: a new retrieval signal lands as one signal row on `HybridSearch` with its weight, never a parallel search service; a new embedding profile lands as one profile row; a new HNSW tuning lands as one `efSearch` value; a new fusion strategy lands as one rank arm; a new reranking strategy lands as one `RerankModel` case on the one tagged axis (one `Match.tagsExhaustive` arm), never a parallel reranker service or a second pipeline.
- Boundary: the named defects — four parallel search services instead of one fused owner; a second SQL surface beside the one `PgClient` (acquired from the `SqlClient` context tag in the `HybridSearch.Default` Layer); four sequential branch-side queries instead of the one round-trip; a free-floating `set_config` CTE Postgres prunes instead of the non-prunable cross join that pins `efSearch` to the executing connection; a recomputed branch-side staleness heuristic instead of the column flag; a parallel reranker beside the one tagged axis; an unchecked embedding width poisoning the `::vector` cast instead of the pre-query `feature`-stage arity assert. This is a node-only surface, never browser-reachable.

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
  SELECT s.id,
         SUM(b.score * b.weight)                                                AS "fusedScore",
         SUM(b.score * b.weight)                                                AS "rerankedScore",
         jsonb_object_agg(b.signal, b.score)                                    AS signals,
         bool_or(b.stale)                                                       AS stale
  FROM searchable s
  CROSS JOIN (SELECT set_config('hnsw.ef_search', ${o.profile.efSearch}::text, true)) cfg
  JOIN LATERAL (
    SELECT 'semantic' AS signal, 1 - (s.embedding <=> ${o.embedding}::vector) AS score, ${o.weights.semantic}::float8 AS weight, s.embedding_model <> ${o.profile.model} AS stale
    UNION ALL
    SELECT 'lexical',  COALESCE(ts_rank_cd(s.tsv, websearch_to_tsquery('english', ${o.text})), 0), ${o.weights.lexical}::float8,  false
    UNION ALL
    SELECT 'trigram',  COALESCE(similarity(s.label, ${o.text}), 0), ${o.weights.trigram}::float8, false
    UNION ALL
    SELECT 'phonetic', CASE WHEN dmetaphone(s.label) = dmetaphone(${o.text}) THEN 1.0 ELSE 0.0 END, ${o.weights.phonetic}::float8, false
  ) b ON true
  GROUP BY s.id
  ORDER BY "fusedScore" DESC
  LIMIT ${o.limit}
`

const HybridSearchLayer: Layer.Layer<HybridSearch, never, SqlClient.SqlClient | PgClient.PgClient> = HybridSearch.Default
```
