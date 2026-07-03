# [STORE_ROW]

The fail-closed capability rail of the store: one closed `{extension, floor, probeSql, capabilities, layer}` row vocabulary, one `Capability` service that probes every row and every declared relation at Layer construction, and one fault family for everything a probe refuses. Nothing in the folder assumes a PostgreSQL extension, a version floor, or a relation — presence is proven by a probe or the capability does not exist, and the granted set is a value sibling pages gate on: `require` fails typed, `when` degrades to `Option`. The same rail carries the DDL-split verify — `iac` applies the idempotent ensure rows at provision time, this service proves them at startup, runtime never mutates schema.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                               |
| :-----: | :--------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `ROW_VOCABULARY` | the closed capability-row shape, the ensure-row shape, the version-order fold          |
|  [02]   | `PROBE_SERVICE`  | the `Capability` service — probe-at-construction, granted set, `require`/`when` gates  |

## [2]-[ROW_VOCABULARY]

- Owner: `Capability.Row` — the closed `{extension, floor, probeSql, capabilities, layer}` shape every matrix row inhabits; `Capability.Ensure` — the `{relation, pg, sqlite}` DDL row every table-owning page publishes as data.
- Packages: `effect` (`Data`, `Order`, `String`).
- Growth: a new probe posture is a new `probeSql` literal on the owning row; a new realization site is a new `layer` literal — the shape never widens per extension.
- Boundary: the row set is `capability/matrix.md`'s; ensure rows are authored by the page owning the relation (`journal/*`, `project/*`, `scope/tenant.md`, `object/key.md`, `retrieve/index.md`) and collected by `scope/handle.md` at Layer construction.
- Law: `floor` is a dotted version literal compared segment-numeric through `_byVersion`; `"0.0.0"` is the presence-only floor a `[R11]`-open row carries until its pin lands — the probe still fails closed on absence.
- Law: `layer` names the realization site — `"image"` rows are CNPG deployment-image facts `iac/kube` satisfies, `"core"` rows ship in-core or as contrib and still probe; neither is ever a JS dependency.
- Law: `probeSql` is a closed-vocabulary literal, never caller input — `sql.unsafe` is legal in `[3]` exactly because the text is a row of a sealed `as const` table.
- Fault: one `CapabilityFault` family, reason-discriminated — `absent` (extension not installed), `floor` (installed below floor), `schema` (declared relation missing); all three route identically (fail startup, repair provision), so one class carries them and no policy table is earned.

```typescript
import { Data, Order, String, pipe } from "effect"

declare namespace Capability {
  type Row = {
    readonly extension: string
    readonly floor: string
    readonly probeSql: string
    readonly capabilities: ReadonlyArray<string>
    readonly layer: "image" | "core"
  }
  type Ensure = {
    readonly relation: string
    readonly pg: string
    readonly sqlite: string
  }
  type Reason = "absent" | "floor" | "schema"
  type Fault = _Fault
}

class _Fault extends Data.TaggedError("CapabilityFault")<{
  readonly reason: Capability.Reason
  readonly subject: string
  readonly detail: string
}> {}

const _segments = (version: string): ReadonlyArray<number> =>
  pipe(version, String.split("."), (parts) => parts.map((part) => Number.parseInt(part, 10) || 0))

const _byVersion: Order.Order<string> = Order.mapInput(Order.array(Order.number), _segments)

const _meets = (installed: string, floor: string): boolean =>
  Order.greaterThanOrEqualTo(_byVersion)(installed, floor)
```

## [3]-[PROBE_SERVICE]

- Owner: the `Capability` service — one `Effect.Service` class whose parameterized `Default(rows, ensures)` Layer factory probes every extension row and every ensure relation during construction, publishing the granted set, the installed-version report, and the refused evidence.
- Packages: `effect` (`Effect.Service`, `HashMap`, `HashSet`, `Option`); `@effect/sql` (`SqlClient`, `sql.unsafe`, `sql.onDialectOrElse`).
- Entry: `Capability.Default(rows, ensures)` — the one construction; `scope/handle.md` composes it under every `StoreHandle`, so an unprovisioned scope fails at the Layer, never at first query.
- Receipt: `Capability.Report` — `granted`, `versions`, `refused` — the typed probe evidence the startup log and `telemetry` consume; a tally kept beside it restates what the report carries.
- Growth: a consumer needing a new gate composes `require`/`when`, never a second probe path; a new dialect's relation probe is one `onDialectOrElse` arm.
- Law: probes are fail-closed — an absent row is refused, never assumed; a refused ensure relation fails Layer construction (the DDL split was violated), a refused extension row only shrinks the granted set (consumers degrade through `when`).
- Law: `require` is the hard gate (`Effect<void, CapabilityFault>`), `when` the soft gate (refusal reifies as `Option.none`, the statement-closer form); a boolean read of the granted set outside these two members is the smuggled knob.
- Law: probing runs once per Layer construction and the report is immutable thereafter — a live re-probe is `Stores.invalidate` on the owning scope, never a poll.
- Boundary: which rows exist is `capability/matrix.md`'s; where the Layer composes and which ensures are collected is `scope/handle.md`'s; the image that makes `"image"` rows probe true is `iac/kube`'s.

```typescript
import { Effect, HashMap, HashSet, Option } from "effect"
import { SqlClient } from "@effect/sql"

declare namespace Capability {
  type Report = {
    readonly granted: HashSet.HashSet<string>
    readonly versions: HashMap.HashMap<string, string>
    readonly refused: ReadonlyArray<_Fault>
  }
}

class Capability extends Effect.Service<Capability>()("store/Capability", {
  effect: (rows: ReadonlyArray<Capability.Row>, ensures: ReadonlyArray<Capability.Ensure>) =>
    Effect.gen(function* () {
      const sql = yield* SqlClient.SqlClient
      const probed = yield* Effect.forEach(rows, (row) =>
        Effect.map(
          sql.unsafe(row.probeSql).values,
          (cells) => [row, Option.map(Option.fromNullable(cells[0]?.[0]), globalThis.String)] as const,
        ))
      yield* Effect.forEach(ensures, (ensure) =>
        Effect.filterOrFail(
          sql.onDialectOrElse({
            orElse: () => sql`SELECT 1 FROM sqlite_master WHERE name = ${ensure.relation}`,
            pg: () => sql`SELECT 1 FROM information_schema.tables WHERE table_name = ${ensure.relation}`,
          }).values,
          (cells) => cells.length > 0,
          () => new _Fault({ reason: "schema", subject: ensure.relation, detail: ensure.pg }),
        ), { discard: true })
      const report: Capability.Report = probed.reduce<Capability.Report>(
        (acc, [row, version]) =>
          Option.match(version, {
            onNone: () => ({
              ...acc,
              refused: [...acc.refused, new _Fault({ reason: "absent", subject: row.extension, detail: row.floor })],
            }),
            onSome: (installed) =>
              _meets(installed, row.floor)
                ? {
                    granted: row.capabilities.reduce((set, key) => HashSet.add(set, key), acc.granted),
                    versions: HashMap.set(acc.versions, row.extension, installed),
                    refused: acc.refused,
                  }
                : {
                    ...acc,
                    refused: [...acc.refused, new _Fault({ reason: "floor", subject: row.extension, detail: installed })],
                  },
          }),
        { granted: HashSet.empty(), versions: HashMap.empty(), refused: [] },
      )
      return {
        report,
        granted: report.granted,
        require: (key: string): Effect.Effect<void, _Fault> =>
          HashSet.has(report.granted, key)
            ? Effect.void
            : Effect.fail(new _Fault({ reason: "absent", subject: key, detail: "<ungranted>" })),
        when: <A, E, R>(key: string, effect: Effect.Effect<A, E, R>): Effect.Effect<Option.Option<A>, E, R> =>
          Effect.when(effect, () => HashSet.has(report.granted, key)),
      }
    }),
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Capability }
```
