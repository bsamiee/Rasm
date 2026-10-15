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
from blender.imaging.color.processors import basis, matrix
from blender.imaging.faults import ImagingBaseFault, ImagingContrastFault, ImagingResult, ImagingWhiteFault
from blender.imaging.parameters import Case, ColorSpace, Finite, Gated, Positive, Presentation, Stops

# --- [TYPES] ----------------------------------------------------------------------------

type DisplayWhite = Annotated[float, msgspec.Meta(ge=0.2, le=16.0), Presentation(soft=Some((0.5, 1.0)), decimals=Some(3))]
type Curve = Sigmoid | PiecewisePower | Logarithmic | ReinhardExtended

# --- [MODELS] ---------------------------------------------------------------------------


class Sigmoid(Case, frozen=True, kw_only=True, tag=1):
    """Log-logistic response pinning scene zero to black and grey to itself."""

    title = "Sigmoid"
    contrast: Annotated[
        float, msgspec.Meta(ge=0.1, le=10.0, title="Contrast", description="Slope at grey, setting the scene range the curve compresses"), Presentation(soft=Some((0.7, 3.0)), decimals=Some(3))
    ] = 1.5
    skew: Annotated[float, msgspec.Meta(ge=-1.0, le=1.0, title="Skew", description="Contrast moved into shadows below 0 and highlights above 0"), Presentation(decimals=Some(3))] = 0.0
    display_white: Annotated[DisplayWhite, msgspec.Meta(title="Display White", description="Display value approached at infinity, 1 at reference white")] = 1.0
    display_black: Annotated[float, msgspec.Meta(ge=0.0, le=0.15, title="Display Black", description="Display value at scene zero"), Presentation(soft=Some((0.0, 0.01)), decimals=Some(6))] = 0.000152


class PiecewisePower(Case, frozen=True, kw_only=True, tag=2):
    """Hable's toe, linear, and shoulder segments normalized at their white point."""

    title = "Piecewise Power"
    toe_strength: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Toe Strength", description="Depth of the toe below the linear segment"), Presentation(decimals=Some(3))] = 0.5
    toe_length: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Toe Length", description="Lower range the toe spans, applied as its 2.2 power"), Presentation(decimals=Some(3))] = 0.5
    shoulder_strength: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=16.0, title="Shoulder Strength", description="Stops of highlight range the shoulder holds above white"),
        Presentation(soft=Some((0.0, 10.0)), step=Some(0.01), decimals=Some(3)),
    ] = 2.0
    shoulder_length: Annotated[float, msgspec.Meta(ge=1e-5, le=1.0, title="Shoulder Length", description="Upper range the shoulder spans"), Presentation(decimals=Some(3))] = 0.5
    shoulder_angle: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Shoulder Angle", description="Overshoot of the shoulder past white"), Presentation(decimals=Some(3))] = 1.0
    gamma: Annotated[float, msgspec.Meta(ge=0.1, le=10.0, title="Gamma", description="Power of the linear segment"), Presentation(soft=Some((0.5, 2.0)), decimals=Some(3))] = 1.0


class Logarithmic(Case, frozen=True, kw_only=True, tag=3):
    """Drago's adaptive logarithm mapping the scene white level to display white."""

    title = "Logarithmic"
    bias: Annotated[
        float, msgspec.Meta(ge=0.5, le=1.0, title="Bias", description="Shape of the logarithm base between 2 at black and 10 at scene white"), Presentation(soft=Some((0.7, 0.9)), decimals=Some(2))
    ] = 0.85
    scene_white: Annotated[Gated[Stops], msgspec.Meta(title="Scene White", description="Manual white in stops from 1.0, off deriving white from the entering frame")] = Gated(
        enabled=False, value=math.log2(100.0)
    )
    display_white: Annotated[DisplayWhite, msgspec.Meta(title="Display White", description="Display value at scene white, 1 at reference white")] = 1.0


class ReinhardExtended(Case, frozen=True, kw_only=True, tag=4):
    """Reinhard's photographic response with a finite scene white mapping to one."""

    title = "Reinhard Extended"
    scene_white: Annotated[Gated[Stops], msgspec.Meta(title="Scene White", description="Manual white in stops from 1.0, off deriving white from the entering frame")] = Gated(enabled=False, value=0.0)


class RenderingPrimary(msgspec.Struct, frozen=True, kw_only=True):
    """Primary inset and rotated within the selected base gamut's angular limit."""

    inset: Annotated[
        float, msgspec.Meta(ge=0.0, le=0.99, title="Inset", description="Share of the distance from the base gamut edge toward white"), Presentation(soft=Some((0.0, 0.5)), decimals=Some(3))
    ] = 0.0
    rotation: Annotated[float, msgspec.Meta(gt=-1.0, lt=1.0, title="Rotation", description="Fraction of the selected base gamut's rotation limit about white"), Presentation(decimals=Some(3))] = 0.0


class Application(msgspec.Struct, frozen=True, kw_only=True):
    """Per-channel curve over rendering primaries with hue and energy preservation."""

    preserve_hue: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Preserve Hue", description="Share of the hue twist corrected while preserving the channel sum")] = 1.0
    red: Annotated[RenderingPrimary, msgspec.Meta(title="Red", description="Red rendering primary")] = RenderingPrimary()
    green: Annotated[RenderingPrimary, msgspec.Meta(title="Green", description="Green rendering primary")] = RenderingPrimary()
    blue: Annotated[RenderingPrimary, msgspec.Meta(title="Blue", description="Blue rendering primary")] = RenderingPrimary()
    purity: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Recover Purity", description="Share of each inset the return through the outset undoes")] = 0.0


class ToneCurve(Effect, frozen=True, kw_only=True, tag=4):
    """Scene-linear to display-linear curve applied per rendering channel."""

    stage = Stage.TONE
    title = "Tone Curve"
    curve: Annotated[Curve, msgspec.Meta(title="Curve", description="Curve each rendering channel takes")] = Sigmoid()
    space: Annotated[ColorSpace, msgspec.Meta(title="Base Primaries", description="Colorspace whose primaries the rendering primaries move from")] = PyOpenColorIO.ROLE_SCENE_LINEAR
    application: Annotated[Application, msgspec.Meta(title="Application", description="Rendering primaries and hue handling around the curve")] = Application()

    @property
    @override
    def pointwise(self) -> bool:
        """Whether the selected curve uses a manual white or no frame adaptation."""
        match self.curve:
            case Logarithmic(scene_white=white) | ReinhardExtended(scene_white=white):
                return white.enabled
            case Sigmoid() | PiecewisePower():
                return True

    def base(self, config: PyOpenColorIO.Config) -> ImagingResult[npt.NDArray[np.float64]]:
        """Returns a linear RGB-to-XYZ D65 basis, or provider and unsupported-base faults."""

        def admitted(values: npt.NDArray[np.float64]) -> ImagingResult[npt.NDArray[np.float64]]:
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

        return basis(config, self.space).bind(admitted)


class Neutral(Effect, frozen=True, kw_only=True, tag=5):
    """Khronos PBR Neutral preserving base colors and compressing highlights toward white."""

    stage = Stage.TONE
    title = "PBR Neutral"
    pointwise = True
    space: ClassVar[str] = "lin_rec709_scene"
    reflectance: ClassVar[float] = 0.04
    diffuse: ClassVar[float] = 0.8
    desaturation: ClassVar[float] = 0.15


class Photoreceptor(Effect, frozen=True, kw_only=True, tag=6):
    """Reinhard and Devlin's response against an adaptation level read from the frame."""

    stage = Stage.TONE
    title = "Photoreceptor"
    pointwise = False
    space: ClassVar[str] = PyOpenColorIO.ROLE_SCENE_LINEAR
    intensity: Annotated[
        Finite, msgspec.Meta(title="Intensity", description="Brightness, scaling adaptation by e to the negative intensity"), Presentation(soft=Some((-8.0, 8.0)), decimals=Some(3))
    ] = 0.0
    contrast: Annotated[
        Gated[Positive], msgspec.Meta(title="Contrast", description="Response exponent, off deriving it from the frame's luminance range"), Presentation(soft=Some((0.3, 1.0)), decimals=Some(3))
    ] = Gated(enabled=False, value=1.0)
    light_adaptation: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Light Adaptation", description="Blend from frame adaptation at 0 to pixel adaptation at 1")] = 0.0
    chromatic_adaptation: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Chromatic Adaptation", description="Blend from luminance at 0 to each channel at 1")] = 0.0


class Hable(Effect, frozen=True, kw_only=True, tag=48):
    """White-normalized Uncharted 2 response with independent authored coefficients."""

    stage = Stage.TONE
    title = "Hable"
    space: ClassVar[str] = PyOpenColorIO.ROLE_SCENE_LINEAR
    a: Annotated[float, msgspec.Meta(ge=0.001, le=0.5, title="A", description="Shoulder strength"), Presentation(step=Some(0.01), decimals=Some(3))] = 0.15
    b: Annotated[float, msgspec.Meta(ge=0.0, le=0.6, title="B", description="Linear strength"), Presentation(step=Some(0.01), decimals=Some(3))] = 0.5
    c: Annotated[float, msgspec.Meta(ge=0.0, le=0.3, title="C", description="Linear angle"), Presentation(step=Some(0.01), decimals=Some(3))] = 0.1
    d: Annotated[float, msgspec.Meta(gt=0.0, le=0.9, title="D", description="Toe strength"), Presentation(step=Some(0.01), decimals=Some(3))] = 0.2
    e: Annotated[float, msgspec.Meta(ge=0.0001, le=0.05, title="E", description="Toe numerator"), Presentation(step=Some(0.001), decimals=Some(4))] = 0.02
    f: Annotated[float, msgspec.Meta(ge=0.2, le=0.6, title="F", description="Toe denominator"), Presentation(step=Some(0.01), decimals=Some(3))] = 0.3
    scene_white: Annotated[Gated[Stops], msgspec.Meta(title="Scene White", description="Manual white in stops from 1.0, off deriving white from the entering frame")] = Gated(
        enabled=True, value=math.log2(11.2)
    )

    @property
    @override
    def pointwise(self) -> bool:
        """Whether white normalization uses the authored manual override."""
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
    space: ClassVar[str] = "lin_rec709_scene"


# --- [OPERATIONS] -----------------------------------------------------------------------


def metered[T: Logarithmic | ReinhardExtended | Hable](curve: T, white: float) -> ImagingResult[T]:
    """Resolves a measured white to a manual override, retaining an out-of-domain white as a fault."""
    return (
        catch(lambda: msgspec.convert(math.log2(white), type=Stops), exception=msgspec.ValidationError)()
        .map(lambda stops: msgspec.structs.replace(curve, scene_white=Gated(enabled=True, value=stops)))
        .map_error(lambda _: (ImagingWhiteFault(white=Some(white)),))
        if white > 0.0
        else Error((ImagingWhiteFault(white=Some(white)),))
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
