"""RGB and HSY curve tables with knots inserted from covered frames."""

from bisect import bisect_left
from typing import Final

import bpy
from expression import Error, Nothing, Option, Result, Some
from expression.extra.result import catch
import msgspec
import msgspec.inspect as native
import numpy as np
import numpy.typing as npt
import PyOpenColorIO
from scipy.interpolate import make_interp_spline

from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderFaults, BlenderImagingFault, BlenderResult, BlenderUnmetFault
from blender.bpy.nodes.graphs import CurvePoints, End, ended, Fragment, math, nested, node, NodeGroup, Points
from blender.bpy.nodes.interface import item_key
from blender.bpy.render.color.spaces import COLOR_INPUT, COLOR_RESULT, config_of, hsy, logarithmic
from blender.bpy.render.effects.stack import CompositorEffect, CurveEditor, Shell
from blender.bpy.render.effects.viewers import Unread, ViewerRead
from blender.bpy.settings.shapes import unwrapped
from blender.imaging.color.processors import applied, lin_to_log, luma, processor, unassociated
from blender.imaging.compositing.blending import Entry
from blender.imaging.effects.grade.curves import Curves, HueCurves, sampled, validated
from blender.imaging.faults import accumulated, paired
from blender.imaging.files.images import ImageFile
from blender.imaging.parameters import Knot

# --- [CONSTANTS] ------------------------------------------------------------------------

CHANNELS: Final = CurveEditor(key="channels", node=bpy.types.CompositorNodeCurveRGB, fields=Curves.__struct_fields__)
HUES: Final = CurveEditor(key="hues", node=bpy.types.CompositorNodeHueCorrect, fields=HueCurves.__struct_fields__[:3])

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [BINDINGS]


def _editors(bindings: dict[str, tuple[Points, CurvePoints]], editors: tuple[CurveEditor, ...]) -> BlenderResult[Fragment]:
    """Builds one node per shared editor over its fields, one curve node per other field."""
    shared = frozenset(field for editor in editors for field in editor.fields)
    return Fragment.joined(
        *(
            node(
                editor.key,
                editor.node,
                {},
                {},
                Some(
                    msgspec.structs.replace(
                        bindings[editor.fields[0]][1], maps=tuple(bindings[field][1].maps[0] for field in editor.fields), clip=Some(bounded(tuple(bindings[field][1] for field in editor.fields)))
                    )
                ),
            )
            for editor in editors
        ),
        *(node(key, bpy.types.ShaderNodeFloatCurve, {}, {}, Some(curve)) for key, (_, curve) in bindings.items() if key not in shared),
    )


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


def curved(shell: Shell[Curves]) -> BlenderResult[tuple[Fragment, End]]:
    """Applies channel tables, then master table."""
    bindings = _bindings(shell.record)
    red, green, blue, master = Curves.__struct_fields__
    identity = tuple((value, value) for value, _ in bindings[master][0])
    clip = bindings[master][1].clip

    def built(groups: tuple[NodeGroup, ...]) -> BlenderResult[tuple[Fragment, End]]:
        forward, inverse = groups
        return Fragment.joined(
            _editors(bindings, (CHANNELS,)),
            nested("encoded", forward, {COLOR_INPUT.key: shell.source}, COLOR_RESULT.key),
            node(
                "channel_table",
                bpy.types.CompositorNodeCurveRGB,
                {"Image": End(node="encoded", socket=COLOR_RESULT.key), "Fac": 1.0, "Black Level": (0.0, 0.0, 0.0, 1.0), "White Level": (1.0, 1.0, 1.0, 1.0)},
                {"mapping.tone": "STANDARD"},
                Some(CurvePoints(maps=(bindings[red][0], bindings[green][0], bindings[blue][0], identity), clip=clip)),
            ),
            node(
                "master_table",
                bpy.types.CompositorNodeCurveRGB,
                {"Image": End(node="channel_table", socket="Image"), "Fac": 1.0, "Black Level": (0.0, 0.0, 0.0, 1.0), "White Level": (1.0, 1.0, 1.0, 1.0)},
                {"mapping.tone": "STANDARD"},
                Some(CurvePoints(maps=(identity, identity, identity, bindings[master][0]), clip=clip)),
            ),
            nested("decoded", inverse, {COLOR_INPUT.key: End(node="master_table", socket="Image")}, COLOR_RESULT.key),
        ).map(ended("decoded", COLOR_RESULT.key))

    return accumulated(logarithmic(direction) for direction in (PyOpenColorIO.TRANSFORM_DIR_FORWARD, PyOpenColorIO.TRANSFORM_DIR_INVERSE)).bind(built)


def hue_curved(shell: Shell[HueCurves]) -> BlenderResult[tuple[Fragment, End]]:
    """Maps HSY curves in OCIO saturation and luma order."""
    bindings = _bindings(shell.record)
    hue_hue, hue_sat, hue_lum, lum_sat, sat_sat, lum_lum, sat_lum = HueCurves.__struct_fields__
    hue, saturation = (End(node="split", socket=key) for key in ("Red", "Green"))
    stops = End(node="stops", socket="Blue")
    grown_saturation = End(node="grown_saturation")
    inputs = {hue_hue: hue, hue_sat: hue, hue_lum: hue, lum_sat: stops, sat_sat: saturation, lum_lum: stops, sat_lum: grown_saturation}
    gains = (hue_sat, hue_lum, lum_sat, sat_sat, sat_lum)

    def built(groups: tuple[NodeGroup, ...]) -> BlenderResult[tuple[Fragment, End]]:
        forward, inverse, encoded, decoded = groups
        return Fragment.joined(
            _editors(bindings, (HUES,)),
            nested("hsy", forward, {COLOR_INPUT.key: shell.source}, COLOR_RESULT.key),
            node("split", bpy.types.CompositorNodeSeparateColor, {"Image": End(node="hsy", socket=COLOR_RESULT.key)}, {"mode": "RGB"}),
            nested("encoded", encoded, {COLOR_INPUT.key: End(node="hsy", socket=COLOR_RESULT.key)}, COLOR_RESULT.key),
            node("stops", bpy.types.CompositorNodeSeparateColor, {"Image": End(node="encoded", socket=COLOR_RESULT.key)}, {"mode": "RGB"}),
            *(
                node(item_key(key, "table"), bpy.types.ShaderNodeFloatCurve, {"Factor": 1.0, "Value": inputs[key]}, {}, Some(CurvePoints(maps=(table,), clip=editor.clip)))
                for key, (table, editor) in bindings.items()
            ),
            *(math(item_key(key, "gain"), "MAXIMUM", End(node=item_key(key, "table")), 0.0) for key in gains),
            math("saturation_luma", "MULTIPLY", End(node=item_key(sat_sat, "gain")), End(node=item_key(lum_sat, "gain"))),
            math(grown_saturation.node, "MULTIPLY", End(node="saturation_luma"), End(node=item_key(hue_sat, "gain"))),
            nested("decoded", decoded, {COLOR_INPUT.key: End(node=item_key(lum_lum, "table"))}, COLOR_RESULT.key),
            node("linear", bpy.types.CompositorNodeSeparateColor, {"Image": End(node="decoded", socket=COLOR_RESULT.key)}, {"mode": "RGB"}),
            math("coverage", "MINIMUM", grown_saturation, 1.0),
            math("hue_cut", "SUBTRACT", 1.0, End(node=item_key(hue_lum, "gain"))),
            math("covered_cut", "MULTIPLY", End(node="hue_cut"), End(node="coverage")),
            math("hue_gain", "SUBTRACT", 1.0, End(node="covered_cut")),
            math("hue_luma", "MULTIPLY", End(node="linear", socket="Blue"), End(node="hue_gain")),
            math("grown_luma", "MULTIPLY", End(node="hue_luma"), End(node=item_key(sat_lum, "gain"))),
            math("wrapped", "FRACT", End(node=item_key(hue_hue, "table"))),
            node("combined", bpy.types.CompositorNodeCombineColor, {"Red": End(node="wrapped"), "Green": grown_saturation, "Blue": End(node="grown_luma")}, {"mode": "RGB"}),
            nested("rgb", inverse, {COLOR_INPUT.key: End(node="combined", socket="Image")}, COLOR_RESULT.key),
        ).map(ended("rgb", COLOR_RESULT.key))

    return accumulated((
        hsy(PyOpenColorIO.TRANSFORM_DIR_FORWARD),
        hsy(PyOpenColorIO.TRANSFORM_DIR_INVERSE),
        logarithmic(PyOpenColorIO.TRANSFORM_DIR_FORWARD),
        logarithmic(PyOpenColorIO.TRANSFORM_DIR_INVERSE),
    )).bind(built)


# --- [READ]


def consumed[R: (Curves, HueCurves)](
    row: CompositorEffect[R], identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, plane: ImageFile[np.float32]
) -> BlenderResult[Option[str]]:
    """Inserts knots at covered RGB mean, keeping entry blend, naming undo step on change."""
    unread = (BlenderUnmetFault(requirement=Unread(entry=entry.name)),)
    if not (covered := straight[..., :3][straight[..., 3] > 0.0] if (straight := (unassociated(plane.pixels))).shape[-1] == 4 else straight[..., :3].reshape(-1, 3)).size:
        return Error(unread)

    def inserted(record: R, inputs: dict[str, float], bindings: dict[str, tuple[Points, CurvePoints]]) -> Option[R]:
        convert = catch(msgspec.convert, exception=msgspec.ValidationError)

        def knotted(field: msgspec.structs.FieldInfo, knots: tuple[Knot[float, float], ...]) -> Option[object]:
            value = inputs[field.name]
            position: int = bisect_left(knots, value, key=lambda point: point.x)
            return (
                Nothing
                if position < len(knots) and knots[position].x == value
                else convert((*knots[:position], (value, float(np.interp(value, *np.asarray(bindings[field.name][0]).T))), *knots[position:]), field.type).to_option()
            )

        changes = {field.name: accepted for field, knots in zip(msgspec.structs.fields(record), msgspec.structs.astuple(record)) for accepted in knotted(field, knots).to_list()}
        return Some(msgspec.structs.replace(record, **changes)) if changes else Nothing

    def grown(record: R, mean: npt.NDArray[np.float32], config: PyOpenColorIO.Config) -> BlenderResult[R]:
        bindings = _bindings(record)
        splines = {key: make_interp_spline(x, y, k=1) for key, (table, _) in bindings.items() for x, y in (np.unstack(np.asarray(table), axis=1),)}
        log = lin_to_log()
        logs: BlenderResult[tuple[PyOpenColorIO.CPUProcessor, ...]] = accumulated(
            processor(PyOpenColorIO.Config(), transform).map(PyOpenColorIO.Processor.getDefaultCPUProcessor)
            for transform in (log, PyOpenColorIO.GroupTransform([log], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE))
        ).map_error(BlenderImagingFault.lifted)

        def rgb(values: tuple[tuple[PyOpenColorIO.CPUProcessor, ...], npt.NDArray[np.float32]]) -> BlenderResult[R]:
            (forward, inverse), weights = values
            stops = applied(forward, mean)
            red, green, blue, master_field = Curves.__struct_fields__
            channels = red, green, blue
            output = np.asarray([splines[key](value) for key, value in zip(channels, stops)], dtype=np.float32)
            working_y = float(applied(inverse, output) @ weights)
            master = float(applied(forward, np.full(3, working_y, dtype=np.float32))[0])
            return inserted(record, dict(zip(channels, map(float, stops))) | {master_field: master}, bindings).to_result(unread)

        def hue(values: tuple[tuple[PyOpenColorIO.CPUProcessor, ...], PyOpenColorIO.CPUProcessor]) -> BlenderResult[R]:
            (forward, _inverse), conversion = values
            hue, saturation, luminance = map(float, applied(conversion, mean))
            stops = float(applied(forward, np.full(3, luminance, dtype=np.float32))[0])
            hue_hue, hue_sat, hue_lum, lum_sat, sat_sat, lum_lum, sat_lum = HueCurves.__struct_fields__
            at = {hue_hue: hue, hue_sat: hue, hue_lum: hue, lum_sat: stops, sat_sat: saturation, lum_lum: stops}
            at[sat_lum] = float(np.prod([max(0.0, float(splines[key](at[key]))) for key in (sat_sat, lum_sat, hue_sat)]))
            return inserted(record, at, bindings).to_result(unread)

        return (
            Result[tuple[tuple[PyOpenColorIO.CPUProcessor, ...], npt.NDArray[np.float32]], BlenderFaults].bind(paired(logs, luma(config).map_error(BlenderImagingFault.lifted)), rgb)
            if isinstance(record, Curves)
            else Result[tuple[tuple[PyOpenColorIO.CPUProcessor, ...], PyOpenColorIO.CPUProcessor], BlenderFaults].bind(
                paired(
                    logs,
                    processor(PyOpenColorIO.Config(), PyOpenColorIO.FixedFunctionTransform(PyOpenColorIO.FIXED_FUNCTION_RGB_TO_HSY_LIN))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    .map(PyOpenColorIO.Processor.getDefaultCPUProcessor)
                    .map_error(BlenderImagingFault.lifted),
                ),
                hue,
            )
        )

    def read(values: tuple[Entry[R], PyOpenColorIO.Config]) -> BlenderResult[Option[str]]:
        held, config = values
        mean: BlenderResult[npt.NDArray[np.float32]] = (
            processor(config, PyOpenColorIO.ColorSpaceTransform(src=plane.colorspace, dst=PyOpenColorIO.ROLE_SCENE_LINEAR))
            .map_error(BlenderImagingFault.lifted)
            .map(lambda built: applied(built.getDefaultCPUProcessor(), covered).mean(axis=0, dtype=np.float64).astype(np.float32))
        )
        return (
            Result[npt.NDArray[np.float32], BlenderFaults]
            .bind(mean, lambda mean: grown(held.effect, mean, config))
            .bind(lambda record: row.write(identity, scene, entry, msgspec.structs.replace(held, effect=record)))
            .map(lambda changed: Some("Insert Knots") if changed else Nothing)
        )

    reading: BlenderResult[tuple[Entry[R], PyOpenColorIO.Config]] = paired(row.read(identity, entry), config_of())
    return Result[tuple[Entry[R], PyOpenColorIO.Config], BlenderFaults].bind(reading, read)


# --- [COMPOSITION] ----------------------------------------------------------------------

curves: CompositorEffect[Curves] = CompositorEffect(
    record=Curves,
    graph=curved,
    straight=True,
    validated=Some(validated),
    editors=(CHANNELS,),
    registrables=(ViewerRead(consume=lambda identity, scene, entry, plane: consumed(curves, identity, scene, entry, plane)),),
)
hue_curves: CompositorEffect[HueCurves] = CompositorEffect(
    record=HueCurves,
    graph=hue_curved,
    straight=True,
    validated=Some(validated),
    editors=(HUES,),
    registrables=(ViewerRead(consume=lambda identity, scene, entry, plane: consumed(hue_curves, identity, scene, entry, plane)),),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CHANNELS", "HUES", "bounded", "consumed", "curved", "curves", "hue_curved", "hue_curves"]
