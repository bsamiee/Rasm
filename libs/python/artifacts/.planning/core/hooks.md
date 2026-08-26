# [PY_ARTIFACTS_HOOKS]

`ArtifactHook` owns issue lifecycle and transmittal chronology on the runtime `Hooks` registry. Producer measurements remain local observations over canonical domain values.

`Production` is the composed entry over the runtime registry: `registered` lands `ARTIFACT_POINTS` through `Hooks.register` under a scope-keyed one-shot cell and deposits its `ArtifactInstall` record on that registry's install ledger, so the support-bundle capsule reads this plane's admission where an absent row is its stated diagnosis. `fired` rides the cell so every boundary self-registers before `Hooks.fire` and a hook-free app's veto gate passes clean, and `subscribed` is the app-root attach answering the `Attachment` detacher the composition root brackets.

`ScopeKey` partitions points, subscribers, and replay rings. `ArtifactKind` supplies the shared metric dimension and delivery-gate policy key.

## [01]-[INDEX]

- [02]-[POINTS]: artifact vocabulary, issue and transmittal points, scope attribution, and registry composition.

## [02]-[POINTS]

- Owner: `ARTIFACT_POINTS` binds each lifecycle or chronology id to one closed payload and modality; runtime `Hooks` owns registration, subscription, isolation, and replay.
- Cases: `ISSUE_ADMITTED` carries veto authority, `FRONT_DRAINED` carries the runtime lane's replayable `StageMark`, and the remaining lifecycle and chronology points observe without changing their producer.
- Law: issue payloads project the staged or settled issue state already in hand. `TransmittalIssued` alone announces a domain occurrence to downstream transport.
- Entry: `Production.registered` installs the point table once per scope. `Production.fired` and `Production.subscribed` compose registration with the runtime hook operations.
- Packages: `msgspec` owns payload rows; `expression` owns the scope cell; runtime hooks own point behavior; OpenTelemetry baggage supplies issue correlation.
- Growth: a new domain occurrence adds one typed payload, one point row, and one emitting-owner projection.
- Boundary: producer values, byte metrics, durable domain facts, and transport delivery stay at their owning boundaries.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from datetime import datetime
from enum import StrEnum
from threading import RLock
from typing import ClassVar, Final, Literal, overload

from expression import Option
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import baggage
from opentelemetry import context as otel_context

from rasm.runtime.faults import RuntimeResult
from rasm.runtime.hooks import Attachment, HookPoint, Hooks, Modality, StageMark, Tap, TapRow, Veto
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey

# --- [TYPES] ----------------------------------------------------------------------------

type ArtifactKind = Literal[
    "pdf",
    "office",
    "report",
    "document",
    "chart",
    "dashboard",
    "scene",
    "table",
    "preview",
    "color",
    "texture",
    "bundle",
    "introspection",
    "egress",
    "verdict",
    "credential",
    "media",
    "diagram",
    "metadata",
    "drawing",
    "schedule",
    "spec",
    "cad",
    "register",
    "transmittal",
]


class ArtifactsLeg(StrEnum):
    COMPOSE = "artifacts.composition.compose"
    IMPOSITION = "artifacts.composition.imposition"
    SHEET = "artifacts.composition.sheet"
    BENCH = "artifacts.core.bench"
    HOOKS = "artifacts.core.hooks"
    ISSUE = "artifacts.core.issue"
    PLAN = "artifacts.core.plan"
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
    TRANSMITTAL_ISSUED = "rasm.artifacts.delivery.issued"


# --- [CONSTANTS] ------------------------------------------------------------------------

ISSUE_BAGGAGE: Final[str] = "rasm.artifacts.issue"

OWNER: Final[str] = "artifacts.production"

DOMAIN: Final[str] = "artifact"
BYTE_VOLUME: Final[str] = "rasm.artifact.byte_volume"

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
    TRANSMITTAL_POINT,
])

# --- [OPERATIONS] -----------------------------------------------------------------------


def scoped(context: otel_context.Context, /) -> str:
    entry = baggage.get_baggage(ISSUE_BAGGAGE, context)
    return entry if isinstance(entry, str) else ""


# --- [SERVICES] -------------------------------------------------------------------------


class Production:
    _lock: ClassVar[RLock] = RLock()
    _wired: ClassVar[Map[ScopeKey, RuntimeResult[ArtifactInstall]]] = Map.empty()

    @classmethod
    def registered(cls, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeResult[ArtifactInstall]:
        with cls._lock:
            match cls._wired.try_find(scope):
                case Option(tag="some", some=prior):
                    return prior
                case _:
                    held = Hooks.register(ARTIFACT_POINTS, scope=scope).map(
                        lambda points: Hooks.installed(OWNER, ArtifactInstall(points=tuple(point.id for point in points)), scope=scope)
                    )
                    cls._wired = cls._wired.add(scope, held)
                    return held

    @classmethod
    def fired[P: Struct](cls, point: ArtifactHook, payload: P, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeResult[P]:
        return cls.registered(scope=scope).bind(lambda _install: Hooks.fire(point, payload, scope=scope))

    @overload
    @classmethod
    def subscribed[P: Struct](cls, point: ArtifactHook, tap: Tap[P] | Veto[P] | TapRow, *, scope: ScopeKey = ...) -> RuntimeResult[Attachment]: ...
    @overload
    @classmethod
    def subscribed[P: Struct](
        cls, point: Block[HookPoint[Struct]], tap: Tap[P] | Veto[P] | TapRow, *, scope: ScopeKey = ...
    ) -> RuntimeResult[Block[Attachment]]: ...

    @classmethod
    def subscribed[P: Struct](
        cls, point: ArtifactHook | Block[HookPoint[Struct]], tap: Tap[P] | Veto[P] | TapRow, *, scope: ScopeKey = DEFAULT_SCOPE
    ) -> RuntimeResult[Attachment] | RuntimeResult[Block[Attachment]]:
        return cls.registered(scope=scope).bind(lambda _install: Hooks.subscribe(point, tap, scope=scope))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = (
    "ARTIFACT_POINTS",
    "BYTE_VOLUME",
    "DOMAIN",
    "ISSUE_BAGGAGE",
    "OWNER",
    "TRANSMITTAL_POINT",
    "ArtifactHook",
    "ArtifactInstall",
    "ArtifactKind",
    "ArtifactStage",
    "ArtifactsLeg",
    "IssueAdmitted",
    "IssuePlanned",
    "IssueRefused",
    "Production",
    "TransmittalIssued",
    "scoped",
)
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
