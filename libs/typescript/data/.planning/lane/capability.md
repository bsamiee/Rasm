# [DATA_CAPABILITY]

This folder's fail-closed capability rail: one closed row/ensure vocabulary, one `Capability` service that proves every extension row, every declared relation, and every dependency demand at Layer construction through two roster-batched probes, and one fault family for everything a probe refuses. Nothing in the folder assumes an extension, a version floor, or a relation — presence is proven or the capability does not exist, and the granted set is a value siblings gate on: `require` fails typed, `when` degrades to `Option`. Extension proof is one `RequestResolver`-batched dialect-honest catalog lookup; relation proof is one schema-qualified census over the complete ensure roster. This same rail carries the DDL split: the provisioning plane applies the idempotent ensure rows, this service proves them at startup, runtime never mutates schema.

## [01]-[INDEX]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                                  |
| :-----: | :--------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `ROW_VOCABULARY` | the row, ensure, and demand shapes, the version-order fold, the one fault family        |
|  [02]   | `BATCHED_PROBE`  | the probe request family and the one-statement `RequestResolver` over the roster        |
|  [03]   | `PROBE_SERVICE`  | the `Capability` service — construction-time proof, granted set, `require`/`when` gates |
|  [04]   | `CONTRACT`       | generated contract decode, canonical identity, local adapter admission, evidence        |

## [02]-[ROW_VOCABULARY]

- Owner: `Capability.Row<G, F>` — the structural bound every `lane/postgres.md` matrix row inhabits, parameterized by its value-derived grant and flag vocabularies; `Capability.Ensure` — the `{relation, pg, sqlite}` DDL row every table-owning page publishes as data; `Capability.Demand<F, G>` — the `[flag, grant]` dependency pair; `Capability.Atomicity` — the closed interactive-transaction versus batch grant; the segment-numeric version order; the one reason-discriminated fault.
- Law: `floor` compares segment-numeric through `_byVersion` over one padded width, so a segment count never outranks a segment value and a real two-segment `extversion` meets an equal three-segment floor; `"0.0.0"` is the presence-only floor and the probe still fails closed on absence. Floor gating itself is a pg fact: the sqlite arm answers presence only, so its admission is membership and no pg-authored floor can refuse a present module.
- Law: ensure rows are authored by the page owning the relation and collected by `lane/tenant.md` at Layer construction; demand pairs are authored by the page owning the matrix (`Pg.demands`). `G` and `F` infer from those values through the service factory, so `require` and `when` accept only the roster's closed grant vocabulary and a misspelled gate is unrepresentable.
- Law: `transaction` admits `SqlClient.withTransaction`; `batch` admits an engine-native atomic batch but refuses an interactive transaction. PostgreSQL, server sqlite, wasm sqlite, and libSQL seed `transaction`; D1 seeds only `batch`, so the generic journal publisher cannot compose on D1 and a D1 publisher must submit its whole write set through the batch boundary. Atomicity is a demandable grant like any other — `Demand`'s grant slot is the full `Grant<G>` vocabulary, so a flagged row may demand `transaction` or `batch` and the fixed-point fold refuses it on D1 exactly as it refuses an absent extension grant.
- Law: one `CapabilityFault` family, reason-discriminated — `absent` (extension not installed), `floor` (installed below floor), `schema` (declared relation missing), `requires` (dependency grant refused); all four route identically (fail or shrink at startup, repair provision), so one class carries them and no policy table is earned.
- Packages: `effect` (`Data`, `Order`, `String`).
- Growth: a new probe posture is a `probeSql` override on the owning matrix row; a new ensure dialect is one field on the ensure shape; a new dependency edge is one demand pair — the shapes never widen per extension.

```typescript signature
import { Array, Data, Order, String, pipe } from "effect"

declare namespace Capability {
  type Row<G extends string = string, F extends string = string> = {
    readonly extension: string
    readonly floor: string
    readonly probeSql?: string
    readonly capabilities: ReadonlyArray<G>
    readonly layer: "image" | "core"
    readonly flags?: ReadonlyArray<F>
  }
  type Ensure = {
    readonly relation: string
    readonly pg: string
    readonly sqlite: string
  }
  type Atomicity = "transaction" | "batch"
  type Grant<G extends string = string> = G | Atomicity
  type Demand<F extends string = string, G extends string = string> = readonly [flag: F, grant: Grant<G>]
  type Reason = "absent" | "floor" | "schema" | "requires"
  type Fault = CapabilityFault
}

class CapabilityFault extends Data.TaggedError("CapabilityFault")<{
  readonly reason: Capability.Reason
  readonly subject: string
  readonly detail: string
}> {}

// `Order.array` compares LENGTH before elements, so a two-segment `extversion` sorts under an equal-valued
// three-segment floor and a real `1.6` refuses against `1.6.0`; padding both operands to one width makes the
// length tiebreak vacuous and leaves segment-wise numeric order deciding, an absent segment reading zero.
const _WIDTH = 4

const _segments = (version: string): ReadonlyArray<number> =>
  pipe(version, String.split("."), (parts) =>
    Array.makeBy(_WIDTH, (index) => Number.parseInt(parts[index] ?? "", 10) || 0))

const _byVersion: Order.Order<string> = Order.mapInput(Order.array(Order.number), _segments)

const _meets = (installed: string, floor: string): boolean =>
  Order.greaterThanOrEqualTo(_byVersion)(installed, floor)
```

## [03]-[BATCHED_PROBE]

- Owner: the `_Probe` request class and `_probeResolver` — one `RequestResolver.makeBatched` that folds every extension lookup queued in a construction pass into a single decoded catalog scan keyed by `sql.in`, resolving each request to the installed version as `Option`; the statement is dialect-honest through `sql.onDialectOrElse` inside the one execute.
- Packages: `effect` (`Effect`, `Option`, `Request`, `RequestResolver`); `@effect/sql` (`SqlClient`, `SqlSchema`, `sql.in`, `sql.onDialectOrElse`).
- Entry: `[4]`'s construction issues `Effect.request(new _Probe({...}), resolver)` per matrix row under `{ batching: true }`, so the whole roster costs one round trip; a sealed per-row `probeSql` override routes through `sql.unsafe` and `SqlSchema.findOne` only when the row declares an exotic probe, with `{ version }` as its required result shape.
- Growth: a new probed catalog (a settings scan, a schema census) is a second request class over the same resolver pattern — the batching law is settled here once.
- Law: PostgreSQL queries `pg_extension` through `sql.in`; SQLite scans `pragma_module_list` for modules installed by `loadExtension`.
- Law: SQLite reports presence as `0.0.0`; version floors remain PostgreSQL facts.
- Law: absent names resolve to `Option.none`; refusal semantics live in the fold, never the statement.
- Law: a statement failure is NOT absence — the window settles every request with the `SqlError` itself, so a broken connection fails Layer construction typed instead of silently shrinking the granted set; fail-closed refuses capabilities on evidence, never on transport accident.
- Law: `sql.unsafe` never meets the batch — the batched form is fully parameterized through `sql.in` and decodes through the one `SqlSchema` scan (the folder's typed-read law holds even on catalog tables); the unsafe escape exists only for sealed per-row overrides, its `{ version }` result still decodes through `SqlSchema`, and the override text is a row of the sealed matrix, never caller input.

```typescript signature
import { Effect, HashMap, Option, Request, RequestResolver, Schema } from "effect"
import type { ParseResult } from "effect"
import { SqlClient, SqlSchema, type SqlError } from "@effect/sql"

class _Probe extends Request.Class<Option.Option<string>, SqlError.SqlError | ParseResult.ParseError, {
  readonly extension: string
}> {}

const _Installed = Schema.Struct({ extname: Schema.String, extversion: Schema.String })

const _probeResolver = (sql: SqlClient.SqlClient): RequestResolver.RequestResolver<_Probe> => {
  const scan = SqlSchema.findAll({
    Request: Schema.Array(Schema.String),
    Result: _Installed,
    execute: (names) =>
      sql.onDialectOrElse({
        orElse: () => sql`SELECT name AS extname, '0.0.0' AS extversion FROM pragma_module_list WHERE ${sql.in("name", names)}`,
        pg: () => sql`SELECT extname, extversion FROM pg_extension WHERE ${sql.in("extname", names)}`,
      }),
  })
  return RequestResolver.makeBatched((requests: ReadonlyArray<_Probe>) =>
    Effect.matchEffect(scan(requests.map((request) => request.extension)), {
      onFailure: (fault) =>
        Effect.forEach(requests, (request) => Request.fail(request, fault), { discard: true }),
      onSuccess: (rows) => {
        const found = HashMap.fromIterable(rows.map((row) => [row.extname, row.extversion] as const))
        return Effect.forEach(
          requests,
          (request) => Request.succeed(request, HashMap.get(found, request.extension)),
          { discard: true },
        )
      },
    }))
}
```

## [04]-[PROBE_SERVICE]

- Owner: the `Capability` service — one vocabulary-parameterized Tag whose `Default(build)` Layer factory batch-probes the extension roster, scans the complete ensure roster at the build's physical schema coordinate in one statement, seeds the granted set with the caller's dialect core and atomicity grants, computes the dependency closure, and publishes the closed granted set, the installed-version report, and the refused evidence.
- Packages: `effect` (`Context.GenericTag`, `Layer.effect`, `HashMap`, `HashSet`, `Array`, `Either`, `Option`); `@effect/sql` (`SqlClient`, `sql.onDialectOrElse`); `lane/postgres.md` (`Pg.rows`, `Pg.core`, `Pg.demands` arrive as arguments, never imports — the service is roster-agnostic).
- Entry: `Capability.Default({ rows: Pg.rows, ensures, core: Pg.core, atomicity: "transaction", demands: Pg.demands, schema: locus.schema })` composes under every pg scope in `lane/tenant.md`; a sqlite profile seeds the grants its own degradation row proves native, an empty demand set, `main`, and `atomicity: profile === "d1" ? "batch" : "transaction"`. Generic journal construction requires `transaction`; D1 composes a batch publisher or routes writes to pg.
- Receipt: `Capability.Report` — `granted`, `versions`, `refused` — the typed probe evidence startup logging and the fact journal consume; a tally beside it restates what the report carries.
- Growth: a consumer needing a new gate composes `require`/`when`, never a second probe path; a new dialect's relation probe is one `onDialectOrElse` arm.
- Law: probes are fail-closed — an absent row is refused, never assumed; a refused ensure relation fails Layer construction (the DDL split was violated), a refused extension row only shrinks the granted set and consumers degrade through `when`.
- Law: demands compute a least fixed point from core grants — each pass admits only rows whose demanded grants already exist in the accepted set, then widens that set with their capabilities; convergence refuses unresolved and cyclic rows with reason `requires`, so one starved row can never satisfy another row transiently. Demand pairs arrive as data, and the deploy plane's image derivation reads the same pairs at provision.
- Law: the Tag carries the caller's grant vocabulary — `Capability.of<Pg.Grant>()` reaches one runtime key with `G` live in the static type, so `require` and `when` accept only that roster's keys; a Tag fixing its service type at the class widens every gate to bare `string` and deletes the closure the roster exists to provide.
- Law: `require` is the hard gate (`Effect<void, CapabilityFault>`), `when` the soft gate (refusal reifies as `Option.none`); a boolean read of the granted set outside these two members is the smuggled knob.
- Law: probing runs once per Layer construction and the report is immutable thereafter — a live re-probe is scope invalidation on the owning `Stores` map, never a poll.
- Boundary: which rows exist is `lane/postgres.md`'s; where the Layer composes and which ensures collect is `lane/tenant.md`'s; the image that makes `"image"` rows probe true is the deployment plane's.

```typescript signature
import { Array, Context, Either, HashSet, Layer } from "effect"

declare namespace Capability {
  type Build<G extends string, F extends string> = {
    readonly rows: ReadonlyArray<Capability.Row<G, F>>
    readonly ensures: ReadonlyArray<Capability.Ensure>
    readonly core: ReadonlyArray<G>
    readonly atomicity: Capability.Atomicity
    readonly demands: ReadonlyArray<Capability.Demand<F, G>>
    readonly schema: string
  }
  type Service<G extends string = string> = {
    readonly report: Capability.Report<G>
    readonly granted: HashSet.HashSet<Capability.Grant<G>>
    readonly require: (key: Capability.Grant<G>) => Effect.Effect<void, CapabilityFault>
    readonly when: <A, E, R>(
      key: Capability.Grant<G>,
      effect: Effect.Effect<A, E, R>,
    ) => Effect.Effect<Option.Option<A>, E, R>
  }
}

// One runtime key, one static vocabulary per caller: `Effect.Service` fixes its service type at the class, so a
// grant union declared on the effect factory never reaches the Tag and every `require` widens to bare `string`,
// erasing exactly the closure the roster exists to provide. `Capability.of<G>()` re-seats it, so a consumer
// naming a gate outside its own roster fails to compile instead of failing closed at run time.
const _KEY = "data/Capability"

const _capability = <G extends string, F extends string>(
  build: Capability.Build<G, F>,
): Effect.Effect<
  Capability.Service<G>,
  CapabilityFault | SqlError.SqlError | ParseResult.ParseError,
  SqlClient.SqlClient
> =>
  Effect.gen(function* () {
    const { atomicity, core, demands, ensures, rows, schema } = build
    const sql = yield* SqlClient.SqlClient
    const resolver = _probeResolver(sql)
    const relations = SqlSchema.findAll({
      Request: Schema.Struct({ schema: Schema.String, relations: Schema.Array(Schema.String) }),
      Result: Schema.Struct({ relation: Schema.String }),
      execute: (coordinate) =>
        sql.onDialectOrElse({
          orElse: () => sql`SELECT name AS relation FROM sqlite_master WHERE ${sql.in("name", coordinate.relations)}`,
          pg: () => sql`SELECT table_name AS relation FROM information_schema.tables
                        WHERE table_schema = ${coordinate.schema} AND ${sql.in("table_name", coordinate.relations)}`,
        }),
    })
    const probed = yield* Effect.forEach(
      rows,
      (row) =>
        Effect.map(
          row.probeSql === undefined
            ? Effect.request(new _Probe({ extension: row.extension }), resolver)
            : SqlSchema.findOne({
                Request: Schema.Void,
                Result: Schema.Struct({ version: Schema.String }),
                execute: () => sql.unsafe(row.probeSql),
              })(undefined).pipe(Effect.map(Option.map((found) => found.version))),
          (version) => [row, version] as const,
        ),
      { batching: true },
    )
    const present = yield* (ensures.length === 0
      ? Effect.succeed(HashSet.empty<string>())
      : Effect.map(
          relations({ schema, relations: Array.map(ensures, (ensure) => ensure.relation) }),
          (found) => HashSet.fromIterable(Array.map(found, (row) => row.relation)),
        ))
    const absent = Array.filter(ensures, (ensure) => !HashSet.has(present, ensure.relation))
    yield* Effect.when(
      Effect.fail(new CapabilityFault({
        reason: "schema",
        subject: Array.map(absent, (ensure) => ensure.relation).join(","),
        detail: Array.map(absent, (ensure) =>
          sql.onDialectOrElse({ orElse: () => ensure.sqlite, pg: () => ensure.pg })).join("\n"),
      })),
      () => Array.isNonEmptyReadonlyArray(absent),
    )
    const floored = sql.onDialectOrElse({ orElse: () => false, pg: () => true }) // version floors are pg facts: the sqlite arm admits present modules on membership alone
    const [missing, held] = Array.partitionMap(probed, ([row, version]) =>
      Option.match(version, {
        onNone: () =>
          Either.left(new CapabilityFault({ reason: "absent", subject: row.extension, detail: row.floor })),
        onSome: (installed) =>
          !floored || _meets(installed, row.floor)
            ? Either.right([row, installed] as const)
            : Either.left(new CapabilityFault({ reason: "floor", subject: row.extension, detail: installed })),
      }))
    const resolve = (
      accepted: typeof held,
      pending: typeof held,
    ): readonly [accepted: typeof held, refused: ReadonlyArray<CapabilityFault>] => {
      const available = HashSet.union(
        HashSet.fromIterable<Capability.Grant<G>>([...core, atomicity]),
        HashSet.fromIterable(Array.flatMap(accepted, ([row]) => row.capabilities)),
      )
      const [waiting, ready] = Array.partitionMap(pending, (entry) => {
        const [row] = entry
        const unmet = Array.findFirst(demands, ([flag, grant]) =>
          Array.contains(row.flags, flag) && !HashSet.has(available, grant))
        return Option.isNone(unmet) ? Either.right(entry) : Either.left(entry)
      })
      return ready.length > 0
        ? resolve([...accepted, ...ready], waiting)
        : [
            accepted,
            Array.map(waiting, ([row]) => {
              const demand = Array.findFirst(demands, ([flag, grant]) =>
                Array.contains(row.flags, flag) && !HashSet.has(available, grant))
              return new CapabilityFault({
                reason: "requires",
                subject: row.extension,
                detail: Option.match(demand, { onNone: () => "<cycle>", onSome: ([, grant]) => grant }),
              })
            }),
          ]
    }
    const [granted, starved] = resolve([], held)
    const report: Capability.Report<G> = {
      granted: HashSet.union(
        HashSet.fromIterable<Capability.Grant<G>>([...core, atomicity]),
        HashSet.fromIterable(Array.flatMap(granted, ([row]) => row.capabilities)),
      ),
      versions: HashMap.fromIterable(Array.map(granted, ([row, installed]) => [row.extension, installed] as const)),
      refused: [...missing, ...starved],
    }
    return {
      report,
      granted: report.granted,
      require: (key: Capability.Grant<G>): Effect.Effect<void, CapabilityFault> =>
        HashSet.has(report.granted, key)
          ? Effect.void
          : Effect.fail(new CapabilityFault({ reason: "absent", subject: key, detail: "<ungranted>" })),
      when: <A, E, R>(key: Capability.Grant<G>, effect: Effect.Effect<A, E, R>): Effect.Effect<Option.Option<A>, E, R> =>
        Effect.when(effect, () => HashSet.has(report.granted, key)),
    }
  })

const Capability = {
  of: <G extends string = string>(): Context.Tag<Capability.Service<G>, Capability.Service<G>> =>
    Context.GenericTag<Capability.Service<G>, Capability.Service<G>>(_KEY),
  make: _capability,
  Default: <G extends string, F extends string>(
    build: Capability.Build<G, F>,
  ): Layer.Layer<
    Capability.Service<G>,
    CapabilityFault | SqlError.SqlError | ParseResult.ParseError,
    SqlClient.SqlClient
  > => Layer.effect(Capability.of<G>(), _capability(build)),
} as const

declare namespace Capability {
  type Report<G extends string = string> = {
    readonly granted: HashSet.HashSet<Capability.Grant<G>>
    readonly versions: HashMap.HashMap<string, string>
    readonly refused: ReadonlyArray<CapabilityFault>
  }
}

```

## [05]-[CONTRACT]

- Owner: `Backend.compose` mints this branch's own contribution; `Backend.merge` folds contributions into the deployment unit; `Backend.project` decodes a foreign branch's contribution; `Backend.observe` maps local evidence; `Backend.admit` joins expected against observed.
- Cases: Effect SQL migrations, journal DDL, the PGLite generation, and object-plane ensures each land as one `Source` row carrying key, role, bytes, providers, and dependencies — the branch composes from its own artifacts alone.
- Law: compose encodes the contract ONCE and digests the bytes it just framed; `project` reads transported octets and never re-encodes to compare, because `JSON.stringify` is canonical in no runtime but its own. `JSONSchema.make` derives the schema artifact from the same AST, so the encode and the schema cannot drift.
- Law: core `Digest.mint("content", bytes)` mints the generation over the canonical UTF-8 artifact; data imports no hash package and reads no peer's digest.
- Law: contributions union by artifact key under `_byKey`, artifact and capability rows alike; a key two branches claim with differing content raises `collision`, so neither first-wins nor last-wins resolves one and the merged generation re-mints from the union.
- Law: artifact key order is the whole wire order — a dependency-depth or topological rank inside the stream mints a second generation from one artifact set, so `_projection` proves the dependency keys and no path validates by sorting.
- Law: `failureRank` and `restartClass` decode as closed literals whose tokens are corpus law; `_RESTART_ORDER` declares the restart tokens in rank order and its index IS the rank, so `Backend.worst` folds an aggregated repair to the worst disruption across its gap set rather than the least.
- Law: provider rows map the contract's canonical capability key onto the GRANT that proves it, and the granted set is the one membership value observation reads; a probed-version lookup answers floor evidence alone, and a semantic fallback proves nothing.
- Law: generation, required grants, and artifact observations all hold before `Backend.Generation` exists, each proved against the corpus and the transported bytes rather than a local re-encode.
- Packages: `effect` Schema, JSONSchema, collections, and rails; core `ContentKey` and `Digest`.
- Growth: a provider adds adapter rows over its existing matrix; a new invariant is one `_Check` row; a generated field changes the one projection schema.
- Boundary: a TypeScript-only application composes, merges, deploys, and admits its backend with no peer branch present; desired rows and local availability never count as realized evidence.

```typescript signature
import { ContentKey, Digest } from "@rasm/ts/core"
import { Array, Data, Effect, Equivalence, HashSet, JSONSchema, Order, Schema, String } from "effect"

// --- [TYPES] ----------------------------------------------------------------------------

const _ArtifactWire = Schema.Struct({
  key: Schema.String,
  role: Schema.String,
  content: Schema.String,
  providers: Schema.Array(Schema.String),
  dependsOn: Schema.Array(Schema.String),
})

// Both vocabularies are closed corpus law, so the decoder refuses a foreign token at the wire instead of
// carrying it inward as a string a downstream comparison silently misses. `_RESTART_ORDER` is declared in rank
// order and its index IS the rank: an aggregated repair folds the WORST disruption across its gap set, so a
// token set read as unordered would report a per-row minimum that understates the bounce the operator pays.
const _RESTART_ORDER = ["session", "reload", "restart"] as const

const _CapabilityWire = Schema.Struct({
  key: Schema.String,
  lane: Schema.String,
  requirement: Schema.String,
  requirementValue: Schema.String,
  failureRank: Schema.Literal("required", "degradable", "observational"),
  restartClass: Schema.Literal(..._RESTART_ORDER),
})

const _ContractWire = Schema.Struct({
  contract: Schema.String,
  artifacts: Schema.Array(_ArtifactWire),
  capabilities: Schema.Array(_CapabilityWire),
})

const _ConformanceWire = Schema.Struct({
  contract: Schema.String,
  generation: ContentKey,
  canonical: Schema.Uint8ArrayFromBase64,
  jsonSchema: Schema.Uint8ArrayFromBase64,
  artifactKeys: Schema.Array(Schema.String),
  capabilityKeys: Schema.Array(Schema.String),
  requiredCapabilities: Schema.Array(Schema.String),
})

const _ContractJson = Schema.parseJson(_ContractWire)
const _ConformanceJson = Schema.parseJson(_ConformanceWire)

// --- [CONSTANTS] ------------------------------------------------------------------------

const _utf8 = { decode: new TextDecoder(), encode: new TextEncoder() } as const
// Provisioning names this rank required — the one value promoting a capability row into the
// required set both this branch's compose and a foreign branch's conformance corpus report.
const _REQUIRED = "required" satisfies Backend.FailureRank
const _keys = Array.getEquivalence(String.Equivalence)
// Uint8Array carries no structural Equal instance, so Equal.equals answers reference identity and
// every byte comparison over distinct buffers passes vacuously; the byte-wise row is the honest one.
const _bytes: Equivalence.Equivalence<Uint8Array> = Equivalence.make(
  (self, that) => self.length === that.length && self.every((byte, index) => byte === that[index]),
)
const _byKey = Order.mapInput(String.Order, (row: { readonly key: string }) => row.key)
const _sameKey = (self: { readonly key: string }, that: { readonly key: string }): boolean =>
  self.key === that.key

// --- [ERRORS] ---------------------------------------------------------------------------

// `dependency` carries both dependency-key refusals — an edge naming no artifact in the set, and a set whose
// edges admit no order — because each names the same broken payload and the subjects name which keys broke it.
class BackendFault extends Data.TaggedError("BackendFault")<{
  readonly reason: "wire" | "identity" | "capability" | "artifact" | "dependency" | "collision"
  readonly subjects: ReadonlyArray<string>
}> {}

// --- [MODELS] ---------------------------------------------------------------------------

declare namespace Backend {
  type FailureRank = typeof _CapabilityWire.Type["failureRank"]
  type RestartClass = typeof _CapabilityWire.Type["restartClass"]
  type Files = {
    readonly contract: Uint8Array
    readonly schema: Uint8Array
    readonly conformance: Uint8Array
  }
  type Adapter = {
    readonly canonical: string
    readonly local: string
  }
  type Source = {
    readonly key: string
    readonly role: string
    readonly bytes: Uint8Array
    readonly providers: ReadonlyArray<string>
    readonly dependsOn: ReadonlyArray<string>
  }
  type Sources = {
    readonly contract: string
    readonly artifacts: ReadonlyArray<Source>
    readonly capabilities: ReadonlyArray<typeof _CapabilityWire.Type>
  }
  type Contract = {
    readonly id: ContentKey
    readonly wire: typeof _ContractWire.Type
    readonly required: HashSet.HashSet<string>
    readonly artifacts: HashSet.HashSet<string>
  }
  type Projection = {
    readonly contract: Contract
    readonly files: Files
  }
  type Observation = {
    readonly generation: ContentKey
    readonly capabilities: HashSet.HashSet<string>
    readonly artifacts: HashSet.HashSet<string>
  }
  type Generation = Contract & { readonly observed: Observation }
}

// --- [OPERATIONS] -----------------------------------------------------------------------

// One discriminant table carries every contract invariant: each row names the fault reason it
// raises, the subjects it reports, and the predicate that must hold — so a new invariant is one
// row and admission stays one traversal instead of a per-check `Effect.when` ladder whose reason
// collapses onto whichever tag the first arm happened to carry.
type _Check = {
  readonly reason: BackendFault["reason"]
  readonly holds: () => boolean
  readonly subjects: () => ReadonlyArray<string>
}

const _prove = (checks: ReadonlyArray<_Check>): Effect.Effect<void, BackendFault> =>
  Effect.forEach(
    checks,
    (row) =>
      row.holds()
        ? Effect.void
        : Effect.fail(new BackendFault({ reason: row.reason, subjects: row.subjects() })),
    { discard: true },
  )

// Worst disruption across a gap set, read off the declared rank order — an aggregated repair names ONE bounce
// cost so the operator never reads a per-row minimum understating it, and an empty set is the floor rank.
const _worst = (over: ReadonlyArray<Backend.RestartClass>): Backend.RestartClass =>
  Array.reduce(over, _RESTART_ORDER[0], (held: Backend.RestartClass, next) =>
    _RESTART_ORDER.indexOf(next) > _RESTART_ORDER.indexOf(held) ? next : held)

const _wire = <A, I>(
  schema: Schema.Schema<A, I>,
  subject: string,
): ((encoded: I) => Effect.Effect<A, BackendFault>) =>
(encoded) =>
  Schema.decode(schema)(encoded).pipe(
    Effect.mapError(() => new BackendFault({ reason: "wire", subjects: [subject] })),
  )

const _canonical = (wire: typeof _ContractWire.Type): Effect.Effect<Uint8Array, BackendFault> =>
  Schema.encode(_ContractJson)(wire).pipe(
    Effect.mapBoth({
      onFailure: () => new BackendFault({ reason: "wire", subjects: ["canonical"] }),
      onSuccess: (json) => _utf8.encode.encode(json),
    }),
  )

// Kahn peel over the artifact set: each pass releases every row whose dependencies already left, and a pass
// releasing nothing while rows remain names exactly the cycle members. Acyclicity therefore costs no traversal
// state and no throw, and the remainder IS the witness the fault reports.
const _cyclic = (
  rows: ReadonlyArray<typeof _ArtifactWire.Type>,
): ReadonlyArray<string> => {
  const peel = (
    held: ReadonlyArray<typeof _ArtifactWire.Type>,
    settled: HashSet.HashSet<string>,
  ): ReadonlyArray<string> => {
    const [waiting, ready] = Array.partition(held, (row) =>
      row.dependsOn.every((key) => HashSet.has(settled, key)))
    return ready.length === 0
      ? Array.map(waiting, (row) => row.key)
      : peel(waiting, HashSet.union(settled, HashSet.fromIterable(Array.map(ready, (row) => row.key))))
  }
  return peel(rows, HashSet.empty())
}

// `_distinct` marks every row distinct, so two rows sharing one key never agree their way out — a repeated key
// inside ONE contribution is malformed whatever its content carries, while a cross-contribution scan passes its
// own content equivalence.
const _distinct: Equivalence.Equivalence<{ readonly key: string }> = Equivalence.make(() => false)

// One key whose rows disagree under the given mark is a collision; deduplication downstream is then safe
// because every surviving group agrees, so first-wins and last-wins both decide nothing.
const _collided = <A extends { readonly key: string }>(
  rows: ReadonlyArray<A>,
  same: Equivalence.Equivalence<A>,
): ReadonlyArray<string> =>
  Array.dedupe(Array.map(
    Array.filter(rows, (row, index) =>
      Array.some(rows, (other, at) => at !== index && other.key === row.key && !same(other, row))),
    (row) => row.key,
  ))

// Every mint path lands on this one projection, so the dependency proof binds compose and merge alike instead
// of a single path that happened to sort: dependency keys are digest-bearing payload the funnel proves closed
// and acyclic, and artifact key order stays the whole wire order. Proof order is load-bearing — a repeated key
// makes the key set a lie, an out-of-set edge names no row, and the peel reads a closed set.
const _projection = (
  wire: typeof _ContractWire.Type,
): Effect.Effect<Backend.Projection, BackendFault> =>
  Effect.gen(function* () {
    const declared = HashSet.fromIterable(wire.artifacts.map((row) => row.key))
    const repeated = _collided(wire.artifacts, _distinct)
    const dangling = wire.artifacts.flatMap((row) =>
      row.dependsOn.filter((key) => !HashSet.has(declared, key)))
    const cyclic = () => _cyclic(wire.artifacts)

    yield* _prove([
      { reason: "artifact", holds: () => repeated.length === 0, subjects: () => repeated },
      { reason: "dependency", holds: () => dangling.length === 0, subjects: () => dangling },
      { reason: "dependency", holds: () => cyclic().length === 0, subjects: cyclic },
    ])

    const contract = yield* _canonical(wire)
    const id = yield* Digest.mint("content", contract)
    const schema = _utf8.encode.encode(JSON.stringify(JSONSchema.make(_ContractWire)))
    const artifactKeys = wire.artifacts.map((row) => row.key)
    const capabilityKeys = wire.capabilities.map((row) => row.key)
    const required = wire.capabilities
      .filter((row) => row.failureRank === _REQUIRED)
      .map((row) => row.key)
    const conformance = yield* Schema.encode(_ConformanceJson)({
      contract: wire.contract,
      generation: id,
      canonical: contract,
      jsonSchema: schema,
      artifactKeys,
      capabilityKeys,
      requiredCapabilities: required,
    }).pipe(Effect.mapError(() => new BackendFault({ reason: "wire", subjects: ["conformance"] })))

    return {
      contract: {
        id,
        wire,
        required: HashSet.fromIterable(required),
        artifacts: HashSet.fromIterable(artifactKeys),
      },
      files: { contract, schema, conformance: _utf8.encode.encode(conformance) },
    }
  })

const Backend = {
  worst: _worst,
  compose: (sources: Backend.Sources): Effect.Effect<Backend.Projection, BackendFault> =>
    Effect.gen(function* () {
      const artifacts = yield* Effect.forEach(
        Array.sort(sources.artifacts, _byKey),
        (row) =>
          Digest.mint("content", row.bytes).pipe(
            Effect.map((content) => ({
              key: row.key,
              role: row.role,
              content,
              providers: row.providers,
              dependsOn: row.dependsOn,
            })),
          ),
      )
      return yield* _projection({
        contract: sources.contract,
        artifacts,
        capabilities: Array.sort(sources.capabilities, _byKey),
      })
    }),
  merge: (
    contributions: ReadonlyArray<Backend.Projection>,
    contract: string,
  ): Effect.Effect<Backend.Projection, BackendFault> =>
    Effect.gen(function* () {
      const artifacts = contributions.flatMap((one) => [...one.contract.wire.artifacts])
      const capabilities = contributions.flatMap((one) => [...one.contract.wire.capabilities])
      // Artifacts collide on their content digest and capability rows on their WHOLE record, because a lane,
      // requirement, rank, or restart-class disagreement forks the deployment exactly as a byte disagreement
      // does; both refuse, so neither first-wins nor last-wins ever elects a branch.
      const collided = Array.appendAll(
        _collided(artifacts, Equivalence.mapInput(String.Equivalence, (row: typeof _ArtifactWire.Type) => row.content)),
        _collided(capabilities, Schema.equivalence(_CapabilityWire)),
      )

      yield* _prove([{
        reason: "collision",
        holds: () => collided.length === 0,
        subjects: () => collided,
      }])

      return yield* _projection({
        contract,
        artifacts: Array.sort(Array.dedupeWith(artifacts, _sameKey), _byKey),
        capabilities: Array.sort(Array.dedupeWith(capabilities, _sameKey), _byKey),
      })
    }),
  project: (files: Backend.Files): Effect.Effect<Backend.Projection, BackendFault> =>
    Effect.gen(function* () {
      const wire = yield* _wire(_ContractJson, "contract")(_utf8.decode.decode(files.contract))
      const corpus = yield* _wire(_ConformanceJson, "conformance")(
        _utf8.decode.decode(files.conformance),
      )
      const id = yield* Digest.mint("content", files.contract)
      const capabilityKeys = wire.capabilities.map((row) => row.key)
      const artifactKeys = wire.artifacts.map((row) => row.key)

      yield* _prove([
        { reason: "identity", holds: () => id === corpus.generation, subjects: () => [id, corpus.generation] },
        { reason: "identity", holds: () => corpus.contract === wire.contract, subjects: () => [corpus.contract, wire.contract] },
        { reason: "wire", holds: () => _bytes(corpus.canonical, files.contract), subjects: () => ["corpus.canonical"] },
        { reason: "wire", holds: () => _bytes(corpus.jsonSchema, files.schema), subjects: () => ["corpus.jsonSchema"] },
        { reason: "capability", holds: () => _keys(corpus.capabilityKeys, capabilityKeys), subjects: () => capabilityKeys },
        { reason: "artifact", holds: () => _keys(corpus.artifactKeys, artifactKeys), subjects: () => artifactKeys },
      ])

      return {
        contract: {
          id,
          wire,
          required: HashSet.fromIterable(corpus.requiredCapabilities),
          artifacts: HashSet.fromIterable(artifactKeys),
        },
        files,
      }
    }),
  // `granted` is the one authoritative membership value the probe publishes — the closed grant vocabulary
  // holding core spine keys, primitives, atomicity, and every capability an admitted extension carries.
  // `versions` keys on the probed EXTENSION name and answers floor evidence alone, so an adapter resolved
  // against it reports every core-seeded grant missing and every extension whose grant name differs from its
  // own. An adapter therefore maps the contract's canonical capability key onto the grant that proves it.
  observe: <G extends string>(
    generation: ContentKey,
    report: Capability.Report<G>,
    adapters: ReadonlyArray<Backend.Adapter>,
    observedArtifacts: HashSet.HashSet<string>,
  ): Backend.Observation => ({
    generation,
    capabilities: HashSet.fromIterable(
      adapters
        .filter((row) => HashSet.has(report.granted, row.local as Capability.Grant<G>))
        .map((row) => row.canonical),
    ),
    artifacts: observedArtifacts,
  }),
  admit: (
    expected: Backend.Contract,
    observed: Backend.Observation,
  ): Effect.Effect<Backend.Generation, BackendFault> =>
    Effect.gen(function* () {
      const missingCapabilities = HashSet.difference(expected.required, observed.capabilities)
      const missingArtifacts = HashSet.difference(expected.artifacts, observed.artifacts)

      yield* _prove([
        {
          reason: "identity",
          holds: () => expected.id === observed.generation,
          subjects: () => [expected.id, observed.generation],
        },
        {
          reason: "capability",
          holds: () => HashSet.size(missingCapabilities) === 0,
          subjects: () => [...missingCapabilities],
        },
        {
          reason: "artifact",
          holds: () => HashSet.size(missingArtifacts) === 0,
          subjects: () => [...missingArtifacts],
        },
      ])

      return {
        ...expected,
        observed,
      }
    }),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Backend, BackendFault, Capability, CapabilityFault }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
