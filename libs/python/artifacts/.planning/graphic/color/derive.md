# [PY_ARTIFACTS_GRAPHIC_COLOR_DERIVE]

Upstream color-derivation substrate feeding perceptually correct color into every visual sub-domain — derive returns color values, never a receipt. `Colorimetry` owns one two-engine dispatch. colour-science carries colorimetric truth through the universal `colour.convert` model-pair gateway, appearance models, spectral intake, spectral resampling, color-difference families, chromatic adaptation, correlated color temperature, colorimetric indices, color correction, graded CVD simulation, reflectance recovery, and Munsell notation. ColorAide's `everything.ColorAll` carries the per-color presentation legs: gamut mapping, filters, smooth and categorical palettes, harmony, blending, Porter-Duff composition, Planckian swatches, perceptual difference, contrast, and CSS egress. colour-cxf supplies the inbound CxF3 spot-library graph. `ColorOp` and `Derivation` are closed `@tagged_union` families: operation admission rejects invalid source-model, route, arity, and empty-payload combinations, while each result case carries only its operation's evidence and exposes `coords` as the one common projection.

Color arity is ruled at two pages: measurement, the `Metric` family, and spectral resampling are derive owner blocks, while CxF device-half intake, plate authoring, TAC gating, LUT authoring, and raster egress consolidate in `graphic/color/managed#MANAGED`. This page owns `AdaptMethod`, `BlendMode`, `PorterDuff`, the shared `Palette` carrier, and `hex_ramp`; `graphic/layer#LAYER` and `graphic/style#STYLE` compose those values downward. Each `_METRIC` row binds its measure to the `ColorModel` its engine demands. Refined `Annotated` contracts admit scalar, vector, and matrix invariants, while every field-spanning or engine-route factory returns `Result[ColorOp, DeriveFault]`. Every arm resolves through `self.lane.offload` as a `KernelTrait.HOSTILE` kernel — colour-science's numpy-band natives never import inside an isolated subinterpreter, so the warm process pool is the one isolation that keeps the process-global `domain_range_scale` state worker-local — and carries provider faults on `RuntimeRail`.

## [01]-[INDEX]

- [01]-[DERIVE]: the two-engine `Colorimetry` owner over the closed-payload `ColorOp` family — `convert`/`adapt`/`gamut`/`filter`/`simulate`/`palette`/`compose`/`temperature`/`measure`/`correct`/`spot`/`recover`/`notate` fold into the closed `Derivation` result family whose cases carry operation-specific evidence and whose `coords` projection is total; the dual-name `ColorModel` vocabulary, the `Metric`/`_METRIC` measure table, the canonical `BlendMode`/`PorterDuff` policy vocabularies, and the shared `Palette` carrier plus the `hex_ramp` and `cxf_book` egress projections.

## [02]-[DERIVE]

- Cases: `ColorOp` routes `Convert` through `colour.convert`, `wavelength_to_XYZ`, `msds_to_XYZ`, or the ColorAide gateway; `Adapt` through `chromatic_adaptation`; `Gamut` through `Color.fit` or `fit_pointer_gamut`; `Filter` through `Color.filter`; `Simulate` through `matrix_cvd_Machado2009`; `Palette` through `Color.average`, `steps`, `discrete`, `harmony`, and `closest`; `Compose` through `mask`, `layer`, or `weighted_mix`; `Temperature` through the CCT pair; `Measure` through `_METRIC`; `Correct` through `colour_correction`; `Spot` through CxF spectral, Lab, or XYZ intake; `Recover` through `XYZ_to_sd`; and `Notate` through the Munsell pair. `Derivation.gamut` alone carries `GamutFit`, evidence-bearing cases carry their `frozendict[Metric, float]`, and value-only cases cannot construct either payload.
- Auto: every arm resolves inside `self.lane.offload(Kernel.of(Colorimetry._resolved, KernelTrait.HOSTILE), op)` — a process worker runs one kernel at a time, so the non-thread-local `domain_range_scale` global never races, and the runtime lane owns capacity and provider-fault conversion; the numpy-band `colour`/`coloraide` imports bar the subinterpreter arm outright. `Convert` folds the source through `_resolve` under `domain_range_scale("reference")`, and its admission rail proves source-model agreement plus an engine route before dispatch; every spectral ingress passes `_aligned` onto `_WORKING_SHAPE`, lands in XYZ, and `_landed` rides the ColorAide gateway for an aide-only target, so a wavelength or measured spectrum converts into every non-spectral `ColorModel`. `Measure` admits a single `SpectralDistribution` for spectral-only metrics, rejects a mismatched source model, and resolves the `Option`-carried reference into the `_METRIC[metric]` row's input model. Each `Derivation` case stores only its payload's notation, path, measures, or `GamutFit` witness. `hex_ramp` is the one sRGB-array-to-CSS-hex projection.
- Growth: a new operation is one `ColorOp` case plus one acceptor arm folding into `Derivation`; a new colorimetric measure one `Metric` member plus one `_METRIC` `MetricSpec` row carrying its input `ColorModel` and callable; a new filter or CVD type one `ColorFilter` row, a new graded deficiency one `Deficiency` row; a new gamut-fit method one `FitMethod` row; a new palette modality one `Ramp` case, a new interpolation curve one `Interp` row (the mixbox `spectral` pigment curves are those), a new hue arc or easing one `HueArc`/`Easing` row; a new blend or operator one `BlendMode`/`PorterDuff` row every downstream lowering composes by name; a new harmony wheel one `Harmony` row; a new appearance or named model one `ColorModel` row carrying both engine columns (the ZCAM/Hellwig-2022/Kim-2009/RLAB/Hunt roster is that); a new CCT/Planckian/adaptation method one `CctMethod`/`Blackbody`/`AdaptMethod`/`CamMethod` row; a new white point or observer one `Illuminant`/`Observer` row keying `_WHITEPOINT`; a new correction solver one `CorrectMethod` row; a new reflectance-recovery solver one `RecoverMethod` row; a new inbound exchange intake one `ColorOp` case decoding its wire (the `Spot` CxF seed is that); a new palette egress one projection beside `hex_ramp` (the `cxf_book` CxF3 ink-book serialization is that — the `Spot` intake's inverse, `write_cxf` over the authored `cxf3.CxF` graph); a finer working grid the one `_WORKING_SHAPE` row; zero new surface.
- Boundary: no chart or scene rendering, ICC/LUT/CCTF raster egress, plate authoring, TAC gate, or CxF device-half. `graphic/color/managed#MANAGED` owns those operations, and derive imports no `pillow`. Derive mints no receipt, owns no folder-local limiter, and admits no tag-plus-default result bag. colour-science owns spectral, appearance, CIE difference, adaptation, temperature, index, CCM, CVD-matrix, recovery, and Munsell operations; ColorAide owns gamut fitting, filters, compositing, palettes, harmony, blackbody, perceptual difference, contrast, and CSS notation; colour-cxf contributes the color half of CxF intake and the `cxf_book` authoring egress, so the ink lifecycle closes at derive — measured or derived spot colors leave as CxF3 the same measurement-context grammar the intake resolves.
- Packages: `colour-science` (the colorimetric-truth engine — `convert`/`chromatic_adaptation`/`delta_E`/`temperature`/colorimetric-index/`colour_correction`/`matrix_cvd_Machado2009`/`XYZ_to_sd`/`munsell_colour_to_xyY` and the `SpectralDistribution`+`align` resample surface per the fence imports), `coloraide` (`everything.ColorAll`, the all-plugins engine — gamut fit, CVD+W3C filters, palette interpolation including the mixbox pigment curves, mask/layer/weighted-mix compositing, harmony, blackbody, OKLab-perceptual difference, WCAG contrast, CSS notation), `colour-cxf` (`read_cxf` → the `cxf3.CxF` graph for the CxF3 spot-library intake; `write_cxf` over the built `cxf3` dataclass graph — `Object`/`ColorSpecification`/`ReflectanceSpectrum`/`ColorCielab` — for the `cxf_book` egress), plus `expression`/`numpy`/`beartype` and the runtime `LanePolicy`/`Kernel`/`KernelTrait`; the full member surface lives in the package `.api` catalogs.

```python signature
# --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
from collections.abc import Callable
from dataclasses import dataclass
from enum import Enum, StrEnum
from typing import Annotated, Final, Literal, NamedTuple, assert_never

import colour
import numpy as np
from beartype import beartype
from beartype.vale import Is
from builtins import frozendict
from coloraide import ease, ease_in, ease_in_out, ease_out, linear
from coloraide.everything import ColorAll as Color
from colour import MultiSpectralDistributions, SpectralDistribution, SpectralShape
from expression import Error, Nothing, Ok, Option, Result, case, tag, tagged_union
from expression.collections import Map
from numpy.typing import NDArray

from rasm.runtime.faults import RuntimeRail
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

lazy from colour_cxf import (
    cxf3,
    read_cxf,
    write_cxf,
)

# --- [TYPES] ------------------------------------------------------------------------------
type Tristimulus = Annotated[
    NDArray[np.float64],
    Is[lambda value: value.ndim == 1 and value.shape == (3,) and bool(np.isfinite(value).all())],
]
type ColorMatrix = Annotated[
    NDArray[np.float64],
    Is[lambda value: value.ndim == 2 and value.shape[1] == 3 and value.shape[0] > 0 and bool(np.isfinite(value).all())],
]
type Coordinates = NDArray[np.float64]
type Palette = ColorMatrix
type Wavelength = Annotated[float, Is[lambda nm: 360.0 <= nm <= 830.0]]
type ColorSource = SpectralDistribution | MultiSpectralDistributions | Tristimulus | Wavelength
type MetricInput = Tristimulus | SpectralDistribution
type Kelvin = Annotated[float, Is[lambda k: k > 0.0]]
type Chromaticity = tuple[float, float]
type CctSource = Kelvin | Chromaticity
type Amount = Annotated[float, Is[lambda a: 0.0 <= a <= 1.0]]
type Severity = Annotated[float, Is[lambda s: 0.0 <= s <= 1.0]]
type PaletteCount = Annotated[int, Is[lambda n: n >= 1]]
type Spacing = Annotated[float, Is[lambda s: s >= 0.0]]
type ColorText = Annotated[str, Is[lambda value: Color.match(value, fullmatch=True) is not None]]
type NotateSource = str | Tristimulus
type ColorOpTag = Literal[
    "convert", "adapt", "gamut", "filter", "simulate", "palette", "compose", "temperature", "measure", "correct", "spot", "recover", "notate"
]
type Weight = Annotated[float, Is[lambda value: 0.0 <= value <= 1.0]]
type DeriveFault = Literal[
    "<empty-colors>",
    "<empty-document>",
    "<empty-notation>",
    "<empty-palette>",
    "<matrix-arity>",
    "<non-spectral-sample>",
    "<source-model>",
    "<unsupported-route>",
    "<weights-arity>",
]


class ModelNames(NamedTuple):
    science: str | None
    aide: str | None
    spectral: bool = False


class ColorModel(ModelNames, Enum):
    SPECTRAL = ModelNames("Spectral Distribution", None, spectral=True)
    XYZ = ModelNames("CIE XYZ", "xyz-d65")
    XYY = ModelNames("CIE xyY", "xyy")
    LAB = ModelNames("CIE Lab", "lab-d65")
    LCHAB = ModelNames("CIE LCHab", "lch-d65")
    OKLAB = ModelNames("Oklab", "oklab")
    OKLCH = ModelNames("Oklch", "oklch")
    SRGB = ModelNames("sRGB", "srgb")
    SRGB_LINEAR = ModelNames(None, "srgb-linear")
    ICTCP = ModelNames("ICtCp", "ictcp")
    DISPLAY_P3 = ModelNames(None, "display-p3")
    DISPLAY_P3_LINEAR = ModelNames(None, "display-p3-linear")
    A98_RGB = ModelNames(None, "a98-rgb")
    A98_RGB_LINEAR = ModelNames(None, "a98-rgb-linear")
    PROPHOTO_RGB = ModelNames(None, "prophoto-rgb")
    PROPHOTO_RGB_LINEAR = ModelNames(None, "prophoto-rgb-linear")
    REC2020 = ModelNames(None, "rec2020")
    REC2020_LINEAR = ModelNames(None, "rec2020-linear")
    REC2100_PQ = ModelNames(None, "rec2100-pq")
    REC2100_HLG = ModelNames(None, "rec2100-hlg")
    REC2100_LINEAR = ModelNames(None, "rec2100-linear")
    HSV = ModelNames("HSV", "hsv")
    HSL = ModelNames(None, "hsl")
    HWB = ModelNames(None, "hwb")
    JZAZBZ = ModelNames("Jzazbz", "jzazbz")
    JZCZHZ = ModelNames(None, "jzczhz")
    CMYK = ModelNames(None, "cmyk")
    HCT = ModelNames(None, "hct")
    LUV = ModelNames("CIE Luv", "luv")
    LCHUV = ModelNames(None, "lchuv")
    HSLUV = ModelNames(None, "hsluv")
    HPLUV = ModelNames(None, "hpluv")
    OKHSL = ModelNames(None, "okhsl")
    OKHSV = ModelNames(None, "okhsv")
    ACES2065_1 = ModelNames(None, "aces2065-1")
    ACESCC = ModelNames(None, "acescc")
    ACESCCT = ModelNames(None, "acescct")
    ACESCG = ModelNames(None, "acescg")
    CIECAM02 = ModelNames("CIECAM02", None)
    CAM16 = ModelNames("CAM16", None)
    ZCAM = ModelNames("ZCAM", None)
    HELLWIG2022 = ModelNames("Hellwig 2022", None)
    KIM2009 = ModelNames("Kim 2009", None)
    RLAB = ModelNames("RLAB", None)
    HUNT = ModelNames("Hunt", None)


class Observer(StrEnum):
    CIE_1931_2 = "CIE 1931 2 Degree Standard Observer"
    CIE_1964_10 = "CIE 1964 10 Degree Standard Observer"


class Illuminant(StrEnum):
    D65 = "D65"
    D50 = "D50"
    D55 = "D55"
    D75 = "D75"
    A = "A"
    E = "E"


class AdaptMethod(StrEnum):
    VON_KRIES = "Von Kries"
    BRADFORD = "Bradford"
    CAT02 = "CAT02"
    CAT16 = "CAT16"
    CMCCAT2000 = "CMCCAT2000"
    SHARP = "Sharp"


class CamMethod(StrEnum):
    CIE_1994 = "CIE 1994"
    CMCCAT2000 = "CMCCAT2000"
    FAIRCHILD_1990 = "Fairchild 1990"
    VON_KRIES = "Von Kries"
    VK20 = "vK20"
    ZHAI_2018 = "Zhai 2018"
    LI_2025 = "Li 2025"


class FitMethod(StrEnum):
    RAYTRACE = "raytrace"
    OKLCH_CHROMA = "oklch-chroma"
    OKLCH_CUBIC = "oklch-cubic"
    HCT_CHROMA = "hct-chroma"
    LCH_CHROMA = "lch-chroma"
    MINDE_CHROMA = "minde-chroma"
    SCALE = "scale"
    SCALE_LUMINANCE = "scale-luminance"
    POINTER = "pointer"


class ColorFilter(StrEnum):
    PROTAN = "protan"
    DEUTAN = "deutan"
    TRITAN = "tritan"
    BRIGHTNESS = "brightness"
    CONTRAST = "contrast"
    SATURATE = "saturate"
    HUE_ROTATE = "hue-rotate"
    GRAYSCALE = "grayscale"
    SEPIA = "sepia"
    INVERT = "invert"
    OPACITY = "opacity"


class Deficiency(StrEnum):
    PROTANOMALY = "Protanomaly"
    DEUTERANOMALY = "Deuteranomaly"
    TRITANOMALY = "Tritanomaly"


class Harmony(StrEnum):
    MONOCHROMATIC = "mono"
    COMPLEMENTARY = "complement"
    SPLIT_COMPLEMENTARY = "split"
    ANALOGOUS = "analogous"
    TRIADIC = "triad"
    SQUARE = "square"
    RECTANGLE = "rectangle"
    WHEEL = "wheel"


class Interp(StrEnum):
    LINEAR = "linear"
    CONTINUOUS = "continuous"
    BSPLINE = "bspline"
    NATURAL = "natural"
    MONOTONE = "monotone"
    CSS_LINEAR = "css-linear"
    CATROM = "catrom"
    SPECTRAL = "spectral"
    SPECTRAL_CONTINUOUS = "spectral-continuous"


class HueArc(StrEnum):
    SHORTER = "shorter"
    LONGER = "longer"
    INCREASING = "increasing"
    DECREASING = "decreasing"
    SPECIFIED = "specified"


class Easing(StrEnum):
    LINEAR = "linear"
    EASE = "ease"
    EASE_IN = "ease-in"
    EASE_OUT = "ease-out"
    EASE_IN_OUT = "ease-in-out"


class BlendMode(StrEnum):
    NORMAL = "normal"
    MULTIPLY = "multiply"
    SCREEN = "screen"
    OVERLAY = "overlay"
    DARKEN = "darken"
    LIGHTEN = "lighten"
    COLOR_DODGE = "color-dodge"
    COLOR_BURN = "color-burn"
    HARD_LIGHT = "hard-light"
    SOFT_LIGHT = "soft-light"
    DIFFERENCE = "difference"
    EXCLUSION = "exclusion"
    HUE = "hue"
    SATURATION = "saturation"
    COLOR = "color"
    LUMINOSITY = "luminosity"


class PorterDuff(StrEnum):
    CLEAR = "clear"
    COPY = "copy"
    DESTINATION = "destination"
    SOURCE_OVER = "source-over"
    DESTINATION_OVER = "destination-over"
    SOURCE_IN = "source-in"
    DESTINATION_IN = "destination-in"
    SOURCE_OUT = "source-out"
    DESTINATION_OUT = "destination-out"
    SOURCE_ATOP = "source-atop"
    DESTINATION_ATOP = "destination-atop"
    XOR = "xor"
    LIGHTER = "lighter"
    PLUS_LIGHTER = "plus-lighter"
    PLUS_DARKER = "plus-darker"


class CctMethod(StrEnum):
    DAYLIGHT = "CIE Illuminant D Series"
    KANG = "Kang 2002"
    HERNANDEZ = "Hernandez 1999"
    MCCAMY = "McCamy 1992"


class Blackbody(StrEnum):
    OHNO_2013 = "ohno-2013"
    ROBERTSON_1968 = "robertson-1968"


class CorrectMethod(StrEnum):
    CHEUNG_2004 = "Cheung 2004"
    FINLAYSON_2015 = "Finlayson 2015"
    VANDERMONDE = "Vandermonde"


class RecoverMethod(StrEnum):
    JAKOB_2019 = "Jakob 2019"
    MALLETT_2019 = "Mallett 2019"
    MENG_2015 = "Meng 2015"
    OTSU_2018 = "Otsu 2018"
    SMITS_1999 = "Smits 1999"


class Metric(StrEnum):
    DELTA_E_2000 = "deltaE-2000"
    DELTA_E_1994 = "deltaE-1994"
    DELTA_E_1976 = "deltaE-1976"
    DELTA_E_CMC = "deltaE-CMC"
    DELTA_E_DIN99 = "deltaE-DIN99"
    DELTA_E_OK = "deltaE-ok"
    DELTA_E_JZ = "deltaE-jz"
    DELTA_E_HYAB = "deltaE-hyab"
    DELTA_E_ITP = "deltaE-itp"
    DELTA_E_99O = "deltaE-99o"
    DELTA_E_CAM02 = "deltaE-cam02"
    DELTA_E_CAM16 = "deltaE-cam16"
    DELTA_E_HCT = "deltaE-hct"
    DELTA_E_HELMLAB = "deltaE-helmlab"
    DISTANCE = "distance"
    CONTRAST = "contrast"
    CONTRAST_LSTAR = "contrast-lstar"
    LUMINANCE = "luminance"
    WHITENESS = "whiteness"
    YELLOWNESS = "yellowness"
    DOMINANT_WAVELENGTH = "dominant-wavelength"
    COMPLEMENTARY_WAVELENGTH = "complementary-wavelength"
    CRI = "cri"
    CFI = "cfi"
    CCT = "cct"
    DUV = "duv"
    CHROMATICITY_X = "chromaticity-x"
    CHROMATICITY_Y = "chromaticity-y"
    SEVERITY = "severity"


class MetricSpec(NamedTuple):
    space: ColorModel
    fn: Callable[[MetricInput, MetricInput], float]

    @property
    def spectral(self) -> bool:
        return self.space is ColorModel.SPECTRAL


# --- [CONSTANTS] --------------------------------------------------------------------------
_WORKING_SHAPE: Final[SpectralShape] = SpectralShape(360.0, 830.0, 1.0)


# --- [MODELS] -----------------------------------------------------------------------------
@dataclass(frozen=True, slots=True, kw_only=True)
class GamutFit:
    in_gamut: bool
    pointer: bool


type ValuePayload = tuple[Coordinates, tuple[str, ...], str, tuple[str, ...]]
type EvidencePayload = tuple[Coordinates, tuple[str, ...], str, tuple[str, ...], frozendict[Metric, float]]
type GamutPayload = tuple[ValuePayload, GamutFit]


@tagged_union(frozen=True)
class Derivation:
    tag: ColorOpTag = tag()
    convert: ValuePayload = case()
    adapt: ValuePayload = case()
    gamut: GamutPayload = case()
    filter: EvidencePayload = case()
    simulate: EvidencePayload = case()
    palette: EvidencePayload = case()
    compose: ValuePayload = case()
    temperature: EvidencePayload = case()
    measure: EvidencePayload = case()
    correct: ValuePayload = case()
    spot: ValuePayload = case()
    recover: ValuePayload = case()
    notate: ValuePayload = case()

    @property
    def coords(self) -> Coordinates:
        match self:
            case Derivation(tag="convert", convert=(coords, _, _, _)) | Derivation(tag="adapt", adapt=(coords, _, _, _)) | Derivation(tag="compose", compose=(coords, _, _, _)):
                return coords
            case Derivation(tag="correct", correct=(coords, _, _, _)) | Derivation(tag="spot", spot=(coords, _, _, _)) | Derivation(tag="recover", recover=(coords, _, _, _)):
                return coords
            case Derivation(tag="notate", notate=(coords, _, _, _)):
                return coords
            case Derivation(tag="filter", filter=(coords, _, _, _, _)) | Derivation(tag="simulate", simulate=(coords, _, _, _, _)):
                return coords
            case Derivation(tag="palette", palette=(coords, _, _, _, _)) | Derivation(tag="temperature", temperature=(coords, _, _, _, _)):
                return coords
            case Derivation(tag="measure", measure=(coords, _, _, _, _)):
                return coords
            case Derivation(tag="gamut", gamut=((coords, _, _, _), _)):
                return coords
            case _ as unreachable:
                assert_never(unreachable)


# --- [TABLES] -----------------------------------------------------------------------------
_WHITEPOINT: Final[Map[Observer, Map[Illuminant, Tristimulus]]] = Map.of_seq(
    (
        observer,
        Map.of_seq(
            (illum, np.asarray(colour.xy_to_XYZ(colour.CCS_ILLUMINANTS[observer.value][illum.value]), dtype=np.float64)) for illum in Illuminant
        ),
    )
    for observer in Observer
)

_CXF_OBSERVER: Final[Map[str, Observer]] = Map.of_seq([("2_Degree", Observer.CIE_1931_2), ("10_Degree", Observer.CIE_1964_10)])
_CXF_ILLUMINANT: Final[Map[str, Illuminant]] = Map.of_seq((illum.value, illum) for illum in Illuminant)
_OBSERVER_CXF: Final[frozendict[Observer, str]] = frozendict({member: token for token, member in _CXF_OBSERVER.items()})
_CXF_EXPORT_GRID: Final[SpectralShape] = SpectralShape(360.0, 830.0, 10.0)

_EASING: Final[Map[Easing, Callable[[float], float]]] = Map.of_seq([
    (Easing.LINEAR, linear),
    (Easing.EASE, ease),
    (Easing.EASE_IN, ease_in),
    (Easing.EASE_OUT, ease_out),
    (Easing.EASE_IN_OUT, ease_in_out),
])

_METRIC: Final[Map[Metric, MetricSpec]] = Map.of_seq([
    (Metric.DELTA_E_2000, MetricSpec(ColorModel.LAB, lambda a, b: float(colour.delta_E(b, a, method="CIE 2000")))),
    (Metric.DELTA_E_1994, MetricSpec(ColorModel.LAB, lambda a, b: float(colour.delta_E(b, a, method="CIE 1994")))),
    (Metric.DELTA_E_1976, MetricSpec(ColorModel.LAB, lambda a, b: float(colour.delta_E(b, a, method="CIE 1976")))),
    (Metric.DELTA_E_CMC, MetricSpec(ColorModel.LAB, lambda a, b: float(colour.delta_E(b, a, method="CMC")))),
    (Metric.DELTA_E_DIN99, MetricSpec(ColorModel.LAB, lambda a, b: float(colour.delta_E(b, a, method="DIN99")))),
    (Metric.DELTA_E_OK, MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).delta_e(Color("srgb", list(b)), method="ok"))),
    (Metric.DELTA_E_JZ, MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).delta_e(Color("srgb", list(b)), method="jz"))),
    (Metric.DELTA_E_HYAB, MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).delta_e(Color("srgb", list(b)), method="hyab"))),
    (Metric.DELTA_E_ITP, MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).delta_e(Color("srgb", list(b)), method="itp"))),
    (Metric.DELTA_E_99O, MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).delta_e(Color("srgb", list(b)), method="99o"))),
    (Metric.DELTA_E_CAM02, MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).delta_e(Color("srgb", list(b)), method="cam02"))),
    (Metric.DELTA_E_CAM16, MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).delta_e(Color("srgb", list(b)), method="cam16"))),
    (Metric.DELTA_E_HCT, MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).delta_e(Color("srgb", list(b)), method="hct"))),
    (Metric.DELTA_E_HELMLAB, MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).delta_e(Color("srgb", list(b)), method="helmlab"))),
    (Metric.DISTANCE, MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).distance(Color("srgb", list(b)), space="lab"))),
    (Metric.CONTRAST, MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).contrast(Color("srgb", list(b))))),
    (Metric.CONTRAST_LSTAR, MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).contrast(Color("srgb", list(b)), method="lstar"))),
    (Metric.LUMINANCE, MetricSpec(ColorModel.SRGB, lambda a, _b: Color("srgb", list(a)).luminance())),
    (
        Metric.WHITENESS,
        MetricSpec(ColorModel.XYZ, lambda a, b: float(np.ravel(colour.whiteness(a, b))[0])),
    ),
    (Metric.YELLOWNESS, MetricSpec(ColorModel.XYZ, lambda a, _b: float(colour.yellowness(a)))),
    (
        Metric.DOMINANT_WAVELENGTH,
        MetricSpec(ColorModel.XYY, lambda a, b: float(np.ravel(colour.dominant_wavelength(a[:2], b[:2])[0])[0])),
    ),
    (Metric.COMPLEMENTARY_WAVELENGTH, MetricSpec(ColorModel.XYY, lambda a, b: float(np.ravel(colour.complementary_wavelength(a[:2], b[:2])[0])[0]))),
    (Metric.CRI, MetricSpec(ColorModel.SPECTRAL, lambda a, _b: float(colour.colour_rendering_index(a)))),
    (Metric.CFI, MetricSpec(ColorModel.SPECTRAL, lambda a, _b: float(colour.colour_fidelity_index(a)))),
    (Metric.CCT, MetricSpec(ColorModel.SRGB, lambda a, _b: float(Color("srgb", list(a)).cct()[0]))),
    (Metric.DUV, MetricSpec(ColorModel.SRGB, lambda a, _b: float(Color("srgb", list(a)).cct()[1]))),
    (Metric.CHROMATICITY_X, MetricSpec(ColorModel.XYY, lambda a, _b: float(a[0]))),
    (Metric.CHROMATICITY_Y, MetricSpec(ColorModel.XYY, lambda a, _b: float(a[1]))),
    (Metric.SEVERITY, MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).delta_e(Color("srgb", list(b)), method="2000"))),
])


@tagged_union(frozen=True)
class Ramp:
    tag: Literal["smooth", "discrete", "harmony"] = tag()
    smooth: tuple[Interp, HueArc, Easing] = case()
    discrete: tuple[Interp, HueArc, Easing] = case()
    harmony: Harmony = case()

    @staticmethod
    def Smooth(interp: Interp = Interp.BSPLINE, hue: HueArc = HueArc.SHORTER, easing: Easing = Easing.LINEAR) -> "Ramp":
        return Ramp(smooth=(interp, hue, easing))

    @staticmethod
    def Discrete(interp: Interp = Interp.LINEAR, hue: HueArc = HueArc.SHORTER, easing: Easing = Easing.LINEAR) -> "Ramp":
        return Ramp(discrete=(interp, hue, easing))

    @staticmethod
    def Harmony(name: Harmony = Harmony.ANALOGOUS) -> "Ramp":
        return Ramp(harmony=name)


@tagged_union(frozen=True)
class ColorOp:
    tag: ColorOpTag = tag()
    convert: tuple[ColorSource, ColorModel, ColorModel, AdaptMethod, Observer] = case()
    adapt: tuple[Tristimulus, Illuminant, Illuminant, CamMethod, Observer] = case()
    gamut: tuple[Tristimulus, ColorModel, ColorModel, FitMethod] = case()
    filter: tuple[Tristimulus, ColorFilter, Amount] = case()
    simulate: tuple[Tristimulus, Deficiency, Severity] = case()
    palette: tuple[tuple[ColorText, ...], ColorText, PaletteCount, Spacing, ColorModel, Ramp, tuple[ColorText, ...]] = case()
    compose: tuple[tuple[ColorText, ...], ColorModel, BlendMode, PorterDuff, tuple[Weight, ...], tuple[str, ...]] = case()
    temperature: tuple[CctSource, CctMethod, Blackbody, ColorModel] = case()
    measure: tuple[ColorSource, ColorModel, Option[Tristimulus], Metric, Observer] = case()
    correct: tuple[ColorMatrix, ColorMatrix, ColorMatrix, CorrectMethod] = case()
    spot: tuple[bytes, ColorModel] = case()
    recover: tuple[Tristimulus, ColorModel, RecoverMethod, Observer] = case()
    notate: NotateSource = case()

    @staticmethod
    @beartype
    def Convert(
        value: ColorSource,
        source: ColorModel,
        target: ColorModel,
        adapt: AdaptMethod = AdaptMethod.BRADFORD,
        observer: Observer = Observer.CIE_1931_2,
    ) -> Result["ColorOp", DeriveFault]:
        measured = isinstance(value, SpectralDistribution | MultiSpectralDistributions)
        spectral = measured or isinstance(value, float)
        route = (
            (source.science is not None and target.science is not None)
            or (source.aide is not None and target.aide is not None)
            or (spectral and (target.science is not None or target.aide is not None))
        )
        return (
            Error("<source-model>")
            if spectral is not source.spectral
            else Error("<unsupported-route>")
            if target.spectral and not measured
            else Ok(ColorOp(convert=(value, source, target, adapt, observer)))
            if route or target.spectral
            else Error("<unsupported-route>")
        )

    @staticmethod
    @beartype
    def Adapt(
        value: Tristimulus, source: Illuminant, target: Illuminant, method: CamMethod = CamMethod.VON_KRIES, observer: Observer = Observer.CIE_1931_2
    ) -> Result["ColorOp", DeriveFault]:
        # `_WHITEPOINT` is total over Observer x Illuminant by construction, so admission holds no residual
        # refusal today; the rail is uniform with every sibling factory so a future refusal lands with zero caller churn.
        return Ok(ColorOp(adapt=(value, source, target, method, observer)))

    @staticmethod
    @beartype
    def Gamut(
        value: Tristimulus, source: ColorModel, target: ColorModel, method: FitMethod = FitMethod.OKLCH_CHROMA
    ) -> Result["ColorOp", DeriveFault]:
        return (
            Ok(ColorOp(gamut=(value, source, target, method)))
            if source.aide is not None and target.aide is not None
            else Error("<unsupported-route>")
        )

    @staticmethod
    @beartype
    def Filter(value: Tristimulus, name: ColorFilter, amount: Amount = 1.0) -> Result["ColorOp", DeriveFault]:
        return Ok(ColorOp(filter=(value, name, amount)))

    @staticmethod
    @beartype
    def Simulate(value: Tristimulus, deficiency: Deficiency, severity: Severity = 1.0) -> Result["ColorOp", DeriveFault]:
        return Ok(ColorOp(simulate=(value, deficiency, severity)))

    @staticmethod
    @beartype
    def Palette(
        seed: tuple[ColorText, ...],
        stop: ColorText,
        count: PaletteCount,
        spacing: Spacing = 0.0,
        space: ColorModel = ColorModel.OKLCH,
        ramp: Ramp = Ramp.Smooth(),
        anchors: tuple[ColorText, ...] = (),
    ) -> Result["ColorOp", DeriveFault]:
        return (
            Ok(ColorOp(palette=(seed, stop, count, spacing, space, ramp, anchors)))
            if seed and stop and space.aide is not None
            else Error("<empty-palette>")
            if not seed or not stop
            else Error("<unsupported-route>")
        )

    @staticmethod
    @beartype
    def Compose(
        colors: tuple[ColorText, ...],
        space: ColorModel = ColorModel.OKLCH,
        blend: BlendMode = BlendMode.NORMAL,
        operator: PorterDuff = PorterDuff.SOURCE_OVER,
        weights: tuple[Weight, ...] = (),
        mask: tuple[str, ...] = (),
    ) -> Result["ColorOp", DeriveFault]:
        return (
            Error("<empty-colors>")
            if not colors
            else Error("<weights-arity>")
            if weights and len(weights) != len(colors)
            else Ok(ColorOp(compose=(colors, space, blend, operator, weights, mask)))
            if space.aide is not None
            else Error("<unsupported-route>")
        )

    @staticmethod
    @beartype
    def Temperature(
        value: CctSource, method: CctMethod = CctMethod.DAYLIGHT, planck: Blackbody = Blackbody.OHNO_2013, space: ColorModel = ColorModel.SRGB
    ) -> Result["ColorOp", DeriveFault]:
        return Ok(ColorOp(temperature=(value, method, planck, space))) if space.aide is not None else Error("<unsupported-route>")

    @staticmethod
    @beartype
    def Measure(
        sample: SpectralDistribution | Tristimulus,
        metric: Metric,
        source: ColorModel = ColorModel.SRGB,
        reference: Option[Tristimulus] = Nothing,
        observer: Observer = Observer.CIE_1931_2,
    ) -> Result["ColorOp", DeriveFault]:
        spectral_sample = isinstance(sample, SpectralDistribution)
        return (
            Error("<non-spectral-sample>")
            if _METRIC[metric].spectral and not spectral_sample
            else Error("<source-model>")
            if spectral_sample is not source.spectral
            else Ok(ColorOp(measure=(sample, source, reference, metric, observer)))
        )

    @staticmethod
    @beartype
    def Correct(
        colors: ColorMatrix,
        measured: ColorMatrix,
        reference: ColorMatrix,
        method: CorrectMethod = CorrectMethod.CHEUNG_2004,
    ) -> Result["ColorOp", DeriveFault]:
        return (
            Ok(ColorOp(correct=(colors, measured, reference, method)))
            if measured.shape == reference.shape
            else Error("<matrix-arity>")
        )

    @staticmethod
    @beartype
    def Spot(document: bytes, target: ColorModel = ColorModel.SRGB) -> Result["ColorOp", DeriveFault]:
        return (
            Error("<empty-document>")
            if not document
            else Error("<unsupported-route>")
            if target.science is None or target.spectral
            else Ok(ColorOp(spot=(document, target)))
        )

    @staticmethod
    @beartype
    def Recover(
        seed: Tristimulus, source: ColorModel = ColorModel.SRGB, method: RecoverMethod = RecoverMethod.MENG_2015, observer: Observer = Observer.CIE_1931_2
    ) -> Result["ColorOp", DeriveFault]:
        # seed is a tristimulus, so a spectral source model contradicts it and an engine-less model
        # cannot reach the XYZ hop `_resolved`'s recover arm takes.
        return (
            Error("<source-model>")
            if source.spectral
            else Error("<unsupported-route>")
            if source.science is None and source.aide is None
            else Ok(ColorOp(recover=(seed, source, method, observer)))
        )

    @staticmethod
    @beartype
    def Notate(value: NotateSource) -> Result["ColorOp", DeriveFault]:
        return Error("<empty-notation>") if isinstance(value, str) and not value.strip() else Ok(ColorOp(notate=value))


# --- [SERVICES] ---------------------------------------------------------------------------
@dataclass(frozen=True, slots=True, kw_only=True)
class Colorimetry:
    op: ColorOp
    lane: LanePolicy

    async def derive(self) -> RuntimeRail[Derivation]:
        # synchronous colour-science/ColorAide CPU kernels: numpy-band natives bar the subinterpreter arm, so the
        # HOSTILE process pool owns isolation — one kernel per worker at a time keeps the non-thread-local
        # domain_range_scale global race-free; runtime owns the bound, no folder limiter.
        return await self.lane.offload(Kernel.of(Colorimetry._resolved, KernelTrait.HOSTILE), self.op)

    # --- [OPERATIONS]
    @staticmethod
    def _resolved(op: ColorOp) -> Derivation:
        match op:
            case ColorOp(tag="convert", convert=(value, source, target, adapt, observer)):
                with colour.domain_range_scale("reference"):
                    coords = np.asarray(Colorimetry._resolve(value, source, target, observer, adapt), dtype=np.float64)
                return Derivation(
                    convert=(coords, Colorimetry._notate(target.aide, coords), target.science or target.aide or "", Colorimetry._path(source, target))
                )
            case ColorOp(tag="adapt", adapt=(value, source, target, method, observer)):
                with colour.domain_range_scale("reference"):
                    adapted = np.asarray(
                        colour.chromatic_adaptation(
                            np.asarray(value, dtype=np.float64), _WHITEPOINT[observer][source], _WHITEPOINT[observer][target], method=method.value
                        ),
                        dtype=np.float64,
                    )
                return Derivation(
                    adapt=(adapted, Colorimetry._notate(ColorModel.XYZ.aide, adapted), "CIE XYZ", (observer.value, source.value, target.value, method.value))
                )
            case ColorOp(tag="gamut", gamut=(value, source, target, method)):
                seeded = Color(source.aide, list(value)).convert(target.aide)
                fitted = seeded.clone().fit_pointer_gamut() if method is FitMethod.POINTER else seeded.clone().fit(method=method.value)
                coords = np.asarray(fitted.coords(), dtype=np.float64)
                return Derivation(
                    gamut=(
                        (coords, (fitted.to_string(),), target.aide or "", Colorimetry._path(source, target)),
                        GamutFit(in_gamut=seeded.in_gamut(target.aide), pointer=seeded.in_pointer_gamut()),
                    )
                )
            case ColorOp(tag="filter", filter=(value, name, amount)):
                origin = Color("srgb", list(value))
                filtered = origin.clone().filter(name.value, amount=amount)
                coords = np.asarray(filtered.coords(), dtype=np.float64)
                return Derivation(
                    filter=(coords, (filtered.to_string(),), "srgb", (name.value, str(amount)), frozendict({Metric.SEVERITY: origin.delta_e(filtered, method="2000")}))
                )
            case ColorOp(tag="simulate", simulate=(value, deficiency, severity)):
                matrix = np.asarray(colour.matrix_cvd_Machado2009(deficiency.value, float(severity)), dtype=np.float64)
                shifted = np.asarray(matrix @ np.asarray(value, dtype=np.float64), dtype=np.float64)
                origin, simulated = Color("srgb", list(value)), Color("srgb", list(shifted))
                return Derivation(
                    simulate=(
                        shifted,
                        (simulated.to_string(),),
                        "srgb",
                        (deficiency.value, str(severity)),
                        frozendict({Metric.SEVERITY: origin.delta_e(simulated, method="2000")}),
                    )
                )
            case ColorOp(tag="palette", palette=(seed, stop, count, spacing, space, ramp, anchors)):
                base = Color.average(list(seed), space=space.aide, out_space="srgb")
                match ramp:
                    case Ramp(tag="smooth", smooth=(interp, hue, easing)):
                        ramped = Color.steps(
                            [base, stop],
                            steps=count,
                            max_steps=count,
                            max_delta_e=spacing,
                            delta_e="2000",
                            method=interp.value,
                            hue=hue.value,
                            progress=_EASING[easing],
                            space=space.aide,
                            out_space="srgb",
                        )
                        trail = (ramp.tag, interp.value, hue.value, easing.value)
                    case Ramp(tag="discrete", discrete=(interp, hue, easing)):
                        curve = Color.discrete(
                            [base, stop],
                            steps=count,
                            method=interp.value,
                            hue=hue.value,
                            progress=_EASING[easing],
                            space=space.aide,
                            out_space="srgb",
                        )
                        ramped = [curve(i / (count - 1)) if count > 1 else curve(0.0) for i in range(count)]
                        trail = (ramp.tag, interp.value, hue.value, easing.value)
                    case Ramp(tag="harmony", harmony=name):
                        ramped = base.harmony(name.value, space=space.aide, out_space="srgb")
                        trail = (ramp.tag, name.value)
                    case _ as unreachable:
                        assert_never(unreachable)
                snapped = [step.closest(list(anchors), method="2000") for step in ramped] if anchors else ramped
                coords = np.array([step.coords() for step in snapped], dtype=np.float64)
                contrast = float(Color("srgb", list(coords[0])).contrast(Color("srgb", list(coords[-1]))))
                return Derivation(
                    palette=(
                        coords,
                        tuple(step.to_string() for step in snapped),
                        "srgb",
                        (space.aide or "", *trail),
                        frozendict({Metric.CONTRAST: contrast}),
                    )
                )
            case ColorOp(tag="compose", compose=(colors, space, blend, operator, weights, mask)):
                stacked = [Color(row).mask(list(mask)) for row in colors] if mask else list(colors)
                blended = (
                    Color.weighted_mix(stacked, weights=list(weights), space=space.aide, out_space="srgb")
                    if weights
                    else Color.layer(stacked, blend=blend.value, operator=operator.value, space=space.aide, out_space="srgb")
                )
                coords = np.asarray(blended.coords(), dtype=np.float64)
                return Derivation(compose=(coords, (blended.to_string(),), "srgb", (blend.value, operator.value)))
            case ColorOp(tag="temperature", temperature=(value, method, planck, space)):
                match value:
                    case float() as kelvin:
                        swatch = Color.blackbody(space.aide, kelvin, method=planck.value).convert("srgb")
                        chroma = np.asarray(colour.temperature.CCT_to_xy(kelvin, method=method.value), dtype=np.float64)
                        coords = np.asarray(swatch.coords(), dtype=np.float64)
                        return Derivation(
                            temperature=(
                                coords,
                                (swatch.to_string(),),
                                "srgb",
                                (method.value, planck.value),
                                frozendict({
                                Metric.CCT: kelvin,
                                Metric.CHROMATICITY_X: float(chroma[0]),
                                Metric.CHROMATICITY_Y: float(chroma[1]),
                                }),
                            )
                        )
                    case (x, y):
                        kelvin = float(np.ravel(colour.temperature.xy_to_CCT(np.asarray((x, y), dtype=np.float64), method=method.value))[0])
                        return Derivation(
                            temperature=(
                                np.asarray((x, y), dtype=np.float64),
                                (),
                                "CIE xyY",
                                (method.value,),
                                frozendict({Metric.CCT: kelvin, Metric.CHROMATICITY_X: x, Metric.CHROMATICITY_Y: y}),
                            )
                        )
                    case _ as unreachable:
                        assert_never(unreachable)
            case ColorOp(tag="measure", measure=(sample, source, reference, metric, observer)):
                space, fn = _METRIC[metric].space, _METRIC[metric].fn
                with colour.domain_range_scale("reference"):
                    a = Colorimetry._resolve(sample, source, space, observer)
                    b = reference.map(lambda ref: Colorimetry._resolve(ref, source, space, observer)).default_with(
                        lambda: Colorimetry._white(space, observer)
                    )
                    value = float(fn(a, b))
                coords = a if isinstance(a, np.ndarray) else np.empty(0, dtype=np.float64)
                return Derivation(measure=(coords, (), space.science or space.aide or "", (metric.value,), frozendict({metric: value})))
            case ColorOp(tag="correct", correct=(colors, measured, reference, method)):
                corrected = np.asarray(colour.colour_correction(colors, measured, reference, method=method.value), dtype=np.float64)
                return Derivation(correct=(corrected, Colorimetry._notate(ColorModel.SRGB.aide, corrected), "sRGB", (method.value,)))
            case ColorOp(tag="spot", spot=(document, target)):
                with colour.domain_range_scale("reference"):
                    swatches = Colorimetry._swatches(read_cxf(document), target)
                coords = np.array([row[0] for row in swatches], dtype=np.float64) if swatches else np.empty((0, 3), dtype=np.float64)
                return Derivation(
                    spot=(coords, tuple(row[1] for row in swatches), target.science or target.aide or "", ("cxf", *(row[2] for row in swatches)))
                )
            case ColorOp(tag="recover", recover=(seed, source, method, observer)):
                with colour.domain_range_scale("reference"):
                    xyz = np.asarray(Colorimetry._resolve(seed, source, ColorModel.XYZ, observer), dtype=np.float64)
                    sd = colour.XYZ_to_sd(xyz, method=method.value).align(_WORKING_SHAPE)
                return Derivation(recover=(np.asarray(sd.values, dtype=np.float64), (), "Spectral Distribution", (method.value,)))
            case ColorOp(tag="notate", notate=value):
                with colour.domain_range_scale("reference"):
                    match value:
                        case str() as munsell:
                            xyy = np.asarray(colour.munsell_colour_to_xyY(munsell), dtype=np.float64)
                            return Derivation(notate=(xyy, (munsell,), "CIE xyY", ("munsell",)))
                        case array:
                            name = str(colour.xyY_to_munsell_colour(np.asarray(array, dtype=np.float64)))
                            return Derivation(notate=(np.asarray(array, dtype=np.float64), (name,), "CIE xyY", ("munsell",)))
            case _:
                assert_never(op)

    @staticmethod
    def _aligned(value: ColorSource, /) -> ColorSource:
        # measurement-side resample: an arbitrary-grid measured spectrum aligns to the working shape
        # once at ingress (copy-then-align — colour mutates in place); non-spectral sources pass through.
        match value:
            case SpectralDistribution() | MultiSpectralDistributions():
                return value.copy().align(_WORKING_SHAPE)
            case other:
                return other

    @staticmethod
    def _resolve(
        value: ColorSource, source: ColorModel, target: ColorModel, observer: Observer, adapt: AdaptMethod = AdaptMethod.BRADFORD
    ) -> MetricInput | ColorMatrix | MultiSpectralDistributions:
        aligned = Colorimetry._aligned(value)
        if target.spectral:
            return aligned
        cmfs = colour.MSDS_CMFS[observer.value]
        match aligned:
            case float() as nm:
                return Colorimetry._landed(np.asarray(colour.wavelength_to_XYZ(nm, cmfs), dtype=np.float64), target)
            case MultiSpectralDistributions():
                return Colorimetry._landed(
                    np.asarray(colour.msds_to_XYZ(aligned, cmfs, colour.SDS_ILLUMINANTS[Illuminant.D65.value]), dtype=np.float64), target
                )
            case _ if source.aide is not None and target.aide is not None and (source.science is None or target.science is None):
                # a science=None endpoint (wide-gamut/HSL/HWB) colour-science has no node for rides the ColorAide
                # gateway so Convert is total over every ColorModel pair; coords enter in source.aide, leave in target.aide.
                return np.asarray(
                    Color(source.aide, list(np.ravel(np.asarray(aligned, dtype=np.float64)))).convert(target.aide).coords(), dtype=np.float64
                )
            case _:
                return np.asarray(
                    colour.convert(aligned, source.science, target.science, chromatic_adaptation_transform=adapt.value), dtype=np.float64
                )

    @staticmethod
    def _landed(xyz: Coordinates, target: ColorModel) -> Coordinates:
        # spectral intake lands in XYZ; a science-named target continues on the colour graph, an aide-only
        # (wide-gamut/HSL/HWB) target rides the ColorAide gateway so Convert is total over every spectral ingress.
        if target.science is not None:
            return np.asarray(colour.convert(xyz, "CIE XYZ", target.science), dtype=np.float64)
        rows = xyz if xyz.ndim == 2 else xyz[np.newaxis, :]
        landed = np.asarray([Color("xyz-d65", list(row)).convert(target.aide).coords() for row in rows], dtype=np.float64)
        return landed if xyz.ndim == 2 else landed[0]

    @staticmethod
    def _white(space: ColorModel, observer: Observer) -> Coordinates:
        if space.spectral:
            return np.empty(0, dtype=np.float64)
        return np.asarray(Colorimetry._resolve(_WHITEPOINT[observer][Illuminant.D65], ColorModel.XYZ, space, observer), dtype=np.float64)

    @staticmethod
    def _notate(space_aide: str | None, coords: Coordinates) -> tuple[str, ...]:
        if space_aide is None or coords.size == 0:
            return ()
        rows = coords if coords.ndim == 2 else coords[np.newaxis, :]
        return tuple(Color(space_aide, list(row)).to_string() for row in rows)

    @staticmethod
    def _path(source: ColorModel, target: ColorModel) -> tuple[str, ...]:
        if source.science is None or target.science is None:
            return tuple(name for name in (source.aide, target.aide) if name is not None)
        captured: list[str] = []
        colour.describe_conversion_path(source.science, target.science, mode="Long", print_callable=captured.append)
        return (source.science, *captured, target.science)

    @staticmethod
    def _swatches(cxf: "cxf3.CxF", target: ColorModel) -> tuple[tuple[Tristimulus, str, str], ...]:
        resources = cxf.resources
        specs = frozendict({
            row.id: row
            for row in (
                resources.color_specification_collection.color_specification if resources and resources.color_specification_collection else ()
            )
        })
        collection = resources.object_collection if resources else None
        return tuple(
            (resolved, notes[0] if (notes := Colorimetry._notate(target.aide, resolved)) else "", str(obj.name or ""))
            for obj in (collection.object_value if collection else ())
            if (resolved := Colorimetry._resolve_spot(obj, specs, target)) is not None
        )

    @staticmethod
    def _resolve_spot(obj: "cxf3.Object", specs: "frozendict[str, cxf3.ColorSpecification]", target: ColorModel) -> Tristimulus | None:
        values = list(obj.color_values.choice) if obj.color_values else []
        spectrum = next((member for member in values if isinstance(member, cxf3.ReflectanceSpectrum)), None)
        if (
            spectrum is not None
            and (resolved := Colorimetry._spectrum_xyz(spectrum, specs.get(spectrum.color_specification or ""), target)) is not None
        ):
            return resolved
        lab = next((member for member in values if isinstance(member, cxf3.ColorCielab)), None)
        if lab is not None:
            return np.asarray(
                colour.convert(np.asarray((lab.l or 0.0, lab.a or 0.0, lab.b or 0.0), dtype=np.float64), "CIE Lab", target.science), dtype=np.float64
            )
        xyz = next((member for member in values if isinstance(member, cxf3.ColorCiexyz)), None)
        if xyz is not None:
            return np.asarray(
                colour.convert(np.asarray((xyz.x or 0.0, xyz.y or 0.0, xyz.z or 0.0), dtype=np.float64), "CIE XYZ", target.science), dtype=np.float64
            )
        return None

    @staticmethod
    def _spectrum_xyz(spectrum: "cxf3.ReflectanceSpectrum", spec: "cxf3.ColorSpecification | None", target: ColorModel) -> Tristimulus | None:
        samples = spectrum.value
        if not samples:
            return None
        observer, illuminant, interval = Colorimetry._cxf_context(spec)
        raw = colour.SpectralDistribution(dict(zip((int(spectrum.start_wl or 380) + index * interval for index in range(len(samples))), samples, strict=True)))
        sd = raw.align(_WORKING_SHAPE)
        xyz = (
            np.asarray(
                colour.sd_to_XYZ(sd, cmfs=colour.MSDS_CMFS[observer.value], illuminant=colour.SDS_ILLUMINANTS[illuminant.value]), dtype=np.float64
            )
            / 100.0
        )
        return np.asarray(colour.convert(xyz, "CIE XYZ", target.science), dtype=np.float64)

    @staticmethod
    def _cxf_context(spec: "cxf3.ColorSpecification | None") -> tuple[Observer, Illuminant, int]:
        # CxF measurement admission kernel: a spectrum is interpretable only against its ColorSpecification
        # illuminant/observer/grid; a missing spec folds to D65 / 2-degree / 10nm.
        if spec is None:
            return Observer.CIE_1931_2, Illuminant.D65, 10
        tri = spec.tristimulus_spec
        observer, illuminant = Observer.CIE_1931_2, Illuminant.D65
        if tri is not None:
            if tri.observer is not None and tri.observer.value is not None:
                observer = _CXF_OBSERVER.try_find(tri.observer.value.value).default_value(Observer.CIE_1931_2)
            illum = tri.illuminant_or_custom_illuminant
            if isinstance(illum, cxf3.Illuminant) and illum.value is not None:
                illuminant = _CXF_ILLUMINANT.try_find(illum.value.value).default_value(Illuminant.D65)
        measurement = spec.measurement_spec
        increment = measurement.wavelength_range.increment if measurement is not None and measurement.wavelength_range is not None else None
        return observer, illuminant, int(increment or 10)


# --- [OPERATIONS] -------------------------------------------------------------------------
def hex_ramp(palette: Palette, /) -> tuple[str, ...]:
    rows = palette if palette.ndim == 2 else palette[np.newaxis, :]
    return tuple(Color("srgb", list(row)).clip().to_string(hex=True) for row in rows)


def cxf_book(
    swatches: tuple[tuple[str, SpectralDistribution | Tristimulus], ...],
    /,
    *,
    source: ColorModel = ColorModel.XYZ,
    illuminant: Illuminant = Illuminant.D50,
    observer: Observer = Observer.CIE_1931_2,
    creator: str = "rasm.artifacts",
    stamp: str = "1970-01-01T00:00:00Z",
) -> Result[bytes, DeriveFault]:
    # Spot intake's inverse: a named swatch set serialized as one CxF3 ink book. Each entry references the
    # specification matching its own evidence — a reflectance spectrum the measurement-bearing `cs0`, a bare
    # tristimulus the tristimulus-only `cs1` — so the book never declares a `Spectrum_Reflectance` measurement
    # for a swatch that carries none, and a partner press re-resolves each spectrum against the exact context.
    if not swatches:
        return Error("<empty-document>")
    if source.science is None:
        return Error("<source-model>")
    if any(isinstance(value, SpectralDistribution) and not bool(np.all((value.values >= 0.0) & (value.values <= 1.0))) for _, value in swatches):
        return Error("<source-model>")  # an emissive or unbounded distribution is not the reflectance the book declares
    spectral_id, tristimulus_id = "cs0", "cs1"

    def _lab(xyz: Tristimulus, spec_id: str) -> "cxf3.ColorCielab":
        # Lab is illuminant-relative: the conversion reads the BOOK's declared illuminant under its declared observer
        # (a bare convert would default to D65), so the emitted Lab values agree with the TristimulusSpec a partner
        # press re-resolves against — both entry branches converge here, one white point for the whole book.
        white = colour.CCS_ILLUMINANTS[observer.value][illuminant.value]
        lab = np.asarray(colour.XYZ_to_Lab(xyz, illuminant=white), dtype=np.float64)
        return cxf3.ColorCielab(l=float(lab[0]), a=float(lab[1]), b=float(lab[2]), color_specification=spec_id)

    def _entry(ordinal: int, name: str, value: SpectralDistribution | Tristimulus) -> "cxf3.Object":
        # ONE scale context spans both branches — `sd_to_XYZ` and every `convert` hop read the same reference
        # domain, so spectral and tristimulus swatches land Lab values on one scale with no manual rescale.
        with colour.domain_range_scale("reference"):
            if isinstance(value, SpectralDistribution):
                aligned = value.copy().align(_CXF_EXPORT_GRID)
                xyz = np.asarray(
                    colour.sd_to_XYZ(aligned, cmfs=colour.MSDS_CMFS[observer.value], illuminant=colour.SDS_ILLUMINANTS[illuminant.value]),
                    dtype=np.float64,
                )
                choice = [
                    cxf3.ReflectanceSpectrum(value=[float(sample) for sample in aligned.values], start_wl=360, color_specification=spectral_id),
                    _lab(xyz, spectral_id),
                ]
            else:
                choice = [
                    _lab(np.asarray(colour.convert(np.asarray(value, dtype=np.float64), source.science, "CIE XYZ"), dtype=np.float64), tristimulus_id)
                ]
        return cxf3.Object(
            creation_date=stamp, color_values=cxf3.ColorValues(choice=choice), object_type="Standard", name=name, id=f"c{ordinal}"
        )

    tristimulus_spec = cxf3.TristimulusSpec(
        illuminant_or_custom_illuminant=cxf3.Illuminant(value=cxf3.EilluminantType(illuminant.value)),
        observer=cxf3.Observer(value=cxf3.EobserverType(_OBSERVER_CXF[observer])),
        method=cxf3.Method(value=cxf3.EastmTableType("E308_1nm")),
    )
    spectral_present = any(isinstance(value, SpectralDistribution) for _, value in swatches)
    tristimulus_present = any(not isinstance(value, SpectralDistribution) for _, value in swatches)
    specifications = [
        spec
        for spec, present in (
            (
                cxf3.ColorSpecification(
                    tristimulus_spec=tristimulus_spec,
                    measurement_spec=cxf3.MeasurementSpec(
                        measurement_type=cxf3.MeasurementType(value=cxf3.EspectrumType("Spectrum_Reflectance")),
                        geometry_choice=cxf3.GeometryChoice(choice=cxf3.EsphereType("Specular_Excluded")),
                        wavelength_range=cxf3.WavelengthRange(start_wl=360, increment=10),
                    ),
                    id=spectral_id,
                ),
                spectral_present,
            ),
            (cxf3.ColorSpecification(tristimulus_spec=tristimulus_spec, id=tristimulus_id), tristimulus_present),
        )
        if present
    ]
    book = cxf3.CxF(
        file_information=cxf3.FileInformation(creator=creator, creation_date=stamp, description="<ink-book>"),
        resources=cxf3.Resources(
            object_collection=cxf3.ObjectCollection(object_value=[_entry(ordinal, name, value) for ordinal, (name, value) in enumerate(swatches)]),
            color_specification_collection=cxf3.ColorSpecificationCollection(color_specification=specifications),
        ),
    )
    return Ok(write_cxf(book))


# --- [EXPORTS] ----------------------------------------------------------------------------
__all__ = [
    "AdaptMethod",
    "Amount",
    "Blackbody",
    "BlendMode",
    "CamMethod",
    "CctMethod",
    "ColorFilter",
    "ColorModel",
    "ColorOp",
    "ColorSource",
    "ColorText",
    "Colorimetry",
    "CorrectMethod",
    "DeriveFault",
    "Deficiency",
    "Derivation",
    "Easing",
    "FitMethod",
    "GamutFit",
    "Harmony",
    "HueArc",
    "Illuminant",
    "Interp",
    "Metric",
    "Observer",
    "Palette",
    "PaletteCount",
    "PorterDuff",
    "Ramp",
    "RecoverMethod",
    "Severity",
    "Spacing",
    "Tristimulus",
    "cxf_book",
    "hex_ramp",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
