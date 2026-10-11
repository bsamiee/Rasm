"""Editable lighting references, seeded distributions, and Lambertian sphere images."""

import colorsys
import math
import sys
from typing import Annotated, get_origin, Self

from expression import Error, Nothing, Ok, Option, Some
import msgspec.inspect
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from rasm.imaging.chain import GREY
from rasm.imaging.color.processors import applied, processor, xyz
from rasm.imaging.color.white import White
from rasm.imaging.faults import accumulated, ImagingFaults, paired
from rasm.imaging.hashing import content_hash, rng
from rasm.imaging.parameters import Choice, Finite, Gated, markers, Position, Presentation, Rgb, Seed
from rasm.rendering.faults import RenderingArithmeticFault, RenderingFaults, RenderingImagingFault, RenderingResult, RenderingSizeFault
from rasm.rendering.lighting.rigs import Disk, Placement, Placements, Point, Spot, Sun
from rasm.rendering.outputs.framing import Side

# --- [TYPES] ----------------------------------------------------------------------------


class Family(Choice):
    """Family of lighting references."""

    SKY = 1, "Time of Day"
    STUDIO = 2, "Studio"
    PORTRAIT = 3, "Portrait"
    CINEMATIC = 4, "Cinematic"
    ENVIRONMENT = 5, "Environment"


class ReferenceDistribution(Choice):
    """Color and spatial strategy for a seeded lighting reference."""

    BALANCED = 1, "Balanced"
    VIVID = 2, "Vivid"
    SOFT = 3, "Soft"


# --- [MODELS] ---------------------------------------------------------------------------


class DistantLight(msgspec.Struct, frozen=True, kw_only=True):
    """Directional radiance and its exposure offset for a sphere image."""

    direction: tuple[Finite, Finite, Finite]
    color: Rgb[Finite]
    stops: Finite = 0.0


class Highlight(msgspec.Struct, frozen=True, kw_only=True):
    """One colored Gaussian glow centered in a reference frame."""

    position: Position = (0.5, 0.5)
    sharpness: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max), Presentation(soft=Some((0.55, 2.4)))] = 1.2
    intensity: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max), Presentation(soft=Some((0.35, 1.45)))] = 0.8
    color: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)


class Dapple(msgspec.Struct, frozen=True, kw_only=True):
    """Seeded spots with independent position, sharpness, and intensity draws."""

    count: Annotated[int, msgspec.Meta(ge=0)] = 12
    sharpness: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max), Presentation(soft=Some((8.0, 22.0)))] = 8.0
    intensity: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max), Presentation(soft=Some((0.3, 0.8)))] = 0.3
    sharpness_spread: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, description="Uniform variation above the minimum sharpness"), Presentation(soft=Some((0.0, 14.0)))] = 14.0
    intensity_spread: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, description="Uniform variation above the minimum intensity"), Presentation(soft=Some((0.0, 0.5)))] = 0.5
    seed: Seed = 0


class Blinds(msgspec.Struct, frozen=True, kw_only=True):
    """Horizontal slat shadows across the reference."""

    cycles: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max), Presentation(soft=Some((5.0, 7.0)))] = 6.0
    shade: Annotated[float, msgspec.Meta(ge=0.0, le=1.0)] = 0.45


class Rim(msgspec.Struct, frozen=True, kw_only=True):
    """Colored glow rising toward the edges of the reference."""

    amount: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max), Presentation(soft=Some((0.0, 1.2)))] = 0.6
    color: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)
    radius: Annotated[float, msgspec.Meta(ge=0.0, le=math.sqrt(2.0))] = 0.5
    falloff: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max)] = 1.4


class Roll(msgspec.Struct, frozen=True, kw_only=True):
    """Draw of arbitrary editable fields over their presentation ranges."""

    highlights: Annotated[int, msgspec.Meta(ge=1, le=8)] = 3
    saturation: Annotated[float, msgspec.Meta(ge=0.0, le=1.0)] = 0.8
    seed: Seed = 0


class ReferenceImage(msgspec.Struct, frozen=True, kw_only=True):
    """Editable scene-linear reference with floor, glows, shadows, and edge light."""

    tint: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)
    level: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max), Presentation(soft=Some((0.03, 0.55)))] = 0.08
    gradient: Annotated[tuple[Finite, Finite], Presentation(soft=Some((0.0, 0.6)))] = (0.0, 0.0)
    highlights: tuple[Highlight, ...] = ()
    dapple: Gated[Dapple] = Gated(enabled=False, value=Dapple())
    blinds: Gated[Blinds] = Gated(enabled=False, value=Blinds())
    rim: Gated[Rim] = Gated(enabled=False, value=Rim())
    vignette: Annotated[float, msgspec.Meta(ge=0.0, le=1.0), Presentation(soft=Some((0.0, 0.5)))] = 0.0

    # --- [DISTRIBUTIONS]

    @classmethod
    def drawn(cls, distribution: ReferenceDistribution, seed: Seed) -> Self:
        """Draws an editable reference under the selected distribution and seed."""
        generator = rng(content_hash(ReferenceDistribution), seed, int(distribution))
        gels = cls.gels()
        match distribution:
            case ReferenceDistribution.BALANCED:
                colors = tuple(gels.values())
                slots: tuple[tuple[tuple[float, float], Rgb], ...] = ()
                count, horizontal, vertical = (1, 3), (-0.8, 0.8), (-0.7, 0.85)
                sharpness, intensity, level, gradient = (0.7, 2.4), (0.5, 1.1), (0.03, 0.5), (0.0, 0.6)
                tints, rim, vignette = colors, (0.4, 1.2), (0.5, 0.0, 0.5)
                cycles: tuple[float, ...] = (0.0, 0.0, 0.0, 5.0, 7.0)
                dapples: tuple[int, ...] = (0, 0, 0, 12)
            case ReferenceDistribution.VIVID:
                colors = (
                    Rgb(red=1.4, green=0.12, blue=0.85),
                    Rgb(red=0.15, green=0.75, blue=1.35),
                    Rgb(red=1.35, green=0.55, blue=0.05),
                    Rgb(red=0.25, green=1.05, blue=0.35),
                    Rgb(red=0.75, green=0.25, blue=1.35),
                )
                positions = ((-0.62, 0.18), (0.62, 0.18), (0.0, 0.58), (-0.38, -0.42), (0.38, -0.42))
                spreads = (((0, 1, 3), (0, 1, 4), (0, 2, 4), (1, 2, 4)), ((0, 1, 2, 3), (0, 1, 3, 4), (0, 2, 3, 4)), ((0, 1, 2, 3, 4),))
                group = spreads[int(generator.integers(len(spreads)))]
                slots = tuple((positions[index], colors[index]) for index in group[int(generator.integers(len(group)))])
                slot_count = len(slots)
                count, horizontal, vertical = (slot_count, slot_count), (-0.08, 0.08), (-0.08, 0.08)
                sharpness, intensity, level, gradient = (1.3, 2.1), (0.85, 1.45), (0.03, 0.1), (0.0, 0.15)
                tints, rim, vignette = (Rgb(red=0.42, green=0.46, blue=0.52),), (0.25, 0.5), (1.0, 0.15, 0.45)
                cycles, dapples = (0.0,), (0,)
            case ReferenceDistribution.SOFT:
                colors = (Rgb(red=1.0, green=0.96, blue=0.92), Rgb(red=0.94, green=0.96, blue=1.0), Rgb(red=0.98, green=0.95, blue=0.9))
                slots = ()
                count, horizontal, vertical = (1, 2), (-0.45, 0.45), (-0.35, 0.55)
                sharpness, intensity, level, gradient = (0.55, 1.0), (0.35, 0.65), (0.32, 0.55), (0.0, 0.25)
                tints, rim, vignette = (Rgb(red=0.97, green=0.95, blue=0.92), Rgb(red=0.93, green=0.95, blue=0.98)), (0.0, 0.0), (0.3, 0.0, 0.2)
                cycles, dapples = (0.0,), (0,)
        highlights = tuple(
            Highlight(
                position=(
                    ((slots[index][0][0] if slots else 0.0) + float(generator.uniform(*horizontal)) + 1.0) / 2.0,
                    ((slots[index][0][1] if slots else 0.0) + float(generator.uniform(*vertical)) + 1.0) / 2.0,
                ),
                sharpness=float(generator.uniform(*sharpness)),
                intensity=float(generator.uniform(*intensity)),
                color=slots[index][1] if slots else colors[int(generator.integers(len(colors)))],
            )
            for index in range(int(generator.integers(count[0], count[1] + 1)))
        )
        rim_on, vignette_on = float(generator.random()) < rim[0], float(generator.random()) < vignette[0]
        stripes, dapple_count = cycles[int(generator.integers(len(cycles)))], dapples[int(generator.integers(len(dapples)))]
        return cls(
            tint=tints[int(generator.integers(len(tints)))],
            level=float(generator.uniform(*level)),
            gradient=(float(generator.uniform(*gradient)), float(generator.uniform(*gradient))),
            highlights=highlights,
            rim=Gated(enabled=rim_on, value=Rim(amount=float(generator.uniform(0.0, rim[1])))),
            vignette=float(generator.uniform(vignette[1], vignette[2])) if vignette_on else 0.0,
            blinds=Gated(enabled=stripes > 0.0, value=Blinds(cycles=stripes)),
            dapple=Gated(enabled=dapple_count > 0, value=Dapple(count=dapple_count, seed=int(generator.integers(10000)))),
        )

    # --- [CATALOG]

    @staticmethod
    def gels() -> dict[str, Rgb]:
        """Returns the twelve named source colors in scene-linear RGB."""
        return {
            "Warm": Rgb(red=1.0, green=0.78, blue=0.5),
            "Deep Warm": Rgb(red=1.0, green=0.58, blue=0.28),
            "Neutral": Rgb(red=1.0, green=1.0, blue=1.0),
            "Cool": Rgb(red=0.6, green=0.72, blue=1.0),
            "Sky": Rgb(red=0.5, green=0.68, blue=1.0),
            "Magenta": Rgb(red=1.0, green=0.3, blue=0.85),
            "Cyan": Rgb(red=0.2, green=0.85, blue=0.95),
            "Pink": Rgb(red=1.0, green=0.45, blue=0.7),
            "Blue": Rgb(red=0.3, green=0.5, blue=1.0),
            "Green": Rgb(red=0.45, green=0.95, blue=0.5),
            "Fire": Rgb(red=1.0, green=0.5, blue=0.2),
            "Teal": Rgb(red=0.2, green=0.72, blue=0.72),
        }

    @classmethod
    def catalog(cls) -> dict[Family, dict[str, Self]]:
        """Constructs all forty-seven named source recipes in their five families."""
        gels = cls.gels()
        warm, deep, neutral, cool = gels["Warm"], gels["Deep Warm"], gels["Neutral"], gels["Cool"]
        sky, magenta, cyan, pink = gels["Sky"], gels["Magenta"], gels["Cyan"], gels["Pink"]
        blue, green, fire, teal = gels["Blue"], gels["Green"], gels["Fire"], gels["Teal"]
        beauty = Rgb(red=1.0, green=0.95, blue=0.9)
        portrait = Rgb(red=1.0, green=0.96, blue=0.9)
        soft = Rgb(red=1.0, green=0.97, blue=0.92)
        return {
            Family.SKY: {
                "Golden Hour": cls(level=0.2, gradient=(0.9, 0.1), tint=warm, highlights=(Highlight(position=((0.7 + 1.0) / 2.0, (0.3 + 1.0) / 2.0), sharpness=1.2, intensity=0.7, color=warm),)),
                "Sunrise": cls(
                    level=0.16,
                    gradient=(0.5, 0.2),
                    tint=Rgb(red=1.0, green=0.7, blue=0.45),
                    highlights=(Highlight(position=((-0.6 + 1.0) / 2.0, (-0.2 + 1.0) / 2.0), sharpness=1.4, intensity=0.8, color=Rgb(red=1.0, green=0.65, blue=0.4)),),
                ),
                "Sunset": cls(
                    level=0.18, gradient=(0.6, 0.0), tint=deep, highlights=(Highlight(position=((0.6 + 1.0) / 2.0, (-0.25 + 1.0) / 2.0), sharpness=1.3, intensity=0.9, color=deep),), vignette=0.3
                ),
                "Blue Hour": cls(level=0.12, gradient=(0.0, 0.6), tint=cool),
                "Twilight": cls(
                    level=0.1, gradient=(0.0, 0.5), tint=Rgb(red=0.7, green=0.6, blue=1.0), highlights=(Highlight(position=(0.5, (0.8 + 1.0) / 2.0), sharpness=1.0, intensity=0.4, color=magenta),)
                ),
                "Midday": cls(level=0.4, gradient=(0.0, 0.5), highlights=(Highlight(position=((0.1 + 1.0) / 2.0, (0.8 + 1.0) / 2.0), sharpness=1.4, intensity=0.6, color=neutral),)),
                "Harsh Noon": cls(level=0.25, highlights=(Highlight(position=(0.5, (0.85 + 1.0) / 2.0), sharpness=3.0, intensity=1.1, color=neutral),), vignette=0.45),
                "Overcast": cls(level=0.55, gradient=(0.0, 0.25), tint=Rgb(red=0.95, green=0.97, blue=1.0)),
                "Foggy Morning": cls(level=0.5, gradient=(0.0, 0.3), tint=Rgb(red=0.9, green=0.95, blue=1.0), vignette=0.2),
                "Night Sky": cls(
                    level=0.04,
                    gradient=(0.0, 0.25),
                    tint=cool,
                    highlights=(Highlight(position=((0.4 + 1.0) / 2.0, (0.8 + 1.0) / 2.0), sharpness=6.0, intensity=0.5, color=Rgb(red=0.8, green=0.9, blue=1.0)),),
                ),
                "Top Sky": cls(gradient=(0.0, 0.95), tint=Rgb(red=0.85, green=0.92, blue=1.0)),
            },
            Family.STUDIO: {
                "Softbox": cls(level=0.5, highlights=(Highlight(position=(0.5, (0.4 + 1.0) / 2.0), sharpness=0.6, intensity=0.5, color=neutral),)),
                "Softbox Left": cls(level=0.18, highlights=(Highlight(position=((-0.6 + 1.0) / 2.0, (0.35 + 1.0) / 2.0), sharpness=0.9, intensity=0.8, color=neutral),)),
                "Softbox Right": cls(level=0.18, highlights=(Highlight(position=((0.6 + 1.0) / 2.0, (0.35 + 1.0) / 2.0), sharpness=0.9, intensity=0.8, color=neutral),)),
                "Beauty Dish": cls(
                    level=0.2, highlights=(Highlight(position=(0.5, (0.45 + 1.0) / 2.0), sharpness=1.6, intensity=0.9, color=neutral),), rim=Gated(enabled=True, value=Rim(amount=0.25))
                ),
                "Ring Light": cls(level=0.18, highlights=(Highlight(sharpness=0.8, intensity=0.7, color=neutral),), rim=Gated(enabled=True, value=Rim(amount=0.7))),
                "Clamshell": cls(
                    level=0.25, highlights=(Highlight(position=(0.5, (0.6 + 1.0) / 2.0), intensity=0.7, color=neutral), Highlight(position=(0.5, (-0.6 + 1.0) / 2.0), intensity=0.45, color=neutral))
                ),
                "Butterfly": cls(level=0.2, highlights=(Highlight(position=(0.5, (0.55 + 1.0) / 2.0), sharpness=1.4, intensity=0.85, color=beauty),)),
                "Broad Light": cls(level=0.16, highlights=(Highlight(position=((-0.45 + 1.0) / 2.0, (0.25 + 1.0) / 2.0), sharpness=1.0, intensity=0.85, color=portrait),)),
                "Short Light": cls(level=0.12, highlights=(Highlight(position=((0.55 + 1.0) / 2.0, (0.3 + 1.0) / 2.0), sharpness=1.3, intensity=0.85, color=portrait),), vignette=0.2),
                "High Key": cls(level=0.7, highlights=(Highlight(position=(0.5, (0.5 + 1.0) / 2.0), sharpness=0.7, intensity=0.4, color=neutral),)),
                "Low Key": cls(level=0.03, highlights=(Highlight(position=((0.45 + 1.0) / 2.0, (0.2 + 1.0) / 2.0), sharpness=2.2, intensity=1.0, color=soft),), vignette=0.5),
            },
            Family.PORTRAIT: {
                "Rembrandt": cls(level=0.06, highlights=(Highlight(position=((-0.5 + 1.0) / 2.0, (0.45 + 1.0) / 2.0), sharpness=1.6, intensity=1.1, color=warm),)),
                "Rembrandt Right": cls(level=0.06, highlights=(Highlight(position=((0.5 + 1.0) / 2.0, (0.45 + 1.0) / 2.0), sharpness=1.6, intensity=1.1, color=warm),)),
                "Loop": cls(level=0.12, highlights=(Highlight(position=((0.35 + 1.0) / 2.0, (0.3 + 1.0) / 2.0), sharpness=1.4, intensity=0.95, color=Rgb(red=1.0, green=0.95, blue=0.88)),)),
                "Split": cls(level=0.05, highlights=(Highlight(position=((0.85 + 1.0) / 2.0, (0.1 + 1.0) / 2.0), sharpness=2.0, intensity=1.1, color=soft),), vignette=0.35),
                "Under Light": cls(level=0.06, highlights=(Highlight(position=(0.5, (-0.7 + 1.0) / 2.0), sharpness=1.6, intensity=1.0, color=beauty),)),
                "Hair Light": cls(level=0.1, highlights=(Highlight(position=(0.5, (0.5 + 1.0) / 2.0), sharpness=1.0, intensity=0.5, color=neutral),), rim=Gated(enabled=True, value=Rim(amount=0.9))),
            },
            Family.CINEMATIC: {
                "Film Noir": cls(
                    level=0.02,
                    highlights=(Highlight(position=((-0.8 + 1.0) / 2.0, (0.35 + 1.0) / 2.0), sharpness=2.4, intensity=1.2, color=neutral),),
                    blinds=Gated(enabled=True, value=Blinds()),
                    vignette=0.55,
                ),
                "Teal & Orange": cls(
                    highlights=(
                        Highlight(position=((0.6 + 1.0) / 2.0, (0.25 + 1.0) / 2.0), intensity=0.9, color=deep),
                        Highlight(position=((-0.6 + 1.0) / 2.0, (-0.1 + 1.0) / 2.0), sharpness=1.0, intensity=0.6, color=teal),
                    )
                ),
                "Neon M/C": cls(
                    level=0.04,
                    highlights=(
                        Highlight(position=((-0.55 + 1.0) / 2.0, (0.3 + 1.0) / 2.0), intensity=0.9, color=magenta),
                        Highlight(position=((0.55 + 1.0) / 2.0, (-0.1 + 1.0) / 2.0), intensity=0.9, color=cyan),
                    ),
                ),
                "Neon B/P": cls(
                    level=0.04,
                    highlights=(Highlight(position=((-0.5 + 1.0) / 2.0, 0.5), intensity=0.9, color=blue), Highlight(position=((0.55 + 1.0) / 2.0, (0.3 + 1.0) / 2.0), intensity=0.9, color=pink)),
                ),
                "Candlelight": cls(level=0.05, highlights=(Highlight(position=(0.5, (-0.1 + 1.0) / 2.0), sharpness=1.8, intensity=1.0, color=fire),), vignette=0.5),
                "Firelight": cls(level=0.06, gradient=(0.0, -0.1), tint=fire, highlights=(Highlight(position=(0.5, (-0.5 + 1.0) / 2.0), sharpness=1.0, intensity=0.9, color=fire),), vignette=0.35),
                "Moonlight": cls(level=0.05, highlights=(Highlight(position=((-0.4 + 1.0) / 2.0, (0.5 + 1.0) / 2.0), sharpness=1.4, intensity=0.7, color=sky),), vignette=0.3),
                "Moonlit Window": cls(
                    level=0.04, highlights=(Highlight(position=((0.5 + 1.0) / 2.0, (0.2 + 1.0) / 2.0), sharpness=1.0, color=sky),), blinds=Gated(enabled=True, value=Blinds(cycles=5.0)), vignette=0.35
                ),
                "Horror Uplight": cls(level=0.04, highlights=(Highlight(position=(0.5, (-0.7 + 1.0) / 2.0), sharpness=1.6, intensity=1.0, color=green),), vignette=0.45),
            },
            Family.ENVIRONMENT: {
                "Window Light": cls(level=0.12, highlights=(Highlight(position=((0.55 + 1.0) / 2.0, (0.35 + 1.0) / 2.0), sharpness=0.8, intensity=0.85, color=Rgb(red=1.0, green=0.98, blue=0.92)),)),
                "Window Blinds": cls(
                    level=0.1, highlights=(Highlight(position=((0.5 + 1.0) / 2.0, (0.3 + 1.0) / 2.0), sharpness=0.8, intensity=0.9, color=warm),), blinds=Gated(enabled=True, value=Blinds(cycles=7.0))
                ),
                "Forest Dapple": cls(level=0.1, gradient=(0.0, 0.3), tint=green, dapple=Gated(enabled=True, value=Dapple(count=14, seed=7))),
                "Underwater": cls(level=0.12, gradient=(0.0, 0.4), tint=teal, dapple=Gated(enabled=True, value=Dapple(seed=3))),
                "Snow Bounce": cls(
                    level=0.6, gradient=(0.0, 0.2), tint=Rgb(red=0.95, green=0.98, blue=1.0), highlights=(Highlight(position=(0.5, (-0.5 + 1.0) / 2.0), sharpness=0.8, intensity=0.3, color=neutral),)
                ),
                "Desert": cls(
                    level=0.35,
                    gradient=(0.2, 0.4),
                    tint=Rgb(red=1.0, green=0.85, blue=0.6),
                    highlights=(Highlight(position=((0.3 + 1.0) / 2.0, (0.7 + 1.0) / 2.0), sharpness=1.4, intensity=0.6, color=warm),),
                ),
                "Street Lamp": cls(level=0.03, highlights=(Highlight(position=(0.5, (0.4 + 1.0) / 2.0), sharpness=2.0, intensity=1.0, color=Rgb(red=1.0, green=0.8, blue=0.5)),), vignette=0.6),
                "Stage Spot": cls(level=0.02, highlights=(Highlight(position=(0.5, (0.6 + 1.0) / 2.0), sharpness=2.6, intensity=1.2, color=neutral),), vignette=0.6),
                "Concert RGB": cls(
                    level=0.04,
                    highlights=(
                        Highlight(position=((-0.6 + 1.0) / 2.0, (0.4 + 1.0) / 2.0), sharpness=1.4, color=blue),
                        Highlight(position=(0.5, (0.5 + 1.0) / 2.0), sharpness=1.4, color=magenta),
                        Highlight(position=((0.6 + 1.0) / 2.0, (0.4 + 1.0) / 2.0), sharpness=1.4, color=cyan),
                    ),
                ),
                "Sci-Fi Panel": cls(
                    level=0.06, highlights=(Highlight(position=((0.5 + 1.0) / 2.0, 0.5), sharpness=1.0, intensity=0.7, color=cyan),), blinds=Gated(enabled=True, value=Blinds(cycles=4.0)), vignette=0.3
                ),
            },
        }

    # --- [DRAWING]

    @classmethod
    def rolled(cls, roll: Roll) -> Self:
        """Draws native fields recursively over their declared presentation ranges."""
        generator = rng(content_hash(Roll), roll.seed)

        def drawn(kind: msgspec.inspect.Type, annotation: object) -> Option[object]:
            if isinstance(kind, msgspec.inspect.Metadata):
                return drawn(kind.type, annotation)
            if isinstance(kind, msgspec.inspect.FloatType):
                return next(iter(markers(annotation, Presentation)), Presentation()).soft.map(lambda soft: float(generator.uniform(*soft)))
            if isinstance(kind, msgspec.inspect.TupleType):
                return next(iter(markers(annotation, Presentation)), Presentation()).soft.map(lambda soft: tuple(float(value) for value in generator.uniform(*soft, size=len(kind.item_types))))
            if isinstance(kind, msgspec.inspect.StructType):
                if (record := kind.cls) is Rgb or get_origin(record) is Rgb:
                    red, green, blue = colorsys.hsv_to_rgb(float(generator.random()), roll.saturation, 1.0)
                    return Some(Rgb(red=red, green=green, blue=blue))
                if get_origin(record) is Gated:
                    values = fields(record)
                    return Some(record(enabled=bool(generator.integers(2)), **values))
                return Some(record(**fields(record)))
            if isinstance(kind, msgspec.inspect.VarTupleType) and isinstance(kind.item_type, msgspec.inspect.StructType):
                record = kind.item_type.cls
                return Some(tuple(record(**fields(record)) for _ in range(roll.highlights)))
            return Nothing

        def fields(record: type[msgspec.Struct]) -> dict[str, object]:
            return {field.name: value for field in msgspec.structs.fields(record) for value in drawn(msgspec.inspect.type_info(field.type), field.type).to_list()}

        return msgspec.structs.replace(cls(), **fields(cls))

    def plane(self, size: Side | tuple[Side, Side]) -> RenderingResult[npt.NDArray[np.float32]]:
        """Returns unclipped float32 RGB at pixel centers, size as side or (width, height)."""
        width, height = (size, size) if isinstance(size, int) else size

        def formed(axes: tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]) -> RenderingResult[npt.NDArray[np.float32]]:
            def glow(x: float, y: float, sharpness: float) -> npt.NDArray[np.float64]:
                if sharpness == 0.0:
                    return np.ones_like(xs)
                with np.errstate(over="ignore", under="ignore"):
                    distance = np.hypot((xs + 1.0) / 2.0 - x, (ys + 1.0) / 2.0 - y)
                    return np.exp(-((distance * math.sqrt(sharpness)) ** 2) * 4.0)

            try:
                speckles = rng(content_hash(Dapple), self.dapple.value.seed).random((self.dapple.value.count, 4)) if self.dapple.enabled else np.empty((0, 4))
            except (OverflowError, ValueError, MemoryError) as error:
                return Error((RenderingSizeFault(request=(self.dapple.value.count, 4), error=error),))
            try:
                with np.errstate(over="raise", invalid="raise", divide="raise", under="ignore"):
                    right, up = axes
                    xs, ys = right * 2.0 / width, up * 2.0 / height
                    reach, tint = np.hypot(xs, ys), np.array(msgspec.structs.astuple(self.tint))
                    dapple, blinds, rim = self.dapple.value, self.blinds.value, self.rim.value
                    floor = _product((self.level, tint)) + _product((np.maximum(self.gradient[0] * (xs + 1.0) / 2.0 + self.gradient[1] * (ys + 1.0) / 2.0, 0.0)[..., None], tint))
                    lit = sum((_product((spot.intensity, glow(*spot.position, spot.sharpness)[..., None], np.array(msgspec.structs.astuple(spot.color)))) for spot in self.highlights), floor)
                    speckled = (
                        lit
                        + sum(
                            (
                                _product((dapple.intensity, envelope, tint)) + _product((float(intensity), dapple.intensity_spread, envelope, tint))
                                for x, y, sharpness, intensity in speckles
                                for envelope in ((glow(x, y, dapple.sharpness) * glow(x, y, float(sharpness * dapple.sharpness_spread)))[..., None],)
                            ),
                            np.zeros_like(lit),
                        )
                        if self.dapple.enabled
                        else lit
                    )
                    phase = np.remainder((np.remainder(blinds.cycles, 2.0 * height) / height) * (height - 2.0 * np.arange(height) - 1.0), 2.0)
                    shaded = speckled * (blinds.shade + (1.0 - blinds.shade) * ((phase > 0.0) & (phase < 1.0)))[:, None, None] if self.blinds.enabled else speckled
                    rimmed = (
                        shaded + _product((rim.amount, np.maximum(reach - rim.radius, 0.0)[..., None] ** rim.falloff, np.array(msgspec.structs.astuple(rim.color))))
                        if self.rim.enabled and rim.amount != 0.0
                        else shaded
                    )
                    return Ok(np.ascontiguousarray(_product((rimmed, (1.0 - self.vignette * np.minimum(reach**2, 1.0))[..., None])), dtype=np.float32))
            except (OverflowError, FloatingPointError) as error:
                return Error((RenderingArithmeticFault.of(self, error),))

        try:
            return _grid((width, height)).bind(formed)
        except MemoryError as error:
            return Error((RenderingSizeFault(request=(width, height), error=error),))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GEOMETRY]


def _grid(size: tuple[Side, Side]) -> RenderingResult[tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]]:
    """Returns pixel-center offsets from the image center, right and up, rows top first."""
    width, height = size
    try:
        with np.errstate(over="raise", invalid="raise", divide="raise", under="ignore"):
            right, up = np.meshgrid(np.arange(width, dtype=np.float64) + 0.5 - width / 2.0, height / 2.0 - np.arange(height, dtype=np.float64) - 0.5)
            return Ok((right, up))
    except (OverflowError, FloatingPointError, ValueError, MemoryError) as error:
        return Error((RenderingSizeFault(request=size, error=error),))


def _ball(size: tuple[Side, Side]) -> RenderingResult[tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]]:
    """Returns the inscribed sphere's silhouette normals and pixel coverage."""

    def formed(axes: tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]) -> RenderingResult[tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]]:
        try:
            radius = min(size) / 2.0
            xs, ys = (axis / radius for axis in axes)
            reach = np.hypot(xs, ys)
            radial = np.minimum(reach, 1.0)
            scale = np.divide(radial, reach, out=np.ones_like(reach), where=reach > 0.0)
            return Ok((np.stack((xs * scale, ys * scale, np.sqrt((1.0 - radial) * (1.0 + radial))), axis=-1), np.clip((1.0 - reach) * radius + 0.5, 0.0, 1.0)[..., None]))
        except (OverflowError, FloatingPointError, MemoryError) as error:
            return Error((RenderingSizeFault(request=size, error=error),))

    return _grid(size).bind(formed)


# --- [IMAGES]


def _product(factors: tuple[float | npt.NDArray[np.float64], ...], stops: float = 0.0) -> npt.NDArray[np.float64]:
    """Forms broadcast radiometric products before applying their common exponent."""
    fraction, whole = math.modf(stops)
    parts = tuple(np.frexp(factor) for factor in np.broadcast_arrays(*factors))
    mantissas = np.prod(np.stack(tuple(mantissa for mantissa, _ in parts)), axis=0) * math.exp2(fraction)
    exponents = np.sum(np.stack(tuple(exponent for _, exponent in parts)), axis=0)
    return (
        np.ldexp(mantissas, exponents) if whole == 0.0 else np.vectorize(lambda mantissa, exponent: math.ldexp(float(mantissa), int(exponent) + int(whole)), otypes=[np.float64])(mantissas, exponents)
    )


def sphere(lights: tuple[DistantLight, ...], size: tuple[Side, Side]) -> RenderingResult[npt.NDArray[np.float32]]:
    """Returns a rectangular float32 associated RGBA sphere under directional radiance."""

    def formed(geometry: tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]) -> RenderingResult[npt.NDArray[np.float32]]:
        normals, coverage = geometry
        try:
            with np.errstate(over="raise", invalid="raise", divide="raise", under="ignore"):
                lit = sum(
                    (_product((np.maximum(normals @ light.direction, 0.0)[..., None], coverage, np.array(msgspec.structs.astuple(light.color))), light.stops) for light in lights),
                    np.zeros((*normals.shape[:2], 3)),
                )
                return Ok(np.ascontiguousarray(np.concatenate((lit, coverage), axis=-1), dtype=np.float32))
        except (OverflowError, FloatingPointError) as error:
            return Error((RenderingArithmeticFault.of(lights, error),))

    try:
        return _ball(size).bind(formed)
    except MemoryError as error:
        return Error((RenderingSizeFault(request=size, error=error),))


def gray_ball(placements: Placements, config: PyOpenColorIO.Config, size: Side) -> RenderingResult[npt.NDArray[np.float32]]:
    """Returns a gray Lambertian ball under placed lights, or typed color-processor faults."""

    def rendered(prepared: tuple[tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]], tuple[npt.NDArray[np.float64], ...]]) -> RenderingResult[npt.NDArray[np.float32]]:
        geometry, temperature_colors = prepared
        normals, coverage = geometry
        converted = iter(temperature_colors)
        colors = tuple(next(converted) if placement.light.temperature.enabled else np.ones(3) for placement in placements.lights)

        def received(placement: Placement) -> npt.NDArray[np.float64]:
            direction = np.array(placement.light.direction)
            match placement.light.emitter:
                case Sun():
                    towards = direction
                    alpha, level = np.full(normals.shape[:2], placement.light.size / 2.0), np.ones(normals.shape[:2])
                case Point() | Disk() | Spot():
                    offset = direction - normals / placements.distance
                    distance = np.hypot.reduce(offset, axis=-1)
                    extent = math.sin(placement.light.size / 2.0)
                    towards, alpha = offset / distance[..., None], np.arcsin(np.minimum(extent / distance, 1.0))
                    level = (1.0 + extent**2) / (distance**2 + extent**2)
                    if isinstance(placement.light.emitter, Spot):
                        cone, blend = placement.light.emitter.cone, placement.light.emitter.blend
                        along, edge = towards @ direction, math.cos(cone / 2.0)
                        ramp = np.clip(np.divide(along - edge, (1.0 - edge) * blend, out=np.where(along > edge, 1.0, 0.0), where=blend > 0.0), 0.0, 1.0)
                        level = level * ramp**2 * (3.0 - 2.0 * ramp)
            cosine = np.sum(normals * towards, axis=-1)
            sine = np.linalg.norm(np.cross(normals, towards), axis=-1)
            delta = np.arctan2(sine, cosine) - math.pi / 2.0
            straddle = np.abs(delta) < alpha
            alpha_sine = np.sin(alpha)
            spread = alpha_sine**2
            cot = np.divide(np.cos(alpha), alpha_sine, out=np.zeros_like(sine), where=straddle)
            y = np.divide(-cot * cosine, sine, out=np.zeros_like(sine), where=straddle)
            z = np.divide(np.sqrt(np.sin(np.where(straddle, alpha + delta, 0.0)) * np.sin(np.where(straddle, alpha - delta, 0.0))), alpha_sine, out=np.zeros_like(sine), where=straddle)
            cap = np.divide((cosine * np.arccos(y) - cot * z) * spread + np.arctan2(z, cot), math.pi * spread, out=np.zeros_like(sine), where=straddle)
            return level * np.where(straddle, cap, np.maximum(cosine, 0.0))

        try:
            with np.errstate(over="raise", invalid="raise", divide="raise", under="ignore"):
                world = placements.background.map(
                    lambda background: _product((background.strength, np.array(msgspec.structs.astuple(background.color)), 1.0 - coverage + coverage * GREY))
                ).default_with(lambda: np.zeros((size, size, 3)))
                lit = sum(
                    (
                        _product((received(placement)[..., None], color, np.array(msgspec.structs.astuple(placement.light.color)), placement.irradiance, coverage, GREY / math.pi))
                        for placement, color in zip(placements.lights, colors)
                        if placement.irradiance != 0.0
                    ),
                    world,
                )
                return Ok(np.ascontiguousarray(lit, dtype=np.float32))
        except (OverflowError, FloatingPointError) as error:
            return Error((RenderingArithmeticFault.of(placements, error),))

    def admitted(row: npt.NDArray[np.float32]) -> RenderingResult[npt.NDArray[np.float64]]:
        try:
            color = msgspec.convert(row.tolist(), type=Rgb[Finite])
            return Ok(np.asarray(msgspec.structs.astuple(color), dtype=np.float64))
        except msgspec.ValidationError as error:
            return Error((RenderingArithmeticFault.of(placements, error),))

    def tinted(built: PyOpenColorIO.Processor) -> RenderingResult[tuple[npt.NDArray[np.float64], ...]]:
        return accumulated(admitted(row) for row in applied(built.getDefaultCPUProcessor(), np.array(temperatures, dtype=np.float32)))

    def refused(faults: ImagingFaults) -> RenderingFaults:
        return (RenderingImagingFault(faults=faults),)

    try:
        temperatures = tuple(White(temperature=placement.light.temperature.value, tint=0.0).xyz for placement in placements.lights if placement.light.temperature.enabled)
        colors = (
            processor(config, PyOpenColorIO.GroupTransform([xyz(config, PyOpenColorIO.ROLE_SCENE_LINEAR)], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE)).map_error(refused).bind(tinted)
            if temperatures
            else Ok(())
        )
        return paired(_ball((size, size)), colors).bind(rendered)
    except MemoryError as error:
        return Error((RenderingSizeFault(request=(size, size), error=error),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Blinds", "Dapple", "DistantLight", "Family", "Highlight", "ReferenceDistribution", "ReferenceImage", "Rim", "Roll", "gray_ball", "sphere"]
