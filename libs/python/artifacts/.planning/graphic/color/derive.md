# [PY_ARTIFACTS_GRAPHIC_COLOR_DERIVE]

The upstream color-derivation SUBSTRATE feeding consistent, perceptually-correct color into every visual sub-domain — derive returns color VALUES, never a receipt. `Colorimetry` is ONE owner over a two-engine dispatch. colour-science (NumFOCUS-affiliated, BSD-3, NumPy-backed, pure-Python, host-free) carries the colorimetric truth — the universal `colour.convert` model-pair gateway over 30+ spaces, the CIECAM02 / CAM16 appearance models, `colour.wavelength_to_XYZ` spectral-locus intake, the `colour.delta_E` CIE/CMC/DIN99 difference family, the `colour.chromatic_adaptation` cross-illuminant white-point transform over an `Observer`-keyed `colour.CCS_ILLUMINANTS` whitepoint table, the `colour.msds_to_XYZ` multi-spectral batch intake, the measurement-side spectral RESAMPLE (`SpectralDistribution.align` onto the one working `SpectralShape`), the bidirectional correlated-color-temperature axis, the `whiteness`/`yellowness`/`colour_rendering_index`/`colour_fidelity_index`/`dominant_wavelength`/`complementary_wavelength` colorimetric-index family, and the `colour_correction` measured-vs-reference CCM. ColorAide (pure-Python, zero-native, the `everything.ColorAll` all-plugins engine) carries the per-color presentation legs colour-science lacks — `Color.fit(method=...)` perceptual gamut mapping with the `in_gamut` predicate, the full `Color.filter(name=...)` CVD-plus-W3C effect surface scored by `delta_e`, the `Color.steps`/`Color.discrete`/`Color.interpolate` `method`/`hue`-keyed smooth-or-categorical palette over the `Color.harmony` wheel and `Color.average` seed-blend, the `Color.layer` blend-mode/Porter-Duff compositing with `Color.mask` channel prep and `Color.weighted_mix` (the two-color `mix` is its N=2 weighted arm), the `Color.blackbody` Planckian swatch, the OKLab-perceptual `Color.delta_e(method=...)` `ok`/`jz`/`hyab`/`itp`/`99o`/`cam02`/`cam16`/`hct` difference family colour-science has no Lab-array form for, the WCAG21 `Color.contrast`/`Color.luminance`/`Color.distance`/`Color.cct` safety measures, and the `Color.to_string` CSS-notation egress. colour-cxf (BSD, `xsdata`-bound CxF3) is the inbound spot-library exchange skin the `Spot` arm owns: `read_cxf` decodes a print partner's `.cxf` into the typed `cxf3` graph whose measured `ReflectanceSpectrum`/`ColorCielab`/`ColorCiexyz` per `Object`, resolved through the `ColorSpecification` IDREF (illuminant/observer/wavelength grid) into `colour.sd_to_XYZ` under the CxF measurement context, seeds the palette from an exchanged measured spot set — the one CxF colour-half intake derive owns, the device-half `ColorCmykplusN` separations remaining `graphic/color/managed#MANAGED`'s. `ColorOp` is ONE closed `@tagged_union` family carrying each operation's typed payload, never an erased `params` bag, dispatched by one total `match` returning the frozen `Derivation` value-bundle every arm folds into — `coords` arrays, the `Color.to_string` CSS `notation`, the resolved model-graph `path`, an immutable `frozendict[Metric, float]` `measures` evidence map, and the space/pointer gamut predicates. The color arity is RULED at two pages: measurement (the 28-member `Metric` family, the spectral resample) is a derive owner block, while CxF device-half intake, plate authoring, the TAC gate, LUT authoring, and every raster egress consolidate in `graphic/color/managed#MANAGED` — 3/4-page splits are thin single-consumer shells. This page OWNS `AdaptMethod` (managed composes it downward), the re-homed `Palette` carrier and `hex_ramp` CSS-hex projection every visual band shares, and NO receipt: the deleted `ColorReceipt`/`ColorReceiptWire` parallel rail is the anti-pattern — the LUT/plate/swatch terminals mint `ArtifactReceipt.Color` on managed, and derive's consumers compose values. Each `Metric` row in the `_METRIC` `Map[Metric, MetricSpec]` table binds the measure to the input `ColorModel` its engine demands, and `Measure` resolves every sample into that space through the one `_resolve` kernel `Convert` shares. Scalar-bounded payloads admit through refined `Annotated`+`beartype.vale.Is` contracts at the `@beartype`-guarded factories, so an out-of-range amount, count, spacing, kelvin, or wavelength is refused at construction. Every arm resolves through the runtime `LanePolicy.offload` under `Modality.INTERPRETER` — interpreter-local `colour` module state makes the non-thread-local `domain_range_scale` global race-free without any folder-minted limiter — and hands palettes, conversions, adaptations, and measures to `visualization/chart#CHART`, `scene#SCENE`, `visualization/table#TABLE`, `graphic/marks/encode#MARK`, the drawing plane, and the document output, so every visual artifact draws color from one owner; the color-space and `describe_conversion_path` provenance on `path` feeds the `graphic/color/managed#MANAGED` ICC/LUT raster-egress leg.

## [01]-[INDEX]

- [01]-[DERIVE]: the two-engine `Colorimetry` owner over the closed-payload `ColorOp` family — `convert`/`adapt`/`gamut`/`filter`/`palette`/`compose`/`temperature`/`measure`/`correct`/`spot` folding into one frozen `Derivation` value whose `tag` carries the same closed `ColorOpTag` literal and whose `measures` is a `Metric`-keyed scalar-evidence map, never a bare `str` tag, an overloaded `scalar` field, or a receipt; `colour.convert` is the universal colour-science gateway (with `colour.wavelength_to_XYZ` admitting a spectral-locus `Wavelength` source) and `describe_conversion_path(mode="Long")` recovers the resolved model-graph onto `path`; `colour.chromatic_adaptation` is the standalone cross-illuminant XYZ adaptation between `Illuminant` white points read from the `Observer`-keyed `_WHITEPOINT` `Map[Observer, Map[Illuminant, Tristimulus]]` nested table derived once from `colour.CCS_ILLUMINANTS[observer]`, keyed by the `CamMethod` `CHROMATIC_ADAPTATION_METHODS` registry (`Von Kries`/`CMCCAT2000`/`Zhai 2018`/`Li 2025`), distinct from the in-conversion `AdaptMethod` transforms `convert` resolves — `AdaptMethod` is declared HERE and `graphic/color/managed#MANAGED` composes it; ColorAide `everything.ColorAll` owns the disjoint gamut/filter/compose/palette/contrast legs; `ColorModel` is the dual-name `ModelNames(science, aide, spectral)` vocabulary collapsing the two engine spellings into one row (either column `None` where one engine lacks the space) so one model value serves both engines — never two parallel model enums; `ColorFilter` keys the full `Color.filter` CVD-plus-W3C plugin map, `Ramp` is the closed palette-modality `@tagged_union` (`smooth` `Color.steps`, `discrete` `Color.discrete`, `harmony` `Color.harmony`) carrying its own `Interp`/`HueArc`/`Easing`/`Harmony` payload so a perceptually-even ramp, a categorical scale, and a named wheel are three cases of one axis; `BlendMode`/`PorterDuff` key the full 16-blend/15-operator `Color.layer` compositing axes with the `mask` channel tuple riding the same `Compose` payload, `FitMethod`/`Harmony` the ColorAide fit/harmony maps, `AdaptMethod` the `convert` `chromatic_adaptation_transform` registry, `CctMethod` the four `colour.temperature` illuminant methods and `Blackbody` the ColorAide Planckian methods, `CorrectMethod` the `colour_correction` CCM solvers, all carried as case payload; `Metric` is the one 28-member scalar-evidence vocabulary whose `_METRIC` `Map[Metric, MetricSpec]` table binds each row to the input `ColorModel` its engine demands — colour-science CIE/CMC/DIN99 `delta_E` over `LAB`, `whiteness`/`yellowness` over `XYZ`, `dominant`/`complementary` wavelength over `XYY`, `cri`/`cfi` over `SPECTRAL` (resampled to `_WORKING_SHAPE` at ingress), the ColorAide OKLab-perceptual rows plus `distance`/`contrast`/`luminance`/`cct`/`duv`/`severity` over `SRGB`, and the `chromaticity-x`/`chromaticity-y` `XYY` reads — so `Measure` resolves each sample into the row's space through the shared `_resolve` kernel under its payload `Observer` before applying; `ColorSource = SpectralDistribution | MultiSpectralDistributions | Tristimulus | Wavelength` is the convert/measure input union whose `MultiSpectralDistributions` member folds the batch `colour.msds_to_XYZ` modality, and `CctSource = Kelvin | Chromaticity` the bidirectional `Temperature` payload discriminating by input shape; `Palette` is the re-homed N-swatch carrier and `hex_ramp` the one srgb-array→CSS-hex projection (MOVED IN from `visualization/chart/spec` — chart, table, diagram, and the drawing pens rewire to this owner); scalar-bounded payloads (`Amount`/`PaletteCount`/`Spacing`/`Kelvin`/`Wavelength`) are refined `Annotated`+`Is` aliases admitted through the `@beartype`-guarded factories.

## [02]-[DERIVE]

- Cases: `ColorOp` cases — `Convert(value, source, target, adapt, observer)` (any model-pair through the universal `colour.convert` gateway, the typed `ColorSource` payload discriminating a single spectral distribution, a multi-channel batch folded through `colour.msds_to_XYZ`, a tristimulus array, and a single-`Wavelength` spectral-locus float folded through `colour.wavelength_to_XYZ(nm, cmfs)` under the payload `Observer` cmfs then converted to the target, absorbing the SD→XYZ→display and CIECAM02/CAM16 appearance arms as `ColorModel` values, the `path` recovered from `colour.describe_conversion_path(mode="Long")`) · `Adapt(value, source, target, method, observer)` (one cross-illuminant chromatic-adaptation axis folding `colour.chromatic_adaptation(xyz, source_white, dest_white, method=...)` where the `Illuminant` source/target resolve to XYZ white points through the `Observer`-keyed `_WHITEPOINT[observer]` table, keyed by the `CamMethod` registry — the standalone explicit-white-point form `convert`'s in-conversion `chromatic_adaptation_transform` kwarg cannot express, never a parallel adaptation surface) · `Gamut(value, source, target, method)` (`Color(source.aide, value).convert(target.aide).clone().fit(method=...)` perceptual gamut mapping into a destination space, the pre-fit `in_gamut(target.aide)` and `in_pointer_gamut()` predicates carried as evidence, `FitMethod.POINTER` routing to the `fit_pointer_gamut()` real-surface printable map) · `Filter(value, name, amount)` (the single `Color.filter` surface keyed by the `ColorFilter` vocabulary — the `protan`/`deutan`/`tritan` CVD legs and the eight W3C effect legs folded into one case, the `amount` a refined `Amount` in `[0, 1]`, the source-to-filtered `delta_e` shift carried as `Metric.SEVERITY` — so a deficiency simulation and a presentation effect share one arm) · `Palette(seed, stop, count, spacing, space, ramp, anchors)` (one perceptual-palette axis whose `Ramp` discriminant folds the `smooth` perceptually-even `Color.steps` ramp `max_delta_e`-spaced and `interp`/`hue`/`easing`-keyed, the `discrete` categorical `Color.discrete` Interpolator sampled at `count` points for a non-blended data scale, or the `harmony` named-wheel `Color.harmony` — from the `Color.average` seed-blend, the `anchors` brand-color tuple snapping each ramp step to its nearest admissible color via `Color.closest` keyed on `delta_e`, the endpoint WCAG21 `contrast` carried as `Metric.CONTRAST`, never a parallel ramp-versus-harmony-versus-discrete surface nor a `harmony is None` discriminant) · `Compose(colors, space, blend, operator, weights, mask)` (the compositing axis whose empty-`weights` arm folds the W3C blend-mode/Porter-Duff stack via `Color.layer` keyed by `BlendMode`/`PorterDuff` over the `mask`-prepped stack — a non-empty `mask` tuple zeroes the named channels per color through `Color.mask` before layering — and whose non-empty-`weights` arm folds the perceptual `Color.weighted_mix`, the two-color `mix` exactly its N=2 form, so a layered translucent overlay, a masked-channel composite, and a weighted N-color blend share one arm) · `Temperature(value, method, planck, space)` (one bidirectional correlated-color-temperature axis whose refined-`Kelvin` payload mints the Planckian swatch via `Color.blackbody` keyed by the `Blackbody` method and records `colour.temperature.CCT_to_xy` chromaticity with `Metric.CCT`, and whose `Chromaticity` payload folds the inverse `colour.temperature.xy_to_CCT` to a `Kelvin`, the direction discriminated by the `CctSource` input shape — never a parallel inverse-temperature surface) · `Measure(sample, source, reference, metric, observer)` (the one colorimetric-measure surface keyed by the `Metric` vocabulary over the `_METRIC` `Map[Metric, MetricSpec]` policy table — each row binding its measure to the input `ColorModel` it demands so the arm resolves the `source`-spaced sample and the `Option`-carried reference into that space through the shared `_resolve` kernel under the payload `Observer` cmfs and `D65` white before applying, the resolved coordinates on `coords` and the result on the `metric`-keyed `measures` slot — so every scalar color measure shares one arm and reads its correct input space) · `Correct(measured, reference, method)` (the measured-vs-reference correction-matrix calibration via `colour.colour_correction` keyed by the `CorrectMethod` solver, the corrected array on `coords`) · `Spot(document, target)` (the colour-cxf inbound spot-library seed — `read_cxf` decodes the `.cxf` bytes into the `cxf3.CxF` graph, each `Object`'s primary `ReflectanceSpectrum` resolved through its `ColorSpecification` IDREF measurement context into `colour.sd_to_XYZ`, falling back to a direct `ColorCielab`/`ColorCiexyz` when no spectrum is measured, converted to the `target` model and folded into a palette on `coords`+`notation` with the spot names on `path` — never re-parsing CxF XML by hand nor re-integrating an SPD colour-science owns) — matched by one total `match`/`case`.
- Auto: every arm resolves inside `LanePolicy.offload(Colorimetry._resolved, op, modality=Modality.INTERPRETER)` — the runtime-owned bound; `Modality.INTERPRETER` gives each offload interpreter-local `colour` module state, so the non-thread-local `domain_range_scale` process-global never races concurrent derives and the prior folder-minted `CapacityLimiter(1)`/`to_thread` serialization lane is DEAD; a provider raise crosses the offload's own `async_boundary` onto the rail. `Convert` folds the source through the shared `_resolve` kernel under `colour.domain_range_scale("reference")` — a `Wavelength` float through `colour.wavelength_to_XYZ(nm, cmfs)` then `colour.convert(xyz, "CIE XYZ", target.science)`, a `MultiSpectralDistributions` batch through `colour.msds_to_XYZ` then convert, and a spectral/tristimulus source through `colour.convert(value, source.science, target.science, chromatic_adaptation_transform=adapt.value)` — or, when either endpoint is a `science=None` wide-gamut/HSL/HWB space, through the ColorAide `Color(source.aide, coords).convert(target.aide)` gateway so the fold is TOTAL over every `ColorModel` pair rather than passing `None` to `colour.convert`; every spectral ingress first passes `_aligned` — the measurement-side resample onto the one `_WORKING_SHAPE` working grid (copy-then-align; colour mutates in place), distinct from the LUT-authoring interpolators `graphic/color/managed#MANAGED` owns. `Palette` folds the multi-color `seed` tuple through `Color.average` to one perceptually-weighted base, discriminates on the `Ramp` case, snaps through `Color.closest` when `anchors` is non-empty, and snapshots the endpoint WCAG21 `contrast`; `Compose` masks then layers, or weighted-mixes; `Measure` resolves the sample and the `Option`-carried reference (defaulting to the payload `Observer` `D65` white when `Nothing`) into the `_METRIC[metric]` row's space before applying its callable. `hex_ramp` is the one srgb-array→CSS-hex projection — rows clip lawfully through the engine (`Color.clip().to_string(hex=True)`), never a hand-rolled int cast and never a literal hex constant. Each arm writes one `Derivation` with typed `frozendict` `measures` keys and `notation` strings, never an overloaded scalar.
- Growth: a new color-derivation operation is one `ColorOp` case carrying its payload plus one acceptor arm folding into `Derivation`, its `tag` extending the one `ColorOpTag` literal; a new colorimetric measure is one `Metric` member plus one `_METRIC` `MetricSpec` row carrying its input `ColorModel` and callable, a measure under a different standard observer the `Observer` axis already on the `Measure` payload; a new filter or CVD type is one `ColorFilter` row; a new gamut-fit method is one `FitMethod` row; a new palette modality is one `Ramp` case, a new interpolation curve one `Interp` row (`catrom`/`spectral`/`spectral-continuous` are those, the AEC-material mixbox pigment palette), a new hue arc one `HueArc` row, a new easing curve one `Easing` row and one `_EASING` callable; a new blend mode is one `BlendMode` row and a new compositing operator one `PorterDuff` row (the full 15-operator Porter-Duff set stands); a new harmony wheel is one `Harmony` row; a new appearance or named model is one `ColorModel` row carrying both engine name columns; a new CCT illuminant method is one `CctMethod` row and a new Planckian method one `Blackbody` row; a new in-conversion adaptation transform is one `AdaptMethod` row and a new standalone CAT one `CamMethod` row; a new adaptation white point is one `Illuminant` row and a new standard observer one `Observer` row keying the nested `_WHITEPOINT`/`MSDS_CMFS`; a new correction solver is one `CorrectMethod` row; a brand-palette constraint is the `anchors` tuple and a masked composite the `mask` tuple on the existing payloads; a batch spectral source is the `MultiSpectralDistributions` member on the existing `ColorSource` union; a new inbound exchange intake is one `ColorOp` case decoding its wire (the `Spot` CxF seed is exactly that); a new palette egress is one projection beside `hex_ramp`, never a per-consumer re-spelling; a finer working grid is the one `_WORKING_SHAPE` policy row — never a parallel surface; zero new surface.
- Boundary: no chart/scene rendering (that stays at the visual owners); no ICC/LUT/CCTF raster egress, no plate authoring, no TAC gate, no CxF device-half — `graphic/color/managed#MANAGED` owns them exclusively (this owner resolves the color-space and tone-curve provenance the managed leg consumes but writes no raster and imports no `pillow`); derive mints NO receipt — the deleted forms are the `ColorReceipt`/`ColorReceiptWire` parallel receipt rail (a substrate returns values; the LUT/plate/swatch terminals mint `ArtifactReceipt.Color` on managed), a folder-minted `CapacityLimiter`/`to_thread` lane beside the runtime `offload`, a literal hex constant anywhere a derive projection resolves color, and a per-consumer RGB→hex re-spelling beside `hex_ramp`. The gamut-mapping, filter (CVD + W3C), compositing (mask/layer/weighted-mix), smooth-or-categorical palette, harmony, average/nearest-snap, blackbody, OKLab-perceptual difference, and contrast/luminance/distance legs are ColorAide's, and colour-science keeps spectral/CAM/CIE-difference/cross-illuminant-adaptation/temperature/colorimetric-index/CCM/graph-query — never a second convert, gamut-fit, or contrast engine. The colour-cxf intake is COLOUR-HALF only: `read_cxf` decodes each `Object`'s measured `ColorValues` (spectral/Lab/XYZ) for the palette seed, while the device-half `ColorCmykplusN`/`ColorRecipe` separations declaration is managed's (two readers of one immutable `.cxf`, disjoint projections, no trample). The difference family spans both engines under the ONE `_METRIC` surface — colour-science `delta_E` owns the CIE/CMC/DIN99 Lab-array rows, ColorAide `Color.delta_e` owns the OKLab-perceptual sRGB-coord rows — so the table is one measure surface keyed by `Metric`, never two parallel difference owners.
- Packages: `colour-science` (`convert`/`describe_conversion_path`/`chromatic_adaptation`/`delta_E`/`whiteness`/`yellowness`/`dominant_wavelength`/`complementary_wavelength`/`colour_rendering_index`/`colour_fidelity_index`/`colour_correction`/`wavelength_to_XYZ`/`msds_to_XYZ`/`sd_to_XYZ`/`temperature.CCT_to_xy`/`temperature.xy_to_CCT`/`domain_range_scale`, the `MSDS_CMFS`/`SDS_ILLUMINANTS`/`CCS_ILLUMINANTS` registries, `SpectralDistribution`/`MultiSpectralDistributions`/`SpectralShape`+`align` the measurement-side resample; the `CctMethod` four-string illuminant literal set and the `describe_conversion_path` `mode=`/`print_callable=` capture kwargs stand ahead of the folder catalog — the catalog-deepen obligation rides `.api/colour-science.md`), `coloraide` (`everything.ColorAll` the all-plugins engine — 8-method `FIT_MAP` + `in_pointer_gamut`/`fit_pointer_gamut`, 11-row `FILTER_MAP`, 13-method `DE_MAP` incl. `helmlab`, 9-method `INTERPOLATE_MAP` incl. the mixbox `spectral` curves, 8-wheel `harmonies.SUPPORTED`, 16-mode/15-operator `layer` axes, `mask`/`weighted_mix`/`average`/`closest`/`steps`/`discrete`/`blackbody`/`cct`/`contrast`/`luminance`/`clip`/`to_string(hex=True)`, the `ease*` progress callables), `colour-cxf` (`read_cxf` → the `cxf3.CxF.resources.{object_collection.object_value, color_specification_collection.color_specification}` spine; `Object.color_values.choice` yielding `ReflectanceSpectrum(value, start_wl, color_specification)`/`ColorCielab(l, a, b)`/`ColorCiexyz(x, y, z)`; `ColorSpecification.tristimulus_spec{observer, illuminant_or_custom_illuminant}` + `measurement_spec[].wavelength_range.increment` the measurement context — all cp315-wheel-verified field names), `expression` (`tagged_union`/`case`/`tag`, `Option`, `Map.of_seq` the V16 table spine), `numpy` (array carriers, `ravel`/`asarray`/`clip`), `beartype` (`@beartype` + `vale.Is` the refined-payload admission), runtime (`RuntimeRail`, `LanePolicy.offload`/`Modality.INTERPRETER` the one execution bound).

```python signature
from collections.abc import Callable
from dataclasses import dataclass
from enum import Enum, StrEnum
from typing import Annotated, Final, Literal, NamedTuple, assert_never

import colour
import numpy as np
from beartype import beartype
from beartype.vale import Is
from coloraide import ease, ease_in, ease_in_out, ease_out, linear
from coloraide.everything import ColorAll as Color
from colour import MultiSpectralDistributions, SpectralDistribution, SpectralShape
from expression import Option, case, tag, tagged_union
from expression.collections import Map
from numpy.typing import NDArray

from rasm.runtime.faults import RuntimeRail
from rasm.runtime.lanes import LanePolicy, Modality

lazy from colour_cxf import (
    cxf3,
    read_cxf,
)  # CxF3 spot-library intake (cold): the proxy reifies on first Spot-arm use, decoding a print partner's .cxf into the palette source

type Tristimulus = NDArray[np.float64]
type Palette = NDArray[np.float64]  # N-swatch (N, 3) srgb rows — the re-homed carrier chart/table/diagram/drawing consume
type Wavelength = Annotated[float, Is[lambda nm: 360.0 <= nm <= 830.0]]
type ColorSource = SpectralDistribution | MultiSpectralDistributions | Tristimulus | Wavelength
type MetricInput = Tristimulus | SpectralDistribution
type Kelvin = Annotated[float, Is[lambda k: k > 0.0]]
type Chromaticity = tuple[float, float]
type CctSource = Kelvin | Chromaticity
type Amount = Annotated[float, Is[lambda a: 0.0 <= a <= 1.0]]
type PaletteCount = Annotated[int, Is[lambda n: n >= 1]]
type Spacing = Annotated[float, Is[lambda s: s >= 0.0]]
type ColorOpTag = Literal["convert", "adapt", "gamut", "filter", "palette", "compose", "temperature", "measure", "correct", "spot"]


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
    # canonical HERE (E6): the in-conversion chromatic_adaptation_transform registry; managed composes, never re-declares.
    VON_KRIES = "Von Kries"
    BRADFORD = "Bradford"
    CAT02 = "CAT02"
    CAT16 = "CAT16"
    CMCCAT2000 = "CMCCAT2000"
    SHARP = "Sharp"


class CamMethod(StrEnum):
    VON_KRIES = "Von Kries"
    CMCCAT2000 = "CMCCAT2000"
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
    POINTER = "pointer"  # routes to Color.fit_pointer_gamut() — the real-surface printable-gamut map for the print plane, not a FIT_MAP method= key


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
    CATROM = "catrom"  # Catmull-Rom spline through the stops
    SPECTRAL = "spectral"  # mixbox pigment-mixing (Kubelka-Munk) — the paint/material palette the AEC-material color plane needs
    SPECTRAL_CONTINUOUS = "spectral-continuous"  # continuous mixbox pigment interpolation


class HueArc(StrEnum):
    SHORTER = "shorter"
    LONGER = "longer"
    INCREASING = "increasing"
    DECREASING = "decreasing"


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
    # the full 15-operator compositing set the engine registers — porter_duff.SUPPORTED, closed.
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
    space: ColorModel  # the input space the row demands; Measure resolves sample+reference into it before applying fn
    fn: Callable[[MetricInput, MetricInput], float]


# the one measurement-side working grid: an arbitrary-grid measured spectrum aligns here ONCE at
# ingress before sd_to_XYZ/msds_to_XYZ/CRI/CFI; LUT-authoring interpolators stay managed's.
_WORKING_SHAPE: Final[SpectralShape] = SpectralShape(360.0, 830.0, 1.0)

# Observer-keyed whitepoint XYZ derived once from CCS_ILLUMINANTS so a 2-degree vs 10-degree adaptation reads the right white.
_WHITEPOINT: Final[Map[Observer, Map[Illuminant, Tristimulus]]] = Map.of_seq(
    (
        observer,
        Map.of_seq(
            (illum, np.asarray(colour.xy_to_XYZ(colour.CCS_ILLUMINANTS[observer.value][illum.value]), dtype=np.float64)) for illum in Illuminant
        ),
    )
    for observer in Observer
)

# CxF EobserverType.value -> the Observer (colour MSDS_CMFS key); the two named standard observers, Custom_CMF folding to 2-degree.
_CXF_OBSERVER: Final[Map[str, Observer]] = Map.of_seq([("2_Degree", Observer.CIE_1931_2), ("10_Degree", Observer.CIE_1964_10)])
# CxF EilluminantType.value -> the Illuminant (colour SDS_ILLUMINANTS key); an unmapped illuminant folds to D65.
_CXF_ILLUMINANT: Final[Map[str, Illuminant]] = Map.of_seq((illum.value, illum) for illum in Illuminant)

# Color.steps/discrete take a progress= easing callable; _EASING resolves the Easing vocabulary to the package's own curves.
_EASING: Final[Map[Easing, Callable[[float], float]]] = Map.of_seq([
    (Easing.LINEAR, linear),
    (Easing.EASE, ease),
    (Easing.EASE_IN, ease_in),
    (Easing.EASE_OUT, ease_out),
    (Easing.EASE_IN_OUT, ease_in_out),
])

# Each row binds the metric to the input ColorModel its engine demands — colour delta_E/whiteness Lab/XYZ, dominant/complementary xy,
# CRI/CFI a SpectralDistribution, the ColorAide rows sRGB — so Measure resolves sample+reference into that space before applying fn.
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
    (Metric.LUMINANCE, MetricSpec(ColorModel.SRGB, lambda a, _b: Color("srgb", list(a)).luminance())),
    (
        Metric.WHITENESS,
        MetricSpec(ColorModel.XYZ, lambda a, b: float(np.ravel(colour.whiteness(a, b))[0])),
    ),  # CIE 2004 returns [W, T]; the whiteness index is the head
    (Metric.YELLOWNESS, MetricSpec(ColorModel.XYZ, lambda a, _b: float(colour.yellowness(a)))),
    (
        Metric.DOMINANT_WAVELENGTH,
        MetricSpec(ColorModel.XYY, lambda a, b: float(np.ravel(colour.dominant_wavelength(a[:2], b[:2])[0])[0])),
    ),  # returns (wl, xy_wl, xy_cwl); the wl head is the measure
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
    palette: tuple[tuple[str, ...], str, PaletteCount, Spacing, ColorModel, Ramp, tuple[str, ...]] = case()
    compose: tuple[tuple[str, ...], ColorModel, BlendMode, PorterDuff, tuple[float, ...], tuple[str, ...]] = case()
    temperature: tuple[CctSource, CctMethod, Blackbody, ColorModel] = case()
    measure: tuple[ColorSource, ColorModel, Option[Tristimulus], Metric, Observer] = case()
    correct: tuple[Tristimulus, Tristimulus, CorrectMethod] = case()
    spot: tuple[bytes, ColorModel] = case()

    @staticmethod
    @beartype
    def Convert(
        value: ColorSource,
        source: ColorModel,
        target: ColorModel,
        adapt: AdaptMethod = AdaptMethod.BRADFORD,
        observer: Observer = Observer.CIE_1931_2,
    ) -> "ColorOp":
        return ColorOp(convert=(value, source, target, adapt, observer))

    @staticmethod
    def Adapt(
        value: Tristimulus, source: Illuminant, target: Illuminant, method: CamMethod = CamMethod.VON_KRIES, observer: Observer = Observer.CIE_1931_2
    ) -> "ColorOp":
        return ColorOp(adapt=(value, source, target, method, observer))

    @staticmethod
    def Gamut(value: Tristimulus, source: ColorModel, target: ColorModel, method: FitMethod = FitMethod.OKLCH_CHROMA) -> "ColorOp":
        return ColorOp(gamut=(value, source, target, method))

    @staticmethod
    @beartype
    def Filter(value: Tristimulus, name: ColorFilter, amount: Amount = 1.0) -> "ColorOp":
        return ColorOp(filter=(value, name, amount))

    @staticmethod
    @beartype
    def Palette(
        seed: tuple[str, ...],
        stop: str,
        count: PaletteCount,
        spacing: Spacing = 0.0,
        space: ColorModel = ColorModel.OKLCH,
        ramp: Ramp = Ramp.Smooth(),
        anchors: tuple[str, ...] = (),
    ) -> "ColorOp":
        return ColorOp(palette=(seed, stop, count, spacing, space, ramp, anchors))

    @staticmethod
    def Compose(
        colors: tuple[str, ...],
        space: ColorModel = ColorModel.OKLCH,
        blend: BlendMode = BlendMode.NORMAL,
        operator: PorterDuff = PorterDuff.SOURCE_OVER,
        weights: tuple[float, ...] = (),
        mask: tuple[str, ...] = (),
    ) -> "ColorOp":
        return ColorOp(compose=(colors, space, blend, operator, weights, mask))

    @staticmethod
    @beartype
    def Temperature(
        value: CctSource, method: CctMethod = CctMethod.DAYLIGHT, planck: Blackbody = Blackbody.OHNO_2013, space: ColorModel = ColorModel.SRGB
    ) -> "ColorOp":
        return ColorOp(temperature=(value, method, planck, space))

    @staticmethod
    def Measure(
        sample: ColorSource,
        metric: Metric,
        source: ColorModel = ColorModel.SRGB,
        reference: Tristimulus | None = None,
        observer: Observer = Observer.CIE_1931_2,
    ) -> "ColorOp":
        return ColorOp(measure=(sample, source, Option.of_optional(reference), metric, observer))

    @staticmethod
    def Correct(measured: Tristimulus, reference: Tristimulus, method: CorrectMethod = CorrectMethod.CHEUNG_2004) -> "ColorOp":
        return ColorOp(correct=(measured, reference, method))

    @staticmethod
    def Spot(document: bytes, target: ColorModel = ColorModel.SRGB) -> "ColorOp":
        return ColorOp(spot=(document, target))


@dataclass(frozen=True, slots=True, kw_only=True)
class Derivation:
    # the derived color VALUE bundle of one ColorOp — a substrate result, never a receipt: no
    # contribute, no wire projection; consumers compose coords/notation/measures directly.
    tag: ColorOpTag
    coords: Tristimulus
    notation: tuple[str, ...] = ()  # Color.to_string() of each output color so a consumer renders without re-instantiating Color
    space: str = ""
    path: tuple[str, ...] = ()
    measures: frozendict[Metric, float] = frozendict()
    in_gamut: bool = True
    pointer_gamut: bool = True  # Pointer's-gamut (real-surface printable) membership — the print-plane predicate beside the space-gamut in_gamut


def hex_ramp(palette: Palette, /) -> tuple[str, ...]:
    # the ONE srgb-array -> CSS-hex projection every band shares (chart theme ranges, drawing pens,
    # diagram inks, legend swatches); rows clip lawfully through the engine, never a hand cast.
    rows = palette if palette.ndim == 2 else palette[np.newaxis, :]
    return tuple(Color("srgb", list(row)).clip().to_string(hex=True) for row in rows)


@dataclass(frozen=True, slots=True, kw_only=True)
class Colorimetry:
    op: ColorOp
    lane: LanePolicy

    async def derive(self) -> RuntimeRail[Derivation]:
        # colour-science + ColorAide are synchronous CPU kernels (spectral integration, delta-E-spaced
        # ramps, N-swatch CxF decode). INTERPRETER modality gives each offload interpreter-local
        # `colour` module state, so the non-thread-local domain_range_scale global never races
        # concurrent derives; the runtime owns the bound and the boundary rail — no folder limiter.
        return await self.lane.offload(Colorimetry._resolved, self.op, modality=Modality.INTERPRETER)

    @staticmethod
    def _resolved(op: ColorOp) -> Derivation:
        match op:
            case ColorOp(tag="convert", convert=(value, source, target, adapt, observer)):
                with colour.domain_range_scale("reference"):
                    coords = np.asarray(Colorimetry._resolve(value, source, target, observer, adapt), dtype=np.float64)
                return Derivation(
                    tag="convert",
                    coords=coords,
                    notation=Colorimetry._notate(target.aide, coords),
                    space=target.science or target.aide or "",
                    path=Colorimetry._path(source, target),
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
                    tag="adapt",
                    coords=adapted,
                    notation=Colorimetry._notate(ColorModel.XYZ.aide, adapted),
                    space="CIE XYZ",
                    path=(observer.value, source.value, target.value, method.value),
                )
            case ColorOp(tag="gamut", gamut=(value, source, target, method)):
                seeded = Color(source.aide, list(value)).convert(target.aide)
                fitted = seeded.clone().fit_pointer_gamut() if method is FitMethod.POINTER else seeded.clone().fit(method=method.value)
                coords = np.asarray(fitted.coords(), dtype=np.float64)
                return Derivation(
                    tag="gamut",
                    coords=coords,
                    notation=(fitted.to_string(),),
                    space=target.aide or "",
                    path=Colorimetry._path(source, target),
                    in_gamut=seeded.in_gamut(target.aide),
                    pointer_gamut=seeded.in_pointer_gamut(),
                )
            case ColorOp(tag="filter", filter=(value, name, amount)):
                origin = Color("srgb", list(value))
                filtered = origin.clone().filter(name.value, amount=amount)
                coords = np.asarray(filtered.coords(), dtype=np.float64)
                return Derivation(
                    tag="filter",
                    coords=coords,
                    notation=(filtered.to_string(),),
                    space="srgb",
                    measures=frozendict({Metric.SEVERITY: origin.delta_e(filtered, method="2000")}),
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
                    tag="palette",
                    coords=coords,
                    notation=tuple(step.to_string() for step in snapped),
                    space="srgb",
                    path=(space.aide or "", *trail),
                    measures=frozendict({Metric.CONTRAST: contrast}),
                )
            case ColorOp(tag="compose", compose=(colors, space, blend, operator, weights, mask)):
                stacked = [Color(row).mask(list(mask)) for row in colors] if mask else list(colors)
                blended = (
                    Color.weighted_mix(stacked, weights=list(weights), space=space.aide, out_space="srgb")
                    if weights
                    else Color.layer(stacked, blend=blend.value, operator=operator.value, space=space.aide, out_space="srgb")
                )
                coords = np.asarray(blended.coords(), dtype=np.float64)
                return Derivation(tag="compose", coords=coords, notation=(blended.to_string(),), space="srgb", path=(blend.value, operator.value))
            case ColorOp(tag="temperature", temperature=(value, method, planck, space)):
                match value:
                    case float() as kelvin:
                        swatch = Color.blackbody(space.aide, kelvin, method=planck.value).convert("srgb")
                        chroma = np.asarray(colour.temperature.CCT_to_xy(kelvin, method=method.value), dtype=np.float64)
                        coords = np.asarray(swatch.coords(), dtype=np.float64)
                        return Derivation(
                            tag="temperature",
                            coords=coords,
                            notation=(swatch.to_string(),),
                            space="srgb",
                            path=(method.value, planck.value),
                            measures=frozendict({
                                Metric.CCT: kelvin,
                                Metric.CHROMATICITY_X: float(chroma[0]),
                                Metric.CHROMATICITY_Y: float(chroma[1]),
                            }),
                        )
                    case (x, y):
                        kelvin = float(np.ravel(colour.temperature.xy_to_CCT(np.asarray((x, y), dtype=np.float64), method=method.value))[0])
                        return Derivation(
                            tag="temperature",
                            coords=np.asarray((x, y), dtype=np.float64),
                            space="CIE xyY",
                            path=(method.value,),
                            measures=frozendict({Metric.CCT: kelvin, Metric.CHROMATICITY_X: x, Metric.CHROMATICITY_Y: y}),
                        )
                    case _ as unreachable:
                        assert_never(unreachable)
            case ColorOp(tag="measure", measure=(sample, source, reference, metric, observer)):
                space, fn = _METRIC[metric]
                with colour.domain_range_scale("reference"):
                    a = Colorimetry._resolve(sample, source, space, observer)
                    b = reference.map(lambda ref: Colorimetry._resolve(ref, source, space, observer)).default_with(
                        lambda: Colorimetry._white(space, observer)
                    )
                    value = float(fn(a, b))
                coords = a if isinstance(a, np.ndarray) else np.empty(0, dtype=np.float64)
                return Derivation(tag="measure", coords=coords, space=space.science or space.aide or "", measures=frozendict({metric: value}))
            case ColorOp(tag="correct", correct=(measured, reference, method)):
                corrected = np.asarray(colour.colour_correction(measured, measured, reference, method=method.value), dtype=np.float64)
                return Derivation(
                    tag="correct", coords=corrected, notation=Colorimetry._notate(ColorModel.SRGB.aide, corrected), space="sRGB", path=(method.value,)
                )
            case ColorOp(tag="spot", spot=(document, target)):
                with colour.domain_range_scale("reference"):
                    swatches = Colorimetry._swatches(read_cxf(document), target)
                coords = np.array([row[0] for row in swatches], dtype=np.float64) if swatches else np.empty((0, 3), dtype=np.float64)
                return Derivation(
                    tag="spot",
                    coords=coords,
                    notation=tuple(row[1] for row in swatches),
                    space=target.science or target.aide or "",
                    path=("cxf", *(row[2] for row in swatches)),
                )
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
    ) -> MetricInput:
        aligned = Colorimetry._aligned(value)
        if target.spectral:
            return aligned  # CRI/CFI read the resampled SpectralDistribution directly; no coordinate form exists to resolve into
        cmfs = colour.MSDS_CMFS[observer.value]
        match aligned:
            case float() as nm:
                return np.asarray(colour.convert(colour.wavelength_to_XYZ(nm, cmfs), "CIE XYZ", target.science), dtype=np.float64)
            case MultiSpectralDistributions():
                return np.asarray(
                    colour.convert(colour.msds_to_XYZ(aligned, cmfs, colour.SDS_ILLUMINANTS[Illuminant.D65.value]), "CIE XYZ", target.science),
                    dtype=np.float64,
                )
            case _ if source.aide is not None and target.aide is not None and (source.science is None or target.science is None):
                # a wide-gamut/HSL/HWB endpoint (`science is None`) colour-science has no convert node for rides the
                # ColorAide `Color.convert` gateway so `Convert` is total over every ColorModel pair rather than
                # handing `None` to `colour.convert`; the coords enter in `source.aide` and leave in `target.aide`.
                return np.asarray(
                    Color(source.aide, list(np.ravel(np.asarray(aligned, dtype=np.float64)))).convert(target.aide).coords(), dtype=np.float64
                )
            case _:
                return np.asarray(
                    colour.convert(aligned, source.science, target.science, chromatic_adaptation_transform=adapt.value), dtype=np.float64
                )

    @staticmethod
    def _white(space: ColorModel, observer: Observer) -> Tristimulus:
        if space.spectral:
            return np.empty(0, dtype=np.float64)  # unary spectral metrics ignore the reference operand
        return np.asarray(Colorimetry._resolve(_WHITEPOINT[observer][Illuminant.D65], ColorModel.XYZ, space, observer), dtype=np.float64)

    @staticmethod
    def _notate(space_aide: str | None, coords: Tristimulus) -> tuple[str, ...]:
        if space_aide is None or coords.size == 0:  # appearance-correlate or measure outputs have no CSS form
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
    def _swatches(cxf: "cxf3.CxF", target: ColorModel) -> list[tuple[Tristimulus, str, str]]:
        resources = cxf.resources
        specs = frozendict({
            row.id: row
            for row in (
                resources.color_specification_collection.color_specification if resources and resources.color_specification_collection else ()
            )
        })
        collection = resources.object_collection if resources else None
        rows: list[tuple[Tristimulus, str, str]] = []
        for obj in collection.object_value if collection else ():
            resolved = Colorimetry._resolve_spot(obj, specs, target)
            if resolved is not None:
                notes = Colorimetry._notate(target.aide, resolved)
                rows.append((resolved, notes[0] if notes else "", str(obj.name or "")))
        return rows

    @staticmethod
    def _resolve_spot(obj: "cxf3.Object", specs: "frozendict[str, cxf3.ColorSpecification]", target: ColorModel) -> Tristimulus | None:
        values = list(obj.color_values.choice) if obj.color_values else []
        spectrum = next((member for member in values if isinstance(member, cxf3.ReflectanceSpectrum)), None)
        if (
            spectrum is not None
            and (resolved := Colorimetry._spectrum_xyz(spectrum, specs.get(spectrum.color_specification or ""), target)) is not None
        ):
            return resolved  # the CxF measurement context (illuminant/observer/grid) resolves the primary spectral spot payload through sd_to_XYZ
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
        sd = raw.align(_WORKING_SHAPE)  # freshly built from the CxF grid — resampled to the working shape before integration
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
        measurements = spec.measurement_spec
        increment = measurements[0].wavelength_range.increment if measurements and measurements[0].wavelength_range is not None else None
        return observer, illuminant, int(increment or 10)
```

`Colorimetry` is the substrate every visual plane draws color from, and its egress is VALUES: `Derivation.coords`/`notation`/`measures` compose into `graphic/style#STYLE` theme rows, `visualization/chart` theme ranges, `visualization/table` palettes, `visualization/diagram/draw` inks, `scene/render` colormaps, and the `drawing/*` discipline pens — each consumer reads the value bundle and mints its own artifacts; the LUT/plate/swatch/plate-set terminals that DO produce artifacts mint `ArtifactReceipt.Color` on `graphic/color/managed#MANAGED`, never here. `Palette` and `hex_ramp` are derive-owned: the one N-swatch carrier and the one srgb→CSS-hex projection every band shares, so a recolor anywhere in the corpus is a palette swap against this owner and a literal hex constant anywhere downstream is a defect. `AdaptMethod` is declared here once; managed composes it for its ICC/LUT egress provenance and re-declares nothing.

The engine boundary is law: colour-science owns spectral/colorimetric truth (SPD→XYZ integration over the `_WORKING_SHAPE`-aligned grid, CAM appearance, CIE/CMC/DIN99 Lab-array difference, cross-illuminant adaptation, temperature, colorimetric indices, CCM), ColorAide owns the per-color presentation legs (gamut fit, CVD+W3C filters, palette interpolation incl. the mixbox pigment curves, mask/layer/weighted-mix compositing, harmony, blackbody, OKLab-perceptual difference, WCAG contrast, CSS notation), and colour-cxf owns the CxF3 exchange shape — derive never parses CxF XML by hand, never re-integrates an SPD, never parses CSS inside colour-science, and never gamut-maps inside colour-cxf. `everything.ColorAll` is the single all-plugins `Color` so OKLCH/HCT and the registered fit/filter/harmony/blend/average/closest/blackbody plugins resolve without a per-space class; execution rides the runtime lane exclusively — `Modality.INTERPRETER` isolation makes the `domain_range_scale` process-global race-free per interpreter, the policy's capacity is the one bound, and the offload's `async_boundary` is the one fault seam, so the page mints no limiter, no thread hop, and no local boundary wrap.
