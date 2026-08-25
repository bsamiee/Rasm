# [PY_CAD_BOOLEAN]

`boolean` runs the provider's n-ary set algebra: five operators over one operand partition, one tolerance knob, and one parallelism verdict. This owner is the only B-rep family where operand ORDER carries meaning and the only one that reads a correspondence off the finished operator, so the argument-and-tool split, the fuzzy value, and the run-parallel decision all settle here.

Operands resolve through `sourced` at `exchange/step#CODEC` and every verdict through `built` at `brep/placement#ADMISSION`; refusal rows come from `faults#ROWS`; the correspondence a finished operator carries is read by `brep/provenance#READING` and rides home on its `Outcome`. `BOOLEANS` is the roster `brep/operation#ARMS` folds into its five wire rows, and `nary` is the in-process form `brep/solid#FOLD` composes for region union and hole carving.

## [01]-[INDEX]

- [02]-[BOOLEAN]: operator roster, operand partition, and the two entries that reach them.
- [03]-[TOLERANCE]: fuzzy value, parallel custody, and the one seating every operator passes through.
- [04]-[RESEARCH]: open questions.

## [02]-[BOOLEAN]

- Owner: `boolean` — n-ary set algebra over wire operands; `nary` — the same operators over shapes already in hand.
- Cases: `BOOLEANS` rows `fuse`, `cut`, `common`, `section`, and `split`, each carrying a `BRepAlgoAPI` class literal and nothing else.
- Law: operand order is semantic here and nowhere else — head is the argument set, tail the tool set, and one partition rule serves all five rows.
- Law: `cut` and `split` read that partition directionally while `fuse` and `common` do not, so the rule states itself once rather than once per row.
- Law: `BooleanInputs.operands` carries a two-item floor on the wire, which is what makes the head-and-tail split total without a length guard.
- Law: `listed` seats here because the n-ary operators are its only consumers, so `TopTools_ListOfShape` never reaches another page.
- Law: `nary` takes an operand run already in hand and mints no correspondence, so hole carving and region union pay no per-sub-shape history read.
- Law: `nary` takes its factory rather than a roster key, so an in-process caller naming `BRepAlgoAPI_Cut` proves its operator at the call site instead of through a lookup this owner then guards.
- Output: `boolean` returns the `Outcome` pair, so a result body and its correspondence travel as one value the fold never reassembles.
- Boundary: refusal grading belongs to `built`, so this owner adds no second verdict and never re-asks validity of a body the verdict already admitted.

```python
from collections.abc import Callable, Iterable, Sequence
from functools import partial
from pathlib import Path
from typing import Final, Protocol

from OCP.BRepAlgoAPI import (
    BRepAlgoAPI_Common,
    BRepAlgoAPI_Cut,
    BRepAlgoAPI_Fuse,
    BRepAlgoAPI_Section,
    BRepAlgoAPI_Splitter,
)
from OCP.TopTools import TopTools_ListOfShape
from OCP.TopoDS import TopoDS_Shape
from builtins import frozendict
from expression.collections import Block
from expression.extra.result import traverse
from rasm.contracts.rasm.contracts.cad.operations_pb import BooleanInputs

from rasm.cad.brep.placement import ShapeBuilder, built
from rasm.cad.brep.provenance import Outcome, ShapeHistory, read
from rasm.cad.exchange.step import sourced
from rasm.cad.faults import CadRail

# --- [SERVICES] -------------------------------------------------------------------------


class BooleanBuilder(ShapeBuilder, ShapeHistory, Protocol):
    def SetArguments(self, shapes: TopTools_ListOfShape) -> None: ...
    def SetTools(self, shapes: TopTools_ListOfShape) -> None: ...
    def SetFuzzyValue(self, value: float) -> None: ...
    def SetRunParallel(self, enabled: bool) -> None: ...
    def SectionEdges(self) -> TopTools_ListOfShape: ...


# --- [OPERATIONS] -----------------------------------------------------------------------


def listed(shapes: Iterable[TopoDS_Shape], /) -> TopTools_ListOfShape:
    carrier = TopTools_ListOfShape()
    for shape in shapes:
        carrier.Append(shape)
    return carrier


def nary(
    arguments: Sequence[TopoDS_Shape],
    tools: Sequence[TopoDS_Shape],
    factory: Callable[[], BooleanBuilder],
    coordinate: str,
    /,
) -> CadRail[TopoDS_Shape]:
    return built(_seeded(factory(), arguments, tools, 0.0), coordinate)


def _performed(field: str, fuzzy_m: float, shapes: Block[TopoDS_Shape], /) -> CadRail[Outcome]:
    operands = tuple(shapes)
    operation = _seeded(BOOLEANS[field](), operands[:1], operands[1:], fuzzy_m)
    return built(operation, field).map(
        lambda shape: Outcome(shape=shape, correspondence=read(operation, operands, shape, operation.SectionEdges()))
    )


def boolean(field: str, payload: BooleanInputs, sources: frozendict[bytes, Path], /) -> CadRail[Outcome]:
    return traverse(lambda operand: sourced(operand, sources), Block.of_seq(payload.operands)).bind(
        partial(_performed, field, payload.fuzzy_m)
    )


# --- [OPERATORS] ------------------------------------------------------------------------

BOOLEANS: Final[frozendict[str, Callable[[], BooleanBuilder]]] = frozendict({
    "fuse": BRepAlgoAPI_Fuse,
    "cut": BRepAlgoAPI_Cut,
    "common": BRepAlgoAPI_Common,
    "section": BRepAlgoAPI_Section,
    "split": BRepAlgoAPI_Splitter,
})
```

## [03]-[TOLERANCE]

- Owner: `_seeded` — one seating of arguments, tools, fuzzy tolerance, and parallel custody ahead of every `Build`.
- Law: zero on `BooleanInputs.fuzzy_m` means kernel-owned tolerance rather than zero tolerance, so the setter is skipped instead of handed zero.
- Law: `PARALLEL` is admitted because `service/lane` grants the worker whole-process custody for one call at a time; a shared lane flips this constant, never a per-call knob.
- Law: parallelism is a lane fact rather than an operator preference, so the same custody answer governs the tessellation mesher instead of being re-decided per kernel.
- Boundary: `fuzzy_m` is the only tolerance the wire carries into this kernel, because sewing tolerance rides `SewOp` and mesh deflection rides the tessellation policy.

```python
# --- [CONSTANTS] ------------------------------------------------------------------------

PARALLEL: Final[bool] = True


# --- [OPERATIONS] -----------------------------------------------------------------------


def _seeded(
    operation: BooleanBuilder,
    arguments: Sequence[TopoDS_Shape],
    tools: Sequence[TopoDS_Shape],
    fuzzy_m: float,
    /,
) -> BooleanBuilder:
    operation.SetArguments(listed(arguments))
    operation.SetTools(listed(tools))
    if fuzzy_m > 0.0:
        operation.SetFuzzyValue(fuzzy_m)
    operation.SetRunParallel(PARALLEL)
    return operation
```

## [04]-[RESEARCH]

- [SPLITTER_SECTION]-[OPEN]: does `BRepAlgoAPI_Splitter` carry `SectionEdges` like the Boolean operators, or does the protocol need a narrower base for the split row; verify against the installed `OCP.BRepAlgoAPI` surface.
- [NON_DESTRUCTIVE]-[OPEN]: does `SetNonDestructive(True)` change the history a correspondence reads, and does an operand a later arm reuses require it; verify against the folder `.api` catalogue and a live n-ary probe.
- [GLUE_POLICY]-[OPEN]: does `SetGlue` earn a wire arm for operands known to share coincident faces, and what does it cost when that assumption fails; verify against the folder `.api` catalogue and a live probe.
