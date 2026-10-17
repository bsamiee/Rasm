"""Shape, color, and transition keys with mask joins and track mattes."""

import math
from typing import Annotated

from expression import Some
import msgspec

from blender.imaging.parameters import Case, Choice, FrameLength, Gated, Position, Presentation, Quantity, Unit

# --- [TYPES] ----------------------------------------------------------------------------


class Outline(Choice):
    """Inside test of a shape."""

    BOX = 1
    ELLIPSE = 2


class MaskOperation(Choice):
    """Join of a key with preceding masks."""

    ADD = 1
    SUBTRACT = 2
    MULTIPLY = 3


class MatteChannel(Choice):
    """Coverage channel read from the layer beneath."""

    ALPHA = 1
    LUMINANCE = 2


type Key = Shape | Qualifier | Transition

# --- [MODELS] ---------------------------------------------------------------------------


class Shape(Case, frozen=True, kw_only=True, tag=1):
    """Box or ellipse with an independently feathered edge."""

    outline: Annotated[Outline, msgspec.Meta(description="Box or ellipse")] = Outline.ELLIPSE
    position: Annotated[Position, msgspec.Meta(description="Center as a share of each frame side")] = (0.5, 0.5)
    size: Annotated[tuple[float, float], msgspec.Meta(description="Half extents as shares of the frame's greater side"), Presentation(quantity=Some(Quantity.FRAME_GREATER))] = (0.1, 0.05)
    rotation: Annotated[float, msgspec.Meta(description="Turn about the center"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    feather: Annotated[FrameLength, msgspec.Meta(description="Edge blur radius as a share of the frame's greater side"), Presentation(soft=Some((0.0, 0.1)))] = 0.0


class Band(msgspec.Struct, frozen=True, kw_only=True):
    """Selected interval with smoothstep feather beyond each bound."""

    low: Annotated[float, msgspec.Meta(description="Lower bound of full selection")]
    high: Annotated[float, msgspec.Meta(description="Upper bound of full selection")]
    softness: Annotated[float, msgspec.Meta(ge=0.0, description="Ramp width beyond each bound")]


class HueRange(msgspec.Struct, frozen=True, kw_only=True):
    """Circular HSY hue interval with smoothstep feather beyond each edge."""

    center: Annotated[float, msgspec.Meta(description="HSY hue in radians, magenta at zero and red at one sixth turn"), Presentation(quantity=Some(Quantity.ANGLE))] = math.tau / 6.0
    width: Annotated[float, msgspec.Meta(ge=0.0, le=math.tau, description="Hue span of full selection"), Presentation(quantity=Some(Quantity.ANGLE))] = math.radians(30.0)
    softness: Annotated[float, msgspec.Meta(ge=0.0, le=math.pi, description="Ramp width beyond each edge"), Presentation(quantity=Some(Quantity.ANGLE))] = math.radians(10.0)


class Qualifier(Case, frozen=True, kw_only=True, tag=2):
    """Pixels selected by HSY hue, saturation, and LIN-wrapped luminance stops."""

    hue: Annotated[Gated[HueRange], msgspec.Meta(description="HSY hue range on the hue circle")] = Gated(enabled=True, value=HueRange())
    saturation: Annotated[Gated[Band], msgspec.Meta(description="HSY saturation range")] = Gated(enabled=False, value=Band(low=0.2, high=1.0, softness=0.1))
    luminance: Annotated[Gated[Band], msgspec.Meta(description="LIN-wrapped HSY luminance stops from scene-linear grey")] = Gated(enabled=False, value=Band(low=-2.0, high=2.0, softness=1.0))


class Transition(Case, frozen=True, kw_only=True, tag=3):
    """Spatial wipe revealing a layer as completion rises."""

    completion: Annotated[Unit, msgspec.Meta(description="Share of the travel, 0 hidden and 1 fully shown"), Presentation(soft=Some((0.0, 1.0)))] = 0.5
    angle: Annotated[float, msgspec.Meta(description="Travel direction, 0 running left to right"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    softness: Annotated[Unit, msgspec.Meta(description="Edge ramp width as a share of the travel"), Presentation(soft=Some((0.0, 1.0)))] = 0.1


class Mask(msgspec.Struct, frozen=True, kw_only=True):
    """Key joined with preceding masks, optionally inverted."""

    key: Annotated[Key, msgspec.Meta(description="Shape, qualifier, or transition")] = Shape()
    operation: Annotated[MaskOperation, msgspec.Meta(description="Join with preceding masks")] = MaskOperation.ADD
    invert: Annotated[bool, msgspec.Meta(description="Select the key's complement")] = False


class TrackMatte(msgspec.Struct, frozen=True, kw_only=True):
    """Coverage read from the associated layer beneath, optionally inverted."""

    channel: Annotated[MatteChannel, msgspec.Meta(description="Coverage channel of the layer beneath")] = MatteChannel.ALPHA
    invert: Annotated[bool, msgspec.Meta(description="Read the channel's complement")] = False


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Band", "HueRange", "Key", "Mask", "MaskOperation", "MatteChannel", "Outline", "Qualifier", "Shape", "TrackMatte", "Transition"]
