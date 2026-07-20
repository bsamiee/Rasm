# [DATA_EVOLVE]

Schema evolution without migrations and its read accelerator in one owner: every persisted payload carries the `eventVersion` its author stamped, reads lift it to the current shape through a total per-tag step chain — array-indexed so completeness is a construction fact — before one decode through the live family proves the landing, and the snapshot store is nothing but that same lift applied to a latest-per-stream projection row. The raw log is never rewritten; a new event shape is one step appended plus the bumped `latest`, a state reshape is one step on the snapshot's single-shape chain, and every journal read, projection lane, and hydrate fold inherits the lift through the one plan value. A snapshot is always discardable evidence, never truth — dropping the table costs a replay, nothing more — and the monotonic upsert guard makes concurrent snapshotters harmless without coordination.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                          |
| :-----: | :----------------- | :------------------------------------------------------------------------------ |
|  [01]   | `CHAIN_VOCABULARY` | the raw envelope, the step chain, construction-checked completeness             |
|  [02]   | `PLAN_FOLD`        | `Upcast.plan` for tagged families, `Upcast.chain` for single shapes, the decode |
|  [03]   | `SNAPSHOT_ROW`     | the snapshot-as-projection ensure, the bound save/load, the monotonic upsert    |
|  [04]   | `HYDRATE`          | the cadence policy row and the snapshot-plus-tail recovery fold                 |

## [02]-[CHAIN_VOCABULARY]

- Owner: `Upcast.Raw` — the `{tag, version, payload}` envelope every persisted row projects into before lifting; `Upcast.Chain` — `{latest, steps}` where `steps[i]` lifts version `i + 1` to `i + 2` over the encoded shape.
- Packages: `effect` (types only at this altitude).
- Growth: a new version of one event is one step pushed onto its chain and `latest` bumped by one — old steps never change, because the versions they lift are already in the log.
- Law: steps are total pure functions over encoded payloads — `(payload: unknown) => unknown` with no failure channel; partiality has nowhere to hide because the terminal decode re-proves every invariant the current schema states.
- Law: completeness is positional — a chain of `latest: 4` carries exactly three steps; `_sized` proves it at plan construction as a value, `Either.right` the proven chain and `Either.left` the typed `ChainIncomplete`, so a roster mismatch is a wiring fault the composing Layer folds once, never a throw and never a read-time surprise.
- Boundary: `_sized` is the one construction check on the page — `Upcast.plan` and `Upcast.chain` fold it through `Either.all`, so the constructors are total pure functions into `Either` and no throw, `die`, or defect exit is spellable anywhere on the page.
- Law: the step transforms the whole encoded member including its `_tag` — a rename across versions is a step that rewrites the tag, and the plan indexes chains by the tag AS WRITTEN, so renamed families keep their history reachable.
- Boundary: where `Raw` comes from is `journal/append.md`'s row projection; the current family is app material arriving as a `Schema.Union` value.

```typescript signature
import { Data, Either } from "effect"

declare namespace Upcast {
  type Raw = {
    readonly tag: string
    readonly version: number
    readonly payload: unknown
  }
  type Step = (payload: unknown) => unknown
  type Chain = {
    readonly latest: number
    readonly steps: ReadonlyArray<Step>
  }
  type Roster = { readonly [tag: string]: Chain }
}

class ChainIncomplete extends Data.TaggedError("ChainIncomplete")<{
  readonly tag: string
  readonly steps: number
  readonly latest: number
}> {}

const _sized = (tag: string, chain: Upcast.Chain): Either.Either<Upcast.Chain, ChainIncomplete> =>
  chain.steps.length === chain.latest - 1
    ? Either.right(chain)
    : Either.left(new ChainIncomplete({ tag, steps: chain.steps.length, latest: chain.latest }))
```

## [03]-[PLAN_FOLD]

- Owner: `Upcast` — `plan(family, roster)` binds a tagged event family to its chains; `chain(shape, spec)` is the single-shape twin the snapshot row keys by `snapshot_schema_version`; both return `Either.right` the decode fold that lifts then proves, `Either.left` the typed `ChainIncomplete` the wiring Layer folds once; `Upcast.Column` is the one fused JSON-column codec folder-wide (parse-if-string and decode are ONE schema, never a bare `JSON.parse` beside a decode), and `Upcast.json(shape)` composes it with any owning shape for typed column reads.
- Packages: `effect` (`Effect`, `Array`, `Option`, `Record`, `Schema`, `ParseResult`).
- Entry: the wiring site folds the constructor's `Either` exactly once into its Layer, and the held `plan.decode(raw)` is then the ONLY road from a persisted payload to a live event value — the journal read stream, the projection lanes, and the DSAR fold all compose it; `plan.latest(tag)` is what the append surface stamps, so write-version and read-lift share one anchor and cannot drift.
- Receipt: the decode lands in the family type or fails as `ParseError` on the one admission rail — a version outside the closed `1..latest` interval, a lifted payload failing the current schema, and a persisted tag absent from the roster alike are malformed-history findings routed to quarantine by the consuming lane, never swallowed and never a defect exit.
- Growth: a new tag is one roster entry (`latest: 1`, empty steps); a new version is one step; a family-wide reshape is still per-tag steps — the fold never widens.
- Law: `Upcast.Column` exists because the spine returns json columns as live objects while the sqlite profiles return TEXT — the dialect difference is one codec every payload-bearing `Result` schema composes as a field, so the miss rides `ParseError` on the one admission rail; a malformed stored text is a projection-time `ParseError` because the column was written by `Schema.encode` and cannot lawfully hold non-JSON.
- Law: an unknown persisted tag fails the decode as a minted `ParseResult.ParseError` on the same rail every malformed row rides — the read stays typed, the finding quarantines, and the sanctioned retirement path remains the tombstone union member, so a correctly maintained family never reaches that arm and a dropped one degrades to evidence, never to a crash.
- Law: the lift first admits an integer version inside `1..latest`, then runs `Array.reduce` over `Array.drop(steps, version - 1)` — versions already at `latest` fold through zero steps, invalid coordinates cannot exploit JavaScript slice semantics, and hot reads pay one interval check plus one decode.
- Law: totality is proven per chain by the test-estate law combinators over `plan.census` — the `(tag, latest)` roster projection the plan carries — so every `(tag, version)` pair present in the corpus composes to a decodable value and the proof reads its coordinates off the plan value itself.
- Boundary: snapshot bodies ride `chain` with `snapshot_schema_version` as the coordinate; C#-minted wire shapes arrive already decoded through the interchange codec and never re-enter this fold.

```typescript signature
import { Array, Effect, Either, Option, ParseResult, Record, Schema, SchemaAST } from "effect"

declare namespace Upcast {
  type Plan<A> = {
    readonly census: ReadonlyArray<readonly [tag: string, latest: number]>
    readonly latest: (tag: string) => Option.Option<number>
    readonly decode: (raw: Raw) => Effect.Effect<A, ParseResult.ParseError>
  }
  type Lift<A> = {
    readonly latest: number
    readonly decode: (version: number, payload: unknown) => Effect.Effect<A, ParseResult.ParseError>
  }
}

const _lift = (ast: SchemaAST.AST, chain: Upcast.Chain, version: number, payload: unknown): Effect.Effect<unknown, ParseResult.ParseError> =>
  Number.isSafeInteger(version) && version >= 1 && version <= chain.latest
    ? Effect.succeed(Array.reduce(Array.drop(chain.steps, version - 1), payload, (held, step) => step(held)))
    : Effect.fail(new ParseResult.ParseError({
        issue: new ParseResult.Type(ast, { version, payload }, `<upcast-version:1..${chain.latest}>`),
      }))

const _Column: Schema.Schema<unknown> = Schema.transformOrFail(Schema.Unknown, Schema.Unknown, {
  strict: true,
  decode: (column, _options, ast) =>
    typeof column === "string"
      ? Either.try({ try: (): unknown => JSON.parse(column), catch: () => new ParseResult.Type(ast, column) })
      : ParseResult.succeed(column),
  encode: (value) => ParseResult.succeed(value),
})

const Upcast = {
  Column: _Column,
  json: <A, I>(shape: Schema.Schema<A, I>): Schema.Schema<A, unknown> =>
    Schema.compose(_Column, shape, { strict: false }),
  plan: <A, I>(family: Schema.Schema<A, I>, roster: Upcast.Roster): Either.Either<Upcast.Plan<A>, ChainIncomplete> =>
    Either.map(Either.all(Record.map(roster, (chain, tag) => _sized(tag, chain))), (chains) => {
      const admit = Schema.decodeUnknown(family)
      return {
        census: Array.map(Record.toEntries(chains), ([tag, chain]) => [tag, chain.latest] as const),
        latest: (tag) => Option.map(Record.get(chains, tag), (chain) => chain.latest),
        decode: (raw) =>
          Option.match(Record.get(chains, raw.tag), {
            // the unplanned tag stays on the one admission rail: a minted ParseError, quarantine-routable like any malformed row
            onNone: () => Effect.fail(new ParseResult.ParseError({ issue: new ParseResult.Type(family.ast, raw, `<upcast-unknown-tag:${raw.tag}>`) })),
            onSome: (chain) => Effect.flatMap(_lift(family.ast, chain, raw.version, raw.payload), admit),
          }),
      }
    }),
  chain: <A, I>(shape: Schema.Schema<A, I>, spec: Upcast.Chain): Either.Either<Upcast.Lift<A>, ChainIncomplete> =>
    Either.map(_sized("<snapshot>", spec), (chain) => {
      const admit = Schema.decodeUnknown(shape)
      return {
        latest: chain.latest,
        decode: (version, payload) => Effect.flatMap(_lift(shape.ast, chain, version, payload), admit),
      }
    }),
} as const
```

## [04]-[SNAPSHOT_ROW]

- Owner: `Snapshot.of(spec)` — binds one state schema plus its `Upcast.Lift` and yields `{ save, load }` over the neutral `SqlClient`; the `journal_snapshot` ensure row with its latest-only primary key.
- Packages: `effect` (`Effect`, `Option`, `Schema`); `@effect/sql` (`SqlClient`, `SqlSchema` — the load decodes through a `Result` schema whose `body` field is `Upcast.Column`, so no snapshot cell is ever hand-coerced); the monotonic upsert is one dialect-shared statement because both engines carry the same `ON CONFLICT … DO UPDATE … WHERE` form.
- Entry: `bound.save(stream, state, version)` and `bound.load(stream)` — the only snapshot road; projection lanes and rebuilds compose these, and nothing else touches the table.
- Receipt: `load` yields `Option<{ state, version }>` — present means fold-from-`version + 1`, absent means replay from origin; the option IS the protocol.
- Growth: a state reshape is one `Upcast.Chain` step plus a bumped `latest` stamped on subsequent saves; a second snapshotted shape for one stream family is a second `Snapshot.of` binding, never a widened row.
- Law: the snapshot is a projection — latest-per-stream folded state addressed by the same `StreamKey`, rebuilt from the journal at will; its authority is zero and its value is read cost.
- Law: the upsert is monotonic — `WHERE excluded.version > journal_snapshot.version` — a stale snapshotter racing a fresh one commits nothing, so cadence needs no coordination.
- Law: `snapshot_schema_version` is stamped from `lift.latest` at save and consumed by `lift.decode` at load — write coordinate and read fold share one anchor exactly as events do.
- Law: a load whose body fails the lift is `ParseError` on the admission rail — the consuming lane discards the snapshot and replays; corruption degrades to cost, never to wrong state.
- Boundary: the C#-minted snapshot header arrives decoded through the interchange codec and lands here as an ordinary save by its consuming lane — this page never re-decodes wire bytes.

```typescript signature
import { SqlClient, SqlSchema, type SqlError } from "@effect/sql"
import type { Capability } from "../lane/capability.ts"
import { Journal, StreamKey } from "./append.ts"

declare namespace Snapshot {
  type Spec<S, I> = {
    readonly state: Schema.Schema<S, I>
    readonly lift: Upcast.Lift<S>
  }
  type Held<S> = {
    readonly state: S
    readonly version: number
  }
}

const _ddl: Capability.Ensure = {
  relation: "journal_snapshot",
  pg: `CREATE TABLE IF NOT EXISTS journal_snapshot (
    app TEXT NOT NULL, tenant TEXT NOT NULL, aggregate TEXT NOT NULL,
    version BIGINT NOT NULL,
    snapshot_schema_version INT NOT NULL,
    body JSONB NOT NULL,
    taken_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (app, tenant, aggregate));`,
  sqlite: `CREATE TABLE IF NOT EXISTS journal_snapshot (
    app TEXT NOT NULL, tenant TEXT NOT NULL, aggregate TEXT NOT NULL,
    version INTEGER NOT NULL,
    snapshot_schema_version INTEGER NOT NULL,
    body TEXT NOT NULL,
    taken_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
    PRIMARY KEY (app, tenant, aggregate));`,
}

const _save = <S, I>(spec: Snapshot.Spec<S, I>) =>
  (stream: StreamKey, state: S, version: number) =>
    Effect.gen(function* () {
      const sql = yield* SqlClient.SqlClient
      const body = yield* Schema.encode(Schema.parseJson(spec.state))(state)
      yield* sql`INSERT INTO journal_snapshot ${sql.insert([{
        app: stream.app,
        tenant: stream.tenant,
        aggregate: stream.aggregate,
        version,
        snapshot_schema_version: spec.lift.latest,
        body,
      }])} ON CONFLICT (app, tenant, aggregate) DO UPDATE
        SET version = excluded.version, snapshot_schema_version = excluded.snapshot_schema_version,
            body = excluded.body, taken_at = ${Journal.now(sql)}
        WHERE excluded.version > journal_snapshot.version`
    })

const _SnapshotRow = Schema.Struct({
  version: Journal.Version,
  snapshot_schema_version: Schema.Int.pipe(Schema.positive()),
  body: Upcast.Column,
})

const _load = <S, I>(spec: Snapshot.Spec<S, I>) =>
  (stream: StreamKey) =>
    Effect.gen(function* () {
      const sql = yield* SqlClient.SqlClient
      const found = SqlSchema.findOne({
        Request: StreamKey,
        Result: _SnapshotRow,
        execute: (key) =>
          sql`SELECT version, snapshot_schema_version, body FROM journal_snapshot
              WHERE app = ${key.app} AND tenant = ${key.tenant} AND aggregate = ${key.aggregate}`,
      })
      return yield* Effect.transposeOption(
        Option.map(yield* found(stream), (row) =>
          Effect.map(
            spec.lift.decode(row.snapshot_schema_version, row.body),
            (state): Snapshot.Held<S> => ({ state, version: row.version }),
          )))
    })
```

## [05]-[HYDRATE]

- Owner: the admitted `Snapshot.Cadence` policy, the `due` cadence fold, and `hydrate` — snapshot-plus-tail is one load: the option folds to a seed and a `from` window, the journal read stream folds the tail.
- Packages: `effect` (`Stream`); `journal/append.md` (`Journal.of(...).read`).
- Entry: lanes call `Snapshot.due(version, cadence)` after each apply and `bound.save` when it answers true; `Snapshot.hydrate(bound, journal, stream, fold)` is the one state-recovery entry every lane and rebuild composes.
- Growth: a new cadence shape (byte budget, elapsed time) is a field on the policy row consumed inside `due` — the call sites never change.
- Law: cadence is admitted data — `Snapshot.Cadence` proves a positive integer before the modulo fold; snapshotting is always safe to skip and safe to repeat, so `due` is pure and no lane coordinates with another.

```typescript signature
import { Stream } from "effect"

const _Cadence = Schema.Struct({ every: Schema.Int.pipe(Schema.positive()) })

declare namespace Snapshot {
  type Cadence = typeof _Cadence.Type
  type Bound<S> = {
    readonly save: (stream: StreamKey, state: S, version: number) => Effect.Effect<
      void,
      SqlError.SqlError | ParseResult.ParseError,
      SqlClient.SqlClient
    >
    readonly load: (stream: StreamKey) => Effect.Effect<
      Option.Option<Held<S>>,
      SqlError.SqlError | ParseResult.ParseError,
      SqlClient.SqlClient
    >
  }
}

const _due = (version: number, cadence: Snapshot.Cadence): boolean =>
  version > 0 && version % cadence.every === 0

const _hydrate = <S, A extends Journal.Event>(
  bound: Snapshot.Bound<S>,
  journal: ReturnType<typeof Journal.of<A, unknown>>,
  stream: StreamKey,
  fold: { readonly seed: S; readonly step: (state: S, event: A) => S },
) =>
  Effect.gen(function* () {
    const held = yield* bound.load(stream)
    const origin = Option.match(held, {
      onNone: () => ({ state: fold.seed, from: 1 }),
      onSome: (row) => ({ state: row.state, from: row.version + 1 }),
    })
    return yield* Stream.runFold(
      journal.read(stream, { from: origin.from }),
      origin.state,
      fold.step,
    )
  })

const Snapshot = {
  of: <S, I>(spec: Snapshot.Spec<S, I>): Snapshot.Bound<S> => ({ save: _save(spec), load: _load(spec) }),
  Cadence: _Cadence,
  due: _due,
  hydrate: _hydrate,
  ddl: [_ddl],
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { ChainIncomplete, Snapshot, Upcast }
```
