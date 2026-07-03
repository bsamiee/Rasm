# [PY_COMPUTE_CODEGEN]

The typed-stub projector that decodes the C# graduation-evidence bundle and synthesizes `msgspec.Struct` stubs through the stdlib `ast` builder. `StubCodegen` consumes the offline-seam bundle the C# graduation evidence seals, decodes it once polymorphically over the wire format the seam chose, and folds each owner descriptor into Python type-stub source — so downstream compute composes against the C# owner row by import rather than by re-typing it. The descriptor is a wire-decoded discriminated union: `FieldDescriptor` is the `tag_field`-discriminated `Struct` family whose scalar case carries a `FieldScalar` literal and whose composite cases (`array`/`nested`/`mapping`/`optional`/`union`) carry their typed sub-descriptors, so the shape kind lives in the case the discriminant selects rather than in parallel `element`/`nested` optionals racing the kind; the decoder targets the closed `FieldNode` leaf union, never the open base, so `kind` selects exactly one case. The descriptor descent is ONE recursion-schema `_fold(node, alg)` parameterized by a `FieldAlgebra[T]` output interpreter — the catamorphism is written once and run by three interpreters: `_NODE` producing the `ast.expr` annotation, `_TYPES` collecting the scalar runtime types, and `_REFS` collecting the `nested` owner-name edges. The divergence-free invariant is a literal identity rather than a parallel claim: the `defstruct` field type IS `ast.unparse(_fold(field, _NODE))`, the same `ast.expr` the stub source renders, so the stub annotation and the schema field type cannot diverge on shape. A separate `_HINT` interpreter that re-evaluated `|`/subscription at fold time is the deleted form — it raised `TypeError` on `optional`/`union` over a `nested` owner-name string (`str | None` is unsupported), whereas the `ast.BinOp(ast.BitOr())` node never evaluates the operator and the forward ref stays a name `defstruct(namespace=)` binds at class creation. The eight wire primitives live in the one `_SCALAR` table the annotation and the type-collection interpreters read, and there is no second per-kind annotation method, parallel scalar table, or hand-written dependency walk. No annotation is rendered to a string and re-parsed through `ast.parse`: every node is synthesized, the scalar leaf rendering to its dotted module+qualname `ast` node so `decimal.Decimal` and `ContentKey` resolve without a hand-keyed `ast` row, and a data-driven import preamble (synthesized off the same `_TYPES` fold over the `_SCALAR`/`_BARE` policy) prepends `from msgspec import Struct` plus exactly the scalar imports the annotations name — so the emitted stub is importable, not a body of unresolved bases and scalar names. `emit` is polymorphic over the inbound wire format (`json`/`msgpack`, one `msgspec` codec) AND the outbound target (`stub` source, `schema` `$defs`, or `both`), and runs the decode-fold-render under one OTel span weaving `msgspec`, `beartype`, OTel, and the runtime `Signals` receipt fold into a single rail. The `RawBundle` `beartype.vale.Is` refinement is checked by `@beartype(conf=FAULT_CONF)` on the inner `_decode` thunk the runtime `boundary` wraps — so a contract breach folds onto the fault rail through the `CLASSIFY` `api` row rather than escaping the owner — and the `emitted` receipt streams through the `@receipted(_REDACTION)` egress aspect wearing the `_emit` builder inside the same `boundary` thunk, the canonical decorator rail `observability/receipts#RECEIPT` declares and the sibling `graduation/handoff.md#GRADUATION`/`experiments/inference.md#BAYESIAN`/`experiments/model.md#ASSET` owners hold, never an inline `Signals.emit` double-emit threaded through the body and never an inline `structlog` line. The bundle is consumed at the boundary and never re-minted: this owner reads the evidence shape the C# graduation evidence already carries, emits type stubs and JSON Schema only, never runtime behavior, and imports nothing from a C# interior.

## [01]-[INDEX]

- [01]-[STUB_CODEGEN]: the wire-decoded `FieldNode` union, the one `_fold` recursion schema run under three `FieldAlgebra` interpreters (`_NODE` annotation nodes whose `ast.unparse` IS both the stub source and the `defstruct` field type, `_TYPES` scalar-type collection feeding the import preamble and the `defstruct` namespace seed, `_REFS` nested-edge collection for the dependency-ordered `defstruct` build), the data-driven import preamble, the format-and-target-polymorphic `emit` rail folding the contract fence onto the `boundary`, and the `GeneratedModule` receipt streamed through the `@receipted(_REDACTION)` egress aspect over the `_emit` builder.

## [02]-[STUB_CODEGEN]

- Owner: `StubCodegen` — the one projector consuming the C# graduation-evidence bundle and emitting typed Python stubs plus their JSON Schema; it reads `EvidenceBundle`, `OwnerDescriptor`, and the `FieldNode` leaf union (the shape the C# graduation evidence carries) and folds each descriptor into an `ast.ClassDef` through the stdlib `ast` builder and into a `$defs` component through `msgspec.json.schema_components`. `FieldScalar` is the scalar-kind `StrEnum` whose runtime type lives in the `_SCALAR` data table the `_NODE` annotation and `_TYPES` type-collection interpreters read; the composite kinds are `FieldDescriptor` union cases, not enum members, because they carry sub-shape. The bundle decodes once and is re-minted nowhere.
- Cases: `FieldDescriptor` is the `Struct` family tagged on `kind` — `scalar(FieldScalar)`, `array(element)`, `nested(ref)`, `mapping(key, value)`, `optional(element)`, and `union(members)` — so `array`/`optional` carry the one inner `FieldNode`, `mapping` carries the key/value pair, `union` carries the `Meta(min_length=1)`-bounded member tuple (an empty union is a decode-time `ValidationError` the `CLASSIFY` codec row folds onto the rail, so the `_NODE` `union` left-fold over `members[0]` is total by construction), and `nested` carries the forward owner name; the former parallel `element: FieldDescriptor | None` and `nested: str | None` optionals collapse into the case payload the `kind` selects, and the `FieldNode` alias unions the six leaves so `msgspec.json.Decoder(type=...)` discriminates over the closed leaf set rather than the open base. `FieldScalar` folds the eight wire primitives the C# seam emits — `i32`, `i64`, `f64`, `bool`, `string`, `key`, `bytes`, `decimal` — each mapping through the one `_SCALAR` table to its runtime type (`int`, `float`, `bool`, `str`, `ContentKey`, `bytes`, `decimal.Decimal`) the `_NODE` annotation interpreter and the `_TYPES` type-collection interpreter read; the third interpreter `_REFS` reads no scalar, carrying only the `nested` owner-name edges for the build order. A new scalar primitive is one `FieldScalar` member and one `_SCALAR` row; a new composite kind is one `FieldDescriptor` case plus one `_fold` match arm plus one constructor field on each of the three `FieldAlgebra` interpreters; never a parallel emitter.
- Fold: `_fold(node, alg)` is the one total recursion schema over the `FieldNode` union — a catamorphism parameterized by a `FieldAlgebra[T]` whose six `Callable` fields name the per-case constructors — closed by `assert_never` so an unmodelled kind is a compile-surfaced gap. It is written once and run by THREE interpreters over the identical descent. `_NODE: FieldAlgebra[ast.expr]` synthesizes the `ast` annotation directly (`array` -> `tuple[..., ...]` via `ast.Subscript`, `optional` -> `inner | None` via `ast.BinOp(ast.BitOr())`, `mapping` -> `dict[k, v]`, `union` left-folds members through `ast.BitOr`, `nested` -> the bare forward-ref `ast.Name`, `scalar` -> the dotted module+qualname node `_qual` builds), and its `ast.unparse` IS both the stub source line and the `defstruct` field type — so the divergence-free invariant is a literal identity, the schema and the stub descending from the same `ast.expr` rather than a parallel `_HINT` interpreter claimed to agree. A second `_HINT: FieldAlgebra[object]` producing runtime types by re-evaluating `tuple[e, ...]`/`e | None`/`reduce(acc | m, ...)` at fold time is the deleted form: `str | None` and `str | type` raise `TypeError` on a `nested` owner-name string, so `_HINT` diverged from `_NODE` exactly on `optional`/`union` over a nested ref, whereas the `ast.BinOp(ast.BitOr())` node never evaluates the operator and the forward ref stays a name the `namespace=` binds at class creation. `_TYPES: FieldAlgebra[frozenset[type]]` collects the scalar runtime types each descriptor references (composites union their children, `nested` contributes the empty set) so `_imports` synthesizes the stub import preamble AND `_owner_types` seeds the `defstruct` namespace from the same `_SCALAR`/`_BARE` policy `_NODE` reads — one fold feeding the annotation, the import statement, and the namespace binding so they cannot disagree on shape. `_REFS: FieldAlgebra[frozenset[str]]` is the mirror image collecting the `nested` owner-name edges (`scalar` contributes the empty set, `nested` carries the one ref) so `_ordered` topologically sequences the `defstruct` build off the same descent rather than a hand-walked edge scan. There is no second per-kind annotation method, parallel `_SCALAR_HINT` table, hand-keyed import list, or hand-written dependency walk. No annotation is rendered to a string and re-parsed through `ast.parse` — every node is synthesized, so a malformed sub-shape is impossible by construction.
- Entry: `StubCodegen.emit(raw, *, wire="json", target="both")` returns `RuntimeRail[GeneratedModule]` under one OTel span — it wraps the inner `_decode` thunk in the runtime `boundary` fence, selects the `msgspec` decoder for the inbound `WireFormat`, decodes the bundle once, runs `_fold` under `_NODE` to render `ast.ClassDef` nodes via `ast.unparse` when the `EmitTarget` requests `stub`, and reconstructs the owner structs from the same `ast.unparse(_fold(field, _NODE))` field-type strings — resolved through `defstruct(namespace=registry)` — that `msgspec.json.schema_components` projects to a `$defs` schema when it requests `schema`. The `RawBundle` = `Annotated[bytes, beartype.vale.Is[...]]` refinement is checked by `@beartype(conf=FAULT_CONF)` on `_decode` — the inner thunk the `boundary` wraps, not `emit` itself — so a refinement breach raises the canonical `BeartypeCallHintViolation` INSIDE the fence and the `CLASSIFY` `api` row folds it onto the one rail rather than letting it escape `emit` past the boundary, and the same fence catches a `msgspec` `DecodeError`; this binds the one shared runtime `FAULT_CONF`, never a private `violation_type=TypeError` conf that both re-mints the cached config and downgrades the breach to the catch-all `boundary` case. The span carries only the bounded `GeneratedModule.span_facts` scalar map (`owner_count`/`field_count` as native `int`, `schema_version` and `bundle_key.hex` as `str` — exactly the `str | int` set `Span.set_attributes` admits), never `msgspec.to_builtins(module)` which would attach the multi-KB rendered `source` and the nested `schema` dict (not a valid attribute value). Both writes sit behind the `is_recording()` gate the sibling `experiments/model.md#ASSET`, `experiments/inference.md#BAYESIAN`, and `graduation/handoff.md#GRADUATION` owners hold — the pre-boundary `wire`/`target` dims both arms need and the `Ok`-arm `span_facts` the rejected bundle has no module to carry — so a no-op span builds no attribute map. Span-egress ownership is split exactly as `evidence/identity#IDENTITY` `derived` and `transport/wire#WIRE_RAIL` `Decode._traced` split it: the `boundary` thunk wraps the `@receipted(_REDACTION)` `_emit` builder so the `emitted` receipt streams through the aspect's `Signals.emit` fold on the success path inside the fence (an emit-time render or sink raise folding onto the same rail), the `Ok` arm then sets `Status(StatusCode.OK)` itself — the one annotation the conversion does not own — and the `Error` arm is a bare `pass` because the `boundary` fence's `reliability/faults#FAULT` `_convert` already `record_exception`s the cause and sets `Status(StatusCode.ERROR, fault.tag)` on the same active span; a second `set_status(StatusCode.ERROR)` on the fault arm is the faults-owner-egress trample, the inline `Signals.emit(module)`-plus-fault-arm-`Signals.emit(Receipt.of(owner, fault))` double-emit is the deleted form `experiments/study.md#STUDY` names (the rejected receipt is not streamed; the fence's span annotation is the rejection evidence the sibling `pass`-arm owners hold), and a hand-built `structlog.get_logger().error` line is the deleted egress. `GeneratedModule` carries the rendered source, the optional schema `$defs`, the owner count, the field count, the C# `schema_version` (no longer decoded-and-discarded), and the bundle `ContentKey`; its `span_facts` is the one bounded-scalar fact source both the span and `contribute` read, and `contribute` renders that map to the `(Receipt, ...)` stream the `ReceiptContributor` port expects.
- Nested resolution: every `defstruct` field type is the `ast.unparse(_fold(field, _NODE))` string, so a `nested` field's annotation is the bare forward owner-name and a composite over a `nested` ref (`Owner | None`, `tuple[Owner, ...]`, `int | Owner`) is the string carrying that name unevaluated; `msgspec.defstruct(..., namespace=registry)` evaluates the string against the registry at class creation and binds the owner-name to the already-built sibling struct, so `msgspec.json.schema_components` cross-links the nested owner as a `#/$defs/<name>` `$ref` natively — no `schema_hook` and no unresolved-forward-type decode failure. Stringizing the whole annotation (rather than the eager `_HINT` `e | None`) is what lets `optional`/`union` over a nested ref resolve at all: `str | None` raises `TypeError` at fold time, but the deferred string `"Owner | None"` evaluates cleanly once `Owner` is in the namespace. `msgspec.StructMeta` resolves field types at class creation, not at schema-projection time, so the referenced sibling must already be in `registry` when its referrer is built; the seam does not guarantee that emission order, so `_ordered` topologically sequences the build off the `_REFS` `nested`-edge fold and a referenced sibling always precedes its referrer regardless of seam order. Sequential `defstruct(namespace=registry)` binds a `nested` ref only to an ALREADY-built sibling — a name still mid-construction is unresolvable (`schema_components` raises `NameError` on a self-referencing struct built this way), so the owner graph is a DAG by contract; `_ordered` carries a `visiting` gray set that turns a back-edge (`A -> B -> A` or a self-ref) into a deterministic `ValueError` the `boundary` fence converts to one typed fault, never the unbounded `RecursionError` an unguarded descent would blow. The `_NODE` `nested` arm renders the same forward name as a bare `ast.Name`, so the stub source and the schema agree the field references a sibling owner. `schema_hook` stays reserved for a genuinely custom-typed field (a type `schema_components` cannot introspect); the `key` scalar's `ContentKey` is itself a `Struct`, so it renders as a struct `$ref` without a hook.
- Target fold: `EmitTarget` discriminates `stub`, `schema`, and `both`, parameterizing the OUTPUT shape the one rail produces — `stub` renders only the `ast` module source, `schema` projects only the `msgspec.json.schema_components` `$defs` over the reconstructed owner structs, `both` carries each. Both projections descend the same `_fold` over the same decoded `OwnerDescriptor` tuple, so the stub source and the schema can never disagree on the field set; a consumer wanting only the wire-contract schema or only the importable stub selects a target rather than calling a second generator.
- Packages: stdlib `ast` (`ClassDef`/`AnnAssign`/`Subscript`/`Name`/`Attribute`/`Tuple`/`BinOp`/`BitOr`/`keyword`/`Constant`/`Import`/`ImportFrom`/`alias`/`Module`/`Pass`/`fix_missing_locations`/`unparse` — the one annotation source for both the stub line and the `defstruct` field-type string), stdlib `importlib.import_module` (seeds the `defstruct` namespace with the top-level scalar module per the `_BARE`/`_qual` policy), `msgspec` (`Struct` with `tag`/`tag_field`-discriminated `FieldNode` union, `field(default_factory=...)`, `json.Decoder`/`msgpack.Decoder` over the `WireFormat`-keyed pair, `json.schema_components`, `defstruct` resolving each unparsed-annotation string field type through `namespace=`), `beartype` (`@beartype` on `_decode` binding the runtime `FAULT_CONF`, `beartype.vale.Is` refinement on the `RawBundle` alias), `opentelemetry-api` (`trace.get_tracer`/`Tracer.start_as_current_span`/`Span.is_recording`/`Span.set_attributes`/`Span.set_status`/`StatusCode` for the projection span), `expression` (`Map.empty`, `Ok`/`Error`), stdlib `functools.reduce` (the `_qual` dotted-attribute walk and the `_NODE` `union` left-fold over `ast.BitOr`), runtime (`RuntimeRail`/`boundary` the one fault fence, `FAULT_CONF` the one shared domain `BeartypeConf`, `ContentKey`, `Receipt`/`ReceiptContributor`/`Redaction`/`receipted` the `@receipted(_REDACTION)` egress aspect the `_emit` builder wears, the aspect's `Signals.emit` fold referenced as the egress contract rather than imported here).
- Growth: a new wire primitive is one `FieldScalar` member plus one `_SCALAR` row the `_NODE` annotation and `_TYPES` type-collection interpreters absorb (the annotation renders it, the `_TYPES` fold imports its module into the preamble and binds it into the `defstruct` namespace with zero extra surface; `_REFS` ignores it since a scalar names no owner edge); a new composite shape is one `FieldDescriptor` case plus one `FieldNode` union member plus one `_fold` match arm plus one constructor field on each of the three `FieldAlgebra` interpreters; a new inbound wire format is one `WireFormat` member plus one decoder row; a new output artifact is one `EmitTarget` member plus one fold arm; zero new surface, no external-package member beyond the runtime port and the woven codec/contract/telemetry libraries, no parallel emitter, no per-kind annotation method, no second scalar table, no hand-keyed import list, no hand-written dependency walk.

```python signature
import ast
import decimal
import importlib
from collections.abc import Callable, Iterable
from enum import StrEnum
from functools import reduce
from typing import Annotated, Final, Literal, assert_never

import msgspec
from beartype import beartype
from beartype.vale import Is
from expression import Error, Ok
from expression.collections import Map
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt, Redaction, receipted

# --- [TYPES] ----------------------------------------------------------------------------

type WireFormat = Literal["json", "msgpack"]
type EmitTarget = Literal["stub", "schema", "both"]
# the boundary-input refinement the `@beartype(conf=FAULT_CONF)` fence on `_decode` checks in O(1); an empty
# payload raises `BeartypeCallHintViolation` the `CLASSIFY` `api` row folds onto the rail.
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

# the one scalar->runtime-type table the `_NODE` and `_TYPES` interpreters read, so a new wire primitive is
# exactly one row and never a second parallel scalar table.
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
    # `Meta(min_length=1)` rejects a zero-member union at the wire boundary so the `_NODE` `union`
    # left-fold over `ms[0]` is total by construction — an empty `members` is a decode-time
    # `ValidationError` the `CLASSIFY` codec row folds onto the rail, never an `IndexError` in the fold.
    members: Annotated[tuple["FieldNode", ...], msgspec.Meta(min_length=1)]


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

    @property
    def span_facts(self) -> dict[str, str | int]:
        # the one bounded-scalar fact source both the span and the receipt read — exactly the `str | int` set
        # `Span.set_attributes` admits, never the multi-KB `source` or the nested `schema` dict.
        return {
            "schema_version": self.schema_version,
            "owner_count": self.owner_count,
            "field_count": self.field_count,
            "bundle_key": self.bundle_key.hex,
        }

    def contribute(self) -> Iterable[Receipt]:
        # the `(owner, (phase, subject, facts))` two-arg `Receipt.of` the `fact` case discriminates, the form
        # every sibling evidence contributor mints; widening `span_facts` to the `dict[str, object]` slot keeps
        # the native `int` counts off a `str()` coerce through the `enc_hook=repr` renderer.
        facts: dict[str, object] = dict(self.span_facts)
        return (Receipt.of("compute.codegen", ("emitted", self.bundle_key.hex, facts)),)


class FieldAlgebra[T](Struct, frozen=True):
    scalar: Callable[[FieldScalar], T]
    array: Callable[[T], T]
    optional: Callable[[T], T]
    mapping: Callable[[T, T], T]
    union: Callable[[tuple[T, ...]], T]
    nested: Callable[[str], T]


# --- [SERVICES] -------------------------------------------------------------------------

_TRACER: Final[trace.Tracer] = trace.get_tracer("compute.codegen")

# --- [TABLES] ---------------------------------------------------------------------------

_DECODER: Final[dict[WireFormat, msgspec.json.Decoder[EvidenceBundle] | msgspec.msgpack.Decoder[EvidenceBundle]]] = {
    "json": msgspec.json.Decoder(type=EvidenceBundle),
    "msgpack": msgspec.msgpack.Decoder(type=EvidenceBundle),
}


def _sub(value: str, *elts: ast.expr) -> ast.expr:
    return ast.Subscript(value=ast.Name(id=value, ctx=ast.Load()), slice=ast.Tuple(elts=list(elts), ctx=ast.Load()), ctx=ast.Load())


def _bitor(left: ast.expr, right: ast.expr) -> ast.expr:
    return ast.BinOp(left=left, op=ast.BitOr(), right=right)


# the modules the stub imports bare (`from <mod> import <Name>`): builtins and the identity owner of
# `ContentKey`. Every other scalar renders dotted (`decimal.Decimal` under `import decimal`). The one row both
# `_qual` (rendering) and `_imports` (preamble) read, so a rendered name and its import never disagree.
_BARE: Final[frozenset[str]] = frozenset({"builtins", "rasm.runtime.content_identity"})


# the scalar leaf renders to its `ast` node by the type's module+qualname, dotted unless its module is imported
# bare per `_BARE` — never a string re-parsed through `ast.parse`, and never a second hand-keyed `ast` table.
def _qual(tp: type) -> ast.expr:
    parts = (tp.__qualname__ if tp.__module__ in _BARE else f"{tp.__module__}.{tp.__qualname__}").split(".")
    return reduce(lambda node, attr: ast.Attribute(value=node, attr=attr, ctx=ast.Load()), parts[1:], ast.Name(id=parts[0], ctx=ast.Load()))


# the one annotation interpreter; its `ast.expr` is the single source for BOTH the stub line (`ast.unparse`)
# and the `defstruct` field type — the `ast.BinOp(ast.BitOr())` node never evaluates `|`, so a forward ref
# stays an unbound name `defstruct(namespace=)` resolves at class creation rather than a fold-time `TypeError`.
_NODE: Final[FieldAlgebra[ast.expr]] = FieldAlgebra(
    scalar=lambda s: _qual(_SCALAR[s]),
    array=lambda e: _sub("tuple", e, ast.Constant(value=...)),
    optional=lambda e: _bitor(e, ast.Constant(value=None)),
    mapping=lambda k, v: _sub("dict", k, v),
    union=lambda ms: reduce(_bitor, ms[1:], ms[0]),
    nested=lambda ref: ast.Name(id=ref, ctx=ast.Load()),
)

# the second interpreter over the identical descent: the scalar runtime types each descriptor references,
# feeding both the stub preamble and the `defstruct` namespace seed. `nested` contributes nothing; composites
# union their children, so `dict[ContentKey, decimal.Decimal]` folds to `{ContentKey, Decimal}`.
_TYPES: Final[FieldAlgebra[frozenset[type]]] = FieldAlgebra(
    scalar=lambda s: frozenset({_SCALAR[s]}),
    array=lambda e: e,
    optional=lambda e: e,
    mapping=lambda k, v: k | v,
    union=lambda ms: frozenset().union(*ms),
    nested=lambda ref: frozenset(),
)

# the third interpreter over the identical descent, mirror of `_TYPES`: the `nested` owner-name edges feeding
# the `_ordered` topological build. `scalar` contributes nothing; `nested` carries the one ref, so a
# `tuple[Owner, ...]` field folds to `{Owner}` and the build resolves every forward ref regardless of seam order.
_REFS: Final[FieldAlgebra[frozenset[str]]] = FieldAlgebra(
    scalar=lambda s: frozenset(),
    array=lambda e: e,
    optional=lambda e: e,
    mapping=lambda k, v: k | v,
    union=lambda ms: frozenset().union(*ms),
    nested=lambda ref: frozenset({ref}),
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
    def emit(raw: bytes, *, wire: WireFormat = "json", target: EmitTarget = "both") -> RuntimeRail[GeneratedModule]:
        with _TRACER.start_as_current_span("codegen.emit") as span:
            if span.is_recording():
                span.set_attributes({"wire": wire, "target": target})  # the format/target dims both arms need, behind the no-op gate
            rail: RuntimeRail[GeneratedModule] = boundary("codegen.stub", lambda: StubCodegen._emit(StubCodegen._decode(raw, wire, target)))
            match rail:
                case Ok(module):
                    if span.is_recording():
                        span.set_attributes(module.span_facts)  # the rejected bundle has no module to widen with
                    span.set_status(Status(StatusCode.OK))  # the ERROR side is the `boundary` fence's `_convert`, never re-set here
                case Error(_):
                    pass
            return rail

    @staticmethod
    @receipted(_REDACTION)
    def _emit(module: GeneratedModule) -> GeneratedModule:
        # the `@receipted(_REDACTION)` egress aspect harvests `module.contribute()` and streams the `emitted`
        # receipt on exit INSIDE the `boundary` thunk, so a render or sink raise folds onto the same rail and
        # the `Error` arm needs no body — the fence's `_convert` already recorded the cause; the inline
        # double-emit (`Signals.emit(module)` plus a fault-arm `Signals.emit(Receipt.of(owner, fault))`) is the
        # deleted form `experiments/study.md#STUDY` names and the sibling `graduation/handoff.md#GRADUATION`,
        # `experiments/inference.md#BAYESIAN`, `experiments/model.md#ASSET` replaced with this `@receipted` weave.
        return module

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def _decode(raw: RawBundle, wire: WireFormat, target: EmitTarget) -> GeneratedModule:
        # the `@beartype(conf=FAULT_CONF)` fence sits on the thunk the `boundary` wraps, NOT on `emit`, so a
        # `RawBundle` breach raises `BeartypeCallHintViolation` and a `DecodeError` lands INSIDE the fence —
        # both fold onto the rail through the `CLASSIFY` rows rather than escaping the owner.
        return StubCodegen._render(_DECODER[wire].decode(raw), target)

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
        body: list[ast.stmt] = [*StubCodegen._imports(owners), *(StubCodegen._class(owner) for owner in owners)]
        return ast.unparse(ast.fix_missing_locations(ast.Module(body=body, type_ignores=[])))

    @staticmethod
    def _scalars(owners: tuple[OwnerDescriptor, ...]) -> frozenset[type]:
        # the one `_TYPES` fold both the import preamble and the `defstruct` namespace seed read, so the
        # rendered scalar names, their import statements, and their namespace bindings descend one set.
        return frozenset().union(*(_fold(field, _TYPES) for owner in owners for field in owner.fields))

    @staticmethod
    def _imports(owners: tuple[OwnerDescriptor, ...]) -> list[ast.stmt]:
        # data-driven off the `_TYPES` fold: the `from msgspec import Struct` base plus `from <mod> import
        # <Name>` for a `_BARE` scalar, `import <mod>` for a dotted scalar, nothing for a `builtins` scalar —
        # so the stub imports exactly its own bases and scalars, no hardcoded list racing `_qual`/`_BARE`.
        scalars = StubCodegen._scalars(owners)
        bare = {tp.__module__: tp.__qualname__ for tp in scalars if tp.__module__ in _BARE - {"builtins"}}
        dotted = sorted({tp.__module__ for tp in scalars} - _BARE)
        base: ast.stmt = ast.ImportFrom(module="msgspec", names=[ast.alias(name="Struct")], level=0)
        bare_imports = [ast.ImportFrom(module=mod, names=[ast.alias(name=name)], level=0) for mod, name in sorted(bare.items())]
        dotted_imports = [ast.Import(names=[ast.alias(name=mod)]) for mod in dotted]
        return [base, *bare_imports, *dotted_imports]

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
        # `StructMeta` evaluates each field's string annotation against `namespace` at class creation, so the
        # `defstruct` field type is `ast.unparse(_fold(f, _NODE))` — the SAME `ast.expr` the stub renders. The
        # namespace seeds off the same `_TYPES`/`_BARE`/`_qual` policy (the `decimal` module for `decimal.Decimal`,
        # the `ContentKey` type for its bare name), and `_ordered` sequences the build so a `nested` ref always
        # resolves against an already-registered sibling and `schema_components` cross-links it as `#/$defs/<name>`.
        scalars = StubCodegen._scalars(owners)
        registry: dict[str, object] = {}
        for tp in scalars:
            if tp.__module__ in _BARE:
                registry[tp.__qualname__] = tp  # `ContentKey` resolves the bare name
            else:
                head = tp.__module__.split(".", 1)[0]
                registry[head] = importlib.import_module(head)  # `decimal.Decimal` resolves off the top package name
        for owner in StubCodegen._ordered(owners):
            registry[owner.name] = msgspec.defstruct(
                owner.name, [(f.name, ast.unparse(_fold(f, _NODE))) for f in owner.fields], frozen=True, namespace=registry
            )
        return tuple(registry[owner.name] for owner in owners)

    @staticmethod
    def _ordered(owners: tuple[OwnerDescriptor, ...]) -> tuple[OwnerDescriptor, ...]:
        # depth-first topological order over the `nested`-ref edge set; a ref absent from the bundle is left to
        # `defstruct` to surface as the unbound-name fault. The `visiting` gray set turns a back-edge (a cyclic
        # graph or self-ref, outside the DAG contract) into a deterministic `ValueError` the fence folds, never
        # the unbounded `RecursionError` an unguarded descent would blow.
        by_name = {owner.name: owner for owner in owners}
        out: dict[str, OwnerDescriptor] = {}
        visiting: set[str] = set()

        def visit(owner: OwnerDescriptor) -> None:
            if owner.name in out:
                return
            if owner.name in visiting:
                raise ValueError(f"cyclic owner reference at {owner.name!r}")
            visiting.add(owner.name)
            for ref in frozenset().union(*(_fold(field, _REFS) for field in owner.fields)):
                if (dep := by_name.get(ref)) is not None:
                    visit(dep)
            visiting.discard(owner.name)
            out[owner.name] = owner

        for owner in owners:
            visit(owner)
        return tuple(out.values())
```

## [03]-[RESEARCH]
