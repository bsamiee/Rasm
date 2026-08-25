# [PY_ARTIFACTS_TAGGED]

PDF/UA (ISO 14289) structure, ISO 15930 PDF/X print production, and ISO 19005 archival conversion close over one document rail: `Access` authors the marked-content structure tree into an emitted PDF, audits its conformance, upgrades to the archival profile, and preflights the PDF/X claim through one closed `AccessOp` over `_ARM`. Validation combines explainable owner-local clauses with the independent MIT/Apache `pdf_oxide` oracle; archival conversion keeps the converter `success` verdict and the post-convert validation verdict as distinct `ArchiveCheck` clauses, so an empty error list never substitutes for either boolean.

Structure vocabulary is consumed from `document/model#NODE`: `DocumentNode`, the role family, and the `role_of`/`role_category`/`alt_of`/`children`/`standard_for` projections derive the `/S` algebra once from `StructEltKind`. A born-tagged emitter (`document/emit#DOCUMENT` UA arms) arrives with page-local MCIDs already marked in document order; an unmarked source gains them through `_stamped`'s explicit-proplist `BDC`/`EMC` pass per text block, and `_tag` then rejects any page whose marked MCID set differs from the structure leaves before binding `/StructParents` into `/ParentTree`. `StructureAudit.conformant` threads into the `exchange/conformance#CONFORMANCE` `SourceConformance.structural` half of `AuditSpec.source`.

## [01]-[INDEX]

- [02]-[ACCESS]: the tag/audit/archive/preflight conformance close over `_ARM`, every verdict combining local clauses with an independent validation observation.

## [02]-[ACCESS]

- Owner: `Access` — `_ARM` maps each op to its single `AccessFact`-returning arm with zero `match` sprawl, the closed `StrEnum` membership total over the table by construction; each close's audit value carries every clause input and its `failures` derive through one clause table under the shared `_failed` fold, so a decoded content-addressed audit re-derives its own verdict; `pikepdf` owns the qpdf object model and the XMP context, `pdf_oxide` the independent oracle under its deterministic-close capsule, and the model owns the tree algebra this page only reads.
- Cases: TAG writes the catalog requirements ISO 14289 mandates beyond the tree (`/Lang`, `/DisplayDocTitle`, XMP `pdfuaid:part` + `dc:title`) and re-emits under `Pdf.save(deterministic_id=True)` while a scoped lock pins and restores `settings.set_decimal_precision`; AUDIT includes per-page `has_text_layer`, `has_xfa`, exact MCID-to-`/ParentTree` binding, and per-owner table/list regularity, so one valid structure never masks a malformed sibling; `ua_part=2` adds `UA2_VERSION` and `UA2_NAMESPACES`; the WTPDF declaration pair closes the well-tagged interchange claim — a declared accessibility conformance holds only under a part-2 audit whose UA oracle, PDF 2.0 version, and structure namespaces all pass (`validate_pdf_ua` carries no part argument, so part-2 specificity stays local evidence), a declared reuse conformance under local structure plus PDF 2.0 evidence because no reuse oracle exists, and each `pdfd:conformsTo` level admits its erratum-canonical and as-published URI spellings; ARCHIVE folds converter self-report and `validate_pdf_a` into `ArchiveAudit`; PREFLIGHT turns the declared claim, `/OutputIntents`, and per-page `TrimBox`/`ArtBox` geometry into a clause verdict.
- Auto: `_audit` walks the UNTRUSTED `/StructTreeRoot` `/K` spine through the depth-safe `Block` frontier — never native recursion an adversarial nesting depth overflows — and TAG authors through the same discipline, a pre-order frontier whose per-parent child lists assemble the `/K` arrays after the sweep, because a lens-recovered source tree carries the same adversarial depth; every `pikepdf`-touching clause predicate resolves to a plain value BEFORE the handle frees, and metadata reads through the read-only `open_metadata(set_pikepdf_as_editor=False, update_docinfo=False)` form that never mutates the bytes it audits; the `pdfd:declarations` bag defeats the pikepdf mapping view, so the WTPDF `conformsTo` URIs read off the raw `/Metadata` stream's element tree in all three RDF spellings — element text, shorthand attribute, `rdf:resource` reference; the `/ParentTree` IS a PDF number-tree, owned by the modeled `pikepdf.NumberTree.new(pdf)` mapping-view, never a hand-assembled flat `Nums` array; `pikepdf` exposes no high-level `StructTreeRoot` helper, so the raw `Object`-model spike is the real surface and a phantom `pdf.add_structure_tree()` convenience is the rejected form.
- Output: `_emit` returns the settled `AccessFact`; `StructureAudit`, `PreflightAudit`, and `ArchiveAudit` remain the content-addressed verdict carriers, while `Metrics.record` records byte volume and `Journal.record` persists the regulatory clause changes.
- Growth: a new access op is one `AccessRequest` case, one `AccessOp` row, and one `_ARM` entry; a new conformance clause is one `UaCheck`/`PreflightCheck`/`ArchiveCheck` member plus one predicate row in its `_UA_CLAUSES`/`_PREFLIGHT_CLAUSES`/`_ARCHIVE_CLAUSES` table (a clause needing fresh evidence also lands its audit field); a new standard PDF/UA role is one model `StructEltKind` member; a new nesting rule is one `_NESTING` row; a new archival or print level is one `Literal` member.
- Boundary: born-PDF/A authoring stays at `document/emit#DOCUMENT` — ARCHIVE upgrades an ALREADY-emitted PDF in place; `pdf_oxide.DocumentBuilder.tagged_pdf_ua1()` is the from-scratch born-tagged author reserved for emit, never a second structure author over an existing PDF here; signing stays at `exchange/conformance#CONFORMANCE`, security finishing at `document/egress#FINISH`, OCG authoring at `export/layered#LAYERED`.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import Callable
from dataclasses import dataclass, field as dc_field
from enum import StrEnum
from functools import partial
from io import BytesIO
from itertools import pairwise
from threading import Lock
from typing import TYPE_CHECKING, Final, Literal, Self, assert_never

import msgspec
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, structs

from rasm.artifacts.core.hooks import BYTE_VOLUME, DOMAIN, ArtifactKind, ArtifactsLeg
from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.document.model import (
    DocumentNode,
    FigureNode,
    ForeignRole,
    FormulaNode,
    LangTag,
    Lapse,
    RunNode,
    StandardRole,
    StructCategory,
    StructEltKind,
    StructureNode,
    alt_of,
    children,
    hardened_parse,
    lapsed,
    role_category,
    role_of,
    standard_for,
)
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import TRANSIENT, Catch, FaultRow, RuntimeRail, async_boundary, rostered
from rasm.runtime.journal import Actor, Assigned, AuditFact, Change, Journal, Party, Retain
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.metrics import Metrics
from rasm.runtime.workers import Kernel, KernelTrait

lazy import pikepdf
lazy import pdf_oxide
lazy from lxml import etree
lazy from pikepdf import Array, Dictionary, Name, NumberTree, String

if TYPE_CHECKING:
    import pikepdf

# --- [TYPES] ----------------------------------------------------------------------------

type Arm = Callable[["Access"], "AccessFact"]
type PdfaLevel = Literal["1a", "1b", "2a", "2b", "2u", "3a", "3b", "3u"]
type PdfxLevel = Literal[
    "1a_2001", "3_2002", "4"
]


class AccessOp(StrEnum):
    TAG = "tag"
    AUDIT = "audit"
    ARCHIVE = "archive"
    PREFLIGHT = "preflight"


class UaCheck(StrEnum):
    MARKED = "marked"
    STRUCT_TREE = "struct-tree"
    LANG = "lang"
    TITLE = "title"
    UA_ID = "ua-id"
    NOT_SUSPECT = "not-suspect"
    FIGURE_ALT = "figure-alt"
    HEADING_NESTING = "heading-nesting"
    ROLE_MAP = "role-map"
    STRUCTURE_NESTING = "structure-nesting"
    TABLE_REGULAR = "table-regular"
    LIST_STRUCTURE = "list-structure"
    LINK_CONTENT = "link-content"
    PAGES_KEYED = "pages-keyed"
    SYNTAX = "syntax"
    TEXT_LAYER = "text-layer"
    NO_XFA = "no-xfa"
    UA2_VERSION = "ua2-version"
    UA2_NAMESPACES = "ua2-namespaces"
    WTPDF_ACCESSIBILITY = "wtpdf-accessibility"
    WTPDF_REUSE = "wtpdf-reuse"
    ORACLE = "oracle"


class PreflightCheck(StrEnum):
    PDFX_VALID = "pdfx-valid"
    CLAIM_HONEST = "claim-honest"
    OUTPUT_INTENT = "output-intent"
    PAGE_BOXES = "page-boxes"


class ArchiveCheck(StrEnum):
    CONVERTED = "converted"
    ORACLE = "oracle"


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class AccessFault:
    tag: Literal["empty"] = tag()
    empty: None = case()


# --- [MODELS] ---------------------------------------------------------------------------



class StructureAudit(Struct, frozen=True, gc=False):
    ua_part: int
    elements: int
    depth: int
    pages: int
    pages_keyed: int
    pages_marked: int
    mcids: int
    mcids_keyed: int
    figures: int
    figures_with_alt: int
    headings: int
    headings_monotone: bool
    tables: int
    tables_irregular: int
    lists: int
    lists_irregular: int
    links: int
    links_with_content: int
    role_map: int
    roles_unmapped: int
    misnested: int
    marked: bool
    has_struct: bool
    has_lang: bool
    title_ok: bool
    ua_id: bool
    not_suspect: bool
    namespaced: bool
    syntax_warnings: int
    oracle_valid: bool
    oracle_errors: int
    oracle_warnings: int
    structured_warnings: int
    has_tree: bool
    pages_with_text: int
    pages_accessible: int
    has_xfa: bool
    pdf_version: tuple[int, int]
    pdfa_claim: str
    pdfx_claim: str
    wtpdf_accessibility: bool
    wtpdf_reuse: bool
    failures: tuple[UaCheck, ...]

    @property
    def coverage(self) -> float:
        return 1.0 if self.figures == 0 else self.figures_with_alt / self.figures

    @property
    def conformant(self) -> bool:
        return not self.failures

    def facts(self) -> dict[str, str]:
        return {
            "ua_part": str(self.ua_part),
            "elements": str(self.elements),
            "depth": str(self.depth),
            "pages": str(self.pages),
            "pages_keyed": str(self.pages_keyed),
            "pages_marked": str(self.pages_marked),
            "mcids": str(self.mcids),
            "mcids_keyed": str(self.mcids_keyed),
            "figures": str(self.figures),
            "coverage": f"{self.coverage:.6f}",
            "headings_monotone": str(self.headings_monotone),
            "tables_irregular": str(self.tables_irregular),
            "lists_irregular": str(self.lists_irregular),
            "links_with_content": str(self.links_with_content),
            "role_map": str(self.role_map),
            "roles_unmapped": str(self.roles_unmapped),
            "misnested": str(self.misnested),
            "marked": str(self.marked),
            "has_struct": str(self.has_struct),
            "has_lang": str(self.has_lang),
            "title_ok": str(self.title_ok),
            "ua_id": str(self.ua_id),
            "not_suspect": str(self.not_suspect),
            "namespaced": str(self.namespaced),
            "syntax_warnings": str(self.syntax_warnings),
            "oracle_valid": str(self.oracle_valid),
            "oracle_errors": str(self.oracle_errors),
            "oracle_warnings": str(self.oracle_warnings),
            "structured_warnings": str(self.structured_warnings),
            "has_tree": str(self.has_tree),
            "pages_with_text": str(self.pages_with_text),
            "pages_accessible": str(self.pages_accessible),
            "has_xfa": str(self.has_xfa),
            "pdf_version": f"{self.pdf_version[0]}.{self.pdf_version[1]}",
            "pdfa_claim": self.pdfa_claim,
            "pdfx_claim": self.pdfx_claim,
            "wtpdf_accessibility": str(self.wtpdf_accessibility),
            "wtpdf_reuse": str(self.wtpdf_reuse),
            "failures": ",".join(self.failures),
            "conformant": str(self.conformant),
        }


class PreflightAudit(Struct, frozen=True, gc=False):
    level: PdfxLevel
    pdfx_valid: bool
    pdfx_errors: int
    pdfx_warnings: int
    pdfx_claim: str
    output_intents: int
    pages: int
    pages_boxed: int
    structured_warnings: int
    failures: tuple[PreflightCheck, ...]

    @property
    def conformant(self) -> bool:
        return not self.failures

    def facts(self) -> dict[str, str]:
        return {
            "level": self.level,
            "pdfx_valid": str(self.pdfx_valid),
            "pdfx_errors": str(self.pdfx_errors),
            "pdfx_warnings": str(self.pdfx_warnings),
            "pdfx_claim": self.pdfx_claim,
            "output_intents": str(self.output_intents),
            "pages": str(self.pages),
            "pages_boxed": str(self.pages_boxed),
            "structured_warnings": str(self.structured_warnings),
            "failures": ",".join(self.failures),
            "conformant": str(self.conformant),
        }


class ArchiveAudit(Struct, frozen=True, gc=False):
    level: PdfaLevel
    converted: bool
    applied: int
    converter_errors: int
    oracle_valid: bool
    oracle_errors: int
    oracle_warnings: int
    failures: tuple[ArchiveCheck, ...]

    @property
    def conformant(self) -> bool:
        return not self.failures

    def facts(self) -> dict[str, str]:
        return {
            "level": self.level,
            "converted": str(self.converted),
            "applied": str(self.applied),
            "converter_errors": str(self.converter_errors),
            "oracle_valid": str(self.oracle_valid),
            "oracle_errors": str(self.oracle_errors),
            "oracle_warnings": str(self.oracle_warnings),
            "failures": ",".join(self.failures),
            "conformant": str(self.conformant),
        }


@tagged_union(frozen=True)
class AccessRequest:
    tag: Literal["tagged", "audit", "archive", "preflight"] = tag()
    tagged: tuple[DocumentNode, LangTag | None, str, Literal[1, 2]] = case()
    audit: Literal[1, 2] = case()
    archive: PdfaLevel = case()
    preflight: PdfxLevel = case()


@tagged_union(frozen=True)
class AccessFact:
    tag: Literal["produced", "audit", "archive", "preflight"] = tag()
    produced: tuple[bytes, int, int, int] = case()
    audit: tuple[bytes, int, StructureAudit] = case()
    archive: tuple[bytes, int, int, int, ArchiveAudit] = case()
    preflight: tuple[bytes, int, PreflightAudit] = case()


class Access(Struct, frozen=True):
    request: AccessRequest
    pdf: bytes
    lane: LanePolicy
    key: ContentKey
    fact: AccessFact | None = None

    @property
    def op(self) -> AccessOp:
        return AccessOp.TAG if self.request.tag == "tagged" else AccessOp(self.request.tag)

    @classmethod
    def of(cls, request: AccessRequest, pdf: bytes, /, *, lane: LanePolicy) -> Result[Self, AccessFault]:
        return Ok(cls(request=request, pdf=pdf, lane=lane, key=_minted(request, pdf))) if pdf else Error(AccessFault(empty=None))

    def _stepped(self) -> Self:
        return structs.replace(self, fact=_ARM[self.op](self))

    def emit(self, /) -> ArtifactWork[AccessFact]:
        return ArtifactWork(key=self.key, work=partial(self._emit, self.key), parents=(), admission=Admission(keyed=None), cost=float(len(self.pdf)))

    async def _authored(self) -> Self:
        crossed = await self.lane.offload(Kernel.of(self._stepped, KernelTrait.RELEASING))
        return crossed.default_with(lapsed)

    async def _emit(self, key: ContentKey, /) -> RuntimeRail[AccessFact]:
        match await async_boundary(ACCESS_AUTHOR, self._authored, catch=_AUTHOR_RAISES):
            case Result(tag="ok", ok=done):
                assert done.fact is not None
                fact = done.fact
                match fact:
                    case AccessFact(tag="produced", produced=(data, _pages, _elements, _figures)) | AccessFact(
                        tag="archive", archive=(data, _pages, _applied, _residual, _audit)
                    ):
                        kind: ArtifactKind = "egress"
                    case AccessFact(tag="audit", audit=(data, _pages, _audit)) | AccessFact(
                        tag="preflight", preflight=(data, _pages, _audit)
                    ):
                        kind = "pdf"
                    case _ as unreachable:
                        assert_never(unreachable)
                audited = AuditFact(
                    action=f"artifacts.document.{self.op.value}",
                    actor=Party(kind=Actor.SERVICE, key="artifacts"),
                    target=Party(kind=kind, key=key.hex),
                    retention=Retain.REGULATORY,
                    change=done._verdict,
                )
                Metrics.record({BYTE_VOLUME: float(len(data))}, domain=DOMAIN, kind=kind, scope=self.lane.scope)
                await Journal.record((audited,), scope=self.lane.scope)
                return Ok(fact)
            case refused:
                return Error(refused.error)

    @property
    def _verdict(self) -> tuple[Change, ...]:
        match self.fact:
            case (
                AccessFact(tag="audit", audit=(_, _, audit))
                | AccessFact(tag="preflight", preflight=(_, _, audit))
                | AccessFact(tag="archive", archive=(_, _, _, _, audit))
            ):
                failures = audit.failures
            case _:
                failures = ()
        return (Assigned(path="/op", next=self.op.value), *(Assigned(path=f"/failed/{check.value}", next="true") for check in failures))

def _minted(request: AccessRequest, pdf: bytes, /) -> ContentKey:
    op = AccessOp.TAG if request.tag == "tagged" else AccessOp(request.tag)
    return ContentIdentity.key(f"access-{op}", _AUDIT_ENCODER.encode((request, pdf)))



# --- [CONSTANTS] ------------------------------------------------------------------------

_ELT: Final[Map[str, StructEltKind]] = Map.of_seq((f"/{elt.value}", elt) for elt in StructEltKind)
_CATEGORY: Final[Map[StructEltKind, tuple[StructCategory, int]]] = Map.of_seq((
    (elt, role_category(StandardRole(elt=elt))) for elt in StructEltKind
))
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
    (StructEltKind.RB, frozenset({StructEltKind.RUBY})),
    (StructEltKind.RT, frozenset({StructEltKind.RUBY})),
    (StructEltKind.RP, frozenset({StructEltKind.RUBY})),
    (StructEltKind.WT, frozenset({StructEltKind.WARICHU})),
    (StructEltKind.WP, frozenset({StructEltKind.WARICHU})),
])
_ROW_GROUPS: Final[frozenset[StructEltKind]] = frozenset({StructEltKind.THEAD, StructEltKind.TBODY, StructEltKind.TFOOT})
_PDF2_SSN: Final[str] = "http://iso.org/pdf2/ssn"
_PDFD_CONFORMS: Final[str] = "{http://pdfa.org/declarations/}conformsTo"
_RDF_RESOURCE: Final[str] = "{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource"
_WTPDF_ACCESSIBILITY: Final[frozenset[str]] = frozenset({
    "http://pdfa.org/declarations/wtpdf#accessibility1.0",
    "http://pdfa.org/declarations/wtpdf/#accessibility1.0",
})
_WTPDF_REUSE: Final[frozenset[str]] = frozenset({
    "http://pdfa.org/declarations/wtpdf#reuse1.0",
    "http://pdfa.org/declarations/wtpdf/#reuse1.0",
})
_AUDIT_ENCODER: Final = msgspec.msgpack.Encoder(order="deterministic")
_DECIMAL_PRECISION: Final = 8
_PIKEPDF_SETTINGS: Final = Lock()

_UA_CLAUSES: Final[tuple[tuple[UaCheck, Callable[[StructureAudit], bool]], ...]] = (
    (UaCheck.MARKED, lambda a: a.marked),
    (UaCheck.STRUCT_TREE, lambda a: a.has_struct and a.has_tree and a.elements > 0),
    (UaCheck.LANG, lambda a: a.has_lang),
    (UaCheck.TITLE, lambda a: a.title_ok),
    (UaCheck.UA_ID, lambda a: a.ua_id),
    (UaCheck.NOT_SUSPECT, lambda a: a.not_suspect),
    (UaCheck.FIGURE_ALT, lambda a: a.figures == a.figures_with_alt),
    (UaCheck.HEADING_NESTING, lambda a: a.headings_monotone),
    (UaCheck.ROLE_MAP, lambda a: a.roles_unmapped == 0),
    (UaCheck.STRUCTURE_NESTING, lambda a: a.misnested == 0),
    (UaCheck.TABLE_REGULAR, lambda a: a.tables_irregular == 0),
    (UaCheck.LIST_STRUCTURE, lambda a: a.lists_irregular == 0),
    (UaCheck.LINK_CONTENT, lambda a: a.links == a.links_with_content),
    (UaCheck.PAGES_KEYED, lambda a: a.elements == 0 or (a.mcids > 0 and a.pages_keyed == a.pages_marked and a.mcids_keyed == a.mcids)),
    (UaCheck.SYNTAX, lambda a: a.syntax_warnings == 0),
    (UaCheck.TEXT_LAYER, lambda a: a.pages_accessible == a.pages),
    (UaCheck.NO_XFA, lambda a: not a.has_xfa),
    (UaCheck.UA2_VERSION, lambda a: a.ua_part != 2 or a.pdf_version >= (2, 0)),
    (UaCheck.UA2_NAMESPACES, lambda a: a.ua_part != 2 or a.namespaced),
    (
        UaCheck.WTPDF_ACCESSIBILITY,
        lambda a: not a.wtpdf_accessibility or (a.ua_part == 2 and a.oracle_valid and a.oracle_errors == 0 and a.pdf_version >= (2, 0) and a.namespaced),
    ),
    (UaCheck.WTPDF_REUSE, lambda a: not a.wtpdf_reuse or (a.has_struct and a.elements > 0 and a.pdf_version >= (2, 0))),
    (UaCheck.ORACLE, lambda a: a.oracle_valid and a.oracle_errors == 0),
)
_PREFLIGHT_CLAUSES: Final[tuple[tuple[PreflightCheck, Callable[[PreflightAudit], bool]], ...]] = (
    (PreflightCheck.PDFX_VALID, lambda a: a.pdfx_valid and a.pdfx_errors == 0),
    (PreflightCheck.CLAIM_HONEST, lambda a: not a.pdfx_claim or a.pdfx_valid),
    (PreflightCheck.OUTPUT_INTENT, lambda a: a.output_intents > 0),
    (PreflightCheck.PAGE_BOXES, lambda a: a.pages_boxed == a.pages),
)
_ARCHIVE_CLAUSES: Final[tuple[tuple[ArchiveCheck, Callable[[ArchiveAudit], bool]], ...]] = (
    (ArchiveCheck.CONVERTED, lambda a: a.converted and a.converter_errors == 0),
    (ArchiveCheck.ORACLE, lambda a: a.oracle_valid and a.oracle_errors == 0),
)



# --- [TABLES] ---------------------------------------------------------------------------

ACCESS_AUTHOR: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.TAGGED, point="author", arm="boundary", defect="author-fold", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[ArtifactsLeg]]] = rostered(Block.of_seq([ACCESS_AUTHOR]))

_AUTHOR_RAISES: Final[Catch] = (Lapse,)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _failed[C, A](clauses: tuple[tuple[C, Callable[[A], bool]], ...], audit: A, /) -> tuple[C, ...]:
    return tuple(check for check, holds in clauses if not holds(audit))


@dataclass(slots=True)
class _Author:
    role_map: dict[str, str] = dc_field(default_factory=dict)
    slots: dict[int, list["pikepdf.Object"]] = dc_field(default_factory=dict)
    elements: int = 0
    figures: int = 0


def _elem(pdf: "pikepdf.Pdf", node: DocumentNode, parent: "pikepdf.Object", build: _Author, /) -> "pikepdf.Object":
    build.elements += 1
    role = role_of(node)
    if not 0 <= node.meta.page < len(pdf.pages):
        raise ValueError(f"structure element /{role} names page {node.meta.page} outside 0..{len(pdf.pages) - 1}")
    elem = pdf.make_indirect(Dictionary(Type=Name.StructElem, S=Name("/" + role), P=parent, Pg=pdf.pages[node.meta.page].obj))
    if isinstance(node, StructureNode) and isinstance(node.role, ForeignRole):
        build.role_map[role] = standard_for(node.role).value
    if isinstance(node, FigureNode):
        build.figures += 1
    if isinstance(node, FigureNode | FormulaNode) and (alt := alt_of(node)[0]):
        elem.Alt = String(alt)
    if not isinstance(node.meta.actual_text, msgspec.UnsetType) and node.meta.actual_text:
        elem.ActualText = String(node.meta.actual_text)
    if not isinstance(node.meta.lang, msgspec.UnsetType) and node.meta.lang:
        elem.Lang = String(node.meta.lang)
    return elem


def _authored_tree(pdf: "pikepdf.Pdf", source: DocumentNode, struct_root: "pikepdf.Object", build: _Author, /) -> None:
    grown: dict[int, tuple["pikepdf.Object", list["pikepdf.Object"]]] = {}
    frontier: Block[tuple[DocumentNode, "pikepdf.Object"]] = Block.singleton((source, struct_root))
    while not frontier.is_empty():
        (node, parent), frontier = frontier.head(), frontier.tail()
        elem = _elem(pdf, node, parent, build)
        grown.setdefault(id(parent), (parent, []))[1].append(elem)
        branches = children(node)
        if branches:
            frontier = Block.of_seq((kid, elem) for kid in branches).append(frontier)
        elif isinstance(node, RunNode):
            owners = build.slots.setdefault(node.meta.page, [])
            elem.K = len(owners)
            owners.append(elem)
    for holder, kids in grown.values():
        holder.K = Array(kids)


def _stamped(pdf: "pikepdf.Pdf", page: "pikepdf.Page", /) -> None:
    marked: list[object] = []
    ordinal = 0
    for instruction in pikepdf.parse_content_stream(page):
        is_op = isinstance(instruction, pikepdf.ContentStreamInstruction)
        if is_op and str(instruction.operator) == "BT":
            marked.append(pikepdf.ContentStreamInstruction([Name("/P"), Dictionary(MCID=ordinal)], pikepdf.Operator("BDC")))
            ordinal += 1
        marked.append(instruction)
        if is_op and str(instruction.operator) == "ET":
            marked.append(pikepdf.ContentStreamInstruction([], pikepdf.Operator("EMC")))
    page.Contents = pdf.make_stream(pikepdf.unparse_content_stream(marked))


def _numeric(value: object, /) -> int | None:
    text = str(value)
    return int(text) if text.lstrip("-").isdigit() else None


def _page_mcids(page: "pikepdf.Page", /) -> tuple[int, ...]:
    return tuple(
        ordinal
        for instruction in pikepdf.parse_content_stream(page)
        if isinstance(instruction, pikepdf.ContentStreamInstruction)
        for operand in instruction.operands
        if isinstance(operand, pikepdf.Dictionary)
        and (mcid := operand.get(Name("/MCID"))) is not None
        and (ordinal := _numeric(mcid)) is not None
    )


def _bound_mcids(page: "pikepdf.Page", entries: "pikepdf.Object", /) -> tuple[int, ...]:
    return tuple(
        ordinal
        for entry in entries
        if isinstance(entry, pikepdf.Dictionary)
        and entry.get(Name.Type) == Name.StructElem
        and entry.get(Name.Pg) == page.obj
        and (kid := entry.get(Name.K)) is not None
        and (ordinal := _numeric(kid)) is not None
    )


def _tag(access: "Access") -> AccessFact:
    match access.request:
        case AccessRequest(tag="tagged", tagged=(source, lang, title, ua_part)):
            pass
        case _ as unreachable:
            assert_never(unreachable)
    with pikepdf.open(BytesIO(access.pdf)) as pdf:
        mark_info = pdf.Root.get(Name.MarkInfo, Dictionary())
        mark_info.Marked = True
        pdf.Root.MarkInfo = mark_info
        viewer = pdf.Root.get(Name.ViewerPreferences, Dictionary())
        viewer.DisplayDocTitle = True
        pdf.Root.ViewerPreferences = viewer
        meta_lang = None if isinstance(source.meta.lang, msgspec.UnsetType) else source.meta.lang
        if (document_lang := lang or meta_lang) is not None:
            pdf.Root.Lang = String(document_lang)
        with pdf.open_metadata() as xmp:
            xmp["pdfuaid:part"] = str(ua_part)
            if title:
                xmp["dc:title"] = title
        struct_root = pdf.make_indirect(Dictionary(Type=Name.StructTreeRoot, K=Array([]), ParentTreeNextKey=0, RoleMap=Dictionary()))
        pdf.Root.StructTreeRoot = struct_root
        build = _Author()
        _authored_tree(pdf, source, struct_root, build)
        role_map = Dictionary()
        for foreign, standard in build.role_map.items():
            role_map[Name("/" + foreign)] = Name("/" + standard)
        struct_root.RoleMap = role_map
        if ua_part == 2:
            struct_root.Namespaces = Array([pdf.make_indirect(Dictionary(Type=Name.Namespace, NS=String(_PDF2_SSN)))])
        parent_tree = NumberTree.new(
            pdf
        )
        for page_key in sorted(build.slots):
            if not _page_mcids(pdf.pages[page_key]):
                _stamped(pdf, pdf.pages[page_key])
            pdf.pages[page_key].obj.StructParents = page_key
            parent_tree[page_key] = Array(build.slots[page_key])
            if _page_mcids(pdf.pages[page_key]) != tuple(range(len(build.slots[page_key]))):
                raise ValueError(f"page {page_key} MCIDs do not match the structure-tree leaves")
        struct_root.ParentTree = parent_tree.obj
        struct_root.ParentTreeNextKey = max(build.slots) + 1 if build.slots else 0
        sink = BytesIO()
        with _PIKEPDF_SETTINGS:
            precision = pikepdf.settings.get_decimal_precision()
            try:
                pikepdf.settings.set_decimal_precision(_DECIMAL_PRECISION)
                pdf.save(sink, deterministic_id=True)
            finally:
                pikepdf.settings.set_decimal_precision(precision)
        return AccessFact(produced=(sink.getvalue(), len(pdf.pages), build.elements, build.figures))


@dataclass(slots=True)
class _Tally:
    elements: int = 0
    depth: int = 0
    figures: int = 0
    figures_with_alt: int = 0
    headings: int = 0
    tables: int = 0
    tables_irregular: int = 0
    lists: int = 0
    lists_irregular: int = 0
    links: int = 0
    links_with_content: int = 0
    misnested: int = 0
    roles: set[str] = dc_field(default_factory=set)
    levels: list[int] = dc_field(default_factory=list)
    alt_pages: set[tuple[int, int]] = dc_field(default_factory=set)


def _struct_kids(elem: "pikepdf.Object", /) -> tuple["pikepdf.Object", ...]:
    kids = elem.get(Name.K)
    members = kids if isinstance(kids, pikepdf.Array) else (kids,) if isinstance(kids, pikepdf.Dictionary) else ()
    return tuple(kid for kid in members if isinstance(kid, pikepdf.Dictionary) and kid.get(Name.Type) == Name.StructElem)


def _kid_elts(elem: "pikepdf.Object", /) -> tuple[StructEltKind | None, ...]:
    return tuple(_ELT.try_find(str(kid.get(Name.S, ""))).default_value(None) for kid in _struct_kids(elem))


def _table_regular(elem: "pikepdf.Object", /) -> bool:
    kinds = _kid_elts(elem)
    return StructEltKind.TR in kinds or any(
        kind in _ROW_GROUPS and StructEltKind.TR in _kid_elts(kid) for kind, kid in zip(kinds, _struct_kids(elem), strict=True)
    )


def _list_regular(elem: "pikepdf.Object", /) -> bool:
    return StructEltKind.LI in _kid_elts(elem)


def _walk(root: "pikepdf.Object", tally: _Tally, /) -> None:
    stack: Block[tuple["pikepdf.Object", int, StructEltKind | None]] = Block.singleton((root, 1, None))
    while not stack.is_empty():
        (elem, depth, parent), stack = stack.head(), stack.tail()
        tally.elements += 1
        tally.depth = max(tally.depth, depth)
        role = str(elem.get(Name.S, ""))
        tally.roles.add(role)
        elt = _ELT.try_find(role).default_value(None)
        if elt is not None and parent is not None and elt in _NESTING and parent not in _NESTING[elt]:
            tally.misnested += 1
        match elt:
            case StructEltKind.FIGURE | StructEltKind.FORMULA:
                tally.figures += 1
                covered = bool(elem.get(Name.Alt) or elem.get(Name.ActualText))
                tally.figures_with_alt += covered
                if covered and (pg := elem.get(Name.Pg)) is not None:
                    tally.alt_pages.add(pg.objgen)
            case StructEltKind.TABLE:
                tally.tables += 1
                tally.tables_irregular += not _table_regular(elem)
            case StructEltKind.L:
                tally.lists += 1
                tally.lists_irregular += not _list_regular(elem)
            case StructEltKind.LINK:
                tally.links += 1
                tally.links_with_content += bool(elem.get(Name.K))
            case StructEltKind() as heading if _CATEGORY[heading][0] is StructCategory.HEADING:
                tally.headings += 1
                tally.levels.append(_CATEGORY[heading][1])
            case _:
                pass
        branches = Block.of_seq((kid, depth + 1, elt) for kid in _struct_kids(elem))
        stack = branches.append(stack)


def _declared(packet: bytes, /) -> frozenset[str]:
    try:
        root = hardened_parse(packet)
    except etree.XMLSyntaxError:
        return frozenset()
    elements = ((node.text or "") for node in root.iter(_PDFD_CONFORMS))
    references = (node.attrib.get(_RDF_RESOURCE, "") for node in root.iter(_PDFD_CONFORMS))
    attributes = (value for node in root.iter() for name, value in node.attrib.items() if name == _PDFD_CONFORMS)
    return frozenset(uri.strip() for uri in (*elements, *references, *attributes) if uri.strip())


def _audit(access: "Access") -> AccessFact:
    match access.request:
        case AccessRequest(tag="audit", audit=ua_part):
            pass
        case _ as unreachable:
            assert_never(unreachable)
    with pikepdf.open(BytesIO(access.pdf)) as pdf:
        root = pdf.Root
        mark_info = root.get(Name.MarkInfo, Dictionary())
        struct_root = root.get(Name.StructTreeRoot)
        role_map = struct_root.get(Name.RoleMap, Dictionary()) if struct_root is not None else Dictionary()
        tally = _Tally()
        for kid in struct_root.get(Name.K, Array([])) if struct_root is not None else ():
            if isinstance(kid, pikepdf.Dictionary) and kid.get(Name.Type) == Name.StructElem:
                _walk(kid, tally)
        pages = len(pdf.pages)
        parent_tree = NumberTree(struct_root.ParentTree) if struct_root is not None and Name.ParentTree in struct_root else None
        page_bindings = tuple(
            (
                _page_mcids(page),
                (
                    _bound_mcids(page, parent_tree[slot])
                    if parent_tree is not None
                    and (raw := page.obj.get(Name.StructParents)) is not None
                    and (slot := _numeric(raw)) is not None
                    and slot in parent_tree
                    else ()
                ),
            )
            for page in pdf.pages
        )
        pages_marked = sum(bool(found) for found, _expected in page_bindings)
        mcids = sum(len(found) for found, _expected in page_bindings)
        keyed = sum(bool(found) and found == expected for found, expected in page_bindings)
        mcids_keyed = sum(len(frozenset(found) & frozenset(expected)) for found, expected in page_bindings)
        syntax = len(pdf.check_pdf_syntax())
        alt_indices = frozenset(index for index, page in enumerate(pdf.pages) if page.obj.objgen in tally.alt_pages)
        mapped = {str(name) for name in role_map.keys()}
        with pdf.open_metadata(set_pikepdf_as_editor=False, update_docinfo=False) as xmp:
            ua_id, has_title = str(xmp.get("pdfuaid:part", "")) == str(ua_part), bool(xmp.get("dc:title"))
            pdfa_claim, pdfx_claim = xmp.pdfa_status, xmp.pdfx_status
        metadata = root.get(Name.Metadata)
        declared = _declared(bytes(metadata.read_bytes()) if metadata is not None else b"")
        wtpdf_accessibility = not declared.isdisjoint(_WTPDF_ACCESSIBILITY)
        wtpdf_reuse = not declared.isdisjoint(_WTPDF_REUSE)
        marked = bool(mark_info.get(Name.Marked, False))
        has_struct = struct_root is not None
        namespaced = has_struct and Name("/Namespaces") in struct_root
        has_lang = bool(root.get(Name.Lang, ""))
        title_ok = has_title and bool(root.get(Name.ViewerPreferences, Dictionary()).get(Name.DisplayDocTitle, False))
        not_suspect = not bool(mark_info.get(Name.Suspects, False))
        unmapped = sum(1 for role in tally.roles if role not in _ELT and role not in mapped)
        monotone = all(b - a <= 1 for a, b in pairwise(tally.levels) if b > a)
        role_map_n = len(mapped)
    with pdf_oxide.PdfDocument.from_bytes(access.pdf) as oracle:
        verdict = oracle.validate_pdf_ua()
        oracle_valid, oracle_errors, oracle_warnings = bool(verdict["valid"]), len(verdict["errors"]), len(verdict["warnings"])
        has_tree = oracle.has_structure_tree()
        xfa = oracle.has_xfa()
        version = tuple(oracle.version())
        text_flags = tuple(bool(oracle.has_text_layer(page_index)) for page_index in range(int(oracle.page_count)))
        text_pages = sum(text_flags)
        structured = len(
            oracle.structured_warnings()
        )
    accessible = sum(1 for index, has_text in enumerate(text_flags) if has_text or index in alt_indices)
    evidence = StructureAudit(
        ua_part=ua_part,
        elements=tally.elements,
        depth=tally.depth,
        pages=pages,
        pages_keyed=keyed,
        pages_marked=pages_marked,
        mcids=mcids,
        mcids_keyed=mcids_keyed,
        figures=tally.figures,
        figures_with_alt=tally.figures_with_alt,
        headings=tally.headings,
        headings_monotone=monotone,
        tables=tally.tables,
        tables_irregular=tally.tables_irregular,
        lists=tally.lists,
        lists_irregular=tally.lists_irregular,
        links=tally.links,
        links_with_content=tally.links_with_content,
        role_map=role_map_n,
        roles_unmapped=unmapped,
        misnested=tally.misnested,
        marked=marked,
        has_struct=has_struct,
        has_lang=has_lang,
        title_ok=title_ok,
        ua_id=ua_id,
        not_suspect=not_suspect,
        namespaced=namespaced,
        syntax_warnings=syntax,
        oracle_valid=oracle_valid,
        oracle_errors=oracle_errors,
        oracle_warnings=oracle_warnings,
        structured_warnings=structured,
        has_tree=has_tree,
        pages_with_text=text_pages,
        pages_accessible=accessible,
        has_xfa=xfa,
        pdf_version=version,
        pdfa_claim=pdfa_claim,
        pdfx_claim=pdfx_claim,
        wtpdf_accessibility=wtpdf_accessibility,
        wtpdf_reuse=wtpdf_reuse,
        failures=(),
    )
    audit = structs.replace(evidence, failures=_failed(_UA_CLAUSES, evidence))
    return AccessFact(audit=(_AUDIT_ENCODER.encode(audit), pages, audit))


def _archive(access: "Access") -> AccessFact:
    match access.request:
        case AccessRequest(tag="archive", archive=level):
            pass
        case _ as unreachable:
            assert_never(unreachable)
    with pdf_oxide.PdfDocument.from_bytes(access.pdf) as doc:
        outcome = doc.convert_to_pdf_a(level)
        verified = doc.validate_pdf_a(level)
        converted, applied, converter_errors = bool(outcome["success"]), len(outcome["actions"]), len(outcome["errors"])
        oracle_valid, oracle_errors, oracle_warnings = bool(verified["valid"]), len(verified["errors"]), len(verified["warnings"])
        data, pages = doc.to_bytes(), int(doc.page_count)
    evidence = ArchiveAudit(
        level=level,
        converted=converted,
        applied=applied,
        converter_errors=converter_errors,
        oracle_valid=oracle_valid,
        oracle_errors=oracle_errors,
        oracle_warnings=oracle_warnings,
        failures=(),
    )
    audit = structs.replace(evidence, failures=_failed(_ARCHIVE_CLAUSES, evidence))
    return AccessFact(archive=(data, pages, applied, converter_errors + oracle_errors + len(audit.failures), audit))


def _preflight(access: "Access") -> AccessFact:
    match access.request:
        case AccessRequest(tag="preflight", preflight=level):
            pass
        case _ as unreachable:
            assert_never(unreachable)
    with pikepdf.open(BytesIO(access.pdf)) as pdf:
        with pdf.open_metadata(set_pikepdf_as_editor=False, update_docinfo=False) as xmp:
            pdfx_claim = str(xmp.pdfx_status)
        intents = len(pdf.Root.get(Name.OutputIntents, Array()))
        boxed = sum(Name.TrimBox in page.obj or Name.ArtBox in page.obj for page in pdf.pages)
        page_count = len(pdf.pages)
    with pdf_oxide.PdfDocument.from_bytes(access.pdf) as oracle:
        verdict = oracle.validate_pdf_x(
            level
        )
        valid, errors, warnings = bool(verdict["valid"]), len(verdict["errors"]), len(verdict["warnings"])
        structured, oracle_pages = len(oracle.structured_warnings()), int(oracle.page_count)
    if page_count != oracle_pages:
        raise ValueError(f"preflight page-count split: pikepdf={page_count} pdf-oxide={oracle_pages}")
    evidence = PreflightAudit(
        level=level,
        pdfx_valid=valid,
        pdfx_errors=errors,
        pdfx_warnings=warnings,
        pdfx_claim=pdfx_claim,
        output_intents=intents,
        pages=page_count,
        pages_boxed=boxed,
        structured_warnings=structured,
        failures=(),
    )
    audit = structs.replace(evidence, failures=_failed(_PREFLIGHT_CLAUSES, evidence))
    return AccessFact(preflight=(_AUDIT_ENCODER.encode(audit), page_count, audit))


# --- [COMPOSITION] ----------------------------------------------------------------------
_ARM: Final[Map[AccessOp, Arm]] = Map.of_seq([
    (AccessOp.TAG, _tag),
    (AccessOp.AUDIT, _audit),
    (AccessOp.ARCHIVE, _archive),
    (AccessOp.PREFLIGHT, _preflight),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
