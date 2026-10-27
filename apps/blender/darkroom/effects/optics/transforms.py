"""Corner pin, perspective tilt, affine placement, and seeded camera shake of the frame, trailed across the scene shutter between posed endpoints."""

from collections.abc import Mapping
import functools
from math import log2, pi, radians
from typing import Annotated, Final

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec

from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes import textures
from blender.bpy.nodes.graphs import Expr, math, switched, vector, xyz
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, Socket, under
from blender.bpy.render.compositor.colors import composed, inverted
from blender.bpy.render.compositor.coordinates import Border, BORDERS, clocked, covered, entering, extended, lensed, origin, pinned, placement, Projective, reaimed, sampled, ShutterPoses, trailed
from blender.bpy.render.compositor.effects import Alpha, Animates, Checks, CompositorEffect, Context, Derives, precise, Requests, Shell, shutter
from blender.bpy.render.compositor.inputs import captured, valued
from blender.bpy.render.compositor.layers import AFFINE
from blender.bpy.scene.cameras import camera_type
from blender.imaging.geometry.frames import Homography, Quad
from blender.records.faults import accumulated, Fault, Outcome, RecordReason

from ...chain.stages import Effect, Stage
from .procedural import CLOCK

# --- [CONSTANTS] ------------------------------------------------------------------------

_CORNERS: Final = ("top_left", "top_right", "bottom_right", "bottom_left")
_ROWS: Final = tuple(Socket(key=key, name=bpy.path.display_name(key), kind=bpy.types.NodeSocketVector, description="Corner pin homography row") for key in ("first", "second", "third"))
_FACE: Final = Interface(
    inputs=(
        Socket(key="travel", name="Travel", kind=bpy.types.NodeSocketVector, description="Translation across exposure", members={"dimensions": 2, "default_value": (0.0, 0.0)}),
        Socket(key="turn", name="Turn", kind=bpy.types.NodeSocketFloat, description="Turn across exposure", members={"subtype": "ANGLE"}),
        Socket(key="zoom", name="Zoom", kind=bpy.types.NodeSocketFloat, description="Zoom in stops across exposure"),
        Socket(
            key="extension", name="Extension", kind=bpy.types.NodeSocketMenu, description="Source sampling past frame edges", members={"default_value": Border.BLACK.item.name}, items=Some(BORDERS)
        ),
        Socket(key="motion_blur", name="Motion Blur", kind=bpy.types.NodeSocketBool, description="Integrate travel and timed shake across exposure"),
    ),
    panels=(
        InterfacePanel(
            key="corner_pin",
            name="Corner Pin",
            description="Frame corners placed before tilt and placement",
            toggle=Some(False),
            inputs=tuple(
                Socket(
                    key=item_key("corner_pin", corner),
                    name=bpy.path.display_name(corner),
                    kind=bpy.types.NodeSocketVector,
                    description="Image quad corner",
                    members={"dimensions": 2, "default_value": point},
                )
                for corner, point in zip(_CORNERS, Quad().corners)
            ),
        ),
        InterfacePanel(
            key="perspective",
            name="Perspective",
            description="Turn of scene camera viewing frame",
            inputs=tuple(
                Socket(
                    key=item_key("perspective", field),
                    name=bpy.path.display_name(field),
                    kind=bpy.types.NodeSocketFloat,
                    description=description,
                    members={"subtype": "ANGLE", "min_value": -bound, "max_value": bound},
                )
                for field, description, bound in (("heading", "Rightward turn of view", pi), ("elevation", "Upward tilt of view", pi / 2.0))
            ),
        ),
        InterfacePanel(key="placement", name="Placement", description="Frame scale, shear, rotation, position, and pivot", inputs=under("placement", AFFINE).inputs),
        InterfacePanel(
            key="shake",
            name="Shake",
            description="Seeded camera motion over time",
            toggle=Some(False),
            inputs=(
                Socket(
                    key="shake_position",
                    name="Position",
                    kind=bpy.types.NodeSocketVector,
                    description="Peak offset per frame axis",
                    members={"dimensions": 2, "min_value": 0.0, "default_value": (0.01, 0.01)},
                ),
                *(
                    Socket(key=item_key("shake", field), name=bpy.path.display_name(field), kind=kind, description=description, members={"subtype": subtype, **numbers})
                    for field, kind, subtype, description, numbers in (
                        ("rotation", bpy.types.NodeSocketFloat, "ANGLE", "Peak turn about frame center", {"min_value": 0.0, "default_value": radians(5.0)}),
                        ("scale", bpy.types.NodeSocketFloat, "NONE", "Peak zoom in stops", {"min_value": 0.0, "default_value": log2(1.1)}),
                        ("noise_scale", bpy.types.NodeSocketFloat, "NONE", "Motion noise features per clock frame", {"min_value": -1000.0, "max_value": 1000.0, "default_value": 1.0}),
                        ("noise_detail", bpy.types.NodeSocketFloat, "NONE", "Motion noise octaves past first", {"min_value": 0.0, "max_value": 15.0, "default_value": 4.0}),
                        ("noise_roughness", bpy.types.NodeSocketFloat, "FACTOR", "Motion noise octave amplitude over previous", {"min_value": 0.0, "max_value": 1.0, "default_value": 0.5}),
                        ("noise_lacunarity", bpy.types.NodeSocketFloat, "NONE", "Motion noise octave frequency over previous", {"min_value": 0.0, "max_value": 1000.0, "default_value": 2.0}),
                        ("noise_distortion", bpy.types.NodeSocketFloat, "NONE", "Motion noise domain warp", {"min_value": -1000.0, "max_value": 1000.0}),
                        ("seed", bpy.types.NodeSocketInt, "NONE", "Number selecting pattern", {"min_value": 0, "max_value": 2**24}),
                    )
                ),
            ),
            panels=(
                msgspec.structs.replace(
                    under("shake", CLOCK),
                    description="Shake clock, continuous within exposure",
                    inputs=tuple(
                        msgspec.structs.replace(socket, members={**socket.members, "default_value": 0.0417}) if socket.key == "shake_clock_speed" else socket for socket in under("shake", CLOCK).inputs
                    ),
                ),
            ),
        ),
    ),
)
_POSES: Final = ShutterPoses(
    sockets=tuple(
        socket
        for _, item, _, _ in _FACE.walked()
        if isinstance(item, tuple)
        for socket in (item[0],)
        if socket.kind in {bpy.types.NodeSocketFloat, bpy.types.NodeSocketInt, bpy.types.NodeSocketVector} and not socket.key.startswith("corner_pin_")
    )
)

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
    match values.get("corner_pin"), tuple(values.get(item_key("corner_pin", corner)) for corner in _CORNERS):
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


def checked(identity: Identity, scene: bpy.types.Scene, entry: Option[bpy.types.SceneCompositorEffect], _record: Transform, _context: Context) -> Outcome[None]:
    """Refuses enabled corner pins whose corners turn both ways, and a tilted view through a camera other than perspective."""

    def refused(values: Mapping[Key, Value]) -> Outcome[None]:
        concave = pin_quad(values).filter(lambda quad: not quad.convex)
        return accumulated((
            concave.map(lambda quad: Error(Fault.of(RecordReason.OUTSIDE, "corner_pin", str(quad.corners), member="corners of a strictly convex quad"))).default_value(Ok(None)),
            camera_type("PERSP").check(bpy.context, Some(scene)) if values.get("perspective_heading", 0.0) != 0.0 or values.get("perspective_elevation", 0.0) != 0.0 else Ok(None),
        )).map(lambda _: None)

    return entry.map(lambda held: captured(identity, held).map(valued).bind(refused)).default_value(Ok(None))


# --- [GRAPHS]


def transformed(shell: Shell[Transform]) -> Outcome[Expr]:
    """Samples the stage input through the current pose, or trails it between the posed shutter endpoints.

    Each pose maps camera frame fractions carrying geometric coverage in Z, inverse for sampling and forward for the trail endpoints.

    Returns:
        Warped stage input.
    """
    frame, lens, share = shell.space, lensed("Projection Matrix"), shell.input(Shell.frame, "scale")
    first, second, third = (shell.input(socket.key) for socket in _ROWS)
    length, opening = shell.input("length"), shell.input("opening")

    def value(side: int | None, *at: str) -> Expr:
        return shell.input(*at) if side is None else shell.input(ShutterPoses.key(side, item_key(*at)))

    def pose(side: int | None) -> Expr:
        def shaken(*at: str) -> Expr:
            return value(side, "shake", *at)

        offset: Expr | float = 0.0 if side is None else opening + float(side)
        time = math("MULTIPLY_ADD", offset * length, shaken("clock", "speed"), clocked(shaken("clock", "speed"), shaken("clock", "hold")))
        x, y, angle, stops = (
            math(
                "MULTIPLY_ADD",
                shell.record.noise.node(0.0, time + origin(shaken("seed"), float(textures.stream(Transform, "shake", axis)))["X"], linked=Some(lambda info: Some(shaken("noise", info.name)))),
                2.0,
                -1.0,
            )
            for axis in range(4)
        )
        gate = shell.input("shake")
        jitter = placement(
            frame,
            switched(gate, (0.0, 0.0, 0.0), vector("DIVIDE", vector("MULTIPLY", xyz(x, y), shaken("position")), frame.extent)),
            switched(gate, 0.0, angle * shaken("rotation")),
            switched(gate, 1.0, 2.0 ** (stops * shaken("scale"))),
            (0.0, 0.0, 0.0),
            (0.5, 0.5, 0.0),
        )
        drift = placement(
            frame, vector("SCALE", value(side, "travel"), offset), value(side, "turn") * offset, 2.0 ** (value(side, "zoom") * offset), (0.0, 0.0, 0.0), value(side, "placement", "pivot")
        )
        position, rotation, scale, skew, pivot = (value(side, socket.key) for socket in under("placement", AFFINE).inputs)
        return composed(drift, composed(jitter, placement(frame, position, rotation, scale, skew, pivot)))

    def mapped(side: int | None, fraction: Expr, *, forward: bool = False) -> Expr:
        matrix = pose(side)

        def pinning(held: Expr, *, ahead: bool) -> Expr:
            return switched(shell.input("corner_pin"), held, pinned(held, (first, second, third), forward=ahead))

        def tilting(held: Expr, *, ahead: bool) -> Expr:
            return reaimed(held, lens, value(side, "perspective", "heading"), value(side, "perspective", "elevation"), share, forward=ahead)

        source = tilting(pinning(fraction, ahead=True), ahead=True) if forward else fraction
        moved = Expr(
            kind=bpy.types.FunctionNodeTransformPoint,
            inputs=(("Vector", vector("MULTIPLY", source, frame.extent)), ("Transform", matrix if forward else inverted(matrix))),
            socket="Vector",
            shape="VECTOR",
        )
        landed = vector("MULTIPLY_ADD", (0.0, 0.0, 1.0), source["Z"], vector("DIVIDE", moved, frame.extent))
        projected = landed if forward else pinning(tilting(landed, ahead=False), ahead=False)
        return vector("MULTIPLY_ADD", (0.0, 0.0, 1.0), projected["Z"] * msgspec.structs.replace(inverted(matrix), socket="Invertible"), vector("MULTIPLY", projected, (1.0, 1.0, 0.0)))

    start = vector("ADD", frame.fraction, (0.0, 0.0, 1.0))
    border = shell.input("extension")
    blurred = shell.input("motion_blur")
    uv = covered(frame, switched(blurred, mapped(None, start), mapped(0, start))).named("uv")
    image = extended(shell.color, uv, (border, border), clipped=True)
    dimensions = Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", shell.color),), socket="Dimensions", shape="VECTOR")
    center = (0.5, 0.5, 1.0)
    opened, stepped = (sampled(frame, mapped(1, mapped(0, entering(frame, at)), forward=True)) for at in (center, vector("ADD", center, vector("DIVIDE", (1.0, 0.0, 0.0), dimensions))))
    still = functools.reduce(
        lambda held, match: held * match,
        (
            switched(shell.input("corner_pin"), 1.0, 0.0),
            math("COMPARE", abs(value(None, "perspective", "heading")) + abs(value(None, "perspective", "elevation")), 0.0, 0.0),
            switched(shell.input("shake"), 1.0, 0.0),
            *(math("COMPARE", abs(value(None, member)), 0.0, 0.0) for member in ("turn", "zoom")),
            math("COMPARE", vector("LENGTH", value(None, "travel")), 0.0, 0.0),
        ),
    )
    return Ok(switched(blurred, image, switched(still, trailed(image, shell.color, opened, stepped), image)))


# --- [COMPOSITION] ----------------------------------------------------------------------

transform = CompositorEffect(
    record=Transform,
    graph=transformed,
    interface=_FACE,
    alpha=Alpha.RESULT,
    parts=(
        Requests[Transform](request=precise),
        Checks[Transform](check=checked),
        Derives[Transform](of=rows, sockets=_ROWS),
        Derives[Transform](of=lambda identity, scene, entry, _record: _POSES.posed(identity, scene, entry), sockets=_POSES.poses, reads=shutter.reads),
        Animates[Transform](keys=lambda identity, scene, entry, _record: _POSES.keyed(identity, scene, entry)),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Transform", "transform"]
