# [PY_ARTIFACTS_DASHBOARD]

`DashboardPlan` composes already-rendered sibling panes into ONE offline single-file HTML artifact — filterable Vega charts, great-tables schedule and register tables, and named-layer diagram SVG — so a receipts roll-up, a QTO takeoff, and a delivery register read as one shareable document with no server, no CDN, and no live UI. It renders no chart, builds no table, and lays out no graph: each pane arrives as the bytes or spec its owning producer already answers, and this owner spends every line on the shared-runtime problem those producers cannot solve alone.

Bundle sharing is the whole reason this owner exists. `vl_convert.vegalite_to_html` answers a WHOLE document per spec on the hardcoded `vega-chart` div with no output-div override, and `altair` `Chart.to_html(inline=True)` embeds the same runtime while ignoring `fullhtml`, so either path repeats the Vega runtime once per pane. One `vl_convert.javascript_bundle(vl_version=…)` call carries it ONCE, its default snippet publishing `vegaEmbed`/`vega`/`vegaLite` onto `window`, and each chart pane mounts as one `vegaEmbed('#<slot>', <spec>, <opts>)` against that shared global.

Interactivity survives the `visualization/chart/export#PREPASS` `VegaTransform` `state` arm, which strips no signal. Table panes take `great_tables` `as_raw_html` at `inline_css=False`, whose scoped `<style>` travels inside the emitted div and needs no unadmitted inliner. Markup assembly rides `string.templatelib.Template` under one destination-keyed escaper: a pane title, a slot id, and a spec body each escape against a different grammar, and a JSON body inside `<script>` neutralizes `</script>` before a browser reads it as markup.

## [01]-[INDEX]

- [02]-[DASHBOARD]: `DashPane` pane vocabulary, `DashboardPlan` single-bundle composition fold, and the escaped single-file emit minting `ArtifactReceipt.Dashboard`.

## [02]-[DASHBOARD]

- Owner: `DashboardPlan` folds a `Block[DashPane]` into one self-contained document. `DashPane` is the closed pane vocabulary — `chart` carrying a `visualization/chart/spec#CHART` `ChartSpec` vega case beside its embed options, `table` carrying the HTML fragment `visualization/table#TABLE` `TablePlan.build` already answered, `figure` carrying the SVG bytes `visualization/diagram/draw#DRAW` or `visualization/chart/export#EXPORT` `layered()` already answered — and each case pairs with one `PaneSlot` stating id, title, and grid span. `Grid` carries the sheet geometry as data (`columns`, `gap`, `max_width`) so a two-up register and a four-up receipts board differ by row, never by branch. `DashFault` (`<no-panes>`/`<duplicate-slot>`/`<fenced-html>`/`<non-vega-pane>`) closes the refusal vocabulary, each case naming what admission proved rather than a boolean the caller re-derives.
- Cases: `chart` admits the vega case ALONE and rails `<non-vega-pane>` on the others — a `lets_plot` or `matplotlib` case owns no Vega spec the shared runtime mounts, and its raster or SVG output enters as a `figure` pane instead, so one embedded runtime serves every interactive pane and no second engine bundle rides along. `table` and `figure` carry finished bytes because their producers already crossed the lane: re-rendering here forks the content key that producer minted and re-opens a provider this owner never opens.
- Entry: `DashboardPlan.of` is the one construction arity — a lone `DashPane` or any `Iterable` normalized at the head by input shape — and it returns `Result[DashboardPlan, DashFault]` so an empty deck and a slot collision refuse at admission rather than emitting a document whose panes silently overwrite each other. `emit()` returns one `ArtifactWork` whose `parents` are every pane's producing `ContentKey`, so `core/plan#PLAN` schedules each producer ahead of the composition and a warm pane elides without re-rendering; admission is `Admission(keyed=None)` because a dashboard is content-determined by its panes' own keys.
- Auto: `_key` hands the runtime `IdentitySource.parts` fold — the one owner of preimage framing — its canonical field set: each pane's producer key and canonically-encoded payload in slot order, and the encoded `(grid, title, policy)` bundle, so a pane reorder, a grid change, and a policy change each miss the cache while an identical deck hits it. `_bundled` calls `vlc.javascript_bundle(vl_version=pin_version(policy.vl_version))` exactly once per document and only when a chart pane is present, so a table-and-figure deck carries no JavaScript at all. Each chart pane crosses `VegaTransform.of(spec, ExportFormat.HTML, transform, retention).apply(spec)` on the pre-pass owner, whose `state` arm preserves every signal and dataset the interactive mount needs; its `PrePassEvidence` folds onto the receipt band rather than a span event, because the composition opens ONE span for the whole document.
- Auto: `_document` builds the page as one `Template` whose every interpolation names its destination — `SLOT` for an attribute-position id, `TEXT` for a title, `JSON` for a spec body inside `<script>`, `RAW` for producer-owned markup — and `_escaped` folds each through the `_ESCAPE` row that destination names. `JSON` carries the load-bearing fold: it neutralizes `</script>`, `<!--`, and the U+2028/U+2029 separators a JSON encoder emits raw, each terminating or reframing the script element a browser parses, while staying JSON-valid where an HTML-entity escape corrupts the value the parser then reads.
- Output: one `bytes` document — a `<style>` head carrying the grid rules, one `<script>` carrying the shared runtime, each pane's own markup inside its slot, and one trailing `<script>` mounting every chart. Byte-identical output for identical input holds because every map crosses the deterministic encoder and every pane renders in declared slot order.
- Receipt: `emit()` mints the key ONCE and captures it into the work closure, so the receipt threads the pre-run key rather than re-walking every pane's preimage and re-opening a second `content.derive` span for one artifact. `ArtifactReceipt.Dashboard(key, bytes_, panes, charts, tables, diagrams, facts)` mints the banded kind `core/receipt#RECEIPT` carries — `bytes` reaching `_METRIC` as byte volume like every sibling, the three counts stating the deck's composition, and the `pane` band folding each chart pane's pre-pass evidence so a row-limited or interactivity-broken transform stays addressable evidence rather than a silent degradation. `_emit` then awaits `Journal.record` over `receipt.evidence()` — one `OPERATIONAL` fact for the COMPOSITION, its diff naming the deck's own counts and byte volume where each pane's producer already recorded its own artifact; the band never enters that diff, and the seat is that awaitable fold because recording suspends where `contribute` cannot.
- Span: the composition's one native crossing carries interior stages the lane aspect cannot attribute — the shared-runtime bundle beside every pane's pre-pass — so this owner opens exactly one span and folds its egress INSIDE that scope through the runtime `faulted`, which marks `ERROR` and emits the correlated `fault.facts()` line before the span closes. The fold is the charter's, composed rather than re-spelled: a page-local twin forks the one behavior every span-carrying producer owes.
- Packages: `vl-convert-python` (`javascript_bundle` the one shared runtime, `get_vegalite_versions` the pin); `vegafusion` reached only through `visualization/chart/export#PREPASS` `VegaTransform`; `msgspec` (`Struct` the owners, `json.Encoder(order="deterministic")` the canonical preimage and the spec bodies); `expression` (`Block`/`Map` the folds, `tagged_union` the pane and fault families); `string.templatelib` (the markup `Template` this page folds); `opentelemetry-api` (the one span, opened through the runtime-versioned `scoped` triple); runtime (`identity.ContentIdentity`/`ContentKey`, `journal.Journal` the durable seat's one writer, `lanes.LanePolicy`, `workers.Kernel`/`KernelTrait`, `faults.RuntimeRail`/`BoundaryFault`/`scoped`/`faulted` — the charter's error fold composed, never re-spelled, so this page owns no logger); `core/plan#PLAN` (`Admission`/`ArtifactWork`), `core/receipt#RECEIPT` (`ArtifactReceipt`). No `great_tables`, `altair`, `lxml`, or diagram surface imports — their bytes arrive finished.
- Growth: a new pane kind is one `DashPane` case, one `_paned` arm, and one receipt count; a new grid axis is one `Grid` field read by the style fold; a new escape destination is one `Destination` member with its `_ESCAPE` row, an absent row failing at type-check rather than emitting unescaped; a new embed knob is one `EmbedOptions` field the mount projection spreads; a new pre-pass mode arrives free, the pre-pass owner already dispatching it.
- Boundary: no chart authoring, no table building, no diagram layout, and no rasterization — every pane's producer owns its own render, content key, and receipt, and this owner composes their bytes. No live server, no CDN reference, no WebSocket, and no external fetch: embedding is the invariant rather than a policy, so a `ChartRenderPolicy` carrying `allowed_base_urls` refuses `<fenced-html>` exactly as `visualization/chart/export#EXPORT` refuses it, since a browser-side render enforces no fence. `great_tables` `inline_css=True` stays the rejected pane source — it needs the unadmitted `css-inline` distribution while the default scoped `<style>` already travels inside the emitted div. Every dynamic value reaches markup through a `Template` interpolation carrying its destination, so an f-string, `%`-format, or `str.format` splice is the rejected assembly form, and a per-pane `vegalite_to_html` document the rejected composition form.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from enum import StrEnum
from functools import partial
from string.templatelib import Interpolation, Template
from typing import Final, Literal, Self, assert_never

import vl_convert as vlc
from builtins import frozendict
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block
from expression.extra.result import traverse
from msgspec import Struct, json, structs
from opentelemetry import trace

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.visualization.chart.export import (
    ChartRenderPolicy,
    ExportFormat,
    PrePassEvidence,
    Retention,
    TransformPolicy,
    VegaTransform,
    pin_version,
)
from rasm.artifacts.visualization.chart.spec import ChartSpec
from rasm.runtime.faults import BoundaryFault, RuntimeRail, faulted, scoped
from rasm.runtime.identity import ContentIdentity, ContentKey, IdentitySource
from rasm.runtime.journal import Journal
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

# --- [TYPES] ----------------------------------------------------------------------------
type DashFault = Literal["<no-panes>", "<duplicate-slot>", "<fenced-html>", "<non-vega-pane>"]
type Spec = dict[str, object]
type Escaper = Callable[[object], str]

# --- [CONSTANTS] ------------------------------------------------------------------------
_TRACER: Final = scoped(trace.get_tracer, "rasm.artifacts.visualization.dashboard")
_CANON: Final = json.Encoder(order="deterministic")

# `</script>` ends the element mid-JSON and `<!--` reframes the remainder as a comment, so both neutralize inside the
# string literal the JSON already is; U+2028/U+2029 are raw in JSON yet terminate a JavaScript line, so both escape to
# their `\u` form. Every replacement stays JSON-valid — an HTML entity here would corrupt the value the parser reads.
_SCRIPT_SAFE: Final[tuple[tuple[str, str], ...]] = (
    ("</", "<\\/"),
    ("<!--", "\\u003c!--"),
    ("\u2028", "\\u2028"),
    ("\u2029", "\\u2029"),
)
_MARKUP: Final[tuple[tuple[str, str], ...]] = (("&", "&amp;"), ("<", "&lt;"), (">", "&gt;"))
_ATTRIBUTE: Final[tuple[tuple[str, str], ...]] = (*_MARKUP, ('"', "&quot;"), ("'", "&#x27;"))


# --- [MODELS] ---------------------------------------------------------------------------
class Destination(StrEnum):
    # `Destination` names the grammar an interpolation lands in and `_ESCAPE` rows one fold per member, so a member
    # naming no row fails at the table read rather than emitting an unescaped splice.
    SLOT = "slot"
    TEXT = "text"
    JSON = "json"
    RAW = "raw"


def _folded(value: str, rows: tuple[tuple[str, str], ...], /) -> str:
    return Block.of_seq(rows).fold(lambda text, pair: text.replace(pair[0], pair[1]), value)


# the row VALUE is typed, not erased to `object`: an `object`-valued table makes `_ESCAPE[dest](value)` an unchecked
# call, so the "an absent row fails at type-check rather than emitting unescaped" guarantee holds for the KEY alone
# and a row returning the wrong shape splices unescaped past every checker.
_ESCAPE: Final[frozendict[Destination, Escaper]] = frozendict({
    Destination.SLOT: lambda value: _folded(str(value), _ATTRIBUTE),
    Destination.TEXT: lambda value: _folded(str(value), _MARKUP),
    Destination.JSON: lambda value: _folded(_CANON.encode(value).decode(), _SCRIPT_SAFE),
    # RAW carries producer-owned markup this page never re-escapes — a great-tables div and a diagram SVG are already
    # well-formed documents their own owners built, and escaping them would render their source as visible text.
    Destination.RAW: lambda value: value.decode() if isinstance(value, bytes) else str(value),
})


class PaneSlot(Struct, frozen=True):
    # `slot` is the DOM id every chart mount targets, so admission proves it unique across the deck before render.
    slot: str
    title: str = ""
    span: int = 1


class Grid(Struct, frozen=True):
    columns: int = 2
    gap: str = "1rem"
    max_width: str = "1400px"

    def styled(self) -> Template:
        return t"""main{{display:grid;grid-template-columns:repeat({self.columns},minmax(0,1fr));
gap:{self.gap};max-width:{self.max_width};margin:0 auto}}
section{{min-width:0;overflow-x:auto}}section>h2{{font:600 0.95rem system-ui;margin:0 0 .4rem}}"""


class EmbedOptions(Struct, frozen=True):
    # vega-embed's own option bag as a typed row: the mount spreads it, so a new knob never widens a signature.
    renderer: Literal["svg", "canvas", "hybrid"] = "svg"
    actions: bool = False
    tooltip: bool = True


@tagged_union(frozen=True)
class DashPane:
    tag: Literal["chart", "table", "figure"] = tag()
    chart: tuple[PaneSlot, ContentKey, ChartSpec, EmbedOptions] = case()
    table: tuple[PaneSlot, ContentKey, bytes] = case()
    figure: tuple[PaneSlot, ContentKey, bytes] = case()

    @property
    def placed(self) -> PaneSlot:
        match self:
            case DashPane(tag="chart", chart=(slot, *_)) | DashPane(tag="table", table=(slot, *_)) | DashPane(tag="figure", figure=(slot, *_)):
                return slot
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def parent(self) -> ContentKey:
        # `parent` serves as BOTH the plan edge and a preimage chunk, so a re-rendered producer shifts this deck's
        # own key while a warm pane elides its producer without touching the composition.
        match self:
            case DashPane(tag="chart", chart=(_, key, *_)) | DashPane(tag="table", table=(_, key, _)) | DashPane(tag="figure", figure=(_, key, _)):
                return key
            case _ as unreachable:
                assert_never(unreachable)


class DashRenderPolicy(Struct, frozen=True):
    # `chart` reuses the export owner's own policy value rather than a twin, so one rename there breaks here.
    chart: ChartRenderPolicy = ChartRenderPolicy()
    transform: TransformPolicy = TransformPolicy()
    retention: Retention = Retention(preserve_interactivity=True)


# --- [OPERATIONS] -----------------------------------------------------------------------




def _escaped(rendered: Template, /) -> str:
    # ONE render-time fold over the template's own segments: a static chunk passes through and every interpolation
    # crosses the row its `format_spec` names, defaulting to TEXT so an unmarked interpolation escapes rather than
    # leaks. This is why the whole document is a Template — an f-string renders before any escape point can run.
    return "".join(
        part if isinstance(part, str) else _ESCAPE[Destination(part.format_spec or Destination.TEXT.value)](part.value) for part in rendered
    )


def _body(pane: DashPane, /) -> Template:
    match pane:
        case DashPane(tag="chart", chart=(slot, _key, _spec, _opts)):
            return t"""<div id="{slot.slot:slot}" class="vega"></div>"""
        case DashPane(tag="table", table=(_slot, _key, html)) | DashPane(tag="figure", figure=(_slot, _key, html)):
            return t"{html:raw}"
        case _ as unreachable:
            assert_never(unreachable)


def _paned(pane: DashPane, /) -> Template:
    placed = pane.placed
    return t"""<section style="grid-column:span {placed.span:text}"><h2>{placed.title:text}</h2>{_escaped(_body(pane)):raw}</section>"""


def _mounted(slot: str, spec: Spec, options: EmbedOptions, /) -> Template:
    return t"vegaEmbed('#{slot:slot}',{spec:json},{structs.asdict(options):json});"


# --- [COMPOSITION] ----------------------------------------------------------------------
class DashboardPlan(Struct, frozen=True):
    panes: Block[DashPane]
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    lane: LanePolicy
    title: str = ""
    grid: Grid = Grid()
    policy: DashRenderPolicy = DashRenderPolicy()

    @classmethod
    def of(
        cls,
        panes: DashPane | Iterable[DashPane],
        *,
        lane: LanePolicy,
        title: str = "",
        grid: Grid = Grid(),
        policy: DashRenderPolicy = DashRenderPolicy(),
    ) -> Result[Self, DashFault]:
        held = Block.singleton(panes) if isinstance(panes, DashPane) else Block.of_seq(panes)
        slots = tuple(pane.placed.slot for pane in held)
        wrong = tuple(pane for pane in held if pane.tag == "chart" and pane.chart[2].tag != "vega")
        return (
            Error("<no-panes>")
            if held.is_empty()
            else Error("<duplicate-slot>")
            if len(frozenset(slots)) != len(slots)
            else Error("<non-vega-pane>")
            if wrong
            else Error("<fenced-html>")
            if policy.chart.allowed_base_urls
            else Ok(cls(panes=held, lane=lane, title=title, grid=grid, policy=policy))
        )

    def emit(self, /) -> ArtifactWork:
        # every pane's producer is a PARENT edge, so the plan fronts them ahead of this node and a warm pane elides.
        # ONE mint, captured into the closure: `_key` re-walks every pane's preimage and re-opens a `content.derive`
        # span per access, so a receipt re-deriving it doubles both for one artifact.
        key = self._key
        parents = tuple(pane.parent for pane in self.panes)
        return ArtifactWork(key=key, work=partial(self._emit, key), parents=parents, admission=Admission(keyed=None), cost=float(len(self.panes) or 1))

    @property
    def _seed(self) -> tuple[bytes, ...]:
        panes = tuple(
            piece
            for pane in self.panes
            for piece in (
                pane.parent.hex.encode(),
                _CANON.encode((pane.tag, structs.asdict(pane.placed))),
            )
        )
        # the WHOLE policy joins the preimage, never its chart block alone: `transform` and `retention` both reach
        # `_staged` and change the emitted bytes, so a deck differing only by pre-pass policy would otherwise alias
        # a cached artifact rendered under the other one — and a new policy field joins by construction.
        bundle = _CANON.encode((self.title, structs.asdict(self.grid), structs.asdict(self.policy)))
        return (*panes, bundle)

    @property
    def _key(self) -> ContentKey:
        # `parts`, never a bare tuple: an `Iterable[bytes]` lifts to `stream`, which concatenates chunk bytes with
        # no delimiter — right for buffer chunks of ONE payload, wrong for N semantic fields whose boundary IS meaning.
        return ContentIdentity.key("dashboard-html", IdentitySource(parts=self._seed))

    def _staged(self, pane: DashPane, /) -> Result[tuple[str, Spec, EmbedOptions, PrePassEvidence], BoundaryFault]:
        # `VegaTransform`'s `state` arm strips no signal, so an interactive mount keeps every declared selection
        # while server-evaluated transforms inline as reduced data inside the spec itself.
        match pane:
            case DashPane(tag="chart", chart=(slot, _key, spec, options)):
                staged = VegaTransform.of(spec.vega, ExportFormat.HTML, self.policy.transform, self.policy.retention).apply(spec.vega)
                return staged.map(lambda pre: (slot.slot, pre.spec, options, pre.evidence)).map_error(
                    lambda fault: BoundaryFault(boundary=(f"dashboard.prepass.{slot.slot}", fault))
                )
            case _ as unreachable:
                assert_never(unreachable)

    def _composed(self) -> Result[tuple[bytes, tuple[PrePassEvidence, ...]], BoundaryFault]:
        # `traverse` is the substrate's own applicative threader — it short-circuits the whole block on the first
        # refused pre-pass, so a deck never emits with one pane silently unstaged; the empty-chart deck threads the
        # empty block and renders table and figure panes with no runtime at all.
        charts = self.panes.filter(lambda pane: pane.tag == "chart")
        return traverse(self._staged, charts).map(self._document)

    def _document(self, staged: Block[tuple[str, Spec, EmbedOptions, PrePassEvidence]], /) -> tuple[bytes, tuple[PrePassEvidence, ...]]:
        # ONE bundle for the whole document — its default snippet publishes `vegaEmbed`/`vega`/`vegaLite` onto
        # `window`, which is exactly the global every per-pane mount below resolves against. A table-and-figure deck
        # stages no chart, so the bundle call never runs and the document carries no JavaScript at all.
        runtime = "" if staged.is_empty() else vlc.javascript_bundle(vl_version=pin_version(self.policy.chart.vl_version))
        mounts = "".join(_escaped(_mounted(slot, spec, options)) for slot, spec, options, _evidence in staged)
        sections = "".join(_escaped(_paned(pane)) for pane in self.panes)
        page = t"""<!doctype html><html lang="en"><head><meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1"><title>{self.title:text}</title>
<style>body{{margin:0;padding:1rem;font:14px/1.45 system-ui;background:#fff}}{_escaped(self.grid.styled()):raw}</style>
{runtime:raw}</head><body><h1>{self.title:text}</h1><main>{sections:raw}</main>
{mounts:raw}</body></html>"""
        return _escaped(page).encode(), tuple(evidence for _slot, _spec, _options, evidence in staged)

    def _banded(self, evidence: tuple[PrePassEvidence, ...], /) -> frozendict[str, float | str]:
        return frozendict({
            "prepass": float(len(evidence)),
            "row_limit_exceeded": float(sum(1 for one in evidence if one.row_limit_exceeded)),
            "interactivity_broken": float(sum(1 for one in evidence if one.interactivity_broken)),
            "unsupported": float(sum(1 for one in evidence if one.unsupported)),
            "warnings": float(sum(len(one.warnings) for one in evidence)),
        })

    async def _emit(self, key: ContentKey, /) -> RuntimeRail[ArtifactReceipt]:
        with _TRACER.start_as_current_span("dashboard.compose") as span:
            counts = frozendict({kind: sum(1 for pane in self.panes if pane.tag == kind) for kind in ("chart", "table", "figure")})
            span.set_attributes({"panes": len(self.panes), "charts": counts["chart"], "vega": vlc.get_vega_version()})
            built = await self.lane.offload(Kernel.of(self._composed, KernelTrait.HOSTILE))
            # egress fold closes INSIDE the span scope per the receipt spine's charter: a refused pre-pass or a dead
            # bundle call otherwise exits an UNSET span with no correlated line, and the deck's one native crossing
            # is exactly the interior the lane aspect cannot attribute.
            settled = (
                built.bind(lambda inner: inner)
                .map(
                    lambda pair: ArtifactReceipt.Dashboard(
                        key, len(pair[0]), len(self.panes), counts["chart"], counts["table"], counts["figure"], self._banded(pair[1])
                    )
                )
                .map_error(partial(faulted, span, "dashboard.compose"))
            )
            # ONE durable fact for the whole deck, and the composition's own: each pane's producer already recorded
            # its artifact, so this diff names the deck's composition — pane, chart, table, and figure counts — and
            # its byte volume, never a re-statement of what the panes already landed. The pre-pass band never enters
            # the diff: band leaves are this producer's instrumentation and the receipt owner filters them out by
            # construction. The seat is this awaitable fold inside the span, because recording suspends on a bounded
            # intake and `contribute` is the synchronous projection beside it.
            match settled:
                case Result(tag="ok", ok=receipt):
                    return (await Journal.record(receipt.evidence())).map(lambda _landed: receipt)
                case refused:
                    return Error(refused.error)


# --- [EXPORTS] ----------------------------------------------------------------------------

__all__ = ("DashPane", "DashRenderPolicy", "DashboardPlan", "Destination", "EmbedOptions", "Grid", "PaneSlot")
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
