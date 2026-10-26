"""RGB and HSY curve tables with knots inserted from covered frames."""

from bisect import bisect_left
from collections.abc import Mapping
from typing import Final

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import msgspec.inspect as native
import numpy as np
import numpy.typing as npt
import PyOpenColorIO
from scipy.interpolate import make_interp_spline

from blender.bpy.faults import BlenderFaults, BlenderResult, BlenderUnmetFault
from blender.bpy.nodes.graphs import CurvePoints, Expr, math, NodeGroup, Points, rgba, Scalar
from blender.bpy.nodes.interface import item_key
from blender.bpy.render.color.spaces import COLOR_INPUT, COLOR_RESULT, hsy, logarithmic
from blender.bpy.render.effects.records import CurveEditor, Plot
from blender.bpy.render.effects.rows import Alpha, CompositorEffect, Measured, Shell
from blender.bpy.render.effects.viewers import Unread
from blender.bpy.settings.shapes import unwrapped
from blender.imaging.color.processors import converted, lin_to_log, luma, unassociated
from blender.imaging.effects.grade.curves import Curves, HueCurves, sampled, validated
from blender.imaging.faults import accumulated, ImagingResult, paired
from blender.imaging.files.images import ImageFile
from blender.imaging.parameters import Knot

# --- [CONSTANTS] ------------------------------------------------------------------------

CHANNELS: Final = CurveEditor(key="channels", node=bpy.types.CompositorNodeCurveRGB, fields=Curves.__struct_fields__)
HUES: Final = CurveEditor(
    key="hues", node=bpy.types.CompositorNodeHueCorrect, fields=HueCurves.__struct_fields__[:3], plots=(Plot(slope=-1.0, offset=0.5), Plot(scale=0.5), Plot(scale=0.5)), view=Some((0.0, 0.0, 1.0, 1.0))
)

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [BINDINGS]


def bounded(curves: tuple[CurvePoints, ...]) -> tuple[float, float, float, float]:
    """Returns view bounds covering every curve."""
    bounds = tuple(curve.clip.default_value((0.0, 0.0, 1.0, 1.0)) for curve in curves)
    return (min(bound[0] for bound in bounds), min(bound[1] for bound in bounds), max(bound[2] for bound in bounds), max(bound[3] for bound in bounds))


def _bindings(record: Curves | HueCurves) -> dict[str, tuple[Points, CurvePoints]]:
    """Resolves knot axes, sampled rows, and editor viewports per field."""
    return {
        field.name: (
            tuple((float(left), float(right)) for left, right in sampled(curve, 257, (low_x, high_x))),
            CurvePoints(maps=(tuple((point.x, point.y) for point in knots),), clip=Some((low_x, low_y, high_x, high_y)), handle="AUTO", clipped=True),
        )
        for field, curve, knots in zip(msgspec.structs.fields(type(record)), record.curves(), msgspec.structs.astuple(record))
        for shape in (unwrapped(native.type_info(field.type)),)
        if isinstance(shape, native.VarTupleType) and isinstance(shape.item_type, native.StructType)
        for axes in ({axis.name: unwrapped(axis.type) for axis in shape.item_type.fields},)
        for x, y in (tuple(axes[axis] for axis in Knot.__struct_fields__),)
        if isinstance(x, native.FloatType) and isinstance(y, native.FloatType)
        for low_x, high_x, low_y, high_y in (tuple(next(bound for bound in choices if bound is not None) for choices in ((x.ge, x.gt), (x.le, x.lt), (y.ge, y.gt), (y.le, y.lt))),)
    }


# --- [GRAPHS]


def _edited(result: Expr, bindings: dict[str, tuple[Points, CurvePoints]], editor: CurveEditor) -> Expr:
    """Keeps the editor node over the editor's fields and one Float Curve editor per other field beside result."""
    shared = msgspec.structs.replace(
        bindings[editor.fields[0]][1],
        maps=tuple(tuple((x, editor.plot(field).shown(x, y)) for x, y in bindings[field][1].maps[0]) for field in editor.fields),
        clip=Some(editor.view.default_with(lambda: bounded(tuple(bindings[field][1] for field in editor.fields)))),
    )
    editors = (
        Expr(kind=editor.node, content=Some(shared)).named(editor.key),
        *(Expr(kind=bpy.types.ShaderNodeFloatCurve, content=Some(curve)).named(key) for key, (_, curve) in bindings.items() if key not in editor.fields),
    )
    return result.keeping(*editors)


def curved(shell: Shell[Curves]) -> BlenderResult[Expr]:
    """Applies channel tables, then master table."""
    bindings = _bindings(shell.record)
    *channels, master = Curves.__struct_fields__
    identity = tuple((value, value) for value, _ in bindings[master][0])

    def table(key: str, image: Expr, maps: tuple[Points, Points, Points, Points]) -> Expr:
        return Expr(
            kind=bpy.types.CompositorNodeCurveRGB,
            inputs=(("Image", image), ("Fac", 1.0), ("Black Level", (0.0, 0.0, 0.0, 1.0)), ("White Level", (1.0, 1.0, 1.0, 1.0))),
            members=(("mapping.tone", "STANDARD"),),
            content=Some(CurvePoints(maps=maps, clip=bindings[master][1].clip)),
            socket="Image",
            shape="RGBA",
        ).named(key)

    def built(groups: tuple[NodeGroup, ...]) -> Expr:
        forward, inverse = groups
        red, green, blue = (bindings[channel][0] for channel in channels)
        tabled = table(
            "master_table", table("channel_table", forward.placed({COLOR_INPUT.key: shell.color}, COLOR_RESULT.key), (red, green, blue, identity)), (identity, identity, identity, bindings[master][0])
        )
        return _edited(inverse.placed({COLOR_INPUT.key: tabled}, COLOR_RESULT.key), bindings, CHANNELS)

    return accumulated(logarithmic(direction) for direction in (PyOpenColorIO.TRANSFORM_DIR_FORWARD, PyOpenColorIO.TRANSFORM_DIR_INVERSE)).map(built)


def hue_curved(shell: Shell[HueCurves]) -> BlenderResult[Expr]:
    """Maps HSY curves in OCIO saturation and luma order."""
    bindings = _bindings(shell.record)
    hue_hue, hue_sat, hue_lum, lum_sat, sat_sat, lum_lum, sat_lum = HueCurves.__struct_fields__

    def table(key: str, value: Scalar) -> Expr:
        points, curve = bindings[key]
        return Expr(kind=bpy.types.ShaderNodeFloatCurve, inputs=(("Factor", 1.0), ("Value", value)), content=Some(CurvePoints(maps=(points,), clip=curve.clip))).named(item_key(key, "table"))

    def built(groups: tuple[NodeGroup, ...]) -> Expr:
        forward, inverse, encoded, decoded = groups
        spaced = forward.placed({COLOR_INPUT.key: shell.color}, COLOR_RESULT.key)
        hue, saturation, stops = spaced["Red"], spaced["Green"], encoded.placed({COLOR_INPUT.key: spaced}, COLOR_RESULT.key)["Blue"]
        gains = {key: math("MAXIMUM", table(key, value), 0.0) for key, value in ((hue_sat, hue), (hue_lum, hue), (lum_sat, stops), (sat_sat, saturation))}
        grown = gains[sat_sat] * gains[lum_sat] * gains[hue_sat]
        lightness = (
            decoded.placed({COLOR_INPUT.key: table(lum_lum, stops)}, COLOR_RESULT.key)["Blue"]
            * (1.0 - (1.0 - gains[hue_lum]) * math("MINIMUM", grown, 1.0))
            * math("MAXIMUM", table(sat_lum, grown), 0.0)
        )
        return _edited(inverse.placed({COLOR_INPUT.key: rgba(math("FRACT", table(hue_hue, hue)), grown, lightness)}, COLOR_RESULT.key), bindings, HUES)

    return accumulated((
        hsy(PyOpenColorIO.TRANSFORM_DIR_FORWARD),
        hsy(PyOpenColorIO.TRANSFORM_DIR_INVERSE),
        logarithmic(PyOpenColorIO.TRANSFORM_DIR_FORWARD),
        logarithmic(PyOpenColorIO.TRANSFORM_DIR_INVERSE),
    )).map(built)


# --- [READ]


def measured[R: (Curves, HueCurves)](config: PyOpenColorIO.Config, record: R, plane: ImageFile[np.float32]) -> BlenderResult[Option[R]]:
    """Inserts knots at the covered RGB mean of the entering frame, in stops for channel tables and in HSY for hue tables."""
    unread = (BlenderUnmetFault(requirement=Unread(entry=record.title)),)
    if not (covered := straight[..., :3][straight[..., 3] > 0.0] if (straight := unassociated(plane.pixels)).shape[-1] == 4 else straight[..., :3].reshape(-1, 3)).size:
        return Error(unread)
    bindings = _bindings(record)
    splines = {key: make_interp_spline(x, y, k=1) for key, (table, _) in bindings.items() for x, y in (np.unstack(np.asarray(table), axis=1),)}
    native_config, log = PyOpenColorIO.Config(), lin_to_log()

    def stops(values: npt.NDArray[np.float32]) -> ImagingResult[npt.NDArray[np.float32]]:
        return converted(native_config, log, values)

    def channel_stops(mean: npt.NDArray[np.float32]) -> ImagingResult[dict[str, float]]:
        *channels, master = Curves.__struct_fields__

        def solved(entering: npt.NDArray[np.float32], weights: npt.NDArray[np.float64]) -> ImagingResult[dict[str, float]]:
            output = np.asarray([splines[key](value) for key, value in zip(channels, entering)], dtype=np.float32)
            return (
                converted(native_config, PyOpenColorIO.GroupTransform([log], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE), output)
                .bind(lambda linear: stops(np.full(3, float(linear @ weights), dtype=np.float32)))
                .map(lambda graded: dict(zip(channels, map(float, entering))) | {master: float(graded[0])})
            )

        return paired(stops(mean), luma(config)).bind(lambda pair: solved(*pair))

    def hue_stops(mean: npt.NDArray[np.float32]) -> ImagingResult[dict[str, float]]:
        hue_hue, hue_sat, hue_lum, lum_sat, sat_sat, lum_lum, sat_lum = HueCurves.__struct_fields__

        def located(hue: float, saturation: float, stop: float) -> dict[str, float]:
            at = {hue_hue: hue, hue_sat: hue, hue_lum: hue, lum_sat: stop, sat_sat: saturation, lum_lum: stop}
            return at | {sat_lum: float(np.prod([max(0.0, float(splines[key](at[key]))) for key in (sat_sat, lum_sat, hue_sat)]))}

        return converted(native_config, PyOpenColorIO.FixedFunctionTransform(PyOpenColorIO.FIXED_FUNCTION_RGB_TO_HSY_LIN), mean).bind(  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            lambda spaced: stops(np.full(3, spaced[2], dtype=np.float32)).map(lambda graded: located(float(spaced[0]), float(spaced[1]), float(graded[0])))
        )

    def inserted(at: Mapping[str, float]) -> BlenderResult[Option[R]]:
        convert = catch(msgspec.convert, exception=msgspec.ValidationError)

        def knotted(field: msgspec.structs.FieldInfo, knots: tuple[Knot[float, float], ...]) -> Option[object]:
            value = at[field.name]
            position: int = bisect_left(knots, value, key=lambda point: point.x)
            return (
                Nothing
                if position < len(knots) and knots[position].x == value
                else convert((*knots[:position], (value, float(np.interp(value, *np.asarray(bindings[field.name][0]).T))), *knots[position:]), field.type).to_option()
            )

        changes = {field.name: accepted for field, knots in zip(msgspec.structs.fields(record), msgspec.structs.astuple(record)) for accepted in knotted(field, knots).to_list()}
        return Ok(Some(msgspec.structs.replace(record, **changes))) if changes else Error(unread)

    return Result[dict[str, float], BlenderFaults].bind(
        converted(config, PyOpenColorIO.ColorSpaceTransform(src=plane.colorspace, dst=PyOpenColorIO.ROLE_SCENE_LINEAR), covered).bind(
            lambda linear: (channel_stops if isinstance(record, Curves) else hue_stops)(linear.mean(axis=0, dtype=np.float64).astype(np.float32))
        ),
        inserted,
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

curves: CompositorEffect[Curves] = CompositorEffect(record=Curves, graph=curved, alpha=Alpha.STRAIGHT, validated=Some(validated), editors=(CHANNELS,), measured=Some(Measured(revise=measured)))
hue_curves: CompositorEffect[HueCurves] = CompositorEffect(
    record=HueCurves, graph=hue_curved, alpha=Alpha.STRAIGHT, validated=Some(validated), editors=(HUES,), measured=Some(Measured(revise=measured))
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CHANNELS", "HUES", "bounded", "curved", "curves", "hue_curved", "hue_curves", "measured"]
