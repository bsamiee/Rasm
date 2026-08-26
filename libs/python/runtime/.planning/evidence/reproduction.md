# [PY_RUNTIME_REPRODUCTION]

One fault-combining fold carries the suite — `reliability/faults#FAULT` `traversed(by=Disposition.ACCUMULATE)` — so a single fixture's fault never masks a later fixture's evidence, and a pending fixture stays readable on the corpus as a row whose `carried` is `Nothing`, keyed to the anchor owing its payload. This module splits out of `evidence/identity` so the corpus fixtures never load into the identity mint path (`identity < reproduction`).

## [01]-[INDEX]

- [02]-[SEED_REPRODUCTION]: `_CORPUS` fixtures grade through the `ParityAspect` relations over one accumulate fold behind the `claimed` fmt census.

## [02]-[SEED_REPRODUCTION]

- Law: `claimed` is the fmt census `grade` binds through, so a corpus whose tags cannot resolve grades no row at all: each declared tag admits against the identity owner's own `KEY_FMT` grammar and resolves to exactly one producing fixture, accumulating so one bad row never hides another. Both halves matter while most rows carry no payload: an unlawful tag otherwise sits unrefused until its payload lands and the derivation runs, long after the row itself did, and two rows claiming one tag collapse two producers' evidence onto one key while the parity fold reads the collision as agreement.
- Auto: `xxhash` returns the digest as a Python `int` whose `to_bytes(16, "little")` IS the C# `UInt128` in-memory layout `BinaryPrimitives.WriteUInt128LittleEndian` writes, so value-equality holds with no byte-swap when both sides read seed zero. Seed zero is never a peer-supplied seed — the settings-folded `ContentIdentity.seed` governs the re-tessellation cache identity alone. `reference_digest` hashes the re-derived preimage through `xxhash` directly and `byte_ledger` sums the payload's own members, so both answer to the payload rather than to the fold that read it.
- Growth: a new parity aspect is one `ParityAspect` member with one `Parity.judged` line in `_related`, reaching every carrying fixture at once; a manifest entry newly naming this suite one `_CORPUS` row carrying its producing tag, which the census admits with no second declaration; a pending fixture graduates by one `Some(payload)` and the `state="real"` flip, zero new row and zero new method; a sibling-authored corpus (data's icechunk snapshot-seed fixtures, compute's canonical array bytes) is the `corpus` constructor argument over the same exported row types, never a second suite; a new derivation modality is one `FixturePayload` member only when `IdentitySource` itself grows one.

```python
from typing import Final, Literal, assert_never

import xxhash
from expression import Error, Nothing, Ok, Option, Some, identity
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.faults import CORPUS_DOUBLED, CORPUS_FMT, Disposition, RuntimeResult, traversed
from rasm.runtime.identity import KEY_FMT, ContentIdentity, ContentKey, KeyRender

# --- [TYPES] ----------------------------------------------------------------------------

type ParityAspect = Literal["reference_digest", "byte_ledger"]
type FixtureClass = Literal["infrastructure", "domain"]
type FixtureState = Literal["real", "design_pin"]
type FixturePayload = bytes | tuple[bytes, ...] | tuple[ContentKey, ...]
type ParityRender = KeyRender | Option[int]

# --- [CONSTANTS] ------------------------------------------------------------------------

SEED: Final[int] = 0
MESH_FMT: Final[str] = "geometry-topology"
MESH_STREAM: Final[bytes] = bytes.fromhex(
    "03000000030000000000000001000000000000000200000001000000020000000100000003000000000000000100000002000000"
)

# --- [MODELS] ---------------------------------------------------------------------------


class Parity(Struct, frozen=True, gc=False):
    fixture: str
    aspect: ParityAspect
    expected: ParityRender
    observed: ParityRender
    verified: bool

    @staticmethod
    def judged(fixture: str, aspect: ParityAspect, expected: ParityRender, observed: ParityRender) -> "Parity":
        return Parity(fixture=fixture, aspect=aspect, expected=expected, observed=observed, verified=observed == expected)

    @property
    def fact(self) -> tuple[str, str]:
        return f"{self.fixture}.{self.aspect}", "ok" if self.verified else f"{self.observed!r}!={self.expected!r}"


class CorpusFixture(Struct, frozen=True):
    name: str
    kind: FixtureClass
    state: FixtureState
    source: str
    fmt: str
    carried: Option[FixturePayload]


# --- [TABLES] ---------------------------------------------------------------------------

_CORPUS: Final[Block[CorpusFixture]] = Block.of_seq((
    CorpusFixture(
        name="content-identity",
        kind="infrastructure",
        state="design_pin",
        source="python:runtime/evidence/identity#IDENTITY",
        fmt="content-identity",
        carried=Nothing,
    ),
    CorpusFixture(
        name="mesh-adjacency",
        kind="domain",
        state="real",
        source="dotnet:Rasm/Spatial/reconciliation#RECONCILIATION_BRIDGE",
        fmt=MESH_FMT,
        carried=Some(MESH_STREAM),
    ),
    CorpusFixture(
        name="material-layer",
        kind="domain",
        state="design_pin",
        source="dotnet:Rasm.Element/Projection/address#CONTENT_ADDRESS",
        fmt="material-layer",
        carried=Nothing,
    ),
    CorpusFixture(
        name="element-corpus",
        kind="domain",
        state="design_pin",
        source="dotnet:Rasm.Element/Graph/corpus#CORPUS_ROSTER",
        fmt="element-graph",
        carried=Nothing,
    ),
    CorpusFixture(
        name="fault-detail",
        kind="domain",
        state="design_pin",
        source="dotnet:Rasm.Compute/Runtime/wire#FAULT_PROJECTION",
        fmt="fault-detail",
        carried=Nothing,
    ),
    CorpusFixture(
        name="crdt-op-set",
        kind="infrastructure",
        state="design_pin",
        source="python:runtime/transport/wire#CRDT_CODEC",
        fmt="crdt-op",
        carried=Nothing,
    ),
    CorpusFixture(
        name="hlc-two-half",
        kind="infrastructure",
        state="design_pin",
        source="python:runtime/evidence/clock#CLOCK",
        fmt="hlc-stamp",
        carried=Nothing,
    ),
    CorpusFixture(
        name="glb-by-key",
        kind="domain",
        state="design_pin",
        source="dotnet:Rasm.Bim/Exchange/export#EXPORT_PIPELINE",
        fmt="glb",
        carried=Nothing,
    ),
))

# --- [OPERATIONS] -----------------------------------------------------------------------


def _sized(member: ContentKey | bytes) -> Option[int]:
    return member.byte_length if isinstance(member, ContentKey) else Some(len(member))


def _preimage(payload: FixturePayload) -> bytes:
    match payload:
        case bytes() as whole:
            return whole
        case tuple() as members:
            return b"".join(member.memory if isinstance(member, ContentKey) else member for member in members)
        case _ as unreachable:
            assert_never(unreachable)


def _extent(payload: FixturePayload) -> Option[int]:
    match payload:
        case bytes() as whole:
            return Some(len(whole))
        case tuple() as members:
            return Block.of_seq(members).fold(
                lambda held, member: held.bind(lambda total: _sized(member).map(lambda size: total + size)), Some(0)
            )
        case _ as unreachable:
            assert_never(unreachable)


def _related(fixture: CorpusFixture, payload: FixturePayload) -> RuntimeResult[Block[Parity]]:
    def judged(key: ContentKey) -> Block[Parity]:
        return Block.of_seq((
            Parity.judged(
                fixture.name, "reference_digest", xxhash.xxh3_128_intdigest(_preimage(payload), seed=SEED), key.project("digest")
            ),
            Parity.judged(fixture.name, "byte_ledger", _extent(payload), key.byte_length),
        ))

    return ContentIdentity.of(fixture.fmt, payload, seed=Some(SEED)).map(judged)


# --- [COMPOSITION] ----------------------------------------------------------------------


class SeedReproduction(Struct, frozen=True):
    corpus: Block[CorpusFixture] = _CORPUS

    def claimed(self) -> RuntimeResult[Map[str, str]]:
        results = self.corpus.map(
            lambda fixture: Ok((fixture.fmt, fixture.source))
            if KEY_FMT.fullmatch(fixture.fmt) is not None
            else Error(CORPUS_FMT.raised(fixture.name, fixture.fmt, KEY_FMT.pattern))
        )
        return traversed(results, by=Disposition.ACCUMULATE).bind(
            lambda pairs: Ok(census)
            if len(census := Map.of_seq(pairs)) == len(pairs)
            else Error(CORPUS_DOUBLED.raised())
        )

    def grade(self) -> RuntimeResult[Block[Parity]]:
        results = self.corpus.choose(lambda fixture: fixture.carried.map(lambda payload: _related(fixture, payload)))
        return self.claimed().bind(lambda _tags: traversed(results, by=Disposition.ACCUMULATE).map(lambda graded: graded.collect(identity)))

    def pending(self) -> Block[CorpusFixture]:
        return self.corpus.filter(lambda fixture: fixture.carried.is_none())
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
