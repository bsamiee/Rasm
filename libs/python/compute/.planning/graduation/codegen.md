# [PY_COMPUTE_CODEGEN]

`StubCodegen` is the typed-stub projector: it decodes the C# graduation-evidence bundle once — canonical UTF-8 JSON under the producer's CamelCase wire policy, the one format `dotnet:Rasm.Compute/Model/identity#GRADUATION_EVIDENCE` emits — and folds each owner descriptor into `msgspec.Struct` stub source and a JSON Schema `$defs` projection through the stdlib `ast` builder; downstream compute composes against the .NET owner row by import rather than by re-typing it. Bundles are consumed at the boundary and never re-minted: this owner emits type stubs and schema only, never runtime behavior, and imports nothing from a C# interior.

Descriptor descent is ONE `_fold` recursion schema run by three `FieldAlgebra` interpreters — `_NODE` annotation nodes, `_TYPES` scalar-type collection, `_REFS` nested-edge collection — and the `defstruct` field type IS `ast.unparse(_fold(field, _NODE))`, so the stub annotation and the schema field type cannot diverge on shape. `emit` rides the hub `evidence_run` weave from `graduation/handoff#EVIDENCE_WEAVE`; the `EvidenceBundle` wire is OFFLINE — msgspec json bytes at rest, never the UDS gRPC leg — so it stays compute-owned and enters no runtime `transport/shapes` registry row until the crossing moves onto the gRPC channel.

## [01]-[INDEX]

- [02]-[STUB_CODEGEN]: the wire-decoded `FieldNode` union, the one `_fold` catamorphism under three interpreters, the target-polymorphic `emit` rail with its `emit_async` operational trail, and the `drift` round-trip gate on one `StubCodegen` owner.

## [02]-[STUB_CODEGEN]

- Owner: `StubCodegen` — it decodes the LANDED peer mint `dotnet:Rasm.Compute/Model/identity#GRADUATION_EVIDENCE`: `EvidenceBundle` mirrors `GraduationEvidence(SchemaVersion, Owners, BundleKey)` under the `ComputeWireContext` CamelCase policy (`rename="camel"`), `OwnerDescriptor` its `(Name, Fields)` pair, and the `FieldNode` leaf union the six `[JsonDerivedType]` kind literals `scalar`/`array`/`nested`/`mapping`/`optional`/`union` — one agreed roster at both ends, no assumed shape. `FieldScalar` transcribes the peer's eight locked rows; its runtime type lives in the one `_SCALAR` table; the composite kinds are `FieldDescriptor` union cases, not enum members, because they carry sub-shape.
- Cases: the shape kind lives in the case the discriminant selects — parallel `element`/`nested` optionals racing the kind have no owner — and the decoder targets the closed `FieldNode` leaf union, never the open base. `schema_hook` stays reserved for a genuinely custom-typed field: the `key` scalar's `ContentKey` is itself a `Struct` and renders as a struct `$ref` without a hook. `bundle_key` crosses as the bare 32-hex key render under the estate x32 content-key law, parsed by `_bundle_key` into the typed `ContentKey`, never a raw integer column double precision shreds; the retired manifest ordinal carried no surviving case authority.
- Law: a landed emission reaches the `python:runtime/observability/journal#LEDGER` plane as one `OPERATIONAL` `AuditFact` keyed on the bundle it projected, and `emit_async` is its ONE seat — the awaitable twin this pure fold mints over the band hop, since recording suspends. The fact mints off the CLEARED projection, so a decode or render fault names no stub nobody generated, and the target is the bundle key rather than a path: this owner writes no file, so naming a location asserts a write nobody made. `drift` stays on the sync leg by design — a golden-fixture re-emit proves byte stability and journalling it fills the plane with reproduction noise under one repeated key. No meter rides the leg, the fold's cpu being the resource band's one charge.
- Entry: `emit(raw, *, target)` is polymorphic over the outbound `EmitTarget` — a consumer wanting only the wire-contract schema or only the importable stub selects a target, never a second generator; both projections descend the same fold over the same decoded descriptors, so they can never disagree on the field set. Inbound wire stays the producer's one canonical UTF-8 JSON form; no second decode arm exists without a producer emitting it.
- Auto: every refusal on this page resolves ONE `RAISES` anchor, so a subject derives from its leg rather than being spelled at the raise: a `schema_version` the decoder does not carry rails `SCHEMA_VERSION` — the peer pins `Schema = "1"` — never a best-effort decode off a drifted wire shape; a malformed `bundle_key` render rails `BUNDLE_KEY`; a cyclic owner graph rails `OWNER_CYCLE` as the caller-repairable refusal it is, decided on the rail BEFORE the render fence opens, so no `ValueError` funnel spans a body that also raises library `ValueError`s of its own; `drift` proves decode AND emit round-trip byte-stability against the producer-minted `evidence-bundle` `CorpusFixture` in the runtime reproduction corpus, a byte drift railing `DRIFT`.
- Stage: `emit` is a long fold and takes the optional lane tap: `CodegenStage` is its OWN closed roster — decode, topological order, render — beaten through the hub `StageTap`, so a bundle whose render dominates reports where it stands instead of two positions across the whole projection. One cross-fold phase ladder is the refused form.
- Growth: a new wire primitive is one `FieldScalar` member and one `_SCALAR` row the three interpreters absorb with zero extra surface, landed beside the peer's `FieldScalar` row in the same change; a new composite shape is one `FieldDescriptor` case, one `FieldNode` union member, one `_fold` arm, and one constructor field on each interpreter, beside the peer's case and `[JsonDerivedType]` literal; a new inbound wire format re-mints the `WireFormat` axis as one member and one decoder row when a producer emits it; a new output artifact is one `EmitTarget` member and one fold arm; a new refusal is one `FaultRow` anchor in `RAISES` whose coordinates are its declared `slots`; a new fold position is one `CodegenStage` member whose ordinal derives.

```python
import ast
import decimal
import importlib
import re
from collections.abc import Callable, Iterable
from enum import StrEnum
from functools import reduce
from queue import Queue
from typing import Annotated, Final, Literal, assert_never

import msgspec
from beartype import beartype
from beartype.roar import BeartypeCallHintViolation
from beartype.vale import Is
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block, Map
from msgspec import DecodeError, Struct, ValidationError

from rasm.compute.graduation.handoff import EVIDENCE_DOMAIN, ComputeLeg, EvidenceScope, StageTap, evidence_run
from rasm.runtime.identity import ContentKey
from rasm.runtime.faults import FAULT_CONF, TERMINAL, FaultRow, RuntimeRail, boundary, rostered
from rasm.runtime.journal import Actor, Assigned, AuditFact, Fact, Journal, Party, Retain
from rasm.runtime.lanes import PulseFact
from rasm.runtime.receipts import DEFAULT_SCOPE, Provenance, Receipt, ScopeKey

# --- [TYPES] ----------------------------------------------------------------------------

type EmitTarget = Literal["stub", "schema", "both"]
type RawBundle = Annotated[bytes, Is[lambda b: len(b) > 0]]


class CodegenStage(StrEnum):
    DECODED = "decoded"
    ORDERED = "ordered"
    RENDERED = "rendered"


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

_SCHEMA_VERSIONS: Final[frozenset[str]] = frozenset({"1"})

_BUNDLE_KEY: Final[re.Pattern[str]] = re.compile(r"[0-9a-f]{32}")

_BUNDLE_FMT: Final[str] = "graduation-evidence"

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
    members: Annotated[tuple["FieldNode", ...], msgspec.Meta(min_length=1)]


type FieldNode = ScalarField | ArrayField | NestedField | MappingField | OptionalField | UnionField


class OwnerDescriptor(Struct, frozen=True):
    name: str
    fields: tuple[FieldNode, ...]


class EvidenceBundle(Struct, frozen=True, rename="camel"):
    schema_version: str
    owners: tuple[OwnerDescriptor, ...]
    bundle_key: str


class GeneratedModule(Struct, frozen=True):
    schema_version: str
    owner_count: int
    field_count: int
    bundle_key: ContentKey
    source: str = ""
    schema: dict[str, object] = msgspec.field(default_factory=dict)

    @property
    def span_facts(self) -> dict[str, str | int]:
        return {
            "schema_version": self.schema_version,
            "owner_count": self.owner_count,
            "field_count": self.field_count,
            "bundle_key": self.bundle_key.hex,
        }

    def contribute(self) -> Iterable[Receipt]:
        facts: dict[str, object] = dict(self.span_facts)
        return (
            Receipt.of(
                EvidenceScope.CODEGEN.value,
                ("emitted", self.bundle_key.hex, facts),
                key=Some(self.bundle_key),
                provenance=Some(Provenance(consumed=Block.singleton(self.bundle_key), produced=self.bundle_key)),
            ),
        )


class FieldAlgebra[T](Struct, frozen=True):
    scalar: Callable[[FieldScalar], T]
    array: Callable[[T], T]
    optional: Callable[[T], T]
    mapping: Callable[[T, T], T]
    union: Callable[[tuple[T, ...]], T]
    nested: Callable[[str], T]


# --- [TABLES] ---------------------------------------------------------------------------

_JSON: Final[msgspec.json.Decoder[EvidenceBundle]] = msgspec.json.Decoder(type=EvidenceBundle)

DECODE: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.CODEGEN, point="decode", arm="boundary", defect="bundle-decode", retriability=TERMINAL
)
SCHEMA_VERSION: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.CODEGEN, point="version", arm="config", defect="uncarried-schema", retriability=TERMINAL, slots=("version",)
)
BUNDLE_KEY: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.CODEGEN, point="key", arm="boundary", defect="malformed-bundle-key", retriability=TERMINAL, slots=("render",)
)
OWNER_CYCLE: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.CODEGEN, point="order", arm="config", defect="cyclic-owner", retriability=TERMINAL, slots=("owner",)
)
RENDER: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.CODEGEN, point="render", arm="boundary", defect="stub-render", retriability=TERMINAL
)
DRIFT: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.CODEGEN, point="drift", arm="boundary", defect="byte-drift", retriability=TERMINAL, slots=("version",)
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([DECODE, SCHEMA_VERSION, BUNDLE_KEY, OWNER_CYCLE, RENDER, DRIFT]))

_ORDINAL: Final[Map[CodegenStage, int]] = Map.of_seq([(stage, index + 1) for index, stage in enumerate(CodegenStage)])


def _sub(value: str, *elts: ast.expr) -> ast.expr:
    return ast.Subscript(value=ast.Name(id=value, ctx=ast.Load()), slice=ast.Tuple(elts=list(elts), ctx=ast.Load()), ctx=ast.Load())


def _bitor(left: ast.expr, right: ast.expr) -> ast.expr:
    return ast.BinOp(left=left, op=ast.BitOr(), right=right)


_BARE: Final[frozenset[str]] = frozenset({"builtins", "rasm.runtime.identity"})


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

_TYPES: Final[FieldAlgebra[frozenset[type]]] = FieldAlgebra(
    scalar=lambda s: frozenset({_SCALAR[s]}),
    array=lambda e: e,
    optional=lambda e: e,
    mapping=lambda k, v: k | v,
    union=lambda ms: frozenset().union(*ms),
    nested=lambda ref: frozenset(),
)

_REFS: Final[FieldAlgebra[frozenset[str]]] = FieldAlgebra(
    scalar=lambda s: frozenset(),
    array=lambda e: e,
    optional=lambda e: e,
    mapping=lambda k, v: k | v,
    union=lambda ms: frozenset().union(*ms),
    nested=lambda ref: frozenset({ref}),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _bundle_key(render: str) -> RuntimeRail[ContentKey]:
    if _BUNDLE_KEY.fullmatch(render) is None:
        return Error(BUNDLE_KEY.raised(render[:40]))
    return Ok(ContentKey.decoded(value=int(render, 16), fmt=_BUNDLE_FMT))


def _evidence(module: GeneratedModule) -> Block[Fact]:
    return Block.singleton(
        AuditFact(
            action=f"{EVIDENCE_DOMAIN}.codegen",
            actor=Party(kind=Actor.SERVICE, key=EvidenceScope.CODEGEN.value),
            target=Party(kind="bundle", key=module.bundle_key.hex),
            retention=Retain.OPERATIONAL,
            change=(
                Assigned(path="/schema_version", next=module.schema_version),
                Assigned(path="/owner_count", next=str(module.owner_count)),
                Assigned(path="/field_count", next=str(module.field_count)),
            ),
        )
    )


def _staged[T](mark: Option[StageTap], stage: CodegenStage, value: T) -> T:
    mark.map(lambda tap: tap.beat(stage, _ORDINAL[stage]))
    return value


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
    def emit(
        raw: bytes, *, target: EmitTarget = "both", tap: Queue[PulseFact | None] | None = None, composition: ScopeKey = DEFAULT_SCOPE
    ) -> RuntimeRail[GeneratedModule]:
        mark = Option.of_optional(tap).map(lambda queue: StageTap.of(EvidenceScope.CODEGEN, queue, total=Nothing))

        def rail() -> RuntimeRail[GeneratedModule]:
            return (
                boundary(DECODE, lambda: StubCodegen._decode(raw), catch=(ValidationError, DecodeError, BeartypeCallHintViolation))
                .bind(StubCodegen._carried)
                .bind(lambda bundle: _bundle_key(bundle.bundle_key).map(lambda key: (bundle, key)))
                .map(lambda pair: _staged(mark, CodegenStage.DECODED, pair))
                .bind(lambda pair: StubCodegen._ordered(pair[0].owners).map(lambda ordered: (pair[0], pair[1], ordered)))
                .map(lambda triple: _staged(mark, CodegenStage.ORDERED, triple))
                .bind(
                    lambda triple: boundary(
                        RENDER, lambda: StubCodegen._render(triple[0], triple[1], triple[2], target), catch=(TypeError, NameError, AttributeError)
                    )
                )
                .map(lambda module: _staged(mark, CodegenStage.RENDERED, module))
            )

        return evidence_run(
            EvidenceScope.CODEGEN, f"emit.{target}", rail, facts={"target": target, "byte_count": len(raw)},
            composition=composition, stage=mark,
        )

    @staticmethod
    async def emit_async(
        raw: bytes, *, target: EmitTarget = "both", composition: ScopeKey = DEFAULT_SCOPE
    ) -> RuntimeRail[GeneratedModule]:
        match StubCodegen.emit(raw, target=target, composition=composition):
            case Result(tag="ok", ok=module):
                return (await Journal.record(_evidence(module), scope=composition)).map(lambda _landed: module)
            case refused:
                return Error(refused.error)

    @staticmethod
    def drift(golden: bytes, expected: GeneratedModule) -> RuntimeRail[GeneratedModule]:
        pinned = msgspec.json.Encoder(order="deterministic")

        def check(module: GeneratedModule) -> RuntimeRail[GeneratedModule]:
            if pinned.encode(module) == pinned.encode(expected):
                return Ok(module)
            return Error(DRIFT.raised(expected.schema_version))

        return StubCodegen.emit(golden).bind(check)

    @staticmethod
    def _carried(bundle: EvidenceBundle) -> RuntimeRail[EvidenceBundle]:
        if bundle.schema_version in _SCHEMA_VERSIONS:
            return Ok(bundle)
        return Error(SCHEMA_VERSION.raised(bundle.schema_version))

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def _decode(raw: RawBundle) -> EvidenceBundle:
        return _JSON.decode(raw)

    @staticmethod
    def _render(bundle: EvidenceBundle, key: ContentKey, ordered: tuple[OwnerDescriptor, ...], target: EmitTarget) -> GeneratedModule:
        owners = bundle.owners
        return GeneratedModule(
            schema_version=bundle.schema_version,
            owner_count=len(owners),
            field_count=sum(len(owner.fields) for owner in owners),
            bundle_key=key,
            source=StubCodegen._source(owners) if target in ("stub", "both") else "",
            schema=msgspec.json.schema_components(StubCodegen._owner_types(owners, ordered))[1] if target in ("schema", "both") else {},
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
    def _owner_types(owners: tuple[OwnerDescriptor, ...], ordered: tuple[OwnerDescriptor, ...]) -> tuple[type, ...]:
        scalars = StubCodegen._scalars(owners)
        registry: dict[str, object] = {}
        for tp in scalars:
            if tp.__module__ in _BARE:
                registry[tp.__qualname__] = tp
            else:
                head = tp.__module__.split(".", 1)[0]
                registry[head] = importlib.import_module(head)
        for owner in ordered:
            registry[owner.name] = msgspec.defstruct(
                owner.name, [(f.name, ast.unparse(_fold(f, _NODE))) for f in owner.fields], frozen=True, namespace=registry
            )
        return tuple(registry[owner.name] for owner in owners)

    @staticmethod
    def _ordered(owners: tuple[OwnerDescriptor, ...]) -> RuntimeRail[tuple[OwnerDescriptor, ...]]:
        by_name = {owner.name: owner for owner in owners}
        out: dict[str, OwnerDescriptor] = {}
        visiting: set[str] = set()

        def visit(owner: OwnerDescriptor) -> Option[str]:
            if owner.name in out:
                return Nothing
            if owner.name in visiting:
                return Some(owner.name)
            visiting.add(owner.name)
            refs = Block.of_seq(sorted(frozenset().union(*(_fold(field, _REFS) for field in owner.fields))))
            closed = refs.choose(lambda ref: Option.of_optional(by_name.get(ref))).choose(visit).try_head()
            visiting.discard(owner.name)
            out[owner.name] = owner
            return closed

        match Block.of_seq(owners).choose(visit).try_head():
            case Option(tag="some", some=cycle):
                return Error(OWNER_CYCLE.raised(cycle))
            case _:
                return Ok(tuple(out.values()))
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
