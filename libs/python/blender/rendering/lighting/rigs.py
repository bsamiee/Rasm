"""Light rigs placed about a target from the irradiance each light delivers there."""

import math
import sys
from typing import Annotated

from colour.models.rgb.datasets.srgb import MATRIX_sRGB_TO_XYZ
from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np
import PyOpenColorIO

from blender.imaging.analysis.lighting import LightingReading
from blender.imaging.analysis.metering import Samples
from blender.imaging.color.processors import applied, processor
from blender.imaging.faults import accumulated, ImagingFaults, paired
from blender.imaging.parameters import Case, Choice, Finite, Gated, Presentation, Quantity, Rgb, Stops
from blender.rendering.faults import RenderingArithmeticFault, RenderingFaults, RenderingImagingFault, RenderingResult, RenderingSizeFault

# --- [TYPES] ----------------------------------------------------------------------------

type Radius = Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max)]
type Emitter = Point | Spot | Disk | Sun
type Layout = Arranged | Matched


class Role(Choice):
    """Part a light plays in its rig."""

    KEY = 1, "Key"
    FILL = 2, "Fill"
    RIM = 3, "Rim"
    SKY = 4, "Sky"
    ACCENT = 5, "Accent"


# --- [MODELS] ---------------------------------------------------------------------------


class Point(Case, frozen=True, tag=1):
    """Sphere light emitting in every direction."""

    title = "Point"


class Spot(Case, frozen=True, kw_only=True, tag=2):
    """Sphere light whose beam the cone limits."""

    title = "Spot"
    cone: Annotated[float, msgspec.Meta(ge=math.radians(1.0), le=math.pi, description="Angular diameter of the beam"), Presentation(quantity=Some(Quantity.ANGLE))] = math.pi / 4.0
    blend: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, description="Share of the beam the edge softens")] = 0.15


class Disk(Case, frozen=True, tag=3):
    """Disk-shaped area light facing the target."""

    title = "Area"


class Sun(Case, frozen=True, tag=4):
    """Distant light reaching every point at one irradiance."""

    title = "Sun"


class Light(msgspec.Struct, frozen=True, kw_only=True):
    """Rig light's direction, angular extent, color, and irradiance offset."""

    role: Role = Role.KEY
    emitter: Emitter = Disk()
    azimuth: Annotated[float, msgspec.Meta(ge=-math.pi, le=math.pi), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    elevation: Annotated[float, msgspec.Meta(ge=-math.pi / 2.0, le=math.pi / 2.0), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    stops: Annotated[Finite, msgspec.Meta(description="Irradiance at the target in stops from the rig's key irradiance")] = 0.0
    size: Annotated[float, msgspec.Meta(ge=0.0, le=math.pi, description="Angular diameter at the target center"), Presentation(quantity=Some(Quantity.ANGLE))] = 2.0 * math.asin(0.2)
    color: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)
    temperature: Annotated[Gated[Annotated[float, msgspec.Meta(ge=1800.0, le=20000.0)]], Presentation(step=Some(1.0), decimals=Some(0), quantity=Some(Quantity.COLOR_TEMPERATURE))] = Gated(
        enabled=False, value=6500.0
    )

    @property
    def direction(self) -> tuple[float, float, float]:
        """Unit vector from the target toward the light over right, up, and back."""
        horizontal = math.cos(self.elevation)
        return horizontal * math.sin(self.azimuth), math.sin(self.elevation), horizontal * math.cos(self.azimuth)


class Ambient(msgspec.Struct, frozen=True, kw_only=True):
    """World light's color and irradiance offset."""

    color: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)
    stops: Finite = 0.0


class Arranged(Case, frozen=True, kw_only=True, tag=5):
    """Lights and world light as stated."""

    title = "Arranged"
    lights: tuple[Light, ...] = (Light(),)
    ambient: Gated[Ambient] = Gated(enabled=False, value=Ambient())


class Matched(Case, frozen=True, kw_only=True, tag=6):
    """Lights derived from a reference reading at placement."""

    title = "Matched"
    count: Annotated[int, msgspec.Meta(ge=1, le=8, title="Lights")] = 3
    key: Annotated[Emitter, msgspec.Meta(title="Key Emitter")] = Disk()
    softness: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max), Presentation(soft=Some((0.0, 4.0)), decimals=Some(2))] = 1.0
    fill_size: Annotated[float, msgspec.Meta(ge=0.0, le=math.pi), Presentation(quantity=Some(Quantity.ANGLE))] = math.radians(44.0)
    fill: Annotated[Gated[Stops], msgspec.Meta(title="Fill Stops")] = Gated(enabled=False, value=math.log2(0.3))
    rim: Gated[Stops] = Gated(enabled=True, value=0.0)
    ambient: Gated[Stops] = Gated(enabled=True, value=0.0)
    tint: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)

    def arranged(self, reading: LightingReading) -> RenderingResult[Arranged]:
        """Derives ordered key, fill, rim, accents, and world from the reading."""

        def peak(color: Rgb) -> float:
            return max(color.red, color.green, color.blue, Samples.floor)

        def filtered(color: Rgb) -> Rgb:
            level = peak(color)
            red, green, blue = (float(np.multiply(shade, value / level)) for shade, value in zip(msgspec.structs.astuple(self.tint), msgspec.structs.astuple(color)))
            return Rgb(red=red, green=green, blue=blue)

        def lit(hardness: float) -> Arranged:
            azimuth, elevation = (component * math.pi / 2.0 for component in reading.direction)
            scale = max(hardness, abs(reading.contrast))
            size = math.pi - 2.0 * math.atan2(hardness / scale, self.softness * (reading.contrast / scale))
            key = Light(emitter=self.key, azimuth=azimuth, elevation=elevation, size=size, color=filtered(reading.key))
            fill = Light(role=Role.FILL, azimuth=-azimuth, stops=self.fill.value if self.fill.enabled else -reading.contrast, size=self.fill_size, color=filtered(reading.fill))
            rim = (
                reading.rim.bind(
                    lambda color: reading.backlight.map(
                        lambda backlight: Light(
                            role=Role.RIM, azimuth=math.remainder(azimuth + math.pi, math.tau), elevation=elevation, stops=backlight + self.rim.value, size=size, color=filtered(color)
                        )
                    )
                )
                if self.rim.enabled
                else Nothing
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
                ambient=Gated(enabled=self.ambient.enabled, value=Ambient(color=filtered(reading.ambient), stops=math.log2(peak(reading.ambient)) - math.log2(peak(reading.key)) + self.ambient.value)),
            )

        try:
            with np.errstate(over="raise", invalid="raise", divide="raise", under="ignore"):
                return Ok(lit(reading.hardness) if reading.hardness > 0.0 else Arranged(lights=(), ambient=Gated(enabled=True, value=Ambient(color=filtered(reading.key)))))
        except (OverflowError, FloatingPointError) as error:
            return Error((RenderingArithmeticFault.of((self, reading), error),))


class Placement(msgspec.Struct, frozen=True, kw_only=True):
    """Light at its target distance and delivered irradiance."""

    light: Light
    reach: Radius
    irradiance: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max)]

    @property
    def location(self) -> tuple[float, float, float]:
        """Light position in meters over the target's right, up, and back axes."""
        right, up, back = self.light.direction
        return self.reach * right, self.reach * up, self.reach * back

    @property
    def radius(self) -> float:
        """Radius in meters of the light's sphere at the target center."""
        return self.reach * math.sin(self.light.size / 2.0)

    @property
    def power(self) -> RenderingResult[float]:
        """Radiant watts, or a sun's watts per square meter."""
        try:
            match self.light.emitter:
                case Point() | Spot():
                    coefficient = 4.0 * math.pi
                case Disk():
                    coefficient = math.pi
                case Sun():
                    return Ok(self.irradiance)
            return Ok(_scaled((coefficient, self.reach, self.reach, 1.0 + math.sin(self.light.size / 2.0) ** 2, self.irradiance)))
        except OverflowError as error:
            return Error((RenderingArithmeticFault.of(self, error),))


class Background(msgspec.Struct, frozen=True, kw_only=True):
    """Uniform world radiance a rig sets."""

    color: Rgb[Finite]
    strength: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max)]


class Placements(msgspec.Struct, frozen=True, kw_only=True):
    """Lights placed about a target sphere with their world."""

    radius: Radius
    distance: Annotated[float, msgspec.Meta(gt=1.0, le=sys.float_info.max)]
    lights: tuple[Placement, ...]
    background: Option[Background]


class Rig(msgspec.Struct, frozen=True, kw_only=True):
    """Lights placed about a target at a declared key irradiance."""

    irradiance: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Key Irradiance"), Presentation(decimals=Some(3), quantity=Some(Quantity.IRRADIANCE))] = math.pi
    distance: Annotated[float, msgspec.Meta(gt=1.0, le=sys.float_info.max), Presentation(soft=Some((1.0, 10.0)), decimals=Some(2))] = 2.5
    color_strength: Annotated[
        float, msgspec.Meta(ge=0.0, le=sys.float_info.max, description="White interpolation through one, then fixed Rec.709 chroma boost"), Presentation(soft=Some((0.0, 1.5)))
    ] = 0.85
    color_saturation: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, description="Independent fixed Rec.709 saturation after color strength"), Presentation(soft=Some((0.0, 2.0)))] = 1.0
    layout: Layout = Matched()

    def arranged(self, reading: Option[LightingReading]) -> RenderingResult[Option[Arranged]]:
        """Returns the stated layout or the matched layout when a reading exists."""
        match self.layout:
            case Arranged() as arranged:
                return Ok(Some(arranged))
            case Matched() as matched:
                return reading.map(lambda found: matched.arranged(found).map(Some)).default_value(Ok(Nothing))

    def placements(self, config: PyOpenColorIO.Config, radius: Radius, reading: Option[LightingReading]) -> RenderingResult[Option[Placements]]:
        """Prepares final light and world colors, then places the admitted target rig."""
        blend = min(self.color_strength, 1.0)
        luma = MATRIX_sRGB_TO_XYZ[1].tolist()
        transform = PyOpenColorIO.GroupTransform([
            PyOpenColorIO.MatrixTransform.Fit(newMin=[1.0 - blend] * 3 + [0.0]),
            PyOpenColorIO.MatrixTransform.Sat(max(self.color_strength, 1.0), luma),
            PyOpenColorIO.MatrixTransform.Sat(self.color_saturation, luma),
            PyOpenColorIO.RangeTransform(minInValue=0.0, minOutValue=0.0),
        ])

        def placed(arranged: Arranged, operation: PyOpenColorIO.CPUProcessor) -> RenderingResult[Option[Placements]]:
            try:
                reach = _scaled((self.distance, radius))
            except OverflowError as error:
                return Error((RenderingArithmeticFault(request=(self.distance, radius), error=error),))

            def graded(color: Rgb[Finite]) -> RenderingResult[Rgb[Finite]]:
                try:
                    with np.errstate(over="raise", invalid="raise", under="ignore"):
                        values = applied(operation, np.asarray([msgspec.structs.astuple(color)], dtype=np.float32))
                    return Ok(msgspec.convert(values[0].tolist(), type=Rgb[Finite]))
                except (OverflowError, FloatingPointError, msgspec.ValidationError) as error:
                    return Error((RenderingArithmeticFault.of((self.color_strength, self.color_saturation, color), error),))
                except MemoryError as error:
                    return Error((RenderingSizeFault(request=(1, 3), error=error),))

            def light_at(light: Light) -> RenderingResult[Placement]:
                try:
                    irradiance = Ok(_scaled((self.irradiance,), light.stops))
                except OverflowError as error:
                    irradiance = Error((RenderingArithmeticFault(request=(self.irradiance, light.stops), error=error),))
                return paired(graded(light.color), irradiance).map(lambda found: Placement(light=msgspec.structs.replace(light, color=found[0]), reach=reach, irradiance=found[1]))

            def world() -> RenderingResult[Option[Background]]:
                if not arranged.ambient.enabled:
                    return Ok(Nothing)
                try:
                    strength = Ok(_scaled((self.irradiance, 1.0 / math.pi), arranged.ambient.value.stops))
                except OverflowError as error:
                    strength = Error((RenderingArithmeticFault(request=(self.irradiance, arranged.ambient.value.stops, math.pi), error=error),))
                return paired(graded(arranged.ambient.value.color), strength).map(lambda found: Some(Background(color=found[0], strength=found[1])))

            return paired(accumulated(light_at(light) for light in arranged.lights), world()).map(
                lambda found: Some(Placements(radius=radius, distance=self.distance, lights=found[0], background=found[1]))
            )

        def refused(faults: ImagingFaults) -> RenderingFaults:
            return (RenderingImagingFault(faults=faults),)

        return self.arranged(reading).bind(
            lambda arranged: arranged.map(lambda layout: processor(config, transform).map_error(refused).bind(lambda built: placed(layout, built.getDefaultCPUProcessor()))).default_value(Ok(Nothing))
        )


def rig_catalog() -> dict[str, Rig]:
    """Constructs the six arranged and twenty-five matched source recipes."""

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
                        temperature=Gated(enabled=True, value=temperature),
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
        "Portrait": Rig(distance=2.4, layout=Matched(softness=1.1, tint=Rgb(red=1.0, green=0.97, blue=0.92))),
        "Rembrandt": Rig(
            color_strength=0.9,
            distance=2.4,
            layout=Matched(key=Spot(), softness=0.9, rim=Gated(enabled=True, value=math.log2(0.8)), fill=Gated(enabled=True, value=math.log2(0.12)), tint=Rgb(red=1.0, green=0.95, blue=0.85)),
        ),
        "Butterfly": Rig(
            color_strength=0.8,
            distance=2.3,
            layout=Matched(softness=1.3, rim=Gated(enabled=True, value=math.log2(0.6)), fill=Gated(enabled=True, value=math.log2(0.45)), tint=Rgb(red=1.0, green=0.97, blue=0.95)),
        ),
        "Loop": Rig(distance=2.4, layout=Matched(softness=1.1, rim=Gated(enabled=True, value=math.log2(0.7)), fill=Gated(enabled=True, value=math.log2(0.3)), tint=Rgb(red=1.0, green=0.96, blue=0.9))),
        "Split": Rig(
            color_strength=0.9,
            distance=2.4,
            layout=Matched(softness=0.8, rim=Gated(enabled=True, value=math.log2(0.5)), fill=Gated(enabled=True, value=math.log2(0.08)), tint=Rgb(red=1.0, green=0.98, blue=0.95)),
        ),
        "Clamshell": Rig(
            color_strength=0.7,
            distance=2.2,
            layout=Matched(softness=2.2, rim=Gated(enabled=False, value=0.0), fill=Gated(enabled=True, value=math.log2(0.85)), tint=Rgb(red=1.0, green=0.97, blue=0.96)),
        ),
        "Beauty": Rig(
            color_strength=0.75,
            distance=2.2,
            layout=Matched(softness=2.4, rim=Gated(enabled=True, value=math.log2(0.5)), fill=Gated(enabled=True, value=math.log2(0.8)), tint=Rgb(red=1.0, green=0.96, blue=0.95)),
        ),
        "Cinematic": Rig(
            color_strength=1.0,
            distance=2.6,
            layout=Matched(key=Spot(), softness=0.7, rim=Gated(enabled=True, value=math.log2(1.5)), fill=Gated(enabled=True, value=math.log2(0.15)), tint=Rgb(red=1.05, green=0.95, blue=0.85)),
        ),
        "Dramatic": Rig(
            color_strength=0.9,
            layout=Matched(key=Spot(), softness=0.45, rim=Gated(enabled=True, value=math.log2(0.8)), fill=Gated(enabled=True, value=math.log2(0.05)), tint=Rgb(red=1.0, green=0.98, blue=0.92)),
        ),
        "Film Noir": Rig(
            color_strength=0.95,
            layout=Matched(key=Spot(), softness=0.4, rim=Gated(enabled=True, value=math.log2(1.2)), fill=Gated(enabled=True, value=math.log2(0.03)), tint=Rgb(red=0.98, green=0.99, blue=1.0)),
        ),
        "Low Key": Rig(
            color_strength=0.9,
            layout=Matched(key=Spot(), softness=0.6, rim=Gated(enabled=True, value=math.log2(0.6)), fill=Gated(enabled=True, value=math.log2(0.05)), tint=Rgb(red=1.0, green=0.97, blue=0.92)),
        ),
        "High Key": Rig(color_strength=0.55, distance=2.8, layout=Matched(softness=2.6, rim=Gated(enabled=False, value=0.0), fill=Gated(enabled=True, value=math.log2(0.9)))),
        "Studio": Rig(color_strength=0.6, distance=2.8, layout=Matched(softness=2.2, rim=Gated(enabled=False, value=0.0), fill=Gated(enabled=True, value=math.log2(0.7)))),
        "Product": Rig(color_strength=0.6, distance=2.7, layout=Matched(softness=2.0, rim=Gated(enabled=False, value=0.0), fill=Gated(enabled=True, value=math.log2(0.6)))),
        "Soft Even": Rig(color_strength=0.5, distance=3.0, layout=Matched(softness=3.0, rim=Gated(enabled=False, value=0.0), fill=Gated(enabled=True, value=math.log2(0.95)))),
        "Rim / Hair": Rig(layout=Matched(rim=Gated(enabled=True, value=math.log2(2.0)), fill=Gated(enabled=True, value=math.log2(0.3)), tint=Rgb(red=1.0, green=0.98, blue=0.95))),
        "Backlight": Rig(
            color_strength=0.9,
            distance=2.7,
            layout=Matched(softness=1.2, rim=Gated(enabled=False, value=0.0), fill=Gated(enabled=True, value=math.log2(0.25)), tint=Rgb(red=1.0, green=0.97, blue=0.9)),
        ),
        "Outdoor": Rig(distance=3.0, layout=Matched(key=Sun(), rim=Gated(enabled=False, value=0.0), fill=Gated(enabled=True, value=math.log2(0.4)), tint=Rgb(red=1.0, green=0.97, blue=0.9))),
        "Sunset": Rig(
            color_strength=1.0, distance=3.0, layout=Matched(key=Sun(), rim=Gated(enabled=False, value=0.0), fill=Gated(enabled=True, value=math.log2(0.35)), tint=Rgb(red=1.1, green=0.82, blue=0.55))
        ),
        "Twilight": Rig(
            color_strength=0.9, distance=2.9, layout=Matched(softness=1.6, rim=Gated(enabled=False, value=0.0), fill=Gated(enabled=True, value=math.log2(0.6)), tint=Rgb(red=0.7, green=0.8, blue=1.05))
        ),
        "Neon": Rig(color_strength=1.0, layout=Matched(rim=Gated(enabled=True, value=math.log2(1.4)), fill=Gated(enabled=True, value=math.log2(0.4)))),
        "Candlelight": Rig(
            color_strength=1.0,
            distance=2.4,
            layout=Matched(key=Point(), softness=1.4, rim=Gated(enabled=False, value=0.0), fill=Gated(enabled=True, value=math.log2(0.5)), tint=Rgb(red=1.12, green=0.72, blue=0.42)),
        ),
        "Moonlight": Rig(
            color_strength=0.95,
            distance=3.0,
            layout=Matched(key=Sun(), softness=1.2, rim=Gated(enabled=False, value=0.0), fill=Gated(enabled=True, value=math.log2(0.3)), tint=Rgb(red=0.6, green=0.72, blue=1.05)),
        ),
        "Underlight": Rig(
            color_strength=0.9,
            distance=2.4,
            layout=Matched(key=Spot(), softness=0.9, rim=Gated(enabled=False, value=0.0), fill=Gated(enabled=True, value=math.log2(0.15)), tint=Rgb(red=1.0, green=0.95, blue=0.9)),
        ),
    }


# --- [OPERATIONS] -----------------------------------------------------------------------


def _scaled(factors: tuple[float, ...], stops: float = 0.0) -> float:
    """Multiplies physical factors and a stop offset before rounding to native float."""
    fraction, whole = math.modf(stops)
    parts = tuple(math.frexp(value) for value in factors)
    return math.ldexp(math.prod(mantissa for mantissa, _ in parts) * math.exp2(fraction), sum(exponent for _, exponent in parts) + int(whole))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Ambient", "Arranged", "Background", "Disk", "Emitter", "Layout", "Light", "Matched", "Placement", "Placements", "Point", "Radius", "Rig", "Role", "Spot", "Sun", "rig_catalog"]
