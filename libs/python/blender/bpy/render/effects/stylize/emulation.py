"""Coded, transmitted, taped, received, and displayed video graphs."""

from collections.abc import Mapping
from enum import IntEnum
from fractions import Fraction
from functools import partial
from itertools import product
from math import ceil, floor, tau
from typing import Final

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Component, Expr, math, mixed, rgba, Scalar, switched, vector, xyz
from blender.bpy.nodes.interface import MenuItems
from blender.bpy.render.effects.coordinates import clocked, extended, origin, pixels, sampled
from blender.bpy.render.effects.generators.noise import gaussian
from blender.bpy.render.effects.optics.distortion import radial
from blender.bpy.render.effects.rows import Alpha, CameraFrame, CompositorEffect, precise, Shell
from blender.bpy.render.outputs.framing import capacity
from blender.imaging.color.processors import unassociated
from blender.imaging.effects.stylize.emulation import Corruption, Display, Emulation, Ghost, Layout, Phosphors, Scanlines, Signal, Tape
from blender.imaging.faults import accumulated, ImagingBufferFault, ImagingNumericFault, ImagingResult, paired
from blender.imaging.geometry.stmaps import Extension
from blender.imaging.parameters import Gated, Motion
from blender.imaging.sampling import Estimate, Precision

# --- [TYPES] ----------------------------------------------------------------------------


class Noise(IntEnum):
    """Persistent decorrelation keys for video random fields."""

    CELLS = 0
    BOUNCE = 1
    JITTER = 2
    MISTRACKING = 3
    DROPOUTS = 4
    LUMA = 5
    CHROMA = 6


# --- [CONSTANTS] ------------------------------------------------------------------------

RGB: Final[tuple[Component, Component, Component]] = ("Red", "Green", "Blue")
YCC: Final = (("mode", "YCC"), ("ycc_mode", "ITUBT601"))
BORDERS: Final = MenuItems.of(Extension)
CENTER: Final = 128.0 / 255.0

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PREPARATION]


def prepared(
    _identity: Identity,
    scene: bpy.types.Scene,
    record: Emulation,
    _config: PyOpenColorIO.Config,
    source: OpenImageIO.ImageBuf,
    _precision: Precision,
    frame: CameraFrame,
    _entry: bpy.types.SceneCompositorEffect,
) -> BlenderResult[tuple[Emulation, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]]:
    """Accepts the Tape canvases of the captured raster and codes the captured display image before remaining sections.

    Args:
        _identity: Extension owning stage job.
        scene: Evaluated frame of stage job, its compositor device bounding canvases.
        record: Evaluated compression, tape, and later section parameters.
        _config: Validated color configuration, capture holds row encoding.
        source: Associated display RGBA with captured data and display windows.
        _precision: Sampling policy, unused by JPEG cycle.
        frame: Evaluated original camera share of the captured raster.
        _entry: Evaluated stage entry.

    Returns:
        Unchanged record with the coded image, or the captured image without compression, or canvas, imaging, and codec faults.
    """
    compression = record.compression.value

    def fitted(height: int) -> BlenderResult[None]:
        share = Fraction(frame.scale)
        margin = (1 - share) / 2
        lines = Fraction(record.lines)
        first, last = (floor((edge - margin) / share * lines) for edge in (Fraction(1, 2 * height), 1 - Fraction(1, 2 * height)))
        radius = ceil(lines * Fraction(record.tape.value.jitter_period))
        return accumulated(capacity(scene, size) for size in ((1, 4 * radius + 1), (1, last - first + 1 + 2 * radius))).map(lambda _: None)

    def coded(image: OpenImageIO.ImageBuf) -> Option[OpenImageIO.ImageSpec]:
        if not (width := compression.coded_width).enabled:
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

    def cycled(rgb: OpenImageIO.ImageBuf) -> ImagingResult[OpenImageIO.ImageBuf]:
        try:
            return compression.applied(rgb, coded(rgb))
        except (TypeError, OverflowError, MemoryError) as error:
            return Error((ImagingNumericFault(request=compression, error=error),))

    def associated(alpha: npt.NDArray[np.float32], rgb: npt.NDArray[np.float32]) -> ImagingResult[OpenImageIO.ImageBuf]:
        return ImagingBufferFault.located(np.concatenate((np.where(alpha != 0.0, rgb * alpha, rgb), alpha), axis=-1), source)

    def compressed(straight: npt.NDArray[np.float32]) -> ImagingResult[OpenImageIO.ImageBuf]:
        return (
            ImagingBufferFault
            .located(straight, source)
            .bind(lambda image: ImagingBufferFault.checked(OpenImageIO.ImageBufAlgo.channels(image, (0, 1, 2))))
            .bind(cycled)
            .bind(ImagingBufferFault.pixels)
            .bind(lambda rgb: associated(straight[..., 3:], rgb))
        )

    return paired(
        fitted(source.spec().height) if record.tape.enabled else Ok(None), ImagingBufferFault.pixels(source).map(unassociated).bind(compressed) if record.compression.enabled else Ok(source)
    ).map(lambda parts: (record, {Shell.image: parts[1]}))


# --- [GRAPH]


def raster_noise(lattice: Scalar, offset: Scalar, pattern: Scalar) -> tuple[Expr, Expr, Expr, Expr]:
    """Builds four Gaussian samples at one raster lattice and pattern, the enclosing row requesting full compositor precision.

    Args:
        lattice: Raster sample or line coordinates in XY.
        offset: Seed and noise key offset from owning origin.
        pattern: Integer pattern index as fourth noise coordinate.

    Returns:
        Four normals in cosine and sine order of two Box-Muller pairs.
    """
    axes = vector("ADD", lattice, offset)
    first, second, third, fourth = (_white(xyz(axes["X"], axes["Y"], stream), pattern) for stream in range(4))
    return (*gaussian(first, second), *gaussian(third, fourth))


def emulated(shell: Shell[Emulation]) -> BlenderResult[Expr]:
    """Builds corruption, tape, reception, and display after codec section.

    Args:
        shell: Codec-processed image, camera frame, and parameter group inputs.

    Returns:
        Displayed color.
    """
    _, value = Gated.__struct_fields__
    _, corruption, tape, signal, display, lines_field, motion, seed_field = Emulation.__struct_fields__
    speed, hold = Motion.__struct_fields__
    _, share, _ = CameraFrame.__struct_fields__
    source, frame, gate = shell.color, shell.space, shell.gate
    scale, seed, lines = shell.input(Shell.frame, share), shell.input(seed_field), shell.input(lines_field)
    time = clocked(shell.input(motion, speed), shell.input(motion, hold))
    pattern = math("FLOOR", time)
    x, y = frame.fraction["X"], frame.fraction["Y"]
    aspect = frame.extent["X"] / frame.extent["Y"]
    line = math("FLOOR", y * lines)
    line_cell, sample_cell = xyz(line), xyz(math("FLOOR", x * math("ROUND", lines * aspect)), line)
    dimensions = Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", source),), socket="Dimensions", shape="VECTOR")
    half_x, half_y = (0.5 / (dimensions[axis] * scale) for axis in ("X", "Y"))

    def sample(image: Scalar, position: Expr, across: Extension = Extension.EXTEND, up: Extension = Extension.EXTEND) -> Expr:
        return extended(image, sampled(frame, position), ((across.label, BORDERS), (up.label, BORDERS)), clipped=True)

    def shifted(image: Scalar, horizontal: Scalar, vertical: Scalar = y, across: Extension = Extension.EXTEND, up: Extension = Extension.EXTEND) -> Expr:
        return sample(image, xyz(horizontal, vertical), across, up)

    def banded(image: Scalar, horizontal: Scalar, vertical: Scalar) -> Expr:
        return _blurred(image, pixels(frame, xyz(horizontal, vertical) * frame.extent, "VECTOR"))

    def fetched(image: Scalar, uv: Scalar | tuple[float, ...]) -> Expr:
        clip = Extension.CLIP.label
        return Expr(kind=bpy.types.CompositorNodeMapUV, inputs=(("Image", image), ("UV", uv), ("Interpolation", "Nearest"), ("Extension X", clip), ("Extension Y", clip)), socket="Image", shape="RGBA")

    def drawn(noise: Noise, lattice: Scalar | tuple[float, ...]) -> Expr:
        return _white(vector("ADD", lattice, origin(seed, noise)), pattern)

    def corrupted(image: Expr) -> Expr:
        coverage, cell_width, cell_height, displacement, split, chroma = Corruption.__struct_fields__
        held = partial(shell.input, corruption, value)
        cells = drawn(Noise.CELLS, vector("FLOOR", vector("DIVIDE", frame.point, xyz(held(cell_width), held(cell_height), 1.0))))
        tone = msgspec.structs.replace(cells, socket="Color", shape="RGBA")
        struck = cells < held(coverage)
        cell_x = x + math("MULTIPLY_ADD", tone["Red"], 2.0, -1.0) * held(displacement) * struck
        red, green, blue = (
            shifted(image, math("MULTIPLY_ADD", held(split), direction, cell_x), across=Extension.REPEAT, up=Extension.CLIP)[channel] for channel, direction in zip(RGB, (-1.0, 0.0, 1.0))
        )
        luma, blue_difference, red_difference = (_split(rgba(red, green, blue), channel) for channel in RGB)
        power = 2.0 ** math("MULTIPLY_ADD", tone["Green"], 4.0, -2.0)

        def powered(difference: Expr) -> Expr:
            centered = difference - CENTER
            return mixed(held(chroma) * struck, difference, math("MULTIPLY_ADD", math("SIGN", centered) * (abs(centered) * 2.0) ** power, 0.5, CENTER))

        return switched(gate(corruption), image, _joined(luma, powered(blue_difference), powered(red_difference)))

    def played(image: Expr) -> Expr:
        jitter, jitter_period, bounce, head_switching, skew, tracking_shift, tracking_height, tracking_rate, mistracking, dropouts, dropout_length = Tape.__struct_fields__
        held = partial(shell.input, tape, value)
        margin = math("MULTIPLY_ADD", scale, -0.5, 0.5)
        half = 0.5 / dimensions["Y"]
        first, last = (math("FLOOR", (edge - margin) / scale * lines) for edge in (half, 1.0 - half))
        radius = held(jitter_period) * lines
        support = math("CEIL", radius)
        padding = support * 2.0
        height = last - first + 1.0 + padding
        jitter_canvas, impulse_canvas = (
            Expr(kind=bpy.types.CompositorNodeBlankImage, inputs=(("Size", xyz(1.0, extent)),), socket="Image", shape="RGBA") for extent in (height, math("MULTIPLY_ADD", support, 4.0, 1.0))
        )
        jitter_row, impulse_row = (Expr(kind=bpy.types.CompositorNodeImageCoordinates, inputs=(("Image", canvas),), socket="Pixel", shape="VECTOR")["Y"] for canvas in (jitter_canvas, impulse_canvas))
        filtered = xyz(0.0, radius)
        jittered = _blurred(_normals(seed, Noise.JITTER, xyz(jitter_row + (first - support)), pattern)[0], filtered)
        impulse = fetched(_blurred(_blurred(math("COMPARE", impulse_row, padding, 0.0), filtered), filtered), (0.5, 0.5, 1.0))
        normalized = jittered["Red"] / math("SQRT", impulse["Red"])
        jitter_shift = math("MULTIPLY", fetched(normalized, xyz(0.5, (line - first + support + 0.5) / height, 1.0)), held(jitter))
        bounce_shift = math("MULTIPLY_ADD", drawn(Noise.BOUNCE, (0.0, 0.0, 0.0)), 2.0, -1.0) * held(bounce)
        head = y / held(head_switching)
        head_shift = math("MINIMUM", head / 0.9, math("MULTIPLY_ADD", head, -10.0, 10.0)) * ((1.0 - (y < 0.0)) * (y < held(head_switching))) * held(skew)
        tracking = math("MULTIPLY_ADD", abs(math("WRAP", y - math("FRACT", time * held(tracking_rate)), 0.5, -0.5)) / held(tracking_height), -2.0, 1.0, clamp=True)
        picture = shifted(image, x + jitter_shift + head_shift + tracking * held(tracking_shift), y + bounce_shift, up=Extension.CLIP)
        luma, blue_difference, red_difference = (_split(picture, channel) for channel in RGB)
        mistracked = _joined(mixed(tracking * held(mistracking), luma, drawn(Noise.MISTRACKING, line_cell)), blue_difference, red_difference)
        position = x / held(dropout_length)
        struck = drawn(Noise.DROPOUTS, xyz(math("FLOOR", position), line)) < held(dropouts)
        return switched(gate(tape), image, mixed((1.0 - math("FRACT", position)) * struck, mistracked, (1.0, 1.0, 1.0, 1.0)))

    def received(image: Expr) -> Expr:
        luma_blur, chroma_blur, chroma_vertical, chroma_delay, saturation, ringing, cross_color, noise_field, chroma_noise, hum, ghost_field = Signal.__struct_fields__
        amplitude, delay = Ghost.__struct_fields__
        held, ghost = partial(shell.input, signal, value), partial(shell.input, signal, value, ghost_field, value)
        luma, blue_difference, red_difference = (_split(image, channel) for channel in RGB)
        luma_noise, chroma_noises = _normals(seed, Noise.LUMA, sample_cell, pattern), _normals(seed, Noise.CHROMA, sample_cell, pattern)
        strength = math("SUBTRACT", luma, banded(luma, 1.0 / 377.0, 0.0)) * held(cross_color)
        phase = math("MULTIPLY_ADD", x, 188.3, (line + pattern) * 0.5) * tau
        green, blue = (
            math("MULTIPLY_ADD", noise, held(chroma_noise), math("MULTIPLY_ADD", strength, wave, difference))
            for difference, wave, noise in ((blue_difference, math("COSINE", phase), chroma_noises[0]), (red_difference, math("SINE", phase), chroma_noises[1]))
        )
        band = banded(math("MULTIPLY_ADD", luma_noise[0], held(noise_field), luma), held(luma_blur), 0.0)
        chroma = shifted(banded(rgba(green, blue, 0.0), held(chroma_blur), held(chroma_vertical)), x - held(chroma_delay))
        ringed = (shifted(band, math("MULTIPLY_ADD", held(luma_blur) / 3.0, direction, x)) for direction in (-1.0, 1.0))
        overshoot = -math("MULTIPLY_ADD", band, -2.0, math("ADD", *ringed))
        packed = rgba(math("MULTIPLY_ADD", overshoot, held(ringing), band), *(math("MULTIPLY_ADD", chroma[channel] - CENTER, held(saturation), CENTER) for channel in RGB[:2]))
        echoed = switched(gate(signal, value, ghost_field), packed, mixed(ghost(amplitude) / (ghost(amplitude) + 1.0), packed, shifted(packed, x - ghost(delay))))
        loss = math("MULTIPLY_ADD", math("SINE", math("MULTIPLY_ADD", time, -0.002, y) * tau), 0.5, 0.5) * held(hum)
        return switched(gate(signal), image, _joined(echoed["Red"] * (1.0 - loss), echoed["Green"], echoed["Blue"]))

    def displayed(image: Expr) -> Expr:
        glow, scanlines, phosphors_field, curvature, roll, collapse = Display.__struct_fields__
        strength, interlaced = Scanlines.__struct_fields__
        layout, triads, phosphor_strength = Phosphors.__struct_fields__
        held, scan, phosphors = partial(shell.input, display, value), partial(shell.input, display, value, scanlines, value), partial(shell.input, display, value, phosphors_field, value)
        rolled = shifted(image, x, math("MULTIPLY_ADD", held(roll), time, y), up=Extension.REPEAT)
        least = 1.0 / lines
        lowered = math("MULTIPLY_ADD", held(collapse), -2.0, 1.0)
        height, width = math("MAXIMUM", lowered, least), math("MINIMUM", math("MAXIMUM", lowered + 1.0, least), 1.0)
        at = vector("ADD", vector("DIVIDE", vector("SUBTRACT", frame.fraction, (0.5, 0.5, 0.0)), xyz(width, height, 1.0)), (0.5, 0.5, 0.0))
        collapsed = vector("DIVIDE", sample(rolled, at, Extension.CLIP, Extension.CLIP), width * height)
        radius = pixels(frame, xyz(held(glow, value), held(glow, value)), "VECTOR")
        near = _blurred(collapsed, radius)
        glowed = switched(gate(display, value, glow), collapsed, mixed(0.890909, _blurred(near, radius * (4.0, 1.0, 0.0)), near))
        field = math("PINGPONG", pattern, 1.0)

        def scanned_row(sign: float) -> Expr:
            raster = ((math("MULTIPLY_ADD", half_y, sign, y) - 0.5) / height + 0.5) * lines
            return switched(scan(interlaced), raster, (raster + field) * 0.5)

        low, high = scanned_row(-1.0), scanned_row(1.0)
        light = 1.0 - ((math("SINE", high * tau) - math("SINE", low * tau)) / ((high - low) * (2.0 * tau)) + 0.5) * scan(strength)
        scanned = switched(gate(display, value, scanlines), glowed, vector("SCALE", glowed, light / math("MULTIPLY_ADD", scan(strength), -0.5, 1.0)))
        geometry = shell.menu(display, value, phosphors_field, value, layout, cases={member.label: member.geometry for member in Layout}, kind="VECTOR")
        fill = 1.0 - geometry["X"]
        rows_per_frame = phosphors(triads) / aspect
        reach = {"x": (x * phosphors(triads), half_x * phosphors(triads), geometry["Z"]), "y": (y * rows_per_frame, half_y * rows_per_frame, geometry["Y"])}
        ends = {
            (axis, parity, sign): math("MULTIPLY_ADD", half, sign, math("MULTIPLY_ADD", stagger, parity, center))
            for axis, (center, half, stagger) in reach.items()
            for parity in range(2)
            for sign in (-1.0, 1.0)
        }

        def covered(axis: str, parity: int, gain: Scalar, start: Scalar, span: Scalar) -> Expr:
            return _primitive(ends[axis, parity, 1.0], gain, start, span) - _primitive(ends[axis, parity, -1.0], gain, start, span)

        matrix = {(row, column): covered("y", column, fill, float(row), fill) for row, column in product(range(2), repeat=2)}
        area = reach["y"][1] * 2.0 * (reach["x"][1] * 2.0)

        def gained(index: int) -> Expr:
            lit = {(row, column): covered("x", row, 1.0 / 3.0, column + index / 3.0, 1.0 / 3.0) * cell for (row, column), cell in matrix.items()}
            integral = lit[0, 0] + lit[0, 1] + (lit[1, 0] + lit[1, 1])
            return math("MULTIPLY_ADD", integral / area / fill * 3.0, phosphors(phosphor_strength), 1.0 - phosphors(phosphor_strength))

        masked = switched(gate(display, value, phosphors_field), scanned, vector("MULTIPLY", scanned, rgba(gained(0), gained(1), gained(2))))
        lens = radial(frame.point, frame.extent, scale, held(curvature), 1.0 / (held(curvature) + 1.0))
        return switched(gate(display), image, extended(masked, lens, ((Extension.CLIP.label, BORDERS), (Extension.CLIP.label, BORDERS)), clipped=True))

    return Ok(displayed(received(played(corrupted(source)))))


def _white(coordinate: Scalar, pattern: Scalar) -> Expr:
    """Builds 4D White Noise over a coordinate and pattern, its `Value` read."""
    return Expr(kind=bpy.types.ShaderNodeTexWhiteNoise, inputs=(("Vector", coordinate), ("W", pattern)), members=(("noise_dimensions", "4D"),))


def _blurred(image: Scalar, size: Scalar) -> Expr:
    """Builds Gaussian blur of an image over a per-axis pixel size."""
    return Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", image), ("Size", size), ("Type", "Gaussian")), socket="Image", shape="RGBA")


def _normals(seed: Scalar, noise: Noise, lattice: Scalar, pattern: Scalar) -> tuple[Expr, Expr, Expr, Expr]:
    """Builds four Gaussian samples of one noise key at a raster lattice."""
    return raster_noise(lattice, origin(seed, noise), pattern)


def _split(image: Scalar, channel: Component) -> Expr:
    """Reads one BT.601 YCbCr channel of an image."""
    return Expr(kind=bpy.types.CompositorNodeSeparateColor, inputs=(("Image", image),), members=YCC, socket=channel)


def _joined(luma: Scalar, blue: Scalar, red: Scalar) -> Expr:
    """Combines BT.601 luma and color differences into an image."""
    return Expr(kind=bpy.types.CompositorNodeCombineColor, inputs=(("Red", luma), ("Green", blue), ("Blue", red)), members=YCC, socket="Image", shape="RGBA")


def _primitive(at: Expr, gain: Scalar, start: Scalar, width: Scalar) -> Expr:
    """Integrates a two-cell periodic box of `width` starting at `start` from zero to `at`, `gain` per whole period."""
    whole = math("FLOOR", at / 2.0)
    return math("MULTIPLY_ADD", whole, gain, math("MINIMUM", math("MAXIMUM", math("MULTIPLY_ADD", whole, -2.0, at) - start, 0.0), width))


# --- [COMPOSITION] ----------------------------------------------------------------------

emulation = CompositorEffect(record=Emulation, graph=emulated, alpha=Alpha.STRAIGHT, requested=Some(precise), preparation=Some(prepared))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Noise", "emulated", "emulation", "prepared", "raster_noise"]
