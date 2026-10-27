"""RGB and HSY grading curves of OCIO's linear curve grades, sampled into compositor curve tables beside native curve editors, with knots inserted from covered frames."""

from bisect import bisect_left
from collections.abc import Callable, Mapping
from functools import partial
from typing import Annotated, Any, ClassVar, Final

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.operators import Answer, Call, Done, Operator
from blender.bpy.data.rna.conditions import Job
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.graphs import CurvePoints, Expr, math, NodeGroup, Points, rgba, Scalar
from blender.bpy.nodes.interface import CurveAxes, item_key
from blender.bpy.render.compositor.colors import COLOR_INPUT, COLOR_RESULT, hsy, logarithmic
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Named, Registers, revised, Shell
from blender.bpy.render.compositor.tables import curved
from blender.bpy.render.compositor.viewers import READ
from blender.imaging.color.processors import converted, lin_to_log, luma
from blender.imaging.files.buffers import unassociated
from blender.imaging.files.images import ImageFile
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.records.fields import Knot

from ...chain.stages import Effect, Stage, transform

# --- [TYPES] ----------------------------------------------------------------------------

type CurveAxis = Annotated[float, msgspec.Meta(ge=-7.0, le=7.0)]
type Hue = Annotated[float, msgspec.Meta(ge=0.0, lt=1.0)]
type MappedHue = Annotated[float, msgspec.Meta(ge=-1.0, le=2.0)]
type Chroma = Annotated[float, msgspec.Meta(ge=0.0, le=2.5)]
type Gain = Annotated[float, msgspec.Meta(ge=0.0, le=2.0)]
type ChannelCurve = Annotated[tuple[Knot[CurveAxis, CurveAxis], ...], msgspec.Meta(min_length=2, max_length=15)]
type HueCurve[X: float, Y: float] = Annotated[tuple[Knot[X, Y], ...], msgspec.Meta(min_length=2, max_length=8)]
type Knots = tuple[Knot[float, float], ...]
type ChannelCurveSet = tuple[ChannelCurve, ChannelCurve, ChannelCurve, ChannelCurve]
type HueCurveSet = tuple[
    HueCurve[Hue, MappedHue], HueCurve[Hue, Gain], HueCurve[Hue, Gain], HueCurve[CurveAxis, Gain], HueCurve[Chroma, Chroma], HueCurve[CurveAxis, CurveAxis], HueCurve[Chroma, Gain]
]

# --- [CONSTANTS] ------------------------------------------------------------------------

_NEUTRAL_RGB: Final = PyOpenColorIO.GradingRGBCurve(PyOpenColorIO.GRADING_LIN)
_HUE_CURVE: Final[type[Any]] = PyOpenColorIO.GradingHueCurve  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
_HUE_TRANSFORM: Final[Callable[..., PyOpenColorIO.Transform]] = PyOpenColorIO.GradingHueCurveTransform  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
_SPLINE_OF: Final[Callable[[PyOpenColorIO.GradingBSplineCurve], Any]] = PyOpenColorIO.GradingBSplineCurve.getSplineType  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
_SPLINE_SET: Final[Callable[[PyOpenColorIO.GradingBSplineCurve, Any], None]] = PyOpenColorIO.GradingBSplineCurve.setSplineType  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
_NEUTRAL_HUE: Final = _HUE_CURVE(PyOpenColorIO.GRADING_LIN)

# --- [MODELS] ---------------------------------------------------------------------------


class Curve(msgspec.Struct, frozen=True, kw_only=True):
    """Knot curve of a grade: its key, input and output bounds, and the native editor holding it in the editor's own axes."""

    key: Key
    inputs: tuple[float, float]
    outputs: tuple[float, float]
    node: type[bpy.types.Node] = bpy.types.ShaderNodeFloatCurve
    axes: CurveAxes = CurveAxes()
    view: Option[tuple[float, float, float, float]] = Nothing


class Curves(Effect, frozen=True, kw_only=True):
    """Maps RGB channels, then all channels, on OCIO's scene-linear stop axis, the curves refused at decode when two knots share an input or OCIO rejects them."""

    stage = Stage.CURVES
    pointwise = True
    rows: ClassVar[tuple[Curve, ...]] = tuple(Curve(key=key, inputs=(-7.0, 7.0), outputs=(-7.0, 7.0), node=bpy.types.CompositorNodeCurveRGB) for key in ("red", "green", "blue", "master"))
    curves: ChannelCurveSet = msgspec.convert(
        tuple(tuple(curve.getControlPoints()) for curve in (_NEUTRAL_RGB.red, _NEUTRAL_RGB.green, _NEUTRAL_RGB.blue, _NEUTRAL_RGB.master)), ChannelCurveSet, from_attributes=True
    )

    def __post_init__(self) -> None:
        """Refuses curves holding coincident knot inputs or failing OCIO's validation."""
        if any(len({knot.x for knot in held}) != len(held) for held in self.curves):
            raise ValueError(f"{type(self).__name__} curves hold one knot per input")
        try:
            for held in self.splines:
                held.validate()
        except PyOpenColorIO.Exception as error:
            raise ValueError(str(error)) from error

    @property
    def splines(self) -> tuple[PyOpenColorIO.GradingBSplineCurve, ...]:
        """Channel splines, then the master spline."""
        return tuple(map(spline, self.curves))

    @staticmethod
    def alone(curve: PyOpenColorIO.GradingBSplineCurve) -> PyOpenColorIO.Transform:
        """Master-only RGB curve grade on the stop axis itself."""
        operation = PyOpenColorIO.GradingRGBCurveTransform(values=PyOpenColorIO.GradingRGBCurve(master=curve), style=PyOpenColorIO.GRADING_LIN)
        operation.setBypassLinToLog(True)
        return operation

    def located(self, config: PyOpenColorIO.Config, mean: npt.NDArray[np.float32], held: Mapping[Key, Points]) -> Outcome[dict[Key, float]]:
        """Channel inputs at the mean's stops, the master input at the stops of the channel-graded mean's luminance."""
        *channels, master = (row.key for row in self.rows)
        log, raw = lin_to_log(), PyOpenColorIO.Config()

        def solved(entering: npt.NDArray[np.float32], weights: npt.NDArray[np.float64]) -> Outcome[dict[Key, float]]:
            graded = np.asarray([read(held[name], float(value)) for name, value in zip(channels, entering)], dtype=np.float32)
            return (
                converted(raw, PyOpenColorIO.GroupTransform([log], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE), graded)
                .bind(lambda linear: converted(raw, log, np.full(3, float(linear @ weights), dtype=np.float32)))
                .map(lambda stops: dict(zip(channels, map(float, entering))) | {master: float(stops[0])})
            )

        return paired(converted(raw, log, mean), luma(config)).bind(lambda pair: solved(*pair))


class HueCurves(Effect, frozen=True, kw_only=True):
    """Maps hue, saturation, and luma through OCIO's scene-linear HSY grade, the curves refused at decode when two knots share an input or OCIO rejects them."""

    stage = Stage.HUE_CURVES
    pointwise = True
    rows: ClassVar[tuple[Curve, ...]] = (
        Curve(key="hue_hue", inputs=(0.0, 1.0), outputs=(-1.0, 2.0), node=bpy.types.CompositorNodeHueCorrect, axes=CurveAxes(slope=-1.0, offset=0.5), view=Some((0.0, 0.0, 1.0, 1.0))),
        Curve(key="hue_sat", inputs=(0.0, 1.0), outputs=(0.0, 2.0), node=bpy.types.CompositorNodeHueCorrect, axes=CurveAxes(scale=0.5)),
        Curve(key="hue_lum", inputs=(0.0, 1.0), outputs=(0.0, 2.0), node=bpy.types.CompositorNodeHueCorrect, axes=CurveAxes(scale=0.5)),
        Curve(key="lum_sat", inputs=(-7.0, 7.0), outputs=(0.0, 2.0)),
        Curve(key="sat_sat", inputs=(0.0, 2.5), outputs=(0.0, 2.5)),
        Curve(key="lum_lum", inputs=(-7.0, 7.0), outputs=(-7.0, 7.0)),
        Curve(key="sat_lum", inputs=(0.0, 2.5), outputs=(0.0, 2.0)),
    )
    curves: HueCurveSet = msgspec.convert(
        tuple(
            tuple(curve.getControlPoints())
            for curve in (_NEUTRAL_HUE.hue_hue, _NEUTRAL_HUE.hue_sat, _NEUTRAL_HUE.hue_lum, _NEUTRAL_HUE.lum_sat, _NEUTRAL_HUE.sat_sat, _NEUTRAL_HUE.lum_lum, _NEUTRAL_HUE.sat_lum)
        ),
        HueCurveSet,
        from_attributes=True,
    )

    def __post_init__(self) -> None:
        """Refuses curves holding coincident knot inputs or failing OCIO's validation of the hue grade."""
        if any(len({knot.x for knot in held}) != len(held) for held in self.curves):
            raise ValueError(f"{type(self).__name__} curves hold one knot per input")
        try:
            self.grade.validate()
        except PyOpenColorIO.Exception as error:
            raise ValueError(str(error)) from error

    @property
    def splines(self) -> tuple[PyOpenColorIO.GradingBSplineCurve, ...]:
        """Splines of each curve typed as the neutral grade's curve in the same OCIO position."""
        kinds = (_NEUTRAL_HUE.hue_hue, _NEUTRAL_HUE.hue_sat, _NEUTRAL_HUE.hue_lum, _NEUTRAL_HUE.lum_sat, _NEUTRAL_HUE.sat_sat, _NEUTRAL_HUE.lum_lum, _NEUTRAL_HUE.sat_lum)
        splines = tuple(map(spline, self.curves))
        for held, kind in zip(splines, kinds):
            _SPLINE_SET(held, _SPLINE_OF(kind))
        return splines

    @property
    def grade(self) -> PyOpenColorIO.Transform:
        """Native linear hue curve grade, hue folding at the neutral grade's own."""
        return _HUE_TRANSFORM(values=_HUE_CURVE(*self.splines, _NEUTRAL_HUE.hue_fx), style=PyOpenColorIO.GRADING_LIN)

    @staticmethod
    def alone(curve: PyOpenColorIO.GradingBSplineCurve) -> PyOpenColorIO.Transform:
        """Hue grade drawing the spline alone, held in a slot retyped to the spline's kind."""
        grade = _HUE_CURVE(PyOpenColorIO.GRADING_LIN)
        _SPLINE_SET(grade.hue_sat, _SPLINE_OF(curve))
        grade.hue_sat = curve
        grade.setDrawCurveOnly(True)
        return _HUE_TRANSFORM(values=grade, style=PyOpenColorIO.GRADING_LIN)

    def located(self, _config: PyOpenColorIO.Config, mean: npt.NDArray[np.float32], held: Mapping[Key, Points]) -> Outcome[dict[Key, float]]:
        """Hue inputs at the mean's hue, luma inputs at its luma stops, saturation input at its saturation, and the luma-gain input at the saturation the grade outputs."""
        hue_hue, hue_sat, hue_lum, lum_sat, sat_sat, lum_lum, sat_lum = (row.key for row in self.rows)
        raw = PyOpenColorIO.Config()

        def at(hue: float, saturation: float, stop: float) -> dict[Key, float]:
            inputs = {hue_hue: hue, hue_sat: hue, hue_lum: hue, lum_sat: stop, sat_sat: saturation, lum_lum: stop}
            gains = (max(0.0, read(held[name], inputs[name])) for name in (sat_sat, lum_sat, hue_sat))
            return inputs | {sat_lum: float(np.prod(tuple(gains)))}

        return converted(raw, PyOpenColorIO.FixedFunctionTransform(PyOpenColorIO.FIXED_FUNCTION_RGB_TO_HSY_LIN), mean).bind(  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            lambda spaced: converted(raw, lin_to_log(), np.full(3, spaced[2], dtype=np.float32)).map(lambda stops: at(float(spaced[0]), float(spaced[1]), float(stops[0])))
        )


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SPLINES]


def spline(held: Knots) -> PyOpenColorIO.GradingBSplineCurve:
    """Builds the native B-spline of knots."""
    return PyOpenColorIO.GradingBSplineCurve([value for knot in held for value in (knot.x, knot.y)])


def read(table: Points, value: float) -> float:
    """Reads a sampled curve table at an input, linear between samples."""
    inputs, outputs = np.asarray(table, dtype=np.float64).T
    return float(np.interp(value, inputs, outputs))


def tables(record: Curves | HueCurves, config: PyOpenColorIO.Config) -> Outcome[dict[Key, Points]]:
    """Samples each curve's spline over its input bounds through its transform's processor."""
    return accumulated(curved(config, record.alone(held), row.inputs).map(lambda channels: (row.key, channels[0])) for row, held in zip(record.rows, record.splines)).map(dict)


def editors(record: Curves | HueCurves) -> tuple[Expr, ...]:
    """Native curve editors holding the knots, curves of one editor class sharing its node under the first curve's key in their axes."""

    def editor(rows: tuple[tuple[Curve, Knots], ...]) -> Expr:
        first, *_ = (row for row, _ in rows)
        clip = first.view.default_with(
            lambda: (min(row.inputs[0] for row, _ in rows), min(row.outputs[0] for row, _ in rows), max(row.inputs[1] for row, _ in rows), max(row.outputs[1] for row, _ in rows))
        )
        shown = tuple(tuple((float(knot.x), row.axes.shown(knot.x, knot.y)) for knot in held) for row, held in rows)
        return Expr(kind=first.node, content=Some(CurvePoints(maps=(shown[0], *shown[1:]), clip=Some(clip), handle="AUTO", clipped=True))).named(first.key)

    paired_rows = tuple(zip(record.rows, record.curves))
    shared = tuple(dict.fromkeys(row.node for row, _ in paired_rows if row.node is not bpy.types.ShaderNodeFloatCurve))
    return (*(editor(tuple(pair for pair in paired_rows if pair[0].node is node)) for node in shared), *(editor((pair,)) for pair in paired_rows if pair[0].node is bpy.types.ShaderNodeFloatCurve))


# --- [MEASURE]


def knotted[R: (Curves, HueCurves)](effect: CompositorEffect[R], call: Call[Named], title: str) -> Outcome[Answer]:
    """Arms the entering-frame read on the named entry, inserting a knot on each curve's sampled table at the covered scene-linear mean, refusing a frame that adds none."""

    def measured(identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, file: ImageFile[np.float32]) -> Outcome[Option[str]]:
        config = PyOpenColorIO.GetCurrentConfig()
        unread = Fault.of(BlenderReason.ABSENT, f"Region of frame entering {entry.name}", member="value its effect reads")
        straight = unassociated(file.pixels)
        covered = straight[..., :3][straight[..., 3] > 0.0] if straight.shape[-1] == 4 else straight[..., :3].reshape(-1, 3)

        def inserted(record: R, held: dict[Key, Points], at: dict[Key, float]) -> Outcome[R]:
            def grown(row: Curve, curve: Knots) -> Knots:
                value = at[row.key]
                position: int = bisect_left(curve, value, key=lambda knot: knot.x)
                return curve if position < len(curve) and curve[position].x == value else (*curve[:position], Knot(x=value, y=read(held[row.key], value)), *curve[position:])

            curves = tuple(map(grown, record.rows, record.curves))
            return (
                Error(unread)
                if curves == record.curves
                else catch(msgspec.convert, exception=msgspec.ValidationError)({"curves": msgspec.to_builtins(curves)}, type(record)).map_error(
                    partial(Fault.of, RecordReason.OUTSIDE, type(record).__name__, member="curves")
                )
            )

        def change(record: R) -> Outcome[R]:
            located = converted(config, PyOpenColorIO.ColorSpaceTransform(src=file.colorspace, dst=PyOpenColorIO.ROLE_SCENE_LINEAR), covered).map(
                lambda linear: linear.mean(axis=0, dtype=np.float64).astype(np.float32)
            )
            return tables(record, config).bind(lambda held: Result[dict[Key, float], Faults].bind(located.bind(lambda mean: record.located(config, mean, held)), lambda at: inserted(record, held, at)))

        wrote: Outcome[bool] = revised(effect, identity, scene, entry, change) if covered.size else Error(unread)
        return wrote.map(lambda changed: Some(title) if changed else Nothing)

    return (
        call
        .root(bpy.types.Scene)
        .bind(lambda scene: effect.named(call.identity, scene, call.properties.name).bind(lambda entry: READ.entering(call.identity, scene, entry, measured)))
        .map(lambda _: Done())
    )


def curve_knot(call: Call[Named]) -> Outcome[Answer]:
    """Insert RGB curve knots at the stage input's covered mean."""
    return knotted(curves, call, bpy.path.display_name(curve_knot.__name__))


def hue_curve_knot(call: Call[Named]) -> Outcome[Answer]:
    """Insert hue curve knots at the stage input's covered mean."""
    return knotted(hue_curves, call, bpy.path.display_name(hue_curve_knot.__name__))


@transform.register
def _(record: Curves, _values: Mapping[Key, Value], _config: PyOpenColorIO.Config, /) -> Outcome[Option[PyOpenColorIO.Transform]]:
    """Returns OCIO's linear RGB curve grade."""
    return Ok(Some(PyOpenColorIO.GradingRGBCurveTransform(values=PyOpenColorIO.GradingRGBCurve(*record.splines), style=PyOpenColorIO.GRADING_LIN)))


@transform.register
def _(record: HueCurves, _values: Mapping[Key, Value], _config: PyOpenColorIO.Config, /) -> Outcome[Option[PyOpenColorIO.Transform]]:
    """Returns OCIO's linear hue curve grade."""
    return Ok(Some(record.grade))


# --- [GRAPHS]


def channel_mapped(shell: Shell[Curves]) -> Outcome[Expr]:
    """Applies channel tables, then the master table, on the stop axis, beside the curve editors."""
    low, high = Curves.rows[-1].inputs
    identity: Points = ((low, low), (high, high))

    def table(key: Key, image: Expr, maps: tuple[Points, Points, Points, Points]) -> Expr:
        return Expr(
            kind=bpy.types.CompositorNodeCurveRGB,
            inputs=(("Image", image), ("Fac", 1.0), ("Black Level", (0.0, 0.0, 0.0, 1.0)), ("White Level", (1.0, 1.0, 1.0, 1.0))),
            members=((("mapping", "tone"), "STANDARD"),),
            content=Some(CurvePoints(maps=maps, clip=Some((low, low, high, high)))),
            socket="Image",
            shape="RGBA",
        ).named(key)

    def built(held: dict[Key, Points], groups: tuple[NodeGroup, ...]) -> Expr:
        forward, inverse = groups
        red, green, blue, master = (held[row.key] for row in Curves.rows)
        stops = table("master_table", table("channel_table", forward.placed({COLOR_INPUT.key: shell.color}, COLOR_RESULT.key), (red, green, blue, identity)), (identity, identity, identity, master))
        return inverse.placed({COLOR_INPUT.key: stops}, COLOR_RESULT.key).keeping(*editors(shell.record))

    return paired(tables(shell.record, shell.config), accumulated(map(logarithmic, (PyOpenColorIO.TRANSFORM_DIR_FORWARD, PyOpenColorIO.TRANSFORM_DIR_INVERSE)))).map(lambda pair: built(*pair))


def hue_mapped(shell: Shell[HueCurves]) -> Outcome[Expr]:
    """Maps HSY through the curve tables in OCIO's saturation then luma order, beside the curve editors."""
    rows: dict[Key, Curve] = {row.key: row for row in HueCurves.rows}
    hue_hue, hue_sat, hue_lum, lum_sat, sat_sat, lum_lum, sat_lum = rows

    def built(held: dict[Key, Points], groups: tuple[NodeGroup, ...]) -> Expr:
        forward, inverse, encoded, decoded = groups

        def table(key: Key, value: Scalar) -> Expr:
            row = rows[key]
            clip = (row.inputs[0], row.outputs[0], row.inputs[1], row.outputs[1])
            return Expr(kind=bpy.types.ShaderNodeFloatCurve, inputs=(("Factor", 1.0), ("Value", value)), content=Some(CurvePoints(maps=(held[key],), clip=Some(clip)))).named(item_key(key, "table"))

        spaced = forward.placed({COLOR_INPUT.key: shell.color}, COLOR_RESULT.key)
        hue, saturation, stops = spaced["Red"], spaced["Green"], encoded.placed({COLOR_INPUT.key: spaced}, COLOR_RESULT.key)["Blue"]
        gains = {key: math("MAXIMUM", table(key, value), 0.0) for key, value in ((hue_sat, hue), (hue_lum, hue), (lum_sat, stops), (sat_sat, saturation))}
        grown = gains[sat_sat] * gains[lum_sat] * gains[hue_sat]
        lightness = (
            decoded.placed({COLOR_INPUT.key: table(lum_lum, stops)}, COLOR_RESULT.key)["Blue"]
            * (1.0 - (1.0 - gains[hue_lum]) * math("MINIMUM", grown, 1.0))
            * math("MAXIMUM", table(sat_lum, grown), 0.0)
        )
        return inverse.placed({COLOR_INPUT.key: rgba(math("FRACT", table(hue_hue, hue)), grown, lightness)}, COLOR_RESULT.key).keeping(*editors(shell.record))

    return paired(
        tables(shell.record, shell.config),
        accumulated((
            hsy(shell.config, PyOpenColorIO.TRANSFORM_DIR_FORWARD),
            hsy(shell.config, PyOpenColorIO.TRANSFORM_DIR_INVERSE),
            logarithmic(PyOpenColorIO.TRANSFORM_DIR_FORWARD),
            logarithmic(PyOpenColorIO.TRANSFORM_DIR_INVERSE),
        )),
    ).map(lambda pair: built(*pair))


# --- [COMPOSITION] ----------------------------------------------------------------------

curves = CompositorEffect(
    record=Curves, graph=channel_mapped, alpha=Alpha.STRAIGHT, parts=(Registers(rows=(Operator(form=Named, run=curve_knot, icon=Some("EYEDROPPER"), undo=False, condition=Some(Job(kind="RENDER"))),)),)
)
hue_curves = CompositorEffect(
    record=HueCurves,
    graph=hue_mapped,
    alpha=Alpha.STRAIGHT,
    parts=(Registers(rows=(Operator(form=Named, run=hue_curve_knot, icon=Some("EYEDROPPER"), undo=False, condition=Some(Job(kind="RENDER"))),)),),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Curve", "Curves", "HueCurves", "curves", "hue_curves"]
