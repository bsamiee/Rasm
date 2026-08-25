# [PY_ARTIFACTS_REPORT]

Reproducible-report composition binds data and visual outputs into one `document/model#NODE` `DocumentNode` tree beside the `document/emit#DOCUMENT` lowering owner. `ReportPlan` discriminates report kind over `COMPOSE_ARMS`, so `_emit` uniformly awaits one row; figures bind by content key as `FigureNode`s produced by `visualization/chart/spec#CHART`, `visualization/table#TABLE`, and `scene/render#SCENE`. `ReportPlan.matrix` fans one notebook across a parameter grid into content-keyed cell plans whose unchanged cells the `core/plan` elision replays, while grouped execution remains `core/issue`'s construction.

`ReportSpec` admits once at `ReportPlan.of` through the closed `ReportPayload` `TypedDict` and `_REQUIRED`; `TemplateTrust.UNTRUSTED` rejects `NativeEnvironment`, filesystem/package/module loaders, bytecode-cache paths, and extension imports before `ImmutableSandboxedEnvironment` renders scalar context. Every kind threads one closed `ReportFact` case onto the frozen owner, every offload crosses `lane: LanePolicy`, and provider raises convert to `BoundaryFault` at `async_boundary`. `REFLOW` is the HTML-into-PDF inverse of `document/lens#LENS` `STORY`; `AUTHOR` is its MIT/Apache `pdf_oxide` peer. PAdES finishing routes to `exchange/conformance#CONFORMANCE` and security/navigation finishing to `document/egress#FINISH`.

## [01]-[INDEX]

- [02]-[REPORT]: one composition axis over the section/template/notebook/reflow/author kinds, dispatched by the `COMPOSE_ARMS` coroutine-row table into one `DocumentNode` interior.

## [02]-[REPORT]

- Owner: `ReportPlan` — `SectionBlock` is the closed body-unit union interleaving prose, lists, figures, AND data tables IN one ordered flow, never a `Section.figures` trail parallel to the block flow; `TableData` lowers REAL cell text to a `TableNode` the audit's `THead`/`TR` nesting check reads, never a `FigureRef` pre-render flattening cells to an image; `NotebookEngine.client_kwargs` and `ExportPolicy.exporter_kwargs` project their full trait sets to constructor kwargs, so a new bounded-safety or export-shaping trait is one field with zero call-site edit.
- Cases: the `TEMPLATE` arm renders strict-undefined — a missing section key is a `jinja2.UndefinedError` fault, never a silent blank — under the trusted/sandboxed/native `Environment` policy built at boundary scope, the sandbox arm winning first so an untrusted source can never reach the native engine; the `FunctionLoader` callable provider is deleted because a callable is no serializable spec value; the `NOTEBOOK` export round-trip target is deleted because it returns a `NotebookNode` the `jupytext.writes` archive already owns; `ReportSource.AUTO` defers to `jupytext.reads(fmt=None)` content detection.
- Entry: the key mints over the canonical `(kind, spec)` input; grouped composition is `core/issue`'s construction, and `matrix` mints the per-cell plans and stops.
- Auto: `parameterize_notebook` threads `kernel_name` through `papermill.parameterize`; `nbconvert.get_exporter` resolves enabled and plugin exporters at admission; `(output, resources)` partitions on output type, so text becomes a `BlockKind.CODE` leaf while bytes remain a content-addressed `FigureNode` asset; `resources['outputs']` display figures splice through `_notebook_figures`; every non-reflow `media_box` derives from `_PAPER[spec.paper]`; REFLOW's `positionfn` sweep projects each placed element to a `Placement` row of native scalars and `_deposits` folds that stream to what a recovery reads back — heading nodes carrying their text, one link node per link occurrence — fragment boxes unioning only over a stable element id, never a bare (href, text) agreement fusing two same-target links — and a link-bearing heading nesting its link node as a child so neither fact drops — so the emitted `StructureNode` DOCUMENT root carries one `PageNode` per laid-out page (each keyed over its own deposit rows, a deposit-less page riding empty) exactly as the `document/lens#LENS` STORY and LINK arms recover off the rendered bytes page-for-page, and an id-bearing element MuPDF reports without any text deposits nothing rather than an empty node.
- Output: `_emit` returns the settled `ReportFact`; `Metrics.record` records the rendered body and notebook archive byte volume at the same fold.
- Packages: providers defer through module-scope `lazy` imports; `traitlets.config.Config` carries `ExportPolicy`; the runtime process lane isolates `REFLOW`, and the thread lane isolates GIL-releasing `AUTHOR` and blocking nbconvert renders.
- Growth: a new report kind is one `ReportKind` row with one `COMPOSE_ARMS` row and optional `_REQUIRED` row; a new section-body unit is one `SectionBlock` case with one `_block_node` arm; exporter growth arrives through the nbconvert registry; a new result modality is one `ReportFact` case; a new recoverable reflow deposit is one `Placement` field MuPDF already reports with its `_placed_node` arm; a parameter study is one `matrix` grid with its `matrix_comparison` cross-cell section.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from collections.abc import Awaitable, Callable, Iterator
from enum import StrEnum
from itertools import groupby
from typing import Annotated, Final, Literal, NotRequired, ReadOnly, Self, TypedDict, Unpack, assert_never

from builtins import frozendict
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block, Map
from expression.extra.result import traverse
import msgspec
from msgspec import UNSET, Struct, field, structs
from msgspec.json import Encoder
from msgspec.structs import asdict
from pydantic import Field, TypeAdapter, ValidationError

from rasm.artifacts.core.hooks import BYTE_VOLUME, DOMAIN, ArtifactKind, ArtifactsLeg
from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.document.model import (
    AnnotKind,
    AnnotationNode,
    BlockKind,
    BlockNode,
    DocumentNode,
    FigureNode,
    Lapse,
    ListKind,
    ListNode,
    NodeMeta,
    PageNode,
    RunNode,
    SectionNode,
    StandardRole,
    StructEltKind,
    StructureNode,
    TableNode,
    Uri,
    encode,
    lapsed,
)
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import TRANSIENT, Catch, FaultRow, RuntimeRail, async_boundary, rostered
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.metrics import Metrics
from rasm.runtime.workers import Kernel, KernelTrait

lazy import jupytext
lazy import nbconvert
lazy import pymupdf
lazy from jinja2 import (
    BaseLoader,
    ChoiceLoader,
    DictLoader,
    Environment,
    FileSystemBytecodeCache,
    FileSystemLoader,
    ModuleLoader,
    PackageLoader,
    PrefixLoader,
    StrictUndefined,
    select_autoescape,
)
lazy from jinja2.nativetypes import NativeEnvironment
lazy from jinja2.sandbox import ImmutableSandboxedEnvironment
lazy from jinja2 import TemplateError
lazy from jupytext.formats import JupytextFormatError
lazy from nbclient import NotebookClient
lazy from nbclient.exceptions import CellExecutionError, CellTimeoutError, DeadKernelError
lazy from papermill.exceptions import PapermillException
lazy from papermill.parameterize import add_builtin_parameters, parameterize_notebook
lazy from pdf_oxide import Footer, Header, OfficeConverter, PageTemplate, Pdf
lazy from traitlets.config import Config

# --- [TYPES] ----------------------------------------------------------------------------


class ReportKind(StrEnum):
    COMPOSE = "compose"
    TEMPLATE = "template"
    NOTEBOOK = "notebook"
    REFLOW = "reflow"
    AUTHOR = "author"


class TemplateRender(StrEnum):
    STRING = "string"
    NATIVE = "native"


class TemplateTrust(StrEnum):
    TRUSTED = "trusted"
    UNTRUSTED = "untrusted"


class AuthorSource(StrEnum):
    MARKDOWN = "markdown"
    HTML = "html"
    SHEET = "sheet"
    DOCX = "docx"
    PPTX = "pptx"
    XLSX = "xlsx"


class ReflowLayout(StrEnum):
    DIRECT = "direct"
    STABILIZED = "stabilized"


class ReportLoader(StrEnum):
    DICT = "dict"
    FILESYSTEM = "filesystem"
    PACKAGE = "package"
    PREFIX = "prefix"
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


class ReflowPaper(StrEnum):
    A4 = "a4"
    A3 = "a3"
    A5 = "a5"
    LETTER = "letter"
    LEGAL = "legal"


type ParamScalar = str | int | float | bool | None
type ReportExport = str
type Rect = tuple[float, float, float, float]
type ComposeArm = Callable[["ReportPlan"], Awaitable["ReportFact"]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_NATIVE: Final[Encoder] = Encoder()
_KEY_ENCODER: Final = msgspec.msgpack.Encoder(order="deterministic")
_EXCLUDE_TRAITS: Final[tuple[str, ...]] = (
    "exclude_input",
    "exclude_output",
    "exclude_input_prompt",
    "exclude_output_prompt",
    "exclude_markdown",
    "exclude_code_cell",
    "exclude_raw",
    "exclude_unknown",
)
_PAPER: Final[Map[ReflowPaper, Rect]] = Map.of_seq([
    (ReflowPaper.A4, (0.0, 0.0, 595.0, 842.0)),
    (ReflowPaper.A3, (0.0, 0.0, 842.0, 1191.0)),
    (ReflowPaper.A5, (0.0, 0.0, 420.0, 595.0)),
    (ReflowPaper.LETTER, (0.0, 0.0, 612.0, 792.0)),
    (ReflowPaper.LEGAL, (0.0, 0.0, 612.0, 1008.0)),
])
_FIGURE_MEDIA: Final[Map[str, str]] = Map.of_seq([
    ("png", "image/png"),
    ("jpg", "image/jpeg"),
    ("jpeg", "image/jpeg"),
    ("svg", "image/svg+xml"),
    ("gif", "image/gif"),
    ("webp", "image/webp"),
    ("pdf", "application/pdf"),
])

# --- [MODELS] ---------------------------------------------------------------------------


class ReportParams(TypedDict, extra_items=ParamScalar):
    pass


class NotebookEngine(Struct, frozen=True):
    timeout: int | None = 600
    startup_timeout: int = 60
    allow_errors: bool = False
    allow_error_names: tuple[str, ...] = ()
    force_raise_errors: bool = True
    record_timing: bool = True
    interrupt_on_timeout: bool = True
    error_on_timeout: frozendict[str, str] = field(default_factory=frozendict)
    iopub_timeout: int = 4
    raise_on_iopub_timeout: bool = True
    coalesce_streams: bool = True
    store_widget_state: bool = True
    skip_cells_with_tag: str = "skip-execution"
    display_data_priority: tuple[str, ...] = ()
    extra_arguments: tuple[str, ...] = ()
    shutdown_kernel: Literal["graceful", "immediate"] = "graceful"
    kernel_name: str = "python3"

    def client_kwargs(self) -> dict[str, object]:
        return {**asdict(self), "error_on_timeout": dict(self.error_on_timeout) or None}


class ExportPolicy(Struct, frozen=True):
    remove_cell_tags: tuple[str, ...] = ()
    remove_input_tags: tuple[str, ...] = ()
    remove_all_outputs_tags: tuple[str, ...] = ()
    extract_outputs: bool = False
    template_name: str = ""
    template_file: str = ""
    template_paths: tuple[str, ...] = ()
    extra_template_paths: tuple[str, ...] = ()
    raw_mimetypes: tuple[str, ...] = ()
    exclude_input: bool = False
    exclude_output: bool = False
    exclude_input_prompt: bool = False
    exclude_output_prompt: bool = False
    exclude_markdown: bool = False
    exclude_code_cell: bool = False
    exclude_raw: bool = False
    exclude_unknown: bool = False
    optimistic_validation: bool = False
    embed_images: bool = False
    sanitize_html: bool = False
    theme: str = ""
    mathjax_url: str = ""
    exclude_anchor_links: bool = False
    skip_svg_encoding: bool = False
    allow_chromium_download: bool = False
    disable_sandbox: bool = False
    page_render_timeout: int | None = None
    paginate: bool = True
    browser_args: tuple[str, ...] = ()
    latex_command: tuple[str, ...] = ()
    bib_command: tuple[str, ...] = ()
    latex_count: int | None = None
    verbose: bool = False
    reveal_theme: str = ""
    reveal_transition: str = ""
    reveal_scroll: bool = False

    def exporter_kwargs(self) -> dict[str, object]:
        bands = frozendict({
            "remove_cell_tags": tuple(dict.fromkeys(self.remove_cell_tags)),
            "remove_input_tags": tuple(dict.fromkeys(self.remove_input_tags)),
            "remove_all_outputs_tags": tuple(dict.fromkeys(self.remove_all_outputs_tags)),
        })
        active = {name: value for name, value in bands.items() if value}
        template = {
            **({"template_name": self.template_name} if self.template_name else {}),
            **({"template_file": self.template_file} if self.template_file else {}),
            **({"template_paths": list(self.template_paths)} if self.template_paths else {}),
            **({"extra_template_paths": list(self.extra_template_paths)} if self.extra_template_paths else {}),
            **({"raw_mimetypes": list(self.raw_mimetypes)} if self.raw_mimetypes else {}),
            **{name: True for name in _EXCLUDE_TRAITS if getattr(self, name)},
        }
        html = {
            **({"embed_images": True} if self.embed_images else {}),
            **({"sanitize_html": True} if self.sanitize_html else {}),
            **({"theme": self.theme} if self.theme else {}),
            **({"mathjax_url": self.mathjax_url} if self.mathjax_url else {}),
            **({"exclude_anchor_links": True} if self.exclude_anchor_links else {}),
            **({"skip_svg_encoding": True} if self.skip_svg_encoding else {}),
        }
        webpdf = {
            **({"allow_chromium_download": True} if self.allow_chromium_download else {}),
            **({"disable_sandbox": True} if self.disable_sandbox else {}),
            **({"page_render_timeout": self.page_render_timeout} if self.page_render_timeout is not None else {}),
            **({"paginate": False} if not self.paginate else {}),
            **({"browser_args": list(self.browser_args)} if self.browser_args else {}),
        }
        pdf = {
            **({"latex_command": list(self.latex_command)} if self.latex_command else {}),
            **({"bib_command": list(self.bib_command)} if self.bib_command else {}),
            **({"latex_count": self.latex_count} if self.latex_count is not None else {}),
            **({"verbose": True} if self.verbose else {}),
        }
        slides = {
            **({"reveal_theme": self.reveal_theme} if self.reveal_theme else {}),
            **({"reveal_transition": self.reveal_transition} if self.reveal_transition else {}),
            **({"reveal_scroll": True} if self.reveal_scroll else {}),
        }
        stages: dict[str, object] = {
            **({"TagRemovePreprocessor": {"enabled": True, **active}} if active else {}),
            **({"ExtractOutputPreprocessor": {"enabled": True}} if self.extract_outputs else {}),
            **({"Exporter": {"optimistic_validation": True}} if self.optimistic_validation else {}),
            **({"TemplateExporter": template} if template else {}),
            **({"HTMLExporter": html} if html else {}),
            **({"WebPDFExporter": webpdf} if webpdf else {}),
            **({"PDFExporter": pdf} if pdf else {}),
            **({"SlidesExporter": slides} if slides else {}),
        }
        return {"config": Config(stages)} if stages else {}


class FigureRef(Struct, frozen=True):
    asset_key: ContentKey
    alt: str = ""
    caption: str = ""
    media_type: str = "image/png"
    intrinsic: tuple[float, float] | None = None


class Placement(Struct, frozen=True, gc=False):
    page: int
    heading: int
    anchor: str
    href: str
    text: str
    rect: Rect


class TableData(Struct, frozen=True):
    rows: tuple[tuple[str, ...], ...] = ()
    header_rows: int = 0
    footer_rows: int = 0
    header_cols: int = 0
    spans: tuple[
        tuple[int, int, int, int], ...
    ] = ()
    caption: str = ""


@tagged_union(frozen=True)
class SectionBlock:
    tag: Literal["prose", "listing", "figure", "table"] = tag()
    prose: tuple[BlockKind, tuple[str, ...]] = case()
    listing: tuple[ListKind, tuple[str, ...]] = case()
    figure: FigureRef = case()
    table: TableData = case()


class Section(Struct, frozen=True):
    level: int
    heading: str
    blocks: tuple[SectionBlock, ...] = ()
    classification: str = ""
    children: tuple["Section", ...] = ()


@tagged_union(frozen=True)
class ReportFact:
    tag: Literal["composed", "template", "notebook", "pdf"] = tag()
    composed: tuple[DocumentNode, bytes] = case()
    template: tuple[DocumentNode, bytes, ReportLoader] = case()
    notebook: tuple[DocumentNode, bytes, bytes, int, ReportExport, str, str, tuple[str, ...], ReportSource, bool, bool] = case()
    pdf: tuple[DocumentNode, bytes, int] = case()


class ReportSpec(Struct, frozen=True, omit_defaults=True):
    source: str = ""
    sections: tuple[Section, ...] = ()
    figures: tuple[FigureRef, ...] = ()
    notebook_parameters: ReportParams = field(default_factory=dict)
    section_templates: frozendict[str, str] = field(default_factory=frozendict)
    template_roots: tuple[str, ...] = ("templates",)
    package: str = ""
    package_path: str = "templates"
    module_path: str = ""
    loader: ReportLoader = ReportLoader.DICT
    trust: TemplateTrust = TemplateTrust.UNTRUSTED
    render: TemplateRender = TemplateRender.STRING
    context: frozendict[str, ParamScalar] = field(
        default_factory=frozendict
    )
    template_globals: frozendict[str, ParamScalar] = field(
        default_factory=frozendict
    )
    bytecode_cache: str = ""
    extensions: tuple[str, ...] = ()
    report_source: ReportSource = ReportSource.IPYNB
    resource_path: str = ""
    engine: NotebookEngine = field(default_factory=NotebookEngine)
    export: ReportExport = "html"
    export_policy: ExportPolicy = field(default_factory=ExportPolicy)
    paper: ReflowPaper = ReflowPaper.A4
    user_css: str = ""
    em: float = 12.0
    layout: ReflowLayout = ReflowLayout.DIRECT
    author_source: AuthorSource = AuthorSource.MARKDOWN
    title: str = ""
    author: str = ""
    header: str = ""
    footer: str = ""
    toc: bool = True
    toc_title: str = "Contents"


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class ReportFault:
    tag: Literal["payload", "unsatisfied", "irrelevant", "sandboxed_native", "unsafe", "export"] = tag()
    payload: tuple[str, ...] = case()
    unsatisfied: tuple[ReportKind, str] = case()
    irrelevant: tuple[ReportKind, tuple[str, ...]] = case()
    sandboxed_native: None = case()
    unsafe: tuple[str, str] = case()
    export: str = case()


# --- [BOUNDARIES] -----------------------------------------------------------------------


class ReportPayload(TypedDict, closed=True):
    source: NotRequired[ReadOnly[str]]
    sections: NotRequired[ReadOnly[tuple[Section, ...]]]
    figures: NotRequired[ReadOnly[tuple[FigureRef, ...]]]
    notebook_parameters: NotRequired[ReadOnly[ReportParams]]
    section_templates: NotRequired[ReadOnly[frozendict[str, str]]]
    template_roots: NotRequired[ReadOnly[tuple[str, ...]]]
    package: NotRequired[ReadOnly[str]]
    package_path: NotRequired[ReadOnly[str]]
    module_path: NotRequired[ReadOnly[str]]
    loader: NotRequired[ReadOnly[ReportLoader]]
    trust: NotRequired[ReadOnly[TemplateTrust]]
    render: NotRequired[ReadOnly[TemplateRender]]
    context: NotRequired[ReadOnly[frozendict[str, ParamScalar]]]
    template_globals: NotRequired[ReadOnly[frozendict[str, ParamScalar]]]
    bytecode_cache: NotRequired[ReadOnly[str]]
    extensions: NotRequired[ReadOnly[tuple[str, ...]]]
    report_source: NotRequired[ReadOnly[ReportSource]]
    resource_path: NotRequired[ReadOnly[str]]
    engine: NotRequired[ReadOnly[NotebookEngine]]
    export: NotRequired[ReadOnly[ReportExport]]
    export_policy: NotRequired[ReadOnly[ExportPolicy]]
    paper: NotRequired[ReadOnly[ReflowPaper]]
    user_css: NotRequired[ReadOnly[str]]
    em: NotRequired[ReadOnly[Annotated[float, Field(gt=0, allow_inf_nan=False)]]]
    layout: NotRequired[ReadOnly[ReflowLayout]]
    author_source: NotRequired[ReadOnly[AuthorSource]]
    title: NotRequired[ReadOnly[str]]
    author: NotRequired[ReadOnly[str]]
    header: NotRequired[ReadOnly[str]]
    footer: NotRequired[ReadOnly[str]]
    toc: NotRequired[ReadOnly[bool]]
    toc_title: NotRequired[ReadOnly[str]]


_PAYLOAD: Final = TypeAdapter(ReportPayload)
_REQUIRED: Final[Map[ReportKind, tuple[str, ...]]] = Map.of_seq([
    (ReportKind.COMPOSE, ("sections",)),
    (ReportKind.TEMPLATE, ("source",)),
    (ReportKind.NOTEBOOK, ("source",)),
    (ReportKind.REFLOW, ("source",)),
    (ReportKind.AUTHOR, ("source",)),
])
_ALLOWED: Final[Map[ReportKind, frozenset[str]]] = Map.of_seq([
    (ReportKind.COMPOSE, frozenset({"sections", "paper", "toc", "toc_title"})),
    (
        ReportKind.TEMPLATE,
        frozenset({
            "source",
            "sections",
            "figures",
            "section_templates",
            "template_roots",
            "package",
            "package_path",
            "module_path",
            "loader",
            "trust",
            "render",
            "context",
            "template_globals",
            "bytecode_cache",
            "extensions",
            "paper",
        }),
    ),
    (
        ReportKind.NOTEBOOK,
        frozenset({"source", "notebook_parameters", "trust", "report_source", "resource_path", "engine", "export", "export_policy", "paper"}),
    ),
    (ReportKind.REFLOW, frozenset({"source", "paper", "user_css", "em", "layout"})),
    (ReportKind.AUTHOR, frozenset({"source", "paper", "author_source", "title", "author", "header", "footer"})),
])

# --- [OPERATIONS] -----------------------------------------------------------------------


def _meta(role: str, label: str, path: tuple[int, ...], classification: str = "", /, *, bounds: Rect | None = None) -> NodeMeta:
    trail = "-".join(map(str, path)) or "root"
    return NodeMeta(
        key=ContentIdentity.key(f"report-{role}-{trail}", label.encode()),
        role=role,
        page=path[0] if path else 0,
        bounds=bounds,
        classification=classification or UNSET,
    )


def _runs(role: str, path: tuple[int, ...], *lines: str, size: float = 11.0, bounds: Rect | None = None) -> tuple[RunNode, ...]:
    return tuple(
        RunNode(meta=_meta(f"{role}-run", line, path, bounds=bounds), text=line, font_key="body", size=size) for line in lines if line
    )


def _deposits(stream: tuple[Placement, ...], /) -> Iterator[Placement]:
    keyed = groupby(
        enumerate(stream),
        key=lambda slot: (slot[1].page, slot[1].anchor or f"\x00{slot[0]}", slot[1].href, slot[1].heading, slot[1].text),
    )
    for _key, group in keyed:
        run = tuple(placed for _ordinal, placed in group)
        head = run[0]
        if head.heading or head.href:
            boxes = tuple(placed.rect for placed in run)
            yield structs.replace(
                head, rect=(min(b[0] for b in boxes), min(b[1] for b in boxes), max(b[2] for b in boxes), max(b[3] for b in boxes))
            )


def _placed_node(ordinal: int, placed: Placement, em: float, /) -> DocumentNode:
    path, label = (placed.page, ordinal), f"{placed.anchor}\x1f{placed.href}\x1f{placed.text}"
    linked = (
        (
            AnnotationNode(
                meta=_meta("reflow-link", label, path, bounds=placed.rect),
                annot=AnnotKind.LINK,
                target=placed.rect,
                contents=placed.text,
                link=Uri(href=placed.href),
            ),
        )
        if placed.href
        else ()
    )
    match placed:
        case Placement(heading=level) if level:
            return SectionNode(
                meta=_meta("reflow-heading", label, path, bounds=placed.rect),
                level=level,
                heading=_runs("reflow-heading", path, placed.text, size=em, bounds=placed.rect),
                children=linked,
            )
        case _:
            return linked[0]


def _block_node(path: tuple[int, ...], block: SectionBlock, /) -> DocumentNode:
    match block:
        case SectionBlock(tag="prose", prose=(kind, lines)):
            return BlockNode(meta=_meta("block", kind.value, path), block=kind, runs=_runs("body", path, *lines))
        case SectionBlock(tag="listing", listing=(list_kind, items)):
            return ListNode(
                meta=_meta("list", list_kind.value, path),
                list_kind=list_kind,
                items=tuple(
                    BlockNode(meta=_meta("item", item, (*path, ordinal)), block=BlockKind.PARAGRAPH, runs=_runs("item", (*path, ordinal), item))
                    for ordinal, item in enumerate(items)
                ),
            )
        case SectionBlock(tag="figure", figure=ref):
            return FigureNode(
                meta=_meta("figure", ref.alt, path),
                asset_key=ref.asset_key,
                alt=ref.alt,
                media_type=ref.media_type,
                intrinsic=ref.intrinsic,
                caption=_runs("caption", path, ref.caption),
            )
        case SectionBlock(
            tag="table", table=data
        ):
            return TableNode(
                meta=_meta("table", data.caption or "table", path),
                rows=tuple(
                    tuple(
                        BlockNode(
                            meta=_meta("cell", cell, (*path, row_index, col_index)),
                            block=BlockKind.PARAGRAPH,
                            runs=_runs("cell", (*path, row_index, col_index), cell),
                        )
                        for col_index, cell in enumerate(row)
                    )
                    for row_index, row in enumerate(data.rows)
                ),
                spans=data.spans,
                header_rows=data.header_rows,
                footer_rows=data.footer_rows,
                header_cols=data.header_cols,
                caption=_runs("caption", path, data.caption),
            )
        case _ as unreachable:
            assert_never(unreachable)


def _section_node(path: tuple[int, ...], section: Section, /) -> SectionNode:
    blocks = tuple(_block_node((*path, index), block) for index, block in enumerate(section.blocks))
    kids = tuple(_section_node((*path, len(blocks) + index), child) for index, child in enumerate(section.children))
    return SectionNode(
        meta=_meta("section", section.heading, path, section.classification),
        level=section.level,
        heading=_runs("heading", path, section.heading),
        children=(*blocks, *kids),
    )


def _toc(title: str, sections: tuple[Section, ...], /) -> SectionNode:
    return SectionNode(
        meta=_meta("toc", title, (0,)),
        level=1,
        heading=_runs("toc-heading", (0,), title),
        children=(
            ListNode(
                meta=_meta("toc-list", title, (0,)),
                list_kind=ListKind.ORDERED,
                items=tuple(
                    BlockNode(
                        meta=_meta("toc-entry", section.heading, (index,)),
                        block=BlockKind.PARAGRAPH,
                        runs=_runs("toc-item", (index,), section.heading),
                    )
                    for index, section in enumerate(sections)
                ),
            ),
        ),
    )


def _output_page(
    spec: ReportSpec,
    role: str,
    output: str | bytes,
    figures: tuple[FigureNode, ...] = (),
    media_type: str = "application/octet-stream",
    /,
) -> PageNode:
    primary: DocumentNode = (
        BlockNode(meta=_meta(role, role, (0,)), block=BlockKind.CODE, runs=_runs(role, (0,), output))
        if isinstance(output, str)
        else FigureNode(
            meta=_meta(f"{role}-output", role, (0,)),
            asset_key=ContentIdentity.key(f"report-{role}-output", output),
            alt=f"{role} export",
            media_type=media_type,
        )
    )
    return PageNode(
        meta=_meta("page", role, (0,)),
        media_box=_PAPER[spec.paper],
        children=(primary, *figures),
    )


def _notebook_figures(resources: object, /) -> tuple[FigureNode, ...]:
    outputs: dict[str, bytes] = resources.get("outputs", {}) if isinstance(resources, dict) else {}
    return tuple(
        FigureNode(
            meta=_meta("nb-figure", name, (0, ordinal)),
            asset_key=ContentIdentity.key(f"report-notebook-figure-{name}", data),
            alt=name,
            media_type=_FIGURE_MEDIA.try_find(name.rpartition(".")[2].lower()).default_value("image/png"),
        )
        for ordinal, (name, data) in enumerate(outputs.items())
    )


async def _compose_arm(plan: "ReportPlan") -> "ReportFact":
    spec = plan.spec
    outline = (_toc(spec.toc_title, spec.sections),) if spec.toc else ()
    page = PageNode(
        meta=_meta("page", plan.kind.value, (0,)),
        media_box=_PAPER[spec.paper],
        children=(*outline, *(_section_node((len(outline) + index,), section) for index, section in enumerate(spec.sections))),
    )
    return ReportFact(composed=(page, encode(page)))


def _loader(spec: ReportSpec, /) -> BaseLoader:
    match spec.loader:
        case ReportLoader.DICT:
            return DictLoader({"<root>": spec.source, **spec.section_templates})
        case ReportLoader.FILESYSTEM:
            return FileSystemLoader(list(spec.template_roots))
        case ReportLoader.PACKAGE:
            return PackageLoader(spec.package, spec.package_path)
        case ReportLoader.PREFIX:
            return PrefixLoader({prefix: DictLoader({"<root>": source}) for prefix, source in spec.section_templates.items()})
        case ReportLoader.MODULE:
            return ModuleLoader(spec.module_path)
        case _ as unreachable:
            assert_never(unreachable)


def _environment(spec: ReportSpec, /) -> Environment:
    factory = (
        ImmutableSandboxedEnvironment
        if spec.trust is TemplateTrust.UNTRUSTED
        else NativeEnvironment if spec.render is TemplateRender.NATIVE else Environment
    )
    env = factory(
        loader=ChoiceLoader([_loader(spec), DictLoader({})]),
        extensions=list(spec.extensions),
        autoescape=select_autoescape(enabled_extensions=("html", "xml")),
        undefined=StrictUndefined,
        enable_async=True,
        trim_blocks=True,
        lstrip_blocks=True,
        bytecode_cache=FileSystemBytecodeCache(spec.bytecode_cache) if spec.bytecode_cache else None,
    )
    env.filters.update(dict(_REPORT_FILTERS.items()))
    env.globals.update(dict(spec.template_globals))
    env.policies["json.dumps_kwargs"] = {"sort_keys": True}
    return env


async def _template_arm(plan: "ReportPlan") -> "ReportFact":
    spec = plan.spec
    output = await _environment(spec).from_string(spec.source).render_async(sections=spec.sections, figures=spec.figures, **dict(spec.context))
    match spec.render:
        case TemplateRender.STRING:
            page = _output_page(spec, "template", output)
        case TemplateRender.NATIVE:
            page = _output_page(spec, "template", _NATIVE.encode(output).decode())
        case _ as unreachable:
            assert_never(unreachable)
    return ReportFact(template=(page, encode(page), spec.loader))


def _exported(spec: ReportSpec, executed: object, /) -> tuple[object, object, str]:
    exporter = nbconvert.get_exporter(spec.export)(**spec.export_policy.exporter_kwargs())
    output, resources = exporter.from_notebook_node(executed)
    return output, resources, type(exporter).__qualname__


def _resources(spec: ReportSpec, /) -> dict[str, object]:
    return {"metadata": {"path": spec.resource_path}} if spec.resource_path else {}


async def _notebook_arm(plan: "ReportPlan") -> "ReportFact":
    spec = plan.spec
    fmt = None if spec.report_source is ReportSource.AUTO else spec.report_source.value
    node = jupytext.reads(spec.source, fmt=fmt)
    parameterized = parameterize_notebook(
        node, add_builtin_parameters(dict(spec.notebook_parameters)), report_mode=False, kernel_name=spec.engine.kernel_name
    )
    executed = await NotebookClient(parameterized, resources=_resources(spec), **spec.engine.client_kwargs()).async_execute()
    archive = jupytext.writes(executed, fmt="ipynb").encode()
    exported = await plan.lane.offload(Kernel.of(_exported, KernelTrait.RELEASING), spec, executed)
    output, resources, exporter_type = exported.default_with(lapsed)
    if not isinstance(output, str | bytes):
        raise TypeError(f"exporter {spec.export!r} returned {type(output).__name__}, expected str or bytes")
    figures = _notebook_figures(resources)
    extension = str(resources.get("output_extension", "")) if isinstance(resources, dict) else ""
    media_type = _FIGURE_MEDIA.try_find(extension.removeprefix(".").lower()).default_value("application/octet-stream")
    page = _output_page(spec, "notebook", output, figures, media_type)
    body = encode(page) if isinstance(output, str) else output
    return ReportFact(notebook=(
        page,
        body,
        archive,
        len(figures),
        spec.export,
        exporter_type,
        extension,
        tuple(sorted(resources)) if isinstance(resources, dict) else (),
        spec.report_source,
        spec.engine.record_timing,
        spec.engine.store_widget_state,
    ))


async def _reflow_arm(plan: "ReportPlan") -> "ReportFact":
    spec = plan.spec
    crossed = await plan.lane.offload(Kernel.of(_reflow, KernelTrait.HOSTILE), spec.source, spec.user_css, spec.em, spec.paper.value, spec.layout)
    pdf, count, box, placed = crossed.default_with(lapsed)
    deposits = tuple(enumerate(_deposits(placed)))
    pages = tuple(
        PageNode(
            meta=NodeMeta(
                key=ContentIdentity.key(
                    f"report-{plan.kind.value}-page-{index}",
                    _KEY_ENCODER.encode(tuple(item for _ordinal, item in deposits if item.page == index)),
                ),
                role="reflow-page",
                page=index,
                bounds=box,
            ),
            media_box=box,
            children=tuple(_placed_node(ordinal, item, spec.em) for ordinal, item in deposits if item.page == index),
        )
        for index in range(count)
    )
    root = StructureNode(
        meta=NodeMeta(key=ContentIdentity.key(f"report-{plan.kind.value}", pdf), role="reflow", page=0, bounds=box),
        role=StandardRole(elt=StructEltKind.DOCUMENT),
        children=pages,
    )
    return ReportFact(pdf=(root, pdf, count))


def _reflow(html: str, user_css: str, em: float, paper: str, layout: ReflowLayout) -> tuple[bytes, int, Rect, tuple[Placement, ...]]:
    rect = pymupdf.paper_rect(paper)
    buffer = io.BytesIO()
    writer = pymupdf.DocumentWriter(buffer)
    positions: list[object] = []
    placed: list[Placement] = []
    pages = 0

    def rectfn(rect_number: int, filled: object, /) -> tuple[object, object, None]:
        return (rect, rect, None)

    def pagefn(page_number: int, mediabox: object, device: object, after: int, /) -> None:
        nonlocal pages
        if after:
            pages = page_number + 1

    def positionfn(position: object, /) -> None:
        positions.append(position)
        if position.open_close & 1:
            placed.append(
                Placement(
                    page=position.page_num - 1,
                    heading=position.heading,
                    anchor=position.id or "",
                    href=position.href or "",
                    text=position.text or "",
                    rect=tuple(position.rect),
                )
            )

    try:
        match layout:
            case ReflowLayout.STABILIZED:
                pymupdf.Story.write_stabilized(
                    writer,
                    lambda _prev: html,
                    rectfn,
                    user_css=user_css or None,
                    em=em,
                    positionfn=positionfn,
                    pagefn=pagefn,
                    add_header_ids=True,
                )
            case ReflowLayout.DIRECT:
                pymupdf.Story(html=html, user_css=user_css or None, em=em).write(
                    writer, rectfn, positionfn=positionfn, pagefn=pagefn
                )
            case _ as unreachable:
                assert_never(unreachable)
    finally:
        writer.close()
    return pymupdf.Story.add_pdf_links(buffer, positions).tobytes(), pages, tuple(rect), tuple(placed)


async def _author_arm(plan: "ReportPlan") -> "ReportFact":
    spec = plan.spec
    crossed = await plan.lane.offload(Kernel.of(_authored, KernelTrait.RELEASING), spec)
    pdf, count = crossed.default_with(lapsed)
    key = ContentIdentity.key(f"report-{plan.kind.value}", pdf)
    page = PageNode(meta=NodeMeta(key=key, role="author", page=0), media_box=_PAPER[spec.paper])
    return ReportFact(pdf=(page, pdf, count))


def _authored(spec: ReportSpec, /) -> tuple[bytes, int]:
    pdf = _AUTHOR_BUILD[spec.author_source](spec)
    return pdf.to_bytes(), len(pdf)


# --- [TABLES] ---------------------------------------------------------------------------

_REPORT_FILTERS: Final[Map[str, Callable[..., str]]] = Map.of_seq([
    ("sigfig", lambda value, digits=3: f"{float(value):.{digits}g}"),
    ("si", lambda value, unit="": f"{float(value):.3g} {unit}".rstrip()),
    ("ratio_pct", lambda value: f"{float(value) * 100:.1f}%"),
    ("dimension", lambda value, unit="mm": f"{float(value):.0f} {unit}"),
])

_AUTHOR_BUILD: Final[Map[AuthorSource, Callable[[ReportSpec], Pdf]]] = Map.of_seq([
    (AuthorSource.MARKDOWN, lambda spec: Pdf.from_markdown(spec.source, title=spec.title or None, author=spec.author or None)),
    (AuthorSource.HTML, lambda spec: Pdf.from_html(spec.source, title=spec.title or None, author=spec.author or None)),
    (AuthorSource.SHEET, lambda spec: Pdf.from_markdown_with_template(
        spec.source,
        PageTemplate().header(Header.center(spec.header)).footer(Footer.center(spec.footer)),
        title=spec.title or None,
        author=spec.author or None,
    )),
    (AuthorSource.DOCX, lambda spec: OfficeConverter.from_docx(spec.source)),
    (AuthorSource.PPTX, lambda spec: OfficeConverter.from_pptx(spec.source)),
    (AuthorSource.XLSX, lambda spec: OfficeConverter.from_xlsx(spec.source)),
])

REPORT_COMPOSE: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.REPORT, point="compose", arm="boundary", defect="compose-fold", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[ArtifactsLeg]]] = rostered(Block.of_seq([REPORT_COMPOSE]))


def _compose_raises() -> Catch:
    return (
        Lapse, TemplateError, JupytextFormatError, PapermillException,
        CellTimeoutError, CellExecutionError, DeadKernelError, TypeError, ValueError, OSError,
    )


COMPOSE_ARMS: Final[Map[ReportKind, ComposeArm]] = Map.of_seq([
    (ReportKind.COMPOSE, _compose_arm),
    (ReportKind.TEMPLATE, _template_arm),
    (ReportKind.NOTEBOOK, _notebook_arm),
    (ReportKind.REFLOW, _reflow_arm),
    (ReportKind.AUTHOR, _author_arm),
])

# --- [COMPOSITION] ----------------------------------------------------------------------


class ReportPlan(Struct, frozen=True):
    kind: ReportKind
    lane: LanePolicy
    spec: ReportSpec = field(default_factory=ReportSpec)
    fact: ReportFact | None = None

    async def _composed(self) -> Self:
        return structs.replace(self, fact=await COMPOSE_ARMS[self.kind](self))

    def emit(self, /) -> ArtifactWork[ReportFact]:
        key = self._key
        return ArtifactWork(key=key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        return ContentIdentity.key(f"report-{self.kind.value}", _KEY_ENCODER.encode((self.kind, self.spec)))

    async def _emit(self) -> RuntimeRail[ReportFact]:
        match await async_boundary(REPORT_COMPOSE, self._composed, catch=_compose_raises()):
            case Result(tag="ok", ok=done):
                assert done.fact is not None
                fact = done.fact
                match fact:
                    case ReportFact(tag="pdf", pdf=(_node, body, _pages)):
                        kind: ArtifactKind = "pdf"
                        size = len(body)
                    case ReportFact(tag="composed", composed=(_node, body)) | ReportFact(
                        tag="template", template=(_node, body, _loader)
                    ):
                        kind, size = "report", len(body)
                    case ReportFact(
                        tag="notebook",
                        notebook=(_node, body, archive, _figures, _export, _exporter, _extension, _resources, _source, _timed, _widgets),
                    ):
                        kind, size = "report", len(body) + len(archive)
                    case _ as unreachable:
                        assert_never(unreachable)
                Metrics.record({BYTE_VOLUME: float(size)}, domain=DOMAIN, kind=kind, scope=self.lane.scope)
                return Ok(fact)
            case refused:
                return Error(refused.error)

    @classmethod
    def of(cls, kind: ReportKind, /, *, lane: LanePolicy, **raw: Unpack[ReportPayload]) -> Result[Self, ReportFault]:
        try:
            payload = _PAYLOAD.validate_python(raw, strict=True)
        except ValidationError as fault:
            return Error(ReportFault(payload=tuple(str(error["loc"]) for error in fault.errors())))
        irrelevant = tuple(sorted(set(payload) - _ALLOWED[kind]))
        if irrelevant:
            return Error(ReportFault(irrelevant=(kind, irrelevant)))
        spec = ReportSpec(**payload)
        if kind is ReportKind.NOTEBOOK and spec.trust is TemplateTrust.UNTRUSTED:
            return Error(ReportFault(unsafe=("notebook", "kernel-execution")))
        if spec.render is TemplateRender.NATIVE and spec.trust is TemplateTrust.UNTRUSTED:
            return Error(ReportFault(sandboxed_native=None))
        if spec.trust is TemplateTrust.UNTRUSTED and spec.loader not in (ReportLoader.DICT, ReportLoader.PREFIX):
            return Error(ReportFault(unsafe=("loader", spec.loader.value)))
        if spec.trust is TemplateTrust.UNTRUSTED and spec.bytecode_cache:
            return Error(ReportFault(unsafe=("bytecode_cache", spec.bytecode_cache)))
        if spec.trust is TemplateTrust.UNTRUSTED and spec.extensions:
            return Error(ReportFault(unsafe=("extension", spec.extensions[0])))
        if kind is ReportKind.NOTEBOOK:
            try:
                nbconvert.get_exporter(spec.export)
            except (nbconvert.exporters.ExporterNameError, nbconvert.exporters.ExporterDisabledError):
                return Error(ReportFault(export=spec.export))
            if spec.export == "notebook":
                return Error(ReportFault(export=spec.export))
        missing = next((name for name in _REQUIRED.try_find(kind).default_value(()) if not getattr(spec, name)), None)
        return Error(ReportFault(unsatisfied=(kind, missing))) if missing else Ok(cls(kind=kind, lane=lane, spec=spec))

    @classmethod
    def matrix(cls, grid: tuple[ReportParams, ...], /, *, lane: LanePolicy, **raw: Unpack[ReportPayload]) -> Result[Block[Self], ReportFault]:
        base = raw.get("notebook_parameters", {})
        return traverse(
            lambda cell: cls.of(ReportKind.NOTEBOOK, lane=lane, **{**raw, "notebook_parameters": {**base, **cell}}),
            Block.of_seq(grid),
        )

    @classmethod
    def matrix_comparison(
        cls, grid: tuple[ReportParams, ...], cells: tuple[ContentKey, ...], /, *, lane: LanePolicy, title: str = "Parameter matrix"
    ) -> Result[Self, "ReportFault"]:
        if len(grid) != len(cells):
            return Error(ReportFault(unsatisfied=(ReportKind.COMPOSE, "matrix grid and cell keys differ in length")))
        names = tuple(sorted({name for cell in grid for name in cell}))
        rows = (
            (*names, "artifact key"),
            *((*(str(cell.get(name, "")) for name in names), key.hex) for cell, key in zip(grid, cells, strict=True)),
        )
        table = SectionBlock(table=TableData(rows=rows, header_rows=1, header_cols=0, caption=title))
        return cls.of(ReportKind.COMPOSE, lane=lane, sections=(Section(level=1, heading=title, blocks=(table,)),))

```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
