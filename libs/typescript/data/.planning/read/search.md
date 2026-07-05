# [DATA_SEARCH]

Retrieval as one owner: five lanes — FTS, trigram, phonetic, fuzzy, semantic — each a row carrying its grant and its rank fragment, fused by reciprocal-rank fusion (`Σ 1/(k + rank)`) inside the database, so one round trip answers what five sequential searches answer worse. The lane roster a query runs is the intersection of what it asks and what the scope's grants admit, computed per call from the capability report — a degraded scope answers with fewer lanes and says so in the reply. The BM25 relevance lane rides the ruled `vchord_bm25` grant with core FTS as its ungranted floor; the semantic lane rides the grant-ordered vector index (`vchordrq` where `vchord` holds, `hnsw` under bare `vector`) over an embedding table whose every row is keyed by the `Embedder` port's fingerprint, so vectors minted by different models can never join one distance scan. `Search.of(corpus)` binds the whole read family — fused search, facets, snippets, keyset cursor — and the index plane is provisioned state under the same DDL split as every relation: ensure at construction, grant-gated row by row.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                             |
| :-----: | :------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `PORTS`        | the `Embedder` port (fingerprint contract, `EmbedFault`) and the optional `Reranker`    |
|  [02]   | `INDEX_PLANE`  | the fingerprint brand, the embedding relation, the grant-gated index-row ensure fold    |
|  [03]   | `LANE_ROSTER`  | the closed five-lane row table — grant, rank fragment, per-call admission               |
|  [04]   | `FUSION_QUERY` | `Search.of` — the RRF statement, rerank window, facet/snippet/cursor families           |

## [2]-[PORTS]

- Owner: the `Embedder` `Context.Tag` — embed-with-fingerprint, the one cross-folder retrieval contract — and the `Reranker` tag read through `Effect.serviceOption` so rerank is presence-typed, never a knob.
- Packages: `effect` (`Context`, `Data`, `Array`).
- Entry: the runtime branch's embedding rows satisfy `Embedder` at app composition; nothing in this folder imports a provider — the port is the whole seam, and a scope without an embedder simply has no semantic lane, the same degradation shape as a missing grant.
- Receipt: `embed` answers vectors paired with the port's own `fingerprint` — the write path stamps rows with it, the read path scans by it; the fingerprint travels with the vectors because it IS their identity.
- Growth: an embedding capability axis (dimension negotiation, batch policy) is a member on this one port; a second model in one app is a second Layer against the same tag selected per scope, never a second tag.
- Law: `EmbedFault` is the port's typed failure — reason-discriminated `budget | provider | shape` — and retrieval folds it into lane exclusion BEFORE the census settles: the embed runs first, a failed embed folds to absence through `Effect.option`, the census marks the semantic lane `unembedded`, and the fused statement never names the lane its parameters cannot serve; text lanes still answer.
- Law: the port's provider side batches through `read/batch.md`'s engine — the window geometry is the satisfying Layer's concern; this port declares only the vector contract.

```typescript
import { Array, Context, Data, Effect } from "effect"

class EmbedFault extends Data.TaggedError("EmbedFault")<{
  readonly reason: "budget" | "provider" | "shape"
  readonly detail: string
}> {}

class Embedder extends Context.Tag("data/Embedder")<Embedder, {
  readonly fingerprint: Search.Fingerprint
  readonly embed: (
    texts: Array.NonEmptyReadonlyArray<string>,
  ) => Effect.Effect<Array.NonEmptyReadonlyArray<ReadonlyArray<number>>, EmbedFault>
}>() {}

class Reranker extends Context.Tag("data/Reranker")<Reranker, {
  readonly rerank: (
    query: string,
    hits: Array.NonEmptyReadonlyArray<{ readonly cell: string; readonly body: string }>,
  ) => Effect.Effect<Array.NonEmptyReadonlyArray<string>, EmbedFault>
}>() {}
```

## [3]-[INDEX_PLANE]

- Owner: the `Fingerprint` brand (`<model>:<dims>:<revision>`), the `retrieve_embedding` ensure whose primary key includes it, and the closed index-row vocabulary with its grant-gated ensure fold — vector method selection is grant-ordered data, never a query rewrite.
- Packages: `effect` (`Schema`, `Array`, `Effect`, `Option`); `@effect/sql` (`sql.unsafe` over closed ensure literals); `lane/capability.md` (`Capability.when` per row's grant); `lane/postgres.md` (the grants — `vchord`, `vector`, `bm25`, `trigram` — are matrix rows arriving as the granted set).
- Entry: `Search.ensure(corpus, dims)` runs at scope construction — index presence is provisioned state under the DDL split exactly like tables; a refused grant skips its row and the corresponding lane self-excludes at query time through the same grant read.
- Receipt: the ensure fold returns the registered row names — the corpus's index census, joined with the capability report in startup evidence.
- Growth: a model migration is a new fingerprint value — old vectors stay queryable under theirs until re-embedding completes; a new index posture (second metric, partial index) is one row; dims live in the DDL as data, so a dims change is a new fingerprint hence a new ensure, and mixed widths are refused by the engine.
- Law: every vector write and scan carries the fingerprint predicate — the column sits in the primary key and the semantic lane filters on it, so cross-model distance comparison is unrepresentable.
- Law: the vector row is ONE row with a grant-ordered method — `vchordrq` under `vchord`, else `hnsw` under `vector` — the stronger engine is data and an image upgrade re-indexes without touching a query.
- Law: the BM25 row is `vchord_bm25`'s index under the `bm25` grant; the trigram row is a GIN `gin_trgm_ops` index under `trigram`; the keyset row supports cursor pagination and carries no gate beyond core.
- RESEARCH: the `vchord_bm25` index DDL and rank-fragment spellings (the index access method name, the tokenizer registration, the query operator) settle from the extension's own reference before the `bm25` row's literals leave RESEARCH; the row, its grant, and its floor arm are settled law — only the SQL text awaits the catalogued spelling.

```typescript
import { Schema } from "effect"

const _Fingerprint = Schema.NonEmptyString.pipe(
  Schema.pattern(/^[a-z0-9._-]+:\d+:[a-z0-9._-]+$/),
  Schema.brand("Fingerprint"),
)

declare namespace Search {
  type Fingerprint = typeof _Fingerprint.Type
}

const _fingerprint = (model: string, dims: number, revision: string): Search.Fingerprint =>
  Schema.decodeUnknownSync(_Fingerprint)(`${model}:${dims}:${revision}`)

const _embeddingDdl = (dims: number): Capability.Ensure => ({
  relation: "retrieve_embedding",
  pg: `CREATE TABLE IF NOT EXISTS retrieve_embedding (
    corpus TEXT NOT NULL, cell TEXT NOT NULL,
    fingerprint TEXT NOT NULL,
    embedding vector(${dims}) NOT NULL,
    embedded_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (corpus, cell, fingerprint));`,
  sqlite: `CREATE TABLE IF NOT EXISTS retrieve_embedding (
    corpus TEXT NOT NULL, cell TEXT NOT NULL,
    fingerprint TEXT NOT NULL,
    embedding BLOB NOT NULL,
    embedded_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
    PRIMARY KEY (corpus, cell, fingerprint));`,
})

const _indexRows = {
  vectorChord: {
    lane: "semantic",
    grant: "vchord",
    ensure: (corpus: string) =>
      `CREATE INDEX IF NOT EXISTS ${corpus}_embedding_vchord ON retrieve_embedding
       USING vchordrq (embedding vector_cosine_ops) WHERE corpus = '${corpus}';`,
  },
  vectorHnsw: {
    lane: "semantic",
    grant: "vector",
    ensure: (corpus: string) =>
      `CREATE INDEX IF NOT EXISTS ${corpus}_embedding_hnsw ON retrieve_embedding
       USING hnsw (embedding vector_cosine_ops) WHERE corpus = '${corpus}';`,
  },
  trigram: {
    lane: "trigram",
    grant: "trigram",
    ensure: (corpus: string) => `CREATE INDEX IF NOT EXISTS ${corpus}_trgm ON ${corpus} USING gin (body gin_trgm_ops);`,
  },
  keyset: {
    lane: "keyset",
    grant: "core",
    ensure: (corpus: string) => `CREATE INDEX IF NOT EXISTS ${corpus}_keyset ON ${corpus} (score DESC, cell);`,
  },
} as const

declare namespace Search {
  type IndexKind = keyof typeof _indexRows
  type _IndexRows<T extends Record<IndexKind, { readonly lane: string; readonly grant: string; readonly ensure: (corpus: string) => string }> = typeof _indexRows> = T
}
```

## [4]-[LANE_ROSTER]

- Owner: the `_lanes` anchor — five rows, each `{ grant, rank }` where `rank` builds the lane's scored CTE body — and `_admitted`, the per-call fold intersecting the request's lanes with the scope's grants and the embedder's presence.
- Packages: composition over `[2]`/`[3]` values and the granted set; `effect` (`Array`, `HashSet`, `Option`).
- Growth: a sixth lane is one row — the fusion statement folds whatever the admission returns; a lane's SQL tuning edits its row alone.
- Law: every lane emits one shape — `(cell, rank)` with rank 1-based by lane-local score — because RRF consumes ranks, never scores; score normalization across heterogeneous lanes is exactly what the fusion deletes.
- Law: lane SQL rides its grant — `fts` under `bm25` with the core-FTS floor arm (`ts_rank_cd` over `websearch_to_tsquery`) serving where the grant is refused, `trigram` under `trigram` (`similarity()` ordering), `phonetic` and `fuzzy` under their contrib grants (`soundex()` equality, `levenshtein()` bounded distance), `semantic` under `vector` plus a present embedder (cosine `<=>` filtered by fingerprint); the sqlite profiles substitute FTS5 through the degradation vocabulary and drop what they cannot express.
- Law: the corpus contract is one relation with `cell` (the stable key), `body` (the searchable text), and the keyset columns — `Search.of` takes the corpus value, and a second searchable relation is a second binding, never a wider query.
- Law: admission is evidence — each requested lane resolves to `ran`, `ungranted`, `unembedded`, or `excluded`, and the fold's output is both the CTE roster and the reply census.

```typescript
const _lanes = {
  fts: {
    grant: "bm25",
    rank: (corpus: string) =>
      `SELECT cell, rank() OVER (ORDER BY ts_rank_cd(to_tsvector('simple', body), websearch_to_tsquery('simple', $1)) DESC) AS rank
       FROM ${corpus} WHERE to_tsvector('simple', body) @@ websearch_to_tsquery('simple', $1) LIMIT $2`,
  },
  trigram: {
    grant: "trigram",
    rank: (corpus: string) =>
      `SELECT cell, rank() OVER (ORDER BY similarity(body, $1) DESC) AS rank
       FROM ${corpus} WHERE body % $1 LIMIT $2`,
  },
  phonetic: {
    grant: "phonetic",
    rank: (corpus: string) =>
      `SELECT cell, rank() OVER (ORDER BY body) AS rank
       FROM ${corpus} WHERE soundex(body) = soundex($1) LIMIT $2`,
  },
  fuzzy: {
    grant: "fuzzy",
    rank: (corpus: string) =>
      `SELECT cell, rank() OVER (ORDER BY levenshtein(left(body, 64), left($1, 64))) AS rank
       FROM ${corpus} LIMIT $2`,
  },
  semantic: {
    grant: "vector",
    rank: (corpus: string) =>
      `SELECT e.cell, rank() OVER (ORDER BY e.embedding <=> $3::vector) AS rank
       FROM retrieve_embedding e WHERE e.corpus = '${corpus}' AND e.fingerprint = $4 LIMIT $2`,
  },
} as const

declare namespace Search {
  type Lane = keyof typeof _lanes
  type Disposition = "ran" | "ungranted" | "unembedded" | "excluded"
  type _LaneRows<T extends Record<Lane, { readonly grant: string; readonly rank: (corpus: string) => string }> = typeof _lanes> = T
}
```

## [5]-[FUSION_QUERY]

- Owner: `Search.of(corpus)` — the bound read family: `search` (the fused RRF statement plus optional rerank), `facets`, the snippet projection, the keyset cursor codec, and `ensure` from `[3]`; one request shape carries every modality.
- Packages: `effect` (`Effect`, `Option`, `HashMap`, `Record`, `Schema`); `@effect/sql` (the rerank-window body fetch and the snippet fetch are each ONE `sql.in` set-shaped statement over the hit cells, never a per-hit query); `lane/capability.md` (`Capability` — the per-call grant read).
- Entry: `bound.search(request)` — text plus option-bag fields (`lanes`, `limit`, `k`, `cursor`, `facets`, `snippet`, `rerank`); the reply is one `Search.Page` carrying scored hits with snippets, facet counts when asked, the next cursor when more remains, and the lane census that actually ran.
- Receipt: `Search.Page.lanes` names each lane's disposition — a degraded scope is visible in every reply, so a relevance regression traces to capability, never to guesswork.
- Growth: rerank depth, fusion constant `k`, and snippet shape are request fields with policy defaults; a new reply projection (highlight offsets, per-lane scores) is a field on the page, never a second search.
- Law: fusion is in-database — admitted lanes materialize as CTEs, `Σ 1.0/(k + rank)` groups by cell, order-by-score-desc with the keyset predicate pages stably, and the statement is ONE round trip; assembling lanes in process re-buys N queries and loses the shared plan.
- Law: the cursor is opaque and typed — `{ score, cell }` under one composed codec, `Schema.StringFromBase64Url` over `Schema.parseJson`, so encode and decode share the schema and a malformed caller cursor is `ParseError` on the admission rail; a raw offset is the rejected pagination, and the cursor mints from the FUSED order — rerank re-orders presentation inside the page and never moves the keyset coordinate, so a full-page rerank window cannot skip rows.
- Law: snippets ride the granted relevance lane — the `bm25` snippet function where the grant holds (its spelling travels with the `[3]` RESEARCH row), `ts_headline` as the in-core floor — selected by the same grant read as the lane itself.
- Law: rerank is a window policy — when the `Reranker` is present and the request asks, the top `window` fused hits re-order by the port's verdict and the tail keeps fusion order; the candidate bodies arrive through one set-shaped fetch; absence of the port makes rerank a no-op through `serviceOption`, and a rerank fault degrades to fusion order through `Effect.option` — retrieval never fails on the accelerator.

```typescript
import { HashMap, HashSet, Option, Record, Schema } from "effect"
import { SqlClient, type SqlError } from "@effect/sql"
import { Capability } from "../lane/capability.ts"

declare namespace Search {
  type Corpus = { readonly table: string; readonly dims: number }
  type Request = {
    readonly text: string
    readonly lanes?: ReadonlyArray<Lane>
    readonly limit?: number
    readonly k?: number
    readonly cursor?: string
    readonly facets?: ReadonlyArray<string>
    readonly snippet?: boolean
    readonly rerank?: number
  }
  type Hit = { readonly cell: string; readonly score: number; readonly snippet: Option.Option<string> }
  type Page = {
    readonly hits: ReadonlyArray<Hit>
    readonly facets: ReadonlyArray<{ readonly dim: string; readonly value: string; readonly count: number }>
    readonly cursor: Option.Option<string>
    readonly lanes: Record.ReadonlyRecord<Lane, Disposition>
  }
}

const _Cursor = Schema.compose(
  Schema.StringFromBase64Url,
  Schema.parseJson(Schema.Struct({ score: Schema.Number, cell: Schema.String })),
)

const _fused = (corpus: string, lanes: ReadonlyArray<Search.Lane>, k: number, cursored: boolean, limit: number): string => {
  const score = `sum(1.0 / (${k} + rank))`
  const cursorAt = lanes.includes("semantic") ? 5 : 3
  return [
    "WITH",
    lanes.map((lane) => `${lane} AS (${_lanes[lane].rank(corpus)})`).join(", "),
    `SELECT cell, ${score} AS score FROM (`,
    lanes.map((lane) => `SELECT cell, rank FROM ${lane}`).join(" UNION ALL "),
    ") pool GROUP BY cell",
    cursored ? `HAVING ${score} < $${cursorAt} OR (${score} = $${cursorAt} AND cell > $${cursorAt + 1})` : "",
    `ORDER BY score DESC, cell LIMIT ${limit}`,
  ].join(" ")
}

const _admitted = (
  requested: ReadonlyArray<Search.Lane>,
  granted: HashSet.HashSet<string>,
  embedded: boolean,
): Record.ReadonlyRecord<Search.Lane, Search.Disposition> =>
  Record.map(_lanes, (row, lane) =>
    !Array.contains(requested, lane)
      ? "excluded"
      : lane === "semantic" && !embedded
        ? "unembedded"
        : HashSet.has(granted, row.grant) || lane === "fts"
          ? "ran"
          : "ungranted")
```

```mermaid
flowchart LR
  R[request] --> A[admission: lanes ∩ grants ∩ embedder]
  A --> C[lane CTEs]
  C --> F[RRF Σ 1/(k+rank) GROUP BY cell]
  F --> K[keyset predicate + ORDER BY score]
  K --> W[rerank window via Reranker]
  W --> P[Page: hits + facets + cursor + lane census]
```

```typescript
const _ensure = (corpus: Search.Corpus) =>
  Effect.flatMap(Capability, (capability) =>
    Effect.flatMap(SqlClient.SqlClient, (sql) =>
      Effect.gen(function* () {
        const vector = yield* capability.when("vchord", Effect.succeed(_indexRows.vectorChord)).pipe(
          Effect.flatMap(Option.match({
            onNone: () => capability.when("vector", Effect.succeed(_indexRows.vectorHnsw)),
            onSome: (row) => Effect.succeed(Option.some(row)),
          })),
        )
        yield* Effect.when(
          sql.onDialectOrElse({
            orElse: () => sql.unsafe(_embeddingDdl(corpus.dims).sqlite),
            pg: () => sql.unsafe(_embeddingDdl(corpus.dims).pg),
          }),
          () => Option.isSome(vector),
        )
        const admitted = [
          ...Option.toArray(vector),
          ...Array.getSomes(yield* Effect.forEach([_indexRows.trigram, _indexRows.keyset], (row) =>
            row.grant === "core"
              ? Effect.succeed(Option.some(row))
              : capability.when(row.grant, Effect.succeed(row)))),
        ]
        yield* Effect.forEach(admitted, (row) => sql.unsafe(row.ensure(corpus.table)), { discard: true })
        return admitted.map((row) => row.lane)
      })))

const _reranked = (
  sql: SqlClient.SqlClient,
  corpus: Search.Corpus,
  text: string,
  window: number,
  hits: ReadonlyArray<Search.Hit>,
) =>
  Effect.flatMap(Effect.serviceOption(Reranker), (port) =>
    Effect.transposeOption(
      Option.map(
        Option.filter(port, () => window > 0 && hits.length > 0),
        (reranker) =>
          Effect.gen(function* () {
            const head = hits.slice(0, window)
            const bodies = yield* sql`SELECT cell, body FROM ${sql(corpus.table)} WHERE ${sql.in("cell", head.map((hit) => hit.cell))}`
            const candidates = bodies.map((row) => ({ cell: String(row["cell"]), body: String(row["body"]) }))
            return yield* Array.isNonEmptyReadonlyArray(candidates)
              ? Effect.map(reranker.rerank(text, candidates), (order) => {
                  const byCell = HashMap.fromIterable(head.map((hit) => [hit.cell, hit] as const))
                  return [...Array.getSomes(order.map((cell) => HashMap.get(byCell, cell))), ...hits.slice(window)]
                }).pipe(Effect.option, Effect.map(Option.getOrElse(() => hits)))
              : Effect.succeed(hits)
          }),
      )).pipe(Effect.map(Option.getOrElse(() => hits))))

const _search = (corpus: Search.Corpus) =>
  (request: Search.Request) =>
    Effect.gen(function* () {
      const sql = yield* SqlClient.SqlClient
      const capability = yield* Capability
      const requested = request.lanes ?? ["fts", "trigram", "semantic"]
      const embedder = yield* Effect.serviceOption(Embedder)
      const embedded = Option.flatten(yield* Effect.transposeOption(
        Option.map(Option.filter(embedder, () => Array.contains(requested, "semantic")), (port) =>
          Effect.option(
            Effect.map(port.embed([request.text]), (vectors) => ({ vector: vectors[0], fingerprint: port.fingerprint }))))))
      const census = _admitted(requested, capability.granted, Option.isSome(embedded))
      const running = Record.keys(census).filter((lane) => census[lane] === "ran")
      const cursor = yield* Effect.transposeOption(
        Option.map(Option.fromNullable(request.cursor), Schema.decodeUnknown(_Cursor)))
      const limit = request.limit ?? 20
      const rows = yield* sql.unsafe(
        _fused(corpus.table, running, request.k ?? 60, Option.isSome(cursor), limit + 1),
        [request.text, limit, ...Option.match(embedded, {
          onNone: () => [],
          onSome: (held) => [`[${held.vector.join(",")}]`, held.fingerprint],
        }), ...Option.match(cursor, { onNone: () => [], onSome: (held) => [held.score, held.cell] })],
      )
      const scored = rows.slice(0, limit).map((row) => ({
        cell: String(row["cell"]),
        score: Number(row["score"]),
        snippet: Option.none<string>(),
      }))
      const clipped = request.snippet === true && scored.length > 0
        ? yield* Effect.map(
            sql`SELECT cell, ts_headline('simple', body, websearch_to_tsquery('simple', ${request.text})) AS clip
                FROM ${sql(corpus.table)} WHERE ${sql.in("cell", scored.map((hit) => hit.cell))}`,
            (clips) => {
              const byCell = HashMap.fromIterable(clips.map((row) => [String(row["cell"]), String(row["clip"])] as const))
              return scored.map((hit) => ({ ...hit, snippet: HashMap.get(byCell, hit.cell) }))
            },
          )
        : scored
      const ordered = yield* _reranked(sql, corpus, request.text, request.rerank ?? 0, clipped)
      const facets = request.facets === undefined ? [] : yield* _facets(corpus)(request.facets)
      const next = rows.length > limit ? Option.fromNullable(clipped.at(-1)) : Option.none<Search.Hit>()
      return {
        hits: ordered,
        facets,
        cursor: yield* Effect.transposeOption(
          Option.map(next, (hit) => Schema.encode(_Cursor)({ score: hit.score, cell: hit.cell }))),
        lanes: census,
      } satisfies Search.Page
    })

const _facets = (corpus: Search.Corpus) =>
  (dims: ReadonlyArray<string>) =>
    Effect.flatMap(SqlClient.SqlClient, (sql) =>
      Effect.forEach(dims, (dim) =>
        Effect.map(
          sql`SELECT ${sql(dim)} AS value, count(*) AS count FROM ${sql(corpus.table)} GROUP BY ${sql(dim)} ORDER BY count DESC LIMIT 50`,
          (rows) => rows.map((row) => ({ dim, value: String(row["value"]), count: Number(row["count"]) })),
        )).pipe(Effect.map(Array.flatten)))

const Search = {
  Fingerprint: _Fingerprint,
  fingerprint: _fingerprint,
  Cursor: _Cursor,
  lanes: _lanes,
  indexes: _indexRows,
  embedding: _embeddingDdl,
  admitted: _admitted,
  fused: _fused,
  of: (corpus: Search.Corpus) => ({
    ensure: _ensure(corpus),
    search: _search(corpus),
    facets: _facets(corpus),
  }),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Embedder, EmbedFault, Reranker, Search }
```
