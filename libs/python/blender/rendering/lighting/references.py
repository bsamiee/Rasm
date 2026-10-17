"""Editable lighting references, seeded distributions, and Lambertian sphere images."""

from collections.abc import Mapping
import colorsys
import functools
import math
from typing import Annotated, get_origin, Self

from expression import Nothing, Ok, Option, Some
import msgspec.inspect
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.imaging.chain import GREY
from blender.imaging.color.processors import applied, processor, xyz
from blender.imaging.color.white import White
from blender.imaging.faults import accumulated, guarded
from blender.imaging.hashing import content_hash, rng
from blender.imaging.parameters import Choice, Finite, Gated, NonNegative, Position, Presentation, Rgb, Seed, Unit
from blender.rendering.faults import RenderingArithmeticFault, RenderingImagingFault, RenderingResult
from blender.rendering.lighting.rigs import Disk, Placement, Placements, Point, product, Spot, Sun
from blender.rendering.outputs.framing import Side

# --- [TYPES] ----------------------------------------------------------------------------


class Family(Choice):
    """Family of lighting references."""

    SKY = 1, "Time of Day"
    STUDIO = 2
    PORTRAIT = 3
    CINEMATIC = 4
    ENVIRONMENT = 5


class ReferenceDistribution(Choice):
    """Color and spatial strategy for a seeded lighting reference."""

    BALANCED = 1
    VIVID = 2
    SOFT = 3


# --- [MODELS] ---------------------------------------------------------------------------


class DistantLight(msgspec.Struct, frozen=True, kw_only=True):
    """Directional radiance and its exposure offset for a sphere image."""

    direction: tuple[Finite, Finite, Finite]
    color: Rgb[Finite]
    stops: Finite = 0.0


class Highlight(msgspec.Struct, frozen=True, kw_only=True):
    """One colored Gaussian glow centered in a reference frame."""

    position: Position = (0.5, 0.5)
    sharpness: Annotated[NonNegative, Presentation(soft=Some((0.55, 2.4)))] = 1.2
    intensity: Annotated[NonNegative, Presentation(soft=Some((0.35, 1.45)))] = 0.8
    color: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)


class Dapple(msgspec.Struct, frozen=True, kw_only=True):
    """Seeded spots with independent position, sharpness, and intensity draws."""

    count: Annotated[int, msgspec.Meta(ge=0)] = 12
    sharpness: Annotated[NonNegative, Presentation(soft=Some((8.0, 22.0)))] = 8.0
    intensity: Annotated[NonNegative, Presentation(soft=Some((0.3, 0.8)))] = 0.3
    sharpness_spread: Annotated[NonNegative, msgspec.Meta(description="Uniform variation above the minimum sharpness"), Presentation(soft=Some((0.0, 14.0)))] = 14.0
    intensity_spread: Annotated[NonNegative, msgspec.Meta(description="Uniform variation above the minimum intensity"), Presentation(soft=Some((0.0, 0.5)))] = 0.5
    seed: Seed = 0


class Blinds(msgspec.Struct, frozen=True, kw_only=True):
    """Horizontal slat shadows across the reference."""

    cycles: Annotated[NonNegative, Presentation(soft=Some((5.0, 7.0)))] = 6.0
    shade: Unit = 0.45


class Rim(msgspec.Struct, frozen=True, kw_only=True):
    """Colored glow rising toward the edges of the reference."""

    amount: Annotated[NonNegative, Presentation(soft=Some((0.0, 1.2)))] = 0.6
    color: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)
    radius: Annotated[float, msgspec.Meta(ge=0.0, le=math.sqrt(2.0))] = 0.5
    falloff: NonNegative = 1.4


class Roll(msgspec.Struct, frozen=True, kw_only=True):
    """Draw of arbitrary editable fields over their presentation ranges."""

    highlights: Annotated[int, msgspec.Meta(ge=1, le=8)] = 3
    saturation: Unit = 0.8
    seed: Seed = 0


class ReferenceImage(msgspec.Struct, frozen=True, kw_only=True):
    """Editable scene-linear reference with floor, glows, shadows, and edge light."""

    tint: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)
    level: Annotated[NonNegative, Presentation(soft=Some((0.03, 0.55)))] = 0.08
    gradient: Annotated[tuple[Finite, Finite], Presentation(soft=Some((0.0, 0.6)))] = (0.0, 0.0)
    highlights: tuple[Highlight, ...] = ()
    dapple: Gated[Dapple] = Gated(enabled=False, value=Dapple())
    blinds: Gated[Blinds] = Gated(enabled=False, value=Blinds())
    rim: Gated[Rim] = Gated(enabled=False, value=Rim())
    vignette: Annotated[Unit, Presentation(soft=Some((0.0, 0.5)))] = 0.0

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
        """Returns the named source colors in scene-linear RGB."""
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
    @functools.cache
    def catalog(cls) -> Mapping[Family, Mapping[str, Self]]:
        """Constructs every named source recipe in its family."""
        gels = cls.gels()
        warm, deep, neutral, cool = gels["Warm"], gels["Deep Warm"], gels["Neutral"], gels["Cool"]
        sky, magenta, cyan, pink = gels["Sky"], gels["Magenta"], gels["Cyan"], gels["Pink"]
        blue, green, fire, teal = gels["Blue"], gels["Green"], gels["Fire"], gels["Teal"]
        beauty = Rgb(red=1.0, green=0.95, blue=0.9)
        portrait = Rgb(red=1.0, green=0.96, blue=0.9)
        soft = Rgb(red=1.0, green=0.97, blue=0.92)
        return {
            Family.SKY: {
                "Golden Hour": cls(level=0.2, gradient=(0.9, 0.1), tint=warm, highlights=(Highlight(position=(0.85, 0.65), sharpness=1.2, intensity=0.7, color=warm),)),
                "Sunrise": cls(
                    level=0.16,
                    gradient=(0.5, 0.2),
                    tint=Rgb(red=1.0, green=0.7, blue=0.45),
                    highlights=(Highlight(position=(0.2, 0.4), sharpness=1.4, intensity=0.8, color=Rgb(red=1.0, green=0.65, blue=0.4)),),
                ),
                "Sunset": cls(level=0.18, gradient=(0.6, 0.0), tint=deep, highlights=(Highlight(position=(0.8, 0.375), sharpness=1.3, intensity=0.9, color=deep),), vignette=0.3),
                "Blue Hour": cls(level=0.12, gradient=(0.0, 0.6), tint=cool),
                "Twilight": cls(level=0.1, gradient=(0.0, 0.5), tint=Rgb(red=0.7, green=0.6, blue=1.0), highlights=(Highlight(position=(0.5, 0.9), sharpness=1.0, intensity=0.4, color=magenta),)),
                "Midday": cls(level=0.4, gradient=(0.0, 0.5), highlights=(Highlight(position=(0.55, 0.9), sharpness=1.4, intensity=0.6, color=neutral),)),
                "Harsh Noon": cls(level=0.25, highlights=(Highlight(position=(0.5, 0.925), sharpness=3.0, intensity=1.1, color=neutral),), vignette=0.45),
                "Overcast": cls(level=0.55, gradient=(0.0, 0.25), tint=Rgb(red=0.95, green=0.97, blue=1.0)),
                "Foggy Morning": cls(level=0.5, gradient=(0.0, 0.3), tint=Rgb(red=0.9, green=0.95, blue=1.0), vignette=0.2),
                "Night Sky": cls(level=0.04, gradient=(0.0, 0.25), tint=cool, highlights=(Highlight(position=(0.7, 0.9), sharpness=6.0, intensity=0.5, color=Rgb(red=0.8, green=0.9, blue=1.0)),)),
                "Top Sky": cls(gradient=(0.0, 0.95), tint=Rgb(red=0.85, green=0.92, blue=1.0)),
            },
            Family.STUDIO: {
                "Softbox": cls(level=0.5, highlights=(Highlight(position=(0.5, 0.7), sharpness=0.6, intensity=0.5, color=neutral),)),
                "Softbox Left": cls(level=0.18, highlights=(Highlight(position=(0.2, 0.675), sharpness=0.9, intensity=0.8, color=neutral),)),
                "Softbox Right": cls(level=0.18, highlights=(Highlight(position=(0.8, 0.675), sharpness=0.9, intensity=0.8, color=neutral),)),
                "Beauty Dish": cls(level=0.2, highlights=(Highlight(position=(0.5, 0.725), sharpness=1.6, intensity=0.9, color=neutral),), rim=Gated(enabled=True, value=Rim(amount=0.25))),
                "Ring Light": cls(level=0.18, highlights=(Highlight(sharpness=0.8, intensity=0.7, color=neutral),), rim=Gated(enabled=True, value=Rim(amount=0.7))),
                "Clamshell": cls(level=0.25, highlights=(Highlight(position=(0.5, 0.8), intensity=0.7, color=neutral), Highlight(position=(0.5, 0.2), intensity=0.45, color=neutral))),
                "Butterfly": cls(level=0.2, highlights=(Highlight(position=(0.5, 0.775), sharpness=1.4, intensity=0.85, color=beauty),)),
                "Broad Light": cls(level=0.16, highlights=(Highlight(position=(0.275, 0.625), sharpness=1.0, intensity=0.85, color=portrait),)),
                "Short Light": cls(level=0.12, highlights=(Highlight(position=(0.775, 0.65), sharpness=1.3, intensity=0.85, color=portrait),), vignette=0.2),
                "High Key": cls(level=0.7, highlights=(Highlight(position=(0.5, 0.75), sharpness=0.7, intensity=0.4, color=neutral),)),
                "Low Key": cls(level=0.03, highlights=(Highlight(position=(0.725, 0.6), sharpness=2.2, intensity=1.0, color=soft),), vignette=0.5),
            },
            Family.PORTRAIT: {
                "Rembrandt": cls(level=0.06, highlights=(Highlight(position=(0.25, 0.725), sharpness=1.6, intensity=1.1, color=warm),)),
                "Rembrandt Right": cls(level=0.06, highlights=(Highlight(position=(0.75, 0.725), sharpness=1.6, intensity=1.1, color=warm),)),
                "Loop": cls(level=0.12, highlights=(Highlight(position=(0.675, 0.65), sharpness=1.4, intensity=0.95, color=Rgb(red=1.0, green=0.95, blue=0.88)),)),
                "Split": cls(level=0.05, highlights=(Highlight(position=(0.925, 0.55), sharpness=2.0, intensity=1.1, color=soft),), vignette=0.35),
                "Under Light": cls(level=0.06, highlights=(Highlight(position=(0.5, 0.15), sharpness=1.6, intensity=1.0, color=beauty),)),
                "Hair Light": cls(level=0.1, highlights=(Highlight(position=(0.5, 0.75), sharpness=1.0, intensity=0.5, color=neutral),), rim=Gated(enabled=True, value=Rim(amount=0.9))),
            },
            Family.CINEMATIC: {
                "Film Noir": cls(level=0.02, highlights=(Highlight(position=(0.1, 0.675), sharpness=2.4, intensity=1.2, color=neutral),), blinds=Gated(enabled=True, value=Blinds()), vignette=0.55),
                "Teal & Orange": cls(highlights=(Highlight(position=(0.8, 0.625), intensity=0.9, color=deep), Highlight(position=(0.2, 0.45), sharpness=1.0, intensity=0.6, color=teal))),
                "Neon M/C": cls(level=0.04, highlights=(Highlight(position=(0.225, 0.65), intensity=0.9, color=magenta), Highlight(position=(0.775, 0.45), intensity=0.9, color=cyan))),
                "Neon B/P": cls(level=0.04, highlights=(Highlight(position=(0.25, 0.5), intensity=0.9, color=blue), Highlight(position=(0.775, 0.65), intensity=0.9, color=pink))),
                "Candlelight": cls(level=0.05, highlights=(Highlight(position=(0.5, 0.45), sharpness=1.8, intensity=1.0, color=fire),), vignette=0.5),
                "Firelight": cls(level=0.06, gradient=(0.0, -0.1), tint=fire, highlights=(Highlight(position=(0.5, 0.25), sharpness=1.0, intensity=0.9, color=fire),), vignette=0.35),
                "Moonlight": cls(level=0.05, highlights=(Highlight(position=(0.3, 0.75), sharpness=1.4, intensity=0.7, color=sky),), vignette=0.3),
                "Moonlit Window": cls(level=0.04, highlights=(Highlight(position=(0.75, 0.6), sharpness=1.0, color=sky),), blinds=Gated(enabled=True, value=Blinds(cycles=5.0)), vignette=0.35),
                "Horror Uplight": cls(level=0.04, highlights=(Highlight(position=(0.5, 0.15), sharpness=1.6, intensity=1.0, color=green),), vignette=0.45),
            },
            Family.ENVIRONMENT: {
                "Window Light": cls(level=0.12, highlights=(Highlight(position=(0.775, 0.675), sharpness=0.8, intensity=0.85, color=Rgb(red=1.0, green=0.98, blue=0.92)),)),
                "Window Blinds": cls(level=0.1, highlights=(Highlight(position=(0.75, 0.65), sharpness=0.8, intensity=0.9, color=warm),), blinds=Gated(enabled=True, value=Blinds(cycles=7.0))),
                "Forest Dapple": cls(level=0.1, gradient=(0.0, 0.3), tint=green, dapple=Gated(enabled=True, value=Dapple(count=14, seed=7))),
                "Underwater": cls(level=0.12, gradient=(0.0, 0.4), tint=teal, dapple=Gated(enabled=True, value=Dapple(seed=3))),
                "Snow Bounce": cls(
                    level=0.6, gradient=(0.0, 0.2), tint=Rgb(red=0.95, green=0.98, blue=1.0), highlights=(Highlight(position=(0.5, 0.25), sharpness=0.8, intensity=0.3, color=neutral),)
                ),
                "Desert": cls(level=0.35, gradient=(0.2, 0.4), tint=Rgb(red=1.0, green=0.85, blue=0.6), highlights=(Highlight(position=(0.65, 0.85), sharpness=1.4, intensity=0.6, color=warm),)),
                "Street Lamp": cls(level=0.03, highlights=(Highlight(position=(0.5, 0.7), sharpness=2.0, intensity=1.0, color=Rgb(red=1.0, green=0.8, blue=0.5)),), vignette=0.6),
                "Stage Spot": cls(level=0.02, highlights=(Highlight(position=(0.5, 0.8), sharpness=2.6, intensity=1.2, color=neutral),), vignette=0.6),
                "Concert RGB": cls(
                    level=0.04,
                    highlights=(
                        Highlight(position=(0.2, 0.7), sharpness=1.4, color=blue),
                        Highlight(position=(0.5, 0.75), sharpness=1.4, color=magenta),
                        Highlight(position=(0.8, 0.7), sharpness=1.4, color=cyan),
                    ),
                ),
                "Sci-Fi Panel": cls(
                    level=0.06, highlights=(Highlight(position=(0.75, 0.5), sharpness=1.0, intensity=0.7, color=cyan),), blinds=Gated(enabled=True, value=Blinds(cycles=4.0)), vignette=0.3
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
                return Presentation.of(annotation).soft.map(lambda soft: float(generator.uniform(*soft)))
            if isinstance(kind, msgspec.inspect.TupleType):
                return Presentation.of(annotation).soft.map(lambda soft: tuple(float(value) for value in generator.uniform(*soft, size=len(kind.item_types))))
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

    def plane(self, size: Side) -> RenderingResult[npt.NDArray[np.float32]]:
        """Returns unclipped float32 RGB at pixel centers of a square side."""

        def formed() -> RenderingResult[npt.NDArray[np.float32]]:
            def glow(x: float, y: float, sharpness: float) -> npt.NDArray[np.float64]:
                if sharpness == 0.0:
                    return np.ones_like(xs)
                with np.errstate(over="ignore", under="ignore"):
                    distance = np.hypot(across - x, along - y)
                    return np.exp(-((distance * math.sqrt(sharpness)) ** 2) * 4.0)

            speckles = rng(content_hash(Dapple), self.dapple.value.seed).random((self.dapple.value.count, 4)) if self.dapple.enabled else np.empty((0, 4))
            xs, ys = _grid(size)
            across, along = (xs + 1.0) / 2.0, (ys + 1.0) / 2.0
            reach, tint = np.hypot(xs, ys), np.array(self.tint)
            dapple, blinds, rim = self.dapple.value, self.blinds.value, self.rim.value
            floor = product((self.level, tint)) + product((np.maximum(self.gradient[0] * across + self.gradient[1] * along, 0.0)[..., None], tint))
            lit = sum((product((spot.intensity, glow(*spot.position, spot.sharpness)[..., None], np.array(spot.color))) for spot in self.highlights), floor)
            speckled = sum(
                (
                    product((dapple.intensity, envelope, tint)) + product((float(intensity), dapple.intensity_spread, envelope, tint))
                    for x, y, sharpness, intensity in speckles
                    for envelope in ((glow(x, y, dapple.sharpness) * glow(x, y, float(sharpness * dapple.sharpness_spread)))[..., None],)
                ),
                lit,
            )
            phase = np.remainder((np.remainder(blinds.cycles, 2.0 * size) / size) * (size - 2.0 * np.arange(size) - 1.0), 2.0)
            shaded = speckled * (blinds.shade + (1.0 - blinds.shade) * ((phase > 0.0) & (phase < 1.0)))[:, None, None] if self.blinds.enabled else speckled
            rimmed = shaded + product((rim.amount, np.maximum(reach - rim.radius, 0.0)[..., None] ** rim.falloff, np.array(rim.color))) if self.rim.enabled and rim.amount != 0.0 else shaded
            return Ok(np.ascontiguousarray(product((rimmed, (1.0 - self.vignette * np.minimum(reach**2, 1.0))[..., None])), dtype=np.float32))

        return guarded(RenderingArithmeticFault, self, formed)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GEOMETRY]


def _grid(side: Side) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]:
    """Returns pixel-center offsets from the center of a square image in half sides, right and up, rows top first."""
    centers = (np.arange(side, dtype=np.float64) + 0.5 - side / 2.0) / (side / 2.0)
    return np.meshgrid(centers, -centers)


def _ball(side: Side) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]:
    """Returns the silhouette normals and pixel coverage of the sphere inscribed in a square image."""
    radius = side / 2.0
    xs, ys = _grid(side)
    reach = np.hypot(xs, ys)
    radial = np.minimum(reach, 1.0)
    scale = np.divide(radial, reach, out=np.ones_like(reach), where=reach > 0.0)
    return np.stack((xs * scale, ys * scale, np.sqrt((1.0 - radial) * (1.0 + radial))), axis=-1), np.clip((1.0 - reach) * radius + 0.5, 0.0, 1.0)[..., None]


# --- [IMAGES]


def sphere(lights: tuple[DistantLight, ...], size: Side) -> RenderingResult[npt.NDArray[np.float32]]:
    """Returns a square float32 associated RGBA sphere under directional radiance."""

    def formed() -> RenderingResult[npt.NDArray[np.float32]]:
        normals, coverage = _ball(size)
        lit = sum((product((np.maximum(normals @ light.direction, 0.0)[..., None], coverage, np.array(light.color)), light.stops) for light in lights), np.zeros((*normals.shape[:2], 3)))
        return Ok(np.ascontiguousarray(np.concatenate((lit, coverage), axis=-1), dtype=np.float32))

    return guarded(RenderingArithmeticFault, lights, formed)


def gray_ball(placements: Placements, config: PyOpenColorIO.Config, size: Side) -> RenderingResult[npt.NDArray[np.float32]]:
    """Returns a gray Lambertian ball under placed lights, or typed color-processor faults."""

    def rendered(temperature_colors: tuple[npt.NDArray[np.float64], ...]) -> RenderingResult[npt.NDArray[np.float32]]:
        normals, coverage = _ball(size)
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
                    extent = placement.light.extent
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

        world = placements.background.map(lambda background: product((background.strength, np.array(background.color), 1.0 - coverage + coverage * GREY))).default_with(
            lambda: np.zeros((size, size, 3))
        )
        lit = sum(
            (
                product((received(placement)[..., None], color, np.array(placement.light.color), placement.irradiance, coverage, GREY / math.pi))
                for placement, color in zip(placements.lights, colors)
                if placement.irradiance != 0.0
            ),
            world,
        )
        return Ok(np.ascontiguousarray(lit, dtype=np.float32))

    def admitted(row: npt.NDArray[np.float32]) -> RenderingResult[npt.NDArray[np.float64]]:
        return guarded(RenderingArithmeticFault, placements, lambda: Ok(np.asarray(msgspec.convert(row.tolist(), type=Rgb[Finite]), dtype=np.float64)))

    def tinted(built: PyOpenColorIO.Processor) -> RenderingResult[tuple[npt.NDArray[np.float64], ...]]:
        return accumulated(admitted(row) for row in applied(built.getDefaultCPUProcessor(), np.array(temperatures, dtype=np.float32)))

    temperatures = tuple(White(temperature=placement.light.temperature.value, tint=0.0).xyz for placement in placements.lights if placement.light.temperature.enabled)
    colors = (
        processor(config, PyOpenColorIO.GroupTransform([xyz(config, PyOpenColorIO.ROLE_SCENE_LINEAR)], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE))
        .map_error(RenderingImagingFault.lifted)
        .bind(tinted)
        if temperatures
        else Ok(())
    )
    return colors.bind(lambda found: guarded(RenderingArithmeticFault, placements, lambda: rendered(found)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Blinds", "Dapple", "DistantLight", "Family", "Highlight", "ReferenceDistribution", "ReferenceImage", "Rim", "Roll", "gray_ball", "sphere"]
