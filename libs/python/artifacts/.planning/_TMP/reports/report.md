# [PY_ARTIFACTS_REPORT]

The reproducible-report composition layer binding data and visual outputs into the one `documents/model#NODE` `DocumentNode` tree. `ReportPlan` is ONE async owner spanning four report kinds on one `COMPOSE_ARMS`-table dispatch (the `BACKENDS`/`_CORE_ARMS` band-table idiom the `documents/emit#DOCUMENT` and `documents/lens#LENS` siblings prove, every arm a coroutine row so `_emit` is one uniform `await COMPOSE_ARMS[self.kind](self)` with zero branch), and the canonical interior is the `DocumentNode` tree every kind PRODUCES — never a string: section composition (a `Section` value-object algebra folding heading level, body runs, and figure content-key refs into a `SectionNode`/`BlockNode`/`FigureNode` tree with a TOC `SectionNode` synthesized from the heading hierarchy), jinja2 template composition (a `ReportLoader`-row engine over the trusted/sandboxed `Environment` policy, strict-undefined async render emitting a free-form HTML `BlockNode` leaf), parameterized-notebook execution (a `ReportSource`-row `jupytext` text-source pairing into a `NotebookNode` — `guess_format`-driven on the `AUTO` row — a `msgspec.convert` declared-schema parameter gate the `schema_fields` introspection surface mirrors, a `papermill.parameterize_notebook` injection threading the `NotebookEngine.kernel_name` so a non-`python3` kernel routes its own `PapermillTranslators` row, a host-free `nbclient.NotebookClient.async_execute` run under the frozen `NotebookEngine` traits, an executed-notebook archive, and a `nbconvert.from_notebook_node` lowering whose `(output, resources)` pair feeds a `BlockNode` leaf — the `HTML_EXPORTS`-membered `str` output a `BlockKind.CODE` HTML leaf and the binary `PDF`/`WEBPDF` `bytes` output a `BlockKind.QUOTE` hex leaf, never an HTML wrapper over PDF bytes), and reflowable re-layout (the `pymupdf.Story`/`DocumentWriter` HTML-to-PDF reflow the `REFLOW` kind AUTHORS as a fresh `PageNode` plus the reflowed PDF bytes, the authoring inverse of the `documents/lens#LENS` recover-TO half — `LENS` recovers a tree OUT of an emitted PDF, this kind reflows HTML INTO a fresh PDF). Every kind reduces to one `DocumentNode` the `documents/emit#DOCUMENT` axis lowers and one `RuntimeRail[tuple[ContentKey, tuple[ArtifactReceipt, ...]]]` carries; the executed notebook keys as a second `ArtifactReceipt.Report` row and the reflowed PDF keys as the page-count-bearing `ArtifactReceipt.Pdf` row. It owns neither the visual nor the codec — it binds the figures keyed by content key into the tree (a `FigureNode` per `asset_key`, never re-rendering the chart/table/scene), executes a notebook under a bounded-safety engine, reflows HTML through the native MuPDF story loop, contributes each emitted artifact as a receipt row, and hands the tree to the document axis.

## [01]-[INDEX]

- [01]-[REPORT]: report-composition axis over the section/template/notebook/reflow kinds producing one `DocumentNode` interior through the one `COMPOSE_ARMS` coroutine-row dispatch table, the `Section` heading-and-figure-ref value object the `COMPOSE` kind folds into the tree, the `ParamSchema` `msgspec`-struct parameter gate plus the `schema_fields` declared-field introspection surface, the `NotebookEngine` bounded-safety engine-config value, the `ReportLoader` jinja2 loader-and-sandbox sub-axis, the `ReportSource` jupytext text-source sub-axis (the `guess_format` `AUTO` row included), the `ReportExport` closed `nbconvert` export-name vocabulary with its `HTML_EXPORTS` text-vs-binary output partition, and the `ReflowPaper` named-paper-size sub-axis the `pymupdf.Story` re-layout flows into and every kind's `media_box` derives from through `paper_rect`.

## [02]-[REPORT]

- Owner: `ReportPlan` the one async composition axis discriminating report kind through the one `COMPOSE_ARMS` coroutine-row dispatch table (`MappingProxyType[ReportKind, ComposeArm]`, every row a `Callable[[ReportPlan], Awaitable[tuple[PageNode, tuple[ArtifactReceipt, ...]]]]`, the `documents/emit#DOCUMENT` `BACKENDS`/`documents/lens#LENS` `_CORE_ARMS` band-table idiom) and producing one `DocumentNode` interior; `ReportKind` the closed `StrEnum` over `COMPOSE` (section-tree binding), `TEMPLATE` (jinja2 free-form HTML), `NOTEBOOK` (papermill/nbclient/nbconvert), and `REFLOW` (pymupdf `Story` HTML-to-PDF re-layout); `Section` the frozen value object carrying a heading level, body runs, and a `FigureRef` sequence the `COMPOSE` fold lowers into a `SectionNode`/`BlockNode`/`FigureNode` subtree; `FigureRef` the one figure-reference value object (`asset_key` content key, `alt` equivalent, `caption`) the `COMPOSE` and `TEMPLATE` kinds both bind, never a parallel content-key tuple beside it; `ParamSchema` the `msgspec.Struct(forbid_unknown_fields=True)` declared-parameter contract `msgspec.convert` gates the supplied parameters against before a kernel boots — the base rejects every unknown field and each caller subclass adds its required typed fields, so an empty bag against a field-bearing subclass and an unexpected key against any subclass both fault, the declared field set recoverable through the `schema_fields` -> `msgspec.structs.fields` introspection surface that replaces the path-reading `papermill.inspect_notebook`; `ReportLoader` the closed `StrEnum` selecting the jinja2 loader composition (`DictLoader`/`FileSystemLoader`/`PackageLoader`/`PrefixLoader`/`FunctionLoader`/`ModuleLoader` folded through `ChoiceLoader`) the `LOADERS` row table resolves; `ReportSource` the closed `StrEnum` selecting the `jupytext` text representation a notebook source admits before execution, with the `AUTO` row deferring to `jupytext.guess_format`; `ReportExport` the closed `StrEnum` over the `nbconvert.exporters` registry name rows the `get_exporter` call resolves, partitioned by the `HTML_EXPORTS` frozenset into the text-output rows whose `str` lowers to a `BlockKind.CODE` HTML leaf and the binary `PDF`/`WEBPDF` rows whose `bytes` lower to a `BlockKind.QUOTE` hex leaf; `NotebookEngine` the frozen engine-config value object carrying the `nbclient.NotebookClient` bounded-safety traits (`timeout`/`startup_timeout`/`allow_errors`/`force_raise_errors`/`record_timing`/`interrupt_on_timeout`/`coalesce_streams`/`store_widget_state`/`skip_cells_with_tag`/`kernel_name`), projected to constructor kwargs through `msgspec.structs.asdict`; `ReflowPaper` the closed `StrEnum` over the `pymupdf.paper_rect` named-size vocabulary every kind's `media_box` derives from through `_box` and the `REFLOW` loop flows pages into; the bind layer between visual outputs and document inputs.
- Cases: `_compose_tree` (the `COMPOSE` row) folds the `Section` value-object sequence into a `SectionNode` outline (each section's runs into a `BlockNode`, each figure ref into a `FigureNode` keyed by the bound `asset_key`), synthesizes a leading table-of-contents `SectionNode` from the heading hierarchy through `_toc`, and returns the assembled `PageNode` tree the document axis lowers; `_template_tree` (the `TEMPLATE` row) renders the `report_env(loader, trusted)` engine — a trusted `jinja2.Environment` or an untrusted `jinja2.sandbox.ImmutableSandboxedEnvironment` selected by the `trusted` policy, both under `REPORT_ENV_KWARGS` — over a `ReportLoader`-resolved loader, the section data, and the one `FigureRef` sequence the `COMPOSE` kind also binds, into an HTML string wrapped through `_html_page` as one `BlockNode(block=BlockKind.CODE)` HTML leaf the `documents/emit#DOCUMENT PDF_HTML` weasyprint arm consumes; `_notebook_tree` (the `NOTEBOOK` row) admits the source through the `ReportSource` jupytext row (`AUTO` resolving the `fmt` through `guess_format`) into one `nbformat.NotebookNode`, gates the supplied parameters against the `ParamSchema` `msgspec.Struct` through `msgspec.convert` (an absent required field or a type mismatch raising `msgspec.ValidationError` before a kernel boots), injects the parameter cell through `papermill.parameterize_notebook` over `papermill.add_builtin_parameters` threading `kernel_name=plan.engine.kernel_name` so the `PapermillTranslators` row matches the configured kernel language, executes the node headlessly through `nbclient.NotebookClient.async_execute` under the `NotebookEngine` traits, reads the `(output, resources)` pair from `nbconvert.from_notebook_node`, lowers the `str` output to a `BlockKind.CODE` HTML leaf or the binary `PDF`/`WEBPDF` `bytes` output to a `BlockKind.QUOTE` hex leaf through `_html_page`'s `HTML_EXPORTS`-keyed partition, and yields the `jupytext.writes(executed, "ipynb")` archive bytes as the audit artifact; `_reflow_tree` (the `REFLOW` row) flows the source HTML through one `pymupdf.Story(html, user_css, em, archive)`/`DocumentWriter(BytesIO)` `place`/`draw` loop over the `ReflowPaper`-resolved `paper_rect`, recovers the `BytesIO` buffer's reflowed PDF bytes plus the page count, and keys the bytes plus the page count as the `ArtifactReceipt.Pdf(key, byte_count, page_count)` row the page count lands on rather than a `Report` row that drops it, carrying the page count and rect on the returned `PageNode`. `ReportLoader` rows `DICT` (`DictLoader` in-memory sections) · `FILESYSTEM` (`FileSystemLoader` template roots) · `PACKAGE` (`PackageLoader` package-resource tree) · `PREFIX` (`PrefixLoader` namespaced child loaders) · `FUNCTION` (`FunctionLoader` callable source provider) · `MODULE` (`ModuleLoader` pre-compiled template tree) — each a loader-row value the catalogue loader axis declares, never a parallel engine per source. `ReportSource` rows `IPYNB` (`nbformat` round-trip) · `MYST` · `PERCENT` (`py:percent`) · `LIGHT` (`py:light`) · `MARKDOWN` · `RMD` (`Rmd`) · `QMD` (`qmd`) · `AUTO` (content-detected through `guess_format`) — each a `jupytext` `fmt` row, never a per-format reader. `ReportExport` rows `HTML` · `PDF` · `WEBPDF` · `LATEX` · `MARKDOWN` · `RST` · `ASCIIDOC` · `SLIDES` · `NOTEBOOK` · `PYTHON` · `SCRIPT` — each a `nbconvert.exporters` name `get_exporter` resolves, raising `ExporterNameError` on an unknown name, never a per-format convert function; the `HTML_EXPORTS` frozenset is the one membership predicate partitioning the text-output rows (HTML leaf) from the binary `PDF`/`WEBPDF` rows (hex `bytes` leaf), never a per-export wrapper branch. `ReflowPaper` rows `A4` · `A3` · `A5` · `LETTER` · `LEGAL` — each a `pymupdf.paper_rect` named-size string every kind's `media_box` derives from through `_box`, never a hand-built rect.
- Entry: `ReportPlan.render` is `async` over the runtime `async_boundary`, dispatching the kind inside the one fault capsule through `_emit`'s single `await COMPOSE_ARMS[self.kind](self)` table lookup (zero `match`, zero branch — the coroutine row is total over the four-kind `StrEnum`) and returning a `RuntimeRail[tuple[ContentKey, tuple[ArtifactReceipt, ...]]]` carrying the rendered tree's content key and the emitted-artifact receipt rows; the `COMPOSE` fold lowers the `Section` sequence into a `PageNode` through `_compose_tree`; the template environment is one `report_env(loader, trusted)` over `REPORT_ENV_KWARGS` (`undefined=StrictUndefined`, `enable_async=True`); the notebook run is one `nbclient.NotebookClient(node, **asdict(engine)).async_execute()` call applying the `NotebookEngine` traits; the reflow loop runs the gated `pymupdf.Story`/`DocumentWriter` on the runtime subprocess lane through `anyio.to_process.run_sync`; the boundary fault-funnels `jinja2.UndefinedError`/`TemplateSyntaxError`/`TemplateNotFound`/`sandbox.SecurityError`, `msgspec.ValidationError`, `papermill.PapermillMissingParameterException`/`PapermillExecutionError`, `nbclient.exceptions.CellExecutionError`/`CellTimeoutError`/`DeadKernelError`, and `nbconvert.ExporterNameError` through the boundary into `BoundaryFault`.
- Auto: the `COMPOSE` fold reduces the `Section` sequence into a `SectionNode` outline through `_compose_tree` — each section's body runs become a `BlockNode`, each `figure_ref` an `asset_key`-keyed `FigureNode` whose `alt` the section authors, and `_toc` synthesizes a leading table-of-contents `SectionNode` from the section heading levels and texts (a `BlockNode(block=BlockKind.LIST_ITEM)` per heading, the outline depth the heading `level` carries) so the bound figures land IN the tree rather than as a jinja kwarg; template binding folds the sections and the one `FigureRef` sequence into `Environment.from_string(...).render_async` (strict-undefined: a missing section key is a `jinja2.UndefinedError` fault, never a silent blank) and wraps the rendered HTML as one `BlockNode` HTML leaf; notebook execution admits the `ReportSource` text through `jupytext.reads(text, fmt)` (`AUTO` resolving `fmt` through `jupytext.guess_format(text, ".ipynb")` first), gates the supplied parameters through `msgspec.convert(parameters, self.schema)` against the `ParamSchema` declared-field contract (the in-memory gate `papermill.inspect_notebook` cannot serve because it reads a path, not a node — a missing required field or a wrong type raises `msgspec.ValidationError` before a kernel boots), injects parameters through `papermill.parameterize_notebook(node, papermill.add_builtin_parameters(asdict(gated)), report_mode=False, kernel_name=plan.engine.kernel_name)` (the one injection point `papermill` owns, the `kernel_name` selecting the `PapermillTranslators` row so an `ir`/`julia`/`xcsharp` kernel serializes its parameter cell in its own language rather than defaulting to Python), runs the node through `nbclient.NotebookClient(node, **asdict(engine)).async_execute()` on the async kernel lane (a runaway cell raising `CellTimeoutError`/`CellExecutionError`, a kernel death `DeadKernelError`, an unhandled cell fault `PapermillExecutionError`), serializes the executed node — whose per-cell timing `record_timing=True` already stamped into its metadata and whose widget state `store_widget_state=True` captured into the notebook metadata — to the reproducibility-archive bytes through `jupytext.writes(executed, "ipynb")`, and lowers the executed node through `nbconvert.get_exporter(export)().from_notebook_node(executed)` reading the `(output, resources)` pair: a `HTML_EXPORTS`-membered export's `str` output wraps as a `BlockKind.CODE` HTML leaf while a `PDF`/`WEBPDF` export's `bytes` output wraps as a `BlockKind.QUOTE` hex leaf, the `isinstance(output, str)` partition the `_html_page` row owns so a binary export never mis-renders as HTML text, the `resources` sidecar of extracted-output/extension keys staying the export-extraction channel; the `REFLOW` loop opens one `pymupdf.DocumentWriter(io.BytesIO())` over a held buffer, lays the HTML through `pymupdf.Story(html, user_css, em, archive).place(rect)` until `more == 0`, draws each filled slice onto the `begin_page(paper_rect)` device, closes the writer, and returns the buffer's reflowed-PDF bytes, the page count, and the rect; every kind's non-reflow `media_box` derives from `_box(plan)` over `pymupdf.paper_rect(ReflowPaper(...).value)`, never a hand-built `(0, 0, 595, 842)` literal.
- Receipt: each render contributes `receipt/receipt#RECEIPT` `ArtifactReceipt.Report(key, byte_count)` keyed by the content key over the encoded `DocumentNode` tree; the `NOTEBOOK` kind keys the `jupytext.writes(executed, "ipynb")` archive (carrying the per-cell timing `record_timing` and the widget state `store_widget_state` stamped into the executed node's metadata) through a second `ContentIdentity.of` as a second `ArtifactReceipt.Report` row, and the `REFLOW` kind keys the reflowed-PDF bytes through a second `ContentIdentity.of` as the page-count-bearing `ArtifactReceipt.Pdf(key, byte_count, page_count)` row (the receipt owner's three-field PDF case the reflowed PDF is — a `Report(key, byte_count)` row would silently drop the page count the reflow loop already counts), so the reproducibility artifact and the bound tree are two keyed receipt rows the one `RuntimeRail` payload carries onto the runtime `contribute` fold. The `ArtifactReceipt.Report` case is `(ContentKey, byte_count)` and the `ArtifactReceipt.Pdf` case is `(ContentKey, byte_count, page_count)` (both owned by `receipt/receipt#RECEIPT`); the resolved loader/source/export row, the autoescape decision, and the gated `ParamSchema` field set the `schema_fields` introspection surfaces are recoverable from the `ReportPlan` value the runtime keys, never a parallel fact map this page invents.
- Packages: `jinja2` (`Environment`/`sandbox.ImmutableSandboxedEnvironment`/`sandbox.SecurityError`/`StrictUndefined`/`select_autoescape`/`FileSystemLoader`/`DictLoader`/`PackageLoader`/`PrefixLoader`/`FunctionLoader`/`ModuleLoader`/`ChoiceLoader`/`Environment.from_string`/`Template.render_async`/`UndefinedError`/`TemplateSyntaxError`/`TemplateNotFound`), `papermill` (`parameterize_notebook(nb, parameters, report_mode, kernel_name)`/`add_builtin_parameters`/`PapermillTranslators` the kernel-language translator registry the `kernel_name` selects/`PapermillMissingParameterException`/`PapermillExecutionError`), `nbclient` (`NotebookClient`/`async_execute`, `CellExecutionError`/`CellTimeoutError`/`DeadKernelError`, the `timeout`/`startup_timeout`/`kernel_name`/`allow_errors`/`force_raise_errors`/`record_timing`/`interrupt_on_timeout`/`coalesce_streams`/`store_widget_state`/`skip_cells_with_tag` traits), `jupytext` (`reads`/`writes`/`guess_format`, `fmt` rows), `nbconvert` (`get_exporter`/`HTMLExporter`/`Exporter.from_notebook_node`/`get_export_names`/`ExporterNameError`), `nbformat` (`NotebookNode`), `pymupdf` (`Story(html, user_css, em, archive)`/`Story.place`/`Story.draw`/`DocumentWriter`/`DocumentWriter.begin_page`/`end_page`/`close`/`paper_rect`), `msgspec` (`Struct`/`field`/`structs.asdict`/`structs.fields`/`structs.FieldInfo`/`convert`/`ValidationError`/`json.Encoder`), runtime (`content_identity.ContentKey`/`ContentIdentity`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` the gated subprocess lane for `REFLOW`), `documents/model#NODE` (`DocumentNode`/`PageNode`/`SectionNode`/`BlockNode`/`RunNode`/`FigureNode`/`NodeMeta`/`BlockKind`/`encode`), `artifacts.receipt.receipt` (`ArtifactReceipt.Report`/`ArtifactReceipt.Pdf`).
- Growth: a new report kind is one `ReportKind` row plus one `COMPOSE_ARMS` coroutine-row producing a `DocumentNode`; a new section concept is one `Section` field plus one `_compose_tree` lowering arm; a new loader root is one `ReportLoader` row plus one `LOADERS` table arm; a new text source is one `ReportSource` row plus one `jupytext` `fmt` value; a new export target is one `ReportExport` row on the `nbconvert.exporters` registry plus its `HTML_EXPORTS` membership (text output) or absence (binary output); a new paper size is one `ReflowPaper` row on `paper_rect`; a new bounded-safety trait is one field on `NotebookEngine`, carried into the client by `asdict` with zero call-site edit; a new declared parameter is one `ParamSchema` field the `schema_fields` introspection surface picks up; zero new surface.
- Boundary: a string-returning report that never produces a node, a figure passed only as a jinja kwarg where the `COMPOSE` fold splices a `FigureNode` into the tree, a hand-rolled section-composition loop, an inline `match self.kind` ladder where the one `COMPOSE_ARMS` coroutine-row table dispatches, a lenient default `Environment` that blanks missing keys, a sync straggler render, an untrusted-template `Environment` with the sandbox disabled, a `papermill.inspect_notebook` parameter gate on a path where the in-memory node demands `msgspec.convert` against the `ParamSchema` (its declared-parameter introspection value carried instead by `schema_fields` over `msgspec.structs.fields`, not discarded), a bare `dict` parameter bag where the closed `ParamSchema` struct validates field types, a hand-rolled parameter injection outside `parameterize_notebook`, a Python-defaulted `parameterize_notebook` where the `kernel_name` routes the matching `PapermillTranslators` row, a bare `str` export knob where the closed `ReportExport` vocabulary resolves through `get_exporter`, a `BlockKind.CODE` HTML wrapper over the binary `PDF`/`WEBPDF` `bytes` output where the `HTML_EXPORTS` partition selects the hex `BlockKind.QUOTE` leaf, a path-based `papermill.execute_notebook` blocking the async boundary, a hand-rolled percent/myst cell parser, a hand-rolled format guess outside `guess_format`, a `subprocess` shell-out to the `jupyter-nbconvert` CLI, a parallel `tuple[ContentKey, ...]` figure channel beside the `FigureRef` value object, a discarded reflowed-PDF buffer where the bytes are the artifact, a hand-built `(0, 0, 595, 842)` media-box literal where `_box` derives the rect through `paper_rect`, an `ArtifactReceipt.Report` row on the reflowed PDF that drops the page count the `ArtifactReceipt.Pdf` case carries, and an empty `ParamSchema` base that admits any parameter bag are the deleted forms — the report binds `FigureRef` content keys produced by `figures/chart#CHART`, `figures/table#TABLE`, and `figures/scene#SCENE` (never re-rendering them) into the tree, parses the text source through `jupytext`, runs the node on the async `nbclient` kernel lane, exports the executed node in-process through `nbconvert`, and reflows HTML through the native `pymupdf.Story` loop into a keyed PDF artifact. The produced `DocumentNode` tree hands to `documents/emit#DOCUMENT` for lowering (`PDF_HTML` for the HTML leaves, `PDF_TYPST`/`PDF_AUTHOR` for the structured tree); PAdES/PDF-A finishing routes to `typography/conformance#CONFORM`; the security-and-navigation finishing close routes to `documents/egress#FINISH`; the `REFLOW` kind is this owner's HTML-INTO-PDF authoring half, the directional inverse of the `documents/lens#LENS` `STORY` recover-OUT-of-PDF extraction arm — authoring and extraction are two arms of the one `pymupdf.Story` capability split across the two owners by direction, not a duplicate; no live kernel server, no UI; the content key is consumed from runtime, never re-minted.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from __future__ import annotations

import io
from collections.abc import Awaitable, Callable, Mapping
from enum import StrEnum
from types import MappingProxyType
from typing import Final, assert_never

import jupytext
import nbconvert
import papermill
import pymupdf
from anyio import to_process
from jinja2 import (
    BaseLoader,
    ChoiceLoader,
    DictLoader,
    Environment,
    FileSystemLoader,
    FunctionLoader,
    ModuleLoader,
    PackageLoader,
    PrefixLoader,
    StrictUndefined,
    select_autoescape,
)
from jinja2.sandbox import ImmutableSandboxedEnvironment
from msgspec import Struct, convert, field
from msgspec.structs import FieldInfo, asdict, fields
from nbclient import NotebookClient
from nbformat import NotebookNode

from artifacts.documents.model import (
    BlockKind,
    BlockNode,
    DocumentNode,
    FigureNode,
    NodeMeta,
    PageNode,
    RunNode,
    SectionNode,
    encode,
)
from artifacts.receipt.receipt import ArtifactReceipt
from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

# --- [TYPES] ----------------------------------------------------------------------------


class ReportKind(StrEnum):
    COMPOSE = "compose"
    TEMPLATE = "template"
    NOTEBOOK = "notebook"
    REFLOW = "reflow"


class ReportLoader(StrEnum):
    DICT = "dict"
    FILESYSTEM = "filesystem"
    PACKAGE = "package"
    PREFIX = "prefix"
    FUNCTION = "function"
    MODULE = "module"


class ReportSource(StrEnum):
    IPYNB = "ipynb"
    MYST = "md:myst"
    PERCENT = "py:percent"
    LIGHT = "py:light"
    MARKDOWN = "md"
    RMD = "Rmd"
    QMD = "qmd"
    AUTO = "auto"


class ReportExport(StrEnum):
    HTML = "html"
    PDF = "pdf"
    WEBPDF = "webpdf"
    LATEX = "latex"
    MARKDOWN = "markdown"
    RST = "rst"
    ASCIIDOC = "asciidoc"
    SLIDES = "slides"
    NOTEBOOK = "notebook"
    PYTHON = "python"
    SCRIPT = "script"


class ReflowPaper(StrEnum):
    A4 = "a4"
    A3 = "a3"
    A5 = "a5"
    LETTER = "letter"
    LEGAL = "legal"


type LoaderRow = Callable[["ReportPlan"], BaseLoader]
type ComposeArm = Callable[["ReportPlan"], Awaitable[tuple[PageNode, tuple[ArtifactReceipt, ...]]]]

# --- [MODELS] ---------------------------------------------------------------------------


class NotebookEngine(Struct, frozen=True):
    timeout: int = 600
    startup_timeout: int = 60
    allow_errors: bool = False
    force_raise_errors: bool = True
    record_timing: bool = True
    interrupt_on_timeout: bool = True
    coalesce_streams: bool = True
    store_widget_state: bool = True
    skip_cells_with_tag: str = ""
    kernel_name: str = "python3"


class FigureRef(Struct, frozen=True):
    asset_key: ContentKey
    alt: str = ""
    caption: str = ""


class Section(Struct, frozen=True):
    level: int
    heading: str
    body: tuple[str, ...] = ()
    figures: tuple[FigureRef, ...] = ()


class ParamSchema(Struct, frozen=True, forbid_unknown_fields=True):
    pass


# --- [CONSTANTS] ------------------------------------------------------------------------

STRICT_ENGINE: Final[NotebookEngine] = NotebookEngine()
HTML_EXPORTS: Final[frozenset[ReportExport]] = frozenset({
    ReportExport.HTML, ReportExport.SLIDES, ReportExport.LATEX, ReportExport.MARKDOWN,
    ReportExport.RST, ReportExport.ASCIIDOC, ReportExport.PYTHON, ReportExport.SCRIPT, ReportExport.NOTEBOOK,
})

REPORT_ENV_KWARGS: Final[dict[str, object]] = {
    "autoescape": select_autoescape(enabled_extensions=("html", "xml")),
    "undefined": StrictUndefined,
    "enable_async": True,
    "trim_blocks": True,
    "lstrip_blocks": True,
}

# --- [OPERATIONS] -----------------------------------------------------------------------


def report_env(loader: BaseLoader, trusted: bool) -> Environment:
    factory = Environment if trusted else ImmutableSandboxedEnvironment
    return factory(loader=ChoiceLoader((loader, DictLoader({}))), **REPORT_ENV_KWARGS)


def schema_fields(schema: type[Struct]) -> tuple[FieldInfo, ...]:
    return fields(schema)


def _meta(key_role: str, role: str, page: int) -> NodeMeta:
    return NodeMeta(key=ContentIdentity.of(f"report-{key_role}-{page}", role.encode()), role=role, page=page)


def _runs(role: str, page: int, *lines: str) -> tuple[RunNode, ...]:
    return tuple(RunNode(meta=_meta(f"{role}-run", role, page), text=line, font_key="body", size=11.0) for line in lines if line)


def _box(plan: ReportPlan) -> tuple[float, float, float, float]:
    return tuple(pymupdf.paper_rect(ReflowPaper(plan.parameters.get("paper", ReflowPaper.A4)).value))


def _section_node(index: int, section: Section) -> SectionNode:
    return SectionNode(
        meta=_meta("section", section.heading, index),
        level=section.level,
        heading=_runs("heading", index, section.heading),
        children=(
            BlockNode(meta=_meta("block", "body", index), block=BlockKind.PARAGRAPH, runs=_runs("body", index, *section.body)),
            *(
                FigureNode(meta=_meta("figure", ref.alt, index), asset_key=ref.asset_key, alt=ref.alt, caption=_runs("caption", index, ref.caption))
                for ref in section.figures
            ),
        ),
    )


def _toc(title: str, sections: tuple[Section, ...]) -> SectionNode:
    return SectionNode(
        meta=_meta("toc", title, 0),
        level=1,
        heading=_runs("toc-heading", 0, title),
        children=tuple(
            BlockNode(meta=_meta("toc-entry", section.heading, index), block=BlockKind.LIST_ITEM, level=section.level, runs=_runs("toc-item", index, section.heading))
            for index, section in enumerate(sections)
        ),
    )


async def _compose_tree(plan: ReportPlan) -> tuple[PageNode, tuple[ArtifactReceipt, ...]]:
    sections = tuple(convert(row, Section) for row in plan.parameters.get("sections", ()))
    outline = (_toc(str(plan.parameters.get("toc_title", "Contents")), sections),) if plan.parameters.get("toc", True) else ()
    page = PageNode(
        meta=_meta("page", str(plan.kind), 0),
        media_box=tuple(plan.parameters.get("media_box", _box(plan))),
        children=(*outline, *(_section_node(index, section) for index, section in enumerate(sections, start=len(outline)))),
    )
    return page, ()


async def _template_tree(plan: ReportPlan) -> tuple[PageNode, tuple[ArtifactReceipt, ...]]:
    env = report_env(LOADERS[plan.loader](plan), plan.trusted)
    html = await env.from_string(plan.source).render_async(sections=plan.sections, figures=plan.figures)
    return _html_page(plan, "template", html.encode()), ()


async def _notebook_tree(plan: ReportPlan) -> tuple[PageNode, tuple[ArtifactReceipt, ...]]:
    node: NotebookNode = jupytext.reads(
        plan.source,
        fmt=jupytext.guess_format(plan.source, ".ipynb")[0] if plan.report_source is ReportSource.AUTO else plan.report_source.value,
    )
    gated = convert(plan.parameters.get("notebook_parameters", {}), plan.schema)
    parameterized = papermill.parameterize_notebook(
        node, papermill.add_builtin_parameters(asdict(gated)), report_mode=False, kernel_name=plan.engine.kernel_name
    )
    executed = await NotebookClient(parameterized, **asdict(plan.engine)).async_execute()
    archive = jupytext.writes(executed, fmt="ipynb").encode()
    archive_key = ContentIdentity.of("report-notebook-archive", archive)
    output, _resources = nbconvert.get_exporter(plan.export.value)().from_notebook_node(executed)
    body = output.encode() if isinstance(output, str) else output
    return _html_page(plan, "notebook", body), (ArtifactReceipt.Report(archive_key, len(archive)),)


async def _reflow_tree(plan: ReportPlan) -> tuple[PageNode, tuple[ArtifactReceipt, ...]]:
    pdf, count, box = await to_process.run_sync(_reflow, plan.source, plan.parameters)
    reflow_key = ContentIdentity.of("report-reflow-pdf", pdf)
    page = PageNode(meta=NodeMeta(key=reflow_key, role="reflow", page=count), media_box=box)
    return page, (ArtifactReceipt.Pdf(reflow_key, len(pdf), count),)


def _html_page(plan: ReportPlan, role: str, payload: bytes) -> PageNode:
    html, block = (payload.decode(), BlockKind.CODE) if plan.export in HTML_EXPORTS else (payload.hex(), BlockKind.QUOTE)
    return PageNode(
        meta=_meta("page", role, 0),
        media_box=_box(plan),
        children=(BlockNode(meta=_meta(role, role, 0), block=block, runs=_runs(role, 0, html)),),
    )


def _reflow(html: str, params: Mapping[str, object]) -> tuple[bytes, int, tuple[float, float, float, float]]:
    rect = pymupdf.paper_rect(ReflowPaper(params.get("paper", ReflowPaper.A4)).value)
    buffer = io.BytesIO()
    sink = pymupdf.DocumentWriter(buffer)
    story = pymupdf.Story(html=html, user_css=params.get("user_css"), em=float(params.get("em", 12)), archive=params.get("archive"))
    more, count = 1, 0
    while more:
        device = sink.begin_page(rect)
        more, _filled = story.place(rect)
        story.draw(device)
        sink.end_page()
        count += 1
    sink.close()
    return buffer.getvalue(), count, tuple(rect)


# --- [TABLES] ---------------------------------------------------------------------------

LOADERS: Final[MappingProxyType[ReportLoader, LoaderRow]] = MappingProxyType({
    ReportLoader.DICT: lambda plan: DictLoader({"<root>": plan.source, **plan.section_templates}),
    ReportLoader.FILESYSTEM: lambda plan: FileSystemLoader(tuple(plan.parameters.get("template_roots", ("templates",)))),
    ReportLoader.PACKAGE: lambda plan: PackageLoader(str(plan.parameters["package"]), str(plan.parameters.get("package_path", "templates"))),
    ReportLoader.PREFIX: lambda plan: PrefixLoader({str(prefix): DictLoader(mapping) for prefix, mapping in plan.section_templates.items()}),
    ReportLoader.FUNCTION: lambda plan: FunctionLoader(plan.parameters["load_func"]),
    ReportLoader.MODULE: lambda plan: ModuleLoader(str(plan.parameters["module_path"])),
})

COMPOSE_ARMS: Final[MappingProxyType[ReportKind, ComposeArm]] = MappingProxyType({
    ReportKind.COMPOSE: _compose_tree,
    ReportKind.TEMPLATE: _template_tree,
    ReportKind.NOTEBOOK: _notebook_tree,
    ReportKind.REFLOW: _reflow_tree,
})

# --- [COMPOSITION] ----------------------------------------------------------------------


class ReportPlan(Struct, frozen=True):
    kind: ReportKind
    source: str
    sections: dict[str, object] = field(default_factory=dict)
    section_templates: dict[str, str] = field(default_factory=dict)
    figures: tuple[FigureRef, ...] = ()
    parameters: dict[str, object] = field(default_factory=dict)
    schema: type[Struct] = ParamSchema
    loader: ReportLoader = ReportLoader.DICT
    trusted: bool = True
    report_source: ReportSource = ReportSource.IPYNB
    engine: NotebookEngine = STRICT_ENGINE
    export: ReportExport = ReportExport.HTML

    async def render(self) -> RuntimeRail[tuple[ContentKey, tuple[ArtifactReceipt, ...]]]:
        return await async_boundary(f"report.{self.kind}", self._emit)

    async def _emit(self) -> tuple[ContentKey, tuple[ArtifactReceipt, ...]]:
        tree, extra = await COMPOSE_ARMS[self.kind](self)
        body = encode(tree)
        key = ContentIdentity.of(f"report-{self.kind}", body)
        return key, (ArtifactReceipt.Report(key, len(body)), *extra)
```

## [03]-[RESEARCH]

- [REPORT_INTERIOR_TREE]: every `ReportKind` produces a `documents/model#NODE` `DocumentNode`, never a string — the canonical interior the `documents/emit#DOCUMENT` axis lowers. `COMPOSE` is the figure-binding kind the old fence omitted: `_compose_tree` folds the `Section` value-object sequence (each `convert(row, Section)`-admitted) into a `PageNode` carrying a `SectionNode` per section, each section's `body` lines into a `BlockNode(block=BlockKind.PARAGRAPH)` and each `FigureRef` into a `FigureNode(asset_key=ref.asset_key, alt=ref.alt)` keyed by the bound figure content key, so the figure lands IN the tree (a `FigureNode` the `documents/emit#DOCUMENT` lowering and the `accessibility/tagged#ACCESS` audit both read) rather than as a jinja `figures=` kwarg that never splices a node. `_toc` synthesizes the leading table-of-contents `SectionNode` from the heading hierarchy — one `BlockNode(block=BlockKind.LIST_ITEM, level=section.level)` per section heading, the outline depth the heading `level` carries, the TOC the section hierarchy implies rather than the `documents/emit#DOCUMENT TYPST_QUERY` `Compiler.query(selector="figure")` introspection (the query path stays the document-axis post-emission inverse, never re-run here). `TEMPLATE` wraps its strict-rendered HTML and `NOTEBOOK` its `from_notebook_node` output through the shared `_html_page` builder both kinds reuse — a `str` output (the `HTML_EXPORTS`-membered exports plus the always-text template) lands as a `BlockNode(block=BlockKind.CODE)` HTML leaf the `documents/emit#DOCUMENT PDF_HTML` weasyprint arm lowers, while a binary `PDF`/`WEBPDF` `bytes` output lands as a `BlockNode(block=BlockKind.QUOTE)` hex leaf rather than corrupting into an HTML code block — and `REFLOW` returns one `PageNode` over the reflowed-PDF content key carrying the page count and rect — one `COMPOSE_ARMS` table, four coroutine kinds, one `DocumentNode` contract, `encode(tree)` keying the content the receipt carries. The `PageNode`/`SectionNode`/`BlockNode`/`RunNode`/`FigureNode`/`NodeMeta`/`BlockKind` spellings are owned by `documents/model#NODE` and composed here, never re-declared; `BlockKind.CODE`/`PARAGRAPH`/`LIST_ITEM`/`QUOTE` are `documents/model#NODE` `BlockKind` rows verified against that page.
- [JINJA_STRICT_ASYNC]: the jinja2 catalogue's `[REPORT_TEMPLATING]` law fixes `undefined=StrictUndefined`, `enable_async=True`, and `autoescape=select_autoescape(enabled_extensions=(...))` as the default report-templating policy, so `report_env(loader, trusted)` admits a loader row composed through `ChoiceLoader` and `render_async` awaits the strict render — a missing section key raises `jinja2.UndefinedError` rather than blanking, the lone sync straggler is retired onto `async_boundary`, and the untrusted source rides `ImmutableSandboxedEnvironment` where a forbidden access is a typed `sandbox.SecurityError` on the templating rail. The `LOADERS` table mines the full catalogue loader axis — `DictLoader`/`FileSystemLoader`/`PackageLoader`/`PrefixLoader`/`FunctionLoader`/`ModuleLoader` (six of the catalogue's eight loader rows, `BaseLoader` abstract and `ChoiceLoader` the composition wrapper) — each a `ReportLoader` row resolving its loader value, never a parallel engine per source; `FunctionLoader(load_func)` admits a callable source provider and `ModuleLoader(module_path)` a pre-compiled-template tree, the two rows the prior fence left untapped. The `Environment`/`Environment.from_string`/`Template.render_async`/`select_autoescape`/`StrictUndefined`/`ChoiceLoader`/`DictLoader`/`FileSystemLoader`/`PackageLoader`/`PrefixLoader`/`FunctionLoader`/`ModuleLoader`/`UndefinedError`/`TemplateSyntaxError`/`TemplateNotFound`/`SecurityError` spellings verify against the folder `.api` catalogue for `jinja2` loader-axis rows `[01]`-`[08]`, engine entrypoint rows `[01]`/`[05]`, and the undefined/fault families.
- [NOTEBOOK_PARAM_GATE]: the in-memory notebook gate is `msgspec.convert(parameters, self.schema)` against the caller-declared `ParamSchema` `msgspec.Struct`, NOT `papermill.inspect_notebook` — the catalogue's `inspect_notebook(notebook_path, parameters)` row reads a notebook PATH and starts no kernel, so it cannot gate the in-memory `NotebookNode` the async boundary executes (the prior fence's `gated_parameters` computed a bare `papermill.inspect_notebook` function reference and discarded it, a dead gate). `convert` coerces and validates the supplied parameter dict against the declared field set and types, raising `msgspec.ValidationError` (a missing required field, a wrong type, or an out-of-bound value) before a kernel boots — the schema-driven gate the binding-algebra task names. `ParamSchema` carries `forbid_unknown_fields=True`, so the base is not a permissive empty bag: an unexpected key faults against any subclass and the inherited reject-extra closes the contract, while each caller subclass adds the required typed fields whose absence faults — a bare `dict[str, object]` parameter bag, which `convert` would accept wholesale, is the rejected form the closed struct replaces. The gated struct projects to a parameter dict through `msgspec.structs.asdict`, merges the papermill built-ins through `papermill.add_builtin_parameters`, and injects through `papermill.parameterize_notebook(node, ..., report_mode=False, kernel_name=plan.engine.kernel_name)` — the one injection point the catalogue's `[RAIL_LAW]` rejects hand-rolling outside, the `kernel_name` carrying the engine's configured kernel so `parameterize_notebook` resolves the matching `papermill.PapermillTranslators` row (`PythonTranslator`/`RTranslator`/`JuliaTranslator`/`ScalaTranslator`/`BashTranslator`/`CSharpTranslator`/`FSharpTranslator`) and serializes the parameter cell in the kernel's own language rather than defaulting every kernel to Python source. The `papermill.inspect_notebook(notebook_path, parameters)` row's declared-parameter introspection value — the capability the binding task names — is NOT discarded but re-homed: it reads a PATH and returns `Parameter` named tuples by starting no kernel, and the in-memory gate has no path, so `schema_fields(self.schema)` over `msgspec.structs.fields` surfaces the SAME declared-parameter contract (each `FieldInfo` carrying the parameter's `name`, `encode_name`, `type`, and `default`) off the live `ParamSchema` struct, the in-memory equivalent of the path-based inspection a caller queries before supplying parameters. The `convert`/`ValidationError`/`structs.asdict`/`structs.fields`/`structs.FieldInfo` spellings are core `msgspec` surface (the branch `.api/msgspec.md` `structs.fields` row `[01]` and the `FieldInfo` type row `[01]`); `parameterize_notebook(nb, parameters, report_mode, kernel_name, language, engine_name)`/`add_builtin_parameters(parameters)`/`PapermillTranslators`/`PapermillMissingParameterException`/`PapermillExecutionError` verify against the folder `.api` catalogue for `papermill` rows `[03]`/`[04]`, the translator-family rows, and the exception family. The path-routed `papermill.execute_notebook` is the rejected sync end-to-end call that blocks the async boundary.
- [NBCLIENT_ENGINE]: the parameterized node executes through `nbclient.NotebookClient(node, **asdict(engine)).async_execute()` — the catalogue's preferred async entrypoint over the in-memory `nbformat.NotebookNode`, returning the mutated node — where `**asdict(engine)` is the `NotebookEngine` struct projected by `msgspec.structs.asdict`, every key a verified `nbclient.NotebookClient` trait: `timeout`/`startup_timeout`/`kernel_name`/`allow_errors`/`force_raise_errors`/`record_timing`/`interrupt_on_timeout`/`coalesce_streams`/`store_widget_state`/`skip_cells_with_tag` (the configuration-trait rows `[01]`-`[15]` of the folder `.api` catalogue for `nbclient`, `store_widget_state` the `[NOTEBOOK_TOPOLOGY]` widget-state row). `record_timing=True` stamps per-cell timing into the executed node's metadata as the reproducibility timing receipt and `store_widget_state=True` captures the Jupyter widget state into the same metadata for UI replay; `interrupt_on_timeout=True` with `force_raise_errors=True` makes a runaway cell a `nbclient.exceptions.CellTimeoutError`/`CellExecutionError` and a kernel death a `DeadKernelError`, each funneled through the boundary into `BoundaryFault`, never a silent hang; `coalesce_streams=True` merges consecutive stream outputs and `skip_cells_with_tag` drops tagged cells before execution. The lifecycle-hook traits (`on_cell_execute`/`on_cell_executed`/`on_notebook_start`/`on_notebook_complete`/`on_notebook_error`, catalogue config rows `[09]`-`[13]`) stay off the frozen `NotebookEngine` because a `Callable` is not a reproducibility-receipt fact the content key keys — a hook is a per-run observation channel the runtime `observability` owns, not an engine-config value, so admitting one would smuggle a non-serializable runtime capability onto the keyed plan. A new bounded-safety trait is one `NotebookEngine` field `asdict` carries into the client with zero call-site edit; the `execute`/`async_execute`/`store_widget_state`/`CellExecutionError`/`CellTimeoutError`/`DeadKernelError` spellings verify against the folder `.api` catalogue for `nbclient` rows `[02]`/`[03]`, the config-trait table, and the exception family.
- [NBCONVERT_HTML]: the executed node lowers to document bytes through `nbconvert.get_exporter(plan.export.value)().from_notebook_node(executed)` — `get_exporter` resolves the `html`/`pdf`/`webpdf`/`latex`/`markdown`/`rst`/`asciidoc`/`slides`/`notebook`/`python`/`script` name row on the `nbconvert.exporters` registry (raising `ExporterNameError` on an unknown name), constructs the exporter, and `from_notebook_node(nb)` dispatches the in-memory `NotebookNode` returning the `(output, resources)` pair the catalogue's conversion axis fixes — the in-process path that retires the `subprocess` shell-out to the `jupyter-nbconvert` CLI. The `output` is the export-kind discriminant the `HTML_EXPORTS` frozenset partitions: a text exporter (`HTML`/`SLIDES`/`LATEX`/`MARKDOWN`/`RST`/`ASCIIDOC`/`PYTHON`/`SCRIPT`/`NOTEBOOK`) returns a `str` that `_html_page` wraps as a `BlockKind.CODE` HTML leaf feeding `documents/emit#DOCUMENT PDF_HTML`, while a binary exporter (`PDF` via LaTeX, `WEBPDF` via headless Chromium) returns `bytes` that `_html_page` hex-encodes into a `BlockKind.QUOTE` leaf — the `isinstance(output, str)` partition the `body = output.encode() if isinstance(output, str) else output` line owns so a `PDF`/`WEBPDF` export never mis-renders its assembled-PDF bytes as an HTML code block (the prior fence's unconditional `_html_page(html)` wrap was the silent corruption a binary export hit). The `resources` dict is the exporter's extracted-output/extension sidecar, bound only when an exporter extracts assets (the `WEBPDF`/extraction targets), never a captured receipt fact the `(ContentKey, byte_count)` `ArtifactReceipt.Report` case cannot carry. The prior fence's `nbconvert.export(exporter, nb)` collapses to the direct `get_exporter(name)().from_notebook_node(nb)` composition (`export` only re-dispatches on the `nb` shape an in-memory node already fixes, the deleted hop). The `get_exporter(name, config)`/`Exporter.from_notebook_node(nb, resources, **kw) -> tuple[output, resources]`/`HTMLExporter`/`PDFExporter`/`WebPDFExporter`/`get_export_names`/`ExporterNameError` spellings verify against the folder `.api` catalogue for `nbconvert` module-function rows `[01]`/`[03]`, convert-method row `[02]`, and exporter classes `[03]`-`[05]`/`[17]`.
- [JUPYTEXT_SOURCE]: the `ReportSource` rows route through `jupytext.reads(text, fmt)`/`jupytext.writes(notebook, fmt)` — `ipynb`/`md:myst`/`py:percent`/`py:light`/`md`/`Rmd`/`qmd` are `fmt` rows on the one conversion pair (seven rows, `AUTO` deferring to `guess_format`), the source-vs-string distinction the `text` argument, never a per-format reader — and `AUTO` resolves the `fmt` through `jupytext.guess_format(plan.source, ".ipynb")[0]` first, the content-plus-extension detection the catalogue's detection axis owns. The executed node serializes to the audit-archive bytes through `jupytext.writes(executed, "ipynb")` on the cp315 core. The `reads(text, fmt, ...)`/`writes(notebook, fmt, ...)`/`guess_format(text, ext)` spellings verify against the folder `.api` catalogue for `jupytext` read/write rows `[02]`/`[04]` and detection row `[01]`; the `py:light`/`Rmd`/`qmd` `fmt` rows are catalogued in the package-surface capability line (`py:percent`/`py:light`/`R`/`jl` and `Rmd`/`qmd`/`myst`), the two text-source rows the prior fence left untapped.
- [REFLOW_STORY]: the `REFLOW` kind is the HTML-INTO-PDF authoring direction of the `pymupdf.Story` reflow capability, the directional inverse of the `documents/lens#LENS` `STORY` `LensOp` row that recovers a `PageNode` sequence OUT of an emitted PDF — one capability, two arms split by direction across the producing and recovering owners, never one arm duplicating the other. `_reflow` runs on the runtime subprocess lane (`anyio.to_process.run_sync`, the gated `pymupdf` native-wheel band) over a held `io.BytesIO()` buffer: it opens one `pymupdf.DocumentWriter(buffer)`, builds one `pymupdf.Story(html=..., user_css=..., em=..., archive=...)`, loops `begin_page(paper_rect)` → `Story.place(rect) -> (more, filled)` → `Story.draw(device)` → `end_page()` until `more == 0`, calls `close()`, and returns `buffer.getvalue()` — the reflowed-PDF bytes the receipt keys, never a discarded sink whose bytes the prior fence lost — alongside the page count and the rect. The reflow is the one kind whose artifact is a PDF, so `_reflow_tree` keys it as `ArtifactReceipt.Pdf(reflow_key, len(pdf), count)` — the receipt owner's three-field `(ContentKey, byte_count, page_count)` PDF case — where the prior fence's `ArtifactReceipt.Report(reflow_key, len(pdf))` silently dropped the `count` the reflow loop already increments, the byte-equal-but-page-blind row a downstream page-budget audit cannot read. The paper rect resolves through `pymupdf.paper_rect(ReflowPaper(...).value)` over the named-size vocabulary, the same `_box` derivation every non-reflow `media_box` reads so the `(0, 0, 595, 842)` hand-built literal is deleted everywhere. The `Story(html, user_css, em, archive)`/`Story.place(where, flags) -> (more, filled)`/`Story.draw(device, matrix)`/`DocumentWriter(path, options)`/`DocumentWriter.begin_page(mediabox) -> Device`/`end_page()`/`close()`/`paper_rect(s) -> Rect` spellings verify against the folder `.api` catalogue for `pymupdf` story-layout rows `[01]`-`[07]` and paper-size row `[08]`. The report owns the authoring arm; the lens owns the recovering arm; neither is the other's deleted form.
- [RECEIPT_ROWS]: the bound tree and the executed-notebook archive return as `ArtifactReceipt.Report(key, byte_count)` rows and the reflowed PDF as the `ArtifactReceipt.Pdf(key, byte_count, page_count)` row the `receipt/receipt#RECEIPT` fold consumes — `_emit` keys the encoded `DocumentNode` tree once through `ContentIdentity.of(f"report-{self.kind}", body)` as a `Report` row, the `NOTEBOOK` kind keys the `jupytext.writes(executed, "ipynb")` archive through a second `ContentIdentity.of` as a second `Report` row, and the `REFLOW` kind keys the `buffer.getvalue()` reflowed-PDF bytes through a second `ContentIdentity.of` as the page-count-bearing `Pdf` row (the case the receipt owner declares for a PDF artifact — a `Report` row would erase the page count the loop counts), so the reproducibility artifact and the rendered output land as two keyed receipt rows on the one `RuntimeRail` payload, and the bound `DocumentNode` keys outward only as the one `compute/graduation` `HandoffAxis` model-asset case keyed by `ContentIdentity` (the existing handoff card covers it, never a private per-report handoff). The `ArtifactReceipt.Report(key, byte_count)` two-field case and the `ArtifactReceipt.Pdf(key, byte_count, page_count)` three-field case are owned by `receipt/receipt#RECEIPT` (cases `report`/`pdf`) and composed here, so the resolved loader/source/export row and the gated field set the `schema_fields` introspection surfaces stay recoverable from the keyed `ReportPlan` rather than a parallel fact map; the content key is consumed from runtime, never re-minted.
