"""Video path emulation: JPEG coding, digital corruption, tape playback, analog reception in BT.601 luma and color difference, and a picture tube showing the signal."""

from collections.abc import Mapping
from fractions import Fraction
from itertools import product
from math import ceil, floor
from typing import Final

import bpy
from colour.models import WEIGHTS_YCBCR
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Key
from blender.bpy.nodes.graphs import Component, Expr, math, mixed, rgba, Scalar, switched, vector, where, xyz
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, MenuItem, MenuItems, Socket
from blender.bpy.nodes.textures import gaussian, stream, White
from blender.bpy.render.compositor.colors import from_ycc, ycc
from blender.bpy.render.compositor.coordinates import Border, clocked, extended, origin, pixels, sampled
from blender.bpy.render.compositor.effects import Alpha, CameraFrame, CompositorEffect, precise, Requests, Shell
from blender.bpy.render.outputs.framing import capacity
from blender.imaging.color.processors import Domain
from blender.imaging.faults import ImagingReason
from blender.imaging.files import buffers, coding
from blender.imaging.sampling.integration import Estimate, Precision
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason

from ...chain.stages import Effect, prepared, Stage
from ..optics.procedural import CLOCK

# --- [MODELS] ---------------------------------------------------------------------------


class Compression(msgspec.Struct, frozen=True, kw_only=True):
    """Repeated JPEG generations at a coded picture width, input raster size without one."""

    quality: int
    coded_width: int | None
    subsampling: str
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
            rgb: Outcome[npt.NDArray[np.uint8]] = Option.of_optional(image.get_pixels(OpenImageIO.UINT8)).to_result_with(
                lambda: (Fault(reason=ImagingReason.BUFFER, subject=image.name, cause=image.geterror()),)
            )
            return (
                Result[npt.NDArray[np.uint8], Faults]
                .bind(rgb, lambda codes: coding.generations(np.ascontiguousarray(codes), quality=self.quality, subsampling=self.subsampling, count=self.generations))
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
    staged: IdName | None = None


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PREPARATION]


@prepared.register
def _(
    _record: Emulation, values: Mapping[Key, Value], _config: PyOpenColorIO.Config, source: OpenImageIO.ImageBuf, _precision: Precision, frame: CameraFrame, scene: bpy.types.Scene, /
) -> Outcome[Option[OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf] | Mapping[Key, Value]]]:
    """Accepts the tape canvases of the captured raster and codes the captured display image before the node sections."""

    def fitted(height: int) -> Outcome[None]:
        match values:
            case {"tape": False}:
                return Ok(None)
            case {"lines": int(count), "tape_jitter_period": float(period)}:
                share = Fraction(frame.scale)
                margin = (1 - share) / 2
                lines = Fraction(count)
                first, last = (floor((edge - margin) / share * lines) for edge in (Fraction(1, 2 * height), 1 - Fraction(1, 2 * height)))
                radius = ceil(lines * Fraction(period))
                return accumulated(capacity(scene, size) for size in ((1, 4 * radius + 1), (1, last - first + 1 + 2 * radius))).map(lambda _: None)
            case _:
                return Error((Fault(reason=RecordReason.UNREADABLE, subject=Emulation.__name__, cause=repr(values)),))

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
        case {"compression": False}:
            requested: Outcome[Option[Compression]] = Ok(Nothing)
        case {
            "compression_quality": int(quality),
            "compression_coded_width": bool(sized),
            "compression_coded_width_value": int(width),
            "compression_subsampling": str(chroma),
            "compression_generations": int(count),
        }:
            requested = Ok(Some(Compression(quality=quality, coded_width=width if sized else None, subsampling=chroma, generations=count)))
        case _:
            requested = Error((Fault(reason=RecordReason.UNREADABLE, subject=Compression.__name__, cause=repr(values)),))
    return paired(fitted(source.spec().height), requested.bind(lambda found: found.map(lambda held: compressed(held).map(Some)).default_value(Ok(Nothing)))).map(lambda parts: parts[1])


# --- [GRAPH]


def emulated(shell: Shell[Emulation]) -> Outcome[Expr]:
    """Builds corruption, tape, reception, and display after the codec section, color differences in BT.601 luma weights at unit swing about zero.

    Args:
        shell: Codec-processed image, camera frame, and parameter group inputs.

    Returns:
        Displayed color, or the phosphor layout menu fault.
    """
    source = Option.of_optional(shell.record.staged).map(lambda image: Expr(kind=bpy.types.CompositorNodeImage, members=(("image", image),), socket="Image", shape="RGBA")).default_value(shell.color)
    frame, held = shell.space, shell.input
    scale, seed, lines = held(Shell.frame, "scale"), held("seed"), held("lines")
    red_weight, blue_weight = (float(weight) for weight in WEIGHTS_YCBCR["ITU-R BT.601"])
    weights = (red_weight, 1.0 - red_weight - blue_weight, blue_weight)
    time = clocked(held("clock_speed"), held("clock_hold"))
    pattern = math("FLOOR", time)
    x, y = frame.fraction["X"], frame.fraction["Y"]
    aspect = frame.extent["X"] / frame.extent["Y"]
    line = math("FLOOR", y * lines)
    line_cell, sample_cell = xyz(line), xyz(math("FLOOR", x * math("ROUND", lines * aspect)), line)
    dimensions = Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", source),), socket="Dimensions", shape="VECTOR")
    half_x, half_y = (0.5 / (dimensions[axis] * scale) for axis in ("X", "Y"))

    def white(coordinate: Scalar) -> Expr:
        return White(noise_dimensions="4D").node(coordinate, pattern)

    def blurred(image: Scalar, size: Scalar) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", image), ("Size", size), ("Type", "Gaussian")), socket="Image", shape="RGBA")

    def sample(image: Scalar, position: Expr, across: Border = Border.CLAMP, up: Border = Border.CLAMP) -> Expr:
        return extended(image, sampled(frame, position), (across.item.name, up.item.name), clipped=True)

    def shifted(image: Scalar, horizontal: Scalar, vertical: Scalar = y, across: Border = Border.CLAMP, up: Border = Border.CLAMP) -> Expr:
        return sample(image, xyz(horizontal, vertical), across, up)

    def banded(image: Scalar, horizontal: Scalar, vertical: Scalar) -> Expr:
        return blurred(image, pixels(frame, xyz(horizontal, vertical) * frame.extent, "VECTOR"))

    def fetched(image: Scalar, uv: Scalar | tuple[float, ...]) -> Expr:
        return Expr(
            kind=bpy.types.CompositorNodeMapUV, inputs=(("Image", image), ("UV", uv), ("Interpolation", "Nearest"), ("Extension X", "Clip"), ("Extension Y", "Clip")), socket="Image", shape="RGBA"
        )

    def drawn(key: str, lattice: Scalar | tuple[float, ...]) -> Expr:
        return white(vector("ADD", lattice, origin(seed, stream(Emulation, key))))

    def normals(key: str, lattice: Scalar) -> tuple[Expr, Expr, Expr, Expr]:
        axes = vector("ADD", lattice, origin(seed, stream(Emulation, key)))
        first, second, third, fourth = (white(xyz(axes["X"], axes["Y"], index)) for index in range(4))
        return (*gaussian(first, second, Expr), *gaussian(third, fourth, Expr))

    def corrupted(image: Expr) -> Expr:
        cells = drawn("coverage", vector("FLOOR", vector("DIVIDE", frame.point, xyz(held("corruption_cell_width"), held("corruption_cell_height"), 1.0))))
        tone = msgspec.structs.replace(cells, socket="Color", shape="RGBA")
        struck = cells < held("corruption_coverage")
        cell_x = x + math("MULTIPLY_ADD", tone["Red"], 2.0, -1.0) * held("corruption_displacement") * struck
        channels: tuple[Component, ...] = ("Red", "Green", "Blue")
        red, green, blue = (
            shifted(image, math("MULTIPLY_ADD", held("corruption_split"), direction, cell_x), across=Border.PERIODIC, up=Border.BLACK)[channel]
            for channel, direction in zip(channels, (-1.0, 0.0, 1.0))
        )
        luma, blue_difference, red_difference = ycc(rgba(red, green, blue), weights)
        power = 2.0 ** math("MULTIPLY_ADD", tone["Green"], 4.0, -2.0)

        def powered(difference: Expr) -> Expr:
            return mixed(held("corruption_chroma") * struck, difference, math("SIGN", difference) * (abs(difference) * 2.0) ** power * 0.5)

        return switched(held("corruption"), image, from_ycc(luma, powered(blue_difference), powered(red_difference), weights))

    def played(image: Expr) -> Expr:
        margin = math("MULTIPLY_ADD", scale, -0.5, 0.5)
        half = 0.5 / dimensions["Y"]
        first, last = (math("FLOOR", (edge - margin) / scale * lines) for edge in (half, 1.0 - half))
        radius = held("tape_jitter_period") * lines
        support = math("CEIL", radius)
        padding = support * 2.0
        height = last - first + 1.0 + padding
        jitter_canvas, impulse_canvas = (
            Expr(kind=bpy.types.CompositorNodeBlankImage, inputs=(("Size", xyz(1.0, extent)),), socket="Image", shape="RGBA") for extent in (height, math("MULTIPLY_ADD", support, 4.0, 1.0))
        )
        jitter_row, impulse_row = (Expr(kind=bpy.types.CompositorNodeImageCoordinates, inputs=(("Image", canvas),), socket="Pixel", shape="VECTOR")["Y"] for canvas in (jitter_canvas, impulse_canvas))
        filtered = xyz(0.0, radius)
        jittered = blurred(normals("jitter", xyz(jitter_row + (first - support)))[0], filtered)
        impulse = fetched(blurred(blurred(math("COMPARE", impulse_row, padding, 0.0), filtered), filtered), (0.5, 0.5, 1.0))
        normalized = jittered["Red"] / math("SQRT", impulse["Red"])
        jitter_shift = math("MULTIPLY", fetched(normalized, xyz(0.5, (line - first + support + 0.5) / height, 1.0)), held("tape_jitter"))
        bounce_shift = math("MULTIPLY_ADD", drawn("bounce", (0.0, 0.0, 0.0)), 2.0, -1.0) * held("tape_bounce")
        head = y / held("tape_head_switching")
        head_shift = math("MINIMUM", head / 0.9, math("MULTIPLY_ADD", head, -10.0, 10.0)) * ((1.0 - (y < 0.0)) * (y < held("tape_head_switching"))) * held("tape_skew")
        tracking = math("MULTIPLY_ADD", abs(math("WRAP", y - math("FRACT", time * held("tape_tracking_rate")), 0.5, -0.5)) / held("tape_tracking_height"), -2.0, 1.0, clamp=True)
        picture = shifted(image, x + jitter_shift + head_shift + tracking * held("tape_tracking_shift"), y + bounce_shift, up=Border.BLACK)
        luma, blue_difference, red_difference = ycc(picture, weights)
        mistracked = from_ycc(mixed(tracking * held("tape_mistracking"), luma, drawn("mistracking", line_cell)), blue_difference, red_difference, weights)
        position = x / held("tape_dropout_length")
        struck = drawn("dropouts", xyz(math("FLOOR", position), line)) < held("tape_dropouts")
        return switched(held("tape"), image, mixed((1.0 - math("FRACT", position)) * struck, mistracked, (1.0, 1.0, 1.0)))

    def received(image: Expr) -> Expr:
        luma, blue_difference, red_difference = ycc(image, weights)
        luma_noise, chroma_noises = normals("noise", sample_cell), normals("chroma_noise", sample_cell)
        strength = math("SUBTRACT", luma, banded(luma, 1.0 / 377.0, 0.0)) * held("signal_cross_color")
        phase = math("MULTIPLY_ADD", x, 188.3, (line + pattern) * 0.5) * 2.0 * np.pi
        green, blue = (
            math("MULTIPLY_ADD", noise, held("signal_chroma_noise"), math("MULTIPLY_ADD", strength, wave, difference))
            for difference, wave, noise in ((blue_difference, math("COSINE", phase), chroma_noises[0]), (red_difference, math("SINE", phase), chroma_noises[1]))
        )
        band = banded(math("MULTIPLY_ADD", luma_noise[0], held("signal_noise"), luma), held("signal_luma_blur"), 0.0)
        chroma = shifted(banded(rgba(green, blue, 0.0), held("signal_chroma_blur"), held("signal_chroma_vertical")), x - held("signal_chroma_delay"))
        ringed = (shifted(band, math("MULTIPLY_ADD", held("signal_luma_blur") / 3.0, direction, x)) for direction in (-1.0, 1.0))
        overshoot = -math("MULTIPLY_ADD", band, -2.0, math("ADD", *ringed))
        packed = rgba(math("MULTIPLY_ADD", overshoot, held("signal_ringing"), band), chroma["Red"] * held("signal_saturation"), chroma["Green"] * held("signal_saturation"))
        amplitude = held("signal_ghost_amplitude")
        echoed = switched(held("signal_ghost"), packed, mixed(amplitude / (amplitude + 1.0), packed, shifted(packed, x - held("signal_ghost_delay"))))
        loss = math("MULTIPLY_ADD", math("SINE", math("MULTIPLY_ADD", time, -0.002, y) * 2.0 * np.pi), 0.5, 0.5) * held("signal_hum")
        return switched(held("signal"), image, from_ycc(echoed["Red"] * (1.0 - loss), echoed["Green"], echoed["Blue"], weights))

    def displayed(image: Expr, geometry: Expr) -> Expr:
        rolled = shifted(image, x, math("MULTIPLY_ADD", held("display_roll"), time, y), up=Border.PERIODIC)
        least = 1.0 / lines
        lowered = math("MULTIPLY_ADD", held("display_collapse"), -2.0, 1.0)
        height, width = math("MAXIMUM", lowered, least), math("MINIMUM", math("MAXIMUM", lowered + 1.0, least), 1.0)
        at = vector("ADD", vector("DIVIDE", vector("SUBTRACT", frame.fraction, (0.5, 0.5, 0.0)), xyz(width, height, 1.0)), (0.5, 0.5, 0.0))
        collapsed = vector("DIVIDE", sample(rolled, at, Border.BLACK, Border.BLACK), width * height)
        radius = pixels(frame, xyz(held("display_glow_radius"), held("display_glow_radius")), "VECTOR")
        near = blurred(collapsed, radius)
        glowed = switched(held("display_glow"), collapsed, mixed(0.890909, blurred(near, radius * (4.0, 1.0, 0.0)), near))
        field = math("PINGPONG", pattern, 1.0)

        def scanned_row(sign: float) -> Expr:
            raster = ((math("MULTIPLY_ADD", half_y, sign, y) - 0.5) / height + 0.5) * lines
            return switched(held("display_scanlines_interlaced"), raster, (raster + field) * 0.5)

        low, high = scanned_row(-1.0), scanned_row(1.0)
        tau = 2.0 * np.pi
        strength = held("display_scanlines_strength")
        light = 1.0 - ((math("SINE", high * tau) - math("SINE", low * tau)) / ((high - low) * (2.0 * tau)) + 0.5) * strength
        scanned = switched(held("display_scanlines"), glowed, vector("SCALE", glowed, light / math("MULTIPLY_ADD", strength, -0.5, 1.0)))
        fill = 1.0 - geometry["X"]
        triads = held("display_phosphors_triads")
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
        mask = held("display_phosphors_strength")

        def gained(index: int) -> Expr:
            lit = {(row, column): covered("x", row, 1.0 / 3.0, column + index / 3.0, 1.0 / 3.0) * cell for (row, column), cell in matrix.items()}
            integral = lit[0, 0] + lit[0, 1] + (lit[1, 0] + lit[1, 1])
            return math("MULTIPLY_ADD", integral / area / fill * 3.0, mask, 1.0 - mask)

        masked = switched(held("display_phosphors"), scanned, vector("MULTIPLY", scanned, rgba(gained(0), gained(1), gained(2))))
        bulge = held("display_curvature")
        offset = frame.point * (2.0, 2.0, 0.0) - frame.extent
        stretched = vector("SCALE", offset, math("SQRT", bulge) * 2.0 / (bulge + 1.0))
        real = 1.0 - (vector("LENGTH", stretched) > 1.0)
        lens = vector(
            "MULTIPLY_ADD",
            vector("SCALE", offset / frame.extent, scale),
            where(real, 1.0 / ((bulge + 1.0) * (vector("LENGTH", vector("ADD", stretched, (0.0, 0.0, 1.0))) + 1.0)), 0.0),
            xyz(0.5, 0.5, real),
        )
        return switched(held("display"), image, extended(masked, lens, (Border.BLACK.item.name, Border.BLACK.item.name), clipped=True))

    return shell.menu("display_phosphors_layout", cases={"Aperture Grille": (0.0, 0.0, 0.0), "Slot Mask": (1.0 / 3.0, 0.5, 0.0), "Shadow Mask": (1.0 / 3.0, 0.0, 0.5)}, kind="VECTOR").map(
        lambda geometry: displayed(received(played(corrupted(source))), geometry)
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

emulation: Final = CompositorEffect(
    record=Emulation,
    graph=emulated,
    interface=Interface(
        inputs=(
            Socket(key="lines", name="Lines", kind=bpy.types.NodeSocketInt, description="Raster lines across frame height", members={"min_value": 1, "default_value": 480}),
            Socket(key="seed", name="Seed", kind=bpy.types.NodeSocketInt, description="Number selecting every random pattern", members={"min_value": 0, "max_value": 2**24}),
        ),
        panels=(
            *(
                InterfacePanel(
                    key=key,
                    name=bpy.path.display_name(key),
                    description=summary,
                    toggle=Some(enabled),
                    inputs=(
                        *lead,
                        *(
                            Socket(key=item_key(key, field), name=bpy.path.display_name(field), kind=kind, description=description, members={"subtype": subtype, **members})
                            for field, kind, subtype, description, members in rows
                        ),
                    ),
                    panels=panels,
                )
                for key, summary, enabled, lead, rows, panels in (
                    (
                        "compression",
                        "JPEG picture block coding",
                        False,
                        (
                            Socket(
                                key="compression_subsampling",
                                name="Subsampling",
                                kind=bpy.types.NodeSocketMenu,
                                description="Chroma resolution relative to luma",
                                members={"default_value": "4:2:0"},
                                items=Some(MenuItems(items=(MenuItem(name="4:4:4"), MenuItem(name="4:2:2"), MenuItem(name="4:2:0"), MenuItem(name="4:1:1")))),
                            ),
                        ),
                        (
                            ("quality", bpy.types.NodeSocketInt, "NONE", "JPEG quality on IJG scale", {"min_value": 1, "max_value": 100, "default_value": 60}),
                            ("generations", bpy.types.NodeSocketInt, "NONE", "JPEG encode and decode cycles", {"min_value": 1, "default_value": 1}),
                        ),
                        (
                            InterfacePanel(
                                key="compression_coded_width",
                                name="Coded Width",
                                description="Picture width before coding, input raster size when off",
                                toggle=Some(False),
                                inputs=(Socket(key="compression_coded_width_value", name="Coded Width", kind=bpy.types.NodeSocketInt, members={"min_value": 1, "default_value": 640}),),
                            ),
                        ),
                    ),
                    (
                        "corruption",
                        "Rectangular buffer corruption with a frame-wide RGB split",
                        False,
                        (),
                        (
                            ("coverage", bpy.types.NodeSocketFloat, "FACTOR", "Share of corrupted cells per pattern", {"min_value": 0.0, "max_value": 1.0, "default_value": 0.2}),
                            ("cell_width", bpy.types.NodeSocketFloat, "NONE", "Corrupted cell width over greater frame side", {"min_value": 1e-6, "default_value": 0.03}),
                            ("cell_height", bpy.types.NodeSocketFloat, "NONE", "Corrupted cell height over greater frame side", {"min_value": 1e-6, "default_value": 0.03}),
                            ("displacement", bpy.types.NodeSocketFloat, "NONE", "Greatest horizontal cell shift as a frame share", {"min_value": 0.0, "default_value": 0.004}),
                            ("split", bpy.types.NodeSocketFloat, "NONE", "Opposing red and blue offsets across frame as a frame share", {"min_value": 0.0}),
                            ("chroma", bpy.types.NodeSocketFloat, "FACTOR", "Sign-preserving chroma power in struck cells", {"min_value": 0.0, "max_value": 1.0, "default_value": 0.6}),
                        ),
                        (),
                    ),
                    (
                        "tape",
                        "Time-base error, head switching, tracking disturbance, and dropouts",
                        False,
                        (),
                        (
                            ("jitter", bpy.types.NodeSocketFloat, "NONE", "Standard deviation of horizontal line displacement as a frame share", {"min_value": 0.0, "default_value": 0.002}),
                            ("jitter_period", bpy.types.NodeSocketFloat, "NONE", "Vertical jitter smoothing period as a frame share", {"min_value": 0.0, "default_value": 1.0 / 65.0}),
                            ("bounce", bpy.types.NodeSocketFloat, "NONE", "Greatest vertical frame shift as a frame share", {"min_value": 0.0}),
                            ("head_switching", bpy.types.NodeSocketFloat, "NONE", "Skewed bottom band height as a frame share", {"min_value": 0.0, "default_value": 0.0176}),
                            ("skew", bpy.types.NodeSocketFloat, "NONE", "Peak horizontal head-switching shift at nine tenths of band", {"default_value": 0.05}),
                            ("tracking_shift", bpy.types.NodeSocketFloat, "NONE", "Shift at tracking band center", {}),
                            ("tracking_height", bpy.types.NodeSocketFloat, "NONE", "Tracking band height as a frame share", {"min_value": 1e-6, "default_value": 0.15}),
                            ("tracking_rate", bpy.types.NodeSocketFloat, "NONE", "Frame heights traveled upward per clock frame", {"min_value": 0.0}),
                            ("mistracking", bpy.types.NodeSocketFloat, "FACTOR", "Luma noise in tracking band", {"min_value": 0.0, "max_value": 1.0}),
                            ("dropouts", bpy.types.NodeSocketFloat, "FACTOR", "Share of dropout-length cells struck per raster line", {"min_value": 0.0, "max_value": 1.0, "default_value": 0.02}),
                            ("dropout_length", bpy.types.NodeSocketFloat, "NONE", "Fading dropout cell length as a frame share", {"min_value": 1e-6, "default_value": 0.04}),
                        ),
                        (),
                    ),
                    (
                        "signal",
                        "Analog video bandwidth, Gaussian noise, and carrier disturbances",
                        True,
                        (),
                        (
                            ("luma_blur", bpy.types.NodeSocketFloat, "NONE", "Horizontal luma bandwidth blur as a frame share", {"min_value": 0.0, "default_value": 0.0023}),
                            ("chroma_blur", bpy.types.NodeSocketFloat, "NONE", "Horizontal color-difference bandwidth blur as a frame share", {"min_value": 0.0, "default_value": 0.0073}),
                            ("chroma_vertical", bpy.types.NodeSocketFloat, "NONE", "Vertical color-difference blur as a frame share", {"min_value": 0.0}),
                            ("chroma_delay", bpy.types.NodeSocketFloat, "NONE", "Color-difference offset from luma, right above zero", {}),
                            ("saturation", bpy.types.NodeSocketFloat, "NONE", "Color-difference gain", {"min_value": 0.0, "default_value": 1.0}),
                            ("ringing", bpy.types.NodeSocketFloat, "FACTOR", "Second-difference overshoot at luma edges", {"min_value": 0.0, "max_value": 1.0, "default_value": 0.1}),
                            ("cross_color", bpy.types.NodeSocketFloat, "FACTOR", "Fine luma detail decoded as NTSC subcarrier color", {"min_value": 0.0, "max_value": 1.0}),
                            ("noise", bpy.types.NodeSocketFloat, "NONE", "Luma noise standard deviation per raster sample", {"min_value": 0.0, "default_value": 0.02}),
                            ("chroma_noise", bpy.types.NodeSocketFloat, "NONE", "Color-difference noise standard deviation per raster sample", {"min_value": 0.0, "default_value": 0.03}),
                            ("hum", bpy.types.NodeSocketFloat, "FACTOR", "Depth of mains hum bar drifting upward", {"min_value": 0.0, "max_value": 1.0}),
                        ),
                        (
                            InterfacePanel(
                                key="signal_ghost",
                                name="Ghost",
                                description="Multipath echo of whole signal",
                                toggle=Some(False),
                                inputs=(
                                    Socket(
                                        key="signal_ghost_amplitude",
                                        name="Amplitude",
                                        kind=bpy.types.NodeSocketFloat,
                                        description="Delayed echo strength",
                                        members={"subtype": "FACTOR", "min_value": 0.0, "max_value": 1.0, "default_value": 0.1},
                                    ),
                                    Socket(
                                        key="signal_ghost_delay", name="Delay", kind=bpy.types.NodeSocketFloat, description="Horizontal echo offset, right above zero", members={"default_value": 0.05}
                                    ),
                                ),
                            ),
                        ),
                    ),
                    (
                        "display",
                        "Picture tube showing a collapsed, glowing, scanned, and masked signal",
                        True,
                        (),
                        (
                            ("curvature", bpy.types.NodeSocketFloat, "NONE", "Screen glass barrel bulge", {"min_value": 0.0, "default_value": 0.03}),
                            ("roll", bpy.types.NodeSocketFloat, "NONE", "Frame heights rolled per clock frame without vertical hold", {}),
                            ("collapse", bpy.types.NodeSocketFloat, "FACTOR", "Power-off collapse to line, then dot", {"min_value": 0.0, "max_value": 1.0}),
                        ),
                        (
                            InterfacePanel(
                                key="display_glow",
                                name="Glow",
                                description="Linear screen-glass scattering before scanlines and phosphors",
                                toggle=Some(False),
                                inputs=(
                                    Socket(
                                        key="display_glow_radius", name="Radius", kind=bpy.types.NodeSocketFloat, description="Gaussian glass blur", members={"min_value": 0.0, "default_value": 0.01}
                                    ),
                                ),
                            ),
                            InterfacePanel(
                                key="display_scanlines",
                                name="Scanlines",
                                description="Raster lines the beam draws",
                                toggle=Some(True),
                                inputs=(
                                    Socket(
                                        key="display_scanlines_strength",
                                        name="Strength",
                                        kind=bpy.types.NodeSocketFloat,
                                        description="Darkening between raster lines before mean normalization",
                                        members={"subtype": "FACTOR", "min_value": 0.0, "max_value": 1.0, "default_value": 0.4},
                                    ),
                                    Socket(key="display_scanlines_interlaced", name="Interlaced", kind=bpy.types.NodeSocketBool, description="Light alternate raster fields per pattern"),
                                ),
                            ),
                            InterfacePanel(
                                key="display_phosphors",
                                name="Phosphors",
                                description="Phosphor mask on glass",
                                toggle=Some(False),
                                inputs=(
                                    Socket(
                                        key="display_phosphors_layout",
                                        name="Layout",
                                        kind=bpy.types.NodeSocketMenu,
                                        description="Phosphor arrangement",
                                        members={"default_value": "Aperture Grille"},
                                        items=Some(MenuItems(items=(MenuItem(name="Aperture Grille"), MenuItem(name="Slot Mask"), MenuItem(name="Shadow Mask")))),
                                    ),
                                    Socket(
                                        key="display_phosphors_triads",
                                        name="Triads",
                                        kind=bpy.types.NodeSocketInt,
                                        description="Phosphor triads across frame width",
                                        members={"min_value": 1, "default_value": 640},
                                    ),
                                    Socket(
                                        key="display_phosphors_strength",
                                        name="Strength",
                                        kind=bpy.types.NodeSocketFloat,
                                        description="Blend toward mean-one phosphor mask",
                                        members={"subtype": "FACTOR", "min_value": 0.0, "max_value": 1.0, "default_value": 0.705394},
                                    ),
                                ),
                            ),
                        ),
                    ),
                )
            ),
            CLOCK,
        ),
    ),
    domain=Domain.DISPLAY,
    alpha=Alpha.STRAIGHT,
    parts=(Requests[Emulation](request=precise),),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Compression", "Emulation", "emulation"]
