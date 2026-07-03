# [STATE_AVAILABILITY]

`evidence/availability.ts` owns the degradation and command-availability vocabulary the wire gateway gate types against: `Availability` — the decoded snapshot of a host's degradation level and per-command verdicts — with the level table, the verdict family, the worst-wins merge, and the total gate read riding one owner. `wire/codec/envelope` decodes the C# `DegradationLevel`/`CommandAvailabilityWire` (seam `AH:58`) INTO this vocabulary, and the `wire/gateway` command gate consumes it as an injected value typed against this module (seam `AU:59`) — ordinary dependency injection over a legal import, never a port. Availability composes across sources as a lattice: level merges by maximum severity, per-command verdicts merge worst-wins under the union, so N health feeds fold into one honest verdict with no precedence ladder.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                          | [SURFACE]                                            |
| :-----: | :----------------- | :---------------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | [LEVEL_VOCABULARY] | the degradation level rows and their gate policy columns          | interior `_LEVELS`/`_ROWS`, `Availability.Level`         |
|  [02]   | [VERDICT_FAMILY]   | the per-command availability verdict union                        | `Availability.Command`, `Availability.Verdict`           |
|  [03]   | [SNAPSHOT_OWNER]   | the decoded snapshot, worst-wins merge, gate read, fold plan      | `Availability`, `.worst/.admits/.plan`                   |

## [2]-[LEVEL_VOCABULARY]

- Owner: the `_LEVELS` key tuple and `_ROWS` policy table — each level carries `rank` (the severity lattice coordinate) and `admits` (the gate posture: everything, reads only, nothing); the tuple spread feeds `Schema.Literal` so the wire arm, the type, and the ordered severity axis derive from one anchor.
- Law: severity is the only comparison — `_byRank` orders levels by their row's rank, `Merge.max(_byRank)` is the worst-wins semilattice, and no consumer ever compares level names lexically or through a hand ladder.
- Law: gate policy is a column, not a branch — `admits` is data a total read projects; adding a level is one tuple entry plus one row, and every dispatch over `Availability.Level` breaks loudly until its arm exists.
- Boundary: the level roster mirrors the C# `Rasm.AppHost/Observability/Health.cs` `DegradationLevel` one-to-one; roster parity pins at the `wire/codec/envelope` decode seam.
- Growth: a new gate posture is one `_POSTURES` row; a new level is one `_LEVELS` entry plus its `_ROWS` row.

```typescript
import { Array, HashMap, Option, Order, Schema } from "effect"
import { Hlc, TenantContext } from "@rasm/ts/kernel"
import { Merge } from "../crdt/merge.ts"
import { Fold } from "../fold/algebra.ts"

const _LEVELS = ["full", "degraded", "readonly", "offline"] as const
const _POSTURES = ["all", "reads", "none"] as const

const _ROWS = {
  full: { rank: 0, admits: "all" },
  degraded: { rank: 1, admits: "all" },
  readonly: { rank: 2, admits: "reads" },
  offline: { rank: 3, admits: "none" },
} as const

const _Level = Schema.Literal(..._LEVELS)
const _Command = Schema.NonEmptyString.pipe(Schema.brand("CommandName"))

const _byRank: Order.Order<(typeof _LEVELS)[number]> = Order.mapInput(
  Order.number,
  (level: (typeof _LEVELS)[number]) => _ROWS[level].rank,
)
```

## [3]-[VERDICT_FAMILY]

- Owner: `Availability.Command` — the closed verdict union: `Available` (unconditional), `Gated` (admitted with a declared constraint — the reason and an optional `until` stamp the gateway surfaces to callers), `Withheld` (refused at a named level with its reason) — decoded per command from the wire availability map.
- Law: verdicts order by restrictiveness — `Available < Gated < Withheld`, with `Withheld` tie-broken by level rank — and `_worstVerdict` is `Merge.max` over that order, so merging two sources never loosens a constraint.
- Law: a verdict carries its evidence — reason strings and stamps are fields policy reads, and the gateway's 429/Retry-After style surfacing derives from `Gated.until`, never from prose parsing.
- Growth: a new verdict kind is one tagged case plus one restrictiveness rank — the gate read and merges absorb it with zero consumer edits.

```typescript
const _Available = Schema.TaggedStruct("Available", {})

const _Gated = Schema.TaggedStruct("Gated", {
  reason: Schema.NonEmptyString,
  until: Schema.optionalWith(Hlc, { as: "Option" }),
})

const _Withheld = Schema.TaggedStruct("Withheld", {
  level: _Level,
  reason: Schema.NonEmptyString,
})

const _Verdict: Schema.Union<[typeof _Available, typeof _Gated, typeof _Withheld]> = Schema.Union(
  _Available,
  _Gated,
  _Withheld,
)

const _VERDICT_RANKS = { Available: 0, Gated: 1, Withheld: 2 } as const

const _byRestrictiveness: Order.Order<typeof _Verdict.Type> = Order.combine(
  Order.mapInput(Order.number, (verdict: typeof _Verdict.Type) => _VERDICT_RANKS[verdict._tag]),
  Order.mapInput(Order.number, (verdict: typeof _Verdict.Type) =>
    verdict._tag === "Withheld" ? _ROWS[verdict.level].rank : 0),
)

const _worstVerdict: Merge.Instance<typeof _Verdict.Type> = Merge.max(_byRestrictiveness)
```

## [4]-[SNAPSHOT_OWNER]

- Owner: `Availability` — the decoded snapshot class: `level`, the per-command verdict `HashMap`, the `since` stamp, and the tenant; `worst` (the snapshot lattice), `admits` (the total gate read), and `plan` (the per-tenant fold) ride it as statics.
- Law: `Availability.worst` merges snapshots field-wise as a lattice — level by severity max, commands by union with per-command worst-wins, `since` by stamp max — so the fold of N health feeds is associative, commutative, and idempotent, and `crdt/converge` proves it like any instance.
- Law: `Availability.admits` is total — a command absent from the map answers from the level row's posture (`all` admits, `reads` gates with the level's name as reason, `none` withholds), so the gateway gate never meets `undefined` and never re-implements the fallback.
- Law: gating durations and retry posture type against `kernel/fault` budget rows — the gateway composes budget vocabulary with these verdicts; neither is re-declared here.
- Boundary: the wire gateway gate (`wire/gateway/command`) types against this owner; `evidence/timeline` records level shifts; `query/live` serves the folded snapshot to shells.

```typescript
class Availability extends Schema.Class<Availability>("Availability")({
  level: _Level,
  commands: Schema.HashMap({ key: _Command, value: _Verdict }),
  since: Hlc,
  tenant: TenantContext,
}) {
  static readonly worst: Merge.Instance<Availability> = Merge.instance({
    combine: {
      combine: (self, that) =>
        new Availability({
          level: Merge.max(_byRank).combine.combine(self.level, that.level),
          commands: HashMap.reduce(that.commands, self.commands, (acc, verdict, command) =>
            HashMap.set(
              acc,
              command,
              Option.match(HashMap.get(acc, command), {
                onNone: () => verdict,
                onSome: (held) => _worstVerdict.combine.combine(held, verdict),
              }),
            )),
          since: Merge.max(Hlc.Order).combine.combine(self.since, that.since),
          tenant: self.tenant,
        }),
      combineMany: (self, rest) =>
        Array.reduce(Array.fromIterable(rest), self, (held, next) => Availability.worst.combine.combine(held, next)),
    },
    posture: { commutative: true, idempotent: true },
    alike: Schema.equivalence(Availability),
    empty: Option.none(),
  })
  static readonly plan: Fold.Plan<Availability, TenantContext, Availability> = Fold.plan({
    name: "evidence/availability",
    key: (snapshot) => snapshot.tenant,
    lift: (snapshot) => snapshot,
    merge: Availability.worst,
  })
  static admits(snapshot: Availability, command: Availability.Command): Availability.Verdict {
    return Option.getOrElse(HashMap.get(snapshot.commands, command), () =>
      _ROWS[snapshot.level].admits === "all"
        ? _Available.make({})
        : _ROWS[snapshot.level].admits === "reads"
          ? _Gated.make({ reason: snapshot.level, until: Option.none() })
          : _Withheld.make({ level: snapshot.level, reason: snapshot.level }))
  }
}

declare namespace Availability {
  type Level = (typeof _LEVELS)[number]
  type Posture = (typeof _POSTURES)[number]
  type Command = typeof _Command.Type
  type Verdict = typeof _Verdict.Type
  type _Rows<T extends Record<Level, { readonly rank: number; readonly admits: Posture }> = typeof _ROWS> = T
  type _Keys<K extends keyof typeof _ROWS = Level> = K
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Availability }
```
