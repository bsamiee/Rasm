# [PY_ARTIFACTS_COLORIMETRY]

The color-science owner feeding consistent, perceptually-correct color into the visual sub-domains. `Colorimetry` is ONE owner over colour-science (NumFOCUS-affiliated, BSD-3, NumPy-backed, pure-Python, host-free): CIE colorimetry, spectral distributions, 30+ color-space conversions, and the CIECAM02 / CAM16 appearance models. It hands palettes and color-space conversions to `charts/chart-spec#CHART`, `scene3d/scene#SCENE`, `tables/table-plan#TABLE`, and the PDF output, so every visual artifact draws color from one owner rather than each engine picking color ad hoc. colour-science is admitted in the manifest with no design-page home; this page closes the gap the `COLOR_MANAGED_VISUAL_PIPELINE` idea raises.

## [1]-[INDEX]

[CLUSTERS]: one cluster — `[2]-[COLOR]`, the colour-science colorimetry, spectral, appearance, and palette owner.

## [2]-[COLOR]

- Owner: `Colorimetry` the one color owner discriminating operation; `ColorOp` the closed `StrEnum` over the colorimetry operations; colour-science is the conversion and appearance-model engine.
- Cases: `ColorOp` rows `CONVERT` (color-space conversion via `colour.convert`) · `SPECTRAL` (spectral-distribution to tristimulus/display via `colour.sd_to_XYZ`/`XYZ_to_sRGB`) · `APPEARANCE` (CIECAM02 / CAM16 appearance correlates via `colour.XYZ_to_CIECAM02`/`XYZ_to_CAM16`) · `PALETTE` (perceptually-uniform, colorblind-safe palette derivation) — matched by `match`/`case`.
- Entry: `Colorimetry.derive` dispatches the operation and returns the converted color array or the derived palette; the result is a settled value the visual owners consume, never a re-derivation per engine.
- Auto: spectral-to-display folds the spectral distribution through `sd_to_XYZ` then `XYZ_to_sRGB`; appearance correlates fold the tristimulus through the CAM16 model under the configured viewing conditions; palette derivation folds the domain endpoints through a perceptually-uniform interpolation.
- Packages: `colour-science` (`colour.convert`/`sd_to_XYZ`/`XYZ_to_sRGB`/`XYZ_to_CIECAM02`/`XYZ_to_CAM16`), `numpy` (the array backing), runtime (`faults.RuntimeRail`/`boundary`).
- Growth: a new colorimetry operation is one `ColorOp` row plus one acceptor arm; a new appearance model is one acceptor arm; zero new surface.
- Boundary: no chart/scene rendering (that stays at the visual owners); this owner emits color arrays and palettes only, consumed inward by the visual and document sub-domains; pure-Python and host-free.

```python signature
from enum import StrEnum
from typing import assert_never

import colour
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
                return colour.XYZ_to_CAM16(self.source, **self.params)
            case ColorOp.PALETTE:
                return _palette(self.source, self.params)
            case _:
                assert_never(self.op)
```

## [3]-[RESEARCH]

- [COLOR_SPELLINGS]: the colour-science `convert`/`sd_to_XYZ`/`XYZ_to_sRGB`/`XYZ_to_CIECAM02`/`XYZ_to_CAM16` member spellings and the CAM16 viewing-condition parameter names verify against a branch `.api` catalogue authored for `colour-science`; the perceptually-uniform palette interpolation confirms against the colour-science plotting/notation helpers.
