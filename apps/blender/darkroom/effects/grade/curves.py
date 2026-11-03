"""RGB and HSY grading curves of OCIO's linear curve grades, sampled into compositor curve tables beside native curve editors whose knots the entry takes back, with knots inserted from covered frames."""

from bisect import bisect_left
from collections.abc import Callable, Mapping
from functools import partial
from itertools import chain
from typing import Any, ClassVar, Final

import bpy
from expression import Error, Nothing, Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.operators import Answer, Call, Done, Operator, Unchanged
from blender.bpy.data.rna.paths import rooted
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes.addressing import owned
from blender.bpy.nodes.content import CurveAxes, CurvePoints, Points
from blender.bpy.nodes.desired import item_key
from blender.bpy.nodes.graphs import Expr, math, rgba, Scalar
from blender.bpy.render.compositor.colors import hsy, logarithmic
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, Named
from blender.bpy.render.compositor.grades import curved
from blender.bpy.render.compositor.inputs import GraphInputs, grouped
from blender.bpy.render.compositor.viewers import READ
from blender.imaging.color.processors import converted, GET_SPLINE_TYPE, HUE_CURVE, HUE_CURVE_TRANSFORM, lin_to_log, luma, RGB_TO_HSY, SET_DRAW_CURVE_ONLY, SET_SPLINE_TYPE
from blender.imaging.files.images import ImageFile
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
from blender.records.fields import Case, Knot

from .colorimetry import finite_mean, unread

# --- [TYPES] ----------------------------------------------------------------------------

type Knots = tuple[Knot, ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

_NEUTRAL_RGB: Final = PyOpenColorIO.GradingRGBCurve(PyOpenColorIO.GRADING_LIN)
_NEUTRAL_HUE: Final = HUE_CURVE(PyOpenColorIO.GRADING_LIN)
_NEUTRAL_KNOTS: Final = tuple(_NEUTRAL_RGB.master.getControlPoints())
_STOPS: Final = (_NEUTRAL_KNOTS[0].x, _NEUTRAL_KNOTS[-1].x)

# --- [MODELS] ---------------------------------------------------------------------------


class Curve(msgspec.Struct, frozen=True, kw_only=True):
    """Knot curve of a grade under the name of its OCIO curve: its input and output bounds, its most knots, and the native editor holding it in the editor's own axes."""

    key: Key
    inputs: tuple[float, float]
    outputs: tuple[float, float]
    knots: int
    node: type[bpy.types.Node] = bpy.types.ShaderNodeFloatCurve
    axes: CurveAxes = CurveAxes()
    view: Option[tuple[float, float, float, float]] = Nothing

    def holds(self, knots: Knots) -> bool:
        """Whether knots count from two to the most, each inside the bounds at a distinct input."""
        (low, high), (bottom, top) = self.inputs, self.outputs
        return 2 <= len(knots) <= self.knots and len({knot.x for knot in knots}) == len(knots) and all(low <= knot.x <= high and bottom <= knot.y <= top for knot in knots)


class Curves(Case, frozen=True, kw_only=True):
    """Maps RGB channels, then all channels, on OCIO's scene-linear stop axis, the curves refused at decode outside their bounds or when OCIO rejects them."""

    rows: ClassVar[tuple[Curve, ...]] = tuple(Curve(key=key, inputs=_STOPS, outputs=_STOPS, knots=15, node=bpy.types.CompositorNodeCurveRGB) for key in ("red", "green", "blue", "master"))
    curves: tuple[Knots, ...] = msgspec.convert(tuple(tuple(getattr(_NEUTRAL_RGB, row.key).getControlPoints()) for row in rows), tuple[Knots, ...], from_attributes=True)

    def __post_init__(self) -> None:
        """Refuses curves outside their rows' bounds or failing OCIO's validation."""
        _validated(self, lambda: tuple(map(PyOpenColorIO.GradingBSplineCurve.validate, self.splines)))

    @property
    def splines(self) -> tuple[PyOpenColorIO.GradingBSplineCurve, ...]:
        """Channel splines, then the master spline."""
        return tuple(map(_spline, self.curves))

    @property
    def grade(self) -> PyOpenColorIO.Transform:
        """Native linear RGB curve grade."""
        return PyOpenColorIO.GradingRGBCurveTransform(values=PyOpenColorIO.GradingRGBCurve(*self.splines), style=PyOpenColorIO.GRADING_LIN)

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
            graded = np.asarray([_read(held[name], float(level)) for name, level in zip(channels, entering)], dtype=np.float32)
            return (
                converted(raw, PyOpenColorIO.GroupTransform([log], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE), graded)
                .bind(lambda linear: converted(raw, log, np.full(3, float(linear @ weights), dtype=np.float32)))
                .map(lambda stops: dict(zip(channels, map(float, entering))) | {master: float(stops[0])})
            )

        return paired(converted(raw, log, mean), luma(config)).bind(lambda pair: solved(*pair))


class HueCurves(Case, frozen=True, kw_only=True):
    """Maps hue, saturation, and luma through OCIO's scene-linear HSY grade, the curves refused at decode outside their bounds or when OCIO rejects them."""

    hue_hue: ClassVar[Curve] = Curve(
        key="hue_hue", inputs=(0.0, 1.0), outputs=(-1.0, 2.0), knots=8, node=bpy.types.CompositorNodeHueCorrect, axes=CurveAxes(slope=-1.0, offset=0.5), view=Some((0.0, 0.0, 1.0, 1.0))
    )
    hue_sat: ClassVar[Curve] = Curve(key="hue_sat", inputs=(0.0, 1.0), outputs=(0.0, 2.0), knots=8, node=bpy.types.CompositorNodeHueCorrect, axes=CurveAxes(scale=0.5))
    hue_lum: ClassVar[Curve] = Curve(key="hue_lum", inputs=(0.0, 1.0), outputs=(0.0, 2.0), knots=8, node=bpy.types.CompositorNodeHueCorrect, axes=CurveAxes(scale=0.5))
    lum_sat: ClassVar[Curve] = Curve(key="lum_sat", inputs=_STOPS, outputs=(0.0, 2.0), knots=8)
    sat_sat: ClassVar[Curve] = Curve(key="sat_sat", inputs=(0.0, 2.5), outputs=(0.0, 2.5), knots=8)
    lum_lum: ClassVar[Curve] = Curve(key="lum_lum", inputs=_STOPS, outputs=_STOPS, knots=8)
    sat_lum: ClassVar[Curve] = Curve(key="sat_lum", inputs=(0.0, 2.5), outputs=(0.0, 2.0), knots=8)
    rows: ClassVar[tuple[Curve, ...]] = (hue_hue, hue_sat, hue_lum, lum_sat, sat_sat, lum_lum, sat_lum)
    curves: tuple[Knots, ...] = msgspec.convert(tuple(tuple(getattr(_NEUTRAL_HUE, row.key).getControlPoints()) for row in rows), tuple[Knots, ...], from_attributes=True)

    def __post_init__(self) -> None:
        """Refuses curves outside their rows' bounds or failing OCIO's validation of the hue grade."""
        _validated(self, lambda: HUE_CURVE(*self.splines, _NEUTRAL_HUE.hue_fx).validate())

    @property
    def splines(self) -> tuple[PyOpenColorIO.GradingBSplineCurve, ...]:
        """Splines of each curve typed as the neutral grade's curve of the same name."""
        splines = tuple(map(_spline, self.curves))
        for held, row in zip(splines, self.rows):
            SET_SPLINE_TYPE(held, GET_SPLINE_TYPE(getattr(_NEUTRAL_HUE, row.key)))
        return splines

    @property
    def grade(self) -> PyOpenColorIO.Transform:
        """Native linear hue curve grade, hue folding at the neutral grade's own."""
        return HUE_CURVE_TRANSFORM(values=HUE_CURVE(*self.splines, _NEUTRAL_HUE.hue_fx), style=PyOpenColorIO.GRADING_LIN)

    @staticmethod
    def alone(curve: PyOpenColorIO.GradingBSplineCurve) -> PyOpenColorIO.Transform:
        """Hue grade drawing the spline alone, held in a slot retyped to the spline's kind."""
        grade = HUE_CURVE(PyOpenColorIO.GRADING_LIN)
        SET_SPLINE_TYPE(grade.hue_sat, GET_SPLINE_TYPE(curve))
        grade.hue_sat = curve
        SET_DRAW_CURVE_ONLY(grade, True)
        return HUE_CURVE_TRANSFORM(values=grade, style=PyOpenColorIO.GRADING_LIN)

    def located(self, _config: PyOpenColorIO.Config, mean: npt.NDArray[np.float32], held: Mapping[Key, Points]) -> Outcome[dict[Key, float]]:
        """Hue inputs at the mean's hue, luma inputs at its luma stops, saturation input at its saturation, and the luma-gain input at the saturation the grade outputs."""
        raw = PyOpenColorIO.Config()

        def at(hue: float, saturation: float, stop: float) -> dict[Key, float]:
            inputs = {self.hue_hue.key: hue, self.hue_sat.key: hue, self.hue_lum.key: hue, self.lum_sat.key: stop, self.sat_sat.key: saturation, self.lum_lum.key: stop}
            gains = (max(0.0, _read(held[row.key], inputs[row.key])) for row in (self.sat_sat, self.lum_sat, self.hue_sat))
            return inputs | {self.sat_lum.key: float(np.prod(tuple(gains)))}

        return converted(raw, PyOpenColorIO.FixedFunctionTransform(RGB_TO_HSY), mean).bind(
            lambda spaced: converted(raw, lin_to_log(), np.full(3, spaced[2], dtype=np.float32)).map(lambda stops: at(float(spaced[0]), float(spaced[1]), float(stops[0])))
        )


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SPLINES]


def _spline(held: Knots) -> PyOpenColorIO.GradingBSplineCurve:
    """Builds the native B-spline of knots."""
    return PyOpenColorIO.GradingBSplineCurve([coordinate for knot in held for coordinate in (knot.x, knot.y)])


def _validated(record: Curves | HueCurves, validate: Callable[[], object]) -> None:
    """Refuses curves outside their rows' bounds, then any OCIO refuses through `validate`."""
    if len(record.curves) != len(record.rows) or not all(map(Curve.holds, record.rows, record.curves)):
        raise ValueError(f"{type(record).__name__} curves hold distinct knots inside their bounds")
    try:
        validate()
    except PyOpenColorIO.Exception as error:
        raise ValueError(str(error)) from error


def _read(table: Points, level: float) -> float:
    """Reads a sampled curve table at an input, linear between samples."""
    inputs, outputs = np.asarray(table, dtype=np.float64).T
    return float(np.interp(level, inputs, outputs))


def _tables(record: Curves | HueCurves, config: PyOpenColorIO.Config) -> Outcome[dict[Key, Points]]:
    """Samples each curve's spline over its input bounds through its transform's processor."""
    return accumulated(curved(config, record.alone(held), row.inputs).map(lambda channels: (row.key, channels[0])) for row, held in zip(record.rows, record.splines)).map(dict)


def _rebuilt[R: (Curves, HueCurves)](record: R, curves: tuple[Knots, ...]) -> Outcome[R]:
    """Record holding the curves, refused outside their bounds or by OCIO."""
    return catch(msgspec.convert, exception=msgspec.ValidationError)({"curves": msgspec.to_builtins(curves)}, type(record)).map_error(
        partial(Fault.of, Reason.OUTSIDE, type(record).__name__, member="curves")
    )


# --- [EDITORS]


def _shared(rows: tuple[Curve, ...]) -> tuple[tuple[int, ...], ...]:
    """Row positions of each native editor, rows of one editor class past the float curve sharing its node under the first row's key."""
    kinds = tuple(dict.fromkeys(row.node for row in rows if row.node is not bpy.types.ShaderNodeFloatCurve))
    return (*(tuple(index for index, row in enumerate(rows) if row.node is kind) for kind in kinds), *((index,) for index, row in enumerate(rows) if row.node is bpy.types.ShaderNodeFloatCurve))


def _editors(record: Curves | HueCurves) -> tuple[Expr, ...]:
    """Native curve editors holding the knots in their own axes, each keyed by its first row."""

    def editor(positions: tuple[int, ...]) -> Expr:
        rows = tuple(record.rows[index] for index in positions)
        first = rows[0]
        clip = first.view.default_with(lambda: (min(row.inputs[0] for row in rows), min(row.outputs[0] for row in rows), max(row.inputs[1] for row in rows), max(row.outputs[1] for row in rows)))
        first_map, *rest = (tuple((knot.x, row.axes.shown(knot.x, knot.y)) for knot in record.curves[index]) for row, index in zip(rows, positions))
        return Expr(kind=first.node, content=Some(CurvePoints(maps=(first_map, *rest), clip=Some(clip), handle="AUTO", clipped=True))).keyed(first.key)

    return tuple(map(editor, _shared(record.rows)))


def _taken[R: (Curves, HueCurves)](effect: CompositorEffect[R], identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> Outcome[bool]:
    """Writes the knots the entry's native editors hold into its curves, answering whether the record changed."""

    def change(record: R) -> Outcome[R]:
        def knots(nodes: Mapping[Key, bpy.types.Node], positions: tuple[int, ...]) -> Outcome[tuple[tuple[int, Knots], ...]]:
            first = record.rows[positions[0]]
            return (
                Option
                .of_optional(nodes.get(first.key))
                .to_result_with(partial(Fault.of, Reason.ABSENT, entry.name, member=first.key))
                .bind(CurvePoints.read)
                .map(lambda read: tuple((index, tuple(Knot(x=x, y=record.rows[index].axes.held(x, y)) for x, y in points)) for index, points in zip(positions, read.maps)))
            )

        return (
            grouped(entry)
            .map(lambda tree: owned(tree, identity))
            .bind(lambda nodes: accumulated(knots(nodes, positions) for positions in _shared(record.rows)))
            .bind(lambda found: _rebuilt(record, tuple(dict(chain.from_iterable(found))[index] for index in range(len(record.rows)))))
        )

    return effect.revised(identity, scene, entry, change)


def _knotted[R: (Curves, HueCurves)](effect: CompositorEffect[R], identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, file: ImageFile) -> Outcome[bool]:
    """Inserts a knot on each curve's sampled table at the covered scene-linear mean of the entering frame, refusing a frame that adds none."""
    config = PyOpenColorIO.GetCurrentConfig()

    def inserted(record: R, held: Mapping[Key, Points], at: Mapping[Key, float]) -> Outcome[R]:
        def grown(row: Curve, curve: Knots) -> Knots:
            level = at[row.key]
            position: int = bisect_left(curve, level, key=lambda knot: knot.x)
            return curve if position < len(curve) and curve[position].x == level else (*curve[:position], Knot(x=level, y=_read(held[row.key], level)), *curve[position:])

        curves = tuple(map(grown, record.rows, record.curves))
        return Error(unread(entry)) if curves == record.curves else _rebuilt(record, curves)

    def change(mean: npt.NDArray[np.float32], record: R) -> Outcome[R]:
        return _tables(record, config).bind(lambda held: record.located(config, mean, held).bind(lambda at: inserted(record, held, at)))

    return finite_mean(config, entry, file).bind(lambda mean: effect.revised(identity, scene, entry, partial(change, mean.astype(np.float32))))


# --- [COMMANDS]


def _owner(identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> CompositorEffect[Any]:
    """Curve effect owning an entry both effects' commands read."""
    return curves if entry in curves.entries(identity, scene) else hue_curves


def curve_knot(identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, file: ImageFile) -> Outcome[bool]:
    """Insert curve knots at the stage input's covered mean."""
    return _knotted(_owner(identity, scene, entry), identity, scene, entry, file)


def take_curve_edits(call: Call[Named]) -> Outcome[Answer]:
    """Write the curve editors' knots into the entry's curves."""
    identity, name = call.identity, call.properties.name

    def taken(scene: bpy.types.Scene) -> Outcome[bool]:
        return curves.named(identity, scene, name).or_else_with(lambda _: hue_curves.named(identity, scene, name)).bind(lambda entry: _taken(_owner(identity, scene, entry), identity, scene, entry))

    return rooted(bpy.types.Scene, call.context).bind(taken).map(lambda changed: Done() if changed else Unchanged(reason=f"{name} holds its editors' knots"))


# --- [GRAPHS]


def _curves(shell: GraphInputs[Curves]) -> Outcome[Expr]:
    """Applies channel tables, then the master table, on the stop axis, beside the curve editors."""
    low, high = _STOPS
    flat: Points = ((low, low), (high, high))

    def table(key: Key, image: Expr, maps: tuple[Points, Points, Points, Points]) -> Expr:
        return Expr(
            kind=bpy.types.CompositorNodeCurveRGB,
            inputs=(("Image", image), ("Fac", 1.0), ("Black Level", (0.0, 0.0, 0.0, 1.0)), ("White Level", (1.0, 1.0, 1.0, 1.0))),
            members={("mapping", "tone"): "STANDARD"},
            content=Some(CurvePoints(maps=maps, clip=Some((low, low, high, high)))),
            socket="Image",
            socket_type=bpy.types.NodeSocketColor,
        ).keyed(key)

    def built(held: Mapping[Key, Points]) -> Outcome[Expr]:
        red, green, blue, master = (held[row.key] for row in Curves.rows)
        return logarithmic(PyOpenColorIO.TRANSFORM_DIR_FORWARD, shell.color).bind(
            lambda logged: logarithmic(PyOpenColorIO.TRANSFORM_DIR_INVERSE, table("master_table", table("channel_table", logged, (red, green, blue, flat)), (flat, flat, flat, master)))
        )

    return _tables(shell.record, shell.config).bind(built).map(lambda result: result.keeping(*_editors(shell.record)))


def _hue_curves(shell: GraphInputs[HueCurves]) -> Outcome[Expr]:
    """Maps HSY through the curve tables in OCIO's saturation then luma order, beside the curve editors."""

    def built(held: Mapping[Key, Points]) -> Outcome[Expr]:
        def table(row: Curve, level: Scalar) -> Expr:
            clip = (row.inputs[0], row.outputs[0], row.inputs[1], row.outputs[1])
            return Expr(kind=bpy.types.ShaderNodeFloatCurve, inputs=(("Factor", 1.0), ("Value", level)), content=Some(CurvePoints(maps=(held[row.key],), clip=Some(clip)))).keyed(
                item_key(row.key, "table")
            )

        def lifted(hue: Expr, saturation: Expr, stops: Expr) -> Outcome[Expr]:
            hue_gain, luma_gain, stop_gain, saturation_gain = (
                math("MAXIMUM", table(row, level), 0.0) for row, level in ((HueCurves.hue_sat, hue), (HueCurves.hue_lum, hue), (HueCurves.lum_sat, stops), (HueCurves.sat_sat, saturation))
            )
            grown = saturation_gain * stop_gain * hue_gain
            return logarithmic(PyOpenColorIO.TRANSFORM_DIR_INVERSE, table(HueCurves.lum_lum, stops)).bind(
                lambda decoded: hsy(
                    shell.config,
                    PyOpenColorIO.TRANSFORM_DIR_INVERSE,
                    rgba(
                        math("FRACT", table(HueCurves.hue_hue, hue)),
                        grown,
                        decoded["Blue"] * (1.0 - (1.0 - luma_gain) * math("MINIMUM", grown, 1.0)) * math("MAXIMUM", table(HueCurves.sat_lum, grown), 0.0),
                    ),
                )
            )

        return hsy(shell.config, PyOpenColorIO.TRANSFORM_DIR_FORWARD, shell.color).bind(
            lambda spaced: logarithmic(PyOpenColorIO.TRANSFORM_DIR_FORWARD, spaced).bind(lambda encoded: lifted(spaced["Red"], spaced["Green"], encoded["Blue"]))
        )

    return _tables(shell.record, shell.config).bind(built).map(lambda result: result.keeping(*_editors(shell.record)))


# --- [COMPOSITION] ----------------------------------------------------------------------

_COMMANDS: Final = (READ.measuring(curve_knot, icon=Some("EYEDROPPER")), Operator(form=Named, run=take_curve_edits, icon=Some("CHECKMARK")))
curves: Final = CompositorEffect(_curves, record=Curves, mix=Blended(alpha=Alpha.STRAIGHT), pointwise=True, registers=_COMMANDS)
hue_curves: Final = CompositorEffect(_hue_curves, record=HueCurves, mix=Blended(alpha=Alpha.STRAIGHT), pointwise=True, registers=_COMMANDS)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Curve", "Curves", "HueCurves", "curves", "hue_curves"]
