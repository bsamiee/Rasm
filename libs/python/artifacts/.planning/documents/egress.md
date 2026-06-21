# [PY_ARTIFACTS_EGRESS]

The security-and-navigation finishing close over an emitted PDF or Office container. `DocumentEgress` is ONE owner that takes a payload already authored by `folder:documents/emit#DOCUMENT` and returns a sealed, navigable, watermarked, attachment-bearing, content-rewritten, redaction-burned, imposed, or Office-decrypted artifact keyed by the runtime content key — it finishes an emitted artifact and never authors one. `EgressStep` is the closed `StrEnum` over the eight finishing operations (encryption, outline-tree authoring, overlay/underlay watermarking, embedded-attachment binding, n-up/booklet imposition, qpdf content-stream rewriting, MuPDF redaction burn-in, Office confidentiality), each a `Finisher` policy-table row binding its single finishing arm and its runtime band in one value. Every arm returns a `FinishFact` carrying the finished bytes beside the produced evidence (page count, applied encryption R, authored outline depth, applied overlay count), so the receipt reads the arm's own output and the finished bytes are never re-parsed by a second pypdf pass. `Permissions` is the row-policy value object the encryption step carries instead of scattered boolean knobs, projecting once to BOTH the qpdf `pikepdf.Permissions` flags and the pypdf permission-flag bitmask; `Encryption` is the row-policy value object whose `Strength` discriminant — one row in the `_STRENGTHS` cross-product table — projects in one read to the `(r, band, use_128bit)` triple, routing the encrypt close across the core-process pypdf leg (`RC4-40`/`RC4-128`/`AES-128` in-process) and the worker pikepdf leg (`AES-256`/R6) inside the one `_encrypt` arm; `Scrub` is the row-policy value object the redaction step carries projecting once to the full pymupdf `Document.scrub` sanitize-axis kwarg set; `Label` is the row-policy value object the redaction overlay carries projecting once to the `add_redact_annot` `(fontname, fontsize, align)` typography axis; `ContentEdit` is the row-policy value object the rewrite step carries folding the qpdf token stream. pikepdf owns qpdf-native encryption, page composition, attachments, content-stream tokenization, the object model, and the declarative qpdf job pipeline; pypdf owns the pure-Python writer, the `Transformation` imposition algebra, in-process RC4/AES-128 encryption, and outline authoring; pymupdf owns the MuPDF redaction burn-in plus the `bake`/`scrub`/`subset_fonts` sanitize sweep that destroys content bytes irreversibly; msoffcrypto owns the encrypted-Office container detection, the `keyTypes`-advertised credential axis, and decryption. Because pikepdf is the qpdf-native wheel reflecting on the gated `python_version<'3.15'` band, the `WATERMARK`/`ATTACH`/`REWRITE` arms and the `AES-256` `ENCRYPT` leg cross the runtime subprocess seam exactly as `folder:documents/emit#DOCUMENT` and `folder:../figures/preview#PREVIEW` do, while pymupdf, pypdf, and msoffcrypto resolve on the cp315 core; the `DocumentEgress.band` property reads the `Finisher.band` table default plus the `Encryption.band` ENCRYPT override to select the lane, never a hardcoded native set or a per-arm ternary. The OUTLINE step folds the `folder:documents/model#NODE` `DocumentNode` heading tree so the bookmark hierarchy is recovered from the one semantic tree, and the REDACT step folds the `DocumentNode` `AnnotationNode` `AnnotKind.REDACTION` rects so destruction targets the one semantic tree. Re-signing is never an egress step and routes to `folder:../typography/conformance#CONFORM`; PDF/A authoring is never an egress step and routes to `folder:documents/emit#DOCUMENT`. Every finish returns a `RuntimeRail[ContentKey]` and, through `DocumentEgress.contribute`, builds the one `folder:../receipt/receipt#RECEIPT` `ArtifactReceipt` case off the `FinishFact` ternary then yields the `Iterable[Receipt]` stream the runtime `ReceiptContributor` port declares by delegating to that case's own `contribute`, so the egress owner exposes the canonical streaming port like every sibling consumer.

## [01]-[INDEX]

- [01]-[FINISH]: `DocumentEgress` + `EgressStep` (ENCRYPT/OUTLINE/WATERMARK/ATTACH/IMPOSE/REWRITE/REDACT/PROTECT) policy-table dispatch over pikepdf/pypdf/pymupdf/msoffcrypto; the `Finisher` row binds each step to its `(band, arm)` pair and `_worker_finish` re-resolves the SAME row, collapsing the native frozenset, the `_on_worker` sentinel, and the dual worker `match` cascade into ONE arm per step; every arm returns a `FinishFact` carrying the finished bytes beside the produced page/encryption-R/outline-depth/overlay evidence, so `DocumentEgress.contribute` reads the arm's own fact and folds BOTH the PDF `ArtifactReceipt.Egress` and the Office `ArtifactReceipt.Office` case off `DocumentEgress.step` in one ternary — no second pypdf re-parse of the finished bytes, no `_pages_in` re-reader, no `_egress_receipt` projector re-deriving facts the arm already produced, the `contributes` column collapsed off the `Finisher` row because every row contributes through the SAME `FinishFact` shape, never a per-row receipt delegate and never two parallel receipt functions; `Permissions`/`Encryption`/`Scrub`/`Label`/`ContentEdit` the row-policy value objects the ENCRYPT/REDACT/REWRITE steps carry; the ENCRYPT step reads the one `Strength` row off the `_STRENGTHS` cross-product table into the `(r, band, use_128bit)` triple, projecting to BOTH the pypdf `(use_128bit, algorithm)` pair (RC4/AES-128 core) and the pikepdf `R` level (AES-256 worker); the OUTLINE step folds the `DocumentNode` heading tree; the REDACT step burns the `AnnotKind.REDACTION` rects under the `Label` typography axis, flattens annotations through `bake`, sweeps residual content through the `Scrub` sanitize axis, and subsets fonts through `subset_fonts`; the REWRITE step folds the qpdf object model through the `ContentEdit` policy (operator drop plus `Name`-guarded resource rename written back through `make_stream`); the IMPOSE step the `pypdf.Transformation` n-up algebra; and the PROTECT step folds the format-discriminated credential axis (OOXML `keyTypes` or legacy-97 `password`) and the OOXML-only verify knobs into one `load_key`/`decrypt` pair.

## [02]-[FINISH]

- Owner: `DocumentEgress` the one finishing-close owner discriminating the egress step; `EgressStep` the closed `StrEnum` over encryption/outline/watermark/attach/impose/rewrite/redact/protect; `FINISHERS` the `dict[EgressStep, Finisher]` policy table binding each step to one `Finisher` row carrying its runtime `Band` and its single `FinishFact`-returning arm over the already-emitted payload — the table is the totality proof, one arm per step and `_worker_finish` re-resolving the SAME row off the table, never an `if step == ...` cascade, never a parallel `NATIVE` frozenset, and never a second `match` enumerating the steps a second time inside the worker; `FinishFact` the bytes-plus-produced-evidence carrier every arm returns so the receipt reads the page/encryption-R/outline-depth/overlay facts the arm produced, never re-parsed off the finished bytes; `Strength` the closed `StrEnum` whose `_STRENGTHS` cross-product table projects the `(r, band, use_128bit)` triple in one read; `Permissions`/`Encryption`/`Scrub`/`Label`/`ContentEdit` the row-policy value objects the ENCRYPT/REDACT/REWRITE steps carry. pikepdf owns qpdf-native AES encryption, page overlay/underlay composition, attachment specs, content-stream tokenization, the object model, and the deferred `Job.run` declarative pipeline; pypdf owns the pure-Python `PdfWriter` outline authoring, the `Transformation` imposition algebra, and in-process `PdfWriter.encrypt` for `RC4-40`/`RC4-128`/`AES-128`; pymupdf owns the MuPDF `Page.apply_redactions` burn-in plus the `Document.bake`/`scrub`/`subset_fonts` sanitize sweep; msoffcrypto owns `OfficeFile` detection, the `keyTypes` credential axis, and `decrypt`; the weasyprint `Document.make_bookmark_tree` outline lowering is the deferred HTML-authored alternate, never a settled arm.
- Cases: `EgressStep` rows `ENCRYPT` (the one band-internal encrypt arm — `Encryption.band` selects the core pypdf `PdfWriter.encrypt(user_password, owner_password, use_128bit, algorithm, permissions_flag)` `RC4-40`/`RC4-128`/`AES-128` leg or the worker pikepdf `Pdf.save(encryption=Encryption(owner, user, R, aes=True, allow=Permissions(...)))` `AES-256`/R6 leg inside the single `_encrypt` body, the one `Strength` row projecting its `(r, band, use_128bit)` triple off `_STRENGTHS` into both the pypdf `(use_128bit, algorithm)` pair and the pikepdf `R` level, the one `Permissions` policy projecting to both flag sets) · `OUTLINE` (the bookmark-tree authoring arm folding the `SectionNode` heading hierarchy through pypdf `PdfWriter.add_outline_item` into nested bookmarks, tracking the deepest authored `level` as the receipt depth, or lowering the `params["bookmarks"]` fallback when no `DocumentNode` tree is supplied) · `WATERMARK` (pikepdf opening the stamp into a named `Pdf` that outlives the loop, `contents_coalesce` forcing its `/Contents` indirect, then `Page.add_overlay`/`add_underlay` copying it across documents summing the placed-overlay count through one `sum` over the apply-and-count generator — `as_form_xobject` is internal to `add_overlay`, never pre-called) · `ATTACH` (pikepdf `Pdf.attachments[name] = AttachedFileSpec(pdf, data, filename=name, description=...)` embedding a file into the document catalog) · `IMPOSE` (pypdf n-up/booklet imposition folding each source `PageObject` onto a target sheet through `Transformation` then `merge_page` over the inline `range`-window slicing) · `REWRITE` (pikepdf object-model surgery folding the qpdf token pairs through the `ContentEdit.fold` over `parse_content_stream`, dropping operators by `str(op)` and renaming `isinstance(token, Name)`-guarded resource operands through `rename_resources`, then writing `make_stream(unparse_content_stream(folded))` into `page.obj[Name.Contents]`) · `REDACT` (pymupdf staging each `AnnotKind.REDACTION` `target` rect as a black `add_redact_annot` carrying the optional `overlay_text` over white `text_color` under the `Label` `(fontname, fontsize, align)` typography axis, burning through `apply_redactions` with the `PDF_REDACT_*` flag triple, flattening annotations through `bake`, sweeping residual content through the `Scrub` sanitize axis off `scrub(**Scrub.kwargs())`, and subsetting embedded fonts through `subset_fonts(fallback=False)`, destroying the underlying content bytes irreversibly) · `PROTECT` (msoffcrypto `OfficeFile.is_encrypted` gating, the format-discriminated credential map — the OOXML `keyTypes` axis or the legacy-97 `("password",)` axis recovered through one `hasattr(office, "keyTypes")` discriminant — driving one `load_key(**credentials, **verify)` deriving the key, the `verify_password`/`verify_integrity` kwargs riding the OOXML leg only because the 97 `load_key(password=None)`/`decrypt(outfile)` rows admit no verify knob, `decrypt(outfile, **verify)` streaming the plaintext, raising `InvalidKeyError` on an empty credential set) — each one `Finisher.arm` resolved off the `FINISHERS` table, never re-enumerated by a worker-side `match`.
- Entry: `DocumentEgress.finish` is `async` over the runtime `async_boundary`, dispatching the step inside the one fault capsule — the `DocumentEgress.band` property reads the `Finisher.band` table default and overrides it from `Encryption.band` for the ENCRYPT step, routing the cp315-core arms (`OUTLINE` pypdf, `IMPOSE` pypdf, the `RC4`/`AES-128` `ENCRYPT` pypdf leg, `REDACT` pymupdf, `PROTECT` msoffcrypto) in-process and the worker-band arms (`WATERMARK`/`ATTACH`/`REWRITE` pikepdf, the `AES-256` `ENCRYPT` pikepdf leg) across `anyio.to_process.run_sync` onto `_worker_finish` — and returns a `RuntimeRail[ContentKey]` keyed by the content key minted over the `FinishFact.data` finished bytes.
- Auto: `_emit` routes on `DocumentEgress.band` to the in-process `FINISHERS[step].arm(self)` call or the `to_process.run_sync(_worker_finish, self)` worker, stashes the returned `FinishFact` into `params["_fact"]` so the receipt reuses the produced evidence, then mints the content key over `fact.data` through `ContentIdentity.of`; `_worker_finish` is one line — it re-resolves `FINISHERS[egress.step].arm` and calls it, so the worker carries no second step `match`, the `FinishFact` (a plain `msgspec.Struct` of bytes and ints) crosses the process seam back intact, and the arm body is identical in either lane. The ENCRYPT arm discriminates on `Encryption.band` inside `_encrypt`: a `WORKER`-band `Encryption` (`AES-256`) runs the pikepdf `Pdf.save` leg projecting `Permissions.to_pikepdf` and reporting `len(pdf.pages)`, otherwise the pypdf `PdfWriter.encrypt` leg projects `Encryption.use_128bit`/`Encryption.algorithm` plus `Permissions.to_pypdf` and reports `len(writer.pages)`. Every arm imports its package at boundary scope inside the arm body; no module-top native import lands on the core owner, and the worker-shipped arm imports pikepdf inside the subprocess.
- Receipt: each finish contributes one `folder:../receipt/receipt#RECEIPT` case — `DocumentEgress.contribute` builds the `ArtifactReceipt` case off the `FinishFact` ternary and yields the `Iterable[Receipt]` stream the runtime `ReceiptContributor` port declares by delegating `yield from case.contribute()` to the case's own `ArtifactReceipt.contribute` (which projects the case facts through `_facts` onto the 2-positional `Receipt.of("artifacts", ("emitted", tag, facts))` contract), so the egress projection carries no second port method, no parallel `Receipt` projector, and no rail-threaded key. `DocumentEgress.contribute` reads the arm's `FinishFact` (the stashed `params["_fact"]`, or one re-run of the arm when the receipt is requested without a prior `_emit`), re-mints the content key over `fact.data` through the same `ContentIdentity.of` `_emit` used so the projection is self-contained with no caller-passed key or bytes, and folds the case off `DocumentEgress.step` in one ternary: the PDF-finishing rows (`ENCRYPT`/`OUTLINE`/`WATERMARK`/`ATTACH`/`IMPOSE`/`REWRITE`/`REDACT`) emit `ArtifactReceipt.Egress` carrying the content key, the post-finish byte count, and the `fact.pages`/`fact.encryption_r`/`fact.outline_depth`/`fact.overlays` evidence the arm produced (each defaulting to `0` off the rows that do not produce it); the `PROTECT` step emits `ArtifactReceipt.Office` carrying the content key and the decrypted byte count — one shared receipt stream off the one `FinishFact` shape, the `Finisher.contributes` column and the prior `_egress_receipt`/`_pages_in` re-parse both collapsed because the arm produces every fact, never a per-row receipt delegate, never a parallel `_office_receipt` function, never a second pypdf parse of the finished bytes, never a dead `data: bytes` parameter restating `fact.data`, and never threaded through the rail beside the key.
- Packages: `pikepdf` (`open`/`Pdf.save(linearize, encryption)`/`Encryption(owner, user, R, aes, allow)`/`Permissions`/`Pdf.pages`/`Page`/`Page.add_overlay`/`add_underlay`/`Page.contents_coalesce`/`Pdf.attachments`/`AttachedFileSpec`/`parse_content_stream`/`unparse_content_stream`/`Pdf.make_stream`/`Page.obj`/`Object`/`Dictionary`/`Stream`/`Name`, qpdf-native wheel crossing the subprocess seam), `pypdf` (`PdfReader`/`PdfWriter`/`PdfWriter.pages`/`PdfWriter.add_outline_item`/`PdfWriter.encrypt(use_128bit, algorithm, permissions_flag)`/`PageObject.add_transformation`/`merge_page`/`add_blank_page`/`Transformation`/`pypdf.constants.UserAccessPermissions`, pure-Python on the cp315 core), `pymupdf` (`open`/`Page.add_redact_annot(text, fontname, fontsize, align, fill, text_color, cross_out)`/`Page.apply_redactions`/`Document.page_count`/`Document.bake`/`Document.scrub`/`Document.subset_fonts`/`Document.save`/`Rect`/`PDF_REDACT_IMAGE_REMOVE`/`PDF_REDACT_LINE_ART_REMOVE_IF_TOUCHED`/`PDF_REDACT_TEXT_REMOVE`, native MuPDF on the cp315 core), `msoffcrypto` (`OfficeFile` factory dispatch to `OOXMLFile`/`Doc97File`/`Xls97File`/`Ppt97File`, `BaseOfficeFile.is_encrypted`, `OOXMLFile.keyTypes`, the polymorphic `load_key(password, private_key, secret_key, verify_password)` OOXML row and `load_key(password)` legacy-97 row, `decrypt(outfile, verify_integrity)` OOXML row and `decrypt(outfile)` legacy-97 row, `exceptions.FileFormatError`/`exceptions.InvalidKeyError`, pure-Python on the cp315 core); `folder:documents/model#NODE` (`DocumentNode`/`SectionNode`/`AnnotationNode`/`AnnotKind`/`NodeMeta`/`walk` the outline-and-redaction-fold source); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` the runtime subprocess lane for the worker-band arms).
- Growth: a new finishing step is one `EgressStep` row plus one `Finisher` row in `FINISHERS` carrying its band and `FinishFact`-returning arm; a new receipt fact is one `FinishFact` field the arm populates, never a re-derivation off the bytes; a new permission knob is one `Permissions` field projected once into both the qpdf and pypdf flag sets, never a new encrypt parameter; an encryption strength is one `Strength` row plus one `_STRENGTHS` cell carrying its `(r, band, use_128bit)` triple, never a parallel encrypt owner or a per-leg ternary; a sanitize knob is one `Scrub` field projected into the `scrub` kwarg map, never a `_redact` parameter; a redaction-label knob is one `Label` field projected into the `add_redact_annot` kwarg map; a credential kind is already carried by the format-discriminated credential map (the OOXML `keyTypes` axis or the legacy-97 `("password",)` axis), never a `load_key` method family and never a per-format `_protect` arm; a deeper outline is the same `SectionNode` fold over more heading levels; a redaction kind is the same `AnnotKind.REDACTION` filter over more rects; an n-up layout change is a `Transformation` parameter; a content-stream edit is one more `ContentEdit` field folded through `ContentEdit.fold`; zero new surface.
- Boundary: this owner finishes an already-emitted artifact and authors none — a PDF-from-scratch path, a per-finishing-step service class family, a stringly-typed `if step == "encrypt"` branch, scattered boolean permission knobs, a hardcoded `NATIVE` frozenset, a worker-side `_on_worker` sentinel arm, a per-row `Finisher.contributes` receipt delegate, a second worker `match` re-enumerating the steps, a `_pages_in` pypdf re-parse of the finished bytes, and an `(r, aes)` ternary that admits the invalid R2/AES-128 cell are the deleted forms. PAdES cryptographic re-signing is never an egress step and routes to `folder:../typography/conformance#CONFORM`; PDF/A structural authoring is never an egress step and routes to `folder:documents/emit#DOCUMENT` `PDF_TYPST`; the recover-TO extraction half routes to `folder:documents/lens#LENS`. Office password re-encryption is not an egress capability — msoffcrypto owns detection and decryption only, so the `PROTECT` row gates on `is_encrypted`, filters the supplied credentials through the format-discriminated credential axis (OOXML `keyTypes` or legacy-97 `password`), decrypts, and never re-protects. The `DocumentEgress.band` property decides the runtime lane off the `Finisher.band` table default plus the `Encryption.band` ENCRYPT override: worker-band qpdf arms ride `anyio.to_process.run_sync` where `_worker_finish` re-resolves the row, the cp315-core pypdf/pymupdf/msoffcrypto arms resolve in-process, and every arm imports its package at boundary scope so neither a module-top nor a lazy native import lands on the core owner. The content key is minted over the `FinishFact.data` finished bytes through the runtime `ContentIdentity.of`, never re-minted off the source key.

```python signature
from collections.abc import Callable, Iterable, Iterator, Mapping
from enum import StrEnum
from functools import reduce
from io import BytesIO
from operator import or_
from typing import TYPE_CHECKING, Final

from anyio import to_process
from msgspec import Struct, field, structs

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.receipts import Receipt

from artifacts.documents.model import AnnotKind, AnnotationNode, DocumentNode, SectionNode, walk
from artifacts.receipt.receipt import ArtifactReceipt

if TYPE_CHECKING:
    import pikepdf
    from pypdf.constants import UserAccessPermissions


class EgressStep(StrEnum):
    ENCRYPT = "encrypt"
    OUTLINE = "outline"
    WATERMARK = "watermark"
    ATTACH = "attach"
    IMPOSE = "impose"
    REWRITE = "rewrite"
    REDACT = "redact"
    PROTECT = "protect"


class Band(StrEnum):
    CORE = "core"
    WORKER = "worker"


class Strength(StrEnum):
    RC4_40 = "RC4-40"
    RC4_128 = "RC4-128"
    AES_128 = "AES-128"
    AES_256 = "AES-256"


_STRENGTHS: Final[dict[Strength, tuple[int, Band, bool]]] = {
    Strength.RC4_40: (2, Band.CORE, False),
    Strength.RC4_128: (4, Band.CORE, False),
    Strength.AES_128: (4, Band.CORE, True),
    Strength.AES_256: (6, Band.WORKER, True),
}


class Permissions(Struct, frozen=True):
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
            (self.extract, P.EXTRACT | P.EXTRACT_TEXT_AND_GRAPHICS),
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

    def kwargs(self) -> dict[str, str | float | int]:
        return structs.asdict(self)


class ContentEdit(Struct, frozen=True):
    drop_operators: frozenset[str] = frozenset()
    rename_resources: Mapping[str, str] = field(default_factory=dict)

    def fold(self, page: "pikepdf.Page") -> bytes:
        import pikepdf

        rename = {pikepdf.Name(f"/{k}"): pikepdf.Name(f"/{v}") for k, v in self.rename_resources.items()}
        kept = [
            ([rename.get(tok, tok) if isinstance(tok, pikepdf.Name) else tok for tok in operands], op)
            for operands, op in pikepdf.parse_content_stream(page)
            if str(op) not in self.drop_operators
        ]
        return pikepdf.unparse_content_stream(kept)


class FinishFact(Struct, frozen=True):
    data: bytes
    pages: int = 0
    encryption_r: int = 0
    outline_depth: int = 0
    overlays: int = 0


class Finisher(Struct, frozen=True):
    band: Band
    arm: Callable[["DocumentEgress"], FinishFact]


class DocumentEgress(Struct, frozen=True):
    step: EgressStep
    pdf: bytes
    params: dict[str, object]
    node: DocumentNode | None = None
    permissions: Permissions = Permissions()
    encryption: Encryption | None = None
    scrub: Scrub = Scrub()
    label: Label = Label()
    edit: ContentEdit = ContentEdit()

    @property
    def band(self) -> Band:
        if self.step is EgressStep.ENCRYPT and self.encryption is not None:
            return self.encryption.band
        return FINISHERS[self.step].band

    async def finish(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"egress.{self.step}", self._emit)

    async def _emit(self) -> ContentKey:
        fact = await to_process.run_sync(_worker_finish, self) if self.band is Band.WORKER else FINISHERS[self.step].arm(self)
        self.params["_fact"] = fact
        return ContentIdentity.of(f"egress-{self.step}", fact.data)

    def contribute(self) -> Iterable[Receipt]:
        fact = self.params.get("_fact") or FINISHERS[self.step].arm(self)
        key = ContentIdentity.of(f"egress-{self.step}", fact.data)
        case = (
            ArtifactReceipt.Office(key, len(fact.data))
            if self.step is EgressStep.PROTECT
            else ArtifactReceipt.Egress(key, len(fact.data), fact.pages, fact.encryption_r, fact.outline_depth, fact.overlays)
        )
        yield from case.contribute()


def _worker_finish(egress: "DocumentEgress") -> FinishFact:
    return FINISHERS[egress.step].arm(egress)


def _sections(node: DocumentNode | None) -> Iterator[SectionNode]:
    return (n for n in walk(node) if isinstance(n, SectionNode)) if node is not None else iter(())


def _redaction_rects(node: DocumentNode | None) -> dict[int, tuple[tuple[float, float, float, float], ...]]:
    annots = (n for n in (walk(node) if node is not None else ()) if isinstance(n, AnnotationNode) and n.annot is AnnotKind.REDACTION)
    return reduce(lambda acc, a: {**acc, a.meta.page: (*acc.get(a.meta.page, ()), a.target)}, annots, {})


def _encrypt(egress: DocumentEgress) -> FinishFact:
    enc, sink = egress.encryption, BytesIO()
    if enc.band is Band.WORKER:
        import pikepdf

        pdf = pikepdf.open(BytesIO(egress.pdf))
        pdf.save(
            sink,
            linearize=True,
            encryption=pikepdf.Encryption(owner=enc.owner, user=enc.user, R=enc.r, aes=True, allow=egress.permissions.to_pikepdf()),
        )
        return FinishFact(sink.getvalue(), pages=len(pdf.pages), encryption_r=enc.r)
    from pypdf import PdfReader, PdfWriter

    writer = PdfWriter(clone_from=PdfReader(BytesIO(egress.pdf)))
    writer.encrypt(
        user_password=enc.user,
        owner_password=enc.owner,
        use_128bit=enc.use_128bit,
        algorithm=enc.algorithm,
        permissions_flag=egress.permissions.to_pypdf(),
    )
    writer.write(sink)
    return FinishFact(sink.getvalue(), pages=len(writer.pages), encryption_r=enc.r)


def _outline(egress: DocumentEgress) -> FinishFact:
    from pypdf import PdfReader, PdfWriter

    writer = PdfWriter(clone_from=PdfReader(BytesIO(egress.pdf)))
    parents: dict[int, object] = {}
    depth = 0
    for section in _sections(egress.node):
        title = "".join(run.text for run in section.heading)
        parents[section.level] = writer.add_outline_item(title, section.meta.page, parent=parents.get(section.level - 1))
        depth = max(depth, section.level)
    if egress.node is None:
        for title, page in egress.params.get("bookmarks", ()):
            writer.add_outline_item(title, page)
    sink = BytesIO()
    writer.write(sink)
    return FinishFact(sink.getvalue(), pages=len(writer.pages), outline_depth=depth)


def _impose(egress: DocumentEgress) -> FinishFact:
    from pypdf import PdfReader, PdfWriter, Transformation

    reader = PdfReader(BytesIO(egress.pdf))
    across, down = int(egress.params.get("across", 2)), int(egress.params.get("down", 1))
    width, height = egress.params["sheet"]
    cell_w, cell_h, slots = width / across, height / down, across * down
    writer = PdfWriter()
    for start in range(0, len(reader.pages), slots):
        sheet = writer.add_blank_page(width=width, height=height)
        for offset, index in enumerate(range(start, min(start + slots, len(reader.pages)))):
            source, (row, col) = reader.pages[index], divmod(offset, across)
            source.add_transformation(
                Transformation()
                .scale(cell_w / source.mediabox.width, cell_h / source.mediabox.height)
                .translate(col * cell_w, (down - 1 - row) * cell_h)
            )
            sheet.merge_page(source)
    sink = BytesIO()
    writer.write(sink)
    return FinishFact(sink.getvalue(), pages=len(writer.pages))


def _watermark(egress: DocumentEgress) -> FinishFact:
    import pikepdf

    pdf = pikepdf.open(BytesIO(egress.pdf))
    stamp = pikepdf.open(BytesIO(egress.params["stamp"]))
    mark = pikepdf.Page(stamp.pages[0])
    mark.contents_coalesce()
    place = pikepdf.Page.add_underlay if bool(egress.params.get("under", False)) else pikepdf.Page.add_overlay
    overlays = sum(place(pikepdf.Page(page), mark) is None for page in pdf.pages)
    sink = BytesIO()
    pdf.save(sink, linearize=True)
    return FinishFact(sink.getvalue(), pages=len(pdf.pages), overlays=overlays)


def _attach(egress: DocumentEgress) -> FinishFact:
    import pikepdf

    pdf = pikepdf.open(BytesIO(egress.pdf))
    name = str(egress.params["name"])
    pdf.attachments[name] = pikepdf.AttachedFileSpec(
        pdf, egress.params["data"], filename=name, description=str(egress.params.get("description", ""))
    )
    sink = BytesIO()
    pdf.save(sink, linearize=True)
    return FinishFact(sink.getvalue(), pages=len(pdf.pages))


def _rewrite(egress: DocumentEgress) -> FinishFact:
    import pikepdf

    pdf = pikepdf.open(BytesIO(egress.pdf))
    for page in pdf.pages:
        page.obj[pikepdf.Name.Contents] = pdf.make_stream(egress.edit.fold(page))
    sink = BytesIO()
    pdf.save(sink, linearize=True)
    return FinishFact(sink.getvalue(), pages=len(pdf.pages))


def _redact(egress: DocumentEgress) -> FinishFact:
    import pymupdf

    doc = pymupdf.open(stream=egress.pdf, filetype="pdf")
    overlay = str(egress.params.get("overlay_text", "")) or None
    for index, page_rects in _redaction_rects(egress.node).items():
        page = doc[index]
        for rect in page_rects:
            page.add_redact_annot(
                pymupdf.Rect(rect), text=overlay, fill=(0.0, 0.0, 0.0), text_color=(1.0, 1.0, 1.0), cross_out=overlay is None, **egress.label.kwargs()
            )
        page.apply_redactions(
            images=pymupdf.PDF_REDACT_IMAGE_REMOVE,
            graphics=pymupdf.PDF_REDACT_LINE_ART_REMOVE_IF_TOUCHED,
            text=pymupdf.PDF_REDACT_TEXT_REMOVE,
        )
    doc.bake(annots=True, widgets=bool(egress.params.get("flatten_widgets", True)))
    doc.scrub(**egress.scrub.kwargs())
    doc.subset_fonts(fallback=False)
    sink = BytesIO()
    doc.save(sink, garbage=4, deflate=True, clean=True)
    return FinishFact(sink.getvalue(), pages=doc.page_count)


def _protect(egress: DocumentEgress) -> FinishFact:
    import msoffcrypto
    from msoffcrypto.exceptions import FileFormatError, InvalidKeyError

    office = msoffcrypto.OfficeFile(BytesIO(egress.pdf))
    if not office.is_encrypted():
        raise FileFormatError("egress.protect: container is not encrypted")
    ooxml = hasattr(office, "keyTypes")
    key_axis = office.keyTypes if ooxml else ("password",)
    credentials = {kind: egress.params[kind] for kind in key_axis if kind in egress.params}
    if not credentials:
        raise InvalidKeyError(f"egress.protect: no admissible key input in {key_axis}")
    office.load_key(**credentials, **({"verify_password": True} if ooxml else {}))
    sink = BytesIO()
    office.decrypt(sink, **({"verify_integrity": bool(egress.params.get("verify_integrity", True))} if ooxml else {}))
    return FinishFact(sink.getvalue())


FINISHERS: Final[dict[EgressStep, Finisher]] = {
    EgressStep.ENCRYPT: Finisher(Band.CORE, _encrypt),
    EgressStep.OUTLINE: Finisher(Band.CORE, _outline),
    EgressStep.WATERMARK: Finisher(Band.WORKER, _watermark),
    EgressStep.ATTACH: Finisher(Band.WORKER, _attach),
    EgressStep.IMPOSE: Finisher(Band.CORE, _impose),
    EgressStep.REWRITE: Finisher(Band.WORKER, _rewrite),
    EgressStep.REDACT: Finisher(Band.CORE, _redact),
    EgressStep.PROTECT: Finisher(Band.CORE, _protect),
}
```

## [03]-[RESEARCH]

- [SEAM_CROSSING]: pikepdf is the qpdf-backed native wheel reflecting on the gated `python_version<'3.15'` band (catalogue header `10.8.0 reflected ... on the gated python_version<'3.15'> band (cp313)`), so the `WATERMARK`/`ATTACH`/`REWRITE` arms and the `AES-256` `ENCRYPT` leg carry `Band.WORKER`, crossing `anyio.to_process.run_sync` onto `_worker_finish`, which re-resolves `FINISHERS[egress.step].arm` and runs the SAME arm body that imports `pikepdf` at boundary scope inside the subprocess. pypdf is pure-Python (catalogue `[01]-[PACKAGE_SURFACE]` `6.13.2 reflected ... on cp315`), pymupdf is native MuPDF reflecting on the cp315 core (catalogue `[01]-[PACKAGE_SURFACE]` `1.27.2.3 reflected ... on cp315`), and msoffcrypto is pure-Python on the cp315 core (catalogue `[01]-[PACKAGE_SURFACE]` `6.0.0 reflected ... on cp315`), so the `OUTLINE`/`IMPOSE`/`REDACT`/`PROTECT` arms and the `RC4`/`AES-128` `ENCRYPT` leg carry `Band.CORE` and resolve in-process; the `import pypdf`/`pymupdf`/`msoffcrypto` calls sit at boundary scope per the manifest import policy, never module-top. The `FinishFact` returned across `to_process.run_sync` is a flat `msgspec.Struct` of `bytes` and `int` fields, so it serializes across the subprocess seam without a custom encoder.
- [BAND_COLLAPSE]: the `Band` discriminant carried on each `Finisher` row plus the `DocumentEgress.band` property replace the deleted `NATIVE: frozenset[EgressStep]`, the `_on_worker` sentinel arm, and the `_pure_finish`/`_native_finish` dual `match` cascade — the `FINISHERS` table binds one arm per step, `_emit` is the one dispatch, and `_worker_finish` is a one-line row re-resolution that carries NO second step `match`, so the arm body is identical whether it runs in-process or shipped to the subprocess. The `ENCRYPT` row carries `Band.CORE` as its table default and `DocumentEgress.band` overrides it from `Encryption.band` (the `_STRENGTHS[Strength.AES_256]` cell carries `Band.WORKER`), the single `_encrypt` arm then selecting its pikepdf or pypdf leg off the same `Encryption.band`, so the strength choice routes both the lane and the library without a parallel encrypt method. The `_STRENGTHS` table is the cross-product owner collapsing the `(r, band, use_128bit)` triple onto one `Strength` row, so the deleted `(r, aes)` ternary — which admitted the invalid R2/AES-128 cell and contradicted `use_128bit` against an AES algorithm — cannot recur. A new finishing step lands as one `EgressStep` value plus one `FINISHERS` row; a missing table entry is a `KeyError` at admission, and the totality is the table itself rather than a worker-side exhaustiveness arm.
- [PIKEPDF_SPELLINGS]: the `pikepdf.open(src, *, password='', attempt_recovery=True)` (catalogue `[03]` row `[01]`), `Pdf.save(target, *, linearize=False, encryption=None)` (catalogue `[03]` row `[03]`), `Encryption(owner, user, R=6, aes=True, allow=Permissions(...))`, `Permissions` row policy (catalogue `[02]` rows `[03]`/`[04]` and `[04]-[IMPLEMENTATION_LAW]` encryption-axis line `Encryption(...) is one policy object; permission flags are Permissions rows, never parallel boolean knobs`), `Pdf.pages` (catalogue page-assembly row `[01]`), `Page.add_overlay(other, ...)`/`add_underlay(other, ...)`/`Page.contents_coalesce` (catalogue page-assembly rows `[02]`/`[03]`/`[05]`), `parse_content_stream`/`unparse_content_stream` (catalogue page-assembly rows `[07]`/`[08]`), `Pdf.make_stream(data) -> Stream` and `page.obj[Name.Contents]` write-back (catalogue page-assembly rows `[09]`/`[06]`, endorsed by the `[04]-[IMPLEMENTATION_LAW]` object-axis line `writes the re-encoded unparse_content_stream bytes back through Pdf.make_stream into page.obj[Name.Contents]`), `Pdf.attachments[name] = AttachedFileSpec(pdf, data, filename=name, description=...)` (catalogue page-assembly rows `[10]`/`[11]`, endorsed by the `[04]-[IMPLEMENTATION_LAW]` attachment-axis line), the `Page` unit (catalogue `[02]` document-type row `[02]`), and the `Object`/`Dictionary`/`Name`/`Stream` object model (catalogue `[02]` object-model rows `[01]`/`[02]`/`[04]`/`[05]`) all verify against the folder `.api` catalogue for `pikepdf`, so the `WATERMARK` overlay/underlay arm, the `ATTACH` embed arm, the `_encrypt` pikepdf-leg `Encryption(...)` policy, and the `_rewrite` object-model fold are settled fence code. The `Permissions` type is catalogued (`[02]` row `[04]` `permission flags`, `[04]-[IMPLEMENTATION_LAW]` encryption-axis `permission flags are Permissions rows`), but its individual qpdf field names — `extract`/`modify_other`/`modify_annotation`/`modify_assembly`/`modify_form`/`print_lowres`/`print_highres` — are NOT enumerated in the catalogue, so the `Permissions.to_pikepdf` field mapping stays a marked RESEARCH item ([RESEARCH:PIKEPDF_PERMISSION_FIELDS]): the catalogue verifies that `Permissions` is the one policy type the `allow=` keyword consumes, and `assay api` reflection over `pikepdf.Permissions` deepens the catalogue with the seven keyword spellings before the field mapping promotes to settled fence code. The `WATERMARK` source page is opened into a named `stamp` `Pdf` that outlives the overlay loop and `contents_coalesce` forces its `/Contents` indirect before `add_overlay`/`add_underlay` copy it across documents — the dangling-source and direct-object-xobject faults are the deleted forms, `as_form_xobject` is never pre-called since `add_overlay` performs the conversion internally, and the placed-overlay count is one `sum` over the apply-and-count generator (`place(...) is None` is `True` per placed overlay because `add_overlay`/`add_underlay` return `None`) so the `FinishFact.overlays` evidence is the loop's own count, never a re-parse. The `_rewrite` arm folds the qpdf token pairs through the `ContentEdit` policy value object's `ContentEdit.fold` over `parse_content_stream`, dropping operators by `str(op)` membership and renaming only `isinstance(token, pikepdf.Name)`-guarded resource operands through the `rename_resources` map — the guard is load-bearing because a content-stream operand may be an unhashable `Array`/`Dictionary` (catalogue object-model rows `[03]`/`[02]`) that a bare `dict.get(token)` membership test raises `TypeError` on — then writes `make_stream(unparse_content_stream(kept))` into `page.obj[Name.Contents]`; operand-level object-model surgery folded through one frozen policy value, not a mere inline operator drop, so a new content-stream edit is one `ContentEdit` field rather than a `_rewrite` parameter. The `rename_resources` keys map bare resource names to qpdf `/Name` tokens through the leading-slash `pikepdf.Name(f"/{k}")` spelling the `Name` constructor requires. The `ContentEdit` CTM-prepend extension (a `prepend_matrix` field minting a leading `cm` token pair through `pikepdf.Matrix`, `pikepdf.Operator`, and `pikepdf.Object.parse`) stays a marked RESEARCH alternate ([RESEARCH:PIKEPDF_CONTENT_CM]) — the catalogue carries `Matrix` (object-model row `[06]` `content-stream affine matrix`) and `Object` (object-model row `[01]`) but not the `pikepdf.Operator(...)` constructor, the `Object.parse(repr(Matrix(...)))` operand-construction call shape, nor the `Matrix.__repr__`-to-`cm`-operand contract, so the settled `ContentEdit.fold` carries only the verified operator-drop and resource-rename token edits. The declarative `Job.run` job-JSON pipeline (catalogue page-assembly row `[13]` `Job.run` `execute a declarative qpdf job`, the surface but not the `Job(spec).run(stdin=, stdout=)` constructor and call shape) stays a marked RESEARCH alternate ([RESEARCH:PIKEPDF_JOB_RUN]) — the settled `REWRITE` path is the `ContentEdit.fold` object-model token fold, the declarative job pipeline the deferred denser alternate once the `Job(spec)` constructor and stream-IO call rows are catalogued.
- [PYPDF_ENCRYPT_CORE]: the `PdfWriter.encrypt(user_password, owner_password=None, use_128bit=True, *, algorithm=None, permissions_flag=...)` spelling verifies against the folder `.api` catalogue for `pypdf` (catalogue page-assembly row `[04]`, `algorithm` ∈ `RC4-40`/`RC4-128`/`AES-128`/`AES-256`/`AES-256-R5`, endorsed by the `[04]-[IMPLEMENTATION_LAW]` encryption-axis line), and the `PdfReader`/`PdfWriter(clone_from=...)` (catalogue `[02]` rows `[01]`/`[02]`, `[04]-[IMPLEMENTATION_LAW]` reader/writer-split line), `PdfWriter.pages` (catalogue `[02]` writer-root row `[02]` `append pages, ... pages`, the same page collection `add_page`/`append` extend), `PdfWriter.add_outline_item`, `PageObject.add_transformation`/`merge_page` (catalogue page-assembly rows `[09]`/`[10]`), `PdfWriter.add_blank_page` (catalogue page-assembly row `[03]`), `Transformation().scale(...).translate(...)` (catalogue `[02]` row `[04]`), and the `pypdf.constants.UserAccessPermissions` `IntFlag` members `PRINT`/`MODIFY`/`EXTRACT`/`EXTRACT_TEXT_AND_GRAPHICS`/`ADD_OR_MODIFY`/`ASSEMBLE_DOC`/`FILL_FORM_FIELDS`/`PRINT_TO_REPRESENTATION` (catalogue `[02]` enum row `[04]`) all verify, so the `OUTLINE`/`IMPOSE` arms, the `_encrypt` pypdf leg keyword arguments, the `len(writer.pages)` page-count evidence, and the `Permissions.to_pypdf` projection are settled fence code. `Permissions.to_pypdf` folds the one gate-flag row table to a single `IntFlag` through `reduce(or_, ...)` — `extract` maps to `EXTRACT | EXTRACT_TEXT_AND_GRAPHICS` and `assemble` to `ASSEMBLE_DOC` (never the non-existent `ASSEMBLE_DOCUMENT`). The `Strength` vocabulary projects to BOTH legs off the one `_STRENGTHS` cross-product table: each `Strength` row carries its `(r, band, use_128bit)` triple, `Encryption.algorithm` is the `Strength.value` the pypdf `algorithm=` keyword consumes against the catalogued range, and `Encryption.use_128bit` is the table cell the pypdf `use_128bit=` keyword consumes for RC4 strength selection, so the `R` level, the lane, and the `use_128bit` toggle derive from the one row rather than a parallel encrypt parameter or an `(r, aes)` ternary, and the `Encryption.band` property routes the pikepdf-only `AES-256`/R6 leg to the worker while the `RC4-40`/`RC4-128`/`AES-128` pypdf legs stay core. The four `Strength` rows are exactly the valid `(R, algorithm)` cells — `RC4-40` (R2), `RC4-128` (R4), `AES-128` (R4), `AES-256` (R6) — so the invalid R2/AES-128 cell the prior `(r, aes)` match admitted is structurally unrepresentable. This leg removes the subprocess hop for the non-`AES-256` case: every `ENCRYPT` finish crossed the seam under the prior `NATIVE` set even for non-AES-256, and pypdf covers RC4 and AES-128 in-process while pikepdf covers AES-256.
- [PYMUPDF_REDACT]: the `Page.add_redact_annot(quad, text=None, fontname=None, fontsize=11, align=0, fill=None, text_color=None, cross_out=True)` staging, `Page.apply_redactions(images=PDF_REDACT_IMAGE_REMOVE, graphics=PDF_REDACT_LINE_ART_REMOVE_IF_TOUCHED, text=PDF_REDACT_TEXT_REMOVE)` burn, `Document.bake(annots=True, widgets=True)` annotation/widget flatten, `Document.scrub(attached_files, clean_pages, embedded_files, hidden_text, javascript, metadata, redactions, redact_images, remove_links, reset_fields, reset_responses, thumbnails, xml_metadata)` sanitize sweep, the `Document.page_count` attribute, and `Document.subset_fonts(verbose=False, fallback=False) -> int | None` embedded-font shrink all verify against the folder `.api` catalogue for `pymupdf` (catalogue page-authoring row `[01]` carrying the `text`/`fontname`/`fontsize`/`align`/`fill`/`text_color`/`cross_out` keywords, render row `[07]` carrying the `PDF_REDACT_*` int-flag spelling, scrub-bake-subset rows `[01]`/`[02]`/`[03]`), and the `pymupdf.open(stream=..., filetype="pdf")` (catalogue document row `[01]`), `Document.save` (catalogue document row `[03]`), `pymupdf.Rect` value object (catalogue `[02]` geometry row `[01]`), and `Page` unit (catalogue `[02]` row `[02]`) spellings verify, so the entire `REDACT` arm is settled fence code. The arm folds the `folder:documents/model#NODE` `AnnotationNode` nodes whose `annot is AnnotKind.REDACTION` through `reduce` over `walk`, grouping their `target` rects by `meta.page`, stages each rect as a black `add_redact_annot` whose optional `overlay_text` writes a white-on-black replacement label over `text_color=(1, 1, 1)` (a `cross_out` strike-through only when no overlay text is supplied) under the `Label` policy value object's `(fontname, fontsize, align)` typography triple projected through `Label.kwargs()`, burns each page through `apply_redactions` with the image-remove/line-art-touch/text-remove flag triple, flattens annotations and form widgets into content through `bake`, then sweeps residual content through the `Scrub` policy value object's full sanitize axis (`scrub(**Scrub.kwargs())` with `redactions=False` because the redaction annotations are already consumed, covering attachments, embedded files, hidden text, javascript, links, form fields, thumbnails, XML and info metadata, and page cleaning) and shrinks the surviving fonts through `subset_fonts(fallback=False)` before `save(garbage=4, deflate=True, clean=True)`, reporting `doc.page_count` as the `FinishFact` evidence — destroying the underlying content bytes irreversibly and closing the `model#NODE` reversible-until-burned promise the `documents/model#DELTA` redaction-patch inverse opens. `Scrub` and `Label` are the redaction counterparts of the `Permissions` encryption policy — frozen value objects projecting once into the `scrub` and `add_redact_annot` kwarg maps through `Scrub.kwargs()`/`Label.kwargs()`, so a new sanitize knob is one `Scrub` field and a new label knob one `Label` field rather than a `_redact` parameter, the catalogue `scrub` row's thirteen-knob signature is mined to its full sanitize breadth rather than the prior two-knob `redactions`/`metadata` call, and the catalogue `add_redact_annot` row's `fontname`/`fontsize`/`align` typography keywords are mined rather than defaulted away.
- [MSOFFCRYPTO_DECRYPT]: the `OfficeFile`/`OOXMLFile.keyTypes`/`load_key`/`decrypt`/`is_encrypted` spellings and the `exceptions.FileFormatError`/`InvalidKeyError` fault family verify against the folder `.api` catalogue for `msoffcrypto-tool` (catalogue `[03]` factory-dispatch row `[01]` `OfficeFile(file) -> BaseOfficeFile`, `OOXMLFile` rows `[01]` `load_key(password=None, private_key=None, secret_key=None, verify_password=False)` / `[02]` `decrypt(outfile, verify_integrity=False)` / `[03]` `is_encrypted()` / `[04]` `keyTypes -> tuple[str, ...]`, and the `[02]-[PUBLIC_TYPES]` fault rows `[07]` `exceptions.FileFormatError` / `[08]` `exceptions.InvalidKeyError`), endorsed by the `[04]-[IMPLEMENTATION_LAW]` dispatch-axis line `OfficeFile(file) is the single entry; container kind ... is a stream-layout discriminant resolved by the factory`, the key-axis line `load_key owns key derivation; password, private_key, and secret_key are call rows ... verify_password=True folds key verification into load`, and the evidence-axis line `InvalidKeyError is the typed failure for a wrong or unverifiable key, FileFormatError for an unrecognized container`. The `PROTECT` arm is the Office confidentiality counterpart to the PDF `ENCRYPT` row — `OfficeFile` sniffs the container (OOXML agile/standard/extensible or legacy DOC/XLS/PPT 97), `is_encrypted()` (the shared `BaseOfficeFile.is_encrypted` contract, catalogue `[02]` row `[02]` and legacy row `[07]`) gates a plaintext or unrecognized container with a typed `FileFormatError` before any key work, the format-discriminated `{kind: params[kind] for kind in key_axis if kind in params}` credential map folds the container's own credential axis into one `load_key(**credentials, **verify)` call (raising `InvalidKeyError` when the supplied credential set is empty), and `decrypt(outfile, **verify)` streams the plaintext, contributing `ArtifactReceipt.Office`. The credential axis is recovered through one `hasattr(office, "keyTypes")` discriminant: an `OOXMLFile` advertises its `password`/`private_key`/`secret_key` inputs through the `keyTypes` attribute (catalogue `[03]` `OOXMLFile.keyTypes` row `[04]`) and admits the `verify_password`/`verify_integrity` HMAC knobs (catalogue `OOXMLFile.load_key` row `[01]` `verify_password=False`, `OOXMLFile.decrypt` row `[02]` `verify_integrity=False`), so the OOXML leg verifies the key and the HMAC; a legacy `Doc97File`/`Xls97File`/`Ppt97File` carries no `keyTypes` attribute and its `load_key(password=None)`/`decrypt(outfile)` rows (catalogue `[03]` legacy rows `[01]`-`[06]`) admit no verify knob, so the 97 leg resolves the `("password",)` axis and ships `load_key`/`decrypt` with no verify kwarg. Recovering the per-format credential axis is the capability conservation the prior `keyTypes`-only literal lost — that literal read an attribute the 97 format objects do not carry and passed `verify_password`/`verify_integrity` the 97 `load_key`/`decrypt` signatures reject, dropping the entire legacy DOC/XLS/PPT 97 leg the package's factory dispatch admits; the corrected fold drives the catalogue's single polymorphic `load_key`/`decrypt` surface by the format object's own advertised inputs across both container families, never a per-credential method family, never a hardcoded `password` knob, and never a parallel 97 arm. The typed `exceptions` family is the boundary fault surface the runtime `async_boundary` capsule converts, never a bare `Exception` catch. Office re-encryption is NOT a catalogued capability: the catalogue `[05]-[LOCAL_ADMISSION]` RAIL_LAW owns `decrypted-stream output` only and the `[04]-[IMPLEMENTATION_LAW]` decrypt-axis names `decrypt(outfile, ...)` as the single output surface with no encrypt counterpart, so the `PROTECT` row gates on `is_encrypted`, decrypts, and never re-protects; an Office password-protect arm stays a marked RESEARCH item ([RESEARCH:MSOFFCRYPTO_ENCRYPT]) and never appears as settled fence code.
- [OUTLINE_FALLBACK]: the settled `OUTLINE` arm is pypdf-only on `Band.CORE` — the `SectionNode` heading fold through `PdfWriter.add_outline_item` is the primary path over the `DocumentNode` tree, and the `params["bookmarks"]` `(title, page)` pair sequence is the verified fallback when no tree is supplied, both composing the catalogued `add_outline_item` (catalogue `[03]` page-assembly row `[09]`-equivalent, `[04]-[IMPLEMENTATION_LAW]` writer-axis `outline/annotation/attachment authoring`). The weasyprint `Document.make_bookmark_tree(scale=1, transform_pages=False)` and `HTML.render(font_config=...)` outline-tree lowering verify against the `weasyprint` catalogue (`[03]` render-and-output rows `[02]`/`[05]`, `Page.bookmarks` at `[02]` members row `[04]`) as a denser HTML-authored alternate, but the egress fence never carries a weasyprint arm: weasyprint loads native `libpango`/`libgobject`/`libcairo` through cffi at import time and its `make_bookmark_tree`-to-pypdf-merge call chain is not reflected, so it stays a marked RESEARCH alternate ([RESEARCH:WEASYPRINT_OUTLINE_WORKER]) on `Band.WORKER` and never appears as settled fence code — the pypdf `add_outline_item` fold is the one settled outline owner, never two competing owners.
- [BOUNDARY_AGAINST_CONFORMANCE]: the egress close finishes structure, navigation, content surgery, redaction, and confidentiality only; PAdES cryptographic signing is owned by `folder:../typography/conformance#CONFORM` `SIGN`/`AUDIT` (pyhanko), and PDF/A archival authoring by `folder:documents/emit#DOCUMENT` `PDF_TYPST` `pdf_standards` — neither is an `EgressStep`. The `ENCRYPT` arm is document encryption (a confidentiality control, the `Strength.AES_256` qpdf/R6 worker leg or the `Strength.RC4_40`/`RC4_128`/`AES_128` pypdf core leg), not a digital signature; a signed-then-encrypted archival close is the `conformance#CONFORM` `SIGN` arm composing this owner's `ENCRYPT` output, the signature applied last so encryption never invalidates the byte-range. The `ArtifactReceipt.Egress` case carries the post-finish byte count, page count, encryption R level, outline depth, and overlay count, and the `ArtifactReceipt.Office` case carries the decrypted byte count, so each finish is one fact row in the shared `folder:../receipt/receipt#RECEIPT` stream the `DocumentEgress.contribute` yields by delegating to the built case's own `contribute` through the `ReceiptContributor` port, never a per-step egress receipt.
