# [KERNEL_UNCERTAINTY]

The honest clock-uncertainty law: a wall reading is never a point — it is an `Uncertainty` window `[earliest, latest]` on the HLC physical axis, wide enough to contain the true instant, and `state/causal` folds happened-before verdicts over windows instead of trusting skewed points. `ClockGrade` is the honesty ladder — a vocabulary of sync postures each carrying its conservative bound — so a device that cannot prove synchronization still stamps truthfully, and order over overlapping windows is `"indeterminate"` by construction rather than silently wrong. The window rides the same branded physical axis `clock/hlc` owns, so uncertainty and stamps stay unit-coherent through the one unit site. The module is `kernel/src/clock/uncertainty.ts`; a new sync posture is one grade row, and a new window fold is one static on the owner.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                        | [PUBLIC]      |
| :-----: | :--------------- | :------------------------------------------------------------- | :------------ |
|  [01]   | `GRADE_LADDER`   | the sync-posture vocabulary and its conservative bounds        | `ClockGrade`  |
|  [02]   | `WINDOW_ALGEBRA` | the interval value, precedence verdicts, hull and containment  | `Uncertainty` |

## [2]-[GRADE_LADDER]

[GRADE_LADDER]:
- Owner: `ClockGrade`, the assembled grade vocabulary — an interior key tuple anchors order and non-emptiness, the interior row table carries each posture's bound, the merged hub carries the derived types and the guard pair, and the exported owner assembles rows plus the `kinds` tuple under a `typeof`-derived stated annotation.
- Law: three postures ride the ladder — `disciplined` (sync-disciplined clock, 250ms bound), `drifting` (wall clock with no sync evidence, 5s bound), `isolated` (offline or never-synced device, 5m bound) — ordered most- to least-trustworthy; the bounds are conservative floors, and a host that measures a tighter real offset passes its measured `Duration` to the window constructor instead of a grade.
- Law: the grade is chosen by evidence the host actually holds — sync-daemon health, last-sync age, platform monotonicity — and `host`/`browser` own that evidence read; this vocabulary owns only the posture-to-bound correspondence.
- Growth: a new posture is one tuple entry plus one row; a per-deployment bound override is a caller-supplied `Duration` at the window constructor, never a row edit.
- Boundary: the grade never travels a wire — wire stamps carry windows or points, and the posture that produced them stays a process fact.
- Packages: `effect` (`Duration`, `Schema`).

```typescript
import { Duration, Schema, type Types } from "effect"
import { Hlc } from "./hlc.ts"

const _kinds = ["disciplined", "drifting", "isolated"] as const
const _grades = {
  disciplined: { bound: Duration.millis(250) },
  drifting: { bound: Duration.seconds(5) },
  isolated: { bound: Duration.minutes(5) },
} as const

const _Kind = Schema.Literal(..._kinds)
const _isKind = Schema.is(_Kind)

declare namespace ClockGrade {
  type Kinds = typeof _kinds
  type Kind = keyof typeof _grades
  type Row = { readonly bound: Duration.Duration }
  type Contract = Record<Kinds[number], Row>
  type Shape = Types.Simplify<typeof _grades & { readonly kinds: Kinds }>
  type _Rows<T extends Contract = typeof _grades> = T
  type _Keys<K extends keyof Contract = Kind> = K
}

const ClockGrade: ClockGrade.Shape = { ..._grades, kinds: _kinds }
```

## [3]-[WINDOW_ALGEBRA]

[WINDOW_ALGEBRA]:
- Owner: `Uncertainty`, a `Schema.Class` of `earliest`/`latest` bounds composed from `Hlc.fields.physical` — the brand reaches this module only through the owning class's field record, so window and stamp are the same axis by construction and no second physical notion exists.
- Law: `around(at, bound)` is the one constructor and its `bound` is modality-polymorphic — a `ClockGrade.Kind` selects the ladder row, any `Duration.DurationInput` carries a measured bound — discriminated by the derived grade guard on the value itself, never a flag; the window is `[at - delta, at + delta]` with the lower edge clamped at zero.
- Law: `precedes(left, right)` is the three-verdict fold — `"before"` when `left.latest < right.earliest`, `"after"` when `right.latest < left.earliest`, `"indeterminate"` on overlap — and the verdict union is a pure type anchor (`Uncertainty.Precedence`) because only the type plane reads it; `state/causal` dispatches on the literal and a definite order is claimed only when the windows prove it.
- Law: `hull(left, right)` is the associative window join — least earliest, greatest latest — the aggregation fold a batch of readings collapses under; `contains(self, at)` answers point membership for watermark and frontier reads.
- Law: construction rides `around` and the interior mint proves its own inputs — clamped subtraction and checked addition stay non-negative — so the class filters cannot trip and the algebra is total.
- Growth: a new verdict consumer is a `state` fold over `Precedence`; a new window operation (meet, widen-by-grade) is one static composing the existing bounds.
- Boundary: happened-before over *stamps* (physical+logical) is `Hlc.Order`'s total comparison; windows answer the honest wall-clock question only, and `state/causal` decides when each applies.
- Packages: `effect` (`Schema`, `Duration`); `kernel/clock/hlc` (`Hlc.fields.physical`, `Hlc.delta`).

```typescript
const _mint = Schema.decodeSync(Hlc.fields.physical)

const _floored = (at: Hlc.Physical, spread: Hlc.Physical): Hlc.Physical =>
  at > spread ? _mint(at - spread) : _mint(0n)

class Uncertainty extends Schema.Class<Uncertainty>("Uncertainty")({
  earliest: Hlc.fields.physical,
  latest: Hlc.fields.physical,
}) {
  static readonly around = (at: Hlc.Physical, bound: Duration.DurationInput | ClockGrade.Kind): Uncertainty =>
    Uncertainty.spanning(at, Hlc.delta(_isKind(bound) ? ClockGrade[bound].bound : bound))
  static readonly spanning = (at: Hlc.Physical, spread: Hlc.Physical): Uncertainty =>
    new Uncertainty({ earliest: _floored(at, spread), latest: _mint(at + spread) })
  static readonly precedes = (left: Uncertainty, right: Uncertainty): Uncertainty.Precedence =>
    left.latest < right.earliest ? "before" : right.latest < left.earliest ? "after" : "indeterminate"
  static readonly hull = (left: Uncertainty, right: Uncertainty): Uncertainty =>
    new Uncertainty({
      earliest: left.earliest < right.earliest ? left.earliest : right.earliest,
      latest: left.latest > right.latest ? left.latest : right.latest,
    })
  static readonly contains = (self: Uncertainty, at: Hlc.Physical): boolean =>
    self.earliest <= at && at <= self.latest
  get width(): Hlc.Physical {
    return _mint(this.latest - this.earliest)
  }
}

declare namespace Uncertainty {
  type Precedence = "after" | "before" | "indeterminate"
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { ClockGrade, Uncertainty }
```
