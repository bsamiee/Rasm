# [PY_RUNTIME_REPRODUCTION]

Cross-runtime seed parity binds here, the module `rasm.runtime.reproduction`: `SeedReproduction` asserts `ContentIdentity` reproduces every frozen seed-zero `XxHash128` reference whose `libs/contracts/manifest.json` case names the `python:runtime/evidence/reproduction#SEED_REPRODUCTION` consumer actor. It re-mints no digest and authors no fixture byte — it consumes the production `ContentIdentity.of`/`ContentKey.project` surface, so a derivation or render regression surfaces as a failed parity receipt, never a pass against a parallel path.

One fault-combining fold carries the suite — `reliability/faults#FAULT` `traversed(by=Disposition.ACCUMULATE)` — so a single fixture's fault never masks a later fixture's evidence, and `contribute` satisfies the `observability/receipts#RECEIPT` `ReceiptContributor` port, each pending fixture riding a `planned` obligation keyed to the anchor owing its freeze. This module splits out of `evidence/identity` so its `receipts` import stays DAG-legal (`identity < receipts < reproduction`).

## [01]-[INDEX]

- [02]-[SEED_REPRODUCTION]: `_CORPUS` fixtures grade through the `ParityAspect` vocabulary over one accumulate fold behind the `claimed` fmt census, every `KeyView` member proven.

## [02]-[SEED_REPRODUCTION]

- Owner: `_CORPUS` transcribes the `libs/contracts/manifest.json` cases whose consumers name the `python:runtime/evidence/reproduction#SEED_REPRODUCTION` actor — `name` the entry's seam, `kind` its class, `state` its pin, `source` the actor anchor that freezes the reference: the one producer of a `domain` case, this branch's own minter of an `infrastructure` case every named branch mints from its own inputs. `real` transcribes the frozen reference verbatim; `design_pin` carries `Nothing` and no rows until its source freezes the preimage. `fmt` is a column on EVERY row, pending included — the producing wire tag rather than the seam name, so a fmt drift fails the hex row instead of passing silently, and the census reaches the tags no freeze has handed over yet. `key_identity` grades the whole `ContentKey` by structural equality, so the `fmt` threading and the `byte_length` ledger ride the parity fold — evidence no scalar view reaches.
- Law: `claimed` is the fmt census `grade` binds through, so a corpus whose tags cannot resolve grades no row at all: each declared tag admits against the identity owner's own `KEY_FMT` grammar and resolves to exactly one producing fixture, accumulating so one bad row never hides another. Both halves matter on a mostly-unfrozen corpus — an unlawful tag would otherwise refuse at the derivation its freeze eventually runs, long after the row landed, and two rows claiming one tag would collapse two producers' evidence onto one key while the parity fold read the collision as agreement.
- Auto: `xxhash` returns the digest as a Python `int` whose `to_bytes(16, "little")` IS the C# `UInt128` in-memory layout `BinaryPrimitives.WriteUInt128LittleEndian` writes, so value-equality holds with no byte-swap when both sides read seed zero. Seed zero is the `docs/laws/patterns.md` `[CONTENT_KEY]` law each branch mints its own entry under, never a peer-supplied seed — the settings-folded `ContentIdentity.seed` governs the re-tessellation cache identity alone. `MESH_ADJACENCY_GOLDEN` proves digest value, `fmt` threading, LE layout, and the byte-length ledger off one frozen literal pair; `MATERIAL_LAYER_GOLDEN` is the corpus's one carrier for the `CanonicalWriter` `Double`/`Measure` float canon an integer-topology stream cannot reach, grading once its producer freezes the reference and its rows land.
- Growth: a new parity aspect is one `ParityAspect` member with one `ParityRow` on the owning fixture; a manifest entry newly naming this suite one `_CORPUS` row carrying its producing tag, which the census admits with no second declaration; a pending fixture graduates by one `Some(FrozenReference(...))`, its rows, and the `state="real"` flip, zero new method; a sibling-authored corpus (data's icechunk snapshot-seed fixtures, compute's canonical array bytes) is the `corpus` constructor argument over the same exported row types, never a second suite; a new derivation modality is one `FixturePayload` member only when `IdentitySource` itself grows one.
- Boundary: the reference is read-only — a Python-fabricated byte set for an unfrozen row is the one forbidden authorship, a `domain` row decodes its producer's semantics and never re-derives them, and an `infrastructure` row grades this branch's own mint against the vector every branch reproduces. The grammar is the identity owner's and is read here, never re-spelled — a second pattern beside `KEY_FMT` would let the census admit a tag the derivation refuses. Pending rows graduate at the anchor their `source` names; the harness driver feeding payloads and grading rows is a `libs/contracts/conformance` consumer of this same corpus, never a second fixture store here.

```python signature
from collections.abc import Iterable
from typing import Final, Literal

from expression import Error, Nothing, Ok, Option, Some, identity
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.faults import CORPUS_DOUBLED, CORPUS_FMT, Disposition, RuntimeRail, traversed
from rasm.runtime.identity import KEY_FMT, ContentIdentity, ContentKey, KeyRender, KeyView
from rasm.runtime.receipts import Receipt

# --- [TYPES] ----------------------------------------------------------------------------

type ParityAspect = Literal["value_identity", "hex_identity", "memory_layout", "key_identity"]
# Manifest CLASS discriminates the reference: `domain` transcribes its one producer's frozen bytes, `infrastructure` grades this
# branch's own mint against the vector every branch reproduces from its own inputs.
type FixtureClass = Literal["infrastructure", "domain"]
# Manifest PIN discriminates readiness: `real` grades NOW, `design_pin` rides the `planned` obligation until its source freezes.
type FixtureState = Literal["real", "design_pin"]
# payload SHAPE keys the `IdentitySource.lift` arm (whole/stream/merkle); a live `Struct` is excluded because the corpus transcribes
# producer-frozen bytes and a local canonical re-encode would be a second mint.
type FixturePayload = bytes | tuple[bytes, ...] | tuple[ContentKey, ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

MESH_FMT: Final[str] = "geometry-topology"
SUITE: Final[str] = "seed-reproduction"
# MESH_ADJACENCY_GOLDEN as its producer froze it: the int32-LE adjacency stream of the single-triangle topology (VertexCount=3;
# edges (0,1),(0,2),(1,2); face cycle [0,1,2]) and its seed-zero XxHash128 digest; the LE memory stays a producer-TRANSCRIBED
# literal — a `to_bytes` derivation would grade a tautology.
MESH_STREAM: Final[bytes] = bytes.fromhex(
    "03000000030000000000000001000000000000000200000001000000020000000100000003000000000000000100000002000000"
)
MESH_DIGEST: Final[int] = 0x9462A71A5DD13DCFA3B1D6D225FCBE70
MESH_LE_MEMORY: Final[bytes] = bytes.fromhex("70befc25d2d6b1a3cf3dd15d1aa76294")
# C# InterchangeIdentity.Key's `{value:032x}:{fmt}` render of the frozen digest — derived, never a second literal.
MESH_HEX: Final[str] = f"{MESH_DIGEST:032x}:{MESH_FMT}"

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


class FrozenReference(Struct, frozen=True, gc=False):
    # the producer-frozen bytes alone: `fmt` moved up to the fixture, because a PENDING row has no reference yet and
    # its producing wire tag is exactly what the census must see before the freeze lands.
    payload: FixturePayload


class CorpusFixture(Struct, frozen=True):
    # `fmt` is a first-class column on EVERY row, pending included, so the census reads the producing wire tag a
    # fixture will key under rather than only the tags a freeze already handed over — the reach a `FrozenReference`-only
    # spelling could never have, since it proves nothing about the rows that have not frozen. One declaration serves
    # both readers: `_claimed` admits it against the identity grammar and `grade` derives the key under it.
    name: str
    kind: FixtureClass
    state: FixtureState
    source: str
    fmt: str
    reference: Option[FrozenReference]
    rows: Block[ParityRow]


# --- [TABLES] ---------------------------------------------------------------------------

_CORPUS: Final[Block[CorpusFixture]] = Block.of_seq((
    # `content-identity` — the framing and seed law itself, minted here from this branch's own canonical writer.
    # The MINTER now exists: `IdentitySource.parts` folds the count frame plus a per-field little-endian u64 length,
    # so a payload-agnostic framed preimage is derivable rather than hypothetical. What still withholds the freeze is
    # the CROSS-BRANCH half an infrastructure row means — every branch reproduces this vector from its own inputs, so
    # the reference freezes when the peer writers agree on the same frame widths, never when one branch mints first.
    CorpusFixture(
        name="content-identity",
        kind="infrastructure",
        state="design_pin",
        source="python:runtime/evidence/identity#IDENTITY",
        fmt="content-identity",
        reference=Nothing,
        rows=Block.empty(),
    ),
    # MESH_ADJACENCY_GOLDEN — mesh adjacency is the producer's domain capability, so this row decodes the frozen stream and
    # never re-derives the topology.
    CorpusFixture(
        name="mesh-adjacency",
        kind="domain",
        state="real",
        source="dotnet:Rasm/Spatial/reconciliation#RECONCILIATION_BRIDGE",
        fmt=MESH_FMT,
        reference=Some(FrozenReference(payload=MESH_STREAM)),
        rows=Block.of_seq((
            ParityRow(aspect="value_identity", view="digest", expected=MESH_DIGEST),
            ParityRow(aspect="hex_identity", view="hex", expected=MESH_HEX),
            ParityRow(aspect="memory_layout", view="memory", expected=MESH_LE_MEMORY),
            ParityRow(
                aspect="key_identity",
                view="value",
                expected=ContentKey(value=MESH_DIGEST, fmt=MESH_FMT, byte_length=Some(len(MESH_STREAM))),
            ),
        )),
    ),
    # MATERIAL_LAYER_GOLDEN — the float-bearing IfcMaterialLayer-shaped LayerSet node whose CanonicalWriter IEEE-754-LE bytes
    # exercise the Double/Measure canon an integer-topology stream cannot reach.
    CorpusFixture(
        name="material-layer",
        kind="domain",
        state="design_pin",
        source="dotnet:Rasm.Element/Projection/address#CONTENT_ADDRESS",
        fmt="material-layer",
        reference=Nothing,
        rows=Block.empty(),
    ),
    # ELEMENT_CORPUS — the four graded `S`/`M`/`L`/`XL` snapshot addresses of the seeded `GraphForge` models; graduation
    # waits on the first sanctioned execution of the settled forge (the manifest `element-graph/corpus` blocker carries the arming), so
    # the row holds the producing tag and no bytes.
    CorpusFixture(
        name="element-corpus",
        kind="domain",
        state="design_pin",
        source="dotnet:Rasm.Element/Graph/corpus#CORPUS_ROSTER",
        fmt="element-graph",
        reference=Nothing,
        rows=Block.empty(),
    ),
    # FAULT_DETAIL — the compact numeric envelope produced by C# and decoded here without source-union rehydration.
    CorpusFixture(
        name="fault-detail",
        kind="domain",
        state="design_pin",
        source="dotnet:Rasm.Compute/Runtime/wire#FAULT_PROJECTION",
        fmt="fault-detail",
        reference=Nothing,
        rows=Block.empty(),
    ),
    # CRDT_OP_SET — the op multiset whose divergent-delivery folds converge byte-identically; it graduates through the MERKLE
    # arm over the LE child-transcription spine. The anchor is the CODEC that mints and drains those bytes, not the state
    # fold that consumes them: the round-trip claim freezes on the encode/decode pair, so the state cluster owes nothing.
    CorpusFixture(
        name="crdt-op-set",
        kind="infrastructure",
        state="design_pin",
        source="python:runtime/transport/wire#CRDT_CODEC",
        fmt="crdt-op",
        reference=Nothing,
        rows=Block.empty(),
    ),
    # HLC_TWO_HALF — the two-64-bit-half stamps whose half order an off-by-one-half would corrupt.
    CorpusFixture(
        name="hlc-two-half",
        kind="infrastructure",
        state="design_pin",
        source="python:runtime/evidence/clock#CLOCK",
        fmt="hlc-stamp",
        reference=Nothing,
        rows=Block.empty(),
    ),
    # keyed-artifact/glb — one content-keyed GLB sample graduating with digest, memory, and hex rows, the hex row
    # proving the `InterchangeIdentity.Key` render; `geometry/mesh/daemon` is the branch consumer that graduates
    # the graded sample. The producer is the manifest's own: Bim's export rail, keyed by Compute's partition.
    CorpusFixture(
        name="glb-by-key",
        kind="domain",
        state="design_pin",
        source="dotnet:Rasm.Bim/Exchange/export#EXPORT_RAIL",
        fmt="glb",
        reference=Nothing,
        rows=Block.empty(),
    ),
))

# --- [COMPOSITION] ----------------------------------------------------------------------


class SeedReproduction(Struct, frozen=True):
    corpus: Block[CorpusFixture] = _CORPUS

    def claimed(self) -> RuntimeRail[Map[str, str]]:
        # the fmt census, accumulating so one bad row never hides another: every declared tag admits against the
        # identity owner's own `KEY_FMT` grammar and resolves to exactly ONE producing fixture. Both halves are
        # load-bearing on a corpus whose rows mostly have not frozen — an unlawful tag would refuse at the derivation
        # the freeze eventually runs, long after the row landed, and two rows claiming one tag would collapse two
        # producers' evidence onto one key with the parity fold reading the collision as agreement. Reading the
        # DECLARED column rather than a frozen reference is what gives the census its reach: it grades the tags no
        # freeze has handed over yet, which is every pending row.
        rails = self.corpus.map(
            lambda fixture: Ok((fixture.fmt, fixture.source))
            if KEY_FMT.fullmatch(fixture.fmt) is not None
            else Error(CORPUS_FMT.raised(fixture.name, fixture.fmt, KEY_FMT.pattern))
        )
        # the census builds ONCE and the walrus binds it inside the guard the conditional evaluates first, so the
        # collision test and the returned map are the same value — three constructions of one tree would let a future
        # edit change the returned census without moving the test that admitted it.
        return traversed(rails, by=Disposition.ACCUMULATE).bind(
            lambda pairs: Ok(census)
            if len(census := Map.of_seq(pairs)) == len(pairs)
            else Error(CORPUS_DOUBLED.raised())
        )

    def grade(self) -> RuntimeRail[Block[ParityReceipt]]:
        # the census gates the fold, so a corpus whose tags cannot resolve never grades a single row: a graded pass
        # over an unlawful or doubly-claimed namespace reports agreement about a key nothing can join on.
        rails = self.corpus.choose(
            lambda fixture: fixture.reference.map(
                lambda frozen: ContentIdentity.of(fixture.fmt, frozen.payload, seed=Some(0)).map(
                    lambda key: fixture.rows.map(lambda row: row.grade(fixture.name, key))
                )
            )
        )
        return self.claimed().bind(lambda _tags: traversed(rails, by=Disposition.ACCUMULATE).map(lambda graded: graded.collect(identity)))

    def contribute(self) -> Iterable[Receipt]:
        # both arms are Receipt-typed, satisfying `Result.merge`; each pending fixture mints one `planned` obligation keyed
        # `{kind: source}`, so an ungraded parity names the anchor owing the freeze rather than leaving a silent gap.
        graded = (
            self.grade()
            .map(lambda rows: Receipt.of(SUITE, ("emitted", SUITE, dict(rows.map(lambda receipt: receipt.fact)))))
            .map_error(lambda fault: Receipt.of(SUITE, fault))
            .merge()
        )
        pending = self.corpus.choose(
            lambda fixture: (
                Some(Receipt.of(fixture.name, ("planned", fixture.name, {fixture.kind: fixture.source}))) if fixture.reference.is_none() else Nothing
            )
        )
        return (graded, *pending)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
