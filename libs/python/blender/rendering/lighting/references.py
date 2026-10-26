"""Scene-linear lighting reference images."""

from collections.abc import Mapping
from enum import IntEnum
import functools
import math
from typing import Annotated, Self

from expression import Option
from expression.extra.result import catch
import msgspec.inspect
import numpy as np
import numpy.typing as npt

from blender.records.faults import Fault, Outcome, RecordReason
from blender.records.fields import Finite, NonNegative, Seed, Side, Unit
from blender.records.identity import content_hash, rng

# --- [TYPES] ----------------------------------------------------------------------------


class Family(IntEnum):
    """Lighting reference family."""

    SKY = 1
    STUDIO = 2
    PORTRAIT = 3
    CINEMATIC = 4
    ENVIRONMENT = 5


class ReferenceDistribution(IntEnum):
    """Seeded reference color and placement strategy."""

    BALANCED = 1
    VIVID = 2
    SOFT = 3

    @property
    def draw(self) -> "Draw":
        """Colors, placement, and field ranges references of this distribution draw from."""
        match self:
            case ReferenceDistribution.BALANCED:
                gels = tuple(ReferenceImage.gels().values())
                return Draw(
                    colors=gels,
                    placement=Scattered(count=(1, 3), horizontal=(-0.8, 0.8), vertical=(-0.7, 0.85)),
                    sharpness=(0.7, 2.4),
                    intensity=(0.5, 1.1),
                    level=(0.03, 0.5),
                    gradient=(0.0, 0.6),
                    tints=gels,
                    rim=(0.4, 1.2),
                    vignette=(0.5, 0.0, 0.5),
                    cycles=(0.0, 0.0, 0.0, 5.0, 7.0),
                    dapples=(0, 0, 0, 12),
                )
            case ReferenceDistribution.VIVID:
                return Draw(
                    colors=((1.4, 0.12, 0.85), (0.15, 0.75, 1.35), (1.35, 0.55, 0.05), (0.25, 1.05, 0.35), (0.75, 0.25, 1.35)),
                    placement=Slotted(
                        positions=((-0.62, 0.18), (0.62, 0.18), (0.0, 0.58), (-0.38, -0.42), (0.38, -0.42)),
                        spreads=(((0, 1, 3), (0, 1, 4), (0, 2, 4), (1, 2, 4)), ((0, 1, 2, 3), (0, 1, 3, 4), (0, 2, 3, 4)), ((0, 1, 2, 3, 4),)),
                        jitter=0.08,
                    ),
                    sharpness=(1.3, 2.1),
                    intensity=(0.85, 1.45),
                    level=(0.03, 0.1),
                    gradient=(0.0, 0.15),
                    tints=((0.42, 0.46, 0.52),),
                    rim=(0.25, 0.5),
                    vignette=(1.0, 0.15, 0.45),
                )
            case ReferenceDistribution.SOFT:
                return Draw(
                    colors=((1.0, 0.96, 0.92), (0.94, 0.96, 1.0), (0.98, 0.95, 0.9)),
                    placement=Scattered(count=(1, 2), horizontal=(-0.45, 0.45), vertical=(-0.35, 0.55)),
                    sharpness=(0.55, 1.0),
                    intensity=(0.35, 0.65),
                    level=(0.32, 0.55),
                    gradient=(0.0, 0.25),
                    tints=((0.97, 0.95, 0.92), (0.93, 0.95, 0.98)),
                    rim=(0.0, 0.0),
                    vignette=(0.3, 0.0, 0.2),
                )


# --- [MODELS] ---------------------------------------------------------------------------


class Scattered(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Highlight count range with offsets drawn over the whole frame."""

    count: tuple[int, int]
    horizontal: tuple[float, float]
    vertical: tuple[float, float]


class Slotted(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Highlights at one drawn slot group, each slot pairing a position with the color at its index."""

    positions: tuple[tuple[float, float], ...]
    spreads: tuple[tuple[tuple[int, ...], ...], ...]
    jitter: float


class Draw(msgspec.Struct, frozen=True, kw_only=True):
    """Ranges one reference distribution draws its fields from."""

    colors: tuple[tuple[Finite, Finite, Finite], ...]
    placement: Scattered | Slotted
    sharpness: tuple[float, float]
    intensity: tuple[float, float]
    level: tuple[float, float]
    gradient: tuple[float, float]
    tints: tuple[tuple[Finite, Finite, Finite], ...]
    rim: Annotated[tuple[float, float], msgspec.Meta(description="Chance of a rim and its amount bound")]
    vignette: Annotated[tuple[float, float, float], msgspec.Meta(description="Chance of a vignette and its amount range")]
    cycles: tuple[float, ...] = (0.0,)
    dapples: tuple[int, ...] = (0,)


class Highlight(msgspec.Struct, frozen=True, kw_only=True):
    """Colored Gaussian glow."""

    position: tuple[Finite, Finite] = (0.5, 0.5)
    sharpness: NonNegative = 1.2
    intensity: NonNegative = 0.8
    color: tuple[Finite, Finite, Finite] = (1.0, 1.0, 1.0)


class Dapple(msgspec.Struct, frozen=True, kw_only=True):
    """Seeded random spots."""

    count: Annotated[int, msgspec.Meta(ge=0)] = 12
    sharpness: NonNegative = 8.0
    intensity: NonNegative = 0.3
    sharpness_spread: Annotated[NonNegative, msgspec.Meta(description="Variation above sharpness")] = 14.0
    intensity_spread: Annotated[NonNegative, msgspec.Meta(description="Variation above intensity")] = 0.5
    seed: Seed = 0


class Blinds(msgspec.Struct, frozen=True, kw_only=True):
    """Horizontal slat shadows."""

    cycles: NonNegative = 6.0
    shade: Unit = 0.45


class Rim(msgspec.Struct, frozen=True, kw_only=True):
    """Colored glow rising toward edges."""

    amount: NonNegative = 0.6
    color: tuple[Finite, Finite, Finite] = (1.0, 1.0, 1.0)
    radius: Annotated[float, msgspec.Meta(ge=0.0, le=math.sqrt(2.0))] = 0.5
    falloff: NonNegative = 1.4


class ReferenceImage(msgspec.Struct, frozen=True, kw_only=True):
    """Scene-linear lighting reference image."""

    tint: tuple[Finite, Finite, Finite] = (1.0, 1.0, 1.0)
    level: NonNegative = 0.08
    gradient: tuple[Finite, Finite] = (0.0, 0.0)
    highlights: tuple[Highlight, ...] = ()
    dapple: Dapple | None = None
    blinds: Blinds | None = None
    rim: Rim | None = None
    vignette: Unit = 0.0

    # --- [DISTRIBUTIONS]

    @classmethod
    def drawn(cls, distribution: ReferenceDistribution, seed: Seed) -> Self:
        """Draws editable reference from distribution and seed."""
        generator = rng(content_hash(ReferenceDistribution), seed, int(distribution))
        draw = distribution.draw

        def uniform(low: float, high: float) -> float:
            return float(generator.uniform(low, high))

        def chosen[T](items: tuple[T, ...]) -> T:
            return items[int(generator.integers(len(items)))]

        match draw.placement:
            case Scattered(count=count, horizontal=horizontal, vertical=vertical):
                slots: tuple[tuple[tuple[float, float], tuple[Finite, Finite, Finite]], ...] = ()
            case Slotted(positions=positions, spreads=spreads, jitter=jitter):
                slots = tuple((positions[index], draw.colors[index]) for index in chosen(chosen(spreads)))
                count, horizontal, vertical = (len(slots), len(slots)), (-jitter, jitter), (-jitter, jitter)
        highlights = tuple(
            Highlight(
                position=(((slots[index][0][0] if slots else 0.0) + uniform(*horizontal) + 1.0) / 2.0, ((slots[index][0][1] if slots else 0.0) + uniform(*vertical) + 1.0) / 2.0),
                sharpness=uniform(*draw.sharpness),
                intensity=uniform(*draw.intensity),
                color=slots[index][1] if slots else chosen(draw.colors),
            )
            for index in range(int(generator.integers(count[0], count[1] + 1)))
        )
        rim_on, vignette_on = float(generator.random()) < draw.rim[0], float(generator.random()) < draw.vignette[0]
        stripes, dapple_count = chosen(draw.cycles), chosen(draw.dapples)
        return cls(
            tint=chosen(draw.tints),
            level=uniform(*draw.level),
            gradient=(uniform(*draw.gradient), uniform(*draw.gradient)),
            highlights=highlights,
            rim=Rim(amount=uniform(0.0, draw.rim[1])) if rim_on else None,
            vignette=uniform(*draw.vignette[1:]) if vignette_on else 0.0,
            blinds=Blinds(cycles=stripes) if stripes > 0.0 else None,
            dapple=Dapple(count=dapple_count, seed=int(generator.integers(10000))) if dapple_count > 0 else None,
        )

    # --- [CATALOG]

    @staticmethod
    def gels() -> dict[str, tuple[Finite, Finite, Finite]]:
        """Returns named gel colors in scene-linear RGB."""
        return {
            "Warm": (1.0, 0.78, 0.5),
            "Deep Warm": (1.0, 0.58, 0.28),
            "Neutral": (1.0, 1.0, 1.0),
            "Cool": (0.6, 0.72, 1.0),
            "Sky": (0.5, 0.68, 1.0),
            "Magenta": (1.0, 0.3, 0.85),
            "Cyan": (0.2, 0.85, 0.95),
            "Pink": (1.0, 0.45, 0.7),
            "Blue": (0.3, 0.5, 1.0),
            "Green": (0.45, 0.95, 0.5),
            "Fire": (1.0, 0.5, 0.2),
            "Teal": (0.2, 0.72, 0.72),
        }

    @classmethod
    @functools.cache
    def catalog(cls) -> Mapping[Family, Mapping[str, Self]]:
        """Returns named reference recipes per family."""
        gels = cls.gels()
        warm, deep, neutral, cool = gels["Warm"], gels["Deep Warm"], gels["Neutral"], gels["Cool"]
        sky, magenta, cyan, pink = gels["Sky"], gels["Magenta"], gels["Cyan"], gels["Pink"]
        blue, green, fire, teal = gels["Blue"], gels["Green"], gels["Fire"], gels["Teal"]
        beauty = (1.0, 0.95, 0.9)
        portrait = (1.0, 0.96, 0.9)
        return {
            Family.SKY: {
                "Golden Hour": cls(level=0.2, gradient=(0.9, 0.1), tint=warm, highlights=(Highlight(position=(0.85, 0.65), intensity=0.7, color=warm),)),
                "Sunrise": cls(level=0.16, gradient=(0.5, 0.2), tint=(1.0, 0.7, 0.45), highlights=(Highlight(position=(0.2, 0.4), sharpness=1.4, color=(1.0, 0.65, 0.4)),)),
                "Blue Hour": cls(level=0.12, gradient=(0.0, 0.6), tint=cool),
                "Midday": cls(level=0.4, gradient=(0.0, 0.5), highlights=(Highlight(position=(0.55, 0.9), sharpness=1.4, intensity=0.6, color=neutral),)),
                "Harsh Noon": cls(level=0.25, highlights=(Highlight(position=(0.5, 0.925), sharpness=3.0, intensity=1.1, color=neutral),), vignette=0.45),
                "Overcast": cls(level=0.55, gradient=(0.0, 0.25), tint=(0.95, 0.97, 1.0)),
                "Foggy Morning": cls(level=0.5, gradient=(0.0, 0.3), tint=(0.9, 0.95, 1.0), vignette=0.2),
                "Night Sky": cls(level=0.04, gradient=(0.0, 0.25), tint=cool, highlights=(Highlight(position=(0.7, 0.9), sharpness=6.0, intensity=0.5, color=(0.8, 0.9, 1.0)),)),
                "Top Sky": cls(gradient=(0.0, 0.95), tint=(0.85, 0.92, 1.0)),
            },
            Family.STUDIO: {
                "Softbox": cls(level=0.5, highlights=(Highlight(position=(0.5, 0.7), sharpness=0.6, intensity=0.5, color=neutral),)),
                "Softbox Left": cls(level=0.18, highlights=(Highlight(position=(0.2, 0.675), sharpness=0.9, color=neutral),)),
                "Softbox Right": cls(level=0.18, highlights=(Highlight(position=(0.8, 0.675), sharpness=0.9, color=neutral),)),
                "Beauty Dish": cls(level=0.2, highlights=(Highlight(position=(0.5, 0.725), sharpness=1.6, intensity=0.9, color=neutral),), rim=Rim(amount=0.25)),
                "Ring Light": cls(level=0.18, highlights=(Highlight(sharpness=0.8, intensity=0.7, color=neutral),), rim=Rim(amount=0.7)),
                "Broad Light": cls(level=0.16, highlights=(Highlight(position=(0.275, 0.625), sharpness=1.0, intensity=0.85, color=portrait),)),
                "Short Light": cls(level=0.12, highlights=(Highlight(position=(0.775, 0.65), sharpness=1.3, intensity=0.85, color=portrait),), vignette=0.2),
            },
            Family.PORTRAIT: {
                "Rembrandt Right": cls(level=0.06, highlights=(Highlight(position=(0.75, 0.725), sharpness=1.6, intensity=1.1, color=warm),)),
                "Under Light": cls(level=0.06, highlights=(Highlight(position=(0.5, 0.15), sharpness=1.6, intensity=1.0, color=beauty),)),
                "Hair Light": cls(level=0.1, highlights=(Highlight(position=(0.5, 0.75), sharpness=1.0, intensity=0.5, color=neutral),), rim=Rim(amount=0.9)),
            },
            Family.CINEMATIC: {
                "Teal & Orange": cls(highlights=(Highlight(position=(0.8, 0.625), intensity=0.9, color=deep), Highlight(position=(0.2, 0.45), sharpness=1.0, intensity=0.6, color=teal))),
                "Neon M/C": cls(level=0.04, highlights=(Highlight(position=(0.225, 0.65), intensity=0.9, color=magenta), Highlight(position=(0.775, 0.45), intensity=0.9, color=cyan))),
                "Neon B/P": cls(level=0.04, highlights=(Highlight(position=(0.25, 0.5), intensity=0.9, color=blue), Highlight(position=(0.775, 0.65), intensity=0.9, color=pink))),
                "Firelight": cls(level=0.06, gradient=(0.0, -0.1), tint=fire, highlights=(Highlight(position=(0.5, 0.25), sharpness=1.0, intensity=0.9, color=fire),), vignette=0.35),
                "Moonlit Window": cls(level=0.04, highlights=(Highlight(position=(0.75, 0.6), sharpness=1.0, color=sky),), blinds=Blinds(cycles=5.0), vignette=0.35),
                "Horror Uplight": cls(level=0.04, highlights=(Highlight(position=(0.5, 0.15), sharpness=1.6, intensity=1.0, color=green),), vignette=0.45),
            },
            Family.ENVIRONMENT: {
                "Window Light": cls(level=0.12, highlights=(Highlight(position=(0.775, 0.675), sharpness=0.8, intensity=0.85, color=(1.0, 0.98, 0.92)),)),
                "Window Blinds": cls(level=0.1, highlights=(Highlight(position=(0.75, 0.65), sharpness=0.8, intensity=0.9, color=warm),), blinds=Blinds(cycles=7.0)),
                "Forest Dapple": cls(level=0.1, gradient=(0.0, 0.3), tint=green, dapple=Dapple(count=14, seed=7)),
                "Underwater": cls(level=0.12, gradient=(0.0, 0.4), tint=teal, dapple=Dapple(seed=3)),
                "Snow Bounce": cls(level=0.6, gradient=(0.0, 0.2), tint=(0.95, 0.98, 1.0), highlights=(Highlight(position=(0.5, 0.25), sharpness=0.8, intensity=0.3, color=neutral),)),
                "Desert": cls(level=0.35, gradient=(0.2, 0.4), tint=(1.0, 0.85, 0.6), highlights=(Highlight(position=(0.65, 0.85), sharpness=1.4, intensity=0.6, color=warm),)),
                "Street Lamp": cls(level=0.03, highlights=(Highlight(position=(0.5, 0.7), sharpness=2.0, intensity=1.0, color=(1.0, 0.8, 0.5)),), vignette=0.6),
                "Stage Spot": cls(level=0.02, highlights=(Highlight(position=(0.5, 0.8), sharpness=2.6, intensity=1.2, color=neutral),), vignette=0.6),
                "Concert RGB": cls(
                    level=0.04,
                    highlights=(
                        Highlight(position=(0.2, 0.7), sharpness=1.4, color=blue),
                        Highlight(position=(0.5, 0.75), sharpness=1.4, color=magenta),
                        Highlight(position=(0.8, 0.7), sharpness=1.4, color=cyan),
                    ),
                ),
                "Sci-Fi Panel": cls(level=0.06, highlights=(Highlight(position=(0.75, 0.5), sharpness=1.0, intensity=0.7, color=cyan),), blinds=Blinds(cycles=4.0), vignette=0.3),
            },
        }

    # --- [DRAWING]

    def plane(self, size: Side) -> Outcome[npt.NDArray[np.float32]]:
        """Returns unclipped float32 RGB of square image."""

        @np.errstate(over="raise", divide="raise", invalid="raise")
        def formed() -> npt.NDArray[np.float32]:
            def glow(x: float, y: float, sharpness: float) -> npt.NDArray[np.float64]:
                if sharpness == 0.0:
                    return np.ones_like(xs)
                with np.errstate(over="ignore", under="ignore"):
                    distance = np.hypot(across - x, along - y)
                    return np.exp(-((distance * math.sqrt(sharpness)) ** 2) * 4.0)

            centers = (np.arange(size, dtype=np.float64) + 0.5 - size / 2.0) / (size / 2.0)
            xs, ys = np.meshgrid(centers, -centers)
            across, along = (xs + 1.0) / 2.0, (ys + 1.0) / 2.0
            reach, tint = np.hypot(xs, ys), np.array(self.tint)
            floor = self.level * tint + np.maximum(self.gradient[0] * across + self.gradient[1] * along, 0.0)[..., None] * tint
            lit = sum((spot.intensity * glow(*spot.position, spot.sharpness)[..., None] * np.array(spot.color) for spot in self.highlights), floor)
            speckled = sum(
                (
                    dapple.intensity * envelope * tint + float(intensity) * dapple.intensity_spread * envelope * tint
                    for dapple in Option.of_optional(self.dapple).to_list()
                    for x, y, sharpness, intensity in rng(content_hash(Dapple), dapple.seed).random((dapple.count, 4))
                    for envelope in ((glow(x, y, dapple.sharpness) * glow(x, y, float(sharpness * dapple.sharpness_spread)))[..., None],)
                ),
                lit,
            )
            rows = size - 2.0 * np.arange(size) - 1.0
            shaded = (
                Option
                .of_optional(self.blinds)
                .map(lambda blinds: (blinds, np.remainder((np.remainder(blinds.cycles, 2.0 * size) / size) * rows, 2.0)))
                .map(lambda held: speckled * (held[0].shade + (1.0 - held[0].shade) * ((held[1] > 0.0) & (held[1] < 1.0)))[:, None, None])
                .default_value(speckled)
            )
            rimmed = (
                Option
                .of_optional(self.rim)
                .filter(lambda rim: rim.amount != 0.0)
                .map(lambda rim: shaded + rim.amount * np.maximum(reach - rim.radius, 0.0)[..., None] ** rim.falloff * np.array(rim.color))
                .default_value(shaded)
            )
            return np.ascontiguousarray(rimmed * (1.0 - self.vignette * np.minimum(reach**2, 1.0))[..., None], dtype=np.float32)

        return catch(formed, exception=FloatingPointError)().map_error(lambda error: (Fault(reason=RecordReason.NUMERIC, subject=repr(self), cause=error),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Blinds", "Dapple", "Family", "Highlight", "ReferenceDistribution", "ReferenceImage", "Rim"]
