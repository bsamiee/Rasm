# [CORE_CLOCK]

The one clock owner of the branch: `Hlc` is the hybrid-logical stamp — one `Schema.Class` of two branded non-negative bigint halves, `physical` then `logical`, whose compose order is byte-identical to the C# port law (physical half first, logical half second, both little-endian), asserted bit-level by the `tests/contracts` `HLC_TWO_HALF` parity vectors — and `Uncertainty` is the honest wall-clock window on the same physical axis, carrying the sync-posture grade ladder that prices how wide an unproven clock must claim. Stamp algebra (order, tick, receive, the sixteen-byte layout twin, the single epoch unit-site) and window algebra (the join semilattice, precedence verdicts, meet, containment) are two clusters of one module because they share one branded axis: `state/causal` orders stamps through `Hlc.Order` and folds happened-before verdicts over windows, and a definite order is claimed only when the windows prove it. The module is `core/src/value/clock.ts`; a new clock fold is one static, a new sync posture is one grade row.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                  | [PUBLIC]             |
| :-----: | :---------------- | :---------------------------------------------------------------------- | :------------------- |
|  [01]   | `STAMP_OWNER`     | the two-half value, its order, the tick/receive folds, the unit site    | `Hlc`                |
|  [02]   | `TWO_HALF_LAYOUT` | the sixteen-byte little-endian twin and its overflow seam               | `Hlc.FromBytes`      |
|  [03]   | `GRADE_LADDER`    | the sync-posture vocabulary and its conservative bounds                 | `Uncertainty.grades` |
|  [04]   | `WINDOW_ALGEBRA`  | the interval value, its join semilattice, precedence, meet, containment | `Uncertainty`        |

## [02]-[STAMP_OWNER]

[STAMP_OWNER]:
- Owner: `Hlc`, the one stamp authority — `physical` and `logical` are `Schema.BigIntFromSelf` non-negative brands declared as interior anchors, reached by consumers only as `Hlc.fields.physical`/`Hlc.fields.logical` (field composition) and `Hlc.Physical`/`Hlc.Logical` (merged-namespace types), never as standalone brand exports.
- Law: the value space is unbounded-monotone — the brands bound only `>= 0n`, so `tick` and `receive` are total folds with no mid-domain range throw; the u64 ceiling is a wire-layout fact enforced by the `FromBytes` encode seam alone.
- Law: `Hlc.Order` is physical-then-logical lexicographic ascending — the one causal comparison every fold, sort, and max shares; the member name mirrors the ecosystem's canonical-instance convention (`Duration.Order`, `DateTime.Order`).
- Law: `tick(local, now)` is the send/local-event fold — a wall reading beyond `physical` resets `logical` to zero, otherwise `logical` advances by one; `receive(local, remote, now)` is the merge fold — the greatest physical wins and `logical` advances past whichever operands share it — so a merged stamp never regresses under `Hlc.Order` and causality survives clock skew.
- Law: `physicalOf(instant)` and `delta(span)` are the single unit site — epoch milliseconds, both zero-clamped under one clamp law, while `delta` converts through `Duration.toNanos` and saturates an infinite or over-u64 span at the byte layout's `_U64` ceiling; negative and `NaN` inputs decode to zero in the `Duration` algebra, finite sub-millisecond spans truncate on the bigint division, and every `Duration.DurationInput` path is total without a `BigInt(Infinity)` throw. `Uncertainty.around` therefore cannot smuggle a signed or non-finite span past the physical axis, and no other module converts time into the physical half.
- Law: `alike` is the class-derived `Schema.equivalence` — stamp deduplication and trace comparison consume the same structural relation as decode and law derivation.
- Law: `genesis` is the zero stamp — the fold seed for empty causality chains and the identity every replay starts from.
- Law: interior mints ride `Schema.decodeSync` over proven inputs — zero literals, `+ 1n` on a non-negative brand, zero-clamped epoch reads — so the throw path is structurally unreachable and the mint stays inside the trusted-construction channel `new Hlc` completes.
- Growth: a new clock fold (bounded-drift tick, batch merge) is one static composing the existing folds; a new stamp field is a wire-shape question for the interchange codec, never a widening here.
- Boundary: wall-clock reads ride the consumer's rail (`DateTime.now` under its `Clock` service) and arrive as values; the wire stamp shape coupling an `Hlc` with a tenant is the interchange codec's decode, built from this value.
- Packages: `effect` (`Schema`, `Order`, `DateTime`, `Duration`, `ParseResult`).

## [03]-[TWO_HALF_LAYOUT]

[TWO_HALF_LAYOUT]:
- Owner: `Hlc.FromBytes`, the sixteen-byte layout twin riding the class as a static — bytes 0..7 the physical half, bytes 8..15 the logical half, both read and written little-endian (`getBigUint64(offset, true)`/`setBigUint64(offset, true)`) — so the byte order, the half order, and the endianness flag live in one declaration and the parity drivers assert exactly this twin against the frozen vectors.
- Law: decode is total over sixteen admitted bytes — both halves land non-negative by construction and the class filters re-prove; encode is the one overflow seam — a half above `0xffffffffffffffff` fails as a typed `ParseResult.Type` fault at the boundary, so the unbounded value space meets the u64 wire honestly.
- Law: a half-swap is the named catastrophic drift — reading the logical half into the physical slot silently folds fresh ops as stale — so the offsets are spelled once here and every byte-carried stamp in the branch decodes through this twin, never through a second `DataView` read.
- Exemption: `_unpacked`/`_packed` are marked kernels — `DataView` reads and writes are the platform-forced statement seam; each detaches an immutable value, and the implementer carries the `// BOUNDARY ADAPTER` mark on each kernel's first line.
- Growth: a JSON or proto stamp spelling is an interchange codec built from `Hlc.fields`; a second byte layout is unspellable — this twin is the layout.
- Boundary: the fixture bytes and the cross-runtime equality grade live in `tests/contracts` with TS readers in `tests/typescript/_testkit`; this owner fixes the reproduction obligation and stores no fixture.
- Packages: the `DataView` platform surface inside the marked kernels.

```typescript
import * as Semigroup from "@effect/typeclass/Semigroup"
import { DateTime, Duration, Option, Order, ParseResult, pipe, Schema, type Types } from "effect"

const _Physical = Schema.BigIntFromSelf.pipe(Schema.nonNegativeBigInt(), Schema.brand("HlcPhysical"))
const _Logical = Schema.BigIntFromSelf.pipe(Schema.nonNegativeBigInt(), Schema.brand("HlcLogical"))
const _Bytes = Schema.Uint8ArrayFromSelf.pipe(Schema.filter((bytes) => bytes.length === 16))
const _U64 = 0xffffffffffffffffn

const _physical = Schema.decodeSync(_Physical)
const _logical = Schema.decodeSync(_Logical)
const _FLOOR = _logical(0n)

const _succ = (held: typeof _Logical.Type): typeof _Logical.Type => _logical(held + 1n)

const _unpacked = (bytes: Uint8Array): { readonly physical: bigint; readonly logical: bigint } => {
  // BOUNDARY ADAPTER: DataView reads are the platform-forced statement seam; the value detaches immutable
  const view = new DataView(bytes.buffer, bytes.byteOffset, bytes.byteLength)
  return { physical: view.getBigUint64(0, true), logical: view.getBigUint64(8, true) }
}

const _packed = (stamp: { readonly physical: bigint; readonly logical: bigint }): Uint8Array => {
  // BOUNDARY ADAPTER: DataView writes are the platform-forced statement seam; the buffer detaches immutable
  const view = new DataView(new ArrayBuffer(16))
  view.setBigUint64(0, stamp.physical, true)
  view.setBigUint64(8, stamp.logical, true)
  return new Uint8Array(view.buffer)
}

class Hlc extends Schema.Class<Hlc>("Hlc")({
  physical: _Physical,
  logical: _Logical,
}) {
  static readonly alike = Schema.equivalence(Hlc)
  static readonly Order: Order.Order<Hlc> = Order.combine(
    Order.mapInput(Order.bigint, (stamp: Hlc) => stamp.physical),
    Order.mapInput(Order.bigint, (stamp: Hlc) => stamp.logical),
  )
  static readonly genesis: Hlc = new Hlc({ physical: _physical(0n), logical: _FLOOR })
  static readonly FromBytes: Schema.transformOrFail<typeof _Bytes, typeof Hlc> = Schema.transformOrFail(_Bytes, Hlc, {
    strict: true,
    decode: (bytes) => ParseResult.succeed(_unpacked(bytes)),
    encode: (stamp, _options, ast) =>
      stamp.physical > _U64 || stamp.logical > _U64
        ? ParseResult.fail(new ParseResult.Type(ast, stamp, "<u64-overflow>"))
        : ParseResult.succeed(_packed(stamp)),
  })
  static readonly physicalOf = (instant: DateTime.Utc): Hlc.Physical => _physical(BigInt(Math.max(DateTime.toEpochMillis(instant), 0)))
  static readonly delta = (span: Duration.DurationInput): Hlc.Physical =>
    _physical(Option.match(Duration.toNanos(span), {
      onNone: () => _U64,
      onSome: (nanos) => pipe(nanos / 1_000_000n, (millis) => (millis > _U64 ? _U64 : millis)),
    }))
  static readonly tick = (local: Hlc, now: Hlc.Physical): Hlc =>
    now > local.physical
      ? new Hlc({ physical: now, logical: _FLOOR })
      : new Hlc({ physical: local.physical, logical: _succ(local.logical) })
  static readonly receive = (local: Hlc, remote: Hlc, now: Hlc.Physical): Hlc =>
    now > local.physical && now > remote.physical
      ? new Hlc({ physical: now, logical: _FLOOR })
      : local.physical > remote.physical
        ? new Hlc({ physical: local.physical, logical: _succ(local.logical) })
        : remote.physical > local.physical
          ? new Hlc({ physical: remote.physical, logical: _succ(remote.logical) })
          : new Hlc({
              physical: local.physical,
              logical: _succ(local.logical > remote.logical ? local.logical : remote.logical),
            })
}

declare namespace Hlc {
  type Physical = typeof _Physical.Type
  type Logical = typeof _Logical.Type
  type Packed = typeof Hlc.FromBytes.Encoded
}
```

## [04]-[GRADE_LADDER]

[GRADE_LADDER]:
- Owner: `Uncertainty.grades`, the sync-posture vocabulary riding the window owner — an interior key tuple anchors order and non-emptiness, the interior row table carries each posture's bound, and the assembled static exposes rows plus `kinds` so the ladder is one lookup away from the constructor it parameterizes.
- Law: three postures ride the ladder — `disciplined` (sync-disciplined clock, 250ms bound), `drifting` (wall clock with no sync evidence, 5s bound), `isolated` (offline or never-synced device, 5m bound) — ordered most- to least-trustworthy; the bounds are conservative floors, and a host that measures a tighter real offset passes its measured `Duration` to the window constructor instead of a grade.
- Law: the grade is chosen by evidence the host actually holds — sync-daemon health, last-sync age, platform monotonicity — and the runtime owns that evidence read; this vocabulary owns only the posture-to-bound correspondence.
- Law: the grade never travels a wire — wire stamps carry windows or points, and the posture that produced them stays a process fact.
- Growth: a new posture is one tuple entry plus one row; a per-deployment bound override is a caller-supplied `Duration` at the window constructor, never a row edit.
- Packages: `effect` (`Duration`, `Schema`).

## [05]-[WINDOW_ALGEBRA]

[WINDOW_ALGEBRA]:
- Owner: `Uncertainty`, a `Schema.Class` of `earliest`/`latest` bounds composed from `Hlc.fields.physical` — the brand reaches this cluster only through the owning class's field record, so window and stamp are the same axis by construction and no second physical notion exists.
- Law: `around(at, bound)` is the one constructor and its `bound` is modality-polymorphic — a grade kind selects the ladder row, any `Duration.DurationInput` carries a measured bound — discriminated by the derived grade guard on the value itself, never a flag; the window is `[at - delta, at + delta]` with the lower edge clamped at zero.
- Law: the window join is a lawful instance, never an ad-hoc function — `Semigroup.struct` composes `Semigroup.min`/`Semigroup.max` over the one physical `Order` (`Order.bigint` typed to the brand), `Semigroup.imap` carries the bounds product across the class constructor, and the instance rides the owner as `Uncertainty.Semigroup` so `state/merge` composes it as a field row in record CRDTs with zero re-derivation.
- Law: `hull(head, ...rest)` is the instance's `combineMany` projection — the join is identity-free (no lawful empty window exists), so the witnessed head is the first parameter and plurality is the variadic tail: one arity serves the binary join and the batch aggregation fold, and a forged sentinel window is unspellable; `meet(left, right)` is the dual bounds product (`max` earliest, `min` latest) folded to `Option` because disjoint windows share no instant — the agreement window `state/causal` narrows definite claims through.
- Law: `precedes(left, right)` is the three-verdict fold — `"before"`, `"after"`, `"indeterminate"` on overlap — and `contains(self, at)` answers point membership for watermark and frontier reads; both project the one composed physical `Order` through `Order.lessThan`/`Order.between`, so no comparison policy is restated inline, and the verdict union is a pure type anchor (`Uncertainty.Precedence`) because only the type plane reads it — `state/causal` dispatches on the literal and a definite order is claimed only when the windows prove it.
- Law: construction rides `around`/`spanning` and the interior mint proves its own inputs — clamped subtraction and checked addition stay non-negative — while the class carries `earliest <= latest` as its own filter, so decode, `new`, and `make` all prove the window, an inverted wire window fails admission as a `ParseError`, and `width` is total on every channel.
- Law: `alike` is the class-derived `Schema.equivalence` — window joins, deduplication, and convergence witnesses share the declaration's structural relation.
- Growth: a new verdict consumer is a `state` fold over `Precedence`; a new window operation (widen-by-grade) is one static composing the existing bounds or projecting the instance.
- Boundary: happened-before over stamps (physical+logical) is `Hlc.Order`'s total comparison; windows answer the honest wall-clock question only, and `state/causal` decides when each applies.
- Packages: `effect` (`Schema`, `Duration`, `Order`, `Option`); `@effect/typeclass` (`Semigroup`).

```typescript
const _kinds = ["disciplined", "drifting", "isolated"] as const
const _grades = {
  disciplined: { bound: Duration.millis(250) },
  drifting: { bound: Duration.seconds(5) },
  isolated: { bound: Duration.minutes(5) },
} as const

const _Grade = Schema.Literal(..._kinds)
const _isGrade = Schema.is(_Grade)

const _mint = Schema.decodeSync(Hlc.fields.physical)

const _floored = (at: Hlc.Physical, spread: Hlc.Physical): Hlc.Physical =>
  at > spread ? _mint(at - spread) : _mint(0n)

const _axis: Order.Order<Hlc.Physical> = Order.bigint     // the one physical comparison policy: every window operator projects it
const _past = Order.lessThan(_axis)
const _within = Order.between(_axis)
const _Overlap = Semigroup.struct({ earliest: Semigroup.max(_axis), latest: Semigroup.min(_axis) }) // the meet dual: bounds cross, validity folds to Option below

class Uncertainty extends Schema.Class<Uncertainty>("Uncertainty")(
  Schema.Struct({
    earliest: Hlc.fields.physical,
    latest: Hlc.fields.physical,
  }).pipe(Schema.filter((window) => window.earliest <= window.latest)),
) {
  static readonly alike = Schema.equivalence(Uncertainty)
  static readonly grades: Uncertainty.Grades = { ..._grades, kinds: _kinds }
  static readonly Semigroup: Semigroup.Semigroup<Uncertainty> = Semigroup.imap(
    Semigroup.struct({ earliest: Semigroup.min(_axis), latest: Semigroup.max(_axis) }), // the join semilattice: field rows are shipped extremum atoms over the one Order
    (bounds) => new Uncertainty(bounds),
    (window) => ({ earliest: window.earliest, latest: window.latest }),
  )
  static readonly around = (at: Hlc.Physical, bound: Duration.DurationInput | Uncertainty.Grade): Uncertainty =>
    Uncertainty.spanning(at, Hlc.delta(_isGrade(bound) ? _grades[bound].bound : bound))
  static readonly spanning = (at: Hlc.Physical, spread: Hlc.Physical): Uncertainty =>
    new Uncertainty({ earliest: _floored(at, spread), latest: _mint(at + spread) })
  static readonly precedes = (left: Uncertainty, right: Uncertainty): Uncertainty.Precedence =>
    _past(left.latest, right.earliest) ? "before" : _past(right.latest, left.earliest) ? "after" : "indeterminate"
  static readonly hull = (head: Uncertainty, ...rest: ReadonlyArray<Uncertainty>): Uncertainty =>
    Uncertainty.Semigroup.combineMany(head, rest)          // identity-free join: the witnessed head is the arity, binary and batch are one fold
  static readonly meet = (left: Uncertainty, right: Uncertainty): Option.Option<Uncertainty> =>
    Option.map(
      Option.liftPredicate(_Overlap.combine(left, right), (bounds) => bounds.earliest <= bounds.latest),
      (bounds) => new Uncertainty(bounds),
    )
  static readonly contains = (self: Uncertainty, at: Hlc.Physical): boolean =>
    _within(at, { minimum: self.earliest, maximum: self.latest })
  get width(): Hlc.Physical {
    return _mint(this.latest - this.earliest)
  }
}

declare namespace Uncertainty {
  type Grade = keyof typeof _grades
  type GradeRow = { readonly bound: Duration.Duration }
  type Grades = Types.Simplify<typeof _grades & { readonly kinds: typeof _kinds }>
  type Precedence = "after" | "before" | "indeterminate"
  type _Rows<T extends Record<Grade, GradeRow> = typeof _grades> = T
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Hlc, Uncertainty }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
