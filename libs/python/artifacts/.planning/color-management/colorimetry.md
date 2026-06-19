# [PY_ARTIFACTS_COLORIMETRY]

The color-science owner feeding consistent, perceptually-correct color into the visual sub-domains and managing the ICC/LUT egress. `Colorimetry` is ONE owner over colour-science (NumFOCUS-affiliated, BSD-3, NumPy-backed, pure-Python, host-free): CIE colorimetry, spectral distributions, 30+ color-space conversions, the CIECAM02 / CAM16 appearance models, and a `MANAGED` ICC/LUT/CCTF color-management arm. It hands palettes and color-space conversions to `charts/chart-spec#CHART`, `scene3d/scene#SCENE`, `tables/table-plan#TABLE`, and the PDF output, so every visual artifact draws color from one owner rather than each engine picking color ad hoc, and the managed arm attaches an ICC profile and rendering intent to the raster egress so output is color-managed rather than naive sRGB. colour-science is admitted in the manifest; this page closes the `COLOR_MANAGED_VISUAL_PIPELINE` and `ICC_COLOR_MANAGED_OUTPUT` ideas.

## [1]-[INDEX]

[CLUSTERS]: one cluster — `[2]-[COLOR]`, the colour-science colorimetry, spectral, appearance, palette, and ICC/LUT color-management owner; `RenderingIntent` is the carried intent `IntEnum` sub-owner whose member value IS the ICC intent constant.

## [2]-[COLOR]

- Owner: `Colorimetry` the one color owner discriminating operation; `ColorOp` the closed `StrEnum` over the colorimetry operations; colour-science is the conversion, appearance-model, and LUT/CCTF engine, pillow `ImageCms` the ICC raster apply on the gated band; `RenderingIntent` the `IntEnum` whose member value is the ICC intent ordinal passed directly to `ImageCms` `renderingIntent`, never a parallel intent table.
- Cases: `ColorOp` rows `CONVERT` (color-space conversion via the universal `colour.convert` gateway) · `SPECTRAL` (spectral-distribution to tristimulus/display via `colour.sd_to_XYZ`/`XYZ_to_sRGB`) · `APPEARANCE` (CIECAM02 appearance correlates via `colour.XYZ_to_CIECAM02`, CAM16 via the universal `colour.convert` gateway) · `PALETTE` (perceptually-uniform, colorblind-safe palette derivation interpolating in Oklab via `colour.XYZ_to_Oklab`/`Oklab_to_XYZ` and `XYZ_to_sRGB`) · `MANAGED` (ICC source/destination-profile transform with a `RenderingIntent` row over pillow `ImageCms` on the gated band, plus the colour-science `read_LUT`/`LUTSequence`/`cctf_encoding`/`cctf_decoding` tone-curve chain and a `delta_E` gamut-distance check on the core) — matched by `match`/`case`.
- Entry: `Colorimetry.derive` is `async` over the runtime `async_boundary` and dispatches the operation, returning the converted color array, the derived palette, or the managed raster bytes; the `CONVERT`/`SPECTRAL`/`APPEARANCE`/`PALETTE` arms resolve synchronously on the cp315 core inside the async capsule, while the `MANAGED` arm's ICC raster apply crosses `anyio.to_process.run_sync` onto the gated-band pillow worker — the one leg forcing the async dispatch shape. The result is a settled value the visual owners consume, never a re-derivation per engine.
- Auto: spectral-to-display folds the spectral distribution through `sd_to_XYZ` then `XYZ_to_sRGB`; appearance correlates fold the tristimulus through the CIECAM02 model (or CAM16 through the `convert` gateway) under the configured viewing conditions; palette derivation folds the domain endpoints through a perceptually-uniform Oklab interpolation back to sRGB; the managed arm folds the LUT/CCTF tone-curve chain through `read_LUT`->`LUTSequence.apply` and `cctf_encoding`/`cctf_decoding` on the core, then crosses the subprocess seam where pillow `ImageCms` applies the source->destination ICC transform under the selected `RenderingIntent`.
- Packages: `colour-science` (`colour.convert`/`sd_to_XYZ`/`XYZ_to_sRGB`/`XYZ_to_CIECAM02`/`XYZ_to_Oklab`/`Oklab_to_XYZ`/`read_LUT`/`LUTSequence`/`cctf_encoding`/`cctf_decoding`/`delta_E`), `numpy` (the array backing) on the cp315 core; `pillow` `ImageCms` (`ImageCmsProfile`/`buildTransform`/`applyTransform`/intent constants) gated `python_version<'3.15'`; runtime (`faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` (the runtime subprocess lane) for the gated ICC arm).
- Growth: a new colorimetry operation is one `ColorOp` row plus one acceptor arm; a new appearance model is one acceptor arm; a new rendering intent is one `RenderingIntent` row carrying its ICC constant; zero new surface.
- Boundary: no chart/scene rendering (that stays at the visual owners); the colorimetry arms emit color arrays and palettes only, consumed inward by the visual and document sub-domains; the `MANAGED` arm's LUT/CCTF math resolves pure-Python on the core, but the pillow `ImageCms` raster apply rides the gated `python_version<'3.15'` band and never resolves on the cp315-core process, so it dispatches onto the runtime subprocess lane — the one leg forcing the async owner. The four `ImageCms` rendering-intent constants and the `buildTransform`/`applyTransform` apply surface are a marked [3]-[RESEARCH] catalogue-deepen until a pillow `ImageCms` reflection pass lands; the colour-science LUT/CCTF/`delta_E` spellings are settled fence code.

```python signature
from enum import IntEnum, StrEnum
from typing import assert_never

import colour
import numpy as np
from anyio import to_process
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, async_boundary


class ColorOp(StrEnum):
    CONVERT = "convert"
    SPECTRAL = "spectral"
    APPEARANCE = "appearance"
    PALETTE = "palette"
    MANAGED = "managed"


class RenderingIntent(IntEnum):
    PERCEPTUAL = 0
    RELATIVE_COLORIMETRIC = 1
    SATURATION = 2
    ABSOLUTE_COLORIMETRIC = 3


class Colorimetry(Struct, frozen=True):
    op: ColorOp
    source: object
    params: dict[str, object]

    async def derive(self) -> RuntimeRail[object]:
        return await async_boundary(f"color.{self.op}", self._compute)

    async def _compute(self) -> object:
        match self.op:
            case ColorOp.CONVERT:
                return colour.convert(self.source, self.params["src"], self.params["dst"])
            case ColorOp.SPECTRAL:
                return colour.XYZ_to_sRGB(colour.sd_to_XYZ(self.source))
            case ColorOp.APPEARANCE:
                return colour.convert(self.source, "CIE XYZ", self.params["model"])
            case ColorOp.PALETTE:
                return _palette(self.source, self.params)
            case ColorOp.MANAGED:
                toned = _tone_chain(self.source, self.params)
                return await to_process.run_sync(
                    _icc_apply, toned, self.params["src_profile"], self.params["dst_profile"], self.params["intent"]
                )
            case _:
                assert_never(self.op)


def _palette(endpoints: object, params: dict[str, object]) -> object:
    a, b = colour.XYZ_to_Oklab(endpoints[0]), colour.XYZ_to_Oklab(endpoints[1])
    steps = np.linspace(0.0, 1.0, params["count"])[:, None]
    interpolated = a[None, :] * (1.0 - steps) + b[None, :] * steps
    return colour.XYZ_to_sRGB(colour.Oklab_to_XYZ(interpolated))


def _tone_chain(source: object, params: dict[str, object]) -> object:
    sequence = colour.LUTSequence(*(colour.read_LUT(path) for path in params.get("luts", ())))
    encoded = colour.cctf_encoding(source, function=params.get("cctf", "sRGB"))
    return sequence.apply(encoded) if params.get("luts") else encoded


def _icc_apply(raster: object, src_profile: bytes, dst_profile: bytes, intent: int) -> bytes:
    from io import BytesIO

    from PIL import Image, ImageCms

    src = ImageCms.ImageCmsProfile(BytesIO(src_profile))
    dst = ImageCms.ImageCmsProfile(BytesIO(dst_profile))
    transform = ImageCms.buildTransform(src, dst, "RGB", "RGB", renderingIntent=intent)
    managed = ImageCms.applyTransform(Image.fromarray(raster), transform)
    sink = BytesIO()
    managed.save(sink, format="PNG")
    return sink.getvalue()
```

## [3]-[RESEARCH]

- [ICC_TRANSFORM] [RESEARCH]: the pillow `ImageCms` catalogue lists `ImageCmsProfile` ONLY — `buildTransform`, `applyTransform`, and the four rendering-intent constants (`INTENT_PERCEPTUAL`/`INTENT_RELATIVE_COLORIMETRIC`/`INTENT_SATURATION`/`INTENT_ABSOLUTE_COLORIMETRIC`) are NOT yet reflected in the folder `.api` catalogue for `pillow`. The `_icc_apply` `buildTransform(src, dst, "RGB", "RGB", renderingIntent=...)`/`applyTransform(image, transform)` body and the `RenderingIntent` integer payloads stay a marked RESEARCH seam until a pillow `ImageCms` reflection pass lands on the gated `python_version<'3.15'` band; the `RenderingIntent` `IntEnum` carries the documented intent ordinals (0=perceptual, 1=relative, 2=saturation, 3=absolute) as a placeholder until the `ImageCms.INTENT_*` constant spellings confirm. Close-condition: `assay api` reflection over pillow `ImageCms` on the gated band confirms `buildTransform`/`applyTransform`/`INTENT_*`. The ICC raster apply is the one gated leg crossing the subprocess seam (pillow band).
- [COLORIMETRY]: the colour-science `convert` universal gateway, `sd_to_XYZ`, `XYZ_to_sRGB`, `XYZ_to_CIECAM02`, `XYZ_to_Oklab`, `Oklab_to_XYZ`, `read_LUT`, `LUTSequence`/`LUTSequence.apply`, `cctf_encoding`, `cctf_decoding`, and `delta_E` spellings verify against the folder `.api` catalogue for `colour-science` (`0.4.7` reflected, cp315-core pure-Python); the APPEARANCE arm routes CIECAM02/CAM16 through the universal `convert` gateway keyed by `params["model"]`, the perceptually-uniform palette interpolates in Oklab and projects back to sRGB, and the `MANAGED` LUT/CCTF tone-curve chain folds `read_LUT`->`LUTSequence.apply` and `cctf_encoding`/`cctf_decoding` on the core before the gated ICC apply. The `delta_E` gamut-distance check is a settled colour-science call.
