# [CORE_CLOCK]

`Clock` is the branch's sole causal-time owner: nested `Hlc` carries the two-half stamp on the frozen sixteen-byte layout — physical half a Unix-epoch count of 100-nanosecond TICKS, logical half the monotone counter — while nested `Uncertainty` carries windows over that same tick axis. Module: `core/src/value/clock.ts`.

## [01]-[CLOCK_OWNER]

- `Clock.Hlc.Physical` counts 100-nanosecond TICKS, never milliseconds, and carries the layout's signed-64-bit domain at the brand, so an out-of-domain half faults on decode; `Clock.Hlc.FromBytes` guards the unsigned 64-bit logical half alone.
- `Clock.Hlc.physicalOf` mints ticks from a millisecond platform reading, so a browser stamp lands on an exact 10,000-tick multiple and still orders against a finer-resolution peer stamp.
- `Clock.Uncertainty.grades` is one ordered vocabulary of sync posture and conservative bounds; rows stay private behind `at`.
- `Clock.Uncertainty` owns interval construction, join, meet, precedence, containment, and width over `Clock.Hlc.Physical`.

```typescript
import * as Semigroup from "@effect/typeclass/Semigroup"
import { DateTime, Duration, Option, Order, ParseResult, pipe, Schema } from "effect"
import { Shape } from "./schema.ts"

// `_Physical` counts Unix-epoch TICKS, 100 nanoseconds each, inside an int64 byte cell, so its mint domain is I63
// and not the U64 the cell's width suggests. Bounding at the BRAND makes that domain one fact every construction,
// decode, and window arithmetic answers, where a seam-only guard let an out-of-domain half live in memory until it
// reached bytes. Ticks cost range against a millisecond half — I63 ticks reach 31197-CE where I63 millis reach year
// 292-million — and buy the 10,000x resolution the logical half otherwise absorbs as same-instant collisions.
const _I63 = 0x7fffffffffffffffn
const _U64 = 0xffffffffffffffffn
const _TICKS_PER_MILLI = 10_000n
const _NANOS_PER_TICK = 100n

const _Physical = Schema.BigIntFromSelf.pipe(Schema.betweenBigInt(0n, _I63), Schema.brand("ClockPhysical"))
const _Logical = Schema.BigIntFromSelf.pipe(Schema.nonNegativeBigInt(), Schema.brand("ClockLogical"))
const _Bytes = Schema.Uint8ArrayFromSelf.pipe(Schema.filter((bytes) => bytes.length === 16))

const _physical = Schema.decodeSync(_Physical)
const _logical = Schema.decodeSync(_Logical)
const _ZERO = _logical(0n)
const _succ = (held: typeof _Logical.Type): typeof _Logical.Type => _logical(held + 1n)

const _unpack = (bytes: Uint8Array): { readonly physical: bigint; readonly logical: bigint } => {
  // BOUNDARY ADAPTER: DataView is the platform layout seam; the returned halves detach as immutable values.
  const view = new DataView(bytes.buffer, bytes.byteOffset, bytes.byteLength)
  return { physical: view.getBigUint64(0, true), logical: view.getBigUint64(8, true) }
}

const _pack = (stamp: { readonly physical: bigint; readonly logical: bigint }): Uint8Array => {
  // BOUNDARY ADAPTER: DataView is the platform layout seam; the returned byte array owns its buffer.
  const view = new DataView(new ArrayBuffer(16))
  view.setBigUint64(0, stamp.physical, true)
  view.setBigUint64(8, stamp.logical, true)
  return new Uint8Array(view.buffer)
}

class _Hlc extends Schema.Class<_Hlc>("Clock.Hlc")({ physical: _Physical, logical: _Logical }) {
  static readonly alike = Schema.equivalence(_Hlc)
  static readonly Order: Order.Order<_Hlc> = Order.combine(
    Order.mapInput(Order.bigint, (stamp: _Hlc) => stamp.physical),
    Order.mapInput(Order.bigint, (stamp: _Hlc) => stamp.logical),
  )
  static readonly genesis = new _Hlc({ physical: _physical(0n), logical: _ZERO })
  static readonly FromBytes: Schema.transformOrFail<typeof _Bytes, typeof _Hlc> = Schema.transformOrFail(_Bytes, _Hlc, {
    strict: true,
    // `_Physical` re-admits the unpacked half, so a cell carrying a value past the I63 mint domain faults here
    // rather than decoding as a stamp no peer could have written. Only the logical half needs its own encode
    // guard, since the physical domain already refused anything a minter cannot write.
    decode: (bytes) => ParseResult.succeed(_unpack(bytes)),
    encode: (stamp, _options, ast) => stamp.logical > _U64
      ? ParseResult.fail(new ParseResult.Type(ast, stamp, "<u64-overflow>"))
      : ParseResult.succeed(_pack(stamp)),
  })
  // `physicalOf` SCALES rather than transcribes, because the platform reads milliseconds and the axis counts ticks:
  // a bare `toEpochMillis` half writes a number 10,000x small into the frozen cell and every comparison against a
  // peer stamp inverts. Resolution stays the platform's — each stamp is an exact 10,000-tick multiple — while the
  // UNIT matches the layout, which is what ordering against a finer-resolution peer requires.
  static readonly physicalOf = (instant: DateTime.Utc): typeof _Physical.Type =>
    _physical(BigInt(Math.max(DateTime.toEpochMillis(instant), 0)) * _TICKS_PER_MILLI)
  // Spans ride the same tick axis, so a grade bound and a stamp subtract coherently; saturation pins at the mint
  // domain, since a window clipped to the last representable tick still orders where an overflowed one faults.
  static readonly delta = (span: Duration.DurationInput): typeof _Physical.Type =>
    _physical(Option.match(Duration.toNanos(span), {
      onNone: () => _I63,
      onSome: (nanos) => pipe(nanos / _NANOS_PER_TICK, (ticks) => ticks <= 0n ? 0n : ticks > _I63 ? _I63 : ticks),
    }))
  static readonly tick = (local: _Hlc, now: typeof _Physical.Type): _Hlc => now > local.physical
    ? new _Hlc({ physical: now, logical: _ZERO })
    : new _Hlc({ physical: local.physical, logical: _succ(local.logical) })
  static readonly receive = (local: _Hlc, remote: _Hlc, now: typeof _Physical.Type): _Hlc =>
    now > local.physical && now > remote.physical
      ? new _Hlc({ physical: now, logical: _ZERO })
      : local.physical > remote.physical
        ? new _Hlc({ physical: local.physical, logical: _succ(local.logical) })
        : remote.physical > local.physical
          ? new _Hlc({ physical: remote.physical, logical: _succ(remote.logical) })
          : new _Hlc({
              physical: local.physical,
              logical: _succ(local.logical > remote.logical ? local.logical : remote.logical),
            })
}

const _gradeKinds = ["disciplined", "drifting", "isolated"] as const
const _gradeRows = {
  disciplined: { bound: Duration.millis(250) },
  drifting: { bound: Duration.seconds(5) },
  isolated: { bound: Duration.minutes(5) },
} as const
const _grades = Shape.vocabulary(_gradeKinds, _gradeRows)

const _mint = Schema.decodeSync(_Hlc.fields.physical)
const _axis: Order.Order<typeof _Physical.Type> = Order.bigint
const _past = Order.lessThan(_axis)
const _within = Order.between(_axis)
const _overlap = Semigroup.struct({ earliest: Semigroup.max(_axis), latest: Semigroup.min(_axis) })
// Both window bounds clamp to the mint domain, because the axis is bounded at BOTH ends: a spread wider than the
// stamp underflows past the epoch and a spread added near the ceiling overflows past I63, and either one throws
// out of `_mint` where a clamped bound still answers every precedence, containment, and width read.
const _floored = (at: typeof _Physical.Type, spread: typeof _Physical.Type): typeof _Physical.Type =>
  at > spread ? _mint(at - spread) : _mint(0n)
const _ceiled = (at: typeof _Physical.Type, spread: typeof _Physical.Type): typeof _Physical.Type =>
  pipe(at + spread, (sum) => _mint(sum > _I63 ? _I63 : sum))

class _Uncertainty extends Schema.Class<_Uncertainty>("Clock.Uncertainty")(
  Schema.Struct({ earliest: _Hlc.fields.physical, latest: _Hlc.fields.physical }).pipe(
    Schema.filter((window) => window.earliest <= window.latest),
  ),
) {
  static readonly alike = Schema.equivalence(_Uncertainty)
  static readonly grades = _grades
  static readonly Semigroup: Semigroup.Semigroup<_Uncertainty> = Semigroup.imap(
    Semigroup.struct({ earliest: Semigroup.min(_axis), latest: Semigroup.max(_axis) }),
    (bounds) => new _Uncertainty(bounds),
    (window) => ({ earliest: window.earliest, latest: window.latest }),
  )
  static readonly around = (
    at: typeof _Physical.Type,
    bound: Duration.DurationInput | (typeof _gradeKinds)[number],
  ): _Uncertainty => _Uncertainty.spanning(at, _Hlc.delta(_grades.is(bound) ? _grades.at(bound).bound : bound))
  static readonly spanning = (at: typeof _Physical.Type, spread: typeof _Physical.Type): _Uncertainty =>
    new _Uncertainty({ earliest: _floored(at, spread), latest: _ceiled(at, spread) })
  static readonly precedes = (left: _Uncertainty, right: _Uncertainty): Clock.Uncertainty.Precedence =>
    _past(left.latest, right.earliest) ? "before" : _past(right.latest, left.earliest) ? "after" : "indeterminate"
  static readonly hull = (head: _Uncertainty, ...rest: ReadonlyArray<_Uncertainty>): _Uncertainty =>
    _Uncertainty.Semigroup.combineMany(head, rest)
  static readonly meet = (left: _Uncertainty, right: _Uncertainty): Option.Option<_Uncertainty> =>
    Option.map(
      Option.liftPredicate(_overlap.combine(left, right), (bounds) => bounds.earliest <= bounds.latest),
      (bounds) => new _Uncertainty(bounds),
    )
  static readonly contains = (self: _Uncertainty, at: typeof _Physical.Type): boolean =>
    _within(at, { minimum: self.earliest, maximum: self.latest })
  get width(): typeof _Physical.Type {
    return _mint(this.latest - this.earliest)
  }
}

const Clock = { Hlc: _Hlc, Uncertainty: _Uncertainty } as const

declare namespace Clock {
  type Hlc = _Hlc
  namespace Hlc {
    type Physical = typeof _Physical.Type
    type Logical = typeof _Logical.Type
    type Packed = typeof _Hlc.FromBytes.Encoded
  }
  type Uncertainty = _Uncertainty
  namespace Uncertainty {
    type Grade = (typeof _gradeKinds)[number]
    type Precedence = "after" | "before" | "indeterminate"
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Clock }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
