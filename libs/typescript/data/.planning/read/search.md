# [DATA_SEARCH]

Retrieval as one owner: five lanes — FTS, trigram, phonetic, fuzzy, semantic — each a row carrying its grant and its rank fragment, fused by reciprocal-rank fusion (`Σ 1/(k + rank)`) inside the database, so one round trip answers what five sequential searches answer worse. The lane roster a query runs is the intersection of what it asks and what the scope's grants admit, computed per call from the capability report — a degraded scope answers with fewer lanes and says so in the reply. The BM25 relevance lane rides the ruled `vchord_bm25` grant with core FTS as its ungranted floor; the semantic lane rides the grant-ordered vector index (`vchordrq` where `vchord` holds, `hnsw` under bare `vector`) over an embedding table whose every row is keyed by the `Embedder` port's fingerprint, so vectors minted by different models can never join one distance scan. `Search.of(corpus)` binds the whole read family — fused search, facets, snippets, keyset cursor — and the index plane is provisioned state under the same DDL split as every relation: `Search.ddl` derives the grant-gated ensure roster as data the provision plane applies, and the runtime never executes a schema statement.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                               |
| :-----: | :------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `PORTS`        | the `Embedder` port (fingerprint contract, `EmbedFault`) and the optional `Reranker` |
|  [02]   | `INDEX_PLANE`  | the fingerprint brand, the embedding relation, the grant-gated index-row ensure fold |
|  [03]   | `LANE_ROSTER`  | the closed five-lane row table — grant, rank fragment, per-call admission            |
|  [04]   | `FUSION_QUERY` | `Search.of` — the RRF statement, rerank window, facet/snippet/cursor families        |

## [02]-[PORTS]

- Owner: the `Embedder` `Context.Tag` — embed-with-fingerprint, the one cross-folder retrieval contract — and the `Reranker` tag read through `Effect.serviceOption` so rerank is presence-typed, never a knob.
- Packages: `effect` (`Context`, `Schema`, `Array`).
- Entry: the runtime branch's embedding rows satisfy `Embedder` at app composition; nothing in this folder imports a provider — the port is the whole seam, and a scope without an embedder simply has no semantic lane, the same degradation shape as a missing grant.
- Receipt: `embed` answers vectors paired with the port's own `fingerprint` — the write path stamps rows with it, the read path scans by it; the fingerprint travels with the vectors because it IS their identity.
- Growth: an embedding capability axis (dimension negotiation, batch policy) is a member on this one port; a second model in one app is a second Layer against the same tag selected per scope, never a second tag.
- Law: `EmbedFault` is the port's typed failure — reason-discriminated `budget | provider | shape`, schema-tagged so the persisted request band and any wire crossing carry it structurally — and retrieval folds it into lane exclusion BEFORE the census settles: the embed runs first, a failed embed folds to absence through `Effect.option`, the census marks the semantic lane `unembedded`, and the fused statement never names the lane its parameters cannot serve; text lanes still answer.
- Law: the port's provider side batches through `read/batch.md`'s engine — the window geometry is the satisfying Layer's concern; this port declares only the vector contract.

```typescript
import { Array, Context, Effect, Schema } from "effect"

class EmbedFault extends Schema.TaggedError<EmbedFault>()("EmbedFault", {
  reason: Schema.Literal("budget", "provider", "shape"),
  detail: Schema.String,
}) {}

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

## [03]-[INDEX_PLANE]

- Owner: the `Fingerprint` brand (`<model>:<dims>:<revision>`), the `Table` identifier brand every corpus coordinate rides, the `retrieve_embedding` ensure whose primary key includes the fingerprint, and the closed index-row vocabulary whose grant-gated selection emits PROVISION data — vector method selection is grant-ordered data, never a query rewrite, and the runtime never executes a DDL statement.
- Packages: `effect` (`Schema`, `Array`, `HashSet`, `Option`); `lane/capability.md` (`Capability.Ensure` — the shape the provision plane applies and the rail proves); `lane/postgres.md` (the grants — `vchord`, `vector`, `bm25`, `trigram` — are matrix rows arriving as the granted set).
- Entry: `Search.ddl(corpus, granted)` derives the corpus's ensure roster — the embedding relation plus the admitted index rows — which the provision plane applies and `lane/tenant.md`'s roster collection proves at scope construction; an absent index degrades scan speed, never correctness, so index presence is a performance fact and only the RELATION rides the fail-closed check.
- Receipt: the derivation returns the admitted row names — the corpus's index census, joined with the capability report in startup evidence.
- Growth: a model migration is a new fingerprint value — old vectors stay queryable under theirs until re-embedding completes; a new index posture (second metric, partial index) is one row; dims live in the DDL as data, so a dims change is a new fingerprint hence a new ensure, and mixed widths are refused by the engine.
- Law: the corpus coordinate is a branded identifier — `Search.Table` pattern-refines to `[a-z_][a-z0-9_]*`, so a caller-derived string can never reach an identifier position and the ensure texts interpolate only sealed, brand-proven names; facet dimensions ride the same brand because a facet name is a column identifier and the identifier law admits no second lexical class.
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

const _Table = Schema.NonEmptyString.pipe(
  Schema.pattern(/^[a-z_][a-z0-9_]*$/),
  Schema.brand("CorpusTable"),
)

declare namespace Search {
  type Fingerprint = typeof _Fingerprint.Type
  type Table = typeof _Table.Type
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
} as const satisfies {
  readonly [row: string]: { readonly lane: string; readonly grant: string; readonly ensure: (corpus: string) => string }
}

declare namespace Search {
  type IndexKind = keyof typeof _indexRows
}
```

## [04]-[LANE_ROSTER]

- Owner: the `_lanes` anchor — five rows, each `{ grant, rank }` where `rank` builds the lane's scored CTE body as a composed `sql` FRAGMENT over a typed bind value — and `_admitted`, the per-call fold intersecting the request's lanes with the scope's grants and the embedder's presence.
- Packages: composition over `[2]`/`[3]` values and the granted set; `@effect/sql` (the `sql` fragment constructor — every parameter binds by value inside the fragment, so no positional index exists anywhere on the page); `effect` (`Array`, `HashSet`, `Option`).
- Growth: a sixth lane is one row — the fusion statement folds whatever the admission returns; a lane's SQL tuning edits its row alone; a new bind axis is a `Search.Bind` field every row can read.
- Law: lane rows are fragment builders, never SQL text — `rank(sql, corpus, bind)` interpolates `bind.text`/`bind.limit`/the vector literal as BOUND parameters and the corpus only through the brand-proven identifier or a value predicate, so a lane-set change cannot misalign parameters (there are none to count) and the statement stays typed, batched, and dialect-switched; hand-assembled `$N` text is the deleted spelling.
- Law: every lane emits one shape — `(cell, rank)` with rank 1-based by lane-local score — because RRF consumes ranks, never scores; score normalization across heterogeneous lanes is exactly what the fusion deletes.
- Law: lane SQL rides its grant AND its dialect — `fts` under `bm25` with the core-FTS floor arm (`ts_rank_cd` over `websearch_to_tsquery`) serving where the grant is refused and the FTS5 `MATCH` arm serving the sqlite profiles through `onDialectOrElse` inside the row itself, `trigram` under `trigram` (`similarity()` ordering), `phonetic` and `fuzzy` under their contrib grants (`soundex()` equality, `levenshtein()` bounded distance), `semantic` under `vector` plus a present embedder (cosine `<=>` filtered by fingerprint); a lane the profile cannot express self-excludes through its grant, so degradation is the fence, not prose.
- Law: the corpus contract is one relation with `cell` (the stable key), `body` (the searchable text), and the keyset columns — `Search.of` takes the corpus value, and a second searchable relation is a second binding, never a wider query.
- Law: admission is evidence — each requested lane resolves to `ran`, `ungranted`, `unembedded`, or `excluded`, and the fold's output is both the CTE roster and the reply census.

```typescript
import type { SqlClient } from "@effect/sql"

declare namespace Search {
  type Bind = {
    readonly text: string
    readonly limit: number
    readonly vector: Option.Option<{ readonly literal: string; readonly fingerprint: Fingerprint }>
  }
}

const _lanes = {
  fts: {
    grant: "bm25",
    rank: (sql: SqlClient.SqlClient, corpus: Search.Table, bind: Search.Bind) =>
      sql.onDialectOrElse({
        orElse: () =>
          sql`SELECT cell, rank() OVER (ORDER BY rank) AS rank
              FROM ${sql(`${corpus}_fts`)} WHERE ${sql(`${corpus}_fts`)} MATCH ${bind.text} LIMIT ${bind.limit}`,
        pg: () =>
          sql`SELECT cell, rank() OVER (ORDER BY ts_rank_cd(to_tsvector('simple', body), websearch_to_tsquery('simple', ${bind.text})) DESC) AS rank
              FROM ${sql(corpus)} WHERE to_tsvector('simple', body) @@ websearch_to_tsquery('simple', ${bind.text}) LIMIT ${bind.limit}`,
      }),
  },
  trigram: {
    grant: "trigram",
    rank: (sql: SqlClient.SqlClient, corpus: Search.Table, bind: Search.Bind) =>
      sql`SELECT cell, rank() OVER (ORDER BY similarity(body, ${bind.text}) DESC) AS rank
          FROM ${sql(corpus)} WHERE body % ${bind.text} LIMIT ${bind.limit}`,
  },
  phonetic: {
    grant: "phonetic",
    rank: (sql: SqlClient.SqlClient, corpus: Search.Table, bind: Search.Bind) =>
      sql`SELECT cell, rank() OVER (ORDER BY body) AS rank
          FROM ${sql(corpus)} WHERE soundex(body) = soundex(${bind.text}) LIMIT ${bind.limit}`,
  },
  fuzzy: {
    grant: "fuzzy",
    rank: (sql: SqlClient.SqlClient, corpus: Search.Table, bind: Search.Bind) =>
      sql`SELECT cell, rank() OVER (ORDER BY levenshtein(left(body, 64), left(${bind.text}, 64))) AS rank
          FROM ${sql(corpus)} LIMIT ${bind.limit}`,
  },
  semantic: {
    grant: "vector",
    rank: (sql: SqlClient.SqlClient, corpus: Search.Table, bind: Search.Bind) =>
      Option.match(bind.vector, {
        onNone: () => sql`SELECT cell, 1 AS rank FROM retrieve_embedding WHERE 1 = 0`,
        onSome: (held) =>
          sql`SELECT e.cell, rank() OVER (ORDER BY e.embedding <=> ${held.literal}::vector) AS rank
              FROM retrieve_embedding e WHERE e.corpus = ${corpus} AND e.fingerprint = ${held.fingerprint} LIMIT ${bind.limit}`,
      }),
  },
} as const

declare namespace Search {
  type Lane = keyof typeof _lanes
  type Disposition = "ran" | "ungranted" | "unembedded" | "excluded"
}
```

## [05]-[FUSION_QUERY]

- Owner: `Search.of(corpus)` — the bound read family: `search` (the fused RRF statement plus optional rerank), `facets`, the snippet projection, the keyset cursor codec, and `ddl` from `[3]`; one request shape carries every modality.
- Packages: `effect` (`Effect`, `Option`, `HashMap`, `Record`, `Schema`); `@effect/sql` (the fused statement, the rerank-window body fetch, and the snippet fetch are each composed fragment values — `sql.in` set-shaped over the hit cells, never a per-hit query and never assembled text); `lane/capability.md` (`Capability` — the per-call grant read).
- Entry: `bound.search(request)` — text plus option-bag fields (`lanes`, `limit`, `k`, `cursor`, `facets`, `snippet`, `rerank`); the reply is one `Search.Page` carrying scored hits with snippets, facet counts when asked, the next cursor when more remains, and the lane census that actually ran.
- Receipt: `Search.Page.lanes` names each lane's disposition — a degraded scope is visible in every reply, so a relevance regression traces to capability, never to guesswork.
- Growth: rerank depth, fusion constant `k`, and snippet shape are request fields with policy defaults; a new reply projection (highlight offsets, per-lane scores) is a field on the page, never a second search.
- Law: fusion is in-database and fragment-composed — admitted lane fragments fold into the `WITH` roster and the `UNION ALL` pool by fragment interpolation, `Σ 1.0/(k + rank)` groups by cell, the keyset predicate arrives as a bound-value `HAVING` fragment when a cursor exists, and the statement is ONE round trip whose every parameter is value-bound; assembling lanes in process re-buys N queries and loses the shared plan, and hand-counted placeholder text is the deleted defect.
- Law: every reply row decodes — the fused rows, the snippet clips, the facet counts, and the rerank bodies each prove through a `Result` schema (`score` through the numeric-or-string codec because aggregate numerics arrive dialect-dependent), so no `String(row[...])` cast exists on the page.
- Law: the cursor is opaque and typed — `{ score, cell }` under one composed codec, `Schema.StringFromBase64Url` over `Schema.parseJson`, so encode and decode share the schema and a malformed caller cursor is `ParseError` on the admission rail; a raw offset is the rejected pagination, and the cursor mints from the FUSED order — rerank re-orders presentation inside the page and never moves the keyset coordinate, so a full-page rerank window cannot skip rows.
- Law: snippets ride the granted relevance lane AND its dialect — the `bm25` snippet function where the grant holds (its spelling travels with the `[3]` RESEARCH row), `ts_headline` as the in-core pg floor, the FTS5 `snippet()` arm serving the sqlite profiles through the same `onDialectOrElse` fold as the lane rows.
- Law: rerank is a window policy — when the `Reranker` is present and the request asks, the top `window` fused hits re-order by the port's verdict and the tail keeps fusion order; the candidate bodies arrive through one set-shaped fetch; absence of the port makes rerank a no-op through `serviceOption`, and a rerank fault degrades to fusion order through `Effect.option` — retrieval never fails on the accelerator.

```typescript
import { Array, Effect, HashMap, HashSet, Option, Record, Schema } from "effect"
import { SqlClient, SqlSchema, type SqlError } from "@effect/sql"
import { Capability } from "../lane/capability.ts"

declare namespace Search {
  type Corpus = { readonly table: Table; readonly dims: number }
  type Request = {
    readonly text: string
    readonly lanes?: ReadonlyArray<Lane>
    readonly limit?: number
    readonly k?: number
    readonly cursor?: string
    readonly facets?: ReadonlyArray<Table>
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

const _PAGE = { limit: 20, k: 60, rerank: 0, facetTop: 50 } as const

const _Cursor = Schema.compose(
  Schema.StringFromBase64Url,
  Schema.parseJson(Schema.Struct({ score: Schema.Number, cell: Schema.String })),
)

const _Score = Schema.Union(Schema.Number, Schema.NumberFromString)

const _HitRow = Schema.Struct({ cell: Schema.String, score: _Score })

const _fused = (
  sql: SqlClient.SqlClient,
  corpus: Search.Table,
  lanes: Array.NonEmptyReadonlyArray<Search.Lane>,
  bind: Search.Bind,
  k: number,
  cursor: Option.Option<{ readonly score: number; readonly cell: string }>,
  limit: number,
) => {
  const roster = Array.map(lanes, (lane) => sql`${sql(lane)} AS (${_lanes[lane].rank(sql, corpus, bind)})`)
  const pool = Array.map(lanes, (lane) => sql`SELECT cell, rank FROM ${sql(lane)}`)
  const ctes = Array.reduce(Array.tailNonEmpty(roster), Array.headNonEmpty(roster), (held, cte) => sql`${held}, ${cte}`)
  const union = Array.reduce(Array.tailNonEmpty(pool), Array.headNonEmpty(pool), (held, arm) => sql`${held} UNION ALL ${arm}`)
  const paging = Option.match(cursor, {
    onNone: () => sql``,
    onSome: (at) =>
      sql`HAVING sum(1.0 / (${k} + rank)) < ${at.score} OR (sum(1.0 / (${k} + rank)) = ${at.score} AND cell > ${at.cell})`,
  })
  return sql`WITH ${ctes}
    SELECT cell, sum(1.0 / (${k} + rank)) AS score FROM (${union}) pool
    GROUP BY cell ${paging}
    ORDER BY score DESC, cell LIMIT ${limit}`
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
const _ddl = (corpus: Search.Corpus, granted: HashSet.HashSet<string>) => {
  const vector = HashSet.has(granted, "vchord")
    ? Option.some(_indexRows.vectorChord)
    : HashSet.has(granted, "vector")
      ? Option.some(_indexRows.vectorHnsw)
      : Option.none<(typeof _indexRows)["vectorChord" | "vectorHnsw"]>()
  const admitted = [
    ...Option.toArray(vector),
    ...[_indexRows.trigram, _indexRows.keyset].filter((row) => row.grant === "core" || HashSet.has(granted, row.grant)),
  ]
  return {
    census: admitted.map((row) => row.lane),
    ensures: [
      ...(Option.isSome(vector) ? [_embeddingDdl(corpus.dims)] : []),
      ...admitted.map((row): Capability.Ensure => ({
        relation: corpus.table,
        pg: row.ensure(corpus.table),
        sqlite: "SELECT 1",
      })),
    ],
  }
}

const _Body = Schema.Struct({ cell: Schema.String, body: Schema.String })

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
            const bodies = SqlSchema.findAll({
              Request: Schema.Array(Schema.String),
              Result: _Body,
              execute: (cells) => sql`SELECT cell, body FROM ${sql(corpus.table)} WHERE ${sql.in("cell", cells)}`,
            })
            const candidates = yield* bodies(head.map((hit) => hit.cell))
            return yield* Array.isNonEmptyReadonlyArray(candidates)
              ? Effect.map(reranker.rerank(text, candidates), (order) => {
                  const byCell = HashMap.fromIterable(head.map((hit) => [hit.cell, hit] as const))
                  return [...Array.getSomes(order.map((cell) => HashMap.get(byCell, cell))), ...hits.slice(window)]
                }).pipe(Effect.option, Effect.map(Option.getOrElse(() => hits)))
              : Effect.succeed(hits)
          }),
      )).pipe(Effect.map(Option.getOrElse(() => hits))))

const _Clip = Schema.Struct({ cell: Schema.String, clip: Schema.String })

const _Facet = Schema.Struct({ value: Schema.String, count: _Score })

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
      const limit = request.limit ?? _PAGE.limit
      const bind: Search.Bind = {
        text: request.text,
        limit,
        vector: Option.map(embedded, (held) => ({
          literal: `[${held.vector.join(",")}]`,
          fingerprint: held.fingerprint,
        })),
      }
      const rows = yield* Array.isNonEmptyReadonlyArray(running)
        ? Effect.flatMap(
            _fused(sql, corpus.table, running, bind, request.k ?? _PAGE.k, cursor, limit + 1),
            Schema.decodeUnknown(Schema.Array(_HitRow)),
          )
        : Effect.succeed<ReadonlyArray<typeof _HitRow.Type>>([])
      const scored = rows.slice(0, limit).map((row) => ({
        cell: row.cell,
        score: row.score,
        snippet: Option.none<string>(),
      }))
      const clips = SqlSchema.findAll({
        Request: Schema.Array(Schema.String),
        Result: _Clip,
        execute: (cells) =>
          sql.onDialectOrElse({
            orElse: () =>
              sql`SELECT cell, snippet(${sql(`${corpus.table}_fts`)}, -1, '', '', '…', 12) AS clip
                  FROM ${sql(`${corpus.table}_fts`)} WHERE ${sql(`${corpus.table}_fts`)} MATCH ${request.text} AND ${sql.in("cell", cells)}`,
            pg: () =>
              sql`SELECT cell, ts_headline('simple', body, websearch_to_tsquery('simple', ${request.text})) AS clip
                  FROM ${sql(corpus.table)} WHERE ${sql.in("cell", cells)}`,
          }),
      })
      const clipped = request.snippet === true && scored.length > 0
        ? yield* Effect.map(clips(scored.map((hit) => hit.cell)), (found) => {
            const byCell = HashMap.fromIterable(found.map((row) => [row.cell, row.clip] as const))
            return scored.map((hit) => ({ ...hit, snippet: HashMap.get(byCell, hit.cell) }))
          })
        : scored
      const ordered = yield* _reranked(sql, corpus, request.text, request.rerank ?? _PAGE.rerank, clipped)
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
  (dims: ReadonlyArray<Search.Table>) =>
    Effect.flatMap(SqlClient.SqlClient, (sql) =>
      Effect.forEach(dims, (dim) =>
        Effect.map(
          SqlSchema.findAll({
            Request: Schema.String,
            Result: _Facet,
            execute: (facet) =>
              sql`SELECT ${sql(facet)} AS value, count(*) AS count FROM ${sql(corpus.table)} GROUP BY ${sql(facet)} ORDER BY count DESC LIMIT ${_PAGE.facetTop}`,
          })(dim),
          (rows) => rows.map((row) => ({ dim, value: row.value, count: row.count })),
        )).pipe(Effect.map(Array.flatten)))

const Search = {
  Fingerprint: _Fingerprint,
  Table: _Table,
  defaults: _PAGE,
  fingerprint: _fingerprint,
  Cursor: _Cursor,
  lanes: _lanes,
  indexes: _indexRows,
  embedding: _embeddingDdl,
  admitted: _admitted,
  fused: _fused,
  ddl: _ddl,
  of: (corpus: Search.Corpus) => ({
    ddl: (granted: HashSet.HashSet<string>) => _ddl(corpus, granted),
    search: _search(corpus),
    facets: _facets(corpus),
  }),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Embedder, EmbedFault, Reranker, Search }
```
