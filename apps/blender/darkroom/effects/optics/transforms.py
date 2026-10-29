"""Corner pin, perspective tilt, affine placement, and seeded camera shake of the frame, trailed across the scene shutter between posed endpoints."""

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
from blender.bpy.nodes.graphs import composed, Expr, inverted, math, switched, transformed, vector, xyz
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, Socket, under
from blender.bpy.render.compositor.coordinates import Border, BORDERS, covered, dimensions, entering, extended, pinned, placement, Projective, sampled
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, precise
from blender.bpy.render.compositor.inputs import captured, Derives, FRAME_SCALE, valued
from blender.bpy.render.compositor.layers import AFFINE
from blender.bpy.render.compositor.lenses import lensed, reaimed
from blender.bpy.render.compositor.motion import clocked, LENGTH, OPENING, origin, shutter, ShutterPoses, trailed
from blender.bpy.render.compositor.shells import PreviewFrame, Shell
from blender.bpy.scene.cameras import camera_type
from blender.imaging.geometry.frames import Homography, Quad
from blender.imaging.sampling import tiles
from blender.records.faults import accumulated, Fault, Outcome, RecordReason

from ...chain.stages import Effect, Stage
from .procedural import CLOCK

# --- [CONSTANTS] ------------------------------------------------------------------------

_CORNERS: Final = ("top_left", "top_right", "bottom_right", "bottom_left")
_ROWS: Final = tuple(Socket(key=key, kind=bpy.types.NodeSocketVector, description="Corner pin homography row") for key in ("first", "second", "third"))
_TRAVEL: Final = Socket(key="travel", kind=bpy.types.NodeSocketVector2D, description="Translation across exposure", default_value=(0.0, 0.0))
_TURN: Final = Socket(key="turn", kind=bpy.types.NodeSocketFloatAngle, description="Turn across exposure")
_ZOOM: Final = Socket(key="zoom", kind=bpy.types.NodeSocketFloat, description="Zoom in stops across exposure")
_EXTENSION: Final = Socket(key="extension", kind=bpy.types.NodeSocketMenu, description="Source sampling past frame edges", default_value=Border.BLACK.item.name, items=Some(BORDERS))
_MOTION_BLUR: Final = Socket(key="motion_blur", kind=bpy.types.NodeSocketBool, description="Integrate travel and timed shake across exposure")
_CORNER_SOCKETS: Final = tuple(
    Socket(key=item_key("corner_pin", corner), name=bpy.path.display_name(corner), kind=bpy.types.NodeSocketVector2D, description="Image quad corner", default_value=point)
    for corner, point in zip(_CORNERS, Quad().corners)
)
_CORNER_PIN: Final = InterfacePanel(key="corner_pin", name="Corner Pin", description="Frame corners placed before tilt and placement", toggle=Some(False), inputs=_CORNER_SOCKETS)
_HEADING: Final = Socket(key="perspective_heading", name="Heading", kind=bpy.types.NodeSocketFloatAngle, description="Rightward turn of view", min_value=-pi, max_value=pi)
_ELEVATION: Final = Socket(key="perspective_elevation", name="Elevation", kind=bpy.types.NodeSocketFloatAngle, description="Upward tilt of view", min_value=-pi / 2.0, max_value=pi / 2.0)
_PLACED: Final = under("placement", AFFINE).inputs
_SHAKE_POSITION: Final = Socket(key="shake_position", name="Position", kind=bpy.types.NodeSocketVector2D, description="Peak offset per frame axis", min_value=0.0, default_value=(0.01, 0.01))
_SHAKE_ROTATION: Final = Socket(key="shake_rotation", name="Rotation", kind=bpy.types.NodeSocketFloatAngle, description="Peak turn about frame center", min_value=0.0, default_value=radians(5.0))
_SHAKE_SCALE: Final = Socket(key="shake_scale", name="Scale", kind=bpy.types.NodeSocketFloat, description="Peak zoom in stops", min_value=0.0, default_value=log2(1.1))
_SHAKE_NOISE: Final = MappingProxyType({
    socket.key: msgspec.structs.replace(socket, key=item_key("shake", "noise", socket.key), name=bpy.path.display_name(item_key("noise", socket.key)))
    for socket in (
        Socket(key="scale", kind=bpy.types.NodeSocketFloat, description="Motion noise features per clock unit", min_value=-1000.0, max_value=1000.0, default_value=1.0),
        Socket(key="detail", kind=bpy.types.NodeSocketFloat, description="Motion noise octaves past first", min_value=0.0, max_value=15.0, default_value=4.0),
        Socket(key="roughness", kind=bpy.types.NodeSocketFloatFactor, description="Motion noise octave amplitude over previous", min_value=0.0, max_value=1.0, default_value=0.5),
        Socket(key="lacunarity", kind=bpy.types.NodeSocketFloat, description="Motion noise octave frequency over previous", min_value=0.0, max_value=1000.0, default_value=2.0),
        Socket(key="distortion", kind=bpy.types.NodeSocketFloat, description="Motion noise domain warp", min_value=-1000.0, max_value=1000.0),
    )
})
_SHAKE_SEED: Final = Socket(key="shake_seed", name="Seed", kind=bpy.types.NodeSocketInt, description="Number selecting pattern", min_value=0, max_value=2**24)
_SHAKE_CLOCK: Final = msgspec.structs.replace(
    under("shake", CLOCK),
    description="Shake clock, continuous within exposure",
    inputs=tuple(msgspec.structs.replace(socket, default_value=1.0008) if socket.key == "shake_clock_speed" else socket for socket in under("shake", CLOCK).inputs),
)
_SHAKE: Final = InterfacePanel(
    key="shake",
    name="Shake",
    description="Seeded camera motion over time",
    toggle=Some(False),
    inputs=(_SHAKE_POSITION, _SHAKE_ROTATION, _SHAKE_SCALE, *_SHAKE_NOISE.values(), _SHAKE_SEED),
    panels=(_SHAKE_CLOCK,),
)
_FACE: Final = Interface(
    inputs=(_TRAVEL, _TURN, _ZOOM, _EXTENSION, _MOTION_BLUR),
    panels=(
        _CORNER_PIN,
        InterfacePanel(key="perspective", name="Perspective", description="Turn of scene camera viewing frame", inputs=(_HEADING, _ELEVATION)),
        InterfacePanel(key="placement", name="Placement", description="Frame scale, shear, rotation, position, and pivot", inputs=_PLACED),
        _SHAKE,
    ),
)
_POSES: Final = ShutterPoses(
    sockets=tuple(
        socket
        for _, item, _, _ in _FACE.walked()
        if isinstance(item, tuple)
        for socket in (item[0],)
        if socket.base in {bpy.types.NodeSocketFloat, bpy.types.NodeSocketInt, bpy.types.NodeSocketVector} and socket not in _CORNER_SOCKETS
    )
)
_POSED: Final = MappingProxyType({socket.key: socket for socket in _POSES.poses})

# --- [MODELS] ---------------------------------------------------------------------------


class Transform(Effect, frozen=True, kw_only=True):
    """Pins, tilts, places, and shakes the frame, motion blurred across the scene shutter."""

    stage = Stage.WARP
    pointwise = False
    noise: Annotated[textures.Noise, msgspec.Meta(description="Shake noise over the clock, detail across scales")] = textures.Noise(noise_dimensions="1D", scale=1.0, detail=4.0)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [POSES]


def pin_quad(values: Mapping[Key, Value]) -> Option[Quad]:
    """Corner pin quad of entry input values while the pin is on."""
    match values.get(_CORNER_PIN.key), tuple(values.get(socket.key) for socket in _CORNER_SOCKETS):
        case True, ((float(a), float(b), *_), (float(c), float(d), *_), (float(e), float(f), *_), (float(g), float(h), *_)):
            return Some(Quad(corners=((a, b), (c, d), (e, f), (g, h))))
        case _:
            return Nothing


def rows(identity: Identity, _scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, _record: Transform) -> Outcome[Mapping[Key, Value]]:
    """Derives the corner pin homography rows, the identity while the pin is off or its corners admit no strictly convex map."""
    return (
        captured(identity, entry)
        .map(lambda snapshot: pin_quad(valued(snapshot)))
        .bind(lambda quad: quad.map(Homography.of).default_value(Ok(Nothing)))
        .map(lambda found: msgspec.structs.asdict(found.map(Projective.of).default_value(Projective())))
    )


def checked(identity: Identity, scene: bpy.types.Scene, entry: Option[bpy.types.SceneCompositorEffect], _record: Transform, _preview: Option[PreviewFrame]) -> Outcome[None]:
    """Refuses enabled corner pins whose corners turn both ways, and a tilted view through a camera other than perspective."""

    def refused(values: Mapping[Key, Value]) -> Outcome[None]:
        concave = pin_quad(values).filter(lambda quad: not quad.convex)
        return accumulated((
            concave.map(lambda quad: Error(Fault.of(RecordReason.OUTSIDE, "corner_pin", str(quad.corners), member="corners of a strictly convex quad"))).default_value(Ok(None)),
            camera_type("PERSP").check(bpy.context, Some(scene)) if values.get(_HEADING.key, 0.0) != 0.0 or values.get(_ELEVATION.key, 0.0) != 0.0 else Ok(None),
        )).map(lambda _: None)

    return entry.map(lambda held: captured(identity, held).map(valued).bind(refused)).default_value(Ok(None))


# --- [GRAPHS]


def reframed(shell: Shell[Transform]) -> Outcome[Expr]:
    """Samples the stage input through the current pose, or trails it between the posed shutter endpoints.

    Each pose maps camera frame fractions carrying geometric coverage in Z, inverse for sampling and forward for the trail endpoints.

    Returns:
        Warped stage input.
    """
    frame, lens, share = shell.space, lensed("Projection Matrix"), shell.input(FRAME_SCALE)
    first, second, third = (shell.input(socket) for socket in _ROWS)
    length, opening = shell.input(LENGTH), shell.input(OPENING)
    seconds = math("DIVIDE", Expr(kind=bpy.types.CompositorNodeSceneTime, socket="Seconds"), Expr(kind=bpy.types.CompositorNodeSceneTime, socket="Frame"))
    speed, hold = _SHAKE_CLOCK.inputs
    position, rotation, scale, skew, pivot = _PLACED

    def value(side: int | None, socket: Socket) -> Expr:
        return shell.input(socket) if side is None else shell.input(_POSED[ShutterPoses.key(side, socket.key)])

    def pose(side: int | None) -> Expr:
        offset: Expr | float = 0.0 if side is None else opening + float(side)
        time = math("MULTIPLY_ADD", offset * length * seconds, value(side, speed), clocked(value(side, speed), value(side, hold)))
        noise = shell.record.noise

        def linked(member: MemberDescriptorType) -> Option[Expr]:
            return Option.of_optional(_SHAKE_NOISE.get(member.__name__)).map(lambda socket: value(side, socket))

        x, y, angle, stops = (
            math("MULTIPLY_ADD", noise.node(0.0, time + origin(value(side, _SHAKE_SEED), float(tiles.stream(Transform, "shake", axis)))["X"], linked=Some(linked)), 2.0, -1.0) for axis in range(4)
        )
        gate = shell.input(_SHAKE)
        jitter = placement(
            frame,
            switched(gate, (0.0, 0.0, 0.0), vector("DIVIDE", vector("MULTIPLY", xyz(x, y), value(side, _SHAKE_POSITION)), frame.extent)),
            switched(gate, 0.0, angle * value(side, _SHAKE_ROTATION)),
            switched(gate, 1.0, 2.0 ** (stops * value(side, _SHAKE_SCALE))),
            (0.0, 0.0, 0.0),
            (0.5, 0.5, 0.0),
        )
        drift = placement(frame, vector("SCALE", value(side, _TRAVEL), offset), value(side, _TURN) * offset, 2.0 ** (value(side, _ZOOM) * offset), (0.0, 0.0, 0.0), value(side, pivot))
        placed = placement(frame, value(side, position), value(side, rotation), value(side, scale), value(side, skew), value(side, pivot))
        return composed(drift, composed(jitter, placed))

    def mapped(side: int | None, fraction: Expr, *, forward: bool = False) -> Expr:
        matrix = pose(side)

        def pinning(held: Expr, *, ahead: bool) -> Expr:
            return switched(shell.input(_CORNER_PIN), held, pinned(held, (first, second, third), forward=ahead))

        def tilting(held: Expr, *, ahead: bool) -> Expr:
            return reaimed(held, lens, value(side, _HEADING), value(side, _ELEVATION), share, forward=ahead)

        source = tilting(pinning(fraction, ahead=True), ahead=True) if forward else fraction
        moved = transformed(vector("MULTIPLY", source, frame.extent), matrix if forward else inverted(matrix))
        landed = vector("MULTIPLY_ADD", (0.0, 0.0, 1.0), source["Z"], vector("DIVIDE", moved, frame.extent))
        projected = landed if forward else pinning(tilting(landed, ahead=False), ahead=False)
        return vector(
            "MULTIPLY_ADD",
            (0.0, 0.0, 1.0),
            projected["Z"] * msgspec.structs.replace(inverted(matrix), socket="Invertible", shape=bpy.types.NodeSocketFloat),
            vector("MULTIPLY", projected, (1.0, 1.0, 0.0)),
        )

    start = vector("ADD", frame.fraction, (0.0, 0.0, 1.0))
    border = shell.input(_EXTENSION)
    blurred = shell.input(_MOTION_BLUR)
    uv = covered(frame, switched(blurred, mapped(None, start), mapped(0, start))).named("uv")
    image = extended(shell.color, uv, (border, border), clipped=True)
    center = (0.5, 0.5, 1.0)
    opened, stepped = (sampled(frame, mapped(1, mapped(0, entering(frame, at)), forward=True)) for at in (center, vector("ADD", center, vector("DIVIDE", (1.0, 0.0, 0.0), dimensions(shell.color)))))
    still = functools.reduce(
        lambda held, match: held * match,
        (
            switched(shell.input(_CORNER_PIN), 1.0, 0.0),
            math("COMPARE", abs(value(None, _HEADING)) + abs(value(None, _ELEVATION)), 0.0, 0.0),
            switched(shell.input(_SHAKE), 1.0, 0.0),
            *(math("COMPARE", abs(value(None, member)), 0.0, 0.0) for member in (_TURN, _ZOOM)),
            math("COMPARE", vector("LENGTH", value(None, _TRAVEL)), 0.0, 0.0),
        ),
    )
    return Ok(switched(blurred, image, switched(still, trailed(image, shell.color, opened, stepped), image)))


# --- [COMPOSITION] ----------------------------------------------------------------------

transform: Final = CompositorEffect(
    reframed,
    record=Transform,
    interface=_FACE,
    mix=Blended(alpha=Alpha.RESULT),
    request=Some(precise),
    check=Some(checked),
    derives=(
        Derives[Transform](of=rows, sockets=_ROWS),
        Derives[Transform](of=lambda identity, scene, entry, _record: _POSES.posed(identity, scene, entry), sockets=_POSES.poses, reads=shutter.reads),
    ),
    animates=Some(lambda identity, scene, entry, _record: _POSES.keyed(identity, scene, entry)),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Transform", "transform"]
