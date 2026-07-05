# [PY_RUNTIME_REPRODUCTION]

The cross-runtime seed-parity binding, the module `rasm.runtime.reproduction` — split out of `evidence/identity` so its `receipts` import stays DAG-legal (`identity < receipts < reproduction`). `SeedReproduction` asserts `ContentIdentity` reproduces the one C#-owned `XxHash128` seed bit-identically across the frozen `ONE_WIRE_FIXTURE_CORPUS` federation index; it re-mints no digest, authors no fixture byte, and consumes the production `ContentIdentity.of`/`ContentKey.project` surface so a derivation or render regression surfaces as a failed parity receipt, never a pass against a parallel path. The suite is one fault-COMBINING fold: every graded fixture reaches its verdict and the faults accumulate into one `aggregate`, so a single fixture's fault never masks a later fixture's evidence.

## [01]-[INDEX]

- [01]-[SEED_REPRODUCTION]: the corpus-parameterized parity suite whose `grade` folds every REAL `CorpusFixture` through `traversed(ACCUMULATE)` into `ParityReceipt` rows the `ReceiptContributor` port contributes, each GATED or DESIGN-PIN fixture riding a state-keyed `planned`-phase obligation — the nine-row `_CORPUS` transcribing the `csharp:Rasm/Spatial/reconciliation#ONE_WIRE_FIXTURE_CORPUS` index rows [1]-[7] plus the two admitted compute fixtures, the fixture payload's SHAPE keying the `IdentitySource.lift` modality, and the four-aspect `ParityAspect` vocabulary closing digest-value, `InterchangeIdentity.Key` hex-render, LE-memory, and whole-`ContentKey` coverage — every `KeyView` member proven — over one fold.

## [02]-[SEED_REPRODUCTION]

- Owner: `SeedReproduction` is the frozen suite owner whose one `corpus: Block[CorpusFixture]` field defaults to `_CORPUS`, so the runtime suite and a sibling-authored corpus (data's icechunk snapshot-seed fixtures) ride one constructor argument over the same exported row types, never a second suite class or fixture store. `CorpusFixture` is the typed corpus row — a fixture `name` (the producer's wire `fmt` tag: `grade` keys the derivation off it, and a name≠fmt drift fails the hex row rather than passing silently), its `FixtureState`, the `producer` seam anchor it transcribes, the `Option[FixturePayload]` producer-frozen seed-zero reference, and the `Block[ParityRow]` it grades — and the payload's VALUE SHAPE keys the derivation modality through `IdentitySource.lift`: `bytes` derives `whole`, `tuple[bytes, ...]` derives `stream`, `tuple[ContentKey, ...]` derives `merkle`, so a modality is never a flag beside the value. `ParityAspect` is the closed four-member vocabulary (`value_identity`/`hex_identity`/`memory_layout`/`key_identity`) — `key_identity` grades the `"value"` view's whole `ContentKey` by structural equality, so the `fmt` threading and the `byte_length` ledger the merkle `sum_by(byte_length)` spine sums ride the parity fold, evidence no scalar view reaches; `ParityRow` pairs each aspect with the `KeyView` projector and the typed `KeyRender` expected reference, owning the `grade(fixture, key)` that derives the observed render and the verdict; `ParityReceipt` is the evidence row `grade` yields, owning the `fact` projection to its `(fixture.aspect, status)` log pair. `_CORPUS` is the nine-row `Block[CorpusFixture]` federation index — rows [1]-[7] transcribed from `csharp:Rasm/Spatial/reconciliation#ONE_WIRE_FIXTURE_CORPUS`, rows [8]-[9] the two admitted compute fixtures — each row binding its producer under the one C# seed (seed zero, two-64-bit-half order), the single mint this binding never re-authors: a REAL fixture transcribes the producer-frozen reference verbatim, a GATED fixture (`clash-golden` — REAL on its producer, its stream literals citable only after the one-time harness reproof) and a DESIGN-PIN fixture both carry `Nothing` and no rows until the producer freezes or the gate confirms the byte-deriving reference.
- Reference: the FROZEN 52-byte int32-LE canonical-adjacency stream of the single-triangle topology (`VertexCount=3`; edges `(0,1),(0,2),(1,2)`; face cycle `[0,1,2]`) is `03 00 00 00 03 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 02 00 00 00 01 00 00 00 02 00 00 00 01 00 00 00 03 00 00 00 00 00 00 00 01 00 00 00 02 00 00 00`, its `XxHash128.HashToUInt128` digest under seed zero is `0x9462A71A5DD13DCFA3B1D6D225FCBE70`, persisted big-endian and read C#-side as the 16-byte LE memory `70 be fc 25 d2 d6 b1 a3 cf 3d d1 5d 1a a7 62 94`, and `CANONICAL_HEX` is the `{value:032x}:{fmt}` render of that same frozen digest — the C# `InterchangeIdentity.Key`/companion-GLB key string form, a derivation off `CANONICAL_DIGEST`, never a second literal; the whole-key expected row derives its `byte_length` from `len(CANONICAL_STREAM)`, so row [1] proves the digest value, the `fmt` threading, the LE layout, AND the byte-length ledger off the one frozen literal pair. The float-bearing `MATERIAL_LAYER_GOLDEN` golden vector ([H7]) is row [7]: an `IfcMaterialLayer`-shaped `MaterialComposition.LayerSet` node whose `csharp:Rasm.Element/Projection/address#CONTENT_ADDRESS` `CanonicalWriter` bytes — case ordinal `1`, layer count, then per layer the material-id length-prefixed UTF-8, the `ThicknessMm` `Measure` (the length-prefixed `QuantityType` discriminator token the C# `CanonicalWriter.Measure` writes FIRST through `String(q.Type.Value)`, then the IEEE-754 little-endian `Si` magnitude with `-0.0`→`0.0`/`NaN`→one quiet pattern quantized to `Header.Tolerance`, then the 7 SI `Dimension` exponent ordinals — the type token separates same-dimension quantities so dropping it would fork the digest), and the layer-name UTF-8 — are hashed seed-zero, the ONLY corpus fixture that exercises the `Double`/`Measure` float canon the integer-topology row [1] cannot reach; it grades the moment its producer pins the node through the SAME `whole`/seed-zero path row [1] uses.
- Entry: `SeedReproduction.grade` is one suite-wide fold: `corpus.choose` drains each fixture's `Option` payload, a present payload derives its `ContentKey` through `ContentIdentity.of(fixture.name, payload, seed=Some(0))` — the bare C# `HashToUInt128(span)` seed-zero path, the payload shape selecting the `whole`/`stream`/`merkle` arm with zero fixture-side dispatch — and the fixture's rows grade against that one key, so row [1]'s digest-value, hex-render, LE-memory, and whole-key checks are four rows of one fold rather than parallel boolean methods. The per-fixture rails accumulate through `reliability/faults#FAULT` `traversed(by=Disposition.ACCUMULATE)` — every REAL fixture is graded and the faults combine into one `aggregate BoundaryFault` — and the graded blocks flatten through `Block.collect(identity)` into the one `RuntimeRail[Block[ParityReceipt]]`. `contribute` satisfies the `observability/receipts#RECEIPT` `ReceiptContributor` Protocol: the graded fold maps the `Ok` rows through `ParityReceipt.fact` into one `dict` behind an `emitted`-phase `Receipt.of(SUITE, ...)`, the `Error` arm folds the accumulated fault through `Receipt.of` into the receipts owner's `rejected` case, and the two `Receipt`-typed arms collapse through `Result.merge` (both branches the SAME type, the member's own constraint); each pending fixture additionally mints one `planned`-phase obligation whose fact keys the fixture's state to its producer — `{"gated": ...}` or `{"design_pin": ...}` off the one `{fixture.state: fixture.producer}` spelling with zero branch — so every ungraded parity is a VISIBLE obligation on the one receipt stream the metrics/lanes fold reads, never a bare assert or a silent gap.
- Auto: `xxhash.xxh3_128_intdigest` returns the 128-bit digest as a Python `int` whose `ContentKey.memory` (`to_bytes(16, "little")`) is the C# `UInt128` in-memory layout — `XxHash128.HashToUInt128` returns a `UInt128` and `BinaryPrimitives.WriteUInt128LittleEndian` writes the same LE memory, so value-equality holds with no byte-swap when both sides read seed zero. The corpus seed is zero because the C# `NamingHashOps.Encode`/`GeometryHash` path calls `XxHash128.HashToUInt128(span)` with no seed parameter, distinct from the settings-folded `ContentIdentity.seed` the tessellation rail derives — the seed origin rides the `seed=Some(0)` parameter, and the format-policy seed governs only the re-tessellation cache identity. The merkle arm's `b"".join(child.memory)` spine reproduces the C# `CommitGraph.Of`/`MerkleRange.Of`/`CrdtWire.ContentKey` LE child transcription, so `crdt-op-set` graduates as an order-sensitive `tuple[ContentKey, ...]` derivation; a chunked producer reference graduates through the stream arm's stateful updater. The parity reuses the production `ContentIdentity.of` and `ContentKey.project` — `hex_identity` grades the exact `{value:032x}:{fmt}` render the companion-GLB key law fixes — so a regression in the owner's derivation OR projection surfaces as a failed `ParityReceipt`, never a pass against a parallel fixture path.
- Packages: `xxhash` (`xxh3_128_intdigest` — reused INDIRECTLY through `ContentIdentity.of`, never called in this body), `expression` (`Block.of_seq`/`empty`/`choose`/`map`/`collect` the corpus and flatten folds, `identity` the `collect` flattener, `Result.map`/`map_error`/`merge` the two-arm receipt collapse, `Option.map`/`is_none` the payload-gated REAL-vs-pending dispatch, `Nothing`/`Some` the seed override and the pending gap), `msgspec` (`Struct` the rows; `gc=False` on the container-free `ParityReceipt`/`ParityRow` per the leaf-topology law the sibling value objects hold); runtime (`faults.traversed`/`Disposition`/`RuntimeRail` the suite accumulation rail, `identity.ContentIdentity`/`ContentKey`/`KeyRender`/`KeyView`, `receipts.Receipt` — every import strictly downward in the `[00]` order).
- Growth: a new parity aspect is one `ParityAspect` member plus one `ParityRow` on the owning fixture, reaching the fold and the contributed receipt through the existing `grade`/`fact`; a new cross-runtime fixture is one `CorpusFixture` row on `_CORPUS`; a GATED or DESIGN-PIN fixture GRADUATES by one `Some(payload)` plus its rows plus the `state="real"` flip, with zero new method — `glb-by-key` graduates with digest, memory, AND hex rows (the hex row proving the companion-GLB `InterchangeIdentity.Key` form on the fixture that demands it), `crdt-op-set` through the merkle arm, `clash-golden` the instant its harness gate confirms the 160-byte stream literals; a sibling-authored corpus is the `corpus` constructor argument over the same exported `CorpusFixture`/`ParityRow` types; a new derivation modality is one `FixturePayload` member only when `IdentitySource` itself grows one.
- Boundary: the corpus is read-only and the C# seed is the single mint — a Python-fabricated byte set for an unpinned or gate-pending row, a per-runtime digest function, a parallel boolean method per assertion, a bind-abort suite fold that lets one fixture's fault mask every later verdict where `traversed(ACCUMULATE)` combines them, a `Struct` payload arm whose local canonical re-encode would be a second mint beside the producer-frozen bytes, a modality flag beside the payload shape the value already discriminates, and a second fixture store beside the `corpus` parameter are the named drift defects. Rows [3]-[9] stay pending on their cross-folder producers (`fault-triples` on `csharp:Rasm.Compute/Runtime/channels#FAULT_PROJECTION`, `crdt-op-set` on `csharp:Rasm.Persistence/Version/commits#CRDT_ALGEBRA`, `glb-by-key` on `csharp:Rasm.Compute/Runtime/codecs#TILE_PARTITION` with geometry `mesh/daemon` the graduating consumer, `hlc-two-half` on `csharp:Rasm.AppHost/Runtime/ports#HLC_FANIN`, `material-layer-golden` on `csharp:Rasm.Element/Projection/address#CONTENT_ADDRESS`, `array-layout` on `python:compute/numerics/array#PAYLOAD`, `evidence-bundle` on the `csharp:Rasm.Compute/Runtime` graduation-evidence wire with `python:compute/graduation/codegen` pairing the bundle bytes with the expected generated-stub projection so the corpus proves decode AND emit round-trip); the harness DRIVER feeding payloads through `ContentIdentity.of` and grading the receipt rows is a future `python:testing` consumer of this same corpus, never a second fixture store here.

```python signature
from collections.abc import Iterable
from typing import Final, Literal

from expression import Nothing, Option, Some, identity
from expression.collections import Block
from msgspec import Struct

from rasm.runtime.faults import Disposition, RuntimeRail, traversed
from rasm.runtime.identity import ContentIdentity, ContentKey, KeyRender, KeyView
from rasm.runtime.receipts import Receipt

# --- [TYPES] ----------------------------------------------------------------------------

# `key_identity` grades the `"value"` view's whole `ContentKey` — the fmt threading and the
# `byte_length` ledger the merkle `sum_by(byte_length)` spine sums, evidence no scalar view reaches.
type ParityAspect = Literal["value_identity", "hex_identity", "memory_layout", "key_identity"]
# `real` grades NOW; `gated`/`design_pin` carry `Nothing` and ride the state-keyed `planned`
# obligation — `gated` awaits the one-time harness reproof, `design_pin` the producer's freeze.
type FixtureState = Literal["real", "gated", "design_pin"]
# the payload SHAPE keys the `IdentitySource.lift` arm — `bytes` -> whole, `tuple[bytes, ...]` ->
# stream, `tuple[ContentKey, ...]` -> merkle; a live `Struct` is excluded because the corpus
# transcribes producer-frozen bytes and a local canonical re-encode would be a second mint.
type FixturePayload = bytes | tuple[bytes, ...] | tuple[ContentKey, ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

IDENTITY_FMT: Final[str] = "geometry-topology"
SUITE: Final[str] = "seed-reproduction"
# [1] CANONICAL_BYTE_IDENTITY — frozen on csharp:Rasm/Spatial/reconciliation#ONE_WIRE_FIXTURE_CORPUS[1]:
# the 52-byte int32-LE single-triangle adjacency stream and its seed-zero XxHash128 digest; the LE
# memory stays a producer-TRANSCRIBED literal — a `to_bytes` derivation would grade a tautology.
CANONICAL_STREAM: Final[bytes] = bytes.fromhex(
    "03000000030000000000000001000000000000000200000001000000020000000100000003000000000000000100000002000000"
)
CANONICAL_DIGEST: Final[int] = 0x9462A71A5DD13DCFA3B1D6D225FCBE70
CANONICAL_LE_MEMORY: Final[bytes] = bytes.fromhex("70befc25d2d6b1a3cf3dd15d1aa76294")
# the C# InterchangeIdentity.Key `{value:032x}:{fmt}` render of the frozen digest — derived, never a second literal.
CANONICAL_HEX: Final[str] = f"{CANONICAL_DIGEST:032x}:{IDENTITY_FMT}"

# --- [MODELS] ---------------------------------------------------------------------------


class ParityReceipt(Struct, frozen=True, gc=False):
    fixture: str
    aspect: ParityAspect
    expected: KeyRender
    observed: KeyRender
    verified: bool

    @property
    def fact(self) -> tuple[str, str]:
        return f"{self.fixture}.{self.aspect}", "ok" if self.verified else f"{self.observed!r}!={self.expected!r}"


class ParityRow(Struct, frozen=True, gc=False):
    aspect: ParityAspect
    view: KeyView
    expected: KeyRender

    def grade(self, fixture: str, key: ContentKey) -> ParityReceipt:
        observed = key.project(self.view)
        return ParityReceipt(fixture=fixture, aspect=self.aspect, expected=self.expected, observed=observed, verified=observed == self.expected)


class CorpusFixture(Struct, frozen=True):
    # one cross-runtime golden fixture: `name` IS the producer's wire `fmt` tag `grade` derives
    # under; a REAL row carries `Some(payload)` + expected renders; a GATED/DESIGN-PIN row names
    # its `producer`, carries `Nothing`, and graduates by one `Some(payload)` + rows + state flip.
    name: str
    state: FixtureState
    producer: str
    payload: Option[FixturePayload]
    rows: Block[ParityRow]


# --- [TABLES] ---------------------------------------------------------------------------

_CORPUS: Final[Block[CorpusFixture]] = Block.of_seq((
    CorpusFixture(
        name=IDENTITY_FMT,
        state="real",
        producer="csharp:Rasm/Spatial/reconciliation#ONE_WIRE_FIXTURE_CORPUS[1]",
        payload=Some(CANONICAL_STREAM),
        rows=Block.of_seq((
            ParityRow(aspect="value_identity", view="digest", expected=CANONICAL_DIGEST),
            ParityRow(aspect="hex_identity", view="hex", expected=CANONICAL_HEX),
            ParityRow(aspect="memory_layout", view="memory", expected=CANONICAL_LE_MEMORY),
            ParityRow(
                aspect="key_identity",
                view="value",
                expected=ContentKey(value=CANONICAL_DIGEST, fmt=IDENTITY_FMT, byte_length=len(CANONICAL_STREAM)),
            ),
        )),
    ),
    # [2] REAL on the producer, HARNESS-GATED here: the 160-byte Bounds/Nodes LE stream re-derives
    # under the outward-rounding law and its literals are citable only after the one-time reproof.
    CorpusFixture(name="clash-golden", state="gated", producer="csharp:Rasm/Spatial/index#CLASH_GOLDEN", payload=Nothing, rows=Block.empty()),
    # [3] FaultDetail (package, code, case) triples spanning the disjoint fault-code bands.
    CorpusFixture(
        name="fault-triples", state="design_pin", producer="csharp:Rasm.Compute/Runtime/channels#FAULT_PROJECTION", payload=Nothing, rows=Block.empty()
    ),
    # [4] the MvRegister/opMerge op-set whose divergent-delivery folds converge byte-identically —
    # graduates through the MERKLE arm, the C# CrdtWire.ContentKey LE child-transcription spine.
    CorpusFixture(
        name="crdt-op-set", state="design_pin", producer="csharp:Rasm.Persistence/Version/commits#CRDT_ALGEBRA", payload=Nothing, rows=Block.empty()
    ),
    # [5] one content-keyed GLB sample — graduates with digest + memory + hex rows, the hex row
    # proving the companion-GLB InterchangeIdentity.Key form; geometry mesh/daemon graduates it.
    CorpusFixture(
        name="glb-by-key", state="design_pin", producer="csharp:Rasm.Compute/Runtime/codecs#TILE_PARTITION", payload=Nothing, rows=Block.empty()
    ),
    # [6] the two-64-bit-half HLC stamps whose half order an off-by-one-half would corrupt.
    CorpusFixture(
        name="hlc-two-half", state="design_pin", producer="csharp:Rasm.AppHost/Runtime/ports#HLC_FANIN", payload=Nothing, rows=Block.empty()
    ),
    # [7] MATERIAL_LAYER_GOLDEN ([H7]) — the float-bearing IfcMaterialLayer-shaped LayerSet node whose
    # CanonicalWriter IEEE-754-LE bytes exercise the Double/Measure canon row [1] cannot reach.
    CorpusFixture(
        name="material-layer-golden",
        state="design_pin",
        producer="csharp:Rasm.Element/Projection/address#CONTENT_ADDRESS",
        payload=Nothing,
        rows=Block.empty(),
    ),
    # [8] cross-backend array-layout bit-identity — compute keys canonical array bytes and freezes
    # the reference on its array-admission owner.
    CorpusFixture(name="array-layout", state="design_pin", producer="python:compute/numerics/array#PAYLOAD", payload=Nothing, rows=Block.empty()),
    # [9] the C#-minted graduation-evidence golden bundle — pairs the bundle bytes with the expected
    # generated-stub projection so the corpus proves decode AND emit round-trip byte-stability.
    CorpusFixture(name="evidence-bundle", state="design_pin", producer="csharp:Rasm.Compute/Runtime", payload=Nothing, rows=Block.empty()),
))

# --- [COMPOSITION] ----------------------------------------------------------------------


class SeedReproduction(Struct, frozen=True):
    # the one suite owner: the default corpus is the federation index; a sibling-authored corpus
    # (data's icechunk snapshot-seed fixtures) is this one constructor argument, never a second suite.
    corpus: Block[CorpusFixture] = _CORPUS

    def grade(self) -> RuntimeRail[Block[ParityReceipt]]:
        # every present payload derives once through the production seed-zero path (the payload
        # shape keying whole/stream/merkle) and grades its rows; faults COMBINE under ACCUMULATE so
        # one fixture never masks a later verdict; `Nothing` fixtures ride `contribute`'s obligations.
        rails = self.corpus.choose(
            lambda fixture: fixture.payload.map(
                lambda payload: ContentIdentity.of(fixture.name, payload, seed=Some(0)).map(
                    lambda key: fixture.rows.map(lambda row: row.grade(fixture.name, key))
                )
            )
        )
        return traversed(rails, by=Disposition.ACCUMULATE).map(lambda graded: graded.collect(identity))

    def contribute(self) -> Iterable[Receipt]:
        # the graded suite collapses to ONE emitted-or-rejected receipt — both arms Receipt-typed,
        # satisfying `Result.merge`; each pending fixture mints one `planned` obligation keyed
        # `{state: producer}`, so an ungraded parity is a VISIBLE obligation, never a silent gap.
        graded = (
            self.grade()
            .map(lambda rows: Receipt.of(SUITE, ("emitted", SUITE, dict(rows.map(lambda receipt: receipt.fact)))))
            .map_error(lambda fault: Receipt.of(SUITE, fault))
            .merge()
        )
        pending = self.corpus.choose(
            lambda fixture: (
                Some(Receipt.of(fixture.name, ("planned", fixture.name, {fixture.state: fixture.producer}))) if fixture.payload.is_none() else Nothing
            )
        )
        return (graded, *pending)
```
