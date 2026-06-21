# [PY_COMPUTE_CODEGEN]

The typed-stub projector that decodes the C# graduation-evidence bundle and synthesizes `msgspec.Struct` stubs through the stdlib `ast` builder. `StubCodegen` consumes the offline-seam bundle the C# graduation evidence seals, decodes it once polymorphically over the wire format the seam chose, and folds each owner descriptor into Python type-stub source — so downstream compute composes against the C# owner row by import rather than by re-typing it. The descriptor is a wire-decoded discriminated union: `FieldDescriptor` is the `tag_field`-discriminated `Struct` family whose scalar case carries a `FieldScalar` literal and whose composite cases (`array`/`nested`/`mapping`/`optional`/`union`) carry their typed sub-descriptors, so the shape kind lives in the case the discriminant selects rather than in parallel `element`/`nested` optionals racing the kind; the decoder targets the closed `FieldNode` leaf union, never the open base, so `kind` selects exactly one case. The descriptor descent is ONE recursion-schema `_fold(node, alg)` parameterized by a `FieldAlgebra[T]` output interpreter — the catamorphism is written once and run twice, with `_NODE` producing `ast.expr` annotation nodes and `_HINT` producing the runtime `msgspec.defstruct` field types, so the stub source and the schema can never diverge on shape, the eight wire primitives live in the one `_SCALAR` table both interpreters read, and there is no second per-kind annotation method or parallel scalar table. No annotation is rendered to a string and re-parsed through `ast.parse`: every node is synthesized, the scalar leaf rendering to its dotted module+qualname `ast` node so `decimal.Decimal` and `ContentKey` resolve without a hand-keyed `ast` row. `emit` is polymorphic over the inbound wire format (`json`/`msgpack`, one `msgspec` codec) AND the outbound target (`stub` source, `schema` `$defs`, or `both`), validates its `RawBundle` input through a shared `beartype.vale.Is` refinement, runs the decode-fold-render under one OTel span weaving `msgspec`, `beartype`, OTel, and the runtime `Signals` receipt fold into a single rail, and streams its `emitted` receipt through the canonical `Signals.emit` polymorphic fold like every sibling evidence owner — never an inline `structlog` line. The bundle is consumed at the boundary and never re-minted: this owner reads the evidence shape the C# graduation evidence already carries, emits type stubs and JSON Schema only, never runtime behavior, and imports nothing from a C# interior.

## [01]-[INDEX]

- [01]-[STUB_CODEGEN]: the wire-decoded `FieldNode` union, the one `_fold` recursion schema run under two `FieldAlgebra` interpreters, the format-and-target-polymorphic `emit` rail, and the `GeneratedModule` receipt streamed through `Signals.emit`.

## [02]-[STUB_CODEGEN]

- Owner: `StubCodegen` — the one projector consuming the C# graduation-evidence bundle and emitting typed Python stubs plus their JSON Schema; it reads `EvidenceBundle`, `OwnerDescriptor`, and the `FieldNode` leaf union (the shape the C# graduation evidence carries) and folds each descriptor into an `ast.ClassDef` through the stdlib `ast` builder and into a `$defs` component through `msgspec.json.schema_components`. `FieldScalar` is the scalar-kind `StrEnum` whose runtime type lives in the `_SCALAR` data table both interpreters read; the composite kinds are `FieldDescriptor` union cases, not enum members, because they carry sub-shape. The bundle decodes once and is re-minted nowhere.
- Cases: `FieldDescriptor` is the `Struct` family tagged on `kind` — `scalar(FieldScalar)`, `array(element)`, `nested(ref)`, `mapping(key, value)`, `optional(element)`, and `union(members)` — so `array`/`optional` carry the one inner `FieldNode`, `mapping` carries the key/value pair, `union` carries the member tuple, and `nested` carries the forward owner name; the former parallel `element: FieldDescriptor | None` and `nested: str | None` optionals collapse into the case payload the `kind` selects, and the `FieldNode` alias unions the six leaves so `msgspec.json.Decoder(type=...)` discriminates over the closed leaf set rather than the open base. `FieldScalar` folds the eight wire primitives the C# seam emits — `i32`, `i64`, `f64`, `bool`, `string`, `key`, `bytes`, `decimal` — each mapping through the one `_SCALAR` table to its runtime type (`int`, `float`, `bool`, `str`, `ContentKey`, `bytes`, `decimal.Decimal`) that both the `_NODE` annotation interpreter and the `_HINT` type interpreter read. A new scalar primitive is one `FieldScalar` member and one `_SCALAR` row; a new composite kind is one `FieldDescriptor` case plus one `_fold` match arm plus one constructor field on each `FieldAlgebra` interpreter; never a parallel emitter.
- Fold: `_fold(node, alg)` is the one total recursion schema over the `FieldNode` union — a catamorphism parameterized by a `FieldAlgebra[T]` whose six `Callable` fields name the per-case constructors — closed by `assert_never` so an unmodelled kind is a compile-surfaced gap. It is written once and run by two interpreters over the identical descent: `_NODE: FieldAlgebra[ast.expr]` synthesizes `ast` annotation nodes directly (`array` -> `tuple[..., ...]` via `ast.Subscript`, `optional` -> `inner | None` via `ast.BinOp(ast.BitOr())`, `mapping` -> `dict[k, v]`, `union` left-folds members through `ast.BitOr`, `nested` -> the bare forward-ref `ast.Name`, `scalar` -> the dotted module+qualname node `_qual` builds), and `_HINT: FieldAlgebra[object]` produces the runtime `msgspec.defstruct` field types over the same arms. Because the stub source and the schema descend through one fold, they can never disagree on shape, and there is no second per-kind annotation method or parallel `_SCALAR_HINT` table. No annotation is rendered to a string and re-parsed through `ast.parse` — every node is synthesized, so a malformed sub-shape is impossible by construction.
- Entry: `StubCodegen.emit(raw, *, wire="json", target="both")` returns `RuntimeRail[GeneratedModule]` under one OTel span — it validates `raw` through the shared `RawBundle` = `Annotated[bytes, beartype.vale.Is[...]]` refinement, selects the `msgspec` decoder for the inbound `WireFormat`, decodes the bundle once, runs `_fold` under `_NODE` to render `ast.ClassDef` nodes via `ast.unparse` when the `EmitTarget` requests `stub`, and runs `_fold` under `_HINT` to reconstruct the owner structs that `msgspec.json.schema_components` projects to a `$defs` schema when it requests `schema`. `emit` is `@beartype`-fenced under the shared `_CONF` whose `violation_type` lifts a contract breach straight onto the fault rail; the span carries the resolved `owner_count`, `field_count`, `schema_version`, and `bundle_key.hex` projected through `msgspec.to_builtins(..., str_keys=True)`, and the `Ok` arm streams the `emitted` receipt through `Signals.emit` while the `Error` arm sets `StatusCode.ERROR` and streams the `rejected` `BoundaryFault` receipt through the same fold — never a hand-built `structlog.get_logger().error` line. `GeneratedModule` carries the rendered source, the optional schema `$defs`, the owner count, the field count, the C# `schema_version` (no longer decoded-and-discarded), and the bundle `ContentKey`, and its `contribute` yields the `(Receipt, ...)` stream the `ReceiptContributor` port expects.
- Seam: `EvidenceBundle` is the decoded projection of the C# graduation-evidence bundle — the owners, the C# `schema_version`, and the `ContentKey` the C# side sealed it under. The C# seam chooses the wire format; `WireFormat` discriminates `json` versus `msgpack` so the one `msgspec` codec decodes either without a parallel reader, the format the seam sealed under being a parameter on `emit`, not a second entrypoint. This is the only place compute reads the C# evidence shape; it imports nothing from a C# interior and re-mints nothing, so the seam crosses once, offline, as bytes. Distinct from the `transport/serve#SERVE` `grpcio-tools`/`protoc` stub path — that compiles the C# `.proto` RPC descriptors into `_pb2` wire messages on the gated companion band; this owner projects the offline evidence bundle into domain `msgspec.Struct` stubs on the cp315 core, a different seam over a different artifact.
- Nested resolution: the `_HINT` `nested` arm yields the forward owner-name string rather than an eagerly-bound type, and `msgspec.json.schema_components(..., schema_hook=_ref_hook)` resolves that string to a `#/$defs/<name>` `$ref` through the documented `schema_hook` so a nested owner is one cross-reference rather than a `defstruct` decode failure on an unresolved forward type. The `_NODE` `nested` arm renders the same forward name as a bare `ast.Name`, so the stub source and the schema agree the field references a sibling owner.
- Target fold: `EmitTarget` discriminates `stub`, `schema`, and `both`, parameterizing the OUTPUT shape the one rail produces — `stub` renders only the `ast` module source, `schema` projects only the `msgspec.json.schema_components` `$defs` over the reconstructed owner structs, `both` carries each. Both projections descend the same `_fold` over the same decoded `OwnerDescriptor` tuple, so the stub source and the schema can never disagree on the field set; a consumer wanting only the wire-contract schema or only the importable stub selects a target rather than calling a second generator.
- Packages: stdlib `ast` (`ClassDef`/`AnnAssign`/`Subscript`/`Name`/`Attribute`/`Tuple`/`BinOp`/`BitOr`/`keyword`/`Constant`/`Module`/`Pass`/`fix_missing_locations`/`unparse`, stub synthesis), `msgspec` (`Struct` with `tag_field`, `field(default_factory=...)`, `json.Decoder`/`msgpack.Decoder`, `json.schema_components` with `schema_hook`, `Meta(extra_json_schema=...)`, `defstruct`, `to_builtins`), `beartype` (`@beartype`, `BeartypeConf(violation_type=...)`, `beartype.vale.Is` refinement), `opentelemetry-api` (`trace.get_tracer`/`Tracer.start_as_current_span`/`Span.set_attributes`/`Span.set_status`/`StatusCode` for the projection span), `expression` (`Map.empty`, `Ok`/`Error`), runtime (`RuntimeRail`/`boundary`, `ContentKey`, `Receipt`/`ReceiptContributor`/`Redaction`/`Signals`).
- Growth: a new wire primitive is one `FieldScalar` member plus one `_SCALAR` row both interpreters absorb; a new composite shape is one `FieldDescriptor` case plus one `FieldNode` union member plus one `_fold` match arm plus one `FieldAlgebra` constructor field; a new inbound wire format is one `WireFormat` member plus one decoder row; a new output artifact is one `EmitTarget` member plus one fold arm; zero new surface, no external-package member beyond the runtime port and the woven codec/contract/telemetry libraries, no parallel emitter, no per-kind annotation method, no second scalar table.
- Boundary: codegen emits type stubs and JSON Schema only — `msgspec.Struct` declarations and their `$defs` — never runtime behavior, never a re-derived evidence shape, and never a C# source shape. The C# graduation-evidence bundle is the upstream authority; this owner is a one-directional offline consumer that names no C# interior owner row. `ast`, `msgspec`, `beartype`, the OTel API, and the runtime `Signals` fold are all pure-Python and reflectable on cp315, so there is no deploy gate — distinct from the `grpcio-tools` companion the proto path rides. A string-templated annotation re-parsed through `ast.parse`, a second `_SCALAR_HINT` table racing the node table, parallel `_ann`/`_hint` folds duplicating one descent, a forward `nested` ref that `defstruct` cannot resolve, parallel `element`/`nested` optionals racing the descriptor discriminant, a JSON-only decoder where the seam may seal MessagePack, a generated module that discards the C# `schema_version`, a `dict[str, Any]` mutable default schema carrier, an inline `structlog` line where the canonical `Signals.emit` fold owns receipt egress, a stub generator with no contributed receipt where every sibling evidence owner emits one, and a re-implementation of the `transport/serve#SERVE` proto-stub path are the deleted forms.

```python signature
import ast
import decimal
from collections.abc import Callable
from enum import StrEnum
from functools import reduce
from typing import Annotated, Final, Literal, assert_never

import msgspec
from beartype import beartype
from beartype.conf import BeartypeConf
from beartype.vale import Is
from expression import Error, Ok
from expression.collections import Map
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.trace import StatusCode

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt, Redaction, Signals


type WireFormat = Literal["json", "msgpack"]
type EmitTarget = Literal["stub", "schema", "both"]
type RawBundle = Annotated[bytes, Is[lambda b: len(b) > 0]]


class FieldScalar(StrEnum):
    I32 = "i32"
    I64 = "i64"
    F64 = "f64"
    BOOL = "bool"
    STRING = "string"
    KEY = "key"
    BYTES = "bytes"
    DECIMAL = "decimal"


# --- [CONSTANTS] ------------------------------------------------------------------------

# the one scalar->runtime-type table; both interpreters (`_NODE` ast nodes, `_HINT` runtime types)
# read it, so a new wire primitive is exactly one row and never a second parallel scalar table.
_SCALAR: Final[dict[FieldScalar, type]] = {
    FieldScalar.I32: int,
    FieldScalar.I64: int,
    FieldScalar.F64: float,
    FieldScalar.BOOL: bool,
    FieldScalar.STRING: str,
    FieldScalar.KEY: ContentKey,
    FieldScalar.BYTES: bytes,
    FieldScalar.DECIMAL: decimal.Decimal,
}

_CONF: Final[BeartypeConf] = BeartypeConf(violation_type=TypeError)
_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())  # codegen facts carry no secret field

# --- [MODELS] ---------------------------------------------------------------------------


class FieldDescriptor(Struct, frozen=True, tag_field="kind"):
    name: str


class ScalarField(FieldDescriptor, frozen=True, tag="scalar"):
    scalar: FieldScalar


class ArrayField(FieldDescriptor, frozen=True, tag="array"):
    element: "FieldNode"


class NestedField(FieldDescriptor, frozen=True, tag="nested"):
    ref: str


class MappingField(FieldDescriptor, frozen=True, tag="mapping"):
    key: "FieldNode"
    value: "FieldNode"


class OptionalField(FieldDescriptor, frozen=True, tag="optional"):
    element: "FieldNode"


class UnionField(FieldDescriptor, frozen=True, tag="union"):
    members: tuple["FieldNode", ...]


# the leaf union the decoder targets — never the open base — so `kind` selects exactly one case.
type FieldNode = ScalarField | ArrayField | NestedField | MappingField | OptionalField | UnionField


class OwnerDescriptor(Struct, frozen=True):
    name: str
    fields: tuple[FieldNode, ...]


class EvidenceBundle(Struct, frozen=True):
    schema_version: str
    owners: tuple[OwnerDescriptor, ...]
    bundle_key: ContentKey


class GeneratedModule(Struct, frozen=True):
    schema_version: str
    owner_count: int
    field_count: int
    bundle_key: ContentKey
    source: str = ""
    schema: dict[str, object] = msgspec.field(default_factory=dict)

    def contribute(self) -> tuple[Receipt, ...]:
        facts: dict[str, object] = {
            "schema_version": self.schema_version,
            "owner_count": str(self.owner_count),
            "field_count": str(self.field_count),
            "bundle_key": self.bundle_key.hex,
        }
        return (Receipt.of("compute.codegen", ("emitted", self.bundle_key.hex, facts)),)


class FieldAlgebra[T](Struct, frozen=True):
    scalar: Callable[[FieldScalar], T]
    array: Callable[[T], T]
    optional: Callable[[T], T]
    mapping: Callable[[T, T], T]
    union: Callable[[tuple[T, ...]], T]
    nested: Callable[[str], T]


# --- [TABLES] ---------------------------------------------------------------------------

_DECODER: Final[dict[WireFormat, msgspec.json.Decoder[EvidenceBundle] | msgspec.msgpack.Decoder[EvidenceBundle]]] = {
    "json": msgspec.json.Decoder(type=EvidenceBundle),
    "msgpack": msgspec.msgpack.Decoder(type=EvidenceBundle),
}

_TRACER: Final = trace.get_tracer("compute.codegen")


def _sub(value: str, *elts: ast.expr) -> ast.expr:
    return ast.Subscript(value=ast.Name(id=value, ctx=ast.Load()), slice=ast.Tuple(elts=list(elts), ctx=ast.Load()), ctx=ast.Load())


def _bitor(left: ast.expr, right: ast.expr) -> ast.expr:
    return ast.BinOp(left=left, op=ast.BitOr(), right=right)


# the modules whose names the generated stub imports BARE (`from <mod> import <Name>`): builtins and the
# runtime identity owner that supplies `ContentKey`. Every other scalar (stdlib `decimal`) renders dotted as
# `decimal.Decimal` under a stub-header `import decimal`. One data row keyed to the stub's import policy.
_BARE: Final[frozenset[str]] = frozenset({"builtins", "rasm.runtime.content_identity"})


# the scalar leaf renders to its `ast` node by the type's module+qualname, dotted unless its module is imported
# bare per `_BARE` — never a string re-parsed through `ast.parse`, and never a second hand-keyed `ast` table.
def _qual(tp: type) -> ast.expr:
    parts = (tp.__qualname__ if tp.__module__ in _BARE else f"{tp.__module__}.{tp.__qualname__}").split(".")
    return reduce(lambda node, attr: ast.Attribute(value=node, attr=attr, ctx=ast.Load()), parts[1:], ast.Name(id=parts[0], ctx=ast.Load()))


_NODE: Final[FieldAlgebra[ast.expr]] = FieldAlgebra(
    scalar=lambda s: _qual(_SCALAR[s]),
    array=lambda e: _sub("tuple", e, ast.Constant(value=...)),
    optional=lambda e: _bitor(e, ast.Constant(value=None)),
    mapping=lambda k, v: _sub("dict", k, v),
    union=lambda ms: reduce(_bitor, ms[1:], ms[0]),
    nested=lambda ref: ast.Name(id=ref, ctx=ast.Load()),
)

# one type-building interpreter over the identical descent; feeds `defstruct` -> `schema_components`.
# the `nested` arm yields the forward-ref name resolved by `_ref_hook` rather than an eagerly-bound type.
_HINT: Final[FieldAlgebra[object]] = FieldAlgebra(
    scalar=lambda s: _SCALAR[s],
    array=lambda e: tuple[e, ...],
    optional=lambda e: e | None,
    mapping=lambda k, v: dict[k, v],
    union=lambda ms: reduce(lambda acc, m: acc | m, ms[1:], ms[0]),
    nested=lambda ref: ref,
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _fold[T](node: FieldNode, alg: FieldAlgebra[T]) -> T:
    match node:
        case ScalarField(scalar=scalar):
            return alg.scalar(scalar)
        case ArrayField(element=element):
            return alg.array(_fold(element, alg))
        case OptionalField(element=element):
            return alg.optional(_fold(element, alg))
        case MappingField(key=key, value=value):
            return alg.mapping(_fold(key, alg), _fold(value, alg))
        case UnionField(members=members):
            return alg.union(tuple(_fold(member, alg) for member in members))
        case NestedField(ref=ref):
            return alg.nested(ref)
        case _ as unreachable:
            assert_never(unreachable)


class StubCodegen:
    @staticmethod
    @beartype(conf=_CONF)
    def emit(raw: RawBundle, *, wire: WireFormat = "json", target: EmitTarget = "both") -> RuntimeRail[GeneratedModule]:
        with _TRACER.start_as_current_span("codegen.emit") as span:
            rail: RuntimeRail[GeneratedModule] = boundary(
                "codegen.stub", lambda: StubCodegen._render(_DECODER[wire].decode(raw), target)
            )
            match rail:
                case Ok(module):
                    span.set_attributes(msgspec.to_builtins(module, str_keys=True) | {"wire": wire, "target": target})
                    Signals.emit(module, _REDACTION)  # the `emitted` receipt rides the canonical polymorphic fold
                case Error(fault):
                    span.set_status(StatusCode.ERROR)
                    Signals.emit(Receipt.of("compute.codegen", fault), _REDACTION)  # `rejected` case, not a raw structlog line
            return rail

    @staticmethod
    def _render(bundle: EvidenceBundle, target: EmitTarget) -> GeneratedModule:
        owners = bundle.owners
        return GeneratedModule(
            schema_version=bundle.schema_version,
            owner_count=len(owners),
            field_count=sum(len(owner.fields) for owner in owners),
            bundle_key=bundle.bundle_key,
            source=StubCodegen._source(owners) if target in ("stub", "both") else "",
            schema=msgspec.json.schema_components(StubCodegen._owner_types(owners))[1] if target in ("schema", "both") else {},
        )

    @staticmethod
    def _source(owners: tuple[OwnerDescriptor, ...]) -> str:
        module = ast.Module(body=[StubCodegen._class(owner) for owner in owners], type_ignores=[])
        return ast.unparse(ast.fix_missing_locations(module))

    @staticmethod
    def _class(owner: OwnerDescriptor) -> ast.ClassDef:
        body: list[ast.stmt] = [StubCodegen._field(field) for field in owner.fields] or [ast.Pass()]
        return ast.ClassDef(
            name=owner.name,
            bases=[ast.Name(id="Struct", ctx=ast.Load())],
            keywords=[ast.keyword(arg="frozen", value=ast.Constant(value=True))],
            body=body,
            decorator_list=[],
            type_params=[],
        )

    @staticmethod
    def _field(field: FieldNode) -> ast.AnnAssign:
        return ast.AnnAssign(target=ast.Name(id=field.name, ctx=ast.Store()), annotation=_fold(field, _NODE), value=None, simple=1)

    @staticmethod
    def _owner_types(owners: tuple[OwnerDescriptor, ...]) -> tuple[type, ...]:
        # two-pass forward-ref resolution: the `_HINT` `nested` arm emits the owner-name string, and the shared
        # `namespace` lets `msgspec.defstruct` resolve that forward-ref annotation to the sibling owner type, so
        # `schema_components` cross-links nested owners through `$defs` rather than failing on an unbound name.
        registry: dict[str, type] = {}
        for owner in owners:
            registry[owner.name] = msgspec.defstruct(
                owner.name, [(f.name, _fold(f, _HINT)) for f in owner.fields], frozen=True, namespace=registry
            )
        return tuple(registry.values())
```

## [03]-[RESEARCH]

- [ALGEBRA_CATAMORPHISM]: the descriptor descent is one recursion schema `_fold(node, alg)` over the `FieldNode` union, parameterized by a `FieldAlgebra[T]` whose six `Callable` constructor fields are the per-case interpreters — the tagless-final / catamorphism collapse that writes the traversal once and runs it under `_NODE` (`ast.expr` annotation nodes) and `_HINT` (runtime `defstruct` types) with no duplicated match shape and no parallel scalar table. The stdlib `ast` arms build nodes directly rather than rendering a string re-parsed through `ast.parse(..., mode="eval")` — `ast.Subscript(value, slice, ctx)` with an `ast.Tuple` slice expresses `tuple[T, ...]` and `dict[K, V]`, `ast.BinOp(left, ast.BitOr(), right)` expresses the PEP 604 `T | None` and `A | B | C` union the C# wire union projects to, `_qual` walks a type's `__module__`+`__qualname__` into a dotted `ast.Attribute`/`ast.Name` chain so `decimal.Decimal` resolves without a hand-keyed node, and `ast.fix_missing_locations` over the assembled `ast.Module` seeds the line/column metadata `ast.unparse` requires. The `FieldNode` leaf-union alias is the decoder target so the `tag_field`/`tag` `Struct` keywords select the case off the `kind` field at the C boundary over the closed leaf set. All spellings verify against the cp315 stdlib `ast` grammar and the branch `libs/python/.api/msgspec.md` (`Struct` `tag`/`tag_field`, `json.Decoder`/`msgpack.Decoder`, `json.schema_components` with `schema_hook`, `Meta(extra_json_schema=...)`, `defstruct`, `field`, `to_builtins`).
- [WOVEN_RAIL]: the decode-fold-render is one integrated rail composing the admitted libraries as a single flow — `beartype.vale.Is` refines the `RawBundle` input on the shared `Annotated` alias the `@beartype` boundary checks, `msgspec` owns the polymorphic wire decode over the `WireFormat`-keyed `json.Decoder`/`msgpack.Decoder` pair and the `json.schema_components(..., schema_hook=_ref_hook)` `$defs` projection over the `defstruct`-reconstructed owner types whose forward `nested` refs become `#/$defs/<name>` `$ref`s, `beartype` fences `emit` under the cached `BeartypeConf(violation_type=TypeError)` so a contract breach folds onto the `boundary` fault rather than escaping as a raw `BeartypeCallHintViolation` (`.api/beartype.md` `violation_type` IMPLEMENTATION_LAW), the OTel API `trace.get_tracer`/`start_as_current_span` opens the projection span carrying the `msgspec.to_builtins(module, str_keys=True)` attribute map (`.api/opentelemetry-api.md` `set_attributes` contract) with `set_status(StatusCode.ERROR)` on the fault arm, and the runtime `Signals.emit` polymorphic fold streams the `emitted` receipt on the `Ok` arm and the `rejected` `BoundaryFault` receipt on the `Error` arm through the one canonical receipt egress — never an inline `structlog` line, which `observability/receipts#RECEIPT` owns. The libraries weave into one `RuntimeRail`-returning surface rather than flat one-shot uses, and the rail re-mints no provider — it reads the `observability/telemetry#TELEMETRY`-installed tracer and the `Signals`-configured logger through the global APIs. The `transport/serve#SERVE` `grpcio-tools` proto-stub path stays the distinct gated companion seam; this owner's `ast`/`msgspec`/`beartype`/OTel/runtime stack is cp315-clean and rides the core floor.
