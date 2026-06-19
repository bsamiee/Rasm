# [PY_ARTIFACTS_COLORIMETRY]

The color-science owner feeding consistent, perceptually-correct color into the visual sub-domains. `Colorimetry` is ONE owner over colour-science (NumFOCUS-affiliated, BSD-3, NumPy-backed, pure-Python, host-free): CIE colorimetry, spectral distributions, 30+ color-space conversions, and the CIECAM02 / CAM16 appearance models. It hands palettes and color-space conversions to `charts/chart-spec#CHART`, `scene3d/scene#SCENE`, `tables/table-plan#TABLE`, and the PDF output, so every visual artifact draws color from one owner rather than each engine picking color ad hoc. colour-science is admitted in the manifest with no design-page home; this page closes the gap the `COLOR_MANAGED_VISUAL_PIPELINE` idea raises.

## [1]-[INDEX]

[CLUSTERS]: one cluster — `[2]-[COLOR]`, the colour-science colorimetry, spectral, appearance, and palette owner.

## [2]-[COLOR]

- Owner: `Colorimetry` the one color owner discriminating operation; `ColorOp` the closed `StrEnum` over the colorimetry operations; colour-science is the conversion and appearance-model engine.
- Cases: `ColorOp` rows `CONVERT` (color-space conversion via the universal `colour.convert` gateway) · `SPECTRAL` (spectral-distribution to tristimulus/display via `colour.sd_to_XYZ`/`XYZ_to_sRGB`) · `APPEARANCE` (CIECAM02 appearance correlates via `colour.XYZ_to_CIECAM02`, CAM16 via the universal `colour.convert` gateway) · `PALETTE` (perceptually-uniform, colorblind-safe palette derivation interpolating in Oklab via `colour.XYZ_to_Oklab`/`Oklab_to_XYZ` and `XYZ_to_sRGB`) — matched by `match`/`case`.
- Entry: `Colorimetry.derive` dispatches the operation and returns the converted color array or the derived palette; the result is a settled value the visual owners consume, never a re-derivation per engine.
- Auto: spectral-to-display folds the spectral distribution through `sd_to_XYZ` then `XYZ_to_sRGB`; appearance correlates fold the tristimulus through the CIECAM02 model (or CAM16 through the `convert` gateway) under the configured viewing conditions; palette derivation folds the domain endpoints through a perceptually-uniform Oklab interpolation back to sRGB.
- Packages: `colour-science` (`colour.convert`/`sd_to_XYZ`/`XYZ_to_sRGB`/`XYZ_to_CIECAM02`/`XYZ_to_Oklab`/`Oklab_to_XYZ`), `numpy` (the array backing), runtime (`faults.RuntimeRail`/`boundary`).
- Growth: a new colorimetry operation is one `ColorOp` row plus one acceptor arm; a new appearance model is one acceptor arm; zero new surface.
- Boundary: no chart/scene rendering (that stays at the visual owners); this owner emits color arrays and palettes only, consumed inward by the visual and document sub-domains; pure-Python and host-free.

```python signature
from enum import StrEnum
from typing import assert_never

import colour
import numpy as np
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary


class ColorOp(StrEnum):
    CONVERT = "convert"
    SPECTRAL = "spectral"
    APPEARANCE = "appearance"
    PALETTE = "palette"


class Colorimetry(Struct, frozen=True):
    op: ColorOp
    source: object
    params: dict[str, object]

    def derive(self) -> RuntimeRail[object]:
        return boundary(f"color.{self.op}", self._compute)

    def _compute(self) -> object:
        match self.op:
            case ColorOp.CONVERT:
                return colour.convert(self.source, self.params["src"], self.params["dst"])
            case ColorOp.SPECTRAL:
                return colour.XYZ_to_sRGB(colour.sd_to_XYZ(self.source))
            case ColorOp.APPEARANCE:
                return colour.convert(self.source, "CIE XYZ", self.params["model"])
            case ColorOp.PALETTE:
                return _palette(self.source, self.params)
            case _:
                assert_never(self.op)


def _palette(endpoints: object, params: dict[str, object]) -> object:
    a, b = colour.XYZ_to_Oklab(endpoints[0]), colour.XYZ_to_Oklab(endpoints[1])
    steps = np.linspace(0.0, 1.0, params["count"])[:, None]
    interpolated = a[None, :] * (1.0 - steps) + b[None, :] * steps
    return colour.XYZ_to_sRGB(colour.Oklab_to_XYZ(interpolated))
```

## [3]-[RESEARCH]

No open items. The colour-science `convert` universal gateway, `sd_to_XYZ`, `XYZ_to_sRGB`, `XYZ_to_CIECAM02`, `XYZ_to_Oklab`, and `Oklab_to_XYZ` spellings verify against the folder `.api` catalogue for `colour-science`; the APPEARANCE arm routes CIECAM02/CAM16 through the universal `convert` gateway keyed by `params["model"]`, and the perceptually-uniform palette interpolates in Oklab and projects back to sRGB.
