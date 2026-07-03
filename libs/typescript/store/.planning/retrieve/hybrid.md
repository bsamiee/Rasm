# [STORE_HYBRID]

Hybrid retrieval as one fused statement: five lanes — FTS, trigram, phonetic, fuzzy, semantic — each a row carrying its grant and its rank fragment, fused by reciprocal-rank fusion (`Σ 1/(k + rank)`) inside the database, so one round trip answers what five sequential searches would; the lane roster a query runs is the intersection of what it asks and what the scope's grants admit, computed per call from the capability report. The `Embedder` port is declared here and satisfied by `ai/embed` at the app root — its fingerprint keys every vector row so mixed embeddings never join — and the optional `Reranker` port re-scores the fused window when present. One `Search.of(corpus)` binding yields the whole read family: fused search, facet counts, snippets, and keyset-cursor pagination, every modality a field of one request shape.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                             |
| :-----: | :------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `PORTS`        | the `Embedder` port (fingerprint contract, `EmbedFault`) and the optional `Reranker`   |
|  [02]   | `LANE_ROSTER`  | the closed five-lane row table — grant, rank fragment, admission fold                  |
|  [03]   | `FUSION_QUERY` | `Search.of` — the RRF statement, rerank window, facet/snippet/cursor families          |

## [2]-[PORTS]

- Owner: the `Embedder` `Context.Tag` — embed-with-fingerprint, the one cross-folder retrieval contract — and the `Reranker` tag read through `Effect.serviceOption` so rerank is presence-typed, never a knob.
- Packages: `effect` (`Context`, `Data`); `retrieve/index.md` (`Index.Fingerprint`).
- Entry: `ai/embed` satisfies `Embedder` at app composition; nothing in `store` imports `ai` — the port is the whole seam, and a scope without an embedder simply has no semantic lane (the same degradation shape as a missing grant).
- Receipt: `embed` answers vectors paired with the port's own `fingerprint` — the caller stamps rows with it and scans by it; the fingerprint travels with the vectors because it IS their identity.
- Growth: a new embedding capability (batching, dimensions negotiation) is a member on this one port; a second embedding model in one app is a second Layer against the same tag selected per scope — never a second tag.
- Law: `EmbedFault` is the port's typed failure — reason-discriminated `budget | provider | shape` — and retrieval folds it into lane exclusion (a failed embed drops the semantic lane and reports it in the page's `lanes` census) rather than failing the whole search; text lanes still answer.

```typescript
import { Context, Data, Effect } from "effect"
import { Array } from "effect"
import type { Index } from "./index.ts"

class EmbedFault extends Data.TaggedError("EmbedFault")<{
  readonly reason: "budget" | "provider" | "shape"
  readonly detail: string
}> {}

class Embedder extends Context.Tag("store/Embedder")<Embedder, {
  readonly fingerprint: Index.Fingerprint
  readonly embed: (
    texts: Array.NonEmptyReadonlyArray<string>,
  ) => Effect.Effect<Array.NonEmptyReadonlyArray<ReadonlyArray<number>>, EmbedFault>
}>() {}

class Reranker extends Context.Tag("store/Reranker")<Reranker, {
  readonly rerank: (
    query: string,
    hits: Array.NonEmptyReadonlyArray<{ readonly cell: string; readonly body: string }>,
  ) => Effect.Effect<Array.NonEmptyReadonlyArray<string>, EmbedFault>
}>() {}
```

## [3]-[LANE_ROSTER]

- Owner: the `_lanes` anchor — five rows, each `{ grant, rank }` where `rank` builds the lane's scored CTE body over the corpus — and `_admitted`, the per-call fold intersecting the request's lanes with the scope's grants and the embedder's presence.
- Packages: `capability/row.md` (the grant vocabulary); `@effect/sql` (fragment composition).
- Growth: a sixth lane is one row — the fusion statement folds whatever the admission returns, so lanes are data end to end; a lane's SQL tuning (weights, operators) edits its row alone.
- Law: each lane emits the same shape — `(cell, rank)` with rank 1-based by lane-local score — because RRF consumes ranks, not scores; score normalization across heterogeneous lanes is exactly what the fusion deletes.
- Law: lane SQL rides its grant — `fts` under `"search"` (BM25 via pg_search, `[R11]`), `trigram` under `"trigram"` (`similarity()` ordering), `phonetic` and `fuzzy` under `"phonetic"`/`"fuzzy"` (`soundex()` equality, `levenshtein()` distance — both fuzzystrmatch), `semantic` under `"vector"` plus a present embedder (pgvector cosine `<=>`, `[R11]`); the sqlite lane substitutes its FTS5 arm through the degradation row and drops what it cannot express.
- Law: the corpus contract is one relation with `cell` (the stable key), `body` (the searchable text), and score-bearing columns the keyset index covers — `Search.of` takes the corpus value; a second searchable relation is a second binding, never a wider query.

```typescript
declare namespace Search {
  type Lane = "fts" | "trigram" | "phonetic" | "fuzzy" | "semantic"
  type Corpus = {
    readonly table: string
    readonly dims: number
  }
}

const _lanes = {
  fts: {
    grant: "search",
    rank: (corpus: string) =>
      `SELECT cell, rank() OVER (ORDER BY paradedb.score(cell) DESC) AS rank
       FROM ${corpus} WHERE body @@@ $1 LIMIT $2`,
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
  type _Rows<T extends Record<Lane, { readonly grant: string; readonly rank: (corpus: string) => string }> = typeof _lanes> = T
}
```

## [4]-[FUSION_QUERY]

- Owner: `Search.of(corpus)` — the bound read family: `search` (the fused RRF statement plus optional rerank), `facets` (dimension counts), the snippet projection riding the hit rows, and the keyset cursor codec; one request shape carries every modality.
- Packages: `effect` (`Effect`, `Option`, `Encoding`, `Schema`); `@effect/sql` (`SqlSchema.findAll` for the fused decode); `capability/row.md` (`Capability`).
- Entry: `bound.search(request)` — text plus option-bag fields (`lanes`, `limit`, `k`, `cursor`, `facets`, `snippet`); the reply is one `Search.Page` carrying hits with scores and snippets, facet counts when asked, the next cursor when more remains, and the lane census that actually ran — evidence, not mystery.
- Receipt: `Search.Page.lanes` names each lane's disposition (`ran | ungranted | unembedded | excluded`) — a degraded scope is visible in every reply, so relevance regressions trace to capability, not to guesswork.
- Growth: rerank depth, fusion constant `k`, and snippet shape are request fields with policy defaults; a new reply projection (highlight offsets, per-lane scores) is a field on the page, never a second search.
- Law: fusion is in-database — lanes materialize as CTEs, `Σ 1.0/(k + rank)` groups by cell, order-by-score-desc with the keyset predicate `(score, cell) < (cursor.score, cursor.cell)` pages stably, and the statement is ONE round trip; assembling lanes in process re-buys N queries and loses the shared plan.
- Law: the cursor is opaque and typed — `{ score, cell }` encoded `Schema.parseJson` then `Encoding.encodeBase64Url`, decoded by the same codec; a raw offset is the rejected pagination.
- Law: snippets ride the granted text lane — `paradedb.snippet` under `"search"`, `ts_headline` as the in-core fallback — selected by the same grant read as the lane itself `[R11]`.
- Law: rerank is a window policy — when the `Reranker` is present and the request asks, the top `window` fused hits re-order by the port's verdict and the tail keeps fusion order; absence of the port makes rerank a no-op by `serviceOption`, never an error.

```typescript
import { Array, Effect, Encoding, HashSet, Option, Schema, type ParseResult } from "effect"
import { SqlClient, type SqlError, type Statement } from "@effect/sql"
import { Capability } from "../capability/row.ts"
import { Index } from "./index.ts"

declare namespace Search {
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
  type Hit = {
    readonly cell: string
    readonly score: number
    readonly snippet: Option.Option<string>
  }
  type Page = {
    readonly hits: ReadonlyArray<Hit>
    readonly facets: ReadonlyArray<{ readonly dim: string; readonly value: string; readonly count: number }>
    readonly cursor: Option.Option<string>
    readonly lanes: Record<Lane, "ran" | "ungranted" | "unembedded" | "excluded">
  }
  type Bound = {
    readonly search: (request: Request) => Effect.Effect<
      Page,
      SqlError.SqlError | ParseResult.ParseError,
      SqlClient.SqlClient | Capability
    >
    readonly ensure: Effect.Effect<ReadonlyArray<string>, SqlError.SqlError | Capability.Fault, SqlClient.SqlClient | Capability>
  }
}

const _Cursor = Schema.parseJson(Schema.Struct({ score: Schema.Number, cell: Schema.String }))

const _cursor = {
  encode: (score: number, cell: string) =>
    Effect.map(Schema.encode(_Cursor)({ score, cell }), Encoding.encodeBase64Url),
  decode: (raw: string) =>
    Effect.flatMap(
      Effect.orDie(Encoding.decodeBase64UrlString(raw)),
      Schema.decodeUnknown(_Cursor),
    ),
}

const _fused = (corpus: string, lanes: ReadonlyArray<Search.Lane>, k: number): string =>
  [
    "WITH",
    lanes.map((lane) => `${lane} AS (${_lanes[lane].rank(corpus)})`).join(", "),
    `SELECT cell, sum(1.0 / (${k} + rank)) AS score FROM (`,
    lanes.map((lane) => `SELECT cell, rank FROM ${lane}`).join(" UNION ALL "),
    ") pool GROUP BY cell ORDER BY score DESC",
  ].join(" ")

const _reranked = (
  sql: SqlClient.SqlClient,
  corpus: string,
  request: Search.Request,
  hits: ReadonlyArray<Search.Hit>,
): Effect.Effect<ReadonlyArray<Search.Hit>, SqlError.SqlError> =>
  Effect.flatMap(Effect.serviceOption(Reranker), (reranker) => {
    const window = Math.min(request.rerank ?? 0, hits.length)
    if (Option.isNone(reranker) || window === 0) return Effect.succeed(hits)
    return Effect.gen(function* () {
      const head = hits.slice(0, window)
      const rows = yield* sql`SELECT cell, body FROM ${sql(corpus)}
        WHERE ${sql.in("cell", head.map((hit) => hit.cell))}`
      const bodies = new Map(rows.map((row) => [String(row.cell), String(row.body)]))
      const candidates = head.map((hit) => ({ cell: hit.cell, body: bodies.get(hit.cell) ?? "" }))
      if (!Array.isNonEmptyReadonlyArray(candidates)) return hits
      const verdict = yield* Effect.option(reranker.value.rerank(request.text, candidates))
      return Option.match(verdict, {
        onNone: () => hits,
        onSome: (order) => {
          const byCell = new Map(head.map((hit) => [hit.cell, hit]))
          const led = order.flatMap((cell) => (byCell.has(cell) ? [byCell.get(cell)!] : []))
          return [...led, ...hits.slice(window)]
        },
      })
    })
  })

const _snippets = (
  sql: SqlClient.SqlClient,
  corpus: string,
  text: string,
  hits: ReadonlyArray<Search.Hit>,
): Effect.Effect<ReadonlyArray<Search.Hit>, SqlError.SqlError> =>
  Effect.map(
    sql`SELECT cell, ts_headline('simple', body, websearch_to_tsquery('simple', ${text})) AS clip
        FROM ${sql(corpus)} WHERE ${sql.in("cell", hits.map((hit) => hit.cell))}`,
    (rows) => {
      const clips = new Map(rows.map((row) => [String(row.cell), String(row.clip)]))
      return hits.map((hit) => ({ ...hit, snippet: Option.fromNullable(clips.get(hit.cell)) }))
    },
  )

const _facets = (
  sql: SqlClient.SqlClient,
  corpus: string,
  dims: ReadonlyArray<string>,
  cells: ReadonlyArray<string>,
): Effect.Effect<ReadonlyArray<{ readonly dim: string; readonly value: string; readonly count: number }>, SqlError.SqlError> =>
  Effect.map(
    Effect.forEach(dims, (dim) =>
      Effect.map(
        sql`SELECT ${sql(dim)} AS value, count(*) AS count FROM ${sql(corpus)}
            WHERE ${sql.in("cell", cells)} GROUP BY ${sql(dim)} ORDER BY count DESC`,
        (rows) => rows.map((row) => ({ dim, value: String(row.value), count: Number(row.count) })),
      )),
    (groups) => groups.flat(),
  )

const Search = {
  of: (corpus: Search.Corpus): Search.Bound => ({
    ensure: Effect.suspend(() => Index.ensure(corpus.table, corpus.dims)),
    search: (request) =>
      Effect.gen(function* () {
        const sql = yield* SqlClient.SqlClient
        const capability = yield* Capability
        const embedder = yield* Effect.serviceOption(Embedder)
        const asked = request.lanes ?? ["fts", "trigram", "semantic"]
        const granted = asked.filter((lane) => HashSet.has(capability.granted, _lanes[lane].grant))
        const vector = yield* Option.match(embedder, {
          onNone: () => Effect.succeed(Option.none<ReadonlyArray<number>>()),
          onSome: (port) =>
            granted.includes("semantic")
              ? Effect.option(Effect.map(port.embed([request.text]), (rows) => rows[0]))
              : Effect.succeed(Option.none<ReadonlyArray<number>>()),
        })
        const ran = Option.isSome(vector) ? granted : granted.filter((lane) => lane !== "semantic")
        const held = request.cursor === undefined
          ? Option.none<{ readonly score: number; readonly cell: string }>()
          : Option.some(yield* _cursor.decode(request.cursor))
        const limit = request.limit ?? 20
        const params: ReadonlyArray<Statement.Primitive> = Option.match(vector, {
          onNone: () => [request.text, limit * 3],
          onSome: (embedding) => [
            request.text,
            limit * 3,
            JSON.stringify(embedding),
            Option.match(embedder, { onNone: () => "", onSome: (port) => port.fingerprint }),
          ],
        })
        const pool = yield* sql.unsafe(_fused(corpus.table, ran, request.k ?? 60), params)
        const paged = pool
          .map((row) => ({ cell: String(row.cell), score: Number(row.score), snippet: Option.none<string>() }))
          .filter((hit) =>
            Option.match(held, {
              onNone: () => true,
              onSome: (cursor) => hit.score < cursor.score || (hit.score === cursor.score && hit.cell > cursor.cell),
            }))
          .slice(0, limit)
        const ordered = yield* _reranked(sql, corpus.table, request, paged)
        const clipped = request.snippet === true && ordered.length > 0
          ? yield* _snippets(sql, corpus.table, request.text, ordered)
          : ordered
        const facets = request.facets === undefined || clipped.length === 0
          ? []
          : yield* _facets(sql, corpus.table, request.facets, clipped.map((hit) => hit.cell))
        return {
          hits: clipped,
          facets,
          cursor: clipped.length === limit
            ? Option.some(yield* _cursor.encode(clipped.at(-1)!.score, clipped.at(-1)!.cell))
            : Option.none(),
          lanes: Object.fromEntries(
            (["fts", "trigram", "phonetic", "fuzzy", "semantic"] as const).map((lane) => [
              lane,
              ran.includes(lane)
                ? "ran"
                : !asked.includes(lane)
                  ? "excluded"
                  : lane === "semantic" && granted.includes("semantic")
                    ? "unembedded"
                    : "ungranted",
            ]),
          ) as Search.Page["lanes"],
        } satisfies Search.Page
      }),
  }),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Embedder, Search }
```
