"""Descriptor-ruled artifact custody, framing, identity proof, and reference traversal."""

from collections.abc import AsyncGenerator, AsyncIterator, Callable, Iterable, Iterator, Mapping, Sequence
from contextlib import asynccontextmanager
from dataclasses import dataclass
import hashlib
import hmac
import math
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import assert_never, Final, final, Protocol

import anyio
import anyio.lowlevel
import anyio.to_thread
from expression import Error, Ok, Result
from protobuf import DescField, DescFieldValueList, DescFieldValueMap, DescFieldValueMessage, DescMessage, Message
from protobuf.wkt import Any

from rasm.contracts.admission import AsyncClosable
from rasm.contracts.gen.buf.validate.validate_pb import BytesRules, ext_field, FieldRules, UInt64Rules
from rasm.contracts.gen.rasm.contracts.artifact.v1.artifact_pb import ArtifactFrame, ArtifactRef, FetchRequest, FetchResponse, PutRequest, PutResponse


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

# Every bound below READS the corpus `buf.validate` rule off the generated descriptor. A literal
# restating one here would be contract law spelled twice in two languages with nothing to raise on drift.


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
