# [PYTHON_DATA_SHAPES]

Every Python shape must prove its lifecycle role, invariant owner, projection relation, and collapse test. The governing path is `Raw -> Payload -> Canonical owner -> Rail/effect -> Projection -> Egress`; choose the role first, the owner second, and the projection third.

Admit payloads, schemas, models, structs, dataclasses, rich classes, enums, protocols, rails, immutable evidence, projections, and replacements only when they change implementation law. Reject package-branded internal layers, package-named model families, research taxonomy, and shapes added to reduce local line count or hide weak ownership.

## [1]-[SHAPE_LIFECYCLE]

Choose the lifecycle role before adding an owner, construct, rail, or projection.

[ROLE_INDEX]:

| [INDEX] | [ROLE]          | [POSITION]             | [ACCEPTS]             | [EMITS]            | [OWNER]          | [REJECT]             |
| :-----: | :-------------- | :--------------------- | :-------------------- | :----------------- | :--------------- | :------------------- |
|   [1]   | Raw ingress     | before admission       | external material     | payload or ingress | boundary adapter | domain raw           |
|   [2]   | Typed payload   | before materialization | static dictionary law | ingress or owner   | payload contract | interior payload     |
|   [3]   | Canonical owner | domain entry           | admitted values       | owner rail or view | domain concept   | parallel DTO         |
|   [4]   | Rail/effect     | operation result       | owner or fault        | value or error     | operation edge   | exception flow       |
|   [5]   | Projection      | boundary view          | canonical owner       | wire or row shape  | adapter surface  | projection authority |
|   [6]   | Egress          | final handoff          | projection            | encoded material   | codec writer     | late correctness     |

[LIFECYCLE_ROLES]:
- Raw ingress: external `bytes`, `str`, `Mapping[str, object]`, CLI, env, or provider material stops at a boundary adapter and emits a typed payload or ingress model.
- Typed payload: static dictionary, patch, event, or keyword evidence stops at root admission or an adapter gate and must not enter domain interiors.
- Canonical owner: admitted values become the first durable domain shape; this owner carries invariants, transitions, owner rails, successors, and owner-approved projections.
- Rail or effect: absent or fallible operation results emit `Option[T]` or `Result[T, E]`; `None` failure and exception control flow do not cross domain logic.
- Projection: adapter, configuration, egress, wire, row, receipt, or export views derive from canonical owners or map foreign material inward before canonical entry.
- Egress: final handoff encodes a projection into bytes, JSON-safe material, persistence rows, or foreign handoff; egress never becomes the only domain-correctness proof.

[HANDOFF_LAW]:
- Handoff: each stage emits one typed artifact accepted by the next stage.
- Skip rule: stage-skipping requires an owning boundary reason.
- Erasure rule: erased `object` handoffs stop at boundaries.
- Interior rule: domain interiors accept canonical owners, closed members, owner rails, and admitted ports.

[LIFECYCLE_FLOW]:

```python conceptual
from dataclasses import dataclass
from enum import StrEnum
from typing import Literal, NotRequired, ReadOnly, Required, TypedDict
import msgspec
from expression import Error, Ok, Result
from pydantic import BaseModel, ConfigDict, TypeAdapter

class Variant(StrEnum):
    PRIMARY = "<value-a>"
    SECONDARY = "<value-b>"

type ShapeFault = Literal["<empty-key>"]

class ShapePayload(TypedDict, closed=True):
    kind: NotRequired[ReadOnly[Variant]]
    key: Required[ReadOnly[str]]
    note: NotRequired[ReadOnly[str | None]]

class ShapeIngress(BaseModel):
    model_config = ConfigDict(frozen=True, extra="forbid", strict=True)
    kind: Variant = Variant.PRIMARY
    key: str
    note: str | None = None

@dataclass(frozen=True, slots=True, kw_only=True)
class Shape:
    kind: Variant
    key: str
    note: str | None
    @classmethod
    def admit(cls, ingress: ShapeIngress, /) -> Result["Shape", ShapeFault]:
        return (
            Ok(cls(kind=ingress.kind, key=ingress.key, note=ingress.note))
            if ingress.key
            else Error("<empty-key>")
        )

class ShapeWire(msgspec.Struct, frozen=True, forbid_unknown_fields=True, omit_defaults=True):
    kind: Variant
    key: str
    note: str | None = None

SHAPE_INGRESS = TypeAdapter(ShapeIngress)

def materialized(row: ShapePayload, /) -> Result[ShapeWire, ShapeFault]:
    return Shape.admit(SHAPE_INGRESS.validate_python(row)).map(
        lambda shape: ShapeWire(shape.kind, shape.key, shape.note)
    )
```

## [2]-[OWNER_CHOOSER]

Choose the invariant owner before choosing a package-backed model, wrapper, protocol, rail, enum, or immutable collection.

[OWNER_INDEX]:

| [INDEX] | [DECISION]               | [OWNER]              | [CHOOSE]                    | [REJECT]              |
| :-----: | :----------------------- | :------------------- | :-------------------------- | :-------------------- |
|   [1]   | static keys              | `[BOUNDARY_SHAPES]`  | `TypedDict`                 | `dict[str, object]`   |
|   [2]   | untrusted admission      | `[BOUNDARY_SHAPES]`  | Pydantic                    | interior revalidation |
|   [3]   | wire or row              | `[BOUNDARY_SHAPES]`  | `msgspec.Struct`            | domain wire owner     |
|   [4]   | compact invariant        | `[DOMAIN_SHAPES]`    | frozen dataclass            | field rename class    |
|   [5]   | durable schema or wire   | `[DOMAIN_SHAPES]`    | frozen Pydantic or msgspec  | second owner          |
|   [6]   | behavior-dense owner     | `[DOMAIN_SHAPES]`    | rich class                  | forwarding helper     |
|   [7]   | token or absence state   | `[TOKEN_STATE_PORT]` | vocabulary, sentinel, rail  | duplicate carriers    |
|   [8]   | immutable evidence       | `[TOKEN_STATE_PORT]` | tuple, `frozendict`, `Map`  | mutable staging       |
|   [9]   | replaceable capability   | `[TOKEN_STATE_PORT]` | `Protocol`                  | single implementation |

[BOUNDARY_SHAPES]:
- Payload contract: use `TypedDict` when static key presence, closure, extension, or read-only evidence matters before materialization; reject runtime validation, behavior, serialization, and domain truth.
- Admission schema: use Pydantic when untrusted ingress, settings, alias policy, schema admission, or rich errors are boundary-visible; reject hot wire paths, package-branded DTOs, and second domain passes.
- Wire projection: use `msgspec.Struct` when deterministic wire, cache, row, or high-volume serialization layout is the boundary contract; reject rich domain behavior and second invariant owners.
- Collapse: payloads materialize into ingress or canonical owners, admission schemas promote once, and wire projections derive from canonical owners.

[DOMAIN_SHAPES]:
- Canonical record: use a frozen dataclass when validation is complete, behavior is compact, and schema is not the durable owner.
- Schema owner: use frozen Pydantic as canonical only when compiled validation, schema, computed fields, and frozen semantics are the domain contract.
- Wire owner: use msgspec as canonical only when fixed wire layout is policy-canonical.
- Rich owner: use a rich class when construction law, folds, transitions, evidence, or projection methods exceed declarative fields.
- Collapse: absorb one-field wrappers, field-rename classes, sibling factories, and variant shells into the deeper owner.

[TOKEN_STATE_PORT]:
- Vocabulary owner: use one `StrEnum` for runtime token identity or one `Literal` set for static token proof; use `Flag` or `IntFlag` only when bit composition is the contract.
- Absence/failure owner: use `sentinel()` for caller omission, explicit members for dispatch-changing state, `Option[T]` for computed absence, and `Result[T, E]` for typed fallibility.
- Immutable evidence owner: use `frozendict` for immutable map rows, `Map` for persistent updates, `tuple` or `Block` for ordered evidence, and `frozenset` for membership facts.
- Structural port: use `Protocol` only when independent implementers provide one replaceable operation family; otherwise use `Callable`, a concrete owner, or a closed family.

[OWNER_REJECTS]:
- Package-branded internal shape layers.
- One-field wrappers without independent invariants.
- Duplicate enum, literal, string, handler, or fixture token carriers.
- `None` failure, `Option` hiding errors, and mutable staging after materialization.
- Protocols used for variant identity, callback shells, or weak model repair.

## [3]-[PAYLOAD_AND_MATERIALIZATION]

[TYPED_PAYLOADS]:
- Payloads are static dictionary contracts before materialization.
- Declare openness with `closed=True`, `extra_items=T`, or an intentional default-open posture.
- Declare per-key presence with `Required[T]` and `NotRequired[T]`.
- Declare static read-only evidence with `ReadOnly[T]`.
- Use `Unpack[Payload]` at root keyword entrypoints.
- Use `Callable[[Unpack[Payload]], R]` for keyword-callable values.
- Fold `extra_items` extension bands into `frozendict` or tuple evidence at promotion.
- Reject total/non-total mirror shapes, homogeneous `**kwargs`, forwarded payload kwargs, and payload imports in domain interiors.

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

## [4]-[CANONICAL_OWNERS]

[CANONICAL_OWNER_LAW]:
- Definition: the canonical owner is the first durable frozen shape accepted by domain logic.
- Invariants: it owns domain invariants, lifecycle transitions, behavior, folds, and projections that do not belong to a boundary.
- Compact owner: use a frozen dataclass when validation is complete and behavior is small.
- Schema owner: use frozen Pydantic only when schema behavior is the durable domain contract.
- Wire owner: use msgspec only when fixed wire layout is policy-canonical.
- Rich owner: use a rich class when construction law, behavior, folds, transition methods, evidence slots, or projection methods exceed declarative fields.
- Family owner: use one closed family owner when mutually exclusive shapes represent one concept.

[GRADUATION_REJECTS]:
- Graduate repeated primitive validation to a vocabulary, constrained scalar, or owner field.
- Graduate repeated field bundles to one model, struct, dataclass, or rich owner.
- Graduate three or more sibling factories, models, or dispatch arms to a closed family or polymorphic owner.
- Graduate mutable update law to immutable replacement.
- Graduate stable wire or persistence concern to an egress projection, not a second domain owner.
- Reject mirrored validation/domain/wire hierarchies, validator side effects, forwarding helper factories, tag-only shape families, optional-field drift, and package-boundary splits.

## [5]-[VOCABULARY_ABSENCE_AND_VARIANTS]

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
from dataclasses import dataclass
from typing import Literal, NotRequired, ReadOnly, TypedDict
from expression import Error, Nothing, Ok, Option, Result, Some

MISSING = sentinel("MISSING")
type ShapeFault = Literal["<invalid-note>"]
type NoteSource = Literal["<omitted>", "<provided>"]

class ShapePatch(TypedDict, total=False, closed=True):
    note: NotRequired[ReadOnly[str | None]]

@dataclass(frozen=True, slots=True, kw_only=True)
class Shape:
    note: str | None = None
    source: NoteSource = "<omitted>"
    def selected(self, fallback: str | MISSING = MISSING, /) -> Option[str]:
        return (
            Some(self.note)
            if self.note is not None
            else Some(fallback) if fallback is not MISSING else Nothing
        )

def admitted(row: ShapePatch, /) -> Result[Shape, ShapeFault]:
    return (
        Error("<invalid-note>")
        if row.get("note") == ""
        else Ok(Shape(note=row["note"], source="<provided>") if "note" in row else Shape())
    )
```

[FAMILIES]:
- Closed family: one owner namespace, one union alias, one vocabulary, total `match`, and `assert_never`.
- Semi-closed family: closed core plus typed extension band or explicit extension variant.
- Open family: only when foreign or plugin code can add members without editing the owner.
- Use `TypeIs` only when a reusable predicate proves exact membership, not filtered validity.
- Use `@disjoint_base`, tagged generics, discriminated Pydantic unions, msgspec tagged unions, or expression tagged unions by lifecycle role.
- Reject optional-field variant bags, string dispatch, `singledispatch` for owned closed vocabularies, protocol-per-variant, catch-all default arms, and foreign token spelling inside canonical owners.

[CLOSED_FAMILY]:

```python conceptual
from dataclasses import dataclass
from enum import StrEnum
from typing import Literal, TypeIs, assert_never

class Variant(StrEnum):
    PRIMARY = "<value-a>"
    SECONDARY = "<value-b>"

@dataclass(frozen=True, slots=True, kw_only=True)
class Shape[T: Variant]:
    kind: T
    key: str

type Primary = Shape[Literal[Variant.PRIMARY]]
type Secondary = Shape[Literal[Variant.SECONDARY]]
type Member = Primary | Secondary

def is_primary(shape: Member, /) -> TypeIs[Primary]:
    return shape.kind is Variant.PRIMARY

def selected(shape: Member, /) -> tuple[Variant, str]:
    match shape:
        case selected if is_primary(selected):
            return selected.kind, selected.key
        case Shape(kind=Variant.SECONDARY) as selected:
            return selected.kind, selected.key
        case unreachable:
            assert_never(unreachable)
```

## [6]-[PROJECTIONS_PORTS_AND_BOUNDARIES]

[BOUNDARY_PROJECTIONS]:
- Ingress direction: payloads, settings, CLI slices, provider material, and ingress models admit foreign material inward.
- Egress direction: wire structs, persistence rows, receipts, schema views, and export views derive outward from canonical owners.
- Adapter ownership: foreign boundaries remap provider names, token vocabularies, cardinality, discriminants, aliases, and omitted fields before canonical entry.
- Correspondence: field mapping belongs in adapter tables, explicit constructors, `msgspec.convert`, or schema-owned aliases.
- Reject: projection-to-projection authority, codec engines in canonical owners, scattered `model_dump` key pops, model-per-provider interiors, provider-shaped domain fields, and canonical `schema_version` branches that belong to read-boundary migration.

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

## [7]-[IMMUTABLE_REPLACEMENT]

[IMMUTABLE_REPLACEMENT_LAW]:
- Owner state: durable owners are frozen after materialization, and durable collections are `tuple`, `frozenset`, `frozendict`, `Map`, `Block`, or another admitted immutable owner.
- Transition shape: state change returns `Self`, `Result[Self, E]`, or a closed successor union; mutation is not a transition.
- Trusted swap: use the owner kernel directly for same-process trusted shallow swaps, such as `copy.replace`, `model_copy(update=...)`, `msgspec.structs.replace`, `frozendict` union, or persistent `Map` and `Block` combinators.
- Revalidated delta: validate a closed patch at the boundary before replacement when the delta is untrusted, computed, wire-sourced, cross-boundary, or requires full schema semantics.
- Deep transition: isolate or rebuild nested identity when shallow replacement would replay mutable, cached, or session-owned state.
- Patch payload: represent patch contracts as closed `TypedDict` shapes with `NotRequired` update fields and `ReadOnly` identity or version fields; patch payloads stop at root materialization and become replacement expressions.
- Alias boundary: normalize aliases before replacement and never use alias keys inside owner replacement.
- Reject: mutable fields on frozen owners unless promoted or isolated, direct `__replace__`, mutate-then-freeze, shallow nested dict updates, cached-session replay by shallow replace, mutable staging maps, and `MappingProxyType` as durable immutability.

## [8]-[COLLAPSE_TESTS]

Apply this lookup before adding a model, struct, dataclass, enum, protocol, payload, wrapper, alias, or helper. All matching rows apply.

| [INDEX] | [SMELL]                    | [TARGET]                           | [EXIT]                      |
| :-----: | :------------------------- | :--------------------------------- | :-------------------------- |
|   [1]   | domain `dict[str, object]` | payload, ingress model, or owner   | domain APIs accept owners   |
|   [2]   | payload after promotion    | adapter materialization            | no interior payload imports |
|   [3]   | mirrored DTO/domain/wire   | canonical owner plus projections   | one invariant owner         |
|   [4]   | duplicate token sets       | vocabulary plus remaps             | no domain `.value` dispatch |
|   [5]   | one-field wrapper          | owner, vocabulary, or scalar       | independent invariant       |
|   [6]   | protocol shell             | `Callable`, owner, family, or port | independent implementers    |
|   [7]   | optional-field variant bag | closed or semi-closed family       | total fold                  |
|   [8]   | provider-shaped interior   | seam adapter plus owner            | provider names stop         |
|   [9]   | domain revalidation        | construction gate                  | no domain validator pass    |
|  [10]   | `model_dump` key surgery   | explicit projection row            | field map owned once        |
|  [11]   | per-request adapter/codec  | module-level singleton             | no hot-path rebuild         |
|  [12]   | mutable staging map        | patch plus replacement owner       | mutation stops at gate      |

## [9]-[VALIDATION]

[FILE]:
- The page gives implementation law, not a research digest, package manual, or essay.
- Every section changes an implementation decision, and examples use neutral placeholders only.
- Indexed tables stay narrow; row detail lives in cards.
- Snippets sit beside the rule they prove and no generic reference-snippet bucket remains.

[SHAPES]:
- Every admitted shape has a lifecycle role.
- Every invariant has exactly one owner.
- Payloads stop at promotion; projections derive from canonical or map inward through adapters.
- Every closed family has one vocabulary and total folds.
- Semi-closed families have typed extension bands or extension variants; open families are justified by foreign extension.

[BOUNDARIES]:
- Canonical owners do not import projection engines, provider payloads, settings readers, codec singletons, or foreign interiors.
- Pydantic and msgspec are lifecycle owners, not preferences; aliases, wire tokens, schema versions, command strings, and provider names stay at boundaries.
- Exceptions from validation and codecs map at boundary owners.

[ABSENCE_AND_FAILURE]:
- Omitted key, valid `None`, sentinel omission, wire unset/null, explicit state, `Option.none`, and `Result.Error` are distinct.
- Sentinels are module-global and never serialized; `None` is not failure; `Option` does not hide validation errors.

[COLLAPSE]:
- Duplicate schema/model/class/enum/protocol/payload surfaces are collapsed before implementation.
- Wrappers, aliases, constants, helpers, and tiny classes remain only when they add an invariant or boundary role.
- `dict[str, object]`, provider-shaped interiors, string dispatch, and mutable staging maps are eliminated or confined to their owner stage.
