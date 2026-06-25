# [PY_ARTIFACTS_EGRESS]

The security-and-navigation finishing close over an emitted PDF or Office container. `DocumentEgress` is ONE owner that takes bytes already authored by `folder:document/emit#DOCUMENT` and returns a sealed, navigable, watermarked, attachment-bearing, content-rewritten, redaction-burned, imposed, confidentiality-scrubbed, structure-optimized, or Office-(de)sealed artifact keyed by the runtime content key — it finishes an emitted artifact and never authors one. `EgressStep` is the closed `StrEnum` over the ten finishing operations, each a `Finisher` row in the `FINISHERS` policy table binding its single `FinishFact`-returning arm, its runtime `Band`, and its `office` receipt discriminant in one value; the table is the totality proof and `_worker_finish` re-resolves the SAME row, so the native lane carries no second `match`. The per-step call inputs are one frozen `msgspec.Struct` `Extras` carrying every step's typed material, admitted exactly once at `DocumentEgress.of` through the closed `EgressPayload` `TypedDict` and its module-level `TypeAdapter` — the `extra_items=str` band folds the format-discriminated Office credential axis into one `frozendict` while every declared key materializes a typed `Extras` field, so each arm reads `egress.extras.<field>` and the interior never re-validates, re-coerces, or reaches a stringly-keyed bag.

Every arm returns a `FinishFact` carrying the finished bytes beside the evidence it produced (page count, applied encryption `R`, authored outline depth, placed overlay or stripped-layer count), and the fold threads each fact onto a successor owner through `structs.replace` so `contribute` reads the arm's own evidence and projects the receipt without a second parse, a re-reader, or an in-process re-run of a worker-gated arm. `EgressFault` is the closed `@tagged_union` admission vocabulary `DocumentEgress.of` produces — `payload` for a rejected `EgressPayload` shape, `empty` for an empty finishing chain — while the arm-level provider exception families (`pikepdf.PdfError`, `pymupdf.FileDataError`, `msoffcrypto.exceptions.DecryptionError`/`FileFormatError`) convert to the runtime `BoundaryFault` at the `async_boundary` capsule, never a bare `raise` surviving into domain flow.

`Permissions`/`Encryption`/`Bookmark`/`Scrub`/`Label`/`Sanitize`/`Optimize`/`ContentEdit`/`Confidentiality` are the row-policy value objects the steps carry instead of scattered knobs. `Permissions` projects once to BOTH the qpdf `pikepdf.Permissions` eight-field axis and the pypdf permission bitmask; `Encryption` carries a `Strength` discriminant whose `_STRENGTHS` cross-product row projects in one read to the `(r, band, use_128bit)` triple, routing the encrypt close across the core pypdf leg (`RC4-40`/`RC4-128`/`AES-128` in-process) and the worker pikepdf leg (`AES-256`/R6) inside the one `_encrypt` arm. `Bookmark` carries the `add_outline_item` `bold`/`italic`/`color`/`fit`/`is_open` typography axis the OUTLINE fold reads per level. `Scrub` projects the full pymupdf `Document.scrub` sanitize-axis kwargs; `Label` projects the `add_redact_annot` typography axis; `Sanitize` selects the `pikepdf.sanitize` strip family plus annotation flatten and signature-field disable; `Optimize` selects the `Pdf.save` linearize/recompress/object-stream/deterministic-id strategy; `ContentEdit` folds the qpdf `parse_content_stream` operand+operator instructions through one immutable `Block.fold` threading the `/OC` marked-content stack — operator drop, `Name`-guarded resource rename, and OCG named-layer strip/flatten over the `BDC`/`BMC`…`EMC` span plus the `/Root/OCProperties` catalog prune, so a layered artifact's named layer is removed from BOTH its content and its catalog registration; `Confidentiality` carries the Office disposition (unlock the encrypted container OR re-seal a plaintext OOXML payload under a fresh agile container) so the PROTECT row is the full bidirectional confidentiality rail, not a decrypt-only half.

Ownership across the wire is single-lane. `pikepdf` owns qpdf-native encryption, page composition, attachments, the `parse_content_stream`/`unparse_content_stream` token model, the object model, the `sanitize` strip family, `flatten_annotations`, `acroform.disable_digital_signatures`, `remove_unreferenced_resources`, and the recompress/linearize/deterministic-id `Pdf.save` strategy; `pypdf` owns the pure-Python writer, the `Transformation` imposition algebra, in-process RC4/AES-128 encryption, and `add_outline_item` outline authoring; `pymupdf` owns the MuPDF redaction burn-in plus the `bake`/`scrub`/`subset_fonts` sanitize sweep that destroys content bytes irreversibly; `msoffcrypto` owns the encrypted-Office container detection, the `keyTypes` credential axis, decryption, AND OOXML re-encryption. Because `pikepdf` is the qpdf-native wheel reflecting on the gated `python_version<'3.15'` band, the `WATERMARK`/`ATTACH`/`REWRITE`/`SANITIZE`/`OPTIMIZE` arms and the `AES-256` `ENCRYPT` leg cross the runtime subprocess seam through `anyio.to_process.run_sync` exactly as `folder:document/emit#DOCUMENT` does, while `pymupdf`/`pypdf`/`msoffcrypto` resolve on the cp315 core; the `DocumentEgress.band(step)` method reads the `Finisher.band` table default plus the `Encryption.band` ENCRYPT override to select the lane per step, never a hardcoded native set.

Cross-cutting receipt emission is a definition-time aspect over the thin pure `_emit`: the runtime `@receipted(Redaction.STRUCTURAL)` weave drains `contribute` and emits through the runtime `Signals.emit_async`, harvesting the threaded `FinishFact` evidence without an inline `emit` per arm, while the `async_boundary` capsule is the fault weave that converts a provider raise into the runtime `BoundaryFault` rail; the owner reads as the receipt weave over a pure dispatch core, never inline-repeated concerns. The OUTLINE step folds the `folder:document/model#NODE` `SectionNode` heading tree so the bookmark hierarchy is recovered from the one semantic tree, and the REDACT step folds the `AnnotationNode` `AnnotKind.REDACTION` rects so destruction targets the one semantic tree. One polymorphic `finish` entry owns BOTH the singular step and the finishing CHAIN: a `EgressStep | tuple[EgressStep, ...]` discriminant threads the finished bytes step-to-step through the rail, so a watermark-then-impose-then-encrypt deliverable is one fold over the input shape, never a caller-orchestrated re-entry or a `mode`/`batch` knob. Re-signing is never an egress step and routes to `folder:../exchange/conformance#CONFORM`; PDF/A authoring routes to `folder:document/emit#DOCUMENT`; named-layer AUTHORING routes to `folder:../export/layered#LAYERED`. Every finish returns a `RuntimeRail[ContentKey]` and, through `DocumentEgress.contribute`, builds the one `folder:../core/receipt#RECEIPT` `ArtifactReceipt` case off the `Finisher.office` discriminant then yields the `Iterable[Receipt]` stream the runtime `ReceiptContributor` port declares.

## [01]-[INDEX]

- [01]-[FINISH]: `DocumentEgress` + `EgressStep` (ENCRYPT/OUTLINE/WATERMARK/ATTACH/IMPOSE/REWRITE/REDACT/SANITIZE/OPTIMIZE/PROTECT) policy-table dispatch over pikepdf/pypdf/pymupdf/msoffcrypto; the `Finisher` row binds each step to its `(band, arm, office)` triple and `_worker_finish` re-resolves the SAME row; `Extras` is the one frozen `msgspec.Struct` admitted once through `DocumentEgress.of` over the `EgressPayload` `TypedDict` + `TypeAdapter`, the `extra_items=str` band folding the Office credentials into a `frozendict`; every arm returns a `FinishFact` carrying the finished bytes beside its produced page/encryption-R/outline-depth/overlay evidence, `_emit` threads the fact onto the frozen owner through `structs.replace`, and `DocumentEgress.contribute` reads that fact and folds BOTH the PDF `ArtifactReceipt.Egress` and the Office `ArtifactReceipt.Office` case off the `Finisher.office` discriminant; `EgressFault` is the closed `@tagged_union` over the `payload`/`empty` admission causes `of` produces, the arm-level provider exception families converting to the runtime `BoundaryFault` at the `async_boundary` capsule; `Permissions`/`Encryption`/`Bookmark`/`Scrub`/`Label`/`Sanitize`/`Optimize`/`ContentEdit`/`Confidentiality` the row-policy value objects; the ENCRYPT step reads the one `Strength` row off `_STRENGTHS` into the `(r, band, use_128bit)` triple projecting to BOTH the pypdf `(use_128bit, algorithm)` pair and the pikepdf `R` level; the OUTLINE step folds the `SectionNode` heading tree under the `Bookmark` typography axis; the REDACT step burns the `AnnotKind.REDACTION` rects under the `Label` axis, flattens through `bake`, sweeps through the `Scrub` axis, subsets through `subset_fonts`; the SANITIZE step folds `Sanitize` over the `pikepdf.sanitize` strip family; the OPTIMIZE step folds `Optimize` over the `Pdf.save` strategy; the REWRITE step folds the qpdf object model through one immutable `ContentEdit` `Block.fold` over `parse_content_stream` plus the `/Root/OCProperties` catalog prune; the IMPOSE step the `pypdf.Transformation` n-up algebra; the PROTECT step folds `Confidentiality` over the format-discriminated `load_key`/`decrypt` unlock OR the `OOXMLFile.encrypt` re-seal; `finish` owns BOTH the singular step and the `EgressStep | tuple[EgressStep, ...]` finishing chain threading bytes step-to-step; the runtime `@receipted(Redaction.STRUCTURAL)` weave harvests `contribute` over the pure `_emit` and `async_boundary` is the fault-converting capsule.

## [02]-[FINISH]

- Owner: `DocumentEgress` the one finishing-close owner discriminating the egress step; `EgressStep` the closed `StrEnum` over the ten finishing operations; `FINISHERS` the `frozendict[EgressStep, Finisher]` policy table binding each step to one `Finisher` row carrying its `Band`, its single `FinishFact`-returning arm, and its `office` discriminant — the table is the totality proof, one arm per step and `_worker_finish` re-resolving the SAME row, never an `if step == ...` cascade and never a parallel native frozenset. `FinishFact` the bytes-plus-evidence carrier every arm returns so the receipt reads the page/encryption-R/outline-depth/overlay facts the arm produced, threaded onto the owner through `structs.replace`. `Strength` the closed `StrEnum` whose `_STRENGTHS` cross-product table projects the `(r, band, use_128bit)` triple in one read. `Permissions`/`Encryption`/`Bookmark`/`Scrub`/`Label`/`Sanitize`/`Optimize`/`ContentEdit`/`Confidentiality` the row-policy value objects the steps carry. pikepdf owns qpdf-native AES encryption, overlay/underlay composition, attachment specs, the `parse_content_stream`/`unparse_content_stream` token model, the object model, and the `Job.run` declarative pipeline; pypdf owns `PdfWriter.add_outline_item`, the `Transformation` imposition algebra, and in-process `PdfWriter.encrypt`; pymupdf owns `Page.apply_redactions` plus the `Document.bake`/`scrub`/`subset_fonts` sanitize sweep; msoffcrypto owns `OfficeFile` detection, the `keyTypes` credential axis, `decrypt`, and `OOXMLFile.encrypt`.
- Cases: `EgressStep` rows · `ENCRYPT` (the one band-internal encrypt arm — `Encryption.band` selects the core `PdfWriter.encrypt(use_128bit, algorithm, permissions_flag)` `RC4`/`AES-128` leg or the worker `Pdf.save(encryption=pikepdf.Encryption(owner, user, R, aes, metadata, allow))` `AES-256`/R6 leg inside the single `_encrypt` body, the one `Strength` row projecting its `(r, band, use_128bit)` triple off `_STRENGTHS` into both legs, the one `Permissions` policy projecting to both flag sets) · `OUTLINE` (the bookmark-tree authoring arm folding the `SectionNode` heading hierarchy through `PdfWriter.add_outline_item` under the `Bookmark` `(bold, italic, color, fit, is_open)` axis into nested bookmarks, tracking the deepest authored `level` as the receipt depth, or lowering the `extras.bookmarks` fallback when no `DocumentNode` tree is supplied) · `WATERMARK` (pikepdf opening the stamp into a named `Pdf` that outlives the loop, `contents_coalesce` forcing its `/Contents` indirect, then `Page.add_overlay`/`add_underlay` copying it across documents, the placed-overlay count one `sum` over the apply-and-count generator) · `ATTACH` (`Pdf.attachments[name] = AttachedFileSpec(pdf, data, filename, description)` embedding a file into the catalog) · `IMPOSE` (pypdf n-up/booklet imposition folding each source `PageObject` onto a target sheet through `Transformation` then `merge_page` over `range`-window slicing) · `REWRITE` (pikepdf object-model surgery folding the qpdf `parse_content_stream` operand+operator instructions through one immutable `Block.fold` whose carried `(stack, kept)` threads the `/OC` `BDC`/`BMC`…`EMC` marked-content stack — operator drop by `str(op)`, `Name`-guarded resource rename, and OCG strip/flatten over the `LayerMode` per-region stack — writing `make_stream(unparse_content_stream(...))` into `page.obj[Name.Contents]` then pruning the stripped OCGs from the `/Root/OCProperties` `/OCGs`/`/D` catalog, reporting the catalog-removed layer count) · `REDACT` (pymupdf staging each `AnnotKind.REDACTION` rect as a black `add_redact_annot` carrying the optional `overlay_text` under the `Label` typography axis, burning through `apply_redactions` with the `PDF_REDACT_*` flag triple, flattening through `bake`, sweeping through `scrub(**Scrub.kwargs())`, subsetting through `subset_fonts(fallback=False)`, destroying the underlying bytes irreversibly) · `SANITIZE` (pikepdf folding `Sanitize` over the `pikepdf.sanitize` strip family plus `flatten_annotations` and `acroform.disable_digital_signatures`) · `OPTIMIZE` (pikepdf folding `Optimize` over `remove_unreferenced_resources` plus the `Pdf.save` linearize/recompress/object-stream/deterministic-id strategy) · `PROTECT` (the Office confidentiality arm folding `Confidentiality` over `OfficeFile.is_encrypted` gating then EITHER the format-discriminated `load_key(**credentials, **verify)`/`decrypt(outfile, **verify)` unlock — the OOXML `keyTypes` axis or the legacy-97 `("password",)` axis recovered through one `hasattr(office, "keyTypes")` discriminant — OR the `OOXMLFile.encrypt(password, outfile)` re-seal of a plaintext OOXML payload under a fresh agile container) — each one `Finisher.arm` resolved off `FINISHERS`, never re-enumerated by a worker-side `match`.
- Entry: `DocumentEgress.finish` is `async` over the runtime `async_boundary`, dispatching the step inside the one fault capsule. `step` is `EgressStep | tuple[EgressStep, ...]`: a singular step runs its one arm, a finishing CHAIN folds the sequence through the rail threading `FinishFact.data` step-to-step (each successor `structs.replace`-ing the owner's `source` with the prior finished bytes), so a watermark-then-impose-then-encrypt deliverable is one polymorphic fold over the input shape, never a name suffix or a `batch` knob. `DocumentEgress.band(step)` reads the `Finisher.band` table default and overrides it from `Encryption.band` for ENCRYPT, routing the cp315-core arms in-process and the worker-band arms across `anyio.to_process.run_sync` onto `_worker_finish`; the entry returns a `RuntimeRail[ContentKey]` keyed by the content key minted over the final `FinishFact.data`.
- Auto: `_stepped` routes on `DocumentEgress.band(step)` to the in-process `FINISHERS[step].arm(self)` call or the `to_process.run_sync(_worker_finish, self)` worker, then threads the returned `FinishFact` onto a successor owner through `structs.replace(self, fact=..., source=fact.data)` — the frozen seed is never mutated, the successor carries both the evidence and the reseeded bytes. `finished` runs `_stepped` per step folding to the final owner and `_emit` mints the content key over its `source` (the last finished bytes) through `ContentIdentity.of`; `contribute` reads the final owner's `fact`. `_worker_finish` re-resolves `FINISHERS[egress.step].arm` and calls it, so the worker carries no second step `match`, the `FinishFact` (a flat `msgspec.Struct` of bytes and ints) crosses the process seam intact, and the arm body is identical in either lane. The ENCRYPT arm discriminates on `Encryption.band` inside `_encrypt`: a `WORKER`-band `Encryption` runs the pikepdf leg projecting `Permissions.to_pikepdf`, otherwise the pypdf leg projects `Encryption.use_128bit`/`algorithm` plus `Permissions.to_pypdf`. Every arm imports its package at boundary scope inside the arm body; no native import lands on the core owner.
- Receipt: each finish contributes one `folder:../core/receipt#RECEIPT` case. The runtime `@receipted(Redaction.STRUCTURAL)` weave wraps `_emit`, draining `DocumentEgress.contribute` and emitting through `Signals.emit_async`; `contribute` reads the threaded `FinishFact` off `self.fact` (never a re-run of a worker-gated arm), re-mints the content key over `fact.data`, and folds the case off `Finisher.office` in one expression — the PDF-finishing rows emit `ArtifactReceipt.Egress` carrying the content key, the post-finish byte count, and the `fact.pages`/`encryption_r`/`outline_depth`/`overlays` evidence the arm produced (each defaulting to `0` off the rows that do not produce it; the REWRITE `fact.layers_removed` OCG-strip count rides the `overlays` layer-composition slot since an OCG strip is the inverse layer operation the watermark overlay count measures), and the `PROTECT` step emits `ArtifactReceipt.Office` carrying the content key and the decrypted-or-resealed byte count — one shared receipt stream off the one `FinishFact` shape, every row contributing through the SAME projection rather than a per-row receipt delegate. The case's own `contribute` projects through the runtime `Receipt.of("artifacts", ("emitted", subject, facts))` contract, so the egress owner declares the canonical streaming port like every sibling consumer and carries no second port method.
- Packages: `pikepdf` (`open`/`Pdf.save(linearize, encryption, recompress_flate, deterministic_id, object_stream_mode)`/`Encryption(owner, user, R, aes, metadata, allow)`/`Permissions`/`Pdf.pages`/`Pdf.Root`/`Page`/`Page.add_overlay`/`add_underlay`/`Page.contents_coalesce`/`Pdf.attachments`/`AttachedFileSpec`/`parse_content_stream`/`unparse_content_stream`/`Pdf.make_stream`/`Page.obj`/`Object`/`Object.get`/`Dictionary`/`Array`/`Name`/`ObjectStreamMode`/`sanitize`/`Pdf.flatten_annotations`/`Pdf.acroform.disable_digital_signatures`/`Pdf.remove_unreferenced_resources`, qpdf-native wheel crossing the subprocess seam), `pypdf` (`PdfReader`/`PdfWriter`/`PdfWriter(clone_from)`/`PdfWriter.pages`/`PdfWriter.add_outline_item(title, page_number, parent, color, bold, italic, fit, is_open)`/`PdfWriter.encrypt(use_128bit, algorithm, permissions_flag)`/`PageObject.add_transformation`/`merge_page`/`add_blank_page`/`Transformation`/`generic.Fit`/`constants.UserAccessPermissions`, pure-Python on cp315), `pymupdf` (`open`/`Page.add_redact_annot(text, fontname, fontsize, align, fill, text_color, cross_out)`/`Page.apply_redactions`/`Document.page_count`/`Document.bake`/`Document.scrub`/`Document.subset_fonts`/`Document.save`/`Rect`/`PDF_REDACT_IMAGE_REMOVE`/`PDF_REDACT_LINE_ART_REMOVE_IF_TOUCHED`/`PDF_REDACT_TEXT_REMOVE`, native MuPDF on cp315), `msoffcrypto` (`OfficeFile`/`BaseOfficeFile.is_encrypted`/`OOXMLFile.keyTypes`/`load_key`/`decrypt`/`OOXMLFile.encrypt`/`exceptions.FileFormatError`/`InvalidKeyError`/`DecryptionError`, pure-Python on cp315); `folder:document/model#NODE` (`DocumentNode`/`SectionNode`/`AnnotationNode`/`AnnotKind`/`RunNode`/`walk`); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`, `receipts.Receipt`/`ReceiptContributor`/`Phase`/`receipted`/`Redaction`, `anyio.to_process.run_sync`).
- Growth: a new finishing step is one `EgressStep` row plus one `Finisher` row carrying its band, arm, and office discriminant; a new receipt fact is one `FinishFact` field the arm populates, never a re-derivation off the bytes; a new permission knob is one `Permissions` field projected once into both flag sets; an encryption strength is one `Strength` row plus one `_STRENGTHS` cell carrying its `(r, band, use_128bit)` triple, never a parallel encrypt owner; a sanitize knob is one `Scrub` field; a bookmark-typography knob is one `Bookmark` field; a redaction-label knob is one `Label` field; a credential kind is already carried by the format-discriminated credential axis; an Office disposition (unlock vs re-seal) is one `Confidentiality` case; a content-stream edit is one `ContentEdit` field folded through `_folded_stream`'s `Block.fold`; an OCG named-layer removal is one `strip_layers`/`flatten_layers` membership; a deeper finishing chain is one more `EgressStep` in the sequence the rail already folds; zero new surface.
- Boundary: this owner finishes an already-emitted artifact and authors none — a PDF-from-scratch path, a per-finishing-step service class family, a stringly-typed `if step == "encrypt"` branch, scattered boolean permission knobs, a hardcoded native frozenset, a worker-side sentinel arm, a per-row receipt delegate, a second worker `match`, a re-parse of the finished bytes, a re-run of a worker-gated arm in the contribute path, a `TokenFilter` whose flat token stream cannot pair the `/OC` operand with its `BDC` operator, a mutable-list content-stream accumulator, and an `(r, aes)` ternary admitting the invalid R2/AES-128 cell are the deleted forms. PAdES cryptographic re-signing routes to `folder:../exchange/conformance#CONFORM`; PDF/A structural authoring routes to `folder:document/emit#DOCUMENT`; the recover-TO extraction half routes to `folder:document/lens#LENS`. The REWRITE OCG edit STRIPS or FLATTENS an existing named layer but never AUTHORS one — minting a named editable layer is the `folder:../export/layered#LAYERED` owner's concern, so the egress close consumes an OCG-layered artifact and can subtract a layer, while layer composition is the upstream export owner. Office confidentiality is the full bidirectional rail — `PROTECT` gates on `is_encrypted`, unlocks an encrypted container through the format-discriminated credential axis, OR re-seals a plaintext OOXML payload through `OOXMLFile.encrypt`, the legacy-97 formats decrypt-only so a re-seal of a 97 container is rejected at the `OOXMLFile.encrypt` type edge rather than a runtime flag. The `DocumentEgress.band(step)` method decides the runtime lane off the `Finisher.band` default plus the `Encryption.band` ENCRYPT override; every arm imports its package at boundary scope so neither a module-top nor a lazy native import lands on the core owner. The content key is minted over the final `FinishFact.data`, never re-minted off the source key.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable, Iterator
from enum import StrEnum
from functools import reduce
from io import BytesIO
from operator import or_
from typing import TYPE_CHECKING, Final, Literal, NotRequired, ReadOnly, Self, TypedDict, Unpack, assert_never

from anyio import to_process
from builtins import frozendict
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct, field, structs
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.receipts import Phase, Receipt, Redaction, receipted

from artifacts.core.receipt import ArtifactReceipt
from artifacts.document.model import AnnotKind, AnnotationNode, DocumentNode, SectionNode, walk

if TYPE_CHECKING:
    import pikepdf
    from pypdf.constants import UserAccessPermissions


# --- [TYPES] ----------------------------------------------------------------------------
class EgressStep(StrEnum):
    ENCRYPT = "encrypt"
    OUTLINE = "outline"
    WATERMARK = "watermark"
    ATTACH = "attach"
    IMPOSE = "impose"
    REWRITE = "rewrite"
    REDACT = "redact"
    SANITIZE = "sanitize"
    OPTIMIZE = "optimize"
    PROTECT = "protect"


class Band(StrEnum):
    CORE = "core"
    WORKER = "worker"


class Strength(StrEnum):
    RC4_40 = "RC4-40"
    RC4_128 = "RC4-128"
    AES_128 = "AES-128"
    AES_256 = "AES-256"


class LayerMode(StrEnum):
    KEEP = "keep"
    FLATTEN = "flatten"
    STRIP = "strip"


class FitMode(StrEnum):  # the pypdf `generic.Fit` destination-zoom family the outline node carries
    FIT = "fit"
    FIT_H = "fit_h"
    FIT_V = "fit_v"
    FIT_B = "fit_b"
    XYZ = "xyz"


class StreamDecode(StrEnum):  # the `pikepdf.StreamDecodeLevel` re-encode strength the OPTIMIZE close projects
    NONE = "none"
    GENERALIZED = "generalized"
    SPECIALIZED = "specialized"
    ALL = "all"


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class EgressFault:
    # the closed ADMISSION vocabulary `of` produces; the arm-level provider raises
    # (`pikepdf.PdfError`, `pymupdf.FileDataError`, `msoffcrypto.DecryptionError`) convert to the
    # runtime `BoundaryFault` at the `async_boundary` capsule, never into this interior vocabulary.
    tag: Literal["payload", "empty"] = tag()
    payload: tuple[str, ...] = case()        # the rejected payload key paths
    empty: None = case()                     # an empty finishing chain


# --- [CONSTANTS] ------------------------------------------------------------------------
_STRENGTHS: Final[frozendict[Strength, tuple[int, Band, bool]]] = frozendict({
    Strength.RC4_40: (2, Band.CORE, False),
    Strength.RC4_128: (4, Band.CORE, False),
    Strength.AES_128: (4, Band.CORE, True),
    Strength.AES_256: (6, Band.WORKER, True),
})
_FITS: Final[frozendict[FitMode, str]] = frozendict({  # the FitMode -> `generic.Fit` factory the OUTLINE node projects
    FitMode.FIT: "fit",
    FitMode.FIT_H: "fit_horizontally",
    FitMode.FIT_V: "fit_vertically",
    FitMode.FIT_B: "fit_box",
    FitMode.XYZ: "xyz",
})


# --- [MODELS] ---------------------------------------------------------------------------
class Permissions(Struct, frozen=True):
    accessibility: bool = True
    extract: bool = False
    modify: bool = False
    print_lowres: bool = True
    print_highres: bool = False
    annotate: bool = False
    fill_forms: bool = False
    assemble: bool = False

    def to_pikepdf(self) -> "pikepdf.Permissions":
        import pikepdf

        return pikepdf.Permissions(
            accessibility=self.accessibility,
            extract=self.extract,
            modify_other=self.modify,
            modify_annotation=self.annotate,
            modify_assembly=self.assemble,
            modify_form=self.fill_forms,
            print_lowres=self.print_lowres,
            print_highres=self.print_highres,
        )

    def to_pypdf(self) -> "UserAccessPermissions":
        from pypdf.constants import UserAccessPermissions as P

        rows = (
            (self.extract, P.EXTRACT),
            (self.accessibility, P.EXTRACT_TEXT_AND_GRAPHICS),
            (self.modify, P.MODIFY),
            (self.annotate, P.ADD_OR_MODIFY),
            (self.assemble, P.ASSEMBLE_DOC),
            (self.fill_forms, P.FILL_FORM_FIELDS),
            (self.print_lowres, P.PRINT),
            (self.print_highres, P.PRINT_TO_REPRESENTATION),
        )
        return reduce(or_, (flag for gate, flag in rows if gate), P(0))


class Encryption(Struct, frozen=True):
    owner: str
    user: str = ""
    strength: Strength = Strength.AES_256
    encrypt_metadata: bool = True

    @property
    def r(self) -> int:
        return _STRENGTHS[self.strength][0]

    @property
    def band(self) -> Band:
        return _STRENGTHS[self.strength][1]

    @property
    def use_128bit(self) -> bool:
        return self.r >= 4

    @property
    def algorithm(self) -> str:
        return self.strength.value


class Bookmark(Struct, frozen=True):
    bold_top: bool = True
    italic: bool = False
    color: tuple[float, float, float] | None = None
    open_depth: int = 1
    fit: FitMode = FitMode.FIT

    def style(self, level: int, /) -> dict[str, object]:
        from pypdf.generic import Fit

        return {"bold": level == 1 and self.bold_top, "italic": self.italic, "color": self.color, "fit": getattr(Fit, _FITS[self.fit])(), "is_open": level < self.open_depth}


class Scrub(Struct, frozen=True):
    metadata: bool = True
    xml_metadata: bool = True
    attached_files: bool = True
    embedded_files: bool = True
    hidden_text: bool = True
    javascript: bool = True
    remove_links: bool = False
    reset_fields: bool = False
    reset_responses: bool = False
    thumbnails: bool = False
    clean_pages: bool = True
    redact_images: int = 0

    def kwargs(self) -> dict[str, bool | int]:
        return {**structs.asdict(self), "redactions": False}


class Label(Struct, frozen=True):
    fontname: str = "helv"
    fontsize: float = 11.0
    align: int = 0
    fill: tuple[float, float, float] = (0.0, 0.0, 0.0)        # the burned-region fill colour
    text_color: tuple[float, float, float] = (1.0, 1.0, 1.0)  # the overlay-text colour
    cross_out: bool = True                                    # struck-through default when no overlay text rides the rect

    def kwargs(self, overlay: str | None, /) -> dict[str, object]:
        # the full `add_redact_annot` typography axis; an overlay string suppresses the strike-out
        return {**structs.asdict(self), "cross_out": self.cross_out and overlay is None, "text": overlay}


class Sanitize(Struct, frozen=True):
    javascript: bool = True
    external_access: bool = True
    multimedia: bool = True
    attachments: bool = False
    private_app_data: bool = True
    flatten_annotations: bool = True
    disable_signatures: bool = False


class Optimize(Struct, frozen=True):
    linearize: bool = True
    sweep_unreferenced: bool = True
    recompress: bool = True
    deterministic_id: bool = True
    object_streams: bool = True
    compress_streams: bool = True
    stream_decode: StreamDecode = StreamDecode.GENERALIZED


class ContentEdit(Struct, frozen=True):
    drop_operators: frozenset[str] = frozenset()
    rename_resources: frozendict[str, str] = field(default_factory=frozendict)
    strip_layers: frozenset[str] = frozenset()
    flatten_layers: frozenset[str] = frozenset()

    @property
    def touches_layers(self) -> bool:
        return bool(self.strip_layers or self.flatten_layers)

    def layer_mode(self, name: str, /) -> LayerMode:
        return LayerMode.STRIP if name in self.strip_layers else LayerMode.FLATTEN if name in self.flatten_layers else LayerMode.KEEP


@tagged_union(frozen=True)
class Confidentiality:
    # the Office disposition: `unlock` an encrypted container through the format-discriminated
    # credential axis, or `reseal` a plaintext OOXML payload under a fresh agile container; only
    # the OOXML object carries `encrypt`, so a 97-container re-seal is unrepresentable by case.
    tag: Literal["unlock", "reseal"] = tag()
    unlock: bool = case()        # `verify` — fold the HMAC/password verification into load+decrypt
    reseal: str = case()         # the re-seal password


class FinishFact(Struct, frozen=True):
    data: bytes
    pages: int = 0
    encryption_r: int = 0
    outline_depth: int = 0
    overlays: int = 0
    layers_removed: int = 0


class Extras(Struct, frozen=True, omit_defaults=True):
    stamp: bytes = b""
    under: bool = False
    bookmarks: tuple[tuple[str, int], ...] = ()
    attachment_name: str = ""
    attachment_data: bytes = b""
    attachment_description: str = ""
    across: int = 2
    down: int = 1
    sheet: tuple[float, float] = (612.0, 792.0)
    overlay_text: str = ""
    flatten_widgets: bool = True
    credentials: frozendict[str, str] = field(default_factory=frozendict)


# --- [BOUNDARIES] -----------------------------------------------------------------------
class EgressPayload(TypedDict, extra_items=str):
    stamp: NotRequired[ReadOnly[bytes]]
    under: NotRequired[ReadOnly[bool]]
    bookmarks: NotRequired[ReadOnly[tuple[tuple[str, int], ...]]]
    attachment_name: NotRequired[ReadOnly[str]]
    attachment_data: NotRequired[ReadOnly[bytes]]
    attachment_description: NotRequired[ReadOnly[str]]
    across: NotRequired[ReadOnly[int]]
    down: NotRequired[ReadOnly[int]]
    sheet: NotRequired[ReadOnly[tuple[float, float]]]
    overlay_text: NotRequired[ReadOnly[str]]
    flatten_widgets: NotRequired[ReadOnly[bool]]


_PAYLOAD: Final = TypeAdapter(EgressPayload)
_DECLARED: Final[frozenset[str]] = EgressPayload.__optional_keys__ | EgressPayload.__required_keys__


# --- [SERVICES] -------------------------------------------------------------------------
class Finisher(Struct, frozen=True):
    band: Band
    arm: Callable[["DocumentEgress"], FinishFact]
    office: bool = False


class DocumentEgress(Struct, frozen=True):
    step: EgressStep | tuple[EgressStep, ...]
    source: bytes
    extras: Extras = field(default_factory=Extras)
    node: DocumentNode | None = None
    permissions: Permissions = Permissions()
    encryption: Encryption | None = None
    bookmark: Bookmark = Bookmark()
    scrub: Scrub = Scrub()
    label: Label = Label()
    sanitize: Sanitize = Sanitize()
    optimize: Optimize = Optimize()
    edit: ContentEdit = ContentEdit()
    confidentiality: Confidentiality = field(default_factory=lambda: Confidentiality(unlock=True))
    fact: FinishFact | None = None

    @property
    def steps(self) -> tuple[EgressStep, ...]:
        return self.step if isinstance(self.step, tuple) else (self.step,)

    def band(self, step: EgressStep, /) -> Band:
        if step is EgressStep.ENCRYPT and self.encryption is not None:
            return self.encryption.band
        return FINISHERS[step].band

    async def finish(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"egress.{'+'.join(self.steps)}", self._emit)

    async def _stepped(self, step: EgressStep, /) -> Self:
        staged = structs.replace(self, step=step)
        fact = await to_process.run_sync(_worker_finish, staged) if staged.band(step) is Band.WORKER else FINISHERS[step].arm(staged)
        return structs.replace(staged, fact=fact, source=fact.data)

    async def finished(self) -> Self:
        # the immutable fold: each step threads the prior finished bytes forward and the returned
        # owner carries the final `FinishFact`, so the frozen owner is never mutated in place.
        live = self
        for step in self.steps:
            live = await live._stepped(step)
        return live

    @receipted(Redaction.STRUCTURAL)  # the runtime weave drains `contribute` off the finished owner and emits via `Signals.emit_async`
    async def _emit(self) -> ContentKey:
        live = await self.finished()
        return ContentIdentity.of(f"egress-{live.steps[-1]}", live.source)  # `_stepped` reseeds `source` to the final finished bytes

    def contribute(self, phase: Phase = "emitted") -> Iterable[Receipt]:
        if (fact := self.fact) is None:  # contribute rides the finished owner the fold returned, never the pre-fold seed
            return
        key = ContentIdentity.of(f"egress-{self.steps[-1]}", fact.data)
        case = (
            ArtifactReceipt.Office(key, len(fact.data))
            if FINISHERS[self.steps[-1]].office
            else ArtifactReceipt.Egress(key, len(fact.data), fact.pages, fact.encryption_r, fact.outline_depth, fact.overlays + fact.layers_removed)
        )
        yield from case.contribute(phase)

    @classmethod
    def of(cls, step: EgressStep | tuple[EgressStep, ...], source: bytes, /, **raw: Unpack[EgressPayload]) -> Result[Self, EgressFault]:
        if isinstance(step, tuple) and not step:
            return Error(EgressFault(empty=None))
        try:
            payload = _PAYLOAD.validate_python(raw)
        except ValidationError as fault:
            return Error(EgressFault(payload=tuple(str(error["loc"]) for error in fault.errors())))
        credentials = frozendict({name: value for name, value in payload.items() if name not in _DECLARED})
        known = {name: value for name, value in payload.items() if name in _DECLARED}
        return Ok(cls(step=step, source=source, extras=Extras(credentials=credentials, **known)))


# --- [OPERATIONS] -----------------------------------------------------------------------
def _worker_finish(egress: "DocumentEgress") -> FinishFact:
    return FINISHERS[egress.step].arm(egress) if not isinstance(egress.step, tuple) else FINISHERS[egress.steps[-1]].arm(egress)


def _sections(node: DocumentNode | None, /) -> Iterator[SectionNode]:
    return (n for n in walk(node) if isinstance(n, SectionNode)) if node is not None else iter(())


def _redaction_rects(node: DocumentNode | None, /) -> dict[int, tuple[tuple[float, float, float, float], ...]]:
    annots = (n for n in (walk(node) if node is not None else ()) if isinstance(n, AnnotationNode) and n.annot is AnnotKind.REDACTION)
    return reduce(lambda acc, a: {**acc, a.meta.page: (*acc.get(a.meta.page, ()), a.target)}, annots, {})


def _encrypt(egress: DocumentEgress) -> FinishFact:
    enc, sink = egress.encryption, BytesIO()
    if enc.band is Band.WORKER:
        import pikepdf

        pdf = pikepdf.open(BytesIO(egress.source))
        pdf.save(sink, linearize=True, encryption=pikepdf.Encryption(owner=enc.owner, user=enc.user, R=enc.r, aes=True, metadata=enc.encrypt_metadata, allow=egress.permissions.to_pikepdf()))
        return FinishFact(sink.getvalue(), pages=len(pdf.pages), encryption_r=enc.r)
    from pypdf import PdfReader, PdfWriter

    writer = PdfWriter(clone_from=PdfReader(BytesIO(egress.source)))
    writer.encrypt(user_password=enc.user, owner_password=enc.owner, use_128bit=enc.use_128bit, algorithm=enc.algorithm, permissions_flag=egress.permissions.to_pypdf())
    writer.write(sink)
    return FinishFact(sink.getvalue(), pages=len(writer.pages), encryption_r=enc.r)


type _Outline = tuple[frozendict[int, object], int]


def _outline(egress: DocumentEgress) -> FinishFact:
    from pypdf import PdfReader, PdfWriter

    writer = PdfWriter(clone_from=PdfReader(BytesIO(egress.source)))

    def author(state: _Outline, section: SectionNode, /) -> _Outline:  # the boundary `add_outline_item` is the seam; the parent/depth thread is pure
        parents, depth = state
        node = writer.add_outline_item("".join(run.text for run in section.heading), section.meta.page, parent=parents.get(section.level - 1), **egress.bookmark.style(section.level))
        return parents | {section.level: node}, max(depth, section.level)
    _parents, depth = Block.of_seq(_sections(egress.node)).fold(author, (frozendict(), 0))
    if egress.node is None:
        Block.of_seq(egress.extras.bookmarks).iter(lambda row: writer.add_outline_item(row[0], row[1]))
    sink = BytesIO()
    writer.write(sink)
    return FinishFact(sink.getvalue(), pages=len(writer.pages), outline_depth=depth)


def _impose(egress: DocumentEgress) -> FinishFact:
    from pypdf import PdfReader, PdfWriter, Transformation

    reader = PdfReader(BytesIO(egress.source))
    across, down = egress.extras.across, egress.extras.down
    width, height = egress.extras.sheet
    cell_w, cell_h, slots = width / across, height / down, across * down
    writer = PdfWriter()

    def placed(sheet: "PageObject", indexed: tuple[int, int], /) -> "PageObject":  # `add_transformation`/`merge_page` mutate in place at the pypdf seam
        offset, index = indexed
        row, col = divmod(offset, across)
        source = reader.pages[index]
        source.add_transformation(Transformation().scale(cell_w / source.mediabox.width, cell_h / source.mediabox.height).translate(col * cell_w, (down - 1 - row) * cell_h))
        sheet.merge_page(source)
        return sheet

    def imposed(window: tuple[int, ...], /) -> object:
        return Block.of_seq(enumerate(window)).fold(placed, writer.add_blank_page(width=width, height=height))
    Block.of_seq(batched(range(len(reader.pages)), slots)).iter(imposed)
    sink = BytesIO()
    writer.write(sink)
    return FinishFact(sink.getvalue(), pages=len(writer.pages))


def _watermark(egress: DocumentEgress) -> FinishFact:
    import pikepdf

    pdf = pikepdf.open(BytesIO(egress.source))
    stamp = pikepdf.open(BytesIO(egress.extras.stamp))
    mark = pikepdf.Page(stamp.pages[0])
    mark.contents_coalesce()
    place = pikepdf.Page.add_underlay if egress.extras.under else pikepdf.Page.add_overlay
    overlays = sum(place(pikepdf.Page(page), mark) is None for page in pdf.pages)
    sink = BytesIO()
    pdf.save(sink, linearize=True)
    return FinishFact(sink.getvalue(), pages=len(pdf.pages), overlays=overlays)


def _attach(egress: DocumentEgress) -> FinishFact:
    import pikepdf

    pdf = pikepdf.open(BytesIO(egress.source))
    name = egress.extras.attachment_name
    pdf.attachments[name] = pikepdf.AttachedFileSpec(pdf, egress.extras.attachment_data, filename=name, description=egress.extras.attachment_description)
    sink = BytesIO()
    pdf.save(sink, linearize=True)
    return FinishFact(sink.getvalue(), pages=len(pdf.pages))


type _Instr = tuple[list[object], object]
type _Fold = tuple[tuple[LayerMode, ...], Block[_Instr]]


def _folded_stream(pikepdf: object, page: "pikepdf.Page", edit: ContentEdit) -> bytes:
    # the OCG-aware content fold over `parse_content_stream`'s operand+operator instructions:
    # the carried `(stack, kept)` threads the `/OC` BDC/BMC…EMC marked-content stack immutably so a
    # STRIP layer's whole span is omitted and a FLATTEN layer keeps its body without its markers,
    # while every surviving instruction drops a dropped operator and renames a `Name`-guarded operand.
    rename = {pikepdf.Name(f"/{k}"): pikepdf.Name(f"/{v}") for k, v in edit.rename_resources.items()}

    def step(state: _Fold, instr: _Instr, /) -> _Fold:
        # a KEEP-mode marker survives; a STRIP/FLATTEN BDC/EMC marker is dropped (the FLATTEN body
        # then passes the STRIP-only guard below, the STRIP body is dropped by it).
        stack, kept = state
        operands, op = instr
        token = str(op)
        if token in ("BDC", "BMC"):
            name = _oc_name(pikepdf, page, operands) if operands and str(operands[0]) == "/OC" else ""
            mode = edit.layer_mode(name) if name else LayerMode.KEEP
            return (*stack, mode), (kept.append(Block.singleton(instr)) if mode is LayerMode.KEEP else kept)
        if token == "EMC":
            mode = stack[-1] if stack else LayerMode.KEEP
            return stack[:-1], (kept.append(Block.singleton(instr)) if mode is LayerMode.KEEP else kept)
        if any(m is LayerMode.STRIP for m in stack) or token in edit.drop_operators:
            return stack, kept
        renamed = [rename.get(tok, tok) if isinstance(tok, pikepdf.Name) else tok for tok in operands]
        return stack, kept.append(Block.singleton((renamed, op)))

    _residual, kept = Block.of_seq(pikepdf.parse_content_stream(page)).fold(step, ((), Block.empty()))
    return pikepdf.unparse_content_stream(list(kept))


def _oc_name(pikepdf: object, page: "pikepdf.Page", operands: "list[object]") -> str:
    marker = operands[1] if len(operands) > 1 else None
    if isinstance(marker, pikepdf.Dictionary):
        return str(marker.get(pikepdf.Name.Name, ""))
    properties = page.obj.get(pikepdf.Name.Resources, pikepdf.Dictionary()).get(pikepdf.Name.Properties, pikepdf.Dictionary())
    ocg = properties.get(pikepdf.Name(str(marker)), pikepdf.Dictionary()) if marker is not None else pikepdf.Dictionary()
    target = ocg.get(pikepdf.Name.OCGs, ocg) if str(ocg.get(pikepdf.Name.Type, "")) == "/OCMD" else ocg
    return str(target.get(pikepdf.Name.Name, ""))


def _strip_ocg_catalog(pikepdf: object, pdf: "pikepdf.Pdf", removed: frozenset[str]) -> int:
    root = pdf.Root
    if pikepdf.Name.OCProperties not in root:
        return 0
    ocprops = root[pikepdf.Name.OCProperties]
    survivors = [ocg for ocg in ocprops.get(pikepdf.Name.OCGs, pikepdf.Array()) if str(ocg.get(pikepdf.Name.Name, "")) not in removed]
    cut = len(ocprops.get(pikepdf.Name.OCGs, pikepdf.Array())) - len(survivors)
    ocprops[pikepdf.Name.OCGs] = pikepdf.Array(survivors)
    config = ocprops.get(pikepdf.Name.D, pikepdf.Dictionary())
    for axis in (pikepdf.Name.ON, pikepdf.Name.OFF, pikepdf.Name("/Order")):
        if axis in config:
            config[axis] = pikepdf.Array([ref for ref in config[axis] if ref in survivors])
    return cut


def _rewrite(egress: DocumentEgress) -> FinishFact:
    import pikepdf

    pdf = pikepdf.open(BytesIO(egress.source))
    for page in pdf.pages:
        page.obj[pikepdf.Name.Contents] = pdf.make_stream(_folded_stream(pikepdf, page, egress.edit))
    layers = _strip_ocg_catalog(pikepdf, pdf, egress.edit.strip_layers) if egress.edit.touches_layers else 0
    sink = BytesIO()
    pdf.save(sink, linearize=True)
    return FinishFact(sink.getvalue(), pages=len(pdf.pages), layers_removed=layers)


def _redact(egress: DocumentEgress) -> FinishFact:
    import pymupdf

    doc = pymupdf.open(stream=egress.source, filetype="pdf")
    overlay = egress.extras.overlay_text or None
    label = egress.label.kwargs(overlay)
    for index, page_rects in _redaction_rects(egress.node).items():
        page = doc[index]
        for rect in page_rects:
            page.add_redact_annot(pymupdf.Rect(rect), **label)
        page.apply_redactions(images=pymupdf.PDF_REDACT_IMAGE_REMOVE, graphics=pymupdf.PDF_REDACT_LINE_ART_REMOVE_IF_TOUCHED, text=pymupdf.PDF_REDACT_TEXT_REMOVE)
    doc.bake(annots=True, widgets=egress.extras.flatten_widgets)
    doc.scrub(**egress.scrub.kwargs())
    doc.subset_fonts(fallback=False)
    sink = BytesIO()
    doc.save(sink, garbage=4, deflate=True, clean=True)
    return FinishFact(sink.getvalue(), pages=doc.page_count)


def _sanitize(egress: DocumentEgress) -> FinishFact:
    import pikepdf
    from pikepdf import sanitize

    pol, pdf = egress.sanitize, pikepdf.open(BytesIO(egress.source))
    strips = (
        (pol.javascript, sanitize.remove_javascript),
        (pol.external_access, sanitize.remove_external_access),
        (pol.multimedia, sanitize.remove_multimedia),
        (pol.attachments, sanitize.remove_attachments),
        (pol.private_app_data, sanitize.remove_private_app_data),
    )
    for _gate, strip in (row for row in strips if row[0]):
        strip(pdf)
    if pol.disable_signatures:
        pdf.acroform.disable_digital_signatures()
    if pol.flatten_annotations:
        pdf.flatten_annotations(mode="all")
    sink = BytesIO()
    pdf.save(sink, linearize=True)
    return FinishFact(sink.getvalue(), pages=len(pdf.pages))


def _optimize(egress: DocumentEgress) -> FinishFact:
    import pikepdf

    pol, pdf = egress.optimize, pikepdf.open(BytesIO(egress.source))
    if pol.sweep_unreferenced:
        pdf.remove_unreferenced_resources()
    sink = BytesIO()
    pdf.save(
        sink,
        linearize=pol.linearize,
        compress_streams=pol.compress_streams,
        stream_decode_level=pikepdf.StreamDecodeLevel[pol.stream_decode.value],
        recompress_flate=pol.recompress,
        deterministic_id=pol.deterministic_id,
        object_stream_mode=pikepdf.ObjectStreamMode.generate if pol.object_streams else pikepdf.ObjectStreamMode.preserve,
    )
    return FinishFact(sink.getvalue(), pages=len(pdf.pages))


def _protect(egress: DocumentEgress) -> FinishFact:
    import msoffcrypto

    office = msoffcrypto.OfficeFile(BytesIO(egress.source))
    sink = BytesIO()
    match egress.confidentiality:
        case Confidentiality(tag="reseal", reseal=password):  # the inverse rail — only the OOXML object carries `encrypt`
            office.encrypt(password, sink)
        case Confidentiality(tag="unlock", unlock=verify):
            ooxml = hasattr(office, "keyTypes")
            key_axis = office.keyTypes if ooxml else ("password",)
            credentials = {kind: egress.extras.credentials[kind] for kind in key_axis if kind in egress.extras.credentials}
            office.load_key(**credentials, **({"verify_password": True} if ooxml and verify else {}))
            office.decrypt(sink, **({"verify_integrity": verify} if ooxml else {}))
        case _ as unreachable:
            assert_never(unreachable)
    return FinishFact(sink.getvalue())


# --- [COMPOSITION] ----------------------------------------------------------------------
FINISHERS: Final[frozendict[EgressStep, Finisher]] = frozendict({
    EgressStep.ENCRYPT: Finisher(Band.CORE, _encrypt),
    EgressStep.OUTLINE: Finisher(Band.CORE, _outline),
    EgressStep.WATERMARK: Finisher(Band.WORKER, _watermark),
    EgressStep.ATTACH: Finisher(Band.WORKER, _attach),
    EgressStep.IMPOSE: Finisher(Band.CORE, _impose),
    EgressStep.REWRITE: Finisher(Band.WORKER, _rewrite),
    EgressStep.REDACT: Finisher(Band.CORE, _redact),
    EgressStep.SANITIZE: Finisher(Band.WORKER, _sanitize),
    EgressStep.OPTIMIZE: Finisher(Band.WORKER, _optimize),
    EgressStep.PROTECT: Finisher(Band.CORE, _protect, office=True),
})
```
