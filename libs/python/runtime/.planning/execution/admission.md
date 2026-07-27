# [PY_RUNTIME_ADMISSION]

Caller-owned context and settings admission: one immutable `RuntimeContext` carries the supplied `ConsumptionProfile` axis row, correlation, deadline, classification, and inbound `CausalFrame` a caller supplies; one `SettingsAdmission` owns the local settings source order; `SecretBoundary.resolve` is the one credential reader the outbound `transport/roots#RESOURCE` legs consume — output-parameterized, profile-gated, and lazy on the outbound leg, never an eager unattended probe.

`CausalFrame`, `Hlc`, and `Tenant` arrive from the `clock/clock#CLOCK` owner — admission consumes the stamp that owner mints and re-mints nothing. Each `SECRET_LADDER` tier folds through the `reliability/resilience#RESILIENCE` `guarded` envelope under the `RetryClass.SECRET` row, so a transiently-locked keystore or unreachable Secret Manager retries inside one derivation span rather than failing the resolve. Feature gating folds from the bound provider rows and killswitch state rides the context, never boolean knobs the caller re-derives. This package never discovers the host, starts services, owns lifecycle, derives product roots, reads the environment after admission, or caches a global mutable context, and a resolved secret crosses as `SecretStr`, never a bare `str` a receipt or log egress serializes.

## [01]-[INDEX]

- [02]-[CONTEXT]: six-axis `ConsumptionProfile` row with its refusal, adopted W3C correlation, clock-consumed causal frame.
- [03]-[BACKEND_CONTRACT]: local contract composition, the deterministic branch merge, generation proof, and realized-evidence admission.
- [04]-[SETTINGS]: the `pydantic-settings` admission, the `SECRET_LADDER` tier table, and the output-parameterized `SecretBoundary.resolve`.

## [02]-[CONTEXT]

- Owner: `RuntimeContext` is the one caller-supplied context discriminating deployment shape, correlation, deadline, and classification, carrying the inbound `causal` frame as `Option[CausalFrame]` — `Nothing` locally minted, `Some(frame)` the host stamp — and the inbound header map as `carrier`. `ConsumptionProfile` carries the six-axis row `tenancy`, `topology`, `host`, `lifecycle`, `isolation`, `providers`; `RuntimeProfile` names the presets this branch supplies over that roster and `PROFILE_ROW` expands each to its axis tuple, so a preset is a name for a row and never a discriminant a fold switches on.
- Cases: `tenancy` = none | single | multi; `topology` = in-host | sidecar | companion | service | edge | cli; `lifecycle` = caller-owned | package-owned; `isolation` = in-proc | thread | process | wasm | remote; `host` and `providers` carry `HostRow` and `ProviderRow` descriptors this branch supplies through `HOST_ROWS` and `PROVIDER_ROWS`; `AxisRefused` carries the refusal evidence.
- Entry: `ConsumptionProfile.admit` is the one axis gate — it returns `Result[ConsumptionProfile, AxisRefused]`, every `PROFILE_ROW` preset proves through it at declaration, and a composition root folds a hand-supplied row through it before `RuntimeContext.admit` ever sees one. `Deadline.seconds` is the one `float` the `execution/lanes#LANE` `LanePolicy.deadline` reads — never a re-derived `total_seconds()` at the lane seam. `Correlation.seed` is the one inbound-context owner: it adopts the extracted W3C parent whole under the widths `TraceId` and `SpanId` fix, and `attribute` folds the carried frame through `CausalFrame.attributes("packed")` rather than re-spelling the `(rasm.tenant, rasm.hlc)` columns, so the result is admissible to `Span.set_attributes` directly.
- Correlation: adoption is the conformance — an inbound `traceparent` continues its trace id and parents its span id, and a fresh 16-byte root mints only where extraction yields no valid parent, so the Python leg never fractures a distributed trace. `Hlc` and the W3C context ride one carrier as two disjoint reads: the packed cell projects as the `rasm.hlc` span attribute, and the identity slots take propagator output alone.
- Auto: `ProfilePolicy.of` folds every behavior column out of axis values — `lifecycle` decides eager import, `lifecycle` beside the host descriptor's `scratch` column decides scratch writability, a bound telemetry provider decides OTel emission, and the host descriptor's `lanes` column overrides `TOPOLOGY_LANES` for capacity — so no column keys on a preset name and a caller never re-derives a flag.
- Growth: a new context field is one `RuntimeContext` column; a new host integration is one `HOST_ROWS` descriptor and a new bound port one `PROVIDER_ROWS` descriptor; a new feature is one `Feature` case supplied by a provider row; a new killswitch is one `Killswitch` case with one `KILLSWITCH_FEATURE` disabling edge — never a parallel boolean knob; a new attribute dimension is one entry in the `attribute` projection; a new propagated wire format is one row at the telemetry install's composite, reaching `seed` with no edit here.
- Boundary: no environment probing, host discovery, service-root construction, or global mutable context lives here — deployment shape arrives as one supplied row and this package infers none of it; axis values stay data, so a compile-time assumption, an ambient global, an environment flag, and a fold branching on which product hosts the package are the four deleted forms; `ConsumptionProfile.admit` refuses an unservable axis value with `AxisRefused` naming the axis, so silent degradation and a narrowed public surface never happen; in-host topology carrying no host descriptor refuses on the `host` axis because a consuming application supplies its own row; killswitches ride `RuntimeContext.killswitches` as caller-supplied operational state, never a profile column, so revoking a feature never re-cuts deployment shape; `CausalFrame`/`Hlc`/`Tenant` stay the `clock/clock#CLOCK` owner's records; propagator registration stays the `observability/telemetry#TELEMETRY` install's, this owner reading the global it publishes; each branch spells the roster in its own types, so a peer branch's descriptor rows are never mirrored here row-for-row.

Each `isolation` value names the worker crossing that answers it; `Kernel` selects the `WorkerKind` inside a value and never widens the axis, and an unbound feature refuses on the `isolation` axis rather than silently dropping to a weaker crossing:

| [INDEX] | [ISOLATION] | [CROSSING]                            | [ADMISSION]                  |
| :-----: | :---------- | :------------------------------------ | :--------------------------- |
|  [01]   | `in-proc`   | loop-resident `KernelTrait.INLINE`    | always served                |
|  [02]   | `thread`    | `WorkerKind.THREAD`, `INTERPRETER`    | always served                |
|  [03]   | `process`   | `WorkerKind.PROCESS`, `GPU`, `DAEMON` | `Feature.LOCAL_SPAWN`        |
|  [04]   | `wasm`      | `WorkerKind.WASM` guest sandbox       | `Feature.WASM_GUEST`         |
|  [05]   | `remote`    | `WorkerKind.REMOTE` fleet arm         | `Feature.OUTBOUND_TRANSPORT` |

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Mapping
from datetime import timedelta
from enum import StrEnum
from secrets import token_bytes
from typing import Annotated, Final, Self

from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Map
from msgspec import Meta, Struct, field
from opentelemetry import propagate, trace

from rasm.runtime.clock import CausalFrame

# --- [TYPES] ----------------------------------------------------------------------------


class ProfileAxis(StrEnum):
    TENANCY = "tenancy"
    TOPOLOGY = "topology"
    HOST = "host"
    LIFECYCLE = "lifecycle"
    ISOLATION = "isolation"
    PROVIDERS = "providers"


class Tenancy(StrEnum):
    NONE = "none"
    SINGLE = "single"
    MULTI = "multi"


class Topology(StrEnum):
    IN_HOST = "in-host"
    SIDECAR = "sidecar"
    COMPANION = "companion"
    SERVICE = "service"
    EDGE = "edge"
    CLI = "cli"


class Lifecycle(StrEnum):
    CALLER_OWNED = "caller-owned"
    PACKAGE_OWNED = "package-owned"


class Isolation(StrEnum):
    IN_PROC = "in-proc"
    THREAD = "thread"
    PROCESS = "process"
    WASM = "wasm"
    REMOTE = "remote"


# Preset NAMES over the axis roster, never a discriminant: PROFILE_ROW expands each to its tuple and
# every behavior column folds from those axis values, so switching on a member here is the fused-key defect.
class RuntimeProfile(StrEnum):
    TOOL = "tool"
    SIDECAR = "sidecar"
    PACKAGE = "package"
    TEST = "test"


class Feature(StrEnum):
    SECRET_MANAGER = "secret_manager"
    KEYSTORE_PROBE = "keystore_probe"
    OUTBOUND_TRANSPORT = "outbound_transport"
    TELEMETRY_EXPORT = "telemetry_export"
    LOCAL_SPAWN = "local_spawn"
    WASM_GUEST = "wasm_guest"


class Killswitch(StrEnum):
    DISABLE_OUTBOUND = "disable_outbound"
    DISABLE_SECRET_MANAGER = "disable_secret_manager"


# Each W3C identity width carries its own domain, so the 128-bit HLC cell cannot land in a span-id slot and a
# truncated or padded id fails at construction rather than at a collector rejecting the export.
type TraceId = Annotated[bytes, Meta(min_length=16, max_length=16)]
type SpanId = Annotated[bytes, Meta(min_length=8, max_length=8)]

# --- [CONSTANTS] ------------------------------------------------------------------------

_TRACE_BYTES: Final[int] = 16  # the W3C trace id every hop of one distributed trace continues
_SPAN_BYTES: Final[int] = 8  # the W3C span id naming exactly one hop's parent

# each killswitch names the feature it revokes, so a tripped switch and an admitted feature meet in one `admits` fold rather than two
# predicates a caller must remember to AND.
KILLSWITCH_FEATURE: Final[Map[Killswitch, Feature]] = Map.of_seq([
    (Killswitch.DISABLE_OUTBOUND, Feature.OUTBOUND_TRANSPORT),
    (Killswitch.DISABLE_SECRET_MANAGER, Feature.SECRET_MANAGER),
])

# each isolation value names the feature a bound provider must supply before the crossing answering it can
# run; the two loop-resident values need none, so `Nothing` is the served-unconditionally answer.
ISOLATION_FEATURE: Final[Map[Isolation, Option[Feature]]] = Map.of_seq([
    (Isolation.IN_PROC, Nothing),
    (Isolation.THREAD, Nothing),
    (Isolation.PROCESS, Some(Feature.LOCAL_SPAWN)),
    (Isolation.WASM, Some(Feature.WASM_GUEST)),
    (Isolation.REMOTE, Some(Feature.OUTBOUND_TRANSPORT)),
])

# lane capacity an UNHOSTED profile inherits from its deployment shape; a host descriptor's `lanes`
# column overrides it, so an embedding application sizes the pool its own process can carry.
TOPOLOGY_LANES: Final[Map[Topology, int]] = Map.of_seq([
    (Topology.IN_HOST, 4),
    (Topology.SIDECAR, 16),
    (Topology.COMPANION, 8),
    (Topology.SERVICE, 16),
    (Topology.EDGE, 8),
    (Topology.CLI, 8),
])

# --- [MODELS] ---------------------------------------------------------------------------


class FeatureGate(Struct, frozen=True, gc=False):
    admitted: frozenset[Feature]
    tripped: frozenset[Killswitch]

    def admits(self, feature: Feature) -> bool:
        # effective availability folds BOTH axes — admitted AND not revoked — so a killswitch is never dead policy a row ignores.
        return feature in self.admitted and not any(KILLSWITCH_FEATURE[k] is feature for k in self.tripped)

    def is_tripped(self, killswitch: Killswitch) -> bool:
        return killswitch in self.tripped


class AxisRefused(Struct, frozen=True, gc=False):
    axis: ProfileAxis
    value: str
    reason: str


class HostRow(Struct, frozen=True, gc=False):
    key: str
    lanes: int
    scratch: bool
    document: bool


class ProviderRow(Struct, frozen=True, gc=False):
    key: str
    supplies: Feature
    reach: Isolation


# Rows this branch supplies for the two OPEN axes. An application embedding the estate inside its own
# process mints its own descriptor against the same shape; nothing here is a set a fold may close over.
HOST_ROWS: Final[Map[str, HostRow]] = Map.of_seq([
    ("embedded", HostRow(key="embedded", lanes=4, scratch=False, document=False)),
    ("test-harness", HostRow(key="test-harness", lanes=2, scratch=True, document=False)),
])

PROVIDER_ROWS: Final[Map[str, ProviderRow]] = Map.of_seq([
    ("secret-manager", ProviderRow(key="secret-manager", supplies=Feature.SECRET_MANAGER, reach=Isolation.REMOTE)),
    ("keystore", ProviderRow(key="keystore", supplies=Feature.KEYSTORE_PROBE, reach=Isolation.IN_PROC)),
    ("egress", ProviderRow(key="egress", supplies=Feature.OUTBOUND_TRANSPORT, reach=Isolation.REMOTE)),
    ("otlp-collector", ProviderRow(key="otlp-collector", supplies=Feature.TELEMETRY_EXPORT, reach=Isolation.REMOTE)),
    ("process-pool", ProviderRow(key="process-pool", supplies=Feature.LOCAL_SPAWN, reach=Isolation.PROCESS)),
    ("wasm-sandbox", ProviderRow(key="wasm-sandbox", supplies=Feature.WASM_GUEST, reach=Isolation.WASM)),
])


class ConsumptionProfile(Struct, frozen=True):
    tenancy: Tenancy
    topology: Topology
    host: Option[HostRow]
    lifecycle: Lifecycle
    isolation: Isolation
    providers: tuple[ProviderRow, ...]

    @property
    def grants(self) -> frozenset[Feature]:
        return frozenset(row.supplies for row in self.providers)

    @property
    def host_key(self) -> str:
        return self.host.map(lambda row: row.key).default_value("none")

    @classmethod
    def admit(cls, row: "ConsumptionProfile") -> Result["ConsumptionProfile", AxisRefused]:
        # one axis gate for both open questions: in-host names a host the consumer supplies, and an
        # isolation value whose crossing feature no provider carries refuses rather than dropping a tier.
        if row.topology is Topology.IN_HOST and row.host is Nothing:
            return Error(AxisRefused(axis=ProfileAxis.HOST, value="none", reason="in-host topology carries no host descriptor row"))
        match ISOLATION_FEATURE[row.isolation]:
            case Option(tag="some", some=needed) if needed not in row.grants:
                return Error(AxisRefused(axis=ProfileAxis.ISOLATION, value=row.isolation, reason=needed))
            case _:
                return Ok(row)

    def canonical(self) -> tuple[tuple[str, str], ...]:
        # six rows in roster order under an ordinal provider-key sort: the canonical-json preimage the
        # corpus parity reads, so a provider tuple reordered at the composition root re-serializes identically.
        return (
            (ProfileAxis.TENANCY, self.tenancy),
            (ProfileAxis.TOPOLOGY, self.topology),
            (ProfileAxis.HOST, self.host_key),
            (ProfileAxis.LIFECYCLE, self.lifecycle),
            (ProfileAxis.ISOLATION, self.isolation),
            (ProfileAxis.PROVIDERS, ",".join(sorted(row.key for row in self.providers))),
        )


PROFILE_ROW: Final[Map[RuntimeProfile, ConsumptionProfile]] = Map.of_seq([
    (
        RuntimeProfile.TOOL,
        ConsumptionProfile(
            tenancy=Tenancy.SINGLE,
            topology=Topology.CLI,
            host=Nothing,
            lifecycle=Lifecycle.PACKAGE_OWNED,
            isolation=Isolation.PROCESS,
            providers=(PROVIDER_ROWS["secret-manager"], PROVIDER_ROWS["keystore"], PROVIDER_ROWS["egress"], PROVIDER_ROWS["otlp-collector"], PROVIDER_ROWS["process-pool"]),
        ),
    ),
    (
        RuntimeProfile.SIDECAR,
        ConsumptionProfile(
            tenancy=Tenancy.MULTI,
            topology=Topology.SIDECAR,
            host=Nothing,
            lifecycle=Lifecycle.PACKAGE_OWNED,
            isolation=Isolation.PROCESS,
            providers=(PROVIDER_ROWS["secret-manager"], PROVIDER_ROWS["keystore"], PROVIDER_ROWS["egress"], PROVIDER_ROWS["otlp-collector"], PROVIDER_ROWS["process-pool"]),
        ),
    ),
    (
        RuntimeProfile.PACKAGE,
        ConsumptionProfile(
            tenancy=Tenancy.NONE,
            topology=Topology.IN_HOST,
            host=Some(HOST_ROWS["embedded"]),
            lifecycle=Lifecycle.CALLER_OWNED,
            isolation=Isolation.THREAD,
            providers=(PROVIDER_ROWS["egress"],),
        ),
    ),
    (
        RuntimeProfile.TEST,
        ConsumptionProfile(
            tenancy=Tenancy.NONE,
            topology=Topology.IN_HOST,
            host=Some(HOST_ROWS["test-harness"]),
            lifecycle=Lifecycle.CALLER_OWNED,
            isolation=Isolation.IN_PROC,
            providers=(),
        ),
    ),
])


class ProfilePolicy(Struct, frozen=True):
    eager_import: bool
    scratch_writable: bool
    emit_otel: bool
    lane_capacity: int
    gate: FeatureGate

    @classmethod
    def of(cls, axes: ConsumptionProfile, tripped: frozenset[Killswitch]) -> "ProfilePolicy":
        # every column folds from an axis value: a package-owned lifetime pays import cost at its own
        # boot, a caller-owned one never does, and the host descriptor overrides the topology defaults.
        return cls(
            eager_import=axes.lifecycle is Lifecycle.PACKAGE_OWNED,
            scratch_writable=axes.lifecycle is Lifecycle.PACKAGE_OWNED or axes.host.map(lambda row: row.scratch).default_value(False),
            emit_otel=Feature.TELEMETRY_EXPORT in axes.grants,
            lane_capacity=axes.host.map(lambda row: row.lanes).default_value(TOPOLOGY_LANES[axes.topology]),
            gate=FeatureGate(admitted=axes.grants, tripped=tripped),
        )


class Correlation(Struct, frozen=True):
    trace_id: TraceId
    parent_span: Option[SpanId] = Nothing
    remote: bool = False

    @classmethod
    def mint(cls) -> Self:
        # `mint` opens the branch's only trace root: a caller reaching admission with no inbound carrier starts parentless.
        return cls(trace_id=token_bytes(_TRACE_BYTES))

    @classmethod
    def seed(cls, carrier: Option[Mapping[str, str]]) -> Self:
        # inbound context is ADOPTED whole: the `observability/telemetry#TELEMETRY` composite reads the carrier through the
        # globally registered propagator, the extracted trace id continues unchanged, and the extracted span id becomes this
        # context's parent. An absent or grammar-invalid `traceparent` decodes to the all-zero trace id, the one case minting a root.
        match carrier.map(lambda inbound: trace.get_current_span(propagate.extract(inbound)).get_span_context()):
            case Option(tag="some", some=parent) if parent.trace_id != 0:
                return cls(
                    trace_id=parent.trace_id.to_bytes(_TRACE_BYTES),
                    parent_span=Some(parent.span_id.to_bytes(_SPAN_BYTES)),
                    remote=True,
                )
            case _:
                return cls.mint()

    def attributes(self) -> dict[str, str | int]:
        # `032x`/`016x` widths are the ids' own hex renderings; the parent key appears only where a parent was adopted, so an
        # absent key reads as a root rather than as a zeroed span.
        stamped: dict[str, str | int] = {"rasm.trace_id": self.trace_id.hex()}
        return self.parent_span.map(lambda span: stamped | {"rasm.parent_span_id": span.hex()}).default_value(stamped)


class Deadline(Struct, frozen=True, gc=False):
    budget: timedelta

    @property
    def seconds(self) -> float:
        return self.budget.total_seconds()


class RuntimeContext(Struct, frozen=True):
    profile: RuntimeProfile
    axes: Option[ConsumptionProfile] = Nothing
    killswitches: frozenset[Killswitch] = frozenset()
    correlation: Correlation = field(default_factory=Correlation.mint)
    deadline: Option[Deadline] = Nothing
    classification: str = "internal"
    causal: Option[CausalFrame] = Nothing

    @classmethod
    def admit(
        cls,
        profile: RuntimeProfile,
        *,
        axes: ConsumptionProfile | None = None,
        killswitches: frozenset[Killswitch] = frozenset(),
        deadline: Deadline | None = None,
        classification: str = "internal",
        causal: CausalFrame | None = None,
        carrier: Mapping[str, str] | None = None,
    ) -> Self:
        # `axes` arrives already proved: a composition root supplying its own row folds ConsumptionProfile.admit
        # first, and omitting it takes the preset's row, so no unadmitted axis value ever reaches a context.
        # One inbound header map, two disjoint reads: the clock owner decoded `causal` off the `SLOTS` slots, the propagator
        # reads `traceparent`/`tracestate`/`baggage` off the same map here — neither identity ever occupies the other's slot.
        return cls(
            profile=profile,
            axes=Option.of_optional(axes),
            killswitches=killswitches,
            correlation=Correlation.seed(Option.of_optional(carrier)),
            deadline=Option.of_optional(deadline),
            classification=classification,
            causal=Option.of_optional(causal),
        )

    @property
    def shape(self) -> ConsumptionProfile:
        return self.axes.default_value(PROFILE_ROW[self.profile])

    @property
    def policy(self) -> ProfilePolicy:
        return ProfilePolicy.of(self.shape, self.killswitches)

    @property
    def budget(self) -> Option[float]:
        return self.deadline.map(lambda d: d.seconds)

    def admits(self, feature: Feature) -> bool:
        return self.policy.gate.admits(feature)

    def tripped(self, killswitch: Killswitch) -> bool:
        return self.policy.gate.is_tripped(killswitch)

    def attribute(self) -> dict[str, str | int]:
        # `CausalFrame` contributes its packed cell as one more span ATTRIBUTE beside the W3C ids — ordering evidence, never identity.
        shape = self.shape
        base = self.correlation.attributes() | {
            "rasm.profile": self.profile.value,
            "rasm.classification": self.classification,
            "rasm.host.kind": shape.host_key,
            "rasm.deploy.tenancy": shape.tenancy.value,
            "rasm.deploy.topology": shape.topology.value,
            "rasm.deploy.lifecycle": shape.lifecycle.value,
            "rasm.deploy.isolation": shape.isolation.value,
        }
        return self.causal.map(lambda frame: base | frame.attributes("packed")).default_value(base)
```

## [03]-[BACKEND_CONTRACT]

- Owner: `BackendGeneration` is the one polymorphic entry over both directions — `compose` mints this branch's contribution from its own store artifacts, `admit` proves a contract set (its own or a merged one) against provider observations, and `merge` folds branch contributions into the deployment unit. All three land on the `_funnelled` projection, so no mint path reaches the canonical framing unproved.
- Sources: each local store artifact — migration script, journal DDL, embedded-store ensure, object-plane bucket — lands as one `ArtifactSource` row carrying key, role, bytes, providers, and dependencies; Python composes from its own artifacts alone.
- Shape: `msgspec.Struct` mirrors the contract wire once; `forbid_unknown_fields` rejects drift at decode, `order="deterministic"` frames the compose-side stream this branch then digests, while admit digests the octets it received, and `FailureRank`/`RestartClass` carry the capability vocabularies as closed wire values a peer's spelling decodes against.
- Absence: `FailureRank.absorbs` decides what a missing capability costs — refusal, a folded lane surfacing here rather than at first query, or recorded evidence — and `admit` carries the whole absence set as `absorbed`, keyed by rank. `RestartClass` ranks its disruption in declaration order, so `disruption` reports the worst bounce across a gap set instead of the cheapest.
- Identity: `xxh3_128_intdigest` at seed zero mints the generation over the compose-side framed bytes and re-derives it over the octets admit received, never over a local re-encode of a decoded document — the `CANONICAL_BYTE_IDENTITY` law, which no peer's JSON encoder reproduces; `Digest128` fixes the 32-hex width every identity cell carries.
- Order: artifact key ordinal IS the whole wire order, so `_claimed` sorts by key alone and dependency depth never re-ranks the stream into a second generation over one artifact set.
- Dependencies: `_closed` proves the `depends_on` graph closed and acyclic at the funnel every path reaches — dangling keys report before cycles, each refusal naming the exact edges — so a sort a path can skip never carries the proof.
- Merge: contributions union by key under that same order and refuse any key two claimants spell differently, artifact and capability rows alike, on the WHOLE row rather than the content cell — first-wins and last-wins each mint a generation no claimant composed.
- Evidence: `_FACTS` rows prove corpus, generation, key-set, and realization invariants under `Disposition.ACCUMULATE`, so a refusal reports every failed invariant with its reason and the exact subjects that failed it.
- Boundary: a Python-only application composes, deploys, and admits its stores with no peer branch present; provider migration execution and journal identity stay outside this owner.
- Growth: a contract field changes the one wire shape; a local provider adds one observation adapter; a new invariant is one `_FACTS` row; a new failure rank is one `FailureRank` member with one `absorbs` arm; a new disruption class is one `RestartClass` member seated at its rank.
- Packages: `msgspec`, `xxhash`, `expression`, and the shared runtime fault rail.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from base64 import b64decode, b64encode
from collections.abc import Callable, Iterable
from enum import StrEnum
from typing import Annotated, Final, Self, assert_never

import msgspec
import xxhash
from expression import Error, Ok
from expression.collections import Block, Map
from msgspec import Meta, Struct

from rasm.runtime.faults import BoundaryFault, Disposition, RuntimeRail, boundary, traversed

# --- [TYPES] ----------------------------------------------------------------------------

# one 128-bit-digest domain serves every identity cell the contract carries — artifact content and generation
# alike — so a peer re-deriving either reads the same 32-hex width off one alias and a truncated cell fails at decode.
type Digest128 = Annotated[str, Meta(pattern=r"^[0-9a-f]{32}$")]


class Absorption(StrEnum):
    REFUSE = "refuse"
    FOLD = "fold"
    RECORD = "record"


class FailureRank(StrEnum):
    REQUIRED = "required"
    DEGRADABLE = "degradable"
    OBSERVATIONAL = "observational"

    @property
    def absorbs(self) -> Absorption:
        # Rank IS the absence policy: admission refuses a missing required capability, folds a degradable lane out so
        # its absence surfaces here instead of at first query, and records an observational gap as evidence.
        match self:
            case FailureRank.REQUIRED:
                return Absorption.REFUSE
            case FailureRank.DEGRADABLE:
                return Absorption.FOLD
            case FailureRank.OBSERVATIONAL:
                return Absorption.RECORD
            case _ as unreachable:
                assert_never(unreachable)


class RestartClass(StrEnum):
    SESSION = "session"
    RELOAD = "reload"
    RESTART = "restart"

    @property
    def rank(self) -> int:
        # declaration order IS the disruption order, least to worst, so a new class lands between its neighbours and
        # every aggregate keeps ranking against one spelling.
        return tuple(RestartClass).index(self)

    @staticmethod
    def worst(over: Iterable[RestartClass], /) -> RestartClass:
        # an operator reads ONE bounce cost for a whole gap set, so the aggregate takes the worst rank; the least
        # member under-reports the disruption the repair actually costs.
        return max(over, key=lambda row: row.rank, default=RestartClass.SESSION)


# --- [MODELS] ---------------------------------------------------------------------------


class ArtifactWire(Struct, frozen=True, gc=False, forbid_unknown_fields=True, rename="camel"):
    key: str
    role: str
    content: Digest128
    providers: tuple[str, ...]
    depends_on: tuple[str, ...]


class CapabilityWire(Struct, frozen=True, gc=False, forbid_unknown_fields=True, rename="camel"):
    key: str
    lane: str
    requirement: str
    requirement_value: str
    failure_rank: FailureRank
    restart_class: RestartClass


class SchemaContractWire(Struct, frozen=True, forbid_unknown_fields=True, rename="camel"):
    contract: str
    artifacts: tuple[ArtifactWire, ...]
    capabilities: tuple[CapabilityWire, ...]


class ConformanceWire(Struct, frozen=True, forbid_unknown_fields=True, rename="camel"):
    contract: str
    generation: Digest128
    canonical: str
    json_schema: str
    artifact_keys: tuple[str, ...]
    capability_keys: tuple[str, ...]
    required_capabilities: tuple[str, ...]


class BackendObservation(Struct, frozen=True):
    generation: Digest128
    capabilities: frozenset[str]
    artifacts: frozenset[str]


class ArtifactSource(Struct, frozen=True):
    key: str
    role: str
    payload: bytes
    providers: tuple[str, ...] = ()
    depends_on: tuple[str, ...] = ()


class ContractFiles(Struct, frozen=True):
    contract: bytes
    schema: bytes
    conformance: bytes


class ContractEvidence(Struct, frozen=True):
    files: ContractFiles
    wire: SchemaContractWire
    corpus: ConformanceWire
    corpus_canonical: bytes
    corpus_schema: bytes
    generation: Digest128
    observed: BackendObservation

    @classmethod
    def of(cls, files: ContractFiles, observed: BackendObservation, /) -> Self:
        # every decode and base64 lift lands here under the admit fence, so each `_FACTS` row stays a pure predicate over
        # settled material and a malformed corpus fails as one decode fault instead of mid-proof. Generation digests the
        # TRANSPORTED bytes: re-encoding a decoded document reproduces no peer's bytes, so a local re-encode forks the digest.
        wire = msgspec.json.decode(files.contract, type=SchemaContractWire)
        corpus = msgspec.json.decode(files.conformance, type=ConformanceWire)
        return cls(
            files=files,
            wire=wire,
            corpus=corpus,
            corpus_canonical=b64decode(corpus.canonical, validate=True),
            corpus_schema=b64decode(corpus.json_schema, validate=True),
            generation=f"{xxhash.xxh3_128_intdigest(files.contract):032x}",
            observed=observed,
        )

    @property
    def artifact_keys(self) -> tuple[str, ...]:
        return tuple(row.key for row in self.wire.artifacts)

    @property
    def capability_keys(self) -> tuple[str, ...]:
        return tuple(row.key for row in self.wire.capabilities)


class ContractFact(Struct, frozen=True):
    reason: str
    holds: Callable[[ContractEvidence], bool]
    subjects: Callable[[ContractEvidence], tuple[str, ...]]


# --- [OPERATIONS] -------------------------------------------------------------------------


def _refused(subject: str, reason: str, subjects: tuple[str, ...]) -> BoundaryFault:
    # a refusal names the invariant AND the exact keys failing it, so the caller repairs the row it owns rather than
    # re-deriving which of nine invariants one opaque message meant.
    return BoundaryFault(config=(subject, f"{reason}:{','.join(subjects)}"))


# --- [SERVICES] ---------------------------------------------------------------------------


class BackendGeneration(Struct, frozen=True):
    contract: SchemaContractWire
    generation: Digest128
    observed: BackendObservation
    absorbed: Map[FailureRank, frozenset[str]]

    @property
    def disruption(self) -> RestartClass:
        # `disruption` reports one bounce cost for the whole absorbed gap set, keyed off the capability rows themselves.
        return RestartClass.worst(row.restart_class for row in self.contract.capabilities if row.key not in self.observed.capabilities)

    @classmethod
    def compose(
        cls,
        contract: str,
        artifacts: Block[ArtifactSource],
        capabilities: Block[CapabilityWire],
        /,
    ) -> RuntimeRail[ContractFiles]:
        """Mint this branch's contribution from its own store artifacts."""
        return cls._funnelled(
            contract,
            artifacts.map(lambda row: ArtifactWire(
                key=row.key,
                role=row.role,
                content=f"{xxhash.xxh3_128_intdigest(row.payload):032x}",
                # dedupe-and-sort at the mint: the framed preimage carries one spelling per provider and dependency,
                # so two branches listing the same set in different order derive one digest.
                providers=tuple(sorted(frozenset(row.providers))),
                depends_on=tuple(sorted(frozenset(row.depends_on))),
            )),
            capabilities,
            "backend.compose",
        )

    @classmethod
    def merge(cls, contract: str, contributions: Block[ContractFiles], /) -> RuntimeRail[ContractFiles]:
        """Fold branch contributions into the one deployment unit, keyed and deterministic."""
        return boundary(
            "backend.merge",
            lambda: Block.of_seq(msgspec.json.decode(one.contract, type=SchemaContractWire) for one in contributions),
            catch=(msgspec.DecodeError, msgspec.ValidationError),
        ).bind(lambda wires: cls._funnelled(
            contract,
            Block.of_seq(row for wire in wires for row in wire.artifacts),
            Block.of_seq(row for wire in wires for row in wire.capabilities),
            "backend.merge",
        ))

    @classmethod
    def admit(cls, files: ContractFiles, observed: BackendObservation, /) -> RuntimeRail[Self]:
        """Prove a contract set — locally composed or merged — against realized provider observations."""
        return boundary(
            "backend.contract",
            lambda: ContractEvidence.of(files, observed),
            catch=(ValueError, msgspec.DecodeError, msgspec.ValidationError),
        ).bind(cls._proved)

    # `_funnelled` is the one projection funnel: both row families claim their keys, the dependency graph proves closed
    # and acyclic, and only a survivor of both reaches the canonical framing, so no mint path leaves it unproved.
    @classmethod
    def _funnelled(
        cls, contract: str, artifacts: Block[ArtifactWire], capabilities: Block[CapabilityWire], subject: str, /
    ) -> RuntimeRail[ContractFiles]:
        def framed(rows: tuple[ArtifactWire, ...]) -> RuntimeRail[ContractFiles]:
            return cls._claimed(capabilities, subject, "capability").bind(
                lambda held: cls._projected(SchemaContractWire(contract=contract, artifacts=rows, capabilities=held), subject)
            )

        return cls._claimed(artifacts, subject, "artifact").bind(lambda rows: cls._closed(rows, subject)).bind(framed)

    @staticmethod
    def _claimed[R: (ArtifactWire, CapabilityWire)](rows: Block[R], subject: str, family: str, /) -> RuntimeRail[tuple[R, ...]]:
        # one claim owner over both row families and both mint paths: identical rows dedupe, a key two claimants spell
        # differently REFUSES on the WHOLE row, and survivors leave in key-ordinal order — the entire wire order.
        claimed = {row.key: row for row in rows}
        collided = tuple(sorted({row.key for row in rows if claimed[row.key] != row}))
        return (
            Error(_refused(subject, f"{family}-collision", collided))
            if collided
            else Ok(tuple(claimed[key] for key in sorted(claimed)))
        )

    @staticmethod
    def _closed(artifacts: tuple[ArtifactWire, ...], subject: str, /) -> RuntimeRail[tuple[ArtifactWire, ...]]:
        # dependency keys are digest-bearing payload, so closure and acyclicity prove HERE and never at a sort a path
        # can skip: one Kahn sweep grows the settled set to its fixpoint, and every key left outside it sits on a cycle.
        keys = frozenset(row.key for row in artifacts)
        edges = {row.key: frozenset(row.depends_on) for row in artifacts}
        dangling = tuple(sorted({f"{row.key}->{dep}" for row in artifacts for dep in row.depends_on if dep not in keys}))

        def settled(done: frozenset[str]) -> frozenset[str]:
            grown = done | frozenset(key for key, deps in edges.items() if deps <= done)
            return done if grown == done else settled(grown)

        match (dangling, tuple(sorted(keys - settled(frozenset())))):
            case ((), ()):
                return Ok(artifacts)
            case ((), cyclic):
                return Error(_refused(subject, "artifact-dependency-cyclic", cyclic))
            case (loose, _):
                # Dangling edges also strand their claimants outside the fixpoint, so this arm reports first and an
                # operator repairs a real missing artifact instead of chasing a phantom cycle.
                return Error(_refused(subject, "artifact-dependency-dangling", loose))

    @staticmethod
    def _projected(wire: SchemaContractWire, subject: str, /) -> RuntimeRail[ContractFiles]:
        # deterministic bytes, the seed-zero generation over exactly those bytes, and the corpus stating what a peer
        # re-proves from its own inputs rather than by reading this branch's digest.
        def framed() -> ContractFiles:
            canonical = msgspec.json.encode(wire, order="deterministic")
            schema = msgspec.json.encode(msgspec.json.schema(SchemaContractWire))
            corpus = ConformanceWire(
                contract=wire.contract,
                generation=f"{xxhash.xxh3_128_intdigest(canonical):032x}",
                canonical=b64encode(canonical).decode(),
                json_schema=b64encode(schema).decode(),
                artifact_keys=tuple(row.key for row in wire.artifacts),
                capability_keys=tuple(row.key for row in wire.capabilities),
                required_capabilities=tuple(row.key for row in wire.capabilities if row.failure_rank.absorbs is Absorption.REFUSE),
            )
            return ContractFiles(
                contract=canonical,
                schema=schema,
                conformance=msgspec.json.encode(corpus, order="deterministic"),
            )

        return boundary(subject, framed, catch=msgspec.EncodeError)

    @classmethod
    def _proved(cls, evidence: ContractEvidence, /) -> RuntimeRail[Self]:
        # ACCUMULATE folds every failed row into one aggregate fault, so a refusal reports the whole failure set with
        # each invariant's reason and subjects rather than the first row to trip.
        return traversed(
            _FACTS.map(
                lambda fact: Ok(fact.reason)
                if fact.holds(evidence)
                else Error(_refused("backend.contract", fact.reason, fact.subjects(evidence)))
            ),
            by=Disposition.ACCUMULATE,
        ).map(lambda _: cls(
            contract=evidence.wire,
            generation=evidence.generation,
            observed=evidence.observed,
            absorbed=cls._absorbed(evidence),
        ))

    @staticmethod
    def _absorbed(evidence: ContractEvidence, /) -> Map[FailureRank, frozenset[str]]:
        # every capability the observation lacks, bucketed by the rank that decides its absorption — the REQUIRED
        # bucket is empty on every admitted generation because `capability-unrealized` refuses first, the DEGRADABLE
        # bucket names the folded lanes, and the OBSERVATIONAL bucket carries the recorded gaps.
        missing = {row.key: row.failure_rank for row in evidence.wire.capabilities if row.key not in evidence.observed.capabilities}
        return Map.of_seq((rank, frozenset(key for key, held in missing.items() if held is rank)) for rank in FailureRank)


# --- [TABLES] -----------------------------------------------------------------------------

# Every contract invariant rides this table as data: a whole-document equality names the contract as its subject, a set
# relation names exactly the keys that failed it, and an ordering-only drift falls back to the rejected corpus order.
_FACTS: Final[Block[ContractFact]] = Block.of_seq([
    ContractFact("corpus-canonical-drift", lambda e: e.corpus_canonical == e.files.contract, lambda e: (e.wire.contract,)),
    ContractFact("corpus-schema-drift", lambda e: e.corpus_schema == e.files.schema, lambda e: (e.wire.contract,)),
    ContractFact("contract-name-drift", lambda e: e.corpus.contract == e.wire.contract, lambda e: (e.corpus.contract, e.wire.contract)),
    ContractFact(
        "generation-drift",
        lambda e: e.corpus.generation == e.generation == e.observed.generation,
        lambda e: (e.corpus.generation, e.generation, e.observed.generation),
    ),
    ContractFact(
        "artifact-key-drift",
        lambda e: e.corpus.artifact_keys == e.artifact_keys,
        lambda e: tuple(sorted(frozenset(e.corpus.artifact_keys) ^ frozenset(e.artifact_keys))) or e.corpus.artifact_keys,
    ),
    ContractFact(
        "capability-key-drift",
        lambda e: e.corpus.capability_keys == e.capability_keys,
        lambda e: tuple(sorted(frozenset(e.corpus.capability_keys) ^ frozenset(e.capability_keys))) or e.corpus.capability_keys,
    ),
    ContractFact(
        "artifact-unrealized",
        lambda e: frozenset(e.artifact_keys) <= e.observed.artifacts,
        lambda e: tuple(sorted(frozenset(e.artifact_keys) - e.observed.artifacts)),
    ),
    ContractFact(
        "capability-unrealized",
        lambda e: frozenset(e.corpus.required_capabilities) <= e.observed.capabilities,
        lambda e: tuple(sorted(frozenset(e.corpus.required_capabilities) - e.observed.capabilities)),
    ),
])
```

## [04]-[SETTINGS]

- Owner: `SettingsAdmission` admits init mapping, environment, dotenv, and OS secret files over the DEFAULT `pydantic-settings` precedence — no `settings_customise_sources` override exists, because restating the default order is ceremony and an override is earned only by a permutation or a new origin. Every root is typed against the `pydantic` catalogue, never bare `str`. `BasicCredential` is deliberately not named `Credential`: the serve-side `CredentialPolicy` union is `transport/serve#SERVE`'s decode of the peer-minted wire axis, a different concept under a different name.
- Entry: `SecretBoundary.resolve` is the one credential reader, parameterized over output shape by `@overload` — admitting a new consumer shape is one `SecretShape` member, one overload arm, and one fold-tail arm, never a parallel resolver. Absence folds to `Ok(Nothing)` rather than a fault: a missing credential is a wire fact the outbound leg routes. `known_hosts` returns the admission-loaded `SSHKnownHosts` the `transport/roots#RESOURCE` `ssh` leg binds — host-key verification is admission-supplied, never the disabled-verification `known_hosts=None` the connection law forbids.
- Auto: the ladder fold drops every row the carried `FeatureGate` refuses, so a session that cannot answer a keychain prompt never triggers one and a killswitched deployment dials no vault. Its declared-field twin is the branch-catalogued `GoogleSecretManagerSettingsSource` injected with this same cached client — the settings-source chain row a deployment adds when declared model fields, not per-service credentials, live in Secret Manager.
- Growth: a new setting is one typed field on the model; a new source origin or precedence permutation is the one `settings_customise_sources` override, absent until needed; a new secret BACKEND is one `CloudVault` arm with its resolver case and zero ladder edits, while a new resolution TIER is one `SecretTier` case with one `SECRET_LADDER` row carrying its `Option[Feature]` gate and `RetryClass`; a new output shape one `SecretShape` member, one overload, and one fold-tail arm.
- Law: `SecretTier` names the rung and `CloudVault` names the backend serving it — the deployment supplies one `providers`-axis row, so the resolver holds no provider default and an unnamed backend folds the rung out instead of assuming one.
- Boundary: no package reads `os.environ` after admission. Keystore and cloud tiers read credentials for the OUTBOUND transport legs only — the companion UDS serve leg reads no keyring, peer identity being the kernel accept-time credential (`transport/serve#SERVE`). Multi-source remote-config work rides the `STRUCTURED_SETTINGS_SCHEMA` idea card.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from functools import cache
from pathlib import Path
from typing import Final, Literal, assert_never, overload

import anyio
import asyncssh
import keyring
from expression import Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct
from pydantic import AnyUrl, DirectoryPath, FilePath, HttpUrl, SecretStr
from pydantic_settings import BaseSettings, SettingsConfigDict

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.resilience import RetryClass, guarded

# cold cloud dependencies: `lazy` binds defer the gRPC client stack and the crc32c digest to the gated arm's first fire.
lazy import google_crc32c
lazy import hvac
lazy from azure.core.exceptions import ResourceNotFoundError
lazy from azure.identity import DefaultAzureCredential
lazy from azure.keyvault.secrets import SecretClient
lazy from google.api_core.exceptions import NotFound
lazy from google.cloud.secretmanager import SecretManagerServiceClient
lazy from hvac.exceptions import InvalidPath

# `Feature`/`FeatureGate` are the [02]-[CONTEXT] owners of this same `rasm.runtime.admission`
# module — no cross-module import, the two fences are one module's two declaration regions.

# --- [TYPES] ----------------------------------------------------------------------------


class SecretShape(StrEnum):
    TOKEN = "token"  # the bare passphrase/bearer the SSH/HTTP legs read off `.get_secret_value()`
    CREDENTIAL = "credential"  # the (username, secret) pair the `httpx.BasicAuth` leg reads


@tagged_union(frozen=True)
class SecretTier:
    # discriminant IS the resolver — never a `Probe` callable type plus parallel free probe functions. `cloud` names the TIER;
    # which backend serves it is the deployment's `providers`-axis row, read off settings, never a literal on this union.
    tag: Literal["keystore", "cloud", "file"] = tag()
    keystore: bool = case()
    cloud: bool = case()
    file: bool = case()


@tagged_union(frozen=True)
class CloudVault:
    # one arm per admitted secret backend, each carrying its own coordinate pair `(endpoint, namespace)`: the GCP project and
    # secret-id prefix, the Vault server and KV-v2 mount, the Key Vault url and name prefix. A fourth backend is one arm and
    # one resolver case; the ladder never widens, because the tier is the rung and the backend is the deployment's row.
    tag: Literal["gcp", "vault", "azure"] = tag()
    gcp: tuple[str, str] = case()
    vault: tuple[str, str] = case()
    azure: tuple[str, str] = case()


# --- [CONSTANTS] ------------------------------------------------------------------------

# one secret-mount anchor the `secrets_dir` source target and the `secrets_mount` field default both read; a deployment override
# threads the paired `secrets_mount=` field + `_secrets_dir=` kwarg so the two never split.
_SECRETS_MOUNT: Final[str] = "/run/secrets"

# secret-probe thread bound: this tier sits below the lanes bands, so it carries its own explicit limiter sized just past the
# ladder depth, bounding every concurrent resolve without leaning on the ambient default limiter.
_PROBE_BAND: Final[anyio.CapacityLimiter] = anyio.CapacityLimiter(4)

# --- [MODELS] ---------------------------------------------------------------------------


class BasicCredential(Struct, frozen=True):
    username: str
    secret: SecretStr


class SettingsAdmission(BaseSettings):
    # pydantic's edge carries pydantic-native types and `| None`, lifted to `Option` at the read site — never an `expression.Option`
    # field pydantic-core cannot build a core schema for.
    model_config = SettingsConfigDict(frozen=True, extra="forbid", env_prefix="RASM_PY_", env_nested_delimiter="__", secrets_dir=_SECRETS_MOUNT)

    scratch_root: DirectoryPath
    object_store_root: AnyUrl | None = None
    otel_endpoint: HttpUrl | None = None
    pyroscope_endpoint: HttpUrl | None = None
    known_hosts: FilePath | None = None
    secrets_mount: Path = Path(_SECRETS_MOUNT)
    vault_backend: Literal["gcp", "vault", "azure"] | None = None
    vault_endpoint: str | None = None  # gcp project id, Vault server url, or Key Vault url — the backend's one coordinate
    vault_namespace: str = "rasm"  # gcp secret-id prefix, Vault KV-v2 mount point, or Key Vault name prefix

    def vault(self) -> Option[CloudVault]:
        # deployment names the backend; an unnamed or coordinate-less backend folds the cloud tier out rather than defaulting.
        return Option.of_optional(self.vault_backend).bind(
            lambda backend: Option.of_optional(self.vault_endpoint).map(
                lambda endpoint: CloudVault(**{backend: (endpoint, self.vault_namespace)})
            )
        )


class TierRow(Struct, frozen=True):
    tier: SecretTier
    gate: Option[Feature]
    retry_class: RetryClass


class SecretBoundary(Struct, frozen=True):
    settings: SettingsAdmission
    gate: FeatureGate

    @overload
    async def resolve(self, service: str, username: str | None = ..., shape: Literal[SecretShape.TOKEN] = ...) -> RuntimeRail[Option[SecretStr]]: ...
    @overload
    async def resolve(
        self, service: str, username: str | None = ..., *, shape: Literal[SecretShape.CREDENTIAL]
    ) -> RuntimeRail[Option[BasicCredential]]: ...
    async def resolve(
        self, service: str, username: str | None = None, shape: SecretShape = SecretShape.TOKEN
    ) -> RuntimeRail[Option[SecretStr]] | RuntimeRail[Option[BasicCredential]]:
        # `username=None` resolves the keystore's backend-default user; the resolved `BasicCredential.username` is read back off the store, never re-stamped.
        admitted = SECRET_LADDER.filter(lambda row: row.gate.map(self.gate.admits).default_value(True))

        async def walk(rows: Block[TierRow]) -> RuntimeRail[Option[BasicCredential]]:
            # `Block.fold` cannot await the per-tier `guarded`, so the closed-arity ladder recurses head-then-tail, the empty ladder
            # folding to `Ok(Nothing)`; the arms match the carrier tag, never a class pattern — `Ok`/`Error`/`Some` are constructor functions.
            match rows.try_head():
                case Option(tag="some", some=head):
                    match await self._probe(head, service, username):
                        case Result(tag="error") as faulted:
                            return faulted
                        case Result(tag="ok", ok=Option(tag="some")) as hit:
                            return hit
                        case _:
                            return await walk(rows.tail())
                case _:
                    return Ok(Nothing)

        resolved = await walk(admitted)
        return resolved if shape is SecretShape.CREDENTIAL else resolved.map(lambda cred: cred.map(lambda c: c.secret))

    async def _probe(self, row: TierRow, service: str, username: str | None) -> RuntimeRail[Option[BasicCredential]]:
        # each tier arm owns its complete `guarded(...)` envelope — the blocking read offloads to the anyio worker pool, a transient
        # retries under the row's policy inside one span, the terminal raise lifts once — never a bare `boundary` failing on the first transient.
        match row.tier:
            case SecretTier(tag="keystore"):

                def keystore_read() -> Option[BasicCredential]:
                    # `NoKeyringError` (no viable backend, headless) is a MISS floored to `Nothing` — never retried, never a terminal Error.
                    try:
                        found = keyring.get_credential(service, username)
                    except keyring.errors.NoKeyringError:
                        return Nothing
                    return Option.of_optional(found).map(lambda c: BasicCredential(c.username, SecretStr(c.password)))

                return await guarded(row.retry_class, anyio.to_thread.run_sync, keystore_read, subject="secret", limiter=_PROBE_BAND)
            case SecretTier(tag="cloud"):
                secret = _secret_name(service, username)
                lifted = lambda raw: Some(BasicCredential(username or service, SecretStr(raw.strip())))

                def gcp_read(project: str, prefix: str) -> Option[BasicCredential]:
                    # `NotFound` is a MISS (the keyring missing-vs-error law on the cloud store); transport faults raise into the retry.
                    client = _secret_client()
                    name = client.secret_version_path(project, f"{prefix}-{secret}", "latest")
                    try:
                        payload = client.access_secret_version(name=name).payload
                    except NotFound:
                        return Nothing
                    if google_crc32c.value(payload.data) != payload.data_crc32c:
                        # Secret Manager's client does NOT self-verify `data_crc32c`: a mismatch is corrupted transport — a retryable
                        # `OSError` transient, never a MISS and never a silently-trusted payload.
                        raise OSError(f"secret-crc32c:{name}")
                    return lifted(payload.data.decode("utf-8"))

                def vault_read(url: str, mount: str) -> Option[BasicCredential]:
                    # `InvalidPath` (404) is the MISS arm matching GCP's `NotFound`; Forbidden/Unauthorized raise as boundary faults.
                    try:
                        body = hvac.Client(url=url).secrets.kv.v2.read_secret_version(path=secret, mount_point=mount)
                    except InvalidPath:
                        return Nothing
                    return Option.of_optional(body["data"]["data"].get(secret)).bind(lifted)

                def azure_read(vault_url: str, prefix: str) -> Option[BasicCredential]:
                    # `ResourceNotFoundError` (404) is the MISS arm; the credential resolves at deployment as ADC backs the GCP arm.
                    try:
                        found = SecretClient(vault_url, DefaultAzureCredential()).get_secret(f"{prefix}-{secret}")
                    except ResourceNotFoundError:
                        return Nothing
                    return Option.of_optional(found.value).bind(lifted)

                match self.settings.vault():
                    case Option(tag="some", some=CloudVault(tag="gcp", gcp=(endpoint, namespace))):
                        read = lambda: gcp_read(endpoint, namespace)
                    case Option(tag="some", some=CloudVault(tag="vault", vault=(endpoint, namespace))):
                        read = lambda: vault_read(endpoint, namespace)
                    case Option(tag="some", some=CloudVault(tag="azure", azure=(endpoint, namespace))):
                        read = lambda: azure_read(endpoint, namespace)
                    case _:
                        # no backend named: the rung folds to a miss, no client constructed and no provider assumed.
                        return Ok(Nothing)
                return await guarded(row.retry_class, anyio.to_thread.run_sync, read, subject="secret", limiter=_PROBE_BAND)
            case SecretTier(tag="file"):

                def file_read() -> Option[BasicCredential]:
                    path = self.settings.secrets_mount / _secret_name(service, username)
                    return (
                        Some(BasicCredential(username or service, SecretStr(path.read_text(encoding="utf-8").strip()))) if path.exists() else Nothing
                    )

                return await guarded(row.retry_class, anyio.to_thread.run_sync, file_read, subject="secret", limiter=_PROBE_BAND)
            case _ as unreachable:
                assert_never(unreachable)

    def known_hosts(self) -> RuntimeRail[asyncssh.SSHKnownHosts]:
        path = Option.of_optional(self.settings.known_hosts).map(str).default_value(str(Path.home() / ".ssh" / "known_hosts"))
        return boundary("resource", lambda: asyncssh.read_known_hosts(path), catch=OSError)


# --- [OPERATIONS] -----------------------------------------------------------------------


# one secret-naming correspondence the file mount and the cloud namespace both read.
def _secret_name(service: str, username: str | None) -> str:
    return Option.of_optional(username).map(lambda u: f"{service}_{u}").default_value(service)


# ADC-resolved once per process; a pinned key path rides `from_service_account_file`.
@cache
def _secret_client() -> SecretManagerServiceClient:
    return SecretManagerServiceClient()


# --- [TABLES] ---------------------------------------------------------------------------

# keystore over cloud over file: each row binds its profile gate and the `RetryClass.SECRET` row (`KeyringLocked`/`OSError` transients
# under one backoff); the cloud rung is live only where `Feature.SECRET_MANAGER` admits AND settings name a vault backend.
SECRET_LADDER: Final[Block[TierRow]] = Block.of_seq([
    TierRow(SecretTier(keystore=True), Some(Feature.KEYSTORE_PROBE), RetryClass.SECRET),
    TierRow(SecretTier(cloud=True), Some(Feature.SECRET_MANAGER), RetryClass.SECRET),
    TierRow(SecretTier(file=True), Nothing, RetryClass.SECRET),
])
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [SPAN_CONTEXT_VALIDITY]-[OPEN]: which `SpanContext` members spell the validity and remoteness reads, so `Correlation.seed` matches on them instead of comparing the extracted trace id against the all-zero W3C invalid value; verify against `libs/python/.api/opentelemetry-api.md` `[02]` trace family, package `opentelemetry-api`.
