# [PY_ARTIFACTS_COLOR]

The color-science owner feeding consistent, perceptually-correct color into the visual sub-domains and managing the ICC/LUT egress. `Colorimetry` is ONE owner over a two-engine dispatch — colour-science (NumFOCUS-affiliated, BSD-3, NumPy-backed, pure-Python, host-free) carries CIE color, spectral distributions, 30+ color-space conversions through the universal `colour.convert` gateway, the CIECAM02 / CAM16 appearance models, `delta_E` difference, and chromatic adaptation; ColorAide (pure-Python, zero-native, the `everything.ColorAll` all-plugins engine) carries the genuinely-disjoint legs colour-science lacks — `Color.fit(method=...)` perceptual gamut mapping with the `in_gamut` predicate, `Color.filter(name=...)` CVD simulation scored by `delta_e`, the `Color.steps` `max_delta_e`-spaced perceptually-even palette, and the WCAG21 `Color.contrast` safety scalar. The `MANAGED` ICC/LUT/CCTF arm folds the tone-curve chain on the core and crosses the runtime subprocess seam where pillow `ImageCms` applies the source->destination ICC transform under the selected `RenderingIntent`. `ColorOp` is ONE closed family carrying each operation's typed payload, never an erased `params` bag, dispatched by one total `match` returning `RuntimeRail[ColorReceipt]` — the one typed receipt every arm folds into, never an erased `object`. It hands palettes and color-space conversions to `figures/chart#CHART`, `figures/scene#SCENE`, `figures/table#TABLE`, and the PDF output, so every visual artifact draws color from one owner; the managed arm attaches an ICC profile and rendering intent to the raster egress so output is color-managed rather than naive sRGB. This page closes the `COLOR_MANAGED_VISUAL_PIPELINE`, `ICC_COLOR_MANAGED_OUTPUT`, and `COLORBLIND_SAFE_PALETTE` ideas.

## [01]-[INDEX]

- [01]-[COLOR]: the two-engine `Colorimetry` owner over the closed-payload `ColorOp` family — convert/difference/temperature/palette/gamut/cvd/managed/export folding into one typed `ColorReceipt` whose `tag` carries the same closed `ColorOpTag` literal, never a bare `str`; `colour.convert` is the universal colour-science gateway with `describe_conversion_path(mode="Long")` recovering the resolved model-graph onto the receipt `path` and `RGB_COLOURSPACES` the named-space registry, ColorAide `everything.ColorAll` owns the disjoint gamut/CVD/contrast legs plus the `steps`-spaced, `harmony`-derived, `average`-blended, and `closest`-snapped perceptual-palette modalities, colour-science `write_image`/`write_LUT` the bit-depth-correct image and LUT egress, pillow `ImageCms` the gated ICC raster apply over the `faults`-owned `to_process.run_sync` subprocess seam; `ColorModel` is the dual-name `ModelNames(science, aide, spectral)` vocabulary collapsing the two disjoint engine spellings into one row whose `science` column keys colour-science and whose `aide` column keys ColorAide so one model value serves both engines — never two parallel model enums; `DeltaMethod` keys colour-science `DELTA_E_METHODS`, `AdaptMethod` keys `CHROMATIC_ADAPTATION_TRANSFORMS` (the `convert` `chromatic_adaptation_transform` registry, never the standalone-function `CHROMATIC_ADAPTATION_METHODS`), `CctMethod` keys the four `colour.temperature` illuminant methods, `ToneCurve` keys `CCTF_ENCODINGS`, and `CvdFilter`/`Harmony`/`FitMethod` key the ColorAide filter/harmony/fit plugin maps, all carried as case payload; `RenderingIntent` is the carried ICC intent `IntEnum` whose ordinals mirror `ImageCms.Intent`; `ColorSource = SpectralDistribution | Tristimulus` is the convert input union; `CctSource = Kelvin | Chromaticity` is the bidirectional `Temperature` payload discriminating CCT-to-chromaticity from chromaticity-to-CCT by input shape.

## [02]-[COLOR]

- Owner: `Colorimetry` the one color owner discriminating operation over the closed `ColorOp` family; `ColorOp` an `expression.tagged_union` whose every case carries its own typed payload, never a shared erased `params` dict; `ColorReceipt` the one typed result every arm folds into, its `tag` carrying the same closed `ColorOpTag` literal the `ColorOp` discriminant carries — never a bare `str` the consumer must re-validate — and its `coords`/`path`/`scalar`/`in_gamut`/`raster`/`egress` slots recovering each algorithm choice and every written artifact path; colour-science is the conversion, appearance-model, difference, adaptation, LUT/CCTF, bit-depth image/LUT egress, and `describe_conversion_path` graph-query engine on the cp315 core; ColorAide the pure-Python gamut-mapping, CVD, perceptual-palette, harmony, average-blend, nearest-snap, and contrast engine on the cp315 core; pillow `ImageCms` the ICC raster apply on the gated band; `ColorModel` the dual-name `ModelNames` vocabulary whose `science`/`aide` columns key the two engines from one model value so a `Palette`/`Gamut` arm reaches ColorAide through `space.aide` while a `Convert` arm reaches colour-science through `source.science`, never a per-engine model enum; `RenderingIntent` the `IntEnum` whose ordinal mirrors `ImageCms.Intent` and passes directly to `buildTransform` `renderingIntent` as a raw int, never a parallel intent table; the `everything.ColorAll` all-plugins `Color` binds once at the capsule boundary per the manifest import policy, never re-imported per `derive`.
- Cases: `ColorOp` cases — `Convert(value, source, target, adapt)` (any model-pair through the universal `colour.convert` gateway, the typed `ColorSource = SpectralDistribution | Tristimulus` payload discriminating spectral input from tristimulus arrays, absorbing the former two-call SD->XYZ->display and CIECAM02/CAM16 appearance arms as `ColorModel` source/target values reached through `source.science`/`target.science`, the receipt `path` recovered from `colour.describe_conversion_path(mode="Long")` as the resolved model-graph rather than a hand-built endpoint pair) · `Difference(reference, sample, method)` (`colour.delta_E` over the `DELTA_E_METHODS` registry as a `DeltaMethod` row) · `Temperature(value, method)` (one bidirectional correlated-color-temperature axis whose `Kelvin` payload folds `colour.temperature.CCT_to_xy` to a chromaticity on `coords` and whose `Chromaticity` payload folds the inverse `colour.temperature.xy_to_CCT` to a `Kelvin` on `scalar`, the `CctMethod` illuminant row carried on `path`, the direction discriminated by the `CctSource` input shape so forward and inverse share one case — never a parallel inverse-temperature surface) · `Palette(seed, stop, count, spacing, space, harmony, anchors)` (one perceptual-palette axis whose `harmony is None` arm folds the perceptually-even colorblind-aware ramp via `Color.steps` `max_delta_e`-spaced from the `seed` blend and whose `Harmony` arm folds the named wheel via `Color.harmony`, the multi-color `seed` tuple collapsing to one perceptually-weighted base via `Color.average` so a one-color and a multi-color seed share the arm, the `anchors` brand-color tuple snapping each ramp step to its nearest admissible color via `Color.closest` keyed on `delta_e` so a fixed palette is recoverable, both carrying the endpoint WCAG21 `contrast` as the safety scalar — never a parallel ramp-versus-harmony-versus-snap surface) · `Gamut(value, source, target, method)` (`Color(source.aide, value).convert(target.aide).clone().fit(method=...)` perceptual gamut mapping into a destination space, the pre-fit `in_gamut(target.aide)` predicate carried as evidence, the colour-science `describe_conversion_path(mode="Long")` graph on the receipt `path`) · `Cvd(value, filter, severity)` (`Color("srgb", value).clone().filter(name=...)` deficiency simulation, the source-to-filtered `delta_e` shift carried as the severity scalar) · `Managed(raster, src_profile, dst_profile, intent, curve, luts)` (ICC source/destination-profile transform under a `RenderingIntent`, preceded by the `cctf_encoding`/`read_LUT`/`LUTSequence.apply` tone-curve chain keyed by the typed `ToneCurve` and LUT-path tuple on the core) · `Export(field, path, curve, luts)` (the tone-curve chain folded on the core then bit-depth-correct `colour.write_image` at `uint16` and `colour.write_LUT` LUT serialization beside `read_LUT`, the written artifact paths carried on the receipt `egress`, removing the pillow encode leg from non-ICC image egress) — matched by one total `match`/`case`.
- Entry: `Colorimetry.derive` is `async` over the runtime `async_boundary` and dispatches the `ColorOp` case, returning one `RuntimeRail[ColorReceipt]` whose typed `ColorReceipt` carries the resolved `coords` array, the `describe_conversion_path` model-graph `path`, the difference or contrast `scalar`, the target-space `in_gamut` flag, the managed `raster` bytes, and the written-artifact `egress` paths — never an erased `object`, so a re-admitting consumer recovers every algorithm choice and every emitted path from the receipt; the `Convert`/`Difference`/`Temperature`/`Palette`/`Gamut`/`Cvd`/`Export` arms resolve synchronously on the cp315 core inside the async capsule, while the `Managed` arm's ICC raster apply crosses the runtime `reliability/faults#FAULT` `anyio.to_process.run_sync` subprocess-seam onto the gated-band pillow worker — the one leg forcing the async dispatch shape, the genuine separate-process crossing the gated cp313 band needs because the cp315-core `execution/lanes#LANE` `to_interpreter.run_sync` subinterpreter offload shares the host interpreter version and cannot host the cp313-band `pillow` `ImageCms` package. The `to_process.run_sync` subprocess-seam is the spelling the runtime `reliability/faults#FAULT` owner has already settled as the boundary the `async_boundary` closes over, so it is settled fence code here and folds inline, never a forwarding helper. The result is a settled rail the visual owners consume, never a re-derivation per engine.
- Auto: spectral-source `Convert` folds the `SpectralDistribution` through `colour.convert` with the source `ColorModel.SPECTRAL.science` routing to the target model internally, tristimulus-source `Convert` routes XYZ/Lab/RGB/CAM pairs directly through `source.science`/`target.science`, the `AdaptMethod` (a `CHROMATIC_ADAPTATION_TRANSFORMS` key) selecting `convert`'s `chromatic_adaptation_transform`, the receipt carrying the `describe_conversion_path(mode="Long")` resolved model-graph as `path`; `Difference` folds the two arrays through `delta_E` under the `DeltaMethod` (a `DELTA_E_METHODS` key) registry row into the scalar; `Temperature` discriminates on the `CctSource` shape — a `Kelvin` float folds `colour.temperature.CCT_to_xy(value, method=method.value)` to the chromaticity `coords`, a `Chromaticity` tuple folds `colour.temperature.xy_to_CCT(np.asarray(value), method=method.value)` to the `Kelvin` `scalar` — the `CctMethod` illuminant string on `path`; `Palette` folds the multi-color `seed` tuple through `Color.average(seeds, space=space.aide, out_space='srgb')` to one perceptually-weighted base, then discriminates on the `Harmony | None` payload — the `None` arm folds the base and `stop` endpoint through `Color.steps([base, stop], steps=count, max_steps=count, max_delta_e=spacing, delta_e="2000", space=space.aide, out_space='srgb')` for perceptually-even spacing, the `Harmony` arm folds the base through `base.harmony(name, space=space.aide, out_space='srgb')` for the named wheel — then, when `anchors` is non-empty, snaps each ramp step through `step.closest(anchors, method="2000")` so every emitted color is the nearest admissible brand color, all arms snapshotting the endpoint `contrast` (WCAG21) as the colorblind-safety scalar; `Gamut` folds the source through `Color(source.aide, value).convert(target.aide)` then a `clone().fit(method=...)`, carrying the pre-fit `in_gamut(target.aide)` predicate as evidence so a no-op fit is recoverable and the colour-science `describe_conversion_path` graph as `path`; `Cvd` folds through `Color('srgb', value).clone().filter(name, amount=severity)` and carries the source-to-filtered `delta_e` shift as the deficiency-severity scalar; `Export` folds the `cctf_encoding`->`LUTSequence.apply` tone chain inline then writes the field through `colour.write_image(..., bit_depth="uint16")` and each LUT through `colour.write_LUT`, carrying the `RGB_COLOURSPACES["sRGB"].name`/`ToneCurve` provenance on `path` and the written paths on `egress`; the `Managed` arm folds the same inline `cctf_encoding`->`LUTSequence.apply` tone-curve chain keyed by the typed `ToneCurve` and LUT-path tuple on the core, then crosses the `to_process.run_sync` subprocess seam where pillow `ImageCms` applies the source->destination ICC transform under the selected `RenderingIntent`.
- Packages: `colour-science` (`convert` the universal gateway subsuming `sd_to_XYZ`/`XYZ_to_sRGB`/`XYZ_to_RGB`/`RGB_to_XYZ`, `describe_conversion_path(mode="Long", print_callable=...)` the model-graph query feeding the receipt `path`, `RGB_COLOURSPACES` the named-space registry, `delta_E` over the `DELTA_E_METHODS` registry feeding `DeltaMethod`, `CHROMATIC_ADAPTATION_TRANSFORMS` feeding `AdaptMethod`, `colour.temperature.CCT_to_xy`/`xy_to_CCT` over the four-illuminant method axis feeding `CctMethod`, `CCTF_ENCODINGS` feeding `ToneCurve`, `read_LUT`/`write_LUT`/`LUTSequence`/`cctf_encoding`, `write_image` bit-depth-correct egress, `SpectralDistribution`, `domain_range_scale`), `coloraide` (`everything.ColorAll` as the all-plugins `Color`; `Color.steps` with `max_delta_e`/`delta_e` perceptual spacing, `Color.harmony` named wheels, `Color.average` perceptual seed-blend, `Color.closest` nearest-by-`delta_e` snap, `Color.contrast` WCAG21, `Color.convert`/`Color.clone`/`Color.fit`/`Color.in_gamut`, `Color.filter`/`Color.delta_e`, `Color.coords`), `numpy` (the array backing) on the cp315 core; `pillow` `ImageCms` (`ImageCmsProfile`/`buildTransform`/`applyTransform`/`Intent` enum) gated `python_version<'3.15'`; runtime (`faults.RuntimeRail`/`async_boundary` and the `faults`-owned `anyio.to_process.run_sync` subprocess-seam the gated ICC arm crosses — the genuine separate-process crossing distinct from the cp315-core `execution/lanes#LANE` `to_interpreter.run_sync` subinterpreter offload, both settled at their owners).
- Growth: a new color operation is one `ColorOp` case carrying its payload plus one acceptor arm folding into `ColorReceipt`, its `tag` extending the one `ColorOpTag` literal the receipt shares; a new difference metric is one `DeltaMethod` row; a new gamut-fit method is one `FitMethod` row; a new CVD type is one `CvdFilter` row; a new harmony wheel is one `Harmony` row; a new appearance or named model is one `ColorModel` row carrying both its `science` and `aide` (or `None` where one engine lacks it) name columns; a new CCT illuminant method is one `CctMethod` row; a new adaptation transform is one `AdaptMethod` row; a new tone curve is one `ToneCurve` row; a new rendering intent is one `RenderingIntent` row mirroring its `ImageCms.Intent` ordinal; a brand-palette constraint is the `anchors` tuple on the existing `Palette` payload, a blended seed the multi-color `seed` tuple — never a parallel ramp/harmony/snap/blend surface; zero new surface.
- Boundary: no chart/scene rendering (that stays at the visual owners); the color arms emit one `ColorReceipt` carrying arrays, scalars, chromaticities, palettes, and written-artifact paths, consumed inward by the visual and document sub-domains; the gamut-mapping, CVD, perceptual-palette, harmony, average-blend, nearest-snap, and contrast legs are ColorAide's exclusively — the prior hand-rolled Oklab lerp and any bespoke Daltonization transform are the deleted forms, and colour-science keeps spectral/CAM/difference/adaptation/LUT/image-egress/graph-query, never a second convert, delta-E, gamut-fit, or contrast engine; bit-depth-correct image and LUT egress folds through `colour.write_image`/`colour.write_LUT` so the pillow encode leg survives only inside the gated ICC `Managed` arm, never as a second non-ICC image writer; `everything.ColorAll` is the single all-plugins `Color` so OKLCH/HCT and the registered fit/filter/harmony/average/closest plugins resolve without a per-space class; the `MANAGED` `cctf_encoding`/`LUTSequence` math resolves pure-Python on the core, but the pillow `ImageCms` raster apply rides the gated `python_version<'3.15'` band and never resolves on the cp315-core process, so it dispatches onto the `faults`-owned `to_process.run_sync` cp313-band subprocess seam — a different interpreter version than the cp315-core `to_interpreter.run_sync` subinterpreter offload can host, the one leg forcing the async owner. The `ImageCms.Intent` enum, the `buildTransform`/`applyTransform` apply surface, the `colour.temperature.CCT_to_xy`/`xy_to_CCT` spellings, the `describe_conversion_path` `mode`/`print_callable` capture surface, and the ColorAide harmony-name strings are reflection-verified against the installed packages but stay [03]-[RESEARCH] catalogue-deepen items until the folder `.api` catalogues carry them; the `to_process.run_sync` subprocess-seam is settled by the runtime `reliability/faults#FAULT` owner, and every other colour-science and ColorAide spelling below is settled fence code.

```python signature
from enum import Enum, IntEnum, StrEnum
from typing import Literal, NamedTuple, assert_never

import colour
import numpy as np
from anyio import to_process
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
type ColorOpTag = Literal["convert", "difference", "temperature", "palette", "gamut", "cvd", "managed", "export"]


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


class ToneCurve(StrEnum):
    SRGB = "sRGB"
    GAMMA_2_2 = "Gamma 2.2"
    ST2084 = "ST 2084"
    BT1886 = "ITU-R BT.1886"
    PROPHOTO = "ProPhoto RGB"


class RenderingIntent(IntEnum):
    PERCEPTUAL = 0
    RELATIVE_COLORIMETRIC = 1
    SATURATION = 2
    ABSOLUTE_COLORIMETRIC = 3


@tagged_union(frozen=True)
class ColorOp:
    tag: ColorOpTag = tag()
    convert: tuple[ColorSource, ColorModel, ColorModel, AdaptMethod] = case()
    difference: tuple[Tristimulus, Tristimulus, DeltaMethod] = case()
    temperature: tuple[CctSource, CctMethod] = case()
    palette: tuple[tuple[str, ...], str, int, float, ColorModel, Harmony | None, tuple[str, ...]] = case()
    gamut: tuple[Tristimulus, ColorModel, ColorModel, FitMethod] = case()
    cvd: tuple[Tristimulus, CvdFilter, float] = case()
    managed: tuple[NDArray[np.uint8], bytes, bytes, RenderingIntent, ToneCurve, tuple[str, ...]] = case()
    export: tuple[NDArray[np.float64], str, ToneCurve, tuple[str, ...]] = case()

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

    @staticmethod
    def Managed(raster: NDArray[np.uint8], src_profile: bytes, dst_profile: bytes, intent: RenderingIntent, curve: ToneCurve = ToneCurve.SRGB, luts: tuple[str, ...] = ()) -> "ColorOp":
        return ColorOp(managed=(raster, src_profile, dst_profile, intent, curve, luts))

    @staticmethod
    def Export(field: NDArray[np.float64], path: str, curve: ToneCurve = ToneCurve.SRGB, luts: tuple[str, ...] = ()) -> "ColorOp":
        return ColorOp(export=(field, path, curve, luts))


class ColorReceipt(Struct, frozen=True):
    tag: ColorOpTag
    coords: Tristimulus
    path: tuple[str, ...] = ()
    scalar: float = 0.0
    in_gamut: bool = True
    raster: bytes = b""
    egress: tuple[str, ...] = ()


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
            case ColorOp(tag="managed", managed=(raster, src_profile, dst_profile, intent, curve, luts)):
                toned = self._tone(raster / np.float64(255.0), curve, luts)
                return ColorReceipt("managed", toned, raster=await to_process.run_sync(_icc_apply, toned, src_profile, dst_profile, int(intent)))
            case ColorOp(tag="export", export=(field, path, curve, luts)):
                toned = self._tone(field, curve, luts)
                colour.write_image(toned, path, bit_depth="uint16")
                egress = tuple(
                    sink for index, lut in enumerate(luts)
                    if (sink := f"{path}.{index}.cube") and colour.write_LUT(colour.read_LUT(lut), sink) is None
                )
                return ColorReceipt("export", toned, path=(colour.RGB_COLOURSPACES["sRGB"].name, curve.value), egress=(path, *egress))
            case _:
                assert_never(self.op)

    @staticmethod
    def _path(source: ColorModel, target: ColorModel) -> tuple[str, ...]:
        captured: list[str] = []
        colour.describe_conversion_path(source.science, target.science, mode="Long", print_callable=captured.append)
        return (source.science, *captured, target.science)

    @staticmethod
    def _tone(field: Tristimulus, curve: ToneCurve, luts: tuple[str, ...]) -> Tristimulus:
        encoded = colour.cctf_encoding(field, function=curve.value)
        return colour.LUTSequence(*(colour.read_LUT(lut) for lut in luts)).apply(encoded) if luts else encoded


def _icc_apply(raster: Tristimulus, src_profile: bytes, dst_profile: bytes, intent: int) -> bytes:
    from io import BytesIO

    from PIL import Image, ImageCms

    source = ImageCms.ImageCmsProfile(BytesIO(src_profile))
    target = ImageCms.ImageCmsProfile(BytesIO(dst_profile))
    transform = ImageCms.buildTransform(source, target, "RGB", "RGB", renderingIntent=intent)
    managed = ImageCms.applyTransform(Image.fromarray((raster * 255.0).astype(np.uint8)), transform)
    sink = BytesIO()
    managed.save(sink, format="PNG")
    return sink.getvalue()
```

## [03]-[RESEARCH]

- [TEMPERATURE] [RESEARCH]: the colour-science `colour.temperature.CCT_to_xy(CCT, method=...)` and inverse `colour.temperature.xy_to_CCT(xy, method=...)` correlated-color-temperature spellings are NOT yet reflected in the folder `.api` catalogue for `colour-science`; the [03]-[ENTRYPOINTS] tables catalogue `convert`, `chromatic_adaptation`, `XYZ_to_sRGB`, `sRGB_to_XYZ`, `XYZ_to_RGB`, `RGB_to_XYZ`, `sd_to_XYZ`, `cctf_encoding`, `cctf_decoding`, `delta_E`, `read_LUT`, `write_LUT`, `read_image`, `write_image`, `XYZ_to_Lab`, `Lab_to_XYZ`, `XYZ_to_CIECAM02`, `XYZ_to_Oklab`, `Oklab_to_XYZ`, `domain_range_scale`, and `describe_conversion_path`, but no `temperature` sub-namespace row. Both directions carry a `method` axis over `{"CIE Illuminant D Series", "Kang 2002", "Hernandez 1999", "McCamy 1992"}` — the `CctMethod` vocabulary — and the top-level `colour.CCT_to_xy` re-exports the daylight-default forward; the bidirectional `ColorOp.Temperature` case (the `CctSource = Kelvin | Chromaticity` payload discriminating direction by input shape, the `CctMethod` carried on `path`) stays a marked RESEARCH seam until a `colour.temperature` `.api` reflection pass lands. Close-condition: `.api` catalogue carries `colour.temperature.CCT_to_xy`/`xy_to_CCT` with the four-method literal.
- [ICC_TRANSFORM] [RESEARCH]: the pillow `ImageCms` catalogue lists `ImageCmsProfile` ONLY — `buildTransform`, `applyTransform`, and the rendering-intent surface are NOT yet reflected in the folder `.api` catalogue for `pillow`. The `_icc_apply` `buildTransform(source, target, "RGB", "RGB", renderingIntent=...)`/`applyTransform(image, transform)` body and the raw-int intent payload stay a marked RESEARCH seam until a pillow `ImageCms` `.api` reflection pass lands on the gated `python_version<'3.15'` band. The pillow-12 intent surface is the `ImageCms.Intent` IntEnum (`PERCEPTUAL=0`, `RELATIVE_COLORIMETRIC=1`, `SATURATION=2`, `ABSOLUTE_COLORIMETRIC=3`), NOT the legacy module-level `INTENT_*` constants — those are removed in pillow 12; the `RenderingIntent` `IntEnum` mirrors the `ImageCms.Intent` ordinals exactly and `buildTransform` accepts the raw int directly, so `int(intent)` is the settled spelling. Close-condition: `.api` catalogue carries `buildTransform`/`applyTransform`/`ImageCms.Intent`. The ICC raster apply is the one gated leg crossing the cp313-band process seam.
- [ICC_SEAM]: the `Managed` arm's process dispatch is spelled `anyio.to_process.run_sync(_icc_apply, ...)`, the subprocess-seam the runtime `reliability/faults#FAULT` owner has already settled (`anyio.to_process.run_sync` named as the subprocess-seam the `async_boundary` closes over). A subinterpreter shares the host interpreter version and cannot host the cp313-band `pillow` `ImageCms` package, so the `Managed` arm crosses this genuine separate-process seam rather than the cp315-core `execution/lanes#LANE` `to_interpreter.run_sync` subinterpreter offload. The `_icc_apply` body is a module-level function dispatched by qualified name across the process seam (`to_process.run_sync` cannot target a bound method or closure), so it stays out of the `Colorimetry` owner deliberately, not as a stray helper. The seam is settled fence code by inheritance from its owner; the only open item is a branch-catalogue gap — the branch `anyio` `.api` catalogue reflects `open_process`/`run_process`/`to_thread.run_sync`/`to_interpreter.run_sync` but no `to_process` row, so `assay api` reflection over `anyio.to_process` deepens the branch catalogue to match the settled owner spelling, never re-opening this fence.
- [COLOUR_SCIENCE]: the colour-science universal `convert` gateway, `describe_conversion_path` model-graph query, the `RGB_COLOURSPACES` named-space registry, `delta_E` over the `DELTA_E_METHODS` registry, the `CCTF_ENCODINGS` registry feeding `ToneCurve`, `read_LUT`/`write_LUT`, `LUTSequence`/`LUTSequence.apply`, `cctf_encoding`, `write_image`, `SpectralDistribution`, and `domain_range_scale` spellings verify against the folder `.api` catalogue for `colour-science` (`0.4.7` reflected, cp315-core pure-Python). `AdaptMethod` keys `CHROMATIC_ADAPTATION_TRANSFORMS` (the matrix registry `convert`'s `chromatic_adaptation_transform` kwarg resolves — `Bradford`, `CAT02`, `CAT16`, `Von Kries`, `CMCCAT2000`, `Sharp`, `XYZ Scaling`), NOT the catalogue-listed `CHROMATIC_ADAPTATION_METHODS` which keys the standalone `chromatic_adaptation()` function (`Von Kries`, `Zhai 2018`, `Li 2025`, ...) — the two registries share names like `Von Kries` but `Bradford`/`CAT02`/`CAT16`/`Sharp` exist only in the transforms registry and `Zhai 2018`/`Li 2025` only in the methods registry, so binding `AdaptMethod` to the methods registry breaks the default `Bradford` convert path; the `CHROMATIC_ADAPTATION_TRANSFORMS` row is a [03]-[RESEARCH] catalogue-deepen item since the `.api` lists only `CHROMATIC_ADAPTATION_METHODS`. The `Convert` arm routes spectral input by the `ColorSource` payload type and CIECAM02/CAM16 through the universal `convert` gateway keyed by the target `ColorModel.science`, the `AdaptMethod` selecting `chromatic_adaptation_transform`, the receipt `path` recovered from `describe_conversion_path(mode="Long")`; the universal `convert` subsumes the per-pair `sd_to_XYZ`/`XYZ_to_sRGB`/`XYZ_to_RGB`/`RGB_to_XYZ` transforms, so naming them as separate engine members is the deleted sprawl; the `Difference` arm folds `delta_E` over the `DELTA_E_METHODS` registry; the `Export` arm folds the `cctf_encoding`->`LUTSequence.apply` tone chain then `write_image(..., bit_depth="uint16")` plus `write_LUT`, carrying `RGB_COLOURSPACES["sRGB"].name` provenance; the `MANAGED` LUT/CCTF tone-curve chain folds `cctf_encoding`->`read_LUT`->`LUTSequence.apply` keyed by the typed `ToneCurve` on the core. Every spelling other than the `CHROMATIC_ADAPTATION_TRANSFORMS` and `describe_conversion_path` `mode`/`print_callable` catalogue-deepen seams is settled colour-science fence code.
- [CONVERSION_PATH] [RESEARCH]: the `Colorimetry._path` graph capture spells `describe_conversion_path(source, target, mode="Long", print_callable=captured.append)`, but the folder `.api` catalogue verifies the two-argument `describe_conversion_path(source, target)` call without the `mode`/`print_callable` capture kwargs or the per-line callable contract. The full signature is `describe_conversion_path(source, target, mode='Short', width=79, padding=3, print_callable=print, **kwargs) -> None` — `mode='Long'` is load-bearing: the default `'Short'` prints a seven-line truncated summary while `'Long'` yields the nineteen-line resolved model-graph the receipt `path` claims, so `mode='Long'` is settled here, not optional. The `mode`/`print_callable`-fed capture and the `(source, *captured, target)` path tuple stay a marked RESEARCH catalogue-deepen seam until a `describe_conversion_path` signature reflection lands; the `Convert`/`Gamut` receipt-`path` shape is settled. Close-condition: `.api` catalogue carries `describe_conversion_path` with `mode`/`print_callable`.
- [COLORAIDE]: the ColorAide `everything.ColorAll` (the all-plugins `Color`), `Color.convert(space)`, `Color.clone()`, `Color.fit(method=...)`, `Color.in_gamut(space)`, `Color.filter(name, amount=...)`, `Color.delta_e(color, method=...)`, `Color.contrast(color)`, `Color.steps(colors, steps=, max_steps=, max_delta_e=, delta_e=, space=, out_space=)`, `Color.harmony(name, space=, out_space=)`, `Color.average(colors, space=, out_space=)`, `Color.closest(colors, method=...)`, and `Color.coords()` spellings verify against the folder `.api` catalogue for `coloraide` (`8.8.1` reflected, cp315 pure-Python). ColorAide spaces are lowercase-hyphenated registered names (`srgb`/`oklch`/`oklab`/`lab-d65`/`lch-d65`/`xyz-d65`/`xyy`/`hsv`), disjoint from the colour-science model strings, so every ColorAide call reaches the space through `ColorModel.aide` and every colour-science call through `ColorModel.science` — the single `ModelNames(science, aide)` row carrying both spellings, a colour-science name passed to a ColorAide call raising `KeyError`/`ValueError` is the deleted defect. The `Gamut` arm folds `Color(source.aide, coords).convert(target.aide)` then `clone().fit(method=FitMethod)` carrying `in_gamut(target.aide)`, the `Cvd` arm folds `Color("srgb", coords).clone().filter(CvdFilter, amount=severity)` carrying `delta_e(method="2000")`, and the `Palette` arm folds the multi-color `seed` through `Color.average(seeds, space=space.aide, out_space='srgb')` to one base then discriminates the `Harmony | None` payload — the `None` arm folds `Color.steps([base, stop], steps=count, max_delta_e=spacing, delta_e="2000", space=space.aide, out_space='srgb')` and the `Harmony` arm folds `base.harmony(name, space=space.aide, out_space='srgb')` — then snaps each step through `step.closest(anchors, method="2000")` when `anchors` is non-empty, reading each result `coords()` and the endpoint `contrast()`. The six `FitMethod` rows (`raytrace`/`oklch-chroma`/`lch-chroma`/`minde-chroma`/`scale`/`scale-luminance`), the three `CvdFilter` rows (`protan`/`deutan`/`tritan`), and the eight `delta_e` metrics (`76`/`2000`/`94`/`cmc`/`hyab`/`itp`/`jz`/`ok`) are the registered method/filter/metric names, never parallel mapping functions or a hand-rolled Daltonization transform. The `Color.harmony` call surface and its `space`/`out_space` kwargs are settled, but the `.api` catalogue documents the call without enumerating the registered harmony names; every other spelling is settled ColorAide fence code; `everything.ColorAll` imports as `Color` at boundary scope per the manifest import policy.
- [HARMONY] [RESEARCH]: the `Harmony` `StrEnum` carries the ColorAide harmony-wheel names (`mono`/`complement`/`split`/`analogous`/`triad`/`square`/`rectangle`/`wheel`) as the `Color.harmony(name, ...)` payload, but the folder `.api` catalogue for `coloraide` documents the `harmony` call surface without enumerating the registered harmony-name strings. The accepted strings are `mono`, `complement`, `split`, `analogous`, `triad`, `square`, `rectangle`, `wheel` — the row is `rectangle`, NOT `rectangular` (which raises `ValueError: color harmony 'rectangular' cannot be found`), and the singular `complement`/`triad`/`mono` forms not the `-ary`/`-ic` long forms. The `Harmony` row strings stay a marked RESEARCH catalogue-deepen seam until a `coloraide` harmony-name reflection pass lands; the `Palette` `Harmony` arm shape (one named-wheel payload folding `base.harmony` to a coords ramp) is settled. Close-condition: `.api` catalogue carries the registered harmony `name` strings.
