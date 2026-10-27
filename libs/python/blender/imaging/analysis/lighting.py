"""Subject lighting, spatial cues, and distinct linear-light palettes."""

from enum import auto, Enum
import functools
import math
from typing import Annotated

import colour
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO
from scipy.cluster.vq import kmeans, vq
from scipy.spatial.distance import cdist
from scipy.stats import binned_statistic

from blender.imaging.analysis.metering import Samples
from blender.imaging.color.processors import basis, GREY
from blender.imaging.color.white import White
from blender.imaging.files.buffers import checked, pixels
from blender.records.faults import Outcome, paired
from blender.records.fields import Frozen, NonNegative, Positive, Side, Unit

# --- [TYPES] ----------------------------------------------------------------------------

type _Inset = Annotated[float, msgspec.Meta(ge=0.0, lt=0.5)]


class LightingMood(Enum):
    """Lighting class from mean luminance, contrast, and split."""

    HIGH_KEY = auto()
    NEUTRAL = auto()
    LOW_KEY = auto()


# --- [MODELS] ---------------------------------------------------------------------------


class PaletteEntry(Frozen, frozen=True, kw_only=True, eq=False):
    """Cluster mean color of shape `(3,)`, bottom-left position, and subject share."""

    color: npt.NDArray[np.float64]
    position: tuple[float, float]
    share: float

    @property
    def chroma(self) -> float:
        """Channel spread over peak channel, zero for nonpositive peak."""
        peak = float(self.color.max())
        return (peak - float(self.color.min())) / peak if peak > 0.0 else 0.0


class LightingReading(Frozen, frozen=True, kw_only=True, eq=False):
    """Reference light colors of shape `(3,)`, contrast, spatial cues, and palette."""

    key: npt.NDArray[np.float64]
    fill: npt.NDArray[np.float64]
    ambient: npt.NDArray[np.float64]
    rim: Option[npt.NDArray[np.float64]]
    contrast: float
    range: float
    metered_exposure: float
    mean_luminance: float
    direction: tuple[float, float]
    split: float
    backlight: Option[float]
    backlit: bool
    hardness: float
    key_white: Option[White]
    palette: tuple[PaletteEntry, ...]


class Lighting(msgspec.Struct, frozen=True, kw_only=True):
    """Lighting read settings: analysis grid, palette clusters and merge distance, split and rim thresholds, gel chroma, subject insets, and mood thresholds."""

    grid: Side = 256
    size: Side = 3
    separation: Annotated[float, msgspec.Meta(ge=0.0)] = 0.08
    split_range: Positive = math.log2(6.0)
    highlights: Annotated[float, msgspec.Meta(ge=0.0, le=100.0)] = 85.0
    chromatic: Annotated[float, msgspec.Meta(ge=0.0, le=1.0)] = 0.18
    subject_inset: _Inset = 0.12
    center_inset: _Inset = 0.25
    core_inset: _Inset = 0.18
    high_key_luminance: NonNegative = float(colour.cctf_decoding(0.45, function="sRGB"))
    low_key_luminance: NonNegative = float(colour.cctf_decoding(0.22, function="sRGB"))
    high_key_contrast: NonNegative = 2.0
    low_key_contrast: NonNegative = 3.0
    low_key_split: Unit = 0.42

    def gels(self, reading: LightingReading) -> Option[tuple[PaletteEntry, PaletteEntry]]:
        """Two largest palette entries of `reading` as key and accent when both are chromatic."""
        match tuple(entry for entry in reading.palette[:2] if entry.chroma >= self.chromatic):
            case (first, second):
                return Some((first, second) if np.dot(np.subtract(first.position, second.position), reading.direction) >= 0.0 else (second, first))
            case _:
                return Nothing

    def mood(self, reading: LightingReading) -> LightingMood:
        """Mood of `reading` from frame luminance, key-to-fill contrast in stops, and split."""
        return (
            LightingMood.HIGH_KEY
            if reading.mean_luminance > self.high_key_luminance and reading.contrast < self.high_key_contrast
            else LightingMood.LOW_KEY
            if reading.mean_luminance < self.low_key_luminance or reading.contrast > self.low_key_contrast or reading.split > self.low_key_split
            else LightingMood.NEUTRAL
        )

    def read(self, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32]) -> Outcome[Option[LightingReading]]:
        """Reads lighting of plane fitted within `grid` pixels, or absence without covered subject inside `subject_inset`."""

        def measured(pair: tuple[Samples, npt.NDArray[np.float64]]) -> Outcome[Option[LightingReading]]:
            samples, matrix = pair
            covered = samples.finite
            height, width = covered.shape
            rows, columns = np.indices(covered.shape, dtype=np.float64)
            subject, central, core = (
                covered & (columns >= across) & (columns < width - across) & (rows >= down) & (rows < height - down)
                for across, down in ((math.floor(inset * width), math.floor(inset * height)) for inset in (self.subject_inset, self.center_inset, self.core_inset))
            )
            if not subject.any():
                return Ok(Nothing)
            levels = np.where(covered, np.maximum(samples.luminance, Samples.floor), np.nan)
            found = levels[subject]
            colors, raw = samples.color[subject].astype(np.float64), samples.luminance[subject]
            points = np.stack(((columns + 0.5) / width, 1.0 - (rows + 0.5) / height), axis=-1)[subject]
            coordinates = 2.0 * points - 1.0
            ordered, count = np.argsort(found, stable=True), min(self.size, found.size)

            def band(low: float, high: float) -> npt.NDArray[np.intp]:
                start = math.floor(found.size * low)
                return ordered[start : max(math.ceil(found.size * high), start + 1)]

            lit, filled, weighted = band(0.85, 1.0), band(0.35, 0.60), band(0.55, 1.0)
            key, fill, ambient = (colors[held].mean(axis=0, dtype=np.float64) for held in (lit, filled, band(0.0, 0.15)))
            key_y, fill_y = (raw[held].mean(dtype=np.float64) for held in (lit, filled))
            darkest, brightest, lower, upper = np.percentile(found, (5.0, 95.0, 10.0, 90.0), method="inverted_cdf")
            spread = math.log2(float(brightest / darkest))
            asymmetry = tuple(
                float((positive.mean() - negative.mean()) / (positive.mean() + negative.mean())) if positive.size and negative.size else 0.0
                for positive, negative in ((found[coordinates[:, axis] > 0.0], found[coordinates[:, axis] < 0.0]) for axis in (0, 1))
            )
            weights = found[weighted] - found[weighted[0]]
            centroid = np.average(coordinates[weighted], axis=0, weights=weights) if weights.sum() > 0.0 else np.zeros(2, dtype=np.float64)
            gradients = np.stack((np.gradient(levels, axis=1) if width > 1 else np.zeros_like(levels), -np.gradient(levels, axis=0) if height > 1 else np.zeros_like(levels)), axis=-1)
            slopes = gradients[subject & np.isfinite(gradients).all(axis=-1)]
            magnitudes = np.linalg.norm(slopes, axis=-1)
            energy = np.square(magnitudes).sum(dtype=np.float64)
            gradient = (slopes * magnitudes[:, None]).sum(axis=0, dtype=np.float64) / energy if energy > 0.0 else np.zeros(2, dtype=np.float64)
            stacked = np.stack((asymmetry, 1.25 * (coordinates[band(0.78, 1.0)].mean(axis=0) - coordinates[band(0.0, 0.22)].mean(axis=0)), centroid, gradient), dtype=np.float64)
            cues = stacked / np.maximum(1.0, np.linalg.norm(stacked, axis=-1, keepdims=True))
            direction = np.array((0.42, 0.28, 0.18, 0.12), dtype=np.float64) @ cues
            derivative = float(np.abs(gradients[subject[..., None] & np.isfinite(gradients)]).max(initial=0.0))
            variation = derivative + float(upper - lower)
            border = covered & ~central
            frame_mean, subject_mean = float(levels[covered].mean(dtype=np.float64)), float(found.mean(dtype=np.float64))
            center_mean, border_mean, ring_mean = (
                Some(levels[group]).filter(lambda held: held.size > 0).map(lambda held: float(held.mean(dtype=np.float64))) for group in (central, border, subject & ~core)
            )
            centers, _ = kmeans(colors, np.stack(tuple(colors[band(index / count, (index + 1) / count)].mean(axis=0) for index in range(count))), thresh=0.0, check_finite=False)
            labels, _ = vq(colors, centers, check_finite=False)
            counts = np.bincount(labels, minlength=centers.shape[0])
            means = np.asarray(binned_statistic(labels, np.column_stack((colors, points)).T, statistic="mean", bins=np.arange(centers.shape[0] + 1) - 0.5).statistic.T, dtype=np.float64)
            clusters = np.argsort(-counts, stable=True)
            clusters = clusters[counts[clusters] > 0]
            tristimulus = means[clusters, :3] @ matrix.T
            lab = colour.XYZ_to_Oklab(tristimulus / np.maximum(tristimulus[:, 1:2], Samples.floor))
            distances = cdist(lab, lab)

            def merged(state: tuple[tuple[int, ...], npt.NDArray[np.float64]], index: int) -> tuple[tuple[int, ...], npt.NDArray[np.float64]]:
                kept, shares = state
                nearest = kept[int(np.argmin(distances[index, list(kept)]))]
                return (kept, np.where(np.arange(shares.size) == nearest, shares + shares[index], shares)) if distances[index, nearest] < self.separation else ((*kept, index), shares)

            first: tuple[tuple[int, ...], npt.NDArray[np.float64]] = ((0,), counts[clusters].astype(np.float64) / found.size)
            kept, shares = functools.reduce(merged, range(1, clusters.size), first)
            return White.of(config, key).map(
                lambda white: Some(
                    LightingReading(
                        key=key,
                        fill=fill,
                        ambient=ambient,
                        rim=Some(levels[border])
                        .filter(lambda held: held.size > 0)
                        .map(lambda held: samples.color[border & (levels >= np.percentile(held, self.highlights, method="inverted_cdf"))].mean(axis=0, dtype=np.float64)),
                        contrast=math.log2(max(key_y, Samples.floor) / max(fill_y, Samples.floor)),
                        range=spread,
                        metered_exposure=math.log2(GREY) - float(Samples.logarithm(found).mean(dtype=np.float64)),
                        mean_luminance=frame_mean,
                        direction=(float(direction[0]), float(direction[1])),
                        split=float(np.linalg.norm(cues[0])) * min(1.0, spread / self.split_range),
                        backlight=border_mean.map2(lambda outside, inner: math.log2(outside / inner), center_mean),
                        backlit=border_mean.map2(lambda outside, inner: outside > 1.35 * inner and inner <= 1.08 * frame_mean, center_mean).default_value(False)
                        or ring_mean.map(lambda ring: ring > 1.25 * subject_mean and subject_mean < 1.05 * frame_mean).default_value(False),
                        hardness=derivative / variation if variation > 0.0 else 0.0,
                        key_white=white,
                        palette=tuple(
                            PaletteEntry(color=means[clusters[index], :3], position=(float(means[clusters[index], 3]), float(means[clusters[index], 4])), share=float(shares[index])) for index in kept
                        ),
                    )
                )
            )

        return (
            checked(OpenImageIO.ImageBufAlgo.fit(OpenImageIO.ImageBuf(plane), filtername="triangle", roi=OpenImageIO.ROI(0, min(self.grid, plane.shape[1]), 0, min(self.grid, plane.shape[0]))))
            .bind(pixels)
            .bind(lambda values: paired(Samples.of(config, values), basis(config, PyOpenColorIO.ROLE_SCENE_LINEAR)))
            .bind(measured)
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Lighting", "LightingMood", "LightingReading", "PaletteEntry"]
