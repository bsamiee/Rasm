# [PY_RUNTIME_REPRODUCTION]

Cross-runtime seed parity binds here, the module `rasm.runtime.reproduction`: `SeedReproduction` asserts `ContentIdentity` reproduces the one C#-owned `XxHash128` seed bit-identically across the frozen `ONE_WIRE_FIXTURE_CORPUS` federation index. It re-mints no digest and authors no fixture byte — it consumes the production `ContentIdentity.of`/`ContentKey.project` surface, so a derivation or render regression surfaces as a failed parity receipt, never a pass against a parallel path.

One fault-combining fold carries the suite — `reliability/faults#FAULT` `traversed(by=Disposition.ACCUMULATE)` — so a single fixture's fault never masks a later fixture's evidence, and `contribute` satisfies the `observability/receipts#RECEIPT` `ReceiptContributor` port, each pending fixture riding a state-keyed `planned` obligation. This module splits out of `evidence/identity` so its `receipts` import stays DAG-legal (`identity < receipts < reproduction`).

## [01]-[INDEX]

- [01]-[SEED_REPRODUCTION]: the corpus-parameterized parity suite — `_CORPUS` fixtures graded through the four-aspect `ParityAspect` vocabulary over one accumulate fold, every `KeyView` member proven.

## [02]-[SEED_REPRODUCTION]

- Owner: `_CORPUS` transcribes the `csharp:Rasm/Spatial/reconciliation#ONE_WIRE_FIXTURE_CORPUS` federation index and the admitted compute fixtures, each row binding its producer under the one C# seed — the single mint this binding never re-authors: a REAL fixture transcribes the producer-frozen reference verbatim, a GATED or DESIGN-PIN fixture carries `Nothing` and no rows until the producer freezes or the gate confirms the reference. A fixture `name` IS the producer's wire `fmt` tag, so a name-vs-fmt drift fails the hex row rather than passing silently. `key_identity` grades the whole `ContentKey` by structural equality, so the `fmt` threading and the `byte_length` ledger ride the parity fold — evidence no scalar view reaches.
- Auto: `xxhash` returns the digest as a Python `int` whose `to_bytes(16, "little")` IS the C# `UInt128` in-memory layout `BinaryPrimitives.WriteUInt128LittleEndian` writes, so value-equality holds with no byte-swap when both sides read seed zero. Corpus seed is zero because the C# `NamingHashOps.Encode`/`GeometryHash` path calls `XxHash128.HashToUInt128(span)` with no seed parameter — the settings-folded `ContentIdentity.seed` governs only the re-tessellation cache identity, never this parity. Row [1] proves digest value, `fmt` threading, LE layout, and the byte-length ledger off one frozen literal pair; row [7] is the corpus's one carrier for the C# `CanonicalWriter` `Double`/`Measure` float canon the integer-topology row cannot reach, grading only once its producer freezes the reference and its rows land.
- Growth: a new parity aspect is one `ParityAspect` member with one `ParityRow` on the owning fixture; a new cross-runtime fixture one `_CORPUS` row; a pending fixture graduates by one `Some(payload)`, its rows, and the `state="real"` flip, zero new method; a sibling-authored corpus (data's icechunk snapshot-seed fixtures) is the `corpus` constructor argument over the same exported row types, never a second suite; a new derivation modality is one `FixturePayload` member only when `IdentitySource` itself grows one.
- Boundary: the corpus is read-only and the C# seed is the single mint — a Python-fabricated byte set for an unpinned row is the one forbidden authorship. Pending rows graduate producer-side through their fence-anchored producers; the harness driver feeding payloads and grading rows is a `python:testing` consumer of this same corpus, never a second fixture store here.

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

type ParityAspect = Literal["value_identity", "hex_identity", "memory_layout", "key_identity"]
# `real` grades NOW; `gated` awaits the one-time harness reproof, `design_pin` the producer's freeze — both ride the `planned` obligation.
type FixtureState = Literal["real", "gated", "design_pin"]
# payload SHAPE keys the `IdentitySource.lift` arm (whole/stream/merkle); a live `Struct` is excluded because the corpus transcribes
# producer-frozen bytes and a local canonical re-encode would be a second mint.
type FixturePayload = bytes | tuple[bytes, ...] | tuple[ContentKey, ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

IDENTITY_FMT: Final[str] = "geometry-topology"
SUITE: Final[str] = "seed-reproduction"
# [1] CANONICAL_BYTE_IDENTITY — frozen on csharp:Rasm/Spatial/reconciliation#ONE_WIRE_FIXTURE_CORPUS[1]: the int32-LE adjacency stream
# of the single-triangle topology (VertexCount=3; edges (0,1),(0,2),(1,2); face cycle [0,1,2]) and its seed-zero XxHash128 digest; the
# LE memory stays a producer-TRANSCRIBED literal — a `to_bytes` derivation would grade a tautology.
CANONICAL_STREAM: Final[bytes] = bytes.fromhex(
    "03000000030000000000000001000000000000000200000001000000020000000100000003000000000000000100000002000000"
)
CANONICAL_DIGEST: Final[int] = 0x9462A71A5DD13DCFA3B1D6D225FCBE70
CANONICAL_LE_MEMORY: Final[bytes] = bytes.fromhex("70befc25d2d6b1a3cf3dd15d1aa76294")
# C# InterchangeIdentity.Key's `{value:032x}:{fmt}` render of the frozen digest — derived, never a second literal.
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
        name="fault-triples", state="design_pin", producer="csharp:Rasm.Compute/Runtime/wire#FAULT_PROJECTION", payload=Nothing, rows=Block.empty()
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
    # [8] cross-backend array-layout bit-identity — graduates when the array-admission owner keys canonical
    # array bytes and freezes the reference; until then the row carries no payload and grades nothing.
    CorpusFixture(name="array-layout", state="design_pin", producer="python:compute/numerics/array#PAYLOAD", payload=Nothing, rows=Block.empty()),
    # [9] the C#-minted graduation-evidence golden bundle — pairs the bundle bytes with the expected
    # generated-stub projection so the corpus proves decode AND emit round-trip byte-stability.
    CorpusFixture(name="evidence-bundle", state="design_pin", producer="csharp:Rasm.Compute/Runtime", payload=Nothing, rows=Block.empty()),
))

# --- [COMPOSITION] ----------------------------------------------------------------------


class SeedReproduction(Struct, frozen=True):
    corpus: Block[CorpusFixture] = _CORPUS

    def grade(self) -> RuntimeRail[Block[ParityReceipt]]:
        rails = self.corpus.choose(
            lambda fixture: fixture.payload.map(
                lambda payload: ContentIdentity.of(fixture.name, payload, seed=Some(0)).map(
                    lambda key: fixture.rows.map(lambda row: row.grade(fixture.name, key))
                )
            )
        )
        return traversed(rails, by=Disposition.ACCUMULATE).map(lambda graded: graded.collect(identity))

    def contribute(self) -> Iterable[Receipt]:
        # both arms are Receipt-typed, satisfying `Result.merge`; each pending fixture mints one `planned` obligation keyed
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

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
