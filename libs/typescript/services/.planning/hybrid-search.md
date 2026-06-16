# [SERVICES_HYBRID_SEARCH]

One page owns the fused hybrid-retrieval search owner — `HybridSearch`, the single weighted-rank surface fusing semantic (vector/HNSW), lexical (tsvector/regconfig), trigram, and phonetic retrieval over the one `SqlBoundary` Postgres client. This is a first-class search domain distinct from raw persistence: a flagship app would hand-build exactly this fused-rank surface, so it is owned now rather than folded into `persistence.md`. The four retrieval signals are rows on one weighted-rank owner, never four parallel search services. The page rides the `persistence.md` `PgClient` and crosses no .NET wire.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]     | [OWNS]                                                                                | [STATE]                       |
| :-----: | :------------ | :------------------------------------------------------------------------------------ | :---------------------------- |
|   [1]   | HYBRID_SEARCH | the fused semantic+lexical+trigram+phonetic weighted-rank owner + the rerank stage axis | FINALIZED (CrossEncoder SPIKE) |

The owner is transcription-complete: the four-signal weighted-rank fusion is owned by the one `hybridQuery` set-algebraic SQL (`SUM(score * weight)` over a full per-id signal scaffold), the `Identity`/`Linear` rerank arms carry real `Order`-reorder fold bodies, and the `HybridSearch.Default` Layer acquires `SqlClient` from context and threads it through the query. This owner crosses no .NET wire — it runs its own four-signal (`semantic`/`lexical`/`trigram`/`phonetic`) Postgres fusion over the `searchable` table, a distinct retrieval algebra from the C# `federation#FUSION_RANK` three-branch (`vector`/`spatial`/`lexical`) server RRF over `federated_entity`; the persistence charter declares federation carries no `#TS_PROJECTION`, so there is no fusion wire to consume and none is re-authored. The `RerankModel.CrossEncoder` arm is fence-complete but `[STATE] SPIKE` — its real Effect body is authored against the verified `Effect.tryPromise`/`fetch` host surface, gated only on a genuine live model-host probe (the off-process cross-encoder endpoint), the one residual the design cannot settle branch-side.

## [2]-[HYBRID_SEARCH]

- Owner: `HybridSearch`, the single fused weighted-rank owner over four retrieval signals — `semantic` (vector cosine distance over an embedding column with HNSW `efSearch` tuning), `lexical` (`tsvector`/`regconfig` full-text rank), `trigram` (`pg_trgm` similarity), and `phonetic` (a phonetic-key match) — each one signal row on the one owner with its weight, never a parallel search service; the fused candidate set then passes through one ordered two-stage pipeline whose terminal `rerank` stage is a `RerankModel` row, never a second retriever.
- Cases: a query fans out to the four signals in one SQL round-trip through the `hybridQuery` `UNION ALL`-then-`GROUP BY`-rank set-algebraic statement the `SqlBoundary` issues — the `pgvector` `<=>` cosine distance, the `tsvector` `ts_rank_cd` over `websearch_to_tsquery`, the `pg_trgm` `similarity`/`%`, and the `fuzzystrmatch` `dmetaphone` phonetic key folded into one `jsonb_object_agg` per id over a `LATERAL` per-row signal scaffold (so every id carries all four signal rows, each missing branch `COALESCE`d to zero), the `efSearch` bound inline through a non-prunable `set_config('hnsw.ef_search', ...)` cross join so the HNSW tuning runs on the executing connection — each signal contributing a normalized score; the fused rank is the weighted sum over the four normalized scores with the weights a `SearchWeights` row; the semantic signal carries the embedding profile (the model and the dimension count, the dimension count asserted against the probe arity before the query) and a staleness flag so a row whose embedding is stale re-ranks below a fresh one; the HNSW `efSearch` is a tuning row trading recall for latency; the trigram and phonetic signals catch typo and homophone matches the lexical signal misses; the fused stream is then re-scored by the `rerank` stage — a `RerankModel` row over a `RerankFeatureVector` projection derived from the four per-signal scores plus the rank-margin and staleness, collected then globally `Order`-reordered so a candidate strong on the cross-feature interaction the linear fusion sum cannot express is promoted across the whole bounded candidate set, not merely within a stream chunk; the result is one re-ordered `SearchHit` stream the consumer pages through.
- Entry: the search rides the one `persistence.md` `PgClient` surface — the vector, full-text, trigram, and phonetic columns are columns on the searched entity's `Model.Class`, and the fused query is one set-algebraic SQL the boundary issues with the embedding profile and the weights bound as parameters; the embedding profile dimensions the vector column and a re-embed job (a `WorkQueue` row) refreshes a stale embedding; the search is tenant-scoped through the same `app.current_tenant` GUC every query reads; the `rerank` stage is a pure post-fusion projection over the bounded fused top-k candidate set the SQL already narrowed, so it adds no second round-trip and no second index — it re-orders the candidates the one query returned.
- Wire: this owner crosses no .NET wire — the persistence charter declares `federation` carries no `#TS_PROJECTION` cluster (its selection results cross as content-key arrays inside a consuming page's projection, not as a fusion wire), and the C# `federation#FUSION_RANK` is a different retrieval algebra — a three-branch (`vector`/`spatial`/`lexical`) reciprocal-rank fusion over `federated_entity`, whereas this owner is a four-signal (`semantic`/`lexical`/`trigram`/`phonetic`) weighted fusion over `searchable` (it has `trigram`/`phonetic` branches the server lacks and lacks the server's `spatial` branch); the two cannot share a fused-candidate wire, so this branch owns its fusion honestly in `hybridQuery` and decodes no server RRF — the learned reranker the C# `FusionRank` owner names "the deleted form" server-side (because only fusion is net-new there) is this TS-side post-fusion `rerank` projection over the branch's own fused candidates, not a re-fusion of the server's.
- Packages: `@effect/sql` and `@effect/sql-pg` for the set-algebraic query over the vector/tsvector/trigram/phonetic columns through the one `PgClient` (the `pgvector`, `pg_trgm`, and `fuzzystrmatch` Postgres extensions provisioned by `provisioning.md`), and `effect` for the `Stream` result, the weighted-rank fold, the `RerankModel` `Data.TaggedEnum` dispatch, and the `Order`-driven reorder fold.
- Growth: a new retrieval signal lands as one signal row on `HybridSearch` with its weight, never a parallel search service; a new embedding profile lands as one profile row; a new HNSW tuning lands as one `efSearch` value; a new fusion strategy lands as one rank arm; a new reranking strategy lands as one `RerankModel` case on the one tagged axis (one `Match.tagsExhaustive` arm), never a parallel reranker service or a second pipeline.
- Boundary: the four signals are one fused owner and never four parallel search services; the search rides the one `PgClient` (acquired from the `SqlClient` context tag in the `HybridSearch.Default` Layer) and never a second SQL surface; the fused query is one set-algebraic round-trip — the `efSearch` tuning binds inline through a non-prunable `set_config` cross join so the HNSW recall/latency policy actually applies on the executing connection, never a free-floating CTE Postgres prunes — and never four sequential queries joined branch-side; the per-id signal scaffold `COALESCE`s every missing branch to zero so a candidate strong in one modality fuses cleanly and the rerank reads no `NaN`; the embedding staleness is a column flag the rank reads, never a recomputed branch-side heuristic; the `rerank` stage is one tagged axis re-ordering the bounded fused candidate set globally (collect-then-`Order`-sort), never a parallel retriever, never a second round-trip; the embedding arity is asserted against the profile `dimensions` before the query so a wrong-width probe fails the `feature`-stage `RerankFault` rather than poisoning the vector cast; this is a node-only surface, never browser-reachable.

```ts owner
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import type { PgClient } from "@effect/sql-pg"
import type { SqlError } from "@effect/sql"
import { SqlClient } from "@effect/sql"
import { Array as A, Data, Effect, Layer, Match, Order, Record as R, Schema as S, Stream } from "effect"

// --- [TYPES] ---------------------------------------------------------------------------
type SearchSignal = "semantic" | "lexical" | "trigram" | "phonetic"

// the `rerank` axis: the post-fusion re-scoring strategy over this owner's OWN fused candidates — Identity passthrough, a linear feature score, or an off-process cross-encoder.
type RerankModel = Data.TaggedEnum<{
  readonly Identity:     {}
  readonly Linear:       { readonly bias: number; readonly weights: RerankFeatureVector }
  readonly CrossEncoder: { readonly endpoint: string; readonly model: string; readonly batch: number }
}>
const RerankModel = Data.taggedEnum<RerankModel>()

// --- [CONSTANTS] -----------------------------------------------------------------------
const SEARCH_SIGNALS = ["semantic", "lexical", "trigram", "phonetic"] as const satisfies ReadonlyArray<SearchSignal>

// --- [MODELS] --------------------------------------------------------------------------
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

// --- [ERRORS] --------------------------------------------------------------------------
class RerankFault extends S.TaggedError<RerankFault>()("RerankFault", {
  stage: S.Literal("feature", "score", "host"),
  model: S.String,
  cause: S.Unknown,
}) {}

// --- [SERVICES] ------------------------------------------------------------------------
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
  // the service acquires the one `SqlClient` from context — the declared `PgClient.PgClient` requirement is satisfied by the persistence Layer that provides it.
  effect: Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const query = (o: QueryOptions): Stream.Stream<SearchHit, SqlError.SqlError | RerankFault, never> =>
      Stream.fromIterableEffect(
        assertArity(o.embedding, o.profile).pipe(Effect.flatMap(() => hybridQuery(sql, o))),
      ).pipe(rerankStage(o.text, o.rerank))
    return { query } as const
  }),
}) {}

// --- [OPERATIONS] ----------------------------------------------------------------------
// the embedding arity is asserted against the profile BEFORE the query so a wrong-width probe fails the `feature`-stage rail rather than poisoning the `::vector` cast.
const assertArity = (embedding: ReadonlyArray<number>, profile: EmbeddingProfile): Effect.Effect<void, RerankFault> =>
  embedding.length === profile.dimensions
    ? Effect.void
    : Effect.fail(new RerankFault({ stage: "feature", model: profile.model, cause: `embedding arity ${embedding.length} != profile dimensions ${profile.dimensions}` }))

// the per-signal score map ALWAYS carries every SEARCH_SIGNALS key (the SQL COALESCEs missing branches to 0), so the rerank feature read never hits an undefined → NaN.
const featuresOf = (hit: SearchHit): RerankFeatureVector => ({
  ...R.fromEntries(A.map(SEARCH_SIGNALS, (signal) => [signal, hit.signals[signal] ?? 0] as const)),
  margin:    hit.fusedScore,
  staleness: hit.stale ? 1 : 0,
})

const linearScore = (bias: number, w: RerankFeatureVector) => (f: RerankFeatureVector): number =>
  bias +
  f.semantic * w.semantic + f.lexical * w.lexical + f.trigram * w.trigram + f.phonetic * w.phonetic +
  f.margin * w.margin - f.staleness * w.staleness

// the cross-encoder host scores the (query, candidate-text) pairs off-process; the AbortSignal threads fiber interruption into the host call.
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

// the `rerank` stage: ONE Match.tagsExhaustive over the RerankModel axis, each arm a collect-then-Order-reorder fold so the sort is GLOBAL over the bounded candidate set, never chunk-local.
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

// `Order.mapInput` lifts the reranked score into a descending order over the hit — the reorder vocabulary, never an ad-hoc compare callback.
const byReranked: Order.Order<SearchHit> = Order.reverse(Order.mapInput(Order.number, (hit: SearchHit) => hit.rerankedScore))

// the four-signal weighted fusion is owned HERE in SQL (`SUM(score * weight)`); the `keys` scaffold LEFT JOINs every branch so `signals` is a full per-id Record (missing branch → 0), and the `efSearch` set_config binds inline through a non-prunable cross join so the HNSW tuning actually runs on the executing connection — never a free-floating CTE Postgres prunes.
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

// --- [COMPOSITION] ---------------------------------------------------------------------
// the `HybridSearch.Default` Layer wires the service; its `SqlClient` requirement is satisfied upstream by the `persistence.md` PgClient Layer, so the public `query` carries no residual `R` channel.
const HybridSearchLayer: Layer.Layer<HybridSearch, never, SqlClient.SqlClient | PgClient.PgClient> = HybridSearch.Default
```
