# [PY_ARTIFACTS_CLASSIFY]

One owner holds the construction-classification semantics — the CSI/CSC vocabularies (`MasterFormat` work-results, `UniFormat` elemental, `OmniClass` faceted) and the drawing↔spec `ReferenceIndex` every specification, schedule, and drawing consumer keys against. `crosswalk` maps one code onto its cross-system peers; `resolve`/`coordinate` bind each specification section to the sheets that detail it. This owner authors no artifact and mints no receipt: it is the pure vocabulary-and-resolver substrate `specification/section#SECTION`, `drawing/schedule#SCHEDULE`, `drawing/detail#DETAIL`, and `drawing/annotate#ANNOTATE` compose, exactly as `exchange/detect#DETECT` is the format-ID gate the document owners read before dispatch.

`ClassCode` is one frozen `msgspec.Struct` value object spanning all three systems — a `ClassSystem` discriminant, the normalized numeric `segments`, and the `UniFormat` `group` letter — whose projections dispatch by system through one total `match`, never a per-system code triple and never a bare `str` the consumer re-splits. Three closed `frozendict` correspondence tables carry the vocabularies, authored to published cardinality; `crosswalk` derives every peer from one primary `_CROSSWALK` division→group row, its comprehension-derived `_GROUP_DIVISIONS` inverse, and the OmniClass table-alignment invariant (Table 22 IS `MasterFormat`, Table 21 IS `UniFormat`). Validated codes and their crosswalk peers travel INTO the composing producers' `ArtifactReceipt` facts — `section#SECTION` carries its section's `division`, `schedule#SCHEDULE` its classified line item — never a parallel classification receipt rail. No runtime import: the value object mints no `ContentKey` and the resolver runs no boundary.

## [01]-[INDEX]

- [02]-[CODE]: `ClassCode` — the cross-system classification value object and the three closed vocabularies, `crosswalk` folding a code onto its peers across systems.
- [03]-[COORDINATE]: `ReferenceIndex` — the drawing↔spec cross-reference half, `resolve` the one polymorphic query and `coordinate` the section-to-sheet reconciliation.

## [02]-[CODE]

- Owner: `ClassCode` — one frozen `msgspec.Struct` spanning `MasterFormat`/`UniFormat`/`OmniClass` on a `system` discriminant, never a per-system `MfCode`/`UfCode`/`OcCode` triple and never a bare `str` re-split at every read. `order=True` makes it a legal key for the `ReferenceIndex.forward` sorted `Map` and sorts a schedule in canonical classification order.
- Cases: the three vocabularies are closed `frozendict` correspondence tables authored to exact published cardinality — `_DIVISIONS` (the `MasterFormat` 2020 divisions over six `Subgroup` bands), `_ELEMENTS` (the `UniFormat` groups and their Level-2 elements), `_OMNI_TABLES` (the OmniClass tables); the reserved `MasterFormat` slots are the `range(50)` complement over the assigned set, never fabricated titles, each row a frozen value, never a stringly prefix probe.
- Entry: `ClassCode.parse(system, text)` returns `Result[Self, ClassFault]` — the system's module-level `re.Pattern` matches once, then the HEAD validates against its vocabulary, so a deeper code admits under a known broadscope parent (`UniFormat` `B1010` under element `B10`, `MasterFormat` `03 30 53` under Division `03`). A reserved slot, an out-of-range division, an unknown element, and an unknown table each surface as their own `ClassFault` case, the `_RESERVED` complement splitting the future-expansion slot from the invalid number.
- Auto: `crosswalk` derives the peer set from the one primary `_CROSSWALK` division→group row, its comprehension-derived `_GROUP_DIVISIONS` inverse, and the OmniClass table-alignment invariant — Table 22 Work Results IS `MasterFormat` (the exact `(22, *segments)` digit copy), Table 21 Elements IS `UniFormat` only semantically (the `(21,)` anchor, no digit copy Table 21's numeric notation forges). A new crosswalk edge is one primary row the inverse re-derives, never a parallel literal drifting out of sync. `level`/`parent` share the one `_deepest` projection so depth and roll-up derive from one computation.
- Receipt: none — `ClassCode` is a pure value object and `crosswalk` a pure projection; the validated code travels INTO the composing producers' facts, exactly as `exchange/detect#DETECT` and `drawing/regime#REGIME` contribute none.
- Packages: `msgspec` (`Struct(frozen=True, order=True)` the value object, hashable for the crosswalk keys; `structs.replace` the `parent` roll-up); `re` (the three system `Pattern`s compiled once, the `None` miss crossing the rail at `parse`); `frozendict` (the vocabulary tables and `_CROSSWALK` with its derived inverse); `expression` (the `Result`/`Option`/`Block` parse-and-lookup rails).
- Growth: a new division/element/table is one `_DIVISIONS`/`_ELEMENTS`/`_OMNI_TABLES` row; a new classification system is one `ClassSystem` member plus one `re.Pattern` and one arm on each projection (`assert_never` forcing each); a new crosswalk edge is one `_CROSSWALK` row the inverse absorbs; a new fault cause is one `ClassFault` case.
- Boundary: this owner authors classification semantics, never bytes — no `ArtifactReceipt` case (the composing producers carry the code in their own facts), no rendered artifact.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import re
from enum import StrEnum
from typing import Final, Literal, Self, assert_never

from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block, Map
from msgspec import Struct, structs

# --- [TYPES] ----------------------------------------------------------------------------


class ClassSystem(StrEnum):
    MASTERFORMAT = "masterformat"  # CSI/CSC work-results — the specification-section number
    UNIFORMAT = "uniformat"  # ASTM E1557 elemental — the building-element classification
    OMNICLASS = "omniclass"  # OCCS faceted — ISO 12006-2 aligned


class Subgroup(StrEnum):  # the MasterFormat 2020 subgroup a division belongs to
    PROCUREMENT = "procurement"  # Division 00
    GENERAL = "general"  # Division 01
    CONSTRUCTION = "construction"  # Divisions 02-19
    SERVICES = "services"  # Divisions 20-29
    INFRASTRUCTURE = "infrastructure"  # Divisions 30-39
    PROCESS = "process"  # Divisions 40-49


type ClassFault = Literal["<malformed-code>", "<unknown-division>", "<reserved-division>", "<unknown-element>", "<unknown-table>"]

# --- [MODELS] ---------------------------------------------------------------------------


class Division(Struct, frozen=True):  # one MasterFormat division row
    number: int
    title: str
    subgroup: Subgroup


class Element(Struct, frozen=True):  # one UniFormat elemental row
    code: str
    title: str
    group: str


class ClassCode(Struct, frozen=True, order=True):
    # `segments` the normalized numeric hierarchy (MasterFormat `(div, l2, l3)`, OmniClass `(table, *hierarchy)`,
    # UniFormat the Level-2+ digit pairs) and `group` the UniFormat Level-1 letter, empty for the numeric systems.
    system: ClassSystem
    segments: tuple[int, ...] = ()
    group: str = ""

    @classmethod
    def parse(cls, system: ClassSystem, text: str, /) -> Result[Self, ClassFault]:
        match system:
            case ClassSystem.MASTERFORMAT:
                if (found := _MF_PATTERN.match(text)) is None:
                    return Error("<malformed-code>")
                segments = tuple(int(part) for part in found.groups() if part is not None)
                if segments[0] in _DIVISIONS:
                    return Ok(cls(system, segments))
                # a reserved slot is valid-but-unassigned, distinct from out-of-range: `_RESERVED` splits the faults.
                return Error("<reserved-division>") if segments[0] in _RESERVED else Error("<unknown-division>")
            case ClassSystem.UNIFORMAT:
                if (found := _UF_PATTERN.match(text)) is None:
                    return Error("<malformed-code>")
                code = cls(system, _pairs(found["pairs"]), found["group"])
                # head-validate the Level-2 element anchor, so a Level-3 `B1010` admits under `B10`.
                return Ok(code) if not code.segments or code._uf_anchor in _ELEMENTS else Error("<unknown-element>")
            case ClassSystem.OMNICLASS:
                if (found := _OC_PATTERN.match(text)) is None:
                    return Error("<malformed-code>")
                tail = found["tail"] or ""
                segments = (int(found["table"]), *(int(pair) for pair in tail.split()))
                return Ok(cls(system, segments)) if segments[0] in _OMNI_TABLES else Error("<unknown-table>")
            case _ as unreachable:
                assert_never(unreachable)

    def render(self) -> str:
        match self.system:
            case ClassSystem.MASTERFORMAT:
                # the triple space-joins; a `.NN` level-4 suffix dot-joins so `03 30 53.13` round-trips `_MF_PATTERN`.
                head = " ".join(f"{part:02d}" for part in self.segments[:3])
                return head + "".join(f".{part:02d}" for part in self.segments[3:])
            case ClassSystem.UNIFORMAT:
                return self.group + "".join(f"{part:02d}" for part in self.segments)
            case ClassSystem.OMNICLASS:
                head = f"{self.segments[0]:02d}" if self.segments else ""
                tail = " ".join(f"{part:02d}" for part in self.segments[1:])
                return f"{head}-{tail}" if tail else head
            case _ as unreachable:
                assert_never(unreachable)

    def title(self) -> Option[str]:
        match self.system:
            case ClassSystem.MASTERFORMAT:
                return Some(_DIVISIONS[self.segments[0]].title) if self.segments and self.segments[0] in _DIVISIONS else Nothing
            case ClassSystem.UNIFORMAT:
                # the deepest known anchor: the Level-2 element title, falling to the Level-1 group title.
                return (
                    Some(found.title) if (found := _ELEMENTS.try_find(self._uf_anchor).default_value(None)) is not None else Option.of_optional(_GROUP_TITLES.try_find(self.group).default_value(None))
                )
            case ClassSystem.OMNICLASS:
                return Option.of_optional(_OMNI_TABLES.try_find(self.segments[0]).default_value(None)) if self.segments else Nothing
            case _ as unreachable:
                assert_never(unreachable)

    def division(self) -> int:
        # the MasterFormat division head the crosswalk keys on — a non-MasterFormat code projects `-1`,
        # distinct from every assigned `0..49` and never a raise on the wrong system.
        return self.segments[0] if self.system is ClassSystem.MASTERFORMAT and self.segments else -1

    def subgroup(self) -> Option[Subgroup]:
        # the MasterFormat 2020 subgroup read off the one `_DIVISIONS` row rather than a parallel map;
        # `Nothing` for the non-MasterFormat systems and a reserved/out-of-range division.
        found = _DIVISIONS.try_find(self.segments[0]).default_value(None) if self.system is ClassSystem.MASTERFORMAT and self.segments else None
        return Some(found.subgroup) if found is not None else Nothing

    @property
    def _uf_anchor(self) -> str:
        # the UniFormat Level-2 element anchor (group + first digit-pair) `parse` and `title` widen from, so
        # admission and titling reach Level-3+ codes.
        return self.group + (f"{self.segments[0]:02d}" if self.segments else "")

    @property
    def _deepest(self) -> int:
        # the highest positional index carrying a non-zero segment — the trailing-zero-insensitive depth
        # `level` and `parent` both derive from.
        return next((at for at in reversed(range(len(self.segments))) if self.segments[at]), 0)

    @property
    def level(self) -> int:
        # the hierarchy depth: the UniFormat group plus its appended pairs, or the deepest significant
        # positional segment of the numeric systems — a division/table/group is `1`.
        return 1 + len(self.segments) if self.system is ClassSystem.UNIFORMAT else self._deepest + 1 if self.segments else 0

    def parent(self) -> Option[Self]:
        # the immediate hierarchy container: a MasterFormat `.NN` level-4 truncates to level-3, the numeric
        # triple zeros the deepest significant level (`03 30 53` -> `03 30 00` -> `03 00 00`), UniFormat drops
        # the last pair (`B1010` -> `B10` -> `B`), `Nothing` at the root.
        match self.system:
            case ClassSystem.UNIFORMAT:
                return Some(structs.replace(self, segments=self.segments[:-1])) if self.segments else Nothing
            case ClassSystem.MASTERFORMAT if len(self.segments) > 3:
                return Some(structs.replace(self, segments=self.segments[:3]))
            case _:
                rolled = (*self.segments[: self._deepest], *(0 for _ in self.segments[self._deepest :]))
                return Some(structs.replace(self, segments=rolled)) if self._deepest else Nothing

    def descends_from(self, ancestor: "ClassCode", /) -> bool:
        # a code descends from an ancestor when they share system and group, this code is at least as deep, and
        # every SIGNIFICANT ancestor segment matches — a `0` ancestor segment is the unspecified-level wildcard
        # (`03 00 00` contains `03 30 00`), where a flat prefix equality misses the trailing-zero heading.
        # The `>=` depth makes it REFLEXIVE, so a consumer never re-checks the exact match.
        return (
            self.system is ancestor.system
            and self.group == ancestor.group
            and len(self.segments) >= len(ancestor.segments)
            and all(anc in (0, seg) for anc, seg in zip(ancestor.segments, self.segments, strict=False))
        )

    def crosswalk(self) -> "CrossReference":
        # a code with no crosswalk edge yields an empty peer set rather than a raise.
        match self.system:
            case ClassSystem.MASTERFORMAT:
                group = _CROSSWALK.try_find(self.division()).default_value("")
                elements = Block.of_seq(_ELEMENTS.values()).filter(lambda element: element.group == group)
                return CrossReference(
                    self,
                    frozendict({
                        ClassSystem.UNIFORMAT: tuple(ClassCode(ClassSystem.UNIFORMAT, _pairs(element.code[1:]), group) for element in elements),
                        ClassSystem.OMNICLASS: (ClassCode(ClassSystem.OMNICLASS, (_OMNI_WORK_RESULTS, *self.segments)),),
                    }),
                )
            case ClassSystem.UNIFORMAT:
                divisions = _GROUP_DIVISIONS.try_find(self.group).default_value(())
                return CrossReference(
                    self,
                    frozendict({
                        ClassSystem.MASTERFORMAT: tuple(ClassCode(ClassSystem.MASTERFORMAT, (division, 0, 0)) for division in divisions),
                        ClassSystem.OMNICLASS: (ClassCode(ClassSystem.OMNICLASS, (_OMNI_ELEMENTS,)),),
                    }),
                )
            case ClassSystem.OMNICLASS:
                table, *tail = self.segments or (0,)
                mirror = (ClassCode(ClassSystem.MASTERFORMAT, tuple(tail)),) if table == _OMNI_WORK_RESULTS and len(tail) >= 3 else ()
                return CrossReference(self, frozendict({ClassSystem.MASTERFORMAT: mirror}) if mirror else frozendict())
            case _ as unreachable:
                assert_never(unreachable)


class CrossReference(Struct, frozen=True):
    # one code folded onto its per-system peer set.
    source: ClassCode
    peers: frozendict[ClassSystem, tuple[ClassCode, ...]] = frozendict()

    def peer(self, system: ClassSystem, /) -> tuple[ClassCode, ...]:
        return self.peers.get(system, ())


# --- [CONSTANTS] ------------------------------------------------------------------------

_MF_PATTERN: Final[re.Pattern[str]] = re.compile(r"\A(\d{2})\s*(\d{2})\s*(\d{2})(?:\.(\d{2}))?\Z")
_UF_PATTERN: Final[re.Pattern[str]] = re.compile(r"\A(?P<group>[A-GZ])(?P<pairs>(?:\d{2})*)\Z")
_OC_PATTERN: Final[re.Pattern[str]] = re.compile(r"\A(?P<table>\d{2})(?:-(?P<tail>\d{2}(?:\s+\d{2})*))?\Z")

# --- [TABLES] ---------------------------------------------------------------------------

# the MasterFormat 2020 ASSIGNED divisions; the reserved gaps are the `range(50)` complement, never a title.
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
# the reserved MasterFormat slots `parse` maps to `<reserved-division>`, distinct from `<unknown-division>`.
_RESERVED: Final[frozenset[int]] = frozenset(range(50)) - frozenset(_DIVISIONS.keys())

# the UniFormat Level-1 group titles (ASTM E1557 + CSI Z General).
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

# the OmniClass tables (OCCS / ISO 12006-2), keyed by table number.
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

# the ONE primary crosswalk correspondence: MasterFormat division -> UniFormat Level-1 group. `_GROUP_DIVISIONS`
# derives the inverse; OmniClass peers derive from the table-alignment invariant in `crosswalk`, never a third map.
_CROSSWALK: Final[Map[int, str]] = Map.of_seq([
    (2, "G"),
    (3, "B"),
    (4, "B"),
    (5, "B"),
    (6, "B"),
    (7, "B"),
    (8, "B"),
    (9, "C"),
    (10, "C"),
    (11, "E"),
    (12, "E"),
    (13, "F"),
    (14, "D"),
    (21, "D"),
    (22, "D"),
    (23, "D"),
    (25, "D"),
    (26, "D"),
    (27, "D"),
    (28, "D"),
    (31, "G"),
    (32, "G"),
    (33, "G"),
    (34, "G"),
    (35, "G"),
])
_GROUP_DIVISIONS: Final[Map[str, tuple[int, ...]]] = Map.of_seq((
    (group, tuple(division for division, mapped in _CROSSWALK.items() if mapped == group)) for group in frozenset(_CROSSWALK.values())
))
_OMNI_WORK_RESULTS: Final[int] = 22  # OmniClass Table 22 == MasterFormat (exact digit copy)
_OMNI_ELEMENTS: Final[int] = 21  # OmniClass Table 21 == UniFormat (elemental alignment, notation-distinct)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _pairs(digits: str, /) -> tuple[int, ...]:
    # split a UniFormat digit tail into its Level-2+ pair segments ("1010" -> (10, 10)).
    return tuple(int(digits[at : at + 2]) for at in range(0, len(digits), 2))
```

## [03]-[COORDINATE]

- Owner: `ReferenceIndex` — the drawing↔spec cross-reference owner admitting the `Reference` link set (one specification `ClassCode` bound to one `SheetRef`) ONCE into a bidirectional index at `of`, so `resolve`/`coordinate` read pre-built indexes rather than re-folding per call.
- Cases: `SheetRef` (`sheet`, `detail`, `discipline`) the drawing-side reference, its `Discipline` the `drawing/regime#REGIME` layer codec owns (composed, never a parallel stringly discipline); `Reference` the one folded link; `Coordination` the reconciliation triple — `matched`, `orphan_sections` (specced but drawn nowhere), `orphan_details` (keynoted but never specced) — the two silent-truncation gaps a manual owes its drawing set, surfaced as evidence.
- Entry: `ReferenceIndex.of(references)` folds both directions once — `forward` keyed on `ref.section`, `inverse` on `ref.sheet.sheet` (the sheet NUMBER, so a sheet query gathers every governing section regardless of detail/discipline). `resolve(query)` is ONE polymorphic method discriminating a `ClassCode`→sheets query (widened through `descends_from` so a division sweeps every descendant) from a `SheetRef`→sections query, never a `sheets_for`/`sections_for` sibling pair. `coordinate(specified)` reconciles the admitted set against the specified sections.
- Auto: `coordinate` partitions the reference set against `specified` in one `Block.partition` so the `specifies` predicate runs once per reference, `orphan_sections` the filtered-into-`frozenset` of specified codes no reference reaches. `rows()` projects the flat records a `visualization/table#TABLE` `TableNode` renders and `facts()` the native-int tally the composing producer folds into its OWN `ArtifactReceipt` — this owner computes the verdict, never the rendered artifact.
- Receipt: none — `ReferenceIndex`/`Coordination` are a pure reconciliation whose `rows`/`facts` a downstream `visualization/table#TABLE` or `specification/section#SECTION` producer renders and folds into its own receipt.
- Packages: `msgspec` (`Struct(frozen=True)` the value objects, `SheetRef` hashable for the reference dedup); `expression` (`Map` the two indexes built by one fold, `Block.partition`/`filter` the one-pass reconciliation); `frozendict` (the flat projections and the orphan frozenset); `drawing/regime#REGIME` (`Discipline`) and the sibling `ClassCode` (`.render`/`.descends_from`), composed never re-declared.
- Growth: a new reference axis is one `Reference`/`SheetRef` field the fold and `rows` absorb; a new query modality is one `resolve` arm (`assert_never` forcing it); a new reconciliation category is one `Coordination` field; a new coordination fact is one `facts()` entry.
- Boundary: this owner computes the coordination, never the artifact — no rendered matrix (the `visualization/table#TABLE` renderer folds `rows`/`facts`), no receipt rail.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable
from typing import Self, assert_never, overload

from expression import Some
from expression.collections import Block, Map
from msgspec import Struct

from artifacts.drawing.standard import Discipline
from artifacts.specification.classify import ClassCode

# --- [MODELS] ---------------------------------------------------------------------------


class SheetRef(Struct, frozen=True):
    # a sheet number, an optional detail-bubble id, and the `Discipline` the `drawing/regime#REGIME` codec owns.
    sheet: str
    detail: str = ""
    discipline: Discipline = Discipline.ARCHITECTURAL


class Reference(Struct, frozen=True):  # one keynote link: this section is detailed on this sheet
    section: ClassCode
    sheet: SheetRef


class Coordination(Struct, frozen=True):
    # the drawing<->spec reconciliation: the specified links, the sections specced but drawn nowhere, and the
    # sheets keynoted against a section the manual never specifies.
    matched: tuple[Reference, ...] = ()
    orphan_sections: frozenset[ClassCode] = frozenset()
    orphan_details: tuple[SheetRef, ...] = ()

    def facts(self) -> frozendict[str, int]:
        # the native-int tally the composing producer folds into its own ArtifactReceipt (this owner mints none).
        return frozendict({"matched": len(self.matched), "orphan_sections": len(self.orphan_sections), "orphan_details": len(self.orphan_details)})

    def rows(self) -> tuple[frozendict[str, str], ...]:
        # the flat tabular egress — one row per matched link, one flagged row per orphan section and detail.
        linked = tuple(
            frozendict({
                "section": ref.section.render(),
                "sheet": ref.sheet.sheet,
                "detail": ref.sheet.detail,
                "discipline": ref.sheet.discipline.value,
                "status": "matched",
            })
            for ref in self.matched
        )
        gaps_spec = tuple(
            frozendict({"section": code.render(), "sheet": "", "detail": "", "discipline": "", "status": "unresolved-section"})
            for code in self.orphan_sections
        )
        gaps_draw = tuple(
            frozendict({"section": "", "sheet": ref.sheet, "detail": ref.detail, "discipline": ref.discipline.value, "status": "unresolved-detail"})
            for ref in self.orphan_details
        )
        return (*linked, *gaps_spec, *gaps_draw)


class ReferenceIndex(Struct, frozen=True):
    # both directions built by one fold at `of`: `forward` (section -> sheets) and `inverse` (sheet NUMBER ->
    # sections), and `references` the admitted set `coordinate` reconciles against a specified-section set.
    forward: Map[ClassCode, tuple[SheetRef, ...]] = Map.empty()
    inverse: Map[str, tuple[ClassCode, ...]] = Map.empty()
    references: tuple[Reference, ...] = ()

    @classmethod
    def of(cls, references: Reference | Iterable[Reference], /) -> Self:
        held = _normalized(references)
        forward = held.fold(lambda index, ref: index.change(ref.section, lambda seen: Some((*seen.default_value(()), ref.sheet))), Map.empty())
        inverse = held.fold(lambda index, ref: index.change(ref.sheet.sheet, lambda seen: Some((*seen.default_value(()), ref.section))), Map.empty())
        return cls(forward=forward, inverse=inverse, references=tuple(held))

    @overload
    def resolve(self, query: ClassCode, /) -> Block[SheetRef]: ...
    @overload
    def resolve(self, query: SheetRef, /) -> Block[ClassCode]: ...
    def resolve(self, query: ClassCode | SheetRef, /) -> Block[ClassCode] | Block[SheetRef]:
        # discriminate on the query shape: a `ClassCode` gathers the detailing sheets (widened by
        # `descends_from` so a division sweeps every descendant), a `SheetRef` the governing sections.
        match query:
            case ClassCode() as section:
                return Block.of_seq(sheet for keyed, sheets in self.forward.items() if keyed.descends_from(section) for sheet in sheets)
            case SheetRef() as sheet:
                return Block.of_seq(self.inverse.try_find(sheet.sheet).default_value(()))
            case _ as unreachable:
                assert_never(unreachable)

    def coordinate(self, specified: ClassCode | Iterable[ClassCode], /) -> Coordination:
        # a reference matches when the manual specifies its section or an ancestor of it; `Block.partition`
        # splits matched from orphan in one pass, membership a non-empty filtered block (`Block` has no `exists`).
        wanted, refs = _normalized(specified), Block.of_seq(self.references)

        def specifies(section: ClassCode, /) -> bool:
            return not wanted.filter(lambda spec: section.descends_from(spec)).is_empty()

        def referenced(code: ClassCode, /) -> bool:
            return not refs.filter(lambda ref: ref.section.descends_from(code)).is_empty()

        matched, orphan_refs = refs.partition(lambda ref: specifies(ref.section))
        orphan_sections = frozenset(wanted.filter(lambda code: not referenced(code)))
        return Coordination(matched=tuple(matched), orphan_sections=orphan_sections, orphan_details=tuple(orphan_refs.map(lambda ref: ref.sheet)))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _normalized[T](items: T | Iterable[T], /) -> Block[T]:
    # the one modal-arity head — a lone `Reference`/`ClassCode` the singleton, any container the multi case;
    # never a `single`/`many` suffix pair.
    match items:
        case Iterable() as stream if not isinstance(stream, (str, bytes)):
            return Block.of_seq(stream)
        case lone:
            return Block.singleton(lone)
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
