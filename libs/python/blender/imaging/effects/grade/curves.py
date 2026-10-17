"""Native RGB and HSY grading curves, raw-knot admission, and display samples."""

import itertools
from typing import Annotated, ClassVar, override

from expression import Error, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.imaging.chain import Effect, Stage
from blender.imaging.color.processors import applied
from blender.imaging.faults import colored, ImagingCoincidentFault, ImagingResult, paired
from blender.imaging.parameters import Knot

# --- [TYPES] ----------------------------------------------------------------------------

type Axis = Annotated[float, msgspec.Meta(ge=-7.0, le=7.0)]
type Hue = Annotated[float, msgspec.Meta(ge=0.0, lt=1.0)]
type MappedHue = Annotated[float, msgspec.Meta(ge=-1.0, le=2.0)]
type HsySaturation = Annotated[float, msgspec.Meta(ge=0.0, le=2.5)]
type Gain = Annotated[float, msgspec.Meta(ge=0.0, le=2.0)]
type ChannelCurve = Annotated[tuple[Knot[Axis, Axis], ...], msgspec.Meta(min_length=2, max_length=15)]
type HueCurve[X: float, Y: float] = Annotated[tuple[Knot[X, Y], ...], msgspec.Meta(min_length=2, max_length=8)]

# --- [MODELS] ---------------------------------------------------------------------------


class Curves(Effect, frozen=True, kw_only=True, tag=16):
    """Maps RGB channels and their combined output on OCIO's scene-linear stop axis."""

    stage = Stage.CURVES
    pointwise = True
    neutral: ClassVar[PyOpenColorIO.GradingRGBCurve] = PyOpenColorIO.GradingRGBCurve(PyOpenColorIO.GRADING_LIN)
    red: Annotated[ChannelCurve, msgspec.Meta(description="Red output against red input")] = tuple(Knot(x=point.x, y=point.y) for point in neutral.red.getControlPoints())
    green: Annotated[ChannelCurve, msgspec.Meta(description="Green output against green input")] = tuple(Knot(x=point.x, y=point.y) for point in neutral.green.getControlPoints())
    blue: Annotated[ChannelCurve, msgspec.Meta(description="Blue output against blue input")] = tuple(Knot(x=point.x, y=point.y) for point in neutral.blue.getControlPoints())
    master: Annotated[ChannelCurve, msgspec.Meta(description="Every channel against its channel curve's output")] = tuple(Knot(x=point.x, y=point.y) for point in neutral.master.getControlPoints())

    def curves(self) -> tuple[PyOpenColorIO.GradingBSplineCurve, ...]:
        """Builds native channel splines in declaration order."""
        return tuple(PyOpenColorIO.GradingBSplineCurve(list(itertools.chain.from_iterable(map(msgspec.structs.astuple, knots)))) for knots in msgspec.structs.astuple(self))

    def values(self) -> PyOpenColorIO.GradingRGBCurve:
        """Builds the native grade over RGB and master splines."""
        return PyOpenColorIO.GradingRGBCurve(*self.curves())

    @override
    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        return Ok(Some(PyOpenColorIO.GradingRGBCurveTransform(values=self.values(), style=PyOpenColorIO.GRADING_LIN)))


class HueCurves(Effect, frozen=True, kw_only=True, tag=17):
    """Maps hue, saturation, and luma through OCIO's scene-linear HSY grade."""

    stage = Stage.HUE_CURVES
    pointwise = True
    neutral: ClassVar[PyOpenColorIO.GradingHueCurve] = PyOpenColorIO.GradingHueCurve(PyOpenColorIO.GRADING_LIN)  # type: ignore[name-defined, attr-defined]  # ty: ignore[unresolved-attribute]
    hue_hue: Annotated[HueCurve[Hue, MappedHue], msgspec.Meta(title="Hue vs Hue", description="Output hue against input hue")] = tuple(
        Knot(x=point.x, y=point.y) for point in neutral.hue_hue.getControlPoints()
    )
    hue_sat: Annotated[HueCurve[Hue, Gain], msgspec.Meta(title="Hue vs Saturation", description="Saturation gain against input hue")] = tuple(
        Knot(x=point.x, y=point.y) for point in neutral.hue_sat.getControlPoints()
    )
    hue_lum: Annotated[HueCurve[Hue, Gain], msgspec.Meta(title="Hue vs Luma", description="Luma gain against input hue")] = tuple(
        Knot(x=point.x, y=point.y) for point in neutral.hue_lum.getControlPoints()
    )
    lum_sat: Annotated[HueCurve[Axis, Gain], msgspec.Meta(title="Luma vs Saturation", description="Saturation gain against input luma")] = tuple(
        Knot(x=point.x, y=point.y) for point in neutral.lum_sat.getControlPoints()
    )
    sat_sat: Annotated[HueCurve[HsySaturation, HsySaturation], msgspec.Meta(title="Saturation vs Saturation", description="Output saturation against input saturation")] = tuple(
        Knot(x=point.x, y=point.y) for point in neutral.sat_sat.getControlPoints()
    )
    lum_lum: Annotated[HueCurve[Axis, Axis], msgspec.Meta(title="Luma vs Luma", description="Output luma against input luma")] = tuple(
        Knot(x=point.x, y=point.y) for point in neutral.lum_lum.getControlPoints()
    )
    sat_lum: Annotated[HueCurve[HsySaturation, Gain], msgspec.Meta(title="Saturation vs Luma", description="Luma gain against output saturation")] = tuple(
        Knot(x=point.x, y=point.y) for point in neutral.sat_lum.getControlPoints()
    )

    def curves(self) -> tuple[PyOpenColorIO.GradingBSplineCurve, ...]:
        """Builds each declared curve with its corresponding native hue kind."""
        return tuple(
            PyOpenColorIO.GradingBSplineCurve(list(itertools.chain.from_iterable(map(msgspec.structs.astuple, knots))), PyOpenColorIO.HueCurveType.__members__[field.name.upper()])  # type: ignore[call-overload, attr-defined]  # ty: ignore[no-matching-overload, unresolved-attribute]
            for field, knots in zip(msgspec.structs.fields(type(self)), msgspec.structs.astuple(self))
        )

    def values(self) -> PyOpenColorIO.GradingHueCurve:  # type: ignore[name-defined]  # ty: ignore[unresolved-attribute]
        """Builds the native grade with hue folding at its native identity."""
        return PyOpenColorIO.GradingHueCurve(*self.curves(), self.neutral.hue_fx)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

    @override
    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        return Ok(Some(PyOpenColorIO.GradingHueCurveTransform(values=self.values(), style=PyOpenColorIO.GRADING_LIN)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]


# --- [OPERATIONS] -----------------------------------------------------------------------


def admitted[R: (Curves, HueCurves)](record: R) -> ImagingResult[R]:
    """Admits raw knots against native spline ordering and coincident native inputs."""
    curves = record.curves()
    native: ImagingResult[object] = colored(
        record,
        lambda: PyOpenColorIO.GradingRGBCurve(*curves).validate() if isinstance(record, Curves) else PyOpenColorIO.GradingHueCurve(*curves, HueCurves.neutral.hue_fx),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    )
    coincident: ImagingResult[None] = (
        Error((ImagingCoincidentFault(curve=record),)) if any(len({point.x for point in curve.getControlPoints()}) != len(curve.getControlPoints()) for curve in curves) else Ok(None)
    )
    return paired(native, coincident).map(lambda _: record)


def sampled(curve: PyOpenColorIO.GradingBSplineCurve, count: int, axis: tuple[float, float]) -> npt.NDArray[np.float32]:
    """Samples a native spline as float32 input/output rows over the supplied axis."""
    transform: PyOpenColorIO.Transform
    match curve.getSplineType():  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        case PyOpenColorIO.BSplineType.B_SPLINE:  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            transform = PyOpenColorIO.GradingRGBCurveTransform(values=PyOpenColorIO.GradingRGBCurve(master=curve), style=PyOpenColorIO.GRADING_LIN)
            transform.setBypassLinToLog(True)
        case kind:
            graph = PyOpenColorIO.GradingHueCurve(PyOpenColorIO.GRADING_LIN)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            graph.hue_sat.setSplineType(kind)
            graph.hue_sat = curve
            graph.setDrawCurveOnly(True)
            transform = PyOpenColorIO.GradingHueCurveTransform(values=graph, style=PyOpenColorIO.GRADING_LIN)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    inputs = np.linspace(*axis, count, dtype=np.float32)
    outputs = applied(PyOpenColorIO.Config().getProcessor(transform).getOptimizedCPUProcessor(PyOpenColorIO.OPTIMIZATION_NONE), np.repeat(inputs[:, np.newaxis], 3, axis=1))[:, 0]
    return np.stack((inputs, outputs), axis=-1)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Axis", "ChannelCurve", "Curves", "Gain", "HsySaturation", "Hue", "HueCurve", "HueCurves", "MappedHue", "admitted", "sampled"]
