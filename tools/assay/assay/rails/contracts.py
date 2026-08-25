"""Drive the contracts plane through buf: the gate lanes, the corpus audit, scratch-regeneration freshness, and the committed regeneration.

``check`` probes every plugin binary, fans ``buf build``/``lint``/``format``/``generate``-to-scratch under one repo
lease, projects every `proto:`-derived seam schema into scratch through ``protoc-gen-jsonschema``, audits the corpus
registry against its schema, disk, anchors, derivations, emitted rosters, and the built descriptor set, then proves the
committed out roots fresh by byte diff — a local estate proof reaching no registry; ``generate`` probes, refuses on a
template-plugin miss, builds one complete package/schema image in scratch, validates it, and commits every package root
and derived schema through one recovery journal under the same lease. ``publish`` resolves the credential and the
module's default-label commit, runs the complete check, re-resolves that commit immediately before the irreversible
push, admits only the exact first-publish absence as bootstrap, and reads the pushed coordinate back off the label. The
msgspec manifest model here is the registry's one authority: ``manifest.schema.json`` derives from it and the audit rows
carry every cross-field invariant.
"""

from collections.abc import Callable, Hashable, Iterable, Iterator, Mapping
from dataclasses import dataclass
from hashlib import sha256
from importlib import import_module
import importlib.util
from math import isfinite
from operator import itemgetter
import os
from pathlib import Path, PurePosixPath
import re
from struct import pack
from types import MappingProxyType
from typing import Annotated, ClassVar, Final, Literal, override, Protocol, runtime_checkable, Self
lazy import shutil

from expression import Error, Ok, Result
from expression.collections import block
from expression.extra.result import sequence
import msgspec
import xxhash
lazy from google.protobuf.descriptor_pb2 import FileDescriptorSet as GoogleFileDescriptorSet
lazy from google.protobuf.descriptor_pool import DescriptorPool as GoogleDescriptorPool
lazy from google.protobuf.json_format import Parse, ParseError
lazy from google.protobuf.message import DecodeError, Message
lazy from google.protobuf.message_factory import GetMessageClass
lazy from google.protobuf.unknown_fields import UnknownFieldSet
lazy import jsonschema
lazy from obspec_utils.registry import ObjectStoreRegistry
lazy from obstore.exceptions import BaseError as ObjectStoreError
lazy from obstore.store import from_url
lazy from protobuf.wkt import DescriptorProto, FieldDescriptorProto, FileDescriptorSet
lazy from referencing import Registry, Resource
lazy from referencing.exceptions import Unresolvable
lazy from referencing.jsonschema import DRAFT202012
lazy from ruamel.yaml import YAML
lazy from virtualizarr import open_virtual_dataset
lazy from virtualizarr.manifests import ManifestArray
lazy from virtualizarr.parsers import HDFParser

from assay.composition.catalog import JSONSCHEMA_PLUGIN, select
from assay.composition.settings import AssaySettings
from assay.composition.store import ArtifactScope
from assay.core.exec import Executor
from assay.core.govern import leased
from assay.core.model import (
    Artifact,
    ArtifactKind,
    Band,
    BaseParams,
    Check,
    Claim,
    Completed,
    ContractsRun,
    Fault,
    InprocThunk,
    Language,
    Match,
    Mode,
    RailStatus,
    receipt,
    Report,
    Step,
    Tool,
    ToolArgs,
)
from assay.core.routing import discover, Routed, Scope
from assay.core.transaction import SwapTransaction
from assay.diagnostics import cap_note, fold
from assay.rails.contracts_generation import changes, compose_image, freshness_rows, GenerationImage, render


# --- [TYPES] ----------------------------------------------------------------------------

type EntryClass = Literal["application", "infrastructure", "domain", "publisher"]
type Oracle = Literal["semantic-conformance", "semantic-roundtrip", "value-parity", "external-digest", "publisher-digest"]
type FingerprintAlgorithm = Literal["xxh128", "sha256"]
type FactsFormat = Literal["backend-generation", "content-digest", "hdf5-facts", "hlc-value", "matrix-market-facts"]
type DistributionKind = Literal["python-package-resource", "typescript-json-module"]
type ActorBinding = Literal["generated", "package", "proof"]
type ActorDirection = Literal["message", "client-request", "client-response", "server-request", "server-response"]
type SupportKind = Literal["message", "service", "method"]
type ProtoFraming = Literal["proto-binary", "proto-json", "canonical-frame"]
type SchemaFraming = Literal["framed-binary", "proto-json", "canonical-json", "msgpack", "proto-binary", "container"]
type LawFormat = Literal["binary", "hdf5", "json", "text"]
type _Kind = Literal["message", "enum", "service", "method"]
type _Document = dict[str, object]
type _DistributionRow = tuple[str, EntryClass, SpecimenAsset, Distribution]
type _Projections = Mapping[str, Result[bytes, str]]
type _RosterFiles = Mapping[str, tuple[_File, ...]]
type _Rule = Callable[[_Corpus], Iterable[_Finding]]
type _Clause[T] = tuple[Callable[[T], bool], str, str, str]
type _Lanes = tuple[tuple[str, Completed], ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

CORPUS: Final = "libs/contracts"
MANIFEST: Final = "manifest.json"
SCHEMA: Final = "manifest.schema.json"
TEMPLATE: Final = f"{CORPUS}/buf.gen.yaml"
ROSTER_BEGIN: Final = "<!-- roster:begin -->"
ROSTER_END: Final = "<!-- roster:end -->"
_CONFIG: Final = f"{CORPUS}/buf.yaml"
_LOCK: Final = f"{CORPUS}/buf.lock"
_SCHEMA_ANCHOR: Final = "https://json-schema.org/draft/2020-12/schema"
_JSON_INDENT: Final = 4
_JSON_WIDTH: Final = 150
_LEASE: Final = "contracts"
_VENDOR: Final = "vendor"
_CONFORMANCE: Final = "conformance"
_ESTATE: Final = "proto"
_EMISSION: Final = PurePosixPath(CORPUS) / "gen"
_IMAGE: Final = "image.binpb"
_GEN: Final = "gen"
_SCHEMAS: Final = "schema"
_DIFF: Final = "freshness.diff"
_COMMIT_IMAGE: Final = "commit-image"
_COMMIT_MARKER: Final = ".contracts-generation-commit.json"
_BUNDLE: Final = ".jsonschema.strict.bundle.json"
_JSON_FRAMINGS: Final[frozenset[SchemaFraming]] = frozenset(("proto-json", "canonical-json"))
_NO_PROJECTIONS: Final[_Projections] = MappingProxyType({})
_NO_ROSTER_FILES: Final[_RosterFiles] = MappingProxyType({})
_ROOTS: Final[frozenset[str]] = frozenset((".api", _ESTATE, _VENDOR, _CONFORMANCE, _EMISSION.name))
_ROOT_FILES: Final[frozenset[str]] = frozenset((
    "README.md",
    "ARCHITECTURE.md",
    "RULINGS.md",
    "IDEAS.md",
    "TASKLOG.md",
    MANIFEST,
    SCHEMA,
    "buf.yaml",
    "buf.gen.yaml",
    "buf.lock",
    "package.json",
    "tsconfig.json",
    "pyproject.toml",
    "Rasm.Contracts.csproj",
    "packages.lock.json",
))
_IGNORED: Final[tuple[str, ...]] = ("git", "ls-files", "-z", "--others", "--ignored", "--exclude-standard", "--directory", "--", CORPUS)
_BLOCKER_FLOOR: Final = 24
_IGNORE_TIMEOUT_S: Final = 30.0
_LAW_CAP: Final = 240
_DIFF_LINES: Final = 400
_STALE_CAP: Final = 200
_TAIL: Final = 512
_CLOSED: Final = re.compile(r"\b(DISCHARGED|resolved|closed)\b", re.IGNORECASE)
_ANCHOR: Final = re.compile(r"^(dotnet|python|typescript):([A-Za-z0-9_.]+)/([A-Za-z0-9_./-]+)#([A-Z0-9_]+)$")
_TIER0: Final = re.compile(r"^tier0:(ARCHITECTURE|RULINGS)#\[(\d{2})\]-\[([A-Z0-9_]+)\]$")
_LAW_ANCHOR: Final = re.compile(r"^([A-Za-z0-9_./-]+\.md)#\[(\d{2})\]-\[([A-Z0-9_]+)\]$")
_DERIVED: Final = re.compile(r"^(proto|msgspec):([A-Za-z0-9_.]+)$")
_SYMBOL: Final = re.compile(r"^[A-Z][A-Za-z0-9]*$")
_COORDINATE: Final = re.compile(r"^[A-Za-z_][A-Za-z0-9_.]*$")
_DIFF_HEADER: Final = re.compile(r"^\+\+\+ (\S+)")
_BSR_MODULE: Final = re.compile(r"^buf\.build/[a-z0-9][a-z0-9._-]*/[a-z0-9][a-z0-9._-]*$")
_BSR_COMMIT: Final = re.compile(r"^[0-9a-f]{32}$")
_EVENT_TYPE: Final = re.compile(r"^rasm(?:\.[a-z][a-z0-9]*){3}$")
_DEFAULT_LABEL: Final = "main"
_BARE_ANCHOR_ROOTS: Final = ("tests/", f"{CORPUS}/")
_TS_PACKAGE: Final = f"{CORPUS}/package.json"
_TS_BUILD: Final = "tsc --build"
_TS_DIRECTORY: Final = CORPUS
_TS_EXPORT: Final = "./*"
_TS_SOURCE: Final = "./gen/typescript/*.ts"
_TS_TARGET: Final[Mapping[str, str]] = MappingProxyType({"types": "./dist/*.d.ts", "import": "./dist/*.js", "default": "./dist/*.js"})
_PY_PACKAGE: Final = "rasm.contracts"
_PY_PACKAGE_ROOT: Final = PurePosixPath(f"{CORPUS}/gen/python/rasm/contracts")
_PY_MARKER: Final = (_PY_PACKAGE_ROOT / "py.typed").as_posix()
_ROUTED: Final[Routed] = Routed(language=Language.PROTO, scope=Scope.CHANGED)
_HINTS: Final[tuple[tuple[str, str], ...]] = ((".venv/", "uv sync"), ("node_modules/", "pnpm install"))
_MACHINE_HINT: Final = f"the machine estate supplies protoc, the grpc plugins, and {JSONSCHEMA_PLUGIN}"
_CREDENTIAL_HINT: Final = "a non-empty BUF_TOKEN outranks ~/.netrc; clear the stale export, or seat the live one"

# --- [MODELS] ---------------------------------------------------------------------------

# --- [MANIFEST]


class _Record(msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True, rename="camel"):
    """Manifest record policy: frozen, keyword-only, camelCase wire, unknown fields refused."""


@runtime_checkable
class _ArrayValue(Protocol):
    """Array/scalar normalization surface used by the dynamically loaded h5py boundary."""

    def tolist(self) -> object: ...


@runtime_checkable
class _ScalarValue(Protocol):
    """Scalar normalization surface used by the dynamically loaded h5py boundary."""

    def item(self) -> object: ...


@runtime_checkable
class _TypedValue(Protocol):
    """NumPy scalar or array carrying the exact HDF wire dtype."""

    dtype: _HdfDtype


@runtime_checkable
class _HdfDtype(Protocol):
    """Dataset dtype fact read by the proof gate."""

    name: str
    descr: list[tuple[str, str]]
    metadata: Mapping[str, object] | None


@runtime_checkable
class _HdfAttributes(Protocol):
    """Exact attribute lookup surface read by the proof gate."""

    def __getitem__(self, name: str, /) -> object: ...

    def keys(self) -> Iterable[str]: ...


@runtime_checkable
class _HdfDataset(Protocol):
    """Narrow dataset surface needed to normalize the HDF proof shapes."""

    dtype: _HdfDtype
    shape: tuple[int, ...]
    chunks: tuple[int, ...] | None
    compression: str | None
    compression_opts: object
    shuffle: bool

    @property
    def attrs(self) -> _HdfAttributes: ...

    def __getitem__(self, key: tuple[()], /) -> object: ...


@runtime_checkable
class _HdfGroup(Protocol):
    """Narrow group/file surface needed to resolve named proof nodes."""

    @property
    def attrs(self) -> _HdfAttributes: ...

    def __getitem__(self, name: str, /) -> object: ...

    def keys(self) -> Iterable[str]: ...


class _HdfFile(_HdfGroup, Protocol):
    """Context-managed HDF root surface."""

    def __enter__(self) -> Self: ...

    def __exit__(self, exc_type: object, exc_value: object, traceback: object) -> bool | None: ...


@runtime_checkable
class _HdfModule(Protocol):
    """One dynamically loaded h5py constructor; no package-wide shadow stub."""

    def File(self, name: str, mode: Literal["r"]) -> _HdfFile: ...  # ruff:ignore[invalid-function-name]


@runtime_checkable
class _CoordinateArray(Protocol):
    """SciPy COO result surface needed for Matrix Market semantic facts."""

    shape: tuple[int, int]
    row: _ArrayValue
    col: _ArrayValue
    data: _ArrayValue


@runtime_checkable
class _MatrixMarketModule(Protocol):
    """Official SciPy Matrix Market decode/probe surface."""

    def mminfo(self, source: str) -> tuple[int, int, int, str, str, str]: ...

    def mmread(self, source: str, *, spmatrix: Literal[False]) -> object: ...


class TypeScriptJsonModule(_Record, frozen=True, kw_only=True, tag_field="kind", tag="typescript-json-module"):
    """One readonly TypeScript module generated from exact publisher JSON bytes."""

    path: str
    symbol: str


class PythonPackageResource(_Record, frozen=True, kw_only=True, tag_field="kind", tag="python-package-resource"):
    """One exact publisher asset installed below an importlib.resources package root."""

    path: str
    package: Literal["rasm.contracts"]


type Distribution = TypeScriptJsonModule | PythonPackageResource


class Fingerprint(_Record, frozen=True, kw_only=True):
    """One algorithm-tagged digest over the asset bytes exactly as stored."""

    algorithm: FingerprintAlgorithm
    value: str


class _Asset(_Record, frozen=True, kw_only=True, tag_field="role"):
    """Shared immutable evidence fields; the role tag closes specimen and expected-facts routing."""

    path: str
    bytes: Annotated[int, msgspec.Meta(ge=0)]
    fingerprint: Fingerprint


class SpecimenAsset(_Asset, frozen=True, kw_only=True, tag="specimen"):
    """Bytes decoded by the case definition or held exactly by its digest oracle."""

    minter: str = ""
    distributions: tuple[Distribution, ...] = ()


class ExpectedAsset(_Asset, frozen=True, kw_only=True, tag="expected"):
    """Typed normalized facts compared with one or more decoded specimens."""

    facts_format: FactsFormat


type Asset = SpecimenAsset | ExpectedAsset


class ProofVector(_Record, frozen=True, kw_only=True):
    """One explicit proof unit pairing its specimen assets with optional typed normalized facts."""

    specimens: tuple[SpecimenAsset, ...]
    expected: ExpectedAsset | None = None


class FieldCompressionFacts(_Record, frozen=True, kw_only=True):
    kind: Literal["gzip"]
    level: int
    shuffle: bool


class FieldRootFacts(_Record, frozen=True, kw_only=True):
    bits: int
    bound: float
    format_key: str
    max_residual: float
    residence: Literal["exact"]


class VirtualChunkFacts(_Record, frozen=True, kw_only=True):
    key: str
    offset: int
    length: int


class VirtualFieldFacts(_Record, frozen=True, kw_only=True):
    dims: tuple[str, str]
    shape: tuple[int, int]
    dtype: Literal["float32"]
    chunks: tuple[VirtualChunkFacts, ...]


class FieldFacts(_Record, frozen=True, kw_only=True, tag_field="kind", tag="field"):
    path: Literal["/field"]
    dtype: Literal["float32"]
    shape: tuple[int, int]
    chunks: tuple[int, int]
    compression: FieldCompressionFacts
    values: tuple[tuple[float, float], tuple[float, float]]
    root_attributes: FieldRootFacts
    virtual: VirtualFieldFacts


class CategoricalBandFacts(_Record, frozen=True, kw_only=True, tag_field="kind", tag="categorical"):
    categories: tuple[str, ...]
    mass: tuple[float, ...]


class NumericBandFacts(_Record, frozen=True, kw_only=True, tag_field="kind", tag="numeric"):
    edges: tuple[float, ...]
    mass: tuple[float, ...]


class GraduationFacts(_Record, frozen=True, kw_only=True, tag_field="kind", tag="graduation"):
    path: Literal["/bands"]
    evidence_key: Annotated[str, msgspec.Meta(pattern=r"^[0-9a-f]{32}$")]
    climate: CategoricalBandFacts
    temperature: NumericBandFacts


class SparseAttributesFacts(_Record, frozen=True, kw_only=True):
    fill: int
    format: Literal["csc"]
    frobenius: float
    kind: Literal["lu"]
    ordering: int
    shape: tuple[int, int]
    symmetric: bool


class SparseFacts(_Record, frozen=True, kw_only=True, tag_field="kind", tag="sparse"):
    path: Literal["/A"]
    attributes: SparseAttributesFacts
    indices: tuple[int, ...]
    indptr: tuple[int, ...]
    permutation: tuple[int, ...]
    values: tuple[float, ...]


class WaveformFacts(_Record, frozen=True, kw_only=True, tag_field="kind", tag="waveform"):
    path: Literal["/waveform"]
    dtype: Literal["float32"]
    shape: tuple[int, int]
    chunks: tuple[int, int]
    compression: FieldCompressionFacts
    sample_rate: float
    values: tuple[tuple[float, ...], ...]


type HdfFacts = FieldFacts | GraduationFacts | SparseFacts | WaveformFacts


class MatrixEntryFacts(_Record, frozen=True, kw_only=True):
    row: int
    column: int
    value: float


class MatrixMarketFacts(_Record, frozen=True, kw_only=True, tag_field="kind", tag="matrix-market"):
    shape: tuple[int, int]
    entries: tuple[MatrixEntryFacts, ...]


class ContentDigestFacts(_Record, frozen=True, kw_only=True, tag_field="kind", tag="content-digest"):
    algorithm: Literal["xxh128"]
    seed: Literal[0]
    value: Annotated[str, msgspec.Meta(pattern=r"^[0-9a-f]{32}$")]


class HlcValueFacts(_Record, frozen=True, kw_only=True, tag_field="kind", tag="hlc-value"):
    physical: Annotated[str, msgspec.Meta(pattern=r"^[0-9]+$")]
    logical: Annotated[str, msgspec.Meta(pattern=r"^[0-9]+$")]


class BackendGenerationFacts(_Record, frozen=True, kw_only=True, tag_field="kind", tag="backend-generation"):
    """Canonical Backend semantic keys and the exact estate writer preimage."""

    contract: str
    artifact_keys: tuple[str, ...]
    capability_keys: tuple[str, ...]
    preimage_bytes: int
    preimage_xxh128: Annotated[str, msgspec.Meta(pattern=r"^[0-9a-f]{32}$")]
    preimage_hex: Annotated[str, msgspec.Meta(pattern=r"^(?:[0-9a-f]{2})+$")]


type ExpectedFacts = BackendGenerationFacts | ContentDigestFacts | HdfFacts | HlcValueFacts | MatrixMarketFacts


class Support(_Record, frozen=True, kw_only=True):
    """One generated descriptor symbol used semantically beyond an actor's boundary-root closure."""

    kind: SupportKind
    fqn: str


class _Actor(_Record, frozen=True, kw_only=True, tag_field="direction"):
    """One exact branch boundary entrypoint bound to this case definition."""

    anchor: str
    coordinate: str
    binding: ActorBinding
    supports: tuple[Support, ...] = ()


class MessageActor(_Actor, frozen=True, kw_only=True, tag="message"):
    """A non-RPC producer, minter, or consumer of the case contract."""

    roots: tuple[str, ...] = ()


class ClientRequestActor(_Actor, frozen=True, kw_only=True, tag="client-request"):
    """A client request producer or server-side peer consumer at one exact RPC."""

    method: str


class ClientResponseActor(_Actor, frozen=True, kw_only=True, tag="client-response"):
    """A client response consumer or server-side peer producer at one exact RPC."""

    method: str


class ServerRequestActor(_Actor, frozen=True, kw_only=True, tag="server-request"):
    """A server request consumer or client-side peer producer at one exact RPC."""

    method: str


class ServerResponseActor(_Actor, frozen=True, kw_only=True, tag="server-response"):
    """A server response producer or client-side peer consumer at one exact RPC."""

    method: str


type Actor = MessageActor | ClientRequestActor | ClientResponseActor | ServerRequestActor | ServerResponseActor


class ProtoDefinition(_Record, frozen=True, kw_only=True, tag_field="kind", tag="proto"):
    """One exact message descriptor, the framing its bytes ride, and any service descriptors binding that message."""

    message: str
    framing: ProtoFraming


class CloudEventDefinition(_Record, frozen=True, kw_only=True, tag_field="kind", tag="cloudevent"):
    """One exact CloudEvents protobuf envelope and its application event-type discriminant."""

    message: Literal["io.cloudevents.v1.CloudEvent"]
    framing: Literal["proto-binary"]
    type: str


class SchemaDefinition(_Record, frozen=True, kw_only=True, tag_field="kind", tag="schema"):
    """A DERIVED JSON Schema 2020-12 file whose `$id` is its file name: `derived_from` spells `proto:<fqn>` or `msgspec:<dotted.path.Struct>`."""

    path: str
    framing: SchemaFraming
    derived_from: str


class PublisherLicense(_Record, frozen=True, kw_only=True):
    """The colocated license file covering one immutable publisher origin."""

    spdx: Literal["Apache-2.0"]
    path: str
    sha256: Annotated[str, msgspec.Meta(pattern=r"^[0-9a-f]{64}$")]


class PublisherOrigin(_Record, frozen=True, kw_only=True):
    """An immutable upstream repository coordinate for publisher bytes under local custody."""

    repository: str
    commit: Annotated[str, msgspec.Meta(pattern=r"^[0-9a-f]{40}$")]
    upstream_path: str
    license: PublisherLicense


class PublisherDefinition(_Record, frozen=True, kw_only=True, tag_field="kind", tag="publisher"):
    """A publisher definition with exact local custody and immutable upstream provenance."""

    format: str
    source: str
    origin: PublisherOrigin


class LawDefinition(_Record, frozen=True, kw_only=True, tag_field="kind", tag="law"):
    """A framing-law seam with one closed evidence medium, anchored as `path#[NN]-[CLUSTER]`."""

    anchor: str
    format: LawFormat


type Definition = CloudEventDefinition | ProtoDefinition | SchemaDefinition | PublisherDefinition | LawDefinition


class DomainAuthority(_Record, frozen=True, kw_only=True, tag_field="kind", tag="domain"):
    """One semantic producer owns the case value."""

    producer: Actor


class InfrastructureAuthority(_Record, frozen=True, kw_only=True, tag_field="kind", tag="infrastructure"):
    """Independent minters define a shared infrastructure contract."""

    minters: tuple[Actor, ...]


class PublisherAuthority(_Record, frozen=True, kw_only=True, tag_field="kind", tag="publisher"):
    """An immutable external publisher owns the definition bytes."""


class ApplicationAuthority(_Record, frozen=True, kw_only=True, tag_field="kind", tag="application"):
    """A deploying application or external client supplies a public inbound value the estate only reads."""


type Authority = ApplicationAuthority | DomainAuthority | InfrastructureAuthority | PublisherAuthority


class BlockedReadiness(_Record, frozen=True, kw_only=True, tag_field="kind", tag="blocked"):
    """A decision-complete contract lacking executable evidence."""

    blockers: tuple[str, ...]


class VerifiedReadiness(_Record, frozen=True, kw_only=True, tag_field="kind", tag="verified"):
    """A case discharged by one independently falsifiable oracle and its evidence."""

    oracle: Oracle
    vectors: tuple[ProofVector, ...]


type Readiness = BlockedReadiness | VerifiedReadiness


class Case(_Record, frozen=True, kw_only=True):
    """One atomic contract with tagged authority, tagged readiness, exact definition, and boundary actors."""

    id: str
    definition: Definition
    authority: Authority
    readiness: Readiness
    consumers: tuple[Actor, ...] = ()


class Entry(_Record, frozen=True, kw_only=True):
    """One decoder-partition grouping owner; every semantic fact belongs to one non-empty atomic case."""

    id: str
    law: str
    cases: tuple[Case, ...]


class Manifest(_Record, frozen=True, kw_only=True):
    """The corpus registry: one entry per seam."""

    entries: tuple[Entry, ...]


# --- [TEMPLATE]


class _Row(msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True):
    """Template record policy: every key the rail does not model is a decode fault, so a template feature lands as a field first."""


class _ManagedRow(_Row, frozen=True, kw_only=True):
    file_option: str = ""
    field_option: str = ""
    module: str = ""
    path: str = ""
    value: str | bool | int = ""


class _Managed(_Row, frozen=True, kw_only=True):
    enabled: bool = False
    disable: tuple[_ManagedRow, ...] = ()
    override: tuple[_ManagedRow, ...] = ()


class _Input(_Row, frozen=True, kw_only=True):
    directory: str = ""
    module: str = ""
    paths: tuple[str, ...] = ()
    exclude_paths: tuple[str, ...] = ()
    types: tuple[str, ...] = ()


class _Plugin(_Row, frozen=True, kw_only=True):
    out: str
    local: str | tuple[str, ...] = ""
    protoc_builtin: str = ""
    protoc_path: str | tuple[str, ...] = ""
    remote: str = ""
    revision: int | None = None
    opt: str | tuple[str, ...] = ()
    strategy: str = ""
    types: tuple[str, ...] = ()
    include_imports: bool = False
    include_wkt: bool = False

    @property
    def binary(self) -> str:
        """The executable the row drives: the `local` spelling, or protoc (the `protoc_path` head, PATH `protoc` when omitted) under a builtin.

        Returns:
            The binary spelling, or ``""`` for a remote plugin.
        """
        match (self.local, self.protoc_builtin, self.protoc_path):
            case (str(name), _, _) if name:
                return name
            case ((head, *_), _, _):
                return head
            case ("", builtin, str(path)) if builtin:
                return path or "protoc"
            case ("", builtin, (head, *_)) if builtin:
                return head
            case _:
                return ""

    @property
    def language(self) -> str:
        """The emission target the out root lands under: the first segment strictly inside the generated tree, else ``""``."""
        out = PurePosixPath(self.out)
        return out.relative_to(_EMISSION).parts[0] if out != _EMISSION and out.is_relative_to(_EMISSION) else ""

    @property
    def kinds(self) -> frozenset[_Kind]:
        """Descriptor kinds the selected emitter writes: descriptor carriers own message/enum/service, stub emitters own service/method."""
        spelling = self.local if isinstance(self.local, str) else (self.local[0] if self.local else "")
        name = PurePosixPath(spelling).name
        remote = re.fullmatch(r"buf\.build/([^/:]+)/([^/:]+):([^/:]+)", self.remote)
        identity = remote.group(1, 2) if remote is not None else ()
        if identity == ("protocolbuffers", "csharp"):
            return frozenset(("message", "enum"))
        if identity == ("grpc", "csharp"):
            return frozenset(("service", "method"))
        if name == "protoc-gen-es":
            return frozenset(("message", "enum", "service", "method"))
        if self.protoc_builtin in {"csharp", "python", "pyi"} or name == "protoc-gen-py":
            return frozenset(("message", "enum", "service"))
        if name in {"protoc-gen-connectrpc", "grpc_csharp_plugin", "grpc_python_plugin"}:
            return frozenset(("service", "method"))
        return frozenset()


class _BufGen(_Row, frozen=True, kw_only=True):
    """The estate `buf.gen.yaml` as the rail reads it: the clean sweep, managed mode, the workspace input, and every plugin row."""

    version: Literal["v2"]
    clean: bool = False
    managed: _Managed = msgspec.field(default_factory=_Managed)
    inputs: tuple[_Input, ...] = ()
    plugins: tuple[_Plugin, ...]


@dataclass(frozen=True, slots=True)
class _Templates:
    """The one clean generation template."""

    main: _BufGen

    @property
    def plugins(self) -> tuple[_Plugin, ...]:
        return self.main.plugins


class _BufConfig(msgspec.Struct, frozen=True, kw_only=True):
    """The estate ``buf.yaml`` module identity and dependency roster consumed by the corpus audit and publish custody."""

    version: Literal["v2"]
    modules: tuple["_BufModule", ...]
    deps: tuple[str, ...] = ()

    @property
    def module(self) -> str:
        """The validated estate module coordinate."""
        return next(row.name for row in self.modules if row.path == _ESTATE)


class _BufModule(msgspec.Struct, frozen=True, kw_only=True):
    path: str
    name: str = ""


class _Baseline(msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True):
    commit: str
    create_time: str


class _ModuleInfo(msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True):
    """BSR module identity returned by ``buf registry module info --format json``."""

    id: str
    remote: str
    owner: str
    name: str
    create_time: str
    state: Literal["MODULE_STATE_ACTIVE"]
    default_label_name: str


class _Identity(msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True):
    """The registry account the publication credential resolves to, from ``buf registry whoami --format json``."""

    username: str


# --- [GATES]


class _Finding(msgspec.Struct, frozen=True, gc=False):
    rule: str
    subject: str
    detail: str
    repair: str
    path: str = ""
    token: str = ""


class CorpusOracleFinding(msgspec.Struct, frozen=True, gc=False):
    """One exact specimen or oracle defect returned by the direct proof API."""

    rule: str
    detail: str
    path: str = ""


class CorpusOracleReceipt(msgspec.Struct, frozen=True, gc=False):
    """Typed direct verdict for one verified manifest case."""

    subject: str
    oracle: Oracle
    vectors: int
    specimens: int
    findings: tuple[CorpusOracleFinding, ...] = ()


class _Method(msgspec.Struct, frozen=True, gc=False):
    name: str
    input: str
    output: str
    client_streaming: bool = False
    server_streaming: bool = False


class _File(msgspec.Struct, frozen=True, gc=False):
    name: str
    package: str
    messages: tuple[str, ...] = ()
    enums: tuple[str, ...] = ()
    services: tuple[str, ...] = ()
    methods: tuple[_Method, ...] = ()
    dependencies: tuple[str, ...] = ()
    map_messages: tuple[str, ...] = ()
    message_links: tuple[tuple[str, tuple[str, ...]], ...] = ()


class _Probe(msgspec.Struct, frozen=True, gc=False, tag_field="gate", tag="probe"):
    plugins: tuple[tuple[str, str], ...] = ()
    misses: tuple[str, ...] = ()
    hint: str = ""
    projector: str = ""


class _Audit(msgspec.Struct, frozen=True, gc=False, tag_field="gate", tag="corpus"):
    findings: tuple[_Finding, ...] = ()
    files: tuple[_File, ...] = ()
    seams: tuple[str, ...] = ()
    anchors: tuple[tuple[str, str], ...] = ()
    entries: int = 0
    assets: int = 0


class _Freshness(msgspec.Struct, frozen=True, gc=False, tag_field="gate", tag="freshness"):
    stale: tuple[tuple[str, str], ...] = ()
    roots: tuple[str, ...] = ()
    diff: str = ""


type _Gate = _Probe | _Audit | _Freshness


@dataclass(frozen=True, slots=True)
class _Lane:
    """One contracts gate lane, its scratch output, and plugin dependency."""

    name: str
    mode: Mode
    output: str = ""
    gated: bool = False


@dataclass(frozen=True, slots=True)
class _Roster:
    """One emission target's roster: the censused descriptor kinds and the name speller; the `.api` catalog derives from the target."""

    language: str
    kinds: frozenset[_Kind]
    name: Callable[[_Kind, str], str]

    @property
    def catalog(self) -> str:
        return f"{CORPUS}/.api/{self.language}.md"


@dataclass(frozen=True, slots=True)
class _RosterSpec:
    """One plugin's exact Buf type-filtered descriptor image used to census its emitted closure."""

    language: str
    identity: str
    output: Path
    roots: tuple[str, ...]
    kinds: frozenset[_Kind]
    include_imports: bool
    include_wkt: bool


@dataclass(frozen=True, slots=True)
class _Corpus:
    """The audited corpus: roots, the decoded registry, the built descriptor files, the template, the resolved message names, and the projections."""

    repo: Path
    image: Path
    manifest: Manifest
    files: tuple[_File, ...]
    template: _Templates
    known: frozenset[str]
    services: frozenset[str]
    methods: Mapping[str, _Method]
    map_messages: frozenset[str]
    projections: _Projections = _NO_PROJECTIONS
    roster_files: _RosterFiles = _NO_ROSTER_FILES
    material: Path | None = None

    @property
    def root(self) -> Path:
        return self.repo / CORPUS

    @property
    def output(self) -> Path:
        return self.material or self.repo

    @property
    def schema_root(self) -> Path:
        return self.output / CORPUS

    def seam_dir(self, entry: Entry) -> Path:
        publisher = all(isinstance(case.authority, PublisherAuthority) for case in entry.cases)
        return self.root / _VENDOR / entry.id if publisher else self.root / _CONFORMANCE / entry.id


class _ProofContext(Protocol):
    """Corpus roots required by direct specimen and semantic oracle proof."""

    @property
    def root(self) -> Path: ...

    @property
    def schema_root(self) -> Path: ...

    @property
    def image(self) -> Path: ...


@dataclass(frozen=True, slots=True)
class _DirectProofContext:
    root: Path
    image: Path

    @property
    def schema_root(self) -> Path:
        return self.root


@dataclass(frozen=True, slots=True, kw_only=True)
class ContractsParams(BaseParams):
    """Parameters for the contracts verbs: no positional tokens and no flags, since the template and the corpus are the only inputs."""

    SLOTS: ClassVar[dict[str, str]] = {"": ""}

    @override
    def _arity(self, verb: str) -> int:
        _ = verb
        return 0


# --- [TABLES] ---------------------------------------------------------------------------

_ROWS: Final[dict[tuple[str, Mode], Tool]] = {(t.name, t.mode): t for t in select(Claim.CONTRACTS, Language.PROTO)}
_LANES: Final[tuple[_Lane, ...]] = (
    _Lane("buf-build", Mode.QUERY, output=_IMAGE),
    _Lane("buf-lint", Mode.CHECK),
    _Lane("buf-format", Mode.CHECK),
    _Lane("buf-generate", Mode.STAGE, output=_GEN, gated=True),
)
_GATE: Final[msgspec.json.Decoder[_Gate]] = msgspec.json.Decoder(_Gate)
_GATED: Final[frozenset[str]] = frozenset(("plugin-probe", "corpus-gate", "freshness-gate", "corpus-emit"))
_EMITTABLE: Final[frozenset[str]] = frozenset(("distribution-missing", "distribution-stale", "schema-missing", "schema-stale", "roster-stale"))
_ENCODER: Final[msgspec.json.Encoder] = msgspec.json.Encoder()
_MANIFEST: Final[msgspec.json.Decoder[Manifest]] = msgspec.json.Decoder(Manifest)
_EXPECTED_FACTS: Final[msgspec.json.Decoder[ExpectedFacts]] = msgspec.json.Decoder(ExpectedFacts)


def actor_key(actor: Actor) -> str:
    """Derive the stable provenance key for one exact manifest actor without introducing an alias registry.

    Returns:
        The actor's anchor and boundary coordinate as one unaliased key.
    """
    return f"{actor.anchor}@{actor.coordinate}"


def _parity_provenance_broken(case: Case) -> bool:
    if not (
        isinstance(case.authority, InfrastructureAuthority)
        and isinstance(case.readiness, VerifiedReadiness)
        and case.readiness.oracle == "value-parity"
    ):
        return False
    expected = frozenset(actor_key(actor) for actor in case.authority.minters)
    return any(
        len(vector.specimens) != len(expected)
        or len({specimen.path for specimen in vector.specimens}) != len(vector.specimens)
        or frozenset(specimen.minter for specimen in vector.specimens) != expected
        for vector in case.readiness.vectors
    )


_ENTRY_LAW: Final[tuple[_Clause[Entry], ...]] = (
    (lambda e: not e.cases, "cases-empty", "entry owns no atomic case", "land at least one exact decoder case, or delete the entry"),
    (lambda e: not e.law.strip(), "law-empty", "law sentence is empty", "state the one-sentence law"),
    (lambda e: len(e.law) > _LAW_CAP, "law-length", f"law exceeds {_LAW_CAP} characters", "move the detail to the owning page; keep one sentence"),
)
_CASE_LAW: Final[tuple[_Clause[Case], ...]] = (
    (
        lambda c: not isinstance(c.authority, PublisherAuthority) and not c.consumers,
        "authority-actors",
        "process contract names no consuming boundary",
        "bind the exact reader, or delete the registry case; a producer or descriptor alone does not establish a crossing",
    ),
    (
        lambda c: (
            isinstance(c.authority, ApplicationAuthority)
            and bool(c.consumers)
            and not any(actor.binding in {"generated", "package"} for actor in c.consumers)
        ),
        "authority-actors",
        "application authority names no executable wire reader",
        "bind the exact generated or package ingress; proof-only readers do not establish a public application boundary",
    ),
    (
        lambda c: (
            isinstance(c.authority, ApplicationAuthority) and not isinstance(c.definition, (CloudEventDefinition, ProtoDefinition, SchemaDefinition))
        ),
        "authority-definition",
        "application authority is attached to a non-inbound definition",
        "reserve application authority for a public typed application input; missing estate producers remain defects",
    ),
    (
        lambda c: isinstance(c.authority, InfrastructureAuthority) and len(c.authority.minters) < 2,
        "authority-actors",
        "infrastructure authority does not name two independent minters",
        "name every exact minter, or reclass the atomic case",
    ),
    (
        lambda c: isinstance(c.authority, PublisherAuthority) != isinstance(c.definition, PublisherDefinition),
        "authority-definition",
        "publisher definitions require publisher authority",
        "name the publisher source and format, or reclass the case",
    ),
    (
        lambda c: (
            isinstance(c.authority, PublisherAuthority)
            and (not isinstance(c.readiness, VerifiedReadiness) or c.readiness.oracle != "publisher-digest")
        ),
        "publisher-readiness",
        "publisher authority is not verified by publisher-digest",
        "freeze the publisher bytes and verify immutable custody",
    ),
    (
        lambda c: isinstance(c.readiness, VerifiedReadiness) and not c.readiness.vectors,
        "readiness-vectors",
        "verified readiness carries no proof vectors",
        "land exact evidence or mark the case blocked",
    ),
    (
        lambda c: isinstance(c.readiness, VerifiedReadiness) and any(not vector.specimens for vector in c.readiness.vectors),
        "proof-vector",
        "proof vector carries no specimen",
        "name the exact specimen assets the oracle evaluates",
    ),
    (
        lambda c: (
            isinstance(c.readiness, VerifiedReadiness)
            and c.readiness.oracle == "semantic-conformance"
            and any(len(vector.specimens) != 1 or vector.expected is None for vector in c.readiness.vectors)
        ),
        "proof-vector",
        "semantic-conformance vector is not one specimen paired with typed expected facts",
        "pair each producer specimen with one expected facts asset",
    ),
    (
        lambda c: (
            isinstance(c.readiness, VerifiedReadiness)
            and c.readiness.oracle == "value-parity"
            and any(vector.expected is None for vector in c.readiness.vectors)
        ),
        "proof-vector",
        "value-parity vector carries no normalized expected facts",
        "pair each specimen group with the one typed value all independent implementations must produce",
    ),
    (
        _parity_provenance_broken,
        "proof-provenance",
        "value-parity vectors do not carry exactly one specimen from every declared minter",
        "mint one independent specimen per actor and bind it with actor_key(actor)",
    ),
    (
        lambda c: (
            isinstance(c.readiness, VerifiedReadiness)
            and c.readiness.oracle != "value-parity"
            and any(specimen.minter for vector in c.readiness.vectors for specimen in vector.specimens)
        ),
        "proof-provenance",
        "non-parity specimens carry unrelated minter provenance",
        "reserve specimen minter keys for independent value-parity producers",
    ),
    (
        lambda c: (
            isinstance(c.readiness, VerifiedReadiness)
            and c.readiness.oracle in {"semantic-roundtrip", "external-digest", "publisher-digest"}
            and any(vector.expected is not None for vector in c.readiness.vectors)
        ),
        "proof-vector",
        "digest or semantic-roundtrip vector carries unrelated expected facts",
        "keep expected facts only where the oracle compares normalized values",
    ),
    (
        lambda c: isinstance(c.readiness, BlockedReadiness) and not c.readiness.blockers,
        "readiness-blockers",
        "blocked readiness names no blocker",
        "state what no source tree runs",
    ),
    (
        lambda c: (
            isinstance(c.readiness, VerifiedReadiness)
            and c.readiness.oracle == "publisher-digest"
            and not isinstance(c.authority, PublisherAuthority)
        ),
        "oracle-authority",
        "publisher-digest is attached to estate authority",
        "use publisher authority, or select the estate oracle that proves the case",
    ),
    (
        lambda c: (
            isinstance(c.readiness, VerifiedReadiness)
            and c.readiness.oracle == "semantic-roundtrip"
            and not isinstance(c.definition, (CloudEventDefinition, ProtoDefinition))
        ),
        "oracle-definition",
        "semantic-roundtrip is attached to a non-protobuf definition",
        "use semantic-roundtrip only for decoded protobuf value normalization",
    ),
    (
        lambda c: (
            isinstance(c.readiness, VerifiedReadiness) and c.readiness.oracle == "semantic-roundtrip" and not isinstance(c.authority, DomainAuthority)
        ),
        "oracle-authority",
        "semantic-roundtrip is not owned by one domain producer",
        "use semantic-roundtrip only for a single-producer protobuf boundary; compare independent minters through value-parity",
    ),
    (
        lambda c: (
            isinstance(c.readiness, VerifiedReadiness)
            and c.readiness.oracle == "external-digest"
            and isinstance(c.definition, (CloudEventDefinition, ProtoDefinition))
        ),
        "oracle-definition",
        "external-digest is attached to protobuf bytes",
        "prove protobuf by semantic roundtrip; reserve external-digest for byte-defined external formats",
    ),
    (
        lambda c: (
            isinstance(c.readiness, VerifiedReadiness)
            and c.readiness.oracle == "value-parity"
            and not isinstance(c.authority, InfrastructureAuthority)
        ),
        "oracle-authority",
        "value-parity is not owned by independent infrastructure minters",
        "name the independent minters, or select the producer-owned oracle",
    ),
)
_BLOCKER_LAW: Final[tuple[_Clause[str], ...]] = (
    (lambda s: len(s) < _BLOCKER_FLOOR, "blocker-short", f"blocker shorter than {_BLOCKER_FLOOR} characters", "name what no source tree runs"),
    (lambda s: _CLOSED.search(s) is not None, "blocker-closed", "blocker reads as discharged", "verified readiness replaces the blocker"),
)
# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [REGISTRY]


def load_manifest(root: Path) -> Result[Manifest, Fault]:
    """Decode ``root/manifest.json`` into the typed registry.

    Returns:
        The manifest; a ``FAILED`` fault naming a missing or undecodable file (a corpus defect), a ``FAULTED`` fault for an unreadable one.
    """
    path = root / MANIFEST
    try:
        raw = path.read_bytes()
    except FileNotFoundError:
        return Error(Fault((MANIFEST,), RailStatus.FAILED, f"{MANIFEST}: missing"))
    except OSError as exc:
        return Error(Fault((MANIFEST,), RailStatus.FAULTED, f"{MANIFEST}: {exc}"))
    try:
        return Ok(_MANIFEST.decode(raw))
    except msgspec.ValidationError as exc:
        return Error(Fault((MANIFEST,), RailStatus.FAILED, f"{MANIFEST}: {exc}"))
    except msgspec.DecodeError as exc:
        return Error(Fault((MANIFEST,), RailStatus.FAILED, f"{MANIFEST}: malformed JSON: {exc}"))


def _json_layout(value: object, depth: int = 0) -> str:
    indent = " " * (_JSON_INDENT * depth)
    if isinstance(value, dict):
        if not value:
            return "{}"
        items: list[tuple[str, object]] = []
        for key, item in value.items():
            if not isinstance(key, str):
                raise TypeError("JSON object key is not a string")
            items.append((key, item))
        rows = (
            f"{' ' * (_JSON_INDENT * (depth + 1))}{msgspec.json.encode(key).decode()}: {_json_layout(item, depth + 1)}"
            for key, item in sorted(items, key=itemgetter(0))
        )
        return "{\n" + ",\n".join(rows) + f"\n{indent}}}"
    if isinstance(value, list):
        if not value:
            return "[]"
        if all(not isinstance(item, (dict, list)) for item in value):
            compact = "[" + ", ".join(msgspec.json.encode(item).decode() for item in value) + "]"
            if len(indent) + len(compact) <= _JSON_WIDTH:
                return compact
        child = " " * (_JSON_INDENT * (depth + 1))
        return "[\n" + ",\n".join(f"{child}{_json_layout(item, depth + 1)}" for item in value) + f"\n{indent}]"
    return msgspec.json.encode(value).decode()


def derived_schema(struct: type[msgspec.Struct] = Manifest, *, identity: str = "") -> bytes:
    """Render a msgspec owner's JSON Schema 2020-12 in the one canonical byte form the gate checks files against.

    ``Manifest`` derives ``manifest.schema.json``; a `msgspec:` seam definition derives its file under the file name as ``$id``.

    Returns:
        Deterministically keyed, repository-formatted JSON bytes with a trailing newline.
    """
    document = {"$schema": _SCHEMA_ANCHOR, **({"$id": identity} if identity else {}), **msgspec.json.schema(struct)}
    return f"{_json_layout(document)}\n".encode()


def _admitted_templates(main: _BufGen) -> Result[_Templates, Fault]:
    if not main.clean:
        return Error(Fault((TEMPLATE,), RailStatus.FAULTED, f"{Step.PARSE}: template: {TEMPLATE} must clean"))
    unpinned = tuple(
        row.remote for row in main.plugins if row.remote and (":" not in row.remote.rsplit("/", 1)[-1] or row.revision is None or row.revision < 1)
    )
    misplaced = tuple(row.local for row in main.plugins if not row.remote and row.revision is not None)
    if unpinned or misplaced:
        reason = (
            f"remote plugins must pin version and positive revision: {', '.join(unpinned)}"
            if unpinned
            else "revision belongs only to a remote plugin"
        )
        return Error(Fault((TEMPLATE,), RailStatus.FAULTED, f"{Step.PARSE}: template: {reason}"))
    return Ok(_Templates(main=main))


def read_template(root: Path) -> Result[_Templates, Fault]:
    """Decode the one clean generation template.

    Returns:
        The template, or a ``FAULTED`` parse fault for a missing, unreadable, or unmodelled document.
    """
    try:
        main = msgspec.convert(YAML(typ="safe").load((root / TEMPLATE).read_text(encoding="utf-8")), _BufGen)
        return _admitted_templates(main)
    except OSError as exc:
        return Error(Fault((TEMPLATE,), RailStatus.FAULTED, f"{Step.PARSE}: template: {exc}"))
    except msgspec.ValidationError as exc:
        return Error(Fault((TEMPLATE,), RailStatus.FAULTED, f"{Step.PARSE}: template: {exc}"))


def _read_config(root: Path) -> Result[_BufConfig, Fault]:
    """Decode and admit the one named estate module beside unnamed publisher modules.

    Returns:
        The admitted Buf workspace config, or a parse fault naming the exact topology defect.
    """
    try:
        config = msgspec.convert(YAML(typ="safe").load((root / _CONFIG).read_text(encoding="utf-8")), _BufConfig)
    except OSError as exc:
        return Error(Fault((_CONFIG,), RailStatus.FAULTED, f"{Step.PARSE}: config: {exc}"))
    except msgspec.ValidationError as exc:
        return Error(Fault((_CONFIG,), RailStatus.FAULTED, f"{Step.PARSE}: config: {exc}"))
    estate = tuple(row for row in config.modules if row.path == _ESTATE)
    foreign = tuple(row for row in config.modules if row.path != _ESTATE and not PurePosixPath(row.path).is_relative_to(_VENDOR))
    named_vendor = tuple(row.path for row in config.modules if row.path != _ESTATE and row.name)
    reason = (
        f"{_CONFIG} must declare {_ESTATE} exactly once"
        if len(estate) != 1
        else f"{_ESTATE} must name one buf.build owner/module coordinate"
        if not _BSR_MODULE.fullmatch(estate[0].name)
        else f"non-estate modules must live under {_VENDOR}: {', '.join(row.path for row in foreign)}"
        if foreign
        else f"publisher modules must remain unnamed: {', '.join(named_vendor)}"
        if named_vendor
        else ""
    )
    return Error(Fault((_CONFIG,), RailStatus.FAULTED, f"{Step.PARSE}: config: {reason}")) if reason else Ok(config)


def out_dirs(template: _Templates | _BufGen) -> tuple[str, ...]:
    """Project the template's distinct outermost ``out`` roots in declaration order — a nested out rides its parent's walk.

    Returns:
        Repo-relative generated out roots, each holding generated files alone.
    """
    outs = tuple(dict.fromkeys(PurePosixPath(row.out).as_posix() for row in template.plugins))
    return tuple(out for out in outs if not any(other != out and PurePosixPath(out).is_relative_to(other) for other in outs))


def _covers(token: str, candidate: str) -> bool:
    prefix = token.removesuffix(".**")
    return candidate == prefix or candidate.startswith(f"{prefix}.")


def _actor_language(actor: Actor) -> str:
    found = _ANCHOR.match(actor.anchor)
    return found.group(1) if found is not None else ""


def _python_member(name: str) -> str:
    words = re.sub(r"(.)([A-Z][a-z]+)", r"\1_\2", name)
    return re.sub(r"([a-z0-9])([A-Z])", r"\1_\2", words).lower()


def _resolve(root: Path, spelling: str) -> str:
    if "/" in spelling:
        candidate = root / spelling
        return str(candidate) if candidate.is_file() and os.access(candidate, os.X_OK) else ""
    return shutil.which(spelling) or ""


def _hint(spelling: str) -> str:
    return next((hint for prefix, hint in _HINTS if spelling.startswith(prefix)), _MACHINE_HINT)


# --- [DESCRIPTORS]


def _dotted(prefix: str, rows: Iterable[DescriptorProto]) -> tuple[tuple[str, ...], tuple[str, ...]]:
    messages: list[str] = []
    enums: list[str] = []
    for row in rows:
        if row.options is not None and row.options.map_entry:
            continue
        name = f"{prefix}{row.name}"
        inner_messages, inner_enums = _dotted(f"{name}.", row.nested_type)
        messages.extend((name, *inner_messages))
        enums.extend((*(f"{name}.{enum.name}" for enum in row.enum_type), *inner_enums))
    return tuple(messages), tuple(enums)


def _known(files: tuple[_File, ...]) -> frozenset[str]:
    return frozenset(f"{file.package}.{name}" for file in files for name in file.messages)


def _services(files: tuple[_File, ...]) -> frozenset[str]:
    return frozenset(f"{file.package}.{name}" for file in files for name in file.services)


def _methods(files: tuple[_File, ...]) -> Mapping[str, _Method]:
    return MappingProxyType({method.name: method for file in files for method in file.methods})


def _message_rows(prefix: str, rows: Iterable[DescriptorProto]) -> tuple[tuple[str, DescriptorProto], ...]:
    found: list[tuple[str, DescriptorProto]] = []
    for row in rows:
        name = f"{prefix}{row.name}"
        found.append((name, row))
        found.extend(_message_rows(f"{name}.", row.nested_type))
    return tuple(found)


def _files(image: Path) -> tuple[_File, ...]:
    descriptors = tuple(FileDescriptorSet.from_binary(image.read_bytes()).file)
    all_messages = {f".{file.package}.{name}": row for file in descriptors for name, row in _message_rows("", file.message_type)}
    map_entries = frozenset(name for name, row in all_messages.items() if row.options is not None and row.options.map_entry)
    direct_maps = frozenset(
        name
        for name, row in all_messages.items()
        if name not in map_entries and any(field.type == FieldDescriptorProto.Type.MESSAGE and field.type_name in map_entries for field in row.field)
    )

    def message_targets(row: DescriptorProto) -> frozenset[str]:
        targets: set[str] = set()
        for field in row.field:
            if field.type != FieldDescriptorProto.Type.MESSAGE:
                continue
            target = all_messages.get(field.type_name)
            if target is not None and target.options is not None and target.options.map_entry:
                value = next((member for member in target.field if member.name == "value"), None)
                if value is not None and value.type == FieldDescriptorProto.Type.MESSAGE:
                    targets.add(value.type_name)
            else:
                targets.add(field.type_name)
        return frozenset(targets)

    links = {name: message_targets(row) for name, row in all_messages.items() if name not in map_entries}

    def bears_map(name: str, seen: frozenset[str] = frozenset()) -> bool:
        return name in direct_maps or any(target not in seen and bears_map(target, seen | {name}) for target in links.get(name, ()))

    map_bearing = frozenset(name for name in links if bears_map(name))
    rows: list[_File] = []
    for file in descriptors:
        messages, enums = _dotted("", file.message_type)
        methods = tuple(
            _Method(
                name=f"{file.package}.{service.name}.{method.name}",
                input=method.input_type.removeprefix("."),
                output=method.output_type.removeprefix("."),
                client_streaming=method.client_streaming,
                server_streaming=method.server_streaming,
            )
            for service in file.service
            for method in service.method
        )
        rows.append(
            _File(
                name=file.name,
                package=file.package,
                messages=messages,
                enums=(*enums, *(enum.name for enum in file.enum_type)),
                services=tuple(service.name for service in file.service),
                methods=methods,
                dependencies=tuple(file.dependency),
                map_messages=tuple(name for name in messages if f".{file.package}.{name}" in map_bearing),
                message_links=tuple(
                    (f"{file.package}.{name}", tuple(sorted(target.removeprefix(".") for target in links.get(f".{file.package}.{name}", ()))))
                    for name in messages
                ),
            )
        )
    return tuple(rows)


def _actor_roots(manifest: Manifest, language: str, kinds: frozenset[_Kind]) -> tuple[str, ...]:
    roots: list[str] = []
    for entry in manifest.entries:
        for case in entry.cases:
            actors = tuple(actor for actor in _actors(case) if actor.binding == "generated" and _actor_language(actor) == language)
            if not actors:
                continue
            if kinds & {"message", "enum"} and isinstance(case.definition, (CloudEventDefinition, ProtoDefinition)):
                roots.append(case.definition.message)
            elif kinds & {"message", "enum"}:
                roots.extend(root for actor in actors if isinstance(actor, MessageActor) for root in actor.roots)
                roots.extend(actor.method for actor in actors if not isinstance(actor, MessageActor))
            if "service" in kinds:
                roots.extend(actor.method for actor in actors if not isinstance(actor, MessageActor))
            roots.extend(
                support.fqn
                for actor in actors
                for support in actor.supports
                if (support.kind == "message" and "message" in kinds) or (support.kind in {"service", "method"} and "service" in kinds)
            )
    return tuple(dict.fromkeys(roots))


def _file_owns(file: _File, symbol: str) -> bool:
    return symbol in frozenset((
        *(f"{file.package}.{name}" for name in (*file.messages, *file.enums, *file.services)),
        *(method.name for method in file.methods),
    ))


def _emitted(file: _File, kinds: frozenset[_Kind]) -> _File:
    """Project one filtered descriptor file to the symbol kinds the plugin actually emits.

    Returns:
        The descriptor row with non-emitted symbol kinds removed.
    """
    return msgspec.structs.replace(
        file,
        messages=file.messages if "message" in kinds else (),
        enums=file.enums if "enum" in kinds else (),
        services=file.services if "service" in kinds else (),
        methods=file.methods if "method" in kinds else (),
    )


def _plugin_roots(corpus: _Corpus, plugin: _Plugin) -> tuple[str, ...]:
    return tuple(dict.fromkeys(_actor_roots(corpus.manifest, plugin.language, plugin.kinds)))


def _known_symbols(corpus: _Corpus) -> frozenset[str]:
    return frozenset((*corpus.known, *corpus.services, *corpus.methods, *(f"{file.package}.{name}" for file in corpus.files for name in file.enums)))


def _actor_needs(manifest: Manifest) -> tuple[tuple[str, _Kind, str], ...]:
    needs: list[tuple[str, _Kind, str]] = []
    for entry in manifest.entries:
        for case in entry.cases:
            for actor in _actors(case):
                language = _actor_language(actor)
                if actor.binding != "generated" or not language:
                    continue
                if isinstance(case.definition, (CloudEventDefinition, ProtoDefinition)):
                    needs.append((language, "message", case.definition.message))
                elif isinstance(actor, MessageActor):
                    needs.extend((language, "message", root) for root in actor.roots)
                if not isinstance(actor, MessageActor):
                    needs.append((language, "method", actor.method))
                needs.extend((language, support.kind, support.fqn) for support in actor.supports)
    return tuple(dict.fromkeys(needs))


# --- [ROSTERS]


def _ts_name(kind: _Kind, dotted: str) -> str:
    return dotted if kind in {"service", "method"} else f"{dotted.replace('.', '_')}Schema"


def _py_name(kind: _Kind, dotted: str) -> str:
    _ = kind
    return dotted


def _cs_name(kind: _Kind, dotted: str) -> str:
    return dotted if kind in {"service", "method"} else dotted.replace(".", ".Types.")


_ROSTERS: Final[dict[str, _Roster]] = {
    row.language: row
    for row in (
        _Roster("typescript", frozenset(("message", "enum", "service", "method")), _ts_name),
        _Roster("python", frozenset(("message", "enum", "service", "method")), _py_name),
        _Roster("dotnet", frozenset(("message", "enum", "service", "method")), _cs_name),
    )
}
_KINDS: Final[tuple[tuple[_Kind, Callable[[_File], tuple[str, ...]]], ...]] = (
    ("message", lambda file: file.messages),
    ("enum", lambda file: file.enums),
    ("service", lambda file: file.services),
    ("method", lambda file: tuple(method.name.removeprefix(f"{file.package}.") for method in file.methods)),
)


def _table(header: tuple[str, ...], rows: tuple[tuple[str, ...], ...]) -> str:
    cells = tuple((f"[{index:02d}]", *row) for index, row in enumerate(rows, start=1))
    widths = tuple(max(len(column[i]) for column in (header, *cells)) for i in range(len(header)))

    def line(row: tuple[str, ...], *, head: bool = False) -> str:
        return (
            "| "
            + " | ".join((value if head or i else f" {value}").ljust(width) for i, (value, width) in enumerate(zip(row, widths, strict=True)))
            + " |"
        )

    rule = "| " + " | ".join(f":{'-' * (width - 2)}:" if i == 0 else f":{'-' * (width - 1)}" for i, width in enumerate(widths)) + " |"
    return "\n".join((line(header, head=True), rule, *(line(row) for row in cells)))


def _distribution_language(distribution: Distribution) -> str:
    return "typescript" if isinstance(distribution, TypeScriptJsonModule) else "python"


def _catalog_distributions(manifest: Manifest, language: str) -> tuple[Distribution, ...]:
    return tuple(
        distribution
        for entry in manifest.entries
        for case in entry.cases
        for asset in _assets(case)
        if isinstance(asset, SpecimenAsset)
        for distribution in asset.distributions
        if _distribution_language(distribution) == language
    )


def _asset_table(distributions: tuple[Distribution, ...]) -> str:
    rows = tuple(
        (
            f"`{distribution.symbol if isinstance(distribution, TypeScriptJsonModule) else PurePosixPath(distribution.path).name}`",
            "readonly-json" if isinstance(distribution, TypeScriptJsonModule) else "package-resource",
            f"`{distribution.path}`",
        )
        for distribution in distributions
    )
    return "[ASSET_SCOPE]: exact publisher projections emitted by `assay contracts generate`; hand edits are overwritten\n\n" + _table(
        ("[INDEX]", "[NAME]", "[KIND]", "[PATH]"), rows
    )


def _roster_block(roster: _Roster, files: tuple[_File, ...], roots: tuple[str, ...], distributions: tuple[Distribution, ...] = ()) -> str:
    public = frozenset(roots)
    packages = tuple(
        dict.fromkeys(
            file.package for file in files if file.package != "google.protobuf" and (*file.messages, *file.enums, *file.services, *file.methods)
        )
    )
    tables = tuple(
        f"[ROSTER_SCOPE]: `{package}` — public roots and reachable support closure emitted by "
        "`assay contracts generate`; hand edits are overwritten\n\n"
        + _table(
            ("[INDEX]", "[NAME]", "[KIND]", "[ORIGIN]", "[SYMBOL]"),
            tuple(
                dict.fromkeys(
                    (f"`{roster.name(kind, dotted)}`", kind, "public-root" if fqn in public else "support-closure", f"`{dotted}`")
                    for file in files
                    if file.package == package
                    for kind, names in _KINDS
                    if kind in roster.kinds
                    for dotted in names(file)
                    for fqn in (f"{file.package}.{dotted}",)
                )
            ),
        )
        for package in packages
    )
    sections = (*tables, *((_asset_table(distributions),) if distributions else ()))
    return "\n\n".join(sections) + "\n\n" if sections else ""


def _roster_span(text: str) -> tuple[int, int] | None:
    begin, end = text.find(ROSTER_BEGIN), text.find(ROSTER_END)
    return (begin + len(ROSTER_BEGIN) + 1, end) if 0 <= begin < end else None


# --- [AUDIT]


def _finding(rule: str, subject: str, detail: str, repair: str, path: str = "", token: str = "") -> _Finding:
    return _Finding(rule=rule, subject=subject, detail=detail, repair=repair, path=path, token=token)


def _repeated[T: Hashable](names: tuple[T, ...]) -> tuple[T, ...]:
    return tuple(dict.fromkeys(name for index, name in enumerate(names) if name in names[:index]))


def _json(path: Path) -> Result[object, str]:
    try:
        return Ok(msgspec.json.decode(path.read_bytes()))
    except OSError as exc:
        return Error(f"unreadable: {exc}")
    except msgspec.DecodeError as exc:
        return Error(f"malformed JSON: {exc}")


def _document(path: Path) -> Result[_Document, str]:
    return _json(path).bind(lambda loaded: Ok(loaded) if isinstance(loaded, dict) else Error("not a JSON object"))


def _check_schema(schema: _Document) -> str:
    try:
        jsonschema.Draft202012Validator.check_schema(schema)
    except jsonschema.exceptions.SchemaError as exc:
        return str(exc)[:_TAIL]
    return ""


def _schema_errors(schema: _Document, instance: object) -> tuple[str, ...]:
    if invalid := _check_schema(schema):
        return (f"schema: {invalid}",)
    try:
        validator = jsonschema.Draft202012Validator(schema, format_checker=jsonschema.Draft202012Validator.FORMAT_CHECKER)
        return tuple(sorted(f"{error.json_path}: {error.message}"[:_TAIL] for error in validator.iter_errors(instance)))
    except Unresolvable as exc:
        return (f"$ref: {type(exc).__name__}"[:_TAIL],)


def _refs(node: object) -> tuple[str, ...]:
    match node:
        case dict() as mapping:
            return tuple(ref for key, value in mapping.items() for ref in ((value,) if key == "$ref" and isinstance(value, str) else _refs(value)))
        case list() as rows:
            return tuple(ref for row in rows for ref in _refs(row))
        case _:
            return ()


def _ref_errors(schema: _Document) -> tuple[str, ...]:
    base = "urn:assay:definition"
    resolver = Registry().with_resource(base, Resource.from_contents(schema, default_specification=DRAFT202012)).resolver(base_uri=base)
    errors: list[str] = []
    for ref in dict.fromkeys(_refs(schema)):
        try:
            _ = resolver.lookup(ref)
        except Unresolvable as exc:
            errors.append(f"$ref {ref!r}: {type(exc).__name__}"[:_TAIL])
    return tuple(errors)


def _struct(repo: Path, dotted: str) -> Result[type[msgspec.Struct], str]:
    module_name, _, attribute = dotted.rpartition(".")
    if not module_name:
        return Error(f"{dotted!r} names no module")
    try:
        module = import_module(module_name)
    except ImportError:
        candidates = (repo / Path(*module_name.split(".")).with_suffix(".py"), repo / Path(*module_name.split(".")) / "__init__.py")
        located = next((path for path in candidates if path.is_file()), None)
        spec = importlib.util.spec_from_file_location(module_name, located) if located is not None else None
        if spec is None or spec.loader is None:
            return Error(f"module {module_name!r} is not importable under the repo root")
        module = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(module)
    owner = getattr(module, attribute, None)
    return Ok(owner) if isinstance(owner, type) and issubclass(owner, msgspec.Struct) else Error(f"{dotted!r} is not a msgspec Struct")


def _arm(derived_from: str) -> tuple[str, str]:
    found = _DERIVED.match(derived_from)
    return (found.group(1), found.group(2)) if found is not None else ("", "")


def _derived(corpus: _Corpus, subject: str, definition: SchemaDefinition, path: str) -> Result[bytes, tuple[_Finding, ...]]:
    name = PurePosixPath(definition.path).name
    if not definition.derived_from:
        repair = "derive the file from its proto message or msgspec owner; a hand-written schema is the deleted form"
        return Error((_finding("schema-hand-authored", subject, f"{definition.path} names no derivedFrom", repair, path),))
    if definition.framing not in _JSON_FRAMINGS:
        detail = f"{definition.path} frames {definition.framing}; a JSON Schema evaluates a JSON document alone"
        return Error((_finding("schema-framing", subject, detail, "define a binary seam by its proto message, or frame the JSON crossing", path),))
    match _arm(definition.derived_from):
        case ("", _):
            outcome: Result[bytes, str] = Error(f"derivedFrom {definition.derived_from!r} is not proto:<fqn> or msgspec:<dotted.path.Struct>")
        case ("msgspec", dotted):
            outcome = _struct(corpus.repo, dotted).map(lambda owner: derived_schema(owner, identity=name))
        case (_, fqn) if corpus.files and fqn not in corpus.known:
            return Error(())
        case (_, fqn):
            outcome = corpus.projections.get(fqn, Error(f"{fqn}: no projection lane ran"))
    return outcome.map_error(lambda reason: (_finding("schema-derivation", subject, reason, "land the derivation, or re-point derivedFrom", path),))


def _derived_rows(corpus: _Corpus, subject: str, definition: SchemaDefinition, path: str) -> tuple[_Finding, ...]:
    match _derived(corpus, subject, definition, path):
        case Result(tag="ok", ok=derived):
            stale = (corpus.schema_root / definition.path).read_bytes() != derived
            return (
                (
                    _finding(
                        "schema-stale",
                        subject,
                        f"{definition.path} differs from its derivation",
                        "assay contracts generate, then commit the file",
                        path,
                    ),
                )
                if stale
                else ()
            )
        case Result(error=rows):
            return rows
        case never:  # pragma: no cover
            raise TypeError(never)


def _schema_findings(corpus: _Corpus, subject: str, definition: SchemaDefinition) -> tuple[_Finding, ...]:
    rel = definition.path
    path, expected = f"{CORPUS}/{rel}", PurePosixPath(rel).name
    match _document(corpus.schema_root / rel):
        case Result(tag="error", error=reason):
            return (_finding("definition-missing", subject, f"{rel}: {reason}", "assay contracts generate lands the derived document", path),)
        case Result(ok=schema):
            identity = str(schema.get("$id", ""))
            return (
                *_derived_rows(corpus, subject, definition, path),
                *(
                    (_finding("definition-id", subject, f"$id {identity!r} is not the file name {expected!r}", "regenerate the derived file", path),)
                    if identity != expected
                    else ()
                ),
                *((_finding("definition-invalid", subject, error, "repair the schema document", path),) if (error := _check_schema(schema)) else ()),
                *(_finding("definition-ref", subject, error, "land the referenced row, or re-point the $ref", path) for error in _ref_errors(schema)),
            )
        case never:  # pragma: no cover
            raise TypeError(never)


def _named(definition: SchemaDefinition) -> tuple[str, ...]:
    arm, owner = _arm(definition.derived_from)
    return (owner,) if arm == "proto" else ()


def _decoder(definition: Definition) -> str:
    match definition:
        case CloudEventDefinition(message=message):
            return f"proto:{message}"
        case ProtoDefinition(message=message):
            return f"proto:{message}"
        case SchemaDefinition(path=path):
            return f"schema:{path}"
        case PublisherDefinition(format=format_name):
            return f"publisher:{format_name}"
        case LawDefinition(anchor=anchor):
            return f"law:{anchor}"


def _entry_class(case: Case) -> EntryClass:
    match case.authority:
        case ApplicationAuthority():
            return "application"
        case DomainAuthority():
            return "domain"
        case InfrastructureAuthority():
            return "infrastructure"
        case PublisherAuthority():
            return "publisher"


def _assets(case: Case) -> tuple[Asset, ...]:
    return (
        tuple(
            asset for vector in case.readiness.vectors for asset in (*vector.specimens, *((vector.expected,) if vector.expected is not None else ()))
        )
        if isinstance(case.readiness, VerifiedReadiness)
        else ()
    )


def _blockers(case: Case) -> tuple[str, ...]:
    return case.readiness.blockers if isinstance(case.readiness, BlockedReadiness) else ()


def _producer(case: Case) -> Actor | None:
    return case.authority.producer if isinstance(case.authority, DomainAuthority) else None


def _minters(case: Case) -> tuple[Actor, ...]:
    return case.authority.minters if isinstance(case.authority, InfrastructureAuthority) else ()


def _actors(case: Case) -> tuple[Actor, ...]:
    producer = _producer(case)
    return (*((producer,) if producer is not None else ()), *_minters(case), *case.consumers)


def _direction(actor: Actor) -> ActorDirection:
    match actor:
        case MessageActor():
            return "message"
        case ClientRequestActor():
            return "client-request"
        case ClientResponseActor():
            return "client-response"
        case ServerRequestActor():
            return "server-request"
        case ServerResponseActor():
            return "server-response"


def _path_reason(value: str) -> str:
    path = PurePosixPath(value)
    if not value or path.is_absolute():
        return "path is empty or absolute"
    if value != path.as_posix() or any(part in {"", ".", ".."} for part in path.parts):
        return "path is not normalized repo-relative POSIX"
    return ""


def _inside(value: str, root: PurePosixPath) -> bool:
    return not _path_reason(value) and PurePosixPath(value).is_relative_to(root)


def _unresolved(corpus: _Corpus, subject: str, messages: tuple[str, ...]) -> tuple[_Finding, ...]:
    return tuple(
        _finding("message-unresolved", subject, f"{name} is not in the built descriptor set", "name a message the corpus declares, or land the proto")
        for name in messages
        if corpus.files and name not in corpus.known
    )


def _anchor(repo: Path, text: str, *, paths: bool) -> str:
    match (_ANCHOR.match(text), _TIER0.match(text)):
        case (re.Match() as found, _):
            language, package, page, cluster = found.groups()
            return _cluster(repo / "libs" / language / package / ".planning" / f"{page}.md", rf"\[\d{{2}}\]-\[{re.escape(cluster)}\]")
        case (_, re.Match() as found):
            name, index, token = found.groups()
            return _cluster(repo / "libs" / ".planning" / f"{name}.md", rf"\[{index}\]-\[{re.escape(token)}\]")
        case _ if paths and text.startswith(_BARE_ANCHOR_ROOTS) and not text.startswith("/") and (repo / text).is_file():
            return ""
        case _:
            return "anchor grammar is lang:pkg/page#CLUSTER, tier0:ARCHITECTURE#[NN]-[TOKEN], tier0:RULINGS#[NN]-[TOKEN]" + (
                ", or a repo-relative path" if paths else ""
            )


def _cluster(page: Path, header: str) -> str:
    try:
        text = page.read_text(encoding="utf-8")
    except OSError:
        return f"page {page.as_posix()} is absent"
    return "" if re.search(rf"^## {header}\s*$", text, re.MULTILINE) else f"page {page.name} carries no ## {header} cluster"


def _anchor_body(repo: Path, anchor: str) -> Result[str, str]:
    """Read the exact cluster or proof file an actor names, without widening to the rest of its page.

    Returns:
        The cluster body, or the exact path/grammar refusal.
    """
    actor_found, tier_found = _ANCHOR.match(anchor), _TIER0.match(anchor)
    if actor_found is not None:
        language, package, page, cluster = actor_found.groups()
        path = repo / "libs" / language / package / ".planning" / f"{page}.md"
        header = rf"^## \[\d{{2}}\]-\[{re.escape(cluster)}\]\s*$"
    elif tier_found is not None:
        name, index, token = tier_found.groups()
        path = repo / "libs" / ".planning" / f"{name}.md"
        header = rf"^## \[{index}\]-\[{re.escape(token)}\]\s*$"
    elif anchor.startswith(_BARE_ANCHOR_ROOTS) and not anchor.startswith("/"):
        path, header = repo / anchor, ""
    else:
        return Error("anchor grammar is invalid")
    try:
        text = path.read_text(encoding="utf-8")
    except OSError as exc:
        return Error(str(exc))
    if not header:
        return Ok(text)
    header_found = re.search(header, text, re.MULTILINE)
    if header_found is None:
        return Error("cluster is absent")
    tail = text[header_found.end() :]
    end = re.search(r"^## ", tail, re.MULTILINE)
    return Ok(tail[: end.start()] if end is not None else tail)


def _rule_schema(corpus: _Corpus) -> Iterable[_Finding]:
    path = corpus.schema_root / SCHEMA
    repair = "write derived_schema() to manifest.schema.json; the msgspec model is the one authority"
    if not path.is_file():
        return (_finding("schema-missing", SCHEMA, "manifest.schema.json is absent", repair, f"{CORPUS}/{SCHEMA}"),)
    return (
        ()
        if path.read_bytes() == derived_schema()
        else (_finding("schema-stale", SCHEMA, "manifest.schema.json differs from the derived schema", repair, f"{CORPUS}/{SCHEMA}"),)
    )


def _rule_identity(corpus: _Corpus) -> Iterable[_Finding]:
    entries = corpus.manifest.entries
    yield from (
        _finding("id-duplicate", name, "entry id registered more than once", "merge the entries; one entry per seam")
        for name in _repeated(tuple(e.id for e in entries))
    )
    yield from (
        _finding("seam-duplicate", seam, "seam registered by more than one entry", "merge into one grouping owner")
        for seam in _repeated(tuple(e.id.lower().replace("_", "-") for e in entries))
    )
    yield from (
        _finding(
            "case-duplicate",
            f"{entry.id}/{name}",
            "case id registered more than once under the entry",
            "merge the cases; one case per discriminant value",
        )
        for entry in entries
        for name in _repeated(tuple(case.id for case in entry.cases))
    )


def _rule_entry(corpus: _Corpus) -> Iterable[_Finding]:
    for entry in corpus.manifest.entries:
        yield from (_finding(rule, entry.id, detail, repair) for broken, rule, detail, repair in _ENTRY_LAW if broken(entry))
        for case in entry.cases:
            subject = f"{entry.id}/{case.id}"
            yield from (_finding(rule, subject, detail, repair) for broken, rule, detail, repair in _CASE_LAW if broken(case))
            actors = _actors(case)
            yield from (
                _finding("actor-duplicate", subject, f"{role} actor repeated: {key}", "bind each exact boundary entrypoint once per role")
                for role, rows in (("minter", _minters(case)), ("consumer", case.consumers))
                for key in _repeated(tuple(actor_key(actor) for actor in rows))
            )
            message_actors = tuple(actor for actor in actors if isinstance(actor, MessageActor))
            publisher_proto = isinstance(case.definition, PublisherDefinition) and PurePosixPath(case.definition.source).suffix == ".proto"
            yield from (
                _finding(
                    "actor-roots",
                    subject,
                    f"{actor.anchor} repeats descriptor roots {repeated!r}",
                    "name each exact publisher descriptor root once",
                    token=actor.anchor,
                )
                for actor in message_actors
                if (repeated := _repeated(actor.roots))
            )
            yield from (
                _finding(
                    "actor-roots",
                    subject,
                    f"{actor.anchor} carries descriptor roots outside a publisher protobuf case",
                    "remove the redundant roots; proto cases derive their one message from the definition",
                    token=actor.anchor,
                )
                for actor in message_actors
                if actor.roots and not publisher_proto
            )
            yield from (
                _finding(
                    "actor-roots",
                    subject,
                    f"{actor.anchor} names no exact publisher descriptor root",
                    "name every publisher message this generated or package binding consumes directly",
                    token=actor.anchor,
                )
                for actor in message_actors
                if publisher_proto and actor.binding in {"generated", "package"} and not actor.roots
            )
        sentences = tuple((f"{entry.id}/{case.id}", sentence) for case in entry.cases for sentence in _blockers(case))
        yield from (
            _finding(rule, subject, f"{detail}: {sentence!r}", repair)
            for subject, sentence in sentences
            for broken, rule, detail, repair in _BLOCKER_LAW
            if broken(sentence)
        )


def _rule_definition(corpus: _Corpus) -> Iterable[_Finding]:
    for entry in corpus.manifest.entries:
        for case in entry.cases:
            subject, definition = f"{entry.id}/{case.id}", case.definition
            match definition:
                case CloudEventDefinition(message=message, type=event_type):
                    if not _EVENT_TYPE.fullmatch(event_type):
                        yield _finding(
                            "event-type",
                            subject,
                            f"CloudEvents type {event_type!r} is not an exact application discriminant",
                            "name the exact producer type as rasm.<domain>.<subject>.<fact>",
                        )
                    yield from _unresolved(corpus, subject, (message,))
                case ProtoDefinition(message=message):
                    if not message.strip():
                        yield _finding("message-empty", subject, "proto definition names no message", "name one exact message FQN")
                    yield from _unresolved(corpus, subject, (message,))
                case SchemaDefinition() as schema:
                    yield from _unresolved(corpus, subject, _named(schema))
                    yield from _schema_findings(corpus, subject, schema)
                case PublisherDefinition(format=format_name, source=source, origin=origin):
                    if not format_name.strip():
                        yield _finding("publisher-format", subject, "publisher definition names no format", "name the publisher format")
                    if reason := _path_reason(source):
                        yield _finding("definition-path", subject, f"{source!r}: {reason}", "name the normalized corpus-relative publisher source")
                    elif not (corpus.root / source).exists():
                        yield _finding(
                            "definition-missing", subject, f"{source} is absent", "vendor the publisher source byte-identically", f"{CORPUS}/{source}"
                        )
                    yield from _unresolved(
                        corpus, subject, tuple(root for actor in _actors(case) if isinstance(actor, MessageActor) for root in actor.roots)
                    )
                    if not origin.repository.startswith("https://"):
                        yield _finding(
                            "publisher-origin",
                            subject,
                            f"publisher repository {origin.repository!r} is not an immutable HTTPS origin",
                            "name the canonical upstream HTTPS repository",
                        )
                    if reason := _path_reason(origin.upstream_path):
                        yield _finding(
                            "publisher-origin", subject, f"upstream path {origin.upstream_path!r}: {reason}", "name the normalized upstream path"
                        )
                case LawDefinition(anchor=anchor):
                    match _LAW_ANCHOR.match(anchor):
                        case None:
                            yield _finding(
                                "definition-anchor",
                                subject,
                                f"law anchor {anchor!r} is not path#[NN]-[CLUSTER]",
                                "spell the repo-relative page and its cluster header",
                            )
                        case found if reason := _cluster(corpus.repo / found.group(1), rf"\[{found.group(2)}\]-\[{re.escape(found.group(3))}\]"):
                            yield _finding(
                                "definition-anchor",
                                subject,
                                f"law anchor {anchor}: {reason}",
                                "land the cluster at the page, or re-point the anchor",
                                found.group(1),
                            )
                        case _:
                            pass


def _message_closure(corpus: _Corpus, roots: Iterable[str]) -> frozenset[str]:
    links = {name: targets for file in corpus.files for name, targets in file.message_links}
    pending = list(roots)
    found: set[str] = set()
    while pending:
        name = pending.pop()
        if name in found:
            continue
        found.add(name)
        pending.extend(target for target in links.get(name, ()) if target not in found)
    return frozenset(found)


def _support_findings(corpus: _Corpus, subject: str, case: Case, actor: Actor, symbol_kind: Mapping[str, SupportKind]) -> Iterator[_Finding]:
    """Audit one actor's exceptional generated supports.

    Yields:
        Exact context, duplicate, descriptor-kind, resolution, and closure-redundancy defects.
    """
    if actor.binding != "generated" or not isinstance(case.definition, ProtoDefinition):
        yield _finding(
            "support-context",
            subject,
            f"{actor.anchor} attaches descriptor support outside a generated protobuf actor",
            "attach support only to the exact generated protobuf actor that semantically uses it",
            token=actor.anchor,
        )
    yield from (
        _finding(
            "support-duplicate",
            subject,
            f"{actor.anchor} repeats {kind} support {fqn}",
            "name each exact semantic support root once on its actor",
            token=actor.anchor,
        )
        for kind, fqn in _repeated(tuple((support.kind, support.fqn) for support in actor.supports))
    )
    method = corpus.methods.get(actor.method) if not isinstance(actor, MessageActor) else None
    message_roots = (
        (case.definition.message, *((method.input, method.output) if method is not None else ()))
        if isinstance(case.definition, ProtoDefinition)
        else ()
    )
    message_closure = _message_closure(corpus, message_roots)
    boundary = set(message_roots)
    if not isinstance(actor, MessageActor):
        boundary.update((actor.method, actor.method.rpartition(".")[0]))
    for support in actor.supports:
        found = symbol_kind.get(support.fqn)
        if corpus.files and found is None:
            yield _finding(
                "support-unresolved",
                subject,
                f"{actor.anchor} support {support.fqn} is absent from the built descriptor set",
                "name one exact generated message, service, or method FQN",
                token=actor.anchor,
            )
        elif found is not None and found != support.kind:
            yield _finding(
                "support-kind",
                subject,
                f"{actor.anchor} declares {support.fqn} as {support.kind}, but the descriptor declares {found}",
                "use the descriptor symbol's exact closed kind",
                token=actor.anchor,
            )
        if support.fqn in boundary or (support.kind == "message" and support.fqn in message_closure):
            yield _finding(
                "support-redundant",
                subject,
                f"{actor.anchor} support {support.fqn} is already reachable from its boundary root",
                "remove the support; Buf owns recursive descriptor closure",
                token=actor.anchor,
            )


def _rule_supports(corpus: _Corpus) -> Iterable[_Finding]:
    """Prove actor-scoped generated support roots are exact, kinded, and outside the boundary closure.

    Yields:
        Support defects from every actor that declares an exceptional descriptor root.
    """

    def symbols(names: Iterable[str], kind: SupportKind) -> dict[str, SupportKind]:
        return dict.fromkeys(names, kind)

    symbol_kind = {**symbols(corpus.known, "message"), **symbols(corpus.services, "service"), **symbols(corpus.methods, "method")}
    for entry in corpus.manifest.entries:
        for case in entry.cases:
            for actor in _actors(case):
                if actor.supports:
                    yield from _support_findings(corpus, f"{entry.id}/{case.id}", case, actor, symbol_kind)


def _actor_direction(subject: str, role: Literal["producer", "minter", "consumer"], actor: Actor) -> _Finding | None:
    producing = (ClientRequestActor, ServerResponseActor)
    consuming = (ServerRequestActor, ClientResponseActor)
    invalid = isinstance(actor, producing if role == "consumer" else consuming)
    if not invalid:
        return None
    expected = "server-request or client-response" if role == "consumer" else "client-request or server-response"
    return _finding(
        "actor-direction",
        subject,
        f"{role} {actor.anchor} uses {_direction(actor)} direction",
        f"use {expected} for an RPC {role}",
        token=actor.anchor,
    )


def _method_finding(corpus: _Corpus, subject: str, case: Case, actor: Actor) -> _Finding | None:
    if isinstance(actor, MessageActor):
        return None
    method = corpus.methods.get(actor.method)
    if method is None:
        return (
            _finding(
                "method-unresolved",
                subject,
                f"{actor.method} is not in the built descriptor set",
                "name one exact service method FQN",
                token=actor.anchor,
            )
            if corpus.files
            else None
        )
    if not isinstance(case.definition, ProtoDefinition):
        return _finding(
            "actor-direction",
            subject,
            f"RPC actor {actor.anchor} binds a non-protobuf definition",
            "split the direction into a case owning its exact protobuf message FQN",
            token=actor.anchor,
        )
    expected = method.input if isinstance(actor, (ClientRequestActor, ServerRequestActor)) else method.output
    return (
        None
        if case.definition.message == expected
        else _finding(
            "actor-direction",
            subject,
            f"{actor.method} {_direction(actor)} carries {expected}, not {case.definition.message}",
            "bind the actor to the direction-atomic request or response case",
            token=actor.anchor,
        )
    )


def _rule_actor_methods(corpus: _Corpus) -> Iterable[_Finding]:
    cases = tuple((f"{entry.id}/{case.id}", case) for entry in corpus.manifest.entries for case in entry.cases)
    for subject, case in cases:
        producer = _producer(case)
        actors = (
            *(((("producer", producer),)) if producer is not None else ()),
            *(("minter", actor) for actor in _minters(case)),
            *(("consumer", actor) for actor in case.consumers),
        )
        yield from (finding for role, actor in actors if (finding := _actor_direction(subject, role, actor)) is not None)
        yield from (finding for actor in _actors(case) if (finding := _method_finding(corpus, subject, case, actor)) is not None)


def _validated(schema: Result[_Document, str], path: Path) -> tuple[str, ...]:
    match (schema, _json(path)):
        case (Result(tag="ok", ok=document), Result(tag="ok", ok=instance)):
            return _schema_errors(document, instance)
        case (Result(tag="ok"), Result(error=reason)):
            return (reason,)
        case _:
            return ()


def _distribution_bytes(repo: Path, asset: Asset, distribution: Distribution) -> Result[bytes, str]:
    """Render one package-native projection from exact publisher bytes.

    Returns:
        Generated module or resource bytes, or the exact source/shape defect preventing the projection.
    """
    try:
        raw = (repo / CORPUS / asset.path).read_bytes()
    except OSError as exc:
        return Error(str(exc))
    if isinstance(distribution, PythonPackageResource):
        return Ok(raw)
    try:
        document = msgspec.json.decode(raw)
    except msgspec.DecodeError as exc:
        return Error(f"publisher asset is not JSON: {exc}")
    if not isinstance(document, dict):
        return Error("publisher JSON root is not an object")
    return Ok(
        b"// @generated by assay contracts generate from the frozen publisher asset; do not edit.\n\n"
        + f"const {distribution.symbol} = (\n".encode()
        + raw
        + b") as const;\n\n// --- [EXPORTS] --------------------------------------------------------------------------\n\n"
        + f"export {{ {distribution.symbol} }};\n".encode()
    )


def _distributions(manifest: Manifest) -> tuple[_DistributionRow, ...]:
    return tuple(
        (f"{entry.id}/{case.id}", _entry_class(case), asset, distribution)
        for entry in manifest.entries
        for case in entry.cases
        for asset in _assets(case)
        if isinstance(asset, SpecimenAsset)
        for distribution in asset.distributions
    )


def _distribution_path_finding(corpus: _Corpus, subject: str, distribution: Distribution) -> _Finding | None:
    where = distribution.path
    if reason := _path_reason(where):
        return _finding("distribution-path", subject, f"{where!r}: {reason}", "name one normalized repo-relative generated-package path", where)
    if isinstance(distribution, PythonPackageResource):
        if PurePosixPath(where).suffix == ".avsc" and _inside(where, _PY_PACKAGE_ROOT):
            return None
        return _finding(
            "distribution-path",
            subject,
            f"{where} is not an .avsc below the {_PY_PACKAGE} package root",
            f"seat the exact resource below {_PY_PACKAGE_ROOT.as_posix()}",
            where,
        )
    roots = tuple(PurePosixPath(row.out) for row in corpus.template.plugins if row.language == "typescript")
    if PurePosixPath(where).suffix == ".ts" and any(_inside(where, root) for root in roots):
        return None
    return _finding(
        "distribution-path",
        subject,
        f"{where} is outside a configured TypeScript out root or is not a .ts module",
        "seat the module under the configured TypeScript generated out root",
        where,
    )


def _distribution_findings(corpus: _Corpus) -> Iterable[_Finding]:
    distributions = _distributions(corpus.manifest)
    if not distributions:
        return
    if any(isinstance(distribution, TypeScriptJsonModule) for _, _, _, distribution in distributions):
        package = _json(corpus.repo / _TS_PACKAGE)
        private_ok = True
        match package:
            case Result(tag="ok", ok={"private": True}):
                private_ok = False
            case _:
                pass
        export_ok = False
        match package:
            case Result(
                tag="ok",
                ok={
                    "name": "@rasm/contracts",
                    "version": str() as version,
                    "license": "MIT",
                    "sideEffects": False,
                    "type": "module",
                    "files": list() as files,
                    "exports": dict() as exports,
                    "publishConfig": {"exports": dict() as published},
                    "scripts": dict() as scripts,
                    "dependencies": dict() as dependencies,
                    "repository": {"directory": str() as directory},
                },
            ):
                export_ok = (
                    bool(version)
                    and private_ok
                    and all(isinstance(item, str) for item in files)
                    and frozenset(("dist", "README.md")).issubset(files)
                    and all(item == "dist" or (corpus.repo / _TS_PACKAGE).parent.joinpath(item).exists() for item in files)
                    and exports.get(_TS_EXPORT) == _TS_SOURCE
                    and published.get(_TS_EXPORT) == _TS_TARGET
                    and scripts.get("build") == _TS_BUILD
                    and directory == _TS_DIRECTORY
                    and scripts.get("prepack") == "pnpm run build"
                    and dependencies.get("@bufbuild/protobuf") == "catalog:"
                )
            case _:
                pass
        if not export_ok:
            yield _finding(
                "distribution-export",
                _TS_PACKAGE,
                f"{_TS_EXPORT} does not resolve {_TS_SOURCE} in the workspace and publish {_TS_TARGET}",
                "restore the standalone contracts package metadata before publishing generated assets",
                _TS_PACKAGE,
            )
    for subject, entry_class, asset, distribution in distributions:
        where = distribution.path
        if entry_class != "publisher" or not isinstance(asset, SpecimenAsset):
            yield _finding(
                "distribution-authority",
                subject,
                f"{asset.path} is not publisher-owned evidence",
                "distribute only an exact publisher asset; estate definitions generate through their descriptor",
                where,
            )
        if finding := _distribution_path_finding(corpus, subject, distribution):
            yield finding
            continue
        if isinstance(distribution, TypeScriptJsonModule) and _SYMBOL.fullmatch(distribution.symbol) is None:
            yield _finding(
                "distribution-symbol", subject, f"{distribution.symbol!r} is not a PascalCase export", "name one PascalCase package export", where
            )
            continue
        rendered = _distribution_bytes(corpus.repo, asset, distribution)
        if rendered.is_error():
            yield _finding(
                "distribution-source",
                subject,
                f"{asset.path}: {rendered.error}",
                "restore the exact publisher asset before generating its package projection",
                f"{CORPUS}/{asset.path}",
            )
            continue
        target = corpus.output / where
        if not target.is_file():
            yield _finding(
                "distribution-missing", subject, f"{where} is absent", "run assay contracts generate and commit the generated projection", where
            )
        elif target.read_bytes() != rendered.ok:
            yield _finding(
                "distribution-stale",
                subject,
                f"{where} differs from {CORPUS}/{asset.path}",
                "run assay contracts generate and commit the generated projection",
                where,
            )
    yield from (
        _finding(
            "distribution-owned-twice",
            path,
            f"package target is owned by {', '.join(subjects)}",
            "assign the generated package target to exactly one frozen asset",
            path,
        )
        for path in _repeated(tuple(distribution.path for _, _, _, distribution in distributions))
        for subjects in (tuple(subject for subject, _, _, distribution in distributions if distribution.path == path),)
    )


def _pool(image: Path) -> Result[GoogleDescriptorPool, str]:
    try:
        image_message = GoogleFileDescriptorSet.FromString(image.read_bytes())
    except (OSError, DecodeError) as exc:
        return Error(str(exc))
    pool = GoogleDescriptorPool()
    pending = tuple(image_message.file)
    while pending:
        pending_names = frozenset(file.name for file in pending)
        ready = tuple(file for file in pending if all(dependency not in pending_names for dependency in file.dependency))
        if not ready:
            blocked = ", ".join(file.name for file in pending[:4])
            return Error(f"descriptor closure is incomplete at {blocked}")
        for file in ready:
            try:
                pool.AddSerializedFile(file.SerializeToString())
            except TypeError as exc:
                return Error(f"{file.name}: {exc}")
        ready_names = frozenset(file.name for file in ready)
        pending = tuple(file for file in pending if file.name not in ready_names)
    return Ok(pool)


def _semantic_roundtrip(image: Path, message_fqn: str, raw: bytes, *, event_type: str = "") -> Result[tuple[str, ...], str]:
    """Decode, normalize, re-encode, and decode protobuf bytes while locating nested unknown wire fields.

    Returns:
        The nested field paths retaining unknown bytes, or the descriptor/decode/normalized-value refusal.
    """
    match _pool(image):
        case Result(tag="ok", ok=pool):
            pass
        case Result(error=reason):
            return Error(reason)
    try:
        message_type = GetMessageClass(pool.FindMessageTypeByName(message_fqn))
        message = message_type()
        message.ParseFromString(raw)
        if event_type and getattr(message, "type", None) != event_type:
            return Error(f"CloudEvents type is {getattr(message, 'type', None)!r}, expected {event_type!r}")
    except (KeyError, TypeError, DecodeError) as exc:
        return Error(str(exc))

    found: list[str] = []

    def visit(value: Message, path: str) -> None:
        if len(UnknownFieldSet(value)):
            found.append(path)
        for field, child in value.ListFields():
            if field.message_type is None:
                continue
            if field.is_repeated:
                if field.message_type.GetOptions().map_entry:
                    value_field = field.message_type.fields_by_name["value"]
                    if value_field.type != value_field.TYPE_MESSAGE:
                        continue
                    for key, nested in child.items():
                        visit(nested, f"{path}.{field.name}[{key!r}]")
                else:
                    for index, nested in enumerate(child):
                        visit(nested, f"{path}.{field.name}[{index}]")
            else:
                visit(child, f"{path}.{field.name}")

    try:
        visit(message, message_fqn)
        normalized = message.SerializeToString(deterministic=True)
        roundtripped = message_type()
        roundtripped.ParseFromString(normalized)
        if message != roundtripped:
            return Error("decoded value changes across normalized re-encoding")
    except (NotImplementedError, TypeError, ValueError) as exc:
        return Error(str(exc))
    return Ok(tuple(found))


def _native(value: object) -> object:
    """Project one h5py/numpy scalar or array to JSON-native values without importing a second numeric authority.

    Returns:
        A recursively normalized scalar or list.
    """
    held = value.tolist() if isinstance(value, _ArrayValue) else (value.item() if isinstance(value, _ScalarValue) else value)
    if isinstance(held, bytes):
        return held.decode("utf-8")
    if isinstance(held, list):
        return [_native(item) for item in held]
    return held


def _hdf_module() -> _HdfModule:
    module = import_module("h5py")
    if not isinstance(module, _HdfModule):
        raise TypeError("h5py does not expose the required File constructor")
    return module


def _matrix_market_module() -> _MatrixMarketModule:
    module = import_module("scipy.io")
    if not isinstance(module, _MatrixMarketModule):
        raise TypeError("scipy.io does not expose the required Matrix Market surface")
    return module


def _hdf_group(owner: _HdfGroup, name: str) -> _HdfGroup:
    node = owner[name]
    if not isinstance(node, _HdfGroup):
        raise TypeError(f"{name} is not an HDF group")
    return node


def _hdf_dataset(owner: _HdfGroup, name: str) -> _HdfDataset:
    node = owner[name]
    if not isinstance(node, _HdfDataset):
        raise TypeError(f"{name} is not an HDF dataset")
    return node


def _hdf_roster(owner: _HdfGroup | _HdfAttributes, expected: tuple[str, ...], label: str) -> None:
    found = frozenset(owner.keys())
    if found != frozenset(expected):
        raise ValueError(f"{label} roster is {tuple(sorted(found))!r}, expected {expected!r}")


def _hdf_dtype(value: _HdfDataset | object, expected: str, label: str) -> None:
    if not isinstance(value, (_HdfDataset, _TypedValue)) or value.dtype.descr != [("", expected)]:
        raise ValueError(f"{label} must use canonical HDF5 dtype {expected}")


def _sequence(value: object, label: str) -> tuple[object, ...]:
    held = _native(value)
    if not isinstance(held, list):
        raise TypeError(f"{label} is not an array")
    return tuple(_native(item) for item in held)


def _integer(value: object, label: str) -> int:
    held = _native(value)
    if isinstance(held, bool) or not isinstance(held, int):
        raise TypeError(f"{label} is not an integer")
    return held


def _number(value: object, label: str) -> float:
    held = _native(value)
    if isinstance(held, bool) or not isinstance(held, (float, int)):
        raise TypeError(f"{label} is not numeric")
    return float(held)


def _text(value: object, label: str) -> str:
    held = _native(value)
    if not isinstance(held, str):
        raise TypeError(f"{label} is not text")
    return held


def _virtual_field_facts(path: Path) -> VirtualFieldFacts:
    source, root = path.as_uri(), path.parent.as_uri()
    cube = open_virtual_dataset(source, registry=ObjectStoreRegistry({root: from_url(root)}), parser=HDFParser(group=None, drop_variables=[]))
    if frozenset(cube.variables) != {"field"}:
        raise ValueError("VirtualiZarr field projection must contain only /field")
    variable, data = cube["field"], cube["field"].data
    if not isinstance(data, ManifestArray):
        raise TypeError("VirtualiZarr /field projection is not a ManifestArray")
    raw_dims, shape = tuple(variable.dims), tuple(int(value) for value in data.shape)
    if len(shape) != 2 or data.dtype.name != "float32":
        raise ValueError("VirtualiZarr /field projection must preserve two phony axes and float32 shape")
    match raw_dims:
        case (str() as first, str() as second):
            dims = (first, second)
        case _:
            raise ValueError("VirtualiZarr /field projection must carry exactly two named axes")
    chunks = tuple(
        VirtualChunkFacts(
            key=key, offset=_integer(entry["offset"], f"VirtualiZarr {key} offset"), length=_integer(entry["length"], f"VirtualiZarr {key} length")
        )
        for key, entry in sorted(data.manifest.dict().items())
        if _text(entry["path"], f"VirtualiZarr {key} path") == source
    )
    if len(chunks) != len(data.manifest.dict()):
        raise ValueError("VirtualiZarr field chunks must all reference the admitted specimen")
    return VirtualFieldFacts(dims=(dims[0], dims[1]), shape=(shape[0], shape[1]), dtype="float32", chunks=chunks)


def _field_facts(file: _HdfFile, path: Path) -> FieldFacts:
    data, attrs = _hdf_dataset(file, "field"), file.attrs
    _hdf_roster(file, ("field",), "/")
    _hdf_roster(data.attrs, (), "/field attributes")
    _hdf_roster(attrs, ("bits", "bound", "format-key", "max-residual", "residence"), "/ attributes")
    _hdf_dtype(data, "<f4", "/field")
    _hdf_dtype(attrs["bits"], "<i8", "/ bits")
    _hdf_dtype(attrs["bound"], "<f8", "/ bound")
    _hdf_dtype(attrs["max-residual"], "<f8", "/ max-residual")
    shape = tuple(int(value) for value in data.shape)
    chunks = tuple(int(value) for value in data.chunks or ())
    rows = _sequence(data[()], "/field values")
    values = tuple(_sequence(row, "/field row") for row in rows)
    residence = _text(attrs["residence"], "/ residence")
    if data.dtype.name != "float32" or shape != (2, 2) or len(chunks) != 2:
        raise ValueError("/field must be a 2x2 float32 dataset with two-axis chunks")
    if data.compression != "gzip" or residence != "exact":
        raise ValueError("/field must use gzip and exact residence")
    if len(values) != 2 or any(len(row) != 2 for row in values):
        raise ValueError("/field values must be a 2x2 matrix")
    return FieldFacts(
        path="/field",
        dtype="float32",
        shape=(shape[0], shape[1]),
        chunks=(chunks[0], chunks[1]),
        compression=FieldCompressionFacts(kind="gzip", level=_integer(data.compression_opts, "/field gzip level"), shuffle=bool(data.shuffle)),
        values=(
            (_number(values[0][0], "/field[0,0]"), _number(values[0][1], "/field[0,1]")),
            (_number(values[1][0], "/field[1,0]"), _number(values[1][1], "/field[1,1]")),
        ),
        root_attributes=FieldRootFacts(
            bits=_integer(attrs["bits"], "/ bits"),
            bound=_number(attrs["bound"], "/ bound"),
            format_key=_text(attrs["format-key"], "/ format-key"),
            max_residual=_number(attrs["max-residual"], "/ max-residual"),
            residence="exact",
        ),
        virtual=_virtual_field_facts(path),
    )


def _graduation_facts(file: _HdfFile) -> GraduationFacts:
    bands = _hdf_group(file, "bands")
    climate, temperature = _hdf_group(bands, "climate"), _hdf_group(bands, "temperature")
    categories = _hdf_dataset(climate, "categories")
    climate_mass = _hdf_dataset(climate, "mass")
    edges, temperature_mass = _hdf_dataset(temperature, "edges"), _hdf_dataset(temperature, "mass")
    _hdf_roster(file, ("bands",), "/")
    _hdf_roster(file.attrs, (), "/ attributes")
    _hdf_roster(bands, ("climate", "temperature"), "/bands")
    _hdf_roster(bands.attrs, ("evidence-key",), "/bands attributes")
    _hdf_roster(climate, ("categories", "mass"), "/bands/climate")
    _hdf_roster(climate.attrs, ("kind",), "/bands/climate attributes")
    _hdf_roster(temperature, ("edges", "mass"), "/bands/temperature")
    _hdf_roster(temperature.attrs, ("kind",), "/bands/temperature attributes")
    if (
        _text(climate.attrs["kind"], "/bands/climate kind") != "categorical"
        or _text(temperature.attrs["kind"], "/bands/temperature kind") != "numeric"
    ):
        raise ValueError("graduation band kinds must match their categorical and numeric layouts")
    if categories.dtype.descr != [("", "|O")] or categories.dtype.name != "object" or categories.dtype.metadata != {"vlen": str}:
        raise ValueError("/bands/climate/categories must use the HDF5 UTF-8 variable-length string dtype")
    for dataset, label in ((climate_mass, "/bands/climate/mass"), (edges, "/bands/temperature/edges"), (temperature_mass, "/bands/temperature/mass")):
        _hdf_dtype(dataset, "<f8", label)
        _hdf_roster(dataset.attrs, (), f"{label} attributes")
        if len(dataset.shape) != 1:
            raise ValueError(f"{label} must be rank one")
    if len(categories.shape) != 1:
        raise ValueError("/bands/climate/categories must be rank one")
    _hdf_roster(categories.attrs, (), "/bands/climate/categories attributes")
    return GraduationFacts(
        path="/bands",
        evidence_key=_text(bands.attrs["evidence-key"], "/bands evidence-key"),
        climate=CategoricalBandFacts(
            categories=tuple(_text(value, "/bands/climate/categories") for value in _sequence(categories[()], "/bands/climate/categories")),
            mass=tuple(_number(value, "/bands/climate/mass") for value in _sequence(climate_mass[()], "/bands/climate/mass")),
        ),
        temperature=NumericBandFacts(
            edges=tuple(_number(value, "/bands/temperature/edges") for value in _sequence(edges[()], "/bands/temperature/edges")),
            mass=tuple(_number(value, "/bands/temperature/mass") for value in _sequence(temperature_mass[()], "/bands/temperature/mass")),
        ),
    )


def _sparse_facts(file: _HdfFile) -> SparseFacts:
    group = _hdf_group(file, "A")
    attrs, symmetric = group.attrs, group.attrs["symmetric"]
    datasets = {name: _hdf_dataset(group, name) for name in ("indices", "indptr", "permutation", "values")}
    _hdf_roster(file, ("A",), "/")
    _hdf_roster(file.attrs, (), "/ attributes")
    _hdf_roster(group, tuple(datasets), "/A")
    _hdf_roster(attrs, ("fill", "format", "frobenius", "kind", "ordering", "shape", "symmetric"), "/A attributes")
    for name in ("indices", "indptr", "permutation"):
        _hdf_dtype(datasets[name], "<i4", f"/A/{name}")
        _hdf_roster(datasets[name].attrs, (), f"/A/{name} attributes")
        if len(datasets[name].shape) != 1:
            raise ValueError(f"/A/{name} must be rank one")
    _hdf_dtype(datasets["values"], "<f8", "/A/values")
    _hdf_roster(datasets["values"].attrs, (), "/A/values attributes")
    if len(datasets["values"].shape) != 1:
        raise ValueError("/A/values must be rank one")
    _hdf_dtype(attrs["fill"], "<i8", "/A fill")
    _hdf_dtype(attrs["frobenius"], "<f8", "/A frobenius")
    _hdf_dtype(attrs["ordering"], "<i8", "/A ordering")
    _hdf_dtype(attrs["shape"], "<i8", "/A shape")
    _hdf_dtype(symmetric, "|u1", "/A symmetric")
    shape = tuple(_integer(value, "/A shape") for value in _sequence(attrs["shape"], "/A shape"))
    format_name, kind = _text(attrs["format"], "/A format"), _text(attrs["kind"], "/A kind")
    symmetric_value = _integer(symmetric, "/A symmetric")
    if symmetric_value not in {0, 1}:
        raise ValueError("/A symmetric must be canonical HDF5 uint8 0 or 1")
    if format_name != "csc" or kind != "lu" or len(shape) != 2:
        raise ValueError("/A must be a two-axis csc lu archive")
    return SparseFacts(
        path="/A",
        attributes=SparseAttributesFacts(
            fill=_integer(attrs["fill"], "/A fill"),
            format="csc",
            frobenius=_number(attrs["frobenius"], "/A frobenius"),
            kind="lu",
            ordering=_integer(attrs["ordering"], "/A ordering"),
            shape=(shape[0], shape[1]),
            symmetric=bool(symmetric_value),
        ),
        indices=tuple(_integer(value, "/A/indices") for value in _sequence(datasets["indices"][()], "/A/indices")),
        indptr=tuple(_integer(value, "/A/indptr") for value in _sequence(datasets["indptr"][()], "/A/indptr")),
        permutation=tuple(_integer(value, "/A/permutation") for value in _sequence(datasets["permutation"][()], "/A/permutation")),
        values=tuple(_number(value, "/A/values") for value in _sequence(datasets["values"][()], "/A/values")),
    )


def _waveform_facts(file: _HdfFile) -> WaveformFacts:
    data = _hdf_dataset(file, "waveform")
    _hdf_roster(file, ("waveform",), "/")
    _hdf_roster(file.attrs, (), "/ attributes")
    _hdf_roster(data.attrs, ("sample-rate",), "/waveform attributes")
    _hdf_dtype(data, "<f4", "/waveform")
    _hdf_dtype(data.attrs["sample-rate"], "<f8", "/waveform sample-rate")
    shape = tuple(int(value) for value in data.shape)
    chunks = tuple(int(value) for value in data.chunks or ())
    rows = _sequence(data[()], "/waveform values")
    values = tuple(tuple(_number(value, "/waveform value") for value in _sequence(row, "/waveform row")) for row in rows)
    sample_rate = _number(data.attrs["sample-rate"], "/waveform sample-rate")
    if len(shape) != 2 or 0 in shape or len(chunks) != 2 or chunks[1] != shape[1]:
        raise ValueError("/waveform must be a non-empty rank-two dataset chunked across its whole channel axis")
    if data.dtype.name != "float32" or data.compression != "gzip" or not data.shuffle:
        raise ValueError("/waveform must use little-endian float32 with Shuffle and gzip")
    if len(values) != shape[0] or any(len(row) != shape[1] for row in values):
        raise ValueError("/waveform values do not match the declared shape")
    if not isfinite(sample_rate) or sample_rate <= 0.0:
        raise ValueError("/waveform sample-rate must be finite and positive")
    return WaveformFacts(
        path="/waveform",
        dtype="float32",
        shape=(shape[0], shape[1]),
        chunks=(chunks[0], chunks[1]),
        compression=FieldCompressionFacts(kind="gzip", level=_integer(data.compression_opts, "/waveform gzip level"), shuffle=True),
        sample_rate=sample_rate,
        values=values,
    )


def _decoded_matrix_market(path: Path) -> MatrixMarketFacts:
    module = _matrix_market_module()
    rows, columns, stored, layout, field, symmetry = module.mminfo(path.as_posix())
    decoded = module.mmread(path.as_posix(), spmatrix=False)
    if layout != "coordinate" or field != "real" or symmetry != "general":
        raise ValueError(f"Matrix Market header must be coordinate real general, found {layout} {field} {symmetry}")
    if not isinstance(decoded, _CoordinateArray) or decoded.shape != (rows, columns):
        raise TypeError("Matrix Market body is not the declared SciPy COO array")
    row_values = _sequence(decoded.row, "Matrix Market rows")
    column_values = _sequence(decoded.col, "Matrix Market columns")
    data_values = _sequence(decoded.data, "Matrix Market values")
    if len(row_values) != stored or len(column_values) != stored or len(data_values) != stored:
        raise ValueError("Matrix Market body count does not match its header")
    entries = tuple(
        sorted(
            (
                MatrixEntryFacts(
                    row=_integer(row, "Matrix Market row"),
                    column=_integer(column, "Matrix Market column"),
                    value=_number(value, "Matrix Market value"),
                )
                for row, column, value in zip(row_values, column_values, data_values, strict=True)
            ),
            key=lambda entry: (entry.row, entry.column),
        )
    )
    coordinates = tuple((entry.row, entry.column) for entry in entries)
    if len(frozenset(coordinates)) != len(entries):
        raise ValueError("Matrix Market body contains duplicate coordinates")
    if any(
        entry.row < 0 or entry.row >= rows or entry.column < 0 or entry.column >= columns or not isfinite(entry.value) or not entry.value
        for entry in entries
    ):
        raise ValueError("Matrix Market body contains an out-of-range, non-finite, or explicit-zero entry")
    return MatrixMarketFacts(shape=(rows, columns), entries=entries)


def _matrix_market_facts(path: Path) -> Result[MatrixMarketFacts, str]:
    try:
        return Ok(_decoded_matrix_market(path))
    except (ImportError, OSError, RuntimeError, TypeError, ValueError) as exc:
        return Error(str(exc))


def _decoded_hdf(file: _HdfFile, path: Path, expected: HdfFacts) -> HdfFacts:
    match expected:
        case FieldFacts():
            return _field_facts(file, path)
        case GraduationFacts():
            return _graduation_facts(file)
        case SparseFacts():
            return _sparse_facts(file)
        case WaveformFacts():
            return _waveform_facts(file)


def _hdf_facts(path: Path, expected: HdfFacts) -> Result[HdfFacts, str]:
    """Decode one HDF specimen into the closed normalized facts variant named by its expected asset.

    Returns:
        Typed normalized facts, or the exact HDF boundary refusal.
    """
    try:
        with _hdf_module().File(path.as_posix(), "r") as file:
            return Ok(_decoded_hdf(file, path, expected))
    except (ImportError, IndexError, KeyError, NotImplementedError, ObjectStoreError, OSError, RuntimeError, TypeError, ValueError) as exc:
        return Error(str(exc))


def _message_integer(message: Message, name: str) -> int:
    value = getattr(message, name)
    if isinstance(value, bool) or not isinstance(value, int):
        raise TypeError(f"{name} is not an integer")
    return value


def _hlc_facts(image: Path, message_fqn: str, raw: bytes) -> Result[HlcValueFacts, str]:
    match _pool(image):
        case Result(tag="ok", ok=pool):
            pass
        case Result(error=reason):
            return Error(reason)
    try:
        message = GetMessageClass(pool.FindMessageTypeByName(message_fqn))()
        message.ParseFromString(raw)
        physical, logical = _message_integer(message, "physical"), _message_integer(message, "logical")
        if physical < 0 or logical < 0:
            return Error("HLC descriptor does not expose non-negative integer physical and logical fields")
        return Ok(HlcValueFacts(physical=str(physical), logical=str(logical)))
    except (AttributeError, KeyError, TypeError, DecodeError) as exc:
        return Error(str(exc))


def _message_text(message: Message, name: str) -> str:
    value = getattr(message, name)
    if not isinstance(value, str):
        raise TypeError(f"{name} is not text")
    return value


def _message_bytes(message: Message, name: str) -> bytes:
    value = getattr(message, name)
    if not isinstance(value, bytes):
        raise TypeError(f"{name} is not bytes")
    return value


def _field_message_rows(message: Message, name: str) -> tuple[Message, ...]:
    value = getattr(message, name)
    if isinstance(value, (bytes, str)) or not isinstance(value, Iterable):
        raise TypeError(f"{name} is not a repeated message field")
    rows: list[Message] = []
    for row in value:
        if not isinstance(row, Message):
            raise TypeError(f"{name} contains a non-message value")
        rows.append(row)
    return tuple(rows)


def _field_ints(message: Message, name: str) -> tuple[int, ...]:
    value = getattr(message, name)
    if isinstance(value, (bytes, str)) or not isinstance(value, Iterable):
        raise TypeError(f"{name} is not a repeated integer field")
    rows: list[int] = []
    for row in value:
        if isinstance(row, bool) or not isinstance(row, int):
            raise TypeError(f"{name} contains a non-integer value")
        rows.append(row)
    return tuple(rows)


def _field_texts(message: Message, name: str) -> tuple[str, ...]:
    value = getattr(message, name)
    if isinstance(value, (bytes, str)) or not isinstance(value, Iterable):
        raise TypeError(f"{name} is not a repeated text field")
    rows: list[str] = []
    for row in value:
        if not isinstance(row, str):
            raise TypeError(f"{name} contains a non-text value")
        rows.append(row)
    return tuple(rows)


def _ordered_unique[T: int | str](values: tuple[T, ...], name: str) -> None:
    if tuple(sorted(values)) != values or len(frozenset(values)) != len(values):
        raise ValueError(f"{name} must be strictly ascending and unique")


def _printable(value: str, name: str, *, empty: bool = False) -> str:
    pattern = r"^[\x20-\x7e]*$" if empty else r"^[\x20-\x7e]+$"
    if re.fullmatch(pattern, value) is None:
        raise ValueError(f"{name} must contain printable ASCII only")
    return value


def _defined_enum(message: Message, name: str, value: int | None = None) -> int:
    ordinal = _message_integer(message, name) if value is None else value
    field = message.DESCRIPTOR.fields_by_name.get(name)
    if field is None or field.enum_type is None or ordinal == 0 or ordinal not in field.enum_type.values_by_number:
        raise ValueError(f"{name} must be a defined nonzero enum value")
    return ordinal


def _backend_generation(backend: Message) -> BackendGenerationFacts:
    contract = _printable(_message_text(backend, "contract"), "contract")
    artifacts = _field_message_rows(backend, "artifacts")
    capabilities = _field_message_rows(backend, "capabilities")
    artifact_keys = tuple(_message_text(row, "key") for row in artifacts)
    capability_keys = tuple(_message_text(row, "key") for row in capabilities)
    _ordered_unique(artifact_keys, "artifact keys")
    _ordered_unique(capability_keys, "capability keys")
    preimage = bytearray()

    def ordinal(value: int) -> None:
        preimage.extend(pack("<i", value))

    def octets(value: bytes) -> None:
        ordinal(len(value))
        preimage.extend(value)

    def text(value: str) -> None:
        octets(value.encode("utf-8"))

    text(contract)
    ordinal(len(artifacts))
    for artifact in artifacts:
        providers = _field_ints(artifact, "providers")
        dependencies = _field_texts(artifact, "depends_on")
        _ordered_unique(providers, f"{_message_text(artifact, 'key')} providers")
        _ordered_unique(dependencies, f"{_message_text(artifact, 'key')} dependencies")
        key = _printable(_message_text(artifact, "key"), "artifact key")
        if key in dependencies:
            raise ValueError(f"artifact {key} cannot depend on itself")
        text(key)
        ordinal(_defined_enum(artifact, "role"))
        octets(_message_bytes(artifact, "content"))
        ordinal(len(providers))
        for provider in providers:
            ordinal(_defined_enum(artifact, "providers", provider))
        ordinal(len(dependencies))
        for dependency in dependencies:
            text(_printable(dependency, f"artifact {key} dependency"))
    ordinal(len(capabilities))
    for capability in capabilities:
        text(_printable(_message_text(capability, "key"), "capability key"))
        text(_printable(_message_text(capability, "lane"), "capability lane"))
        text(_printable(_message_text(capability, "requirement"), "capability requirement"))
        text(_printable(_message_text(capability, "requirement_value"), "capability requirement value", empty=True))
        ordinal(_defined_enum(capability, "failure_rank"))
        ordinal(_defined_enum(capability, "restart_class"))
    held = bytes(preimage)
    return BackendGenerationFacts(
        contract=contract,
        artifact_keys=artifact_keys,
        capability_keys=capability_keys,
        preimage_bytes=len(held),
        preimage_xxh128=xxhash.xxh128(held, seed=0).hexdigest(),
        preimage_hex=held.hex(),
    )


def _backend_facts(image: Path, message_fqn: str, raw: bytes) -> Result[BackendGenerationFacts, str]:
    match _pool(image):
        case Result(tag="ok", ok=pool):
            pass
        case Result(error=reason):
            return Error(reason)
    try:
        backend = GetMessageClass(pool.FindMessageTypeByName(message_fqn))()
        Parse(raw.decode("utf-8"), backend, ignore_unknown_fields=False)
        if len(UnknownFieldSet(backend)):
            raise ValueError("backend specimen retains unknown protobuf fields")
        return Ok(_backend_generation(backend))
    except (AttributeError, KeyError, TypeError, UnicodeDecodeError, ValueError, DecodeError, ParseError) as exc:
        return Error(str(exc))


def _expected_facts[T: ExpectedFacts](result: Result[T, str]) -> Result[ExpectedFacts, str]:
    return Ok(result.ok) if result.is_ok() else Error(result.error)


def _specimen_facts(corpus: _ProofContext, case: Case, specimen_path: Path, expected: ExpectedFacts) -> Result[ExpectedFacts, str]:
    if isinstance(expected, BackendGenerationFacts) and isinstance(case.definition, ProtoDefinition):
        try:
            return _expected_facts(_backend_facts(corpus.image, case.definition.message, specimen_path.read_bytes()))
        except OSError as exc:
            return Error(str(exc))
    if isinstance(expected, ContentDigestFacts) and isinstance(case.definition, LawDefinition) and case.definition.format == "binary":
        try:
            facts: ExpectedFacts = ContentDigestFacts(algorithm="xxh128", seed=0, value=xxhash.xxh128(specimen_path.read_bytes(), seed=0).hexdigest())
            return Ok(facts)
        except OSError as exc:
            return Error(str(exc))
    if isinstance(expected, HlcValueFacts) and isinstance(case.definition, ProtoDefinition):
        try:
            return _expected_facts(_hlc_facts(corpus.image, case.definition.message, specimen_path.read_bytes()))
        except OSError as exc:
            return Error(str(exc))
    if (
        isinstance(expected, (FieldFacts, GraduationFacts, SparseFacts, WaveformFacts))
        and isinstance(case.definition, LawDefinition)
        and case.definition.format == "hdf5"
    ):
        return _expected_facts(_hdf_facts(specimen_path, expected))
    if isinstance(expected, MatrixMarketFacts) and isinstance(case.definition, LawDefinition) and case.definition.format == "text":
        return _expected_facts(_matrix_market_facts(specimen_path))
    return Error("expected facts do not match the definition's decoder route")


def _proof_findings(corpus: _ProofContext, subject: str, case: Case) -> Iterable[_Finding]:
    if not isinstance(case.readiness, VerifiedReadiness):
        return
    for index, vector in enumerate(case.readiness.vectors, start=1):
        expected_asset = vector.expected
        if expected_asset is None:
            continue
        expected_path = corpus.root / expected_asset.path
        try:
            expected = _EXPECTED_FACTS.decode(expected_path.read_bytes())
        except (OSError, msgspec.DecodeError, msgspec.ValidationError) as exc:
            yield _finding(
                "expected-invalid",
                subject,
                f"vector {index} expected facts {expected_asset.path}: {exc}",
                "restore the typed expected facts asset",
                f"{CORPUS}/{expected_asset.path}",
            )
            continue
        format_matches = (
            (expected_asset.facts_format == "backend-generation" and isinstance(expected, BackendGenerationFacts))
            or (expected_asset.facts_format == "content-digest" and isinstance(expected, ContentDigestFacts))
            or (expected_asset.facts_format == "hdf5-facts" and isinstance(expected, (FieldFacts, GraduationFacts, SparseFacts, WaveformFacts)))
            or (expected_asset.facts_format == "hlc-value" and isinstance(expected, HlcValueFacts))
            or (expected_asset.facts_format == "matrix-market-facts" and isinstance(expected, MatrixMarketFacts))
        )
        if not format_matches:
            yield _finding(
                "expected-format",
                subject,
                f"vector {index} {expected_asset.facts_format} does not match {type(expected).__name__}",
                "name the expected asset's exact closed facts format",
                f"{CORPUS}/{expected_asset.path}",
            )
            continue
        for specimen in vector.specimens:
            specimen_path = corpus.root / specimen.path
            facts = _specimen_facts(corpus, case, specimen_path, expected)
            if facts.is_error():
                yield _finding(
                    "specimen-invalid",
                    subject,
                    f"vector {index} specimen {specimen.path}: {facts.error}",
                    "repair the native specimen or its case-local facts route",
                    f"{CORPUS}/{specimen.path}",
                )
            elif facts.ok != expected:
                yield _finding(
                    "semantic-mismatch",
                    subject,
                    f"vector {index} specimen {specimen.path} differs from {expected_asset.path}",
                    "repair the producer or replace both specimen and expected facts from a fresh proof",
                    f"{CORPUS}/{specimen.path}",
                )


def _asset_findings(corpus: _ProofContext, subject: str, case: Case) -> Iterable[_Finding]:
    schema: Result[_Document, str] = (
        _document(corpus.schema_root / case.definition.path) if isinstance(case.definition, SchemaDefinition) else Error("no schema definition")
    )
    for asset in _assets(case):
        path, where = corpus.root / asset.path, f"{CORPUS}/{asset.path}"
        if isinstance(case.authority, PublisherAuthority) and asset.fingerprint.algorithm != "sha256":
            yield _finding(
                "publisher-fingerprint",
                subject,
                f"{asset.path} uses {asset.fingerprint.algorithm}, not SHA-256",
                "record immutable publisher custody with SHA-256",
                where,
            )
        if not path.is_file():
            yield _finding("asset-missing", subject, f"{asset.path} is absent", "land the asset, or drop the row", where)
            continue
        raw = path.read_bytes()
        if len(raw) != asset.bytes:
            yield _finding(
                "asset-bytes",
                subject,
                f"{asset.path}: {len(raw)} bytes on disk, {asset.bytes} registered",
                "re-prove in scratch and re-record bytes",
                where,
            )
        expected_length = 32 if asset.fingerprint.algorithm == "xxh128" else 64
        if re.fullmatch(rf"[0-9a-f]{{{expected_length}}}", asset.fingerprint.value) is None:
            yield _finding(
                "asset-fingerprint",
                subject,
                f"{asset.path}: {asset.fingerprint.algorithm} value has the wrong shape",
                "record the lowercase digest at the algorithm's exact width",
                where,
            )
        digest = xxhash.xxh128(raw, seed=0).hexdigest() if asset.fingerprint.algorithm == "xxh128" else sha256(raw).hexdigest()
        if digest != asset.fingerprint.value:
            yield _finding(
                "asset-digest",
                subject,
                f"{asset.path}: {asset.fingerprint.algorithm} {digest} on disk, {asset.fingerprint.value} registered",
                "re-prove in scratch and re-record the digest",
                where,
            )
        if (
            isinstance(case.readiness, VerifiedReadiness)
            and case.readiness.oracle == "semantic-roundtrip"
            and isinstance(case.definition, (CloudEventDefinition, ProtoDefinition))
            and case.definition.framing == "proto-binary"
        ):
            unknown = _semantic_roundtrip(
                corpus.image,
                case.definition.message,
                raw,
                event_type=case.definition.type if isinstance(case.definition, CloudEventDefinition) else "",
            )
            if unknown.is_error():
                yield _finding(
                    "asset-invalid",
                    subject,
                    f"{asset.path}: protobuf decode failed: {unknown.error}",
                    "repair the fixture or its exact message descriptor",
                    where,
                )
            elif unknown.ok:
                yield _finding(
                    "roundtrip-unknown",
                    subject,
                    f"{asset.path} retains unknown protobuf fields at {', '.join(unknown.ok)}",
                    "rebuild the fixture through the current exact descriptor and prove semantic roundtrip again",
                    where,
                )
        if isinstance(case.readiness, VerifiedReadiness) and path.suffix == ".json":
            yield from (
                _finding("asset-invalid", subject, f"{asset.path}: {row}", "repair the asset or its seam schema", where)
                for row in _validated(schema, path)
            )


def _publisher_evidence_findings(root: Path, subject: str, case: Case) -> Iterable[_Finding]:
    definition = case.definition
    if not isinstance(definition, PublisherDefinition):
        return
    source = root / definition.source
    vectors = case.readiness.vectors if isinstance(case.readiness, VerifiedReadiness) else ()
    for specimen in (asset for vector in vectors for asset in vector.specimens):
        contained = specimen.path == definition.source or (source.is_dir() and _inside(specimen.path, PurePosixPath(definition.source)))
        if not contained:
            yield _finding(
                "publisher-source",
                subject,
                f"{specimen.path} is outside publisher source {definition.source}",
                "register only exact bytes at or below the immutable publisher source",
                f"{CORPUS}/{specimen.path}",
            )
    license_row = definition.origin.license
    license_path = root / license_row.path
    if reason := _path_reason(license_row.path):
        yield _finding("publisher-license", subject, f"license path {license_row.path!r}: {reason}", "name the colocated license path")
    elif not license_path.is_file():
        yield _finding(
            "publisher-license",
            subject,
            f"{license_row.path} is absent",
            "vendor the publisher license beside its custody assets",
            f"{CORPUS}/{license_row.path}",
        )
    elif (digest := sha256(license_path.read_bytes()).hexdigest()) != license_row.sha256:
        yield _finding(
            "publisher-license",
            subject,
            f"{license_row.path}: SHA-256 {digest} on disk, {license_row.sha256} registered",
            "restore the exact upstream license bytes or re-prove custody",
            f"{CORPUS}/{license_row.path}",
        )


def _rule_assets(corpus: _Corpus) -> Iterable[_Finding]:
    for entry in corpus.manifest.entries:
        for case in entry.cases:
            subject = f"{entry.id}/{case.id}"
            yield from _asset_findings(corpus, subject, case)
            yield from _proof_findings(corpus, subject, case)
            yield from _publisher_evidence_findings(corpus.root, subject, case)


def prove_case(root: Path, subject: str, case: Case, *, image: Path | None = None) -> Result[CorpusOracleReceipt, str]:
    """Prove one verified case directly through the Assay-owned specimen decoders and typed oracle.

    Returns:
        Typed per-case receipt, or a refusal when the case is blocked rather than evidence-bearing.
    """
    if not isinstance(case.readiness, VerifiedReadiness):
        return Error(f"{subject} is blocked and carries no proof vectors")
    if image is None and isinstance(case.definition, (CloudEventDefinition, ProtoDefinition)):
        return Error(f"{subject} requires the exact built descriptor image")
    context = _DirectProofContext(root=root, image=image or Path())
    findings = (
        *(_finding(rule, subject, detail, repair) for broken, rule, detail, repair in _CASE_LAW if broken(case)),
        *_asset_findings(context, subject, case),
        *_proof_findings(context, subject, case),
        *_publisher_evidence_findings(root, subject, case),
    )
    return Ok(
        CorpusOracleReceipt(
            subject=subject,
            oracle=case.readiness.oracle,
            vectors=len(case.readiness.vectors),
            specimens=sum(len(vector.specimens) for vector in case.readiness.vectors),
            findings=tuple(CorpusOracleFinding(rule=row.rule, detail=row.detail, path=row.path) for row in findings),
        )
    )


def _rule_anchors(corpus: _Corpus) -> Iterable[_Finding]:
    repair = "land the cluster at the page, or re-point the anchor"
    for entry in corpus.manifest.entries:
        cells = tuple((f"{entry.id}/{case.id}", case, actor) for case in entry.cases for actor in _actors(case))
        yield from (
            _finding("anchor-dangling", subject, f"actor {actor.anchor}: {reason}", repair, token=actor.anchor)
            for subject, _, actor in cells
            if (reason := _anchor(corpus.repo, actor.anchor, paths=True))
        )
        for subject, case, actor in cells:
            body_result = _anchor_body(corpus.repo, actor.anchor)
            if _COORDINATE.fullmatch(actor.coordinate) is None:
                yield _finding(
                    "actor-coordinate",
                    subject,
                    f"{actor.anchor} coordinate {actor.coordinate!r} is not one singular source symbol",
                    "name the exact implementation entrypoint without alternatives or heading aliases",
                    token=actor.anchor,
                )
            elif body_result.is_ok() and actor.coordinate not in body_result.ok:
                yield _finding(
                    "actor-coordinate",
                    subject,
                    f"{actor.anchor} body does not contain coordinate {actor.coordinate!r}",
                    "name the literal boundary entrypoint inside the exact anchor cluster",
                    token=actor.anchor,
                )
            if not isinstance(case.definition, (CloudEventDefinition, ProtoDefinition)) or _anchor(corpus.repo, actor.anchor, paths=True):
                continue
            method_tokens: tuple[str, ...] = ()
            if not isinstance(actor, MessageActor):
                service, method = actor.method.rsplit(".", 2)[-2:]
                method_tokens = (service, _python_member(method) if _actor_language(actor) == "python" else method)
            tokens = (case.definition.message.rsplit(".", 1)[-1], *method_tokens)
            match body_result:
                case Result(tag="ok", ok=body) if (
                    actor.binding == "proof" and re.search(r"\b(manifest|descriptor|corpus|contract|generated)\b", body, re.IGNORECASE) is None
                ):
                    yield _finding(
                        "anchor-generic",
                        subject,
                        f"{actor.anchor} does not evidence manifest, descriptor, corpus, contract, or generated-surface consumption",
                        "bind the proof actor to the exact generic proof rail that reads contract authority",
                        token=actor.anchor,
                    )
                case Result(tag="ok", ok=body) if actor.binding != "proof" and any(token not in body for token in tokens):
                    yield _finding(
                        "anchor-generic",
                        subject,
                        f"{actor.anchor} does not name {', '.join(token for token in tokens if token not in body)}",
                        "bind the actor to the exact fence that constructs or decodes this message and RPC method",
                        token=actor.anchor,
                    )
                case _:
                    pass


def _seam_materialization(corpus: _Corpus, entry: Entry) -> _Finding | None:
    seam = corpus.seam_dir(entry)
    expected = PurePosixPath(seam.relative_to(corpus.root).as_posix())
    paths = tuple(
        path
        for case in entry.cases
        for path in (
            *(asset.path for asset in _assets(case)),
            *((case.definition.path,) if isinstance(case.definition, SchemaDefinition) else ()),
            *((case.definition.source,) if isinstance(case.definition, PublisherDefinition) else ()),
            *((case.definition.origin.license.path,) if isinstance(case.definition, PublisherDefinition) else ()),
        )
    )
    if any(_inside(path, expected) for path in paths) and not seam.is_dir():
        return _finding(
            "seam-missing", entry.id, f"seam directory {expected.as_posix()} is absent", "land the registered definition and evidence under its seam"
        )
    if seam.is_dir() and not any(isinstance(case.readiness, VerifiedReadiness) for case in entry.cases):
        return _finding(
            "seam-without-proof",
            entry.id,
            f"seam directory {expected.as_posix()} exists while every case is blocked",
            "remove the directory; only verified cases materialize proof assets",
            f"{CORPUS}/{expected.as_posix()}",
        )
    return None


def _ignored(repo: Path) -> frozenset[str]:
    listed = discover(_IGNORED, root=repo, timeout=_IGNORE_TIMEOUT_S).default_value(b"")
    return frozenset(
        PurePosixPath(row).relative_to(CORPUS).parts[0] for row in listed.decode(errors="replace").split("\0") if row.startswith(f"{CORPUS}/")
    )


def _stray_findings(corpus: _Corpus, estate: frozenset[str], publishers: frozenset[str]) -> Iterable[_Finding]:
    ignored = _ignored(corpus.repo)
    admitted: tuple[tuple[str, Callable[[Path], bool], str], ...] = (
        ("", lambda child: child.name in ignored or (child.name in _ROOTS if child.is_dir() else child.name in _ROOT_FILES), "at the estate root"),
        (_VENDOR, lambda child: child.is_dir() and child.name in publishers, f"under {_VENDOR}/"),
        (_CONFORMANCE, lambda child: child.is_dir() and child.name in estate, f"under {_CONFORMANCE}/"),
    )
    for seam_root, admits, where in admitted:
        directory = corpus.root / seam_root if seam_root else corpus.root
        for child in sorted(directory.iterdir()) if directory.is_dir() else ():
            if not admits(child):
                rel = f"{seam_root}/{child.name}" if seam_root else child.name
                yield _finding(
                    "root-stray",
                    rel,
                    f"unregistered entry {where}",
                    "seat the file under its seam, vendor, conformance, or gen root, register the directory under its authority class, or delete it",
                    f"{CORPUS}/{rel}",
                )


def _rule_disk(corpus: _Corpus) -> Iterable[_Finding]:
    root, entries = corpus.root, corpus.manifest.entries
    classes = {entry.id: frozenset(_entry_class(case) for case in entry.cases) for entry in entries}
    estate = frozenset(entry.id for entry in entries if classes[entry.id] != frozenset(("publisher",)))
    publishers = frozenset(entry.id for entry in entries if classes[entry.id] == frozenset(("publisher",)))
    for entry in entries:
        subject_classes = classes[entry.id]
        if "publisher" in subject_classes and len(subject_classes) != 1:
            yield _finding(
                "authority-mixed-publisher",
                entry.id,
                "publisher and estate authorities share one seam",
                "split publisher custody from estate custody",
            )
        if finding := _seam_materialization(corpus, entry):
            yield finding
    yield from _stray_findings(corpus, estate, publishers)
    owners: dict[str, list[str]] = {}
    owned: dict[Path, set[str]] = {}
    for entry in entries:
        expected = PurePosixPath(corpus.seam_dir(entry).relative_to(root).as_posix())
        owned.setdefault(corpus.seam_dir(entry), set())
        for license_path in dict.fromkeys(
            case.definition.origin.license.path for case in entry.cases if isinstance(case.definition, PublisherDefinition)
        ):
            if reason := _path_reason(license_path):
                yield _finding("path-normalization", entry.id, f"{license_path!r}: {reason}", "name one normalized corpus-relative path")
            elif not _inside(license_path, expected):
                yield _finding(
                    "wrong-seam",
                    entry.id,
                    f"{license_path} is outside {expected.as_posix()}/",
                    "seat the publisher license beside its custody assets",
                    f"{CORPUS}/{license_path}",
                )
            else:
                owned[corpus.seam_dir(entry)].add(license_path)
        for case in entry.cases:
            subject = f"{entry.id}/{case.id}"
            definition_paths = (
                (case.definition.path,)
                if isinstance(case.definition, SchemaDefinition)
                else ((case.definition.source,) if isinstance(case.definition, PublisherDefinition) else ())
            )
            for path in dict.fromkeys((*definition_paths, *(asset.path for asset in _assets(case)))):
                if reason := _path_reason(path):
                    yield _finding("path-normalization", subject, f"{path!r}: {reason}", "name one normalized corpus-relative path")
                    continue
                if not _inside(path, expected):
                    yield _finding(
                        "wrong-seam",
                        subject,
                        f"{path} is outside {expected.as_posix()}/",
                        "seat the definition or evidence under its grouping owner's seam",
                        f"{CORPUS}/{path}",
                    )
                    continue
                target = root / path
                if not target.is_dir():
                    owners.setdefault(path, []).append(subject)
                    owned[corpus.seam_dir(entry)].add(path)
    yield from (
        _finding(
            "path-owned-twice", path, f"file is owned by {', '.join(subjects)}", "assign the file to exactly one atomic case", f"{CORPUS}/{path}"
        )
        for path, subjects in sorted(owners.items())
        if len(subjects) > 1
    )
    for seam_dir, registered in owned.items():
        for file in sorted(candidate for candidate in seam_dir.rglob("*") if candidate.is_file()) if seam_dir.is_dir() else ():
            if (rel := file.relative_to(root).as_posix()) not in registered:
                yield _finding(
                    "seam-stray",
                    rel,
                    "file under a seam that no entry registers",
                    "register it as an asset or definition, or delete it",
                    f"{CORPUS}/{rel}",
                )


def _rule_selectors(corpus: _Corpus) -> Iterable[_Finding]:
    known = _known_symbols(corpus)
    plugins = tuple(row for row in corpus.template.main.plugins if row.language and row.kinds)
    roots = {id(plugin): _plugin_roots(corpus, plugin) for plugin in plugins}
    for plugin in plugins:
        subject = f"{plugin.language}:{plugin.out}:{plugin.binary or plugin.remote}"
        expected = roots[id(plugin)]
        if tuple(plugin.types) != expected:
            yield _finding(
                "selector-drift",
                subject,
                f"configured roots {plugin.types!r} differ from manifest actor roots {expected!r}",
                "replace the plugin types with the exact actor-derived FQNs in manifest order",
                TEMPLATE,
            )
        yield from (
            _finding(
                "selector-generic",
                subject,
                f"{token!r} is not one exact message, enum, service, or method FQN",
                "name exact actor-owned descriptor roots; Buf computes the recursive support closure",
                TEMPLATE,
            )
            for token in plugin.types
            if corpus.files and token not in known
        )
    yield from (
        _finding(
            "selector-coverage",
            f"{language}:{fqn}",
            f"no {language} plugin emitting {kind} owns generated actor root {fqn}",
            f"land one {kind}-capable plugin row whose exact types derive this actor root",
            TEMPLATE,
        )
        for language, kind, fqn in _actor_needs(corpus.manifest)
        if not any(plugin.language == language and kind in plugin.kinds and fqn in roots[id(plugin)] for plugin in plugins)
    )


def _rule_roster(corpus: _Corpus) -> Iterable[_Finding]:
    for language in dict.fromkeys(row.language for row in corpus.template.plugins if row.language):
        roster = _ROSTERS.get(language)
        if roster is None:
            yield _finding("roster-row", language, f"{_EMISSION}/{language} has no roster row", "land the emission target's _Roster row")
            continue
        try:
            text = (corpus.output / roster.catalog).read_text(encoding="utf-8")
        except OSError:
            yield _finding("roster-catalog", language, f"{roster.catalog} is absent", "land the generated package's .api catalog", roster.catalog)
            continue
        span = _roster_span(text)
        if span is None:
            yield _finding(
                "roster-missing",
                language,
                f"{roster.catalog} carries no {ROSTER_BEGIN} / {ROSTER_END} block",
                "land the marker pair; assay contracts generate fills it",
                roster.catalog,
            )
        files = corpus.roster_files.get(language, ())
        roots = _actor_roots(corpus.manifest, language, roster.kinds)
        distributions = _catalog_distributions(corpus.manifest, language)
        if span is not None and corpus.files and not files:
            yield _finding(
                "roster-image",
                language,
                "no Buf type-filtered descriptor image was built for the language",
                "repair the buf-roster lanes; the roster must derive from Buf's filtered closure",
                roster.catalog,
            )
        elif span is not None and text[span[0] : span[1]] != _roster_block(roster, files, roots, distributions):
            yield _finding(
                "roster-stale",
                language,
                f"{roster.catalog} roster block differs from the emission",
                "assay contracts generate, then commit the catalog",
                roster.catalog,
            )


def _rule_lock(corpus: _Corpus) -> Iterable[_Finding]:
    try:
        config = msgspec.convert(YAML(typ="safe").load((corpus.repo / _CONFIG).read_text(encoding="utf-8")), _BufConfig)
    except OSError, msgspec.ValidationError:
        return ()
    return (
        (_finding("lock-present", _LOCK, f"{_CONFIG} declares deps but {_LOCK} is absent", "run buf dep update and commit buf.lock", _LOCK),)
        if config.deps and not (corpus.repo / _LOCK).is_file()
        else ()
    )


_RULES: Final[tuple[_Rule, ...]] = (
    _rule_schema,
    _rule_identity,
    _rule_entry,
    _rule_definition,
    _rule_supports,
    _rule_actor_methods,
    _rule_assets,
    _distribution_findings,
    _rule_anchors,
    _rule_disk,
    _rule_selectors,
    _rule_roster,
    _rule_lock,
)


def _corpus_of(
    repo: Path,
    image: Path,
    template: _Templates,
    projections: _Projections,
    roster_files: _RosterFiles = _NO_ROSTER_FILES,
    material: Path | None = None,
) -> Result[tuple[_Corpus, tuple[_Finding, ...]], Fault]:
    files = _files(image) if image.is_file() else ()
    head = (
        []
        if files
        else [
            _finding(
                "image-missing", _IMAGE, "no descriptor set was built", "repair the buf-build lane; message resolution and roster emission wait on it"
            )
        ]
    )
    match load_manifest(repo / CORPUS):
        case Result(tag="ok", ok=manifest):
            pass
        case Result(error=Fault(status=RailStatus.FAILED, message=message)):
            rule = "manifest-missing" if message.endswith(": missing") else "manifest-undecodable"
            repair = (
                "author manifest.json from the owning fences"
                if rule == "manifest-missing"
                else "repair the named field; the schema is derived_schema()"
            )
            head.append(_finding(rule, MANIFEST, message, repair, f"{CORPUS}/{MANIFEST}"))
            manifest = Manifest(entries=())
        case Result(error=fault):
            return Error(fault)
    corpus = _Corpus(
        repo=repo,
        image=image,
        manifest=manifest,
        files=files,
        template=template,
        known=_known(files),
        services=_services(files),
        methods=_methods(files),
        map_messages=frozenset(f"{file.package}.{name}" for file in files for name in file.map_messages),
        projections=projections,
        roster_files=roster_files,
        material=material,
    )
    return Ok((corpus, tuple(head)))


def _targets(repo: Path, image: Path) -> tuple[str, ...]:
    known = _known(_files(image)) if image.is_file() else frozenset()
    entries = load_manifest(repo / CORPUS).map(lambda manifest: manifest.entries).default_value(())
    return tuple(
        dict.fromkeys(
            owner
            for entry in entries
            for case in entry.cases
            if isinstance(case.definition, SchemaDefinition) and case.definition.framing in _JSON_FRAMINGS
            for arm, owner in (_arm(case.definition.derived_from),)
            if arm == "proto" and owner in known
        )
    )


def _audit(
    repo: Path,
    image: Path,
    template: _Templates,
    projections: _Projections = _NO_PROJECTIONS,
    roster_files: _RosterFiles = _NO_ROSTER_FILES,
    material: Path | None = None,
) -> Result[_Audit, Fault]:
    match _corpus_of(repo, image, template, projections, roster_files, material):
        case Result(tag="ok", ok=(corpus, head)):
            pass
        case Result(error=fault):
            return Error(fault)
    manifest, files = corpus.manifest, corpus.files
    findings = (*head, *(finding for rule in _RULES for finding in rule(corpus)))
    return Ok(
        _Audit(
            findings=findings,
            files=files,
            seams=tuple(dict.fromkeys(entry.id for entry in manifest.entries)),
            anchors=tuple((row.subject, row.token) for row in findings if row.rule == "anchor-dangling"),
            entries=len(manifest.entries),
            assets=sum(len(_assets(case)) for entry in manifest.entries for case in entry.cases),
        )
    )


# --- [THUNKS]


def _plugins(root: Path, template: _Templates) -> InprocThunk:
    def run(check: Check) -> Completed:
        binaries = dict.fromkeys(filter(None, (row.binary for row in template.plugins)))
        resolved = tuple((spelling, _resolve(root, spelling)) for spelling in binaries)
        projector = _resolve(root, JSONSCHEMA_PLUGIN)
        misses = tuple(spelling for spelling, path in resolved if not path)
        hint = "; ".join(f"{spelling}: {_hint(spelling)}" for spelling in misses)
        status = RailStatus.UNSUPPORTED if misses else RailStatus.OK
        notes = () if projector else (f"{JSONSCHEMA_PLUGIN}: {_MACHINE_HINT}",)
        payload = _ENCODER.encode(_Probe(plugins=(*resolved, (JSONSCHEMA_PLUGIN, projector)), misses=misses, hint=hint, projector=projector))
        return receipt(check.args.fill(check.tool.command), status.exit_code, stdout=payload, stderr=hint.encode(), status=status, notes=notes)

    return run


def _corpus(repo: Path, image: Path, template: _Templates, projections: _Projections, roster_files: _RosterFiles) -> InprocThunk:
    def run(check: Check) -> Completed:
        argv = check.args.fill(check.tool.command)
        match _audit(repo, image, template, projections, roster_files):
            case Result(tag="ok", ok=audit):
                status = RailStatus.FAILED if audit.findings else RailStatus.OK
                return receipt(argv, status.exit_code, stdout=_ENCODER.encode(audit), status=status)
            case Result(error=fault):
                return receipt(argv, RailStatus.FAULTED.exit_code, stderr=fault.message.encode(), status=RailStatus.FAULTED)

    return run


def _preflight(repo: Path, image: Path, template: _Templates, projections: _Projections, roster_files: _RosterFiles) -> InprocThunk:
    """Refuse every unsafe registry or derivation defect before generation can write a public out root.

    Returns:
        In-process preflight thunk carrying only write-blocking findings.
    """

    def run(check: Check) -> Completed:
        argv = check.args.fill(check.tool.command)
        match _audit(repo, image, template, projections, roster_files):
            case Result(tag="ok", ok=audit):
                blocking = tuple(finding for finding in audit.findings if finding.rule not in _EMITTABLE)
                status = RailStatus.FAILED if blocking else RailStatus.OK
                return receipt(argv, status.exit_code, stdout=_ENCODER.encode(msgspec.structs.replace(audit, findings=blocking)), status=status)
            case Result(error=fault):
                return receipt(argv, RailStatus.FAULTED.exit_code, stderr=fault.message.encode(), status=RailStatus.FAULTED)

    return run


def _freshness(repo: Path, scratch: Path, roots: tuple[str, ...]) -> InprocThunk:
    def run(check: Check) -> Completed:
        match load_manifest(repo / CORPUS):
            case Result(tag="ok", ok=manifest):
                _write_distributions(repo, scratch / _GEN, manifest)
            case _:
                pass
        stale, diff = freshness_rows(repo, scratch / _GEN, roots, _DIFF_LINES)
        target = scratch / _DIFF
        artifacts: tuple[Artifact, ...] = ()
        if diff:
            target.write_text(diff, encoding="utf-8")
            artifacts = (Artifact(id="freshness-diff", kind=ArtifactKind.SCOPE, path=str(target), bytes=len(diff.encode()), lines=diff.count("\n")),)
        status = RailStatus.FAILED if stale else RailStatus.OK
        payload = _ENCODER.encode(_Freshness(stale=stale, roots=roots, diff=str(target) if diff else ""))
        return receipt(check.args.fill(check.tool.command), status.exit_code, stdout=payload, status=status, artifacts=artifacts)

    return run


def _write_distributions(repo: Path, base: Path, manifest: Manifest) -> tuple[tuple[_Finding, ...], tuple[str, ...]]:
    findings: list[_Finding] = []
    notes: list[str] = []
    emissions: list[tuple[str, bytes]] = []
    for subject, _, asset, distribution in _distributions(manifest):
        rendered = _distribution_bytes(repo, asset, distribution)
        if rendered.is_error():
            findings.append(
                _finding(
                    "distribution-source",
                    subject,
                    f"{asset.path}: {rendered.error}",
                    "restore the exact publisher asset before generating its package projection",
                    f"{CORPUS}/{asset.path}",
                )
            )
            continue
        emissions.append((distribution.path, rendered.ok))
    emissions.append((_PY_MARKER, b""))
    rendered_image = render(base, tuple(emissions))
    if rendered_image.is_error():
        findings.append(_finding("distribution-write", "generation", rendered_image.error.message, "restore the staged image filesystem"))
    else:
        notes.extend(f"distribution: {path} {'written' if changed else 'unchanged'}" for path, changed in rendered_image.ok)
    return tuple(findings), tuple(notes)


def _schemas(corpus: _Corpus) -> tuple[tuple[_Finding, ...], tuple[str, ...]]:
    findings: list[_Finding] = []
    notes: list[str] = []
    emissions: list[tuple[str, bytes]] = []
    for entry in corpus.manifest.entries:
        for case in entry.cases:
            if not isinstance(case.definition, SchemaDefinition):
                continue
            rel = case.definition.path
            match _derived(corpus, f"{entry.id}/{case.id}", case.definition, f"{CORPUS}/{rel}"):
                case Result(tag="ok", ok=derived):
                    emissions.append((f"{CORPUS}/{rel}", derived))
                case Result(error=rows):
                    findings.extend(rows)
    rendered = render(corpus.output, tuple(emissions))
    if rendered.is_error():
        findings.append(_finding("schema-write", "generation", rendered.error.message, "restore the staged image filesystem"))
    else:
        compared = changes(corpus.repo, tuple(emissions))
        if compared.is_error():
            findings.append(_finding("schema-read", "generation", compared.error.message, "restore the committed schema filesystem"))
        else:
            notes.extend(f"schema: {path} {'written' if changed else 'unchanged'}" for path, changed in compared.ok)
    return tuple(findings), tuple(notes)


def _roster_catalogs(template: _Templates) -> tuple[str, ...]:
    catalogs = (
        _ROSTERS[language].catalog for language in dict.fromkeys(row.language for row in template.plugins if row.language) if language in _ROSTERS
    )
    return tuple(dict.fromkeys(catalogs))


def _schema_files(manifest: Manifest) -> tuple[str, ...]:
    derived = (
        f"{CORPUS}/{case.definition.path}" for entry in manifest.entries for case in entry.cases if isinstance(case.definition, SchemaDefinition)
    )
    return tuple(dict.fromkeys((f"{CORPUS}/{SCHEMA}", *derived)))


def _owned_files(template: _Templates, manifest: Manifest) -> tuple[str, ...]:
    """Project every derived file the emission rewrites outside the generated out roots.

    Returns:
        Repo-relative roster catalogs and seam schemas, deduplicated in emission order.
    """
    return (*_roster_catalogs(template), *_schema_files(manifest))


def _generation_transaction(repo: Path, template: _Templates, manifest: Manifest) -> SwapTransaction:
    targets = tuple(repo / rel for rel in (*out_dirs(template), *_owned_files(template, manifest)))
    return SwapTransaction(marker=repo / _COMMIT_MARKER, argv=("contracts", "generate"), targets=targets)


def _roster_emission(staged: GenerationImage, corpus: _Corpus, language: str) -> tuple[tuple[_Finding, ...], tuple[str, bytes] | None, str | None]:
    roster = _ROSTERS.get(language)
    if roster is None:
        return (_finding("roster-row", language, f"{_EMISSION}/{language} has no roster row", "land the emission target's _Roster row"),), None, None
    catalog = staged.read(roster.catalog)
    if catalog.is_error():
        finding = _finding(
            "roster-catalog", language, catalog.error.message, "restore the generated package's unaliased .api catalog", roster.catalog
        )
        return (finding,), None, None
    try:
        text = catalog.ok.decode()
    except UnicodeDecodeError:
        finding = _finding(
            "roster-catalog", language, f"{roster.catalog} is not UTF-8", "restore the generated package's UTF-8 .api catalog", roster.catalog
        )
        return (finding,), None, None
    span = _roster_span(text)
    if span is None:
        finding = _finding(
            "roster-missing", language, f"{roster.catalog} carries no {ROSTER_BEGIN} / {ROSTER_END} block", "land the marker pair", roster.catalog
        )
        return (finding,), None, None
    files = corpus.roster_files.get(language, ())
    if not files:
        finding = _finding(
            "roster-image",
            language,
            "no Buf type-filtered descriptor image was built for the language",
            "repair the buf-roster lanes; the roster must derive from Buf's filtered closure",
            roster.catalog,
        )
        return (finding,), None, None
    start, end = span
    roots = _actor_roots(corpus.manifest, language, roster.kinds)
    distributions = _catalog_distributions(corpus.manifest, language)
    emitted = text[:start] + _roster_block(roster, files, roots, distributions) + text[end:]
    changed = emitted != text
    emission = (roster.catalog, emitted.encode()) if changed else None
    return (), emission, f"roster: {roster.catalog} {'written' if changed else 'unchanged'}"


def _emit_rosters(staged: GenerationImage, corpus: _Corpus, template: _Templates) -> tuple[tuple[_Finding, ...], tuple[str, ...]]:
    findings: list[_Finding] = []
    emissions: list[tuple[str, bytes]] = []
    notes: list[str] = []
    for language in dict.fromkeys(row.language for row in template.plugins if row.language):
        refused, emission, note = _roster_emission(staged, corpus, language)
        findings.extend(refused)
        emissions.extend((emission,) if emission is not None else ())
        notes.extend((note,) if note is not None else ())
    rendered = render(staged.root, tuple(emissions))
    if rendered.is_error():
        findings.append(_finding("roster-write", "generation", rendered.error.message, "restore the staged image filesystem"))
    return tuple(findings), tuple(notes)


def _emit(repo: Path, image: Path, template: _Templates, projections: _Projections, roster_files: _RosterFiles) -> InprocThunk:
    def run(check: Check) -> Completed:
        argv = check.args.fill(check.tool.command)
        if not image.is_file():
            return receipt(
                argv, RailStatus.FAULTED.exit_code, stderr=b"no descriptor set was built; the roster cannot be emitted", status=RailStatus.FAULTED
            )
        registry = load_manifest(repo / CORPUS).default_value(Manifest(entries=()))
        owned = _owned_files(template, registry)
        match compose_image(repo, image.parent / _GEN, image.parent / _COMMIT_IMAGE, out_dirs(template), owned):
            case Result(tag="ok", ok=staged):
                pass
            case Result(error=fault):
                return receipt(argv, RailStatus.FAULTED.exit_code, stderr=fault.message.encode(), status=RailStatus.FAULTED)
        match _corpus_of(repo, image, template, projections, roster_files, staged.root):
            case Result(tag="ok", ok=(corpus, head)):
                pass
            case Result(error=fault):
                return receipt(argv, RailStatus.FAULTED.exit_code, stderr=fault.message.encode(), status=RailStatus.FAULTED)
        distribution_findings, distribution_notes = _write_distributions(repo, staged.root, corpus.manifest)
        refused, written = _schemas(corpus)
        findings: list[_Finding] = [*head, *distribution_findings, *refused]
        notes: list[str] = [*distribution_notes, *written]
        model_schema = derived_schema()
        rendered = render(staged.root, ((f"{CORPUS}/{SCHEMA}", model_schema),))
        if rendered.is_error():
            findings.append(_finding("schema-write", SCHEMA, rendered.error.message, "restore the staged image filesystem"))
        else:
            compared = changes(repo, ((f"{CORPUS}/{SCHEMA}", model_schema),))
            if compared.is_error():
                findings.append(_finding("schema-read", SCHEMA, compared.error.message, "restore the committed schema filesystem"))
            else:
                notes.extend(f"schema: {path} {'written' if changed else 'unchanged'}" for path, changed in compared.ok)
        roster_findings, roster_notes = _emit_rosters(staged, corpus, template)
        findings.extend(roster_findings)
        notes.extend(roster_notes)
        if not findings:
            match _audit(repo, image, template, projections, roster_files, staged.root):
                case Result(tag="ok", ok=audit):
                    findings.extend(audit.findings)
                case Result(error=fault):
                    return receipt(argv, RailStatus.FAULTED.exit_code, stderr=fault.message.encode(), status=RailStatus.FAULTED)
        if not findings:
            committed = staged.sources(repo, owned).bind(_generation_transaction(repo, template, registry).install)
            if committed.is_error():
                return receipt(argv, RailStatus.FAULTED.exit_code, stderr=committed.error.message.encode(), status=RailStatus.FAULTED)
        status = RailStatus.FAILED if findings else RailStatus.OK
        return receipt(
            argv, status.exit_code, stdout=_ENCODER.encode(_Audit(findings=tuple(findings), files=corpus.files)), status=status, notes=tuple(notes)
        )

    return run


# --- [FOLD]


def _gate(done: Completed) -> _Gate | None:
    if not (done.argv and done.argv[0] in _GATED and done.stdout.startswith(b"{")):
        return None
    try:
        return _GATE.decode(done.stdout)
    except msgspec.MsgspecError:
        return None


def _verdict(done: Completed) -> RailStatus:
    return RailStatus.OK if done.status.band is Band.PROVED else done.status


def _rows(gates: tuple[_Gate, ...]) -> tuple[Match, ...]:
    findings = tuple(
        Match(
            id=f"corpus:{row.rule}",
            kind=ArtifactKind.CODE,
            text=f"corpus: {row.subject}: {row.rule}: {row.detail}; repair: {row.repair}",
            severity="error",
            path=row.path,
            message=row.detail,
        )
        for gate in gates
        if isinstance(gate, _Audit)
        for row in gate.findings
    )
    stale = tuple((kind, path) for gate in gates if isinstance(gate, _Freshness) for kind, path in gate.stale)
    repair = "assay contracts generate, then commit the tree"
    freshness = tuple(
        Match(
            id=f"freshness:{kind}",
            kind=ArtifactKind.CODE,
            text=f"freshness: {path}: {kind}; repair: {repair}",
            severity="error",
            path=path,
            message=kind,
        )
        for kind, path in stale[:_STALE_CAP]
    )
    return (*findings, *freshness)


def _resolved(done: Completed, module: str) -> Result[str, str]:
    if done.status not in {RailStatus.OK, RailStatus.EMPTY}:
        return Error(done.stderr[-_TAIL:].decode(errors="replace").strip() or f"baseline resolver exited {done.returncode}")
    try:
        commit = msgspec.json.decode(done.stdout, type=_Baseline).commit
    except msgspec.MsgspecError as exc:
        return Error(f"baseline resolver returned malformed JSON: {exc}")
    return Ok(f"{module}:{commit}") if _BSR_COMMIT.fullmatch(commit) else Error(f"baseline resolver returned invalid commit {commit!r}")


def _located(done: Completed, module: str) -> Result[None, str]:
    if done.status not in {RailStatus.OK, RailStatus.EMPTY}:
        return Error(done.stderr[-_TAIL:].decode(errors="replace").strip() or f"module lookup exited {done.returncode}")
    try:
        info = msgspec.json.decode(done.stdout, type=_ModuleInfo)
    except msgspec.MsgspecError as exc:
        return Error(f"module lookup returned malformed JSON: {exc}")
    coordinate = f"{info.remote}/{info.owner}/{info.name}"
    return (
        Error(f"module lookup returned {coordinate!r}, expected {module!r}")
        if coordinate != module
        else Error(f"module lookup returned default label {info.default_label_name!r}, expected {_DEFAULT_LABEL!r}")
        if info.default_label_name != _DEFAULT_LABEL
        else Ok(None)
    )


def _absent_module(done: Completed, module: str) -> bool:
    return done.stderr.decode(errors="replace") == (
        f'Failure: a module named "{module}" does not exist, use "buf registry module create" to create one\n'
    )


def _authenticated(done: Completed) -> Result[str, str]:
    if done.status not in {RailStatus.OK, RailStatus.EMPTY}:
        return Error(done.stderr[-_TAIL:].decode(errors="replace").strip() or f"credential probe exited {done.returncode}")
    try:
        identity = msgspec.json.decode(done.stdout, type=_Identity)
    except msgspec.MsgspecError as exc:
        return Error(f"credential probe returned malformed JSON: {exc}")
    return Ok(identity.username) if identity.username else Error("credential probe resolved an empty account")


def _published(done: Completed, module: str) -> Result[str, str]:
    if done.status not in {RailStatus.OK, RailStatus.EMPTY}:
        return Error(done.stderr[-_TAIL:].decode(errors="replace").strip() or f"publish exited {done.returncode}")
    coordinate = done.stdout.decode(errors="replace").removesuffix("\n")
    prefix = f"{module}:"
    commit = coordinate.removeprefix(prefix) if coordinate.startswith(prefix) else ""
    return Ok(coordinate) if _BSR_COMMIT.fullmatch(commit) else Error(f"publish returned invalid commit coordinate {coordinate!r}")


def _detail(lanes: _Lanes, gates: tuple[_Gate, ...], rows: tuple[Match, ...], scratch: str, module: str) -> ContractsRun:
    probe = next((gate for gate in gates if isinstance(gate, _Probe)), _Probe())
    audit = next((gate for gate in gates if isinstance(gate, _Audit)), _Audit())
    findings = sum(len(gate.findings) for gate in gates if isinstance(gate, _Audit))
    fresh = next((gate for gate in gates if isinstance(gate, _Freshness)), _Freshness())
    format_out = next((done.stdout for name, done in lanes if name == "buf-format"), b"")
    counts = (
        ("files", len(audit.files)),
        ("packages", len({file.package for file in audit.files})),
        ("messages", sum(len(file.messages) for file in audit.files)),
        ("services", sum(len(file.services) for file in audit.files)),
        ("entries", audit.entries),
        ("assets", audit.assets),
        ("findings", findings),
        ("stale", len(fresh.stale)),
    )
    return ContractsRun(
        lanes=tuple((name, _verdict(done).value) for name, done in lanes),
        module=module,
        baseline=next((coordinate.ok for name, done in lanes if name == "buf-baseline" and (coordinate := _resolved(done, module)).is_ok()), ""),
        published=next((coordinate.ok for name, done in lanes if name == "buf-push" and (coordinate := _published(done, module)).is_ok()), ""),
        counts=counts,
        packages=tuple(sorted({file.package for file in audit.files})),
        stale=fresh.stale,
        unformatted=tuple(
            found.group(1) for line in format_out.decode(errors="replace").splitlines() if (found := _DIFF_HEADER.match(line)) is not None
        ),
        violations=tuple((row.id.removeprefix("buf:"), row.path, row.line) for row in rows if row.id.startswith("buf:")),
        plugins=probe.plugins,
        seams=audit.seams,
        anchors=audit.anchors,
        template=TEMPLATE,
        scratch=scratch,
        faults=tuple((name, done.stderr[-_TAIL:].decode(errors="replace").strip()) for name, done in lanes if done.status is RailStatus.FAULTED),
    )


def _report(verb: str, lanes: _Lanes, scratch: str, module: str) -> Report:
    decoded = tuple((done, _gate(done)) for _, done in lanes)
    gates = tuple(gate for _, gate in decoded if gate is not None)
    base = fold(Claim.CONTRACTS, verb, tuple(done for _, done in lanes), promote_empty=True)
    consumed = frozenset(" ".join(done.argv) for done, gate in decoded if gate is not None)
    kept = tuple(row for row in base.results if row.id not in consumed)
    stale_total = sum(len(gate.stale) for gate in gates if isinstance(gate, _Freshness))
    notes = (*base.notes, *cap_note(min(stale_total, _STALE_CAP), stale_total, _STALE_CAP, tail="stale rows ride the detail"))
    return msgspec.structs.replace(base, results=(*_rows(gates), *kept), notes=notes, detail=_detail(lanes, gates, kept, scratch, module))


# --- [PHASES]


def _scratch(settings: AssaySettings, scope: ArtifactScope) -> Result[Path, Fault]:
    protocol = settings.artifact_backend.protocol
    if protocol not in {"", "file"}:
        return Error(Fault(("buf", "generate"), RailStatus.UNSUPPORTED, f"scratch regeneration requires the file artifact backend; got {protocol!r}"))
    return Ok(Path(scope.ensure()))


def _lane_name(chk: Check) -> str:
    return f"{chk.tool.name}:{chk.args.fqn}" if chk.args.fqn else chk.tool.name


def _seat(chk: Check, status: RailStatus, reason: str) -> Completed:
    argv = chk.args.fill(chk.tool.command)
    return receipt(argv, status.exit_code, stderr=reason.encode(), status=status, notes=(f"{_lane_name(chk)}: {status.value}: {reason}",))


def _phase(
    executor: Executor, checks: tuple[Check, ...], seat: Callable[[Check], Completed | None], *, settings: AssaySettings, scope: ArtifactScope
) -> Result[_Lanes, Fault]:
    seated = tuple(seat(chk) for chk in checks)
    live = tuple(chk for chk, done in zip(checks, seated, strict=True) if done is None)

    def folded(done: tuple[Completed, ...]) -> _Lanes:
        played = iter(done)
        return tuple((_lane_name(chk), pre if pre is not None else next(played)) for chk, pre in zip(checks, seated, strict=True))

    return sequence(block.of_seq(executor.fan(live, settings=settings, scope=scope, routed=_ROUTED))).map(lambda done: folded(tuple(done)))


def _probe(root: Path, template: _Templates, executor: Executor, *, settings: AssaySettings, scope: ArtifactScope) -> Result[Completed, Fault]:
    probe = Check(tool=_ROWS["plugin-probe", Mode.VERIFY], thunk=_plugins(root, template))
    return _phase(executor, (probe,), lambda _chk: None, settings=settings, scope=scope).map(itemgetter(0)).map(itemgetter(1))


def _lane_check(lane: _Lane, scratch: Path) -> Check:
    args = ToolArgs(output=str(scratch / lane.output)) if lane.output else ToolArgs()
    return Check(tool=_ROWS[lane.name, lane.mode], args=args)


def _module_phase(module: str, executor: Executor, *, settings: AssaySettings, scope: ArtifactScope) -> Result[tuple[Completed, bool], Fault]:
    check = Check(tool=_ROWS["buf-module", Mode.QUERY], args=ToolArgs(input=module))

    def admitted(done: Completed) -> tuple[Completed, bool]:
        located = _located(done, module)
        if located.is_ok():
            return done, True
        if _absent_module(done, module):
            return _seat(check, RailStatus.SKIP, f"first publish: {module} does not exist"), False
        if done.status in {RailStatus.OK, RailStatus.EMPTY}:
            reason = located.error
            return (
                msgspec.structs.replace(
                    done, status=RailStatus.FAULTED, stderr=reason.encode(), notes=(*done.notes, f"buf-module: faulted: {reason}")
                ),
                False,
            )
        return (msgspec.structs.replace(done, status=RailStatus.FAULTED) if done.status is RailStatus.FAILED else done), False

    return _phase(executor, (check,), lambda _chk: None, settings=settings, scope=scope).map(itemgetter(0)).map(itemgetter(1)).map(admitted)


def _baseline_phase(module: str, executor: Executor, *, present: bool, settings: AssaySettings, scope: ArtifactScope) -> Result[Completed, Fault]:
    """Resolve the module's default label to the immutable commit publish custody compares against.

    Returns:
        The resolver lane, seated SKIP when the module is absent for first publish and faulted when its coordinate cannot be read.
    """
    check = Check(tool=_ROWS["buf-baseline", Mode.QUERY], args=ToolArgs(input=f"{module}:{_DEFAULT_LABEL}"))

    def admitted(done: Completed) -> Completed:
        coordinate = _resolved(done, module)
        if coordinate.is_ok():
            return done
        if done.status in {RailStatus.OK, RailStatus.EMPTY}:
            reason = coordinate.error
            return msgspec.structs.replace(
                done, status=RailStatus.FAULTED, stderr=reason.encode(), notes=(*done.notes, f"buf-baseline: faulted: {reason}")
            )
        return msgspec.structs.replace(done, status=RailStatus.FAULTED) if done.status is RailStatus.FAILED else done

    return (
        _phase(
            executor,
            (check,),
            lambda chk: _seat(chk, RailStatus.SKIP, "first publish: named module is absent") if not present else None,
            settings=settings,
            scope=scope,
        )
        .map(itemgetter(0))
        .map(itemgetter(1))
        .map(admitted)
    )


def _auth_phase(module: str, executor: Executor, *, settings: AssaySettings, scope: ArtifactScope) -> Result[Completed, Fault]:
    """Resolve the publication credential against the module's own registry before the gate spends any work.

    Returns:
        The credential probe, carrying the resolved account in its notes, or faulted with the repair the operator owes.
    """
    remote = module.partition("/")[0]
    base = _ROWS["buf-module", Mode.QUERY]
    tool = msgspec.structs.replace(base, name="buf-auth", command=("buf", "registry", "whoami", "{input}", "--format", "json"))
    check = Check(tool=tool, args=ToolArgs(input=remote))

    def admitted(done: Completed) -> Completed:
        account = _authenticated(done)
        if account.is_ok():
            return msgspec.structs.replace(done, notes=(*done.notes, f"buf-auth: {remote}: {account.ok}"))
        reason = f"{account.error}; {_CREDENTIAL_HINT}"
        return msgspec.structs.replace(done, status=RailStatus.FAULTED, stderr=reason.encode(), notes=(*done.notes, f"buf-auth: faulted: {reason}"))

    return _phase(executor, (check,), lambda _chk: None, settings=settings, scope=scope).map(itemgetter(0)).map(itemgetter(1)).map(admitted)


def _verify_phase(module: str, published: str, executor: Executor, *, settings: AssaySettings, scope: ArtifactScope) -> Result[Completed, Fault]:
    """Re-resolve the default label after the push and require it to carry the coordinate the push returned.

    Returns:
        The confirming resolver, faulted when the label carries another commit or when the coordinate cannot be read back.
    """
    tool = msgspec.structs.replace(_ROWS["buf-baseline", Mode.QUERY], name="buf-verify")
    check = Check(tool=tool, args=ToolArgs(input=f"{module}:{_DEFAULT_LABEL}"))

    def admitted(done: Completed) -> Completed:
        coordinate = _resolved(done, module)
        reason = (
            f"published {published!r} but the label could not be read back: {coordinate.error}"
            if coordinate.is_error()
            else ""
            if coordinate.ok == published
            else f"{_DEFAULT_LABEL} carries {coordinate.ok!r} after publishing {published!r}"
        )
        if not reason:
            return done
        return msgspec.structs.replace(done, status=RailStatus.FAULTED, stderr=reason.encode(), notes=(*done.notes, f"buf-verify: faulted: {reason}"))

    return _phase(executor, (check,), lambda _chk: None, settings=settings, scope=scope).map(itemgetter(0)).map(itemgetter(1)).map(admitted)


def _prepush_phase(module: str, lanes: _Lanes, executor: Executor, *, settings: AssaySettings, scope: ArtifactScope) -> Result[Completed, Fault]:
    """Revalidate the publication coordinate immediately before the irreversible push.

    Returns:
        The reused module lookup for bootstrap, or the reused default-label resolver for an existing module, faulted when its evidence changed.
    """
    bootstrap = dict(lanes)["buf-module"].status is RailStatus.SKIP
    if bootstrap:

        def absent(row: tuple[Completed, bool]) -> Completed:
            done, present = row
            if not present or done.status is RailStatus.FAULTED:
                return done
            reason = f"{module} appeared after first-publish admission"
            return msgspec.structs.replace(
                done, status=RailStatus.FAULTED, stderr=reason.encode(), notes=(*done.notes, f"buf-prepush: faulted: {reason}")
            )

        return _module_phase(module, executor, settings=settings, scope=scope).map(absent)

    baseline = next((coordinate.ok for name, done in lanes if name == "buf-baseline" and (coordinate := _resolved(done, module)).is_ok()), "")

    def unchanged(done: Completed) -> Completed:
        current = _resolved(done, module).default_value("")
        if done.status not in {RailStatus.OK, RailStatus.EMPTY} or current == baseline:
            return done
        reason = f"default label moved from {baseline!r} to {current!r} before publish"
        return msgspec.structs.replace(
            done, status=RailStatus.FAULTED, stderr=reason.encode(), notes=(*done.notes, f"buf-prepush: faulted: {reason}")
        )

    return _baseline_phase(module, executor, present=True, settings=settings, scope=scope).map(unchanged)


def _derive_check(module: str, scratch: Path, fqn: str) -> Check:
    return Check(tool=_ROWS["buf-jsonschema", Mode.STAGE], args=ToolArgs(input=module, output=str(scratch / _SCHEMAS), fqn=fqn))


def _projection(fqn: str, done: Completed, scratch: Path) -> Result[bytes, str]:
    target = scratch / _SCHEMAS / f"{fqn}{_BUNDLE}"
    if done.status in {RailStatus.OK, RailStatus.EMPTY} and target.is_file():
        return Ok(target.read_bytes())
    reason = done.stderr[-_TAIL:].decode(errors="replace").strip() or "; ".join(done.notes) or f"lane {done.status.value}"
    return Error(f"{fqn}: {reason}")


def _roster_specs(template: _Templates, scratch: Path) -> tuple[_RosterSpec, ...]:
    """Deduplicate the exact configured plugin filters into Buf-owned closure images.

    Returns:
        One scratch descriptor-image specification per distinct plugin emission policy.
    """
    seen: set[tuple[str, tuple[str, ...], frozenset[_Kind], bool, bool]] = set()
    specs: list[_RosterSpec] = []
    for plugin in template.main.plugins:
        key = (plugin.language, plugin.types, plugin.kinds, plugin.include_imports, plugin.include_wkt)
        if not plugin.language or not plugin.kinds or not plugin.types or key in seen:
            continue
        seen.add(key)
        identity = f"{plugin.language}-{len(specs) + 1:02d}"
        specs.append(
            _RosterSpec(
                language=plugin.language,
                identity=identity,
                output=scratch / "roster" / f"{identity}.binpb",
                roots=plugin.types,
                kinds=plugin.kinds,
                include_imports=plugin.include_imports,
                include_wkt=plugin.include_wkt,
            )
        )
    return tuple(specs)


def _roster_check(spec: _RosterSpec) -> Check:
    base = _ROWS["buf-build", Mode.QUERY]
    tool = msgspec.structs.replace(base, name="buf-roster", command=(*base.command, "{targets*}"))
    targets = tuple(token for root in spec.roots for token in ("--type", root))
    return Check(tool=tool, args=ToolArgs(output=str(spec.output), targets=targets, fqn=spec.identity))


def _roster_phase(
    done: _Lanes, template: _Templates, scratch: Path, executor: Executor, *, settings: AssaySettings, scope: ArtifactScope
) -> Result[tuple[_Lanes, _RosterFiles], Fault]:
    specs = _roster_specs(template, scratch)
    if specs:
        (scratch / "roster").mkdir(parents=True, exist_ok=True)
    checks = tuple(_roster_check(spec) for spec in specs)

    def project(played: _Lanes) -> tuple[_Lanes, _RosterFiles]:
        files: dict[str, list[_File]] = {}
        for spec, (_, completed) in zip(specs, played, strict=True):
            if completed.status in {RailStatus.OK, RailStatus.EMPTY} and spec.output.is_file():
                filtered = _files(spec.output)
                selected = tuple(
                    file
                    for file in filtered
                    if (spec.include_imports or any(_file_owns(file, root) for root in spec.roots))
                    and (spec.include_wkt or file.package != "google.protobuf")
                )
                files.setdefault(spec.language, []).extend(_emitted(file, spec.kinds) for file in selected)
        return ((*done, *played), MappingProxyType({language: tuple(rows) for language, rows in files.items()}))

    return _phase(executor, checks, lambda _chk: None, settings=settings, scope=scope).map(project)


def _derive_phase(
    root: Path, scratch: Path, done: _Lanes, executor: Executor, *, settings: AssaySettings, scope: ArtifactScope
) -> Result[tuple[_Lanes, _Projections], Fault]:
    projector = next((gate.projector for _, played in done for gate in (_gate(played),) if isinstance(gate, _Probe)), "")
    refusal = "" if projector else f"{JSONSCHEMA_PLUGIN} is not resolvable; {_MACHINE_HINT}"
    fqns = _targets(root, scratch / _IMAGE)
    checks = tuple(_derive_check(CORPUS, scratch, fqn) for fqn in fqns)
    if checks and not refusal:
        (scratch / _SCHEMAS).mkdir(parents=True, exist_ok=True)
    return _phase(executor, checks, lambda chk: _seat(chk, RailStatus.UNSUPPORTED, refusal) if refusal else None, settings=settings, scope=scope).map(
        lambda played: (
            (*done, *played),
            MappingProxyType({fqn: _projection(fqn, lane, scratch) for fqn, (_, lane) in zip(fqns, played, strict=True)}),
        )
    )


def _check_lanes(
    root: Path, template: _Templates, scratch: Path, settings: AssaySettings, scope: ArtifactScope, executor: Executor
) -> Result[_Lanes, Fault]:
    gated = frozenset(lane.name for lane in _LANES if lane.gated)
    freshness = Check(tool=_ROWS["freshness-gate", Mode.QUERY], thunk=_freshness(root, scratch, out_dirs(template)))

    def gate_phase(probe: Completed) -> Result[_Lanes, Fault]:
        missed = probe.status is not RailStatus.OK
        lanes = tuple(_lane_check(lane, scratch) for lane in _LANES)

        def seated(chk: Check) -> Completed | None:
            return _seat(chk, RailStatus.SKIP, "plugin probe missed") if missed and chk.tool.name in gated else None

        return _phase(executor, lanes, seated, settings=settings, scope=scope).map(lambda done: (("plugin-probe", probe), *done))

    def post_phase(derived: tuple[_Lanes, _Projections, _RosterFiles]) -> Result[_Lanes, Fault]:
        done, projections, roster_files = derived
        generated = dict(done)["buf-generate"]
        reason = "" if generated.status in {RailStatus.OK, RailStatus.EMPTY} else "scratch generation did not complete"
        corpus = Check(tool=_ROWS["corpus-gate", Mode.CHECK], thunk=_corpus(root, scratch / _IMAGE, template, projections, roster_files))

        def seated(chk: Check) -> Completed | None:
            return _seat(chk, RailStatus.SKIP, reason) if reason and chk is freshness else None

        return _phase(executor, (corpus, freshness), seated, settings=settings, scope=scope).map(lambda post: (*done, *post))

    return (
        _probe(root, template, executor, settings=settings, scope=scope)
        .bind(gate_phase)
        .bind(lambda done: _derive_phase(root, scratch, done, executor, settings=settings, scope=scope))
        .bind(
            lambda derived: _roster_phase(derived[0], template, scratch, executor, settings=settings, scope=scope).map(
                lambda rostered: (rostered[0], derived[1], rostered[1])
            )
        )
        .bind(post_phase)
    )


def _generate_run(
    root: Path, template: _Templates, config: _BufConfig, scratch: Path, settings: AssaySettings, scope: ArtifactScope, executor: Executor
) -> Result[Report, Fault]:
    build = _lane_check(_LANES[0], scratch)
    write = _lane_check(_LANES[-1], scratch)

    def build_phase(probe: Completed) -> Result[_Lanes, Fault]:
        if probe.status is not RailStatus.OK:
            return Error(
                Fault(("buf", "generate"), RailStatus.UNSUPPORTED, f"plugin probe: {probe.stderr.decode(errors='replace') or 'missed'}"[:_TAIL])
            )
        return _phase(executor, (build,), lambda _chk: None, settings=settings, scope=scope).map(lambda done: (("plugin-probe", probe), *done))

    def preflight_phase(derived: tuple[_Lanes, _Projections, _RosterFiles]) -> Result[tuple[_Lanes, _Projections, _RosterFiles], Fault]:
        done, projections, roster_files = derived
        built = dict(done)["buf-build"]
        rostered = tuple(played for name, played in done if name.startswith("buf-roster:"))
        reason = (
            "descriptor build did not complete"
            if built.status not in {RailStatus.OK, RailStatus.EMPTY}
            else "one or more exact roster images did not complete"
            if any(played.status not in {RailStatus.OK, RailStatus.EMPTY} for played in rostered)
            else ""
        )
        gate = Check(tool=_ROWS["corpus-gate", Mode.CHECK], thunk=_preflight(root, scratch / _IMAGE, template, projections, roster_files))
        return _phase(executor, (gate,), lambda chk: _seat(chk, RailStatus.SKIP, reason) if reason else None, settings=settings, scope=scope).map(
            lambda post: ((*done, *post), projections, roster_files)
        )

    def write_phase(preflighted: tuple[_Lanes, _Projections, _RosterFiles]) -> Result[tuple[_Lanes, _Projections, _RosterFiles], Fault]:
        done, projections, roster_files = preflighted
        gate = dict(done)["corpus-gate"]
        reason = "" if gate.status is RailStatus.OK else "corpus preflight did not pass"
        return _phase(executor, (write,), lambda chk: _seat(chk, RailStatus.SKIP, reason) if reason else None, settings=settings, scope=scope).map(
            lambda main: ((*done, *main), projections, roster_files)
        )

    def emit_phase(derived: tuple[_Lanes, _Projections, _RosterFiles]) -> Result[_Lanes, Fault]:
        done, projections, roster_files = derived
        written = dict(done)["buf-generate"]
        reason = "" if written.status in {RailStatus.OK, RailStatus.EMPTY} else "regeneration did not complete"
        emit = Check(tool=_ROWS["corpus-emit", Mode.WRITE], thunk=_emit(root, scratch / _IMAGE, template, projections, roster_files))
        return _phase(executor, (emit,), lambda chk: _seat(chk, RailStatus.SKIP, reason) if reason else None, settings=settings, scope=scope).map(
            lambda post: (*done, *post)
        )

    def run() -> Result[Report, Fault]:
        return (
            _probe(root, template, executor, settings=settings, scope=scope)
            .bind(build_phase)
            .bind(lambda done: _derive_phase(root, scratch, done, executor, settings=settings, scope=scope))
            .bind(
                lambda derived: _roster_phase(derived[0], template, scratch, executor, settings=settings, scope=scope).map(
                    lambda rostered: (rostered[0], derived[1], rostered[1])
                )
            )
            .bind(preflight_phase)
            .bind(write_phase)
            .bind(emit_phase)
            .map(lambda done: _report("generate", done, str(scratch), config.module))
        )

    manifest = load_manifest(root / CORPUS).default_value(Manifest(entries=()))
    return _generation_transaction(root, template, manifest).recover().bind(lambda _direction: run())


def _publish_run(
    root: Path, template: _Templates, config: _BufConfig, scratch: Path, settings: AssaySettings, scope: ArtifactScope, executor: Executor
) -> Result[Report, Fault]:
    def push_phase(lanes: _Lanes) -> Result[Report, Fault]:
        preflight = _report("publish", lanes, str(scratch), config.module)
        if preflight.status is not RailStatus.OK:
            return Ok(preflight)
        bootstrap = dict(lanes)["buf-module"].status is RailStatus.SKIP
        create = ("--create", "--create-visibility", "public", "--create-default-label", _DEFAULT_LABEL) if bootstrap else ()
        push = Check(tool=_ROWS["buf-push", Mode.PUBLISH], args=ToolArgs(flags=create, target=_DEFAULT_LABEL))

        def revalidated(prepush: Completed) -> Result[Report, Fault]:
            verified = (*lanes, ("buf-prepush", prepush))
            if _verdict(prepush) not in {RailStatus.OK, RailStatus.SKIP}:
                return Ok(_report("publish", verified, str(scratch), config.module))

            def admitted(done: Completed) -> Completed:
                coordinate = _published(done, config.module)
                if coordinate.is_ok():
                    return done
                if done.status not in {RailStatus.OK, RailStatus.EMPTY}:
                    return msgspec.structs.replace(done, status=RailStatus.FAULTED) if done.status is RailStatus.FAILED else done
                reason = coordinate.error
                return msgspec.structs.replace(
                    done, status=RailStatus.FAULTED, stderr=reason.encode(), notes=(*done.notes, f"buf-push: faulted: {reason}")
                )

            def confirmed(done: Completed) -> Result[Report, Fault]:
                pushed = (*verified, ("buf-push", done))
                coordinate = _published(done, config.module)
                if coordinate.is_error():
                    return Ok(_report("publish", pushed, str(scratch), config.module))
                return _verify_phase(config.module, coordinate.ok, executor, settings=settings, scope=scope).map(
                    lambda seen: _report("publish", (*pushed, ("buf-verify", seen)), str(scratch), config.module)
                )

            return (
                _phase(executor, (push,), lambda _chk: None, settings=settings, scope=scope)
                .map(itemgetter(0))
                .map(itemgetter(1))
                .map(admitted)
                .bind(confirmed)
            )

        return _prepush_phase(config.module, lanes, executor, settings=settings, scope=scope).bind(revalidated)

    def custody(auth: Completed) -> Result[_Lanes, Fault]:
        return _module_phase(config.module, executor, settings=settings, scope=scope).bind(
            lambda module: _baseline_phase(config.module, executor, present=module[1], settings=settings, scope=scope).map(
                lambda baseline: (("buf-auth", auth), ("buf-module", module[0]), ("buf-baseline", baseline))
            )
        )

    def gated(auth: Completed) -> Result[Report, Fault]:
        if _verdict(auth) is not RailStatus.OK:
            return Ok(_report("publish", (("buf-auth", auth),), str(scratch), config.module))
        return custody(auth).bind(
            lambda custodial: _check_lanes(root, template, scratch, settings, scope, executor).bind(lambda lanes: push_phase((*custodial, *lanes)))
        )

    return _auth_phase(config.module, executor, settings=settings, scope=scope).bind(gated)


def _leased(settings: AssaySettings, run: Callable[[], Result[Report, Fault]]) -> Result[Report, Fault]:
    return leased(_LEASE, lambda _held: run(), settings=settings, run_id=settings.run_id, mode="exclusive")


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(settings: AssaySettings, scope: ArtifactScope, params: ContractsParams, executor: Executor) -> Result[Report, Fault]:
    """Run every local proof lane over the contracts plane under one lease: build, lint, format, scratch generation, and the gates.

    A template-plugin miss skips scratch generation and freshness; a projector miss seats the projection lanes UNSUPPORTED
    and the audit names each underivable seam. Executed lane verdicts read their exit code, a defect exit parses to rows,
    and any other failure keeps its siblings' rows.

    Returns:
        Folded report with ``ContractsRun`` detail, or a template, scratch, lease, or spawn fault.
    """
    _ = params
    root = Path(str(settings.root))
    return read_template(root).bind(
        lambda template: _read_config(root).bind(
            lambda config: _scratch(settings, scope).bind(
                lambda scratch: _leased(
                    settings,
                    lambda: _check_lanes(root, template, scratch, settings, scope, executor).map(
                        lambda lanes: _report("check", lanes, str(scratch), config.module)
                    ),
                )
            )
        )
    )


def generate(settings: AssaySettings, scope: ArtifactScope, params: ContractsParams, executor: Executor) -> Result[Report, Fault]:
    """Build and validate one complete staged contracts image, then commit every package and schema target transactionally.

    A template-plugin miss refuses outright; a projector miss lands everything but the proto-derived schemas, each named as a finding.

    Returns:
        Folded report with ``ContractsRun`` detail, or a template, scratch, probe, lease, or spawn fault.
    """
    _ = params
    root = Path(str(settings.root))
    return read_template(root).bind(
        lambda template: _read_config(root).bind(
            lambda config: _scratch(settings, scope).bind(
                lambda scratch: _leased(settings, lambda: _generate_run(root, template, config, scratch, settings, scope, executor))
            )
        )
    )


def publish(settings: AssaySettings, scope: ArtifactScope, params: ContractsParams, executor: Executor) -> Result[Report, Fault]:
    """Run the full contracts gate, then publish only the named estate module to the public BSR under the same lease.

    The credential resolves first, since module absence reads identically with and without one and would otherwise elect the
    bootstrap create blind. Existing modules re-resolve the unchanged default-label commit immediately before push; bootstrap
    re-proves exact absence; the pushed coordinate is read back off the default label. Authentication, network, malformed
    resolver output, a moved label, stale generated trees, and every other non-clean lane prevent the push.

    Returns:
        Folded report with the immutable published commit in ``ContractsRun``, or a config, scratch, lease, or spawn fault.
    """
    _ = params
    root = Path(str(settings.root))
    return read_template(root).bind(
        lambda template: _read_config(root).bind(
            lambda config: _scratch(settings, scope).bind(
                lambda scratch: _leased(settings, lambda: _publish_run(root, template, config, scratch, settings, scope, executor))
            )
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CORPUS",
    "MANIFEST",
    "ROSTER_BEGIN",
    "ROSTER_END",
    "SCHEMA",
    "TEMPLATE",
    "Asset",
    "Actor",
    "ActorBinding",
    "ActorDirection",
    "ApplicationAuthority",
    "Authority",
    "BackendGenerationFacts",
    "BlockedReadiness",
    "Case",
    "CloudEventDefinition",
    "ClientRequestActor",
    "ClientResponseActor",
    "ContractsParams",
    "CorpusOracleFinding",
    "CorpusOracleReceipt",
    "Definition",
    "Distribution",
    "DistributionKind",
    "DomainAuthority",
    "Entry",
    "EntryClass",
    "Fingerprint",
    "FingerprintAlgorithm",
    "InfrastructureAuthority",
    "LawFormat",
    "LawDefinition",
    "Manifest",
    "MessageActor",
    "Oracle",
    "ProtoDefinition",
    "ProtoFraming",
    "PublisherAuthority",
    "PublisherDefinition",
    "PublisherLicense",
    "PublisherOrigin",
    "PythonPackageResource",
    "Readiness",
    "ProofVector",
    "SchemaDefinition",
    "SchemaFraming",
    "ServerRequestActor",
    "ServerResponseActor",
    "TypeScriptJsonModule",
    "SpecimenAsset",
    "Support",
    "SupportKind",
    "ExpectedAsset",
    "FactsFormat",
    "VerifiedReadiness",
    "actor_key",
    "check",
    "derived_schema",
    "generate",
    "load_manifest",
    "out_dirs",
    "publish",
    "prove_case",
    "read_template",
]
