# [PY_ARTIFACTS_FORMAT]

The declarative one-context-to-many-formats template engine binding a single structured context into a closed output vocabulary, owning NO producer of its own. `TemplatePipeline` is ONE owner discriminating output format over the `TemplateTarget` closed `StrEnum` AND modality over `TemplateTarget | Iterable[TemplateTarget]` — singular renders one format, plural fans the SAME bound owner across many formats inside one `anyio` task-group failure boundary, never a per-format pipeline class and never a sheet-set helper beside the singular path. The `DocumentNode` tree `pipeline.node` is the universal structural carrier every format lowers from; `pipeline.bindings` is the late-bound data-injection band the two template-renderers read — the typst `sys.inputs` map (PDF) and the `jinja2` `render_async` sections (HTML) — never a docxtpl context (the `document/emit#DOCUMENT` docxtpl arm derives its context from the tree, so a render binding has no docx home). Every `TemplateTarget` DELEGATES to the sibling owner that owns its producer: the `DOCX`/`PDF`/`PPTX`/`ODF` document forms and the `XML`/`YAML`/`TOML` structured-data forms construct a `document/emit#DOCUMENT` `DocumentPlan` over a `DocumentMode` — the `ODF` form riding the emit owner's own `DocumentMode.ODT` odfpy `OpenDocumentText` arm, because `document/emit#DOCUMENT` IS the document-lowering owner and already declares `ODT`/`ODS`, so the pipeline NEVER re-authors ODF — while the `HTML` form constructs a `document/report#REPORT` `ReportPlan` over `ReportKind.TEMPLATE`. The pipeline mints no `jinja2.Environment`, no `DocxTemplate`, no typst `Compiler`, no `python-pptx` `Presentation`, and no odfpy `OpenDocument` — it binds the owner once and constructs the owning producer's plan, a pure bind-and-route layer with zero owned engine.

Raw binding material is admitted EXACTLY ONCE: `TemplatePipeline.of(node, **payload)` validates the keyword material through the closed `TemplatePayload` `TypedDict` and its module-level `_PAYLOAD` `TypeAdapter`, folding the `extra_items=object` heterogeneous render band into the frozen `bindings` `frozendict` evidence and materializing every declared key (`template`/`figures`/`assets`/`variant`/`loader`/`trusted`) onto a typed field of the ONE frozen owner — there is no separate `TemplateContext` wrapper, the admitted owner IS the dispatcher — so a malformed payload rails as `Error("<invalid-payload>")` and the interior is total over the admitted owner, never a `dict[str, object]` bag a binding arm re-coerces and never a raw constructor a caller fills by hand. The delegated plans are constructed DIRECTLY from the already-admitted owner (`DocumentPlan(mode=..., node=pipeline.node, spec=EmitSpec(...))`, `ReportPlan(kind=..., source=pipeline.template, ...)`) — trusted construction, never a second `DocumentPlan.of`/`ReportPlan.of` re-validation of values the seam already proved. The `assets` band is the `asset_key.hex -> path` figure map threaded into the office arms' `EmitSpec.assets` so the same bound figures embed across `DOCX`/`PPTX`/`ODF`, the report owner reading the typed `figures: tuple[FigureRef, ...]` for `HTML`. Every arm returns `RuntimeRail[Keyed]` — a `(ContentKey, tuple[ArtifactReceipt, ...])` pair the rail threads unchanged, never cracked to `.value` and re-wrapped. The plural fan-out threads the per-target pairs through the runtime `faults#FAULT` `traversed` fail-fast reducer into `RuntimeRail[Block[Keyed]]`, so a spec-sheet rendered to `DOCX`, `PDF`, and `HTML` is ONE bound owner driven three ways under one cancellation scope, its first delegated fault cancelling the siblings.

Receipt ownership stays with the producing sibling. The emit-delegated `DOCX`/`PDF`/`PPTX`/`ODF`/`XML`/`YAML`/`TOML` arms carry an EMPTY receipt tuple because the emit owner's `produced` modal entrypoint weave-emits its own `ArtifactReceipt` through the `@receipted` harvest on `DocumentPlan._emit` — the `ODF`->`ODT` row's `office` receipt is the emit weave's, exactly as `DOCX`/`PPTX` are — so the row is already on the runtime stream and a second in-band copy would double-count it. The `HTML` arm threads back UNCHANGED the `report`/`pdf` rows the `ReportPlan.render` in-band `Keyed` pair carries (the report owner returns its receipts in-band, precisely so a composing caller surfaces them). `contribute(outcomes)` is the in-band drain a composing caller folds to surface the carried `report`/`pdf` rows onto the runtime fold by delegating to each `ArtifactReceipt.contribute`, adding NO new `ArtifactReceipt` case, minting no receipt of its own, and reading nothing off the pipeline — a module fold over the outcomes, never an instance method that ignores `self`.

## [01]-[INDEX]

- [01]-[FORMAT]: the output-format-and-modality dispatch axis binding one `TemplatePipeline` owner through the `DELEGATES` mode table and the `TARGETS` arm table into the owning producer — the `TemplatePayload` closed `TypedDict` plus the module-level `_PAYLOAD` `TypeAdapter` admitting raw keyword material once into the frozen owner through `TemplatePipeline.of`, the `TemplateTarget` closed output vocabulary (`DOCX`/`PDF`/`HTML`/`PPTX`/`ODF` document forms plus the `XML`/`YAML`/`TOML` structured-data forms the delegated emit owner lowers the same `DocumentNode` tree to), the one frozen `TemplatePipeline` owner whose `node` is the structural carrier and `bindings` is a `frozendict` data-injection band not a `dict[str, object]` bag, the `DELEGATES` `frozendict[TemplateTarget, DocumentMode]` collapsing the seven `DocumentPlan` arms (`DOCX`/`PDF`/`PPTX`/`ODF`/`XML`/`YAML`/`TOML`) into one derived row, the `TARGETS` `frozendict[TemplateTarget, Bind]` carrying only the one arm `DELEGATES` cannot — the `HTML` `ReportPlan` row, the report owner being a distinct producer from the emit owner, the `Keyed` `(ContentKey, tuple[ArtifactReceipt, ...])` rail payload every arm threads, the `bound` modal entrypoint discriminating `TemplateTarget | Iterable[TemplateTarget]` so the sheet-set family is one `start_soon`/`TaskHandle.return_value` fan-out not a sibling, and the `contribute` module-fold receipt drain streaming the `HTML` arm's carried `report`/`pdf` rows.

## [02]-[FORMAT]

- Owner: `TemplatePipeline` the one frozen `msgspec.Struct(frozen=True)` format-and-modality dispatch owner — admitted once, holding the bound material AND the dispatch, no second wrapper: the `node: DocumentNode` structural tree every format lowers, the `bindings: frozendict[str, object]` data-injection band (NOT a `dict[str, object]` bag) feeding the typst `sys.inputs` and the `jinja2` `render_async` sections, the typed `figures: tuple[FigureRef, ...]` the HTML delegate splices, the `assets: frozendict[str, str]` `asset_key.hex -> path` figure band threaded into the office arms' `EmitSpec.assets`, the `template: str` source (the docxtpl/`python-pptx`/`.odt` template path, the `jinja2` `from_string` source), and the `variant: PdfVariant`/`loader`/`trusted` policy values — ONE owner every target binds, never a per-format parameter bag, never the parallel `context`/`sections` two-dict split a delegate kwarg-rename would smuggle in, and never the `TemplateContext`-wrapped-by-`TemplatePipeline` one-field indirection a separate value object would re-introduce; `TemplateTarget` the closed `StrEnum` over the output formats (the `DOCX`/`PDF`/`HTML`/`PPTX`/`ODF` document forms and the `XML`/`YAML`/`TOML` structured-data forms), one row per output, never a per-format class; `TemplatePayload` the closed `TypedDict(extra_items=object)` ingress contract with `Required[ReadOnly[]]` `template` plus `NotRequired[ReadOnly[]]` `figures`/`assets`/`variant`/`loader`/`trusted`, admitted exactly once through the module-level `_PAYLOAD` `TypeAdapter` at `TemplatePipeline.of` so the heterogeneous render kwargs fold into the `bindings` band and a malformed payload faults at the seam, never an interior re-validation; `DELEGATES` the `frozendict[TemplateTarget, DocumentMode]` mapping `DOCX`/`PDF`/`PPTX`/`ODF` and the structured-data `XML`/`YAML`/`TOML` targets to their `document/emit#DOCUMENT` `DocumentMode` so the seven emit arms are ONE derived dispatch row rather than seven sibling functions (the gated `XML` row riding the emit owner's own `to_process` band transparently, the binding never re-deriving the subprocess hop the emit mode internalizes); `TARGETS` the `frozendict[TemplateTarget, Bind]` carrying only the one arm `DELEGATES` cannot — the `HTML` `ReportPlan` row, the table left open so a future distinct-producer target lands as one row; `DocumentPlan`/`EmitSpec`/`ReportPlan` the `document/emit#DOCUMENT`/`document/report#REPORT` owners the pipeline composes for every target, constructed directly from the trusted owner and minting no engine of its own; the bind-and-route layer between a structured owner and the format-owning producers.
- Cases: the `DOCX`/`PDF`/`PPTX`/`ODF` template-bind arms and the `XML`/`YAML`/`TOML` structured-text arms collapse into ONE `_via_document` derived dispatch — `DELEGATES[target]` resolves the `DocumentMode` (`DOCX_TEMPLATE`/`TYPST_DATA`/`PPTX`/`ODT`/`XML`/`YAML`/`TOML`) and `_emit_spec(target, pipeline)` projects the per-mode `EmitSpec` (`sys_inputs=` the JSON-coerced `bindings` plus `variant=pipeline.variant` for the typst data-bind `PDF`; `template=pipeline.template` plus `assets=pipeline.assets` for the `DOCX`/`PPTX`/`ODF` office arms so the bound template and the figure band reach docxtpl/`python-pptx`/odfpy alike; the bare default for the node-lowered structured-text arms) — so one `DocumentPlan(mode=DELEGATES[target], node=pipeline.node, spec=...)` is threaded through the emit owner's `produced` entrypoint, `rail.map(lambda keys: (keys.head(), ()))` carrying the head key with the empty receipt tuple the emit weave already owns, never a second engine, never seven near-identical arm functions, never a phantom `params=` constructor, and never an owned odfpy re-author where `DocumentMode.ODT` already lowers the tree to `.odt`; `_html` binds the owner into a `document/report#REPORT` `ReportPlan(kind=ReportKind.TEMPLATE, source=pipeline.template, sections=dict(pipeline.bindings), figures=pipeline.figures, loader=pipeline.loader, trusted=pipeline.trusted)` and AWAITS its `render` rail directly — `ReportPlan.render` already returns the `RuntimeRail[(ContentKey, receipts)]` shape, so the arm threads the whole pair UNCHANGED, keeping the report's in-band rows rather than projecting to the key and discarding them, the HTML render being the report owner's `report_env(loader, trusted)`/`render_async` `TEMPLATE` kind over the `ReportLoader` axis, never a second `Environment`.
- Modality: `TemplatePipeline.bound` is the one polymorphic entrypoint over `target: TemplateTarget | Iterable[TemplateTarget]`, two `@overload` arms carrying the per-modality output shape (`TemplateTarget -> RuntimeRail[Keyed]`, `Iterable[TemplateTarget] -> RuntimeRail[Block[Keyed]]`) so a caller narrows on the input it passes rather than the runtime union, the body normalizing once at the head through one structural `match` — the lone `TemplateTarget()` arm read FIRST so a `StrEnum` target (itself iterable) never shatters through the `stream` catch-all that folds any `tuple`/`frozenset`/iterable uniformly through `Block.of_seq` — so arity is a property of the value, never a `batch`/`many` knob and never a `tuple()`/`frozenset()` arm beside an identical catch-all. A lone target awaits the single `_dispatch` rail directly; a target family fans the SHARED owner across one `anyio.create_task_group` — each format an independent `group.start_soon(self._dispatch, target)` returning a `TaskHandle[RuntimeRail[Keyed]]`, the child rails its own faults so the group exits clean and every handle settles `FINISHED`, and the post-scope `handles.map(lambda handle: handle.return_value)` reads each carrier with no re-raise — then threads the `Block[RuntimeRail[Keyed]]` through the runtime `faults#FAULT` `traversed` fail-fast reducer into one `RuntimeRail[Block[Keyed]]`, the first delegated fault cancelling the siblings inside the boundary. The child carrier rides the handle the task group already owns (the concurrency `CHILD_CARRIER` law), never a `create_memory_object_stream` result-gather rig re-implementing it. `_dispatch` is the per-target capsule — `DELEGATES` resolves a `DocumentPlan` threaded through the emit owner's `produced` entrypoint, else `TARGETS` resolves the HTML `ReportPlan` `render` — returning the producer's `RuntimeRail[Keyed]` threaded unchanged.
- Auto: the pipeline binds ONE structured owner and routes it — the `DOCX` arm reaches docxtpl `DocxTemplate.render` through the emit owner's `DOCX_TEMPLATE` mode lowering `node` (the docxtpl context is the emit owner's node walk, the `template`/`assets` the bound `.docx` template and figure band), the `PDF` arm reaches typst `Compiler.compile_with_warnings(sys_inputs=..., pdf_standards=variant.typst)` through the emit owner's `TYPST_DATA` mode lowering `node` to typst source with `bindings` JSON-coerced into `EmitSpec.sys_inputs` and `variant` selecting the archival `PdfVariant`, the `HTML` arm reaches `jinja2` `report_env(loader, trusted).from_string(source).render_async(sections=dict(bindings), figures=figures)` through the report owner's `TEMPLATE` kind (the strict-undefined `ReportLoader` axis), the `PPTX` arm reaches `python-pptx` `Presentation` template-clone through the emit owner's `PPTX` mode, the `ODF` arm reaches the emit owner's odfpy `OpenDocumentText` author through the `ODT` mode lowering the SAME `node` tree to `.odt`, and the `XML`/`YAML`/`TOML` arms reach the emit owner's modes lowering the bound `node` tree through lxml `etree.tostring` / ruamel-yaml `YAML().dump` / tomlkit `dumps` so a spec-sheet's bound tree also egresses as a machine-readable structured-data sidecar. The pipeline constructs no engine for any target. The `node` tree is the structural collapse: one immutable `DocumentNode` drives every emit and report lowering, while `bindings` is the data-injection collapse across the two template-renderers (typst `sys.inputs`, jinja2 `sections`), so a spec-sheet rendered to `DOCX`, `PDF`, and `HTML` in one `bound((DOCX, PDF, HTML))` call is one owner bound three ways through three `_dispatch` arms rather than three parameter bags and three call sites.
- Receipt: `contribute(outcomes)` is the module-fold in-band receipt drain a composing caller folds — it streams every `ArtifactReceipt` row the arms CARRIED in their `Keyed` pair onto the runtime fold by delegating to each `ArtifactReceipt.contribute`, adding NO new `ArtifactReceipt` case and reading nothing off the pipeline (it folds the `outcomes` the caller already holds, so it is a free fold, never a `self`-ignoring method). Ownership is by the producing sibling: the `HTML` arm threads back unchanged the `report`/`pdf` rows the `ReportPlan.render` in-band `Keyed` pair carries (the report owner returns its receipts in-band, precisely so a composing caller surfaces them). The emit-delegated `DOCX`/`PDF`/`PPTX`/`ODF`/`XML`/`YAML`/`TOML` arms carry an EMPTY receipt tuple because the emit owner's `produced` entrypoint weave-emits its own `ArtifactReceipt` through the `@receipted` harvest on `DocumentPlan._emit` — the `ODF`->`ODT` `office` row is the emit weave's, exactly as `DOCX`/`PPTX`, so an in-band copy would double-count it and the `document/emit#DOCUMENT` owner stays the emit receipt owner. The pipeline mints no receipt of its own — there is no owned producer whose bytes it would key. The resolved `TemplateTarget`, its delegated `DocumentMode`/`ReportKind`, and the bound material stay recoverable from the `TemplatePipeline` value, never a parallel fact map and never a dead descriptor struct beside the dispatch.
- Fault: fault capture rides the per-arm fences the rail owner mints, not an invented entrypoint weave — every arm returns a `produced`/`ReportPlan.render` rail already fenced by the delegate's own `async_boundary` (with the delegate's contract arm folded innermost), so a docxtpl/typst/jinja2/odfpy binding failure rails as `BoundaryFault` rather than raises, surfaced unchanged through the task-group edge. The plural `_fanned` runs inside one `anyio.create_task_group` whose `__aexit__` converts a child raise into the group edge, and the runtime `faults#FAULT` `traversed` reducer short-circuits the gathered rails on the first `Error`; the all-clean `handle.return_value` read is reached only when every child settled `FINISHED`, because a child raise would have propagated the group's `BaseExceptionGroup` before the post-scope read. The pipeline weaves no `async_boundary`, no span, and no retry of its own — it owns no engine to fence, the in-process bind carries no transient provider, and the delegated owners already fence and span their work, so re-deriving any of them here would duplicate a concern the owning sibling holds.
- Packages: the pipeline COMPOSES the sibling owners and cites their binding surfaces, owning none — `document/emit#DOCUMENT` (`DocumentPlan`/`DocumentMode`/`EmitSpec`/`PdfVariant` the `DOCX`/`PDF`/`PPTX`/`ODF`/structured-text delegate constructed directly and threaded through the `produced` modal entrypoint, the receipt owner for every emit-delegated arm; `DocumentMode.DOCX_TEMPLATE`/`TYPST_DATA`/`PPTX`/`ODT`/`XML`/`YAML`/`TOML` the resolved rows, `EmitSpec.sys_inputs`/`variant`/`template`/`assets` the per-mode fields, the docxtpl/typst/`python-pptx`/odfpy/lxml/ruamel-yaml/tomlkit engines all held inside the emit owner), `document/report#REPORT` (`ReportPlan`/`ReportKind.TEMPLATE`/`ReportLoader`/`FigureRef`/`ReportPlan.render` the HTML delegate returning its `report`/`pdf` rows in-band, the `jinja2` `report_env` engine held inside the report owner), `document/model#NODE` (`DocumentNode` the structural tree the bound owner carries and every delegate lowers); `anyio` (`create_task_group`/`start_soon` returning the `TaskHandle[RuntimeRail[Keyed]]` whose `return_value` carries each child's rail back, the fan-out failure boundary the plural modality runs inside — no `to_thread`/offload of its own, the pipeline owning no blocking author); `expression` (`Block`/`Block.of_seq`/`Block.map` the normalized target family and the per-child carrier map, `Result`/`Ok`/`Error` the `of` admission rail); `pydantic` (`TypeAdapter` the module-level `_PAYLOAD` admission gate over the `TemplatePayload` `TypedDict`, `ValidationError` the seam fault mapped to `<invalid-payload>`); `msgspec` (`Struct(frozen=True)`/`field` the `TemplatePipeline` owner, `json.Encoder` the typst `sys.inputs` JSON coercion); `builtins.frozendict` (the `DELEGATES` mode table, the `TARGETS` arm table, the `bindings` data band, and the `assets` `asset_key.hex -> path` figure band, never a `MappingProxyType` view over a mutable dict and never a `dict[str, object]` interior bag); runtime (`content_identity.ContentKey` the keyed result, `faults.RuntimeRail` the rail type and `faults.traversed` the branch-canonical `Block[RuntimeRail[T]]`-to-`RuntimeRail[Block[T]]` fail-fast fan-out reducer, `receipts.Receipt` the row `contribute` yields); `core/receipt#RECEIPT` (`ArtifactReceipt` the `Keyed` payload type and `ArtifactReceipt.contribute()` the no-argument per-row stream `contribute` delegates to). No new external library and no directly-imported producer — every engine is held inside the emit or report owner the pipeline composes.
- Growth: a new output format with a `DocumentPlan` arm is one `TemplateTarget` row plus one `DELEGATES` `DocumentMode` row plus, where it carries distinct `EmitSpec` knobs, one `_emit_spec` `match` arm (zero new function); a new output format with a distinct producer (a sibling owner neither emit nor report) is one `TemplateTarget` row plus one `TARGETS` arm returning a `Keyed` pair; a new declared ingress key is one `TemplatePayload` `NotRequired[ReadOnly[]]` line plus one `TemplatePipeline` field plus one `of` materialization branch; a new template source is one `template` value; a new bound render value rides the `extra_items=object` band with zero schema edit, folded into `bindings` automatically; a new embeddable figure is one `assets` band entry threaded into the office arms with zero arm edit; a new archival PDF profile is one `PdfVariant` value threaded into the `TYPST_DATA` delegate; a new loader root is one `ReportLoader` value on the HTML delegate; a new sandbox policy is the `trusted` flag the report owner's `report_env` reads; a new format batched into the sheet-set family is automatic — the modal `bound` already fans any `TemplateTarget` family with zero edit. A richer ODF lowering is NOT a growth site here — it is a growth site on `document/emit#DOCUMENT`'s `ODT` arm, which the pipeline reaches by delegation. Zero new surface — the pipeline grows by target row, always delegating to the sibling owner that holds the producer.
- Boundary: no `jinja2.Environment`, no `DocxTemplate`, no typst `Compiler`, no `python-pptx` `Presentation`, and no odfpy `OpenDocument` construction of its own — the `document/emit#DOCUMENT` `DocumentPlan` owns the docxtpl/typst/`python-pptx`/odfpy/structured-text arms and the `document/report#REPORT` `ReportPlan` owns the jinja2 arm, and `TemplatePipeline` binds the owner and constructs them, owning NO engine. The deleted forms: an owned `_odf_lower` odfpy `OpenDocumentText` recursion (the `attach`/`inject`/`emphasis`/`direction` fold, the `_odf_keyed`/`_cell_runs` helpers, the `to_thread` offload, the `ArtifactReceipt.Office(key, len(blob))` mint) justified by the illusory claim "no emit/report owner declares an ODF WRITE arm; a phantom emit ODF `DocumentMode` does not exist" — `document/emit#DOCUMENT` DECLARES `DocumentMode.ODT`/`ODS` and lowers the same `DocumentNode` tree to `.odt`/`.ods`, so an owned arm is a double-owned re-implementation of the document-lowering owner's job and the `ODF` target collapses into the `DELEGATES` row `ODF -> ODT`; a `TemplateContext` frozen value wrapped one-field-deep inside `TemplatePipeline` where the admitted owner IS the dispatcher (the wrapper added no invariant and forced a `pipeline.context` hop on every arm), collapsed into the one `TemplatePipeline` owner carrying both the bound fields and the dispatch; an `assets: frozendict[str, tuple[bytes, str]]` in-memory `(bytes, mediatype)` band where the emit owner resolves figures by `asset_key.hex -> path` through `EmitSpec.assets` and the office arms read that band, replaced by the path-aligned `assets: frozendict[str, str]` threaded into every office arm so the same figures embed across `DOCX`/`PPTX`/`ODF` rather than only an owned ODF arm; a `TemplatePipeline.contribute(self, outcomes)` instance method that ignores `self` where a module-level `contribute(outcomes)` fold drains the carried rows; a `TemplatePayload.template` bare `ReadOnly[str]` where the doctrine and the sibling `EmitPayload`/`ReportPayload` ingress declare the required identity key `Required[ReadOnly[str]]`; a phantom `DocumentPlan(mode=, node=, params=...)` constructor where the emit owner takes a typed `spec: EmitSpec`, replaced by `DocumentPlan(mode=, node=pipeline.node, spec=_emit_spec(target, pipeline))` constructed directly from the trusted owner with no second admission; a `pdf_standards: tuple[str, ...]` field where the emit owner takes a `variant: PdfVariant` projecting to both the typst `pdf_standards` and the weasyprint profile; a `bindings`-as-docxtpl-context model where the emit docxtpl arm derives its context from the `DocumentNode` tree (so a render binding reaches only the typst `sys.inputs` and the jinja2 `sections`, never the docx context); a `dict[str, object]` `bindings` bag a binding arm re-coerces and a raw `TemplatePipeline(...)` construction a caller fills by hand, where `TemplatePipeline.of` admits the closed `TemplatePayload` once through the `_PAYLOAD` `TypeAdapter`; a `create_memory_object_stream` result-gather rig draining each child's rail off a cloned `MemoryObjectSendStream` where `start_soon` returns the `TaskHandle` whose `return_value` already carries the child's rail (the concurrency `CHILD_CARRIER` law's rejected "stream gather rig"); a claim that `TemplatePipeline` is itself the `ReceiptContributor` the runtime keys where the carried `ArtifactReceipt`s are the contributors and the module `contribute(outcomes)` is the drain a caller folds; an `_html` arm that projects the `ReportPlan.render` pair to the key and DISCARDS the report's `report`/`pdf` rows, where the rebuilt arm threads the whole in-band `Keyed` pair; a fabricated zero-byte emit receipt on any emit-delegated arm where the emit owner weave-emits its own row and the arm carries an empty tuple; a per-format parameter bag or the parallel `context`/`sections` two-dict split where one `node`+`bindings` owner binds every target; a `TemplateBinding` descriptor struct and a `BINDINGS` table read by nothing — the `DELEGATES` mode row and the `TARGETS` arm carry the dispatch; a `_docx`/`_pdf`/`_pptx`/`_odf`/`_xml`/`_yaml`/`_toml` sibling-function family where the seven `DocumentPlan` arms differ only by `(DocumentMode, EmitSpec)` and collapse into one `_via_document` derived dispatch; an `if target == ...` branch where `DELEGATES`/`TARGETS` dispatch; a single-`target` owner with a sheet-set helper beside it where the modal `bound` discriminates `TemplateTarget | Iterable[TemplateTarget]`; and a `rail.value` crack mid-flow where the delegated rail threads unchanged. `TemplatePipeline` is the bind-and-route layer composing the emit and report binding owners, never a re-implementation of a binding a sibling already owns.

```python signature
from collections.abc import Awaitable, Callable, Iterable
from enum import StrEnum
from typing import Final, Literal, NotRequired, ReadOnly, Required, TypedDict, Unpack, overload

import msgspec
from anyio import TaskHandle, create_task_group
from builtins import frozendict
from expression import Block, Error, Ok, Result
from msgspec import Struct, field
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import RuntimeRail, traversed
from rasm.runtime.receipts import Receipt

from artifacts.core.receipt import ArtifactReceipt
from artifacts.document.emit import DocumentMode, DocumentPlan, EmitSpec, PdfVariant, produced
from artifacts.document.model import DocumentNode
from artifacts.document.report import FigureRef, ReportKind, ReportLoader, ReportPlan

# --- [TYPES] ----------------------------------------------------------------------------


class TemplateTarget(StrEnum):
    DOCX = "docx"
    PDF = "pdf"
    HTML = "html"
    PPTX = "pptx"
    ODF = "odf"
    XML = "xml"
    YAML = "yaml"
    TOML = "toml"


type Keyed = tuple[ContentKey, tuple[ArtifactReceipt, ...]]
type Bind = Callable[["TemplatePipeline", TemplateTarget], Awaitable[RuntimeRail[Keyed]]]
type TemplateFault = Literal["<invalid-payload>"]

# --- [MODELS] ---------------------------------------------------------------------------


class TemplatePayload(TypedDict, extra_items=object):
    template: Required[ReadOnly[str]]
    figures: NotRequired[ReadOnly[tuple[FigureRef, ...]]]
    assets: NotRequired[ReadOnly[frozendict[str, str]]]
    variant: NotRequired[ReadOnly[PdfVariant]]
    loader: NotRequired[ReadOnly[ReportLoader]]
    trusted: NotRequired[ReadOnly[bool]]

# --- [OPERATIONS] -----------------------------------------------------------------------


def _emit_spec(target: TemplateTarget, pipeline: "TemplatePipeline", /) -> EmitSpec:
    # the node is the structural carrier every emit arm lowers; the per-mode `EmitSpec` carries only
    # the knobs — typst reads the JSON-coerced `bindings` as `sys.inputs` (str-valued), the office
    # arms the bound template path plus the `asset_key.hex -> path` figure band, the rest nothing.
    match target:
        case TemplateTarget.PDF:
            inputs = frozendict({key: value if isinstance(value, str) else _INPUT.encode(value).decode() for key, value in pipeline.bindings.items()})
            return EmitSpec(sys_inputs=inputs, variant=pipeline.variant)
        case TemplateTarget.DOCX | TemplateTarget.PPTX | TemplateTarget.ODF:
            return EmitSpec(template=pipeline.template, assets=pipeline.assets)
        case _:
            return EmitSpec()


async def _via_document(pipeline: "TemplatePipeline", target: TemplateTarget, /) -> RuntimeRail[Keyed]:
    # the emit owner's production surface is the `produced` modal entrypoint; a lone plan normalizes
    # to a one-element `Block`, so its head key is the delegated row, and the emit `@receipted` weave
    # already emits the row's `ArtifactReceipt`, so this arm carries an empty in-band tuple.
    rail = await produced(DocumentPlan(mode=DELEGATES[target], node=pipeline.node, spec=_emit_spec(target, pipeline)))
    return rail.map(lambda keys: (keys.head(), ()))


async def _html(pipeline: "TemplatePipeline", _target: TemplateTarget, /) -> RuntimeRail[Keyed]:
    # `ReportPlan.render` already returns the `(ContentKey, receipts)` rail in-band, so the arm
    # threads the whole pair unchanged, keeping the report owner's `report`/`pdf` rows.
    return await ReportPlan(
        kind=ReportKind.TEMPLATE, source=pipeline.template, sections=dict(pipeline.bindings),
        figures=pipeline.figures, loader=pipeline.loader, trusted=pipeline.trusted,
    ).render()


def contribute(outcomes: Block[Keyed], /) -> Iterable[Receipt]:
    # the in-band drain a composing caller folds to surface the HTML arm's carried `report`/`pdf`
    # rows; the emit-delegated arms carry empty tuples because the emit `@receipted` weave owns them.
    for _key, receipts in outcomes:
        for receipt in receipts:
            yield from receipt.contribute()

# --- [TABLES] ---------------------------------------------------------------------------

DELEGATES: Final[frozendict[TemplateTarget, DocumentMode]] = frozendict({
    TemplateTarget.DOCX: DocumentMode.DOCX_TEMPLATE,
    TemplateTarget.PDF: DocumentMode.TYPST_DATA,
    TemplateTarget.PPTX: DocumentMode.PPTX,
    TemplateTarget.ODF: DocumentMode.ODT,
    TemplateTarget.XML: DocumentMode.XML,
    TemplateTarget.YAML: DocumentMode.YAML,
    TemplateTarget.TOML: DocumentMode.TOML,
})

TARGETS: Final[frozendict[TemplateTarget, Bind]] = frozendict({TemplateTarget.HTML: _html})

_PAYLOAD: Final = TypeAdapter(TemplatePayload)
_INPUT: Final = msgspec.json.Encoder()
_DECLARED: Final[frozenset[str]] = TemplatePayload.__required_keys__ | TemplatePayload.__optional_keys__

# --- [COMPOSITION] ----------------------------------------------------------------------


class TemplatePipeline(Struct, frozen=True):
    template: str
    node: DocumentNode
    bindings: frozendict[str, object] = field(default_factory=frozendict)
    figures: tuple[FigureRef, ...] = ()
    assets: frozendict[str, str] = field(default_factory=frozendict)
    variant: PdfVariant = PdfVariant.NONE
    loader: ReportLoader = ReportLoader.DICT
    trusted: bool = True

    @classmethod
    def of(cls, node: DocumentNode, /, **raw: Unpack[TemplatePayload]) -> Result["TemplatePipeline", TemplateFault]:
        try:
            payload = _PAYLOAD.validate_python(raw)
        except ValidationError as fault:
            fault.add_note(f"<template.payload:{[error['loc'] for error in fault.errors()]}>")
            return Error("<invalid-payload>")
        bindings = frozendict({name: value for name, value in payload.items() if name not in _DECLARED})
        return Ok(cls(
            template=payload["template"],
            node=node,
            bindings=bindings,
            figures=payload.get("figures", ()),
            assets=payload.get("assets", frozendict()),
            variant=payload.get("variant", PdfVariant.NONE),
            loader=payload.get("loader", ReportLoader.DICT),
            trusted=payload.get("trusted", True),
        ))

    @overload
    async def bound(self, target: TemplateTarget, /) -> RuntimeRail[Keyed]: ...
    @overload
    async def bound(self, target: Iterable[TemplateTarget], /) -> RuntimeRail[Block[Keyed]]: ...
    async def bound(self, target: TemplateTarget | Iterable[TemplateTarget], /) -> RuntimeRail[Keyed] | RuntimeRail[Block[Keyed]]:
        # `TemplateTarget()` is read before the `Iterable` arm because a `StrEnum` is itself
        # iterable; the overloads carry the per-modality output shape so a caller narrows on the
        # target it passes, never re-matching the runtime union.
        match target:
            case TemplateTarget() as one:
                return await self._dispatch(one)
            case stream:
                return await self._fanned(Block.of_seq(stream))

    async def _dispatch(self, target: TemplateTarget, /) -> RuntimeRail[Keyed]:
        arm = _via_document if target in DELEGATES else TARGETS[target]
        return await arm(self, target)

    async def _fanned(self, targets: Block[TemplateTarget], /) -> RuntimeRail[Block[Keyed]]:
        # each child rails its own faults so the group exits clean and every `TaskHandle` settles
        # `FINISHED`; the post-scope `return_value` read carries each rail back with no re-raise —
        # the concurrency `CHILD_CARRIER` law, never a memory-stream result-gather rig.
        async with create_task_group() as group:
            handles: Block[TaskHandle[RuntimeRail[Keyed]]] = targets.map(lambda target: group.start_soon(self._dispatch, target))
        return traversed(handles.map(lambda handle: handle.return_value))
```

## [03]-[SIGNALS]

- [ODT_DEEPEN] [BLOCKED]: the `ODF` target delegates to `document/emit#DOCUMENT` `DocumentMode.ODT`, whose current `_odf_emit`/`_odf_block`/`_odf_text` arm is a SHALLOW lowering — plain `text.H`/`text.P` over `addTextToElement` with runs flattened to `"".join(run.text for run in runs)`, list items as loose paragraphs, and no tables, figures, annotations, run styling, or RTL. Deepen the emit `ODT` arm to the full `DocumentNode` lowering so the delegated `ODF` render is rich: a `match`-per-node `attach`/`inject`/`emphasis`/`direction` fold authoring `text.Span` over a cached appearance-keyed `style.Style`+`TextProperties` for EVERY run (weight/italic/`fontname`/`fontsize`/`color`/`textposition` from `RunScript`/`fo:language`+`fo:country` from `NodeMeta.lang`), `text.List`/`text.ListItem` nesting, `table.Table`+`TableHeaderRows`+`CoveredTableCell`+`numberrowsspanned`/`numbercolumnsspanned` over the `TableNode.spans` `(row, col, col_span, row_span)` quad, `FigureNode` embed through `addPictureFromString` resolved from `EmitSpec.assets`, `AnnotationNode` LINK/NOTE/markup via `text.A`/`text.Note(noteclass="footnote")`/`office.Annotation`, RTL via `style.ParagraphProperties(writingmode="rl-tb")`, and `load(spec.template)` template support — the rich fold previously authored here (recoverable from git), relocated to the document-lowering owner where it belongs. Spans `core/format.md`, `document/emit.md`, `.api/odfpy.md`.
- [ODF_CATALOG] [BLOCKED]: the `.api/odfpy.md` folder catalogue documents `OpenDocumentText`, `Frame`/`Image`, `P`/`Span`/`H`, `Style`/`TextProperties`, `Table`/`TableRow`/`TableCell`/`CoveredTableCell`/`TableHeaderRows`, `addPictureFromString`, `getStyleByName`, and `addTextToElement`, but NOT the write-side members the deepened `ODT` arm composes — `odf.text.A`/`List`/`ListItem`/`Note`/`NoteCitation`/`NoteBody`, `odf.style.ParagraphProperties`, and `odf.office.Annotation` (all verified present in the installed `odfpy 1.4.1` surface). Add these rows so the emit `ODT` deepening cites only catalogued members. Spans `.api/odfpy.md`, `document/emit.md`.
- [REPORT_RAIL_SEAM] [BLOCKED]: `document/report.md` is internally inconsistent on the `ReportPlan.render` contract — its prose states `render -> RuntimeRail[ContentKey]` with a `@receipted` weave and `_emit -> Self`, while its authoritative signature declares `render -> RuntimeRail[tuple[ContentKey, tuple[ArtifactReceipt, ...]]]` and `_emit -> tuple[ContentKey, tuple[ArtifactReceipt, ...]]` returning receipts IN-BAND with no weave. The `_html` arm here depends on the SIGNATURE (the in-band `Keyed` pair threaded unchanged). Reconcile the report prose to its signature (in-band receipts, `render` returns the `Keyed` pair); if instead the `@receipted`-weave form is intended, `_html` must map the bare `ContentKey` to `(key, ())` like every emit arm. Spans `document/report.md`, `core/format.md`.
- [ASSET_BAND] [BLOCKED]: the pipeline carries figure assets as `assets: frozendict[str, str]` (`asset_key.hex -> path`) to match the emit owner's `EmitSpec.assets: frozendict[str, str]` path band, threaded into the `DOCX`/`PPTX`/`ODF` office arms. A binding/template engine more naturally carries IN-MEMORY render-pipeline figure bytes; decide whether `EmitSpec.assets` should widen to `frozendict[str, tuple[bytes, str]]` (`(bytes, mediatype)`, embedded through odfpy `addPictureFromString` / docx/pptx/reportlab `BytesIO`) so binding-time figures need no temp-file materialization, or whether the path band stays canonical and the pipeline materializes. Spans `document/emit.md`, `core/format.md`, `document/report.md`.
