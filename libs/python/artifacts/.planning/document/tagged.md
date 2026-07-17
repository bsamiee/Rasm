# [PY_ARTIFACTS_TAGGED]

PDF/UA (ISO 14289) structure, ISO 15930 PDF/X print production, and ISO 19005 archival conversion close over one document rail: `Access` authors the marked-content structure tree into an emitted PDF, audits its conformance, upgrades to the archival profile, and preflights the PDF/X claim through one closed `AccessOp` over `_ARM`. Validation combines explainable owner-local clauses with the independent MIT/Apache `pdf_oxide` oracle; archival conversion keeps the converter `success` verdict and the post-convert validation verdict as distinct `ArchiveCheck` clauses, so an empty error list never substitutes for either boolean.

Structure vocabulary is consumed from `document/model#NODE`: `DocumentNode`, the role family, and the `role_of`/`role_category`/`alt_of`/`children`/`standard_for` projections derive the `/S` algebra once from `StructEltKind`. A born-tagged emitter (`document/emit#DOCUMENT` UA arms) arrives with page-local MCIDs already marked in document order; an unmarked source gains them through `_stamped`'s explicit-proplist `BDC`/`EMC` pass per text block, and `_tag` then rejects any page whose marked MCID set differs from the structure leaves before binding `/StructParents` into `/ParentTree`. `StructureAudit.conformant` threads into the `exchange/conformance#CONFORMANCE` `SourceConformance.structural` half of `AuditSpec.source`; the receipt cases reuse `core/receipt#RECEIPT` `Egress`/`Pdf` under `@receipted(OPEN)` and the owner's `lane: LanePolicy`.

## [01]-[INDEX]

- [01]-[ACCESS]: the tag/audit/archive/preflight conformance close over `_ARM`, every verdict combining local clauses with an independent validation observation.

## [02]-[ACCESS]

- Owner: `Access` — `_ARM` maps each op to its single `AccessFact`-returning arm with zero `match` sprawl, the closed `StrEnum` membership total over the table by construction; each close's audit value carries every clause input and its `failures` derive through one clause table under the shared `_failed` fold, so a decoded content-addressed audit re-derives its own verdict; `pikepdf` owns the qpdf object model and the XMP context, `pdf_oxide` the independent oracle under its deterministic-close capsule, and the model owns the tree algebra this page only reads.
- Cases: TAG writes the catalog requirements ISO 14289 mandates beyond the tree (`/Lang`, `/DisplayDocTitle`, XMP `pdfuaid:part` + `dc:title`) and re-emits under `Pdf.save(deterministic_id=True)` while a scoped lock pins and restores `settings.set_decimal_precision`; AUDIT includes per-page `has_text_layer`, `has_xfa`, exact MCID-to-`/ParentTree` binding, and per-owner table/list regularity, so one valid structure never masks a malformed sibling; `ua_part=2` adds `UA2_VERSION` and `UA2_NAMESPACES`; the WTPDF declaration pair closes the well-tagged interchange claim — a declared accessibility conformance holds only under a part-2 audit whose UA oracle, PDF 2.0 version, and structure namespaces all pass (`validate_pdf_ua` carries no part argument, so part-2 specificity stays local evidence), a declared reuse conformance under local structure plus PDF 2.0 evidence because no reuse oracle exists, and each `pdfd:conformsTo` level admits its erratum-canonical and as-published URI spellings; ARCHIVE folds converter self-report and `validate_pdf_a` into `ArchiveAudit`; PREFLIGHT turns the declared claim, `/OutputIntents`, and per-page `TrimBox`/`ArtBox` geometry into a clause verdict.
- Auto: `_audit` walks the UNTRUSTED `/StructTreeRoot` `/K` spine through the depth-safe `Block` frontier — never native recursion an adversarial nesting depth overflows — and TAG authors through the same discipline, a pre-order frontier whose per-parent child lists assemble the `/K` arrays after the sweep, because a lens-recovered source tree carries the same adversarial depth; every `pikepdf`-touching clause predicate resolves to a plain value BEFORE the handle frees, and metadata reads through the read-only `open_metadata(set_pikepdf_as_editor=False, update_docinfo=False)` form that never mutates the bytes it audits; the `pdfd:declarations` bag defeats the pikepdf mapping view, so the WTPDF `conformsTo` URIs read off the raw `/Metadata` stream's element tree in all three RDF spellings — element text, shorthand attribute, `rdf:resource` reference; the `/ParentTree` IS a PDF number-tree, owned by the modeled `pikepdf.NumberTree.new(pdf)` mapping-view, never a hand-assembled flat `Nums` array; `pikepdf` exposes no high-level `StructTreeRoot` helper, so the raw `Object`-model spike is the real surface and a phantom `pdf.add_structure_tree()` convenience is the rejected form.
- Receipt: the two producing ops share `ArtifactReceipt.Egress` (structure-element count riding `outline_depth`, figure count riding `overlays` — the finishing-facts convention `document/egress#FINISH` fixes) and the two validating ops share `ArtifactReceipt.Pdf`, never a new receipt case; the `StructureAudit`/`PreflightAudit`/`ArchiveAudit` values are content-addressed by their op keys, and the composition root decodes them to thread each `conformant` verdict onward.
- Growth: a new access op is one `AccessRequest` case, one `AccessOp` row, and one `_ARM` entry; a new conformance clause is one `UaCheck`/`PreflightCheck`/`ArchiveCheck` member plus one predicate row in its `_UA_CLAUSES`/`_PREFLIGHT_CLAUSES`/`_ARCHIVE_CLAUSES` table (a clause needing fresh evidence also lands its audit field); a new standard PDF/UA role is one model `StructEltKind` member; a new nesting rule is one `_NESTING` row; a new archival or print level is one `Literal` member.
- Boundary: born-PDF/A authoring stays at `document/emit#DOCUMENT` — ARCHIVE upgrades an ALREADY-emitted PDF in place; `pdf_oxide.DocumentBuilder.tagged_pdf_ua1()` is the from-scratch born-tagged author reserved for emit, never a second structure author over an existing PDF here; signing stays at `exchange/conformance#CONFORMANCE`, security finishing at `document/egress#FINISH`, OCG authoring at `export/layered#LAYERED`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from dataclasses import dataclass, field as dc_field
from enum import StrEnum
from io import BytesIO
from itertools import pairwise
from threading import Lock
from typing import TYPE_CHECKING, Final, Literal, Self, assert_never

import msgspec
from defusedxml import DefusedXmlException
from defusedxml.ElementTree import ParseError, fromstring
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, structs

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.document.model import (
    DocumentNode,
    FigureNode,
    ForeignRole,
    FormulaNode,
    LangTag,
    RunNode,
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
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait
from rasm.runtime.receipts import OPEN, Receipt, receipted

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


class UaCheck(StrEnum):  # the ISO 14289 structural-conformance clauses the AUDIT closes over
    MARKED = "marked"  # /MarkInfo /Marked true
    STRUCT_TREE = "struct-tree"  # /StructTreeRoot present
    LANG = "lang"  # catalog /Lang present
    TITLE = "title"  # XMP dc:title + /ViewerPreferences /DisplayDocTitle true
    UA_ID = "ua-id"  # XMP pdfuaid:part identifier
    NOT_SUSPECT = "not-suspect"  # /MarkInfo /Suspects absent or false
    FIGURE_ALT = "figure-alt"  # every /Figure AND /Formula carries /Alt or /ActualText — ISO 14289 §7.9 holds formulas to the figure bar
    HEADING_NESTING = "heading-nesting"  # no skipped heading level in document order
    ROLE_MAP = "role-map"  # every non-standard /S mapped in /RoleMap
    STRUCTURE_NESTING = "structure-nesting"  # every constrained /S nests under a standard-legal parent role
    TABLE_REGULAR = "table-regular"  # EVERY /Table carries its own /TR rows (direct or under THead/TBody/TFoot) — per-owner, never a global tally
    LIST_STRUCTURE = "list-structure"  # EVERY /L carries its own /LI items — per-owner, never a global tally
    LINK_CONTENT = "link-content"  # every /Link carries content (kids)
    PAGES_KEYED = "pages-keyed"  # every page carrying MCIDs resolves the full MCID set through /StructParents into /ParentTree
    SYNTAX = "syntax"  # pikepdf `check_pdf_syntax` (qpdf --check) reports no structural warning
    TEXT_LAYER = "text-layer"  # every page carries extractable text or alt-covered tagged imagery — a genuine /Figure page with /Alt conforms
    NO_XFA = "no-xfa"  # no dynamic XFA form (`pdf_oxide.has_xfa` false) — ISO 14289-1 §7.18.1 prohibits it
    UA2_VERSION = "ua2-version"  # PDF/UA-2 (ISO 14289-2) targets PDF 2.0 — the oracle's `(major, minor)` version tuple reads >= (2, 0)
    UA2_NAMESPACES = "ua2-namespaces"  # PDF/UA-2 structure namespaces — /StructTreeRoot carries /Namespaces
    WTPDF_ACCESSIBILITY = "wtpdf-accessibility"  # a declared WTPDF accessibility conformance holds only under a passing part-2 audit
    WTPDF_REUSE = "wtpdf-reuse"  # a declared WTPDF reuse conformance holds under local structure + PDF 2.0 evidence — no reuse oracle exists
    ORACLE = "oracle"  # the independent `pdf_oxide.validate_pdf_ua` in-process oracle agrees (valid, zero errors)


class PreflightCheck(StrEnum):  # the ISO 15930 PDF/X print-production clauses the PREFLIGHT closes over
    PDFX_VALID = "pdfx-valid"  # the independent `pdf_oxide.validate_pdf_x(level)` oracle agrees (valid, zero errors)
    CLAIM_HONEST = "claim-honest"  # a document that DECLARES a /pdfxid (`pdfx_claim`) actually validates — the decorative-claim close
    OUTPUT_INTENT = "output-intent"  # every PDF/X level requires a catalog /OutputIntents entry naming the target print condition
    PAGE_BOXES = "page-boxes"  # every page declares its print geometry — a TrimBox or ArtBox beside the MediaBox


class ArchiveCheck(StrEnum):  # the ISO 19005 archival clauses the ARCHIVE close reads over its two sources
    CONVERTED = "converted"  # the converter's OWN `success` verdict with zero converter errors — an empty action list never stands in for it
    ORACLE = "oracle"  # the post-convert `validate_pdf_a` verdict on the SAME upgraded handle (valid AND zero errors)


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class AccessFault:
    # closed ADMISSION vocabulary `of` produces; the arm-level `pikepdf.PdfError`/`pdf_oxide` raise converts
    # to the runtime `BoundaryFault` at the `async_boundary` capsule, never this interior vocabulary.
    tag: Literal["empty"] = tag()
    empty: None = case()


# --- [MODELS] ---------------------------------------------------------------------------



class StructureAudit(Struct, frozen=True, gc=False):
    # one TOTAL clause-evidence record: every `_UA_CLAUSES` predicate reads this value alone, so a decoded
    # content-addressed audit re-derives its own verdict and no clause input dies at the borrow window.
    ua_part: int  # the audited conformance part (1 or 2); the UA2 and WTPDF clauses condition on it
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
    headings_monotone: bool  # no skipped heading level in document order; the HEADING_NESTING clause reads it
    tables: int
    tables_irregular: int  # /Table owners whose OWN kid scan found no /TR (direct or under a row group) — the per-owner evidence TABLE_REGULAR reads
    lists: int
    lists_irregular: int  # /L owners whose OWN kid scan found no /LI — the per-owner evidence LIST_STRUCTURE reads
    links: int
    links_with_content: int  # /Link owners carrying kids; the LINK_CONTENT clause reads full coverage
    role_map: int
    roles_unmapped: int  # read /S roles neither standard nor /RoleMap-registered; the ROLE_MAP clause reads zero
    misnested: int
    marked: bool  # /MarkInfo /Marked
    has_struct: bool  # pikepdf /StructTreeRoot present; STRUCT_TREE reconciles it against the oracle's independent has_tree
    has_lang: bool  # catalog /Lang present
    title_ok: bool  # XMP dc:title beside /ViewerPreferences /DisplayDocTitle
    ua_id: bool  # XMP pdfuaid:part matches the audited part
    not_suspect: bool  # /MarkInfo /Suspects absent or false
    namespaced: bool  # /StructTreeRoot /Namespaces present — the PDF 2.0 structure-namespace entry UA2_NAMESPACES reads
    syntax_warnings: int  # pikepdf `check_pdf_syntax` structural-warning count; the SYNTAX clause reads its emptiness
    oracle_valid: bool  # the `pdf_oxide.validate_pdf_ua` external verdict `valid` boolean
    oracle_errors: int  # the external oracle's reported error count; the ORACLE clause reads valid AND zero errors
    oracle_warnings: int  # the external oracle's non-fatal warning count; captured whole (never a 2-of-3 slice), evidence not a clause since a warning is not a conformance failure
    structured_warnings: int  # the `pdf_oxide.structured_warnings()` structure-diagnostic count (`{category, page, message, spec_section}` rows), additional two-source structural evidence beside the clause set
    has_tree: bool  # the oracle's independent `has_structure_tree` confirmation the STRUCT_TREE clause reconciles against the pikepdf `/StructTreeRoot` read
    pages_with_text: int  # count of pages carrying an extractable text layer (`pdf_oxide.has_text_layer`); evidence the accessible count folds
    pages_accessible: int  # pages carrying text OR alt-covered tagged imagery; the TEXT_LAYER clause reads full coverage
    has_xfa: bool  # dynamic XFA presence (`pdf_oxide.has_xfa`); the NO_XFA clause reads its absence
    pdf_version: tuple[int, int]  # the oracle's `(major, minor)` version tuple; the UA2_VERSION clause reads >= (2, 0) under part 2
    pdfa_claim: str  # the document's OWN declared PDF/A conformance (pikepdf XMP `pdfa_status`), evidence not a clause
    pdfx_claim: str  # the document's OWN declared PDF/X conformance (pikepdf XMP `pdfx_status`)
    wtpdf_accessibility: bool  # a `pdfd:conformsTo` declaration names the WTPDF accessibility level; the WTPDF_ACCESSIBILITY clause conditions on it
    wtpdf_reuse: bool  # a `pdfd:conformsTo` declaration names the WTPDF reuse level; the WTPDF_REUSE clause conditions on it
    failures: tuple[UaCheck, ...]

    @property
    def coverage(self) -> float:
        return 1.0 if self.figures == 0 else self.figures_with_alt / self.figures

    @property
    def conformant(self) -> bool:
        return not self.failures

    def facts(self) -> dict[str, str]:  # the scalar projection a span/log consumer reads off the decoded content-addressed audit
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
    # `conformant` gates the PAdES/print-issue close.
    level: PdfxLevel
    pdfx_valid: bool  # the `pdf_oxide.validate_pdf_x(level)` external verdict `valid` boolean
    pdfx_errors: int  # the oracle's reported error count; the PDFX_VALID clause reads valid AND zero errors
    pdfx_warnings: int  # the oracle's non-fatal warning count; evidence, not a clause
    pdfx_claim: str  # the document's OWN declared PDF/X conformance (pikepdf XMP `pdfx_status`)
    output_intents: int  # catalog /OutputIntents entry count; the OUTPUT_INTENT clause reads non-emptiness
    pages: int
    pages_boxed: int  # pages declaring a TrimBox or ArtBox; the PAGE_BOXES clause reads full coverage
    structured_warnings: int  # the `pdf_oxide.structured_warnings()` diagnostic count folded as print-side evidence
    failures: tuple[PreflightCheck, ...]

    @property
    def conformant(self) -> bool:  # empty only when the oracle passes AND a declared claim is honest AND the print geometry holds
        return not self.failures

    def facts(self) -> dict[str, str]:  # the scalar projection a span/log consumer reads off the decoded content-addressed verdict
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
    # two-observation archival verdict: the converter self-report AND the post-upgrade oracle, each a clause — a false
    # provider boolean fails its clause even when the error lists arrive empty, so success-equivalent evidence is unforgeable.
    level: PdfaLevel
    converted: bool  # `convert_to_pdf_a` `success` — the self-report verdict, admitted whole
    applied: int  # converter action count
    converter_errors: int
    oracle_valid: bool  # `validate_pdf_a` `valid` on the SAME upgraded handle
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
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    request: AccessRequest
    pdf: bytes
    lane: LanePolicy
    fact: AccessFact | None = None

    @property
    def op(self) -> AccessOp:
        return AccessOp.TAG if self.request.tag == "tagged" else AccessOp(self.request.tag)

    @classmethod
    def of(cls, request: AccessRequest, pdf: bytes, /, *, lane: LanePolicy) -> Result[Self, AccessFault]:
        return Ok(cls(request=request, pdf=pdf, lane=lane)) if pdf else Error(AccessFault(empty=None))

    def _stepped(self) -> Self:
        return structs.replace(self, fact=_ARM[self.op](self))

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=float(len(self.pdf)))

    @property
    def _key(self) -> ContentKey:
        # `ContentIdentity.key` mints the bare `ContentKey`; `.of` is the railed form and never keys a plan.
        return ContentIdentity.key(f"access-{self.op}", _AUDIT_ENCODER.encode((self.request, self.pdf)))

    @receipted(
        OPEN
    )  # the keep-all redaction policy the runtime receipts owner exports (never a re-minted per-file `Redaction`); drains `contribute` off the stepped owner
    async def _authored(self) -> Self:
        # GIL-releasing native folds cross the runtime thread lane through the owner's bound `lane`, never a folder-minted limiter.
        crossed = await self.lane.offload(Kernel.of(self._stepped, KernelTrait.RELEASING))
        return crossed.default_with(lambda fault: _access_raise(fault))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # terminal receipt threads the PRE-RUN input key so receipt.slot == node.key.
        return (await async_boundary(f"access.{self.op}", self._authored)).map(lambda done: done._receipt(self._key))

    def _receipt(self, key: ContentKey, /) -> ArtifactReceipt:
        assert self.fact is not None
        match self.fact:
            case AccessFact(tag="produced", produced=(data, pages, elements, figures)):
                emitted = ArtifactReceipt.Egress(key, len(data), pages, 0, elements, figures)
            case AccessFact(tag="archive", archive=(data, pages, applied, residual, _audit)):
                emitted = ArtifactReceipt.Egress(key, len(data), pages, 0, applied, residual)
            case AccessFact(tag="audit", audit=(data, pages, _audit)) | AccessFact(
                tag="preflight", preflight=(data, pages, _audit)
            ):
                emitted = ArtifactReceipt.Pdf(key, len(data), pages)
            case _ as unreachable:
                assert_never(unreachable)
        return emitted

    def contribute(self) -> Iterable[Receipt]:
        if self.fact is None:  # contribute rides the stepped owner the weave returned, never the seed
            return
        yield from self._receipt(self._key).contribute()


def _access_raise(fault: object) -> "Access":
    # terminal collapse at the authoring boundary: an offload fault reconstructs the raise the node's rail folds.
    raise ValueError(str(fault))


# --- [CONSTANTS] ------------------------------------------------------------------------

# `_ELT` decodes a read `/S` Name to its model member AND its key set IS the standard-structure membership the /RoleMap
# completeness check reads; `_CATEGORY` projects each member through `role_category`, so both track the model enum.
_ELT: Final[Map[str, StructEltKind]] = Map.of_seq((f"/{elt.value}", elt) for elt in StructEltKind)
_CATEGORY: Final[Map[StructEltKind, tuple[StructCategory, int]]] = Map.of_seq((
    (elt, role_category(StandardRole(elt=elt))) for elt in StructEltKind
))
# standard structure-nesting policy this AUDIT owns: each constrained role's legal parent set per ISO 14289 (list/table
# grouping + the East-Asian ruby/warichu assemblies the model vocabulary carries); a role absent from the table nests
# anywhere, a foreign role is unconstrained.
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
_ROW_GROUPS: Final[frozenset[StructEltKind]] = frozenset({StructEltKind.THEAD, StructEltKind.TBODY, StructEltKind.TFOOT})
# WTPDF (PDF Association well-tagged PDF 1.0) PDF Declarations — the pdfd schema (namespace `http://pdfa.org/declarations/`, prefix `pdfd`)
# carries `pdfd:declarations`, a bag of declaration structs whose `pdfd:conformsTo` URI names the conformance level. Each level admits
# two live spellings: the erratum-canonical form where `#` is the only path-fragment separator, and the as-published WTPDF 1.0
# §6.1.2/§6.1.3 form producers wrote with a `/` before the fragment.
_PDF2_SSN: Final[str] = "http://iso.org/pdf2/ssn"  # the PDF 2.0 standard structure namespace part-2 authoring stamps on /Namespaces
_PDFD_CONFORMS: Final[str] = "{http://pdfa.org/declarations/}conformsTo"
_RDF_RESOURCE: Final[str] = "{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource"  # the URI-reference spelling of a conformsTo value
_WTPDF_ACCESSIBILITY: Final[frozenset[str]] = frozenset({
    "http://pdfa.org/declarations/wtpdf#accessibility1.0",
    "http://pdfa.org/declarations/wtpdf/#accessibility1.0",
})
_WTPDF_REUSE: Final[frozenset[str]] = frozenset({
    "http://pdfa.org/declarations/wtpdf#reuse1.0",
    "http://pdfa.org/declarations/wtpdf/#reuse1.0",
})
_AUDIT_ENCODER: Final = msgspec.msgpack.Encoder(order="deterministic")  # the content key addresses this stable encoding
_DECIMAL_PRECISION: Final = 8  # pinned qpdf real-number precision so a re-emit serializes coordinates identically — the content-addressing precondition beside `deterministic_id`
_PIKEPDF_SETTINGS: Final = Lock()

# each close's clause set is one predicate table over its OWN audit value — the audit carries every clause input, so a
# verdict re-derives from the decoded record and a new clause is one row here plus its enum member, never a fold edit.
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
    (UaCheck.TABLE_REGULAR, lambda a: a.tables_irregular == 0),  # per-owner: every table proved its OWN rows
    (UaCheck.LIST_STRUCTURE, lambda a: a.lists_irregular == 0),  # per-owner: every list proved its OWN items
    (UaCheck.LINK_CONTENT, lambda a: a.links == a.links_with_content),
    (UaCheck.PAGES_KEYED, lambda a: a.elements == 0 or (a.mcids > 0 and a.pages_keyed == a.pages_marked and a.mcids_keyed == a.mcids)),
    (UaCheck.SYNTAX, lambda a: a.syntax_warnings == 0),
    (UaCheck.TEXT_LAYER, lambda a: a.pages_accessible == a.pages),  # every page carries text or alt-covered tagged imagery — an uncovered image page fails
    (UaCheck.NO_XFA, lambda a: not a.has_xfa),
    (UaCheck.UA2_VERSION, lambda a: a.ua_part != 2 or a.pdf_version >= (2, 0)),  # part-2 clause; satisfied by construction under part 1
    (UaCheck.UA2_NAMESPACES, lambda a: a.ua_part != 2 or a.namespaced),
    # a declared WTPDF accessibility conformance is honest only under a part-2 audit whose generic UA oracle passes beside the local PDF 2.0
    # version and namespace evidence — `validate_pdf_ua` carries no part argument, so part-2 specificity is local, never oracle-attributed;
    # a part-1 audit cannot verify the claim, so it fails there rather than passing unexamined
    (
        UaCheck.WTPDF_ACCESSIBILITY,
        lambda a: not a.wtpdf_accessibility or (a.ua_part == 2 and a.oracle_valid and a.oracle_errors == 0 and a.pdf_version >= (2, 0) and a.namespaced),
    ),
    # a declared WTPDF reuse conformance refutes on local evidence alone — WTPDF is a PDF 2.0 technology, so an untagged or pre-2.0
    # file declaring it is a false claim; no reuse oracle participates
    (UaCheck.WTPDF_REUSE, lambda a: not a.wtpdf_reuse or (a.has_struct and a.elements > 0 and a.pdf_version >= (2, 0))),
    (UaCheck.ORACLE, lambda a: a.oracle_valid and a.oracle_errors == 0),  # the independent oracle catches what the clause set cannot enumerate
)
_PREFLIGHT_CLAUSES: Final[tuple[tuple[PreflightCheck, Callable[[PreflightAudit], bool]], ...]] = (
    (PreflightCheck.PDFX_VALID, lambda a: a.pdfx_valid and a.pdfx_errors == 0),
    (PreflightCheck.CLAIM_HONEST, lambda a: not a.pdfx_claim or a.pdfx_valid),  # a declared /pdfxid the oracle refutes is a false claim
    (PreflightCheck.OUTPUT_INTENT, lambda a: a.output_intents > 0),
    (PreflightCheck.PAGE_BOXES, lambda a: a.pages_boxed == a.pages),
)
_ARCHIVE_CLAUSES: Final[tuple[tuple[ArchiveCheck, Callable[[ArchiveAudit], bool]], ...]] = (
    (ArchiveCheck.CONVERTED, lambda a: a.converted and a.converter_errors == 0),  # the self-report boolean participates — never inferred off list lengths
    (ArchiveCheck.ORACLE, lambda a: a.oracle_valid and a.oracle_errors == 0),
)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _failed[C, A](clauses: tuple[tuple[C, Callable[[A], bool]], ...], audit: A, /) -> tuple[C, ...]:
    # one clause fold serves the UA, print, and archival closes; a verdict is the audit value's own derivation.
    return tuple(check for check, holds in clauses if not holds(audit))


@dataclass(slots=True)
class _Author:  # the TAG boundary accumulator: foreign role map, per-page MCID slots, running counts
    role_map: dict[str, str] = dc_field(default_factory=dict)
    slots: dict[int, list["pikepdf.Object"]] = dc_field(default_factory=dict)
    elements: int = 0
    figures: int = 0


def _elem(pdf: "pikepdf.Pdf", node: DocumentNode, parent: "pikepdf.Object", build: _Author, /) -> "pikepdf.Object":
    # one element shell: role, parent link, /Alt off the model `alt_of` projection, /ActualText, /Lang — no recursion,
    # no /K assignment; the frontier owns descent and the K arrays.
    build.elements += 1
    role = role_of(node)
    if not 0 <= node.meta.page < len(pdf.pages):  # a lens-recovered page ordinal is untrusted: a negative index wraps silently, an overrun raises raw
        raise ValueError(f"structure element /{role} names page {node.meta.page} outside 0..{len(pdf.pages) - 1}")
    elem = pdf.make_indirect(Dictionary(Type=Name.StructElem, S=Name("/" + role), P=parent, Pg=pdf.pages[node.meta.page].obj))
    if isinstance(node, StructureNode) and isinstance(node.role, ForeignRole):
        build.role_map[role] = standard_for(node.role).value  # /RoleMap maps the foreign role to its standard type
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
    # pre-order frontier: parents mint before children so `/P` links resolve, leaves bind MCIDs in document order
    # exactly as the recursive form did, and the per-parent child lists assemble the `/K` arrays after the sweep —
    # a lens-recovered source tree carries adversarial depth, so native recursion is forfeit here as in `_walk`.
    grown: dict[int, tuple["pikepdf.Object", list["pikepdf.Object"]]] = {}
    frontier: Block[tuple[DocumentNode, "pikepdf.Object"]] = Block.singleton((source, struct_root))
    while not frontier.is_empty():  # Exemption: iterative frontier — the untrusted tree depth forfeits the recursive form
        (node, parent), frontier = frontier.head(), frontier.tail()
        elem = _elem(pdf, node, parent, build)
        grown.setdefault(id(parent), (parent, []))[1].append(elem)
        branches = children(node)
        if branches:
            frontier = Block.of_seq((kid, elem) for kid in branches).append(frontier)
        elif isinstance(node, RunNode):
            # only a TEXT leaf binds an MCID: `_stamped` marks BT..ET text spans alone, so the slot arrays and the
            # in-stream marks stay congruent under the per-page equality gate; a figure, formula, field, or annotation
            # leaf carries its /Alt and /ActualText evidence without claiming a marked span no emitter writes for it.
            owners = build.slots.setdefault(node.meta.page, [])
            elem.K = len(owners)
            owners.append(elem)
    for holder, kids in grown.values():
        holder.K = Array(kids)


def _stamped(pdf: "pikepdf.Pdf", page: "pikepdf.Page", /) -> None:
    # post-hoc MCID authoring for an unmarked emitter: every BT..ET text block wraps in `/P BDC <</MCID n>> … EMC`
    # in document order — the explicit-proplist operator pair the pikepdf catalog verifies — so structure leaves
    # bind REAL marked spans and the equality gate below verifies congruence instead of trusting convention.
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
    # total integer read over a hostile PDF object — the one guard every MCID, /K, and /StructParents ordinal
    # resolves through: only a textual-integer object admits, so a name, string, or real planted in a malformed
    # proplist or tree slot drops as absent evidence instead of raising mid-tag or mid-audit.
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
    # each /ParentTree slot must resolve to a StructElem leaf of THIS page whose integer /K is the MCID it claims;
    # a non-StructElem, foreign-page, or non-integer-/K entry drops out, so a reordered or malformed tree never keys.
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
    with pikepdf.open(BytesIO(access.pdf)) as pdf:  # deterministic close, never GC-reaped
        mark_info = pdf.Root.get(Name.MarkInfo, Dictionary())
        mark_info.Marked = True
        pdf.Root.MarkInfo = mark_info
        viewer = pdf.Root.get(Name.ViewerPreferences, Dictionary())
        viewer.DisplayDocTitle = True
        pdf.Root.ViewerPreferences = viewer
        meta_lang = None if isinstance(source.meta.lang, msgspec.UnsetType) else source.meta.lang
        if (document_lang := lang or meta_lang) is not None:
            pdf.Root.Lang = String(document_lang)
        with pdf.open_metadata() as xmp:  # XMP PDF/UA identifier + document title
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
            # part-2 output carries the PDF 2.0 standard structure namespace on /StructTreeRoot /Namespaces — the
            # entry the UA2_NAMESPACES audit clause reads — so an authored part-2 document passes its own audit.
            struct_root.Namespaces = Array([pdf.make_indirect(Dictionary(Type=Name.Namespace, NS=String(_PDF2_SSN)))])
        parent_tree = NumberTree.new(
            pdf
        )  # the /ParentTree IS a PDF number-tree; the modeled `NumberTree` mapping-view owner replaces the hand-assembled flat `Nums` Array
        for page_key in sorted(build.slots):
            if not _page_mcids(pdf.pages[page_key]):  # an unmarked emitter's page gains its marked spans here
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
class _Tally:  # the AUDIT boundary accumulator over the authored /StructTreeRoot spine
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
    alt_pages: set[tuple[int, int]] = dc_field(default_factory=set)  # /Pg objgen of pages whose tagged Figure/Formula carries an alternate representation


def _struct_kids(elem: "pikepdf.Object", /) -> tuple["pikepdf.Object", ...]:
    kids = elem.get(Name.K)
    members = kids if isinstance(kids, pikepdf.Array) else (kids,) if isinstance(kids, pikepdf.Dictionary) else ()
    return tuple(kid for kid in members if isinstance(kid, pikepdf.Dictionary) and kid.get(Name.Type) == Name.StructElem)


def _kid_elts(elem: "pikepdf.Object", /) -> tuple[StructEltKind | None, ...]:
    return tuple(_ELT.try_find(str(kid.get(Name.S, ""))).default_value(None) for kid in _struct_kids(elem))


def _table_regular(elem: "pikepdf.Object", /) -> bool:
    # THIS table's own row structure: a direct /TR kid, or a /TR under one of ITS /THead//TBody//TFoot groups —
    # never a document-wide tally one well-formed sibling could satisfy.
    kinds = _kid_elts(elem)
    return StructEltKind.TR in kinds or any(
        kind in _ROW_GROUPS and StructEltKind.TR in _kid_elts(kid) for kind, kid in zip(kinds, _struct_kids(elem), strict=True)
    )


def _list_regular(elem: "pikepdf.Object", /) -> bool:
    return StructEltKind.LI in _kid_elts(elem)


def _walk(root: "pikepdf.Object", tally: _Tally, /) -> None:
    # frontier pushes children before siblings, keeping the document order the `tally.levels` monotonicity read depends on.
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
            case StructEltKind.FIGURE | StructEltKind.FORMULA:  # both roles owe an alternate representation; `_elem` authors Alt for both
                tally.figures += 1
                covered = bool(elem.get(Name.Alt) or elem.get(Name.ActualText))
                tally.figures_with_alt += covered
                if covered and (pg := elem.get(Name.Pg)) is not None:
                    tally.alt_pages.add(pg.objgen)  # the page this covered figure marks accessible for the TEXT_LAYER clause
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
    # `pdfd:declarations` is an rdf:Bag of parseType=Resource structs the pikepdf mapping view cannot decode (the dict read yields
    # whitespace), so the conformsTo URIs read off the raw `/Metadata` packet's element tree — all three RDF spellings: child element
    # text, shorthand attribute, and the `rdf:resource` URI reference — and a malformed packet declares nothing rather than failing the audit.
    try:
        root = fromstring(packet)  # defused parse: the /Metadata packet is document-controlled bytes, so entity and DTD tricks refuse
    except (ParseError, DefusedXmlException):
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
        syntax = len(pdf.check_pdf_syntax())  # qpdf --check structural-syntax warnings; a well-formed PDF is the ISO 14289 precondition
        alt_indices = frozenset(index for index, page in enumerate(pdf.pages) if page.obj.objgen in tally.alt_pages)  # alt-covered page ordinals
        mapped = {str(name) for name in role_map.keys()}  # the foreign /S Names the /RoleMap registers
        with pdf.open_metadata(set_pikepdf_as_editor=False, update_docinfo=False) as xmp:  # read-only audit never mutates the audited bytes
            ua_id, has_title = str(xmp.get("pdfuaid:part", "")) == str(ua_part), bool(xmp.get("dc:title"))
            pdfa_claim, pdfx_claim = xmp.pdfa_status, xmp.pdfx_status  # the document's OWN declared PDF/A·PDF/X claim (evidence, not a UaCheck)
        metadata = root.get(Name.Metadata)  # the raw /Metadata stream octets, never a mapping-view re-serialization
        declared = _declared(bytes(metadata.read_bytes()) if metadata is not None else b"")
        wtpdf_accessibility = not declared.isdisjoint(_WTPDF_ACCESSIBILITY)
        wtpdf_reuse = not declared.isdisjoint(_WTPDF_REUSE)
        # every pikepdf-touching predicate resolved to a plain bool/int here, so no qpdf `Object` escapes the borrow window
        marked = bool(mark_info.get(Name.Marked, False))
        has_struct = struct_root is not None
        namespaced = has_struct and Name("/Namespaces") in struct_root  # the PDF 2.0 structure-namespace entry UA2_NAMESPACES reads
        has_lang = bool(root.get(Name.Lang, ""))
        title_ok = has_title and bool(root.get(Name.ViewerPreferences, Dictionary()).get(Name.DisplayDocTitle, False))
        not_suspect = not bool(mark_info.get(Name.Suspects, False))
        unmapped = sum(1 for role in tally.roles if role not in _ELT and role not in mapped)  # standard membership IS `_ELT`'s key set
        monotone = all(b - a <= 1 for a, b in pairwise(tally.levels) if b > a)
        role_map_n = len(mapped)
    with pdf_oxide.PdfDocument.from_bytes(access.pdf) as oracle:  # independent in-process Rust oracle, deterministic close via __exit__
        verdict = oracle.validate_pdf_ua()  # {'valid': bool, 'errors': list, 'warnings': list} — the veraPDF-grade cross-check, read once
        oracle_valid, oracle_errors, oracle_warnings = bool(verdict["valid"]), len(verdict["errors"]), len(verdict["warnings"])
        has_tree = oracle.has_structure_tree()  # the independent structure-tree confirmation STRUCT_TREE reconciles against the pikepdf read
        xfa = oracle.has_xfa()  # a dynamic XFA form is a PDF/UA-1 §7.18.1 violation
        version = tuple(oracle.version())  # the (major, minor) version tuple — `version()` returns a pair, not a string
        text_flags = tuple(bool(oracle.has_text_layer(page_index)) for page_index in range(int(oracle.page_count)))  # per-page extractable-text evidence
        text_pages = sum(text_flags)
        structured = len(
            oracle.structured_warnings()
        )  # the oracle's `{category, page, message, spec_section}` structure diagnostics, folded as additional two-source evidence
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
    with pdf_oxide.PdfDocument.from_bytes(access.pdf) as doc:  # deterministic close via __exit__, never GC-reaped
        outcome = doc.convert_to_pdf_a(level)  # {'success': bool, 'level': str, 'actions': list, 'errors': list}, upgraded in place
        verified = doc.validate_pdf_a(level)  # {'valid': bool, 'level': str, 'errors': list, 'warnings': list} — the oracle on the SAME upgraded handle
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
    with pikepdf.open(BytesIO(access.pdf)) as pdf:  # read-only: claim, output intents, and page print geometry resolve to plain values here
        with pdf.open_metadata(set_pikepdf_as_editor=False, update_docinfo=False) as xmp:
            pdfx_claim = str(xmp.pdfx_status)  # the document's OWN declared PDF/X conformance, resolved before the qpdf handle frees
        intents = len(pdf.Root.get(Name.OutputIntents, Array()))  # every PDF/X level demands a declared output intent
        boxed = sum(Name.TrimBox in page.obj or Name.ArtBox in page.obj for page in pdf.pages)  # per-page print geometry
        page_count = len(pdf.pages)
    with pdf_oxide.PdfDocument.from_bytes(access.pdf) as oracle:  # independent in-process Rust PDF/X oracle, deterministic close via __exit__
        verdict = oracle.validate_pdf_x(
            level
        )  # {'valid': bool, 'level': str, 'errors': list, 'warnings': list} — read once, every value resolved before the handle frees
        valid, errors, warnings = bool(verdict["valid"]), len(verdict["errors"]), len(verdict["warnings"])
        structured, oracle_pages = len(oracle.structured_warnings()), int(oracle.page_count)
    if page_count != oracle_pages:
        # two independent parsers splitting on the page tree is itself a malformed-document verdict — one provider
        # misread /Pages, so no audit minted over either count is trustworthy and the seam converts the raise.
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

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
