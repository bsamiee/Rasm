"""Negative and print-stock development into a display-linear film formation."""

import math
import sys
from typing import Annotated, ClassVar

from expression import Error, Option
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO
from scipy.special import exprel

from blender.imaging.color.luts import Lattice, Logarithmic
from blender.imaging.color.processors import REC709_SCENE, Stops
from blender.records.faults import Fault, Outcome, RecordReason
from blender.records.fields import AtLeastOne, NonNegative, Positive, Unit

# --- [TYPES] ----------------------------------------------------------------------------

type Share = Annotated[float, msgspec.Meta(ge=sys.float_info.min, lt=1.0)]

# --- [MODELS] ---------------------------------------------------------------------------


class Sensitivity(msgspec.Struct, frozen=True, kw_only=True):
    """Primary's hue, reciprocal saturation, and photographic gain."""

    scale: AtLeastOne = 1.0
    rotation: Annotated[float, msgspec.Meta(gt=-math.pi / 6.0, lt=math.pi / 6.0)] = 0.0
    gain: Positive = 1.0


class Characteristic(msgspec.Struct, frozen=True, kw_only=True):
    """Logarithmic exposure window with a tangent-continuous toe and shoulder."""

    low: Stops = -10.0
    span: Positive = 22.0 - -10.0
    toe_x: Share = 0.44
    toe_y: Share = 0.28
    across: Share = (0.591 - 0.44) / (1.0 - 0.44)
    up: Share = (0.779 - 0.28) / (1.0 - 0.28)


class Stock(msgspec.Struct, frozen=True, kw_only=True):
    """Photographic stock's exposure and maximum density in stops."""

    exposure: Stops = 6.0
    density: Positive = 5.0


class Film(msgspec.Struct, frozen=True, kw_only=True):
    """Editable negative and print stocks sampled into an OCIO formation."""

    space: ClassVar[str] = REC709_SCENE
    pre_exposure: Stops = 4.3
    pre_filter: tuple[Unit, Unit, Unit] = (1.0, 1.0, 1.0)
    sensitivities: tuple[Sensitivity, Sensitivity, Sensitivity] = (
        Sensitivity(scale=1.05, rotation=math.radians(0.5)),
        Sensitivity(scale=1.12, rotation=math.radians(2.0)),
        Sensitivity(scale=1.045, rotation=math.radians(0.1)),
    )
    characteristic: Characteristic = Characteristic()
    negative: Stock = Stock()
    backlight: tuple[Positive, Positive, Positive] = (1.0, 1.0, 1.0)
    positive: Stock = Stock(density=27.5)
    black_point: Annotated[float, msgspec.Meta(ge=-sys.float_info.max, lt=1.0)] | None = None
    post_filter: tuple[Unit, Unit, Unit] = (1.0, 1.0, 1.0)
    saturation: NonNegative = 1.02
    weights: tuple[Positive, Positive, Positive] = (0.3, 0.5, 0.2)
    lattice: Lattice = Lattice(size=80, spacing=Logarithmic())

    # --- [DEVELOPMENT]

    def developed(self, values: npt.NDArray[np.float32]) -> Outcome[npt.NDArray[np.float32]]:
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
            return np.asarray(np.exp(-np.exp(exponent, out=np.full_like(exponent, np.inf), where=exponent < log_maximum)), dtype=np.float64)

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
        if (floor := Option.of_optional(self.black_point)).is_none() and not np.any(interval):
            return Error((Fault(reason=RecordReason.OUTSIDE, subject=type(self).__name__, member="black and white print densities that differ", cause=str((black, white))),))
        light = np.maximum(values.astype(np.float64), 0.0)
        log_light = logged(light)
        log_pre = logged(pre)
        filtered = np.add(log_light, log_pre, out=np.full_like(light, -np.inf), where=pre > 0.0) + self.pre_exposure
        extended = np.logaddexp2.reduce(np.add(filtered[..., None, :], log_matrix, out=np.full((*light.shape[:-1], 3, 3), -np.inf), where=matrix > 0.0), axis=-1) + log_gains
        printed = density(log_backlight - self.negative.density * density(extended, self.negative), self.positive)
        log_printed = -self.positive.density * (printed - white)
        log_mono = np.logaddexp2.reduce(log_printed + log_weights, axis=-1, keepdims=True)
        match floor:
            case Option(tag="some", some=level):
                log_luminance = logged(np.clip((np.exp2(log_mono) - level) / (1.0 - level), 0.0, 1.0))
            case _:
                scale = -math.log(2.0) * self.positive.density
                rise = black - printed
                denominator = np.logaddexp2.reduce(logged(interval) + np.log2(exprel(scale * interval)) + log_weights)
                numerator = np.logaddexp2.reduce(log_printed + logged(rise) + np.log2(exprel(scale * rise)) + log_weights, axis=-1, keepdims=True)
                log_luminance = np.minimum(numerator - denominator, 0.0)
        log_right = log_printed - log_mono - log_gains + log_luminance
        log_scale = -np.maximum(log_right.max(axis=-1, keepdims=True), 0.0)
        right = np.exp2(log_right + log_scale)

        def finished(solved: npt.NDArray[np.inexact]) -> npt.NDArray[np.float32]:
            formed: npt.NDArray[np.float64] = np.exp2(np.minimum(logged(np.maximum(solved.T.reshape(light.shape), 0.0)) + logged(post) - log_scale, 0.0))
            peak = formed.max(axis=-1, keepdims=True)
            chroma = peak - formed
            width = chroma.max(axis=-1, keepdims=True)
            saturated = peak - np.minimum(chroma * self.saturation, peak * np.divide(chroma, width, out=np.zeros_like(chroma), where=width > 0.0))
            blend = np.maximum(1.0 - np.abs((formed @ weights)[..., None] - 0.5) / 0.45, 0.0)
            return np.asarray(formed + (saturated - formed) * blend, dtype=np.float32)

        return (
            catch(np.linalg.solve, exception=np.linalg.LinAlgError)(matrix, right.reshape(-1, 3).T)
            .map(finished)
            .map_error(lambda error: (Fault(reason=RecordReason.NUMERIC, subject=str(matrix), cause=error),))
        )

    # --- [TABLE]

    def transform(self) -> Outcome[PyOpenColorIO.GroupTransform]:
        """Builds native encoded tetrahedral formation table."""
        return self.lattice.samples().bind(self.developed).map(self.lattice.transform)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Characteristic", "Film", "Sensitivity", "Stock"]
