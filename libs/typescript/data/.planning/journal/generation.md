# [DATA_GENERATION]

One floor mint holds the journal's shape identity: a log carries ONE generation — a content digest over the compiled event family's declared shape — on an append-only custody ledger whose head every binder and every transaction proves against, so a runtime compiled elsewhere refuses at the consumer. `Payload.Envelope` is the persisted `(tag, payload)` coordinate every journal projection spreads, and `Payload.Column` the one fused JSON-column codec, so a digest-addressed TEXT column and a json column the spine driver hands back as a live object decode through one admission. This page imports no data sibling: `append` opens every transaction on its guard, `evolve` runs the cutover over its ledger, and `retain`, `fact`, and `fold` spread its coordinate.

Every entry carries its tag and its bytes alone, because a homogeneous log leaves a per-entry shape coordinate nothing to say. Its ledger's HEAD is the live generation, its origin row is a measured zero, and the custody receipt every cutover seals is the second and every later row — so the lineage a receipt reads and the identity a runtime proves are one relation rather than a current-value row beside a history nobody joins.

## [01]-[INDEX]

- [02]-[PAYLOAD_COLUMN]: `Payload.Envelope`, its fused JSON-column codec, and the typed column read composing both.
- [03]-[GENERATION_IDENTITY]: `Generation.of` over the framed shape preimage, the custody ledger, `bind`, the per-transaction guard, the cutover seal, `GenerationSkew`.

## [02]-[PAYLOAD_COLUMN]

- Owner: `Payload.Envelope` is the persisted `(tag, payload)` coordinate every journal projection spreads and `Payload.Raw` its decoded type; `Payload.Column` is the fused JSON-column codec every payload-bearing field composes; `Payload.json(shape)` composes that codec with an owning shape.
- Packages: `effect` (`Either`, `ParseResult`, `Schema`).
- Growth: a new payload-bearing relation spreads the envelope fields and declares only the columns it owns.
- Law: the envelope coordinate is ONE declaration and every persisted projection spreads `Payload.Envelope.fields`; a struct restating the pair beside this family is the parallel-shape defect, and a form diverging in MEANING rather than in spelling keeps its own declaration and reuses the field alone.
- Law: the entry carries its tag and its bytes and nothing else — the log is homogeneous in shape by the generation law, so a per-entry shape coordinate carries a value every reader already holds, and every read decodes through the one compiled family.
- Law: the pair spells one key set in every dialect, so the coordinate stays a plain struct — a variant family binds two projections differing in no key.
- Law: `Payload.Column` exists because two column postures reach one decode — every digest-preimage payload column is TEXT in every dialect by the append owner's byte-truth law, while the snapshot body and frontier floor stay json columns the spine driver hands back as live objects — so one codec admits string and object arrivals alike, the miss rides `ParseError` on the one admission rail, and a malformed stored text is a projection-time `ParseError` because the column was written by `Schema.encode` and cannot lawfully hold non-JSON.
- Boundary: the current family is app material arriving as a `Schema.Union` value; the relations carrying this envelope are `journal/append.md`'s journal rows and `journal/retain.md`'s export rows; the native op-log keeps a distinct thirteen-position envelope that aliases this coordinate nowhere.

```typescript signature
import { Either, ParseResult, Schema } from "effect"

const _Column: Schema.Schema<unknown> = Schema.transformOrFail(Schema.Unknown, Schema.Unknown, {
  strict: true,
  decode: (column, _options, ast) =>
    typeof column === "string"
      ? Either.try({ try: (): unknown => JSON.parse(column), catch: () => new ParseResult.Type(ast, column) })
      : ParseResult.succeed(column),
  encode: (value) => ParseResult.succeed(value),
})

const _Envelope = Schema.Struct({
  tag: Schema.String,
  payload: _Column,
})

const Payload = {
  Column: _Column,
  Envelope: _Envelope,
  json: <A, I>(shape: Schema.Schema<A, I>): Schema.Schema<A, unknown> =>
    Schema.compose(_Column, shape, { strict: false }),
} as const

declare namespace Payload {
  type Raw = typeof _Envelope.Type
}
```

## [03]-[GENERATION_IDENTITY]

- Owner: `Generation.of(shape)` mints the content digest over one framed shape preimage; `Generation.bind(app, family)` seats the compiled generation against the log's custody head and answers `Generation.Held`; `Generation.guard(sql, held)` is the one statement every append transaction opens with; `Generation.fence` and `Generation.head` are the two halves the guard composes and the cutover takes apart — the exclusive fence and the head read; `Generation.seal` lands a cutover's custody row; `GenerationSkew` refuses a binder or a writer whose compiled digest disagrees; the `journal_custody` ensure row is the append-only lineage whose HEAD is the live generation.
- Packages: `effect` (`Array`, `Effect`, `JSONSchema`, `Option`, `Order`, `Predicate`, `Record`, `Schema`); `@effect/sql` (`SqlClient`, `SqlSchema`); `@rasm/core` (`CanonicalWriter`, `Digest`, `Fault.Class`, `Identity.App`).
- Entry: the composition root binds once — `Generation.bind` answers the `Generation.Held` the journal spec carries, so write path, read path, and the cutover all name one anchor and none re-derives it.
- Receipt: `Generation.Held` — `{ app, generation, ordinal }` — the app's log, the digest it holds, and the custody position that seated it; `Generation.Custody` — `{ ordinal, source, target, entries, digest }` — the row a cutover seals: the position it seated, the generation it left, the generation it landed, the count it re-encoded, and the digest over the bytes it wrote, in sequence order.
- Law: the generation IS a content key over declared shape, so it decodes through the one content-key codec and mints no second brand; `Digest.mint("content", …)` reads the framed preimage `CanonicalWriter` emits, exactly as every other generation identity in this branch does.
- Law: the preimage frames the JSON Schema projection of the family — the ENCODED shape a reader must decode — so a transformation edit leaving the stored shape identical moves nothing, while a member added, removed, or reshaped moves the digest.
- Law: framing sorts every record key and keeps every array position — a record's key order is enumeration order, so a digest reading it gives one logical shape a different preimage per build, while `anyOf` member order IS the family's declared order and the sort destroys that meaning.
- Law: record and array nodes emit their own kind marker before their members, so a positional array and a numerically-keyed record cannot fold onto one preimage.
- Law: the ledger is append-only and its HEAD is the live generation — one row per cutover, `ordinal` monotone from the origin row a binder seats itself; `seal` inserts without a conflict arm, so a cutover that lost its fence and re-seals a position the lineage already holds refuses as the primary-key conflict it is rather than overwriting a receipt.
- Law: the origin row carries a structural zero — a log at ordinal zero holds no entry and no source, so `entries` is a MEASURED zero and `source` is absent rather than self-referential; the digest is the empty fold's own answer, so the receipt shape stays total across the log's whole life.
- Law: skew classifies `invalid` — no schedule outlasts a process compiled against a shape the store does not hold, so the refusal is terminal evidence a deployment repairs, never a re-drive.
- Law: the guard rides EVERY append transaction and answers in one round trip — the shared app lock and the head read compose into one statement, so a writer that bound before a cutover and kept writing across the swap refuses at its next transaction rather than landing old-shaped bytes in a re-minted log; the cost is one indexed single-row read per commit, and what it buys is a fence that closes on an in-flight writer instead of only on a re-binder.
- Law: the lock is SHARED at the guard and exclusive at the cutover, so concurrent appends never serialize against each other and the cutover waits for every in-flight commit before its fold begins; the sqlite profiles carry no advisory lock and rest on their single writer, exactly as the per-stream OCC lock already does.
- Law: custody carries no tenant column and registers no row-level policy — the log's generation spans every tenant of its app, so a tenant-scoped generation lets one tenant's cutover fence another's, and the maintenance plane reads this relation under the posture the tenancy owner mints.
- Growth: a shape change is one cutover landing one custody row; a second app is one origin row.
- Boundary: the compiled family is app material and this page never reads a payload — it digests the family's declared shape and compares digests; the cutover itself is `journal/evolve.md`'s.

```typescript signature
import { Array, Effect, JSONSchema, Option, Order, Predicate, Record, Schema, type SqlError } from "effect"
import { SqlClient, SqlSchema, type Statement } from "@effect/sql"
import { CanonicalWriter, Digest, Fault, Identity } from "@rasm/core"
import type { Capability } from "../lane/capability.ts"

const _framed = (writer: CanonicalWriter, node: unknown): CanonicalWriter =>
  Array.isArray(node)
    ? writer.string("[").rows(node, (member, held) => {
      _framed(held, member)
    })
    : Predicate.isRecord(node)
    ? writer.string("{").rows(
      Array.sort(Record.toEntries(node), Order.mapInput(Order.string, ([key]: readonly [string, unknown]) => key)),
      ([key, value], held) => {
        _framed(held.string(key), value)
      },
    )
    : writer.string(JSON.stringify(node))

const _of = <A, I>(shape: Schema.Schema<A, I>): Effect.Effect<Digest.Key<"content">> =>
  Digest.mint("content", _framed(new CanonicalWriter(), JSONSchema.make(shape)).close())

const _Ordinal = Schema.Int.pipe(Schema.nonNegative())

class GenerationSkew extends Schema.TaggedError<GenerationSkew>()("GenerationSkew", {
  app: Identity.App.fields.app,
  compiled: Digest.Key.content,
  observed: Digest.Key.content,
  ordinal: _Ordinal,
}) {
  get class(): Fault.Class.Kind {
    return "invalid"
  }
  override get message(): string {
    return `<journal:generation> ${this.app} compiled ${this.compiled} observed ${this.observed} at ${this.ordinal}`
  }
}

const _custodyDdl: Capability.Ensure = {
  relation: "journal_custody",
  pg: `CREATE TABLE IF NOT EXISTS journal_custody (
    app TEXT NOT NULL,
    ordinal INT NOT NULL,
    source TEXT,
    target TEXT NOT NULL,
    entries BIGINT NOT NULL,
    digest TEXT NOT NULL,
    sealed_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (app, ordinal));`,
  sqlite: `CREATE TABLE IF NOT EXISTS journal_custody (
    app TEXT NOT NULL,
    ordinal INTEGER NOT NULL,
    source TEXT,
    target TEXT NOT NULL,
    entries INTEGER NOT NULL,
    digest TEXT NOT NULL,
    sealed_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
    PRIMARY KEY (app, ordinal));`,
}

const _Custody = Schema.Struct({
  ordinal: _Ordinal,
  source: Schema.OptionFromNullOr(Digest.Key.content),
  target: Digest.Key.content,
  entries: Schema.BigInt,
  digest: Digest.Key.content,
})

declare namespace Generation {
  type Key = Digest.Key<"content">
  type Custody = typeof _Custody.Type
  type Held = {
    readonly app: Identity.App.Key
    readonly generation: Key
    readonly ordinal: number
  }
}

const _Head = Schema.Struct({ target: Digest.Key.content, ordinal: _Ordinal })

const _head = (sql: SqlClient.SqlClient, app: Identity.App.Key) =>
  SqlSchema.single({
    Request: Identity.App.fields.app,
    Result: _Head,
    execute: (key) =>
      sql`SELECT target, ordinal FROM journal_custody
          WHERE app = ${key} ORDER BY ordinal DESC LIMIT 1`,
  })(app)

const _fence = (sql: SqlClient.SqlClient, app: Identity.App.Key, mode: "shared" | "exclusive"): Statement.Fragment =>
  sql.onDialectOrElse({
    orElse: () => sql`SELECT 1`,
    pg: () =>
      mode === "shared"
        ? sql`SELECT pg_advisory_xact_lock_shared(hashtextextended(${app}, 0))`
        : sql`SELECT pg_advisory_xact_lock(hashtextextended(${app}, 0))`,
  })

const _held = (app: Identity.App.Key, compiled: Generation.Key) => (row: typeof _Head.Type) =>
  row.target === compiled
    ? Effect.succeed<Generation.Held>({ app, generation: compiled, ordinal: row.ordinal })
    : Effect.fail(new GenerationSkew({ app, compiled, observed: row.target, ordinal: row.ordinal }))

const _bind = <A, I>(app: Identity.App.Key, family: Schema.Schema<A, I>) =>
  Effect.flatMap(SqlClient.SqlClient, (sql) =>
    sql.withTransaction(
      Effect.gen(function* () {
        const compiled = yield* _of(family)
        const empty = yield* Digest.Session.finish(yield* Digest.Session.open("content"))
        yield* _fence(sql, app, "shared")
        yield* sql`INSERT INTO journal_custody ${sql.insert([{
          app,
          ordinal: 0,
          source: null,
          target: compiled,
          entries: 0,
          digest: empty,
        }])} ON CONFLICT (app, ordinal) DO NOTHING`
        return yield* Effect.flatMap(_head(sql, app), _held(app, compiled))
      }),
    ))

const _guard = (sql: SqlClient.SqlClient, held: Generation.Held) =>
  Effect.gen(function* () {
    yield* _fence(sql, held.app, "shared")
    return yield* Effect.flatMap(_head(sql, held.app), _held(held.app, held.generation))
  })

const _seal = (sql: SqlClient.SqlClient, app: Identity.App.Key, custody: Generation.Custody) =>
  sql`INSERT INTO journal_custody ${sql.insert([{
    app,
    ordinal: custody.ordinal,
    source: Option.getOrNull(custody.source),
    target: custody.target,
    entries: custody.entries,
    digest: custody.digest,
  }])}`

const Generation = {
  of: _of,
  bind: _bind,
  guard: _guard,
  fence: _fence,
  head: _head,
  seal: _seal,
  Custody: _Custody,
  ddl: [_custodyDdl],
} as const

// --- [EXPORTS] -------------------------------------------------------------------------

export { Generation, GenerationSkew, Payload }
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
