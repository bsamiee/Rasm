# [PY_ARTIFACTS_EGRESS]

The security-and-navigation finishing close over an emitted PDF or Office container. `DocumentEgress` is ONE owner that takes bytes already authored by `folder:document/emit#DOCUMENT` and returns a sealed, navigable, watermarked, attachment-bearing, content-rewritten, redaction-burned, imposed, view-configured, confidentiality-scrubbed, structure-optimized, or Office-(de)sealed artifact keyed by the runtime content key — it finishes an emitted artifact and never authors one. `EgressStep` is the closed `StrEnum` over the eleven finishing operations, each a `Finisher` row in the `FINISHERS` policy table binding its single `FinishFact`-returning arm and its `office` receipt discriminant in one value; the table is the totality proof. Every arm resolves IN-PROCESS on the runtime — `pikepdf` (`worker-native` forward-compatible to runtime), `pymupdf` (`cp310-native`), and the pure-Python `pypdf`/`msoffcrypto` are all ungated in the manifest — so the close is one synchronous finishing fold the `_emit` weave crosses onto the GIL-releasing `anyio.to_thread` seam under a bounded `CapacityLimiter`, in-process exactly as `folder:document/tagged#ACCESS` finishes a `pikepdf` tree but never inline on the event loop and never a subprocess `Band` split — the heavy native render runs off the scheduler so a concurrent serve is never stalled.

Admission splits by trust. Trusted finishing policy enters as the one frozen `Finishing` value-object bundle — `Permissions`/`Encryption`/`Bookmark`/`Watermark`/`Attachment`/`Imposition`/`Viewer`/`Scrub`/`Label`/`Sanitize`/`Optimize`/`ContentEdit`/`Confidentiality`, each a behavior-carrying owner the caller constructs from its own validated source — while untrusted material (the watermark `stamp` bytes, the attachment payload bytes, the Office credentials) is admitted exactly once at `DocumentEgress.of` through the closed `EgressPayload` `TypedDict` and its module-level `TypeAdapter` into the `Extras` struct, the `extra_items=str` band folding the format-discriminated Office credential axis into one `frozendict`. `of` rejects an under-supplied step through the `_PREREQ` predicate table into `EgressFault.incomplete` before the fold runs — `ENCRYPT` without an `Encryption`, `WATERMARK` without a `stamp`, `ATTACH` without payload-or-name, `OUTLINE`/`REDACT` with neither a `DocumentNode` nor a fallback source — so the interior is total over admitted owners and never re-validates, re-coerces, or reaches a stringly-keyed bag.

Every arm returns a `FinishFact` carrying the finished bytes beside the evidence it produced (page count, applied encryption `R`, authored outline depth, placed overlay or stripped-layer count), and the fold threads each fact onto a successor owner through `structs.replace` so `contribute` reads the arm's own evidence and projects the receipt without a second parse or a re-reader. `EgressFault` is the closed `@tagged_union` admission vocabulary `DocumentEgress.of` produces — `payload` for a rejected `EgressPayload` shape, `empty` for an empty finishing chain, `incomplete` for a step missing its required owner — while the arm-level provider exception families (`pikepdf.PdfError`, `pymupdf.FileDataError`, `msoffcrypto.exceptions.DecryptionError`/`FileFormatError`) convert to the runtime `BoundaryFault` at the `async_boundary` capsule, never a bare `raise` surviving into domain flow.

The value objects carry every knob the steps need so no loose flag rides the signature. `Permissions` projects to the qpdf `pikepdf.Permissions` eight-field axis; `Encryption` carries a `Strength` discriminant whose `_STRENGTHS` row projects in one read to the `(r, aes)` pair driving the single `pikepdf.Encryption(owner, user, R, aes, metadata, allow)` leg across `RC4-40`/`RC4-128`/`AES-128`/`AES-256` (qpdf rejects metadata encryption below `AES`, so `Encryption.metadata` derives `aes and encrypt_metadata` rather than admitting the invalid RC4-with-encrypted-metadata cell). `Bookmark` carries the `add_outline_item` `bold`/`italic`/`color`/`fit`/`is_open` typography axis the OUTLINE fold reads per level plus the `fallback` `(title, page)` rows lowered when no `DocumentNode` tree is supplied; `Watermark` carries the overlay-versus-underlay placement and the optional target `Rectangle`; `Attachment` carries the embedded-file `name`/`description`/`mime` plus the `AFRelationship` `/AFRelationship` the PDF/A-3 source-file embed requires; `Imposition` folds `across`/`down`/`sheet` plus an `ImposeLayout` selecting sequential `NUP` against the saddle-stitch `BOOKLET` page order; `Viewer` carries the `PageMode`/`PageLayout` open-state plus the `hide_toolbar`/`fit_window`/`center_window`/`display_doctitle` viewer-preference axis; `Scrub` projects the full pymupdf `Document.scrub` sanitize-axis kwargs; `Label` projects the `add_redact_annot` typography axis AND the `needles` content-search terms the REDACT arm resolves to rects through `Page.search_for`; `Sanitize` selects the `pikepdf.sanitize` strip family plus annotation flatten and signature-field disable; `Optimize` selects the `Pdf.save` linearize/recompress/object-stream/deterministic-id strategy; `ContentEdit` folds the qpdf `parse_content_stream` operand+operator instructions through one immutable `Block.fold` threading the `/OC` marked-content stack — operator drop, `Name`-guarded resource rename, and OCG named-layer strip/flatten over the `BDC`/`BMC`…`EMC` span plus the `/Root/OCProperties` catalog prune, so a layered artifact's named layer is removed from BOTH its content and its catalog registration; `Confidentiality` carries the Office disposition (unlock the encrypted container OR re-seal a plaintext OOXML payload under a fresh agile container) so the PROTECT row is the full bidirectional confidentiality rail, not a decrypt-only half.

Ownership across the wire is single-lane and in-process. `pikepdf` owns qpdf-native encryption across every strength, page composition, attachments, the `parse_content_stream`/`unparse_content_stream` token model, the object model, the `sanitize` strip family, `flatten_annotations`, `acroform.disable_digital_signatures`, `remove_unreferenced_resources`, and the recompress/linearize/deterministic-id `Pdf.save` strategy; `pypdf` owns the pure-Python writer, the `Transformation` imposition algebra, `add_outline_item` outline authoring, and the `create_viewer_preferences`/`page_layout`/`page_mode` viewer-navigation surface; `pymupdf` owns the MuPDF redaction burn-in, the `Page.search_for` content match, plus the `bake`/`scrub`/`subset_fonts` sanitize sweep that destroys content bytes irreversibly; `msoffcrypto` owns the encrypted-Office container detection, the `keyTypes` credential axis, decryption, AND OOXML re-encryption.

Cross-cutting receipt emission is a definition-time aspect over the thin pure `_emit`: the runtime `@receipted(_REDACTION)` weave drains `contribute` and emits through the runtime `Signals.emit_async`, harvesting the threaded `FinishFact` evidence without an inline `emit` per arm, while the `async_boundary` capsule converts a provider raise into the runtime `BoundaryFault` rail; the owner reads as the receipt weave over a pure synchronous dispatch fold, never inline-repeated concerns. The OUTLINE step folds the `folder:document/model#NODE` `SectionNode` heading tree so the bookmark hierarchy is recovered from the one semantic tree, and the REDACT step folds the `AnnotationNode` `AnnotKind.REDACTION` rects so destruction targets the one semantic tree. One polymorphic `finish` entry owns BOTH the singular step and the finishing CHAIN: an `EgressStep | tuple[EgressStep, ...]` discriminant threads the finished bytes step-to-step through one `reduce` fold, so a watermark-then-impose-then-encrypt deliverable is one fold over the input shape, never a caller-orchestrated re-entry or a `mode`/`batch` knob. Re-signing is never an egress step and routes to `folder:../exchange/conformance#CONFORM`; PDF/A authoring routes to `folder:document/emit#DOCUMENT`; named-layer AUTHORING routes to `folder:../export/layered#LAYERED`; descriptive-metadata authoring routes to `folder:../exchange/metadata#METADATA`. Every finish returns a `RuntimeRail[ContentKey]` and, through `DocumentEgress.contribute`, builds the one `folder:../core/receipt#RECEIPT` `ArtifactReceipt` case off the `Finisher.office` discriminant then yields the `Iterable[Receipt]` stream the runtime `ReceiptContributor` port declares.

## [01]-[INDEX]


## [02]-[FINISH]

- Owner: `DocumentEgress` the one finishing-close owner discriminating the egress step; `EgressStep` the closed `StrEnum` over the eleven finishing operations; `FINISHERS` the `frozendict[EgressStep, Finisher]` policy table binding each step to one `Finisher` row carrying its single `FinishFact`-returning arm and its `office` discriminant — the table is the totality proof, one arm per step, never an `if step == ...` cascade and never a parallel native frozenset. `Finishing` the one frozen value-object bundle carrying every trusted finishing policy, mirrored by the untrusted `Extras` material struct so the admission split is trust, not concern. `FinishFact` the bytes-plus-evidence carrier every arm returns so the receipt reads the page/encryption-R/outline-depth/overlay facts the arm produced, threaded onto the owner through `structs.replace`. `Strength` the closed `StrEnum` whose `_STRENGTHS` table projects the `(r, aes)` pair in one read. `Permissions`/`Encryption`/`Bookmark`/`Watermark`/`Attachment`/`Imposition`/`Viewer`/`Scrub`/`Label`/`Sanitize`/`Optimize`/`ContentEdit`/`Confidentiality` the row-policy value objects the `Finishing` bundle carries. pikepdf owns qpdf-native encryption across every strength, overlay/underlay composition, attachment specs, the `parse_content_stream`/`unparse_content_stream` token model, the object model, and the `Pdf.save` strategy; pypdf owns `PdfWriter.add_outline_item`, the `Transformation` imposition algebra, and the `create_viewer_preferences`/`page_layout`/`page_mode` viewer surface; pymupdf owns `Page.apply_redactions`/`search_for` plus the `Document.bake`/`scrub`/`subset_fonts` sanitize sweep; msoffcrypto owns `OfficeFile` detection, the `keyTypes` credential axis, `decrypt`, and `OOXMLFile.encrypt`.
- Cases: `EgressStep` rows · `ENCRYPT` (the one pikepdf encrypt arm — `Pdf.save(encryption=pikepdf.Encryption(owner, user, R, aes, metadata, allow))` across `RC4-40`/`RC4-128`/`AES-128`/`AES-256`, the one `Strength` row projecting its `(r, aes)` pair off `_STRENGTHS`, `Encryption.metadata` deriving `aes and encrypt_metadata` so the qpdf "encrypt metadata only under AES" rule is a derived value never a runtime raise, the one `Permissions` policy projecting to `pikepdf.Permissions`) · `OUTLINE` (the bookmark-tree authoring arm folding the `SectionNode` heading hierarchy through `PdfWriter.add_outline_item` under the `Bookmark` `(bold, italic, color, fit, is_open)` axis into nested bookmarks, tracking the deepest authored `level` as the receipt depth, or lowering the `Bookmark.fallback` `(title, page)` rows when no `DocumentNode` tree is supplied) · `WATERMARK` (pikepdf opening the stamp into a named `Pdf` that outlives the loop, `contents_coalesce` forcing its `/Contents` indirect, then `Page.add_overlay`/`add_underlay` placing it at the `Watermark.rect` `Rectangle` across documents, the placed count one `sum` over the apply-and-count generator since `add_overlay` returns the placement `Name`) · `ATTACH` (`Pdf.attachments[name] = AttachedFileSpec(pdf, data, filename, description, mime_type, relationship)` embedding a file into the catalog with the `AFRelationship` `/AFRelationship` a PDF/A-3 source embed requires, the relationship a `pikepdf.Name` not a bare string) · `IMPOSE` (pypdf n-up/booklet imposition folding each source `PageObject` of the `Imposition.order(count)` sequence onto a target sheet through `Transformation` then `merge_page`, the `NUP` order `range(count)` batched by `slots` and the `BOOKLET` order the saddle-stitch quartet sequence with `-1` blanks the placer skips) · `NAVIGATE` (pypdf authoring the `Viewer` open-state — `PdfWriter.page_layout`/`page_mode` plus the `create_viewer_preferences()` `hide_toolbar`/`fit_window`/`center_window`/`display_doctitle` axis — so a navigable deliverable opens to its bookmark panel in the chosen layout, the navigation half OUTLINE's bookmark tree does not own) · `REWRITE` (pikepdf object-model surgery folding the qpdf `parse_content_stream` operand+operator instructions through one immutable `Block.fold` whose carried `(stack, kept)` threads the `/OC` `BDC`/`BMC`…`EMC` marked-content stack — operator drop by `str(op)`, `Name`-guarded resource rename, and OCG strip/flatten over the `LayerMode` per-region stack — writing `make_stream(unparse_content_stream(...))` into `page.obj[Name.Contents]` then pruning the stripped OCGs from the `/Root/OCProperties` `/OCGs`/`/D` catalog, reporting the catalog-removed layer count) · `REDACT` (pymupdf staging each `AnnotKind.REDACTION` tree rect AND each `Label.needles` `Page.search_for` match as a black `add_redact_annot` carrying the optional `Label.overlay_text` under the `Label` typography axis, burning through `apply_redactions` with the `PDF_REDACT_*` flag triple, flattening through `bake`, sweeping through `scrub(**Scrub.kwargs())`, subsetting through `subset_fonts(fallback=False)`, destroying the underlying bytes irreversibly) · `SANITIZE` (pikepdf folding `Sanitize` over the `pikepdf.sanitize` strip family plus `flatten_annotations` and `acroform.disable_digital_signatures`) · `OPTIMIZE` (pikepdf folding `Optimize` over `remove_unreferenced_resources` plus the `Pdf.save` linearize/recompress/object-stream/deterministic-id strategy) · `PROTECT` (the Office confidentiality arm folding `Confidentiality` over `OfficeFile.is_encrypted` gating then EITHER the format-discriminated `load_key(**credentials, **verify)`/`decrypt(outfile, **verify)` unlock — the OOXML `keyTypes` axis or the legacy-97 `("password",)` axis recovered through the `office.format == "ooxml"` factory-set discriminant — OR the `OOXMLFile.encrypt(password, outfile)` re-seal of a plaintext OOXML payload, both arms idempotent at the `is_encrypted()` gate so an already-plaintext unlock and an already-sealed re-seal each return the source unchanged) — each one `Finisher.arm` resolved off `FINISHERS`, never re-enumerated by a worker-side `match`.
- Auto: `_stepped` runs `FINISHERS[step].arm(staged)` synchronously in-process, then threads the returned `FinishFact` onto a successor owner through `structs.replace(self, fact=..., source=fact.data)` — the frozen seed is never mutated, the successor carries both the evidence and the reseeded bytes. `finished` is one `reduce` over `self.steps` folding `_stepped` to the final owner, and `_emit` returns that finished owner (a `ReceiptContributor`) the `@receipted` weave drains — `finish` mints the content key over its `source` (the last finished bytes) through `ContentIdentity.of`; `contribute` reads the final owner's `fact`. The ENCRYPT arm reads the one `Strength` row into `pikepdf.Encryption`, the dual RC4/AES strengths sharing one leg because qpdf authors them all in-process — `pypdf` is reserved for the structural OUTLINE/IMPOSE/NAVIGATE arms it owns, never a parallel encryptor. Every arm's native package is a module-scope `lazy import`/`lazy from` binding reified on first arm use; no native import lands on the core owner.
- Receipt: each finish contributes one `folder:../core/receipt#RECEIPT` case. The runtime `@receipted(_REDACTION)` weave wraps `_emit` and drains `DocumentEgress.contribute` off the stepped owner `_emit` returns, emitting through `Signals.emit_async`; `contribute` reads the threaded `FinishFact` off `self.fact` (never a re-run of an arm), re-mints the content key over `fact.data`, and folds the case off `Finisher.office` in one expression — the PDF-finishing rows emit `ArtifactReceipt.Egress` carrying the content key, the post-finish byte count, and the `fact.pages`/`encryption_r`/`outline_depth`/`overlays` evidence the arm produced (each defaulting to `0` off the rows that do not produce it; the REWRITE `fact.layers_removed` OCG-strip count rides the `overlays` layer-composition slot since an OCG strip is the inverse layer operation the watermark overlay count measures), and the `PROTECT` step emits `ArtifactReceipt.Office` carrying the content key and the decrypted-or-resealed byte count — one shared receipt stream off the one `FinishFact` shape, every row contributing through the SAME projection rather than a per-row receipt delegate. The case's own `contribute` projects through the runtime `Receipt.of("artifacts", ("emitted", subject, facts))` contract, so the egress owner declares the canonical streaming port like every sibling consumer and carries no second port method.
- Growth: a new finishing step is one `EgressStep` row plus one `Finisher` row carrying its arm and office discriminant, plus one `_PREREQ` row when the step needs material; a new finishing-policy concern is one `Finishing` field carrying its own value object; a new receipt fact is one `FinishFact` field the arm populates, never a re-derivation off the bytes; a new permission knob is one `Permissions` field; an encryption strength is one `Strength` row plus one `_STRENGTHS` cell carrying its `(r, aes)` pair, never a parallel encrypt owner; a sanitize knob is one `Scrub` field; a bookmark-typography knob is one `Bookmark` field; a redaction-label or content-search knob is one `Label` field; an imposition layout is one `ImposeLayout` member plus one `Imposition.order` arm; a viewer-state knob is one `Viewer` field; an attachment-relationship kind is one `AFRelationship` member; a credential kind is already carried by the format-discriminated credential axis; an Office disposition (unlock vs re-seal) is one `Confidentiality` case; a content-stream edit is one `ContentEdit` field folded through `_folded_stream`'s `Block.fold`; a deeper finishing chain is one more `EgressStep` in the sequence the rail already folds; zero new surface.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable, Iterator
from enum import StrEnum
from functools import reduce
from io import BytesIO
from itertools import batched
from typing import TYPE_CHECKING, Final, Literal, NotRequired, ReadOnly, Self, TypedDict, Unpack, assert_never

from anyio import CapacityLimiter, to_thread
from builtins import frozendict
from expression import Error, Ok, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, field, structs
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.receipts import OPEN, Receipt, receipted

from artifacts.core.receipt import ArtifactReceipt
from artifacts.document.model import AnnotKind, AnnotationNode, DocumentNode, SectionNode, walk

lazy import msoffcrypto
lazy import pikepdf
lazy import pymupdf
lazy from pikepdf import sanitize
lazy from pypdf import PdfReader, PdfWriter, Transformation
lazy from pypdf.generic import Fit

if TYPE_CHECKING:
    import pikepdf
    from pypdf import PageObject
    from pypdf.generic import IndirectObject


# --- [TYPES] ----------------------------------------------------------------------------
class EgressStep(StrEnum):
    ENCRYPT = "encrypt"
    OUTLINE = "outline"
    WATERMARK = "watermark"
    ATTACH = "attach"
    IMPOSE = "impose"
    NAVIGATE = "navigate"
    REWRITE = "rewrite"
    REDACT = "redact"
    SANITIZE = "sanitize"
    OPTIMIZE = "optimize"
    PROTECT = "protect"


class Strength(StrEnum):
    RC4_40 = "RC4-40"
    RC4_128 = "RC4-128"
    AES_128 = "AES-128"
    AES_256 = "AES-256"


class ImposeLayout(StrEnum):
    NUP = "nup"
    BOOKLET = "booklet"


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


class PageMode(StrEnum):  # the `pypdf.PdfWriter.page_mode` `/PageMode` open-panel state the NAVIGATE close authors
    NONE = "/UseNone"
    OUTLINES = "/UseOutlines"
    THUMBS = "/UseThumbs"
    FULLSCREEN = "/FullScreen"
    OPTIONAL_CONTENT = "/UseOC"
    ATTACHMENTS = "/UseAttachments"


class PageLayout(StrEnum):  # the `pypdf.PdfWriter.set_page_layout` `/PageLayout` spread the NAVIGATE close authors
    NONE = "/NoLayout"
    SINGLE = "/SinglePage"
    ONE_COLUMN = "/OneColumn"
    TWO_LEFT = "/TwoColumnLeft"
    TWO_RIGHT = "/TwoColumnRight"
    TWO_PAGE_LEFT = "/TwoPageLeft"
    TWO_PAGE_RIGHT = "/TwoPageRight"


class AFRelationship(StrEnum):  # the `/AFRelationship` an embedded source file declares for PDF/A-3
    SOURCE = "/Source"
    DATA = "/Data"
    ALTERNATIVE = "/Alternative"
    SUPPLEMENT = "/Supplement"
    UNSPECIFIED = "/Unspecified"


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class EgressFault:
    # the closed ADMISSION vocabulary `of` produces; the arm-level provider raises
    # (`pikepdf.PdfError`, `pymupdf.FileDataError`, `msoffcrypto.DecryptionError`) convert to the
    # runtime `BoundaryFault` at the `async_boundary` capsule, never into this interior vocabulary.
    tag: Literal["payload", "empty", "incomplete"] = tag()
    payload: tuple[str, ...] = case()        # the rejected payload key paths
    empty: None = case()                     # an empty finishing chain
    incomplete: EgressStep = case()          # a step admitted without its required material/policy


# --- [CONSTANTS] ------------------------------------------------------------------------
# the keep-all redaction the `@receipted` weave rides; `Redaction` is a `Struct(classified, salt)` with
# NO named instances, so `Redaction.STRUCTURAL` was a phantom — the empty `classified` map scrubs nothing.
_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())
_OFFLOAD: Final = CapacityLimiter(8)  # the finishing fold is GIL-releasing native (pikepdf/pymupdf/msoffcrypto); never inline on the loop
_STRENGTHS: Final[frozendict[Strength, tuple[int, bool]]] = frozendict({
    Strength.RC4_40: (2, False),
    Strength.RC4_128: (4, False),
    Strength.AES_128: (4, True),
    Strength.AES_256: (6, True),
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


class Encryption(Struct, frozen=True):
    owner: str
    user: str = ""
    strength: Strength = Strength.AES_256
    encrypt_metadata: bool = True

    @property
    def r(self) -> int:
        return _STRENGTHS[self.strength][0]

    @property
    def aes(self) -> bool:
        return _STRENGTHS[self.strength][1]

    @property
    def metadata(self) -> bool:
        return self.aes and self.encrypt_metadata  # qpdf rejects metadata encryption below AES; the RC4 cell is plaintext


class Bookmark(Struct, frozen=True):
    bold_top: bool = True
    italic: bool = False
    color: tuple[float, float, float] | None = None
    open_depth: int = 1
    fit: FitMode = FitMode.FIT
    fallback: tuple[tuple[str, int], ...] = ()  # flat `(title, page)` outline lowered when no `DocumentNode` tree is supplied

    def style(self, level: int, /) -> dict[str, object]:
        return {"bold": level == 1 and self.bold_top, "italic": self.italic, "color": self.color, "fit": getattr(Fit, _FITS[self.fit])(), "is_open": level < self.open_depth}


class Watermark(Struct, frozen=True):
    under: bool = False
    rect: tuple[float, float, float, float] | None = None  # the target placement box; None stamps the whole page


class Attachment(Struct, frozen=True):
    name: str = ""
    description: str = ""
    mime: str = ""
    relationship: AFRelationship = AFRelationship.UNSPECIFIED


class Imposition(Struct, frozen=True):
    across: int = 2
    down: int = 1
    sheet: tuple[float, float] = (612.0, 792.0)
    layout: ImposeLayout = ImposeLayout.NUP

    @property
    def slots(self) -> int:
        return self.across * self.down

    def order(self, count: int, /) -> tuple[int, ...]:
        # the source-page index sequence per sheet cell: `NUP` is sequential, `BOOKLET` is the
        # saddle-stitch 2-up fold padded to a multiple of four with `-1` blanks the placer skips.
        if self.layout is ImposeLayout.NUP:
            return tuple(range(count))
        padded = -(-count // 4) * 4
        folded = (page for i in range(padded // 4) for page in (padded - 1 - 2 * i, 2 * i, 2 * i + 1, padded - 2 - 2 * i))
        return tuple(page if page < count else -1 for page in folded)


class Viewer(Struct, frozen=True):
    page_mode: PageMode = PageMode.NONE
    page_layout: PageLayout = PageLayout.NONE
    hide_toolbar: bool = False
    fit_window: bool = False
    center_window: bool = False
    display_doctitle: bool = True


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
    overlay_text: str = ""                                   # the label burned into every redacted region
    flatten_widgets: bool = True                             # bake form widgets during the redaction `bake`
    needles: tuple[str, ...] = ()                            # content-search terms resolved to rects through `Page.search_for`

    def annot(self) -> dict[str, object]:
        # the full `add_redact_annot` typography axis; a non-empty `overlay_text` suppresses the strike-out
        overlay = self.overlay_text or None
        return {"fontname": self.fontname, "fontsize": self.fontsize, "align": self.align, "fill": self.fill, "text_color": self.text_color, "cross_out": self.cross_out and overlay is None, "text": overlay}


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


class Finishing(Struct, frozen=True):
    # the one TRUSTED finishing-policy bundle the caller constructs; each field a behavior-carrying
    # value object the arms read as `egress.finishing.<policy>`, the admission counterpart of the
    # untrusted `Extras` material — the split is trust, not concern, so passwords and strip sets
    # never ride the untrusted payload while stamp/attachment bytes never ride a trusted default.
    permissions: Permissions = Permissions()
    encryption: Encryption | None = None
    bookmark: Bookmark = Bookmark()
    watermark: Watermark = Watermark()
    attachment: Attachment = Attachment()
    imposition: Imposition = Imposition()
    viewer: Viewer = Viewer()
    scrub: Scrub = Scrub()
    label: Label = Label()
    sanitize: Sanitize = Sanitize()
    optimize: Optimize = Optimize()
    edit: ContentEdit = ContentEdit()
    confidentiality: Confidentiality = field(default_factory=lambda: Confidentiality(unlock=True))


class Extras(Struct, frozen=True, omit_defaults=True):
    stamp: bytes = b""                                            # the WATERMARK stamp PDF bytes
    attachment_data: bytes = b""                                 # the ATTACH embedded-file payload
    credentials: frozendict[str, str] = field(default_factory=frozendict)  # the PROTECT `keyTypes` credential axis


# --- [BOUNDARIES] -----------------------------------------------------------------------
class EgressPayload(TypedDict, extra_items=str):
    stamp: NotRequired[ReadOnly[bytes]]
    attachment_data: NotRequired[ReadOnly[bytes]]


_PAYLOAD: Final = TypeAdapter(EgressPayload)
_DECLARED: Final[frozenset[str]] = EgressPayload.__optional_keys__ | EgressPayload.__required_keys__


# --- [SERVICES] -------------------------------------------------------------------------
class Finisher(Struct, frozen=True):
    arm: Callable[["DocumentEgress"], FinishFact]
    office: bool = False


class DocumentEgress(Struct, frozen=True):
    step: EgressStep | tuple[EgressStep, ...]
    source: bytes
    node: DocumentNode | None = None
    finishing: Finishing = field(default_factory=Finishing)
    extras: Extras = field(default_factory=Extras)
    fact: FinishFact | None = None

    @property
    def steps(self) -> tuple[EgressStep, ...]:
        return self.step if isinstance(self.step, tuple) else (self.step,)

    def _stepped(self, step: EgressStep, /) -> Self:
        staged = structs.replace(self, step=step)
        fact = FINISHERS[step].arm(staged)
        return structs.replace(staged, fact=fact, source=fact.data)

    def finished(self) -> Self:
        # the immutable fold: each step threads the prior finished bytes forward and the returned
        # owner carries the final `FinishFact`, so the frozen owner is never mutated in place.
        return reduce(lambda live, step: live._stepped(step), self.steps, self)

    @receipted(_REDACTION)  # the harvest weave drains `contribute` off the returned stepped owner and emits via `Signals.emit_async`
    async def _emit(self) -> Self:
        # the finishing fold is heavy GIL-releasing native work, so it crosses the thread seam under the
        # shared limiter rather than stalling the loop; the finished owner is the `ReceiptContributor` the weave drains.
        return await to_thread.run_sync(self.finished, limiter=_OFFLOAD)

    async def finish(self) -> RuntimeRail[ContentKey]:
        # mint the content key off the finished owner the boundary returns; `_stepped` reseeds `source` to the final finished bytes
        return (await async_boundary(f"egress.{'+'.join(self.steps)}", self._emit)).map(
            lambda done: ContentIdentity.of(f"egress-{done.steps[-1]}", done.source)
        )

    def contribute(self) -> Iterable[Receipt]:
        # the canonical `ReceiptContributor.contribute(self)` port — phase is the constant `"emitted"`
        # the `ArtifactReceipt` already fixes by construction (KNOB_TEST), never a parameter here.
        if (fact := self.fact) is None:  # contribute rides the finished owner the fold returned, never the pre-fold seed
            return
        key = ContentIdentity.of(f"egress-{self.steps[-1]}", fact.data)
        case = (
            ArtifactReceipt.Office(key, len(fact.data))
            if FINISHERS[self.steps[-1]].office
            else ArtifactReceipt.Egress(key, len(fact.data), fact.pages, fact.encryption_r, fact.outline_depth, fact.overlays + fact.layers_removed)
        )
        yield from case.contribute()

    @classmethod
    def of(
        cls,
        step: EgressStep | tuple[EgressStep, ...],
        source: bytes,
        /,
        *,
        finishing: Finishing = Finishing(),
        node: DocumentNode | None = None,
        **raw: Unpack[EgressPayload],
    ) -> Result[Self, EgressFault]:
        if isinstance(step, tuple) and not step:
            return Error(EgressFault(empty=None))
        try:
            payload = _PAYLOAD.validate_python(raw)
        except ValidationError as fault:
            return Error(EgressFault(payload=tuple(str(error["loc"]) for error in fault.errors())))
        credentials = frozendict({name: value for name, value in payload.items() if name not in _DECLARED})
        known = {name: value for name, value in payload.items() if name in _DECLARED}
        candidate = cls(step=step, source=source, node=node, finishing=finishing, extras=Extras(credentials=credentials, **known))
        missing = next((member for member in candidate.steps if member in _PREREQ and not _PREREQ[member](candidate)), None)
        return Error(EgressFault(incomplete=missing)) if missing is not None else Ok(candidate)


# --- [OPERATIONS] -----------------------------------------------------------------------
def _sections(node: DocumentNode | None, /) -> Iterator[SectionNode]:
    return (n for n in walk(node) if isinstance(n, SectionNode)) if node is not None else iter(())


def _redaction_rects(node: DocumentNode | None, /) -> Map[int, tuple[tuple[float, float, float, float], ...]]:
    annots = (n for n in (walk(node) if node is not None else ()) if isinstance(n, AnnotationNode) and n.annot is AnnotKind.REDACTION)
    return Block.of_seq(annots).fold(
        lambda acc, a: acc.change(a.meta.page, lambda cur: Some((*cur.default_value(()), a.target))), Map.empty()
    )


def _encrypt(egress: DocumentEgress) -> FinishFact:
    enc, sink = egress.finishing.encryption, BytesIO()
    with pikepdf.open(BytesIO(egress.source)) as pdf:  # deterministic close, never GC-reaped
        pdf.save(sink, linearize=True, encryption=pikepdf.Encryption(owner=enc.owner, user=enc.user, R=enc.r, aes=enc.aes, metadata=enc.metadata, allow=egress.finishing.permissions.to_pikepdf()))
        return FinishFact(sink.getvalue(), pages=len(pdf.pages), encryption_r=enc.r)


type _Outline = tuple[frozendict[int, "IndirectObject"], int]


def _outline(egress: DocumentEgress) -> FinishFact:
    bookmark = egress.finishing.bookmark
    writer = PdfWriter(clone_from=PdfReader(BytesIO(egress.source)))

    def author(state: _Outline, section: SectionNode, /) -> _Outline:  # the boundary `add_outline_item` is the seam; the parent/depth thread is pure
        parents, depth = state
        node = writer.add_outline_item("".join(run.text for run in section.heading), section.meta.page, parent=parents.get(section.level - 1), **bookmark.style(section.level))
        return parents | {section.level: node}, max(depth, section.level)
    _parents, depth = Block.of_seq(_sections(egress.node)).fold(author, (frozendict(), 0))
    if egress.node is None:
        for title, page in bookmark.fallback:
            writer.add_outline_item(title, page)
    sink = BytesIO()
    writer.write(sink)
    return FinishFact(sink.getvalue(), pages=len(writer.pages), outline_depth=depth)


def _watermark(egress: DocumentEgress) -> FinishFact:
    wm = egress.finishing.watermark
    with pikepdf.open(BytesIO(egress.source)) as pdf, pikepdf.open(BytesIO(egress.extras.stamp)) as stamp:  # both closed deterministically
        mark = pikepdf.Page(stamp.pages[0])
        mark.contents_coalesce()
        rect = pikepdf.Rectangle(*wm.rect) if wm.rect is not None else None
        place = pikepdf.Page.add_underlay if wm.under else pikepdf.Page.add_overlay
        overlays = sum(place(pikepdf.Page(page), mark, rect) is not None for page in pdf.pages)  # add_overlay returns the placement `Name`
        sink = BytesIO()
        pdf.save(sink, linearize=True)
        return FinishFact(sink.getvalue(), pages=len(pdf.pages), overlays=overlays)


def _attach(egress: DocumentEgress) -> FinishFact:
    att = egress.finishing.attachment
    with pikepdf.open(BytesIO(egress.source)) as pdf:  # deterministic close, never GC-reaped
        pdf.attachments[att.name] = pikepdf.AttachedFileSpec(
            pdf, egress.extras.attachment_data, filename=att.name, description=att.description, mime_type=att.mime, relationship=pikepdf.Name(att.relationship.value)
        )
        sink = BytesIO()
        pdf.save(sink, linearize=True)
        return FinishFact(sink.getvalue(), pages=len(pdf.pages))


def _impose(egress: DocumentEgress) -> FinishFact:
    imp = egress.finishing.imposition
    reader = PdfReader(BytesIO(egress.source))
    width, height = imp.sheet
    cell_w, cell_h = width / imp.across, height / imp.down
    writer = PdfWriter()

    def placed(sheet: "PageObject", indexed: tuple[int, int], /) -> "PageObject":  # `add_transformation`/`merge_page` mutate in place at the pypdf seam
        offset, index = indexed
        if index < 0:  # a booklet pad slot keeps the cell blank
            return sheet
        row, col = divmod(offset, imp.across)
        source = reader.pages[index]
        source.add_transformation(Transformation().scale(cell_w / source.mediabox.width, cell_h / source.mediabox.height).translate(col * cell_w, (imp.down - 1 - row) * cell_h))
        sheet.merge_page(source)
        return sheet

    def imposed(window: tuple[int, ...], /) -> object:
        return Block.of_seq(enumerate(window)).fold(placed, writer.add_blank_page(width=width, height=height))
    for window in batched(imp.order(len(reader.pages)), imp.slots):
        imposed(window)
    sink = BytesIO()
    writer.write(sink)
    return FinishFact(sink.getvalue(), pages=len(writer.pages))


def _navigate(egress: DocumentEgress) -> FinishFact:
    view = egress.finishing.viewer
    writer = PdfWriter(clone_from=PdfReader(BytesIO(egress.source)))
    writer.page_layout = view.page_layout.value
    writer.page_mode = view.page_mode.value
    prefs = writer.create_viewer_preferences()  # binds a `/ViewerPreferences` dict to the catalog; attribute sets persist
    prefs.hide_toolbar, prefs.fit_window, prefs.center_window, prefs.display_doctitle = view.hide_toolbar, view.fit_window, view.center_window, view.display_doctitle
    sink = BytesIO()
    writer.write(sink)
    return FinishFact(sink.getvalue(), pages=len(writer.pages))


type _Instr = tuple[list[object], object]
type _Fold = tuple[tuple[LayerMode, ...], Block[_Instr]]


def _folded_stream(page: "pikepdf.Page", edit: ContentEdit) -> bytes:
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
            name = _oc_name(page, operands) if operands and str(operands[0]) == "/OC" else ""
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


def _oc_name(page: "pikepdf.Page", operands: list[object]) -> str:
    marker = operands[1] if len(operands) > 1 else None
    if isinstance(marker, pikepdf.Dictionary):
        return str(marker.get(pikepdf.Name.Name, ""))
    properties = page.obj.get(pikepdf.Name.Resources, pikepdf.Dictionary()).get(pikepdf.Name.Properties, pikepdf.Dictionary())
    ocg = properties.get(pikepdf.Name(str(marker)), pikepdf.Dictionary()) if marker is not None else pikepdf.Dictionary()
    target = ocg.get(pikepdf.Name.OCGs, ocg) if str(ocg.get(pikepdf.Name.Type, "")) == "/OCMD" else ocg
    return str(target.get(pikepdf.Name.Name, ""))


def _strip_ocg_catalog(pdf: "pikepdf.Pdf", removed: frozenset[str]) -> int:
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
    edit = egress.finishing.edit
    with pikepdf.open(BytesIO(egress.source)) as pdf:  # deterministic close, never GC-reaped
        for page in pdf.pages:
            page.obj[pikepdf.Name.Contents] = pdf.make_stream(_folded_stream(page, edit))
        layers = _strip_ocg_catalog(pdf, edit.strip_layers) if edit.touches_layers else 0
        sink = BytesIO()
        pdf.save(sink, linearize=True)
        return FinishFact(sink.getvalue(), pages=len(pdf.pages), layers_removed=layers)


def _redact(egress: DocumentEgress) -> FinishFact:
    label, scrub = egress.finishing.label, egress.finishing.scrub
    with pymupdf.open(stream=egress.source, filetype="pdf") as doc:  # deterministic close, never GC-reaped
        marks, tree_rects = label.annot(), _redaction_rects(egress.node)
        for index in range(doc.page_count):
            page = doc[index]
            hits = [*(pymupdf.Rect(rect) for rect in tree_rects.try_find(index).default_value(())), *(rect for needle in label.needles for rect in page.search_for(needle))]
            for rect in hits:
                page.add_redact_annot(rect, **marks)
            if hits:
                page.apply_redactions(images=pymupdf.PDF_REDACT_IMAGE_REMOVE, graphics=pymupdf.PDF_REDACT_LINE_ART_REMOVE_IF_TOUCHED, text=pymupdf.PDF_REDACT_TEXT_REMOVE)
        doc.bake(annots=True, widgets=label.flatten_widgets)
        doc.scrub(**scrub.kwargs())
        doc.subset_fonts(fallback=False)
        sink = BytesIO()
        doc.save(sink, garbage=4, deflate=True, clean=True)
        return FinishFact(sink.getvalue(), pages=doc.page_count)


def _sanitize(egress: DocumentEgress) -> FinishFact:
    pol = egress.finishing.sanitize
    with pikepdf.open(BytesIO(egress.source)) as pdf:  # deterministic close, never GC-reaped
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
    pol = egress.finishing.optimize
    with pikepdf.open(BytesIO(egress.source)) as pdf:  # deterministic close, never GC-reaped
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
    office = msoffcrypto.OfficeFile(BytesIO(egress.source))
    sink = BytesIO()
    match egress.finishing.confidentiality:
        case Confidentiality(tag="reseal", reseal=password):  # the inverse rail — only the OOXML object carries `encrypt`
            if office.is_encrypted():  # an already-sealed container is the idempotent terminal, never a double-seal
                return FinishFact(egress.source)
            office.encrypt(password, sink)
        case Confidentiality(tag="unlock", unlock=verify):
            if not office.is_encrypted():  # an already-plaintext container is the idempotent unlock terminal
                return FinishFact(egress.source)
            ooxml = getattr(office, "format", "") == "ooxml"  # `format` is the factory-set discriminant; `keyTypes` is absent on 97/extensible/plain
            key_axis = getattr(office, "keyTypes", ("password",))
            credentials = {kind: egress.extras.credentials[kind] for kind in key_axis if kind in egress.extras.credentials}
            office.load_key(**credentials, **({"verify_password": True} if ooxml and verify else {}))
            office.decrypt(sink, **({"verify_integrity": verify} if ooxml else {}))
        case _ as unreachable:
            assert_never(unreachable)
    return FinishFact(sink.getvalue())


# --- [COMPOSITION] ----------------------------------------------------------------------
FINISHERS: Final[frozendict[EgressStep, Finisher]] = frozendict({
    EgressStep.ENCRYPT: Finisher(_encrypt),
    EgressStep.OUTLINE: Finisher(_outline),
    EgressStep.WATERMARK: Finisher(_watermark),
    EgressStep.ATTACH: Finisher(_attach),
    EgressStep.IMPOSE: Finisher(_impose),
    EgressStep.NAVIGATE: Finisher(_navigate),
    EgressStep.REWRITE: Finisher(_rewrite),
    EgressStep.REDACT: Finisher(_redact),
    EgressStep.SANITIZE: Finisher(_sanitize),
    EgressStep.OPTIMIZE: Finisher(_optimize),
    EgressStep.PROTECT: Finisher(_protect, office=True),
})
# the per-step admission prerequisite: `of` rejects a step whose required owner is absent into
# `EgressFault.incomplete` so the in-process fold is total — steps omitted here are always ready.
_PREREQ: Final[frozendict[EgressStep, Callable[[DocumentEgress], bool]]] = frozendict({
    EgressStep.ENCRYPT: lambda eg: eg.finishing.encryption is not None,
    EgressStep.WATERMARK: lambda eg: bool(eg.extras.stamp),
    EgressStep.ATTACH: lambda eg: bool(eg.extras.attachment_data and eg.finishing.attachment.name),
    EgressStep.OUTLINE: lambda eg: eg.node is not None or bool(eg.finishing.bookmark.fallback),
    EgressStep.REDACT: lambda eg: eg.node is not None or bool(eg.finishing.label.needles),
})
```
