# [PY_CAD_PROVENANCE]

`Correspondence` is the map from every operand sub-shape to the images it owns in a result, and `read` is the one fold that builds it off a finished operator. This owner exists because the kernel already answers per-shape which sub-shapes it kept, modified, generated, and consumed, and the wire carries every one of those answers as a `Trace` row.

`Outcome` pairs a result body with its correspondence, so `brep/operation#SPINE` threads one value instead of a mutable accumulator and an arm that maps nothing carries the empty `Correspondence` rather than a null. `Correspondence`, `Trace`, `Image`, `Grain`, and `Relation` are the generated wire classes and this owner mints them directly; `brep/operation#SPINE` lands the value on `BrepEvidence` and `service/provider#PROVIDER` publishes it as `ExecuteResponse.correspondence`.

## [01]-[INDEX]

- [02]-[CORRESPONDENCE]: the grain roster, the outcome pair, and the wire correspondence every arm carries.
- [03]-[READING]: history protocol, the ordered relation election, and the trace fold over every operand.
- [04]-[RESEARCH]: open questions.

## [02]-[CORRESPONDENCE]

- Owner: `Outcome` — a result body beside the `Correspondence` its operator answered, every operand sub-shape addressed by where it came from and the result ordinals it now occupies.
- Cases: `Relation` spells `DELETED`, `GENERATED`, `MODIFIED`, and `KEPT`, which are the four answers a finished operator gives about one source sub-shape.
- Cases: `_GRAINS` seats the two `Grain` members a finished operator answers history for; `VERTEX` and `SOLID` stay wire vocabulary no history query returns.
- Law: `Trace` carries an operand ordinal beside a source ordinal, so a result sub-shape is named by its origin rather than by a position the next reseal renumbers.
- Law: `KEPT` names itself as its own image, so a surviving sub-shape is a row like any other and survival is never inferred from an absent row.
- Law: an arm that maps nothing carries `Correspondence()` — an empty trace roster the wire frames as a zero count, never an unset slot a consumer tests beside the roster.
- Law: the generated classes are the interior carrier — a `msgspec` twin restating `Trace` or `Correspondence` is the deleted mirror, and `Outcome` alone stays a dataclass because it holds a native handle.
- Boundary: `Outcome` never crosses the worker seam; `service/lane` marshals `BrepEvidence.correspondence` as binary and the native handle stays worker-local.

```python
from collections.abc import Callable, Sequence
from dataclasses import dataclass, field
from functools import partial
from typing import Final, Protocol

from OCP.TopAbs import TopAbs_EDGE, TopAbs_FACE, TopAbs_ShapeEnum
from OCP.TopExp import TopExp
from OCP.TopTools import TopTools_IndexedMapOfShape, TopTools_ListOfShape
from OCP.TopoDS import TopoDS_Shape
from builtins import frozendict
from expression import Nothing, Option, Some
from expression.collections import Block
# Contracts are retired from this logic.

# --- [CONSTANTS] ------------------------------------------------------------------------

_GRAINS: Final[frozendict[Grain, TopAbs_ShapeEnum]] = frozendict({Grain.FACE: TopAbs_FACE, Grain.EDGE: TopAbs_EDGE})


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class Outcome:
    shape: TopoDS_Shape
    correspondence: Correspondence = field(default_factory=Correspondence)

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
- Law: image ordinals index the result body's `TopExp` order before the seal, so durable selection stays open — every `Execute` returns a resealed body whose order the seal decides, and a `fillet` after a `fuse` cannot yet name the edge it wants.
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


def _imaged(grain: Grain, mapped: TopTools_IndexedMapOfShape, shapes: Block[TopoDS_Shape], /) -> list[Image]:
    return [Image(grain=grain, ordinal=ordinal) for ordinal in shapes.choose(partial(_ordinal, mapped))]


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
    return Trace(grain=grain, operand=operand, source=ordinal, relation=relation, images=_imaged(grain, images, kept))


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
        traces=[
            trace
            for index, operand in enumerate(operands)
            for grain in _GRAINS
            for trace in _traced(history, images[grain], operand, grain, index)
        ],
        section=_imaged(Grain.EDGE, images[Grain.EDGE], _shapes(section)),
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

(none)
