"""Frame warps: corner pin, perspective tilt, affine placement, and seeded camera shake trailed across the scene shutter, and mirror folds, tiling, polar conversions, and the perspective frame placed into a panorama."""

from collections.abc import Mapping
import functools
from math import log2, pi, radians
from types import MappingProxyType, MemberDescriptorType
from typing import Annotated, Final

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec

from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes import textures
from blender.bpy.nodes.desired import item_key, MenuItem, MenuItems
from blender.bpy.nodes.functions import EXPRESSIONS
from blender.bpy.nodes.graphs import composed, Expr, inverted, math, switched, transformed, vector, xyz
from blender.bpy.nodes.interfaces import Interface, InterfacePanel, Socket
from blender.bpy.render.compositor import motion
from blender.bpy.render.compositor.coordinates import Border, BORDERS, dimensions, extended, PIN_ROWS, pin_rows, pinned, shorter
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, precise
from blender.bpy.render.compositor.inputs import captured, DerivedInputs, FRAME_SCALE, GraphInputs, path_key, PreviewFrame, Step, value, valued
from blender.bpy.render.compositor.layers import AFFINE
from blender.bpy.render.compositor.lenses import camera_info, reaimed, seen, to_camera_space
from blender.bpy.render.compositor.motion import LENGTH, OPENING, origin, shutter, ShutterPoses, trailed
from blender.bpy.scene.cameras import camera_type
from blender.imaging.geometry.frames import Homography, Quad
from blender.imaging.geometry.panoramas import Equirectangular, folded, from_polar, Panoramas, to_polar
from blender.imaging.sampling import tiles
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
from blender.records.fields import Case

# --- [CONSTANTS] ------------------------------------------------------------------------

# --- [TRANSFORM]

_TRAVEL: Final = Socket(key="travel", kind=bpy.types.NodeSocketVector2D, description="Translation across exposure", default_value=(0.0, 0.0))
_TURN: Final = Socket(key="turn", kind=bpy.types.NodeSocketFloatAngle, description="Turn across exposure")
_ZOOM: Final = Socket(key="zoom", kind=bpy.types.NodeSocketFloat, description="Zoom in stops across exposure")
_EXTENSION: Final = Socket(key="extension", kind=bpy.types.NodeSocketMenu, description="Source sampling past frame edges", default_value=MenuItem.of(Border.BLACK).name, items=Some(BORDERS))
_MOTION_BLUR: Final = Socket(key="motion_blur", kind=bpy.types.NodeSocketBool, description="Integrate travel and timed shake across exposure")
_CORNERS: Final = tuple(
    Socket(key=corner, kind=bpy.types.NodeSocketVector2D, description="Image quad corner", default_value=point)
    for corner, point in zip(("top_left", "top_right", "bottom_right", "bottom_left"), Quad().corners, strict=True)
)
_CORNER_PIN: Final = InterfacePanel.of("corner_pin", Interface(inputs=_CORNERS), description="Frame corners placed before tilt and placement", toggle=Some(False))
_HEADING: Final = Socket(key="heading", kind=bpy.types.NodeSocketFloatAngle, description="Rightward turn of view", min_value=-pi, max_value=pi)
_ELEVATION: Final = Socket(key="elevation", kind=bpy.types.NodeSocketFloatAngle, description="Upward tilt of view", min_value=-pi / 2.0, max_value=pi / 2.0)
_PERSPECTIVE: Final = InterfacePanel.of("perspective", Interface(inputs=(_HEADING, _ELEVATION)), description="Turn of scene camera viewing frame")
_PLACEMENT: Final = InterfacePanel.of("placement", AFFINE, description="Frame scale, shear, rotation, position, and pivot")
_SHAKE_POSITION: Final = Socket(key="position", kind=bpy.types.NodeSocketVector2D, description="Peak offset per frame axis", min_value=0.0, default_value=(0.01, 0.01))
_SHAKE_ROTATION: Final = Socket(key="rotation", kind=bpy.types.NodeSocketFloatAngle, description="Peak turn about frame center", min_value=0.0, default_value=radians(5.0))
_SHAKE_SCALE: Final = Socket(key="scale", kind=bpy.types.NodeSocketFloat, description="Peak zoom in stops", min_value=0.0, default_value=log2(1.1))
_SHAKE_NOISE: Final = textures.Noise(noise_dimensions="1D", scale=1.0, detail=4.0)
_UNREAD: Final = frozenset(map(item_key, (textures.Noise.offset, textures.Noise.gain)))
_NOISE: Final = InterfacePanel.of(
    "noise", Interface(inputs=tuple(socket for socket in _SHAKE_NOISE.sockets if socket.key not in _UNREAD)), description="Fractal noise each shake axis reads over the clock"
)
_SHAKE: Final = InterfacePanel.of(
    "shake",
    Interface(inputs=(_SHAKE_POSITION, _SHAKE_ROTATION, _SHAKE_SCALE, motion.SEED), panels=(_NOISE, motion.CLOCK)),
    description="Seeded camera motion over time, continuous within exposure",
    toggle=Some(False),
)
_INTERFACE: Final = Interface(inputs=(_TRAVEL, _TURN, _ZOOM, _EXTENSION, _MOTION_BLUR), panels=(_CORNER_PIN, _PERSPECTIVE, _PLACEMENT, _SHAKE))
_POSES: Final = ShutterPoses(sockets=(_TRAVEL, _TURN, _ZOOM, *(item[0] for panel in (_PERSPECTIVE, _PLACEMENT, _SHAKE) for _, item, _, _ in panel.walked() if isinstance(item, tuple))))
_POSED: Final = MappingProxyType({socket.key: socket for socket in _POSES.poses})

# --- [PROJECTION]

_MIRRORED: Final = msgspec.structs.replace(_EXTENSION, default_value=MenuItem.of(Border.MIRROR).name)
_CENTER: Final = Socket(key="center", kind=bpy.types.NodeSocketVector2D, description="Point mirror lines pass through", default_value=(0.5, 0.5))
_ANGLE: Final = Socket(key="angle", kind=bpy.types.NodeSocketFloatAngle, description="First mirror line turn from horizontal", min_value=-pi, max_value=pi, default_value=pi / 2.0)
_MAGNIFICATION: Final = Socket(key="zoom", kind=bpy.types.NodeSocketFloat, description="Magnification about fold center", min_value=2.0**-15, max_value=2.0**15, default_value=1.0)
_TILE_CENTER: Final = Socket(key="center", kind=bpy.types.NodeSocketVector2D, description="Point copies scale about", default_value=(0.5, 0.5))
_COUNT: Final = Socket(key="count", kind=bpy.types.NodeSocketFloat, description="Frame copies across each side", min_value=1.0, default_value=3.0)
_TO_POLAR: Final = MenuItem(name="Rectangular to Polar")
_TO_RECTANGULAR: Final = MenuItem(name="Polar to Rectangular")
_CONVERSION: Final = Socket(
    key="conversion", kind=bpy.types.NodeSocketMenu, description="Conversion direction", default_value=_TO_POLAR.name, items=Some(MenuItems(items=(_TO_POLAR, _TO_RECTANGULAR)))
)
_KALEIDOSCOPE: Final = InterfacePanel.of("kaleidoscope", Interface(inputs=(_MIRRORED, _CENTER, _ANGLE, _MAGNIFICATION)), description="Mirror lines and zoom about one center")
_TILING: Final = InterfacePanel.of("tiling", Interface(inputs=(_MIRRORED, _TILE_CENTER, _COUNT)), description="Frame copies scaled about a center")
_POLAR: Final = InterfacePanel.of("polar", Interface(inputs=(_EXTENSION, _CONVERSION)), description="Full rectangular turn wrapped into the frame's inscribed disc or unwrapped from it")
_PANORAMIC: Final = InterfacePanel.of("panoramic", Interface(inputs=(_MIRRORED,)), description="Perspective frame sampled into a panorama projection")
_MODES: Final = Interface(panels=(_KALEIDOSCOPE, _TILING, _POLAR, _PANORAMIC))
_MODE: Final = Socket(key="mode", kind=bpy.types.NodeSocketMenu, description="Frame mapping", default_value=_KALEIDOSCOPE.title, items=Some(_MODES.menu))

# --- [MODELS] ---------------------------------------------------------------------------


class Transform(Case, frozen=True, kw_only=True):
    """Pins, tilts, places, and shakes the frame, motion blurred across the scene shutter."""


class Projection(Case, frozen=True, kw_only=True):
    """Mirror folds, tiling, polar conversions, or the rendered frame placed into a panorama."""

    mirrors: Annotated[int, msgspec.Meta(ge=1, description="Kaleidoscope mirror lines through center")] = 3
    panorama: Annotated[Panoramas, msgspec.Meta(description="Panorama receiving perspective frame")] = Equirectangular()


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [POSES]


def _pin_quad(values: Mapping[Key, Value]) -> Outcome[Option[Quad]]:
    """Corner pin quad of entry input values while the pin is on."""
    top_left, top_right, bottom_right, bottom_left = (value(values, tuple[float, float], _CORNER_PIN, corner) for corner in _CORNERS)
    corners = paired(top_left, top_right, bottom_right, bottom_left)
    return paired(value(values, bool, _CORNER_PIN), corners).map(lambda read: Some(Quad(corners=read[1])) if read[0] else Nothing)


def _pinned_rows(identity: Identity, _scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, _record: Case) -> Outcome[Mapping[Key, Value]]:
    """Derives the corner pin homography rows, the identity while the pin is off or its corners form no strictly convex map."""
    return captured(identity, entry).map(valued).bind(_pin_quad).bind(lambda quad: quad.map(Homography.of).default_value(Ok(Nothing))).map(pin_rows)


def _checked(identity: Identity, scene: bpy.types.Scene, entry: Option[bpy.types.SceneCompositorEffect], _record: Transform, _preview: Option[PreviewFrame]) -> Outcome[None]:
    """Refuses enabled corner pins whose corners turn both ways, and a tilted view through a camera other than perspective."""

    def refused(values: Mapping[Key, Value]) -> Outcome[None]:
        concave = _pin_quad(values).bind(
            lambda quad: (
                quad
                .filter(lambda held: not held.convex)
                .map(lambda held: Error(Fault.of(Reason.OUTSIDE, _CORNER_PIN.key, str(held.corners), member="corners of a strictly convex quad")))
                .default_value(Ok(None))
            )
        )
        turned = paired(value(values, float, _PERSPECTIVE, _HEADING), value(values, float, _PERSPECTIVE, _ELEVATION)).bind(
            lambda turn: camera_type("PERSP").check(bpy.context, Some(scene)) if turn != (0.0, 0.0) else Ok(None)
        )
        return accumulated((concave, turned)).map(lambda _: None)

    return entry.map(lambda held: captured(identity, held).map(valued).bind(refused)).default_value(Ok(None))


# --- [TRANSFORM]


def _reframed(shell: GraphInputs[Transform]) -> Outcome[Expr]:
    """Samples the stage input through the current pose, or trails it between the posed shutter endpoints.

    Each pose maps camera frame fractions holding geometric coverage in Z, inverse for sampling and forward for the trail endpoints.

    Returns:
        Warped stage input.
    """
    frame, lens, share = shell.space, camera_info("Projection Matrix"), shell.input(FRAME_SCALE)
    first, second, third = (shell.input(socket) for socket in PIN_ROWS)
    length, opening, rate = shell.input(LENGTH), shell.input(OPENING), shell.input(motion.RATE)
    position, rotation, scale, skew, pivot = AFFINE.inputs
    noises = {socket.key: socket for socket in _NOISE.inputs}
    gate = shell.input(_SHAKE)

    def value(side: int | None, *path: Step) -> Expr:
        return shell.input(*path) if side is None else shell.input(_POSED[ShutterPoses.key(side, path_key(*path))])

    def pose(side: int | None) -> Expr:
        offset: Expr | float = 0.0 if side is None else opening + float(side)
        shaken = functools.partial(value, side, _SHAKE)
        speed, hold = (shaken(motion.CLOCK.socket(field)) for field in (motion.Clock.speed, motion.Clock.hold))
        time = math("MULTIPLY_ADD", offset * length / rate, speed, motion.clock(speed, hold, rate))

        def linked(member: MemberDescriptorType) -> Option[Expr]:
            return Option.of_optional(noises.get(item_key(_NOISE.key, member))).map(shaken)

        x, y, angle, stops = (
            math("MULTIPLY_ADD", textures.evolving(_SHAKE_NOISE, xyz(time + origin(shaken(motion.SEED), float(tiles.stream(Transform, _SHAKE.key, axis)))["X"]), 0.0, linked=Some(linked)), 2.0, -1.0)
            for axis in range(4)
        )
        jitter = frame.placement(
            switched(gate, (0.0, 0.0, 0.0), vector("DIVIDE", vector("MULTIPLY", xyz(x, y), shaken(_SHAKE_POSITION)), frame.extent)),
            switched(gate, 0.0, angle * shaken(_SHAKE_ROTATION)),
            switched(gate, 1.0, 2.0 ** (stops * shaken(_SHAKE_SCALE))),
            (0.0, 0.0, 0.0),
            (0.5, 0.5, 0.0),
        )
        placed = functools.partial(value, side, _PLACEMENT)
        drift = frame.placement(vector("SCALE", value(side, _TRAVEL), offset), value(side, _TURN) * offset, 2.0 ** (value(side, _ZOOM) * offset), (0.0, 0.0, 0.0), placed(pivot))
        return composed(drift, composed(jitter, frame.placement(placed(position), placed(rotation), placed(scale), placed(skew), placed(pivot))))

    def mapped(side: int | None, fraction: Expr, *, forward: bool = False) -> Expr:
        matrix = pose(side)

        def pinning(held: Expr, *, ahead: bool) -> Expr:
            return switched(shell.input(_CORNER_PIN), held, pinned(held, (first, second, third), forward=ahead))

        def tilting(held: Expr, *, ahead: bool) -> Expr:
            return reaimed(held, lens, value(side, _PERSPECTIVE, _HEADING), value(side, _PERSPECTIVE, _ELEVATION), share, forward=ahead)

        source = tilting(pinning(fraction, ahead=True), ahead=True) if forward else fraction
        moved = transformed(vector("MULTIPLY", source, frame.extent), matrix if forward else inverted(matrix))
        target = vector("MULTIPLY_ADD", (0.0, 0.0, 1.0), source["Z"], vector("DIVIDE", moved, frame.extent))
        projected = target if forward else pinning(tilting(target, ahead=False), ahead=False)
        return vector("MULTIPLY_ADD", (0.0, 0.0, 1.0), projected["Z"] * inverted(matrix).at("Invertible"), vector("MULTIPLY", projected, (1.0, 1.0, 0.0)))

    start = vector("ADD", frame.fraction, (0.0, 0.0, 1.0))
    blurred = shell.input(_MOTION_BLUR)
    uv = frame.covered(switched(blurred, mapped(None, start), mapped(0, start))).keyed("uv")
    image = extended(shell.color, uv, shell.input(_EXTENSION), clipped=True)
    center = (0.5, 0.5, 1.0)
    opened, stepped = (frame.sampled(mapped(1, mapped(0, frame.from_uv(at)), forward=True)) for at in (center, vector("ADD", center, vector("DIVIDE", (1.0, 0.0, 0.0), dimensions(shell.color)))))
    still = functools.reduce(
        lambda held, match: held * match,
        (
            switched(shell.input(_CORNER_PIN), 1.0, 0.0),
            math("COMPARE", abs(value(None, _PERSPECTIVE, _HEADING)) + abs(value(None, _PERSPECTIVE, _ELEVATION)), 0.0, 0.0),
            switched(gate, 1.0, 0.0),
            *(math("COMPARE", abs(value(None, member)), 0.0, 0.0) for member in (_TURN, _ZOOM)),
            math("COMPARE", vector("LENGTH", value(None, _TRAVEL)), 0.0, 0.0),
        ),
    )
    return Ok(switched(blurred, image, switched(still, trailed(image, shell.color, opened, stepped), image)))


# --- [PROJECTION]


def _projected(shell: GraphInputs[Projection]) -> Outcome[Expr]:
    """Builds every mode's covered sample and keeps the selected mode's."""
    record, frame, read = shell.record, shell.space, shell.input
    offset = vector("MULTIPLY_ADD", frame.extent, (-0.5, -0.5, 0.0), frame.point)
    short = shorter(frame.extent)
    unit = vector("ADD", frame.fraction, (0.0, 0.0, 1.0))["Z"]
    center = vector("MULTIPLY", read(_KALEIDOSCOPE, _CENTER), frame.extent)
    local = frame.point - center
    x, y = folded(local["X"], local["Y"], mirrors=record.mirrors, angle=read(_KALEIDOSCOPE, _ANGLE), zoom=read(_KALEIDOSCOPE, _MAGNIFICATION), ns=EXPRESSIONS)
    s, t, _ = to_polar((offset["X"] / short + 0.5, offset["Y"] / short + 0.5, unit), EXPRESSIONS)
    u, v, _ = from_polar((frame.fraction["X"], frame.fraction["Y"], unit), EXPRESSIONS)
    unwrapped = vector("MULTIPLY_ADD", vector("DIVIDE", xyz(u - 0.5, v - 0.5), frame.extent), xyz(short, short, 0.0), (0.5, 0.5, 0.0))
    across, along = (offset["X"], offset["Y"]) if record.panorama.centered else (frame.fraction["X"], frame.fraction["Y"])
    rx, ry, rz, coverage = record.panorama.to_ray((across, along, unit), EXPRESSIONS)
    up = to_camera_space((0.0, 0.0, 1.0))
    level = Expr(
        kind=bpy.types.FunctionNodeEulerToRotation,
        inputs=(("Euler", xyz(-math("ARCTAN2", -up["Z"], vector("LENGTH", xyz(up["X"], up["Y"]))), 0.0, -math("ARCTAN2", up["X"], up["Y"]))),),
        socket="Rotation",
    )
    ray = xyz(rx, ry, rz)
    turned = Expr(kind=bpy.types.FunctionNodeRotateVector, inputs=(("Vector", ray), ("Rotation", level)), socket="Vector", socket_type=bpy.types.NodeSocketVector) if record.panorama.upright else ray
    tiling, tiled = read(_TILING, _TILE_CENTER), read(_TILING, _COUNT)
    sampled = {
        _KALEIDOSCOPE: frame.sampled(vector("DIVIDE", xyz(x, y) + center, frame.extent)),
        _TILING: frame.sampled(vector("SCALE", frame.fraction - tiling, tiled) + tiling),
        _POLAR: shell.menu(_POLAR, _CONVERSION, cases={_TO_POLAR: frame.sampled(xyz(s, t)), _TO_RECTANGULAR: frame.sampled(unwrapped)}),
        _PANORAMIC: seen(turned, camera_info("Projection Matrix"), coverage),
    }
    return Ok(shell.menu(_MODE, cases={panel.item: extended(shell.color, uv, read(panel, _EXTENSION), clipped=True) for panel, uv in sampled.items()}))


# --- [COMPOSITION] ----------------------------------------------------------------------

transform: Final = CompositorEffect(
    _reframed,
    record=Transform,
    interface=_INTERFACE,
    mix=Blended(alpha=Alpha.RESULT),
    request=Some(precise),
    check=Some(_checked),
    derives=(
        motion.timing,
        DerivedInputs[Case](of=_pinned_rows, sockets=PIN_ROWS),
        DerivedInputs[Case](of=lambda identity, scene, entry, _record: _POSES.posed(identity, scene, entry), sockets=_POSES.poses, reads=shutter.reads),
    ),
    animates=Some(lambda identity, scene, entry, _record: _POSES.keyed(identity, scene, entry)),
)
projection: Final = CompositorEffect(_projected, record=Projection, interface=Interface(inputs=(_MODE,), panels=_MODES.panels), mix=Blended(alpha=Alpha.RESULT), request=Some(precise))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Projection", "Transform", "projection", "transform"]
