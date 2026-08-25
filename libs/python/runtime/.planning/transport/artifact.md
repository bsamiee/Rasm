# [PY_RUNTIME_ARTIFACT]

`transport/artifact` owns the verified artifact lifecycle every branch consumer composes: the frame and extent law read off the generated `buf.validate` descriptors, single-use helper-owned spool custody, SHA-256 identity proof on every seal and every re-emission, direction-unique envelope streams over one frame state machine, descriptor-driven reference traversal, and the `ArtifactTransfer` dial that stages, publishes, fetches, and confirms. Consumers hold a proved `OwnedArtifact` or a railed `ArtifactRefusal` and re-prove no octet.

Wire vocabulary imports from the one `rasm.contracts` root: the artifact family from `rasm.contracts.rasm.contracts.artifact.artifact_pb` and the rule extension from `rasm.contracts.buf.validate.validate_pb`; the release probe is `transport/body`'s `AsyncClosable`, imported as `from rasm.runtime.transport.body import AsyncClosable`. Every refusal rides the `expression` `Result` rail until a generated stream demands a raise, where `ArtifactError` reconstructs it whole; `transport/shapes#VOCABULARY` lifts that carrier at the client edge, and no consumer authors a parallel integrity machine.

## [01]-[INDEX]

- [02]-[LAW]: claim and source shapes, the descriptor-read `ArtifactLaw`, custody and envelope value models, and the closed `ArtifactRefusal` family.
- [03]-[PROOF]: two-axis `confirm`, thread-bounded spool digesting, claim holding, and the iterative `references` traversal.
- [04]-[CUSTODY]: `output` as the one spool owner, `ArtifactSink` single-use sealing, copy under the claimed ceiling, `_FrameLaw`, `stage`, `receive`.
- [05]-[FRAMING]: `ArtifactStream` re-proving before the first envelope, the three envelope entry functions, and the unwrap inverse.
- [06]-[TRANSFER]: `ArtifactTransfer` over the generated client with the enclosing-deadline budget projection, and the module exports.

## [02]-[LAW]

- Owner: `ArtifactLaw` is the one bound set — identity width, frame and extent floors and ceilings — `_declared` reads at import and seats as `_LAW`.
- Law: every bound READS the corpus `buf.validate` rule off `ArtifactRef.desc()` and `ArtifactFrame.desc()`; no literal restates one.
- Law: a missing rule or a rule on the sibling oneof arm raises `LookupError` at import, so a dropped rule fails the boot, never a transfer.
- Law: `extent_floor` is one above the declared `gt` bound and `extent_ceiling` the `lte` bound, read off the arm the corpus spells by name.
- Law: the proof pins each read bound to the value the corpus `artifact.proto` declares, so a rule edit lands with its proof row.
- Cases: `ArtifactClaim` — a full `ArtifactRef` confirms both axes, a bare digest confirms identity alone, and `None` accepts the minted reference.
- Cases: `ArtifactSource` admits an in-memory body, a caller-owned path, or an async chunk stream; `ArtifactCustody` is the open-or-sealed seal.
- Cases: `_ArtifactEnvelope[E]` is one direction's wrap and unwrap pair; `_FRAMES`, `_FETCH`, `_PUT` are its rows over one frame state machine.
- Law: `ArtifactRefusal` is the closed family and `rendered` is total by `assert_never`, each case rendering a distinct token beside its own evidence.
- Law: `ArtifactError(refusal)` carries `str` equal to the rendered token and `.refusal` the same value, so a raise loses nothing the rail carried.
- Law: the proof censuses `get_args(ArtifactRefusal.__value__)` against the rendered cases, so a new case lands with its arm and its proof row.
- Entry: `_ArtifactClient` is the structural client shape `ArtifactTransfer` binds — `fetch(request, *, timeout_ms)`, `put(request, *, timeout_ms)`.
- Packages: `rasm.contracts` (`artifact_pb`, `validate_pb`), `protobuf-py` (`DescMessage`), stdlib `dataclasses`.
- Growth: a new bound is one `ArtifactLaw` field and one `_declared` read; a new refusal is one dataclass, one union arm, and one `rendered` arm.
- Boundary: this cluster reads descriptors and mints values; no octet, path, or socket is touched here.

```python signature
"""Descriptor-ruled artifact custody, framing, identity proof, and reference traversal."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import AsyncIterator, Callable
from dataclasses import dataclass
from pathlib import Path
from typing import assert_never, Final, final, Protocol

from protobuf import DescMessage

from rasm.contracts.buf.validate.validate_pb import BytesRules, ext_field, FieldRules, UInt64Rules
from rasm.contracts.rasm.contracts.artifact.artifact_pb import ArtifactFrame, ArtifactRef, FetchRequest, FetchResponse, PutRequest, PutResponse


# --- [TYPES] ----------------------------------------------------------------------------


type ArtifactClaim = ArtifactRef | bytes


type ArtifactSource = bytes | Path | AsyncIterator[bytes]


class _ArtifactClient(Protocol):
    def fetch(self, request: FetchRequest, *, timeout_ms: int | None = None) -> AsyncIterator[FetchResponse]:
        """Fetch one artifact stream."""

    async def put(self, request: AsyncIterator[PutRequest], *, timeout_ms: int | None = None) -> PutResponse:
        """Publish one artifact stream."""


# --- [CONSTANTS] ------------------------------------------------------------------------

_SPOOL_PREFIX: Final = "rasm-artifact-"
_SPOOL_STEM: Final = "artifact"
_SPOOL_THREAD_CEILING: Final = 8


# --- [POLICIES] -------------------------------------------------------------------------



def _rules(message: DescMessage, field: str, /) -> FieldRules:
    """Read the `buf.validate` rules the corpus declares on one generated field.

    Returns:
        The declared field rules.

    Raises:
        LookupError: The corpus declares no rules on that field.
    """
    declared = next((row for row in message.fields if row.name == field), None)
    options = None if declared is None else declared.proto.options
    if options is None or ext_field not in options:
        raise LookupError(f"{message.type_name}.{field} declares no buf.validate field rules")
    return options[ext_field]


def _bytes_rules(message: DescMessage, field: str, /) -> BytesRules:
    """Narrow one field's declared rules to its bytes arm.

    Returns:
        The declared bytes rules.

    Raises:
        LookupError: The field carries no bytes rule arm.
    """
    carrier = _rules(message, field).type
    if carrier is None or carrier.field != "bytes":
        raise LookupError(f"{message.type_name}.{field} declares no buf.validate bytes rules")
    return carrier.value


def _uint64_rules(message: DescMessage, field: str, /) -> UInt64Rules:
    """Narrow one field's declared rules to its uint64 arm.

    Returns:
        The declared uint64 rules.

    Raises:
        LookupError: The field carries no uint64 rule arm.
    """
    carrier = _rules(message, field).type
    if carrier is None or carrier.field != "uint64":
        raise LookupError(f"{message.type_name}.{field} declares no buf.validate uint64 rules")
    return carrier.value


def _uint64_bound(rules: UInt64Rules, arm: str, /) -> int:
    """Read one exclusive or inclusive uint64 bound off its declared oneof arm.

    Returns:
        The declared bound.

    Raises:
        LookupError: The declared bound is absent or spelled on the sibling arm.
    """
    carrier = rules.less_than if arm in {"lt", "lte"} else rules.greater_than
    if carrier is None or carrier.field != arm:
        raise LookupError(f"buf.validate uint64 rules declare no {arm} bound")
    return carrier.value


@final
@dataclass(frozen=True, slots=True, kw_only=True)
class ArtifactLaw:
    """The frame and extent bounds the corpus declares on the artifact family."""

    identity_bytes: int
    frame_floor: int
    frame_ceiling: int
    extent_floor: int
    extent_ceiling: int


def _declared() -> ArtifactLaw:
    """Read the artifact family's whole bound set off its generated descriptors.

    Returns:
        The corpus-declared artifact law.
    """
    reference = ArtifactRef.desc()
    frame = ArtifactFrame.desc()
    payload = _bytes_rules(frame, "payload")
    extent = _uint64_rules(reference, "artifact_bytes")
    return ArtifactLaw(
        identity_bytes=_bytes_rules(reference, "sha256").len,
        frame_floor=payload.min_len,
        frame_ceiling=payload.max_len,
        extent_floor=_uint64_bound(extent, "gt") + 1,
        extent_ceiling=_uint64_bound(extent, "lte"),
    )


_LAW: Final = _declared()


# --- [MODELS] ---------------------------------------------------------------------------


@final
@dataclass(frozen=True, slots=True, kw_only=True)
class OwnedArtifact:
    """A verified artifact whose path is owned by its active helper context."""

    artifact: ArtifactRef
    path: Path


@final
@dataclass(frozen=True, slots=True, kw_only=True)
class ArtifactOpen:
    """A spool holding its one unclaimed seal."""

    path: Path


@final
@dataclass(frozen=True, slots=True, kw_only=True)
class ArtifactSealed:
    """A spool whose seal already proved its own octets."""

    artifact: OwnedArtifact


type ArtifactCustody = ArtifactOpen | ArtifactSealed


@final
@dataclass(frozen=True, slots=True, kw_only=True)
class _ArtifactEnvelope[E]:
    """One RPC direction's envelope correspondence over the shared artifact frame."""

    wrap: Callable[[ArtifactFrame], E]
    unwrap: Callable[[E], ArtifactFrame | None]


_FRAMES: Final = _ArtifactEnvelope[ArtifactFrame](wrap=lambda frame: frame, unwrap=lambda envelope: envelope)
_FETCH: Final = _ArtifactEnvelope[FetchResponse](wrap=lambda frame: FetchResponse(frame=frame), unwrap=lambda envelope: envelope.frame)
_PUT: Final = _ArtifactEnvelope[PutRequest](wrap=lambda frame: PutRequest(frame=frame), unwrap=lambda envelope: envelope.frame)


# --- [ERRORS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class _Divergence[T]:
    """One value custody expected beside the value it observed."""

    expected: T
    actual: T


@final
@dataclass(frozen=True, slots=True, kw_only=True)
class ArtifactEmpty:
    """No payload octet reached custody."""


@final
@dataclass(frozen=True, slots=True, kw_only=True)
class ArtifactExtent(_Divergence[int]):
    """The accumulated octet count diverges from the declared artifact extent."""


@final
@dataclass(frozen=True, slots=True, kw_only=True)
class ArtifactWidth(_Divergence[int]):
    """A nonterminal frame carries a payload narrower than the declared frame width."""


@final
@dataclass(frozen=True, slots=True, kw_only=True)
class ArtifactIdentity(_Divergence[bytes]):
    """The streamed digest diverges from the declared artifact identity."""


@final
@dataclass(frozen=True, slots=True, kw_only=True)
class ArtifactReference(_Divergence[ArtifactRef | None]):
    """A frame or receipt carries a reference the stream never declared."""


@final
@dataclass(frozen=True, slots=True, kw_only=True)
class ArtifactOpaque:
    """Reference traversal met a foreign-extension slot it may not unpack."""

    type_url: str


@final
@dataclass(frozen=True, slots=True, kw_only=True)
class ArtifactCycle:
    """Reference traversal met a message already live on its own ancestry."""

    type_name: str


@final
@dataclass(frozen=True, slots=True, kw_only=True)
class ArtifactResealed:
    """A single-use spool refused a second seal."""

    path: Path


type ArtifactRefusal = (
    ArtifactEmpty | ArtifactExtent | ArtifactWidth | ArtifactIdentity | ArtifactReference | ArtifactOpaque | ArtifactCycle | ArtifactResealed
)


def rendered(refusal: ArtifactRefusal, /) -> str:
    """Render one refusal as its own law token beside the evidence it carries.

    Returns:
        The rendered refusal.
    """
    match refusal:
        case ArtifactEmpty():
            return "artifact empty"
        case ArtifactExtent(expected=expected, actual=actual):
            return f"artifact extent {expected} != {actual}"
        case ArtifactWidth(expected=expected, actual=actual):
            return f"artifact frame width {expected} != {actual}"
        case ArtifactIdentity(expected=expected, actual=actual):
            return f"artifact identity {expected.hex()} != {actual.hex()}"
        case ArtifactReference(expected=expected, actual=actual):
            return f"artifact reference {expected} != {actual}"
        case ArtifactOpaque(type_url=type_url):
            return f"artifact reference traversal refuses {type_url}"
        case ArtifactCycle(type_name=type_name):
            return f"artifact reference traversal cycles at {type_name}"
        case ArtifactResealed(path=path):
            return f"artifact spool {path} is already sealed"
        case _ as unreachable:
            assert_never(unreachable)


@final
class ArtifactError(Exception):
    """The egress carrier reconstructing one artifact refusal for a foreign caller."""

    __slots__ = ("_refusal",)

    def __init__(self, refusal: ArtifactRefusal) -> None:
        """Reconstruct one railed refusal as the raise a generated stream demands."""
        super().__init__(rendered(refusal))
        self._refusal = refusal

    @property
    def refusal(self) -> ArtifactRefusal:
        """The closed artifact law that refused, carrying its own evidence."""
        return self._refusal
```

## [03]-[PROOF]

- Owner: `confirm` is the one two-axis comparison every proof folds through; `_minted` digests a spool off the loop; `references` walks a message.
- Law: `confirm` reads extent before identity — a short or overrun stream reports the axis a caller acts on, equal-extent divergence is substitution.
- Law: digests compare through `hmac.compare_digest` at every site — `confirm` and the bare-digest claim alike — never `==`.
- Law: `_minted` rails `ArtifactEmpty` below the extent floor and `ArtifactExtent` above the ceiling before any claim is held.
- Law: `_ceiling` tightens a copy's ceiling to a claimed full reference's extent, so an overrun refuses at the first excess chunk.
- Law: `_SPOOL_THREADS` bounds digest and cleanup threads under one `anyio.CapacityLimiter`, so a burst of seals never exhausts the worker pool.
- Law: `references` walks an explicit LIFO frontier with `_Left` markers retiring ancestry, so no recursion limit bounds the depth.
- Law: discovery order is descriptor field, list element, sorted map key; `_collapsed` keeps first discovery, two extents railing `ArtifactExtent`.
- Law: `references` returns a bare `ArtifactRef` as itself and refuses a `wkt.Any` slot at any depth with `ArtifactOpaque`.
- Law: `references` refuses a message live on its own ancestry with `ArtifactCycle`, so a self-referencing `Struct` never loops.
- Entry: `confirm(expected, actual)`, `references(message)`.
- Packages: `protobuf-py` (`DescField` value kinds, `Message`, `wkt.Any`), `expression` (`Result`), `anyio.to_thread`, stdlib `hashlib` and `hmac`.
- Growth: a new descriptor value kind is one `_embedded` match arm; a new proof axis is one `confirm` comparison in its ordered slot.
- Boundary: proof opens no spool it was not handed a path for; custody hands paths in, proof hands references out.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable, Iterator, Mapping, Sequence
from dataclasses import dataclass
import hashlib
import hmac
from pathlib import Path
from typing import assert_never, Final, final, Protocol

import anyio
import anyio.to_thread
from expression import Error, Ok, Result
from protobuf import DescField, DescFieldValueList, DescFieldValueMap, DescFieldValueMessage, DescMessage, Message
from protobuf.wkt import Any

from rasm.contracts.rasm.contracts.artifact.artifact_pb import ArtifactRef


# --- [SERVICES] -------------------------------------------------------------------------

_SPOOL_THREADS: Final = anyio.CapacityLimiter(_SPOOL_THREAD_CEILING)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PROOF]


def confirm(expected: ArtifactRef, actual: ArtifactRef, /) -> Result[ArtifactRef, ArtifactRefusal]:
    """Require an observed reference to repeat both axes of the declared one.

    Extent is read before identity so a truncated or overrun stream reports the axis a
    caller can act on; a digest divergence at equal extent is the substitution case.

    Returns:
        The confirmed reference, or the axis that diverged.
    """
    if expected.artifact_bytes != actual.artifact_bytes:
        return Error(ArtifactExtent(expected=expected.artifact_bytes, actual=actual.artifact_bytes))
    if not hmac.compare_digest(expected.sha256, actual.sha256):
        return Error(ArtifactIdentity(expected=expected.sha256, actual=actual.sha256))
    return Ok(actual)


def _octets(path: Path, /) -> tuple[int, bytes]:
    """Measure and digest one spool in a single owning thread.

    Returns:
        The spool's extent beside its SHA-256 digest.
    """
    with path.open("rb") as handle:
        return path.stat().st_size, hashlib.file_digest(handle, "sha256").digest()


async def _minted(path: Path, /) -> Result[ArtifactRef, ArtifactRefusal]:
    """Mint the canonical reference of one spool from its own octets.

    Returns:
        The minted reference, or the extent law the spool broke.
    """
    extent, digest = await anyio.to_thread.run_sync(_octets, path, limiter=_SPOOL_THREADS)
    if extent < _LAW.extent_floor:
        return Error(ArtifactEmpty())
    if extent > _LAW.extent_ceiling:
        return Error(ArtifactExtent(expected=_LAW.extent_ceiling, actual=extent))
    return Ok(ArtifactRef(sha256=digest, artifact_bytes=extent))


def _claimed(minted: ArtifactRef, claim: ArtifactClaim | None, /) -> Result[ArtifactRef, ArtifactRefusal]:
    """Hold a minted reference against whichever claim the caller stated.

    Returns:
        The claimed reference, or the axis that diverged.
    """
    match claim:
        case None:
            return Ok(minted)
        case bytes() as digest:
            if hmac.compare_digest(minted.sha256, digest):
                return Ok(minted)
            return Error(ArtifactIdentity(expected=digest, actual=minted.sha256))
        case ArtifactRef() as declared:
            return confirm(declared, minted)
        case _ as unreachable:
            assert_never(unreachable)


def _ceiling(claim: ArtifactClaim | None, /) -> int:
    """Read the tightest extent ceiling the claim and the corpus law agree on.

    Returns:
        The octet ceiling one copy may reach.
    """
    match claim:
        case ArtifactRef() as declared:
            return min(declared.artifact_bytes, _LAW.extent_ceiling)
        case bytes() | None:
            return _LAW.extent_ceiling
        case _ as unreachable:
            assert_never(unreachable)


# --- [TRAVERSAL]


class _Node(Protocol):
    """The descriptor-driven walk a generated message answers, free of its invariant field-name parameter."""

    def __iter__(self) -> Iterator[DescField]:
        """Iterate the fields this message sets."""

    def __getitem__(self, field: DescField, /) -> object:
        """Read one set field's value."""

    def desc(self) -> DescMessage:
        """Read the descriptor this message was generated from."""


@final
@dataclass(frozen=True, slots=True, kw_only=True)
class _Left:
    """A frontier marker retiring one message from the live ancestry."""

    identity: int


def _embedded(node: _Node, /) -> Iterator[_Node]:
    """Yield one message's embedded messages in descriptor, element, and key order.

    Yields:
        Each embedded generated message.
    """
    for field in node:
        value = node[field]
        match field.value:
            case DescFieldValueMessage(message=DescMessage()) if isinstance(value, Message):
                yield value
            case DescFieldValueList(element=DescMessage()) if isinstance(value, Sequence):
                yield from (item for item in value if isinstance(item, Message))
            case DescFieldValueMap(value=DescMessage()) if isinstance(value, Mapping):
                yield from (found for key in sorted(value) if isinstance(found := value[key], Message))
            case _:
                continue


def _descent(node: _Node, /) -> list[_Node | _Left]:
    """Seat one message's leave marker beneath its embedded messages, reversed so a LIFO frontier pops walk order.

    Returns:
        The frontier chunk this message contributes.
    """
    return [_Left(identity=id(node)), *reversed(tuple(_embedded(node)))]


def _collapsed(discovered: Iterable[ArtifactRef], /) -> Result[tuple[ArtifactRef, ...], ArtifactRefusal]:
    """Collapse repeated references, holding only extent-coherent duplicates.

    Returns:
        The unique references in discovery order, or the pair that disagreed.
    """
    unique: dict[bytes, ArtifactRef] = {}
    for artifact in discovered:
        prior = unique.setdefault(artifact.sha256, artifact)
        if prior.artifact_bytes != artifact.artifact_bytes:
            return Error(ArtifactExtent(expected=prior.artifact_bytes, actual=artifact.artifact_bytes))
    return Ok(tuple(unique.values()))


def references[F: str](message: Message[F], /) -> Result[tuple[ArtifactRef, ...], ArtifactRefusal]:
    """Discover embedded artifact references without recursing to caller-supplied depth.

    Returns:
        The extent-coherent reference set, or the traversal law the message broke.
    """
    if isinstance(message, ArtifactRef):
        return Ok((message,))
    if isinstance(message, Any):
        return Error(ArtifactOpaque(type_url=message.type_url))
    found: list[ArtifactRef] = []
    ancestry = {id(message)}
    frontier = _descent(message)
    while frontier:
        node = frontier.pop()
        match node:
            case _Left(identity=identity):
                ancestry.discard(identity)
            case ArtifactRef():
                found.append(node)
            case Any():
                return Error(ArtifactOpaque(type_url=node.type_url))
            case _:
                identity = id(node)
                if identity in ancestry:
                    return Error(ArtifactCycle(type_name=node.desc().type_name))
                ancestry.add(identity)
                frontier.extend(_descent(node))
    return _collapsed(found)
```

## [04]-[CUSTODY]

- Owner: `output` is the one spool owner — a `TemporaryDirectory` under `_SPOOL_PREFIX` holding one `ArtifactSink` at `_SPOOL_STEM` with its suffix.
- Owner: `stage` and `receive` compose `output` for a caller-owned source and an inbound frame stream; neither mints a second spool or a second proof.
- Law: `ArtifactSink.seal` spends its single use on a proved seal alone; a refusal leaves `ArtifactOpen` for a corrected claim or a late write.
- Law: a second seal on `ArtifactSealed` rails `ArtifactResealed`; an unwritten spool rails `ArtifactEmpty` and keeps its seal unclaimed.
- Law: `seal()` with no source proves the octets a native producer wrote to `path` in place, copying nothing; a stated source copies first.
- Law: `stage` copies a caller-owned path into the spool before proof, so a mutation landing on the origin after the seal moves no proved octet.
- Law: `_discarded` runs cleanup under `CancelScope(shield=True)`, so a cancelled enclosing scope still removes the spool directory.
- Law: `output` refuses a suffix that is not one dotted alphanumeric extension with `ValueError`, closing the one traversal door a suffix opens.
- Law: `_FrameLaw._admitted` rails `ArtifactReference` on a frame with no reference or a second reference, `ArtifactExtent` on the overrunning frame.
- Law: `_FrameLaw._admitted` rails `ArtifactWidth` on a nonterminal frame under `frame_ceiling`, refusing a fragmented body before its digest.
- Law: `_FrameLaw.settled` confirms the sealed spool against the declared reference; short rails `ArtifactExtent`, empty rails `ArtifactEmpty`.
- Law: `_FrameLaw.payloads` stops at the first refusing frame and releases the caller-owned source in `finally` through `AsyncClosable`.
- Law: `_pumped` releases its chunk source the same way and rails `ArtifactExtent` the moment the running extent passes the ceiling.
- Entry: `output(*, suffix="")`, `stage(source, *, claim=None)`, `receive(source, *, claim=None)` as async contexts.
- Entry: `ArtifactSink.path` and `ArtifactSink.seal(source=None, *, claim=None)` on the sink `output` yields.
- Packages: `anyio` (`open_file`, `CancelScope`, `to_thread`, `lowlevel.checkpoint`), stdlib `tempfile` and `contextlib`.
- Growth: a new source shape is one `ArtifactSource` arm and one `_copied` match arm; a new frame rule is one `_FrameLaw._admitted` check.
- Boundary: custody owns the path for the context's lifetime alone; a persisting consumer copies inside the context and holds no path past exit.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import AsyncGenerator, AsyncIterator
from contextlib import asynccontextmanager
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import assert_never, final

import anyio
import anyio.lowlevel
import anyio.to_thread
from expression import Error, Ok, Result

from rasm.contracts.rasm.contracts.artifact.artifact_pb import ArtifactFrame, ArtifactRef
from rasm.runtime.transport.body import AsyncClosable


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SPOOL]


async def _one(body: bytes, /) -> AsyncGenerator[bytes]:
    """Present one in-memory body as a single-chunk stream.

    Yields:
        The whole body.
    """
    await anyio.lowlevel.checkpoint()
    if body:
        yield body


async def _slices(reader: anyio.AsyncFile[bytes], /) -> AsyncGenerator[bytes]:
    """Slice an already-open reader into frame-width chunks.

    Yields:
        Each chunk the reader still holds.
    """
    while chunk := await reader.read(_LAW.frame_ceiling):
        yield chunk


async def _pumped(path: Path, chunks: AsyncIterator[bytes], /, *, ceiling: int) -> Result[int, ArtifactRefusal]:
    """Pump one chunk stream into the spool under a stated octet ceiling.

    Returns:
        The written extent, or the ceiling the stream overran.
    """
    extent = 0
    try:
        async with await anyio.open_file(path, "wb") as target:
            async for chunk in chunks:
                extent += len(chunk)
                if extent > ceiling:
                    return Error(ArtifactExtent(expected=ceiling, actual=extent))
                await target.write(chunk)
    finally:
        if isinstance(chunks, AsyncClosable):
            await chunks.aclose()
    return Ok(extent)


async def _copied(path: Path, source: ArtifactSource, /, *, ceiling: int) -> Result[int, ArtifactRefusal]:
    """Copy any admitted source shape into the spool without re-reading it later.

    Returns:
        The written extent, or the ceiling the source overran.
    """
    match source:
        case bytes() as body:
            return await _pumped(path, _one(body), ceiling=ceiling)
        case Path() as origin:
            async with await anyio.open_file(origin, "rb") as reader:
                return await _pumped(path, _slices(reader), ceiling=ceiling)
        case AsyncIterator() as stream:
            return await _pumped(path, stream, ceiling=ceiling)
        case _ as unreachable:
            assert_never(unreachable)


async def _discarded(directory: TemporaryDirectory[str], /) -> None:
    """Retire one spool directory under a shield an outer cancellation cannot abort."""
    with anyio.CancelScope(shield=True):
        await anyio.to_thread.run_sync(directory.cleanup, limiter=_SPOOL_THREADS)


# --- [MODELS] ---------------------------------------------------------------------------


@final
class ArtifactSink:
    """A single-use helper-owned spool whose one seal proves its own octets."""

    __slots__ = ("_custody",)

    def __init__(self, path: Path) -> None:
        """Open one spool holding its single unclaimed seal."""
        self._custody: ArtifactCustody = ArtifactOpen(path=path)

    @property
    def path(self) -> Path:
        """The stable path a producer writes within the surrounding output context."""
        match self._custody:
            case ArtifactOpen(path=path):
                return path
            case ArtifactSealed(artifact=artifact):
                return artifact.path
            case _ as unreachable:
                assert_never(unreachable)

    async def seal(self, source: ArtifactSource | None = None, /, *, claim: ArtifactClaim | None = None) -> Result[OwnedArtifact, ArtifactRefusal]:
        """Fold one source into custody, prove the spool, and advance the seal once.

        A refused seal leaves custody open so a caller may correct its claim and retry;
        only a proved seal consumes the single use. Omitting the source seals the octets
        a native producer already wrote to `path`, copying nothing.

        Returns:
            The proved artifact, or the law its octets broke.
        """
        match self._custody:
            case ArtifactSealed(artifact=artifact):
                return Error(ArtifactResealed(path=artifact.path))
            case ArtifactOpen(path=path):
                proved = await _proved(path, source=source, claim=claim)
                self._custody = proved.map(lambda artifact: ArtifactSealed(artifact=artifact)).default_value(self._custody)
                return proved
            case _ as unreachable:
                assert_never(unreachable)


@final
class _FrameLaw:
    """One inbound frame stream's own reference, width, and extent proof."""

    __slots__ = ("_declared", "_extent", "_refusal")

    def __init__(self, declared: ArtifactRef | None) -> None:
        """Open one inbound stream's proof against whichever reference the caller stated."""
        self._declared = declared
        self._extent = 0
        self._refusal: ArtifactRefusal | None = None

    async def payloads(self, frames: AsyncIterator[ArtifactFrame], /) -> AsyncGenerator[bytes]:
        """Project a frame stream onto its payload octets while proving every frame.

        Yields:
            Each admitted frame payload, in arrival order.
        """
        try:
            async for frame in frames:
                self._refusal = self._admitted(frame)
                if self._refusal is not None:
                    return
                yield frame.payload
        finally:
            if isinstance(frames, AsyncClosable):
                await frames.aclose()

    def settled(self, sealed: Result[OwnedArtifact, ArtifactRefusal], /) -> Result[OwnedArtifact, ArtifactRefusal]:
        """Settle a sealed spool against the reference the stream itself declared.

        Returns:
            The proved artifact, or the first law the stream broke.
        """
        if self._refusal is not None:
            return Error(self._refusal)
        if sealed.is_error() or self._declared is None:
            return sealed if sealed.is_error() else Error(ArtifactEmpty())
        owned = sealed.ok
        return confirm(self._declared, owned.artifact).map(lambda proved: OwnedArtifact(artifact=proved, path=owned.path))

    def _admitted(self, frame: ArtifactFrame, /) -> ArtifactRefusal | None:
        """Hold one arriving frame against the stream's reference, width, and extent law.

        Returns:
            The law this frame broke, or nothing when it admits.
        """
        coordinate = frame.artifact
        if coordinate is None:
            return ArtifactReference(expected=self._declared, actual=None)
        if self._declared is None:
            self._declared = coordinate
        elif confirm(self._declared, coordinate).is_error():
            return ArtifactReference(expected=self._declared, actual=coordinate)
        if self._extent and self._extent % _LAW.frame_ceiling:
            return ArtifactWidth(expected=_LAW.frame_ceiling, actual=self._extent % _LAW.frame_ceiling)
        self._extent += len(frame.payload)
        if self._extent > coordinate.artifact_bytes:
            return ArtifactExtent(expected=coordinate.artifact_bytes, actual=self._extent)
        return None


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CUSTODY]


async def _proved(path: Path, /, *, source: ArtifactSource | None, claim: ArtifactClaim | None) -> Result[OwnedArtifact, ArtifactRefusal]:
    """Copy any source into the spool when one is stated, then prove the spool once.

    Returns:
        The proved artifact, or the first law its octets broke.
    """
    if source is not None:
        copied = await _copied(path, source, ceiling=_ceiling(claim))
        if copied.is_error():
            return Error(copied.error)
    minted = await _minted(path)
    if minted.is_error():
        return Error(minted.error)
    return _claimed(minted.ok, claim).map(lambda proved: OwnedArtifact(artifact=proved, path=path))


@asynccontextmanager
async def output(*, suffix: str = "") -> AsyncGenerator[ArtifactSink]:
    """Own a writable artifact spool until the surrounding context exits.

    Yields:
        The single-use spool a native producer writes and then seals.

    Raises:
        ValueError: The suffix is not one alphanumeric extension.
    """
    if suffix and (not suffix.startswith(".") or not suffix[1:].isalnum()):
        raise ValueError("artifact output suffix must be one alphanumeric extension")
    directory = TemporaryDirectory(prefix=_SPOOL_PREFIX)
    sink = ArtifactSink(Path(directory.name, f"{_SPOOL_STEM}{suffix}"))
    try:
        async with await anyio.open_file(sink.path, "wb"):
            pass
        yield sink
    finally:
        await _discarded(directory)


@asynccontextmanager
async def stage(source: ArtifactSource, /, *, claim: ArtifactClaim | None = None) -> AsyncGenerator[Result[OwnedArtifact, ArtifactRefusal]]:
    """Copy a caller-owned source into a verified helper-owned spool.

    Yields:
        The proved artifact, or the law the source broke.
    """
    async with output() as sink:
        yield await sink.seal(source, claim=claim)


@asynccontextmanager
async def receive(
    source: AsyncIterator[ArtifactFrame], /, *, claim: ArtifactRef | None = None
) -> AsyncGenerator[Result[OwnedArtifact, ArtifactRefusal]]:
    """Receive and prove one framed artifact into a helper-owned spool.

    Yields:
        The proved artifact, or the first law the frame stream broke.
    """
    async with output() as sink:
        law = _FrameLaw(claim)
        yield law.settled(await sink.seal(law.payloads(source)))
```

## [05]-[FRAMING]

- Owner: `ArtifactStream[E]` is the one framing state machine, re-proving the sealed octets before the first envelope and stamping every frame.
- Law: `_opened` folds `_minted` through `confirm` once, so a spool mutated after its seal raises `ArtifactError` before any envelope leaves.
- Law: every envelope carries a `frame_ceiling` chunk until the tail; a reader ending short of the extent raises `ArtifactError(ArtifactExtent)`.
- Law: the stream closes its reader at the declared extent, on the short-read raise, on `aclose`, and on context exit; no handle outlives its frames.
- Law: `frames`, `fetch_responses`, and `put_requests` bind the three envelope rows over one stream, so direction wrappers never fork the machine.
- Law: `_unwrapped` inverts a row, raising `ArtifactError(ArtifactReference)` on an envelope carrying no frame and releasing its source in `finally`.
- Entry: `frames(artifact)`, `fetch_responses(artifact)`, `put_requests(artifact)`; `put_frames(requests)`, `fetch_frames(responses)`.
- Packages: `anyio` (`AsyncFile`, `open_file`), `rasm.contracts` (`ArtifactFrame`, `FetchResponse`, `PutRequest`).
- Growth: a new RPC direction is one `_ArtifactEnvelope` row and its wrap and unwrap entry pair.
- Boundary: framing emits and unwraps envelopes; it opens no socket and proves no inbound frame — `receive` does.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import AsyncGenerator, AsyncIterator
from typing import final

import anyio

from rasm.contracts.rasm.contracts.artifact.artifact_pb import ArtifactFrame, FetchResponse, PutRequest
from rasm.runtime.transport.body import AsyncClosable


# --- [MODELS] ---------------------------------------------------------------------------


@final
class ArtifactStream[E]:
    """One sealed artifact emitted as envelopes after re-proving its custody octets."""

    __slots__ = ("_artifact", "_emitted", "_envelope", "_reader")

    def __init__(self, artifact: OwnedArtifact, envelope: _ArtifactEnvelope[E], /) -> None:
        """Bind one owned artifact to the envelope its RPC direction carries."""
        self._artifact = artifact
        self._envelope = envelope
        self._emitted = 0
        self._reader: anyio.AsyncFile[bytes] | None = None

    def __aiter__(self) -> ArtifactStream[E]:
        """Serve as this stream's own iterator.

        Returns:
            This stream.
        """
        return self

    async def __anext__(self) -> E:
        """Emit the next envelope, proving the sealed octets before the first one.

        Returns:
            The next envelope.

        Raises:
            ArtifactError: The spool no longer holds the octets its seal proved.
            StopAsyncIteration: Every declared octet has been framed.
        """
        declared = self._artifact.artifact
        if self._emitted >= declared.artifact_bytes:
            await self.aclose()
            raise StopAsyncIteration
        reader = self._reader if self._reader is not None else await self._opened()
        chunk = await reader.read(_LAW.frame_ceiling)
        if not chunk:
            await self.aclose()
            raise ArtifactError(ArtifactExtent(expected=declared.artifact_bytes, actual=self._emitted))
        self._emitted += len(chunk)
        return self._envelope.wrap(ArtifactFrame(payload=chunk, artifact=declared))

    async def aclose(self) -> None:
        """Release the spool handle this stream holds."""
        reader, self._reader = self._reader, None
        if reader is not None:
            await reader.aclose()

    async def __aenter__(self) -> ArtifactStream[E]:
        """Bracket the spool handle for a consumer that wants deterministic release.

        Returns:
            This stream.
        """
        return self

    async def __aexit__(self, *_details: object) -> None:
        """Release the spool handle on scope exit."""
        await self.aclose()

    async def _opened(self) -> anyio.AsyncFile[bytes]:
        """Prove the sealed octets once, then open the spool for framing.

        Returns:
            The open spool reader.

        Raises:
            ArtifactError: The spool diverged from the reference its seal minted.
        """
        proved = (await _minted(self._artifact.path)).bind(lambda found: confirm(self._artifact.artifact, found))
        if proved.is_error():
            raise ArtifactError(proved.error)
        reader = await anyio.open_file(self._artifact.path, "rb")
        self._reader = reader
        return reader


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [FRAMING]


def frames(artifact: OwnedArtifact, /) -> ArtifactStream[ArtifactFrame]:
    """Frame one owned artifact while re-proving its sealed octets.

    Returns:
        The bare frame stream.
    """
    return ArtifactStream(artifact, _FRAMES)


def fetch_responses(artifact: OwnedArtifact, /) -> ArtifactStream[FetchResponse]:
    """Wrap one verified artifact for the generated Fetch response stream.

    Returns:
        The Fetch response stream.
    """
    return ArtifactStream(artifact, _FETCH)


def put_requests(artifact: OwnedArtifact, /) -> ArtifactStream[PutRequest]:
    """Wrap one verified artifact for the generated Put request stream.

    Returns:
        The Put request stream.
    """
    return ArtifactStream(artifact, _PUT)


async def _unwrapped[E](envelopes: AsyncIterator[E], envelope: _ArtifactEnvelope[E], /) -> AsyncGenerator[ArtifactFrame]:
    """Unwrap one direction-unique envelope stream onto the shared frame it carries.

    Yields:
        Each carried frame.

    Raises:
        ArtifactError: An envelope arrived carrying no frame.
    """
    try:
        async for carried in envelopes:
            frame = envelope.unwrap(carried)
            if frame is None:
                raise ArtifactError(ArtifactReference(expected=None, actual=None))
            yield frame
    finally:
        if isinstance(envelopes, AsyncClosable):
            await envelopes.aclose()


def put_frames(requests: AsyncIterator[PutRequest], /) -> AsyncGenerator[ArtifactFrame]:
    """Unwrap a generated Put request stream for frame-centric receipt proof.

    Returns:
        The unwrapped frame stream.
    """
    return _unwrapped(requests, _PUT)


def fetch_frames(responses: AsyncIterator[FetchResponse], /) -> AsyncGenerator[ArtifactFrame]:
    """Unwrap a generated Fetch response stream for frame-centric receipt proof.

    Returns:
        The unwrapped frame stream.
    """
    return _unwrapped(responses, _FETCH)
```

## [06]-[TRANSFER]

- Owner: `ArtifactTransfer` composes the generated client with custody — `put` stages and publishes, `publish` streams and confirms, `fetch` receives.
- Law: `_budget` projects `anyio.current_effective_deadline` onto `timeout_ms` as remaining milliseconds per dial, `None` where no scope bounds it.
- Law: no window is re-threaded through a signature — the caller's enclosing `fail_after` or `move_on_after` is the budget every dial reads live.
- Law: `publish` rails `ArtifactReference` when the receipt carries no reference and `confirm`s the peer's reference against the staged one.
- Law: `fetch` dials `FetchRequest(sha256=...)` alone and hands the expected reference to `receive` as its claim, so both axes prove on arrival.
- Entry: `ArtifactTransfer(client)`; `put(source, *, claim=None)`, `publish(artifact)`, and `fetch(artifact)` as an async context yielding the rail.
- Packages: `anyio` (`current_effective_deadline`, `current_time`), `rasm.contracts` (`FetchRequest`), stdlib `math`.
- Growth: a new `ArtifactService` rpc is one `_ArtifactClient` method and one `ArtifactTransfer` method over the existing custody owners.
- Boundary: the transfer holds no retry, window, or credential — `transport/serve#CAPABILITY_INVOKE` owns re-drive; the client arrives constructed.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import AsyncGenerator
from contextlib import asynccontextmanager
import math
from typing import final

import anyio
from expression import Error, Result

from rasm.contracts.rasm.contracts.artifact.artifact_pb import ArtifactRef, FetchRequest


# --- [COMPOSITION] ----------------------------------------------------------------------


def _budget() -> int | None:
    """Project the enclosing cancel scope's deadline onto the generated client's budget.

    Returns:
        The remaining milliseconds, or nothing where no scope bounds the call.
    """
    deadline = anyio.current_effective_deadline()
    if math.isinf(deadline):
        return None
    return max(0, math.ceil((deadline - anyio.current_time()) * 1000))


@final
class ArtifactTransfer:
    """Compose generated ArtifactService calls with the verified custody lifecycle."""

    __slots__ = ("_client",)

    def __init__(self, client: _ArtifactClient) -> None:
        """Bind one generated ArtifactService client."""
        self._client = client

    async def put(self, source: ArtifactSource, /, *, claim: ArtifactClaim | None = None) -> Result[ArtifactRef, ArtifactRefusal]:
        """Stage, publish, and confirm one artifact.

        Returns:
            The confirmed reference, or the law the transfer broke.
        """
        async with stage(source, claim=claim) as staged:
            if staged.is_error():
                return Error(staged.error)
            return await self.publish(staged.ok)

    async def publish(self, artifact: OwnedArtifact, /) -> Result[ArtifactRef, ArtifactRefusal]:
        """Publish and confirm an artifact already held in helper-owned custody.

        Returns:
            The confirmed reference, or the law the receipt broke.
        """
        async with put_requests(artifact) as requests:
            response = await self._client.put(requests, timeout_ms=_budget())
        if response.artifact is None:
            return Error(ArtifactReference(expected=artifact.artifact, actual=None))
        return confirm(artifact.artifact, response.artifact)

    @asynccontextmanager
    async def fetch(self, artifact: ArtifactRef, /) -> AsyncGenerator[Result[OwnedArtifact, ArtifactRefusal]]:
        """Fetch and verify one artifact into a helper-owned spool.

        Yields:
            The proved artifact, or the law the fetched stream broke.
        """
        responses = self._client.fetch(FetchRequest(sha256=artifact.sha256), timeout_ms=_budget())
        async with receive(fetch_frames(responses), claim=artifact) as received:
            yield received


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ArtifactClaim",
    "ArtifactCustody",
    "ArtifactCycle",
    "ArtifactEmpty",
    "ArtifactError",
    "ArtifactExtent",
    "ArtifactIdentity",
    "ArtifactLaw",
    "ArtifactOpaque",
    "ArtifactOpen",
    "ArtifactReference",
    "ArtifactRefusal",
    "ArtifactResealed",
    "ArtifactSealed",
    "ArtifactSink",
    "ArtifactSource",
    "ArtifactStream",
    "ArtifactTransfer",
    "ArtifactWidth",
    "OwnedArtifact",
    "confirm",
    "fetch_frames",
    "fetch_responses",
    "frames",
    "output",
    "put_frames",
    "put_requests",
    "receive",
    "references",
    "rendered",
    "stage",
]
```

## [07]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
