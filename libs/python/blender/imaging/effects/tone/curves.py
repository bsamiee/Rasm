"""Tone curves, rendering primaries, and frame-adapted photoreceptor parameters."""

import math
from typing import Annotated, ClassVar, override

from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.imaging.chain import Effect, Stage
from blender.imaging.color.processors import basis, matrix, REC709_SCENE
from blender.imaging.faults import ImagingBaseFault, ImagingContrastFault, ImagingResult, ImagingWhiteFault
from blender.imaging.parameters import Case, ColorSpace, Finite, Gated, Positive, Presentation, Stops, Unit

# --- [TYPES] ----------------------------------------------------------------------------

type DisplayWhite = Annotated[float, msgspec.Meta(ge=0.2, le=16.0), Presentation(soft=Some((0.5, 1.0)), decimals=Some(3))]
type Curve = Sigmoid | PiecewisePower | Logarithmic | ReinhardExtended

# --- [MODELS] ---------------------------------------------------------------------------


class Sigmoid(Case, frozen=True, kw_only=True, tag=1):
    """Log-logistic response pinning scene zero to black and middle gray to itself."""

    contrast: Annotated[float, msgspec.Meta(ge=0.1, le=10.0, description="Slope at middle gray, setting compressed scene range"), Presentation(soft=Some((0.7, 3.0)), decimals=Some(3))] = 1.5
    skew: Annotated[float, msgspec.Meta(ge=-1.0, le=1.0, description="Contrast moved into shadows below 0 and highlights above 0"), Presentation(decimals=Some(3))] = 0.0
    display_white: Annotated[DisplayWhite, msgspec.Meta(description="Display value approached at infinity, 1 at reference white")] = 1.0
    display_black: Annotated[float, msgspec.Meta(ge=0.0, le=0.15, description="Display value at scene zero"), Presentation(soft=Some((0.0, 0.01)), decimals=Some(6))] = 0.000152


class PiecewisePower(Case, frozen=True, kw_only=True, tag=2):
    """Hable's toe, linear, and shoulder segments normalized at their white point."""

    toe_strength: Annotated[Unit, msgspec.Meta(description="Toe depth below linear segment"), Presentation(decimals=Some(3))] = 0.5
    toe_length: Annotated[Unit, msgspec.Meta(description="Lower range the toe spans, applied as its 2.2 power"), Presentation(decimals=Some(3))] = 0.5
    shoulder_strength: Annotated[
        float, msgspec.Meta(ge=0.0, le=16.0, description="Stops of highlight range the shoulder holds above white"), Presentation(soft=Some((0.0, 10.0)), step=Some(0.01), decimals=Some(3))
    ] = 2.0
    shoulder_length: Annotated[float, msgspec.Meta(ge=1e-5, le=1.0, description="Upper range the shoulder spans"), Presentation(decimals=Some(3))] = 0.5
    shoulder_angle: Annotated[Unit, msgspec.Meta(description="Shoulder overshoot past white"), Presentation(decimals=Some(3))] = 1.0
    gamma: Annotated[float, msgspec.Meta(ge=0.1, le=10.0, description="Linear segment power"), Presentation(soft=Some((0.5, 2.0)), decimals=Some(3))] = 1.0


class Logarithmic(Case, frozen=True, kw_only=True, tag=3):
    """Drago's adaptive logarithm mapping scene white to display white."""

    bias: Annotated[float, msgspec.Meta(ge=0.5, le=1.0, description="Logarithm base shape between 2 at black and 10 at scene white"), Presentation(soft=Some((0.7, 0.9)), decimals=Some(2))] = 0.85
    scene_white: Annotated[Gated[Stops], msgspec.Meta(description="Manual white in stops from 1.0, read from frame when off")] = Gated(enabled=False, value=math.log2(100.0))
    display_white: Annotated[DisplayWhite, msgspec.Meta(description="Display value at scene white, 1 at reference white")] = 1.0


class ReinhardExtended(Case, frozen=True, kw_only=True, tag=4):
    """Reinhard's photographic response with a finite scene white mapping to one."""

    scene_white: Annotated[Gated[Stops], msgspec.Meta(description="Manual white in stops from 1.0, read from frame when off")] = Gated(enabled=False, value=0.0)


class RenderingPrimary(msgspec.Struct, frozen=True, kw_only=True):
    """Primary inset and rotated within base gamut's angular limit."""

    inset: Annotated[float, msgspec.Meta(ge=0.0, le=0.99, description="Share of distance from base gamut edge toward white"), Presentation(soft=Some((0.0, 0.5)), decimals=Some(3))] = 0.0
    rotation: Annotated[float, msgspec.Meta(gt=-1.0, lt=1.0, description="Share of base gamut rotation limit about white"), Presentation(decimals=Some(3))] = 0.0


class Application(msgspec.Struct, frozen=True, kw_only=True):
    """Per-channel curve over rendering primaries with hue and energy preservation."""

    preserve_hue: Annotated[Unit, msgspec.Meta(description="Share of hue twist corrected, channel sum kept")] = 1.0
    red: Annotated[RenderingPrimary, msgspec.Meta(description="Red rendering primary")] = RenderingPrimary()
    green: Annotated[RenderingPrimary, msgspec.Meta(description="Green rendering primary")] = RenderingPrimary()
    blue: Annotated[RenderingPrimary, msgspec.Meta(description="Blue rendering primary")] = RenderingPrimary()
    purity: Annotated[Unit, msgspec.Meta(title="Recover Purity", description="Share of each inset the return through outset undoes")] = 0.0


class ToneCurve(Effect, frozen=True, kw_only=True, tag=4):
    """Scene-linear to display-linear curve applied per rendering channel."""

    stage = Stage.TONE
    curve: Annotated[Curve, msgspec.Meta(description="Curve each rendering channel takes")] = Sigmoid()
    space: Annotated[ColorSpace, msgspec.Meta(title="Base Primaries", description="Colorspace rendering primaries move from")] = PyOpenColorIO.ROLE_SCENE_LINEAR
    application: Annotated[Application, msgspec.Meta(description="Rendering primaries and hue handling around curve")] = Application()

    @property
    @override
    def pointwise(self) -> bool:
        """Whether selected curve uses a manual white or no frame adaptation."""
        match self.curve:
            case Logarithmic(scene_white=white) | ReinhardExtended(scene_white=white):
                return white.enabled
            case Sigmoid() | PiecewisePower():
                return True

    def base(self, config: PyOpenColorIO.Config) -> ImagingResult[npt.NDArray[np.float64]]:
        """Returns a linear RGB-to-XYZ D65 basis, or provider and unsupported-base faults."""

        def validated(values: npt.NDArray[np.float64]) -> ImagingResult[npt.NDArray[np.float64]]:
            unsupported = (ImagingBaseFault(space=self.space, transform=matrix(values)),)
            try:
                with np.errstate(over="raise", invalid="raise", divide="raise"):
                    sums = values.sum(axis=0)
                    if not ((values[1] > 0.0).all() and ((sums > 0.0).all() or (sums < 0.0).all()) and np.isfinite(sums.sum()) and np.isfinite(values.sum(axis=1)).all()):
                        return Error(unsupported)
                    normalized = values / sums
                    return Ok(values) if np.isfinite(normalized).all() and abs(np.linalg.slogdet(normalized).sign) == 1.0 else Error(unsupported)
            except FloatingPointError:
                return Error(unsupported)

        return basis(config, self.space).bind(validated)


class Neutral(Effect, frozen=True, kw_only=True, tag=5):
    """Khronos PBR Neutral preserving base colors and compressing highlights toward white."""

    stage = Stage.TONE
    title = "PBR Neutral"
    pointwise = True
    space: ClassVar[str] = REC709_SCENE
    reflectance: ClassVar[float] = 0.04
    diffuse: ClassVar[float] = 0.8
    desaturation: ClassVar[float] = 0.15


class Photoreceptor(Effect, frozen=True, kw_only=True, tag=6):
    """Reinhard and Devlin's response against frame adaptation level."""

    stage = Stage.TONE
    pointwise = False
    intensity: Annotated[Finite, msgspec.Meta(description="Brightness, scaling adaptation by e to the negative intensity"), Presentation(soft=Some((-8.0, 8.0)), decimals=Some(3))] = 0.0
    contrast: Annotated[Gated[Positive], msgspec.Meta(description="Response exponent, read from frame luminance range when off"), Presentation(soft=Some((0.3, 1.0)), decimals=Some(3))] = Gated(
        enabled=False, value=1.0
    )
    light_adaptation: Annotated[Unit, msgspec.Meta(description="Blend from frame adaptation at 0 to pixel adaptation at 1")] = 0.0
    chromatic_adaptation: Annotated[Unit, msgspec.Meta(description="Blend from luminance at 0 to each channel at 1")] = 0.0


class Hable(Effect, frozen=True, kw_only=True, tag=48):
    """White-normalized Uncharted 2 response with independent authored coefficients."""

    stage = Stage.TONE
    a: Annotated[float, msgspec.Meta(ge=0.001, le=0.5, description="Shoulder strength"), Presentation(step=Some(0.01), decimals=Some(3))] = 0.15
    b: Annotated[float, msgspec.Meta(ge=0.0, le=0.6, description="Linear strength"), Presentation(step=Some(0.01), decimals=Some(3))] = 0.5
    c: Annotated[float, msgspec.Meta(ge=0.0, le=0.3, description="Linear angle"), Presentation(step=Some(0.01), decimals=Some(3))] = 0.1
    d: Annotated[float, msgspec.Meta(gt=0.0, le=0.9, description="Toe strength"), Presentation(step=Some(0.01), decimals=Some(3))] = 0.2
    e: Annotated[float, msgspec.Meta(ge=0.0001, le=0.05, description="Toe numerator"), Presentation(step=Some(0.001), decimals=Some(4))] = 0.02
    f: Annotated[float, msgspec.Meta(ge=0.2, le=0.6, description="Toe denominator"), Presentation(step=Some(0.01), decimals=Some(3))] = 0.3
    scene_white: Annotated[Gated[Stops], msgspec.Meta(description="Manual white in stops from 1.0, read from frame when off")] = Gated(enabled=True, value=math.log2(11.2))

    @property
    @override
    def pointwise(self) -> bool:
        """Whether white normalization uses manual override."""
        return self.scene_white.enabled

    def normalization(self) -> ImagingResult[Option[tuple[float, float]]]:
        """Returns normalized slope and intercept, absent for automatic white or failing at zero contrast."""
        if not self.scene_white.enabled:
            return Ok(Nothing)
        white = 2.0**self.scene_white.value
        slope = self.a * (self.f - self.e)
        intercept = self.b * (self.c * self.f - self.e)
        denominator = white * slope + intercept
        return Error((ImagingContrastFault(request=self, black=np.array(0.0), white=np.array(0.0)),)) if denominator == 0.0 else Ok(Some((slope / denominator, intercept / denominator)))


class Tony(Effect, frozen=True, kw_only=True, tag=49):
    """Tony McMapface's rationally shaped RGB table in linear Rec.709."""

    stage = Stage.TONE
    title = "Tony McMapface"
    pointwise = True
    space: ClassVar[str] = REC709_SCENE


# --- [OPERATIONS] -----------------------------------------------------------------------


def metered[T: Logarithmic | ReinhardExtended | Hable](curve: T, white: float) -> ImagingResult[T]:
    """Resolves a measured white to a manual override, faulting on an out-of-domain white."""
    return (
        catch(lambda: msgspec.convert(math.log2(white), type=Stops), exception=ValueError)()
        .map(lambda stops: msgspec.structs.replace(curve, scene_white=Gated(enabled=True, value=stops)))
        .map_error(lambda _: (ImagingWhiteFault(white=Some(white)),))
    )


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = [
    "Application",
    "Curve",
    "DisplayWhite",
    "Hable",
    "Logarithmic",
    "Neutral",
    "Photoreceptor",
    "PiecewisePower",
    "ReinhardExtended",
    "RenderingPrimary",
    "Sigmoid",
    "ToneCurve",
    "Tony",
    "metered",
]
