"""Video path emulation: JPEG coding, digital corruption, tape playback, analog reception in BT.601 luma and color difference, and a picture tube showing the signal."""

from collections.abc import Mapping
from fractions import Fraction
from functools import partial
from itertools import product
from math import ceil, floor
from typing import Final, get_args

import bpy
from colour.models import WEIGHTS_YCBCR
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes.functions import EXPRESSIONS
from blender.bpy.nodes.graphs import Component, Expr, math, mixed, rgba, Scalar, switched, vector, where, xyz
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, MenuItem, MenuItems, Socket
from blender.bpy.nodes.textures import White
from blender.bpy.render.compositor.colors import from_ycc, ycc
from blender.bpy.render.compositor.coordinates import Border, dimensions, extended, pixels, sampled
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, precise
from blender.bpy.render.compositor.inputs import CameraFrame, FRAME_SCALE
from blender.bpy.render.compositor.motion import clocked, origin
from blender.bpy.render.compositor.shells import Shell
from blender.bpy.render.outputs.framing import capacity
from blender.imaging.color.processors import Domain
from blender.imaging.files import buffers, images
from blender.imaging.geometry.arithmetic import gaussian
from blender.imaging.sampling.integration import Estimate, Precision
from blender.imaging.sampling.tiles import stream
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason

from ...chain.stages import Effect, prepared, Stage
from ..optics.procedural import CLOCK

# --- [CONSTANTS] ------------------------------------------------------------------------

_APERTURE_GRILLE: Final = MenuItem(name="Aperture Grille")
_SLOT_MASK: Final = MenuItem(name="Slot Mask")
_SHADOW_MASK: Final = MenuItem(name="Shadow Mask")
_CLOCK_SPEED, _CLOCK_HOLD = CLOCK.inputs
_LINES: Final = Socket(key="lines", kind=bpy.types.NodeSocketInt, description="Raster lines across frame height", min_value=1, default_value=480)
_SEED: Final = Socket(key="seed", kind=bpy.types.NodeSocketInt, description="Number selecting every random pattern", min_value=0, max_value=2**24)
_SUBSAMPLING: Final = Socket(
    key="compression_subsampling",
    name="Subsampling",
    kind=bpy.types.NodeSocketMenu,
    description="Chroma resolution relative to luma",
    default_value="4:2:0",
    items=Some(MenuItems(items=tuple(MenuItem(name=name) for name in get_args(images.Subsampling.__value__)))),
)
_QUALITY, _GENERATIONS = (
    Socket(key=item_key("compression", field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketInt, description=description, min_value=low, max_value=high, default_value=default)
    for field, description, low, high, default in (("quality", "JPEG quality on IJG scale", 1, 100, 60), ("generations", "JPEG encode and decode cycles", 1, None, 1))
)
_CODED_WIDTH_VALUE: Final = Socket(key="compression_coded_width_value", name="Coded Width", kind=bpy.types.NodeSocketInt, min_value=1, default_value=640)
_CODED_WIDTH: Final = InterfacePanel(
    key="compression_coded_width", name="Coded Width", description="Picture width before coding, input raster size when off", toggle=Some(False), inputs=(_CODED_WIDTH_VALUE,)
)
_COMPRESSION: Final = InterfacePanel(
    key="compression", name="Compression", description="JPEG picture block coding", toggle=Some(False), inputs=(_SUBSAMPLING, _QUALITY, _GENERATIONS), panels=(_CODED_WIDTH,)
)
_COVERAGE, _CELL_WIDTH, _CELL_HEIGHT, _DISPLACEMENT, _SPLIT, _CHROMA = (
    Socket(key=item_key("corruption", field), name=bpy.path.display_name(field), kind=kind, description=description, min_value=low, max_value=high, default_value=default)
    for field, kind, description, low, high, default in (
        ("coverage", bpy.types.NodeSocketFloatFactor, "Share of corrupted cells per pattern", 0.0, 1.0, 0.2),
        ("cell_width", bpy.types.NodeSocketFloat, "Corrupted cell width over greater frame side", 1e-6, None, 0.03),
        ("cell_height", bpy.types.NodeSocketFloat, "Corrupted cell height over greater frame side", 1e-6, None, 0.03),
        ("displacement", bpy.types.NodeSocketFloat, "Greatest horizontal cell shift as a frame share", 0.0, None, 0.004),
        ("split", bpy.types.NodeSocketFloat, "Opposing red and blue offsets across frame as a frame share", 0.0, None, None),
        ("chroma", bpy.types.NodeSocketFloatFactor, "Sign-preserving chroma power in struck cells", 0.0, 1.0, 0.6),
    )
)
_CORRUPTION: Final = InterfacePanel(
    key="corruption",
    name="Corruption",
    description="Rectangular buffer corruption with a frame-wide RGB split",
    toggle=Some(False),
    inputs=(_COVERAGE, _CELL_WIDTH, _CELL_HEIGHT, _DISPLACEMENT, _SPLIT, _CHROMA),
)
_JITTER, _JITTER_PERIOD, _BOUNCE, _HEAD_SWITCHING, _SKEW, _TRACKING_SHIFT, _TRACKING_HEIGHT, _TRACKING_RATE, _MISTRACKING, _DROPOUTS, _DROPOUT_LENGTH = (
    Socket(key=item_key("tape", field), name=bpy.path.display_name(field), kind=kind, description=description, min_value=low, max_value=high, default_value=default)
    for field, kind, description, low, high, default in (
        ("jitter", bpy.types.NodeSocketFloat, "Standard deviation of horizontal line displacement as a frame share", 0.0, None, 0.002),
        ("jitter_period", bpy.types.NodeSocketFloat, "Vertical jitter smoothing period as a frame share", 0.0, None, 1.0 / 65.0),
        ("bounce", bpy.types.NodeSocketFloat, "Greatest vertical frame shift as a frame share", 0.0, None, None),
        ("head_switching", bpy.types.NodeSocketFloat, "Skewed bottom band height as a frame share", 0.0, None, 0.0176),
        ("skew", bpy.types.NodeSocketFloat, "Peak horizontal head-switching shift at nine tenths of band", None, None, 0.05),
        ("tracking_shift", bpy.types.NodeSocketFloat, "Shift at tracking band center", None, None, None),
        ("tracking_height", bpy.types.NodeSocketFloat, "Tracking band height as a frame share", 1e-6, None, 0.15),
        ("tracking_rate", bpy.types.NodeSocketFloat, "Frame heights traveled upward per clock frame", 0.0, None, None),
        ("mistracking", bpy.types.NodeSocketFloatFactor, "Luma noise in tracking band", 0.0, 1.0, None),
        ("dropouts", bpy.types.NodeSocketFloatFactor, "Share of dropout-length cells struck per raster line", 0.0, 1.0, 0.02),
        ("dropout_length", bpy.types.NodeSocketFloat, "Fading dropout cell length as a frame share", 1e-6, None, 0.04),
    )
)
_TAPE: Final = InterfacePanel(
    key="tape",
    name="Tape",
    description="Time-base error, head switching, tracking disturbance, and dropouts",
    toggle=Some(False),
    inputs=(_JITTER, _JITTER_PERIOD, _BOUNCE, _HEAD_SWITCHING, _SKEW, _TRACKING_SHIFT, _TRACKING_HEIGHT, _TRACKING_RATE, _MISTRACKING, _DROPOUTS, _DROPOUT_LENGTH),
)
_LUMA_BLUR, _CHROMA_BLUR, _CHROMA_VERTICAL, _CHROMA_DELAY, _SATURATION, _RINGING, _CROSS_COLOR, _NOISE, _CHROMA_NOISE, _HUM = (
    Socket(key=item_key("signal", field), name=bpy.path.display_name(field), kind=kind, description=description, min_value=low, max_value=high, default_value=default)
    for field, kind, description, low, high, default in (
        ("luma_blur", bpy.types.NodeSocketFloat, "Horizontal luma bandwidth blur as a frame share", 0.0, None, 0.0023),
        ("chroma_blur", bpy.types.NodeSocketFloat, "Horizontal color-difference bandwidth blur as a frame share", 0.0, None, 0.0073),
        ("chroma_vertical", bpy.types.NodeSocketFloat, "Vertical color-difference blur as a frame share", 0.0, None, None),
        ("chroma_delay", bpy.types.NodeSocketFloat, "Color-difference offset from luma, right above zero", None, None, None),
        ("saturation", bpy.types.NodeSocketFloat, "Color-difference gain", 0.0, None, 1.0),
        ("ringing", bpy.types.NodeSocketFloatFactor, "Second-difference overshoot at luma edges", 0.0, 1.0, 0.1),
        ("cross_color", bpy.types.NodeSocketFloatFactor, "Fine luma detail decoded as NTSC subcarrier color", 0.0, 1.0, None),
        ("noise", bpy.types.NodeSocketFloat, "Luma noise standard deviation per raster sample", 0.0, None, 0.02),
        ("chroma_noise", bpy.types.NodeSocketFloat, "Color-difference noise standard deviation per raster sample", 0.0, None, 0.03),
        ("hum", bpy.types.NodeSocketFloatFactor, "Depth of mains hum bar drifting upward", 0.0, 1.0, None),
    )
)
_GHOST_AMPLITUDE: Final = Socket(
    key="signal_ghost_amplitude", name="Amplitude", kind=bpy.types.NodeSocketFloatFactor, description="Delayed echo strength", min_value=0.0, max_value=1.0, default_value=0.1
)
_GHOST_DELAY: Final = Socket(key="signal_ghost_delay", name="Delay", kind=bpy.types.NodeSocketFloat, description="Horizontal echo offset, right above zero", default_value=0.05)
_GHOST: Final = InterfacePanel(key="signal_ghost", name="Ghost", description="Multipath echo of whole signal", toggle=Some(False), inputs=(_GHOST_AMPLITUDE, _GHOST_DELAY))
_SIGNAL: Final = InterfacePanel(
    key="signal",
    name="Signal",
    description="Analog video bandwidth, Gaussian noise, and carrier disturbances",
    toggle=Some(True),
    inputs=(_LUMA_BLUR, _CHROMA_BLUR, _CHROMA_VERTICAL, _CHROMA_DELAY, _SATURATION, _RINGING, _CROSS_COLOR, _NOISE, _CHROMA_NOISE, _HUM),
    panels=(_GHOST,),
)
_CURVATURE, _ROLL, _COLLAPSE = (
    Socket(key=item_key("display", field), name=bpy.path.display_name(field), kind=kind, description=description, min_value=low, max_value=high, default_value=default)
    for field, kind, description, low, high, default in (
        ("curvature", bpy.types.NodeSocketFloat, "Screen glass barrel bulge", 0.0, None, 0.03),
        ("roll", bpy.types.NodeSocketFloat, "Frame heights rolled per clock frame without vertical hold", None, None, None),
        ("collapse", bpy.types.NodeSocketFloatFactor, "Power-off collapse to line, then dot", 0.0, 1.0, None),
    )
)
_GLOW_RADIUS: Final = Socket(key="display_glow_radius", name="Radius", kind=bpy.types.NodeSocketFloat, description="Gaussian glass blur", min_value=0.0, default_value=0.01)
_GLOW: Final = InterfacePanel(key="display_glow", name="Glow", description="Linear screen-glass scattering before scanlines and phosphors", toggle=Some(False), inputs=(_GLOW_RADIUS,))
_SCANLINES_STRENGTH: Final = Socket(
    key="display_scanlines_strength",
    name="Strength",
    kind=bpy.types.NodeSocketFloatFactor,
    description="Darkening between raster lines before mean normalization",
    min_value=0.0,
    max_value=1.0,
    default_value=0.4,
)
_INTERLACED: Final = Socket(key="display_scanlines_interlaced", name="Interlaced", kind=bpy.types.NodeSocketBool, description="Light alternate raster fields per pattern")
_SCANLINES: Final = InterfacePanel(key="display_scanlines", name="Scanlines", description="Raster lines the beam draws", toggle=Some(True), inputs=(_SCANLINES_STRENGTH, _INTERLACED))
_LAYOUT: Final = Socket(
    key="display_phosphors_layout",
    name="Layout",
    kind=bpy.types.NodeSocketMenu,
    description="Phosphor arrangement",
    default_value=_APERTURE_GRILLE.name,
    items=Some(MenuItems(items=(_APERTURE_GRILLE, _SLOT_MASK, _SHADOW_MASK))),
)
_TRIADS: Final = Socket(key="display_phosphors_triads", name="Triads", kind=bpy.types.NodeSocketInt, description="Phosphor triads across frame width", min_value=1, default_value=640)
_PHOSPHORS_STRENGTH: Final = Socket(
    key="display_phosphors_strength", name="Strength", kind=bpy.types.NodeSocketFloatFactor, description="Blend toward mean-one phosphor mask", min_value=0.0, max_value=1.0, default_value=0.705394
)
_PHOSPHORS: Final = InterfacePanel(key="display_phosphors", name="Phosphors", description="Phosphor mask on glass", toggle=Some(False), inputs=(_LAYOUT, _TRIADS, _PHOSPHORS_STRENGTH))
_DISPLAY: Final = InterfacePanel(
    key="display",
    name="Display",
    description="Picture tube showing a collapsed, glowing, scanned, and masked signal",
    toggle=Some(True),
    inputs=(_CURVATURE, _ROLL, _COLLAPSE),
    panels=(_GLOW, _SCANLINES, _PHOSPHORS),
)

# --- [MODELS] ---------------------------------------------------------------------------


class Compression(msgspec.Struct, frozen=True, kw_only=True):
    """Repeated JPEG generations at a coded picture width, input raster size without one."""

    quality: int
    coded_width: int | None
    subsampling: images.Subsampling
    generations: int

    def coded(self, original: OpenImageIO.ImageSpec) -> Option[OpenImageIO.ImageSpec]:
        """Returns the data and display windows of a raster rescaled to the coded width, Nothing at input raster size."""
        return Option.of_optional(self.coded_width).map(lambda width: self.windowed(original, width))

    @staticmethod
    def windowed(original: OpenImageIO.ImageSpec, width: int) -> OpenImageIO.ImageSpec:
        """Returns the raster's windows rescaled to a coded width."""
        height = max(1, round(Fraction(original.full_height * width, original.full_width)))
        horizontal, vertical = Fraction(width, original.full_width), Fraction(height, original.full_height)
        left, bottom = floor((original.x - original.full_x) * horizontal), floor((original.y - original.full_y) * vertical)
        right, top = ceil((original.x + original.width - original.full_x) * horizontal), ceil((original.y + original.height - original.full_y) * vertical)
        spec = OpenImageIO.ImageSpec(original)
        spec.x, spec.y, spec.width, spec.height = left, bottom, right - left, top - bottom
        spec.full_x, spec.full_y, spec.full_width, spec.full_height = 0, 0, width, height
        return spec

    def applied(self, source: OpenImageIO.ImageBuf) -> Outcome[OpenImageIO.ImageBuf]:
        """Codes straight display RGB through box reduction to the coded width, JPEG generations, and triangle reconstruction."""
        coded = self.coded(source.spec())

        def resized(image: OpenImageIO.ImageBuf, spec: OpenImageIO.ImageSpec, filtername: str) -> Outcome[OpenImageIO.ImageBuf]:
            target = OpenImageIO.ImageBuf(spec)
            OpenImageIO.ImageBufAlgo.resize(target, image, filtername=filtername, roi=target.roi)
            return buffers.checked(target)

        def cycled(image: OpenImageIO.ImageBuf) -> Outcome[OpenImageIO.ImageBuf]:
            rgb: Outcome[npt.NDArray[np.uint8]] = buffers.pixels(image, buffers.Depth.UINT8)
            return (
                Result[npt.NDArray[np.uint8], Faults]
                .bind(rgb, lambda codes: images.generations(np.ascontiguousarray(codes), quality=self.quality, subsampling=self.subsampling, count=self.generations))
                .bind(lambda values: buffers.located(values, image))
            )

        return (
            coded
            .map(lambda spec: resized(source, spec, "box"))
            .default_value(Ok(source))
            .bind(cycled)
            .bind(lambda image: coded.map(lambda _: resized(image, source.spec(), "triangle")).default_value(Ok(image)))
        )


class Emulation(Effect, frozen=True, kw_only=True):
    """Coded, transmitted, taped, received, and displayed video path, the image the CPU render coded in place of the entering color held while it renders."""

    stage = Stage.EMULATION
    pointwise = False
    domain = Domain.DISPLAY
    staged: IdName | None = None


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PREPARATION]


@prepared.register
def _(
    _record: Emulation,
    _identity: Identity,
    values: Mapping[Key, Value],
    _config: PyOpenColorIO.Config,
    source: OpenImageIO.ImageBuf,
    _precision: Precision,
    frame: CameraFrame,
    scene: bpy.types.Scene,
    /,
) -> Outcome[Option[OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf] | Mapping[Key, Value]]]:
    """Accepts the tape canvases of the captured raster and codes the captured display image before the node sections."""

    def fitted(height: int) -> Outcome[None]:
        match values:
            case {_TAPE.key: False}:
                return Ok(None)
            case {_LINES.key: int(count), _JITTER_PERIOD.key: float(period)}:
                share = Fraction(frame.scale)
                margin = (1 - share) / 2
                lines = Fraction(count)
                first, last = (floor((edge - margin) / share * lines) for edge in (Fraction(1, 2 * height), 1 - Fraction(1, 2 * height)))
                radius = ceil(lines * Fraction(period))
                return accumulated(capacity(scene, size) for size in ((1, 4 * radius + 1), (1, last - first + 1 + 2 * radius))).map(lambda _: None)
            case _:
                return Error(Fault.of(RecordReason.UNREADABLE, Emulation.__name__, repr(values)))

    def compressed(compression: Compression) -> Outcome[OpenImageIO.ImageBuf]:
        def coded(straight: npt.NDArray[np.float32]) -> Outcome[OpenImageIO.ImageBuf]:
            alpha = straight[..., 3:]
            return (
                buffers
                .located(straight, source)
                .bind(lambda image: buffers.checked(OpenImageIO.ImageBufAlgo.channels(image, (0, 1, 2))))
                .bind(compression.applied)
                .bind(buffers.pixels)
                .bind(lambda rgb: buffers.located(buffers.associated(np.concatenate((rgb, alpha), axis=-1)), source))
            )

        return buffers.pixels(source).map(buffers.unassociated).bind(coded)

    match values:
        case {_COMPRESSION.key: False}:
            requested: Outcome[Option[Compression]] = Ok(Nothing)
        case {_QUALITY.key: int(quality), _CODED_WIDTH.key: bool(sized), _CODED_WIDTH_VALUE.key: int(width), _SUBSAMPLING.key: str(chroma), _GENERATIONS.key: int(count)}:
            requested = (
                catch(msgspec.convert, exception=msgspec.ValidationError)({"quality": quality, "coded_width": width if sized else None, "subsampling": chroma, "generations": count}, Compression)
                .map(Some)
                .map_error(partial(Fault.of, RecordReason.UNREADABLE, Compression.__name__))
            )
        case _:
            requested = Error(Fault.of(RecordReason.UNREADABLE, Compression.__name__, repr(values)))
    return paired(fitted(source.spec().height), requested.bind(lambda found: found.map(lambda held: compressed(held).map(Some)).default_value(Ok(Nothing)))).map(lambda parts: parts[1])


# --- [GRAPH]


def _emulation(shell: Shell[Emulation]) -> Outcome[Expr]:
    """Builds corruption, tape, reception, and display after the codec section, color differences in BT.601 luma weights at unit swing about zero.

    Args:
        shell: Codec-processed image, camera frame, and parameter group inputs.

    Returns:
        Displayed color, or the phosphor layout menu fault.
    """
    source = (
        Option
        .of_optional(shell.record.staged)
        .map(lambda image: Expr(kind=bpy.types.CompositorNodeImage, members={"image": image}, socket="Image", shape=bpy.types.NodeSocketColor))
        .default_value(shell.color)
    )
    frame, held = shell.space, shell.input
    scale, seed, lines = held(FRAME_SCALE), held(_SEED), held(_LINES)
    red_weight, blue_weight = (float(weight) for weight in WEIGHTS_YCBCR["ITU-R BT.601"])
    weights = (red_weight, 1.0 - red_weight - blue_weight, blue_weight)
    time = clocked(held(_CLOCK_SPEED), held(_CLOCK_HOLD))
    pattern = math("FLOOR", time)
    x, y = frame.fraction["X"], frame.fraction["Y"]
    aspect = frame.extent["X"] / frame.extent["Y"]
    line = math("FLOOR", y * lines)
    line_cell, sample_cell = xyz(line), xyz(math("FLOOR", x * math("ROUND", lines * aspect)), line)
    sides = dimensions(source)
    half_x, half_y = (0.5 / (sides[axis] * scale) for axis in ("X", "Y"))

    def white(coordinate: Scalar) -> Expr:
        return White(noise_dimensions="4D").node(coordinate, pattern)

    def blurred(image: Scalar, size: Scalar) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", image), ("Size", size), ("Type", "Gaussian")), socket="Image", shape=bpy.types.NodeSocketColor)

    def sample(image: Scalar, position: Expr, across: Border = Border.CLAMP, up: Border = Border.CLAMP) -> Expr:
        return extended(image, sampled(frame, position), (across.item.name, up.item.name), clipped=True)

    def shifted(image: Scalar, horizontal: Scalar, vertical: Scalar = y, across: Border = Border.CLAMP, up: Border = Border.CLAMP) -> Expr:
        return sample(image, xyz(horizontal, vertical), across, up)

    def banded(image: Scalar, horizontal: Scalar, vertical: Scalar) -> Expr:
        return blurred(image, pixels(frame, xyz(horizontal, vertical) * frame.extent, bpy.types.NodeSocketVector))

    def fetched(image: Scalar, uv: Scalar | tuple[float, ...]) -> Expr:
        return Expr(
            kind=bpy.types.CompositorNodeMapUV,
            inputs=(("Image", image), ("UV", uv), ("Interpolation", "Nearest"), ("Extension X", "Clip"), ("Extension Y", "Clip")),
            socket="Image",
            shape=bpy.types.NodeSocketColor,
        )

    def drawn(key: str, lattice: Scalar | tuple[float, ...]) -> Expr:
        return white(vector("ADD", lattice, origin(seed, stream(Emulation, key))))

    def normals(key: str, lattice: Scalar) -> tuple[Expr, Expr, Expr, Expr]:
        axes = vector("ADD", lattice, origin(seed, stream(Emulation, key)))
        first, second, third, fourth = (white(xyz(axes["X"], axes["Y"], index)) for index in range(4))
        return (*gaussian(first, second, EXPRESSIONS), *gaussian(third, fourth, EXPRESSIONS))

    def corrupted(image: Expr) -> Expr:
        cells = drawn("coverage", vector("FLOOR", vector("DIVIDE", frame.point, xyz(held(_CELL_WIDTH), held(_CELL_HEIGHT), 1.0))))
        tone = msgspec.structs.replace(cells, socket="Color", shape=bpy.types.NodeSocketColor)
        struck = cells < held(_COVERAGE)
        cell_x = x + math("MULTIPLY_ADD", tone["Red"], 2.0, -1.0) * held(_DISPLACEMENT) * struck
        channels: tuple[Component, ...] = ("Red", "Green", "Blue")
        red, green, blue = (
            shifted(image, math("MULTIPLY_ADD", held(_SPLIT), direction, cell_x), across=Border.PERIODIC, up=Border.BLACK)[channel] for channel, direction in zip(channels, (-1.0, 0.0, 1.0))
        )
        luma, blue_difference, red_difference = ycc(rgba(red, green, blue), weights)
        power = 2.0 ** math("MULTIPLY_ADD", tone["Green"], 4.0, -2.0)

        def powered(difference: Expr) -> Expr:
            return mixed(held(_CHROMA) * struck, difference, math("SIGN", difference) * (abs(difference) * 2.0) ** power * 0.5)

        return switched(held(_CORRUPTION), image, from_ycc(luma, powered(blue_difference), powered(red_difference), weights))

    def played(image: Expr) -> Expr:
        margin = math("MULTIPLY_ADD", scale, -0.5, 0.5)
        half = 0.5 / sides["Y"]
        first, last = (math("FLOOR", (edge - margin) / scale * lines) for edge in (half, 1.0 - half))
        radius = held(_JITTER_PERIOD) * lines
        support = math("CEIL", radius)
        padding = support * 2.0
        height = last - first + 1.0 + padding
        jitter_canvas, impulse_canvas = (
            Expr(kind=bpy.types.CompositorNodeBlankImage, inputs=(("Size", xyz(1.0, extent)),), socket="Image", shape=bpy.types.NodeSocketColor)
            for extent in (height, math("MULTIPLY_ADD", support, 4.0, 1.0))
        )
        jitter_row, impulse_row = (
            Expr(kind=bpy.types.CompositorNodeImageCoordinates, inputs=(("Image", canvas),), socket="Pixel", shape=bpy.types.NodeSocketVector)["Y"] for canvas in (jitter_canvas, impulse_canvas)
        )
        filtered = xyz(0.0, radius)
        jittered = blurred(normals("jitter", xyz(jitter_row + (first - support)))[0], filtered)
        impulse = fetched(blurred(blurred(math("COMPARE", impulse_row, padding, 0.0), filtered), filtered), (0.5, 0.5, 1.0))
        normalized = jittered["Red"] / math("SQRT", impulse["Red"])
        jitter_shift = math("MULTIPLY", fetched(normalized, xyz(0.5, (line - first + support + 0.5) / height, 1.0)), held(_JITTER))
        bounce_shift = math("MULTIPLY_ADD", drawn("bounce", (0.0, 0.0, 0.0)), 2.0, -1.0) * held(_BOUNCE)
        head = y / held(_HEAD_SWITCHING)
        head_shift = math("MINIMUM", head / 0.9, math("MULTIPLY_ADD", head, -10.0, 10.0)) * ((1.0 - (y < 0.0)) * (y < held(_HEAD_SWITCHING))) * held(_SKEW)
        tracking = math("MULTIPLY_ADD", abs(math("WRAP", y - math("FRACT", time * held(_TRACKING_RATE)), 0.5, -0.5)) / held(_TRACKING_HEIGHT), -2.0, 1.0, clamp=True)
        picture = shifted(image, x + jitter_shift + head_shift + tracking * held(_TRACKING_SHIFT), y + bounce_shift, up=Border.BLACK)
        luma, blue_difference, red_difference = ycc(picture, weights)
        mistracked = from_ycc(mixed(tracking * held(_MISTRACKING), luma, drawn("mistracking", line_cell)), blue_difference, red_difference, weights)
        position = x / held(_DROPOUT_LENGTH)
        struck = drawn("dropouts", xyz(math("FLOOR", position), line)) < held(_DROPOUTS)
        return switched(held(_TAPE), image, mixed((1.0 - math("FRACT", position)) * struck, mistracked, (1.0, 1.0, 1.0)))

    def received(image: Expr) -> Expr:
        luma, blue_difference, red_difference = ycc(image, weights)
        luma_noise, chroma_noises = normals("noise", sample_cell), normals("chroma_noise", sample_cell)
        strength = math("SUBTRACT", luma, banded(luma, 1.0 / 377.0, 0.0)) * held(_CROSS_COLOR)
        phase = math("MULTIPLY_ADD", x, 188.3, (line + pattern) * 0.5) * 2.0 * np.pi
        green, blue = (
            math("MULTIPLY_ADD", noise, held(_CHROMA_NOISE), math("MULTIPLY_ADD", strength, wave, difference))
            for difference, wave, noise in ((blue_difference, math("COSINE", phase), chroma_noises[0]), (red_difference, math("SINE", phase), chroma_noises[1]))
        )
        band = banded(math("MULTIPLY_ADD", luma_noise[0], held(_NOISE), luma), held(_LUMA_BLUR), 0.0)
        chroma = shifted(banded(rgba(green, blue, 0.0), held(_CHROMA_BLUR), held(_CHROMA_VERTICAL)), x - held(_CHROMA_DELAY))
        ringed = (shifted(band, math("MULTIPLY_ADD", held(_LUMA_BLUR) / 3.0, direction, x)) for direction in (-1.0, 1.0))
        overshoot = -math("MULTIPLY_ADD", band, -2.0, math("ADD", *ringed))
        packed = rgba(math("MULTIPLY_ADD", overshoot, held(_RINGING), band), chroma["Red"] * held(_SATURATION), chroma["Green"] * held(_SATURATION))
        amplitude = held(_GHOST_AMPLITUDE)
        echoed = switched(held(_GHOST), packed, mixed(amplitude / (amplitude + 1.0), packed, shifted(packed, x - held(_GHOST_DELAY))))
        loss = math("MULTIPLY_ADD", math("SINE", math("MULTIPLY_ADD", time, -0.002, y) * 2.0 * np.pi), 0.5, 0.5) * held(_HUM)
        return switched(held(_SIGNAL), image, from_ycc(echoed["Red"] * (1.0 - loss), echoed["Green"], echoed["Blue"], weights))

    def displayed(image: Expr, geometry: Expr) -> Expr:
        rolled = shifted(image, x, math("MULTIPLY_ADD", held(_ROLL), time, y), up=Border.PERIODIC)
        least = 1.0 / lines
        lowered = math("MULTIPLY_ADD", held(_COLLAPSE), -2.0, 1.0)
        height, width = math("MAXIMUM", lowered, least), math("MINIMUM", math("MAXIMUM", lowered + 1.0, least), 1.0)
        at = vector("ADD", vector("DIVIDE", vector("SUBTRACT", frame.fraction, (0.5, 0.5, 0.0)), xyz(width, height, 1.0)), (0.5, 0.5, 0.0))
        collapsed = vector("DIVIDE", sample(rolled, at, Border.BLACK, Border.BLACK), width * height)
        radius = pixels(frame, xyz(held(_GLOW_RADIUS), held(_GLOW_RADIUS)), bpy.types.NodeSocketVector)
        near = blurred(collapsed, radius)
        glowed = switched(held(_GLOW), collapsed, mixed(0.890909, blurred(near, radius * (4.0, 1.0, 0.0)), near))
        field = math("PINGPONG", pattern, 1.0)

        def scanned_row(sign: float) -> Expr:
            raster = ((math("MULTIPLY_ADD", half_y, sign, y) - 0.5) / height + 0.5) * lines
            return switched(held(_INTERLACED), raster, (raster + field) * 0.5)

        low, high = scanned_row(-1.0), scanned_row(1.0)
        tau = 2.0 * np.pi
        strength = held(_SCANLINES_STRENGTH)
        light = 1.0 - ((math("SINE", high * tau) - math("SINE", low * tau)) / ((high - low) * (2.0 * tau)) + 0.5) * strength
        scanned = switched(held(_SCANLINES), glowed, vector("SCALE", glowed, light / math("MULTIPLY_ADD", strength, -0.5, 1.0)))
        fill = 1.0 - geometry["X"]
        triads = held(_TRIADS)
        rows_per_frame = triads / aspect
        reach = {"x": (x * triads, half_x * triads, geometry["Z"]), "y": (y * rows_per_frame, half_y * rows_per_frame, geometry["Y"])}
        ends = {
            (axis, parity, sign): math("MULTIPLY_ADD", half, sign, math("MULTIPLY_ADD", stagger, parity, center))
            for axis, (center, half, stagger) in reach.items()
            for parity in range(2)
            for sign in (-1.0, 1.0)
        }

        def primitive(at: Expr, gain: Scalar, start: Scalar, span: Scalar) -> Expr:
            whole = math("FLOOR", at / 2.0)
            return math("MULTIPLY_ADD", whole, gain, math("MINIMUM", math("MAXIMUM", math("MULTIPLY_ADD", whole, -2.0, at) - start, 0.0), span))

        def covered(axis: str, parity: int, gain: Scalar, start: Scalar, span: Scalar) -> Expr:
            return primitive(ends[axis, parity, 1.0], gain, start, span) - primitive(ends[axis, parity, -1.0], gain, start, span)

        matrix = {(row, column): covered("y", column, fill, float(row), fill) for row, column in product(range(2), repeat=2)}
        area = reach["y"][1] * 2.0 * (reach["x"][1] * 2.0)
        mask = held(_PHOSPHORS_STRENGTH)

        def gained(index: int) -> Expr:
            lit = {(row, column): covered("x", row, 1.0 / 3.0, column + index / 3.0, 1.0 / 3.0) * cell for (row, column), cell in matrix.items()}
            integral = lit[0, 0] + lit[0, 1] + (lit[1, 0] + lit[1, 1])
            return math("MULTIPLY_ADD", integral / area / fill * 3.0, mask, 1.0 - mask)

        masked = switched(held(_PHOSPHORS), scanned, vector("MULTIPLY", scanned, rgba(gained(0), gained(1), gained(2))))
        bulge = held(_CURVATURE)
        offset = frame.point * (2.0, 2.0, 0.0) - frame.extent
        stretched = vector("SCALE", offset, math("SQRT", bulge) * 2.0 / (bulge + 1.0))
        real = 1.0 - (vector("LENGTH", stretched) > 1.0)
        lens = vector(
            "MULTIPLY_ADD",
            vector("SCALE", offset / frame.extent, scale),
            where(real, 1.0 / ((bulge + 1.0) * (vector("LENGTH", vector("ADD", stretched, (0.0, 0.0, 1.0))) + 1.0)), 0.0),
            xyz(0.5, 0.5, real),
        )
        return switched(held(_DISPLAY), image, extended(masked, lens, (Border.BLACK.item.name, Border.BLACK.item.name), clipped=True))

    geometry = shell.menu(_LAYOUT, cases={_APERTURE_GRILLE: (0.0, 0.0, 0.0), _SLOT_MASK: (1.0 / 3.0, 0.5, 0.0), _SHADOW_MASK: (1.0 / 3.0, 0.0, 0.5)})
    return Ok(displayed(received(played(corrupted(source))), geometry))


emulation: Final = CompositorEffect(
    graph=_emulation,
    record=Emulation,
    interface=Interface(inputs=(_LINES, _SEED), panels=(_COMPRESSION, _CORRUPTION, _TAPE, _SIGNAL, _DISPLAY, CLOCK)),
    mix=Blended(alpha=Alpha.STRAIGHT),
    request=Some(precise),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Compression", "Emulation", "emulation"]
