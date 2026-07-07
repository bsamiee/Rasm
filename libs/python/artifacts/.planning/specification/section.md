# [PY_ARTIFACTS_SECTION]

The construction-specification production owner: the one authority over the CSI/CSC SectionFormat three-part specification section (`PART 1 GENERAL` / `PART 2 PRODUCTS` / `PART 3 EXECUTION`), its published article vocabulary, the four methods of specifying, and the PageFormat multi-level numbering the section presents through. `Spec` is ONE producer that admits a raw specification payload EXACTLY ONCE through a closed `SpecPayload` `TypedDict`, validates its MasterFormat section number against `specification/classify#CLASSIFY` and its article titles against the owned SectionFormat roster under an accumulating disposition, lowers the validated section INTO a `document/model#MODEL` `DocumentNode` tree (never a parallel document type), and contributes the one new `core/receipt#RECEIPT` `ArtifactReceipt.Spec` case — so a specification section is one schedulable `core/plan#PLAN` `ArtifactWork` producer beside every drawing, chart, and register artifact, its typeset PDF/Typst/HTML emission owned downstream by `document/emit#DOCUMENT` folding FROM the tree. This owner authors the SECTION SEMANTICS and the numbered node tree; it renders no page, re-implements no `DocumentNode` backend, and re-authors no IFC — the QTO facts a specification cites arrive from `csharp:Rasm.Bim` through `visualization/table#TABLE`, never re-computed here.

The SectionFormat vocabularies are OWNED closed families authored to the CSI MP-2-2 published cardinality, never a stringly heading the body re-parses. `SectionPart` is the three-part spine; `_ARTICLES` is the primary article roster per part carried as ONE `frozendict` correspondence in canonical published order (the fifteen `GENERAL` articles, the eleven `PRODUCTS` articles, the fifteen `EXECUTION` articles), `_SUBORDINATE` the checklist of subordinate paragraph titles each primary article offers, and `_MAIN_WORK` the `ERECTION`/`INSTALLATION`/`APPLICATION`/`CONSTRUCTION` alternative-main-work set a section selects exactly one of. `SpecMethod` is the closed four-method specifying vocabulary (`DESCRIPTIVE`/`PERFORMANCE`/`REFERENCE_STANDARD`/`PROPRIETARY`) every paragraph may declare, `SubmittalClass` the `ACTION`/`INFORMATIONAL`/`CLOSEOUT` SubmittalFormat regime, and `ParagraphRole` the editorial disposition each paragraph carries at edit time — a `CONTENT` paragraph is retained and numbered, a `NOTE` specifier-guidance paragraph (the `SPEC NOTE:` a master spec hides) lowers to a decorative `document/model#MODEL` `BlockKind.ARTIFACT` the issued manual strips and the PDF/UA tag tree excludes, so ONE lowered tree serves both the editing view and the issued view without a second projection. Article titles validate against `_ARTICLES` at admission so an unknown or misplaced heading surfaces as a typed `SpecFault` rather than a silently malformed section, and the article ORDER, the mandatory main-work article, the specifying-method/submittal histograms, the specifier-note tally, the unresolved `[____]`/`<Insert>` fill-in count a section may not be issued carrying, the subordinate-heading off-checklist reconciliation (a first-level paragraph heading absent from the article's `_SUBORDINATE` SF-1 checklist), and the `REFERENCES`-article citation reconciliation (a standard the body cites but the manual's own `REFERENCES` roster never lists) fold once into the `SpecVerdict` a project manual owes its quality review.

The CSI PageFormat numbering is the presentation half no SectionFormat article spells: `PageFormat` carries the `NumberScheme` policy that projects one ordinal PATH to its label through the OWNED level-style correspondence — the CSI alphanumeric `1.01` → `A.` → `1.` → `a.` → `1)` → `a)` → `(1)` → `(a)` eight-level hierarchy, or the UFGS/SpecsIntact cumulative-decimal `1.1.1.1.1` variant, selected by ONE `NumberMode` policy value rather than two parallel numbering owners. Every operation is content-keyed through `ContentIdentity.of` over the encoded tree, offloaded off the event loop through a `CapacityLimiter`-bounded the runtime thread lane so the `msgspec` node encode never blocks the loop, and railed as `RuntimeRail[ArtifactReceipt] = Result[ContentKey, BoundaryFault]` minted once at `async_boundary`. This owner composes `specification/classify#CLASSIFY` (the section number IS a `ClassCode`), `document/model#MODEL` (the tree it lowers into), and `core/receipt#RECEIPT` (the one case it mints); it owns the specification algebra and never the sibling owners' interiors.

## [01]-[INDEX]

- [02]-[PAGE]: `PageFormat` — the CSI PageFormat page-presentation owner over the `NumberScheme` numbering policy (`NumberMode` alphanumeric-vs-decimal + `article_pad`), the closed `NumberLevel` eight-level hierarchy, and the `_LEVEL_STYLE` primary `(Alphabet, Decoration)` correspondence from which `label(part, path)` derives every article/paragraph/subparagraph designation through one total `match` — the CSI `1.01`/`A.`/`1.`/`a.`/`1)`/`a)`/`(1)`/`(a)` scheme or the UFGS cumulative `1.1.1.1.1` variant — plus the typography (`size`/`font` ISO 3098 body height), the `page_footer(section, page)` section-number-page projection, and the `end_of_section()` marker CSI PageFormat places AFTER schedules; the pure presentation substrate `Spec` composes, minting no receipt exactly as `classify#CLASSIFY` mints none.
- [03]-[SECTION]: `Spec` — the CSI SectionFormat producer over the owned article vocabularies (`SectionPart`, `_ARTICLES` the published roster per part, `_SUBORDINATE` the paragraph checklist, `_MAIN_WORK` the alternative-main-work set, `SpecMethod` the four methods, `SubmittalClass` the three submittal regimes, `ParagraphRole` the content-vs-note editorial disposition), the `Article`/`Paragraph` recursive content tree, the `SpecPayload` `TypedDict` admitted once through the `_PAYLOAD` `TypeAdapter` and the `ClassCode` section-number seam under the accumulating disposition, the `SpecFault` accumulating fault vocabulary with its `combined` monoid, the `SpecVerdict` article-order/main-work/method-histogram/note/fill-in/reference-reconciliation QA fold, the `to_document` lowering into the `document/model#MODEL` `SectionNode`/`BlockNode` tree numbered through `PageFormat` (a `NOTE` paragraph lowered to the decorative `BlockKind.ARTIFACT`), the `RuntimeRail[ArtifactReceipt]` rail over the bounded off-loop encode, and the one new `core/receipt#RECEIPT` `ArtifactReceipt.Spec` contribution.

## [02]-[PAGE]

- Owner: `PageFormat` the one CSI PageFormat page-presentation value object — a `NumberScheme` numbering policy, the `size`/`font` typography (the ISO 3098 body-text height the emitted section sets), the `page_size`, and the page-identity projections (`page_footer` the section-number-page footer, `end_of_section` the marker) — carried by every `Spec` and read by `to_document`, never a stringly numbering format re-derived per level. `NumberScheme` is the closed numbering policy over `NumberMode` (`ALPHANUMERIC` the CSI PageFormat multi-level scheme, `DECIMAL` the UFGS/SpecsIntact cumulative scheme) plus the `article_pad` width (`1.01` two-digit MasterSpec vs `1.6` single-digit), its `label(part, path)` projecting one ordinal PATH to its designation through one total `match` on the mode. This owner mints NO `core/receipt#RECEIPT` case and NO plan node — it is the pure presentation substrate the `Spec` producer composes, exactly as `classify#CLASSIFY` `ClassCode` and `drawing/standard#DRAWING_STANDARD` `Standard` are pure substrate.
- Cases: `NumberMode` the two numbering regimes; `NumberLevel` the closed eight-level hierarchy (`ARTICLE`=0 the part-prefixed head, then `PARAGRAPH`/`SUBPARAGRAPH`/`CLAUSE`/`SUBCLAUSE`/`ITEM`/`SUBITEM`/`DETAIL`=1..7); `Alphabet` (`ARABIC`/`UPPER`/`LOWER`) and `Decoration` (`DOT` `A.`, `CLOSE_PAREN` `1)`, `BOTH_PARENS` `(a)`) the two axes of a level's glyph style; `PageSize` (`LETTER`/`A4`). Each a closed `StrEnum`/`IntEnum`, never a magic-number level or a stringly format code.
- Entry: `PageFormat()` is direct construction with CSI-alphanumeric defaults; `label` is the projection `to_document` reads. `NumberScheme.label(part, path)` normalizes on the path shape — an empty path yields `PART {part}`, a length-1 path the part-prefixed article number (`ALPHANUMERIC` → `1.01` under `article_pad`, `DECIMAL` → `1.1`), a deeper path the local level glyph (`ALPHANUMERIC` reads `_LEVEL_STYLE[NumberLevel(len(path)-1)]` and wraps `path[-1]`; `DECIMAL` joins the whole path) — so one entrypoint owns every level and mode, the discriminant the path length and the mode value, never a `paragraph_label`/`subparagraph_label` method family.
- Auto: `label` derives every designation from the ONE `_LEVEL_STYLE` primary correspondence — `_glyph(alphabet, ordinal)` projecting the ordinal through its alphabet (`ARABIC` decimal, `UPPER`/`LOWER` bijective base-26 so a 27th sibling is `AA` not a wraparound collision) and `_wrap(decoration, glyph)` applying the level's punctuation, both total `match` closed by `assert_never`. The `DECIMAL` mode reuses the same path but joins it cumulatively (`1.1.1`), so a UFGS section and a CSI section share one `label` entrypoint discriminating on `mode`, never a parallel decimal-numbering owner. `page_footer(section, page)` projects the CSI `{section} - {page}` footer, and `end_of_section` the marker CSI PageFormat mandates AFTER (never before) the `SCHEDULES` article.
- Receipt: none. `PageFormat` is a pure presentation policy and `NumberScheme.label` a pure projection — the numbering it produces travels INTO the `Spec` producer's `to_document` tree and its `ArtifactReceipt.Spec` facts (the article count the numbering designates), never an `ArtifactReceipt` case this owner mints. It is presentation substrate, not a production sub-domain, exactly as `classify#CLASSIFY` contributes no receipt.
- Packages: `msgspec` (`Struct(frozen=True)` the `PageFormat`/`NumberScheme` value objects, hashable so a `Spec` carrying a shared `PageFormat` keys deterministically); `frozendict` (`_LEVEL_STYLE` the one primary `NumberLevel → (Alphabet, Decoration)` correspondence); `enum` (`StrEnum`/`IntEnum` the closed numbering vocabularies). No runtime import — the policy mints no `ContentKey` and runs no boundary.
- Growth: a new numbering regime is one `NumberMode` member plus one `label` `match` arm (the `assert_never` forcing it); a new nesting level is one `NumberLevel` member plus one `_LEVEL_STYLE` row; a new glyph alphabet is one `Alphabet` member plus one `_glyph` arm; a new punctuation is one `Decoration` member plus one `_wrap` arm; a new page identity fact is one `PageFormat` field and one projection; zero new surface — the numbering owner grows by level row and mode arm.
- Boundary: a per-level `paragraph_label`/`subparagraph_label`/`clause_label` method family where one `label` discriminates on the path is the deleted surface spam; a stringly `"1.01"` format re-parsed at every level where `NumberLevel` + `_LEVEL_STYLE` derive it is the deleted stringly form; a hand-kept `_ALPHANUMERIC_LABELS`/`_DECIMAL_LABELS` parallel pair where one `mode` arm derives both is the deleted parallel map; a modulo-26 letter wrap colliding the 27th sibling onto the 1st where bijective base-26 spills to `AA` is the deleted defect; a second UFGS-decimal numbering owner where the one `NumberMode` policy carries it is the deleted fragmentation. This owner authors presentation policy, never bytes.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import IntEnum, StrEnum
from typing import Final, assert_never

from msgspec import Struct

# --- [TYPES] ----------------------------------------------------------------------------


class NumberMode(StrEnum):
    ALPHANUMERIC = "alphanumeric"  # CSI PageFormat: `1.01` -> `A.` -> `1.` -> `a.` -> `1)` -> `a)` -> `(1)` -> `(a)`
    DECIMAL = "decimal"  # UFGS/SpecsIntact: cumulative `1.1.1.1.1` (five subpart levels)


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
# (its part-prefixed `1.01` form is built directly), so the table covers the seven sub-article CSI levels and
# `label` reads one row per level rather than a per-level format method.
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
    # the CSI PageFormat numbering policy — one behavior-carrying value the `label` reads, never a flag set the
    # body re-derives; `CSI_PAGEFORMAT`/`UFGS_DECIMAL` are the two named policy instances a caller selects.
    mode: NumberMode = NumberMode.ALPHANUMERIC
    article_pad: int = 2  # `1.01` two-digit (MasterSpec/PageFormat) vs `1.6` single-digit

    def label(self, part: int, path: tuple[int, ...], /) -> str:
        # one entrypoint over every level and mode: an empty path is the PART head, a length-1 path the
        # part-prefixed article number, a deeper path the local level glyph — the discriminant the path length
        # and the mode, never a per-level method. `NumberLevel(len(path) - 1)` is in range because admission
        # caps paragraph nesting at `_MAX_LEVEL`, so `len(path) <= _MAX_LEVEL + 1`.
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

- Owner: `Spec` the one CSI SectionFormat producer — a `section: ClassCode` (the MasterFormat number IS the section identity, admitted through `classify#CLASSIFY`, never re-parsed), a `title`, a `tuple[Article, ...]` in canonical part-then-article order, and the `PageFormat` it numbers through — lowering the validated section into a `document/model#MODEL` `DocumentNode` tree and contributing one `ArtifactReceipt.Spec` case, never a stringly heading tree or a parallel per-part builder. `Article` binds one `SectionPart`, one validated primary title from `_ARTICLES`, and its `tuple[Paragraph, ...]`; `Paragraph` is the recursive content node carrying its `title` subordinate heading (the CSI SF-1 paragraph heading `_audited` reconciles against `_SUBORDINATE[article.title]`), its `text`, its `ParagraphRole` editorial disposition (`CONTENT` numbered spec text vs a `NOTE` decorative specifier note), its optional `SpecMethod` (the paragraph's method of specifying) and `SubmittalClass`, its `references` reference-standard citations, and its `children` sub-paragraphs. Raw specification content crosses the boundary EXACTLY ONCE: `Spec.admit(**payload)` validates the `SpecPayload` `TypedDict` through the module-level `_PAYLOAD` `TypeAdapter`, parses the section number through `ClassCode.parse`, and validates every article title against `_ARTICLES[part]` and every paragraph role/method/submittal against the closed vocabularies under the accumulating disposition, so a whole malformed section surfaces every bad article, unknown method, and out-of-vocabulary role as one combined `SpecFault` rather than aborting on the first, and the interior is total over admitted `Article` rows. `msgspec` owns the `Struct(frozen=True)` value objects and the deterministic node encode; `classify#CLASSIFY` owns `ClassCode`; `document/model#MODEL` owns the `SectionNode`/`BlockNode`/`RunNode`/`ListNode` tree; `PageFormat` owns the numbering; no specification library is admitted, so the SectionFormat algebra is this owner's composition over those owners, never a re-implemented heading emitter.
- Cases: the owned SectionFormat vocabularies are closed families authored to the CSI MP-2-2 published cardinality. `SectionPart` the three-part spine; `_ARTICLES` the primary article roster per part in published order — the fifteen `GENERAL` articles (`SUMMARY` … `MAINTENANCE`), the eleven `PRODUCTS` articles (`MANUFACTURERS` … `SOURCE QUALITY CONTROL`), the fifteen `EXECUTION` articles (`INSTALLERS` … `SCHEDULES`) — carried as ONE `frozendict` correspondence, never a stringly `.startswith` heading probe; `_SUBORDINATE` the checklist of subordinate paragraph titles each primary article offers (the `SUMMARY` `Section Includes`/`Allowances`/`Unit Prices` set, the `SUBMITTALS` `Product Data`/`Shop Drawings`/`Closeout Submittals` set, and the rest); `_MAIN_WORK` the `ERECTION`/`INSTALLATION`/`APPLICATION`/`CONSTRUCTION` alternative-main-work set an `EXECUTION` part selects exactly one of. `SpecMethod` the four CSI methods of specifying (`DESCRIPTIVE` exact properties, `PERFORMANCE` required results + criteria, `REFERENCE_STANDARD` by published standard, `PROPRIETARY` named product); `SubmittalClass` the `ACTION`/`INFORMATIONAL`/`CLOSEOUT` SubmittalFormat regime; `ParagraphRole` the `CONTENT`/`NOTE` editorial disposition (a `NOTE` is the `SPEC NOTE:` specifier guidance stripped at issue). `SpecFault` the accumulating fault vocabulary (`bad_section`/`unknown_article`/`bad_method`/`bad_submittal`/`bad_role`/`depth_overflow`/`invalid_payload`/`aggregate`) with its `combined` monoid, and `SpecVerdict` the QA evidence (part/article/paragraph counts, the specifier-note, unresolved-fill-in, and off-checklist-heading tallies, max depth, the reference-citation total and distinct-standard count, method + submittal histograms, the canonical-order flag, and the accumulated coverage-fault tags `empty_section`/`out_of_order`/`missing_main_work`/`unresolved_fill_ins`/`unlisted_references`/`off_checklist_titles`).
- Entry: `Spec.admit(page, /, **payload)` is the one boundary ingress — the `SpecPayload` shape gate through `_PAYLOAD`, then the `ClassCode.parse` section-number seam and the accumulating article fold so a batch reports every casualty at once. `to_document()` is the lowering entrypoint (the `SectionNode` tree `document/emit#DOCUMENT` folds FROM), `of()` the content-keyed rail, `contribute()` the receipt, `audit()` the QA verdict — one polymorphic producer, never a per-part or per-target emit family.
- Auto: `to_document` lowers the section into the `document/model#MODEL` tree in one pass — the `Spec` a level-1 `SectionNode` headed by the `{section} {TITLE}` run, each present part a level-2 `SectionNode` headed by `PART {n} {NAME}`, each article a level-3 `SectionNode` headed by its `PageFormat`-numbered `{1.01} {TITLE}` run, and each paragraph a `BlockNode` whose leading run is the `PageFormat.label(part, path)` designation and whose children recurse the sub-paragraph tree, the ordinal PATH threaded down so the numbering is the tree's own structure rather than a flattened string — bounded at `_MAX_LEVEL` depth so native recursion is safe where the `model#MODEL` general tree needs a depth-safe frontier. A `NOTE` paragraph lowers to a `BlockKind.ARTIFACT` block (the `document/model#MODEL` decorative kind the PDF/UA tag tree excludes and `document/emit#DOCUMENT` strips at issue), unnumbered, so the CONTENT ordinal `_article_node` threads is a running `accumulate` count that never advances over a note — the retained paragraphs number contiguously with no gap where a stripped note sat, the one place a naive spec producer leaks a numbering hole. `_audited(spec)` folds the verdict in one pass — the part/article/paragraph tallies, the specifier-note count and the unresolved-fill-in count (`_FILL_IN` over each CONTENT paragraph's text, a section issued carrying a `[____]`/`<Insert>` blank being editorially incomplete), the `SpecMethod`/`SubmittalClass` histograms and reference count over the `_flat` paragraph walk, the `_cited` REFERENCES-vs-body citation reconciliation (`cited - listed` the unlisted-reference gap), the `_SUBORDINATE` first-level-heading off-checklist reconciliation (a subordinate paragraph heading absent from its article's SF-1 checklist, the intra-article counterpart of the citation reconciliation), the canonical-order check comparing each part's article positions against `_ARTICLES[part]`, and the mandatory main-work check (an `EXECUTION` part carrying none of `_MAIN_WORK`) accumulated into the coverage-fault tags — so the QA verdict is one accumulating fold rather than a per-check re-walk. The whole encode runs off the event loop through the bounded `to_thread.run_sync(self._encoded, limiter=_GATE)` because `msgspec` node encoding is GIL-releasing native work, and `contribute` re-enters the same deterministic encode synchronously.
- Receipt: `Spec` contributes the one new `core/receipt#RECEIPT` `ArtifactReceipt.Spec(key, section, division, parts, articles, bytes)` case — the MasterFormat section number, the `ClassCode.division()` head the `classify#CLASSIFY` crosswalk keys on (the `Spec` receipt "carries its section's `division`" `classify` names), the count of present parts, the total article count, and the encoded-tree byte count — read off the same `_encoded` bytes and `_audited` verdict `of` drives, so the producer adds ONE new receipt case (the specification evidence the `[07]-[SEAM_UNIFICATION]` target admits as a CASE on the one `ArtifactReceipt` family, never a parallel receipt rail) and the `_facts` zip projects it. `contribute` computes the tree bytes ONCE and derives both the content key and the byte count from that single fact, never a second encode to mint the receipt.
- Packages: `msgspec` (`Struct(frozen=True)` the `Spec`/`Article`/`Paragraph`/`SpecVerdict` value objects, `Struct(frozen=True, gc=False)` the scalar verdict leaf); `expression` (`tagged_union`/`tag`/`case` the `SpecFault` union; `Block.of_seq`/`choose`/`reduce`/`is_empty` the accumulating admission fold; `Result`/`Ok`/`Error`/`Option` the admission and lookup rails); `pydantic` (`TypeAdapter` the module-level `_PAYLOAD` gate over the recursive `SpecPayload` `TypedDict`, `ValidationError` mapped to `<invalid-payload>` at the seam); `frozendict` (`_ARTICLES`/`_SUBORDINATE` the owned vocabulary correspondences, the verdict histograms); `re` (`_FILL_IN` the unresolved-fill-in `Pattern` `findall`-counted per CONTENT paragraph); `anyio` (`CapacityLimiter`/the runtime thread lane the bounded off-loop encode); `itertools.accumulate` (the CONTENT-only running ordinal a note never advances); `collections.Counter` (the method/submittal histograms); runtime (`identity.ContentIdentity`/`ContentKey` the section key, `faults.RuntimeRail`/`async_boundary` the rail and fault capsule); `specification/classify#CLASSIFY` (`ClassCode`/`ClassSystem` the section number and division, composed never re-declared); `document/model#MODEL` (`SectionNode`/`BlockNode`/`RunNode`/`NodeMeta`/`BlockKind` — the `PARAGRAPH` content block AND the `ARTIFACT` decorative note block — and `encode`, the tree it lowers into); `PageFormat` (the co-located `[02]-[PAGE]` numbering owner); `core/receipt#RECEIPT` (`ArtifactReceipt.Spec` the new contributed case). No new external library — every engine is admitted, and the specification composes the classification, document, and numbering owners rather than re-authoring them.
- Growth: a new SectionFormat edition's article is one `_ARTICLES` row; a new subordinate paragraph title is one `_SUBORDINATE` row; a new alternative main-work title is one `_MAIN_WORK` member; a new method of specifying is one `SpecMethod` member the histogram absorbs; a new submittal regime is one `SubmittalClass` member; a new editorial disposition is one `ParagraphRole` member plus one `_paragraph_node` lowering arm (a `NOTE` already routes to `BlockKind.ARTIFACT`); a new QA rule is one `SpecVerdict` field and one `_audited` detection; a new fault cause is one `SpecFault` case plus one `combined` member; a new section-number system is already carried by `ClassCode`'s own `ClassSystem` growth; a new document node the lowering emits is one `to_document` arm over the `model#MODEL` tree; zero new surface — the producer grows by vocabulary row, verdict field, and fault case.
- Boundary: no page render (`document/emit#DOCUMENT` folds the tree to PDF/Typst/HTML), no numbering re-derivation (`PageFormat` owns it), no classification re-parse (`classify#CLASSIFY` owns `ClassCode`), no QTO/IFC authoring (`csharp:Rasm.Bim` owns them; a specification cites the QTO facts through `visualization/table#TABLE`, never re-computed). The deleted forms are a stringly `part: str`/`article: str` field where the closed `SectionPart`/`_ARTICLES` vocabulary types it, a `.startswith("PART 1")` heading probe where `_ARTICLES[part]` validates the title, a per-part `_general_tree`/`_products_tree`/`_execution_tree` builder family where one `to_document` folds every part, a fabricated article title outside the CSI roster where `_ARTICLES` states the closed set, a first-fault-abort admission where the accumulating disposition reports every bad article, a second document type beside the `model#MODEL` tree where `to_document` lowers into the canonical owner, a flattened `"1.01 A. 1."` numbering string where the ordinal PATH threads the tree structure, a `note: bool` flag beside the paragraph where the closed `ParagraphRole` vocabulary types the disposition, a specifier note lowered as a structural `BlockKind.PARAGRAPH` polluting the PDF/UA tag tree where the decorative `BlockKind.ARTIFACT` the tagged tree excludes carries it, a note consuming a paragraph ordinal so the issued manual leaks a numbering hole where the `accumulate` CONTENT-only counter numbers the retained paragraphs contiguously, an unresolved `[____]` fill-in issued silently where the `_FILL_IN` count refuses it, a body-cited reference standard absent from the `REFERENCES` article silently dropped where the `_cited` reconciliation surfaces it, a re-encode for the receipt where `contribute` reads the one `_encoded` fact, a parallel `specification`-receipt rail where the one new `ArtifactReceipt.Spec` case carries the evidence, and an event-loop-blocking encode where the bounded `to_thread` band offloads it. This owner authors the specification, never the rendered page.

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

# `NumberLevel`/`PageFormat` are the co-located `[02]-[PAGE]` owners declared above in this same module
# (`specification/section.py`) — in scope directly, never a self-import of the module being defined.

if TYPE_CHECKING:
    from rasm.runtime.receipts import Receipt

# --- [TYPES] ----------------------------------------------------------------------------


class SectionPart(StrEnum):
    GENERAL = "general"  # PART 1 — administrative/procedural requirements unique to the section
    PRODUCTS = "products"  # PART 2 — materials/products/equipment/fabrication/finishes at the required quality
    EXECUTION = "execution"  # PART 3 — installation/application + preparatory, quality-control, and post-install work


class SpecMethod(StrEnum):  # the four CSI methods of specifying a product/paragraph
    DESCRIPTIVE = "descriptive"  # exact properties and workmanship, no product name
    PERFORMANCE = "performance"  # required results + criteria/verification, means left open
    REFERENCE_STANDARD = "reference"  # compliance by published ASTM/ANSI/UL standard
    PROPRIETARY = "proprietary"  # named product/manufacturer (open or closed)


class SubmittalClass(StrEnum):  # the CSI SubmittalFormat regimes
    ACTION = "action"  # requires A/E review + approval before proceeding (shop drawings, product data, samples)
    INFORMATIONAL = "informational"  # for record, no approval (certificates, test reports, qualification statements)
    CLOSEOUT = "closeout"  # project record documents, O&M data, and warranties at completion


class ParagraphRole(StrEnum):  # the editorial disposition every master-spec paragraph carries at edit time
    CONTENT = "content"  # specification text, retained in the issued manual
    NOTE = "note"  # specifier note / editing guidance (`SPEC NOTE:`), stripped at issue -> `BlockKind.ARTIFACT`


# --- [CONSTANTS] ------------------------------------------------------------------------

_MAX_DEPTH: Final[int] = NumberLevel.DETAIL.value  # 7 — deepest CSI PageFormat sub-article level; nesting past it faults
# the master-spec unresolved-fill-in vocabulary: a bracketed blank `[____]` or an angle-bracket insert prompt
# `<Insert value>` the specifier never resolved — a section issued carrying one is editorially incomplete.
_FILL_IN: Final[re.Pattern[str]] = re.compile(r"\[_{2,}\]|<[^<>]+>")
_PART_NUMBER: Final[Map[SectionPart, int]] = Map.of_seq([(SectionPart.GENERAL, 1), (SectionPart.PRODUCTS, 2), (SectionPart.EXECUTION, 3)])

# --- [TABLES] ---------------------------------------------------------------------------

# the CSI MP-2-2 SectionFormat primary article roster per part, in published order (Figure SF-1). The order IS
# load-bearing: the `_audited` canonical-order check reads each title's index. Article titles are UPPERCASE, the
# owned closed vocabulary a heading validates against rather than a stringly `.startswith` probe.
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
# the alternative main-work titles an EXECUTION part selects EXACTLY ONE of (you ERECT steel, INSTALL equipment,
# APPLY coatings, CONSTRUCT concrete); an EXECUTION part carrying none is the `missing_main_work` coverage fault.
_MAIN_WORK: Final[frozenset[str]] = frozenset({"ERECTION", "INSTALLATION", "APPLICATION", "CONSTRUCTION"})
# the checklist of subordinate paragraph titles each primary article offers (Figure SF-1) — the vocabulary a
# specification author selects paragraph headings from; carried as data, keyed by the primary article title.
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
    # the closed admission-fault vocabulary carrying its offending token; the accumulating disposition folds
    # every casualty through `combined` so a whole section reports each bad article rather than aborting first.
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
    # the recursive specification content node — its heading, text, editorial disposition (a specifier note
    # lowers to a decorative `BlockKind.ARTIFACT`), its optional method of specifying and submittal class, its
    # reference-standard citations, and its sub-paragraph children; numbered by depth in `to_document`.
    text: str
    title: str = ""  # the CSI SF-1 subordinate paragraph heading; `_audited` reconciles a first-level title against `_SUBORDINATE[article.title]`
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
    coverage: tuple[
        str, ...
    ]  # accumulated coverage-fault tags (`empty_section`/`out_of_order`/`missing_main_work`/`unresolved_fill_ins`/`unlisted_references`/`off_checklist_titles`)

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
    # partition every body citation by whether its article IS the REFERENCES roster (`listed`) or any other
    # (`cited`) in one paragraph walk, so `cited - listed` is the unlisted-reference completeness gap and
    # `listed | cited` the distinct-standard tally — one fold, never a bool-selected two-body helper.
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
    # the accumulating disposition: partition the railed rows, combine every fault through the monoid, and return
    # the whole set only when the casualty set is empty — the `Validation`-style fold `register#REGISTER` shares.
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
    # one accumulating pass: the tallies, the editorial note/fill-in counts, the method/submittal histograms and
    # reference count over the `_flat` walk, the per-part canonical-order check, the mandatory main-work check,
    # and the REFERENCES-article citation reconciliation all fold once.
    articles = spec.articles
    walked = tuple(paragraph for article in articles for paragraph in _flat(article.paragraphs))
    methods = Counter(paragraph.method for paragraph in walked if paragraph.method is not None)
    submittals = Counter(paragraph.submittal for paragraph in walked if paragraph.submittal is not None)
    notes = sum(1 for paragraph in walked if paragraph.role is ParagraphRole.NOTE)
    fill_ins = sum(_fill_ins(paragraph.text) for paragraph in walked if paragraph.role is ParagraphRole.CONTENT)
    # the CSI SF-1 subordinate-heading reconciliation: a first-level CONTENT paragraph carrying a heading off its
    # article's `_SUBORDINATE` checklist is an off-checklist title the QA pass surfaces (the drawing↔spec-style
    # evidence discipline applied to the intra-article paragraph vocabulary), never an admission fault.
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
        # the one boundary ingress: the shape gate through `_PAYLOAD`, the `ClassCode` section-number seam, then
        # the accumulating article fold so a whole client section reports each malformed article at once.
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
        # lower the section into the `document/model#MODEL` tree: a level-1 `SectionNode` over the present parts,
        # each part a level-2 node, each article a `PageFormat`-numbered level-3 node, each paragraph a numbered
        # `BlockNode` recursing its sub-tree — the ordinal PATH the numbering, `document/emit#DOCUMENT` folds FROM.
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
        # key-over-INPUT: canonical section identity + admitted payload minted PRE-RUN — never a key over encoded bytes.
        return ContentIdentity.of(f"spec-{self.section.render()}", self.section, policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the renamed private thunk — the receipt threads the PRE-RUN key; the contribute weave dies into it.
        verdict = self.audit()
        return (await async_boundary(f"spec.{self.section.render()}", self._encoded_rail, catch=_FAULTS)).map(
            lambda payload: ArtifactReceipt.Spec(
                self._key, self.section.render(), self.section.division(), verdict.parts_present, verdict.articles, len(payload)
            )
        )

    async def _encoded_rail(self) -> bytes:
        # the offloaded encode returns the PAYLOAD BYTES the receipt counts — identity minting stays `_key`'s.
        crossed = await LanePolicy.offload(self._encoded, modality=Modality.THREAD, retry=RetryClass.OCCT)
        return crossed.default_with(_spec_raise)

    def _encoded(self) -> bytes:
        return encode(self.to_document())

    def contribute(self) -> "Iterable[Receipt]":
        payload = self._encoded()  # the ONE fact both the key and the byte count derive from — never a second encode
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
        # a specifier NOTE never consumes a paragraph ordinal (it is stripped at issue), so the CONTENT ordinal is
        # a running `accumulate` count — the retained paragraphs number contiguously with no gap where a note sat.
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
