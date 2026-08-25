# [PY_RUNTIME_REPRODUCTION]

Cross-runtime seed parity binds here, the module `rasm.runtime.reproduction`: `SeedReproduction` asserts `ContentIdentity` reproduces every frozen seed-zero `XxHash128` reference whose `libs/contracts/manifest.json` case names the `python:runtime/evidence/reproduction#SEED_REPRODUCTION` consumer actor. It re-mints no digest and authors no fixture byte — it consumes the production `ContentIdentity.of`/`ContentKey.project` surface, so a derivation or render regression surfaces as a failed `Parity` row, never a pass against a parallel path.

One fault-combining fold carries the suite — `reliability/faults#FAULT` `traversed(by=Disposition.ACCUMULATE)` — so a single fixture's fault never masks a later fixture's evidence, and a pending fixture stays readable on the corpus as a row whose `reference` is `Nothing`, keyed to the anchor owing its freeze. This module splits out of `evidence/identity` so the corpus fixtures never load into the identity mint path (`identity < reproduction`).

## [01]-[INDEX]

- [02]-[SEED_REPRODUCTION]: `_CORPUS` fixtures grade through the `ParityAspect` vocabulary over one accumulate fold behind the `claimed` fmt census, every `KeyView` member proven.

## [02]-[SEED_REPRODUCTION]

- Owner: `_CORPUS` transcribes the `libs/contracts/manifest.json` cases whose consumers name the `python:runtime/evidence/reproduction#SEED_REPRODUCTION` actor — `name` the entry's seam, `kind` its class, `state` its pin, `source` the actor anchor that freezes the reference: the one producer of a `domain` case, this branch's own minter of an `infrastructure` case every named branch mints from its own inputs. `real` transcribes the frozen reference verbatim; `design_pin` carries `Nothing` and no rows until its source freezes the preimage. `fmt` is a column on EVERY row, pending included — the producing wire tag rather than the seam name, so a fmt drift fails the hex row instead of passing silently, and the census reaches the tags no freeze has handed over yet. `key_identity` grades the whole `ContentKey` by structural equality, so the `fmt` threading and the `byte_length` ledger ride the parity fold — evidence no scalar view reaches.
- Law: `claimed` is the fmt census `grade` binds through, so a corpus whose tags cannot resolve grades no row at all: each declared tag admits against the identity owner's own `KEY_FMT` grammar and resolves to exactly one producing fixture, accumulating so one bad row never hides another. Both halves matter on a mostly-unfrozen corpus — an unlawful tag would otherwise refuse at the derivation its freeze eventually runs, long after the row landed, and two rows claiming one tag would collapse two producers' evidence onto one key while the parity fold read the collision as agreement.
- Auto: `xxhash` returns the digest as a Python `int` whose `to_bytes(16, "little")` IS the C# `UInt128` in-memory layout `BinaryPrimitives.WriteUInt128LittleEndian` writes, so value-equality holds with no byte-swap when both sides read seed zero. Seed zero is never a peer-supplied seed — the settings-folded `ContentIdentity.seed` governs the re-tessellation cache identity alone. `MESH_ADJACENCY_GOLDEN` proves digest value, `fmt` threading, LE layout, and the byte-length ledger off one frozen literal pair; `MATERIAL_LAYER_GOLDEN` is the corpus's one carrier for the `CanonicalWriter` `Double`/`Measure` float canon an integer-topology stream cannot reach, grading once its producer freezes the reference and its rows land.
- Growth: a new parity aspect is one `ParityAspect` member with one `ParityRow` on the owning fixture; a manifest entry newly naming this suite one `_CORPUS` row carrying its producing tag, which the census admits with no second declaration; a pending fixture graduates by one `Some(FrozenReference(...))`, its rows, and the `state="real"` flip, zero new method; a sibling-authored corpus (data's icechunk snapshot-seed fixtures, compute's canonical array bytes) is the `corpus` constructor argument over the same exported row types, never a second suite; a new derivation modality is one `FixturePayload` member only when `IdentitySource` itself grows one.
- Boundary: the reference is read-only — a Python-fabricated byte set for an unfrozen row is the one forbidden authorship, a `domain` row decodes its producer's semantics and never re-derives them, and an `infrastructure` row grades this branch's own mint against the vector every branch reproduces. The grammar is the identity owner's and is read here, never re-spelled — a second pattern beside `KEY_FMT` would let the census admit a tag the derivation refuses. Pending rows graduate at the anchor their `source` names; the harness driver feeding payloads and grading rows is a `libs/contracts/conformance` consumer of this same corpus, never a second fixture store here.

```python
from typing import Final, Literal

from expression import Error, Nothing, Ok, Option, Some, identity
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.faults import CORPUS_DOUBLED, CORPUS_FMT, Disposition, RuntimeRail, traversed
from rasm.runtime.identity import KEY_FMT, ContentIdentity, ContentKey, KeyRender, KeyView

# --- [TYPES] ----------------------------------------------------------------------------

type ParityAspect = Literal["value_identity", "hex_identity", "memory_layout", "key_identity"]
type FixtureClass = Literal["infrastructure", "domain"]
type FixtureState = Literal["real", "design_pin"]
type FixturePayload = bytes | tuple[bytes, ...] | tuple[ContentKey, ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

MESH_FMT: Final[str] = "geometry-topology"
MESH_STREAM: Final[bytes] = bytes.fromhex(
    "03000000030000000000000001000000000000000200000001000000020000000100000003000000000000000100000002000000"
)
MESH_DIGEST: Final[int] = 0x9462A71A5DD13DCFA3B1D6D225FCBE70
MESH_LE_MEMORY: Final[bytes] = bytes.fromhex("70befc25d2d6b1a3cf3dd15d1aa76294")
MESH_HEX: Final[str] = f"{MESH_DIGEST:032x}:{MESH_FMT}"

# --- [MODELS] ---------------------------------------------------------------------------


class Parity(Struct, frozen=True, gc=False):
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

    def grade(self, fixture: str, key: ContentKey) -> Parity:
        observed = key.project(self.view)
        return Parity(fixture=fixture, aspect=self.aspect, expected=self.expected, observed=observed, verified=observed == self.expected)


class FrozenReference(Struct, frozen=True, gc=False):
    payload: FixturePayload


class CorpusFixture(Struct, frozen=True):
    name: str
    kind: FixtureClass
    state: FixtureState
    source: str
    fmt: str
    reference: Option[FrozenReference]
    rows: Block[ParityRow]


# --- [TABLES] ---------------------------------------------------------------------------

_CORPUS: Final[Block[CorpusFixture]] = Block.of_seq((
    CorpusFixture(
        name="content-identity",
        kind="infrastructure",
        state="design_pin",
        source="python:runtime/evidence/identity#IDENTITY",
        fmt="content-identity",
        reference=Nothing,
        rows=Block.empty(),
    ),
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
    CorpusFixture(
        name="material-layer",
        kind="domain",
        state="design_pin",
        source="dotnet:Rasm.Element/Projection/address#CONTENT_ADDRESS",
        fmt="material-layer",
        reference=Nothing,
        rows=Block.empty(),
    ),
    CorpusFixture(
        name="element-corpus",
        kind="domain",
        state="design_pin",
        source="dotnet:Rasm.Element/Graph/corpus#CORPUS_ROSTER",
        fmt="element-graph",
        reference=Nothing,
        rows=Block.empty(),
    ),
    CorpusFixture(
        name="fault-detail",
        kind="domain",
        state="design_pin",
        source="dotnet:Rasm.Compute/Runtime/wire#FAULT_PROJECTION",
        fmt="fault-detail",
        reference=Nothing,
        rows=Block.empty(),
    ),
    CorpusFixture(
        name="crdt-op-set",
        kind="infrastructure",
        state="design_pin",
        source="python:runtime/transport/wire#CRDT_CODEC",
        fmt="crdt-op",
        reference=Nothing,
        rows=Block.empty(),
    ),
    CorpusFixture(
        name="hlc-two-half",
        kind="infrastructure",
        state="design_pin",
        source="python:runtime/evidence/clock#CLOCK",
        fmt="hlc-stamp",
        reference=Nothing,
        rows=Block.empty(),
    ),
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
        rails = self.corpus.map(
            lambda fixture: Ok((fixture.fmt, fixture.source))
            if KEY_FMT.fullmatch(fixture.fmt) is not None
            else Error(CORPUS_FMT.raised(fixture.name, fixture.fmt, KEY_FMT.pattern))
        )
        return traversed(rails, by=Disposition.ACCUMULATE).bind(
            lambda pairs: Ok(census)
            if len(census := Map.of_seq(pairs)) == len(pairs)
            else Error(CORPUS_DOUBLED.raised())
        )

    def grade(self) -> RuntimeRail[Block[Parity]]:
        rails = self.corpus.choose(
            lambda fixture: fixture.reference.map(
                lambda frozen: ContentIdentity.of(fixture.fmt, frozen.payload, seed=Some(0)).map(
                    lambda key: fixture.rows.map(lambda row: row.grade(fixture.name, key))
                )
            )
        )
        return self.claimed().bind(lambda _tags: traversed(rails, by=Disposition.ACCUMULATE).map(lambda graded: graded.collect(identity)))

    def pending(self) -> Block[CorpusFixture]:
        return self.corpus.filter(lambda fixture: fixture.reference.is_none())
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
