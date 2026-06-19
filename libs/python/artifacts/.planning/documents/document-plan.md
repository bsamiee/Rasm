# [PY_ARTIFACTS_DOCUMENT_PLAN]

The document-emission and post-processing axis, rebased onto the one `DocumentNode` semantic tree. `DocumentPlan` is ONE dispatch surface discriminating document mode over a backend-per-mode policy table whose every emitter is a LOWERING ARM folding FROM the `documents/model#NODE` `DocumentNode` tree, never an opaque payload: PDF authoring (reportlab canvas/flowables, weasyprint HTML-CSS, typst markup typesetting with PDF/A standard selection plus the held-`Compiler` introspection and `sys.inputs` data-binding rows), PDF render/raster (pymupdf, pypdfium2), PDF assembly (pypdf), PDF repair/linearize (pikepdf), Office (python-docx/python-pptx/openpyxl), and structured-text (lxml/ruamel-yaml/tomlkit) — never parallel rails. Emission lowers FROM the tree and `documents/inspection/lens#LENS` recovers TO it, so production and extraction are inverses over the one node algebra defined once at `documents/model#NODE`. Every production returns a `RuntimeRail[ContentKey]` keyed by the runtime content key and contributes one `ArtifactReceipt` row.

## [1]-[INDEX]

[CLUSTERS]: one cluster — `[2]-[DOCUMENT]`, the document-mode dispatch axis whose backend-per-mode policy rows lower FROM `DocumentNode`, carrying the Typst introspection-and-data-bind rows on the held `Compiler` world.

## [2]-[DOCUMENT]

- Owner: `DocumentPlan` the one dispatch axis discriminating mode; `DocumentMode` the closed `StrEnum` of emission, introspection, and post-processing modes; `BACKENDS` the policy table binding each mode to the lowering arm that folds the `DocumentNode` tree through its owning package; `_typst_world` the held `typst.Compiler` font-cached world the owner amortizes across a batch.
- Cases: `DocumentMode` rows `PDF_AUTHOR` (reportlab) · `PDF_HTML` (weasyprint) · `PDF_TYPST` (typst markup-to-PDF with `pdf_standards` PDF/A selection) · `TYPST_QUERY` (typst `Compiler.query(selector, field, one)` structured element extraction — the document-introspection inverse) · `TYPST_DATA` (typst `Compiler.compile(sys_inputs=...)` runtime data injection through `sys.inputs`, no string templating) · `PDF_RENDER` (pymupdf) · `PDF_RASTER` (pypdfium2) · `PDF_ASSEMBLE` (pypdf) · `PDF_REPAIR` (pikepdf) · `DOCX` (python-docx) · `PPTX` (python-pptx) · `XLSX` (openpyxl) · `XML` (lxml) · `YAML` (ruamel-yaml) · `TOML` (tomlkit) — each backend a row in the `BACKENDS` table, never an `if pdf` branch.
- Entry: `DocumentPlan.produce` is `async` over the runtime `async_boundary`, dispatching the mode inside the one fault capsule — a `BACKENDS` lookup driving the bound lowering arm over the `DocumentNode` tree (the `TYPST_QUERY`/`TYPST_DATA`/`PDF_TYPST` rows close over the held `Compiler` world), or the `GATED` `XML` arm crossing `anyio.to_process.run_sync` onto the gated-band `_gated_emit` worker — and returns a `RuntimeRail[ContentKey]` keyed by the content key.
- Auto: every arm receives the `DocumentNode` tree and lowers it — PDF authoring folds the tree through reportlab `Canvas`/flowable build; HTML-CSS folds it through weasyprint `HTML.write_pdf`; Typst typesetting folds it through `Compiler.compile(output=None, pdf_standards=...)` returning PDF bytes; `TYPST_QUERY` folds the selector through `Compiler.query(selector, field, one)` returning the structured element data; `TYPST_DATA` folds the injected data through `Compiler.compile(sys_inputs=...)`; render runs pymupdf `Document`/`Page.get_pixmap`/`Document.tobytes`; raster runs pypdfium2 `PdfDocument`; assembly runs pypdf `PdfWriter`; repair runs pikepdf `Pdf.save(linearize=True)`; Office folds the tree through the python-docx/python-pptx/openpyxl document roots; structured-text folds it through ruamel-yaml `YAML().dump` and tomlkit `dumps` on the core, with lxml `etree.tostring` on the gated `_gated_emit` worker.
- Receipt: each production contributes `receipt/artifact-receipt#RECEIPT` `ArtifactReceipt.Pdf`/`.Office`/`.Document` through the runtime `ReceiptContributor`, keyed by the content key; the `TYPST_QUERY` introspection row contributes `ArtifactReceipt.Document` carrying the queried-element byte length, and `TYPST_DATA` contributes `ArtifactReceipt.Pdf`.
- Packages: `reportlab`, `weasyprint`, `typst` (`Compiler`/`Compiler.compile`/`Compiler.query`/`Compiler.eval`, `compile`/`query`/`eval` with `sys_inputs`/`pdf_standards`, `Fonts`/`FontInfo`, cp38-abi3 wheel installing on the cp315 core), `pymupdf`, `pypdfium2`, `pypdf`, `pikepdf`, `python-docx`, `python-pptx`, `openpyxl`, `ruamel-yaml`, `tomlkit` on the cp315 core; `lxml` gated `python_version<'3.15'`; `documents/model#NODE` (`DocumentNode` the lowering source); runtime (`content_identity.ContentKey`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` (the runtime subprocess lane) for the gated `XML` arm).
- Growth: a new document format is one `DocumentMode` row plus one `BACKENDS` lowering arm; a new PDF post-step or Typst introspection row is one mode row; the held `Compiler` amortizes font load across a batched render of many documents, never a fresh world per document; zero new surface.
- Boundary: no durable document store, no hand-rolled PDF/XML parser, no UI. A per-format emitter-class service family, a stringly-typed `if mode == ...` branch, and an opaque `dict[str, object]` payload with no interior representation are the deleted forms — every arm lowers the `documents/model#NODE` tree instead. The `XML` (lxml) emitter rides the gated `python_version<'3.15'` band and dispatches onto the runtime subprocess lane (`anyio.to_process.run_sync`); every other mode resolves on the cp315 core. The `PDF_TYPST` `pdf_standards` row authors PDF/A structural conformance at emission; PAdES cryptographic signing is never a typst row and routes to `typography/conformance#CONFORM`, which also owns the signed/archival/font-embedded close; the security-and-navigation finishing close (encryption/outline/watermark/attach/impose) routes to `documents/egress/finish#FINISH`; the recover-TO extraction half routes to `documents/inspection/lens#LENS`; report composition rides `reporting/report-plan#REPORT`; the content key is consumed from runtime, never re-minted.

```python signature
from collections.abc import Callable
from enum import StrEnum
from types import MappingProxyType
from typing import Final

from anyio import to_process
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.documents.model import DocumentNode, to_typst_source


class DocumentMode(StrEnum):
    PDF_AUTHOR = "pdf-author"
    PDF_HTML = "pdf-html"
    PDF_TYPST = "pdf-typst"
    TYPST_QUERY = "typst-query"
    TYPST_DATA = "typst-data"
    PDF_RENDER = "pdf-render"
    PDF_RASTER = "pdf-raster"
    PDF_ASSEMBLE = "pdf-assemble"
    PDF_REPAIR = "pdf-repair"
    DOCX = "docx"
    PPTX = "pptx"
    XLSX = "xlsx"
    XML = "xml"
    YAML = "yaml"
    TOML = "toml"


GATED: frozenset[DocumentMode] = frozenset({DocumentMode.XML})

type Emitter = Callable[["DocumentPlan", DocumentNode], bytes]

BACKENDS: Final[MappingProxyType[DocumentMode, Emitter]] = MappingProxyType({
    DocumentMode.PDF_AUTHOR: _reportlab_author,
    DocumentMode.PDF_HTML: _weasyprint_html,
    DocumentMode.PDF_TYPST: _typst_compile,
    DocumentMode.TYPST_QUERY: _typst_query,
    DocumentMode.TYPST_DATA: _typst_data,
    DocumentMode.PDF_RENDER: _pymupdf_render,
    DocumentMode.PDF_RASTER: _pypdfium2_raster,
    DocumentMode.PDF_ASSEMBLE: _pypdf_assemble,
    DocumentMode.PDF_REPAIR: _pikepdf_repair,
    DocumentMode.DOCX: _docx_emit,
    DocumentMode.PPTX: _pptx_emit,
    DocumentMode.XLSX: _xlsx_emit,
    DocumentMode.YAML: _ruamel_emit,
    DocumentMode.TOML: _tomlkit_emit,
})


def _typst_compile(plan: "DocumentPlan", node: DocumentNode) -> bytes:
    return plan.typst_world().compile(
        input=to_typst_source(node),
        output=None,
        sys_inputs=plan.params.get("sys_inputs", {}),
        pdf_standards=plan.params.get("pdf_standards"),
    )


def _typst_data(plan: "DocumentPlan", node: DocumentNode) -> bytes:
    return plan.typst_world().compile(
        input=to_typst_source(node),
        output=None,
        sys_inputs=plan.params["sys_inputs"],
        pdf_standards=plan.params.get("pdf_standards"),
    )


def _typst_query(plan: "DocumentPlan", node: DocumentNode) -> bytes:
    return plan.typst_world().query(
        selector=plan.params["selector"],
        field=plan.params.get("field"),
        one=plan.params.get("one", False),
    ).encode()


class DocumentPlan(Struct, frozen=True):
    mode: DocumentMode
    node: DocumentNode
    params: dict[str, object]

    def typst_world(self) -> object:
        from typst import Compiler

        return Compiler(
            input=self.params.get("source"),
            font_paths=self.params.get("font_paths", []),
            sys_inputs=self.params.get("sys_inputs", {}),
        )

    async def produce(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"document.{self.mode}", self._emit)

    async def _emit(self) -> ContentKey:
        data = (
            await to_process.run_sync(_gated_emit, self.mode.value, self.node)
            if self.mode in GATED
            else BACKENDS[self.mode](self, self.node)
        )
        return ContentIdentity.of(self.mode.value, data)
```

## [3]-[RESEARCH]

- [LOWERING_BODIES]: each `BACKENDS` arm is a lowering fold FROM the `documents/model#NODE` `DocumentNode` tree on the cp315 core — reportlab `Canvas`/flowable build over the tree's `page`/`block`/`run` nodes, weasyprint `HTML.write_pdf` over the tree serialized to HTML, typst `Compiler.compile(input=to_typst_source(node), output=None, pdf_standards=...)` (the `PDF_TYPST` arm verified against the folder `.api` catalogue for `typst`; `output=None` returns PDF `bytes`, `pdf_standards` selects the PDF/A archival target, `sys_inputs` binds runtime data through `sys.inputs`), pymupdf `Page.get_pixmap`/`Document.tobytes`, pypdfium2 `PdfDocument`, pypdf `PdfWriter`, pikepdf `Pdf.save(linearize=True)`, python-docx/python-pptx `Document`/`Presentation` over the tree, openpyxl `Workbook`, ruamel-yaml `YAML().dump`, tomlkit `dumps`. The `to_typst_source(node)` projection is the module-level lowering defined once on the `DocumentNode` tree at `documents/model#NODE` (the tree is a `type` union alias, so the lowering is a free function over the tree, never a method on the alias) so the three Typst rows share one lowering, never three string templates. The gated `XML` mode never enters `BACKENDS`: `_gated_emit` runs on the `python_version<'3.15'` band through `anyio.to_process.run_sync`, dispatches the `XML` mode value over the tree, and runs the `lxml` `etree.tostring` arm importing `lxml.etree` at module scope inside the gated-band worker.
- [TYPST_INTROSPECTION]: the held `typst.Compiler` world is the reusable font-cached compiler the owner amortizes across a batched render — `typst_world()` constructs one `Compiler(input, font_paths, sys_inputs)` and the `PDF_TYPST`/`TYPST_DATA`/`TYPST_QUERY` rows call `Compiler.compile`/`Compiler.query` against it rather than the single-shot module `compile`, so font loading is paid once per batch (catalogue compiler axis: a fresh `Compiler` per render in a batch is the rejected form). `TYPST_QUERY` is the produce-then-introspect inverse: `Compiler.query(selector, field, one)` extracts structured element data (a TOC/index feeding `reporting/report-plan#REPORT`) parallel to the PDF document-node seam; `TYPST_DATA` injects runtime data through `sys_inputs` with no string templating. The `Compiler`/`Compiler.compile`/`Compiler.query`/`Compiler.eval`/`compile(sys_inputs, pdf_standards)`/`query(selector, field, one)`/`Fonts`/`FontInfo` spellings verify against the folder `.api` catalogue for `typst`.
- [SPELLINGS]: the cp315-core dists resolve in the active lock, and the reportlab `Canvas`/`SimpleDocTemplate`, weasyprint `HTML.write_pdf`, pymupdf `Page.get_pixmap`/`Document.tobytes`, pikepdf `Pdf.save(linearize=True)`, and python-pptx `Presentation` member spellings verify against the folder `.api` catalogues for each owning package; the gated `lxml` `etree.tostring` spellings verify against the `lxml` catalogue, reflected on the `python_version<'3.15'` band (no CPython 3.15 wheel), and never resolve on the cp315 core.
