# [PY_ARTIFACTS_TEMPLATE]

The declarative input-to-output template/format pipeline binding one structured context value into a chosen output format. `TemplatePipeline` is ONE owner discriminating output format over the closed `TemplateTarget` vocabulary and binding a single `TemplateContext` value through the EXISTING binding surfaces the sibling owners already own wherever one exists — never re-implementing it: the DOCX arm DELEGATES to the `documents/emit#DOCUMENT` `DocumentPlan` `DOCX_TEMPLATE` mode (docxtpl `DocxTemplate.render`), the PDF arm to the `documents/emit#DOCUMENT` `DocumentPlan` `PDF_TYPST`/`TYPST_DATA` mode (typst `Compiler.compile(sys_inputs=...)`), the HTML arm to the `reports/report#REPORT` `ReportPlan` `TEMPLATE` kind (the jinja2 `ReportLoader`/`Environment` templating owner), and the PPTX arm to the `documents/emit#DOCUMENT` `DocumentPlan` `PPTX` mode (python-pptx template clone); the ODF arm is the ONE binding `TemplatePipeline` composes DIRECTLY — odfpy `OpenDocument` authoring lowering the `documents/model#NODE` `DocumentNode` tree on the cp315 core (the `documents/lens#LENS` `ODS_READ` arm owns only the odfpy READ inverse, and no emit/report owner authors ODF, so the WRITE binding lands here as `TemplatePipeline`'s sole owned producer). It binds the context and ROUTES it to the owning producer for every delegated target; it owns no jinja2 `Environment`, no `DocxTemplate`, and no typst `Compiler` — those stay at `documents/emit#DOCUMENT` and `reports/report#REPORT` — and `TemplatePipeline` composes them, plus the directly-owned odfpy ODF arm, for spec-sheet / sheet-set / parameterized-document-family production where one structured context drives many output formats. Every binding returns a `RuntimeRail[ContentKey]` keyed by the runtime content key and contributes the emitted artifact's `receipt/receipt#RECEIPT` `ArtifactReceipt` row — through the delegated owner for `DOCX`/`PDF`/`HTML`/`PPTX`, and as the directly-minted `ArtifactReceipt.Office` for the owned `ODF` arm — never a second receipt rail.

## [01]-[INDEX]

- [01]-[TEMPLATE]: output-format dispatch axis binding one `TemplateContext` structured value through the `TARGETS` format-per-target policy table into the binding owner — the `TemplateTarget` closed output vocabulary (`DOCX`/`PDF`/`HTML`/`PPTX`/`ODF`), the `TemplateContext` frozen structured-context value the binding lowers, the `TARGETS` table routing each target to its `documents/emit#DOCUMENT` `DocumentMode` delegate, its `reports/report#REPORT` `ReportKind` delegate, or the directly-owned odfpy `.odt` round-trip, and the `TemplateBinding` per-target descriptor carrying the delegated `DocumentMode`/`ReportKind` or the `office_round_trip` direct-compose flag the context binds through.

## [02]-[TEMPLATE]

- Owner: `TemplatePipeline` the one output-format dispatch axis binding a structured context into a chosen format through the existing binding owners plus the directly-owned ODF arm; `TemplateTarget` the closed `StrEnum` over the output formats (`DOCX`/`PDF`/`HTML`/`PPTX`/`ODF`), one row per output, never a per-format pipeline class; `TemplateContext` the frozen structured-context value object carrying the named binding values (the `context: dict[str, object]` the docxtpl `render`/typst `sys_inputs`/jinja2 `render_async` all consume, the `node: DocumentNode` tree the ODF arm lowers, the `sections`/`figures` the `reports/report#REPORT` `TEMPLATE` kind binds, and the `template: str | PathLike` source) — ONE context value every target binds, never a per-format parameter bag; `TARGETS` the `MappingProxyType[TemplateTarget, Bind]` policy table binding each target to its delegate-or-direct arm, so a new output format is one row not a new method; `TemplateBinding` the frozen per-target descriptor carrying the delegated `documents/emit#DOCUMENT` `DocumentMode` (for `DOCX`/`PDF`/`PPTX`) or `reports/report#REPORT` `ReportKind` (for `HTML`) the context binds through, or the `office_round_trip` flag marking the directly-owned ODF arm, so the binding IS a delegate row or the one direct-compose marker rather than an inline branch; `DocumentPlan`/`ReportPlan` the `documents/emit#DOCUMENT`/`reports/report#REPORT` owners the pipeline composes for every delegated target, the pipeline minting no template engine of its own and owning only the odfpy ODF authoring; the bind-and-route layer between a structured context and the format-owning producers.
- Cases: `_docx` binds the `TemplateContext` into the `documents/emit#DOCUMENT` `DocumentPlan(mode=DocumentMode.DOCX_TEMPLATE, node=..., params={"template": ctx.template, ...ctx.context})` and awaits its `produce`, so the DOCX template render IS the emit owner's docxtpl `DocxTemplate.render(context)` arm reached through the `DOCX_TEMPLATE` mode, never a second `DocxTemplate` loaded here; `_pdf` binds the context into the `documents/emit#DOCUMENT` `DocumentPlan(mode=DocumentMode.TYPST_DATA, node=..., params={"sys_inputs": ctx.context, ...})` and awaits `produce`, so the PDF template bind IS the emit owner's typst `Compiler.compile(sys_inputs=...)` arm reached through the `TYPST_DATA` mode (the runtime-data injection the emit owner already owns, no string templating), the `PDF_TYPST` `pdf_standards` archival row reachable through the same delegate; `_html` binds the context into the `reports/report#REPORT` `ReportPlan(kind=ReportKind.TEMPLATE, source=ctx.template, sections=ctx.context, figures=ctx.figures, loader=ctx.loader)` and awaits its `render`, so the HTML template render IS the report owner's jinja2 `report_env(loader, trusted)`/`render_async` `TEMPLATE` kind over the `ReportLoader` loader axis, never a second `Environment` constructed here; `_pptx` binds the context into the `documents/emit#DOCUMENT` `DocumentPlan(mode=DocumentMode.PPTX, ...)` and awaits `produce`, the python-pptx template-clone (`Presentation(template_stream)` opened from the binding's template source and re-saved with the bound shapes) reached through the `PPTX` mode the emit owner already owns; `_odf` is the ONE directly-owned arm — it lowers the `documents/model#NODE` `DocumentNode` tree into an odfpy `OpenDocumentText` `.odt` document on the cp315 core through `_odf_bind`: the binding's `.odt` template `load`ed (or a fresh `OpenDocumentText()` minted when no template), one `walk(node)` `match` folding each `SectionNode` into a `text.H(outlinelevel=level)`, each `BlockNode` into a `text.P`, and each `TableNode` into a `table.Table`/`TableRow`/`TableCell` grid (each cell's text wrapped in an inner `text.P` the OASIS grammar requires), every text run injected through the whitespace-correct `teletype.addTextToElement`, the nodes attached to the `document.text` content root (the `office:text` body the `DocumentNode` prose tree maps to, never `office:spreadsheet`/`office:presentation` which take a different content model), and the packaged ZIP recovered through `OpenDocument.write(BytesIO)`; each delegated arm is a `TARGETS` row reaching a `DocumentMode` or `ReportKind` and the ODF arm the one direct-compose row, never an `if target == ...` branch and never a re-implemented engine where a sibling owner already binds.
- Modality: `TemplatePipeline.bind` is `async` over the runtime `async_boundary`, dispatching the target inside the one fault capsule — a `TARGETS` lookup resolving the `TemplateBinding` whose delegate is a `documents/emit#DOCUMENT` `DocumentPlan` (DOCX/PDF/PPTX/ODF) `produce` or a `reports/report#REPORT` `ReportPlan` (HTML) `render` over the bound `TemplateContext` — and returns a `RuntimeRail[ContentKey]` keyed by the content key the delegated owner mints; a sheet-set / spec-sheet family is one `bind` per `TemplateTarget` over the shared `TemplateContext`, the pipeline routing one context into many formats. The delegated owner's `RuntimeRail` fault (a docxtpl/typst/jinja2 binding failure) folds through the delegate's own `async_boundary`, surfaced unchanged.
- Auto: the pipeline binds ONE structured context and routes it — the DOCX arm reaches docxtpl `DocxTemplate.render(context, autoescape=True)` through the emit owner's `DOCX_TEMPLATE` mode (the `RichText`/`Listing`/`InlineImage` carriers and the `replace_*` part-swap the emit owner already folds), the PDF arm reaches typst `Compiler.compile(sys_inputs=ctx.context, pdf_standards=...)` through the emit owner's `TYPST_DATA`/`PDF_TYPST` mode (the held font-cached `Compiler` world the emit owner amortizes), the HTML arm reaches the jinja2 `report_env(loader, trusted).from_string(source).render_async(sections=..., figures=...)` through the report owner's `TEMPLATE` kind (the strict-undefined `ReportLoader` loader axis the report owner owns), the PPTX arm reaches python-pptx `Presentation` template-clone through the emit owner's `PPTX` mode, and the ODF arm composes odfpy `OpenDocument` `load`/`body.addElement`/`teletype.addTextToElement`/`write` DIRECTLY on the cp315 core (the one binding no sibling owner authors); the pipeline constructs no template engine for any DELEGATED target, loads no jinja2 template root, and re-renders nothing it can delegate — it binds the `TemplateContext` once and the `TARGETS` row carries it into the owning producer, the ODF arm the sole place it lowers the `DocumentNode` tree itself. The shared `TemplateContext` is the collapse: one structured value drives the docxtpl `context` dict, the typst `sys_inputs` map, the jinja2 `render_async` kwargs, the python-pptx bound shapes, and the odfpy node lowering, so a spec-sheet rendered to DOCX, PDF, and HTML is one context bound three ways rather than three parameter bags.
- Receipt: each binding contributes the emitted artifact's `receipt/receipt#RECEIPT` `ArtifactReceipt` row — the DOCX/PPTX arms `ArtifactReceipt.Office` and the PDF arm `ArtifactReceipt.Pdf` through the DELEGATED `documents/emit#DOCUMENT` owner, the HTML arm `ArtifactReceipt.Report` through the DELEGATED `reports/report#REPORT` owner, and the directly-owned ODF arm `ArtifactReceipt.Office` for the odfpy bytes it produces (the one receipt `TemplatePipeline` contributes itself, the office case the receipt owner already declares, never a new case) — keyed by the content key the delegated `produce`/`render` or the ODF `_odf_round_trip` `ContentIdentity.of` mints, never a second template-pipeline receipt rail. The pipeline adds NO new `ArtifactReceipt` case: the delegated arms route to the owner that already contributes one and the ODF arm reuses the existing `Office` case, so the resolved `TemplateTarget`, the delegated `DocumentMode`/`ReportKind` or the `office_round_trip` flag, and the bound context are recoverable from the `TemplatePipeline` value the runtime keys, never a parallel fact map this page invents.
- Packages: the pipeline COMPOSES the sibling owners and cites their binding surfaces as the delegated targets — `docxtpl` (`DocxTemplate.render`/`save`/`new_subdoc`/`build_url_id`, the DOCX bind reached through the `documents/emit#DOCUMENT` `DOCX_TEMPLATE` mode), `typst` (`Compiler.compile(sys_inputs=, pdf_standards=)`/`Compiler.query`, the PDF bind reached through the `documents/emit#DOCUMENT` `TYPST_DATA`/`PDF_TYPST` mode), `jinja2` (`Environment`/`ImmutableSandboxedEnvironment`/`from_string`/`render_async`/the `DictLoader`/`FileSystemLoader`/`PackageLoader`/`PrefixLoader`/`FunctionLoader`/`ModuleLoader`/`ChoiceLoader` loader axis/`StrictUndefined`/`select_autoescape`, the HTML bind reached through the `reports/report#REPORT` `TEMPLATE` kind), `python-pptx` (`Presentation`/`Presentation.save`/`Slides.add_slide`/`slide_layouts`, the PPTX template-clone reached through the `documents/emit#DOCUMENT` `PPTX` mode), `odfpy` (`opendocument.OpenDocumentText`/`load`, `OpenDocument.text.addElement`/`write`, `text.H(outlinelevel=)`/`text.P`, `table.Table`/`TableRow`/`TableCell(valuetype=)`, `teletype.addTextToElement`, the ODF `.odt` `OpenDocumentText` authoring composed DIRECTLY on the cp315 core at boundary-scope import — the WRITE binding no emit/report owner declares); `expression` (`tag`/`case` consumed only as the imported `DocumentMode`/`ReportKind` shapes, never re-declared); `msgspec` (`Struct(frozen=True)` the `TemplateContext`/`TemplateBinding` value objects); runtime (`content_identity.ContentIdentity`/`ContentKey` the keyed result and the ODF arm's key mint, `faults.RuntimeRail`/`async_boundary`/`boundary` the rail, the awaitable-delegate capsule, and the synchronous ODF-arm capsule); `documents/emit#DOCUMENT` (`DocumentPlan`/`DocumentMode` the DOCX/PDF/PPTX delegate), `documents/model#NODE` (`DocumentNode`/`SectionNode`/`BlockNode`/`TableNode`/`RunNode`/`walk` the tree the ODF arm lowers), `reports/report#REPORT` (`ReportPlan`/`ReportKind`/`ReportLoader` the HTML delegate). No new external library — `odfpy` is already admitted (the `documents/lens#LENS` `ODS_READ` arm consumes its READ surface), and every delegated binding surface is one the emit or report owner already admits.
- Growth: a new output format with a sibling owner is one `TemplateTarget` row plus one `TARGETS` arm and one `TemplateBinding` delegating to its `documents/emit#DOCUMENT` `DocumentMode` or `reports/report#REPORT` `ReportKind`; a new ODF node lowering is one `_odf_bind` `match` arm folding a `DocumentNode` variant into its odfpy element; a new template source is one `TemplateContext.template` value; a new bound value is one `TemplateContext` field the delegated owner's `params`/`sections` or the ODF lowering consume; a new archival PDF standard is one `pdf_standards` value threaded into the `TYPST_DATA`/`PDF_TYPST` delegate; a new loader root is one `ReportLoader` value on the HTML delegate (the report owner's loader axis, untouched here); zero new surface — the pipeline grows by target row, delegating to a sibling owner where one binds and composing the one directly-owned odfpy `.odt` arm only because no sibling authors ODF.
- Boundary: no jinja2 `Environment`, no `DocxTemplate`, no typst `Compiler`, and no `python-pptx` `Presentation` construction of its own — the `documents/emit#DOCUMENT` `DocumentPlan` owns the docxtpl/typst/python-pptx arms and the `reports/report#REPORT` `ReportPlan` owns the jinja2 arm, and `TemplatePipeline` binds the `TemplateContext` and routes it to them; the ONE engine it owns is the odfpy `OpenDocument` ODF authoring, because no emit/report owner declares an ODF WRITE arm (the `documents/lens#LENS` `ODS_READ` arm owns only the odfpy READ inverse). A second `DocxTemplate` loaded here where the `DOCX_TEMPLATE` mode owns it, a second jinja2 `Environment` constructed here where the `reports/report#REPORT` `TEMPLATE` kind owns the `ReportLoader` axis, a second typst `Compiler` where the `TYPST_DATA` mode owns the held world, a second python-pptx `Presentation` author where the `PPTX` mode owns it, an ODF arm routed through the `documents/emit#DOCUMENT` `XML` lxml mode (which serializes a hardened-XML tree, NOT an ODF container) or any phantom emit ODF mode that does not exist, a per-format parameter bag where one `TemplateContext` binds every target, an `if target == ...` branch where the `TARGETS` table dispatches the arm, a re-implemented string-templating render where `sys_inputs`/`render_async` bind without templating, a duplicated `RichText`/`InlineImage` carrier fold where the emit owner's `DOCX_TEMPLATE` arm already folds the tree, a duplicated `Story`/`DocumentWriter` reflow where the report owner's `REFLOW` kind owns it, a gated `to_process` hop for the cp315-clean odfpy author where the in-process OASIS writer needs none (the `documents/lens#LENS` `ODS_READ` core band the ODF arm matches), and a second template-pipeline `ArtifactReceipt` case where the delegated owner or the reused `Office` case carries the row are the deleted forms — `TemplatePipeline` is the bind-and-route layer composing the emit and report binding owners and owning only the odfpy ODF arm, never a re-implementation of a binding a sibling already owns. The DOCX_TEMPLATE/PDF_TYPST arms and the ReportLoader templating owner are COMPOSED, never duplicated; the content key is consumed from the delegated owner for every delegated target and minted by the ODF arm's own `ContentIdentity.of`, never re-minted over a delegate's key.

```python signature
from collections.abc import Awaitable, Callable
from enum import StrEnum
from os import PathLike
from types import MappingProxyType
from typing import Final

from msgspec import Struct, field

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary, boundary

from artifacts.documents.emit import DocumentMode, DocumentPlan
from artifacts.documents.model import BlockNode, DocumentNode, RunNode, SectionNode, TableNode, walk
from artifacts.reports.report import ReportKind, ReportLoader, ReportPlan


class TemplateTarget(StrEnum):
    DOCX = "docx"
    PDF = "pdf"
    HTML = "html"
    PPTX = "pptx"
    ODF = "odf"


type Bind = Callable[["TemplatePipeline"], Awaitable[RuntimeRail[ContentKey]]]


class TemplateContext(Struct, frozen=True):
    template: str | PathLike[str]
    node: DocumentNode
    context: dict[str, object] = field(default_factory=dict)
    sections: dict[str, object] = field(default_factory=dict)
    figures: tuple[object, ...] = ()
    pdf_standards: tuple[str, ...] = ()
    loader: ReportLoader = ReportLoader.DICT


class TemplateBinding(Struct, frozen=True):
    document_mode: DocumentMode | None = None
    report_kind: ReportKind | None = None
    office_round_trip: bool = False


async def _via_document(plan: "TemplatePipeline", mode: DocumentMode, params: dict[str, object]) -> RuntimeRail[ContentKey]:
    return await DocumentPlan(mode=mode, node=plan.context.node, params=params).produce()


async def _docx(plan: "TemplatePipeline") -> RuntimeRail[ContentKey]:
    return await _via_document(plan, DocumentMode.DOCX_TEMPLATE, {"template": plan.context.template, **plan.context.context})


async def _pdf(plan: "TemplatePipeline") -> RuntimeRail[ContentKey]:
    return await _via_document(
        plan, DocumentMode.TYPST_DATA, {"source": plan.context.template, "sys_inputs": plan.context.context, "pdf_standards": plan.context.pdf_standards}
    )


async def _pptx(plan: "TemplatePipeline") -> RuntimeRail[ContentKey]:
    return await _via_document(plan, DocumentMode.PPTX, {"template": plan.context.template, **plan.context.context})


async def _odf(plan: "TemplatePipeline") -> RuntimeRail[ContentKey]:
    return boundary("template.odf", lambda: _odf_round_trip(plan.context))


def _odf_round_trip(ctx: TemplateContext) -> ContentKey:
    return ContentIdentity.of("template-odf", _odf_bind(ctx.template, ctx.node))


def _odf_bind(template: str | PathLike[str], node: DocumentNode) -> bytes:
    import io

    from odf.opendocument import OpenDocumentText, load
    from odf.table import Table, TableCell, TableRow
    from odf.teletype import addTextToElement
    from odf.text import H, P

    document = load(template) if template else OpenDocumentText()
    for child in walk(node):
        match child:
            case SectionNode(level=level, heading=heading):
                document.text.addElement(_filled(H(outlinelevel=level), addTextToElement, "".join(run.text for run in heading)))
            case BlockNode(runs=runs):
                document.text.addElement(_filled(P(), addTextToElement, "".join(run.text for run in runs)))
            case TableNode(rows=rows):
                table = Table()
                for row in rows:
                    table_row = TableRow()
                    for cell in row:
                        table_row.addElement(_filled(TableCell(valuetype="string"), addTextToElement, "".join(leaf.text for leaf in walk(cell) if isinstance(leaf, RunNode)), P))
                    table.addElement(table_row)
                document.text.addElement(table)
    sink = io.BytesIO()
    document.write(sink)
    return sink.getvalue()


def _filled(element: object, inject: object, text: str, wrap: object = None) -> object:
    target = element
    if wrap is not None:
        target = wrap()
        element.addElement(target)
    inject(target, text)
    return element


async def _html(plan: "TemplatePipeline") -> RuntimeRail[ContentKey]:
    report = ReportPlan(
        kind=ReportKind.TEMPLATE,
        source=str(plan.context.template),
        sections=plan.context.sections,
        figures=plan.context.figures,
        loader=plan.context.loader,
    )
    rail = await report.render()
    return rail.map(lambda keyed: keyed[0])


TARGETS: Final[MappingProxyType[TemplateTarget, Bind]] = MappingProxyType({
    TemplateTarget.DOCX: _docx,
    TemplateTarget.PDF: _pdf,
    TemplateTarget.HTML: _html,
    TemplateTarget.PPTX: _pptx,
    TemplateTarget.ODF: _odf,
})

BINDINGS: Final[MappingProxyType[TemplateTarget, TemplateBinding]] = MappingProxyType({
    TemplateTarget.DOCX: TemplateBinding(document_mode=DocumentMode.DOCX_TEMPLATE),
    TemplateTarget.PDF: TemplateBinding(document_mode=DocumentMode.TYPST_DATA),
    TemplateTarget.HTML: TemplateBinding(report_kind=ReportKind.TEMPLATE),
    TemplateTarget.PPTX: TemplateBinding(document_mode=DocumentMode.PPTX),
    TemplateTarget.ODF: TemplateBinding(office_round_trip=True),
})


class TemplatePipeline(Struct, frozen=True):
    target: TemplateTarget
    context: TemplateContext

    async def bind(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"template.{self.target}", self._bind)

    async def _bind(self) -> ContentKey:
        rail = await TARGETS[self.target](self)
        return rail.value
```

## [03]-[RESEARCH]

- [DELEGATED_BINDINGS] [RESOLVED]: every `TemplateTarget` row with a sibling owner delegates to it rather than re-implementing one (the ODF row, alone, is composed directly per `[OFFICE_TEMPLATE_ARMS]` because no sibling authors ODF) — the DOCX arm to `documents/emit#DOCUMENT` `DocumentMode.DOCX_TEMPLATE` (the docxtpl `DocxTemplate.render(context, autoescape=True)` arm the emit owner's `_docxtpl_emit` folds, verified against the emit page's `DOCX_TEMPLATE` row and the folder `docxtpl` `.api` `DocxTemplate.__init__`/`render`/`save`/`new_subdoc`/`build_url_id` rows `[01]`-`[07]`), the PDF arm to `DocumentMode.TYPST_DATA` (the typst `Compiler.compile(sys_inputs=...)` runtime-data injection the emit owner's `_typst_compile` runs, verified against the emit page's `TYPST_DATA` row and the folder `typst` `.api` `Compiler.compile(input=, output=, sys_inputs=, pdf_standards=)` row `[01]` — `sys_inputs` a `dict[str, str]` exposed as `sys.inputs`, no string templating), the HTML arm to `reports/report#REPORT` `ReportKind.TEMPLATE` (the jinja2 `report_env(loader, trusted).from_string(source).render_async(...)` the report owner's `_template_tree` runs over the `ReportLoader` loader axis, verified against the report page's `TEMPLATE` kind and the folder `jinja2` `.api` `Environment`/`from_string`/`render_async`/loader rows). The pipeline constructs no engine — it binds the context and the delegate produces, so the docxtpl/typst/jinja2 binding surfaces are COMPOSED through the owning `DocumentMode`/`ReportKind`, never duplicated. The `DocumentPlan`/`DocumentMode` and `ReportPlan`/`ReportKind`/`ReportLoader` spellings and the `from artifacts.documents.emit import ...`/`from artifacts.reports.report import ...` paths verify against the `documents/emit#DOCUMENT` and `reports/report#REPORT` owners.
- [SHARED_CONTEXT] [RESOLVED]: one `TemplateContext` value drives every target — the `context: dict[str, object]` binds the docxtpl `render` context, the typst `sys_inputs` map, and the python-pptx bound values; the `node: DocumentNode` is the tree the docxtpl/typst/python-pptx delegates lower and the directly-owned ODF arm folds through `walk`; the `sections`/`figures` bind the `reports/report#REPORT` `TEMPLATE` kind's `render_async(sections=, figures=)` kwargs; the `template` source loads the docxtpl/typst/python-pptx template, the jinja2 `from_string` source, and the odfpy `.odt` template (or is empty for a fresh `OpenDocumentText`); the `pdf_standards` threads the archival PDF/A target into the `TYPST_DATA`/`PDF_TYPST` delegate. This is the collapse the work-item names: a spec-sheet rendered to DOCX, PDF, and HTML is ONE `TemplateContext` bound three ways through three `TARGETS` rows, never three per-format parameter bags, so the sheet-set / parameterized-document-family production is one context routed into many formats. The `context`/`sys_inputs`/`sections`/`figures` binding shapes verify against the consuming arms in `documents/emit#DOCUMENT` (`DOCX_TEMPLATE`/`TYPST_DATA`/`PPTX`) and `reports/report#REPORT` (`TEMPLATE`), and the `node` lowering against the `documents/model#NODE` `DocumentNode`/`walk` the ODF arm consumes.
- [OFFICE_TEMPLATE_ARMS] [RESOLVED]: the PPTX arm delegates to `documents/emit#DOCUMENT` `DocumentMode.PPTX` for the python-pptx template clone (the emit owner's office arm opening `Presentation(template_stream)` from the binding source and re-saving with bound shapes, verified against the folder `python-pptx` `.api` `Presentation(pptx=None)`/`Presentation.save`/`Slides.add_slide(layout)`/`slide_layouts` rows `[01]`-`[05]` — the open-or-create factory the template clone uses), so the python-pptx sub-3.15-worker gating stays at the emit owner where the `PPTX` arm already places it, the pipeline crossing no gated band for PPTX. The ODF arm is NOT delegated — `documents/emit#DOCUMENT` declares no ODF `DocumentMode` (its closed mode set is PDF/Typst/Office-DOCX-PPTX-XLSX/structured-text, no ODF row), and the only odfpy in the corpus is the `documents/lens#LENS` `ODS_READ` recover-TO inverse, so the ODF WRITE binding has no sibling owner and `TemplatePipeline` composes odfpy `OpenDocumentText` `.odt` authoring DIRECTLY: `_odf_bind` mints a fresh `OpenDocumentText` (or `load`s the `.odt` template), folds the `DocumentNode` prose tree through one `walk`+`match` into `text.H`/`text.P`/`table.Table`/`TableRow`/`TableCell` nodes attached to the `document.text` content root, injects each run through `teletype.addTextToElement`, and recovers the ZIP through `OpenDocument.write(BytesIO)`. The `DocumentNode` tree is the document-prose interior (headings, blocks, ruled tables), so it lowers to the `office:text` content model of an `.odt` text document, never the disjoint `office:spreadsheet`/`office:presentation` content models a `.ods`/`.odp` flavor takes — `text:h`/`text:p` are illegal children of `office:spreadsheet`, so a single `.odt` target is the honest ODF form for this tree rather than a three-flavor factory that faults on two of its rows. The `opendocument.OpenDocumentText`/`load` (factory row `[03]`, parse row `[03]`), `OpenDocument.body` (`Element`) `.addElement(element, check_grammar=True)` reached through the `doc.text` flavor alias the row names (row `[02]` — `OpenDocument` itself has no `addElement`, the node attaches through the content root), `OpenDocument.write(outputfp)` (row `[04]`), `text.H(outlinelevel=)`/`text.P` (factory rows `[09]`/`[07]`), `table.Table`/`TableRow`/`TableCell(valuetype=)` (factory rows `[01]`-`[03]`), and `teletype.addTextToElement(odfElement, s)` (row `[16]`) spellings verify against the folder `odfpy` `.api` catalogue; odfpy is dependency-free pure Python installing on the cp315 core, so the ODF author resolves in-process on the synchronous `boundary` capsule with no `to_process` hop, matching the `documents/lens#LENS` `ODS_READ` cp315-core band rather than a gated Rust seam.
- [RECEIPT_DELEGATION] [RESOLVED]: the pipeline adds NO new `ArtifactReceipt` case to the `receipt/receipt#RECEIPT` owner — the delegated arms' receipts are the delegated owner's (`documents/emit#DOCUMENT` `DOCX_TEMPLATE`/`PPTX` arms contributing `ArtifactReceipt.Office`, the `TYPST_DATA`/`PDF_TYPST` arm `ArtifactReceipt.Pdf`, and the `reports/report#REPORT` `TEMPLATE` kind `ArtifactReceipt.Report`, each keyed by the content key the delegated `produce`/`render` mints — the `documents/emit#DOCUMENT` and `reports/report#REPORT` owners both return a content-keyed `RuntimeRail`, the HTML delegate's `RuntimeRail[tuple[ContentKey, tuple[ArtifactReceipt, ...]]]` projected to its `ContentKey` through `rail.map(lambda keyed: keyed[0])`), and the directly-owned ODF arm contributes the SAME `ArtifactReceipt.Office` case the receipt owner already declares (the odfpy bytes keyed by `_odf_round_trip`'s `ContentIdentity.of`, the office-artifact case reused rather than a new template-pipeline case). The pipeline invents no receipt surface — every delegated arm routes to the owner that already contributes one and the ODF arm reuses the existing `Office` case, so the `receipt/receipt#RECEIPT` owner gains zero cases for the template pipeline. The `ArtifactReceipt.Office`/`.Pdf`/`.Report` cases verify against the `receipt/receipt#RECEIPT` owner; the delegated-owner content-key return shapes verify against the `documents/emit#DOCUMENT` `produce` and `reports/report#REPORT` `render` signatures, and the ODF arm's office contribution reuses the office case the `documents/emit#DOCUMENT` `DOCX`/`PPTX` arms also contribute.
- [NO_REIMPLEMENTATION] [RESOLVED]: the pipeline re-implements no engine a sibling owner already owns — no jinja2 `Environment` (the `reports/report#REPORT` `report_env` owns it over the `ReportLoader` axis), no `DocxTemplate` (the `documents/emit#DOCUMENT` `_docxtpl_emit` owns it), no typst `Compiler` (the `documents/emit#DOCUMENT` `_typst_compile` owns the held world), and no python-pptx `Presentation` authoring (the `documents/emit#DOCUMENT` `PPTX` arm owns it). The ONE binding `TemplatePipeline` composes directly is odfpy `OpenDocument` ODF authoring, because no emit/report owner declares an ODF WRITE arm — a routing of ODF through the `documents/emit#DOCUMENT` `XML` mode is the rejected form, since that mode is the lxml `etree.tostring` hardened-XML serializer (it lowers `to_lxml_tree(node)` to a generic XML document, never an OASIS ODF ZIP container), and a phantom emit ODF `DocumentMode` does not exist in the emit owner's closed mode set. The `TARGETS` table is the one dispatch — every row a delegate `Callable` reaching a `DocumentMode` or `ReportKind`, plus the one direct odfpy arm — and the `BINDINGS` table is the readable descriptor naming each target's delegated mode/kind or the `office_round_trip` direct marker for the spec-sheet family that selects an output format by row. This is the disciplined composition the work-item names: `template.md` COMPOSES the emit `DOCX_TEMPLATE`/`PDF_TYPST` arms and the report `ReportLoader` templating owner rather than duplicating them, and composes the odfpy `.odt` WRITE surface only because it is the binding no sibling owns, so the new surface is the `TemplateTarget` vocabulary, the `TemplateContext` shared-context value, and the one directly-owned odfpy ODF arm — every other binding engine is a delegated sibling owner.
