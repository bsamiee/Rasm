# [PY_ARTIFACTS_TAGGED]

The PDF/UA (ISO 14289-1) structural close over the document rail, unified with the ISO 15930 PDF/X print-production close: `Access` authors the marked-content structure tree into an emitted PDF, audits its conformance, upgrades to the ISO 19005 archival profile, and preflights the PDF/X claim — one closed `AccessOp` over the frozen `_ARM` table, never a `tag_pdf`/`audit_pdf` writer family. Every verdict is TWO-SOURCE: the `pikepdf` self-audit owns the explainable per-clause verdict and the independent MIT/Apache `pdf_oxide` Rust oracle owns the cross-check the clause set cannot enumerate — `conformant` holds only when both agree, and a declared /pdfxid the oracle refutes fails `CLAIM_HONEST`.

The structure vocabulary is CONSUMED, never re-minted: `document/model#NODE` owns the `StructureNode` variant, the role family, and the `role_of`/`role_category`/`alt_of`/`children`/`standard_for` projections, so the `/S`-string algebra derives once from `StructEltKind` with zero re-declared literal. The MCID seam is shared law: this owner assigns MCIDs in document order per page into the `/ParentTree`, and `document/emit#DOCUMENT` — the only producer that knows each region's content-stream extent — marks the regions in the same document order, so the two agree by construction. `StructureAudit.conformant` threads into the `../exchange/conformance#CONFORMANCE` `AuditSpec.structural_conformant`; the receipt cases are the settled `core/receipt#RECEIPT` `Egress`/`Pdf` reuse under the `@receipted(OPEN)` weave.

## [01]-[INDEX]

- [01]-[ACCESS]: the tag/audit/archive/preflight conformance close over the frozen `_ARM` table, every verdict two-source.

## [02]-[ACCESS]

- Owner: `Access` — `_ARM` maps each op to its single `AccessFact`-returning arm with zero `match` sprawl, the closed `StrEnum` membership total over the table by construction; `pikepdf` owns the qpdf object model and the XMP context, `pdf_oxide` the independent oracle under its deterministic-close capsule, and the model owns the tree algebra this page only reads.
- Cases: TAG writes the catalog requirements ISO 14289 mandates beyond the tree (`/Lang`, `/DisplayDocTitle`, XMP `pdfuaid:part` + `dc:title`) and re-emits under `Pdf.save(deterministic_id=True)` with a pinned `settings.set_decimal_precision` — the content-addressing precondition, since a randomized `/ID` keys two identical-tree runs differently and defeats reuse elision; AUDIT includes the per-page `has_text_layer` gate (an image-only page fails PDF/UA even when tagged) and the `has_xfa` prohibition; ARCHIVE re-validates the upgraded handle through `validate_pdf_a` so the convert never self-certifies; PREFLIGHT is the print-plane sibling turning the declared claim into a two-source verdict.
- Auto: `_audit` walks the UNTRUSTED `/StructTreeRoot` `/K` spine through the depth-safe `Block` frontier — never native recursion an adversarial nesting depth overflows — resolves every `pikepdf`-touching clause predicate to a plain value BEFORE the handle frees, and reads metadata through the read-only `open_metadata(set_pikepdf_as_editor=False, update_docinfo=False)` form that never mutates the bytes it audits; the `/ParentTree` IS a PDF number-tree, owned by the modeled `pikepdf.NumberTree.new(pdf)` mapping-view, never a hand-assembled flat `Nums` array; `pikepdf` exposes no high-level `StructTreeRoot` helper, so the raw `Object`-model spike is the real surface and a phantom `pdf.add_structure_tree()` convenience is the rejected form.
- Receipt: the two producing ops share `ArtifactReceipt.Egress` (structure-element count riding `outline_depth`, figure count riding `overlays` — the finishing-facts convention `document/egress#FINISH` fixes) and the two validating ops share `ArtifactReceipt.Pdf`, never a new receipt case; the `StructureAudit`/`PreflightAudit` values are content-addressed by their op keys, and the composition root decodes them to thread each `conformant` verdict onward.
- Growth: a new access op is one `AccessOp` row plus one `_ARM` entry; a new conformance clause is one `UaCheck` member plus one `(UaCheck, predicate)` row in the `failures` fold; a new standard PDF/UA role is absorbed upstream as one model `StructEltKind` member with zero change here; a new nesting rule is one `_NESTING` row; a new archival or print level is one `Literal` member.
- Boundary: born-PDF/A authoring stays at `document/emit#DOCUMENT` — ARCHIVE upgrades an ALREADY-emitted PDF in place; `pdf_oxide.DocumentBuilder.tagged_pdf_ua1()` is the from-scratch born-tagged author reserved for emit, never a second structure author over an existing PDF here; signing stays at `../exchange/conformance#CONFORMANCE`, security finishing at `document/egress#FINISH`, OCG authoring at `../export/layered#LAYERED`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from dataclasses import dataclass, field as dc_field
from enum import StrEnum
from io import BytesIO
from itertools import pairwise
from typing import TYPE_CHECKING, Final, Literal, Self, assert_never

import msgspec
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, field, structs

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.receipts import OPEN, Receipt, receipted

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt
from artifacts.document.model import (
    FigureNode,
    ForeignRole,
    FormulaNode,
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
lazy import pdf_oxide
lazy from pikepdf import Array, Dictionary, Name, NumberTree, String

if TYPE_CHECKING:
    import pikepdf

# --- [TYPES] ----------------------------------------------------------------------------

type Arm = Callable[["Access"], "AccessFact"]
type PdfaLevel = Literal["1a", "1b", "2a", "2b", "2u", "3a", "3b", "3u"]  # the ISO 19005 conformance levels `pdf_oxide.convert_to_pdf_a` admits
type PdfxLevel = Literal[
    "1a_2001", "3_2002", "4"
]  # the ISO 15930 PDF/X print-production levels `pdf_oxide.validate_pdf_x` admits (the exact accepted token set)


class AccessOp(StrEnum):
    TAG = "tag"
    AUDIT = "audit"
    ARCHIVE = "archive"
    PREFLIGHT = "preflight"  # the ISO 15930 PDF/X print-production close, the print-plane sibling of the ARCHIVE PDF/A close


class UaCheck(StrEnum):  # the ISO 14289-1 structural-conformance clauses the AUDIT closes over
    MARKED = "marked"  # /MarkInfo /Marked true
    STRUCT_TREE = "struct-tree"  # /StructTreeRoot present
    LANG = "lang"  # catalog /Lang present
    TITLE = "title"  # XMP dc:title + /ViewerPreferences /DisplayDocTitle true
    UA_ID = "ua-id"  # XMP pdfuaid:part identifier
    NOT_SUSPECT = "not-suspect"  # /MarkInfo /Suspects absent or false
    FIGURE_ALT = "figure-alt"  # every /Figure carries /Alt or /ActualText
    HEADING_NESTING = "heading-nesting"  # no skipped heading level in document order
    ROLE_MAP = "role-map"  # every non-standard /S mapped in /RoleMap
    STRUCTURE_NESTING = "structure-nesting"  # every constrained /S nests under a standard-legal parent role
    TABLE_REGULAR = "table-regular"  # every /Table carries /TR rows
    LIST_STRUCTURE = "list-structure"  # every /L carries /LI items
    LINK_CONTENT = "link-content"  # every /Link carries content (kids)
    PAGES_KEYED = "pages-keyed"  # every page keyed into the /ParentTree via /StructParents
    SYNTAX = "syntax"  # pikepdf `check_pdf_syntax` (qpdf --check) reports no structural warning
    TEXT_LAYER = "text-layer"  # every page carries an extractable text layer (`pdf_oxide.has_text_layer`) — content is real text, not image-only
    NO_XFA = "no-xfa"  # no dynamic XFA form (`pdf_oxide.has_xfa` false) — ISO 14289-1 §7.18.1 prohibits it
    ORACLE = "oracle"  # the independent `pdf_oxide.validate_pdf_ua` in-process oracle agrees (valid, zero errors)


class PreflightCheck(StrEnum):  # the ISO 15930 PDF/X print-production clauses the PREFLIGHT closes over
    PDFX_VALID = "pdfx-valid"  # the independent `pdf_oxide.validate_pdf_x(level)` oracle agrees (valid, zero errors)
    CLAIM_HONEST = "claim-honest"  # a document that DECLARES a /pdfxid (`pdfx_claim`) actually validates — the decorative-claim close


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class AccessFault:
    # the closed ADMISSION vocabulary `of` produces; the arm-level `pikepdf.PdfError`/`pdf_oxide` raise converts
    # to the runtime `BoundaryFault` at the `async_boundary` capsule, never this interior vocabulary.
    tag: Literal["incomplete"] = tag()
    incomplete: AccessOp = case()  # a TAG op admitted without its required `AccessParams.source` tree


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
    syntax_warnings: int  # pikepdf `check_pdf_syntax` structural-warning count; the SYNTAX clause reads its emptiness
    oracle_valid: bool  # the `pdf_oxide.validate_pdf_ua` external verdict `valid` boolean
    oracle_errors: int  # the external oracle's reported error count; the ORACLE clause reads valid AND zero errors
    oracle_warnings: int  # the external oracle's non-fatal warning count; captured whole (never a 2-of-3 slice), evidence not a clause since a warning is not a conformance failure
    structured_warnings: int  # the `pdf_oxide.structured_warnings()` structure-diagnostic count (`{category, page, message, spec_section}` rows), additional two-source structural evidence beside the clause set
    has_tree: bool  # the oracle's independent `has_structure_tree` confirmation the STRUCT_TREE clause reconciles against the pikepdf `/StructTreeRoot` read
    pages_with_text: int  # count of pages carrying an extractable text layer (`pdf_oxide.has_text_layer`); the TEXT_LAYER clause reads full coverage
    has_xfa: bool  # dynamic XFA presence (`pdf_oxide.has_xfa`); the NO_XFA clause reads its absence
    pdfa_claim: str  # the document's OWN declared PDF/A conformance (pikepdf XMP `pdfa_status`), evidence not a clause
    pdfx_claim: str  # the document's OWN declared PDF/X conformance (pikepdf XMP `pdfx_status`)
    failures: tuple[UaCheck, ...]

    @property
    def coverage(self) -> float:
        return 1.0 if self.figures == 0 else self.figures_with_alt / self.figures

    @property
    def conformant(self) -> bool:
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
            "syntax_warnings": str(self.syntax_warnings),
            "oracle_valid": str(self.oracle_valid),
            "oracle_errors": str(self.oracle_errors),
            "oracle_warnings": str(self.oracle_warnings),
            "structured_warnings": str(self.structured_warnings),
            "has_tree": str(self.has_tree),
            "pages_with_text": str(self.pages_with_text),
            "has_xfa": str(self.has_xfa),
            "pdfa_claim": self.pdfa_claim,
            "pdfx_claim": self.pdfx_claim,
            "failures": ",".join(self.failures),
            "conformant": str(self.conformant),
        }


class PreflightAudit(Struct, frozen=True, gc=False):
    # `conformant` gates the PAdES/print-issue close.
    level: PdfxLevel
    pdfx_valid: bool  # the `pdf_oxide.validate_pdf_x(level)` external verdict `valid` boolean
    pdfx_errors: int  # the oracle's reported error count; the PDFX_VALID clause reads valid AND zero errors
    pdfx_warnings: int  # the oracle's non-fatal warning count; evidence, not a clause
    pdfx_claim: (
        str  # the document's OWN declared PDF/X conformance (pikepdf XMP `pdfx_status`)
    )
    structured_warnings: int  # the `pdf_oxide.structured_warnings()` diagnostic count folded as print-side evidence
    failures: tuple[PreflightCheck, ...]

    @property
    def conformant(self) -> bool:  # empty only when the oracle passes AND a declared claim is honest
        return not self.failures

    def facts(self) -> dict[str, str]:  # the scalar projection a span/log consumer reads off the decoded content-addressed verdict
        return {
            "level": self.level,
            "pdfx_valid": str(self.pdfx_valid),
            "pdfx_errors": str(self.pdfx_errors),
            "pdfx_warnings": str(self.pdfx_warnings),
            "pdfx_claim": self.pdfx_claim,
            "structured_warnings": str(self.structured_warnings),
            "failures": ",".join(self.failures),
            "conformant": str(self.conformant),
        }


class AccessParams(Struct, frozen=True, kw_only=True):
    # each arm reads its own slice: `source` the tree TAG lowers, `lang`/`title`/`ua_part` the catalog requirements, `pdfa_level` the ARCHIVE target.
    source: StructureNode | None = None
    lang: LangTag | None = None
    title: str = ""
    ua_part: Literal[1, 2] = 1  # PDF/UA-1 (ISO 14289-1) or PDF/UA-2 (ISO 14289-2); the only defined parts
    pdfa_level: PdfaLevel = "2b"  # the ARCHIVE target; an out-of-range level is unrepresentable rather than a runtime-rejected string
    pdfx_level: PdfxLevel = "4"  # the PREFLIGHT PDF/X target (PDF/X-4 the modern print standard); an out-of-range level is unrepresentable


class AccessFact(Struct, frozen=True):
    # `audit` is the AUDIT verdict the content key addresses; `applied`/`residual` the ARCHIVE convert action/error counts.
    data: bytes
    pages: int = 0
    elements: int = 0
    figures: int = 0
    applied: int = 0
    residual: int = 0
    audit: StructureAudit | None = None
    preflight: PreflightAudit | None = None  # the PREFLIGHT PDF/X print-production verdict the content key addresses


class Access(Struct, frozen=True):
    op: AccessOp
    pdf: bytes
    params: AccessParams = field(default_factory=AccessParams)
    fact: AccessFact | None = None

    @classmethod
    def of(cls, op: AccessOp, pdf: bytes, /, *, params: AccessParams = AccessParams()) -> Result[Self, AccessFault]:
        ready = op is not AccessOp.TAG or params.source is not None  # only TAG needs a source tree; AUDIT/ARCHIVE read the emitted bytes
        return Ok(cls(op=op, pdf=pdf, params=params)) if ready else Error(AccessFault(incomplete=op))

    def _stepped(self) -> Self:
        return structs.replace(self, fact=_ARM[self.op](self))

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=float(len(self.pdf)))

    @property
    def _key(self) -> ContentKey:
        return ContentIdentity.of(f"access-{self.op}", (self.op, self.pdf, self.params), policy=CANONICAL_POLICY)

    @receipted(
        OPEN
    )  # the keep-all redaction policy the runtime receipts owner exports (never a re-minted per-file `Redaction`); drains `contribute` off the stepped owner, emits via Signals.emit_async
    async def _authored(self) -> Self:
        # the GIL-releasing native folds cross the runtime thread lane, never a folder-minted limiter.
        crossed = await LanePolicy.offload(self._stepped, modality=Modality.THREAD, retry=RetryClass.OCCT)
        return crossed.default_with(lambda fault: _access_raise(fault))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the terminal receipt threads the PRE-RUN input key so receipt.slot == node.key.
        return (await async_boundary(f"access.{self.op}", self._authored)).map(lambda done: done._receipt(self._key))

    def _receipt(self, key: ContentKey, /) -> ArtifactReceipt:
        fact = self.fact if self.fact is not None else AccessFact(data=self.pdf)
        match (
            self.op
        ):  # the two producing ops share the Egress finishing case, the two validating ops the byte-only Pdf case — never a fifteenth receipt
            case AccessOp.TAG:
                emitted = ArtifactReceipt.Egress(key, len(fact.data), fact.pages, 0, fact.elements, fact.figures)
            case AccessOp.ARCHIVE:
                emitted = ArtifactReceipt.Egress(key, len(fact.data), fact.pages, 0, fact.applied, fact.residual)
            case AccessOp.AUDIT | AccessOp.PREFLIGHT:  # both validators content-address their verdict and land the byte-only Pdf case
                emitted = ArtifactReceipt.Pdf(key, len(fact.data), fact.pages)
            case _ as unreachable:
                assert_never(unreachable)
        return emitted

    def contribute(self) -> Iterable[Receipt]:
        if self.fact is None:  # contribute rides the stepped owner the weave returned, never the seed
            return
        yield from self._receipt(self._key).contribute()


def _access_raise(fault: object) -> "DocumentAccess":
    # terminal collapse at the authoring boundary: an offload fault reconstructs the raise the node's rail folds.
    raise ValueError(str(fault))


# --- [CONSTANTS] ------------------------------------------------------------------------

# `_ELT` decodes a read `/S` Name to its model member AND its key set IS the standard-structure membership the /RoleMap
# completeness check reads; `_CATEGORY` projects each member through `role_category`, so both track the model enum.
_ELT: Final[Map[str, StructEltKind]] = Map.of_seq((f"/{elt.value}", elt) for elt in StructEltKind)
_CATEGORY: Final[Map[StructEltKind, tuple[StructCategory, int]]] = Map.of_seq((
    (elt, role_category(StandardRole(elt=elt))) for elt in StructEltKind
))
# the standard structure-nesting policy this AUDIT owns: each constrained role's legal parent set per ISO 14289
# (list/table grouping + the East-Asian ruby/warichu assemblies the model vocabulary carries); a role absent from
# the table nests anywhere, a foreign role is unconstrained.
_NESTING: Final[Map[StructEltKind, frozenset[StructEltKind]]] = Map.of_seq([
    (StructEltKind.LI, frozenset({StructEltKind.L})),
    (StructEltKind.LBL, frozenset({StructEltKind.LI})),
    (StructEltKind.LBODY, frozenset({StructEltKind.LI})),
    (StructEltKind.THEAD, frozenset({StructEltKind.TABLE})),
    (StructEltKind.TBODY, frozenset({StructEltKind.TABLE})),
    (StructEltKind.TFOOT, frozenset({StructEltKind.TABLE})),
    (StructEltKind.TR, frozenset({StructEltKind.TABLE, StructEltKind.THEAD, StructEltKind.TBODY, StructEltKind.TFOOT})),
    (StructEltKind.TH, frozenset({StructEltKind.TR})),
    (StructEltKind.TD, frozenset({StructEltKind.TR})),
    (StructEltKind.RB, frozenset({StructEltKind.RUBY})),  # ruby base text nests under its `Ruby` assembly
    (StructEltKind.RT, frozenset({StructEltKind.RUBY})),  # ruby annotation text
    (StructEltKind.RP, frozenset({StructEltKind.RUBY})),  # ruby fallback punctuation
    (StructEltKind.WT, frozenset({StructEltKind.WARICHU})),  # warichu text nests under its `Warichu` assembly
    (StructEltKind.WP, frozenset({StructEltKind.WARICHU})),  # warichu punctuation
])
_AUDIT_ENCODER: Final = msgspec.msgpack.Encoder(order="deterministic")  # the content key addresses this stable encoding
_DECIMAL_PRECISION: Final = 8  # pinned qpdf real-number precision so a re-emit serializes coordinates identically — the content-addressing precondition beside `deterministic_id`


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
    if (illustrated := next((kid for kid in children(node) if isinstance(kid, FigureNode | FormulaNode)), None)) is not None:
        if isinstance(illustrated, FigureNode):
            build.figures += 1  # a formula is alt-bearing but not a figure — the receipt count stays figure-only
        if alt := alt_of(illustrated)[0]:
            elem.Alt = String(alt)  # ISO 14289: a /Figure AND a /Formula both carry the /Alt text equivalent `alt_of` projects
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
        struct_root = pdf.make_indirect(Dictionary(Type=Name.StructTreeRoot, K=Array([]), ParentTreeNextKey=0, RoleMap=Dictionary()))
        pdf.Root.StructTreeRoot = struct_root
        build = _Author()
        struct_root.K = Array([_elem(pdf, source, struct_root, build)]) if source is not None else Array([])
        role_map = Dictionary()
        for foreign, standard in build.role_map.items():
            role_map[Name("/" + foreign)] = Name("/" + standard)
        struct_root.RoleMap = role_map
        parent_tree = NumberTree.new(
            pdf
        )  # the /ParentTree IS a PDF number-tree; the modeled `NumberTree` mapping-view owner replaces the hand-assembled flat `Nums` Array
        for page_key in sorted(build.slots):
            pdf.pages[page_key].obj.StructParents = page_key
            parent_tree[page_key] = Array(build.slots[page_key])
        struct_root.ParentTree = parent_tree.obj
        struct_root.ParentTreeNextKey = max(build.slots) + 1 if build.slots else 0
        pikepdf.settings.set_decimal_precision(_DECIMAL_PRECISION)  # pin real-number precision before the content-addressed emit
        sink = BytesIO()
        pdf.save(
            sink, deterministic_id=True
        )  # deterministic /ID: two identical-`StructureNode`-tree TAG runs content-key IDENTICALLY, the reuse-fabric elision the receipt spine depends on
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


def _walk(root: "pikepdf.Object", tally: _Tally, /) -> None:
    # the frontier pushes children before siblings, keeping the document order the `tally.levels` monotonicity read depends on.
    stack: Block[tuple["pikepdf.Object", int, StructEltKind | None]] = Block.singleton((root, 1, None))
    while not stack.is_empty():  # Exemption: iterative frontier — the untrusted `/K` spine forfeits the recursive form
        (elem, depth, parent), stack = stack.head(), stack.tail()
        tally.elements += 1
        tally.depth = max(tally.depth, depth)
        role = str(elem.get(Name.S, ""))
        tally.roles.add(role)
        elt = _ELT.try_find(role).default_value(None)  # the model member behind the read /S, or None for a foreign role (nesting-exempt)
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
        branches = Block.of_seq(
            (kid, depth + 1, elt) for kid in members if isinstance(kid, pikepdf.Dictionary) and kid.get(Name.Type) == Name.StructElem
        )
        stack = branches.append(stack)


def _audit(access: "Access") -> AccessFact:
    with pikepdf.open(BytesIO(access.pdf)) as pdf:  # deterministic close; every clause resolves to a plain value before the handle frees
        root = pdf.Root
        mark_info = root.get(Name.MarkInfo, Dictionary())
        struct_root = root.get(Name.StructTreeRoot)
        role_map = struct_root.get(Name.RoleMap, Dictionary()) if struct_root is not None else Dictionary()
        tally = _Tally()
        for kid in struct_root.get(Name.K, Array([])) if struct_root is not None else ():
            if isinstance(kid, pikepdf.Dictionary) and kid.get(Name.Type) == Name.StructElem:
                _walk(kid, tally)
        pages = len(pdf.pages)
        keyed = sum(Name.StructParents in page.obj for page in pdf.pages)
        syntax = len(pdf.check_pdf_syntax())  # qpdf --check structural-syntax warnings; a well-formed PDF is the ISO 14289 precondition
        mapped = {str(name) for name in role_map.keys()}  # the foreign /S Names the /RoleMap registers
        with pdf.open_metadata(set_pikepdf_as_editor=False, update_docinfo=False) as xmp:  # read-only audit never mutates the audited bytes
            ua_id, has_title = "pdfuaid:part" in xmp, "dc:title" in xmp
            pdfa_claim, pdfx_claim = xmp.pdfa_status, xmp.pdfx_status  # the document's OWN declared PDF/A·PDF/X claim (evidence, not a UaCheck)
        # every pikepdf-touching predicate resolved to a plain bool/int here, so no qpdf `Object` escapes the borrow window
        marked = bool(mark_info.get(Name.Marked, False))
        has_struct = struct_root is not None
        has_lang = Name.Lang in root
        title_ok = has_title and bool(root.get(Name.ViewerPreferences, Dictionary()).get(Name.DisplayDocTitle, False))
        not_suspect = not bool(mark_info.get(Name.Suspects, False))
        unmapped = any(role not in _ELT and role not in mapped for role in tally.roles)  # standard membership IS `_ELT`'s key set
        monotone = all(b - a <= 1 for a, b in pairwise(tally.levels) if b > a)
        role_map_n = len(mapped)
    with pdf_oxide.PdfDocument.from_bytes(access.pdf) as oracle:  # independent in-process Rust oracle, deterministic close via __exit__
        verdict = oracle.validate_pdf_ua()  # {'valid': bool, 'errors': list, 'warnings': list} — the veraPDF-grade cross-check, read once
        oracle_valid, oracle_errors, oracle_warnings = bool(verdict["valid"]), len(verdict["errors"]), len(verdict["warnings"])
        has_tree = oracle.has_structure_tree()  # the independent structure-tree confirmation STRUCT_TREE reconciles against the pikepdf read
        xfa = oracle.has_xfa()  # a dynamic XFA form is a PDF/UA-1 §7.18.1 violation
        text_pages = sum(oracle.has_text_layer(page_index) for page_index in range(int(oracle.page_count)))  # pages with real extractable text
        structured = len(
            oracle.structured_warnings()
        )  # the oracle's `{category, page, message, spec_section}` structure diagnostics, folded as additional two-source evidence
    failures = tuple(
        check
        for check, ok in (
            (UaCheck.MARKED, marked),
            (UaCheck.STRUCT_TREE, has_struct and has_tree),  # two-source: pikepdf sees /StructTreeRoot AND the oracle confirms a structure tree
            (UaCheck.LANG, has_lang),
            (UaCheck.TITLE, title_ok),
            (UaCheck.UA_ID, ua_id),
            (UaCheck.NOT_SUSPECT, not_suspect),
            (UaCheck.FIGURE_ALT, tally.figures == tally.figures_with_alt),
            (UaCheck.HEADING_NESTING, monotone),
            (UaCheck.ROLE_MAP, not unmapped),
            (UaCheck.STRUCTURE_NESTING, tally.misnested == 0),
            (UaCheck.TABLE_REGULAR, tally.tables == 0 or tally.table_rows > 0),
            (UaCheck.LIST_STRUCTURE, tally.lists == 0 or tally.list_items > 0),
            (UaCheck.LINK_CONTENT, tally.links == tally.links_with_content),
            (UaCheck.PAGES_KEYED, keyed == pages),
            (UaCheck.SYNTAX, syntax == 0),
            (UaCheck.TEXT_LAYER, text_pages == pages),  # every page has real text — an image-only page fails PDF/UA even when tagged
            (UaCheck.NO_XFA, not xfa),  # a dynamic XFA form is prohibited
            (UaCheck.ORACLE, oracle_valid and oracle_errors == 0),  # the independent oracle catches what the clause set cannot enumerate
        )
        if not ok
    )
    audit = StructureAudit(
        elements=tally.elements,
        depth=tally.depth,
        pages=pages,
        pages_keyed=keyed,
        figures=tally.figures,
        figures_with_alt=tally.figures_with_alt,
        headings=tally.headings,
        tables=tally.tables,
        lists=tally.lists,
        links=tally.links,
        role_map=role_map_n,
        misnested=tally.misnested,
        syntax_warnings=syntax,
        oracle_valid=oracle_valid,
        oracle_errors=oracle_errors,
        oracle_warnings=oracle_warnings,
        structured_warnings=structured,
        has_tree=has_tree,
        pages_with_text=text_pages,
        has_xfa=xfa,
        pdfa_claim=pdfa_claim,
        pdfx_claim=pdfx_claim,
        failures=failures,
    )
    return AccessFact(_AUDIT_ENCODER.encode(audit), pages=pages, audit=audit)


def _archive(access: "Access") -> AccessFact:
    level = access.params.pdfa_level
    with pdf_oxide.PdfDocument.from_bytes(access.pdf) as doc:  # deterministic close via __exit__, never GC-reaped
        outcome = doc.convert_to_pdf_a(level)  # {'success': bool, 'actions': list[str], 'errors': list[str]}, upgraded in place
        verified = doc.validate_pdf_a(
            level
        )  # {'valid': bool, 'level': str, 'errors': list, 'warnings': list} — the independent post-convert oracle on the SAME upgraded handle
        residual = len(outcome["errors"]) + len(
            verified["errors"]
        )  # two-source archival close: the converter self-report PLUS the oracle's post-upgrade conformance verdict, never a lone self-trust
        return AccessFact(doc.to_bytes(), pages=int(doc.page_count), applied=len(outcome["actions"]), residual=residual)


def _preflight(access: "Access") -> AccessFact:
    level = access.params.pdfx_level
    with (
        pikepdf.open(BytesIO(access.pdf)) as pdf,
        pdf.open_metadata(set_pikepdf_as_editor=False, update_docinfo=False) as xmp,
    ):  # read the OWN /pdfxid, read-only, never mutating the audited bytes
        pdfx_claim = str(xmp.pdfx_status)  # the document's OWN declared PDF/X conformance, resolved to a plain str before the qpdf handle frees
    with pdf_oxide.PdfDocument.from_bytes(access.pdf) as oracle:  # independent in-process Rust PDF/X oracle, deterministic close via __exit__
        verdict = oracle.validate_pdf_x(
            level
        )  # {'valid': bool, 'level': str, 'errors': list, 'warnings': list} — read once, every value resolved before the handle frees
        valid, errors, warnings = bool(verdict["valid"]), len(verdict["errors"]), len(verdict["warnings"])
        structured, pages = len(oracle.structured_warnings()), int(oracle.page_count)
    failures = tuple(
        check
        for check, ok in (
            (PreflightCheck.PDFX_VALID, valid and errors == 0),  # the independent oracle passes
            (
                PreflightCheck.CLAIM_HONEST,
                not pdfx_claim or valid,
            ),  # a declared /pdfxid the oracle refutes is a false claim — the decorative-evidence trap closed
        )
        if not ok
    )
    audit = PreflightAudit(
        level=level,
        pdfx_valid=valid,
        pdfx_errors=errors,
        pdfx_warnings=warnings,
        pdfx_claim=pdfx_claim,
        structured_warnings=structured,
        failures=failures,
    )
    return AccessFact(_AUDIT_ENCODER.encode(audit), pages=pages, preflight=audit)


# --- [COMPOSITION] ----------------------------------------------------------------------
_ARM: Final[Map[AccessOp, Arm]] = Map.of_seq([
    (AccessOp.TAG, _tag),
    (AccessOp.AUDIT, _audit),
    (AccessOp.ARCHIVE, _archive),
    (AccessOp.PREFLIGHT, _preflight),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [MCID_PROPLIST]-[OPEN]: does `ContentStreamBuilder.begin_marked_content_proplist(mctype, mcid)` carry the explicit `<</MCID n>>` on the installed pikepdf build, so the post-hoc canvas path stamps a deterministic MCID rather than relying on document-order convention alone; verify against the folder `.api/pikepdf.md` entrypoint rows.
