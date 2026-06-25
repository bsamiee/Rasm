# [PY_ARTIFACTS_GRAPHIC_COLOR_MANAGED]

The ICC/LUT/CCTF color-managed raster-egress owner — the one async-forcing gated leg of the color sub-domain. `ColorManaged` is ONE owner folding the colour-science tone-curve chain on the cp315 core then, for the ICC arm, crossing the runtime subprocess seam where pillow `ImageCms` applies the source->destination ICC transform under the selected `RenderingIntent`. colour-science (NumFOCUS-affiliated, BSD-3, NumPy-backed, pure-Python) carries the `cctf_encoding`->`read_LUT`->`LUTSequence.apply` tone-curve chain and the bit-depth-correct `write_image`/`write_LUT` egress entirely on the core; pillow `ImageCms` (`ImageCmsProfile`/`buildTransform`/`applyTransform`, gated `python_version<'3.15'`) carries the genuinely-native ICC apply that cannot resolve in the cp315-core process, so the `Managed` arm dispatches it onto the `runtime/reliability/faults#FAULT`-owned `anyio.to_process.run_sync` subprocess seam through the module-level `_icc_apply` function. `ManageOp` is ONE closed family carrying each operation's typed payload, never an erased `params` bag, dispatched by one total `match` returning `RuntimeRail[ArtifactReceipt]` — keying the color-managed bytes through `ContentIdentity.of` and contributing the settled `core/receipt#RECEIPT` `ArtifactReceipt.Preview` case, the one receipt family every artifacts producer contributes to, never a parallel managed-receipt type. It consumes the tone-curve and color-space provenance `graphic/color/derive#DERIVE` resolves and hands the color-managed raster to `graphic/raster/io#RASTER` and `document/emit#DOCUMENT`, so figure and PDF output is color-managed rather than naive sRGB. This page closes the `ICC_COLOR_MANAGED_OUTPUT` idea — the color-managed egress side of the visual pipeline.

## [01]-[INDEX]

- [01]-[MANAGED]: the one `ColorManaged` owner over the closed-payload `ManageOp` family — managed/export keying the color-managed bytes through `ContentIdentity.of` and contributing the settled `core/receipt#RECEIPT` `ArtifactReceipt.Preview` case, the `ManageOp` discriminant the closed `ManageOpTag` literal, never a bare `str`; the colour-science `cctf_encoding`->`read_LUT`->`LUTSequence.apply` tone-curve chain keyed by the typed `ToneCurve` resolves bit-depth-correctly on the core, `colour.write_image` at `uint16` and `colour.write_LUT` LUT serialization beside `read_LUT` carry the bit-depth-correct image and LUT egress, pillow `ImageCms` the gated ICC raster apply over the `runtime/reliability/faults#FAULT`-owned `to_process.run_sync` subprocess seam; `RenderingIntent` is the carried ICC intent `IntEnum` whose ordinals mirror `ImageCms.Intent` exactly and pass directly to `buildTransform` `renderingIntent` as a raw int, never a parallel intent table; `ToneCurve` keys `CCTF_ENCODINGS`; the `Managed` arm is the one async-forcing leg crossing the gated `python_version<'3.15'` cp313-band process seam, the `Export` arm resolves fully on the core, so the async dispatch shape is forced by the single ICC apply.

## [02]-[MANAGED]

- Owner: `ColorManaged` the one ICC/LUT/CCTF color-managed raster-egress owner discriminating operation over the closed `ManageOp` family; `ManageOp` an `expression.tagged_union` whose every case carries its own typed payload, never a shared erased `params` dict; the owner contributes the settled `core/receipt#RECEIPT` `ArtifactReceipt.Preview` case keyed by `ContentIdentity.of` over the managed bytes (the `Managed` arm) or the written field (the `Export` arm), so the managed egress is a producer of the one `ArtifactReceipt` family every artifacts owner contributes to, never a parallel managed-receipt type the receipt owner would import; colour-science is the `cctf_encoding`/`read_LUT`/`LUTSequence`/`write_image`/`write_LUT` tone-and-egress engine on the cp315 core; pillow `ImageCms` the ICC raster apply on the gated band; `RenderingIntent` the `IntEnum` whose ordinal mirrors `ImageCms.Intent` and passes directly to `buildTransform` `renderingIntent` as a raw int, never a parallel intent table. The tone-curve and color-space provenance this owner resolves is the `graphic/color/derive#DERIVE` `ColorReceipt.path` provenance the derivation owner produces — this owner consumes it, it derives no color; the `ToneCurve` keys the same `CCTF_ENCODINGS` registry the derivation owner's spectral chain reads, threaded here as the tone-curve payload, never a second tone-curve vocabulary.
- Cases: `ManageOp` cases — `Managed(raster, src_profile, dst_profile, intent, curve, luts)` (ICC source/destination-profile transform under a `RenderingIntent`, preceded by the `cctf_encoding`/`read_LUT`/`LUTSequence.apply` tone-curve chain keyed by the typed `ToneCurve` and LUT-path tuple on the core, then the pillow `ImageCms` apply on the gated subprocess seam — the one async-forcing leg, the managed bytes keyed through `ContentIdentity.of` into the `ArtifactReceipt.Preview` content key) · `Export(field, path, curve, luts)` (the tone-curve chain folded on the core then bit-depth-correct `colour.write_image` at `uint16` and `colour.write_LUT` LUT serialization beside `read_LUT`, the written field keyed through `ContentIdentity.of` into the `ArtifactReceipt.Preview` content key, resolving fully on the core with no process seam, removing the pillow encode leg from non-ICC image egress) — matched by one total `match`/`case`. The `_tone` `cctf_encoding`->`LUTSequence.apply` chain is the shared core fold both arms reach, never duplicated per arm.
- Entry: `ColorManaged.apply` is `async` over the runtime `async_boundary` and dispatches the `ManageOp` case, returning one `RuntimeRail[ArtifactReceipt]` and contributing the settled `core/receipt#RECEIPT` `ArtifactReceipt.Preview(key, width, height)` case — the key the `ContentIdentity.of` digest over the managed bytes or written field, the width/height the managed-raster dimensions — all flat scalars the receipt owner reads through its own `_facts` arm with no producer value object imported, mirroring the settled `graphic/raster/io#RASTER` `Raster.of -> RuntimeRail[ArtifactReceipt]` producer shape so the managed egress lands in the one receipt family beside every other producer; the `Export` arm resolves synchronously on the cp315 core inside the async capsule, while the `Managed` arm's ICC raster apply crosses the runtime `reliability/faults#FAULT` `anyio.to_process.run_sync` subprocess-seam onto the gated-band pillow worker — the one leg forcing the async dispatch shape, the genuine separate-process crossing the gated cp313 band needs because the cp315-core `execution/lanes#LANE` `to_interpreter.run_sync` subinterpreter offload shares the host interpreter version and cannot host the cp313-band `pillow` `ImageCms` package. The `to_process.run_sync` subprocess-seam is the spelling the runtime `reliability/faults#FAULT` owner has already settled as the boundary the `async_boundary` closes over, so it is settled fence code here and folds inline, never a forwarding helper. The result is a settled rail the figure and document owners consume, never a re-derivation per output.
- Auto: both arms first fold the inline `cctf_encoding`->`LUTSequence.apply` tone-curve chain through `_tone` keyed by the typed `ToneCurve` and LUT-path tuple on the core — `colour.cctf_encoding(field, function=curve.value)` applies the transfer function and `colour.LUTSequence(*(colour.read_LUT(lut) for lut in luts)).apply(encoded)` chains the LUT pipeline when `luts` is non-empty; `Managed` then normalizes the `uint8` raster to the unit range, folds it through `_tone`, crosses the `to_process.run_sync` subprocess seam where `_icc_apply` builds the `ImageCms.buildTransform(source, target, "RGB", "RGB", renderingIntent=int(intent))` transform and applies it through `ImageCms.applyTransform`, then keys the returned managed PNG bytes through `ContentIdentity.of("color-managed", managed)` and projects `ArtifactReceipt.Preview` with the raster dimensions; `Export` folds the field through `_tone`, writes it through `colour.write_image(toned, path, bit_depth="uint16")` and each LUT through `colour.write_LUT(colour.read_LUT(lut), sink)`, then keys the toned field bytes through `ContentIdentity.of` and projects `ArtifactReceipt.Preview` with the field dimensions. The pre-fit tone chain runs once on the core for both arms; only the ICC apply rides the gated band.
- Packages: `colour-science` (`cctf_encoding` the OETF/CCTF encode over `CCTF_ENCODINGS` feeding `ToneCurve`, `read_LUT`/`write_LUT` the `.cube`/`.clf`/`.csp` LUT IO, `LUTSequence`/`LUTSequence.apply` the ordered LUT pipeline, `write_image` the bit-depth-correct `uint16` image egress, `RGB_COLOURSPACES` the named-space registry for the `sRGB` provenance) on the cp315 core; `numpy` (the array backing for the `uint8`->unit-range normalization and the `float64` toned field); `pillow` `ImageCms` (`ImageCmsProfile`/`buildTransform`/`applyTransform`/`Intent` enum) gated `python_version<'3.15'`; runtime (`faults.RuntimeRail`/`async_boundary` and the `faults`-owned `anyio.to_process.run_sync` subprocess-seam the gated ICC arm crosses — the genuine separate-process crossing distinct from the cp315-core `execution/lanes#LANE` `to_interpreter.run_sync` subinterpreter offload, both settled at their owners), `graphic/color/derive#DERIVE` (the `ColorReceipt.path` tone-curve/space provenance the managed `path` consumes), `core/receipt#RECEIPT` (`ArtifactReceipt.Preview` carrying the managed-raster content key plus the egress facts).
- Growth: a new managed operation is one `ManageOp` case carrying its payload plus one acceptor arm contributing `ArtifactReceipt.Preview`, its `tag` extending the one `ManageOpTag` literal; a new tone curve is one `ToneCurve` row keying `CCTF_ENCODINGS`; a new rendering intent is one `RenderingIntent` row mirroring its `ImageCms.Intent` ordinal; a richer ICC management leg (perceptual-rendering-intent gamut audit, profile-completeness probe, BPC-flag policy) lands as one `ManageOp` case or one carried `ImageCms.Flags` payload on the existing `Managed` arm — never a parallel managed surface, never a second image writer; zero new surface.
- Boundary: no color derivation (that stays at `graphic/color/derive#DERIVE` — this owner resolves no convert/difference/temperature/palette/gamut/cvd and imports neither the `ColorOp` family nor ColorAide); no chart/scene rendering (that stays at the visual owners); the managed arms key the color-managed raster or written field through `ContentIdentity.of` and contribute one `ArtifactReceipt.Preview`, consumed inward by `graphic/raster/io#RASTER` and `document/emit#DOCUMENT` — a parallel managed-receipt type beside the one `ArtifactReceipt` family is the deleted form; bit-depth-correct image and LUT egress folds through `colour.write_image`/`colour.write_LUT` so the pillow encode leg survives only inside the gated ICC `Managed` arm, never as a second non-ICC image writer; the `cctf_encoding`/`LUTSequence` math resolves pure-Python on the core, but the pillow `ImageCms` raster apply rides the gated `python_version<'3.15'` band and never resolves on the cp315-core process, so it dispatches onto the `faults`-owned `to_process.run_sync` cp313-band subprocess seam — a different interpreter version than the cp315-core `to_interpreter.run_sync` subinterpreter offload can host, the one leg forcing the async owner. The `colour.temperature`/CAM/convert/delta-E derivation surface and the ColorAide gamut/CVD/palette engine are NOT this page's — they are `graphic/color/derive#DERIVE`'s; this owner cites colour-science only for the tone/LUT/image-egress chain. Every colour-science spelling below is settled fence code; the pillow `ImageCms` `buildTransform`/`applyTransform` apply surface, the `ImageCms.Intent` ordinal mirror, and the `to_process.run_sync` seam are settled by inheritance from their owners with one catalogue-deepen gap tracked in [03]-[RESEARCH].

```python signature
from enum import IntEnum, StrEnum
from typing import Literal, assert_never

import colour
import numpy as np
from anyio import to_process
from expression import case, tag, tagged_union
from msgspec import Struct
from numpy.typing import NDArray

from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.receipt.receipt import ArtifactReceipt

type Tristimulus = NDArray[np.float64]
type ManageOpTag = Literal["managed", "export"]


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
class ManageOp:
    tag: ManageOpTag = tag()
    managed: tuple[NDArray[np.uint8], bytes, bytes, RenderingIntent, ToneCurve, tuple[str, ...]] = case()
    export: tuple[NDArray[np.float64], str, ToneCurve, tuple[str, ...]] = case()

    @staticmethod
    def Managed(raster: NDArray[np.uint8], src_profile: bytes, dst_profile: bytes, intent: RenderingIntent, curve: ToneCurve = ToneCurve.SRGB, luts: tuple[str, ...] = ()) -> "ManageOp":
        return ManageOp(managed=(raster, src_profile, dst_profile, intent, curve, luts))

    @staticmethod
    def Export(field: NDArray[np.float64], path: str, curve: ToneCurve = ToneCurve.SRGB, luts: tuple[str, ...] = ()) -> "ManageOp":
        return ManageOp(export=(field, path, curve, luts))


class ColorManaged(Struct, frozen=True):
    op: ManageOp

    async def apply(self) -> RuntimeRail[ArtifactReceipt]:
        return await async_boundary(f"color.managed.{self.op.tag}", self._compute)

    async def _compute(self) -> ArtifactReceipt:
        match self.op:
            case ManageOp(tag="managed", managed=(raster, src_profile, dst_profile, intent, curve, luts)):
                toned = self._tone(raster / np.float64(255.0), curve, luts)
                managed = await to_process.run_sync(_icc_apply, toned, src_profile, dst_profile, int(intent))
                height, width = raster.shape[0], raster.shape[1]
                return ArtifactReceipt.Preview(ContentIdentity.of("color-managed", managed), width, height)
            case ManageOp(tag="export", export=(field, path, curve, luts)):
                toned = self._tone(field, curve, luts)
                colour.write_image(toned, path, bit_depth="uint16")
                for lut in luts:
                    colour.write_LUT(colour.read_LUT(lut), f"{path}.cube")
                height, width = toned.shape[0], toned.shape[1]
                return ArtifactReceipt.Preview(ContentIdentity.of("color-export", toned.tobytes()), width, height)
            case _:
                assert_never(self.op)

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

- [ICC_TRANSFORM] [RESEARCH]: the pillow `ImageCms` `buildTransform`/`applyTransform`/`profileToProfile` ICC-managed conversion row and the `ImageCms.Intent` intent enum reflect in the folder `.api` catalogue for `pillow` (`12.2.0`, surface read from the lock-built module on the gated cp313 band): the `[10]` `buildTransform`/`applyTransform`/`profileToProfile` `(in_profile, out_profile, in_mode, out_mode, intent)` row and the `[06]` `ImageCms.Intent`/`Flags` intent/BPC-flag enum row, plus `ImageCmsProfile` `[09]`. The `_icc_apply` `buildTransform(source, target, "RGB", "RGB", renderingIntent=...)`/`applyTransform(image, transform)` body is therefore settled fence code on the gated band. The pillow-12 intent surface is the `ImageCms.Intent` IntEnum (`PERCEPTUAL=0`, `RELATIVE_COLORIMETRIC=1`, `SATURATION=2`, `ABSOLUTE_COLORIMETRIC=3`), NOT the legacy module-level `INTENT_*` constants — those are removed in pillow 12; the `RenderingIntent` `IntEnum` mirrors the `ImageCms.Intent` ordinals exactly and `buildTransform` accepts the raw int directly, so `int(intent)` is the settled spelling. The one open leg is the `renderingIntent` keyword name versus a positional `intent` argument and the `ImageCms.Intent` ordinal-value confirmation — the catalogue row names the `(…, intent)` argument without fixing the keyword spelling or the enum-member ordinals, so the `renderingIntent=int(intent)` keyword and the four-ordinal mirror stay a marked RESEARCH catalogue-deepen item until an `ImageCms.buildTransform` signature plus `ImageCms.Intent` ordinal reflection pass lands on the gated band. Close-condition: `.api` catalogue carries the `buildTransform` `renderingIntent` keyword and the `ImageCms.Intent` member ordinals. The ICC raster apply is the one gated leg crossing the cp313-band process seam.
- [ICC_SEAM]: the `Managed` arm's process dispatch is spelled `anyio.to_process.run_sync(_icc_apply, ...)`, the subprocess-seam the runtime `reliability/faults#FAULT` owner has already settled (`anyio.to_process.run_sync` named as the subprocess-seam the `async_boundary` closes over). A subinterpreter shares the host interpreter version and cannot host the cp313-band `pillow` `ImageCms` package, so the `Managed` arm crosses this genuine separate-process seam rather than the cp315-core `execution/lanes#LANE` `to_interpreter.run_sync` subinterpreter offload. The `_icc_apply` body is a module-level function dispatched by qualified name across the process seam (`to_process.run_sync` cannot target a bound method or closure), so it stays out of the `ColorManaged` owner deliberately, not as a stray helper. The seam is settled fence code by inheritance from its owner; the only open item is a branch-catalogue gap — the branch `anyio` `.api` catalogue reflects `open_process`/`run_process`/`to_thread.run_sync`/`to_interpreter.run_sync` but no `to_process` row, so `assay api` reflection over `anyio.to_process` deepens the branch catalogue to match the settled owner spelling, never re-opening this fence.
</content>
</invoke>
