# [PY_ARTIFACTS_EGRESS_FINISH]

The security-and-navigation finishing close over an emitted PDF. `DocumentEgress` is ONE owner that takes a PDF already authored by `folder:../document-plan#DOCUMENT` and returns a sealed, navigable, watermarked, attachment-bearing, imposed PDF keyed by the runtime content key — it finishes an emitted PDF and never authors one. `EgressStep` is the closed `StrEnum` over the five finishing operations (encryption, outline-tree authoring, overlay/underlay watermarking, embedded-attachment binding, n-up/booklet imposition), each a policy-table row binding its package call, and `Permissions` is the row-policy value object the encryption step carries instead of scattered boolean knobs. pikepdf owns qpdf-native encryption, page composition, attachments, and structure repair; pypdf owns the pure-Python writer, the `Transformation` imposition algebra, and outline authoring; weasyprint owns the `Document.make_bookmark_tree` outline lowering. Because pikepdf and weasyprint load native libraries, the finishing steps cross the runtime subprocess seam exactly as `folder:../document-plan#DOCUMENT` and `folder:../../imaging/preview#PREVIEW` do, while the OUTLINE step folds the `folder:../model#NODE` `DocumentNode` heading tree so the bookmark hierarchy is recovered from the one semantic tree rather than re-parsed. Re-signing is never an egress step and routes to `folder:../../typography/conformance#CONFORM`; PDF/A authoring is never an egress step and routes to `folder:../document-plan#DOCUMENT`. Every finish returns a `RuntimeRail[ContentKey]` and contributes one `folder:../../receipt/artifact-receipt#RECEIPT` `ArtifactReceipt.Egress` case.

## [1]-[INDEX]

| [CLUSTER]      | [OWNS]                                                                                                                                                              |
| :------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `[2]-[FINISH]` | `DocumentEgress` + `EgressStep` (ENCRYPT/OUTLINE/WATERMARK/ATTACH/IMPOSE) policy-table dispatch over pikepdf/pypdf/weasyprint, with the `Permissions` row-policy value object the ENCRYPT step carries; the OUTLINE step folds the `DocumentNode` heading tree and the IMPOSE step the `pypdf.Transformation` n-up algebra |
| `[3]-[RESEARCH]` | the subprocess-seam crossing (pikepdf qpdf-native + weasyprint cffi cross `anyio.to_process.run_sync`; pypdf is pure-Python on the cp315 core), the `.api`-verified pikepdf/pypdf/weasyprint spellings, and the boundary against `conformance#CONFORM` re-signing and `document-plan#DOCUMENT` authoring |

## [2]-[FINISH]

- Owner: `DocumentEgress` the one finishing-close owner discriminating the egress step; `EgressStep` the closed `StrEnum` over encryption/outline/watermark/attach/impose; `FINISHERS` the policy table binding each step to the finishing arm that drives its owning package over the already-emitted PDF; `Permissions` the row-policy value object the ENCRYPT step carries (one permission-set value projecting to a pikepdf `Permissions`, never parallel boolean print/extract/modify knobs scattered across the encrypt call). pikepdf owns qpdf-native AES encryption, page overlay/underlay composition, attachment specs, and XMP metadata; pypdf owns the pure-Python `PdfWriter` outline authoring and the `Transformation` imposition algebra; weasyprint owns the `Document.make_bookmark_tree` outline lowering.
- Cases: `EgressStep` rows `ENCRYPT` (pikepdf `Pdf.save(encryption=Encryption(owner, user, R=6, aes=True, allow=Permissions(...)))` AES encryption with the carried permission policy) · `OUTLINE` (the bookmark-tree authoring arm: when the PDF was authored from a `DocumentNode` tree it folds the `SectionNode` heading hierarchy through pypdf `PdfWriter.add_outline_item` into nested bookmarks, otherwise it lowers a weasyprint `Document.make_bookmark_tree` tree) · `WATERMARK` (pikepdf `Page.add_overlay`/`add_underlay` stamping a watermark page atop or beneath every page through `as_form_xobject`) · `ATTACH` (pikepdf `AttachedFileSpec` embedding a file into the document catalog) · `IMPOSE` (pypdf n-up/booklet imposition folding each source `PageObject` onto a target sheet through `Transformation` scale-and-translate then `merge_page`) — matched by `match`/`case` over the `FINISHERS` table, never an `if step == ...` cascade.
- Entry: `DocumentEgress.finish` is `async` over the runtime `async_boundary`, dispatching the step inside the one fault capsule — the pure-Python `OUTLINE`/`IMPOSE` pypdf arms run in-process on the cp315 core, while the qpdf-native `ENCRYPT`/`WATERMARK`/`ATTACH` pikepdf arms and the cffi-native weasyprint outline lowering cross `anyio.to_process.run_sync` onto the `_native_finish` gated-band worker — and returns a `RuntimeRail[ContentKey]` keyed by the content key minted over the finished bytes.
- Auto: `_emit` folds the step — `ENCRYPT` opens the emitted PDF through pikepdf `open` and re-saves it through `Pdf.save(encryption=...)` with the carried `Permissions` projected to the qpdf permission flags; `OUTLINE` discriminates on whether a `DocumentNode` tree is supplied (folding its `SectionNode` heading levels into a nested pypdf outline via `walk`) or lowers a weasyprint bookmark tree; `WATERMARK` opens the watermark stamp as a pikepdf form XObject and folds it across every `Pdf.pages` entry through `add_overlay`/`add_underlay`; `ATTACH` binds an `AttachedFileSpec` into the document; `IMPOSE` reduces the source pages onto target sheets through the pypdf `Transformation` n-up fold. The native arms run at module scope inside `_native_finish`; the pure-Python arms run in-process.
- Receipt: each finish contributes `folder:../../receipt/artifact-receipt#RECEIPT` `ArtifactReceipt.Egress` carrying the content key, the post-finish byte count, the page count, the applied encryption R level (`0` when the step is not ENCRYPT), the authored outline depth (`0` when the step is not OUTLINE), and the applied overlay count (`0` when the step is not WATERMARK) — the one `Egress` case the receipt union landed once, never a per-step receipt.
- Packages: `pikepdf` (`open`/`Pdf.save(linearize, encryption)`/`Encryption(owner, user, R, aes, allow)`/`Permissions`/`Pdf.pages`/`Page.add_overlay`/`add_underlay`/`as_form_xobject`/`AttachedFileSpec`/`open_metadata`, qpdf-native wheel crossing the subprocess seam), `weasyprint` (`HTML.render`/`Document.make_bookmark_tree(scale, transform_pages)` outline lowering, cffi-native crossing the subprocess seam), `pypdf` (`PdfReader`/`PdfWriter`/`PdfWriter.add_outline_item`/`PageObject.add_transformation`/`merge_page`/`Transformation`, pure-Python on the cp315 core); `folder:../model#NODE` (`DocumentNode`/`SectionNode`/`walk` the outline-fold source); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` the runtime subprocess lane for the native arms).
- Growth: a new finishing step is one `EgressStep` row plus one `FINISHERS` arm; a new permission knob is one `Permissions` field projected once into the qpdf flag set, never a new encrypt parameter; a deeper outline is the same `SectionNode` fold over more heading levels; an n-up layout change is a `Transformation` parameter, never a new imposition owner; zero new surface.
- Boundary: this owner finishes an already-emitted PDF and authors none — a PDF-from-scratch path, a per-finishing-step service class family, a stringly-typed `if step == "encrypt"` branch, and scattered boolean permission knobs are the deleted forms. PAdES cryptographic re-signing is never an egress step and routes to `folder:../../typography/conformance#CONFORM` (which owns the signed/archival/font-embedded close); PDF/A structural authoring is never an egress step and routes to `folder:../document-plan#DOCUMENT` `PDF_TYPST`; the recover-TO extraction half routes to `folder:../inspection/lens#LENS`. The qpdf-native pikepdf arms and the cffi-native weasyprint outline lowering ride the runtime subprocess lane (`anyio.to_process.run_sync`) where `_native_finish` imports them at module scope; the pure-Python pypdf `OUTLINE`/`IMPOSE` arms resolve on the cp315 core — neither a module-top nor a lazy native import lands on the core owner. The content key is minted over the finished bytes through the runtime `ContentIdentity.of`, never re-minted off the source key.

```python signature
from enum import StrEnum
from io import BytesIO
from typing import assert_never

from anyio import to_process
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.documents.model import DocumentNode, SectionNode, walk


class EgressStep(StrEnum):
    ENCRYPT = "encrypt"
    OUTLINE = "outline"
    WATERMARK = "watermark"
    ATTACH = "attach"
    IMPOSE = "impose"


class Permissions(Struct, frozen=True):
    extract: bool = False
    modify: bool = False
    print_lowres: bool = True
    print_highres: bool = False
    annotate: bool = False
    fill_forms: bool = False
    assemble: bool = False

    def to_pikepdf(self) -> object:
        from pikepdf import Permissions as QpdfPermissions

        return QpdfPermissions(
            extract=self.extract,
            modify_other=self.modify,
            modify_annotation=self.annotate,
            modify_assembly=self.assemble,
            modify_form=self.fill_forms,
            print_lowres=self.print_lowres,
            print_highres=self.print_highres,
        )


NATIVE: frozenset[EgressStep] = frozenset(
    {EgressStep.ENCRYPT, EgressStep.WATERMARK, EgressStep.ATTACH}
)


class DocumentEgress(Struct, frozen=True):
    step: EgressStep
    pdf: bytes
    params: dict[str, object]
    node: DocumentNode | None = None
    permissions: Permissions = Permissions()

    async def finish(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"egress.{self.step}", self._emit)

    async def _emit(self) -> ContentKey:
        data = (
            await to_process.run_sync(
                _native_finish, self.step.value, self.pdf, self.params, self.permissions
            )
            if self.step in NATIVE
            else _pure_finish(self.step, self.pdf, self.params, self.node)
        )
        return ContentIdentity.of(f"egress-{self.step}", data)


def _pure_finish(
    step: EgressStep, payload: bytes, params: dict[str, object], node: DocumentNode | None
) -> bytes:
    match step:
        case EgressStep.OUTLINE:
            return _outline(payload, params, node)
        case EgressStep.IMPOSE:
            return _impose(payload, params)
        case _:
            assert_never(step)


def _outline(payload: bytes, params: dict[str, object], node: DocumentNode | None) -> bytes:
    from pypdf import PdfReader, PdfWriter

    writer = PdfWriter(clone_from=PdfReader(BytesIO(payload)))
    if node is not None:
        parents: dict[int, object] = {}
        for section in (n for n in walk(node) if isinstance(n, SectionNode)):
            title = "".join(run.text for run in section.heading)
            parent = parents.get(section.level - 1)
            parents[section.level] = writer.add_outline_item(
                title, section.meta.page, parent=parent
            )
    else:
        for title, page in params["bookmarks"]:
            writer.add_outline_item(title, page)
    sink = BytesIO()
    writer.write(sink)
    return sink.getvalue()


def _impose(payload: bytes, params: dict[str, object]) -> bytes:
    from pypdf import PdfReader, PdfWriter, Transformation

    reader = PdfReader(BytesIO(payload))
    across, down = params.get("across", 2), params.get("down", 1)
    width, height = params["sheet"]
    cell_w, cell_h = width / across, height / down
    writer = PdfWriter()
    slots = across * down
    for start in range(0, len(reader.pages), slots):
        sheet = writer.add_blank_page(width=width, height=height)
        for offset, source in enumerate(reader.pages[start : start + slots]):
            col, row = offset % across, offset // across
            placement = Transformation().scale(
                cell_w / source.mediabox.width, cell_h / source.mediabox.height
            ).translate(col * cell_w, (down - 1 - row) * cell_h)
            source.add_transformation(placement)
            sheet.merge_page(source)
    sink = BytesIO()
    writer.write(sink)
    return sink.getvalue()


def _native_finish(
    step: str, payload: bytes, params: dict[str, object], permissions: Permissions
) -> bytes:
    from io import BytesIO

    import pikepdf

    pdf = pikepdf.open(BytesIO(payload))
    sink = BytesIO()
    match EgressStep(step):
        case EgressStep.ENCRYPT:
            pdf.save(
                sink,
                linearize=True,
                encryption=pikepdf.Encryption(
                    owner=params["owner"],
                    user=params.get("user", ""),
                    R=6,
                    aes=True,
                    allow=permissions.to_pikepdf(),
                ),
            )
        case EgressStep.WATERMARK:
            stamp = pikepdf.open(BytesIO(params["stamp"]))
            mark = pikepdf.Page(stamp.pages[0]).as_form_xobject()
            under = params.get("under", False)
            for page in pdf.pages:
                target = pikepdf.Page(page)
                target.add_underlay(mark) if under else target.add_overlay(mark)
            pdf.save(sink, linearize=True)
        case EgressStep.ATTACH:
            pdf.attachments[params["name"]] = pikepdf.AttachedFileSpec(
                pdf, params["data"], description=params.get("description", "")
            )
            pdf.save(sink, linearize=True)
        case _:
            assert_never(step)
    return sink.getvalue()
```

## [3]-[RESEARCH]

- [SEAM_CROSSING]: pikepdf is the qpdf-backed native wheel reflecting on the gated `python_version<'3.15'` band (catalogue header `10.8.0 reflected ... on the gated python_version<'3.15'> band (cp313)`) and weasyprint loads native `libpango`/`libgobject`/`libcairo` through cffi at import time (catalogue topology line), so the `ENCRYPT`/`WATERMARK`/`ATTACH` arms and the weasyprint outline lowering cross `anyio.to_process.run_sync` onto the `_native_finish` worker, importing `pikepdf` at module scope inside it — neither resolves on the cp315 core. pypdf is pure-Python (catalogue header `6.13.2 reflected ... on cp315`), so the `OUTLINE`/`IMPOSE` arms run in-process on the core; the `import pypdf` and `import pikepdf` calls sit at boundary scope per the manifest import policy, never module-top.
- [PIKEPDF_SPELLINGS]: the `pikepdf.open(src, *, attempt_recovery=True)`, `Pdf.save(target, *, linearize=False, encryption=None)`, `Encryption(owner, user, R=6, aes=True, allow=Permissions(...))`, `Permissions` row policy, `Pdf.pages` mutable collection, `Page.add_overlay`/`add_underlay`/`as_form_xobject`, `AttachedFileSpec`, and `open_metadata` spellings verify against the folder `.api` catalogue for `pikepdf` (the encryption-axis line `Encryption(owner=..., user=..., R=6, aes=True, allow=Permissions(...)) is one policy object; permission flags are Permissions rows, never parallel boolean knobs` directly endorses the `Permissions` value object the `ENCRYPT` arm carries over scattered booleans, and `add_overlay`/`add_underlay`/`as_form_xobject` are the page-composition rows the `WATERMARK` arm folds). The `Permissions.to_pikepdf` projection maps the artifacts value-object fields onto the qpdf `extract`/`modify_other`/`modify_annotation`/`modify_assembly`/`modify_form`/`print_lowres`/`print_highres` flags, so the permission policy is one value the caller sets once, never a knob bag re-specified per save.
- [PYPDF_SPELLINGS]: the `PdfReader`/`PdfWriter(clone_from=...)`, `PdfWriter.add_outline_item`, `PageObject.add_transformation`/`merge_page`, and `Transformation().scale(...).translate(...)` spellings verify against the folder `.api` catalogue for `pypdf` (the page axis `PageObject is the single page owner; transform/merge/rotate/scale compose Transformation, never a per-operation page subtype` confirms the `IMPOSE` n-up fold composes the one `Transformation` algebra rather than a per-layout imposition type, and `PdfWriter.add_blank_page` is the imposition sheet target). The `IMPOSE` arm reduces source pages onto target sheets through the pure-Python writer with no native dependency, so it is the cp315-core arm; the `OUTLINE` arm folds the `folder:../model#NODE` `SectionNode` heading levels through `walk` into nested `add_outline_item` calls keyed by `meta.page`, recovering the bookmark hierarchy from the one semantic tree rather than re-parsing the emitted PDF.
- [WEASYPRINT_OUTLINE]: the `HTML.render(font_config=...)` two-phase path and `Document.make_bookmark_tree(scale=1, transform_pages=False)` outline tree verify against the folder `.api` catalogue for `weasyprint` (entry `[5]` `Document.make_bookmark_tree` `build the PDF outline tree`, the navigation entry the local-admission law names as the two-phase paging-and-bookmark path) — this is the fallback `OUTLINE` lowering when no `DocumentNode` tree is supplied, used when the source PDF was authored by the weasyprint HTML-CSS backend whose `Page.bookmarks` carry the heading structure; it crosses the subprocess seam with the other native arms. The pypdf `add_outline_item` `DocumentNode` fold is the primary path and the weasyprint bookmark tree the HTML-authored fallback, never two competing outline owners.
- [BOUNDARY_AGAINST_CONFORMANCE]: the egress close finishes structure and navigation only; PAdES cryptographic signing is owned by `folder:../../typography/conformance#CONFORM` `SIGN`/`AUDIT` (pyhanko), and PDF/A archival authoring by `folder:../document-plan#DOCUMENT` `PDF_TYPST` `pdf_standards` — neither is an `EgressStep`. The `ENCRYPT` arm is qpdf AES document encryption (a confidentiality control), not a digital signature; a signed-then-encrypted archival close is the `conformance#CONFORM` `SIGN` arm composing this owner's `ENCRYPT` output, the signature applied last so encryption never invalidates the byte-range. The `ArtifactReceipt.Egress` case owned at `folder:../../receipt/artifact-receipt#RECEIPT` carries the post-finish byte count, page count, encryption R level, outline depth, and overlay count, so the finish is one fact row in the shared receipt stream, never a per-step egress receipt.
