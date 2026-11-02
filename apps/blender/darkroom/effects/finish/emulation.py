"""Video path emulation: JPEG coding, digital corruption, tape playback, and analog reception in BT.601 luma and color difference, shown on a picture tube with vertical roll, power-off collapse, screen-glass glow, beam scanlines, phosphor masks, and glass curvature."""

from functools import partial
from itertools import product
from types import MappingProxyType
from typing import Annotated, Final

import bpy
from colour.models import WEIGHTS_YCBCR
from expression import Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO

from blender.bpy.nodes.desired import MenuItem, MenuItems
from blender.bpy.nodes.functions import EXPRESSIONS
from blender.bpy.nodes.graphs import Component, Expr, math, mixed, rgba, Scalar, switched, vector, where, xyz
from blender.bpy.nodes.interfaces import decoded, Interface, InterfacePanel, Socket
from blender.bpy.nodes.textures import WhiteNoise
from blender.bpy.render.compositor.colors import from_ycc, ycc
from blender.bpy.render.compositor.coordinates import Border, dimensions, extended, uv_mapped
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, precise, Prepared, Stage
from blender.bpy.render.compositor.filters import blurred
from blender.bpy.render.compositor.inputs import FRAME_SCALE, GraphInputs, toggled, value
from blender.bpy.render.compositor.motion import CLOCK, Clock, clocked, origin, SEED, timing
from blender.bpy.render.outputs.framing import capacity
from blender.imaging.files import buffers, images
from blender.imaging.geometry.arithmetic import Elementwise, gaussian, Namespace
from blender.imaging.sampling.tiles import stream
from blender.records.faults import accumulated, Faults, Outcome, paired
from blender.records.fields import Case, Count

# --- [CONSTANTS] ------------------------------------------------------------------------

_LINES: Final = Socket(key="lines", kind=bpy.types.NodeSocketInt, description="Raster lines across frame height", min_value=1, default_value=480)
_COVERAGE: Final = Socket(key="coverage", kind=bpy.types.NodeSocketFloatFactor, description="Share of corrupted cells per pattern", default_value=0.2)
_CELL_WIDTH: Final = Socket(key="cell_width", kind=bpy.types.NodeSocketFloat, description="Corrupted cell width over greater frame side", min_value=1e-6, default_value=0.03)
_CELL_HEIGHT: Final = Socket(key="cell_height", kind=bpy.types.NodeSocketFloat, description="Corrupted cell height over greater frame side", min_value=1e-6, default_value=0.03)
_DISPLACEMENT: Final = Socket(key="displacement", kind=bpy.types.NodeSocketFloat, description="Greatest horizontal cell shift as a frame share", min_value=0.0, default_value=0.004)
_SPLIT: Final = Socket(key="split", kind=bpy.types.NodeSocketFloat, description="Opposing red and blue offsets across frame as a frame share", min_value=0.0)
_POWER: Final = Socket(key="chroma", kind=bpy.types.NodeSocketFloatFactor, description="Sign-preserving chroma power in struck cells", default_value=0.6)
_CORRUPTION: Final = InterfacePanel.of(
    "corruption", Interface(inputs=(_COVERAGE, _CELL_WIDTH, _CELL_HEIGHT, _DISPLACEMENT, _SPLIT, _POWER)), description="Rectangular buffer corruption with a frame-wide RGB split", toggle=Some(False)
)
_JITTER: Final = Socket(key="jitter", kind=bpy.types.NodeSocketFloat, description="Standard deviation of horizontal line displacement as a frame share", min_value=0.0, default_value=0.002)
_JITTER_PERIOD: Final = Socket(key="jitter_period", kind=bpy.types.NodeSocketFloat, description="Vertical jitter smoothing period as a frame share", min_value=0.0, default_value=1.0 / 65.0)
_BOUNCE: Final = Socket(key="bounce", kind=bpy.types.NodeSocketFloat, description="Greatest vertical frame shift as a frame share", min_value=0.0)
_HEAD_SWITCHING: Final = Socket(key="head_switching", kind=bpy.types.NodeSocketFloat, description="Skewed bottom band height as a frame share", min_value=0.0, default_value=0.0176)
_SKEW: Final = Socket(key="skew", kind=bpy.types.NodeSocketFloat, description="Peak horizontal head-switching shift at nine tenths of band", default_value=0.05)
_TRACKING_SHIFT: Final = Socket(key="tracking_shift", kind=bpy.types.NodeSocketFloat, description="Shift at tracking band center")
_TRACKING_HEIGHT: Final = Socket(key="tracking_height", kind=bpy.types.NodeSocketFloat, description="Tracking band height as a frame share", min_value=1e-6, default_value=0.15)
_TRACKING_RATE: Final = Socket(key="tracking_rate", kind=bpy.types.NodeSocketFloat, description="Frame heights traveled upward per clock unit", min_value=0.0)
_MISTRACKING: Final = Socket(key="mistracking", kind=bpy.types.NodeSocketFloatFactor, description="Luma noise in tracking band")
_DROPOUTS: Final = Socket(key="dropouts", kind=bpy.types.NodeSocketFloatFactor, description="Share of dropout-length cells struck per raster line", default_value=0.02)
_DROPOUT_LENGTH: Final = Socket(key="dropout_length", kind=bpy.types.NodeSocketFloat, description="Fading dropout cell length as a frame share", min_value=1e-6, default_value=0.04)
_TAPE: Final = InterfacePanel.of(
    "tape",
    Interface(inputs=(_JITTER, _JITTER_PERIOD, _BOUNCE, _HEAD_SWITCHING, _SKEW, _TRACKING_SHIFT, _TRACKING_HEIGHT, _TRACKING_RATE, _MISTRACKING, _DROPOUTS, _DROPOUT_LENGTH)),
    description="Time-base error, head switching, tracking disturbance, and dropouts",
    toggle=Some(False),
)
_LUMA_BLUR: Final = Socket(key="luma_blur", kind=bpy.types.NodeSocketFloat, description="Horizontal luma bandwidth blur as a frame share", min_value=0.0, default_value=0.0023)
_CHROMA_BLUR: Final = Socket(key="chroma_blur", kind=bpy.types.NodeSocketFloat, description="Horizontal color-difference bandwidth blur as a frame share", min_value=0.0, default_value=0.0073)
_CHROMA_VERTICAL: Final = Socket(key="chroma_vertical", kind=bpy.types.NodeSocketFloat, description="Vertical color-difference blur as a frame share", min_value=0.0)
_CHROMA_DELAY: Final = Socket(key="chroma_delay", kind=bpy.types.NodeSocketFloat, description="Color-difference offset from luma, right above zero")
_SATURATION: Final = Socket(key="saturation", kind=bpy.types.NodeSocketFloat, description="Color-difference gain", min_value=0.0, default_value=1.0)
_RINGING: Final = Socket(key="ringing", kind=bpy.types.NodeSocketFloatFactor, description="Second-difference overshoot at luma edges", default_value=0.1)
_CROSS_COLOR: Final = Socket(key="cross_color", kind=bpy.types.NodeSocketFloatFactor, description="Fine luma detail decoded as NTSC subcarrier color")
_NOISE: Final = Socket(key="noise", kind=bpy.types.NodeSocketFloat, description="Luma noise standard deviation per raster sample", min_value=0.0, default_value=0.02)
_CHROMA_NOISE: Final = Socket(key="chroma_noise", kind=bpy.types.NodeSocketFloat, description="Color-difference noise standard deviation per raster sample", min_value=0.0, default_value=0.03)
_HUM: Final = Socket(key="hum", kind=bpy.types.NodeSocketFloatFactor, description="Depth of mains hum bar drifting upward")
_ECHO: Final = Socket(key="amplitude", kind=bpy.types.NodeSocketFloatFactor, description="Delayed echo strength", default_value=0.1)
_DELAY: Final = Socket(key="delay", kind=bpy.types.NodeSocketFloat, description="Horizontal echo offset, right above zero", default_value=0.05)
_GHOST: Final = InterfacePanel.of("ghost", Interface(inputs=(_ECHO, _DELAY)), description="Multipath echo of whole signal", toggle=Some(False))
_SIGNAL: Final = InterfacePanel.of(
    "signal",
    Interface(inputs=(_LUMA_BLUR, _CHROMA_BLUR, _CHROMA_VERTICAL, _CHROMA_DELAY, _SATURATION, _RINGING, _CROSS_COLOR, _NOISE, _CHROMA_NOISE, _HUM), panels=(_GHOST,)),
    description="Analog video bandwidth, Gaussian noise, and carrier disturbances",
    toggle=Some(True),
)
_LAYOUTS: Final = MappingProxyType({MenuItem(name="Aperture Grille"): (0.0, 0.0, 0.0), MenuItem(name="Slot Mask"): (1.0 / 3.0, 0.5, 0.0), MenuItem(name="Shadow Mask"): (1.0 / 3.0, 0.0, 0.5)})
_CURVATURE: Final = Socket(key="curvature", kind=bpy.types.NodeSocketFloat, description="Screen glass barrel bulge", min_value=0.0, default_value=0.03)
_ROLL: Final = Socket(key="roll", kind=bpy.types.NodeSocketFloat, description="Frame heights rolled per clock unit without vertical hold")
_COLLAPSE: Final = Socket(key="collapse", kind=bpy.types.NodeSocketFloatFactor, description="Power-off collapse to line, then dot")
_RADIUS: Final = Socket(key="radius", kind=bpy.types.NodeSocketFloat, description="Gaussian glass blur", min_value=0.0, default_value=0.01)
_GLOW: Final = InterfacePanel.of("glow", Interface(inputs=(_RADIUS,)), description="Linear screen-glass scattering before scanlines and phosphors", toggle=Some(False))
_DARKENING: Final = Socket(key="strength", kind=bpy.types.NodeSocketFloatFactor, description="Darkening between raster lines before mean normalization", default_value=0.4)
_INTERLACED: Final = Socket(key="interlaced", kind=bpy.types.NodeSocketBool, description="Light alternate raster fields per pattern")
_SCANLINES: Final = InterfacePanel.of("scanlines", Interface(inputs=(_DARKENING, _INTERLACED)), description="Raster lines the beam draws", toggle=Some(True))
_LAYOUT: Final = Socket(key="layout", kind=bpy.types.NodeSocketMenu, description="Phosphor arrangement", default_value=next(iter(_LAYOUTS)).name, items=Some(MenuItems(items=tuple(_LAYOUTS))))
_TRIADS: Final = Socket(key="triads", kind=bpy.types.NodeSocketInt, description="Phosphor triads across frame width", min_value=1, default_value=640)
_MASKING: Final = Socket(key="strength", kind=bpy.types.NodeSocketFloatFactor, description="Blend toward mean-one phosphor mask", default_value=0.705394)
_PHOSPHORS: Final = InterfacePanel.of("phosphors", Interface(inputs=(_LAYOUT, _TRIADS, _MASKING)), description="Phosphor mask on glass", toggle=Some(False))
_DISPLAY: Final = InterfacePanel.of(
    "display",
    Interface(inputs=(_CURVATURE, _ROLL, _COLLAPSE), panels=(_GLOW, _SCANLINES, _PHOSPHORS)),
    description="Picture tube showing a collapsed, glowing, scanned, and masked signal",
    toggle=Some(True),
)

# --- [MODELS] ---------------------------------------------------------------------------


class Compression(msgspec.Struct, frozen=True, kw_only=True):
    """JPEG picture block coding."""

    subsampling: Annotated[images.Subsampling, msgspec.Meta(description="Chroma resolution relative to luma")] = "4:2:0"
    quality: Annotated[int, msgspec.Meta(ge=1, le=100, description="JPEG quality on IJG scale")] = 60
    generations: Annotated[Count, msgspec.Meta(description="JPEG encode and decode cycles")] = 1
    coded_width: Annotated[int, msgspec.Meta(ge=0, description="Picture width before coding, the input raster's at 0")] = 0

    def applied(self, source: OpenImageIO.ImageBuf) -> Outcome[OpenImageIO.ImageBuf]:
        """Codes straight display RGB through box reduction to the coded width, JPEG generations, and triangle reconstruction."""
        spec = source.spec()
        coded = Some(self.coded_width).filter(bool).map(partial(buffers.rescaled, spec))

        def cycled(image: OpenImageIO.ImageBuf) -> Outcome[OpenImageIO.ImageBuf]:
            rgb: Outcome[npt.NDArray[np.uint8]] = buffers.pixels(image, buffers.Depth.UINT8)
            return (
                Result[npt.NDArray[np.uint8], Faults]
                .bind(rgb, lambda codes: images.generations(np.ascontiguousarray(codes), quality=self.quality, subsampling=self.subsampling, count=self.generations))
                .bind(lambda values: buffers.located(values, image))
            )

        return (
            coded
            .map(lambda held: buffers.resized(source, held, filtername="box"))
            .default_value(Ok(source))
            .bind(cycled)
            .bind(lambda image: coded.map(lambda _: buffers.resized(image, spec, filtername="triangle")).default_value(Ok(image)))
        )


class Emulation(Case, frozen=True, kw_only=True):
    """Coded, transmitted, taped, received, and displayed video path, the coding rendered on the CPU before the graph reads it."""


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CANVASES]


def _canvases[V: Elementwise, B](scale: V, height: V, lines: V, period: V, ns: Namespace[V, B], /) -> tuple[V, V, V, V]:
    """Tape jitter canvases over the raster lines a frame's data window spans, in any namespace.

    Args:
        scale: Camera share of overscan.
        height: Data window height in pixels.
        lines: Raster lines across frame height.
        period: Jitter smoothing period as a frame share.
        ns: Elementwise namespace, numpy on the CPU and `EXPRESSIONS` in a graph.

    Returns:
        First spanned line, smoothing support in lines, then the jitter and impulse canvas heights.
    """
    margin = (1.0 - scale) * 0.5
    half = 0.5 / height
    first, last = (ns.floor((edge - margin) / scale * lines) for edge in (half, 1.0 - half))
    support = ns.negative(ns.floor(ns.negative(period * lines)))
    return first, support, last - first + 1.0 + support * 2.0, support * 4.0 + 1.0


# --- [PREPARATION]


def _prepared(stage: Stage[Emulation]) -> Outcome[Option[Prepared]]:
    """Accepts the tape canvases of the captured raster and codes the captured display image before the graph.

    Args:
        stage: Captured display image, entry values, camera frame, and scene.

    Returns:
        Coded image while compression is on, else Nothing, or the faults of the entry values, canvas sizes, and coding.
    """
    values, source = stage.values, stage.image

    def fitted(lines: int, period: float) -> Outcome[None]:
        held = (np.asarray(stage.frame.scale, dtype=np.float64), np.asarray(source.spec().height, dtype=np.float64), np.asarray(lines, dtype=np.float64), np.asarray(period, dtype=np.float64))
        _, _, rows, taps = _canvases(*held, np)
        return accumulated(capacity(stage.scene, (1, int(size))) for size in (taps, rows)).map(lambda _: None)

    def coded(compression: Compression) -> Outcome[OpenImageIO.ImageBuf]:
        def straight(pixels: npt.NDArray[np.float32]) -> Outcome[OpenImageIO.ImageBuf]:
            alpha = pixels[..., 3:]
            return (
                buffers
                .located(pixels, source)
                .bind(lambda image: buffers.checked(OpenImageIO.ImageBufAlgo.channels(image, (0, 1, 2))))
                .bind(compression.applied)
                .bind(buffers.pixels)
                .bind(lambda rgb: buffers.located(buffers.associated(np.concatenate((rgb, alpha), axis=-1)), source))
            )

        return buffers.pixels(source).map(buffers.unassociated).bind(straight)

    taped = toggled(values, lambda: paired(value(values, int, _LINES), value(values, float, _TAPE, _JITTER_PERIOD)).bind(lambda held: fitted(*held)), _TAPE)
    compressed = toggled(values, lambda: decoded(Compression(), values, _COMPRESSION.key).bind(coded), _COMPRESSION)
    return paired(taped, compressed).map(lambda parts: parts[1])


# --- [TUBE]


def _displayed(shell: GraphInputs[Emulation], image: Expr, *, time: Expr, lines: Expr) -> Expr:
    """Shows a received signal on a picture tube whose sockets the display panel declares, the entering image's data window setting the beam's pixel footprint.

    Args:
        shell: Entry inputs and the entering image.
        image: Received associated signal.
        time: Clock units animating roll and interlaced fields.
        lines: Raster lines across frame height.

    Returns:
        Displayed associated color, the signal while the display is off.
    """
    frame, display = shell.space, shell.input
    x, y = frame.fraction["X"], frame.fraction["Y"]
    aspect = frame.extent["X"] / frame.extent["Y"]
    sides, scale = dimensions(shell.color), display(FRAME_SCALE)
    half_x, half_y = (0.5 / (sides[axis] * scale) for axis in ("X", "Y"))
    rolled = extended(image, frame.sampled(xyz(x, math("MULTIPLY_ADD", display(_DISPLAY, _ROLL), time, y))), (Border.CLAMP, Border.PERIODIC), clipped=True)
    least = 1.0 / lines
    lowered = math("MULTIPLY_ADD", display(_DISPLAY, _COLLAPSE), -2.0, 1.0)
    height, width = math("MAXIMUM", lowered, least), math("MINIMUM", math("MAXIMUM", lowered + 1.0, least), 1.0)
    at = vector("ADD", vector("DIVIDE", vector("SUBTRACT", frame.fraction, (0.5, 0.5, 0.0)), xyz(width, height, 1.0)), (0.5, 0.5, 0.0))
    collapsed = vector("DIVIDE", extended(rolled, frame.sampled(at), Border.BLACK, clipped=True), width * height)
    radius = frame.pixel_size(xyz(display(_DISPLAY, _GLOW, _RADIUS), display(_DISPLAY, _GLOW, _RADIUS)))
    near = blurred(collapsed, radius)
    glowed = switched(display(_DISPLAY, _GLOW), collapsed, mixed(0.890909, blurred(near, radius * (4.0, 1.0, 0.0)), near))
    field = math("PINGPONG", math("FLOOR", time), 1.0)

    def scanned_row(sign: float) -> Expr:
        raster = ((math("MULTIPLY_ADD", half_y, sign, y) - 0.5) / height + 0.5) * lines
        return switched(display(_DISPLAY, _SCANLINES, _INTERLACED), raster, (raster + field) * 0.5)

    low, high = scanned_row(-1.0), scanned_row(1.0)
    tau = 2.0 * np.pi
    darkening = display(_DISPLAY, _SCANLINES, _DARKENING)
    light = 1.0 - ((math("SINE", high * tau) - math("SINE", low * tau)) / ((high - low) * (2.0 * tau)) + 0.5) * darkening
    scanned = switched(display(_DISPLAY, _SCANLINES), glowed, vector("SCALE", glowed, light / math("MULTIPLY_ADD", darkening, -0.5, 1.0)))
    geometry = shell.menu(_DISPLAY, _PHOSPHORS, _LAYOUT, cases=_LAYOUTS)
    fill = 1.0 - geometry["X"]
    triads = display(_DISPLAY, _PHOSPHORS, _TRIADS)
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
    mask = display(_DISPLAY, _PHOSPHORS, _MASKING)

    def gained(index: int) -> Expr:
        lit = {(row, column): covered("x", row, 1.0 / 3.0, column + index / 3.0, 1.0 / 3.0) * cell for (row, column), cell in matrix.items()}
        integral = lit[0, 0] + lit[0, 1] + (lit[1, 0] + lit[1, 1])
        return math("MULTIPLY_ADD", integral / area / fill * 3.0, mask, 1.0 - mask)

    masked = switched(display(_DISPLAY, _PHOSPHORS), scanned, vector("MULTIPLY", scanned, rgba(gained(0), gained(1), gained(2))))
    bulge = display(_DISPLAY, _CURVATURE)
    offset = frame.point * (2.0, 2.0, 0.0) - frame.extent
    stretched = vector("SCALE", offset, math("SQRT", bulge) * 2.0 / (bulge + 1.0))
    real = 1.0 - (vector("LENGTH", stretched) > 1.0)
    lens = vector(
        "MULTIPLY_ADD",
        vector("SCALE", offset / frame.extent, scale),
        where(real, 1.0 / ((bulge + 1.0) * (vector("LENGTH", vector("ADD", stretched, (0.0, 0.0, 1.0))) + 1.0)), 0.0),
        xyz(0.5, 0.5, real),
    )
    return switched(display(_DISPLAY), image, extended(masked, lens, Border.BLACK, clipped=True))


# --- [GRAPH]


def _emulation(shell: GraphInputs[Emulation]) -> Outcome[Expr]:
    """Builds corruption, tape, reception, and display after the CPU coding, color differences in BT.601 luma weights at unit swing about zero.

    Args:
        shell: Coded image, camera frame, and parameter group inputs.

    Returns:
        Displayed color.
    """
    source, frame, held = shell.color, shell.space, shell.input
    corrupting, taping, signaling = partial(held, _CORRUPTION), partial(held, _TAPE), partial(held, _SIGNAL)
    seed, lines = held(SEED), held(_LINES)
    red_weight, blue_weight = (float(weight) for weight in WEIGHTS_YCBCR["ITU-R BT.601"])
    weights = (red_weight, 1.0 - red_weight - blue_weight, blue_weight)
    time = clocked(held, _CLOCK)
    pattern = math("FLOOR", time)
    x, y = frame.fraction["X"], frame.fraction["Y"]
    line = math("FLOOR", y * lines)
    line_cell, sample_cell = xyz(line), xyz(math("FLOOR", x * math("ROUND", lines * frame.extent["X"] / frame.extent["Y"])), line)

    def white(coordinate: Scalar) -> Expr:
        return WhiteNoise(noise_dimensions="4D").node(coordinate, pattern)

    def shifted(image: Scalar, horizontal: Scalar, vertical: Scalar = y, across: Border = Border.CLAMP, up: Border = Border.CLAMP) -> Expr:
        return extended(image, frame.sampled(xyz(horizontal, vertical)), (across, up), clipped=True)

    def banded(image: Scalar, horizontal: Scalar, vertical: Scalar) -> Expr:
        return blurred(image, frame.pixel_size(xyz(horizontal, vertical) * frame.extent))

    def drawn(socket: Socket, lattice: Scalar | tuple[float, ...]) -> Expr:
        return white(vector("ADD", lattice, origin(seed, stream(Emulation, socket.key))))

    def normals(socket: Socket, lattice: Scalar) -> tuple[Expr, Expr, Expr, Expr]:
        axes = vector("ADD", lattice, origin(seed, stream(Emulation, socket.key)))
        first, second, third, fourth = (white(xyz(axes["X"], axes["Y"], index)) for index in range(4))
        return (*gaussian(first, second, EXPRESSIONS), *gaussian(third, fourth, EXPRESSIONS))

    def corrupted(image: Expr) -> Expr:
        cells = drawn(_COVERAGE, vector("FLOOR", vector("DIVIDE", frame.point, xyz(corrupting(_CELL_WIDTH), corrupting(_CELL_HEIGHT), 1.0))))
        tone = cells.at("Color", bpy.types.NodeSocketColor)
        struck = cells < corrupting(_COVERAGE)
        cell_x = x + math("MULTIPLY_ADD", tone["Red"], 2.0, -1.0) * corrupting(_DISPLACEMENT) * struck
        channels: tuple[Component, ...] = ("Red", "Green", "Blue")
        red, green, blue = (
            shifted(image, math("MULTIPLY_ADD", corrupting(_SPLIT), direction, cell_x), across=Border.PERIODIC, up=Border.BLACK)[channel] for channel, direction in zip(channels, (-1.0, 0.0, 1.0))
        )
        luma, blue_difference, red_difference = ycc(rgba(red, green, blue), weights)
        power = 2.0 ** math("MULTIPLY_ADD", tone["Green"], 4.0, -2.0)

        def powered(difference: Expr) -> Expr:
            return mixed(corrupting(_POWER) * struck, difference, math("SIGN", difference) * (abs(difference) * 2.0) ** power * 0.5)

        return switched(held(_CORRUPTION), image, from_ycc(luma, powered(blue_difference), powered(red_difference), weights))

    def played(image: Expr) -> Expr:
        first, support, rows, taps = _canvases(held(FRAME_SCALE), dimensions(source)["Y"], lines, taping(_JITTER_PERIOD), EXPRESSIONS)
        padding = support * 2.0
        jitter_canvas, impulse_canvas = (
            Expr(kind=bpy.types.CompositorNodeBlankImage, inputs=(("Size", xyz(1.0, extent)),), socket="Image", socket_type=bpy.types.NodeSocketColor) for extent in (rows, taps)
        )
        jitter_row, impulse_row = (
            Expr(kind=bpy.types.CompositorNodeImageCoordinates, inputs=(("Image", canvas),), socket="Pixel", socket_type=bpy.types.NodeSocketVector)["Y"] for canvas in (jitter_canvas, impulse_canvas)
        )
        filtered = xyz(0.0, taping(_JITTER_PERIOD) * lines)
        jittered = blurred(normals(_JITTER, xyz(jitter_row + (first - support)))[0], filtered)
        impulse = uv_mapped(blurred(blurred(math("COMPARE", impulse_row, padding, 0.0), filtered), filtered), (0.5, 0.5, 1.0), interpolation="Nearest")
        normalized = jittered["Red"] / math("SQRT", impulse["Red"])
        jitter_shift = math("MULTIPLY", uv_mapped(normalized, xyz(0.5, (line - first + support + 0.5) / rows, 1.0), interpolation="Nearest"), taping(_JITTER))
        bounce_shift = math("MULTIPLY_ADD", drawn(_BOUNCE, (0.0, 0.0, 0.0)), 2.0, -1.0) * taping(_BOUNCE)
        head = y / taping(_HEAD_SWITCHING)
        head_shift = math("MINIMUM", head / 0.9, math("MULTIPLY_ADD", head, -10.0, 10.0)) * ((1.0 - (y < 0.0)) * (y < taping(_HEAD_SWITCHING))) * taping(_SKEW)
        tracking = math("MULTIPLY_ADD", abs(math("WRAP", y - math("FRACT", time * taping(_TRACKING_RATE)), 0.5, -0.5)) / taping(_TRACKING_HEIGHT), -2.0, 1.0, clamp=True)
        picture = shifted(image, x + jitter_shift + head_shift + tracking * taping(_TRACKING_SHIFT), y + bounce_shift, up=Border.BLACK)
        luma, blue_difference, red_difference = ycc(picture, weights)
        mistracked = from_ycc(mixed(tracking * taping(_MISTRACKING), luma, drawn(_MISTRACKING, line_cell)), blue_difference, red_difference, weights)
        position = x / taping(_DROPOUT_LENGTH)
        struck = drawn(_DROPOUTS, xyz(math("FLOOR", position), line)) < taping(_DROPOUTS)
        return switched(held(_TAPE), image, mixed((1.0 - math("FRACT", position)) * struck, mistracked, (1.0, 1.0, 1.0)))

    def received(image: Expr) -> Expr:
        luma, blue_difference, red_difference = ycc(image, weights)
        luma_noise, chroma_noises = normals(_NOISE, sample_cell), normals(_CHROMA_NOISE, sample_cell)
        strength = math("SUBTRACT", luma, banded(luma, 1.0 / 377.0, 0.0)) * signaling(_CROSS_COLOR)
        phase = math("MULTIPLY_ADD", x, 188.3, (line + pattern) * 0.5) * 2.0 * np.pi
        green, blue = (
            math("MULTIPLY_ADD", noise, signaling(_CHROMA_NOISE), math("MULTIPLY_ADD", strength, wave, difference))
            for difference, wave, noise in ((blue_difference, math("COSINE", phase), chroma_noises[0]), (red_difference, math("SINE", phase), chroma_noises[1]))
        )
        band = banded(math("MULTIPLY_ADD", luma_noise[0], signaling(_NOISE), luma), signaling(_LUMA_BLUR), 0.0)
        chroma = shifted(banded(rgba(green, blue, 0.0), signaling(_CHROMA_BLUR), signaling(_CHROMA_VERTICAL)), x - signaling(_CHROMA_DELAY))
        ringed = (shifted(band, math("MULTIPLY_ADD", signaling(_LUMA_BLUR) / 3.0, direction, x)) for direction in (-1.0, 1.0))
        overshoot = -math("MULTIPLY_ADD", band, -2.0, math("ADD", *ringed))
        packed = rgba(math("MULTIPLY_ADD", overshoot, signaling(_RINGING), band), chroma["Red"] * signaling(_SATURATION), chroma["Green"] * signaling(_SATURATION))
        amplitude = signaling(_GHOST, _ECHO)
        echoed = switched(signaling(_GHOST), packed, mixed(amplitude / (amplitude + 1.0), packed, shifted(packed, x - signaling(_GHOST, _DELAY))))
        loss = math("MULTIPLY_ADD", math("SINE", math("MULTIPLY_ADD", time, -0.002, y) * 2.0 * np.pi), 0.5, 0.5) * signaling(_HUM)
        return switched(held(_SIGNAL), image, from_ycc(echoed["Red"] * (1.0 - loss), echoed["Green"], echoed["Blue"], weights))

    return Ok(_displayed(shell, received(played(corrupted(source))), time=time, lines=lines))


# --- [COMPOSITION] ----------------------------------------------------------------------

_COMPRESSION: Final = InterfacePanel.of("compression", Compression(), toggle=Some(False))
_CLOCK: Final = InterfacePanel.of(CLOCK.key, Clock(speed=24.0))
emulation: Final = CompositorEffect(
    _emulation,
    record=Emulation,
    interface=Interface(inputs=(_LINES, SEED), panels=(_COMPRESSION, _CORRUPTION, _TAPE, _SIGNAL, _DISPLAY, _CLOCK)),
    mix=Blended(alpha=Alpha.STRAIGHT),
    prepared=Some(_prepared),
    request=Some(precise),
    derives=(timing,),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Emulation", "emulation"]
