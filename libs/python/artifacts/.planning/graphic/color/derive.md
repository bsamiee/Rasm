# [PY_ARTIFACTS_GRAPHIC_COLOR_DERIVE]

The upstream color-derivation owner feeding consistent, perceptually-correct color into every visual sub-domain. `Colorimetry` is ONE owner over a two-engine dispatch — colour-science (NumFOCUS-affiliated, BSD-3, NumPy-backed, pure-Python, host-free) carries CIE color, spectral distributions, 30+ color-space conversions through the universal `colour.convert` gateway, the CIECAM02 / CAM16 appearance models, `delta_E` difference, chromatic adaptation, and the bidirectional correlated-color-temperature axis; ColorAide (pure-Python, zero-native, the `everything.ColorAll` all-plugins engine) carries the genuinely-disjoint legs colour-science lacks — `Color.fit(method=...)` perceptual gamut mapping with the `in_gamut` predicate, `Color.filter(name=...)` CVD simulation scored by `delta_e`, the `Color.steps` `max_delta_e`-spaced perceptually-even palette, the `Color.harmony` named-wheel derivation, the `Color.average` perceptual seed-blend, the `Color.closest` nearest-snap, and the WCAG21 `Color.contrast` safety scalar. `ColorOp` is ONE closed family carrying each operation's typed payload, never an erased `params` bag, dispatched by one total `match` returning `RuntimeRail[ColorReceipt]` — the one typed receipt every arm folds into, never an erased `object`. It hands palettes and color-space conversions to `visualization/chart#CHART`, `scene#SCENE`, `visualization/table#TABLE`, `graphic/marks/encode#MARK`, and the document output, so every visual artifact draws color from one owner; the `ColorReceipt.coords` palette arrays and appearance correlates project to `data/tabular#WIRE` for columnar persistence, and the tone-curve and color-space provenance it resolves feeds the `graphic/color/managed#MANAGED` ICC/LUT raster-egress leg. This page carries no raster egress — every arm resolves on the cp315 core and folds into `ColorReceipt`. It closes the `COLOR_MANAGED_VISUAL_PIPELINE` and `COLORBLIND_SAFE_PALETTE` ideas on the derivation side.

## [01]-[INDEX]

- [01]-[DERIVE]: the two-engine `Colorimetry` owner over the closed-payload `ColorOp` family — convert/difference/temperature/palette/gamut/cvd folding into one typed `ColorReceipt` whose `tag` carries the same closed `ColorOpTag` literal, never a bare `str`; `colour.convert` is the universal colour-science gateway with `describe_conversion_path(mode="Long")` recovering the resolved model-graph onto the receipt `path` and `RGB_COLOURSPACES` the named-space registry, `colour.temperature.CCT_to_xy`/`xy_to_CCT` the bidirectional correlated-color-temperature axis, ColorAide `everything.ColorAll` owns the disjoint gamut/CVD/contrast legs plus the `steps`-spaced, `harmony`-derived, `average`-blended, and `closest`-snapped perceptual-palette modalities; `ColorModel` is the dual-name `ModelNames(science, aide, spectral)` vocabulary collapsing the two disjoint engine spellings into one row whose `science` column keys colour-science and whose `aide` column keys ColorAide so one model value serves both engines — never two parallel model enums; `DeltaMethod` keys colour-science `DELTA_E_METHODS`, `AdaptMethod` keys `CHROMATIC_ADAPTATION_TRANSFORMS` (the `convert` `chromatic_adaptation_transform` registry, never the standalone-function `CHROMATIC_ADAPTATION_METHODS`), `CctMethod` keys the four `colour.temperature` illuminant methods, and `CvdFilter`/`Harmony`/`FitMethod` key the ColorAide filter/harmony/fit plugin maps, all carried as case payload; `ColorSource = SpectralDistribution | Tristimulus` is the convert input union; `CctSource = Kelvin | Chromaticity` is the bidirectional `Temperature` payload discriminating CCT-to-chromaticity from chromaticity-to-CCT by input shape.

## [02]-[DERIVE]

- Owner: `Colorimetry` the one color-derivation owner discriminating operation over the closed `ColorOp` family; `ColorOp` an `expression.tagged_union` whose every case carries its own typed payload, never a shared erased `params` dict; `ColorReceipt` the one typed result every arm folds into, its `tag` carrying the same closed `ColorOpTag` literal the `ColorOp` discriminant carries — never a bare `str` the consumer must re-validate — and its `coords`/`path`/`scalar`/`in_gamut` slots recovering each algorithm choice; colour-science is the conversion, appearance-model, difference, adaptation, correlated-color-temperature, and `describe_conversion_path` graph-query engine on the cp315 core; ColorAide the pure-Python gamut-mapping, CVD, perceptual-palette, harmony, average-blend, nearest-snap, and contrast engine on the cp315 core; `ColorModel` the dual-name `ModelNames` vocabulary whose `science`/`aide` columns key the two engines from one model value so a `Palette`/`Gamut` arm reaches ColorAide through `space.aide` while a `Convert` arm reaches colour-science through `source.science`, never a per-engine model enum; the `everything.ColorAll` all-plugins `Color` binds once at the capsule boundary per the manifest import policy, never re-imported per `derive`. The ICC/LUT/CCTF managed raster egress is NOT this owner's concern — it is `graphic/color/managed#MANAGED`'s; `Colorimetry` resolves the tone-curve and space provenance that leg consumes but writes no raster.
- Cases: `ColorOp` cases — `Convert(value, source, target, adapt)` (any model-pair through the universal `colour.convert` gateway, the typed `ColorSource = SpectralDistribution | Tristimulus` payload discriminating spectral input from tristimulus arrays, absorbing the former two-call SD->XYZ->display and CIECAM02/CAM16 appearance arms as `ColorModel` source/target values reached through `source.science`/`target.science`, the receipt `path` recovered from `colour.describe_conversion_path(mode="Long")` as the resolved model-graph rather than a hand-built endpoint pair) · `Difference(reference, sample, method)` (`colour.delta_E` over the `DELTA_E_METHODS` registry as a `DeltaMethod` row) · `Temperature(value, method)` (one bidirectional correlated-color-temperature axis whose `Kelvin` payload folds `colour.temperature.CCT_to_xy` to a chromaticity on `coords` and whose `Chromaticity` payload folds the inverse `colour.temperature.xy_to_CCT` to a `Kelvin` on `scalar`, the `CctMethod` illuminant row carried on `path`, the direction discriminated by the `CctSource` input shape so forward and inverse share one case — never a parallel inverse-temperature surface) · `Palette(seed, stop, count, spacing, space, harmony, anchors)` (one perceptual-palette axis whose `harmony is None` arm folds the perceptually-even colorblind-aware ramp via `Color.steps` `max_delta_e`-spaced from the `seed` blend and whose `Harmony` arm folds the named wheel via `Color.harmony`, the multi-color `seed` tuple collapsing to one perceptually-weighted base via `Color.average` so a one-color and a multi-color seed share the arm, the `anchors` brand-color tuple snapping each ramp step to its nearest admissible color via `Color.closest` keyed on `delta_e` so a fixed palette is recoverable, both carrying the endpoint WCAG21 `contrast` as the safety scalar — never a parallel ramp-versus-harmony-versus-snap surface) · `Gamut(value, source, target, method)` (`Color(source.aide, value).convert(target.aide).clone().fit(method=...)` perceptual gamut mapping into a destination space, the pre-fit `in_gamut(target.aide)` predicate carried as evidence, the colour-science `describe_conversion_path(mode="Long")` graph on the receipt `path`) · `Cvd(value, filter, severity)` (`Color("srgb", value).clone().filter(name=...)` deficiency simulation, the source-to-filtered `delta_e` shift carried as the severity scalar) — matched by one total `match`/`case`.
- Entry: `Colorimetry.derive` is `async` over the runtime `async_boundary` and dispatches the `ColorOp` case, returning one `RuntimeRail[ColorReceipt]` whose typed `ColorReceipt` carries the resolved `coords` array, the `describe_conversion_path` model-graph `path`, the difference or contrast `scalar`, and the target-space `in_gamut` flag — never an erased `object`, so a re-admitting consumer recovers every algorithm choice from the receipt. Every arm resolves synchronously on the cp315 core inside the async capsule: colour-science is pure-Python NumPy and ColorAide is pure-Python, so no leg crosses a process seam and no arm forces the async dispatch — the `async_boundary` is the uniform consumer contract the visual owners `await`, mirroring the settled `visualization/chart#EXPORT` `ChartExport.render -> RuntimeRail[ArtifactReceipt]` rail shape so a chart/table/scene consumer reaches color through one awaited rail rather than a per-engine sync branch. The result is a settled rail the visual owners consume, never a re-derivation per engine.
- Auto: spectral-source `Convert` folds the `SpectralDistribution` through `colour.convert` with the source `ColorModel.SPECTRAL.science` routing to the target model internally, tristimulus-source `Convert` routes XYZ/Lab/RGB/CAM pairs directly through `source.science`/`target.science`, the `AdaptMethod` (a `CHROMATIC_ADAPTATION_TRANSFORMS` key) selecting `convert`'s `chromatic_adaptation_transform`, the receipt carrying the `describe_conversion_path(mode="Long")` resolved model-graph as `path`; `Difference` folds the two arrays through `delta_E` under the `DeltaMethod` (a `DELTA_E_METHODS` key) registry row into the scalar; `Temperature` discriminates on the `CctSource` shape — a `Kelvin` float folds `colour.temperature.CCT_to_xy(value, method=method.value)` to the chromaticity `coords`, a `Chromaticity` tuple folds `colour.temperature.xy_to_CCT(np.asarray(value), method=method.value)` to the `Kelvin` `scalar` — the `CctMethod` illuminant string on `path`; `Palette` folds the multi-color `seed` tuple through `Color.average(seeds, space=space.aide, out_space='srgb')` to one perceptually-weighted base, then discriminates on the `Harmony | None` payload — the `None` arm folds the base and `stop` endpoint through `Color.steps([base, stop], steps=count, max_steps=count, max_delta_e=spacing, delta_e="2000", space=space.aide, out_space='srgb')` for perceptually-even spacing, the `Harmony` arm folds the base through `base.harmony(name, space=space.aide, out_space='srgb')` for the named wheel — then, when `anchors` is non-empty, snaps each ramp step through `step.closest(anchors, method="2000")` so every emitted color is the nearest admissible brand color, all arms snapshotting the endpoint `contrast` (WCAG21) as the colorblind-safety scalar; `Gamut` folds the source through `Color(source.aide, value).convert(target.aide)` then a `clone().fit(method=...)`, carrying the pre-fit `in_gamut(target.aide)` predicate as evidence so a no-op fit is recoverable and the colour-science `describe_conversion_path` graph as `path`; `Cvd` folds through `Color('srgb', value).clone().filter(name, amount=severity)` and carries the source-to-filtered `delta_e` shift as the deficiency-severity scalar.
- Packages: `colour-science` (`convert` the universal gateway subsuming `sd_to_XYZ`/`XYZ_to_sRGB`/`XYZ_to_RGB`/`RGB_to_XYZ`, `describe_conversion_path(mode="Long", print_callable=...)` the model-graph query feeding the receipt `path`, `RGB_COLOURSPACES` the named-space registry, `delta_E` over the `DELTA_E_METHODS` registry feeding `DeltaMethod`, `CHROMATIC_ADAPTATION_TRANSFORMS` feeding `AdaptMethod`, `colour.temperature.CCT_to_xy`/`xy_to_CCT` over the four-illuminant method axis feeding `CctMethod`, `SpectralDistribution`, `domain_range_scale`), `coloraide` (`everything.ColorAll` as the all-plugins `Color`; `Color.steps` with `max_delta_e`/`delta_e` perceptual spacing, `Color.harmony` named wheels, `Color.average` perceptual seed-blend, `Color.closest` nearest-by-`delta_e` snap, `Color.contrast` WCAG21, `Color.convert`/`Color.clone`/`Color.fit`/`Color.in_gamut`, `Color.filter`/`Color.delta_e`, `Color.coords`), `numpy` (the array backing) on the cp315 core; runtime (`faults.RuntimeRail`/`async_boundary`). No `to_process`/`to_interpreter` seam — every arm resolves in-capsule on the core; the ICC raster apply that does cross a process seam lives on `graphic/color/managed#MANAGED`, not here.
- Growth: a new color-derivation operation is one `ColorOp` case carrying its payload plus one acceptor arm folding into `ColorReceipt`, its `tag` extending the one `ColorOpTag` literal the receipt shares; a new difference metric is one `DeltaMethod` row; a new gamut-fit method is one `FitMethod` row; a new CVD type is one `CvdFilter` row; a new harmony wheel is one `Harmony` row; a new appearance or named model is one `ColorModel` row carrying both its `science` and `aide` (or `None` where one engine lacks it) name columns; a new CCT illuminant method is one `CctMethod` row; a new adaptation transform is one `AdaptMethod` row; a brand-palette constraint is the `anchors` tuple on the existing `Palette` payload, a blended seed the multi-color `seed` tuple — never a parallel ramp/harmony/snap/blend surface; zero new surface.
- Boundary: no chart/scene rendering (that stays at the visual owners); no ICC/LUT/CCTF raster egress (that is `graphic/color/managed#MANAGED`'s exclusively — this owner resolves the tone-curve and color-space provenance the managed leg consumes but writes no raster and imports no `pillow`); the color arms emit one `ColorReceipt` carrying arrays, scalars, chromaticities, and palettes, consumed inward by the visual and document sub-domains; the gamut-mapping, CVD, perceptual-palette, harmony, average-blend, nearest-snap, and contrast legs are ColorAide's exclusively — the prior hand-rolled Oklab lerp and any bespoke Daltonization transform are the deleted forms, and colour-science keeps spectral/CAM/difference/adaptation/temperature/graph-query, never a second convert, delta-E, gamut-fit, or contrast engine; `everything.ColorAll` is the single all-plugins `Color` so OKLCH/HCT and the registered fit/filter/harmony/average/closest plugins resolve without a per-space class; the `colour.temperature.CCT_to_xy`/`xy_to_CCT` four-method literal set, the `describe_conversion_path` `mode`/`print_callable` capture surface, the `CHROMATIC_ADAPTATION_TRANSFORMS` registry binding, and the ColorAide harmony-name strings stay [03]-[RESEARCH] catalogue-deepen items until the folder `.api` catalogues carry their full literal sets; every other colour-science and ColorAide spelling below is settled fence code.

```python signature
from enum import Enum, StrEnum
from typing import Literal, NamedTuple, assert_never

import colour
import numpy as np
from coloraide.everything import ColorAll as Color
from colour import SpectralDistribution
from expression import case, tag, tagged_union
from msgspec import Struct
from numpy.typing import NDArray

from rasm.runtime.faults import RuntimeRail, async_boundary

type Tristimulus = NDArray[np.float64]
type ColorSource = SpectralDistribution | Tristimulus
type Kelvin = float
type Chromaticity = tuple[float, float]
type CctSource = Kelvin | Chromaticity
type ColorOpTag = Literal["convert", "difference", "temperature", "palette", "gamut", "cvd"]


class ModelNames(NamedTuple):
    science: str
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
    HSV = ModelNames("HSV", "hsv")
    CIECAM02 = ModelNames("CIECAM02", None)
    CAM16 = ModelNames("CAM16", None)


class DeltaMethod(StrEnum):
    CIE2000 = "CIE 2000"
    CIE1994 = "CIE 1994"
    CIE1976 = "CIE 1976"
    CMC = "CMC"
    DIN99 = "DIN99"


class AdaptMethod(StrEnum):
    VON_KRIES = "Von Kries"
    BRADFORD = "Bradford"
    CAT02 = "CAT02"
    CAT16 = "CAT16"
    CMCCAT2000 = "CMCCAT2000"
    SHARP = "Sharp"


class CctMethod(StrEnum):
    DAYLIGHT = "CIE Illuminant D Series"
    KANG = "Kang 2002"
    HERNANDEZ = "Hernandez 1999"
    MCCAMY = "McCamy 1992"


class FitMethod(StrEnum):
    RAYTRACE = "raytrace"
    OKLCH_CHROMA = "oklch-chroma"
    LCH_CHROMA = "lch-chroma"
    MINDE_CHROMA = "minde-chroma"
    SCALE = "scale"
    SCALE_LUMINANCE = "scale-luminance"


class CvdFilter(StrEnum):
    PROTAN = "protan"
    DEUTAN = "deutan"
    TRITAN = "tritan"


class Harmony(StrEnum):
    MONOCHROMATIC = "mono"
    COMPLEMENTARY = "complement"
    SPLIT_COMPLEMENTARY = "split"
    ANALOGOUS = "analogous"
    TRIADIC = "triad"
    SQUARE = "square"
    RECTANGLE = "rectangle"
    WHEEL = "wheel"


@tagged_union(frozen=True)
class ColorOp:
    tag: ColorOpTag = tag()
    convert: tuple[ColorSource, ColorModel, ColorModel, AdaptMethod] = case()
    difference: tuple[Tristimulus, Tristimulus, DeltaMethod] = case()
    temperature: tuple[CctSource, CctMethod] = case()
    palette: tuple[tuple[str, ...], str, int, float, ColorModel, Harmony | None, tuple[str, ...]] = case()
    gamut: tuple[Tristimulus, ColorModel, ColorModel, FitMethod] = case()
    cvd: tuple[Tristimulus, CvdFilter, float] = case()

    @staticmethod
    def Convert(value: ColorSource, source: ColorModel, target: ColorModel, adapt: AdaptMethod = AdaptMethod.BRADFORD) -> "ColorOp":
        return ColorOp(convert=(value, source, target, adapt))

    @staticmethod
    def Difference(reference: Tristimulus, sample: Tristimulus, method: DeltaMethod = DeltaMethod.CIE2000) -> "ColorOp":
        return ColorOp(difference=(reference, sample, method))

    @staticmethod
    def Temperature(value: CctSource, method: CctMethod = CctMethod.DAYLIGHT) -> "ColorOp":
        return ColorOp(temperature=(value, method))

    @staticmethod
    def Palette(seed: tuple[str, ...], stop: str, count: int, spacing: float = 0.0, space: ColorModel = ColorModel.OKLCH, harmony: Harmony | None = None, anchors: tuple[str, ...] = ()) -> "ColorOp":
        return ColorOp(palette=(seed, stop, count, spacing, space, harmony, anchors))

    @staticmethod
    def Gamut(value: Tristimulus, source: ColorModel, target: ColorModel, method: FitMethod = FitMethod.OKLCH_CHROMA) -> "ColorOp":
        return ColorOp(gamut=(value, source, target, method))

    @staticmethod
    def Cvd(value: Tristimulus, filter: CvdFilter, severity: float = 1.0) -> "ColorOp":
        return ColorOp(cvd=(value, filter, severity))


class ColorReceipt(Struct, frozen=True):
    tag: ColorOpTag
    coords: Tristimulus
    path: tuple[str, ...] = ()
    scalar: float = 0.0
    in_gamut: bool = True


class Colorimetry(Struct, frozen=True):
    op: ColorOp

    async def derive(self) -> RuntimeRail[ColorReceipt]:
        return await async_boundary(f"color.{self.op.tag}", self._compute)

    async def _compute(self) -> ColorReceipt:
        match self.op:
            case ColorOp(tag="convert", convert=(value, source, target, adapt)):
                with colour.domain_range_scale("reference"):
                    coords = colour.convert(value, source.science, target.science, chromatic_adaptation_transform=adapt.value)
                return ColorReceipt("convert", np.asarray(coords, dtype=np.float64), path=self._path(source, target))
            case ColorOp(tag="difference", difference=(reference, sample, method)):
                return ColorReceipt("difference", sample, scalar=float(colour.delta_E(reference, sample, method=method.value)))
            case ColorOp(tag="temperature", temperature=(value, method)):
                forward = isinstance(value, float)
                resolved = colour.temperature.CCT_to_xy(value, method=method.value) if forward else colour.temperature.xy_to_CCT(np.asarray(value), method=method.value)
                return ColorReceipt(
                    "temperature",
                    np.asarray(resolved if forward else value, dtype=np.float64),
                    path=(method.value,),
                    scalar=0.0 if forward else float(resolved))
            case ColorOp(tag="palette", palette=(seed, stop, count, spacing, space, harmony, anchors)):
                base = Color.average(list(seed), space=space.aide, out_space="srgb")
                wheel = (
                    base.harmony(harmony.value, space=space.aide, out_space="srgb")
                    if harmony is not None
                    else Color.steps([base, stop], steps=count, max_steps=count, max_delta_e=spacing, delta_e="2000", space=space.aide, out_space="srgb")
                )
                ramp = [step.closest(list(anchors), method="2000") for step in wheel] if anchors else wheel
                coords = np.array([step.coords() for step in ramp], dtype=np.float64)
                return ColorReceipt("palette", coords, path=(space.aide,), scalar=float(Color("srgb", list(coords[0])).contrast(Color("srgb", list(coords[-1])))))
            case ColorOp(tag="gamut", gamut=(value, source, target, method)):
                fitted = Color(source.aide, list(value)).convert(target.aide)
                return ColorReceipt("gamut", np.array(fitted.clone().fit(method=method.value).coords(), dtype=np.float64), path=self._path(source, target), in_gamut=fitted.in_gamut(target.aide))
            case ColorOp(tag="cvd", cvd=(value, cvd_filter, severity)):
                origin = Color("srgb", list(value))
                filtered = origin.clone().filter(cvd_filter.value, amount=severity)
                return ColorReceipt("cvd", np.array(filtered.coords(), dtype=np.float64), scalar=origin.delta_e(filtered, method="2000"))
            case _:
                assert_never(self.op)

    @staticmethod
    def _path(source: ColorModel, target: ColorModel) -> tuple[str, ...]:
        captured: list[str] = []
        colour.describe_conversion_path(source.science, target.science, mode="Long", print_callable=captured.append)
        return (source.science, *captured, target.science)
```

## [03]-[RESEARCH]

- [TEMPERATURE] [RESEARCH]: the colour-science `colour.temperature.CCT_to_xy(CCT, method=...)` and inverse `colour.temperature.xy_to_CCT(xy, method=...)` correlated-color-temperature spellings reflect in the folder `.api` catalogue for `colour-science` as the `[CCT and chromaticity]` row `CCT_to_xy(CCT, method)`/`xy_to_CCT(xy, method)`, so the call surface is settled fence code; the bidirectional `ColorOp.Temperature` case (the `CctSource = Kelvin | Chromaticity` payload discriminating direction by input shape, the `CctMethod` carried on `path`) is settled. The one open leg is the `method` literal set — the catalogue row names a `method` axis without enumerating the four illuminant strings, so the `CctMethod` vocabulary (`{"CIE Illuminant D Series", "Kang 2002", "Hernandez 1999", "McCamy 1992"}`) and the top-level `colour.CCT_to_xy` daylight-default re-export stay a marked RESEARCH catalogue-deepen item until a `colour.temperature` method-literal reflection pass lands. Close-condition: `.api` catalogue enumerates the `CCT_to_xy`/`xy_to_CCT` four-method literal set.
- [CONVERSION_PATH] [RESEARCH]: the `Colorimetry._path` graph capture spells `describe_conversion_path(source, target, mode="Long", print_callable=captured.append)`, but the folder `.api` catalogue verifies the two-argument `describe_conversion_path(source, target)` call without the `mode`/`print_callable` capture kwargs or the per-line callable contract. The full signature is `describe_conversion_path(source, target, mode='Short', width=79, padding=3, print_callable=print, **kwargs) -> None` — `mode='Long'` is load-bearing: the default `'Short'` prints a seven-line truncated summary while `'Long'` yields the nineteen-line resolved model-graph the receipt `path` claims, so `mode='Long'` is settled here, not optional. The `mode`/`print_callable`-fed capture and the `(source, *captured, target)` path tuple stay a marked RESEARCH catalogue-deepen seam until a `describe_conversion_path` signature reflection lands; the `Convert`/`Gamut` receipt-`path` shape is settled. Close-condition: `.api` catalogue carries `describe_conversion_path` with `mode`/`print_callable`.
- [COLOUR_SCIENCE]: the colour-science universal `convert` gateway, `describe_conversion_path` model-graph query, the `RGB_COLOURSPACES` named-space registry, `delta_E` over the `DELTA_E_METHODS` registry, `SpectralDistribution`, and `domain_range_scale` spellings verify against the folder `.api` catalogue for `colour-science` (`0.4.7` reflected, cp315-core pure-Python). `AdaptMethod` keys `CHROMATIC_ADAPTATION_TRANSFORMS` (the matrix registry `convert`'s `chromatic_adaptation_transform` kwarg resolves — `Bradford`, `CAT02`, `CAT16`, `Von Kries`, `CMCCAT2000`, `Sharp`, `XYZ Scaling`), NOT the catalogue-listed `CHROMATIC_ADAPTATION_METHODS` which keys the standalone `chromatic_adaptation()` function (`Von Kries`, `Zhai 2018`, `Li 2025`, ...) — the two registries share names like `Von Kries` but `Bradford`/`CAT02`/`CAT16`/`Sharp` exist only in the transforms registry and `Zhai 2018`/`Li 2025` only in the methods registry, so binding `AdaptMethod` to the methods registry breaks the default `Bradford` convert path; the `CHROMATIC_ADAPTATION_TRANSFORMS` row is a [03]-[RESEARCH] catalogue-deepen item since the `.api` lists only `CHROMATIC_ADAPTATION_METHODS`. The `Convert` arm routes spectral input by the `ColorSource` payload type and CIECAM02/CAM16 through the universal `convert` gateway keyed by the target `ColorModel.science`, the `AdaptMethod` selecting `chromatic_adaptation_transform`, the receipt `path` recovered from `describe_conversion_path(mode="Long")`; the universal `convert` subsumes the per-pair `sd_to_XYZ`/`XYZ_to_sRGB`/`XYZ_to_RGB`/`RGB_to_XYZ` transforms, so naming them as separate engine members is the deleted sprawl; the `Difference` arm folds `delta_E` over the `DELTA_E_METHODS` registry. The `cctf_encoding`/`read_LUT`/`write_LUT`/`LUTSequence`/`write_image` tone-and-egress surface is NOT this page's — it is `graphic/color/managed#MANAGED`'s; this owner cites colour-science only for derivation. Every spelling other than the `CHROMATIC_ADAPTATION_TRANSFORMS` and `describe_conversion_path` `mode`/`print_callable` catalogue-deepen seams is settled colour-science fence code.
- [COLORAIDE]: the ColorAide `everything.ColorAll` (the all-plugins `Color`), `Color.convert(space)`, `Color.clone()`, `Color.fit(method=...)`, `Color.in_gamut(space)`, `Color.filter(name, amount=...)`, `Color.delta_e(color, method=...)`, `Color.contrast(color)`, `Color.steps(colors, steps=, max_steps=, max_delta_e=, delta_e=, space=, out_space=)`, `Color.harmony(name, space=, out_space=)`, `Color.average(colors, space=, out_space=)`, `Color.closest(colors, method=...)`, and `Color.coords()` spellings verify against the folder `.api` catalogue for `coloraide` (`8.8.1` reflected, cp315 pure-Python). ColorAide spaces are lowercase-hyphenated registered names (`srgb`/`oklch`/`oklab`/`lab-d65`/`lch-d65`/`xyz-d65`/`xyy`/`hsv`), disjoint from the colour-science model strings, so every ColorAide call reaches the space through `ColorModel.aide` and every colour-science call through `ColorModel.science` — the single `ModelNames(science, aide)` row carrying both spellings, a colour-science name passed to a ColorAide call raising `KeyError`/`ValueError` is the deleted defect. The `Gamut` arm folds `Color(source.aide, coords).convert(target.aide)` then `clone().fit(method=FitMethod)` carrying `in_gamut(target.aide)`, the `Cvd` arm folds `Color("srgb", coords).clone().filter(CvdFilter, amount=severity)` carrying `delta_e(method="2000")`, and the `Palette` arm folds the multi-color `seed` through `Color.average(seeds, space=space.aide, out_space='srgb')` to one base then discriminates the `Harmony | None` payload — the `None` arm folds `Color.steps([base, stop], steps=count, max_delta_e=spacing, delta_e="2000", space=space.aide, out_space='srgb')` and the `Harmony` arm folds `base.harmony(name, space=space.aide, out_space='srgb')` — then snaps each step through `step.closest(anchors, method="2000")` when `anchors` is non-empty, reading each result `coords()` and the endpoint `contrast()`. The six `FitMethod` rows (`raytrace`/`oklch-chroma`/`lch-chroma`/`minde-chroma`/`scale`/`scale-luminance`), the three `CvdFilter` rows (`protan`/`deutan`/`tritan`), and the eight `delta_e` metrics (`76`/`2000`/`94`/`cmc`/`hyab`/`itp`/`jz`/`ok`) are the registered method/filter/metric names, never parallel mapping functions or a hand-rolled Daltonization transform. The `Color.harmony` call surface and its `space`/`out_space` kwargs are settled, but the `.api` catalogue documents the call without enumerating the registered harmony names; every other spelling is settled ColorAide fence code; `everything.ColorAll` imports as `Color` at boundary scope per the manifest import policy.
- [HARMONY] [RESEARCH]: the `Harmony` `StrEnum` carries the ColorAide harmony-wheel names (`mono`/`complement`/`split`/`analogous`/`triad`/`square`/`rectangle`/`wheel`) as the `Color.harmony(name, ...)` payload, but the folder `.api` catalogue for `coloraide` documents the `harmony` call surface without enumerating the registered harmony-name strings. The accepted strings are `mono`, `complement`, `split`, `analogous`, `triad`, `square`, `rectangle`, `wheel` — the row is `rectangle`, NOT `rectangular` (which raises `ValueError: color harmony 'rectangular' cannot be found`), and the singular `complement`/`triad`/`mono` forms not the `-ary`/`-ic` long forms. The `Harmony` row strings stay a marked RESEARCH catalogue-deepen seam until a `coloraide` harmony-name reflection pass lands; the `Palette` `Harmony` arm shape (one named-wheel payload folding `base.harmony` to a coords ramp) is settled. Close-condition: `.api` catalogue carries the registered harmony `name` strings.
</content>
</invoke>
