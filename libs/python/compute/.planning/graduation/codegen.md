# [PY_COMPUTE_CODEGEN]

`StubCodegen` is the typed-stub projector: it decodes the C# graduation-evidence bundle once, polymorphically over the wire format the seam chose, and folds each owner descriptor into `msgspec.Struct` stub source and a JSON Schema `$defs` projection through the stdlib `ast` builder — downstream compute composes against the C# owner row by import rather than by re-typing it. Bundles are consumed at the boundary and never re-minted: this owner emits type stubs and schema only, never runtime behavior, and imports nothing from a C# interior.

Descriptor descent is ONE `_fold` recursion schema run by three `FieldAlgebra` interpreters — `_NODE` annotation nodes, `_TYPES` scalar-type collection, `_REFS` nested-edge collection — and the `defstruct` field type IS `ast.unparse(_fold(field, _NODE))`, so the stub annotation and the schema field type cannot diverge on shape. `emit` rides the hub `evidence_run` weave from `graduation/handoff.md#EVIDENCE_WEAVE`; the `EvidenceBundle` wire is OFFLINE — msgspec json/msgpack bytes at rest, never the UDS gRPC leg — so it stays compute-owned and enters no runtime `transport/shapes` registry row until the crossing moves onto the gRPC channel.

## [01]-[INDEX]

- [01]-[STUB_CODEGEN]: the wire-decoded `FieldNode` union, the one `_fold` catamorphism under three interpreters, the format-and-target-polymorphic `emit` rail, and the `drift` round-trip gate on one `StubCodegen` owner.

## [02]-[STUB_CODEGEN]

- Owner: `StubCodegen` — it reads `EvidenceBundle`, `OwnerDescriptor`, and the `FieldNode` leaf union, the shape the C# graduation evidence already carries. `FieldScalar` is the scalar-kind vocabulary whose runtime type lives in the one `_SCALAR` table; the composite kinds are `FieldDescriptor` union cases, not enum members, because they carry sub-shape.
- Cases: the shape kind lives in the case the discriminant selects — parallel `element`/`nested` optionals racing the kind have no owner — and the decoder targets the closed `FieldNode` leaf union, never the open base. `schema_hook` stays reserved for a genuinely custom-typed field: the `key` scalar's `ContentKey` is itself a `Struct` and renders as a struct `$ref` without a hook.
- Entry: `emit(raw, *, wire, target)` is polymorphic over the inbound wire format AND the outbound `EmitTarget` — a consumer wanting only the wire-contract schema or only the importable stub selects a target, never a second generator; both projections descend the same fold over the same decoded descriptors, so they can never disagree on the field set.
- Auto: a `schema_version` the decoder does not carry rails on the typed `("codegen.decode", "schema-version:...")` band, never a best-effort decode off a drifted wire shape; `drift` proves decode AND emit round-trip byte-stability against the C#-minted `evidence-bundle` `CorpusFixture` in the runtime reproduction corpus, a byte drift railing typed.
- Growth: a new wire primitive is one `FieldScalar` member and one `_SCALAR` row the three interpreters absorb with zero extra surface; a new composite shape is one `FieldDescriptor` case, one `FieldNode` union member, one `_fold` arm, and one constructor field on each interpreter; a new inbound wire format is one `WireFormat` member and one decoder row; a new output artifact is one `EmitTarget` member and one fold arm.

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

from rasm.compute.graduation.handoff import EvidenceScope, evidence_run
from rasm.runtime.identity import ContentKey
from rasm.runtime.faults import FAULT_CONF, BoundaryFault, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

# --- [TYPES] ----------------------------------------------------------------------------

type WireFormat = Literal["json", "msgpack"]
type EmitTarget = Literal["stub", "schema", "both"]
# boundary-input refinement the `@beartype(conf=FAULT_CONF)` fence on `_decode` checks in O(1); an empty
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

# one scalar->runtime-type table the `_NODE` and `_TYPES` interpreters read, so a new wire primitive is
# exactly one row and never a second parallel scalar table; `Map` is the folder's one dispatch-table rail.
_SCALAR: Final[Map[FieldScalar, type]] = Map.of_seq([
    (FieldScalar.I32, int),
    (FieldScalar.I64, int),
    (FieldScalar.F64, float),
    (FieldScalar.BOOL, bool),
    (FieldScalar.STRING, str),
    (FieldScalar.KEY, ContentKey),
    (FieldScalar.BYTES, bytes),
    (FieldScalar.DECIMAL, decimal.Decimal),
])

# schema versions this decoder CARRIES; a decoded bundle outside the set rails on the
# `("codegen.decode", "schema-version:...")` typed fault band, never a best-effort decode.
_SCHEMA_VERSIONS: Final[frozenset[str]] = frozenset({"1"})

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


# leaf union the decoder targets — never the open base — so `kind` selects exactly one case.
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
        # bounded scalars only — never the multi-KB `source` or the nested `schema` dict.
        return {
            "schema_version": self.schema_version,
            "owner_count": self.owner_count,
            "field_count": self.field_count,
            "bundle_key": self.bundle_key.hex,
        }

    def contribute(self) -> Iterable[Receipt]:
        # native scalars only — no `str()` coerce where the deterministic renderer keeps types.
        facts: dict[str, object] = dict(self.span_facts)
        return (Receipt.of(EvidenceScope.CODEGEN.value, ("emitted", self.bundle_key.hex, facts)),)


class FieldAlgebra[T](Struct, frozen=True):
    scalar: Callable[[FieldScalar], T]
    array: Callable[[T], T]
    optional: Callable[[T], T]
    mapping: Callable[[T, T], T]
    union: Callable[[tuple[T, ...]], T]
    nested: Callable[[str], T]


# --- [TABLES] ---------------------------------------------------------------------------

_DECODER: Final[Map[WireFormat, msgspec.json.Decoder[EvidenceBundle] | msgspec.msgpack.Decoder[EvidenceBundle]]] = Map.of_seq([
    ("json", msgspec.json.Decoder(type=EvidenceBundle)),
    ("msgpack", msgspec.msgpack.Decoder(type=EvidenceBundle)),
])


def _sub(value: str, *elts: ast.expr) -> ast.expr:
    return ast.Subscript(value=ast.Name(id=value, ctx=ast.Load()), slice=ast.Tuple(elts=list(elts), ctx=ast.Load()), ctx=ast.Load())


def _bitor(left: ast.expr, right: ast.expr) -> ast.expr:
    return ast.BinOp(left=left, op=ast.BitOr(), right=right)


# modules the stub imports bare; every other scalar renders dotted — one row both `_qual` (rendering) and `_imports`
# (preamble) read, so a rendered name and its import never disagree.
_BARE: Final[frozenset[str]] = frozenset({"builtins", "rasm.runtime.identity"})


# scalar leaves render to their `ast` node by module+qualname — never a string re-parsed through `ast.parse`.
def _qual(tp: type) -> ast.expr:
    parts = (tp.__qualname__ if tp.__module__ in _BARE else f"{tp.__module__}.{tp.__qualname__}").split(".")
    return reduce(lambda node, attr: ast.Attribute(value=node, attr=attr, ctx=ast.Load()), parts[1:], ast.Name(id=parts[0], ctx=ast.Load()))


# one annotation interpreter: its `ast.expr` is the single source for BOTH the stub line and the `defstruct` field type — the
# `ast.BinOp(ast.BitOr())` node never evaluates `|`, so a forward ref stays an unbound name `defstruct(namespace=)` resolves at
# class creation rather than the fold-time `TypeError` an eager `str | None` interpreter raises on a nested owner-name.
_NODE: Final[FieldAlgebra[ast.expr]] = FieldAlgebra(
    scalar=lambda s: _qual(_SCALAR[s]),
    array=lambda e: _sub("tuple", e, ast.Constant(value=...)),
    optional=lambda e: _bitor(e, ast.Constant(value=None)),
    mapping=lambda k, v: _sub("dict", k, v),
    union=lambda ms: reduce(_bitor, ms[1:], ms[0]),
    nested=lambda ref: ast.Name(id=ref, ctx=ast.Load()),
)

# scalar runtime types each descriptor references, feeding both the stub preamble and the `defstruct` namespace seed —
# one fold, so the annotation, the import statement, and the namespace binding cannot disagree.
_TYPES: Final[FieldAlgebra[frozenset[type]]] = FieldAlgebra(
    scalar=lambda s: frozenset({_SCALAR[s]}),
    array=lambda e: e,
    optional=lambda e: e,
    mapping=lambda k, v: k | v,
    union=lambda ms: frozenset().union(*ms),
    nested=lambda ref: frozenset(),
)

# `nested` owner-name edges feed the `_ordered` topological build, so every forward ref resolves regardless of seam order.
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
        # weave owns span, fence, and the contributor harvest on the clean exit.
        def rail() -> RuntimeRail[GeneratedModule]:
            return (
                boundary("codegen.decode", lambda: StubCodegen._decode(raw, wire))
                .bind(StubCodegen._carried)
                .bind(lambda bundle: boundary("codegen.render", lambda: StubCodegen._render(bundle, target)))
            )

        return evidence_run(EvidenceScope.CODEGEN, f"emit.{wire}.{target}", rail, facts={"wire": wire, "target": target, "byte_count": len(raw)})

    @staticmethod
    def drift(golden: bytes, expected: GeneratedModule, *, wire: WireFormat = "json") -> RuntimeRail[GeneratedModule]:
        # golden bundle re-emits and the projection must equal the pinned expected byte-for-byte under the deterministic encoder.
        pinned = msgspec.json.Encoder(order="deterministic")

        def check(module: GeneratedModule) -> RuntimeRail[GeneratedModule]:
            if pinned.encode(module) == pinned.encode(expected):
                return Ok(module)
            return Error(BoundaryFault(boundary=("codegen.drift", expected.schema_version)))

        return StubCodegen.emit(golden, wire=wire).bind(check)

    @staticmethod
    def _carried(bundle: EvidenceBundle) -> RuntimeRail[EvidenceBundle]:
        if bundle.schema_version in _SCHEMA_VERSIONS:
            return Ok(bundle)
        return Error(BoundaryFault(boundary=("codegen.decode", f"schema-version:{bundle.schema_version}")))

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def _decode(raw: RawBundle, wire: WireFormat) -> EvidenceBundle:
        # beartype fence sits on the thunk the `boundary` wraps, NOT on `emit`, so a `RawBundle` breach and a `DecodeError`
        # both land INSIDE the fence and fold onto the rail.
        return _DECODER[wire].decode(raw)

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
        return frozenset().union(*(_fold(field, _TYPES) for owner in owners for field in owner.fields))

    @staticmethod
    def _imports(owners: tuple[OwnerDescriptor, ...]) -> list[ast.stmt]:
        # data-driven off the `_TYPES` fold, so the stub imports exactly its own bases and scalars — no hardcoded list racing `_qual`/`_BARE`.
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
        # `StructMeta` evaluates each field's string annotation against `namespace` at class creation — the deferred string
        # `"Owner | None"` evaluates cleanly once `Owner` is registered where an eager `str | None` TypeErrors — so `_ordered`
        # sequences the build and a `nested` ref always resolves against an already-registered sibling, `schema_components`
        # cross-linking it as `#/$defs/<name>` with no `schema_hook`.
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
        # owner graph is a DAG by contract — a mid-construction name is unresolvable to `defstruct` — so the `visiting` gray
        # set turns a back-edge or self-ref into a deterministic `ValueError` the fence folds, never an unbounded `RecursionError`;
        # a ref absent from the bundle is left to `defstruct` to surface as the unbound-name fault.
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

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
