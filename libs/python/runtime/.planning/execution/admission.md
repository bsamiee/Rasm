# [PY_RUNTIME_ADMISSION]

Caller-owned context and settings admission: one immutable `RuntimeContext` carries the supplied `ConsumptionProfile` axis row, correlation, deadline, classification, and inbound `CausalFrame` a caller supplies; one `SettingsAdmission` owns the local settings source order; `SecretBoundary.resolve` is the one credential reader the outbound `transport/roots#RESOURCE` legs consume — output-parameterized, profile-gated, and lazy on the outbound leg, never an eager unattended probe.

`CausalFrame`, `Hlc`, and `Tenant` arrive from the `evidence/clock#CLOCK` owner — admission consumes the stamp that owner mints and re-mints nothing. Each `SECRET_LADDER` tier folds through the `reliability/resilience#RESILIENCE` `guarded` envelope under the `RetryClass.SECRET` row, so a transiently-locked keystore or unreachable Secret Manager retries inside one derivation span rather than failing the resolve. Feature gating folds from the bound provider rows and killswitch state rides the context, never boolean knobs the caller re-derives. This package never discovers the host, starts services, owns lifecycle, derives product roots, reads the environment after admission, or caches a global mutable context, and a resolved secret crosses as `SecretStr`, never a bare `str` a receipt or log egress serializes.

## [01]-[INDEX]

- [02]-[CONTEXT]: six-axis `ConsumptionProfile` row with its refusal, closed classification band, declared `RecoveryObjective` window, adopted W3C correlation, clock-consumed causal frame.
- [03]-[BACKEND_CONTRACT]: local contract composition, the deterministic branch merge, generation proof, realized-evidence admission, and the measured recovery window graded against the declared objective.
- [04]-[SETTINGS]: `SettingsAdmission` source order, the `SECRET_LADDER` tier table, and the output-parameterized `SecretBoundary.resolve`.
- [05]-[TENANCY]: `PrincipalScope`, `Trust` issuer rows, and `TenantAdoption` — authenticated delivery scope plus event-claim admission.

## [02]-[CONTEXT]

- Owner: `RuntimeContext` is the one caller-supplied context discriminating deployment shape, correlation, deadline, and classification, carrying the inbound `causal` frame as `Option[CausalFrame]` — `Nothing` locally minted, `Some(frame)` the host stamp — and the inbound header map as `carrier`. `ConsumptionProfile` carries the six-axis row `tenancy`, `topology`, `host`, `lifecycle`, `isolation`, `providers`; `RuntimeProfile` names the presets this branch supplies over that roster and `PROFILE_ROW` expands each to its axis tuple, so a preset is a name for a row and never a discriminant a fold switches on. `Classification` closes the sensitivity band every projection carries, and `RecoveryObjective` the declared `(rpo, rto)` durability window `TOPOLOGY_RECOVERY` supplies per deployment shape.
- Cases: `tenancy` = none | single | multi; `topology` = in-host | sidecar | companion | service | edge | cli; `lifecycle` = caller-owned | package-owned; `isolation` = in-proc | thread | process | wasm | remote; `classification` = internal | restricted | public; `host` and `providers` carry `HostRow` and `ProviderRow` descriptors this branch supplies through `HOST_ROWS` and `PROVIDER_ROWS`; a refused axis rides the fault union's `config` case keyed `profile.<axis>`, so the boot fold that admits a row composes it through the same rail every other step returns through.
- Law: `HostRow` and `ProviderRow` answer the estate consumption-descriptor floor — `fits`, `admit`, `lifetime`, and a `degrade` DERIVED off the capability columns beside a stated `concedes` — and refuse the two coordinates neither family decides: a descriptor separates no tenant, since the profile's own closed `tenancy` axis does, and the closed `isolation` roster stays Tier-0's, since `ISOLATION_FEATURE` crosses each value with the feature a row `supplies` so `admit` refuses an unserved value on the `isolation` axis by name where a per-row `reach` column re-minted that roster once per port.
- Law: adoption is the conformance — an inbound `traceparent` continues its trace id and parents its span id, and a fresh 16-byte root mints only where extraction yields no valid parent, so the Python leg never fractures a distributed trace. `Hlc` and the W3C context ride one carrier as two disjoint reads: the packed cell projects as the `rasm.hlc` span attribute, and the identity slots take propagator output alone.
- Entry: `ConsumptionProfile.admit` is the one axis gate returning `RuntimeRail[ConsumptionProfile]`, and `seated` is its construction face — `PROFILE_ROW` folds every preset through it at import and `RuntimeContext.admit` folds a hand-supplied row through it at boot, so the gate has a producer on both paths rather than an obligation a composition root can skip. `Deadline.seconds` is the one `float` the `execution/lanes#LANE` `LanePolicy.deadline` reads — never a re-derived `total_seconds()` at the lane seam. `Correlation.seed` is the one inbound-context owner: it adopts the extracted W3C parent whole under the disjoint `TraceId`/`SpanId` domains its two mint sites size, and `attribute` folds the carried frame through `CausalFrame.attributes("packed")` rather than re-spelling the `(rasm.tenant, rasm.hlc)` columns, so the result is admissible to `Span.set_attributes` directly.
- Auto: `ProfilePolicy.of` folds every behavior column out of axis values — `lifecycle` decides eager import, `lifecycle` beside the host descriptor's `scratch` column decides scratch writability, and the host descriptor's `lanes` and `recovery` columns override `TOPOLOGY_LANES` and `TOPOLOGY_RECOVERY` for capacity and durability — so no column keys on a preset name and a caller never re-derives a flag. `emit_otel` is a fold over the carried `FeatureGate` rather than a stored column, so the bound telemetry provider and the killswitch revoking it answer as one availability read.
- Receipt: the admitted `ConsumptionProfile` itself is the local deployment receipt; no generated peer document restates its branch-owned axes.
- Law: every refusal on this page resolves ONE `reliability/faults#FAULT` `RAISES` anchor under `RuntimeLeg.ADMISSION` and derives its subject from that leg; the axis a profile refuses, the isolation grant it lacks, the issuer, the grade, and the authenticated tenant selection all ride NAMED row slots, so recovery keys on the coordinate rather than parsing a reason sentence.
- Growth: a new context field is one `RuntimeContext` column; a new host integration is one `HOST_ROWS` descriptor and a new bound port one `PROVIDER_ROWS` descriptor; a new feature is one `Feature` case supplied by a provider row; a new killswitch is one `Killswitch` case with one `KILLSWITCH_FEATURE` disabling edge — never a parallel boolean knob; a new sensitivity band is one `Classification` member reaching the projection unedited; a new durability preset is one `TOPOLOGY_RECOVERY` row or one `HostRow.recovery` value, never a second DR taxonomy; a new attribute dimension is one entry in the `attribute` projection; a new propagated wire format is one row at the telemetry install's composite, reaching `seed` with no edit here.
- Boundary: no environment probing, host discovery, service-root construction, or global mutable context lives here — deployment shape arrives as one supplied row and this package infers none of it; axis values stay data, so a compile-time assumption, an ambient global, an environment flag, and a fold branching on which product hosts the package are the four deleted forms; `ConsumptionProfile.admit` refuses an unservable axis value onto the fault union's `config` case whose subject names the axis, so silent degradation and a narrowed public surface never happen and no sibling refusal type stands beside the one union every package returns through; in-host topology carrying no host descriptor refuses on the `host` axis because a consuming application supplies its own row; killswitches ride `RuntimeContext.killswitches` as caller-supplied operational state, never a profile column, so revoking a feature never re-cuts deployment shape; `RecoveryObjective` is DECLARED here and measured nowhere on this cluster — `[03]-[BACKEND_CONTRACT]` grades an observed window against the row a caller threads it, so a target and a reading never share a struct; `CausalFrame`/`Hlc`/`Tenant` stay the `evidence/clock#CLOCK` owner's records; propagator registration stays the `observability/telemetry#TELEMETRY` install's, this owner reading the global it publishes; each branch spells the roster in its own types, so a peer branch's descriptor rows are never mirrored here row-for-row.

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
from typing import Final, NewType, Self

from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Map
from msgspec import Struct, ValidationError, field
from opentelemetry import propagate, trace
from rasm.runtime.clock import CausalFrame
from rasm.runtime.faults import PROFILE_GRANT, PROFILE_HOST, RuntimeRail

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


class Classification(StrEnum):
    PUBLIC = "public"
    INTERNAL = "internal"
    RESTRICTED = "restricted"
    SECRET = "secret"

    @property
    def rank(self) -> int:
        return GRADE_RANK[self]


GRADE_RANK: Final[Map[Classification, int]] = Map.of_seq((grade, rank) for rank, grade in enumerate(Classification))


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
    DISABLE_TELEMETRY = "disable_telemetry"


TraceId = NewType("TraceId", bytes)
SpanId = NewType("SpanId", bytes)

Ticks = NewType("Ticks", int)

# --- [CONSTANTS] ------------------------------------------------------------------------

_TRACE_BYTES: Final[int] = 16
_SPAN_BYTES: Final[int] = 8
_TICK_MINUTE: Final[int] = 600_000_000

KILLSWITCH_FEATURE: Final[Map[Killswitch, Feature]] = Map.of_seq([
    (Killswitch.DISABLE_OUTBOUND, Feature.OUTBOUND_TRANSPORT),
    (Killswitch.DISABLE_SECRET_MANAGER, Feature.SECRET_MANAGER),
    (Killswitch.DISABLE_TELEMETRY, Feature.TELEMETRY_EXPORT),
])

ISOLATION_FEATURE: Final[Map[Isolation, Option[Feature]]] = Map.of_seq([
    (Isolation.IN_PROC, Nothing),
    (Isolation.THREAD, Nothing),
    (Isolation.PROCESS, Some(Feature.LOCAL_SPAWN)),
    (Isolation.WASM, Some(Feature.WASM_GUEST)),
    (Isolation.REMOTE, Some(Feature.OUTBOUND_TRANSPORT)),
])

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
        return feature in self.admitted and not any(KILLSWITCH_FEATURE[k] is feature for k in self.tripped)

    def is_tripped(self, killswitch: Killswitch) -> bool:
        return killswitch in self.tripped


class RecoveryObjective(Struct, frozen=True, gc=False):
    rpo: Ticks
    rto: Ticks


TOPOLOGY_RECOVERY: Final[Map[Topology, RecoveryObjective]] = Map.of_seq([
    (Topology.IN_HOST, RecoveryObjective(rpo=Ticks(15 * _TICK_MINUTE), rto=Ticks(60 * _TICK_MINUTE))),
    (Topology.SIDECAR, RecoveryObjective(rpo=Ticks(5 * _TICK_MINUTE), rto=Ticks(30 * _TICK_MINUTE))),
    (Topology.COMPANION, RecoveryObjective(rpo=Ticks(5 * _TICK_MINUTE), rto=Ticks(30 * _TICK_MINUTE))),
    (Topology.SERVICE, RecoveryObjective(rpo=Ticks(1 * _TICK_MINUTE), rto=Ticks(15 * _TICK_MINUTE))),
    (Topology.EDGE, RecoveryObjective(rpo=Ticks(1 * _TICK_MINUTE), rto=Ticks(15 * _TICK_MINUTE))),
    (Topology.CLI, RecoveryObjective(rpo=Ticks(15 * _TICK_MINUTE), rto=Ticks(60 * _TICK_MINUTE))),
])


class HostRow(Struct, frozen=True):
    key: str
    lanes: int
    recovery: RecoveryObjective
    scratch: bool
    document: bool
    fits: str
    admit: str
    lifetime: str
    concedes: tuple[str, ...] = ()

    @property
    def degrade(self) -> tuple[str, ...]:
        return (
            *(() if self.scratch else ("no writable scratch root — every spill, cache, and staged artifact stays in memory or refuses by name",)),
            *(() if self.document else ("no host document surface — an owner needing one refuses rather than opening a document of its own",)),
            *self.concedes,
        )


class ProviderRow(Struct, frozen=True, gc=False):
    key: str
    supplies: Feature
    fits: str
    admit: str
    lifetime: str
    concedes: tuple[str, ...] = ()

    @property
    def degrade(self) -> tuple[str, ...]:
        return (
            *(
                f"an `{isolation}` crossing refuses at `profile.{ProfileAxis.ISOLATION}` in every profile leaving this row unbound"
                for isolation, needed in ISOLATION_FEATURE.items()
                if needed == Some(self.supplies)
            ),
            *(
                f"`{switch}` revokes `{self.supplies}`, so a tripped switch reads exactly as an unbound port"
                for switch, feature in KILLSWITCH_FEATURE.items()
                if feature is self.supplies
            ),
            *self.concedes,
        )


HOST_ROWS: Final[Map[str, HostRow]] = Map.of_seq([
    (
        "embedded",
        HostRow(
            key="embedded", lanes=4, recovery=TOPOLOGY_RECOVERY[Topology.IN_HOST], scratch=False, document=False,
            fits="an application embedding this branch inside its own process and keeping the lifecycle",
            admit="work enters on the embedding application's own call and rides the four-lane pool `lanes` sizes",
            lifetime="every lane and handle lives exactly as long as the embedding process, and the CALLER ends them — a caller-owned lifecycle makes teardown the host's call, never this branch's",
        ),
    ),
    (
        "test-harness",
        HostRow(
            key="test-harness", lanes=2, recovery=RecoveryObjective(rpo=Ticks(0), rto=Ticks(0)), scratch=True, document=False,
            fits="the proof estate's per-run host, whose store is rebuilt whole rather than carried between runs",
            admit="work enters per proof through the two-lane pool, and fixture bytes through the scratch root `scratch` opens",
            lifetime="everything lives exactly one run and the harness ends it by rebuilding the store whole, so nothing survives into a second proof",
            concedes=("zero recovery is STRUCTURAL rather than measured: a store rebuilt whole per run loses no data a restore could take back and times no restore",),
        ),
    ),
])

PROVIDER_ROWS: Final[Map[str, ProviderRow]] = Map.of_seq([
    (
        "secret-manager",
        ProviderRow(
            key="secret-manager", supplies=Feature.SECRET_MANAGER,
            fits="a deployment whose secret material lives in an external manager rather than in the process image",
            admit="the composition root binds the manager port and material enters on a resolve call, never at import",
            lifetime="material lives for the lease the manager grants and this branch caches none of it, so the manager alone expires it",
        ),
    ),
    (
        "keystore",
        ProviderRow(
            key="keystore", supplies=Feature.KEYSTORE_PROBE,
            fits="a host carrying an OS keystore this process may probe for a key the host already holds",
            admit="the composition root binds the probe and a key handle enters on the probing call inside this process",
            lifetime="a handle lives for the probing call and nothing outlives that frame; the operating system's own keystore ends the key behind it",
            concedes=("probe-only: this branch reads what the host already holds and mints, rotates, and stores nothing",),
        ),
    ),
    (
        "egress",
        ProviderRow(
            key="egress", supplies=Feature.OUTBOUND_TRANSPORT,
            fits="a deployment reaching a network peer at all, which is why every profile past in-proc binds it",
            admit="the composition root binds the outbound transport and a request enters through that transport's own pool",
            lifetime="a connection lives for the pool's idle window and a request for its deadline; the transport resource closes both and process exit ends the pool",
        ),
    ),
    (
        "otlp-collector",
        ProviderRow(
            key="otlp-collector", supplies=Feature.TELEMETRY_EXPORT,
            fits="a deployment exporting spans, metrics, and logs to a collector rather than dropping them at the process edge",
            admit="the composition root binds the exporter and a signal enters through the telemetry install's batch processor",
            lifetime="a batched signal lives to the processor's next flush; the telemetry install's shutdown drains the last batch",
        ),
    ),
    (
        "process-pool",
        ProviderRow(
            key="process-pool", supplies=Feature.LOCAL_SPAWN,
            fits="a host permitting this branch to spawn operating-system processes of its own",
            admit="the composition root binds the spawner and a work item enters on the crossing that submits it",
            lifetime="a worker lives for the pool the crossing opened; the lane owner ends it at that crossing's close and orphans none across a boot",
        ),
    ),
    (
        "wasm-sandbox",
        ProviderRow(
            key="wasm-sandbox", supplies=Feature.WASM_GUEST,
            fits="a deployment running third-party or untrusted code inside a guest rather than in this process's own address space",
            admit="the composition root binds the guest runtime and a module, with every call into it, enters through that runtime",
            lifetime="an instance lives for the call that entered it and the guest runtime tears it down at return, holding nothing across calls",
            concedes=("a guest reaches the host through bound ports alone, so a module wanting a filesystem or a socket refuses rather than borrowing this process's own",),
        ),
    ),
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
    def admit(cls, row: "ConsumptionProfile") -> RuntimeRail["ConsumptionProfile"]:
        if row.topology is Topology.IN_HOST and row.host is Nothing:
            return Error(PROFILE_HOST.raised(ProfileAxis.HOST.value))
        match ISOLATION_FEATURE[row.isolation]:
            case Option(tag="some", some=needed) if needed not in row.grants:
                return Error(PROFILE_GRANT.raised(ProfileAxis.ISOLATION.value, row.isolation.value, needed.value))
            case _:
                return Ok(row)

    @classmethod
    def seated(cls, row: "ConsumptionProfile", /) -> "ConsumptionProfile":
        match cls.admit(row):
            case Result(tag="error", error=fault):
                raise ValidationError(f"unadmitted deployment axes: {fault.facts()}")
            case _:
                return row

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
]).map(lambda _preset, row: ConsumptionProfile.seated(row))


class ProfilePolicy(Struct, frozen=True):
    eager_import: bool
    scratch_writable: bool
    lane_capacity: int
    recovery: RecoveryObjective
    gate: FeatureGate

    @classmethod
    def of(cls, axes: ConsumptionProfile, tripped: frozenset[Killswitch]) -> "ProfilePolicy":
        return cls(
            eager_import=axes.lifecycle is Lifecycle.PACKAGE_OWNED,
            scratch_writable=axes.lifecycle is Lifecycle.PACKAGE_OWNED or axes.host.map(lambda row: row.scratch).default_value(False),
            lane_capacity=axes.host.map(lambda row: row.lanes).default_value(TOPOLOGY_LANES[axes.topology]),
            recovery=axes.host.map(lambda row: row.recovery).default_value(TOPOLOGY_RECOVERY[axes.topology]),
            gate=FeatureGate(admitted=axes.grants, tripped=tripped),
        )

    @property
    def emit_otel(self) -> bool:
        return self.gate.admits(Feature.TELEMETRY_EXPORT)


class Correlation(Struct, frozen=True):
    trace_id: TraceId
    parent_span: Option[SpanId] = Nothing
    remote: bool = False

    @classmethod
    def mint(cls) -> Self:
        return cls(trace_id=TraceId(token_bytes(_TRACE_BYTES)))

    @classmethod
    def seed(cls, carrier: Option[Mapping[str, str]]) -> Self:
        match carrier.map(lambda inbound: trace.get_current_span(propagate.extract(inbound)).get_span_context()):
            case Option(tag="some", some=parent) if parent.is_valid:
                return cls(
                    trace_id=TraceId(parent.trace_id.to_bytes(_TRACE_BYTES)),
                    parent_span=Some(SpanId(parent.span_id.to_bytes(_SPAN_BYTES))),
                    remote=parent.is_remote,
                )
            case _:
                return cls.mint()

    def attributes(self) -> dict[str, str | int | bool]:
        stamped: dict[str, str | int | bool] = {"rasm.trace_id": self.trace_id.hex(), "rasm.trace.remote": self.remote}
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
    classification: Classification = Classification.INTERNAL
    causal: Option[CausalFrame] = Nothing

    @classmethod
    def admit(
        cls,
        profile: RuntimeProfile,
        *,
        axes: ConsumptionProfile | None = None,
        killswitches: frozenset[Killswitch] = frozenset(),
        deadline: Deadline | None = None,
        classification: Classification = Classification.INTERNAL,
        causal: Option[CausalFrame] = Nothing,
        carrier: Mapping[str, str] | None = None,
    ) -> Self:
        return cls(
            profile=profile,
            axes=Option.of_optional(axes).map(ConsumptionProfile.seated),
            killswitches=killswitches,
            correlation=Correlation.seed(Option.of_optional(carrier)),
            deadline=Option.of_optional(deadline),
            classification=classification,
            causal=causal,
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

    def attribute(self) -> dict[str, str | int | bool]:
        shape = self.shape
        base: dict[str, str | int | bool] = self.correlation.attributes() | {
            "rasm.profile": self.profile.value,
            "rasm.classification": self.classification.value,
            "rasm.host.kind": shape.host_key,
            "rasm.deploy.tenancy": shape.tenancy.value,
            "rasm.deploy.topology": shape.topology.value,
            "rasm.deploy.lifecycle": shape.lifecycle.value,
            "rasm.deploy.isolation": shape.isolation.value,
        }
        return self.causal.map(lambda frame: base | frame.attributes("packed")).default_value(base)
```

## [03]-[BACKEND_CONTRACT]

- Owner: `BackendGeneration` is the one polymorphic entry over both directions — `BackendGeneration.compose` mints this branch's contribution from its own store artifacts, `BackendGeneration.admit` proves a contract set against provider observations, and `BackendGeneration.merge` folds branch contributions into the deployment unit. All three land on `_settled`, so no local mint or peer document skips collision, ordering, and dependency proof. `RecoveryObjective` arrives declared as an `admit` parameter off the caller's resolved `ProfilePolicy.recovery`, so deployment shape reaches this owner as data and no `RuntimeContext` import inverts the strata.
- Cases: each local store artifact — generation script, journal DDL, embedded-store ensure, object-plane bucket — lands as one `ArtifactSource` row carrying key, role, bytes, providers, and dependencies; Python composes from its own artifacts alone.
- Law: generated `parity.Backend`, `Artifact`, and `Capability` messages are the contract vocabulary; `from_json()` and `to_json()` own ProtoJSON while `protovalidate` evaluates the descriptor constraints at both directions, so no branch-local schema, codec, or validator can diverge from the corpus descriptor.
- Law: `BACKEND_DOCUMENT_CEILING` rejects transported ProtoJSON before generated decode and emitted ProtoJSON before publication. Its 512 KiB budget sits beneath the 1 MiB ConfigMap residence after base64 and object metadata; descriptor string, content, and repeated-field ceilings remain the constructed-message floor.
- Law: `ABSORPTION` binds each generated `FailureRank` to the missing-capability policy, while the generated `RestartClass` ordinal is its disruption order; `absorbed` and `disruption` read those owners over the one `_lacking` projection.
- Law: the shared seed-zero `CanonicalWriter` mints generation over known semantic fields, never serialized protobuf or ProtoJSON: contract string; counted artifacts as key, role ordinal, framed content, counted provider ordinals, counted dependency strings; counted capabilities as their six generated fields in tag order. The schema is map-free and float-free, and unknown protobuf residue enters no field call, so parser retention, serialization order, and NaN spelling cannot fork identity. `Digest128` keeps it distinct from free text.
- Law: artifact key ordinal IS the whole wire order, so `_claimed` sorts by key alone and dependency depth never re-ranks the stream into a second generation over one artifact set.
- Law: `_closed` proves the `depends_on` graph closed and acyclic at the funnel every path reaches — dangling keys report before cycles because the sorter seats an unknown predecessor as a leaf, and `graphlib`'s `CycleError` names the ring in order — so a sort a path can skip never carries the proof and no chain depth reaches a recursion ceiling.
- Law: contributions union by key under that same order and refuse any key two claimants spell differently, artifact and capability rows alike, on the WHOLE row rather than the content cell — first-wins and last-wins each mint a generation no claimant composed; each contribution decodes on its own rail under `ACCUMULATE`, so a malformed claimant names itself by ordinal.
- Law: one local composition rejects every repeated source key before merge settlement, including byte-identical rows; identical deduplication exists only across independently admitted branch contributions, where the same whole-row claim is convergence rather than a duplicated local declaration.
- Law: `_FACTS` rows prove corpus, generation, key-set, derived-required, realization, and recovery invariants under `Disposition.ACCUMULATE`, so a refusal reports every failed invariant with its reason and the exact subjects that failed it.
- Law: contract identity and data recency are two proofs on one verdict, never two generations — `generation-drift` proves the store carries the composed contract off the existing digest, and `recovery-window-exceeded` proves the data behind it is recent enough for the window the deployment declared. `RecoveryWindow` derives from the observation's own stamps so no provider hands in a lag it computed against a clock this owner never saw — a frontier stamped after the reading is skew and drops to unmeasured — and its two halves absorb absence oppositely: an unmeasured `rpo` REFUSES, because a restore admitted with no recency evidence grades a window nobody took, while an absent `rto` passes, a store that never restored owing no bounce time.
- Packages: `protobuf-py`, `protovalidate`, `msgspec`, `expression`, the stdlib `graphlib` dependency sorter, and the shared runtime `CanonicalWriter`/fault rail.
- Law: the contract funnel keeps THREE rostered anchors and one fold — `BACKEND_MINT`, `BACKEND_MERGE`, and `BACKEND_CONTRACT` are the coordinates the free `subject` string used to thread, `_refused` raises through whichever the caller passed, and the per-claimant decode keeps its ORDINAL as a NAMED slot on `BACKEND_CLAIMANT` because a malformed claimant an operator must bisect for is exactly what a shared subject erases.
- Growth: a contract field changes the corpus message and regenerated class; a local provider adds one observation adapter; a new invariant is one `_FACTS` row; a new failure rank adds one `ABSORPTION` row; a new disruption class takes its order from the generated enum; a new recovery axis adds one `RecoveryWindow` column with its absence law and matching objective column.
- Boundary: a Python-only application composes, deploys, and admits its stores with no peer branch present; provider generation execution and journal identity stay outside this owner; recovery evidence stays observation-side and never enters `parity.Backend`, so the emitted document and every peer decode remain unchanged by a recovery reading. `ContractEvidence` holds the mutable generated message only for the admission fold; the returned generation retains one frozen `CapabilityPolicy` projection carrying exactly the behavior columns it reads. ProtoJSON comparison is semantic; byte-identity graduation belongs to a future real cross-runtime binary fixture, never a formatting assertion.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections import Counter
from collections.abc import Callable, Iterable
from enum import StrEnum
from graphlib import CycleError, TopologicalSorter
from typing import Final, NewType, Protocol, Self, assert_never

from expression import Error, Nothing, Ok, Option, Some
from expression.collections import Block, Map
from msgspec import Struct
from protovalidate import CompilationError, EvaluationError, ValidationError as ContractValidationError

from rasm.contracts.rasm.contracts.parity import parity_pb
from rasm.runtime.identity import CanonicalWriter
from rasm.runtime.faults import (
    BACKEND_CLAIMANT,
    BACKEND_CONTRACT,
    BACKEND_MERGE,
    BACKEND_MINT,
    BoundaryFault,
    Disposition,
    FaultRow,
    RuntimeLeg,
    RuntimeRail,
    boundary,
    traversed,
)


# --- [TYPES] ----------------------------------------------------------------------------

Digest128 = NewType("Digest128", str)


class Absorption(StrEnum):
    REFUSE = "refuse"
    FOLD = "fold"
    RECORD = "record"


class ContractRow(Protocol):
    key: str


# --- [CONSTANTS] ------------------------------------------------------------------------


ABSORPTION: Final[Map[parity_pb.FailureRank, Absorption]] = Map.of_seq([
    (parity_pb.FailureRank.REQUIRED, Absorption.REFUSE),
    (parity_pb.FailureRank.DEGRADABLE, Absorption.FOLD),
    (parity_pb.FailureRank.OBSERVATIONAL, Absorption.RECORD),
])

FAILURE_RANKS: Final[tuple[parity_pb.FailureRank, ...]] = tuple(
    rank
    for rank in parity_pb.FailureRank
    if rank is not parity_pb.FailureRank.UNSPECIFIED and ABSORPTION[rank]
)


# --- [MODELS] ---------------------------------------------------------------------------


class RecoveryWindow(Struct, frozen=True):
    rpo: Option[Ticks]
    rto: Option[Ticks]

    def exceeding(self, objective: RecoveryObjective, /) -> tuple[str, ...]:
        recency = self.rpo.map(lambda held: () if held <= objective.rpo else (f"rpo:{held}>{objective.rpo}",)).default_value(("rpo:unmeasured",))
        bounce = self.rto.map(lambda held: () if held <= objective.rto else (f"rto:{held}>{objective.rto}",)).default_value(())
        return recency + bounce


class BackendObservation(Struct, frozen=True):
    generation: Digest128
    capabilities: frozenset[str]
    artifacts: frozenset[str]
    observed_at: Ticks
    frontier: Option[Ticks]
    restored_in: Option[Ticks]

    @property
    def window(self) -> RecoveryWindow:
        return RecoveryWindow(
            rpo=self.frontier.map(lambda seen: Ticks(self.observed_at - seen)).filter(lambda lag: lag >= 0),
            rto=self.restored_in,
        )


class ArtifactSource(Struct, frozen=True):
    key: str
    role: parity_pb.ArtifactRole
    payload: bytes
    providers: tuple[parity_pb.Provider, ...] = ()
    depends_on: tuple[str, ...] = ()


class CapabilityPolicy(Struct, frozen=True):
    key: str
    failure_rank: parity_pb.FailureRank
    restart_class: parity_pb.RestartClass


class ContractEvidence(Struct, frozen=True):
    wire: parity_pb.Backend
    generation: Digest128
    observed: BackendObservation
    objective: RecoveryObjective

    @classmethod
    def of(cls, wire: parity_pb.Backend, observed: BackendObservation, objective: RecoveryObjective, /) -> Self:
        return cls(
            wire=wire,
            generation=_generation(wire),
            observed=observed,
            objective=objective,
        )

    @property
    def artifact_keys(self) -> tuple[str, ...]:
        return tuple(row.key for row in self.wire.artifacts)


class ContractFact(Struct, frozen=True):
    reason: str
    holds: Callable[[ContractEvidence], bool]
    subjects: Callable[[ContractEvidence], tuple[str, ...]]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _refused(at: FaultRow[RuntimeLeg], reason: str, subjects: tuple[str, ...]) -> BoundaryFault:
    return at.raised(reason, ",".join(subjects))


def _cyclic(artifacts: tuple[parity_pb.Artifact, ...], /) -> tuple[str, ...]:
    try:
        TopologicalSorter({row.key: row.depends_on for row in artifacts}).prepare()
    except CycleError as ring:
        return tuple(ring.args[1])
    return ()


def _required(rows: Iterable[parity_pb.Capability], /) -> tuple[str, ...]:
    return tuple(
        row.key for row in rows
        if row.failure_rank is parity_pb.FailureRank.REQUIRED
    )


def _ordered_artifact(row: parity_pb.Artifact, /) -> parity_pb.Artifact:
    return parity_pb.Artifact(
        key=row.key,
        role=row.role,
        content=row.content,
        providers=sorted(frozenset(row.providers)),
        depends_on=sorted(frozenset(row.depends_on)),
    )


def _normalized(wire: parity_pb.Backend, /) -> parity_pb.Backend:
    return parity_pb.Backend(
        contract=wire.contract,
        artifacts=[_ordered_artifact(row) for row in sorted(wire.artifacts, key=lambda row: row.key)],
        capabilities=sorted(wire.capabilities, key=lambda row: row.key),
    )


BACKEND_DOCUMENT_CEILING: Final[int] = 512 * 1024


def _decoded(document: bytes, /) -> parity_pb.Backend:
    if len(document) > BACKEND_DOCUMENT_CEILING:
        raise ValueError(f"backend contract exceeds {BACKEND_DOCUMENT_CEILING} bytes")
    wire = parity_pb.Backend.from_json(document.decode())
    validate(wire)
    if wire != _normalized(wire):
        raise ValueError("parity contract rows are not in canonical order")
    return wire


def _encoded(wire: parity_pb.Backend, /) -> bytes:
    validate(wire)
    document = wire.to_json().encode()
    if len(document) > BACKEND_DOCUMENT_CEILING:
        raise ValueError(f"backend contract exceeds {BACKEND_DOCUMENT_CEILING} bytes")
    return document


def _generation(wire: parity_pb.Backend, /) -> Digest128:
    writer = CanonicalWriter().string(wire.contract)
    writer.rows(
        tuple(wire.artifacts),
        lambda row, nested: nested.string(row.key)
        .ordinal(row.role.value)
        .bytes(row.content)
        .rows(tuple(row.providers), lambda provider, field: field.ordinal(provider.value))
        .rows(tuple(row.depends_on), lambda dependency, field: field.string(dependency)),
    )
    writer.rows(
        tuple(wire.capabilities),
        lambda row, nested: nested.string(row.key)
        .string(row.lane)
        .string(row.requirement)
        .string(row.requirement_value)
        .ordinal(row.failure_rank.value)
        .ordinal(row.restart_class.value),
    )
    return Digest128(writer.key("content").project("wire"))


# --- [SERVICES] -------------------------------------------------------------------------


class BackendGeneration(Struct, frozen=True):
    capabilities: tuple[CapabilityPolicy, ...]
    generation: Digest128
    observed: BackendObservation
    objective: RecoveryObjective

    @property
    def _lacking(self) -> Block[CapabilityPolicy]:
        return Block.of_seq(row for row in self.capabilities if row.key not in self.observed.capabilities)

    @property
    def absorbed(self) -> Map[parity_pb.FailureRank, frozenset[str]]:
        return Map.of_seq(
            (rank, frozenset(row.key for row in self._lacking if row.failure_rank is rank))
            for rank in FAILURE_RANKS
        )

    @property
    def disruption(self) -> Option[parity_pb.RestartClass]:
        lacking = self._lacking.map(lambda row: row.restart_class)
        return lacking.try_head().map(lambda _seed: max(lacking))

    @property
    def margin(self) -> RecoveryWindow:
        window = self.observed.window
        return RecoveryWindow(
            rpo=window.rpo.map(lambda held: Ticks(self.objective.rpo - held)),
            rto=window.rto.map(lambda held: Ticks(self.objective.rto - held)),
        )

    @classmethod
    def compose(
        cls,
        contract: str,
        artifacts: Block[ArtifactSource],
        capabilities: Block[parity_pb.Capability],
        /,
    ) -> RuntimeRail[bytes]:
        """Mint this branch's contribution from its own store artifacts."""
        local_artifacts = artifacts.map(lambda row: parity_pb.Artifact(
            key=row.key,
            role=row.role,
            content=row.payload,
            providers=list(row.providers),
            depends_on=list(row.depends_on),
        ))
        return cls._exclusive(local_artifacts, BACKEND_MINT, "artifact").bind(
            lambda unique_artifacts: cls._exclusive(capabilities, BACKEND_MINT, "capability").bind(
                lambda unique_capabilities: cls._funnelled(
                    BACKEND_MINT,
                    contract,
                    unique_artifacts,
                    unique_capabilities,
                )
            )
        )

    @classmethod
    def merge(cls, contract: str, contributions: Block[bytes], /) -> RuntimeRail[bytes]:
        """Fold branch contributions into the one deployment unit, keyed and deterministic."""
        def merged(wires: Block[parity_pb.Backend]) -> RuntimeRail[bytes]:
            mismatched = tuple(sorted({wire.contract for wire in wires if wire.contract != contract}))
            return (
                Error(_refused(BACKEND_MERGE, "contract-name-drift", mismatched))
                if mismatched
                else cls._funnelled(
                    BACKEND_MERGE,
                    contract,
                    Block.of_seq(row for wire in wires for row in wire.artifacts),
                    Block.of_seq(row for wire in wires for row in wire.capabilities),
                )
            )

        return traversed(
            contributions.mapi(
                lambda index, one: boundary(
                    BACKEND_CLAIMANT,
                    lambda: _decoded(one),
                    catch=(UnicodeDecodeError, ValueError, ContractValidationError, CompilationError, EvaluationError),
                ).map_error(lambda fault: BACKEND_CLAIMANT.raised(str(index), fault.detail))
            ),
            by=Disposition.ACCUMULATE,
        ).bind(merged)

    @classmethod
    def admit(cls, document: bytes, observed: BackendObservation, objective: RecoveryObjective, /) -> RuntimeRail[Self]:
        """Prove a contract set — locally composed or merged — against realized provider observations."""
        return boundary(
            BACKEND_CONTRACT,
            lambda: _decoded(document),
            catch=(UnicodeDecodeError, ValueError, ContractValidationError, CompilationError, EvaluationError),
        ).bind(
            lambda wire: cls._settled(
                BACKEND_CONTRACT,
                wire.contract,
                Block.of_seq(wire.artifacts),
                Block.of_seq(wire.capabilities),
            ).bind(lambda settled: cls._proved(ContractEvidence.of(settled, observed, objective)))
        )

    @classmethod
    def _funnelled(
        cls,
        at: FaultRow[RuntimeLeg],
        contract: str,
        artifacts: Block[parity_pb.Artifact],
        capabilities: Block[parity_pb.Capability],
        /,
    ) -> RuntimeRail[bytes]:
        return cls._settled(at, contract, artifacts, capabilities).bind(lambda wire: cls._projected(wire, at))

    @classmethod
    def _settled(
        cls,
        at: FaultRow[RuntimeLeg],
        contract: str,
        artifacts: Block[parity_pb.Artifact],
        capabilities: Block[parity_pb.Capability],
        /,
    ) -> RuntimeRail[parity_pb.Backend]:
        ordered = artifacts.map(_ordered_artifact)

        def framed(rows: tuple[parity_pb.Artifact, ...]) -> RuntimeRail[parity_pb.Backend]:
            return cls._claimed(capabilities, at, "capability").bind(
                lambda held: Ok(parity_pb.Backend(contract=contract, artifacts=list(rows), capabilities=list(held)))
            )

        return cls._claimed(ordered, at, "artifact").bind(lambda rows: cls._closed(rows, at)).bind(framed)

    @staticmethod
    def _exclusive[R: ContractRow](
        rows: Block[R],
        at: FaultRow[RuntimeLeg],
        family: str,
        /,
    ) -> RuntimeRail[Block[R]]:
        duplicated = tuple(sorted(key for key, count in Counter(row.key for row in rows).items() if count > 1))
        return Error(_refused(at, f"{family}-duplicate-source", duplicated)) if duplicated else Ok(rows)

    @staticmethod
    def _claimed[R: ContractRow](rows: Block[R], at: FaultRow[RuntimeLeg], family: str, /) -> RuntimeRail[tuple[R, ...]]:
        claimed = {row.key: row for row in rows}
        collided = tuple(sorted({row.key for row in rows if claimed[row.key] != row}))
        return (
            Error(_refused(at, f"{family}-collision", collided))
            if collided
            else Ok(tuple(claimed[key] for key in sorted(claimed)))
        )

    @staticmethod
    def _closed(
        artifacts: tuple[parity_pb.Artifact, ...],
        at: FaultRow[RuntimeLeg],
        /,
    ) -> RuntimeRail[tuple[parity_pb.Artifact, ...]]:
        keys = frozenset(row.key for row in artifacts)
        dangling = tuple(sorted({f"{row.key}->{dep}" for row in artifacts for dep in row.depends_on if dep not in keys}))
        ring = () if dangling else _cyclic(artifacts)
        return (
            Error(_refused(at, "artifact-dependency-dangling", dangling))
            if dangling
            else Error(_refused(at, "artifact-dependency-cyclic", ring))
            if ring
            else Ok(artifacts)
        )

    @staticmethod
    def _projected(wire: parity_pb.Backend, at: FaultRow[RuntimeLeg], /) -> RuntimeRail[bytes]:
        return boundary(
            at,
            lambda: _encoded(wire),
            catch=(TypeError, ValueError, OverflowError, ContractValidationError, CompilationError, EvaluationError),
        )

    @classmethod
    def _proved(cls, evidence: ContractEvidence, /) -> RuntimeRail[Self]:
        return traversed(
            _FACTS.map(
                lambda fact: Ok(fact.reason)
                if fact.holds(evidence)
                else Error(_refused(BACKEND_CONTRACT, fact.reason, fact.subjects(evidence)))
            ),
            by=Disposition.ACCUMULATE,
        ).map(lambda _: cls(
            capabilities=tuple(
                CapabilityPolicy(
                    key=row.key,
                    failure_rank=row.failure_rank,
                    restart_class=row.restart_class,
                )
                for row in evidence.wire.capabilities
            ),
            generation=evidence.generation,
            observed=evidence.observed,
            objective=evidence.objective,
        ))


# --- [TABLES] ---------------------------------------------------------------------------

_FACTS: Final[Block[ContractFact]] = Block.of_seq([
    ContractFact(
        "generation-drift",
        lambda e: e.generation == e.observed.generation,
        lambda e: (e.generation, e.observed.generation),
    ),
    ContractFact(
        "artifact-unrealized",
        lambda e: frozenset(e.artifact_keys) <= e.observed.artifacts,
        lambda e: tuple(sorted(frozenset(e.artifact_keys) - e.observed.artifacts)),
    ),
    ContractFact(
        "capability-unrealized",
        lambda e: frozenset(_required(e.wire.capabilities)) <= e.observed.capabilities,
        lambda e: tuple(sorted(frozenset(_required(e.wire.capabilities)) - e.observed.capabilities)),
    ),
    ContractFact(
        "recovery-window-exceeded",
        lambda e: not e.observed.window.exceeding(e.objective),
        lambda e: e.observed.window.exceeding(e.objective),
    ),
])
```

## [04]-[SETTINGS]

- Owner: `SettingsAdmission` admits init mapping, environment, dotenv, and the OS secret tree in the DEFAULT `pydantic-settings` precedence, with the one substitution that order alone cannot express: `settings_customise_sources` wraps the flat `file_secret_settings` rung in `NestedSecretsSettingsSource(..., secrets_nested_subdir=True)`, so a flat `<mount>/<field>` file and a subdirectory-per-model tree both resolve off the one `_SECRETS_MOUNT` without a single rung moving. `mounted` is its construction entry, threading one mount onto both the `secrets_dir` source and the `secrets_mount` field, since `secrets_dir` resolves ahead of every validator and no validator reaches it to refuse a split. Every root is typed against the `pydantic` catalogue, never bare `str`, and every environment-derived path — the `known_hosts` home default included — resolves inside this construction rather than at the leg that reads it. `BasicCredential` is deliberately not named `Credential`: the serve-side `CredentialPolicy` union is `transport/serve#SERVE`'s decode of the peer-minted wire axis, a different concept under a different name.
- Entry: `SecretBoundary.resolve` is the one credential reader, parameterized over output shape by a keyword-only `@overload` pair — admitting a new consumer shape is one `SecretShape` member, one overload arm, and one fold-tail arm, never a parallel resolver. `SecretRequest.admitted` gates the coordinate before any rung fires, so a name outside the shared alphabet refuses as a `config` fault rather than reaching a provider. Absence folds to `Ok(Nothing)` rather than a fault: a missing credential is a wire fact the outbound leg routes. `known_hosts` returns the admission-loaded `SSHKnownHosts` the `transport/roots#RESOURCE` `ssh` leg binds — host-key verification is admission-supplied, never the disabled-verification `known_hosts=None` the connection law forbids.
- Auto: the ladder fold drops every row the carried `FeatureGate` refuses, so a session that cannot answer a keychain prompt never triggers one and a killswitched deployment dials no vault. `GoogleSecretManagerSettingsSource` and `AzureKeyVaultSettingsSource` are the branch-catalogued declared-field twins — deployment-added rows on this same source chain, serving model fields the chain resolves at construction where `SecretBoundary` serves the per-service `(service, username)` credentials no construction-time source can address.
- Growth: a new setting is one typed field on the model; a new source origin is one row on the `settings_customise_sources` tuple; a new secret BACKEND is one `CloudVault` arm, one `VaultTag` member, one `read` case, and one `vault()` arm with zero ladder edits, while a new resolution TIER is one `SecretTier` member with one `SECRET_LADDER` row carrying its `Option[Feature]` gate and one `_read` arm; a tier needing a retry policy the others do not share re-lands `TierRow`'s retry column with two distinct values, never one repeated; a new output shape one `SecretShape` member, one overload, and one fold-tail arm.
- Law: `SecretTier` names the rung and `CloudVault` names the backend serving it — the deployment supplies one `providers`-axis row, so the resolver holds no provider default and an unnamed backend folds the rung out instead of assuming one. Each backend owns its own read, so the arm carrying a coordinate set is the arm dialling it and a probe builds exactly one client, INSIDE the read and released on the way out: a memoized client binds credential-carrying state to no composition, which is the handle the branch's per-composition custody law forbids, and boot-only resolution makes the per-read construction free.
- Law: the ladder is ONE synchronous union crossing on `anyio.to_thread.run_sync` under `_PROBE_BAND`, and GCP and Azure publish an `aio` twin where `hvac` publishes none.
- Law: one `SecretRequest.name` spelling serves every backend under the narrowest alphabet any of them admits and refuses outside it, and each arm's miss-vs-fault split is its provider's own 404 case — so a walk to the next rung and a refusal never collapse into one answer, and a transport digest mismatch names itself as `IntegrityError` rather than as the bare `OSError` a mount read shares.
- Boundary: no code here reads `os.environ` after admission, and no admitted client is left to reach one for a credential — GCP and Azure resolve ambient workload identity inside their own construction as their catalogues rule, while the Vault token is admitted material precisely because `hvac.Client` at `token=None` falls back to `VAULT_TOKEN` and then `~/.vault-token`. Keystore and cloud tiers read credentials for the OUTBOUND transport legs only — the companion UDS serve leg reads no keyring, peer identity being the kernel accept-time credential (`transport/serve#SERVE`).

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import re
import threading
from collections.abc import Callable, Iterator
from contextlib import ExitStack, closing, contextmanager
from enum import StrEnum
from functools import partial
from pathlib import Path
from typing import Annotated, Final, Literal, Self, assert_never, overload

import anyio
import asyncssh
import keyring
import keyring.credentials
import keyring.errors
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Meta, Struct
from pydantic import AnyUrl, DirectoryPath, Field, HttpUrl, SecretStr
from pydantic_settings import BaseSettings, NestedSecretsSettingsSource, PydanticBaseSettingsSource, SettingsConfigDict

from rasm.runtime.faults import (
    ADMISSION_HOSTS,
    SECRET_NAME,
    SECRET_READ,
    TENANCY_GRADE,
    TENANCY_ISSUER,
    TENANCY_SCOPE,
    TENANCY_TENANT,
    RuntimeRail,
    boundary,
)
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.resilience import RetryClass, guarded

lazy import google_crc32c
lazy import hvac
lazy from azure.core.exceptions import ResourceNotFoundError
lazy from azure.identity import DefaultAzureCredential
lazy from azure.keyvault.secrets import SecretClient
lazy from google.api_core.exceptions import NotFound
lazy from google.cloud.secretmanager import SecretManagerServiceClient
lazy from hvac.exceptions import InvalidPath


# --- [TYPES] ----------------------------------------------------------------------------


class SecretShape(StrEnum):
    TOKEN = "token"
    CREDENTIAL = "credential"


class SecretTier(StrEnum):
    KEYSTORE = "keystore"
    CLOUD = "cloud"
    FILE = "file"


type VaultTag = Literal["gcp", "vault", "azure"]

# --- [CONSTANTS] ------------------------------------------------------------------------

_SECRETS_MOUNT: Final[str] = "/run/secrets"

_VAULT_FIELD: Final[str] = "value"

_SECRET_ALPHABET: Final[re.Pattern[str]] = re.compile(r"[A-Za-z0-9-]+")

_PROBE_BAND: Final[anyio.CapacityLimiter] = anyio.CapacityLimiter(4)
_SECRET_GATE: Final[threading.Lock] = threading.Lock()
_SECRET_BANDS: Final[dict[ScopeKey, tuple[ExitStack, list[object]]]] = {}


def _secret_occupancy() -> int:
    return _PROBE_BAND.borrowed_tokens


@contextmanager
def _secret_band(scope: ScopeKey) -> Iterator[None]:
    token = object()
    with _SECRET_GATE:
        stack, holders = _SECRET_BANDS.setdefault(scope, (ExitStack(), []))
        holders.append(token)
        if len(holders) == 1:
            stack.enter_context(Metrics.occupied(_secret_occupancy, band="secret", scope=scope))
    try:
        yield
    finally:
        with _SECRET_GATE:
            stack, holders = _SECRET_BANDS[scope]
            holders.remove(token)
            if not holders:
                stack.close()
                del _SECRET_BANDS[scope]

# --- [MODELS] ---------------------------------------------------------------------------


class BasicCredential(Struct, frozen=True):
    username: str
    secret: SecretStr


class SecretRequest(Struct, frozen=True, gc=False):
    service: str
    username: str | None

    @property
    def name(self) -> str:
        return Option.of_optional(self.username).map(lambda held: f"{self.service}-{held}").default_value(self.service)

    @property
    def user(self) -> str:
        return Option.of_optional(self.username).default_value(self.service)

    @classmethod
    def admitted(cls, service: str, username: str | None, /) -> RuntimeRail[Self]:
        request = cls(service=service, username=username)
        return (
            Ok(request)
            if _SECRET_ALPHABET.fullmatch(request.name)
            else Error(SECRET_NAME.raised(request.name, _SECRET_ALPHABET.pattern))
        )

    def paired(self, raw: str, /) -> Option[BasicCredential]:
        return Some(BasicCredential(self.user, SecretStr(raw.strip())))

    def stored(self, found: keyring.credentials.Credential, /) -> BasicCredential:
        match found:
            case keyring.credentials.AnonymousCredential():
                return BasicCredential(self.user, SecretStr(found.password))
            case _:
                return BasicCredential(found.username, SecretStr(found.password))


@tagged_union(frozen=True)
class CloudVault:
    tag: VaultTag = tag()
    gcp: tuple[str, str] = case()
    vault: tuple[str, str, str | None, SecretStr] = case()
    azure: tuple[str, str] = case()

    def read(self, request: SecretRequest, /) -> Option[BasicCredential]:
        match self:
            case CloudVault(tag="gcp", gcp=(project, prefix)):
                with SecretManagerServiceClient() as client:
                    name = client.secret_version_path(project, f"{prefix}-{request.name}", "latest")
                    try:
                        payload = client.access_secret_version(name=name).payload
                    except NotFound:
                        return Nothing
                    if google_crc32c.value(payload.data) != payload.data_crc32c:
                        raise IntegrityError(f"secret-crc32c:{name}")
                    return request.paired(payload.data.decode("utf-8"))
            case CloudVault(tag="vault", vault=(url, mount, namespace, token)):
                client = hvac.Client(url=url, token=token.get_secret_value(), namespace=namespace)
                with closing(client.adapter):
                    try:
                        body = client.secrets.kv.v2.read_secret_version(path=request.name, mount_point=mount)
                    except InvalidPath:
                        return Nothing
                    return Option.of_optional(body["data"]["data"].get(_VAULT_FIELD)).bind(request.paired)
            case CloudVault(tag="azure", azure=(vault_url, prefix)):
                with DefaultAzureCredential() as credential, SecretClient(vault_url, credential) as client:
                    try:
                        found = client.get_secret(f"{prefix}-{request.name}")
                    except ResourceNotFoundError:
                        return Nothing
                    return Option.of_optional(found.value).bind(request.paired)
            case _ as unreachable:
                assert_never(unreachable)


class SettingsAdmission(BaseSettings):
    model_config = SettingsConfigDict(frozen=True, extra="forbid", env_prefix="RASM_PY_", env_nested_delimiter="__", secrets_dir=_SECRETS_MOUNT)

    scratch_root: DirectoryPath
    object_store_root: AnyUrl | None = None
    otel_endpoint: HttpUrl | None = None
    pyroscope_endpoint: HttpUrl | None = None
    known_hosts: Path = Field(default_factory=lambda: Path.home() / ".ssh" / "known_hosts")
    secrets_mount: Path = Path(_SECRETS_MOUNT)
    vault_backend: VaultTag | None = None
    vault_endpoint: str | None = None
    vault_prefix: str = "rasm"
    vault_mount: str = "secret"
    vault_namespace: str | None = None
    vault_token: SecretStr | None = None

    @classmethod
    def settings_customise_sources(
        cls,
        settings_cls: type[BaseSettings],
        init_settings: PydanticBaseSettingsSource,
        env_settings: PydanticBaseSettingsSource,
        dotenv_settings: PydanticBaseSettingsSource,
        file_secret_settings: PydanticBaseSettingsSource,
    ) -> tuple[PydanticBaseSettingsSource, ...]:
        return (init_settings, env_settings, dotenv_settings, NestedSecretsSettingsSource(file_secret_settings, secrets_nested_subdir=True))

    @classmethod
    def mounted(cls, secrets_mount: Path | None = None, /, **overrides: object) -> Self:
        root = secrets_mount if secrets_mount is not None else Path(_SECRETS_MOUNT)
        return cls(secrets_mount=root, _secrets_dir=root, **overrides)

    def vault(self) -> Option[CloudVault]:
        match (self.vault_backend, self.vault_endpoint, self.vault_token):
            case ("gcp", str() as endpoint, _):
                return Some(CloudVault(gcp=(endpoint, self.vault_prefix)))
            case ("azure", str() as endpoint, _):
                return Some(CloudVault(azure=(endpoint, self.vault_prefix)))
            case ("vault", str() as endpoint, SecretStr() as token):
                return Some(CloudVault(vault=(endpoint, self.vault_mount, self.vault_namespace, token)))
            case _:
                return Nothing


class TierRow(Struct, frozen=True):
    tier: SecretTier
    gate: Option[Feature]


# --- [ERRORS] ---------------------------------------------------------------------------


class IntegrityError(OSError):
    """Secret payload whose transport digest disagrees with the octets received."""


# --- [SERVICES] -------------------------------------------------------------------------


class SecretBoundary(Struct, frozen=True):
    settings: SettingsAdmission
    gate: FeatureGate
    scope: ScopeKey = DEFAULT_SCOPE

    @overload
    async def resolve(self, service: str, username: str | None = ..., *, shape: Literal[SecretShape.TOKEN] = ...) -> RuntimeRail[Option[SecretStr]]: ...
    @overload
    async def resolve(
        self, service: str, username: str | None = ..., *, shape: Literal[SecretShape.CREDENTIAL]
    ) -> RuntimeRail[Option[BasicCredential]]: ...
    async def resolve(
        self, service: str, username: str | None = None, *, shape: SecretShape = SecretShape.TOKEN
    ) -> RuntimeRail[Option[SecretStr]] | RuntimeRail[Option[BasicCredential]]:
        admitted = SECRET_LADDER.filter(lambda row: row.gate.map(self.gate.admits).default_value(True))

        async def walk(rows: Block[TierRow], request: SecretRequest) -> RuntimeRail[Option[BasicCredential]]:
            match rows.try_head():
                case Option(tag="some", some=head):
                    match await self._probe(head, request):
                        case Result(tag="error") as faulted:
                            return faulted
                        case Result(tag="ok", ok=Option(tag="some")) as hit:
                            return hit
                        case _:
                            return await walk(rows.tail(), request)
                case _:
                    return Ok(Nothing)

        match SecretRequest.admitted(service, username):
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=request):
                with _secret_band(self.scope):
                    resolved = await walk(admitted, request)
                return resolved if shape is SecretShape.CREDENTIAL else resolved.map(lambda held: held.map(lambda pair: pair.secret))
            case _ as unreachable:
                assert_never(unreachable)

    async def _probe(self, row: TierRow, request: SecretRequest, /) -> RuntimeRail[Option[BasicCredential]]:
        match self._read(row, request):
            case Option(tag="some", some=read):
                return await guarded(
                    RetryClass.SECRET, anyio.to_thread.run_sync, read, at=SECRET_READ, on=Some(self._peer(row)), limiter=_PROBE_BAND
                )
            case _:
                return Ok(Nothing)

    def _peer(self, row: TierRow, /) -> str:
        match row.tier:
            case SecretTier.CLOUD:
                return self.settings.vault().map(lambda held: held.tag).default_value(row.tier.value)
            case _:
                return row.tier.value

    def _read(self, row: TierRow, request: SecretRequest, /) -> Option[Callable[[], Option[BasicCredential]]]:
        def keystore() -> Option[BasicCredential]:
            try:
                found = keyring.get_credential(request.service, request.username)
            except keyring.errors.NoKeyringError:
                return Nothing
            return Option.of_optional(found).map(request.stored)

        def mounted() -> Option[BasicCredential]:
            try:
                raw = (self.settings.secrets_mount / request.name).read_text(encoding="utf-8")
            except (FileNotFoundError, NotADirectoryError):
                return Nothing
            return request.paired(raw)

        match row.tier:
            case SecretTier.KEYSTORE:
                return Some(keystore)
            case SecretTier.CLOUD:
                return self.settings.vault().map(lambda named: partial(named.read, request))
            case SecretTier.FILE:
                return Some(mounted)
            case _ as unreachable:
                assert_never(unreachable)

    def known_hosts(self) -> RuntimeRail[asyncssh.SSHKnownHosts]:
        return boundary(ADMISSION_HOSTS, lambda: asyncssh.read_known_hosts(str(self.settings.known_hosts)), catch=OSError)


# --- [TABLES] ---------------------------------------------------------------------------

SECRET_LADDER: Final[Block[TierRow]] = Block.of_seq([
    TierRow(SecretTier.KEYSTORE, Some(Feature.KEYSTORE_PROBE)),
    TierRow(SecretTier.CLOUD, Some(Feature.SECRET_MANAGER)),
    TierRow(SecretTier.FILE, Nothing),
])
```

## [05]-[TENANCY]

- Owner: `Trust` is the issuer table a composition binds and `TenantAdoption` the one ingress gate every producer claim crosses. `TrustRow` answers what ONE issuer may assert — the tenants its authenticated principals may occupy and the sensitivity ceiling a fact from it may carry — so trust is DATA bound at the composition root rather than a predicate scattered across every decode site. `Claim` is the untrusted source and grade decoded off the event; `PrincipalScope` is the authenticated principal and selected tenant injected by the application-owned protocol binder; `TenantAdoption` is the verified answer a routing decision reads.
- Cases: `Tenancy` already closes the deployment axis and each value answers the gate differently. `NONE` requires an empty authenticated grant set and no selected tenant. `SINGLE` requires exactly one grant and that same selected tenant. `MULTI` requires the selected tenant to belong to the authenticated principal's nonempty grant set. Both tenantful arms then admit the selection only where the source issuer's row carries it; the event never invents that coordinate and the generic lane never derives it from a credential.
- Law: ingress ADMITS and NEVER inherits. A broker credential proves only the connection unless the application-owned protocol binder authenticates a principal and emits its `PrincipalScope` beside the delivery. That typed scope is the sole tenancy authority the generic lane may compare; ambient context, an event extension, a username parse, and a broker address are all refused substitutes. A refusing leg clears the delivery rather than falling back to an ambient frame.
- Law: `source` is the producer CLAIM verified before any routing decision reads it. Matching a filter on an unverified `source` routes on a value its producer chose freely, so a peer naming another capability's source reaches that capability's subscriptions. Verification is issuer-prefix, resolved LONGEST-first over resolved segments rather than by string prefix, because `rasm/element-evil` carries the `rasm/element` string prefix and none of its segments. Principals remain typed payload facts and authenticate through their owning application boundary, never a generic CloudEvents extension.
- Law: `ceiling` is an UPPER bound and never a stamp. Issuers claim a grade at or below their row's ceiling and anything above it refuses — never downgrades — because silently lowering a `restricted` fact to `internal` publishes it onto every binding the lower grade admits, which is exactly the exfiltration path the classification gate exists to close. That comparison reads `Classification.rank` and never the members themselves: a `StrEnum` orders LEXICALLY, so `internal` sorts below `public` and a bare `>` admits precisely the crossing this bound forbids.
- Law: an unbound `Trust` is CLOSED, not open. `Trust.closed()` trusts no issuer, so a composition that binds no table refuses every claim rather than admitting every one, and the failure mode of forgetting the binding is a refusal an operator sees immediately rather than an authorization hole nobody observes.
- Entry: `TenantAdoption.of(context, trust, scope, claim)` is the one gate, railed, answering the adopted tenant, authenticated principal, and row that admitted both — the composition-bound table and protocol-authenticated scope cross as arguments rather than ambient state a decode site resolves; `Trust.issuer(source)` is the longest-prefix resolve every other read composes. None takes a knob, and no consumer re-spells the fold.
- Auto: a refusal rides the fault union's `config` case keyed `tenancy.<axis>`, so the boot fold and the ingress fold return through the one rail every other step returns through, and a refused claim sheds exactly the fact carrying it rather than the connection that delivered it.
- Growth: a new trusted issuer is one `TRUST_ROWS`-shaped entry the composition binds; a new event claim is one `Claim` field with its `TrustRow` column; a new authenticated principal dimension is one `PrincipalScope` field produced by every protocol binder; a new tenancy shape is one `Tenancy` member with its arm on the one adoption fold, the standing `assert_never` breaking every arm that lacks it.
- Boundary: claim verification and tenant adoption only. Mints no `Tenant` — `evidence/clock#CLOCK` owns that newtype and its root — no credential, no session, no row-level predicate, and no transport identity. Rejected: a tenant inferred from a raw transport credential; a tenant encoded as a generic event extension; a claim verified at a routing site rather than at ingress; a substring issuer match; a ceiling that downgrades; an open default trust table.

```python signature
# --- [MODELS] ---------------------------------------------------------------------------


class TrustRow(Struct, frozen=True, gc=False):
    issuer: str
    tenants: frozenset[str]
    ceiling: Classification


class Trust(Struct, frozen=True, gc=False):
    rows: Map[str, TrustRow] = Map.empty()

    @classmethod
    def closed(cls) -> Self:
        return cls()

    def issuer(self, source: str, /) -> Option[TrustRow]:
        segments = source.strip("/").split("/")
        candidates = ("/".join(segments[:depth]) for depth in range(len(segments), 0, -1))
        return Block.of_seq(candidates).choose(self.rows.try_find).try_head()


class PrincipalScope(Struct, frozen=True, gc=False):
    principal: Annotated[str, Meta(min_length=1)]
    grants: frozenset[Annotated[str, Meta(min_length=1)]]
    tenant: Option[Annotated[str, Meta(min_length=1)]]


class Claim(Struct, frozen=True, gc=False):
    source: str
    grade: Classification


class TenantAdoption(Struct, frozen=True, gc=False):
    admitted: Option[Tenant]
    principal: str
    row: TrustRow

    @classmethod
    def of(
        cls, context: RuntimeContext, trust: Trust, scope: PrincipalScope, claim: Claim, /
    ) -> RuntimeRail[Self]:
        match trust.issuer(claim.source):
            case Option(tag="none"):
                return Error(TENANCY_ISSUER.raised(claim.source))
            case Option(some=row) if claim.grade.rank > row.ceiling.rank:
                return Error(TENANCY_GRADE.raised(claim.grade.value, row.issuer))
            case Option(some=row):
                return cls._partitioned(context.shape.tenancy, row, scope).map(
                    lambda held: cls(admitted=held, principal=scope.principal, row=row)
                )

    @staticmethod
    def _partitioned(
        axis: Tenancy, row: TrustRow, scope: PrincipalScope, /
    ) -> RuntimeRail[Option[Tenant]]:
        match (axis, scope.tenant):
            case (Tenancy.NONE, Option(tag="none")) if not scope.grants:
                return Ok(Nothing)
            case (Tenancy.NONE, Option(tag="none")) | (Tenancy.NONE, Option(tag="some")):
                return Error(TENANCY_SCOPE.raised(axis.value, scope.principal))
            case (Tenancy.SINGLE | Tenancy.MULTI, Option(tag="none")):
                return Error(TENANCY_SCOPE.raised(axis.value, scope.principal))
            case (Tenancy.SINGLE, Option(some=held)) if scope.grants == frozenset({held}) and held in row.tenants:
                return Ok(Some(Tenant(held)))
            case (Tenancy.SINGLE, Option(some=held)) if scope.grants != frozenset({held}):
                return Error(TENANCY_SCOPE.raised(axis.value, scope.principal))
            case (Tenancy.SINGLE, Option(some=held)):
                return Error(TENANCY_TENANT.raised(row.issuer, held))
            case (Tenancy.MULTI, Option(some=held)) if held not in scope.grants:
                return Error(TENANCY_SCOPE.raised(axis.value, scope.principal))
            case (Tenancy.MULTI, Option(some=held)) if held in row.tenants:
                return Ok(Some(Tenant(held)))
            case (Tenancy.MULTI, Option(some=held)):
                return Error(TENANCY_TENANT.raised(row.issuer, held))
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
