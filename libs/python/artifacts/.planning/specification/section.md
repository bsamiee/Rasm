# [PY_ARTIFACTS_SECTION]

One owner holds the construction-specification production — the CSI/CSC SectionFormat three-part section (`PART 1 GENERAL` / `PART 2 PRODUCTS` / `PART 3 EXECUTION`), its article vocabulary, the four methods of specifying, and the PageFormat numbering. `Spec` admits a raw payload EXACTLY ONCE through the closed `SpecPayload`, validates its MasterFormat section number against `specification/classify#CODE` and its article titles against the owned roster under an accumulating disposition, lowers the validated section INTO a `document/model#NODE` `DocumentNode` tree, and contributes the one new `core/receipt#RECEIPT` `ArtifactReceipt.Spec` case — so a specification section is one schedulable `core/plan#PLAN` `ArtifactWork` producer beside every other artifact, its typeset emission owned downstream by `document/emit#DOCUMENT` folding FROM the tree. This owner authors the section semantics and the numbered node tree; it renders no page and re-authors no IFC — the QTO facts a specification cites arrive from `csharp:Rasm.Bim` through `visualization/table#TABLE`.

Owned families carry the three-part SectionFormat spine, canonical article roster, subordinate-title checklist, exact-one main-work alternatives, specifying methods, submittal classes, and paragraph roles. `NOTE` guidance strips at issue — the audit counts notes off the source spec and no lowering renders them; accumulating admission retains independent casualties; one audit fold derives ordering, cardinality, histogram, fill-in, checklist, and citation evidence. `NumberScheme.alphanumeric` and `decimal` carry only their own policy payloads, and lowering terminates with `END OF SECTION`.

## [01]-[INDEX]

- [02]-[PAGE]: `PageFormat` — the CSI PageFormat numbering and page-presentation substrate, `label` projecting one ordinal path to its designation across both numbering modes.
- [03]-[SECTION]: `Spec` — the CSI SectionFormat producer over the owned article vocabularies, lowering a validated section into the `document/model#NODE` tree and minting the one `ArtifactReceipt.Spec` case.

## [02]-[PAGE]

- Owner: `PageFormat` carries typography, page size, and one closed `NumberScheme`. `NumberScheme.alphanumeric(article_pad)` owns CSI article padding; `decimal` is tag-only because UFGS numbering has no padding axis.
- Cases: `NumberScheme` carries `alphanumeric(article_pad)` and tag-only `decimal`; `NumberLevel` carries the eight-level hierarchy; `Alphabet` and `Decoration` carry glyph axes; `PageSize` carries sheet regimes.
- Entry: `NumberScheme.label(part, path)` normalizes on the path shape — an empty path yields `PART {part}`, a length-1 path the part-prefixed article number, a deeper path the local level glyph (`ALPHANUMERIC` reads `_LEVEL_STYLE[NumberLevel(len(path)-1)]`, `DECIMAL` joins the whole path) — so one entrypoint owns every level and mode, the discriminant the path length and mode, never a `paragraph_label`/`subparagraph_label` method family.
- Auto: `label` derives every designation from the ONE `_LEVEL_STYLE` correspondence — `_glyph` projecting the ordinal through its alphabet and `_wrap` applying the level's punctuation, both closed by `assert_never`. `page_footer(section, page)` projects the CSI `{section} - {page}` footer, and `end_of_section` the marker CSI PageFormat mandates AFTER the `SCHEDULES` article — the marker `to_document` appends as the tree's terminal block.
- Receipt: none — `PageFormat` is pure presentation policy; the numbering travels INTO the `Spec` producer's tree and `ArtifactReceipt.Spec` facts, exactly as `classify#CODE` contributes none.
- Packages: `msgspec` (`Struct(frozen=True)` the value objects, hashable so a shared `PageFormat` keys deterministically); `expression` (`Map` the `_LEVEL_STYLE` correspondence); `enum` (the closed numbering vocabularies). No runtime import.
- Growth: a new numbering regime adds one `NumberScheme` case and `label` arm; a new nesting level adds one `NumberLevel` and `_LEVEL_STYLE` row; new glyph and punctuation policies add one enum value and total arm.
- Boundary: this owner authors presentation policy, never bytes or receipts.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import IntEnum, StrEnum
from math import isfinite
from typing import Final, Literal, assert_never

from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

# --- [TYPES] ----------------------------------------------------------------------------


class NumberLevel(IntEnum):  # the CSI PageFormat subordination levels; `ARTICLE` is the part-prefixed head
    ARTICLE = 0  # `1.01` — part digit + `article_pad`-wide ordinal, UPPERCASE title
    PARAGRAPH = 1  # `A.`
    SUBPARAGRAPH = 2  # `1.`
    CLAUSE = 3  # `a.`
    SUBCLAUSE = 4  # `1)`
    ITEM = 5  # `a)`
    SUBITEM = 6  # `(1)`
    DETAIL = 7  # `(a)` — the deepest CSI PageFormat level


class Alphabet(StrEnum):
    ARABIC = "arabic"
    UPPER = "upper"
    LOWER = "lower"


class Decoration(StrEnum):
    DOT = "dot"  # `A.`
    CLOSE_PAREN = "close_paren"  # `1)`
    BOTH_PARENS = "both_parens"  # `(a)`


class PageSize(StrEnum):
    LETTER = "letter"  # 8.5x11 in — US project manuals
    A4 = "a4"  # 210x297 mm — ISO project manuals


# --- [TABLES] ---------------------------------------------------------------------------

# ONE primary correspondence: subordination level -> (glyph alphabet, punctuation). `ARTICLE` is absent
# (its `1.01` form builds directly), so the table covers the seven sub-article levels.
_LEVEL_STYLE: Final[Map[NumberLevel, tuple[Alphabet, Decoration]]] = Map.of_seq([
    (NumberLevel.PARAGRAPH, (Alphabet.UPPER, Decoration.DOT)),
    (NumberLevel.SUBPARAGRAPH, (Alphabet.ARABIC, Decoration.DOT)),
    (NumberLevel.CLAUSE, (Alphabet.LOWER, Decoration.DOT)),
    (NumberLevel.SUBCLAUSE, (Alphabet.ARABIC, Decoration.CLOSE_PAREN)),
    (NumberLevel.ITEM, (Alphabet.LOWER, Decoration.CLOSE_PAREN)),
    (NumberLevel.SUBITEM, (Alphabet.ARABIC, Decoration.BOTH_PARENS)),
    (NumberLevel.DETAIL, (Alphabet.LOWER, Decoration.BOTH_PARENS)),
])
_MAX_LEVEL: Final[int] = NumberLevel.DETAIL.value  # the deepest CSI PageFormat level; admission caps paragraph nesting here

# --- [OPERATIONS] -----------------------------------------------------------------------


def _alpha(ordinal: int, /, *, upper: bool) -> str:
    # bijective base-26 (spreadsheet-column) so the 27th sibling is `AA`, never a modulo wraparound colliding on `A`.
    base, letters, remaining = (65 if upper else 97), "", ordinal
    while remaining > 0:
        remaining, digit = divmod(remaining - 1, 26)
        letters = chr(base + digit) + letters
    return letters or chr(base)


def _glyph(alphabet: Alphabet, ordinal: int, /) -> str:
    match alphabet:
        case Alphabet.ARABIC:
            return str(ordinal)
        case Alphabet.UPPER:
            return _alpha(ordinal, upper=True)
        case Alphabet.LOWER:
            return _alpha(ordinal, upper=False)
        case _ as unreachable:
            assert_never(unreachable)


def _wrap(decoration: Decoration, glyph: str, /) -> str:
    match decoration:
        case Decoration.DOT:
            return f"{glyph}."
        case Decoration.CLOSE_PAREN:
            return f"{glyph})"
        case Decoration.BOTH_PARENS:
            return f"({glyph})"
        case _ as unreachable:
            assert_never(unreachable)


# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class NumberScheme:
    tag: Literal["alphanumeric", "decimal"] = tag()
    alphanumeric: int = case()
    decimal: None = case()

    def label(self, part: int, path: tuple[int, ...], /) -> str:
        if part not in (1, 2, 3) or len(path) > _MAX_LEVEL + 1 or any(ordinal <= 0 for ordinal in path):
            raise ValueError("numbering path is outside the SectionFormat hierarchy")
        if not path:
            return f"PART {part}"
        match self:
            case NumberScheme(tag="decimal"):
                return f"{part}." + ".".join(str(ordinal) for ordinal in path)
            case NumberScheme(tag="alphanumeric", alphanumeric=article_pad) if article_pad > 0:
                return (
                    f"{part}.{path[0]:0{article_pad}d}"
                    if len(path) == 1
                    else _wrap(
                        (style := _LEVEL_STYLE[NumberLevel(len(path) - 1)])[1],
                        _glyph(style[0], path[-1]),
                    )
                )
            case NumberScheme(tag="alphanumeric"):
                raise ValueError("article padding must be positive")
            case _ as unreachable:
                assert_never(unreachable)


class PageFormat(Struct, frozen=True):
    numbering: NumberScheme = NumberScheme(alphanumeric=2)
    size: float = 10.0  # ISO 3098 / CSI body-text height in points the emitted section sets
    font: str = "body"
    page_size: PageSize = PageSize.LETTER

    def __post_init__(self) -> None:
        # finiteness precedes the range read: an inf satisfies a bare positivity check and admits unbounded type.
        if not isfinite(self.size) or self.size <= 0.0 or not self.font.strip():
            raise ValueError("page typography requires finite positive size and a font key")

    def label(self, part: int, path: tuple[int, ...], /) -> str:
        return self.numbering.label(part, path)

    def page_footer(self, section: str, page: int, /) -> str:
        return f"{section} - {page}"  # the CSI PageFormat section-number-page footer

    def end_of_section(self) -> str:
        return "END OF SECTION"  # CSI PageFormat places this AFTER (never before) the `SCHEDULES` article

    def uppercase(self, level: NumberLevel, /) -> bool:
        return level is NumberLevel.ARTICLE  # CSI: PART + article titles UPPERCASE, subordinate titles Title Case


# --- [CONSTANTS] ------------------------------------------------------------------------

CSI_PAGEFORMAT: Final[NumberScheme] = NumberScheme(alphanumeric=2)
UFGS_DECIMAL: Final[NumberScheme] = NumberScheme(decimal=None)


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = ["Alphabet", "CSI_PAGEFORMAT", "Decoration", "NumberLevel", "NumberScheme", "PageFormat", "PageSize", "UFGS_DECIMAL"]
```

## [03]-[SECTION]

- Owner: `Spec` — the CSI SectionFormat producer over a `section: ClassCode` (the MasterFormat number IS the section identity, admitted through `classify#CODE`), a `title`, a `tuple[Article, ...]` in canonical order, the runtime `lane: LanePolicy` the encode offloads on, and its `PageFormat` — lowering the validated section into a `document/model#NODE` `DocumentNode` tree and contributing one `ArtifactReceipt.Spec` case, never a stringly heading tree or a parallel per-part builder. `Article` binds one `SectionPart`, one validated `_ARTICLES` title, and its paragraphs; `Paragraph` is the recursive content node carrying its subordinate heading, text, `ParagraphRole` disposition, optional `SpecMethod`/`SubmittalClass`, `references`, and `children`. No specification library is admitted — the SectionFormat algebra is this owner's composition over the classify, document, and numbering owners.
- Cases: owned SectionFormat vocabularies are closed families authored to the CSI MP-2-2 published cardinality — `SectionPart` the three-part spine, `_ARTICLES` the primary article roster per part in published order (order IS load-bearing for the `_audited` canonical-order check), `_SUBORDINATE` the paragraph-title checklist each article carries, `_MAIN_WORK` the alternative-main-work set an `EXECUTION` part selects exactly one of — `main_works` counts the selection so zero, one, and multiple are three distinguishable audit outcomes. `SpecMethod` closes the four methods of specifying, `SubmittalClass` the three submittal regimes, `ParagraphRole` the `CONTENT`/`NOTE` editorial disposition. `SpecFault` carries the accumulating fault vocabulary with its `combined` monoid, and `SpecVerdict` carries the QA evidence — tallies, note/fill-in/off-checklist counts, reference and distinct-standard totals, method + submittal histograms, the canonical-order flag, the main-work count, and the accumulated coverage-fault tags.
- Entry: `Spec.admit(lane, page, /, **payload)` is the one boundary ingress — the `SpecPayload` shape gate through `_PAYLOAD`, then the section-number seam and the article fold composed under one accumulating disposition: the independent checks on one paragraph (depth, role, method, submittal, children) each contribute their casualty to the same `SpecFault.aggregate`, an article's part/title faults join its paragraphs' casualties, and the section-number fault joins the article casualties — a whole malformed section reports EVERY casualty at once, never the first per node. `to_document()` is the lowering entrypoint, `emit()` the schedulable `ArtifactWork`, `contribute()` the receipt, `audit()` the verdict — one polymorphic producer, never a per-part or per-target emit family. `submittal_register(specs)` is the manual-level modality over `Spec | Iterable[Spec]` — the flat CSI submittal-log row stream every `SubmittalClass`-carrying paragraph contributes, the schedule a `visualization/table#TABLE` frame renders and `delivery/register#REGISTER` keys.
- Auto: `to_document` lowers the section in one pass — the `Spec` a level-1 `SectionNode`, each present part a level-2, each article a `PageFormat`-numbered level-3, each paragraph a `BlockNode` recursing its sub-tree with the ordinal PATH threaded down so the numbering is the tree's own structure, and the `end_of_section` marker closing the tree as its terminal block. A `NOTE` paragraph STRIPS at issue — it never enters the issued tree, `BlockKind.ARTIFACT` being tagging semantics rather than elision — so the CONTENT ordinal `_article_node` threads is a running `accumulate` count that never advances over a note — the retained paragraphs number contiguously with no gap where a stripped note sat, the one place a naive producer leaks a numbering hole. `_audited` is ONE seed fold over the `_walk` flattening — the `_Tally` frozen seed carries the paragraph/note/fill-in/reference counts, the depth high-water, the off-checklist count, the method/submittal histograms, and the listed/cited citation partition, each stepped per walked paragraph — with only the per-article order, main-work, and coverage checks reading the article roster beside it, never a per-check re-walk.
- Receipt: `Spec` contributes the one new `core/receipt#RECEIPT` `ArtifactReceipt.Spec` case — the section number, the `division()` head the `classify#CODE` crosswalk keys on, the present-part and article counts, and the encoded-tree byte count. ONE identity serves `emit`, `contribute`, and the receipt: `_key` derives from the full frozen input spec (`section`, `title`, `articles`, `page`), so two sections sharing a number but differing in content never collide, and the byte count reads off the one encode `contribute` computes — never a second key regime hashing the encoded bytes.
- Packages: `msgspec` (the `Struct(frozen=True)` value objects and the deterministic node encode); `expression` (`tagged_union` the `SpecFault`; `Block` the accumulating admission fold; the `Result`/`Option` rails); `pydantic` (`TypeAdapter` the `_PAYLOAD` gate over the recursive `SpecPayload`, `ValidationError.errors()` the structured `loc` paths the `invalid_payload` case carries); `frozendict` (the owned vocabulary correspondences and verdict histograms); `re` (`_FILL_IN` the unresolved-fill-in `Pattern`); `itertools.accumulate` (the CONTENT-only ordinal); runtime (`ContentIdentity`/`ContentKey`, `RuntimeRail`/`async_boundary`, `LanePolicy`/`Kernel`/`KernelTrait` the off-loop encode); `classify#CODE` (`ClassCode`), `document/model#NODE` (the `SectionNode`/`BlockNode` tree and `encode`), and `core/receipt#RECEIPT` (`ArtifactReceipt.Spec`), composed never re-authored. No new external library.
- Growth: a new article is one `_ARTICLES` row; a new subordinate title one `_SUBORDINATE` row; a new main-work title one `_MAIN_WORK` member; a new method/submittal/disposition one `SpecMethod`/`SubmittalClass`/`ParagraphRole` member (a `NOTE` already strips at issue); a new QA rule one `SpecVerdict` field and one `_Tally` step or article check; a new fault cause one `SpecFault` case plus one `combined` member; a new section-number system is already carried by `ClassCode`.
- Boundary: this owner authors the specification, never the rendered page — `document/emit#DOCUMENT` folds the tree to PDF/Typst/HTML, `PageFormat` owns the numbering, `classify#CODE` owns `ClassCode`, and `csharp:Rasm.Bim` owns the QTO/IFC a specification cites through `visualization/table#TABLE`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import re
from builtins import frozendict
from collections import Counter
from collections.abc import Iterable, Iterator
from enum import StrEnum
from functools import reduce
from itertools import accumulate
from typing import TYPE_CHECKING, Final, Literal, NotRequired, ReadOnly, Required, Self, TypedDict, Unpack

from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import UNSET, Struct, msgpack
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait
from rasm.runtime.faults import RuntimeRail, async_boundary

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.document.model import BlockKind, BlockNode, DocumentNode, NodeMeta, RunNode, SectionNode, encode
from rasm.artifacts.specification.classify import ClassCode, ClassSystem

# `NumberLevel`/`PageFormat`/`_MAX_LEVEL` are the co-located `[02]-[PAGE]` owners above in this module — in scope directly.

if TYPE_CHECKING:
    from rasm.runtime.receipts import Receipt

# --- [TYPES] ----------------------------------------------------------------------------


class SectionPart(StrEnum):
    GENERAL = "general"  # PART 1 — administrative/procedural requirements
    PRODUCTS = "products"  # PART 2 — materials/products/equipment at the required quality
    EXECUTION = "execution"  # PART 3 — installation/application + quality-control work


class SpecMethod(StrEnum):  # the four CSI methods of specifying a product/paragraph
    DESCRIPTIVE = "descriptive"  # exact properties, no product name
    PERFORMANCE = "performance"  # required results + criteria, means open
    REFERENCE_STANDARD = "reference"  # by published ASTM/ANSI/UL standard
    PROPRIETARY = "proprietary"  # named product/manufacturer


class SubmittalClass(StrEnum):  # the CSI SubmittalFormat regimes
    ACTION = "action"  # requires A/E review + approval before proceeding
    INFORMATIONAL = "informational"  # for record, no approval
    CLOSEOUT = "closeout"  # project record documents, O&M data, warranties at completion


class ParagraphRole(StrEnum):  # the editorial disposition every master-spec paragraph carries at edit time
    CONTENT = "content"  # specification text, retained at issue
    NOTE = "note"  # specifier note (`SPEC NOTE:`), stripped at issue — audited from the source, never lowered


# --- [CONSTANTS] ------------------------------------------------------------------------

# unresolved-fill-in vocabulary: a bracketed blank `[____]` or an insert prompt `<Insert value>` the
# specifier never resolved — a section issued carrying one is editorially incomplete.
_FILL_IN: Final[re.Pattern[str]] = re.compile(r"\[_{2,}\]|<[^<>]+>")
_PART_NUMBER: Final[Map[SectionPart, int]] = Map.of_seq([(SectionPart.GENERAL, 1), (SectionPart.PRODUCTS, 2), (SectionPart.EXECUTION, 3)])
_CANON: Final = msgpack.Encoder(order="deterministic")  # the stable preimage encoding the bare `ContentIdentity.key` mint addresses

# --- [TABLES] ---------------------------------------------------------------------------

# CSI MP-2-2 primary article roster per part, in published order (Figure SF-1) — order IS load-bearing:
# `_audited` canonical-order checks read each title's index. Titles are UPPERCASE, the validated vocabulary.
_ARTICLES: Final[Map[SectionPart, tuple[str, ...]]] = Map.of_seq([
    (SectionPart.GENERAL, (
        "SUMMARY",
        "REFERENCES",
        "DEFINITIONS",
        "SYSTEM DESCRIPTION",
        "SUBMITTALS",
        "QUALITY ASSURANCE",
        "DELIVERY, STORAGE, AND HANDLING",
        "PROJECT/SITE CONDITIONS",
        "SEQUENCING",
        "SCHEDULING",
        "WARRANTY",
        "SYSTEM STARTUP",
        "OWNER'S INSTRUCTIONS",
        "COMMISSIONING",
        "MAINTENANCE",
    )),
    (SectionPart.PRODUCTS, (
        "MANUFACTURERS",
        "EXISTING PRODUCTS",
        "MATERIALS",
        "MANUFACTURED UNITS",
        "EQUIPMENT",
        "COMPONENTS",
        "ACCESSORIES",
        "MIXES",
        "FABRICATION",
        "FINISHES",
        "SOURCE QUALITY CONTROL",
    )),
    (SectionPart.EXECUTION, (
        "INSTALLERS",
        "EXAMINATION",
        "PREPARATION",
        "ERECTION",
        "INSTALLATION",
        "APPLICATION",
        "CONSTRUCTION",
        "REPAIR/RESTORATION",
        "RE-INSTALLATION",
        "FIELD QUALITY CONTROL",
        "ADJUSTING",
        "CLEANING",
        "DEMONSTRATION",
        "PROTECTION",
        "SCHEDULES",
    )),
])
# alternative main-work titles an EXECUTION part selects EXACTLY ONE of; `main_works` counts the selection —
# zero is the `missing_main_work` coverage fault, more than one the `multiple_main_work` fault.
_MAIN_WORK: Final[frozenset[str]] = frozenset({"ERECTION", "INSTALLATION", "APPLICATION", "CONSTRUCTION"})
# subordinate paragraph-title checklist each primary article carries (Figure SF-1), keyed by article title.
_SUBORDINATE: Final[Map[str, tuple[str, ...]]] = Map.of_seq([
    ("SUMMARY", (
        "Section Includes",
        "Products Supplied But Not Installed Under This Section",
        "Products Installed But Not Supplied Under This Section",
        "Related Sections",
        "Allowances",
        "Unit Prices",
        "Measurement Procedures",
        "Payment Procedures",
        "Alternates",
    )),
    ("REFERENCES", ("Reference Standards", "Abbreviations and Acronyms")),
    ("DEFINITIONS", ("Terms", "Definitions")),
    ("SYSTEM DESCRIPTION", ("Design Requirements", "Performance Requirements")),
    ("SUBMITTALS", ("Product Data", "Shop Drawings", "Samples", "Quality Assurance/Control Submittals", "Closeout Submittals")),
    ("QUALITY ASSURANCE", ("Qualifications", "Regulatory Requirements", "Certifications", "Field Samples", "Mock-ups", "Pre-installation Meetings")),
    ("DELIVERY, STORAGE, AND HANDLING", (
        "Packing, Shipping, Handling, and Unloading",
        "Acceptance at Site",
        "Storage and Protection",
        "Waste Management and Disposal",
    )),
    ("PROJECT/SITE CONDITIONS", ("Project/Site Environmental Requirements", "Existing Conditions")),
    ("SEQUENCING", ("Sequence of Operations", "Coordination")),
    ("SCHEDULING", ("Milestones", "Restrictions")),
    ("WARRANTY", ("Special Warranty",)),
    ("SYSTEM STARTUP", ("Startup", "Testing and Adjusting")),
    ("OWNER'S INSTRUCTIONS", ("Demonstration", "Training")),
    ("COMMISSIONING", ("Commissioning Requirements", "Functional Testing")),
    ("MAINTENANCE", ("Extra Materials", "Maintenance Service")),
    ("MANUFACTURERS", ("Acceptable Manufacturers", "Substitutions")),
    ("EXISTING PRODUCTS", ("Reuse", "Removal and Salvage")),
    ("MATERIALS", ("Materials", "Performance Criteria")),
    ("MANUFACTURED UNITS", ("Products", "Assemblies")),
    ("EQUIPMENT", ("Equipment", "Accessories")),
    ("COMPONENTS", ("Components", "Accessories")),
    ("ACCESSORIES", ("Accessories", "Fasteners")),
    ("MIXES", ("Mix Design", "Mixing")),
    ("FABRICATION", ("Shop Assembly", "Fabrication Tolerances")),
    ("FINISHES", ("Shop Priming, Shop Finishing",)),
    ("SOURCE QUALITY CONTROL", ("Tests, Inspection", "Verification of Performance")),
    ("INSTALLERS", ("Qualifications", "Experience")),
    ("EXAMINATION", ("Site Verification of Conditions",)),
    ("PREPARATION", ("Protection", "Surface Preparation")),
    ("ERECTION", ("Erection", "Tolerances")),
    ("INSTALLATION", ("Installation", "Tolerances")),
    ("APPLICATION", ("Application", "Tolerances")),
    ("CONSTRUCTION", ("Special Techniques", "Interface with Other Work", "Sequences of Operation", "Site Tolerances")),
    ("REPAIR/RESTORATION", ("Repair", "Restoration")),
    ("RE-INSTALLATION", ("Re-installation", "Adjustment")),
    ("FIELD QUALITY CONTROL", ("Site Tests, Inspection", "Manufacturers' Field Services")),
    ("ADJUSTING", ("Adjustment", "Balancing")),
    ("CLEANING", ("Cleaning", "Waste Disposal")),
    ("DEMONSTRATION", ("Demonstration", "Training")),
    ("PROTECTION", ("Protection", "Repair of Damage")),
    ("SCHEDULES", ("Schedules",)),
])
_PART_VALUES: Final[frozenset[str]] = frozenset(part.value for part in SectionPart)
_METHOD_VALUES: Final[frozenset[str]] = frozenset(method.value for method in SpecMethod)
_SUBMITTAL_VALUES: Final[frozenset[str]] = frozenset(kind.value for kind in SubmittalClass)
_ROLE_VALUES: Final[frozenset[str]] = frozenset(role.value for role in ParagraphRole)
_REFERENCES: Final[str] = _ARTICLES[SectionPart.GENERAL][1]  # the `REFERENCES` article title the citation reconciliation lists against

# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class SpecFault:
    # closed admission-fault vocabulary carrying its offending token; `combined` folds every casualty so a
    # whole section reports each bad article rather than aborting first.
    tag: Literal[
        "bad_section", "unknown_article", "duplicate_article", "bad_method", "bad_submittal", "bad_role", "depth_overflow", "invalid_payload",
        "aggregate"
    ] = tag()
    bad_section: str = case()  # the MasterFormat number failed `ClassCode.parse`
    unknown_article: str = case()  # an article title or part outside the SectionFormat roster
    duplicate_article: str = case()  # a repeated (part, title) identity in one admitted section
    bad_method: str = case()  # a paragraph method outside the four SpecMethod cases
    bad_submittal: str = case()  # a submittal class outside the three SubmittalClass cases
    bad_role: str = case()  # a paragraph role outside the two ParagraphRole cases
    depth_overflow: int = case()  # a paragraph nested past the deepest CSI PageFormat level
    invalid_payload: tuple[str, ...] = case()  # the `ValidationError.errors()` `loc` paths the shape gate reports
    aggregate: tuple["SpecFault", ...] = case()

    @staticmethod
    def _members(fault: "SpecFault", /) -> tuple["SpecFault", ...]:
        return fault.aggregate if fault.tag == "aggregate" else (fault,)

    @staticmethod
    def combined(left: "SpecFault", right: "SpecFault", /) -> "SpecFault":
        return SpecFault(aggregate=(*SpecFault._members(left), *SpecFault._members(right)))


# --- [MODELS] ---------------------------------------------------------------------------


class Paragraph(Struct, frozen=True):
    # recursive specification content node; a `NOTE` role strips at issue (audited from the source),
    # numbered by depth in `to_document`.
    text: str
    title: Option[str] = Nothing
    role: ParagraphRole = ParagraphRole.CONTENT
    method: Option[SpecMethod] = Nothing
    submittal: Option[SubmittalClass] = Nothing
    references: tuple[str, ...] = ()  # cited published standards, e.g. `ASTM C150`
    children: tuple["Paragraph", ...] = ()

    def __post_init__(self) -> None:
        if not self.text.strip() or any(not reference.strip() for reference in self.references):
            raise ValueError("paragraph text and references must not be empty")
        if self.title.map(lambda title: not title.strip()).default_value(False):
            raise ValueError("optional paragraph title must not be empty")


class Article(Struct, frozen=True):
    part: SectionPart
    title: str  # one primary title from `_ARTICLES[part]`
    paragraphs: tuple[Paragraph, ...] = ()

    def __post_init__(self) -> None:
        if self.title not in _ARTICLES[self.part]:
            raise ValueError("article title is outside its SectionFormat part")


class SpecVerdict(Struct, frozen=True):
    parts_present: int
    articles: int
    paragraphs: int
    notes: int  # specifier-note paragraphs the issue projection strips
    fill_ins: int  # unresolved `[____]`/`<Insert>` blanks over the CONTENT paragraphs
    off_checklist: int  # first-level CONTENT paragraph headings off the article's `_SUBORDINATE` checklist
    max_depth: int
    references: int  # total reference-standard citation occurrences over the paragraph walk
    standards: int  # distinct reference-standard designations the section invokes
    methods: frozendict[SpecMethod, int]
    submittals: frozendict[SubmittalClass, int]
    ordered: bool  # every part's articles appear in canonical `_ARTICLES` order
    main_works: int  # `_MAIN_WORK` titles the EXECUTION part carries — the exactly-one invariant's count
    coverage: tuple[str, ...]  # accumulated coverage-fault tags

    def facts(self) -> frozendict[str, object]:
        return frozendict({
            "parts": self.parts_present,
            "articles": self.articles,
            "paragraphs": self.paragraphs,
            "notes": self.notes,
            "fill_ins": self.fill_ins,
            "off_checklist": self.off_checklist,
            "max_depth": self.max_depth,
            "references": self.references,
            "standards": self.standards,
            "ordered": self.ordered,
            "main_works": self.main_works,
            "coverage": ",".join(self.coverage),
        })


class ParagraphPayload(TypedDict, closed=True):  # the raw content node ingress — codes as strings, admitted once
    text: Required[ReadOnly[str]]
    title: NotRequired[ReadOnly[str]]
    role: NotRequired[ReadOnly[str]]
    method: NotRequired[ReadOnly[str]]
    submittal: NotRequired[ReadOnly[str]]
    references: NotRequired[ReadOnly[tuple[str, ...]]]
    children: NotRequired[ReadOnly[tuple["ParagraphPayload", ...]]]


class ArticlePayload(TypedDict, closed=True):
    part: Required[ReadOnly[str]]
    title: Required[ReadOnly[str]]
    paragraphs: NotRequired[ReadOnly[tuple[ParagraphPayload, ...]]]


class SpecPayload(TypedDict, closed=True):
    section: Required[ReadOnly[str]]  # the MasterFormat number, parsed through `ClassCode`
    title: Required[ReadOnly[str]]
    articles: Required[ReadOnly[tuple[ArticlePayload, ...]]]


class _Tally(Struct, frozen=True):
    # one-pass audit seed: every walked-paragraph statistic advances in one step, so the audit never
    # re-walks the tree per check; histograms and citation partitions ride immutable unions.
    paragraphs: int = 0
    notes: int = 0
    fill_ins: int = 0
    references: int = 0
    max_depth: int = 0
    off_checklist: int = 0
    methods: frozendict[SpecMethod, int] = frozendict()
    submittals: frozendict[SubmittalClass, int] = frozendict()
    listed: frozenset[str] = frozenset()  # citations inside the REFERENCES article
    cited: frozenset[str] = frozenset()  # citations everywhere else — `cited - listed` is the unlisted gap


# --- [CONSTANTS] ------------------------------------------------------------------------

_PAYLOAD: Final = TypeAdapter(SpecPayload)
_FAULTS: Final[tuple[type[Exception], ...]] = (RuntimeError, ValueError, KeyError, OSError)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _walk(articles: tuple[Article, ...], /) -> Iterator[tuple[Article, Paragraph, int, bool]]:
    # one flattening carrying the audit discriminants: owning article, depth, and first-level position.
    def down(article: Article, paragraph: Paragraph, depth: int, first: bool, /) -> Iterator[tuple[Article, Paragraph, int, bool]]:
        yield (article, paragraph, depth, first)
        for child in paragraph.children:
            yield from down(article, child, depth + 1, False)  # bounded by `_MAX_LEVEL`, so native recursion is safe

    for article in articles:
        for paragraph in article.paragraphs:
            yield from down(article, paragraph, 1, True)


def _stepped(acc: _Tally, walked: tuple[Article, Paragraph, int, bool], /) -> _Tally:
    article, paragraph, depth, first = walked
    content = paragraph.role is ParagraphRole.CONTENT
    at_references = article.title == _REFERENCES
    title = paragraph.title.default_value("")
    off = first and content and bool(title) and article.title in _SUBORDINATE and title not in _SUBORDINATE[article.title]
    return _Tally(
        paragraphs=acc.paragraphs + 1,
        notes=acc.notes + (paragraph.role is ParagraphRole.NOTE),
        fill_ins=acc.fill_ins + (len(_FILL_IN.findall(paragraph.text)) if content else 0),
        references=acc.references + len(paragraph.references),
        max_depth=max(acc.max_depth, depth),
        off_checklist=acc.off_checklist + off,
        methods=paragraph.method.map(lambda method: acc.methods | {method: acc.methods.get(method, 0) + 1}).default_value(acc.methods),
        submittals=paragraph.submittal.map(lambda kind: acc.submittals | {kind: acc.submittals.get(kind, 0) + 1}).default_value(acc.submittals),
        listed=acc.listed | (frozenset(paragraph.references) if at_references else frozenset()),
        cited=acc.cited | (frozenset(paragraph.references) if not at_references else frozenset()),
    )


def _ordered(part: SectionPart, titles: tuple[str, ...], /) -> bool:
    # every present article appears in canonical `_ARTICLES[part]` order — strictly increasing, since admission
    # already refused repeated identities, so an equal position pair is itself an order fault.
    positions = tuple(_ARTICLES[part].index(title) for title in titles if title in _ARTICLES[part])
    return all(earlier < later for earlier, later in zip(positions, positions[1:], strict=False))


def _accumulated[T](results: Block[Result[T, SpecFault]], /) -> Result[tuple[T, ...], SpecFault]:
    # combine every fault through the monoid, return the whole set only when the casualty set is empty — the
    # `Validation`-style fold `register#REGISTER` shares.
    faults = results.choose(lambda outcome: outcome.swap().to_option())
    return Ok(tuple(results.choose(lambda outcome: outcome.to_option()))) if faults.is_empty() else Error(faults.reduce(SpecFault.combined))


def _casualties(candidates: Iterable[Option[SpecFault]], /) -> Block[SpecFault]:
    return Block.of_seq(candidates).choose(lambda held: held)


def _admit_paragraph(payload: ParagraphPayload, depth: int, /) -> Result[Paragraph, SpecFault]:
    # every independent field check contributes its casualty — a paragraph carrying a bad role AND a bad method
    # reports both; recursion halts at the depth cap so an adversarial payload never grows the stack past it.
    role = payload.get("role", ParagraphRole.CONTENT.value)
    method = payload.get("method", "")
    submittal = payload.get("submittal", "")
    children = (
        _accumulated(Block.of_seq(_admit_paragraph(child, depth + 1) for child in payload.get("children", ())))
        if depth <= _MAX_LEVEL
        else Ok(())
    )
    faults = _casualties((
        Some(SpecFault(invalid_payload=("text",))) if not payload["text"].strip() else Nothing,
        # a PRESENT title that strips to nothing is a payload fault, never a silent demotion to omission —
        # only a genuinely absent key constructs `Nothing` below.
        Some(SpecFault(invalid_payload=("title",))) if "title" in payload and not payload["title"].strip() else Nothing,
        Some(SpecFault(invalid_payload=("references",))) if any(not reference.strip() for reference in payload.get("references", ())) else Nothing,
        Some(SpecFault(depth_overflow=depth)) if depth > _MAX_LEVEL else Nothing,
        Some(SpecFault(bad_role=role)) if role not in _ROLE_VALUES else Nothing,
        Some(SpecFault(bad_method=method)) if method and method not in _METHOD_VALUES else Nothing,
        Some(SpecFault(bad_submittal=submittal)) if submittal and submittal not in _SUBMITTAL_VALUES else Nothing,
        children.swap().to_option(),
    ))
    if not faults.is_empty():
        return Error(faults.reduce(SpecFault.combined))
    return children.map(
        lambda kids: Paragraph(
            text=payload["text"],
            title=Some(title) if (title := payload.get("title", "").strip()) else Nothing,
            role=ParagraphRole(role),
            method=Some(SpecMethod(method)) if method else Nothing,
            submittal=Some(SubmittalClass(submittal)) if submittal else Nothing,
            references=payload.get("references", ()),
            children=kids,
        )
    )


def _admit_article(payload: ArticlePayload, /) -> Result[Article, SpecFault]:
    # title roster depends on the part (dependent check), while the paragraph fold is independent of both —
    # its casualties join the header faults rather than being shadowed by them.
    part_ok = payload["part"] in _PART_VALUES
    title_ok = part_ok and payload["title"] in _ARTICLES[SectionPart(payload["part"])]
    paragraphs = _accumulated(Block.of_seq(_admit_paragraph(entry, 1) for entry in payload.get("paragraphs", ())))
    faults = _casualties((
        Nothing if part_ok else Some(SpecFault(unknown_article=payload["part"])),
        Some(SpecFault(unknown_article=payload["title"])) if part_ok and not title_ok else Nothing,
        paragraphs.swap().to_option(),
    ))
    if not faults.is_empty():
        return Error(faults.reduce(SpecFault.combined))
    return paragraphs.map(lambda paras: Article(part=SectionPart(payload["part"]), title=payload["title"], paragraphs=paras))


def _audited(spec: "Spec", /) -> SpecVerdict:
    # ONE seed fold over the flattened walk carries every per-paragraph statistic; only the order, main-work,
    # and coverage checks read the article roster beside it.
    articles = spec.articles
    tally = reduce(_stepped, _walk(articles), _Tally())
    present = {article.part for article in articles}
    ordered = all(_ordered(part, tuple(article.title for article in articles if article.part is part)) for part in present)
    main_works = sum(1 for article in articles if article.part is SectionPart.EXECUTION and article.title in _MAIN_WORK)
    coverage = tuple(
        marker
        for failed, marker in (
            (not articles, "empty_section"),
            (not ordered, "out_of_order"),
            (SectionPart.EXECUTION in present and main_works == 0, "missing_main_work"),
            (main_works > 1, "multiple_main_work"),
            (bool(tally.fill_ins), "unresolved_fill_ins"),
            (bool(tally.cited - tally.listed), "unlisted_references"),
            (bool(tally.off_checklist), "off_checklist_titles"),
        )
        if failed
    )
    return SpecVerdict(
        parts_present=len(present),
        articles=len(articles),
        paragraphs=tally.paragraphs,
        notes=tally.notes,
        fill_ins=tally.fill_ins,
        off_checklist=tally.off_checklist,
        max_depth=tally.max_depth,
        references=tally.references,
        standards=len(tally.listed | tally.cited),
        methods=frozendict({method: tally.methods.get(method, 0) for method in SpecMethod}),
        submittals=frozendict({kind: tally.submittals.get(kind, 0) for kind in SubmittalClass}),
        ordered=ordered,
        main_works=main_works,
        coverage=coverage,
    )


def submittal_register(specs: "Spec | Iterable[Spec]", /) -> tuple[frozendict[str, str], ...]:
    # manual-wide CSI submittal log — every submittal-carrying paragraph across the section set as one flat
    # row stream a `visualization/table#TABLE` `TablePlan.of` frame renders and `delivery/register#REGISTER` keys.
    match specs:
        case Spec() as lone:
            manual: tuple[Spec, ...] = (lone,)
        case stream:
            manual = tuple(stream)
    return tuple(
        frozendict({
            "section": spec.section.render(),
            "title": spec.title,
            "article": article.title,
            "item": paragraph.title.default_value(paragraph.text),
            "class": kind.value,
            "method": paragraph.method.map(lambda method: method.value).default_value(""),
            "references": ", ".join(paragraph.references),
        })
        for spec in manual
        for article, paragraph, _depth, _first in _walk(spec.articles)
        for kind in paragraph.submittal.to_list()
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


class Spec(Struct, frozen=True):
    section: ClassCode
    title: str
    lane: LanePolicy
    articles: tuple[Article, ...] = ()
    page: PageFormat = PageFormat()

    def __post_init__(self) -> None:
        if self.section.system is not ClassSystem.MASTERFORMAT or not self.title.strip():
            raise ValueError("specification requires a MasterFormat section and title")

    @classmethod
    def admit(cls, lane: LanePolicy, page: PageFormat = PageFormat(), /, **payload: Unpack[SpecPayload]) -> Result[Self, SpecFault]:
        # shape gate through `_PAYLOAD` (the named TypeAdapter statement seam), then the section-number seam
        # and the article fold accumulated TOGETHER — a bad section number never shadows the article casualties.
        try:
            valid = _PAYLOAD.validate_python(payload)
        except ValidationError as fault:
            return Error(SpecFault(invalid_payload=tuple("/".join(str(at) for at in error["loc"]) for error in fault.errors())))
        parsed = ClassCode.parse(ClassSystem.MASTERFORMAT, valid["section"]).map_error(lambda _cause: SpecFault(bad_section=valid["section"]))
        admitted = _accumulated(Block.of_seq(_admit_article(entry) for entry in valid["articles"]))
        # (part, title) identity is set-level: a repeat contributes one duplicate_article casualty per identity,
        # so a doubled main-work article refuses at admission rather than collapsing inside a later audit frozenset.
        repeated = Counter((entry["part"], entry["title"]) for entry in valid["articles"])
        faults = _casualties((
            Some(SpecFault(invalid_payload=("title",))) if not valid["title"].strip() else Nothing,
            parsed.swap().to_option(),
            admitted.swap().to_option(),
            *(Some(SpecFault(duplicate_article=f"{part}:{title}")) for (part, title), count in repeated.items() if count > 1),
        ))
        if not faults.is_empty():
            return Error(faults.reduce(SpecFault.combined))
        return parsed.bind(lambda code: admitted.map(lambda arts: cls(section=code, title=valid["title"].strip(), lane=lane, articles=arts, page=page)))

    def to_document(self) -> DocumentNode:
        # ordinal PATH is the numbering; `document/emit#DOCUMENT` folds FROM this tree. The CSI-mandated
        # `END OF SECTION` marker closes the tree as its terminal block.
        parts = tuple(self._part_node(part) for part in SectionPart if any(article.part is part for article in self.articles))
        close = BlockNode(
            meta=self._meta("P", "END OF SECTION"), block=BlockKind.PARAGRAPH, runs=(self._run(self.page.end_of_section(), weight=700),)
        )
        heading = (self._run(f"{self.section.render()}  {self.title.upper()}", weight=700),)
        return SectionNode(
            meta=self._meta("Sect", self.section.render(), classification=Some(self.section.render())),
            level=1,
            heading=heading,
            children=(*parts, close),
        )

    def audit(self) -> SpecVerdict:
        return _audited(self)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # ONE identity: the full frozen input spec, minted PRE-RUN — `emit`, `contribute`, and the receipt all
        # thread it, so a section number shared by two different manuals never collides and no second key regime
        # hashes the encoded bytes.
        # `ContentIdentity.key` is the bare mint (`of` returns the railed `RuntimeRail[ContentKey]`).
        return ContentIdentity.key(f"spec-{self.section.render()}", _CANON.encode((self.section, self.title, self.articles, self.page)))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        verdict = self.audit()
        return (await async_boundary(f"spec.{self.section.render()}", self._encoded_rail, catch=_FAULTS)).map(
            lambda payload: ArtifactReceipt.Spec(
                self._key, self.section.render(), self._division, verdict.parts_present, verdict.articles, len(payload)
            )
        )

    async def _encoded_rail(self) -> bytes:
        # offloaded encode returns the PAYLOAD BYTES the receipt counts; identity stays `_key`'s.
        crossed = await self.lane.offload(Kernel.of(self._encoded, KernelTrait.RELEASING))
        return crossed.default_with(self._raise)

    @staticmethod
    def _raise(fault: object) -> bytes:
        raise ValueError(str(fault))

    @property
    def _division(self) -> int:
        match self.section.division():
            case Option(tag="some", some=division):
                return division
            case Option(tag="none"):
                raise ValueError("specification section is not MasterFormat")
            case _ as unreachable:
                assert_never(unreachable)

    def _encoded(self) -> bytes:
        return encode(self.to_document())

    def contribute(self) -> "Iterable[Receipt]":
        payload = self._encoded()  # the one encode the byte count derives from; the key is the shared `_key`
        verdict = self.audit()
        receipt = ArtifactReceipt.Spec(self._key, self.section.render(), self._division, verdict.parts_present, verdict.articles, len(payload))
        yield from receipt.contribute()

    def _part_node(self, part: SectionPart, /) -> DocumentNode:
        number = _PART_NUMBER[part]
        articles = tuple(
            self._article_node(article, number, ordinal) for ordinal, article in enumerate((a for a in self.articles if a.part is part), start=1)
        )
        heading = (self._run(self.page.label(number, ()) + f" {part.value.upper()}", weight=700),)
        return SectionNode(meta=self._meta("Sect", part.value), level=2, heading=heading, children=articles)

    def _article_node(self, article: Article, part: int, ordinal: int, /) -> DocumentNode:
        # a NOTE never consumes an ordinal, so the CONTENT ordinal is a running `accumulate` count.
        label = self.page.label(part, (ordinal,))
        # NOTE guidance STRIPS at issue: specifier notes never enter the issued tree (the audit counts them off
        # source spec strips them), so no lowering can render them — BlockKind.ARTIFACT is tagging semantics, not elision.
        retained = tuple(paragraph for paragraph in article.paragraphs if paragraph.role is not ParagraphRole.NOTE)
        numbers = tuple(accumulate(int(paragraph.role is ParagraphRole.CONTENT) for paragraph in retained))
        paragraphs = tuple(
            self._paragraph_node(paragraph, part, (ordinal, number)) for paragraph, number in zip(retained, numbers, strict=True)
        )
        heading = (self._run(f"{label}  {article.title}", weight=700),)
        return SectionNode(meta=self._meta("H2", label, article.title), level=3, heading=heading, children=paragraphs)

    def _paragraph_node(self, paragraph: Paragraph, part: int, path: tuple[int, ...], /) -> DocumentNode:
        kept = tuple(child for child in paragraph.children if child.role is not ParagraphRole.NOTE)
        numbers = tuple(accumulate(int(child.role is ParagraphRole.CONTENT) for child in kept))
        children = tuple(self._paragraph_node(child, part, (*path, number)) for child, number in zip(kept, numbers, strict=True))
        label = self.page.label(part, path)
        # a subordinate heading rides its own bold lead run before the body run; a bare paragraph is one run.
        runs = paragraph.title.map(
            lambda title: (self._run(f"{label}  {title}", weight=700), self._run(paragraph.text, anchor=label))
        ).default_value((self._run(f"{label}  {paragraph.text}"),))
        return BlockNode(
            meta=self._meta("P", label, paragraph.title.default_value(""), paragraph.text),
            block=BlockKind.PARAGRAPH,
            runs=runs,
            children=children,
        )


    def _run(self, text: str, /, *, weight: int = 400, anchor: str = "") -> RunNode:
        # `anchor` carries the position label for a bare body run whose text alone repeats across paragraphs
        return RunNode(meta=self._meta("Span", text, anchor), text=text, font_key=self.page.font, size=self.page.size, weight=weight)

    def _meta(self, role: str, token: str, /, *content: str, classification: Option[str] = Nothing) -> NodeMeta:
        # section's CSI ClassCode rides the root SectionNode's NodeMeta.classification so the one lowered
        # model tree carries the code the classify#CODE ReferenceIndex keys the drawing<->spec cross-ref on.
        match classification:
            case Option(tag="some", some=value):
                classified = value
            case Option(tag="none"):
                classified = UNSET
            case _ as unreachable:
                assert_never(unreachable)
        # preimage joins position AND canonical content under an unambiguous separator: two distinct nodes that
        # share a generated label (a repeated non-CONTENT ordinal, a duplicated body run) still key apart, and the
        # `\x1f` join forecloses the ("a","bc") == ("ab","c") concat collision a bare ":" join admits.
        return NodeMeta(
            key=ContentIdentity.key(f"spec-{role}", "\x1f".join((self.section.render(), token, *content)).encode()),
            role=role,
            page=0,
            classification=classified,
        )


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = ["Article", "Paragraph", "ParagraphRole", "SectionPart", "Spec", "SpecFault", "SpecMethod", "SpecVerdict", "SubmittalClass", "submittal_register"]
```
