# [DATA_CAPABILITY]

One fail-closed rail carries this folder: a closed row/ensure vocabulary, one `Capability` service proving every extension row, relation, and demand at Layer construction, and one fault family for its refusals. Nothing assumes an extension, a version floor, or a relation — presence proves or the capability does not exist, and siblings gate on the granted set through `require` and `when`. `Backend` composes that proven catalogue into one generation and grades a restored store on two proofs: identity that it carries the generation, recency that its frontier holds the declared window.

## [01]-[INDEX]

- [02]-[ROW_VOCABULARY]: row, ensure, and demand shapes, the two demand relations, the version-order fold, one fault family.
- [03]-[BATCHED_PROBE]: probe request family and the one-statement `RequestResolver` over the roster.
- [04]-[PROBE_SERVICE]: `Capability` service construction-time proof, corner phase, granted set, `require`/`when` gates.
- [05]-[CONTRACT]: generated contract admission, semantic identity, local adapter admission, staged accumulating admission, recovery grading.

## [02]-[ROW_VOCABULARY]

- Owner: `Capability.Row<G, F>` — the structural bound every `lane/postgres.md` matrix row inhabits, parameterized by its value-derived grant and flag vocabularies; `Capability.Ensure` — the `{relation, pg, sqlite}` DDL row every table-owning page publishes as data; `Capability.Demand<F, G>` — the `{relation, flag, grant}` dependency row over the `_RELATIONS` table; `Capability.Atomicity` — the closed interactive-transaction versus batch grant; the segment-numeric version order; the one reason-discriminated fault family and the `Capability.Issue` payload every refusal carries.
- Law: `floor` compares segment-numeric through `_byVersion` over one padded width, so a segment count never outranks a segment value and a real two-segment `extversion` meets an equal three-segment floor; `"0.0.0"` is the presence-only floor and the probe still fails closed on absence. Floor gating itself is a pg fact: the sqlite arm answers presence only, so its admission is membership and no pg-authored floor can refuse a present module.
- Law: ensure rows are authored by the page owning the relation and collected by `lane/tenant.md` at Layer construction; demand rows are authored by the page owning the matrix (`Pg.demands`). `G` and `F` infer from those values through the service factory, so `require` and `when` accept only the roster's closed grant vocabulary and a misspelled gate is unrepresentable.
- Law: dependency is TWO relations over ONE row — `requires` names a grant a flagged row cannot run without, `excludes` names a grant it cannot run beside — and `_RELATIONS` carries each relation's unmet predicate beside the issue it mints, so the two arms differ by one negation held in a table rather than by two hand-written bodies, and a third relation is one row. An algebra spelling implication alone leaves every mutual-exclusion corner unspellable, which is exactly how a corner survives on a matrix row as prose no gate reads.
- Law: the excluded member must be a `Grant<G>` the matrix itself admits — an exclusion naming an engine no row grants is unspellable at the producer and unprobeable at the gate, because the probe reads the row roster and nothing else, so a corner against an unadmitted engine is documentation rather than law.
- Law: `transaction` admits `SqlClient.withTransaction`; `batch` admits an engine-native atomic batch but refuses an interactive transaction. PostgreSQL, server sqlite, wasm sqlite, and libSQL seed `transaction`; D1 seeds only `batch`, so the generic journal publisher cannot compose on D1 and a D1 publisher must submit its whole write set through the batch boundary. Atomicity is a demandable grant like any other — `Demand`'s grant slot is the full `Grant<G>` vocabulary, so a flagged row may demand `transaction` or `batch` and the fixed-point fold refuses it on D1 exactly as it refuses an absent extension grant.
- Law: `CapabilityFault` is `_family.census("CapabilityFault")` — every refusal is an `Issue` its own family row renders, the census carries a `NonEmptyArray` of them, and class, leg, and both recovery axes follow the DOMINANT issue's own class row; core class rows own retryability and blame, and a free-string `subject`/`detail` pair beside a closed `reason` re-opens the axis the reason already closed.
- Packages: `effect` (`Array`, `HashSet`, `Order`, `Schema`, `String`); `@rasm/core` (`Fault.Class`).
- Growth: a new probe posture is a `probeSql` override on the owning matrix row; a new ensure dialect is one field on the ensure shape; a new dependency edge is one demand row and a new dependency relation is one `_RELATIONS` row — the shapes never widen per extension.

```typescript signature
import { Array, HashSet, Order, Schema, String, pipe } from "effect"
import { Fault } from "@rasm/core"

// Every refusal on this family raises from the one probe surface, so the leg is one anchor rather than a word each
// row re-spells; the census reads it back off whichever issue dominates. The anchor is the bare SURFACE SEGMENT — a
// module path or a section token restates coordinates the page already is, and a census carrying them reads a file
// tree where it must read a seam.
const _LEG = "probe"

// One row per reason and the ROW is the whole declaration: the core kind it grades as, the leg it refuses at, the
// subject record every raise supplies, and the renderer over that record. Retryability, blame, and quarantine are
// the core Fault.Class row table's — a rank or retry column here would fork that taxonomy into this folder — and a
// free-string detail column would re-open the subject axis `reason` closes, so each row states its own subjects.
const _family = Fault.Class.family(["absent", "floor", "schema", "requires", "conflicts", "ungranted"] as const, {
  absent: Fault.Class.row({
    class: "absent",
    leg: _LEG,
    detail: Schema.Struct({ extension: Schema.NonEmptyString, floor: Schema.NonEmptyString }),
    render: ({ extension, floor }) => `${extension} carries no installed version against floor ${floor}`,
  }),
  floor: Fault.Class.row({
    class: "breached",
    leg: _LEG,
    detail: Schema.Struct({
      extension: Schema.NonEmptyString,
      installed: Schema.NonEmptyString,
      floor: Schema.NonEmptyString,
    }),
    render: ({ extension, installed, floor }) => `${extension} installed ${installed} under floor ${floor}`,
  }),
  schema: Fault.Class.row({
    class: "absent",
    leg: _LEG,
    detail: Schema.Struct({ relation: Schema.NonEmptyString, ddl: Schema.NonEmptyString }),
    render: ({ relation, ddl }) => `${relation} unensured, DDL ${ddl}`,
  }),
  requires: Fault.Class.row({
    class: "denied",
    leg: _LEG,
    detail: Schema.Struct({
      extension: Schema.NonEmptyString,
      flag: Schema.NonEmptyString,
      grant: Schema.NonEmptyString,
    }),
    render: ({ extension, flag, grant }) => `${extension} flagged ${flag} demands ungranted ${grant}`,
  }),
  conflicts: Fault.Class.row({
    class: "denied",
    leg: _LEG,
    detail: Schema.Struct({
      extension: Schema.NonEmptyString,
      flag: Schema.NonEmptyString,
      grant: Schema.NonEmptyString,
    }),
    render: ({ extension, flag, grant }) => `${extension} flagged ${flag} excludes granted ${grant}`,
  }),
  ungranted: Fault.Class.row({
    class: "absent",
    leg: _LEG,
    detail: Schema.Struct({ grant: Schema.NonEmptyString }),
    render: ({ grant }) => `${grant} sits outside the granted set`,
  }),
})

// The family's OWN accumulating-admission carrier: rows admitted independently census every offender in one refusal,
// each issue rendered by the row that declared it, and class, leg, and both recovery axes follow the dominant issue
// rather than whichever refusal was raised first.
class CapabilityFault extends _family.census("CapabilityFault") {}

// Both relations carry the SAME triple and differ in one thing — what makes a demand unmet — so the negation lives in
// a table column and each row also mints its own issue, which is what lets the requires arm and the corner arm share
// one traversal, one refusal constructor, and one flag-membership test.
type _Corner = { readonly extension: string; readonly flag: string; readonly grant: string }

const _RELATIONS = {
  requires: {
    unmet: <A>(available: HashSet.HashSet<A>, grant: A): boolean => !HashSet.has(available, grant),
    issue: (corner: _Corner): Capability.Issue => ({ reason: "requires", ...corner }),
  },
  excludes: {
    unmet: <A>(available: HashSet.HashSet<A>, grant: A): boolean => HashSet.has(available, grant),
    issue: (corner: _Corner): Capability.Issue => ({ reason: "conflicts", ...corner }),
  },
} as const satisfies {
  readonly [relation: string]: {
    readonly unmet: <A>(available: HashSet.HashSet<A>, grant: A) => boolean
    readonly issue: (corner: _Corner) => Capability.Issue
  }
}

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
  type Relation = keyof typeof _RELATIONS
  type Demand<F extends string = string, G extends string = string> = {
    readonly relation: Relation
    readonly flag: F
    readonly grant: Grant<G>
  }
  type Issue = typeof _family.payload.Type
  type Fault = CapabilityFault
}

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

- Owner: the `Capability` service — one vocabulary-parameterized Tag whose `Default(build)` Layer factory batch-probes the extension roster, scans the complete ensure roster at the build's physical schema coordinate in one statement, seeds the granted set with the caller's dialect core and atomicity grants, refuses every declared corner against the probed horizon, computes the dependency closure over the survivors, and publishes the closed granted set, the installed-version report, and the refused evidence.
- Packages: `effect` (`Context.GenericTag`, `Layer.effect`, `HashMap`, `HashSet`, `Array`, `Either`, `Option`); `@effect/sql` (`SqlClient`, `sql.onDialectOrElse`); `lane/postgres.md` (`Pg.rows`, `Pg.core`, `Pg.demands` arrive as arguments, never imports — the service is roster-agnostic).
- Entry: `Capability.Default({ rows: Pg.rows, ensures, core: Pg.core, atomicity: "transaction", demands: Pg.demands, schema: locus.schema })` composes under every pg scope in `lane/tenant.md`; a sqlite profile seeds the grants its own degradation row proves native, an empty demand set, `main`, and `atomicity: profile === "d1" ? "batch" : "transaction"`. Generic journal construction requires `transaction`; D1 composes a batch publisher or routes writes to pg.
- Receipt: `Capability.Report` — `granted`, `versions`, `refused` — the typed probe evidence startup logging and the fact journal consume; `refused` carries ISSUE VALUES rather than raised faults, because a refused extension row shrinks the granted set without failing construction and only a gate or an unensured relation raises the census over them; a tally beside the report restates what it already carries.
- Growth: a consumer needing a new gate composes `require`/`when`, never a second probe path; a new dialect's relation probe is one `onDialectOrElse` arm.
- Law: probes are fail-closed — an absent row is refused, never assumed; a refused ensure relation fails Layer construction (the DDL split was violated) carrying ONE issue per unensured relation with that relation's own DDL, so an operator reads every missing relation from one refusal instead of a joined string; a refused extension row only shrinks the granted set and consumers degrade through `when`.
- Law: the relation census reads the CATALOG and never `information_schema` — the standard view carries base tables, views, and foreign tables while omitting materialized views entirely, so an ensure for the incremental-view row the matrix admits never verifies and fails construction forever against a relation that exists; the same view also hides relations the current role holds no privilege on, turning a grant defect into an indistinguishable absence whose fault detail prints DDL an operator then re-runs for nothing. `relkind` names the admitted kinds as a literal set, so a new relation kind is one token.
- Law: the sqlite census filters on relation TYPE — the schema table lists indexes and triggers beside tables, so an unfiltered name match lets an index sharing an ensured relation's name answer for it and construction passes over a table nobody created.
- Law: the two relations resolve in TWO phases because they pull opposite ways under a growing grant set. `excludes` grades ONCE against the probed HORIZON — core grants, atomicity, and every capability a present, above-floor row carries — so a corner refuses on what the store CARRIES, both members of a mutual corner refuse together, and no admission order elects a winner the operator never chose; grading a corner per pass would instead admit a row an early pass found legal and a later, wider pass would have refused.
- Law: `requires` then computes a least fixed point over the corner survivors — each pass admits only rows whose demanded grants already exist in the accepted set, then widens that set with their capabilities; convergence refuses unresolved and cyclic rows with reason `requires`, so one starved row can never satisfy another row transiently. Demand rows arrive as data, and the deploy plane's image derivation reads the same rows at provision.
- Law: the Tag carries the caller's grant vocabulary — `Capability.of<Pg.Grant>()` reaches one runtime key with `G` live in the static type, so `require` and `when` accept only that roster's keys; a Tag fixing its service type at the class widens every gate to bare `string` and deletes the closure the roster exists to provide.
- Law: `require` is the hard gate (`Effect<void, CapabilityFault>`) refusing `ungranted` with the grant it was handed, `when` the soft gate (refusal reifies as `Option.none`); a boolean read of the granted set outside these two members is the smuggled knob, and a sentinel detail standing where a reason belongs hides one refusal inside another's spelling.
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

// Admitted relation kinds, one roster per dialect: ordinary and partitioned tables, plain views, MATERIALIZED
// views (which the standard information view omits outright, so an incremental-view ensure verifies here and
// nowhere else), and foreign tables. Adding a kind is one token.
const _PG_KINDS = ["r", "p", "v", "m", "f"] as const
const _SQLITE_KINDS = ["table", "view"] as const

// One traversal answers both relations: flag membership is the row's half of the pair and the relation's own
// predicate is the grant set's half, so neither arm re-spells the other's negation and neither hard-codes a reason.
const _unmet = <G extends string, F extends string>(
  demands: ReadonlyArray<Capability.Demand<F, G>>,
  relation: Capability.Relation,
  available: HashSet.HashSet<Capability.Grant<G>>,
  row: Capability.Row<G, F>,
): Option.Option<Capability.Demand<F, G>> =>
  Array.findFirst(demands, (demand) =>
    demand.relation === relation
    && Array.contains(row.flags ?? [], demand.flag)
    && _RELATIONS[relation].unmet(available, demand.grant))

// The refused row and its unmet demand ARE the corner, and the relation row carries which reason names it, so the
// starved arm and the excluded arm mint through one constructor instead of forking on a token the row already holds.
const _refusal = <G extends string, F extends string>(
  row: Capability.Row<G, F>,
  demand: Capability.Demand<F, G>,
): Capability.Issue =>
  _RELATIONS[demand.relation].issue({ extension: row.extension, flag: demand.flag, grant: demand.grant })

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
          orElse: () =>
            sql`SELECT name AS relation FROM sqlite_master
                WHERE ${sql.in("type", _SQLITE_KINDS)} AND ${sql.in("name", coordinate.relations)}`,
          pg: () =>
            sql`SELECT c.relname AS relation FROM pg_catalog.pg_class AS c
                JOIN pg_catalog.pg_namespace AS n ON n.oid = c.relnamespace
                WHERE n.nspname = ${coordinate.schema}
                  AND ${sql.in("c.relkind", _PG_KINDS)}
                  AND ${sql.in("c.relname", coordinate.relations)}`,
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
    // Every unensured relation is its own issue carrying its own DDL: the census is the roster, so an operator reads
    // which relations are missing and what to run for each rather than two joined strings they must re-pair by hand.
    yield* Array.match(absent, {
      onEmpty: () => Effect.void,
      onNonEmpty: (unensured) =>
        Effect.fail(new CapabilityFault({
          issues: Array.map(unensured, (ensure) => ({
            reason: "schema",
            relation: ensure.relation,
            ddl: sql.onDialectOrElse({ orElse: () => ensure.sqlite, pg: () => ensure.pg }),
          } satisfies Capability.Issue)),
        })),
    })
    const floored = sql.onDialectOrElse({ orElse: () => false, pg: () => true }) // version floors are pg facts: the sqlite arm admits present modules on membership alone
    const [missing, held] = Array.partitionMap(probed, ([row, version]) =>
      Option.match(version, {
        onNone: () =>
          Either.left({ reason: "absent", extension: row.extension, floor: row.floor } satisfies Capability.Issue),
        onSome: (installed) =>
          !floored || _meets(installed, row.floor)
            ? Either.right([row, installed] as const)
            : Either.left(
              { reason: "floor", extension: row.extension, installed, floor: row.floor } satisfies Capability.Issue,
            ),
      }))
    // The HORIZON is what this store can grant at all — core, atomicity, and every row that answered present above
    // its floor. Corners grade against it ONCE and before admission runs, because an exclusion is a fact of what is
    // INSTALLED rather than of what the fold happened to accept first: two rows excluding each other therefore both
    // refuse and the operator repairs the deployment, where a per-pass grading would admit a row the next pass would
    // have refused and leave the verdict reading off iteration order.
    const horizon = HashSet.union(
      HashSet.fromIterable<Capability.Grant<G>>([...core, atomicity]),
      HashSet.fromIterable(Array.flatMap(held, ([row]) => row.capabilities)),
    )
    const [cornered, legal] = Array.partitionMap(held, (entry) =>
      Option.match(_unmet(demands, "excludes", horizon, entry[0]), {
        onNone: () => Either.right(entry),
        onSome: (demand) => Either.left(_refusal(entry[0], demand)),
      }))
    // Partition arms CARRY each starved row's unmet demand, so the terminal pass reports the demand that starved it
    // without re-running the probe — and a cyclic row is a row whose unmet grant never arrives, so no separate cycle
    // arm exists for a state the carried demand already names.
    const resolve = (
      accepted: typeof held,
      pending: typeof held,
    ): readonly [accepted: typeof held, refused: ReadonlyArray<Capability.Issue>] => {
      const available = HashSet.union(
        HashSet.fromIterable<Capability.Grant<G>>([...core, atomicity]),
        HashSet.fromIterable(Array.flatMap(accepted, ([row]) => row.capabilities)),
      )
      const [starved, ready] = Array.partitionMap(pending, (entry) =>
        Option.match(_unmet(demands, "requires", available, entry[0]), {
          onNone: () => Either.right(entry),
          onSome: (demand) => Either.left([entry, demand] as const),
        }))
      return ready.length > 0
        ? resolve([...accepted, ...ready], Array.map(starved, ([entry]) => entry))
        : [accepted, Array.map(starved, ([[row], demand]) => _refusal(row, demand))]
    }
    const [granted, starved] = resolve([], legal)
    const report: Capability.Report<G> = {
      granted: HashSet.union(
        HashSet.fromIterable<Capability.Grant<G>>([...core, atomicity]),
        HashSet.fromIterable(Array.flatMap(granted, ([row]) => row.capabilities)),
      ),
      versions: HashMap.fromIterable(Array.map(granted, ([row, installed]) => [row.extension, installed] as const)),
      refused: [...missing, ...cornered, ...starved],
    }
    return {
      report,
      granted: report.granted,
      require: (key: Capability.Grant<G>): Effect.Effect<void, CapabilityFault> =>
        HashSet.has(report.granted, key)
          ? Effect.void
          : Effect.fail(new CapabilityFault({ issues: [{ reason: "ungranted", grant: key }] })),
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
    readonly refused: ReadonlyArray<Capability.Issue>
  }
}

```

## [05]-[CONTRACT]

- Owner: `Backend.compose` mints this branch's own contribution; `Backend.merge` folds a non-empty contribution set into the deployment unit and retains the generated document's own contract coordinate; `Backend.project` decodes a foreign branch's contribution; `Backend.observe` maps one local reading — realized catalogue and recovery stamps together — into an observation; `Backend.admit` grades one verdict on the two proofs.
- Cases: the generation script, journal DDL, the PGLite generation, object-plane ensures, and the object custody descriptor each land as one generated `Artifact` carrying key, role, content, providers, and dependencies — the branch composes from its own artifacts alone; the custody row's stable content derives from settled retention and conformance tables with operator coordinates excluded from the preimage, minted at `object/store#CUSTODY_CONTRACT` beside its capability rows and realized-state observation.
- Law: generated `Backend`, `Artifact`, and `Capability` messages are the contract vocabulary; `Format.proto` validates every message against the generated descriptor and protovalidate rules and emits the peer document through ProtoJSON. `CanonicalWriter` derives generation identity from the message's known semantic fields, never from protobuf or JSON serialization.
- Law: ProtoJSON has no canonical byte spelling; `project` retains transported octets only for deployment, admits their semantic message once, and never compares them with a local re-encode. Merge collision checks use generated message equality over `Artifact` and `Capability` values.
- Law: one generated-message projection defines the published order of every set-like repeated field; local and foreign documents must already equal that semantic projection before admission, so artifact, capability, provider, and dependency order cannot mint a second generation from one set.
- Law: core `Digest.mint("content", chunks)` mints the generation over the one framed preimage: contract string; counted artifacts as key, role ordinal, framed content, counted provider ordinals, counted dependency strings; counted capabilities as their six generated fields in tag order. The schema is map-free and float-free, and unknown protobuf residue enters no field call, so map order, NaN spelling, and parser retention cannot fork it.
- Law: contributions union by artifact key under `_byKey`, artifact and capability rows alike; a key two branches claim with differing content raises `collision`, so neither first-wins nor last-wins resolves one and the merged generation re-mints from the union.
- Law: the generated document owns `contract` — `merge` derives the coordinate from the non-empty contribution set and proves every peer carries the same value; a deploy pointer, target name, or registry coordinate never enters the fold, so deployment naming cannot rename a branch contract or make one contribution merge for one target and refuse for another.
- Law: artifact key order is the whole wire order — a dependency-depth or topological rank inside the stream mints a second generation from one artifact set, so `_projection` proves the dependency keys and no path validates by sorting.
- Law: generated `FailureRank` carries the explicit absorption policy and is never ordinal-ranked. `RestartClass` alone carries disruption order; every admitted optional gap survives on `Generation.gaps`, and `restart` folds the worst repair disruption while required gaps still refuse admission.
- Law: `_BACKEND_DOCUMENT_CEILING` rejects transported ProtoJSON before generated decode and emitted ProtoJSON before publication. Its 512 KiB budget sits beneath the 1 MiB ConfigMap residence after base64 and object metadata; descriptor string, content, and repeated-field ceilings remain the constructed-message floor.
- Law: admission ACCUMULATES across independent columns and SEQUENCES across dependent ones, and the shape at the call site is the whole disposition — `_prove` takes stages, censuses every failing row inside one stage through `Effect.validateAll`, and aborts between stages; projection, merge, and admission each grade one stage whose rows read values already in hand, while the artifact proof is three stages because a repeated key makes the key set a lie, an out-of-set edge names no row, and the peel reads a closed set. A first-failure abort over independent columns hands the operator one repair at a time and hides how many the store actually owes.
- Law: one refusal carries every failing column as a typed ISSUE its own family row renders, so a verdict names which proof failed beside its subjects, and class, leg, and both recovery axes follow the DOMINANT issue — a store carrying the wrong generation on a stale window grades on the breach rather than on whichever proof was seated first.
- Law: provider rows map the contract's canonical capability key onto the GRANT that proves it, and the granted set is the one membership value observation reads; a probed-version lookup answers floor evidence alone, and a semantic fallback proves nothing.
- Law: capabilities enter an observation two ways and the split is the probe's REACH — a relational grant resolves through the adapter map against `report.granted`, while a provider plane whose custody no SQL catalog scans hands its canonical keys in already resolved on `Reading.granted`, the mirror of the `artifacts` slot beside it. `Capability.Grant<G>` stays closed against those keys: no probe ever finds a canonical contract key as a grant, and a roster admitting one fails every `require` that names it.
- Law: the composition root builds ONE `Reading` from the relational report and adapters beside every provider plane's observed set — `granted` empty on a purely relational reading — so one `Backend.admit` verdict covers relational and object state together and no plane publishes a second generation against the same contract.
- Law: generation, required grants, and artifact observations all hold before `Backend.Generation` exists, each proved against the admitted generated message rather than a branch-local mirror.
- Law: recovery is ONE verdict on two proofs, never two generations — contract identity proves the store carries the composed generation, and recency proves the frontier behind it is current for the window the deployment declared; both refuse through `BackendFault`, so a stale store never publishes as an admitted generation.
- Law: the measured window derives from the observation's OWN stamps, so a provider hands in the readings it took and never a lag it computed against a clock this owner never saw; a lag admits at ZERO — a frontier stamped at its own reading instant is the freshest measured recency — and only a frontier stamped after that reading is skew grading as unmeasured.
- Law: absence splits opposite ways and the split is a COLUMN on the axis table — an unmeasured recovery point refuses, because a restore admitted with no recency evidence grades a window nobody took, while an absent restore duration passes on a store that never restored; each axis reads its own window half, its own objective half, and its own absence posture from one row, so a third axis is one row rather than a third hand-written arm.
- Law: the objective is a value the composition root supplies off its consumption profile row; this page grades a declared window and owns no durability table of its own.
- Packages: `@bufbuild/protobuf` generated construction and semantic equality; `@rasm\/contracts` parity descriptors and enums; `effect` collections, schemas, and rails; core `Digest.Key`, `Digest`, `Fault.Class`, and `Format.proto`.
- Growth: a provider adds adapter rows over its existing matrix, and a provider plane holding no probeable catalog adds canonical keys on `Reading.granted` instead; a new invariant is one `_Check` row inside the stage that already holds its inputs; a generated field changes the corpus message and regenerated binding; a new recovery axis is one `_AXES` token with its `_WINDOWS` row carrying both readers and its absence posture.
- Boundary: a TypeScript-only application composes, merges, deploys, and admits its backend with no peer branch present; desired rows and local availability never count as realized evidence. Assembling the one `Reading` — relational report and adapters beside every provider plane's already-canonical grants and artifacts — is the composition root's obligation, and this page grades whatever that root hands it.

```typescript signature
import { equals, type MessageInitShape, type MessageValidType } from "@bufbuild/protobuf"
import {
  ArtifactSchema,
  BackendSchema,
  CapabilitySchema,
  FailureRank,
  RestartClass,
} from "@rasm\/contracts/rasm/contracts/parity/parity_pb"
import { CanonicalWriter, Digest, Fault, Format } from "@rasm/core"
import { Array, DateTime, Duration, Effect, HashSet, Option, Order, Schema, String } from "effect"

// --- [TYPES] ----------------------------------------------------------------------------

// The recovery axes are a closed vocabulary before they are a table: the fault row renders against these tokens and
// the policy rows below key on them, so an axis exists in exactly one place and both surfaces move with it.
const _AXES = ["rpo", "rto"] as const

const _codec = {
  message: Format.proto.message(BackendSchema),
  document: Format.proto.frame(BackendSchema, "json"),
} as const

// --- [CONSTANTS] ------------------------------------------------------------------------

const _REQUIRED = FailureRank.REQUIRED satisfies Backend.FailureRank
const _BACKEND_DOCUMENT_CEILING = 512 * 1024
const _byKey = Order.mapInput(String.Order, (row: { readonly key: string }) => row.key)
const _byRestart: Order.Order<Backend.RestartClass> = Order.number
const _sameKey = (self: { readonly key: string }, that: { readonly key: string }): boolean =>
  self.key === that.key

// --- [ERRORS] ---------------------------------------------------------------------------

// Contract refusals raise from one surface and probe refusals from another, so the two families carry two legs and a
// consumer reading `leg` off a census learns which plane refused without re-deriving it from the reason. Both spell
// the bare surface segment, so the two words partition a seam rather than restating the page's own coordinates.
const _BACKEND_LEG = "contract"

// One row per reason and the row states its own subjects: the core kind, the leg it refuses at, the subject record
// every raise supplies, and the renderer over it — so retryability, blame, and quarantine stay the core row table's
// while the shape of the evidence stays this family's. `dependency` carries both dependency-key refusals — an edge
// naming no artifact in the set, and a set whose edges admit no order — because each names the same broken payload
// and the keys name which broke it.
const _backendFamily = Fault.Class.family(
  ["wire", "identity", "capability", "artifact", "dependency", "collision", "recovery"] as const,
  {
    wire: Fault.Class.row({
      class: "malformed",
      leg: _BACKEND_LEG,
      detail: Schema.Struct({ document: Schema.NonEmptyString }),
      render: ({ document }) => `${document} refused at the wire`,
    }),
    identity: Fault.Class.row({
      class: "breached",
      leg: _BACKEND_LEG,
      detail: Schema.Struct({ expected: Schema.NonEmptyString, actual: Schema.NonEmptyString }),
      render: ({ expected, actual }) => `expected ${expected}, observed ${actual}`,
    }),
    capability: Fault.Class.row({
      class: "absent",
      leg: _BACKEND_LEG,
      detail: Schema.Struct({ keys: Schema.Array(Schema.String) }),
      render: ({ keys }) => `capability keys unproved: ${Array.join(keys, ",")}`,
    }),
    artifact: Fault.Class.row({
      class: "invalid",
      leg: _BACKEND_LEG,
      detail: Schema.Struct({ keys: Schema.Array(Schema.String) }),
      render: ({ keys }) => `artifact keys refused: ${Array.join(keys, ",")}`,
    }),
    dependency: Fault.Class.row({
      class: "invalid",
      leg: _BACKEND_LEG,
      detail: Schema.Struct({ keys: Schema.Array(Schema.String) }),
      render: ({ keys }) => `dependency keys refused: ${Array.join(keys, ",")}`,
    }),
    collision: Fault.Class.row({
      class: "conflicted",
      leg: _BACKEND_LEG,
      detail: Schema.Struct({ keys: Schema.Array(Schema.String) }),
      render: ({ keys }) => `keys claimed twice with differing content: ${Array.join(keys, ",")}`,
    }),
    // a store carrying the generation on stale data is recoverable by waiting, never by re-composing: this class puts
    // the refusal on the retry rail instead of the quarantine one, and the row renders the MEASURED bound beside the
    // declared one so an operator reads the gap rather than an axis name they must go re-measure.
    recovery: Fault.Class.row({
      class: "expired",
      leg: _BACKEND_LEG,
      detail: Schema.Struct({
        axis: Schema.Literal(..._AXES),
        measured: Schema.OptionFromSelf(Schema.DurationFromSelf),
        declared: Schema.DurationFromSelf,
      }),
      render: ({ axis, measured, declared }) =>
        Option.match(measured, {
          onNone: () => `${axis} unmeasured against ${Duration.toMillis(declared)}ms`,
          onSome: (held) => `${axis} ${Duration.toMillis(held)}ms over ${Duration.toMillis(declared)}ms`,
        }),
    }),
  },
)

// The census IS the admission carrier: independent proofs report every offender in one refusal, each issue rendered
// by the row that declared it, and class, leg, and both recovery axes follow the dominant issue's own class row.
class BackendFault extends _backendFamily.census("BackendFault") {}

// --- [MODELS] ---------------------------------------------------------------------------

declare namespace Backend {
  type Axis = (typeof _AXES)[number]
  type Artifact = MessageValidType<typeof ArtifactSchema>
  type Capability = MessageValidType<typeof CapabilitySchema>
  type Document = MessageValidType<typeof BackendSchema>
  type FailureRank = Capability["failureRank"]
  type Issue = typeof _backendFamily.payload.Type
  type RestartClass = Capability["restartClass"]
  type Files = {
    readonly contract: Uint8Array
  }
  type Adapter = {
    readonly canonical: string
    readonly local: string
  }
  // Composition input is a required readonly view of the generated init shape. Field selection is deliberate —
  // callers cannot smuggle message internals into this boundary — while every selected field's value type still
  // moves with BackendSchema rather than surviving as a hand-maintained wire twin.
  type Sources = Readonly<Required<Pick<
    MessageInitShape<typeof BackendSchema>,
    "contract" | "artifacts" | "capabilities"
  >>>
  type Contract = {
    readonly id: Digest.Key<"content">
    readonly document: Document
    readonly required: HashSet.HashSet<string>
    readonly artifacts: HashSet.HashSet<string>
  }
  type Projection = {
    readonly contract: Contract
    readonly files: Files
  }
  // DECLARED durability window a measured one is graded against — how much data a restore may lose and how long
  // it may take. It arrives as a value the composition root reads off its consumption profile row, so this
  // page grades an objective and declares none: a durability table here answers for deployments it never sees.
  type Objective = {
    readonly rpo: Duration.Duration
    readonly rto: Duration.Duration
  }
  // Each half is optional because each has a real absence a zero forges — a reading that took no frontier, and a
  // live store that never restored.
  type Window = {
    readonly rpo: Option.Option<Duration.Duration>
    readonly rto: Option.Option<Duration.Duration>
  }
  // One reading row rather than a positional tail: an adapter states the catalogue it read AND the stamps it took
  // in one value, so a provider cannot skip the recovery question by omitting an argument. `granted` is the
  // already-canonical half — provider-shaped custody no SQL catalog can scan, carried as contract keys the way
  // `artifacts` already is, because no grant vocabulary exists for it to enter through.
  type Reading<G extends string> = {
    readonly generation: Digest.Key<"content">
    readonly report: Capability.Report<G>
    readonly adapters: ReadonlyArray<Adapter>
    readonly granted: HashSet.HashSet<string>
    readonly artifacts: HashSet.HashSet<string>
    readonly observedAt: DateTime.Utc
    readonly frontier: Option.Option<DateTime.Utc>
    readonly restoredIn: Option.Option<Duration.Duration>
  }
  type Observation = {
    readonly generation: Digest.Key<"content">
    readonly capabilities: HashSet.HashSet<string>
    readonly artifacts: HashSet.HashSet<string>
    readonly observedAt: DateTime.Utc
    readonly frontier: Option.Option<DateTime.Utc>
    readonly restoredIn: Option.Option<Duration.Duration>
  }
  type Gap = Pick<Capability, "key" | "failureRank" | "restartClass">
  type Generation = Contract & {
    readonly observed: Observation
    readonly gaps: ReadonlyArray<Gap>
    readonly restart: Option.Option<RestartClass>
  }
}

// --- [OPERATIONS] -----------------------------------------------------------------------

// One discriminant table carries every contract invariant: each row names the predicate that must hold beside the
// ISSUE it raises — reason and subjects together — so a new invariant is one row, no reason collapses onto whichever
// tag an arm happened to carry, and the payload mints on the failing arm alone because the row is a deferred thunk.
type _Check = {
  readonly holds: () => boolean
  readonly issue: () => Backend.Issue
}

// Stages sequence, rows accumulate, and the CALL SITE's shape is the whole disposition. Rows inside one stage are
// independent columns of the same value, so the census names every column that refused instead of the first; a later
// stage reads a fact an earlier stage proved, so the ladder between stages aborts. An abort spanning independent
// columns is the deleted form: it makes a store owing three repairs look like a store owing one.
const _prove = (stages: ReadonlyArray<ReadonlyArray<_Check>>): Effect.Effect<void, BackendFault> =>
  Effect.forEach(
    stages,
    (stage) =>
      Effect.validateAll(stage, (row) => row.holds() ? Effect.void : Effect.fail(row.issue()), { discard: true }).pipe(
        Effect.mapError((issues) => new BackendFault({ issues })),
      ),
    { discard: true },
  )

// Generated ordinals carry the declared disruption order; absence means the gap set owes no restart at all.
const _worst = (over: ReadonlyArray<Backend.RestartClass>): Option.Option<Backend.RestartClass> =>
  Array.reduce(over, Option.none<Backend.RestartClass>(), (held, next) =>
    Option.some(Option.match(held, {
      onNone: () => next,
      onSome: (current) => Order.greaterThan(_byRestart)(next, current) ? next : current,
    })))

// Window derives HERE from the two stamps the observation carries, so no provider hands in a lag it measured
// against a clock this verdict never saw. The SIGN is the whole discriminant and ZERO sits on the measured side: a
// frontier stamped at its own reading instant is the freshest store this verdict exists to admit, where a frontier
// stamped AFTER it is skew rather than recency and owes the unmeasured half. `distanceDurationEither` splits at
// `> 0` and drops that zero onto its `Left`, so the signed millisecond distance reads directly instead.
const _recency = (millis: number): Option.Option<Duration.Duration> =>
  millis >= 0 ? Option.some(Duration.millis(millis)) : Option.none()

const _window = (observed: Backend.Observation): Backend.Window => ({
  rpo: Option.flatMap(observed.frontier, (seen) => _recency(DateTime.distance(seen, observed.observedAt))),
  rto: observed.restoredIn,
})

// Both halves absorb absence OPPOSITELY, and that split is a COLUMN rather than two hand-written arms: a reading
// carrying no frontier proves no recency and refuses, where admitting it grades a window nobody took, while an absent
// restore time is a store that never bounced and owes none. Every admitted generation therefore carries a measured
// recovery point, and each row names the window half it reads, the objective half it grades against, and its own
// posture on absence — the two arms differed in nothing else, which is why one table replaces both.
const _WINDOWS = {
  rpo: { measured: (window) => window.rpo, declared: (objective) => objective.rpo, unmeasured: "refuse" },
  rto: { measured: (window) => window.rto, declared: (objective) => objective.rto, unmeasured: "admit" },
} as const satisfies {
  readonly [Axis in Backend.Axis]: {
    readonly measured: (window: Backend.Window) => Option.Option<Duration.Duration>
    readonly declared: (objective: Backend.Objective) => Duration.Duration
    readonly unmeasured: "refuse" | "admit"
  }
}

// One check row per axis, so the axes stay INDEPENDENT columns of one verdict: a store both stale and slow to
// restore reports both, and the issue carries the measured and declared durations as values the family row renders.
const _breaches = (window: Backend.Window, objective: Backend.Objective): ReadonlyArray<_Check> =>
  Array.map(_AXES, (axis) => {
    const measured = _WINDOWS[axis].measured(window)
    const declared = _WINDOWS[axis].declared(objective)
    return {
      holds: () =>
        Option.match(measured, {
          onNone: () => _WINDOWS[axis].unmeasured === "admit",
          onSome: (held) => Duration.lessThanOrEqualTo(held, declared),
        }),
      issue: () => ({ reason: "recovery", axis, measured, declared } satisfies Backend.Issue),
    }
  })

const _wire = <A, I>(
  schema: Schema.Schema<A, I>,
  subject: string,
): ((encoded: I) => Effect.Effect<A, BackendFault>) =>
(encoded) =>
  Schema.decode(schema)(encoded).pipe(
    Effect.mapError(() => new BackendFault({ issues: [{ reason: "wire", document: subject }] })),
  )

const _encoded = <A, I>(
  schema: Schema.Schema<A, I>,
  subject: string,
): ((value: A) => Effect.Effect<I, BackendFault>) =>
(value) =>
  Schema.encode(schema)(value).pipe(
    Effect.mapError(() => new BackendFault({ issues: [{ reason: "wire", document: subject }] })),
  )

const _document = (encoded: Uint8Array): Effect.Effect<Backend.Document, BackendFault> =>
  encoded.byteLength <= _BACKEND_DOCUMENT_CEILING
    ? _wire(_codec.document, "contract")(encoded)
    : Effect.fail(new BackendFault({ issues: [{ reason: "wire", document: "contract document ceiling" }] }))

const _documentEncoded = (value: Backend.Document): Effect.Effect<Uint8Array, BackendFault> =>
  _encoded(_codec.document, "contract")(value).pipe(
    Effect.filterOrFail(
      (encoded) => encoded.byteLength <= _BACKEND_DOCUMENT_CEILING,
      () => new BackendFault({ issues: [{ reason: "wire", document: "contract document ceiling" }] }),
    ),
  )

// Kahn peel over the artifact set: each pass releases every row whose dependencies already left, and a pass
// releasing nothing while rows remain names exactly the cycle members. Acyclicity therefore costs no traversal
// state and no throw, and the remainder IS the witness the fault reports.
const _cyclic = (rows: ReadonlyArray<Backend.Artifact>): ReadonlyArray<string> => {
  const peel = (
    held: ReadonlyArray<Backend.Artifact>,
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
const _distinct = (): boolean => false

// One key whose rows disagree under the given mark is a collision; deduplication downstream is then safe
// because every surviving group agrees, so first-wins and last-wins both decide nothing.
const _collided = <A extends { readonly key: string }>(
  rows: ReadonlyArray<A>,
  same: (self: A, that: A) => boolean,
): ReadonlyArray<string> =>
  Array.dedupe(Array.map(
    Array.filter(rows, (row, index) =>
      Array.some(rows, (other, at) => at !== index && other.key === row.key && !same(other, row))),
    (row) => row.key,
  ))

const _sameArtifact = (self: Backend.Artifact, that: Backend.Artifact): boolean =>
  equals(ArtifactSchema, self, that)

const _sameCapability = (self: Backend.Capability, that: Backend.Capability): boolean =>
  equals(CapabilitySchema, self, that)

// Repeated fields are ordered protobuf values even where this contract uses them as sets. One generated-message
// projection owns their published order; local mints pass through it and a foreign document must already equal it.
const _normalized = (document: Backend.Document): Backend.Document =>
  Format.proto.create(BackendSchema, {
    contract: document.contract,
    artifacts: Array.map(Array.sort(document.artifacts, _byKey), (row) =>
      Format.proto.create(ArtifactSchema, {
        key: row.key,
        role: row.role,
        content: row.content,
        providers: Array.sort(row.providers, Order.number),
        dependsOn: Array.sort(row.dependsOn, String.Order),
      })),
    capabilities: Array.sort(document.capabilities, _byKey),
  })

// The schema fixes semantic field order and the core writer fixes widths, byte order, and every variable boundary.
// No serialized-message byte enters this stream: protobuf map/unknown ordering and floating encodings therefore
// cannot become generation inputs now or through an additive field the old reader does not know.
const _preimage = (document: Backend.Document): Iterable<Uint8Array> =>
  new CanonicalWriter()
    .string(document.contract)
    .rows(document.artifacts, (row, writer) => {
      writer
        .string(row.key)
        .ordinal(row.role)
        .bytes(row.content)
        .rows(row.providers, (provider, nested) => { nested.ordinal(provider) })
        .rows(row.dependsOn, (dependency, nested) => { nested.string(dependency) })
    })
    .rows(document.capabilities, (row, writer) => {
      writer
        .string(row.key)
        .string(row.lane)
        .string(row.requirement)
        .string(row.requirementValue)
        .ordinal(row.failureRank)
        .ordinal(row.restartClass)
    })
    .close()

// Every mint path lands on this one projection. Dependency keys prove closed and acyclic before the map-free
// binary identity projection, while the peer document remains ordinary ProtoJSON whose transported bytes carry no
// equality claim.
const _projection = (
  document: Backend.Document,
  transported: Option.Option<Uint8Array>,
): Effect.Effect<Backend.Projection, BackendFault> =>
  Effect.gen(function* () {
    const normalized = _normalized(document)
    const declared = HashSet.fromIterable(document.artifacts.map((row) => row.key))
    const repeatedArtifacts = _collided(document.artifacts, _distinct)
    const repeatedCapabilities = _collided(document.capabilities, _distinct)
    const dangling = document.artifacts.flatMap((row) =>
      row.dependsOn.filter((key) => !HashSet.has(declared, key)))
    const cyclic = () => _cyclic(document.artifacts)

    // Three stages: claim uniqueness settles both key spaces before dependency closure, and the peel reads only a
    // closed artifact set.
    yield* _prove([
      [
        {
          holds: () => equals(BackendSchema, document, normalized),
          issue: () => ({ reason: "wire", document: "contract order" } satisfies Backend.Issue),
        },
        {
          holds: () => repeatedArtifacts.length === 0,
          issue: () => ({ reason: "artifact", keys: repeatedArtifacts } satisfies Backend.Issue),
        },
        {
          holds: () => repeatedCapabilities.length === 0,
          issue: () => ({ reason: "capability", keys: repeatedCapabilities } satisfies Backend.Issue),
        },
      ],
      [{
        holds: () => dangling.length === 0,
        issue: () => ({ reason: "dependency", keys: dangling } satisfies Backend.Issue),
      }],
      [{
        holds: () => cyclic().length === 0,
        issue: () => ({ reason: "dependency", keys: cyclic() } satisfies Backend.Issue),
      }],
    ])

    const contract = yield* Option.match(transported, {
      onNone: () => _documentEncoded(normalized),
      onSome: Effect.succeed,
    })
    const id = yield* Digest.mint("content", _preimage(normalized))
    const artifactKeys = normalized.artifacts.map((row) => row.key)
    const required = normalized.capabilities
      .filter((row) => row.failureRank === _REQUIRED)
      .map((row) => row.key)

    return {
      contract: {
        id,
        document: normalized,
        required: HashSet.fromIterable(required),
        artifacts: HashSet.fromIterable(artifactKeys),
      },
      files: { contract },
    }
  })

const Backend = {
  worst: _worst,
  compose: (sources: Backend.Sources): Effect.Effect<Backend.Projection, BackendFault> =>
    Effect.gen(function* () {
      const document = yield* _wire(_codec.message, "contract")(Format.proto.create(BackendSchema, {
        contract: sources.contract,
        artifacts: Array.sort(sources.artifacts, _byKey),
        capabilities: Array.sort(sources.capabilities, _byKey),
      }))
      return yield* _projection(document, Option.none())
    }),
  merge: (
    contributions: Array.NonEmptyReadonlyArray<Backend.Projection>,
  ): Effect.Effect<Backend.Projection, BackendFault> =>
    Effect.gen(function* () {
      const [head, ...tail] = contributions
      const contract = head.contract.document.contract
      const artifacts = contributions.flatMap((one) => [...one.contract.document.artifacts])
      const capabilities = contributions.flatMap((one) => [...one.contract.document.capabilities])
      const mismatched = Array.dedupe(
        tail
          .map((one) => one.contract.document.contract)
          .filter((held) => held !== contract),
      )
      const collided = Array.appendAll(
        _collided(artifacts, _sameArtifact),
        _collided(capabilities, _sameCapability),
      )

      yield* _prove([[
        {
          holds: () => mismatched.length === 0,
          issue: () => ({
            reason: "identity",
            expected: contract,
            actual: Array.join(mismatched, ","),
          } satisfies Backend.Issue),
        },
        {
          holds: () => collided.length === 0,
          issue: () => ({ reason: "collision", keys: collided } satisfies Backend.Issue),
        },
      ]])

      const document = yield* _wire(_codec.message, "contract")(Format.proto.create(BackendSchema, {
        contract,
        artifacts: Array.sort(Array.dedupeWith(artifacts, _sameKey), _byKey),
        capabilities: Array.sort(Array.dedupeWith(capabilities, _sameKey), _byKey),
      }))
      return yield* _projection(document, Option.none())
    }),
  project: (files: Backend.Files): Effect.Effect<Backend.Projection, BackendFault> =>
    Effect.gen(function* () {
      const document = yield* _document(files.contract)
      return yield* _projection(document, Option.some(files.contract))
    }),
  // `report.granted` is the one authoritative membership value the PROBE publishes — the closed grant vocabulary
  // holding core spine keys, primitives, atomicity, and every capability an admitted extension carries.
  // `versions` keys on the probed EXTENSION name and answers floor evidence alone, so an adapter resolved
  // against it reports every core-seeded grant missing and every extension whose grant name differs from its
  // own. An adapter therefore maps the contract's canonical capability key onto the grant that proves it, and
  // `reading.granted` unions in the keys a probe has no catalog to find — already canonical, so no adapter row
  // could translate them and no grant row could hold them.
  observe: <G extends string>(reading: Backend.Reading<G>): Backend.Observation => ({
    generation: reading.generation,
    capabilities: HashSet.union(
      reading.granted,
      HashSet.fromIterable(
        reading.adapters
          .filter((row) => HashSet.has(reading.report.granted, row.local as Capability.Grant<G>))
          .map((row) => row.canonical),
      ),
    ),
    artifacts: reading.artifacts,
    observedAt: reading.observedAt,
    frontier: reading.frontier,
    restoredIn: reading.restoredIn,
  }),
  admit: (
    expected: Backend.Contract,
    observed: Backend.Observation,
    objective: Backend.Objective,
  ): Effect.Effect<Backend.Generation, BackendFault> =>
    Effect.gen(function* () {
      const gaps = expected.document.capabilities
        .filter((row) => !HashSet.has(observed.capabilities, row.key))
        .map(({ key, failureRank, restartClass }) => ({ key, failureRank, restartClass }))
      const missingCapabilities = gaps
        .filter((row) => row.failureRank === _REQUIRED)
        .map((row) => row.key)
      const missingArtifacts = HashSet.difference(expected.artifacts, observed.artifacts)

      // ONE stage over four independent columns of one store — generation, capabilities, artifacts, and each recovery
      // axis — so a store carrying the wrong generation on a stale window reports both halves of its own repair, and
      // the census grades on the dominant issue instead of on whichever proof a ladder happened to seat first.
      yield* _prove([[
        {
          holds: () => expected.id === observed.generation,
          issue: () =>
            ({ reason: "identity", expected: expected.id, actual: observed.generation } satisfies Backend.Issue),
        },
        {
          holds: () => missingCapabilities.length === 0,
          issue: () => ({ reason: "capability", keys: missingCapabilities } satisfies Backend.Issue),
        },
        {
          holds: () => HashSet.size(missingArtifacts) === 0,
          issue: () => ({ reason: "artifact", keys: [...missingArtifacts] } satisfies Backend.Issue),
        },
        ..._breaches(_window(observed), objective),
      ]])

      return {
        ...expected,
        observed,
        gaps,
        restart: _worst(gaps.map((row) => row.restartClass)),
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
