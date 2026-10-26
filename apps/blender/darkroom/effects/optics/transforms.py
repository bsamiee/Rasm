"""Corner pin, perspective tilt, affine placement, and seeded camera shake of the frame, trailed across the scene shutter between posed endpoints."""

import functools
from math import log2, radians
from typing import Annotated, Final

import bpy
from expression import Error, Ok, Option, Some
import msgspec
import msgspec.inspect

from blender.bpy.data.rna.conditions import AllOf, Condition
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes import textures
from blender.bpy.nodes.graphs import End, Expr, group_input, math, Scalar, switched, vector, xyz
from blender.bpy.render.compositor.colors import composed, inverted
from blender.bpy.render.compositor.coordinates import Border, Clock, clocked, covered, entering, extended, lensed, origin, pinned, placement, Projective, reaimed, sampled, ShutterPoses, trailed
from blender.bpy.render.compositor.effects import Alpha, Animates, CameraFrame, Checks, CompositorEffect, Conditions, Context, Derives, Inspects, precise, Requests, Shell, shutter
from blender.bpy.render.compositor.inputs import Field
from blender.bpy.render.compositor.layers import Affine
from blender.bpy.scene.cameras import camera_type
from blender.bpy.settings.shapes import GraphForm, unwrapped
from blender.imaging.geometry.homography import Homography, Quad
from blender.imaging.geometry.regions import Position
from blender.records.faults import DomainFault
from blender.records.fields import Elevation, Finite, Gated, markers, NonNegative, Presentation, Quantity, Seed, Turn

from ...chain.stages import Effect, Stage

# --- [MODELS] ---------------------------------------------------------------------------


class Tilt(msgspec.Struct, frozen=True, kw_only=True):
    """Turn of a scene camera viewing its own frame."""

    heading: Annotated[Turn, msgspec.Meta(description="Rightward turn of view")] = 0.0
    elevation: Annotated[Elevation, msgspec.Meta(description="Upward tilt of view")] = 0.0


class Shake(msgspec.Struct, frozen=True, kw_only=True):
    """Seeded camera motion on position, rotation, and scale over time."""

    position: Annotated[tuple[NonNegative, NonNegative], msgspec.Meta(description="Peak offset per frame axis"), Presentation(soft=Some((0.0, 0.1)), quantity=Some(Quantity.FRAME))] = (0.01, 0.01)
    rotation: Annotated[NonNegative, msgspec.Meta(description="Peak turn about frame center"), Presentation(soft=Some((0.0, radians(10.0))), quantity=Some(Quantity.ANGLE))] = radians(5.0)
    scale: Annotated[NonNegative, msgspec.Meta(description="Peak zoom in stops"), Presentation(soft=Some((0.0, 1.0)))] = log2(1.1)
    clock: Annotated[Clock, msgspec.Meta(description="Shake clock, continuous within exposure")] = Clock(speed=1.0 / 24.0)
    noise: Annotated[textures.Noise, msgspec.Meta(description="Motion noise over the clock, detail across scales")] = textures.Noise(noise_dimensions="1D", scale=1.0, detail=4.0)
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting pattern")] = 0


class Transform(Effect, frozen=True, kw_only=True):
    """Pins, tilts, places, and shakes the frame, motion blurred across the scene shutter."""

    stage = Stage.WARP
    pointwise = False
    pin: Annotated[Gated[Quad], msgspec.Meta(title="Corner Pin", description="Frame corners placed before tilt and placement")] = Gated(enabled=False, value=Quad())
    tilt: Annotated[Tilt, msgspec.Meta(title="Perspective", description="Turn of scene camera viewing frame")] = Tilt()
    affine: Annotated[Affine, msgspec.Meta(title="Placement", description="Frame scale, shear, rotation, position, and pivot")] = Affine()
    travel: Annotated[Position, msgspec.Meta(description="Translation across exposure")] = (0.0, 0.0)
    turn: Annotated[Finite, msgspec.Meta(description="Turn across exposure"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    zoom: Annotated[Finite, msgspec.Meta(description="Zoom in stops across exposure")] = 0.0
    shake: Annotated[Gated[Shake], msgspec.Meta(description="Seeded camera motion over time")] = Gated(enabled=False, value=Shake())
    extension: Annotated[Border, msgspec.Meta(description="Source sampling past frame edges")] = Border.BLACK
    motion_blur: Annotated[bool, msgspec.Meta(description="Integrate travel and timed shake across exposure")] = False


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [POSES]


def _temporal(field: Field) -> bool:
    """Whether a field is a numeric leaf the shutter poses sample, the corner pin reaching the graph as derived rows."""
    pin, *_ = Transform.__struct_fields__

    def numeric(shape: msgspec.inspect.Type) -> bool:
        match unwrapped(shape):
            case msgspec.inspect.FloatType() | msgspec.inspect.IntType():
                return True
            case msgspec.inspect.TupleType(item_types=items):
                return all(map(numeric, items))
            case _:
                return False

    return field.path[0] != pin and not markers(field.info.type, GraphForm) and numeric(field.shape)


def rows(_identity: Identity, _scene: bpy.types.Scene, record: Transform) -> BlenderResult[Projective]:
    """Derives the corner pin homography rows, the identity while the pin is off or its corners admit no strictly convex map."""
    return Homography.of(record.pin.value).map(lambda found: found.map(Projective.of).default_value(Projective())) if record.pin.enabled else Ok(Projective())


def convex(_identity: Identity, _scene: bpy.types.Scene, _entry: Option[bpy.types.SceneCompositorEffect], record: Transform, _context: Context) -> BlenderResult[None]:
    """Refuses enabled corner pins whose corners turn both ways."""
    pin, *_ = Transform.__struct_fields__
    quad = record.pin.value
    return Ok(None) if not record.pin.enabled or quad.convex else Error((DomainFault(subject=pin, value=msgspec.structs.astuple(quad), accepted="corners of a strictly convex quad"),))


def aimed(record: Transform) -> Condition:
    """Requires a perspective camera while the tilt turns the view."""
    return camera_type("PERSP") if record.tilt.heading != 0.0 or record.tilt.elevation != 0.0 else AllOf(conditions=())


# --- [GRAPHS]


def transformed(shell: Shell[Transform]) -> BlenderResult[Expr]:
    """Samples the stage input through the current pose, or trails it between the posed shutter endpoints.

    Each pose maps camera frame fractions carrying geometric coverage in Z, inverse for sampling and forward for the trail endpoints.

    Returns:
        Warped stage input.
    """
    pin, tilt, affine, travel, turn, zoom, shake, extension, motion_blur = Transform.__struct_fields__
    _, value_field = Gated.__struct_fields__
    heading, elevation = Tilt.__struct_fields__
    position_field, rotation_field, scale_field, clock, noise, seed = Shake.__struct_fields__
    speed, hold = Clock.__struct_fields__
    *_, pivot = Affine.__struct_fields__
    _, share_field, _ = CameraFrame.__struct_fields__
    frame, lens, share = shell.space, lensed("Projection Matrix"), shell.input(Shell.frame, share_field)
    first, second, third = (group_input(name, "VECTOR") for name in Projective.__struct_fields__)
    length, opening = (group_input(name) for name in ("length", "opening"))

    def value(side: int | None, *at: str) -> Expr:
        held = shell.input(*at)
        return held if side is None else msgspec.structs.replace(held, socket=ShutterPoses.key(side, *at))

    def pose(side: int | None) -> Expr:
        def shaken(*at: str) -> Expr:
            return value(side, shake, value_field, *at)

        offset: Scalar = 0.0 if side is None else opening + float(side)
        time = math("MULTIPLY_ADD", offset * length, shaken(clock, speed), clocked(shaken(clock, speed), shaken(clock, hold)))
        wobble = shell.record.shake.value.noise
        x, y, angle, stops = (
            math("MULTIPLY_ADD", wobble.node(0.0, time + origin(shaken(seed), float(textures.stream(Shake, axis)))["X"], linked=Some(lambda info: Some(shaken(noise, info.name)))), 2.0, -1.0)
            for axis in range(4)
        )
        gate = shell.gate(shake)
        jitter = placement(
            frame,
            switched(gate, (0.0, 0.0, 0.0), vector("DIVIDE", vector("MULTIPLY", xyz(x, y), shaken(position_field)), frame.extent)),
            switched(gate, 0.0, angle * shaken(rotation_field)),
            switched(gate, 1.0, 2.0 ** (stops * shaken(scale_field))),
            (0.0, 0.0, 0.0),
            (0.5, 0.5, 0.0),
        )
        drift = placement(frame, vector("SCALE", value(side, travel), offset), value(side, turn) * offset, 2.0 ** (value(side, zoom) * offset), (0.0, 0.0, 0.0), value(side, affine, pivot))
        return composed(drift, composed(jitter, placement(frame, **{field.name: value(side, affine, field.name) for field in msgspec.structs.fields(Affine)})))

    def mapped(side: int | None, fraction: Expr, *, forward: bool = False) -> Expr:
        matrix = pose(side)

        def pinning(held: Expr, *, ahead: bool) -> Expr:
            return switched(shell.gate(pin), held, pinned(held, (first, second, third), forward=ahead))

        def tilting(held: Expr, *, ahead: bool) -> Expr:
            return reaimed(held, lens, value(side, tilt, heading), value(side, tilt, elevation), share, forward=ahead)

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
    border = shell.input(extension)
    uv = covered(frame, switched(shell.gate(motion_blur), mapped(None, start), mapped(0, start))).named("uv")
    image = extended(shell.color, uv, (border, border), clipped=True)
    dimensions = Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", shell.color),), socket="Dimensions", shape="VECTOR")
    center = (0.5, 0.5, 1.0)
    opened, stepped = (sampled(frame, mapped(1, mapped(0, entering(frame, at)), forward=True)) for at in (center, vector("ADD", center, vector("DIVIDE", (1.0, 0.0, 0.0), dimensions))))
    still = functools.reduce(
        lambda held, match: held * match,
        (
            switched(shell.gate(pin), 1.0, 0.0),
            math("COMPARE", abs(value(None, tilt, heading)) + abs(value(None, tilt, elevation)), 0.0, 0.0),
            switched(shell.gate(shake), 1.0, 0.0),
            *(math("COMPARE", abs(value(None, member)), 0.0, 0.0) for member in (turn, zoom)),
            math("COMPARE", vector("LENGTH", value(None, travel)), 0.0, 0.0),
        ),
    )
    return Ok(switched(shell.gate(motion_blur), image, switched(still, trailed(image, shell.color, opened, stepped), image)))


# --- [COMPOSITION] ----------------------------------------------------------------------

_POSES: Final = ShutterPoses.of(tuple(filter(_temporal, Field.of(Transform))))
transform = CompositorEffect(
    record=Transform,
    graph=transformed,
    alpha=Alpha.RESULT,
    parts=(
        Requests[Transform](request=precise),
        Conditions[Transform](condition=aimed),
        Checks[Transform](check=convex),
        Derives[Transform, msgspec.Struct](record=Some(Projective), of=rows),
        Derives[Transform, msgspec.Struct](record=Some(_POSES.record), of=lambda _identity, scene, record: Ok(_POSES.posed(scene, record)), reads=shutter.reads),
        Animates[Transform](keys=lambda identity, scene, entry, _record: _POSES.keyed(identity, scene, entry)),
        Inspects(key="transform_uv", end=End(node="uv", socket="Vector"), description="Source positions and coverage"),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Shake", "Tilt", "Transform", "transform"]
