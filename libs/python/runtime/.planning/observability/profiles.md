# [PY_RUNTIME_PROFILES]

`Profiles` pushes continuous CPU profiles beside the OTLP rails and links them to traces: the pyroscope push agent streams samples to the profile store, and `PyroscopeSpanProcessor` stamps every root span with `pyroscope.profile.id` so a trace click-through lands on its flame graph. This page also owns the benchmark-receipt family — the macro-latency and throughput evidence the request-duration histogram cannot carry — and the offline-job envelope that makes a short-lived process export completely before it exits.

Install custody is two-tier — per-composition `ProfilesReceipt`s key by the receipts-owned `ScopeKey` (a same-scope re-install returns `REENTRANT`, a later composition `ADOPTED`) while the imported `latched` guards the one process push agent, `pyroscope.configure` being process-global — and rides the `execution/admission#CONTEXT` `emit_otel` gate, sequenced after `observability/telemetry#TELEMETRY` so the span processor attaches to the registered SDK provider. `SCHEMA_URL`, `SignalProfile`, and the flush-then-shutdown drain arrive settled from the telemetry owner; `Metrics.record` from `observability/metrics#METRIC`; `Receipt`/`Signals` from `observability/receipts#RECEIPT`. Job identity is hand-built — no detector carries job semantics — and the job lane's delta-temporality preference rides one launcher-environment knob.

## [01]-[INDEX]

- [01]-[PROFILES]: the scope-keyed, profile-gated pyroscope push install and the span-profile link.
- [02]-[BENCH]: the benchmark-receipt family and its instrument projection.
- [03]-[JOB]: the offline-job envelope — hand-built resource, high-interval safety net, and the flush-then-shutdown boundary.

## [02]-[PROFILES]

- Owner: `Profiles.install` configures the push agent once — application name from the faults-owned `SCOPES[Scope.SERVICE]` row, server address, static tags, and tenant caller-supplied — and attaches `PyroscopeSpanProcessor` to the registered SDK `TracerProvider`, so every root span carries `pyroscope.profile.id` and the profiler's thread tags carry `span_id`/`span_name`/`trace_id` for the reverse jump. `tenant_id` threads the folder's first-class tenant dimension onto the push, so a multi-tenant profile store slices flame graphs by the same org routing every measurement already carries; `Profiles.phase` scopes sample tags to a bounded window — a recipe stage, a worker kernel window — so a flame graph slices by phase while static dimensions stay install-time `tags=`. Worker floors attach through the workers-owned boot capture: `install` runs in every pool initializer with the `worker.kind` install tag and the parent-captured tenant, the kernel-subject `phase` window rides `traced_kernel`, and the atexit-registered `shutdown` stops the push at worker retirement — so flames come from the process that burns the cycles and a slow offload span clicks through to its worker's graph.
- Entry: a silent profile caches a `SILENT` receipt per scope and starts no agent, so PACKAGE/TEST processes pay nothing; a same-scope re-install returns its cached receipt stamped `REENTRANT` off the `_receipts` map fold, and a later composition arriving after the push agent exists receives `ADOPTED` through the `latched` reentrant closure — the agent never doubles. `PyroscopeSpanProcessor` attaches only when the global resolves to the SDK `TracerProvider` the telemetry install registered — the API no-op provider matches no arm and the receipt records `linked=False`. `shutdown` is scope-keyed custody: only the scope holding the `INSTALLED` receipt stops the push thread and clears every scope receipt; a `SILENT`/`ADOPTED` scope retires its own receipt alone.
- Auto: `oncpu=True` and `gil_only=False` keep samples on-CPU across Python and native kernels that release the GIL, while idle waits fall out; `shutdown` stops the push thread through `pyroscope.shutdown()` on the drain fold beside the telemetry providers.
- Packages: `pyroscope-otel` (`PyroscopeSpanProcessor` and its bundled push agent `pyroscope.configure`/`shutdown`/`tag_wrapper`/`add_thread_tag`), `opentelemetry-sdk` (the `TracerProvider` match arm — composition-root altitude), runtime (`latched`, `SCOPES`, admission gate).
- Growth: a new static profile dimension is one entry in the caller's `tags` mapping; a bounded-window dimension one entry in a `Profiles.phase` mapping; a new worker-floor dimension is one entry in the workers boot-capture tags; a new agent knob is one `configure` keyword threaded through `install`; a new composition is one `ScopeKey` value threaded through the `scope` keyword.
- Boundary: profiles egress through the pyroscope push wire alone — the OTLP trio stays the telemetry owner's, and no library module below the composition root imports this page.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Mapping
from contextlib import AbstractContextManager, nullcontext
from enum import StrEnum
from statistics import quantiles
from threading import RLock
from time import perf_counter
from typing import ClassVar, Final
from uuid import uuid4

import pyroscope
from expression import Option
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import replace
from opentelemetry import trace
from opentelemetry.exporter.otlp.proto.http import Compression
from opentelemetry.sdk.resources import SERVICE_INSTANCE_ID, SERVICE_NAME, SERVICE_NAMESPACE, Resource
from opentelemetry.sdk.trace import TracerProvider
from pyroscope.otel import PyroscopeSpanProcessor

from rasm.runtime.admission import PROFILE_POLICY, RuntimeProfile
from rasm.runtime.faults import SCOPES, RuntimeRail, Scope, boundary, latched
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey
from rasm.runtime.telemetry import NAMESPACE, SCHEMA_URL, SignalProfile, Telemetry

# --- [TYPES] ----------------------------------------------------------------------------


# INSTALLED started the process push agent; SILENT kept the gate closed; REENTRANT is a same-scope re-install;
# ADOPTED is a later composition riding the standing agent with its own receipt.
class ProfilesOutcome(StrEnum):
    INSTALLED = "installed"
    SILENT = "silent"
    REENTRANT = "reentrant"
    ADOPTED = "adopted"


# --- [MODELS] ---------------------------------------------------------------------------


class ProfilesReceipt(Struct, frozen=True):
    outcome: ProfilesOutcome
    application: str
    endpoint: str
    linked: bool  # True when PyroscopeSpanProcessor attached to the registered SDK provider
    tenant: str | None = None  # org routing the push carried; None on a single-tenant store


# --- [SERVICES] -------------------------------------------------------------------------


class Profiles:
    # two-tier custody: per-composition receipts key by ScopeKey; `latched` guards the one process push agent,
    # pyroscope.configure being process-global — the first emitting install owns the push pipeline.
    _receipts: ClassVar[Map[ScopeKey, ProfilesReceipt]] = Map.empty()
    _process: ClassVar[ProfilesReceipt | None] = None
    _gate = RLock()

    @classmethod
    @latched(lambda: Profiles._process, lambda r: setattr(Profiles, "_process", r), lambda prior: replace(prior, outcome=ProfilesOutcome.ADOPTED))
    def _pushed(cls, endpoint: str, tags: Mapping[str, str], tenant: str | None) -> ProfilesReceipt:
        application = SCOPES[Scope.SERVICE]
        # tenant_id carries the store's org routing when a multi-tenant store fronts the push — the profile-store
        # half of the folder's tenant dimension, matching the rasm.tenant fold on every measurement.
        pyroscope.configure(application_name=application, server_address=endpoint, tags=dict(tags), tenant_id=tenant, oncpu=True, gil_only=False)
        match trace.get_tracer_provider():
            case TracerProvider() as sdk_provider:  # registered by Telemetry.install; the API no-op provider matches no arm
                sdk_provider.add_span_processor(PyroscopeSpanProcessor())
                return ProfilesReceipt(ProfilesOutcome.INSTALLED, application, endpoint, linked=True, tenant=tenant)
            case _:
                return ProfilesReceipt(ProfilesOutcome.INSTALLED, application, endpoint, linked=False, tenant=tenant)

    @classmethod
    def install(
        cls,
        profile: RuntimeProfile,
        endpoint: str,
        tags: Mapping[str, str] | None = None,
        tenant: str | None = None,
        *,
        scope: ScopeKey = DEFAULT_SCOPE,
    ) -> ProfilesReceipt:
        with cls._gate:
            match cls._receipts.try_find(scope):
                case Option(tag="some", some=prior):
                    return replace(prior, outcome=ProfilesOutcome.REENTRANT)
                case _:
                    receipt = (
                        ProfilesReceipt(ProfilesOutcome.SILENT, SCOPES[Scope.SERVICE], endpoint, linked=False, tenant=tenant)
                        if not PROFILE_POLICY[profile].emit_otel
                        else cls._pushed(endpoint, tags if tags is not None else {}, tenant)
                    )
                    cls._receipts = cls._receipts.add(scope, receipt)
                    return receipt

    @staticmethod
    def phase(tags: Mapping[str, str]) -> AbstractContextManager[None]:
        # bounded-window sample tagging: a recipe stage or worker kernel window scopes its flame samples, and the
        # wrapper restores the prior thread tags on exit — never a hand-paired add/remove_thread_tag ladder. With no
        # process agent the window is a nullcontext, so an uninstalled worker floor and a silent profile compose the
        # same call shape at zero cost.
        with Profiles._gate:
            installed = Profiles._process is not None
        return pyroscope.tag_wrapper(dict(tags)) if installed else nullcontext()

    @classmethod
    def receipt(cls) -> Option[ProfilesReceipt]:
        # process-custody read: Some only while the push agent runs — the workers boot capture and the bundle capsule
        # read the standing endpoint and tenant as data, never the private latch.
        with cls._gate:
            return Option.of_optional(cls._process)

    @classmethod
    def shutdown(cls, scope: ScopeKey = DEFAULT_SCOPE) -> None:
        # custody law: only the scope holding the INSTALLED receipt stops the push thread, clearing every scope receipt
        # (an ADOPTED receipt over a stopped agent is stale) and re-arming a clean re-install; any other scope retires
        # its own receipt alone.
        with cls._gate:
            match cls._receipts.try_find(scope).map(lambda r: r.outcome is ProfilesOutcome.INSTALLED).default_value(False):
                case True:
                    pyroscope.shutdown()
                    cls._process = None
                    cls._receipts = Map.empty()
                case _:
                    cls._receipts = cls._receipts.remove(scope) if cls._receipts.contains_key(scope) else cls._receipts
```

## [03]-[BENCH]

- Owner: `BenchmarkReceipt` carries the macro-benchmark evidence — subject, rounds, warmup, the latency quartet, throughput — and `Bench.run` is the one runner: warmup rounds discarded, measured rounds folded into per-round wall samples, quantiles derived at read, never fold state. One measured window yields both latency and throughput; a mode knob cannot alter facts already present in the same samples.
- Receipt: `contribute` streams one `Receipt.of("runtime.bench", ("emitted", subject, facts))` row and projects the duration and throughput measures onto the `Metrics.record` mapping arm under `domain="bench"`, so the receipt stays truth and the instruments stay projections of it.
- Growth: a new benchmark statistic is one `BenchmarkReceipt` field derived from the held samples; a new bench instrument is one measure name here and one `InstrumentSpec` row on the metrics owner.
- Boundary: this family owns local macro evidence; cross-runtime benchmark authority and corpus gates stay the C# owner's, reached only through the wire. A process-terminal bench run rides the `JobRun.bounded` envelope so the final `domain="bench"` projection flushes before exit; an in-daemon bench rides the standing periodic reader.

```python signature
class BenchmarkReceipt(Struct, frozen=True):
    subject: str
    rounds: int
    warmup: int
    low_ms: float
    p50_ms: float
    p95_ms: float
    high_ms: float
    throughput_hz: float

    @classmethod
    def of(cls, subject: str, warmup: int, samples_ms: tuple[float, ...]) -> "BenchmarkReceipt":
        cut = quantiles(samples_ms, n=20) if len(samples_ms) > 1 else [samples_ms[0]] * 19
        total_s = sum(samples_ms) / 1000.0
        return cls(
            subject=subject,
            rounds=len(samples_ms),
            warmup=warmup,
            low_ms=min(samples_ms),
            p50_ms=cut[9],
            p95_ms=cut[18],
            high_ms=max(samples_ms),
            throughput_hz=len(samples_ms) / total_s if total_s > 0.0 else 0.0,
        )

    def contribute(self) -> tuple[Receipt, ...]:
        Metrics.record({"rasm.bench.duration": self.p50_ms, "rasm.bench.throughput": self.throughput_hz}, domain="bench", kind=self.subject)
        facts = {"rounds": self.rounds, "p50_ms": self.p50_ms, "p95_ms": self.p95_ms, "hz": self.throughput_hz}
        return (Receipt.of("runtime.bench", ("emitted", self.subject, facts)),)


class Bench:
    @staticmethod
    def run(subject: str, op: Callable[[], object], *, rounds: int = 32, warmup: int = 4) -> BenchmarkReceipt:
        if rounds < 1 or warmup < 0:
            raise ValueError("benchmark rounds must be positive and warmup nonnegative")

        def timed(_: int) -> float:
            start = perf_counter()
            op()
            return (perf_counter() - start) * 1000.0

        Block.range(warmup).map(timed)  # warmup rounds run and drop; cache and JIT warm before the measured window
        return BenchmarkReceipt.of(subject, warmup, tuple(Block.range(rounds).map(timed)))
```

## [04]-[JOB]

- Owner: `JobRun.bounded` is the offline-job envelope — install with the hand-built job resource and the high-interval `JOB_SIGNAL_PROFILE`, enroll `Metrics` against that provider, run the body under the `boundary` fence, then drive the telemetry drain so every buffered signal exports before exit. Its drain is the settled telemetry flush-then-shutdown accumulate fold; a body fault outranks a drain fault, and a drain fault surfaces on a clean body.
- Auto: `job_resource` hand-builds identity — `service.name` off `SCOPES[Scope.SERVICE]`, a per-run `service.instance.id`, `job.id`/`run.id` as the job axes — because no auto-detector carries job semantics, and two runs of one job binary must key distinct instances. `JOB_SIGNAL_PROFILE` sets a high export interval so the periodic timer is the safety net and the boundary flush is the egress.
- Cases: `JOB_TEMPORALITY` names the one launcher-environment knob — `OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE=delta` — so each flush is self-contained and the backend sums deltas across runs; a cumulative stream from an exited process orphans its last window.
- Growth: a new job axis is one attribute in `job_resource`; a new lane geometry is one `JOB_SIGNAL_PROFILE` field value.
- Boundary: the envelope composes `Telemetry.install`/`shutdown` with `Metrics.install` and constructs no provider of its own; long-lived daemons keep the profile-keyed `SIGNAL_PROFILE` rows and never ride this envelope.

```python signature
# one launcher-environment knob the job lane sets; each flush self-contains so the backend sums deltas across runs.
JOB_TEMPORALITY: Final[Mapping[str, str]] = {"OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE": "delta"}

# high interval = the timer is the safety net; the boundary force_flush is the real egress for a short-lived process.
JOB_SIGNAL_PROFILE: Final[SignalProfile] = SignalProfile(
    export_interval_ms=60000, schedule_delay_ms=5000, max_queue_size=2048, max_export_batch_size=512, compression=Compression.Gzip
)


def job_resource(job_id: str, run_id: str) -> Resource:
    # hand-built: no detector carries job semantics, and a per-run instance id keys two runs of one binary distinctly.
    return Resource.create(
        {SERVICE_NAMESPACE: NAMESPACE, SERVICE_NAME: SCOPES[Scope.SERVICE], SERVICE_INSTANCE_ID: uuid4().hex, "job.id": job_id, "run.id": run_id},
        schema_url=SCHEMA_URL,
    )


class JobRun:
    @staticmethod
    def bounded[T](profile: RuntimeProfile, endpoint: str, job_id: str, run_id: str, body: Callable[[], T]) -> RuntimeRail[T]:
        Telemetry.install(profile, endpoint, resource=job_resource(job_id, run_id), signal_profile=JOB_SIGNAL_PROFILE)
        Metrics.install()
        outcome = boundary(f"job.{job_id}", body)
        drained = Telemetry.shutdown()  # flush-then-shutdown per provider, ACCUMULATE — runs on the fault arm too
        return outcome.bind(lambda value: drained.map(lambda _flushed: value))
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
