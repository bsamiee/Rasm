"""Aspect layer laws."""

from collections import deque
from pathlib import Path
import sys
from typing import Protocol

import anyio
import anyio.lowlevel
from expression import Ok, Result
from opentelemetry import trace
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import SimpleSpanProcessor
from opentelemetry.sdk.trace.export.in_memory_span_exporter import InMemorySpanExporter
from pydantic import ValidationError
import pytest
from structlog.contextvars import get_contextvars

import tools.assay as assay_pkg
from tools.assay.composition.registry import _checked_report, _guard  # noqa: PLC2701
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # beartype boundary test needs runtime annotations
from tools.assay.core.aspect import _RING, compose, Inversion, logged, ring_processor, Slot, traced  # noqa: PLC2701
from tools.assay.core.model import Claim, Fault, fold, Report  # noqa: TC001
from tools.assay.rails.static import StaticParams


class _ProcessResult(Protocol):
    returncode: int
    stdout: bytes


@pytest.fixture
def traced_spans(monkeypatch: pytest.MonkeyPatch) -> InMemorySpanExporter:
    """A LOCAL TracerProvider+exporter bound to ``aspect._TRACER``.

    Isolated from the global session provider, which ``main()`` shuts down in its finally block — so any
    ``cli``-fixture test running earlier would otherwise stop span export and make these laws order-dependent.

    Returns:
        The in-memory exporter capturing only spans this test records.
    """
    exporter = InMemorySpanExporter()
    provider = TracerProvider()
    provider.add_span_processor(SimpleSpanProcessor(exporter))
    monkeypatch.setattr("tools.assay.core.aspect._TRACER", provider.get_tracer("assay.core"))
    return exporter


def test_ring_processor_records_level_event_and_passes_dict() -> None:
    """Structlog events seed the recent-event ring without mutating unrelated keys."""
    ring: deque[str] = deque(maxlen=4)
    token = _RING.set(ring)
    try:
        event = {"event": "rail.finish", "level": "info", "status": "ok"}
        assert ring_processor(None, "info", event) is event
        assert tuple(ring) == ("info:rail.finish",)
    finally:
        _RING.reset(token)


def test_compose_rejects_layer_inversion() -> None:
    """Layer slots are monotonic; a lower slot after logged raises Inversion."""

    def identity() -> Result[None, Fault]:
        return Ok(None)

    with pytest.raises(TypeError, match=r"Slot\.logged") as raised:
        compose(logged(event="x", keys=dict), (Slot.checked, lambda fn: fn))(identity)

    assert isinstance(raised.value.args[0], Inversion)


def test_assay_claw_imports_package() -> None:
    """ASSAY_CLAW import-time beartype checking loads the package instead of failing on aspect annotations."""
    root = Path(__file__).resolve().parents[4]

    async def run() -> _ProcessResult:
        return await anyio.run_process([sys.executable, "-c", "import tools.assay"], cwd=root, env={"ASSAY_CLAW": "1"}, check=False)

    done = anyio.run(run)

    assert done.returncode == 0
    assert done.stdout == b""


def test_bootstrap_error_preserves_import_time_settings_fault(monkeypatch: pytest.MonkeyPatch) -> None:
    """Import-time settings validation faults are inspectable instead of discarded."""
    try:
        AssaySettings(run_id="")
    except ValidationError as exc:
        fault = exc
    else:  # pragma: no cover
        pytest.fail("expected invalid run_id to raise ValidationError")
    monkeypatch.setattr(assay_pkg, "_BOOTSTRAP_ERROR", fault)

    assert assay_pkg.bootstrap_error() is fault


def test_install_tracing_is_endpoint_gated(monkeypatch: pytest.MonkeyPatch) -> None:
    """Tracing provider installation is an explicit lifecycle call, not package-import side effect."""
    calls: list[str] = []

    def install(endpoint: str) -> None:
        calls.append(endpoint)

    monkeypatch.setattr(assay_pkg, "_install_tracing", install)

    assay_pkg.install_tracing("")
    assay_pkg.install_tracing("http://collector")

    assert calls == ["http://collector"]


def test_registry_checked_layer_faults_beartype_violations() -> None:
    """Registry checked layer turns wrong boundary shapes into validation Faults through the real guard."""

    def handler(settings: AssaySettings, scope: ArtifactScope, params: object) -> Result[Report, Fault]:
        _ = (settings, scope, params)
        return Ok(fold(Claim.STATIC, "report", ()))

    checked = _checked_report(handler)
    invoke = getattr(checked, "__call__")  # noqa: B004, B009  # intentional runtime-call boundary to prove beartype catches bad shapes
    outcome = _guard(lambda: invoke("not-settings", object(), StaticParams()))

    assert outcome.is_error()
    assert outcome.error.message.startswith("validation:")


# --- [TRACING] --------------------------------------------------------------------------


def test_ring_processor_injects_trace_ids_under_active_span(otel_spans: InMemorySpanExporter) -> None:
    """ring_processor stamps trace_id/span_id from the ambient span via the session provider, no override."""
    tracer = trace.get_tracer("assay.test")
    with tracer.start_as_current_span("op"):
        event = ring_processor(None, "info", {"event": "x", "level": "info"})

    assert event["trace_id"] == f"{int(event['trace_id'], 16):032x}"
    assert event["span_id"] == f"{int(event['span_id'], 16):016x}"
    _ = otel_spans


def test_ring_processor_omits_trace_ids_without_active_span() -> None:
    """No recording span leaves the event free of trace identifiers."""
    event = ring_processor(None, "info", {"event": "y", "level": "info"})

    assert "trace_id" not in event
    assert "span_id" not in event


def test_logged_layer_binds_keys_during_call_and_clears_after() -> None:
    """Logged binds the projected keys for the handler body and clears them on exit."""
    during: dict[str, object] = {}

    def handler(settings: AssaySettings, scope: object, params: StaticParams) -> Result[Report, Fault]:
        _ = (settings, scope, params)
        during.update(get_contextvars())
        return Ok(fold(Claim.STATIC, "report", ()))

    woven = compose(logged(event="rail", keys=lambda *a, **kw: {"run_id": "r1"}))(handler)
    outcome = woven(AssaySettings(), None, StaticParams())

    assert outcome.is_ok()
    assert during.get("run_id") == "r1"
    assert get_contextvars() == {}


def test_traced_sync_stamps_status_and_records_span(traced_spans: InMemorySpanExporter) -> None:
    """A sync woven rail records one span carrying the stamped status attribute."""

    def handler(settings: AssaySettings, scope: object, params: StaticParams) -> Result[Report, Fault]:
        _ = (settings, scope, params)
        return Ok(fold(Claim.STATIC, "report", ()))

    woven = compose(traced(span="static.report", attrs=lambda *a, **kw: {"assay.verb": "report"}))(handler)
    outcome = woven(AssaySettings(), None, StaticParams())
    spans = traced_spans.get_finished_spans()

    assert outcome.is_ok()
    assert tuple(s.name for s in spans) == ("static.report",)
    assert spans[0].attributes is not None
    assert spans[0].attributes["assay.verb"] == "report"
    assert spans[0].attributes["assay.status"] == str(outcome.ok.status)


def test_traced_async_dispatches_awoven_and_stamps_fault(traced_spans: InMemorySpanExporter) -> None:
    """An async handler routes through awoven, stamping the fault status and adding a fault event."""
    fault = Fault((), message="boom")

    async def handler(settings: AssaySettings, scope: object, params: StaticParams) -> Result[Report, Fault]:
        _ = (settings, scope, params)
        await anyio.lowlevel.checkpoint()
        return Result.Error(fault)

    woven = compose(traced(span="static.async", attrs=lambda *a, **kw: {"assay.verb": "async"}))(handler)  # ty: ignore[invalid-argument-type]  # compose returns awoven (coroutine fn) at runtime via iscoroutinefunction dispatch; ty cannot narrow the overload
    outcome = anyio.run(lambda: woven(AssaySettings(), None, StaticParams()))  # ty: ignore[invalid-argument-type]  # woven is awoven (coroutine fn) at runtime; lambda returns Awaitable
    spans = traced_spans.get_finished_spans()

    assert outcome.is_error()
    assert tuple(s.name for s in spans) == ("static.async",)
    assert spans[0].attributes is not None
    assert spans[0].attributes["assay.status"] == fault.status.value
    assert tuple(e.name for e in spans[0].events) == ("assay.fault",)
