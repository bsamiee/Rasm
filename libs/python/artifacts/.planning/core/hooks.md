# [PY_ARTIFACTS_HOOKS]

`ArtifactHook` is the artifact plane's production-fact point table — registered rows on the runtime `Hooks` registry, each a `rasm.artifacts.<domain>.<point>` id bound to one closed msgspec payload and one modality, so an app vetoes an issue pre-drain, audits every emitted receipt, and replays the last drain facts without touching a producer page. Telemetry is a tap: observability subscribes through the runtime `TapRow` cases, no producer page emits a hook or opens a span, and every payload projects FROM the receipt, fault, or drain evidence already in hand.

`Production` is the composed entry over the runtime registry: `registered` lands `ARTIFACT_POINTS` through `Hooks.register` under a scope-keyed one-shot cell and deposits its `ArtifactInstall` receipt on that registry's install ledger, so the support-bundle capsule reads this plane's admission where an absent row is its stated diagnosis. `fired` rides the cell so every seam self-registers before `Hooks.fire` and a hook-free app's veto gate passes clean, and `subscribed` is the app-root attach answering the `Attachment` detacher the composition root brackets.

Every arm threads the runtime `ScopeKey` composition axis under a `DEFAULT_SCOPE` keyword default, so two compositions embedding artifacts in one process partition points, subscribers, and replay rings structurally. `ISSUE_BAGGAGE` is the issue-scope correlation key every payload carries as `scope`; fire seams live at their emitting owners — the issue rails, the receipt `contribute` fold, and the transmittal close.

## [01]-[INDEX]

- [02]-[POINTS]: `ArtifactsLeg` raise-leg roster, `ArtifactHook` id vocabulary, its payload projections, the `ARTIFACT_POINTS` row table, the `scoped` baggage read, and the latched `Production` register/fire/subscribe surface.

## [02]-[POINTS]

- Owner: `ArtifactStage` is the folder's per-lane stage roster — the LABEL this plane hands each `Fronts` wave, which the lane's drive carries onto its `StageMark` and onto its deadline refusal, so progress on a long fold speaks the branch's one mark carrier rather than a payload-local position field. `ArtifactsLeg` is the folder's closed raise-leg roster — one member per artifacts module, the value its dotted path beneath `rasm` — seated here because this page reaches every raiser without a cycle, so a `FaultRow` anywhere in the folder anchors on a member and the `rostered` census proves it against a real module at import. `ArtifactHook` is the closed `StrEnum` id vocabulary — every member spells the runtime `HOOK_ID` grammar `rasm.artifacts.<domain>.<point>`, so a point id travels as a symbol, never a string literal a fire seam re-spells. `ARTIFACT_POINTS` is the one row table binding each id to its payload `Struct` and `Modality`; the table is the single growth site and no second registry, subscriber map, or ring exists at artifacts grain — the runtime `Hooks` registry owns registration, subscription, isolation, and replay.
- Cases: modality is capability, never preference — `ISSUE_ADMITTED` is the one `Modality(veto=None)` row because pre-drain rejection is the point's declared contract (a veto subscriber's `Error` rides the issue rail as its `BoundaryFault` refusal, and the payload projection means a veto rejects, never rewrites, the staged work); `FRONT_DRAINED` is the one replay row and carries its window depth on that arm alone, sized to the deepest CPM front chain a sheet-set issue drains so a late subscriber reads the whole last drain; every other row is `Modality(observe=None)` — a fenced tap whose fault lands on the receipt stream while the emitter's value passes untouched.
- Law: each payload is a closed `Struct(frozen=True, gc=False)` of native scalars projected from the evidence in hand — `IssueAdmitted` from the staged node set, `IssuePlanned` from the cleared plan, `IssueRefused` from the terminal `BoundaryFault.tag`, `FRONT_DRAINED` carrying the drive's own `StageMark` and no payload of this plane's at all, `ReceiptEmitted` from the `ArtifactReceipt` case scalars at the `contribute` fold, `TransmittalIssued` from the settled `TransmittalEvidence` at the transmittal close — and every payload carries `scope`, the `ISSUE_BAGGAGE` correlation id, so a subscriber slices facts per issue with no join against a second stream. Fields a payload cannot project from landed evidence stay off the point.
- Law: `FRONT_DRAINED` is the plane's one BORROWED point: `execution/lanes#LANE` `LanePolicy.driven` owns the front drive and fires this id as its between-wave gate under the lane's own composition scope, so the payload is that drive's `StageMark` and the per-front outcome counts stay where the lane already publishes them — its `drained` receipt line — rather than riding a second artifacts payload that re-derived the same roster. A producer-side struct here would make this plane a second reader of both the mark and the drain columns.
- Law: `TransmittalIssued` is the plane's one ANNOUNCED fact and its width is the announcement's, not the receipt's — an issue crosses to a downstream system as attributes and a routed payload, so every scalar an ingesting system routes on rides the point and the delivery projection at `delivery/notice#NOTICE` invents none. Its identity fields carry the runtime `ContentKey` renders rather than the values, because a tap projects each payload onto a receipt through `structs.asdict` and a nested value object reaches that fact stream as an object no reader renders.
- Entry: `scoped(context)` reads the `ISSUE_BAGGAGE` baggage entry off an explicit context — the empty string when no issue scope is live, so a receipt contributed outside an issue drain still fires lawfully. `Production.registered` claims the WHOLE table through one `Hooks.register` roster call — the registry's gated transition swaps only past its last admitted row and reports every collision together, so a refusal leaves this plane's custody untouched instead of half-mounted — then deposits one `ArtifactInstall` naming the landed ids through `Hooks.installed` under `OWNER` — the deposit passes its receipt through, so the install IS the rail's terminal and the cached one-shot holds it; its locked `_wired` map returns that prior rail on same-scope re-entry — the scope-keyed one-shot mirrors the runtime registry's own `ScopeKey` partition and stays singular under free-threaded concurrent first use; `Production.fired` composes the cell before `Hooks.fire`, so the first fire from any seam self-registers, a `VETO` rail returns to the emitter, and an `OBSERVE` rail is fire-and-forget by modality contract; `Production.subscribed` composes the same cell before `Hooks.subscribe` and forwards its grain — one id or the whole `ARTIFACT_POINTS` roster — answering the `Attachment` detacher a composition root closes, since a count retires nothing. Subscribers stay app-root — `Production.subscribed(point, TapRow(receipts=OWNER))` or a domain tap — and this page registers points alone.
- Packages: `msgspec` (`Struct` payload rows and `to_builtins(payload, str_keys=True)` the direct telemetry projection), `expression` (`Block` the row table, `Option`/`Map` the one-shot cell), `opentelemetry-api` (`baggage.get_baggage` the scope read), runtime (`HookPoint`/`Hooks`/`Modality`/`Tap`/`TapRow`/`Veto`/`Attachment`, `Hooks.register`'s roster arm and `Hooks.installed` the producer-install ledger, `RuntimeRail`, `ScopeKey`/`DEFAULT_SCOPE` the composition axis, `StageMark` the shared long-fold mark the borrowed front point carries).
- Growth: a new production fact is one `ArtifactHook` member, one payload `Struct`, and one `ARTIFACT_POINTS` row — the fire seam lands at the owner that holds the evidence, registration, isolation, and taps follow with zero edits here, and the install receipt widens by derivation because it names the landed ids rather than a hand-kept list; a new admission fact this plane can prove at composition is one `ArtifactInstall` field of native scalars; a new payload field is one `Struct` field every tap projects through `msgspec.to_builtins(payload, str_keys=True)`; a new consumer is one `Production.subscribed` call at the app root with zero artifacts edit; a wider replay window is the depth on the `FRONT_DRAINED` row's own replay arm; a new artifacts module is one `ArtifactsLeg` member; a new long fold is one `ArtifactStage` member; a second composition is one `ScopeKey` value threaded through `scope` with every bare call untouched.
- Boundary: this page imports no artifacts sibling — payloads carry native scalars and the leg roster carries none, so the floor stays acyclic under every raiser and under `core/receipt` and `core/issue` composing it downward; a `FaultRow` table seats at the page that raises, never here; the fire seams, the veto consequence, and the baggage bind are the emitting owners'; exporter, provider, and transport wiring stay the runtime telemetry owner's; and a hook payload never re-narrates a receipt — the settled receipt and its evidence remain the one truth the fired fact projects. Artifacts taps are synchronous by charter — payloads are small scalar facts the built-in taps project onto receipts and metrics in-line; an async tap rides the runtime `fire_async` surface an app wires itself, and no fire opens a span, since a fire runs under whatever span is active.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from datetime import datetime
from enum import StrEnum
from threading import RLock
from typing import ClassVar, Final, overload

from expression import Option
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import baggage
from opentelemetry import context as otel_context

from rasm.runtime.faults import RuntimeRail
from rasm.runtime.hooks import Attachment, HookPoint, Hooks, Modality, StageMark, Tap, TapRow, Veto
from rasm.runtime.receipts import DEFAULT_SCOPE, ScopeKey

# --- [TYPES] ----------------------------------------------------------------------------


class ArtifactsLeg(StrEnum):
    COMPOSE = "artifacts.composition.compose"
    IMPOSITION = "artifacts.composition.imposition"
    SHEET = "artifacts.composition.sheet"
    BENCH = "artifacts.core.bench"
    HOOKS = "artifacts.core.hooks"
    ISSUE = "artifacts.core.issue"
    PLAN = "artifacts.core.plan"
    RECEIPT = "artifacts.core.receipt"
    GATE = "artifacts.delivery.gate"
    NOTICE = "artifacts.delivery.notice"
    REGISTER = "artifacts.delivery.register"
    TRANSMITTAL = "artifacts.delivery.transmittal"
    EGRESS = "artifacts.document.egress"
    EMIT = "artifacts.document.emit"
    LENS = "artifacts.document.lens"
    MODEL = "artifacts.document.model"
    REPORT = "artifacts.document.report"
    TAGGED = "artifacts.document.tagged"
    ANNOTATE = "artifacts.drawing.annotate"
    DETAIL = "artifacts.drawing.detail"
    DIMENSION = "artifacts.drawing.dimension"
    REGIME = "artifacts.drawing.regime"
    SCHEDULE = "artifacts.drawing.schedule"
    STANDARD = "artifacts.drawing.standard"
    SYMBOL = "artifacts.drawing.symbol"
    CONFORMANCE = "artifacts.exchange.conformance"
    CREDENTIAL = "artifacts.exchange.credential"
    DETECT = "artifacts.exchange.detect"
    METADATA = "artifacts.exchange.metadata"
    DXF = "artifacts.export.dxf"
    INDESIGN = "artifacts.export.indesign"
    LAYERED = "artifacts.export.layered"
    COLOR_DERIVE = "artifacts.graphic.color.derive"
    MANAGED = "artifacts.graphic.color.managed"
    LAYER = "artifacts.graphic.layer"
    DECODE = "artifacts.graphic.marks.decode"
    ENCODE = "artifacts.graphic.marks.encode"
    MARK = "artifacts.graphic.marks.mark"
    IO = "artifacts.graphic.raster.io"
    MEASURE = "artifacts.graphic.raster.measure"
    PROCESS = "artifacts.graphic.raster.process"
    STYLE = "artifacts.graphic.style"
    TEXTURE_DERIVE = "artifacts.graphic.texture.derive"
    IBL = "artifacts.graphic.texture.ibl"
    INGEST = "artifacts.graphic.texture.ingest"
    PLANE = "artifacts.graphic.texture.plane"
    SET = "artifacts.graphic.texture.set"
    PATH = "artifacts.graphic.vector.path"
    PATTERN = "artifacts.graphic.vector.pattern"
    REGION = "artifacts.graphic.vector.region"
    ANALYSIS = "artifacts.media.analysis"
    AUDIO = "artifacts.media.audio"
    CONTAINER = "artifacts.media.container"
    FILTERGRAPH = "artifacts.media.filtergraph"
    SUBTITLE = "artifacts.media.subtitle"
    SYNTHESIS = "artifacts.media.synthesis"
    TIMELINE = "artifacts.media.timeline"
    ARCHIVE = "artifacts.package.archive"
    BUNDLE = "artifacts.package.bundle"
    CODEC = "artifacts.package.codec"
    DELTA = "artifacts.package.delta"
    SCENE_EXPORT = "artifacts.scene.export"
    RENDER = "artifacts.scene.render"
    RENDER_WORKER = "artifacts.scene.render_worker"
    SCENE_SPEC = "artifacts.scene.spec"
    STAGE = "artifacts.scene.stage"
    CLASSIFY = "artifacts.specification.classify"
    SECTION = "artifacts.specification.section"
    FONT = "artifacts.typography.font"
    TYPOGRAPHY_LAYOUT = "artifacts.typography.layout"
    MATH = "artifacts.typography.math"
    SHAPE = "artifacts.typography.shape"
    CHART_EXPORT = "artifacts.visualization.chart.export"
    CHART_SPEC = "artifacts.visualization.chart.spec"
    DASHBOARD = "artifacts.visualization.dashboard"
    DRAW = "artifacts.visualization.diagram.draw"
    GLYPHSET = "artifacts.visualization.diagram.glyphset"
    DIAGRAM_LAYOUT = "artifacts.visualization.diagram.layout"
    SCHEMATIC = "artifacts.visualization.diagram.schematic"
    SOLAR = "artifacts.visualization.diagram.solar"
    TABLE = "artifacts.visualization.table"


class ArtifactStage(StrEnum):
    DRAIN = "drain"


class ArtifactHook(StrEnum):
    ISSUE_ADMITTED = "rasm.artifacts.issue.admitted"
    ISSUE_PLANNED = "rasm.artifacts.issue.planned"
    ISSUE_REFUSED = "rasm.artifacts.issue.refused"
    FRONT_DRAINED = "rasm.artifacts.issue.drained"
    RECEIPT_EMITTED = "rasm.artifacts.receipt.emitted"
    TRANSMITTAL_ISSUED = "rasm.artifacts.delivery.issued"


# --- [CONSTANTS] ------------------------------------------------------------------------

ISSUE_BAGGAGE: Final[str] = "rasm.artifacts.issue"

OWNER: Final[str] = "artifacts.production"

# --- [MODELS] ---------------------------------------------------------------------------


class ArtifactInstall(Struct, frozen=True, gc=False):
    points: tuple[str, ...]


class IssueAdmitted(Struct, frozen=True, gc=False):
    modality: str
    works: int
    targets: int
    scope: str


class IssuePlanned(Struct, frozen=True, gc=False):
    works: int
    fronts: int
    targets: int
    scope: str


class IssueRefused(Struct, frozen=True, gc=False):
    cause: str
    scope: str


class ReceiptEmitted(Struct, frozen=True, gc=False):
    kind: str
    key: str
    scope: str


class TransmittalIssued(Struct, frozen=True, gc=False):
    key: str
    register: str
    container: str
    transmittal_id: str
    issuing_party: str
    purpose: str
    revision: str
    revision_ordinal: int
    confidentiality: str
    issued_at: str
    occurred: datetime
    sheets: int
    lineage: int
    suitability: str
    pades_level: str
    validation_state: str
    record_state: str
    gate_grade: str
    scope: str


# --- [TABLES] ---------------------------------------------------------------------------

TRANSMITTAL_POINT: Final[HookPoint[TransmittalIssued]] = HookPoint(
    id=ArtifactHook.TRANSMITTAL_ISSUED,
    payload=TransmittalIssued,
    modality=Modality(observe=None),
)

ARTIFACT_POINTS: Final[Block[HookPoint[Struct]]] = Block.of_seq([
    HookPoint(id=ArtifactHook.ISSUE_ADMITTED, payload=IssueAdmitted, modality=Modality(veto=None)),
    HookPoint(id=ArtifactHook.ISSUE_PLANNED, payload=IssuePlanned, modality=Modality(observe=None)),
    HookPoint(id=ArtifactHook.ISSUE_REFUSED, payload=IssueRefused, modality=Modality(observe=None)),
    HookPoint(id=ArtifactHook.FRONT_DRAINED, payload=StageMark, modality=Modality(replay=8)),
    HookPoint(id=ArtifactHook.RECEIPT_EMITTED, payload=ReceiptEmitted, modality=Modality(observe=None)),
    TRANSMITTAL_POINT,
])

# --- [OPERATIONS] -----------------------------------------------------------------------


def scoped(context: otel_context.Context, /) -> str:
    entry = baggage.get_baggage(ISSUE_BAGGAGE, context)
    return entry if isinstance(entry, str) else ""


# --- [SERVICES] -------------------------------------------------------------------------


class Production:
    _lock: ClassVar[RLock] = RLock()
    _wired: ClassVar[Map[ScopeKey, RuntimeRail[ArtifactInstall]]] = Map.empty()

    @classmethod
    def registered(cls, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[ArtifactInstall]:
        with cls._lock:
            match cls._wired.try_find(scope):
                case Option(tag="some", some=prior):
                    return prior
                case _:
                    rail = Hooks.register(ARTIFACT_POINTS, scope=scope).map(
                        lambda points: Hooks.installed(OWNER, ArtifactInstall(points=tuple(point.id for point in points)), scope=scope)
                    )
                    cls._wired = cls._wired.add(scope, rail)
                    return rail

    @classmethod
    def fired[P: Struct](cls, point: ArtifactHook, payload: P, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[P]:
        return cls.registered(scope=scope).bind(lambda _install: Hooks.fire(point, payload, scope=scope))

    @overload
    @classmethod
    def subscribed[P: Struct](cls, point: ArtifactHook, tap: Tap[P] | Veto[P] | TapRow, *, scope: ScopeKey = ...) -> RuntimeRail[Attachment]: ...
    @overload
    @classmethod
    def subscribed[P: Struct](
        cls, point: Block[HookPoint[Struct]], tap: Tap[P] | Veto[P] | TapRow, *, scope: ScopeKey = ...
    ) -> RuntimeRail[Block[Attachment]]: ...

    @classmethod
    def subscribed[P: Struct](
        cls, point: ArtifactHook | Block[HookPoint[Struct]], tap: Tap[P] | Veto[P] | TapRow, *, scope: ScopeKey = DEFAULT_SCOPE
    ) -> RuntimeRail[Attachment] | RuntimeRail[Block[Attachment]]:
        return cls.registered(scope=scope).bind(lambda _install: Hooks.subscribe(point, tap, scope=scope))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = (
    "ARTIFACT_POINTS",
    "ISSUE_BAGGAGE",
    "OWNER",
    "ArtifactHook",
    "ArtifactStage",
    "ArtifactsLeg",
    "ArtifactInstall",
    "IssueAdmitted",
    "IssuePlanned",
    "IssueRefused",
    "Production",
    "ReceiptEmitted",
    "TransmittalIssued",
    "TRANSMITTAL_POINT",
    "scoped",
)
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
