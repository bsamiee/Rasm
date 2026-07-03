# [PY_ARTIFACTS_DRAWING_STANDARD]

The root of the drawing-production plane: the closed owned-vocabulary substrate every other `drawing/` producer (`dimension`/`annotate`/`symbol`/`detail`/`schedule`) composes, structurally a peer of `visualization/diagram/glyphset#GLYPHSET` — a pure vocabulary owner that mints NO receipt of its own and contributes NO `core/plan#PLAN` node. It authors the AEC drafting standards to their exact published cardinalities as closed families — ISO 128 line types and line-width groups, ISO 128-50 material section-hatch patterns, ISO 5455 drawing scales, the ISO 13567 + AIA CAD-Layer-Guidelines + NCS discipline/major/minor/status layer-name codec, the NCS/US-National-CAD-Standard discipline+sheet-type sheet-identification codec, ISO 3098 lettering heights and the full type-A/B lettering geometry, and the ISO 129-1 dimension-style families — and LOWERS each onto the real `ezdxf` symbol-table resource it names, so a drawing producer draws onto a `Drawing` already seeded with the standard's linetypes, textstyles, layers, dimstyles, insertion units, and section-hatch definitions rather than re-deriving a drafting convention per figure.

`Standard` is ONE lowering owner over the closed vocabulary set. `of(scale, lettering, font)` admits the profile ONCE — the base `ScaleRatio`, the `LetteringStyle`, and the optional lettering font whose ISO 3098 cap-height metric it binds through `fonttools` `TTFont` at admission so no projection re-reads the disk. `seed(doc)` authors every ISO resource onto an `ezdxf.Drawing` in one imperative table-builder fold (the mm insertion unit, the linetype dash arrays, the ISO-3098 textstyle, the ISO-3098 multileader style `annotate` composes by name, the discipline-penned layers carrying their true-color, the ISO 129-1 dimstyles); `graphics(layer)` projects the ONE uniform `GfxAttribs` bundle — pen, linetype, weight, and the ISO 13567 status-screen transparency — every downstream `add_*` entity carries as its `dxfattribs=`; `dimstyle(family, scale)` derives the DIM-variable override `dimension#DIMENSION` lowers onto `add_*_dim`; `hatch(material)` projects the `HatchSpec` whose `apply(hatch)` dispatches the ISO 128-50 fill regime — a scaled `ezdxf.tools.pattern` pattern, a solid steel poche, or a graded earth fill; `lettering(height)` resolves the ISO 3098 drawn geometry (pen thickness, baseline pitch, char/word spacing, and the cap-corrected TTF outline point size) the text producers read; `rgb(layer)` resolves the discipline pen's sRGB through `ezdxf.colors.aci2rgb` — the seam a CMYK/spectral swatch routes through `graphic/color/derive#DERIVE`; and `paper_factor(model_units)` anchors the ISO 5455 ratio to real drawing units through `ezdxf.units.conversion_factor`. Never a per-entity `set_layer`/`set_color` setter, never a bare `StrEnum` name with no lowering to a real DXF resource, never a dead vocabulary table with no projection, and never a 2-of-8 line-type slice where the full ISO cardinality is the design claim. The vocabularies are closed and total: a discipline (`Discipline`), a section material (`HatchMaterial`), a sheet type (`SheetType`), or a dimension family (`DimStyleFamily`) selects WHICH resource the fold authors and WHICH pen an entity draws with, never a new primitive kind.

The layer palette is an owned discipline-to-ACI correspondence lowered through `ezdxf` `colors.aci2rgb`/`colors.rgb2int` to the layer's true-color — NOT a spectral notation conversion, because NCS and RAL are proprietary atlas notations no colorimetry library parses; a swatch that must reach CMYK or a spectral space routes the discipline's resolved sRGB through `graphic/color/derive#DERIVE`, this owner holding only the ISO 13567/AIA/NCS discipline/sheet-type/status CODE structure and its ACI pen assignment. The ISO 3098 lettering height is bound to the referenced font's real cap-height metric so the drawn nominal height matches the standard rather than the font's arbitrary em-square, the metric an `Option` whose absent branch (a stroke SHX font, a missing file, a metric-less face) falls to the ISO 3098 nominal cascade so no font ever fails the substrate. This page carries no async, no offload, no rail, and no receipt — the vocabularies are total and the lowering is a synchronous builder fold the consuming producer runs inside its OWN offload seam, exactly as `glyphset` is the synchronous vocabulary the `layout`/`draw` producers compose.

## [01]-[INDEX]

- [01]-[STANDARD]: the closed AEC-drafting owned-vocabulary substrate + its `ezdxf` symbol-table lowering — the ISO 128 `LineType` (the full ISO 128-2:2020 Table 1 15 basic line types → `_LINETYPE` dash/dot/gap arrays) and `LineWeight` (the R10 0.13-2.0 mm cascade → DXF `1/100 mm` lineweight, its `.group` deriving the ISO 128-20 two-width pen pair), the ISO 128-50 `HatchMaterial` → `HatchSpec` section-fill table over the closed `HatchFill` regime (→ `HatchSpec.apply(hatch)` dispatching `Hatch.set_pattern_fill` + the `ezdxf.tools.pattern` scaled definition, `set_solid_fill` steel poche, or `set_gradient` graded earth), the ISO 5455 `ScaleRatio` reduction/full/enlargement vocabulary (→ `.ratio`/`.factor` + the `ezdxf.units.conversion_factor` unit anchor), the ISO 13567/AIA/NCS `LayerName` codec over `Discipline`/`Status` (→ one composed AIA string + one `Layer` carrying the discipline ACI/true-color/linetype/lineweight) and the NCS `SheetId` codec over `Discipline`/`SheetType` (→ the `"A-201"` sheet identifier `composition/sheet#SHEET` assembles), the ISO 3098 `TextHeight` cascade + `LetteringStyle` type-A/B `LetteringRow` stroke/width/spacing geometry bound to the real font cap-height via `fonttools` `TTFont` into the `LetteringMetric` the text producers read (→ `Textstyle`), the ISO 129-1 `DimStyleFamily`/`Terminator` families deriving the `DimStyleSpec` DIM-variable override (→ `DimStyle`), and the `Standard` owner whose `of` admits the font metric once and whose `seed`/`graphics`/`dimstyle`/`hatch`/`rgb`/`lettering`/`paper_factor` projections lower the vocabulary onto the real `ezdxf` resources — the substrate READ by every drawing producer, minting no receipt and no plan node.

## [02]-[STANDARD]

- Owner: `Standard` the one drawing-standard lowering owner, a frozen `Struct` carrying the resolved profile (the base `ScaleRatio`, the `LetteringStyle`, the optional lettering font path, and the `FontMetric` `Option` admitted once at `of`) and exposing seven pure projections over the closed vocabularies — `seed(doc)` the imperative table-builder fold authoring every ISO resource onto an `ezdxf.Drawing` (the mm insertion unit, linetypes → `doc.linetypes.add`, the ISO-3098 textstyle → `doc.styles.add`, the ISO-3098 multileader style → `doc.mleader_styles.new`, discipline layers → `doc.layers.add`, dimstyles → `doc.dimstyles.add`), `graphics(layer)` projecting one `GfxAttribs(layer, color, rgb, linetype, lineweight, ltscale, transparency)` — the status-screen transparency riding the uniform pen — the whole `add_*` builder family carries as its `dxfattribs=`, `dimstyle(family, scale)` deriving the `DimStyleSpec` override the dimension producer threads into `add_*_dim`'s `override=`, `hatch(material)` projecting the `HatchSpec` whose `apply(hatch)` dispatches the pattern / solid-poche / graded fill regime, `rgb(layer)` resolving the discipline pen's sRGB (the `graphic/color/derive#DERIVE` seam), `lettering(height)` resolving the ISO 3098 drawn geometry, and `paper_factor(model_units)` anchoring the ISO 5455 ratio to real units — never a per-entity attribute setter, never a second lowering surface per resource kind, never a re-implemented DXF tag writer where `ezdxf`'s symbol-table `.add` owns the resource, and never a dead vocabulary table with no projection lowering it. The closed vocabularies are module-level `StrEnum` owners and their derived-once `frozendict` correspondences: `LineType` the ISO 128 line-type family, `LineWeight` the ISO 128-20 R10 width group, `HatchMaterial` the ISO 128-50 section material, `ScaleRatio` the ISO 5455 scale, `Discipline`/`Status`/`SheetType` the ISO 13567/AIA/NCS code sets, `TextHeight`/`LetteringStyle` the ISO 3098 lettering axes, and `DimStyleFamily`/`Terminator` the ISO 129-1 dimension-style axes. `LayerName`/`SheetId`/`DimStyleSpec`/`HatchSpec`/`FontMetric`/`LetteringMetric` are the composite frozen value objects the vocabularies fold into. `ezdxf` owns the DXF resource model (the `Drawing`, the `Linetype`/`Textstyle`/`Layer`/`DimStyle` symbol-table entries, the `Hatch.set_pattern_fill` pattern fill and the `ezdxf.tools.pattern` `load`/`scale_pattern` definitions, the `colors.RGB`/`aci2rgb`/`rgb2int` true-color conversion, the `units.MM`/`conversion_factor`/`InsertUnits` anchor, and the `GfxAttribs` value object); `fonttools` owns the `TTFont` binary cap-height read; no drawing-standard library exists, so the ISO vocabularies and their published cardinalities are this owner's composition over the `ezdxf` resource surface, never a re-authored resource table.
- Cases: the closed vocabularies authored to exact published cardinality — `LineType` the full ISO 128-2:2020 Table 1 basic line-type set (`CONTINUOUS`/`DASHED`/`DASHED_SPACED`/`LONG_DASH_DOT`/`LONG_DASH_DOUBLE_DOT`/`LONG_DASH_TRIPLE_DOT`/`DOTTED`/`LONG_DASH_SHORT_DASH`/`LONG_DASH_DOUBLE_SHORT_DASH`/`DASH_DOT`/`DOUBLE_DASH_DOT`/`DASH_DOUBLE_DOT`/`DOUBLE_DASH_DOUBLE_DOT`/`DASH_TRIPLE_DOT`/`DOUBLE_DASH_TRIPLE_DOT`) each keyed in `_LINETYPE` to its `LineTypeRow(description, pattern)` where `pattern` is the `ezdxf` dash-length array (positive dash, negative gap, `0.0` dot) `doc.linetypes.add(name, pattern=, description=)` authors, `CONTINUOUS` alone carrying the empty pattern the built-in solid line uses; `LineWeight` the ISO 128-20 R10 cascade (`W013`/`W018`/`W025`/`W035`/`W050`/`W070`/`W100`/`W140`/`W200`) keyed in `_LINEWEIGHT` to the DXF `1/100 mm` integer, its `.group` property deriving the two-width line group (wide:narrow ≈ 2:1) as the wide pen plus its partner two R10 steps down; `HatchMaterial` the ISO 128-50 section family (`STEEL`/`CONCRETE`/`CONCRETE_REINFORCED`/`MASONRY`/`TIMBER_GRAIN`/`TIMBER_END`/`INSULATION_THERMAL`/`EARTH`/`HARDCORE`/`LIQUID`/`GLASS`) keyed in `_HATCH` to its `HatchSpec(pattern, scale, angle)` whose `definition()` resolves the `ezdxf.tools.pattern` scaled pattern lines; `ScaleRatio` the ISO 5455 vocabulary (the enlargement `E50`/`E20`/`E10`/`E5`/`E2`, the full `FULL`, the reduction `R2`/`R5`/`R10`/`R20`/`R50`/`R100`/`R200`/`R500`/`R1000`/`R2000`/`R5000`/`R10000`) each a `ScaleRow(ratio, factor)` in `_SCALE` where `ratio` is the printed `"1:100"` string `composition/sheet#SHEET`'s `Scale.ratio` draws and `factor` the paper-over-model float; `Discipline` the AIA CAD-Layer-Guidelines designator set (`A`/`C`/`E`/`F`/`G`/`H`/`I`/`L`/`M`/`P`/`Q`/`R`/`S`/`T`/`V`/`W`/`X`/`Z`) each keyed in `_DISCIPLINE` to its `DisciplineStyle(aci, linetype, lineweight)` pen assignment, `Status` the AIA layer-status set (`NEW`/`EXISTING`/`DEMOLISH`/`FUTURE`/`TEMPORARY`/`MOVED`/`RELOCATED`/`NOT_IN_CONTRACT`), `SheetType` the NCS/UDS sheet-type designators (`GENERAL`/`PLAN`/`ELEVATION`/`SECTION`/`LARGE_SCALE`/`DETAIL`/`SCHEDULE`/`USER_7`/`USER_8`/`THREE_D` = digits `0`-`9`); `TextHeight` the ISO 3098 nominal cascade (`H2_5`/`H3_5`/`H5`/`H7`/`H10`/`H14`/`H20`) keyed in `_TEXT_HEIGHT` to its mm float, `LetteringStyle` the ISO 3098 type (`TYPE_A` stroke `h/14`, `TYPE_B` stroke `h/10`) keyed in `_LETTERING` to its `LetteringRow(stroke, width, spacing, baseline, word)` full ratio geometry; `DimStyleFamily` the ISO 129-1 dimension-style family (`ARCHITECTURAL`/`ENGINEERING`/`CIVIL`/`ANGULAR`/`RADIAL`) keyed in `_DIMSTYLE` to its `DimStyleFamilyRow` base parameters, `Terminator` the ISO 129-1 line-termination family (`FILLED_ARROW`/`OPEN_ARROW`/`OBLIQUE_STROKE`/`DOT`/`NONE`) keyed in `_TERMINATOR` to its `(dimblk, tick_factor)` — a pure table read the consumers fold, matched by no `match` here.
- Entry: `Standard.of(scale=ScaleRatio.FULL, lettering=LetteringStyle.TYPE_B, font=Nothing)` is the one construction surface resolving the drawing profile AND admitting the font's cap-height metric once through `_read_metric`; `LayerName.of(discipline, major, minor="", status=Status.NEW)` composes one AIA-format layer name (`"A-WALL-FULL-N"`) and the discipline row that pens it, its `.compose()` the string a `Layer` name AND a `GfxAttribs.layer` carries; `SheetId.of(discipline, sheet_type, sequence=1)` composes the NCS sheet identifier (`"A-201"`) `composition/sheet#SHEET`'s sheet-number assembly consumes; `DimStyleSpec.of(family, scale, text)` derives the ISO 129-1 DIM-variable dict from the family row, the scale factor, and the `TextHeight`; `HatchSpec` and `LetteringMetric` are projected by `hatch`/`lettering`, never hand-built — the constructors the vocabularies fold into. There is no `Standard`-side render, no `_emit`, and no `contribute` — the substrate authors resources and projects attributes, its consumers own the render and the receipt.
- Auto: `of` binds the font metric ONCE — `font.bind(_read_metric)` opening the referenced `TTFont` at the admission boundary so `lettering` reads the stored `FontMetric` rather than re-reading the disk per glyph, a non-outline SHX font or a missing file folding to `Nothing`. `seed(doc)` folds every vocabulary onto the `ezdxf` document in one imperative table-builder pass — `doc.units = units.MM` anchors the ISO drafting unit, each non-`CONTINUOUS` `LineType` authors a `doc.linetypes.add(lt.value, pattern=list(row.pattern), description=row.description)`, the ISO 3098 lettering authors one `doc.styles.add("ISO-3098", font=self.font.default_value("isocp.shx"))` textstyle, each seeded `LayerName` authors one `doc.layers.add(name.compose(), color=row.aci, true_color=colors.rgb2int(colors.aci2rgb(row.aci)), linetype=row.linetype.value, lineweight=_LINEWEIGHT[row.lineweight])` so the layer carries both the ACI pen AND its resolved true-color, each `DimStyleFamily` authors one `doc.dimstyles.add(family.value)` whose `.dxf` DIM-variables are set from the `dimstyle(family)` override, and one `doc.mleader_styles.new("ISO-3098-MLEADER")` seeds the ISO 128-2 multileader style `drawing/annotate#ANNOTATE` composes by name (its `char_height` the ISO 3098 `TextHeight.H3_5.mm`, its `text_style_handle` linked to the seeded ISO-3098 textstyle, its `landing_gap_size`/`dogleg_length` the ISO landing geometry) rather than the `ezdxf` default `"Standard"` — the one place the standard mutates the native `ezdxf` builder, the platform-forced symbol-table authoring seam. `graphics(layer)` returns `GfxAttribs(layer=layer.compose(), color=pen.aci, rgb=self.rgb(layer), linetype=pen.linetype.value, lineweight=_LINEWEIGHT[pen.lineweight], ltscale=self.scale.factor, transparency=_STATUS_TRANSPARENCY[layer.status])` whose `.asdict()` every `add_*` builder splats, so a mark's whole pen — including the ISO 13567 status screen that plots an `EXISTING`/`DEMOLISH`/`FUTURE` layer at a derived transparency so the current-work layers read solid over it — is one projection off one discipline row plus one status row. `dimstyle(family, scale)` reads `_DIMSTYLE[family]`, derives the `DimStyleSpec` scaling the base parameters by the ISO 5455 factor (an architectural dimension at `1:50` scales its 2.5 mm text and arrow to paper), and returns the DIM-variable override dict — the terminator resolving `_TERMINATOR[row.terminator]` into the `(dimblk, dimtsz)` pair. `hatch(material)` returns `_HATCH[material]`, whose `HatchSpec.apply(hatch)` dispatches the closed `HatchFill` regime onto the section producer's `Hatch` — a scaled `ezdxf.tools.pattern.load`+`scale_pattern` `set_pattern_fill` definition (the ISO pattern lines ezdxf's OWN renderer draws), a `set_solid_fill` solid poche for structural steel in section, or a two-color `aci2rgb`-resolved `set_gradient` graded fill for earth/subgrade — so the producer composes `spec.apply(hatch)` and the fill regime stays owned here rather than a hardcoded `set_pattern_fill` for every material. `lettering(height)` folds `_LETTERING[self.lettering]` and the admitted `FontMetric` into one `LetteringMetric` — the ISO 3098 pen thickness (`stroke·h`), baseline pitch (`baseline·h`), char/word spacing, and the cap-corrected TTF outline `point_size` (falling to the nominal mm when no metric binds) the text producers read. `paper_factor(model_units)` returns `self.scale.factor * units.conversion_factor(model_units, units.MM)` so a model authored in metres draws at the ISO scale on the mm sheet.
- Packages: `ezdxf` (`ezdxf.new`/`Drawing` the document, `doc.units`/`units.MM`/`units.conversion_factor`/`InsertUnits` the unit anchor, `doc.linetypes.add`/`doc.styles.add`/`doc.layers.add`/`doc.dimstyles.add`/`doc.mleader_styles.new` the symbol-table authoring, `DimStyle.dxf.set`/`MLeaderStyle.dxf` the DIM/mleader-variable set, `Hatch.set_pattern_fill`/`set_solid_fill`/`set_gradient` + `tools.pattern.load`/`scale_pattern`/`ISO_PATTERN` the pattern/solid/gradient section-hatch lowering, `gfxattribs.GfxAttribs(transparency=)` the uniform attribute value object with the status screen, `colors.RGB`/`colors.aci2rgb`/`colors.rgb2int` the ACI→true-color conversion, `ARROWS` the terminator arrow-block names); `fonttools` (`ttLib.TTFont(path, lazy=True)`, `font["head"].unitsPerEm`, `font["OS/2"].sCapHeight` the cap-height metric bind, `TTLibError` the trapped read fault); `expression` (`Option`/`Some`/`Nothing`/`Option.bind`/`Option.map`/`Option.default_value` the font-metric absence axis, no `Result` — the vocabularies are total and the lowering imperative); `msgspec` (`Struct(frozen=True)` the value objects); `beartype` (`@beartype` on `LayerName.of`/`SheetId.of`/`DimStyleSpec.of` so a malformed token or out-of-cascade height is refused at construction); `builtins.frozendict` (the closed correspondence tables, each a single-edit-site primary the secondaries derive from). No `colour-science` — NCS/RAL are proprietary atlas notations no colorimetry library parses (verified: the `colour-science` surface is spectral/CIE/CAM/`convert`, with no NCS or RAL member), so the discipline color is an owned ACI assignment lowered through `ezdxf.colors`, and a downstream CMYK/spectral swatch routes the resolved sRGB through `graphic/color/derive#DERIVE`. No `kiwisolver`/`ziamath`/`ziafont` — those are the dimension/annotation/symbol PRODUCER surfaces the substrate is read by, not composed here.
- Growth: a new ISO 128 line type is one `LineType` member plus one `_LINETYPE` pattern row (the `seed` fold authors it with zero edit); a new line-width group is one `LineWeight` member plus one `_LINEWEIGHT` integer; a new section material is one `HatchMaterial` member plus one `_HATCH` `HatchSpec` row; a new drawing scale is one `ScaleRatio` member plus one `_SCALE` row; a new discipline is one `Discipline` member plus one `_DISCIPLINE` pen row (its layer color/true-color/linetype/weight authored for free), a new layer status one `Status` member, a new sheet type one `SheetType` member; a new lettering height is one `TextHeight` member plus one `_TEXT_HEIGHT` mm, a new lettering type one `LetteringStyle` member plus one `_LETTERING` `LetteringRow`; a new dimension-style family is one `DimStyleFamily` member plus one `_DIMSTYLE` row, a new line termination one `Terminator` member plus one `_TERMINATOR` row; a new layer-codec field (a phase code, a work-package suffix) is one `LayerName` field threaded through `.compose()`; a new pen attribute axis (the realized ISO 13567 status transparency, a future plot-style) is one `GfxAttribs` field on the one `graphics` projection plus one `_STATUS_TRANSPARENCY` row; a new section-fill regime is one `HatchFill` case plus one `HatchSpec.apply` arm; a new seeded resource (the ISO-3098 mleader style, a future table style) is one `seed` authoring block; a new lettering metric (an oblique slope, an x-height) is one `LetteringRow`/`FontMetric` field on the one `lettering` projection; a new DIM-variable is one `DimStyleSpec.override()` key; a downstream color space is the `graphic/color/derive#DERIVE` seam over the resolved sRGB, never a second color engine here; zero new surface for a new drawing producer — it reads the existing vocabularies and lowering.
- Boundary: no render (the substrate authors resources and projects attributes; `dimension`/`annotate`/`symbol` own the `add_*` geometry and the `SVGBackend`/`PyMuPdfBackend` render); no receipt and no `core/plan#PLAN` node (a pure vocabulary owner mints neither, exactly as `visualization/diagram/glyphset#GLYPHSET`); no async and no offload (the vocabularies are total and `seed` is a synchronous builder fold the consuming producer runs inside its OWN `to_thread` seam); no color conversion beyond the ACI→sRGB pen resolution (a discipline color's CMYK/spectral form is `graphic/color/derive#DERIVE`'s, and NCS/RAL notation parsing exists in no library, so the substrate holds only the ISO 13567/AIA/NCS discipline/sheet-type/status CODE structure and its owned ACI table); no IFC (the `csharp:Rasm.Bim` boundary — the drawing plane is a documentation projection, never the semantic model); no sheet placement, sheet-number assembly, or scale-fit (`composition/sheet#SHEET` owns the viewport and reads `ScaleRatio.ratio` and `SheetId.compose()`). A per-entity `set_layer`/`set_color` setter where the uniform `GfxAttribs` projection applies, a bare `StrEnum` name with no `ezdxf` resource lowering, a dead `_HATCH`/`_LETTERING`/`FontMetric` table no projection reads, a hardcoded per-arm dimension literal where `_DIMSTYLE` carries the family, a hand-rolled NCS→sRGB parser where no library owns the notation, a phantom `ezdxf.pattern` where the real module is `ezdxf.tools.pattern`, a `Result` rail over a total vocabulary read, a 2-of-N line-type or scale slice where the full ISO cardinality is authored, and a receipt or plan node on a pure substrate are the deleted forms — `Standard` is the one owned-vocabulary substrate the drawing plane composes, lowered to the real `ezdxf` symbol tables, minting nothing.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from typing import TYPE_CHECKING, Self, assert_never

from beartype import beartype
from builtins import frozendict
from expression import Nothing, Option, Some
from msgspec import Struct

lazy import ezdxf
lazy from ezdxf import colors as _colors
lazy from ezdxf import units as _units
lazy from ezdxf.gfxattribs import GfxAttribs
lazy from ezdxf.tools import pattern as _dxfpattern
lazy from fontTools.ttLib import TTFont, TTLibError

if TYPE_CHECKING:
    from ezdxf.document import Drawing
    from ezdxf.entities import Hatch

# --- [TYPES] ----------------------------------------------------------------------------
type Pattern = tuple[float, ...]  # ezdxf linetype dash array: +dash / -gap / 0.0 dot, drawing units (mm)


class LineType(StrEnum):  # ISO 128-2:2020 Table 1 — the full 15 basic line types; the member value IS the ezdxf linetype name
    CONTINUOUS = "CONTINUOUS"  # 01 continuous — visible edges
    DASHED = "ISO_DASHED"  # 02 dashed — hidden edges
    DASHED_SPACED = "ISO_DASHED_SPACED"  # 03 dashed spaced — hidden alt
    LONG_DASH_DOT = "ISO_LONG_DASH_DOT"  # 04 long-dashed dotted — centre / axis
    LONG_DASH_DOUBLE_DOT = "ISO_LONG_DASH_DOUBLE_DOT"  # 05 long-dashed double-dotted — phantom / adjacent
    LONG_DASH_TRIPLE_DOT = "ISO_LONG_DASH_TRIPLE_DOT"  # 06 long-dashed triple-dotted — special surface
    DOTTED = "ISO_DOTTED"  # 07 dotted — hidden detail
    LONG_DASH_SHORT_DASH = "ISO_LONG_DASH_SHORT_DASH"  # 08 long-dashed short-dashed — cutting-plane
    LONG_DASH_DOUBLE_SHORT_DASH = "ISO_LONG_DASH_DOUBLE_SHORT_DASH"  # 09 long-dashed double-short-dashed
    DASH_DOT = "ISO_DASH_DOT"  # 10 dashed dotted — chain / pitch / symmetry
    DOUBLE_DASH_DOT = "ISO_DOUBLE_DASH_DOT"  # 11 double-dashed dotted — movable-part outline
    DASH_DOUBLE_DOT = "ISO_DASH_DOUBLE_DOT"  # 12 dashed double-dotted
    DOUBLE_DASH_DOUBLE_DOT = "ISO_DOUBLE_DASH_DOUBLE_DOT"  # 13 double-dashed double-dotted
    DASH_TRIPLE_DOT = "ISO_DASH_TRIPLE_DOT"  # 14 dashed triple-dotted
    DOUBLE_DASH_TRIPLE_DOT = "ISO_DOUBLE_DASH_TRIPLE_DOT"  # 15 double-dashed triple-dotted

    @property
    def pattern(self) -> "Pattern":
        # the ISO 128-2 dash array (+dash / -gap / 0.0 dot, mm) the drawing/schedule#SCHEDULE line-type legend
        # swatch reads, the public projection of the private _LINETYPE table beside LineWeight.group / TextHeight.mm.
        return _LINETYPE[self].pattern


class LineWeight(StrEnum):  # ISO 128-20 R10 line-width cascade (mm) — value keys the DXF 1/100 mm int
    W013 = "0.13"
    W018 = "0.18"
    W025 = "0.25"
    W035 = "0.35"
    W050 = "0.50"
    W070 = "0.70"
    W100 = "1.00"
    W140 = "1.40"
    W200 = "2.00"

    @property
    def group(self) -> "tuple[LineWeight, LineWeight]":
        # the ISO 128-20 two-width line group (wide:narrow ≈ 2:1): self is the wide pen, the narrow its
        # partner two R10 steps down (0.50->0.25, 0.70->0.35, 1.00->0.50) — the drafting pen pair one row derives.
        order = tuple(LineWeight)
        return (self, order[max(order.index(self) - 2, 0)])


class HatchMaterial(StrEnum):  # ISO 128-50 section-hatch material indications
    STEEL = "steel"
    CONCRETE = "concrete"
    CONCRETE_REINFORCED = "concrete_reinforced"
    MASONRY = "masonry"
    TIMBER_GRAIN = "timber_grain"
    TIMBER_END = "timber_end"
    INSULATION_THERMAL = "insulation_thermal"
    EARTH = "earth"
    HARDCORE = "hardcore"
    LIQUID = "liquid"
    GLASS = "glass"


class HatchFill(StrEnum):  # the ISO 128-50 section-fill regime — the axis the pattern-only table could not express
    PATTERN = "pattern"  # a scaled ezdxf.tools.pattern definition (the ANSI/ISO hatch lines)
    SOLID = "solid"  # a solid poche (solid-black structural steel in section) — Hatch.set_solid_fill
    GRADIENT = "gradient"  # a two-color graded fill (earth / subgrade) — Hatch.set_gradient


class ScaleRatio(StrEnum):  # ISO 5455 recommended drawing scales — enlargement / full / reduction
    E50 = "50:1"
    E20 = "20:1"
    E10 = "10:1"
    E5 = "5:1"
    E2 = "2:1"
    FULL = "1:1"
    R2 = "1:2"
    R5 = "1:5"
    R10 = "1:10"
    R20 = "1:20"
    R50 = "1:50"
    R100 = "1:100"
    R200 = "1:200"
    R500 = "1:500"
    R1000 = "1:1000"
    R2000 = "1:2000"
    R5000 = "1:5000"
    R10000 = "1:10000"

    @property
    def ratio(self) -> str:  # the printed "1:100" string composition/sheet#SHEET's Scale.ratio draws
        return _SCALE[self].ratio

    @property
    def factor(self) -> float:  # paper-over-model — the viewport scale and the DIM-variable scaling read this
        return _SCALE[self].factor


class Discipline(StrEnum):  # AIA CAD-Layer-Guidelines / ISO 13567 discipline designators
    ARCHITECTURAL = "A"
    CIVIL = "C"
    ELECTRICAL = "E"
    FIRE = "F"
    GENERAL = "G"
    HAZMAT = "H"
    INTERIORS = "I"
    LANDSCAPE = "L"
    MECHANICAL = "M"
    PLUMBING = "P"
    EQUIPMENT = "Q"
    RESOURCE = "R"
    STRUCTURAL = "S"
    TELECOM = "T"
    SURVEY = "V"
    PROCESS = "W"
    OTHER = "X"
    CONTRACTOR = "Z"


class Status(StrEnum):  # AIA layer status field — the codec's trailing field
    NEW = "N"
    EXISTING = "E"
    DEMOLISH = "D"
    FUTURE = "F"
    TEMPORARY = "T"
    MOVED = "M"
    RELOCATED = "R"
    NOT_IN_CONTRACT = "X"


class SheetType(StrEnum):  # NCS / US National CAD Standard sheet-type designators — the 2nd char of a sheet id
    GENERAL = "0"  # general (symbols, notes, legends)
    PLAN = "1"  # plans (horizontal views)
    ELEVATION = "2"  # elevations (vertical exterior views)
    SECTION = "3"  # sections
    LARGE_SCALE = "4"  # large-scale views (plan / elevation enlargements)
    DETAIL = "5"  # details
    SCHEDULE = "6"  # schedules and diagrams
    USER_7 = "7"  # user defined
    USER_8 = "8"  # user defined
    THREE_D = "9"  # 3D representations (isometric / perspective)


class TextHeight(StrEnum):  # ISO 3098 nominal lettering height cascade (mm)
    H2_5 = "2.5"
    H3_5 = "3.5"
    H5 = "5.0"
    H7 = "7.0"
    H10 = "10.0"
    H14 = "14.0"
    H20 = "20.0"

    @property
    def mm(self) -> float:
        return _TEXT_HEIGHT[self]


class LetteringStyle(StrEnum):  # ISO 3098 lettering type — stroke and proportion of the height h, keying _LETTERING
    TYPE_A = "A"  # line thickness d = h/14, tighter proportions
    TYPE_B = "B"  # line thickness d = h/10 — the drawing-office default


class DimStyleFamily(StrEnum):  # ISO 129-1 dimension-style families dimension#DIMENSION lowers onto DimStyle
    ARCHITECTURAL = "architectural"  # oblique tick terminator, 1/50-typical, mm, 0 decimals
    ENGINEERING = "engineering"  # filled-arrow terminator, structural/mechanical, mm, 1 decimal
    CIVIL = "civil"  # filled-arrow, large-scale site, m, 2 decimals
    ANGULAR = "angular"  # arrow terminator, degree units
    RADIAL = "radial"  # arrow terminator, radius / diameter prefix


class Terminator(StrEnum):  # ISO 129-1 dimension-line terminations
    FILLED_ARROW = "filled_arrow"  # closed filled — mechanical / engineering
    OPEN_ARROW = "open_arrow"  # open 90 degrees
    OBLIQUE_STROKE = "oblique_stroke"  # 45-degree architectural tick (DIMTSZ)
    DOT = "dot"  # small filled circle
    NONE = "none"


# --- [MODELS] ---------------------------------------------------------------------------
class LineTypeRow(Struct, frozen=True):
    description: str
    pattern: Pattern


class ScaleRow(Struct, frozen=True):
    ratio: str  # "1:100"
    factor: float  # 0.01 — paper units per model unit


class DisciplineStyle(Struct, frozen=True):
    aci: int  # AutoCAD Color Index pen — the discipline's standard color
    linetype: LineType
    lineweight: LineWeight


class LetteringRow(Struct, frozen=True):
    # the full ISO 3098 lettering geometry, every ratio relative to the nominal cap height h — never a
    # bare stroke/width tuple: a text producer reads pen thickness, baseline pitch, and char/word spacing.
    stroke: float  # d/h — line thickness ratio (type A 1/14, type B 1/10)
    width: float  # w/h — nominal character width ratio (uppercase)
    spacing: float  # a/h — minimum inter-character spacing (2d)
    baseline: float  # b/h — minimum baseline spacing (type A 20d, type B 14d)
    word: float  # e/h — minimum word spacing (6d)


class DimStyleFamilyRow(Struct, frozen=True):
    terminator: Terminator
    height: TextHeight  # DIMTXT base
    arrow_size: float  # DIMASZ base (mm)
    extension: float  # DIMEXE — extension-line reach beyond the dim line
    offset: float  # DIMEXO — extension-line offset from the feature
    gap: float  # DIMGAP — gap around the dimension text
    baseline: float  # DIMDLI — baseline-dimension line increment
    decimals: int  # DIMDEC
    unit: str  # "mm" / "m" / "deg" — dimension#DIMENSION reads for the suffix / DIMLFAC decision


class HatchSpec(Struct, frozen=True):
    # the ISO 128-50 section-fill projection — `apply(hatch)` dispatches the fill REGIME onto a section producer's
    # ezdxf `Hatch` (a scaled pattern, a solid poche, or a two-color graded fill), so the producer composes
    # `spec.apply(hatch)` and the fill regime stays owned here rather than a hardcoded `set_pattern_fill` per material.
    pattern: str  # the ACAD/ISO pattern name ezdxf.tools.pattern.load carries (the PATTERN regime)
    scale: float = 1.0
    angle: float = 0.0
    fill: HatchFill = HatchFill.PATTERN
    fill_color: int = 7  # the SOLID poche ACI pen
    gradient: tuple[int, int] = (250, 8)  # the GRADIENT (dark, light) ACI pair, aci2rgb-resolved for set_gradient

    def definition(self) -> list:
        # the ezdxf pattern-line definition, scaled — so ezdxf's OWN renderer draws the fill rather than
        # deferring the pattern name to a foreign CAD app; the 172-pattern table builds lazily on first use.
        base = _dxfpattern.load(factor=1.0).get(self.pattern)
        return _dxfpattern.scale_pattern(base, factor=self.scale, angle=self.angle) if base else []

    def apply(self, hatch: "Hatch") -> "Hatch":
        # Exemption: the ezdxf Hatch fill setters mutate the native entity in place — the platform-forced fill seam;
        # one total dispatch over the closed HatchFill regime the section producer composes as `spec.apply(hatch)`.
        match self.fill:
            case HatchFill.SOLID:
                hatch.set_solid_fill(color=self.fill_color)
            case HatchFill.GRADIENT:
                hatch.set_gradient(
                    color1=_colors.RGB(*_colors.aci2rgb(self.gradient[0])),
                    color2=_colors.RGB(*_colors.aci2rgb(self.gradient[1])),
                    rotation=self.angle,
                )
            case HatchFill.PATTERN:
                hatch.set_pattern_fill(self.pattern, scale=self.scale, angle=self.angle, definition=self.definition())
            case _ as unreachable:
                assert_never(unreachable)
        return hatch


class FontMetric(Struct, frozen=True):
    # the ISO 3098 cap-height binding — cap_height / units_per_em is the fraction of the em the drawn
    # capital occupies, so a TextHeight nominal mm resolves to the point size whose drawn cap = the nominal.
    units_per_em: int
    cap_height: int

    @property
    def cap_fraction(self) -> float:
        return self.cap_height / self.units_per_em if self.units_per_em else 0.7

    def point_size(self, nominal_mm: float, /) -> float:
        # ISO 3098 nominal height is the CAP height; the em point size scales it by the em/cap ratio.
        return nominal_mm / self.cap_fraction

    @staticmethod
    def bind(font: TTFont, /) -> "Option[FontMetric]":
        os2, head = font.get("OS/2"), font.get("head")
        cap = getattr(os2, "sCapHeight", 0) if os2 is not None else 0
        return Some(FontMetric(units_per_em=head.unitsPerEm, cap_height=cap)) if head is not None and cap else Nothing


class LetteringMetric(Struct, frozen=True):
    # the resolved ISO 3098 drawn geometry at one nominal height — the value object the dimension/annotate/
    # symbol text producers read, never three parallel projections: the DXF add_text uses `height`, the text
    # pen its `pen` lineweight, multi-line advance its `pitch`, and the ziafont/SVG outliner its `point_size`.
    height: float  # nominal cap height (mm) == TextHeight.mm — the DXF add_text height
    pen: float  # ISO 3098 line thickness d = stroke·h — the text entity's lineweight
    char_width: float  # nominal glyph advance w = width·h — the text-extent estimate the producers box against
    pitch: float  # minimum baseline spacing b = baseline·h — multi-line advance
    char_spacing: float  # minimum inter-character spacing a
    word_spacing: float  # minimum word spacing e
    point_size: float  # TTF em point size whose cap == h (ziafont/SVG outline); == h when no metric binds


class LayerName(Struct, frozen=True):
    discipline: Discipline
    major: str  # 4-char functional group — WALL / DOOR / DIMS / GRID
    minor: str = ""  # 4-char sub-group — FULL / PATT / IDEN
    status: Status = Status.NEW

    @classmethod
    @beartype
    def of(cls, discipline: Discipline, major: str, minor: str = "", status: Status = Status.NEW) -> Self:
        return cls(discipline=discipline, major=major.upper()[:4], minor=minor.upper()[:4], status=status)

    def compose(self) -> str:
        # the AIA long-format layer name: Discipline-Major[-Minor]-Status ("A-WALL-FULL-N"), a plain identifier.
        parts = (self.discipline.value, self.major, *((self.minor,) if self.minor else ()), self.status.value)
        return "-".join(parts)

    @property
    def pen(self) -> DisciplineStyle:
        return _DISCIPLINE[self.discipline]


class SheetId(Struct, frozen=True):
    # the NCS / US National CAD Standard sheet identifier — discipline letter + sheet-type digit + sequence
    # ("A-201" = architectural, elevations, 01). composition/sheet#SHEET's sheet-number assembly composes this.
    discipline: Discipline
    sheet_type: SheetType
    sequence: int = 1

    @classmethod
    @beartype
    def of(cls, discipline: Discipline, sheet_type: SheetType, sequence: int = 1) -> Self:
        return cls(discipline=discipline, sheet_type=sheet_type, sequence=sequence)

    def compose(self) -> str:
        return f"{self.discipline.value}-{self.sheet_type.value}{self.sequence:02d}"


class DimStyleSpec(Struct, frozen=True):
    # the ISO 129-1 DIM-variable override dimension#DIMENSION threads into add_*_dim(override=). Derived from
    # the family base row scaled by the ISO 5455 factor, so a 1:50 architectural dimension draws its 2.5 mm
    # text and tick at paper scale rather than a per-arm literal.
    family: DimStyleFamily
    scale: ScaleRatio
    text: TextHeight

    @classmethod
    @beartype
    def of(cls, family: DimStyleFamily, scale: ScaleRatio = ScaleRatio.FULL, text: TextHeight | None = None) -> Self:
        return cls(family=family, scale=scale, text=text or _DIMSTYLE[family].height)

    def override(self) -> "frozendict[str, object]":
        row, s = _DIMSTYLE[self.family], self.scale.factor
        blk, tick = _TERMINATOR[row.terminator]
        base: dict[str, object] = {
            "dimtxt": self.text.mm / s,
            "dimasz": row.arrow_size / s,
            "dimexe": row.extension / s,
            "dimexo": row.offset / s,
            "dimgap": row.gap / s,
            "dimdli": row.baseline / s,
            "dimdec": row.decimals,
            "dimtsz": tick / s,
            "dimtad": 1,
            "dimtih": 0,
            "dimtoh": 0,
            "dimscale": 1.0,
            "dimtxsty": "ISO-3098",
            "dimlunit": 2,
            "dimzin": 8,
            "dimtol": 0,
        }
        return frozendict({**base, **({"dimblk": blk} if blk else {})})


class Standard(Struct, frozen=True):
    scale: ScaleRatio = ScaleRatio.FULL
    lettering: LetteringStyle = LetteringStyle.TYPE_B
    font: Option[str] = Nothing  # the Textstyle font (shx/ttf); Nothing -> the ezdxf isocp default
    metric: Option[FontMetric] = Nothing  # the ISO 3098 cap-height, admitted ONCE at `of` from the font

    @classmethod
    def of(cls, scale: ScaleRatio = ScaleRatio.FULL, lettering: LetteringStyle = LetteringStyle.TYPE_B, font: Option[str] = Nothing) -> Self:
        return cls(scale=scale, lettering=lettering, font=font, metric=font.bind(_read_metric))

    def seed(self, doc: "Drawing", layers: tuple[LayerName, ...] = (), families: tuple[DimStyleFamily, ...] = ()) -> "Drawing":
        # Exemption: the ezdxf symbol-table builder mutates the native Drawing in place — the platform-forced
        # resource-authoring seam; one imperative fold over the closed vocabularies, no per-resource method family.
        doc.units = _units.MM  # ISO drafting anchor: the ISO 5455 factor is dimensionless over the mm insertion unit
        for lt in LineType:
            if lt is not LineType.CONTINUOUS and lt.value not in doc.linetypes:
                row = _LINETYPE[lt]
                doc.linetypes.add(lt.value, pattern=list(row.pattern), description=row.description)
        if "ISO-3098" not in doc.styles:
            doc.styles.add("ISO-3098", font=self.font.default_value("isocp.shx"))
        if not doc.mleader_styles.has_entry("ISO-3098-MLEADER"):
            # the ISO 128-2 multileader style drawing/annotate composes by NAME (add_multileader_mtext("ISO-3098-MLEADER"))
            # rather than the ezdxf default "Standard" — ISO 3098 char height, seeded lettering face, ISO landing/dogleg.
            mleader = doc.mleader_styles.new("ISO-3098-MLEADER")
            mleader.dxf.text_style_handle = doc.styles.get("ISO-3098").dxf.handle
            mleader.dxf.char_height = TextHeight.H3_5.mm
            mleader.dxf.landing_gap_size = 0.9
            mleader.dxf.dogleg_length = 4.0
        for name in layers:
            pen = name.pen
            doc.layers.add(
                name.compose(),
                color=pen.aci,
                true_color=_colors.rgb2int(_colors.aci2rgb(pen.aci)),
                linetype=pen.linetype.value,
                lineweight=_LINEWEIGHT[pen.lineweight],
            )
        for family in families:
            dimstyle = doc.dimstyles.add(family.value)
            for key, value in self.dimstyle(family).items():
                dimstyle.dxf.set(key, value)
        return doc

    def graphics(self, layer: LayerName) -> GfxAttribs:
        # the ONE uniform attribute value object every downstream add_* entity carries as dxfattribs=; the ISO 13567
        # status screens an EXISTING/DEMOLISH/FUTURE layer through one derived transparency, never a per-entity set.
        pen = layer.pen
        return GfxAttribs(
            layer=layer.compose(),
            color=pen.aci,
            rgb=self.rgb(layer),
            linetype=pen.linetype.value,
            lineweight=_LINEWEIGHT[pen.lineweight],
            ltscale=self.scale.factor,
            transparency=_STATUS_TRANSPARENCY[layer.status],
        )

    def dimstyle(self, family: DimStyleFamily, scale: ScaleRatio | None = None) -> "frozendict[str, object]":
        return DimStyleSpec.of(family, scale or self.scale).override()

    def hatch(self, material: HatchMaterial) -> HatchSpec:
        return _HATCH[material]

    def rgb(self, layer: LayerName) -> tuple[int, int, int]:
        # the discipline pen's sRGB — the seam a CMYK/spectral swatch routes through graphic/color/derive#DERIVE;
        # colors.aci2rgb returns an RGB namedtuple GfxAttribs.rgb and the derive Convert both read as a tuple.
        return tuple(_colors.aci2rgb(layer.pen.aci))

    def lettering(self, height: TextHeight) -> LetteringMetric:
        row, h = _LETTERING[self.lettering], height.mm
        point = self.metric.map(lambda m: m.point_size(h)).default_value(h)
        return LetteringMetric(
            height=h,
            pen=row.stroke * h,
            char_width=row.width * h,
            pitch=row.baseline * h,
            char_spacing=row.spacing * h,
            word_spacing=row.word * h,
            point_size=point,
        )

    def paper_factor(self, model_units: int | None = None) -> float:
        # paper mm per model unit: the ISO 5455 scale factor over the model->mm unit conversion, so a model
        # authored in metres draws at the ISO scale on the mm sheet. Composes ezdxf.units.conversion_factor.
        unit = model_units if model_units is not None else _units.MM
        return self.scale.factor * _units.conversion_factor(unit, _units.MM)


# --- [OPERATIONS] -----------------------------------------------------------------------
def _read_metric(path: str, /) -> "Option[FontMetric]":
    # admit the referenced font's ISO 3098 cap-height metric ONCE; a non-outline (SHX) font, a missing file,
    # or a metric-less face is ABSENCE (the nominal cascade governs), never a fault on this total substrate.
    # Exemption: the fonttools TTFont open is the platform-forced file-read boundary; its raise is trapped to Nothing.
    if not path.lower().endswith((".ttf", ".otf", ".ttc", ".woff", ".woff2")):
        return Nothing
    try:
        return FontMetric.bind(TTFont(path, lazy=True))
    except OSError, TTLibError:
        return Nothing


# --- [TABLES] ---------------------------------------------------------------------------
# ISO 128-2 dash arrays (mm at 1:1) — +dash / -gap / 0.0 dot; the single edit site per line type.
_LINETYPE: frozendict[LineType, LineTypeRow] = frozendict({
    LineType.CONTINUOUS: LineTypeRow("ISO full line", ()),
    LineType.DASHED: LineTypeRow("ISO dashed __ __ __", (15.0, 12.0, -3.0)),
    LineType.DASHED_SPACED: LineTypeRow("ISO dashed spaced __  __  __", (18.0, 12.0, -6.0)),
    LineType.LONG_DASH_DOT: LineTypeRow("ISO long-dash dot ____ . ____", (36.0, 24.0, -3.0, 0.0, -3.0)),
    LineType.LONG_DASH_DOUBLE_DOT: LineTypeRow("ISO long-dash double-dot ____ .. ____", (42.0, 24.0, -3.0, 0.0, -3.0, 0.0, -3.0)),
    LineType.LONG_DASH_TRIPLE_DOT: LineTypeRow("ISO long-dash triple-dot ____ ... ____", (48.0, 24.0, -3.0, 0.0, -3.0, 0.0, -3.0, 0.0, -3.0)),
    LineType.DOTTED: LineTypeRow("ISO dotted . . .", (3.0, 0.0, -3.0)),
    LineType.LONG_DASH_SHORT_DASH: LineTypeRow("ISO long-dash short-dash ____ __ ____", (33.0, 24.0, -3.0, 6.0, -3.0)),
    LineType.LONG_DASH_DOUBLE_SHORT_DASH: LineTypeRow("ISO long-dash double-short-dash ____ __ __ ____", (45.0, 24.0, -3.0, 6.0, -3.0, 6.0, -3.0)),
    LineType.DASH_DOT: LineTypeRow("ISO dash-dot __ . __ .", (18.0, 12.0, -3.0, 0.0, -3.0)),
    LineType.DOUBLE_DASH_DOT: LineTypeRow("ISO double-dash dot __ __ . __ __", (30.0, 12.0, -3.0, 12.0, -3.0, 0.0, -3.0)),
    LineType.DASH_DOUBLE_DOT: LineTypeRow("ISO dash double-dot __ . . __ . .", (21.0, 12.0, -3.0, 0.0, -3.0, 0.0, -3.0)),
    LineType.DOUBLE_DASH_DOUBLE_DOT: LineTypeRow("ISO double-dash double-dot __ __ . . __ __", (36.0, 12.0, -3.0, 12.0, -3.0, 0.0, -3.0, 0.0, -3.0)),
    LineType.DASH_TRIPLE_DOT: LineTypeRow("ISO dash triple-dot __ . . . __", (24.0, 12.0, -3.0, 0.0, -3.0, 0.0, -3.0, 0.0, -3.0)),
    LineType.DOUBLE_DASH_TRIPLE_DOT: LineTypeRow(
        "ISO double-dash triple-dot __ __ . . . __ __", (39.0, 12.0, -3.0, 12.0, -3.0, 0.0, -3.0, 0.0, -3.0, 0.0, -3.0)
    ),
})
# ISO 128-20 R10 cascade -> DXF lineweight (1/100 mm). Derived: the int is round(mm * 100).
_LINEWEIGHT: frozendict[LineWeight, int] = frozendict({w: round(float(w.value) * 100) for w in LineWeight})
# ISO 128-50 material section patterns -> HatchSpec(pattern, scale, angle); the pattern name resolves a real
# scaled definition through ezdxf.tools.pattern.load, so ezdxf's own renderer draws the section fill.
_HATCH: frozendict[HatchMaterial, HatchSpec] = frozendict({
    HatchMaterial.STEEL: HatchSpec("ANSI31", 1.0, 45.0, fill=HatchFill.SOLID),  # structural steel in section reads as a solid poche
    HatchMaterial.CONCRETE: HatchSpec("AR-CONC", 0.04, 0.0),
    HatchMaterial.CONCRETE_REINFORCED: HatchSpec("ANSI33", 1.0, 45.0),
    HatchMaterial.MASONRY: HatchSpec("ANSI32", 1.0, 45.0),
    HatchMaterial.TIMBER_GRAIN: HatchSpec("AR-HBONE", 0.02, 0.0),
    HatchMaterial.TIMBER_END: HatchSpec("ANSI37", 1.0, 0.0),
    HatchMaterial.INSULATION_THERMAL: HatchSpec("INSUL", 1.0, 0.0),
    HatchMaterial.EARTH: HatchSpec("EARTH", 1.0, 45.0, fill=HatchFill.GRADIENT, gradient=(43, 8)),  # earth/subgrade as a graded fill
    HatchMaterial.HARDCORE: HatchSpec("GRAVEL", 1.0, 0.0),
    HatchMaterial.LIQUID: HatchSpec("ANSI37", 2.0, 0.0),
    HatchMaterial.GLASS: HatchSpec("ANSI31", 4.0, 135.0),
})
# ISO 5455 scale -> ScaleRow(printed ratio, paper/model factor); factor derived once from the ratio arithmetic.
_SCALE: frozendict[ScaleRatio, ScaleRow] = frozendict({
    s: ScaleRow(s.value, (lambda a, b: a / b)(*(float(p) for p in s.value.split(":")))) for s in ScaleRatio
})
# AIA/ISO 13567 discipline -> pen (ACI color, ISO 128 linetype, ISO 128 lineweight). The owned color table no
# NCS/RAL parser replaces; a CMYK/spectral swatch routes the ACI-resolved sRGB through graphic/color/derive.
_DISCIPLINE: frozendict[Discipline, DisciplineStyle] = frozendict({
    Discipline.ARCHITECTURAL: DisciplineStyle(7, LineType.CONTINUOUS, LineWeight.W025),
    Discipline.CIVIL: DisciplineStyle(3, LineType.CONTINUOUS, LineWeight.W035),
    Discipline.ELECTRICAL: DisciplineStyle(6, LineType.DASHED, LineWeight.W025),
    Discipline.FIRE: DisciplineStyle(1, LineType.DASH_DOT, LineWeight.W035),
    Discipline.GENERAL: DisciplineStyle(8, LineType.CONTINUOUS, LineWeight.W018),
    Discipline.HAZMAT: DisciplineStyle(30, LineType.DASH_DOT, LineWeight.W035),
    Discipline.INTERIORS: DisciplineStyle(4, LineType.CONTINUOUS, LineWeight.W018),
    Discipline.LANDSCAPE: DisciplineStyle(94, LineType.LONG_DASH_DOT, LineWeight.W025),
    Discipline.MECHANICAL: DisciplineStyle(5, LineType.DASHED, LineWeight.W035),
    Discipline.PLUMBING: DisciplineStyle(140, LineType.DASHED_SPACED, LineWeight.W035),
    Discipline.EQUIPMENT: DisciplineStyle(2, LineType.CONTINUOUS, LineWeight.W025),
    Discipline.RESOURCE: DisciplineStyle(9, LineType.CONTINUOUS, LineWeight.W013),
    Discipline.STRUCTURAL: DisciplineStyle(1, LineType.CONTINUOUS, LineWeight.W050),
    Discipline.TELECOM: DisciplineStyle(150, LineType.DOTTED, LineWeight.W025),
    Discipline.SURVEY: DisciplineStyle(52, LineType.LONG_DASH_DOT, LineWeight.W018),
    Discipline.PROCESS: DisciplineStyle(40, LineType.CONTINUOUS, LineWeight.W035),
    Discipline.OTHER: DisciplineStyle(250, LineType.CONTINUOUS, LineWeight.W018),
    Discipline.CONTRACTOR: DisciplineStyle(253, LineType.DASHED, LineWeight.W018),
})
# AIA/ISO 13567 layer status -> the screened-plot transparency (0.0 opaque .. 1.0 clear); EXISTING/DEMOLISH/FUTURE
# plot screened so the current-work (NEW) layers read solid over them. `None` keeps the layer fully opaque.
_STATUS_TRANSPARENCY: frozendict[Status, float | None] = frozendict({
    Status.NEW: None,
    Status.EXISTING: 0.55,
    Status.DEMOLISH: 0.7,
    Status.FUTURE: 0.4,
    Status.TEMPORARY: 0.35,
    Status.MOVED: 0.5,
    Status.RELOCATED: 0.5,
    Status.NOT_IN_CONTRACT: 0.6,
})
# ISO 3098 nominal height cascade (mm).
_TEXT_HEIGHT: frozendict[TextHeight, float] = frozendict({h: float(h.value) for h in TextHeight})
# ISO 3098 lettering type -> the full LetteringRow geometry (stroke d/h, width w/h, char a/h, baseline b/h, word e/h).
_LETTERING: frozendict[LetteringStyle, LetteringRow] = frozendict({
    LetteringStyle.TYPE_A: LetteringRow(stroke=1.0 / 14.0, width=12.0 / 14.0, spacing=2.0 / 14.0, baseline=20.0 / 14.0, word=6.0 / 14.0),
    LetteringStyle.TYPE_B: LetteringRow(stroke=1.0 / 10.0, width=7.0 / 10.0, spacing=2.0 / 10.0, baseline=14.0 / 10.0, word=6.0 / 10.0),
})
# ISO 129-1 dimension-style family base parameters (mm, at 1:1).
_DIMSTYLE: frozendict[DimStyleFamily, DimStyleFamilyRow] = frozendict({
    DimStyleFamily.ARCHITECTURAL: DimStyleFamilyRow(Terminator.OBLIQUE_STROKE, TextHeight.H2_5, 2.5, 1.25, 0.625, 0.625, 7.0, 0, "mm"),
    DimStyleFamily.ENGINEERING: DimStyleFamilyRow(Terminator.FILLED_ARROW, TextHeight.H3_5, 3.5, 1.25, 0.625, 0.9, 8.0, 1, "mm"),
    DimStyleFamily.CIVIL: DimStyleFamilyRow(Terminator.FILLED_ARROW, TextHeight.H5, 5.0, 2.0, 1.0, 1.5, 10.0, 2, "m"),
    DimStyleFamily.ANGULAR: DimStyleFamilyRow(Terminator.FILLED_ARROW, TextHeight.H3_5, 3.5, 1.25, 0.625, 0.9, 8.0, 1, "deg"),
    DimStyleFamily.RADIAL: DimStyleFamilyRow(Terminator.FILLED_ARROW, TextHeight.H3_5, 3.5, 1.25, 0.625, 0.9, 8.0, 1, "mm"),
})
# ISO 129-1 line termination -> (DXF arrow-block name from ezdxf.ARROWS, tick factor). Empty dimblk + tick>0
# forces the oblique architectural tick (DIMTSZ); a filled arrow carries the closed-filled default (empty, 0).
_TERMINATOR: frozendict[Terminator, tuple[str, float]] = frozendict({
    Terminator.FILLED_ARROW: ("", 0.0),
    Terminator.OPEN_ARROW: ("OPEN", 0.0),
    Terminator.OBLIQUE_STROKE: ("", 2.5),
    Terminator.DOT: ("DOT", 0.0),
    Terminator.NONE: ("NONE", 0.0),
})

# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "DimStyleFamily",
    "DimStyleSpec",
    "Discipline",
    "FontMetric",
    "HatchFill",
    "HatchMaterial",
    "HatchSpec",
    "LayerName",
    "LetteringMetric",
    "LetteringStyle",
    "LineType",
    "LineWeight",
    "ScaleRatio",
    "SheetId",
    "SheetType",
    "Standard",
    "Status",
    "Terminator",
    "TextHeight",
]
```

`Standard` lowers the closed AEC-drafting vocabularies onto the real `ezdxf` resource model so the drawing plane draws against a document already seeded with the ISO standard rather than a per-figure convention: `of` admits the lettering font's ISO 3098 cap-height metric once through `fonttools` `TTFont`; the ISO 128 `LineType` family authors its dash-array `Linetype` entries through `doc.linetypes.add`, the ISO 3098 lettering authors one `ISO-3098` `Textstyle`, each seeded `LayerName` authors one `Layer` whose ACI pen, resolved true-color (`colors.rgb2int(colors.aci2rgb(aci))`), ISO 128 linetype, and ISO 128 line-width group resolve from its `Discipline` row, each `DimStyleFamily` authors one `DimStyle` whose ISO 129-1 DIM-variables are the `DimStyleSpec` override scaled by the ISO 5455 `ScaleRatio.factor`, and `doc.units = units.MM` anchors the drafting unit. The `graphics(layer)` projection is the one `GfxAttribs` bundle (layer, ACI color, true-color `rgb`, linetype, weight, ltscale, and the ISO 13567 status-screen `transparency`) the whole `add_*` builder family carries, `hatch(material)` the `HatchSpec` whose `apply(hatch)` dispatches the ISO 128-50 pattern / solid-poche / graded fill regime, `rgb(layer)` the `graphic/color/derive#DERIVE` sRGB seam, and `lettering(height)` the ISO 3098 drawn geometry the text producers read — so a mark's layer, color, linetype, weight, hatch, and text all trace to one owned vocabulary row rather than a per-mark literal. The substrate the `dimension`/`annotate`/`symbol`/`detail`/`schedule` producers read, minting no receipt, no plan node, and no render of its own.

## [03]-[RESEARCH]

- [NCS_NOTATION_PHANTOM] [RESOLVED]: the discipline layer color is an OWNED ACI pen assignment (`_DISCIPLINE`) lowered through `ezdxf` `colors.aci2rgb`/`colors.rgb2int` to the layer's true-color, NOT a `colour-science` NCS/RAL notation conversion — verified against the `colour-science` `.api` catalog, whose surface is spectral distributions, the `convert` model-pair gateway over CIE/RGB/Lab/CAM spaces, `delta_E`, CCT, and CCTF, with NO NCS or RAL atlas-notation parser (both are proprietary commercial notations no OSS colorimetry library owns). The reading-map claim "NCS is a spectral notation colour-science owns the convert for" is a phantom the rebuild refuses; the ISO 13567/AIA/NCS contribution is the discipline/sheet-type/status CODE structure and the pen table, and a discipline color's CMYK/spectral form routes the ACI-resolved sRGB through `graphic/color/derive#DERIVE`'s `Convert` op, never a re-authored notation parser. Justified on PACKAGE (the verified `colour-science` surface carries no NCS member) and DOMAIN (the layer codec owns discipline codes + an ACI pen convention, not a spectral atlas).
- [HATCH_PATTERN_LOWERING] [RESOLVED]: the reading-map `ezdxf.pattern` is a PHANTOM — the module does not exist; the real surface is `ezdxf.tools.pattern` (verified: `load(measurement=1, factor=None)` returns the 172-entry pattern dict incl. `ANSI31`/`ANSI32`/`ANSI33`/`ANSI37`/`AR-CONC`/`AR-HBONE`/`EARTH`/`GRAVEL`/`INSUL`, `scale_pattern(pattern, factor, angle)` scales one, `ISO_PATTERN` the metric set). `HatchMaterial` lowers to a `HatchSpec` whose `definition()` composes `load` + `scale_pattern` so `Hatch.set_pattern_fill(name, scale=, angle=, definition=)` (verified signature) draws a real scaled ISO section fill through ezdxf's own renderer rather than deferring a bare pattern name. This closes the reading-map `add_hatch`/`ezdxf.pattern` underutilization concretely. Justified on PACKAGE (the verified `ezdxf.tools.pattern`/`set_pattern_fill` surface) and DOMAIN (ISO 128-50 section-hatch material indications).
- [FONT_METRIC_BIND] [RESOLVED]: `Standard.of` admits the lettering font's cap-height metric ONCE through `_read_metric` (the `TTFont(path, lazy=True)` open guarded to `Nothing` on a non-outline SHX font, a missing file `OSError`, or a `fonttools` `TTLibError`), `FontMetric.bind` reading `TTFont["head"].unitsPerEm` and `TTFont["OS/2"].sCapHeight` (all verified members) so `lettering(height)` resolves the ISO 3098 nominal cap-height to the `point_size` whose drawn capital equals the standard without re-reading the disk — the `Option` `Nothing` branch falling to the nominal cascade, absence not failure on a total substrate. Justified on PACKAGE (`fonttools` `TTFont` metric read) and DOMAIN (ISO 3098 lettering height is a cap-height, not an em-square, nominal) and CONSUMER (the `symbol`/`annotate` SVG outliner reads `LetteringMetric.point_size` rather than hardcoding an em scale).
- [LETTERING_GEOMETRY] [RESOLVED]: `_LETTERING` carries the FULL ISO 3098 `LetteringRow` geometry (stroke `d/h`, width `w/h`, char spacing `a/h`, baseline `b/h`, word `e/h`) rather than a dead stroke/width tuple, and `lettering(height)` folds it with the admitted `FontMetric` into one `LetteringMetric` (pen thickness, baseline pitch, char/word spacing, TTF outline point size) the dimension/annotate/symbol text producers read — the DXF `Textstyle` keeps its font reference (isocp.shx already encodes the ISO proportions, so no width-factor distortion), and the drawn pen thickness/spacing are the `LetteringMetric` the text entities apply. Justified on DOMAIN (ISO 3098 defines the full lettering geometry, not just stroke + width) and CONSUMER (the text-pen lineweight and multi-line pitch the producers need).
- [SHEET_TYPE_CODEC] [RESOLVED]: `SheetType` (the NCS/UDS `0`-`9` sheet-type designators the brief `[06]` names) and the `SheetId` codec (`"A-201"` = discipline + sheet-type + zero-padded sequence) close the "discipline/sheet-type/status codes" vocabulary the layer codec alone did not carry — the sheet-type is part of the SHEET IDENTIFICATION (sheet number), not the layer name, so it is a standalone code set `composition/sheet#SHEET`'s sheet-number assembly consumes rather than a `LayerName` field. Justified on DOMAIN (the NCS sheet-type designator set) and CONSUMER (the `composition/sheet#SHEET` sheet-number assembly the brief `[04]` extends).
- [UNITS_AND_COLOR_ANCHOR] [RESOLVED]: `seed` sets `doc.units = units.MM` (verified `units.MM`/`InsertUnits.Millimeters` = 4) so the ISO 5455 factor is dimensionless over the mm insertion unit, and `paper_factor(model_units)` composes `units.conversion_factor(model_units, units.MM)` (verified) so a model authored in metres draws at the ISO scale on the mm sheet — closing the reading-map `ezdxf.units`/`InsertUnits` underutilization. `graphics(layer)` and `rgb(layer)` resolve the discipline pen's true-color through `colors.aci2rgb` (verified `aci2rgb(1)` = `RGB(255,0,0)`) so the `GfxAttribs.rgb` and the `graphic/color/derive#DERIVE` seam read the same sRGB. Justified on PACKAGE (the verified `ezdxf.units`/`colors` surface) and CONSUMER (the derive color seam and the metric drawing anchor).
- [DIMSTYLE_LOWERING] [RESOLVED]: `_DIMSTYLE`/`DimStyleSpec.override()` derive the ISO 129-1 DIM-variable dict (`dimtxt`/`dimasz`/`dimexe`/`dimexo`/`dimgap`/`dimdli`/`dimdec`/`dimtsz`/`dimtad`/`dimtxsty`/`dimlunit`/`dimzin`/`dimtol`) scaled by the ISO 5455 factor, the terminator resolving `_TERMINATOR` into the `(dimblk, dimtsz)` pair (the `dimblk` values the verified `ezdxf.ARROWS` block names `''`/`OPEN`/`DOT`/`NONE`, the oblique tick via empty dimblk + `dimtsz`), `seed` setting each through the verified `DimStyle.dxf.set(key, value)` — so `dimension#DIMENSION` threads the base override rather than a per-arm literal, `dimtol=0` leaving the per-dimension tolerance to the dimension producer. Justified on PACKAGE (the verified `doc.dimstyles.add`/`DimStyle.dxf.set`/`ARROWS` surface) and DOMAIN (the ISO 129-1 dimension-style families).
- [LINETYPE_FULL_CARDINALITY] [RESOLVED]: `LineType` authors the FULL ISO 128-2:2020 Table 1 fifteen basic line types (`01` continuous through `15` double-dashed triple-dotted), not the prior ten-of-fifteen slice the page's own "never a 2-of-8 line-type slice" claim condemned — the five that were missing (`09` long-dashed double-short-dashed, `12` dashed double-dotted, `13` double-dashed double-dotted, `14` dashed triple-dotted, `15` double-dashed triple-dotted) each land as one `LineType` member plus one `_LINETYPE` `LineTypeRow` dash-array row exactly as the Growth law legislates, and the prior mislabeled `09`/`10` comments are corrected to the ISO `10` dashed-dotted / `11` double-dashed-dotted numbering. Each new pattern is a `+dash / -gap / 0.0 dot` array whose leading total equals the sum of element lengths in the page's established `long-dash 24 / dash 12 / short-dash 6 / gap 3 / dot 0` convention, verified to author cleanly through `doc.linetypes.add(name, pattern=, description=)` on `ezdxf` 1.4.4 (all five names — the longest `ISO_LONG_DASH_DOUBLE_SHORT_DASH` at 31 chars — accepted on R2018). Justified on DOMAIN (ISO 128-2:2020 Table 1 is a fifteen-type closed family, the "exact published cardinality" this substrate claims) and PACKAGE (the `doc.linetypes.add` custom-pattern authoring, since `ezdxf setup=True` ships no `ISO_*` linetype — the substrate authors its own dash arrays rather than assuming a built-in).
- [STATUS_TRANSPARENCY] [RESOLVED]: the one `graphics(layer)` `GfxAttribs` projection omitted the transparency axis, yet ISO 13567 status is conventionally shown SCREENED — an `EXISTING`/`DEMOLISH`/`FUTURE` layer plots at reduced opacity so the current-work (`NEW`) layers read solid over it. `graphics` now derives `transparency=_STATUS_TRANSPARENCY[layer.status]` from one closed `Status`-keyed row on the uniform pen bundle rather than a per-entity set — the exact "a new attribute axis is one `GfxAttribs` field on the one projection" growth the page already named, now realized. Verified: `GfxAttribs(transparency=0.5)` accepts a `0.0..1.0` float (encoded to the DXF transparency int) and `transparency=None` (the `NEW`/opaque row) omits the attribute. Justified on PACKAGE (the verified `GfxAttribs(transparency=)` field) and DOMAIN (ISO 13567 status screening).
- [HATCH_FILL_REGIME] [RESOLVED]: the `_HATCH` table was PATTERN-only and could not express the two other ISO 128-50 section indicators — a solid poche (solid-black structural steel in section) and a two-color graded fill (earth/subgrade). `HatchSpec` grows the closed `HatchFill` regime (`PATTERN`/`SOLID`/`GRADIENT`) plus `fill_color`/`gradient` fields and one total `apply(hatch)` dispatch composing `Hatch.set_pattern_fill` (the scaled `ezdxf.tools.pattern` definition), `Hatch.set_solid_fill(color=)`, or `Hatch.set_gradient(color1, color2, rotation)` — the fill knowledge stays owned here, the section producer composes `spec.apply(hatch)` rather than hardcoding `set_pattern_fill` for every material; `STEEL` moves to `SOLID` (the poche) and `EARTH` to `GRADIENT`. Verified: `set_solid_fill(color=7, style=1, rgb=None)`, `set_gradient(color1=RGB, color2=RGB, rotation=0.0, …)` (the `aci2rgb`-resolved `RGB` pair), and `paths.add_polyline_path(is_closed=True)` all present on the `Hatch` entity. Justified on PACKAGE (the verified `set_solid_fill`/`set_gradient` surface) and DOMAIN (ISO 128-50 admits solid + graded section indicators, not pattern alone).
- [MLEADER_STYLE_SEED] [RESOLVED]: `seed` authored linetypes/textstyles/layers/dimstyles but no multileader style, so `drawing/annotate#ANNOTATE`'s `add_multileader_mtext`/`add_multileader_block` composed the `ezdxf` default `"Standard"` mleader style (non-ISO lettering, default landing). `seed` now authors one `doc.mleader_styles.new("ISO-3098-MLEADER")` — `char_height=TextHeight.H3_5.mm`, `text_style_handle` linked to the seeded ISO-3098 textstyle handle, ISO `landing_gap_size`/`dogleg_length` — so `annotate` composes the seeded ISO style by name and the DXF multileaders carry the ISO 3098 lettering face. Verified: `doc.mleader_styles.new(name)` exists (with `has_entry` guard) and `char_height`/`landing_gap_size`/`dogleg_length`/`text_style_handle` are settable `MLEADERSTYLE` attributes; `doc.tablestyles` does NOT exist on `ezdxf 1.4.4`, so the reading-map table-style seed is correctly dropped as a phantom. Justified on PACKAGE (the verified `doc.mleader_styles.new` surface) and CONSUMER (`drawing/annotate#ANNOTATE` composes the named ISO mleader style rather than the `ezdxf` default). Cross-file: `annotate.md` should compose `add_multileader_mtext("ISO-3098-MLEADER")` in place of `"Standard"`.
