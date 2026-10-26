"""Video path emulation: JPEG coding, digital corruption, tape playback, analog reception in BT.601 luma and color difference, and a picture tube showing the signal."""

from fractions import Fraction
from functools import partial
from itertools import product
from math import ceil, floor
from typing import Annotated

import bpy
from colour.models import WEIGHTS_YCBCR
from expression import Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Component, Expr, math, mixed, NODES, rgba, Scalar, switched, vector, where, xyz
from blender.bpy.nodes.textures import gaussian, stream
from blender.bpy.render.compositor.colors import from_ycc, ycc
from blender.bpy.render.compositor.coordinates import Border, Clock, clocked, extended, origin, pixels, sampled
from blender.bpy.render.compositor.effects import Alpha, CameraFrame, CompositorEffect, precise, Prepared, Prepares, Requests, Shell
from blender.bpy.render.outputs.framing import capacity
from blender.imaging.color.processors import Domain
from blender.imaging.faults import ImagingFaults, ImagingImageFault, ImagingResult
from blender.imaging.files import images
from blender.imaging.geometry.regions import FrameExtent, FrameLength
from blender.imaging.sampling.integration import Precision
from blender.records.faults import accumulated, paired
from blender.records.fields import Choice, Finite, Gated, NonNegative, Positive, Presentation, Quantity, Seed, Unit

from ...chain.stages import Effect, Stage

# --- [TYPES] ----------------------------------------------------------------------------


class Subsampling(Choice):
    """Chroma resolution relative to coded luma."""

    factors: tuple[int, int]

    def __init__(self, value: int, factors: tuple[int, int], label: str | None = None) -> None:
        """Sets chroma block width and height in luma blocks."""
        super().__init__(value, label)
        self.factors = factors

    YUV444 = 1, (1, 1), "4:4:4"
    YUV422 = 2, (2, 1), "4:2:2"
    YUV420 = 3, (2, 2), "4:2:0"
    YUV411 = 4, (4, 1), "4:1:1"


class Layout(Choice):
    """Arrangement of red, green, and blue phosphors."""

    geometry: tuple[float, float, float]

    def __init__(self, value: int, geometry: tuple[float, float, float]) -> None:
        """Sets black-matrix gap, column stagger, and row stagger in triad cells."""
        super().__init__(value)
        self.geometry = geometry

    APERTURE_GRILLE = 1, (0.0, 0.0, 0.0)
    SLOT_MASK = 2, (1.0 / 3.0, 0.5, 0.0)
    SHADOW_MASK = 3, (1.0 / 3.0, 0.0, 0.5)


# --- [MODELS] ---------------------------------------------------------------------------


class Compression(msgspec.Struct, frozen=True, kw_only=True):
    """Repeated JPEG generations at a coded picture width."""

    quality: Annotated[int, msgspec.Meta(ge=1, le=100, description="JPEG quality on IJG scale")] = 60
    coded_width: Annotated[Gated[Annotated[int, msgspec.Meta(ge=1), Presentation(soft=Some((160.0, 1920.0)))]], msgspec.Meta(description="Picture width before coding, input raster size when off")] = (
        Gated(enabled=False, value=640)
    )
    subsampling: Annotated[Subsampling, msgspec.Meta(description="Chroma resolution relative to luma")] = Subsampling.YUV420
    generations: Annotated[int, msgspec.Meta(ge=1, description="JPEG encode and decode cycles"), Presentation(soft=Some((1.0, 8.0)))] = 1

    def coded(self, original: OpenImageIO.ImageSpec) -> Option[OpenImageIO.ImageSpec]:
        """Returns the data and display windows of a raster rescaled to the coded width, Nothing at input raster size."""
        if not self.coded_width.enabled:
            return Nothing
        width = self.coded_width.value
        height = max(1, round(Fraction(original.full_height * width, original.full_width)))
        horizontal, vertical = Fraction(width, original.full_width), Fraction(height, original.full_height)
        left, bottom = floor((original.x - original.full_x) * horizontal), floor((original.y - original.full_y) * vertical)
        right, top = ceil((original.x + original.width - original.full_x) * horizontal), ceil((original.y + original.height - original.full_y) * vertical)
        spec = OpenImageIO.ImageSpec(original)
        spec.x, spec.y, spec.width, spec.height = left, bottom, right - left, top - bottom
        spec.full_x, spec.full_y, spec.full_width, spec.full_height = 0, 0, width, height
        return Some(spec)

    def applied(self, source: OpenImageIO.ImageBuf) -> ImagingResult[OpenImageIO.ImageBuf]:
        """Codes straight display RGB through box reduction to the coded width, JPEG generations, and triangle reconstruction."""
        coded = self.coded(source.spec())

        def resized(image: OpenImageIO.ImageBuf, spec: OpenImageIO.ImageSpec, filtername: str) -> ImagingResult[OpenImageIO.ImageBuf]:
            target = OpenImageIO.ImageBuf(spec)
            OpenImageIO.ImageBufAlgo.resize(target, image, filtername=filtername, roi=target.roi)
            return images.checked(target)

        def cycled(image: OpenImageIO.ImageBuf) -> ImagingResult[OpenImageIO.ImageBuf]:
            rgb: ImagingResult[npt.NDArray[np.uint8]] = Option.of_optional(image.get_pixels(OpenImageIO.UINT8)).to_result_with(lambda: (ImagingImageFault(request=image, error=image.geterror()),))
            return (
                Result[npt.NDArray[np.uint8], ImagingFaults]
                .bind(rgb, lambda codes: images.generations(np.ascontiguousarray(codes), quality=self.quality, subsampling=self.subsampling.factors, count=self.generations))
                .bind(lambda values: images.located(values, image))
            )

        return (
            coded
            .map(lambda spec: resized(source, spec, "box"))
            .default_value(Ok(source))
            .bind(cycled)
            .bind(lambda image: coded.map(lambda _: resized(image, source.spec(), "triangle")).default_value(Ok(image)))
        )


class Corruption(msgspec.Struct, frozen=True, kw_only=True):
    """Rectangular buffer corruption with a frame-wide RGB split."""

    coverage: Annotated[Unit, msgspec.Meta(description="Share of corrupted cells per pattern")] = 0.2
    cell_width: Annotated[FrameExtent, msgspec.Meta(description="Corrupted cell width"), Presentation(soft=Some((0.005, 1.0)))] = 0.03
    cell_height: Annotated[FrameExtent, msgspec.Meta(description="Corrupted cell height"), Presentation(soft=Some((0.005, 1.0)))] = 0.03
    displacement: Annotated[NonNegative, msgspec.Meta(description="Greatest horizontal cell shift"), Presentation(soft=Some((0.0, 0.2)), quantity=Some(Quantity.FRAME))] = 0.004
    split: Annotated[NonNegative, msgspec.Meta(description="Opposing red and blue offsets across frame"), Presentation(soft=Some((0.0, 0.2)), quantity=Some(Quantity.FRAME))] = 0.0
    chroma: Annotated[Unit, msgspec.Meta(description="Sign-preserving chroma power in struck cells")] = 0.6


class Tape(msgspec.Struct, frozen=True, kw_only=True):
    """Time-base error, head switching, tracking disturbance, and dropouts."""

    jitter: Annotated[NonNegative, msgspec.Meta(description="Standard deviation of horizontal line displacement"), Presentation(soft=Some((0.0, 0.01)), quantity=Some(Quantity.FRAME))] = 0.002
    jitter_period: Annotated[NonNegative, msgspec.Meta(description="Vertical jitter smoothing period"), Presentation(quantity=Some(Quantity.FRAME))] = 1.0 / 65.0
    bounce: Annotated[NonNegative, msgspec.Meta(description="Greatest vertical frame shift"), Presentation(soft=Some((0.0, 0.02)), quantity=Some(Quantity.FRAME))] = 0.0
    head_switching: Annotated[NonNegative, msgspec.Meta(description="Skewed bottom band height"), Presentation(soft=Some((0.0, 0.05)), quantity=Some(Quantity.FRAME))] = 0.0176
    skew: Annotated[Finite, msgspec.Meta(description="Peak horizontal head-switching shift at nine tenths of band"), Presentation(soft=Some((-0.1, 0.1)), quantity=Some(Quantity.FRAME))] = 0.05
    tracking_shift: Annotated[Finite, msgspec.Meta(description="Shift at tracking band center"), Presentation(soft=Some((-0.1, 0.1)), quantity=Some(Quantity.FRAME))] = 0.0
    tracking_height: Annotated[Positive, msgspec.Meta(description="Tracking band height"), Presentation(soft=Some((0.0, 0.25)), quantity=Some(Quantity.FRAME))] = 0.15
    tracking_rate: Annotated[NonNegative, msgspec.Meta(description="Frame heights traveled upward per clock frame")] = 0.0
    mistracking: Annotated[Unit, msgspec.Meta(description="Luma noise in tracking band")] = 0.0
    dropouts: Annotated[Unit, msgspec.Meta(description="Share of dropout-length cells struck per raster line"), Presentation(soft=Some((0.0, 0.2)))] = 0.02
    dropout_length: Annotated[Positive, msgspec.Meta(description="Fading dropout cell length"), Presentation(soft=Some((0.0, 0.2)), quantity=Some(Quantity.FRAME))] = 0.04


class Ghost(msgspec.Struct, frozen=True, kw_only=True):
    """Delayed signal echo from a second transmission path."""

    amplitude: Annotated[Unit, msgspec.Meta(description="Delayed echo strength")] = 0.1
    delay: Annotated[Finite, msgspec.Meta(description="Horizontal echo offset, right above zero"), Presentation(soft=Some((-0.2, 0.2)), quantity=Some(Quantity.FRAME))] = 0.05


class Signal(msgspec.Struct, frozen=True, kw_only=True):
    """Analog video bandwidth, Gaussian noise, and carrier disturbances."""

    luma_blur: Annotated[NonNegative, msgspec.Meta(description="Horizontal luma bandwidth blur"), Presentation(soft=Some((0.0, 0.01)), quantity=Some(Quantity.FRAME))] = 0.0023
    chroma_blur: Annotated[NonNegative, msgspec.Meta(description="Horizontal color-difference bandwidth blur"), Presentation(soft=Some((0.0, 0.05)), quantity=Some(Quantity.FRAME))] = 0.0073
    chroma_vertical: Annotated[NonNegative, msgspec.Meta(description="Vertical color-difference blur"), Presentation(soft=Some((0.0, 0.02)), quantity=Some(Quantity.FRAME))] = 0.0
    chroma_delay: Annotated[Finite, msgspec.Meta(description="Color-difference offset from luma, right above zero"), Presentation(soft=Some((-0.02, 0.02)), quantity=Some(Quantity.FRAME))] = 0.0
    saturation: Annotated[NonNegative, msgspec.Meta(description="Color-difference gain"), Presentation(soft=Some((0.0, 2.0)))] = 1.0
    ringing: Annotated[Unit, msgspec.Meta(description="Second-difference overshoot at luma edges")] = 0.1
    cross_color: Annotated[Unit, msgspec.Meta(description="Fine luma detail decoded as NTSC subcarrier color")] = 0.0
    noise: Annotated[NonNegative, msgspec.Meta(description="Luma noise standard deviation per raster sample"), Presentation(soft=Some((0.0, 0.2)))] = 0.02
    chroma_noise: Annotated[NonNegative, msgspec.Meta(description="Color-difference noise standard deviation per raster sample"), Presentation(soft=Some((0.0, 0.2)))] = 0.03
    hum: Annotated[Unit, msgspec.Meta(description="Depth of mains hum bar drifting upward"), Presentation(soft=Some((0.0, 0.5)))] = 0.0
    ghost: Annotated[Gated[Ghost], msgspec.Meta(description="Multipath echo of whole signal")] = Gated(enabled=False, value=Ghost())


class Phosphors(msgspec.Struct, frozen=True, kw_only=True):
    """Mean-preserving phosphor mask integrated per pixel footprint."""

    layout: Annotated[Layout, msgspec.Meta(description="Phosphor arrangement")] = Layout.APERTURE_GRILLE
    triads: Annotated[int, msgspec.Meta(ge=1, description="Phosphor triads across frame width"), Presentation(soft=Some((80.0, 1280.0)))] = 640
    strength: Annotated[Unit, msgspec.Meta(description="Blend toward mean-one phosphor mask")] = 0.705394


class Scanlines(msgspec.Struct, frozen=True, kw_only=True):
    """Mean-preserving raster lines integrated per pixel footprint."""

    strength: Annotated[Unit, msgspec.Meta(description="Darkening between raster lines before mean normalization")] = 0.4
    interlaced: Annotated[bool, msgspec.Meta(description="Light alternate raster fields per pattern")] = False


class Display(msgspec.Struct, frozen=True, kw_only=True):
    """Picture tube showing a collapsed, glowing, scanned, and masked signal."""

    glow: Annotated[
        Gated[Annotated[FrameLength, msgspec.Meta(title="Radius", description="Gaussian glass blur"), Presentation(soft=Some((0.0, 0.05)))]],
        msgspec.Meta(description="Linear screen-glass scattering before scanlines and phosphors"),
    ] = Gated(enabled=False, value=0.01)
    scanlines: Annotated[Gated[Scanlines], msgspec.Meta(description="Raster lines the beam draws")] = Gated(enabled=True, value=Scanlines())
    phosphors: Annotated[Gated[Phosphors], msgspec.Meta(description="Phosphor mask on glass")] = Gated(enabled=False, value=Phosphors())
    curvature: Annotated[NonNegative, msgspec.Meta(description="Screen glass barrel bulge"), Presentation(soft=Some((0.0, 0.25)))] = 0.03
    roll: Annotated[Finite, msgspec.Meta(description="Frame heights rolled per clock frame without vertical hold"), Presentation(soft=Some((-0.05, 0.05)))] = 0.0
    collapse: Annotated[Unit, msgspec.Meta(description="Power-off collapse to line, then dot")] = 0.0


class Emulation(Effect, frozen=True, kw_only=True):
    """Coded, transmitted, taped, received, and displayed video path."""

    stage = Stage.EMULATION
    pointwise = False
    compression: Annotated[Gated[Compression], msgspec.Meta(description="JPEG picture block coding")] = Gated(enabled=False, value=Compression())
    corruption: Annotated[Gated[Corruption], msgspec.Meta(description="Digital picture corruption")] = Gated(enabled=False, value=Corruption())
    tape: Annotated[Gated[Tape], msgspec.Meta(description="Videotape playback errors")] = Gated(enabled=False, value=Tape())
    signal: Annotated[Gated[Signal], msgspec.Meta(description="Analog video bandwidth and noise")] = Gated(enabled=True, value=Signal())
    display: Annotated[Gated[Display], msgspec.Meta(description="Picture tube showing signal")] = Gated(enabled=True, value=Display())
    lines: Annotated[int, msgspec.Meta(ge=1, description="Raster lines across frame height"), Presentation(soft=Some((120.0, 1080.0)))] = 480
    clock: Annotated[Clock, msgspec.Meta(description="Clock of every pattern")] = Clock()
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting every random pattern")] = 0


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
) -> BlenderResult[Prepared[Emulation]]:
    """Accepts the tape canvases of the captured raster and codes the captured display image before the node sections.

    Args:
        _identity: Extension owning the stage job.
        scene: Evaluated frame whose compositor device bounds the canvases.
        record: Evaluated compression, tape, and later section parameters.
        _config: Validated color configuration.
        source: Associated display RGBA with captured data and display windows.
        _precision: Sampling policy, unused by the JPEG cycle.
        frame: Original camera share of the captured raster.
        _entry: Evaluated stage entry.

    Returns:
        Unchanged record with the coded image, or the captured image without compression, or canvas, image, and codec faults.
    """

    def fitted(height: int) -> BlenderResult[None]:
        share = Fraction(frame.scale)
        margin = (1 - share) / 2
        lines = Fraction(record.lines)
        first, last = (floor((edge - margin) / share * lines) for edge in (Fraction(1, 2 * height), 1 - Fraction(1, 2 * height)))
        radius = ceil(lines * Fraction(record.tape.value.jitter_period))
        return accumulated(capacity(scene, size) for size in ((1, 4 * radius + 1), (1, last - first + 1 + 2 * radius))).map(lambda _: None)

    def compressed(straight: np.ndarray) -> ImagingResult[OpenImageIO.ImageBuf]:
        alpha = straight[..., 3:]
        return (
            images
            .located(straight, source)
            .bind(lambda image: images.checked(OpenImageIO.ImageBufAlgo.channels(image, (0, 1, 2))))
            .bind(record.compression.value.applied)
            .bind(images.pixels)
            .bind(lambda rgb: images.located(np.concatenate((np.where(alpha != 0.0, rgb * alpha, rgb), alpha), axis=-1), source))
        )

    return paired(fitted(source.spec().height) if record.tape.enabled else Ok(None), images.pixels(source).map(images.unassociated).bind(compressed) if record.compression.enabled else Ok(source)).map(
        lambda parts: (record, {Shell.image: parts[1]})
    )


# --- [GRAPH]


def emulated(shell: Shell[Emulation]) -> BlenderResult[Expr]:
    """Builds corruption, tape, reception, and display after the codec section, color differences in BT.601 luma weights at unit swing about zero.

    Args:
        shell: Codec-processed image, camera frame, and parameter group inputs.

    Returns:
        Displayed color.
    """
    _, value = Gated.__struct_fields__
    _, corruption, tape, signal, display, lines_field, clock, seed_field = Emulation.__struct_fields__
    speed, hold = Clock.__struct_fields__
    _, share, _ = CameraFrame.__struct_fields__
    source, frame, gate = shell.color, shell.space, shell.gate
    scale, seed, lines = shell.input(Shell.frame, share), shell.input(seed_field), shell.input(lines_field)
    red_weight, blue_weight = (float(weight) for weight in WEIGHTS_YCBCR["ITU-R BT.601"])
    weights = (red_weight, 1.0 - red_weight - blue_weight, blue_weight)
    time = clocked(shell.input(clock, speed), shell.input(clock, hold))
    pattern = math("FLOOR", time)
    x, y = frame.fraction["X"], frame.fraction["Y"]
    aspect = frame.extent["X"] / frame.extent["Y"]
    line = math("FLOOR", y * lines)
    line_cell, sample_cell = xyz(line), xyz(math("FLOOR", x * math("ROUND", lines * aspect)), line)
    dimensions = Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", source),), socket="Dimensions", shape="VECTOR")
    half_x, half_y = (0.5 / (dimensions[axis] * scale) for axis in ("X", "Y"))

    def sample(image: Scalar, position: Expr, across: Border = Border.CLAMP, up: Border = Border.CLAMP) -> Expr:
        return extended(image, sampled(frame, position), (across.label, up.label), clipped=True)

    def shifted(image: Scalar, horizontal: Scalar, vertical: Scalar = y, across: Border = Border.CLAMP, up: Border = Border.CLAMP) -> Expr:
        return sample(image, xyz(horizontal, vertical), across, up)

    def banded(image: Scalar, horizontal: Scalar, vertical: Scalar) -> Expr:
        return _blurred(image, pixels(frame, xyz(horizontal, vertical) * frame.extent, "VECTOR"))

    def fetched(image: Scalar, uv: Scalar | tuple[float, ...]) -> Expr:
        return Expr(
            kind=bpy.types.CompositorNodeMapUV, inputs=(("Image", image), ("UV", uv), ("Interpolation", "Nearest"), ("Extension X", "Clip"), ("Extension Y", "Clip")), socket="Image", shape="RGBA"
        )

    def drawn(key: str, lattice: Scalar | tuple[float, ...]) -> Expr:
        return _white(vector("ADD", lattice, origin(seed, stream(Emulation, key))), pattern)

    def normals(key: str, lattice: Scalar) -> tuple[Expr, Expr, Expr, Expr]:
        axes = vector("ADD", lattice, origin(seed, stream(Emulation, key)))
        first, second, third, fourth = (_white(xyz(axes["X"], axes["Y"], index), pattern) for index in range(4))
        return (*gaussian(first, second, NODES), *gaussian(third, fourth, NODES))

    def split(image: Expr) -> tuple[Expr, Expr, Expr]:
        return ycc(image, weights)

    def corrupted(image: Expr) -> Expr:
        coverage, cell_width, cell_height, displacement, split_field, chroma = Corruption.__struct_fields__
        held = partial(shell.input, corruption, value)
        cells = drawn(coverage, vector("FLOOR", vector("DIVIDE", frame.point, xyz(held(cell_width), held(cell_height), 1.0))))
        tone = msgspec.structs.replace(cells, socket="Color", shape="RGBA")
        struck = cells < held(coverage)
        cell_x = x + math("MULTIPLY_ADD", tone["Red"], 2.0, -1.0) * held(displacement) * struck
        channels: tuple[Component, ...] = ("Red", "Green", "Blue")
        red, green, blue = (
            shifted(image, math("MULTIPLY_ADD", held(split_field), direction, cell_x), across=Border.PERIODIC, up=Border.BLACK)[channel] for channel, direction in zip(channels, (-1.0, 0.0, 1.0))
        )
        luma, blue_difference, red_difference = split(rgba(red, green, blue))
        power = 2.0 ** math("MULTIPLY_ADD", tone["Green"], 4.0, -2.0)

        def powered(difference: Expr) -> Expr:
            return mixed(held(chroma) * struck, difference, math("SIGN", difference) * (abs(difference) * 2.0) ** power * 0.5)

        return switched(gate(corruption), image, from_ycc(luma, powered(blue_difference), powered(red_difference), weights))

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
        jittered = _blurred(normals(jitter, xyz(jitter_row + (first - support)))[0], filtered)
        impulse = fetched(_blurred(_blurred(math("COMPARE", impulse_row, padding, 0.0), filtered), filtered), (0.5, 0.5, 1.0))
        normalized = jittered["Red"] / math("SQRT", impulse["Red"])
        jitter_shift = math("MULTIPLY", fetched(normalized, xyz(0.5, (line - first + support + 0.5) / height, 1.0)), held(jitter))
        bounce_shift = math("MULTIPLY_ADD", drawn(bounce, (0.0, 0.0, 0.0)), 2.0, -1.0) * held(bounce)
        head = y / held(head_switching)
        head_shift = math("MINIMUM", head / 0.9, math("MULTIPLY_ADD", head, -10.0, 10.0)) * ((1.0 - (y < 0.0)) * (y < held(head_switching))) * held(skew)
        tracking = math("MULTIPLY_ADD", abs(math("WRAP", y - math("FRACT", time * held(tracking_rate)), 0.5, -0.5)) / held(tracking_height), -2.0, 1.0, clamp=True)
        picture = shifted(image, x + jitter_shift + head_shift + tracking * held(tracking_shift), y + bounce_shift, up=Border.BLACK)
        luma, blue_difference, red_difference = split(picture)
        mistracked = from_ycc(mixed(tracking * held(mistracking), luma, drawn(mistracking, line_cell)), blue_difference, red_difference, weights)
        position = x / held(dropout_length)
        struck = drawn(dropouts, xyz(math("FLOOR", position), line)) < held(dropouts)
        return switched(gate(tape), image, mixed((1.0 - math("FRACT", position)) * struck, mistracked, (1.0, 1.0, 1.0)))

    def received(image: Expr) -> Expr:
        luma_blur, chroma_blur, chroma_vertical, chroma_delay, saturation, ringing, cross_color, noise_field, chroma_noise, hum, ghost_field = Signal.__struct_fields__
        amplitude, delay = Ghost.__struct_fields__
        held, ghost = partial(shell.input, signal, value), partial(shell.input, signal, value, ghost_field, value)
        luma, blue_difference, red_difference = split(image)
        luma_noise, chroma_noises = normals(noise_field, sample_cell), normals(chroma_noise, sample_cell)
        strength = math("SUBTRACT", luma, banded(luma, 1.0 / 377.0, 0.0)) * held(cross_color)
        phase = math("MULTIPLY_ADD", x, 188.3, (line + pattern) * 0.5) * float(2.0 * np.pi)
        green, blue = (
            math("MULTIPLY_ADD", noise, held(chroma_noise), math("MULTIPLY_ADD", strength, wave, difference))
            for difference, wave, noise in ((blue_difference, math("COSINE", phase), chroma_noises[0]), (red_difference, math("SINE", phase), chroma_noises[1]))
        )
        band = banded(math("MULTIPLY_ADD", luma_noise[0], held(noise_field), luma), held(luma_blur), 0.0)
        chroma = shifted(banded(rgba(green, blue, 0.0), held(chroma_blur), held(chroma_vertical)), x - held(chroma_delay))
        ringed = (shifted(band, math("MULTIPLY_ADD", held(luma_blur) / 3.0, direction, x)) for direction in (-1.0, 1.0))
        overshoot = -math("MULTIPLY_ADD", band, -2.0, math("ADD", *ringed))
        packed = rgba(math("MULTIPLY_ADD", overshoot, held(ringing), band), chroma["Red"] * held(saturation), chroma["Green"] * held(saturation))
        echoed = switched(gate(signal, value, ghost_field), packed, mixed(ghost(amplitude) / (ghost(amplitude) + 1.0), packed, shifted(packed, x - ghost(delay))))
        loss = math("MULTIPLY_ADD", math("SINE", math("MULTIPLY_ADD", time, -0.002, y) * float(2.0 * np.pi)), 0.5, 0.5) * held(hum)
        return switched(gate(signal), image, from_ycc(echoed["Red"] * (1.0 - loss), echoed["Green"], echoed["Blue"], weights))

    def displayed(image: Expr) -> Expr:
        glow, scanlines, phosphors_field, curvature, roll, collapse = Display.__struct_fields__
        strength, interlaced = Scanlines.__struct_fields__
        layout, triads, phosphor_strength = Phosphors.__struct_fields__
        held, scan, phosphors = partial(shell.input, display, value), partial(shell.input, display, value, scanlines, value), partial(shell.input, display, value, phosphors_field, value)
        rolled = shifted(image, x, math("MULTIPLY_ADD", held(roll), time, y), up=Border.PERIODIC)
        least = 1.0 / lines
        lowered = math("MULTIPLY_ADD", held(collapse), -2.0, 1.0)
        height, width = math("MAXIMUM", lowered, least), math("MINIMUM", math("MAXIMUM", lowered + 1.0, least), 1.0)
        at = vector("ADD", vector("DIVIDE", vector("SUBTRACT", frame.fraction, (0.5, 0.5, 0.0)), xyz(width, height, 1.0)), (0.5, 0.5, 0.0))
        collapsed = vector("DIVIDE", sample(rolled, at, Border.BLACK, Border.BLACK), width * height)
        radius = pixels(frame, xyz(held(glow, value), held(glow, value)), "VECTOR")
        near = _blurred(collapsed, radius)
        glowed = switched(gate(display, value, glow), collapsed, mixed(0.890909, _blurred(near, radius * (4.0, 1.0, 0.0)), near))
        field = math("PINGPONG", pattern, 1.0)

        def scanned_row(sign: float) -> Expr:
            raster = ((math("MULTIPLY_ADD", half_y, sign, y) - 0.5) / height + 0.5) * lines
            return switched(scan(interlaced), raster, (raster + field) * 0.5)

        low, high = scanned_row(-1.0), scanned_row(1.0)
        tau = float(2.0 * np.pi)
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
        bulge = held(curvature)
        offset = frame.point * (2.0, 2.0, 0.0) - frame.extent
        stretched = vector("SCALE", offset, math("SQRT", bulge) * 2.0 / (bulge + 1.0))
        real = 1.0 - (vector("LENGTH", stretched) > 1.0)
        lens = vector(
            "MULTIPLY_ADD",
            vector("SCALE", offset / frame.extent, scale),
            where(real, 1.0 / ((bulge + 1.0) * (vector("LENGTH", vector("ADD", stretched, (0.0, 0.0, 1.0))) + 1.0)), 0.0),
            xyz(0.5, 0.5, real),
        )
        return switched(gate(display), image, extended(masked, lens, (Border.BLACK.label, Border.BLACK.label), clipped=True))

    return Ok(displayed(received(played(corrupted(source)))))


def _white(coordinate: Scalar, pattern: Scalar) -> Expr:
    """Builds 4D White Noise over a coordinate and pattern, its `Value` read."""
    return Expr(kind=bpy.types.ShaderNodeTexWhiteNoise, inputs=(("Vector", coordinate), ("W", pattern)), members=(("noise_dimensions", "4D"),))


def _blurred(image: Scalar, size: Scalar) -> Expr:
    """Builds Gaussian blur of an image over a per-axis pixel size."""
    return Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", image), ("Size", size), ("Type", "Gaussian")), socket="Image", shape="RGBA")


def _primitive(at: Expr, gain: Scalar, start: Scalar, width: Scalar) -> Expr:
    """Integrates a two-cell periodic box of `width` starting at `start` from zero to `at`, `gain` per whole period."""
    whole = math("FLOOR", at / 2.0)
    return math("MULTIPLY_ADD", whole, gain, math("MINIMUM", math("MAXIMUM", math("MULTIPLY_ADD", whole, -2.0, at) - start, 0.0), width))


# --- [COMPOSITION] ----------------------------------------------------------------------

emulation = CompositorEffect(record=Emulation, graph=emulated, domain=Domain.DISPLAY, alpha=Alpha.STRAIGHT, parts=(Requests[Emulation](request=precise), Prepares(prepare=prepared)))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Compression", "Corruption", "Display", "Emulation", "Ghost", "Layout", "Phosphors", "Scanlines", "Signal", "Subsampling", "Tape", "emulation"]
