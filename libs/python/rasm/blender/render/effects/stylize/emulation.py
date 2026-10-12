"""Native coded, transmitted, taped, received, and displayed video graphs."""

from collections.abc import Mapping
from enum import IntEnum
from fractions import Fraction
from itertools import product
from math import ceil, floor, tau

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from rasm.blender.data.rna.animation import sampled as sampled_channel
from rasm.blender.extension.identity import Identity, Key
from rasm.blender.faults import BlenderAbsentFault, BlenderFaults, BlenderImagingFault, BlenderResult
from rasm.blender.nodes.coordinates import framed, FrameSpace, pixels, sampled
from rasm.blender.nodes.graphs import Clock, End, ended, Fragment, math, menu, mixed, nested, node, NodeGroup, Operand, origin, switched, vector
from rasm.blender.nodes.interface import item_key, MenuItems
from rasm.blender.render.effects.generators.noise import gaussian
from rasm.blender.render.effects.optics.distortion import radial
from rasm.blender.render.effects.stack import camera_frame, CameraFrame, CompositorEffect, Context, Coverage, path, precise, Shell
from rasm.blender.render.effects.warp.transforms import extended
from rasm.blender.render.outputs.framing import capacity, FrameSize
from rasm.imaging.color.processors import unassociated
from rasm.imaging.effects.generators.noise import Estimate, Precision
from rasm.imaging.effects.stylize.emulation import Corruption, Display, Emulation, Ghost, Layout, Phosphors, Scanlines, Signal, Tape
from rasm.imaging.faults import accumulated, ImagingBufferFault, ImagingFaults, ImagingRepresentationFault, ImagingResult, paired
from rasm.imaging.geometry.stmaps import Extension
from rasm.imaging.parameters import Gated, Motion

# --- [TYPES] ----------------------------------------------------------------------------


class Noise(IntEnum):
    """Persistent decorrelation identities of the video path's random fields."""

    CELLS = 0
    BOUNCE = 1
    JITTER = 2
    MISTRACKING = 3
    DROPOUTS = 4
    LUMA = 5
    CHROMA = 6


# --- [OPERATIONS] -----------------------------------------------------------------------


def admitted(identity: Identity, scene: bpy.types.Scene, entry: Option[bpy.types.SceneCompositorEffect], record: Emulation, context: Context) -> BlenderResult[None]:
    """Admits the finite Tape canvases at every rendered frame before native writes.

    Args:
        identity: Extension identity resolving the entry's native inputs.
        scene: Render range, animation owner, and compositor receiving capacity.
        entry: Existing entry whose keyed channels override proposed scalar values.
        record: Proposed Emulation parameters.
        context: Evaluated entering raster dimensions and camera framing.

    Returns:
        Admission, native animation or framing faults, missing entering raster
        facts for enabled Tape, or the compositor's typed size requirement.
    """
    frames = tuple(range(scene.frame_start, scene.frame_end + 1, scene.frame_step))
    _, _, tape, _, _, lines_field, _, _ = Emulation.__struct_fields__

    def samples(key: str, proposed: float) -> BlenderResult[tuple[float, ...]]:
        return entry.map(lambda held: path(identity, held, key).bind(lambda at: sampled_channel(scene, at, frames, proposed=Some(proposed)))).default_with(lambda: Ok(tuple(proposed for _ in frames)))

    def taped(enabled: tuple[float, ...]) -> BlenderResult[None]:
        if not any(enabled):
            return Ok(None)

        def sized(height: int, scale: float, lines: tuple[float, ...], periods: tuple[float, ...]) -> BlenderResult[None]:
            share = Fraction(scale)
            margin = (1 - share) / 2
            first = (Fraction(1, 2 * height) - margin) / share
            last = (1 - Fraction(1, 2 * height) - margin) / share
            return accumulated(
                capacity(scene, size)
                for active, count, period in zip(enabled, lines, periods)
                if active
                for radius in (ceil(Fraction(count) * Fraction(period)),)
                for reached in (floor(last * Fraction(count)) - floor(first * Fraction(count)) + 1,)
                for size in ((1, 4 * radius + 1), (1, reached + 2 * radius))
            ).map(lambda _: None)

        _, value = Gated.__struct_fields__
        _, jitter_period, _, _, _, _, _, _, _, _, _ = Tape.__struct_fields__
        dimensions: BlenderResult[tuple[int, int]] = context.map(lambda held: held[0]).to_result_with(lambda: (BlenderAbsentFault(owner=Emulation.title, key="entering raster dimensions"),))
        return Result[tuple[int, int], BlenderFaults].bind(
            dimensions,
            lambda size: paired(
                FrameSize().bound(identity).bind(lambda framed: camera_frame(framed, scene)),
                paired(samples(lines_field, float(record.lines)), samples(item_key(tape, value, jitter_period), record.tape.value.jitter_period)),
            ).bind(lambda values: sized(size[0], values[0].scale, *values[1])),
        )

    return samples(tape, float(record.tape.enabled)).bind(taped)


def prepared(
    _identity: Identity,
    _scene: bpy.types.Scene,
    record: Emulation,
    _config: PyOpenColorIO.Config,
    source: OpenImageIO.ImageBuf,
    _precision: Precision,
    _frame: CameraFrame,
    _entry: bpy.types.SceneCompositorEffect,
) -> BlenderResult[tuple[Emulation, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]]:
    """Codes the captured associated display image before the remaining native sections.

    Args:
        _identity: Stage job's extension owner.
        _scene: Stage job's evaluated frame.
        record: Evaluated compression and following-section parameters.
        _config: Admitted color configuration; the capture already has the row's encoding.
        source: Associated display RGBA with the captured data and display windows.
        _precision: Numerical sampling policy; the native JPEG cycle has no sampled integral.
        _frame: Evaluated original camera units; coded resize uses the captured full window.
        _entry: Evaluated native stage entry.

    Returns:
        The same record and its published replacement image, or an empty mapping
        when coding is disabled. Native imaging refusals retain their typed cause.
    """
    if not record.compression.enabled:
        return Ok((record, {}))

    def buffered(image: OpenImageIO.ImageBuf) -> ImagingResult[OpenImageIO.ImageBuf]:
        return Error((ImagingBufferFault(request=image, error=image.geterror()),)) if image.has_error else Ok(image)

    def located(pixels: npt.NDArray[np.float32]) -> ImagingResult[OpenImageIO.ImageBuf]:
        image = OpenImageIO.ImageBuf(pixels)
        image.copy_metadata(source)
        image.set_origin(source.xbegin, source.ybegin, source.zbegin)
        image.roi_full = source.roi_full
        return buffered(image)

    def coded(image: OpenImageIO.ImageBuf) -> Option[OpenImageIO.ImageSpec]:
        if not (width := (record.compression.value.coded_width)).enabled:
            return Nothing
        original = image.spec()
        height = max(1, round(Fraction(original.full_height * width.value, original.full_width)))
        horizontal, vertical = Fraction(width.value, original.full_width), Fraction(height, original.full_height)
        left, bottom = floor((original.x - original.full_x) * horizontal), floor((original.y - original.full_y) * vertical)
        right, top = ceil((original.x + original.width - original.full_x) * horizontal), ceil((original.y + original.height - original.full_y) * vertical)
        spec = OpenImageIO.ImageSpec(original)
        spec.x, spec.y, spec.width, spec.height = left, bottom, right - left, top - bottom
        spec.full_x, spec.full_y, spec.full_width, spec.full_height = 0, 0, width.value, height
        return Some(spec)

    def compressed(plane: npt.NDArray[np.float32]) -> ImagingResult[OpenImageIO.ImageBuf]:
        straight = unassociated(plane)

        def associated(image: OpenImageIO.ImageBuf) -> ImagingResult[OpenImageIO.ImageBuf]:
            def restored(rgb: npt.NDArray[np.float32]) -> ImagingResult[OpenImageIO.ImageBuf]:
                straight[..., :3] = rgb
                alpha = straight[..., 3:]
                np.multiply(straight[..., :3], alpha, out=straight[..., :3], where=alpha != 0.0)
                return located(straight)

            return Result[npt.NDArray[np.float32], ImagingFaults].bind(
                Option.of_optional(image.get_pixels(OpenImageIO.FLOAT)).to_result_with(lambda: (ImagingBufferFault(request=image, error=image.geterror()),)), restored
            )

        return located(straight).bind(lambda image: buffered(OpenImageIO.ImageBufAlgo.channels(image, (0, 1, 2)))).bind(lambda rgb: record.compression.value.applied(rgb, coded(rgb))).bind(associated)

    try:
        return (
            Result[npt.NDArray[np.float32], ImagingFaults]
            .bind(Option.of_optional(source.get_pixels(OpenImageIO.FLOAT)).to_result_with(lambda: (ImagingBufferFault(request=source, error=source.geterror()),)), compressed)
            .map_error(lambda faults: (BlenderImagingFault(faults=faults),))
            .map(lambda image: (record, {Shell.image: image}))
        )
    except (TypeError, OverflowError, MemoryError) as error:
        return Error((BlenderImagingFault(faults=(ImagingRepresentationFault(request=record.compression.value, error=error),)),))


def raster_noise(key: str, lattice: Operand, offset: Operand, pattern: Operand) -> BlenderResult[tuple[tuple[Fragment, End], ...]]:
    """Constructs four Gaussian samples at one raster lattice and pattern.

    Args:
        key: Prefix for the raster field's native node keys.
        lattice: Raster sample or line coordinates in XY.
        offset: Seed and noise identity offset from the owning origin operation.
        pattern: Integer pattern index retained as the fourth noise coordinate.

    Returns:
        Four normal fragment/end pairs in cosine/sine pair order, or accumulated
        native graph faults. The enclosing row supplies full compositor precision.
    """
    return (
        vector(f"{key}_shifted", "ADD", lattice, offset)
        .bind(lambda shifted: node(f"{key}_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": shifted}, {}))
        .bind(
            lambda axes: accumulated(
                Fragment.joined(
                    node(f"{key}_stream_{stream}", bpy.types.ShaderNodeCombineXYZ, {"X": ended(f"{key}_axes", "X")(axes), "Y": ended(f"{key}_axes", "Y")(axes), "Z": stream}, {}),
                    node(f"{key}_uniform_{stream}", bpy.types.ShaderNodeTexWhiteNoise, {"Vector": End(node=f"{key}_stream_{stream}", socket="Vector"), "W": pattern}, {"noise_dimensions": "4D"}),
                ).map(ended(f"{key}_uniform_{stream}", "Value"))
                for stream in range(4)
            )
        )
        .bind(lambda uniforms: paired(gaussian(f"{key}_first", uniforms[0], uniforms[1]), gaussian(f"{key}_second", uniforms[2], uniforms[3])))
        .map(lambda pairs: (*pairs[0], *pairs[1]))
    )


def emulated(shell: Shell[Emulation]) -> BlenderResult[tuple[Fragment, End]]:
    """Builds corruption, tape, reception, and display after the native codec section.

    Args:
        shell: Codec-processed image, camera frame, and native parameter endpoints.

    Returns:
        Displayed color and complete graph, or accumulated native graph faults.
    """
    socket, source = shell.socket, shell.source
    _, value = Gated.__struct_fields__
    _, corruption_field, tape_field, signal_field, display_field, lines_field, motion, seed_field = Emulation.__struct_fields__
    speed, hold = Motion.__struct_fields__
    corruption, tape, signal, display = ((field, value) for field in (corruption_field, tape_field, signal_field, display_field))
    seed, lines = socket(seed_field), socket(lines_field)
    clock = Clock(key="clock", speed=socket(motion, speed), hold=socket(motion, hold))
    gates = {field: item_key(field, "gate") for field in (corruption_field, tape_field, signal_field, display_field)}
    center = 128.0 / 255.0

    def built(frame: FrameSpace, time: Operand, pattern: Operand, lens: NodeGroup) -> BlenderResult[tuple[Fragment, End]]:
        x, y = End(node="fraction", socket="X"), End(node="fraction", socket="Y")
        aspect = End(node="aspect")

        def sample(
            key: str, image: Operand, position: Operand, across: Extension = Extension.EXTEND, up: Extension = Extension.EXTEND, interpolation: str = "Bilinear"
        ) -> BlenderResult[tuple[Fragment, End]]:
            return sampled(f"{key}_uv", frame, position).bind(
                lambda uv: (
                    extended(key, image, uv, across.label, MenuItems.of(Extension), vertical=Some((up.label, MenuItems.of(Extension))))
                    if interpolation == "Bilinear"
                    else node(key, bpy.types.CompositorNodeMapUV, {"Image": image, "UV": uv, "Interpolation": interpolation, "Extension X": across.label, "Extension Y": up.label}, {}).map(
                        ended(key, "Image")
                    )
                )
            )

        def shifted(
            key: str, image: Operand, horizontal: Operand, vertical: Operand = y, across: Extension = Extension.EXTEND, up: Extension = Extension.EXTEND
        ) -> BlenderResult[tuple[Fragment, End]]:
            return node(f"{key}_at", bpy.types.ShaderNodeCombineXYZ, {"X": horizontal, "Y": vertical}, {}).map(ended(f"{key}_at", "Vector")).bind(lambda at: sample(key, image, at, across, up))

        def banded(key: str, image: Operand, horizontal: Operand, vertical: Operand) -> BlenderResult[tuple[Fragment, End]]:
            return (
                node(f"{key}_fraction", bpy.types.ShaderNodeCombineXYZ, {"X": horizontal, "Y": vertical}, {})
                .map(ended(f"{key}_fraction", "Vector"))
                .bind(lambda radius: vector(f"{key}_radius", "MULTIPLY", radius, frame.extent))
                .bind(lambda radius: pixels(f"{key}_pixels", frame, radius, "VECTOR"))
                .bind(lambda radius: node(key, bpy.types.CompositorNodeBlur, {"Image": image, "Size": radius, "Type": "Gaussian"}, {}))
                .map(ended(key, "Image"))
            )

        def draw(key: str, identity: Noise, lattice: Operand) -> BlenderResult[Fragment]:
            return (
                origin(f"{key}_origin", seed, identity)
                .bind(lambda offset: vector(f"{key}_coordinate", "ADD", lattice, offset))
                .bind(lambda coordinate: node(key, bpy.types.ShaderNodeTexWhiteNoise, {"Vector": coordinate, "W": pattern}, {"noise_dimensions": "4D"}))
            )

        def normal(key: str, identity: Noise, lattice: Operand) -> BlenderResult[tuple[tuple[Fragment, End], ...]]:
            return origin(f"{key}_origin", seed, identity).bind(lambda offset: raster_noise(key, lattice, offset, pattern))

        def corrupted(image: Operand) -> BlenderResult[tuple[Fragment, End]]:
            coverage, cell_width, cell_height, displacement, split, chroma = Corruption.__struct_fields__
            return Fragment.joined(
                node("cell_size", bpy.types.ShaderNodeCombineXYZ, {"X": socket(*corruption, cell_width), "Y": socket(*corruption, cell_height), "Z": 1.0}, {}),
                vector("corruption_scaled", "DIVIDE", frame.point, End(node="cell_size", socket="Vector")),
                vector("corruption_cell", "FLOOR", End(node="corruption_scaled", socket="Vector")),
                draw("cells", Noise.CELLS, End(node="corruption_cell", socket="Vector")),
                node("cell_color", bpy.types.CompositorNodeSeparateColor, {"Image": End(node="cells", socket="Color")}, {"mode": "RGB"}),
                math("struck", "LESS_THAN", End(node="cells", socket="Value"), socket(*corruption, coverage)),
                math("cell_signed", "MULTIPLY_ADD", End(node="cell_color", socket="Red"), 2.0, -1.0),
                math("cell_displacement", "MULTIPLY", End(node="cell_signed"), socket(*corruption, displacement)),
                math("cell_shift", "MULTIPLY", End(node="cell_displacement"), End(node="struck")),
                math("cell_x", "ADD", x, End(node="cell_shift")),
                *(
                    part
                    for channel, direction in (("Red", -1.0), ("Green", 0.0), ("Blue", 1.0))
                    for part in (
                        math(f"split_{channel.lower()}", "MULTIPLY_ADD", socket(*corruption, split), direction, End(node="cell_x")),
                        shifted(f"cell_{channel.lower()}", image, End(node=f"split_{channel.lower()}"), across=Extension.REPEAT, up=Extension.CLIP),
                        node(f"cell_{channel.lower()}_channels", bpy.types.CompositorNodeSeparateColor, {"Image": End(node=f"cell_{channel.lower()}", socket="Image")}, {"mode": "RGB"}),
                    )
                ),
                node("shifted", bpy.types.CompositorNodeCombineColor, {channel: End(node=f"cell_{channel.lower()}_channels", socket=channel) for channel in ("Red", "Green", "Blue")}, {"mode": "RGB"}),
                node("corruption_channels", bpy.types.CompositorNodeSeparateColor, {"Image": End(node="shifted", socket="Image")}, {"mode": "YCC", "ycc_mode": "ITUBT601"}),
                math("cell_exponent", "MULTIPLY_ADD", End(node="cell_color", socket="Green"), 4.0, -2.0),
                math("cell_power", "POWER", 2.0, End(node="cell_exponent")),
                math("cell_chroma", "MULTIPLY", socket(*corruption, chroma), End(node="struck")),
                *(
                    part
                    for channel in ("Green", "Blue")
                    for part in (
                        math(f"cell_{channel.lower()}_centered", "SUBTRACT", End(node="corruption_channels", socket=channel), center),
                        math(f"cell_{channel.lower()}_sign", "SIGN", End(node=f"cell_{channel.lower()}_centered")),
                        math(f"cell_{channel.lower()}_absolute", "ABSOLUTE", End(node=f"cell_{channel.lower()}_centered")),
                        math(f"cell_{channel.lower()}_doubled", "MULTIPLY", End(node=f"cell_{channel.lower()}_absolute"), 2.0),
                        math(f"cell_{channel.lower()}_raised", "POWER", End(node=f"cell_{channel.lower()}_doubled"), End(node="cell_power")),
                        math(f"cell_{channel.lower()}_signed", "MULTIPLY", End(node=f"cell_{channel.lower()}_sign"), End(node=f"cell_{channel.lower()}_raised")),
                        math(f"cell_{channel.lower()}_offset", "MULTIPLY_ADD", End(node=f"cell_{channel.lower()}_signed"), 0.5, center),
                        mixed(f"cell_{channel.lower()}_mixed", "FLOAT", End(node="cell_chroma"), End(node="corruption_channels", socket=channel), End(node=f"cell_{channel.lower()}_offset")),
                    )
                ),
                node(
                    "corrupted",
                    bpy.types.CompositorNodeCombineColor,
                    {"Red": End(node="corruption_channels", socket="Red"), **{channel: End(node=f"cell_{channel.lower()}_mixed", socket="Result_Float") for channel in ("Green", "Blue")}},
                    {"mode": "YCC", "ycc_mode": "ITUBT601"},
                ),
                switched(gates[corruption_field], "RGBA", socket(corruption_field), image, End(node="corrupted", socket="Image")),
            ).map(ended(gates[corruption_field], "Output"))

        def played(image: Operand) -> BlenderResult[tuple[Fragment, End]]:
            jitter, jitter_period, bounce, head_switching, skew, tracking_shift, tracking_height, tracking_rate, mistracking, dropouts, dropout_length = Tape.__struct_fields__
            return Fragment.joined(
                node("dimensions", bpy.types.CompositorNodeImageInfo, {"Image": source}, {}),
                node("dimensions_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": End(node="dimensions", socket="Dimensions")}, {}),
                math("half_pixel_y", "DIVIDE", 0.5, End(node="dimensions_axes", socket="Y")),
                math("margin", "MULTIPLY_ADD", shell.scale, -0.5, 0.5),
                math("first_y", "SUBTRACT", End(node="half_pixel_y"), End(node="margin")),
                math("first_fraction", "DIVIDE", End(node="first_y"), shell.scale),
                math("first_scan", "MULTIPLY", End(node="first_fraction"), lines),
                math("first_line", "FLOOR", End(node="first_scan")),
                math("last_y", "SUBTRACT", 1.0, End(node="half_pixel_y")),
                math("last_camera", "SUBTRACT", End(node="last_y"), End(node="margin")),
                math("last_fraction", "DIVIDE", End(node="last_camera"), shell.scale),
                math("last_scan", "MULTIPLY", End(node="last_fraction"), lines),
                math("last_line", "FLOOR", End(node="last_scan")),
                math("reached_span", "SUBTRACT", End(node="last_line"), End(node="first_line")),
                math("reached_lines", "ADD", End(node="reached_span"), 1.0),
                math("jitter_radius", "MULTIPLY", socket(*tape, jitter_period), lines),
                math("jitter_support", "CEIL", End(node="jitter_radius")),
                math("jitter_padding", "MULTIPLY", End(node="jitter_support"), 2.0),
                math("jitter_height", "ADD", End(node="reached_lines"), End(node="jitter_padding")),
                math("impulse_height", "MULTIPLY_ADD", End(node="jitter_support"), 4.0, 1.0),
                node("jitter_size", bpy.types.ShaderNodeCombineXYZ, {"X": 1.0, "Y": End(node="jitter_height")}, {}),
                node("impulse_size", bpy.types.ShaderNodeCombineXYZ, {"X": 1.0, "Y": End(node="impulse_height")}, {}),
                node("jitter_canvas", bpy.types.CompositorNodeBlankImage, {"Size": End(node="jitter_size", socket="Vector")}, {}),
                node("impulse_canvas", bpy.types.CompositorNodeBlankImage, {"Size": End(node="impulse_size", socket="Vector")}, {}),
                node("jitter_coordinates", bpy.types.CompositorNodeImageCoordinates, {"Image": End(node="jitter_canvas", socket="Image")}, {}),
                node("jitter_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": End(node="jitter_coordinates", socket="Pixel")}, {}),
                math("jitter_origin_line", "SUBTRACT", End(node="first_line"), End(node="jitter_support")),
                math("jitter_conceptual_line", "ADD", End(node="jitter_axes", socket="Y"), End(node="jitter_origin_line")),
                node("jitter_lattice", bpy.types.ShaderNodeCombineXYZ, {"X": End(node="jitter_conceptual_line")}, {}),
                normal("jitter_normal", Noise.JITTER, End(node="jitter_lattice", socket="Vector")).bind(
                    lambda normals: node("jitter_blur", bpy.types.CompositorNodeBlur, {"Image": normals[0], "Size": End(node="jitter_filter_size", socket="Vector"), "Type": "Gaussian"}, {})
                ),
                node("jitter_filter_size", bpy.types.ShaderNodeCombineXYZ, {"Y": End(node="jitter_radius")}, {}),
                node("impulse_coordinates", bpy.types.CompositorNodeImageCoordinates, {"Image": End(node="impulse_canvas", socket="Image")}, {}),
                node("impulse_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": End(node="impulse_coordinates", socket="Pixel")}, {}),
                math("impulse", "COMPARE", End(node="impulse_axes", socket="Y"), End(node="jitter_padding"), 0.0),
                node("impulse_first", bpy.types.CompositorNodeBlur, {"Image": End(node="impulse"), "Size": End(node="jitter_filter_size", socket="Vector"), "Type": "Gaussian"}, {}),
                node(
                    "impulse_second",
                    bpy.types.CompositorNodeBlur,
                    {"Image": End(node="impulse_first", socket="Image"), "Size": End(node="jitter_filter_size", socket="Vector"), "Type": "Gaussian"},
                    {},
                ),
                node(
                    "impulse_center",
                    bpy.types.CompositorNodeMapUV,
                    {"Image": End(node="impulse_second", socket="Image"), "UV": (0.5, 0.5, 1.0), "Interpolation": "Nearest", "Extension X": "Clip", "Extension Y": "Clip"},
                    {},
                ),
                node("impulse_value", bpy.types.CompositorNodeSeparateColor, {"Image": End(node="impulse_center", socket="Image")}, {"mode": "RGB"}),
                node("jitter_value", bpy.types.CompositorNodeSeparateColor, {"Image": End(node="jitter_blur", socket="Image")}, {"mode": "RGB"}),
                math("jitter_norm", "SQRT", End(node="impulse_value", socket="Red")),
                math("jitter_normalized", "DIVIDE", End(node="jitter_value", socket="Red"), End(node="jitter_norm")),
                math("jitter_relative_line", "SUBTRACT", End(node="line"), End(node="first_line")),
                math("jitter_canvas_line", "ADD", End(node="jitter_relative_line"), End(node="jitter_support")),
                math("jitter_canvas_center", "ADD", End(node="jitter_canvas_line"), 0.5),
                math("jitter_canvas_y", "DIVIDE", End(node="jitter_canvas_center"), End(node="jitter_height")),
                node("jitter_uv", bpy.types.ShaderNodeCombineXYZ, {"X": 0.5, "Y": End(node="jitter_canvas_y"), "Z": 1.0}, {}),
                node(
                    "jitter_sample",
                    bpy.types.CompositorNodeMapUV,
                    {"Image": End(node="jitter_normalized"), "UV": End(node="jitter_uv", socket="Vector"), "Interpolation": "Nearest", "Extension X": "Clip", "Extension Y": "Clip"},
                    {},
                ),
                math("jitter_shift", "MULTIPLY", End(node="jitter_sample", socket="Image"), socket(*tape, jitter)),
                draw("bounce", Noise.BOUNCE, (0.0, 0.0, 0.0)),
                math("bounce_signed", "MULTIPLY_ADD", End(node="bounce", socket="Value"), 2.0, -1.0),
                math("bounce_shift", "MULTIPLY", End(node="bounce_signed"), socket(*tape, bounce)),
                math("head_t", "DIVIDE", y, socket(*tape, head_switching)),
                math("head_rising", "DIVIDE", End(node="head_t"), 0.9),
                math("head_falling", "MULTIPLY_ADD", End(node="head_t"), -10.0, 10.0),
                math("head_shape", "MINIMUM", End(node="head_rising"), End(node="head_falling")),
                math("head_negative", "LESS_THAN", y, 0.0),
                math("head_above", "SUBTRACT", 1.0, End(node="head_negative")),
                math("head_below", "LESS_THAN", y, socket(*tape, head_switching)),
                math("head_band", "MULTIPLY", End(node="head_above"), End(node="head_below")),
                math("head_active", "MULTIPLY", End(node="head_shape"), End(node="head_band")),
                math("head_shift", "MULTIPLY", End(node="head_active"), socket(*tape, skew)),
                math("tracking_at", "MULTIPLY", time, socket(*tape, tracking_rate)),
                math("tracking_center", "FRACT", End(node="tracking_at")),
                math("tracking_delta", "SUBTRACT", y, End(node="tracking_center")),
                math("tracking_signed", "WRAP", End(node="tracking_delta"), 0.5, -0.5),
                math("tracking_distance", "ABSOLUTE", End(node="tracking_signed")),
                math("tracking_relative", "DIVIDE", End(node="tracking_distance"), socket(*tape, tracking_height)),
                math("tracking_weight", "MULTIPLY_ADD", End(node="tracking_relative"), -2.0, 1.0, clamp=True),
                math("tracking_shift", "MULTIPLY", End(node="tracking_weight"), socket(*tape, tracking_shift)),
                math("played_jitter", "ADD", x, End(node="jitter_shift")),
                math("played_head", "ADD", End(node="played_jitter"), End(node="head_shift")),
                math("played_x", "ADD", End(node="played_head"), End(node="tracking_shift")),
                math("played_y", "ADD", y, End(node="bounce_shift")),
                shifted("played", image, End(node="played_x"), End(node="played_y"), up=Extension.CLIP),
                node("played_channels", bpy.types.CompositorNodeSeparateColor, {"Image": End(node="played", socket="Image")}, {"mode": "YCC", "ycc_mode": "ITUBT601"}),
                draw("mistracking", Noise.MISTRACKING, End(node="line_cell", socket="Vector")),
                math("mistracking_weight", "MULTIPLY", End(node="tracking_weight"), socket(*tape, mistracking)),
                mixed("mistracked_luma", "FLOAT", End(node="mistracking_weight"), End(node="played_channels", socket="Red"), End(node="mistracking", socket="Value")),
                node(
                    "mistracked",
                    bpy.types.CompositorNodeCombineColor,
                    {"Red": End(node="mistracked_luma", socket="Result_Float"), "Green": End(node="played_channels", socket="Green"), "Blue": End(node="played_channels", socket="Blue")},
                    {"mode": "YCC", "ycc_mode": "ITUBT601"},
                ),
                math("dropout_at", "DIVIDE", x, socket(*tape, dropout_length)),
                math("dropout_cell", "FLOOR", End(node="dropout_at")),
                node("dropout_lattice", bpy.types.ShaderNodeCombineXYZ, {"X": End(node="dropout_cell"), "Y": End(node="line")}, {}),
                draw("dropouts", Noise.DROPOUTS, End(node="dropout_lattice", socket="Vector")),
                math("dropout_struck", "LESS_THAN", End(node="dropouts", socket="Value"), socket(*tape, dropouts)),
                math("dropout_fraction", "FRACT", End(node="dropout_at")),
                math("dropout_fade", "SUBTRACT", 1.0, End(node="dropout_fraction")),
                math("dropout_weight", "MULTIPLY", End(node="dropout_fade"), End(node="dropout_struck")),
                mixed("dropout_picture", "RGBA", End(node="dropout_weight"), End(node="mistracked", socket="Image"), (1.0, 1.0, 1.0, 1.0)),
                switched(gates[tape_field], "RGBA", socket(tape_field), image, End(node="dropout_picture", socket="Result_Color")),
            ).map(ended(gates[tape_field], "Output"))

        def received(image: Operand) -> BlenderResult[tuple[Fragment, End]]:
            luma_blur, chroma_blur, chroma_vertical, chroma_delay, saturation, ringing, cross_color, noise_field, chroma_noise, hum, ghost_field = Signal.__struct_fields__
            amplitude, delay = Ghost.__struct_fields__
            ghost = (*signal, ghost_field, value)
            return paired(normal("luma_noise", Noise.LUMA, End(node="sample_cell", socket="Vector")), normal("chroma_noise", Noise.CHROMA, End(node="sample_cell", socket="Vector"))).bind(
                lambda normals: Fragment.joined(
                    node("signal_channels", bpy.types.CompositorNodeSeparateColor, {"Image": image}, {"mode": "YCC", "ycc_mode": "ITUBT601"}),
                    banded("carrier_low", End(node="signal_channels", socket="Red"), 1.0 / 377.0, 0.0),
                    math("carrier_detail", "SUBTRACT", End(node="signal_channels", socket="Red"), End(node="carrier_low", socket="Image")),
                    math("carrier_strength", "MULTIPLY", End(node="carrier_detail"), socket(*signal, cross_color)),
                    math("carrier_scan", "ADD", End(node="line"), pattern),
                    math("carrier_half_line", "MULTIPLY", End(node="carrier_scan"), 0.5),
                    math("carrier_at", "MULTIPLY_ADD", x, 188.3, End(node="carrier_half_line")),
                    math("carrier_phase", "MULTIPLY", End(node="carrier_at"), tau),
                    math("carrier_cosine", "COSINE", End(node="carrier_phase")),
                    math("carrier_sine", "SINE", End(node="carrier_phase")),
                    math("luma_noisy", "MULTIPLY_ADD", normals[0][0], socket(*signal, noise_field), End(node="signal_channels", socket="Red")),
                    *(
                        part
                        for channel, wave, noise in (("Green", "cosine", normals[1][0]), ("Blue", "sine", normals[1][1]))
                        for part in (
                            math(f"carrier_{channel.lower()}", "MULTIPLY_ADD", End(node="carrier_strength"), End(node=f"carrier_{wave}"), End(node="signal_channels", socket=channel)),
                            math(f"noisy_{channel.lower()}", "MULTIPLY_ADD", noise, socket(*signal, chroma_noise), End(node=f"carrier_{channel.lower()}")),
                        )
                    ),
                    node("chroma_noisy", bpy.types.CompositorNodeCombineColor, {"Red": End(node="noisy_green"), "Green": End(node="noisy_blue")}, {"mode": "RGB"}),
                    banded("luma_band", End(node="luma_noisy"), socket(*signal, luma_blur), 0.0),
                    banded("chroma_band", End(node="chroma_noisy", socket="Image"), socket(*signal, chroma_blur), socket(*signal, chroma_vertical)),
                    math("chroma_x", "SUBTRACT", x, socket(*signal, chroma_delay)),
                    shifted("chroma_delayed", End(node="chroma_band", socket="Image"), End(node="chroma_x")),
                    node("chroma_channels", bpy.types.CompositorNodeSeparateColor, {"Image": End(node="chroma_delayed", socket="Image")}, {"mode": "RGB"}),
                    math("ringing_radius", "DIVIDE", socket(*signal, luma_blur), 3.0),
                    *(
                        part
                        for key, direction in (("left", -1.0), ("right", 1.0))
                        for part in (
                            math(f"ringing_{key}_x", "MULTIPLY_ADD", End(node="ringing_radius"), direction, x),
                            shifted(f"ringing_{key}", End(node="luma_band", socket="Image"), End(node=f"ringing_{key}_x")),
                        )
                    ),
                    math("ringing_neighbors", "ADD", End(node="ringing_left", socket="Image"), End(node="ringing_right", socket="Image")),
                    math("ringing_second", "MULTIPLY_ADD", End(node="luma_band", socket="Image"), -2.0, End(node="ringing_neighbors")),
                    math("ringing_negative", "MULTIPLY", End(node="ringing_second"), -1.0),
                    math("luma_ringed", "MULTIPLY_ADD", End(node="ringing_negative"), socket(*signal, ringing), End(node="luma_band", socket="Image")),
                    *(
                        part
                        for output, channel in (("Green", "Red"), ("Blue", "Green"))
                        for part in (
                            math(f"saturated_{output.lower()}_centered", "SUBTRACT", End(node="chroma_channels", socket=channel), center),
                            math(f"saturated_{output.lower()}", "MULTIPLY_ADD", End(node=f"saturated_{output.lower()}_centered"), socket(*signal, saturation), center),
                        )
                    ),
                    node(
                        "signal_packed",
                        bpy.types.CompositorNodeCombineColor,
                        {"Red": End(node="luma_ringed"), "Green": End(node="saturated_green"), "Blue": End(node="saturated_blue")},
                        {"mode": "RGB"},
                    ),
                    math("ghost_x", "SUBTRACT", x, socket(*ghost, delay)),
                    shifted("ghost_copy", End(node="signal_packed", socket="Image"), End(node="ghost_x")),
                    math("ghost_denominator", "ADD", socket(*ghost, amplitude), 1.0),
                    math("ghost_weight", "DIVIDE", socket(*ghost, amplitude), End(node="ghost_denominator")),
                    mixed("ghosted", "RGBA", End(node="ghost_weight"), End(node="signal_packed", socket="Image"), End(node="ghost_copy", socket="Image")),
                    switched(item_key(*signal, ghost_field, "gate"), "RGBA", socket(*signal, ghost_field), End(node="signal_packed", socket="Image"), End(node="ghosted", socket="Result_Color")),
                    node("received_channels", bpy.types.CompositorNodeSeparateColor, {"Image": End(node=item_key(*signal, ghost_field, "gate"), socket="Output")}, {"mode": "RGB"}),
                    math("hum_at", "MULTIPLY_ADD", time, -0.002, y),
                    math("hum_phase", "MULTIPLY", End(node="hum_at"), tau),
                    math("hum_sine", "SINE", End(node="hum_phase")),
                    math("hum_wave", "MULTIPLY_ADD", End(node="hum_sine"), 0.5, 0.5),
                    math("hum_loss", "MULTIPLY", End(node="hum_wave"), socket(*signal, hum)),
                    math("hum_gain", "SUBTRACT", 1.0, End(node="hum_loss")),
                    math("received_luma", "MULTIPLY", End(node="received_channels", socket="Red"), End(node="hum_gain")),
                    node(
                        "received",
                        bpy.types.CompositorNodeCombineColor,
                        {"Red": End(node="received_luma"), "Green": End(node="received_channels", socket="Green"), "Blue": End(node="received_channels", socket="Blue")},
                        {"mode": "YCC", "ycc_mode": "ITUBT601"},
                    ),
                    switched(gates[signal_field], "RGBA", socket(signal_field), image, End(node="received", socket="Image")),
                ).map(ended(gates[signal_field], "Output"))
            )

        def primitive(key: str, at: Operand, gain: Operand, start: Operand, width: Operand) -> BlenderResult[tuple[Fragment, End]]:
            return Fragment.joined(
                math(f"{key}_period", "DIVIDE", at, 2.0),
                math(f"{key}_whole", "FLOOR", End(node=f"{key}_period")),
                math(f"{key}_fraction", "MULTIPLY_ADD", End(node=f"{key}_whole"), -2.0, at),
                math(f"{key}_shifted", "SUBTRACT", End(node=f"{key}_fraction"), start),
                math(f"{key}_positive", "MAXIMUM", End(node=f"{key}_shifted"), 0.0),
                math(f"{key}_partial", "MINIMUM", End(node=f"{key}_positive"), width),
                math(key, "MULTIPLY_ADD", End(node=f"{key}_whole"), gain, End(node=f"{key}_partial")),
            ).map(ended(key, "Value"))

        def displayed(image: Operand) -> BlenderResult[tuple[Fragment, End]]:
            glow_field, scanlines, phosphors_field, curvature, roll, collapse = Display.__struct_fields__
            strength, interlaced = Scanlines.__struct_fields__
            layout, triads, strength_phosphors = Phosphors.__struct_fields__
            scan = (*display, scanlines, value)
            phosphors = (*display, phosphors_field, value)
            glow = (*display, glow_field)
            scan_gate = item_key(*display, scanlines, "gate")
            phosphor_gate = item_key(*display, phosphors_field, "gate")
            glow_gate = item_key(*glow, "gate")
            cells = tuple(product(range(2), repeat=2))
            return Fragment.joined(
                math("roll_y", "MULTIPLY_ADD", socket(*display, roll), time, y),
                shifted("rolled", image, x, End(node="roll_y"), up=Extension.REPEAT),
                math("minimum_span", "DIVIDE", 1.0, lines),
                math("collapse_y_raw", "MULTIPLY_ADD", socket(*display, collapse), -2.0, 1.0),
                math("collapse_y", "MAXIMUM", End(node="collapse_y_raw"), End(node="minimum_span")),
                math("collapse_x_raw", "ADD", End(node="collapse_y_raw"), 1.0),
                math("collapse_x_positive", "MAXIMUM", End(node="collapse_x_raw"), End(node="minimum_span")),
                math("collapse_x", "MINIMUM", End(node="collapse_x_positive"), 1.0),
                node("collapse_span", bpy.types.ShaderNodeCombineXYZ, {"X": End(node="collapse_x"), "Y": End(node="collapse_y"), "Z": 1.0}, {}),
                vector("collapse_centered", "SUBTRACT", frame.fraction, (0.5, 0.5, 0.0)),
                vector("collapse_relative", "DIVIDE", End(node="collapse_centered", socket="Vector"), End(node="collapse_span", socket="Vector")),
                vector("collapse_at", "ADD", End(node="collapse_relative", socket="Vector"), (0.5, 0.5, 0.0)),
                sample("collapse_sample", End(node="rolled", socket="Image"), End(node="collapse_at", socket="Vector"), Extension.CLIP, Extension.CLIP),
                math("collapse_area", "MULTIPLY", End(node="collapse_x"), End(node="collapse_y")),
                vector("collapsed", "DIVIDE", End(node="collapse_sample", socket="Image"), End(node="collapse_area")),
                node("glow_fraction", bpy.types.ShaderNodeCombineXYZ, {"X": socket(*glow, value), "Y": socket(*glow, value)}, {}),
                pixels("glow_pixels", frame, End(node="glow_fraction", socket="Vector"), "VECTOR"),
                node("glow_first", bpy.types.CompositorNodeBlur, {"Image": End(node="collapsed", socket="Vector"), "Size": End(node="glow_pixels", socket="Vector"), "Type": "Gaussian"}, {}),
                vector("glow_wide_radius", "MULTIPLY", End(node="glow_pixels", socket="Vector"), (4.0, 1.0, 0.0)),
                node("glow_wide", bpy.types.CompositorNodeBlur, {"Image": End(node="glow_first", socket="Image"), "Size": End(node="glow_wide_radius", socket="Vector"), "Type": "Gaussian"}, {}),
                mixed("glowing", "RGBA", 0.890909, End(node="glow_wide", socket="Image"), End(node="glow_first", socket="Image")),
                switched(glow_gate, "RGBA", socket(*glow), End(node="collapsed", socket="Vector"), End(node="glowing", socket="Result_Color")),
                node("display_dimensions", bpy.types.CompositorNodeImageInfo, {"Image": source}, {}),
                node("display_dimension_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": End(node="display_dimensions", socket="Dimensions")}, {}),
                *(
                    part
                    for axis in ("X", "Y")
                    for part in (
                        math(f"camera_pixels_{axis.lower()}", "MULTIPLY", End(node="display_dimension_axes", socket=axis), shell.scale),
                        math(f"half_footprint_{axis.lower()}", "DIVIDE", 0.5, End(node=f"camera_pixels_{axis.lower()}")),
                    )
                ),
                math("field_parity", "PINGPONG", pattern, 1.0),
                *(
                    part
                    for end, sign in (("low", -1.0), ("high", 1.0))
                    for part in (
                        math(f"scan_{end}_y", "MULTIPLY_ADD", End(node="half_footprint_y"), sign, y),
                        math(f"scan_{end}_centered", "SUBTRACT", End(node=f"scan_{end}_y"), 0.5),
                        math(f"scan_{end}_relative", "DIVIDE", End(node=f"scan_{end}_centered"), End(node="collapse_y")),
                        math(f"scan_{end}_row", "ADD", End(node=f"scan_{end}_relative"), 0.5),
                        math(f"scan_{end}_raster", "MULTIPLY", End(node=f"scan_{end}_row"), lines),
                        math(f"scan_{end}_field_sum", "ADD", End(node=f"scan_{end}_raster"), End(node="field_parity")),
                        math(f"scan_{end}_field", "MULTIPLY", End(node=f"scan_{end}_field_sum"), 0.5),
                        switched(f"scan_{end}", "FLOAT", socket(*scan, interlaced), End(node=f"scan_{end}_raster"), End(node=f"scan_{end}_field")),
                        math(f"scan_{end}_phase", "MULTIPLY", End(node=f"scan_{end}", socket="Output"), tau),
                        math(f"scan_{end}_sine", "SINE", End(node=f"scan_{end}_phase")),
                    )
                ),
                math("scan_integral", "SUBTRACT", End(node="scan_high_sine"), End(node="scan_low_sine")),
                math("scan_span", "SUBTRACT", End(node="scan_high", socket="Output"), End(node="scan_low", socket="Output")),
                math("scan_denominator", "MULTIPLY", End(node="scan_span"), 2.0 * tau),
                math("scan_relative", "DIVIDE", End(node="scan_integral"), End(node="scan_denominator")),
                math("scan_mean", "ADD", End(node="scan_relative"), 0.5),
                math("scan_loss", "MULTIPLY", End(node="scan_mean"), socket(*scan, strength)),
                math("scan_light", "SUBTRACT", 1.0, End(node="scan_loss")),
                math("scan_period_mean", "MULTIPLY_ADD", socket(*scan, strength), -0.5, 1.0),
                math("scan_gain", "DIVIDE", End(node="scan_light"), End(node="scan_period_mean")),
                vector("scanned", "SCALE", End(node=glow_gate, socket="Output"), End(node="scan_gain")),
                switched(scan_gate, "RGBA", socket(*display, scanlines), End(node=glow_gate, socket="Output"), End(node="scanned", socket="Vector")),
                menu("layout", "VECTOR", socket(*phosphors, layout), shell.items[*phosphors, layout], {member.label: member.geometry for member in Layout}),
                node("layout_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": End(node="layout", socket="Output")}, {}),
                math("matrix_fill", "SUBTRACT", 1.0, End(node="layout_axes", socket="X")),
                math("triad_x", "MULTIPLY", x, socket(*phosphors, triads)),
                math("triad_rows", "DIVIDE", socket(*phosphors, triads), aspect),
                math("triad_y", "MULTIPLY", y, End(node="triad_rows")),
                math("triad_half_x", "MULTIPLY", End(node="half_footprint_x"), socket(*phosphors, triads)),
                math("triad_half_y", "MULTIPLY", End(node="half_footprint_y"), End(node="triad_rows")),
                *(
                    part
                    for parity in range(2)
                    for axis, stagger in (("x", "Z"), ("y", "Y"))
                    for part in (
                        math(f"triad_{axis}_{parity}", "MULTIPLY_ADD", End(node="layout_axes", socket=stagger), parity, End(node=f"triad_{axis}")),
                        *(
                            math(f"triad_{axis}_{parity}_{end}", "MULTIPLY_ADD", End(node=f"triad_half_{axis}"), sign, End(node=f"triad_{axis}_{parity}"))
                            for end, sign in (("low", -1.0), ("high", 1.0))
                        ),
                    )
                ),
                math("matrix_span", "MULTIPLY", End(node="triad_half_y"), 2.0),
                math("stripe_span", "MULTIPLY", End(node="triad_half_x"), 2.0),
                math("phosphor_area", "MULTIPLY", End(node="matrix_span"), End(node="stripe_span")),
                *(
                    part
                    for row, column in cells
                    for part in (
                        *(primitive(f"matrix_{row}_{column}_{end}", End(node=f"triad_y_{column}_{end}"), End(node="matrix_fill"), row, End(node="matrix_fill")) for end in ("low", "high")),
                        math(f"matrix_{row}_{column}", "SUBTRACT", End(node=f"matrix_{row}_{column}_high"), End(node=f"matrix_{row}_{column}_low")),
                    )
                ),
                math("phosphor_plain", "SUBTRACT", 1.0, socket(*phosphors, strength_phosphors)),
                *(
                    part
                    for index, channel in enumerate(("Red", "Green", "Blue"))
                    for part in (
                        *(
                            part
                            for row, column in cells
                            for part in (
                                *(primitive(f"stripe_{channel.lower()}_{row}_{column}_{end}", End(node=f"triad_x_{row}_{end}"), 1.0 / 3.0, column + index / 3.0, 1.0 / 3.0) for end in ("low", "high")),
                                math(
                                    f"stripe_{channel.lower()}_{row}_{column}",
                                    "SUBTRACT",
                                    End(node=f"stripe_{channel.lower()}_{row}_{column}_high"),
                                    End(node=f"stripe_{channel.lower()}_{row}_{column}_low"),
                                ),
                                math(f"phosphor_{channel.lower()}_{row}_{column}", "MULTIPLY", End(node=f"stripe_{channel.lower()}_{row}_{column}"), End(node=f"matrix_{row}_{column}")),
                            )
                        ),
                        *(math(f"phosphor_{channel.lower()}_{row}", "ADD", End(node=f"phosphor_{channel.lower()}_{row}_0"), End(node=f"phosphor_{channel.lower()}_{row}_1")) for row in range(2)),
                        math(f"phosphor_{channel.lower()}_integral", "ADD", End(node=f"phosphor_{channel.lower()}_0"), End(node=f"phosphor_{channel.lower()}_1")),
                        math(f"phosphor_{channel.lower()}_coverage", "DIVIDE", End(node=f"phosphor_{channel.lower()}_integral"), End(node="phosphor_area")),
                        math(f"phosphor_{channel.lower()}_normalized", "DIVIDE", End(node=f"phosphor_{channel.lower()}_coverage"), End(node="matrix_fill")),
                        math(f"phosphor_{channel.lower()}_unit", "MULTIPLY", End(node=f"phosphor_{channel.lower()}_normalized"), 3.0),
                        math(f"phosphor_{channel.lower()}_gain", "MULTIPLY_ADD", End(node=f"phosphor_{channel.lower()}_unit"), socket(*phosphors, strength_phosphors), End(node="phosphor_plain")),
                    )
                ),
                node("phosphor_gains", bpy.types.CompositorNodeCombineColor, {channel: End(node=f"phosphor_{channel.lower()}_gain") for channel in ("Red", "Green", "Blue")}, {"mode": "RGB"}),
                vector("masked", "MULTIPLY", End(node=scan_gate, socket="Output"), End(node="phosphor_gains", socket="Image")),
                switched(phosphor_gate, "RGBA", socket(*display, phosphors_field), End(node=scan_gate, socket="Output"), End(node="masked", socket="Vector")),
                math("curvature_denominator", "ADD", socket(*display, curvature), 1.0),
                math("curvature_scale", "DIVIDE", 1.0, End(node="curvature_denominator")),
                nested(
                    "glass", lens, {"point": frame.point, "extent": frame.extent, "share": shell.scale, "distortion": socket(*display, curvature), "scale": End(node="curvature_scale")}, "rectilinear"
                ),
                extended("glass_sample", End(node=phosphor_gate, socket="Output"), End(node="glass", socket="rectilinear"), Extension.CLIP.label, MenuItems.of(Extension)),
                switched(gates[display_field], "RGBA", socket(display_field), image, End(node="glass_sample", socket="Image")),
            ).map(ended(gates[display_field], "Output"))

        return Fragment.joined(
            node("fraction", bpy.types.ShaderNodeSeparateXYZ, {"Vector": frame.fraction}, {}),
            node("extent", bpy.types.ShaderNodeSeparateXYZ, {"Vector": frame.extent}, {}),
            math("aspect", "DIVIDE", End(node="extent", socket="X"), End(node="extent", socket="Y")),
            math("scan", "MULTIPLY", y, lines),
            math("line", "FLOOR", End(node="scan")),
            math("raster_width", "MULTIPLY", lines, aspect),
            math("raster_samples", "ROUND", End(node="raster_width")),
            math("sample_at", "MULTIPLY", x, End(node="raster_samples")),
            math("sample", "FLOOR", End(node="sample_at")),
            node("line_cell", bpy.types.ShaderNodeCombineXYZ, {"X": End(node="line")}, {}),
            node("sample_cell", bpy.types.ShaderNodeCombineXYZ, {"X": End(node="sample"), "Y": End(node="line")}, {}),
        ).bind(lambda base: corrupted(source).bind(played).bind(received).bind(displayed).bind(lambda result: Fragment.merged((base, result[0])).map(lambda joined: (joined, result[1]))))

    return paired(paired(framed("frame", source, shell.aspect, shell.scale), paired(clock.value, clock.pattern)), radial()).bind(lambda inputs: built(inputs[0][0], *inputs[0][1], inputs[1]))


# --- [COMPOSITION] ----------------------------------------------------------------------

emulation: CompositorEffect[Emulation] = CompositorEffect(
    record=Emulation, graph=emulated, straight=True, coverage=Coverage.INPUT, requested=Some(precise), native_admitted=Some(admitted), preparation=Some(prepared)
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Noise", "admitted", "emulated", "emulation", "prepared", "raster_noise"]
