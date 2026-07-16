# [PY_ARTIFACTS_DRAWING_STANDARD]

The `ezdxf` symbol-table LOWERING of the drafting regime — the s3 substrate authoring `drawing/regime#REGIME`'s closed vocabulary onto a real DXF `Drawing` so every drawing producer draws against seeded resources instead of re-deriving a convention per figure. `Standard.of(scale, font)` admits the drawing profile once and exposes six pure projections lowering regime's rows onto `ezdxf`'s native resource model: the symbol-table seed, the uniform pen bundle, the dimstyle override, the hatch fill, the ACI→sRGB resolve, and the paper-scale anchor. The page carries no async, no rail, and no receipt — the lowering is a synchronous builder fold the consuming producer runs inside its own offload seam.

Every vocabulary lowered here imports DOWN from `drawing/regime#REGIME` (`Discipline`, `Status`, `LayerName`, `LineType`, `LineWeight`, `HatchMaterial`, `ScaleRatio`, `TextHeight`, `Terminator`) beside the repeating-fill geometry from `graphic/vector/pattern#PATTERN`; this page adds only what is DXF-native — the discipline→ACI pen correspondence, the poché/grade fill codes, the ISO 129-1 DIM-variable derivation, and the terminator arrow-block rows. `export/dxf#DXF` composes `seed` for its document mint (never a second table writer), `dimension` composes `dimstyle`, and `annotate` composes the seeded `"ISO-3098-MLEADER"` style by name — the stable cross-page anchors.

## [01]-[INDEX]

- [01]-[STANDARD]: one drawing-profile admission and six pure projections lowering regime's vocabulary onto a live DXF document's symbol tables.

## [02]-[STANDARD]

- Owner: `Standard` is a frozen `Struct` (base `ScaleRatio`, optional textstyle font) over six pure projections; `ezdxf` owns the DXF resource model — the `Drawing`, its symbol-table entries, the `Hatch` fill setters, `colors`, `units`, `GfxAttribs`, and the `ARROWS` blocks — so no per-entity setter and no re-implemented tag writer where the symbol-table `.add` owns the resource.
- Entry: `seed` is the one imperative fold authoring every ISO resource under `doc.units = MM`, minting the named `"ISO-3098-MLEADER"` style `annotate` composes instead of the `ezdxf` `"Standard"` default; `graphics` projects the one uniform `GfxAttribs` bundle every `add_*` entity carries as `dxfattribs=`, its status-screen transparency from regime's `STATUS_SCREEN`; `dimstyle` derives the FULL ISO 129-1 DIM-variable override scaled by the ISO 5455 factor so the producer threads one override, never a per-arm literal, the `dimalt` dual-unit block OFF at the base row until `dimension`'s DIMALT arm sets it; `hatch` dispatches regime's `HATCH_BIND` onto a live `Hatch` — the `pattern` case through `to_dxf` definition rows so `ezdxf`'s renderer draws the fill (never a bare foreign name deferred to a CAD app), the `solid`/`gradient` cases through the `_FILL` ACI rows; `rgb` resolves the ACI pen sRGB and `paper_factor` anchors the ISO 5455 factor over the model→mm conversion.
- Packages: `ezdxf` owns every DXF resource — a layer authors both the ACI `color` and the resolved `true_color` (`rgb2int(aci2rgb(aci))`), a terminator maps to an `ARROWS` block name or an oblique-tick `DIMTSZ`, and the paper anchor reads `units.conversion_factor`; the `_ACI` discipline pens and `_FILL` fill codes are DXF resource-code assignments at this lowering tier, distinct from and never a second copy of regime's LCh color truth.
- Growth: a new seeded resource kind is one `seed` authoring block; a new DIM-variable one `override()` key; a new pen axis one `GfxAttribs` field; a new discipline's DXF pen one `_ACI` row; a new fill code one `_FILL` row; a new dimension family one `DimStyleFamily` member plus one `_DIMSTYLE` row; a new vocabulary member lands in regime and lowers here with zero new surface.
- Boundary: no vocabulary, codec, bind row, or derivation (`drawing/regime#REGIME`); no pattern geometry (`graphic/vector/pattern#PATTERN`); no `add_*` render (`dimension`/`annotate`/`symbol`); no color-model conversion beyond the ACI→sRGB resolve (`graphic/color/derive#DERIVE` owns model algebra over regime's LCh values); no sheet placement (`composition/sheet#SHEET`); no receipt, plan node, async, or offload (the consuming producer owns the seam); no IFC (`csharp:Rasm.Bim`).

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from typing import TYPE_CHECKING, Final, Self, assert_never

from beartype import beartype
from enum import StrEnum
from expression import Nothing, Option
from expression.collections import Map
from msgspec import Struct

from rasm.artifacts.drawing.regime import (
    Discipline,
    HATCH_BIND,
    HatchMaterial,
    LayerName,
    LineType,
    STATUS_SCREEN,
    ScaleRatio,
    Terminator,
    TextHeight,
)
from rasm.artifacts.graphic.vector.pattern import HatchFill, to_dxf

lazy from ezdxf import colors as _colors
lazy from ezdxf import units as _units
lazy from ezdxf.gfxattribs import GfxAttribs

if TYPE_CHECKING:
    from ezdxf.document import Drawing
    from ezdxf.entities import Hatch


# --- [TYPES] ----------------------------------------------------------------------------
class DimStyleFamily(StrEnum):  # ISO 129-1 dimension-style families
    ARCHITECTURAL = "architectural"
    ENGINEERING = "engineering"
    CIVIL = "civil"
    ANGULAR = "angular"
    RADIAL = "radial"


# --- [MODELS] ---------------------------------------------------------------------------
class DimStyleFamilyRow(Struct, frozen=True):
    terminator: Terminator
    height: TextHeight  # DIMTXT base
    arrow_size: float  # DIMASZ base (mm)
    extension: float  # DIMEXE
    offset: float  # DIMEXO
    gap: float  # DIMGAP
    baseline: float  # DIMDLI
    decimals: int  # DIMDEC
    unit: str  # "mm"/"m"/"deg" — dimension reads for the suffix/DIMLFAC decision


class DimStyleSpec(Struct, frozen=True):
    family: DimStyleFamily
    scale: ScaleRatio
    text: TextHeight

    @classmethod
    @beartype
    def of(cls, family: DimStyleFamily, scale: ScaleRatio = ScaleRatio.FULL, text: TextHeight | None = None) -> Self:
        return cls(family=family, scale=scale, text=text or _DIMSTYLE[family].height)

    def override(self) -> "frozendict[str, object]":
        # full ISO 129-1 DIM-variable derivation scaled by the ISO 5455 factor, threaded into add_*_dim(override=).
        row, s = _DIMSTYLE[self.family], self.scale.factor
        blk, tick = _TERMINATOR[row.terminator]
        base: dict[str, object] = {
            "dimtxt": self.text.mm / s,
            "dimasz": row.arrow_size / s,
            "dimexe": row.extension / s,
            "dimexo": row.offset / s,
            "dimgap": row.gap / s,
            "dimdli": row.baseline / s,
            "dimcen": row.arrow_size / (2.0 * s),
            "dimtsz": tick / s,
            "dimtad": 1,
            "dimjust": 0,
            "dimtih": 0,
            "dimtoh": 0,
            "dimtofl": 1,
            "dimtix": 0,
            "dimse1": 0,
            "dimse2": 0,
            "dimsd1": 0,
            "dimsd2": 0,
            "dimlunit": 2,
            "dimdec": row.decimals,
            "dimdsep": ".",
            "dimzin": 8,
            "dimpost": "",
            "dimalt": 0,
            "dimaltf": 25.4,
            "dimaltd": 2,
            "dimapost": "",
            "dimscale": 1.0,
            "dimtxsty": "ISO-3098",
            "dimtol": 0,
        }
        return frozendict({**base, **({"dimblk": blk} if blk else {})})


class Standard(Struct, frozen=True):
    scale: ScaleRatio = ScaleRatio.FULL
    font: Option[str] = Nothing  # Textstyle font (shx/ttf); Nothing -> ezdxf isocp default

    @classmethod
    def of(cls, scale: ScaleRatio = ScaleRatio.FULL, font: Option[str] = Nothing) -> Self:
        return cls(scale=scale, font=font)

    def seed(self, doc: "Drawing", layers: tuple[LayerName, ...] = (), families: tuple[DimStyleFamily, ...] = ()) -> "Drawing":
        # Exemption: the ezdxf symbol-table builder mutates the native Drawing in place — one imperative
        # fold over regime's closed vocabulary, no per-resource method family.
        doc.units = _units.MM
        for lt in LineType:
            if lt is not LineType.CONTINUOUS and lt.value not in doc.linetypes:
                doc.linetypes.add(lt.value, pattern=list(lt.pattern), description=f"ISO 128 {lt.name.lower()}")
        if "ISO-3098" not in doc.styles:
            doc.styles.add("ISO-3098", font=self.font.default_value("isocp.shx"))
        if not doc.mleader_styles.has_entry("ISO-3098-MLEADER"):
            mleader = doc.mleader_styles.new("ISO-3098-MLEADER")
            mleader.dxf.text_style_handle = doc.styles.get("ISO-3098").dxf.handle
            mleader.dxf.char_height = TextHeight.H3_5.mm
            mleader.dxf.landing_gap_size = 0.9
            mleader.dxf.dogleg_length = 4.0
        for name in layers:
            pen = name.pen
            aci = _ACI[name.discipline]
            doc.layers.add(
                name.compose(),
                color=aci,
                true_color=_colors.rgb2int(_colors.aci2rgb(aci)),
                linetype=pen.linetype.value,
                lineweight=round(pen.lineweight.mm * 100),
            )
        for family in families:
            dimstyle = doc.dimstyles.add(family.value)
            for key, value in DimStyleSpec.of(family, self.scale).override().items():
                dimstyle.dxf.set(key, value)
        return doc

    def graphics(self, layer: LayerName) -> GfxAttribs:
        # the one uniform bundle every add_* entity carries as dxfattribs=.
        pen = layer.pen
        return GfxAttribs(
            layer=layer.compose(),
            color=_ACI[layer.discipline],
            rgb=self.rgb(layer),
            linetype=pen.linetype.value,
            lineweight=round(pen.lineweight.mm * 100),
            ltscale=self.scale.factor,
            transparency=STATUS_SCREEN[layer.status],
        )

    def dimstyle(self, family: DimStyleFamily, scale: ScaleRatio | None = None) -> "frozendict[str, object]":
        return DimStyleSpec.of(family, scale or self.scale).override()

    def hatch(self, hatch: "Hatch", material: HatchMaterial, factor: float | None = None) -> "Hatch":
        # Exemption: the ezdxf Hatch fill setters mutate the native entity in place — one total dispatch over HATCH_BIND.
        fill: HatchFill = HATCH_BIND[material]
        f = factor if factor is not None else self.scale.factor
        match fill.tag:
            case "pattern":
                hatch.set_pattern_fill(material.value, definition=[list(row) for row in to_dxf(fill.pattern, f)])
            case "solid":
                hatch.set_solid_fill(color=_FILL[material][0])
            case "gradient":
                dark, light = _FILL[material]
                hatch.set_gradient(
                    color1=_colors.RGB(*_colors.aci2rgb(dark)),
                    color2=_colors.RGB(*_colors.aci2rgb(light)),
                    rotation=fill.gradient[1],
                )
            case _ as unreachable:
                assert_never(unreachable)
        return hatch

    def rgb(self, layer: LayerName) -> tuple[int, int, int]:
        # discipline ACI pen sRGB; a CMYK/spectral swatch routes regime's LCh value through derive.
        return tuple(_colors.aci2rgb(_ACI[layer.discipline]))

    def paper_factor(self, model_units: int | None = None) -> float:
        # paper mm per model unit: ISO 5455 factor over the model->mm conversion.
        unit = model_units if model_units is not None else _units.MM
        return self.scale.factor * _units.conversion_factor(unit, _units.MM)


# --- [TABLES] ---------------------------------------------------------------------------
# DXF-native discipline pen codes (ACI); regime's LCh rows are the color truth.
_ACI: Final[Map[Discipline, int]] = Map.of_seq([
    (Discipline.ARCHITECTURAL, 7),
    (Discipline.CIVIL, 3),
    (Discipline.ELECTRICAL, 6),
    (Discipline.FIRE, 1),
    (Discipline.GENERAL, 8),
    (Discipline.HAZMAT, 30),
    (Discipline.INTERIORS, 4),
    (Discipline.LANDSCAPE, 94),
    (Discipline.MECHANICAL, 5),
    (Discipline.PLUMBING, 140),
    (Discipline.EQUIPMENT, 2),
    (Discipline.RESOURCE, 9),
    (Discipline.STRUCTURAL, 1),
    (Discipline.TELECOM, 150),
    (Discipline.SURVEY, 52),
    (Discipline.PROCESS, 40),
    (Discipline.OTHER, 250),
    (Discipline.CONTRACTOR, 253),
])
# DXF fill codes per material (solid poché ACI, gradient light ACI).
_FILL: Final[Map[HatchMaterial, tuple[int, int]]] = Map.of_seq([
    (HatchMaterial.STEEL, (250, 250)),
    (HatchMaterial.EARTH, (43, 8)),
])
# ISO 129-1 family base parameters (mm at 1:1).
_DIMSTYLE: Final[Map[DimStyleFamily, DimStyleFamilyRow]] = Map.of_seq([
    (DimStyleFamily.ARCHITECTURAL, DimStyleFamilyRow(Terminator.OBLIQUE_STROKE, TextHeight.H2_5, 2.5, 1.25, 0.625, 0.625, 7.0, 0, "mm")),
    (DimStyleFamily.ENGINEERING, DimStyleFamilyRow(Terminator.FILLED_ARROW, TextHeight.H3_5, 3.5, 1.25, 0.625, 0.9, 8.0, 1, "mm")),
    (DimStyleFamily.CIVIL, DimStyleFamilyRow(Terminator.FILLED_ARROW, TextHeight.H5, 5.0, 2.0, 1.0, 1.5, 10.0, 2, "m")),
    (DimStyleFamily.ANGULAR, DimStyleFamilyRow(Terminator.FILLED_ARROW, TextHeight.H3_5, 3.5, 1.25, 0.625, 0.9, 8.0, 1, "deg")),
    (DimStyleFamily.RADIAL, DimStyleFamilyRow(Terminator.FILLED_ARROW, TextHeight.H3_5, 3.5, 1.25, 0.625, 0.9, 8.0, 1, "mm")),
])
# ISO 129-1 termination -> (ezdxf ARROWS block name, tick factor); empty block + tick>0 = oblique tick.
_TERMINATOR: Final[Map[Terminator, tuple[str, float]]] = Map.of_seq([
    (Terminator.FILLED_ARROW, ("", 0.0)),
    (Terminator.OPEN_ARROW, ("OPEN", 0.0)),
    (Terminator.OBLIQUE_STROKE, ("", 2.5)),
    (Terminator.DOT, ("DOT", 0.0)),
    (Terminator.NONE, ("NONE", 0.0)),
])

# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "DimStyleFamily",
    "DimStyleSpec",
    "Standard",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
