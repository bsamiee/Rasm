# [PY_ARTIFACTS_DRAWING_REGIME]

The closed AEC drafting vocabulary and BIND substrate — the s1 floor the drawing, composition, specification, and delivery planes key. Each drafting code set stands at its published cardinality as a closed `StrEnum` family: the ISO 128-2 `LineType`, the ISO 128-20 `LineWeight` R10 cascade with its two-width group law, the ISO 128-50 `HatchMaterial` section family, the ISO 5455 `ScaleRatio` enlargement/full/reduction set, the AIA/ISO 13567/NCS `Discipline`/`Status`/`SheetType` codes, the ISO 3098 `TextHeight` cascade and `LetteringStyle` type-A/B ratios, and the ISO 129-1 `Terminator` line-ends. The codecs compose codes into identifiers — `LayerName` projects the ISO 13567, AIA, and NCS grammars from one field superset, `SheetId` composes the NCS number — and the derivations compute what a frozen table cannot keep true: `paper()` halves the ISO 216 seed and `ScaleRatio.factor` divides the ratio string.

No `ezdxf` and no lowering crosses this page — `drawing/standard#STANDARD` composes these rows down into the symbol tables (seed, graphics, dimstyle, hatch entity, ACI pens, paper factor) from s3. `graphic/style#STYLE` selects pen/lettering rows per office theme; `graphic/layer#LAYER` reads the ISO 13567 field vocabulary; `composition/sheet#SHEET` reads `ScaleRatio`/`SheetId` and the ISO 216 derivation; `specification/classify#CLASSIFY` and `delivery/register#REGISTER` compose `Discipline`; the drawing producers read every family. The page imports only `graphic/color/derive#DERIVE`, `graphic/vector/pattern#PATTERN`, and the `typography/font#FONT` `FaceMetrics` value, and mints no receipt, plan node, or async — the vocabularies are total, the binds are data, the derivations pure arithmetic.

## [01]-[INDEX]

- [01]-[REGIME]: the closed drafting vocabulary, the three-schema `LayerName` and `SheetId` codecs, the ISO 216/5455 derivations, and the `HATCH_BIND`/`PENS`/`lettering()` BIND rows every drawing consumer keys.

## [02]-[REGIME]

- Owner: one vocabulary-and-bind floor, zero lowering. Every family is closed at its published cardinality; every correspondence is one `Map` row set with a single edit site. A vocabulary member is a code, a bind row is the code's resolved value set, and the lowering that mutates a host resource lives with the host owner — never here.
- Codecs: `LayerName` is ONE owner over three published schemas — the field superset lives on one struct and each `LayerSchema` arm projects its slice (`AIA` the `Discipline-MAJOR[-MINOR]-Status` long form, `ISO13567` the mandatory-plus-optional field string, `NCS` the level-2 form) — so a projector never re-derives a name grammar. `SheetId.of` composes the NCS `"A-201"` number `composition/sheet#SHEET` assembles and `delivery/register#REGISTER` keys.
- Derivations: arithmetic, never a parallel frozen table that can drift. `paper()` floor-halves the series seed per rank with the long-edge swap, so the sheet plane keeps no successor size table; `ScaleRatio.factor` divides the printed ratio; `LineWeight.group` derives the two-width pen pair two R10 steps down; `TextHeight.mm` reads the cascade.
- Bind: `HATCH_BIND` maps each `HatchMaterial` onto a `graphic/vector/pattern#PATTERN` `HatchFill` at its density law with poché/grade resolved, so standard's `Hatch` entity, a schedule legend swatch, and a layered-PSD fill all draw one row. `PENS` carries each discipline pen as LCh coordinates plus ISO 128 linetype/weight — a consumer needing sRGB/CMYK routes the value through `graphic/color/derive#DERIVE`, never a second colour truth here. `lettering()` folds the ISO 3098 ratio row with the optional `typography/font#FONT` `FaceMetrics` cap-height into the `LetteringMetric` the dimension/annotate/symbol/sheet text producers read; with no metric bound the point size falls to the nominal height.
- Growth: a new line type, weight, material, scale, discipline, status, sheet type, height, lettering type, or terminator is one member plus one owning-table row; a new layer-name field is one `LayerName` field threaded through the schema projections; a new name schema is one `LayerSchema` member plus one `compose` arm; a new material fill is one `HATCH_BIND` row; a new pen axis is one `DisciplineStyle` field; a new paper series is one `_PAPER_SEED` row. A new consumer keys the existing families — zero new surface.
- Boundary: no `ezdxf` or host-resource authoring (`drawing/standard#STANDARD`); no pattern geometry (`graphic/vector/pattern#PATTERN`); no colour conversion (`graphic/color/derive#DERIVE` — pen rows carry coordinates, never a conversion arm); no font-binary read (`typography/font#FONT` — the fold consumes the `FaceMetrics` value); no theme selection (`graphic/style#STYLE`); no sheet placement, receipt, plan node, or rail — the substrate is total.

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
type Lch = tuple[float, float, float]  # CIE LCh(ab) pen coordinates — derive resolves the target model from the value
type Pattern = tuple[float, ...]  # ISO 128-2 dash array: +dash / -gap / 0.0 dot, mm at 1:1


class LineType(StrEnum):  # ISO 128-2:2020 Table 1 basic line types
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
    def ratio(self) -> str:  # the printed "1:100" string title block and dimension suffix draw
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


class Terminator(StrEnum):  # ISO 129-1 line-end family — proportion geometry is drawing/symbol's
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
    # the discipline pen — LCh coordinates + ISO 128 line row.
    lch: Lch
    linetype: LineType
    lineweight: LineWeight
    model: ColorModel = ColorModel.LCH


class LayerName(Struct, frozen=True):
    # the field superset; each schema projects its slice via compose().
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
    # NCS sheet id — discipline + sheet-type + sequence ("A-201").
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
# discipline pen rows — LCh value + ISO 128 line row; the DXF ACI pen is standard's lowering, not a second truth here.
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
# ISO 128-50 material -> pattern-plane fill; poché/grade resolved.
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

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
