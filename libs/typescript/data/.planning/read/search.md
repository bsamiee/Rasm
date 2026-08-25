# [DATA_SEARCH]

Retrieval is one bound owner: five data-driven lanes — FTS, trigram, phonetic, fuzzy, semantic — emit ranked rows and fuse through reciprocal-rank fusion inside one database statement. Lane admission intersects request intent with scope grants, declared corpus geometry, and the embedding the semantic seam proves, reporting every exclusion; `Search.Corpus` composes `Search.Embedding` with its distinct relation coordinate. `Search.of(corpus)` binds fused search, facets, snippets, keyset cursor, scoped filters, and the provision roster once — the runtime never executes schema statements.

## [01]-[INDEX]

- [02]-[PORTS]: `Embedder` port and the optional `Reranker` — fingerprint contract, `EmbedFault`.
- [03]-[INDEX_PLANE]: `Search.Embedding`, `Search.Corpus`, the embedding relation, and index rows.
- [04]-[LANE_ROSTER]: `_lanes` and `_admitted` — the closed five-lane row table: grants, floor, geometry, rank fragment, per-call admission.
- [05]-[FUSION_QUERY]: `Search.of` — the RRF statement, rerank admission, facet/snippet/cursor families.

## [02]-[PORTS]

- Owner: the `Embedder` `Context.Tag` — embed-with-fingerprint, the one cross-folder retrieval contract — and the `Reranker` tag read through `Effect.serviceOption` so rerank is presence-typed, never a knob.
- Packages: `effect` (`Context`, `Schema`, `Array`); `@rasm/core` (`Fault.Class`).
- Entry: the runtime branch's embedding rows satisfy `Embedder` at app composition; nothing in this folder imports a provider — the port is the whole seam, and a scope without an embedder has no semantic lane, the same degradation shape as a missing grant.
- Receipt: singular `embed(text)` answers one vector under the port's own `fingerprint` — the satisfying Layer batches calls through `Batch.Engine`, the consuming seam proves both `vector.length === corpus.embedding.dims` and `port.fingerprint === corpus.embedding.fingerprint`, and only then can the semantic lane run.
- Growth: an embedding capability axis (dimension negotiation, batch policy) is a member on this one port; a second model in one app is a second Layer against the same tag selected per scope, never a second tag.
- Law: `EmbedFault` closes through `Fault.Class.family`; a failed embed excludes only the semantic lane before census settlement; each reason declares its own subject and renders its own sentence, so the raise carries ONE `case` payload and the shape disagreement crosses as its four coordinates rather than as a built string.
- Law: recovery policy reads the core lattice off `class` — `budget` classifies `exhausted` and `provider` `unavailable` (both system-blamed and retryable, so a satisfying Layer's own schedule re-drives them), `refused` classifies `denied` because a moderation verdict is settled and a re-drive presents the identical material to the screen that already answered, `malformed` classifies `malformed` because a request the provider rejects and a response this client cannot decode both re-drive identically, and `shape` classifies `invalid` because a vector disagreeing with the corpus dimension or fingerprint is quarantined evidence a re-drive cannot fix — so retryability, blame, and quarantine derive from the core row table and no local rank or retry column rides beside `class`.
- Law: the wire-and-screen reasons carry the port that answered beside the coordinate it answered about — `embed` names its fingerprint, `rerank` names its query — because only one of the two ports this family serves holds an embedding identity at all, and a fingerprint column would make the other forge one.
- Law: a verdict never borrows the transport cell — `provider` carries what the wire did and `refused` what a screen decided — and the reply CONSUMES that split off `class`: a `denied`-classed embed excludes the semantic lane as the census's own `denied` disposition where a transient fault reads `unembedded`, and a `denied`-classed rerank reports `denied` where a wire fault reads `degraded`, so the settled-verdict/retryable split is a reply fact the operator and the retry rail both read rather than a family row standing beside the fences.
- Law: the `Reranker` answer is provider material, never trusted order — the port's declared type admits duplicates, unknown cells, and omissions, so the consuming seam (`[4]`'s rerank admission) proves the answer against its own candidate window and no port value can change hit cardinality; the port stays thin because the evidence lives at the seam that holds the candidates.
- Law: the port's provider side batches through `read/batch.md`'s engine — the window geometry is the satisfying Layer's concern; this port declares only the vector contract.

```typescript signature
import { Array, Context, Effect, Schema } from "effect"
import { Fault } from "@rasm/core"

const _PORTS = ["embed", "rerank"] as const
const _Refusal = Schema.Struct({
  port: Schema.Literal(..._PORTS),
  subject: Schema.NonEmptyString,
  detail: Schema.String,
})

const _family = Fault.Class.family(["budget", "provider", "malformed", "refused", "shape"] as const, {
  budget: Fault.Class.row({
    class: "exhausted",
    leg: "port",
    detail: _Refusal,
    render: ({ detail, port, subject }) => `<${port}:${subject}> spent its quota — ${detail}`,
  }),
  provider: Fault.Class.row({
    class: "unavailable",
    leg: "port",
    detail: _Refusal,
    render: ({ detail, port, subject }) => `<${port}:${subject}> refused at the wire — ${detail}`,
  }),
  malformed: Fault.Class.row({
    class: "malformed",
    leg: "port",
    detail: _Refusal,
    render: ({ detail, port, subject }) => `<${port}:${subject}> exchanged material no schema admits — ${detail}`,
  }),
  refused: Fault.Class.row({
    class: "denied",
    leg: "port",
    detail: _Refusal,
    render: ({ detail, port, subject }) => `<${port}:${subject}> screened the material and refused — ${detail}`,
  }),
  shape: Fault.Class.row({
    class: "invalid",
    leg: "port",
    detail: Schema.Struct({
      expected: Schema.NonEmptyString,
      dims: Schema.Int.pipe(Schema.positive()),
      fingerprint: Schema.NonEmptyString,
      length: Schema.Int.pipe(Schema.nonNegative()),
    }),
    render: ({ expected, dims, fingerprint, length }) =>
      `corpus admits ${expected} at ${dims} dimensions, port answered ${fingerprint} at ${length}`,
  }),
})

class EmbedFault extends Schema.TaggedError<EmbedFault>()("EmbedFault", {
  case: _family.payload,
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.case.reason)
  }
  get leg(): string {
    return _family.legOf(this.case.reason)
  }
  override get message(): string {
    return _family.render(this.case)
  }
}

declare namespace EmbedFault {
  type Issue = typeof _family.payload.Type
  type Port = (typeof _PORTS)[number]
  type Reason = (typeof _family.kinds)[number]
}

class Embedder extends Context.Tag("data/Embedder")<Embedder, {
  readonly fingerprint: Search.Fingerprint
  readonly embed: (text: string) => Effect.Effect<ReadonlyArray<number>, EmbedFault>
}>() {}

class _RerankCandidate extends Schema.Class<_RerankCandidate>("Reranker.Candidate")({
  cell: Schema.NonEmptyString,
  body: Schema.String,
}) {}

class Reranker extends Context.Tag("data/Reranker")<Reranker, {
  readonly rerank: (
    query: string,
    hits: Array.NonEmptyReadonlyArray<_RerankCandidate>,
  ) => Effect.Effect<Array.NonEmptyReadonlyArray<string>, EmbedFault>
}>() {
  static readonly Candidate = _RerankCandidate
}
```

## [03]-[INDEX_PLANE]

- Owner: `Search.Embedding` — model, dimensions, revision, and derived fingerprint under the provider identity regime — and `Search.Corpus` — the distinct relation identity composed with one embedding value and the declared body geometry — with the `retrieve_embedding` ensure whose primary key includes that fingerprint and the dialect-paired index rows; vector method selection is grant-ordered data, never a query rewrite, and the runtime never executes a DDL statement.
- Packages: `effect` (`Schema`, `Array`, `HashSet`, `Option`); `read/query.md` (`Query.Relation` — the identifier lexical class and relation owner); `lane/capability.md` (`Capability.Ensure` — the shape the provision plane applies and the rail proves); `lane/postgres.md` (the `vchord`, `vector`, `trigram`, `phonetic`, and `fuzzy` grants arrive as the granted set).
- Entry: `Search.ddl(corpus, granted)` derives the corpus's ensure roster — the embedding relation and the admitted index rows, each row's dialect pair landing as one `Capability.Ensure` — which the provision plane applies and `lane/tenant.md`'s roster collection proves at scope construction; an absent ACCELERATOR degrades scan speed, never correctness, while the FTS floor row is relation-bearing on the sqlite profiles (the `MATCH` arm queries a virtual table) and therefore always in the roster.
- Receipt: the derivation returns the admitted artifact names — the corpus's index census, joined with the capability report in startup evidence.
- Growth: a model change is a new fingerprint value — the corpus rebuilds under it while the superseded fingerprint's rows stay queryable until re-embedding completes and drops them; a new index posture (second metric, partial index) is one row; dims live in the DDL as data, so a dims change is a new fingerprint hence a new ensure, and mixed widths are refused by the engine.
- Law: fingerprint identity derives from `Search.Embedding` — pattern-refined `model` and `revision` with bounded `dims` assemble under `Schema.decodeSync`, total because the field refinements close the composite pattern; `Search.Corpus` embeds that owner beside its relation field because provider identity and relation identity are distinct discriminants, while loose part brands and parallel DTOs remain unspellable.
- Law: the corpus coordinate is layered evidence — `Search.Corpus.fields.table` derives from `Query.Relation.fields.table` and adds the corpus role, so a caller-derived string can never reach an identifier position, ensure texts interpolate only sealed names, and facet dimensions derive from `Query.Relation.fields.table` because the identifier law admits no second lexical class.
- Law: every vector write and scan carries the fingerprint predicate — the column sits in the primary key and the semantic lane filters on it, so cross-model distance comparison is unrepresentable.
- Law: the corpus declares its body geometry — `document` for prose, `label` for short identifying strings — because whole-string scorers read the leading characters of `body` and a corpus fusing their output over prose banks noise as rank evidence; the declaration is the corpus's own fact, so `[4]` refuses a mis-shaped lane by row rather than by a caller remembering which lanes suit its data.
- Law: SPIKE — the geometry roster converges on its first consumer, and the deterministic floor ships whole beneath it: `document` and `label` are the two geometries every `[4]` row votes against, an unreachable pairing reports `unshaped`, and a corpus whose geometry no row names draws no admission answer. No scope in this branch binds a corpus, so disk carries no construction site declaring a `shape`; the owning scope that lands one WIDENS the roster with the geometry its body carries, and narrowing this field onto one consumer's body is the refused move.
- Law: a grant token names a CAPABILITY, never an install — `lane/postgres.md` declares one `fuzzystrmatch` row supplying `phonetic` and `fuzzy`, so the pair admits and refuses together off one provisioned extension while staying two lane grants; the provision plane counts extension rows and this page counts scorers, so collapsing the pair here forks the grant alphabet that roster owns and merging the two scorers erases an equality lane and a distance lane into one.
- Law: the vector row is ONE row with a grant-ordered method — `vchordrq` under `vchord`, else `hnsw` under `vector` — the stronger engine is data and an image upgrade re-indexes without touching a query.
- Law: every index row states both dialects — the pg text and the sqlite text are one `ensure` pair, `SELECT 1` only where the lane genuinely cannot exist on the profile (vector and trigram ride pg-only grants), and the FTS floor row's sqlite arm is the external-content FTS5 virtual table with three sync triggers and the idempotent `rebuild` command that admits pre-existing corpus rows, so the lane arm and its storage artifact are one row and cannot drift.

```typescript signature
import { Schema } from "effect"
import { Query } from "./query.ts"

const _Model = Schema.NonEmptyString.pipe(Schema.pattern(/^[a-z0-9._-]+$/), Schema.brand("EmbedModel"))

const _Dims = Schema.Int.pipe(Schema.between(1, 16000), Schema.brand("EmbedDims"))

const _Revision = Schema.NonEmptyString.pipe(Schema.pattern(/^[a-z0-9._-]+$/), Schema.brand("EmbedRevision"))

const _Fingerprint = Schema.NonEmptyString.pipe(
  Schema.pattern(/^[a-z0-9._-]+:\d+:[a-z0-9._-]+$/),
  Schema.brand("Fingerprint"),
)

class _Embedding extends Schema.Class<_Embedding>("Search.Embedding")({
  model: _Model,
  dims: _Dims,
  revision: _Revision,
}) {
  get fingerprint(): typeof _Fingerprint.Type {
    return Schema.decodeSync(_Fingerprint)(`${this.model}:${this.dims}:${this.revision}`)
  }
}

const _Table = Query.Relation.fields.table.pipe(Schema.brand("Corpus"))

const _SHAPES = ["document", "label"] as const

class _Corpus extends Schema.Class<_Corpus>("Search.Corpus")({
  table: _Table,
  shape: Schema.Literal(..._SHAPES),
  embedding: _Embedding,
}) {}

declare namespace Search {
  type Corpus = _Corpus
  type Shape = (typeof _SHAPES)[number]
  type Embedding = _Embedding
  type Model = Embedding["model"]
  type Dims = Embedding["dims"]
  type Revision = Embedding["revision"]
  type Fingerprint = Embedding["fingerprint"]
  type Table = Corpus["table"]
}

const _fingerprint = (embedding: Search.Embedding): Search.Fingerprint => embedding.fingerprint

const _embeddingDdl = (dims: Search.Dims): Capability.Ensure => ({
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
  fts: {
    artifact: "fts",
    grant: "core",
    ensure: (corpus: Search.Table) => ({
      pg: `CREATE INDEX IF NOT EXISTS ${corpus}_tsv ON ${corpus} USING gin (to_tsvector('simple', body));`,
      sqlite: `CREATE VIRTUAL TABLE IF NOT EXISTS ${corpus}_fts USING fts5(cell UNINDEXED, body, content='${corpus}', content_rowid='rowid');
CREATE TRIGGER IF NOT EXISTS ${corpus}_fts_ai AFTER INSERT ON ${corpus} BEGIN
  INSERT INTO ${corpus}_fts(rowid, cell, body) VALUES (new.rowid, new.cell, new.body); END;
CREATE TRIGGER IF NOT EXISTS ${corpus}_fts_ad AFTER DELETE ON ${corpus} BEGIN
  INSERT INTO ${corpus}_fts(${corpus}_fts, rowid, cell, body) VALUES ('delete', old.rowid, old.cell, old.body); END;
CREATE TRIGGER IF NOT EXISTS ${corpus}_fts_au AFTER UPDATE OF cell, body ON ${corpus} BEGIN
  INSERT INTO ${corpus}_fts(${corpus}_fts, rowid, cell, body) VALUES ('delete', old.rowid, old.cell, old.body);
  INSERT INTO ${corpus}_fts(rowid, cell, body) VALUES (new.rowid, new.cell, new.body); END;
INSERT INTO ${corpus}_fts(${corpus}_fts) VALUES ('rebuild');`,
    }),
  },
  vectorChord: {
    artifact: "vectorChord",
    grant: "vchord",
    ensure: (corpus: Search.Table) => ({
      pg: `CREATE INDEX IF NOT EXISTS ${corpus}_embedding_vchord ON retrieve_embedding
       USING vchordrq (embedding vector_cosine_ops) WHERE corpus = '${corpus}';`,
      sqlite: "SELECT 1",
    }),
  },
  vectorHnsw: {
    artifact: "vectorHnsw",
    grant: "vector",
    ensure: (corpus: Search.Table) => ({
      pg: `CREATE INDEX IF NOT EXISTS ${corpus}_embedding_hnsw ON retrieve_embedding
       USING hnsw (embedding vector_cosine_ops) WHERE corpus = '${corpus}';`,
      sqlite: "SELECT 1",
    }),
  },
  trigram: {
    artifact: "trigram",
    grant: "trigram",
    ensure: (corpus: Search.Table) => ({
      pg: `CREATE INDEX IF NOT EXISTS ${corpus}_trgm ON ${corpus} USING gin (body gin_trgm_ops);`,
      sqlite: "SELECT 1",
    }),
  },
  phonetic: {
    artifact: "phonetic",
    grant: "phonetic",
    ensure: (corpus: Search.Table) => ({
      pg: `CREATE INDEX IF NOT EXISTS ${corpus}_phonetic ON ${corpus}
       USING gin (daitch_mokotoff(body)) WITH (fastupdate = off);`,
      sqlite: "SELECT 1",
    }),
  },
  keyset: {
    artifact: "keyset",
    grant: "core",
    ensure: (corpus: Search.Table) => ({
      pg: `CREATE INDEX IF NOT EXISTS ${corpus}_keyset ON ${corpus} (cell);`,
      sqlite: `CREATE INDEX IF NOT EXISTS ${corpus}_keyset ON ${corpus} (cell);`,
    }),
  },
} as const satisfies {
  readonly [row: string]: {
    readonly artifact: string
    readonly grant: string
    readonly ensure: (corpus: Search.Table) => { readonly pg: string; readonly sqlite: string }
  }
}

declare namespace Search {
  type IndexKind = keyof typeof _indexRows
}
```

## [04]-[LANE_ROSTER]

- Owner: the `_lanes` anchor — five rows, each `{ grants, floor, material, shapes, rank }` where `rank` builds the lane's scored CTE body as a composed `sql` fragment over a typed bind value — and `_admitted`, the per-call fold intersecting the request's lanes with the scope's grants, floor postures, required material, and the corpus's declared geometry.
- Packages: composition over `[2]`/`[3]` values and the granted set; `@effect/sql` (the `sql` fragment constructor and `sql.and` — every parameter binds by value inside the fragment, so no positional index exists anywhere on the page); `effect` (`Array`, `HashSet`, `Option`, `Record`).
- Growth: a sixth lane is one row — the fusion statement folds whatever the admission returns; a lane's SQL tuning edits its row alone; a new bind axis is a `Search.Bind` field every row can read.
- Law: lane rows are fragment builders, never SQL text — `rank(sql, corpus, bind)` interpolates `bind.text`/`bind.limit`/the vector literal as BOUND parameters and the corpus only through the brand-proven identifier or a value predicate, so a lane-set change cannot misalign parameters (there are none to count) and the statement stays typed, batched, and dialect-switched; hand-assembled `$N` text is the deleted spelling.
- Law: every lane emits one shape — `(cell, rank)` with rank 1-based by lane-local score — because RRF consumes ranks, never scores; score normalization across heterogeneous lanes is exactly what the fusion deletes.
- Law: the floor is a row column, never a special case — `floor: true` marks a lane that runs regardless of its accelerator grant (`fts` degrades to `ts_rank_cd` over `websearch_to_tsquery` on the spine and to the FTS5 `MATCH` arm on the sqlite profiles, both against `[3]`'s provisioned floor artifacts), and `_admitted` reads the column, so the census logic enumerates no lane by name and a future floored lane is one row fact.
- Law: lane SQL rides its grant set AND its dialect — `fts` is the core `tsvector`/FTS5 floor, `trigram` rides `similarity()`, `phonetic` rides `daitch_mokotoff()` array overlap, `fuzzy` rides ceiling-bounded `levenshtein_less_equal()`, and `semantic` accepts the `vector` contract from either vector engine beside an admitted embedding; a lane the profile cannot express self-excludes through its grant set, so degradation is the fence, not prose.
- Law: the phonetic scorer is `daitch_mokotoff`, never `soundex` — `soundex` reads bytes, so it answers one code for `Ozturk` and another for `Öztürk`, and it answers the EMPTY string for every CJK, Arabic, and Cyrillic label, which makes `soundex(a) = soundex(b)` true across an entire non-Latin corpus and floods the fused pool with the whole relation; `daitch_mokotoff` codes those pairs identically, answers NULL where it can code nothing at all, and its `text[]` result carries the GIN artifact `[3]` plants, so the lane gains an index where the equality form had none.
- Law: the phonetic lane emits BOOLEAN evidence and orders by `cell` — a code overlap is a match or nothing, so no score exists to rank on and the stable key order is the honest spelling; what the lane forfeits is intra-lane discrimination, and RRF weights its hits by arrival position alone, which is why it stays off the `document` geometry where the candidate set is unbounded.
- Law: the fuzzy lane bounds the DISTANCE, never the string alone — `levenshtein_less_equal` short-circuits at the request's `distance` ceiling and answers `ceiling + 1` past it, so the same call serves the predicate and the order while a row beyond the budget costs a truncated matrix instead of a full one; a predicate-free spelling ranks the whole corpus by edit distance and hands RRF `limit` rows of pure noise. `fuzzystrmatch` publishes no index operator class, so this lane is a bounded scan by construction and the `_WINDOW` prefix and the ceiling are its whole budget.
- Law: the scope predicate is one composed fragment — `Search.Filter` is one schema-tagged family over equality, inequality, bounds, ranges, and set membership; `_scoped` exhaustively maps its cases into one `sql.and` fragment (or the neutral `1 = 1`), every lane splices it, and the semantic lane joins the corpus relation to apply it — so a filtered search filters EVERY lane before fusion and a hit outside the scope cannot enter the pool through any arm.
- Law: the corpus contract is one relation with stable `cell`, searchable `body`, and admitted facet columns — `score` is a fused-query projection and never a corpus column, so the keyset support row indexes the stable `cell` tie-breaker only; `Search.of` takes the corpus value, and a second searchable relation is a second binding.
- Law: admission is evidence and row-driven — each requested lane resolves to `ran`, `ungranted`, `unembedded`, `denied`, `unshaped`, or `excluded` from its `floor`, `material`, and `shapes` columns beside the embed outcome; no lane name appears in the admission fold, and the output is both CTE roster and reply census.
- Law: the embed outcome crosses as the three-valued `Search.Embed` evidence, never a boolean — `held` proves the vector, `denied` carries a `denied`-classed screen verdict (settled: a re-request presents identical material to the screen that already answered), and `absent` covers no port, no request, and every retryable fault the satisfying Layer's own schedule re-drives — so the census tells a moderation refusal from a missing capability without re-deriving the core lattice.
- Law: each lane answers the coordinates it decides and declares the rest unowned — `grants`, `material`, and `shapes` together ARE the selection sentence; `material` is also the admit column, naming the write that fills the lane (`text` the corpus body itself, `embedding` a vector row minted under the corpus fingerprint); lifetime belongs to the corpus row for every text lane and to the FINGERPRINT for the semantic one, where superseded vectors stay queryable under their own until re-embedding completes. Tenancy is not this table's to decide — `_scoped` splices the caller's one fragment into every lane identically, so a tenancy column here forks the predicate `lane/tenant.md` owns; what each lane forfeits rides `floor` and the `ungranted`, `unembedded`, `denied`, and `unshaped` dispositions, so a degraded lane names its own loss in the reply.
- Law: geometry gates ahead of grants — a lane whose scorer cannot read the corpus's body geometry reports `unshaped` even where its extension is installed, because a granted lane contributing noise ranks that noise into the fused pool while an absent lane costs only recall; `phonetic` and `fuzzy` score `label` corpora alone, the inverted, trigram, and vector lanes score both, and a lane widening its reach is one column edit.

```typescript signature
import { Statement, type SqlClient } from "@effect/sql"

class _Vector extends Schema.Class<_Vector>("Search.Vector")({
  literal: Schema.NonEmptyString,
  fingerprint: _Fingerprint,
}) {}

const _WINDOW = 64

class _Bind extends Schema.Class<_Bind>("Search.Bind")({
  text: Schema.NonEmptyString,
  limit: Schema.Int.pipe(Schema.greaterThan(0)),
  scope: Schema.declare(Statement.isFragment),
  distance: Schema.Int.pipe(Schema.greaterThan(0)),
  vector: Schema.OptionFromSelf(_Vector),
}) {}

declare namespace Search {
  type Bind = _Bind
}

const _lanes = {
  fts: {
    grants: ["core"],
    floor: true,
    material: "text",
    shapes: ["document", "label"],
    rank: (sql: SqlClient.SqlClient, corpus: Search.Table, bind: Search.Bind) =>
      sql.onDialectOrElse({
        orElse: () =>
          sql`SELECT ${sql(`${corpus}_fts`)}.cell, rank() OVER (ORDER BY ${sql(`${corpus}_fts`)}.rank) AS rank
              FROM ${sql(`${corpus}_fts`)} JOIN ${sql(corpus)} c ON c.cell = ${sql(`${corpus}_fts`)}.cell
              WHERE ${sql(`${corpus}_fts`)} MATCH ${bind.text} AND ${bind.scope} LIMIT ${bind.limit}`,
        pg: () =>
          sql`SELECT c.cell, rank() OVER (ORDER BY ts_rank_cd(to_tsvector('simple', c.body), websearch_to_tsquery('simple', ${bind.text})) DESC) AS rank
              FROM ${sql(corpus)} c WHERE to_tsvector('simple', c.body) @@ websearch_to_tsquery('simple', ${bind.text}) AND ${bind.scope} LIMIT ${bind.limit}`,
      }),
  },
  trigram: {
    grants: ["trigram"],
    floor: false,
    material: "text",
    shapes: ["document", "label"],
    rank: (sql: SqlClient.SqlClient, corpus: Search.Table, bind: Search.Bind) =>
      sql`SELECT c.cell, rank() OVER (ORDER BY similarity(c.body, ${bind.text}) DESC) AS rank
          FROM ${sql(corpus)} c WHERE c.body % ${bind.text} AND ${bind.scope} LIMIT ${bind.limit}`,
  },
  phonetic: {
    grants: ["phonetic"],
    floor: false,
    material: "text",
    shapes: ["label"],
    rank: (sql: SqlClient.SqlClient, corpus: Search.Table, bind: Search.Bind) =>
      sql`SELECT c.cell, rank() OVER (ORDER BY c.cell) AS rank
          FROM ${sql(corpus)} c
          WHERE daitch_mokotoff(c.body) && daitch_mokotoff(${bind.text}) AND ${bind.scope} LIMIT ${bind.limit}`,
  },
  fuzzy: {
    grants: ["fuzzy"],
    floor: false,
    material: "text",
    shapes: ["label"],
    rank: (sql: SqlClient.SqlClient, corpus: Search.Table, bind: Search.Bind) =>
      sql`SELECT c.cell, rank() OVER (
            ORDER BY levenshtein_less_equal(left(c.body, ${_WINDOW}), left(${bind.text}, ${_WINDOW}), ${bind.distance})
          ) AS rank
          FROM ${sql(corpus)} c
          WHERE levenshtein_less_equal(left(c.body, ${_WINDOW}), left(${bind.text}, ${_WINDOW}), ${bind.distance}) <= ${bind.distance}
            AND ${bind.scope} LIMIT ${bind.limit}`,
  },
  semantic: {
    grants: ["vector", "vchord"],
    floor: false,
    material: "embedding",
    shapes: ["document", "label"],
    rank: (sql: SqlClient.SqlClient, corpus: Search.Table, bind: Search.Bind) =>
      Option.match(bind.vector, {
        onNone: () => sql`SELECT cell, 1 AS rank FROM retrieve_embedding WHERE 1 = 0`,
        onSome: (held) =>
          sql`SELECT e.cell, rank() OVER (ORDER BY e.embedding <=> ${held.literal}::vector) AS rank
              FROM retrieve_embedding e JOIN ${sql(corpus)} c ON c.cell = e.cell
              WHERE e.corpus = ${corpus} AND e.fingerprint = ${held.fingerprint} AND ${bind.scope} LIMIT ${bind.limit}`,
      }),
  },
} as const

const _LANE_NAMES = ["fts", "trigram", "phonetic", "fuzzy", "semantic"] as const

const _DISPOSITIONS = ["ran", "ungranted", "unembedded", "denied", "unshaped", "excluded"] as const

const _EMBEDS = ["held", "absent", "denied"] as const

declare namespace Search {
  type Lane = (typeof _LANE_NAMES)[number]
  type Disposition = (typeof _DISPOSITIONS)[number]
  type Embed = (typeof _EMBEDS)[number]
}

const _admitted = (
  requested: ReadonlyArray<Search.Lane>,
  granted: HashSet.HashSet<string>,
  embed: Search.Embed,
  shape: Search.Shape,
): Record.ReadonlyRecord<Search.Lane, Search.Disposition> =>
  Record.map(_lanes, (row, lane) =>
    !Array.contains(requested, lane)
      ? "excluded"
      : !Array.contains(row.shapes, shape)
        ? "unshaped"
        : row.material === "embedding" && embed === "denied"
          ? "denied"
          : row.material === "embedding" && embed !== "held"
            ? "unembedded"
            : Array.some(row.grants, (grant) => HashSet.has(granted, grant)) || row.floor
              ? "ran"
              : "ungranted")
```

## [05]-[FUSION_QUERY]

- Owner: `Search.of(corpus)` — the once-per-scope effectful binding whose accessors mint at construction and whose members are the bound read family: `search` (the fused RRF statement and the rerank admission), `facets`, the snippet projection, the keyset cursor codec, and `ddl` from `[3]`; one request shape carries every modality.
- Packages: `effect` (`Effect`, `Option`, `HashMap`, `HashSet`, `Record`, `Schema`, `Array`); `@effect/experimental` (`VariantSchema.make` — the `row`/`domain` field family behind `Search.Hit` and `Search.FacetCount`); `@effect/sql` (the fused statement, the rerank-window body fetch, the snippet fetch, and the one-statement facet census are each composed fragment values — `sql.in` set-shaped over the hit cells, `sql.and` over the filter rows, never a per-hit query and never assembled text); `lane/capability.md` (`Capability` — the grant read, taken once at bind because grants are scope-construction facts); `@rasm/core` (`Shape.Record`).
- Entry: `const bound = yield* Search.of(corpus)` inside the owning scope's construction, then `bound.search(request)` per call; `Search.Request` admits text, lanes, policy refinements, decoded cursor, filters, facets, snippets, and rerank depth once, and the reply carries scored hits, facet counts, next cursor, lane census, and rerank disposition.
- Receipt: `Search.Page.lanes` names each lane's disposition and `Search.Page.rerank` names the accelerator's — `applied`, `partial` (the provider omitted or repeated candidates and the seam repaired the window), `degraded` (a retryable provider fault and fusion order held), `denied` (a settled screen verdict: fusion order held and a re-request is refused by law), `off` — so a degraded scope, a misbehaving provider, and a moderation refusal are each visible in every reply and a relevance regression traces to evidence, never to guesswork.
- Growth: rerank depth, fusion constant `k`, edit-distance ceiling, facet bound, filter rows, and snippet shape are `Search.Request` fields derived from `Search.Policy`; a new reply projection is a field on the page, never a second search.
- Law: the binding is bind-once — `Search.of` yields the client, reads the capability report, and mints every `SqlSchema` accessor exactly once, so a search call pays zero construction and resolver identity holds across calls; an accessor minted inside `search`'s body is the per-call rebuild `read/query.md` already names as the defect.
- Law: fusion is in-database and fragment-composed — admitted lane fragments fold into the `WITH` roster and the `UNION ALL` pool by fragment interpolation, `Σ 1.0/(k + rank)` groups by cell, the keyset predicate arrives as a bound-value `HAVING` fragment when a cursor exists, and the statement is ONE round trip whose every parameter is value-bound; assembling lanes in process re-buys N queries and loses the shared plan, and hand-counted placeholder text is the deleted defect.
- Law: every reply row decodes, and the driver posture is a VARIANT of the decoded truth — `Search.Hit` and `Search.FacetCount` each declare ONE field family over `row` and `domain`, the lenient numeric-or-string codec riding `row` because aggregate numerics arrive dialect-dependent, the settled numeric riding `domain`, and `snippet` declaring domain-only so no row shape carries a key no lane projects; the snippet clips and rerank bodies prove through their own `Result` schemas, so neither a `String(row[...])` cast nor a parallel row struct exists on the page.
- Law: facet census shares request scope — one `SqlSchema.findAll` accessor folds every requested dimension through `UNION ALL`, applies the same `Search.Filter` fragment before grouping, and binds the request's refined `facetTop`; a per-dimension round trip, unfiltered census, or hidden module default is a different query and therefore a defect.
- Law: the cursor is opaque and typed — `{ score, cell }` under one composed codec, `Schema.StringFromBase64Url` over `Schema.parseJson`, so encode and decode share the schema and a malformed caller cursor is `ParseError` on the admission rail; a raw offset is the rejected pagination, and the cursor mints from the FUSED order — rerank re-orders presentation inside the page and never moves the keyset coordinate, so a full-page rerank window cannot skip rows.
- Law: snippets ride the granted relevance lane AND its dialect — `ts_headline` is the in-core pg floor and the FTS5 `snippet()` arm serves the sqlite profiles against the same provisioned virtual table the lane row queries; the `bm25`-granted relevance arm ranks and never clips, because `vchord_bm25` ships NO highlight member and none can exist — a `bm25vector` carries vocabulary ids and frequencies, not text offsets — so a bm25 lane row pairs its ranking with the floor's own headline arm over the matched rows, and the ranking spelling itself proves at the lane's capability probe since the alpha line respells `to_bm25query`'s arity across minors.
- Law: rerank is an admitted window policy — when the `Reranker` is present and the request asks, the top `window` fused hits re-order by the port's verdict AFTER the seam proves it: the answer deduplicates, unknown cells drop, candidates the provider omitted keep their fusion order behind the ranked head, an empty body window reports `partial`, and the tail beyond the window never moves — so hit cardinality is invariant under any provider answer, the page's tail guarantee holds for every value the port type admits, and a port fault holds fusion order through `Effect.either` with the disposition read off `class` — `denied` for a settled verdict, `degraded` for the rest; retrieval never fails on the accelerator.

```typescript signature
import { Array, Effect, Either, HashMap, HashSet, Match, Option, type ParseResult, pipe, Record, Schema } from "effect"
import { VariantSchema } from "@effect/experimental"
import { SqlClient, SqlSchema, type SqlError, type Statement } from "@effect/sql"
import { Shape } from "@rasm/core"
import { Capability } from "../lane/capability.ts"
import type { Pg } from "../lane/postgres.ts"

declare namespace Search {
  type Filter = typeof _Filter.Type
  type Request = _Request
  type Rerank = (typeof _RERANKS)[number]
  type Hit = _Hit
  type Page = _Page
}

const _Cursor = Schema.compose(
  Schema.StringFromBase64Url,
  Schema.parseJson(Schema.Struct({ score: Schema.Number, cell: Schema.NonEmptyString })),
)

class _Policy extends Schema.Class<_Policy>("Search.Policy")({
  limit: Schema.Int.pipe(Schema.between(1, 200)),
  k: Schema.Int.pipe(Schema.between(1, 1000)),
  distance: Schema.Int.pipe(Schema.between(1, _WINDOW - 1)),
  rerank: Schema.NonNegativeInt.pipe(Schema.lessThanOrEqualTo(200)),
  facetTop: Schema.Int.pipe(Schema.between(1, 500)),
}) {}

const _PAGE = new _Policy({ limit: 20, k: 60, distance: 4, rerank: 0, facetTop: 50 })

const _Scalar = Schema.Union(Schema.String, Schema.Number, Schema.Boolean)

const _Filter = Schema.Union(
  Schema.TaggedStruct("Equal", { dim: Query.Relation.fields.table, value: _Scalar }),
  Schema.TaggedStruct("NotEqual", { dim: Query.Relation.fields.table, value: _Scalar }),
  Schema.TaggedStruct("AtLeast", { dim: Query.Relation.fields.table, value: _Scalar }),
  Schema.TaggedStruct("AtMost", { dim: Query.Relation.fields.table, value: _Scalar }),
  Schema.TaggedStruct("Between", { dim: Query.Relation.fields.table, lower: _Scalar, upper: _Scalar }),
  Schema.TaggedStruct("AnyOf", { dim: Query.Relation.fields.table, values: Schema.NonEmptyArray(_Scalar) }),
)

const _RERANKS = ["applied", "partial", "degraded", "denied", "off"] as const

class _Request extends Schema.Class<_Request>("Search.Request")({
  text: Schema.NonEmptyString,
  lanes: Schema.optionalWith(Schema.Array(Schema.Literal(..._LANE_NAMES)), {
    default: () => ["fts", "trigram", "semantic"],
  }),
  limit: Schema.optionalWith(_Policy.fields.limit, { default: () => _PAGE.limit }),
  k: Schema.optionalWith(_Policy.fields.k, { default: () => _PAGE.k }),
  distance: Schema.optionalWith(_Policy.fields.distance, { default: () => _PAGE.distance }),
  cursor: Schema.optionalWith(_Cursor, { as: "Option" }),
  filter: Schema.optionalWith(Schema.Array(_Filter), { default: () => [] }),
  facets: Schema.optionalWith(Schema.Array(Query.Relation.fields.table), { default: () => [] }),
  facetTop: Schema.optionalWith(_Policy.fields.facetTop, { default: () => _PAGE.facetTop }),
  snippet: Schema.optionalWith(Schema.Boolean, { default: () => false }),
  rerank: Schema.optionalWith(_Policy.fields.rerank, { default: () => _PAGE.rerank }),
}) {}

const _Score = Schema.Union(Schema.Number, Schema.NumberFromString)

const _Count = Schema.Union(Schema.NonNegativeInt, Schema.NumberFromString.pipe(Schema.int(), Schema.nonNegative()))

const _posture = VariantSchema.make({ variants: ["row", "domain"], defaultVariant: "domain" })

class _Hit extends _posture.Class<_Hit>("Search.Hit")({
  cell: Schema.NonEmptyString,
  score: _posture.Field({ row: _Score, domain: Schema.Number }),
  snippet: _posture.Field({ domain: Schema.OptionFromSelf(Schema.String) }),
}) {}

class _FacetCount extends _posture.Class<_FacetCount>("Search.FacetCount")({
  dim: Query.Relation.fields.table,
  value: _Scalar,
  count: _posture.Field({ row: _Count, domain: Schema.NonNegativeInt }),
}) {}

class _Page extends Schema.Class<_Page>("Search.Page")({
  hits: Schema.Array(_Hit),
  facets: Schema.Array(_FacetCount),
  cursor: Schema.OptionFromSelf(Schema.String),
  lanes: Shape.Record(Schema.Literal(..._LANE_NAMES), Schema.Literal(..._DISPOSITIONS)),
  rerank: Schema.Literal(..._RERANKS),
}) {}

const _Body = Reranker.Candidate

const _Clip = Schema.Struct({ cell: Schema.NonEmptyString, clip: Schema.String })

const _scoped = (sql: SqlClient.SqlClient, filter: ReadonlyArray<Search.Filter>) =>
  Array.isNonEmptyReadonlyArray(filter)
    ? sql.and(Array.map(
      filter,
      pipe(
        Match.type<Search.Filter>(),
        Match.withReturnType<Statement.Fragment>(),
        Match.tagsExhaustive({
          Equal: (term) => sql`c.${sql(term.dim)} = ${term.value}`,
          NotEqual: (term) => sql`c.${sql(term.dim)} <> ${term.value}`,
          AtLeast: (term) => sql`c.${sql(term.dim)} >= ${term.value}`,
          AtMost: (term) => sql`c.${sql(term.dim)} <= ${term.value}`,
          Between: (term) => sql`c.${sql(term.dim)} BETWEEN ${term.lower} AND ${term.upper}`,
          AnyOf: (term) => sql.or(Array.map(term.values, (value) => sql`c.${sql(term.dim)} = ${value}`)),
        }),
      ),
    ))
    : sql`1 = 1`

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
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
  accTitle: Retrieval fusion rail
  accDescr: Request admission selects data-driven retrieval lanes, every lane rejoins one reciprocal-rank fold, and paging plus reranking yield one typed page.
  R([Search Request]) --> A{Admitted lanes}
  G[(Grant and material rows)] -.->|select| A
  A -->|fts| FTS[Full text]
  A -->|trigram| TRI[Trigram]
  A -->|phonetic| PHO[Phonetic]
  A -->|fuzzy| FUZ[Fuzzy]
  A -->|semantic| SEM[Semantic]
  FTS --> F[[Reciprocal rank fold]]
  TRI --> F
  PHO --> F
  FUZ --> F
  SEM --> F
  F --> K[Keyset page]
  K --> W[Rerank admission]
  W --> P[/Search Page/]
```

```typescript signature
const _ddl = (corpus: Search.Corpus, granted: HashSet.HashSet<string>) => {
  const vector = HashSet.has(granted, "vchord")
    ? Option.some(_indexRows.vectorChord)
    : HashSet.has(granted, "vector")
      ? Option.some(_indexRows.vectorHnsw)
      : Option.none<(typeof _indexRows)["vectorChord" | "vectorHnsw"]>()
  const admitted = Array.appendAll(
    Option.toArray(vector),
    Array.prepend(
      Array.filter(
        [_indexRows.trigram, _indexRows.phonetic, _indexRows.keyset],
        (row) => row.grant === "core" || HashSet.has(granted, row.grant),
      ),
      _indexRows.fts,
    ),
  )
  return {
    census: Array.map(admitted, (row) => row.artifact),
    ensures: Array.appendAll(
      Option.match(vector, { onNone: () => [], onSome: () => [_embeddingDdl(corpus.embedding.dims)] }),
      Array.map(admitted, (row): Capability.Ensure => ({ relation: corpus.table, ...row.ensure(corpus.table) })),
    ),
  }
}

const _reranked = (
  bodies: (cells: ReadonlyArray<string>) => Effect.Effect<ReadonlyArray<typeof _Body.Type>, SqlError.SqlError | ParseResult.ParseError>,
  text: string,
  window: number,
  hits: ReadonlyArray<Search.Hit>,
) =>
  Effect.flatMap(Effect.serviceOption(Reranker), (port) =>
    Option.match(Option.filter(port, () => window > 0 && hits.length > 0), {
      onNone: () => Effect.succeed([hits, "off"] as const),
      onSome: (reranker) =>
        Effect.gen(function* () {
          const head = Array.take(hits, window)
          const candidates = yield* bodies(Array.map(head, (hit) => hit.cell))
          if (!Array.isNonEmptyReadonlyArray(candidates)) return [hits, "partial"] as const
          const verdict = yield* Effect.either(reranker.rerank(text, candidates))
          return Either.match(verdict, {
            onLeft: (fault) => [hits, fault.class === "denied" ? ("denied" as const) : ("degraded" as const)] as const,
            onRight: (order) => {
              const byCell = HashMap.fromIterable(Array.map(head, (hit) => [hit.cell, hit] as const))
              const ranked = Array.filterMap(Array.dedupe(order), (cell) => HashMap.get(byCell, cell))
              const seen = HashSet.fromIterable(Array.map(ranked, (hit) => hit.cell))
              const kept = Array.filter(head, (hit) => !HashSet.has(seen, hit.cell))
              const repaired = ranked.length !== order.length || kept.length > 0
              return [Array.appendAll(Array.appendAll(ranked, kept), Array.drop(hits, window)), repaired ? ("partial" as const) : ("applied" as const)] as const
            },
          })
        }),
    }))

const _facetArm = (
  sql: SqlClient.SqlClient,
  table: Search.Table,
  dim: Query.Relation["table"],
  scope: Statement.Fragment,
  top: number,
) =>
  sql`SELECT * FROM (SELECT ${dim} AS dim, c.${sql(dim)} AS value, count(*) AS count FROM ${sql(table)} c
      WHERE ${scope} GROUP BY c.${sql(dim)} ORDER BY count(*) DESC LIMIT ${top}) f`

const _of = (corpus: Search.Corpus) =>
  Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const capability = yield* Capability.of<Pg.Grant>()
    const hits = Schema.decodeUnknown(Schema.Array(_Hit.row))
    const bodies = SqlSchema.findAll({
      Request: Schema.Array(Schema.String),
      Result: _Body,
      execute: (cells) => sql`SELECT cell, body FROM ${sql(corpus.table)} WHERE ${sql.in("cell", cells)}`,
    })
    const clips = SqlSchema.findAll({
      Request: Schema.Struct({ text: Schema.String, cells: Schema.Array(Schema.String) }),
      Result: _Clip,
      execute: (window) =>
        sql.onDialectOrElse({
          orElse: () =>
            sql`SELECT cell, snippet(${sql(`${corpus.table}_fts`)}, -1, '', '', '…', 12) AS clip
                FROM ${sql(`${corpus.table}_fts`)} WHERE ${sql(`${corpus.table}_fts`)} MATCH ${window.text} AND ${sql.in("cell", window.cells)}`,
          pg: () =>
            sql`SELECT cell, ts_headline('simple', body, websearch_to_tsquery('simple', ${window.text})) AS clip
                FROM ${sql(corpus.table)} WHERE ${sql.in("cell", window.cells)}`,
        }),
    })
    const counted = SqlSchema.findAll({
      Request: Schema.Struct({
        dims: Schema.Array(Query.Relation.fields.table),
        filter: Schema.Array(_Filter),
        top: _Policy.fields.facetTop,
      }),
      Result: _FacetCount.row,
      execute: (request) =>
        Array.isNonEmptyReadonlyArray(request.dims)
          ? Array.reduce(
              Array.tailNonEmpty(request.dims),
              _facetArm(sql, corpus.table, Array.headNonEmpty(request.dims), _scoped(sql, request.filter), request.top),
              (held, dim) => sql`${held} UNION ALL ${_facetArm(sql, corpus.table, dim, _scoped(sql, request.filter), request.top)}`,
            )
          : sql`SELECT '' AS dim, '' AS value, 0 AS count WHERE 1 = 0`,
    })
    const search = (request: Search.Request) =>
      Effect.gen(function* () {
        const requested = request.lanes
        const embedder = yield* Effect.serviceOption(Embedder)
        const outcome = yield* Effect.transposeOption(
          Option.map(Option.filter(embedder, () => Array.contains(requested, "semantic")), (port) =>
            Effect.either(
              Effect.map(
                Effect.filterOrFail(
                  port.embed(request.text),
                  (vector) =>
                    vector.length === corpus.embedding.dims &&
                    Array.every(vector, Number.isFinite) &&
                    port.fingerprint === corpus.embedding.fingerprint,
                  (vector) => new EmbedFault({
                    case: {
                      reason: "shape",
                      expected: corpus.embedding.fingerprint,
                      dims: corpus.embedding.dims,
                      fingerprint: port.fingerprint,
                      length: vector.length,
                    },
                  }),
                ),
                (vector) => ({ vector, fingerprint: port.fingerprint }),
              ))))
        const embedded = Option.flatMap(outcome, Either.getRight)
        const embed = Option.match(outcome, {
          onNone: () => "absent" as const,
          onSome: Either.match({
            onLeft: (fault) => (fault.class === "denied" ? ("denied" as const) : ("absent" as const)),
            onRight: () => "held" as const,
          }),
        })
        const census = _admitted(requested, capability.granted, embed, corpus.shape)
        const running = Array.filter(Record.keys(census), (lane) => census[lane] === "ran")
        const cursor = request.cursor
        const limit = request.limit
        const bind = new _Bind({
          text: request.text,
          limit,
          scope: _scoped(sql, request.filter),
          distance: request.distance,
          vector: Option.map(embedded, (held) => new _Vector({
            literal: `[${Array.join(held.vector, ",")}]`,
            fingerprint: held.fingerprint,
          })),
        })
        const rows = yield* Array.isNonEmptyReadonlyArray(running)
          ? Effect.flatMap(
              _fused(sql, corpus.table, running, bind, request.k, cursor, limit + 1),
              hits,
            )
          : Effect.succeed<ReadonlyArray<typeof _Hit.row.Type>>([])
        const scored = Array.map(Array.take(rows, limit), (row) => new _Hit({
          cell: row.cell,
          score: row.score,
          snippet: Option.none<string>(),
        }))
        const clipped = request.snippet && scored.length > 0
          ? yield* Effect.map(clips({ text: request.text, cells: Array.map(scored, (hit) => hit.cell) }), (found) => {
              const byCell = HashMap.fromIterable(Array.map(found, (row) => [row.cell, row.clip] as const))
              return Array.map(scored, (hit) => new _Hit({
                cell: hit.cell,
                score: hit.score,
                snippet: HashMap.get(byCell, hit.cell),
              }))
            })
          : scored
        const [ordered, reranked] = yield* _reranked(bodies, request.text, request.rerank, clipped)
        const facets = Array.map(
          yield* counted({ dims: request.facets, filter: request.filter, top: request.facetTop }),
          (row) => new _FacetCount(row),
        )
        const next = rows.length > limit ? Array.last(clipped) : Option.none<Search.Hit>()
        return new _Page({
          hits: ordered,
          facets,
          cursor: yield* Effect.transposeOption(
            Option.map(next, (hit) => Schema.encode(_Cursor)({ score: hit.score, cell: hit.cell }))),
          lanes: census,
          rerank: reranked,
        })
      })
    return {
      search,
      facets: counted,
      ddl: _ddl(corpus, capability.granted),
    }
  })

const Search = {
  Corpus: _Corpus,
  Embedding: _Embedding,
  Cursor: _Cursor,
  Filter: _Filter,
  Hit: _Hit,
  Page: _Page,
  Policy: _Policy,
  Request: _Request,
  defaults: _PAGE,
  fingerprint: _fingerprint,
  lanes: _lanes,
  indexes: _indexRows,
  embedding: _embeddingDdl,
  admitted: _admitted,
  fused: _fused,
  ddl: _ddl,
  of: _of,
} as const

// --- [EXPORTS] -------------------------------------------------------------------------

export { Embedder, EmbedFault, Reranker, Search }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
