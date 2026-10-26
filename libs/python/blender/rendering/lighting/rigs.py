"""Light rigs placed about target by delivered irradiance."""

from collections.abc import Mapping
from enum import IntEnum
import functools
import math
import sys
from typing import Annotated

from colour.models.rgb.datasets.srgb import MATRIX_sRGB_TO_XYZ
from expression import Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.imaging.analysis.lighting import LightingReading
from blender.imaging.analysis.metering import Samples
from blender.imaging.color.processors import processor, Stops
from blender.imaging.files.buffers import applied
from blender.records.faults import accumulated, Fault, Outcome, paired, RecordReason
from blender.records.fields import Case, Finite, NonNegative, Positive, Unit

# --- [TYPES] ----------------------------------------------------------------------------

type Emitter = Point | Spot | Disk | Sun
type Layout = Arranged | Matched
type Distance = Annotated[float, msgspec.Meta(gt=1.0, le=sys.float_info.max)]
type Diameter = Annotated[float, msgspec.Meta(ge=0.0, le=math.pi)]


class Role(IntEnum):
    """Light role in rig."""

    KEY = 1
    FILL = 2
    RIM = 3
    SKY = 4
    ACCENT = 5


# --- [MODELS] ---------------------------------------------------------------------------


class Point(Case, frozen=True):
    """Sphere light emitting in every direction."""


class Spot(Case, frozen=True, kw_only=True):
    """Sphere light with beam limited to cone."""

    cone: Annotated[float, msgspec.Meta(ge=math.radians(1.0), le=math.pi, description="Beam angular diameter")] = math.pi / 4.0
    blend: Annotated[Unit, msgspec.Meta(description="Share of beam softened at edge")] = 0.15


class Disk(Case, frozen=True):
    """Disk area light facing target."""


class Sun(Case, frozen=True):
    """Distant light at uniform irradiance."""


class Light(msgspec.Struct, frozen=True, kw_only=True):
    """Rig light with irradiance offset from key."""

    role: Role = Role.KEY
    emitter: Emitter = Disk()
    azimuth: Annotated[float, msgspec.Meta(ge=-math.pi, le=math.pi)] = 0.0
    elevation: Annotated[float, msgspec.Meta(ge=-math.pi / 2.0, le=math.pi / 2.0)] = 0.0
    stops: Annotated[Finite, msgspec.Meta(description="Target irradiance in stops from key")] = 0.0
    size: Annotated[Diameter, msgspec.Meta(description="Angular diameter at target center")] = 2.0 * math.asin(0.2)
    color: tuple[Finite, Finite, Finite] = (1.0, 1.0, 1.0)
    temperature: Annotated[float, msgspec.Meta(ge=1800.0, le=20000.0, description="Blackbody color in place of the color while present")] | None = None

    @property
    def direction(self) -> tuple[float, float, float]:
        """Unit vector toward light over right, up, and back."""
        horizontal = math.cos(self.elevation)
        return horizontal * math.sin(self.azimuth), math.sin(self.elevation), horizontal * math.cos(self.azimuth)

    @property
    def extent(self) -> float:
        """Sine of half angle light subtends at target center."""
        return math.sin(self.size / 2.0)


class Ambient(msgspec.Struct, frozen=True, kw_only=True):
    """World light color and irradiance offset."""

    color: tuple[Finite, Finite, Finite] = (1.0, 1.0, 1.0)
    stops: Finite = 0.0


class Arranged(Case, frozen=True, kw_only=True):
    """Lights and world light as stated."""

    lights: tuple[Light, ...] = (Light(),)
    ambient: Ambient | None = None


class Matched(Case, frozen=True, kw_only=True):
    """Lights derived from reference reading."""

    count: Annotated[int, msgspec.Meta(ge=1, le=8)] = 3
    key: Emitter = Disk()
    softness: NonNegative = 1.0
    fill_size: Diameter = math.radians(44.0)
    fill: Annotated[Stops | None, msgspec.Meta(description="Fill stops from key, the reading contrast while absent")] = None
    rim: Annotated[Stops | None, msgspec.Meta(description="Rim stops over the reading backlight, no rim while absent")] = 0.0
    ambient: Annotated[Stops | None, msgspec.Meta(description="World stops over the reading ambient, no world while absent")] = 0.0
    tint: tuple[Finite, Finite, Finite] = (1.0, 1.0, 1.0)

    def arranged(self, reading: LightingReading) -> Outcome[Arranged]:
        """Derives key, fill, rim, accent, and world lights up to count."""

        def peak(color: npt.NDArray[np.float64]) -> float:
            return max(float(color.max()), Samples.floor)

        def filtered(color: npt.NDArray[np.float64]) -> tuple[float, float, float]:
            red, green, blue = (np.multiply(self.tint, color) / peak(color)).tolist()
            return red, green, blue

        @catch(exception=FloatingPointError)
        @np.errstate(over="raise", divide="raise", invalid="raise")
        def lit(hardness: float) -> Arranged:
            azimuth, elevation = (component * math.pi / 2.0 for component in reading.direction)
            scale = max(hardness, abs(reading.contrast))
            size = math.pi - 2.0 * math.atan2(hardness / scale, self.softness * (reading.contrast / scale))
            key = Light(emitter=self.key, azimuth=azimuth, elevation=elevation, size=size, color=filtered(reading.key))
            fill = Light(role=Role.FILL, azimuth=-azimuth, stops=Option.of_optional(self.fill).default_value(-reading.contrast), size=self.fill_size, color=filtered(reading.fill))
            rim = Option.of_optional(self.rim).bind(
                lambda offset: reading.rim.bind(
                    lambda color: reading.backlight.map(
                        lambda backlight: Light(role=Role.RIM, azimuth=math.remainder(azimuth + math.pi, math.tau), elevation=elevation, stops=backlight + offset, size=size, color=filtered(color))
                    )
                )
            )
            accents = (
                Light(
                    role=Role.ACCENT,
                    azimuth=(2.0 * swatch.position[0] - 1.0) * math.pi / 2.0,
                    elevation=(2.0 * swatch.position[1] - 1.0) * math.pi / 2.0,
                    stops=math.log2(peak(swatch.color)) - math.log2(peak(reading.key)),
                    size=size,
                    color=filtered(swatch.color),
                )
                for swatch in reading.palette[1:]
            )
            return Arranged(
                lights=(key, fill, *rim.to_list(), *accents)[: self.count],
                ambient=Option
                .of_optional(self.ambient)
                .map(lambda offset: Ambient(color=filtered(reading.ambient), stops=math.log2(peak(reading.ambient)) - math.log2(peak(reading.key)) + offset))
                .to_optional(),
            )

        return (
            lit(reading.hardness).map_error(lambda error: (Fault(reason=RecordReason.NUMERIC, subject=repr(reading), cause=error),))
            if reading.hardness > 0.0
            else Ok(Arranged(lights=(), ambient=Ambient(color=filtered(reading.key))))
        )


class Placement(msgspec.Struct, frozen=True, kw_only=True):
    """Light at target distance with delivered irradiance."""

    light: Light
    reach: Positive
    irradiance: NonNegative

    @property
    def location(self) -> tuple[float, float, float]:
        """Position in meters over target right, up, and back."""
        right, up, back = self.light.direction
        return self.reach * right, self.reach * up, self.reach * back

    @property
    def radius(self) -> float:
        """Light sphere radius in meters."""
        return self.reach * self.light.extent

    @property
    def power(self) -> Outcome[float]:
        """Radiant watts, or watts per square meter for sun."""
        match self.light.emitter:
            case Point() | Spot():
                coefficient = 4.0 * math.pi
            case Disk():
                coefficient = math.pi
            case Sun():
                return Ok(self.irradiance)
        return _scaled((coefficient, self.reach, self.reach, 1.0 + self.light.extent**2, self.irradiance))


class Background(msgspec.Struct, frozen=True, kw_only=True):
    """Uniform world radiance."""

    color: tuple[Finite, Finite, Finite]
    strength: NonNegative


class Placements(msgspec.Struct, frozen=True, kw_only=True):
    """Lights placed about target sphere with world."""

    radius: Positive
    distance: Distance
    lights: tuple[Placement, ...]
    background: Option[Background]


class Rig(msgspec.Struct, frozen=True, kw_only=True):
    """Lights about target at key irradiance."""

    irradiance: Annotated[NonNegative, msgspec.Meta(description="Key irradiance")] = math.pi
    distance: Distance = 2.5
    color_strength: Annotated[NonNegative, msgspec.Meta(description="Blend from white up to one, Rec.709 chroma boost above one")] = 0.85
    color_saturation: Annotated[NonNegative, msgspec.Meta(description="Rec.709 saturation after color strength")] = 1.0
    layout: Layout = Matched()

    def arranged(self, reading: Option[LightingReading]) -> Outcome[Option[Arranged]]:
        """Returns stated layout, or matched layout when reading exists."""
        match self.layout:
            case Arranged() as arranged:
                return Ok(Some(arranged))
            case Matched() as matched:
                return reading.map(lambda found: matched.arranged(found).map(Some)).default_value(Ok(Nothing))

    def placements(self, config: PyOpenColorIO.Config, radius: Positive, reading: Option[LightingReading]) -> Outcome[Option[Placements]]:
        """Places rig with graded colors, Nothing for matched layout without reading."""
        blend = min(self.color_strength, 1.0)
        luma = MATRIX_sRGB_TO_XYZ[1].tolist()
        transform = PyOpenColorIO.GroupTransform([
            PyOpenColorIO.MatrixTransform.Fit(newMin=[1.0 - blend] * 3 + [0.0]),
            PyOpenColorIO.MatrixTransform.Sat(max(self.color_strength, 1.0), luma),
            PyOpenColorIO.MatrixTransform.Sat(self.color_saturation, luma),
            PyOpenColorIO.RangeTransform(minInValue=0.0, minOutValue=0.0),
        ])

        def placed(arranged: Arranged, built: PyOpenColorIO.Processor, reach: float) -> Outcome[Option[Placements]]:
            operation = built.getDefaultCPUProcessor()

            def graded(color: tuple[Finite, Finite, Finite]) -> Outcome[tuple[Finite, Finite, Finite]]:
                return catch(
                    lambda: msgspec.convert(applied(operation, np.asarray([color], dtype=np.float32))[0].tolist(), type=tuple[Finite, Finite, Finite]), exception=msgspec.ValidationError
                )().map_error(lambda error: (Fault(reason=RecordReason.NUMERIC, subject=repr(color), cause=error),))

            def light_at(light: Light) -> Outcome[Placement]:
                return paired(graded(light.color), _scaled((self.irradiance,), light.stops)).map(
                    lambda found: Placement(light=msgspec.structs.replace(light, color=found[0]), reach=reach, irradiance=found[1])
                )

            def world(ambient: Ambient) -> Outcome[Option[Background]]:
                return paired(graded(ambient.color), _scaled((self.irradiance, 1.0 / math.pi), ambient.stops)).map(lambda found: Some(Background(color=found[0], strength=found[1])))

            return paired(accumulated(light_at(light) for light in arranged.lights), Option.of_optional(arranged.ambient).map(world).default_value(Ok(Nothing))).map(
                lambda found: Some(Placements(radius=radius, distance=self.distance, lights=found[0], background=found[1]))
            )

        return self.arranged(reading).bind(
            lambda arranged: arranged.map(lambda layout: paired(processor(config, transform), _scaled((self.distance, radius))).bind(lambda found: placed(layout, *found))).default_value(Ok(Nothing))
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


@functools.cache
def rig_catalog() -> Mapping[str, Rig]:
    """Returns named rig recipes."""

    def arranged(rows: tuple[tuple[Role, tuple[float, float, float], float, float, float], ...]) -> Rig:
        levels = tuple(power / (math.pi * (math.hypot(*offset) ** 2 + (side / 2.0) ** 2)) for _, offset, power, side, _ in rows)
        return Rig(
            irradiance=levels[0],
            layout=Arranged(
                lights=tuple(
                    Light(
                        role=role,
                        azimuth=math.atan2(offset[0], -offset[1]),
                        elevation=math.atan2(offset[2], math.hypot(*offset[:2])),
                        size=2.0 * math.asin(side / (2.0 * math.hypot(*offset))),
                        stops=math.log2(level / levels[0]),
                        temperature=temperature,
                    )
                    for (role, offset, _, side, temperature), level in zip(rows, levels)
                )
            ),
        )

    return {
        "Three-Point": arranged(((Role.KEY, (1.6, -1.6, 0.30), 250.0, 0.6, 5500.0), (Role.FILL, (-1.6, -1.2, -0.10), 90.0, 1.5, 6500.0), (Role.RIM, (0.5, 1.6, 0.80), 400.0, 0.4, 5000.0))),
        "Hard Low Key": arranged(((Role.KEY, (2.0, -1.0, 0.40), 450.0, 0.4, 4500.0), (Role.FILL, (-1.5, -1.0, -0.20), 25.0, 1.0, 7000.0), (Role.RIM, (-0.4, 1.5, 0.80), 200.0, 0.3, 4200.0))),
        "Side Key": arranged(((Role.KEY, (2.4, -0.6, 1.00), 550.0, 0.3, 4200.0),)),
        "Soft High Key": arranged(((Role.KEY, (1.2, -2.0, 0.50), 300.0, 2.0, 5500.0), (Role.FILL, (-1.2, -2.0, 0.50), 250.0, 2.0, 5500.0), (Role.SKY, (0.0, 0.0, 2.20), 180.0, 1.5, 5500.0))),
        "Rim Only": arranged(((Role.RIM, (0.0, 1.5, 1.20), 600.0, 0.5, 5000.0),)),
        "Glamour": arranged(((Role.KEY, (0.0, -1.6, 1.40), 350.0, 0.7, 5500.0), (Role.FILL, (0.0, -1.0, -0.40), 80.0, 1.5, 6000.0))),
        "Auto": Rig(),
        "Portrait": Rig(distance=2.4, layout=Matched(softness=1.1, tint=(1.0, 0.97, 0.92))),
        "Rembrandt": Rig(color_strength=0.9, distance=2.4, layout=Matched(key=Spot(), softness=0.9, rim=math.log2(0.8), fill=math.log2(0.12), tint=(1.0, 0.95, 0.85))),
        "Butterfly": Rig(color_strength=0.8, distance=2.3, layout=Matched(softness=1.3, rim=math.log2(0.6), fill=math.log2(0.45), tint=(1.0, 0.97, 0.95))),
        "Loop": Rig(distance=2.4, layout=Matched(softness=1.1, rim=math.log2(0.7), fill=math.log2(0.3), tint=(1.0, 0.96, 0.9))),
        "Split": Rig(color_strength=0.9, distance=2.4, layout=Matched(softness=0.8, rim=math.log2(0.5), fill=math.log2(0.08), tint=(1.0, 0.98, 0.95))),
        "Clamshell": Rig(color_strength=0.7, distance=2.2, layout=Matched(softness=2.2, rim=None, fill=math.log2(0.85), tint=(1.0, 0.97, 0.96))),
        "Beauty": Rig(color_strength=0.75, distance=2.2, layout=Matched(softness=2.4, rim=math.log2(0.5), fill=math.log2(0.8), tint=(1.0, 0.96, 0.95))),
        "Cinematic": Rig(color_strength=1.0, distance=2.6, layout=Matched(key=Spot(), softness=0.7, rim=math.log2(1.5), fill=math.log2(0.15), tint=(1.05, 0.95, 0.85))),
        "Dramatic": Rig(color_strength=0.9, layout=Matched(key=Spot(), softness=0.45, rim=math.log2(0.8), fill=math.log2(0.05), tint=(1.0, 0.98, 0.92))),
        "Film Noir": Rig(color_strength=0.95, layout=Matched(key=Spot(), softness=0.4, rim=math.log2(1.2), fill=math.log2(0.03), tint=(0.98, 0.99, 1.0))),
        "Low Key": Rig(color_strength=0.9, layout=Matched(key=Spot(), softness=0.6, rim=math.log2(0.6), fill=math.log2(0.05), tint=(1.0, 0.97, 0.92))),
        "High Key": Rig(color_strength=0.55, distance=2.8, layout=Matched(softness=2.6, rim=None, fill=math.log2(0.9))),
        "Studio": Rig(color_strength=0.6, distance=2.8, layout=Matched(softness=2.2, rim=None, fill=math.log2(0.7))),
        "Product": Rig(color_strength=0.6, distance=2.7, layout=Matched(softness=2.0, rim=None, fill=math.log2(0.6))),
        "Soft Even": Rig(color_strength=0.5, distance=3.0, layout=Matched(softness=3.0, rim=None, fill=math.log2(0.95))),
        "Rim / Hair": Rig(layout=Matched(rim=math.log2(2.0), fill=math.log2(0.3), tint=(1.0, 0.98, 0.95))),
        "Backlight": Rig(color_strength=0.9, distance=2.7, layout=Matched(softness=1.2, rim=None, fill=math.log2(0.25), tint=(1.0, 0.97, 0.9))),
        "Outdoor": Rig(distance=3.0, layout=Matched(key=Sun(), rim=None, fill=math.log2(0.4), tint=(1.0, 0.97, 0.9))),
        "Sunset": Rig(color_strength=1.0, distance=3.0, layout=Matched(key=Sun(), rim=None, fill=math.log2(0.35), tint=(1.1, 0.82, 0.55))),
        "Twilight": Rig(color_strength=0.9, distance=2.9, layout=Matched(softness=1.6, rim=None, fill=math.log2(0.6), tint=(0.7, 0.8, 1.05))),
        "Neon": Rig(color_strength=1.0, layout=Matched(rim=math.log2(1.4), fill=math.log2(0.4))),
        "Candlelight": Rig(color_strength=1.0, distance=2.4, layout=Matched(key=Point(), softness=1.4, rim=None, fill=math.log2(0.5), tint=(1.12, 0.72, 0.42))),
        "Moonlight": Rig(color_strength=0.95, distance=3.0, layout=Matched(key=Sun(), softness=1.2, rim=None, fill=math.log2(0.3), tint=(0.6, 0.72, 1.05))),
        "Underlight": Rig(color_strength=0.9, distance=2.4, layout=Matched(key=Spot(), softness=0.9, rim=None, fill=math.log2(0.15), tint=(1.0, 0.95, 0.9))),
    }


def _scaled(factors: tuple[float, ...], stops: float = 0.0) -> Outcome[float]:
    """Returns product times 2**stops as float, overflow as numeric fault."""
    return catch(np.errstate(over="raise")(lambda: float(np.prod(factors) * np.exp2(stops))), exception=FloatingPointError)().map_error(
        lambda error: (Fault(reason=RecordReason.NUMERIC, subject=f"{factors} x 2^{stops}", cause=error),)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Ambient", "Arranged", "Background", "Disk", "Emitter", "Layout", "Light", "Matched", "Placement", "Placements", "Point", "Rig", "Role", "Spot", "Sun", "rig_catalog"]
