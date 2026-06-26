# [PY_ARTIFACTS_GRAPHIC_COLOR_DERIVE]

The upstream color-derivation owner feeding consistent, perceptually-correct color into every visual sub-domain. `Colorimetry` is ONE owner over a two-engine dispatch on the cp315 core. colour-science (NumFOCUS-affiliated, BSD-3, NumPy-backed, pure-Python, host-free) carries the colorimetric truth — the universal `colour.convert` model-pair gateway over 30+ spaces, the CIECAM02 / CAM16 appearance models, `colour.wavelength_to_XYZ` spectral-locus intake, `delta_E` difference, chromatic adaptation, the bidirectional correlated-color-temperature axis, the `whiteness`/`yellowness`/`colour_rendering_index`/`colour_fidelity_index`/`dominant_wavelength` colorimetric-index family, and the `colour_correction` measured-vs-reference CCM. ColorAide (pure-Python, zero-native, the `everything.ColorAll` all-plugins engine) carries the per-color presentation legs colour-science lacks — `Color.fit(method=...)` perceptual gamut mapping with the `in_gamut` predicate, the full `Color.filter(name=...)` CVD-plus-W3C effect surface scored by `delta_e`, the `Color.steps`/`Color.interpolate` `method`/`hue`-keyed perceptually-even palette over the `Color.harmony` wheel and `Color.average` seed-blend, the `Color.layer` blend-mode/Porter-Duff compositing with `Color.weighted_mix`, the `Color.blackbody` Planckian swatch, and the WCAG21 `Color.contrast`/`Color.luminance`/`Color.distance` safety measures. `ColorOp` is ONE closed `@tagged_union` family carrying each operation's typed payload, never an erased `params` bag, dispatched by one total `match` returning `RuntimeRail[ColorReceipt]` — the one typed receipt every arm folds into, its `measures` a `Metric`-keyed evidence map recovering every scalar correlate, never an overloaded `scalar` field or an erased `object`. It hands palettes, conversions, and measures to `visualization/chart#CHART`, `scene#SCENE`, `visualization/table#TABLE`, `graphic/marks/encode#MARK`, and the document output, so every visual artifact draws color from one owner; the `ColorReceipt.coords` arrays and `measures` correlates project to `data/tabular#WIRE` for columnar persistence, and the color-space and tone-curve provenance it resolves feeds the `graphic/color/managed#MANAGED` ICC/LUT raster-egress leg. This page carries no raster egress — every arm resolves on the cp315 core and folds into `ColorReceipt`. It closes the `COLOR_MANAGED_VISUAL_PIPELINE` and `COLORBLIND_SAFE_PALETTE` ideas on the derivation side.

## [01]-[INDEX]

- [01]-[DERIVE]: the two-engine `Colorimetry` owner over the closed-payload `ColorOp` family — `convert`/`gamut`/`filter`/`palette`/`compose`/`temperature`/`measure`/`correct` folding into one typed `ColorReceipt` whose `tag` carries the same closed `ColorOpTag` literal and whose `measures` is a `Metric`-keyed scalar-evidence map, never a bare `str` tag or an overloaded `scalar` field; `colour.convert` is the universal colour-science gateway (with `colour.wavelength_to_XYZ` admitting a spectral-locus `Wavelength` source) and `describe_conversion_path(mode="Long")` recovers the resolved model-graph onto the receipt `path`; ColorAide `everything.ColorAll` owns the disjoint gamut/filter/compose/palette/contrast legs; `ColorModel` is the dual-name `ModelNames(science, aide, spectral)` vocabulary collapsing the two engine spellings into one row whose `science` column keys colour-science and whose `aide` column keys ColorAide (either column `None` where one engine lacks the space) so one model value serves both engines — never two parallel model enums; `ColorFilter` keys the full `Color.filter` CVD-plus-W3C plugin map, `Interp`/`HueArc` the `Color.interpolate` curve and hue-arc axes, `BlendMode`/`PorterDuff` the `Color.layer` compositing axes, `FitMethod`/`Harmony` the ColorAide fit/harmony maps, `AdaptMethod` the `convert` `chromatic_adaptation_transform` registry, `CctMethod` the four `colour.temperature` illuminant methods and `Blackbody` the ColorAide Planckian methods, `CorrectMethod` the `colour_correction` CCM solvers, all carried as case payload; `Metric` is the one scalar-evidence vocabulary (`deltaE`/`distance`/`contrast`/`luminance`/`whiteness`/`yellowness`/`dominant-wavelength`/`cri`/`cfi`/`cct`/`duv` selectable through `Measure`, the `cct`/`duv` rows reading a color's correlated temperature via `Color.cct`, plus the `severity` filter-evidence key the `Filter` arm writes) whose `_METRIC` `frozendict` derives every measure callable from one row; `ColorSource = SpectralDistribution | Tristimulus | Wavelength` is the convert/measure input union and `CctSource = Kelvin | Chromaticity` the bidirectional `Temperature` payload discriminating by input shape.

## [02]-[DERIVE]

- Owner: `Colorimetry` the one color-derivation owner discriminating operation over the closed `ColorOp` family; `ColorOp` an `expression.tagged_union` whose every case carries its own typed payload, never a shared erased `params` dict; `ColorReceipt` the one typed result every arm folds into, its `tag` carrying the same closed `ColorOpTag` literal the `ColorOp` discriminant carries — never a bare `str` the consumer must re-validate — and its `coords`/`space`/`path`/`measures`/`in_gamut` slots recovering each algorithm choice, the `measures` `Metric`-keyed map replacing the prior overloaded `scalar` so a difference, a contrast, a luminance, a CCT, a Duv, and a filter severity each carry their own typed key rather than collapsing onto one ambiguous float; colour-science is the conversion, appearance-model, difference, adaptation, correlated-color-temperature, colorimetric-index, spectral-locus, CCM-correction, and `describe_conversion_path` graph-query engine on the cp315 core; ColorAide the pure-Python gamut-mapping, filter, compositing, interpolation-palette, harmony, average-blend, nearest-snap, blackbody, and contrast/luminance/distance engine on the cp315 core; `ColorModel` the dual-name `ModelNames` vocabulary whose `science`/`aide` columns key the two engines from one model value so a `Palette`/`Gamut`/`Filter`/`Compose` arm reaches ColorAide through `space.aide` while a `Convert`/`Measure` arm reaches colour-science through `source.science`, either column `None` where one engine lacks the space, never a per-engine model enum; the `everything.ColorAll` all-plugins `Color` binds once at the capsule boundary per the manifest import policy, never re-imported per `derive`. The ICC/LUT/CCTF managed raster egress is NOT this owner's concern — it is `graphic/color/managed#MANAGED`'s; `Colorimetry` resolves the color-space and tone-curve provenance that leg consumes but writes no raster.
- Cases: `ColorOp` cases — `Convert(value, source, target, adapt)` (any model-pair through the universal `colour.convert` gateway, the typed `ColorSource = SpectralDistribution | Tristimulus | Wavelength` payload discriminating a spectral distribution, a tristimulus array, and a single-`Wavelength` spectral-locus float folded through `colour.wavelength_to_XYZ` then converted to the target, absorbing the SD->XYZ->display and CIECAM02/CAM16 appearance arms as `ColorModel` source/target values reached through `source.science`/`target.science`, the receipt `path` recovered from `colour.describe_conversion_path(mode="Long")`) · `Gamut(value, source, target, method)` (`Color(source.aide, value).convert(target.aide).clone().fit(method=...)` perceptual gamut mapping into a destination space, the pre-fit `in_gamut(target.aide)` predicate carried as evidence, the `_path` graph on the receipt `path`) · `Filter(value, name, amount)` (the single `Color.filter` surface keyed by the `ColorFilter` vocabulary — the `protan`/`deutan`/`tritan` CVD legs and the `brightness`/`contrast`/`saturate`/`hue-rotate`/`grayscale`/`sepia`/`invert`/`opacity` W3C effect legs folded into one case, the source-to-filtered `delta_e` shift carried as the `Metric.SEVERITY` evidence — absorbing the prior CVD-only `Cvd` case so a deficiency simulation and a presentation effect share one arm, never a parallel filter-versus-CVD surface) · `Palette(seed, stop, count, spacing, space, interp, hue, harmony, anchors)` (one perceptual-palette axis whose `harmony is None` arm folds the perceptually-even colorblind-aware ramp via `Color.steps` `max_delta_e`-spaced and `interp`/`hue`-keyed from the `Color.average` seed-blend and whose `Harmony` arm folds the named wheel via `Color.harmony`, the `anchors` brand-color tuple snapping each ramp step to its nearest admissible color via `Color.closest` keyed on `delta_e`, the endpoint WCAG21 `contrast` carried as `Metric.CONTRAST`, the `Interp` curve and `HueArc` arc now first-class payload so a bspline-smooth or monotone hue-arc ramp is one value — never a parallel ramp-versus-harmony-versus-snap surface) · `Compose(colors, space, blend, operator, weights)` (the compositing axis whose empty-`weights` arm folds the W3C blend-mode/Porter-Duff stack via `Color.layer` keyed by `BlendMode`/`PorterDuff` and whose non-empty-`weights` arm folds the perceptual `Color.weighted_mix`, so a layered translucent overlay and a weighted N-color blend share one arm) · `Temperature(value, method, planck, space)` (one bidirectional correlated-color-temperature axis whose `Kelvin` payload mints the Planckian swatch via `Color.blackbody` keyed by the `Blackbody` method and records the `colour.temperature.CCT_to_xy` chromaticity on `path` with `Metric.CCT`, and whose `Chromaticity` payload folds the inverse `colour.temperature.xy_to_CCT` to a `Kelvin` on `Metric.CCT`, the `CctMethod` illuminant on `path`, the direction discriminated by the `CctSource` input shape — never a parallel inverse-temperature surface) · `Measure(sample, reference, metric)` (the one colorimetric-measure surface keyed by the `Metric` vocabulary over the `_METRIC` `frozendict` policy table — `colour.delta_E` difference, `Color.distance`, WCAG21 `Color.contrast`, `Color.luminance`, `colour.whiteness`/`yellowness`, `colour.dominant_wavelength`, the `colour_rendering_index`/`colour_fidelity_index` light-source-quality indices, and the `Color.cct` correlated-temperature read folded into one case, the result carried on the `metric`-keyed `measures` slot — absorbing the prior delta-E-only `Difference` case and the palette-buried contrast so every scalar color measure shares one arm) · `Correct(measured, reference, method)` (the measured-vs-reference correction-matrix calibration via `colour.colour_correction` keyed by the `CorrectMethod` solver `Cheung 2004`/`Finlayson 2015`/`Vandermonde`, the corrected array on `coords`) — matched by one total `match`/`case`.
- Entry: `Colorimetry.derive` is `async` over the runtime `async_boundary` and dispatches the `ColorOp` case, returning one `RuntimeRail[ColorReceipt]` whose typed `ColorReceipt` carries the resolved `coords` array, the output `space`, the `describe_conversion_path` model-graph `path`, the `Metric`-keyed `measures` evidence map, and the target-space `in_gamut` flag — never an erased `object`, so a re-admitting consumer recovers every algorithm choice and every scalar correlate from the receipt. `async_boundary` is the boundary-capture seam mapping any provider raise — a malformed CSS color spec, an unregistered space, a degenerate operand — into the runtime fault vocabulary, so the interior is total over the constructed op and threads no parallel `None`-as-failure path; a `NaN` coordinate is the ColorAide powerless-hue sentinel, a valid achromatic channel never gated as a fault. Every arm resolves synchronously on the cp315 core inside the async capsule: colour-science is pure-Python NumPy and ColorAide is pure-Python, so no leg crosses a process seam and no arm forces the async dispatch — the `async_boundary` is the uniform consumer contract the visual owners `await`, mirroring the settled `visualization/chart#EXPORT` `ChartExport.render -> RuntimeRail[ArtifactReceipt]` rail shape so a chart/table/scene consumer reaches color through one awaited rail rather than a per-engine sync branch. The result is a settled rail the visual owners consume, never a re-derivation per engine.
- Auto: `Convert` folds a `Wavelength` float through `colour.wavelength_to_XYZ` then `colour.convert(xyz, "CIE XYZ", target.science)`, and a spectral/tristimulus source through `colour.convert(value, source.science, target.science, chromatic_adaptation_transform=adapt.value)` under `colour.domain_range_scale("reference")`, the receipt carrying `target.science` and the `describe_conversion_path(mode="Long")` resolved model-graph as `path`; `Gamut` folds the source through `Color(source.aide, value).convert(target.aide)` then a `clone().fit(method=...)`, carrying the pre-fit `in_gamut(target.aide)` predicate as evidence so a no-op fit is recoverable; `Filter` folds `Color('srgb', value).clone().filter(name.value, amount=amount)` and carries the source-to-filtered `delta_e` shift as `Metric.SEVERITY`; `Palette` folds the multi-color `seed` tuple through `Color.average(seeds, space=space.aide, out_space='srgb')` to one perceptually-weighted base, then discriminates on the `Harmony | None` payload — the `None` arm folds the base and `stop` endpoint through `Color.steps([base, stop], steps=count, max_steps=count, max_delta_e=spacing, delta_e="2000", method=interp.value, hue=hue.value, space=space.aide, out_space='srgb')` for the `interp`-curve, `hue`-arc, perceptually-even ramp, the `Harmony` arm folds `base.harmony(name, ...)` for the named wheel — then, when `anchors` is non-empty, snaps each step through `step.closest(anchors, method="2000")` to the nearest admissible brand color, snapshotting the endpoint WCAG21 `contrast` as `Metric.CONTRAST`; `Compose` folds a non-empty-`weights` payload through `Color.weighted_mix(colors, weights=..., space=space.aide, out_space='srgb')` and an empty-`weights` payload through `Color.layer(colors, blend=blend.value, operator=operator.value, space=space.aide, out_space='srgb')`; `Temperature` discriminates on the `CctSource` shape — a `Kelvin` float mints `Color.blackbody(space.aide, value, method=planck.value).convert('srgb')` and records `colour.temperature.CCT_to_xy(value, method=method.value)` on `path` with `Metric.CCT`, a `Chromaticity` tuple folds `colour.temperature.xy_to_CCT(np.asarray(value), method=method.value)` to the `Metric.CCT` Kelvin; `Measure` folds the `sample`/`reference` pair through `_METRIC[metric]` — the `frozendict` row resolving `colour.delta_E`, `Color.distance`/`contrast`/`luminance`/`cct`, `colour.whiteness`/`yellowness`/`dominant_wavelength`, or `colour.colour_rendering_index`/`colour_fidelity_index` — onto the `metric`-keyed `measures` slot; `Correct` folds `colour.colour_correction(measured, measured, reference, method=method.value)` to the corrected array on `coords`. Each arm writes one `ColorReceipt` with its own typed `measures` keys, never an overloaded scalar.
- Packages: `colour-science` (`convert` the universal gateway subsuming `sd_to_XYZ`/`XYZ_to_sRGB`/`RGB_to_XYZ`, `wavelength_to_XYZ` the spectral-locus intake, `describe_conversion_path(mode="Long", print_callable=...)` the model-graph query feeding the receipt `path`, `RGB_COLOURSPACES` the named-space registry, `delta_E` over the `DELTA_E_METHODS` registry, `CHROMATIC_ADAPTATION_TRANSFORMS` feeding `AdaptMethod`, `colour.temperature.CCT_to_xy`/`xy_to_CCT` over the illuminant-method axis feeding `CctMethod`, `whiteness`/`yellowness`/`dominant_wavelength`/`colour_rendering_index`/`colour_fidelity_index` the colorimetric-index family feeding `Metric`, `colour_correction` over the `Cheung 2004`/`Finlayson 2015`/`Vandermonde` solvers feeding `CorrectMethod`, `SpectralDistribution`, `domain_range_scale`), `coloraide` (`everything.ColorAll` as the all-plugins `Color`; `Color.steps`/`Color.interpolate` with `max_delta_e`/`delta_e`/`method`/`hue` perceptual spacing, `Color.harmony` named wheels, `Color.average`/`Color.weighted_mix` perceptual blends, `Color.layer` blend-mode/Porter-Duff compositing, `Color.closest` nearest-by-`delta_e` snap, `Color.blackbody` Planckian swatch, `Color.contrast`/`Color.luminance`/`Color.distance`/`Color.delta_e` measures, `Color.convert`/`Color.clone`/`Color.fit`/`Color.in_gamut`/`Color.filter`/`Color.coords`), `numpy` (the array backing) on the cp315 core; runtime (`faults.RuntimeRail`/`async_boundary`). No `to_process`/`to_interpreter` seam — every arm resolves in-capsule on the core; the ICC raster apply that does cross a process seam lives on `graphic/color/managed#MANAGED`, not here.
- Growth: a new color-derivation operation is one `ColorOp` case carrying its payload plus one acceptor arm folding into `ColorReceipt`, its `tag` extending the one `ColorOpTag` literal the receipt shares; a new colorimetric measure is one `Metric` member plus one `_METRIC` `frozendict` row carrying its callable, the receipt key already typed; a new filter or CVD type is one `ColorFilter` row; a new gamut-fit method is one `FitMethod` row; a new interpolation curve is one `Interp` row and a new hue arc one `HueArc` row; a new blend mode is one `BlendMode` row and a new compositing operator one `PorterDuff` row; a new harmony wheel is one `Harmony` row; a new appearance or named model is one `ColorModel` row carrying both its `science` and `aide` (or `None` where one engine lacks it) name columns; a new CCT illuminant method is one `CctMethod` row and a new Planckian method one `Blackbody` row; a new adaptation transform is one `AdaptMethod` row; a new correction solver is one `CorrectMethod` row; a brand-palette constraint is the `anchors` tuple on the existing `Palette` payload, a weighted blend the `weights` tuple on the existing `Compose` payload — never a parallel surface; zero new surface.
- Boundary: no chart/scene rendering (that stays at the visual owners); no ICC/LUT/CCTF raster egress (that is `graphic/color/managed#MANAGED`'s exclusively — this owner resolves the color-space and tone-curve provenance the managed leg consumes but writes no raster and imports no `pillow`); the color arms emit one `ColorReceipt` carrying arrays, the `Metric`-keyed `measures` map, chromaticities, and palettes, consumed inward by the visual and document sub-domains; the gamut-mapping, filter (CVD + W3C), compositing, interpolation-palette, harmony, average/weighted-blend, nearest-snap, blackbody, and contrast/luminance/distance legs are ColorAide's exclusively — the prior hand-rolled Oklab lerp, the CVD-only `Cvd` case, the delta-E-only `Difference` case, and any bespoke Daltonization transform are the deleted forms — and colour-science keeps spectral/CAM/difference/adaptation/temperature/colorimetric-index/CCM/graph-query, never a second convert, delta-E, gamut-fit, or contrast engine; `everything.ColorAll` is the single all-plugins `Color` so OKLCH/HCT and the registered fit/filter/harmony/blend/average/closest/blackbody plugins resolve without a per-space class; the `colour.temperature` illuminant-method literal set, the `describe_conversion_path` `mode`/`print_callable` capture surface, the `CHROMATIC_ADAPTATION_TRANSFORMS` registry binding, the ColorAide harmony-name and `BlendMode`/`PorterDuff` operator strings, and the `science=None` wide-gamut/`HSL`/`HWB` space rows stay [03]-[RESEARCH] catalogue-deepen items until the folder `.api` catalogues carry their full literal sets; every other colour-science and ColorAide spelling below is settled fence code.

```python signature
from collections.abc import Callable
from enum import Enum, StrEnum
from typing import Literal, NamedTuple, assert_never

import colour
import numpy as np
from builtins import frozendict
from coloraide.everything import ColorAll as Color
from colour import SpectralDistribution
from expression import case, tag, tagged_union
from msgspec import Struct
from numpy.typing import NDArray

from rasm.runtime.faults import RuntimeRail, async_boundary

type Tristimulus = NDArray[np.float64]
type Wavelength = float
type ColorSource = SpectralDistribution | Tristimulus | Wavelength
type Kelvin = float
type Chromaticity = tuple[float, float]
type CctSource = Kelvin | Chromaticity
type ColorOpTag = Literal["convert", "gamut", "filter", "palette", "compose", "temperature", "measure", "correct"]


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
    ICTCP = ModelNames("ICtCp", "ictcp")
    DISPLAY_P3 = ModelNames(None, "display-p3")
    REC2020 = ModelNames(None, "rec2020")
    HSV = ModelNames("HSV", "hsv")
    HSL = ModelNames(None, "hsl")
    HWB = ModelNames(None, "hwb")
    CIECAM02 = ModelNames("CIECAM02", None)
    CAM16 = ModelNames("CAM16", None)


class AdaptMethod(StrEnum):
    VON_KRIES = "Von Kries"
    BRADFORD = "Bradford"
    CAT02 = "CAT02"
    CAT16 = "CAT16"
    CMCCAT2000 = "CMCCAT2000"
    SHARP = "Sharp"


class FitMethod(StrEnum):
    RAYTRACE = "raytrace"
    OKLCH_CHROMA = "oklch-chroma"
    LCH_CHROMA = "lch-chroma"
    MINDE_CHROMA = "minde-chroma"
    SCALE = "scale"
    SCALE_LUMINANCE = "scale-luminance"


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


class HueArc(StrEnum):
    SHORTER = "shorter"
    LONGER = "longer"
    INCREASING = "increasing"
    DECREASING = "decreasing"


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
    SOURCE_OVER = "source-over"
    DESTINATION_OVER = "destination-over"
    SOURCE_IN = "source-in"
    DESTINATION_IN = "destination-in"
    SOURCE_OUT = "source-out"
    DESTINATION_OUT = "destination-out"
    SOURCE_ATOP = "source-atop"
    DESTINATION_ATOP = "destination-atop"
    XOR = "xor"


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


class Metric(StrEnum):
    DELTA_E_2000 = "deltaE-2000"
    DELTA_E_1994 = "deltaE-1994"
    DELTA_E_1976 = "deltaE-1976"
    DELTA_E_CMC = "deltaE-CMC"
    DELTA_E_DIN99 = "deltaE-DIN99"
    DISTANCE = "distance"
    CONTRAST = "contrast"
    LUMINANCE = "luminance"
    WHITENESS = "whiteness"
    YELLOWNESS = "yellowness"
    DOMINANT_WAVELENGTH = "dominant-wavelength"
    CRI = "cri"
    CFI = "cfi"
    CCT = "cct"
    DUV = "duv"
    SEVERITY = "severity"


_METRIC: frozendict[Metric, Callable[[ColorSource, Tristimulus | None], float]] = frozendict({
    Metric.DELTA_E_2000: lambda a, b: float(colour.delta_E(b, a, method="CIE 2000")),
    Metric.DELTA_E_1994: lambda a, b: float(colour.delta_E(b, a, method="CIE 1994")),
    Metric.DELTA_E_1976: lambda a, b: float(colour.delta_E(b, a, method="CIE 1976")),
    Metric.DELTA_E_CMC: lambda a, b: float(colour.delta_E(b, a, method="CMC")),
    Metric.DELTA_E_DIN99: lambda a, b: float(colour.delta_E(b, a, method="DIN99")),
    Metric.DISTANCE: lambda a, b: Color("srgb", list(a)).distance(Color("srgb", list(b)), space="lab"),
    Metric.CONTRAST: lambda a, b: Color("srgb", list(a)).contrast(Color("srgb", list(b))),
    Metric.LUMINANCE: lambda a, _b: Color("srgb", list(a)).luminance(),
    Metric.WHITENESS: lambda a, b: float(colour.whiteness(a, b)),
    Metric.YELLOWNESS: lambda a, _b: float(colour.yellowness(a)),
    Metric.DOMINANT_WAVELENGTH: lambda a, b: float(np.asarray(colour.dominant_wavelength(a, b))[0]),
    Metric.CRI: lambda a, _b: float(colour.colour_rendering_index(a)),
    Metric.CFI: lambda a, _b: float(colour.colour_fidelity_index(a)),
    Metric.CCT: lambda a, _b: float(Color("srgb", list(a)).cct()[0]),
    Metric.DUV: lambda a, _b: float(Color("srgb", list(a)).cct()[1]),
})


@tagged_union(frozen=True)
class ColorOp:
    tag: ColorOpTag = tag()
    convert: tuple[ColorSource, ColorModel, ColorModel, AdaptMethod] = case()
    gamut: tuple[Tristimulus, ColorModel, ColorModel, FitMethod] = case()
    filter: tuple[Tristimulus, ColorFilter, float] = case()
    palette: tuple[tuple[str, ...], str, int, float, ColorModel, Interp, HueArc, Harmony | None, tuple[str, ...]] = case()
    compose: tuple[tuple[str, ...], ColorModel, BlendMode, PorterDuff, tuple[float, ...]] = case()
    temperature: tuple[CctSource, CctMethod, Blackbody, ColorModel] = case()
    measure: tuple[ColorSource, Tristimulus | None, Metric] = case()
    correct: tuple[Tristimulus, Tristimulus, CorrectMethod] = case()

    @staticmethod
    def Convert(value: ColorSource, source: ColorModel, target: ColorModel, adapt: AdaptMethod = AdaptMethod.BRADFORD) -> "ColorOp":
        return ColorOp(convert=(value, source, target, adapt))

    @staticmethod
    def Gamut(value: Tristimulus, source: ColorModel, target: ColorModel, method: FitMethod = FitMethod.OKLCH_CHROMA) -> "ColorOp":
        return ColorOp(gamut=(value, source, target, method))

    @staticmethod
    def Filter(value: Tristimulus, name: ColorFilter, amount: float = 1.0) -> "ColorOp":
        return ColorOp(filter=(value, name, amount))

    @staticmethod
    def Palette(seed: tuple[str, ...], stop: str, count: int, spacing: float = 0.0, space: ColorModel = ColorModel.OKLCH, interp: Interp = Interp.BSPLINE, hue: HueArc = HueArc.SHORTER, harmony: Harmony | None = None, anchors: tuple[str, ...] = ()) -> "ColorOp":
        return ColorOp(palette=(seed, stop, count, spacing, space, interp, hue, harmony, anchors))

    @staticmethod
    def Compose(colors: tuple[str, ...], space: ColorModel = ColorModel.OKLCH, blend: BlendMode = BlendMode.NORMAL, operator: PorterDuff = PorterDuff.SOURCE_OVER, weights: tuple[float, ...] = ()) -> "ColorOp":
        return ColorOp(compose=(colors, space, blend, operator, weights))

    @staticmethod
    def Temperature(value: CctSource, method: CctMethod = CctMethod.DAYLIGHT, planck: Blackbody = Blackbody.OHNO_2013, space: ColorModel = ColorModel.SRGB) -> "ColorOp":
        return ColorOp(temperature=(value, method, planck, space))

    @staticmethod
    def Measure(sample: ColorSource, metric: Metric, reference: Tristimulus | None = None) -> "ColorOp":
        return ColorOp(measure=(sample, reference, metric))

    @staticmethod
    def Correct(measured: Tristimulus, reference: Tristimulus, method: CorrectMethod = CorrectMethod.CHEUNG_2004) -> "ColorOp":
        return ColorOp(correct=(measured, reference, method))


class ColorReceipt(Struct, frozen=True):
    tag: ColorOpTag
    coords: Tristimulus
    space: str = ""
    path: tuple[str, ...] = ()
    measures: dict[Metric, float] = {}
    in_gamut: bool = True


class Colorimetry(Struct, frozen=True):
    op: ColorOp

    async def derive(self) -> RuntimeRail[ColorReceipt]:
        return await async_boundary(f"color.{self.op.tag}", self._compute)

    async def _compute(self) -> ColorReceipt:
        match self.op:
            case ColorOp(tag="convert", convert=(value, source, target, adapt)):
                with colour.domain_range_scale("reference"):
                    coords = (
                        colour.convert(colour.wavelength_to_XYZ(value), "CIE XYZ", target.science)
                        if isinstance(value, float)
                        else colour.convert(value, source.science, target.science, chromatic_adaptation_transform=adapt.value)
                    )
                return ColorReceipt("convert", np.asarray(coords, dtype=np.float64), space=target.science or "", path=self._path(source, target))
            case ColorOp(tag="gamut", gamut=(value, source, target, method)):
                seeded = Color(source.aide, list(value)).convert(target.aide)
                fitted = seeded.clone().fit(method=method.value)
                return ColorReceipt("gamut", np.asarray(fitted.coords(), dtype=np.float64), space=target.aide or "", path=self._path(source, target), in_gamut=seeded.in_gamut(target.aide))
            case ColorOp(tag="filter", filter=(value, name, amount)):
                origin = Color("srgb", list(value))
                filtered = origin.clone().filter(name.value, amount=amount)
                return ColorReceipt("filter", np.asarray(filtered.coords(), dtype=np.float64), space="srgb", measures={Metric.SEVERITY: origin.delta_e(filtered, method="2000")})
            case ColorOp(tag="palette", palette=(seed, stop, count, spacing, space, interp, hue, harmony, anchors)):
                base = Color.average(list(seed), space=space.aide, out_space="srgb")
                ramp = (
                    base.harmony(harmony.value, space=space.aide, out_space="srgb")
                    if harmony is not None
                    else Color.steps([base, stop], steps=count, max_steps=count, max_delta_e=spacing, delta_e="2000", method=interp.value, hue=hue.value, space=space.aide, out_space="srgb")
                )
                snapped = [step.closest(list(anchors), method="2000") for step in ramp] if anchors else ramp
                coords = np.array([step.coords() for step in snapped], dtype=np.float64)
                contrast = float(Color("srgb", list(coords[0])).contrast(Color("srgb", list(coords[-1]))))
                return ColorReceipt("palette", coords, space="srgb", path=(space.aide or "", interp.value, hue.value), measures={Metric.CONTRAST: contrast})
            case ColorOp(tag="compose", compose=(colors, space, blend, operator, weights)):
                blended = (
                    Color.weighted_mix(list(colors), weights=list(weights), space=space.aide, out_space="srgb")
                    if weights
                    else Color.layer(list(colors), blend=blend.value, operator=operator.value, space=space.aide, out_space="srgb")
                )
                return ColorReceipt("compose", np.asarray(blended.coords(), dtype=np.float64), space="srgb", path=(blend.value, operator.value))
            case ColorOp(tag="temperature", temperature=(value, method, planck, space)):
                if isinstance(value, float):
                    swatch = Color.blackbody(space.aide, value, method=planck.value).convert("srgb")
                    chroma = np.asarray(colour.temperature.CCT_to_xy(value, method=method.value), dtype=np.float64)
                    return ColorReceipt("temperature", np.asarray(swatch.coords(), dtype=np.float64), space="srgb", path=(method.value, planck.value, *(f"{c:.5f}" for c in chroma)), measures={Metric.CCT: value})
                kelvin = float(colour.temperature.xy_to_CCT(np.asarray(value, dtype=np.float64), method=method.value))
                return ColorReceipt("temperature", np.asarray(value, dtype=np.float64), space="xyy", path=(method.value,), measures={Metric.CCT: kelvin})
            case ColorOp(tag="measure", measure=(sample, reference, metric)):
                coords = np.asarray(sample, dtype=np.float64) if isinstance(sample, np.ndarray) else np.empty(0, dtype=np.float64)
                return ColorReceipt("measure", coords, measures={metric: float(_METRIC[metric](sample, reference))})
            case ColorOp(tag="correct", correct=(measured, reference, method)):
                corrected = colour.colour_correction(measured, measured, reference, method=method.value)
                return ColorReceipt("correct", np.asarray(corrected, dtype=np.float64), space="sRGB", path=(method.value,))
            case _:
                assert_never(self.op)

    @staticmethod
    def _path(source: ColorModel, target: ColorModel) -> tuple[str, ...]:
        if source.science is None or target.science is None:
            return tuple(name for name in (source.aide, target.aide) if name is not None)
        captured: list[str] = []
        colour.describe_conversion_path(source.science, target.science, mode="Long", print_callable=captured.append)
        return (source.science, *captured, target.science)
```

## [03]-[RESEARCH]

- [COLOUR_SCIENCE]: the universal `convert` gateway, `describe_conversion_path` model-graph query, `RGB_COLOURSPACES` named-space registry, `delta_E` over `DELTA_E_METHODS`, `wavelength_to_XYZ` spectral-locus intake, `whiteness`/`yellowness`/`dominant_wavelength` colorimetric indices, `colour_rendering_index`/`colour_fidelity_index` light-source-quality indices, `colour_correction` over the `Cheung 2004`/`Finlayson 2015`/`Vandermonde` solvers, `SpectralDistribution`, and `domain_range_scale` all verify against the folder `.api` catalogue for `colour-science` (`0.4.7` reflected, cp315-core pure-Python), so the `Convert`/`Measure`/`Correct` colour-science folds are settled fence code. `AdaptMethod` keys `CHROMATIC_ADAPTATION_TRANSFORMS` (the matrix registry `convert`'s `chromatic_adaptation_transform` kwarg resolves — `Bradford`, `CAT02`, `CAT16`, `Von Kries`, `CMCCAT2000`, `Sharp`, `XYZ Scaling`), NOT the catalogue-listed `CHROMATIC_ADAPTATION_METHODS` which keys the standalone `chromatic_adaptation()` function (`Von Kries`, `Zhai 2018`, `Li 2025`, ...) — `Bradford`/`CAT02`/`CAT16`/`Sharp` exist only in the transforms registry, so binding `AdaptMethod` to the methods registry breaks the default `Bradford` convert path; the `CHROMATIC_ADAPTATION_TRANSFORMS` row is a catalogue-deepen item since the `.api` lists only `CHROMATIC_ADAPTATION_METHODS`. The universal `convert` subsumes the per-pair `sd_to_XYZ`/`XYZ_to_sRGB`/`RGB_to_XYZ` transforms, so naming them as separate engine members is the deleted sprawl. The open colour-science seams are the `whiteness`/`yellowness` `method` literal sets and the `CHROMATIC_ADAPTATION_TRANSFORMS`/`describe_conversion_path` items below; the `cctf_encoding`/`read_LUT`/`write_LUT`/`LUTSequence`/`write_image` tone-and-egress surface is NOT this page's — it is `graphic/color/managed#MANAGED`'s.
- [COLORAIDE]: the `everything.ColorAll` `Color` plus `convert`/`clone`/`fit(method=)`/`in_gamut`/`clip`, `filter(name, amount=)`, `delta_e(color, method=)`/`distance(color, space=)`/`contrast(color)`/`luminance()`, `steps`/`interpolate(method=, hue=)`/`mix`/`weighted_mix`/`layer(blend=, operator=)`/`average`/`harmony`/`closest`, `blackbody(space, temp, method=)`/`cct(method=)`, and `coords()` spellings verify against the folder `.api` catalogue for `coloraide` (`8.8.1` reflected, cp315 pure-Python), so the `Gamut`/`Filter`/`Palette`/`Compose`/`Temperature`/`Measure` ColorAide folds are settled fence code. ColorAide spaces are lowercase-hyphenated registered names, disjoint from the colour-science model strings, so every ColorAide call reaches the space through `ColorModel.aide` and every colour-science call through `ColorModel.science` — the single `ModelNames(science, aide)` row carrying both spellings. The collapse is load-bearing: `Filter` folds the three `protan`/`deutan`/`tritan` CVD rows AND the eight `brightness`/`contrast`/`saturate`/`hue-rotate`/`grayscale`/`sepia`/`invert`/`opacity` W3C rows into one `ColorFilter` over `Color.filter` (the prior CVD-only `Cvd` case was a three-of-eleven slice), and `Measure` folds `delta_E`/`distance`/`contrast`/`luminance` (plus the colour-science indices) into one `_METRIC` table (the prior `Difference` case was delta-E-only). The six `FitMethod` rows, the eleven `ColorFilter` rows, the eight `delta_e` metrics, and the six `Interp` curves (`linear`/`continuous`/`bspline`/`natural`/`monotone`/`css-linear`) are catalogue-enumerated registered names; the `HueArc` arc strings (`shorter` default confirmed), the harmony names, and the `BlendMode`/`PorterDuff` operator strings are the open name-enumeration seams below; `everything.ColorAll` imports as `Color` at boundary scope per the manifest import policy.
- [TEMPERATURE] [RESEARCH]: the colour-science `colour.temperature.CCT_to_xy(CCT, method=...)`/`xy_to_CCT(xy, method=...)` call surface is settled fence code, and the ColorAide `Color.blackbody(space, temp, method=...)` swatch and `Color.cct(method=...)` read are settled against the catalogue CCT leg (the `ohno-2013`/`robertson-1968` Planckian methods named, so `Blackbody` is settled). The one open colour-science leg is the illuminant `method` literal set — the catalogue row names a `method` axis without enumerating the four strings, so the `CctMethod` vocabulary (`{"CIE Illuminant D Series", "Kang 2002", "Hernandez 1999", "McCamy 1992"}`) stays a catalogue-deepen item until a `colour.temperature` method-literal reflection pass lands. Close-condition: `.api` catalogue enumerates the `CCT_to_xy`/`xy_to_CCT` four-method literal set.
- [CONVERSION_PATH] [RESEARCH]: the `Colorimetry._path` graph capture spells `describe_conversion_path(source, target, mode="Long", print_callable=captured.append)`, but the folder `.api` catalogue verifies only the two-argument `describe_conversion_path(source, target)` call without the `mode`/`print_callable` capture kwargs. The full signature is `describe_conversion_path(source, target, mode='Short', width=79, padding=3, print_callable=print, **kwargs) -> None` — `mode='Long'` is load-bearing: the default `'Short'` prints a truncated summary while `'Long'` yields the resolved model-graph the receipt `path` claims. The `mode`/`print_callable`-fed capture and the `(source, *captured, target)` path tuple stay a catalogue-deepen seam until a `describe_conversion_path` signature reflection lands; the `Convert`/`Gamut` receipt-`path` shape is settled. Close-condition: `.api` catalogue carries `describe_conversion_path` with `mode`/`print_callable`.
- [COMPOSE] [RESEARCH]: the `Color.layer(colors, blend=, operator=, space=, out_space=)` classmethod and `Color.weighted_mix(colors, weights=, space=, out_space=)` calls verify against the catalogue composition leg, so the `Compose` arm shape is settled; the W3C `BlendMode` (the sixteen `normal`/`multiply`/`screen`/`overlay`/.../`luminosity` separable-and-non-separable modes) and `PorterDuff` (the nine `source-over`/.../`xor` operators) name sets are not enumerated in the catalogue, so those `StrEnum` rows stay a catalogue-deepen seam. Close-condition: `.api` catalogue enumerates the `Color.layer` blend-mode and Porter-Duff operator name sets.
- [SPACES] [RESEARCH]: the `ColorModel` rows carry both engine spellings — `ICTCP` keys `science="ICtCp"` (confirmed by the catalogue `XYZ_to_ICtCp` named transform) and `aide="ictcp"`, while `DISPLAY_P3`/`REC2020`/`HSL`/`HWB` key `science=None` as ColorAide-only gamut/palette/compose/filter targets because the catalogue confirms their presence in the ColorAide base set but does not enumerate the exact registered space ids nor a colour-science convert-node name; those `aide` strings and a future `science` column stay a catalogue-deepen seam. Close-condition: `.api` catalogue enumerates the ColorAide registered space ids and the colour-science wide-gamut convert nodes.
- [HARMONY] [RESEARCH]: the `Harmony` `StrEnum` carries the ColorAide harmony-wheel names (`mono`/`complement`/`split`/`analogous`/`triad`/`square`/`rectangle`/`wheel`) as the `Color.harmony(name, ...)` payload, but the catalogue documents the call without enumerating the registered names. The row is `rectangle`, NOT `rectangular` (which raises `ValueError: color harmony 'rectangular' cannot be found`), and the singular `complement`/`triad`/`mono` forms not the `-ary`/`-ic` long forms. The `Harmony` row strings stay a catalogue-deepen seam until a `coloraide` harmony-name reflection pass lands; the `Palette` `Harmony` arm shape is settled. Close-condition: `.api` catalogue carries the registered harmony `name` strings.
