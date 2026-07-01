# [PY_ARTIFACTS_CLASSIFY]

The construction-classification semantic authority: the one owner over the CSI/CSC classification vocabularies (`MasterFormat` work-results, `UniFormat` elemental, `OmniClass` faceted) and the drawing↔spec `ReferenceIndex` every specification, schedule, and drawing consumer keys against — the classification `crosswalk` that maps one code onto its peers across systems, and the bidirectional `resolve`/`coordinate` that binds each specification section to the sheets that detail it. This owner authors NO artifact and mints NO `core/receipt#RECEIPT` case: it is the pure vocabulary-and-resolver substrate `specification/section#SECTION`, `drawing/schedule#SCHEDULE`, `drawing/detail#DETAIL`, and `drawing/annotate#ANNOTATE` compose exactly as `exchange/detect#DETECT` is the format-ID gate the document owners read before dispatch — the classification codes it validates and crosswalks travel INTO those producers' `ArtifactReceipt` facts, never a parallel classification receipt rail.

`ClassCode` is ONE frozen `msgspec.Struct` value object spanning all three systems — a closed `ClassSystem` discriminant plus the normalized numeric `segments` and the `UniFormat` `group` letter — whose `parse`/`render`/`title`/`division`/`subgroup`/`level`/`parent`/`descends_from`/`crosswalk` projections dispatch by system through one total `match`, never a per-system `MfCode`/`UfCode`/`OcCode` sibling triple. Each system's notation is one module-level `re.Pattern` (MasterFormat `NN NN NN`, UniFormat `X` + digit pairs, OmniClass `NN-NN NN NN` with the table alone valid), so a raw code crosses `parse` once into the validated value — HEAD-validated at its `MasterFormat` division, `UniFormat` Level-2 element anchor, or `OmniClass` table so a deeper Level-3+ code admits whenever its broadscope parent is a known vocabulary member — and every interior consumer reads the typed `ClassCode` rather than re-splitting a string, the `BOUNDARY_ADMISSION` lifecycle applied to a classification token. Absence and malformation ride the rail: an unparseable notation, a reserved-but-unassigned division, an out-of-range division, an unknown element, or an unknown table each surface as one `ClassFault` `Literal` case on `Result`, never a `None` sentinel or a raised `ValueError` into the interior.

The three vocabularies are closed owners authored to their exact published cardinalities — the 35 assigned `MasterFormat` 2020 divisions across six subgroups (the reserved 15-20/24/29/30/36-39/47/49 gaps are the derived complement over `range(50)`, never fabricated titles), the `UniFormat` Level-1 `A`-`G`+`Z` groups and their Level-2 elements, and the 15 `OmniClass` tables — carried as `frozendict` correspondence tables, never a stringly `.startswith("03")` prefix probe. The `crosswalk` is the `DERIVED_LOGIC` law across the classification seam: ONE primary `_CROSSWALK` `frozendict` mapping each `MasterFormat` division to its `UniFormat` group is declared, the inverse `_GROUP_DIVISIONS` derives by comprehension, and the `OmniClass` peers derive from the table-alignment invariant (Table 22 Work Results IS `MasterFormat`, so its peer is an exact digit copy; Table 21 Elements IS `UniFormat` only semantically, so its peer is the table anchor no digit copy can forge), so a new crosswalk edge lands as one primary row and every inverse re-derives with no consumer edited.

The drawing↔spec resolver is the coordination half no single classification system spells: a `Reference` binds one specification `ClassCode` to one `SheetRef` (a keynote on a sheet citing the governing section), and `ReferenceIndex.of` admits the whole reference set ONCE into both a `forward` section→sheets index AND an `inverse` sheet-number→sections index built in one fold, so `resolve` — ONE polymorphic entrypoint discriminating on the query's own shape — reads a pre-built index rather than re-folding per query: a `ClassCode` resolves to the sheets detailing that section, a `SheetRef` resolves by its sheet number to the sections governing that sheet, never a `sheets_for`/`sections_for` sibling pair and never a per-query index rebuild. `coordinate` folds that same admitted reference set against the specified sections into the reconciliation every project manual owes its drawing set — the `matched` links, the `orphan_sections` specced but never drawn, and the `orphan_details` keynoted but never specced — the QA verdict a downstream `visualization/table#TABLE` or `specification/section#SECTION` renders, projected as flat `rows` and a native-int `facts` tally the tabular consumer ingests, this owner computing the reconciliation and never the rendering.

## [01]-[INDEX]

- [02]-[CODE]: `ClassCode` — the one cross-system classification value object (`ClassSystem` discriminant + numeric `segments` + `UniFormat` `group` letter) with `parse`/`render`/`title`/`division`/`subgroup`/`level`/`parent`/`descends_from`/`crosswalk` projections over one total `match`; the closed `MasterFormat` `Division`/`Subgroup`, `UniFormat` `Element`, and `OmniClass` table vocabularies as `frozendict` correspondence tables authored to published cardinality; and `crosswalk` folding one code onto its `CrossReference` peer set across systems through the primary `_CROSSWALK` correspondence and the OmniClass table-alignment invariant, the classification substrate every AEC producer validates a code against and every schedule rolls a keynote up through.
- [03]-[COORDINATE]: `ReferenceIndex` — the drawing↔spec cross-reference half admitting the `Reference`/`SheetRef` link set ONCE into a bidirectional index; `resolve(query)` the one polymorphic method discriminating a `ClassCode`→sheets query from a `SheetRef`→sections query over the `expression.Map` `forward`/`inverse` pair built once; and `coordinate(specified)` the reconciliation fold projecting the `Coordination` `matched`/`orphan_sections`/`orphan_details` and its flat `rows` plus native-int `facts` a `visualization/table#TABLE` or `specification/section#SECTION` consumer renders and folds into its own receipt, computing the coordination verdict and never the artifact.

## [02]-[CODE]

- Owner: `ClassCode` the one frozen `msgspec.Struct` classification value object spanning `MasterFormat`/`UniFormat`/`OmniClass` — a `system: ClassSystem` discriminant, the `segments: tuple[int, ...]` normalized numeric hierarchy (the `MasterFormat` `(division, level2, level3)` triple, the `UniFormat` Level-2+ digit pairs, the `OmniClass` `(table, *hierarchy)` sequence), and the `group: str` `UniFormat` Level-1 letter (empty for the numeric systems) — never a per-system `MfCode`/`UfCode`/`OcCode` triple the interior re-discriminates, and never a bare `str` the consumer re-splits. Its `parse` admits a raw notation string through the system's module-level `re.Pattern` into the validated value, `render` formats the value back to canonical notation, `title` looks up the anchor vocabulary title, `division` projects the `MasterFormat` division head (the crosswalk key), `subgroup` the `MasterFormat` 2020 six-subgroup band (`Option[Subgroup]`) a specification project-manual table of contents groups sections by, `level` projects the trailing-zero-insensitive hierarchy depth, `parent` rolls the code up one level (the `Option[Self]` a schedule groups keynotes by), `descends_from` is the reflexive containment predicate a keynote-scope query reads, and `crosswalk` folds the code onto its cross-system `CrossReference` peers — every projection one total `match` over the three-member `ClassSystem`, closed by `assert_never`.
- Cases: the three vocabularies are closed `frozendict` correspondence owners authored to exact published cardinality. `Division` (`number`, `title`, `subgroup`) is the `MasterFormat` 2020 division row — the `_DIVISIONS` table carrying the 35 ASSIGNED divisions across the six `Subgroup` members (`PROCUREMENT` Div 00, `GENERAL` Div 01, `CONSTRUCTION` Div 02-19, `SERVICES` Div 20-29, `INFRASTRUCTURE` Div 30-39, `PROCESS` Div 40-49), the reserved numbers the derived complement over `range(50)` rather than fabricated titles. `Element` (`code`, `title`, `group`) is the `UniFormat` elemental row — the `_ELEMENTS` table carrying the Level-1 `A`-`G`+`Z` groups and their canonical Level-2 elements (`A10` Foundations … `G90` Other Site Construction). `_OMNI_TABLES` is the `OmniClass` faceted vocabulary — the 15 tables (`11`/`12` Construction Entities by Function/Form, `13`/`14` Spaces by Function/Form, `21` Elements, `22` Work Results, `23` Products, `31` Phases, `32` Services, `33` Disciplines, `34` Organizational Roles, `35` Tools, `36` Information, `41` Materials, `49` Properties). Each row is a frozen `Struct` or `frozendict` value, never a stringly prefix probe.
- Entry: `ClassCode.parse(system, text)` returns `Result[Self, ClassFault]` — the system's module-level `re.Pattern` (`_MF_PATTERN` the `NN NN NN` triple with an optional `.NN` level-4, `_UF_PATTERN` the `X` group plus zero or more digit pairs, `_OC_PATTERN` the `NN` table prefix plus an OPTIONAL `-` and space-separated pairs so the bare table anchor round-trips) matches once, a miss projecting to `<malformed-code>`, and the HEAD validates against its vocabulary: `MasterFormat` against `_DIVISIONS[segments[0]]`, `OmniClass` against `_OMNI_TABLES[segments[0]]`, and `UniFormat` against the `_uf_anchor` Level-2 element (`group` + first digit-pair) in `_ELEMENTS` — so a deeper `UniFormat` Level-3 code (`B1010`) admits when its Level-2 element (`B10`) is known exactly as a `MasterFormat` `03 30 53` admits under Division `03`, and a reserved MasterFormat slot (`<reserved-division>`), an out-of-range division (`<unknown-division>`), an unknown element (`<unknown-element>`), and an unknown table (`<unknown-table>`) each surface as their own `ClassFault` case, the `_RESERVED` complement splitting the future-expansion slot from the invalid number. `render` is the inverse projection formatting the canonical notation; `crosswalk` is the peer resolver folding one code onto a `CrossReference`.
- Auto: `crosswalk` derives the `CrossReference` peer set from one primary correspondence and the OmniClass table-alignment invariant, never a hand-kept per-system map. A `MasterFormat` code resolves its `UniFormat` group through `_CROSSWALK[code.division]` (the primary division→group row) yielding each group's Level-2 elements, and its `OmniClass` peer through the Table 22 = Work Results = MasterFormat identity so `(22, *code.segments)` is an exact digit copy; a `UniFormat` code resolves its `MasterFormat` divisions through the derived `_GROUP_DIVISIONS` inverse and its `OmniClass` peer through the Table 21 anchor `(21,)` — the aligned table without a forged digit copy, because Table 21 Elements uses numeric notation `UniFormat`'s alpha-numeric element codes do NOT map onto digit-for-digit; an `OmniClass` Table-22 code mirrors back to its exact `MasterFormat` digits. `_GROUP_DIVISIONS` is the one secondary map DERIVED by comprehension from `_CROSSWALK.items()` — a new crosswalk edge is one primary row and the inverse re-derives, never a parallel literal drifting out of sync. `level`/`parent` share one `_deepest` significant-index projection so the hierarchy depth and the roll-up derive from one computation.
- Receipt: none. `ClassCode` is a pure value object and `crosswalk` a pure projection — the validated code and its crosswalk peers travel INTO the composing producers' facts (the `specification/section#SECTION` `Spec` receipt carries its section's `division`, the `drawing/schedule#SCHEDULE` rows carry the classified line item), never an `ArtifactReceipt` case this owner mints. This owner is the classification substrate, not a production sub-domain, exactly as `exchange/detect#DETECT` and `drawing/standard#STANDARD` contribute no receipt.
- Packages: `msgspec` (`Struct(frozen=True, order=True)` the `ClassCode` value object — hashable for the `frozenset`/crosswalk keys AND ordered so it keys the `ReferenceIndex.forward` sorted `expression.Map` and sorts a schedule in canonical classification order — `Struct(frozen=True)` the `Division`/`Element` rows, `structs.replace` the `parent` roll-up transition); `re` (the three module-level system `Pattern`s, compiled once, the `None`-miss crossing the rail at `parse`); `frozendict` (`_DIVISIONS`/`_ELEMENTS`/`_OMNI_TABLES` the vocabulary tables and `_CROSSWALK` the primary correspondence with `_GROUP_DIVISIONS` its derived inverse); `expression` (`Result`/`Option`/`Ok`/`Error`/`Some`/`Nothing` the parse and lookup rails, `Block` the crosswalk peer traversal). No runtime import — the value object mints no `ContentKey` and the resolver runs no boundary.
- Growth: a new `MasterFormat` division is one `_DIVISIONS` row; a new `UniFormat` element is one `_ELEMENTS` row; a new `OmniClass` table is one `_OMNI_TABLES` row; a new classification system is one `ClassSystem` member plus one `re.Pattern`, one `render`/`title`/`level`/`parent` arm, and one `parse` arm (the `assert_never` tail forcing each); a new crosswalk edge is one `_CROSSWALK` primary row the `_GROUP_DIVISIONS` inverse absorbs for free; a new fault cause is one `ClassFault` `Literal` case. Zero new surface — the value object owns every system through one discriminant and the crosswalk grows by table row.
- Boundary: a per-system `MfCode`/`UfCode`/`OcCode` value triple where one `ClassCode` discriminates on `system` is the deleted fragmentation; a bare-`str` classification code re-split at every read where `parse` admits it once is the deleted stringly form; a full-render UniFormat membership test that rejects every valid Level-3 code where `_uf_anchor` head-validates the Level-2 element is the deleted over-strict admission; a `.startswith("03 30")` division probe where `code.division` projects the head is the deleted prefix hack; a hand-kept `_DIVISION_GROUP` beside its `_GROUP_DIVISIONS` inverse where the comprehension derives one from the other is the deleted parallel map; a forged `(21, *segments)` OmniClass digit copy where Table 21 uses notation `UniFormat` never maps onto is the deleted invention; a fabricated title for a reserved `MasterFormat` division where the assigned-only table plus the `range(50)` complement states the gap is the deleted invention; a dead `<no-crosswalk>` fault no arm ever returns where the empty peer set states the absence is the deleted vocabulary member; a `crosswalk(code)` free function where `code.crosswalk()` reads the owner's own peers is the deleted hop; a `None`-for-unknown-division lookup where `Result[ClassCode, ClassFault]` carries the cause is the deleted sentinel; an `ArtifactReceipt.Classification` rail where the composing producers carry the code in their own facts is the deleted parallel receipt. This owner authors semantics, never bytes.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import re
from enum import StrEnum
from typing import Final, Literal, Self, assert_never

from builtins import frozendict
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block
from msgspec import Struct, structs

# --- [TYPES] ----------------------------------------------------------------------------


class ClassSystem(StrEnum):
    MASTERFORMAT = "masterformat"   # CSI/CSC work-results — the specification-section number
    UNIFORMAT = "uniformat"         # ASTM E1557 / CSI elemental — the building-element classification
    OMNICLASS = "omniclass"         # OCCS faceted — the 15-table ISO 12006-2 aligned classification


class Subgroup(StrEnum):            # the MasterFormat 2020 subgroup a division belongs to
    PROCUREMENT = "procurement"        # Division 00
    GENERAL = "general"                # Division 01
    CONSTRUCTION = "construction"      # Divisions 02-19 (Facility Construction)
    SERVICES = "services"              # Divisions 20-29 (Facility Services)
    INFRASTRUCTURE = "infrastructure"  # Divisions 30-39 (Site and Infrastructure)
    PROCESS = "process"                # Divisions 40-49 (Process Equipment)


type ClassFault = Literal["<malformed-code>", "<unknown-division>", "<reserved-division>", "<unknown-element>", "<unknown-table>"]

# --- [MODELS] ---------------------------------------------------------------------------


class Division(Struct, frozen=True):        # one MasterFormat division row — number, title, subgroup
    number: int
    title: str
    subgroup: Subgroup


class Element(Struct, frozen=True):         # one UniFormat elemental row — code, title, Level-1 group
    code: str
    title: str
    group: str


class ClassCode(Struct, frozen=True, order=True):
    # the one cross-system code: `segments` the normalized numeric hierarchy (MasterFormat `(div, l2, l3)`,
    # OmniClass `(table, *hierarchy)`, UniFormat the Level-2+ digit pairs) and `group` the UniFormat Level-1
    # letter (empty for the numeric systems) — never a per-system code triple, never a re-split string.
    # `order=True` (system, then segments, then group) makes it a legal key for the `ReferenceIndex.forward`
    # sorted `expression.Map` and sorts the schedule/crosswalk in canonical classification order.
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
                # a reserved MasterFormat slot (15-20/24/29/30/36-39/47/49) is a valid-but-unassigned number
                # distinct from an out-of-range one, so the `_RESERVED` complement splits the two faults.
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
                # the `(division, level2, level3)` triple is space-joined; a `.NN` level-4 suffix is dot-joined
                # so `03 30 53.13` round-trips through `_MF_PATTERN`'s optional `(?:\.(\d{2}))?` group.
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
                # the deepest known anchor: the Level-2 element title (`B1010` -> "Superstructure"), falling
                # to the Level-1 group title when only the group is known.
                return (
                    Some(found.title) if (found := _ELEMENTS.get(self._uf_anchor)) is not None
                    else Option.of_optional(_GROUP_TITLES.get(self.group))
                )
            case ClassSystem.OMNICLASS:
                return Option.of_optional(_OMNI_TABLES.get(self.segments[0])) if self.segments else Nothing
            case _ as unreachable:
                assert_never(unreachable)

    def division(self) -> int:
        # the MasterFormat division head the crosswalk keys on — a UniFormat/OmniClass code has no division,
        # so it projects `-1`, distinct from every assigned `0..49` and never a raise on the wrong system.
        return self.segments[0] if self.system is ClassSystem.MASTERFORMAT and self.segments else -1

    def subgroup(self) -> Option[Subgroup]:
        # the MasterFormat 2020 subgroup the division belongs to (Procurement / General / Facility Construction
        # / Facility Services / Site and Infrastructure / Process Equipment) — the specification project-manual
        # table of contents and a division roll-up group sections by it, reading the one `_DIVISIONS` row the
        # division already carries rather than a parallel division->subgroup table; `Nothing` for the
        # non-MasterFormat systems and a reserved/out-of-range division.
        found = _DIVISIONS.get(self.segments[0]) if self.system is ClassSystem.MASTERFORMAT and self.segments else None
        return Some(found.subgroup) if found is not None else Nothing

    @property
    def _uf_anchor(self) -> str:
        # the UniFormat Level-2 element anchor (group + first digit-pair) `parse` head-validates and `title`
        # resolves the element title from, so admission and titling widen to Level-3+ codes.
        return self.group + (f"{self.segments[0]:02d}" if self.segments else "")

    @property
    def _deepest(self) -> int:
        # the highest positional index carrying a significant (non-zero) segment — the numeric systems'
        # trailing-zero-insensitive depth `level` and `parent` both derive from one computation.
        return next((at for at in reversed(range(len(self.segments))) if self.segments[at]), 0)

    @property
    def level(self) -> int:
        # the classification hierarchy depth a `drawing/schedule#SCHEDULE` roll-up groups by and a
        # keynote-scope query reads: the UniFormat group letter plus its appended pairs, or the deepest
        # significant positional segment of the numeric systems — a division/table/group is `1`.
        return 1 + len(self.segments) if self.system is ClassSystem.UNIFORMAT else self._deepest + 1 if self.segments else 0

    def parent(self) -> Option[Self]:
        # the immediate hierarchy container the schedule roll-up groups keynotes by and the keynote-scope query
        # widens against: a MasterFormat `.NN` level-4 suffix truncates to its level-3 section, the numeric
        # triple zeros the deepest significant positional level (`03 30 53` -> `03 30 00` -> `03 00 00`),
        # UniFormat truncates the last appended pair (`B1010` -> `B10` -> `B`), `Nothing` at the root.
        match self.system:
            case ClassSystem.UNIFORMAT:
                return Some(structs.replace(self, segments=self.segments[:-1])) if self.segments else Nothing
            case ClassSystem.MASTERFORMAT if len(self.segments) > 3:
                return Some(structs.replace(self, segments=self.segments[:3]))
            case _:
                rolled = (*self.segments[: self._deepest], *(0 for _ in self.segments[self._deepest :]))
                return Some(structs.replace(self, segments=rolled)) if self._deepest else Nothing

    def descends_from(self, ancestor: "ClassCode", /) -> bool:
        # the containment predicate a keynote-scope query reads: a code descends from an ancestor when they
        # share a system and group, this code is at least as deep, and every SIGNIFICANT ancestor segment
        # matches — a `0` ancestor segment is the unspecified-level wildcard (`03 00 00` Division 03 contains
        # `03 30 00`), so containment widens along the classification hierarchy where a flat tuple-prefix
        # equality would miss the trailing-zero division heading. `B` contains `B10` and `B10` contains
        # `B1010` fall out of the same rule (the short prefix already matches, no wildcard needed). The `>=`
        # depth makes it REFLEXIVE (a code contains itself), so a consumer never re-checks the exact match.
        return (
            self.system is ancestor.system
            and self.group == ancestor.group
            and len(self.segments) >= len(ancestor.segments)
            and all(anc in (0, seg) for anc, seg in zip(ancestor.segments, self.segments, strict=False))
        )

    def crosswalk(self) -> "CrossReference":
        # fold this code onto its cross-system peers from the primary `_CROSSWALK` correspondence and the
        # OmniClass table-alignment invariant — MasterFormat->UniFormat via the division->group row and its
        # elements, MasterFormat->OmniClass via the exact Table-22 digit copy, UniFormat->MasterFormat via
        # the derived inverse and ->OmniClass via the Table-21 anchor (no digit copy Table 21's notation
        # would forge), OmniClass Table 22 mirroring back to its MasterFormat digits; a code with no
        # crosswalk edge yields an empty peer set rather than a raise.
        match self.system:
            case ClassSystem.MASTERFORMAT:
                group = _CROSSWALK.get(self.division(), "")
                elements = Block.of_seq(_ELEMENTS.values()).filter(lambda element: element.group == group)
                return CrossReference(self, frozendict({
                    ClassSystem.UNIFORMAT: tuple(ClassCode(ClassSystem.UNIFORMAT, _pairs(element.code[1:]), group) for element in elements),
                    ClassSystem.OMNICLASS: (ClassCode(ClassSystem.OMNICLASS, (_OMNI_WORK_RESULTS, *self.segments)),),
                }))
            case ClassSystem.UNIFORMAT:
                divisions = _GROUP_DIVISIONS.get(self.group, ())
                return CrossReference(self, frozendict({
                    ClassSystem.MASTERFORMAT: tuple(ClassCode(ClassSystem.MASTERFORMAT, (division, 0, 0)) for division in divisions),
                    ClassSystem.OMNICLASS: (ClassCode(ClassSystem.OMNICLASS, (_OMNI_ELEMENTS,)),),
                }))
            case ClassSystem.OMNICLASS:
                table, *tail = self.segments or (0,)
                mirror = (ClassCode(ClassSystem.MASTERFORMAT, tuple(tail)),) if table == _OMNI_WORK_RESULTS and len(tail) >= 3 else ()
                return CrossReference(self, frozendict({ClassSystem.MASTERFORMAT: mirror}) if mirror else frozendict())
            case _ as unreachable:
                assert_never(unreachable)


class CrossReference(Struct, frozen=True):
    # one code folded onto its peers across systems — the source code and the per-system related-code set the
    # `crosswalk` derives from the primary correspondence and the OmniClass table-alignment invariant.
    source: ClassCode
    peers: frozendict[ClassSystem, tuple[ClassCode, ...]] = frozendict()

    def peer(self, system: ClassSystem, /) -> tuple[ClassCode, ...]:
        return self.peers.get(system, ())

# --- [CONSTANTS] ------------------------------------------------------------------------

_MF_PATTERN: Final[re.Pattern[str]] = re.compile(r"\A(\d{2})\s*(\d{2})\s*(\d{2})(?:\.(\d{2}))?\Z")
_UF_PATTERN: Final[re.Pattern[str]] = re.compile(r"\A(?P<group>[A-GZ])(?P<pairs>(?:\d{2})*)\Z")
_OC_PATTERN: Final[re.Pattern[str]] = re.compile(r"\A(?P<table>\d{2})(?:-(?P<tail>\d{2}(?:\s+\d{2})*))?\Z")

# --- [TABLES] ---------------------------------------------------------------------------

# the MasterFormat 2020 ASSIGNED divisions (35 of the 50 numbers 00-49); the reserved 15-20/24/29/30/
# 36-39/47/49 gaps are the derived complement over `range(50)`, never a fabricated title.
_DIVISIONS: Final[frozendict[int, Division]] = frozendict({
    0: Division(0, "Procurement and Contracting Requirements", Subgroup.PROCUREMENT),
    1: Division(1, "General Requirements", Subgroup.GENERAL),
    2: Division(2, "Existing Conditions", Subgroup.CONSTRUCTION),
    3: Division(3, "Concrete", Subgroup.CONSTRUCTION),
    4: Division(4, "Masonry", Subgroup.CONSTRUCTION),
    5: Division(5, "Metals", Subgroup.CONSTRUCTION),
    6: Division(6, "Wood, Plastics, and Composites", Subgroup.CONSTRUCTION),
    7: Division(7, "Thermal and Moisture Protection", Subgroup.CONSTRUCTION),
    8: Division(8, "Openings", Subgroup.CONSTRUCTION),
    9: Division(9, "Finishes", Subgroup.CONSTRUCTION),
    10: Division(10, "Specialties", Subgroup.CONSTRUCTION),
    11: Division(11, "Equipment", Subgroup.CONSTRUCTION),
    12: Division(12, "Furnishings", Subgroup.CONSTRUCTION),
    13: Division(13, "Special Construction", Subgroup.CONSTRUCTION),
    14: Division(14, "Conveying Equipment", Subgroup.CONSTRUCTION),
    21: Division(21, "Fire Suppression", Subgroup.SERVICES),
    22: Division(22, "Plumbing", Subgroup.SERVICES),
    23: Division(23, "Heating, Ventilating, and Air Conditioning (HVAC)", Subgroup.SERVICES),
    25: Division(25, "Integrated Automation", Subgroup.SERVICES),
    26: Division(26, "Electrical", Subgroup.SERVICES),
    27: Division(27, "Communications", Subgroup.SERVICES),
    28: Division(28, "Electronic Safety and Security", Subgroup.SERVICES),
    31: Division(31, "Earthwork", Subgroup.INFRASTRUCTURE),
    32: Division(32, "Exterior Improvements", Subgroup.INFRASTRUCTURE),
    33: Division(33, "Utilities", Subgroup.INFRASTRUCTURE),
    34: Division(34, "Transportation", Subgroup.INFRASTRUCTURE),
    35: Division(35, "Waterway and Marine Construction", Subgroup.INFRASTRUCTURE),
    40: Division(40, "Process Interconnections", Subgroup.PROCESS),
    41: Division(41, "Material Processing and Handling Equipment", Subgroup.PROCESS),
    42: Division(42, "Process Heating, Cooling, and Drying Equipment", Subgroup.PROCESS),
    43: Division(43, "Process Gas and Liquid Handling, Purification, and Storage Equipment", Subgroup.PROCESS),
    44: Division(44, "Pollution and Waste Control Equipment", Subgroup.PROCESS),
    45: Division(45, "Industry-Specific Manufacturing Equipment", Subgroup.PROCESS),
    46: Division(46, "Water and Wastewater Equipment", Subgroup.PROCESS),
    48: Division(48, "Electrical Power Generation", Subgroup.PROCESS),
})
# the reserved MasterFormat slots (future-expansion, unassigned) — the derived complement over `range(50)`
# `parse` maps to `<reserved-division>`, distinct from an out-of-range `<unknown-division>`.
_RESERVED: Final[frozenset[int]] = frozenset(range(50)) - frozenset(_DIVISIONS)

# the UniFormat Level-1 group titles and their canonical Level-2 elements (ASTM E1557 + CSI Z General).
_GROUP_TITLES: Final[frozendict[str, str]] = frozendict({
    "A": "Substructure", "B": "Shell", "C": "Interiors", "D": "Services",
    "E": "Equipment and Furnishings", "F": "Special Construction and Demolition",
    "G": "Building Sitework", "Z": "General",
})
_ELEMENTS: Final[frozendict[str, Element]] = frozendict({
    element.code: element for element in (
        Element("A10", "Foundations", "A"), Element("A20", "Basement Construction", "A"),
        Element("B10", "Superstructure", "B"), Element("B20", "Exterior Enclosure", "B"), Element("B30", "Roofing", "B"),
        Element("C10", "Interior Construction", "C"), Element("C20", "Stairs", "C"), Element("C30", "Interior Finishes", "C"),
        Element("D10", "Conveying", "D"), Element("D20", "Plumbing", "D"), Element("D30", "HVAC", "D"),
        Element("D40", "Fire Protection", "D"), Element("D50", "Electrical", "D"),
        Element("E10", "Equipment", "E"), Element("E20", "Furnishings", "E"),
        Element("F10", "Special Construction", "F"), Element("F20", "Selective Building Demolition", "F"),
        Element("G10", "Site Preparation", "G"), Element("G20", "Site Improvements", "G"),
        Element("G30", "Site Mechanical Utilities", "G"), Element("G40", "Site Electrical Utilities", "G"),
        Element("G90", "Other Site Construction", "G"), Element("Z10", "General", "Z"),
    )
})

# the OmniClass 15 tables (OCCS / ISO 12006-2) — the faceted classification vocabulary keyed by table number.
_OMNI_TABLES: Final[frozendict[int, str]] = frozendict({
    11: "Construction Entities by Function", 12: "Construction Entities by Form",
    13: "Spaces by Function", 14: "Spaces by Form", 21: "Elements", 22: "Work Results",
    23: "Products", 31: "Phases", 32: "Services", 33: "Disciplines", 34: "Organizational Roles",
    35: "Tools", 36: "Information", 41: "Materials", 49: "Properties",
})

# the ONE primary crosswalk correspondence: MasterFormat division -> UniFormat Level-1 group. `_GROUP_DIVISIONS`
# is the DERIVED inverse (group -> divisions) by comprehension, so a new edge lands once here and the inverse
# re-derives. OmniClass peers are NOT a table — Table 22 Work Results IS MasterFormat (exact digit copy) and
# Table 21 Elements IS UniFormat (elemental alignment, notation-distinct), so they derive from the alignment
# invariant in `crosswalk`, never a hand-kept third map.
_CROSSWALK: Final[frozendict[int, str]] = frozendict({
    2: "G", 3: "B", 4: "B", 5: "B", 6: "B", 7: "B", 8: "B", 9: "C", 10: "C", 11: "E", 12: "E",
    13: "F", 14: "D", 21: "D", 22: "D", 23: "D", 25: "D", 26: "D", 27: "D", 28: "D",
    31: "G", 32: "G", 33: "G", 34: "G", 35: "G",
})
_GROUP_DIVISIONS: Final[frozendict[str, tuple[int, ...]]] = frozendict({
    group: tuple(division for division, mapped in _CROSSWALK.items() if mapped == group)
    for group in frozenset(_CROSSWALK.values())
})
_OMNI_WORK_RESULTS: Final[int] = 22   # OmniClass Table 22 == MasterFormat (exact digit copy)
_OMNI_ELEMENTS: Final[int] = 21       # OmniClass Table 21 == UniFormat (elemental alignment, notation-distinct)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _pairs(digits: str, /) -> tuple[int, ...]:
    # split a UniFormat digit tail into its Level-2+ pair segments ("1010" -> (10, 10)); the shared
    # conversion `parse`'s UniFormat arm and `crosswalk`'s element-peer build both read.
    return tuple(int(digits[at : at + 2]) for at in range(0, len(digits), 2))
```

## [03]-[COORDINATE]

- Owner: `ReferenceIndex` the one drawing↔spec cross-reference owner admitting the `Reference` link set ONCE into a bidirectional index — a `Reference` binding one specification `ClassCode` to one `SheetRef` (a `sheet` number, an optional `detail` id, a `Discipline`), the keynote correspondence a drawing keynote schedule and a specification section together assert. `ReferenceIndex.of` folds the reference `Block` into one `forward: Map[ClassCode, tuple[SheetRef, ...]]` section→sheets index AND one `inverse: Map[str, tuple[ClassCode, ...]]` sheet-number→sections index in one pass, so `resolve(query)` — ONE polymorphic method discriminating on the query's own shape — reads the pre-built pair rather than re-folding per call: a `ClassCode` query resolves to the `SheetRef`s detailing that section (the "where is Division 03 drawn" lookup, widened through `descends_from`), a `SheetRef` query resolves by its `sheet` number to the `ClassCode` sections governing that sheet (the "what specs govern sheet A-201" lookup), never a `sheets_for`/`sections_for` sibling pair the caller picks between. `coordinate(specified)` folds the same admitted reference set against the specified-section set into the `Coordination` reconciliation.
- Cases: `SheetRef` (`sheet`, `detail`, `discipline`) the drawing-side reference — a sheet number (`A-201`), an optional detail-bubble id, and the ISO 13567 / NCS `Discipline` the `drawing/standard#STANDARD` layer codec owns (composed, never a parallel stringly discipline); `Reference` (`section`, `sheet`) the one link the index folds. `Coordination` carries the reconciliation triple: `matched` the `Reference`s whose `section` the project actually specifies, `orphan_sections` the `frozenset[ClassCode]` specced but detailed on no sheet (the "specified, never drawn" coordination gap), and `orphan_details` the `SheetRef`s keynoted against a section the manual never specifies (the "drawn, never specified" gap) — the two silent-truncation defects a manual owes its drawing set, surfaced as evidence rather than lost.
- Entry: `ReferenceIndex.of(references)` normalizes the `Reference | Iterable[Reference]` at the head and folds both directions once — the `forward` index keyed on `ref.section`, the `inverse` index keyed on `ref.sheet.sheet` (the sheet NUMBER, so a sheet query gathers every governing section regardless of the query's detail/discipline). `resolve(query)` dispatches on `query` shape — a `ClassCode` reads `forward.items()` widened through `descends_from` so a division query gathers every descendant section's sheets, a `SheetRef` reads `inverse[query.sheet]` — returning the resolved `Block` on either arm through one `match`. `coordinate(specified)` normalizes the `ClassCode | Iterable[ClassCode]` head and reconciles against the admitted reference set; both read the index the composition root built once.
- Auto: `coordinate` partitions the reference set against `specified` in one `Block.partition` — `matched` is the references whose `section` the specified set contains (widened by `descends_from` so a `03 30 00` reference matches a specified `03 00 00` division section) and `orphan_details` the complementary half projected to sheets, so the `specifies` predicate is evaluated once per reference rather than twice; `orphan_sections` is the `Block.filter`-into-`frozenset` of specified codes no reference reaches (`Block` exposes no `exists`, so membership is a non-empty filtered block, never a `list.append` accumulator). `rows()` projects the reconciliation to the flat `tuple[frozendict[str, str], ...]` a `visualization/table#TABLE` or `specification/section#SECTION` `TableNode` renders — one row per matched link plus one flagged row per orphan, each carrying its section/sheet/detail/`discipline`/status — and `facts()` projects the native-int `matched`/`orphan_sections`/`orphan_details` tally the composing producer folds into its OWN `ArtifactReceipt`, the tabular egress and the receipt evidence the reconciliation owes its consumer, this owner computing the verdict and never the rendered artifact.
- Receipt: none. `ReferenceIndex`/`Coordination` are a pure reconciliation whose `rows` a downstream `visualization/table#TABLE`/`specification/section#SECTION` producer renders and whose `facts` that producer folds into its own `ArtifactReceipt` — the coordination facts (matched count, orphan counts) ride THAT producer's `Table` or `Spec` case, never a classification receipt rail this owner mints.
- Packages: `msgspec` (`Struct(frozen=True)` the `SheetRef`/`Reference`/`ReferenceIndex`/`Coordination` value objects, `SheetRef` hashable for the reference dedup, `ClassCode` and the `str` sheet number the two index keys); `expression` (`Map.empty`/`change`/`try_find`/`items` the two bidirectional indexes built by one fold, `Block.of_seq`/`fold`/`filter`/`partition`/`map`/`singleton`/`is_empty` the reference traversal and the one-pass reconciliation, `Some`/`Option` the index-fold and lookup rail); `frozendict` (the `rows`/`facts` flat-record projections and the `orphan_sections` frozenset); `drawing/standard#STANDARD` (`Discipline` the owned ISO 13567/NCS discipline vocabulary the `SheetRef` composes); the sibling `ClassCode` (`.render`/`.descends_from` composed, never re-declared). No runtime import — the resolver is pure over the admitted references.
- Growth: a new reference axis (a keynote revision, a spec addendum flag) is one `Reference`/`SheetRef` field the index fold and the `rows` projection absorb; a new query modality is one `resolve` `match` arm on the query shape (the `assert_never` forcing it); a new reconciliation category (a superseded section, a mismatched-discipline link) is one `Coordination` field derived from the one index; a new coordination fact is one `facts()` entry. Zero new surface — the index grows by field on the link and arm on the query.
- Boundary: a `sheets_for(code)`/`sections_for(sheet)` sibling pair where one `resolve` discriminates on the query shape is the deleted surface spam; a re-built index per query where `ReferenceIndex.of` builds both directions once and every `resolve`/`coordinate` reads them is the deleted recompute; a full-`SheetRef` inverse key where the sheet NUMBER keys the governing-section lookup is the deleted over-specific match; two `references.filter` passes computing `specifies` twice where one `Block.partition` splits matched from orphan is the deleted double-scan; a `list.append` reconciliation accumulator where `Block.partition`/`filter`-into-`frozenset` derives the sets is the deleted flat form; a stringly `discipline: str` where the owned `Discipline` vocabulary types the field is the deleted stringly form; a silently dropped orphan section or detail where `Coordination` surfaces both as evidence is the deleted truncation; a rendered coordination matrix this owner emits where `rows()`/`facts()` hand the flat records to the `visualization/table#TABLE` renderer is the deleted boundary trample. This owner computes the coordination, never the artifact.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable
from typing import Self, assert_never, overload

from builtins import frozendict
from expression import Some
from expression.collections import Block, Map
from msgspec import Struct

from artifacts.drawing.standard import Discipline
from artifacts.specification.classify import ClassCode

# --- [MODELS] ---------------------------------------------------------------------------


class SheetRef(Struct, frozen=True):
    # the drawing-side reference: a sheet number, an optional detail-bubble id, and the ISO 13567 / NCS
    # `Discipline` the `drawing/standard#STANDARD` layer codec owns; hashable for the reference dedup.
    sheet: str
    detail: str = ""
    discipline: Discipline = Discipline.ARCHITECTURAL


class Reference(Struct, frozen=True):       # one keynote link: this section is detailed on this sheet
    section: ClassCode
    sheet: SheetRef


class Coordination(Struct, frozen=True):
    # the drawing<->spec reconciliation: the links the manual actually specifies, the sections specced but
    # drawn on no sheet, and the sheets keynoted against a section the manual never specifies.
    matched: tuple[Reference, ...] = ()
    orphan_sections: frozenset[ClassCode] = frozenset()
    orphan_details: tuple[SheetRef, ...] = ()

    def facts(self) -> frozendict[str, int]:
        # the native-int coordination tally the composing producer folds into its own ArtifactReceipt facts
        # (this owner mints none): the matched-link count and the two silent-truncation gap counts.
        return frozendict({"matched": len(self.matched), "orphan_sections": len(self.orphan_sections), "orphan_details": len(self.orphan_details)})

    def rows(self) -> tuple[frozendict[str, str], ...]:
        # the flat tabular egress a `visualization/table#TABLE` / `specification/section#SECTION` `TableNode`
        # renders — one row per matched link, one flagged row per orphan section, one per orphan detail.
        linked = tuple(
            frozendict({"section": ref.section.render(), "sheet": ref.sheet.sheet, "detail": ref.sheet.detail, "discipline": ref.sheet.discipline.value, "status": "matched"})
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
    # the drawing<->spec reference set admitted ONCE into both directions: `forward` (section -> the sheets
    # detailing it) and `inverse` (sheet NUMBER -> the sections governing it), built by one fold at `of` so
    # `resolve` reads a pre-built index rather than re-folding per query, and `references` the admitted set
    # `coordinate` reconciles against a specified-section set.
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
        # one bidirectional resolver discriminating on the query shape over the two indexes built once at
        # `of`: a `ClassCode` gathers the sheets detailing that section (widened by `descends_from` so a
        # division query sweeps every descendant section), a `SheetRef` gathers the sections governing its
        # sheet number.
        match query:
            case ClassCode() as section:
                return Block.of_seq(
                    sheet for keyed, sheets in self.forward.items() if keyed.descends_from(section) for sheet in sheets
                )
            case SheetRef() as sheet:
                return Block.of_seq(self.inverse.try_find(sheet.sheet).default_value(()))
            case _ as unreachable:
                assert_never(unreachable)

    def coordinate(self, specified: ClassCode | Iterable[ClassCode], /) -> Coordination:
        # partition the reference set against the specified sections in one pass — a reference matches when
        # the manual specifies its section or an ancestor division of it; the specified codes no reference
        # reaches are the "specced, never drawn" gaps and the complementary reference half the "drawn, never
        # specced" gaps (`Block.partition` splits matched from orphan so `specifies` runs once per
        # reference, `Block` exposing no `exists` so membership is a non-empty filtered block).
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
    # the one modal-arity head — a lone `Reference`/`ClassCode` (msgspec Structs are non-iterable) the
    # singleton, any container the multi-element case; never a `single`/`many` suffix pair.
    match items:
        case Iterable() as stream if not isinstance(stream, (str, bytes)):
            return Block.of_seq(stream)
        case lone:
            return Block.singleton(lone)
```

## [04]-[RESEARCH]

- [VOCABULARY_CARDINALITY]: the three vocabularies are authored to their exact published extents, the owned-standard capability the brief mandates over an approximated slice. `_DIVISIONS` carries the 35 ASSIGNED `MasterFormat` 2020 divisions — Division 00 (Procurement), 01 (General Requirements), 02-14 (the Facility Construction subgroup's assigned run), 21-23/25-28 (Facility Services), 31-35 (Site and Infrastructure), and 40-46/48 (Process Equipment) — the reserved numbers (15-20, 24, 29, 30, 36-39, 47, 49) the derived `_RESERVED` complement over `range(50)` rather than fabricated titles, so `parse` maps a reserved MasterFormat slot to `<reserved-division>` and an out-of-range number to `<unknown-division>` — the future-expansion slot distinguished from the invalid one, the complement load-bearing on the fault split rather than a dead set. `_ELEMENTS` carries the `UniFormat` Level-2 elements under the `A`-`G`+`Z` Level-1 groups (ASTM E1557 elemental A/B/C/D/E/F/G plus the CSI `Z` General), and `_OMNI_TABLES` the 15 `OmniClass` tables at their published numbers (11/12/13/14/21/22/23/31/32/33/34/35/36/41/49), the table-number gaps the OCCS's own reserved facets. A new assigned division, element, or table is one row; a fabricated reserved title is the rejected invention.
- [ADMISSION_DEPTH]: `parse` head-validates each system so a code admits at any authored depth below its known head — `MasterFormat` and `OmniClass` against `segments[0]` in `_DIVISIONS`/`_OMNI_TABLES` (so `03 30 53` admits under Division `03`, `22-03 30 00` under Table `22`), and `UniFormat` against the `_uf_anchor` Level-2 element in `_ELEMENTS` (so `B1010` admits under element `B10` while `B99` correctly rejects). The prior full-render UniFormat membership test rejected every valid Level-3+ code because `_ELEMENTS` carries only the Level-2 anchors — the over-strict admission a Level-3 estimating code (`B1010` Standard Foundations) would silently fail; head-validation restores the anchor-plus-descendant admission the numeric systems already had. `_OC_PATTERN`'s optional `-tail` makes the bare table anchor (`21`) parse and round-trip so the crosswalk's Table-21 peer is a real re-parseable code, and `render`'s dot-joined `.NN` suffix keeps a `MasterFormat` level-4 code (`03 30 53.13`) re-parseable through `_MF_PATTERN`'s optional level-4 group — the same render/parse asymmetry closed on both the OmniClass and the MasterFormat arm rather than a space-joined fourth segment `parse` would reject.
- [HIERARCHY_ALGEBRA]: the classification systems are trees, so `ClassCode` owns the full navigation algebra a `drawing/schedule#SCHEDULE` roll-up and a keynote-scope query compose — the containment predicate `descends_from` (does this code fall under that broadscope section), the depth `level` (the trailing-zero-insensitive hierarchy level a schedule groups line items by), the constructor `parent` (the immediate container a keynote rolls up to), the head `division` (the crosswalk key), and the `subgroup` band (the `MasterFormat` 2020 six-subgroup partition — Procurement / General / Facility Construction / Facility Services / Site and Infrastructure / Process Equipment — a specification table of contents groups sections by, projected `Option[Subgroup]` off the `Division.subgroup` the row already carries rather than a parallel table) — where the prior page owned only the predicate and the head. `level` and `parent` derive from one `_deepest` significant-index projection over the numeric systems and the group-plus-pairs count over `UniFormat`, so the depth and the roll-up never diverge; `parent` truncates a `MasterFormat` `.NN` level-4 suffix to its level-3 section, then rolls `03 30 53` -> `03 30 00` -> `03 00 00` by zeroing the deepest positional level and `B1010` -> `B10` -> `B` by truncating the last pair, `Nothing` at the division/table/group root.
- [CROSSWALK_DERIVATION]: the classification crosswalk obeys `DERIVED_LOGIC` — ONE primary `_CROSSWALK` `frozendict` maps each `MasterFormat` division to its `UniFormat` Level-1 group (Concrete/Masonry/Metals/Thermal/Openings → `B` Shell, Finishes/Specialties → `C` Interiors, the mechanical/electrical divisions → `D` Services, the sitework divisions → `G` Building Sitework, Equipment/Furnishings → `E`), and `_GROUP_DIVISIONS` derives the group→divisions inverse by comprehension so the two never drift. The `OmniClass` peers are NOT a third hand-kept map: `OmniClass` Table 22 Work Results IS `MasterFormat` (a `MasterFormat` code's Table-22 peer is the exact `(22, *segments)` digit copy, and the reverse mirrors the tail back to `MasterFormat`), while Table 21 Elements IS `UniFormat` only ELEMENTALLY — its numeric notation does not map onto `UniFormat`'s alpha-numeric element codes digit-for-digit, so a `UniFormat` code's `OmniClass` peer is the honest Table-21 anchor `(21,)` rather than a forged digit copy. This is the `boundaries.md` DERIVED_LOGIC law applied across the classification seam — a new crosswalk edge is one primary row and every secondary re-derives with no consumer edited.
- [DRAWING_SPEC_RESOLVER]: the drawing↔spec resolver is the coordination concern no single classification system spells and the one the brief names — a `Reference` binds a specification `ClassCode` to a `SheetRef` (the keynote correspondence a drawing keynote schedule and a spec section jointly assert), and `ReferenceIndex.of` admits the whole reference set ONCE (`BOUNDARY_ADMISSION`) into both a `forward` section→sheets and an `inverse` sheet-number→sections index built in one fold, so `resolve` is `MODAL_ARITY` over the query shape reading the pre-built index rather than re-folding per query — the prior page's per-call one-directional rebuild is the `boundaries.md` recompute the held index deletes, and the sheet-number inverse key is why "what governs sheet A-201" gathers every governing section regardless of the query's detail bubble. `coordinate` `Block.partition`s the same admitted reference set against the specified sections into the reconciliation every project manual owes its drawing set — `orphan_sections` the specced-but-never-drawn gap and `orphan_details` the drawn-but-never-specced gap — the two silent-truncation defects the `boundaries.md` PROBE_SWEEP law surfaces as evidence, projected as flat `rows` and a native-int `facts` tally the tabular consumer renders and folds into its own receipt. The `descends_from` wildcard-zero containment predicate is why a `03 30 00` cast-in-place reference resolves against a `03 00 00` Division-03 section: the `0` level segments are unspecified-level wildcards, so coordination widens along the classification hierarchy, a real containment the flat tuple-prefix equality omits. The `ClassCode`s the `ReferenceIndex` resolves are the SAME codes that ride `document/model#NODE` `NodeMeta.classification` on the lowered document tree — `specification/section#SECTION`'s root `SectionNode` stamps its section code there, `document/report#REPORT`'s `Section.classification` mirrors it, and `DocumentNode.to_corpus` projects the `classification` column — so the model-tree classification space and this resolver's key space are ONE: a consumer harvests the tree's `NodeMeta.classification`, `ClassCode.parse`s each, and folds the `Reference` set `ReferenceIndex.of` admits, never a second parallel classification token.
