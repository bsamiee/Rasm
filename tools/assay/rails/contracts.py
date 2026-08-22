"""Drive the contracts plane through buf: the gate lanes, the corpus audit, scratch-regeneration freshness, and the committed regeneration.

``check`` probes every plugin binary, fans ``buf build``/``lint``/``format``/``breaking``/``generate``-to-scratch under
one repo lease, projects every `proto:`-derived seam schema into scratch through ``protoc-gen-jsonschema``, audits the
corpus registry against its schema, disk, anchors, derivations, emitted rosters, and the built descriptor set, then
proves the committed out roots fresh by byte diff; ``generate`` probes, refuses on a template-plugin miss, rewrites the
out roots with ``buf generate --clean``, and emits the roster block into every generated package's `.api` catalog and
the derived schema file into every seam under the same lease. The msgspec manifest model here is the registry's one
authority: ``manifest.schema.json`` derives from it and the audit rows carry every cross-field invariant.
"""

from collections.abc import Callable, Iterable, Mapping
from dataclasses import dataclass
import difflib
import filecmp
from importlib import import_module
import importlib.util
from operator import itemgetter
import os
from pathlib import Path, PurePosixPath
import re
from types import MappingProxyType
from typing import Annotated, ClassVar, Final, Literal, override
lazy import shutil  # PATH resolution on the plugin probe alone

from expression import Error, Ok, Result  # beartype resolves rail annotations at import time
from expression.collections import block
from expression.extra.result import sequence
import msgspec
import xxhash
lazy import jsonschema  # cold; the corpus gate alone validates REAL .json assets against their seam schema
lazy from protobuf.wkt import DescriptorProto, FileDescriptorSet  # descriptor decode on the corpus gate alone
lazy from referencing import Registry, Resource
lazy from referencing.exceptions import Unresolvable
lazy from referencing.jsonschema import DRAFT202012
lazy from ruamel.yaml import YAML

from tools.assay.composition.catalog import JSONSCHEMA_PLUGIN, select
from tools.assay.composition.settings import AssaySettings  # beartype resolves rail annotations at import time
from tools.assay.composition.store import ArtifactScope  # beartype resolves rail annotations at import time
from tools.assay.core.exec import Executor  # beartype resolves the executor-port annotation at runtime
from tools.assay.core.govern import leased
from tools.assay.core.model import (
    Artifact,
    ArtifactKind,
    BaseParams,
    Check,
    Claim,
    Completed,  # thunks and folds annotate the receipt at runtime
    ContractsRun,
    Fault,  # beartype resolves Result[Report, Fault] under PEP 649 at import time
    InprocThunk,  # beartype resolves the thunk-factory return annotation at import time
    Language,
    Match,
    Mode,
    RailStatus,
    receipt,
    Report,  # beartype resolves Report in return annotations at import time
    Step,
    Tool,
    ToolArgs,
)
from tools.assay.core.routing import Routed, Scope
from tools.assay.diagnostics import cap_note, fold


# --- [TYPES] ----------------------------------------------------------------------------

type EntryClass = Literal["infrastructure", "domain", "vendored"]
type Pin = Literal["REAL", "DESIGN-PIN"]
type Proof = Literal["byte-identity", "structural", "value-parity", "publisher-bytes", "unproven"]
type Role = Literal["wire-bytes", "canonical-json", "plane", "vector", "publisher-bytes", "definition"]
type ProtoFraming = Literal["proto-binary", "proto-json", "canonical-frame"]
type SchemaFraming = Literal["framed-binary", "proto-json", "canonical-json", "msgpack", "proto-binary", "container"]
type _Kind = Literal["message", "enum", "service"]
type _Document = dict[str, object]
type _Projections = Mapping[str, Result[bytes, str]]
type _Rule = Callable[[_Corpus], Iterable[_Finding]]
# One audit clause: (broken predicate, rule id, detail, repair); the predicate closes over the entry or the blocker sentence.
type _Clause[T] = tuple[Callable[[T], bool], str, str, str]
type _Lanes = tuple[tuple[str, Completed], ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

CORPUS: Final = "tests/contracts"
MANIFEST: Final = "manifest.json"
SCHEMA: Final = "manifest.schema.json"
TEMPLATE: Final = "buf.gen.yaml"
ROSTER_BEGIN: Final = "<!-- roster:begin -->"
ROSTER_END: Final = "<!-- roster:end -->"
_CONFIG: Final = "buf.yaml"
_LOCK: Final = "buf.lock"
_SCHEMA_ANCHOR: Final = "https://json-schema.org/draft/2020-12/schema"
_LEASE: Final = "contracts"
_VENDOR: Final = "vendor"
_ESTATE: Final = f"{CORPUS}/proto"
_IMAGE: Final = "image.binpb"
_GEN: Final = "gen"
_SCHEMAS: Final = "schema"
_DIFF: Final = "freshness.diff"
_BUNDLE: Final = ".jsonschema.strict.bundle.json"
_JSON_FRAMINGS: Final[frozenset[SchemaFraming]] = frozenset(("proto-json", "canonical-json"))
_NO_PROJECTIONS: Final[_Projections] = MappingProxyType({})
# The three roots beside the seam directories and the three files beside them; the audit admits nothing else at the corpus root.
_ROOTS: Final[frozenset[str]] = frozenset((".api", "proto", _VENDOR))
_ROOT_FILES: Final[frozenset[str]] = frozenset(("README.md", MANIFEST, SCHEMA))
_BLOCKER_FLOOR: Final = 24
_LAW_CAP: Final = 240
_DIFF_LINES: Final = 400
_STALE_CAP: Final = 200
_TAIL: Final = 512
_CLOSED: Final = re.compile(r"\b(DISCHARGED|resolved|closed)\b", re.IGNORECASE)
_ANCHOR: Final = re.compile(r"^(csharp|python|typescript):([A-Za-z0-9_.]+)/([A-Za-z0-9_./-]+)#([A-Z0-9_]+)$")
_TIER0: Final = re.compile(r"^tier0:(ARCHITECTURE|RULINGS)#\[(\d{2})\]-\[([A-Z0-9_]+)\]$")
_LAW_ANCHOR: Final = re.compile(r"^([A-Za-z0-9_./-]+\.md)#\[(\d{2})\]-\[([A-Z0-9_]+)\]$")
_DERIVED: Final = re.compile(r"^(proto|msgspec):([A-Za-z0-9_.]+)$")
_DIFF_HEADER: Final = re.compile(r"^\+\+\+ (\S+)")
_ROUTED: Final[Routed] = Routed(language=Language.PROTO, scope=Scope.CHANGED)
# A missing plugin names its installer by where the template seats it: venv and node binaries are repo-installed, the rest ride the machine estate.
_HINTS: Final[tuple[tuple[str, str], ...]] = ((".venv/", "uv sync"), ("node_modules/", "pnpm install"))
_MACHINE_HINT: Final = f"the machine estate supplies protoc, the grpc plugins, and {JSONSCHEMA_PLUGIN}"

# --- [MODELS] ---------------------------------------------------------------------------

# --- [MANIFEST]
# Wire casing law: single-word fields ride as spelled, multi-word fields camelCase (`lawAnchors`, `derivedFrom`), and the
# one keyword collision renames explicitly (`class`); every record forbids unknown fields so a stray key is a decode defect.


class _Record(msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True, rename="camel"):
    """Manifest record policy: frozen, keyword-only, camelCase wire, unknown fields refused."""


class Asset(_Record, frozen=True, kw_only=True):
    """One frozen corpus asset: its corpus-relative path, role, size, and seed-0 XxHash128 lowercase-hex digest."""

    path: str
    role: Role
    bytes: Annotated[int, msgspec.Meta(ge=0)]
    digest: Annotated[str, msgspec.Meta(pattern=r"^[0-9a-f]{32}$")]


class ProtoDefinition(_Record, frozen=True, kw_only=True, tag_field="kind", tag="proto"):
    """A definition naming `rasm.contracts.<family>.v1.<Message>` descriptors and the framing their bytes ride."""

    message: tuple[str, ...]
    framing: ProtoFraming


class SchemaDefinition(_Record, frozen=True, kw_only=True, tag_field="kind", tag="schema"):
    """A DERIVED JSON Schema 2020-12 file whose `$id` is its file name: `derived_from` spells `proto:<fqn>` or `msgspec:<dotted.path.Struct>`."""

    path: str
    framing: SchemaFraming
    derived_from: str = ""
    message: str = ""


class PublisherDefinition(_Record, frozen=True, kw_only=True, tag_field="kind", tag="publisher"):
    """A vendored publisher's own definition, named by its format."""

    format: str
    note: str = ""


class LawDefinition(_Record, frozen=True, kw_only=True, tag_field="kind", tag="law"):
    """A framing-law seam defined by one repo page cluster, anchored as `path#[NN]-[CLUSTER]`."""

    anchor: str


type Definition = ProtoDefinition | SchemaDefinition | PublisherDefinition | LawDefinition


class Case(_Record, frozen=True, kw_only=True):
    """One discriminant case under an entry: its own producer and consumer anchors, blockers, and frozen assets."""

    id: str
    producer: tuple[str, ...] = ()
    consumers: tuple[str, ...] = ()
    blocker: tuple[str, ...] = ()
    assets: tuple[Asset, ...] = ()


class Entry(_Record, frozen=True, kw_only=True):
    """One registered seam: identity, class, anchors, pin, proof oracle, definition, cases, assets, blockers, and its one-sentence law."""

    id: str
    seam: str
    entry_class: EntryClass = msgspec.field(name="class")
    minters: tuple[str, ...] = ()
    producer: tuple[str, ...] = ()
    consumers: tuple[str, ...]
    pin: Pin
    proof: Proof
    definition: Definition
    cases: tuple[Case, ...] = ()
    assets: tuple[Asset, ...] = ()
    blocker: tuple[str, ...] = ()
    law: str
    law_anchors: tuple[str, ...]


class Manifest(_Record, frozen=True, kw_only=True):
    """The corpus registry: one entry per seam under one schema version."""

    version: Literal[1]
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
    exclude_types: tuple[str, ...] = ()


class _Plugin(_Row, frozen=True, kw_only=True):
    out: str
    local: str | tuple[str, ...] = ""
    protoc_builtin: str = ""
    protoc_path: str | tuple[str, ...] = ""
    remote: str = ""
    opt: str | tuple[str, ...] = ()
    strategy: str = ""
    types: tuple[str, ...] = ()
    exclude_types: tuple[str, ...] = ()
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
        """The branch the out root lands under: the second segment of a `libs/<language>/...` out, else ``""``."""
        parts = PurePosixPath(self.out).parts
        return parts[1] if len(parts) > 1 and parts[0] == "libs" else ""

    def generates(self, package: str) -> bool:
        """Decide whether the row emits a file of ``package``; a type token covers the package or a name beneath it.

        Returns:
            True when no ``types`` row excludes the package and no ``exclude_types`` row covers it.
        """
        covered = not self.types or any(_covers(token, package) for token in self.types)
        return covered and not any(_covers(token, package) for token in self.exclude_types)


class _BufGen(_Row, frozen=True, kw_only=True):
    """Root `buf.gen.yaml` as the rail reads it: the clean sweep, managed mode, the workspace input, and every plugin row."""

    version: Literal["v2"]
    clean: bool = False
    managed: _Managed = msgspec.field(default_factory=_Managed)
    inputs: tuple[_Input, ...] = ()
    plugins: tuple[_Plugin, ...]


class _BufConfig(msgspec.Struct, frozen=True, kw_only=True):
    """Root `buf.yaml` as the lock row reads it: the `deps` roster alone; every other key is the buf lanes' concern."""

    deps: tuple[str, ...] = ()


# --- [GATES]
# One JSON document per INPROC gate receipt: the thunk encodes it onto stdout, the rail decodes it through the one
# tagged decoder to project Match rows and the ContractsRun detail — no second channel beside the receipt.


class _Finding(msgspec.Struct, frozen=True, gc=False):
    rule: str
    subject: str
    detail: str
    repair: str
    path: str = ""
    token: str = ""


class _File(msgspec.Struct, frozen=True, gc=False):
    name: str
    package: str
    messages: tuple[str, ...] = ()
    enums: tuple[str, ...] = ()
    services: tuple[str, ...] = ()


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
    """One buf gate lane: the catalog row key, its scratch-relative ``{output}`` hole, and whether a plugin miss skips it."""

    name: str
    mode: Mode
    output: str = ""
    gated: bool = False


@dataclass(frozen=True, slots=True)
class _Roster:
    """One generated package's roster: the `.api` catalog the block is emitted into, the censused descriptor kinds, and the name speller."""

    language: str
    catalog: str
    kinds: frozenset[_Kind]
    name: Callable[[_Kind, str], str]


@dataclass(frozen=True, slots=True)
class _Corpus:
    """The audited corpus: roots, the decoded registry, the built descriptor files, the template, the resolved message names, and the projections."""

    repo: Path
    manifest: Manifest
    files: tuple[_File, ...]
    template: _BufGen
    known: frozenset[str]
    projections: _Projections = _NO_PROJECTIONS

    @property
    def root(self) -> Path:
        return self.repo / CORPUS

    def seam_dir(self, entry: Entry) -> Path:
        return self.root / _VENDOR / entry.seam if entry.entry_class == "vendored" else self.root / entry.seam


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
# The buf read fan in report order: build bares the image the corpus gate resolves against, generate regenerates into scratch.
_LANES: Final[tuple[_Lane, ...]] = (
    _Lane("buf-build", Mode.QUERY, output=_IMAGE),
    _Lane("buf-lint", Mode.CHECK),
    _Lane("buf-format", Mode.CHECK),
    _Lane("buf-breaking", Mode.CHECK),
    _Lane("buf-generate", Mode.STAGE, output=_GEN, gated=True),
)
_GATE: Final[msgspec.json.Decoder[_Gate]] = msgspec.json.Decoder(_Gate)
_GATED: Final[frozenset[str]] = frozenset(("plugin-probe", "corpus-gate", "freshness-gate", "corpus-emit"))
_ENCODER: Final[msgspec.json.Encoder] = msgspec.json.Encoder()
_MANIFEST: Final[msgspec.json.Decoder[Manifest]] = msgspec.json.Decoder(Manifest)
# Entry law as data: identity ↔ seam, class ↔ anchor shape and publisher posture, pin ↔ assets and blockers, proof, the one-sentence law.
_ENTRY_LAW: Final[tuple[_Clause[Entry], ...]] = (
    (
        lambda e: e.seam != e.id.lower().replace("_", "-"),
        "seam-derivation",
        "seam is not id.lower() with _ → -",
        "spell the seam as the derived token, or rename the id",
    ),
    (
        lambda e: e.entry_class == "infrastructure" and len(e.minters) < 2,
        "class-minters",
        "infrastructure names fewer than two minters",
        "name every minting anchor, or reclass as domain",
    ),
    (
        lambda e: e.entry_class == "infrastructure" and bool(e.producer),
        "class-producer",
        "infrastructure names a producer",
        "drop the producer; minters mint the shape",
    ),
    (lambda e: e.entry_class == "domain" and not e.producer, "class-producer", "domain names no producer", "name the owning producer anchor"),
    (lambda e: e.entry_class == "domain" and bool(e.minters), "class-minters", "domain names minters", "drop the minters; one producer emits"),
    (
        lambda e: e.entry_class == "vendored" and bool(e.minters or e.producer),
        "class-anchors",
        "vendored names minters or a producer",
        "drop them; the publisher authored the bytes",
    ),
    (lambda e: e.entry_class == "vendored" and e.pin != "REAL", "class-pin", "vendored is not REAL", "vendored bytes are frozen publisher bytes"),
    (
        lambda e: e.entry_class == "vendored" and e.proof != "publisher-bytes",
        "class-proof",
        "vendored proof is not publisher-bytes",
        "publisher bytes prove by identity",
    ),
    (
        lambda e: (e.entry_class == "vendored") != isinstance(e.definition, PublisherDefinition),
        "class-definition",
        "publisher definitions ride vendored entries alone",
        "name the publisher format, or reclass the entry",
    ),
    (
        lambda e: e.pin == "REAL" and not (e.assets or any(c.assets for c in e.cases)),
        "pin-assets",
        "REAL carries no asset",
        "land the frozen asset with bytes and digest, or pin DESIGN-PIN",
    ),
    (lambda e: e.pin == "REAL" and bool(e.blocker), "pin-blocker", "REAL carries a blocker", "delete the blocker on the REAL flip"),
    (
        lambda e: e.pin == "DESIGN-PIN" and not (e.blocker or any(c.blocker for c in e.cases)),
        "pin-blocker",
        "DESIGN-PIN names no blocker",
        "state what no source tree runs yet",
    ),
    (
        lambda e: e.pin == "DESIGN-PIN" and bool(e.assets or any(c.assets for c in e.cases)),
        "pin-assets",
        "DESIGN-PIN carries assets",
        "delete the fabricated asset; bytes land on the REAL flip",
    ),
    (
        lambda e: e.pin == "REAL" and e.proof == "unproven",
        "proof-unproven",
        "REAL entry names no proof oracle",
        "select the oracle the frozen bytes prove under, or pin DESIGN-PIN",
    ),
    (lambda e: not e.law.strip(), "law-empty", "law sentence is empty", "state the one-sentence law"),
    (lambda e: len(e.law) > _LAW_CAP, "law-length", f"law exceeds {_LAW_CAP} characters", "move the detail to the owning page; keep one sentence"),
    (lambda e: not e.law_anchors, "law-anchors", "law names no anchor", "name the owning page cluster"),
)
_BLOCKER_LAW: Final[tuple[_Clause[str], ...]] = (
    (lambda s: len(s) < _BLOCKER_FLOOR, "blocker-short", f"blocker shorter than {_BLOCKER_FLOOR} characters", "name what no source tree runs"),
    (lambda s: _CLOSED.search(s) is not None, "blocker-closed", "blocker reads as discharged", "a closed blocker deletes with the REAL flip"),
)
# Anchor cells: (label, admits repo-relative paths, reader) per entry and per case.
_ENTRY_CELLS: Final[tuple[tuple[str, bool, Callable[[Entry], tuple[str, ...]]], ...]] = (
    ("minters", False, lambda e: e.minters),
    ("producer", False, lambda e: e.producer),
    ("consumers", True, lambda e: e.consumers),
    ("lawAnchors", False, lambda e: e.law_anchors),
)
_CASE_CELLS: Final[tuple[tuple[str, bool, Callable[[Case], tuple[str, ...]]], ...]] = (
    ("producer", False, lambda c: c.producer),
    ("consumers", True, lambda c: c.consumers),
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


def derived_schema(struct: type[msgspec.Struct] = Manifest, *, identity: str = "") -> bytes:
    """Render a msgspec owner's JSON Schema 2020-12 in the one canonical byte form the gate checks files against.

    ``Manifest`` derives ``manifest.schema.json``; a `msgspec:` seam definition derives its file under the file name as ``$id``.

    Returns:
        Deterministically keyed, two-space indented JSON bytes with a trailing newline.
    """
    document = {"$schema": _SCHEMA_ANCHOR, **({"$id": identity} if identity else {}), **msgspec.json.schema(struct)}
    return msgspec.json.format(msgspec.json.encode(document, order="deterministic"), indent=2) + b"\n"


def read_template(root: Path) -> Result[_BufGen, Fault]:
    """Decode ``root/buf.gen.yaml`` into the typed template the plugin probe, freshness gate, and roster emission derive from.

    Returns:
        The template, or a ``FAULTED`` parse fault for a missing, unreadable, or unmodelled document.
    """
    path = root / TEMPLATE
    try:
        return Ok(msgspec.convert(YAML(typ="safe").load(path.read_text(encoding="utf-8")), _BufGen))
    except OSError as exc:
        return Error(Fault((TEMPLATE,), RailStatus.FAULTED, f"{Step.PARSE}: {TEMPLATE}: {exc}"))
    except msgspec.ValidationError as exc:
        return Error(Fault((TEMPLATE,), RailStatus.FAULTED, f"{Step.PARSE}: {TEMPLATE}: {exc}"))


def out_dirs(template: _BufGen) -> tuple[str, ...]:
    """Project the template's distinct outermost ``out`` roots in declaration order — a nested out rides its parent's walk.

    Returns:
        Repo-relative generated out roots, each holding generated files alone.
    """
    outs = tuple(dict.fromkeys(PurePosixPath(row.out).as_posix() for row in template.plugins))
    return tuple(out for out in outs if not any(other != out and PurePosixPath(out).is_relative_to(other) for other in outs))


def _covers(token: str, package: str) -> bool:
    return token == package or token.startswith(f"{package}.")


def _resolve(root: Path, spelling: str) -> str:
    # A repo-relative path resolves under root; a bare name resolves on PATH — the same two seams buf itself reads.
    if "/" in spelling:
        candidate = root / spelling
        return str(candidate) if candidate.is_file() and os.access(candidate, os.X_OK) else ""
    return shutil.which(spelling) or ""


def _hint(spelling: str) -> str:
    return next((hint for prefix, hint in _HINTS if spelling.startswith(prefix)), _MACHINE_HINT)


# --- [DESCRIPTORS]


def _dotted(prefix: str, rows: Iterable[DescriptorProto]) -> tuple[tuple[str, ...], tuple[str, ...]]:
    # Walks the nesting: (message names, enum names) in the proto dotted form under ``prefix``, skipping synthetic map entries.
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


def _files(image: Path) -> tuple[_File, ...]:
    rows: list[_File] = []
    for file in FileDescriptorSet.from_binary(image.read_bytes()).file:
        messages, enums = _dotted("", file.message_type)
        rows.append(
            _File(
                name=file.name,
                package=file.package,
                messages=messages,
                enums=(*enums, *(enum.name for enum in file.enum_type)),
                services=tuple(service.name for service in file.service),
            )
        )
    return tuple(rows)


# --- [ROSTERS]
# The roster is EMITTED: `generate` writes one table per generated package between the catalog's markers from the built
# descriptor set under the template's type filters, and `check` byte-compares that block; hand rows outside the markers
# are not censused.


def _ts_name(kind: _Kind, dotted: str) -> str:
    return dotted if kind == "service" else f"{dotted.replace('.', '_')}Schema"


def _py_name(kind: _Kind, dotted: str) -> str:
    _ = kind
    return dotted


def _cs_name(kind: _Kind, dotted: str) -> str:
    return dotted if kind == "service" else dotted.replace(".", ".Types.")


_ROSTERS: Final[dict[str, _Roster]] = {
    row.language: row
    for row in (
        _Roster("typescript", "libs/typescript/contracts/.api/rasm-ts-contracts.md", frozenset(("message", "enum", "service")), _ts_name),
        _Roster("python", "libs/python/contracts/.api/rasm-contracts.md", frozenset(("message", "service")), _py_name),
        _Roster("csharp", "libs/csharp/Rasm.Contracts/.api/rasm-contracts.md", frozenset(("message", "enum", "service")), _cs_name),
    )
}
_KINDS: Final[tuple[tuple[_Kind, Callable[[_File], tuple[str, ...]]], ...]] = (
    ("message", lambda file: file.messages),
    ("enum", lambda file: file.enums),
    ("service", lambda file: file.services),
)


def _generated(roster: _Roster, files: tuple[_File, ...], template: _BufGen) -> tuple[_File, ...]:
    rows = tuple(row for row in template.plugins if row.language == roster.language)
    return tuple(file for file in files if any(row.generates(file.package) for row in rows))


def _table(header: tuple[str, ...], rows: tuple[tuple[str, ...], ...]) -> str:
    # One padded markdown table in the estate's `[INDEX]`-first grammar; widths derive from the content, the index column
    # centres (`|  [01]   |`) and every other column left-aligns.
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


def _roster_block(roster: _Roster, files: tuple[_File, ...], template: _BufGen) -> str:
    # The body between the markers: a scope line and one table per generated package in descriptor order.
    tables = tuple(
        f"[ROSTER_SCOPE]: `{file.package}` — emitted by `assay contracts generate` from the built descriptor set; hand edits are overwritten\n\n"
        + _table(
            ("[INDEX]", "[NAME]", "[KIND]", "[FQN]"),
            tuple(
                (f"`{roster.name(kind, dotted)}`", kind, f"`{file.package}.{dotted}`")
                for kind, names in _KINDS
                if kind in roster.kinds
                for dotted in names(file)
            ),
        )
        for file in _generated(roster, files, template)
    )
    return "\n\n".join(tables) + "\n" if tables else ""


def _roster_span(text: str) -> tuple[int, int] | None:
    # (start, end) of the body between the two marker lines, or None when either marker is absent or misordered.
    begin, end = text.find(ROSTER_BEGIN), text.find(ROSTER_END)
    return (begin + len(ROSTER_BEGIN) + 1, end) if 0 <= begin < end else None


# --- [AUDIT]


def _finding(rule: str, subject: str, detail: str, repair: str, path: str = "", token: str = "") -> _Finding:
    return _Finding(rule=rule, subject=subject, detail=detail, repair=repair, path=path, token=token)


def _repeated(names: tuple[str, ...]) -> tuple[str, ...]:
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
    # jsonschema ships no py.typed: the SchemaError re-spells as text so nothing untyped crosses further in.
    try:
        jsonschema.Draft202012Validator.check_schema(schema)
    except jsonschema.exceptions.SchemaError as exc:
        return str(exc)[:_TAIL]
    return ""


def _schema_errors(schema: _Document, instance: object) -> tuple[str, ...]:
    # The validator trusts its schema, so an invalid one refuses before any instance walks it; `format` asserts under the
    # draft's checker because 2020-12 makes it annotation-only by default; a dangling `$ref` surfaces as its own row.
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
    # A derived document is self-contained (a bundle inlines its dependencies under `$defs`), so every `$ref` resolves inside
    # it; a pointer to nowhere or a foreign document surfaces at the definition, not at the first asset that walks it.
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
    # `msgspec:<module.path.Struct>`: the importable module first, else the module file under the repo root; the owner must be a Struct.
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
    # A seam schema DERIVES: a hand-written file is the deleted form, a derived file byte-equals its derivation, and an arm that
    # cannot derive yet names itself.
    match _derived(corpus, subject, definition, path):
        case Result(tag="ok", ok=derived):
            stale = (corpus.root / definition.path).read_bytes() != derived
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
    # A definition is a DERIVED, valid 2020-12 schema whose `$id` spells its file name and whose every `$ref` resolves within it.
    rel = definition.path
    path, expected = f"{CORPUS}/{rel}", PurePosixPath(rel).name
    match _document(corpus.root / rel):
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
    return tuple(dict.fromkeys(name for name in (definition.message, owner if arm == "proto" else "") if name))


def _unresolved(corpus: _Corpus, subject: str, messages: tuple[str, ...]) -> tuple[_Finding, ...]:
    # Resolution waits on the built descriptor set; without one the audit names that gap once instead of refusing every message.
    return tuple(
        _finding("message-unresolved", subject, f"{name} is not in the built descriptor set", "name a message the corpus declares, or land the proto")
        for name in messages
        if corpus.files and name not in corpus.known
    )


def _anchor(repo: Path, text: str, *, paths: bool) -> str:
    # Returns "" when the anchor resolves, else the reason; a consumer cell may also name a repo-relative path.
    match (_ANCHOR.match(text), _TIER0.match(text)):
        case (re.Match() as found, _):
            language, package, page, cluster = found.groups()
            return _cluster(repo / "libs" / language / package / ".planning" / f"{page}.md", rf"\[\d{{2}}\]-\[{re.escape(cluster)}\]")
        case (_, re.Match() as found):
            name, index, token = found.groups()
            return _cluster(repo / "libs" / ".planning" / f"{name}.md", rf"\[{index}\]-\[{re.escape(token)}\]")
        case _ if paths and not text.startswith("/") and (repo / text).exists():
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


def _rule_schema(corpus: _Corpus) -> Iterable[_Finding]:
    path = corpus.root / SCHEMA
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
        _finding("seam-duplicate", seam, "seam registered by more than one entry", "merge into one entry; a second producer is a second anchor")
        for seam in _repeated(tuple(e.seam for e in entries if e.entry_class != "vendored"))
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
        sentences = (*((entry.id, s) for s in entry.blocker), *((f"{entry.id}/{case.id}", s) for case in entry.cases for s in case.blocker))
        yield from (
            _finding(rule, subject, f"{detail}: {sentence!r}", repair)
            for subject, sentence in sentences
            for broken, rule, detail, repair in _BLOCKER_LAW
            if broken(sentence)
        )


def _rule_definition(corpus: _Corpus) -> Iterable[_Finding]:
    for entry in corpus.manifest.entries:
        match entry.definition:
            case ProtoDefinition(message=messages):
                if not messages:
                    yield _finding("message-empty", entry.id, "proto definition names no message", "name every rasm.contracts.<family>.v1.<Message>")
                yield from _unresolved(corpus, entry.id, messages)
            case SchemaDefinition() as definition:
                yield from _unresolved(corpus, entry.id, _named(definition))
                yield from _schema_findings(corpus, entry.id, definition)
            case PublisherDefinition(format=format_name):
                if not format_name.strip():
                    yield _finding("publisher-format", entry.id, "publisher definition names no format", "name the publisher format")
            case LawDefinition(anchor=anchor):
                match _LAW_ANCHOR.match(anchor):
                    case None:
                        yield _finding(
                            "definition-anchor",
                            entry.id,
                            f"law anchor {anchor!r} is not path#[NN]-[CLUSTER]",
                            "spell the repo-relative page and its cluster header",
                        )
                    case found if reason := _cluster(corpus.repo / found.group(1), rf"\[{found.group(2)}\]-\[{re.escape(found.group(3))}\]"):
                        yield _finding(
                            "definition-anchor",
                            entry.id,
                            f"law anchor {anchor}: {reason}",
                            "land the cluster at the page, or re-point the anchor",
                            found.group(1),
                        )
                    case _:
                        pass


def _validated(schema: Result[_Document, str], path: Path) -> tuple[str, ...]:
    # A REAL `.json` asset validates against its seam schema; a seam without a schema definition validates nothing.
    match (schema, _json(path)):
        case (Result(tag="ok", ok=document), Result(tag="ok", ok=instance)):
            return _schema_errors(document, instance)
        case (Result(tag="ok"), Result(error=reason)):
            return (reason,)
        case _:
            return ()


def _asset_findings(corpus: _Corpus, subject: str, entry: Entry, assets: tuple[Asset, ...]) -> Iterable[_Finding]:
    schema: Result[_Document, str] = (
        _document(corpus.root / entry.definition.path) if isinstance(entry.definition, SchemaDefinition) else Error("no schema definition")
    )
    for asset in assets:
        path, where = corpus.root / asset.path, f"{CORPUS}/{asset.path}"
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
        if (digest := xxhash.xxh128(raw, seed=0).hexdigest()) != asset.digest:
            yield _finding(
                "asset-digest",
                subject,
                f"{asset.path}: digest {digest} on disk, {asset.digest} registered",
                "re-prove in scratch and re-record the digest",
                where,
            )
        if entry.pin == "REAL" and path.suffix == ".json":
            yield from (
                _finding("asset-invalid", subject, f"{asset.path}: {row}", "repair the asset or its seam schema", where)
                for row in _validated(schema, path)
            )


def _rule_assets(corpus: _Corpus) -> Iterable[_Finding]:
    for entry in corpus.manifest.entries:
        yield from _asset_findings(corpus, entry.id, entry, entry.assets)
        for case in entry.cases:
            yield from _asset_findings(corpus, f"{entry.id}/{case.id}", entry, case.assets)


def _rule_anchors(corpus: _Corpus) -> Iterable[_Finding]:
    repair = "land the cluster at the page, or re-point the anchor"
    for entry in corpus.manifest.entries:
        cells = (
            *((entry.id, label, paths, text) for label, paths, read in _ENTRY_CELLS for text in read(entry)),
            *((f"{entry.id}/{case.id}", label, paths, text) for case in entry.cases for label, paths, read in _CASE_CELLS for text in read(case)),
        )
        yield from (
            _finding("anchor-dangling", subject, f"{label} {text}: {reason}", repair, token=text)
            for subject, label, paths, text in cells
            if (reason := _anchor(corpus.repo, text, paths=paths))
        )


def _rule_disk(corpus: _Corpus) -> Iterable[_Finding]:
    root, entries = corpus.root, corpus.manifest.entries
    estate = frozenset(e.seam for e in entries if e.entry_class != "vendored")
    vendored = frozenset(e.seam for e in entries if e.entry_class == "vendored")
    for entry in entries:
        if not corpus.seam_dir(entry).is_dir():
            yield _finding(
                "seam-missing",
                entry.id,
                f"seam directory {corpus.seam_dir(entry).relative_to(root).as_posix()} is absent",
                "land the seam directory with its definition",
            )
    for child in sorted(root.iterdir()) if root.is_dir() else ():
        if not (child.name in (_ROOTS | estate) if child.is_dir() else child.name in _ROOT_FILES):
            yield _finding(
                "root-stray",
                child.name,
                "unregistered entry at the corpus root",
                "register the seam, move the file under its seam, or delete it",
                f"{CORPUS}/{child.name}",
            )
    for child in sorted((root / _VENDOR).iterdir()) if (root / _VENDOR).is_dir() else ():
        if not (child.is_dir() and child.name in vendored):
            yield _finding(
                "root-stray",
                f"{_VENDOR}/{child.name}",
                "unregistered entry under vendor/",
                "register the publisher as a vendored entry",
                f"{CORPUS}/{_VENDOR}/{child.name}",
            )
    # Every file under a seam is an asset or the definition of an entry (or one of its cases) seated there; a publisher directory unions its entries.
    owned: dict[Path, frozenset[str]] = {}
    for entry in entries:
        definition = (entry.definition.path,) if isinstance(entry.definition, SchemaDefinition) else ()
        assets = (*entry.assets, *(asset for case in entry.cases for asset in case.assets))
        owned[corpus.seam_dir(entry)] = owned.get(corpus.seam_dir(entry), frozenset()) | frozenset((*definition, *(asset.path for asset in assets)))
    for seam_dir, paths in owned.items():
        for file in sorted(candidate for candidate in seam_dir.rglob("*") if candidate.is_file()) if seam_dir.is_dir() else ():
            if (rel := file.relative_to(root).as_posix()) not in paths:
                yield _finding(
                    "seam-stray",
                    rel,
                    "file under a seam that no entry registers",
                    "register it as an asset or definition, or delete it",
                    f"{CORPUS}/{rel}",
                )


def _rule_roster(corpus: _Corpus) -> Iterable[_Finding]:
    # The emitted block between the markers byte-equals the derivation; absent markers or a missing catalog name the repair.
    for language in dict.fromkeys(row.language for row in corpus.template.plugins if row.language):
        roster = _ROSTERS.get(language)
        if roster is None:
            yield _finding("roster-row", language, f"libs/{language} has no roster row", "land the language's _Roster row")
            continue
        try:
            text = (corpus.repo / roster.catalog).read_text(encoding="utf-8")
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
        elif corpus.files and text[span[0] : span[1]] != _roster_block(roster, corpus.files, corpus.template):
            yield _finding(
                "roster-stale",
                language,
                f"{roster.catalog} roster block differs from the emission",
                "assay contracts generate, then commit the catalog",
                roster.catalog,
            )


def _rule_lock(corpus: _Corpus) -> Iterable[_Finding]:
    # A buf.yaml naming deps resolves them through a committed buf.lock; a missing lock floats every generation.
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
    _rule_assets,
    _rule_anchors,
    _rule_disk,
    _rule_roster,
    _rule_lock,
)


def _corpus_of(repo: Path, image: Path, template: _BufGen, projections: _Projections) -> Result[tuple[_Corpus, tuple[_Finding, ...]], Fault]:
    # The corpus both gates read: descriptor files and the decoded registry (an absent or undecodable one reads as empty beside
    # its own head finding); an unreadable manifest is the one faulting path.
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
            manifest = Manifest(version=1, entries=())
        case Result(error=fault):
            return Error(fault)
    corpus = _Corpus(repo=repo, manifest=manifest, files=files, template=template, known=_known(files), projections=projections)
    return Ok((corpus, tuple(head)))


def _targets(repo: Path, image: Path) -> tuple[str, ...]:
    known = _known(_files(image)) if image.is_file() else frozenset()
    entries = load_manifest(repo / CORPUS).map(lambda manifest: manifest.entries).default_value(())
    return tuple(
        dict.fromkeys(
            owner
            for entry in entries
            if isinstance(entry.definition, SchemaDefinition) and entry.definition.framing in _JSON_FRAMINGS
            for arm, owner in (_arm(entry.definition.derived_from),)
            if arm == "proto" and owner in known
        )
    )


def _audit(repo: Path, image: Path, template: _BufGen, projections: _Projections = _NO_PROJECTIONS) -> Result[_Audit, Fault]:
    # The gate's one fold: the corpus, every audit rule, and the census facts the detail projects.
    match _corpus_of(repo, image, template, projections):
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
            seams=tuple(dict.fromkeys(entry.seam for entry in manifest.entries)),
            anchors=tuple((row.subject, row.token) for row in findings if row.rule == "anchor-dangling"),
            entries=len(manifest.entries),
            assets=sum(len(entry.assets) + sum(len(case.assets) for case in entry.cases) for entry in manifest.entries),
        )
    )


# --- [FRESHNESS]


def _tree(base: Path) -> frozenset[str]:
    return frozenset(path.relative_to(base).as_posix() for path in base.rglob("*") if path.is_file()) if base.is_dir() else frozenset()


def _diff(committed: Path, fresh: Path, rel: str) -> tuple[str, ...]:
    try:
        before = (committed / rel).read_text(encoding="utf-8").splitlines(keepends=True)
        after = (fresh / rel).read_text(encoding="utf-8").splitlines(keepends=True)
    except OSError, UnicodeDecodeError:
        return (f"--- {rel}\n+++ {rel}\n@@ binary or unreadable @@\n",)
    return tuple(difflib.unified_diff(before, after, fromfile=f"committed/{rel}", tofile=f"scratch/{rel}", n=1))


def _freshness_rows(repo: Path, scratch: Path, roots: tuple[str, ...]) -> tuple[tuple[tuple[str, str], ...], str]:
    stale: list[tuple[str, str]] = []
    diff: list[str] = []
    for root in roots:
        committed, fresh = repo / root, scratch / root
        have, want = _tree(committed), _tree(fresh)
        _, mismatch, errors = filecmp.cmpfiles(committed, fresh, sorted(have & want), shallow=False)
        changed = sorted((*mismatch, *errors))
        stale.extend(
            (kind, f"{root}/{rel}")
            for kind, rows in (("changed", changed), ("missing", sorted(want - have)), ("orphan", sorted(have - want)))
            for rel in rows
        )
        diff.extend(line for rel in changed for line in _diff(committed, fresh, rel))
    return tuple(stale), "".join(diff[:_DIFF_LINES]) + ("... diff clipped\n" if len(diff) > _DIFF_LINES else "")


# --- [THUNKS]


def _plugins(root: Path, template: _BufGen) -> InprocThunk:
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


def _corpus(repo: Path, image: Path, template: _BufGen, projections: _Projections) -> InprocThunk:
    def run(check: Check) -> Completed:
        argv = check.args.fill(check.tool.command)
        match _audit(repo, image, template, projections):
            case Result(tag="ok", ok=audit):
                status = RailStatus.FAILED if audit.findings else RailStatus.OK
                return receipt(argv, status.exit_code, stdout=_ENCODER.encode(audit), status=status)
            case Result(error=fault):
                return receipt(argv, RailStatus.FAULTED.exit_code, stderr=fault.message.encode(), status=RailStatus.FAULTED)

    return run


def _freshness(repo: Path, scratch: Path, roots: tuple[str, ...]) -> InprocThunk:
    def run(check: Check) -> Completed:
        stale, diff = _freshness_rows(repo, scratch / _GEN, roots)
        target = scratch / _DIFF
        artifacts: tuple[Artifact, ...] = ()
        if diff:
            target.write_text(diff, encoding="utf-8")
            artifacts = (Artifact(id="freshness-diff", kind=ArtifactKind.SCOPE, path=str(target), bytes=len(diff.encode()), lines=diff.count("\n")),)
        status = RailStatus.FAILED if stale else RailStatus.OK
        payload = _ENCODER.encode(_Freshness(stale=stale, roots=roots, diff=str(target) if diff else ""))
        return receipt(check.args.fill(check.tool.command), status.exit_code, stdout=payload, status=status, artifacts=artifacts)

    return run


def _schemas(corpus: _Corpus) -> tuple[tuple[_Finding, ...], tuple[str, ...]]:
    findings: list[_Finding] = []
    notes: list[str] = []
    for entry in corpus.manifest.entries:
        if not isinstance(entry.definition, SchemaDefinition):
            continue
        rel = entry.definition.path
        match _derived(corpus, entry.id, entry.definition, f"{CORPUS}/{rel}"):
            case Result(tag="ok", ok=derived):
                target = corpus.root / rel
                changed = not target.is_file() or target.read_bytes() != derived
                if changed:
                    target.parent.mkdir(parents=True, exist_ok=True)
                    target.write_bytes(derived)
                notes.append(f"schema: {CORPUS}/{rel} {'written' if changed else 'unchanged'}")
            case Result(error=rows):
                findings.extend(rows)
    return tuple(findings), tuple(notes)


def _emit(repo: Path, image: Path, template: _BufGen, projections: _Projections) -> InprocThunk:
    # The writer leg: one roster block per generated package lands between the catalog's markers and every derived schema
    # lands at its seam; a missing catalog, marker pair, or derivation is a finding, never a write elsewhere.
    def run(check: Check) -> Completed:
        argv = check.args.fill(check.tool.command)
        if not image.is_file():
            return receipt(
                argv, RailStatus.FAULTED.exit_code, stderr=b"no descriptor set was built; the roster cannot be emitted", status=RailStatus.FAULTED
            )
        match _corpus_of(repo, image, template, projections):
            case Result(tag="ok", ok=(corpus, head)):
                pass
            case Result(error=fault):
                return receipt(argv, RailStatus.FAULTED.exit_code, stderr=fault.message.encode(), status=RailStatus.FAULTED)
        files = corpus.files
        refused, written = _schemas(corpus)
        findings: list[_Finding] = [*head, *refused]
        notes: list[str] = [*written]
        for language in dict.fromkeys(row.language for row in template.plugins if row.language):
            roster = _ROSTERS.get(language)
            if roster is None:
                findings.append(_finding("roster-row", language, f"libs/{language} has no roster row", "land the language's _Roster row"))
                continue
            catalog = repo / roster.catalog
            try:
                text = catalog.read_text(encoding="utf-8")
            except OSError:
                findings.append(
                    _finding("roster-catalog", language, f"{roster.catalog} is absent", "land the generated package's .api catalog", roster.catalog)
                )
                continue
            match _roster_span(text):
                case None:
                    findings.append(
                        _finding(
                            "roster-missing",
                            language,
                            f"{roster.catalog} carries no {ROSTER_BEGIN} / {ROSTER_END} block",
                            "land the marker pair",
                            roster.catalog,
                        )
                    )
                case (start, end):
                    emitted = text[:start] + _roster_block(roster, files, template) + text[end:]
                    if emitted != text:
                        catalog.write_text(emitted, encoding="utf-8")
                    notes.append(f"roster: {roster.catalog} {'written' if emitted != text else 'unchanged'}")
        status = RailStatus.FAILED if findings else RailStatus.OK
        return receipt(
            argv, status.exit_code, stdout=_ENCODER.encode(_Audit(findings=tuple(findings), files=files)), status=status, notes=tuple(notes)
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
    # A lane that ran and exited clean passed: EMPTY is the returncode projection of a silent exit 0, not an absent run.
    return RailStatus.OK if done.status is RailStatus.EMPTY else done.status


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


def _detail(lanes: _Lanes, gates: tuple[_Gate, ...], rows: tuple[Match, ...], scratch: str) -> ContractsRun:
    probe = next((gate for gate in gates if isinstance(gate, _Probe)), _Probe())
    audit = next((gate for gate in gates if isinstance(gate, _Audit)), _Audit())
    fresh = next((gate for gate in gates if isinstance(gate, _Freshness)), _Freshness())
    format_out = next((done.stdout for name, done in lanes if name == "buf-format"), b"")
    counts = (
        ("files", len(audit.files)),
        ("packages", len({file.package for file in audit.files})),
        ("messages", sum(len(file.messages) for file in audit.files)),
        ("services", sum(len(file.services) for file in audit.files)),
        ("entries", audit.entries),
        ("assets", audit.assets),
        ("findings", len(audit.findings)),
        ("stale", len(fresh.stale)),
    )
    return ContractsRun(
        lanes=tuple((name, _verdict(done).value) for name, done in lanes),
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


def _report(verb: str, lanes: _Lanes, scratch: str) -> Report:
    decoded = tuple((done, _gate(done)) for _, done in lanes)
    gates = tuple(gate for _, gate in decoded if gate is not None)
    base = fold(Claim.CONTRACTS, verb, tuple(done for _, done in lanes), promote_empty=True)
    # A decoded gate's findings ride as typed rows, so its raw stdout tail never doubles as a process defect row.
    consumed = frozenset(" ".join(done.argv) for done, gate in decoded if gate is not None)
    kept = tuple(row for row in base.results if row.id not in consumed)
    stale_total = sum(len(gate.stale) for gate in gates if isinstance(gate, _Freshness))
    notes = (*base.notes, *cap_note(min(stale_total, _STALE_CAP), stale_total, _STALE_CAP, tail="stale rows ride the detail"))
    return msgspec.structs.replace(base, results=(*_rows(gates), *kept), notes=notes, detail=_detail(lanes, gates, kept, scratch))


# --- [PHASES]


def _scratch(settings: AssaySettings, scope: ArtifactScope) -> Result[Path, Fault]:
    # buf regenerates into a real directory and filecmp walks it, so the scratch root must be a local path the file backend owns.
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
    # Fans every check `seat` leaves live and keeps the pre-seated receipts in the declared lane order.
    seated = tuple(seat(chk) for chk in checks)
    live = tuple(chk for chk, done in zip(checks, seated, strict=True) if done is None)

    def folded(done: tuple[Completed, ...]) -> _Lanes:
        played = iter(done)
        return tuple((_lane_name(chk), pre if pre is not None else next(played)) for chk, pre in zip(checks, seated, strict=True))

    return sequence(block.of_seq(executor.fan(live, settings=settings, scope=scope, routed=_ROUTED))).map(lambda done: folded(tuple(done)))


def _probe(root: Path, template: _BufGen, executor: Executor, *, settings: AssaySettings, scope: ArtifactScope) -> Result[Completed, Fault]:
    probe = Check(tool=_ROWS["plugin-probe", Mode.VERIFY], thunk=_plugins(root, template))
    return _phase(executor, (probe,), lambda _chk: None, settings=settings, scope=scope).map(itemgetter(0)).map(itemgetter(1))


def _lane_check(lane: _Lane, scratch: Path) -> Check:
    return Check(tool=_ROWS[lane.name, lane.mode], args=ToolArgs(output=str(scratch / lane.output)) if lane.output else ToolArgs())


def _derive_check(module: str, scratch: Path, fqn: str) -> Check:
    return Check(tool=_ROWS["buf-jsonschema", Mode.STAGE], args=ToolArgs(input=module, output=str(scratch / _SCHEMAS), fqn=fqn))


def _projection(fqn: str, done: Completed, scratch: Path) -> Result[bytes, str]:
    target = scratch / _SCHEMAS / f"{fqn}{_BUNDLE}"
    if done.status in {RailStatus.OK, RailStatus.EMPTY} and target.is_file():
        return Ok(target.read_bytes())
    reason = done.stderr[-_TAIL:].decode(errors="replace").strip() or "; ".join(done.notes) or f"lane {done.status.value}"
    return Error(f"{fqn}: {reason}")


def _derive_phase(
    root: Path, scratch: Path, done: _Lanes, executor: Executor, *, settings: AssaySettings, scope: ArtifactScope
) -> Result[tuple[_Lanes, _Projections], Fault]:
    projector = next((gate.projector for _, played in done for gate in (_gate(played),) if isinstance(gate, _Probe)), "")
    refusal = "" if projector else f"{JSONSCHEMA_PLUGIN} is not resolvable; {_MACHINE_HINT}"
    fqns = _targets(root, scratch / _IMAGE)
    checks = tuple(_derive_check(_ESTATE, scratch, fqn) for fqn in fqns)
    if checks and not refusal:
        (scratch / _SCHEMAS).mkdir(parents=True, exist_ok=True)
    return _phase(executor, checks, lambda chk: _seat(chk, RailStatus.UNSUPPORTED, refusal) if refusal else None, settings=settings, scope=scope).map(
        lambda played: (
            (*done, *played),
            MappingProxyType({fqn: _projection(fqn, lane, scratch) for fqn, (_, lane) in zip(fqns, played, strict=True)}),
        )
    )


def _check_run(
    root: Path, template: _BufGen, scratch: Path, settings: AssaySettings, scope: ArtifactScope, executor: Executor
) -> Result[Report, Fault]:
    lanes = tuple(_lane_check(lane, scratch) for lane in _LANES)
    gated = frozenset(lane.name for lane in _LANES if lane.gated)
    freshness = Check(tool=_ROWS["freshness-gate", Mode.QUERY], thunk=_freshness(root, scratch, out_dirs(template)))

    def gate_phase(probe: Completed) -> Result[_Lanes, Fault]:
        missed = probe.status is not RailStatus.OK
        return _phase(
            executor,
            lanes,
            lambda chk: _seat(chk, RailStatus.SKIP, "plugin probe missed") if missed and chk.tool.name in gated else None,
            settings=settings,
            scope=scope,
        ).map(lambda done: (("plugin-probe", probe), *done))

    def post_phase(derived: tuple[_Lanes, _Projections]) -> Result[_Lanes, Fault]:
        done, projections = derived
        generated = dict(done)["buf-generate"]
        reason = "" if generated.status in {RailStatus.OK, RailStatus.EMPTY} else "scratch generation did not complete"
        corpus = Check(tool=_ROWS["corpus-gate", Mode.CHECK], thunk=_corpus(root, scratch / _IMAGE, template, projections))
        return _phase(
            executor,
            (corpus, freshness),
            lambda chk: _seat(chk, RailStatus.SKIP, reason) if reason and chk is freshness else None,
            settings=settings,
            scope=scope,
        ).map(lambda post: (*done, *post))

    return (
        _probe(root, template, executor, settings=settings, scope=scope)
        .bind(gate_phase)
        .bind(lambda done: _derive_phase(root, scratch, done, executor, settings=settings, scope=scope))
        .bind(post_phase)
        .map(lambda done: _report("check", done, str(scratch)))
    )


def _generate_run(
    root: Path, template: _BufGen, scratch: Path, settings: AssaySettings, scope: ArtifactScope, executor: Executor
) -> Result[Report, Fault]:
    build = _lane_check(_LANES[0], scratch)
    write = Check(tool=_ROWS["buf-generate", Mode.WRITE])

    def write_phase(probe: Completed) -> Result[_Lanes, Fault]:
        if probe.status is not RailStatus.OK:
            return Error(
                Fault(("buf", "generate"), RailStatus.UNSUPPORTED, f"plugin probe: {probe.stderr.decode(errors='replace') or 'missed'}"[:_TAIL])
            )
        return _phase(executor, (build, write), lambda _chk: None, settings=settings, scope=scope).map(lambda done: (("plugin-probe", probe), *done))

    def emit_phase(derived: tuple[_Lanes, _Projections]) -> Result[_Lanes, Fault]:
        # The emission rides the regeneration: a failed write leaves the catalogs and the seams untouched.
        done, projections = derived
        written = dict(done)["buf-generate"]
        reason = "" if written.status in {RailStatus.OK, RailStatus.EMPTY} else "regeneration did not complete"
        emit = Check(tool=_ROWS["corpus-emit", Mode.WRITE], thunk=_emit(root, scratch / _IMAGE, template, projections))
        return _phase(executor, (emit,), lambda chk: _seat(chk, RailStatus.SKIP, reason) if reason else None, settings=settings, scope=scope).map(
            lambda post: (*done, *post)
        )

    return (
        _probe(root, template, executor, settings=settings, scope=scope)
        .bind(write_phase)
        .bind(lambda done: _derive_phase(root, scratch, done, executor, settings=settings, scope=scope))
        .bind(emit_phase)
        .map(lambda done: _report("generate", done, str(scratch)))
    )


def _leased(settings: AssaySettings, run: Callable[[], Result[Report, Fault]]) -> Result[Report, Fault]:
    return leased(_LEASE, lambda _held: run(), settings=settings, run_id=settings.run_id, mode="exclusive")


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(settings: AssaySettings, scope: ArtifactScope, params: ContractsParams, executor: Executor) -> Result[Report, Fault]:
    """Gate the contracts plane: plugin probe, buf lanes, schema projections, corpus audit, and scratch-regeneration freshness under one lease.

    A template-plugin miss degrades the fold to UNSUPPORTED and skips scratch generation and freshness; a projector miss
    seats the projection lanes UNSUPPORTED and the audit names each underivable seam; every lane's verdict reads its exit
    code, a defect exit parses to rows and any other failure keeps its siblings' rows.

    Returns:
        Folded report with ``ContractsRun`` detail, or a template, scratch, lease, or spawn fault.
    """
    _ = params
    root = Path(str(settings.root))
    return read_template(root).bind(
        lambda template: _scratch(settings, scope).bind(
            lambda scratch: _leased(settings, lambda: _check_run(root, template, scratch, settings, scope, executor))
        )
    )


def generate(settings: AssaySettings, scope: ArtifactScope, params: ContractsParams, executor: Executor) -> Result[Report, Fault]:
    """Regenerate every out root with ``buf generate --clean``, then emit the roster blocks and the derived seam schemas under the contracts lease.

    A template-plugin miss refuses outright; a projector miss lands everything but the proto-derived schemas, each named as a finding.

    Returns:
        Folded report with ``ContractsRun`` detail, or a template, scratch, probe, lease, or spawn fault.
    """
    _ = params
    root = Path(str(settings.root))
    return read_template(root).bind(
        lambda template: _scratch(settings, scope).bind(
            lambda scratch: _leased(settings, lambda: _generate_run(root, template, scratch, settings, scope, executor))
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
    "Case",
    "ContractsParams",
    "Definition",
    "Entry",
    "EntryClass",
    "LawDefinition",
    "Manifest",
    "Pin",
    "Proof",
    "ProtoDefinition",
    "ProtoFraming",
    "PublisherDefinition",
    "Role",
    "SchemaDefinition",
    "SchemaFraming",
    "check",
    "derived_schema",
    "generate",
    "load_manifest",
    "out_dirs",
    "read_template",
]
