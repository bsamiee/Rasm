"""Tone curves, rendering primaries, and photoreceptor compositor graphs."""

from collections.abc import Mapping
from contextlib import ExitStack
from functools import partial
from importlib.resources import as_file
from importlib.resources.abc import Traversable
from math import e, log2, pi
from typing import Final, Literal

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Operator, Outcome, Unchanged
from blender.bpy.data.rna.conditions import Job
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.bpy.nodes.graphs import Component, Expr, group_input, grouped, math, mixed, NodeGroup, RESULT, switched, Term, vector, where, xyz
from blender.bpy.nodes.interface import item_key, Socket
from blender.bpy.render.effects.formation import encode_formation
from blender.bpy.render.effects.grade.lookup import tabled, tiled, tiles
from blender.bpy.render.effects.rows import Alpha, CameraFrame, CompositorEffect, Derived, HIDDEN, Measured, Named, Shell
from blender.bpy.render.effects.viewers import measuring
from blender.imaging.analysis.metering import Samples
from blender.imaging.chain import GREY
from blender.imaging.color.luts import Rational
from blender.imaging.compositing.blending import Entry
from blender.imaging.effects.grade.lookup import Lookup
from blender.imaging.effects.tone import curves
from blender.imaging.effects.tone.curves import Application, Curve, Hable, Logarithmic, Neutral, Photoreceptor, PiecewisePower, ReinhardExtended, RenderingPrimary, Sigmoid, ToneCurve, Tony
from blender.imaging.faults import Access, ImagingBufferFault, ImagingFileFault, ImagingResult, ImagingWhiteFault
from blender.imaging.sampling import Estimate, Precision

# --- [CONSTANTS] ------------------------------------------------------------------------

AXES: Final[tuple[Component, Component, Component]] = ("X", "Y", "Z")

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
        return where(delta > 0.0, math("LOGARITHM", lifted, e) * (operand / delta), operand)

    base = math("MULTIPLY_ADD", (clipped / level) ** math("LOGARITHM", bias, 0.5), 8.0, 2.0)
    return natural(clipped) / (natural(level) / float(np.log(10.0)) * math("LOGARITHM", base, e)) * display_white


@grouped(item_key("tone", ReinhardExtended), ReinhardExtended.title)
def reinhard(value: Expr, scene_white: Expr) -> Expr:
    """Reinhard's photographic response of one channel, scene white at one."""
    return value / (value + 1.0) * (value / 2.0 ** (scene_white * 2.0) + 1.0)


# --- [CHANNELS]


def extreme(operation: Literal["MINIMUM", "MAXIMUM"], value: Expr) -> Expr:
    """Smallest or largest component of a vector."""
    x, y, z = (value[axis] for axis in AXES)
    return math(operation, math(operation, x, y), z)


def nonnegative(color: Expr) -> Expr:
    """Desaturates negative channels toward the non-negative channel mean."""
    level = math("MAXIMUM", vector("DOT_PRODUCT", color, (1.0 / 3.0,) * 3), 0.0)
    return mixed(math("MINIMUM", level / (level - extreme("MINIMUM", color)), 1.0), xyz(level, level, level), color)


def preserved(rendering: Expr, shaped: Expr, hue: Expr) -> Expr:
    """Restores the rendering channel order's hue across curved channels, channel sum kept."""
    swapped = rendering["X"] > rendering["Y"]

    def xy(value: Expr) -> tuple[Expr, Expr]:
        return where(swapped, value["Y"], value["X"]), where(swapped, value["X"], value["Y"])

    low, high = xy(rendering)
    below, above = rendering["Z"] < low, rendering["Z"] > high

    def ranked(value: Expr) -> tuple[Expr, Expr, Expr]:
        smaller, larger = xy(value)
        return where(below, value["Z"], smaller), where(above, larger, where(below, smaller, value["Z"])), where(above, value["Z"], larger)

    (r_min, r_mid, r_max), (f_min, f_mid, f_max) = ranked(rendering), ranked(shaped)
    share = (r_mid - r_min) / (r_max - r_min)
    naive = mixed(hue, f_mid, math("MULTIPLY_ADD", f_max - f_min, share, f_min))
    total = mixed(r_min / (r_min + r_mid) * 2.0, f_min + f_max + naive, vector("DOT_PRODUCT", shaped, (1.0,) * 3))
    rest_low, rest_high = total - f_max, total - f_min
    mid_low = mixed(hue, f_mid, mixed(share, rest_low, f_max)) / math("MULTIPLY_ADD", hue, 1.0 - share, 1.0)
    mid_high = mixed(hue, f_mid, mixed(share, f_min, rest_high)) / math("MULTIPLY_ADD", hue, share, 1.0)
    high_hue = naive > f_mid
    middle, minimum, maximum = where(high_hue, mid_high, mid_low), where(high_hue, f_min, rest_low - mid_low), where(high_hue, rest_high - mid_high, f_max)
    toward = vector("MULTIPLY_ADD", vector("SIGN", rendering - r_max), maximum - middle, maximum + minimum - middle)
    return vector("MULTIPLY_ADD", vector("SIGN", rendering - r_min), middle - minimum, toward)


# --- [MATRICES]


def constant(values: npt.NDArray[np.float64]) -> Expr:
    """Combine Matrix node of a literal 3x3 matrix."""
    return Expr(kind=bpy.types.FunctionNodeCombineMatrix, inputs=tuple((f"Column {column + 1} Row {row + 1}", float(value)) for (row, column), value in np.ndenumerate(values)), socket="Matrix")


def composed(first: Expr, second: Expr) -> Expr:
    """Matrix product of two matrix terms."""
    return Expr(kind=bpy.types.FunctionNodeMatrixMultiply, inputs=(("Matrix", first), ("Matrix_001", second)), socket="Matrix")


def inverted(matrix: Expr) -> Expr:
    """Inverse of a matrix term."""
    return Expr(kind=bpy.types.FunctionNodeInvertMatrix, inputs=(("Matrix", matrix),), socket="Matrix")


def directed(direction: Term, matrix: Expr) -> Expr:
    """Direction transformed by a matrix term."""
    return Expr(kind=bpy.types.FunctionNodeTransformDirection, inputs=(("Direction", direction), ("Transform", matrix)), socket="Direction", shape="VECTOR")


def spanned(points: tuple[Expr, ...], white: Term) -> Expr:
    """Rendering-to-XYZ matrix of chromaticity points, columns scaled to the XYZ white."""
    columns = Expr(
        kind=bpy.types.FunctionNodeCombineMatrix,
        inputs=tuple(
            entry
            for column, point in enumerate(points, 1)
            for entry in ((f"Column {column} Row 1", point["X"] / point["Y"]), (f"Column {column} Row 2", 1.0), (f"Column {column} Row 3", (1.0 - point["X"] - point["Y"]) / point["Y"]))
        ),
        socket="Matrix",
    )
    scales = directed(white, inverted(columns))
    return composed(columns, Expr(kind=bpy.types.FunctionNodeCombineMatrix, inputs=tuple((f"Column {index} Row {index}", scales[axis]) for index, axis in enumerate(AXES, 1)), socket="Matrix"))


# --- [GRAPHS]


def manual[T: (ToneCurve, Hable)](record: T) -> BlenderResult[T]:
    """Returns the record when its graph reads a manual white, refusing the frame-read white preparation resolves."""
    return Ok(record) if record.pointwise else Error((BlenderAbsentFault(owner=record.title, key="current_frame_white"),))


def curved(shell: Shell[ToneCurve]) -> BlenderResult[Expr]:
    """Applies live per-channel curve over rendering primaries."""
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
        limit = float((pi - np.diff(np.append(angles, angles[0] + 2.0 * pi)).max()) / 2.0)
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

    return manual(shell.record).bind(lambda record: record.base(shell.config)).map(body)


def neutralized(color: Expr) -> Expr:
    """Builds Khronos PBR Neutral over straight linear Rec.709 RGB."""
    compression = Neutral.diffuse - Neutral.reflectance
    positive = nonnegative(color)
    lowest = extreme("MINIMUM", positive)
    shifted = positive - where(lowest < 2.0 * Neutral.reflectance, lowest * math("MULTIPLY_ADD", lowest, -1.0 / (4.0 * Neutral.reflectance), 1.0), Neutral.reflectance)
    peak = extreme("MAXIMUM", shifted)
    compressed = 1.0 - (1.0 - compression) ** 2 / (peak + (1.0 - 2.0 * compression))
    fade = 1.0 - 1.0 / math("MULTIPLY_ADD", peak - compressed, Neutral.desaturation, 1.0)
    return where(peak < compression, shifted, mixed(fade, vector("SCALE", shifted, compressed / peak), compressed))


def adapted(shell: Shell[Photoreceptor]) -> BlenderResult[Expr]:
    """Runs R/D Photoreceptor tonemap, disabled contrast passing zero as automatic."""
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
    """Applies independent Hable coefficients with live white normalization."""
    a_field, b_field, _, d_field, _, f_field, scene_white = Hable.__struct_fields__
    a, b, d, f, stops = map(shell.input, (a_field, b_field, d_field, f_field, scene_white))
    (slope_log, slope_sign), (intercept_log, intercept_sign) = ((parts["X"], parts["Y"]) for parts in (group_input(socket.key, "VECTOR") for socket in normalization))
    slope, intercept = 2.0**slope_log * slope_sign, 2.0**intercept_log * intercept_sign
    white = 2.0**stops
    denominator = math("MULTIPLY_ADD", white, math("MULTIPLY_ADD", a, white, b), d * f)

    def binary(operand: Expr) -> Expr:
        return math("LOGARITHM", operand, 2.0)

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
    return manual(shell.record).map(lambda _: xyz(*(response(lifted[axis]) for axis in AXES)))


def normalized(_identity: Identity, _scene: bpy.types.Scene, record: Hable) -> BlenderResult[Mapping[Key, Value]]:
    """Derives Hable coefficients from evaluated record."""
    _, _, _, _, _, _, scene_white = Hable.__struct_fields__
    return (
        Result[Option[tuple[float, float]], BlenderFaults]
        .bind(record.normalization(), lambda value: value.to_result_with(lambda: (BlenderAbsentFault(owner=record.title, key=scene_white),)))
        .map(lambda values: {socket.key: (log2(abs(value)) if value else 0.0, float(np.sign(value)), 0.0) for socket, value in zip(normalization, values)})
    )


def tony(table: Traversable) -> CompositorEffect[Tony]:
    """Binds canonical Tony McMapface table from asset catalog.

    Args:
        table: Canonical SPI3D catalog resource.

    Returns:
        Rec.709 row with direct rational shaping and tetrahedral sampling.
    """

    def mapped(shell: Shell[Tony]) -> BlenderResult[Expr]:
        with ExitStack() as scope:
            try:
                path = scope.enter_context(as_file(table))
            except OSError as error:
                return Error((ImagingFileFault(access=Access.READ, request=table, error=error),))
            return (
                tiled(Lookup(source=str(path), space=Tony.space), shell.config)
                .bind(lambda parts: parts[1].to_result_with(lambda: (BlenderAbsentFault(owner=str(path), key="Lut3DTransform"),)))
                .bind(lambda found: tabled(item_key(Tony), Tony.title, msgspec.structs.replace(found[0], spacing=Rational()), found[1], shell.color, shell.config))
            )

    return CompositorEffect(record=Tony, graph=mapped, alpha=Alpha.STRAIGHT, registrables=(tiles, encode_formation))


# --- [WHITE]


def whitened[T: (ToneCurve, Hable)](config: PyOpenColorIO.Config, record: T, plane: npt.NDArray[np.float32]) -> BlenderResult[Option[T]]:
    """Returns the record with its scene white read from covered scene-linear pixels, `Nothing` for a curve holding no scene white."""

    def white() -> ImagingResult[float]:
        return Samples.of(config, plane).bind(lambda samples: samples.extent().to_result_with(lambda: (ImagingWhiteFault(white=Nothing),))).map(lambda extent: extent.white)

    match record:
        case ToneCurve(curve=Logarithmic() | ReinhardExtended() as curve):
            return white().bind(partial(curves.metered, curve)).map(lambda resolved: Some(msgspec.structs.replace(record, curve=resolved)))
        case Hable():
            return white().bind(partial(curves.metered, record)).map(Some)
        case _:
            return Ok(Nothing)


def prepared[T: (ToneCurve, Hable)](
    _identity: Identity,
    _scene: bpy.types.Scene,
    record: T,
    config: PyOpenColorIO.Config,
    image: OpenImageIO.ImageBuf,
    _precision: Precision,
    _frame: CameraFrame,
    _entry: bpy.types.SceneCompositorEffect,
) -> BlenderResult[tuple[T, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]]:
    """Returns the record with its frame-read white resolved, a manual white unchanged."""
    effective: BlenderResult[Option[T]] = (
        Ok(Some(record)) if record.pointwise else Result[npt.NDArray[np.float32], BlenderFaults].bind(ImagingBufferFault.pixels(image), lambda pixels: whitened(config, record, pixels))
    )
    return effective.map(lambda found: (found.default_value(record), {}))


def metered(call: Call[Named]) -> BlenderResult[Outcome]:
    """Arms stage input measurement of a tone curve holding a scene white."""

    def armed(held: Entry[ToneCurve]) -> BlenderResult[Outcome]:
        match held.effect.curve:
            case Logarithmic() | ReinhardExtended():
                return measuring(tone_curve, call)
            case Sigmoid() | PiecewisePower():
                return Ok(Unchanged(reason="Curve has no scene white parameter"))

    return call.root(bpy.types.Scene).bind(lambda scene: tone_curve.named(call.identity, scene, call.record.name)).bind(lambda entry: tone_curve.read(call.identity, entry)).bind(armed)


# --- [COMPOSITION] ----------------------------------------------------------------------

meter_white = Operator(
    key="meter_white", title="Measure Scene White", description="Measure curve scene white from stage input", form=Named, run=metered, undo=False, condition=Some(Job(kind="RENDER"))
)
tone_curve: CompositorEffect[ToneCurve] = CompositorEffect(
    record=ToneCurve,
    graph=curved,
    alpha=Alpha.STRAIGHT,
    preparation=Some(prepared),
    measured=Some(Measured(revise=lambda config, record, file: whitened(config, record, file.pixels), command=Some(meter_white))),
    registrables=(encode_formation,),
)
hable_meter_white = msgspec.structs.replace(meter_white, key="meter_hable_white", run=lambda call: measuring(hable, call))
normalization = tuple(
    Socket(
        key=item_key(Hable.normalization.__name__, name.lower()),
        name=name,
        kind=bpy.types.NodeSocketVector,
        description="Log magnitude and sign of Hable coefficient from evaluated record",
        members={HIDDEN: True},
    )
    for name in ("Slope", "Intercept")
)
hable: CompositorEffect[Hable] = CompositorEffect(
    record=Hable,
    graph=habled,
    alpha=Alpha.STRAIGHT,
    inputs=normalization,
    derived=Some(Derived(of=normalized)),
    validated=Some(lambda record: record.normalization().map(lambda _: record)),
    preparation=Some(prepared),
    measured=Some(Measured(revise=lambda config, record, file: whitened(config, record, file.pixels), command=Some(hable_meter_white))),
    registrables=(encode_formation,),
)
neutral = CompositorEffect(record=Neutral, graph=lambda shell: Ok(neutralized(shell.color)), alpha=Alpha.STRAIGHT, registrables=(encode_formation,))
photoreceptor = CompositorEffect(record=Photoreceptor, graph=adapted, alpha=Alpha.RESULT, registrables=(encode_formation,))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "AXES",
    "adapted",
    "composed",
    "constant",
    "curved",
    "directed",
    "extreme",
    "hable",
    "hable_meter_white",
    "habled",
    "inverted",
    "logarithmic",
    "manual",
    "meter_white",
    "metered",
    "neutral",
    "neutralized",
    "nonnegative",
    "normalization",
    "normalized",
    "photoreceptor",
    "piecewise",
    "prepared",
    "preserved",
    "reinhard",
    "sigmoid",
    "spanned",
    "tone_curve",
    "tony",
    "whitened",
]
