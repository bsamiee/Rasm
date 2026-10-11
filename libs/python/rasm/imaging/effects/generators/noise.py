"""Procedural light and haze: noise, cellular, and wave fields emitting, absorbing, and scattering light."""

from collections.abc import Callable
from fractions import Fraction
import hashlib
import itertools
import math
from sys import float_info
from typing import Annotated, Final

import cbor2
from colour.colorimetry import planck_law
from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO
from scipy.special import logsumexp
from scipy.stats import qmc

from rasm.imaging.chain import Effect, Stage
from rasm.imaging.color.observer import table
from rasm.imaging.color.processors import basis as color_basis
from rasm.imaging.faults import accumulated, ImagingGeneratorFault, ImagingMatrixFault, ImagingResult
from rasm.imaging.parameters import Case, Choice, Finite, Gated, GraphForm, Motion, Position, Presentation, Quantity, Ramp, Rgb, Seed, Stops

# --- [TYPES] ------------------------------------------------------------------------------


class Fractal(Choice):
    """Sum combining the octaves of a noise texture."""

    FBM = 1, "fBM"
    MULTIFRACTAL = 2, "Multifractal"
    RIDGED_MULTIFRACTAL = 3, "Ridged Multifractal"
    HYBRID_MULTIFRACTAL = 4, "Hybrid Multifractal"
    HETERO_TERRAIN = 5, "Hetero Terrain"


class Feature(Choice):
    """Distance the Voronoi cells answer."""

    F1 = 1, "F1"
    F2 = 2, "F2"
    SMOOTH_F1 = 3, "Smooth F1"
    DISTANCE_TO_EDGE = 4, "Distance to Edge"


class Metric(Choice):
    """Distance measure between a position and a cell point."""

    EUCLIDEAN = 1, "Euclidean"
    MANHATTAN = 2, "Manhattan"
    CHEBYCHEV = 3, "Chebychev"
    MINKOWSKI = 4, "Minkowski"


class Pattern(Choice):
    """Bands across the field or rings about its center."""

    BANDS = 1, "Bands"
    RINGS = 2, "Rings"


class Profile(Choice):
    """Shape of each wave period."""

    SIN = 1, "Sine"
    SAW = 2, "Saw"
    TRI = 3, "Triangle"
    SQUARE = 4, "Square"


class Interpolation(Choice):
    """Curve mapping a texture interval to density."""

    LINEAR = 1, "Linear"
    SMOOTHSTEP = 2, "Smooth Step"
    SMOOTHERSTEP = 3, "Smoother Step"


type Basis = Perlin | Voronoi | Wave | Gabor | Magic | White
type Spectrum = Tint | Incandescence | Gradient


# --- [CONSTANTS] --------------------------------------------------------------------------


_FIRE_SHORT_SIDE: Final = 9.0 / 16.0


# --- [MODELS] -----------------------------------------------------------------------------


class Cut(msgspec.Struct, frozen=True, kw_only=True):
    """Soft threshold mapping a texture value to density."""

    threshold: Annotated[Finite, msgspec.Meta(title="Threshold", description="Texture value where density starts rising"), Presentation(soft=Some((0.0, 1.0)))] = 0.0
    softness: Annotated[float, msgspec.Meta(gt=0.0, le=float_info.max, title="Softness", description="Texture span from the threshold to full density"), Presentation(soft=Some((0.001, 1.0)))] = 1.0
    invert: Annotated[bool, msgspec.Meta(title="Invert", description="Density falls across the span in place of rising")] = False
    interpolation: Annotated[Interpolation, msgspec.Meta(title="Interpolation", description="Curve across the density interval"), GraphForm.SETTING] = Interpolation.SMOOTHSTEP

    def applied(self, values: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Maps texture values to the declared bounded density curve."""
        value = np.clip((values - self.threshold) / self.softness, 0.0, 1.0)
        match self.interpolation:
            case Interpolation.SMOOTHSTEP:
                value = value * value * (3.0 - 2.0 * value)
            case Interpolation.SMOOTHERSTEP:
                value = value**3 * (value * (6.0 * value - 15.0) + 10.0)
            case Interpolation.LINEAR:
                pass
        return 1.0 - value if self.invert else value


class Precision(msgspec.Struct, frozen=True, kw_only=True):
    """Caller-selected scrambled Sobol resolution and independent error replicates."""

    power: Annotated[int, msgspec.Meta(ge=0, le=64, title="Samples Exponent", description="Each complete scrambled Sobol net contains 2 to this power observations per support and replicate")]
    replicates: Annotated[int, msgspec.Meta(ge=2, title="Replicates", description="Independent scrambles used to estimate sampling standard error")]
    seed: Annotated[Seed, msgspec.Meta(title="Scramble Seed", description="Deterministic seed of the numerical integration scrambles")]
    absolute: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=float_info.max, title="Profile Absolute Tolerance", description="Adaptive angular quadrature target in unit-radius profile area; distinct from pixel sampling error"),
    ]
    relative: Annotated[
        float,
        msgspec.Meta(
            ge=0.0, le=float_info.max, title="Profile Relative Tolerance", description="Adaptive angular quadrature target as a fraction of profile area; attained accuracy is not guaranteed"
        ),
    ]

    def points(self, dimensions: int, replicate: int, *, key: tuple[int, ...] = ()) -> npt.NDArray[np.float64]:
        """Returns a complete independently scrambled Sobol net for one replicate."""
        digest = hashlib.blake2b(cbor2.dumps((self.seed, replicate, key), canonical=True), digest_size=16).digest()
        rng = np.random.Generator(np.random.Philox(key=int.from_bytes(digest, "little")))
        return qmc.Sobol(dimensions, scramble=True, bits=64, rng=rng).random_base2(self.power)


class Estimate[T](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Numerical estimate and replicate standard error, without an exactness certificate."""

    value: T
    standard_error: npt.NDArray[np.float64]
    precision: Precision
    replicates: npt.NDArray[np.float64]
    normalization: Option[tuple[float, float]] = Nothing

    @staticmethod
    def of(replicates: npt.NDArray[np.float64], precision: Precision) -> "Estimate[npt.NDArray[np.float64]]":
        """Returns replicate mean and standard error of the mean."""
        return Estimate(
            value=np.asarray(np.mean(replicates, axis=0)), standard_error=np.asarray(np.std(replicates, axis=0, ddof=1) / math.sqrt(precision.replicates)), precision=precision, replicates=replicates
        )


class CellularSample(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Cellular value, nearest feature displacement, integer identity, and cell colors."""

    value: npt.NDArray[np.float64]
    delta: npt.NDArray[np.float64]
    cell: tuple[tuple[int, ...], ...]
    color: npt.NDArray[np.float64]


class CellularSite(msgspec.Struct, frozen=True, kw_only=True):
    """Candidate feature in exact lattice coordinates and its coupled cell colors."""

    cell: tuple[int, ...]
    point: tuple[Fraction, ...]
    color: tuple[float, float]


class Octaves(msgspec.Struct, frozen=True, kw_only=True):
    """Fractal sum of a noise texture's octaves."""

    detail: Annotated[float, msgspec.Meta(ge=0.0, le=15.0, title="Detail", description="Detail past the first octave"), Presentation(soft=Some((0.0, 8.0)), decimals=Some(1))] = 2.0
    roughness: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Roughness", description="Amplitude of each octave over the one before")] = 0.5
    lacunarity: Annotated[float, msgspec.Meta(ge=0.0, le=float_info.max, title="Lacunarity", description="Frequency of each octave over the one before"), Presentation(soft=Some((0.0, 4.0)))] = 2.0


class Perlin(Case, frozen=True, kw_only=True, tag=1):
    """Fractal Perlin noise cut to a density by a soft threshold."""

    title = "Perlin Noise"
    dimensions: Annotated[int, msgspec.Meta(ge=1, le=4, title="Dimensions", description="Axes of the native noise lattice"), GraphForm.SETTING] = 4
    size: Annotated[
        float,
        msgspec.Meta(gt=0.0, le=float_info.max, title="Size", description="Width of the largest feature as a share of the frame's greater side"),
        Presentation(soft=Some((0.01, 1.0)), decimals=Some(3), quantity=Some(Quantity.FRAME_GREATER)),
    ] = _FIRE_SHORT_SIDE / 1.4
    stretch: Annotated[float, msgspec.Meta(gt=0.0, le=float_info.max, title="Stretch", description="Feature height over width along the field's axes"), Presentation(soft=Some((0.25, 4.0)))] = 2.0
    fractal: Annotated[Fractal, msgspec.Meta(title="Fractal", description="Sum combining the octaves; ridged noise ignores fractional detail"), GraphForm.SETTING] = Fractal.FBM
    octaves: Annotated[Octaves, msgspec.Meta(title="Octaves", description="Fractal sum of the noise")] = Octaves(detail=4.0)
    offset: Annotated[Finite, msgspec.Meta(title="Offset", description="Value added to each octave of the ridged, hybrid, and hetero terrain sums"), Presentation(soft=Some((-2.0, 2.0)))] = 0.0
    gain: Annotated[
        float, msgspec.Meta(ge=0.0, le=float_info.max, title="Gain", description="Weight each octave hands the next in the ridged and hybrid sums"), Presentation(soft=Some((0.0, 4.0)))
    ] = 1.0
    distortion: Annotated[Finite, msgspec.Meta(title="Distortion", description="Signed warp of the field by its own noise"), Presentation(soft=Some((-4.0, 4.0)))] = 1.0
    cut: Annotated[Cut, msgspec.Meta(title="Density", description="Noise value mapped to density")] = Cut(threshold=0.17, softness=0.83, interpolation=Interpolation.LINEAR)


class Voronoi(Case, frozen=True, kw_only=True, tag=2):
    """Cellular distances cut to a density by a soft threshold."""

    title = "Voronoi"
    dimensions: Annotated[int, msgspec.Meta(ge=1, le=4, title="Dimensions", description="Axes of the cellular lattice"), GraphForm.SETTING] = 4
    spacing: Annotated[
        float,
        msgspec.Meta(gt=0.0, le=float_info.max, title="Spacing", description="Mean distance between cell points as a share of the frame's greater side"),
        Presentation(soft=Some((0.002, 0.5)), decimals=Some(4), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 0.1
    feature: Annotated[Feature, msgspec.Meta(title="Feature", description="Distance the cells answer"), GraphForm.SETTING] = Feature.F1
    metric: Annotated[Metric, msgspec.Meta(title="Metric", description="Distance measure between a position and a cell point"), GraphForm.SETTING] = Metric.EUCLIDEAN
    exponent: Annotated[float, msgspec.Meta(gt=0.0, le=float_info.max, title="Exponent", description="Minkowski exponent, 1 Manhattan and 2 Euclidean"), Presentation(soft=Some((0.1, 4.0)))] = 0.5
    smoothness: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Smoothness", description="Blend between neighboring cells of smooth F1")] = 1.0
    randomness: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Randomness", description="Jitter of each cell point from its lattice corner")] = 1.0
    octaves: Annotated[Octaves, msgspec.Meta(title="Octaves", description="Fractal sum of the cells")] = Octaves(detail=0.0)
    cut: Annotated[Cut, msgspec.Meta(title="Density", description="Distance mapped to density")] = Cut()
    cell_color: Annotated[bool, msgspec.Meta(title="Cell Color", description="Cell red selects tone and cell green scales density")] = False


class Wave(Case, frozen=True, kw_only=True, tag=3):
    """Bands or rings of a periodic profile cut to a density by a soft threshold."""

    title = "Wave"
    wavelength: Annotated[
        float,
        msgspec.Meta(gt=0.0, le=float_info.max, title="Wavelength", description="Distance from one band or ring to the next as a share of the frame's greater side"),
        Presentation(soft=Some((0.005, 1.0)), decimals=Some(4), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 0.1
    pattern: Annotated[Pattern, msgspec.Meta(title="Pattern", description="Bands across the field or rings about the center"), GraphForm.SETTING] = Pattern.BANDS
    profile: Annotated[Profile, msgspec.Meta(title="Profile", description="Shape of each period"), GraphForm.SETTING] = Profile.SIN
    phase: Annotated[Finite, msgspec.Meta(title="Phase", description="Phase added to the periodic profile in radians"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    distortion: Annotated[Finite, msgspec.Meta(title="Distortion", description="Signed phase offset in radians by the distortion noise"), Presentation(soft=Some((-20.0, 20.0)))] = 0.0
    detail: Annotated[float, msgspec.Meta(ge=0.0, le=15.0, title="Detail", description="Octaves of the distortion noise past the first"), Presentation(soft=Some((0.0, 8.0)), decimals=Some(1))] = 2.0
    detail_scale: Annotated[float, msgspec.Meta(ge=0.0, le=float_info.max, title="Detail Scale", description="Frequency of the distortion noise"), Presentation(soft=Some((0.0, 10.0)))] = 1.0
    detail_roughness: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Detail Roughness", description="Amplitude of each distortion octave over the one before")] = 0.5
    cut: Annotated[Cut, msgspec.Meta(title="Density", description="Wave value mapped to density")] = Cut()


class Gabor(Case, frozen=True, kw_only=True, tag=4):
    """Sparse windowed oscillations with a chosen frequency and orientation."""

    title = "Gabor"
    dimensions: Annotated[int, msgspec.Meta(ge=2, le=3, title="Dimensions", description="Planar or spatial impulse lattice"), GraphForm.SETTING] = 3
    size: Annotated[float, msgspec.Meta(gt=0.0, le=float_info.max, title="Size", description="Impulse cell width"), Presentation(quantity=Some(Quantity.FRAME_GREATER))] = 0.1
    frequency: Annotated[float, msgspec.Meta(ge=0.001, le=float_info.max, title="Frequency", description="Oscillations per impulse cell"), Presentation(soft=Some((0.001, 10.0)))] = 2.0
    anisotropy: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Anisotropy", description="Alignment of impulse orientations")] = 1.0
    orientation: Annotated[Finite, msgspec.Meta(title="Orientation", description="Azimuth of the aligned oscillations"), Presentation(quantity=Some(Quantity.ANGLE))] = math.pi / 4.0
    cut: Annotated[Cut, msgspec.Meta(title="Density", description="Oscillation value mapped to density")] = Cut()


class Magic(Case, frozen=True, kw_only=True, tag=5):
    """Nested trigonometric color turbulence averaged into a scalar field."""

    title = "Magic"
    size: Annotated[float, msgspec.Meta(gt=0.0, le=float_info.max, title="Size", description="Spatial scale of the pattern"), Presentation(quantity=Some(Quantity.FRAME_GREATER))] = 0.1
    depth: Annotated[int, msgspec.Meta(ge=0, le=10, title="Depth", description="Nested trigonometric steps"), GraphForm.SETTING] = 2
    distortion: Annotated[Finite, msgspec.Meta(title="Distortion", description="Signed amplitude of each nested step")] = 1.0
    cut: Annotated[Cut, msgspec.Meta(title="Density", description="Mean color mapped to density")] = Cut()


class White(Case, frozen=True, kw_only=True, tag=6):
    """Constant random values on integer lattice cells."""

    title = "White Noise"
    dimensions: Annotated[int, msgspec.Meta(ge=1, le=4, title="Dimensions", description="Axes of the hashed cell lattice"), GraphForm.SETTING] = 4
    spacing: Annotated[float, msgspec.Meta(gt=0.0, le=float_info.max, title="Spacing", description="Cell width"), Presentation(quantity=Some(Quantity.FRAME_GREATER))] = 0.1
    cut: Annotated[Cut, msgspec.Meta(title="Density", description="Cell value mapped to density")] = Cut()


class Tint(Case, frozen=True, kw_only=True, tag=1):
    """Color graded from a start to an end by the field's tone."""

    title = "Tint"
    start: Annotated[Rgb[Finite], msgspec.Meta(title="Start", description="Scene-linear color at the lowest tone")] = Rgb(red=1.0, green=1.0, blue=1.0)
    end: Annotated[Rgb[Finite], msgspec.Meta(title="End", description="Scene-linear color at the highest tone")] = Rgb(red=1.0, green=1.0, blue=1.0)


class Incandescence(Case, frozen=True, kw_only=True, tag=2):
    """Blackbody color of unit luminance graded from a cool to a hot temperature by the field's tone."""

    title = "Incandescence"
    cool: Annotated[
        float, msgspec.Meta(ge=800.0, le=12000.0, title="Cool", description="Temperature at the lowest tone"), Presentation(step=Some(1.0), decimals=Some(0), quantity=Some(Quantity.COLOR_TEMPERATURE))
    ] = 1000.0
    hot: Annotated[
        float, msgspec.Meta(ge=800.0, le=12000.0, title="Hot", description="Temperature at the highest tone"), Presentation(step=Some(1.0), decimals=Some(0), quantity=Some(Quantity.COLOR_TEMPERATURE))
    ] = 2000.0


class Gradient(Case, frozen=True, kw_only=True, tag=3):
    """Color read from a color ramp at the field's tone."""

    title = "Gradient"
    ramp: Annotated[Ramp, msgspec.Meta(title="Ramp", description="Scene-linear colors placed along tone from 0 to 1")] = Ramp()


class Emission(msgspec.Struct, frozen=True, kw_only=True):
    """Light a generator adds by its coverage, colored by tone and leveled in stops."""

    spectrum: Annotated[Spectrum, msgspec.Meta(title="Spectrum", description="Color the field emits at each tone")] = Incandescence()
    intensity: Annotated[Stops, msgspec.Meta(title="Intensity", description="Luminance at full density in stops")] = 2.0


class Extinction(msgspec.Struct, frozen=True, kw_only=True):
    """Light the field absorbs and the light it scatters toward the camera."""

    density: Annotated[float, msgspec.Meta(ge=0.0, le=float_info.max, title="Density", description="Optical depth at full density"), Presentation(soft=Some((0.0, 8.0)))] = 2.0
    color: Annotated[Rgb[Finite], msgspec.Meta(title="Color", description="Scene-linear light the medium scatters toward the camera")] = Rgb(red=0.04, green=0.04, blue=0.04)


class Vortex(msgspec.Struct, frozen=True, kw_only=True):
    """Polar twirl of the field about its center."""

    radius: Annotated[
        float,
        msgspec.Meta(gt=0.0, le=float_info.max, title="Radius", description="Distance where the twirl reaches 0 and samples stop widening as a share of the frame's greater side"),
        Presentation(soft=Some((0.01, 1.0)), decimals=Some(3), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 0.25
    twirl: Annotated[
        Finite, msgspec.Meta(title="Twirl", description="Turn at the center falling to 0 at the radius"), Presentation(soft=Some((-4.0 * math.pi, 4.0 * math.pi)), quantity=Some(Quantity.ANGLE))
    ] = 3.0
    arms: Annotated[int, msgspec.Meta(ge=0, title="Arms", description="Repeats of the field around the center, 0 leaving rings"), Presentation(soft=Some((0.0, 32.0)))] = 1
    spin: Annotated[Finite, msgspec.Meta(title="Spin", description="Turn of the vortex in radians per clock frame"), Presentation(soft=Some((-math.pi / 12.0, math.pi / 12.0)), decimals=Some(4))] = (
        1.0 / 24.0
    )
    tunnel: Annotated[
        float, msgspec.Meta(ge=0.0, le=float_info.max, title="Tunnel", description="Height sampled along the vortex axis per unit of sampled radius"), Presentation(soft=Some((0.0, 4.0)))
    ] = 1.0


class Envelope(msgspec.Struct, frozen=True, kw_only=True):
    """Elliptical falloff subtracted from the texture before density mapping."""

    width: Annotated[
        float,
        msgspec.Meta(gt=0.0, le=float_info.max, title="Width", description="Horizontal radius as a share of the frame's greater side"),
        Presentation(soft=Some((0.01, 1.0)), quantity=Some(Quantity.FRAME_GREATER)),
    ] = _FIRE_SHORT_SIDE
    height: Annotated[
        float,
        msgspec.Meta(gt=0.0, le=float_info.max, title="Height", description="Vertical radius as a share of the frame's greater side"),
        Presentation(soft=Some((0.01, 1.0)), quantity=Some(Quantity.FRAME_GREATER)),
    ] = _FIRE_SHORT_SIDE


class NoiseField(Effect, frozen=True, kw_only=True, tag=32):
    """Procedural light and haze added to the frame: fire, smoke, mist, light leaks, and flow."""

    stage = Stage.GENERATE
    title = "Noise Field"
    pointwise = False
    basis: Annotated[Basis, msgspec.Meta(title="Basis", description="Selected procedural texture")] = Perlin()
    emission: Annotated[Gated[Emission], msgspec.Meta(title="Emission", description="Light the field adds")] = Gated(enabled=True, value=Emission(spectrum=Incandescence(cool=800.0), intensity=0.0))
    extinction: Annotated[Gated[Extinction], msgspec.Meta(title="Extinction", description="Light the field absorbs and scatters")] = Gated(enabled=False, value=Extinction())
    motion: Annotated[Motion, msgspec.Meta(title="Motion", description="Clock the field drifts and changes over")] = Motion()
    drift: Annotated[
        tuple[Finite, Finite],
        msgspec.Meta(title="Drift", description="Field velocity along its axes in shares of the frame's greater side per clock frame"),
        Presentation(decimals=Some(4), quantity=Some(Quantity.FRAME_GREATER)),
    ] = (0.0, 5.0 * _FIRE_SHORT_SIDE / 24.0)
    evolution: Annotated[Finite, msgspec.Meta(title="Evolution", description="Signed feature evolution per clock frame"), Presentation(soft=Some((-0.2, 0.2)), decimals=Some(4))] = 0.0
    vortex: Annotated[Gated[Vortex], msgspec.Meta(title="Vortex", description="Polar twirl of the field about its center")] = Gated(enabled=False, value=Vortex())
    envelope: Annotated[Gated[Envelope], msgspec.Meta(title="Envelope", description="Spatial falloff applied before density mapping")] = Gated(enabled=True, value=Envelope())
    center: Annotated[Position, msgspec.Meta(title="Center", description="Frame position the axes turn about")] = (0.5, 0.0)
    angle: Annotated[Finite, msgspec.Meta(title="Angle", description="Rotation of the field's axes"), Presentation(soft=Some((-math.pi, math.pi)), quantity=Some(Quantity.ANGLE))] = 0.0
    seed: Annotated[Seed, msgspec.Meta(title="Seed", description="Field of equal statistics the entry samples")] = 0


# --- [OPERATIONS] -------------------------------------------------------------------------


def uniforms(cell: tuple[int, ...], *, seed: int, stream: int = 0, count: int = 4) -> npt.NDArray[np.float64]:
    """Returns stable Philox draws for an integer cell without truncating its key."""
    key = hashlib.blake2b(cbor2.dumps((seed, stream, cell), canonical=True), digest_size=16).digest()
    return np.random.Generator(np.random.Philox(key=int.from_bytes(key, "little"))).random(count)


def spectrum(
    request: Spectrum, tones: npt.NDArray[np.float64], *, config: PyOpenColorIO.Config, gradient: Callable[[Ramp, npt.NDArray[np.float64]], ImagingResult[npt.NDArray[np.float64]]]
) -> ImagingResult[npt.NDArray[np.float64]]:
    """Evaluates linear tints, native ramps, or observer-integrated unit-Y blackbodies."""
    try:
        if len(tones) == 0:
            return Ok(np.empty((0, 3), dtype=np.float64))
        with np.errstate(over="raise", divide="raise", invalid="raise"):
            match request:
                case Tint():
                    start, end = np.array(msgspec.structs.astuple(request.start)), np.array(msgspec.structs.astuple(request.end))
                    return Ok((1.0 - tones[:, None]) * start + tones[:, None] * end)
                case Gradient():
                    return gradient(request.ramp, tones)
                case Incandescence():
                    observer = table()
                    temperatures = (1.0 - tones) * request.cool + tones * request.hot
                    radiance = np.asarray(planck_law(observer[:, 0] * 1e-9, temperatures), dtype=np.float64).reshape(len(observer), len(tones))
                    tristimulus = np.trapezoid(radiance[:, :, None] * observer[:, None, 1:], x=observer[:, 0], axis=0)
                    tristimulus /= tristimulus[:, 1:2]  # type: ignore[index]

                    def solved(matrix: npt.NDArray[np.float64]) -> ImagingResult[npt.NDArray[np.float64]]:
                        try:
                            return Ok(np.linalg.solve(matrix, tristimulus.T).T)
                        except np.linalg.LinAlgError as error:
                            return Error((ImagingMatrixFault(request=matrix, error=error),))

                    return color_basis(config, PyOpenColorIO.ROLE_SCENE_LINEAR).bind(solved)
    except (FloatingPointError, MemoryError, OverflowError, ValueError) as error:
        return Error((ImagingGeneratorFault(request=request, error=error),))


def integrated(
    request: msgspec.Struct, field: Callable[[npt.NDArray[np.float64]], ImagingResult[npt.NDArray[np.float64]]], bounds: tuple[tuple[float, ...], tuple[float, ...]], *, precision: Precision
) -> ImagingResult[Estimate[npt.NDArray[np.float64]]]:
    """Estimates a positive rectangular field integral with replicate standard error."""
    try:
        with np.errstate(over="raise", divide="raise", invalid="raise"):
            lower, upper = np.asarray(bounds, dtype=np.float64)
            width = upper - lower
            volume = np.prod(width)
            results = accumulated(
                field(lower + width * precision.points(len(lower), replicate)).map(lambda values: np.asarray(np.mean(values, axis=0) * volume)) for replicate in range(precision.replicates)
            )
            return results.map(lambda values: Estimate.of(np.stack(values), precision))
    except (FloatingPointError, MemoryError, OverflowError, ValueError) as error:
        return Error((ImagingGeneratorFault(request=request, error=error),))


def cellular(basis: Voronoi, points: npt.NDArray[np.float64], *, seed: Seed, stream: int = 0) -> ImagingResult[CellularSample]:
    """Returns a base-octave cellular scan including stable nearest-cell evidence."""
    try:
        with np.errstate(over="raise", divide="raise"):
            return Ok(_cellular(basis, _coordinates(points, basis.spacing, basis.dimensions), seed, stream))
    except (FloatingPointError, MemoryError, OverflowError, ValueError) as error:
        return Error((ImagingGeneratorFault(request=basis, error=error),))


def cellular_sites(basis: Voronoi, bounds: tuple[tuple[Fraction, ...], tuple[Fraction, ...]], *, seed: Seed, stream: int = 0) -> ImagingResult[tuple[CellularSite, ...]]:
    """Enumerates all base-octave stencil candidates over positive lattice bounds."""
    try:
        lower, upper = bounds
        reach = 2 if basis.feature == Feature.SMOOTH_F1 and basis.smoothness > 0.0 else 1
        axes = tuple(range(math.floor(first) - reach, math.floor(last) + reach + 1) for first, last in zip(lower, upper, strict=True))
        sites: list[CellularSite] = []
        for cell in itertools.product(*axes):
            draw = uniforms(cell, seed=seed, stream=stream, count=max(4, len(cell)))
            point = tuple(Fraction(integer) + Fraction(basis.randomness) * Fraction(float(value)) for integer, value in zip(cell, draw[: len(cell)], strict=True))
            sites.append(CellularSite(cell=cell, point=point, color=(float(draw[0]), float(draw[1]))))
        return Ok(tuple(sites))
    except (MemoryError, OverflowError, ValueError) as error:
        return Error((ImagingGeneratorFault(request=basis, error=error),))


def density_range(basis: Basis) -> tuple[float, float]:
    """Bounds finite cut density, including native smooth-cell color corrections."""
    if isinstance(basis, Voronoi) and basis.cell_color and basis.feature == Feature.SMOOTH_F1:
        span = basis.smoothness / 2.0
        return -(5**basis.dimensions - 1) * span / (4.0 * (1.0 + 3.0 * span)), 1.0
    return 0.0, 1.0


def sample(basis: Basis, points: npt.NDArray[np.float64], *, seed: Seed, stream: int = 0) -> ImagingResult[npt.NDArray[np.float64]]:
    """Returns columns density and tone at greater-side XYZ/evolution coordinates."""
    try:
        if len(points) == 0:
            return Ok(np.empty((0, 2), dtype=np.float64))
        with np.errstate(over="raise", divide="raise"):
            value, tone = _sample(basis, points, seed, stream)
            return Ok(np.stack((value, tone), axis=-1))
    except (FloatingPointError, MemoryError, OverflowError, ValueError) as error:
        return Error((ImagingGeneratorFault(request=basis, error=error),))


def _lattice(points: npt.NDArray[np.float64]) -> tuple[list[tuple[int, ...]], npt.NDArray[np.float64]]:
    cells = [tuple(math.floor(float(points[row, axis])) for axis in range(points.shape[1])) for row in range(len(points))]
    local = np.array([[float(points[row, axis]) - integer for axis, integer in enumerate(cell)] for row, cell in enumerate(cells)], dtype=np.float64)
    return cells, local


def _gradient(points: npt.NDArray[np.float64], seed: int, stream: int) -> npt.NDArray[np.float64]:
    cells, local = _lattice(points)
    fade = local**3 * (local * (6.0 * local - 15.0) + 10.0)
    total = np.zeros(len(points), dtype=np.float64)
    rank = points.shape[1]
    sign_split = 0.5
    for corner in itertools.product((0, 1), repeat=rank):
        draws = np.array([uniforms(tuple(value + offset for value, offset in zip(cell, corner, strict=True)), seed=seed, stream=stream, count=rank) for cell in cells])
        gradient = np.where(draws < sign_split, -1.0, 1.0)
        weight = np.prod(np.where(np.array(corner), fade, 1.0 - fade), axis=1)
        total += weight * np.sum(gradient * (local - corner), axis=1)
    total /= rank
    return total


def _fractal(basis: Perlin, points: npt.NDArray[np.float64], seed: int, stream: int) -> npt.NDArray[np.float64]:
    warped = points + basis.distortion * np.stack([_gradient(points, seed, stream * 5 + axis + 1) for axis in range(points.shape[1])], axis=-1)
    value = np.zeros(len(points), dtype=np.float64)
    signal = np.zeros_like(value)
    weight = np.ones_like(value)
    amplitude = 1.0
    total = 0.0
    for octave in range(math.ceil(basis.octaves.detail) + 1):
        fraction = min(1.0, basis.octaves.detail + 1.0 - octave)
        noise = _gradient(warped, seed, stream)
        match basis.fractal:
            case Fractal.FBM:
                previous = value / total if total else value
                value += amplitude * noise
                total += amplitude
                if fraction < 1.0:
                    return 0.5 + 0.5 * ((1.0 - fraction) * previous + fraction * value / total)
            case Fractal.MULTIFRACTAL:
                value = (np.ones_like(value) if octave == 0 else value) * (1.0 + fraction * amplitude * noise)
            case Fractal.RIDGED_MULTIFRACTAL:
                if fraction < 1.0:
                    break
                signal = np.square(basis.offset - np.abs(noise)) * (1.0 if octave == 0 else np.clip(signal * basis.gain, 0.0, 1.0))
                value += amplitude * signal
            case Fractal.HYBRID_MULTIFRACTAL:
                minimum_weight = 0.001
                held = np.minimum(weight, 1.0)
                signal = (noise + basis.offset) * amplitude
                active = weight > minimum_weight
                value += np.where(active, fraction * held * signal, 0.0)
                weight = np.where(active, held * basis.gain * signal, weight)
            case Fractal.HETERO_TERRAIN:
                signal = noise + basis.offset
                value = signal if octave == 0 else value + fraction * amplitude * signal * value
        amplitude *= basis.octaves.roughness
        if octave < math.ceil(basis.octaves.detail):
            warped *= basis.octaves.lacunarity
    return 0.5 + 0.5 * value / total if basis.fractal == Fractal.FBM else value


def _distance(delta: npt.NDArray[np.float64], basis: Voronoi) -> npt.NDArray[np.float64]:
    match basis.metric:
        case Metric.EUCLIDEAN:
            return np.linalg.norm(delta, axis=-1)
        case Metric.MANHATTAN:
            return np.abs(delta).sum(axis=-1)
        case Metric.CHEBYCHEV:
            return np.abs(delta).max(axis=-1)
        case Metric.MINKOWSKI:
            maximum = np.abs(delta).max(axis=-1)
            ratio = np.divide(np.abs(delta), maximum[..., None], out=np.zeros_like(delta), where=maximum[..., None] > 0.0)
            with np.errstate(divide="ignore", under="ignore"):
                logarithm = np.log(ratio)
                with np.errstate(over="ignore"):
                    scaled = basis.exponent * logarithm
                return maximum * np.exp(logsumexp(scaled, axis=-1) / basis.exponent)


def _cellular(basis: Voronoi, points: npt.NDArray[np.float64], seed: int, stream: int) -> CellularSample:
    cells, local = _lattice(points)
    reach = 2 if basis.feature == Feature.SMOOTH_F1 and basis.smoothness > 0.0 else 1
    offsets = tuple(itertools.product(range(-reach, reach + 1), repeat=points.shape[1]))
    colors = np.array([
        [uniforms(tuple(value + offset for value, offset in zip(cell, neighbor, strict=True)), seed=seed, stream=stream, count=max(4, points.shape[1])) for cell in cells] for neighbor in offsets
    ])
    delta = np.array(offsets)[:, None, :] + basis.randomness * colors[:, :, : points.shape[1]] - local
    distance = _distance(delta, basis)
    indices = np.argsort(distance, axis=0, kind="stable")
    columns = np.arange(len(points))
    tone = colors[indices[0], columns, :2]
    match basis.feature:
        case Feature.F1:
            value = distance[indices[0], columns]
        case Feature.F2:
            value = distance[indices[1], columns]
            tone = colors[indices[1], columns, :2]
        case Feature.SMOOTH_F1:
            value = distance[0]
            tone = colors[0, :, :2] if basis.smoothness > 0.0 else tone
            for index in range(1, len(offsets)):
                if basis.smoothness == 0.0:
                    value = np.minimum(value, distance[index])
                else:
                    span = basis.smoothness / 2.0
                    blend = np.clip(0.5 + 0.5 * (value - distance[index]) / span, 0.0, 1.0)
                    blend = blend * blend * (3.0 - 2.0 * blend)
                    correction = span * blend * (1.0 - blend)
                    value = (1.0 - blend) * value + blend * distance[index] - correction
                    tone = (1.0 - blend[:, None]) * tone + blend[:, None] * colors[index, :, :2] - correction[:, None] / (1.0 + 3.0 * span)
        case Feature.DISTANCE_TO_EDGE:
            nearest = np.argmin(np.sum(delta * delta, axis=-1), axis=0)
            closest = delta[nearest, columns]
            difference = delta - closest
            norm = np.linalg.norm(difference, axis=-1)
            bisectors = np.divide(np.sum((closest + delta) * difference / 2.0, axis=-1), norm, out=np.full_like(norm, np.inf), where=norm > 0.0)
            value = bisectors.min(axis=0)
            tone = colors[nearest, columns, :2]
    selected = np.argmin(np.sum(delta * delta, axis=-1), axis=0) if basis.feature == Feature.DISTANCE_TO_EDGE else indices[1] if basis.feature == Feature.F2 else indices[0]
    labels = tuple(tuple(value + offset for value, offset in zip(cell, offsets[int(index)], strict=True)) for cell, index in zip(cells, selected, strict=True))
    return CellularSample(value=value, delta=delta[selected, columns], cell=labels, color=tone)


def _voronoi(basis: Voronoi, points: npt.NDArray[np.float64], seed: int, stream: int) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]:
    edge = basis.feature == Feature.DISTANCE_TO_EDGE
    bound = 0.5 + 0.5 * basis.randomness
    limit = bound if edge else float(_distance(np.full((1, points.shape[1]), bound), basis)[0]) * (2.0 if basis.feature == Feature.F2 else 1.0)
    value = np.full(len(points), 8.0 if edge else 0.0)
    tone = np.zeros((len(points), 2), dtype=np.float64)
    normalizer = limit if edge else 0.0
    scale = 1.0
    amplitude = 1.0
    for octave in range(math.ceil(basis.octaves.detail) + 1):
        if (weight := (amplitude * min(1.0, basis.octaves.detail + 1.0 - octave))) == 0.0:
            break
        feature = _cellular(basis, points * scale, seed, stream)
        distance, color = feature.value, feature.color
        if edge:
            with np.errstate(divide="ignore", invalid="ignore"):
                value = (1.0 - weight) * value + weight * np.minimum(value, np.divide(distance, scale))
                normalizer = (1.0 - weight) * normalizer + weight * np.divide(limit, scale)
        else:
            value += weight * distance
            normalizer += weight * limit
        tone = (1.0 - weight) * tone + weight * color
        amplitude *= basis.octaves.roughness
        if octave < math.ceil(basis.octaves.detail):
            scale *= basis.octaves.lacunarity
    with np.errstate(invalid="ignore"):
        return value / normalizer, tone


def _gabor(basis: Gabor, points: npt.NDArray[np.float64], seed: int, stream: int) -> npt.NDArray[np.float64]:
    rank = basis.dimensions
    cells, local = _lattice(points)
    total = np.zeros(len(points), dtype=np.float64)
    for offset in itertools.product((-1, 0, 1), repeat=rank):
        keys = [tuple(value + shift for value, shift in zip(cell, offset, strict=True)) for cell in cells]
        for impulse in range(8):
            center = np.array([uniforms(key, seed=seed, stream=stream * 17 + 2 * impulse, count=4) for key in keys])
            turns = np.array([uniforms(key, seed=seed, stream=stream * 17 + 2 * impulse + 1, count=2) for key in keys])
            delta = local - offset - center[:, :rank]
            squared = np.sum(delta * delta, axis=1)
            if rank == 2:
                angle = basis.orientation + (turns[:, 0] - 0.5) * math.pi * (1.0 - basis.anisotropy)
                direction = np.stack((np.cos(angle), np.sin(angle)), axis=-1)
            else:
                inclination = math.pi / 2.0 + turns[:, 0] * math.pi * (1.0 - basis.anisotropy)
                azimuth = basis.orientation + turns[:, 1] * math.pi * (1.0 - basis.anisotropy)
                direction = np.stack((np.sin(inclination) * np.cos(azimuth), np.sin(inclination) * np.sin(azimuth), np.cos(inclination)), axis=-1)
            total += np.where(
                squared < 1.0,
                np.copysign(np.exp(-math.pi * squared) * (0.5 + 0.5 * np.cos(math.pi * squared)), center[:, 3] - 0.5) * np.sin(math.tau * basis.frequency * np.sum(delta * direction, axis=1)),
                0.0,
            )
    total /= 6.0 * math.sqrt(4.0 * 2.0 ** (-1.0 - rank / 2.0))
    total *= 0.5
    total += 0.5
    return total


def _magic(basis: Magic, points: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
    x, y, z = (points % math.tau).T
    values = np.stack((np.sin(5.0 * (x + y + z)), np.cos(5.0 * (-x + y - z)), -np.cos(5.0 * (-x - y + z))), axis=-1) * (1.0 if basis.depth == 0 else basis.distortion)
    for step in range(basis.depth):
        x, y, z = values.T
        if step == 0:
            values[:, 1] = -np.cos(x - y + z) * basis.distortion
        elif step == 1:
            values[:, 0] = np.cos(x - y - z) * basis.distortion
        elif step == 2:
            values[:, 2] = np.sin(-x - y - z) * basis.distortion
        elif step == 3:
            values[:, 0] = -np.cos(-x + y - z) * basis.distortion
        elif step == 4:
            values[:, 1] = -np.sin(-x + y + z) * basis.distortion
        elif step == 5:
            values[:, 1] = -np.cos(-x + y + z) * basis.distortion
        elif step == 6:
            values[:, 0] = np.cos(x + y + z) * basis.distortion
        elif step == 7:
            values[:, 2] = np.sin(x + y - z) * basis.distortion
        elif step == 8:
            values[:, 0] = -np.cos(-x - y + z) * basis.distortion
        else:
            values[:, 1] = -np.sin(x - y + z) * basis.distortion
    return np.mean(0.5 - (values if basis.distortion == 0.0 else values / (2.0 * basis.distortion)), axis=-1)


def _sample(basis: Basis, points: npt.NDArray[np.float64], seed: int, stream: int) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]:
    match basis:
        case Perlin():
            coordinates = _coordinates(points, basis.size, basis.dimensions)
            if basis.dimensions > 1:
                coordinates[:, 1] /= basis.stretch
            raw = _fractal(basis, coordinates, seed, stream)
        case Voronoi():
            raw, color = _voronoi(basis, _coordinates(points, basis.spacing, basis.dimensions), seed, stream)
            density = basis.cut.applied(raw)
            return (density * color[:, 1], color[:, 0]) if basis.cell_color else (density, density)
        case White():
            cells, _ = _lattice(_coordinates(points, basis.spacing, basis.dimensions))
            raw = np.array([uniforms(cell, seed=seed, stream=stream, count=1)[0] for cell in cells])
        case Gabor():
            raw = _gabor(basis, _coordinates(points, basis.size, basis.dimensions), seed, stream)
        case Magic():
            raw = _magic(basis, _coordinates(points, basis.size, 3))
        case Wave():
            coordinates = points[:, :3] / basis.wavelength
            phase = math.tau * (coordinates[:, 0] if basis.pattern == Pattern.BANDS else np.linalg.norm(coordinates[:, :2], axis=1)) + basis.phase - math.tau * points[:, 3]
            if basis.distortion != 0.0:
                field = Perlin(dimensions=3, size=1.0, stretch=1.0, octaves=Octaves(detail=basis.detail, roughness=basis.detail_roughness), distortion=0.0)
                phase += basis.distortion * (2.0 * _fractal(field, coordinates * (math.pi / 10.0) * basis.detail_scale, seed, stream) - 1.0)
            period = phase / math.tau
            match basis.profile:
                case Profile.SIN:
                    raw = 0.5 - 0.5 * np.cos(phase)
                case Profile.SAW:
                    raw = period % 1.0
                case Profile.TRI:
                    raw = 2.0 * np.abs(period - np.floor(period + 0.5))
                case Profile.SQUARE:
                    raw = np.floor(2.0 * (period % 1.0))
    value = basis.cut.applied(raw)
    return value, value


def _coordinates(points: npt.NDArray[np.float64], size: float, rank: int) -> npt.NDArray[np.float64]:
    coordinates = points[:, :rank].copy()
    coordinates[:, : min(rank, 3)] /= size
    if rank == 3:
        coordinates[:, 2] += points[:, 3]
    return coordinates


# --- [EXPORTS] ----------------------------------------------------------------------------

__all__ = [
    "Basis",
    "CellularSample",
    "CellularSite",
    "Cut",
    "Emission",
    "Envelope",
    "Estimate",
    "Extinction",
    "Feature",
    "Fractal",
    "Gabor",
    "Gradient",
    "Incandescence",
    "Interpolation",
    "Magic",
    "Metric",
    "NoiseField",
    "Octaves",
    "Pattern",
    "Perlin",
    "Precision",
    "Profile",
    "Spectrum",
    "Tint",
    "Voronoi",
    "Vortex",
    "Wave",
    "White",
    "cellular",
    "cellular_sites",
    "density_range",
    "integrated",
    "sample",
    "spectrum",
    "uniforms",
]
