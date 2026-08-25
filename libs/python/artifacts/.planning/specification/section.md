# [PY_ARTIFACTS_SECTION]

One owner holds the construction-specification production — the CSI/CSC SectionFormat three-part section (`PART 1 GENERAL` / `PART 2 PRODUCTS` / `PART 3 EXECUTION`), its article vocabulary, the four methods of specifying, and the PageFormat numbering. `Spec` admits a raw payload EXACTLY ONCE through the closed `SpecPayload`, validates its MasterFormat section number against `specification/classify#CODE` and its article titles against the owned roster under an accumulating disposition, lowers the validated section INTO a `document/model#NODE` `DocumentNode` tree, and contributes the one new `core/receipt#RECEIPT` `ArtifactReceipt.Spec` case — so a specification section is one schedulable `core/plan#PLAN` `ArtifactWork` producer beside every other artifact, its typeset emission owned downstream by `document/emit#DOCUMENT` folding FROM the tree. This owner authors the section semantics and the numbered node tree; it renders no page and re-authors no IFC — the QTO facts a specification cites arrive from `dotnet:Rasm.Bim` through `visualization/table#TABLE`.

Owned families carry the three-part SectionFormat spine, canonical article roster, subordinate-title checklist, exact-one main-work alternatives, specifying methods, submittal classes, and paragraph roles. `NOTE` guidance strips at issue — the audit counts notes off the source spec and no lowering renders them; accumulating admission retains independent casualties; one audit fold derives ordering, cardinality, histogram, fill-in, checklist, and citation evidence. `NumberScheme.alphanumeric` and `decimal` carry only their own policy payloads, and lowering terminates with `END OF SECTION`.

## [01]-[INDEX]

- [02]-[PAGE]: `PageFormat` — the CSI PageFormat numbering and page-presentation substrate, `label` projecting one ordinal path to its designation across both numbering modes.
- [03]-[SECTION]: `Spec` — the CSI SectionFormat producer over the owned article vocabularies, lowering a validated section into the `document/model#NODE` tree and minting the one `ArtifactReceipt.Spec` case.

## [02]-[PAGE]

- Owner: `PageFormat` carries typography, page size, and one closed `NumberScheme`. `NumberScheme.alphanumeric(article_pad)` owns CSI article padding; `decimal` is tag-only because UFGS numbering has no padding axis.
- Cases: `NumberScheme` carries `alphanumeric(article_pad)` and tag-only `decimal`; `NumberLevel` carries the eight-level hierarchy; `Alphabet` and `Decoration` carry glyph axes; `PageSize` carries sheet regimes.
- Entry: `NumberScheme.label(part, path)` normalizes on the path shape — an empty path yields `PART {part}`, a length-1 path the part-prefixed article number, a deeper path the local level glyph (`ALPHANUMERIC` reads `_LEVEL_STYLE[NumberLevel(len(path)-1)]`, `DECIMAL` joins the whole path) — so one entrypoint owns every level and mode, the discriminant the path length and mode, never a `paragraph_label`/`subparagraph_label` method family.
- Auto: `label` derives every designation from the ONE `_LEVEL_STYLE` correspondence — `_glyph` projecting the ordinal through its alphabet and `_wrap` applying the level's punctuation, both closed by `assert_never`. `page_footer(section, page)` projects the CSI `{section} - {page}` footer, and `end_of_section` the marker CSI PageFormat mandates AFTER the `SCHEDULES` article — the marker `to_document` appends as the tree's terminal block.
- Receipt: none — `PageFormat` is pure presentation policy; the numbering travels INTO the `Spec` producer's tree and `ArtifactReceipt.Spec` facts, exactly as `specification/classify#CODE` contributes none.
- Packages: `msgspec` (`Struct(frozen=True)` the value objects, hashable so a shared `PageFormat` keys deterministically); `expression` (`Map` the `_LEVEL_STYLE` correspondence); `enum` (the closed numbering vocabularies). No runtime import.
- Growth: a new numbering regime adds one `NumberScheme` case and `label` arm; a new nesting level adds one `NumberLevel` and `_LEVEL_STYLE` row; new glyph and punctuation policies add one enum value and total arm.
- Boundary: this owner authors presentation policy, never bytes or receipts.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import IntEnum, StrEnum
from math import isfinite
from typing import Final, Literal, assert_never

from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

# --- [TYPES] ----------------------------------------------------------------------------


class NumberLevel(IntEnum):
    ARTICLE = 0
    PARAGRAPH = 1
    SUBPARAGRAPH = 2
    CLAUSE = 3
    SUBCLAUSE = 4
    ITEM = 5
    SUBITEM = 6
    DETAIL = 7


class Alphabet(StrEnum):
    ARABIC = "arabic"
    UPPER = "upper"
    LOWER = "lower"


class Decoration(StrEnum):
    DOT = "dot"
    CLOSE_PAREN = "close_paren"
    BOTH_PARENS = "both_parens"


class PageSize(StrEnum):
    LETTER = "letter"
    A4 = "a4"


# --- [TABLES] ---------------------------------------------------------------------------

_LEVEL_STYLE: Final[Map[NumberLevel, tuple[Alphabet, Decoration]]] = Map.of_seq([
    (NumberLevel.PARAGRAPH, (Alphabet.UPPER, Decoration.DOT)),
    (NumberLevel.SUBPARAGRAPH, (Alphabet.ARABIC, Decoration.DOT)),
    (NumberLevel.CLAUSE, (Alphabet.LOWER, Decoration.DOT)),
    (NumberLevel.SUBCLAUSE, (Alphabet.ARABIC, Decoration.CLOSE_PAREN)),
    (NumberLevel.ITEM, (Alphabet.LOWER, Decoration.CLOSE_PAREN)),
    (NumberLevel.SUBITEM, (Alphabet.ARABIC, Decoration.BOTH_PARENS)),
    (NumberLevel.DETAIL, (Alphabet.LOWER, Decoration.BOTH_PARENS)),
])
_MAX_LEVEL: Final[int] = NumberLevel.DETAIL.value

# --- [OPERATIONS] -----------------------------------------------------------------------


def _alpha(ordinal: int, /, *, upper: bool) -> str:
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
    size: float = 10.0
    font: str = "body"
    page_size: PageSize = PageSize.LETTER

    def __post_init__(self) -> None:
        if not isfinite(self.size) or self.size <= 0.0 or not self.font.strip():
            raise ValueError("page typography requires finite positive size and a font key")

    def label(self, part: int, path: tuple[int, ...], /) -> str:
        return self.numbering.label(part, path)

    def page_footer(self, section: str, page: int, /) -> str:
        return f"{section} - {page}"

    def end_of_section(self) -> str:
        return "END OF SECTION"

    def uppercase(self, level: NumberLevel, /) -> bool:
        return level is NumberLevel.ARTICLE


# --- [CONSTANTS] ------------------------------------------------------------------------

CSI_PAGEFORMAT: Final[NumberScheme] = NumberScheme(alphanumeric=2)
UFGS_DECIMAL: Final[NumberScheme] = NumberScheme(decimal=None)


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = ["Alphabet", "CSI_PAGEFORMAT", "Decoration", "NumberLevel", "NumberScheme", "PageFormat", "PageSize", "UFGS_DECIMAL"]
```

## [03]-[SECTION]

- Owner: `Spec` — the CSI SectionFormat producer over a `section: ClassCode` (the MasterFormat number IS the section identity, admitted through `specification/classify#CODE`), a `title`, a `tuple[Article, ...]` in canonical order, the runtime `lane: LanePolicy` the encode offloads on, and its `PageFormat` — lowering the validated section into a `document/model#NODE` `DocumentNode` tree and contributing one `ArtifactReceipt.Spec` case, never a stringly heading tree or a parallel per-part builder. `Article` binds one `SectionPart`, one validated `_ARTICLES` title, and its paragraphs; `Paragraph` is the recursive content node carrying its subordinate heading, text, `ParagraphRole` disposition, optional `SpecMethod`/`SubmittalClass`, `references`, and `children`. No specification library is admitted — the SectionFormat algebra is this owner's composition over the classify, document, and numbering owners.
- Cases: owned SectionFormat vocabularies are closed families authored to the CSI MP-2-2 published cardinality — `SectionPart` the three-part spine, `_ARTICLES` the primary article roster per part in published order (order IS load-bearing for the `_audited` canonical-order check), `_SUBORDINATE` the paragraph-title checklist each article carries, `_MAIN_WORK` the alternative-main-work set an `EXECUTION` part selects exactly one of — `main_works` counts the selection so zero, one, and multiple are three distinguishable audit outcomes. `SpecMethod` closes the four methods of specifying, `SubmittalClass` the three submittal regimes, `ParagraphRole` the `CONTENT`/`NOTE` editorial disposition. `SpecFault` carries the accumulating fault vocabulary with its `combined` monoid, and `SpecVerdict` carries the QA evidence — tallies, note/fill-in/off-checklist counts, reference and distinct-standard totals, method + submittal histograms, the canonical-order flag, the main-work count, and the accumulated coverage-fault tags.
- Entry: `Spec.admit(lane, page, /, **payload)` is the one boundary ingress — the `SpecPayload` shape gate through `_PAYLOAD`, then the section-number seam and the article fold composed under one accumulating disposition: the independent checks on one paragraph (depth, role, method, submittal, children) each contribute their casualty to the same `SpecFault.aggregate`, an article's part/title faults join its paragraphs' casualties, and the section-number fault joins the article casualties — a whole malformed section reports EVERY casualty at once, never the first per node. `to_document()` is the lowering entrypoint, `emit()` the schedulable `ArtifactWork`, `contribute()` the receipt, `audit()` the verdict — one polymorphic producer, never a per-part or per-target emit family. `submittal_register(specs)` is the manual-level modality over `Spec | Iterable[Spec]` — the flat CSI submittal-log row stream every `SubmittalClass`-carrying paragraph contributes, the schedule a `visualization/table#TABLE` frame renders and `delivery/register#REGISTER` keys.
- Auto: `to_document` lowers the section in one pass — the `Spec` a level-1 `SectionNode`, each present part a level-2, each article a `PageFormat`-numbered level-3, each paragraph a `BlockNode` recursing its sub-tree with the ordinal PATH threaded down so the numbering is the tree's own structure, and the `end_of_section` marker closing the tree as its terminal block. A `NOTE` paragraph STRIPS at issue — it never enters the issued tree, `BlockKind.ARTIFACT` being tagging semantics rather than elision — so the CONTENT ordinal `_article_node` threads is a running `accumulate` count that never advances over a note — the retained paragraphs number contiguously with no gap where a stripped note sat, the one place a naive producer leaks a numbering hole. `_audited` is ONE seed fold over the `_walk` flattening — the `_Tally` frozen seed carries the paragraph/note/fill-in/reference counts, the depth high-water, the off-checklist count, the method/submittal histograms, and the listed/cited citation partition, each stepped per walked paragraph — with only the per-article order, main-work, and coverage checks reading the article roster beside it, never a per-check re-walk.
- Receipt: `Spec` contributes the one new `core/receipt#RECEIPT` `ArtifactReceipt.Spec` case — the section number, the `division()` head the `specification/classify#CODE` crosswalk keys on, the present-part and article counts, and the encoded-tree byte count. ONE identity serves `emit`, `contribute`, and the receipt: `_key` derives from the full frozen input spec (`section`, `title`, `articles`, `page`), so two sections sharing a number but differing in content never collide, and the byte count reads off the one encode `contribute` computes — never a second key regime hashing the encoded bytes. `_emit` also awaits `Journal.record` over `receipt.evidence()` — a produced section is `OPERATIONAL` production trail under the case's own retention row, its diff naming the section number, division, and part/article cardinality a later revision compares against; the seat is that awaitable fold, because recording suspends where the synchronous `contribute` twin cannot.
- Packages: `msgspec` (the `Struct(frozen=True)` value objects and the deterministic node encode); `expression` (`tagged_union` the `SpecFault`; `Block` the accumulating admission fold; the `Result`/`Option` rails); `pydantic` (`TypeAdapter` the `_PAYLOAD` gate over the recursive `SpecPayload`, `ValidationError.errors()` the structured `loc` paths the `invalid_payload` case carries); `frozendict` (the owned vocabulary correspondences and verdict histograms); `re` (`_FILL_IN` the unresolved-fill-in `Pattern`); `itertools.accumulate` (the CONTENT-only ordinal); runtime (`ContentIdentity`/`ContentKey`, `RuntimeRail`/`async_boundary`, `Journal` the durable seat's one writer, `LanePolicy`/`Kernel`/`KernelTrait` the off-loop encode); `specification/classify#CODE` (`ClassCode`), `document/model#NODE` (the `SectionNode`/`BlockNode` tree and `encode`), and `core/receipt#RECEIPT` (`ArtifactReceipt.Spec`), composed never re-authored. No new external library.
- Growth: a new article is one `_ARTICLES` row; a new subordinate title one `_SUBORDINATE` row; a new main-work title one `_MAIN_WORK` member; a new method/submittal/disposition one `SpecMethod`/`SubmittalClass`/`ParagraphRole` member (a `NOTE` already strips at issue); a new QA rule one `SpecVerdict` field and one `_Tally` step or article check; a new fault cause one `SpecFault` case plus one `combined` member; a new section-number system is already carried by `ClassCode`.
- Boundary: this owner authors the specification, never the rendered page — `document/emit#DOCUMENT` folds the tree to PDF/Typst/HTML, `PageFormat` owns the numbering, `specification/classify#CODE` owns `ClassCode`, and `dotnet:Rasm.Bim` owns the QTO/IFC a specification cites through `visualization/table#TABLE`.

```python
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
from rasm.runtime.journal import Journal
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait
from rasm.runtime.faults import TRANSIENT, FaultRow, RuntimeRail, async_boundary, rostered

from rasm.artifacts.core.hooks import ArtifactsLeg
from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.document.model import BlockKind, BlockNode, DocumentNode, NodeMeta, RunNode, SectionNode, encode
from rasm.artifacts.specification.classify import ClassCode, ClassSystem


if TYPE_CHECKING:
    from rasm.runtime.receipts import Receipt

# --- [TYPES] ----------------------------------------------------------------------------


class SectionPart(StrEnum):
    GENERAL = "general"
    PRODUCTS = "products"
    EXECUTION = "execution"


class SpecMethod(StrEnum):
    DESCRIPTIVE = "descriptive"
    PERFORMANCE = "performance"
    REFERENCE_STANDARD = "reference"
    PROPRIETARY = "proprietary"


class SubmittalClass(StrEnum):
    ACTION = "action"
    INFORMATIONAL = "informational"
    CLOSEOUT = "closeout"


class ParagraphRole(StrEnum):
    CONTENT = "content"
    NOTE = "note"


# --- [CONSTANTS] ------------------------------------------------------------------------

_FILL_IN: Final[re.Pattern[str]] = re.compile(r"\[_{2,}\]|<[^<>]+>")
_PART_NUMBER: Final[Map[SectionPart, int]] = Map.of_seq([(SectionPart.GENERAL, 1), (SectionPart.PRODUCTS, 2), (SectionPart.EXECUTION, 3)])
_CANON: Final = msgpack.Encoder(order="deterministic")

# --- [TABLES] ---------------------------------------------------------------------------

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
_MAIN_WORK: Final[frozenset[str]] = frozenset({"ERECTION", "INSTALLATION", "APPLICATION", "CONSTRUCTION"})
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
_REFERENCES: Final[str] = _ARTICLES[SectionPart.GENERAL][1]

# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class SpecFault:
    tag: Literal[
        "bad_section", "unknown_article", "duplicate_article", "bad_method", "bad_submittal", "bad_role", "depth_overflow", "invalid_payload",
        "aggregate"
    ] = tag()
    bad_section: str = case()
    unknown_article: str = case()
    duplicate_article: str = case()
    bad_method: str = case()
    bad_submittal: str = case()
    bad_role: str = case()
    depth_overflow: int = case()
    invalid_payload: tuple[str, ...] = case()
    aggregate: tuple["SpecFault", ...] = case()

    @staticmethod
    def _members(fault: "SpecFault", /) -> tuple["SpecFault", ...]:
        return fault.aggregate if fault.tag == "aggregate" else (fault,)

    @staticmethod
    def combined(left: "SpecFault", right: "SpecFault", /) -> "SpecFault":
        return SpecFault(aggregate=(*SpecFault._members(left), *SpecFault._members(right)))


# --- [MODELS] ---------------------------------------------------------------------------


class Paragraph(Struct, frozen=True):
    text: str
    title: Option[str] = Nothing
    role: ParagraphRole = ParagraphRole.CONTENT
    method: Option[SpecMethod] = Nothing
    submittal: Option[SubmittalClass] = Nothing
    references: tuple[str, ...] = ()
    children: tuple["Paragraph", ...] = ()

    def __post_init__(self) -> None:
        if not self.text.strip() or any(not reference.strip() for reference in self.references):
            raise ValueError("paragraph text and references must not be empty")
        if self.title.map(lambda title: not title.strip()).default_value(False):
            raise ValueError("optional paragraph title must not be empty")


class Article(Struct, frozen=True):
    part: SectionPart
    title: str
    paragraphs: tuple[Paragraph, ...] = ()

    def __post_init__(self) -> None:
        if self.title not in _ARTICLES[self.part]:
            raise ValueError("article title is outside its SectionFormat part")


class SpecVerdict(Struct, frozen=True):
    parts_present: int
    articles: int
    paragraphs: int
    notes: int
    fill_ins: int
    off_checklist: int
    max_depth: int
    references: int
    standards: int
    methods: frozendict[SpecMethod, int]
    submittals: frozendict[SubmittalClass, int]
    ordered: bool
    main_works: int
    coverage: tuple[str, ...]

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


class ParagraphPayload(TypedDict, closed=True):
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
    section: Required[ReadOnly[str]]
    title: Required[ReadOnly[str]]
    articles: Required[ReadOnly[tuple[ArticlePayload, ...]]]


class _Tally(Struct, frozen=True):
    paragraphs: int = 0
    notes: int = 0
    fill_ins: int = 0
    references: int = 0
    max_depth: int = 0
    off_checklist: int = 0
    methods: frozendict[SpecMethod, int] = frozendict()
    submittals: frozendict[SubmittalClass, int] = frozendict()
    listed: frozenset[str] = frozenset()
    cited: frozenset[str] = frozenset()


# --- [CONSTANTS] ------------------------------------------------------------------------

_PAYLOAD: Final = TypeAdapter(SpecPayload)
_FAULTS: Final[tuple[type[Exception], ...]] = (RuntimeError, ValueError, KeyError, OSError)

# --- [TABLES] ---------------------------------------------------------------------------

SECTION_ENCODE: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.SECTION, point="encode", arm="boundary", defect="section-encode", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[ArtifactsLeg]]] = rostered(Block.of_seq([SECTION_ENCODE]))

# --- [OPERATIONS] -----------------------------------------------------------------------


def _walk(articles: tuple[Article, ...], /) -> Iterator[tuple[Article, Paragraph, int, bool]]:
    def down(article: Article, paragraph: Paragraph, depth: int, first: bool, /) -> Iterator[tuple[Article, Paragraph, int, bool]]:
        yield (article, paragraph, depth, first)
        for child in paragraph.children:
            yield from down(article, child, depth + 1, False)

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
    positions = tuple(_ARTICLES[part].index(title) for title in titles if title in _ARTICLES[part])
    return all(earlier < later for earlier, later in zip(positions, positions[1:], strict=False))


def _accumulated[T](results: Block[Result[T, SpecFault]], /) -> Result[tuple[T, ...], SpecFault]:
    faults = results.choose(lambda outcome: outcome.swap().to_option())
    return Ok(tuple(results.choose(lambda outcome: outcome.to_option()))) if faults.is_empty() else Error(faults.reduce(SpecFault.combined))


def _casualties(candidates: Iterable[Option[SpecFault]], /) -> Block[SpecFault]:
    return Block.of_seq(candidates).choose(lambda held: held)


def _admit_paragraph(payload: ParagraphPayload, depth: int, /) -> Result[Paragraph, SpecFault]:
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
        try:
            valid = _PAYLOAD.validate_python(payload)
        except ValidationError as fault:
            return Error(SpecFault(invalid_payload=tuple("/".join(str(at) for at in error["loc"]) for error in fault.errors())))
        parsed = ClassCode.parse(ClassSystem.MASTERFORMAT, valid["section"]).map_error(lambda _cause: SpecFault(bad_section=valid["section"]))
        admitted = _accumulated(Block.of_seq(_admit_article(entry) for entry in valid["articles"]))
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
        return ContentIdentity.key(f"spec-{self.section.render()}", _CANON.encode((self.section, self.title, self.articles, self.page)))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        verdict = self.audit()
        settled = (await async_boundary(SECTION_ENCODE, self._encoded_rail, catch=_FAULTS)).map(
            lambda payload: ArtifactReceipt.Spec(
                self._key, self.section.render(), self._division, verdict.parts_present, verdict.articles, len(payload)
            )
        )
        match settled:
            case Result(tag="ok", ok=receipt):
                return (await Journal.record(receipt.evidence())).map(lambda _landed: receipt)
            case refused:
                return Error(refused.error)

    async def _encoded_rail(self) -> bytes:
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
        payload = self._encoded()
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
        label = self.page.label(part, (ordinal,))
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
        return RunNode(meta=self._meta("Span", text, anchor), text=text, font_key=self.page.font, size=self.page.size, weight=weight)

    def _meta(self, role: str, token: str, /, *content: str, classification: Option[str] = Nothing) -> NodeMeta:
        match classification:
            case Option(tag="some", some=value):
                classified = value
            case Option(tag="none"):
                classified = UNSET
            case _ as unreachable:
                assert_never(unreachable)
        return NodeMeta(
            key=ContentIdentity.key(f"spec-{role}", "\x1f".join((self.section.render(), token, *content)).encode()),
            role=role,
            page=0,
            classification=classified,
        )


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = ["Article", "Paragraph", "ParagraphRole", "SectionPart", "Spec", "SpecFault", "SpecMethod", "SpecVerdict", "SubmittalClass", "submittal_register"]
```

## [04]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
