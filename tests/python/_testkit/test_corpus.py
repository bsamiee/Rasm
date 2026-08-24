"""Live falsification laws for manifest V2 proof vectors and generated Python bindings."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import Iterable
from hashlib import sha256
from pathlib import Path
from shutil import copytree

import msgspec
import pytest
import xxhash

from assay.rails import contracts as contracts_rail
from assay.rails.contracts import (
    Asset,
    BlockedReadiness,
    Case,
    Entry,
    ExpectedFacts,
    FieldFacts,
    Fingerprint,
    HlcValueFacts,
    Manifest,
    MatrixMarketFacts,
    MessageActor,
    ProofVector,
    PublisherDefinition,
    PythonPackageResource,
    SpecimenAsset,
    VerifiedReadiness,
    WaveformFacts,
)
from tests.python._testkit import corpus as corpus_kit
from tests.python._testkit.corpus import assert_corpus, load_manifest
from tests.python._testkit.spec import assert_ok


# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (load_manifest, assert_corpus)
_ROOT = Path(__file__).parents[3]
_CORPUS = _ROOT / "tests/contracts"


# --- [OPERATIONS] -----------------------------------------------------------------------


def _case(manifest: Manifest, entry_id: str, case_id: str) -> tuple[Entry, Case]:
    entry = next(entry for entry in manifest.entries if entry.id == entry_id)
    return entry, next(case for case in entry.cases if case.id == case_id)


def _fingerprint(asset: Asset, raw: bytes, /) -> Fingerprint:
    value = xxhash.xxh128(raw, seed=0).hexdigest() if asset.fingerprint.algorithm == "xxh128" else sha256(raw).hexdigest()
    return Fingerprint(algorithm=asset.fingerprint.algorithm, value=value)


def _pinned[A: Asset](asset: A, raw: bytes, /) -> A:
    return msgspec.structs.replace(asset, bytes=len(raw), fingerprint=_fingerprint(asset, raw))


def _stage(root: Path, entry: Entry, case: Case, assets: Iterable[tuple[Asset, bytes]], /) -> None:
    """Stage one held case over a whole copy of the live corpus, so the held manifest is the only variable.

    A witness that stages its own specimens alone leaves every other declared asset absent, and the
    first absent-asset refusal answers in place of the law under attack.
    """
    copytree(_CORPUS, root, dirs_exist_ok=True)
    for asset, raw in assets:
        path = root / asset.path
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(raw)
    (root / "manifest.json").write_bytes(msgspec.json.encode(Manifest(entries=(msgspec.structs.replace(entry, cases=(case,)),))))


def test_live_corpus_proves_every_verified_vector_through_its_oracle() -> None:
    """The proof receipt equals the live tagged readiness and proof-vector census."""
    manifest = assert_ok(load_manifest(_CORPUS))
    proof = assert_corpus(_CORPUS)
    verified = tuple(readiness for entry in manifest.entries for case in entry.cases if isinstance(readiness := case.readiness, VerifiedReadiness))
    blocked = tuple(readiness for entry in manifest.entries for case in entry.cases if isinstance(readiness := case.readiness, BlockedReadiness))
    assert proof.assay_corpus_oracles.vectors == sum(len(readiness.vectors) for readiness in verified)
    assert proof.assay_corpus_oracles.semantic_conformance == sum(
        len(readiness.vectors) for readiness in verified if readiness.oracle == "semantic-conformance"
    )
    assert proof.assay_corpus_oracles.semantic_roundtrip == sum(
        len(readiness.vectors) for readiness in verified if readiness.oracle == "semantic-roundtrip"
    )
    assert proof.assay_corpus_oracles.value_parity == sum(len(readiness.vectors) for readiness in verified if readiness.oracle == "value-parity")
    assert proof.assay_corpus_oracles.publisher_digest == sum(
        len(readiness.vectors) for readiness in verified if readiness.oracle == "publisher-digest"
    )
    assert proof.assay_corpus_oracles.external_digest == sum(
        len(readiness.vectors) for readiness in verified if readiness.oracle == "external-digest"
    )
    assert proof.python_oracles.semantic_conformance == 0
    assert proof.python_oracles.vectors == sum(
        vector.expected is None or vector.expected.facts_format not in {"backend-generation", "hdf5-facts", "matrix-market-facts"}
        for readiness in verified
        for vector in readiness.vectors
    )
    assert proof.python_bindings > 0
    assert proof.blocked_cases == len(blocked)
    assert proof.registered_specimens == sum(len(vector.specimens) for readiness in verified for vector in readiness.vectors)
    assert proof.registered_expected == sum(vector.expected is not None for readiness in verified for vector in readiness.vectors)


def test_generated_public_root_refuses_a_descriptor_alias(tmp_path: Path) -> None:
    """A generated actor root resolves through the imported publisher descriptor."""
    manifest = assert_ok(load_manifest(_CORPUS))
    entry, case = _case(manifest, "cloudevents", "proto")
    actor = next(actor for actor in case.consumers if actor.anchor.startswith("python:") and isinstance(actor, MessageActor))
    aliased = msgspec.structs.replace(actor, roots=("io.cloudevents.v1.Alias",))
    held = msgspec.structs.replace(case, consumers=tuple(aliased if candidate is actor else candidate for candidate in case.consumers))
    _stage(tmp_path, entry, held, ())
    with pytest.raises(AssertionError, match="public root"):
        assert_corpus(tmp_path)


def test_package_distribution_refuses_a_repeated_namespace_path_alias() -> None:
    """Resource lookup begins at the one package source root, never the first matching dotted subsequence."""
    distribution = PythonPackageResource(path="tmp/rasm/contracts/vendor/schema.avsc", package="rasm.contracts")
    with pytest.raises(AssertionError, match=r"outside the exact rasm\.contracts package root"):
        corpus_kit._package_bytes(distribution)


def test_rpc_direction_refuses_a_method_alias(tmp_path: Path) -> None:
    """An RPC direction resolves its exact service method before its specimen is decoded."""
    manifest = assert_ok(load_manifest(_CORPUS))
    entry, case = _case(manifest, "compute-rpc", "tessellate-request")
    consumer = msgspec.structs.replace(case.consumers[0], method="rasm.contracts.compute.ComputeService.Alias")
    specimen = SpecimenAsset(
        path="rpc/request.bin", bytes=0, fingerprint=Fingerprint(algorithm="xxh128", value=xxhash.xxh128(b"", seed=0).hexdigest())
    )
    readiness = VerifiedReadiness(oracle="semantic-roundtrip", vectors=(ProofVector(specimens=(specimen,)),))
    held = msgspec.structs.replace(case, consumers=(consumer,), readiness=readiness)
    _stage(tmp_path, entry, held, ((specimen, b""),))
    with pytest.raises(AssertionError, match="do not contain method"):
        assert_corpus(tmp_path)


def test_semantic_roundtrip_refuses_malformed_protobuf(tmp_path: Path) -> None:
    """A digest-correct malformed specimen cannot pass semantic roundtrip."""
    manifest = assert_ok(load_manifest(_CORPUS))
    entry, case = _case(manifest, "fault-wire", "detail")
    readiness = case.readiness
    assert isinstance(readiness, VerifiedReadiness)
    specimen = _pinned(readiness.vectors[0].specimens[0], b"\x80")
    held = msgspec.structs.replace(case, readiness=msgspec.structs.replace(readiness, vectors=(ProofVector(specimens=(specimen,)),)))
    _stage(tmp_path, entry, held, ((specimen, b"\x80"),))
    with pytest.raises(AssertionError, match="asset-invalid"):
        assert_corpus(tmp_path)


def test_value_parity_refuses_typed_fact_drift(tmp_path: Path) -> None:
    """A value-parity specimen must decode to its typed expected facts."""
    manifest = assert_ok(load_manifest(_CORPUS))
    entry, case = _case(manifest, "hlc-stamp", "message")
    readiness = case.readiness
    assert isinstance(readiness, VerifiedReadiness)
    vector = readiness.vectors[0]
    expected = vector.expected
    assert expected is not None
    facts = HlcValueFacts(physical="1", logical="999")
    facts_raw = msgspec.json.encode(facts)
    expected = _pinned(expected, facts_raw)
    held_vector = ProofVector(specimens=vector.specimens, expected=expected)
    held = msgspec.structs.replace(case, readiness=msgspec.structs.replace(readiness, vectors=(held_vector,)))
    specimens = tuple((specimen, (_CORPUS / specimen.path).read_bytes()) for specimen in vector.specimens)
    _stage(tmp_path, entry, held, (*specimens, (expected, facts_raw)))
    with pytest.raises(AssertionError, match="differs"):
        assert_corpus(tmp_path)


def test_semantic_conformance_refuses_hdf_fact_drift(tmp_path: Path) -> None:
    """An HDF specimen is decoded through its closed facts variant, not accepted by fingerprint alone."""
    manifest = assert_ok(load_manifest(_CORPUS))
    entry, case = _case(manifest, "hdf5-exchange", "field")
    readiness = case.readiness
    assert isinstance(readiness, VerifiedReadiness)
    vector = readiness.vectors[0]
    expected = vector.expected
    assert expected is not None
    facts = msgspec.json.decode((_CORPUS / expected.path).read_bytes(), type=ExpectedFacts)
    assert isinstance(facts, FieldFacts)
    drifted = msgspec.structs.replace(facts, root_attributes=msgspec.structs.replace(facts.root_attributes, bound=facts.root_attributes.bound + 1.0))
    facts_raw = msgspec.json.encode(drifted)
    expected = _pinned(expected, facts_raw)
    specimen = vector.specimens[0]
    specimen_raw = (_CORPUS / specimen.path).read_bytes()
    held_vector = ProofVector(specimens=(specimen,), expected=expected)
    held = msgspec.structs.replace(case, readiness=msgspec.structs.replace(readiness, vectors=(held_vector,)))
    _stage(tmp_path, entry, held, ((specimen, specimen_raw), (expected, facts_raw)))
    with pytest.raises(AssertionError, match="differs"):
        assert_corpus(tmp_path)


def test_hdf_field_executes_the_registered_virtualizarr_consumer() -> None:
    """The field specimen must virtualize into the exact typed shape and source byte ranges registered as proof."""
    manifest = assert_ok(load_manifest(_CORPUS))
    _entry, case = _case(manifest, "hdf5-exchange", "field")
    readiness = case.readiness
    assert isinstance(readiness, VerifiedReadiness)
    vector = readiness.vectors[0]
    expected = vector.expected
    assert expected is not None
    facts = msgspec.json.decode((_CORPUS / expected.path).read_bytes(), type=ExpectedFacts)
    assert isinstance(facts, FieldFacts)
    found = assert_ok(contracts_rail._hdf_facts(_CORPUS / vector.specimens[0].path, facts))
    assert isinstance(found, FieldFacts)
    assert found == facts
    assert found.virtual.dims == ("phony_dim_0", "phony_dim_1")
    assert found.virtual.shape == (2, 2) and found.virtual.dtype == "float32"
    assert tuple((chunk.key, chunk.offset, chunk.length) for chunk in found.virtual.chunks) == (("0.0", 76, 16), ("1.0", 92, 16))


def test_hdf_waveform_executes_the_registered_csharp_consumer_law() -> None:
    """The Python specimen must carry the exact native HDF facts C# ImportWaveforms admits."""
    manifest = assert_ok(load_manifest(_CORPUS))
    _entry, case = _case(manifest, "hdf5-exchange", "waveform")
    readiness = case.readiness
    assert isinstance(readiness, VerifiedReadiness)
    vector = readiness.vectors[0]
    expected = vector.expected
    assert expected is not None
    facts = msgspec.json.decode((_CORPUS / expected.path).read_bytes(), type=ExpectedFacts)
    assert isinstance(facts, WaveformFacts)
    found = assert_ok(contracts_rail._hdf_facts(_CORPUS / vector.specimens[0].path, facts))
    assert isinstance(found, WaveformFacts)
    assert found == facts
    assert found.shape == (4, 2) and found.sample_rate == pytest.approx(200.0)


def test_matrix_market_executes_both_registered_peer_decoders() -> None:
    """Both official minters must normalize to one sparse operand through SciPy's Matrix Market decoder."""
    manifest = assert_ok(load_manifest(_CORPUS))
    _entry, case = _case(manifest, "matrix-market-exchange", "sparse")
    readiness = case.readiness
    assert isinstance(readiness, VerifiedReadiness)
    vector = readiness.vectors[0]
    expected = vector.expected
    assert expected is not None
    facts = msgspec.json.decode((_CORPUS / expected.path).read_bytes(), type=ExpectedFacts)
    assert isinstance(facts, MatrixMarketFacts)
    assert all(assert_ok(contracts_rail._matrix_market_facts(_CORPUS / specimen.path)) == facts for specimen in vector.specimens)


def test_publisher_digest_refuses_a_generated_source_alias(tmp_path: Path) -> None:
    """Publisher bytes must own every imported generated root they claim."""
    manifest = assert_ok(load_manifest(_CORPUS))
    entry, case = _case(manifest, "cloudevents", "proto")
    readiness = case.readiness
    definition = case.definition
    assert isinstance(readiness, VerifiedReadiness)
    assert isinstance(definition, PublisherDefinition)
    source = "vendor/alias/no-such.proto"
    raw = (_CORPUS / readiness.vectors[0].specimens[0].path).read_bytes()
    specimen = _pinned(msgspec.structs.replace(readiness.vectors[0].specimens[0], path=source), raw)
    held = msgspec.structs.replace(
        case,
        definition=msgspec.structs.replace(definition, source=source),
        readiness=msgspec.structs.replace(readiness, vectors=(ProofVector(specimens=(specimen,)),)),
    )
    _stage(tmp_path, entry, held, ((specimen, raw),))
    with pytest.raises(AssertionError, match="does not own generated root"):
        assert_corpus(tmp_path)


def test_python_distribution_refuses_a_non_package_path(tmp_path: Path) -> None:
    """A distributed publisher resource resolves through importlib.resources at its declared package root."""
    manifest = assert_ok(load_manifest(_CORPUS))
    entry, case = _case(manifest, "cloudevents", "avsc")
    readiness = case.readiness
    assert isinstance(readiness, VerifiedReadiness)
    specimen = readiness.vectors[0].specimens[0]
    distribution = next(row for row in specimen.distributions if isinstance(row, PythonPackageResource))
    aliased = msgspec.structs.replace(distribution, path="libs/python/contracts/not-rasm/cloudevents.avsc")
    specimen = msgspec.structs.replace(specimen, distributions=tuple(aliased if row is distribution else row for row in specimen.distributions))
    raw = (_CORPUS / specimen.path).read_bytes()
    held = msgspec.structs.replace(case, readiness=msgspec.structs.replace(readiness, vectors=(ProofVector(specimens=(specimen,)),)))
    _stage(tmp_path, entry, held, ((specimen, raw),))
    with pytest.raises(AssertionError, match="package root"):
        assert_corpus(tmp_path)


def test_external_digest_proves_exact_bytes_without_an_expected_twin(tmp_path: Path) -> None:
    """External digest is exact byte custody and carries no unrelated expected-facts asset."""
    manifest = assert_ok(load_manifest(_CORPUS))
    entry, case = _case(manifest, "content-identity", "mesh-adjacency")
    readiness = case.readiness
    assert isinstance(readiness, VerifiedReadiness)
    specimen = msgspec.structs.replace(readiness.vectors[0].specimens[0], minter="")
    raw = (_CORPUS / specimen.path).read_bytes()
    held = msgspec.structs.replace(case, readiness=VerifiedReadiness(oracle="external-digest", vectors=(ProofVector(specimens=(specimen,)),)))
    _stage(tmp_path, entry, held, ((specimen, raw),))
    proof = assert_corpus(tmp_path)
    assert proof.assay_corpus_oracles.external_digest == 1
    assert proof.python_oracles.external_digest == 1
    assert proof.registered_expected == 0
