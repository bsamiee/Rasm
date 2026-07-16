# [PY_ARTIFACTS_SECTION]

One owner holds the construction-specification production — the CSI/CSC SectionFormat three-part section (`PART 1 GENERAL` / `PART 2 PRODUCTS` / `PART 3 EXECUTION`), its article vocabulary, the four methods of specifying, and the PageFormat numbering. `Spec` admits a raw payload EXACTLY ONCE through the closed `SpecPayload`, validates its MasterFormat section number against `specification/classify#CLASSIFY` and its article titles against the owned roster under an accumulating disposition, lowers the validated section INTO a `document/model#MODEL` `DocumentNode` tree, and contributes the one new `core/receipt#RECEIPT` `ArtifactReceipt.Spec` case — so a specification section is one schedulable `core/plan#PLAN` `ArtifactWork` producer beside every other artifact, its typeset emission owned downstream by `document/emit#DOCUMENT` folding FROM the tree. This owner authors the section semantics and the numbered node tree; it renders no page and re-authors no IFC — the QTO facts a specification cites arrive from `csharp:Rasm.Bim` through `visualization/table#TABLE`.

Owned closed families carry the SectionFormat vocabularies, authored to the CSI MP-2-2 published cardinality — `SectionPart` the three-part spine, `_ARTICLES` the primary article roster per part in canonical order (its order IS load-bearing for the `_audited` canonical-order check), `_SUBORDINATE` the paragraph-title checklist each article carries, `_MAIN_WORK` the alternative-main-work set an `EXECUTION` part selects exactly one of, `SpecMethod` the four methods, `SubmittalClass` the three submittal regimes, `ParagraphRole` the `CONTENT`/`NOTE` editorial disposition. A `NOTE` specifier-guidance paragraph lowers to a decorative `document/model#MODEL` `BlockKind.ARTIFACT` the issued manual strips and the PDF/UA tag tree excludes, so ONE lowered tree serves both the editing and issued views. Article titles validate at admission so an unknown heading surfaces as a typed `SpecFault`, and the article order, the mandatory main-work, the method/submittal histograms, the specifier-note and unresolved-fill-in tallies, the `_SUBORDINATE` off-checklist reconciliation, and the `REFERENCES`-vs-body citation reconciliation fold once into the `SpecVerdict`. A co-located `PageFormat` carries the `NumberScheme` policy projecting one ordinal PATH to its label through the owned level-style correspondence — the CSI alphanumeric eight-level hierarchy or the UFGS/SpecsIntact cumulative-decimal variant, selected by ONE `NumberMode` rather than two parallel numbering owners. `section` IS a `ClassCode` (`classify#CLASSIFY`), the tree the `document/model#MODEL` owner's, the one receipt case this owner's; it composes those owners and never their interiors.

## [01]-[INDEX]

- [02]-[PAGE]: `PageFormat` — the CSI PageFormat numbering and page-presentation substrate, `label` projecting one ordinal path to its designation across both numbering modes.
- [03]-[SECTION]: `Spec` — the CSI SectionFormat producer over the owned article vocabularies, lowering a validated section into the `document/model#MODEL` tree and minting the one `ArtifactReceipt.Spec` case.

## [02]-[PAGE]

- Owner: `PageFormat` — the CSI PageFormat page-presentation value object carrying the `NumberScheme` policy, the `size`/`font` typography, `page_size`, and the page-identity projections, read by `to_document` and never a stringly format re-derived per level. `NumberScheme` is the closed numbering policy over `NumberMode` (`ALPHANUMERIC` the CSI multi-level scheme, `DECIMAL` the UFGS cumulative) plus the `article_pad` width, its `label(part, path)` projecting one ordinal PATH to its designation through one total `match` on the mode.
- Cases: `NumberMode` the two regimes; `NumberLevel` the closed eight-level hierarchy (`ARTICLE`=0 the part-prefixed head, `PARAGRAPH`..`DETAIL`=1..7); `Alphabet`/`Decoration` the two axes of a level's glyph style; `PageSize`. Each a closed `StrEnum`/`IntEnum`, never a magic-number level or a stringly format code.
- Entry: `NumberScheme.label(part, path)` normalizes on the path shape — an empty path yields `PART {part}`, a length-1 path the part-prefixed article number, a deeper path the local level glyph (`ALPHANUMERIC` reads `_LEVEL_STYLE[NumberLevel(len(path)-1)]`, `DECIMAL` joins the whole path) — so one entrypoint owns every level and mode, the discriminant the path length and mode, never a `paragraph_label`/`subparagraph_label` method family.
- Auto: `label` derives every designation from the ONE `_LEVEL_STYLE` correspondence — `_glyph` projecting the ordinal through its alphabet and `_wrap` applying the level's punctuation, both closed by `assert_never`. `page_footer(section, page)` projects the CSI `{section} - {page}` footer, and `end_of_section` the marker CSI PageFormat mandates AFTER the `SCHEDULES` article.
- Receipt: none — `PageFormat` is pure presentation policy; the numbering travels INTO the `Spec` producer's tree and `ArtifactReceipt.Spec` facts, exactly as `classify#CLASSIFY` contributes none.
- Packages: `msgspec` (`Struct(frozen=True)` the value objects, hashable so a shared `PageFormat` keys deterministically); `frozendict` (`_LEVEL_STYLE`); `enum` (the closed numbering vocabularies). No runtime import.
- Growth: a new numbering regime is one `NumberMode` member plus one `label` arm; a new nesting level one `NumberLevel` member plus one `_LEVEL_STYLE` row; a new glyph alphabet one `Alphabet` member plus one `_glyph` arm; a new punctuation one `Decoration` member plus one `_wrap` arm; a new page-identity fact one `PageFormat` field.
- Boundary: this owner authors presentation policy, never bytes — no second UFGS-decimal numbering owner (the one `NumberMode` policy carries both), no receipt case.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import IntEnum, StrEnum
from typing import Final, assert_never

from msgspec import Struct

# --- [TYPES] ----------------------------------------------------------------------------


class NumberMode(StrEnum):
    ALPHANUMERIC = "alphanumeric"  # CSI PageFormat multi-level: `1.01` -> `A.` -> `1.` -> ... -> `(a)`
    DECIMAL = "decimal"  # UFGS/SpecsIntact cumulative `1.1.1.1.1`


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

# the ONE primary correspondence: subordination level -> (glyph alphabet, punctuation). `ARTICLE` is absent
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


class NumberScheme(Struct, frozen=True):
    # `CSI_PAGEFORMAT`/`UFGS_DECIMAL` are the two named policy instances a caller selects.
    mode: NumberMode = NumberMode.ALPHANUMERIC
    article_pad: int = 2  # `1.01` two-digit (MasterSpec/PageFormat) vs `1.6` single-digit

    def label(self, part: int, path: tuple[int, ...], /) -> str:
        # `NumberLevel(len(path) - 1)` is in range because admission caps paragraph nesting at `_MAX_LEVEL`.
        if not path:
            return f"PART {part}"
        match self.mode:
            case NumberMode.DECIMAL:
                return f"{part}." + ".".join(str(ordinal) for ordinal in path)
            case NumberMode.ALPHANUMERIC:
                if len(path) == 1:
                    return f"{part}.{path[0]:0{self.article_pad}d}"
                alphabet, decoration = _LEVEL_STYLE[NumberLevel(len(path) - 1)]
                return _wrap(decoration, _glyph(alphabet, path[-1]))
            case _ as unreachable:
                assert_never(unreachable)


class PageFormat(Struct, frozen=True):
    numbering: NumberScheme = NumberScheme()
    size: float = 10.0  # ISO 3098 / CSI body-text height in points the emitted section sets
    font: str = "body"
    page_size: PageSize = PageSize.LETTER

    def label(self, part: int, path: tuple[int, ...], /) -> str:
        return self.numbering.label(part, path)

    def page_footer(self, section: str, page: int, /) -> str:
        return f"{section} - {page}"  # the CSI PageFormat section-number-page footer

    def end_of_section(self) -> str:
        return "END OF SECTION"  # CSI PageFormat places this AFTER (never before) the `SCHEDULES` article

    def uppercase(self, level: NumberLevel, /) -> bool:
        return level is NumberLevel.ARTICLE  # CSI: PART + article titles UPPERCASE, subordinate titles Title Case


def _spec_raise(fault: object) -> bytes:
    raise ValueError(str(fault))


# --- [CONSTANTS] ------------------------------------------------------------------------

CSI_PAGEFORMAT: Final[NumberScheme] = NumberScheme(mode=NumberMode.ALPHANUMERIC, article_pad=2)
UFGS_DECIMAL: Final[NumberScheme] = NumberScheme(mode=NumberMode.DECIMAL, article_pad=2)
```

## [03]-[SECTION]

- Owner: `Spec` — the CSI SectionFormat producer over a `section: ClassCode` (the MasterFormat number IS the section identity, admitted through `classify#CLASSIFY`), a `title`, a `tuple[Article, ...]` in canonical order, and its `PageFormat` — lowering the validated section into a `document/model#MODEL` `DocumentNode` tree and contributing one `ArtifactReceipt.Spec` case, never a stringly heading tree or a parallel per-part builder. `Article` binds one `SectionPart`, one validated `_ARTICLES` title, and its paragraphs; `Paragraph` is the recursive content node carrying its subordinate heading, text, `ParagraphRole` disposition, optional `SpecMethod`/`SubmittalClass`, `references`, and `children`. No specification library is admitted — the SectionFormat algebra is this owner's composition over the classify, document, and numbering owners.
- Cases: the owned SectionFormat vocabularies are closed families authored to the CSI MP-2-2 published cardinality — `SectionPart` the three-part spine, `_ARTICLES` the primary article roster per part in published order (the order IS load-bearing for the `_audited` canonical-order check), `_SUBORDINATE` the paragraph-title checklist each article carries, `_MAIN_WORK` the alternative-main-work set an `EXECUTION` part selects exactly one of. `SpecMethod` the four methods of specifying, `SubmittalClass` the three submittal regimes, `ParagraphRole` the `CONTENT`/`NOTE` editorial disposition. `SpecFault` the accumulating fault vocabulary with its `combined` monoid, and `SpecVerdict` the QA evidence — the tallies, the note/fill-in/off-checklist counts, the reference and distinct-standard totals, method + submittal histograms, the canonical-order flag, and the accumulated coverage-fault tags.
- Entry: `Spec.admit(page, /, **payload)` is the one boundary ingress — the `SpecPayload` shape gate through `_PAYLOAD`, the `ClassCode.parse` section-number seam, then the accumulating article fold so a whole malformed section reports every casualty at once rather than aborting first. `to_document()` is the lowering entrypoint, `emit()` the schedulable `ArtifactWork`, `contribute()` the receipt, `audit()` the verdict — one polymorphic producer, never a per-part or per-target emit family.
- Auto: `to_document` lowers the section in one pass — the `Spec` a level-1 `SectionNode`, each present part a level-2, each article a `PageFormat`-numbered level-3, each paragraph a `BlockNode` recursing its sub-tree with the ordinal PATH threaded down so the numbering is the tree's own structure. A `NOTE` paragraph lowers to an unnumbered `BlockKind.ARTIFACT` block, so the CONTENT ordinal `_article_node` threads is a running `accumulate` count that never advances over a note — the retained paragraphs number contiguously with no gap where a stripped note sat, the one place a naive producer leaks a numbering hole. `_audited` folds the verdict in one pass — the tallies, the note/fill-in counts, the method/submittal histograms and reference count over the `_flat` walk, the `_cited` REFERENCES-vs-body citation reconciliation, the `_SUBORDINATE` off-checklist reconciliation, the canonical-order check, and the mandatory main-work check — rather than a per-check re-walk.
- Receipt: `Spec` contributes the one new `core/receipt#RECEIPT` `ArtifactReceipt.Spec` case — the section number, the `division()` head the `classify#CLASSIFY` crosswalk keys on, the present-part and article counts, and the encoded-tree byte count — read off the same encode `contribute` computes ONCE, deriving both the content key and the byte count from that single fact, never a second encode to mint the receipt.
- Packages: `msgspec` (the `Struct(frozen=True)` value objects and the deterministic node encode); `expression` (`tagged_union` the `SpecFault`; `Block` the accumulating admission fold; the `Result`/`Option` rails); `pydantic` (`TypeAdapter` the `_PAYLOAD` gate over the recursive `SpecPayload`, `ValidationError` mapped to `<invalid-payload>`); `frozendict` (the owned vocabulary correspondences and verdict histograms); `re` (`_FILL_IN` the unresolved-fill-in `Pattern`); `itertools.accumulate` (the CONTENT-only ordinal); `collections.Counter` (the histograms); runtime (`ContentIdentity`/`ContentKey`, `RuntimeRail`/`async_boundary`, `LanePolicy`/`Modality`/`RetryClass` the off-loop encode); `classify#CLASSIFY` (`ClassCode`), `document/model#MODEL` (the `SectionNode`/`BlockNode` tree and `encode`), and `core/receipt#RECEIPT` (`ArtifactReceipt.Spec`), composed never re-authored. No new external library.
- Growth: a new article is one `_ARTICLES` row; a new subordinate title one `_SUBORDINATE` row; a new main-work title one `_MAIN_WORK` member; a new method/submittal/disposition one `SpecMethod`/`SubmittalClass`/`ParagraphRole` member (a `NOTE` already routes to `BlockKind.ARTIFACT`); a new QA rule one `SpecVerdict` field and one `_audited` detection; a new fault cause one `SpecFault` case plus one `combined` member; a new section-number system is already carried by `ClassCode`.
- Boundary: this owner authors the specification, never the rendered page — `document/emit#DOCUMENT` folds the tree to PDF/Typst/HTML, `PageFormat` owns the numbering, `classify#CLASSIFY` owns `ClassCode`, and `csharp:Rasm.Bim` owns the QTO/IFC a specification cites through `visualization/table#TABLE`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import re
from collections import Counter
from collections.abc import Iterable, Iterator
from enum import StrEnum
from itertools import accumulate
from typing import TYPE_CHECKING, Final, Literal, NotRequired, ReadOnly, Required, Self, TypedDict, Unpack

from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import UNSET, Struct
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt
from artifacts.document.model import BlockKind, BlockNode, DocumentNode, NodeMeta, RunNode, SectionNode, encode
from artifacts.specification.classify import ClassCode, ClassSystem

# `NumberLevel`/`PageFormat` are the co-located `[02]-[PAGE]` owners above in this module — in scope directly.

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
    NOTE = "note"  # specifier note (`SPEC NOTE:`), stripped at issue -> `BlockKind.ARTIFACT`


# --- [CONSTANTS] ------------------------------------------------------------------------

_MAX_DEPTH: Final[int] = NumberLevel.DETAIL.value  # 7 — deepest CSI PageFormat sub-article level; nesting past it faults
# the unresolved-fill-in vocabulary: a bracketed blank `[____]` or an insert prompt `<Insert value>` the
# specifier never resolved — a section issued carrying one is editorially incomplete.
_FILL_IN: Final[re.Pattern[str]] = re.compile(r"\[_{2,}\]|<[^<>]+>")
_PART_NUMBER: Final[Map[SectionPart, int]] = Map.of_seq([(SectionPart.GENERAL, 1), (SectionPart.PRODUCTS, 2), (SectionPart.EXECUTION, 3)])

# --- [TABLES] ---------------------------------------------------------------------------

# the CSI MP-2-2 primary article roster per part, in published order (Figure SF-1) — order IS load-bearing:
# the `_audited` canonical-order check reads each title's index. Titles are UPPERCASE, the validated vocabulary.
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
# the alternative main-work titles an EXECUTION part selects EXACTLY ONE of; carrying none is the
# `missing_main_work` coverage fault.
_MAIN_WORK: Final[frozenset[str]] = frozenset({"ERECTION", "INSTALLATION", "APPLICATION", "CONSTRUCTION"})
# the subordinate paragraph-title checklist each primary article carries (Figure SF-1), keyed by article title.
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
    ("SUBMITTALS", ("Product Data", "Shop Drawings", "Samples", "Quality Assurance/Control Submittals", "Closeout Submittals")),
    ("QUALITY ASSURANCE", ("Qualifications", "Regulatory Requirements", "Certifications", "Field Samples", "Mock-ups", "Pre-installation Meetings")),
    ("DELIVERY, STORAGE, AND HANDLING", (
        "Packing, Shipping, Handling, and Unloading",
        "Acceptance at Site",
        "Storage and Protection",
        "Waste Management and Disposal",
    )),
    ("PROJECT/SITE CONDITIONS", ("Project/Site Environmental Requirements", "Existing Conditions")),
    ("WARRANTY", ("Special Warranty",)),
    ("MAINTENANCE", ("Extra Materials", "Maintenance Service")),
    ("FABRICATION", ("Shop Assembly", "Fabrication Tolerances")),
    ("FINISHES", ("Shop Priming, Shop Finishing",)),
    ("SOURCE QUALITY CONTROL", ("Tests, Inspection", "Verification of Performance")),
    ("EXAMINATION", ("Site Verification of Conditions",)),
    ("PREPARATION", ("Protection", "Surface Preparation")),
    ("CONSTRUCTION", ("Special Techniques", "Interface with Other Work", "Sequences of Operation", "Site Tolerances")),
    ("FIELD QUALITY CONTROL", ("Site Tests, Inspection", "Manufacturers' Field Services")),
])
_PART_VALUES: Final[frozenset[str]] = frozenset(part.value for part in SectionPart)
_METHOD_VALUES: Final[frozenset[str]] = frozenset(method.value for method in SpecMethod)
_SUBMITTAL_VALUES: Final[frozenset[str]] = frozenset(kind.value for kind in SubmittalClass)
_ROLE_VALUES: Final[frozenset[str]] = frozenset(role.value for role in ParagraphRole)
_REFERENCES: Final[str] = _ARTICLES[SectionPart.GENERAL][1]  # the `REFERENCES` article title the citation reconciliation lists against

# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class SpecFault:
    # the closed admission-fault vocabulary carrying its offending token; `combined` folds every casualty so a
    # whole section reports each bad article rather than aborting first.
    tag: Literal["bad_section", "unknown_article", "bad_method", "bad_submittal", "bad_role", "depth_overflow", "invalid_payload", "aggregate"] = (
        tag()
    )
    bad_section: str = case()  # the MasterFormat number failed `ClassCode.parse`
    unknown_article: str = case()  # an article title or part outside the SectionFormat roster
    bad_method: str = case()  # a paragraph method outside the four SpecMethod cases
    bad_submittal: str = case()  # a submittal class outside the three SubmittalClass cases
    bad_role: str = case()  # a paragraph role outside the two ParagraphRole cases
    depth_overflow: int = case()  # a paragraph nested past the deepest CSI PageFormat level
    invalid_payload: str = case()  # the payload failed the TypeAdapter shape gate
    aggregate: tuple["SpecFault", ...] = case()

    @staticmethod
    def _members(fault: "SpecFault", /) -> tuple["SpecFault", ...]:
        return fault.aggregate if fault.tag == "aggregate" else (fault,)

    @staticmethod
    def combined(left: "SpecFault", right: "SpecFault", /) -> "SpecFault":
        return SpecFault(aggregate=(*SpecFault._members(left), *SpecFault._members(right)))


# --- [MODELS] ---------------------------------------------------------------------------


class Paragraph(Struct, frozen=True):
    # the recursive specification content node; a `NOTE` role lowers to a decorative `BlockKind.ARTIFACT`,
    # numbered by depth in `to_document`.
    text: str
    title: str = ""  # the CSI SF-1 subordinate paragraph heading
    role: ParagraphRole = ParagraphRole.CONTENT
    method: SpecMethod | None = None
    submittal: SubmittalClass | None = None
    references: tuple[str, ...] = ()  # cited published standards, e.g. `ASTM C150`
    children: tuple["Paragraph", ...] = ()


class Article(Struct, frozen=True):
    part: SectionPart
    title: str  # one primary title from `_ARTICLES[part]`
    paragraphs: tuple[Paragraph, ...] = ()


class SpecVerdict(Struct, frozen=True, gc=False):
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
    coverage: tuple[str, ...]  # accumulated coverage-fault tags

    def facts(self) -> dict[str, object]:
        return {
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
            "coverage": ",".join(self.coverage),
        }


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


# --- [CONSTANTS] ------------------------------------------------------------------------

_PAYLOAD: Final = TypeAdapter(SpecPayload)
_FAULTS: Final[tuple[type[BaseException], ...]] = (RuntimeError, ValueError, KeyError, OSError)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _flat(paragraphs: tuple[Paragraph, ...], /) -> Iterator[Paragraph]:
    for paragraph in paragraphs:
        yield paragraph
        yield from _flat(paragraph.children)  # bounded by `_MAX_DEPTH`, so native recursion is safe


def _max_depth(paragraphs: tuple[Paragraph, ...], depth: int, /) -> int:
    return max((_max_depth(child.children, depth + 1) for child in paragraphs), default=depth) if paragraphs else depth - 1


def _fill_ins(text: str, /) -> int:
    return len(_FILL_IN.findall(text))  # the unresolved `[____]`/`<Insert>` blanks the editorial-completeness check counts


def _citations(articles: tuple[Article, ...], /) -> tuple[frozenset[str], frozenset[str]]:
    # partition every body citation by whether its article IS the REFERENCES roster; `cited - listed` is the
    # unlisted-reference gap.
    tagged = tuple(
        (article.title == _REFERENCES, reference)
        for article in articles
        for paragraph in _flat(article.paragraphs)
        for reference in paragraph.references
    )
    return frozenset(ref for at_references, ref in tagged if at_references), frozenset(ref for at_references, ref in tagged if not at_references)


def _ordered(part: SectionPart, titles: tuple[str, ...], /) -> bool:
    # every present article appears in canonical `_ARTICLES[part]` order — a monotonic index sequence.
    positions = tuple(_ARTICLES[part].index(title) for title in titles if title in _ARTICLES[part])
    return all(earlier <= later for earlier, later in zip(positions, positions[1:], strict=False))


def _accumulated[T](results: Block[Result[T, SpecFault]], /) -> Result[tuple[T, ...], SpecFault]:
    # combine every fault through the monoid, return the whole set only when the casualty set is empty — the
    # `Validation`-style fold `register#REGISTER` shares.
    faults = results.choose(lambda outcome: outcome.swap().to_option())
    return Ok(tuple(results.choose(lambda outcome: outcome.to_option()))) if faults.is_empty() else Error(faults.reduce(SpecFault.combined))


def _admit_paragraph(payload: ParagraphPayload, depth: int, /) -> Result[Paragraph, SpecFault]:
    if depth > _MAX_DEPTH:
        return Error(SpecFault(depth_overflow=depth))
    role = payload.get("role", ParagraphRole.CONTENT.value)
    if role not in _ROLE_VALUES:
        return Error(SpecFault(bad_role=role))
    method = payload.get("method", "")
    if method and method not in _METHOD_VALUES:
        return Error(SpecFault(bad_method=method))
    submittal = payload.get("submittal", "")
    if submittal and submittal not in _SUBMITTAL_VALUES:
        return Error(SpecFault(bad_submittal=submittal))
    children = _accumulated(Block.of_seq(_admit_paragraph(child, depth + 1) for child in payload.get("children", ())))
    return children.map(
        lambda kids: Paragraph(
            text=payload["text"],
            title=payload.get("title", ""),
            role=ParagraphRole(role),
            method=SpecMethod(method) if method else None,
            submittal=SubmittalClass(submittal) if submittal else None,
            references=payload.get("references", ()),
            children=kids,
        )
    )


def _admit_article(payload: ArticlePayload, /) -> Result[Article, SpecFault]:
    if payload["part"] not in _PART_VALUES:
        return Error(SpecFault(unknown_article=payload["part"]))
    part = SectionPart(payload["part"])
    if payload["title"] not in _ARTICLES[part]:
        return Error(SpecFault(unknown_article=payload["title"]))
    paragraphs = _accumulated(Block.of_seq(_admit_paragraph(entry, 1) for entry in payload.get("paragraphs", ())))
    return paragraphs.map(lambda paras: Article(part=part, title=payload["title"], paragraphs=paras))


def _audited(spec: "Spec", /) -> SpecVerdict:
    # one accumulating pass over the tallies, editorial counts, histograms, canonical-order and main-work checks,
    # and the REFERENCES-article citation reconciliation.
    articles = spec.articles
    walked = tuple(paragraph for article in articles for paragraph in _flat(article.paragraphs))
    methods = Counter(paragraph.method for paragraph in walked if paragraph.method is not None)
    submittals = Counter(paragraph.submittal for paragraph in walked if paragraph.submittal is not None)
    notes = sum(1 for paragraph in walked if paragraph.role is ParagraphRole.NOTE)
    fill_ins = sum(_fill_ins(paragraph.text) for paragraph in walked if paragraph.role is ParagraphRole.CONTENT)
    # a first-level CONTENT heading off its article's `_SUBORDINATE` checklist is an off-checklist title the QA
    # pass surfaces, never an admission fault.
    off_checklist = sum(
        1
        for article in articles
        if article.title in _SUBORDINATE
        for paragraph in article.paragraphs
        if paragraph.role is ParagraphRole.CONTENT and paragraph.title and paragraph.title not in _SUBORDINATE[article.title]
    )
    listed, cited = _citations(articles)
    present = {article.part for article in articles}
    ordered = all(_ordered(part, tuple(article.title for article in articles if article.part is part)) for part in present)
    execution = frozenset(article.title for article in articles if article.part is SectionPart.EXECUTION)
    has_main = SectionPart.EXECUTION not in present or bool(execution & _MAIN_WORK)
    coverage = tuple(
        tag
        for present_fault, tag in (
            (not articles, "empty_section"),
            (not ordered, "out_of_order"),
            (not has_main, "missing_main_work"),
            (bool(fill_ins), "unresolved_fill_ins"),
            (bool(cited - listed), "unlisted_references"),
            (bool(off_checklist), "off_checklist_titles"),
        )
        if present_fault
    )
    return SpecVerdict(
        parts_present=len(present),
        articles=len(articles),
        paragraphs=len(walked),
        notes=notes,
        fill_ins=fill_ins,
        off_checklist=off_checklist,
        max_depth=max((_max_depth(article.paragraphs, 1) for article in articles), default=0),
        references=sum(len(paragraph.references) for paragraph in walked),
        standards=len(listed | cited),
        methods=frozendict({method: methods.get(method, 0) for method in SpecMethod}),
        submittals=frozendict({kind: submittals.get(kind, 0) for kind in SubmittalClass}),
        ordered=ordered,
        coverage=coverage,
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


class Spec(Struct, frozen=True):
    section: ClassCode
    title: str
    articles: tuple[Article, ...] = ()
    page: PageFormat = PageFormat()

    @classmethod
    def admit(cls, page: PageFormat = PageFormat(), /, **payload: Unpack[SpecPayload]) -> Result[Self, SpecFault]:
        # the shape gate through `_PAYLOAD`, the `ClassCode` section-number seam, then the accumulating article
        # fold so a whole client section reports each malformed article at once.
        try:
            valid = _PAYLOAD.validate_python(payload)
        except ValidationError as fault:
            fault.add_note(f"<spec.payload:{[error['loc'] for error in fault.errors()]}>")
            return Error(SpecFault(invalid_payload=str(payload.get("section", "<unknown>"))))
        parsed = ClassCode.parse(ClassSystem.MASTERFORMAT, valid["section"])
        if parsed.is_error():
            return Error(SpecFault(bad_section=valid["section"]))
        admitted = _accumulated(Block.of_seq(_admit_article(entry) for entry in valid["articles"]))
        return admitted.map(lambda arts: cls(section=parsed.ok, title=valid["title"], articles=arts, page=page))

    def to_document(self) -> DocumentNode:
        # the ordinal PATH is the numbering; `document/emit#DOCUMENT` folds FROM this tree.
        parts = tuple(self._part_node(part) for part in SectionPart if any(article.part is part for article in self.articles))
        heading = (self._run(f"{self.section.render()}  {self.title.upper()}", weight=700),)
        return SectionNode(
            meta=self._meta("Sect", self.section.render(), classification=self.section.render()), level=1, heading=heading, children=parts
        )

    def audit(self) -> SpecVerdict:
        return _audited(self)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: the section identity minted PRE-RUN, never a key over the encoded bytes.
        return ContentIdentity.of(f"spec-{self.section.render()}", self.section, policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the receipt threads the PRE-RUN key.
        verdict = self.audit()
        return (await async_boundary(f"spec.{self.section.render()}", self._encoded_rail, catch=_FAULTS)).map(
            lambda payload: ArtifactReceipt.Spec(
                self._key, self.section.render(), self.section.division(), verdict.parts_present, verdict.articles, len(payload)
            )
        )

    async def _encoded_rail(self) -> bytes:
        # the offloaded encode returns the PAYLOAD BYTES the receipt counts; identity minting stays `_key`'s.
        crossed = await LanePolicy.offload(self._encoded, modality=Modality.THREAD, retry=RetryClass.OCCT)
        return crossed.default_with(_spec_raise)

    def _encoded(self) -> bytes:
        return encode(self.to_document())

    def contribute(self) -> "Iterable[Receipt]":
        payload = self._encoded()  # the one encode the key and byte count both derive from
        key = ContentIdentity.of(f"spec-{self.section.render()}", payload)
        verdict = self.audit()
        receipt = ArtifactReceipt.Spec(key, self.section.render(), self.section.division(), verdict.parts_present, verdict.articles, len(payload))
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
        numbers = tuple(accumulate(int(paragraph.role is ParagraphRole.CONTENT) for paragraph in article.paragraphs))
        paragraphs = tuple(
            self._paragraph_node(paragraph, part, (ordinal, number)) for paragraph, number in zip(article.paragraphs, numbers, strict=True)
        )
        heading = (self._run(f"{label}  {article.title}", weight=700),)
        return SectionNode(meta=self._meta("H2", label), level=3, heading=heading, children=paragraphs)

    def _paragraph_node(self, paragraph: Paragraph, part: int, path: tuple[int, ...], /) -> DocumentNode:
        children = tuple(self._paragraph_node(child, part, (*path, number)) for number, child in enumerate(paragraph.children, start=1))
        if (
            paragraph.role is ParagraphRole.NOTE
        ):  # decorative editing guidance — unnumbered, `BlockKind.ARTIFACT` excludes it from the PDF/UA tag tree
            return BlockNode(
                meta=self._meta("Note", paragraph.text[:32]), block=BlockKind.ARTIFACT, runs=(self._run(paragraph.text),), children=children
            )
        label = self.page.label(part, path)
        # a subordinate heading rides its own bold lead run before the body run; a bare paragraph is one run.
        runs = (
            (self._run(f"{label}  {paragraph.title}", weight=700), self._run(paragraph.text))
            if paragraph.title
            else (self._run(f"{label}  {paragraph.text}"),)
        )
        return BlockNode(meta=self._meta("P", label), block=BlockKind.PARAGRAPH, runs=runs, children=children)

    def _run(self, text: str, /, *, weight: int = 400) -> RunNode:
        return RunNode(meta=self._meta("Span", text[:32]), text=text, font_key=self.page.font, size=self.page.size, weight=weight)

    def _meta(self, role: str, token: str, /, *, classification: str = "") -> NodeMeta:
        # the section's CSI ClassCode rides the root SectionNode's NodeMeta.classification so the one lowered
        # model tree carries the code the classify#CLASSIFY ReferenceIndex keys the drawing<->spec cross-ref on.
        return NodeMeta(
            key=ContentIdentity.of(f"spec-{role}", f"{self.section.render()}:{token}".encode()),
            role=role,
            page=0,
            classification=classification or UNSET,
        )
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
