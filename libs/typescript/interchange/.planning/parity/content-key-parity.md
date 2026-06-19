# [INTERCHANGE_CONTENT_KEY_PARITY]

The cross-runtime byte-identity contract the wire boundary satisfies: the interchange-side reproduction binding that asserts the worker-minted 16-byte `ContentKey` over an assembled blob is bit-identical to the C#-owned `XxHash128` digest, and the `useBigInt64`/`getBigUint64` HLC two-half round-trip preserves the two-64-bit-half order the conflict-presence fold depends on. The C# seed is the single mint (`csharp:Rasm/Geometry/topology/reconciliation#ONE_WIRE_FIXTURE_CORPUS`, seed zero, two-64-bit-half order); this page owns no fixture bytes and re-mints no digest — it fixes the normalization and the assertion vocabulary the worker reassembly already produces, anchored to the FROZEN `CANONICAL_BYTE_IDENTITY` corpus row. The harness DRIVER that feeds the corpus through the reassembly and grades the equality is the future `typescript:testing/` consumer of the same corpus; this page is the interchange obligation that driver verifies, never a second fixture store.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]            | [OWNS]                                                                       |
| :-----: | :------------------- | :-------------------------------------------------------------------------- |
|   [1]   | CONTENT_KEY_PARITY   | the frozen-corpus reproduction binding, the LE↔BE normalize, the equality   |
|   [2]   | HLC_TWO_HALF_PARITY  | the bigint two-half round-trip the convergence fold reads                   |
|   [3]   | RESEARCH             | the HLC two-half fixture gate on the absent upstream pin                     |

## [2]-[CONTENT_KEY_PARITY]

- Owner: `ContentKeyParity`, the interchange-side reproduction binding asserting the worker content key over an assembled blob equals the C# `XxHash128.HashToUInt128` digest the federation mints once. The reference is the FROZEN `csharp:Rasm/Geometry/topology/reconciliation#ONE_WIRE_FIXTURE_CORPUS` row [1] `CANONICAL_BYTE_IDENTITY` — the 52-byte int32-LE canonical-adjacency stream of the single-triangle topology (`VertexCount=3`; edges `(0,1),(0,2),(1,2)`; face cycle `[0,1,2]`) and its `XxHash128.HashToUInt128` digest `0x9462A71A5DD13DCFA3B1D6D225FCBE70`, persisted big-endian and read by the C# side as the 16-byte LE memory `70 be fc 25 d2 d6 b1 a3 cf 3d d1 5d 1a a7 62 94`. No byte of the corpus is re-authored here; the reference literals are transcribed verbatim from the producer page that froze them.
- Cases: the binding feeds the frozen 52-byte stream through the same `artifacts/frame-reassembly.md` `xxHash128Of` `IHasher` binding the worker reassembly mints with — `createXXHash128(0, 0)` `init`/`update`/`digest("binary")` yields the 16-byte little-endian digest, and the `digest("binary").reverse()` normalize at the `h128` boundary produces the C# big-endian canonical key — then asserts byte-equality against the frozen canonical key. The assertion compares the NORMALIZED 16-byte arrays byte-for-byte, never the raw little-endian hex `digest()` string against the big-endian C# hex, because `createXXHash128` emits little-endian while `System.IO.Hashing.XxHash128` persists big-endian (the `hash-wasm` `[ENDIANNESS_LAW]`) — a reference-equality compare over the `Uint8Array` brand is the rejected form, the assertion reads the 16 bytes.
- Entry: `contentKeyOf(hasher)` reproduces the frozen reference key through the production `xxHash128Of` binding so the parity binding and the worker reassembly share ONE mint path — the binding never re-resolves a hasher or re-derives a digest, it routes the production `xxHash128Of(hasher).h128(bytes)` and asserts equality, so a green parity assertion proves the worker reassembly's content key reproduces the C# seed bit-identically. `CANONICAL_REFERENCE` carries the frozen stream and the frozen normalized key as the two transcribed corpus literals; the equality is `bytesEqual(contentKeyOf(hasher)(CANONICAL_REFERENCE.stream), CANONICAL_REFERENCE.contentKey)`.
- Packages: `hash-wasm` `createXXHash128(0, 0)` `IHasher` resolved once at the worker composition root and threaded as the production binding; `effect` `Schema.decodeSync(ContentKey)` to lift the frozen literal into the branded slot (the settled `frame-reassembly.md` `h128` idiom).
- Growth: a new REAL corpus fixture lands as one `CANONICAL_REFERENCE` row the binding reproduces (the FROZEN `CLASH_GOLDEN` row [2] is the next REAL fixture the federation pins); a DESIGN-PIN corpus row carries no reference literal here until its producer pins the byte-deriving input; zero second digest function and zero re-minted fixture.
- Boundary: the parity binding reproduces the one C#-owned seed and NEVER mints a second content-address notion — the reference digest is the producer's frozen byte set, transcribed not recomputed, and a fabricated digest for an unpinned fixture is the rejected form; the binding routes the production `xxHash128Of` so it shares the single-hash-mint that lives at `platform/worker/decode-pool.md`, never a parallel parity-only hasher; the equality compares the normalized big-endian key the `h128` boundary already produced, never the raw little-endian hex, so the LE↔BE normalize is asserted once at the boundary and the harness reads the canonical key both sides agree on; the harness DRIVER (the cross-package byte-equality grade over the full reassembly path) is the `typescript:testing/` corpus consumer, so this page fixes the reproduction obligation and never duplicates the test-authoring spine the branch testing folder owns.

```ts contract
interface ContentKeyParity {
  readonly contentKeyOf: (hasher: IHasher) => (bytes: Uint8Array) => RefinedIdentity["contentKey"];
  readonly reproduces: (hasher: IHasher, stream: Uint8Array, expected: RefinedIdentity["contentKey"]) => boolean;
}

const CANONICAL_REFERENCE: { readonly stream: Uint8Array; readonly contentKey: RefinedIdentity["contentKey"] } = {
  stream: Uint8Array.from([
    0x03, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
    0x01, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
    0x02, 0x00, 0x00, 0x00,
  ]),
  contentKey: Schema.decodeSync(ContentKey)(
    Uint8Array.from([
      0x70, 0xbe, 0xfc, 0x25, 0xd2, 0xd6, 0xb1, 0xa3, 0xcf, 0x3d, 0xd1, 0x5d, 0x1a, 0xa7, 0x62, 0x94,
    ]),
  ),
};

const bytesEqual = (a: Uint8Array, b: Uint8Array): boolean =>
  a.length === b.length && a.every((byte, i) => byte === b[i]);

const contentKeyParity: ContentKeyParity = {
  contentKeyOf: (hasher) => xxHash128Of(hasher).h128,
  reproduces: (hasher, stream, expected) => bytesEqual(xxHash128Of(hasher).h128(stream), expected),
};
```

## [3]-[HLC_TWO_HALF_PARITY]

- Owner: `HlcTwoHalfParity`, the bigint two-half round-trip binding asserting the `OpLogEntryWire.physical`/`logical` HLC stamp and the `SnapshotHeaderWire.schemaFingerprint` cross `@msgpack/msgpack` `useBigInt64: true` and the `DataView` `getBigUint64(offset, false)` big-endian read holding the same two-64-bit-half order the C# `BinaryPrimitives.WriteUInt64BigEndian` persists. The `logical` half is the load-bearing one: an HLC `logical` off-by-one-half folds a fresh op as stale with no other signal, so the half-order assertion gates the `projection` conflict-presence fold.
- Cases: the snapshot-header fingerprint crosses through the `codecs/decode-rail.md` `decodeSnapshotHeader` `getBigUint64(0, false)` big-endian read (the `littleEndian: false` flag load-bearing); the op-log `logical`/`sequence` bigints cross through the reused `snapshotDecoder` `Decoder({ useBigInt64: true })` mapping the MessagePack 64-bit integers to bigint. The round-trip asserts the decoded bigint equals the canonical two-half composition `(physicalHigh << 32n) | logicalLow` order, so a half-swap that reads the logical half into the physical slot is caught before it corrupts the convergence order.
- Entry: `roundTrip(low, high)` composes the two 32-bit halves into the canonical bigint and asserts the `getBigUint64(_, false)` read of the big-endian-written 8-byte buffer returns the identical bigint, so the `false` endianness flag and the half order are both asserted against the C# write order in one pass. The reference two-half stamp is the DESIGN-PIN corpus row [6] `HLC_TWO_HALF`; until that row is pinned the binding carries the round-trip ALGEBRA but no frozen reference stamp, so the algebra is settled and only the cross-runtime byte-equality against a producer-frozen stamp stays gated.
- Packages: `@msgpack/msgpack` `Decoder({ useBigInt64: true })` for the MessagePack bigint mapping; `effect` for the assertion fold; the `DataView` `getBigUint64`/`setBigUint64` is the BCL inbox big-endian read the snapshot-header decode already carries.
- Growth: the frozen `HLC_TWO_HALF` reference stamp lands as one `HLC_REFERENCE` row the binding asserts against the moment `csharp:Rasm.AppHost/ports/runtime-ports#HLC_FANIN` pins it; zero second HLC encoding notion.
- Boundary: the binding asserts the C#-owned half order and NEVER re-mints an HLC encoding — the two-half order is the producer's, transcribed when the corpus row freezes it; the `getBigUint64(_, false)` big-endian flag is the one read order the snapshot header and the parity binding both use, never a little-endian read beside it; the `physical` ISO-8601 instant the `projection` event-time fold reads is the C#-owned HLC physical half against `csharp:Rasm.Persistence/sync/collaboration#TS_PROJECTION` (`physical: string`), distinct from the `logical` bigint half this binding asserts; the harness DRIVER that grades the round-trip against the frozen stamp is the `typescript:testing/` corpus consumer, never duplicated here.

```ts contract
interface HlcTwoHalfParity {
  readonly compose: (physicalHalf: bigint, logicalHalf: bigint) => bigint;
  readonly roundTrip: (physicalHalf: bigint, logicalHalf: bigint) => boolean;
}

const hlcTwoHalfParity: HlcTwoHalfParity = {
  compose: (physicalHalf, logicalHalf) => (physicalHalf << 32n) | (logicalHalf & 0xffffffffn),
  roundTrip: (physicalHalf, logicalHalf) => {
    const composed = (physicalHalf << 32n) | (logicalHalf & 0xffffffffn);
    const view = new DataView(new ArrayBuffer(8));
    view.setBigUint64(0, composed, false);
    return view.getBigUint64(0, false) === composed;
  },
};
```

## [4]-[RESEARCH]

- [HLC_TWO_HALF_FIXTURE]: the `HlcTwoHalfParity.roundTrip` algebra is settled against the `@msgpack/msgpack` `useBigInt64` and `DataView` `getBigUint64(_, false)` surfaces both verified at `.api/msgpack-msgpack.md` and the snapshot-header decode, but the cross-runtime byte-equality assertion needs the FROZEN two-half HLC reference stamp. The `csharp:Rasm/Geometry/topology/reconciliation#ONE_WIRE_FIXTURE_CORPUS` row [6] `HLC_TWO_HALF` is DESIGN-PIN on `csharp:Rasm.AppHost/ports/runtime-ports#HLC_FANIN`, a producer cluster that the runtime-ports page does not yet carry — the page's `HlcStampWire` is the receipt-envelope stamp (`logical: number`) on `#TS_PROJECTION`, not the two-half bigint fixture the op-log `logical` rides. Until that producer pins the two-half stamp bytes, the round-trip algebra holds but its cross-runtime equality stays HOST/UPSTREAM-gated on the absent `HLC_TWO_HALF` corpus pin; no fabricated stamp stands in for the unpinned fixture.
