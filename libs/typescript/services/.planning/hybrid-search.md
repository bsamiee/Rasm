# [SERVICES_HYBRID_SEARCH]

One page owns the fused hybrid-retrieval search owner — `HybridSearch`, the single weighted-rank surface fusing semantic (vector/HNSW), lexical (tsvector/regconfig), trigram, and phonetic retrieval over the one `SqlBoundary` Postgres client. This is a first-class search domain distinct from raw persistence: a flagship app would hand-build exactly this fused-rank surface, so it is owned now rather than folded into `persistence.md`. The four retrieval signals are rows on one weighted-rank owner, never four parallel search services. The page rides the `persistence.md` `PgClient` and crosses no .NET wire.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]      | [OWNS]                                                       |
| :-----: | :------------- | :--------------------------------------------------------- |
|   [1]   | HYBRID_SEARCH  | the fused semantic+lexical+trigram+phonetic weighted-rank owner |

## [2]-[HYBRID_SEARCH]

- Owner: `HybridSearch`, the single fused weighted-rank owner over four retrieval signals — `semantic` (vector cosine distance over an embedding column with HNSW `efSearch` tuning), `lexical` (`tsvector`/`regconfig` full-text rank), `trigram` (`pg_trgm` similarity), and `phonetic` (a phonetic-key match) — each one signal row on the one owner with its weight, never a parallel search service.
- Cases: a query fans out to the four signals in one SQL round-trip through the `hybridQuery` `UNION ALL`-then-`GROUP BY`-rank set-algebraic statement the `SqlBoundary` issues — the `pgvector` `<=>` cosine distance, the `tsvector` `ts_rank_cd` over `websearch_to_tsquery`, the `pg_trgm` `similarity`/`%`, and the `fuzzystrmatch` `dmetaphone` phonetic key folded into one `jsonb_object_agg` per id, the `efSearch` set per statement through `set_config('hnsw.ef_search', ...)` — each signal contributing a normalized score; the fused rank is the weighted sum over the four normalized scores with the weights a `SearchWeights` row; the semantic signal carries the embedding profile (the model and the dimension count) and a staleness flag so a row whose embedding is stale re-ranks below a fresh one; the HNSW `efSearch` is a tuning row trading recall for latency; the trigram and phonetic signals catch typo and homophone matches the lexical signal misses; the result is one ranked `SearchHit` stream the consumer pages through.
- Entry: the search rides the one `persistence.md` `PgClient` surface — the vector, full-text, trigram, and phonetic columns are columns on the searched entity's `Model.Class`, and the fused query is one set-algebraic SQL the boundary issues with the embedding profile and the weights bound as parameters; the embedding profile dimensions the vector column and a re-embed job (a `WorkQueue` row) refreshes a stale embedding; the search is tenant-scoped through the same `app.current_tenant` GUC every query reads.
- Packages: `@effect/sql` and `@effect/sql-pg` for the set-algebraic query over the vector/tsvector/trigram/phonetic columns through the one `PgClient` (the `pgvector`, `pg_trgm`, and `fuzzystrmatch` Postgres extensions provisioned by `provisioning.md`), and `effect` for the `Stream` result and the weighted-rank fold.
- Growth: a new retrieval signal lands as one signal row on `HybridSearch` with its weight, never a parallel search service; a new embedding profile lands as one profile row; a new HNSW tuning lands as one `efSearch` value; a new fusion strategy lands as one rank arm.
- Boundary: the four signals are one fused owner and never four parallel search services; the search rides the one `PgClient` and never a second SQL surface; the fused query is one set-algebraic round-trip and never four sequential queries joined branch-side; the embedding staleness is a column flag the rank reads, never a recomputed branch-side heuristic; this is a node-only surface, never browser-reachable.

```ts contract
type SearchSignal = "semantic" | "lexical" | "trigram" | "phonetic";

interface SearchWeights {
  readonly semantic: number;
  readonly lexical: number;
  readonly trigram: number;
  readonly phonetic: number;
}

interface EmbeddingProfile {
  readonly model: string;
  readonly dimensions: number;
  readonly efSearch: number;
}

interface SearchHit {
  readonly id: string;
  readonly fusedScore: number;
  readonly signals: Record<SearchSignal, number>;
  readonly stale: boolean;
}

interface HybridSearch {
  readonly query: (options: {
    readonly tenant: string;
    readonly text: string;
    readonly embedding: ReadonlyArray<number>;
    readonly weights: SearchWeights;
    readonly profile: EmbeddingProfile;
    readonly limit: number;
  }) => Stream.Stream<SearchHit, SqlError.SqlError, PgClient.PgClient>;
}

const fuse = (signals: Record<SearchSignal, number>, weights: SearchWeights): number =>
  signals.semantic * weights.semantic +
  signals.lexical * weights.lexical +
  signals.trigram * weights.trigram +
  signals.phonetic * weights.phonetic;

const hybridQuery = (sql: SqlClient.SqlClient, o: {
  readonly text: string;
  readonly embedding: ReadonlyArray<number>;
  readonly weights: SearchWeights;
  readonly profile: EmbeddingProfile;
  readonly limit: number;
}) => sql<SearchHit>`
  WITH params AS (SELECT set_config('hnsw.ef_search', ${o.profile.efSearch}::text, true))
  SELECT id,
         SUM(score * weight) AS "fusedScore",
         jsonb_object_agg(signal, score) AS signals,
         bool_or(stale) AS stale
  FROM (
    SELECT id, 'semantic' AS signal, 1 - (embedding <=> ${sql.array(o.embedding)}::vector) AS score, ${o.weights.semantic} AS weight, embedding_model <> ${o.profile.model} AS stale FROM searchable
    UNION ALL
    SELECT id, 'lexical',  ts_rank_cd(tsv, websearch_to_tsquery('english', ${o.text})), ${o.weights.lexical},  false FROM searchable WHERE tsv @@ websearch_to_tsquery('english', ${o.text})
    UNION ALL
    SELECT id, 'trigram',  similarity(label, ${o.text}), ${o.weights.trigram}, false FROM searchable WHERE label % ${o.text}
    UNION ALL
    SELECT id, 'phonetic', CASE WHEN dmetaphone(label) = dmetaphone(${o.text}) THEN 1.0 ELSE 0.0 END, ${o.weights.phonetic}, false FROM searchable
  ) AS signals
  GROUP BY id
  ORDER BY "fusedScore" DESC
  LIMIT ${o.limit}
`;
```
