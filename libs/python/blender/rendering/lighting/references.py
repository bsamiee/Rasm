"""Scene-linear lighting reference images."""

from enum import Enum, IntEnum
from functools import partial
import math
from types import MappingProxyType
from typing import Annotated, Final, Self

from expression import Option
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt

from blender.records.faults import Fault, Outcome, RecordReason
from blender.records.fields import Finite, NonNegative, Seed, SEED_BOUND, Unit
from blender.records.identity import content_hash, rng

# --- [TYPES] ----------------------------------------------------------------------------

type _Rgb = tuple[Finite, Finite, Finite]


class ReferenceFamily(IntEnum):
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


class Gel(Enum):
    """Lighting gel, valued by its scene-linear RGB tint."""

    WARM = (1.0, 0.78, 0.5)
    DEEP_WARM = (1.0, 0.58, 0.28)
    NEUTRAL = (1.0, 1.0, 1.0)
    COOL = (0.6, 0.72, 1.0)
    SKY = (0.5, 0.68, 1.0)
    MAGENTA = (1.0, 0.3, 0.85)
    CYAN = (0.2, 0.85, 0.95)
    PINK = (1.0, 0.45, 0.7)
    BLUE = (0.3, 0.5, 1.0)
    GREEN = (0.45, 0.95, 0.5)
    FIRE = (1.0, 0.5, 0.2)
    TEAL = (0.2, 0.72, 0.72)


# --- [MODELS] ---------------------------------------------------------------------------


class Highlight(msgspec.Struct, frozen=True, kw_only=True):
    """Colored Gaussian glow."""

    position: tuple[Finite, Finite] = (0.5, 0.5)
    sharpness: NonNegative = 1.2
    intensity: NonNegative = 0.8
    color: _Rgb = (1.0, 1.0, 1.0)


class Dapple(msgspec.Struct, frozen=True, kw_only=True):
    """Seeded random spots, each spread the variation above its base."""

    count: Annotated[int, msgspec.Meta(ge=0)] = 12
    sharpness: NonNegative = 8.0
    intensity: NonNegative = 0.3
    sharpness_spread: NonNegative = 14.0
    intensity_spread: NonNegative = 0.5
    seed: Seed = 0


class Blinds(msgspec.Struct, frozen=True, kw_only=True):
    """Horizontal slat shadows."""

    cycles: NonNegative = 6.0
    shade: Unit = 0.45


class Rim(msgspec.Struct, frozen=True, kw_only=True):
    """Colored glow rising toward edges."""

    amount: NonNegative = 0.6
    color: _Rgb = (1.0, 1.0, 1.0)
    radius: Annotated[float, msgspec.Meta(ge=0.0, le=math.sqrt(2.0))] = 0.5
    falloff: NonNegative = 1.4


class ReferenceImage(msgspec.Struct, frozen=True, kw_only=True):
    """Scene-linear lighting reference image."""

    tint: _Rgb = (1.0, 1.0, 1.0)
    level: NonNegative = 0.08
    gradient: tuple[Finite, Finite] = (0.0, 0.0)
    highlights: tuple[Highlight, ...] = ()
    dapple: Dapple | None = None
    blinds: Blinds | None = None
    rim: Rim | None = None
    vignette: Unit = 0.0

    # --- [DISTRIBUTIONS]

    @classmethod
    def drawn(cls, distribution: ReferenceDistribution, seed: int) -> Self:
        """Draws editable reference from distribution and seed."""
        generator = rng(content_hash(ReferenceDistribution), seed, int(distribution))
        draw = _DRAWS[distribution]

        def uniform(low: float, high: float) -> float:
            return float(generator.uniform(low, high))

        def chosen[T](items: tuple[T, ...]) -> T:
            return items[int(generator.integers(len(items)))]

        highlights = tuple(
            Highlight(position=position, sharpness=uniform(*draw.sharpness), intensity=uniform(*draw.intensity), color=color) for position, color in draw.placement.slots(generator, draw.colors)
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
            dapple=Dapple(count=dapple_count, seed=int(generator.integers(SEED_BOUND + 1))) if dapple_count > 0 else None,
        )

    # --- [DRAWING]

    def plane(self, size: int) -> Outcome[npt.NDArray[np.float32]]:
        """Returns unclipped float32 RGB of square image."""

        @catch(exception=FloatingPointError)
        @np.errstate(over="raise", divide="raise", invalid="raise")
        def formed() -> npt.NDArray[np.float32]:
            @np.errstate(over="ignore", under="ignore")
            def glow(x: float, y: float, sharpness: float) -> npt.NDArray[np.float64]:
                if sharpness == 0.0:
                    return np.ones_like(xs)
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

        return formed().map_error(partial(Fault.of, RecordReason.NUMERIC, repr(self)))


class _Scattered(msgspec.Struct, frozen=True, kw_only=True):
    """Highlight count range with offsets drawn over the whole frame."""

    count: tuple[int, int]
    horizontal: tuple[float, float]
    vertical: tuple[float, float]

    def slots(self, generator: np.random.Generator, colors: tuple[_Rgb, ...]) -> tuple[tuple[tuple[float, float], _Rgb], ...]:
        """Returns a drawn count of frame positions over the whole frame, each with a drawn color."""
        return tuple(
            (((float(generator.uniform(*self.horizontal)) + 1.0) / 2.0, (float(generator.uniform(*self.vertical)) + 1.0) / 2.0), colors[int(generator.integers(len(colors)))])
            for _ in range(int(generator.integers(self.count[0], self.count[1] + 1)))
        )


class _Slotted(msgspec.Struct, frozen=True, kw_only=True):
    """Highlights at one drawn slot group, each slot pairing a position with the color at its index."""

    positions: tuple[tuple[float, float], ...]
    spreads: tuple[tuple[tuple[int, ...], ...], ...]
    jitter: float

    def slots(self, generator: np.random.Generator, colors: tuple[_Rgb, ...]) -> tuple[tuple[tuple[float, float], _Rgb], ...]:
        """Returns the jittered positions of one drawn slot group, each with the color at its slot index."""
        spread = self.spreads[int(generator.integers(len(self.spreads)))]
        return tuple(
            (((x + float(generator.uniform(-self.jitter, self.jitter)) + 1.0) / 2.0, (y + float(generator.uniform(-self.jitter, self.jitter)) + 1.0) / 2.0), colors[index])
            for index in spread[int(generator.integers(len(spread)))]
            for x, y in (self.positions[index],)
        )


class _Draw(msgspec.Struct, frozen=True, kw_only=True):
    """Ranges one reference distribution draws its fields from, rim and vignette leading with their chance."""

    colors: tuple[_Rgb, ...]
    placement: _Scattered | _Slotted
    sharpness: tuple[float, float]
    intensity: tuple[float, float]
    level: tuple[float, float]
    gradient: tuple[float, float]
    tints: tuple[_Rgb, ...]
    rim: tuple[float, float]
    vignette: tuple[float, float, float]
    cycles: tuple[float, ...] = (0.0,)
    dapples: tuple[int, ...] = (0,)


# --- [TABLES] ---------------------------------------------------------------------------

_DRAWS: Final = MappingProxyType({
    ReferenceDistribution.BALANCED: _Draw(
        colors=tuple(gel.value for gel in Gel),
        placement=_Scattered(count=(1, 3), horizontal=(-0.8, 0.8), vertical=(-0.7, 0.85)),
        sharpness=(0.7, 2.4),
        intensity=(0.5, 1.1),
        level=(0.03, 0.5),
        gradient=(0.0, 0.6),
        tints=tuple(gel.value for gel in Gel),
        rim=(0.4, 1.2),
        vignette=(0.5, 0.0, 0.5),
        cycles=(0.0, 0.0, 0.0, 5.0, 7.0),
        dapples=(0, 0, 0, 12),
    ),
    ReferenceDistribution.VIVID: _Draw(
        colors=((1.4, 0.12, 0.85), (0.15, 0.75, 1.35), (1.35, 0.55, 0.05), (0.25, 1.05, 0.35), (0.75, 0.25, 1.35)),
        placement=_Slotted(
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
    ),
    ReferenceDistribution.SOFT: _Draw(
        colors=((1.0, 0.96, 0.92), (0.94, 0.96, 1.0), (0.98, 0.95, 0.9)),
        placement=_Scattered(count=(1, 2), horizontal=(-0.45, 0.45), vertical=(-0.35, 0.55)),
        sharpness=(0.55, 1.0),
        intensity=(0.35, 0.65),
        level=(0.32, 0.55),
        gradient=(0.0, 0.25),
        tints=((0.97, 0.95, 0.92), (0.93, 0.95, 0.98)),
        rim=(0.0, 0.0),
        vignette=(0.3, 0.0, 0.2),
    ),
})
REFERENCES: Final = MappingProxyType({
    ReferenceFamily.SKY: MappingProxyType({
        "Golden Hour": ReferenceImage(level=0.2, gradient=(0.9, 0.1), tint=Gel.WARM.value, highlights=(Highlight(position=(0.85, 0.65), intensity=0.7, color=Gel.WARM.value),)),
        "Sunrise": ReferenceImage(level=0.16, gradient=(0.5, 0.2), tint=(1.0, 0.7, 0.45), highlights=(Highlight(position=(0.2, 0.4), sharpness=1.4, color=(1.0, 0.65, 0.4)),)),
        "Blue Hour": ReferenceImage(level=0.12, gradient=(0.0, 0.6), tint=Gel.COOL.value),
        "Midday": ReferenceImage(level=0.4, gradient=(0.0, 0.5), highlights=(Highlight(position=(0.55, 0.9), sharpness=1.4, intensity=0.6, color=Gel.NEUTRAL.value),)),
        "Harsh Noon": ReferenceImage(level=0.25, highlights=(Highlight(position=(0.5, 0.925), sharpness=3.0, intensity=1.1, color=Gel.NEUTRAL.value),), vignette=0.45),
        "Overcast": ReferenceImage(level=0.55, gradient=(0.0, 0.25), tint=(0.95, 0.97, 1.0)),
        "Foggy Morning": ReferenceImage(level=0.5, gradient=(0.0, 0.3), tint=(0.9, 0.95, 1.0), vignette=0.2),
        "Night Sky": ReferenceImage(level=0.04, gradient=(0.0, 0.25), tint=Gel.COOL.value, highlights=(Highlight(position=(0.7, 0.9), sharpness=6.0, intensity=0.5, color=(0.8, 0.9, 1.0)),)),
        "Top Sky": ReferenceImage(gradient=(0.0, 0.95), tint=(0.85, 0.92, 1.0)),
    }),
    ReferenceFamily.STUDIO: MappingProxyType({
        "Softbox": ReferenceImage(level=0.5, highlights=(Highlight(position=(0.5, 0.7), sharpness=0.6, intensity=0.5, color=Gel.NEUTRAL.value),)),
        "Softbox Left": ReferenceImage(level=0.18, highlights=(Highlight(position=(0.2, 0.675), sharpness=0.9, color=Gel.NEUTRAL.value),)),
        "Softbox Right": ReferenceImage(level=0.18, highlights=(Highlight(position=(0.8, 0.675), sharpness=0.9, color=Gel.NEUTRAL.value),)),
        "Beauty Dish": ReferenceImage(level=0.2, highlights=(Highlight(position=(0.5, 0.725), sharpness=1.6, intensity=0.9, color=Gel.NEUTRAL.value),), rim=Rim(amount=0.25)),
        "Ring Light": ReferenceImage(level=0.18, highlights=(Highlight(sharpness=0.8, intensity=0.7, color=Gel.NEUTRAL.value),), rim=Rim(amount=0.7)),
        "Broad Light": ReferenceImage(level=0.16, highlights=(Highlight(position=(0.275, 0.625), sharpness=1.0, intensity=0.85, color=(1.0, 0.96, 0.9)),)),
        "Short Light": ReferenceImage(level=0.12, highlights=(Highlight(position=(0.775, 0.65), sharpness=1.3, intensity=0.85, color=(1.0, 0.96, 0.9)),), vignette=0.2),
    }),
    ReferenceFamily.PORTRAIT: MappingProxyType({
        "Rembrandt Right": ReferenceImage(level=0.06, highlights=(Highlight(position=(0.75, 0.725), sharpness=1.6, intensity=1.1, color=Gel.WARM.value),)),
        "Under Light": ReferenceImage(level=0.06, highlights=(Highlight(position=(0.5, 0.15), sharpness=1.6, intensity=1.0, color=(1.0, 0.95, 0.9)),)),
        "Hair Light": ReferenceImage(level=0.1, highlights=(Highlight(position=(0.5, 0.75), sharpness=1.0, intensity=0.5, color=Gel.NEUTRAL.value),), rim=Rim(amount=0.9)),
    }),
    ReferenceFamily.CINEMATIC: MappingProxyType({
        "Teal & Orange": ReferenceImage(
            highlights=(Highlight(position=(0.8, 0.625), intensity=0.9, color=Gel.DEEP_WARM.value), Highlight(position=(0.2, 0.45), sharpness=1.0, intensity=0.6, color=Gel.TEAL.value))
        ),
        "Neon M/C": ReferenceImage(
            level=0.04, highlights=(Highlight(position=(0.225, 0.65), intensity=0.9, color=Gel.MAGENTA.value), Highlight(position=(0.775, 0.45), intensity=0.9, color=Gel.CYAN.value))
        ),
        "Neon B/P": ReferenceImage(
            level=0.04, highlights=(Highlight(position=(0.25, 0.5), intensity=0.9, color=Gel.BLUE.value), Highlight(position=(0.775, 0.65), intensity=0.9, color=Gel.PINK.value))
        ),
        "Firelight": ReferenceImage(
            level=0.06, gradient=(0.0, -0.1), tint=Gel.FIRE.value, highlights=(Highlight(position=(0.5, 0.25), sharpness=1.0, intensity=0.9, color=Gel.FIRE.value),), vignette=0.35
        ),
        "Moonlit Window": ReferenceImage(level=0.04, highlights=(Highlight(position=(0.75, 0.6), sharpness=1.0, color=Gel.SKY.value),), blinds=Blinds(cycles=5.0), vignette=0.35),
        "Horror Uplight": ReferenceImage(level=0.04, highlights=(Highlight(position=(0.5, 0.15), sharpness=1.6, intensity=1.0, color=Gel.GREEN.value),), vignette=0.45),
    }),
    ReferenceFamily.ENVIRONMENT: MappingProxyType({
        "Window Light": ReferenceImage(level=0.12, highlights=(Highlight(position=(0.775, 0.675), sharpness=0.8, intensity=0.85, color=(1.0, 0.98, 0.92)),)),
        "Window Blinds": ReferenceImage(level=0.1, highlights=(Highlight(position=(0.75, 0.65), sharpness=0.8, intensity=0.9, color=Gel.WARM.value),), blinds=Blinds(cycles=7.0)),
        "Forest Dapple": ReferenceImage(level=0.1, gradient=(0.0, 0.3), tint=Gel.GREEN.value, dapple=Dapple(count=14, seed=7)),
        "Underwater": ReferenceImage(level=0.12, gradient=(0.0, 0.4), tint=Gel.TEAL.value, dapple=Dapple(seed=3)),
        "Snow Bounce": ReferenceImage(level=0.6, gradient=(0.0, 0.2), tint=(0.95, 0.98, 1.0), highlights=(Highlight(position=(0.5, 0.25), sharpness=0.8, intensity=0.3, color=Gel.NEUTRAL.value),)),
        "Desert": ReferenceImage(level=0.35, gradient=(0.2, 0.4), tint=(1.0, 0.85, 0.6), highlights=(Highlight(position=(0.65, 0.85), sharpness=1.4, intensity=0.6, color=Gel.WARM.value),)),
        "Street Lamp": ReferenceImage(level=0.03, highlights=(Highlight(position=(0.5, 0.7), sharpness=2.0, intensity=1.0, color=(1.0, 0.8, 0.5)),), vignette=0.6),
        "Stage Spot": ReferenceImage(level=0.02, highlights=(Highlight(position=(0.5, 0.8), sharpness=2.6, intensity=1.2, color=Gel.NEUTRAL.value),), vignette=0.6),
        "Concert RGB": ReferenceImage(
            level=0.04,
            highlights=(
                Highlight(position=(0.2, 0.7), sharpness=1.4, color=Gel.BLUE.value),
                Highlight(position=(0.5, 0.75), sharpness=1.4, color=Gel.MAGENTA.value),
                Highlight(position=(0.8, 0.7), sharpness=1.4, color=Gel.CYAN.value),
            ),
        ),
        "Sci-Fi Panel": ReferenceImage(level=0.06, highlights=(Highlight(position=(0.75, 0.5), sharpness=1.0, intensity=0.7, color=Gel.CYAN.value),), blinds=Blinds(cycles=4.0), vignette=0.3),
    }),
})
RECIPES: Final = MappingProxyType({name: recipe for recipes in REFERENCES.values() for name, recipe in recipes.items()})

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["RECIPES", "REFERENCES", "Blinds", "Dapple", "Gel", "Highlight", "ReferenceDistribution", "ReferenceFamily", "ReferenceImage", "Rim"]
