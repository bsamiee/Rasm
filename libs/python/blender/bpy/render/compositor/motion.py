"""Compositor motion: clocks per second with frame holds, decorrelated noise origins, the scene shutter and the poses it samples, and trails between shutter endpoints."""

from typing import Final, Self

import bpy
from expression import Ok
import msgspec
import numpy as np

from blender.bpy.data.rna import animation
from blender.bpy.data.rna.animation import Animation, Keys
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.paths import id_path, RnaPath, Struct
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.graphs import Expr, math, Scalar, vector, xyz
from blender.bpy.nodes.interface import item_key, Socket
from blender.bpy.render.compositor.coordinates import dimensions
from blender.bpy.render.compositor.inputs import Derives, entry_input
from blender.records.faults import accumulated, Fault, Outcome, paired
from blender.records.fields import Case, Count, NonNegative

# --- [CONSTANTS] ------------------------------------------------------------------------

LENGTH: Final = Socket(key="length", kind=bpy.types.NodeSocketFloat, description="Shutter length in frames", hide_in_modifier=True)
OPENING: Final = Socket(key="opening", kind=bpy.types.NodeSocketFloat, description="Shutter opening relative to frame in shutter lengths", hide_in_modifier=True)
SHUTTER: Final = (LENGTH, OPENING)

# --- [MODELS] ---------------------------------------------------------------------------


class Clock(msgspec.Struct, frozen=True, kw_only=True):
    """Clock speed in clock units per second, still at 0, and the scene frames each clock value holds."""

    speed: NonNegative = 1.0
    hold: Count = 1

    def held(self, frame: float) -> float:
        """Returns scene `frame` held to the first frame of its hold."""
        return self.hold * float(np.floor(frame / self.hold))

    def clock(self, frame: float, rate: float) -> float:
        """Returns clock units at held scene `frame` under `rate` scene frames per second."""
        return self.speed * self.held(frame) / rate


class Shutter(msgspec.Struct, frozen=True, kw_only=True):
    """Shutter length in frames and opening offset relative to frame."""

    length: float = 0.5
    opening: float = -0.5

    @classmethod
    def of(cls, scene: bpy.types.Scene) -> Self:
        """Reads shutter length and the opening offset `motion_blur_position` places in shutter lengths."""
        match scene.render.motion_blur_position:
            case "START":
                opening = 0.0
            case "CENTER":
                opening = -0.5
            case "END":
                opening = -1.0
        return cls(length=scene.render.motion_blur_shutter, opening=opening)

    @property
    def values(self) -> dict[Key, Value]:
        """Shutter input values by socket key."""
        return {LENGTH.key: self.length, OPENING.key: self.opening}


class ShutterPoses(msgspec.Struct, frozen=True, kw_only=True):
    """Numeric sockets sampled at both shutter endpoints into hidden pose sockets keyed `pose_<side>_<key>`, the shutter beside them."""

    sockets: tuple[Socket, ...]

    @property
    def poses(self) -> tuple[Socket, ...]:
        """Hidden shutter sockets, then one hidden pose socket per posed socket and shutter endpoint."""
        return (*SHUTTER, *(msgspec.structs.replace(socket, key=self.key(side, socket.key), hide_in_modifier=True) for side in range(2) for socket in self.sockets))

    @staticmethod
    def key(side: int, key: Key) -> Key:
        """Pose input key of a socket at one shutter endpoint."""
        return item_key("pose", side, key)

    def posed(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> Outcome[dict[Key, Value]]:
        """Returns the scene shutter with both endpoint poses held at the entry's input values."""
        return accumulated(
            entry_input(identity, entry, socket.key).bind(lambda held: Descriptor.resolve(held, ("value",), bpy.types.Property).map(lambda descriptor: (socket.key, descriptor.read(held))))
            for socket in self.sockets
        ).map(lambda values: {**Shutter.of(scene).values, **{self.key(side, key): value for side in range(2) for key, value in values}})

    def keyed(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> Outcome[tuple[Animation, ...]]:
        """Keys every pose component at its socket channel sampled at both fractional shutter endpoints of every rendered frame."""
        frames = tuple(range(scene.frame_start, scene.frame_end + 1))
        opening = Shutter.of(scene).opening

        def components(socket: Socket) -> tuple[int | None, ...]:
            prop: bpy.types.Property | None = socket.kind.bl_rna.properties.get("default_value")
            match prop:
                case bpy.types.FloatProperty(array_length=length) | bpy.types.IntProperty(array_length=length) | bpy.types.BoolProperty(array_length=length) if length:
                    return tuple(range(length))
                case _:
                    return (None,)

        def component(lengths: tuple[float, ...], socket: Socket, side: int, index: int | None) -> Outcome[Animation]:
            member: RnaPath = ("value",) if index is None else ("value", index)

            def samples(source: Struct, target: RnaPath) -> Outcome[Animation]:
                return animation.sampled(source, member, (frame + (opening + side) * length for frame, length in zip(frames, lengths))).map(
                    lambda values: Animation(path=(*target, *member), source=Keys(points=tuple(zip(map(float, frames), values))))
                )

            return paired(
                entry_input(identity, entry, socket.key),
                entry_input(identity, entry, self.key(side, socket.key)).bind(lambda held: id_path(held).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(held), member="value"))),
            ).bind(lambda values: samples(*values))

        return animation.sampled(scene.render, ("motion_blur_shutter",), frames).bind(
            lambda lengths: accumulated(component(lengths, socket, side, index) for socket in self.sockets for side in range(2) for index in components(socket))
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def clocked(speed: Scalar, hold: Scalar) -> Expr:
    """Clock units at the scene frame held `hold` frames at a time, `speed` units per second, the held frame timed through Scene Time's seconds per frame."""
    time = Expr(kind=bpy.types.CompositorNodeSceneTime, socket="Frame")
    held = math("MULTIPLY", math("FLOOR", math("DIVIDE", time, hold)), hold)
    return math("MULTIPLY", math("MULTIPLY", held, math("DIVIDE", Expr(kind=bpy.types.CompositorNodeSceneTime, socket="Seconds"), time)), speed)


def origin(seed: Scalar, noise: Scalar, pattern: Scalar = 0, instance: Scalar = 0) -> Expr:
    """Builds decorrelation origin of one noise.

    Args:
        seed: Record `Seed` operand.
        noise: Stream index operand.
        pattern: Whole `clocked` units for origins redrawn per pattern, else 0.
        instance: Repeated draw index, else 0.

    Returns:
        3D origin `Vector` expression.
    """
    hashed = Expr(
        kind=bpy.types.ShaderNodeTexWhiteNoise, inputs=(("Vector", xyz(seed, noise, pattern)), ("W", instance)), members={"noise_dimensions": "4D"}, socket="Color", shape=bpy.types.NodeSocketColor
    )
    return vector("MULTIPLY", hashed, (256.0, 256.0, 256.0))


def trailed(image: Expr, source: Scalar, opened: Expr, stepped: Expr) -> Expr:
    """Smears an associated image along the motion between shutter endpoints through Directional Blur, normalized by the trail's own coverage.

    Args:
        image: Associated image at the shutter opening.
        source: Image whose data window the trail covers.
        opened: Map UV of the frame center at the shutter close.
        stepped: Map UV of the point one pixel right of the center at the shutter close.

    Returns:
        Covered associated trail.
    """
    sides = dimensions(source)
    translation = vector("MULTIPLY", vector("SUBTRACT", opened, (0.5, 0.5, 1.0)), sides)
    factor = vector("MULTIPLY", vector("SUBTRACT", stepped, opened), sides)
    scale, rotation, length, diagonal = vector("LENGTH", factor), math("ARCTAN2", factor["Y"], factor["X"]), vector("LENGTH", translation), vector("LENGTH", sides)
    settings = (
        ("Scale", scale),
        ("Rotation", rotation),
        ("Translation Amount", length / (scale * diagonal)),
        ("Translation Direction", math("ARCTAN2", translation["Y"], -translation["X"])),
        ("Samples", math("CEIL", math("LOGARITHM", length + (abs(scale - 1.0) + abs(rotation)) * diagonal, 2.0))),
    )

    def blurred(held: Expr) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeDBlur, inputs=(("Image", held), ("Center", (0.5, 0.5)), *settings), socket="Image", shape=bpy.types.NodeSocketColor)

    unit = vector("ADD", vector("SCALE", Expr(kind=bpy.types.CompositorNodeImageCoordinates, inputs=(("Image", source),), socket="Pixel", shape=bpy.types.NodeSocketVector), 0.0), (1.0, 1.0, 1.0))
    trail, coverage = (
        blurred(image),
        blurred(Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", unit), ("Alpha", 1.0), ("Type", "Replace Alpha")), socket="Image", shape=bpy.types.NodeSocketColor)),
    )
    return Expr(
        kind=bpy.types.CompositorNodeSetAlpha,
        inputs=(("Image", vector("DIVIDE", trail, coverage)), ("Alpha", trail["Alpha"] / coverage["Alpha"]), ("Type", "Replace Alpha")),
        socket="Image",
        shape=bpy.types.NodeSocketColor,
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

shutter: Final = Derives[Case](
    of=lambda _identity, scene, _entry, _record: Ok(Shutter.of(scene).values),
    sockets=SHUTTER,
    reads=tuple((bpy.types.RenderSettings, bpy.types.RenderSettings.bl_rna.properties[name]) for name in ("motion_blur_shutter", "motion_blur_position")),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LENGTH", "OPENING", "SHUTTER", "Clock", "Shutter", "ShutterPoses", "clocked", "origin", "shutter", "trailed"]
