# [PY_ARTIFACTS_CLASSIFY]

Construction classification binds `MasterFormat` work results, `UniFormat` elements, `OmniClass` facets, and the drawing-to-specification `ReferenceIndex`. `crosswalk` preserves relation semantics across systems; `resolve` and `coordinate` bind sections to drawing references. This pure substrate authors no artifact or receipt.

`ClassCode` stores a `ClassSystem` discriminant and one per-mode `ClassPayload`: numeric segments for `MasterFormat` and `OmniClass`, or `(group, segments)` for `UniFormat`. `__post_init__` admits assigned divisions, known elements, known OmniClass tables, bounded segment pairs, and valid hierarchy depth. `_CROSSWALK` stores related division-to-element rows, `_ELEMENT_DIVISIONS` derives the inverse, OmniClass Table 22 mirrors `MasterFormat` exactly, and Table 21 remains a broader elemental anchor because its notation differs. `ClassRelation` keeps `EXACT`, `RELATED`, and `BROADER` results distinguishable.

## [01]-[INDEX]

- [02]-[CODE]: `ClassCode` — the cross-system classification value object and the three closed vocabularies, `crosswalk` folding a code onto its peers across systems.
- [03]-[COORDINATE]: `ReferenceIndex` — the drawing↔spec cross-reference half, `resolve` the one polymorphic query and `coordinate` the section-to-sheet reconciliation.

## [02]-[CODE]

- Owner: `ClassCode` pairs `ClassSystem` with one `ClassPayload`; only `UniFormat` carries a group, so numeric systems have no dead group field. `__post_init__` enforces vocabulary membership and shape for direct and parsed construction.
- Cases: the three vocabularies are closed correspondence tables authored to exact published cardinality — `_DIVISIONS` (the `MasterFormat` 2020 divisions over six `Subgroup` bands), `_ELEMENTS` (the `UniFormat` groups and their Level-2 elements), `_OMNI_TABLES` (the OmniClass tables); the reserved `MasterFormat` slots are the `range(50)` complement over the assigned set, never fabricated titles, each row a frozen value, never a stringly prefix probe.
- Entry: `ClassCode.parse(system, text)` returns `Result[Self, ClassFault]` — the system's module-level `re.Pattern` matches once, then the HEAD validates against its vocabulary, so a deeper code admits under a known broadscope parent (`UniFormat` `B1010` under element `B10`, `MasterFormat` `03 30 53` under Division `03`). A reserved slot, an out-of-range division, an unknown element, and an unknown table each surface as their own `ClassFault` case, the `_RESERVED` complement splitting the future-expansion slot from the invalid number.
- Auto: `crosswalk` derives relation-bearing peers from `_CROSSWALK`, `_ELEMENT_DIVISIONS`, and OmniClass table alignment. Division-to-element edges are `RELATED`, Table 22 mirrors are `EXACT`, and Table 21 roots are `BROADER`. `level` and `parent` share `_deepest`; `descends_from` treats trailing `MasterFormat` heading zeros as wildcards after an exact division head and uses exact prefixes for `UniFormat` and `OmniClass`.
- Receipt: none — `ClassCode` is a pure value object and `crosswalk` a pure projection; the validated code travels INTO the composing producers' facts, exactly as `exchange/detect#DETECT` and `drawing/regime#REGIME` contribute none.
- Packages: `msgspec` supplies frozen value owners and `structs.replace`; `re` supplies compiled system parsers; `frozendict` carries relation projections; `expression` supplies `Map`, `Result`, `Option`, and `Block` rails.
- Growth: a new division/element/table is one `_DIVISIONS`/`_ELEMENTS`/`_OMNI_TABLES` row; a new classification system is one `ClassSystem` member plus one `re.Pattern` and one arm on each projection (`assert_never` forcing each); a new crosswalk edge is one `_CROSSWALK` row the inverse absorbs; a new fault cause is one `ClassFault` case.
- Boundary: this owner authors classification semantics, never bytes — no `ArtifactReceipt` case (the composing producers carry the code in their own facts), no rendered artifact.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import re
from builtins import frozendict
from enum import StrEnum
from typing import Final, Literal, Self, assert_never

from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Map
from msgspec import Struct, structs

# --- [TYPES] ----------------------------------------------------------------------------


class ClassSystem(StrEnum):
    MASTERFORMAT = "masterformat"
    UNIFORMAT = "uniformat"
    OMNICLASS = "omniclass"


class Subgroup(StrEnum):
    PROCUREMENT = "procurement"
    GENERAL = "general"
    CONSTRUCTION = "construction"
    SERVICES = "services"
    INFRASTRUCTURE = "infrastructure"
    PROCESS = "process"


class ClassRelation(StrEnum):
    EXACT = "exact"
    RELATED = "related"
    BROADER = "broader"


type ClassFault = Literal["<malformed-code>", "<unknown-division>", "<reserved-division>", "<unknown-element>", "<unknown-table>"]
type ClassPayload = tuple[int, ...] | tuple[str, tuple[int, ...]]
type ClassPeer = tuple["ClassCode", ClassRelation]

# --- [MODELS] ---------------------------------------------------------------------------


class Division(Struct, frozen=True):
    number: int
    title: str
    subgroup: Subgroup


class Element(Struct, frozen=True):
    code: str
    title: str
    group: str


class ClassCode(Struct, frozen=True):
    system: ClassSystem
    payload: ClassPayload

    @property
    def segments(self) -> tuple[int, ...]:
        match self.system, self.payload:
            case ClassSystem.UNIFORMAT, (str(), tuple() as segments):
                return segments
            case (ClassSystem.MASTERFORMAT | ClassSystem.OMNICLASS), tuple() as segments:
                return segments
            case _:
                return ()

    @property
    def _group(self) -> str:
        match self.system, self.payload:
            case ClassSystem.UNIFORMAT, (str() as group, tuple()):
                return group
            case _:
                return ""

    def __post_init__(self) -> None:
        segments = self.segments
        band = all(isinstance(segment, int) and 0 <= segment <= 99 for segment in segments)
        match self.system:
            case ClassSystem.MASTERFORMAT:
                valid = len(segments) in (3, 4) and segments[0] in _DIVISIONS
            case ClassSystem.UNIFORMAT:
                valid = self._group in _GROUP_TITLES and len(segments) <= 3 and (not segments or self._uf_anchor in _ELEMENTS)
            case ClassSystem.OMNICLASS:
                valid = 1 <= len(segments) <= 7 and segments[0] in _OMNI_TABLES
            case _ as unreachable:
                assert_never(unreachable)
        if not (band and valid):
            raise ValueError(f"<invalid:{self.system}:{self.payload}>")

    @classmethod
    def parse(cls, system: ClassSystem, text: str, /) -> Result[Self, ClassFault]:
        match system:
            case ClassSystem.MASTERFORMAT:
                if (found := _MF_PATTERN.match(text)) is None:
                    return Error("<malformed-code>")
                segments = tuple(int(part) for part in found.groups() if part is not None)
                if segments[0] in _DIVISIONS:
                    return Ok(cls(system, segments))
                return Error("<reserved-division>") if segments[0] in _RESERVED else Error("<unknown-division>")
            case ClassSystem.UNIFORMAT:
                if (found := _UF_PATTERN.match(text)) is None:
                    return Error("<malformed-code>")
                segments, group = _pairs(found["pairs"]), found["group"]
                anchor = group + (f"{segments[0]:02d}" if segments else "")
                return (
                    Error("<malformed-code>")
                    if len(segments) > 3
                    else Ok(cls(system, (group, segments))) if not segments or anchor in _ELEMENTS else Error("<unknown-element>")
                )
            case ClassSystem.OMNICLASS:
                if (found := _OC_PATTERN.match(text)) is None:
                    return Error("<malformed-code>")
                tail = found["tail"] or ""
                segments = (int(found["table"]), *(int(pair) for pair in tail.split()))
                return (
                    Error("<malformed-code>")
                    if len(segments) > 7
                    else Ok(cls(system, segments)) if segments[0] in _OMNI_TABLES else Error("<unknown-table>")
                )
            case _ as unreachable:
                assert_never(unreachable)

    def render(self) -> str:
        match self.system:
            case ClassSystem.MASTERFORMAT:
                head = " ".join(f"{part:02d}" for part in self.segments[:3])
                return head + "".join(f".{part:02d}" for part in self.segments[3:])
            case ClassSystem.UNIFORMAT:
                return self._group + "".join(f"{part:02d}" for part in self.segments)
            case ClassSystem.OMNICLASS:
                head = f"{self.segments[0]:02d}" if self.segments else ""
                tail = " ".join(f"{part:02d}" for part in self.segments[1:])
                return f"{head}-{tail}" if tail else head
            case _ as unreachable:
                assert_never(unreachable)

    def title(self) -> Option[str]:
        match self.system:
            case ClassSystem.MASTERFORMAT:
                return _DIVISIONS.try_find(self.segments[0]).map(lambda division: division.title) if self.segments else Nothing
            case ClassSystem.UNIFORMAT:
                return _ELEMENTS.try_find(self._uf_anchor).map(lambda element: element.title).or_else_with(lambda: _GROUP_TITLES.try_find(self._group))
            case ClassSystem.OMNICLASS:
                return _OMNI_TABLES.try_find(self.segments[0]) if self.segments else Nothing
            case _ as unreachable:
                assert_never(unreachable)

    def division(self) -> Option[int]:
        return Some(self.segments[0]) if self.system is ClassSystem.MASTERFORMAT else Nothing

    def subgroup(self) -> Option[Subgroup]:
        return (
            _DIVISIONS.try_find(self.segments[0]).map(lambda division: division.subgroup)
            if self.system is ClassSystem.MASTERFORMAT and self.segments
            else Nothing
        )

    @property
    def _uf_anchor(self) -> str:
        return self._group + (f"{self.segments[0]:02d}" if self.segments else "")

    @property
    def _deepest(self) -> int:
        return next((at for at in reversed(range(len(self.segments))) if self.segments[at]), 0)

    @property
    def level(self) -> int:
        return 1 + len(self.segments) if self.system is ClassSystem.UNIFORMAT else self._deepest + 1 if self.segments else 0

    def parent(self) -> Option[Self]:
        match self.system:
            case ClassSystem.UNIFORMAT:
                return Some(structs.replace(self, payload=(self._group, self.segments[:-1]))) if self.segments else Nothing
            case ClassSystem.MASTERFORMAT if len(self.segments) > 3:
                return Some(structs.replace(self, payload=self.segments[:3]))
            case ClassSystem.OMNICLASS:
                return Some(structs.replace(self, payload=self.segments[:-1])) if len(self.segments) > 1 else Nothing
            case _:
                rolled = (*self.segments[: self._deepest], *(0 for _ in self.segments[self._deepest :]))
                return Some(structs.replace(self, payload=rolled)) if self._deepest else Nothing

    def descends_from(self, ancestor: "ClassCode", /) -> bool:
        if self.system is not ancestor.system:
            return False
        match self.system:
            case ClassSystem.MASTERFORMAT:
                return len(self.segments) >= len(ancestor.segments) and self.segments[0] == ancestor.segments[0] and all(
                    root in (0, current) for root, current in zip(ancestor.segments[1:], self.segments[1:], strict=False)
                )
            case ClassSystem.UNIFORMAT:
                return self._group == ancestor._group and self.segments[: len(ancestor.segments)] == ancestor.segments
            case ClassSystem.OMNICLASS:
                return self.segments[: len(ancestor.segments)] == ancestor.segments
            case _ as unreachable:
                assert_never(unreachable)

    def crosswalk(self) -> "CrossReference":
        match self.system:
            case ClassSystem.MASTERFORMAT:
                peers = _CROSSWALK.try_find(self.segments[0]).default_value(())
                return CrossReference(
                    self,
                    frozendict({
                        ClassSystem.UNIFORMAT: tuple(
                            (ClassCode(ClassSystem.UNIFORMAT, (code[0], _pairs(code[1:]))), ClassRelation.RELATED) for code in peers
                        ),
                        ClassSystem.OMNICLASS: ((ClassCode(ClassSystem.OMNICLASS, (_OMNI_WORK_RESULTS, *self.segments)), ClassRelation.EXACT),),
                    }),
                )
            case ClassSystem.UNIFORMAT:
                divisions = (
                    _ELEMENT_DIVISIONS.try_find(self._uf_anchor).default_value(())
                    if self.segments
                    else tuple(
                        dict.fromkeys(
                            division
                            for element in _ELEMENTS.values()
                            if element.group == self._group
                            for division in _ELEMENT_DIVISIONS.try_find(element.code).default_value(())
                        )
                    )
                )
                return CrossReference(
                    self,
                    frozendict({
                        ClassSystem.MASTERFORMAT: tuple(
                            (ClassCode(ClassSystem.MASTERFORMAT, (division, 0, 0)), ClassRelation.RELATED) for division in divisions
                        ),
                        ClassSystem.OMNICLASS: ((ClassCode(ClassSystem.OMNICLASS, (_OMNI_ELEMENTS,)), ClassRelation.BROADER),),
                    }),
                )
            case ClassSystem.OMNICLASS:
                table, *tail = self.segments or (0,)
                mirror = (
                    ((ClassCode(ClassSystem.MASTERFORMAT, tuple(tail)), ClassRelation.EXACT),)
                    if table == _OMNI_WORK_RESULTS and len(tail) in (3, 4) and tail[0] in _DIVISIONS
                    else ()
                )
                return CrossReference(self, frozendict({ClassSystem.MASTERFORMAT: mirror}) if mirror else frozendict())
            case _ as unreachable:
                assert_never(unreachable)


class CrossReference(Struct, frozen=True):
    source: ClassCode
    peers: frozendict[ClassSystem, tuple[ClassPeer, ...]] = frozendict()

    def peer(self, system: ClassSystem, /) -> tuple[ClassPeer, ...]:
        return self.peers.get(system, ())


# --- [CONSTANTS] ------------------------------------------------------------------------

_MF_PATTERN: Final[re.Pattern[str]] = re.compile(r"\A(\d{2})\s*(\d{2})\s*(\d{2})(?:\.(\d{2}))?\Z")
_UF_PATTERN: Final[re.Pattern[str]] = re.compile(r"\A(?P<group>[A-GZ])(?P<pairs>(?:\d{2})*)\Z")
_OC_PATTERN: Final[re.Pattern[str]] = re.compile(r"\A(?P<table>\d{2})(?:-(?P<tail>\d{2}(?:\s+\d{2})*))?\Z")

# --- [TABLES] ---------------------------------------------------------------------------

_GROUP_TITLES: Final[Map[str, str]] = Map.of_seq([
    ("A", "Substructure"),
    ("B", "Shell"),
    ("C", "Interiors"),
    ("D", "Services"),
    ("E", "Equipment and Furnishings"),
    ("F", "Special Construction and Demolition"),
    ("G", "Building Sitework"),
    ("Z", "General"),
])

_DIVISIONS: Final[Map[int, Division]] = Map.of_seq([
    (0, Division(0, "Procurement and Contracting Requirements", Subgroup.PROCUREMENT)),
    (1, Division(1, "General Requirements", Subgroup.GENERAL)),
    (2, Division(2, "Existing Conditions", Subgroup.CONSTRUCTION)),
    (3, Division(3, "Concrete", Subgroup.CONSTRUCTION)),
    (4, Division(4, "Masonry", Subgroup.CONSTRUCTION)),
    (5, Division(5, "Metals", Subgroup.CONSTRUCTION)),
    (6, Division(6, "Wood, Plastics, and Composites", Subgroup.CONSTRUCTION)),
    (7, Division(7, "Thermal and Moisture Protection", Subgroup.CONSTRUCTION)),
    (8, Division(8, "Openings", Subgroup.CONSTRUCTION)),
    (9, Division(9, "Finishes", Subgroup.CONSTRUCTION)),
    (10, Division(10, "Specialties", Subgroup.CONSTRUCTION)),
    (11, Division(11, "Equipment", Subgroup.CONSTRUCTION)),
    (12, Division(12, "Furnishings", Subgroup.CONSTRUCTION)),
    (13, Division(13, "Special Construction", Subgroup.CONSTRUCTION)),
    (14, Division(14, "Conveying Equipment", Subgroup.CONSTRUCTION)),
    (21, Division(21, "Fire Suppression", Subgroup.SERVICES)),
    (22, Division(22, "Plumbing", Subgroup.SERVICES)),
    (23, Division(23, "Heating, Ventilating, and Air Conditioning (HVAC)", Subgroup.SERVICES)),
    (25, Division(25, "Integrated Automation", Subgroup.SERVICES)),
    (26, Division(26, "Electrical", Subgroup.SERVICES)),
    (27, Division(27, "Communications", Subgroup.SERVICES)),
    (28, Division(28, "Electronic Safety and Security", Subgroup.SERVICES)),
    (31, Division(31, "Earthwork", Subgroup.INFRASTRUCTURE)),
    (32, Division(32, "Exterior Improvements", Subgroup.INFRASTRUCTURE)),
    (33, Division(33, "Utilities", Subgroup.INFRASTRUCTURE)),
    (34, Division(34, "Transportation", Subgroup.INFRASTRUCTURE)),
    (35, Division(35, "Waterway and Marine Construction", Subgroup.INFRASTRUCTURE)),
    (40, Division(40, "Process Interconnections", Subgroup.PROCESS)),
    (41, Division(41, "Material Processing and Handling Equipment", Subgroup.PROCESS)),
    (42, Division(42, "Process Heating, Cooling, and Drying Equipment", Subgroup.PROCESS)),
    (43, Division(43, "Process Gas and Liquid Handling, Purification, and Storage Equipment", Subgroup.PROCESS)),
    (44, Division(44, "Pollution and Waste Control Equipment", Subgroup.PROCESS)),
    (45, Division(45, "Industry-Specific Manufacturing Equipment", Subgroup.PROCESS)),
    (46, Division(46, "Water and Wastewater Equipment", Subgroup.PROCESS)),
    (48, Division(48, "Electrical Power Generation", Subgroup.PROCESS)),
])
_RESERVED: Final[frozenset[int]] = frozenset(range(50)) - frozenset(_DIVISIONS.keys())

_ELEMENTS: Final[Map[str, Element]] = Map.of_seq((
    (element.code, element) for element in (
        Element("A10", "Foundations", "A"),
        Element("A20", "Basement Construction", "A"),
        Element("B10", "Superstructure", "B"),
        Element("B20", "Exterior Enclosure", "B"),
        Element("B30", "Roofing", "B"),
        Element("C10", "Interior Construction", "C"),
        Element("C20", "Stairs", "C"),
        Element("C30", "Interior Finishes", "C"),
        Element("D10", "Conveying", "D"),
        Element("D20", "Plumbing", "D"),
        Element("D30", "HVAC", "D"),
        Element("D40", "Fire Protection", "D"),
        Element("D50", "Electrical", "D"),
        Element("E10", "Equipment", "E"),
        Element("E20", "Furnishings", "E"),
        Element("F10", "Special Construction", "F"),
        Element("F20", "Selective Building Demolition", "F"),
        Element("G10", "Site Preparation", "G"),
        Element("G20", "Site Improvements", "G"),
        Element("G30", "Site Mechanical Utilities", "G"),
        Element("G40", "Site Electrical Utilities", "G"),
        Element("G90", "Other Site Construction", "G"),
        Element("Z10", "General", "Z"),
    )
))

_OMNI_TABLES: Final[Map[int, str]] = Map.of_seq([
    (11, "Construction Entities by Function"),
    (12, "Construction Entities by Form"),
    (13, "Spaces by Function"),
    (14, "Spaces by Form"),
    (21, "Elements"),
    (22, "Work Results"),
    (23, "Products"),
    (31, "Phases"),
    (32, "Services"),
    (33, "Disciplines"),
    (34, "Organizational Roles"),
    (35, "Tools"),
    (36, "Information"),
    (41, "Materials"),
    (49, "Properties"),
])

_CROSSWALK: Final[Map[int, tuple[str, ...]]] = Map.of_seq([
    (2, ("F20", "G10")),
    (3, ("A10", "A20", "B10")),
    (4, ("B10", "B20")),
    (5, ("B10", "B20")),
    (6, ("B10", "B20", "C10")),
    (7, ("B20", "B30")),
    (8, ("B20", "C10")),
    (9, ("C30",)),
    (10, ("C10", "E20")),
    (11, ("E10",)),
    (12, ("E20",)),
    (13, ("F10",)),
    (14, ("D10",)),
    (21, ("D40",)),
    (22, ("D20",)),
    (23, ("D30",)),
    (25, ("D50",)),
    (26, ("D50",)),
    (27, ("D50",)),
    (28, ("D50",)),
    (31, ("A10", "G10")),
    (32, ("G20",)),
    (33, ("G30", "G40")),
    (34, ("G20",)),
    (35, ("G90",)),
])
_ELEMENT_DIVISIONS: Final[Map[str, tuple[int, ...]]] = Map.of_seq((
    (element.code, tuple(division for division, peers in _CROSSWALK.items() if element.code in peers)) for element in _ELEMENTS.values()
))
_OMNI_WORK_RESULTS: Final[int] = 22
_OMNI_ELEMENTS: Final[int] = 21

# --- [OPERATIONS] -----------------------------------------------------------------------


def _pairs(digits: str, /) -> tuple[int, ...]:
    return tuple(int(digits[at : at + 2]) for at in range(0, len(digits), 2))


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = ["ClassCode", "ClassFault", "ClassPeer", "ClassRelation", "ClassSystem", "CrossReference", "Division", "Element", "Subgroup"]
```

## [03]-[COORDINATE]

- Owner: `ReferenceIndex` admits deduplicated `Reference` links as its primary fact; `_indexes` derives both query directions together, so stored indexes cannot drift from their source.
- Cases: `SheetRef` carries a required sheet, optional detail, and regime-owned `Discipline`; `Reference` binds one classification to one sheet reference; `Coordination` carries matched links, undrawn specification sections, and unspecced drawing details.
- Entry: `ReferenceIndex.of` deduplicates admitted links. `resolve` discriminates `ClassCode`-to-sheets from `SheetRef`-to-sections and derives both indexes through `_indexes`; `coordinate` reconciles the primary links against specified sections.
- Auto: `coordinate` partitions the reference set against `specified` in one `Block.partition` so the `specifies` predicate runs once per reference, `orphan_sections` the filtered-into-`frozenset` of specified codes no reference reaches. `rows()` projects the flat records a `visualization/table#TABLE` `TablePlan.of` frame renders and `facts()` the native-int tally the composing producer folds into its OWN `ArtifactReceipt` — this owner computes the verdict, never the rendered artifact.
- Receipt: none — `ReferenceIndex`/`Coordination` are a pure reconciliation whose `rows`/`facts` a downstream `visualization/table#TABLE` or `specification/section#SECTION` producer renders and folds into its own receipt.
- Packages: `msgspec` (`Struct(frozen=True)` the value objects, `Reference`/`SheetRef` hashable for the admitted-set dedup); `expression` (`Map` the two indexes built by one fold, `Block.partition`/`filter` the one-pass reconciliation); `frozendict` (the flat projections and the orphan frozenset); `drawing/regime#REGIME` (`Discipline`) and the sibling `ClassCode` (`.render`/`.descends_from`), composed never re-declared.
- Growth: a new reference axis is one `Reference`/`SheetRef` field the fold and `rows` absorb; a new query modality is one `resolve` arm (`assert_never` forcing it); a new reconciliation category is one `Coordination` field; a new coordination fact is one `facts()` entry.
- Boundary: this owner computes the coordination, never the artifact — no rendered matrix (the `visualization/table#TABLE` renderer folds `rows`/`facts`), no receipt rail.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from builtins import frozendict
from collections.abc import Iterable
from typing import Self, assert_never, overload

from expression import Nothing, Option, Some
from expression.collections import Block, Map
from msgspec import Struct

from rasm.artifacts.drawing.regime import Discipline


# --- [MODELS] ---------------------------------------------------------------------------


class SheetRef(Struct, frozen=True):
    sheet: str
    detail: Option[str] = Nothing
    discipline: Discipline = Discipline.ARCHITECTURAL

    def __post_init__(self) -> None:
        match self.detail:
            case _ if not self.sheet.strip():
                raise ValueError("sheet reference requires a sheet")
            case Option(tag="some", some=detail) if not detail.strip():
                raise ValueError("optional detail reference must not be empty")
            case _:
                return


class Reference(Struct, frozen=True):
    section: ClassCode
    sheet: SheetRef


class Coordination(Struct, frozen=True):
    matched: tuple[Reference, ...] = ()
    orphan_sections: frozenset[ClassCode] = frozenset()
    orphan_details: tuple[SheetRef, ...] = ()

    def facts(self) -> frozendict[str, int]:
        return frozendict({"matched": len(self.matched), "orphan_sections": len(self.orphan_sections), "orphan_details": len(self.orphan_details)})

    def rows(self) -> tuple[frozendict[str, str], ...]:
        linked = tuple(
            frozendict({
                "section": ref.section.render(),
                "sheet": ref.sheet.sheet,
                "detail": ref.sheet.detail.default_value(""),
                "discipline": ref.sheet.discipline.value,
                "status": "matched",
            })
            for ref in self.matched
        )
        gaps_spec = tuple(
            frozendict({"section": code.render(), "sheet": "", "detail": "", "discipline": "", "status": "unresolved-section"})
            for code in sorted(self.orphan_sections, key=ClassCode.render)
        )
        gaps_draw = tuple(
            frozendict({"section": "", "sheet": ref.sheet, "detail": ref.detail.default_value(""), "discipline": ref.discipline.value, "status": "unresolved-detail"})
            for ref in self.orphan_details
        )
        return (*linked, *gaps_spec, *gaps_draw)


class ReferenceIndex(Struct, frozen=True):
    references: tuple[Reference, ...] = ()

    @classmethod
    def of(cls, references: Reference | Iterable[Reference], /) -> Self:
        return cls(references=tuple(dict.fromkeys(_normalized(references))))

    @staticmethod
    def _coordinate(sheet: SheetRef, /) -> tuple[str, str, str]:
        return (sheet.sheet, sheet.detail.default_value(""), sheet.discipline.value)

    def _indexes(self) -> tuple[Map[ClassCode, tuple[SheetRef, ...]], Map[tuple[str, str, str], tuple[ClassCode, ...]]]:
        def threaded(
            pair: tuple[Map[ClassCode, tuple[SheetRef, ...]], Map[tuple[str, str, str], tuple[ClassCode, ...]]], ref: Reference, /
        ) -> tuple[Map[ClassCode, tuple[SheetRef, ...]], Map[tuple[str, str, str], tuple[ClassCode, ...]]]:
            forward, inverse = pair
            return (
                forward.change(ref.section, lambda seen: Some((*seen.default_value(()), ref.sheet))),
                inverse.change(ReferenceIndex._coordinate(ref.sheet), lambda seen: Some((*seen.default_value(()), ref.section))),
            )

        return Block.of_seq(self.references).fold(threaded, (Map.empty(), Map.empty()))

    @overload
    def resolve(self, query: ClassCode, /) -> Block[SheetRef]: ...
    @overload
    def resolve(self, query: SheetRef, /) -> Block[ClassCode]: ...
    def resolve(self, query: ClassCode | SheetRef, /) -> Block[ClassCode] | Block[SheetRef]:
        match query:
            case ClassCode() as section:
                forward, _ = self._indexes()
                return Block.of_seq(sheet for keyed, sheets in forward.items() if keyed.descends_from(section) for sheet in sheets)
            case SheetRef() as sheet:
                _, inverse = self._indexes()
                return Block.of_seq(inverse.try_find(self._coordinate(sheet)).default_value(()))
            case _ as unreachable:
                assert_never(unreachable)

    def coordinate(self, specified: ClassCode | Iterable[ClassCode], /) -> Coordination:
        wanted, refs = _normalized(specified), Block.of_seq(self.references)

        def specifies(section: ClassCode, /) -> bool:
            return not wanted.filter(lambda spec: section.descends_from(spec)).is_empty()

        def referenced(code: ClassCode, /) -> bool:
            return not refs.filter(lambda ref: ref.section.descends_from(code)).is_empty()

        matched, orphan_refs = refs.partition(lambda ref: specifies(ref.section))
        return Coordination(matched=tuple(matched), orphan_sections=frozenset(wanted.filter(lambda code: not referenced(code))), orphan_details=tuple(orphan_refs.map(lambda ref: ref.sheet)))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _normalized[T](items: T | Iterable[T], /) -> Block[T]:
    match items:
        case Iterable() as stream if not isinstance(stream, (str, bytes)):
            return Block.of_seq(stream)
        case lone:
            return Block.singleton(lone)


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = ["Coordination", "Reference", "ReferenceIndex", "SheetRef"]
```

## [04]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
