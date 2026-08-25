"""Laws for the contracts rail: lane fan and argv, buf exit algebra, corpus audit rows, freshness, probes, leases, and params."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import datetime
from functools import partial
from hashlib import sha256
from pathlib import Path
import re
import shutil
from struct import pack
from typing import Self, TYPE_CHECKING

import anyio
from expression import Ok, Result
import jsonschema
import msgspec
from protobuf.wkt import (
    DescriptorProto,
    EnumDescriptorProto,
    EnumValueDescriptorProto,
    FieldDescriptorProto,
    FileDescriptorProto,
    FileDescriptorSet,
    MessageOptions,
    MethodDescriptorProto,
    ServiceDescriptorProto,
)
import pytest
import xxhash

from assay.composition.catalog import BUF_DEFECT_EXIT, JSONSCHEMA_PLUGIN, JSONSCHEMA_TEMPLATE, select
from assay.composition.settings import ArtifactBackend
from assay.core.exec import apply_row_status, EngineExecutor
from assay.core.govern import exclusive_lease
from assay.core.model import Band, Check, Claim, Completed, ContractsRun, Fault, Language, Mode, Parser, RailStatus, receipt, Report, Runner, Tool
from assay.diagnostics import fold
from assay.rails import contracts as contracts_rail
from assay.rails.contracts import (
    ActorBinding,
    ApplicationAuthority,
    BackendGenerationFacts,
    BlockedReadiness,
    Case,
    check,
    ClientRequestActor,
    ClientResponseActor,
    CloudEventDefinition,
    ContractsParams,
    derived_schema,
    Distribution,
    DomainAuthority,
    Entry,
    ExpectedAsset,
    Fingerprint,
    generate,
    InfrastructureAuthority,
    LawDefinition,
    load_manifest,
    Manifest,
    MessageActor,
    out_dirs,
    ProofVector,
    ProtoDefinition,
    publish,
    PublisherAuthority,
    PublisherDefinition,
    PublisherLicense,
    PublisherOrigin,
    PythonPackageResource,
    read_template,
    ROSTER_BEGIN,
    ROSTER_END,
    SchemaDefinition,
    ServerRequestActor,
    ServerResponseActor,
    SpecimenAsset,
    Support,
    TypeScriptJsonModule,
    VerifiedReadiness,
)
from tests.python._testkit.runtime import REPO_ROOT
from tests.python._testkit.spec import assert_error_status, assert_ok
from tests.python.tools.assay.kit import SeamExecutor


if TYPE_CHECKING:
    from collections.abc import Callable

    from tests.python.tools.assay.kit import AssayHarness, VerbRunner


# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (
    ContractsParams,
    check,
    generate,
    publish,
    load_manifest,
    derived_schema,
    read_template,
    out_dirs,
    contracts_rail.actor_key,
    contracts_rail.prove_case,
)

_ROWS = {(t.name, t.mode): t for t in select(Claim.CONTRACTS, Language.PROTO)}
_LINT = _ROWS["buf-lint", Mode.CHECK]
_FORMAT = _ROWS["buf-format", Mode.CHECK]
_FAULTED_EXIT = RailStatus.FAULTED.exit_code
_MODULE = "buf.build/rasm/contracts"
_BASELINE_COMMIT = "0123456789abcdef0123456789abcdef"
_PUBLISHED_COMMIT = "fedcba9876543210fedcba9876543210"
_MODULE_INFO = msgspec.json.encode({
    "id": "abcdef0123456789abcdef0123456789",
    "remote": "buf.build",
    "owner": "rasm",
    "name": "contracts",
    "create_time": "2026-08-22T00:00:00Z",
    "state": "MODULE_STATE_ACTIVE",
    "default_label_name": "main",
})
_ACCOUNT = msgspec.json.encode({"username": "rasm-publisher"})
_CORPUS = "libs/contracts"
_README = f"{_CORPUS}/README.md"
_TEMPLATE_PATH = f"{_CORPUS}/buf.gen.yaml"
_CONFIG_PATH = f"{_CORPUS}/buf.yaml"
_TS_PACKAGE = f"{_CORPUS}/package.json"
_TS_REPOSITORY = "https://github.com/bsamiee/Rasm.git"
_SEAM_DIR = "conformance/DEMO_SEAM"
_PY_MARKER = f"{_CORPUS}/gen/python/rasm/contracts/py.typed"
_CONFIG = f"""version: v2
modules:
  - path: proto
    name: {_MODULE}
  - path: vendor/PUB/proto
lint:
  use: [STANDARD]
  except:
    - PACKAGE_VERSION_SUFFIX
"""

_CATALOG = f"""# [DEMO]

## [02]-[SYMBOL_GRAMMAR]

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY] |
| :-----: | :------------------------- | :------------ | :----------- |
|  [01]   | `Rasm.Contracts.<F>.<Msg>` | class         | grammar      |

## [03]-[ROSTER]

{ROSTER_BEGIN}
{ROSTER_END}

## [04]-[IMPLEMENTATION_LAW]

- law
"""

_TEMPLATE = """version: v2
clean: true
managed:
  enabled: true
  disable:
    - path: pub
inputs:
  - directory: libs/contracts
plugins:
  - local: node_modules/.bin/protoc-gen-es
    out: libs/contracts/gen/typescript
    opt: [target=ts]
    types: [demo.Thing, demo.DemoService.Do]
    include_imports: true
  - local: .venv/bin/protoc-gen-py
    out: libs/contracts/gen/python/rasm/contracts
    opt: [init_files=false]
    types: [demo.Thing, demo.DemoService.Do]
    include_imports: true
  - local: .venv/bin/protoc-gen-connectrpc
    out: libs/contracts/gen/python/rasm/contracts
    types: [demo.DemoService.Do]
    include_imports: true
  - remote: buf.build/protocolbuffers/csharp:v36.0
    revision: 1
    out: libs/contracts/gen/dotnet
    types: [demo.Thing]
    include_imports: true
  - remote: buf.build/grpc/csharp:v1.83.0
    revision: 1
    out: libs/contracts/gen/dotnet
    types: [demo.DemoService.Do]
    include_imports: true
"""

_BINARIES = ("node_modules/.bin/protoc-gen-es", ".venv/bin/protoc-gen-py", ".venv/bin/protoc-gen-connectrpc")
_OUTS = ("libs/contracts/gen/typescript", "libs/contracts/gen/python/rasm/contracts", "libs/contracts/gen/dotnet")
_SHELLS = ("pyproject.toml", "tsconfig.json", "Rasm.Contracts.csproj")

_TS_PUBLISHED: dict[str, object] = {"./*": {"types": "./dist/*.d.ts", "import": "./dist/*.js", "default": "./dist/*.js"}}
_TS_MANIFEST: dict[str, object] = {
    "name": "@rasm/contracts",
    "version": "0.1.0",
    "repository": {"type": "git", "url": _TS_REPOSITORY, "directory": _CORPUS},
    "license": "MIT",
    "sideEffects": False,
    "type": "module",
    "exports": {"./*": "./gen/typescript/*.ts"},
    "files": ["dist", "README.md"],
    "scripts": {"build": "tsc --build", "prepack": "pnpm run build"},
    "dependencies": {"@bufbuild/protobuf": "catalog:"},
    "publishConfig": {"access": "public", "exports": _TS_PUBLISHED},
}


class Demo(msgspec.Struct, forbid_unknown_fields=True):
    key: str
    issued: datetime.date | None = None


_DEMO_MODULE = (
    "import datetime\n\nimport msgspec\n\n\n"
    "class Demo(msgspec.Struct, forbid_unknown_fields=True):\n    key: str\n    issued: datetime.date | None = None\n"
)
_SEAM_PATH = f"{_SEAM_DIR}/contract.schema.json"
_SEAM_ID = "contract.schema.json"
_SEAM_SCHEMA = derived_schema(Demo, identity=_SEAM_ID)
_HAND_SCHEMA = {
    "$schema": "https://json-schema.org/draft/2020-12/schema",
    "$id": _SEAM_ID,
    "$defs": {"token": {"type": "string", "minLength": 1}},
    "type": "object",
    "required": ["key"],
    "properties": {"key": {"$ref": "#/$defs/token"}},
    "additionalProperties": False,
}
_THING = "demo.Thing"
_REPLY = "demo.Reply"
_METHOD = "demo.DemoService.Do"
_EVENT = "pub.Event"
_THING_PATH = f"{_SEAM_DIR}/{_THING}.jsonschema.strict.bundle.json"
_DECLARATION = "rasm.contracts.declaration.DeclarationRecord"
_FIXTURE_CONFIG = (
    "version: v2\nmodules:\n  - path: proto\ndeps:\n  - buf.build/bufbuild/protovalidate\n"
    "lint:\n  use: [STANDARD]\n  except:\n    - PACKAGE_VERSION_SUFFIX\n"
)
_FIXTURE_PROTO = (
    'syntax = "proto3";\n\npackage fx;\n\nimport "buf/validate/validate.proto";\n\n'
    "enum Kind {\n  KIND_UNSPECIFIED = 0;\n  KIND_ALPHA = 1;\n}\n\n"
    "message Thing {\n  Kind kind = 1 [(buf.validate.field).enum.defined_only = true];\n"
    "  uint32 count = 2 [(buf.validate.field).uint32.gte = 1];\n}\n"
)
_ASSET_DOC = b'{"key": "alpha"}\n'
_PUB_BYTES = b'{"type":"record","name":"Publisher","fields":[]}\n'
_LICENSE_BYTES = b"Apache License 2.0\n"
_DISTRIBUTION = TypeScriptJsonModule(path="libs/contracts/gen/typescript/io/publisher/v1/publisher_avro.ts", symbol="PublisherAvro")
_PY_DISTRIBUTION = PythonPackageResource(path="libs/contracts/gen/python/rasm/contracts/io/publisher/v1/publisher.avsc", package="rasm.contracts")
_LINT_ROW = (
    b'{"path":"libs/contracts/proto/rasm/contracts/scene/scene.proto","start_line":9,"start_column":3,"end_line":9,"end_column":20,'
    b'"type":"FIELD_LOWER_SNAKE_CASE","message":"Field name must be lower_snake_case."}\n'
)
_DIFF = (
    b"diff -u libs/contracts/proto/a.proto.orig libs/contracts/proto/a.proto\n"
    b"--- libs/contracts/proto/a.proto.orig\t2026\n+++ libs/contracts/proto/a.proto\t2026\n@@ -1 +1 @@\n-x\n+y\n"
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _fingerprint(raw: bytes, algorithm: contracts_rail.FingerprintAlgorithm = "xxh128") -> Fingerprint:
    value = xxhash.xxh128(raw, seed=0).hexdigest() if algorithm == "xxh128" else sha256(raw).hexdigest()
    return Fingerprint(algorithm=algorithm, value=value)


def _specimen(
    path: str,
    raw: bytes,
    algorithm: contracts_rail.FingerprintAlgorithm = "xxh128",
    *,
    minter: str = "",
    distributions: tuple[Distribution, ...] = (),
) -> SpecimenAsset:
    return SpecimenAsset(path=path, bytes=len(raw), fingerprint=_fingerprint(raw, algorithm), minter=minter, distributions=distributions)


def _expected(path: str, raw: bytes, facts_format: contracts_rail.FactsFormat) -> ExpectedAsset:
    return ExpectedAsset(path=path, bytes=len(raw), fingerprint=_fingerprint(raw), facts_format=facts_format)


def _message(
    anchor: str, coordinate: str, binding: ActorBinding = "generated", roots: tuple[str, ...] = (), supports: tuple[Support, ...] = ()
) -> MessageActor:
    return MessageActor(anchor=anchor, coordinate=coordinate, binding=binding, roots=roots, supports=supports)


def _client_request(anchor: str, coordinate: str, binding: ActorBinding = "generated", method: str = _METHOD) -> ClientRequestActor:
    return ClientRequestActor(anchor=anchor, coordinate=coordinate, binding=binding, method=method)


def _client_response(anchor: str, coordinate: str, binding: ActorBinding = "generated", method: str = _METHOD) -> ClientResponseActor:
    return ClientResponseActor(anchor=anchor, coordinate=coordinate, binding=binding, method=method)


def _server_request(anchor: str, coordinate: str, binding: ActorBinding = "generated", method: str = _METHOD) -> ServerRequestActor:
    return ServerRequestActor(anchor=anchor, coordinate=coordinate, binding=binding, method=method)


def _server_response(anchor: str, coordinate: str, binding: ActorBinding = "generated", method: str = _METHOD) -> ServerResponseActor:
    return ServerResponseActor(anchor=anchor, coordinate=coordinate, binding=binding, method=method)


def _bundle(fqn: str) -> bytes:
    """Mirror protoc-gen-jsonschema's strict bundle for one message whose only property is the `key` string.

    Returns:
        The canned bundle bytes under the plugin's file-name `$id` law.
    """
    anchor = "https://json-schema.org/draft/2020-12/schema"
    body = {"$schema": anchor, "additionalProperties": False, "properties": {"key": {"type": "string"}}, "type": "object"}
    return _json({
        "$defs": {f"{fqn}.jsonschema.strict.json": body},
        "$id": f"{fqn}.jsonschema.strict.bundle.json",
        "$ref": f"#/$defs/{fqn}.jsonschema.strict.json",
        "$schema": anchor,
    })


_DEMO_CASE = Case(
    id="demo",
    definition=SchemaDefinition(path=_SEAM_PATH, framing="canonical-json", derived_from="msgspec:demo_docs.shape.Demo"),
    authority=DomainAuthority(producer=_message("dotnet:Demo/Page/one#CLUSTER", "DemoSchema.write", "proof")),
    readiness=VerifiedReadiness(oracle="external-digest", vectors=(ProofVector(specimens=(_specimen(f"{_SEAM_DIR}/demo.json", _ASSET_DOC),)),)),
    consumers=(_message("python:Demo/Page/one#CLUSTER", "DemoSchema.read", "proof"), _message(_README, "DemoSchema.prove", "proof")),
)
_DEMO: Entry = Entry(id="DEMO_SEAM", law="The demo seam is canonical JSON whose key is the vocabulary token.", cases=(_DEMO_CASE,))
_PROTO_DEFINITION = SchemaDefinition(path=_THING_PATH, framing="proto-json", derived_from=f"proto:{_THING}")
_BINDING_CASE = Case(
    id="thing-request",
    definition=ProtoDefinition(message=_THING, framing="proto-binary"),
    authority=DomainAuthority(producer=_client_request("typescript:Demo/Page/one#CLUSTER", "DemoRequest.call")),
    readiness=BlockedReadiness(blockers=("No shipping application invokes the demo request through every generated binding.",)),
    consumers=(
        _server_request("python:Demo/Page/one#CLUSTER", "DemoRequest.handle"),
        _server_request("dotnet:Demo/Page/one#CLUSTER", "DemoRequest.Handle"),
    ),
)
_BINDING = Entry(
    id="DEMO_PROTO",
    law="The exact request message and method select generated bindings while Buf owns their recursive descriptor closure.",
    cases=(_BINDING_CASE,),
)
_PUB_ORIGIN = PublisherOrigin(
    repository="https://example.com/publisher/contracts",
    commit="0123456789abcdef0123456789abcdef01234567",
    upstream_path="schemas/pub.bin",
    license=PublisherLicense(spdx="Apache-2.0", path="vendor/PUB/LICENSE", sha256=sha256(_LICENSE_BYTES).hexdigest()),
)
_PUB_CASE = Case(
    id="publisher",
    definition=PublisherDefinition(format="bytes", source="vendor/PUB/pub.bin", origin=_PUB_ORIGIN),
    authority=PublisherAuthority(),
    readiness=VerifiedReadiness(
        oracle="publisher-digest", vectors=(ProofVector(specimens=(_specimen("vendor/PUB/pub.bin", _PUB_BYTES, "sha256"),)),)
    ),
    consumers=(_message("dotnet:Demo/Page/one#CLUSTER", "PublisherBytes.read", "package"),),
)
_PUB: Entry = Entry(id="PUB", law="The publisher bytes are frozen under immutable upstream custody.", cases=(_PUB_CASE,))
_PUB_EVENT = Entry(
    id="PUB_EVENT",
    law="The publisher event message generates through the estate rows that carry every other root.",
    cases=(
        Case(
            id="event",
            definition=ProtoDefinition(message=_EVENT, framing="proto-binary"),
            authority=DomainAuthority(producer=_message("dotnet:Demo/Page/one#CLUSTER", "DemoEvent.publish", "generated", roots=(_EVENT,))),
            readiness=BlockedReadiness(blockers=("No shipping application publishes the publisher event through every generated binding.",)),
            consumers=(_message("python:Demo/Page/one#CLUSTER", "DemoEvent.read", "generated", roots=(_EVENT,)),),
        ),
    ),
)


def _entry(**overrides: object) -> Entry:
    entry_keys = {"id", "law", "cases"}
    entry_overrides = {key: value for key, value in overrides.items() if key in entry_keys}
    if "cases" in entry_overrides:
        return msgspec.structs.replace(_DEMO, **entry_overrides)
    case_overrides = {key: value for key, value in overrides.items() if key not in entry_keys}
    case = msgspec.structs.replace(_DEMO_CASE, **case_overrides)
    return msgspec.structs.replace(_DEMO, **entry_overrides, cases=(case,))


def _binding(**overrides: object) -> Entry:
    entry_keys = {"id", "law", "cases"}
    entry_overrides = {key: value for key, value in overrides.items() if key in entry_keys}
    if "cases" in entry_overrides:
        return msgspec.structs.replace(_BINDING, **entry_overrides)
    case_overrides = {key: value for key, value in overrides.items() if key not in entry_keys}
    case = msgspec.structs.replace(_BINDING_CASE, **case_overrides)
    return msgspec.structs.replace(_BINDING, **entry_overrides, cases=(case,))


def _vendored(**overrides: object) -> Entry:
    entry_keys = {"id", "law", "cases"}
    entry_overrides = {key: value for key, value in overrides.items() if key in entry_keys}
    case_overrides = {key: value for key, value in overrides.items() if key not in entry_keys}
    case = msgspec.structs.replace(_PUB_CASE, **case_overrides)
    return msgspec.structs.replace(_PUB, **entry_overrides, cases=(case,))


def _distributed(distribution: Distribution | tuple[Distribution, ...] = _DISTRIBUTION, **overrides: object) -> Entry:
    distributions = distribution if isinstance(distribution, tuple) else (distribution,)
    readiness = _PUB_CASE.readiness
    assert isinstance(readiness, VerifiedReadiness)
    specimen = msgspec.structs.replace(readiness.vectors[0].specimens[0], distributions=distributions)
    return _vendored(readiness=msgspec.structs.replace(readiness, vectors=(ProofVector(specimens=(specimen,)),)), **overrides)


def _descriptors(*, thing: bool = True, reply: bool = True, service: bool = True, unused: bool = True) -> FileDescriptorSet:
    messages = []
    if thing:
        messages.append(
            DescriptorProto(
                name="Thing",
                field=[
                    FieldDescriptorProto(
                        name="event",
                        number=1,
                        label=FieldDescriptorProto.Label.OPTIONAL,
                        type=FieldDescriptorProto.Type.MESSAGE,
                        type_name=".pub.Event",
                    )
                ],
                nested_type=[DescriptorProto(name="Inner")],
                enum_type=[EnumDescriptorProto(name="Kind", value=[EnumValueDescriptorProto(name="KIND_UNSPECIFIED", number=0)])],
            )
        )
    if reply:
        messages.append(DescriptorProto(name="Reply"))
    if unused:
        messages.append(DescriptorProto(name="Unused"))
    demo = FileDescriptorProto(
        name="demo/demo.proto",
        package="demo",
        dependency=["pub/pub.proto"],
        message_type=messages,
        service=(
            [ServiceDescriptorProto(name="DemoService", method=[MethodDescriptorProto(name="Do", input_type=f".{_THING}", output_type=f".{_REPLY}")])]
            if service
            else []
        ),
    )
    pub = FileDescriptorProto(
        name="pub/pub.proto",
        package="pub",
        message_type=[
            DescriptorProto(
                name="Event",
                field=[FieldDescriptorProto(name="id", number=1, label=FieldDescriptorProto.Label.OPTIONAL, type=FieldDescriptorProto.Type.STRING)],
            )
        ],
    )
    return FileDescriptorSet(file=[demo, pub])


def _image() -> bytes:
    return _descriptors().to_binary()


def _filtered_image(roots: tuple[str, ...]) -> bytes:
    selected = frozenset(roots)
    method = _METHOD in selected
    thing = _THING in selected or method
    reply = _REPLY in selected or method
    known = frozenset((_THING, _REPLY, _METHOD))
    return _descriptors(thing=thing, reply=reply, service=method, unused=False).to_binary() if selected & known else FileDescriptorSet().to_binary()


def _write(root: Path, rel: str, payload: bytes | str, *, executable: bool = False) -> Path:
    target = root / rel
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_bytes(payload if isinstance(payload, bytes) else payload.encode())
    target.chmod(0o755) if executable else None
    return target


def _json(value: object) -> bytes:
    return msgspec.json.format(msgspec.json.encode(value), indent=2) + b"\n"


def _git_ignored(root: Path, *patterns: str) -> None:
    """Seat the fixture in a git repository whose ignore roster is exactly ``patterns``; the estate audit reads its carve from git alone."""
    if not (root / ".git").is_dir():
        anyio.run(partial(anyio.run_process, cwd=str(root)), ("git", "init", "-q"))
    _write(root, ".gitignore", "".join(f"{pattern}\n" for pattern in patterns))


def _roster_files(root: Path, template: contracts_rail._Templates) -> dict[str, tuple[contracts_rail._File, ...]]:
    rows: dict[str, list[contracts_rail._File]] = {}
    for index, plugin in enumerate(template.main.plugins, start=1):
        if not plugin.language or not plugin.kinds or not plugin.types:
            continue
        filtered = contracts_rail._files(_write(root, f".cache/assay-test/roster-{index:02d}.binpb", _filtered_image(plugin.types)))
        selected = tuple(
            file
            for file in filtered
            if (plugin.include_imports or any(contracts_rail._file_owns(file, fqn) for fqn in plugin.types))
            and (plugin.include_wkt or file.package != "google.protobuf")
        )
        rows.setdefault(plugin.language, []).extend(contracts_rail._emitted(file, plugin.kinds) for file in selected)
    return {language: tuple(files) for language, files in rows.items()}


def _corpus(
    root: Path,
    entries: tuple[Entry, ...] | None = None,
    *,
    manifest: bytes | None = None,
    schema: bytes | None = None,
    definition: str = _SEAM_PATH,
    seam_schema: bytes | dict[str, object] = _SEAM_SCHEMA,
    asset: bytes = _ASSET_DOC,
    catalogs: tuple[str, ...] | None = None,
    roster: bool = True,
    binaries: tuple[str, ...] = _BINARIES,
    template: str = _TEMPLATE,
    config: str = _CONFIG,
) -> Path:
    """Materialize a repo root: template, fake plugin binaries, corpus, planning pages, catalogs with emitted rosters, and committed out roots.

    ``catalogs`` names which language catalogs exist (every roster row by default); ``roster`` False leaves their marker block empty;
    ``definition`` is the seam path ``seam_schema`` lands at.

    Returns:
        The repo root.
    """
    registry = Manifest(entries=entries if entries is not None else (_entry(), _BINDING, _vendored()))
    corpus = root / _CORPUS
    _write(corpus, "buf.gen.yaml", template)
    _write(corpus, "buf.yaml", config)
    _write(corpus, "package.json", _json(_TS_MANIFEST))
    _write(corpus, "README.md", "# contracts\n\nDemoSchema.prove reads the manifest descriptor contract.\n")
    for binary in binaries:
        _write(root, binary, "#!/bin/sh\nexit 0\n", executable=True)
    _write(corpus, "manifest.json", manifest if manifest is not None else _json(msgspec.to_builtins(registry)))
    _write(corpus, "manifest.schema.json", schema if schema is not None else derived_schema())
    _write(corpus, definition, seam_schema if isinstance(seam_schema, bytes) else _json(seam_schema))
    _write(corpus, f"{_SEAM_DIR}/demo.json", asset)
    _write(corpus, "vendor/PUB/pub.bin", _PUB_BYTES)
    _write(corpus, "vendor/PUB/LICENSE", _LICENSE_BYTES)
    (corpus / "proto").mkdir(exist_ok=True)
    (corpus / ".api").mkdir(exist_ok=True)
    cluster = (
        f"ApplicationInput.byte ApplicationInput.socket DemoSchema.write DemoSchema.read DemoRequest.call DemoRequest.handle "
        f"DemoRequest.Handle PublisherBytes.read DemoEvent.publish DemoEvent.read "
        f"owns schema:{_SEAM_PATH}, proto:{_THING}, proto:{_EVENT}, Thing, Event, DemoService, Do, do, {_METHOD}, manifest, descriptor, corpus, "
        "contract, generated.\n"
    )
    _write(root, "libs/dotnet/Demo/.planning/Page/one.md", f"# page\n\n## [01]-[CLUSTER]\n\n{cluster}")
    _write(root, "libs/python/Demo/.planning/Page/one.md", f"# page\n\n## [02]-[CLUSTER]\n\n{cluster}")
    _write(root, "libs/typescript/Demo/.planning/Page/one.md", f"# page\n\n## [03]-[CLUSTER]\n\n{cluster}")
    _write(root, "libs/.planning/ARCHITECTURE.md", "# arch\n\n## [14]-[EVENT_FABRIC]\n\nbody\n")
    _write(root, "demo_docs/shape.py", _DEMO_MODULE)
    _write(root, "demo_docs/__init__.py", "")
    _write(root, "image.binpb", _image())
    template_result = read_template(root)
    roster_files = _roster_files(root, template_result.ok) if template_result.is_ok() else {}
    for language, row in contracts_rail._ROSTERS.items():
        if catalogs is None or language in catalogs:
            roots = contracts_rail._actor_roots(registry, language, row.kinds)
            distributions = contracts_rail._catalog_distributions(registry, language)
            block = (
                contracts_rail._roster_block(row, roster_files.get(language, ()), roots, distributions) if roster and template_result.is_ok() else ""
            )
            _write(root, row.catalog, _CATALOG.replace(f"{ROSTER_BEGIN}\n{ROSTER_END}", f"{ROSTER_BEGIN}\n{block}{ROSTER_END}"))
    for out in _OUTS:
        _write(root, f"{out}/demo/demo_pb.txt", "generated\n")
    _write(root, _PY_MARKER, b"")
    return root


def _after(root: Path, *steps: Callable[[Path], object]) -> Path:
    for step in steps:
        step(root)
    return root


def _stamped(done: Completed, parser: Parser) -> Completed:
    return msgspec.structs.replace(done, parser=parser)


def _applied(tool: Tool, done: Completed) -> Completed:
    return apply_row_status(tool, _stamped(done, tool.parser))


def _fan(
    root: Path,
    *,
    image: bytes | None = None,
    outcomes: dict[str, Result[Completed, Fault]] | None = None,
    sequences: dict[str, list[Result[Completed, Fault]]] | None = None,
    regenerate: Callable[[Path], None] | None = None,
    calls: list[tuple[str, ...]] | None = None,
) -> Callable[..., tuple[Result[Completed, Fault], ...]]:
    """Hermetic executor fan: INPROC thunks run live and the buf lanes land canned artifacts.

    buf-build lands ``image``, buf-generate mirrors the committed trees into scratch, buf-jsonschema lands the canned bundle of its fqn.

    Returns:
        A fan callable for ``SeamExecutor``; ``outcomes`` override by lane name, ``sequences`` override successive calls, and every other Buf
        lane exits 0.
    """
    picture = image if image is not None else _image()

    def mirror(scratch: Path) -> None:
        for out in _OUTS:
            shutil.rmtree(scratch / out, ignore_errors=True)
            shutil.copytree(root / out, scratch / out)

    def fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
        played: list[Result[Completed, Fault]] = []
        for chk in checks:
            argv = chk.args.fill(chk.tool.command)
            if calls is not None:
                calls.append(argv)
            name = chk.tool.name
            if chk.tool.runner is Runner.INPROC:
                assert chk.thunk is not None, f"{name}: INPROC check carries no thunk"
                played.append(Ok(chk.thunk(chk)))
                continue
            lane = f"{name}:{chk.args.fqn}" if chk.args.fqn else name
            if sequences is not None and (queued := sequences.get(lane, sequences.get(name))) is not None and queued:
                played.append(queued.pop(0).map(partial(_applied, chk.tool)))
                continue
            if outcomes is not None and (override := outcomes.get(lane, outcomes.get(name))) is not None:
                played.append(override.map(partial(_applied, chk.tool)))
                continue
            if name == "buf-build":
                Path(chk.args.output).parent.mkdir(parents=True, exist_ok=True)
                Path(chk.args.output).write_bytes(picture)
            if name == "buf-roster":
                Path(chk.args.output).parent.mkdir(parents=True, exist_ok=True)
                Path(chk.args.output).write_bytes(_filtered_image(tuple(chk.args.targets[1::2])))
            if name == "buf-generate" and chk.args.output:
                (regenerate or mirror)(Path(chk.args.output))
            if name == "buf-jsonschema":
                _write(Path(chk.args.output), f"{chk.args.fqn}.jsonschema.strict.bundle.json", _bundle(chk.args.fqn))
            stdout = (
                _ACCOUNT
                if name == "buf-auth"
                else _MODULE_INFO
                if name == "buf-module"
                else msgspec.json.encode({"commit": _BASELINE_COMMIT, "create_time": "2026-08-22T00:00:00Z"})
                if name == "buf-baseline"
                else msgspec.json.encode({"commit": _PUBLISHED_COMMIT, "create_time": "2026-08-22T00:00:00Z"})
                if name == "buf-verify"
                else f"{_MODULE}:{_PUBLISHED_COMMIT}\n".encode()
                if name == "buf-push"
                else b""
            )
            played.append(Ok(apply_row_status(chk.tool, _stamped(receipt(argv, 0, stdout=stdout), chk.tool.parser))))
        return tuple(played)

    return fan


def _run(assay_root: AssayHarness, fan: Callable[..., tuple[Result[Completed, Fault], ...]], verb: str = "check") -> Result[Report, Fault]:
    handler = {"check": check, "generate": generate, "publish": publish}[verb]
    return handler(assay_root.settings, assay_root.scope(Claim.CONTRACTS), ContractsParams(), SeamExecutor(fan_fn=fan))


def _detail(report: Report) -> ContractsRun:
    assert isinstance(report.detail, ContractsRun)
    return report.detail


def _rules(report: Report) -> frozenset[str]:
    return frozenset(row.id.removeprefix("corpus:") for row in report.results if row.id.startswith("corpus:"))


def _audit_rules(root: Path, image: bytes | None = None) -> tuple[frozenset[str], tuple[str, ...]]:
    template = assert_ok(read_template(root))
    picture = root / "image.binpb"
    if image is not None:
        picture.write_bytes(image)
    audit = assert_ok(contracts_rail._audit(root, picture, template, roster_files=_roster_files(root, template)))
    return frozenset(row.rule for row in audit.findings), tuple(f"{row.rule}: {row.subject}: {row.detail}" for row in audit.findings)


@pytest.fixture
def projector(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> Path:
    """Pin PATH to a tmp bin holding a fake protoc-gen-jsonschema, so plugin resolution never reads the machine.

    Returns:
        The fake projector binary; unlink it to stage a miss.
    """
    binary = _write(tmp_path, f"bin/{JSONSCHEMA_PLUGIN}", "#!/bin/sh\nexit 0\n", executable=True)
    monkeypatch.setenv("PATH", str(tmp_path / "bin"))
    return binary


# --- [LANE_MATRIX]


def test_check_runs_every_local_lane_without_reaching_the_registry(assay_root: AssayHarness, projector: Path) -> None:
    """A clean corpus proves itself from the working tree alone: build, lint, format, scratch generation, the rosters, and the gates."""
    root = _corpus(assay_root.root)
    calls: list[tuple[str, ...]] = []
    report = assert_ok(_run(assay_root, _fan(root, calls=calls)))
    detail = _detail(report)
    scratch = Path(detail.scratch)
    assert report.status is RailStatus.OK, report
    assert report.counts.band(Band.REFUSED) == 0 and report.results == ()
    assert [name for name, _ in detail.lanes] == [
        "plugin-probe",
        "buf-build",
        "buf-lint",
        "buf-format",
        "buf-generate",
        "buf-roster:typescript-01",
        "buf-roster:python-02",
        "buf-roster:python-03",
        "buf-roster:dotnet-04",
        "buf-roster:dotnet-05",
        "corpus-gate",
        "freshness-gate",
    ]
    assert all(status == "ok" for _, status in detail.lanes), detail.lanes
    assert calls[1:5] == [
        ("buf", "build", "libs/contracts", "-o", str(scratch / "image.binpb"), "--as-file-descriptor-set"),
        ("buf", "lint", "libs/contracts", "--error-format", "json"),
        ("buf", "format", "--diff", "--exit-code", "libs/contracts/proto"),
        ("buf", "generate", "libs/contracts", "--template", "libs/contracts/buf.gen.yaml", "-o", str(scratch / "gen")),
    ]
    assert not any(argv[:2] == ("buf", "registry") for argv in calls)
    roster = ("buf", "build", "libs/contracts", "-o")
    assert calls[5:10] == [
        (*roster, str(scratch / "roster/typescript-01.binpb"), "--as-file-descriptor-set", "--type", _THING, "--type", _METHOD),
        (*roster, str(scratch / "roster/python-02.binpb"), "--as-file-descriptor-set", "--type", _THING, "--type", _METHOD),
        (*roster, str(scratch / "roster/python-03.binpb"), "--as-file-descriptor-set", "--type", _METHOD),
        (*roster, str(scratch / "roster/dotnet-04.binpb"), "--as-file-descriptor-set", "--type", _THING),
        (*roster, str(scratch / "roster/dotnet-05.binpb"), "--as-file-descriptor-set", "--type", _METHOD),
    ]
    assert detail.module == _MODULE and not detail.baseline and not detail.published
    assert detail.packages == ("demo", "pub")
    assert dict(detail.counts) == {"files": 2, "packages": 2, "messages": 5, "services": 1, "entries": 3, "assets": 2, "findings": 0, "stale": 0}
    assert detail.seams == ("DEMO_SEAM", "DEMO_PROTO", "PUB")
    assert detail.plugins == (*((binary, str(root / binary)) for binary in _BINARIES), (JSONSCHEMA_PLUGIN, str(projector)))
    assert detail.template == "libs/contracts/buf.gen.yaml" and scratch.is_relative_to(assay_root.root / ".artifacts")


@pytest.mark.parametrize(
    "stdout,stderr",
    [
        (b"not-json", b""),
        (msgspec.json.encode({"commit": "short", "create_time": "2026-08-22T00:00:00Z"}), b""),
        (b"", b"Failure: unauthenticated\n"),
        (b"", b"Failure: registry connection refused\n"),
    ],
    ids=("malformed-json", "invalid-commit", "auth", "network"),
)
def test_baseline_resolver_fault_refuses_the_push_and_keeps_every_gate_lane(assay_root: AssayHarness, stdout: bytes, stderr: bytes) -> None:
    """Malformed, authentication, and network resolver failures fault publish custody while every independent gate lane still executes."""
    root = _corpus(assay_root.root)
    rc = 0 if stdout else 1
    calls: list[tuple[str, ...]] = []
    outcomes = {"buf-baseline": Ok(receipt(("buf", "registry"), rc, stdout=stdout, stderr=stderr))}
    report = assert_ok(_run(assay_root, _fan(root, outcomes=outcomes, calls=calls), "publish"))
    detail = _detail(report)
    assert report.status is RailStatus.FAULTED and dict(detail.lanes)["buf-baseline"] == "faulted"
    assert not detail.baseline and not detail.published
    assert all(status == "ok" for name, status in detail.lanes if name != "buf-baseline")
    assert not any(argv[:2] == ("buf", "push") for argv in calls)


def test_publish_admits_only_exact_first_publish_absence_and_returns_commit(assay_root: AssayHarness) -> None:
    """The publish verb proves exact absence twice, gates every sibling, pushes once, and captures the returned commit."""
    root = _corpus(assay_root.root)
    calls: list[tuple[str, ...]] = []
    absent = receipt(
        ("buf", "registry", "module", "info", _MODULE),
        1,
        stderr=f'Failure: a module named "{_MODULE}" does not exist, use "buf registry module create" to create one\n'.encode(),
    )
    report = assert_ok(_run(assay_root, _fan(root, outcomes={"buf-module": Ok(absent)}, calls=calls), "publish"))
    detail = _detail(report)
    push = (
        *("buf", "push", "libs/contracts", "--exclude-unnamed"),
        *("--create", "--create-visibility", "public", "--create-default-label", "main", "--label", "main"),
    )
    assert report.status is RailStatus.OK and detail.module == _MODULE
    assert dict(detail.lanes)["buf-module"] == "skip" and dict(detail.lanes)["buf-baseline"] == "skip"
    assert dict(detail.lanes)["buf-prepush"] == "skip" and dict(detail.lanes)["buf-push"] == "ok"
    assert dict(detail.lanes)["buf-auth"] == "ok" and dict(detail.lanes)["buf-verify"] == "ok"
    assert detail.published == f"{_MODULE}:{_PUBLISHED_COMMIT}"
    assert calls[0] == ("buf", "registry", "whoami", "buf.build", "--format", "json")
    assert calls[-3] == ("buf", "registry", "module", "info", _MODULE, "--format", "json")
    assert calls[-2] == push and "--git-metadata" not in push and "BUF_TOKEN" not in push
    assert "--create-visibility" in push and push[push.index("--create-visibility") + 1] == "public"
    resolves = [index for index, argv in enumerate(calls) if argv[:5] == ("buf", "registry", "module", "commit", "resolve")]
    assert resolves == [len(calls) - 1]
    assert calls[-1] == ("buf", "registry", "module", "commit", "resolve", f"{_MODULE}:main", "--format", "json")


def test_publish_faults_a_push_that_returns_no_exact_coordinate(assay_root: AssayHarness) -> None:
    """A successful push whose stdout carries no exact module commit coordinate faults its receipt and publishes nothing."""
    root = _corpus(assay_root.root)
    malformed = Ok(receipt(("buf", "push"), 0, stdout=b"pushed\n"))
    report = assert_ok(_run(assay_root, _fan(root, outcomes={"buf-push": malformed}), "publish"))
    assert report.status is RailStatus.FAULTED and dict(_detail(report).lanes)["buf-push"] == "faulted"
    assert not _detail(report).published


def test_publish_runs_the_complete_present_module_gate_before_push(assay_root: AssayHarness) -> None:
    """A present module proves every gate, re-resolves the unchanged default label, then pushes as the final lease-held action."""
    root = _corpus(assay_root.root)
    calls: list[tuple[str, ...]] = []
    report = assert_ok(_run(assay_root, _fan(root, calls=calls), "publish"))
    detail = _detail(report)
    lanes = dict(detail.lanes)
    assert report.status is RailStatus.OK and detail.baseline == f"{_MODULE}:{_BASELINE_COMMIT}"
    assert all(lanes[name] == "ok" for name in ("buf-auth", "buf-baseline", "corpus-gate", "freshness-gate", "buf-prepush", "buf-push", "buf-verify"))
    assert calls[0] == ("buf", "registry", "whoami", "buf.build", "--format", "json")
    assert calls[-3] == ("buf", "registry", "module", "commit", "resolve", f"{_MODULE}:main", "--format", "json")
    assert calls[-2] == ("buf", "push", "libs/contracts", "--exclude-unnamed", "--label", "main")
    assert calls[-1] == ("buf", "registry", "module", "commit", "resolve", f"{_MODULE}:main", "--format", "json")
    assert "--create" not in calls[-2] and "--create-visibility" not in calls[-2]
    assert calls.index(("corpus-gate", "check")) < len(calls) - 2
    assert calls.index(("freshness-gate", "diff")) < len(calls) - 2


@pytest.mark.parametrize(
    "outcome",
    [
        Ok(receipt(("buf", "registry", "whoami"), 1, stderr=b"Failure: Not currently logged in for buf.build.\n")),
        Ok(receipt(("buf", "registry", "whoami"), 0, stdout=b"not-json")),
        Ok(receipt(("buf", "registry", "whoami"), 0, stdout=msgspec.json.encode({"username": ""}))),
    ],
    ids=("logged-out", "malformed", "empty-account"),
)
def test_publish_refuses_before_the_gate_when_the_credential_does_not_resolve(assay_root: AssayHarness, outcome: Result[Completed, Fault]) -> None:
    """Module absence reads the same with and without a credential, so an unresolved one refuses before any gate work runs."""
    root = _corpus(assay_root.root)
    calls: list[tuple[str, ...]] = []
    report = assert_ok(_run(assay_root, _fan(root, outcomes={"buf-auth": outcome}, calls=calls), "publish"))
    detail = _detail(report)
    assert report.status is RailStatus.FAULTED and dict(detail.lanes) == {"buf-auth": "faulted"}
    assert calls == [("buf", "registry", "whoami", "buf.build", "--format", "json")]
    assert "BUF_TOKEN" in dict(detail.faults)["buf-auth"] and not detail.published


def test_publish_credential_probe_never_runs_on_the_check_verb(assay_root: AssayHarness, projector: Path) -> None:
    """Every gate lane resolves unauthenticated, so `check` spends no credential and never probes for one."""
    root = _corpus(assay_root.root)
    calls: list[tuple[str, ...]] = []
    report = assert_ok(_run(assay_root, _fan(root, calls=calls)))
    assert report.status is RailStatus.OK and "buf-auth" not in dict(_detail(report).lanes)
    assert not any(argv[:3] == ("buf", "registry", "whoami") for argv in calls)


@pytest.mark.parametrize(
    "outcome,fault",
    [
        (Ok(receipt(("buf", "registry"), 0, stdout=msgspec.json.encode({"commit": _BASELINE_COMMIT, "create_time": "x"}))), "carries"),
        (Ok(receipt(("buf", "registry"), 1, stderr=b"Failure: registry connection refused\n")), "could not be read back"),
    ],
    ids=("label-carries-another-commit", "resolver-fault"),
)
def test_publish_faults_when_the_default_label_does_not_carry_the_pushed_commit(
    assay_root: AssayHarness, outcome: Result[Completed, Fault], fault: str
) -> None:
    """The push is believed only once `main` reads back the coordinate it returned; the receipt still names what was pushed."""
    root = _corpus(assay_root.root)
    calls: list[tuple[str, ...]] = []
    report = assert_ok(_run(assay_root, _fan(root, outcomes={"buf-verify": outcome}, calls=calls), "publish"))
    detail = _detail(report)
    assert report.status is RailStatus.FAULTED and dict(detail.lanes)["buf-verify"] == "faulted"
    assert dict(detail.lanes)["buf-push"] == "ok" and detail.published == f"{_MODULE}:{_PUBLISHED_COMMIT}"
    assert fault in dict(detail.faults)["buf-verify"]
    assert sum(argv[:2] == ("buf", "push") for argv in calls) == 1


@pytest.mark.parametrize(
    "lane,stderr",
    [
        ("buf-module", b"Failure: unauthenticated\n"),
        ("buf-module", b"Failure: registry connection refused\n"),
        ("buf-baseline", f'Failure: "{_MODULE}:main" does not exist\n'.encode()),
    ],
    ids=("module-auth", "module-network", "default-label-absent"),
)
def test_publish_never_bootstraps_auth_network_or_an_absent_label(assay_root: AssayHarness, lane: str, stderr: bytes) -> None:
    """Only exact module absence bootstraps; auth, network, and a missing default label fault without publishing."""
    root = _corpus(assay_root.root)
    calls: list[tuple[str, ...]] = []
    failed = Ok(receipt(("buf", "registry"), 1, stderr=stderr))
    report = assert_ok(_run(assay_root, _fan(root, outcomes={lane: failed}, calls=calls), "publish"))
    assert report.status is RailStatus.FAULTED
    assert not any(argv[:2] == ("buf", "push") for argv in calls)


@pytest.mark.parametrize(
    "stdout",
    [
        b"not-json",
        _MODULE_INFO.replace(b'"name":"contracts"', b'"name":"other"'),
        _MODULE_INFO.replace(b'"MODULE_STATE_ACTIVE"', b'"MODULE_STATE_DEPRECATED"'),
        _MODULE_INFO.replace(b'"default_label_name":"main"', b'"default_label_name":"release"'),
    ],
    ids=("malformed", "wrong-coordinate", "inactive", "wrong-default-label"),
)
def test_module_lookup_requires_the_exact_active_configured_module(assay_root: AssayHarness, stdout: bytes) -> None:
    """A successful lookup is still faulted unless its typed identity is the exact active module whose default label is `main`."""
    root = _corpus(assay_root.root)
    calls: list[tuple[str, ...]] = []
    module = Ok(receipt(("buf", "registry", "module", "info"), 0, stdout=stdout))
    report = assert_ok(_run(assay_root, _fan(root, outcomes={"buf-module": module}, calls=calls), "publish"))
    assert report.status is RailStatus.FAULTED
    assert not any(argv[:2] == ("buf", "push") for argv in calls)


def test_publish_refuses_a_default_label_move_at_the_prepush_resolution(assay_root: AssayHarness) -> None:
    """A label that moves after the gate passed faults the immediate resolver and never reaches push."""
    root = _corpus(assay_root.root)
    moved = "11111111111111111111111111111111"
    sequences = {
        "buf-baseline": [
            Ok(receipt(("buf", "registry"), 0, stdout=msgspec.json.encode({"commit": _BASELINE_COMMIT, "create_time": "2026-08-22"}))),
            Ok(receipt(("buf", "registry"), 0, stdout=msgspec.json.encode({"commit": moved, "create_time": "2026-08-22"}))),
        ]
    }
    calls: list[tuple[str, ...]] = []
    report = assert_ok(_run(assay_root, _fan(root, sequences=sequences, calls=calls), "publish"))
    detail = _detail(report)
    assert report.status is RailStatus.FAULTED and dict(detail.lanes)["buf-prepush"] == "faulted"
    assert detail.baseline == f"{_MODULE}:{_BASELINE_COMMIT}" and not detail.published
    assert "default label moved" in dict(detail.faults)["buf-prepush"]
    assert not any(argv[:2] == ("buf", "push") for argv in calls)


@pytest.mark.parametrize(
    "second",
    [Ok(receipt(("buf", "registry"), 0, stdout=_MODULE_INFO)), Ok(receipt(("buf", "registry"), 1, stderr=b"Failure: unauthenticated\n"))],
    ids=("module-appeared", "resolver-fault"),
)
def test_publish_reproves_bootstrap_absence_immediately_before_push(assay_root: AssayHarness, second: Result[Completed, Fault]) -> None:
    """Bootstrap refuses when the module appears or the second exact-absence lookup cannot prove absence."""
    root = _corpus(assay_root.root)
    absent = Ok(
        receipt(
            ("buf", "registry"),
            1,
            stderr=f'Failure: a module named "{_MODULE}" does not exist, use "buf registry module create" to create one\n'.encode(),
        )
    )
    calls: list[tuple[str, ...]] = []
    report = assert_ok(_run(assay_root, _fan(root, sequences={"buf-module": [absent, second]}, calls=calls), "publish"))
    assert report.status is RailStatus.FAULTED and dict(_detail(report).lanes)["buf-prepush"] == "faulted"
    assert not any(argv[:2] == ("buf", "push") for argv in calls)


# --- [EXIT_ALGEBRA]


@pytest.mark.parametrize(
    "rc, expected",
    [
        (0, RailStatus.EMPTY),
        (BUF_DEFECT_EXIT, RailStatus.FAILED),
        (1, RailStatus.FAULTED),
        (2, RailStatus.FAULTED),
        (124, RailStatus.TIMEOUT),
        (5, RailStatus.BUSY),
    ],
    ids=["clean", "defect", "tool-failure", "other-failure", "timeout", "busy"],
)
def test_defect_exit_algebra_on_buf_rows(rc: int, expected: RailStatus) -> None:
    """The defect exit reads FAILED, any other non-zero exit FAULTED, TIMEOUT and BUSY untouched; rows without defect_exit keep the projection."""
    assert apply_row_status(_LINT, receipt(("buf", "lint"), rc)).status is expected
    plain = msgspec.structs.replace(_LINT, defect_exit=None)
    assert apply_row_status(plain, receipt(("buf", "lint"), rc)).status is RailStatus.from_returncode(rc)


def test_defect_exit_converts_annotations_into_match_rows() -> None:
    """Exit 100 NDJSON annotations fold into ``buf:<rule>`` CODE rows for a non-static claim."""
    done = apply_row_status(_LINT, _stamped(receipt(("buf", "lint"), BUF_DEFECT_EXIT, stdout=_LINT_ROW + b"WARN not json\n"), Parser.BUF))
    report = fold(Claim.CONTRACTS, "check", (done,))
    rows = [row for row in report.results if row.id == "buf:field_lower_snake_case"]
    assert report.status is RailStatus.FAILED
    assert len(rows) == 1 and rows[0].path.endswith("scene.proto") and rows[0].line == 9 and rows[0].severity == "error"


def test_non_defect_exit_faults_its_lane_and_keeps_sibling_rows(assay_root: AssayHarness) -> None:
    """An executable tool failure reads FAULTED with its stderr tail while the lint lane's defect rows survive."""
    root = _corpus(assay_root.root)
    outcomes = {
        "buf-lint": Ok(receipt(("buf", "lint"), BUF_DEFECT_EXIT, stdout=_LINT_ROW)),
        "buf-format": Ok(receipt(("buf", "format"), 1, stderr=b"Failure: formatter could not read the estate module\n")),
    }
    report = assert_ok(_run(assay_root, _fan(root, outcomes=outcomes)))
    detail = _detail(report)
    assert report.status is RailStatus.FAULTED
    assert dict(detail.lanes)["buf-build"] == "ok" and dict(detail.lanes)["buf-format"] == "faulted"
    assert dict(detail.lanes)["buf-lint"] == "failed"
    assert detail.faults == (("buf-format", "Failure: formatter could not read the estate module"),)
    assert detail.violations == (("field_lower_snake_case", "libs/contracts/proto/rasm/contracts/scene/scene.proto", 9),)
    assert any(row.id == "buf:field_lower_snake_case" for row in report.results)


def test_format_lane_projects_diff_headers(assay_root: AssayHarness) -> None:
    """A format diff (exit 100) names the unformatted files through its `+++` headers and fails the lane without writing."""
    root = _corpus(assay_root.root)
    report = assert_ok(_run(assay_root, _fan(root, outcomes={"buf-format": Ok(receipt(("buf", "format"), BUF_DEFECT_EXIT, stdout=_DIFF))})))
    detail = _detail(report)
    assert report.status is RailStatus.FAILED
    assert detail.unformatted == ("libs/contracts/proto/a.proto",)
    assert "--diff" in _FORMAT.command and "--exit-code" in _FORMAT.command and _FORMAT.mode is Mode.CHECK


# --- [FRESHNESS]


def test_freshness_diffs_scratch_against_committed_roots(assay_root: AssayHarness) -> None:
    """Changed, missing, and orphan files per outermost out root fail the lane with a bounded diff artifact; identical trees pass."""
    root = _corpus(assay_root.root)

    def regenerate(scratch: Path) -> None:
        for out in _OUTS:
            shutil.copytree(root / out, scratch / out, dirs_exist_ok=True)
        _write(scratch, "libs/contracts/gen/python/rasm/contracts/demo/demo_pb.txt", "regenerated\n")
        _write(scratch, "libs/contracts/gen/python/rasm/contracts/pub/pub_pb.txt", "new\n")
        (scratch / "libs/contracts/gen/dotnet/demo/demo_pb.txt").unlink()

    report = assert_ok(_run(assay_root, _fan(root, regenerate=regenerate)))
    detail = _detail(report)
    assert report.status is RailStatus.FAILED
    assert detail.stale == (
        ("changed", "libs/contracts/gen/python/rasm/contracts/demo/demo_pb.txt"),
        ("missing", "libs/contracts/gen/python/rasm/contracts/pub/pub_pb.txt"),
        ("orphan", "libs/contracts/gen/dotnet/demo/demo_pb.txt"),
    )
    assert {row.id for row in report.results} == {"freshness:changed", "freshness:missing", "freshness:orphan"}
    diff = next(artifact for artifact in report.artifacts if artifact.id == "freshness-diff")
    assert "-generated" in Path(diff.path).read_text(encoding="utf-8") and "+regenerated" in Path(diff.path).read_text(encoding="utf-8")
    clean = assert_ok(_run(assay_root, _fan(root)))
    assert _detail(clean).stale == () and clean.status is RailStatus.OK


def test_python_typing_marker_is_projected_after_the_sweep(assay_root: AssayHarness) -> None:
    """`py.typed` is a projected distribution, never authored: a swept tree reads it missing, generate lands the empty marker, check reads clean."""
    root = _corpus(assay_root.root)
    marker = root / _PY_MARKER
    marker.unlink()
    stale = assert_ok(_run(assay_root, _fan(root)))
    assert stale.status is RailStatus.FAILED and _detail(stale).stale == (("missing", _PY_MARKER),)
    assert {row.id for row in stale.results} == {"freshness:missing"}
    landed = assert_ok(_run(assay_root, _fan(root), "generate"))
    assert landed.status is RailStatus.OK and f"distribution: {_PY_MARKER} written" in landed.notes
    assert marker.is_file() and marker.read_bytes() == b""
    clean = assert_ok(_run(assay_root, _fan(root)))
    assert clean.status is RailStatus.OK and _detail(clean).stale == ()
    marker.write_bytes(b"# authored\n")
    authored = assert_ok(_run(assay_root, _fan(root)))
    assert authored.status is RailStatus.FAILED and _detail(authored).stale == (("changed", _PY_MARKER),)


def test_out_dirs_and_roster_blocks_derive_from_template(assay_root: AssayHarness) -> None:
    """Exact message/method roots select only Buf's filtered closure; public actors never promote nested or service-container support."""
    root = _corpus(assay_root.root)
    template = assert_ok(read_template(root))
    assert out_dirs(template) == _OUTS
    assert [row.types for row in template.plugins] == [(_THING, _METHOD), (_THING, _METHOD), (_METHOD,), (_THING,), (_METHOD,)]
    assert all(not hasattr(row, "exclude_types") for row in template.plugins)
    assert [row.binary for row in template.plugins] == [
        "node_modules/.bin/protoc-gen-es",
        ".venv/bin/protoc-gen-py",
        ".venv/bin/protoc-gen-connectrpc",
        "",
        "",
    ]
    registry = assert_ok(load_manifest(root / _CORPUS))
    filtered = _roster_files(root, template)
    blocks = {
        language: re.sub(
            r" +", " ", contracts_rail._roster_block(row, filtered[language], contracts_rail._actor_roots(registry, language, row.kinds))
        )
        for language, row in contracts_rail._ROSTERS.items()
    }
    assert "| `Thing_InnerSchema` | message | support-closure | `Thing.Inner` |" in blocks["typescript"]
    assert "`Thing_KindSchema` | enum | support-closure" in blocks["typescript"]
    assert "`DemoService` | service | support-closure | `DemoService` |" in blocks["typescript"]
    assert "`DemoService.Do` | method | public-root | `DemoService.Do` |" in blocks["typescript"]
    assert "[ROSTER_SCOPE]: `pub`" in blocks["typescript"] and "support-closure | `Event`" in blocks["typescript"]
    assert (
        "`Thing.Inner` | message | support-closure" in blocks["python"]
        and "`Thing.Kind` | enum | support-closure" in blocks["python"]
        and "`DemoService.Do` | method | public-root" in blocks["python"]
        and "`Event` | message | support-closure | `Event` |" in blocks["python"]
    )
    assert (
        "`Thing.Types.Inner` | message | support-closure" in blocks["dotnet"]
        and "`Thing.Types.Kind` | enum | support-closure" in blocks["dotnet"]
        and "`DemoService` | service | support-closure" in blocks["dotnet"]
        and "`DemoService.Do` | method | public-root" in blocks["dotnet"]
        and "support-closure | `Event`" in blocks["dotnet"]
    )
    assert all("Unused" not in block for block in blocks.values()) and blocks["typescript"].endswith("|\n\n")


def test_roster_blocks_render_inside_the_markdown_lane(assay_root: AssayHarness) -> None:
    """Emitted spans conform to the markdown lane: every rendered line holds the 150-column cap and the span closes on a blank line."""
    root = _corpus(assay_root.root)
    template = assert_ok(read_template(root))
    registry = assert_ok(load_manifest(root / _CORPUS))
    filtered = _roster_files(root, template)
    for language, row in contracts_rail._ROSTERS.items():
        block = contracts_rail._roster_block(row, filtered[language], contracts_rail._actor_roots(registry, language, row.kinds))
        assert all(len(line) <= 150 for line in block.splitlines()), f"{language} roster exceeds the 150-column cap"
        assert block.endswith("\n\n"), f"{language} roster span must close on the blank line the markdown lane renders"


def test_publisher_message_actor_roots_select_every_direct_generated_message() -> None:
    """Publisher protobuf actors name exact direct roots per language, including batch messages that must not survive as support closure."""
    definition = PublisherDefinition(format="demo protobuf", source="vendor/PUB/pub.proto", origin=_PUB_ORIGIN)
    case = _vendored(
        definition=definition,
        consumers=(
            _message("typescript:Demo/Page/one#CLUSTER", "PublisherProto.read", roots=("pub.Event", "pub.EventBatch")),
            _message("python:Demo/Page/one#CLUSTER", "PublisherProto.read", roots=("pub.Event",)),
        ),
    ).cases[0]
    manifest = Manifest(entries=(msgspec.structs.replace(_PUB, cases=(case,)),))
    assert contracts_rail._actor_roots(manifest, "typescript", frozenset(("message",))) == ("pub.Event", "pub.EventBatch")
    assert contracts_rail._actor_roots(manifest, "python", frozenset(("message",))) == ("pub.Event",)
    assert contracts_rail._actor_needs(manifest) == (
        ("typescript", "message", "pub.Event"),
        ("typescript", "message", "pub.EventBatch"),
        ("python", "message", "pub.Event"),
    )


def test_actor_support_roots_project_only_to_their_descriptor_plugin_kind() -> None:
    """Closed support kinds keep semantic message roots out of service generation and service roots out of message generation."""
    actor = _message(
        "typescript:Demo/Page/one#CLUSTER",
        "DemoSchema.read",
        supports=(
            Support(kind="message", fqn="demo.Unused"),
            Support(kind="service", fqn="demo.OtherService"),
            Support(kind="method", fqn="demo.OtherService.Ping"),
        ),
    )
    entry = _binding(authority=DomainAuthority(producer=actor))
    manifest = Manifest(entries=(entry,))
    assert contracts_rail._actor_roots(manifest, "typescript", frozenset(("message", "enum"))) == (_THING, "demo.Unused")
    assert contracts_rail._actor_roots(manifest, "typescript", frozenset(("enum",))) == (_THING,)
    assert contracts_rail._actor_roots(manifest, "typescript", frozenset(("service",))) == ("demo.OtherService", "demo.OtherService.Ping")
    assert ("typescript", "message", "demo.Unused") in contracts_rail._actor_needs(manifest)
    assert ("typescript", "service", "demo.OtherService") in contracts_rail._actor_needs(manifest)
    assert ("typescript", "method", "demo.OtherService.Ping") in contracts_rail._actor_needs(manifest)


@pytest.mark.parametrize(
    "supports,binding,rule",
    [
        ((Support(kind="message", fqn=_THING),), "generated", "support-redundant"),
        ((Support(kind="message", fqn="pub.Event"),), "generated", "support-redundant"),
        ((Support(kind="service", fqn=_THING),), "generated", "support-kind"),
        ((Support(kind="message", fqn="demo.Ghost"),), "generated", "support-unresolved"),
        ((Support(kind="message", fqn="demo.Unused"),), "package", "support-context"),
        ((Support(kind="message", fqn="demo.Unused"), Support(kind="message", fqn="demo.Unused")), "generated", "support-duplicate"),
    ],
    ids=("boundary", "transitive", "wrong-kind", "unresolved", "non-generated", "duplicate"),
)
def test_actor_support_refuses_redundancy_aliases_and_false_custody(
    tmp_path: Path, supports: tuple[Support, ...], binding: ActorBinding, rule: str
) -> None:
    """Semantic support is an exceptional generated root, never an alias for boundary closure or package custody."""
    producer = _message("typescript:Demo/Page/one#CLUSTER", "DemoSchema.read", binding, supports=supports)
    entry = _binding(authority=DomainAuthority(producer=producer))
    rules, rows = _audit_rules(_corpus(tmp_path, (entry, _vendored())), _image())
    assert rule in rules, "\n".join(rows)


def test_actor_support_refuses_generated_non_protobuf_context(tmp_path: Path) -> None:
    """A generated label alone cannot turn schema or law ownership into descriptor support."""
    producer = _message("typescript:Demo/Page/one#CLUSTER", "DemoSchema.read", supports=(Support(kind="message", fqn="demo.Unused"),))
    entry = _entry(authority=DomainAuthority(producer=producer))
    rules, rows = _audit_rules(_corpus(tmp_path, (entry, _binding(), _vendored())), _image())
    assert "support-context" in rules, "\n".join(rows)


def test_publisher_oracle_refuses_evidence_outside_source_and_license_drift(tmp_path: Path) -> None:
    """Publisher proof binds both the exact upstream source subtree and its colocated license bytes."""
    case = _PUB_CASE
    readiness = case.readiness
    assert isinstance(readiness, VerifiedReadiness)
    stray = _specimen("vendor/PUB/stray.bin", _PUB_BYTES, "sha256")
    held = msgspec.structs.replace(case, readiness=msgspec.structs.replace(readiness, vectors=(ProofVector(specimens=(stray,)),)))
    entry = msgspec.structs.replace(_PUB, cases=(held,))
    root = _corpus(tmp_path, (_entry(), _binding(), entry))
    _write(root / _CORPUS, "vendor/PUB/stray.bin", _PUB_BYTES)
    rules, rows = _audit_rules(root, _image())
    assert "publisher-source" in rules, "\n".join(rows)
    _write(root / _CORPUS, "vendor/PUB/LICENSE", b"drifted\n")
    rules, rows = _audit_rules(root, _image())
    assert "publisher-license" in rules, "\n".join(rows)


@pytest.mark.parametrize(
    "types, rules",
    [
        ("demo", frozenset(("selector-drift", "selector-generic"))),
        (_THING, frozenset(("selector-drift",))),
        (f"{_THING}, {_METHOD}, {_REPLY}", frozenset(("selector-drift",))),
        (f"{_THING}, demo.Ghost", frozenset(("selector-drift", "selector-generic"))),
    ],
    ids=("generic", "missing", "extra", "unknown"),
)
def test_selector_roots_are_exact_actor_fqns(tmp_path: Path, types: str, rules: frozenset[str]) -> None:
    """A package prefix, missing actor method, known extra, and unknown symbol all fail against the manifest-derived exact root list."""
    template = _TEMPLATE.replace(f"types: [{_THING}, {_METHOD}]", f"types: [{types}]", 1)
    actual, rows = _audit_rules(_corpus(tmp_path, template=template), _image())
    assert rules <= actual, "\n".join(rows)


def test_generated_rpc_actor_requires_a_service_plugin(tmp_path: Path) -> None:
    """Deleting one language's service emitter leaves its generated RPC actor uncovered even though message generation still exists."""
    service = (
        f"  - remote: buf.build/grpc/csharp:v1.83.0\n    revision: 1\n    out: libs/contracts/gen/dotnet\n"
        f"    types: [{_METHOD}]\n    include_imports: true\n"
    )
    rules, rows = _audit_rules(_corpus(tmp_path, template=_TEMPLATE.replace(service, "")), _image())
    assert "selector-coverage" in rules, "\n".join(rows)


def test_one_out_root_per_emission_target_carries_estate_and_publisher_roots_alike(tmp_path: Path) -> None:
    """A publisher-package root rides the same `types` row as the estate roots; a second row curating it alone drifts instead of routing it."""
    entries = (_entry(), _BINDING, _PUB_EVENT, _vendored())
    python_row = f"out: {_OUTS[1]}\n    opt: [init_files=false]\n    types: [{_THING}, {_METHOD}]"
    dotnet_row = f"out: {_OUTS[2]}\n    types: [{_THING}]"
    union = _TEMPLATE.replace(python_row, python_row.replace(f"{_METHOD}]", f"{_METHOD}, {_EVENT}]"))
    union = union.replace(dotnet_row, dotnet_row.replace(f"{_THING}]", f"{_THING}, {_EVENT}]"))
    assert union != _TEMPLATE
    rules, rows = _audit_rules(_corpus(tmp_path / "union", entries, template=union), _image())
    assert not rules & {"selector-drift", "selector-coverage", "selector-generic"}, "\n".join(rows)
    rules, rows = _audit_rules(_corpus(tmp_path / "estate-only", entries), _image())
    assert "selector-drift" in rules, "\n".join(rows)
    vendor_row = f"  - local: .venv/bin/protoc-gen-py\n    out: {_OUTS[1]}/vendor\n    opt: [init_files=false]\n    types: [{_EVENT}]\n"
    split = union + vendor_row + "    include_imports: true\n"
    rules, rows = _audit_rules(_corpus(tmp_path / "split", entries, template=split), _image())
    drifted = [row for row in rows if row.startswith(f"selector-drift: python:{_OUTS[1]}/vendor:")]
    assert drifted and f"({_THING!r}, {_METHOD!r}, {_EVENT!r})" in drifted[0], "\n".join(rows)


@pytest.mark.parametrize("coordinate", ["Missing.symbol", "Either|Other"], ids=("missing-literal", "non-singular"))
def test_actor_coordinate_is_one_literal_symbol_in_its_anchor(tmp_path: Path, coordinate: str) -> None:
    """An actor coordinate is one source symbol and appears literally inside the exact anchored cluster."""
    producer = _message("dotnet:Demo/Page/one#CLUSTER", coordinate, "proof")
    rules, rows = _audit_rules(_corpus(tmp_path, (_entry(authority=DomainAuthority(producer=producer)), _BINDING, _vendored())), _image())
    assert "actor-coordinate" in rules, "\n".join(rows)


def test_application_authority_requires_an_inbound_consumer(tmp_path: Path) -> None:
    """Application authority without a reader is an unowned inbound boundary, not an absent producer exception."""
    entry = _entry(authority=ApplicationAuthority(), consumers=())
    rules, rows = _audit_rules(_corpus(tmp_path, (entry, _BINDING, _vendored())), _image())
    assert "authority-actors" in rules, "\n".join(rows)


def test_domain_producer_without_a_consumer_is_not_a_contract_crossing(tmp_path: Path) -> None:
    """A domain producer or descriptor alone does not establish a process boundary."""
    entry = _entry(consumers=())
    rules, rows = _audit_rules(_corpus(tmp_path, (entry, _BINDING, _vendored())), _image())
    assert "authority-actors" in rules, "\n".join(rows)


def test_infrastructure_minters_without_a_consumer_are_not_a_contract_crossing(tmp_path: Path) -> None:
    """Independent minters prove shared creation semantics only when one exact process reader consumes the value."""
    minters = (
        _message("dotnet:Demo/Page/one#CLUSTER", "DemoSchema.write", "proof"),
        _message("python:Demo/Page/one#CLUSTER", "DemoSchema.read", "proof"),
    )
    entry = _entry(authority=InfrastructureAuthority(minters=minters), consumers=())
    rules, rows = _audit_rules(_corpus(tmp_path, (entry, _BINDING, _vendored())), _image())
    assert "authority-actors" in rules, "\n".join(rows)


def test_publisher_custody_requires_no_process_consumer(tmp_path: Path) -> None:
    """Immutable publisher custody remains lawful even when no estate process reads the bytes yet."""
    publisher = _vendored(consumers=())
    rules, rows = _audit_rules(_corpus(tmp_path, (_entry(), _BINDING, publisher)), _image())
    assert rules == frozenset(), "\n".join(rows)


def test_application_authority_requires_an_executable_reader(tmp_path: Path) -> None:
    """A proof-only observer cannot establish the generated or package ingress an application supplies."""
    consumer = _message("python:Demo/Page/one#CLUSTER", "ApplicationInput.byte", "proof")
    entry = _entry(authority=ApplicationAuthority(), consumers=(consumer,))
    rules, rows = _audit_rules(_corpus(tmp_path, (entry, _BINDING, _vendored())), _image())
    assert "authority-actors" in rules, "\n".join(rows)


def test_application_authority_refuses_a_framing_law_definition(tmp_path: Path) -> None:
    """Application authority owns typed inbound values, never an ownerless framing law."""
    consumer = _message("python:Demo/Page/one#CLUSTER", "ApplicationInput.byte", "package")
    entry = _entry(
        definition=LawDefinition(anchor="libs/.planning/ARCHITECTURE.md#[14]-[EVENT_FABRIC]", format="text"),
        authority=ApplicationAuthority(),
        consumers=(consumer,),
    )
    rules, rows = _audit_rules(_corpus(tmp_path, (entry, _BINDING, _vendored())), _image())
    assert "authority-definition" in rules, "\n".join(rows)


def test_application_consumers_may_share_an_anchor_when_coordinates_differ(tmp_path: Path) -> None:
    """One anchored ingress cluster may expose distinct byte and socket readers without aliasing their identities."""
    consumers = (
        _message("python:Demo/Page/one#CLUSTER", "ApplicationInput.byte", "package"),
        _message("python:Demo/Page/one#CLUSTER", "ApplicationInput.socket", "generated"),
    )
    entry = _entry(authority=ApplicationAuthority(), consumers=consumers)
    rules, rows = _audit_rules(_corpus(tmp_path, (entry, _BINDING, _vendored())), _image())
    assert rules == frozenset(), "\n".join(rows)


def test_application_consumer_exact_identity_cannot_repeat(tmp_path: Path) -> None:
    """Repeating one anchor-coordinate identity is duplicate ownership even when both rows name a lawful reader."""
    consumer = _message("python:Demo/Page/one#CLUSTER", "ApplicationInput.byte", "package")
    entry = _entry(authority=ApplicationAuthority(), consumers=(consumer, consumer))
    rules, rows = _audit_rules(_corpus(tmp_path, (entry, _BINDING, _vendored())), _image())
    assert "actor-duplicate" in rules, "\n".join(rows)


def test_faulted_filtered_roster_blocks_generation(assay_root: AssayHarness) -> None:
    """One failed filtered descriptor image blocks both the public generator and roster emitter while sibling closure lanes survive."""
    root = _corpus(assay_root.root)
    calls: list[tuple[str, ...]] = []
    report = assert_ok(
        _run(
            assay_root,
            _fan(root, outcomes={"buf-roster:python-03": Ok(receipt(("buf", "build"), 1, stderr=b"filtered closure failed\n"))}, calls=calls),
            "generate",
        )
    )
    lanes = dict(_detail(report).lanes)
    assert report.status is RailStatus.FAILED and lanes["buf-roster:python-03"] == "failed"
    assert lanes["buf-generate"] == lanes["corpus-emit"] == "skip"
    assert not any("--clean" in argv for argv in calls)


def test_remote_csharp_emitter_identities_are_versioned_and_exact() -> None:
    message = contracts_rail._Plugin(remote="buf.build/protocolbuffers/csharp:v36.0", out=_OUTS[2])
    service = contracts_rail._Plugin(remote="buf.build/grpc/csharp:v1.83.0", out=_OUTS[2])
    unversioned = contracts_rail._Plugin(remote="buf.build/protocolbuffers/csharp", out=_OUTS[2])
    lookalike = contracts_rail._Plugin(remote="buf.build/example/protocolbuffers-csharp:v36.0", out=_OUTS[2])

    assert message.kinds == frozenset(("message", "enum"))
    assert service.kinds == frozenset(("service", "method"))
    assert unversioned.kinds == lookalike.kinds == frozenset()


@pytest.mark.parametrize(
    "out, language",
    [
        (_OUTS[0], "typescript"),
        (_OUTS[1], "python"),
        (_OUTS[2], "dotnet"),
        ("libs/contracts/gen/dotnet/", "dotnet"),
        ("libs/contracts/gen", ""),
        ("libs/contracts/generated/python", ""),
        ("libs/contracts/gen-python", ""),
        ("libs/python/demo/rasm/demo", ""),
        ("gen/python", ""),
    ],
    ids=("typescript", "python-nested", "dotnet", "trailing-slash", "tree-itself", "lookalike-sibling", "lookalike-prefix", "folder", "relative"),
)
def test_plugin_emission_target_is_the_first_segment_strictly_inside_the_generated_tree(out: str, language: str) -> None:
    """An out root names its target by the segment beneath `libs/contracts/gen`; the tree itself, a lookalike, or any other root names none."""
    assert contracts_rail._Plugin(local="x", out=out).language == language


def test_semantic_roundtrip_accepts_messages_that_reach_a_map(tmp_path: Path) -> None:
    scalar_entry = DescriptorProto(
        name="TagsEntry",
        options=MessageOptions(map_entry=True),
        field=[
            FieldDescriptorProto(name="key", number=1, label=FieldDescriptorProto.Label.OPTIONAL, type=FieldDescriptorProto.Type.STRING),
            FieldDescriptorProto(name="value", number=2, label=FieldDescriptorProto.Label.OPTIONAL, type=FieldDescriptorProto.Type.STRING),
        ],
    )
    message_entry = DescriptorProto(
        name="EventsEntry",
        options=MessageOptions(map_entry=True),
        field=[
            FieldDescriptorProto(name="key", number=1, label=FieldDescriptorProto.Label.OPTIONAL, type=FieldDescriptorProto.Type.STRING),
            FieldDescriptorProto(
                name="value", number=2, label=FieldDescriptorProto.Label.OPTIONAL, type=FieldDescriptorProto.Type.MESSAGE, type_name=".pub.Event"
            ),
        ],
    )
    owner = DescriptorProto(
        name="MapOwner",
        nested_type=[scalar_entry, message_entry],
        field=[
            FieldDescriptorProto(
                name="tags",
                number=1,
                label=FieldDescriptorProto.Label.REPEATED,
                type=FieldDescriptorProto.Type.MESSAGE,
                type_name=".demo.MapOwner.TagsEntry",
            ),
            FieldDescriptorProto(
                name="events",
                number=2,
                label=FieldDescriptorProto.Label.REPEATED,
                type=FieldDescriptorProto.Type.MESSAGE,
                type_name=".demo.MapOwner.EventsEntry",
            ),
        ],
    )
    picture = FileDescriptorSet(
        file=[
            FileDescriptorProto(name="demo/map.proto", package="demo", dependency=["pub/pub.proto"], message_type=[owner]),
            FileDescriptorProto(
                name="pub/pub.proto",
                package="pub",
                message_type=[
                    DescriptorProto(
                        name="Event",
                        field=[
                            FieldDescriptorProto(
                                name="id", number=1, label=FieldDescriptorProto.Label.OPTIONAL, type=FieldDescriptorProto.Type.STRING
                            )
                        ],
                    )
                ],
            ),
        ]
    ).to_binary()
    raw = b"\x0a\x06\x0a\x01a\x12\x01b\x12\x08\x0a\x01c\x12\x03\x0a\x01x"
    specimen = _specimen("MAP/map.bin", raw)
    case = Case(
        id="map",
        definition=ProtoDefinition(message="demo.MapOwner", framing="proto-binary"),
        authority=DomainAuthority(producer=_message(_README, "MapOwner.prove", "proof")),
        consumers=(_message(_README, "MapOwner.consume", "proof"),),
        readiness=VerifiedReadiness(oracle="semantic-roundtrip", vectors=(ProofVector(specimens=(specimen,)),)),
    )
    _write(tmp_path, specimen.path, raw)
    image = _write(tmp_path, "map.binpb", picture)
    receipt_row = assert_ok(contracts_rail.prove_case(tmp_path, "MAP/map", case, image=image))
    assert receipt_row.findings == ()

    unknown_raw = b"\x12\x0a\x0a\x01c\x12\x05\x0a\x01x\x10\x01"
    unknown = _specimen("MAP/unknown.bin", unknown_raw)
    readiness = case.readiness
    assert isinstance(readiness, VerifiedReadiness)
    broken = msgspec.structs.replace(case, readiness=msgspec.structs.replace(readiness, vectors=(ProofVector(specimens=(unknown,)),)))
    _write(tmp_path, unknown.path, unknown_raw)
    findings = assert_ok(contracts_rail.prove_case(tmp_path, "MAP/map", broken, image=image)).findings
    assert len(findings) == 1 and findings[0].rule == "roundtrip-unknown" and ".events['c']" in findings[0].detail


def test_semantic_roundtrip_rejects_unknown_fields_but_accepts_the_exact_descriptor(tmp_path: Path) -> None:
    """Dynamic descriptor decoding finds unknown wire tags recursively while an exact known nested message remains admissible."""

    def rules(name: str, raw: bytes) -> frozenset[str]:
        specimen = _specimen("DEMO_SEAM/demo.bin", raw)
        case = msgspec.structs.replace(
            _DEMO_CASE,
            definition=ProtoDefinition(message=_THING, framing="proto-binary"),
            authority=DomainAuthority(producer=_message(_README, "Thing.prove", "proof")),
            consumers=(_message(_README, "Thing.consume", "proof"),),
            readiness=VerifiedReadiness(oracle="semantic-roundtrip", vectors=(ProofVector(specimens=(specimen,)),)),
        )
        root = tmp_path / name
        _write(root, specimen.path, raw)
        image = _write(root, "image.binpb", _image())
        return frozenset(row.rule for row in assert_ok(contracts_rail.prove_case(root, "DEMO_SEAM/demo", case, image=image)).findings)

    assert rules("unknown", b"\x10\x01") == frozenset(("roundtrip-unknown",))
    assert rules("known", b"\x0a\x03\x0a\x01x") == frozenset()


def test_semantic_roundtrip_refuses_independent_infrastructure_minters(tmp_path: Path) -> None:
    """Normalized protobuf round-trip belongs to one producer; independent minters require typed value parity."""
    specimen = _specimen("DEMO_SEAM/demo.bin", b"\x0a\x03\x0a\x01x")
    minters = (_message("dotnet:Demo/Page/one#CLUSTER", "Thing.first", "proof"), _message("python:Demo/Page/one#CLUSTER", "Thing.second", "proof"))
    case = msgspec.structs.replace(
        _DEMO_CASE,
        definition=ProtoDefinition(message=_THING, framing="proto-binary"),
        authority=InfrastructureAuthority(minters=minters),
        readiness=VerifiedReadiness(oracle="semantic-roundtrip", vectors=(ProofVector(specimens=(specimen,)),)),
    )
    _write(tmp_path, specimen.path, b"\x0a\x03\x0a\x01x")
    findings = assert_ok(contracts_rail.prove_case(tmp_path, "DEMO_SEAM/demo", case, image=_write(tmp_path, "image.binpb", _image()))).findings
    assert any(row.rule == "oracle-authority" for row in findings)


def _parity_case(raw: bytes, expected_raw: bytes) -> tuple[Case, tuple[MessageActor, MessageActor]]:
    minters = (_message("dotnet:Demo/Page/one#CLUSTER", "Parity.dotnet", "proof"), _message("python:Demo/Page/one#CLUSTER", "Parity.python", "proof"))
    specimens = tuple(
        _specimen(f"PARITY/{language}.bin", raw, minter=contracts_rail.actor_key(actor))
        for language, actor in zip(("dotnet", "python"), minters, strict=True)
    )
    expected = _expected("PARITY/expected.json", expected_raw, "content-digest")
    return (
        Case(
            id="parity",
            definition=LawDefinition(anchor="libs/.planning/ARCHITECTURE.md#[14]-[EVENT_FABRIC]", format="binary"),
            authority=InfrastructureAuthority(minters=minters),
            readiness=VerifiedReadiness(oracle="value-parity", vectors=(ProofVector(specimens=specimens, expected=expected),)),
            consumers=(_message(_README, "DemoSchema.prove", "proof"),),
        ),
        minters,
    )


@pytest.mark.parametrize(
    "mutate",
    [
        lambda specimens, _minters: (msgspec.structs.replace(specimens[0], minter=""), specimens[1]),
        lambda specimens, minters: (
            specimens[0],
            msgspec.structs.replace(specimens[1], path=specimens[0].path, minter=contracts_rail.actor_key(minters[1])),
        ),
        lambda specimens, _minters: (specimens[0], msgspec.structs.replace(specimens[1], minter=f"{_README}@Wrong.minter")),
        lambda specimens, _minters: (specimens[0],),
    ],
    ids=("missing-minter", "duplicate-path-different-minter", "wrong-minter", "one-specimen"),
)
def test_value_parity_requires_exactly_one_distinct_specimen_per_minter(
    tmp_path: Path, mutate: Callable[[tuple[SpecimenAsset, ...], tuple[MessageActor, MessageActor]], tuple[SpecimenAsset, ...]]
) -> None:
    """Every value-parity vector carries the exact minter-key set once over distinct specimen paths."""
    raw = b"parity\n"
    expected_raw = msgspec.json.encode(contracts_rail.ContentDigestFacts(algorithm="xxh128", seed=0, value=xxhash.xxh128(raw, seed=0).hexdigest()))
    case, minters = _parity_case(raw, expected_raw)
    readiness = case.readiness
    assert isinstance(readiness, VerifiedReadiness)
    vector = readiness.vectors[0]
    broken = msgspec.structs.replace(
        case, readiness=msgspec.structs.replace(readiness, vectors=(msgspec.structs.replace(vector, specimens=mutate(vector.specimens, minters)),))
    )
    for specimen in vector.specimens:
        _write(tmp_path, specimen.path, raw)
    _write(tmp_path, vector.expected.path if vector.expected is not None else "", expected_raw)
    receipt_row = assert_ok(contracts_rail.prove_case(tmp_path, "PARITY/parity", broken))
    assert {finding.rule for finding in receipt_row.findings} == {"proof-provenance"}


def test_value_parity_accepts_exact_minter_provenance_and_nonparity_refuses_it(tmp_path: Path) -> None:
    """Exact independent provenance discharges parity, while every other oracle reserves the minter field as empty."""
    raw = b"parity\n"
    expected_raw = msgspec.json.encode(contracts_rail.ContentDigestFacts(algorithm="xxh128", seed=0, value=xxhash.xxh128(raw, seed=0).hexdigest()))
    case, minters = _parity_case(raw, expected_raw)
    readiness = case.readiness
    assert isinstance(readiness, VerifiedReadiness)
    vector = readiness.vectors[0]
    for specimen in vector.specimens:
        _write(tmp_path, specimen.path, raw)
    assert vector.expected is not None
    _write(tmp_path, vector.expected.path, expected_raw)
    assert assert_ok(contracts_rail.prove_case(tmp_path, "PARITY/parity", case)).findings == ()

    demo_readiness = _DEMO_CASE.readiness
    assert isinstance(demo_readiness, VerifiedReadiness)
    specimen = msgspec.structs.replace(demo_readiness.vectors[0].specimens[0], minter=contracts_rail.actor_key(minters[0]))
    broken = msgspec.structs.replace(_DEMO_CASE, readiness=msgspec.structs.replace(demo_readiness, vectors=(ProofVector(specimens=(specimen,)),)))
    _write(tmp_path, specimen.path, _ASSET_DOC)
    receipt_row = assert_ok(contracts_rail.prove_case(tmp_path, "DEMO_SEAM/demo", broken))
    assert {finding.rule for finding in receipt_row.findings} == {"proof-provenance"}


def test_sparse_hdf_requires_canonical_uint8_boolean() -> None:
    """Sparse archive booleans admit only HDF5 uint8 0/1; wider integer spellings are rejected."""
    expected = contracts_rail.SparseFacts(
        path="/A",
        attributes=contracts_rail.SparseAttributesFacts(fill=0, format="csc", frobenius=2.0, kind="lu", ordering=1, shape=(2, 2), symmetric=True),
        indices=(0, 1),
        indptr=(0, 1, 2),
        permutation=(0, 1),
        values=(1.0, 2.0),
    )

    class Dtype:
        metadata: dict[str, object] | None = None

        def __init__(self, wire: str, name: str) -> None:
            self.descr, self.name = [("", wire)], name

    class Scalar[T: float]:
        def __init__(self, value: T, wire: str, name: str) -> None:
            self.value, self.dtype = value, Dtype(wire, name)

        def item(self) -> T:
            return self.value

    class Array:
        def __init__(self, values: list[int], wire: str, name: str) -> None:
            self.values, self.dtype = values, Dtype(wire, name)

        def tolist(self) -> list[int]:
            return self.values

    class Dataset[T: float]:
        chunks = None
        compression = None
        compression_opts = None
        shuffle = False

        def __init__(self, values: list[T], wire: str, name: str) -> None:
            self.values, self.dtype, self.shape = values, Dtype(wire, name), (len(values),)
            self.attrs: dict[str, object] = {}

        def __getitem__(self, _key: tuple[()]) -> list[T]:
            return self.values

    class Group:
        def __init__(self, symmetric: Scalar[int]) -> None:
            self.attrs: dict[str, object] = {
                "fill": Scalar(0, "<i8", "int64"),
                "format": "csc",
                "frobenius": Scalar(2.0, "<f8", "float64"),
                "kind": "lu",
                "ordering": Scalar(1, "<i8", "int64"),
                "shape": Array([2, 2], "<i8", "int64"),
                "symmetric": symmetric,
            }
            self.nodes: dict[str, object] = {
                "indices": Dataset([0, 1], "<i4", "int32"),
                "indptr": Dataset([0, 1, 2], "<i4", "int32"),
                "permutation": Dataset([0, 1], "<i4", "int32"),
                "values": Dataset([1.0, 2.0], "<f8", "float64"),
            }

        def keys(self) -> tuple[str, ...]:
            return tuple(self.nodes)

        def __getitem__(self, name: str) -> object:
            return self.nodes[name]

    class File(Group):
        def __init__(self, symmetric: Scalar[int]) -> None:
            super().__init__(symmetric)
            self.attrs = {}
            self.nodes = {"A": Group(symmetric)}

        def __enter__(self) -> Self:
            return self

        def __exit__(self, exc_type: object, exc_value: object, traceback: object) -> None:
            return None

    invalid: contracts_rail._HdfFile = File(Scalar(1, "<i8", "int64"))
    valid: contracts_rail._HdfFile = File(Scalar(1, "|u1", "uint8"))
    with pytest.raises(ValueError, match="canonical HDF5 dtype"):
        contracts_rail._sparse_facts(invalid)
    assert contracts_rail._sparse_facts(valid) == expected


def test_blocked_law_does_not_require_a_seam_directory(tmp_path: Path) -> None:
    blocked = Entry(
        id="GHOST",
        law="A design-only law carries no fabricated directory before evidence exists.",
        cases=(
            msgspec.structs.replace(
                _DEMO_CASE,
                id="law",
                definition=LawDefinition(anchor="libs/.planning/ARCHITECTURE.md#[14]-[EVENT_FABRIC]", format="text"),
                authority=DomainAuthority(producer=_message(_README, "Law.prove", "proof")),
                readiness=BlockedReadiness(blockers=("No source tree executes the design-only law, so no corpus evidence exists.",)),
                consumers=(),
            ),
        ),
    )
    root = _corpus(tmp_path, (blocked, _vendored()))

    assert "seam-missing" not in _audit_rules(root, _image())[0]


# --- [PLUGIN_PROBE]


def test_plugin_miss_degrades_check_and_refuses_generate(assay_root: AssayHarness) -> None:
    """A missing plugin binary reads UNSUPPORTED with an installer hint, skips scratch generation and freshness, and refuses generate outright."""
    root = _corpus(assay_root.root, binaries=("node_modules/.bin/protoc-gen-es", "tools/grpc_csharp_plugin"))
    report = assert_ok(_run(assay_root, _fan(root)))
    detail = _detail(report)
    assert report.status is RailStatus.UNSUPPORTED
    assert dict(detail.lanes) == {
        "plugin-probe": "unsupported",
        "buf-build": "ok",
        "buf-lint": "ok",
        "buf-format": "ok",
        "buf-generate": "skip",
        "buf-roster:typescript-01": "ok",
        "buf-roster:python-02": "ok",
        "buf-roster:python-03": "ok",
        "buf-roster:dotnet-04": "ok",
        "buf-roster:dotnet-05": "ok",
        "corpus-gate": "ok",
        "freshness-gate": "skip",
    }
    assert not dict(detail.plugins)[".venv/bin/protoc-gen-py"]
    refused = assert_error_status(_run(assay_root, _fan(root), "generate"), RailStatus.UNSUPPORTED)
    assert ".venv/bin/protoc-gen-py: uv sync" in refused.message


def test_plugin_hint_names_the_installer_by_seat() -> None:
    """Venv binaries hint `uv sync`, node binaries `pnpm install`, everything else the machine estate."""
    assert [
        contracts_rail._hint(spelling) for spelling in (".venv/bin/protoc-gen-py", "node_modules/.bin/protoc-gen-es", "protoc", "grpc_csharp_plugin")
    ] == ["uv sync", "pnpm install", contracts_rail._MACHINE_HINT, contracts_rail._MACHINE_HINT]


# --- [CORPUS_AUDIT]

_DEFECTS: tuple[tuple[str, Callable[[Path], Path], str], ...] = (
    ("manifest-missing", lambda t: _after(_corpus(t), lambda r: (r / _CORPUS / "manifest.json").unlink()), "manifest-missing"),
    ("manifest-undecodable", lambda t: _corpus(t, manifest=b'{"entries": [{"id": "X"}]}'), "manifest-undecodable"),
    ("schema-missing", lambda t: _after(_corpus(t), lambda r: (r / _CORPUS / "manifest.schema.json").unlink()), "schema-missing"),
    ("schema-stale", lambda t: _corpus(t, schema=b"{}\n"), "schema-stale"),
    ("id-duplicate", lambda t: _corpus(t, (_entry(), _entry(), _vendored())), "id-duplicate"),
    ("seam-duplicate", lambda t: _corpus(t, (_entry(), _entry(id="demo-seam"), _vendored())), "seam-duplicate"),
    ("case-duplicate", lambda t: _corpus(t, (_entry(cases=(_DEMO_CASE, _DEMO_CASE)), _vendored())), "case-duplicate"),
    ("cases-empty", lambda t: _corpus(t, (_entry(cases=()), _vendored())), "cases-empty"),
    (
        "authority-actors",
        lambda t: _corpus(
            t,
            (
                _entry(authority=InfrastructureAuthority(minters=(_message("dotnet:Demo/Page/one#CLUSTER", "DemoSchema.write", "proof"),))),
                _vendored(),
            ),
        ),
        "authority-actors",
    ),
    ("authority-definition", lambda t: _corpus(t, (_entry(authority=PublisherAuthority()), _vendored())), "authority-definition"),
    (
        "publisher-readiness",
        lambda t: _corpus(
            t, (_entry(), _vendored(readiness=BlockedReadiness(blockers=("No immutable publisher specimen has been admitted into custody yet.",))))
        ),
        "publisher-readiness",
    ),
    (
        "readiness-vectors",
        lambda t: _corpus(t, (_entry(readiness=VerifiedReadiness(oracle="external-digest", vectors=())), _vendored())),
        "readiness-vectors",
    ),
    (
        "proof-vector",
        lambda t: _corpus(t, (_entry(readiness=VerifiedReadiness(oracle="external-digest", vectors=(ProofVector(specimens=()),))), _vendored())),
        "proof-vector",
    ),
    ("blocker-short", lambda t: _corpus(t, (_entry(readiness=BlockedReadiness(blockers=("gap",))), _vendored())), "blocker-short"),
    (
        "blocker-closed",
        lambda t: _corpus(t, (_entry(readiness=BlockedReadiness(blockers=("This blocker was DISCHARGED by the landing.",))), _vendored())),
        "blocker-closed",
    ),
    ("law-length", lambda t: _corpus(t, (_entry(law="x" * 241), _vendored())), "law-length"),
    ("law-empty", lambda t: _corpus(t, (_entry(law="  "), _vendored())), "law-empty"),
    (
        "actor-direction",
        lambda t: _corpus(
            t,
            (
                _entry(),
                _binding(
                    authority=DomainAuthority(producer=_server_response("typescript:Demo/Page/one#CLUSTER", "DemoRequest.call")),
                    consumers=(
                        _client_response("python:Demo/Page/one#CLUSTER", "DemoRequest.handle"),
                        _client_response("dotnet:Demo/Page/one#CLUSTER", "DemoRequest.Handle"),
                    ),
                ),
                _vendored(),
            ),
        ),
        "actor-direction",
    ),
    (
        "definition-missing",
        lambda t: _corpus(
            t,
            (
                _entry(
                    definition=SchemaDefinition(
                        path=f"{_SEAM_DIR}/absent.schema.json", framing="canonical-json", derived_from="msgspec:demo_docs.shape.Demo"
                    )
                ),
                _vendored(),
            ),
        ),
        "definition-missing",
    ),
    ("definition-invalid", lambda t: _corpus(t, seam_schema={**_HAND_SCHEMA, "type": 7}), "definition-invalid"),
    ("definition-id", lambda t: _corpus(t, seam_schema={**_HAND_SCHEMA, "$id": "elsewhere.json"}), "definition-id"),
    ("definition-ref", lambda t: _corpus(t, seam_schema={**_HAND_SCHEMA, "properties": {"key": {"$ref": "#/$defs/ghost"}}}), "definition-ref"),
    (
        "definition-anchor",
        lambda t: _corpus(t, (_entry(definition=LawDefinition(anchor="docs/laws/patterns.md", format="text")), _vendored())),
        "definition-anchor",
    ),
    ("schema-stale-seam", lambda t: _corpus(t, seam_schema=derived_schema(Demo, identity=_SEAM_ID) + b"\n"), "schema-stale"),
    (
        "schema-framing",
        lambda t: _corpus(
            t, (_entry(definition=msgspec.structs.replace(_PROTO_DEFINITION, framing="proto-binary")), _vendored()), definition=_THING_PATH
        ),
        "schema-framing",
    ),
    (
        "schema-derivation",
        lambda t: _corpus(t, (_entry(definition=_PROTO_DEFINITION), _vendored()), definition=_THING_PATH, seam_schema=_bundle(_THING)),
        "schema-derivation",
    ),
    (
        "schema-owner",
        lambda t: _corpus(
            t,
            (
                _entry(definition=SchemaDefinition(path=_SEAM_PATH, framing="canonical-json", derived_from="msgspec:demo_docs.shape.Ghost")),
                _vendored(),
            ),
        ),
        "schema-derivation",
    ),
    (
        "message-unresolved",
        lambda t: _corpus(t, (_binding(definition=ProtoDefinition(message="demo.Ghost", framing="proto-binary")), _vendored())),
        "message-unresolved",
    ),
    ("message-empty", lambda t: _corpus(t, (_binding(definition=ProtoDefinition(message="", framing="proto-binary")), _vendored())), "message-empty"),
    (
        "event-type",
        lambda t: _corpus(
            t,
            (
                _binding(
                    definition=CloudEventDefinition(
                        message="io.cloudevents.v1.CloudEvent", framing="proto-binary", type="rasm.scene.frame.rendered.v1"
                    )
                ),
                _vendored(),
            ),
        ),
        "event-type",
    ),
    (
        "method-unresolved",
        lambda t: _corpus(
            t,
            (
                _entry(),
                _binding(
                    authority=DomainAuthority(
                        producer=_client_request("typescript:Demo/Page/one#CLUSTER", "DemoRequest.call", method="demo.DemoService.Ghost")
                    )
                ),
                _vendored(),
            ),
        ),
        "method-unresolved",
    ),
    (
        "publisher-format",
        lambda t: _corpus(t, (_entry(), _vendored(definition=PublisherDefinition(format=" ", source="vendor/PUB/pub.bin", origin=_PUB_ORIGIN)))),
        "publisher-format",
    ),
    (
        "asset-bytes",
        lambda t: _corpus(
            t,
            (
                _entry(
                    readiness=VerifiedReadiness(
                        oracle="external-digest",
                        vectors=(ProofVector(specimens=(msgspec.structs.replace(_specimen(f"{_SEAM_DIR}/demo.json", _ASSET_DOC), bytes=1),)),),
                    )
                ),
                _vendored(),
            ),
        ),
        "asset-bytes",
    ),
    (
        "asset-digest",
        lambda t: _corpus(
            t,
            (
                _entry(
                    readiness=VerifiedReadiness(
                        oracle="external-digest",
                        vectors=(
                            ProofVector(
                                specimens=(
                                    msgspec.structs.replace(
                                        _specimen(f"{_SEAM_DIR}/demo.json", _ASSET_DOC), fingerprint=Fingerprint(algorithm="xxh128", value="0" * 32)
                                    ),
                                )
                            ),
                        ),
                    )
                ),
                _vendored(),
            ),
        ),
        "asset-digest",
    ),
    (
        "asset-invalid",
        lambda t: _corpus(
            t,
            (
                _entry(
                    readiness=VerifiedReadiness(
                        oracle="external-digest", vectors=(ProofVector(specimens=(_specimen(f"{_SEAM_DIR}/demo.json", b'{"key": 1}\n'),)),)
                    )
                ),
                _vendored(),
            ),
            asset=b'{"key": 1}\n',
        ),
        "asset-invalid",
    ),
    (
        "distribution-path",
        lambda t: _corpus(t, (_entry(), _distributed(msgspec.structs.replace(_DISTRIBUTION, path="libs/typescript/runtime/schema.ts")))),
        "distribution-path",
    ),
    (
        "distribution-path-python",
        lambda t: _corpus(t, (_entry(), _distributed(msgspec.structs.replace(_PY_DISTRIBUTION, path="libs/python/runtime/schema.avsc")))),
        "distribution-path",
    ),
    (
        "distribution-export",
        lambda t: _after(_corpus(t, (_entry(), _distributed())), lambda r: _write(r, _TS_PACKAGE, '{"name":"@rasm/contracts"}\n')),
        "distribution-export",
    ),
    (
        "distribution-export-unpublished",
        lambda t: _after(
            _corpus(t, (_entry(), _distributed())),
            lambda r: _write(r, _TS_PACKAGE, _json(_TS_MANIFEST | {"exports": _TS_PUBLISHED, "publishConfig": {"access": "public"}})),
        ),
        "distribution-export",
    ),
    (
        "distribution-owned-twice",
        lambda t: _corpus(
            t,
            (
                _entry(),
                msgspec.structs.replace(
                    _PUB,
                    cases=(msgspec.structs.replace(_distributed().cases[0], id="one"), msgspec.structs.replace(_distributed().cases[0], id="two")),
                ),
            ),
        ),
        "distribution-owned-twice",
    ),
    (
        "anchor-dangling",
        lambda t: _corpus(
            t, (_entry(authority=DomainAuthority(producer=_message("dotnet:Demo/Page/one#GHOST", "DemoSchema.write", "proof"))), _vendored())
        ),
        "anchor-dangling",
    ),
    (
        "path-owned-twice",
        lambda t: _corpus(t, (_entry(cases=(_DEMO_CASE, msgspec.structs.replace(_DEMO_CASE, id="twin"))), _vendored())),
        "path-owned-twice",
    ),
    (
        "distribution-export-source",
        lambda t: _after(
            _corpus(t, (_entry(), _distributed())), lambda r: _write(r, _TS_PACKAGE, _json(_TS_MANIFEST | {"exports": {"./*": "./gen/*.ts"}}))
        ),
        "distribution-export",
    ),
    (
        "distribution-export-build",
        lambda t: _after(
            _corpus(t, (_entry(), _distributed())),
            lambda r: _write(r, _TS_PACKAGE, _json(_TS_MANIFEST | {"scripts": {"build": "tsc --build tsconfig.json", "prepack": "pnpm run build"}})),
        ),
        "distribution-export",
    ),
    (
        "distribution-export-directory",
        lambda t: _after(
            _corpus(t, (_entry(), _distributed())),
            lambda r: _write(r, _TS_PACKAGE, _json(_TS_MANIFEST | {"repository": {"type": "git", "url": _TS_REPOSITORY, "directory": "libs/demo"}})),
        ),
        "distribution-export",
    ),
    (
        "distribution-export-no-repository",
        lambda t: _after(
            _corpus(t, (_entry(), _distributed())),
            lambda r: _write(r, _TS_PACKAGE, _json({key: value for key, value in _TS_MANIFEST.items() if key != "repository"})),
        ),
        "distribution-export",
    ),
    ("root-stray", lambda t: _after(_corpus(t), lambda r: (r / _CORPUS / "rogue").mkdir()), "root-stray"),
    ("root-stray-file", lambda t: _after(_corpus(t), lambda r: _write(r / _CORPUS, "rogue.md", "x")), "root-stray"),
    ("vendor-stray", lambda t: _after(_corpus(t), lambda r: (r / _CORPUS / "vendor/rogue").mkdir()), "root-stray"),
    ("vendor-estate-seam", lambda t: _after(_corpus(t), lambda r: (r / _CORPUS / "vendor/DEMO_SEAM").mkdir()), "root-stray"),
    ("conformance-stray", lambda t: _after(_corpus(t), lambda r: (r / _CORPUS / "conformance/rogue").mkdir()), "root-stray"),
    ("conformance-publisher-seam", lambda t: _after(_corpus(t), lambda r: (r / _CORPUS / "conformance/PUB").mkdir()), "root-stray"),
    ("conformance-file", lambda t: _after(_corpus(t), lambda r: _write(r / _CORPUS, "conformance/stray.json", "{}")), "root-stray"),
    ("seam-stray", lambda t: _after(_corpus(t), lambda r: _write(r / _CORPUS, f"{_SEAM_DIR}/extra.bin", "x")), "seam-stray"),
    ("roster-stale", lambda t: _corpus(t, roster=False), "roster-stale"),
    ("roster-missing", lambda t: _after(_corpus(t), lambda r: _write(r, "libs/contracts/.api/python.md", "# [DEMO]\n")), "roster-missing"),
    ("roster-catalog", lambda t: _corpus(t, catalogs=()), "roster-catalog"),
    ("roster-row", lambda t: _corpus(t, template=_TEMPLATE.replace(f"out: {_OUTS[0]}", "out: libs/contracts/gen/rust")), "roster-row"),
    (
        "lock-present",
        lambda t: _corpus(t, config="version: v2\nmodules:\n  - path: proto\ndeps:\n  - buf.build/bufbuild/protovalidate\n"),
        "lock-present",
    ),
)


@pytest.mark.parametrize("label, build, rule", _DEFECTS, ids=[row[0] for row in _DEFECTS])
def test_audit_fires_each_defect_row_by_name(tmp_path: Path, label: str, build: Callable[[Path], Path], rule: str) -> None:
    """Every corpus audit row fires by its rule name on the fixture that breaks it."""
    _ = label
    root = build(tmp_path)
    rules, rows = _audit_rules(root, _image())
    assert rule in rules, f"expected {rule!r}, got:\n" + "\n".join(rows)


def test_audit_clean_corpus_folds_zero_findings(tmp_path: Path) -> None:
    """The clean fixture yields zero findings; rosters census both ways against the descriptor set."""
    rules, rows = _audit_rules(_corpus(tmp_path), _image())
    assert rules == frozenset(), "\n".join(rows)


def test_blocked_case_cannot_materialize_a_seam_directory(tmp_path: Path) -> None:
    """A registered future seam remains virtual until one case carries real proof assets."""
    root = _corpus(tmp_path)
    (root / _CORPUS / "conformance/DEMO_PROTO").mkdir()
    rules, rows = _audit_rules(root, _image())
    assert "seam-without-proof" in rules, "\n".join(rows)


def test_git_ignored_build_output_beside_the_estate_manifests_is_carved_from_the_stray_audit(tmp_path: Path) -> None:
    """Build output at the estate root is admitted exactly when git ignores it: unignored it strays, and a tree with no git carves nothing."""
    root = _corpus(tmp_path)
    (root / _CORPUS / "node_modules/.bin").mkdir(parents=True)
    _write(root / _CORPUS, "obj/Debug/built.dll", b"built")

    def strays() -> tuple[str, ...]:
        return tuple(row.removeprefix("root-stray: ").partition(":")[0] for row in _audit_rules(root, _image())[1] if row.startswith("root-stray"))

    assert strays() == ("node_modules", "obj")
    _git_ignored(root, "node_modules/", "obj/")
    assert strays() == ()
    _git_ignored(root, "node_modules/")
    assert strays() == ("obj",)
    (root / _CORPUS / "rogue").mkdir()
    assert strays() == ("obj", "rogue")


def test_backend_generation_facts_rebuild_exact_canonical_preimage_and_reject_order_drift(tmp_path: Path) -> None:
    """Backend parity proves semantic rows and the estate canonical writer preimage, never ProtoJSON or protobuf bytes."""
    artifact = DescriptorProto(
        name="Artifact",
        field=[
            FieldDescriptorProto(name="key", number=1, label=FieldDescriptorProto.Label.OPTIONAL, type=FieldDescriptorProto.Type.STRING),
            FieldDescriptorProto(
                name="role",
                number=2,
                label=FieldDescriptorProto.Label.OPTIONAL,
                type=FieldDescriptorProto.Type.ENUM,
                type_name=".rasm.contracts.parity.ArtifactRole",
            ),
            FieldDescriptorProto(name="content", number=3, label=FieldDescriptorProto.Label.OPTIONAL, type=FieldDescriptorProto.Type.BYTES),
            FieldDescriptorProto(
                name="providers",
                number=4,
                label=FieldDescriptorProto.Label.REPEATED,
                type=FieldDescriptorProto.Type.ENUM,
                type_name=".rasm.contracts.parity.Provider",
            ),
            FieldDescriptorProto(name="depends_on", number=5, label=FieldDescriptorProto.Label.REPEATED, type=FieldDescriptorProto.Type.STRING),
        ],
    )
    capability = DescriptorProto(
        name="Capability",
        field=[
            FieldDescriptorProto(name="key", number=1, label=FieldDescriptorProto.Label.OPTIONAL, type=FieldDescriptorProto.Type.STRING),
            FieldDescriptorProto(name="lane", number=2, label=FieldDescriptorProto.Label.OPTIONAL, type=FieldDescriptorProto.Type.STRING),
            FieldDescriptorProto(name="requirement", number=3, label=FieldDescriptorProto.Label.OPTIONAL, type=FieldDescriptorProto.Type.STRING),
            FieldDescriptorProto(
                name="requirement_value", number=4, label=FieldDescriptorProto.Label.OPTIONAL, type=FieldDescriptorProto.Type.STRING
            ),
            FieldDescriptorProto(
                name="failure_rank",
                number=5,
                label=FieldDescriptorProto.Label.OPTIONAL,
                type=FieldDescriptorProto.Type.ENUM,
                type_name=".rasm.contracts.parity.FailureRank",
            ),
            FieldDescriptorProto(
                name="restart_class",
                number=6,
                label=FieldDescriptorProto.Label.OPTIONAL,
                type=FieldDescriptorProto.Type.ENUM,
                type_name=".rasm.contracts.parity.RestartClass",
            ),
        ],
    )
    backend = DescriptorProto(
        name="Backend",
        field=[
            FieldDescriptorProto(name="contract", number=1, label=FieldDescriptorProto.Label.OPTIONAL, type=FieldDescriptorProto.Type.STRING),
            FieldDescriptorProto(
                name="artifacts",
                number=2,
                label=FieldDescriptorProto.Label.REPEATED,
                type=FieldDescriptorProto.Type.MESSAGE,
                type_name=".rasm.contracts.parity.Artifact",
            ),
            FieldDescriptorProto(
                name="capabilities",
                number=3,
                label=FieldDescriptorProto.Label.REPEATED,
                type=FieldDescriptorProto.Type.MESSAGE,
                type_name=".rasm.contracts.parity.Capability",
            ),
        ],
    )
    image = FileDescriptorSet(
        file=[
            FileDescriptorProto(
                name="rasm/contracts/parity/parity.proto",
                package="rasm.contracts.parity",
                enum_type=[
                    EnumDescriptorProto(
                        name="ArtifactRole",
                        value=[
                            EnumValueDescriptorProto(name="ARTIFACT_ROLE_UNSPECIFIED", number=0),
                            EnumValueDescriptorProto(name="ARTIFACT_ROLE_RELATIONAL_MODEL", number=1),
                        ],
                    ),
                    EnumDescriptorProto(
                        name="Provider",
                        value=[
                            EnumValueDescriptorProto(name="PROVIDER_UNSPECIFIED", number=0),
                            EnumValueDescriptorProto(name="PROVIDER_POSTGRESQL", number=1),
                        ],
                    ),
                    EnumDescriptorProto(
                        name="FailureRank",
                        value=[
                            EnumValueDescriptorProto(name="FAILURE_RANK_UNSPECIFIED", number=0),
                            EnumValueDescriptorProto(name="FAILURE_RANK_REQUIRED", number=1),
                        ],
                    ),
                    EnumDescriptorProto(
                        name="RestartClass",
                        value=[
                            EnumValueDescriptorProto(name="RESTART_CLASS_UNSPECIFIED", number=0),
                            EnumValueDescriptorProto(name="RESTART_CLASS_SESSION", number=1),
                            EnumValueDescriptorProto(name="RESTART_CLASS_RELOAD", number=2),
                        ],
                    ),
                ],
                message_type=[artifact, capability, backend],
            )
        ]
    ).to_binary()
    image_path = _write(tmp_path, "parity.binpb", image)
    raw = (
        b'{"contract":"v1","artifacts":[{"key":"a","role":1,"content":"eA==","providers":[1],"dependsOn":["z"]}],'
        b'"capabilities":[{"key":"c","lane":"l","requirement":"r","requirementValue":"","failureRank":1,"restartClass":2}]}'
    )

    def framed(value: bytes) -> bytes:
        return pack("<i", len(value)) + value

    preimage = b"".join((
        framed(b"v1"),
        pack("<i", 1),
        framed(b"a"),
        pack("<i", 1),
        framed(b"x"),
        pack("<i", 1),
        pack("<i", 1),
        pack("<i", 1),
        framed(b"z"),
        pack("<i", 1),
        framed(b"c"),
        framed(b"l"),
        framed(b"r"),
        framed(b""),
        pack("<i", 1),
        pack("<i", 2),
    ))
    facts = assert_ok(contracts_rail._backend_facts(image_path, "rasm.contracts.parity.Backend", raw))
    assert facts == BackendGenerationFacts(
        contract="v1",
        artifact_keys=("a",),
        capability_keys=("c",),
        preimage_bytes=len(preimage),
        preimage_xxh128=xxhash.xxh128(preimage, seed=0).hexdigest(),
        preimage_hex=preimage.hex(),
    )
    unordered = raw.replace(b'"artifacts":[', b'"artifacts":[{"key":"b","role":1,"content":"","providers":[],"dependsOn":[]},')
    refused = contracts_rail._backend_facts(image_path, "rasm.contracts.parity.Backend", unordered)
    assert refused.is_error() and "artifact keys must be strictly ascending and unique" in refused.error
    invalid_role = contracts_rail._backend_facts(image_path, "rasm.contracts.parity.Backend", raw.replace(b'"role":1', b'"role":99'))
    assert invalid_role.is_error() and "Invalid enum value 99" in invalid_role.error
    self_dependency = contracts_rail._backend_facts(
        image_path, "rasm.contracts.parity.Backend", raw.replace(b'"dependsOn":["z"]', b'"dependsOn":["a"]')
    )
    assert self_dependency.is_error() and "cannot depend on itself" in self_dependency.error
    nonprintable = contracts_rail._backend_facts(
        image_path, "rasm.contracts.parity.Backend", raw.replace(b'"contract":"v1"', b'"contract":"\\u0001"')
    )
    assert nonprintable.is_error() and "contract must contain printable ASCII only" in nonprintable.error


def test_audit_without_image_reports_the_missing_descriptor_set(tmp_path: Path) -> None:
    """Without a built descriptor set the audit names the gap once and skips message resolution and roster census rather than faking them."""
    root = _corpus(tmp_path)
    (root / "image.binpb").unlink()
    rules, _ = _audit_rules(root)
    assert rules == frozenset({"image-missing"})


def test_manifest_decode_partition(tmp_path: Path) -> None:
    """A missing or undecodable manifest is a FAILED defect naming the field; an unreadable one is a FAULTED corpus lane."""
    root = _corpus(tmp_path)
    corpus = root / _CORPUS
    assert assert_ok(load_manifest(corpus)).entries[0].id == "DEMO_SEAM"
    (corpus / "manifest.json").unlink()
    assert assert_error_status(load_manifest(corpus), RailStatus.FAILED).message.endswith("missing")
    (corpus / "manifest.json").mkdir()
    assert_error_status(load_manifest(corpus), RailStatus.FAULTED)
    template = assert_ok(read_template(root))
    thunk = contracts_rail._corpus(root, root / "image.binpb", template, contracts_rail._NO_PROJECTIONS, _roster_files(root, template))
    done = thunk(Check(tool=_ROWS["corpus-gate", Mode.CHECK]))
    assert done.status is RailStatus.FAULTED and b"manifest.json" in done.stderr


def test_derived_schema_is_the_one_authority(tmp_path: Path) -> None:
    """The derived schema is a valid 2020-12 document that admits the fixture manifest and refuses a vocabulary miss; byte drift is a finding."""
    schema = msgspec.json.decode(derived_schema())
    jsonschema.Draft202012Validator.check_schema(schema)
    validator = jsonschema.Draft202012Validator(schema)
    root = _corpus(tmp_path)
    document = msgspec.json.decode((root / _CORPUS / "manifest.json").read_bytes())
    assert list(validator.iter_errors(document)) == []
    assert list(validator.iter_errors({"entries": [msgspec.to_builtins(_entry()) | {"seam": "legacy"}]}))
    assert schema["$schema"] == "https://json-schema.org/draft/2020-12/schema"
    assert (REPO_ROOT / _CORPUS / "manifest.schema.json").read_bytes() == derived_schema()
    (root / _CORPUS / "manifest.schema.json").write_bytes(derived_schema() + b"\n")
    assert "schema-stale" in _audit_rules(root, _image())[0]


def test_generate_derives_manifest_schema_and_invalid_manifest_blocks_every_write(assay_root: AssayHarness) -> None:
    """Schema drift is one emittable derivation, while an undecodable manifest refuses generation before any public write."""
    root = _corpus(assay_root.root, schema=b"{}\n")
    calls: list[tuple[str, ...]] = []
    landed = assert_ok(_run(assay_root, _fan(root, calls=calls), "generate"))
    assert landed.status is RailStatus.OK
    assert (root / _CORPUS / "manifest.schema.json").read_bytes() == derived_schema()
    assert "schema: libs/contracts/manifest.schema.json written" in landed.notes
    assert any(argv[:2] == ("buf", "generate") and "-o" in argv for argv in calls)

    (root / _CORPUS / "manifest.json").write_bytes(b'{"entries": [{"id": "torn"}]}')
    broken_calls: list[tuple[str, ...]] = []
    blocked = assert_ok(_run(assay_root, _fan(root, calls=broken_calls), "generate"))
    assert blocked.status is RailStatus.FAILED
    assert _rules(blocked) >= {"manifest-undecodable"}
    assert dict(_detail(blocked).lanes)["buf-generate"] == dict(_detail(blocked).lanes)["corpus-emit"] == "skip"
    assert not any(argv[:2] == ("buf", "generate") for argv in broken_calls)


def test_verified_json_specimens_validate_against_the_seam_schema(tmp_path: Path) -> None:
    """A verified JSON specimen validates against its self-contained case schema."""
    root = _corpus(tmp_path)
    assert "asset-invalid" not in _audit_rules(root, _image())[0]
    (root / _CORPUS / _SEAM_DIR / "demo.json").write_bytes(b'{"key": "alpha", "extra": 1}\n')
    rules, rows = _audit_rules(root, _image())
    assert "asset-invalid" in rules and "asset-bytes" in rules, rows
    assert any("additionalProperties" in row or "extra" in row for row in rows if row.startswith("asset-invalid")), rows


def test_publisher_asset_distributions_generate_package_native_projections_and_scratch_proves_freshness(assay_root: AssayHarness) -> None:
    """One publisher asset emits a readonly TS value and byte-exact Python resource; scratch restores both after the clean sweep."""
    root = _corpus(assay_root.root, (_entry(), _BINDING, _distributed((_DISTRIBUTION, _PY_DISTRIBUTION))))
    target = root / _DISTRIBUTION.path
    resource = root / _PY_DISTRIBUTION.path
    assert _audit_rules(root, _image())[0] == frozenset({"distribution-missing"})
    landed = assert_ok(_run(assay_root, _fan(root), "generate"))
    payload = target.read_bytes()
    assert landed.status is RailStatus.OK
    assert f"distribution: {_DISTRIBUTION.path} written" in landed.notes
    assert f"distribution: {_PY_DISTRIBUTION.path} written" in landed.notes
    assert _PUB_BYTES in payload and b"const PublisherAvro = (" in payload and b"export { PublisherAvro };" in payload
    assert resource.read_bytes() == _PUB_BYTES
    python_catalog = (root / contracts_rail._ROSTERS["python"].catalog).read_text(encoding="utf-8")
    assert "[ASSET_SCOPE]" in python_catalog and f"`{_PY_DISTRIBUTION.path}`" in python_catalog
    clean = assert_ok(_run(assay_root, _fan(root)))
    assert _detail(clean).stale == () and dict(_detail(clean).lanes)["freshness-gate"] == "ok"
    resource.write_bytes(_PUB_BYTES + b" ")
    stale = assert_ok(_run(assay_root, _fan(root)))
    assert _rules(stale) == {"distribution-stale"}
    assert _detail(stale).stale == (("changed", _PY_DISTRIBUTION.path),) and dict(_detail(stale).lanes)["freshness-gate"] == "failed"


# --- [PROJECTION]


def test_proto_derivation_projects_check_byte_checks_and_generate_lands(assay_root: AssayHarness, projector: Path) -> None:
    """One projection lane per proto-derived seam carries the estate module, scratch dir, and fqn; check fails stale bytes, generate lands them."""
    root = _corpus(
        assay_root.root, (_entry(definition=_PROTO_DEFINITION), _BINDING, _vendored()), definition=_THING_PATH, seam_schema=_bundle(_THING) + b"\n"
    )
    calls: list[tuple[str, ...]] = []
    stale = assert_ok(_run(assay_root, _fan(root, calls=calls)))
    detail = _detail(stale)
    scratch = Path(detail.scratch)
    assert stale.status is RailStatus.FAILED and _rules(stale) == {"schema-stale"}
    assert calls[5] == ("buf", "generate", "libs/contracts", "--template", JSONSCHEMA_TEMPLATE, "-o", str(scratch / "schema"), "--type", _THING)
    assert dict(detail.lanes)[f"buf-jsonschema:{_THING}"] == "ok" and dict(detail.plugins)[JSONSCHEMA_PLUGIN] == str(projector)
    assert [name for name, _ in detail.lanes][4:] == [
        "buf-generate",
        f"buf-jsonschema:{_THING}",
        "buf-roster:typescript-01",
        "buf-roster:python-02",
        "buf-roster:python-03",
        "buf-roster:dotnet-04",
        "buf-roster:dotnet-05",
        "corpus-gate",
        "freshness-gate",
    ]
    landed = assert_ok(_run(assay_root, _fan(root), "generate"))
    assert landed.status is RailStatus.OK and f"schema: {_CORPUS}/{_THING_PATH} written" in landed.notes
    assert (root / _CORPUS / _THING_PATH).read_bytes() == _bundle(_THING)
    assert [name for name, _ in _detail(landed).lanes] == [
        "plugin-probe",
        "buf-build",
        f"buf-jsonschema:{_THING}",
        "buf-roster:typescript-01",
        "buf-roster:python-02",
        "buf-roster:python-03",
        "buf-roster:dotnet-04",
        "buf-roster:dotnet-05",
        "corpus-gate",
        "buf-generate",
        "corpus-emit",
    ]
    clean = assert_ok(_run(assay_root, _fan(root)))
    assert clean.status is RailStatus.OK and clean.results == ()
    again = assert_ok(_run(assay_root, _fan(root), "generate"))
    assert f"schema: {_CORPUS}/{_THING_PATH} unchanged" in again.notes and (root / _CORPUS / _THING_PATH).read_bytes() == _bundle(_THING)


def test_proto_binary_framing_earns_no_schema_and_no_lane(assay_root: AssayHarness, projector: Path) -> None:
    """A schema definition framed proto-binary is refused as `schema-framing` and never reaches buf."""
    binary = msgspec.structs.replace(_PROTO_DEFINITION, framing="proto-binary")
    root = _corpus(assay_root.root, (_entry(definition=binary), _BINDING, _vendored()), definition=_THING_PATH, seam_schema=_bundle(_THING))
    report = assert_ok(_run(assay_root, _fan(root)))
    assert report.status is RailStatus.FAILED and _rules(report) == {"schema-framing"}
    assert not any(name.startswith("buf-jsonschema") for name, _ in _detail(report).lanes)
    _ = projector


def test_projector_miss_degrades_the_derivation_alone(assay_root: AssayHarness, projector: Path) -> None:
    """Without the projector the projection lane is unsupported and atomic preflight refuses every public write."""
    projector.unlink()
    root = _corpus(
        assay_root.root, (_entry(definition=_PROTO_DEFINITION), _BINDING, _vendored()), definition=_THING_PATH, seam_schema=_bundle(_THING)
    )
    report = assert_ok(_run(assay_root, _fan(root)))
    detail = _detail(report)
    assert report.status is RailStatus.FAILED and _rules(report) == {"schema-derivation"}
    assert dict(detail.lanes)[f"buf-jsonschema:{_THING}"] == "unsupported" and dict(detail.lanes)["freshness-gate"] == "ok"
    assert dict(detail.lanes)["plugin-probe"] == "ok" and not dict(detail.plugins)[JSONSCHEMA_PLUGIN]
    assert any(contracts_rail._MACHINE_HINT in row.text for row in report.results)
    landed = assert_ok(_run(assay_root, _fan(root), "generate"))
    assert landed.status is RailStatus.FAILED and _rules(landed) == {"schema-derivation"}
    assert not any(note.startswith(("roster:", "schema:")) for note in landed.notes)
    assert dict(_detail(landed).lanes)["buf-generate"] == "skip"


# --- [LIVE_PROJECTION]


def _live(assay_root: AssayHarness, module: str, fqn: str) -> tuple[Completed, Path]:
    if shutil.which(JSONSCHEMA_PLUGIN) is None:
        pytest.skip(f"{JSONSCHEMA_PLUGIN} is not on PATH; {contracts_rail._MACHINE_HINT}")
    scratch = assay_root.root / "scratch"
    (scratch / "schema").mkdir(parents=True)
    chk = msgspec.structs.replace(contracts_rail._derive_check(module, scratch, fqn), cwd=REPO_ROOT)
    done = assert_ok(EngineExecutor().run(chk, settings=assay_root.settings, scope=assay_root.scope(Claim.CONTRACTS), routed=contracts_rail._ROUTED))
    assert done.status in {RailStatus.OK, RailStatus.EMPTY}, done.stderr
    return done, scratch / "schema" / f"{fqn}.jsonschema.strict.bundle.json"


def test_live_projection_folds_validate_rules_into_the_bundle(assay_root: AssayHarness) -> None:
    """The real plugin projects a fixture module's enum roster and `gte` rule as `enum` and `minimum` under the file-name `$id`."""
    module = assay_root.root / "fixture"
    _write(module, "buf.yaml", _FIXTURE_CONFIG)
    _write(module, "buf.lock", (REPO_ROOT / _CORPUS / "buf.lock").read_bytes())
    _write(module, "proto/fx/fx.proto", _FIXTURE_PROTO)
    _, bundle = _live(assay_root, str(module), "fx.Thing")
    document = msgspec.json.decode(bundle.read_bytes())
    thing = document["$defs"]["fx.Thing.jsonschema.strict.json"]
    assert document["$id"] == bundle.name and document["$ref"] == "#/$defs/fx.Thing.jsonschema.strict.json"
    assert thing["properties"]["count"]["minimum"] == 1 and thing["properties"]["kind"]["enum"] == ["KIND_UNSPECIFIED", "KIND_ALPHA"]
    assert thing["additionalProperties"] is False and thing["required"] == ["kind", "count"]


def test_live_projection_of_the_declaration_root(assay_root: AssayHarness) -> None:
    """The corpus root message projects to a 2020-12 strict bundle whose `$id` is its file name and whose root refuses unknown properties."""
    _, bundle = _live(assay_root, _CORPUS, _DECLARATION)
    document = msgspec.json.decode(bundle.read_bytes())
    root = document["$ref"].rpartition("/")[2]
    assert (
        document["$schema"] == "https://json-schema.org/draft/2020-12/schema"
        and document["$id"] == bundle.name == f"{_DECLARATION}.jsonschema.strict.bundle.json"
    )
    assert root == f"{_DECLARATION}.jsonschema.strict.json" and document["$defs"][root]["additionalProperties"] is False
    assert "google.type.Date.jsonschema.strict.json" in document["$defs"] and bundle.read_bytes().endswith(b"}\n")


# --- [VERBS_AND_LEASES]


def test_generate_stages_buf_and_commits_through_the_transaction_writer_under_the_contracts_lease(assay_root: AssayHarness) -> None:
    """Generate keeps Buf in scratch, commits through one writer, and shares the contracts lease with check and publish."""
    root = _corpus(assay_root.root)
    calls: list[tuple[str, ...]] = []
    report = assert_ok(_run(assay_root, _fan(root, calls=calls), "generate"))
    scratch = Path(_detail(report).scratch)
    roster = ("buf", "build", "libs/contracts", "-o")
    assert calls == [
        ("plugin-probe", "resolve"),
        (*roster, str(scratch / "image.binpb"), "--as-file-descriptor-set"),
        (*roster, str(scratch / "roster/typescript-01.binpb"), "--as-file-descriptor-set", "--type", _THING, "--type", _METHOD),
        (*roster, str(scratch / "roster/python-02.binpb"), "--as-file-descriptor-set", "--type", _THING, "--type", _METHOD),
        (*roster, str(scratch / "roster/python-03.binpb"), "--as-file-descriptor-set", "--type", _METHOD),
        (*roster, str(scratch / "roster/dotnet-04.binpb"), "--as-file-descriptor-set", "--type", _THING),
        (*roster, str(scratch / "roster/dotnet-05.binpb"), "--as-file-descriptor-set", "--type", _METHOD),
        ("corpus-gate", "check"),
        ("buf", "generate", "libs/contracts", "--template", "libs/contracts/buf.gen.yaml", "-o", str(scratch / "gen")),
        ("corpus-emit", "write"),
    ]
    assert not _ROWS["buf-generate", Mode.STAGE].mode.writes and _ROWS["corpus-emit", Mode.WRITE].mode.writes and report.status is RailStatus.OK
    assert [name for name, _ in _detail(report).lanes] == [
        "plugin-probe",
        "buf-build",
        "buf-roster:typescript-01",
        "buf-roster:python-02",
        "buf-roster:python-03",
        "buf-roster:dotnet-04",
        "buf-roster:dotnet-05",
        "corpus-gate",
        "buf-generate",
        "corpus-emit",
    ]
    with exclusive_lease("contracts", "holder", settings=assay_root.settings) as held:
        assert_ok(held)
        assert_error_status(_run(assay_root, _fan(root), "generate"), RailStatus.BUSY)
        assert_error_status(_run(assay_root, _fan(root)), RailStatus.BUSY)
        assert_error_status(_run(assay_root, _fan(root), "publish"), RailStatus.BUSY)


def test_generate_emits_the_roster_block_between_markers_and_check_byte_checks_it(assay_root: AssayHarness) -> None:
    """Stale blocks rewrite, fresh ones stay, a catalog without markers is a finding and never written; check then reads the block clean."""
    root = _corpus(assay_root.root, roster=False)
    catalog = root / "libs/contracts/.api/python.md"
    bare = root / "libs/contracts/.api/dotnet.md"
    bare.write_text("# [DEMO]\n\nno markers here\n", encoding="utf-8")
    before = catalog.read_text(encoding="utf-8")
    assert "roster-stale" in _audit_rules(root, _image())[0]
    first = assert_ok(_run(assay_root, _fan(root), "generate"))
    assert first.status is RailStatus.FAILED and [row.id for row in first.results] == ["corpus:roster-missing"]
    assert bare.read_text(encoding="utf-8") == "# [DEMO]\n\nno markers here\n"
    assert catalog.read_text(encoding="utf-8") == before
    assert not any(note.startswith("roster:") for note in first.notes)
    assert dict(_detail(first).lanes)["buf-generate"] == "skip"
    bare.write_text(_CATALOG, encoding="utf-8")
    second = assert_ok(_run(assay_root, _fan(root), "generate"))
    rosters = [note for note in second.notes if note.startswith("roster:")]
    text = catalog.read_text(encoding="utf-8")
    span = contracts_rail._roster_span(text)
    assert second.status is RailStatus.OK and all(note.endswith("written") for note in rosters) and len(rosters) == 3
    assert span is not None and "[ROSTER_SCOPE]: `demo`" in text[span[0] : span[1]]
    assert f"schema: {_CORPUS}/{_SEAM_PATH} unchanged" in second.notes
    assert "roster-stale" not in _audit_rules(root, _image())[0] and "roster-missing" not in _audit_rules(root, _image())[0]


def test_exec_offload_is_rejected_as_host_bound(assay_root: AssayHarness) -> None:
    """Under a remote exec target the contracts claim refuses before any argv composes."""
    root = _corpus(assay_root.root)
    remote = assay_root.remote("ssh://x@127.0.0.1:2222")
    fault = assert_error_status(check(remote, assay_root.scope(Claim.CONTRACTS), ContractsParams(), EngineExecutor()), RailStatus.UNSUPPORTED)
    assert "host-bound" in fault.message
    _ = root


def test_scratch_requires_the_file_artifact_backend(assay_root: AssayHarness) -> None:
    """A non-file artifact backend cannot host scratch regeneration, so check refuses UNSUPPORTED before the lease."""
    root = _corpus(assay_root.root)
    settings = assay_root.settings.model_copy(update={"artifact_backend": ArtifactBackend(protocol="memory", root="mem-store/x")})
    fault = assert_error_status(
        check(settings, assay_root.scope(Claim.CONTRACTS), ContractsParams(), SeamExecutor(fan_fn=_fan(root))), RailStatus.UNSUPPORTED
    )
    assert "file artifact backend" in fault.message


def test_template_faults_name_the_parse_step(assay_root: AssayHarness) -> None:
    """A missing or unmodelled buf.gen.yaml is a parse fault for both verbs."""
    root = _corpus(assay_root.root, template="version: v2\nplugins:\n  - local: x\n    out: y\n    ghost: 1\n")
    assert assert_error_status(_run(assay_root, _fan(root)), RailStatus.FAULTED).message.startswith("parse: template")
    (root / _TEMPLATE_PATH).unlink()
    assert assert_error_status(_run(assay_root, _fan(root), "generate"), RailStatus.FAULTED).message.startswith("parse: template")


@pytest.mark.parametrize(
    "template",
    [
        _TEMPLATE.replace("buf.build/protocolbuffers/csharp:v36.0\n    revision: 1", "buf.build/protocolbuffers/csharp\n    revision: 1"),
        _TEMPLATE.replace("buf.build/protocolbuffers/csharp:v36.0\n    revision: 1", "buf.build/protocolbuffers/csharp:v36.0"),
        _TEMPLATE.replace("  - local: node_modules/.bin/protoc-gen-es", "  - local: node_modules/.bin/protoc-gen-es\n    revision: 1"),
    ],
    ids=("remote-version", "remote-revision", "local-revision"),
)
def test_template_requires_exact_remote_plugin_revisions(tmp_path: Path, template: str) -> None:
    """Remote plugins pin both upstream version and positive BSR revision; local plugins cannot carry that registry coordinate."""
    _write(tmp_path, _TEMPLATE_PATH, template)
    fault = assert_error_status(read_template(tmp_path), RailStatus.FAULTED)
    assert fault.message.startswith("parse: template:")


@pytest.mark.parametrize(
    "config",
    [
        "version: v2\nmodules:\n  - path: proto\n",
        (f"version: v2\nmodules:\n  - path: proto\n    name: {_MODULE}\n  - path: vendor/pub/proto\n    name: buf.build/pub/contracts\n"),
        f"version: v2\nmodules:\n  - path: proto\n    name: {_MODULE}\n  - path: other/proto\n",
    ],
    ids=("unnamed-estate", "named-publisher", "foreign-local-module"),
)
def test_config_admits_one_named_estate_and_unnamed_publishers(assay_root: AssayHarness, config: str) -> None:
    """The config boundary refuses an unnamed estate, a named publisher mirror, and any unrelated local module before spawning Buf."""
    root = _corpus(assay_root.root, config=config)
    fault = assert_error_status(_run(assay_root, _fan(root)), RailStatus.FAULTED)
    assert fault.message.startswith("parse: config:")


# --- [PARAMS]


def test_params_reject_positionals_and_flags(cli: VerbRunner) -> None:
    """The verbs take no positional token and no flag: a path is surplus at bind, an option is a parse fault at the CLI."""
    surplus = ContractsParams(paths=("x",)).bound("check")
    assert isinstance(surplus, Fault) and surplus.status is RailStatus.FAULTED
    assert ContractsParams().bound("generate") == ContractsParams()
    assert ContractsParams().bound("publish") == ContractsParams()
    assert ContractsParams.SLOTS == {"": ""}
    res = cli("contracts", "check", "--strict")
    assert res.exit_code == _FAULTED_EXIT and res.envelope.claim is Claim.CONTRACTS and res.envelope.status is RailStatus.FAULTED
    assert res.envelope.error_context is not None and res.envelope.error_context.failing_step == "parse"


def test_catalog_rows_pin_lane_contract() -> None:
    """Every Buf capability is a catalog-owned executable row, including immutable resolution and guarded publication."""
    rows = select(Claim.CONTRACTS)
    assert {t.language for t in rows} == {Language.PROTO} and all(t.input.value == "owned" for t in rows)
    assert {t.name for t in rows if t.runner is Runner.PNPM} == {
        "buf-baseline",
        "buf-module",
        "buf-build",
        "buf-lint",
        "buf-format",
        "buf-generate",
        "buf-jsonschema",
        "buf-push",
    }
    assert {t.name for t in rows if t.runner is Runner.INPROC} == {"plugin-probe", "corpus-gate", "freshness-gate", "corpus-emit"}
    assert _ROWS["buf-jsonschema", Mode.STAGE].command[1:4] == ("generate", "{input}", "--template") and JSONSCHEMA_PLUGIN in JSONSCHEMA_TEMPLATE
    assert {t.name for t in rows if t.defect_exit == BUF_DEFECT_EXIT} == {"buf-lint", "buf-format"}
    assert {t.name for t in rows if t.parser is Parser.BUF} == {"buf-lint"}
    assert _ROWS["buf-baseline", Mode.QUERY].command == ("buf", "registry", "module", "commit", "resolve", "{input}", "--format", "json")
    assert _ROWS["buf-push", Mode.PUBLISH].command == ("buf", "push", "libs/contracts", "--exclude-unnamed", "{flags*}", "--label", "{target}")
    assert all(t.command[0] == "buf" for t in rows if t.runner is Runner.PNPM)
    assert all(dict(t.env).get("BUF_CACHE_DIR") == ".cache/buf" for t in rows if t.runner is Runner.PNPM)
