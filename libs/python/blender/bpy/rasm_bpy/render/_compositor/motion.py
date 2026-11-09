"""Compositor motion: clocks per second with frame holds over the derived scene frame rate, seeded uniforms and decorrelated noise origins, the scene shutter and the poses it samples, and trails between shutter endpoints."""

from collections.abc import Callable, Mapping
from types import MappingProxyType
from typing import Annotated, Final, Self

import bpy
import msgspec
import numpy as np

from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._rna import animation
from rasm_bpy.data._rna.animation import Animation, Keys
from rasm_bpy.data._rna.descriptors import Descriptor, Value
from rasm_bpy.data._rna.paths import id_path, RnaPath, Struct
from rasm_bpy.nodes._desired import item_key
from rasm_bpy.nodes._graphs import Expr, math, Scalar, vector, xyz
from rasm_bpy.nodes._interfaces import InterfacePanel, Socket
from rasm_bpy.render._compositor.colors import with_alpha
from rasm_bpy.render._compositor.coordinates import dimensions
from rasm_bpy.render._compositor.inputs import _addressed, _entry_values, _keyed, DerivedInputs
from rasm_records import accumulated, Count, Fault, NonNegative, Ok, Outcome, paired, Reason, SEED_BOUND

# --- [CONSTANTS] ------------------------------------------------------------------------

_FPS: Final[bpy.types.IntProperty] = bpy.types.RenderSettings.bl_rna.properties["fps"]  # ty: ignore[invalid-assignment]
_FPS_BASE: Final[bpy.types.FloatProperty] = bpy.types.RenderSettings.bl_rna.properties["fps_base"]  # ty: ignore[invalid-assignment]
RATE: Final = Socket(key="frame_rate", kind=bpy.types.NodeSocketFloat, description="Scene frames per second", hide_in_modifier=True, default_value=_FPS.default / _FPS_BASE.default)
SEED: Final = Socket(key="seed", kind=bpy.types.NodeSocketInt, description="Number selecting the pattern", min_value=0, max_value=SEED_BOUND)
LENGTH: Final = Socket(key="length", kind=bpy.types.NodeSocketFloat, description="Shutter length in frames", hide_in_modifier=True)
OPENING: Final = Socket(key="opening", kind=bpy.types.NodeSocketFloat, description="Shutter opening relative to frame in shutter lengths", hide_in_modifier=True)
SHUTTER: Final = (LENGTH, OPENING)
_OPENINGS: Final = MappingProxyType({"START": 0.0, "CENTER": -0.5, "END": -1.0})

# --- [MODELS] ---------------------------------------------------------------------------


class Clock(msgspec.Struct, frozen=True, kw_only=True):
    """Clock speed and frame hold of animated parameters."""

    speed: Annotated[NonNegative, msgspec.Meta(description="Clock units per second, still at 0")] = 1.0
    hold: Annotated[Count, msgspec.Meta(description="Scene frames each clock value holds")] = 1

    def held(self, frame: float) -> float:
        """Returns scene `frame` held to the first frame of its hold."""
        return self.hold * float(np.floor(frame / self.hold))

    def clock(self, frame: float, rate: float) -> float:
        """Returns clock units at held scene `frame` under `rate` scene frames per second."""
        return self.speed * self.held(frame) / rate

    def timed(self, scene: bpy.types.Scene) -> float:
        """Returns clock units at the scene's current frame, subframe included, under its frame rate."""
        return self.clock(scene.frame_current_final, rate(scene))


class Shutter(msgspec.Struct, frozen=True, kw_only=True):
    """Shutter length in frames and opening offset relative to frame."""

    length: float = 0.5
    opening: float = -0.5

    @classmethod
    def of(cls, scene: bpy.types.Scene) -> Self:
        """Reads shutter length and the opening offset `motion_blur_position` places in shutter lengths."""
        return cls(length=scene.render.motion_blur_shutter, opening=_OPENINGS[scene.render.motion_blur_position])

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
        return _entry_values(identity, entry, (socket.key for socket in self.sockets), lambda _key, value: Ok(value)).map(
            lambda values: {**Shutter.of(scene).values, **{self.key(side, key): value for side in range(2) for key, value in values.items()}}
        )

    def keyed(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> Outcome[tuple[Animation, ...]]:
        """Keys every pose component at its socket channel sampled at both fractional shutter endpoints of every rendered frame."""
        frames = tuple(map(float, range(scene.frame_start, scene.frame_end + 1)))
        lengths = animation.sampled(scene.render, ("motion_blur_shutter",), frames)
        opening = Shutter.of(scene).opening

        def baked(parts: tuple[tuple[float, ...], Mapping[Key, str]]) -> Outcome[tuple[Animation, ...]]:
            lengths, identifiers = parts
            moments = tuple(tuple(frame + (opening + side) * length for frame, length in zip(frames, lengths)) for side in range(2))
            sources = {socket.key: _addressed(entry, identifiers, socket.key) for socket in self.sockets}

            def sampled(source: Struct, target: RnaPath, side: int) -> Outcome[tuple[Animation, ...]]:
                sizes = Descriptor.dimensions(source.bl_rna.properties["value"])
                members: tuple[RnaPath, ...] = tuple(("value", index) for index in range(sizes[0])) if sizes else (("value",),)
                return accumulated(
                    animation.sampled(source, member, moments[side]).map(lambda values: Animation(path=(*target, *member), source=Keys(points=tuple(zip(frames, values))))) for member in members
                )

            return accumulated(
                paired(
                    sources[socket.key],
                    _addressed(entry, identifiers, self.key(side, socket.key)).bind(lambda target: id_path(target).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(target), member="value"))),
                ).bind(lambda held: sampled(*held, side))
                for socket in self.sockets
                for side in range(2)
            ).map(lambda channels: tuple(channel for held in channels for channel in held))

        return paired(lengths, _keyed(identity, entry).map(lambda held: held[1])).bind(baked) if self.sockets else lengths.map(lambda _: ())


# --- [OPERATIONS] -----------------------------------------------------------------------


def rate(scene: bpy.types.Scene) -> float:
    """Returns the scene's frames per second, `fps` over `fps_base`."""
    return scene.render.fps / scene.render.fps_base


def clocked(read: Callable[[Socket], Scalar], panel: InterfacePanel) -> Expr:
    """Clock units of a clock panel's speed at the scene frame held `hold` frames at a time, timed through the derived `RATE` input.

    Args:
        read: Group input of a declaring socket (`Shell.input`).
        panel: Clock panel, `CLOCK` or `InterfacePanel.of(<key>, Clock(...))`.

    Returns:
        Clock units at the held frame, `speed` units per second.
    """
    speed, hold = (read(panel.socket(field)) for field in (Clock.speed, Clock.hold))
    return clock(speed, hold, read(RATE))


def clock(speed: Scalar, hold: Scalar, rate: Scalar) -> Expr:
    """Clock units of a speed at the scene frame held `hold` frames at a time, for clocks whose speed and hold `clocked` cannot read through one reader (shutter poses).

    Args:
        speed: Clock units per second.
        hold: Scene frames each clock value holds.
        rate: Scene frames per second, the `RATE` input.

    Returns:
        Clock units at the held frame.
    """
    frame = Expr(kind=bpy.types.CompositorNodeSceneTime, socket="Frame")
    held = math("MULTIPLY", math("FLOOR", math("DIVIDE", frame, hold)), hold)
    return math("MULTIPLY", math("DIVIDE", held, rate), speed)


def hashed(seed: Scalar, noise: Scalar, pattern: Scalar = 0, instance: Scalar = 0) -> Expr:
    """Builds the seeded uniform of one noise, each component in [0, 1].

    Args:
        seed: `SEED` operand.
        noise: Stream index operand.
        pattern: Whole `clocked` units for values redrawn per pattern, else 0.
        instance: Repeated draw index, else 0.

    Returns:
        White Noise `Color` expression.
    """
    return Expr(
        kind=bpy.types.ShaderNodeTexWhiteNoise,
        inputs=(("Vector", xyz(seed, noise, pattern)), ("W", instance)),
        members={"noise_dimensions": "4D"},
        socket="Color",
        socket_type=bpy.types.NodeSocketColor,
    )


def origin(seed: Scalar, noise: Scalar, pattern: Scalar = 0, instance: Scalar = 0) -> Expr:
    """Builds the decorrelation origin of one noise, its `hashed` uniform spread over 256 units per axis.

    Args:
        seed: `SEED` operand.
        noise: Stream index operand.
        pattern: Whole `clocked` units for origins redrawn per pattern, else 0.
        instance: Repeated draw index, else 0.

    Returns:
        3D origin `Vector` expression.
    """
    return vector("SCALE", hashed(seed, noise, pattern, instance), 256.0)


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
        return Expr(kind=bpy.types.CompositorNodeDBlur, inputs=(("Image", held), ("Center", (0.5, 0.5)), *settings), socket="Image", socket_type=bpy.types.NodeSocketColor)

    unit = vector(
        "ADD", vector("SCALE", Expr(kind=bpy.types.CompositorNodeImageCoordinates, inputs=(("Image", source),), socket="Pixel", socket_type=bpy.types.NodeSocketVector), 0.0), (1.0, 1.0, 1.0)
    )
    trail, coverage = blurred(image), blurred(with_alpha(unit, 1.0))
    return with_alpha(vector("DIVIDE", trail, coverage), trail["Alpha"] / coverage["Alpha"])


# --- [COMPOSITION] ----------------------------------------------------------------------

CLOCK: Final = InterfacePanel.of("clock", Clock())
timing: Final = DerivedInputs(of=lambda _identity, scene, _entry: Ok({RATE.key: rate(scene)}), sockets=(RATE,), reads=((bpy.types.RenderSettings, _FPS), (bpy.types.RenderSettings, _FPS_BASE)))
shutter: Final = DerivedInputs(
    of=lambda _identity, scene, _entry: Ok(Shutter.of(scene).values),
    sockets=SHUTTER,
    reads=tuple((bpy.types.RenderSettings, bpy.types.RenderSettings.bl_rna.properties[name]) for name in ("motion_blur_shutter", "motion_blur_position")),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CLOCK", "LENGTH", "OPENING", "RATE", "SEED", "SHUTTER", "Clock", "Shutter", "ShutterPoses", "clock", "clocked", "hashed", "origin", "rate", "shutter", "timing", "trailed"]
