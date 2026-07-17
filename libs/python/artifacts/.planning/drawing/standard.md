# [PY_ARTIFACTS_DRAWING_STANDARD]

`Standard` lowers the drafting regime onto an `ezdxf.Drawing` so every producer consumes one seeded resource vocabulary. `Standard.of(scale, font)` admits the profile once; its projections seed symbol tables, derive graphics and dimension overrides, lower hatch fills through a typed `Result`, resolve legend and pen colors, and anchor paper scale. Consumers run this synchronous builder inside their offload seam.

Every vocabulary lowered here imports DOWN from `drawing/regime#REGIME` (`Discipline`, `Status`, `LayerName`, `LineType`, `LineWeight`, `HatchMaterial`, `ScaleRatio`, `TextHeight`, `Terminator`) beside the repeating-fill geometry from `graphic/vector/pattern#PATTERN`; this page adds only what is DXF-native — the discipline→ACI pen correspondence, the poché/grade fill codes, the ISO 129-1 DIM-variable derivation, and the terminator arrow-block rows. `export/dxf#DXF` composes `seed` for its document mint (never a second table writer), `dimension` composes `dimstyle`, `annotate` composes the seeded `"ISO-3098-MLEADER"` style by name, and `drawing/schedule#SCHEDULE` composes `swatch` for its legend cells — the stable cross-page anchors.

## [01]-[INDEX]

- [01]-[STANDARD]: one drawing-profile admission lowering regime vocabulary onto live DXF resources.

## [02]-[STANDARD]

- Owner: `Standard` is a frozen `Struct` over a base `ScaleRatio` and optional textstyle font; `ezdxf` owns `Drawing`, symbol-table entries, `Hatch` fill setters, `colors`, `units`, `GfxAttribs`, and `ARROWS` blocks.
- Entry: `seed` authors ISO resources under `doc.units = MM` and mints `"ISO-3098-MLEADER"`; `graphics` projects one `GfxAttribs` bundle with `STATUS_SCREEN` transparency; `dimstyle` derives ISO 129-1 variables under the ISO 5455 factor; `hatch` lowers `HATCH_BIND` through `lowered(PatternOp.Dxf(...))` and preserves `PatternFault`; `swatch` projects the entity-free legend twin; `rgb` resolves ACI sRGB; `paper_factor` composes model-to-millimetre conversion.
- Packages: `ezdxf` owns every DXF resource — a layer authors both the ACI `color` and the resolved `true_color` (`rgb2int(aci2rgb(aci))`), a terminator maps to a verified `ezdxf.ARROWS` block name (`OPEN`/`DOT`/`ORIGIN`/`NONE`) or an oblique-tick `DIMTSZ`, and the paper anchor reads `units.conversion_factor`; the `_ACI` discipline pens and `_FILL` fill codes are DXF resource-code assignments at this lowering tier, distinct from and never a second copy of regime's LCh color truth.
- Growth: a new seeded resource kind is one `seed` authoring block; a new DIM-variable one `override()` key; a new pen axis one `GfxAttribs` field; a new discipline's DXF pen one `_ACI` row; a new fill code one `_FILL` row; a new legend fact one `SwatchSpec` field; a new dimension family one `DimStyleFamily` member plus one `_DIMSTYLE` row; a new vocabulary member lands in regime and lowers here with zero new surface.
- Boundary: no vocabulary, codec, bind row, or derivation (`drawing/regime#REGIME`); no pattern geometry (`graphic/vector/pattern#PATTERN`); no `add_*` render (`dimension`/`annotate`/`symbol`); no color-model conversion beyond the ACI→sRGB resolve (`graphic/color/derive#DERIVE` owns model algebra over regime's LCh values); no sheet placement (`composition/sheet#SHEET`); no receipt, plan node, async, or offload (the consuming producer owns the seam); no IFC (`csharp:Rasm.Bim`).

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Self, assert_never

from builtins import frozendict
from expression import Error, Nothing, Ok, Option, Result
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
from rasm.artifacts.graphic.vector.pattern import HatchFill, PatternFault, PatternOp, PatternResult, lowered

lazy from ezdxf import colors as _colors
lazy from ezdxf import units as _units
lazy from ezdxf.gfxattribs import GfxAttribs

if TYPE_CHECKING:
    from ezdxf.document import Drawing
    from ezdxf.entities import Hatch


# --- [TYPES] ----------------------------------------------------------------------------
type DimVar = str | int | float  # the DXF dimstyle-variable value domain — dimpost/dimtxsty/dimblk are strings beside the numeric variables


class DimStyleFamily(StrEnum):  # ISO 129-1 dimension-style families
    ARCHITECTURAL = "architectural"
    ENGINEERING = "engineering"
    CIVIL = "civil"
    ANGULAR = "angular"
    RADIAL = "radial"


class DimUnit(StrEnum):
    MILLIMETRE = "mm"
    METRE = "m"
    DEGREE = "deg"


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
    unit: DimUnit

    def override(self, scale: ScaleRatio, text: Option[TextHeight] = Nothing) -> "frozendict[str, DimVar]":
        s = scale.factor
        blk, tick = _TERMINATOR[self.terminator]
        factor, suffix, linear_unit = _DIM_UNIT[self.unit]
        base: dict[str, DimVar] = {
            "dimtxt": text.default_value(self.height).mm / s,
            "dimasz": self.arrow_size / s,
            "dimexe": self.extension / s,
            "dimexo": self.offset / s,
            "dimgap": self.gap / s,
            "dimdli": self.baseline / s,
            "dimcen": self.arrow_size / (2.0 * s),
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
            "dimlunit": linear_unit,
            "dimlfac": factor,
            "dimdec": self.decimals,
            "dimdsep": ord("."),  # $DIMDSEP is group-code 70: ezdxf stores the separator as its int char code, never the str
            "dimzin": 8,
            "dimpost": suffix,
            "dimalt": 0,
            "dimaltf": 25.4,
            "dimaltd": 2,
            "dimapost": "",
            "dimscale": 1.0,
            "dimtxsty": "ISO-3098",
            "dimtol": 0,
        }
        return frozendict({**base, **({"dimblk": blk} if blk else {})})


class SwatchSpec(Struct, frozen=True):
    # the entity-free legend projection — the resolved fill beside its DXF poché/grade sRGB; a schedule
    # legend or keyplan key draws this value without ever holding a live Drawing.
    fill: HatchFill
    poche: Option[tuple[int, int, int]] = Nothing
    grade: Option[tuple[int, int, int]] = Nothing


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
            if (layer_name := name.compose()) not in doc.layers:
                doc.layers.add(
                    layer_name,
                    color=aci,
                    true_color=_colors.rgb2int(_colors.aci2rgb(aci)),
                    linetype=pen.linetype.value,
                    lineweight=round(pen.lineweight.mm * 100),
                )
            else:
                # a name match is not a seed proof — an existing layer reconciles to the standard pen and colour,
                # so a drifted import never survives under a standard name.
                held = doc.layers.get(layer_name)
                held.color = aci
                held.rgb = _colors.aci2rgb(aci)
                held.dxf.linetype = pen.linetype.value
                held.dxf.lineweight = round(pen.lineweight.mm * 100)
        for family in families:
            # every override re-asserts on the existing style as on the fresh one — seeding converges the
            # document to the Standard instead of trusting the name.
            dimstyle = doc.dimstyles.add(family.value) if family.value not in doc.dimstyles else doc.dimstyles.get(family.value)
            for key, value in _DIMSTYLE[family].override(self.scale).items():
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

    def dimstyle(self, family: DimStyleFamily, scale: Option[ScaleRatio] = Nothing) -> "frozendict[str, DimVar]":
        return _DIMSTYLE[family].override(scale.default_value(self.scale))

    def hatch(self, entity: "Hatch", material: HatchMaterial, factor: Option[float] = Nothing) -> "Result[Hatch, PatternFault]":
        # Exemption: the ezdxf Hatch fill setters mutate the native entity in place — one total dispatch over HATCH_BIND.
        fill: HatchFill = HATCH_BIND[material]
        f = factor.default_value(self.scale.factor)
        match fill:
            case HatchFill(tag="pattern", pattern=spec):
                return lowered(PatternOp.Dxf(spec, f)).bind(lambda result: self._apply_pattern(entity, material, result))
            case HatchFill(tag="solid"):
                entity.set_solid_fill(color=_FILL[material][0])
                return Ok(entity)
            case HatchFill(tag="gradient", gradient=(_stops, rotation)):
                dark, light = _FILL[material]
                entity.set_gradient(
                    color1=_colors.RGB(*_colors.aci2rgb(dark)),
                    color2=_colors.RGB(*_colors.aci2rgb(light)),
                    rotation=rotation,
                )
                return Ok(entity)
            case _ as unreachable:
                assert_never(unreachable)

    @staticmethod
    def _apply_pattern(entity: "Hatch", material: HatchMaterial, result: PatternResult) -> "Result[Hatch, PatternFault]":
        match result:
            case PatternResult(tag="dxf", dxf=rows):
                entity.set_pattern_fill(material.value, definition=[list(row) for row in rows])
                return Ok(entity)
            case PatternResult(tag="svg"):
                return Error(PatternFault(unlowerable=("svg", "ezdxf.Hatch")))
            case PatternResult(tag="geometry"):
                return Error(PatternFault(unlowerable=("geometry", "ezdxf.Hatch")))
            case _ as unreachable:
                assert_never(unreachable)

    def swatch(self, material: HatchMaterial) -> SwatchSpec:
        # the entity-free legend twin of hatch(): one resolved value per material, drawn by schedule/keyplan legends.
        codes = _FILL.try_find(material)
        return SwatchSpec(
            fill=HATCH_BIND[material],
            poche=codes.map(lambda pair: tuple(_colors.aci2rgb(pair[0]))),
            grade=codes.map(lambda pair: tuple(_colors.aci2rgb(pair[1]))),
        )

    def rgb(self, layer: LayerName) -> tuple[int, int, int]:
        # discipline ACI pen sRGB; a CMYK/spectral swatch routes regime's LCh value through derive.
        return tuple(_colors.aci2rgb(_ACI[layer.discipline]))

    def paper_factor(self, model_units: Option[int] = Nothing) -> float:
        # paper mm per model unit: ISO 5455 factor over the model->mm conversion.
        unit = model_units.default_value(_units.MM)
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
# DXF fill codes per material (solid poché ACI, gradient light ACI) — exactly the non-pattern HATCH_BIND rows.
_FILL: Final[Map[HatchMaterial, tuple[int, int]]] = Map.of_seq([
    (HatchMaterial.STEEL, (250, 250)),
    (HatchMaterial.EARTH, (43, 8)),
])
# ISO 129-1 family base parameters (mm at 1:1).
_DIMSTYLE: Final[Map[DimStyleFamily, DimStyleFamilyRow]] = Map.of_seq([
    (DimStyleFamily.ARCHITECTURAL, DimStyleFamilyRow(Terminator.OBLIQUE_STROKE, TextHeight.H2_5, 2.5, 1.25, 0.625, 0.625, 7.0, 0, DimUnit.MILLIMETRE)),
    (DimStyleFamily.ENGINEERING, DimStyleFamilyRow(Terminator.FILLED_ARROW, TextHeight.H3_5, 3.5, 1.25, 0.625, 0.9, 8.0, 1, DimUnit.MILLIMETRE)),
    (DimStyleFamily.CIVIL, DimStyleFamilyRow(Terminator.FILLED_ARROW, TextHeight.H5, 5.0, 2.0, 1.0, 1.5, 10.0, 2, DimUnit.METRE)),
    (DimStyleFamily.ANGULAR, DimStyleFamilyRow(Terminator.FILLED_ARROW, TextHeight.H3_5, 3.5, 1.25, 0.625, 0.9, 8.0, 1, DimUnit.DEGREE)),
    (DimStyleFamily.RADIAL, DimStyleFamilyRow(Terminator.FILLED_ARROW, TextHeight.H3_5, 3.5, 1.25, 0.625, 0.9, 8.0, 1, DimUnit.MILLIMETRE)),
])
_DIM_UNIT: Final[Map[DimUnit, tuple[float, str, int]]] = Map.of_seq([
    (DimUnit.MILLIMETRE, (1.0, "", 2)),
    (DimUnit.METRE, (0.001, " m", 2)),
    (DimUnit.DEGREE, (1.0, "°", 2)),
])
# ISO 129-1 termination -> (verified ezdxf.ARROWS block name, tick factor); empty block + tick>0 = oblique tick.
_TERMINATOR: Final[Map[Terminator, tuple[str, float]]] = Map.of_seq([
    (Terminator.FILLED_ARROW, ("", 0.0)),
    (Terminator.OPEN_ARROW, ("OPEN", 0.0)),
    (Terminator.OBLIQUE_STROKE, ("", 2.5)),
    (Terminator.DOT, ("DOT", 0.0)),
    (Terminator.ORIGIN_INDICATION, ("ORIGIN", 0.0)),
    (Terminator.NONE, ("NONE", 0.0)),
])

# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "DimStyleFamily",
    "DimUnit",
    "DimVar",
    "Standard",
    "SwatchSpec",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
