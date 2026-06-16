# [PY_ARTIFACTS_DOCUMENTS]

The document/PDF/Office/structured-text plan, report templating, and the artifact receipt. `DocumentPlan` is ONE dispatch axis with backend-per-mode policy rows collapsing 6 PDF backends, 3 Office backends, and 3 structured-text backends — never parallel rails. `ReportPlan` is the report-templating composition over jinja2: the bind layer between VisualSpec outputs and DocumentPlan inputs that neither owns alone. `ArtifactReceipt` is one kind-discriminated receipt family keyed by runtime `ContentIdentity` and wired through `ReceiptContributor`.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                          |
| :-----: | :-------- | :------------------------------------------------------------- |
|   [1]   | DOCUMENT  | the document plan dispatch axis with backend rows              |
|   [2]   | REPORT    | the report-templating composition owner                        |
|   [3]   | RECEIPT   | the kind-discriminated artifact receipt family                 |

## [2]-[DOCUMENT]

- Owner: `DocumentPlan` — the one dispatch axis discriminating document mode; `DocumentBackend` the policy table mapping each mode to its backend package; `DocumentMode` the closed `StrEnum`.
- Cases: `DocumentMode` rows `PDF_RENDER`/`PDF_EDIT`/`PDF_REPAIR`/`PDF_EXTRACT` (pymupdf/pypdf/pikepdf/pypdfium2/reportlab/weasyprint) · `DOCX`/`PPTX`/`XLSX` (python-docx/python-pptx/openpyxl) · `XML`/`YAML`/`TOML` (lxml/ruamel-yaml/tomlkit) — matched by `match`/`case`, each binding the backend that owns it.
- Entry: `DocumentPlan.produce` dispatches the mode through its backend row and returns a `RuntimeRail[bytes]` keyed by `ContentIdentity`; the backend is a row column, never an `if (pdf)` branch.
- Auto: PDF render runs pymupdf `Document`/`Page`/`Pixmap`; PDF assembly runs pypdf `PdfWriter`; PDF repair runs pikepdf; the Office modes run the python-docx/python-pptx/openpyxl document roots; structured-text runs lxml `etree`/ruamel-yaml `YAML`/tomlkit `TOMLDocument`.
- Receipt: each production contributes an `ArtifactReceipt` row through `ReceiptContributor` keyed by `ContentIdentity`.
- Packages: `pymupdf`, `pypdf`, `pikepdf`, `pypdfium2`, `reportlab`, `weasyprint`, `python-docx`, `python-pptx`, `openpyxl`, `lxml`, `ruamel-yaml`, `tomlkit`, runtime (`RuntimeRail`/`ContentIdentity`/`ReceiptContributor`).
- Growth: a new document format is one `DocumentMode` row plus one `DocumentBackend` policy entry; zero new surface, no parallel codec rail.
- Boundary: no durable document store, no hand-rolled parser; a `GltfImporter`-class per-format service family and a stringly-typed mode branch are the deleted forms. `SPIKE` where the backend rides the pillow-blocked set.

```python signature
from enum import StrEnum
from typing import Final

from msgspec import Struct

from rasm.runtime.rails_resilience import RuntimeRail, boundary


class DocumentMode(StrEnum):
    PDF_RENDER = "pdf-render"
    PDF_EDIT = "pdf-edit"
    PDF_REPAIR = "pdf-repair"
    PDF_EXTRACT = "pdf-extract"
    DOCX = "docx"
    PPTX = "pptx"
    XLSX = "xlsx"
    XML = "xml"
    YAML = "yaml"
    TOML = "toml"


class DocumentBackend(Struct, frozen=True):
    mode: DocumentMode
    package: str


BACKENDS: Final[dict[DocumentMode, DocumentBackend]] = {
    DocumentMode.PDF_RENDER: DocumentBackend(DocumentMode.PDF_RENDER, "pymupdf"),
    DocumentMode.PDF_EDIT: DocumentBackend(DocumentMode.PDF_EDIT, "pypdf"),
    DocumentMode.PDF_REPAIR: DocumentBackend(DocumentMode.PDF_REPAIR, "pikepdf"),
    DocumentMode.DOCX: DocumentBackend(DocumentMode.DOCX, "python-docx"),
    DocumentMode.XLSX: DocumentBackend(DocumentMode.XLSX, "openpyxl"),
    DocumentMode.YAML: DocumentBackend(DocumentMode.YAML, "ruamel-yaml"),
    DocumentMode.TOML: DocumentBackend(DocumentMode.TOML, "tomlkit"),
}


class DocumentPlan(Struct, frozen=True):
    mode: DocumentMode
    payload: dict[str, object]

    def produce(self) -> "RuntimeRail[bytes]":
        return boundary(f"document.{self.mode}", lambda: _render(BACKENDS[self.mode], self.payload))
```

## [3]-[REPORT]

- Owner: `ReportPlan` — the templated document composition over jinja2: parameterized sections, data + figure binding into a document tree, multi-page layout, TOC/cross-references — the bind layer between VisualSpec outputs and DocumentPlan inputs.
- Entry: `ReportPlan.render` binds the section data and the figure exports into a jinja2 template tree and returns the rendered document feeding `DocumentPlan.produce`; the template environment is one `jinja2.Environment` with the section loader.
- Packages: `jinja2` (`Environment`/`Template`/`select_autoescape`), runtime (`RuntimeRail`).
- Growth: a new section kind is one template macro; zero new surface.
- Boundary: a hand-rolled section-composition loop is the deleted form; the report binds data into a template tree and hands it to `DocumentPlan`, owning neither the visual nor the codec.

```python signature
from jinja2 import Environment, select_autoescape
from msgspec import Struct

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.rails_resilience import RuntimeRail, boundary


class ReportPlan(Struct, frozen=True):
    template: str
    sections: tuple[dict[str, object], ...]
    figures: tuple[ContentKey, ...]

    def render(self, env: Environment) -> "RuntimeRail[str]":
        return boundary("report.render", lambda: env.from_string(self.template).render(sections=self.sections, figures=self.figures))


def report_env() -> Environment:
    return Environment(autoescape=select_autoescape(("html", "xml")), trim_blocks=True, lstrip_blocks=True)
```

## [4]-[RECEIPT]

- Owner: `ArtifactReceipt` — the one kind-discriminated receipt family replacing scattered per-type receipts; wired through runtime `ReceiptContributor`, keyed by runtime `ContentIdentity`.
- Cases: `ArtifactReceipt` cases `Document` · `Pdf` · `Office` · `Report` · `Preview` · `Export` — each a frozen `case()` carrying the content-key, byte count, and mode metadata.
- Entry: `ArtifactReceipt.of` builds the row from the mode/content-key/byte-count; `contribute` projects it onto the runtime `Receipt.emitted` stream.
- Packages: `msgspec`, runtime (`ReceiptContributor`/`ContentKey`).
- Growth: a new artifact kind is one `ArtifactReceipt` case; zero new surface.
- Boundary: a per-type `DocumentReceipt`/`PdfReceipt`/`OfficeReceipt` family is the deleted form; this owner is FINALIZED (only the runtime port and msgspec).

```python signature
from typing import Literal

from expression import case, tag, tagged_union

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.observability import Receipt


@tagged_union(frozen=True)
class ArtifactReceipt:
    tag: Literal["document", "pdf", "office", "report", "preview", "export"] = tag()
    document: tuple[ContentKey, int] = case()
    pdf: tuple[ContentKey, int, int] = case()
    office: tuple[ContentKey, int] = case()
    report: tuple[ContentKey, int] = case()
    preview: tuple[ContentKey, int, int] = case()
    export: tuple[ContentKey, str] = case()

    @staticmethod
    def Document(key: ContentKey, byte_count: int) -> "ArtifactReceipt":
        return ArtifactReceipt(document=(key, byte_count))

    @staticmethod
    def Pdf(key: ContentKey, byte_count: int, pages: int) -> "ArtifactReceipt":
        return ArtifactReceipt(pdf=(key, byte_count, pages))

    @staticmethod
    def Export(key: ContentKey, fmt: str) -> "ArtifactReceipt":
        return ArtifactReceipt(export=(key, fmt))

    def contribute(self) -> Receipt:
        return Receipt.Emitted("artifacts", self.tag, {})
```

## [5]-[RESEARCH]

- [BACKEND_SPELLINGS]: the pymupdf `Document.tobytes`/`Page.get_pixmap`, pypdf `PdfWriter`, ruamel-yaml `YAML().dump`, tomlkit `dumps`, and jinja2 `Environment.from_string` spellings are verified against the cp315-reflected `.api/api-pymupdf.md`, `.api/api-pypdf.md`, `.api/api-ruamel-yaml.md`, `.api/api-tomlkit.md`, `.api/api-jinja2.md`; the pikepdf/reportlab/weasyprint/python-pptx/matplotlib spellings confirm once `pillow` installs (suite TASKLOG `PY_API_004`).
