# [PY_ARTIFACTS_REPORT]

The reproducible-report composition layer binding data and visual outputs into the one `document/model#NODE` `DocumentNode` tree, structurally identical to the `document/emit#DOCUMENT` lowering owner it is the composition peer of. `ReportPlan` is ONE frozen owner discriminating report kind over a `frozendict[ReportKind, ComposeArm]` coroutine-row policy table — the `BACKENDS`/`_CORE_ARMS` band-table idiom `document/emit#DOCUMENT` and `document/lens#LENS` prove — so `_emit` is one uniform `await COMPOSE_ARMS[self.kind](self)` with zero branch. The canonical interior every kind PRODUCES is the `DocumentNode` tree, never a string: section composition (a `Section` value-object algebra folding heading level, body runs, and `FigureRef` content-key refs into a `SectionNode`/`BlockNode`/`FigureNode` subtree with a TOC `SectionNode` synthesized from the heading hierarchy), `jinja2` template composition (a `ReportLoader`-row engine over the trusted/sandboxed `Environment` policy, strict-undefined async render emitting a free-form HTML `BlockNode` leaf), parameterized-notebook execution (a `ReportSource`-row `jupytext` text-source pairing into a `NotebookNode` — `guess_format`-driven on the `AUTO` row — a `ReportParams` `TypedDict` declared-parameter gate admitted through a module-level `TypeAdapter`, a `papermill.parameterize.parameterize_notebook` injection threading the `NotebookEngine.kernel_name` so a non-`python3` kernel routes its own `PapermillTranslators` row, a host-free `nbclient.NotebookClient.async_execute` run under the frozen `NotebookEngine` traits, an executed-notebook archive, and an `nbconvert.export` lowering whose `(output, resources)` pair feeds a `BlockNode` leaf — a `str` output a `BlockKind.CODE` HTML leaf and the binary `PDF`/`WEBPDF` `bytes` output a `BlockKind.QUOTE` hex leaf, never an HTML wrapper over PDF bytes), and reflowable re-layout (the `pymupdf.Story`/`DocumentWriter` HTML-to-PDF reflow the `REFLOW` kind AUTHORS as a fresh `PageNode` plus the reflowed PDF bytes, the authoring inverse of the `document/lens#LENS STORY` recover-TO half).

Every per-arm input is one frozen `ReportSpec` `msgspec.Struct` admitted exactly once at `ReportPlan.of` through the closed `ReportPayload` `TypedDict` and its module-level `TypeAdapter` — never a `dict[str, object]` bag, never re-validated in the interior. Every kind threads ONE `ReportFact` typed-evidence carrier onto the frozen owner through `copy.replace` (a thin pure `_emit` core), returns a `RuntimeRail[ContentKey]` keyed by the runtime content key, and contributes its `ArtifactReceipt` rows through the `@receipted` harvest weave — the `_emit` core stays a pure `Self`-returning fold under a stacked `@aspected` contract arm and `@receipted` harvest arm, never inline-repeated receipt construction. The closed provider-exception families (`jinja2.UndefinedError`/`TemplateSyntaxError`/`TemplateNotFound`/`sandbox.SecurityError`, `pydantic.ValidationError`, `papermill.PapermillMissingParameterException`/`PapermillExecutionError`, `nbclient.exceptions.CellExecutionError`/`CellTimeoutError`/`DeadKernelError`, `nbconvert.ExporterNameError`) convert once at the `async_boundary` capsule into `BoundaryFault`. It owns neither the visual nor the codec — it binds the figures keyed by content key into the tree (a `FigureNode` per `asset_key`, never re-rendering the chart/table/scene), executes a notebook under a bounded-safety engine, reflows HTML through the native MuPDF story loop, threads each emitted artifact onto its `ReportFact`, and hands the tree to the document axis.

## [01]-[INDEX]

- [01]-[REPORT]: report-composition axis over the section/template/notebook/reflow kinds producing one `DocumentNode` interior through the one `COMPOSE_ARMS` coroutine-row `frozendict`, the `Section` heading-and-figure-ref value object the `COMPOSE` kind folds into the tree, the closed `ReportPayload` `TypedDict` ingress plus its `ReportSpec` carrier and `ReportParams` notebook-parameter gate, the `ReportFact` typed-evidence carrier threaded onto the owner, the `NotebookEngine` bounded-safety engine-config value, the `ReportLoader` `jinja2` loader-and-sandbox sub-axis, the `ReportSource` `jupytext` text-source sub-axis (the `guess_format` `AUTO` row included), the `ReportExport` closed `nbconvert` export-name vocabulary with its `HTML_EXPORTS` text-vs-binary partition, the `ReflowPaper` named-paper-size sub-axis, the `@aspected`/`@receipted` weaves over a thin pure `_emit`, and the modal-arity `rendered` batch entry.

## [02]-[REPORT]

- Owner: `ReportPlan` the one frozen composition axis discriminating report kind over the `COMPOSE_ARMS` `frozendict[ReportKind, ComposeArm]` (every row a `Callable[[ReportPlan], Awaitable[ReportFact]]`, the `document/emit#DOCUMENT BACKENDS`/`document/lens#LENS _CORE_ARMS` band-table idiom) and producing one `DocumentNode` interior; `ReportKind` the closed `StrEnum` over `COMPOSE`/`TEMPLATE`/`NOTEBOOK`/`REFLOW`; `ReportSpec` the one frozen `msgspec.Struct` carrier holding the admitted report source, sections, figure refs, loader/source/export rows, engine, trust flag, and paper — admitted once at `ReportPlan.of` from the closed `ReportPayload` `TypedDict` and never re-validated; `ReportFact` the threaded `msgspec.Struct` evidence carrier (the rendered `DocumentNode`, its byte count, the optional notebook-archive bytes, the page count, the resolved export/loader/source rows, the per-cell-timing and widget-state flags) read by `contribute` to mint the receipt rows; `Section` the frozen value object carrying a heading level, body runs, and a `FigureRef` sequence the `COMPOSE` fold lowers into a `SectionNode`/`BlockNode`/`FigureNode` subtree; `FigureRef` the one figure-reference value object (`asset_key` content key, `alt` equivalent, `caption`) the `COMPOSE` and `TEMPLATE` kinds both bind; `ReportParams` the closed `TypedDict(closed=True)` declared-parameter contract a module-level `TypeAdapter` validates the supplied notebook parameters against before a kernel boots, the declared field set recoverable through `ReportParams.__required_keys__`/`__optional_keys__`; `ReportLoader` the closed `StrEnum` selecting the `jinja2` loader composition (`DictLoader`/`FileSystemLoader`/`PackageLoader`/`PrefixLoader`/`FunctionLoader`/`ModuleLoader` folded through `ChoiceLoader`) the `LOADERS` row table resolves; `ReportSource` the closed `StrEnum` selecting the `jupytext` text representation, with `AUTO` deferring to `jupytext.guess_format`; `ReportExport` the closed `StrEnum` over the `nbconvert.exporters` registry name rows `get_exporter` resolves, partitioned by `HTML_EXPORTS` into the text-output rows (`str` -> `BlockKind.CODE` HTML leaf) and the binary `PDF`/`WEBPDF` rows (`bytes` -> `BlockKind.QUOTE` hex leaf); `NotebookEngine` the frozen engine-config value object carrying the full `nbclient.NotebookClient` bounded-safety trait set projected to constructor kwargs through `msgspec.structs.asdict`; `ReflowPaper` the closed `StrEnum` over the `pymupdf.paper_rect` named-size vocabulary every kind's `media_box` derives from through `_box`; the bind layer between visual outputs and document inputs.
- Cases: `_compose_arm` (the `COMPOSE` row) folds the `Section` value-object sequence into a `SectionNode` outline (each section's runs into a `BlockNode`, each `FigureRef` into a `FigureNode` keyed by the bound `asset_key`), synthesizes a leading table-of-contents `SectionNode` from the heading hierarchy through `_toc`, and returns a `ReportFact` over the assembled `PageNode`; `_template_arm` (the `TEMPLATE` row) renders the `report_env(loader, trusted)` engine — a trusted `jinja2.Environment` or an untrusted `jinja2.sandbox.ImmutableSandboxedEnvironment` selected by `ReportSpec.trusted`, both under `REPORT_ENV_KWARGS` — over a `ReportLoader`-resolved loader, the section data, and the one `FigureRef` sequence, into an HTML string wrapped through `_html_page` as one `BlockNode(block=BlockKind.CODE)` HTML leaf the `document/emit#DOCUMENT PDF_HTML` weasyprint arm consumes; `_notebook_arm` (the `NOTEBOOK` row) admits the source through the `ReportSource` `jupytext` row (`AUTO` resolving the `fmt` through `guess_format`) into one `nbformat.NotebookNode`, gates the supplied parameters against `ReportParams` through the module-level `TypeAdapter` (an absent required field or a type mismatch raising `pydantic.ValidationError` before a kernel boots), injects the parameter cell through `papermill.parameterize.parameterize_notebook` over `papermill.parameterize.add_builtin_parameters` threading `kernel_name=spec.engine.kernel_name` so the `PapermillTranslators` row matches the configured kernel language, executes the node headlessly through `nbclient.NotebookClient.async_execute` under the `NotebookEngine` traits, reads the `(output, resources)` pair from `nbconvert.export(get_exporter(...), executed)`, and threads a `ReportFact` carrying the lowered `BlockNode` page, the `jupytext.writes(executed, "ipynb")` archive bytes, the resolved export row, and the engine's timing/widget flags; `_reflow_arm` (the `REFLOW` row) flows the source HTML through one `pymupdf.Story(html, user_css, em, archive)`/`DocumentWriter(BytesIO)` `place`/`draw` loop over the `ReflowPaper`-resolved `paper_rect` on the runtime subprocess lane (`anyio.to_process.run_sync`), recovers the `BytesIO` buffer's reflowed PDF bytes plus the page count, and threads a `ReportFact` carrying the `PageNode`, the reflowed PDF bytes, and the page count the receipt's `ArtifactReceipt.Pdf` case lands on. `ReportLoader` rows `DICT` (`DictLoader` in-memory sections) · `FILESYSTEM` (`FileSystemLoader` template roots) · `PACKAGE` (`PackageLoader` package-resource tree) · `PREFIX` (`PrefixLoader` namespaced child loaders) · `FUNCTION` (`FunctionLoader` callable source provider) · `MODULE` (`ModuleLoader` pre-compiled template tree). `ReportSource` rows `IPYNB` · `MYST` · `PERCENT` (`py:percent`) · `LIGHT` (`py:light`) · `MARKDOWN` · `RMD` (`Rmd`) · `QMD` (`qmd`) · `AUTO` (content-detected through `guess_format`). `ReportExport` rows `HTML` · `PDF` · `WEBPDF` · `LATEX` · `MARKDOWN` · `RST` · `ASCIIDOC` · `SLIDES` · `NOTEBOOK` · `PYTHON` · `SCRIPT` — each an `nbconvert.exporters` name `get_exporter` resolves, raising `ExporterNameError` on an unknown name; `HTML_EXPORTS` is the one membership predicate partitioning text output from the binary `PDF`/`WEBPDF` rows. `ReflowPaper` rows `A4` · `A3` · `A5` · `LETTER` · `LEGAL` — each a `pymupdf.paper_rect` named-size string.
- Entry: `ReportPlan.render` is `async` over the runtime `async_boundary`, mapping the fault-capsuled `_emit` to its content key — `async_boundary(f"report.{self.kind}", self._emit).map(lambda done: done.content_key())` — and returning `RuntimeRail[ContentKey]`, the exact `document/emit#DOCUMENT` shape; `rendered(plans)` is the modal-arity batch entry discriminating arity on the INPUT SHAPE (a lone `ReportPlan` runs its one `_stepped` arm, an `Iterable[ReportPlan]` is the batched render threaded through the rail with NO `batch`/`mode` knob). `_emit` is the thin pure core: it threads `replace(self, fact=await COMPOSE_ARMS[self.kind](self))` and returns `Self` so the `@receipted` harvest drains `contribute`; the `@aspected(lifted=...)` contract arm folds a `BeartypeCallHintViolation` onto a fault, and the `async_boundary` capsule converts every provider-exception family (`jinja2.UndefinedError`/`TemplateSyntaxError`/`TemplateNotFound`/`sandbox.SecurityError`, `pydantic.ValidationError`, `papermill.PapermillMissingParameterException`/`PapermillExecutionError`, `nbclient.exceptions.CellExecutionError`/`CellTimeoutError`/`DeadKernelError`, `nbconvert.ExporterNameError`) into `BoundaryFault` through the runtime `CLASSIFY` rows, never a per-kind `try`/`except` ladder inside `_emit`.
- Auto: the `COMPOSE` fold reduces the `Section` sequence into a `SectionNode` outline through `_section_node` — each section's body runs become a `BlockNode`, each `figure_ref` an `asset_key`-keyed `FigureNode` whose `alt` the section authors, and `_toc` synthesizes a leading table-of-contents `SectionNode` (a `BlockNode(block=BlockKind.LIST_ITEM)` per heading, the outline depth the heading `level` carries) so the bound figures land IN the tree rather than as a jinja kwarg; template binding folds the sections and the one `FigureRef` sequence into `Environment.from_string(...).render_async` (strict-undefined: a missing section key is a `jinja2.UndefinedError` fault, never a silent blank) and wraps the rendered HTML as one `BlockNode` HTML leaf; notebook execution admits the `ReportSource` text through `jupytext.reads(text, fmt)` (`AUTO` resolving `fmt` through `jupytext.guess_format(text, ".ipynb")` first), gates the supplied parameters through `_PARAMS.validate_python(spec.notebook_parameters)` against the `ReportParams` declared-field contract (a missing required field or a wrong type raising `pydantic.ValidationError` before a kernel boots — the in-memory gate `papermill.inspect_notebook` cannot serve because it reads a path, not a node), injects parameters through `parameterize_notebook(node, add_builtin_parameters(dict(gated)), report_mode=False, kernel_name=spec.engine.kernel_name)` (the one injection point `papermill` owns, the `kernel_name` selecting the `PapermillTranslators` row so an `R`/`julia`/`.net-csharp` kernel serializes its parameter cell in its own language), runs the node through `nbclient.NotebookClient(node, **asdict(engine)).async_execute()` on the async kernel lane (a runaway cell raising `CellTimeoutError`/`CellExecutionError`, a kernel death `DeadKernelError`), serializes the executed node — whose per-cell timing `record_timing=True` already stamped into its metadata and whose widget state `store_widget_state=True` captured — to the reproducibility-archive bytes through `jupytext.writes(executed, "ipynb")`, and lowers the executed node through `nbconvert.export(get_exporter(export.value), executed)` reading the `(output, resources)` pair: a `HTML_EXPORTS`-membered export's `str` output wraps as a `BlockKind.CODE` HTML leaf while a `PDF`/`WEBPDF` export's `bytes` output wraps as a `BlockKind.QUOTE` hex leaf, the `isinstance(output, str)` partition `_html_page` owns so a binary export never mis-renders as HTML text, the `resources` sidecar staying the export-extraction channel; the `REFLOW` loop opens one `pymupdf.DocumentWriter(io.BytesIO())` over a held buffer, lays the HTML through `pymupdf.Story(html, user_css, em, archive).place(rect)` until `more == 0`, draws each filled slice onto the `begin_page(paper_rect)` device, closes the writer, and returns the buffer's reflowed-PDF bytes, the page count, and the rect; every kind's non-reflow `media_box` derives from `_box(spec)` over `pymupdf.paper_rect(spec.paper.value)`, never a hand-built `(0, 0, 595, 842)` literal.
- Receipt: the `@receipted(Redaction.STRUCTURAL)` harvest weave stacks over the pure `_emit`, draining `ReportPlan.contribute` and emitting through `Signals.emit_async`; `contribute` reads the threaded `ReportFact` off `self.fact` (never an in-process re-run of an arm), re-mints the content key over `fact.body`, and folds the case off the `ReportKind` discriminant in one expression — the tree keys once through `ContentIdentity.of(f"report-{kind}", fact.body)` as a `core/receipt#RECEIPT`-owned `ArtifactReceipt.Report(key, byte_count)` row, the `NOTEBOOK` kind keys the `fact.archive` archive through a second `ContentIdentity.of` as a second `Report` row, and the `REFLOW` kind keys `fact.body` as the page-count-bearing `ArtifactReceipt.Pdf(key, byte_count, page_count)` row (the receipt owner's three-field PDF case — a `Report` row would silently drop the page count the reflow loop already counts), the exact `(ContentKey, <scalars>)` arities the receipt owner declares, never a widened tuple. The rich per-render evidence (the resolved loader/source/export row, the autoescape decision, the gated `ReportParams` field set, the per-cell timing and widget-state flags) rides the `ReportFact` carrier the consumer reads off `self.fact`; the cross-producer receipt stream carries only the owner's flat scalars, so a richer report-evidence receipt case is a `core/receipt#RECEIPT` growth concern, never minted here against a case that does not exist.
- Packages: `jinja2` (`Environment`/`sandbox.ImmutableSandboxedEnvironment`/`sandbox.SecurityError`/`StrictUndefined`/`select_autoescape`/`FileSystemLoader`/`DictLoader`/`PackageLoader`/`PrefixLoader`/`FunctionLoader`/`ModuleLoader`/`ChoiceLoader`/`Environment.from_string`/`Template.render_async`/`UndefinedError`/`TemplateSyntaxError`/`TemplateNotFound`, catalogue loader rows `[01]`-`[08]`, engine rows `[01]`/`[05]`, undefined/fault families), `papermill` (`papermill.parameterize.parameterize_notebook(nb, parameters, report_mode, kernel_name)`/`papermill.parameterize.add_builtin_parameters`/`PapermillTranslators`/`PapermillMissingParameterException`/`PapermillExecutionError`, catalogue rows `[03]`/`[04]` resolved from `papermill.parameterize`, the translator-family rows, the exception family), `nbclient` (`NotebookClient`/`async_execute`, `CellExecutionError`/`CellTimeoutError`/`DeadKernelError`, the `timeout`/`startup_timeout`/`kernel_name`/`allow_errors`/`allow_error_names`/`force_raise_errors`/`record_timing`/`interrupt_on_timeout`/`error_on_timeout`/`iopub_timeout`/`raise_on_iopub_timeout`/`coalesce_streams`/`store_widget_state`/`skip_cells_with_tag`/`display_data_priority` traits, catalogue config rows `[01]`-`[18]`), `jupytext` (`reads`/`writes`/`guess_format`, `fmt` rows `[02]`/`[04]`/`[01]`), `nbconvert` (`get_exporter`/`export`/`Exporter.from_notebook_node`/`get_export_names`/`ExporterNameError`, catalogue rows `[01]`/`[02]`/`[03]`), `nbformat` (`NotebookNode`), `pymupdf` (`Story(html, user_css, em, archive)`/`Story.place`/`Story.draw`/`DocumentWriter`/`DocumentWriter.begin_page`/`end_page`/`close`/`paper_rect`, catalogue story-layout rows `[01]`/`[04]`/`[06]`-`[10]`), `msgspec` (`Struct`/`field`/`structs.asdict`/`convert`/`json.Encoder`), `pydantic` (`TypeAdapter`/`ValidationError` the `ReportPayload`/`ReportParams` admission), runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`/`BoundaryFault`, `receipts.Receipt`/`receipted`/`Signals`/`Redaction`, the `surfaces-and-dispatch.md @aspected` contract weave, `anyio.to_process.run_sync` the gated subprocess lane for `REFLOW`), `document/model#NODE` (`DocumentNode`/`PageNode`/`SectionNode`/`BlockNode`/`RunNode`/`FigureNode`/`NodeMeta`/`BlockKind`/`encode`), `artifacts.core.receipt` (`ArtifactReceipt.Report`/`ArtifactReceipt.Pdf`).
- Growth: a new report kind is one `ReportKind` row plus one `COMPOSE_ARMS` coroutine-row returning a `ReportFact`; a new section concept is one `Section` field plus one `_section_node` lowering arm; a new loader root is one `ReportLoader` row plus one `LOADERS` table arm; a new text source is one `ReportSource` row plus one `jupytext` `fmt` value; a new export target is one `ReportExport` row on the `nbconvert.exporters` registry plus its `HTML_EXPORTS` membership; a new paper size is one `ReflowPaper` row; a new bounded-safety trait is one field on `NotebookEngine`, carried into the client by `asdict` with zero call-site edit; a new declared parameter is one `ReportParams` band line the `__required_keys__`/`__optional_keys__` introspection picks up; a new evidence scalar is one `ReportFact` field; zero new surface.
- Boundary: a string-returning report that never produces a node, a figure passed only as a jinja kwarg where the `COMPOSE` fold splices a `FigureNode` into the tree, a hand-rolled section-composition loop, an inline `match self.kind` ladder where the `COMPOSE_ARMS` table dispatches, a lenient default `Environment` that blanks missing keys, a sync straggler render, an untrusted-template `Environment` with the sandbox disabled, a top-level `papermill.parameterize_notebook` access where the symbol lives in `papermill.parameterize`, a `papermill.inspect_notebook` parameter gate on a path where the in-memory node demands the `ReportParams` `TypeAdapter`, a bare `dict[str, object]` parameter bag where the closed `ReportParams` `TypedDict` validates field types, a hand-rolled parameter injection outside `parameterize_notebook`, a Python-defaulted `parameterize_notebook` where the `kernel_name` routes the matching `PapermillTranslators` row, a bare `str` export knob where the closed `ReportExport` vocabulary resolves through `get_exporter`, a `BlockKind.CODE` HTML wrapper over the binary `PDF`/`WEBPDF` `bytes` output where the `HTML_EXPORTS` partition selects the hex `BlockKind.QUOTE` leaf, a path-based `papermill.execute_notebook` blocking the async boundary, a `subprocess` shell-out to `jupyter-nbconvert`, a parallel `tuple[ContentKey, ...]` figure channel beside the `FigureRef` value object, a discarded reflowed-PDF buffer where the bytes are the artifact, a hand-built `(0, 0, 595, 842)` media-box literal where `_box` derives the rect through `paper_rect`, an `ArtifactReceipt.Report` row on the reflowed PDF that drops the page count, an `ArtifactReceipt` channel threaded through the rail payload where the `@receipted` harvest owns it, an inline `try`/`except` receipt-construction loop where the stacked aspect drains `contribute`, a `MappingProxyType` table where `frozendict` owns the dispatch rows, and a `from __future__ import annotations` header on a py3.15 fence are the deleted forms — the report binds `FigureRef` content keys produced by `visualization/chart#CHART`, `visualization/table#TABLE`, and `scene#SCENE` (never re-rendering them) into the tree, parses the text source through `jupytext`, runs the node on the async `nbclient` kernel lane, exports the executed node in-process through `nbconvert`, reflows HTML through the native `pymupdf.Story` loop into a keyed PDF artifact, and threads each artifact onto one `ReportFact`. The produced `DocumentNode` tree hands to `document/emit#DOCUMENT` for lowering (`PDF_HTML` for the HTML leaves, `PDF_TYPST`/`PDF_AUTHOR` for the structured tree); PAdES/PDF-A finishing routes to `exchange/conformance#CONFORM`; the security-and-navigation finishing close routes to `document/egress#FINISH`; the `REFLOW` kind is this owner's HTML-INTO-PDF authoring half, the directional inverse of the `document/lens#LENS STORY` recover-OUT-of-PDF extraction arm — authoring and extraction are two arms of the one `pymupdf.Story` capability split across the two owners by direction, not a duplicate; no live kernel server, no UI; the content key is consumed from runtime, never re-minted.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from collections.abc import Awaitable, Callable, Iterable
from copy import replace
from enum import StrEnum
from typing import Final, Literal, NotRequired, ReadOnly, Required, Self, TypedDict, Unpack

import jupytext
import nbconvert
import pymupdf
from anyio import to_process
from builtins import frozendict
from expression import Error, Ok, Result
from expression.collections import Block
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
from msgspec import Struct, field
from msgspec.structs import asdict
from nbclient import NotebookClient
from nbformat import NotebookNode
from papermill.parameterize import add_builtin_parameters, parameterize_notebook
from pydantic import TypeAdapter, ValidationError

from artifacts.core.receipt import ArtifactReceipt
from artifacts.document.model import (
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
from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary
from rasm.runtime.receipts import Receipt, Redaction, receipted
from rasm.runtime.surfaces import aspected  # the surfaces-and-dispatch contract weave

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


type ReportFault = Literal["<contract>"]
type LoaderRow = Callable[["ReportSpec"], BaseLoader]
type ComposeArm = Callable[["ReportPlan"], Awaitable["ReportFact"]]
type Box = tuple[float, float, float, float]

# --- [MODELS] ---------------------------------------------------------------------------


class NotebookEngine(Struct, frozen=True):
    # the full nbclient bounded-safety trait set, projected to constructor kwargs by `asdict`;
    # `allow_error_names`/`error_on_timeout`/`iopub_timeout`/`raise_on_iopub_timeout`/
    # `display_data_priority` are the catalogue traits a runaway-cell, late-iopub, and
    # MIME-selection policy needs — a new trait is one field carried with zero call-site edit.
    timeout: int | None = 600
    startup_timeout: int = 60
    allow_errors: bool = False
    allow_error_names: tuple[str, ...] = ()
    force_raise_errors: bool = True
    record_timing: bool = True
    interrupt_on_timeout: bool = True
    error_on_timeout: dict[str, str] | None = None
    iopub_timeout: int = 4
    raise_on_iopub_timeout: bool = True
    coalesce_streams: bool = True
    store_widget_state: bool = True
    skip_cells_with_tag: str = "skip-execution"
    display_data_priority: tuple[str, ...] = ()
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


class ReportParams(TypedDict, closed=True):
    # the declared notebook-parameter contract admitted exactly once through `_PARAMS`; a caller
    # extension is one `extra_items`-banded subclass, not a `dict[str, object]` bag `convert`
    # waves through. The bare base is closed: an unexpected key faults at `validate_python`.
    pass


class ReportSpec(Struct, frozen=True):
    # the one admitted-once carrier: the report source plus every per-kind knob, materialized at
    # `ReportPlan.of` from the closed `ReportPayload` and never re-validated interior-side.
    source: str
    sections: tuple[Section, ...] = ()
    section_templates: frozendict[str, str] = field(default_factory=frozendict)
    figures: tuple[FigureRef, ...] = ()
    notebook_parameters: ReportParams = field(default_factory=dict)
    template_roots: tuple[str, ...] = ("templates",)
    loader: ReportLoader = ReportLoader.DICT
    trusted: bool = True
    report_source: ReportSource = ReportSource.IPYNB
    engine: NotebookEngine = field(default_factory=NotebookEngine)
    export: ReportExport = ReportExport.HTML
    paper: ReflowPaper = ReflowPaper.A4
    user_css: str | None = None
    em: float = 12.0


class ReportFact(Struct, frozen=True):
    # the threaded evidence the @receipted harvest reads off `self.fact`: the rendered tree, its
    # encoded bytes the content key hashes, the optional notebook-archive bytes and page count the
    # second/Pdf receipt rows carry, the resolved export/loader/source rows, and the timing/widget
    # flags the engine stamped — the rich evidence the flat-scalar receipt stream never carries.
    node: DocumentNode
    body: bytes
    archive: bytes = b""
    pages: int = 0
    export: ReportExport = ReportExport.HTML
    loader: ReportLoader = ReportLoader.DICT
    report_source: ReportSource = ReportSource.IPYNB
    timed: bool = False
    widgets: bool = False


# --- [CONSTANTS] ------------------------------------------------------------------------

HTML_EXPORTS: Final[frozenset[ReportExport]] = frozenset({
    ReportExport.HTML, ReportExport.SLIDES, ReportExport.LATEX, ReportExport.MARKDOWN,
    ReportExport.RST, ReportExport.ASCIIDOC, ReportExport.PYTHON, ReportExport.SCRIPT, ReportExport.NOTEBOOK,
})

REPORT_ENV_KWARGS: Final[frozendict[str, object]] = frozendict({
    "autoescape": select_autoescape(enabled_extensions=("html", "xml")),
    "undefined": StrictUndefined,
    "enable_async": True,
    "trim_blocks": True,
    "lstrip_blocks": True,
})

# --- [BOUNDARIES] -----------------------------------------------------------------------


class ReportPayload(TypedDict, extra_items=object):
    source: Required[ReadOnly[str]]
    sections: NotRequired[ReadOnly[tuple[Section, ...]]]
    figures: NotRequired[ReadOnly[tuple[FigureRef, ...]]]
    notebook_parameters: NotRequired[ReadOnly[ReportParams]]
    loader: NotRequired[ReadOnly[ReportLoader]]
    trusted: NotRequired[ReadOnly[bool]]
    report_source: NotRequired[ReadOnly[ReportSource]]
    engine: NotRequired[ReadOnly[NotebookEngine]]
    export: NotRequired[ReadOnly[ReportExport]]
    paper: NotRequired[ReadOnly[ReflowPaper]]


_PAYLOAD: Final = TypeAdapter(ReportPayload)
_PARAMS: Final = TypeAdapter(ReportParams)

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

- [REPORT_INTERIOR_TREE]: every `ReportKind` produces a `document/model#NODE` `DocumentNode`, never a string — the canonical interior the `document/emit#DOCUMENT` axis lowers. `COMPOSE` is the figure-binding kind the old fence omitted: `_compose_tree` folds the `Section` value-object sequence (each `convert(row, Section)`-admitted) into a `PageNode` carrying a `SectionNode` per section, each section's `body` lines into a `BlockNode(block=BlockKind.PARAGRAPH)` and each `FigureRef` into a `FigureNode(asset_key=ref.asset_key, alt=ref.alt)` keyed by the bound figure content key, so the figure lands IN the tree (a `FigureNode` the `document/emit#DOCUMENT` lowering and the `document/tagged#ACCESS` audit both read) rather than as a jinja `figures=` kwarg that never splices a node. `_toc` synthesizes the leading table-of-contents `SectionNode` from the heading hierarchy — one `BlockNode(block=BlockKind.LIST_ITEM, level=section.level)` per section heading, the outline depth the heading `level` carries, the TOC the section hierarchy implies rather than the `document/emit#DOCUMENT TYPST_QUERY` `Compiler.query(selector="figure")` introspection (the query path stays the document-axis post-emission inverse, never re-run here). `TEMPLATE` wraps its strict-rendered HTML and `NOTEBOOK` its `from_notebook_node` output through the shared `_html_page` builder both kinds reuse — a `str` output (the `HTML_EXPORTS`-membered exports plus the always-text template) lands as a `BlockNode(block=BlockKind.CODE)` HTML leaf the `document/emit#DOCUMENT PDF_HTML` weasyprint arm lowers, while a binary `PDF`/`WEBPDF` `bytes` output lands as a `BlockNode(block=BlockKind.QUOTE)` hex leaf rather than corrupting into an HTML code block — and `REFLOW` returns one `PageNode` over the reflowed-PDF content key carrying the page count and rect — one `COMPOSE_ARMS` table, four coroutine kinds, one `DocumentNode` contract, `encode(tree)` keying the content the receipt carries. The `PageNode`/`SectionNode`/`BlockNode`/`RunNode`/`FigureNode`/`NodeMeta`/`BlockKind` spellings are owned by `document/model#NODE` and composed here, never re-declared; `BlockKind.CODE`/`PARAGRAPH`/`LIST_ITEM`/`QUOTE` are `document/model#NODE` `BlockKind` rows verified against that page.
- [JINJA_STRICT_ASYNC]: the jinja2 catalogue's `[REPORT_TEMPLATING]` law fixes `undefined=StrictUndefined`, `enable_async=True`, and `autoescape=select_autoescape(enabled_extensions=(...))` as the default report-templating policy, so `report_env(loader, trusted)` admits a loader row composed through `ChoiceLoader` and `render_async` awaits the strict render — a missing section key raises `jinja2.UndefinedError` rather than blanking, the lone sync straggler is retired onto `async_boundary`, and the untrusted source rides `ImmutableSandboxedEnvironment` where a forbidden access is a typed `sandbox.SecurityError` on the templating rail. The `LOADERS` table mines the full catalogue loader axis — `DictLoader`/`FileSystemLoader`/`PackageLoader`/`PrefixLoader`/`FunctionLoader`/`ModuleLoader` (six of the catalogue's eight loader rows, `BaseLoader` abstract and `ChoiceLoader` the composition wrapper) — each a `ReportLoader` row resolving its loader value, never a parallel engine per source; `FunctionLoader(load_func)` admits a callable source provider and `ModuleLoader(module_path)` a pre-compiled-template tree, the two rows the prior fence left untapped. The `Environment`/`Environment.from_string`/`Template.render_async`/`select_autoescape`/`StrictUndefined`/`ChoiceLoader`/`DictLoader`/`FileSystemLoader`/`PackageLoader`/`PrefixLoader`/`FunctionLoader`/`ModuleLoader`/`UndefinedError`/`TemplateSyntaxError`/`TemplateNotFound`/`SecurityError` spellings verify against the folder `.api` catalogue for `jinja2` loader-axis rows `[01]`-`[08]`, engine entrypoint rows `[01]`/`[05]`, and the undefined/fault families.
- [NOTEBOOK_PARAM_GATE]: the in-memory notebook gate is `msgspec.convert(parameters, self.schema)` against the caller-declared `ParamSchema` `msgspec.Struct`, NOT `papermill.inspect_notebook` — the catalogue's `inspect_notebook(notebook_path, parameters)` row reads a notebook PATH and starts no kernel, so it cannot gate the in-memory `NotebookNode` the async boundary executes (the prior fence's `gated_parameters` computed a bare `papermill.inspect_notebook` function reference and discarded it, a dead gate). `convert` coerces and validates the supplied parameter dict against the declared field set and types, raising `msgspec.ValidationError` (a missing required field, a wrong type, or an out-of-bound value) before a kernel boots — the schema-driven gate the binding-algebra task names. `ParamSchema` carries `forbid_unknown_fields=True`, so the base is not a permissive empty bag: an unexpected key faults against any subclass and the inherited reject-extra closes the contract, while each caller subclass adds the required typed fields whose absence faults — a bare `dict[str, object]` parameter bag, which `convert` would accept wholesale, is the rejected form the closed struct replaces. The gated struct projects to a parameter dict through `msgspec.structs.asdict`, merges the papermill built-ins through `papermill.add_builtin_parameters`, and injects through `papermill.parameterize_notebook(node, ..., report_mode=False, kernel_name=plan.engine.kernel_name)` — the one injection point the catalogue's `[RAIL_LAW]` rejects hand-rolling outside, the `kernel_name` carrying the engine's configured kernel so `parameterize_notebook` resolves the matching `papermill.PapermillTranslators` row (`PythonTranslator`/`RTranslator`/`JuliaTranslator`/`ScalaTranslator`/`BashTranslator`/`CSharpTranslator`/`FSharpTranslator`) and serializes the parameter cell in the kernel's own language rather than defaulting every kernel to Python source. The `papermill.inspect_notebook(notebook_path, parameters)` row's declared-parameter introspection value — the capability the binding task names — is NOT discarded but re-homed: it reads a PATH and returns `Parameter` named tuples by starting no kernel, and the in-memory gate has no path, so `schema_fields(self.schema)` over `msgspec.structs.fields` surfaces the SAME declared-parameter contract (each `FieldInfo` carrying the parameter's `name`, `encode_name`, `type`, and `default`) off the live `ParamSchema` struct, the in-memory equivalent of the path-based inspection a caller queries before supplying parameters. The `convert`/`ValidationError`/`structs.asdict`/`structs.fields`/`structs.FieldInfo` spellings are core `msgspec` surface (the branch `.api/msgspec.md` `structs.fields` row `[01]` and the `FieldInfo` type row `[01]`); `parameterize_notebook(nb, parameters, report_mode, kernel_name, language, engine_name)`/`add_builtin_parameters(parameters)`/`PapermillTranslators`/`PapermillMissingParameterException`/`PapermillExecutionError` verify against the folder `.api` catalogue for `papermill` rows `[03]`/`[04]`, the translator-family rows, and the exception family. The path-routed `papermill.execute_notebook` is the rejected sync end-to-end call that blocks the async boundary.
- [NBCLIENT_ENGINE]: the parameterized node executes through `nbclient.NotebookClient(node, **asdict(engine)).async_execute()` — the catalogue's preferred async entrypoint over the in-memory `nbformat.NotebookNode`, returning the mutated node — where `**asdict(engine)` is the `NotebookEngine` struct projected by `msgspec.structs.asdict`, every key a verified `nbclient.NotebookClient` trait: `timeout`/`startup_timeout`/`kernel_name`/`allow_errors`/`force_raise_errors`/`record_timing`/`interrupt_on_timeout`/`coalesce_streams`/`store_widget_state`/`skip_cells_with_tag` (the configuration-trait rows `[01]`-`[15]` of the folder `.api` catalogue for `nbclient`, `store_widget_state` the `[NOTEBOOK_TOPOLOGY]` widget-state row). `record_timing=True` stamps per-cell timing into the executed node's metadata as the reproducibility timing receipt and `store_widget_state=True` captures the Jupyter widget state into the same metadata for UI replay; `interrupt_on_timeout=True` with `force_raise_errors=True` makes a runaway cell a `nbclient.exceptions.CellTimeoutError`/`CellExecutionError` and a kernel death a `DeadKernelError`, each funneled through the boundary into `BoundaryFault`, never a silent hang; `coalesce_streams=True` merges consecutive stream outputs and `skip_cells_with_tag` drops tagged cells before execution. The lifecycle-hook traits (`on_cell_execute`/`on_cell_executed`/`on_notebook_start`/`on_notebook_complete`/`on_notebook_error`, catalogue config rows `[09]`-`[13]`) stay off the frozen `NotebookEngine` because a `Callable` is not a reproducibility-receipt fact the content key keys — a hook is a per-run observation channel the runtime `observability` owns, not an engine-config value, so admitting one would smuggle a non-serializable runtime capability onto the keyed plan. A new bounded-safety trait is one `NotebookEngine` field `asdict` carries into the client with zero call-site edit; the `execute`/`async_execute`/`store_widget_state`/`CellExecutionError`/`CellTimeoutError`/`DeadKernelError` spellings verify against the folder `.api` catalogue for `nbclient` rows `[02]`/`[03]`, the config-trait table, and the exception family.
- [NBCONVERT_HTML]: the executed node lowers to document bytes through `nbconvert.get_exporter(plan.export.value)().from_notebook_node(executed)` — `get_exporter` resolves the `html`/`pdf`/`webpdf`/`latex`/`markdown`/`rst`/`asciidoc`/`slides`/`notebook`/`python`/`script` name row on the `nbconvert.exporters` registry (raising `ExporterNameError` on an unknown name), constructs the exporter, and `from_notebook_node(nb)` dispatches the in-memory `NotebookNode` returning the `(output, resources)` pair the catalogue's conversion axis fixes — the in-process path that retires the `subprocess` shell-out to the `jupyter-nbconvert` CLI. The `output` is the export-kind discriminant the `HTML_EXPORTS` frozenset partitions: a text exporter (`HTML`/`SLIDES`/`LATEX`/`MARKDOWN`/`RST`/`ASCIIDOC`/`PYTHON`/`SCRIPT`/`NOTEBOOK`) returns a `str` that `_html_page` wraps as a `BlockKind.CODE` HTML leaf feeding `document/emit#DOCUMENT PDF_HTML`, while a binary exporter (`PDF` via LaTeX, `WEBPDF` via headless Chromium) returns `bytes` that `_html_page` hex-encodes into a `BlockKind.QUOTE` leaf — the `isinstance(output, str)` partition the `body = output.encode() if isinstance(output, str) else output` line owns so a `PDF`/`WEBPDF` export never mis-renders its assembled-PDF bytes as an HTML code block (the prior fence's unconditional `_html_page(html)` wrap was the silent corruption a binary export hit). The `resources` dict is the exporter's extracted-output/extension sidecar, bound only when an exporter extracts assets (the `WEBPDF`/extraction targets), never a captured receipt fact the `(ContentKey, byte_count)` `ArtifactReceipt.Report` case cannot carry. The prior fence's `nbconvert.export(exporter, nb)` collapses to the direct `get_exporter(name)().from_notebook_node(nb)` composition (`export` only re-dispatches on the `nb` shape an in-memory node already fixes, the deleted hop). The `get_exporter(name, config)`/`Exporter.from_notebook_node(nb, resources, **kw) -> tuple[output, resources]`/`HTMLExporter`/`PDFExporter`/`WebPDFExporter`/`get_export_names`/`ExporterNameError` spellings verify against the folder `.api` catalogue for `nbconvert` module-function rows `[01]`/`[03]`, convert-method row `[02]`, and exporter classes `[03]`-`[05]`/`[17]`.
- [JUPYTEXT_SOURCE]: the `ReportSource` rows route through `jupytext.reads(text, fmt)`/`jupytext.writes(notebook, fmt)` — `ipynb`/`md:myst`/`py:percent`/`py:light`/`md`/`Rmd`/`qmd` are `fmt` rows on the one conversion pair (seven rows, `AUTO` deferring to `guess_format`), the source-vs-string distinction the `text` argument, never a per-format reader — and `AUTO` resolves the `fmt` through `jupytext.guess_format(plan.source, ".ipynb")[0]` first, the content-plus-extension detection the catalogue's detection axis owns. The executed node serializes to the audit-archive bytes through `jupytext.writes(executed, "ipynb")` on the cp315 core. The `reads(text, fmt, ...)`/`writes(notebook, fmt, ...)`/`guess_format(text, ext)` spellings verify against the folder `.api` catalogue for `jupytext` read/write rows `[02]`/`[04]` and detection row `[01]`; the `py:light`/`Rmd`/`qmd` `fmt` rows are catalogued in the package-surface capability line (`py:percent`/`py:light`/`R`/`jl` and `Rmd`/`qmd`/`myst`), the two text-source rows the prior fence left untapped.
- [REFLOW_STORY]: the `REFLOW` kind is the HTML-INTO-PDF authoring direction of the `pymupdf.Story` reflow capability, the directional inverse of the `document/lens#LENS` `STORY` `LensOp` row that recovers a `PageNode` sequence OUT of an emitted PDF — one capability, two arms split by direction across the producing and recovering owners, never one arm duplicating the other. `_reflow` runs on the runtime subprocess lane (`anyio.to_process.run_sync`, the gated `pymupdf` native-wheel band) over a held `io.BytesIO()` buffer: it opens one `pymupdf.DocumentWriter(buffer)`, builds one `pymupdf.Story(html=..., user_css=..., em=..., archive=...)`, loops `begin_page(paper_rect)` → `Story.place(rect) -> (more, filled)` → `Story.draw(device)` → `end_page()` until `more == 0`, calls `close()`, and returns `buffer.getvalue()` — the reflowed-PDF bytes the receipt keys, never a discarded sink whose bytes the prior fence lost — alongside the page count and the rect. The reflow is the one kind whose artifact is a PDF, so `_reflow_tree` keys it as `ArtifactReceipt.Pdf(reflow_key, len(pdf), count)` — the receipt owner's three-field `(ContentKey, byte_count, page_count)` PDF case — where the prior fence's `ArtifactReceipt.Report(reflow_key, len(pdf))` silently dropped the `count` the reflow loop already increments, the byte-equal-but-page-blind row a downstream page-budget audit cannot read. The paper rect resolves through `pymupdf.paper_rect(ReflowPaper(...).value)` over the named-size vocabulary, the same `_box` derivation every non-reflow `media_box` reads so the `(0, 0, 595, 842)` hand-built literal is deleted everywhere. The `Story(html, user_css, em, archive)`/`Story.place(where, flags) -> (more, filled)`/`Story.draw(device, matrix)`/`DocumentWriter(path, options)`/`DocumentWriter.begin_page(mediabox) -> Device`/`end_page()`/`close()`/`paper_rect(s) -> Rect` spellings verify against the folder `.api` catalogue for `pymupdf` story-layout rows `[01]`-`[07]` and paper-size row `[08]`. The report owns the authoring arm; the lens owns the recovering arm; neither is the other's deleted form.
- [RECEIPT_ROWS]: the bound tree and the executed-notebook archive return as `ArtifactReceipt.Report(key, byte_count)` rows and the reflowed PDF as the `ArtifactReceipt.Pdf(key, byte_count, page_count)` row the `core/receipt#RECEIPT` fold consumes — `_emit` keys the encoded `DocumentNode` tree once through `ContentIdentity.of(f"report-{self.kind}", body)` as a `Report` row, the `NOTEBOOK` kind keys the `jupytext.writes(executed, "ipynb")` archive through a second `ContentIdentity.of` as a second `Report` row, and the `REFLOW` kind keys the `buffer.getvalue()` reflowed-PDF bytes through a second `ContentIdentity.of` as the page-count-bearing `Pdf` row (the case the receipt owner declares for a PDF artifact — a `Report` row would erase the page count the loop counts), so the reproducibility artifact and the rendered output land as two keyed receipt rows on the one `RuntimeRail` payload, and the bound `DocumentNode` keys outward only as the one `compute/graduation` `HandoffAxis` model-asset case keyed by `ContentIdentity` (the existing handoff card covers it, never a private per-report handoff). The `ArtifactReceipt.Report(key, byte_count)` two-field case and the `ArtifactReceipt.Pdf(key, byte_count, page_count)` three-field case are owned by `core/receipt#RECEIPT` (cases `report`/`pdf`) and composed here, so the resolved loader/source/export row and the gated field set the `schema_fields` introspection surfaces stay recoverable from the keyed `ReportPlan` rather than a parallel fact map; the content key is consumed from runtime, never re-minted.
