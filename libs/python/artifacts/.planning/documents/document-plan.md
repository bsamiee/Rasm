# [PY_ARTIFACTS_DOCUMENT_PLAN]

The document-emission and post-processing axis. `DocumentPlan` is ONE dispatch surface discriminating document mode over a backend-per-mode policy table: PDF authoring (reportlab canvas/flowables, weasyprint HTML-CSS), PDF render/raster (pymupdf, pypdfium2), PDF assembly (pypdf), PDF repair/linearize (pikepdf), Office (python-docx/python-pptx/openpyxl), and structured-text (lxml/ruamel-yaml/tomlkit) — never parallel rails. Every production returns a `RuntimeRail[ContentKey]` keyed by the runtime content key and contributes one `ArtifactReceipt` row.

## [1]-[INDEX]

[CLUSTERS]: one cluster — `[2]-[DOCUMENT]`, the document-mode dispatch axis with one backend-per-mode policy row.

## [2]-[DOCUMENT]

- Owner: `DocumentPlan` the one dispatch axis discriminating mode; `DocumentMode` the closed `StrEnum` of emission and post-processing modes; `BACKENDS` the policy table binding each mode to the callable that drives its owning package.
- Cases: `DocumentMode` rows `PDF_AUTHOR` (reportlab) · `PDF_HTML` (weasyprint) · `PDF_RENDER` (pymupdf) · `PDF_RASTER` (pypdfium2) · `PDF_ASSEMBLE` (pypdf) · `PDF_REPAIR` (pikepdf) · `DOCX` (python-docx) · `PPTX` (python-pptx) · `XLSX` (openpyxl) · `XML` (lxml) · `YAML` (ruamel-yaml) · `TOML` (tomlkit) — each backend a row in the `BACKENDS` table, never an `if pdf` branch.
- Entry: `DocumentPlan.produce` looks the mode up in `BACKENDS`, calls the bound emitter over the payload, and returns a `RuntimeRail[ContentKey]` keyed by the content key inside the one boundary capsule.
- Auto: PDF authoring runs reportlab `Canvas`/flowable build; HTML-CSS runs weasyprint `HTML.write_pdf`; render runs pymupdf `Document`/`Page.get_pixmap`/`Document.tobytes`; raster runs pypdfium2 `PdfDocument`; assembly runs pypdf `PdfWriter`; repair runs pikepdf `Pdf.save(linearize=True)`; Office runs the python-docx/python-pptx/openpyxl document roots; structured-text runs lxml `etree`, ruamel-yaml `YAML().dump`, tomlkit `dumps`.
- Receipt: each production contributes `receipt/artifact-receipt#RECEIPT` `ArtifactReceipt.Pdf`/`.Office`/`.Document` through the runtime `ReceiptContributor`, keyed by the content key.
- Packages: `reportlab`, `weasyprint`, `pymupdf`, `pypdfium2`, `pypdf`, `pikepdf`, `python-docx`, `python-pptx`, `openpyxl`, `lxml`, `ruamel-yaml`, `tomlkit`, runtime (`content_identity.ContentKey`, `faults.RuntimeRail`/`boundary`).
- Growth: a new document format is one `DocumentMode` row plus one `BACKENDS` policy entry; a new PDF post-step is one mode row; zero new surface.
- Boundary: no durable document store, no hand-rolled PDF/XML parser, no UI. A per-format emitter-class service family and a stringly-typed `if mode == ...` branch are the deleted forms. Signed/archival/font-embedded close rides `typography/conformance#CONFORM`; report composition rides `reporting/report-plan#REPORT`; the content key is consumed from runtime, never re-minted.

```python signature
from collections.abc import Callable
from enum import StrEnum
from typing import Final

from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary


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


type Emitter = Callable[[dict[str, object]], bytes]

BACKENDS: Final[dict[DocumentMode, Emitter]] = {
    DocumentMode.PDF_AUTHOR: _reportlab_author,
    DocumentMode.PDF_HTML: _weasyprint_html,
    DocumentMode.PDF_RENDER: _pymupdf_render,
    DocumentMode.PDF_RASTER: _pypdfium2_raster,
    DocumentMode.PDF_ASSEMBLE: _pypdf_assemble,
    DocumentMode.PDF_REPAIR: _pikepdf_repair,
    DocumentMode.DOCX: _docx_emit,
    DocumentMode.PPTX: _pptx_emit,
    DocumentMode.XLSX: _xlsx_emit,
    DocumentMode.XML: _lxml_emit,
    DocumentMode.YAML: _ruamel_emit,
    DocumentMode.TOML: _tomlkit_emit,
}


class DocumentPlan(Struct, frozen=True):
    mode: DocumentMode
    payload: dict[str, object]

    def produce(self) -> RuntimeRail[ContentKey]:
        return boundary(f"document.{self.mode}", self._emit)

    def _emit(self) -> ContentKey:
        data = BACKENDS[self.mode](self.payload)
        return ContentIdentity.key(self.mode.value, data)
```

## [3]-[RESEARCH]

- [EMITTER_BODIES]: each `BACKENDS` emitter binds to its package call — reportlab `Canvas`/flowable build, weasyprint `HTML.write_pdf`, pymupdf `Page.get_pixmap`/`Document.tobytes`, pypdfium2 `PdfDocument`, pypdf `PdfWriter`, pikepdf `Pdf.save(linearize=True)`, python-docx/python-pptx `Document`/`Presentation`, openpyxl `Workbook`, lxml `etree.tostring`, ruamel-yaml `YAML().dump`, tomlkit `dumps`. The bodies resolve against the branch `.api` catalogue once the pillow image toolchain installs and the pillow-dependent distributions re-reflect.
