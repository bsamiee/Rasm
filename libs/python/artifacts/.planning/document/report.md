# [PY_ARTIFACTS_REPORT]

The reproducible-report composition layer binding data and visual outputs into the one `document/model#NODE` `DocumentNode` tree — the composition peer of the `document/emit#DOCUMENT` lowering owner. `ReportPlan` discriminates report kind over the `COMPOSE_ARMS` coroutine-row table, so `_emit` is one uniform `await COMPOSE_ARMS[self.kind](self)` with zero branch, and every kind PRODUCES the tree, never a string. It owns neither the visual nor the codec: figures bind by content key as `FigureNode`s — produced by `visualization/chart#CHART`, `visualization/table#TABLE`, and `scene#SCENE`, never re-rendered — and the produced tree hands to `document/emit#DOCUMENT` for lowering.

`ReportSpec` admits exactly once at `ReportPlan.of` through the closed `ReportPayload` `TypedDict` under the per-kind `_REQUIRED` precondition; every kind threads one `ReportFact` evidence carrier onto the frozen owner, the `@receipted(OPEN)` weave drains `contribute` off the stepped owner, and arm-level provider raises convert to the runtime `BoundaryFault` at the `async_boundary` capsule. The `REFLOW` kind is this owner's HTML-into-PDF authoring half — the directional inverse of the `document/lens#LENS` `STORY` recover-out-of-PDF arm, two arms of one `pymupdf.Story` capability split by direction, not a duplicate; the `AUTHOR` kind is its MIT/Apache `pdf_oxide` license-clean peer. PAdES finishing routes to `../exchange/conformance#CONFORM` and the security-navigation close to `document/egress#FINISH`.

## [01]-[INDEX]

- [01]-[REPORT]: the one composition axis over the section/template/notebook/reflow/author kinds, dispatched by the `COMPOSE_ARMS` coroutine-row table into one `DocumentNode` interior.

## [02]-[REPORT]

- Owner: `ReportPlan` — `SectionBlock` is the closed body-unit union interleaving prose, lists, figures, AND data tables IN one ordered flow, never a `Section.figures` trail parallel to the block flow; `TableData` lowers REAL cell text to a `TableNode` the audit's `THead`/`TR` nesting check reads, never a `FigureRef` pre-render flattening cells to an image; `NotebookEngine.client_kwargs` and `ExportPolicy.exporter_kwargs` project their full trait sets to constructor kwargs, so a new bounded-safety or export-shaping trait is one field with zero call-site edit.
- Cases: the `TEMPLATE` arm renders strict-undefined — a missing section key is a `jinja2.UndefinedError` fault, never a silent blank — under the trusted/sandboxed/native `Environment` policy built at boundary scope; the `FunctionLoader` callable provider is deleted because a callable is no serializable spec value; the `NOTEBOOK` export round-trip target is deleted because it returns a `NotebookNode` the `jupytext.writes` archive already owns; `ReportSource.AUTO` defers to `jupytext.reads(fmt=None)` content detection.
- Entry: the key mints PRE-RUN over the canonical `(kind, spec)` input with `receipt.slot == node.key`; grouped composition is `core/issue`'s construction, never a module batch driver here.
- Auto: `parameterize_notebook` threads `kernel_name` so a non-`python3` kernel serializes its parameter cell through its own `PapermillTranslators` row — and the symbol lives at `papermill.parameterize`, not the package top level; the exporter's `(output, resources)` pair partitions on the output's OWN type — a `str` wraps as a `BlockKind.CODE` HTML leaf, `PDF`/`WEBPDF` `bytes` as a `BlockKind.QUOTE` hex leaf — so a binary export never mis-renders as HTML text; the `resources['outputs']` display figures splice through `_notebook_figures` into content-keyed `FigureNode`s beside the leaf, so a notebook chart survives into the tree rather than vanishing with a discarded sidecar; every non-reflow `media_box` derives from `_box(spec.paper)` over `pymupdf.paper_rect`, never a hand-built rect literal.
- Receipt: `contribute` reads the threaded `ReportFact` off `self.fact`, never an in-process re-run of an arm; the `REFLOW` kind mints the page-count-bearing `ArtifactReceipt.Pdf` case — a `Report` row silently drops the page count the reflow loop already counts; the `NOTEBOOK` kind keys the rendered tree and the `jupytext` archive as two `Report` rows. Rich per-render evidence rides the `ReportFact` carrier; a richer receipt case is a `core/receipt#RECEIPT` growth concern, never minted here against a case that does not exist.
- Packages: every provider defers to first use through the module-scope `lazy` import; `traitlets.config.Config` carries the `ExportPolicy` exporter projection; the runtime process lane gates the `REFLOW` subprocess work.
- Growth: a new report kind is one `ReportKind` row plus one `COMPOSE_ARMS` coroutine-row plus one `_REQUIRED` row when it demands an input; a new section-body unit is one `SectionBlock` case plus one `_block_node` arm; a new loader root, text source, export target, or paper size is one vocabulary row; a new evidence scalar is one `ReportFact` field.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from collections.abc import Awaitable, Callable, Iterable
from copy import replace
from enum import StrEnum
from typing import Final, Literal, NotRequired, ReadOnly, Self, TypedDict, Unpack, assert_never

from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import UNSET, Struct, field
from msgspec.json import Encoder
from msgspec.structs import asdict
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.receipts import OPEN, Receipt, receipted

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt
from artifacts.document.model import (
    BlockKind,
    BlockNode,
    DocumentNode,
    FigureNode,
    ListKind,
    ListNode,
    NodeMeta,
    PageNode,
    RunNode,
    SectionNode,
    TableNode,
    encode,
)

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
lazy from nbclient import NotebookClient
lazy from papermill.parameterize import add_builtin_parameters, parameterize_notebook
lazy from pdf_oxide import Footer, Header, OfficeConverter, PageTemplate, Pdf
lazy from traitlets.config import Config

# --- [TYPES] ----------------------------------------------------------------------------


class ReportKind(StrEnum):
    COMPOSE = "compose"
    TEMPLATE = "template"
    NOTEBOOK = "notebook"
    REFLOW = "reflow"  # AGPL pymupdf.Story HTML-into-PDF authoring
    AUTHOR = "author"  # MIT/Apache pdf_oxide markdown/HTML/office authoring — the commercial-safe REFLOW peer


class TemplateRender(StrEnum):
    STRING = "string"  # jinja `Environment` -> HTML `BlockKind.CODE` leaf
    NATIVE = "native"  # jinja `NativeEnvironment` -> a computed Python value -> deterministic structured-data leaf


class AuthorSource(StrEnum):
    # the pdf_oxide authoring-source vocabulary; each source's `Pdf`/`OfficeConverter` build entry lives in `_AUTHOR_BUILD`.
    MARKDOWN = "markdown"
    HTML = "html"
    SHEET = "sheet"  # markdown + a running-header/footer `PageTemplate` — the AEC titled-sheet report
    DOCX = "docx"  # `OfficeConverter.from_docx(path)` office-source report
    PPTX = "pptx"
    XLSX = "xlsx"


class ReflowLayout(StrEnum):
    DIRECT = "direct"  # `Story.write` one-shot page sweep
    STABILIZED = "stabilized"  # `Story.write_stabilized` re-lays regenerated HTML until stable, its `add_header_ids=True` default injecting navigable header anchors — the TOC/cross-reference-convergence layout


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


class ReportExport(StrEnum):
    HTML = "html"
    PDF = "pdf"
    WEBPDF = "webpdf"
    LATEX = "latex"
    MARKDOWN = "markdown"
    RST = "rst"
    ASCIIDOC = "asciidoc"
    SLIDES = "slides"
    PYTHON = "python"
    SCRIPT = "script"


class ReflowPaper(StrEnum):
    A4 = "a4"
    A3 = "a3"
    A5 = "a5"
    LETTER = "letter"
    LEGAL = "legal"


type ParamScalar = str | int | float | bool | None
type Rect = tuple[float, float, float, float]
type ComposeArm = Callable[["ReportPlan"], Awaitable["ReportFact"]]

# --- [CONSTANTS] ------------------------------------------------------------------------

# the TOTAL deterministic encoder the `NATIVE` template render lowers a computed Python value through; `enc_hook=str`
# never raises on an unsupported type, so a derived report datum crosses into the keyed body without an interior raise.
_NATIVE: Final[Encoder] = Encoder(enc_hook=str)
# the `TemplateExporter` content-exclusion traits `ExportPolicy.exporter_kwargs` projects as top-level `**kw`
# (canonical field name == nbconvert trait name, the boundary correspondence), each admitted only when its flag is set.
_EXCLUDE_TRAITS: Final[tuple[str, ...]] = ("exclude_input", "exclude_output", "exclude_input_prompt", "exclude_output_prompt")
# the display-output MIME map the notebook-figure splice keys each extracted `resources['outputs']` filename by suffix.
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


# the notebook-parameter value space: any name, every value a kernel-serializable `ParamScalar`,
# admitted once through `_PAYLOAD`. A notebook with a strict declared-parameter set subclasses with
# `Required[]` keys; the bare band rejects a non-scalar value `papermill` could not codify.
class ReportParams(TypedDict, extra_items=ParamScalar):
    pass


class NotebookEngine(Struct, frozen=True):
    # the lifecycle-hook traits (`on_cell_executed`/...) stay off: a `Callable` is a per-run observation channel the
    # runtime observability owner holds, never a serializable reproducibility fact the keyed plan carries.
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
        # the nbclient boundary view: a `traitlets.Dict` rejects a `frozendict` (no `dict` subclass),
        # so the one map field projects to a real `dict` (empty -> the client's `None` default); the
        # tuple fields ride as-is because `traitlets.List` coerces a tuple.
        return {**asdict(self), "error_on_timeout": dict(self.error_on_timeout) or None}


class ExportPolicy(Struct, frozen=True):
    # the preprocessor enables only when a band is non-empty, so a bare export pays no preprocessor pass; the tag bands
    # strip the report's scaffolding cells/inputs/outputs before the `from_notebook_node` lowering.
    remove_cell_tags: tuple[str, ...] = ()
    remove_input_tags: tuple[str, ...] = ()
    remove_all_outputs_tags: tuple[str, ...] = ()
    extract_outputs: bool = False  # enable `ExtractOutputPreprocessor` -> display figures land in `resources['outputs']`, spliced as `FigureNode`s the prior arm discarded
    exclude_input: bool = False  # drop every code-cell input from the render (`TemplateExporter.exclude_input` top-level trait)
    exclude_output: bool = False  # drop every cell output from the render
    exclude_input_prompt: bool = False  # drop the `In[ ]:` prompt gutters
    exclude_output_prompt: bool = False  # drop the `Out[ ]:` prompt gutters

    def exporter_kwargs(self) -> dict[str, object]:
        # each `TagRemovePreprocessor` band rides `traitlets.config.Config` as a `set` (the traitlets `Set` trait rejects a
        # tuple), while the `exclude_*` controls ride top-level `**kw` because they are `TemplateExporter` traits, not
        # preprocessor config — an inactive policy yields empty kwargs and a bare export constructs with its defaults.
        bands = frozendict({
            "remove_cell_tags": set(self.remove_cell_tags),
            "remove_input_tags": set(self.remove_input_tags),
            "remove_all_outputs_tags": set(self.remove_all_outputs_tags),
        })
        active = {name: value for name, value in bands.items() if value}
        stages: dict[str, object] = {
            **({"TagRemovePreprocessor": {"enabled": True, **active}} if active else {}),
            **({"ExtractOutputPreprocessor": {"enabled": True}} if self.extract_outputs else {}),
        }
        excludes = {name: True for name in _EXCLUDE_TRAITS if getattr(self, name)}
        return {**({"config": Config(stages)} if stages else {}), **excludes}


class FigureRef(Struct, frozen=True):
    # `media_type`/`intrinsic` ride through so the lowered `FigureNode` carries the producer's MIME and dimensions.
    asset_key: ContentKey
    alt: str = ""
    caption: str = ""
    media_type: str = "image/png"
    intrinsic: tuple[float, float] | None = None


class TableData(Struct, frozen=True):
    # a row-major grid of cell texts plus the `TableNode` band/span/caption metadata.
    rows: tuple[tuple[str, ...], ...] = ()
    header_rows: int = 0  # leading `THead` rows -> `TableNode.header_rows`
    footer_rows: int = 0  # trailing `TFoot` rows -> `TableNode.footer_rows`
    spans: tuple[
        tuple[int, int, int, int], ...
    ] = ()  # (row, present-cell index, col_span, row_span) merged-cell quads a schedule's grouped header needs
    caption: str = ""  # the "Table N: …"/"DOOR SCHEDULE" title -> the `#figure(kind: table)`/`<caption>` the `TableNode` lowers


@tagged_union(frozen=True)
class SectionBlock:
    # a new body concern is one case, never a parallel field beside `blocks`.
    tag: Literal["prose", "listing", "figure", "table"] = tag()
    prose: tuple[BlockKind, tuple[str, ...]] = case()  # (PARAGRAPH/HEADING/QUOTE/CODE/CAPTION, body lines)
    listing: tuple[ListKind, tuple[str, ...]] = case()  # (ORDERED/UNORDERED/DESCRIPTION, item texts)
    figure: FigureRef = case()  # an inline figure placed IN flow -> a `FigureNode` between its sibling blocks
    table: TableData = case()  # an inline data table/schedule -> a `TableNode` in flow between its sibling blocks


class Section(Struct, frozen=True):
    # the section's `classification` lands on the `SectionNode` `NodeMeta`; nested subsections carry the full hierarchy.
    level: int
    heading: str
    blocks: tuple[SectionBlock, ...] = ()
    classification: str = ""  # CSI/OmniClass code -> `NodeMeta.classification`, the specification/section#SECTION consumer
    children: tuple["Section", ...] = ()


class ReportFact(Struct, frozen=True):
    # `body` is the encoded tree for COMPOSE/TEMPLATE/NOTEBOOK and the reflowed PDF for REFLOW; the resolved rows and
    # timing/widget flags ride here because the flat-scalar receipt stream never carries them.
    node: DocumentNode
    body: bytes
    archive: bytes = b""
    pages: int = 0
    figures: int = 0  # NOTEBOOK: the count of `ExtractOutputPreprocessor` display figures spliced as `FigureNode`s
    export: ReportExport = ReportExport.HTML
    loader: ReportLoader = ReportLoader.DICT
    report_source: ReportSource = ReportSource.IPYNB
    timed: bool = False
    widgets: bool = False


class ReportSpec(Struct, frozen=True, omit_defaults=True):
    # `source` is the per-kind material (template/notebook/HTML); `sections`/`figures` the COMPOSE content.
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
    trusted: bool = True
    render: TemplateRender = TemplateRender.STRING  # TEMPLATE: `Environment` HTML string vs `NativeEnvironment` computed value
    context: frozendict[str, object] = field(
        default_factory=frozendict
    )  # TEMPLATE: the jinja render-context data band spread through `render_async(**context)`
    template_globals: frozendict[str, ParamScalar] = field(
        default_factory=frozendict
    )  # TEMPLATE: report-scoped jinja `Environment.globals` injections
    bytecode_cache: str = ""  # TEMPLATE: `FileSystemBytecodeCache` directory for repeated reproducible renders
    extensions: tuple[str, ...] = ()  # jinja2 `Environment(extensions=)` dotted-path rows (`jinja2.ext.do`/`loopcontrols`/`i18n`)
    report_source: ReportSource = ReportSource.IPYNB
    resource_path: str = ""  # NOTEBOOK: cwd seeded into `NotebookClient(resources=)` so a relative-asset notebook resolves
    engine: NotebookEngine = field(default_factory=NotebookEngine)
    export: ReportExport = ReportExport.HTML
    export_policy: ExportPolicy = field(default_factory=ExportPolicy)
    paper: ReflowPaper = ReflowPaper.A4
    user_css: str = ""
    em: float = 12.0
    layout: ReflowLayout = ReflowLayout.DIRECT  # REFLOW: `Story.write` one-shot vs `Story.write_stabilized` convergence
    author_source: AuthorSource = AuthorSource.MARKDOWN  # AUTHOR: the commercial-safe pdf_oxide source kind
    title: str = ""  # AUTHOR: pdf_oxide document title metadata
    author: str = ""  # AUTHOR: pdf_oxide document author metadata
    header: str = ""  # AUTHOR SHEET: running-header center text for the titled sheet
    footer: str = ""  # AUTHOR SHEET: running-footer center text
    toc: bool = True
    toc_title: str = "Contents"


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class ReportFault:
    # the closed ADMISSION vocabulary `of` produces; arm-level provider raises convert to the runtime `BoundaryFault`
    # at the `async_boundary` capsule, never into this vocabulary.
    tag: Literal["payload", "unsatisfied"] = tag()
    payload: tuple[str, ...] = case()  # the rejected ReportPayload key paths
    unsatisfied: tuple[ReportKind, str] = case()  # a kind whose `_REQUIRED` input field is empty


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
    trusted: NotRequired[ReadOnly[bool]]
    render: NotRequired[ReadOnly[TemplateRender]]
    context: NotRequired[ReadOnly[frozendict[str, object]]]
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
    em: NotRequired[ReadOnly[float]]
    layout: NotRequired[ReadOnly[ReflowLayout]]
    author_source: NotRequired[ReadOnly[AuthorSource]]
    title: NotRequired[ReadOnly[str]]
    author: NotRequired[ReadOnly[str]]
    header: NotRequired[ReadOnly[str]]
    footer: NotRequired[ReadOnly[str]]
    toc: NotRequired[ReadOnly[bool]]
    toc_title: NotRequired[ReadOnly[str]]


_PAYLOAD: Final = TypeAdapter(ReportPayload)
# the per-kind precondition: a kind's named `ReportSpec` field must be non-empty so the interior is total.
_REQUIRED: Final[Map[ReportKind, tuple[str, ...]]] = Map.of_seq([
    (ReportKind.COMPOSE, ("sections",)),
    (ReportKind.TEMPLATE, ("source",)),
    (ReportKind.NOTEBOOK, ("source",)),
    (ReportKind.REFLOW, ("source",)),
    (ReportKind.AUTHOR, ("source",)),  # markdown/HTML text or an office file path
])

# --- [OPERATIONS] -----------------------------------------------------------------------


def _box(paper: ReflowPaper) -> Rect:
    return tuple(pymupdf.paper_rect(paper.value))


def _meta(role: str, label: str, path: tuple[int, ...], classification: str = "", /) -> NodeMeta:
    # key by structural PATH joined to the label, so identical-heading siblings under distinct parents never collapse;
    # an empty classification rides `UNSET` so an unclassified node's digest stays omit-defaults-stable.
    trail = "-".join(map(str, path)) or "root"
    return NodeMeta(
        key=ContentIdentity.of(f"report-{role}-{trail}", label.encode()),
        role=role,
        page=path[0] if path else 0,
        classification=classification or UNSET,
    )


def _runs(role: str, path: tuple[int, ...], *lines: str) -> tuple[RunNode, ...]:
    return tuple(RunNode(meta=_meta(f"{role}-run", line, path), text=line, font_key="body", size=11.0) for line in lines if line)


def _block_node(path: tuple[int, ...], block: SectionBlock, /) -> DocumentNode:
    # list items are `LI`-role `BlockNode`s under a real `ListNode`, never a phantom list block kind.
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
        case SectionBlock(tag="figure", figure=ref):  # an inline figure lands as a `FigureNode` at its own path slot, in flow between its siblings
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
        ):  # an inline data table/schedule -> a `TableNode` in flow; each cell one `TD`-role paragraph `BlockNode`
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
                caption=_runs("caption", path, data.caption),
            )
        case _ as unreachable:
            assert_never(unreachable)


def _section_node(path: tuple[int, ...], section: Section, /) -> SectionNode:
    # every child keys by its own path extension so the whole hierarchy carries distinct content slots.
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


def _output_page(spec: ReportSpec, role: str, output: str | bytes, figures: tuple[FigureNode, ...] = (), /) -> PageNode:
    # the rendered output's own type is the discriminant; the extracted display `figures` splice after the leaf.
    text, block = (output, BlockKind.CODE) if isinstance(output, str) else (output.hex(), BlockKind.QUOTE)
    return PageNode(
        meta=_meta("page", role, (0,)),
        media_box=_box(spec.paper),
        children=(BlockNode(meta=_meta(role, role, (0,)), block=block, runs=_runs(role, (0,), text)), *figures),
    )


def _notebook_figures(resources: object, /) -> tuple[FigureNode, ...]:
    # `resources['outputs']` is the filename->bytes display-figure map; each keys by content and splices as a `FigureNode`.
    outputs: dict[str, bytes] = resources.get("outputs", {}) if isinstance(resources, dict) else {}
    return tuple(
        FigureNode(
            meta=_meta("nb-figure", name, (0, ordinal)),
            asset_key=ContentIdentity.of(f"report-notebook-figure-{name}", data),
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
        media_box=_box(spec.paper),
        children=(*outline, *(_section_node((len(outline) + index,), section) for index, section in enumerate(spec.sections))),
    )
    return ReportFact(node=page, body=encode(page))


def _loader(spec: ReportSpec, /) -> BaseLoader:
    # the `ChoiceLoader` fallback applies at the environment, so every loader row composes the same fallback chain.
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
    # `_REPORT_FILTERS`, the spec's serializable `template_globals`, and the deterministic JSON policy install onto the
    # ONE engine, never a second; a `FileSystemBytecodeCache` directory compiles once for repeated reproducible renders.
    factory = NativeEnvironment if spec.render is TemplateRender.NATIVE else Environment if spec.trusted else ImmutableSandboxedEnvironment
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
    env.policies["json.dumps_kwargs"] = {"sort_keys": True}  # deterministic in-template JSON for a reproducible render
    return env


async def _template_arm(plan: "ReportPlan") -> "ReportFact":
    spec = plan.spec
    # one strict-undefined async render; the `context` band spreads the pipeline's bound render values into the
    # jinja context beside `sections`/`figures`, so a missing key is a `jinja2.UndefinedError` fault, never a blank.
    output = await _environment(spec).from_string(spec.source).render_async(sections=spec.sections, figures=spec.figures, **dict(spec.context))
    match spec.render:
        case TemplateRender.STRING:
            page = _output_page(spec, "template", output)  # the rendered HTML string -> `CODE` leaf
        case TemplateRender.NATIVE:
            page = _output_page(spec, "template", _NATIVE.encode(output).decode())  # the computed Python value -> deterministic JSON leaf
        case _ as unreachable:
            assert_never(unreachable)
    return ReportFact(node=page, body=encode(page), loader=spec.loader)


def _exported(spec: ReportSpec, executed: object, /) -> tuple[str | bytes, object]:
    # the blocking nbconvert render — a PDF/WEBPDF export spawns a LaTeX/headless-Chromium subprocess — crosses
    # the thread seam so it never stalls the loop; the `(output, resources)` pair feeds the `_output_page` partition.
    return nbconvert.get_exporter(spec.export.value)(**spec.export_policy.exporter_kwargs()).from_notebook_node(executed)


def _resources(spec: ReportSpec, /) -> dict[str, object]:
    # the nbclient `resources` dict seeding `metadata.path=cwd` so a notebook referencing a relative asset resolves
    # it against `resource_path`; empty when unset, exactly the one-shot `execute(cwd=)` seed the async rail omits.
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
    exported = await LanePolicy.offload(_exported, spec, executed, modality=Modality.THREAD, retry=RetryClass.OCCT)  # blocking nbconvert render off the loop, runtime-owned lane
    output, resources = exported.default_with(lambda fault: _report_raise(fault))
    figures = _notebook_figures(resources)  # the `ExtractOutputPreprocessor` display figures, spliced rather than discarded
    page = _output_page(spec, "notebook", output, figures)
    return ReportFact(
        node=page,
        body=encode(page),
        archive=archive,
        export=spec.export,
        figures=len(figures),
        report_source=spec.report_source,
        timed=spec.engine.record_timing,
        widgets=spec.engine.store_widget_state,
    )


async def _reflow_arm(plan: "ReportPlan") -> "ReportFact":
    spec = plan.spec
    pdf, count, box = await to_process.run_sync(_reflow, spec.source, spec.user_css, spec.em, spec.paper.value, spec.layout)
    key = ContentIdentity.of(f"report-{plan.kind.value}", pdf)
    page = PageNode(meta=NodeMeta(key=key, role="reflow", page=count), media_box=box)
    return ReportFact(node=page, body=pdf, pages=count)


def _reflow(html: str, user_css: str, em: float, paper: str, layout: ReflowLayout) -> tuple[bytes, int, Rect]:
    # one `Story.write`/`write_stabilized` entry lays the whole HTML; `positionfn` collects placed positions and
    # `add_pdf_links` injects live links from them (a no-op when the HTML has no `<a>`), so the returned bytes are the
    # link-enriched reflowed PDF the receipt keys.
    rect = pymupdf.paper_rect(paper)
    buffer = io.BytesIO()
    writer = pymupdf.DocumentWriter(buffer)
    positions: list[object] = []
    pages = 0

    def rectfn(rect_number: int, filled: object, /) -> tuple[object, object, None]:
        return (rect, rect, None)

    def pagefn(page_number: int, mediabox: object, device: object, after: int, /) -> None:
        nonlocal pages  # Exemption: the writer's page callback threads the count through the one imperative page sweep
        if after:
            pages = page_number + 1

    def positionfn(position: object, /) -> None:
        positions.append(position)

    match layout:
        case ReflowLayout.STABILIZED:
            # the static entry builds its own Story and re-lays the regenerated HTML until the layout is stable;
            # `add_header_ids=True` (the parameter default, pinned explicit) injects the navigable header anchors the AEC TOC/cross-reference convergence needs.
            pymupdf.Story.write_stabilized(
                writer, lambda _prev: html, rectfn, user_css=user_css or None, em=em, positionfn=positionfn, pagefn=pagefn, add_header_ids=True
            )
        case ReflowLayout.DIRECT:
            pymupdf.Story(html=html, user_css=user_css or None, em=em).write(writer, rectfn, positionfn=positionfn, pagefn=pagefn)
        case _ as unreachable:
            assert_never(unreachable)
    writer.close()
    return pymupdf.Story.add_pdf_links(buffer, positions).tobytes(), pages, tuple(rect)


async def _author_arm(plan: "ReportPlan") -> "ReportFact":
    # pdf_oxide's Rust core releases the GIL, so the AUTHOR arm crosses the `to_thread` lane, never `to_process`.
    spec = plan.spec
    pdf, count = await to_thread.run_sync(_authored, spec, limiter=_OFFLOAD)
    key = ContentIdentity.of(f"report-{plan.kind.value}", pdf)
    page = PageNode(meta=NodeMeta(key=key, role="author", page=count), media_box=_box(spec.paper))
    return ReportFact(node=page, body=pdf, pages=count)


def _authored(spec: ReportSpec, /) -> tuple[bytes, int]:
    # dispatch the source through the derived `_AUTHOR_BUILD` row to its pdf_oxide entry, then read the bytes and the
    # native page count off the built `Pdf` — one row per source, never an `if source == ...` construction ladder.
    pdf = _AUTHOR_BUILD[spec.author_source](spec)
    return pdf.to_bytes(), len(pdf)


# --- [TABLES] ---------------------------------------------------------------------------

# the report-scoped scientific/AEC formatting filters installed on EVERY report `Environment` (never registered
# per call), so a template renders `{{ load | sigfig }}` / `{{ span | dimension }}` without a bespoke filter map.
_REPORT_FILTERS: Final[Map[str, Callable[..., str]]] = Map.of_seq([
    ("sigfig", lambda value, digits=3: f"{float(value):.{digits}g}"),
    ("si", lambda value, unit="": f"{float(value):.3g} {unit}".rstrip()),
    ("ratio_pct", lambda value: f"{float(value) * 100:.1f}%"),
    ("dimension", lambda value, unit="mm": f"{float(value):.0f} {unit}"),
])

# the commercial-safe pdf_oxide authoring dispatch: one row per `AuthorSource` binding it to its `Pdf`/
# `OfficeConverter` entry, the SHEET row composing a running-header/footer `PageTemplate` for the AEC titled sheet,
# the office rows reading `spec.source` as a file path — a new source is one member plus one row, zero body edit.
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
    spec: ReportSpec = field(default_factory=ReportSpec)
    fact: ReportFact | None = None

    @receipted(
        OPEN
    )  # report facts carry no classified field, so the runtime keep-all `OPEN` policy rides directly, never a re-minted per-file `Redaction`
    async def _composed(self) -> Self:
        return replace(self, fact=await COMPOSE_ARMS[self.kind](self))

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        return ContentIdentity.of(f"report-{self.kind.value}", (self.kind, self.spec), policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the terminal receipt threads the PRE-RUN input key so receipt.slot == node.key.
        return (await async_boundary(f"report.{self.kind.value}", self._composed)).map(lambda done: done._receipt(self._key))

    def _receipt(self, key: ContentKey, /) -> ArtifactReceipt:
        fact = self.fact if self.fact is not None else ReportFact()
        match self.kind:
            case ReportKind.REFLOW | ReportKind.AUTHOR:
                return ArtifactReceipt.Pdf(key, len(fact.body), fact.pages)
            case _:
                return ArtifactReceipt.Report(key, len(fact.body))

    def contribute(self) -> Iterable[Receipt]:
        if (fact := self.fact) is None:  # rides the stepped owner the fold returned, never a re-run
            return
        yield from self._receipt(self._key).contribute()
        if self.kind is ReportKind.NOTEBOOK:  # the executed-notebook archive is its own addressed fact
            yield from ArtifactReceipt.Report(ContentIdentity.of("report-notebook-archive", fact.archive), len(fact.archive)).contribute()

    @classmethod
    def of(cls, kind: ReportKind, /, **raw: Unpack[ReportPayload]) -> Result[Self, ReportFault]:
        try:
            payload = _PAYLOAD.validate_python(raw)
        except ValidationError as fault:
            return Error(ReportFault(payload=tuple(str(error["loc"]) for error in fault.errors())))
        spec = ReportSpec(**payload)
        missing = next((name for name in _REQUIRED.try_find(kind).default_value(()) if not getattr(spec, name)), None)
        return Error(ReportFault(unsatisfied=(kind, missing))) if missing else Ok(cls(kind=kind, spec=spec))


def _report_raise(fault: object) -> tuple[str, dict[str, object]]:
    # terminal collapse at the export boundary: an offload fault reconstructs the raise the node's rail folds.
    raise ValueError(str(fault))
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
