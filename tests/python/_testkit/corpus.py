"""Live contracts-corpus proof over the assay registry and generated Python bindings."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import Iterator
from hashlib import sha256
from importlib import import_module
from importlib.resources import files
from pathlib import Path, PurePosixPath
from tempfile import NamedTemporaryFile
from types import ModuleType
from typing import assert_never, Final

from expression import Result
import msgspec
from protobuf import DescFile, Message, Registry
from protobuf.wkt import FileDescriptorSet
import xxhash

from assay.rails.contracts import (
    Actor,
    ApplicationAuthority,
    Asset,
    BackendGenerationFacts,
    BlockedReadiness,
    Case,
    ClientRequestActor,
    ClientResponseActor,
    CloudEventDefinition,
    ContentDigestFacts,
    CorpusOracleReceipt,
    DomainAuthority,
    ExpectedAsset,
    ExpectedFacts,
    FieldFacts,
    GraduationFacts,
    HlcValueFacts,
    InfrastructureAuthority,
    LawDefinition,
    load_manifest,
    MatrixMarketFacts,
    MessageActor,
    Oracle,
    ProofVector,
    ProtoDefinition,
    prove_case,
    PublisherAuthority,
    PublisherDefinition,
    PythonPackageResource,
    SchemaDefinition,
    ServerRequestActor,
    ServerResponseActor,
    SparseFacts,
    SpecimenAsset,
    TypeScriptJsonModule,
    VerifiedReadiness,
    WaveformFacts,
)
import rasm.contracts.gen as generated
from rasm.contracts.gen.rasm.contracts.clock.v1.hlc_pb import Hlc
import rasm.contracts.vendor as vendored
from rasm.contracts.vendor.io.cloudevents.v1.cloudevents_pb import CloudEvent


# --- [MODELS] ---------------------------------------------------------------------------


class OracleProof(msgspec.Struct, frozen=True, gc=False, kw_only=True):
    """Closed vector census over the manifest V2 oracle family."""

    semantic_conformance: int
    semantic_roundtrip: int
    value_parity: int
    external_digest: int
    publisher_digest: int

    @property
    def vectors(self) -> int:
        return self.semantic_conformance + self.semantic_roundtrip + self.value_parity + self.external_digest + self.publisher_digest


class CorpusProof(msgspec.Struct, frozen=True, gc=False, kw_only=True):
    """Assay corpus verdicts beside independently executed Python proofs and binding checks."""

    assay_corpus_oracles: OracleProof
    python_oracles: OracleProof
    python_bindings: int
    blocked_cases: int
    registered_specimens: int
    registered_expected: int


# --- [CONSTANTS] ------------------------------------------------------------------------


_ROOTS: Final = (generated, vendored)
_FACTS: Final[msgspec.json.Decoder[ExpectedFacts]] = msgspec.json.Decoder(ExpectedFacts)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _closure(file: DescFile, /) -> Iterator[DescFile]:
    yield file
    for dependency in file.dependencies:
        yield from _closure(dependency)


def _emitted(root: ModuleType, /) -> Iterator[DescFile]:
    """Import every generated message module beneath one emission root and yield its descriptor closure.

    A hand-kept module roster silently narrows the registry the moment generation lands a package,
    so the census that proves the Python bindings derives from the emitted tree itself.

    Yields:
        Each descriptor file the emitted modules and their dependencies declare.
    """
    for base in root.__path__:
        for module in sorted(Path(base).rglob("*_pb.py")):
            descriptor: DescFile = import_module(".".join((root.__name__, *module.relative_to(base).with_suffix("").parts))).desc()
            yield from _closure(descriptor)


_FILES: Final = tuple({file.name: file for root in _ROOTS for file in _emitted(root)}.values())
_REGISTRY: Final = Registry(*_FILES)
_IMAGE: Final = FileDescriptorSet(file=[file.proto for file in _FILES]).to_binary()


def _actors(case: Case, /) -> tuple[Actor, ...]:
    match case.authority:
        case ApplicationAuthority():
            return case.consumers
        case DomainAuthority(producer=producer):
            return (producer, *case.consumers)
        case InfrastructureAuthority(minters=minters):
            return (*minters, *case.consumers)
        case PublisherAuthority():
            return case.consumers
        case _ as unreachable:
            assert_never(unreachable)


def _python(actor: Actor, /) -> bool:
    return actor.anchor.startswith("python:")


def _definition_message(case: Case, /) -> str | None:
    match case.definition:
        case ProtoDefinition(message=message) | CloudEventDefinition(message=message):
            return message
        case LawDefinition() | PublisherDefinition() | SchemaDefinition():
            return None
        case _ as unreachable:
            assert_never(unreachable)


def _method_message(actor: Actor, /) -> str:
    match actor:
        case MessageActor():
            raise AssertionError(f"{actor.anchor}: a message actor names no RPC method")
        case ClientRequestActor() | ServerRequestActor():
            request = True
        case ClientResponseActor() | ServerResponseActor():
            request = False
        case _ as unreachable:
            assert_never(unreachable)
    service_name, _, method_name = actor.method.rpartition(".")
    service = _REGISTRY.service(service_name)
    assert service is not None, f"{actor.anchor}: generated Python bindings do not contain service {service_name}"
    method = next((candidate for candidate in service.methods if candidate.name == method_name), None)
    assert method is not None, f"{actor.anchor}: generated Python bindings do not contain method {actor.method}"
    return method.input.type_name if request else method.output.type_name


def _actor_bindings(case: Case, /) -> int:
    message = _definition_message(case)
    proven = 0
    for actor in _actors(case):
        if not (_python(actor) and actor.binding == "generated"):
            continue
        match actor:
            case MessageActor(roots=roots):
                for root in roots:
                    assert _REGISTRY.message(root) is not None, f"{case.id}: generated Python bindings do not contain public root {root}"
                if message is not None:
                    assert _REGISTRY.message(message) is not None, f"{case.id}: generated Python bindings do not contain {message}"
                elif isinstance(case.definition, (LawDefinition, SchemaDefinition)):
                    raise AssertionError(f"{case.id}: {actor.anchor} claims generated custody for a non-generated definition")
            case ClientRequestActor() | ClientResponseActor() | ServerRequestActor() | ServerResponseActor():
                assert message is not None, f"{case.id}: {actor.anchor} binds an RPC direction to a non-Protobuf definition"
                bound = _method_message(actor)
                assert bound == message, f"{case.id}: {actor.anchor} binds {bound}, expected {message}"
            case _ as unreachable:
                assert_never(unreachable)
        for support in actor.supports:
            match support.kind:
                case "message":
                    found = _REGISTRY.message(support.fqn) is not None
                case "service":
                    found = _REGISTRY.service(support.fqn) is not None
                case "method":
                    service_name, _, method_name = support.fqn.rpartition(".")
                    service = _REGISTRY.service(service_name)
                    found = service is not None and any(method.name == method_name for method in service.methods)
                case invalid:
                    assert_never(invalid)
            assert found, f"{case.id}: generated Python bindings do not contain {support.kind} support {support.fqn}"
        proven += 1
    return proven


def _fingerprint(asset: Asset, raw: bytes, /) -> None:
    match asset.fingerprint.algorithm:
        case "xxh128":
            actual = xxhash.xxh128(raw, seed=0).hexdigest()
        case "sha256":
            actual = sha256(raw).hexdigest()
        case invalid:
            assert_never(invalid)
    expected = (asset.bytes, asset.fingerprint.value)
    found = (len(raw), actual)
    assert found == expected, f"{asset.path} drifted: expected {expected}, got {found}"


def _package_bytes(distribution: PythonPackageResource, /) -> bytes:
    path = PurePosixPath(distribution.path)
    package_root = PurePosixPath("libs/python/contracts").joinpath(*distribution.package.split("."))
    assert path.is_relative_to(package_root), f"{distribution.path}: path is outside the exact {distribution.package} package root"
    return files(distribution.package).joinpath(*path.relative_to(package_root).parts).read_bytes()


def _asset(root: Path, asset: Asset, /) -> bytes:
    raw = (root / asset.path).read_bytes()
    _fingerprint(asset, raw)
    distributions = asset.distributions if isinstance(asset, SpecimenAsset) else ()
    for distribution in distributions:
        match distribution:
            case PythonPackageResource():
                assert _package_bytes(distribution) == raw, f"{asset.path}: Python package resource differs from publisher bytes"
            case TypeScriptJsonModule():
                continue
            case _ as unreachable:
                assert_never(unreachable)
    return raw


def _decode(message: str, definition: ProtoDefinition | CloudEventDefinition, raw: bytes, /) -> Message[str]:
    descriptor = _REGISTRY.message(message)
    assert descriptor is not None, f"generated Python bindings do not contain {message}"
    return (
        descriptor.type.from_json(raw, ignore_unknown_fields=False, registry=_REGISTRY)
        if isinstance(definition, ProtoDefinition) and definition.framing == "proto-json"
        else descriptor.type.from_binary(raw, ignore_unknown_fields=False)
    )


def _roundtrip(case: Case, specimen: SpecimenAsset, raw: bytes, /) -> None:
    definition = case.definition
    assert isinstance(definition, (ProtoDefinition, CloudEventDefinition)), f"{case.id}: semantic roundtrip requires Protobuf"
    assert not (isinstance(definition, ProtoDefinition) and definition.framing == "canonical-frame"), (
        f"{case.id}: canonical-frame requires its law decoder, not a protobuf semantic roundtrip"
    )
    decoded = _decode(definition.message, definition, raw)
    canonical_json = decoded.to_json(registry=_REGISTRY, use_proto_field_name=True)
    json_cycle = type(decoded).from_json(canonical_json, ignore_unknown_fields=False, registry=_REGISTRY)
    assert json_cycle == decoded, f"{case.id}: {specimen.path} loses decoded facts through canonical ProtoJSON"
    if isinstance(definition, CloudEventDefinition):
        assert isinstance(decoded, CloudEvent), f"{case.id}: generated CloudEvent definition resolved to {type(decoded).__name__}"
        assert decoded.type == definition.type, f"{case.id}: {specimen.path} carries CloudEvent type {decoded.type!r}, expected {definition.type!r}"
    if not (isinstance(definition, ProtoDefinition) and definition.framing == "proto-json"):
        normalized = decoded.to_binary(write_unknown_fields=False)
        binary_cycle = type(decoded).from_binary(normalized, ignore_unknown_fields=False)
        assert binary_cycle == decoded, f"{case.id}: {specimen.path} changes across normalized Protobuf re-encoding"
        assert decoded.to_binary(write_unknown_fields=True) == normalized, f"{case.id}: {specimen.path} retains unknown Protobuf fields"


def _facts(case: Case, specimen: SpecimenAsset, expected: ExpectedFacts, root: Path, /) -> ExpectedFacts | None:
    path = root / specimen.path
    match expected:
        case ContentDigestFacts():
            return ContentDigestFacts(algorithm="xxh128", seed=0, value=xxhash.xxh128(path.read_bytes(), seed=0).hexdigest())
        case HlcValueFacts():
            definition = case.definition
            assert isinstance(definition, ProtoDefinition), f"{case.id}: HLC facts require a Protobuf definition"
            decoded = _decode(definition.message, definition, path.read_bytes())
            assert isinstance(decoded, Hlc), f"{case.id}: HLC facts resolved to {type(decoded).__name__}"
            return HlcValueFacts(physical=str(decoded.physical), logical=str(decoded.logical))
        case BackendGenerationFacts() | FieldFacts() | GraduationFacts() | SparseFacts() | WaveformFacts() | MatrixMarketFacts():
            return None
        case _ as unreachable:
            assert_never(unreachable)


def _expected(root: Path, asset: ExpectedAsset, /) -> ExpectedFacts:
    decoded = _FACTS.decode(_asset(root, asset))
    matches = (
        (asset.facts_format == "backend-generation-v1" and isinstance(decoded, BackendGenerationFacts))
        or (asset.facts_format == "content-digest-v1" and isinstance(decoded, ContentDigestFacts))
        or (asset.facts_format == "hlc-value-v1" and isinstance(decoded, HlcValueFacts))
        or (asset.facts_format == "hdf5-facts-v1" and isinstance(decoded, (FieldFacts, GraduationFacts, SparseFacts, WaveformFacts)))
        or (asset.facts_format == "matrix-market-facts-v1" and isinstance(decoded, MatrixMarketFacts))
    )
    assert matches, f"{asset.path}: {asset.facts_format} does not match {type(decoded).__name__}"
    return decoded


def _publisher(case: Case, vector: ProofVector, root: Path, /) -> None:
    assert isinstance(case.authority, PublisherAuthority), f"{case.id}: publisher digest requires publisher authority"
    definition = case.definition
    assert isinstance(definition, PublisherDefinition), f"{case.id}: publisher digest requires a publisher definition"
    license_row = definition.origin.license
    license_raw = (root / license_row.path).read_bytes()
    assert sha256(license_raw).hexdigest() == license_row.sha256, f"{case.id}: publisher license {license_row.path} drifted"
    source = PurePosixPath(definition.source)
    for specimen in vector.specimens:
        assert specimen.fingerprint.algorithm == "sha256", f"{case.id}: publisher custody requires SHA-256"
        path = PurePosixPath(specimen.path)
        assert path == source or path.is_relative_to(source), f"{case.id}: {specimen.path} is outside publisher source {source}"
        _asset(root, specimen)
    generated = tuple(
        root_name
        for actor in _actors(case)
        if _python(actor) and actor.binding == "generated" and isinstance(actor, MessageActor)
        for root_name in actor.roots
    )
    for root_name in generated:
        descriptor = _REGISTRY.message(root_name)
        assert descriptor is not None, f"{case.id}: generated Python bindings do not contain {root_name}"
        assert definition.source.endswith(descriptor.file.name), (
            f"{case.id}: publisher source {definition.source} does not own generated root {root_name} from {descriptor.file.name}"
        )


def _vector(case: Case, vector: ProofVector, root: Path, /) -> bool:
    readiness = case.readiness
    assert isinstance(readiness, VerifiedReadiness)
    match readiness.oracle:
        case "semantic-roundtrip":
            assert vector.expected is None, f"{case.id}: semantic roundtrip carries unrelated expected facts"
            for specimen in vector.specimens:
                _roundtrip(case, specimen, _asset(root, specimen))
            return True
        case "semantic-conformance":
            assert vector.expected is not None and vector.expected.facts_format == "hdf5-facts-v1", (
                f"{case.id}: Python has no local semantic-conformance route outside the Assay corpus oracle"
            )
            return False
        case "value-parity":
            assert vector.expected is not None, f"{case.id}: value parity requires typed expected facts"
            if vector.expected.facts_format in {"backend-generation-v1", "hdf5-facts-v1", "matrix-market-facts-v1"}:
                return False
            expected = _expected(root, vector.expected)
            for specimen in vector.specimens:
                _asset(root, specimen)
                found = _facts(case, specimen, expected, root)
                assert found is not None and found == expected, f"{case.id}: {specimen.path} differs from {vector.expected.path}"
            return True
        case "external-digest":
            assert vector.expected is None, f"{case.id}: external digest carries unrelated expected facts"
            for specimen in vector.specimens:
                _asset(root, specimen)
            return True
        case "publisher-digest":
            assert vector.expected is None, f"{case.id}: publisher digest carries unrelated expected facts"
            _publisher(case, vector, root)
            return True
        case invalid:
            assert_never(invalid)


def _count(proof: OracleProof, oracle: Oracle, vectors: int, /) -> OracleProof:
    match oracle:
        case "semantic-conformance":
            return msgspec.structs.replace(proof, semantic_conformance=proof.semantic_conformance + vectors)
        case "semantic-roundtrip":
            return msgspec.structs.replace(proof, semantic_roundtrip=proof.semantic_roundtrip + vectors)
        case "value-parity":
            return msgspec.structs.replace(proof, value_parity=proof.value_parity + vectors)
        case "external-digest":
            return msgspec.structs.replace(proof, external_digest=proof.external_digest + vectors)
        case "publisher-digest":
            return msgspec.structs.replace(proof, publisher_digest=proof.publisher_digest + vectors)
        case invalid:
            assert_never(invalid)


def _empty_oracles() -> OracleProof:
    return OracleProof(semantic_conformance=0, semantic_roundtrip=0, value_parity=0, external_digest=0, publisher_digest=0)


def _assay(root: Path, image: Path, subject: str, case: Case, /) -> CorpusOracleReceipt:
    match prove_case(root, subject, case, image=image):
        case Result(tag="ok", ok=receipt):
            pass
        case Result(error=reason):
            raise AssertionError(reason)
    readiness = case.readiness
    assert isinstance(readiness, VerifiedReadiness)
    assert receipt.subject == subject and receipt.oracle == readiness.oracle, f"{subject}: Assay returned a receipt for another case or oracle"
    assert receipt.vectors == len(readiness.vectors), f"{subject}: Assay vector census differs from the manifest"
    assert receipt.specimens == sum(len(vector.specimens) for vector in readiness.vectors), (
        f"{subject}: Assay specimen census differs from the manifest"
    )
    assert not receipt.findings, "; ".join(f"{subject} {finding.rule}: {finding.detail}" for finding in receipt.findings)
    return receipt


def _verified(proof: CorpusProof, root: Path, image: Path, subject: str, case: Case, /) -> CorpusProof:
    readiness = case.readiness
    assert isinstance(readiness, VerifiedReadiness)
    python_bindings = _actor_bindings(case)
    receipt = _assay(root, image, subject, case)
    python_vectors = sum(_vector(case, vector, root) for vector in readiness.vectors)
    return msgspec.structs.replace(
        proof,
        assay_corpus_oracles=_count(proof.assay_corpus_oracles, receipt.oracle, receipt.vectors),
        python_oracles=_count(proof.python_oracles, readiness.oracle, python_vectors),
        python_bindings=proof.python_bindings + python_bindings,
        registered_specimens=proof.registered_specimens + receipt.specimens,
        registered_expected=proof.registered_expected + sum(vector.expected is not None for vector in readiness.vectors),
    )


def assert_corpus(root: Path) -> CorpusProof:
    """Prove every V2 case through tagged authority, readiness, generated bindings, and its elected oracle.

    Returns:
        The closed census of blocked cases, verified vectors, and their evidence assets.

    Raises:
        AssertionError: A manifest case, generated binding, asset, distribution, or elected oracle is false.
    """
    match load_manifest(root):
        case Result(tag="ok", ok=manifest):
            pass
        case Result(error=fault):
            raise AssertionError(fault.message)
    proof = CorpusProof(
        assay_corpus_oracles=_empty_oracles(),
        python_oracles=_empty_oracles(),
        python_bindings=0,
        blocked_cases=0,
        registered_specimens=0,
        registered_expected=0,
    )
    with NamedTemporaryFile(prefix="rasm-contracts-", suffix=".binpb") as image:
        image.write(_IMAGE)
        image.flush()
        image_path = Path(image.name)
        for subject, case in ((f"{entry.id}/{case.id}", case) for entry in manifest.entries for case in entry.cases):
            match case.readiness:
                case BlockedReadiness():
                    proof = msgspec.structs.replace(proof, blocked_cases=proof.blocked_cases + 1)
                case VerifiedReadiness():
                    proof = _verified(proof, root, image_path, subject, case)
                case _ as unreachable:
                    assert_never(unreachable)
    return proof


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CorpusProof", "OracleProof", "assert_corpus", "load_manifest"]
