# [PY_ARTIFACTS_MODEL]

The semantic document algebra: the single interior representation the `document` axis lowers FROM and recovers TO. `DocumentNode` is ONE recursive `msgspec` tagged-union tree (page/section/block/run/list/table/figure/field/annotation/structure) carrying a closed `NodeMeta` tag on every node, and `DocumentDelta` is ONE diff/merge algebra (inserted/deleted/moved/reparametrized edits) keyed by the stable content key and defined once over the tree as one `expression` immutable fold. Every `folder:document/emit#DOCUMENT` backend becomes a lowering arm folding FROM this tree rather than dispatching an opaque payload, and `folder:document/lens#LENS` is the recover-TO inverse that rebuilds it — production and extraction are inverses over one node algebra, the extracted-tree corpus keys into the runtime columnar lane as a queryable value, and the `DocumentDelta` a structural object-graph diff reuses is defined here once. The tree round-trips through `msgspec.msgpack` (the same canonical codec `folder:../../../runtime/evidence/identity#CONTENT_IDENTITY` keys a corpus on) so a multi-PDF corpus is one content-keyed serialized value; identity comes from `ContentIdentity.of`, never re-minted. The lifecycle is fixed: a `DocumentNode` is the `[03]-[CANONICAL_OWNERS]` durable interior shape, the `CorpusRow`/`CorpusRecord` egress projections are its `[06]-[PROJECTIONS_AND_PORTS]` outward derivations, and no boundary codec attribute lives on the interior.

## [01]-[INDEX]

- [01]-[NODE]: `DocumentNode` — the recursive eleven-variant `msgspec` tagged-union tree (the `FormulaNode` math carrier realizing the `FORMULA` structure role) + the `NodeMeta` closed tag every node carries (content key, semantic role, page, optional `bounds`/`lang`/`actual_text` accessibility evidence, optional `classification` CSI/OmniClass code) + the `StructRole` closed PDF/UA structure-type family (`StandardRole(StructEltKind)` over the full ISO 14289 vocabulary incl. the East-Asian ruby/warichu and `NonStruct`/`Private` roles with the `_STRUCT_CATEGORY` `frozendict` behavior table carrying `StructCategory`/`heading_level`, `ForeignRole(role)` the one open arm) + the AEC `Xref` detail-on-sheet/spec-section cross-reference `AnnotTarget` case + the `CorpusRow`/`CorpusRecord` columnar projections carrying the `AltStatus` alt-presence, `classification` CSI/OmniClass, and `xref` drawing<->spec cross-reference columns; the content-keyed `children`/`walk`/`node_digest`/`role_of`/`role_category`/`standard_for`/`alt_of`/`to_corpus`/`to_typst_source`/`to_html`/`to_lxml_tree`/`to_c14n`/`to_json`/`to_markdown`/`to_latex`/`encode`/`decode` tree algebra over one polymorphic projection entrypoint (the CommonMark/GFM `to_markdown` and journal-submission `to_latex` the plain-text diffable/typeset manuscript egress the `document/emit#DOCUMENT MARKDOWN`/`LATEX` arms lower, the deterministic Canonical-XML-2.0 `to_c14n` and structured-JSON `to_json` the JATS/JSON-LD archival-interchange egress), `walk` and `node_digest` iterative depth-safe frontiers over the recursive tree.
- [02]-[DELTA]: `DocumentDelta` — the four-variant edit algebra (inserted/deleted/moved/reparametrized) keyed by the stable `NodeMeta.key`; `diff`/`merge`/`invert` defined once over the tree as one total `expression` `Map`/`Block` fold, never a `list.append` accumulator.

## [02]-[NODE]

- Owner: `DocumentNode` the one recursive interior tree — eleven `msgspec.Struct` variants (`PageNode`/`SectionNode`/`BlockNode`/`RunNode`/`ListNode`/`TableNode`/`FigureNode`/`FormulaNode`/`FieldNode`/`AnnotationNode`/`StructureNode`) under one `tag`-discriminated `Union` on `tag_field="kind"`, every variant carrying a `NodeMeta` value object (content key, semantic role, page index, optional `bounds`, optional `lang` BCP-47 tag, optional `actual_text` replacement string, optional `classification` CSI/OmniClass code). The tree is the algebra emission lowers FROM and extraction recovers TO; a flat `class DocumentNode` with a `kind: str` field and an `if kind == "page"` cascade is the rejected non-total shape.
- Cases: `PageNode` (page-rooted child sequence + media box) · `SectionNode` (heading-level outline node + heading runs + child sequence) · `BlockNode` (paragraph/quote/code/caption block with a `BlockKind` row, a `1`-`6` heading `level`, and inline runs) · `RunNode` (styled text run: text, font key, size, `weight`, `italic`, a `TextDirection` base direction, a `RunScript` super/sub/normal baseline, a `TextDecoration` underline/strike/overline set, and `Rgb` color — the full character appearance the `folder:../typography/shape#SHAPE` shaping surface carries and `_styled` lowers, never a flag the lowering ignores) · `ListNode` (the list-structure node carrying a `ListKind` ordered/unordered/description row, an ordered-list `start` ordinal, and an `LI` item child sequence, the `L`/`LI`/`Lbl`/`LBody` PDF/UA grouping the lens recovers and the `#list`/`#enum(start:)`/`#terms` Typst markup lowers) · `TableNode` (row-major cell grid of child node sequences + a `spans` merged-cell quad set BOTH lowerings honor through Typst `table.cell(colspan:, rowspan:)`/HTML `<td colspan rowspan>` + `header_rows`/`footer_rows` counts designating the leading `THead` and trailing `TFoot` rows PDF/UA distinguishes, the lens `Table.header` recovers, and the Typst `table.header(repeat: true)`/`table.footer` lowering emits + a `caption` run sequence lowering to the Typst `#figure(kind: table, caption:)` wrapper and the HTML `<caption>` first-child, the PDF/UA `Caption` element both publication tables and AEC schedules title) · `FigureNode` (embedded-graphic node: content key of the placed asset + `MediaType` MIME + intrinsic `(width, height)` + caption runs + the `alt` text equivalent the `folder:document/tagged#ACCESS` AUDIT verifies) · `FormulaNode` (tree-resident equation: LaTeX `tex` math source + `display` inline/block flag + the `alt` text equivalent, the source the `FORMULA` `StructEltKind` role lowers so a journal equation and an AEC `ziamath` render are source-addressable rather than only a pre-rendered `FigureNode`) · `FieldNode` (interactive form field: name, `FieldKind` row, value, `FieldFlag` flag set, choice options) · `AnnotationNode` (markup/redaction/link annotation with an `AnnotKind` row + target rect + an `AnnotTarget` closed link family — external `Uri`, internal `Dest` page destination, the AEC `Xref` detail-on-sheet/spec-section cross-reference, or `NoTarget`). Each a frozen `Struct` variant, never a per-kind class hierarchy.
- Role: `StructRole` the one closed PDF/UA structure-type family — `StandardRole(StructEltKind)` over the `StructEltKind` `StrEnum` of the full ISO 14289 standard-structure vocabulary (the grouping `Document`/`Part`/`Art`/`Sect`/`Div`/`TOC`/`TOCI`/`Index`/`NonStruct`/`Private`, the headings `H1`-`H6`, the block roles `P`/`BlockQuote`/`Note`/`BibEntry`/`Code`/`Caption`, the inline roles `Span`/`Quote`/`Link`/`Reference`/`Annot` plus the East-Asian ruby `Ruby`/`RB`/`RT`/`RP` and warichu `Warichu`/`WT`/`WP` assemblies, the list roles `L`/`LI`/`Lbl`/`LBody`, the table roles `Table`/`THead`/`TBody`/`TFoot`/`TR`/`TH`/`TD`, the illustration roles `Figure`/`Formula`/`Form`), and `ForeignRole(role)` the one open arm carrying a `Meta`-constrained non-empty custom role. The closed `StrEnum` row-set plus the single `ForeignRole` arm is the totality the AUDIT closes over: every standard role a `StructEltKind` key (never a sibling struct), every foreign role one `Meta`-validated arm. The vocabulary is not bare — `_STRUCT_CATEGORY` is the one frozen behavior table keyed by `StructEltKind` carrying each role's `StructCategory` (`GROUPING`/`HEADING`/`BLOCK`/`INLINE`/`LIST`/`TABLE`/`ILLUSTRATION`) and its `heading_level` (`1`-`6` on `H1`-`H6`, `0` elsewhere), the one primary correspondence the AUDIT's structural-nesting and heading-monotonicity checks fold over rather than re-enumerating the roles per check, so the category and level a role carries derive from one table row and never a parallel `match`. `_STANDARD_FOR` is the one secondary map genuinely DERIVED from `_STRUCT_CATEGORY` by first-wins category inversion — each category's first-declared row is its canonical standard role (the inline canonical is `Span`, a foreign role's neutral `_FOREIGN_CATEGORY` grouping default is `Sect`) — exposed through the `standard_for` projection so the `folder:document/tagged#ACCESS` `/RoleMap` foreign-to-standard lowering reads a derived row rather than the hand-kept parallel dict that page formerly owned.
- Entry: `DocumentNode` is a `type` alias over the ten-variant `Union`; construction is direct variant instantiation, decode is `_DOCUMENT_DECODER.decode` (a reusable `msgpack.Decoder` typed on the union, the tag round-tripping under `tag_field="kind"`), and re-encode is `_ENCODER.encode` (the one reusable deterministic `msgpack.Encoder` the node digest, the corpus byte projection, and the public `encode` all share rather than three identical instances). `node_digest` folds a node's identity over its content + children into one `ContentKey` so the tree is content-addressed; `walk` yields every node in document order for the lens fold and the corpus projection.
- Auto: `children` is one total `match` projecting each variant to its interior child sequence (leaves return `()`); `node_digest` keys a leaf over `ContentIdentity.of(node.meta.key.fmt, _ENCODER.encode(node))` and an interior node over `ContentIdentity.of(node.meta.key.fmt, (own-field digest, *child digests))` — `_own_bytes` folding the container's own non-child fields (a changed `header_rows`/`level`/`start`/`list_kind`) beside the child digests so an identical sub-tree keys identically and a re-parametrized container re-keys rather than colliding on its unchanged children; `walk` is a pre-order generator over `children`. `alt_of` derives the `(AltText, AltStatus)` pair in one `FigureNode | FormulaNode` or-pattern discrimination (`PRESENT` with the authored `alt` on a non-empty figure/formula, `ABSENT` with `""` on an un-authored one, `NA` with `""` on every other node), so the accessibility audit queries alt presence as one `kind in (FIGURE, FORMULA) and alt_status == ABSENT` column predicate over the corpus rather than re-deriving emptiness per row — the ISO 14289 `/Alt` requirement covers a `Figure` AND a `Formula` element alike. `to_corpus(node, view)` is the ONE polymorphic columnar projection discriminating on a `CorpusView` axis: `STRUCT` returns the typed `CorpusRow` `Struct` the runtime columnar lane ingests as a value — its `kind` admitted through `NodeKind(node.__struct_config__.tag)` (the variant tag `msgspec` already minted, never a parallel kind `match`), its `page` the `int` preserved, its `lang`/`actual_text` the accessibility columns the audit reads, its `(alt, alt_status)` the `alt_of` pair, its `classification` the `NodeMeta` CSI/OmniClass code the `specification/classify#CLASSIFY` `ReferenceIndex` resolver keys on, and its `xref` the `_link_cite` `AnnotationNode`-`Xref` citation the drawing<->spec cross-reference resolver reads as one column predicate over the corpus exactly as the audit reads `alt_status`; `BYTES` lowers that row through the shared `_ENCODER` so the queryable corpus is one content-keyed serialized value; `RECORD` lowers the same typed `CorpusRow` to the flat `dict[str, object]` (`msgspec.to_builtins`) the `data/tabular/columnar#SCAN` `Corpus` arm's `pa.Table.from_pylist` ingests at the `data ← python:artifacts/document [WIRE]` seam — the producer owns this one mapping projection because `from_pylist` rejects a `msgspec.Struct` directly, the field names and native scalar dtypes (`str`/`StrEnum`-value-`str`/`int`) fixing the columnar Arrow schema so producer and consumer agree on the column shape, the `CorpusRow` `Struct` staying the typed interior and the byte/flat-record forms its two egress projections behind the one `view`-keyed entrypoint, never a `to_corpus_row`/`encode_corpus_row`/`to_corpus_record` sibling triple. `role_of` is the one polymorphic role projection discriminating on input shape — a `StructRole` lowers to its `StructEltKind` value or `ForeignRole` string, a `StructureNode` lowers through its `role` field, and every non-structure `DocumentNode` lowers to `NodeMeta.role` — so the corpus `role` column, the AUDIT role read, and the standard-vs-foreign discriminant resolve through one entrypoint, never a `corpus_role`/`role_of` pair. `role_category` projects a `StructRole` to its `StructCategory`/`heading_level` pair through the `_STRUCT_CATEGORY` table, a `ForeignRole` resolving to the named `_FOREIGN_CATEGORY` open-default row, so the AUDIT's nesting and heading-level checks read one table lookup. `standard_for` projects a `StructRole` to the canonical `StructEltKind` the `folder:document/tagged#ACCESS` `/RoleMap` foreign-to-standard lowering writes — a `StandardRole` to its own `elt`, a `ForeignRole` to its category's first-declared standard role via `_STANDARD_FOR` — so the tagged owner consumes one model projection, never a parallel category dict. `to_typst_source` is the one Typst-markup lowering folding the tree to the source string the `document/emit#DOCUMENT` typst rows compile, escaping every interpolated `RunNode.text`/heading/caption through the markup-context `_typst(..., TypstScope.MARKUP)` and the `FigureNode` `asset_key`/`alt` and link `Uri` through the string-context `_typst(..., TypstScope.STRING)` so a run carrying `]`/`#`/`*` never breaks `caption: [..]`/`#strong[..]` markup, defined once here so the two Typst escaping contexts share one `str.maketrans` algebra rather than per-arm string templates; the `_image` emitter owns the inner `image(source, alt: ..)` per the `.api/typst.md` `[MARKUP_ELEMENT_SCOPE]` `none | str` `alt` law (an authored figure writes `alt: "<escaped>"`, an un-authored figure writes `alt: none` — the `AltStatus.ABSENT` fact the corpus distinguishes, never collapsed to a meaningless `alt: ""`), the enclosing `#figure(.., caption: [..])` reserves its own `alt` slot for custom-content figures; a styled `RunNode` lowers its full appearance through the `_styled` `pipe` fold (`#strong`/`#emph` for weight+italic, `#super`/`#sub` for baseline, the `_DECORATION_MARKUP`-rowed `#underline`/`#strike`/`#overline` decoration set, `#text(dir: rtl)[..]` for a `TextDirection.RTL` run, and `#text(rgb(..))[..]` for a non-black color), a `BlockKind.QUOTE` block lowers through `#quote(block: true)[..]` and a `BlockKind.CODE` block through string-escaped `#raw("..", block: true)` rather than the prior generic-paragraph collapse that ignored the kind, a decorative `BlockKind.ARTIFACT` block lowers through `pdf.artifact[..]` so it is excluded from the tagged structure tree, a list lowers through the `_LIST_MARKUP`-rowed `#list`/`#enum(start:)`/`#terms` builder (an ordered list carrying its `start` ordinal, a description list folding each item to one `terms.item([term], [body])` pair), and a `TableNode` lowers its `header_rows`/`footer_rows` through the Typst `table.header(repeat: true, ..)`/`table.footer(..)` row-band elements rather than the invalid `table.header.repeat` key the prior arm emitted, its `spans` merged cells through `table.cell(colspan:, rowspan:)` with the grid width folded from row 0's colspans (the prior arm stored `spans` yet emitted a flat unmerged grid that ignored it), and its `caption` through the `#figure(kind: table, caption: [..])` wrapper carrying the plain table call form; the future `document/emit#DOCUMENT TYPST_QUERY` pass reads the `image` selector back through `typst.query` to verify every figure carries the equivalent.
- Receipt: the recovered tree contributes the `folder:core/receipt#RECEIPT` introspection case (content key, node count, text length, image count, hit count) at the lens boundary; `model.md` owns the tree type and its digest, never the receipt fold — authoring stays at `document/emit`, recovery at `document/lens`.
- Packages: `msgspec` (`Struct(frozen=True, tag=..., tag_field=...)` variant tree, `Union` alias, one shared `msgpack.Encoder(order="deterministic")` + `msgpack.Decoder` typed round-trip over both the node tree and the corpus row, one shared `json.Encoder(order="deterministic")` the `to_json` structured-data (JSON-LD/JATS-adjacent) interchange egress lowers the whole tagged-union tree through, `to_builtins` the flat-record `RECORD` lowering of the typed `CorpusRow` to the `dict[str, object]` the `data/tabular/columnar#SCAN` `Corpus` arm's `from_pylist` ingests at the WIRE seam, `structs.replace` copy-with, `__struct_config__.tag` the `structs.StructConfig` runtime view of the variant tag the corpus `kind` column admits through `NodeKind(...)` with no kind `match`, `Meta` `Annotated`-constraint admission on the `ForeignRole.role`/`FigureNode.alt`/`NodeMeta.lang`/`NodeMeta.classification`/`CorpusRow.alt`/`MediaType` text fields, `UnsetType`/`UNSET` the wire-absent `NodeMeta.lang`/`actual_text`/`classification` markers round-tripping under `omit_defaults`); `expression` (`pipe` the `_styled` run-markup composition on the node side; `Map.empty`/`add`/`try_find` the `diff`/`merge` structural index and `Block.of_seq`/`fold`/`choose`/`collect` the immutable edit traversal on the delta side); `functools.reduce` (the `_styled` `_DECORATION_MARKUP` decoration fold over the variable-arity `RunNode.decorations` tuple); `builtins.frozendict` (`_STRUCT_CATEGORY` the primary role behavior table, `_STANDARD_FOR` its first-wins-inverted secondary, `_TYPST_ESCAPE`/`_LIST_MARKUP`/`_DECORATION_MARKUP`/`_DECORATION_CSS`/`_BLOCK_HTML`/`_LIST_HTML` the immutable markup-spelling tables); `lxml.etree` (the `to_lxml_tree`/`to_html`/`to_c14n` escape-safe `Element`/`SubElement` builder, `tostring(method="html")` the presentation lowering and `tostring(method="c14n2")` the deterministic Canonical-XML-2.0 archival-interchange egress off the ONE `_element` builder, deferred under module-scope `lazy from lxml import etree` so a Typst-only or corpus-only consumer never pays the libxml2 load); `expression.collections.Block` (the `walk` pre-order and `node_digest` expand/combine depth-safe frontier work-stacks); runtime (`content_identity.ContentIdentity`/`ContentKey` for the node digest and corpus key, consumed never re-minted). `TextDirection` carries the `bidi.get_base_level`/`get_display(base_dir=)` paragraph-direction vocabulary as interior data — the `folder:../typography/shape#SHAPE` shaper owns the reorder, never `model.md`.
- Growth: a new document concept is one `DocumentNode` variant (a frozen `Struct` carrying its payload + `NodeMeta`) plus one `children`/`to_typst_source`/`_element` (serving `to_html` AND `to_c14n` off one builder)/`to_markdown`/`to_latex` arm — `to_json` needs none, the whole tagged-union tree encodes; the decoder, the diff fold, and every backend pick it up by the total `match`. The `FormulaNode` math carrier is exactly that shape: one variant, one arm per lowering, `alt_of` extended by one or-pattern alternative so the `Formula` alt-presence rides the corpus `alt_status` column, the `FORMULA` `StructEltKind` role realized end-to-end without a sibling structure. A new structured value on an existing node is one field. A new standard PDF/UA role is one `StructEltKind` member plus one `_STRUCT_CATEGORY` row, never a sibling struct — the East-Asian `Ruby`/`RB`/`RT`/`RP`/`Warichu`/`WT`/`WP` and the `NonStruct`/`Private` roles land exactly that way, completing the ISO 14289 standard-structure set for both the CJK-typesetting publication plane and the AEC-documentation plane; a foreign role rides the one `ForeignRole` arm and the `_STRUCT_CATEGORY` open default; a new structural category is one `StructCategory` member, one `_STRUCT_CATEGORY` re-keying, and the `_STANDARD_FOR` first-wins derivation absorbs it for free; a new run decoration is one `TextDecoration` member plus one `_DECORATION_MARKUP` row; a new run direction or baseline is one `TextDirection`/`RunScript` member plus one `_styled` `pipe` arm; a new link-target kind is one `AnnotTarget` case (the AEC `Xref` cross-reference is exactly that); a new classification axis is one `NodeMeta.classification` value the `to_corpus` column already carries; a new list dialect one `ListKind` member plus one `_LIST_MARKUP` row; zero new surface.
- Boundary: the opaque `dict[str, object]` payload `document/emit` formerly dispatched over is the deleted form — every backend now lowers from this tree. No durable store, no PDF parser (extraction is `document/lens`'s pymupdf/pypdf/lxml surface), no UI, no second tree type per backend. The tree is the canonical interior representation; the wire projection into the columnar corpus is the typed `CorpusRow` `Struct` lowered to its byte and flat-record egress shapes behind `to_corpus(node, view)`, never a hand-built stringly-typed `dict[str, str]` that erases the `kind`/`page` column types and never a parallel serialized model. `StructureNode.tag_role: str` is the deleted stringly-typed role; the closed `StructEltKind` vocabulary plus the one `ForeignRole(str)` arm is the audited replacement, and a per-role struct hierarchy beside the `StrEnum` is the rejected re-fragmentation. `FigureNode.alt` is the alt-text-presence fact the AUDIT verifies, owned here as one `AltText`-constrained field projected to the `CorpusRow.alt_status` column the audit reads as one predicate; a second alt-text field on a non-figure node, a free-`str` alt escaping the `AltText` bound, and a per-context alt re-derivation outside the one `_image` emission are the rejected re-fragmentations. The deleted `RoleView`/`role_contract`/`_ROLE_CONTRACT` schema-and-inspect view machinery (`json.schema_components`/`inspect.multi_type_info`/`structs.fields` blobs no consumer read) is the prime decorative surface removed: the `folder:document/tagged#ACCESS` AUDIT reads `role_of`/`role_category`/`alt_of`/`children` and nothing else, so a phantom contract view asserting an audit target that does not exist is the rejected illusory density. The `to_json` structured-data egress is NOT that deleted machinery re-added: it is `msgspec.json.Encoder(order="deterministic").encode(tree)` — a real JSON-LD/JATS-adjacent interchange serialization of the node tree a downstream consumer decodes, never a `json.schema_components` schema-shape blob no consumer reads; the `to_c14n` Canonical-XML-2.0 egress is the archival sibling off the same `_element` builder `to_html` already drives. An equation that could ride ONLY a pre-rendered `FigureNode` — its LaTeX source lost to the tree, the `FORMULA` `StructEltKind` role a member no node produces — is the deleted omission, closed by the `FormulaNode` tree-resident `tex` source the four manuscript lowerings emit verbatim and the `Formula` structure element carries with its `/Alt`; a second math-carrier field bolted onto `RunNode`/`BlockNode` beside the `FormulaNode` variant, and a Typst-native-math field parallel to the LaTeX `tex` where the `mitex` bridge already lowers it, are the rejected re-fragmentations. The `rtl: bool` field stored and round-tripped yet never read by the lowering is the deleted illusory flag — `direction: TextDirection` is the replacement the `_styled` fold honors; the `table.header.repeat: bool` dict key (invalid Typst, the `table.header` element takes `repeat:` as a named argument and the header cells as positional children) is the deleted broken spelling, replaced by the `table.header(repeat: true, ..)`/`table.footer(..)` row-band elements; the `spans` merged-cell quad set stored, round-tripped, and digested yet read by NEITHER lowering — the same illusory-field trap as the `rtl` flag, an AEC schedule's merged header silently flattened to an unmerged grid — is the deleted decorative field, now honored through the `table.cell(colspan:, rowspan:)`/`<td colspan rowspan>` emission the `_span_map` lookup drives; a `TableNode` with no `caption` where a publication table and an AEC schedule both title their grid is the deleted omission, closed by the `caption` run sequence the `#figure(kind: table)`/`<caption>` lowering carries as the `FigureNode.caption` sibling; the `BlockKind.QUOTE`/`CODE` cases collapsed into the generic-paragraph arm — declared vocabulary the lowering silently ignored — are the deleted hollow cases, each now its own `#quote(block: true)`/`#raw` arm; the two parallel identical `msgspec.msgpack.Encoder(order="deterministic")` instances (`_DOCUMENT_ENCODER`/`_CORPUS_ENCODER`) are the deleted duplication, one shared `_ENCODER` serving node, digest, and corpus. `NodeMeta.classification` is the rendered CSI/OmniClass notation the `specification/classify#CLASSIFY` owner produces (`ClassCode.render()`) and re-parses (`ClassCode.parse`), carried as a bounded `ClassificationCode` string rather than an imported `ClassCode` so the substrate tree never depends on the specification folder that lowers INTO it — the same string seam `drawing/detail#DETAIL` `DetailEntry.classification` holds; a `ClassCode` field on the interior tree inverting the `specification`->`document` dependency is the rejected coupling. The `Xref` cross-reference — a `detail`/`sheet` detail-on-sheet coordinate and a governing-section `code` — is the one AEC cross-reference `AnnotTarget` case the drawing/detail callout and the specification keynote both resolve over the tree; a parallel callout-target structure beside the tree, and a Typst `label()`-anchored intra-compilation link where the target sheet is a separate compilation the imposition assembly resolves, are the rejected forms. The `diff`/`merge` `list.append`+`dict` procedural accumulator is the deleted flat form; the `expression` `Map`/`Block` immutable fold is the rail-shaped replacement.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterator
from enum import StrEnum
from functools import reduce
from typing import TYPE_CHECKING, Annotated, Final, Literal, assert_never, overload

import msgspec
from builtins import frozendict
from expression import pipe
from expression.collections import Block
from msgspec import Meta, Struct, UnsetType, UNSET

from rasm.runtime.content_identity import ContentIdentity, ContentKey

lazy from lxml import etree  # the tree -> HTML/`_Element` lowering builder; cold, deferred to first `to_html`/`to_lxml_tree`

if TYPE_CHECKING:
    from lxml.etree import _Element

# --- [TYPES] ----------------------------------------------------------------------------


class NodeKind(StrEnum):
    PAGE = "page"
    SECTION = "section"
    BLOCK = "block"
    RUN = "run"
    LIST = "list"
    TABLE = "table"
    FIGURE = "figure"
    FORMULA = "formula"
    FIELD = "field"
    ANNOTATION = "annotation"
    STRUCTURE = "structure"


class BlockKind(StrEnum):
    PARAGRAPH = "paragraph"
    HEADING = "heading"
    QUOTE = "quote"
    CODE = "code"
    CAPTION = "caption"
    ARTIFACT = "artifact"  # decorative content -> Typst `pdf.artifact`, excluded from the tag tree


class ListKind(StrEnum):
    UNORDERED = "unordered"
    ORDERED = "ordered"
    DESCRIPTION = "description"


class RunScript(StrEnum):
    NORMAL = "normal"
    SUPER = "super"
    SUB = "sub"


class TextDirection(StrEnum):
    AUTO = "auto"  # shaper resolves the base level from content (`bidi.get_base_level` → 0/1)
    LTR = "ltr"
    RTL = "rtl"  # the `bidi.get_display(base_dir="R")` paragraph the shaper reorders


class TextDecoration(StrEnum):
    UNDERLINE = "underline"
    STRIKETHROUGH = "strikethrough"
    OVERLINE = "overline"


class FieldKind(StrEnum):
    TEXT = "text"
    CHECKBOX = "checkbox"
    CHOICE = "choice"
    SIGNATURE = "signature"
    BUTTON = "button"


class FieldFlag(StrEnum):
    REQUIRED = "required"
    READONLY = "readonly"
    MULTILINE = "multiline"
    PASSWORD = "password"


class AnnotKind(StrEnum):
    HIGHLIGHT = "highlight"
    REDACTION = "redaction"
    LINK = "link"
    NOTE = "note"
    STAMP = "stamp"


class StructEltKind(StrEnum):
    DOCUMENT = "Document"  # grouping
    PART = "Part"
    ART = "Art"
    SECT = "Sect"
    DIV = "Div"
    TOC = "TOC"
    TOCI = "TOCI"
    INDEX = "Index"
    NONSTRUCT = "NonStruct"  # grouping with no inherent structure (PDF/UA generic container)
    PRIVATE = "Private"  # producer-private content outside the logical structure tree
    H1 = "H1"  # headings
    H2 = "H2"
    H3 = "H3"
    H4 = "H4"
    H5 = "H5"
    H6 = "H6"
    P = "P"  # block-level
    BLOCKQUOTE = "BlockQuote"
    NOTE = "Note"
    BIBENTRY = "BibEntry"
    CODE = "Code"
    CAPTION = "Caption"
    SPAN = "Span"  # inline-level
    QUOTE = "Quote"
    LINK = "Link"
    REFERENCE = "Reference"
    ANNOT = "Annot"
    RUBY = "Ruby"  # East-Asian ruby (furigana) assembly over its RB/RT/RP parts
    RB = "RB"  # ruby base text
    RT = "RT"  # ruby annotation text
    RP = "RP"  # ruby punctuation (fallback delimiters)
    WARICHU = "Warichu"  # East-Asian inline warichu assembly over its WT/WP parts
    WT = "WT"  # warichu text
    WP = "WP"  # warichu punctuation
    L = "L"  # list grouping
    LI = "LI"
    LBL = "Lbl"
    LBODY = "LBody"
    TABLE = "Table"  # table grouping
    THEAD = "THead"
    TBODY = "TBody"
    TFOOT = "TFoot"
    TR = "TR"
    TH = "TH"
    TD = "TD"
    FIGURE = "Figure"  # illustration
    FORMULA = "Formula"
    FORM = "Form"


class StructCategory(StrEnum):
    GROUPING = "grouping"
    HEADING = "heading"
    BLOCK = "block"
    INLINE = "inline"
    LIST = "list"
    TABLE = "table"
    ILLUSTRATION = "illustration"


class AltStatus(StrEnum):
    PRESENT = "present"
    ABSENT = "absent"
    NA = "na"


class TypstScope(StrEnum):
    STRING = "string"
    MARKUP = "markup"


class CorpusView(StrEnum):
    STRUCT = "struct"
    BYTES = "bytes"
    RECORD = "record"


# --- [BOUNDARIES] -----------------------------------------------------------------------

type ForeignRoleStr = Annotated[str, Meta(min_length=1, max_length=64, pattern=r"\A[A-Za-z][\w.\-]*\Z")]
type AltText = Annotated[str, Meta(max_length=2048)]
type LangTag = Annotated[str, Meta(min_length=2, max_length=35, pattern=r"\A[A-Za-z]{2,3}(-[A-Za-z0-9]{2,8})*\Z")]
type MediaType = Annotated[str, Meta(min_length=3, max_length=127, pattern=r"\A[\w.+-]+/[\w.+-]+\Z")]
type ClassificationCode = Annotated[
    str, Meta(min_length=1, max_length=32, pattern=r"\A[A-Za-z0-9][\w .\-]*\Z")
]  # CSI/OmniClass notation `classify#CLASSIFY` renders/parses
type Rgb = tuple[int, int, int]
type Rect = tuple[float, float, float, float]

# --- [MODELS] ---------------------------------------------------------------------------


class NodeMeta(Struct, frozen=True, omit_defaults=True):
    key: ContentKey
    role: str
    page: int
    bounds: Rect | None = None
    lang: LangTag | UnsetType = UNSET  # PDF/UA `/Lang` BCP-47 tag; absent under `omit_defaults`
    actual_text: str | UnsetType = UNSET  # PDF/UA `/ActualText` replacement for non-textual glyphs
    classification: ClassificationCode | UnsetType = (
        UNSET  # CSI/OmniClass code the `specification/classify#CLASSIFY` resolver keys the drawing<->spec cross-reference on
    )


class StandardRole(Struct, frozen=True, tag="standard", tag_field="role_kind"):
    elt: StructEltKind


class ForeignRole(Struct, frozen=True, tag="foreign", tag_field="role_kind"):
    role: ForeignRoleStr


type StructRole = StandardRole | ForeignRole


class Uri(Struct, frozen=True, tag="uri", tag_field="target"):
    href: Annotated[str, Meta(min_length=1, max_length=4096)]


class Dest(Struct, frozen=True, tag="dest", tag_field="target"):
    page: int
    point: tuple[float, float] | None = None


class Xref(Struct, frozen=True, tag="xref", tag_field="target"):
    # the AEC cross-reference target the drawing/detail#DETAIL callout and the specification keynote both cite:
    # `detail`/`sheet` the `DetailRef.cite()` "3/A-501" detail-on-sheet coordinate, `code` the governing
    # `specification/classify#CLASSIFY` `ClassCode.render()` section — so a drawing<->spec cross-reference
    # resolves over the one tree, its cross-sheet target string resolved at `composition/imposition` assembly.
    sheet: str = ""
    detail: str = ""
    code: str = ""

    def cite(self) -> str:
        return f"{self.detail}/{self.sheet}" if self.detail and self.sheet else self.sheet or self.code


class NoTarget(Struct, frozen=True, tag="none", tag_field="target"):
    pass


type AnnotTarget = Uri | Dest | Xref | NoTarget


class PageNode(Struct, frozen=True, tag=NodeKind.PAGE.value, tag_field="kind"):
    meta: NodeMeta
    media_box: Rect
    children: tuple[DocumentNode, ...] = ()


class SectionNode(Struct, frozen=True, tag=NodeKind.SECTION.value, tag_field="kind"):
    meta: NodeMeta
    level: int
    heading: tuple[RunNode, ...] = ()
    children: tuple[DocumentNode, ...] = ()


class BlockNode(Struct, frozen=True, tag=NodeKind.BLOCK.value, tag_field="kind"):
    meta: NodeMeta
    block: BlockKind
    level: int = 1
    runs: tuple[RunNode, ...] = ()
    children: tuple[DocumentNode, ...] = ()


class RunNode(Struct, frozen=True, tag=NodeKind.RUN.value, tag_field="kind"):
    meta: NodeMeta
    text: str
    font_key: str
    size: float
    weight: int = 400
    italic: bool = False
    direction: TextDirection = TextDirection.AUTO
    script: RunScript = RunScript.NORMAL
    decorations: tuple[TextDecoration, ...] = ()
    color: Rgb = (0, 0, 0)


class ListNode(Struct, frozen=True, tag=NodeKind.LIST.value, tag_field="kind"):
    meta: NodeMeta
    list_kind: ListKind = ListKind.UNORDERED
    start: int = 1  # `ORDERED` first ordinal -> Typst `#enum(start:)`
    items: tuple[DocumentNode, ...] = ()  # one `LI` sub-tree per item


class TableNode(Struct, frozen=True, tag=NodeKind.TABLE.value, tag_field="kind"):
    meta: NodeMeta
    rows: tuple[tuple[DocumentNode, ...], ...] = ()
    spans: tuple[tuple[int, int, int, int], ...] = ()  # (row, present-cell index, col_span, row_span) merged-cell quads BOTH lowerings honor
    header_rows: int = 0  # leading `THead` rows -> Typst `table.header(repeat: true)` + `Table.header`
    footer_rows: int = 0  # trailing `TFoot` rows -> Typst `table.footer`
    caption: tuple[RunNode, ...] = ()  # table title -> Typst `#figure(kind: table, caption:)` + HTML `<caption>`, the PDF/UA `Caption` child


class FigureNode(Struct, frozen=True, tag=NodeKind.FIGURE.value, tag_field="kind"):
    meta: NodeMeta
    asset_key: ContentKey
    alt: AltText = ""
    media_type: MediaType = "image/png"
    intrinsic: tuple[float, float] | None = None
    caption: tuple[RunNode, ...] = ()


class FormulaNode(Struct, frozen=True, tag=NodeKind.FORMULA.value, tag_field="kind"):
    # the tree-resident equation the `FORMULA` `StructEltKind` role lowers, so a formula is source-addressable
    # (journal manuscript egress + AEC `ziamath` SVG) rather than only a pre-rendered `FigureNode`.
    meta: NodeMeta
    tex: str  # LaTeX math source `to_latex`/`to_markdown` emit verbatim, `ziamath` lowers to SVG; a TRUSTED authored math island, never markup-escaped
    display: bool = False  # block/display math (`\[..\]`, `$$..$$`, Typst `#mitex`) vs inline (`$..$`, Typst `#mi`)
    alt: AltText = ""  # the ISO 14289 `Formula` structure-element `/Alt` text equivalent the `folder:document/tagged#ACCESS` AUDIT verifies


class FieldNode(Struct, frozen=True, tag=NodeKind.FIELD.value, tag_field="kind"):
    meta: NodeMeta
    name: str
    field: FieldKind
    value: str | bool | None = None
    flags: tuple[FieldFlag, ...] = ()
    options: tuple[str, ...] = ()  # `CHOICE` candidate values


class AnnotationNode(Struct, frozen=True, tag=NodeKind.ANNOTATION.value, tag_field="kind"):
    meta: NodeMeta
    annot: AnnotKind
    target: Rect
    contents: str = ""
    link: AnnotTarget = msgspec.field(default_factory=NoTarget)


class StructureNode(Struct, frozen=True, tag=NodeKind.STRUCTURE.value, tag_field="kind"):
    meta: NodeMeta
    role: StructRole
    children: tuple[DocumentNode, ...] = ()


type DocumentNode = (
    PageNode | SectionNode | BlockNode | RunNode | ListNode | TableNode | FigureNode | FormulaNode | FieldNode | AnnotationNode | StructureNode
)


class CorpusRow(Struct, frozen=True):
    key: str
    kind: NodeKind
    role: str
    page: int
    text: str
    alt: AltText = ""
    alt_status: AltStatus = AltStatus.NA
    lang: str = ""
    actual_text: str = ""
    classification: str = ""  # the `NodeMeta.classification` CSI/OmniClass column the `classify#CLASSIFY` resolver queries
    xref: str = ""  # the `AnnotationNode` `Xref.cite()` column the drawing<->spec cross-reference resolver reads


# --- [CONSTANTS] ------------------------------------------------------------------------

_ENCODER: Final = msgspec.msgpack.Encoder(order="deterministic")  # one deterministic codec for node, digest, and corpus
_JSON_ENCODER: Final = msgspec.json.Encoder(
    order="deterministic"
)  # the structured-data (JSON-LD/JATS-adjacent) tree egress, the `to_json` interchange codec
_DOCUMENT_DECODER: Final = msgspec.msgpack.Decoder(DocumentNode)
_CHILD_FIELDS: Final[frozenset[str]] = frozenset({"children", "heading", "runs", "items", "caption", "rows"})  # every `children`-projected field
_TYPST_ESCAPE: Final[frozendict[TypstScope, dict[int, str]]] = frozendict({
    TypstScope.STRING: str.maketrans({"\\": "\\\\", '"': '\\"'}),
    TypstScope.MARKUP: str.maketrans({c: f"\\{c}" for c in "\\[]#*_@$<>`"}),
})
_LIST_MARKUP: Final[frozendict[ListKind, str]] = frozendict({ListKind.UNORDERED: "list", ListKind.ORDERED: "enum", ListKind.DESCRIPTION: "terms"})
_DECORATION_MARKUP: Final[frozendict[TextDecoration, str]] = frozendict({
    TextDecoration.UNDERLINE: "underline",
    TextDecoration.STRIKETHROUGH: "strike",
    TextDecoration.OVERLINE: "overline",
})
_DECORATION_CSS: Final[frozendict[TextDecoration, str]] = frozendict({
    TextDecoration.UNDERLINE: "underline",
    TextDecoration.STRIKETHROUGH: "line-through",
    TextDecoration.OVERLINE: "overline",
})
_BLOCK_HTML: Final[frozendict[BlockKind, str]] = frozendict({
    BlockKind.PARAGRAPH: "p",
    BlockKind.QUOTE: "blockquote",
    BlockKind.CAPTION: "figcaption",
    BlockKind.ARTIFACT: "div",
})  # `HEADING` -> `h{level}` and `CODE` -> `pre`/`code` are arm-built; this table carries the flat one-tag block kinds
_LIST_HTML: Final[frozendict[ListKind, str]] = frozendict({ListKind.UNORDERED: "ul", ListKind.ORDERED: "ol", ListKind.DESCRIPTION: "dl"})
# the plain-text manuscript spelling tables the `to_markdown`/`to_latex` lowerings read — the same
# markup-table discipline `_TYPST_ESCAPE`/`_BLOCK_HTML` hold, one row per active char / decoration / depth.
_MD_ESCAPE: Final[dict[int, str]] = str.maketrans({
    c: f"\\{c}" for c in "\\`*_[]<>|"
})  # CommonMark inline-active set: neutralize emphasis/code/link/autolink/table-pipe, never mangle every hyphen/period
_MD_DECORATION: Final[frozendict[TextDecoration, tuple[str, str]]] = frozendict({
    TextDecoration.UNDERLINE: ("<u>", "</u>"),  # GFM raw-HTML — CommonMark has no native underline
    TextDecoration.STRIKETHROUGH: ("~~", "~~"),  # GFM strikethrough
    TextDecoration.OVERLINE: ('<span style="text-decoration:overline">', "</span>"),  # GFM raw-HTML — no native overline
})
_LATEX_ESCAPE: Final[dict[int, str]] = str.maketrans({
    "\\": "\\textbackslash{}",
    "~": "\\textasciitilde{}",
    "^": "\\textasciicircum{}",
    "&": "\\&",
    "%": "\\%",
    "$": "\\$",
    "#": "\\#",
    "_": "\\_",
    "{": "\\{",
    "}": "\\}",
})  # the ten LaTeX-active characters — the three control-word forms plus the seven single-backslash escapes
_LATEX_SECTION: Final[frozendict[int, str]] = frozendict({
    1: "section",
    2: "subsection",
    3: "subsubsection",
    4: "paragraph",
    5: "subparagraph",
    6: "subparagraph",
})
_LATEX_DECORATION: Final[frozendict[TextDecoration, str]] = frozendict({
    TextDecoration.UNDERLINE: "uline",
    TextDecoration.STRIKETHROUGH: "sout",
    TextDecoration.OVERLINE: "overline",
})  # the `document/emit#DOCUMENT LATEX` preamble carries `ulem` (uline/sout) and a math-mode overline; the emit arm declares the package set

# --- [TABLES] ---------------------------------------------------------------------------

# The ONE primary correspondence: role -> (category, heading_level). The FIRST row of each category
# is its canonical role, so `_STANDARD_FOR` derives by first-wins inversion rather than a parallel literal.
_STRUCT_CATEGORY: Final[frozendict[StructEltKind, tuple[StructCategory, int]]] = frozendict({
    StructEltKind.SECT: (StructCategory.GROUPING, 0),
    StructEltKind.DOCUMENT: (StructCategory.GROUPING, 0),
    StructEltKind.PART: (StructCategory.GROUPING, 0),
    StructEltKind.ART: (StructCategory.GROUPING, 0),
    StructEltKind.DIV: (StructCategory.GROUPING, 0),
    StructEltKind.TOC: (StructCategory.GROUPING, 0),
    StructEltKind.INDEX: (StructCategory.GROUPING, 0),
    StructEltKind.NONSTRUCT: (StructCategory.GROUPING, 0),
    StructEltKind.PRIVATE: (StructCategory.GROUPING, 0),
    StructEltKind.H1: (StructCategory.HEADING, 1),
    StructEltKind.H2: (StructCategory.HEADING, 2),
    StructEltKind.H3: (StructCategory.HEADING, 3),
    StructEltKind.H4: (StructCategory.HEADING, 4),
    StructEltKind.H5: (StructCategory.HEADING, 5),
    StructEltKind.H6: (StructCategory.HEADING, 6),
    StructEltKind.P: (StructCategory.BLOCK, 0),
    StructEltKind.TOCI: (StructCategory.BLOCK, 0),
    StructEltKind.BLOCKQUOTE: (StructCategory.BLOCK, 0),
    StructEltKind.BIBENTRY: (StructCategory.BLOCK, 0),
    StructEltKind.NOTE: (StructCategory.BLOCK, 0),
    StructEltKind.CODE: (StructCategory.BLOCK, 0),
    StructEltKind.CAPTION: (StructCategory.BLOCK, 0),
    StructEltKind.SPAN: (StructCategory.INLINE, 0),
    StructEltKind.LINK: (StructCategory.INLINE, 0),
    StructEltKind.QUOTE: (StructCategory.INLINE, 0),
    StructEltKind.REFERENCE: (StructCategory.INLINE, 0),
    StructEltKind.ANNOT: (StructCategory.INLINE, 0),
    StructEltKind.RUBY: (StructCategory.INLINE, 0),
    StructEltKind.RB: (StructCategory.INLINE, 0),
    StructEltKind.RT: (StructCategory.INLINE, 0),
    StructEltKind.RP: (StructCategory.INLINE, 0),
    StructEltKind.WARICHU: (StructCategory.INLINE, 0),
    StructEltKind.WT: (StructCategory.INLINE, 0),
    StructEltKind.WP: (StructCategory.INLINE, 0),
    StructEltKind.L: (StructCategory.LIST, 0),
    StructEltKind.LI: (StructCategory.LIST, 0),
    StructEltKind.LBL: (StructCategory.LIST, 0),
    StructEltKind.LBODY: (StructCategory.LIST, 0),
    StructEltKind.TABLE: (StructCategory.TABLE, 0),
    StructEltKind.THEAD: (StructCategory.TABLE, 0),
    StructEltKind.TBODY: (StructCategory.TABLE, 0),
    StructEltKind.TFOOT: (StructCategory.TABLE, 0),
    StructEltKind.TR: (StructCategory.TABLE, 0),
    StructEltKind.TH: (StructCategory.TABLE, 0),
    StructEltKind.TD: (StructCategory.TABLE, 0),
    StructEltKind.FIGURE: (StructCategory.ILLUSTRATION, 0),
    StructEltKind.FORMULA: (StructCategory.ILLUSTRATION, 0),
    StructEltKind.FORM: (StructCategory.ILLUSTRATION, 0),
})
_FOREIGN_CATEGORY: Final[tuple[StructCategory, int]] = (
    StructCategory.GROUPING,
    0,
)  # an unknown role maps to a neutral grouping, never a figure carrying mandatory alt
# DERIVED secondary: category -> its canonical standard role (the `/RoleMap` target the tagged owner reads),
# first-wins inversion of `_STRUCT_CATEGORY` so the canonical is the first-declared role of each category.
_STANDARD_FOR: Final[frozendict[StructCategory, StructEltKind]] = frozendict({
    category: elt for elt, (category, _level) in reversed(tuple(_STRUCT_CATEGORY.items()))
})

# --- [OPERATIONS] -----------------------------------------------------------------------


def children(node: DocumentNode) -> tuple[DocumentNode, ...]:
    match node:
        case PageNode(children=kids) | StructureNode(children=kids):
            return kids
        case SectionNode(heading=head, children=kids):
            return (*head, *kids)
        case BlockNode(runs=runs, children=kids):
            return (*runs, *kids)
        case ListNode(items=items):
            return items
        case TableNode(rows=rows, caption=caption):
            return (*caption, *(cell for row in rows for cell in row))
        case FigureNode(caption=caption):
            return caption
        case RunNode() | FieldNode() | AnnotationNode() | FormulaNode():
            return ()
        case _ as unreachable:
            assert_never(unreachable)


def walk(node: DocumentNode) -> Iterator[DocumentNode]:
    stack = Block.singleton(node)
    while not stack.is_empty():  # Exemption: iterative pre-order frontier — native recursion overflows on an adversarial-depth tree
        head, stack = stack.head(), stack.tail()
        yield head
        stack = Block.of_seq(children(head)).append(stack)  # children before siblings keeps document order


def _own_bytes(node: DocumentNode, /) -> bytes:
    return _ENCODER.encode(msgspec.structs.replace(node, **{name: () for name in node.__struct_fields__ if name in _CHILD_FIELDS}))


def node_digest(node: DocumentNode) -> ContentKey:
    # depth-safe expand/combine frontier: a leaf keys its encoded bytes, a branch keys (own, *child keys)
    # in document order; the two immutable stacks replace the native recursion an adversarial tree overflows.
    frontier: Block[tuple[bool, DocumentNode]] = Block.singleton((False, node))  # (combine?, node)
    results: Block[ContentKey] = Block.empty()
    while not frontier.is_empty():  # Exemption: depth-safe digest frontier over the recursive node tree
        (combine, current), frontier = frontier.head(), frontier.tail()
        kids = children(current)
        if not kids:
            results = results.cons(ContentIdentity.of(current.meta.key.fmt, _ENCODER.encode(current)))
        elif combine:  # the reversed child push above resolves the kids onto `results` head in document order
            own = ContentIdentity.of(current.meta.key.fmt, _own_bytes(current))
            results = results.skip(len(kids)).cons(ContentIdentity.of(current.meta.key.fmt, (own, *results.take(len(kids)))))
        else:
            frontier = Block.of_seq((False, kid) for kid in reversed(kids)).append(frontier.cons((True, current)))
    return results.head()


def role_of(value: StructRole | DocumentNode) -> str:
    match value:
        case StandardRole(elt=elt):
            return elt.value
        case ForeignRole(role=name):
            return name
        case StructureNode(role=role):
            return role_of(role)
        case _:
            return value.meta.role


def role_category(role: StructRole) -> tuple[StructCategory, int]:
    match role:
        case StandardRole(elt=elt):
            return _STRUCT_CATEGORY[elt]
        case ForeignRole():
            return _FOREIGN_CATEGORY
        case _ as unreachable:
            assert_never(unreachable)


def standard_for(role: StructRole) -> StructEltKind:
    match role:
        case StandardRole(elt=elt):
            return elt
        case ForeignRole():
            return _STANDARD_FOR[role_category(role)[0]]
        case _ as unreachable:
            assert_never(unreachable)


def alt_of(node: DocumentNode) -> tuple[AltText, AltStatus]:
    # a figure AND a formula both carry the ISO 14289 `/Alt` requirement, so the audit's alt-presence predicate is
    # one `kind in (FIGURE, FORMULA) and alt_status == ABSENT` column read over the corpus rather than two walks.
    match node:
        case FigureNode(alt=alt) | FormulaNode(alt=alt):
            return alt, (AltStatus.PRESENT if alt else AltStatus.ABSENT)
        case _:
            return "", AltStatus.NA


def _link_cite(node: DocumentNode) -> str:
    # the `AnnotationNode` `Xref` citation projected to the corpus `xref` column so the drawing<->spec
    # cross-reference resolver reads a column predicate over the corpus, exactly as the audit reads `alt_status`.
    match node:
        case AnnotationNode(link=Xref() as xref):
            return xref.cite()
        case _:
            return ""


@overload
def to_corpus(node: DocumentNode, view: Literal[CorpusView.STRUCT] = ..., /) -> CorpusRow: ...
@overload
def to_corpus(node: DocumentNode, view: Literal[CorpusView.BYTES], /) -> bytes: ...
@overload
def to_corpus(node: DocumentNode, view: Literal[CorpusView.RECORD], /) -> dict[str, object]: ...
def to_corpus(node: DocumentNode, view: CorpusView = CorpusView.STRUCT, /) -> CorpusRow | bytes | dict[str, object]:
    alt, status = alt_of(node)
    row = CorpusRow(
        key=node.meta.key.hex,
        kind=NodeKind(node.__struct_config__.tag),
        role=role_of(node),
        page=node.meta.page,
        text="".join(run.text for run in walk(node) if isinstance(run, RunNode)),
        alt=alt,
        alt_status=status,
        lang="" if isinstance(node.meta.lang, UnsetType) else node.meta.lang,
        actual_text="" if isinstance(node.meta.actual_text, UnsetType) else node.meta.actual_text,
        classification="" if isinstance(node.meta.classification, UnsetType) else node.meta.classification,
        xref=_link_cite(node),
    )
    match view:
        case CorpusView.STRUCT:
            return row
        case CorpusView.BYTES:
            return _ENCODER.encode(row)
        case CorpusView.RECORD:  # `from_pylist` rejects a `Struct`; the producer owns the flat-record projection
            return msgspec.to_builtins(row)
        case _ as unreachable:
            assert_never(unreachable)


def to_typst_source(node: DocumentNode, *, title: str | None = None) -> str:
    # `title` prepends the escaped `#set document(title: "..")` set-rule the `document/emit#DOCUMENT` PDF/UA
    # variants require (a `ua-1` render hard-errors `missing document title` without it); the STRING-context
    # `_typst` escaper owns the quoting, so the emit seam composes this rather than a hand-rolled `.replace`.
    # The recursion routes through the default-`title=None` path, so the set-rule lands ONCE at the root.
    prelude = f'#set document(title: "{_typst(title, TypstScope.STRING)}")\n' if title is not None else ""
    return prelude + _typst_body(node)


def _typst_body(node: DocumentNode) -> str:
    match node:
        case RunNode():
            return _styled(node)
        case BlockNode(block=BlockKind.HEADING, level=level, runs=runs):
            return _heading(level, runs)
        case BlockNode(block=BlockKind.ARTIFACT, runs=runs, children=kids):
            return f"#pdf.artifact[{_runs(runs)}{''.join(to_typst_source(child) for child in kids)}]\n"
        case BlockNode(block=BlockKind.QUOTE, runs=runs, children=kids):
            return f"#quote(block: true)[{_runs(runs)}{''.join(to_typst_source(child) for child in kids)}]\n"
        case BlockNode(block=BlockKind.CODE, runs=runs):
            return f'#raw("{_typst("".join(run.text for run in runs), TypstScope.STRING)}", block: true)\n'
        case BlockNode(runs=runs, children=kids):
            return _runs(runs) + "".join(to_typst_source(child) for child in kids) + "\n"
        case ListNode(list_kind=ListKind.DESCRIPTION, items=items):
            return f"#terms({', '.join(_term_pair(item) for item in items)})\n"
        case ListNode(list_kind=ListKind.ORDERED, start=start, items=items):
            return f"#enum({f'start: {start}, ' if start != 1 else ''}{_items(items)})\n"
        case ListNode(list_kind=kind, items=items):
            return f"#{_LIST_MARKUP[kind]}({_items(items)})\n"
        case SectionNode(level=level, heading=head, children=kids):
            return _heading(level, head) + "".join(to_typst_source(child) for child in kids)
        case PageNode(children=kids):
            return "".join(to_typst_source(child) for child in kids) + "#pagebreak()\n"
        case StructureNode(children=kids):
            return "".join(to_typst_source(child) for child in kids)
        case TableNode(rows=rows, spans=spans, header_rows=head_n, footer_rows=foot_n, caption=caption):
            span_map, body_end = _span_map(spans), len(rows) - foot_n
            bands = (
                f"table.header(repeat: true, {_cells(rows[:head_n], span_map, 0)})" if head_n else "",
                _cells(rows[head_n:body_end], span_map, head_n),
                f"table.footer({_cells(rows[body_end:], span_map, body_end)})" if foot_n else "",
            )
            table = f"table(columns: {_column_count(rows, span_map)}, {', '.join(part for part in bands if part)})"
            return f"#figure({table}, caption: [{_runs(caption)}], kind: table)\n" if caption else f"#{table}\n"
        case FigureNode(asset_key=asset_key, alt=alt, caption=caption):
            return f"#figure({_image(asset_key, alt)}, caption: [{_runs(caption)}])\n"
        case FormulaNode(tex=tex, display=display):
            # the LaTeX source rides Typst's `mitex` LaTeX-math bridge (the `@preview` registry package the
            # `document/emit#DOCUMENT` Typst preamble imports as `#import "@preview/mitex": mi, mitex`, resolved
            # through the typst compiler's package cache) — `#mitex` for a display block, `#mi` inline; the tex is
            # STRING-escaped so a `\`/`"` survives the Typst string literal into the LaTeX the bridge parses.
            return f'#mitex("{_typst(tex, TypstScope.STRING)}")\n' if display else f'#mi("{_typst(tex, TypstScope.STRING)}")'
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Uri(href=href)):
            return f'#link("{_typst(href, TypstScope.STRING)}")[{_typst(text, TypstScope.MARKUP)}]'
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Dest(page=page, point=point)):
            x, y = point if point else (0.0, 0.0)
            return f"#link((page: {page + 1}, x: {x}pt, y: {y}pt))[{_typst(text, TypstScope.MARKUP)}]"
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Xref() as xref):
            # the cross-sheet citation is a destination string `composition/imposition` sheet-set assembly resolves — never a `label()` this compilation carries
            return f'#link("{_typst(xref.cite(), TypstScope.STRING)}")[{_typst(text, TypstScope.MARKUP)}]'
        case FieldNode() | AnnotationNode():
            return ""
        case _ as unreachable:
            assert_never(unreachable)


def encode(node: DocumentNode) -> bytes:
    return _ENCODER.encode(node)


def decode(payload: bytes) -> DocumentNode:
    return _DOCUMENT_DECODER.decode(payload)


def _styled(run: RunNode) -> str:
    wrapped = pipe(
        _typst(run.text, TypstScope.MARKUP),
        lambda b: f"#strong[{b}]" if run.weight >= 700 else b,
        lambda b: f"#emph[{b}]" if run.italic else b,
        lambda b: f"#super[{b}]" if run.script is RunScript.SUPER else f"#sub[{b}]" if run.script is RunScript.SUB else b,
        lambda b: reduce(lambda inner, deco: f"#{_DECORATION_MARKUP[deco]}[{inner}]", run.decorations, b),
        lambda b: f"#text(dir: rtl)[{b}]" if run.direction is TextDirection.RTL else b,
    )
    return wrapped if run.color == (0, 0, 0) else f"#text(rgb({run.color[0]}, {run.color[1]}, {run.color[2]}))[{wrapped}]"


def _typst(value: str, scope: TypstScope) -> str:
    return value.translate(_TYPST_ESCAPE[scope])


def _items(items: tuple[DocumentNode, ...]) -> str:
    return ", ".join(f"[{to_typst_source(item).strip()}]" for item in items)


def _span_map(spans: tuple[tuple[int, int, int, int], ...]) -> frozendict[tuple[int, int], tuple[int, int]]:
    return frozendict({(row, col): (col_span, row_span) for row, col, col_span, row_span in spans})


def _column_count(rows: tuple[tuple[DocumentNode, ...], ...], span_map: frozendict[tuple[int, int], tuple[int, int]]) -> int:
    # grid width folds row 0's per-cell colspans (a top-row cell is never covered from above), so a merged header spans correctly
    return sum(span_map.get((0, col), (1, 1))[0] for col in range(len(rows[0]))) if rows else 0


def _cells(rows: tuple[tuple[DocumentNode, ...], ...], span_map: frozendict[tuple[int, int], tuple[int, int]], base: int) -> str:
    return ", ".join(_cell_markup(to_typst_source(cell), span_map.get((base + r, c))) for r, row in enumerate(rows) for c, cell in enumerate(row))


def _cell_markup(content: str, span: tuple[int, int] | None) -> str:
    args = (
        ""
        if span is None
        else ", ".join(part for part in (f"colspan: {span[0]}" if span[0] != 1 else "", f"rowspan: {span[1]}" if span[1] != 1 else "") if part)
    )
    return f"table.cell({args})[{content}]" if args else f"[{content}]"


def _term_pair(item: DocumentNode) -> str:
    kids = children(item)
    term = to_typst_source(kids[0]).strip() if kids else to_typst_source(item).strip()
    body = "".join(to_typst_source(child) for child in kids[1:]).strip()
    return f"terms.item([{term}], [{body}])"


def _runs(runs: tuple[RunNode, ...]) -> str:
    return "".join(_styled(run) for run in runs)


def _heading(level: int, runs: tuple[RunNode, ...]) -> str:
    return f"{'=' * min(max(level, 1), 6)} {_runs(runs)}\n"


def _image(asset_key: ContentKey, alt: AltText) -> str:
    source = _typst(asset_key.hex, TypstScope.STRING)
    equiv = f'"{_typst(alt, TypstScope.STRING)}"' if alt else "none"
    return f'image("{source}", alt: {equiv})'


def to_html(node: DocumentNode) -> str:
    # the tree -> HTML lowering the `document/emit#DOCUMENT PDF_HTML` weasyprint arm consumes; serialized
    # from the one escape-safe `_element` builder so a run carrying `<`/`&`/`"` produces valid markup,
    # never an f-string splice the TEMPLATE-SAFETY law rejects.
    return etree.tostring(_element(node), method="html", encoding="unicode")


def to_lxml_tree(node: DocumentNode) -> "_Element":
    # the tree -> lxml `_Element` lowering the `XML`/`XML_TRANSFORM`/`XML_VALIDATE`/`XML_QUERY` arms fold
    # through `etree.tostring`/`XSLT`/`XPath`; one builder serves both HTML and the XML object tree.
    return _element(node)


def to_c14n(node: DocumentNode) -> bytes:
    # the deterministic canonical-XML (Canonical XML 2.0) egress the journal/archival JATS-adjacent structured
    # interchange consumes: `method="c14n2"` fixes attribute order, namespace prefixes, and whitespace so two
    # structurally-identical trees serialize byte-identically, the archival counterpart to the `method="html"`
    # `to_html` presentation lowering the same `_element` builder feeds — a stable content key over the XML form.
    return etree.tostring(_element(node), method="c14n2")


def to_json(node: DocumentNode) -> bytes:
    # the structured-data interchange egress: the whole recursive tagged-union tree lowered to deterministic JSON
    # via the shared `msgspec.json` encoder (the `kind` tag round-tripping under `tag_field="kind"`), the JSON-LD/
    # JATS-adjacent structured publication lowering distinct from the msgpack `encode`/`to_corpus(BYTES)` byte forms
    # and NOT the deleted `RoleView`/`json.schema_components` introspection blob — a real interchange, not a schema view.
    return _JSON_ENCODER.encode(node)


def _wrapped(inner: "_Element", tag: str) -> "_Element":
    outer = etree.Element(tag)  # Exemption: the `lxml.etree` element builder is the platform-forced markup seam the template-safety law mandates
    outer.append(inner)
    return outer


def _run_element(run: RunNode) -> "_Element":
    inner = etree.Element("span")
    inner.text = run.text  # lxml escapes on serialize; never an f-string interpolation into markup
    decoration = " ".join(_DECORATION_CSS[deco] for deco in run.decorations)
    style = "; ".join(
        part
        for part in (
            f"color:rgb({run.color[0]},{run.color[1]},{run.color[2]})" if run.color != (0, 0, 0) else "",
            f"text-decoration:{decoration}" if decoration else "",
        )
        if part
    )
    if style:
        inner.set("style", style)
    if run.direction is TextDirection.RTL:
        inner.set("dir", "rtl")
    layers = (
        *(("sup",) if run.script is RunScript.SUPER else ("sub",) if run.script is RunScript.SUB else ()),
        *(("em",) if run.italic else ()),
        *(("strong",) if run.weight >= 700 else ()),
    )
    return reduce(_wrapped, layers, inner)


def _filled(element: "_Element", runs: tuple[RunNode, ...], kids: tuple[DocumentNode, ...] = ()) -> "_Element":
    for run in runs:  # Exemption: lxml element assembly is the platform markup builder, escape-safe by construction
        element.append(_run_element(run))
    for kid in kids:
        element.append(_element(kid))
    return element


def _element(node: DocumentNode) -> "_Element":
    match node:
        case RunNode():
            return _run_element(node)
        case BlockNode(block=BlockKind.HEADING, level=level, runs=runs):
            return _filled(etree.Element(f"h{min(max(level, 1), 6)}"), runs)
        case BlockNode(block=BlockKind.CODE, runs=runs):
            pre = etree.Element("pre")
            etree.SubElement(pre, "code").text = "".join(run.text for run in runs)
            return pre
        case BlockNode(block=block, runs=runs, children=kids):
            return _filled(etree.Element(_BLOCK_HTML[block]), runs, kids)
        case ListNode(list_kind=ListKind.DESCRIPTION, items=items):
            return _filled_terms(etree.Element("dl"), items)
        case ListNode(list_kind=kind, start=start, items=items):
            ordered = etree.Element(_LIST_HTML[kind])
            if kind is ListKind.ORDERED and start != 1:
                ordered.set("start", str(start))
            for item in items:
                etree.SubElement(ordered, "li").append(_element(item))
            return ordered
        case SectionNode(level=level, heading=head, children=kids):
            section = _filled(etree.Element("section"), (), kids)
            section.insert(0, _filled(etree.Element(f"h{min(max(level, 1), 6)}"), head))
            return section
        case PageNode(children=kids):
            page = _filled(etree.Element("div"), (), kids)
            page.set("class", "page")
            return page
        case StructureNode(children=kids):
            structured = _filled(etree.Element("div"), (), kids)
            structured.set("role", role_of(node))
            return structured
        case TableNode(rows=rows, spans=spans, header_rows=head_n, footer_rows=foot_n, caption=caption):
            return _table_element(rows, head_n, foot_n, _span_map(spans), caption)
        case FigureNode(asset_key=asset_key, alt=alt, caption=caption):
            figure = etree.Element("figure")
            image = etree.SubElement(figure, "img")
            image.set("src", asset_key.hex)
            image.set("alt", alt)  # the `AltStatus.ABSENT` empty string stays the audited fact, never invented
            return _filled(figure, caption) if caption else figure
        case FormulaNode(tex=tex, display=display, alt=alt):
            math = etree.Element("div" if display else "span")  # MathJax/KaTeX-delimited LaTeX — the journal-web math convention
            math.set("class", "math display" if display else "math")
            math.set("role", "math")
            if alt:
                math.set("aria-label", alt)  # the WCAG text equivalent for the `Formula` structure element
            math.text = (
                f"\\[{tex}\\]" if display else f"\\({tex}\\)"
            )  # lxml escapes `<`/`&`/`"` on serialize; the LaTeX body is never an f-string markup splice
            return math
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Uri(href=href)):
            return _anchor(href, text)
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Dest(page=page)):
            return _anchor(f"#page-{page + 1}", text)
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Xref() as xref):
            return _anchor(f"#{xref.cite()}", text)  # the sheet-set cross-reference fragment the imposition assembly resolves
        case FieldNode() | AnnotationNode():
            return etree.Element("span")  # a non-link annotation or form field carries no inline HTML body
        case _ as unreachable:
            assert_never(unreachable)


def _anchor(href: str, text: str) -> "_Element":
    anchor = etree.Element("a")
    anchor.set("href", href)
    anchor.text = text
    return anchor


def _filled_terms(dl: "_Element", items: tuple[DocumentNode, ...]) -> "_Element":
    for item in items:  # one `<dt>`/`<dd>` pair per description item, the head child the term and the tail the body
        kids = children(item)
        etree.SubElement(dl, "dt").append(_element(kids[0]) if kids else _element(item))
        _filled(etree.SubElement(dl, "dd"), (), kids[1:])
    return dl


def _table_element(
    rows: tuple[tuple[DocumentNode, ...], ...],
    head_n: int,
    foot_n: int,
    span_map: frozendict[tuple[int, int], tuple[int, int]],
    caption: tuple[RunNode, ...],
) -> "_Element":
    table = etree.Element("table")
    if caption:  # the PDF/UA `Caption` element is the first child of `<table>`
        _filled(etree.SubElement(table, "caption"), caption)
    body_end = len(rows) - foot_n
    bands = (("thead", rows[:head_n], "th", 0), ("tbody", rows[head_n:body_end], "td", head_n), ("tfoot", rows[body_end:], "td", body_end))
    for band_tag, band_rows, cell_tag, base in bands:
        if not band_rows:
            continue
        band = etree.SubElement(table, band_tag)
        for r, row in enumerate(band_rows):
            line = etree.SubElement(band, "tr")
            for c, cell in enumerate(row):
                span = span_map.get((base + r, c))
                td = etree.SubElement(line, cell_tag)
                if span and span[0] != 1:
                    td.set("colspan", str(span[0]))
                if span and span[1] != 1:
                    td.set("rowspan", str(span[1]))
                td.append(_element(cell))
    return table


def to_markdown(node: DocumentNode) -> str:
    # the tree -> CommonMark/GFM manuscript lowering the `document/emit#DOCUMENT MARKDOWN` arm encodes: the
    # plain-text diffable egress of the SAME bound tree the PDF/HTML/Typst arms lower, every interpolated
    # `RunNode.text`/heading/caption escaped through the `_MD_ESCAPE` maketrans (trusted-node input, the same
    # f-string-plus-escaper form `to_typst_source` holds) so a `*`/`_`/`[`/`|` never opens spurious markup,
    # the super/sub/underline/overline/colour appearance carried as GFM raw HTML CommonMark cannot express.
    match node:
        case RunNode():
            return _md_styled(node)
        case BlockNode(block=BlockKind.HEADING, level=level, runs=runs):
            return f"{'#' * min(max(level, 1), 6)} {_md_runs(runs)}\n\n"
        case BlockNode(block=BlockKind.CODE, runs=runs):
            return f"```\n{''.join(run.text for run in runs)}\n```\n\n"
        case BlockNode(block=BlockKind.QUOTE, runs=runs, children=kids):
            body = _md_runs(runs) + "".join(to_markdown(child) for child in kids)
            return "".join(f"> {line}\n" for line in (body.splitlines() or [""])) + "\n"
        case BlockNode(runs=runs, children=kids):
            return _md_runs(runs) + "".join(to_markdown(child) for child in kids) + "\n\n"
        case ListNode(list_kind=ListKind.DESCRIPTION, items=items):
            return "".join(_md_term(item) for item in items) + "\n"
        case ListNode(list_kind=ListKind.ORDERED, start=start, items=items):
            return "".join(f"{start + index}. {to_markdown(item).strip()}\n" for index, item in enumerate(items)) + "\n"
        case ListNode(items=items):
            return "".join(f"- {to_markdown(item).strip()}\n" for item in items) + "\n"
        case SectionNode(level=level, heading=head, children=kids):
            return f"{'#' * min(max(level, 1), 6)} {_md_runs(head)}\n\n" + "".join(to_markdown(child) for child in kids)
        case PageNode(children=kids):
            return "".join(to_markdown(child) for child in kids)
        case StructureNode(children=kids):
            return "".join(to_markdown(child) for child in kids)
        case TableNode(rows=rows, header_rows=head_n, caption=caption):
            return _md_table(rows, head_n, caption)
        case FigureNode(asset_key=asset_key, alt=alt, caption=caption):
            figure = f"![{_md(alt)}]({asset_key.hex})\n"
            return f"{figure}\n{_md_runs(caption)}\n\n" if caption else f"{figure}\n"
        case FormulaNode(tex=tex, display=display):
            return f"$$\n{tex}\n$$\n\n" if display else f"${tex}$"  # GFM/Pandoc math; the LaTeX island is verbatim, never `_md`-escaped
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Uri(href=href)):
            return f"[{_md(text)}]({href})"
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Dest(page=page)):
            return f"[{_md(text)}](#page-{page + 1})"
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Xref() as xref):
            return f"[{_md(text)}](#{xref.cite()})"
        case FieldNode() | AnnotationNode():
            return ""
        case _ as unreachable:
            assert_never(unreachable)


def to_latex(node: DocumentNode) -> str:
    # the tree -> LaTeX manuscript lowering the `document/emit#DOCUMENT LATEX` arm encodes: the journal-submission
    # egress of the SAME bound tree, every interpolated `RunNode.text`/heading/caption escaped through the
    # `_LATEX_ESCAPE` maketrans so a `&`/`%`/`$`/`_`/`#`/`{`/`}`/`~`/`^`/`\` never breaks the source, the section
    # depth keyed by `_LATEX_SECTION` and the `hyperref`/`graphicx`/`ulem` control words the emit-side preamble carries.
    match node:
        case RunNode():
            return _latex_styled(node)
        case BlockNode(block=BlockKind.HEADING, level=level, runs=runs):
            return f"\\{_LATEX_SECTION[min(max(level, 1), 6)]}{{{_latex_runs(runs)}}}\n\n"
        case BlockNode(block=BlockKind.CODE, runs=runs):
            return f"\\begin{{verbatim}}\n{''.join(run.text for run in runs)}\n\\end{{verbatim}}\n\n"
        case BlockNode(block=BlockKind.QUOTE, runs=runs, children=kids):
            return f"\\begin{{quote}}\n{_latex_runs(runs)}{''.join(to_latex(child) for child in kids)}\n\\end{{quote}}\n\n"
        case BlockNode(runs=runs, children=kids):
            return _latex_runs(runs) + "".join(to_latex(child) for child in kids) + "\n\n"
        case ListNode(list_kind=ListKind.DESCRIPTION, items=items):
            return f"\\begin{{description}}\n{''.join(_latex_term(item) for item in items)}\\end{{description}}\n\n"
        case ListNode(list_kind=ListKind.ORDERED, start=start, items=items):
            counter = f"\\setcounter{{enumi}}{{{start - 1}}}\n" if start != 1 else ""
            return f"\\begin{{enumerate}}\n{counter}{_latex_items(items)}\\end{{enumerate}}\n\n"
        case ListNode(items=items):
            return f"\\begin{{itemize}}\n{_latex_items(items)}\\end{{itemize}}\n\n"
        case SectionNode(level=level, heading=head, children=kids):
            return f"\\{_LATEX_SECTION[min(max(level, 1), 6)]}{{{_latex_runs(head)}}}\n\n" + "".join(to_latex(child) for child in kids)
        case PageNode(children=kids):
            return "".join(to_latex(child) for child in kids) + "\\clearpage\n"
        case StructureNode(children=kids):
            return "".join(to_latex(child) for child in kids)
        case TableNode(rows=rows, spans=spans, header_rows=head_n, footer_rows=foot_n, caption=caption):
            return _latex_table(rows, head_n, foot_n, _span_map(spans), caption)
        case FigureNode(asset_key=asset_key, alt=alt, caption=caption):
            cap = f"\\caption{{{_latex_runs(caption)}}}\n" if caption else ""
            note = f"% alt: {_latex(alt)}\n" if alt else ""  # the alt equivalent rides a source comment — LaTeX carries no figure `alt` slot
            return f"\\begin{{figure}}\n\\centering\n{note}\\includegraphics{{{asset_key.hex}}}\n{cap}\\end{{figure}}\n\n"
        case FormulaNode(tex=tex, display=display):
            return (
                f"\\[\n{tex}\n\\]\n\n" if display else f"${tex}$"
            )  # native LaTeX math, the source verbatim (escaping the `tex` island would corrupt the math)
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Uri(href=href)):
            return f"\\href{{{href}}}{{{_latex(text)}}}"
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Dest(page=page)):
            return f"\\hyperlink{{page-{page + 1}}}{{{_latex(text)}}}"
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Xref() as xref):
            return f"\\hyperref[{xref.cite()}]{{{_latex(text)}}}"  # the sheet-set cross-reference the imposition assembly resolves
        case FieldNode() | AnnotationNode():
            return ""
        case _ as unreachable:
            assert_never(unreachable)


def _md(value: str) -> str:
    return value.translate(_MD_ESCAPE)


def _md_styled(run: RunNode) -> str:
    body = pipe(
        _md(run.text),
        lambda b: f"**{b}**" if run.weight >= 700 else b,
        lambda b: f"*{b}*" if run.italic else b,
        lambda b: f"<sup>{b}</sup>" if run.script is RunScript.SUPER else f"<sub>{b}</sub>" if run.script is RunScript.SUB else b,
        lambda b: reduce(lambda inner, deco: f"{_MD_DECORATION[deco][0]}{inner}{_MD_DECORATION[deco][1]}", run.decorations, b),
    )
    return body if run.color == (0, 0, 0) else f'<span style="color:rgb({run.color[0]},{run.color[1]},{run.color[2]})">{body}</span>'


def _md_runs(runs: tuple[RunNode, ...]) -> str:
    return "".join(_md_styled(run) for run in runs)


def _md_term(item: DocumentNode) -> str:
    # the pandoc description-list spelling: a bold term line then a `: `-prefixed body the head/tail child split feeds
    kids = children(item)
    term = to_markdown(kids[0]).strip() if kids else to_markdown(item).strip()
    body = "".join(to_markdown(child) for child in kids[1:]).strip()
    return f"**{term}**\n: {body}\n"


def _md_table(rows: tuple[tuple[DocumentNode, ...], ...], header_rows: int, caption: tuple[RunNode, ...]) -> str:
    # a GFM pipe table — the leading `header_rows or 1` rows form the header band above the `---` delimiter,
    # merged cells flattening to their top-left content (GFM carries no colspan/rowspan), the `caption` a
    # titling paragraph below; a cell's own newlines/pipes are neutralized so one logical row stays one line.
    if not rows:
        return ""
    width = max(len(row) for row in rows)
    head_n = header_rows or 1
    lines = (
        *(f"| {' | '.join(_md_cell(row, col) for col in range(width))} |" for row in rows[:head_n]),
        f"| {' | '.join('---' for _ in range(width))} |",
        *(f"| {' | '.join(_md_cell(row, col) for col in range(width))} |" for row in rows[head_n:]),
    )
    table = "\n".join(lines) + "\n"
    return f"{table}\n{_md_runs(caption)}\n\n" if caption else f"{table}\n"


def _md_cell(row: tuple[DocumentNode, ...], col: int) -> str:
    return to_markdown(row[col]).strip().replace("\n", " ").replace("|", "\\|") if col < len(row) else ""


def _latex(value: str) -> str:
    return value.translate(_LATEX_ESCAPE)


def _latex_styled(run: RunNode) -> str:
    body = pipe(
        _latex(run.text),
        lambda b: f"\\textbf{{{b}}}" if run.weight >= 700 else b,
        lambda b: f"\\textit{{{b}}}" if run.italic else b,
        lambda b: f"\\textsuperscript{{{b}}}" if run.script is RunScript.SUPER else f"\\textsubscript{{{b}}}" if run.script is RunScript.SUB else b,
        lambda b: reduce(lambda inner, deco: f"\\{_LATEX_DECORATION[deco]}{{{inner}}}", run.decorations, b),
    )
    return body if run.color == (0, 0, 0) else f"\\textcolor[RGB]{{{run.color[0]},{run.color[1]},{run.color[2]}}}{{{body}}}"


def _latex_runs(runs: tuple[RunNode, ...]) -> str:
    return "".join(_latex_styled(run) for run in runs)


def _latex_items(items: tuple[DocumentNode, ...]) -> str:
    return "".join(f"\\item {to_latex(item).strip()}\n" for item in items)


def _latex_term(item: DocumentNode) -> str:
    kids = children(item)
    term = to_latex(kids[0]).strip() if kids else to_latex(item).strip()
    body = "".join(to_latex(child) for child in kids[1:]).strip()
    return f"\\item[{term}] {body}\n"


def _latex_table(
    rows: tuple[tuple[DocumentNode, ...], ...],
    head_n: int,
    foot_n: int,
    span_map: frozendict[tuple[int, int], tuple[int, int]],
    caption: tuple[RunNode, ...],
) -> str:
    # a `tabular` inside a `table` float — the column spec folds row 0's colspans to the grid width, an `\hline`
    # rules the header/footer band boundaries (the `head_n`/`foot_n` counts), a colspan cell rides `\multicolumn`
    # (rowspan flattens to the top-left cell, `\multirow` an emit-preamble growth axis), the `caption` titling the float.
    if not rows:
        return ""
    width = _column_count(rows, span_map)
    spec = "|" + "l|" * width
    boundaries = frozenset(edge for edge in (head_n, len(rows) - foot_n, len(rows)) if 0 < edge <= len(rows))
    lines = "".join(_latex_row(row, r, span_map) + ("\\hline\n" if r + 1 in boundaries else "") for r, row in enumerate(rows))
    cap = f"\\caption{{{_latex_runs(caption)}}}\n" if caption else ""
    return f"\\begin{{table}}\n\\centering\n{cap}\\begin{{tabular}}{{{spec}}}\n\\hline\n{lines}\\end{{tabular}}\n\\end{{table}}\n\n"


def _latex_row(row: tuple[DocumentNode, ...], r: int, span_map: frozendict[tuple[int, int], tuple[int, int]]) -> str:
    return " & ".join(_latex_cell(row[c], r, c, span_map) for c in range(len(row))) + " \\\\\n"


def _latex_cell(cell: DocumentNode, r: int, c: int, span_map: frozendict[tuple[int, int], tuple[int, int]]) -> str:
    content = to_latex(cell).strip().replace("\n", " ")
    span = span_map.get((r, c))
    return f"\\multicolumn{{{span[0]}}}{{|l|}}{{{content}}}" if span and span[0] != 1 else content
```

## [03]-[DELTA]

- Owner: `DocumentDelta` the one diff/merge edit algebra — four `msgspec.Struct` variants (`Inserted`/`Deleted`/`Moved`/`Reparametrized`) under one `tag`-discriminated `Union`, every edit keyed by the stable `NodeMeta.key` of the node it acts on. `diff` and `merge` are defined once over the tree as one total `expression` `Map`/`Block` fold; the same algebra a structural object-graph diff reuses lives here, never re-minted per consumer.
- Cases: `Inserted` (a new node + the parent key + position) · `Deleted` (the removed node's key) · `Moved` (a node key + the new parent key + new position) · `Reparametrized` (a node key + the field-name→`Raw`-value map of changed own-content fields, the in-place edit a re-styled run or re-bounded figure produces). Each a frozen `Struct` variant keyed by `ContentKey`; the edit set is the patch a `produce → extract → re-produce` round-trip and a privacy-redaction pass both emit.
- Entry: `diff(before, after)` folds the two trees keyed by each node's stable `NodeMeta.key` into an ordered `tuple[DocumentDelta, ...]` — a key present only in `after` (whose parent already existed) is an `Inserted`, only in `before` (whose parent survives) a `Deleted`, present in both under a different parent/index a `Moved`, present in both with a changed own-content payload a `Reparametrized`; `merge(tree, deltas)` folds the patch back over the tree returning the patched `DocumentNode`; `invert(before, deltas)` maps each edit to its inverse so a redaction patch is reversible until burned in. Every arm a total `match`; the patch round-trips through `msgspec.msgpack` so a corpus diff is a content-keyed serialized value.
- Auto: `_index` builds one `Map[Path, IndexEntry]` over the STRUCTURAL `_spine` (the `children`-field child sequence the containers own — `PageNode`/`SectionNode`/`BlockNode`/`StructureNode`/`ListNode`), keying each node by its structural PATH-vector (the child-ordinal sequence from the root, the `boundaries.md` MEMO_KEY structural uid) so two identical-content siblings stay distinct slots where a content-derived `NodeMeta.key` would silently overwrite one; `_identities` derives the `Map[ContentKey, Path]` so move/reparametrize detection keys on the position-stable `NodeMeta.key` (NEVER by `node_digest`, whose Merkle fold re-keys every ancestor when a descendant changes and would spuriously `Moved` every sibling of an edit) while the path keying keeps the structural index collision-free — two distinct keyings, never conflated. `diff` reads the two `Map`s through `try_find` and folds the key-set algebra into the edit `Block`: a node whose key is new and whose parent already existed is the topmost `Inserted` of its subtree (a node under an also-new parent is carried inside that subtree and emits nothing), the symmetric topmost survivor is the `Deleted`, a surviving key under a changed parent/index is `Moved`, and a surviving key whose OWN content (every field except the structural `children` — including a `BlockNode`'s `runs`, a `SectionNode`'s `heading`, a `FigureNode`'s `caption`, a `TableNode`'s `rows`) differs is `Reparametrized`; the casualty set is one `Block.choose` over the survivor `try_find`, never a `list.append` accumulator. `merge` reduces the edit `Block` over the tree through `Block.fold`, `Inserted`/`Deleted` re-splicing a parent's `children` spine, `Moved` re-parenting under the new key, `Reparametrized` overlaying the decoded own-field map through `msgspec.convert` — one immutable fold, no in-place mutation.
- Receipt: the delta count and the changed-node keys ride the lens introspection receipt facts; `DocumentDelta` mints no receipt of its own.
- Packages: `msgspec` (`Struct(frozen=True, tag=True, tag_field=...)` edit variants, `Union` alias, `Raw` the opaque own-field values, `structs.replace` for the spine re-splice, `msgpack` round-trip, `convert` the `Reparametrized` re-coerce, `to_builtins` the overlay base); `expression` (`Map.empty`/`add`/`try_find` the structural index, `Block.of_seq`/`fold`/`choose` the immutable edit traversal, `Option` the lookup rail); runtime (`content_identity.ContentKey` keying every edit, consumed never re-minted).
- Growth: a new edit kind is one `DocumentDelta` variant plus one `diff` emit arm and one `merge` apply arm; the totality `match` forces both. A new diff granularity is a `node_digest` policy change, never a parallel delta family.
- Boundary: a per-consumer diff type (a document diff beside a geometry diff beside a wire diff) is the deleted form — `DocumentDelta` is the one edit algebra keyed by `ContentKey`. No mutation, no positional list patching by index-shift heuristics outside the key algebra, no second merge owner, no `list.append`/`dict` procedural accumulator where the `expression` `Map`/`Block` fold states the traversal. Structural insertion/deletion/move targets the spine containers that own a `children` field through `_spine`/`_with_spine`; a `TableNode` cell grid, a `FigureNode` caption, a `SectionNode` heading, a `ListNode` item bag, and a `BlockNode` inline-run bag are bounded OWN-content sub-payloads re-keyed as a whole through `Reparametrized`, so `_spine` carries only the container `children` field and the sub-payload edits ride the own-field overlay. The fold is total over the four-variant union; a missing arm is an `assert_never` static failure.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from typing import Final, assert_never

import msgspec
from expression import Nothing, Option, Some
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.content_identity import ContentKey

from .model import BlockNode, DocumentNode, ListNode, PageNode, SectionNode, StructureNode, walk

# --- [TYPES] ----------------------------------------------------------------------------

type Path = tuple[int, ...]  # the child-ordinal vector from the root: a node's structural uid
type IndexEntry = tuple[DocumentNode, ContentKey | None, int]

# --- [MODELS] ---------------------------------------------------------------------------


class Inserted(Struct, frozen=True, tag="inserted", tag_field="edit"):
    parent: ContentKey
    index: int
    node: DocumentNode


class Deleted(Struct, frozen=True, tag="deleted", tag_field="edit"):
    key: ContentKey


class Moved(Struct, frozen=True, tag="moved", tag_field="edit"):
    key: ContentKey
    parent: ContentKey
    index: int


class Reparametrized(Struct, frozen=True, tag="reparametrized", tag_field="edit"):
    key: ContentKey
    fields: dict[str, msgspec.Raw]


type DocumentDelta = Inserted | Deleted | Moved | Reparametrized

# --- [CONSTANTS] ------------------------------------------------------------------------

_DELTA_ENCODER: Final = msgspec.msgpack.Encoder(order="deterministic")
_DELTA_DECODER: Final = msgspec.msgpack.Decoder(tuple[DocumentDelta, ...])

# --- [OPERATIONS] -----------------------------------------------------------------------


def _spine(node: DocumentNode) -> tuple[DocumentNode, ...]:
    match node:
        case PageNode(children=kids) | StructureNode(children=kids) | SectionNode(children=kids) | BlockNode(children=kids):
            return kids
        case ListNode(items=items):
            return items
        case _:
            return ()


def _with_spine(node: DocumentNode, kids: tuple[DocumentNode, ...]) -> DocumentNode:
    match node:
        case PageNode() | StructureNode() | SectionNode() | BlockNode():
            return msgspec.structs.replace(node, children=kids)
        case ListNode():
            return msgspec.structs.replace(node, items=kids)
        case _:
            return node


def _own(node: DocumentNode, /) -> tuple[tuple[str, object], ...]:
    return tuple((name, getattr(node, name)) for name in node.__struct_fields__ if name not in {"children", "items"})


def _index(root: DocumentNode, /) -> Map[Path, IndexEntry]:
    # keyed by the structural path-vector so two identical-content siblings never collide on a content-derived
    # `NodeMeta.key`; the entry carries the parent `NodeMeta.key` and ordinal the ContentKey-addressed deltas need.
    def walk_spine(table: Map[Path, IndexEntry], node: DocumentNode, path: Path, parent: ContentKey | None, position: int) -> Map[Path, IndexEntry]:
        seeded = table.add(path, (node, parent, position))
        return Block.of_seq(enumerate(_spine(node))).fold(
            lambda acc, pair: walk_spine(acc, pair[1], (*path, pair[0]), node.meta.key, pair[0]), seeded
        )

    return walk_spine(Map.empty(), root, (), None, 0)


def _identities(index: Map[Path, IndexEntry], /) -> Map[ContentKey, Path]:
    # `NodeMeta.key` -> structural path: move/reparametrize detection keys on the stable identity that survives
    # a position change, while the path-keyed index keeps siblings distinct — the two keyings never conflated.
    return Block.of_seq(index.items()).fold(lambda acc, item: acc.add(item[1][0].meta.key, item[0]), Map.empty())


def _by_key(index: Map[Path, IndexEntry], /) -> Map[ContentKey, IndexEntry]:
    return Block.of_seq(index.items()).fold(lambda acc, item: acc.add(item[1][0].meta.key, item[1]), Map.empty())


def diff(before: DocumentNode, after: DocumentNode, /) -> tuple[DocumentDelta, ...]:
    old, new = _index(before), _index(after)
    old_at, new_at = _identities(old), _identities(new)

    def survived(node: DocumentNode, key: ContentKey, parent: ContentKey, index: int, prior: IndexEntry) -> Block[DocumentDelta]:
        node_prior, parent_prior, index_prior = prior
        moved = (Moved(key=key, parent=parent, index=index),) if (parent_prior, index_prior) != (parent, index) else ()
        changed = (Reparametrized(key=key, fields=_field_delta(node_prior, node)),) if _own(node_prior) != _own(node) else ()
        return Block.of_seq((*moved, *changed))

    def edits_for(item: tuple[Path, IndexEntry]) -> Block[DocumentDelta]:
        _path, (node, parent, index) = item
        if parent is None:
            return Block.empty()
        key = node.meta.key
        return (
            old_at
            .try_find(key)
            .bind(old.try_find)
            .map(lambda prior: survived(node, key, parent, index, prior))
            .default_with(
                lambda: Block.singleton(Inserted(parent=parent, index=index, node=node)) if old_at.try_find(parent).is_some() else Block.empty()
            )
        )

    def deletes_for(item: tuple[Path, IndexEntry]) -> Option[DocumentDelta]:
        _path, (node, parent, _index) = item
        gone = parent is not None and new_at.try_find(node.meta.key).is_none() and new_at.try_find(parent).is_some()
        return Some(Deleted(key=node.meta.key)) if gone else Nothing

    inserts_moves = Block.of_seq(new.items()).collect(edits_for)
    deletes = Block.of_seq(old.items()).choose(deletes_for)
    return tuple(inserts_moves) + tuple(deletes)


def merge(tree: DocumentNode, deltas: tuple[DocumentDelta, ...], /) -> DocumentNode:
    def apply(patched: DocumentNode, delta: DocumentDelta) -> DocumentNode:
        match delta:
            case Inserted(parent=parent, index=index, node=node):
                return _splice(patched, parent, index, node)
            case Deleted(key=key):
                return _prune(patched, key)
            case Moved(key=key, parent=parent, index=index):
                return _splice(_prune(patched, key), parent, index, _find(tree, key))
            case Reparametrized(key=key, fields=fields):
                return _retarget(patched, key, lambda node: _apply_fields(node, fields))
            case _ as unreachable:
                assert_never(unreachable)

    return Block.of_seq(deltas).fold(apply, tree)


def invert(before: DocumentNode, deltas: tuple[DocumentDelta, ...], /) -> tuple[DocumentDelta, ...]:
    by_key = _by_key(_index(before))
    return tuple(Block.of_seq(deltas).map(lambda delta: _invert(delta, by_key)))[::-1]


def encode(deltas: tuple[DocumentDelta, ...]) -> bytes:
    return _DELTA_ENCODER.encode(deltas)


def decode(payload: bytes) -> tuple[DocumentDelta, ...]:
    return _DELTA_DECODER.decode(payload)


def _invert(delta: DocumentDelta, old: Map[ContentKey, IndexEntry], /) -> DocumentDelta:
    match delta:
        case Inserted(node=node):
            return Deleted(key=node.meta.key)
        case Deleted(key=key):
            return old.try_find(key).map(lambda e: Inserted(parent=e[1], index=e[2], node=e[0]) if e[1] is not None else delta).default_value(delta)
        case Moved(key=key):
            return old.try_find(key).map(lambda e: Moved(key=key, parent=e[1], index=e[2]) if e[1] is not None else delta).default_value(delta)
        case Reparametrized(key=key):
            return old.try_find(key).map(lambda e: Reparametrized(key=key, fields=_all_fields(e[0]))).default_value(delta)
        case _ as unreachable:
            assert_never(unreachable)


def _field_delta(prior: DocumentNode, current: DocumentNode, /) -> dict[str, msgspec.Raw]:
    prior_fields = dict(_own(prior))
    return {name: msgspec.Raw(_DELTA_ENCODER.encode(value)) for name, value in _own(current) if prior_fields.get(name) != value}


def _all_fields(node: DocumentNode, /) -> dict[str, msgspec.Raw]:
    return {name: msgspec.Raw(_DELTA_ENCODER.encode(value)) for name, value in _own(node)}


def _apply_fields(node: DocumentNode, fields: dict[str, msgspec.Raw], /) -> DocumentNode:
    merged = {**msgspec.to_builtins(node), **{name: msgspec.msgpack.decode(raw) for name, raw in fields.items()}}
    return msgspec.convert(merged, type(node))


def _splice(tree: DocumentNode, parent: ContentKey, index: int, node: DocumentNode, /) -> DocumentNode:
    return _retarget(tree, parent, lambda target: _with_spine(target, (*_spine(target)[:index], node, *_spine(target)[index:])))


def _prune(tree: DocumentNode, key: ContentKey, /) -> DocumentNode:
    kids = _spine(tree)
    if any(child.meta.key == key for child in kids):
        return _with_spine(tree, tuple(child for child in kids if child.meta.key != key))
    return _with_spine(tree, tuple(_prune(child, key) for child in kids)) if kids else tree


def _retarget(tree: DocumentNode, key: ContentKey, fn: Callable[[DocumentNode], DocumentNode], /) -> DocumentNode:
    if tree.meta.key == key:
        return fn(tree)
    kids = _spine(tree)
    return _with_spine(tree, tuple(_retarget(child, key, fn) for child in kids)) if kids else tree


def _find(tree: DocumentNode, key: ContentKey, /) -> DocumentNode:
    return next(node for node in walk(tree) if node.meta.key == key)
```

## [04]-[RESEARCH]

- [DIGEST_VS_IDENTITY]: `node_digest` is the Merkle CONTENT fold — a leaf keys over its serialized bytes and an interior node folds its child digests through `ContentIdentity.of(tuple_of_child_keys)`, so any descendant edit re-keys every ancestor digest. That re-keying is exactly why the diff does NOT key by `node_digest`: an unstable parent reference would spuriously `Moved` every sibling of an inserted node and break `merge`. The diff INDEXES by the structural path-vector (the `Path` child-ordinal sequence from the root, the `boundaries.md` MEMO_KEY structural uid) so two identical-content siblings under one parent stay distinct slots where a content-derived `NodeMeta.key` would overwrite one, and `_identities` derives the `Map[ContentKey, Path]` so move/reparametrize detection matches the position-stable `NodeMeta.key` minted once per node at authoring/recovery time, an edit at one node never perturbing another's path. `node_digest` serves the cache-hit-by-reference and corpus-residency identity (a content-identical sub-tree keys identically for reuse elision) through an iterative expand/combine frontier that never overflows the frame limit on an adversarial-depth tree; the diff indexes by structural path and matches move identity by `NodeMeta.key`, two distinct keyings never conflated, both reaching `ContentIdentity.of` — the `merkle` arm over child `ContentKey`s for the digest, the `whole` arm over `msgpack` bytes for the corpus.
- [STRUCT_ROLE_TOTALITY]: the PDF/UA structure-type vocabulary is closed except at one extension point, so `StructRole` is a two-arm tagged `Union` under `tag_field="role_kind"` — `StandardRole` wrapping the `StructEltKind` `StrEnum` of the full ISO 14289 standard-structure roles (the grouping `Art`/`Div`/`TOC`/`TOCI`/`Index`/`NonStruct`/`Private` beside `Document`/`Part`/`Sect`, the block `BlockQuote`/`BibEntry` beside `P`/`Note`/`Code`/`Caption`, the inline `Span`/`Reference`/`Annot` and the East-Asian ruby `Ruby`/`RB`/`RT`/`RP` and warichu `Warichu`/`WT`/`WP` assemblies beside `Quote`/`Link`, the `Lbl`/`LBody` list-item structure beside `L`/`LI`, the `THead`/`TBody`/`TFoot` table sections beside `TR`/`TH`/`TD`, and the `Formula`/`Form` illustration roles beside `Figure` — the full ISO 14289 standard-structure-type table, the CJK ruby/warichu inline assemblies the book-typesetting publication plane and the AEC-documentation plane both demand and the `NonStruct`/`Private` generic grouping roles PDF/UA mandates now landed rather than deferred), and `ForeignRole` carrying the one `ForeignRoleStr` escape. A standard role is a `StructEltKind` member (a `StrEnum` row, never a sibling struct), so a new standard role is one enum line plus one `_STRUCT_CATEGORY` row and `role_of`'s `StandardRole` arm plus `role_category`'s table lookup absorb it; a foreign role is the single `ForeignRole` arm `match` reaches, categorized as the named `_FOREIGN_CATEGORY` open-default row. The `StructEltKind` value strings are the literal PDF/UA standard-structure-type names, the one literal source on the page that traces to the external standard vocabulary; `ForeignRoleStr` constrains the foreign role to a non-empty `[A-Za-z][\w.\-]*` identifier through `Meta(min_length, max_length, pattern)`, validated on decode and on the `Reparametrized` re-coerce through `msgspec.convert`. The PDF/UA vocabulary is not bare: `_STRUCT_CATEGORY` is the one frozen `frozendict` behavior table carrying each role's `(StructCategory, heading_level)`, the smart-enum behavior-row collapse the bare `StrEnum` alone defeats, and `_STANDARD_FOR` is the one DERIVED secondary inverting category to its canonical standard role (the inline canonical `Span`, the foreign-default grouping `Sect`), exposed through the `standard_for` projection so the `folder:document/tagged#ACCESS` `/RoleMap` lowering reads a model projection rather than the hand-kept `_STANDARD_FOR` dict that page formerly owned — the `DERIVED_LOGIC` law applied across the model/tagged seam, one primary correspondence declared and the foreign-mapping secondary derived from it. The deleted `RoleView`/`role_contract`/`_ROLE_CONTRACT` machinery (the `json.schema_components((StructRole,), ...)` `(components, defs)` blob, the `inspect.multi_type_info((StructRole,))` node tree, the `structs.fields(FigureNode)` `FieldInfo` set) asserted three "audit views" no consumer reads — the `folder:document/tagged#ACCESS` AUDIT folds `role_of`/`role_category`/`alt_of`/`children` over the authored `pikepdf` tree and never reaches a `msgspec`-introspection blob, so the three views were decorative density (a confident-looking `RoleView`-keyed dispatch carrying no real capability) and are removed; the closed-family totality is proved by the `assert_never` over `StructRole`, not by a schema-component map nothing consumes. The nested `role_kind` discriminant is independent of the outer `kind` discriminant: `StructureNode` decodes its `kind="structure"` tag first, then `role` decodes its own `role_kind` tag, two tag fields on two `Union` levels that never collide because they name distinct fields. The `kind` tag the corpus column reads is recovered from `node.__struct_config__.tag` (the `msgspec.structs.StructConfig` runtime view), never a parallel `match` re-enumerating the ten variants to recover a literal the tag already carries.
- [ACCESSIBILITY_DOMAIN]: the node algebra carries the full PDF/UA + reflow accessibility surface the bare id/role/page slice omitted, each addition citing a real consumer or package member. `NodeMeta.lang` is the `/Lang` BCP-47 tag the PDF/UA structure tree and the `document/report#REPORT` language seam require (the sibling `report.md`/`lens.md` `language` references), typed `LangTag | UnsetType = UNSET` so an untagged node round-trips absent under `omit_defaults` and a tagged node carries the validated tag; `NodeMeta.actual_text` is the `/ActualText` glyph-replacement the PDF/UA spec mandates for ligatures and non-textual glyphs, the same `UnsetType` tri-state. `RunNode` carries `italic`/`script`/`decorations`/`direction`/`color` so a styled run round-trips its full character appearance — the `folder:../typography/shape#SHAPE` shaping surface authors them and the `_styled` `pipe` fold reads every one through `#emph`/`#super`/`#sub`/the `_DECORATION_MARKUP`-rowed `#underline`/`#strike`/`#overline`/`#text(dir: rtl)`/`#text(rgb(..))` markup. The prior `rtl: bool` was the illusory case: stored and round-tripped yet never read by `_styled`, so a right-to-left run emitted no direction markup at all; `direction: TextDirection` (`LTR`/`RTL`/`AUTO`, the python-bidi `base_dir` `'L'`/`'R'`/auto vocabulary the `get_base_level` probe resolves to `0`/`1`) replaces the dead flag with a tri-state the lowering honors and the shaper reorders, and `decorations: tuple[TextDecoration, ...]` adds the underline/strike/overline set the bare weight/italic slice dropped — a run that was bold-italic-superscript-underlined-red lowered to plain bold before. `ListNode` is the distinct `L`/`LI`/`Lbl`/`LBody` PDF/UA list grouping the lens recovers and the `#list`/`#enum(start:)`/`#terms` `_LIST_MARKUP`-rowed Typst markup lowers — the ordered-list `start` ordinal riding `#enum(start:)`, the description list folding each item to one `terms.item([term], [body])` pair — where the prior tree forced a list through a `BlockKind.LIST_ITEM` block that carried no ordered/unordered/description distinction, no start ordinal, and no list-level grouping, a real structural concept the audit's `L → LI` nesting check needs. `TableNode.header_rows`/`footer_rows` are the leading-`THead`-row and trailing-`TFoot`-row counts the pymupdf `Table.header` member recovers (verified in `.api/pymupdf.md`) and PDF/UA distinguishes from `TD` data cells; `to_typst_source` lowers them through the Typst `table.header(repeat: true, ..)`/`table.footer(..)` row-band elements, replacing the invalid `table.header.repeat` dict key the prior arm emitted. `TableNode.spans` is the merged-cell `(row, present-cell index, col_span, row_span)` quad set the `_span_map` lookup drives both lowerings from — Typst `table.cell(colspan:, rowspan:)` with `_column_count` folding the grid width off row 0's colspans, HTML `<td colspan rowspan>` — closing the prior illusory-field defect where `spans` was declared, round-tripped, and digested yet NEITHER backend read it, so an AEC schedule's merged header lowered to a flat unmerged grid; a merged cell is now the same honored concept a `rtl` flag would be if the lowering ignored `direction`. `TableNode.caption` is the run sequence lowering to the Typst `#figure(kind: table, caption: [..])` wrapper over the plain `table(..)` call form and the HTML `<caption>` first-child — the PDF/UA `Caption` element (a legal `Table` child) and the "Table N: …"/"DOOR SCHEDULE" title both a journal-grade publication table and an AEC schedule carry, the sibling of the `FigureNode.caption` the illustration owner already held. `BlockKind.QUOTE`/`CODE` now lower to `#quote(block: true)[..]`/`#raw("..", block: true)` rather than the prior generic-paragraph collapse that declared the kinds yet honored only `HEADING`/`ARTIFACT`. `FigureNode.media_type`/`intrinsic` carry the `MediaType` MIME and intrinsic `(width, height)` the `pikepdf`/`pymupdf` image extraction surfaces (`PdfImage.mode`/`colorspace`, `Pixmap` dimensions) recover and a faithful re-emission needs. `FieldNode.flags`/`options` carry the `FieldFlag` set (required/readonly/multiline/password) and `CHOICE` candidate values the `pymupdf Page.widgets()` recovery (`field_type`/`field_value` plus flags) and the `document/emit#DOCUMENT` form authoring read. `AnnotationNode.link` is the `AnnotTarget` closed family — external `Uri`, internal `Dest` page destination, `NoTarget` — the `LINK`-kind annotation needs so a hyperlink round-trips its href or page jump rather than dropping the target (the sibling `uri` references in `emit.md`/`egress.md`/`report.md`/`lens.md`); `to_typst_source` lowers the `Uri` case through `#link("href")[text]` and the internal `Dest` case through `#link((page:, x:, y:))[text]` (the Typst location-dictionary destination carrying the `Dest.point`), so a recovered page-jump round-trips its target rather than dropping it. `BlockKind.ARTIFACT` is the decorative-content case lowering through Typst `pdf.artifact[..]` (verified `.api/typst.md` `[MARKUP_ELEMENT_SCOPE]` `pdf.artifact`), so a rule/ornament is excluded from the tagged structure tree per the PDF/UA artifact rule rather than mis-tagged as content. Every addition is one field/case on an existing owner reshaping it as if it had always carried the concept, never a parallel surface.
- [ALT_TEXT_PRESENCE]: `FigureNode.alt: AltText` is the alt-text-presence fact the `document/tagged#ACCESS` AUDIT verifies and the `composition/compose#COMPOSE` unit authors, `AltText` an `Annotated[str, Meta(max_length=2048)]` admitting the empty string as the default (an un-authored figure carries `alt=""`, the audit's failing case) while bounding the upper length on decode. `to_corpus(node, STRUCT)` projects the `CorpusRow.alt`/`alt_status` columns from the one `alt_of` `(AltText, AltStatus)` pair so the `FigureNode` discriminant fires once per row, and the audit reads alt-text presence as the single `kind == FIGURE and alt_status == ABSENT` column predicate over the content-keyed corpus rather than re-walking the tree; `to_typst_source` folds the `FigureNode` through the `_image` emitter that owns the `alt: none | str` decision — a non-empty `alt` writes the string-context-escaped `image(source, alt: "<escaped>")`, an empty `alt` writes `image(source, alt: none)`, so the absent case stays `none` rather than collapsing to an empty-string equivalent that asserts a meaningless image, the Typst compiler emits the equivalent into the marked-content figure structure element of the PDF/UA render, and the future `document/emit#DOCUMENT TYPST_QUERY` pass reads the compiled `image` selector back through `typst.query`. The `_typst(value, scope)` escaper is one `TypstScope`-keyed `frozendict` `str.maketrans` algebra: the `STRING` context escapes `\` and `"` for the `alt`/`asset_key`/link-`href` quoted-string arguments, and the `MARKUP` context escapes the Typst content-mode active set `\[]#*_@$<>` and backtick for every interpolated `RunNode.text`, heading, caption, and link-text run, so an `alt` carrying a quote or a run carrying `]`/`#` produces valid markup. The field rides the `Reparametrized` own-field overlay with every other `FigureNode` own field, so a re-authored alt re-keys the figure's `node_digest` and emits one `Reparametrized` edit; no parallel alt-tracking surface exists.
- [AEC_CROSS_REFERENCE]: the node algebra carries the classification-and-cross-reference concept the UNIFIED-TELOS drawing<->spec plane resolves over, so the two AEC coordination consumers key against the one tree rather than a parallel structure. `NodeMeta.classification` is the bounded `ClassificationCode` CSI/OmniClass notation (`specification/classify#CLASSIFY` `ClassCode.render()`), typed `ClassificationCode | UnsetType = UNSET` so an unclassified node round-trips absent under `omit_defaults` and a classified section/figure/detail node carries the validated code the `specification/section#SECTION` `Spec.to_document` lowering writes and the `classify#CLASSIFY` `ReferenceIndex.resolve` reads — carried as the rendered string, never an imported `ClassCode`, because the substrate tree the specification folder lowers INTO must not depend on it (the `document`->`specification` dependency direction the `drawing/detail#DETAIL` `DetailEntry.classification: str` seam already fixes), so an existing tree's `node_digest` is unchanged under `omit_defaults` while a classified node re-keys. `Xref` is the one AEC cross-reference `AnnotTarget` case — `sheet`/`detail` the `drawing/detail#DETAIL` `DetailRef.cite()` "3/A-501" detail-on-sheet coordinate, `code` the governing `classify#CLASSIFY` section — so a detail callout or a specification keynote is a `LINK`-kind `AnnotationNode` whose target round-trips its cited sheet/detail/section rather than dropping it, the citation resolved at `composition/imposition` sheet-set assembly (a cross-sheet target is a separate compilation, so `to_typst_source` lowers the escaped citation as a `#link("<cite>")` destination string, never an intra-compilation `label()` anchor that would hard-error on the absent label). Both project to the corpus as the `classification`/`xref` columns `to_corpus(node, STRUCT)` fills — the classification off `NodeMeta`, the `xref` off `_link_cite`'s one `Xref` discrimination — so the classify `ReferenceIndex` and a revision-impact query read a column predicate over the content-keyed corpus exactly as the accessibility audit reads `alt_status`, the coordination folding over the one queryable tree value rather than re-walking a parallel callout set. Each addition cites its consumer: the classification the `classify#CLASSIFY` `ReferenceIndex.of`/`coordinate` reconciliation, the `Xref` the `drawing/detail#DETAIL` `Callout`/`DetailRef` cross-reference DAG — a contract with no tree-resident spelling before, now one field and one case on the existing owners rather than a sibling structure beside the tree.
- [MSGSPEC_RECURSIVE_UNION]: the ten `DocumentNode` variants form a recursive `Union` via the string forward references PEP 563 is NOT required for — `msgspec.msgpack.Decoder(DocumentNode)` resolves the forward reference on the `children`/`heading`/`runs`/`items`/`caption`/`rows` fields at decoder construction and discriminates on the `tag_field="kind"` tag (the `NodeKind` value), so the tree round-trips without a custom `dec_hook`, the decoded struct exposing the `kind` only as the encoded field, never a runtime `.kind`/`.tag` attribute, and `from __future__ import annotations` is never written (the active-surface deferred-annotation default and the `msgspec` decoder's own forward-reference resolution carry the recursion; the prior page's `__future__` citation was a phantom justification for a forbidden idiom). The `DocumentDelta` patch decodes as `tuple[DocumentDelta, ...]` under `tag_field="edit"`. The `Reparametrized` field map carries `msgspec.Raw` opaque values over the OWN fields only (every field except the structural `children`/`items`), and `_apply_fields` overlays them onto `msgspec.to_builtins(node)` then re-coerces through `msgspec.convert(merged, type(node))` so the changed own-fields re-validate against the concrete leaf variant's field types in one pass — no per-field annotation lookup (deferred annotations are strings, so a field-type decode would mis-resolve), no eager whole-tree re-validation, and a node's structural children are untouched by an own-content overlay. The `_index`/`diff`/`merge`/`invert` traversals fold through `expression` `Map.add`/`try_find` and `Block.fold`/`choose`/`collect` rather than a `dict[ContentKey, ...]` built by mutation and a `list[DocumentDelta]` grown by `append` — the immutable-traversal law applied to the structural index so the diff is one rail-shaped fold, the casualty `Block.choose` over the survivor `try_find` replacing the prior `edits.extend(...)` generator-into-list pattern.
- [FORMULA_MATH]: the equation is a tree-resident concept, so `FormulaNode` is the eleventh `DocumentNode` variant carrying the LaTeX `tex` math source, a `display` inline/block flag, and the `alt` text equivalent, closing the both-plane gap where a formula could ONLY ride a pre-rendered `FigureNode` and the `FORMULA` `StructEltKind` role was a vocabulary member no node produced. LaTeX is the ONE canonical math notation because it is the universal interchange every consumer already reads — journal submission (`to_latex`), Pandoc/GFM manuscripts (`to_markdown` `$..$`/`$$..$$`), MathJax/KaTeX journal-web HTML (`to_html` `\(..\)`/`\[..\]` on a `role="math"` element carrying the `alt` as `aria-label`), and the AEC `folder:../typography` `ziamath` LaTeX→SVG renderer — so no per-backend math dialect and no `notation` discriminant is needed. The Typst backend cannot parse LaTeX natively, so `to_typst_source` lowers through Typst's `mitex` LaTeX-math bridge (`#mitex(..)` block, `#mi(..)` inline) — a `@preview` registry package the `document/emit#DOCUMENT` Typst preamble imports and the typst compiler resolves from its package cache, NOT a pip dependency the roster admits — the `tex` STRING-context-escaped so a `\`/`"` survives the Typst string literal into the LaTeX the bridge parses. The `tex` field is a TRUSTED authored math island emitted verbatim into the target's native math delimiters: escaping it (the TEMPLATE-SAFETY reflex for untrusted markup input) would corrupt the LaTeX, so the escape applies only at the Typst string-literal seam and the HTML lowering sets the delimited LaTeX as escape-safe element `.text` rather than an f-string markup splice. `alt_of` extends by one or-pattern alternative (`FigureNode(alt=alt) | FormulaNode(alt=alt)`) so the `Formula` `/Alt` presence the ISO 14289 §formula requirement mandates rides the corpus `alt_status` column exactly as a figure's does, the accessibility audit's un-described-content predicate now `kind in (FIGURE, FORMULA) and alt_status == ABSENT` over the one queryable corpus; the `folder:document/tagged#ACCESS` `_elem` reads the same `alt_of` for the `/Formula` element's `/Alt`. A `RunNode`-embedded math payload beside the variant, and a parallel Typst-native `typst_math` field beside `tex`, are the rejected re-fragmentations the one variant plus the one `mitex` bridge collapse.
- [STRUCTURED_INTERCHANGE]: the node tree carries two structured-interchange egresses beside the presentation lowerings, both DERIVED from surfaces the page already owns and neither the deleted `RoleView` schema-inspect machinery. `to_json` is `_JSON_ENCODER.encode(node)` over the shared `msgspec.json.Encoder(order="deterministic")` — the whole recursive tagged-union tree lowered to deterministic JSON (the `kind`/`role_kind`/`target` tags round-tripping under their `tag_field`s), the JSON-LD/JATS-adjacent structured-data publication interchange a downstream reader decodes back through the same `_DOCUMENT_DECODER` shape, distinct from the msgpack `encode`/`to_corpus(BYTES)` byte forms by codec and audience (human/interchange JSON vs binary wire/digest) — the real interchange the publication telos admits where the deleted `json.schema_components((StructRole,), ...)` `(components, defs)` blob was a schema-SHAPE view no consumer read. `to_c14n` is `etree.tostring(_element(node), method="c14n2")` — the deterministic Canonical XML 2.0 the journal/archival JATS-adjacent interchange consumes, `method="c14n2"` fixing attribute order, namespace-prefix rewriting, and whitespace so two structurally-identical trees serialize byte-identically (a stable content key over the XML form), the archival counterpart to the `method="html"` presentation lowering off the ONE escape-safe `_element` builder — so the tree that already lowers to an `_Element` for HTML gains canonical XML with zero second builder, and `content_identity.ContentIdentity.of` keys the c14n bytes for a reuse-fabric-stable archival identity. Both are whole-tree projections needing no per-variant arm (the `to_json` encoder walks the union by tag, the `to_c14n` reads the `_element` arms `to_html` already declares), so a new variant reaches both interchanges through the arms Growth already lists.
