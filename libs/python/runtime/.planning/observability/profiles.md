# [PY_RUNTIME_PROFILES]

`Profiles` pushes continuous CPU profiles beside the OTLP rails and links them to traces: the pyroscope push agent streams samples to the profile store, and `PyroscopeSpanProcessor` stamps every root span with `pyroscope.profile.id` so a trace click-through lands on its flame graph. This page also owns the benchmark-receipt family — the macro-latency and throughput evidence the request-duration histogram cannot carry — and the offline-job envelope draining a short-lived process before it exits.

Install custody is two-tier — per-composition `ProfilesReceipt`s key by the receipts-owned `ScopeKey` (a same-scope re-install returns `REENTRANT`, a later composition `ADOPTED`) while the imported `latched` guards the one process push agent, `pyroscope.configure` being process-global — and rides the `execution/admission#CONTEXT` `emit_otel` gate, sequenced after `observability/telemetry#TELEMETRY` so the span processor attaches to the registered SDK provider. `SignalProfile` and the flush-then-shutdown drain arrive settled from the telemetry owner, `SCHEMA_URL` from the `reliability/faults#FAULT` scope coordinate one tier below it; `Metrics.record` from `observability/metrics#METRIC`; `Receipt`/`Signals` from `observability/receipts#RECEIPT`. Job identity is hand-built, no detector carrying job semantics; delta temporality arrives from the telemetry owner's exporter pin, so the job lane sets no launcher variable of its own.

## [01]-[INDEX]

- [02]-[PROFILES]: scope-keyed, profile-gated pyroscope push install beside the span-profile link.
- [03]-[BENCH]: benchmark-receipt family and its instrument projection.
- [04]-[JOB]: offline-job envelope — hand-built resource, high-interval safety net, and the flush-then-shutdown boundary.

## [02]-[PROFILES]

- Owner: `Profiles.install` configures the push agent once — application name from the faults-owned `SCOPES[Scope.SERVICE]` row, server address, static tags, and tenant caller-supplied — and attaches `PyroscopeSpanProcessor` to the registered SDK `TracerProvider`, so every root span carries `pyroscope.profile.id` and the profiler's thread tags carry `span_id`/`span_name`/`trace_id` for the reverse jump. `tenant_id` threads the folder's first-class tenant dimension onto the push, so a multi-tenant profile store slices flame graphs by the same org routing every measurement already carries; `Profiles.phase` scopes sample tags to a bounded window — a recipe stage, a worker kernel window — so a flame graph slices by phase while static dimensions stay install-time `tags=`. Worker floors attach through the workers-owned boot capture: `install` runs in every pool initializer with the `worker.kind` install tag and the parent-captured tenant, the kernel-subject `phase` window rides `traced_kernel`, and the atexit-registered `shutdown` stops the push at worker retirement — so flames come from the process that burns the cycles and a slow offload span clicks through to its worker's graph.
- Entry: a composition whose providers bind no telemetry export caches a `SILENT` receipt per scope and starts no agent, so an embedded or test-harness process pays nothing; a same-scope re-install returns its cached receipt stamped `REENTRANT` off the `_receipts` map fold, and a later composition arriving after the push agent exists receives `ADOPTED` through the `latched` reentrant closure — the agent never doubles. `PyroscopeSpanProcessor` attaches only when the global resolves to the SDK `TracerProvider` the telemetry install registered — the API no-op provider matches no arm and the receipt records `linked=False`. `shutdown` is scope-keyed custody: only the scope holding the `INSTALLED` receipt stops the push thread and clears every scope receipt; a `SILENT`/`ADOPTED` scope retires its own receipt alone.
- Auto: `oncpu=True` and `gil_only=False` keep samples on-CPU across Python and native kernels that release the GIL, while idle waits fall out; `shutdown` stops the push thread through `pyroscope.shutdown()` on the drain fold beside the telemetry providers.
- Packages: `pyroscope-otel` (`PyroscopeSpanProcessor` and its bundled push agent `pyroscope.configure`/`shutdown`/`tag_wrapper`/`add_thread_tag`), `opentelemetry-sdk` (the `TracerProvider` match arm — composition-root altitude), runtime (`latched`, `SCOPES`, admission gate).
- Swap: this owner holds the branch's profiles swap point, so migration off vendor push onto the OTLP profiles signal replaces rows rather than redesigning a lane — `pyroscope.configure` gives way to one `SignalSpec` row on the telemetry owner's signal roster beside one profiles factory on its `EGRESS` map, and profiles then ride the provider drain, exporter policy, and scope coordinate the other three signals already ride. Arming waits on that signal reaching stable across the three SDK trains; `PyroscopeSpanProcessor`'s span-profile stamp, tenant and phase tag projections, and every flamegraph query survive untouched, which leaves transport as the only moving part.
- Growth: a new static profile dimension is one entry in the caller's `tags` mapping; a bounded-window dimension one entry in a `Profiles.phase` mapping; a new worker-floor dimension is one entry in the workers boot-capture tags; a new agent knob is one `configure` keyword threaded through `install`; a new composition is one `ScopeKey` value threaded through the `scope` keyword.
- Boundary: profiles egress through the pyroscope push wire until that swap lands — the OTLP trio stays the telemetry owner's, and no library module below the composition root imports this page.

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
from expression import Error, Ok, Option
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import replace
from opentelemetry import trace
from opentelemetry.exporter.otlp.proto.http import Compression
from opentelemetry.sdk.resources import SERVICE_INSTANCE_ID, SERVICE_NAME, SERVICE_NAMESPACE, Resource
from opentelemetry.sdk.trace import TracerProvider
from pyroscope.otel import PyroscopeSpanProcessor

from rasm.runtime.admission import RuntimeContext
from rasm.runtime.faults import SCHEMA_URL, SCOPES, BoundaryFault, RuntimeRail, Scope, boundary, latched
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey
from rasm.runtime.telemetry import NAMESPACE, SignalProfile, Telemetry

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
        ctx: RuntimeContext,
        endpoint: str,
        tags: Mapping[str, str] | None = None,
        tenant: str | None = None,
        *,
        scope: ScopeKey = DEFAULT_SCOPE,
    ) -> ProfilesReceipt:
        # `ctx.policy.emit_otel` folds off the bound telemetry-export provider, so the push gate reads the same
        # axis value the telemetry install reads and the preset name never discriminates here.
        with cls._gate:
            match cls._receipts.try_find(scope):
                case Option(tag="some", some=prior):
                    return replace(prior, outcome=ProfilesOutcome.REENTRANT)
                case _:
                    receipt = (
                        ProfilesReceipt(ProfilesOutcome.SILENT, SCOPES[Scope.SERVICE], endpoint, linked=False, tenant=tenant)
                        if not ctx.policy.emit_otel
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

- Owner: `BenchmarkReceipt` carries the macro-benchmark evidence — subject, mode, rounds, warmup, the latency quartet, throughput, and the refusal that closed the window — and `Bench.run` is the one runner: warmup rounds discarded, measured rounds folded into per-round wall samples, quantiles derived at read, never fold state.
- Law: `BenchMode` rides the receipt as evidence and selects which bar a consumer grades, one uniform sample stream serving both.
- Law: one measured window yields latency and throughput together, so a mode value alters no fact already present in the samples.
- Law: each round runs behind its own `boundary` fence, so a raising round CLOSES the window and every prior sample survives on the receipt.
- Law: the fold stops at the first refusal, so a broken op pays one round rather than the whole declared window.
- Law: `run` rails only where the window measured nothing, quantiles needing at least one sample.
- Receipt: `contribute` streams one `Receipt.of("runtime.bench", ("emitted", subject, facts))` row and projects the duration and throughput measures onto the `Metrics.record` mapping arm under `domain="bench"`, so the receipt stays truth and the instruments stay projections of it.
- Growth: a new benchmark statistic is one `BenchmarkReceipt` field derived from the held samples; a new bench instrument is one measure name here and one `InstrumentSpec` row on the metrics owner.
- Boundary: this family owns the branch's macro evidence and its own corpus gate; benchmark authority stays branch-local, so no peer runtime's figure is graded or cited here and a cross-runtime speed comparison has no owner. `JobRun.bounded` envelopes a process-terminal bench run so the final `domain="bench"` projection flushes before exit; an in-daemon bench rides the standing periodic reader.

```python signature
# graded-bar policy carried as EVIDENCE on the receipt, never a second measurement contract: one uniform per-round
# wall-clock stream serves both, so a mode selects which bar a consumer's grade reads and alters no measured fact.
class BenchMode(StrEnum):
    LATENCY = "latency"
    THROUGHPUT = "throughput"


class BenchmarkReceipt(Struct, frozen=True):
    subject: str
    mode: BenchMode
    rounds: int
    warmup: int
    low_ms: float
    p50_ms: float
    p95_ms: float
    high_ms: float
    throughput_hz: float
    # `refused` names the round that CLOSED the window early; absent, the window ran whole. The slot exists so a
    # partial window stays evidence carrying its own truncation rather than a full window a reader cannot tell apart.
    refused: BoundaryFault | None = None

    @classmethod
    def of(
        cls, subject: str, mode: BenchMode, warmup: int, samples_ms: tuple[float, ...], refused: BoundaryFault | None = None
    ) -> "BenchmarkReceipt":
        cut = quantiles(samples_ms, n=20) if len(samples_ms) > 1 else [samples_ms[0]] * 19
        total_s = sum(samples_ms) / 1000.0
        return cls(
            subject=subject,
            mode=mode,
            rounds=len(samples_ms),
            warmup=warmup,
            low_ms=min(samples_ms),
            p50_ms=cut[9],
            p95_ms=cut[18],
            high_ms=max(samples_ms),
            throughput_hz=len(samples_ms) / total_s if total_s > 0.0 else 0.0,
            refused=refused,
        )

    def contribute(self) -> tuple[Receipt, ...]:
        Metrics.record({"rasm.bench.duration": self.p50_ms, "rasm.bench.throughput": self.throughput_hz}, domain="bench", kind=self.subject)
        facts = {"mode": self.mode.value, "rounds": self.rounds, "p50_ms": self.p50_ms, "p95_ms": self.p95_ms, "hz": self.throughput_hz}
        truncated = {} if self.refused is None else {"refused_at": self.rounds, **self.refused.facts()}
        return (Receipt.of("runtime.bench", ("emitted", self.subject, facts | truncated)),)


class Bench:
    @staticmethod
    def run(
        subject: str, op: Callable[[], object], *, mode: BenchMode = BenchMode.LATENCY, rounds: int = 32, warmup: int = 4
    ) -> RuntimeRail[BenchmarkReceipt]:
        # Every round runs behind its own fence because a measured window IS the evidence: the first refusal closes
        # this window, its fault rides the receipt beside every sample already taken, and later rounds never run. An
        # unfenced fold discards the whole window on its last round — precisely the loss this tier exists to prevent —
        # and only a window holding no sample rails at all, quantile derivation needing one.
        def timed() -> float:
            start = perf_counter()
            op()
            return (perf_counter() - start) * 1000.0

        def rounded(held: tuple[Block[float], BoundaryFault | None], index: int) -> tuple[Block[float], BoundaryFault | None]:
            samples, refused = held
            if refused is not None:
                return held
            # samples accumulate by `cons`, order-free: every derived statistic sorts or sums, so no round index survives.
            return boundary(f"bench.{subject}.{index}", timed).map(lambda ms: (samples.cons(ms), None)).default_with(lambda fault: (samples, fault))

        if rounds < 1 or warmup < 0:
            return Error(BoundaryFault(config=(f"bench.{subject}", f"rounds={rounds} warmup={warmup}")))
        return boundary(f"bench.{subject}.warmup", lambda: Block.range(warmup).fold(lambda _, __: timed(), 0.0)).bind(
            lambda _warmed: _windowed(subject, mode, warmup, Block.range(rounds).fold(rounded, (Block.empty(), None)))
        )


def _windowed(
    subject: str, mode: BenchMode, warmup: int, window: tuple[Block[float], BoundaryFault | None]
) -> RuntimeRail[BenchmarkReceipt]:
    # one window terminal: a window holding samples yields its receipt whether or not a round closed it early, and a
    # window holding none rails on the fault that stopped it — a receipt with no sample would claim quantiles it never
    # measured. `default_with` supplies the fault for a zero-round window no refusal produced.
    samples, refused = window
    return (
        Ok(BenchmarkReceipt.of(subject, mode, warmup, tuple(samples), refused))
        if not samples.is_empty()
        else Error(Option.of_optional(refused).default_value(BoundaryFault(boundary=(f"bench.{subject}", "no round measured"))))
    )
```

## [04]-[JOB]

- Owner: `JobRun.bounded` is the offline-job envelope — install with the hand-built job resource and the high-interval `JOB_SIGNAL_PROFILE`, enroll `Metrics` against that provider, run the body under the `boundary` fence, then drive the telemetry drain so every buffered signal exports before exit. Its drain is the settled telemetry flush-then-shutdown accumulate fold; a body fault outranks a drain fault, and a drain fault surfaces on a clean body.
- Auto: `job_resource` hand-builds identity — `service.name` off `SCOPES[Scope.SERVICE]`, a per-run `service.instance.id`, `job.id`/`run.id` as the job axes — because no auto-detector carries job semantics, and two runs of one job binary must key distinct instances. `JOB_SIGNAL_PROFILE` sets a high export interval so the periodic timer is the safety net and the boundary flush is the egress.
- Cases: delta temporality arrives from the telemetry owner's `WIRE_TEMPORALITY` pin at the exporter, which the reader applies by instrument family and which supersedes `OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE` for every family the estate rules — so this envelope sets no launcher variable, each flush self-contains, and a knob here re-decides what the branch already pinned. Both non-monotonic sum families stay cumulative under that same ruling and orphan their last window at exit, which is what makes the boundary flush the egress and the periodic timer the safety net.
- Growth: a new job axis is one attribute in `job_resource`; a new lane geometry is one `JOB_SIGNAL_PROFILE` field value.
- Boundary: the envelope threads one admitted `RuntimeContext` into `Telemetry.install`/`shutdown` beside `Metrics.install` and constructs no provider of its own, so the job lane gates emission on the axis value every daemon path reads; long-lived daemons keep the profile-keyed `SIGNAL_PROFILE` rows and never ride this envelope.

```python signature
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
    def bounded[T](ctx: RuntimeContext, endpoint: str, job_id: str, run_id: str, body: Callable[[], T]) -> RuntimeRail[T]:
        # Install receipts carry the EFFECTIVE geometry, so the budget threads off the receipt rather than off the
        # requested row: a scope adopting a standing pipeline enrolls against the ceiling that pipeline fixed, and an
        # unthreaded `install()` silently discards every non-default `cardinality_budget` a profile carries.
        installed = Telemetry.install(ctx, endpoint, resource=job_resource(job_id, run_id), signal_profile=JOB_SIGNAL_PROFILE)
        Metrics.install(budget=installed.signal_profile.cardinality_budget)
        outcome = boundary(f"job.{job_id}", body)
        drained = Telemetry.shutdown()  # flush-then-shutdown per provider, ACCUMULATE — runs on the fault arm too
        return outcome.bind(lambda value: drained.map(lambda _flushed: value))
```

## [05]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
