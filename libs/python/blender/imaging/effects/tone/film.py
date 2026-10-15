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
from blender.imaging.faults import ImagingContrastFault, ImagingMatrixFault, ImagingResult
from blender.imaging.parameters import Gated, Positive, Presentation, Quantity, Rgb, Stops, Unit

# --- [TYPES] ----------------------------------------------------------------------------

type Share = Annotated[float, msgspec.Meta(gt=0.0, lt=1.0)]

# --- [MODELS] ---------------------------------------------------------------------------


class Sensitivity(msgspec.Struct, frozen=True, kw_only=True):
    """Primary's hue, reciprocal saturation, and photographic gain."""

    scale: Annotated[float, msgspec.Meta(ge=1.0, le=sys.float_info.max, description="Reciprocal HSV saturation"), Presentation(soft=Some((1.0, 2.0)))] = 1.0
    rotation: Annotated[float, msgspec.Meta(gt=-math.pi / 6.0, lt=math.pi / 6.0, description="Hue rotation about the primary"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    gain: Annotated[Positive, msgspec.Meta(description="Sensitivity of the photographic layer"), Presentation(soft=Some((0.1, 2.0)))] = 1.0


class Characteristic(msgspec.Struct, frozen=True, kw_only=True):
    """Logarithmic exposure window with a tangent-continuous toe and shoulder."""

    low: Annotated[Stops, msgspec.Meta(description="Stops from scene one to the window's lower endpoint")] = -10.0
    span: Annotated[Positive, msgspec.Meta(description="Stops between the window's endpoints"), Presentation(soft=Some((1.0, 64.0)))] = 22.0 - -10.0
    toe_x: Annotated[Share, msgspec.Meta(description="Toe exposure within the window")] = 0.44
    toe_y: Annotated[Share, msgspec.Meta(description="Toe density within the stock")] = 0.28
    across: Annotated[Share, msgspec.Meta(description="Shoulder's share of exposure above the toe")] = (0.591 - 0.44) / (1.0 - 0.44)
    up: Annotated[Share, msgspec.Meta(description="Shoulder's share of density above the toe")] = (0.779 - 0.28) / (1.0 - 0.28)


class Stock(msgspec.Struct, frozen=True, kw_only=True):
    """Photographic stock's exposure and maximum density in stops."""

    exposure: Annotated[Stops, msgspec.Meta(description="Exposure of this stock in stops")] = 6.0
    density: Annotated[Positive, msgspec.Meta(description="Maximum optical density in stops"), Presentation(soft=Some((0.1, 40.0)))] = 5.0


class Film(msgspec.Struct, frozen=True, kw_only=True):
    """Editable negative and print stocks sampled into an OCIO formation."""

    space: ClassVar[str] = "lin_rec709_scene"
    pre_exposure: Annotated[Stops, msgspec.Meta(description="Exposure ahead of photographic development")] = 4.3
    pre_filter: Annotated[Rgb[Unit], msgspec.Meta(description="Channel transmission ahead of the negative")] = Rgb(red=1.0, green=1.0, blue=1.0)
    red: Sensitivity = Sensitivity(scale=1.05, rotation=math.radians(0.5))
    green: Sensitivity = Sensitivity(scale=1.12, rotation=math.radians(2.0))
    blue: Sensitivity = Sensitivity(scale=1.045, rotation=math.radians(0.1))
    characteristic: Characteristic = Characteristic()
    negative: Stock = Stock()
    backlight: Annotated[Rgb[Positive], msgspec.Meta(description="Light illuminating the developed negative")] = Rgb(red=1.0, green=1.0, blue=1.0)
    positive: Annotated[Stock, msgspec.Meta(title="Print", description="Photographic print stock")] = Stock(density=27.5)
    black_point: Annotated[Gated[Annotated[float, msgspec.Meta(ge=-sys.float_info.max, lt=1.0)]], msgspec.Meta(description="Explicit black as a fraction of white, or the print's own black")] = Gated(
        enabled=False, value=0.0
    )
    post_filter: Annotated[Rgb[Unit], msgspec.Meta(description="Channel transmission after photographic development")] = Rgb(red=1.0, green=1.0, blue=1.0)
    saturation: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, description="HSV saturation multiplier in the midtones"), Presentation(soft=Some((0.0, 2.0)))] = 1.02
    weights: Annotated[tuple[Positive, Positive, Positive], msgspec.Meta(description="Relative channel weights for photographic brightness")] = (0.3, 0.5, 0.2)
    lattice: Lattice = Lattice(size=80, spacing=Logarithmic(low=-10.0, span=20.0))

    # --- [DEVELOPMENT]

    def developed(self, values: npt.NDArray[np.float32]) -> ImagingResult[npt.NDArray[np.float32]]:
        """Develops scene-linear Rec.709 RGB into display-linear Rec.709 RGB."""
        curve = self.characteristic
        scales, rotations, gains = np.array([(value.scale, value.rotation, value.gain) for value in (self.red, self.green, self.blue)]).T
        sectors = (np.array([5.0, 3.0, 1.0]) + 6.0 * (np.arange(3)[:, None] / 3.0 + rotations[:, None] / math.tau)) % 6.0
        primaries = 1.0 - np.clip(np.minimum(sectors, 4.0 - sectors), 0.0, 1.0) / scales[:, None]
        matrix = primaries / primaries.sum(axis=1, keepdims=True)
        log_matrix = np.log2(matrix, out=np.full_like(matrix, -np.inf), where=matrix > 0.0)
        log_weights = np.log2(self.weights)
        log_weights -= np.logaddexp2.reduce(log_weights)
        weights = np.exp2(log_weights)
        log_gains = np.log2(gains)
        pre = np.array((self.pre_filter.red, self.pre_filter.green, self.pre_filter.blue))
        post = np.array((self.post_filter.red, self.post_filter.green, self.post_filter.blue))
        backlight = np.array((self.backlight.red, self.backlight.green, self.backlight.blue))
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
            return Error((ImagingContrastFault(request=self, black=black, white=white),))
        light = np.maximum(values.astype(np.float64), 0.0)
        log_light = np.log2(light, out=np.full_like(light, -np.inf), where=light > 0.0)
        log_pre = np.log2(pre, out=np.full_like(pre, -np.inf), where=pre > 0.0)
        filtered = np.add(log_light, log_pre, out=np.full_like(light, -np.inf), where=pre > 0.0) + self.pre_exposure
        extended = np.logaddexp2.reduce(np.add(filtered[..., None, :], log_matrix, out=np.full((*light.shape[:-1], 3, 3), -np.inf), where=matrix > 0.0), axis=-1) + log_gains
        printed = density(log_backlight - self.negative.density * density(extended, self.negative), self.positive)
        log_printed = -self.positive.density * (printed - white)
        log_mono = np.logaddexp2.reduce(log_printed + log_weights, axis=-1, keepdims=True)
        if self.black_point.enabled:
            luminance = np.clip((np.exp2(log_mono) - self.black_point.value) / (1.0 - self.black_point.value), 0.0, 1.0)
            log_luminance = np.log2(luminance, out=np.full_like(luminance, -np.inf), where=luminance > 0.0)
        else:
            scale = -math.log(2.0) * self.positive.density
            rise = black - printed
            log_interval = np.log2(interval, out=np.full_like(interval, -np.inf), where=interval > 0.0)
            log_rise = np.log2(rise, out=np.full_like(rise, -np.inf), where=rise > 0.0)
            denominator = np.logaddexp2.reduce(log_interval + np.log2(exprel(scale * interval)) + log_weights)
            numerator = np.logaddexp2.reduce(log_printed + log_rise + np.log2(exprel(scale * rise)) + log_weights, axis=-1, keepdims=True)
            log_luminance = np.minimum(numerator - denominator, 0.0)
        log_right = log_printed - log_mono - log_gains + log_luminance
        log_scale = -np.maximum(log_right.max(axis=-1, keepdims=True), 0.0)
        right = np.exp2(log_right + log_scale)
        try:
            solved = np.linalg.solve(matrix, right.reshape(-1, 3).T).T.reshape(light.shape)
        except np.linalg.LinAlgError as error:
            return Error((ImagingMatrixFault(request=matrix, error=error),))
        positive = np.maximum(solved, 0.0)
        log_post = np.log2(post, out=np.full_like(post, -np.inf), where=post > 0.0)
        log_formed = np.log2(positive, out=np.full_like(positive, -np.inf), where=positive > 0.0) + log_post - log_scale
        formed = np.exp2(np.minimum(log_formed, 0.0))
        peak = formed.max(axis=-1, keepdims=True)
        chroma = peak - formed
        width = chroma.max(axis=-1, keepdims=True)
        saturated = peak - np.minimum(chroma * self.saturation, peak * np.divide(chroma, width, out=np.zeros_like(chroma), where=width > 0.0))
        blend = np.maximum(1.0 - np.abs((formed @ weights)[..., None] - 0.5) / 0.45, 0.0)
        return Ok((formed + (saturated - formed) * blend).astype(np.float32))

    # --- [TABLE]

    def transform(self) -> ImagingResult[PyOpenColorIO.GroupTransform]:
        """Builds the film's native encoded tetrahedral formation table."""
        return self.lattice.samples().bind(self.developed).map(self.lattice.transform)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Characteristic", "Film", "Sensitivity", "Stock"]
