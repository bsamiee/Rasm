"""Light rigs placed about target by delivered irradiance."""

from enum import IntEnum
from functools import partial
import math
import sys
from types import MappingProxyType
from typing import Annotated, ClassVar, Final, override, Self

from expression import Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from rasm_imaging import applied, LightingReading, luma, processor, Samples
from rasm_records import accumulated, Bearing, Case, Fault, Finite, Inclination, NonNegative, Nothing, Ok, Outcome, paired, Positive, Reason, Rgb, Unit

# --- [TYPES] ----------------------------------------------------------------------------

type _Emitter = Point | Spot | Disk | Sun
type _Layout = Arranged | Matched
type _Distance = Annotated[float, msgspec.Meta(gt=1.0, le=sys.float_info.max)]
type _Diameter = Annotated[float, msgspec.Meta(ge=0.0, le=math.pi)]


class LightRole(IntEnum):
    """Light role in rig."""

    KEY = 1
    FILL = 2
    RIM = 3
    SKY = 4
    ACCENT = 5


# --- [MODELS] ---------------------------------------------------------------------------


class _Source(Case, frozen=True, kw_only=True):
    """Emitter case base holding the solid angle factor its radiant power takes over the irradiance it delivers."""

    coefficient: ClassVar[float] = math.pi

    def power(self, reach: float, extent: float, irradiance: float) -> Outcome[float]:
        """Returns radiant watts delivering irradiance at reach from a light subtending extent."""
        return _scaled((self.coefficient, reach, reach, 1.0 + extent**2, irradiance))


class Point(_Source, frozen=True, kw_only=True):
    """Sphere light emitting in every direction."""

    coefficient = 4.0 * math.pi


class Spot(_Source, frozen=True, kw_only=True):
    """Sphere light with beam limited to cone."""

    coefficient = 4.0 * math.pi
    cone: Annotated[float, msgspec.Meta(ge=sys.float_info.min, le=math.pi)] = math.pi / 4.0
    blend: Unit = 0.15


class Disk(_Source, frozen=True, kw_only=True):
    """Disk area light facing target."""


class Sun(_Source, frozen=True, kw_only=True):
    """Distant light at uniform irradiance."""

    @override
    def power(self, reach: float, extent: float, irradiance: float) -> Outcome[float]:
        """Returns the irradiance as watts per square meter, a distant light holding it at any reach."""
        return Ok(irradiance)


class Light(msgspec.Struct, frozen=True, kw_only=True):
    """Rig light with irradiance offset from key."""

    role: LightRole = LightRole.KEY
    emitter: _Emitter = Disk()
    azimuth: Bearing = 0.0
    elevation: Inclination = 0.0
    stops: Finite = 0.0
    size: _Diameter = 2.0 * math.asin(0.2)
    color: Rgb = (1.0, 1.0, 1.0)
    temperature: Positive | None = None

    @property
    def direction(self) -> tuple[float, float, float]:
        """Unit vector toward light over right, up, and back."""
        horizontal = math.cos(self.elevation)
        return horizontal * math.sin(self.azimuth), math.sin(self.elevation), horizontal * math.cos(self.azimuth)

    @property
    def extent(self) -> float:
        """Sine of half angle light subtends at target center."""
        return math.sin(self.size / 2.0)


class AmbientLight(msgspec.Struct, frozen=True, kw_only=True):
    """World light color and irradiance offset."""

    color: Rgb = (1.0, 1.0, 1.0)
    stops: Finite = 0.0


class Arranged(Case, frozen=True, kw_only=True):
    """Lights and world light as stated."""

    lights: tuple[Light, ...] = (Light(),)
    ambient: AmbientLight | None = None


class Matched(Case, frozen=True, kw_only=True):
    """Lights derived from reference reading."""

    count: Annotated[int, msgspec.Meta(ge=1, le=8)] = 3
    key: _Emitter = Disk()
    softness: NonNegative = 1.0
    fill_size: _Diameter = math.radians(44.0)
    fill: Finite | None = None
    rim: Finite | None = 0.0
    ambient: Finite | None = 0.0
    tint: Rgb = (1.0, 1.0, 1.0)

    def arranged(self, reading: LightingReading) -> Outcome[Arranged]:
        """Derives key, fill, rim, accent, and world lights up to count."""

        def peak(color: npt.NDArray[np.float64]) -> float:
            return max(float(color.max()), Samples.floor)

        def filtered(color: npt.NDArray[np.float64]) -> tuple[float, float, float]:
            values = color * self.tint / peak(color)
            return values.item(0), values.item(1), values.item(2)

        @catch(exception=FloatingPointError)
        @np.errstate(over="raise", divide="raise", invalid="raise")
        def lit() -> Arranged:
            if (hardness := reading.hardness) <= 0.0:
                return Arranged(lights=(), ambient=AmbientLight(color=filtered(reading.key)))
            azimuth, elevation = (component * math.pi / 2.0 for component in reading.direction)
            scale = max(hardness, abs(reading.contrast))
            size = math.pi - 2.0 * math.atan2(hardness / scale, self.softness * (reading.contrast / scale))
            key = Light(emitter=self.key, azimuth=azimuth, elevation=elevation, size=size, color=filtered(reading.key))
            fill = Light(role=LightRole.FILL, azimuth=-azimuth, stops=Option.of_optional(self.fill).default_value(-reading.contrast), size=self.fill_size, color=filtered(reading.fill))
            rim = Option.of_optional(self.rim).bind(
                lambda offset: reading.rim.map2(
                    lambda color, backlight: Light(
                        role=LightRole.RIM, azimuth=math.remainder(azimuth + math.pi, math.tau), elevation=elevation, stops=backlight + offset, size=size, color=filtered(color)
                    ),
                    reading.backlight,
                )
            )
            accents = (
                Light(
                    role=LightRole.ACCENT,
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
                .map(lambda offset: AmbientLight(color=filtered(reading.ambient), stops=math.log2(peak(reading.ambient)) - math.log2(peak(reading.key)) + offset))
                .to_optional(),
            )

        return lit().map_error(partial(Fault.of, Reason.FAILED, repr(reading)))


class PlacedLight(msgspec.Struct, frozen=True, kw_only=True):
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
        return self.light.emitter.power(self.reach, self.light.extent, self.irradiance)


class Background(msgspec.Struct, frozen=True, kw_only=True):
    """Uniform world radiance."""

    color: Rgb
    strength: NonNegative


class PlacedRig(msgspec.Struct, frozen=True, kw_only=True):
    """Lights placed about target sphere with world."""

    lights: tuple[PlacedLight, ...]
    background: Option[Background]


class _Fixture(msgspec.Struct, frozen=True, kw_only=True):
    """Measured light: role, offset from target in meters, radiant watts, emitter side in meters, and color temperature in kelvin."""

    role: LightRole
    offset: tuple[float, float, float]
    watts: float
    side: float
    temperature: float

    @property
    def distance(self) -> float:
        """Meters from target."""
        return math.hypot(*self.offset)

    @property
    def irradiance(self) -> float:
        """Watts per square meter delivered at target."""
        return self.watts / (math.pi * (self.distance**2 + (self.side / 2.0) ** 2))


class Rig(msgspec.Struct, frozen=True, kw_only=True):
    """Lights about target at key irradiance."""

    irradiance: NonNegative = math.pi
    distance: _Distance = 2.5
    color_strength: NonNegative = 0.85
    color_saturation: NonNegative = 1.0
    layout: _Layout = Matched()

    def arranged(self, reading: Option[LightingReading]) -> Outcome[Option[Arranged]]:
        """Returns stated layout, or matched layout when reading exists."""
        match self.layout:
            case Arranged() as arranged:
                return Ok(Some(arranged))
            case Matched() as matched:
                return reading.map(lambda found: matched.arranged(found).map(Some)).default_value(Ok(Nothing))

    def placements(self, config: PyOpenColorIO.Config, radius: float, reading: Option[LightingReading]) -> Outcome[Option[PlacedRig]]:
        """Places rig with graded colors, Nothing for matched layout without reading."""
        blend = min(self.color_strength, 1.0)

        def transform(weights: list[float]) -> PyOpenColorIO.GroupTransform:
            return PyOpenColorIO.GroupTransform([
                PyOpenColorIO.MatrixTransform.Fit(newMin=[1.0 - blend] * 3 + [0.0]),
                PyOpenColorIO.MatrixTransform.Sat(max(self.color_strength, 1.0), weights),
                PyOpenColorIO.MatrixTransform.Sat(self.color_saturation, weights),
                PyOpenColorIO.RangeTransform(minInValue=0.0, minOutValue=0.0),
            ])

        def placed(arranged: Arranged, built: PyOpenColorIO.Processor, reach: float) -> Outcome[Option[PlacedRig]]:
            operation = built.getDefaultCPUProcessor()

            def graded(color: Rgb) -> Outcome[Rgb]:
                return catch(
                    lambda: msgspec.convert(applied(operation, np.asarray([color], dtype=np.float32))[0].tolist(), type=tuple[Finite, Finite, Finite]), exception=msgspec.ValidationError
                )().map_error(partial(Fault.of, Reason.FAILED, repr(color)))

            def light_at(light: Light) -> Outcome[PlacedLight]:
                return paired(graded(light.color), _scaled((self.irradiance,), light.stops)).map(
                    lambda found: PlacedLight(light=msgspec.structs.replace(light, color=found[0]), reach=reach, irradiance=found[1])
                )

            def world(ambient: AmbientLight) -> Outcome[Option[Background]]:
                return paired(graded(ambient.color), _scaled((self.irradiance, 1.0 / math.pi), ambient.stops)).map(lambda found: Some(Background(color=found[0], strength=found[1])))

            return paired(accumulated(light_at(light) for light in arranged.lights), Option.of_optional(arranged.ambient).map(world).default_value(Ok(Nothing))).map(
                lambda found: Some(PlacedRig(lights=found[0], background=found[1]))
            )

        return self.arranged(reading).bind(
            lambda arranged: arranged.map(
                lambda layout: paired(luma(config).bind(lambda weights: processor(config, transform(weights.tolist()))), _scaled((self.distance, radius))).bind(lambda found: placed(layout, *found))
            ).default_value(Ok(Nothing))
        )

    @classmethod
    def measured(cls, key: _Fixture, /, *others: _Fixture) -> Self:
        """Returns arranged rig of measured lights at the irradiance `key` delivers."""
        return cls(
            irradiance=key.irradiance,
            layout=Arranged(
                lights=tuple(
                    Light(
                        role=fixture.role,
                        azimuth=math.atan2(fixture.offset[0], -fixture.offset[1]),
                        elevation=math.atan2(fixture.offset[2], math.hypot(*fixture.offset[:2])),
                        size=2.0 * math.asin(fixture.side / (2.0 * fixture.distance)),
                        stops=math.log2(fixture.irradiance / key.irradiance),
                        temperature=fixture.temperature,
                    )
                    for fixture in (key, *others)
                )
            ),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _scaled(factors: tuple[float, ...], stops: float = 0.0) -> Outcome[float]:
    """Returns product times 2**stops as float, overflow as `FAILED` fault."""

    @catch(exception=FloatingPointError)
    @np.errstate(over="raise")
    def product() -> float:
        return float(np.prod(factors) * np.exp2(stops))

    return product().map_error(partial(Fault.of, Reason.FAILED, f"{factors} x 2^{stops}"))


# --- [TABLES] ---------------------------------------------------------------------------

RIGS: Final = MappingProxyType({
    "Three-Point": Rig.measured(
        _Fixture(role=LightRole.KEY, offset=(1.6, -1.6, 0.30), watts=250.0, side=0.6, temperature=5500.0),
        _Fixture(role=LightRole.FILL, offset=(-1.6, -1.2, -0.10), watts=90.0, side=1.5, temperature=6500.0),
        _Fixture(role=LightRole.RIM, offset=(0.5, 1.6, 0.80), watts=400.0, side=0.4, temperature=5000.0),
    ),
    "Hard Low Key": Rig.measured(
        _Fixture(role=LightRole.KEY, offset=(2.0, -1.0, 0.40), watts=450.0, side=0.4, temperature=4500.0),
        _Fixture(role=LightRole.FILL, offset=(-1.5, -1.0, -0.20), watts=25.0, side=1.0, temperature=7000.0),
        _Fixture(role=LightRole.RIM, offset=(-0.4, 1.5, 0.80), watts=200.0, side=0.3, temperature=4200.0),
    ),
    "Side Key": Rig.measured(_Fixture(role=LightRole.KEY, offset=(2.4, -0.6, 1.00), watts=550.0, side=0.3, temperature=4200.0)),
    "Soft High Key": Rig.measured(
        _Fixture(role=LightRole.KEY, offset=(1.2, -2.0, 0.50), watts=300.0, side=2.0, temperature=5500.0),
        _Fixture(role=LightRole.FILL, offset=(-1.2, -2.0, 0.50), watts=250.0, side=2.0, temperature=5500.0),
        _Fixture(role=LightRole.SKY, offset=(0.0, 0.0, 2.20), watts=180.0, side=1.5, temperature=5500.0),
    ),
    "Rim Only": Rig.measured(_Fixture(role=LightRole.RIM, offset=(0.0, 1.5, 1.20), watts=600.0, side=0.5, temperature=5000.0)),
    "Glamour": Rig.measured(
        _Fixture(role=LightRole.KEY, offset=(0.0, -1.6, 1.40), watts=350.0, side=0.7, temperature=5500.0),
        _Fixture(role=LightRole.FILL, offset=(0.0, -1.0, -0.40), watts=80.0, side=1.5, temperature=6000.0),
    ),
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
})

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["RIGS", "AmbientLight", "Arranged", "Background", "Disk", "Light", "LightRole", "Matched", "PlacedLight", "PlacedRig", "Point", "Rig", "Spot", "Sun"]
