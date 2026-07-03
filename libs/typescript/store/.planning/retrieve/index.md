# [STORE_INDEX]

The index plane of retrieval: a closed row vocabulary where each row is `{ name, lane, grant, ensure }` — the retrieval lane it serves, the capability grant that admits it, and the ensure DDL that realizes it — plus the embedding table whose every vector row is keyed by an embedding fingerprint, so vectors minted by different models, dimensions, or revisions can never join in one distance scan. VectorChord is the stronger drop-in: the vector index row selects `vchordrq` when the `"vchord"` grant holds and pgvector's `hnsw` otherwise, from one row — an image upgrade re-indexes, it never rewrites a query. All extension index methods ride `[R11]` floors through the fail-closed probe; the in-core rows (trigram GIN, expression indexes, keyset support) carry no gate beyond their contrib grant.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                        |
| :-----: | :-------------- | :------------------------------------------------------------------------------ |
|  [01]   | `FINGERPRINT`   | the embedding-fingerprint brand and the vector table it keys                     |
|  [02]   | `INDEX_ROWS`    | the closed index-row vocabulary and the grant-gated ensure fold                  |

## [2]-[FINGERPRINT]

- Owner: the `Fingerprint` brand-in-field — `<model>:<dims>:<revision>` — and the `retrieve_embedding` ensure row whose primary key includes it; `Index.fingerprint` mints the value from its parts so the spelling exists once.
- Packages: `effect` (`Schema`).
- Growth: a model migration is a new fingerprint value — old vectors stay queryable under their fingerprint until re-embedding completes, and the swap is a predicate change in the lane SQL, never a table rebuild.
- Law: every vector write and every vector scan carries the fingerprint predicate — the column is in the primary key and the lane SQL filters on it, so cross-model distance comparison is unrepresentable, not discouraged.
- Law: the `Embedder` port (declared in `retrieve/hybrid.md`, satisfied by `ai/embed`) publishes the fingerprint of what it mints — the write path stamps `embedder.fingerprint`, the read path scans the same, one anchor.
- Law: dimensions live in the DDL as data — the `vector(n)` column width is generated from the fingerprint's dims at ensure time, and a dims change is a new fingerprint hence a new ensure row; pg refuses mixed widths structurally.

```typescript
import { Schema } from "effect"

const _Fingerprint = Schema.NonEmptyString.pipe(
  Schema.pattern(/^[a-z0-9._-]+:\d+:[a-z0-9._-]+$/),
  Schema.brand("Fingerprint"),
)

declare namespace Index {
  type Fingerprint = typeof _Fingerprint.Type
}

const _fingerprint = (model: string, dims: number, revision: string): Index.Fingerprint =>
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
```

## [3]-[INDEX_ROWS]

- Owner: the `_rows` anchor — one entry per index posture — and `Index.ensure`, the grant-gated fold that registers every admitted row for a corpus; the assembled `Index` export carrying rows, the fingerprint mint, and the ensure fold under one name.
- Packages: `capability/row.md` (`Capability.when` per row's grant); `@effect/sql` (`sql.unsafe` over the closed ensure literals).
- Entry: `retrieve/hybrid.md`'s corpus binding calls `Index.ensure(corpus, dims)` at scope construction — index presence is provisioned state under the DDL split exactly like tables; a refused grant skips its row and the corresponding search lane self-excludes through the same grant read.
- Receipt: the ensure fold returns the registered row names — the corpus's index census, joined with the capability report in the startup evidence.
- Growth: a new index posture (a second vector metric, a partial index) is one `_rows` entry; a new lane's support index is one row keyed by that lane — the fold and the vocabulary never widen.
- Law: the vector row is ONE row with a grant-ordered method — `vchordrq` under `"vchord"`, else `hnsw` under `"vector"` — the stronger drop-in is data, and `[R11]` floors gate both spellings through the matrix probes.
- Law: the embedding table rides the admitted vector row — `retrieve_embedding` materializes through the same fold only where a vector grant holds, dialect-split by `sql.onDialectOrElse`, so a scope without vectors carries no dead table and the index rows always find their relation.
- Law: `ensure` text is closed-vocabulary literal parameterized only by identifier-safe corpus names — `sql.unsafe` under the same provable-literal license the probes carry; corpus names are brand-validated identifiers, never caller strings.

```typescript
import { Array, Effect, Option } from "effect"
import { SqlClient } from "@effect/sql"
import { Capability } from "../capability/row.ts"

declare namespace Index {
  type Lane = "semantic" | "fts" | "trigram" | "keyset"
  type Row = {
    readonly name: string
    readonly lane: Lane
    readonly grant: string
    readonly ensure: (corpus: string, dims: number) => string
  }
}

const _rows = {
  vectorChord: {
    name: "vector_vchord",
    lane: "semantic",
    grant: "vchord",
    ensure: (corpus: string, _dims: number) =>
      `CREATE INDEX IF NOT EXISTS ${corpus}_embedding_vchord ON retrieve_embedding
       USING vchordrq (embedding vector_cosine_ops) WHERE corpus = '${corpus}';`,
  },
  vectorHnsw: {
    name: "vector_hnsw",
    lane: "semantic",
    grant: "vector",
    ensure: (corpus: string, _dims: number) =>
      `CREATE INDEX IF NOT EXISTS ${corpus}_embedding_hnsw ON retrieve_embedding
       USING hnsw (embedding vector_cosine_ops) WHERE corpus = '${corpus}';`,
  },
  search: {
    name: "search_bm25",
    lane: "fts",
    grant: "search",
    ensure: (corpus: string, _dims: number) =>
      `CREATE INDEX IF NOT EXISTS ${corpus}_bm25 ON ${corpus}
       USING bm25 (cell, body) WITH (key_field = 'cell');`,
  },
  trigram: {
    name: "trigram_gin",
    lane: "trigram",
    grant: "trigram",
    ensure: (corpus: string, _dims: number) =>
      `CREATE INDEX IF NOT EXISTS ${corpus}_trgm ON ${corpus} USING gin (body gin_trgm_ops);`,
  },
  keyset: {
    name: "keyset_cursor",
    lane: "keyset",
    grant: "core",
    ensure: (corpus: string, _dims: number) =>
      `CREATE INDEX IF NOT EXISTS ${corpus}_keyset ON ${corpus} (score DESC, cell);`,
  },
} as const

declare namespace Index {
  type Kind = keyof typeof _rows
  type _Rows<T extends Record<Kind, Row> = typeof _rows> = T
}

const _ensure = (corpus: string, dims: number) =>
  Effect.flatMap(Capability, (capability) =>
    Effect.flatMap(SqlClient.SqlClient, (sql) =>
      Effect.gen(function* () {
        const vector = yield* capability.when("vchord", Effect.succeed<Index.Row>(_rows.vectorChord)).pipe(
          Effect.flatMap(Option.match({
            onNone: () => capability.when("vector", Effect.succeed<Index.Row>(_rows.vectorHnsw)),
            onSome: (row) => Effect.succeed(Option.some(row)),
          })),
        )
        yield* Effect.when(
          sql.onDialectOrElse({
            orElse: () => sql.unsafe(_embeddingDdl(dims).sqlite),
            pg: () => sql.unsafe(_embeddingDdl(dims).pg),
          }),
          () => Option.isSome(vector),
        )
        const gated: ReadonlyArray<Index.Row> = [_rows.search, _rows.trigram, _rows.keyset]
        const admitted = [
          ...Option.toArray(vector),
          ...Array.getSomes(yield* Effect.forEach(gated, (row) =>
            row.grant === "core"
              ? Effect.succeed(Option.some(row))
              : capability.when(row.grant, Effect.succeed(row)))),
        ]
        yield* Effect.forEach(admitted, (row) => sql.unsafe(row.ensure(corpus, dims)), { discard: true })
        return admitted.map((row) => row.name)
      })))

const Index = {
  ..._rows,
  Fingerprint: _Fingerprint,
  fingerprint: _fingerprint,
  embedding: _embeddingDdl,
  ensure: _ensure,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Index }
```
