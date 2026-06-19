# [PYTHON_SHAPES]

Every Python shape must prove its lifecycle role, invariant owner, projection relation, and collapse test. The governing path is `Raw -> Payload -> Canonical owner -> Rail/effect -> Projection -> Egress`; choose role first, owner second, projection third. Stage names, function names, and return types agree with their stage: materialization returns the canonical owner, projection returns the boundary view, egress returns encoded or foreign material.

Admit payloads, schemas, models, structs, dataclasses, rich classes, enums, protocols, rails, immutable evidence, projections, and replacements only when they change implementation law. Reject package-branded internal layers, package-named model families, research taxonomy, and shapes added to reduce local line count or hide weak ownership.

## [01]-[SHAPE_LIFECYCLE]

Choose the lifecycle role before adding an owner, construct, rail, or projection.

[ROLE_INDEX]:

| [INDEX] | [ROLE]          | [POSITION]             | [ACCEPTS]             | [EMITS]            | [OWNER]          | [REJECT]             |
| :-----: | :-------------- | :--------------------- | :-------------------- | :----------------- | :--------------- | :------------------- |
|  [01]   | Raw ingress     | before admission       | external material     | payload or ingress | boundary adapter | domain raw           |
|  [02]   | Typed payload   | before materialization | static dictionary law | ingress or owner   | payload contract | interior payload     |
|  [03]   | Canonical owner | domain entry           | admitted values       | owner rail or view | domain concept   | parallel DTO         |
|  [04]   | Rail/effect     | operation result       | owner or fault        | value or error     | operation edge   | exception flow       |
|  [05]   | Projection      | boundary view          | canonical owner       | wire or row shape  | adapter surface  | projection authority |
|  [06]   | Egress          | final handoff          | projection            | encoded material   | codec writer     | late correctness     |

[LIFECYCLE_ROLES]:
- Raw ingress accepts `bytes`, `str`, `Mapping[str, object]`, CLI, env, or provider material; rail roles emit `Option[T]` for non-failing absence and `Result[T, E]` for typed fallibility.
- `None`-failure and exception flow never cross domain logic; egress never becomes the only correctness proof.

[HANDOFF_LAW]:
- Skip rule: stage-skipping requires an owning boundary reason.
- Erasure rule: erased `object` handoffs stop at boundaries.
- Interior rule: domain interiors accept canonical owners, closed members, owner rails, and admitted ports.
- Rail rule: validation and codec exceptions map to `Result` at the owning boundary.
- Composition rule: composed pipelines preserve visible admission, materialization, projection, and egress surfaces.

[LIFECYCLE_FLOW]:

```python conceptual
from dataclasses import dataclass
from typing import Literal, Self

import msgspec
from expression import Error, Ok, Result
from pydantic import BaseModel, ConfigDict, Field, ValidationError

type ShapeFault = Literal["<empty-key>", "<invalid-payload>", "<invalid-egress>"]


class ShapeIngress(BaseModel):
    model_config = ConfigDict(frozen=True, extra="forbid", strict=True)
    key: str = Field(validation_alias="source_key")
    note: str | None = Field(default=None, validation_alias="source_note")


@dataclass(frozen=True, slots=True, kw_only=True)
class Shape:
    key: str
    note: str | None = None

    @classmethod
    def admit(cls, ingress: ShapeIngress, /) -> Result[Self, ShapeFault]:
        return Error("<empty-key>") if ingress.key == "" else Ok(cls(key=ingress.key, note=ingress.note))


class ShapeWire(msgspec.Struct, frozen=True, omit_defaults=True, rename={"key": "wire_key", "note": "wire_note"}):
    key: str
    note: str | None = None


SHAPE_ENCODER = msgspec.json.Encoder()


def admitted(raw: object, /) -> Result[Shape, ShapeFault]:
    try:
        ingress = ShapeIngress.model_validate(raw)
    except ValidationError:
        return Error("<invalid-payload>")
    return Shape.admit(ingress)


def projected(shape: Shape, /) -> ShapeWire:
    # convert matches source attributes by field name, so the wire owner keeps canonical names and renames only at the encoded edge
    return msgspec.convert(shape, ShapeWire, from_attributes=True)


def egressed(wire: ShapeWire, /) -> Result[bytes, ShapeFault]:
    try:
        return Ok(SHAPE_ENCODER.encode(wire))
    except msgspec.EncodeError:
        return Error("<invalid-egress>")


def delivered(raw: object, /) -> Result[bytes, ShapeFault]:
    return admitted(raw).map(projected).bind(egressed)
```

## [02]-[OWNER_CHOOSER]

Choose the invariant owner before choosing a package-backed model, wrapper, protocol, rail, enum, or immutable collection. Five discriminants make the choice mechanical, and every misplaced shape traces to one mis-answered discriminant: admission (is the material trusted, or does it cross an untrusted edge), identity regime (is equality by value, by tag, by key, or by reference), variant arity (one shape, a closed family, or an open extension set), payload timing (is the shape fixed at definition or admitted at runtime), and openness (is the family closed to the program, semi-closed at a versioned wire, or open to foreign code). The OWNER_INDEX rows below are keyed on the answer to these discriminants.

[OWNER_INDEX]:

| [INDEX] | [DECISION]             | [DISCRIMINANT]           | [OWNER]              | [CHOOSE]                   | [REJECT]              |
| :-----: | :--------------------- | :----------------------- | :------------------- | :------------------------- | :-------------------- |
|  [01]   | static keys            | untrusted, def-time      | `[BOUNDARY_SHAPES]`  | `TypedDict`                | `dict[str, object]`   |
|  [02]   | untrusted admission    | untrusted, runtime       | `[BOUNDARY_SHAPES]`  | Pydantic                   | interior revalidation |
|  [03]   | wire or row            | trusted, fixed layout    | `[BOUNDARY_SHAPES]`  | `msgspec.Struct`           | domain wire owner     |
|  [04]   | compact invariant      | trusted, value-equal     | `[DOMAIN_SHAPES]`    | frozen dataclass           | field rename class    |
|  [05]   | durable schema or wire | trusted, schema-bound    | `[DOMAIN_SHAPES]`    | frozen Pydantic or msgspec | second owner          |
|  [06]   | behavior-dense owner   | trusted, behavior > data | `[DOMAIN_SHAPES]`    | rich class                 | forwarding helper     |
|  [07]   | token or absence state | closed, tag identity     | `[TOKEN_STATE_PORT]` | vocabulary, sentinel, rail | duplicate carriers    |
|  [08]   | immutable evidence     | trusted, key/order id    | `[TOKEN_STATE_PORT]` | tuple, `frozendict`, `Map` | mutable staging       |
|  [09]   | replaceable capability | open, structural id      | `[TOKEN_STATE_PORT]` | `Protocol`                 | single implementation |

[BOUNDARY_SHAPES]:
- `TypedDict` admits static key presence, closure (`closed=True`), `extra_items` extension, and `ReadOnly` evidence before materialization.
- Pydantic admits untrusted ingress, settings, alias policy, discriminated variants, and rich errors at the boundary.
- `msgspec.Struct` admits deterministic wire, cache, row, and high-volume serialization layout.
- Collapse: payloads materialize into ingress or canonical owners, admission schemas promote once, and wire projections derive from canonical owners.

[DOMAIN_SHAPES]:
- Frozen dataclass owns a compact invariant; frozen Pydantic only when compiled validation, computed fields, and schema are the contract; msgspec only when fixed wire layout is policy-canonical; rich class when construction law, folds, transitions, evidence, or projection methods exceed declarative fields.
- Collapse: absorb one-field wrappers, field-rename classes, sibling factories, and variant shells into the deeper owner.

[TOKEN_STATE_PORT]:
- Vocabulary owner: use one `StrEnum` for runtime token identity or one `Literal` set for static token proof; use `Flag` or `IntFlag` only when bit composition is the contract.
- Absence/failure owner: use `sentinel()` for caller omission, explicit members for dispatch-changing state, `Option[T]` for computed absence, and `Result[T, E]` for typed fallibility.
- Immutable evidence owner: use `frozendict` for immutable map rows, `Map` for persistent updates, `tuple` or `Block` for ordered evidence, and `frozenset` for membership facts.
- Structural port: use `Protocol` only when independent implementers provide one replaceable operation family; otherwise use `Callable`, a concrete owner, or a closed family.

[OWNER_REJECTS]:
- Package-branded internal layers, parallel DTOs, and one-field wrappers without independent invariants.
- `None` failure, `Option` hiding errors, mutable staging after materialization, and protocols repairing weak ownership.

## [03]-[PAYLOAD_AND_MATERIALIZATION]

[TYPED_PAYLOADS]:
- Payloads are static dictionary contracts before materialization.
- Declare openness with `closed=True` or `extra_items=T`; `extra_items` is the only admitted open band and carries the bare element type, because the materialized `frozendict` owner—not `ReadOnly`, which no runtime validator honors inside a band—enforces extension immutability.
- Declare per-key presence with `Required[T]` and `NotRequired[T]`.
- Declare static per-key read-only evidence with `ReadOnly[T]`.
- Use `Unpack[Payload]` at root keyword entrypoints, never forwarded through domain interiors.
- Use `Callable[[Unpack[Payload]], R]` for keyword-callable values.
- Fold `extra_items` extension bands into `frozendict` or tuple evidence at promotion.
- Reject total/non-total mirror shapes, homogeneous `**kwargs`, forwarded payload kwargs, and payload imports in domain interiors.

```python conceptual
from dataclasses import dataclass, field
from typing import Literal, NotRequired, ReadOnly, Required, Self, TypedDict, Unpack

from expression import Error, Ok, Result
from builtins import frozendict
from pydantic import TypeAdapter, ValidationError

type ShapeFault = Literal["<empty-key>", "<invalid-payload>"]


class ShapePayload(TypedDict, extra_items=str):
    key: Required[ReadOnly[str]]
    note: NotRequired[ReadOnly[str | None]]


SHAPE_PAYLOAD = TypeAdapter(ShapePayload)
_PAYLOAD_KEYS: frozenset[str] = ShapePayload.__required_keys__ | ShapePayload.__optional_keys__


@dataclass(frozen=True, slots=True, kw_only=True)
class Shape:
    key: str
    note: str | None = None
    extensions: frozendict[str, str] = field(default_factory=frozendict)

    @classmethod
    def materialized(cls, payload: ShapePayload, /) -> Result[Self, ShapeFault]:
        return (
            Error("<empty-key>")
            if payload["key"] == ""
            else Ok(
                cls(key=payload["key"], note=payload.get("note"), extensions=frozendict({k: v for k, v in payload.items() if k not in _PAYLOAD_KEYS}))
            )
        )


def accepted(**raw: Unpack[ShapePayload]) -> Result[Shape, ShapeFault]:
    try:
        return Shape.materialized(SHAPE_PAYLOAD.validate_python(raw))
    except ValidationError:
        return Error("<invalid-payload>")
```

[PYDANTIC_ADMISSION]:
- Pydantic owns untrusted ingress, settings, alias policy, discriminated admission, `Annotated` validation, JSON Schema-rich contracts, and mapped `ValidationError` surfaces.
- Use module-level `TypeAdapter` for unions, payloads, scalars, and containers.
- Use frozen and closed ingress models for closed contracts: `ConfigDict(frozen=True, extra="forbid")`.
- Use discriminators for variant admission; do not rely on ambiguous union order.
- Use validators for admission and normalization only; no I/O, registry mutation, or domain orchestration inside validators.
- Reject per-request `TypeAdapter`, `model_construct` on untrusted input, `model_dump` key surgery, and second Pydantic passes in domain interiors.

[MATERIALIZATION]:
- Validate raw material once at the boundary.
- Normalize wire names, aliases, and foreign tokens before canonical construction.
- Promote through one named adapter or composition-root gate.
- Construction that can fail returns `Result[Owner, E]`.
- Domain logic receives owners, closed family members, or rails over owners.
- Egress projects from canonical through explicit adapter construction, `msgspec.convert`, or an owner-approved projection.

## [04]-[CANONICAL_OWNERS]

[CANONICAL_OWNER_LAW]:
- Definition: the canonical owner is the first durable frozen shape accepted by domain logic.
- Invariants: it owns domain invariants, lifecycle transitions, behavior, folds, and projections that do not belong to a boundary.
- Owner-type selection follows [OWNER_INDEX] rows [4]-[6]; a canonical owner never imports a boundary engine unless that engine is itself the durable owner.
- Family owner: use one closed family owner when mutually exclusive shapes represent one concept.

```python conceptual
from collections.abc import Callable
from dataclasses import dataclass, field, replace
from typing import Literal, Self

from expression import Error, Nothing, Ok, Option, Result, Some

type ShapeFault = Literal["<empty-key>", "<empty-tag>", "<duplicate-tag>"]


@dataclass(frozen=True, slots=True, kw_only=True)
class ShapeView:
    key: str
    note: Option[str]
    tags: tuple[str, ...]


@dataclass(frozen=True, slots=True, kw_only=True)
class Shape:
    key: str
    note: str | None = None
    tags: tuple[str, ...] = field(default_factory=tuple)

    @classmethod
    def admit(cls, *, key: str, note: str | None = None) -> Result[Self, ShapeFault]:
        return Error("<empty-key>") if key == "" else Ok(cls(key=key, note=note))

    def tagged(self, tag: str, /) -> Result[Self, ShapeFault]:
        return Error("<empty-tag>") if tag == "" else Error("<duplicate-tag>") if tag in self.tags else Ok(replace(self, tags=(*self.tags, tag)))

    def fold_note[T](self, some: Callable[[str], T], none: Callable[[], T], /) -> T:
        match self.note:
            case str() as note:
                return some(note)
            case None:
                return none()

    def viewed(self, /) -> ShapeView:
        return ShapeView(key=self.key, note=self.fold_note(Some, lambda: Nothing), tags=self.tags)
```

[GRADUATION_REJECTS]:
- Graduate repeated primitive validation to a vocabulary, constrained scalar, or owner field.
- Graduate repeated field bundles to one model, struct, dataclass, or rich owner.
- Graduate three or more sibling factories, models, or dispatch arms to a closed family or polymorphic owner.
- Graduate mutable update law to immutable replacement.
- Graduate stable wire or persistence concern to an egress projection, not a second domain owner.
- Reject mirrored validation/domain/wire hierarchies, validator side effects, and tag-only shape families.

## [05]-[VOCABULARY_ABSENCE_AND_VARIANTS]

[VOCABULARY]:
- One vocabulary owner feeds ingress discriminants, canonical tags, wire tags, registry rows, schema enum arms, and proof samples.
- Use `StrEnum` for runtime token identity, iteration, registry keys, settings, CLI, or wire values.
- Use `Literal` when static proof is sufficient and runtime vocabulary behavior is not needed.
- Use verified `Flag` only when bit composition is the contract.
- Do not route on `.value` strings in domain code.
- Do not duplicate token bands across enums, literals, schemas, fixtures, and handler maps.

[ABSENCE]:
- Omitted key is `NotRequired[T]`.
- Valid null is `T | None` only when `None` is a domain or wire value.
- Caller omission or inherited default is a module-global `sentinel("NAME")`, compared with `is`, never serialized.
- Wire unset/null is a boundary posture collapsed at the owning adapter.
- Domain null, inherit, unknown, or withheld state is an explicit enum member or variant when it changes dispatch.
- `Option.none` is non-failing computed absence after admission.
- `Result.Error` is failure.
- Reject `None` for failure, sentinel on wire structs, `Option` hiding validation errors, bool flags splitting one option shape, and three-way unions without a named contract.

[ABSENCE_STATES]:

```python conceptual
from builtins import sentinel
from dataclasses import dataclass, field
from typing import Literal, NotRequired, ReadOnly, TypedDict, assert_never

from expression import Error, Nothing, Ok, Option, Result, Some
from pydantic import TypeAdapter, ValidationError

MISSING = sentinel("MISSING")
type ShapeFault = Literal["<invalid-patch>", "<invalid-note>"]


class ShapePatch(TypedDict, total=False, closed=True):
    note: NotRequired[ReadOnly[str | None]]


@dataclass(frozen=True, slots=True)
class Omitted: ...


@dataclass(frozen=True, slots=True)
class Null: ...


@dataclass(frozen=True, slots=True)
class Provided:
    text: str


type Note = Omitted | Null | Provided


def admitted_note(row: ShapePatch, /) -> Result[Note, ShapeFault]:
    match row.get("note", MISSING):
        case value if value is MISSING:
            return Ok(Omitted())
        case None:
            return Ok(Null())
        case "":
            return Error("<invalid-note>")
        case str() as text:
            return Ok(Provided(text))


def selected(note: Note, fallback: Option[str] = Nothing, /) -> Option[str]:
    match note:
        case Provided(text):
            return Some(text)
        case Null():
            return Nothing
        case Omitted():
            return fallback
        case unreachable:
            assert_never(unreachable)


SHAPE_PATCH = TypeAdapter(ShapePatch)


@dataclass(frozen=True, slots=True, kw_only=True)
class Shape:
    note: Note = field(default_factory=Omitted)


def admitted(raw: object, /) -> Result[Shape, ShapeFault]:
    try:
        patch = SHAPE_PATCH.validate_python(raw)
    except ValidationError:
        return Error("<invalid-patch>")
    return admitted_note(patch).map(lambda note: Shape(note=note))
```

[FAMILIES]:
- Closed family: one owner namespace, one vocabulary, distinct runtime members or one tagged owner for shallow variants, total `match`, and `assert_never`.
- Encode as a union of distinct frozen records (structural `match`) when variants carry different fields or own different lifecycle states; encode as one tagged owner (`StrEnum` discriminant plus `folded[T]`) when shallow variants share one field set under a single tag.
- Semi-closed family: closed core plus typed extension band or explicit extension variant.
- Open family: only when foreign or plugin code can add members without editing the owner.
- Use `TypeIs` only when a reusable predicate proves exact membership, not filtered validity.
- Match concrete members or discriminated variants in total folds; guards may narrow before a fold but must not be the exhaustiveness proof.
- Use `@disjoint_base`, tagged generics, discriminated Pydantic unions, msgspec tagged unions, or expression tagged unions by lifecycle role.
- Reject optional-field variant bags, string dispatch, `singledispatch` for owned closed vocabularies, protocol-per-variant, catch-all default arms, and foreign token spelling inside canonical owners.

[CLOSED_FAMILY]:

```python conceptual
from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from typing import Self, assert_never

from expression import Nothing, Option, Some


class Variant(StrEnum):
    PRIMARY = "<value-a>"
    SECONDARY = "<value-b>"


@dataclass(frozen=True, slots=True, kw_only=True)
class Shape:
    kind: Variant
    key: str
    note: Option[str] = Nothing

    @classmethod
    def primary(cls, key: str, /) -> Self:
        return cls(kind=Variant.PRIMARY, key=key)

    @classmethod
    def secondary(cls, key: str, note: str | None = None, /) -> Self:
        return cls(kind=Variant.SECONDARY, key=key, note=Nothing if note is None else Some(note))

    def folded[T](self, primary: Callable[[str], T], secondary: Callable[[str, Option[str]], T], /) -> T:
        match self.kind:
            case Variant.PRIMARY:
                return primary(self.key)
            case Variant.SECONDARY:
                return secondary(self.key, self.note)
            case unreachable:
                assert_never(unreachable)
```

## [06]-[PROJECTIONS_PORTS_AND_BOUNDARIES]

[BOUNDARY_PROJECTIONS]:
- Ingress direction: payloads, settings, CLI slices, provider material, and ingress models admit foreign material inward.
- Egress direction: wire structs, persistence rows, receipts, schema views, and export views derive outward from canonical owners.
- Adapter ownership: foreign boundaries remap provider names, token vocabularies, cardinality, discriminants, aliases, and omitted fields before canonical entry.
- Correspondence: field mapping belongs in adapter tables, explicit constructors, `msgspec.convert`, or schema-owned aliases.
- Reject: projection-to-projection authority, codec engines in canonical owners, scattered `model_dump` key pops, model-per-provider interiors, provider-shaped domain fields, and canonical `schema_version` branches that belong to read-boundary migration.

[DISPATCH_TABLE]:
- Drive vocabulary routing, transition selection, and token translation with a `frozendict` table, not conditional chains.
- Declare one primary `frozendict[K, tuple[...]]` and derive secondary maps from its values; never re-declare the correspondence.
- `msgspec.convert(owner, Wire, from_attributes=True)` projects a pure field rename when `Wire` keeps canonical attribute names and renames at the encoded boundary; explicit construction or an adapter table owns any projection that transforms a value.

```python conceptual
from dataclasses import dataclass
from enum import StrEnum
from typing import Literal, ReadOnly, Required, Self, TypedDict

import msgspec
from expression import Error, Ok, Result
from builtins import frozendict
from pydantic import TypeAdapter, ValidationError


class Variant(StrEnum):
    PRIMARY = "<value-a>"
    SECONDARY = "<value-b>"


type WireKind = Literal["<wire-a>", "<wire-b>"]
type ShapeFault = Literal["<empty-key>", "<invalid-row>", "<unknown-kind>"]


class ForeignRow(TypedDict, closed=True):
    foreign_kind: Required[ReadOnly[str]]
    foreign_key: Required[ReadOnly[str]]


@dataclass(frozen=True, slots=True, kw_only=True)
class Shape:
    kind: Variant
    key: str

    @classmethod
    def admit(cls, *, kind: Variant, key: str) -> Result[Self, ShapeFault]:
        return Error("<empty-key>") if key == "" else Ok(cls(kind=kind, key=key))


class ShapeWire(msgspec.Struct, frozen=True, forbid_unknown_fields=True):
    wire_kind: WireKind
    wire_key: str


KIND_ROUTE: frozendict[str, tuple[Variant, WireKind]] = frozendict({
    "<foreign-a>": (Variant.PRIMARY, "<wire-a>"),
    "<foreign-b>": (Variant.SECONDARY, "<wire-b>"),
})
KIND_OUT: frozendict[Variant, WireKind] = frozendict({kind: wire for kind, wire in KIND_ROUTE.values()})

FOREIGN_ROW = TypeAdapter(ForeignRow)


def materialized(raw: object, /) -> Result[Shape, ShapeFault]:
    try:
        row = FOREIGN_ROW.validate_python(raw)
    except ValidationError:
        return Error("<invalid-row>")
    route = KIND_ROUTE.get(row["foreign_kind"])
    return Error("<unknown-kind>") if route is None else Shape.admit(kind=route[0], key=row["foreign_key"])


def projected(shape: Shape, /) -> ShapeWire:
    return ShapeWire(wire_kind=KIND_OUT[shape.kind], wire_key=shape.key)


def translated(raw: object, /) -> Result[ShapeWire, ShapeFault]:
    return materialized(raw).map(projected)
```

[STRUCTURAL_PORTS]:
- Protocols are capability seams, not data shapes.
- Admit `Protocol` only when multiple independent implementers satisfy one replaceable operation family without inheritance.
- Keep method sets minimal and capability-named.
- Key scope maps by `type[Port]` when a port is injected.
- Use `get_protocol_members` for registration or proof, not per-request validation.
- Use `@runtime_checkable` only at real dynamic gates.
- Use `TypeIs[Port]` when semantic narrowing exceeds member presence.
- Port methods return `Result` or owned fault unions for expected failure.
- Reject one-method callback protocols where `Callable` works, protocol unions simulating closed variants, protocols as wire or ingress fields, and protocols used as weak type repair.

```python conceptual
from dataclasses import dataclass, field, replace
from typing import Literal, Protocol, Self

from expression import Error, Ok, Result
from builtins import frozendict

type ShapeFault = Literal["<missing>"]


@dataclass(frozen=True, slots=True, kw_only=True)
class Shape:
    key: str
    value: str


class ShapeStore(Protocol):
    def loaded(self, key: str, /) -> Result[Shape, ShapeFault]: ...
    def stored(self, shape: Shape, /) -> Result[Self, ShapeFault]: ...


@dataclass(frozen=True, slots=True, kw_only=True)
class MemoryStore:
    rows: frozendict[str, Shape] = field(default_factory=frozendict)

    def loaded(self, key: str, /) -> Result[Shape, ShapeFault]:
        return Ok(self.rows[key]) if key in self.rows else Error("<missing>")

    def stored(self, shape: Shape, /) -> Result[Self, ShapeFault]:
        return Ok(replace(self, rows=self.rows | {shape.key: shape}))


@dataclass(frozen=True, slots=True)
class SnapshotStore:
    row: Shape

    def loaded(self, key: str, /) -> Result[Shape, ShapeFault]:
        return Ok(self.row) if key == self.row.key else Error("<missing>")

    def stored(self, shape: Shape, /) -> Result[Self, ShapeFault]:
        return Ok(replace(self, row=shape)) if shape.key == self.row.key else Error("<missing>")


def refreshed[S: ShapeStore](store: S, key: str, value: str, /) -> Result[S, ShapeFault]:
    return store.loaded(key).map(lambda shape: replace(shape, value=value)).bind(store.stored)
```

## [07]-[IMMUTABLE_REPLACEMENT]

[IMMUTABLE_REPLACEMENT_LAW]:
- Owner state: durable owners are frozen after materialization, and durable collections are `tuple`, `frozenset`, `frozendict`, `Map`, `Block`, or another admitted immutable owner.
- Transition shape: state change returns `Self`, `Result[Self, E]`, or a closed successor union; mutation is not a transition.
- Trusted swap: use the owner kernel directly for same-process trusted shallow swaps, such as `copy.replace`, `model_copy(update=...)`, `msgspec.structs.replace`, `frozendict` union, or persistent `Map` and `Block` combinators.
- Revalidated delta: validate a closed patch at the boundary before replacement when the delta is untrusted, computed, wire-sourced, cross-boundary, or requires full schema semantics.
- Lane separation: trusted replacement lives on the owner, while untrusted replacement starts from a patch payload and returns through the owner rail.
- Deep transition: isolate or rebuild nested identity when shallow replacement would replay mutable, cached, or session-owned state.
- Patch payload: represent patch contracts as closed `TypedDict` shapes with `NotRequired` update fields and `ReadOnly` identity or version fields; patch payloads stop at root materialization and become replacement expressions.
- Alias boundary: normalize aliases before replacement and never use alias keys inside owner replacement.
- Reject: mutable fields on frozen owners unless promoted or isolated, direct `__replace__`, mutate-then-freeze, shallow nested dict updates, cached-session replay by shallow replace, mutable staging maps, and `MappingProxyType` as durable immutability.

[REPLACEMENT_FLOW]:

```python conceptual
from copy import replace
from dataclasses import dataclass
from typing import Literal, NotRequired, ReadOnly, Required, Self, TypedDict

from expression import Error, Ok, Result
from pydantic import TypeAdapter, ValidationError

type ShapeFault = Literal["<empty-key>", "<invalid-patch>", "<stale-version>"]


class ShapePatch(TypedDict, closed=True):
    expected_version: Required[ReadOnly[int]]
    key: NotRequired[ReadOnly[str]]
    note: NotRequired[ReadOnly[str | None]]


@dataclass(frozen=True, slots=True, kw_only=True)
class Shape:
    key: str
    note: str | None = None
    version: int = 1

    def advanced(self, *, expected_version: int, key: str, note: str | None) -> Result[Self, ShapeFault]:
        return (
            Error("<stale-version>")
            if expected_version != self.version
            else Error("<empty-key>")
            if key == ""
            else Ok(replace(self, key=key, note=note, version=self.version + 1))
        )


SHAPE_PATCH = TypeAdapter(ShapePatch)


def patched(shape: Shape, raw: object, /) -> Result[Shape, ShapeFault]:
    try:
        patch = SHAPE_PATCH.validate_python(raw)
    except ValidationError:
        return Error("<invalid-patch>")

    return shape.advanced(expected_version=patch["expected_version"], key=patch.get("key", shape.key), note=patch.get("note", shape.note))
```
