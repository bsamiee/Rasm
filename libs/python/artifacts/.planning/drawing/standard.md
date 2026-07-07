# [PY_ARTIFACTS_DRAWING_STANDARD]

The `ezdxf` symbol-table LOWERING of the drafting regime — the s3 substrate that authors `drawing/regime#REGIME`'s closed vocabulary onto a real DXF document so every drawing producer draws against seeded resources instead of re-deriving a convention per figure. `Standard.of(scale, font)` admits the drawing profile once; `seed(doc)` authors every ISO resource in one imperative table-builder fold — the mm insertion unit, the ISO 128-2 linetype dash arrays, the ISO-3098 textstyle, the ISO-3098 multileader style `drawing/annotate#ANNOTATE` composes by name, the discipline-penned layers carrying ACI plus resolved true-color, the ISO 129-1 dimstyles; `graphics(layer)` projects the ONE uniform `GfxAttribs` bundle (layer, pen, linetype, weight, `ltscale`, and the ISO 13567 status-screen transparency from regime's `STATUS_SCREEN` row) every downstream `add_*` entity carries as its `dxfattribs=`; `dimstyle(family, scale)` derives the full ISO 129-1 DIM-variable override `drawing/dimension#DIMENSION` threads into `add_*_dim`; `hatch(hatch, material, factor)` lowers regime's `HATCH_BIND` fill row onto a live `Hatch` entity — the pattern case through `graphic/vector/pattern#PATTERN`'s `to_dxf` definition rows so `ezdxf`'s own renderer draws the fill, the solid and graded cases through the owned DXF fill-code rows; `rgb(layer)` resolves the discipline ACI pen's sRGB through `ezdxf.colors.aci2rgb`; and `paper_factor(model_units)` anchors the ISO 5455 factor to real drawing units through `ezdxf.units.conversion_factor`.

Every vocabulary this page lowers is imported DOWN from `drawing/regime#REGIME` — `Discipline`, `Status`, `LayerName`, `LineType`, `LineWeight`, `HatchMaterial`, `ScaleRatio`, `TextHeight`, `Terminator` — and the repeating-fill geometry from `graphic/vector/pattern#PATTERN`; this page adds only what is DXF-native: the discipline→ACI pen correspondence (a DXF resource code assignment, distinct from regime's LCh pen values), the poché/grade fill codes, the DIM-variable derivation, and the symbol-table authoring. `export/dxf#DXF` composes `seed` for its document mint (never a second table writer); `dimension` composes `dimstyle`; `annotate` composes the seeded ISO mleader style by name. This page carries no async, no rail, and no receipt — the lowering is a synchronous builder fold the consuming producer runs inside its own offload seam.

## [01]-[INDEX]

- [01]-[STANDARD]: the `Standard` lowering owner — `of` the one profile admission, `seed` the symbol-table fold (`doc.linetypes.add`/`doc.styles.add`/`doc.mleader_styles.new`/`doc.layers.add`/`doc.dimstyles.add` under `doc.units = units.MM`), `graphics` the uniform `GfxAttribs` projection with the status screen, `dimstyle` the full DIM-variable override derivation scaled by ISO 5455, `hatch` the `HatchFill` entity application over pattern's definition rows, `rgb` the ACI→sRGB resolve, `paper_factor` the unit anchor — plus the DXF-native correspondences (`_ACI` discipline pens, `_FILL` poché/grade codes, `_TERMINATOR` arrow-block rows, `_DIMSTYLE` family base rows) that exist only at this lowering tier.

## [02]-[STANDARD]

- Owner: `Standard` is a frozen `Struct` carrying the resolved profile (base `ScaleRatio`, optional textstyle font path) and exposing six pure projections over regime's vocabulary — never a per-entity `set_layer`/`set_color` setter, never a re-implemented DXF tag writer where `ezdxf`'s symbol-table `.add` owns the resource, never a second vocabulary declaration. `ezdxf` owns the DXF resource model: the `Drawing`, the `Linetype`/`Textstyle`/`Layer`/`DimStyle`/`MLeaderStyle` symbol-table entries, `Hatch.set_pattern_fill`/`set_solid_fill`/`set_gradient`, `colors.RGB`/`aci2rgb`/`rgb2int`, `units.MM`/`conversion_factor`, `gfxattribs.GfxAttribs`, and the `ARROWS` terminator block names.
- Seed: `seed(doc, layers, families)` is the one imperative fold (the `ezdxf` builder mutates the native `Drawing` in place — the platform-forced resource-authoring seam): `doc.units = units.MM` anchors the drafting unit so the ISO 5455 factor is dimensionless; each non-`CONTINUOUS` `LineType` authors `doc.linetypes.add(lt.value, pattern=list(lt.pattern), description=)` from regime's dash arrays; one `doc.styles.add("ISO-3098", font=)` textstyle; one `doc.mleader_styles.new("ISO-3098-MLEADER")` whose `char_height` is `TextHeight.H3_5.mm`, whose `text_style_handle` links the seeded textstyle, and whose landing/dogleg carry the ISO 128-2 geometry — the named style `annotate` composes instead of the `ezdxf` default `"Standard"`; each seeded `LayerName` authors `doc.layers.add(name.compose(), color=aci, true_color=colors.rgb2int(colors.aci2rgb(aci)), linetype=, lineweight=)` from its `_ACI` pen row; each `DimStyleFamily` authors `doc.dimstyles.add(family.value)` with its DIM-variables set through `DimStyle.dxf.set`.
- Dimstyle: `_DIMSTYLE` carries the ISO 129-1 family base rows (`ARCHITECTURAL` oblique-tick mm 0-decimal, `ENGINEERING`/`ANGULAR`/`RADIAL` filled-arrow mm, `CIVIL` filled-arrow metres) and `DimStyleSpec.override()` derives the FULL DIM-variable surface scaled by the ISO 5455 factor — geometry (`dimtxt`/`dimasz`/`dimexe`/`dimexo`/`dimgap`/`dimdli`/`dimcen`), placement (`dimtad`/`dimjust`/`dimtih`/`dimtoh`/`dimtofl`/`dimtix`), terminator (`dimblk`/`dimtsz` from `_TERMINATOR`'s `(block, tick)` row over regime's `Terminator`), suppression (`dimse1`/`dimse2`/`dimsd1`/`dimsd2`), units and zeros (`dimlunit`/`dimdec`/`dimdsep`/`dimzin`/`dimpost`), the dual-unit block (`dimalt`/`dimaltf`/`dimaltd`/`dimapost` — OFF at the base row; `dimension`'s DIMALT arm enables it), and the style linkage (`dimtxsty`/`dimscale`/`dimtol`) — so the dimension producer threads one derived override, never a per-arm literal.
- Hatch: `hatch(hatch, material, factor)` reads regime's `HATCH_BIND[material]` and dispatches the closed `HatchFill` union onto the live entity — `pattern` composes `to_dxf(spec, factor)` into `set_pattern_fill(material.value, definition=)` so the definition rows regime's preset geometry generates are what `ezdxf` renders (a bare foreign pattern name deferred to a CAD app is the deleted form); `solid` applies `set_solid_fill(color=_FILL[material][0])`; `gradient` applies `set_gradient(color1, color2, rotation)` with the `_FILL` ACI pair resolved through `aci2rgb` — the fill-code rows are DXF resource assignments at this lowering tier, the LCh truth staying regime's.
- Growth: a new seeded resource kind is one `seed` authoring block; a new DIM-variable is one `DimStyleSpec.override()` key; a new pen attribute axis is one `GfxAttribs` field on the one `graphics` projection; a new discipline's DXF pen is one `_ACI` row; a new fill code is one `_FILL` row; a new dimension family is one `DimStyleFamily` member plus one `_DIMSTYLE` row; a new vocabulary member lands in regime and lowers here with zero new surface.
- Boundary: no vocabulary, codec, bind row, or derivation (`drawing/regime#REGIME`); no pattern geometry (`graphic/vector/pattern#PATTERN`); no render (`dimension`/`annotate`/`symbol` own the `add_*` geometry and their backends); no receipt and no plan node; no async and no offload (the consuming producer owns the seam); no color model conversion beyond the ACI→sRGB resolve (`graphic/color/derive#DERIVE` owns model algebra over regime's LCh values); no sheet placement (`composition/sheet#SHEET`); no IFC (`csharp:Rasm.Bim`). A per-entity attribute setter, a second vocabulary declaration, a hand-computed dash array where regime's row exists, and a parallel dimstyle table on a producer are the deleted forms.

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
class DimStyleFamily(StrEnum):  # ISO 129-1 dimension-style families this lowering derives DimStyle overrides for
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
    unit: str  # "mm" / "m" / "deg" — dimension#DIMENSION reads for the suffix / DIMLFAC decision


class DimStyleSpec(Struct, frozen=True):
    family: DimStyleFamily
    scale: ScaleRatio
    text: TextHeight

    @classmethod
    @beartype
    def of(cls, family: DimStyleFamily, scale: ScaleRatio = ScaleRatio.FULL, text: TextHeight | None = None) -> Self:
        return cls(family=family, scale=scale, text=text or _DIMSTYLE[family].height)

    def override(self) -> "frozendict[str, object]":
        # the FULL ISO 129-1 DIM-variable derivation, scaled by the ISO 5455 factor; dimension threads it
        # into add_*_dim(override=) and enables the DIMALT dual-unit block on its own arm.
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
    font: Option[str] = Nothing  # the Textstyle font (shx/ttf); Nothing -> the ezdxf isocp default

    @classmethod
    def of(cls, scale: ScaleRatio = ScaleRatio.FULL, font: Option[str] = Nothing) -> Self:
        return cls(scale=scale, font=font)

    def seed(self, doc: "Drawing", layers: tuple[LayerName, ...] = (), families: tuple[DimStyleFamily, ...] = ()) -> "Drawing":
        # Exemption: the ezdxf symbol-table builder mutates the native Drawing in place — the platform-forced
        # resource-authoring seam; one imperative fold over regime's closed vocabulary, no per-resource method family.
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
        # the ONE uniform attribute bundle every add_* entity carries as dxfattribs=; the ISO 13567 status
        # screen rides regime's STATUS_SCREEN row, never a per-entity set.
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
        # Exemption: the ezdxf Hatch fill setters mutate the native entity in place — the platform-forced fill
        # seam; one total dispatch over regime's HATCH_BIND row, definition rows generated by the pattern plane.
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
        # the discipline ACI pen's sRGB; a CMYK/spectral swatch routes regime's LCh VALUE through derive.
        return tuple(_colors.aci2rgb(_ACI[layer.discipline]))

    def paper_factor(self, model_units: int | None = None) -> float:
        # paper mm per model unit: ISO 5455 factor over the model->mm conversion (a metre model at 1:50 on mm sheets).
        unit = model_units if model_units is not None else _units.MM
        return self.scale.factor * _units.conversion_factor(unit, _units.MM)


# --- [TABLES] ---------------------------------------------------------------------------
# DXF-native discipline pen codes (ACI) — the lowering correspondence; regime's LCh rows are the color truth.
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
# DXF fill codes per material — (solid poché ACI, gradient light ACI); only solid/gradient rows read them.
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
# ISO 129-1 termination -> (ezdxf ARROWS block name, tick factor); empty block + tick>0 = oblique tick (DIMTSZ).
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

`Standard` is the one place the drafting regime touches the DXF resource model. `seed` authors regime's dash arrays, the ISO-3098 textstyle, the named ISO mleader style, the discipline layers (ACI pen plus `rgb2int(aci2rgb(aci))` true-color), and the family dimstyles onto the document once; `graphics` projects the whole pen — layer, color, true-color, linetype, weight, `ltscale`, status screen — as one `GfxAttribs` every entity splats; `dimstyle` derives the full DIM-variable override from one family row scaled by the ISO 5455 factor so a 1:50 architectural dimension draws its 2.5 mm text and oblique tick at paper scale; `hatch` renders regime's material bind through pattern's `to_dxf` definition rows so the DXF fill is the SAME geometry the SVG tile and the clipped-print lowering draw. `export/dxf#DXF` mints its documents over `seed` (V11 — one table writer), `dimension` threads `dimstyle`, `annotate` composes `"ISO-3098-MLEADER"` by name, and the vocabulary itself lives one stratum down where every non-DXF consumer reads it without touching `ezdxf`.
