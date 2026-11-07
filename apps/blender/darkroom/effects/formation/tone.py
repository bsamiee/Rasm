"""Tone maps from scene-linear to display-linear: per-channel curves and Hable's response with a scene white read from the frame, and the fixed Khronos PBR Neutral, Photoreceptor, and Tony McMapface maps."""

from collections.abc import Callable, Mapping
from contextlib import ExitStack
from functools import partial
from importlib.resources import as_file, files
import inspect
from math import log2
from types import MappingProxyType
from typing import Annotated, Final, override

import bpy
from expression import Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from rasm_bpy.data import Identity, Key, Value
from rasm_bpy.nodes import composed, directed, Expr, functions, Group, grouped, Interface, InterfacePanel, inverted, item_key, math, matrix, mixed, RESULT, Socket, StockInput, vector, where, xyz
from rasm_bpy.render import (
    AXES,
    captured,
    ColorSpace,
    CompositorEffect,
    CompositorRecord,
    entered,
    EXPOSURE,
    extreme,
    GraphInputs,
    latticed,
    Mix,
    nonnegative,
    path_key,
    Prepared,
    preserved,
    READ,
    spanned,
    Stage,
    TILES,
    value,
    valued,
)
from rasm_imaging import ConfigBuild, GRAY, ImageFile, Lattice, Logarithmic, pixels, processor, REC709_SCENE, rendering_basis, Samples
from rasm_records import accumulated, Fault, Nothing, Ok, Outcome, Reason

# --- [CONSTANTS] ------------------------------------------------------------------------

_SCENE_WHITE: Final = InterfacePanel.of(
    "scene_white",
    Interface(inputs=(Socket(key="stops", kind=bpy.types.NodeSocketFloat, min_value=EXPOSURE.hard_min, max_value=EXPOSURE.hard_max),)),
    description="Manual white in stops from 1.0, read from frame when off",
)
_WHITE_STOPS: Final = _SCENE_WHITE.inputs[0]
_DRAGO_WHITE: Final = float(np.log2(100.0))
TONY: Final = "TonyMcMapface Base"
UPSTREAM: Final = files(__package__).joinpath("fetched/OCIO/config.ocio")
_APPLICATION: Final = InterfacePanel.of(
    "application",
    Interface(
        inputs=(
            Socket(key="preserve_hue", kind=bpy.types.NodeSocketFloatFactor, description="Share of hue twist corrected, channel sum kept", default_value=1.0),
            Socket(key="recover_purity", kind=bpy.types.NodeSocketFloatFactor, description="Share of each inset the return through outset undoes"),
        ),
        panels=tuple(
            InterfacePanel.of(
                channel,
                Interface(
                    inputs=(
                        Socket(key="inset", kind=bpy.types.NodeSocketFloat, description="Share of distance from base gamut edge toward white", min_value=0.0, max_value=0.99),
                        Socket(key="rotation", kind=bpy.types.NodeSocketFloat, description="Share of base gamut rotation limit about white", min_value=-0.999, max_value=0.999),
                    )
                ),
                description=f"{bpy.path.display_name(channel)} rendering primary inset and rotated within base gamut's angular limit",
            )
            for channel in ("red", "green", "blue")
        ),
    ),
    description="Rendering primaries and hue handling around curve",
)
_PRESERVE_HUE, _RECOVER_PURITY = _APPLICATION.inputs
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
_ADAPTATION: Final = tuple(
    Socket(key=item_key(*name.lower().split()), kind=StockInput(node=bpy.types.CompositorNodeTonemap, input=name)) for name in ("Intensity", "Contrast", "Light Adaptation", "Chromatic Adaptation")
)

# --- [MODELS] ---------------------------------------------------------------------------


class ToneCurve(CompositorRecord, frozen=True, kw_only=True):
    """Scene-linear to display-linear curve applied per rendering channel, its rendering primaries moving from a base colorspace."""

    space: ColorSpace = PyOpenColorIO.ROLE_SCENE_LINEAR

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
        """Applies the live per-channel curve between rendering primaries inset and rotated within the base gamut."""

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
                    socket_type=bpy.types.NodeSocketVector,
                )
                first, second, third = (vector("DOT_PRODUCT", turned, (*map(float, normal), 0.0)) / float(span) for normal, span in zip(normals, spans))
                edge = vector("MULTIPLY_ADD", turned, 1.0 / math("MAXIMUM", math("MAXIMUM", first, second), third), planar)
                share = shell.input(inset)
                return mixed(1.0 - share, planar, edge), mixed(1.0 - shell.input(_RECOVER_PURITY) * share, planar, edge)

            insets, outsets = zip(*map(primary, offsets, _APPLICATION.panels))
            triple = tuple(map(float, white_xyz))
            to_xyz = matrix({(row + 1, column + 1): float(cell) for (row, column), cell in np.ndenumerate(normalized)})
            from_xyz = matrix({(row + 1, column + 1): float(cell) for (row, column), cell in np.ndenumerate(np.linalg.inv(normalized))})
            rendering = directed(nonnegative(shell.color), composed(from_xyz, spanned(insets, triple)))
            shaped = shell.menu(
                _CURVE,
                cases={
                    panel.item: xyz(
                        *(
                            functions.placed(group.group, {level.key: rendering[axis]} | {socket.key: shell.input(panel, socket) for socket in parameters}, RESULT)
                            for axis in AXES
                            for level, *parameters in (group.group.interface.inputs,)
                        )
                    )
                    for group, panel in _CASES
                },
            )
            return Ok(directed(preserved(rendering, shaped, shell.input(_PRESERVE_HUE)), composed(inverted(spanned(outsets, triple)), to_xyz)))

        return rendering_basis(shell.config, self.space).bind(body)

    @property
    @override
    def prepared(self) -> Callable[[Stage], Outcome[Option[Prepared]]]:
        """CPU preparation of this record before its graph reads the input."""

        def prepared(stage: Stage) -> Outcome[Option[Prepared]]:
            """Reads the selected curve's scene white from the entering image while its manual white is off."""
            return _white_panel(stage.values).bind(lambda found: found.map(lambda panel: _read(stage, (panel,), Ok)).default_value(Ok(Nothing)))

        return prepared


class Hable(CompositorRecord, frozen=True, kw_only=True):
    """White-normalized Uncharted 2 response with independent authored coefficients, a metered white refused at zero contrast."""

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
        """Applies independent Hable coefficients normalized at the live white, the response held in log magnitude and sign past middle tones."""
        a, b, c, d, e, f = map(shell.input, (_A, _B, _C, _D, _E, _F))
        stops = shell.input(_WHITE_STOPS)
        white = 2.0**stops
        slope_raw, intercept_raw = a * (f - e), b * (c * f - e)
        normalizer = math("MULTIPLY_ADD", white, slope_raw, intercept_raw)
        slope, intercept = slope_raw / normalizer, intercept_raw / normalizer

        def binary(operand: Expr) -> Expr:
            return math("LOGARITHM", operand, 2.0)

        slope_log, slope_sign, intercept_log, intercept_sign = binary(abs(slope)), math("SIGN", slope), binary(abs(intercept)), math("SIGN", intercept)
        denominator = math("MULTIPLY_ADD", white, math("MULTIPLY_ADD", a, white, b), d * f)

        def response(level: Expr) -> Expr:
            delta, logarithm = level - white, binary(level)
            sloped = logarithm + slope_log
            crossed = intercept_log > sloped
            larger, smaller = where(crossed, intercept_log, sloped), where(crossed, sloped, intercept_log)
            summed = math("MULTIPLY_ADD", slope_sign * intercept_sign, 2.0 ** (smaller - larger), 1.0)
            present = abs(intercept_sign)
            factor = math("MULTIPLY_ADD", delta, slope, 1.0)
            centered = level > white * 0.5
            factor_log = where(centered, binary(abs(factor)), where(present, larger + binary(summed), sloped))
            factor_sign = where(centered, math("SIGN", factor), where(present, where(crossed, intercept_sign, slope_sign), slope_sign))
            magnitude = where(centered, abs(factor), where(present, summed, 1.0))
            scale = math("MAXIMUM", level, d)
            scaled = math("MULTIPLY_ADD", level / scale, math("MULTIPLY_ADD", a, level, b), d / scale * f)
            low_log = logarithm - stops + factor_log + (binary(denominator) - (binary(scale) + binary(scaled)))
            low = where((level > 0.0) * (magnitude > 0.0), 2.0**low_log * factor_sign, 0.0)
            inverse = 1.0 / level
            weight = where(centered, math("MULTIPLY_ADD", delta / level, slope, inverse), math("MULTIPLY_ADD", intercept, inverse, slope))
            high = weight * denominator / (white * math("MULTIPLY_ADD", d * f, inverse * inverse, math("MULTIPLY_ADD", b, inverse, a)))
            return where(level > 1.0, high, low)

        lifted = nonnegative(shell.color)
        return Ok(xyz(*(response(lifted[axis]) for axis in AXES)))

    @property
    @override
    def prepared(self) -> Callable[[Stage], Outcome[Option[Prepared]]]:
        """CPU preparation of this record before its graph reads the input."""

        def prepared(stage: Stage) -> Outcome[Option[Prepared]]:
            """Reads the Hable scene white from the entering image while its manual white is off, refusing a white holding no contrast."""
            return _read(stage, (), partial(_contrasted, stage.values))

        return prepared


class Neutral(CompositorRecord, frozen=True, kw_only=True):
    """Khronos PBR Neutral preserving base colors and compressing highlights toward white."""

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
        """Builds Khronos PBR Neutral over straight linear Rec.709 at its published F90 reflectance, 0.8 compression start, and 0.15 desaturation."""
        reflectance, compression, desaturation = 0.04, 0.8 - 0.04, 0.15
        positive = nonnegative(shell.color)
        lowest = extreme("MINIMUM", positive)
        shifted = positive - where(lowest < 2.0 * reflectance, lowest * math("MULTIPLY_ADD", lowest, -1.0 / (4.0 * reflectance), 1.0), reflectance)
        peak = extreme("MAXIMUM", shifted)
        compressed = 1.0 - (1.0 - compression) ** 2 / (peak + (1.0 - 2.0 * compression))
        fade = 1.0 - 1.0 / math("MULTIPLY_ADD", peak - compressed, desaturation, 1.0)
        return Ok(where(peak < compression, shifted, mixed(fade, vector("SCALE", shifted, compressed / peak), compressed)))

    @property
    @override
    def operation_space(self) -> str:
        """Colorspace in which this effect operates."""
        return REC709_SCENE


class Photoreceptor(CompositorRecord, frozen=True, kw_only=True):
    """Reinhard and Devlin's response against frame adaptation level."""

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
        """Runs the R/D Photoreceptor Tonemap node, a zero contrast read from the frame's luminance range."""
        return Ok(
            Expr(
                kind=bpy.types.CompositorNodeTonemap,
                inputs=(("Image", shell.color), ("Type", "R/D Photoreceptor"), *((stock.input, shell.input(socket)) for socket in _ADAPTATION for stock in socket.stock.to_list())),
                socket="Image",
                socket_type=bpy.types.NodeSocketColor,
            )
        )


class Tony(CompositorRecord, frozen=True, kw_only=True):
    """Tony McMapface's upstream colorspace in linear Rec.709."""

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
        """Samples the upstream colorspace, merged into the scene config and decoded to display-linear, at a table of its grid size over its logarithmic allocation."""
        with ExitStack() as scope:
            return (
                catch(scope.enter_context, exception=OSError)(as_file(UPSTREAM))
                .map_error(partial(Fault.of, Reason.UNREADABLE, str(UPSTREAM)))
                .bind(lambda path: ConfigBuild(sources=(path,)).absorbed(shell.config))
                .bind(lambda config: formed(config).bind(lambda held: latticed(item_key(Tony), "Tony McMapface", config, *held, shell.color)))
            )

    @property
    @override
    def operation_space(self) -> str:
        """Colorspace in which this effect operates."""
        return REC709_SCENE


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CURVES]


@grouped(kind=bpy.types.CompositorNodeTree)
def sigmoid(
    level: Expr,
    contrast: Annotated[Expr | float, msgspec.Meta(ge=0.1, le=10.0, description="Slope at middle gray, setting compressed scene range")] = 1.5,
    skew: Annotated[Expr | float, msgspec.Meta(ge=-1.0, le=1.0, description="Contrast moved into shadows below 0 and highlights above 0")] = 0.0,
    display_white: Annotated[Expr | float, msgspec.Meta(ge=0.2, le=16.0, description="Display value approached at infinity, 1 at reference white")] = 1.0,
    display_black: Annotated[Expr | float, msgspec.Meta(ge=0.0, le=0.15, description="Display value at scene zero")] = 0.000152,
) -> Expr:
    """Log-logistic response pinning scene zero to black and middle gray to itself."""
    skewed = math("POWER", 0.2, skew)
    inverse = 1.0 / skewed
    gray = math("POWER", GRAY / display_white, inverse)
    relation = (1.0 - gray) / gray
    film = contrast * (1.0 - GRAY) / (skewed * (1.0 - gray))
    floor = math("POWER", display_black / display_white, inverse)
    root = (relation * floor / (1.0 - floor)) ** (1.0 / film)
    fog = root * GRAY / (1.0 - root)
    relative = (fog + math("MAXIMUM", level, 0.0)) / (fog + GRAY)
    rising = relative**film
    ratio = where(relative > 1.0, 1.0 / math("MULTIPLY_ADD", relation, relative ** (film * -1.0), 1.0), rising / (relation + rising))
    return ratio**skewed * display_white


@grouped(kind=bpy.types.CompositorNodeTree)
def piecewise_power(
    level: Expr,
    toe_strength: Annotated[Expr | float, msgspec.Meta(ge=0.0, le=1.0, description="Toe depth below linear segment")] = 0.5,
    toe_length: Annotated[Expr | float, msgspec.Meta(ge=0.0, le=1.0, description="Lower range the toe spans, applied as its 2.2 power")] = 0.5,
    shoulder_strength: Annotated[Expr | float, msgspec.Meta(ge=0.0, le=16.0, description="Stops of highlight range the shoulder holds above white")] = 2.0,
    shoulder_length: Annotated[Expr | float, msgspec.Meta(ge=1e-5, le=1.0, description="Upper range the shoulder spans")] = 0.5,
    shoulder_angle: Annotated[Expr | float, msgspec.Meta(ge=0.0, le=1.0, description="Shoulder overshoot past white")] = 1.0,
    gamma: Annotated[Expr | float, msgspec.Meta(ge=0.1, le=10.0, description="Linear segment power")] = 1.0,
) -> Expr:
    """Hable's toe, linear, and shoulder segments normalized at their white point."""
    toe_end = math("POWER", toe_length, 2.2) * 0.5
    toe_height = (1.0 - toe_strength) * toe_end
    span = (1.0 - shoulder_length) * (1.0 - toe_height)
    white = toe_end + (1.0 - toe_height) + (math("POWER", 2.0, shoulder_strength) - 1.0)
    overshoot = shoulder_angle * shoulder_strength
    normalized, toe_x, shoulder_x = level / white, toe_end / white, (toe_end + span) / white
    line = math("MULTIPLY_ADD", white, normalized, toe_height - toe_end) ** gamma
    toe_level, shoulder_level = (math("MAXIMUM", height**gamma, 1e-5) for height in (toe_height, toe_height + span))
    toe_slope, shoulder_slope = (gamma * white * level ** (1.0 - 1.0 / gamma) for level in (toe_level, shoulder_level))
    ceiling = math("POWER", overshoot * 0.5 + 1.0, gamma)
    toe = (normalized / toe_x) ** (toe_slope * toe_x / toe_level) * toe_level
    origin = overshoot * 2.0 + 1.0
    run, rise = origin - shoulder_x, ceiling - shoulder_level
    exponent = shoulder_slope * run / rise

    def shoulder(position: Expr | float) -> Expr:
        return ceiling - ((origin - position) / run) ** exponent * rise

    shaped = where(normalized > 0.0, where(normalized < toe_x, toe, where(normalized < shoulder_x, line, where(normalized < origin, shoulder(normalized), ceiling))), 0.0)
    return shaped / shoulder(1.0)


@grouped(kind=bpy.types.CompositorNodeTree)
def logarithmic(
    level: Expr,
    bias: Annotated[Expr | float, msgspec.Meta(ge=0.5, le=1.0, description="Logarithm base shape between 2 at black and 10 at scene white")] = 0.85,
    display_white: Annotated[Expr | float, msgspec.Meta(ge=0.2, le=16.0, description="Display value at scene white, 1 at reference white")] = 1.0,
    scene_white_stops: Expr | float = _DRAGO_WHITE,
) -> Expr:
    """Drago's adaptive logarithm mapping scene white to display white."""
    white, clipped = math("POWER", 2.0, scene_white_stops), math("MAXIMUM", level, 0.0)

    def natural(operand: Expr | float) -> Expr:
        lifted = operand + 1.0
        delta = lifted - 1.0
        return where(delta > 0.0, math("LOGARITHM", lifted, np.e) * (operand / delta), operand)

    base = math("MULTIPLY_ADD", (clipped / white) ** math("LOGARITHM", bias, 0.5), 8.0, 2.0)
    return natural(clipped) / (natural(white) / float(np.log(10.0)) * math("LOGARITHM", base, np.e)) * display_white


@grouped(kind=bpy.types.CompositorNodeTree)
def reinhard_extended(level: Expr, scene_white_stops: Expr | float = 0.0) -> Expr:
    """Reinhard's photographic response with a finite scene white mapping to one."""
    return level / (level + 1.0) * (level / math("POWER", 2.0, scene_white_stops * 2.0) + 1.0)


# --- [TONY]


def formed(config: PyOpenColorIO.Config) -> Outcome[tuple[PyOpenColorIO.GroupTransform, Lattice]]:
    """Returns the transform from linear Rec.709 through the merged colorspace to display-linear, and the table of its grid size over its logarithmic allocation."""
    space: PyOpenColorIO.ColorSpace = config.getColorSpace(TONY)
    stated: PyOpenColorIO.GroupTransform = space.getTransform(PyOpenColorIO.COLORSPACE_DIR_FROM_REFERENCE)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    stages = tuple(inner for outer in stated for inner in (outer if isinstance(outer, PyOpenColorIO.GroupTransform) else (outer,)))
    low, high = next(stage.getVars() for stage in stages if isinstance(stage, PyOpenColorIO.AllocationTransform))
    encoding = next(stage.getDst() for stage in stages if isinstance(stage, PyOpenColorIO.ColorSpaceTransform))
    transform = PyOpenColorIO.GroupTransform([PyOpenColorIO.ColorSpaceTransform(src=REC709_SCENE, dst=TONY), PyOpenColorIO.ColorSpaceTransform(src=encoding, dst=REC709_SCENE)])
    return processor(config, transform).map(
        lambda built: (
            transform,
            Lattice(size=next(stage.getGridSize() for stage in built.createGroupTransform() if isinstance(stage, PyOpenColorIO.Lut3DTransform)), spacing=Logarithmic(low=low, span=high - low)),
        )
    )


# --- [PANELS]


def _white(stops: Value, /, *, toggle: bool) -> InterfacePanel:
    """Scene white panel at a default white in stops, its toggle starting on or off."""
    return msgspec.structs.replace(_SCENE_WHITE, toggle=Some(toggle)).defaulted({_WHITE_STOPS.key: stops})


def _cased[**P](group: Group[P]) -> InterfacePanel:
    """Case panel of a curve group named and described by its body, holding the parameters past the channel level and the scene white panel of a curve reading one."""
    _, *parameters = group.group.interface.inputs
    return InterfacePanel.of(
        group.body.__name__,
        Interface(
            inputs=tuple(socket for socket in parameters if socket.key != _WHITE_STOPS.key),
            panels=tuple(_white(socket.default_value, toggle=False) for socket in parameters if socket.key == _WHITE_STOPS.key),
        ),
        description=inspect.getdoc(group.body),
    )


# --- [WHITE]


def _measured(config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32]) -> Outcome[float]:
    """Returns the scene white in stops from 1.0 read from covered scene-linear pixels, refusing a frame with no finite white."""
    refused = partial(Fault.of, Reason.OUTSIDE, "White metering", member="covered pixels holding finite luminance")
    return (
        Samples
        .of(config, plane)
        .bind(lambda samples: samples.extent().to_result_with(partial(refused, str(plane.shape))))
        .bind(lambda extent: catch(log2, exception=ValueError)(extent.white).map_error(refused))
    )


def _contrasted(values: Mapping[Key, Value], stops: float) -> Outcome[float]:
    """Keeps a Hable white in stops whose normalization holds nonzero contrast under the coefficients the values hold."""
    white = 2.0**stops
    return (
        accumulated(value(values, float, socket) for socket in (_A, _B, _C, _E, _F))
        .filter_with(
            lambda held: white * held[0] * (held[4] - held[3]) + held[1] * (held[2] * held[4] - held[3]) != 0.0,
            lambda _: Fault.of(Reason.OUTSIDE, path_key(_SCENE_WHITE), str(stops), member="scene white holding nonzero contrast"),
        )
        .map(lambda _: stops)
    )


def _white_panel(values: Mapping[Key, Value]) -> Outcome[Option[InterfacePanel]]:
    """Case panel of the selected curve when it reads a scene white."""
    return value(values, str, _CURVE).map(lambda name: Option.of_optional(_WHITENED.get(name)))


def _read(stage: Stage, prefix: tuple[InterfacePanel, ...], kept: Callable[[float], Outcome[float]]) -> Outcome[Option[Prepared]]:
    """Reads the scene white under a panel prefix from the entering image while its manual white is off, keeping the stops `kept` accepts."""
    return value(stage.values, bool, *prefix, _SCENE_WHITE).bind(
        lambda manual: Ok(Nothing) if manual else pixels(stage.image).bind(partial(_measured, stage.config)).bind(kept).map(lambda stops: Some({path_key(*prefix, _WHITE_STOPS): stops}))
    )


def meter_white(identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, file: ImageFile) -> Outcome[bool]:
    """Measure the curve's scene white from stage input."""
    config = PyOpenColorIO.GetCurrentConfig()

    def manual(prefix: tuple[InterfacePanel, ...], stops: float) -> Outcome[bool]:
        return entered(identity, entry, {path_key(*prefix, _SCENE_WHITE): True, path_key(*prefix, _WHITE_STOPS): stops})

    def curved(values: Mapping[Key, Value]) -> Outcome[bool]:
        return (
            _white_panel(values)
            .bind(lambda found: found.to_result_with(partial(Fault.of, Reason.ABSENT, entry.name, member="curve reading a scene white")))
            .bind(lambda panel: _measured(config, file.pixels).bind(partial(manual, (panel,))))
        )

    def hable_white(values: Mapping[Key, Value]) -> Outcome[bool]:
        return _measured(config, file.pixels).bind(partial(_contrasted, values)).bind(partial(manual, ()))

    return captured(identity, entry).map(valued).bind(hable_white if entry in hable.entries(identity, scene) else curved)


# --- [COMPOSITION] ----------------------------------------------------------------------

_CASES: Final = tuple((group, _cased(group)) for group in (sigmoid, piecewise_power, logarithmic, reinhard_extended))
_WHITENED: Final = MappingProxyType({panel.item.name: panel for _, panel in _CASES if panel.panels})
_CURVE: Final = Socket(
    key="curve", kind=bpy.types.NodeSocketMenu, description="Curve each rendering channel takes", default_value=_CASES[0][1].item.name, items=tuple(panel.item for _, panel in _CASES)
)
_METER: Final = READ.measuring(meter_white)

tone_curve: Final = CompositorEffect(
    record=ToneCurve, interface=Interface(inputs=(_CURVE,), panels=(*(panel for _, panel in _CASES), _APPLICATION)), mix=Mix.STRAIGHT, pointwise=True, registers=(_METER,)
)
hable: Final = CompositorEffect(
    record=Hable, interface=Interface(inputs=(_A, _B, _C, _D, _E, _F), panels=(_white(float(np.log2(11.2)), toggle=True),)), mix=Mix.STRAIGHT, pointwise=True, registers=(_METER,)
)
neutral: Final = CompositorEffect(record=Neutral, mix=Mix.STRAIGHT, pointwise=True)
photoreceptor: Final = CompositorEffect(record=Photoreceptor, interface=Interface(inputs=_ADAPTATION), mix=Mix.RESULT)
tony: Final = CompositorEffect(record=Tony, mix=Mix.STRAIGHT, pointwise=True, registers=(TILES,))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TONY", "UPSTREAM", "Hable", "Neutral", "Photoreceptor", "ToneCurve", "Tony", "formed", "hable", "neutral", "photoreceptor", "tone_curve", "tony"]
