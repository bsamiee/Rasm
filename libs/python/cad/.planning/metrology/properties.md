# [PY_CAD_PROPERTIES]

`receipt` is the provider's one measurement of an exact B-rep shape: validity admission, the dimensional census, mass and centroid at the elected dimension, surface area, and the reconciliation delta against emitted mesh evidence, folded into one `BrepKernelReceipt`. Measurement reads a `TopoDS_Shape` and nothing else — no reader, no document, no emitted file — so every leg holding a shape reaches one owner instead of importing a sibling's modeling page for it.

Seating measurement here dissolves the strata edge the flat corpus carried, where the tessellation leg imported `receipt` off the B-rep page and paid a `watertight=`/`mesh_volume_m3=` argument pair the B-rep leg can never supply. `Closure` replaces that pair arm for arm with the `BrepKernelReceipt` presence rule, so a leg states what it measured instead of threading two nullable knobs. This owner imports `faults` alone among siblings, and `brep/operation#OPERATION` and `tessellation/mesh#MESH` both compose it without either reaching the other.

## [01]-[INDEX]

- [02]-[LADDER]: Validity admission, the indexed topology census, and the dimensional precedence table electing mass and centroid.
- [03]-[RECEIPT]: `Closure` over the wire's own presence rule, the reconciliation delta, and the one `BrepKernelReceipt` mint.

## [02]-[LADDER]

- Owner: `_measured` — one election over `_LADDER`, whose row order IS the dimensional precedence and whose refusal names the rung.
- Cases: `Dimension` ranks `SOLID`, `FACE`, `EDGE` high to low; election takes the first rung whose census extent is non-zero.
- Law: precedence rides row order, never a body — the deleted `elif` ladder re-spelled `solids > 0`, `faces > 0`, `edges > 0` in three arms where one ordered table states the same rank once, and a fourth dimension lands as one row with no body touched.
- Law: `_topology` counts through `TopExp.MapShapes_s` into a per-kind `TopTools_IndexedMapOfShape`, so a shape shared by two parents counts once.
- Law: solid mass folds one mass-weighted centroid over each indexed `TopAbs_SOLID`, taking each solid's mass absolute, so a reversed solid contributes magnitude instead of cancelling a sibling into a forged zero.
- Law: a volume that is not finite and positive refuses by name on `MEASURE_DEGENERATE`; publishing `0.0` certifies a degenerate solid as measured, and every later mesh reconciliation then compares against that forged zero.
- Law: election exhaustion is vertex-only topology and refuses; a `0.0` fall-through at the ladder foot is the deleted form.
- Law: `MatrixOfInertia` rides the filled `GProp_GProps` with no reader, so `geometry/ifc/structural#STRUCTURAL` re-derives section inertia from its own contour fold.
- Growth: a new measured dimension is one `Dimension` member and one `_LADDER` row carrying its extent reader and its property fill.
- Boundary: exact topology alone. Emitted-mesh evidence arrives already admitted from `metrology/census#CENSUS` as a `Closure` arm, and no OCCT mesher, reader, or writer is reachable from this owner.

```python
from collections.abc import Callable
from enum import IntEnum
from math import isfinite
from typing import Final

from OCP.BRepCheck import BRepCheck_Analyzer
from OCP.BRepGProp import BRepGProp
from OCP.GProp import GProp_GProps
from OCP.TopAbs import TopAbs_EDGE, TopAbs_FACE, TopAbs_SOLID, TopAbs_VERTEX, TopAbs_ShapeEnum
from OCP.TopExp import TopExp
from OCP.TopTools import TopTools_IndexedMapOfShape
from OCP.TopoDS import TopoDS, TopoDS_Shape
from OCP.gp import gp_Pnt
from expression import Error, Ok
from expression.collections import Block
from msgspec import Struct
from rasm.contracts.rasm.contracts.cad.types_pb import TopologyCensus

from rasm.cad.faults import MEASURE_DEGENERATE, CadRail

# --- [TYPES] ----------------------------------------------------------------------------


class Dimension(IntEnum):
    SOLID = 3
    FACE = 2
    EDGE = 1


# --- [MODELS] ---------------------------------------------------------------------------


class _Rung(Struct, frozen=True):
    dimension: Dimension
    extent: Callable[[TopologyCensus], int]
    measure: Callable[[TopoDS_Shape], CadRail[tuple[float, gp_Pnt]]]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _admitted(shape: TopoDS_Shape, /) -> CadRail[TopoDS_Shape]:
    return Ok(shape) if not shape.IsNull() and BRepCheck_Analyzer(shape).IsValid() else Error(MEASURE_DEGENERATE.at("shape.invalid"))


def _finite(value: float, coordinate: str, /) -> CadRail[float]:
    return Ok(value) if isfinite(value) else Error(MEASURE_DEGENERATE.at(f"{coordinate}.non-finite"))


def _topology(shape: TopoDS_Shape, /) -> TopologyCensus:
    def extent(kind: TopAbs_ShapeEnum, /) -> int:
        carrier = TopTools_IndexedMapOfShape()
        TopExp.MapShapes_s(shape, kind, carrier)
        return carrier.Extent()

    return TopologyCensus(
        vertices=extent(TopAbs_VERTEX), edges=extent(TopAbs_EDGE), faces=extent(TopAbs_FACE), solids=extent(TopAbs_SOLID)
    )


def _accumulated(
    solids: TopTools_IndexedMapOfShape, /
) -> Callable[[tuple[float, float, float, float], int], tuple[float, float, float, float]]:
    def folded(carried: tuple[float, float, float, float], index: int, /) -> tuple[float, float, float, float]:
        properties = GProp_GProps()
        BRepGProp.VolumeProperties_s(TopoDS.Solid_s(solids.FindKey(index)), properties)
        mass, center = abs(float(properties.Mass())), properties.CentreOfMass()
        volume, x, y, z = carried
        return (volume + mass, x + mass * center.X(), y + mass * center.Y(), z + mass * center.Z())

    return folded


def _solid_properties(shape: TopoDS_Shape, /) -> CadRail[tuple[float, gp_Pnt]]:
    solids = TopTools_IndexedMapOfShape()
    TopExp.MapShapes_s(shape, TopAbs_SOLID, solids)
    moments = Block.of_seq(range(1, solids.Extent() + 1)).fold(_accumulated(solids), (0.0, 0.0, 0.0, 0.0))
    volume, x, y, z = moments
    return (
        Ok((volume, gp_Pnt(x / volume, y / volume, z / volume)))
        if volume > 0.0 and all(isfinite(value) for value in moments)
        else Error(MEASURE_DEGENERATE.at(f"solid.volume:{volume}"))
    )


def _massless(fill: Callable[[TopoDS_Shape, GProp_GProps], None], coordinate: str, /) -> Callable[[TopoDS_Shape], CadRail[tuple[float, gp_Pnt]]]:
    def measured(shape: TopoDS_Shape, /) -> CadRail[tuple[float, gp_Pnt]]:
        properties = GProp_GProps()
        fill(shape, properties)
        center = properties.CentreOfMass()
        return (
            Ok((0.0, center))
            if all(isfinite(value) for value in (center.X(), center.Y(), center.Z()))
            else Error(MEASURE_DEGENERATE.at(f"{coordinate}.centroid"))
        )

    return measured


# --- [POLICIES] -------------------------------------------------------------------------

_LADDER: Final[tuple[_Rung, ...]] = (
    _Rung(dimension=Dimension.SOLID, extent=lambda census: census.solids, measure=_solid_properties),
    _Rung(dimension=Dimension.FACE, extent=lambda census: census.faces, measure=_massless(BRepGProp.SurfaceProperties_s, "face")),
    _Rung(dimension=Dimension.EDGE, extent=lambda census: census.edges, measure=_massless(BRepGProp.LinearProperties_s, "edge")),
)


def _measured(shape: TopoDS_Shape, census: TopologyCensus, /) -> CadRail[tuple[float, gp_Pnt]]:
    return next(
        (rung.measure(shape) for rung in _LADDER if rung.extent(census) > 0),
        Error(MEASURE_DEGENERATE.at("shape.vertex-only")),
    )
```

## [03]-[RECEIPT]

- Owner: `receipt` — the one `BrepKernelReceipt` mint; every leg holding a shape reaches it and no leg rebuilds a field of it.
- Cases: `Closure` carries `unmeasured` for a leg that ran no mesher, `open` for a meshed non-closed body, and `closed` for a meshed closed body beside the emitted volume its delta measures against.
- Law: `_PRESENCE` mints `watertight` and `volume_delta_m3` together off one arm, so the wire's own CEL rule `has(volume_delta_m3) == (has(watertight) && this.watertight)` holds structurally instead of by convention at each caller.
- Law: the deleted form paired a nullable `watertight=` with a nullable `mesh_volume_m3=`, letting a caller spell three unmeanable combinations and forcing the B-rep leg to pass a pair it has no way to supply.
- Law: the delta compares exact volume against the ABSOLUTE emitted volume, so a mesh whose winding inverted reads as magnitude divergence rather than as a sign flip the wire's own `gte: 0` constraint rejects outright.
- Law: `BooleanProvenance` absence rides `Option` through the interior and collapses to the wire's own unset inside this mint alone, so no arm above the boundary ever reads a `None` provenance.
- Law: area admits through the same `_finite` gate as volume, so a garbage `Mass()` read refuses at its own coordinate instead of surfacing later as an opaque rejection of the wire's `finite` constraint; the deleted `if faces > 0` guard reconstructed a value the fold already returns.
- Growth: a new closure disposition is one `Closure` case beside one `_PRESENCE` row; a new receipt field derives inside this mint, so no caller grows an argument and no leg learns a second field.
- Boundary: this owner builds the value; publishing it as `ExecuteResponse.receipt` or `TessellateResponse.kernel` belongs to `service/provider#PROVIDER`.

```python
from collections.abc import Callable
from typing import Final, Literal

from builtins import frozendict
from OCP.BRepGProp import BRepGProp
from OCP.GProp import GProp_GProps
from OCP.TopoDS import TopoDS_Shape
from expression import Nothing, Option, case, effect, tag, tagged_union
from rasm.contracts.rasm.contracts.cad.types_pb import BooleanProvenance, BrepKernelReceipt
from rasm.contracts.rasm.contracts.spatial.vector_pb import Point3

from rasm.cad.faults import MEASURE_DEGENERATE, CadFault

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class Closure:
    tag: Literal["unmeasured", "open", "closed"] = tag()
    unmeasured: None = case()
    open: None = case()
    closed: float = case()


UNMEASURED: Final[Closure] = Closure(unmeasured=None)
OPEN: Final[Closure] = Closure(open=None)


# --- [POLICIES] -------------------------------------------------------------------------

_PRESENCE: Final[frozendict[str, Callable[[float, Closure], tuple[bool | None, float | None]]]] = frozendict({
    "unmeasured": lambda _exact, _closure: (None, None),
    "open": lambda _exact, _closure: (False, None),
    "closed": lambda exact, closure: (True, abs(exact - abs(closure.closed))),
})


# --- [OPERATIONS] -----------------------------------------------------------------------


@effect.result[BrepKernelReceipt, CadFault]()
def receipt(shape: TopoDS_Shape, closure: Closure = UNMEASURED, provenance: Option[BooleanProvenance] = Nothing, /):
    held = yield from _admitted(shape)
    census = _topology(held)
    volume_m3, center = yield from _measured(held, census)
    area = GProp_GProps()
    BRepGProp.SurfaceProperties_s(held, area)
    area_m2 = yield from _finite(abs(float(area.Mass())), "area")
    watertight, volume_delta_m3 = _PRESENCE[closure.tag](volume_m3, closure)
    return BrepKernelReceipt(
        volume_m3=volume_m3,
        area_m2=area_m2,
        centroid=Point3(x_m=center.X(), y_m=center.Y(), z_m=center.Z()),
        topology=census,
        watertight=watertight,
        volume_delta_m3=volume_delta_m3,
        boolean_provenance=provenance.default_value(None),
    )
```

## [04]-[RESEARCH]

- [INERTIA_ARM]-[OPEN]: which principal-axis, gyration-radius, and static-moment members ride `GProp_GProps` beside `MatrixOfInertia`, and what wire carrier lands them on the receipt for `geometry/ifc/structural#STRUCTURAL`; probe the installed `OCP.GProp` surface, seat the roster on `.api/cadquery-ocp.md`, then card the receipt field set.
- [MEASUREMENT_MEMBERS]-[OPEN]: does the catalogue's `BRepGProp` row cover `LinearProperties_s`, and do `TopTools_IndexedMapOfShape.Extent`/`FindKey` and `TopoDS.Solid_s` carry the spellings this fence transcribes; probe the installed `OCP` surface and seat each member on `.api/cadquery-ocp.md`.
