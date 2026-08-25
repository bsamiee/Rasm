# [PY_ARTIFACTS_DRAWING_REGIME]

Closed AEC drafting vocabulary and BIND data form the s1 floor keyed by the drawing, composition, specification, and delivery planes. Each drafting code set is a closed `StrEnum` family: ISO 128-2 `LineType`, ISO 128-20 `LineWeight`, ISO 128-50 `HatchMaterial`, ISO 5455 `ScaleRatio`, AIA/ISO 13567/NCS `Discipline`/`Status`/`SheetType`, ISO 3098 `TextHeight`/`LetteringStyle`/`LetteringPosture`, and ISO 129-1 `Terminator`. `LayerName.compose` and `LayerName.parsed` are inverses over the AIA, ISO 13567, and NCS structures, including the complete ISO optional band and both NCS minor groups; `SheetId.compose` and `SheetId.parsed` own the NCS `"A-201"` form. `paper()` derives ISO 216 sizes from one seed, `ScaleRatio.factor` derives the printed ratio, and `LineWeight.group` returns `Nothing` where no valid two-width pair exists.

No `ezdxf` lowering crosses this owner: `Terminator.lowering` publishes the line-end row — block name beside tick size — that every drawing consumer lowers for itself, and no member of this floor touches a host resource. `Standard` consumes the vocabulary rows for symbol-table mutation; `FaceMetrics` contributes cap-height evidence to `lettering`; `HatchFill` and `ColorModel` remain values whose geometry and conversion stay at their owning boundaries. Admission is concentrated in the `beartype`-refined factories and parsers, and every derivation is total over admitted values.

## [01]-[INDEX]

- [02]-[REGIME]: Closed drafting vocabulary, inverse name codecs, ISO 216/5455 derivations, and the `HATCH_BIND`/`PENS`/`lettering()` BIND rows form one owner.

## [02]-[REGIME]

- Owner: one vocabulary-and-bind floor, zero lowering. Every family is closed at its published cardinality; every correspondence is one `Map` row set with a single edit site. A vocabulary member is a code, a bind row is the code's resolved value set, and the lowering that mutates a host resource lives with the host owner — never here.
- Law: `LayerName` carries both NCS minor groups, the discipline modifier, and the ISO 13567 `agent`/`presentation`/`status` plus `sector`/`phase`/`projection`/`scale`/`work_package`/`user` optional band. `LayerSchema` selects one total `compose` arm, and `parsed` validates the same grammar before calling `LayerName.of`; no arm truncates, shifts, or accepts a partial optional slot. `SheetId.compose` and `SheetId.parsed` form the same inverse over the NCS `"A-201"` identifier.
- Law: `HATCH_BIND` maps every `HatchMaterial` to one `HatchFill`; `PENS` carries each discipline's LCh coordinates and ISO 128 line row; `Terminator.lowering` carries the ONE `TerminatorRow` — block name beside oblique-tick size — every drawing consumer reads, so the DIMTSZ-versus-block choice is the consumer's and the correspondence itself has one edit site; `lettering()` combines type, posture, nominal height, and optional `FaceMetrics` into one `LetteringMetric`, including the inclined-lettering slant.
- Entry: `LayerName.of`, `SheetId.of`, and `paper` carry `Annotated` refinements under `INGRESS`, the ONE drawing-plane ingress conf every producer head above this floor composes rather than re-declaring. `expression.extra.result.catch` captures the one `ValueError` family raised by enum and refinement admission inside each parser, so malformed syntax and unknown codes stay distinct `NameFault` values.
- Auto: every derivation is arithmetic, never a parallel frozen table that can drift. `paper()` floor-halves the series seed per rank with the long-edge swap, so the sheet plane keeps no successor size table; `ScaleRatio.factor` divides the printed ratio; `LineWeight.group` derives the two-width pen pair two R10 steps down and returns `Option` — `W013`/`W018` yield `Nothing` because no thinner ISO member exists, so an invalid 1:1 pair is unrepresentable; `TextHeight.mm` reads the cascade.
- Growth: A new vocabulary member lands with its owning row; a new layer-name field crosses the factory, parser, and projection on the same owner; a new schema adds one `LayerSchema` member and one arm to each inverse direction.
- Boundary: no `ezdxf` or host-resource authoring (`drawing/standard#STANDARD`); no pattern geometry (`graphic/vector/pattern#PATTERN`); no colour conversion (`graphic/color/derive#DERIVE` — pen rows carry coordinates, never a conversion arm); no font-binary read (`typography/font#FONT` — the fold consumes the `FaceMetrics` value); no theme selection (`graphic/style#STYLE`); no sheet placement, receipt, plan node, or rail beyond the codec `Result` — the substrate is total.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import re
from enum import StrEnum
from typing import Annotated, Final, Literal, Self, assert_never

from beartype import BeartypeConf, beartype
from beartype.vale import Is
from expression import Error, Nothing, Option, Result, Some
from expression.collections import Map
from expression.extra.result import catch
from msgspec import Struct

from rasm.artifacts.graphic.color.derive import ColorModel
from rasm.artifacts.graphic.vector.pattern import HatchFill, PRESETS, SectionPattern
from rasm.artifacts.typography.font import FaceMetrics

# --- [TYPES] ----------------------------------------------------------------------------
type Lch = tuple[float, float, float]
type Pattern = tuple[float, ...]
type NameFault = Literal["<malformed-name>", "<unknown-code>"]

type Major = Annotated[str, Is[lambda s: 1 <= len(s) <= 4 and s.isascii() and s.isalnum() and s.isupper()]]
type Minor = Annotated[str, Is[lambda s: len(s) <= 4 and (not s or (s.isascii() and s.isalnum() and s.isupper()))]]
type Code1 = Annotated[str, Is[lambda s: len(s) <= 1 and (not s or (s.isascii() and s.isalnum() and s.isupper()))]]
type Code2 = Annotated[str, Is[lambda s: len(s) == 2 and (s == "--" or (s.isascii() and s.isalnum() and s.isupper()))]]
type Band4 = Annotated[str, Is[lambda s: len(s) <= 4 and (not s or (s.isascii() and s.isalnum() and s.isupper()))]]
type Band2 = Annotated[str, Is[lambda s: len(s) <= 2 and (not s or (s.isascii() and s.isalnum() and s.isupper()))]]
type IsoElement = Annotated[str, Is[lambda s: not s or len(s) == 6 and s.isascii() and all(ch.isalnum() or ch == "_" for ch in s) and s.upper() == s]]
type Sequence99 = Annotated[int, Is[lambda n: 1 <= n <= 99]]
type PaperRank = Annotated[int, Is[lambda n: 0 <= n <= 10]]


class LineType(StrEnum):
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


class LineWeight(StrEnum):
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
    def group(self) -> "Option[tuple[LineWeight, LineWeight]]":
        order = tuple(LineWeight)
        rank = order.index(self)
        return Some((self, order[rank - 2])) if rank >= 2 else Nothing


class HatchMaterial(StrEnum):
    STEEL = "steel"
    ALUMINIUM = "aluminium"
    CONCRETE = "concrete"
    CONCRETE_REINFORCED = "concrete_reinforced"
    MASONRY = "masonry"
    TIMBER_GRAIN = "timber_grain"
    TIMBER_END = "timber_end"
    INSULATION_THERMAL = "insulation_thermal"
    PLASTIC = "plastic"
    EARTH = "earth"
    HARDCORE = "hardcore"
    LIQUID = "liquid"
    GLASS = "glass"


class ScaleRatio(StrEnum):
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
    def ratio(self) -> str:
        return self.value

    @property
    def factor(self) -> float:
        paper, model = (float(p) for p in self.value.split(":"))
        return paper / model


class Discipline(StrEnum):
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


class Status(StrEnum):
    NEW = "N"
    EXISTING = "E"
    DEMOLISH = "D"
    FUTURE = "F"
    TEMPORARY = "T"
    MOVED = "M"
    RELOCATED = "R"
    NOT_IN_CONTRACT = "X"


class SheetType(StrEnum):
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


class TextHeight(StrEnum):
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


class LetteringStyle(StrEnum):
    TYPE_A = "A"
    TYPE_B = "B"


class LetteringPosture(StrEnum):
    UPRIGHT = "upright"
    INCLINED = "inclined"


class Terminator(StrEnum):
    FILLED_ARROW = "filled_arrow"
    OPEN_ARROW = "open_arrow"
    OBLIQUE_STROKE = "oblique_stroke"
    DOT = "dot"
    ORIGIN_INDICATION = "origin_indication"
    NONE = "none"

    @property
    def lowering(self) -> "TerminatorRow":
        return _TERMINATOR[self]


class LayerSchema(StrEnum):
    AIA = "aia"
    ISO13567 = "iso13567"
    NCS = "ncs"


class PaperSeries(StrEnum):
    A = "A"
    B = "B"


# --- [CONSTANTS] ------------------------------------------------------------------------
INGRESS: Final[BeartypeConf] = BeartypeConf(violation_type=ValueError)
_AIA_NAME: Final[re.Pattern[str]] = re.compile(r"^(?P<d>[A-Z])-(?P<major>[A-Z0-9]{1,4})(?:-(?P<minor>[A-Z0-9]{1,4}))?-(?P<status>[A-Z])$")
_NCS_NAME: Final[re.Pattern[str]] = re.compile(
    r"^(?P<d>[A-Z])(?P<modifier>[A-Z]?)-(?P<major>[A-Z0-9]{1,4})(?:-(?P<minor>[A-Z0-9]{1,4}))?"
    r"(?:-(?P<minor_2>[A-Z0-9]{1,4}))?-(?P<status>[A-Z])$"
)
_ISO_NAME: Final[re.Pattern[str]] = re.compile(
    r"^(?P<agent>[A-Z0-9-]{2})(?P<d>[A-Z])(?P<element>[A-Z0-9_]{6})(?P<presentation>[A-Z0-9-]{2})(?P<status>[A-Z])"
    r"(?:(?P<sector>[A-Z0-9_]{4})(?:(?P<phase>[A-Z0-9_])(?:(?P<projection>[A-Z0-9_])(?:(?P<scale>[A-Z0-9_])"
    r"(?:(?P<work>[A-Z0-9_]{2})(?P<user>[A-Z0-9_]{4})?)?)?)?)?)?$"
)
_SHEET_ID: Final[re.Pattern[str]] = re.compile(r"^(?P<d>[A-Z])-(?P<t>[0-9])(?P<seq>[0-9]{2})$")


# --- [MODELS] ---------------------------------------------------------------------------
class LetteringRow(Struct, frozen=True):
    stroke: float
    width: float
    spacing: float
    baseline: float
    word: float


class LetteringMetric(Struct, frozen=True):
    height: float
    pen: float
    char_width: float
    pitch: float
    char_spacing: float
    word_spacing: float
    slant: float
    point_size: float


class DisciplineStyle(Struct, frozen=True):
    lch: Lch
    linetype: LineType
    lineweight: LineWeight
    model: ColorModel = ColorModel.LCHAB


class TerminatorRow(Struct, frozen=True):
    block: str
    tick: float


class LayerName(Struct, frozen=True):
    discipline: Discipline
    major: str
    discipline_modifier: str = ""
    minor: str = ""
    minor_2: str = ""
    iso_element: str = ""
    status: Status = Status.NEW
    agent: str = "--"
    presentation: str = "--"
    sector: str = ""
    phase: str = ""
    projection: str = ""
    scale: str = ""
    work_package: str = ""
    user: str = ""

    @classmethod
    @beartype(conf=INGRESS)
    def of(
        cls,
        discipline: Discipline,
        major: Major,
        minor: Minor = "",
        status: Status = Status.NEW,
        *,
        discipline_modifier: Code1 = "",
        minor_2: Minor = "",
        iso_element: IsoElement = "",
        agent: Code2 = "--",
        presentation: Code2 = "--",
        sector: Band4 = "",
        phase: Code1 = "",
        projection: Code1 = "",
        scale: Code1 = "",
        work_package: Band2 = "",
        user: Band4 = "",
    ) -> Self:
        if minor_2 and not minor:
            raise ValueError("minor_2 requires minor")
        if iso_element and (iso_element[:4].rstrip("_") != major or iso_element[4:].rstrip("_") != minor):
            raise ValueError("iso_element disagrees with major/minor")
        return cls(
            discipline=discipline, discipline_modifier=discipline_modifier, major=major, minor=minor, minor_2=minor_2,
            iso_element=iso_element, status=status,
            agent=agent, presentation=presentation, sector=sector, phase=phase, projection=projection, scale=scale,
            work_package=work_package, user=user,
        )

    @classmethod
    def parsed(cls, text: str, schema: LayerSchema = LayerSchema.AIA, /) -> Result[Self, NameFault]:
        match schema:
            case LayerSchema.AIA:
                return Error("<malformed-name>") if (found := _AIA_NAME.fullmatch(text)) is None else catch(exception=ValueError)(
                    lambda: cls.of(Discipline(found["d"]), found["major"], found["minor"] or "", Status(found["status"]))
                )().map_error(lambda _fault: "<unknown-code>")
            case LayerSchema.NCS:
                return Error("<malformed-name>") if (found := _NCS_NAME.fullmatch(text)) is None or (
                    found["minor_2"] and not found["minor"]
                ) else catch(exception=ValueError)(
                    lambda: cls.of(
                        Discipline(found["d"]), found["major"], found["minor"] or "", Status(found["status"]),
                        discipline_modifier=found["modifier"], minor_2=found["minor_2"] or "",
                    )
                )().map_error(lambda _fault: "<unknown-code>")
            case LayerSchema.ISO13567:
                return Error("<malformed-name>") if (found := _ISO_NAME.fullmatch(text)) is None else catch(exception=ValueError)(
                    lambda: cls.of(
                        Discipline(found["d"]), found["element"][:4].rstrip("_"), found["element"][4:].rstrip("_"),
                        Status(found["status"]), iso_element=found["element"], agent=found["agent"], presentation=found["presentation"],
                        sector=(found["sector"] or "").rstrip("_"), phase=(found["phase"] or "").rstrip("_"),
                        projection=(found["projection"] or "").rstrip("_"), scale=(found["scale"] or "").rstrip("_"),
                        work_package=(found["work"] or "").rstrip("_"), user=(found["user"] or "").rstrip("_"),
                    )
                )().map_error(lambda _fault: "<unknown-code>")
            case unreachable:
                assert_never(unreachable)

    def compose(self, schema: LayerSchema = LayerSchema.AIA, /) -> str:
        match schema:
            case LayerSchema.AIA:
                parts = (self.discipline.value, self.major, *((self.minor,) if self.minor else ()), self.status.value)
                return "-".join(parts)
            case LayerSchema.ISO13567:
                if len(self.minor) > 2:
                    raise ValueError("minor exceeds the two-character ISO 13567 element tail")
                element = self.iso_element or f"{self.major:_<4}{self.minor:_<2}"
                head = f"{self.agent}{self.discipline.value}{element}{self.presentation}{self.status.value}"
                values = (self.sector, self.phase, self.projection, self.scale, self.work_package, self.user)
                optionals = (f"{self.sector:_<4}", f"{self.phase:_<1}", f"{self.projection:_<1}", f"{self.scale:_<1}", f"{self.work_package:_<2}", f"{self.user:_<4}")
                keep = next((rank for rank in range(len(values), 0, -1) if values[rank - 1]), 0)
                return head + "".join(optionals[:keep])
            case LayerSchema.NCS:
                fields = (f"{self.discipline.value}{self.discipline_modifier}", self.major, self.minor, self.minor_2, self.status.value)
                return "-".join(field for field in fields if field)
            case unreachable:
                assert_never(unreachable)

    @property
    def pen(self) -> DisciplineStyle:
        return PENS[self.discipline]


class SheetId(Struct, frozen=True):
    discipline: Discipline
    sheet_type: SheetType
    sequence: int = 1

    @classmethod
    @beartype(conf=INGRESS)
    def of(cls, discipline: Discipline, sheet_type: SheetType, sequence: Sequence99 = 1) -> Self:
        return cls(discipline=discipline, sheet_type=sheet_type, sequence=sequence)

    @classmethod
    def parsed(cls, text: str, /) -> Result[Self, NameFault]:
        match _SHEET_ID.fullmatch(text):
            case None:
                return Error("<malformed-name>")
            case found:
                return catch(exception=ValueError)(
                    lambda: cls.of(Discipline(found["d"]), SheetType(found["t"]), int(found["seq"]))
                )().map_error(lambda _fault: "<unknown-code>")

    def compose(self) -> str:
        return f"{self.discipline.value}-{self.sheet_type.value}{self.sequence:02d}"


# --- [OPERATIONS] -----------------------------------------------------------------------
@beartype(conf=INGRESS)
def paper(series: PaperSeries, rank: PaperRank, /) -> tuple[float, float]:
    w, h = _PAPER_SEED[series]
    for _ in range(rank):
        w, h = h // 2, w
    return (float(w), float(h))


def lettering(
    style: LetteringStyle, height: TextHeight, metrics: Option[FaceMetrics] = Nothing, /, *, posture: LetteringPosture = LetteringPosture.UPRIGHT
) -> LetteringMetric:
    row, h = _LETTERING[style], height.mm
    point = metrics.map(lambda m: m.point_size(h)).default_value(h)
    return LetteringMetric(
        height=h,
        pen=row.stroke * h,
        char_width=row.width * h,
        pitch=row.baseline * h,
        char_spacing=row.spacing * h,
        word_spacing=row.word * h,
        slant=_SLANT[posture],
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
_SLANT: Final[Map[LetteringPosture, float]] = Map.of_seq([
    (LetteringPosture.UPRIGHT, 0.0),
    (LetteringPosture.INCLINED, 15.0),
])
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
_TERMINATOR: Final[Map[Terminator, TerminatorRow]] = Map.of_seq([
    (Terminator.FILLED_ARROW, TerminatorRow(block="", tick=0.0)),
    (Terminator.OPEN_ARROW, TerminatorRow(block="OPEN", tick=0.0)),
    (Terminator.OBLIQUE_STROKE, TerminatorRow(block="OBLIQUE", tick=2.5)),
    (Terminator.DOT, TerminatorRow(block="DOT", tick=0.0)),
    (Terminator.ORIGIN_INDICATION, TerminatorRow(block="ORIGIN", tick=0.0)),
    (Terminator.NONE, TerminatorRow(block="NONE", tick=0.0)),
])
HATCH_BIND: Final[Map[HatchMaterial, HatchFill]] = Map.of_seq([
    (HatchMaterial.STEEL, HatchFill(solid="lch(20% 0 0)")),
    (HatchMaterial.ALUMINIUM, HatchFill(pattern=PRESETS[SectionPattern.CROSS_DIAGONAL])),
    (HatchMaterial.CONCRETE, HatchFill(pattern=PRESETS[SectionPattern.GENERAL])),
    (HatchMaterial.CONCRETE_REINFORCED, HatchFill(pattern=PRESETS[SectionPattern.DOUBLE])),
    (HatchMaterial.MASONRY, HatchFill(pattern=PRESETS[SectionPattern.MASONRY])),
    (HatchMaterial.TIMBER_GRAIN, HatchFill(pattern=PRESETS[SectionPattern.HERRINGBONE])),
    (HatchMaterial.TIMBER_END, HatchFill(pattern=PRESETS[SectionPattern.END_GRAIN])),
    (HatchMaterial.INSULATION_THERMAL, HatchFill(pattern=PRESETS[SectionPattern.INSULATION])),
    (HatchMaterial.PLASTIC, HatchFill(pattern=PRESETS[SectionPattern.CROSS])),
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
    "INGRESS",
    "HatchMaterial",
    "LayerName",
    "LayerSchema",
    "LetteringMetric",
    "LetteringPosture",
    "LetteringRow",
    "LetteringStyle",
    "LineType",
    "LineWeight",
    "NameFault",
    "PENS",
    "PaperSeries",
    "STATUS_SCREEN",
    "ScaleRatio",
    "SheetId",
    "SheetType",
    "Status",
    "Terminator",
    "TerminatorRow",
    "TextHeight",
    "lettering",
    "paper",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
