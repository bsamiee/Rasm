# [PY_CAD_OPERATION]

`execute` folds every `ExecuteRequest.operation` arm through one row table into one spine, then returns the measured and sealed body as `BrepEvidence`. This owner composes source resolution from `exchange/step#CODEC` `sourced` and builder admission from `brep/placement#ADMISSION` `built` — each read at its owner, one hop, never re-exported — and owns no geometry of its own.

Refusals ride `CadRail` on `BREP_INPUT`, `BREP_KERNEL`, and `BREP_OUTPUT` from `faults#ROWS`. Spatial lowering arrives from `brep/placement#PLACEMENT`, analytic bodies from `brep/solid#PRIMITIVES`, generative bodies from `brep/solid#GENERATIVE`, set algebra from `brep/boolean#BOOLEAN`, edge features from `brep/feature#FEATURE`, correspondence from `brep/provenance#CORRESPONDENCE`, measurement from `metrology/properties#MEASURE`, and the codec pair from `exchange/step#CODEC`.

## [01]-[INDEX]

- [02]-[OPERATION]: builder and shape verdicts, source resolution, and the healing boundary every B-rep page composes.
- [03]-[ARMS]: one row per wire operation field, the four row shapes that close the table, and the three arm bodies this page keeps.
- [04]-[SPINE]: election, measurement, seal handoff, and the evidence pair the lane marshals.
- [05]-[RESEARCH]: open questions.

## [02]-[OPERATION]

- Entry: `built` composes from `brep/placement#ADMISSION`, which seats the rail below every arm this fold imports.
- Law: `admitted` (the shape half alone, also placement's) serves any result no builder produced — sewn, lofted, offset, piped — so one site asks one question.
- Law: grading splits the verdict — an unfinished builder is `BREP_KERNEL`, a finished builder yielding a null or invalid body is `BREP_OUTPUT`.
- Law: validity means geometry as well as topology, so a null probe alone admits self-intersecting bodies and never stands in for the analyzer.
- Law: `sourced` lives at `exchange/step#CODEC` and threads that rail unchanged, so a decode refusal keeps its `exchange` leg and no resolution twin grows here.
- Law: every composed name resolves at its owner — placement for admission, step for resolution — so no sibling imports this apex and the arm graph stays acyclic.
- Law: heterogeneous compound assembly is deleted, not carried — no `ExecuteRequest.operation` field asks for it, and every arm collapses to one body before the seal.
- Law: many-part results fold through `brep/solid#FOLD`, whose fuse merges abutting regions into one manifold body where compound assembly leaves coincident faces and a false census.
- Boundary: source topology arrives as the file owns it; reader precision, forced maximums, `ShapeFix_ShapeTolerance`, and `BRepLib.SameParameter_s` stay outside.
- Boundary: healing earns its own typed admission contract and answers `Healing`, and never aliases tessellation deflection or IFC precision.

```python
from collections.abc import Callable
from dataclasses import dataclass
from functools import partial
from operator import attrgetter
from pathlib import Path
from typing import Final

from OCP.BRepBuilderAPI import BRepBuilderAPI_NurbsConvert, BRepBuilderAPI_Sewing, BRepBuilderAPI_Transform
from OCP.TopoDS import TopoDS_Shape
from builtins import frozendict
from expression import Error, Ok, Option
from protobuf import Message, Oneof
from rasm.contracts.rasm.contracts.cad.operations_pb import (
    BooleanInputs,
    ChamferOp,
    ExecuteRequest,
    ExtrudeOp,
    FilletOp,
    LoftOp,
    RevolveOp,
    SewOp,
    SweepOp,
    ThickOp,
    TransformOp,
)
from rasm.contracts.rasm.contracts.cad.types_pb import BrepMeasure, Correspondence, SealedStep, StepProtocol

from rasm.cad.brep.boolean import BOOLEANS, boolean
from rasm.cad.brep.feature import featured
from rasm.cad.brep.placement import admitted, built, displaced
from rasm.cad.brep.provenance import Outcome
from rasm.cad.brep.solid import PRIMITIVE, extruded, lofted, primitive, revolved, swept, thickened
from rasm.cad.exchange.step import sealed, sourced
from rasm.cad.faults import BREP_INPUT, BREP_KERNEL, CadRail
from rasm.cad.metrology.properties import UNMEASURED, measured
```

## [03]-[ARMS]

- Owner: `_ARMS` — one row per `ExecuteRequest.operation` field, each row an arrow from the elected oneof to an outcome.
- Cases: four row shapes close the table — rostered primitive, whole-payload body, single-source rewrite, and set algebra over many operands.
- Law: each row narrows its own payload, so the erasure a heterogeneous table forces never reaches the spine and no row reads a class the next row expects.
- Law: primitive fields route to `brep/solid#PRIMITIVES` on roster membership, so the `BRepPrimAPI` class literals stay at their owner and this table proves nothing twice.
- Law: Boolean fields route to `BOOLEANS` at `brep/boolean#BOOLEAN`, so five wire fields carry no arm body here and a sixth operator lands at its owner.
- Law: rigid placement lands here rather than at `brep/placement#PLACEMENT`, which mints the matrix alone because applying one to a shape is a builder admission this page owns.
- Law: sewing and NURBS conversion are the two arms whose whole body rewrites one decoded source, so both seat with the table rather than at a page of their own.
- Law: `BRepBuilderAPI_Sewing` answers `Perform`/`SewedShape` and never `IsDone`, so it composes `admitted` while every `Make*` builder composes `built`.
- Law: sewing re-rows its refusal to `BREP_INPUT` through `map_error`, so the free, multiple, and degenerate census prices only on the refusing arm.
- Growth: a new operation is one proto field beside one `_ARMS` row, and one row at the owner that mints its body.
- Boundary: payload field reading stays at the row that knows the payload type, so no downstream owner receives a generated message it must re-discriminate.

```python
# --- [TYPES] ----------------------------------------------------------------------------

type Arm = Callable[[Oneof, frozendict[bytes, Path]], CadRail[Outcome]]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _narrowed[P: Message](payload: type[P], held: Oneof, /) -> CadRail[P]:
    return Ok(held.value) if isinstance(held.value, payload) else Error(BREP_INPUT.at(f"operation.payload:{held.field}"))


def _defects(builder: BRepBuilderAPI_Sewing, /) -> str:
    return f"sew.free={builder.NbFreeEdges()};multiple={builder.NbMultipleEdges()};degenerate={builder.NbDegeneratedShapes()}"


def sewn(shape: TopoDS_Shape, tolerance_m: float, /) -> CadRail[TopoDS_Shape]:
    builder = BRepBuilderAPI_Sewing(tolerance_m)
    builder.Add(shape)
    builder.Perform()
    return admitted(builder.SewedShape(), "sew").map_error(lambda _refusal: BREP_INPUT.at(_defects(builder)))


def placed(shape: TopoDS_Shape, op: TransformOp, /) -> CadRail[TopoDS_Shape]:
    matrix = displaced(op.source_frame, op.target_frame, op.uniform_scale)
    return built(BRepBuilderAPI_Transform(shape, matrix, True, False), "transform")


def _rostered(held: Oneof, _sources: frozendict[bytes, Path], /) -> CadRail[Outcome]:
    return primitive(held).map(Outcome.of)


def _algebra(field: str, held: Oneof, sources: frozendict[bytes, Path], /) -> CadRail[Outcome]:
    return _narrowed(BooleanInputs, held).bind(lambda payload: boolean(field, payload, sources))


def _shaped[P: Message](payload: type[P], run: Callable[[P], CadRail[TopoDS_Shape]], /) -> Arm:
    return lambda held, _sources: _narrowed(payload, held).bind(run).map(Outcome.of)


def _rewritten[P: Message](
    payload: type[P],
    source: Callable[[P], SealedStep],
    run: Callable[[TopoDS_Shape, P], CadRail[TopoDS_Shape]],
    /,
) -> Arm:
    def arm(held: Oneof, sources: frozendict[bytes, Path], /) -> CadRail[Outcome]:
        return _narrowed(payload, held).bind(lambda op: sourced(source(op), sources).bind(lambda shape: run(shape, op))).map(Outcome.of)

    return arm


# --- [ARMS] -----------------------------------------------------------------------------

_ARMS: Final[frozendict[str, Arm]] = frozendict({
    **{field: _rostered for field in PRIMITIVE},
    **{field: partial(_algebra, field) for field in BOOLEANS},
    "extrude": _shaped(ExtrudeOp, extruded),
    "revolve": _shaped(RevolveOp, revolved),
    "loft": _shaped(LoftOp, lofted),
    "thick": _shaped(ThickOp, thickened),
    "sweep": _shaped(SweepOp, swept),
    "fillet": _rewritten(FilletOp, attrgetter("source"), lambda shape, op: featured("fillet", shape, op.edges, op.radius_m)),
    "chamfer": _rewritten(ChamferOp, attrgetter("source"), lambda shape, op: featured("chamfer", shape, op.edges, op.distance_m)),
    "sew": _rewritten(SewOp, attrgetter("source"), lambda shape, op: sewn(shape, op.tolerance_m)),
    "nurbs": _rewritten(SealedStep, lambda op: op, lambda shape, _op: built(BRepBuilderAPI_NurbsConvert(shape), "nurbs")),
    "transform": _rewritten(TransformOp, attrgetter("source"), placed),
})
```

## [04]-[SPINE]

- Owner: `execute` — one spine over the elected row: run the arm, measure the body, seal it, pair the evidence.
- Law: an unmapped field refuses at `BREP_KERNEL` because a table lagging the wire is a provider defect, while a payload of the wrong class refuses at `BREP_INPUT` inside the row that expected it.
- Law: measurement precedes sealing, so an unmeasurable body never reaches the caller-owned output path and no partial artifact is published.
- Law: `measured` and `sealed` both return `CadRail`, so the seal's exception-to-exception restamp is gone and each refusal keeps its own producing leg.
- Law: correspondence rides the outcome rather than a mutable accumulator threaded across arms, so an arm mapping no sub-shape carries an empty correspondence instead of a null.
- Law: closure is `UNMEASURED` on every B-rep call because this leg runs no mesher, which is what keeps `watertight` and `volume_delta_m3` jointly absent on the wire.
- Output: `BrepEvidence` carries the generated `BrepMeasure`, the written body's protocol, and the `Correspondence` the arm answered.
- Boundary: `service/lane` marshals the measure and correspondence to bytes and the protocol to an ordinal, so no generated message crosses the process pipe as an object.
- Boundary: this owner never publishes the artifact or mints `SealedStep`, because the response envelope belongs to `service/provider#PROVIDER`.

```python
# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class BrepEvidence:
    measure: BrepMeasure
    protocol: StepProtocol
    correspondence: Correspondence


# --- [OPERATIONS] -----------------------------------------------------------------------


def _dispatched(elected: Oneof, sources: frozendict[bytes, Path], /) -> CadRail[Outcome]:
    return (
        Option.of_optional(_ARMS.get(elected.field))
        .to_result_with(lambda: BREP_KERNEL.at(f"operation.unmapped:{elected.field}"))
        .bind(lambda arm: arm(elected, sources))
    )


def _evidence(outcome: Outcome, output: Path, /) -> CadRail[BrepEvidence]:
    return measured(outcome.shape, UNMEASURED).bind(
        lambda measure: sealed(outcome.shape, output).map(
            lambda protocol: BrepEvidence(measure=measure, protocol=protocol, correspondence=outcome.correspondence)
        )
    )


def execute(request: ExecuteRequest, sources: frozendict[bytes, Path], output: Path, /) -> CadRail[BrepEvidence]:
    return _dispatched(request.operation, sources).bind(lambda outcome: _evidence(outcome, output))
```

## [05]-[RESEARCH]

(none)
