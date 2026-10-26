"""Tone operators from scene-linear to display-linear: per-channel curves over rendering primaries, PBR Neutral, Photoreceptor, Hable, and Tony McMapface."""

from collections.abc import Mapping
from contextlib import ExitStack
from importlib.resources import as_file, files
from typing import Annotated, override, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Operator, Outcome, Unchanged
from blender.bpy.data.rna.conditions import Job
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.bpy.nodes.graphs import Expr, grouped, math, mixed, NodeGroup, RESULT, switched, vector, where, xyz
from blender.bpy.nodes.interface import item_key
from blender.bpy.render.compositor import colors
from blender.bpy.render.compositor.colors import AXES, composed, constant, directed, extreme, inverted, nonnegative, preserved, spanned
from blender.bpy.render.compositor.effects import Alpha, CameraFrame, CompositorEffect, Measures, Named, Prepared, Prepares, Registers, Shell, Spaces
from blender.bpy.render.compositor.tables import tabled, TILES
from blender.bpy.render.compositor.viewers import measuring
from blender.bpy.settings.shapes import GraphForm
from blender.imaging.color.luts import Lattice, Rational, split
from blender.imaging.color.processors import ColorSpace, GREY, REC709_SCENE, rendering_basis, Stops
from blender.imaging.files.images import pixels
from blender.imaging.sampling.integration import Precision
from blender.records.faults import DomainFault, FileFault
from blender.records.fields import Case, Finite, Gated, Positive, Presentation, Unit

from ...chain.stages import Effect, Stage

# --- [TYPES] ----------------------------------------------------------------------------

type DisplayWhite = Annotated[float, msgspec.Meta(ge=0.2, le=16.0), Presentation(soft=Some((0.5, 1.0)), precision=Some(3))]
type Curve = Sigmoid | PiecewisePower | Logarithmic | ReinhardExtended

# --- [MODELS] ---------------------------------------------------------------------------


class Sigmoid(Case, frozen=True, kw_only=True):
    """Log-logistic response pinning scene zero to black and middle gray to itself."""

    contrast: Annotated[float, msgspec.Meta(ge=0.1, le=10.0, description="Slope at middle gray, setting compressed scene range"), Presentation(soft=Some((0.7, 3.0)), precision=Some(3))] = 1.5
    skew: Annotated[float, msgspec.Meta(ge=-1.0, le=1.0, description="Contrast moved into shadows below 0 and highlights above 0"), Presentation(precision=Some(3))] = 0.0
    display_white: Annotated[DisplayWhite, msgspec.Meta(description="Display value approached at infinity, 1 at reference white")] = 1.0
    display_black: Annotated[float, msgspec.Meta(ge=0.0, le=0.15, description="Display value at scene zero"), Presentation(soft=Some((0.0, 0.01)), precision=Some(6))] = 0.000152


class PiecewisePower(Case, frozen=True, kw_only=True):
    """Hable's toe, linear, and shoulder segments normalized at their white point."""

    toe_strength: Annotated[Unit, msgspec.Meta(description="Toe depth below linear segment"), Presentation(precision=Some(3))] = 0.5
    toe_length: Annotated[Unit, msgspec.Meta(description="Lower range the toe spans, applied as its 2.2 power"), Presentation(precision=Some(3))] = 0.5
    shoulder_strength: Annotated[
        float, msgspec.Meta(ge=0.0, le=16.0, description="Stops of highlight range the shoulder holds above white"), Presentation(soft=Some((0.0, 10.0)), step=Some(0.01), precision=Some(3))
    ] = 2.0
    shoulder_length: Annotated[float, msgspec.Meta(ge=1e-5, le=1.0, description="Upper range the shoulder spans"), Presentation(precision=Some(3))] = 0.5
    shoulder_angle: Annotated[Unit, msgspec.Meta(description="Shoulder overshoot past white"), Presentation(precision=Some(3))] = 1.0
    gamma: Annotated[float, msgspec.Meta(ge=0.1, le=10.0, description="Linear segment power"), Presentation(soft=Some((0.5, 2.0)), precision=Some(3))] = 1.0


class Logarithmic(Case, frozen=True, kw_only=True):
    """Drago's adaptive logarithm mapping scene white to display white."""

    bias: Annotated[float, msgspec.Meta(ge=0.5, le=1.0, description="Logarithm base shape between 2 at black and 10 at scene white"), Presentation(soft=Some((0.7, 0.9)), precision=Some(2))] = 0.85
    scene_white: Annotated[Gated[Stops], msgspec.Meta(description="Manual white in stops from 1.0, read from frame when off")] = Gated(enabled=False, value=float(np.log2(100.0)))
    display_white: Annotated[DisplayWhite, msgspec.Meta(description="Display value at scene white, 1 at reference white")] = 1.0


class ReinhardExtended(Case, frozen=True, kw_only=True):
    """Reinhard's photographic response with a finite scene white mapping to one."""

    scene_white: Annotated[Gated[Stops], msgspec.Meta(description="Manual white in stops from 1.0, read from frame when off")] = Gated(enabled=False, value=0.0)


class RenderingPrimary(msgspec.Struct, frozen=True, kw_only=True):
    """Primary inset and rotated within base gamut's angular limit."""

    inset: Annotated[float, msgspec.Meta(ge=0.0, le=0.99, description="Share of distance from base gamut edge toward white"), Presentation(soft=Some((0.0, 0.5)), precision=Some(3))] = 0.0
    rotation: Annotated[float, msgspec.Meta(gt=-1.0, lt=1.0, description="Share of base gamut rotation limit about white"), Presentation(precision=Some(3))] = 0.0


class Application(msgspec.Struct, frozen=True, kw_only=True):
    """Per-channel curve over rendering primaries with hue and energy preservation."""

    preserve_hue: Annotated[Unit, msgspec.Meta(description="Share of hue twist corrected, channel sum kept")] = 1.0
    red: Annotated[RenderingPrimary, msgspec.Meta(description="Red rendering primary")] = RenderingPrimary()
    green: Annotated[RenderingPrimary, msgspec.Meta(description="Green rendering primary")] = RenderingPrimary()
    blue: Annotated[RenderingPrimary, msgspec.Meta(description="Blue rendering primary")] = RenderingPrimary()
    purity: Annotated[Unit, msgspec.Meta(title="Recover Purity", description="Share of each inset the return through outset undoes")] = 0.0


class ToneCurve(Effect, frozen=True, kw_only=True):
    """Scene-linear to display-linear curve applied per rendering channel."""

    stage = Stage.TONE
    curve: Annotated[Curve, msgspec.Meta(description="Curve each rendering channel takes")] = Sigmoid()
    space: Annotated[ColorSpace, msgspec.Meta(title="Base Primaries", description="Colorspace rendering primaries move from"), GraphForm.COLOR_SPACE] = PyOpenColorIO.ROLE_SCENE_LINEAR
    application: Annotated[Application, msgspec.Meta(description="Rendering primaries and hue handling around curve")] = Application()

    @property
    @override
    def pointwise(self) -> bool:
        """Whether the selected curve uses a manual white or no frame adaptation."""
        match self.curve:
            case Logarithmic(scene_white=white) | ReinhardExtended(scene_white=white):
                return white.enabled
            case Sigmoid() | PiecewisePower():
                return True

    def whitened(self, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32]) -> BlenderResult[Option[Self]]:
        """Returns the record with its curve's scene white read from covered scene-linear pixels, Nothing for a curve holding no scene white."""
        match self.curve:
            case Logarithmic() | ReinhardExtended() as curve:
                return colors.whitened(config, plane).bind(lambda white: metered(curve, white)).map(lambda resolved: Some(msgspec.structs.replace(self, curve=resolved)))
            case Sigmoid() | PiecewisePower():
                return Ok(Nothing)


class Neutral(Effect, frozen=True, kw_only=True):
    """Khronos PBR Neutral preserving base colors and compressing highlights toward white."""

    stage = Stage.TONE
    title = "PBR Neutral"
    pointwise = True


class Photoreceptor(Effect, frozen=True, kw_only=True):
    """Reinhard and Devlin's response against frame adaptation level."""

    stage = Stage.TONE
    pointwise = False
    intensity: Annotated[Finite, msgspec.Meta(description="Brightness, scaling adaptation by e to the negative intensity"), Presentation(soft=Some((-8.0, 8.0)), precision=Some(3))] = 0.0
    contrast: Annotated[Gated[Positive], msgspec.Meta(description="Response exponent, read from frame luminance range when off"), Presentation(soft=Some((0.3, 1.0)), precision=Some(3))] = Gated(
        enabled=False, value=1.0
    )
    light_adaptation: Annotated[Unit, msgspec.Meta(description="Blend from frame adaptation at 0 to pixel adaptation at 1")] = 0.0
    chromatic_adaptation: Annotated[Unit, msgspec.Meta(description="Blend from luminance at 0 to each channel at 1")] = 0.0


class Hable(Effect, frozen=True, kw_only=True):
    """White-normalized Uncharted 2 response with independent authored coefficients, a manual white holding nonzero contrast."""

    stage = Stage.TONE
    a: Annotated[float, msgspec.Meta(ge=0.001, le=0.5, description="Shoulder strength"), Presentation(step=Some(0.01), precision=Some(3))] = 0.15
    b: Annotated[float, msgspec.Meta(ge=0.0, le=0.6, description="Linear strength"), Presentation(step=Some(0.01), precision=Some(3))] = 0.5
    c: Annotated[float, msgspec.Meta(ge=0.0, le=0.3, description="Linear angle"), Presentation(step=Some(0.01), precision=Some(3))] = 0.1
    d: Annotated[float, msgspec.Meta(gt=0.0, le=0.9, description="Toe strength"), Presentation(step=Some(0.01), precision=Some(3))] = 0.2
    e: Annotated[float, msgspec.Meta(ge=0.0001, le=0.05, description="Toe numerator"), Presentation(step=Some(0.001), precision=Some(4))] = 0.02
    f: Annotated[float, msgspec.Meta(ge=0.2, le=0.6, description="Toe denominator"), Presentation(step=Some(0.01), precision=Some(3))] = 0.3
    scene_white: Annotated[Gated[Stops], msgspec.Meta(description="Manual white in stops from 1.0, read from frame when off")] = Gated(enabled=True, value=float(np.log2(11.2)))

    def __post_init__(self) -> None:
        """Refuses a manual white at which the normalized slope and intercept share a zero denominator."""
        if self.scene_white.enabled and 2.0**self.scene_white.value * self.a * (self.f - self.e) + self.b * (self.c * self.f - self.e) == 0.0:
            raise ValueError(f"Scene white {self.scene_white.value} stops leaves Hable normalization without contrast")

    @property
    @override
    def pointwise(self) -> bool:
        """Whether white normalization uses the manual white."""
        return self.scene_white.enabled

    def whitened(self, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32]) -> BlenderResult[Option[Self]]:
        """Returns the record with its scene white read from covered scene-linear pixels."""
        return colors.whitened(config, plane).bind(lambda white: metered(self, white)).map(Some)


class Tony(Effect, frozen=True, kw_only=True):
    """Tony McMapface's rationally shaped RGB table in linear Rec.709."""

    stage = Stage.TONE
    title = "Tony McMapface"
    pointwise = True


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CURVES]


@grouped(item_key("tone", Sigmoid), Sigmoid.title)
def sigmoid(value: Expr, contrast: Expr, skew: Expr, display_white: Expr, display_black: Expr) -> Expr:
    """Log-logistic response of one channel through middle gray, black at scene zero."""
    skewed = 0.2**skew
    inverse = 1.0 / skewed
    grey = (GREY / display_white) ** inverse
    relation = (1.0 - grey) / grey
    film = contrast * (1.0 - GREY) / (skewed * (1.0 - grey))
    floor = (display_black / display_white) ** inverse
    root = (relation * floor / (1.0 - floor)) ** (1.0 / film)
    fog = root * GREY / (1.0 - root)
    relative = (fog + math("MAXIMUM", value, 0.0)) / (fog + GREY)
    rising = relative**film
    ratio = where(relative > 1.0, 1.0 / math("MULTIPLY_ADD", relation, relative ** (film * -1.0), 1.0), rising / (relation + rising))
    return ratio**skewed * display_white


@grouped(item_key("tone", PiecewisePower), PiecewisePower.title)
def piecewise(value: Expr, toe_strength: Expr, toe_length: Expr, shoulder_strength: Expr, shoulder_length: Expr, shoulder_angle: Expr, gamma: Expr) -> Expr:
    """Toe, linear, and shoulder segments of one channel normalized at their white point."""
    toe_end = toe_length**2.2 * 0.5
    toe_height = (1.0 - toe_strength) * toe_end
    span = (1.0 - shoulder_length) * (1.0 - toe_height)
    white = toe_end + (1.0 - toe_height) + (2.0**shoulder_strength - 1.0)
    overshoot = shoulder_angle * shoulder_strength
    normalized, toe_x, shoulder_x = value / white, toe_end / white, (toe_end + span) / white
    line = math("MULTIPLY_ADD", white, normalized, toe_height - toe_end) ** gamma
    toe_level, shoulder_level = (math("MAXIMUM", height**gamma, 1e-5) for height in (toe_height, toe_height + span))
    toe_slope, shoulder_slope = (gamma * white * level ** (1.0 - 1.0 / gamma) for level in (toe_level, shoulder_level))
    ceiling = (overshoot * 0.5 + 1.0) ** gamma
    toe = (normalized / toe_x) ** (toe_slope * toe_x / toe_level) * toe_level
    origin = overshoot * 2.0 + 1.0
    run, rise = origin - shoulder_x, ceiling - shoulder_level
    exponent = shoulder_slope * run / rise

    def shoulder(position: Expr | float) -> Expr:
        return ceiling - ((origin - position) / run) ** exponent * rise

    shaped = where(normalized > 0.0, where(normalized < toe_x, toe, where(normalized < shoulder_x, line, where(normalized < origin, shoulder(normalized), ceiling))), 0.0)
    return shaped / shoulder(1.0)


@grouped(item_key("tone", Logarithmic), Logarithmic.title)
def logarithmic(value: Expr, bias: Expr, scene_white: Expr, display_white: Expr) -> Expr:
    """Drago's adaptive logarithm of one channel, scene white at display white."""
    level, clipped = 2.0**scene_white, math("MAXIMUM", value, 0.0)

    def natural(operand: Expr) -> Expr:
        lifted = operand + 1.0
        delta = lifted - 1.0
        return where(delta > 0.0, math("LOGARITHM", lifted, np.e) * (operand / delta), operand)

    base = math("MULTIPLY_ADD", (clipped / level) ** math("LOGARITHM", bias, 0.5), 8.0, 2.0)
    return natural(clipped) / (natural(level) / float(np.log(10.0)) * math("LOGARITHM", base, np.e)) * display_white


@grouped(item_key("tone", ReinhardExtended), ReinhardExtended.title)
def reinhard(value: Expr, scene_white: Expr) -> Expr:
    """Reinhard's photographic response of one channel, scene white at one."""
    return value / (value + 1.0) * (value / 2.0 ** (scene_white * 2.0) + 1.0)


# --- [GRAPHS]


def curved(shell: Shell[ToneCurve]) -> BlenderResult[Expr]:
    """Applies the live per-channel curve between rendering primaries inset and rotated within the base gamut."""
    curve, _, application = ToneCurve.__struct_fields__
    preserve_hue, *channels, purity = Application.__struct_fields__
    inset, rotation = RenderingPrimary.__struct_fields__
    shapes: Mapping[type[Curve], NodeGroup] = {Sigmoid: sigmoid.group, PiecewisePower: piecewise.group, Logarithmic: logarithmic.group, ReinhardExtended: reinhard.group}

    def body(basis: npt.NDArray[np.float64]) -> Expr:
        normalized = basis / basis[1].sum()
        white_xyz = normalized.sum(axis=1)
        white = white_xyz[:2] / white_xyz.sum()
        offsets = (basis / basis.sum(axis=0))[:2].T - white
        edges = np.roll(offsets, -1, axis=0) - offsets
        normals, spans = edges[:, ::-1] * (1.0, -1.0), offsets[:, 0] * edges[:, 1] - offsets[:, 1] * edges[:, 0]
        angles = np.sort(np.arctan2(offsets[:, 1], offsets[:, 0]))
        limit = float((np.pi - np.diff(np.append(angles, angles[0] + 2.0 * np.pi)).max()) / 2.0)
        planar = (*map(float, white), 0.0)

        def primary(offset: npt.NDArray[np.float64], key: str) -> tuple[Expr, Expr]:
            turned = Expr(
                kind=bpy.types.ShaderNodeVectorRotate,
                inputs=(("Vector", (*map(float, offset), 0.0)), ("Angle", shell.input(application, key, rotation) * limit)),
                members=(("rotation_type", "Z_AXIS"),),
                socket="Vector",
                shape="VECTOR",
            )
            first, second, third = (vector("DOT_PRODUCT", turned, (*map(float, normal), 0.0)) / float(span) for normal, span in zip(normals, spans))
            edge = vector("MULTIPLY_ADD", turned, 1.0 / math("MAXIMUM", math("MAXIMUM", first, second), third), planar)
            share = shell.input(application, key, inset)
            return mixed(1.0 - share, planar, edge), mixed(1.0 - shell.input(application, purity) * share, planar, edge)

        insets, outsets = zip(*map(primary, offsets, channels))
        triple = tuple(map(float, white_xyz))
        rendering = directed(nonnegative(shell.color), composed(constant(np.linalg.inv(normalized)), spanned(insets, triple)))
        shaped = shell.menu(
            curve,
            cases={
                kind.title: xyz(*(group.placed({"value": rendering[axis]} | {field: shell.input(curve, kind, field) for field in kind.__struct_fields__}, RESULT) for axis in AXES))
                for kind, group in shapes.items()
            },
            kind="VECTOR",
        )
        return directed(preserved(rendering, shaped, shell.input(application, preserve_hue)), composed(inverted(spanned(outsets, triple)), constant(normalized)))

    return rendering_basis(shell.config, shell.record.space).map(body)


def neutralized(shell: Shell[Neutral]) -> BlenderResult[Expr]:
    """Builds Khronos PBR Neutral over straight linear Rec.709 at its published F90 reflectance, 0.8 compression start, and 0.15 desaturation."""
    reflectance, compression, desaturation = 0.04, 0.8 - 0.04, 0.15
    positive = nonnegative(shell.color)
    lowest = extreme("MINIMUM", positive)
    shifted = positive - where(lowest < 2.0 * reflectance, lowest * math("MULTIPLY_ADD", lowest, -1.0 / (4.0 * reflectance), 1.0), reflectance)
    peak = extreme("MAXIMUM", shifted)
    compressed = 1.0 - (1.0 - compression) ** 2 / (peak + (1.0 - 2.0 * compression))
    fade = 1.0 - 1.0 / math("MULTIPLY_ADD", peak - compressed, desaturation, 1.0)
    return Ok(where(peak < compression, shifted, mixed(fade, vector("SCALE", shifted, compressed / peak), compressed)))


def adapted(shell: Shell[Photoreceptor]) -> BlenderResult[Expr]:
    """Runs the R/D Photoreceptor Tonemap node, a disabled contrast passing zero as automatic."""
    intensity, contrast, light_adaptation, chromatic_adaptation = Photoreceptor.__struct_fields__
    return Ok(
        Expr(
            kind=bpy.types.CompositorNodeTonemap,
            inputs=(
                ("Image", shell.color),
                ("Type", "R/D Photoreceptor"),
                ("Contrast", switched(shell.gate(contrast), 0.0, shell.input(contrast))),
                ("Intensity", shell.input(intensity)),
                ("Light Adaptation", shell.input(light_adaptation)),
                ("Chromatic Adaptation", shell.input(chromatic_adaptation)),
            ),
            socket="Image",
            shape="RGBA",
        )
    )


def habled(shell: Shell[Hable]) -> BlenderResult[Expr]:
    """Applies independent Hable coefficients normalized at the live white, the response held in log magnitude and sign past middle tones."""
    a, b, c, d, e, f, stops = map(shell.input, Hable.__struct_fields__)
    white = 2.0**stops
    slope_raw, intercept_raw = a * (f - e), b * (c * f - e)
    normalizer = math("MULTIPLY_ADD", white, slope_raw, intercept_raw)
    slope, intercept = slope_raw / normalizer, intercept_raw / normalizer

    def binary(operand: Expr) -> Expr:
        return math("LOGARITHM", operand, 2.0)

    slope_log, slope_sign, intercept_log, intercept_sign = binary(abs(slope)), math("SIGN", slope), binary(abs(intercept)), math("SIGN", intercept)
    denominator = math("MULTIPLY_ADD", white, math("MULTIPLY_ADD", a, white, b), d * f)

    def response(value: Expr) -> Expr:
        delta, logarithm = value - white, binary(value)
        sloped = logarithm + slope_log
        crossed = intercept_log > sloped
        larger, smaller = where(crossed, intercept_log, sloped), where(crossed, sloped, intercept_log)
        summed = math("MULTIPLY_ADD", slope_sign * intercept_sign, 2.0 ** (smaller - larger), 1.0)
        present = abs(intercept_sign)
        factor = math("MULTIPLY_ADD", delta, slope, 1.0)
        centered = value > white * 0.5
        factor_log = where(centered, binary(abs(factor)), where(present, larger + binary(summed), sloped))
        factor_sign = where(centered, math("SIGN", factor), where(present, where(crossed, intercept_sign, slope_sign), slope_sign))
        magnitude = where(centered, abs(factor), where(present, summed, 1.0))
        scale = math("MAXIMUM", value, d)
        scaled = math("MULTIPLY_ADD", value / scale, math("MULTIPLY_ADD", a, value, b), d / scale * f)
        low_log = logarithm - stops + factor_log + (binary(denominator) - (binary(scale) + binary(scaled)))
        low = where((value > 0.0) * (magnitude > 0.0), 2.0**low_log * factor_sign, 0.0)
        inverse = 1.0 / value
        weight = where(centered, math("MULTIPLY_ADD", delta / value, slope, inverse), math("MULTIPLY_ADD", intercept, inverse, slope))
        high = weight * denominator / (white * math("MULTIPLY_ADD", d * f, inverse * inverse, math("MULTIPLY_ADD", b, inverse, a)))
        return where(value > 1.0, high, low)

    lifted = nonnegative(shell.color)
    return Ok(xyz(*(response(lifted[axis]) for axis in AXES)))


def tabled_tony(shell: Shell[Tony]) -> BlenderResult[Expr]:
    """Samples the shipped Tony McMapface table tetrahedrally over its rational input shaping."""
    with ExitStack() as scope:
        table = files(__package__).joinpath("tony_mc_mapface.spi3d")
        try:
            path = scope.enter_context(as_file(table))
        except OSError as error:
            return Error((FileFault(access="read", request=table, error=error),))
        return (
            Result[tuple[PyOpenColorIO.GroupTransform, Option[tuple[Lattice, npt.NDArray[np.float32]]]], BlenderFaults]
            .bind(
                split(shell.config, PyOpenColorIO.FileTransform(src=str(path), interpolation=PyOpenColorIO.INTERP_TETRAHEDRAL)),
                lambda parts: parts[1].to_result_with(lambda: (BlenderAbsentFault(owner=str(path), key=PyOpenColorIO.Lut3DTransform.__name__),)),
            )
            .bind(lambda found: tabled(item_key(Tony), Tony.title, msgspec.structs.replace(found[0], spacing=Rational()), found[1], shell.color, shell.config))
        )


# --- [WHITE]


def metered[T: Logarithmic | ReinhardExtended | Hable](curve: T, white: float) -> BlenderResult[T]:
    """Resolves a measured scene white into the manual white, refusing a white outside its stop range or the record's invariants."""
    return catch(lambda: msgspec.structs.replace(curve, scene_white=Gated(enabled=True, value=msgspec.convert(float(np.log2(white)), type=Stops))), exception=ValueError)().map_error(
        lambda _: (DomainFault(subject=type(curve).__name__, value=white, accepted="scene white inside its stop range holding nonzero contrast"),)
    )


def prepared[T: (ToneCurve, Hable)](
    _identity: Identity,
    _scene: bpy.types.Scene,
    record: T,
    config: PyOpenColorIO.Config,
    image: OpenImageIO.ImageBuf,
    _precision: Precision,
    _frame: CameraFrame,
    _entry: bpy.types.SceneCompositorEffect,
) -> BlenderResult[Prepared[T]]:
    """Returns the record with its frame-read white resolved from the entering image."""
    return Result[npt.NDArray[np.float32], BlenderFaults].bind(pixels(image), lambda plane: record.whitened(config, plane)).map(lambda found: (found.default_value(record), {}))


def metering(call: Call[Named]) -> BlenderResult[Outcome]:
    """Arms entering-frame measurement of a tone curve holding a scene white."""
    return (
        call
        .root(bpy.types.Scene)
        .bind(lambda scene: tone_curve.named(call.identity, scene, call.record.name).bind(lambda entry: tone_curve.read(call.identity, entry)))
        .bind(lambda held: measuring(tone_curve, call) if isinstance(held.record.curve, Logarithmic | ReinhardExtended) else Ok(Unchanged(reason="Curve has no scene white parameter")))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

tone_curve: CompositorEffect[ToneCurve] = CompositorEffect(
    record=ToneCurve,
    graph=curved,
    alpha=Alpha.STRAIGHT,
    parts=(
        Prepares(prepare=prepared, when=lambda record: not record.pointwise),
        Measures(
            revise=lambda config, record, file: record.whitened(config, file.pixels),
            command=Some(
                Operator(
                    key="meter_white", title="Measure Scene White", description="Measure curve scene white from stage input", form=Named, run=metering, undo=False, condition=Some(Job(kind="RENDER"))
                )
            ),
        ),
    ),
)
hable: CompositorEffect[Hable] = CompositorEffect(
    record=Hable,
    graph=habled,
    alpha=Alpha.STRAIGHT,
    parts=(
        Prepares(prepare=prepared, when=lambda record: not record.pointwise),
        Measures(
            revise=lambda config, record, file: record.whitened(config, file.pixels),
            command=Some(
                Operator(
                    key="meter_hable_white",
                    title="Measure Hable White",
                    description="Measure Hable scene white from stage input",
                    form=Named,
                    run=lambda call: measuring(hable, call),
                    undo=False,
                    condition=Some(Job(kind="RENDER")),
                )
            ),
        ),
    ),
)
neutral = CompositorEffect(record=Neutral, graph=neutralized, alpha=Alpha.STRAIGHT, parts=(Spaces(space=lambda _record: REC709_SCENE),))
photoreceptor = CompositorEffect(record=Photoreceptor, graph=adapted, alpha=Alpha.RESULT)
tony = CompositorEffect(record=Tony, graph=tabled_tony, alpha=Alpha.STRAIGHT, parts=(Spaces(space=lambda _record: REC709_SCENE), Registers(rows=(TILES,))))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Application",
    "Curve",
    "Hable",
    "Logarithmic",
    "Neutral",
    "Photoreceptor",
    "PiecewisePower",
    "ReinhardExtended",
    "RenderingPrimary",
    "Sigmoid",
    "ToneCurve",
    "Tony",
    "hable",
    "neutral",
    "photoreceptor",
    "tone_curve",
    "tony",
]
