# [PY_ARTIFACTS_DRAWING_REGIME]

The closed AEC drafting VOCABULARY and BIND substrate — the s1 floor the drawing, composition, specification, and delivery planes key. Every drafting code set stands here at its exact published cardinality as a closed family: the ISO 128-2 `LineType` fifteen, the ISO 128-20 `LineWeight` R10 cascade with its two-width group law, the ISO 128-50 `HatchMaterial` section family, the ISO 5455 `ScaleRatio` enlargement/full/reduction set with its derived paper-over-model factor, the AIA/ISO 13567/NCS `Discipline`/`Status`/`SheetType` code sets, the ISO 3098 `TextHeight` cascade and `LetteringStyle` type-A/B ratio geometry, and the ISO 129-1 `Terminator` line-termination family. The codecs compose the code sets into identifiers: `LayerName` carries the full ISO 13567 field structure (agent, element, presentation, status, sector, phase) beside the bounded AIA major/minor form and the NCS level-2 form, one owner projecting all three schemas; `SheetId` composes the NCS discipline+sheet-type+sequence identifier. The derivations compute what a frozen table cannot keep true: `paper()` derives every ISO 216 A/B trim size by aspect-preserving halving from the series seed, and `ScaleRatio.factor` derives each factor from its ratio arithmetic. The BIND rows resolve vocabulary onto values through the owning substrates — `HATCH_BIND` maps each `HatchMaterial` onto a `graphic/vector/pattern#PATTERN` `HatchFill` (preset geometry, density law, poché and grade stops as resolved values), `PENS` maps each `Discipline` onto its LCh pen coordinates plus ISO 128 linetype/weight (a consumer needing sRGB/CMYK routes the coordinates through `graphic/color/derive#DERIVE` by value), and `lettering()` folds the ISO 3098 ratio row with the `typography/font#FONT` `FaceMetrics` cap-height into the drawn `LetteringMetric` — never a raw font-binary read here.

NO `ezdxf` crosses this page: the symbol-table lowering (seed, graphics, dimstyle, hatch entity application, ACI pens, paper factor over insertion units) is `drawing/standard#STANDARD`'s, composing these rows downward from s3. `graphic/style#STYLE` SELECTS pen/lettering rows by key per office theme; `graphic/layer#LAYER` reads the ISO 13567 field vocabulary for its AEC layer names; `composition/sheet#SHEET` reads `ScaleRatio`/`SheetId` and the ISO 216 derivation; `specification/classify#CLASSIFY` and `delivery/register#REGISTER` compose `Discipline`; the drawing producers read every family. This page mints no receipt, contributes no plan node, and runs no async — the vocabularies are total, the binds are data, and the derivations are pure arithmetic.

## [01]-[INDEX]

- [01]-[REGIME]: the closed drafting vocabulary (`LineType`/`LineWeight`/`HatchMaterial`/`ScaleRatio`/`Discipline`/`Status`/`SheetType`/`TextHeight`/`LetteringStyle`/`Terminator`), the three-schema `LayerName` codec and the `SheetId` codec, the ISO 216 halving and ISO 5455 ratio derivations, and the BIND rows (`HATCH_BIND` material→fill over the pattern plane, `PENS` discipline→LCh pen, `lettering()` over the font-metrics value) — imported downward by style, layer, sheet, classify, register, every drawing producer, and standard's lowering; importing only derive, pattern, and the font-metrics value object; minting nothing.

## [02]-[REGIME]

- Owner: one vocabulary-and-bind floor, zero lowering. Every `StrEnum` family is closed at its published cardinality and every correspondence is one `Map` row set with a single edit site: `_LINETYPE` the ISO 128-2 dash arrays (positive dash, negative gap, `0.0` dot, mm at 1:1), `_TEXT_HEIGHT` the nominal mm cascade, `_LETTERING` the full `LetteringRow(stroke, width, spacing, baseline, word)` type-A/B ratio geometry, `PENS` the `DisciplineStyle(lch, linetype, lineweight)` pen rows, `HATCH_BIND` the `HatchFill` rows composing pattern presets. A vocabulary member is a code; a bind row is the code's resolved value set; the lowering that mutates a host resource lives with the host owner.
- Codecs: `LayerName` is ONE owner over three published schemas — `compose(LayerSchema.AIA)` emits the bounded `Discipline-MAJOR[-MINOR]-Status` long format, `compose(LayerSchema.ISO13567)` emits the full mandatory+optional field string (two-char agent, six-char element, two-char presentation, one-char status, plus the sector/phase optional fields when present), `compose(LayerSchema.NCS)` emits the NCS level-2 form — the field superset lives on the one struct and each schema projects its slice, so a projector never re-derives a name grammar. `SheetId.of(discipline, sheet_type, sequence)` composes the NCS `"A-201"` identifier `composition/sheet#SHEET` assembles and `delivery/register#REGISTER` keys.
- Derivations: `paper(series, rank)` derives any ISO 216 trim size by floor-halving the series seed (`A0` 841×1189, `B0` 1000×1414) with the long-edge swap per rank — the sheet plane's frozen `_SIZES` table has no successor; `ScaleRatio.factor` derives paper-over-model from the printed ratio string; `LineWeight.group` derives the ISO 128-20 two-width pen pair (wide plus its partner two R10 steps down); `TextHeight.mm` reads the nominal cascade.
- Bind: `HATCH_BIND` rows are the material axis over the pattern plane's fill regime — `STEEL` a solid poché whose color VALUE arrives derive-resolved, `EARTH` a graded fill with resolved stop rows, every patterned material a `PatternSpec` preset selection at its density law — so `drawing/standard#STANDARD` lowers one row onto its `Hatch` entity, `drawing/schedule#SCHEDULE` draws one row as a legend swatch, and `export/layered#LAYERED` fills one row per layer, all from one bind. `PENS` rows carry the discipline pen as LCh coordinates plus ISO 128 linetype and weight; `lettering(style, height, metrics)` folds `_LETTERING` ratios with the optional `FaceMetrics` into one `LetteringMetric` (nominal cap height, pen thickness `stroke·h`, char advance, baseline pitch, char/word spacing, and the cap-corrected outline point size falling to the nominal when no metric binds) — the value the dimension/annotate/symbol/sheet text producers read.
- Growth: a new line type, weight, material, scale, discipline, status, sheet type, height, lettering type, or terminator is one member plus one row in its owning table; a new layer-name field is one `LayerName` field threaded through the schema projections; a new name schema is one `LayerSchema` member plus one `compose` arm; a new material fill is one `HATCH_BIND` row; a new pen axis is one `DisciplineStyle` field; a new paper series is one `_PAPER_SEED` row; zero new surface for a new consumer — it keys the existing families.
- Boundary: no `ezdxf` and no host resource authoring (`drawing/standard#STANDARD`); no pattern geometry generation (`graphic/vector/pattern#PATTERN` owns `StrokeFamily`/`PatternSpec`/`to_dxf`/`to_svg`/`to_geometry`); no color conversion (`graphic/color/derive#DERIVE` owns the model algebra — pen rows carry coordinates, never a conversion arm); no font-binary read (`typography/font#FONT` owns `FaceMetrics`; the fold here consumes the value); no theme selection (`graphic/style#STYLE` selects rows per office style); no sheet placement, no receipt, no plan node, no rail — the substrate is total.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from typing import Final, Self

from beartype import beartype
from expression import Nothing, Option
from expression.collections import Map
from msgspec import Struct

from rasm.artifacts.graphic.color.derive import ColorModel
from rasm.artifacts.graphic.vector.pattern import HatchFill, PRESETS, SectionPattern
from rasm.artifacts.typography.font import FaceMetrics

# --- [TYPES] ----------------------------------------------------------------------------
type Lch = tuple[float, float, float]  # CIE LCh(ab) pen coordinates — derive resolves sRGB/CMYK from the VALUE
type Pattern = tuple[float, ...]  # ISO 128-2 dash array: +dash / -gap / 0.0 dot, mm at 1:1


class LineType(StrEnum):  # ISO 128-2:2020 Table 1 — the full fifteen basic line types
    CONTINUOUS = "CONTINUOUS"
    DASHED = "ISO_DASHED"
    DASHED_SPACED = "ISO_DASHED_SPACED"
    LONG_DASH_DOT = "ISO_LONG_DASH_DOT"
    LONG_DASH_DOUBLE_DOT = "ISO_LONG_DASH_DOUBLE_DOT"
    LONG_DASH_TRIPLE_DOT = "ISO_LONG_DASH_TRIPLE_DOT"
    DOTTED = "ISO_DOTTED"
    LONG_DASH_SHORT_DASH = "ISO_LONG_DASH_SHORT_DASH"
    LONG_DASH_DOUBLE_SHORT_DASH = "ISO_LONG_DASH_DOUBLE_SHORT_DASH"
    DASH_DOT = "ISO_DASH_DOT"
    DOUBLE_DASH_DOT = "ISO_DOUBLE_DASH_DOT"
    DASH_DOUBLE_DOT = "ISO_DASH_DOUBLE_DOT"
    DOUBLE_DASH_DOUBLE_DOT = "ISO_DOUBLE_DASH_DOUBLE_DOT"
    DASH_TRIPLE_DOT = "ISO_DASH_TRIPLE_DOT"
    DOUBLE_DASH_TRIPLE_DOT = "ISO_DOUBLE_DASH_TRIPLE_DOT"

    @property
    def pattern(self) -> "Pattern":
        return _LINETYPE[self]


class LineWeight(StrEnum):  # ISO 128-20 R10 line-width cascade (mm)
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
    def mm(self) -> float:
        return float(self.value)

    @property
    def group(self) -> "tuple[LineWeight, LineWeight]":
        # ISO 128-20 two-width group (wide:narrow ≈ 2:1): self wide, partner two R10 steps down.
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


class ScaleRatio(StrEnum):  # ISO 5455 recommended drawing scales
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
    def ratio(self) -> str:  # the printed "1:100" string the title block and dimension suffix draw
        return self.value

    @property
    def factor(self) -> float:  # paper-over-model — derived from the ratio arithmetic, never a parallel table
        paper, model = (float(p) for p in self.value.split(":"))
        return paper / model


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


class Status(StrEnum):  # AIA / ISO 13567 layer-status field
    NEW = "N"
    EXISTING = "E"
    DEMOLISH = "D"
    FUTURE = "F"
    TEMPORARY = "T"
    MOVED = "M"
    RELOCATED = "R"
    NOT_IN_CONTRACT = "X"


class SheetType(StrEnum):  # NCS / UDS sheet-type designators — the 2nd char of a sheet id
    GENERAL = "0"
    PLAN = "1"
    ELEVATION = "2"
    SECTION = "3"
    LARGE_SCALE = "4"
    DETAIL = "5"
    SCHEDULE = "6"
    USER_7 = "7"
    USER_8 = "8"
    THREE_D = "9"


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
        return float(self.value)


class LetteringStyle(StrEnum):  # ISO 3098 lettering type — stroke and proportion ratios of the height h
    TYPE_A = "A"
    TYPE_B = "B"


class Terminator(StrEnum):  # ISO 129-1 dimension-line terminations — geometry is drawing/symbol's proportion rows
    FILLED_ARROW = "filled_arrow"
    OPEN_ARROW = "open_arrow"
    OBLIQUE_STROKE = "oblique_stroke"
    DOT = "dot"
    NONE = "none"


class LayerSchema(StrEnum):  # the published layer-name grammars the one LayerName owner projects
    AIA = "aia"
    ISO13567 = "iso13567"
    NCS = "ncs"


class PaperSeries(StrEnum):  # ISO 216 trim series — sizes derive by halving, never a frozen size table
    A = "A"
    B = "B"


# --- [MODELS] ---------------------------------------------------------------------------
class LetteringRow(Struct, frozen=True):
    # full ISO 3098 geometry, every ratio relative to nominal cap height h.
    stroke: float  # d/h — line thickness (type A 1/14, type B 1/10)
    width: float  # w/h — nominal character width
    spacing: float  # a/h — minimum inter-character spacing
    baseline: float  # b/h — minimum baseline spacing
    word: float  # e/h — minimum word spacing


class LetteringMetric(Struct, frozen=True):
    # the resolved ISO 3098 drawn geometry at one nominal height — the one value the text producers read.
    height: float  # nominal cap height (mm)
    pen: float  # line thickness d = stroke·h
    char_width: float  # nominal glyph advance w = width·h
    pitch: float  # baseline spacing b = baseline·h
    char_spacing: float
    word_spacing: float
    point_size: float  # em point size whose drawn cap == h; == h when no FaceMetrics binds


class DisciplineStyle(Struct, frozen=True):
    # the discipline pen: LCh coordinates (derive resolves any target model from the VALUE) + ISO 128 line row.
    lch: Lch
    linetype: LineType
    lineweight: LineWeight
    model: ColorModel = ColorModel.LCH


class LayerName(Struct, frozen=True):
    # the field superset of the three published grammars; each schema projects its slice via compose().
    discipline: Discipline
    major: str  # AIA 4-char functional group / ISO 13567 element head
    minor: str = ""  # AIA 4-char sub-group / ISO 13567 element tail
    status: Status = Status.NEW
    agent: str = "--"  # ISO 13567 responsible-agent field (2 chars, "--" = unassigned)
    presentation: str = "--"  # ISO 13567 presentation field (2 chars)
    sector: str = ""  # ISO 13567 optional sector/zone field
    phase: str = ""  # ISO 13567 optional phase field

    @classmethod
    @beartype
    def of(cls, discipline: Discipline, major: str, minor: str = "", status: Status = Status.NEW) -> Self:
        return cls(discipline=discipline, major=major.upper()[:4], minor=minor.upper()[:4], status=status)

    def compose(self, schema: LayerSchema = LayerSchema.AIA, /) -> str:
        element = f"{self.major:_<4}{self.minor:_<4}"[:6].rstrip("_") or "______"
        match schema:
            case LayerSchema.AIA:
                parts = (self.discipline.value, self.major, *((self.minor,) if self.minor else ()), self.status.value)
                return "-".join(parts)
            case LayerSchema.ISO13567:
                head = f"{self.agent:_<2}{self.discipline.value}{element:_<6}{self.presentation:_<2}{self.status.value}"
                return head + (f"{self.sector:_<4}" if self.sector else "") + (f"{self.phase:_<2}" if self.phase else "")
            case LayerSchema.NCS:
                return f"{self.discipline.value}-{self.major}" + (f"{self.minor}" if self.minor else "")

    @property
    def pen(self) -> DisciplineStyle:
        return PENS[self.discipline]


class SheetId(Struct, frozen=True):
    # NCS sheet identifier — discipline letter + sheet-type digit + sequence ("A-201").
    discipline: Discipline
    sheet_type: SheetType
    sequence: int = 1

    @classmethod
    @beartype
    def of(cls, discipline: Discipline, sheet_type: SheetType, sequence: int = 1) -> Self:
        return cls(discipline=discipline, sheet_type=sheet_type, sequence=sequence)

    def compose(self) -> str:
        return f"{self.discipline.value}-{self.sheet_type.value}{self.sequence:02d}"


# --- [OPERATIONS] -----------------------------------------------------------------------
def paper(series: PaperSeries, rank: int, /) -> tuple[float, float]:
    # ISO 216 derivation: floor-halve the portrait seed rank times — (width, height) trim mm.
    w, h = _PAPER_SEED[series]
    for _ in range(rank):
        w, h = h // 2, w
    return (float(w), float(h))


def lettering(style: LetteringStyle, height: TextHeight, metrics: Option[FaceMetrics] = Nothing, /) -> LetteringMetric:
    # the ONE lettering fold: ISO 3098 ratios × nominal height, cap-corrected through the font-metrics VALUE.
    row, h = _LETTERING[style], height.mm
    point = metrics.map(lambda m: m.point_size(h)).default_value(h)
    return LetteringMetric(
        height=h,
        pen=row.stroke * h,
        char_width=row.width * h,
        pitch=row.baseline * h,
        char_spacing=row.spacing * h,
        word_spacing=row.word * h,
        point_size=point,
    )


# --- [TABLES] ---------------------------------------------------------------------------
_PAPER_SEED: Final[Map[PaperSeries, tuple[int, int]]] = Map.of_seq([
    (PaperSeries.A, (841, 1189)),
    (PaperSeries.B, (1000, 1414)),
])
_LINETYPE: Final[Map[LineType, Pattern]] = Map.of_seq([
    (LineType.CONTINUOUS, ()),
    (LineType.DASHED, (15.0, 12.0, -3.0)),
    (LineType.DASHED_SPACED, (18.0, 12.0, -6.0)),
    (LineType.LONG_DASH_DOT, (36.0, 24.0, -3.0, 0.0, -3.0)),
    (LineType.LONG_DASH_DOUBLE_DOT, (42.0, 24.0, -3.0, 0.0, -3.0, 0.0, -3.0)),
    (LineType.LONG_DASH_TRIPLE_DOT, (48.0, 24.0, -3.0, 0.0, -3.0, 0.0, -3.0, 0.0, -3.0)),
    (LineType.DOTTED, (3.0, 0.0, -3.0)),
    (LineType.LONG_DASH_SHORT_DASH, (33.0, 24.0, -3.0, 6.0, -3.0)),
    (LineType.LONG_DASH_DOUBLE_SHORT_DASH, (45.0, 24.0, -3.0, 6.0, -3.0, 6.0, -3.0)),
    (LineType.DASH_DOT, (18.0, 12.0, -3.0, 0.0, -3.0)),
    (LineType.DOUBLE_DASH_DOT, (30.0, 12.0, -3.0, 12.0, -3.0, 0.0, -3.0)),
    (LineType.DASH_DOUBLE_DOT, (21.0, 12.0, -3.0, 0.0, -3.0, 0.0, -3.0)),
    (LineType.DOUBLE_DASH_DOUBLE_DOT, (36.0, 12.0, -3.0, 12.0, -3.0, 0.0, -3.0, 0.0, -3.0)),
    (LineType.DASH_TRIPLE_DOT, (24.0, 12.0, -3.0, 0.0, -3.0, 0.0, -3.0, 0.0, -3.0)),
    (LineType.DOUBLE_DASH_TRIPLE_DOT, (39.0, 12.0, -3.0, 12.0, -3.0, 0.0, -3.0, 0.0, -3.0, 0.0, -3.0)),
])
_LETTERING: Final[Map[LetteringStyle, LetteringRow]] = Map.of_seq([
    (LetteringStyle.TYPE_A, LetteringRow(stroke=1 / 14, width=12 / 14, spacing=2 / 14, baseline=20 / 14, word=6 / 14)),
    (LetteringStyle.TYPE_B, LetteringRow(stroke=1 / 10, width=7 / 10, spacing=2 / 10, baseline=14 / 10, word=6 / 10)),
])
# Discipline pen rows: LCh VALUE + ISO 128 line row. sRGB/CMYK resolve through derive from the value;
# the DXF ACI pen is drawing/standard's lowering correspondence, never a second color truth here.
PENS: Final[Map[Discipline, DisciplineStyle]] = Map.of_seq([
    (Discipline.ARCHITECTURAL, DisciplineStyle((20.0, 0.0, 0.0), LineType.CONTINUOUS, LineWeight.W025)),
    (Discipline.CIVIL, DisciplineStyle((55.0, 60.0, 135.0), LineType.CONTINUOUS, LineWeight.W035)),
    (Discipline.ELECTRICAL, DisciplineStyle((55.0, 65.0, 328.0), LineType.DASHED, LineWeight.W025)),
    (Discipline.FIRE, DisciplineStyle((50.0, 75.0, 30.0), LineType.DASH_DOT, LineWeight.W035)),
    (Discipline.GENERAL, DisciplineStyle((55.0, 0.0, 0.0), LineType.CONTINUOUS, LineWeight.W018)),
    (Discipline.HAZMAT, DisciplineStyle((60.0, 70.0, 55.0), LineType.DASH_DOT, LineWeight.W035)),
    (Discipline.INTERIORS, DisciplineStyle((70.0, 40.0, 195.0), LineType.CONTINUOUS, LineWeight.W018)),
    (Discipline.LANDSCAPE, DisciplineStyle((60.0, 55.0, 145.0), LineType.LONG_DASH_DOT, LineWeight.W025)),
    (Discipline.MECHANICAL, DisciplineStyle((45.0, 60.0, 280.0), LineType.DASHED, LineWeight.W035)),
    (Discipline.PLUMBING, DisciplineStyle((50.0, 50.0, 230.0), LineType.DASHED_SPACED, LineWeight.W035)),
    (Discipline.EQUIPMENT, DisciplineStyle((75.0, 70.0, 95.0), LineType.CONTINUOUS, LineWeight.W025)),
    (Discipline.RESOURCE, DisciplineStyle((65.0, 0.0, 0.0), LineType.CONTINUOUS, LineWeight.W013)),
    (Discipline.STRUCTURAL, DisciplineStyle((45.0, 70.0, 25.0), LineType.CONTINUOUS, LineWeight.W050)),
    (Discipline.TELECOM, DisciplineStyle((55.0, 45.0, 300.0), LineType.DOTTED, LineWeight.W025)),
    (Discipline.SURVEY, DisciplineStyle((65.0, 35.0, 110.0), LineType.LONG_DASH_DOT, LineWeight.W018)),
    (Discipline.PROCESS, DisciplineStyle((60.0, 65.0, 70.0), LineType.CONTINUOUS, LineWeight.W035)),
    (Discipline.OTHER, DisciplineStyle((80.0, 0.0, 0.0), LineType.CONTINUOUS, LineWeight.W018)),
    (Discipline.CONTRACTOR, DisciplineStyle((40.0, 0.0, 0.0), LineType.DASHED, LineWeight.W018)),
])
# ISO 13567 status -> screened-plot transparency (0.0 opaque .. 1.0 clear); None = fully opaque.
STATUS_SCREEN: Final[Map[Status, float | None]] = Map.of_seq([
    (Status.NEW, None),
    (Status.EXISTING, 0.55),
    (Status.DEMOLISH, 0.7),
    (Status.FUTURE, 0.4),
    (Status.TEMPORARY, 0.35),
    (Status.MOVED, 0.5),
    (Status.RELOCATED, 0.5),
    (Status.NOT_IN_CONTRACT, 0.6),
])
# ISO 128-50 material -> pattern-plane fill: preset geometry at its density law; poché/grade values resolved.
HATCH_BIND: Final[Map[HatchMaterial, HatchFill]] = Map.of_seq([
    (HatchMaterial.STEEL, HatchFill(solid="lch(20% 0 0)")),
    (HatchMaterial.CONCRETE, HatchFill(pattern=PRESETS[SectionPattern.GENERAL])),
    (HatchMaterial.CONCRETE_REINFORCED, HatchFill(pattern=PRESETS[SectionPattern.DOUBLE])),
    (HatchMaterial.MASONRY, HatchFill(pattern=PRESETS[SectionPattern.MASONRY])),
    (HatchMaterial.TIMBER_GRAIN, HatchFill(pattern=PRESETS[SectionPattern.HERRINGBONE])),
    (HatchMaterial.TIMBER_END, HatchFill(pattern=PRESETS[SectionPattern.END_GRAIN])),
    (HatchMaterial.INSULATION_THERMAL, HatchFill(pattern=PRESETS[SectionPattern.INSULATION])),
    (HatchMaterial.EARTH, HatchFill(gradient=(((0.0, "lch(30% 20 70)"), (1.0, "lch(70% 10 90)")), 45.0))),
    (HatchMaterial.HARDCORE, HatchFill(pattern=PRESETS[SectionPattern.GRAVEL])),
    (HatchMaterial.LIQUID, HatchFill(pattern=PRESETS[SectionPattern.LIQUID])),
    (HatchMaterial.GLASS, HatchFill(pattern=PRESETS[SectionPattern.GLASS])),
])

# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "Discipline",
    "DisciplineStyle",
    "HATCH_BIND",
    "HatchMaterial",
    "LayerName",
    "LayerSchema",
    "LetteringMetric",
    "LetteringRow",
    "LetteringStyle",
    "LineType",
    "LineWeight",
    "PENS",
    "PaperSeries",
    "STATUS_SCREEN",
    "ScaleRatio",
    "SheetId",
    "SheetType",
    "Status",
    "Terminator",
    "TextHeight",
    "lettering",
    "paper",
]
```

The vocabulary is the drafting contract and the bind rows are its resolved values, so every consumer reads one truth per code: a schedule legend, a DXF hatch, and a layered-PSD fill all trace `HatchMaterial.CONCRETE` to the same `PatternSpec` preset at the same density law; a dimension pen, an SVG stroke, and a theme swatch all trace `Discipline.STRUCTURAL` to the same LCh row (derive converts the value to whatever model the target needs); a general-note, a dimension text, and a title-block label all trace `TextHeight.H3_5` through the same `lettering()` fold whose cap correction is the `typography/font#FONT` `FaceMetrics` value. The codecs make identifier grammar data — one `LayerName` field superset projecting the AIA, ISO 13567, and NCS spellings, one `SheetId` composing the NCS sheet number — and the derivations make paper and scale arithmetic instead of tables, so ISO 216 sizes and ISO 5455 factors cannot drift from their law. `drawing/standard#STANDARD` composes every row downward into the `ezdxf` symbol tables; `graphic/style#STYLE` selects rows per office theme; nothing above s1 is imported and nothing here mints a receipt.
