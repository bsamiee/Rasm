# mypy: disable-error-code="attr-defined"
# ty: ignore[unresolved-attribute]
"""Native RGB and HSY curve tables, editable knots, and covered-frame insertion."""

from bisect import bisect_left

import bpy
from expression import Error, Nothing, Option, Result, Some
from expression.extra.result import catch
import msgspec
import msgspec.inspect as native
import numpy as np
import numpy.typing as npt
import PyOpenColorIO
from scipy.interpolate import make_interp_spline

from rasm.blender.extension.identity import Identity
from rasm.blender.faults import BlenderFaults, BlenderImagingFault, BlenderResult, BlenderUnmetFault
from rasm.blender.nodes.graphs import CurvePoints, End, ended, Fragment, item_key, math, nested, node, NodeGroup, Points
from rasm.blender.render.color.spaces import COLOR_INPUT, COLOR_RESULT, hsy, logarithmic
from rasm.blender.render.effects.stack import CompositorEffect, Shell
from rasm.blender.render.effects.viewers import Unread, ViewerRead
from rasm.imaging.color import config
from rasm.imaging.color.processors import applied, lin_to_log, luma, processor, unassociated
from rasm.imaging.compositing.blending import Entry
from rasm.imaging.effects.grade.curves import admitted, Curves, HueCurves, sampled
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.files.images import ImageFile
from rasm.imaging.parameters import Knot

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [BINDINGS]


def _bindings(record: Curves | HueCurves) -> dict[str, tuple[Points, CurvePoints]]:
    """Resolves native knot axes, sampled rows, and editor viewports once per field."""

    def bare(shape: native.Type) -> native.Type:
        return bare(shape.type) if isinstance(shape, native.Metadata) else shape

    answer: dict[str, tuple[Points, CurvePoints]] = {}
    for field, curve, knots in zip(msgspec.structs.fields(type(record)), record.curves(), msgspec.structs.astuple(record)):
        match bare(native.type_info(field.type)):  # type: ignore[exhaustive-match]
            case native.VarTupleType(item_type=native.StructType(fields=fields)):
                axes = {field.name: bare(field.type) for field in fields}
                match axes[Knot.x.__name__], axes[Knot.y.__name__]:  # type: ignore[misc, exhaustive-match]  # ty: ignore[invalid-attribute-access]
                    case native.FloatType() as x, native.FloatType() as y:
                        low_x, high_x, low_y, high_y = (next(bound for bound in choices if bound is not None) for choices in ((x.ge, x.gt), (x.le, x.lt), (y.ge, y.gt), (y.le, y.lt)))
                        table = tuple((float(left), float(right)) for left, right in sampled(curve, 257, (low_x, high_x)))
                        editor = CurvePoints(maps=(tuple((point.x, point.y) for point in knots),), clip=Some((low_x, low_y, high_x, high_y)), handle="AUTO", clipped=True)
                        answer[field.name] = table, editor
    return answer


# --- [GRAPHS]


def curved(shell: Shell[Curves]) -> BlenderResult[tuple[Fragment, End]]:
    """Applies channel tables before the master table on both compositor devices."""
    bindings = _bindings(shell.record)
    red, green, blue, master = Curves.red.__name__, Curves.green.__name__, Curves.blue.__name__, Curves.master.__name__
    identity = tuple((value, value) for value, _ in bindings[master][0])
    clip = bindings[master][1].clip

    def built(groups: tuple[NodeGroup, ...]) -> BlenderResult[tuple[Fragment, End]]:
        forward, inverse = groups
        return Fragment.joined(
            *(node(key, bpy.types.ShaderNodeFloatCurve, {}, {}, Some(editor)) for key, (_, editor) in bindings.items()),
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
    """Maps all seven native HSY curves in OCIO's saturation and luma order."""
    bindings = _bindings(shell.record)
    hue_hue, hue_sat, hue_lum, lum_sat, sat_sat, lum_lum, sat_lum = (
        HueCurves.hue_hue.__name__,
        HueCurves.hue_sat.__name__,
        HueCurves.hue_lum.__name__,
        HueCurves.lum_sat.__name__,
        HueCurves.sat_sat.__name__,
        HueCurves.lum_lum.__name__,
        HueCurves.sat_lum.__name__,
    )
    hue, saturation = (End(node="split", socket=key) for key in ("Red", "Green"))
    stops = End(node="stops", socket="Blue")
    grown_saturation = End(node="grown_saturation")
    inputs = {hue_hue: hue, hue_sat: hue, hue_lum: hue, lum_sat: stops, sat_sat: saturation, lum_lum: stops, sat_lum: grown_saturation}
    gains = (hue_sat, hue_lum, lum_sat, sat_sat, sat_lum)

    def built(groups: tuple[NodeGroup, ...]) -> BlenderResult[tuple[Fragment, End]]:
        forward, inverse, encoded, decoded = groups
        return Fragment.joined(
            *(node(key, bpy.types.ShaderNodeFloatCurve, {}, {}, Some(editor)) for key, (_, editor) in bindings.items()),
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


def consumed[R: (Curves, HueCurves)](row: CompositorEffect[R], identity: Identity, entry: bpy.types.SceneCompositorEffect, plane: ImageFile[np.float32]) -> BlenderResult[Option[str]]:
    """Inserts knots at the covered RGB mean, preserving the entry's blend."""
    unread = (BlenderUnmetFault(requirement=Unread(entry=entry.name)),)
    if not (covered := straight[..., :3][straight[..., 3] > 0.0] if (straight := (unassociated(plane.pixels))).shape[-1] == 4 else straight[..., :3].reshape(-1, 3)).size:
        return Error(unread)

    def inserted(record: R, inputs: dict[str, float], bindings: dict[str, tuple[Points, CurvePoints]]) -> Option[R]:
        convert = catch(msgspec.convert, exception=msgspec.ValidationError)
        changes = {}
        for field, knots in zip(msgspec.structs.fields(record), msgspec.structs.astuple(record)):
            value = inputs[field.name]
            position = bisect_left(knots, value, key=lambda point: point.x)
            if position < len(knots) and knots[position].x == value:
                continue
            cursor = value, float(np.interp(value, *np.asarray(bindings[field.name][0]).T))
            for accepted in convert(knots[:position] + (cursor,) + knots[position:], field.type).to_option().to_list():  # ruff: ignore[collection-literal-concatenation]
                changes[field.name] = accepted
        return Some(msgspec.structs.replace(record, **changes)) if changes else Nothing

    def grown(record: R, mean: npt.NDArray[np.float32], config: PyOpenColorIO.Config) -> BlenderResult[R]:
        bindings = _bindings(record)
        splines = {key: make_interp_spline(x, y, k=1) for key, (table, _) in bindings.items() for x, y in (np.unstack(np.asarray(table), axis=1),)}
        log = lin_to_log()
        logs: BlenderResult[tuple[PyOpenColorIO.CPUProcessor, ...]] = accumulated(
            processor(PyOpenColorIO.Config(), transform).map(PyOpenColorIO.Processor.getDefaultCPUProcessor)
            for transform in (log, PyOpenColorIO.GroupTransform([log], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE))
        ).map_error(lambda faults: (BlenderImagingFault(faults=faults),))

        def rgb(values: tuple[tuple[PyOpenColorIO.CPUProcessor, ...], npt.NDArray[np.float32]]) -> BlenderResult[R]:
            (forward, inverse), weights = values
            stops = applied(forward, mean)
            channels = Curves.red.__name__, Curves.green.__name__, Curves.blue.__name__
            output = np.asarray([splines[key](value) for key, value in zip(channels, stops)], dtype=np.float32)
            working_y = float(applied(inverse, output) @ weights)
            master = float(applied(forward, np.full(3, working_y, dtype=np.float32))[0])
            return inserted(record, dict(zip(channels, map(float, stops))) | {Curves.master.__name__: master}, bindings).to_result_with(lambda: unread)

        def hue(values: tuple[tuple[PyOpenColorIO.CPUProcessor, ...], PyOpenColorIO.CPUProcessor]) -> BlenderResult[R]:
            (forward, _inverse), conversion = values
            hue, saturation, luminance = map(float, applied(conversion, mean))
            stops = float(applied(forward, np.full(3, luminance, dtype=np.float32))[0])
            hue_hue, hue_sat, hue_lum, lum_sat, sat_sat, lum_lum, sat_lum = (
                HueCurves.hue_hue.__name__,
                HueCurves.hue_sat.__name__,
                HueCurves.hue_lum.__name__,
                HueCurves.lum_sat.__name__,
                HueCurves.sat_sat.__name__,
                HueCurves.lum_lum.__name__,
                HueCurves.sat_lum.__name__,
            )
            at = {hue_hue: hue, hue_sat: hue, hue_lum: hue, lum_sat: stops, sat_sat: saturation, lum_lum: stops}
            at[sat_lum] = float(np.prod([max(0.0, float(splines[key](at[key]))) for key in (sat_sat, lum_sat, hue_sat)]))
            return inserted(record, at, bindings).to_result_with(lambda: unread)

        return (
            Result[tuple[tuple[PyOpenColorIO.CPUProcessor, ...], npt.NDArray[np.float32]], BlenderFaults].bind(
                paired(logs, luma(config).map_error(lambda faults: (BlenderImagingFault(faults=faults),))), rgb
            )
            if isinstance(record, Curves)
            else Result[tuple[tuple[PyOpenColorIO.CPUProcessor, ...], PyOpenColorIO.CPUProcessor], BlenderFaults].bind(
                paired(
                    logs,
                    processor(PyOpenColorIO.Config(), PyOpenColorIO.FixedFunctionTransform(PyOpenColorIO.FIXED_FUNCTION_RGB_TO_HSY_LIN))
                    .map(PyOpenColorIO.Processor.getDefaultCPUProcessor)
                    .map_error(lambda faults: (BlenderImagingFault(faults=faults),)),
                ),
                hue,
            )
        )

    def read(values: tuple[Entry[R], PyOpenColorIO.Config]) -> BlenderResult[Option[str]]:
        held, config = values
        mean: BlenderResult[npt.NDArray[np.float32]] = (
            processor(config, PyOpenColorIO.ColorSpaceTransform(src=plane.colorspace, dst=PyOpenColorIO.ROLE_SCENE_LINEAR))
            .map_error(lambda faults: (BlenderImagingFault(faults=faults),))
            .map(lambda built: applied(built.getDefaultCPUProcessor(), covered).mean(axis=0, dtype=np.float64).astype(np.float32))
        )
        return (
            Result[npt.NDArray[np.float32], BlenderFaults]
            .bind(mean, lambda mean: grown(held.effect, mean, config))
            .bind(
                lambda changed: row.write(identity, entry.id_data, entry, Entry(effect=changed, blend=held.blend))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            )
            .map(lambda _: Some("Insert Knots"))
        )

    reading: BlenderResult[tuple[Entry[R], PyOpenColorIO.Config]] = paired(
        row.read(identity, entry), config.admitted(PyOpenColorIO.GetCurrentConfig(), ()).map_error(lambda faults: (BlenderImagingFault(faults=faults),))
    )
    return Result[tuple[Entry[R], PyOpenColorIO.Config], BlenderFaults].bind(reading, read)


# --- [COMPOSITION] ----------------------------------------------------------------------

curves: CompositorEffect[Curves] = CompositorEffect(
    record=Curves, graph=curved, straight=True, admitted=Some(admitted), registrables=(ViewerRead(consume=lambda identity, entry, plane: consumed(curves, identity, entry, plane)),)
)
hue_curves: CompositorEffect[HueCurves] = CompositorEffect(
    record=HueCurves, graph=hue_curved, straight=True, admitted=Some(admitted), registrables=(ViewerRead(consume=lambda identity, entry, plane: consumed(hue_curves, identity, entry, plane)),)
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["consumed", "curved", "curves", "hue_curved", "hue_curves"]
