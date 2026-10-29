"""Tone operators from scene-linear to display-linear: per-channel curves over rendering primaries, PBR Neutral, Photoreceptor, Hable, and Tony McMapface."""

from collections.abc import Mapping
from contextlib import ExitStack
from functools import partial
from importlib.resources import as_file, files
from pathlib import Path
from typing import Annotated, Final

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.commands.operators import Answer, Call, Done, Operator, Unchanged
from blender.bpy.data.rna.conditions import Job
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.data.rna.paths import rooted
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes import functions
from blender.bpy.nodes.desired import RESULT
from blender.bpy.nodes.functions import grouped
from blender.bpy.nodes.graphs import composed, directed, Expr, inverted, math, matrix, mixed, switched, vector, where, xyz
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, MenuItem, MenuItems, Socket
from blender.bpy.render.color.spaces import EXPOSURE
from blender.bpy.render.compositor.colors import AXES, extreme, nonnegative, preserved, spanned
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, Named
from blender.bpy.render.compositor.inputs import CameraFrame, captured, entered, valued
from blender.bpy.render.compositor.shells import Shell
from blender.bpy.render.compositor.tables import tabled, TILES
from blender.bpy.render.compositor.viewers import READ
from blender.imaging.analysis.metering import Samples
from blender.imaging.color.luts import Rational, split
from blender.imaging.color.processors import ColorSpace, GREY, REC709_SCENE, rendering_basis
from blender.imaging.files.buffers import pixels
from blender.imaging.files.images import ImageFile
from blender.imaging.sampling.integration import Estimate, Precision
from blender.records.faults import Fault, Faults, Outcome, RecordReason
from blender.records.fields import Finite

from ...chain.stages import Effect, prepared, Stage

# --- [CONSTANTS] ------------------------------------------------------------------------

_SIGMOID: Final = MenuItem(name="Sigmoid", description="Log-logistic response pinning scene zero to black and middle gray to itself")
_PIECEWISE_POWER: Final = MenuItem(name="Piecewise Power", description="Hable's toe, linear, and shoulder segments normalized at their white point")
_LOGARITHMIC: Final = MenuItem(name="Logarithmic", description="Drago's adaptive logarithm mapping scene white to display white")
_REINHARD_EXTENDED: Final = MenuItem(name="Reinhard Extended", description="Reinhard's photographic response with a finite scene white mapping to one")
_WHITES: Final = ("logarithmic", "reinhard_extended")
_CURVE: Final = Socket(
    key="curve",
    kind=bpy.types.NodeSocketMenu,
    description="Curve each rendering channel takes",
    default_value=_SIGMOID.name,
    items=Some(MenuItems(items=(_SIGMOID, _PIECEWISE_POWER, _LOGARITHMIC, _REINHARD_EXTENDED))),
)
_PRESERVE_HUE, _RECOVER_PURITY = (
    Socket(key=item_key("application", field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloatFactor, description=description, min_value=0.0, max_value=1.0, default_value=default)
    for field, description, default in (("preserve_hue", "Share of hue twist corrected, channel sum kept", 1.0), ("recover_purity", "Share of each inset the return through outset undoes", 0.0))
)
_PRIMARIES: Final = tuple(
    InterfacePanel(
        key=item_key("application", channel),
        name=bpy.path.display_name(channel),
        description=f"{bpy.path.display_name(channel)} rendering primary inset and rotated within base gamut's angular limit",
        inputs=tuple(
            Socket(key=item_key("application", channel, field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, description=description, min_value=low, max_value=high)
            for field, description, low, high in (
                ("inset", "Share of distance from base gamut edge toward white", 0.0, 0.99),
                ("rotation", "Share of base gamut rotation limit about white", -0.999, 0.999),
            )
        ),
    )
    for channel in ("red", "green", "blue")
)
_TONE_FACE: Final = Interface(
    inputs=(_CURVE,),
    panels=(
        *(
            InterfacePanel(
                key=case,
                name=bpy.path.display_name(case),
                inputs=tuple(
                    Socket(key=item_key(case, field), name=bpy.path.display_name(field), kind=kind, description=description, min_value=low, max_value=high, default_value=default)
                    for field, kind, description, low, high, default in rows
                ),
                panels=tuple(
                    InterfacePanel(
                        key=item_key(case, "white"),
                        name="Scene White",
                        description="Manual white in stops from 1.0, read from frame when off",
                        toggle=Some(False),
                        inputs=(
                            Socket(
                                key=item_key(case, "white", "stops"),
                                name="Stops",
                                kind=bpy.types.NodeSocketFloat,
                                min_value=EXPOSURE.prop.hard_min,
                                max_value=EXPOSURE.prop.hard_max,
                                default_value=stops,
                            ),
                        ),
                    )
                    for stops in white
                ),
            )
            for case, white, rows in (
                (
                    "sigmoid",
                    (),
                    (
                        ("contrast", bpy.types.NodeSocketFloat, "Slope at middle gray, setting compressed scene range", 0.1, 10.0, 1.5),
                        ("skew", bpy.types.NodeSocketFloat, "Contrast moved into shadows below 0 and highlights above 0", -1.0, 1.0, None),
                        ("display_white", bpy.types.NodeSocketFloat, "Display value approached at infinity, 1 at reference white", 0.2, 16.0, 1.0),
                        ("display_black", bpy.types.NodeSocketFloat, "Display value at scene zero", 0.0, 0.15, 0.000152),
                    ),
                ),
                (
                    "piecewise_power",
                    (),
                    (
                        ("toe_strength", bpy.types.NodeSocketFloatFactor, "Toe depth below linear segment", 0.0, 1.0, 0.5),
                        ("toe_length", bpy.types.NodeSocketFloatFactor, "Lower range the toe spans, applied as its 2.2 power", 0.0, 1.0, 0.5),
                        ("shoulder_strength", bpy.types.NodeSocketFloat, "Stops of highlight range the shoulder holds above white", 0.0, 16.0, 2.0),
                        ("shoulder_length", bpy.types.NodeSocketFloat, "Upper range the shoulder spans", 1e-5, 1.0, 0.5),
                        ("shoulder_angle", bpy.types.NodeSocketFloatFactor, "Shoulder overshoot past white", 0.0, 1.0, 1.0),
                        ("gamma", bpy.types.NodeSocketFloat, "Linear segment power", 0.1, 10.0, 1.0),
                    ),
                ),
                (
                    "logarithmic",
                    (float(np.log2(100.0)),),
                    (
                        ("bias", bpy.types.NodeSocketFloat, "Logarithm base shape between 2 at black and 10 at scene white", 0.5, 1.0, 0.85),
                        ("display_white", bpy.types.NodeSocketFloat, "Display value at scene white, 1 at reference white", 0.2, 16.0, 1.0),
                    ),
                ),
                ("reinhard_extended", (0.0,), ()),
            )
        ),
        InterfacePanel(key="application", name="Application", description="Rendering primaries and hue handling around curve", inputs=(_PRESERVE_HUE, _RECOVER_PURITY), panels=_PRIMARIES),
    ),
)
_A, _B, _C, _D, _E, _F = (
    Socket(key=key, kind=bpy.types.NodeSocketFloat, description=description, min_value=low, max_value=high, default_value=default)
    for key, description, low, high, default in (
        ("a", "Shoulder strength", 0.001, 0.5, 0.15),
        ("b", "Linear strength", 0.0, 0.6, 0.5),
        ("c", "Linear angle", 0.0, 0.3, 0.1),
        ("d", "Toe strength", 1e-6, 0.9, 0.2),
        ("e", "Toe numerator", 0.0001, 0.05, 0.02),
        ("f", "Toe denominator", 0.2, 0.6, 0.3),
    )
)
_WHITE_STOPS: Final = Socket(key="white_stops", name="Stops", kind=bpy.types.NodeSocketFloat, min_value=EXPOSURE.prop.hard_min, max_value=EXPOSURE.prop.hard_max, default_value=float(np.log2(11.2)))
_WHITE: Final = InterfacePanel(key="white", name="Scene White", description="Manual white in stops from 1.0, read from frame when off", toggle=Some(True), inputs=(_WHITE_STOPS,))
_INTENSITY, _LIGHT_ADAPTATION, _CHROMATIC_ADAPTATION = (
    Socket(key=key, kind=kind, description=description, min_value=low, max_value=high, default_value=default)
    for key, kind, description, low, high, default in (
        ("intensity", bpy.types.NodeSocketFloat, "Brightness, scaling adaptation by e to the negative intensity", None, None, None),
        ("light_adaptation", bpy.types.NodeSocketFloatFactor, "Blend from frame adaptation at 0 to pixel adaptation at 1", 0.0, 1.0, None),
        ("chromatic_adaptation", bpy.types.NodeSocketFloatFactor, "Blend from luminance at 0 to each channel at 1", 0.0, 1.0, None),
    )
)
_CONTRAST_VALUE: Final = Socket(key="contrast_value", name="Contrast", kind=bpy.types.NodeSocketFloat, min_value=1e-6, default_value=1.0)
_CONTRAST: Final = InterfacePanel(key="contrast", name="Contrast", description="Response exponent, read from frame luminance range when off", toggle=Some(False), inputs=(_CONTRAST_VALUE,))

# --- [MODELS] ---------------------------------------------------------------------------


class ToneCurve(Effect, frozen=True, kw_only=True):
    """Scene-linear to display-linear curve applied per rendering channel, its rendering primaries moving from a base colorspace."""

    stage = Stage.TONE
    pointwise = True
    space: Annotated[ColorSpace, msgspec.Meta(description="Colorspace rendering primaries move from")] = PyOpenColorIO.ROLE_SCENE_LINEAR


class Neutral(Effect, frozen=True, kw_only=True):
    """Khronos PBR Neutral preserving base colors and compressing highlights toward white."""

    stage = Stage.TONE
    pointwise = True


class Photoreceptor(Effect, frozen=True, kw_only=True):
    """Reinhard and Devlin's response against frame adaptation level."""

    stage = Stage.TONE
    pointwise = False


class Hable(Effect, frozen=True, kw_only=True):
    """White-normalized Uncharted 2 response with independent authored coefficients, a manual white holding nonzero contrast."""

    stage = Stage.TONE
    pointwise = True


class Tony(Effect, frozen=True, kw_only=True):
    """Tony McMapface's rationally shaped RGB table in linear Rec.709."""

    stage = Stage.TONE
    pointwise = True


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CURVES]


@grouped(kind=bpy.types.CompositorNodeTree)
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


@grouped(kind=bpy.types.CompositorNodeTree)
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


@grouped(kind=bpy.types.CompositorNodeTree)
def logarithmic(value: Expr, bias: Expr, white_stops: Expr, display_white: Expr) -> Expr:
    """Drago's adaptive logarithm of one channel, scene white at display white."""
    level, clipped = 2.0**white_stops, math("MAXIMUM", value, 0.0)

    def natural(operand: Expr) -> Expr:
        lifted = operand + 1.0
        delta = lifted - 1.0
        return where(delta > 0.0, math("LOGARITHM", lifted, np.e) * (operand / delta), operand)

    base = math("MULTIPLY_ADD", (clipped / level) ** math("LOGARITHM", bias, 0.5), 8.0, 2.0)
    return natural(clipped) / (natural(level) / float(np.log(10.0)) * math("LOGARITHM", base, np.e)) * display_white


@grouped(kind=bpy.types.CompositorNodeTree)
def reinhard(value: Expr, white_stops: Expr) -> Expr:
    """Reinhard's photographic response of one channel, scene white at one."""
    return value / (value + 1.0) * (value / 2.0 ** (white_stops * 2.0) + 1.0)


# --- [GRAPHS]


def _neutral(shell: Shell[Neutral]) -> Outcome[Expr]:
    """Builds Khronos PBR Neutral over straight linear Rec.709 at its published F90 reflectance, 0.8 compression start, and 0.15 desaturation."""
    reflectance, compression, desaturation = 0.04, 0.8 - 0.04, 0.15
    positive = nonnegative(shell.color)
    lowest = extreme("MINIMUM", positive)
    shifted = positive - where(lowest < 2.0 * reflectance, lowest * math("MULTIPLY_ADD", lowest, -1.0 / (4.0 * reflectance), 1.0), reflectance)
    peak = extreme("MAXIMUM", shifted)
    compressed = 1.0 - (1.0 - compression) ** 2 / (peak + (1.0 - 2.0 * compression))
    fade = 1.0 - 1.0 / math("MULTIPLY_ADD", peak - compressed, desaturation, 1.0)
    return Ok(where(peak < compression, shifted, mixed(fade, vector("SCALE", shifted, compressed / peak), compressed)))


neutral: Final = CompositorEffect(graph=_neutral, record=Neutral, mix=Blended(alpha=Alpha.STRAIGHT), space=Some(lambda _record: REC709_SCENE))


def _photoreceptor(shell: Shell[Photoreceptor]) -> Outcome[Expr]:
    """Runs the R/D Photoreceptor Tonemap node, a disabled contrast passing zero as automatic."""
    return Ok(
        Expr(
            kind=bpy.types.CompositorNodeTonemap,
            inputs=(
                ("Image", shell.color),
                ("Type", "R/D Photoreceptor"),
                ("Contrast", switched(shell.input(_CONTRAST), 0.0, shell.input(_CONTRAST_VALUE))),
                ("Intensity", shell.input(_INTENSITY)),
                ("Light Adaptation", shell.input(_LIGHT_ADAPTATION)),
                ("Chromatic Adaptation", shell.input(_CHROMATIC_ADAPTATION)),
            ),
            socket="Image",
            shape=bpy.types.NodeSocketColor,
        )
    )


photoreceptor: Final = CompositorEffect(
    graph=_photoreceptor, record=Photoreceptor, interface=Interface(inputs=(_INTENSITY, _LIGHT_ADAPTATION, _CHROMATIC_ADAPTATION), panels=(_CONTRAST,)), mix=Blended(alpha=Alpha.RESULT)
)


def _tony(shell: Shell[Tony]) -> Outcome[Expr]:
    """Samples the shipped Tony McMapface table tetrahedrally over its rational input shaping."""
    with ExitStack() as scope:
        table = files(__package__).joinpath("tony_mc_mapface.spi3d")
        return (
            Result[Path, Faults]
            .bind(
                catch(scope.enter_context, exception=OSError)(as_file(table)).map_error(partial(Fault.of, RecordReason.UNREADABLE, str(table))),
                lambda path: split(shell.config, PyOpenColorIO.FileTransform(src=str(path), interpolation=PyOpenColorIO.INTERP_TETRAHEDRAL)).bind(
                    lambda parts: parts[1].to_result_with(partial(Fault.of, BlenderReason.ABSENT, str(path), member=PyOpenColorIO.Lut3DTransform.__name__))
                ),
            )
            .bind(lambda found: tabled(item_key(Tony), "Tony McMapface", msgspec.structs.replace(found[0], spacing=Rational()), found[1], shell.color, shell.config))
        )


tony: Final = CompositorEffect(graph=_tony, record=Tony, mix=Blended(alpha=Alpha.STRAIGHT), space=Some(lambda _record: REC709_SCENE), registers=(TILES,))

# --- [WHITE]


def whitened(prefix: tuple[Key, ...], values: Mapping[Key, Value], config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32]) -> Outcome[Mapping[Key, Value]]:
    """Manual scene white read from covered scene-linear pixels at the white panel under `prefix`, refusing a white outside its stop range or one leaving Hable normalization without contrast."""

    def resolved(white: float) -> Outcome[Mapping[Key, Value]]:
        refused = partial(Fault.of, RecordReason.OUTSIDE, item_key(*prefix, "white"), member="scene white inside its stop range holding nonzero contrast")
        match tuple(values.get(socket.key) for socket in (_A, _B, _C, _E, _F)):
            case (float(shoulder), float(linear), float(angle), float(numerator), float(denominator)):
                contrast = white * shoulder * (denominator - numerator) + linear * (angle * denominator - numerator)
            case _:
                contrast = 1.0
        return (
            catch(msgspec.convert, exception=msgspec.ValidationError)(float(np.log2(white)), Finite)
            .map_error(refused)
            .filter_with(lambda _: contrast != 0.0, lambda _: refused(white))
            .map(lambda held: {item_key(*prefix, "white"): True, item_key(*prefix, "white", "stops"): held})
        )

    return (
        Samples
        .of(config, plane)
        .bind(lambda samples: samples.extent().to_result_with(partial(Fault.of, RecordReason.OUTSIDE, "White metering", str(plane.shape), member="covered pixels holding finite luminance")))
        .bind(lambda extent: resolved(extent.white))
    )


def metered(prefix: tuple[Key, ...], title: str, identity: Identity, _scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, file: ImageFile[np.float32]) -> Outcome[Option[str]]:
    """Writes the manual scene white read from the entering frame under `prefix`, the undo step named by the measuring command."""
    return (
        captured(identity, entry)
        .bind(lambda snapshot: whitened(prefix, valued(snapshot), PyOpenColorIO.GetCurrentConfig(), file.pixels))
        .bind(lambda found: entered(identity, entry, found))
        .map(lambda wrote: Some(title) if wrote else Nothing)
    )


@prepared.register
def _(
    _record: ToneCurve,
    _identity: Identity,
    values: Mapping[Key, Value],
    config: PyOpenColorIO.Config,
    image: OpenImageIO.ImageBuf,
    _precision: Precision,
    _frame: CameraFrame,
    _scene: bpy.types.Scene,
    /,
) -> Outcome[Option[OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf] | Mapping[Key, Value]]]:
    """Reads the selected curve's scene white from the entering image while its manual white is off."""
    match tuple(key for key in _WHITES if values.get(_CURVE.key) == bpy.path.display_name(key) and values.get(item_key(key, "white")) is False):
        case (key,):
            return Result[npt.NDArray[np.float32], Faults].bind(pixels(image), lambda plane: whitened((key,), values, config, plane)).map(Some)
        case _:
            return Ok(Nothing)


@prepared.register
def _(
    _record: Hable, _identity: Identity, values: Mapping[Key, Value], config: PyOpenColorIO.Config, image: OpenImageIO.ImageBuf, _precision: Precision, _frame: CameraFrame, _scene: bpy.types.Scene, /
) -> Outcome[Option[OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf] | Mapping[Key, Value]]]:
    """Reads the Hable scene white from the entering image while its manual white is off."""
    return Result[npt.NDArray[np.float32], Faults].bind(pixels(image), lambda plane: whitened((), values, config, plane)).map(Some) if values.get(_WHITE.key) is False else Ok(Nothing)


def meter_white(call: Call[Named]) -> Outcome[Answer]:
    """Measure curve scene white from stage input."""

    def armed(scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, values: Mapping[Key, Value]) -> Outcome[Answer]:
        match tuple(key for key in _WHITES if values.get(_CURVE.key) == bpy.path.display_name(key)):
            case (key,):
                return READ.entering(call.identity, scene, entry, partial(metered, (key,), bpy.path.display_name(meter_white.__name__))).map(lambda _: Done())
            case _:
                return Ok(Unchanged(reason="Curve has no scene white parameter"))

    return rooted(bpy.types.Scene, call.context).bind(
        lambda scene: tone_curve.named(call.identity, scene, call.properties.name).bind(lambda entry: captured(call.identity, entry).bind(lambda snapshot: armed(scene, entry, valued(snapshot))))
    )


def meter_hable_white(call: Call[Named]) -> Outcome[Answer]:
    """Measure Hable scene white from stage input."""
    return (
        rooted(bpy.types.Scene, call.context)
        .bind(
            lambda scene: hable.named(call.identity, scene, call.properties.name).bind(
                lambda entry: READ.entering(call.identity, scene, entry, partial(metered, (), bpy.path.display_name(meter_hable_white.__name__)))
            )
        )
        .map(lambda _: Done())
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


def _tone_curve(shell: Shell[ToneCurve]) -> Outcome[Expr]:
    """Applies the live per-channel curve between rendering primaries inset and rotated within the base gamut."""
    shapes = (
        (_SIGMOID, "sigmoid", sigmoid.group),
        (_PIECEWISE_POWER, "piecewise_power", piecewise.group),
        (_LOGARITHMIC, "logarithmic", logarithmic.group),
        (_REINHARD_EXTENDED, "reinhard_extended", reinhard.group),
    )

    def body(basis: npt.NDArray[np.float64]) -> Outcome[Expr]:
        normalized = basis / basis[1].sum()
        white_xyz = normalized.sum(axis=1)
        white = white_xyz[:2] / white_xyz.sum()
        offsets = (basis / basis.sum(axis=0))[:2].T - white
        edges = np.roll(offsets, -1, axis=0) - offsets
        normals, spans = edges[:, ::-1] * (1.0, -1.0), offsets[:, 0] * edges[:, 1] - offsets[:, 1] * edges[:, 0]
        angles = np.sort(np.arctan2(offsets[:, 1], offsets[:, 0]))
        limit = float((np.pi - np.diff(np.append(angles, angles[0] + 2.0 * np.pi)).max()) / 2.0)
        planar = (*map(float, white), 0.0)

        def primary(offset: npt.NDArray[np.float64], panel: InterfacePanel) -> tuple[Expr, Expr]:
            inset, rotation = panel.inputs
            turned = Expr(
                kind=bpy.types.ShaderNodeVectorRotate,
                inputs=(("Vector", (*map(float, offset), 0.0)), ("Angle", shell.input(rotation) * limit)),
                members={"rotation_type": "Z_AXIS"},
                socket="Vector",
                shape=bpy.types.NodeSocketVector,
            )
            first, second, third = (vector("DOT_PRODUCT", turned, (*map(float, normal), 0.0)) / float(span) for normal, span in zip(normals, spans))
            edge = vector("MULTIPLY_ADD", turned, 1.0 / math("MAXIMUM", math("MAXIMUM", first, second), third), planar)
            share = shell.input(inset)
            return mixed(1.0 - share, planar, edge), mixed(1.0 - shell.input(_RECOVER_PURITY) * share, planar, edge)

        insets, outsets = zip(*map(primary, offsets, _PRIMARIES))
        triple = tuple(map(float, white_xyz))
        to_xyz = matrix({(row + 1, column + 1): float(value) for (row, column), value in np.ndenumerate(normalized)})
        from_xyz = matrix({(row + 1, column + 1): float(value) for (row, column), value in np.ndenumerate(np.linalg.inv(normalized))})
        rendering = directed(nonnegative(shell.color), composed(from_xyz, spanned(insets, triple)))
        shaped = shell.menu(
            _CURVE,
            cases={
                item: xyz(
                    *(
                        functions.placed(group, {"value": rendering[axis]} | {socket.key: shell.input(key, socket) for socket in group.interface.inputs if socket.key != "value"}, RESULT)
                        for axis in AXES
                    )
                )
                for item, key, group in shapes
            },
        )
        return Ok(directed(preserved(rendering, shaped, shell.input(_PRESERVE_HUE)), composed(inverted(spanned(outsets, triple)), to_xyz)))

    return rendering_basis(shell.config, shell.record.space).bind(body)


tone_curve: Final = CompositorEffect(
    graph=_tone_curve, record=ToneCurve, interface=_TONE_FACE, mix=Blended(alpha=Alpha.STRAIGHT), registers=(Operator(form=Named, run=meter_white, undo=Nothing, condition=Some(Job(kind="RENDER"))),)
)


def _hable(shell: Shell[Hable]) -> Outcome[Expr]:
    """Applies independent Hable coefficients normalized at the live white, the response held in log magnitude and sign past middle tones."""
    a, b, c, d, e, f, stops = map(shell.input, (_A, _B, _C, _D, _E, _F, _WHITE_STOPS))
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


hable: Final = CompositorEffect(
    graph=_hable,
    record=Hable,
    interface=Interface(inputs=(_A, _B, _C, _D, _E, _F), panels=(_WHITE,)),
    mix=Blended(alpha=Alpha.STRAIGHT),
    registers=(Operator(form=Named, run=meter_hable_white, undo=Nothing, condition=Some(Job(kind="RENDER"))),),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Hable", "Neutral", "Photoreceptor", "ToneCurve", "Tony", "hable", "neutral", "photoreceptor", "tone_curve", "tony"]
