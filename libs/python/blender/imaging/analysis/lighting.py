"""Subject lighting, spatial cues, and distinct linear-light palettes."""

from enum import auto, Enum
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
from blender.imaging.faults import ImagingResult
from blender.imaging.files.images import checked, pixels
from blender.imaging.geometry.regions import Position
from blender.records.faults import paired
from blender.records.fields import Rgb

# --- [TYPES] ----------------------------------------------------------------------------


class LightingMood(Enum):
    """Lighting class from mean luminance, contrast, and split."""

    HIGH_KEY = auto()
    NEUTRAL = auto()
    LOW_KEY = auto()


# --- [MODELS] ---------------------------------------------------------------------------


class PaletteEntry(msgspec.Struct, frozen=True, kw_only=True):
    """Cluster mean color, bottom-left position, and subject share."""

    color: Rgb
    position: Position
    share: float

    @property
    def chroma(self) -> float:
        """Channel spread over peak channel, zero for nonpositive peak."""
        channels = msgspec.structs.astuple(self.color)
        peak = max(channels)
        return (peak - min(channels)) / peak if peak > 0.0 else 0.0


class LightingReading(msgspec.Struct, frozen=True, kw_only=True):
    """Reference light colors, contrast, spatial cues, and palette."""

    key: Rgb
    fill: Rgb
    ambient: Rgb
    rim: Option[Rgb]
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

    @property
    def mood(self) -> LightingMood:
        """Mood from frame luminance, contrast, and split."""
        high, low = map(float, colour.cctf_decoding(np.array((0.45, 0.22)), function="sRGB"))
        split_threshold = 0.42
        return (
            LightingMood.HIGH_KEY
            if self.mean_luminance > high and self.contrast < math.log2(4.0)
            else LightingMood.LOW_KEY
            if self.mean_luminance < low or self.contrast > math.log2(8.0) or self.split > split_threshold
            else LightingMood.NEUTRAL
        )


class Lighting(msgspec.Struct, frozen=True, kw_only=True):
    """Lighting read settings: analysis grid, palette clusters and merge distance, split and rim thresholds, and gel chroma."""

    grid: Annotated[int, msgspec.Meta(ge=1, description="Longest analysis side in pixels")] = 256
    size: Annotated[int, msgspec.Meta(ge=1, description="Palette clusters before merging")] = 3
    separation: Annotated[float, msgspec.Meta(ge=0.0, description="Oklab distance below which palette clusters merge")] = 0.08
    split_range: Annotated[float, msgspec.Meta(gt=0.0, description="Stops of range reaching full split")] = math.log2(6.0)
    highlights: Annotated[float, msgspec.Meta(ge=0.0, le=100.0, description="Border percentile reading rim light")] = 85.0
    chromatic: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, description="Least palette chroma a gel holds")] = 0.18

    def gels(self, reading: LightingReading) -> Option[tuple[PaletteEntry, PaletteEntry]]:
        """Two largest palette entries of `reading` as key and accent when both are chromatic."""
        match tuple(entry for entry in reading.palette[:2] if entry.chroma >= self.chromatic):
            case (first, second):
                return Some((first, second) if np.dot(np.subtract(first.position, second.position), reading.direction) >= 0.0 else (second, first))
            case _:
                return Nothing

    def read(self, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32]) -> ImagingResult[Option[LightingReading]]:
        """Reads lighting of plane fitted within `grid` pixels, or absence without covered subject."""

        def measured(pair: tuple[Samples, npt.NDArray[np.float64]]) -> ImagingResult[Option[LightingReading]]:
            samples, matrix = pair
            covered = samples.finite
            height, width = covered.shape
            rows, columns = np.indices(covered.shape, dtype=np.float64)
            positions = np.stack(((columns + 0.5) / width, 1.0 - (rows + 0.5) / height), axis=-1)
            subject, central, core = (
                covered & (columns >= math.floor(inset * width)) & (columns < width - math.floor(inset * width)) & (rows >= math.floor(inset * height)) & (rows < height - math.floor(inset * height))
                for inset in (0.12, 0.25, 0.18)
            )
            if not subject.any():
                return Ok(Nothing)
            levels = np.where(covered, np.maximum(samples.luminance, Samples.floor), np.nan)
            found = levels[subject]
            count = min(self.size, found.size)
            colors, points, raw = samples.color[subject].astype(np.float64), positions[subject], samples.luminance[subject]
            coordinates = 2.0 * points - 1.0
            ordered = np.argsort(found, stable=True)
            bands = tuple(
                ordered[start : max(math.ceil(found.size * high), start + 1)]
                for low, high in ((0.85, 1.0), (0.35, 0.60), (0.0, 0.15), (0.78, 1.0), (0.0, 0.22), (0.55, 1.0), *((index / count, (index + 1) / count) for index in range(count)))
                for start in (math.floor(found.size * low),)
            )
            key, fill, ambient = (msgspec.convert(mean.tolist(), Rgb) for band in bands[:3] for mean in (colors[band].mean(axis=0, dtype=np.float64),))
            darkest, brightest, lower, upper = np.percentile(found, (5.0, 95.0, 10.0, 90.0), method="inverted_cdf")
            spread = math.log2(float(brightest / darkest))
            halves = tuple(found[mask] for mask in (coordinates[:, 0] > 0.0, coordinates[:, 0] < 0.0, coordinates[:, 1] > 0.0, coordinates[:, 1] < 0.0))
            asymmetry = np.array(
                tuple(float((positive.mean() - negative.mean()) / (positive.mean() + negative.mean())) if positive.size and negative.size else 0.0 for positive, negative in (halves[:2], halves[2:])),
                dtype=np.float64,
            )
            shadow_highlight = 1.25 * (coordinates[bands[3]].mean(axis=0) - coordinates[bands[4]].mean(axis=0))
            weights = found[bands[5]] - found[bands[5][0]]
            centroid = np.average(coordinates[bands[5]], axis=0, weights=weights) if weights.sum() > 0.0 else np.zeros(2, dtype=np.float64)
            gradients = np.stack((np.gradient(levels, axis=1) if width > 1 else np.zeros_like(levels), -np.gradient(levels, axis=0) if height > 1 else np.zeros_like(levels)), axis=-1)
            slopes = gradients[subject & np.isfinite(gradients).all(axis=-1)]
            magnitudes = np.linalg.norm(slopes, axis=-1)
            energy = np.square(magnitudes).sum(dtype=np.float64)
            gradient = (slopes * magnitudes[:, None]).sum(axis=0, dtype=np.float64) / energy if energy > 0.0 else np.zeros(2, dtype=np.float64)
            cues = np.stack((asymmetry, shadow_highlight, centroid, gradient))
            cues /= np.maximum(1.0, np.linalg.norm(cues, axis=-1))[:, None]
            direction = np.array((0.42, 0.28, 0.18, 0.12), dtype=np.float64) @ cues
            derivative = float(np.abs(gradients[subject[..., None] & np.isfinite(gradients)]).max(initial=0.0))
            variation = derivative + float(upper - lower)
            hardness = derivative / variation if variation > 0.0 else 0.0
            split = float(np.linalg.norm(cues[0])) * min(1.0, spread / self.split_range)
            border = covered & ~central
            rim = (
                Some(levels[border])
                .filter(lambda held: held.size > 0)
                .map(lambda held: samples.color[border & (levels >= np.percentile(held, self.highlights, method="inverted_cdf"))].mean(axis=0, dtype=np.float64))
                .map(lambda mean: msgspec.convert(mean.tolist(), Rgb))
            )
            center_levels, border_levels, ring_levels = levels[central], levels[border], levels[subject & ~core]
            frame_mean, subject_mean = float(levels[covered].mean(dtype=np.float64)), float(found.mean(dtype=np.float64))
            center_mean, border_mean, ring_mean = (
                Some(group).filter(lambda held: held.size > 0).map(lambda held: float(held.mean(dtype=np.float64))) for group in (center_levels, border_levels, ring_levels)
            )
            backlight = border_mean.map2(lambda outside, inside: math.log2(outside / inside), center_mean)
            backlit = border_mean.map2(lambda outside, inside: outside > 1.35 * inside and inside <= 1.08 * frame_mean, center_mean).default_value(False) or ring_mean.map(
                lambda ring: ring > 1.25 * subject_mean and subject_mean < 1.05 * frame_mean
            ).default_value(False)
            centers, _ = kmeans(colors, np.stack(tuple(colors[band].mean(axis=0) for band in bands[6:])), thresh=0.0, check_finite=False)
            labels, _ = vq(colors, centers, check_finite=False)
            counts = np.bincount(labels, minlength=centers.shape[0])
            means = np.asarray(binned_statistic(labels, np.column_stack((colors, points)).T, statistic="mean", bins=np.arange(centers.shape[0] + 1) - 0.5).statistic.T, dtype=np.float64)
            ordered_clusters = np.argsort(-counts, stable=True)
            ordered_clusters = ordered_clusters[counts[ordered_clusters] > 0]
            key_y, fill_y = (raw[band].mean(dtype=np.float64) for band in bands[:2])

            tristimulus = means[ordered_clusters, :3] @ matrix.T
            tristimulus /= np.maximum(tristimulus[:, 1:2], Samples.floor)
            lab = colour.XYZ_to_Oklab(tristimulus)
            distances = cdist(lab, lab)
            shares = counts[ordered_clusters].astype(np.float64) / found.size
            kept = [0]
            for index in range(1, ordered_clusters.size):
                nearest = kept[int(np.argmin(distances[index, kept]))]
                if distances[index, nearest] < self.separation:
                    shares[nearest] += shares[index]
                else:
                    kept.append(index)
            palette = tuple(
                PaletteEntry(color=msgspec.convert(mean[:3].tolist(), Rgb), position=(float(mean[3]), float(mean[4])), share=float(shares[index]))
                for index in kept
                for mean in (means[ordered_clusters[index]],)
            )
            return White.of(config, key).map(
                lambda white: Some(
                    LightingReading(
                        key=key,
                        fill=fill,
                        ambient=ambient,
                        rim=rim,
                        contrast=math.log2(float(max(key_y, Samples.floor) / max(fill_y, Samples.floor))),
                        range=spread,
                        metered_exposure=math.log2(GREY) - float(Samples.logarithm(found).mean(dtype=np.float64)),
                        mean_luminance=frame_mean,
                        direction=(float(direction[0]), float(direction[1])),
                        split=split,
                        backlight=backlight,
                        backlit=backlit,
                        hardness=hardness,
                        key_white=white,
                        palette=palette,
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
