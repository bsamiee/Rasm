"""RGB and HSY grading curves of OCIO's linear curve grades, sampled into compositor curve tables beside native curve editors, with knots inserted from covered frames."""

from abc import abstractmethod
from bisect import bisect_left
import itertools
from typing import Annotated, ClassVar, override

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import msgspec.inspect
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.faults import BlenderFaults, BlenderResult, BlenderUnmetFault
from blender.bpy.nodes.graphs import CurvePoints, Expr, math, NodeGroup, Points, rgba, Scalar
from blender.bpy.nodes.interface import CurveAxes, CurveNode, item_key
from blender.bpy.render.compositor.colors import COLOR_INPUT, COLOR_RESULT, hsy, logarithmic
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Measures, Shell
from blender.bpy.render.compositor.tables import curved
from blender.bpy.render.compositor.viewers import Unread
from blender.bpy.settings.shapes import unwrapped
from blender.imaging.color.processors import converted, lin_to_log, luma
from blender.imaging.faults import ImagingResult
from blender.imaging.files.images import ImageFile, unassociated
from blender.records.faults import accumulated, paired
from blender.records.fields import bounds, Knot

from ...chain.stages import Effect, Stage

# --- [TYPES] ----------------------------------------------------------------------------

type Axis = Annotated[float, msgspec.Meta(ge=-7.0, le=7.0)]
type Hue = Annotated[float, msgspec.Meta(ge=0.0, lt=1.0)]
type MappedHue = Annotated[float, msgspec.Meta(ge=-1.0, le=2.0)]
type Chroma = Annotated[float, msgspec.Meta(ge=0.0, le=2.5)]
type Gain = Annotated[float, msgspec.Meta(ge=0.0, le=2.0)]
type ChannelCurve = Annotated[tuple[Knot[Axis, Axis], ...], msgspec.Meta(min_length=2, max_length=15), CurveNode(node=bpy.types.CompositorNodeCurveRGB)]
type HueCurve[X: float, Y: float] = Annotated[tuple[Knot[X, Y], ...], msgspec.Meta(min_length=2, max_length=8)]
type Bounds = tuple[tuple[float, float], tuple[float, float]]


# --- [MODELS] ---------------------------------------------------------------------------


class CurveGrade(Effect, frozen=True, kw_only=True):
    """Grade whose fields are OCIO B-spline knot curves, refused at decode when two knots share an input or OCIO rejects the curves."""

    pointwise = True

    def __post_init__(self) -> None:
        """Refuses curves holding coincident knot inputs or failing OCIO's validation."""
        if any(len({knot.x for knot in knots}) != len(knots) for knots in msgspec.structs.astuple(self)):
            raise ValueError(f"{self.title} curves hold one knot per input")
        try:
            self.checked()
        except PyOpenColorIO.Exception as error:
            raise ValueError(str(error)) from error

    # --- [SPLINES]

    @staticmethod
    def knots(curve: PyOpenColorIO.GradingBSplineCurve) -> tuple[Knot[float, float], ...]:
        """Reads a native spline's control points."""
        return tuple(Knot(x=point.x, y=point.y) for point in curve.getControlPoints())

    @staticmethod
    def spline(knots: tuple[Knot[float, float], ...]) -> PyOpenColorIO.GradingBSplineCurve:
        """Builds the native B-spline of knots."""
        return PyOpenColorIO.GradingBSplineCurve(list(itertools.chain.from_iterable(map(msgspec.structs.astuple, knots))))

    @property
    @abstractmethod
    def splines(self) -> tuple[PyOpenColorIO.GradingBSplineCurve, ...]:
        """Native splines of the fields in declaration order."""
        ...

    def checked(self) -> None:
        """Validates each native spline, raising OCIO's refusal."""
        for spline in self.splines:
            spline.validate()

    @staticmethod
    @abstractmethod
    def alone(spline: PyOpenColorIO.GradingBSplineCurve) -> PyOpenColorIO.Transform:
        """Transform evaluating one field's spline on each channel."""
        ...

    @abstractmethod
    def located(self, config: PyOpenColorIO.Config, mean: npt.NDArray[np.float32], tables: dict[str, Points]) -> ImagingResult[dict[str, float]]:
        """Inputs of every field at a scene-linear mean color."""
        ...

    # --- [TABLES]

    @classmethod
    def axes(cls) -> dict[str, Bounds]:
        """Input and output bounds of each knot field from its declared knot axes."""
        return {
            field.name: (bounds(x.type), bounds(y.type))
            for field in msgspec.structs.fields(cls)
            for shape in (unwrapped(msgspec.inspect.type_info(field.type)),)
            if isinstance(shape, msgspec.inspect.VarTupleType) and isinstance(shape.item_type, msgspec.inspect.StructType)
            for x, y in (shape.item_type.fields,)
        }

    @classmethod
    def clip(cls, *names: str) -> tuple[float, float, float, float]:
        """Curve view spanning the bounds of the named fields."""
        held = cls.axes()
        return (min(held[name][0][0] for name in names), min(held[name][1][0] for name in names), max(held[name][0][1] for name in names), max(held[name][1][1] for name in names))

    @staticmethod
    def sampled(table: Points, value: float) -> float:
        """Reads a sampled curve table at an input, linear between samples."""
        inputs, outputs = np.asarray(table, dtype=np.float64).T
        return float(np.interp(value, inputs, outputs))

    def tables(self, config: PyOpenColorIO.Config) -> BlenderResult[dict[str, Points]]:
        """Samples each field's spline over its input bounds through its transform's processor."""
        held = self.axes()
        return accumulated(
            curved(config, self.alone(spline), held[field.name][0]).map(lambda channels: (field.name, channels[0])) for field, spline in zip(msgspec.structs.fields(self), self.splines)
        ).map(dict)

    def editors(self) -> tuple[Expr, ...]:
        """Native curve editors holding the knots, fields marking one node class sharing its node under the first field's key in their axes, other fields each their own Float Curve."""
        marked, knots = CurveNode.marked(type(self)), dict(zip(self.__struct_fields__, msgspec.structs.astuple(self)))

        def shown(name: str, axes: CurveAxes) -> Points:
            return tuple((float(knot.x), axes.shown(knot.x, knot.y)) for knot in knots[name])

        def editor(node: type[bpy.types.Node], rows: tuple[tuple[str, CurveAxes], ...], view: Option[tuple[float, float, float, float]]) -> Expr:
            names = tuple(name for name, _ in rows)
            curve = CurvePoints(maps=(shown(*rows[0]), *itertools.starmap(shown, rows[1:])), clip=Some(view.default_with(lambda: self.clip(*names))), handle="AUTO", clipped=True)
            return Expr(kind=node, content=Some(curve)).named(item_key(names[0]))

        shared = tuple(
            editor(node, tuple((field.name, marker.axes) for field, marker, _ in marked if marker.node is node), next(marker.view for _, marker, _ in marked if marker.node is node))
            for node in dict.fromkeys(marker.node for _, marker, _ in marked)
        )
        named = frozenset(field.name for field, _, _ in marked)
        return (*shared, *(editor(bpy.types.ShaderNodeFloatCurve, ((name, CurveAxes()),), Nothing) for name in self.axes() if name not in named))

    # --- [MEASURE]

    @staticmethod
    def measured[R: CurveGrade](config: PyOpenColorIO.Config, record: R, file: ImageFile[np.float32]) -> BlenderResult[Option[R]]:
        """Inserts a knot on each field's sampled curve at the covered scene-linear mean of the entering frame, refusing a frame that adds none."""
        unread = (BlenderUnmetFault(requirement=Unread(entry=record.title)),)
        straight = unassociated(file.pixels)
        covered = straight[..., :3][straight[..., 3] > 0.0] if straight.shape[-1] == 4 else straight[..., :3].reshape(-1, 3)
        if not covered.size:
            return Error(unread)

        def inserted(tables: dict[str, Points], at: dict[str, float]) -> BlenderResult[Option[R]]:
            convert = catch(msgspec.convert, exception=msgspec.ValidationError)

            def knotted(field: msgspec.structs.FieldInfo, knots: tuple[Knot[float, float], ...]) -> Option[object]:
                value = at[field.name]
                position = bisect_left(knots, value, key=lambda knot: knot.x)
                return (
                    Nothing
                    if position < len(knots) and knots[position].x == value
                    else convert((*knots[:position], (value, CurveGrade.sampled(tables[field.name], value)), *knots[position:]), field.type).to_option()
                )

            changes = {field.name: held for field, knots in zip(msgspec.structs.fields(record), msgspec.structs.astuple(record)) for held in knotted(field, knots).to_list()}
            return Ok(Some(msgspec.structs.replace(record, **changes))) if changes else Error(unread)

        def measuring(tables: dict[str, Points]) -> BlenderResult[Option[R]]:
            located = converted(config, PyOpenColorIO.ColorSpaceTransform(src=file.colorspace, dst=PyOpenColorIO.ROLE_SCENE_LINEAR), covered).bind(
                lambda linear: record.located(config, linear.mean(axis=0, dtype=np.float64).astype(np.float32), tables)
            )
            return Result[dict[str, float], BlenderFaults].bind(located, lambda at: inserted(tables, at))

        return record.tables(config).bind(measuring)


class Curves(CurveGrade, frozen=True, kw_only=True):
    """Maps RGB channels, then all channels, on OCIO's scene-linear stop axis."""

    stage = Stage.CURVES
    neutral: ClassVar[PyOpenColorIO.GradingRGBCurve] = PyOpenColorIO.GradingRGBCurve(PyOpenColorIO.GRADING_LIN)
    red: Annotated[ChannelCurve, msgspec.Meta(description="Red output against red input")] = CurveGrade.knots(neutral.red)
    green: Annotated[ChannelCurve, msgspec.Meta(description="Green output against green input")] = CurveGrade.knots(neutral.green)
    blue: Annotated[ChannelCurve, msgspec.Meta(description="Blue output against blue input")] = CurveGrade.knots(neutral.blue)
    master: Annotated[ChannelCurve, msgspec.Meta(description="All channels after the channel curves")] = CurveGrade.knots(neutral.master)

    @property
    @override
    def splines(self) -> tuple[PyOpenColorIO.GradingBSplineCurve, ...]:
        """Channel splines, then the master spline."""
        return tuple(map(self.spline, msgspec.structs.astuple(self)))

    @staticmethod
    @override
    def alone(spline: PyOpenColorIO.GradingBSplineCurve) -> PyOpenColorIO.Transform:
        """Master-only RGB curve grade on the stop axis itself."""
        transform = PyOpenColorIO.GradingRGBCurveTransform(values=PyOpenColorIO.GradingRGBCurve(master=spline), style=PyOpenColorIO.GRADING_LIN)
        transform.setBypassLinToLog(True)
        return transform

    @override
    def transform(self, _config: PyOpenColorIO.Config, /) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns OCIO's linear RGB curve grade."""
        return Ok(Some(PyOpenColorIO.GradingRGBCurveTransform(values=PyOpenColorIO.GradingRGBCurve(*self.splines), style=PyOpenColorIO.GRADING_LIN)))

    @override
    def located(self, config: PyOpenColorIO.Config, mean: npt.NDArray[np.float32], tables: dict[str, Points]) -> ImagingResult[dict[str, float]]:
        """Channel inputs at the mean's stops, the master input at the stops of the channel-graded mean's luminance."""
        *channels, master = self.__struct_fields__
        log, raw = lin_to_log(), PyOpenColorIO.Config()

        def solved(entering: npt.NDArray[np.float32], weights: npt.NDArray[np.float64]) -> ImagingResult[dict[str, float]]:
            graded = np.asarray([self.sampled(tables[name], float(value)) for name, value in zip(channels, entering)], dtype=np.float32)
            return (
                converted(raw, PyOpenColorIO.GroupTransform([log], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE), graded)
                .bind(lambda linear: converted(raw, log, np.full(3, float(linear @ weights), dtype=np.float32)))
                .map(lambda stops: dict(zip(channels, map(float, entering))) | {master: float(stops[0])})
            )

        return paired(converted(raw, log, mean), luma(config)).bind(lambda pair: solved(*pair))


class HueCurves(CurveGrade, frozen=True, kw_only=True):
    """Maps hue, saturation, and luma through OCIO's scene-linear HSY grade."""

    stage = Stage.HUE_CURVES
    neutral: ClassVar[PyOpenColorIO.GradingHueCurve] = PyOpenColorIO.GradingHueCurve(PyOpenColorIO.GRADING_LIN)  # type: ignore[name-defined, attr-defined]  # ty: ignore[unresolved-attribute]
    hue_hue: Annotated[
        HueCurve[Hue, MappedHue],
        CurveNode(node=bpy.types.CompositorNodeHueCorrect, axes=CurveAxes(slope=-1.0, offset=0.5), view=Some((0.0, 0.0, 1.0, 1.0))),
        msgspec.Meta(title="Hue vs Hue", description="Output hue against input hue"),
    ] = CurveGrade.knots(neutral.hue_hue)
    hue_sat: Annotated[
        HueCurve[Hue, Gain], CurveNode(node=bpy.types.CompositorNodeHueCorrect, axes=CurveAxes(scale=0.5)), msgspec.Meta(title="Hue vs Saturation", description="Saturation gain against input hue")
    ] = CurveGrade.knots(neutral.hue_sat)
    hue_lum: Annotated[
        HueCurve[Hue, Gain], CurveNode(node=bpy.types.CompositorNodeHueCorrect, axes=CurveAxes(scale=0.5)), msgspec.Meta(title="Hue vs Luma", description="Luma gain against input hue")
    ] = CurveGrade.knots(neutral.hue_lum)
    lum_sat: Annotated[HueCurve[Axis, Gain], msgspec.Meta(title="Luma vs Saturation", description="Saturation gain against input luma")] = CurveGrade.knots(neutral.lum_sat)
    sat_sat: Annotated[HueCurve[Chroma, Chroma], msgspec.Meta(title="Saturation vs Saturation", description="Output saturation against input saturation")] = CurveGrade.knots(neutral.sat_sat)
    lum_lum: Annotated[HueCurve[Axis, Axis], msgspec.Meta(title="Luma vs Luma", description="Output luma against input luma")] = CurveGrade.knots(neutral.lum_lum)
    sat_lum: Annotated[HueCurve[Chroma, Gain], msgspec.Meta(title="Saturation vs Luma", description="Luma gain against output saturation")] = CurveGrade.knots(neutral.sat_lum)

    @property
    @override
    def splines(self) -> tuple[PyOpenColorIO.GradingBSplineCurve, ...]:
        """Splines of each field typed as the neutral grade's curve in the same OCIO position."""
        held = self.neutral
        kinds = (held.hue_hue, held.hue_sat, held.hue_lum, held.lum_sat, held.sat_sat, held.lum_lum, held.sat_lum)
        splines = tuple(map(self.spline, msgspec.structs.astuple(self)))
        for spline, kind in zip(splines, kinds):
            spline.setSplineType(kind.getSplineType())  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        return splines

    @override
    def checked(self) -> None:
        """Validates the native hue curve grade."""
        self.values().validate()

    def values(self) -> PyOpenColorIO.GradingHueCurve:  # type: ignore[name-defined]  # ty: ignore[unresolved-attribute]
        """Native hue curve grade, hue folding at the neutral grade's own."""
        return PyOpenColorIO.GradingHueCurve(*self.splines, self.neutral.hue_fx)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

    @staticmethod
    @override
    def alone(spline: PyOpenColorIO.GradingBSplineCurve) -> PyOpenColorIO.Transform:
        """Hue grade drawing the spline alone, held in a slot retyped to the spline's kind."""
        grade = PyOpenColorIO.GradingHueCurve(PyOpenColorIO.GRADING_LIN)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        grade.hue_sat.setSplineType(spline.getSplineType())  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        grade.hue_sat = spline
        grade.setDrawCurveOnly(True)
        return PyOpenColorIO.GradingHueCurveTransform(values=grade, style=PyOpenColorIO.GRADING_LIN)  # type: ignore[attr-defined, no-any-return]  # ty: ignore[unresolved-attribute]

    @override
    def transform(self, _config: PyOpenColorIO.Config, /) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns OCIO's linear hue curve grade."""
        return Ok(Some(PyOpenColorIO.GradingHueCurveTransform(values=self.values(), style=PyOpenColorIO.GRADING_LIN)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

    @override
    def located(self, config: PyOpenColorIO.Config, mean: npt.NDArray[np.float32], tables: dict[str, Points]) -> ImagingResult[dict[str, float]]:
        """Hue inputs at the mean's hue, luma inputs at its luma stops, saturation input at its saturation, and the luma-gain input at the saturation the grade outputs."""
        hue_hue, hue_sat, hue_lum, lum_sat, sat_sat, lum_lum, sat_lum = self.__struct_fields__
        raw = PyOpenColorIO.Config()

        def at(hue: float, saturation: float, stop: float) -> dict[str, float]:
            inputs = {hue_hue: hue, hue_sat: hue, hue_lum: hue, lum_sat: stop, sat_sat: saturation, lum_lum: stop}
            gains = (max(0.0, self.sampled(tables[name], inputs[name])) for name in (sat_sat, lum_sat, hue_sat))
            return inputs | {sat_lum: float(np.prod(tuple(gains)))}

        return converted(raw, PyOpenColorIO.FixedFunctionTransform(PyOpenColorIO.FIXED_FUNCTION_RGB_TO_HSY_LIN), mean).bind(  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            lambda spaced: converted(raw, lin_to_log(), np.full(3, spaced[2], dtype=np.float32)).map(lambda stops: at(float(spaced[0]), float(spaced[1]), float(stops[0])))
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def channel_mapped(shell: Shell[Curves]) -> BlenderResult[Expr]:
    """Applies channel tables, then the master table, on the stop axis, beside the curve editors."""
    *channels, master = Curves.__struct_fields__
    (low, high), _ = Curves.axes()[master]
    identity: Points = ((low, low), (high, high))

    def table(key: str, image: Expr, maps: tuple[Points, Points, Points, Points]) -> Expr:
        return Expr(
            kind=bpy.types.CompositorNodeCurveRGB,
            inputs=(("Image", image), ("Fac", 1.0), ("Black Level", (0.0, 0.0, 0.0, 1.0)), ("White Level", (1.0, 1.0, 1.0, 1.0))),
            members=(("mapping.tone", "STANDARD"),),
            content=Some(CurvePoints(maps=maps, clip=Some((low, low, high, high)))),
            socket="Image",
            shape="RGBA",
        ).named(key)

    def built(tables: dict[str, Points], groups: tuple[NodeGroup, ...]) -> Expr:
        forward, inverse = groups
        red, green, blue = (tables[name] for name in channels)
        stops = table(
            "master_table", table("channel_table", forward.placed({COLOR_INPUT.key: shell.color}, COLOR_RESULT.key), (red, green, blue, identity)), (identity, identity, identity, tables[master])
        )
        return inverse.placed({COLOR_INPUT.key: stops}, COLOR_RESULT.key).keeping(*shell.record.editors())

    return paired(shell.record.tables(shell.config), accumulated(map(logarithmic, (PyOpenColorIO.TRANSFORM_DIR_FORWARD, PyOpenColorIO.TRANSFORM_DIR_INVERSE)))).map(lambda pair: built(*pair))


def hue_mapped(shell: Shell[HueCurves]) -> BlenderResult[Expr]:
    """Maps HSY through the curve tables in OCIO's saturation then luma order, beside the curve editors."""
    hue_hue, hue_sat, hue_lum, lum_sat, sat_sat, lum_lum, sat_lum = HueCurves.__struct_fields__

    def built(tables: dict[str, Points], groups: tuple[NodeGroup, ...]) -> Expr:
        forward, inverse, encoded, decoded = groups

        def table(key: str, value: Scalar) -> Expr:
            return Expr(kind=bpy.types.ShaderNodeFloatCurve, inputs=(("Factor", 1.0), ("Value", value)), content=Some(CurvePoints(maps=(tables[key],), clip=Some(HueCurves.clip(key))))).named(
                item_key(key, "table")
            )

        spaced = forward.placed({COLOR_INPUT.key: shell.color}, COLOR_RESULT.key)
        hue, saturation, stops = spaced["Red"], spaced["Green"], encoded.placed({COLOR_INPUT.key: spaced}, COLOR_RESULT.key)["Blue"]
        gains = {key: math("MAXIMUM", table(key, value), 0.0) for key, value in ((hue_sat, hue), (hue_lum, hue), (lum_sat, stops), (sat_sat, saturation))}
        grown = gains[sat_sat] * gains[lum_sat] * gains[hue_sat]
        lightness = (
            decoded.placed({COLOR_INPUT.key: table(lum_lum, stops)}, COLOR_RESULT.key)["Blue"]
            * (1.0 - (1.0 - gains[hue_lum]) * math("MINIMUM", grown, 1.0))
            * math("MAXIMUM", table(sat_lum, grown), 0.0)
        )
        return inverse.placed({COLOR_INPUT.key: rgba(math("FRACT", table(hue_hue, hue)), grown, lightness)}, COLOR_RESULT.key).keeping(*shell.record.editors())

    return paired(
        shell.record.tables(shell.config),
        accumulated((
            hsy(shell.config, PyOpenColorIO.TRANSFORM_DIR_FORWARD),
            hsy(shell.config, PyOpenColorIO.TRANSFORM_DIR_INVERSE),
            logarithmic(PyOpenColorIO.TRANSFORM_DIR_FORWARD),
            logarithmic(PyOpenColorIO.TRANSFORM_DIR_INVERSE),
        )),
    ).map(lambda pair: built(*pair))


# --- [COMPOSITION] ----------------------------------------------------------------------

curves = CompositorEffect(record=Curves, graph=channel_mapped, alpha=Alpha.STRAIGHT, parts=(Measures(revise=Curves.measured),))
hue_curves = CompositorEffect(record=HueCurves, graph=hue_mapped, alpha=Alpha.STRAIGHT, parts=(Measures(revise=HueCurves.measured),))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CurveGrade", "Curves", "HueCurves", "curves", "hue_curves"]
