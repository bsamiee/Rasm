# [PY_ARTIFACTS_TAGGED]

The PDF/UA (ISO 14289-1) structural close over the document rail. `Access` is ONE owner that authors the marked-content structure-element tree into an emitted PDF AND audits its conformance, discriminating a closed `AccessOp` (`TAG`/`AUDIT`) over the frozen `_ARM` `frozendict` data-row dispatch — never a `tag_pdf`/`audit_pdf`/per-role writer family and never a `MappingProxyType` view. `TAG` lowers the `document/model#NODE` `StructureNode` tree into the `pikepdf` `/StructTreeRoot` (the `/Type /StructElem` indirect dictionaries keyed by `/S`, the multi-page `/ParentTree` number-tree, the `/RoleMap` foreign→standard map, the `/MarkInfo /Marked` flag) AND the catalog-level PDF/UA requirements ISO 14289 mandates beyond the tree — the `/Lang` natural-language tag, the `/ViewerPreferences /DisplayDocTitle` flag, and the XMP `pdfuaid:part` identifier plus `dc:title` over `Pdf.open_metadata`; `AUDIT` walks the authored tree AND the catalog into a typed `StructureAudit` whose `failures` is a closed `UaCheck` clause set and whose `conformant` is its emptiness.

The structure vocabulary is CONSUMED, never re-minted: `document/model#NODE` owns the `StructureNode` variant, the `StructEltKind`/`StructRole` family, the `_STRUCT_CATEGORY` behavior table, and the `role_of`/`role_category`/`alt_of`/`children`/`standard_for` projections this owner reads, so `tagged.md` lowers FROM that algebra into PDF marked content rather than re-declaring a `_STANDARD_ROLES`/`_STANDARD_FOR` parallel — the `/S`-string algebra the audit reads is DERIVED once from `StructEltKind` (`_ELT` decoding a read `/S` Name to its model member, its key set the standard-structure membership the `/RoleMap` completeness check reads; `_CATEGORY` projecting each member to the model `role_category` `(StructCategory, heading_level)` row the heading and nesting checks fold over), tracking the model vocabulary with zero re-declared `_HEADINGS`/`_STANDARD_TYPES` literal. `pikepdf` is admitted and ungated; this owner closes an already-emitted PDF, contributes the settled `core/receipt#RECEIPT` `Egress`/`Pdf` cases through the `@receipted` harvest weave over a thin pure `_emit`, and threads `StructureAudit.conformant` into the `exchange/conformance#SIGN` `ConformanceVerdict.structural_conformant` so the structural verdict pyhanko honestly disclaims is closed by the producer that authored the structure. Every arm returns a `RuntimeRail[ContentKey]` keyed by the runtime content key.

## [01]-[INDEX]

- [01]-[ACCESS]: `pikepdf` `StructTreeRoot`/`StructElem` marked-content tagged-PDF authoring (`TAG`) and ISO 14289-1 structural self-audit (`AUDIT`) owner over the closed `AccessOp` `_ARM` `frozendict` dispatch; `AccessParams` is the one trusted authoring policy (`source` tree, `lang`/`title`/`ua_part` catalog requirements); `AccessFact` the bytes-plus-evidence carrier each arm threads onto the frozen owner through `structs.replace`; `StructureAudit` the typed structural-conformance verdict whose `UaCheck` `failures` set the `core/receipt#RECEIPT` `Egress`/`Pdf` cases evidence and the `exchange/conformance#SIGN` `ConformanceVerdict.structural_conformant` consumes; the `@receipted` weave harvests `contribute` off the stepped owner the pure `_emit` returns and `async_boundary` is the fault-converting capsule.

## [02]-[ACCESS]

- Owner: `Access` the one PDF/UA structural-close owner discriminating the access op; `AccessOp` the closed `StrEnum` over tag-authoring and structural audit; `_ARM` the `frozendict[AccessOp, Arm]` data-row dispatch mapping each op to its single `AccessFact`-returning arm with zero `match`/`case` sprawl, the closed `StrEnum` membership total over the table by construction (the sibling `exchange/conformance#SIGN`/`document/egress#FINISH` dispatch shape, modernized off `MappingProxyType` onto the `frozendict` builtin). `pikepdf` owns the qpdf object model (`Object`/`Dictionary`/`Array`/`Name`/`String`), the indirect-object mint (`Pdf.make_indirect`), the catalog (`Pdf.Root`), the XMP metadata context (`Pdf.open_metadata` → `PdfMetadata`), the page collection, and the content-stream mint (`Pdf.make_stream`)/marked-content emitter (`Page.contents_add` + `canvas.ContentStreamBuilder.begin_marked_content`); `document/model#NODE` owns the source `StructureNode` tree, the `StructEltKind`/`StructRole`/`StructCategory` vocabulary, the `_STRUCT_CATEGORY` table, and the `role_of`/`role_category`/`alt_of`/`children`/`standard_for` projections this owner reads rather than re-deriving — the `TAG` arm reads `role_of`/`standard_for` for the `/S` write and the `/RoleMap` lowering, the `AUDIT` arm reads `role_category` (through the derived `_CATEGORY` table) for its heading-level and structure-nesting clauses. `StructureAudit` is the carried structural verdict; `AccessParams` the trusted policy bundle; `AccessFact` the threaded evidence.
- Cases: `AccessOp` rows `TAG` (fold the `StructureNode` tree into the `pikepdf` structure tree — set `/MarkInfo << /Marked true >>` and `/ViewerPreferences << /DisplayDocTitle true >>` on the catalog, write `/Lang` from `AccessParams.lang` or the root node `NodeMeta.lang`, write the XMP `pdfuaid:part` identifier and `dc:title` through `open_metadata`, mint the indirect `/StructTreeRoot` carrying `/K`/`/ParentTree`/`/ParentTreeNextKey`/`/RoleMap`, recurse the `StructureNode` children into indirect `/Type /StructElem` dictionaries keyed by `/S` the `role_of` value, attach `/P`/`/Pg`/`/Alt` (from the `FigureNode` child `alt_of`)/`/ActualText`+`/Lang` (from `NodeMeta`), assign each leaf an integer MCID into its page's `/ParentTree /Nums` slot keyed by the page `/StructParents` index, and re-emit through `Pdf.save`) · `AUDIT` (walk the authored `/StructTreeRoot` `/K` spine AND the catalog/XMP into a `StructureAudit` — the element count, tag-tree depth, per-role tallies, figure-alt coverage, heading monotonicity, role-map completeness, standard-legal structure nesting, table/list/link structure, multi-page `/StructParents` keying, and the closed `UaCheck` `failures` set gating `conformant`) — selected by the frozen `_ARM` row, never a chain of `is`-probes. Role lowering is one model read: a `StandardRole` writes `/S` the `StructEltKind` value directly, a `ForeignRole` writes `/S` the foreign role string AND registers `/RoleMap[<foreign>] = standard_for(role)` so the non-standard role maps to a standard structure type per the PDF/UA requirement, the foreign→standard correspondence resolved through the model `standard_for` projection rather than a parallel table owned here.
- Auto: `_tag` opens the PDF through `pikepdf.open`, authors the catalog requirements (`Root.MarkInfo`/`Root.ViewerPreferences`/`Root.Lang` plus the `open_metadata` XMP `pdfuaid:part`/`dc:title`), mints the indirect `/StructTreeRoot`, then folds `_elem` over the source tree once threading one `_Author` boundary accumulator (the foreign→standard `role_map`, the per-page `slots` parent-tree arrays where `slots[page][mcid]` is the `StructElem` owning that marked-content id, and the running element/figure counts). `_elem` mints one `/Type /StructElem` per `StructureNode`, writes `/S` through `role_of`, registers a `ForeignRole` in `role_map` through `standard_for`, writes `/Alt` from the `FigureNode` child's `alt_of` and `/ActualText`/`/Lang` from `NodeMeta`, recurses the `StructureNode` branches into `/K`, and binds a content leaf's MCID into `slots[node.meta.page]` — the structural recursion driven by the model `children` projection, never a re-enumeration of the node variants; the in-stream `/Tag <</MCID n>> BDC … EMC` operator emission for each region is `document/emit#DOCUMENT`'s drawing concern, authored in the same document order so the MCIDs `_elem` assigns match the regions emit marked. `_tag` then writes the per-page `/ParentTree /Nums`, the `/StructParents` page keys, and the `/RoleMap`, and `Pdf.save` re-emits the enriched bytes. `_audit` reopens the PDF, reads the catalog flags and the `open_metadata` XMP, recurses the `/StructTreeRoot` `/K` spine discriminating each kid by its qpdf-coerced kind (`isinstance` against `pikepdf.Dictionary`/`pikepdf.Array`, the surface of the `pikepdf.ObjectType` axis: a `Dictionary` `/Type /StructElem` is a child structure element recursed; an `int` MCID and an `/MCR`/`/OBJR` dictionary are content leaves), decoding each read `/S` Name to its model `StructEltKind` through `_ELT` and `match`-ing on that member rather than a `/Figure`/`/Table` string literal, folds one `_Tally` boundary accumulator over the per-role facts — the heading bucket and level read off `_CATEGORY[elt]` (never a `_HEADINGS` set or an `int(role[2:])` slice), the nesting clause threading each element's parent member down the recursion and tallying a `misnested` count against the `_NESTING` legal-parent policy — and projects the `StructureAudit` whose `failures` is the one data-driven fold over the `(UaCheck, predicate)` rows, never a per-metric re-traversal.
- Receipt: the `TAG` arm contributes `ArtifactReceipt.Egress` carrying the content key, the post-author byte count, the page count, and the structural facts mapped onto the existing flat `Egress` slots (the structure-element count riding `outline_depth`, the figure count riding `overlays` — the finishing-facts case the `document/egress#FINISH` owner shares, reused rather than a parallel access case), and the `AUDIT` arm contributes `ArtifactReceipt.Pdf` carrying the content key, the audited byte count, and the page count — the settled `core/receipt#RECEIPT` reuse target the receipt page fixes for this producer, never a fifteenth receipt case. The `@receipted` weave drains `contribute` off the stepped owner the pure `_emit` returns and emits through the runtime `Signals.emit_async` without an inline emit per arm; `contribute` reads the threaded `AccessFact` and folds the case off the `op` discriminant. The `StructureAudit` value is content-addressed by the `AUDIT` content key (its `msgpack` encoding IS the audited artifact) and decoded by the composition root to thread `conformant` into the `exchange/conformance#SIGN` `ConformParams.structural_conformant`, so the structural result is interior evidence the `ConformanceVerdict` carries rather than a new receipt field, the same one-way acyclic edge the `ConformanceVerdict` value carries.
- Packages: `pikepdf` (`open` document factory; `Pdf.Root` catalog mutation; `Pdf.make_indirect` indirect-object mint; `Pdf.make_stream` content-stream mint; `Pdf.open_metadata` → `PdfMetadata` XMP context mapping; `Pdf.pages`/`Pdf.save`; the `Object` model `Dictionary`/`Array`/`Name`/`String`/`Boolean` typed scalars with `.get`/`.keys` traversal; the `ObjectType` kind discriminant for the read walk; `Page.obj` the backing page dictionary the `/StructParents` index binds onto; `Page.contents_add` + `canvas.ContentStreamBuilder.begin_marked_content` the marked-content operator surface emit composes); `document/model#NODE` (`StructureNode`/`FigureNode`, the `StructEltKind`/`StandardRole`/`ForeignRole`/`StructCategory`/`LangTag` vocabulary, `role_of`/`role_category`/`alt_of`/`children`/`standard_for` the consumed tree algebra — never re-minted); `msgspec` (`Struct(frozen=True)` the value owners, `structs.replace` the fact thread, `msgpack.Encoder(order="deterministic")` the `StructureAudit` content codec, `UnsetType` the `NodeMeta` absent-marker read); `expression` (`Result`/`Ok`/`Error`/`tagged_union` the admission rail and fault family); `itertools` (`pairwise` the heading-monotonicity consecutive-level pairing); `frozendict` (the `_ARM` dispatch table, the model-derived `_ELT`/`_CATEGORY` `/S`-string projections, the `_NESTING` legal-parent policy); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`, `receipts.Receipt`/`Redaction`/`receipted`).
- Growth: a new access op is one `AccessOp` row plus one `_ARM` arm entry; a new structural conformance clause is one `UaCheck` member plus one `(UaCheck, predicate)` row in the `failures` fold; a new structural metric is one `StructureAudit` field plus one `_Tally` slot; a new standard nesting rule is one `_NESTING` row keyed by the constrained `StructEltKind`; a new standard PDF/UA role is absorbed upstream as one `document/model#NODE` `StructEltKind` member (the `_elem` `/S` write, the `standard_for` lowering, and the derived `_ELT` membership plus the `_CATEGORY` heading/nesting row pick it up with zero change here); a foreign role rides the one `ForeignRole` arm and the `/RoleMap` registration; a new catalog requirement is one `AccessParams` field; zero new surface.
- Boundary: no document authoring (that stays at `document/emit#DOCUMENT`), no structure-tree TYPE ownership (the `StructureNode`/`StructEltKind`/`StructRole` family and the `_STRUCT_CATEGORY`/`_STANDARD_FOR` tables are `document/model#NODE`'s, consumed here), no PDF signing (that is `exchange/conformance#SIGN`), no security/navigation finishing (that is `document/egress#FINISH`), no OCG optional-content authoring (that is `export/layered#LAYERED`); the owner authors and audits the marked-content structure tree plus the catalog PDF/UA requirements over an already-emitted PDF, never producing one. `pikepdf` exposes no high-level `StructTreeRoot`/`StructElem` helper class — the tree is authored over the raw `Object` model with `make_indirect`, so a phantom `pdf.add_structure_tree()` convenience is the rejected form and the object-model spike is the real surface. The deleted forms: a `MappingProxyType` dispatch where the `frozendict` builtin is the table owner; a re-declared `_STANDARD_ROLES` frozenset and a `_STANDARD_FOR` `dict[str, str]` keyed by category string where the model owns the `StructEltKind` vocabulary and the `standard_for` projection; a `MarkContext` `msgspec.Struct` with mutable `{}`/`[]` defaults `msgspec` rejects where a `_Author` boundary `dataclass` accumulator threads the spike; a single-page `pages[0]`/`StructParents = 0` hardcode where the multi-page `/ParentTree` keys every page by `NodeMeta.page`; a `/Alt`-only figure check ignoring `/ActualText`; a naive seven-field `StructureAudit` slice ignoring `/Lang`, document title, the XMP `pdfuaid:part`, `/Suspects`, role-map completeness, standard structure nesting, and table/list/link structure; a `tag_pdf`/`audit_pdf` writer family; an inline-described receipt no `contribute`/`@receipted` weave wires; a stringly-typed `/S` bypassing the `StructEltKind` vocabulary; a string-literal `match role` re-spelling `/Figure`/`/Table` plus a re-derived `_HEADINGS` frozenset and an `int(role[2:])` level slice where the model-derived `_ELT`/`_CATEGORY` tables decode the `/S` Name to its `StructEltKind` member and project its category/level in one read; a content-addressed `StructureAudit` encoder lacking `order="deterministic"` whose bytes the content key cannot stably address; and a self-authored veraPDF-grade verdict (no JVM, no external grade — `conformant` is the ISO 14289-1 self-audit of the tree this owner authored against its source-of-truth `StructureNode`). The `AUDIT` boolean threads into `exchange/conformance#SIGN` as interior structural evidence, closing the gap pyhanko discloses rather than asserting an independent conformance authority.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from dataclasses import dataclass, field as dc_field
from enum import StrEnum
from io import BytesIO
from itertools import pairwise
from typing import TYPE_CHECKING, Final, Literal, Self

import msgspec
from anyio import CapacityLimiter, to_thread
from builtins import frozendict
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct, field, structs

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.receipts import OPEN, Receipt, receipted

from artifacts.core.receipt import ArtifactReceipt
from artifacts.document.model import (
    FigureNode,
    ForeignRole,
    LangTag,
    StandardRole,
    StructCategory,
    StructEltKind,
    StructureNode,
    alt_of,
    children,
    role_category,
    role_of,
    standard_for,
)

lazy import pikepdf
lazy from pikepdf import Array, Dictionary, Name, String

if TYPE_CHECKING:
    import pikepdf

# --- [TYPES] ----------------------------------------------------------------------------

type Arm = Callable[["Access"], "AccessFact"]


class AccessOp(StrEnum):
    TAG = "tag"
    AUDIT = "audit"


class UaCheck(StrEnum):  # the ISO 14289-1 structural-conformance clauses the AUDIT closes over
    MARKED = "marked"                  # /MarkInfo /Marked true
    STRUCT_TREE = "struct-tree"        # /StructTreeRoot present
    LANG = "lang"                      # catalog /Lang present
    TITLE = "title"                    # XMP dc:title + /ViewerPreferences /DisplayDocTitle true
    UA_ID = "ua-id"                    # XMP pdfuaid:part identifier
    NOT_SUSPECT = "not-suspect"        # /MarkInfo /Suspects absent or false
    FIGURE_ALT = "figure-alt"          # every /Figure carries /Alt or /ActualText
    HEADING_NESTING = "heading-nesting"  # no skipped heading level in document order
    ROLE_MAP = "role-map"              # every non-standard /S mapped in /RoleMap
    STRUCTURE_NESTING = "structure-nesting"  # every constrained /S nests under a standard-legal parent role
    TABLE_REGULAR = "table-regular"    # every /Table carries /TR rows
    LIST_STRUCTURE = "list-structure"  # every /L carries /LI items
    LINK_CONTENT = "link-content"      # every /Link carries content (kids)
    PAGES_KEYED = "pages-keyed"        # every page keyed into the /ParentTree via /StructParents


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class AccessFault:
    # the closed ADMISSION vocabulary `of` produces; the arm-level `pikepdf.PdfError` converts
    # to the runtime `BoundaryFault` at the `async_boundary` capsule, never this interior vocabulary.
    tag: Literal["incomplete"] = tag()
    incomplete: AccessOp = case()  # an op admitted without its required `AccessParams.source` tree


# --- [MODELS] ---------------------------------------------------------------------------


class StructureAudit(Struct, frozen=True, gc=False):
    elements: int
    depth: int
    pages: int
    pages_keyed: int
    figures: int
    figures_with_alt: int
    headings: int
    tables: int
    lists: int
    links: int
    role_map: int
    misnested: int
    failures: tuple[UaCheck, ...]

    @property
    def coverage(self) -> float:
        return 1.0 if self.figures == 0 else self.figures_with_alt / self.figures

    @property
    def conformant(self) -> bool:  # the boolean exchange/conformance#SIGN folds via ConformParams.structural_conformant
        return not self.failures

    def facts(self) -> dict[str, str]:  # the scalar projection a span/log consumer reads off the decoded content-addressed audit
        return {
            "elements": str(self.elements),
            "depth": str(self.depth),
            "pages": str(self.pages),
            "pages_keyed": str(self.pages_keyed),
            "figures": str(self.figures),
            "coverage": f"{self.coverage:.6f}",
            "role_map": str(self.role_map),
            "misnested": str(self.misnested),
            "failures": ",".join(self.failures),
            "conformant": str(self.conformant),
        }


class AccessParams(Struct, frozen=True, kw_only=True):
    # the trusted authoring policy; `source` the model StructureNode tree TAG lowers, the catalog
    # PDF/UA requirements (`lang`/`title`/`ua_part`) authored alongside the structure tree.
    source: StructureNode | None = None
    lang: LangTag | None = None
    title: str = ""
    ua_part: Literal[1, 2] = 1  # PDF/UA-1 (ISO 14289-1) or PDF/UA-2 (ISO 14289-2); the only defined parts


class AccessFact(Struct, frozen=True):
    # the bytes-plus-evidence carrier each arm threads onto the owner; `audit` the AUDIT verdict
    # the content key addresses and exchange/conformance#SIGN decodes for `conformant`.
    data: bytes
    pages: int = 0
    elements: int = 0
    figures: int = 0
    audit: StructureAudit | None = None


class Access(Struct, frozen=True):
    op: AccessOp
    pdf: bytes
    params: AccessParams = field(default_factory=AccessParams)
    fact: AccessFact | None = None

    @classmethod
    def of(cls, op: AccessOp, pdf: bytes, /, *, params: AccessParams = AccessParams()) -> Result[Self, AccessFault]:
        ready = op is not AccessOp.TAG or params.source is not None
        return Ok(cls(op=op, pdf=pdf, params=params)) if ready else Error(AccessFault(incomplete=op))

    def _stepped(self) -> Self:
        return structs.replace(self, fact=_ARM[self.op](self))

    @receipted(Redaction(classified=Map.empty()))  # keep-all redaction (`Redaction` has no named instances; `Redaction.STRUCTURAL` was a phantom); drains `contribute` off the stepped owner, emits via Signals.emit_async
    async def _emit(self) -> Self:
        # the sync pikepdf object-model fold is GIL-releasing native, so it crosses the thread seam under the
        # shared limiter rather than running inline on the loop; the stepped owner is the `ReceiptContributor`.
        return await to_thread.run_sync(self._stepped, limiter=_OFFLOAD)

    async def author(self) -> RuntimeRail[ContentKey]:
        return (await async_boundary(f"access.{self.op}", self._emit)).map(
            lambda done: ContentIdentity.of(f"access-{done.op}", done.fact.data)
        )

    def contribute(self) -> Iterable[Receipt]:
        if (fact := self.fact) is None:  # contribute rides the stepped owner the weave returned, never the seed
            return
        key = ContentIdentity.of(f"access-{self.op}", fact.data)
        emitted = (
            ArtifactReceipt.Egress(key, len(fact.data), fact.pages, 0, fact.elements, fact.figures)
            if self.op is AccessOp.TAG
            else ArtifactReceipt.Pdf(key, len(fact.data), fact.pages)
        )
        yield from emitted.contribute()


# --- [CONSTANTS] ------------------------------------------------------------------------

# The /S-string algebra DERIVED once from the model StructEltKind vocabulary: `_ELT` decodes a read
# `/S` Name to its model member AND its key set IS the standard-structure membership the /RoleMap
# completeness check reads; `_CATEGORY` projects each member to the model's (category, heading_level)
# row through the public `role_category` projection — so the heading bucket, the heading level, and
# the standard set track the model enum with zero re-declared `_HEADINGS`/`_STANDARD_TYPES` literal.
_ELT: Final[frozendict[str, StructEltKind]] = frozendict({f"/{elt.value}": elt for elt in StructEltKind})
_CATEGORY: Final[frozendict[StructEltKind, tuple[StructCategory, int]]] = frozendict(
    {elt: role_category(StandardRole(elt=elt)) for elt in StructEltKind}
)
# the standard structure-nesting policy this AUDIT owns: each constrained role's legal parent set per
# ISO 14289 (list/table grouping); a role absent from the table nests anywhere, a foreign role is unconstrained.
_NESTING: Final[frozendict[StructEltKind, frozenset[StructEltKind]]] = frozendict({
    StructEltKind.LI: frozenset({StructEltKind.L}),
    StructEltKind.LBL: frozenset({StructEltKind.LI}),
    StructEltKind.LBODY: frozenset({StructEltKind.LI}),
    StructEltKind.THEAD: frozenset({StructEltKind.TABLE}),
    StructEltKind.TBODY: frozenset({StructEltKind.TABLE}),
    StructEltKind.TFOOT: frozenset({StructEltKind.TABLE}),
    StructEltKind.TR: frozenset({StructEltKind.TABLE, StructEltKind.THEAD, StructEltKind.TBODY, StructEltKind.TFOOT}),
    StructEltKind.TH: frozenset({StructEltKind.TR}),
    StructEltKind.TD: frozenset({StructEltKind.TR}),
})
_AUDIT_ENCODER: Final = msgspec.msgpack.Encoder(order="deterministic")  # the content key addresses this stable encoding
_OFFLOAD: Final = CapacityLimiter(8)  # the pikepdf object-model fold is GIL-releasing native; crosses the thread seam, never the loop


# --- [OPERATIONS] -----------------------------------------------------------------------


@dataclass(slots=True)
class _Author:  # the TAG boundary accumulator: foreign role map, per-page MCID slots, running counts
    role_map: dict[str, str] = dc_field(default_factory=dict)
    slots: dict[int, list["pikepdf.Object"]] = dc_field(default_factory=dict)
    elements: int = 0
    figures: int = 0


def _elem(pdf: "pikepdf.Pdf", node: StructureNode, parent: "pikepdf.Object", build: _Author, /) -> "pikepdf.Object":
    build.elements += 1
    role = role_of(node)
    elem = pdf.make_indirect(Dictionary(Type=Name.StructElem, S=Name("/" + role), P=parent, Pg=pdf.pages[node.meta.page].obj))
    if isinstance(node.role, ForeignRole):
        build.role_map[role] = standard_for(node.role).value  # /RoleMap maps the foreign role to its standard type
    if (figure := next((kid for kid in children(node) if isinstance(kid, FigureNode)), None)) is not None:
        build.figures += 1
        if alt := alt_of(figure)[0]:
            elem.Alt = String(alt)
    if not isinstance(node.meta.actual_text, msgspec.UnsetType) and node.meta.actual_text:
        elem.ActualText = String(node.meta.actual_text)
    if not isinstance(node.meta.lang, msgspec.UnsetType) and node.meta.lang:
        elem.Lang = String(node.meta.lang)
    if branches := tuple(kid for kid in children(node) if isinstance(kid, StructureNode)):
        elem.K = Array([_elem(pdf, kid, elem, build) for kid in branches])
    else:  # a content leaf binds one MCID into its page's parent-tree slot array (emit marks the in-stream span)
        owners = build.slots.setdefault(node.meta.page, [])
        elem.K = len(owners)
        owners.append(elem)
    return elem


def _tag(access: "Access") -> AccessFact:
    params, source = access.params, access.params.source
    with pikepdf.open(BytesIO(access.pdf)) as pdf:  # deterministic close, never GC-reaped
        pdf.Root.MarkInfo = Dictionary(Marked=True)
        pdf.Root.ViewerPreferences = Dictionary(DisplayDocTitle=True)
        meta_lang = None if source is None or isinstance(source.meta.lang, msgspec.UnsetType) else source.meta.lang
        if (lang := params.lang or meta_lang) is not None:
            pdf.Root.Lang = String(lang)
        with pdf.open_metadata() as xmp:  # XMP PDF/UA identifier + document title
            xmp["pdfuaid:part"] = str(params.ua_part)
            if params.title:
                xmp["dc:title"] = params.title
        struct_root = pdf.make_indirect(Dictionary(
            Type=Name.StructTreeRoot, K=Array([]), ParentTree=pdf.make_indirect(Dictionary(Nums=Array([]))), ParentTreeNextKey=0, RoleMap=Dictionary()
        ))
        pdf.Root.StructTreeRoot = struct_root
        build = _Author()
        struct_root.K = Array([_elem(pdf, source, struct_root, build)]) if source is not None else Array([])
        role_map = Dictionary()
        for foreign, standard in build.role_map.items():
            role_map[Name("/" + foreign)] = Name("/" + standard)
        struct_root.RoleMap = role_map
        nums: list[object] = []
        for page_key in sorted(build.slots):
            pdf.pages[page_key].obj.StructParents = page_key
            nums.extend((page_key, Array(build.slots[page_key])))
        struct_root.ParentTree.Nums = Array(nums)
        struct_root.ParentTreeNextKey = max(build.slots) + 1 if build.slots else 0
        sink = BytesIO()
        pdf.save(sink)
        return AccessFact(sink.getvalue(), pages=len(pdf.pages), elements=build.elements, figures=build.figures)


@dataclass(slots=True)
class _Tally:  # the AUDIT boundary accumulator over the authored /StructTreeRoot spine
    elements: int = 0
    depth: int = 0
    figures: int = 0
    figures_with_alt: int = 0
    headings: int = 0
    tables: int = 0
    table_rows: int = 0
    lists: int = 0
    list_items: int = 0
    links: int = 0
    links_with_content: int = 0
    misnested: int = 0
    roles: set[str] = dc_field(default_factory=set)
    levels: list[int] = dc_field(default_factory=list)


def _walk(elem: "pikepdf.Object", depth: int, parent: StructEltKind | None, tally: _Tally, /) -> None:
    tally.elements += 1
    tally.depth = max(tally.depth, depth)
    role = str(elem.get(Name.S, ""))
    tally.roles.add(role)
    elt = _ELT.get(role)  # the model member behind the read /S, or None for a foreign role (nesting-exempt)
    if elt is not None and parent is not None and elt in _NESTING and parent not in _NESTING[elt]:
        tally.misnested += 1
    match elt:
        case StructEltKind.FIGURE:
            tally.figures += 1
            tally.figures_with_alt += bool(elem.get(Name.Alt) or elem.get(Name.ActualText))
        case StructEltKind.TABLE:
            tally.tables += 1
        case StructEltKind.TR:
            tally.table_rows += 1
        case StructEltKind.L:
            tally.lists += 1
        case StructEltKind.LI:
            tally.list_items += 1
        case StructEltKind.LINK:
            tally.links += 1
            tally.links_with_content += bool(elem.get(Name.K))
        case StructEltKind() as heading if _CATEGORY[heading][0] is StructCategory.HEADING:
            tally.headings += 1
            tally.levels.append(_CATEGORY[heading][1])
        case _:
            pass
    kids = elem.get(Name.K)
    members = kids if isinstance(kids, pikepdf.Array) else (kids,) if isinstance(kids, pikepdf.Dictionary) else ()
    for kid in members:
        if isinstance(kid, pikepdf.Dictionary) and kid.get(Name.Type) == Name.StructElem:
            _walk(kid, depth + 1, elt, tally)


def _audit(access: "Access") -> AccessFact:
    with pikepdf.open(BytesIO(access.pdf)) as pdf:  # deterministic close, never GC-reaped
        root = pdf.Root
        mark_info = root.get(Name.MarkInfo, Dictionary())
        struct_root = root.get(Name.StructTreeRoot)
        role_map = struct_root.get(Name.RoleMap, Dictionary()) if struct_root is not None else Dictionary()
        tally = _Tally()
        for kid in struct_root.get(Name.K, Array([])) if struct_root is not None else ():
            if isinstance(kid, pikepdf.Dictionary) and kid.get(Name.Type) == Name.StructElem:
                _walk(kid, 1, None, tally)
        pages = len(pdf.pages)
        keyed = sum(Name.StructParents in page.obj for page in pdf.pages)
        with pdf.open_metadata() as xmp:
            ua_id, has_title = "pdfuaid:part" in xmp, "dc:title" in xmp
        display_title = bool(root.get(Name.ViewerPreferences, Dictionary()).get(Name.DisplayDocTitle, False))
        mapped = {str(name) for name in role_map.keys()}  # the foreign /S Names the /RoleMap registers
        unmapped = any(role not in _ELT and role not in mapped for role in tally.roles)  # standard membership IS `_ELT`'s key set
        monotone = all(b - a <= 1 for a, b in pairwise(tally.levels) if b > a)
        failures = tuple(check for check, ok in (
            (UaCheck.MARKED, bool(mark_info.get(Name.Marked, False))),
            (UaCheck.STRUCT_TREE, struct_root is not None),
            (UaCheck.LANG, Name.Lang in root),
            (UaCheck.TITLE, has_title and display_title),
            (UaCheck.UA_ID, ua_id),
            (UaCheck.NOT_SUSPECT, not bool(mark_info.get(Name.Suspects, False))),
            (UaCheck.FIGURE_ALT, tally.figures == tally.figures_with_alt),
            (UaCheck.HEADING_NESTING, monotone),
            (UaCheck.ROLE_MAP, not unmapped),
            (UaCheck.STRUCTURE_NESTING, tally.misnested == 0),
            (UaCheck.TABLE_REGULAR, tally.tables == 0 or tally.table_rows > 0),
            (UaCheck.LIST_STRUCTURE, tally.lists == 0 or tally.list_items > 0),
            (UaCheck.LINK_CONTENT, tally.links == tally.links_with_content),
            (UaCheck.PAGES_KEYED, keyed == pages),
        ) if not ok)
        audit = StructureAudit(
            elements=tally.elements, depth=tally.depth, pages=pages, pages_keyed=keyed,
            figures=tally.figures, figures_with_alt=tally.figures_with_alt, headings=tally.headings,
            tables=tally.tables, lists=tally.lists, links=tally.links, role_map=len(mapped),
            misnested=tally.misnested, failures=failures,
        )
        return AccessFact(_AUDIT_ENCODER.encode(audit), pages=pages, audit=audit)


# --- [COMPOSITION] ----------------------------------------------------------------------
_ARM: Final[frozendict[AccessOp, Arm]] = frozendict({AccessOp.TAG: _tag, AccessOp.AUDIT: _audit})
```

## [03]-[RESEARCH]

- [TAGGED_AUTHORING] [RESOLVED]: the `pikepdf` tagged-PDF structure tree is authored over the raw qpdf `Object` model — no high-level `StructTreeRoot`/`StructElem` helper class exists on the installed distribution (`10.9.1`, libqpdf `12.3.2`), so the catalogue `[02]-[PUBLIC_TYPES]` object-model rows `Object`/`Dictionary`/`Array`/`Name`/`String` and the `[03]-[ENTRYPOINTS]` `Pdf.Root`/`Pdf.make_stream`/`Pdf.open_metadata` surface are the authoring primitives. `Pdf.make_indirect(obj) -> Object` mints the indirect dictionaries the structure tree requires (`/StructTreeRoot`, each `/StructElem`, the `/ParentTree`) — it is a live `pikepdf` member used here and across the corpus structure authoring, NOT enumerated in the `[03]-[ENTRYPOINTS]` rows that list only `make_stream`, so the catalogue under-documents it (the cross-file residual). `pdf.Root.MarkInfo = Dictionary(Marked=True)` sets the catalog tagged flag, `pdf.Root.ViewerPreferences = Dictionary(DisplayDocTitle=True)` and `pdf.Root.Lang = String(tag)` author the two ISO 14289 catalog requirements, `pdf.Root.StructTreeRoot = struct_root` binds the tree, and each page's `pdf.pages[i].obj.StructParents = key` keys it into the parent tree. The `/StructElem` dictionary carries `/Type /StructElem`, `/S` the structure-type `Name`, `/P` the parent ref, `/Pg` the page ref, `/K` (an `Array` of child refs and integer MCIDs, or a bare MCID for a single-leaf element), `/Alt`/`/ActualText`/`/Lang` the accessibility evidence, and the `/RoleMap` `Dictionary` maps each foreign role `Name` to a standard structure-type `Name`. The structure tree is a large object-model spike with no external validator and no JVM — the IDEAS `[ACCESSIBILITY]` "large object-model spike with no external grade and no JVM" framing.
- [CATALOG_REQUIREMENTS] [RESOLVED]: ISO 14289-1 (PDF/UA-1) conformance is more than the structure tree — the catalog `/MarkInfo /Marked true`, the document `/Lang` natural-language tag, the document title surfaced through `/ViewerPreferences /DisplayDocTitle true` plus an XMP `dc:title`, and the XMP `pdfuaid:part` identifier are mandatory and were the prior page's naive omission. `Pdf.open_metadata()` (catalogue `[03]-[ENTRYPOINTS]` `[08]`, returning the `models.PdfMetadata` mapping) is the XMP context manager the `TAG` arm sets `pdfuaid:part`/`dc:title` through and the `AUDIT` arm reads back (the same `open_metadata` read the `exchange/conformance#SIGN` `_audit_pdf` already composes), so the producer that authors the structure also stamps and proves the metadata identifier rather than disclaiming it. `AccessParams.lang` falls back to the root `StructureNode`'s `NodeMeta.lang`, and `NodeMeta.actual_text` lowers to `/ActualText` on every element — both the model accessibility fields (`model.md` `[ACCESSIBILITY_DOMAIN]`) authored here for the first time. `AccessParams.ua_part` is a `Literal[1, 2]`: ISO 14289-1 (PDF/UA-1) and ISO 14289-2 (PDF/UA-2) are the only defined parts, so an out-of-range identifier is unrepresentable at the type rather than admitted and written as an invalid `pdfuaid:part`, closing the illegal-value gap a bare `int` left open without growing an admission fault case.
- [MODEL_TREE_CONSUMED] [RESOLVED]: the source structure tree is the `document/model#NODE` `StructureNode` variant carrying the `StructRole` (`StandardRole(StructEltKind)` | `ForeignRole(str)`) the model owns; this owner CONSUMES `role_of`/`alt_of`/`children`/`standard_for` and the `StructEltKind` vocabulary rather than re-declaring the tree, the ARCHITECTURE `[02]-[SEAMS]` `document/model → document/tagged [NODE]` and `document/tagged ← document/model [NODE]` edges. `role_of(node)` projects a `StructureNode` to its `/S` string in one model call; `isinstance(node.role, ForeignRole)` is the standard-vs-foreign discriminant (replacing the prior stale `_STANDARD_ROLES` string-set membership test, which both re-declared the model vocabulary AND omitted `TH`/`Code`/`Quote` and the richer ISO roles the model now carries); `standard_for(role)` projects a foreign role to its canonical standard `StructEltKind` through the model's first-wins `_STANDARD_FOR` category inversion, so the `/RoleMap` lowering reads a model projection rather than a hand-kept parallel `dict` — the prior page's re-declared `_STANDARD_FOR: dict[str, str]` keyed by category string was the `DERIVED_LOGIC` cross-seam violation the model `[STRUCT_ROLE_TOTALITY]` finding names. The `AUDIT` arm consumes the model `role_category`/`StandardRole`/`StructCategory` surface the same way: `_CATEGORY = frozendict({elt: role_category(StandardRole(elt=elt)) for elt in StructEltKind})` is the one DERIVED `(StructCategory, heading_level)` projection this page keeps, composing the public `role_category` over the model `_STRUCT_CATEGORY` primary so the heading bucket, the heading level, and the structure-nesting parent check all read one model-tracked row, and `_ELT = frozendict({f"/{elt.value}": elt for elt in StructEltKind})` decodes a read `/S` Name to its model member with its key set serving as the standard-structure membership the `/RoleMap` completeness audit reads — both tracking the model enum with zero re-declared `_HEADINGS`/`_STANDARD_TYPES` literal. The standard-nesting policy `_NESTING` (each constrained list/table role's legal parent set) is the one conformance table this AUDIT owns, keyed by the model `StructEltKind` it composes rather than a model concern.
- [STRUCTURE_AUDIT] [RESOLVED]: the `AUDIT` arm recurses the authored `/StructTreeRoot` `/K` spine over the `Object` model AND reads the catalog/XMP, folding one `_Tally` into the `StructureAudit` whose `failures` is the closed `UaCheck` clause set — `MARKED`/`STRUCT_TREE`/`LANG`/`TITLE`/`UA_ID`/`NOT_SUSPECT`/`FIGURE_ALT`/`HEADING_NESTING`/`ROLE_MAP`/`STRUCTURE_NESTING`/`TABLE_REGULAR`/`LIST_STRUCTURE`/`LINK_CONTENT`/`PAGES_KEYED`, the full ISO 14289-1 structural surface the prior seven-boolean slice omitted. qpdf auto-coerces a read `/K` element to its Python kind, so the recursion discriminates `isinstance(kid, pikepdf.Dictionary)` `/Type /StructElem` (recursed) from the `int` MCID and `/MCR`/`/OBJR` leaves, the `pikepdf.ObjectType` enum the underlying kind axis, decoding each `/S` Name to its model `StructEltKind` through `_ELT` and `match`-ing the member, reading the heading bucket and level off `_CATEGORY` and the legal parent set off `_NESTING` rather than a `/Figure`/`/H3` string literal. `STRUCTURE_NESTING` threads each element's parent member down the recursion and gates on a zero `misnested` tally — a `LI` outside an `L`, a `TR` outside a `Table`/`THead`/`TBody`/`TFoot`, a `TH`/`TD` outside a `TR` each fails the clause the model `role_category` was provided to close. The `failures` construction is one data-driven fold over `(UaCheck, predicate)` rows collecting the failing clauses, never fourteen parallel boolean fields; `conformant` is `not failures` and `coverage` the `/Figure`-element `/Alt`-or-`/ActualText` ratio. `StructureAudit` round-trips through `msgspec.msgpack` so the verdict is the content the `AUDIT` content key addresses, decoded by the composition root to thread `conformant` into `exchange/conformance#SIGN` — never a veraPDF-grade external verdict (no pure-Python PDF/UA structural validator resolves on PyPI, the `exchange/conformance#SIGN` `[AUDIT]` finding).
- [RECEIPT_AND_ASPECT] [RESOLVED]: the receipt is WIRED, not described — the `@receipted` weave over the thin pure `_emit` drains `Access.contribute` off the stepped owner and emits through the runtime `Signals.emit_async`, the artifacts convention `document/egress#FINISH`/`document/emit#DOCUMENT` establish and the `core/receipt#RECEIPT` `ReceiptContributor` law mandates (the prior page only narrated the receipt in prose with no `contribute`/`@receipted` surface, the unwired-receipt-law defect). The `TAG` arm contributes `ArtifactReceipt.Egress(key, bytes, pages, 0, elements, figures)` mapping the structure-element count onto `outline_depth` and the figure count onto `overlays`, and the `AUDIT` arm `ArtifactReceipt.Pdf(key, bytes, pages)` — the settled `core/receipt#RECEIPT` `Egress`/`Pdf` reuse target (README `[20]-[TAGGED]`, IDEAS `[ACCESSIBILITY]`), never a fifteenth case. The owner crosses the synchronous object-model fold onto the GIL-releasing `anyio.to_thread` seam under a bounded `CapacityLimiter` inside the async `_emit` — in-process but never inline on the loop, exactly as `document/egress#FINISH` offloads its `pikepdf` finishing fold — over `pikepdf.open(...)` handles closed deterministically through `with` rather than left for GC; `async_boundary` converts a `pikepdf.PdfError` into the runtime `BoundaryFault`, and `Access.of` rejects a `TAG` without a `source` tree into `AccessFault.incomplete` before the fold.
- [SIGN_SEAM] [RESOLVED]: the `StructureAudit.conformant` boolean threads into the `exchange/conformance#SIGN` `ConformanceVerdict.structural_conformant` through `ConformParams.structural_conformant`, the ARCHITECTURE `[02]-[SEAMS]` `document/tagged → exchange [SIGN]` and `exchange/conformance ← document/tagged [ACCESS]` edges — the live `exchange/conformance#SIGN` `_audit_pdf` reads `structural_conformant=conform.params.structural_conformant` and its boundary explicitly disclaims pyhanko's lack of PDF/UA enforcement, naming the `document/tagged#ACCESS` owner as the structural authority. The `StructureAudit` is the leaf value this owner mints (content-addressed by the `AUDIT` key) and the composition root decodes to supply the boolean, the one-way acyclic edge mirroring the `ConformanceVerdict` value, closing the structural gap pyhanko honestly discloses rather than asserting an independent conformance authority.
- [MARKED_CONTENT_SEAM] [SEAM]: the `/StructElem` MCID references the integer index into its page's `/StructParents` `/ParentTree` array, and the in-stream `/Tag <</MCID n>> BDC … EMC` operator sequence binding the drawn region to that MCID is authored by `document/emit#DOCUMENT` (the producer that draws the content through `canvas.ContentStreamBuilder`/the typst/weasyprint auto-tagging backends emit.md names), since only the drawing producer knows each region's content-stream extent. This owner assigns MCIDs in document order per page into the `/ParentTree /Nums` and the emit drawing marks the regions in the same document order, so the two agree by construction. The cross-file obligation — emit stamping the marked-content operators (or carrying a `NodeMeta` marked-content id) so a canvas-drawn region resolves to the MCID this owner assigns — is the residual; `Pdf.make_stream`/`Page.contents_add`/`ContentStreamBuilder.begin_marked_content` are the confirmed surface emit composes for the canvas backends, and `begin_marked_content_proplist(mctype, mcid)` (the MCID property-list variant) is unverified against the catalogue's `begin_marked_content` row and is gated until the `pikepdf` catalogue rows it.
