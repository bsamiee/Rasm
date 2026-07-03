# [KERNEL_HLC]

The hybrid-logical clock law: `Hlc` is one `Schema.Class` of two branded non-negative bigint halves — `physical` then `logical` — whose compose order is byte-identical to the C# port law (invariant 3): physical half first, logical half second, both little-endian, asserted bit-level by the `tests/contracts` two-half parity vectors (`[R22]`, seams `AH:53`/`AH:64`). The class carries the whole clock algebra as statics — the canonical `Order`, the `tick` send fold, the `receive` merge fold, the `FromBytes` sixteen-byte layout twin, and the single unit-projection pair (`physicalOf`/`delta`) that owns the epoch encoding in exactly one edit site. `state/causal` consumes stamps and folds; the wire stamp shape coupling an `Hlc` with a tenant is `wire`'s decode, built from this value. The module is `kernel/src/clock/hlc.ts`; the two cluster cards legislate and the terminal fence is the complete module an implementer transcribes.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                          | [PUBLIC]        |
| :-----: | :---------------- | :--------------------------------------------------------------- | :-------------- |
|  [01]   | `STAMP_OWNER`     | the two-half value, its order, the tick/receive folds, unit site | `Hlc`           |
|  [02]   | `TWO_HALF_LAYOUT` | the sixteen-byte little-endian twin and its overflow seam        | `Hlc.FromBytes` |

## [2]-[STAMP_OWNER]

[STAMP_OWNER]:
- Owner: `Hlc`, the one stamp authority — `physical` and `logical` are `Schema.BigIntFromSelf` non-negative brands declared as interior anchors, reached by consumers only as `Hlc.fields.physical`/`Hlc.fields.logical` (field composition) and `Hlc.Physical`/`Hlc.Logical` (merged-namespace types), never as standalone brand exports.
- Law: the value space is unbounded-monotone — the brands bound only `>= 0n`, so `tick` and `receive` are total folds with no mid-domain range throw; the u64 ceiling is a wire-layout fact enforced by the `FromBytes` encode seam alone.
- Law: `Hlc.Order` is physical-then-logical lexicographic ascending — the one causal comparison every fold, sort, and max shares; the member name mirrors the ecosystem's canonical-instance convention (`Duration.Order`, `DateTime.Order`).
- Law: `tick(local, now)` is the send/local-event fold — a wall reading beyond `physical` resets `logical` to zero, otherwise `logical` advances by one; `receive(local, remote, now)` is the merge fold — the greatest physical wins and `logical` advances past whichever operands share it — so a merged stamp never regresses under `Hlc.Order` and causality survives clock skew.
- Law: `physicalOf(instant)` and `delta(span)` are the single unit site — epoch milliseconds, zero-clamped so a pre-epoch wall reading folds to the genesis physical instead of a mid-domain throw, pinned by the `[R22]` parity vectors; a C#-proven different epoch encoding repairs as these two bodies with zero call-site edits, which is why no other module in the branch converts time into the physical half.
- Law: `genesis` is the zero stamp — the fold seed for empty causality chains and the identity every replay starts from.
- Law: interior mints ride `Schema.decodeSync` over proven inputs — zero literals, `+ 1n` on a non-negative brand, zero-clamped epoch reads — so the throw path is structurally unreachable and the mint stays inside the trusted-construction channel `new Hlc` completes.
- Growth: a new clock fold (bounded-drift tick, batch merge) is one static composing the existing folds; a new stamp field is a wire-shape question for `wire`, never a widening here.
- Boundary: wall-clock reads ride the consumer's rail (`DateTime.now` under its `Clock` service) and arrive as values; the uncertainty window over the same physical axis is `clock/uncertainty`'s owner.
- Packages: `effect` (`Schema`, `Order`, `DateTime`, `Duration`, `ParseResult`).

## [3]-[TWO_HALF_LAYOUT]

[TWO_HALF_LAYOUT]:
- Owner: `Hlc.FromBytes`, the sixteen-byte layout twin riding the class as a static — bytes 0..7 the physical half, bytes 8..15 the logical half, both read and written little-endian (`getBigUint64(offset, true)`/`setBigUint64(offset, true)`) — so the byte order, the half order, and the endianness flag live in one declaration and the parity drivers assert exactly this twin against the frozen vectors.
- Law: decode is total over sixteen admitted bytes — both halves land non-negative by construction and the class filters re-prove; encode is the one overflow seam — a half above `0xffffffffffffffff` fails as a typed `ParseResult.Type` fault at the boundary, so the unbounded value space meets the u64 wire honestly.
- Law: a half-swap is the named catastrophic drift — reading the logical half into the physical slot silently folds fresh ops as stale — so the offsets are spelled once here and every byte-carried stamp in the branch decodes through this twin, never through a second `DataView` read.
- Exemption: `_unpacked`/`_packed` are marked `// BOUNDARY ADAPTER` kernels — `DataView` reads and writes are the platform-forced statement seam; each detaches an immutable value, and the implementer carries the mark on each kernel's first line.
- Growth: a JSON or proto stamp spelling is a `wire` codec built from `Hlc.fields`; a second byte layout is unspellable — this twin is the layout.
- Boundary: the fixture bytes and the cross-runtime equality grade live in `tests/contracts` with TS readers in `tests/typescript/_testkit`; this owner fixes the reproduction obligation and stores no fixture.
- Packages: the `DataView` platform surface inside the marked kernels.

```typescript
import { DateTime, Duration, Order, ParseResult, Schema } from "effect"

const _Physical = Schema.BigIntFromSelf.pipe(Schema.nonNegativeBigInt(), Schema.brand("HlcPhysical"))
const _Logical = Schema.BigIntFromSelf.pipe(Schema.nonNegativeBigInt(), Schema.brand("HlcLogical"))
const _Bytes = Schema.Uint8ArrayFromSelf.pipe(Schema.filter((bytes) => bytes.length === 16))
const _U64 = 0xffffffffffffffffn

const _physical = Schema.decodeSync(_Physical)
const _logical = Schema.decodeSync(_Logical)
const _FLOOR = _logical(0n)

const _succ = (held: typeof _Logical.Type): typeof _Logical.Type => _logical(held + 1n)

const _unpacked = (bytes: Uint8Array): { readonly physical: bigint; readonly logical: bigint } => {
  const view = new DataView(bytes.buffer, bytes.byteOffset, bytes.byteLength)
  return { physical: view.getBigUint64(0, true), logical: view.getBigUint64(8, true) }
}

const _packed = (stamp: { readonly physical: bigint; readonly logical: bigint }): Uint8Array => {
  const view = new DataView(new ArrayBuffer(16))
  view.setBigUint64(0, stamp.physical, true)
  view.setBigUint64(8, stamp.logical, true)
  return new Uint8Array(view.buffer)
}

class Hlc extends Schema.Class<Hlc>("Hlc")({
  physical: _Physical,
  logical: _Logical,
}) {
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
  static readonly delta = (span: Duration.DurationInput): Hlc.Physical => _physical(BigInt(Math.trunc(Duration.toMillis(span))))
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

// --- [EXPORTS] --------------------------------------------------------------------------

export { Hlc }
```
