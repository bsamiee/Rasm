# [PY_CAD_PROVENANCE]

`Correspondence` is the map from every operand sub-shape to the images it owns in a result, and `read` is the one fold that builds it off a finished operator. This owner exists because the kernel already answers per-shape which sub-shapes it kept, modified, generated, and consumed, and the receipt that reaches a caller today collapses all of it into two booleans.

`Outcome` pairs a result body with its correspondence, so `brep/operation#SPINE` threads one value instead of a mutable accumulator and an arm that maps nothing carries `Correspondence.EMPTY` rather than a null. `BooleanProvenance` and `BrepKernelReceipt` are the frozen wire names; `projected` is the lossy egress this owner owns, and `metrology/properties#RECEIPT` mints the receipt field from it.

## [01]-[INDEX]

- [02]-[CORRESPONDENCE]: grain, relation, and trace vocabularies, the outcome pair, and the lossy wire egress.
- [03]-[READING]: history protocol, the ordered relation election, and the exact capability the two-boolean collapse destroys.
- [04]-[RESEARCH]: open questions.

## [02]-[CORRESPONDENCE]

- Owner: `Correspondence` — every operand sub-shape addressed by where it came from, beside the result ordinals it now occupies.
- Cases: `Relation` spells `DELETED`, `GENERATED`, `MODIFIED`, and `KEPT`, which are the four answers a finished operator gives about one source sub-shape.
- Cases: `Grain` names the sub-shape kinds a Boolean answers for; it selects from `TopAbs_ShapeEnum` rather than mirroring it, because the remaining members no history query returns.
- Law: `Trace` carries an operand ordinal beside a source ordinal, so a result sub-shape is named by its origin rather than by a position the next reseal renumbers.
- Law: `KEPT` names itself as its own image, so a surviving sub-shape is a row like any other and survival is never inferred from an absent row.
- Law: `Correspondence.EMPTY` is what an arm that maps nothing carries; absence is a value the fold threads, never a null the receipt mint re-tests.
- Law: `projected` derives both wire booleans from the trace roster, so one authority answers modified and generated rather than two independently read operator flags.
- Output: `Option[BooleanProvenance]` — empty where no correspondence exists, so the receipt leaves its field absent instead of asserting a false negative.
- Boundary: `Outcome` holds a native handle and stays a frozen dataclass rather than a wire struct; `service/lane` marshals what leaves the worker, and this pair never does.

```python
from collections.abc import Callable, Sequence
from dataclasses import dataclass
from enum import StrEnum
from functools import partial
from typing import Final, Protocol

from OCP.TopAbs import TopAbs_EDGE, TopAbs_FACE, TopAbs_ShapeEnum
from OCP.TopExp import TopExp
from OCP.TopTools import TopTools_IndexedMapOfShape, TopTools_ListOfShape
from OCP.TopoDS import TopoDS_Shape
from builtins import frozendict
from expression import Nothing, Option, Some
from expression.collections import Block
from msgspec import Struct
from rasm.contracts.rasm.contracts.cad.types_pb import BooleanProvenance

# --- [TYPES] ----------------------------------------------------------------------------


class Grain(StrEnum):
    FACE = "face"
    EDGE = "edge"


class Relation(StrEnum):
    DELETED = "deleted"
    GENERATED = "generated"
    MODIFIED = "modified"
    KEPT = "kept"


# --- [CONSTANTS] ------------------------------------------------------------------------

_GRAINS: Final[frozendict[Grain, TopAbs_ShapeEnum]] = frozendict({Grain.FACE: TopAbs_FACE, Grain.EDGE: TopAbs_EDGE})


# --- [MODELS] ---------------------------------------------------------------------------


class Trace(Struct, frozen=True, kw_only=True):
    grain: Grain
    operand: int
    source: int
    relation: Relation
    images: tuple[int, ...]


class Correspondence(Struct, frozen=True, kw_only=True):
    traces: tuple[Trace, ...] = ()
    section: tuple[int, ...] = ()

    @property
    def projected(self) -> Option[BooleanProvenance]:
        return (
            Nothing
            if not self.traces and not self.section
            else Some(
                BooleanProvenance(
                    modified=any(trace.relation is Relation.MODIFIED for trace in self.traces),
                    generated=any(trace.relation is Relation.GENERATED for trace in self.traces),
                )
            )
        )


EMPTY: Final[Correspondence] = Correspondence()


@dataclass(frozen=True, slots=True, kw_only=True)
class Outcome:
    shape: TopoDS_Shape
    correspondence: Correspondence = EMPTY

    @staticmethod
    def of(shape: TopoDS_Shape, /) -> "Outcome":
        return Outcome(shape=shape)
```

## [03]-[READING]

- Owner: `read` — one fold over the finished operator's `Generated`, `Modified`, and `IsDeleted` answers for every operand sub-shape.
- Law: precedence is the data — `_ELECTED` is an ordered roster and the fall-through names the survivor, so a fifth answer is one row and never a branch.
- Law: an inapplicable probe contributes nothing to the election through `Option.to_list()` — the one member giving `Nothing` an empty-sequence reading, since iterating `Option` directly is the effect-builder protocol and `Nothing` raises — so the roster order alone decides, with no tag re-read.
- Law: result maps build once per read rather than once per operand, so the cost is one index pass per grain no matter how many operands the call carries.
- Law: reading is priced per Boolean, which is why `nary` at `brep/boolean#BOOLEAN` skips it — an in-process assembly has no caller waiting to address its parts.
- Law: `BooleanProvenance` loses which operand contributed a surviving face, so a caller cannot tell a cut's keeper from its tool.
- Law: `BooleanProvenance` loses image cardinality, so a face split into several carries the same flag as one face shifted untouched.
- Law: `BooleanProvenance` loses deletion entirely — `IsDeleted` separates a consumed sub-shape from an untouched one, and neither flag spells it.
- Law: `BooleanProvenance` loses `SectionEdges`, so the `section` arm's whole product goes unnamed in the receipt that arm returns.
- Law: durable selection dies with that collapse — every `Execute` returns a resealed body whose `TopExp` order the seal decides, so a `fillet` after a `fuse` cannot name the edge it wants.
- Boundary: this owner reads history off a finished operator and never runs one; the operator, its operand partition, and its policy belong to `brep/boolean#BOOLEAN`.

```python
# --- [SERVICES] -------------------------------------------------------------------------


class ShapeHistory(Protocol):
    def Generated(self, shape: TopoDS_Shape, /) -> TopTools_ListOfShape: ...
    def Modified(self, shape: TopoDS_Shape, /) -> TopTools_ListOfShape: ...
    def IsDeleted(self, shape: TopoDS_Shape, /) -> bool: ...


# --- [OPERATIONS] -----------------------------------------------------------------------


def _indexed(shape: TopoDS_Shape, kind: TopAbs_ShapeEnum, /) -> TopTools_IndexedMapOfShape:
    carrier = TopTools_IndexedMapOfShape()
    TopExp.MapShapes_s(shape, kind, carrier)
    return carrier


def _shapes(carrier: TopTools_ListOfShape, /) -> Block[TopoDS_Shape]:
    ...


def _ordinal(mapped: TopTools_IndexedMapOfShape, shape: TopoDS_Shape, /) -> Option[int]:
    ...


def _projected(mapped: TopTools_IndexedMapOfShape, shapes: Block[TopoDS_Shape], /) -> tuple[int, ...]:
    return tuple(shapes.choose(partial(_ordinal, mapped)))


def _nonempty(carrier: TopTools_ListOfShape, /) -> Option[Block[TopoDS_Shape]]:
    return Nothing if carrier.Extent() == 0 else Some(_shapes(carrier))


def _relation(history: ShapeHistory, source: TopoDS_Shape, /) -> tuple[Relation, Block[TopoDS_Shape]]:
    return next(
        ((relation, images) for relation, probe in _ELECTED for images in probe(history, source).to_list()),
        (Relation.KEPT, Block.singleton(source)),
    )


def _trace(
    history: ShapeHistory,
    images: TopTools_IndexedMapOfShape,
    source: TopoDS_Shape,
    grain: Grain,
    operand: int,
    ordinal: int,
    /,
) -> Trace:
    relation, kept = _relation(history, source)
    return Trace(grain=grain, operand=operand, source=ordinal, relation=relation, images=_projected(images, kept))


def _traced(
    history: ShapeHistory,
    images: TopTools_IndexedMapOfShape,
    operand: TopoDS_Shape,
    grain: Grain,
    index: int,
    /,
) -> Block[Trace]:
    sources = _indexed(operand, _GRAINS[grain])
    return Block.of_seq(range(sources.Extent())).map(
        lambda ordinal: _trace(history, images, sources.FindKey(ordinal + 1), grain, index, ordinal)
    )


def read(
    history: ShapeHistory,
    operands: Sequence[TopoDS_Shape],
    result: TopoDS_Shape,
    section: TopTools_ListOfShape,
    /,
) -> Correspondence:
    images = frozendict({grain: _indexed(result, kind) for grain, kind in _GRAINS.items()})
    return Correspondence(
        traces=tuple(
            trace
            for index, operand in enumerate(operands)
            for grain in _GRAINS
            for trace in _traced(history, images[grain], operand, grain, index)
        ),
        section=_projected(images[Grain.EDGE], _shapes(section)),
    )


# --- [ELECTION] -------------------------------------------------------------------------

_ELECTED: Final[
    tuple[tuple[Relation, Callable[[ShapeHistory, TopoDS_Shape], Option[Block[TopoDS_Shape]]]], ...]
] = (
    (Relation.DELETED, lambda history, source: Some(Block.empty()) if history.IsDeleted(source) else Nothing),
    (Relation.GENERATED, lambda history, source: _nonempty(history.Generated(source))),
    (Relation.MODIFIED, lambda history, source: _nonempty(history.Modified(source))),
)
```

## [04]-[RESEARCH]

- [LIST_TRAVERSAL]-[OPEN]: which member reads a `TopTools_ListOfShape` in order — a bound iterator class, or Python iteration over the list itself; verify against the installed `OCP.TopTools` surface and land the row in the folder `.api` catalogue.
- [MAP_REVERSE]-[OPEN]: does `TopTools_IndexedMapOfShape` answer a shape-to-index lookup, and does it report absence as zero or by raising; verify against the installed `OCP.TopTools` surface and land the row in the folder `.api` catalogue.
- [PROVENANCE_WIRE]-[OPEN]: which `cad` message carries the per-sub-shape correspondence beside `BooleanProvenance`, and does it ride `BrepKernelReceipt` or a separate response field; verify against `libs/contracts/proto/rasm/contracts/cad/types.proto` and the wire-contract law.
- [DURABLE_NAME]-[OPEN]: does a resealed body admit a durable sub-shape name — a STEP-persisted identifier, or a correspondence stored beside the artifact — so a feature can follow a Boolean; verify against `exchange/identity#CANONICAL` and a live reseal probe.
