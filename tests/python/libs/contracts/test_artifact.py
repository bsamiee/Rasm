"""Artifact custody, framing, identity, wrapper, and traversal laws."""


# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import AsyncIterator
import hashlib
from pathlib import Path
from typing import get_args

import anyio
import anyio.lowlevel
from expression import Result
from protobuf import Oneof
from protobuf.wkt import Any, Struct, Value
import pytest

from rasm.contracts.artifact import (
    _LAW as LAW,
    ArtifactCycle,
    ArtifactEmpty,
    ArtifactError,
    ArtifactExtent,
    ArtifactIdentity,
    ArtifactLaw,
    ArtifactOpaque,
    ArtifactOpen,
    ArtifactReference,
    ArtifactRefusal,
    ArtifactResealed,
    ArtifactSealed,
    ArtifactSink,
    ArtifactStream,
    ArtifactTransfer,
    ArtifactWidth,
    confirm,
    fetch_frames,
    fetch_responses,
    frames,
    output,
    OwnedArtifact,
    put_frames,
    put_requests,
    receive,
    references,
    rendered,
    stage,
)
from rasm.contracts.gen.rasm.contracts.artifact.v1.artifact_pb import ArtifactFrame, ArtifactRef, FetchRequest, FetchResponse, PutRequest, PutResponse
from rasm.contracts.gen.rasm.contracts.cad.v1.operations_pb import BooleanInputs
from rasm.contracts.gen.rasm.contracts.cad.v1.types_pb import SealedStep


# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (
    ArtifactCycle,
    ArtifactEmpty,
    ArtifactError,
    ArtifactExtent,
    ArtifactIdentity,
    ArtifactLaw,
    ArtifactOpaque,
    ArtifactOpen,
    ArtifactReference,
    ArtifactResealed,
    ArtifactSealed,
    ArtifactSink,
    ArtifactStream,
    ArtifactTransfer,
    ArtifactWidth,
    confirm,
    fetch_frames,
    fetch_responses,
    frames,
    output,
    OwnedArtifact,
    put_frames,
    put_requests,
    receive,
    references,
    rendered,
    stage,
)


# --- [MODELS] ---------------------------------------------------------------------------


class _ArtifactClient:
    def __init__(self, body: bytes = b"") -> None:
        self.body = body
        self.put_timeout: int | None = None
        self.fetch_timeout: int | None = None
        self.fetch_request: FetchRequest | None = None
        self.put_requests: tuple[PutRequest, ...] = ()

    def fetch(self, request: FetchRequest, *, timeout_ms: int | None = None) -> AsyncIterator[FetchResponse]:
        self.fetch_timeout = timeout_ms
        self.fetch_request = request
        reference = _ref(self.body)
        return _items(FetchResponse(frame=ArtifactFrame(payload=self.body, artifact=reference)))

    async def put(self, request: AsyncIterator[PutRequest], *, timeout_ms: int | None = None) -> PutResponse:
        self.put_timeout = timeout_ms
        self.put_requests = await _collect(request)
        body = b"".join(row.frame.payload for row in self.put_requests if row.frame is not None)
        return PutResponse(artifact=_ref(body))


# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.fixture
def anyio_backend() -> str:
    return "asyncio"


async def _items[T](*items: T) -> AsyncIterator[T]:
    await anyio.lowlevel.checkpoint()
    for item in items:
        yield item


async def _collect[T](items: AsyncIterator[T]) -> tuple[T, ...]:
    return tuple([item async for item in items])


def _ref(body: bytes) -> ArtifactRef:
    return ArtifactRef(sha256=hashlib.sha256(body).digest(), artifact_bytes=len(body))


def _refused[T](outcome: Result[T, ArtifactRefusal], refusal: type[ArtifactRefusal], /) -> None:
    assert outcome.is_error(), outcome
    assert isinstance(outcome.error, refusal), outcome


# --- [LAW]


def test_every_frame_and_extent_bound_is_read_from_the_corpus_descriptor() -> None:
    assert LAW.identity_bytes == 32
    assert LAW.frame_floor == 1
    assert LAW.frame_ceiling == 65_536
    assert LAW.extent_floor == 1
    assert LAW.extent_ceiling == 1_073_741_824


# --- [CUSTODY]


@pytest.mark.anyio
async def test_stage_matches_the_standard_sha256_vector_and_cleans_its_path() -> None:
    specimen = b"abc"
    expected = bytes.fromhex("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad")

    async with stage(specimen, claim=expected) as staged:
        assert staged.is_ok(), staged
        artifact = staged.ok
        path = artifact.path
        assert artifact.artifact.sha256 == expected
        assert artifact.artifact.artifact_bytes == len(specimen)
        async with await anyio.open_file(path, "rb") as stream:
            assert await stream.read() == specimen

    assert not path.exists()


@pytest.mark.anyio
async def test_stage_rails_a_claim_the_staged_octets_contradict() -> None:
    async with stage(b"abc", claim=b"z" * 32) as staged:
        _refused(staged, ArtifactIdentity)


@pytest.mark.anyio
async def test_stage_copies_a_caller_owned_path_before_proof(tmp_path: Path) -> None:
    """Custody copies a caller-owned path, so a mutation landing after the seal cannot move the proved octets."""
    origin = tmp_path / "caller.bin"
    body = b"caller-owned" * (LAW.frame_ceiling // 4)
    origin.write_bytes(body)

    async with stage(origin) as staged:
        assert staged.is_ok(), staged
        artifact = staged.ok
        assert artifact.artifact == _ref(body)
        assert artifact.path != origin
        origin.write_bytes(b"mutated")
        async with await anyio.open_file(artifact.path, "rb") as stream:
            assert await stream.read() == body
        assert tuple(frame.artifact for frame in await _collect(frames(artifact))) == (_ref(body),) * 3


@pytest.mark.anyio
async def test_output_cleanup_is_shielded_from_cancellation() -> None:
    path: Path | None = None
    with anyio.CancelScope() as scope:
        async with output() as sink:
            path = sink.path
            scope.cancel()
            await anyio.sleep_forever()
    assert path is not None
    assert not path.exists()


@pytest.mark.anyio
async def test_output_seals_a_native_result_in_place_once_without_a_second_spool() -> None:
    async with output(suffix=".glb") as sink:
        assert sink.path.suffix == ".glb"
        async with await anyio.open_file(sink.path, "wb") as stream:
            await stream.write(b"native-output")
        sealed = await sink.seal()
        assert sealed.is_ok(), sealed
        assert sealed.ok.path == sink.path
        assert sealed.ok.artifact == _ref(b"native-output")
        _refused(await sink.seal(), ArtifactResealed)
        path = sink.path

    assert not path.exists()

    with pytest.raises(ValueError, match="one alphanumeric extension"):
        async with output(suffix="../outside"):
            pass


@pytest.mark.anyio
async def test_an_unwritten_spool_rails_empty_and_leaves_its_seal_unclaimed() -> None:
    async with output() as sink:
        _refused(await sink.seal(), ArtifactEmpty)
        async with await anyio.open_file(sink.path, "wb") as stream:
            await stream.write(b"late")
        retried = await sink.seal()
        assert retried.is_ok(), retried
        assert retried.ok.artifact == _ref(b"late")


# --- [FRAMING]


@pytest.mark.anyio
async def test_frames_are_exact_chunks_and_reprove_mutation_before_the_first_frame() -> None:
    body = b"a" * (2 * LAW.frame_ceiling + 3)
    async with stage(body) as staged:
        artifact = staged.ok
        emitted = await _collect(frames(artifact))
        assert tuple(len(frame.payload) for frame in emitted) == (LAW.frame_ceiling, LAW.frame_ceiling, 3)
        assert all(frame.artifact == artifact.artifact for frame in emitted)

        async with await anyio.open_file(artifact.path, "wb") as stream:
            await stream.write(b"changed")
        with pytest.raises(ArtifactError) as mutated:
            await _collect(frames(artifact))
        assert isinstance(mutated.value.refusal, ArtifactExtent)


@pytest.mark.anyio
async def test_receive_proves_reference_extent_identity_and_cleanup() -> None:
    body = b"ordered-frame-body"
    reference = _ref(body)
    async with receive(_items(ArtifactFrame(payload=body, artifact=reference)), claim=reference) as received:
        assert received.is_ok(), received
        path = received.ok.path
        assert received.ok.artifact == reference
        async with await anyio.open_file(path, "rb") as stream:
            assert await stream.read() == body
    assert not path.exists()

    cases: tuple[tuple[AsyncIterator[ArtifactFrame], type[ArtifactRefusal]], ...] = (
        (_items(), ArtifactEmpty),
        (_items(ArtifactFrame(payload=b"toolong", artifact=ArtifactRef(sha256=b"x" * 32, artifact_bytes=3))), ArtifactExtent),
        (_items(ArtifactFrame(payload=b"short", artifact=ArtifactRef(sha256=b"x" * 32, artifact_bytes=6))), ArtifactExtent),
        (_items(ArtifactFrame(payload=b"same", artifact=ArtifactRef(sha256=b"x" * 32, artifact_bytes=4))), ArtifactIdentity),
        (_items(ArtifactFrame(payload=b"lost")), ArtifactReference),
    )
    for source, refusal in cases:
        async with receive(source) as refused:
            _refused(refused, refusal)


@pytest.mark.anyio
async def test_receive_refuses_a_nonterminal_frame_narrower_than_the_declared_width() -> None:
    body = b"ordered-frame-body"
    reference = _ref(body)
    fragmented = (ArtifactFrame(payload=body[:7], artifact=reference), ArtifactFrame(payload=body[7:], artifact=reference))
    async with receive(_items(*fragmented), claim=reference) as received:
        _refused(received, ArtifactWidth)


@pytest.mark.anyio
async def test_receive_refuses_a_stream_whose_frames_declare_two_references() -> None:
    first = ArtifactRef(sha256=b"x" * 32, artifact_bytes=LAW.frame_ceiling + 1)
    second = ArtifactRef(sha256=b"y" * 32, artifact_bytes=LAW.frame_ceiling + 1)
    stream = _items(ArtifactFrame(payload=b"a" * LAW.frame_ceiling, artifact=first), ArtifactFrame(payload=b"b", artifact=second))
    async with receive(stream) as received:
        _refused(received, ArtifactReference)


@pytest.mark.anyio
async def test_direction_wrappers_share_the_raw_frame_state_machine() -> None:
    async with stage(b"wrapped") as staged:
        responses = await _collect(fetch_responses(staged.ok))
        assert all(isinstance(response, FetchResponse) for response in responses)
        requests = tuple(PutRequest(frame=response.frame) for response in responses)
        unwrapped = await _collect(put_frames(_items(*requests)))
        assert tuple(frame.payload for frame in unwrapped) == (b"wrapped",)

    with pytest.raises(ArtifactError) as absent:
        await _collect(put_frames(_items(PutRequest())))
    assert isinstance(absent.value.refusal, ArtifactReference)


# --- [TRANSFER]


@pytest.mark.anyio
async def test_transfer_projects_the_enclosing_deadline_and_confirms_put_publish_and_fetch() -> None:
    client = _ArtifactClient(b"remote")
    transfer = ArtifactTransfer(client)

    with anyio.fail_after(30):
        published = await transfer.put(b"local")
        assert published.is_ok(), published
        assert published.ok == _ref(b"local")
        assert client.put_timeout is not None
        assert 29_000 < client.put_timeout <= 30_000
        assert all(isinstance(request, PutRequest) for request in client.put_requests)

    unbounded = await transfer.put(b"local")
    assert unbounded.is_ok(), unbounded
    assert client.put_timeout is None

    async with output() as sink:
        async with await anyio.open_file(sink.path, "wb") as stream:
            await stream.write(b"native")
        owned = await sink.seal()
        assert owned.is_ok(), owned
        assert (await transfer.publish(owned.ok)).ok == owned.ok.artifact

    expected = _ref(b"remote")
    async with transfer.fetch(expected) as fetched:
        assert fetched.is_ok(), fetched
        assert fetched.ok.artifact == expected
    assert client.fetch_request == FetchRequest(sha256=expected.sha256)


# --- [REFUSALS]


def test_every_refusal_case_renders_a_distinct_token_carrying_its_own_evidence() -> None:
    """The closed family projects total: every case renders, renders distinctly, and keeps its evidence to the egress carrier."""
    cases: tuple[tuple[ArtifactRefusal, tuple[str, ...]], ...] = (
        (ArtifactEmpty(), ()),
        (ArtifactExtent(expected=4, actual=9), ("4", "9")),
        (ArtifactWidth(expected=8, actual=3), ("8", "3")),
        (ArtifactIdentity(expected=b"\x01" * 32, actual=b"\x02" * 32), ("01" * 32, "02" * 32)),
        (ArtifactReference(expected=ArtifactRef(sha256=b"a" * 32, artifact_bytes=4), actual=None), ("None",)),
        (ArtifactOpaque(type_url="type.googleapis.com/rasm.contracts.cad.v1.SealedStep"), ("type.googleapis.com/rasm.contracts.cad.v1.SealedStep",)),
        (ArtifactCycle(type_name="google.protobuf.Struct"), ("google.protobuf.Struct",)),
        (ArtifactResealed(path=Path("/spool/artifact.glb")), ("/spool/artifact.glb",)),
    )
    assert {type(refusal) for refusal, _ in cases} == set(get_args(ArtifactRefusal.__value__)), "the refusal family grew a case no rendering proves"

    tokens = tuple(rendered(refusal) for refusal, _ in cases)
    assert len(set(tokens)) == len(cases), tokens
    for (refusal, evidence), token in zip(cases, tokens, strict=True):
        assert all(fragment in token for fragment in evidence), (refusal, token)
        assert str(ArtifactError(refusal)) == token, refusal
        assert ArtifactError(refusal).refusal is refusal


@pytest.mark.anyio
async def test_a_refused_stream_releases_the_caller_owned_source_it_stopped_reading() -> None:
    """Custody closes a caller-owned async source at the refusal, never leaving it to collection."""
    released = anyio.Event()

    async def source() -> AsyncIterator[ArtifactFrame]:
        await anyio.lowlevel.checkpoint()
        try:
            yield ArtifactFrame(payload=b"first", artifact=ArtifactRef(sha256=b"x" * 32, artifact_bytes=2))
            pytest.fail("custody read past the frame that refused")
        finally:
            released.set()

    async with receive(source()) as received:
        _refused(received, ArtifactExtent)
    assert released.is_set()


# --- [REFERENCES]


def test_confirm_reads_extent_before_identity_and_returns_each_axis() -> None:
    first = ArtifactRef(sha256=b"a" * 32, artifact_bytes=4)
    assert confirm(first, first).ok is first
    _refused(confirm(first, ArtifactRef(sha256=b"a" * 32, artifact_bytes=9)), ArtifactExtent)
    _refused(confirm(first, ArtifactRef(sha256=b"b" * 32, artifact_bytes=4)), ArtifactIdentity)


def test_descriptor_reference_discovery_retains_typed_coordinates() -> None:
    first = ArtifactRef(sha256=b"a" * 32, artifact_bytes=4)
    second = ArtifactRef(sha256=b"b" * 32, artifact_bytes=8)
    third = ArtifactRef(sha256=b"c" * 32, artifact_bytes=16)
    walked = [SealedStep(artifact=first), SealedStep(artifact=second), SealedStep(artifact=third), SealedStep(artifact=first)]
    assert references(BooleanInputs(operands=walked)).ok == (first, second, third)
    unordered = Struct(fields={"z": Value(kind=Oneof("string_value", "last")), "a": Value(kind=Oneof("string_value", "first"))})
    assert references(unordered).ok == ()
    assert references(first).ok == (first,)

    conflicted = BooleanInputs(operands=[SealedStep(artifact=first), SealedStep(artifact=ArtifactRef(sha256=first.sha256, artifact_bytes=9))])
    _refused(references(conflicted), ArtifactExtent)
    _refused(references(Any(type_url="type.googleapis.com/rasm.contracts.cad.v1.SealedStep", value=b"opaque")), ArtifactOpaque)


def test_reference_traversal_refuses_a_message_live_on_its_own_ancestry() -> None:
    cyclic = Struct()
    cyclic.fields["self"] = Value(kind=Oneof("struct_value", cyclic))
    _refused(references(cyclic), ArtifactCycle)
