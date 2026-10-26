"""Negative and print-stock development into a display-linear film formation."""

import math
import sys
from typing import Annotated, ClassVar

from expression import Error, Ok, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO
from scipy.special import exprel

from blender.imaging.color.luts import Lattice, Logarithmic
from blender.imaging.color.processors import REC709_SCENE, Stops
from blender.imaging.faults import ImagingResult
from blender.records.faults import DomainFault, NumericFault
from blender.records.fields import AtLeastOne, Gated, NonNegative, Positive, Presentation, Quantity, Rgb, Unit

# --- [TYPES] ----------------------------------------------------------------------------

type Share = Annotated[float, msgspec.Meta(gt=0.0, lt=1.0)]

# --- [MODELS] ---------------------------------------------------------------------------


class Sensitivity(msgspec.Struct, frozen=True, kw_only=True):
    """Primary's hue, reciprocal saturation, and photographic gain."""

    scale: Annotated[AtLeastOne, msgspec.Meta(description="Reciprocal HSV saturation"), Presentation(soft=Some((1.0, 2.0)))] = 1.0
    rotation: Annotated[float, msgspec.Meta(gt=-math.pi / 6.0, lt=math.pi / 6.0, description="Hue turn about primary"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    gain: Annotated[Positive, msgspec.Meta(description="Photographic layer sensitivity"), Presentation(soft=Some((0.1, 2.0)))] = 1.0


class Characteristic(msgspec.Struct, frozen=True, kw_only=True):
    """Logarithmic exposure window with a tangent-continuous toe and shoulder."""

    low: Annotated[Stops, msgspec.Meta(description="Stops from scene one to window lower endpoint")] = -10.0
    span: Annotated[Positive, msgspec.Meta(description="Stops between window endpoints"), Presentation(soft=Some((1.0, 64.0)))] = 22.0 - -10.0
    toe_x: Annotated[Share, msgspec.Meta(description="Toe exposure within window")] = 0.44
    toe_y: Annotated[Share, msgspec.Meta(description="Toe density within stock")] = 0.28
    across: Annotated[Share, msgspec.Meta(description="Shoulder share of exposure above toe")] = (0.591 - 0.44) / (1.0 - 0.44)
    up: Annotated[Share, msgspec.Meta(description="Shoulder share of density above toe")] = (0.779 - 0.28) / (1.0 - 0.28)


class Stock(msgspec.Struct, frozen=True, kw_only=True):
    """Photographic stock's exposure and maximum density in stops."""

    exposure: Annotated[Stops, msgspec.Meta(description="Stock exposure in stops")] = 6.0
    density: Annotated[Positive, msgspec.Meta(description="Maximum optical density in stops"), Presentation(soft=Some((0.1, 40.0)))] = 5.0


class Film(msgspec.Struct, frozen=True, kw_only=True):
    """Editable negative and print stocks sampled into an OCIO formation."""

    space: ClassVar[str] = REC709_SCENE
    pre_exposure: Annotated[Stops, msgspec.Meta(description="Exposure before development")] = 4.3
    pre_filter: Annotated[Rgb[Unit], msgspec.Meta(description="Channel transmission before negative")] = Rgb(red=1.0, green=1.0, blue=1.0)
    sensitivities: Annotated[tuple[Sensitivity, Sensitivity, Sensitivity], msgspec.Meta(description="Red, green, and blue layer sensitivities")] = (
        Sensitivity(scale=1.05, rotation=math.radians(0.5)),
        Sensitivity(scale=1.12, rotation=math.radians(2.0)),
        Sensitivity(scale=1.045, rotation=math.radians(0.1)),
    )
    characteristic: Characteristic = Characteristic()
    negative: Stock = Stock()
    backlight: Annotated[Rgb[Positive], msgspec.Meta(description="Light behind developed negative")] = Rgb(red=1.0, green=1.0, blue=1.0)
    positive: Annotated[Stock, msgspec.Meta(title="Print", description="Photographic print stock")] = Stock(density=27.5)
    black_point: Annotated[Gated[Annotated[float, msgspec.Meta(ge=-sys.float_info.max, lt=1.0)]], msgspec.Meta(description="Black as a share of white, print black when off")] = Gated(
        enabled=False, value=0.0
    )
    post_filter: Annotated[Rgb[Unit], msgspec.Meta(description="Channel transmission after development")] = Rgb(red=1.0, green=1.0, blue=1.0)
    saturation: Annotated[NonNegative, msgspec.Meta(description="HSV saturation multiplier in midtones"), Presentation(soft=Some((0.0, 2.0)))] = 1.02
    weights: Annotated[tuple[Positive, Positive, Positive], msgspec.Meta(description="Channel weights of photographic brightness")] = (0.3, 0.5, 0.2)
    lattice: Lattice = Lattice(size=80, spacing=Logarithmic())

    # --- [DEVELOPMENT]

    def developed(self, values: npt.NDArray[np.float32]) -> ImagingResult[npt.NDArray[np.float32]]:
        """Develops scene-linear Rec.709 RGB into display-linear Rec.709 RGB."""
        curve = self.characteristic

        def logged(value: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
            return np.log2(value, out=np.full_like(value, -np.inf), where=value > 0.0)

        scales, rotations, gains = np.array([(value.scale, value.rotation, value.gain) for value in self.sensitivities]).T
        sectors = (np.array([5.0, 3.0, 1.0]) + 6.0 * (np.arange(3)[:, None] / 3.0 + rotations[:, None] / math.tau)) % 6.0
        primaries = 1.0 - np.clip(np.minimum(sectors, 4.0 - sectors), 0.0, 1.0) / scales[:, None]
        matrix = primaries / primaries.sum(axis=1, keepdims=True)
        log_matrix = logged(matrix)
        log_weights = np.log2(self.weights)
        log_weights -= np.logaddexp2.reduce(log_weights)
        weights = np.exp2(log_weights)
        log_gains = np.log2(gains)
        pre = np.array(self.pre_filter)
        post = np.array(self.post_filter)
        backlight = np.array(self.backlight)
        log_backlight = np.logaddexp2.reduce(log_matrix + np.log2(backlight), axis=-1) + log_gains
        log_toe = math.log(curve.up) - math.log(curve.across) + math.log(curve.toe_x) - math.log(curve.toe_y) + math.log1p(-curve.toe_y) - math.log1p(-curve.toe_x)
        log_shoulder = math.log(curve.up) - math.log(curve.across) + math.log1p(-curve.across) - math.log1p(-curve.up)
        log_maximum = math.log(sys.float_info.max)

        def powered(ratio: npt.NDArray[np.float64], logarithm: float) -> npt.NDArray[np.float64]:
            """Evaluates a power from its exponent's logarithm, including endpoint limits."""
            magnitude = -np.log(ratio, out=np.full_like(ratio, -np.inf), where=ratio > 0.0)
            exponent = logarithm + np.log(magnitude, out=np.full_like(magnitude, -np.inf), where=magnitude > 0.0)
            return np.exp(-np.exp(exponent, out=np.full_like(exponent, np.inf), where=exponent < log_maximum))

        def density(light: npt.NDArray[np.float64], stock: Stock) -> npt.NDArray[np.float64]:
            """Develops logarithmic light into the shared characteristic's density fraction."""
            u = np.clip(np.logaddexp2(light + stock.exposure, curve.low) - curve.low, 0.0, curve.span) / curve.span
            relative = (u - curve.toe_x) / (1.0 - curve.toe_x)
            toe = powered(np.divide(u, curve.toe_x, out=np.ones_like(u), where=u < curve.toe_x), log_toe)
            shoulder = powered(np.divide(1.0 - relative, 1.0 - curve.across, out=np.ones_like(u), where=relative >= curve.across), log_shoulder)
            line = np.divide(relative, curve.across, out=np.ones_like(u), where=(u >= curve.toe_x) & (relative < curve.across))
            return np.select(
                (u < curve.toe_x, relative < curve.across),
                (curve.toe_y * toe, curve.toe_y + (1.0 - curve.toe_y) * curve.up * line),
                default=curve.toe_y + (1.0 - curve.toe_y) * (curve.up + (1.0 - curve.up) * (1.0 - shoulder)),
            )

        black = density(log_backlight, self.positive)
        white = density(log_backlight - self.negative.density, self.positive)
        interval = black - white
        if not self.black_point.enabled and not np.any(interval):
            return Error((DomainFault(subject=type(self).__name__, value=(black, white), accepted="black and white print densities that differ"),))
        light = np.maximum(values.astype(np.float64), 0.0)
        log_light = logged(light)
        log_pre = logged(pre)
        filtered = np.add(log_light, log_pre, out=np.full_like(light, -np.inf), where=pre > 0.0) + self.pre_exposure
        extended = np.logaddexp2.reduce(np.add(filtered[..., None, :], log_matrix, out=np.full((*light.shape[:-1], 3, 3), -np.inf), where=matrix > 0.0), axis=-1) + log_gains
        printed = density(log_backlight - self.negative.density * density(extended, self.negative), self.positive)
        log_printed = -self.positive.density * (printed - white)
        log_mono = np.logaddexp2.reduce(log_printed + log_weights, axis=-1, keepdims=True)
        if self.black_point.enabled:
            luminance = np.clip((np.exp2(log_mono) - self.black_point.value) / (1.0 - self.black_point.value), 0.0, 1.0)
            log_luminance = logged(luminance)
        else:
            scale = -math.log(2.0) * self.positive.density
            rise = black - printed
            log_interval = logged(interval)
            log_rise = logged(rise)
            denominator = np.logaddexp2.reduce(log_interval + np.log2(exprel(scale * interval)) + log_weights)
            numerator = np.logaddexp2.reduce(log_printed + log_rise + np.log2(exprel(scale * rise)) + log_weights, axis=-1, keepdims=True)
            log_luminance = np.minimum(numerator - denominator, 0.0)
        log_right = log_printed - log_mono - log_gains + log_luminance
        log_scale = -np.maximum(log_right.max(axis=-1, keepdims=True), 0.0)
        right = np.exp2(log_right + log_scale)
        try:
            solved = np.linalg.solve(matrix, right.reshape(-1, 3).T).T.reshape(light.shape)
        except np.linalg.LinAlgError as error:
            return Error((NumericFault(request=matrix, error=error),))
        positive = np.maximum(solved, 0.0)
        log_post = logged(post)
        log_formed = logged(positive) + log_post - log_scale
        formed = np.exp2(np.minimum(log_formed, 0.0))
        peak = formed.max(axis=-1, keepdims=True)
        chroma = peak - formed
        width = chroma.max(axis=-1, keepdims=True)
        saturated = peak - np.minimum(chroma * self.saturation, peak * np.divide(chroma, width, out=np.zeros_like(chroma), where=width > 0.0))
        blend = np.maximum(1.0 - np.abs((formed @ weights)[..., None] - 0.5) / 0.45, 0.0)
        return Ok((formed + (saturated - formed) * blend).astype(np.float32))

    # --- [TABLE]

    def transform(self) -> ImagingResult[PyOpenColorIO.GroupTransform]:
        """Builds native encoded tetrahedral formation table."""
        return self.lattice.samples().bind(self.developed).map(self.lattice.transform)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Characteristic", "Film", "Sensitivity", "Stock"]
