# [PY_ARTIFACTS_DOCUMENT_PLAN]

The document-emission and post-processing axis. `DocumentPlan` is ONE dispatch surface discriminating document mode over a backend-per-mode policy table: PDF authoring (reportlab canvas/flowables, weasyprint HTML-CSS), PDF render/raster (pymupdf, pypdfium2), PDF assembly (pypdf), PDF repair/linearize (pikepdf), Office (python-docx/python-pptx/openpyxl), and structured-text (lxml/ruamel-yaml/tomlkit) — never parallel rails. Every production returns a `RuntimeRail[ContentKey]` keyed by the runtime content key and contributes one `ArtifactReceipt` row.

## [1]-[INDEX]

[CLUSTERS]: one cluster — `[2]-[DOCUMENT]`, the document-mode dispatch axis with one backend-per-mode policy row.

## [2]-[DOCUMENT]

- Owner: `DocumentPlan` the one dispatch axis discriminating mode; `DocumentMode` the closed `StrEnum` of emission and post-processing modes; `BACKENDS` the policy table binding each mode to the callable that drives its owning package.
- Cases: `DocumentMode` rows `PDF_AUTHOR` (reportlab) · `PDF_HTML` (weasyprint) · `PDF_RENDER` (pymupdf) · `PDF_RASTER` (pypdfium2) · `PDF_ASSEMBLE` (pypdf) · `PDF_REPAIR` (pikepdf) · `DOCX` (python-docx) · `PPTX` (python-pptx) · `XLSX` (openpyxl) · `XML` (lxml) · `YAML` (ruamel-yaml) · `TOML` (tomlkit) — each backend a row in the `BACKENDS` table, never an `if pdf` branch.
- Entry: `DocumentPlan.produce` is `async` over the runtime `async_boundary`, dispatching the mode inside the one fault capsule — a `BACKENDS` lookup driving the bound cp315-core emitter, or the `GATED` `XML` arm crossing `anyio.to_process.run_sync` onto the gated-band `_gated_emit` worker — and returns a `RuntimeRail[ContentKey]` keyed by the content key.
- Auto: PDF authoring runs reportlab `Canvas`/flowable build; HTML-CSS runs weasyprint `HTML.write_pdf`; render runs pymupdf `Document`/`Page.get_pixmap`/`Document.tobytes`; raster runs pypdfium2 `PdfDocument`; assembly runs pypdf `PdfWriter`; repair runs pikepdf `Pdf.save(linearize=True)`; Office runs the python-docx/python-pptx/openpyxl document roots; structured-text runs ruamel-yaml `YAML().dump` and tomlkit `dumps` on the core, with lxml `etree.tostring` on the gated `_gated_emit` worker.
- Receipt: each production contributes `receipt/artifact-receipt#RECEIPT` `ArtifactReceipt.Pdf`/`.Office`/`.Document` through the runtime `ReceiptContributor`, keyed by the content key.
- Packages: `reportlab`, `weasyprint`, `pymupdf`, `pypdfium2`, `pypdf`, `pikepdf`, `python-docx`, `python-pptx`, `openpyxl`, `ruamel-yaml`, `tomlkit` on the cp315 core; `lxml` gated `python_version<'3.15'`; runtime (`content_identity.ContentKey`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` (the runtime subprocess lane) for the gated `XML` arm).
- Growth: a new document format is one `DocumentMode` row plus one `BACKENDS` policy entry; a new PDF post-step is one mode row; zero new surface.
- Boundary: no durable document store, no hand-rolled PDF/XML parser, no UI. A per-format emitter-class service family and a stringly-typed `if mode == ...` branch are the deleted forms. The `XML` (lxml) emitter rides the gated `python_version<'3.15'` band and dispatches onto the runtime subprocess lane (`anyio.to_process.run_sync`); every other mode resolves on the cp315 core. Signed/archival/font-embedded close rides `typography/conformance#CONFORM`; report composition rides `reporting/report-plan#REPORT`; the content key is consumed from runtime, never re-minted.

```python signature
from builtins import frozendict
from collections.abc import Callable
from enum import StrEnum
from typing import Final

from anyio import to_process
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary


class DocumentMode(StrEnum):
    PDF_AUTHOR = "pdf-author"
    PDF_HTML = "pdf-html"
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

type Emitter = Callable[[dict[str, object]], bytes]

BACKENDS: Final[frozendict[DocumentMode, Emitter]] = frozendict({
    DocumentMode.PDF_AUTHOR: _reportlab_author,
    DocumentMode.PDF_HTML: _weasyprint_html,
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


class DocumentPlan(Struct, frozen=True):
    mode: DocumentMode
    payload: dict[str, object]

    async def produce(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"document.{self.mode}", self._emit)

    async def _emit(self) -> ContentKey:
        data = (
            await to_process.run_sync(_gated_emit, self.mode.value, self.payload)
            if self.mode in GATED
            else BACKENDS[self.mode](self.payload)
        )
        return ContentIdentity.of(self.mode.value, data)
```

## [3]-[RESEARCH]

- [EMITTER_BODIES]: each `BACKENDS` emitter binds to its package call on the cp315 core — reportlab `Canvas`/flowable build, weasyprint `HTML.write_pdf`, pymupdf `Page.get_pixmap`/`Document.tobytes`, pypdfium2 `PdfDocument`, pypdf `PdfWriter`, pikepdf `Pdf.save(linearize=True)`, python-docx/python-pptx `Document`/`Presentation`, openpyxl `Workbook`, ruamel-yaml `YAML().dump`, tomlkit `dumps`. The gated `XML` mode never enters `BACKENDS`: `_gated_emit` runs on the `python_version<'3.15'` band through `anyio.to_process.run_sync`, dispatches the `XML` mode value, and runs the `lxml` `etree.tostring` arm importing `lxml.etree` at module scope inside the gated-band worker. The cp315-core dists resolve in the active lock; the reportlab/weasyprint/pymupdf/pikepdf/python-pptx member capture is pending the pillow image-toolchain reflection pass, and the gated `lxml` spellings verify once the gated band installs.
