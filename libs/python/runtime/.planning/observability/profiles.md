# [PY_RUNTIME_PROFILES]

`Profiles` pushes continuous CPU profiles beside the OTLP exporters and links them to traces: the pyroscope push agent streams samples to the profile store, and `PyroscopeSpanProcessor` stamps every root span with `pyroscope.profile.id` so a trace click-through lands on its flame graph. This page also owns the whole benchmark tier — the macro-latency and throughput evidence the request-duration histogram cannot carry, the threshold and verdict grading over it, and the repo's one external-tool provision roster the host floor of a graded subject resolves through — plus the offline-job envelope draining a short-lived process before it exits. Measurement and grading seat together because every stratum reaches this tier and none reaches a peer's, so a grader anywhere else is unreachable by the folders that measure.

Install custody is two-tier — per-composition `ProfilesInstall`s key by the observe-owned `ScopeKey` (a same-scope re-install returns `REENTRANT`, a later composition `ADOPTED`) while the imported `latched` guards the one process push agent, `pyroscope.configure` being process-global — and rides the `execution/admission#CONTEXT` `emit_otel` gate, sequenced after `observability/telemetry#TELEMETRY` so the span processor attaches to the registered SDK provider. `SignalProfile` and the flush-then-shutdown drain arrive settled from the telemetry owner, `SCHEMA_URL` from the `reliability/faults#FAULT` scope coordinate one tier below it; `LogShip`/`LogPipeline.configure` from `observability/logging#PIPELINE`; `Metrics.record` from `observability/metrics#METRIC`; `ScopeKey` from `observability/observe#OBSERVE`. Job identity is hand-built, no detector carrying job semantics; delta temporality arrives from the telemetry owner's exporter pin, so the job lane sets no launcher variable of its own.

## [01]-[INDEX]

- [02]-[PROFILES]: scope-keyed, profile-gated pyroscope push install beside the span-profile link.
- [03]-[BENCH]: the `Benchmark` measurement, the threshold/verdict grading half over the repo tool roster, and their instrument projections.
- [04]-[JOB]: offline-job envelope — hand-built resource, one `ship` value arming both halves of the log egress, high-interval safety net, and the flush-then-shutdown boundary.

## [02]-[PROFILES]

- Owner: `Profiles.install` configures the push agent once — application name from the faults-owned `SCOPES[Scope.PROFILES]` row, so the profile store keys the profiler's own emitting plane rather than the served host's name and a backend joining on scope separates the two, server address, static tags, and tenant caller-supplied — and attaches `PyroscopeSpanProcessor` to the registered SDK `TracerProvider`, so every root span carries `pyroscope.profile.id` and the profiler's thread tags carry `span_id`/`span_name`/`trace_id` for the reverse jump. `tenant_id` threads the folder's first-class tenant dimension onto the push, so a multi-tenant profile store slices flame graphs by the same org routing every measurement already carries; `Profiles.phase` scopes sample tags to a bounded window — a recipe stage, a worker kernel window — so a flame graph slices by phase while static dimensions stay install-time `tags=`. Worker floors attach through the workers-owned boot capture: `install` runs in every pool initializer with the `worker.kind` install tag and the parent-captured tenant, the kernel-subject `phase` window rides `traced_kernel`, and the atexit-registered `shutdown` stops the push at worker retirement — so flames come from the process that burns the cycles and a slow offload span clicks through to its worker's graph.
- Entry: a composition whose providers bind no telemetry export caches a `SILENT` install per scope and starts no agent, so an embedded or test-harness process pays nothing; a same-scope re-install returns its cached install stamped `REENTRANT` off the `_installs` map fold, and a later composition arriving after the push agent exists receives `ADOPTED` through the `latched` reentrant closure — the agent never doubles. `PyroscopeSpanProcessor` attaches only when the global resolves to the SDK `TracerProvider` the telemetry install registered — the API no-op provider matches no arm and the install records `linked=False`. `shutdown` is scope-keyed custody: only the scope holding the `INSTALLED` install stops the push thread and clears every scope install; a `SILENT`/`ADOPTED` scope retires its own install alone.
- Auto: `oncpu=True` and `gil_only=False` keep samples on-CPU across Python and native kernels that release the GIL, while idle waits fall out; `shutdown` stops the push thread through `pyroscope.shutdown()` on the drain fold beside the telemetry providers.
- Packages: `pyroscope-otel` (`PyroscopeSpanProcessor` and its bundled push agent `pyroscope.configure`/`shutdown`/`tag_wrapper`/`add_thread_tag`), `opentelemetry-sdk` (the `TracerProvider` match arm — composition-root altitude), runtime (`latched`, `SCOPES`, admission gate).
- Swap: this owner holds the branch's profiles swap point, so the swap off vendor push onto the OTLP profiles signal replaces rows rather than redesigning a lane — `pyroscope.configure` gives way to one `SignalSpec` row on the telemetry owner's signal roster beside one profiles factory on its `EGRESS` map, and profiles then ride the provider drain, exporter policy, and scope coordinate the other three signals already ride. Arming waits on that signal reaching stable across the three SDK trains; `PyroscopeSpanProcessor`'s span-profile stamp, tenant and phase tag projections, and every flamegraph query survive untouched, which leaves transport as the only moving part.
- Growth: a new static profile dimension is one entry in the caller's `tags` mapping; a bounded-window dimension one entry in a `Profiles.phase` mapping; a new worker-floor dimension is one entry in the workers boot-capture tags; a new agent knob is one `configure` keyword threaded through `install`; a new composition is one `ScopeKey` value threaded through the `scope` keyword.
- Boundary: profiles egress through the pyroscope push wire until that swap lands — the OTLP trio stays the telemetry owner's, and no library module below the composition root imports this page.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import Callable, Mapping
from contextlib import AbstractContextManager, nullcontext
from enum import StrEnum
from shutil import which
from statistics import quantiles
from threading import RLock
from time import perf_counter
from typing import ClassVar, Final, Literal, Self
from uuid import uuid4

import pyroscope
from builtins import frozendict
from expression import Error, Nothing, Ok, Option, Some
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import replace
from opentelemetry import trace
from opentelemetry.exporter.otlp.proto.http import Compression
from opentelemetry.sdk.resources import SERVICE_INSTANCE_ID, SERVICE_NAME, SERVICE_NAMESPACE, Resource
from opentelemetry.sdk.trace import TracerProvider
from pydantic_settings import BaseSettings, SettingsConfigDict
from pyroscope.otel import PyroscopeSpanProcessor

from rasm.runtime.admission import RuntimeContext
from rasm.runtime.faults import (
    BENCH_DOUBLED,
    BENCH_EMPTY,
    BENCH_KERNEL,
    BENCH_QUIET,
    BENCH_ROUND,
    BENCH_ROUNDS,
    BENCH_TOOL,
    BENCH_WARMUP,
    PROFILES_JOB,
    SCHEMA_URL,
    SCOPES,
    BoundaryFault,
    Disposition,
    RuntimeResult,
    Scope,
    boundary,
    latched,
    traversed,
)
from rasm.runtime.logging import LogPipeline, LogShip
from rasm.runtime.metrics import Dimension, Metrics
from rasm.runtime.observe import DEFAULT_SCOPE, Facts, ScopeKey, logger
from rasm.runtime.telemetry import NAMESPACE, SignalProfile, Telemetry

# --- [TYPES] ----------------------------------------------------------------------------


class ProfilesOutcome(StrEnum):
    INSTALLED = "installed"
    SILENT = "silent"
    REENTRANT = "reentrant"
    ADOPTED = "adopted"


# --- [MODELS] ---------------------------------------------------------------------------


class ProfilesInstall(Struct, frozen=True):
    outcome: ProfilesOutcome
    application: str
    endpoint: str
    linked: bool
    tenant: str | None = None


# --- [SERVICES] -------------------------------------------------------------------------


class Profiles:
    _installs: ClassVar[Map[ScopeKey, ProfilesInstall]] = Map.empty()
    _process: ClassVar[ProfilesInstall | None] = None
    _gate = RLock()

    @classmethod
    @latched(lambda: Profiles._process, lambda r: setattr(Profiles, "_process", r), lambda prior: replace(prior, outcome=ProfilesOutcome.ADOPTED))
    def _pushed(cls, endpoint: str, tags: Mapping[str, str], tenant: str | None) -> ProfilesInstall:
        application = SCOPES[Scope.PROFILES]
        pyroscope.configure(application_name=application, server_address=endpoint, tags=dict(tags), tenant_id=tenant, oncpu=True, gil_only=False)
        match trace.get_tracer_provider():
            case TracerProvider() as sdk_provider:
                sdk_provider.add_span_processor(PyroscopeSpanProcessor())
                return ProfilesInstall(ProfilesOutcome.INSTALLED, application, endpoint, linked=True, tenant=tenant)
            case _:
                return ProfilesInstall(ProfilesOutcome.INSTALLED, application, endpoint, linked=False, tenant=tenant)

    @classmethod
    def install(
        cls,
        ctx: RuntimeContext,
        endpoint: str,
        tags: Mapping[str, str] | None = None,
        tenant: str | None = None,
        *,
        scope: ScopeKey = DEFAULT_SCOPE,
    ) -> ProfilesInstall:
        with cls._gate:
            match cls._installs.try_find(scope):
                case Option(tag="some", some=prior):
                    return replace(prior, outcome=ProfilesOutcome.REENTRANT)
                case _:
                    installed = (
                        ProfilesInstall(ProfilesOutcome.SILENT, SCOPES[Scope.PROFILES], endpoint, linked=False, tenant=tenant)
                        if not ctx.policy.emit_otel
                        else cls._pushed(endpoint, tags if tags is not None else {}, tenant)
                    )
                    cls._installs = cls._installs.add(scope, installed)
                    return installed

    @staticmethod
    def phase(tags: Mapping[str, str]) -> AbstractContextManager[None]:
        with Profiles._gate:
            installed = Profiles._process is not None
        return pyroscope.tag_wrapper(dict(tags)) if installed else nullcontext()

    @classmethod
    def installed(cls) -> Option[ProfilesInstall]:
        with cls._gate:
            return Option.of_optional(cls._process)

    @classmethod
    def shutdown(cls, scope: ScopeKey = DEFAULT_SCOPE) -> None:
        with cls._gate:
            match cls._installs.try_find(scope).map(lambda r: r.outcome is ProfilesOutcome.INSTALLED).default_value(False):
                case True:
                    pyroscope.shutdown()
                    cls._process = None
                    cls._installs = Map.empty()
                case _:
                    cls._installs = cls._installs.remove(scope) if cls._installs.contains_key(scope) else cls._installs
```

## [03]-[BENCH]

- Owner: `Benchmark` is the macro measurement — subject, mode, rounds, warmup, the latency quartet, throughput, and the refusal that closed the window — and `Bench.run` is the one runner: warmup rounds discarded, measured rounds folded into per-round wall samples, quantiles derived at read, never fold state, and the duration and throughput measures recorded onto `Metrics.record` under `domain="bench"` beside one `bench` line off `Benchmark.facts` at the run site, so the measurement stays truth and the instruments stay its projections. `Bench.graded` closes the other half at the SAME tier: `BenchSubject` rows carry the bar and the host floor, `BenchVerdict.graded` is the one grade projection, and `_verdicted` the one `rasm.bench.verdicts` write. Measurement and grading seat together because every stratum reaches this tier and none reaches a peer's — a grader seated at any producer folder is unreachable by the three folders that measure, so they benched and could never grade.
- Owner: `TOOLS` is the repo's one external-tool roster behind `resolved`, its single discovery entry — settings override, then the row's own probe body — so a host is provisioned or not by one answer rather than by an inline lookup here, a name-to-body map there, and an env-to-constant ladder elsewhere. It seats at this tier for the same reachability reason the grader does: a producer plane can compose it, where a conductor-owned roster it could only reach upward through.
- Entry: `Bench.graded(roster, kernels)` takes ALREADY-BOUND kernels — the `BenchKernel` shape `run` already accepts — which is the whole parameterization: a folder resolves its own deterministic-input edge to a thunk before the call, so its feed, plane, and signal vocabularies never leave its stratum and `BenchSubject` carries no input edge at all. Four refusals close before any counter writes — a doubled subject id, a floor naming a tool no `TOOLS` row keys, a provisioned subject no kernel covers, and a host on which not one subject is provisioned — and the graded verdicts fold under `Disposition.ACCUMULATE` so every subject reports even when one refuses.
- Law: a regression is a VERDICT, never a fault — a slow subject is evidence a board trends, and refusing it would let one regression hide every other subject's grade. Refusal is reserved for a roster or host defect: a doubled id, an unrostered tool, an uncovered subject, a wholly unprovisioned host.
- Law: every anchor a threshold rests on rides the verdict — the INPUT through the feed the calling folder bound, and the HOST through the resolved `floor` paths, since a different binary behind one tool id is a different subject graded on the first one's bar and nothing about the id says so.
- Law: `BenchMode` rides the measurement and selects which bar a consumer grades, one uniform sample stream serving both.
- Law: one measured window yields latency and throughput together, so a mode value alters no fact already present in the samples.
- Law: each round runs behind its own `boundary` fence, so a raising round CLOSES the window and every prior sample survives on the measurement.
- Law: the fold stops at the first refusal, so a broken op pays one round rather than the whole declared window.
- Law: `run` layers only where the window measured nothing, quantiles needing at least one sample.
- Law: the four roster refusals and the three window refusals each resolve their OWN `reliability/faults#FAULT` `RAISES` anchor under `RuntimeLeg.PROFILES` — four distinct census LAWS keep four rows rather than one subject spelling four sentences — and the benched subject rides a NAMED slot where the fault names it. Both timing fences keep the plane's catch-all, since the graded body is a caller kernel no runtime can roster.
- Growth: a new benchmark statistic is one `Benchmark` field derived from the held samples, reaching every verdict through `graded` with no roster edit; a new bench instrument is one measure name here and one `InstrumentSpec` row on the metrics owner; a new run outcome is one `BenchOutcome` member reaching the counter through the single `_verdicted` site; a new external tool is one `TOOLS` row a `floor` names, and a tool whose presence is not a bare PATH lookup grows its own probe body on that row while its deployment override needs no settings edit; a benching folder gains grading by supplying one roster and one kernel map, zero edits here.
- Packages: `pydantic-settings` (the one `RASM_TOOL_PATHS` deployment override, admitted once), the builtin `frozendict` (that override projected immutable), stdlib `shutil.which` (the default `ToolRow` probe body — it answers the resolved path, and an absolute override resolves through it only when executable, never a spawn the roster pays for).
- Boundary: this family owns the branch's macro evidence AND its own corpus gate; benchmark authority stays branch-local, so no peer runtime's figure is graded or cited here and a cross-runtime speed comparison has no owner. A calling folder owns its corpus roster, its recipes, and its deterministic-input vocabulary — this tier reads a bound thunk and never a feed value, so no producer type crosses upward. `JobRun.bounded` envelopes a process-terminal bench run so the final `domain="bench"` projection flushes before exit; an in-daemon bench rides the standing periodic reader.

```python
type BenchKernel = Callable[[], object]
type BenchOutcome = Literal["passed", "regressed", "unprovisioned"]


class BenchMode(StrEnum):
    LATENCY = "latency"
    THROUGHPUT = "throughput"


class Benchmark(Struct, frozen=True):
    subject: str
    mode: BenchMode
    rounds: int
    warmup: int
    low_ms: float
    p50_ms: float
    p95_ms: float
    high_ms: float
    throughput_hz: float
    refused: BoundaryFault | None = None

    @classmethod
    def of(
        cls, subject: str, mode: BenchMode, warmup: int, samples_ms: tuple[float, ...], refused: BoundaryFault | None = None
    ) -> "Benchmark":
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

    def facts(self) -> Facts:
        held: Facts = {"subject": self.subject, "mode": self.mode.value, "rounds": self.rounds, "p50_ms": self.p50_ms, "p95_ms": self.p95_ms, "hz": self.throughput_hz}
        return held if self.refused is None else held | {"refused_at": self.rounds, **self.refused.facts()}


class BenchThreshold(Struct, frozen=True, gc=False):
    p95_ceiling_ms: float
    floor_hz: float = 0.0


class ToolRow(Struct, frozen=True, gc=False):
    binary: str
    probe: Callable[[str], str | None]


class BenchSubject(Struct, frozen=True, gc=False):
    subject: str
    kind: str
    mode: BenchMode
    threshold: BenchThreshold
    floor: tuple[str, ...] = ()
    rounds: int = 32
    warmup: int = 4


class BenchVerdict(Struct, frozen=True, gc=False):
    subject: str
    kind: str
    passed: bool
    p95_ms: float
    ceiling_ms: float
    throughput_hz: float
    floor_hz: float
    floor: tuple[str, ...] = ()

    @classmethod
    def graded(cls, row: BenchSubject, measured: Benchmark, floor: tuple[str, ...], /) -> Self:
        bar = row.threshold
        return cls(
            subject=row.subject,
            kind=row.kind,
            passed=measured.p95_ms <= bar.p95_ceiling_ms and measured.throughput_hz >= bar.floor_hz,
            p95_ms=measured.p95_ms,
            ceiling_ms=bar.p95_ceiling_ms,
            throughput_hz=measured.throughput_hz,
            floor_hz=bar.floor_hz,
            floor=floor,
        )


class ToolSettings(BaseSettings):
    model_config = SettingsConfigDict(env_prefix="RASM_TOOL_", frozen=True, extra="forbid")
    paths: dict[str, str] = {}


_TOOL_PATHS: Final[frozendict[str, str]] = frozendict(ToolSettings().paths)

KTX_TOOL: Final[str] = "ktx"
EXIFTOOL_TOOL: Final[str] = "exiftool"

TOOLS: Final[Map[str, ToolRow]] = Map.of_seq([
    (KTX_TOOL, ToolRow(binary=KTX_TOOL, probe=which)),
    (EXIFTOOL_TOOL, ToolRow(binary=EXIFTOOL_TOOL, probe=which)),
])


def resolved(name: str, /) -> Option[str]:
    return TOOLS.try_find(name).bind(lambda row: Option.of_optional(row.probe(_TOOL_PATHS.get(name, row.binary))))


def _provisioned(row: BenchSubject, /) -> Option[tuple[str, ...]]:
    found = Block.of_seq(row.floor).choose(resolved)
    return Some(tuple(found)) if len(found) == len(row.floor) else Nothing


def _verdicted(subject: str, outcome: BenchOutcome, /) -> None:
    Metrics.record({"rasm.bench.verdicts": 1.0}, domain="bench", kind=subject, dimensions={Dimension.OUTCOME: outcome})


class Bench:
    @staticmethod
    def graded(roster: Block[BenchSubject], kernels: Map[str, BenchKernel], /) -> RuntimeResult[Block[BenchVerdict]]:
        collided = Block.of_seq(subject for subject, count in _tally(roster).items() if count > 1)
        unrostered = frozenset(roster.collect(lambda row: Block.of_seq(row.floor)).filter(lambda tool: tool not in TOOLS))
        probed = roster.map(lambda row: (row, _provisioned(row)))
        live = probed.choose(lambda pair: pair[1].map(lambda found: (pair[0], found)))
        quiet = probed.choose(lambda pair: Nothing if pair[1].is_some() else Some(pair[0]))
        uncovered = live.map(lambda pair: pair[0].subject).filter(lambda subject: kernels.try_find(subject).is_none())

        def one(row: BenchSubject, floor: tuple[str, ...], /) -> RuntimeResult[BenchVerdict]:
            def scored(measured: Benchmark, /) -> BenchVerdict:
                verdict = BenchVerdict.graded(row, measured, floor)
                _verdicted(row.subject, "passed" if verdict.passed else "regressed")
                return verdict

            return Bench.run(row.subject, kernels[row.subject], mode=row.mode, rounds=row.rounds, warmup=row.warmup).map(scored)

        if not collided.is_empty():
            return Error(BENCH_DOUBLED.raised(",".join(sorted(collided))))
        if unrostered:
            return Error(BENCH_TOOL.raised(",".join(sorted(unrostered))))
        if not uncovered.is_empty():
            return Error(BENCH_KERNEL.raised(",".join(sorted(uncovered))))
        if live.is_empty():
            return Error(BENCH_QUIET.raised(",".join(sorted(row.subject for row in quiet))))
        for row in quiet:
            _verdicted(row.subject, "unprovisioned")
        return traversed(live.map(lambda pair: one(*pair)), by=Disposition.ACCUMULATE)

    @staticmethod
    def run(
        subject: str, op: BenchKernel, *, mode: BenchMode = BenchMode.LATENCY, rounds: int = 32, warmup: int = 4
    ) -> RuntimeResult[Benchmark]:
        def timed() -> float:
            start = perf_counter()
            op()
            return (perf_counter() - start) * 1000.0

        def rounded(held: tuple[Block[float], BoundaryFault | None], index: int) -> tuple[Block[float], BoundaryFault | None]:
            samples, refused = held
            if refused is not None:
                return held
            return boundary(BENCH_ROUND, timed, catch=Exception).map(lambda ms: (samples.cons(ms), None)).default_with(lambda fault: (samples, fault))

        if rounds < 1 or warmup < 0:
            return Error(BENCH_ROUNDS.raised(subject, str(rounds), str(warmup)))
        return boundary(BENCH_WARMUP, lambda: Block.range(warmup).fold(lambda _, __: timed(), 0.0), catch=Exception).bind(
            lambda _warmed: _windowed(subject, mode, warmup, Block.range(rounds).fold(rounded, (Block.empty(), None))).map(_recorded)
        )


def _tally(roster: Block[BenchSubject]) -> Map[str, int]:
    return roster.fold(lambda held, row: held.add(row.subject, held.try_find(row.subject).default_value(0) + 1), Map.empty())


def _windowed(
    subject: str, mode: BenchMode, warmup: int, window: tuple[Block[float], BoundaryFault | None]
) -> RuntimeResult[Benchmark]:
    samples, refused = window
    return (
        Ok(Benchmark.of(subject, mode, warmup, tuple(samples), refused))
        if not samples.is_empty()
        else Error(Option.of_optional(refused).default_value(BENCH_EMPTY.raised(subject)))
    )


def _recorded(measured: Benchmark) -> Benchmark:
    Metrics.record({"rasm.bench.duration": measured.p50_ms, "rasm.bench.throughput": measured.throughput_hz}, domain="bench", kind=measured.subject)
    logger().info("bench", **measured.facts())
    return measured
```

## [04]-[JOB]

- Owner: `JobRun.bounded` is the offline-job envelope — arm the log chain, install with the hand-built job resource and the high-interval `JOB_SIGNAL_PROFILE`, enroll `Metrics` against that provider, run the body under the `boundary` fence, then drive the telemetry drain so every buffered signal exports before exit. One `ship` value reaches both halves of the log egress, so the envelope never stands up a `LoggerProvider` no chain row projects onto and a failed job's lines reach the wire beside its spans. Its drain is the settled telemetry flush-then-shutdown accumulate fold; a body fault outranks a drain fault, and a drain fault surfaces on a clean body.
- Auto: `job_resource` hand-builds identity — `service.name` off `SCOPES[Scope.SERVICE]`, a per-run `service.instance.id`, `job.id`/`run.id` as the job axes — because no auto-detector carries job semantics, and two runs of one job binary must key distinct instances. `JOB_SIGNAL_PROFILE` sets a high export interval so the periodic timer is the safety net and the boundary flush is the egress.
- Cases: delta temporality arrives from the telemetry owner's `WIRE_TEMPORALITY` pin at the exporter, which the reader applies by instrument family and which supersedes `OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE` for every family the repo rules — so this envelope sets no launcher variable, each flush self-contains, and a knob here re-decides what the branch already pinned. Both non-monotonic sum families stay cumulative under that same ruling and orphan their last window at exit, which is what makes the boundary flush the egress and the periodic timer the safety net.
- Growth: a new job axis is one attribute in `job_resource`; a new lane geometry is one `JOB_SIGNAL_PROFILE` field value; a new egress arm is the `ship` value the caller threads, reaching chain and provider from that one argument.
- Boundary: the envelope threads one admitted `RuntimeContext` into `Telemetry.install`/`shutdown` beside `LogPipeline.configure` and `Metrics.install` and constructs no provider, processor, or chain row of its own, so the job lane gates emission on the axis value every daemon path reads; long-lived daemons keep the profile-keyed `SIGNAL_PROFILE` rows and never ride this envelope.

```python
JOB_SIGNAL_PROFILE: Final[SignalProfile] = SignalProfile(
    export_interval_ms=60000, schedule_delay_ms=5000, max_queue_size=2048, max_export_batch_size=512, compression=Compression.Gzip
)


def job_resource(job_id: str, run_id: str) -> Resource:
    return Resource.create(
        {SERVICE_NAMESPACE: NAMESPACE, SERVICE_NAME: SCOPES[Scope.SERVICE], SERVICE_INSTANCE_ID: uuid4().hex, "job.id": job_id, "run.id": run_id},
        schema_url=SCHEMA_URL,
    )


class JobRun:
    @staticmethod
    def bounded[T](
        ctx: RuntimeContext, endpoint: str, job_id: str, run_id: str, body: Callable[[], T], *, ship: LogShip = LogShip.OTLP_CONSOLE
    ) -> RuntimeResult[T]:
        LogPipeline.configure(ship=ship)
        installed = Telemetry.install(ctx, endpoint, resource=job_resource(job_id, run_id), signal_profile=JOB_SIGNAL_PROFILE, ship=ship)
        Metrics.install(budget=installed.signal_profile.cardinality_budget)
        outcome = boundary(PROFILES_JOB, body, catch=Exception)
        drained = Telemetry.shutdown()
        return outcome.bind(lambda value: drained.map(lambda _flushed: value))
```

## [05]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
