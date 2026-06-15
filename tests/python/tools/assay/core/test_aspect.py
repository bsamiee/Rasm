"""Aspect laws for decorator ordering, ROP weaving, telemetry, and ring capture.

Pins double-decoration guards, Slot monotonicity, beartype validation, finish severity, OTel
status/event stamping, and recent-event ring projection. Law registration runs at import time
because the policy ledger reads the MANIFEST immediately after collection.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections import deque
from collections.abc import Callable  # noqa: TC003  # runtime annotation
from typing import Annotated

from beartype.roar import BeartypeCallHintViolation
from beartype.vale import Is
from expression import Error, Ok, Result  # noqa: TC002  # Hom rail annotation
from expression.collections import block
from hypothesis import given, strategies as st
from opentelemetry import trace
from opentelemetry.sdk.trace.export.in_memory_span_exporter import InMemorySpanExporter  # noqa: TC002  # collection-time fixture annotation
import pytest
from structlog.contextvars import get_contextvars

from tests.python._testkit.laws import register_law
from tests.python._testkit.spec import assert_error, assert_ok, identity, support_matrix
from tools.assay.core.aspect import (
    _RING,  # ring seam seeded directly for projection laws
    assemble,
    checked,
    checked_call,
    compose,
    Hom,  # noqa: TC001  # runtime woven-Hom annotation
    Inversion,
    Layer,  # noqa: TC001  # runtime layer-tuple annotation
    logged,
    ring_processor,
    ring_recent,
    Slot,
    traced,
)
from tools.assay.core.model import Claim, Fault, fold, Report  # noqa: TC001  # Hom rail annotation
from tools.assay.core.status import RailStatus


# --- [CONSTANTS] ------------------------------------------------------------------------

_REPORT: Report = fold(Claim.STATIC, "probe", ())

# --- [OPERATIONS] -----------------------------------------------------------------------


def _rail(_x: object) -> Result[Report, Fault]:
    return Ok(_REPORT)


def _busy(_x: object) -> Result[Report, Fault]:
    return Error(Fault((), RailStatus.BUSY, "lease busy"))


def _faulting(_x: object) -> Result[Report, Fault]:
    return Error(Fault((), RailStatus.FAULTED, "boom"))


def _raising(_x: object) -> Result[Report, Fault]:
    raise RuntimeError("raised-through-rail")


def _keys(_x: object) -> dict[str, object]:
    return {"run_id": "r1"}


def _attrs(_x: object) -> dict[str, object]:
    return {"assay.verb": "probe"}


# --- [SLOT]


def test_slot_ordering_is_monotonic() -> None:
    """Slot is a totally-ordered IntEnum: checked < logged < traced — the order ``assemble`` folds over."""
    support_matrix(
        ("checked<logged", lambda: Slot.checked < Slot.logged, True),
        ("logged<traced", lambda: Slot.logged < Slot.traced, True),
        ("checked<traced", lambda: Slot.checked < Slot.traced, True),
        ("traced!<checked", lambda: Slot.traced < Slot.checked, False),
        ("checked-is-zero", lambda: int(Slot.checked) == 0, True),
    )


# --- [COMPOSE_ASSEMBLE]


def test_compose_double_checked_equals_single() -> None:
    """Repeating the same layer instance weaves once and preserves the single ``_assay_ids`` marker set.

    The mypy-only suppression keeps the public ``compose(layer, layer)(fn)`` idempotency recipe intact;
    ty specializes ``checked()`` cleanly while mypy collapses its free ParamSpec to ``Never``.
    """
    layer = checked()  # type: ignore[var-annotated]  # mypy Never-collapse; ty infers the generic layer
    once: Hom[[object], Report] = compose(layer)(_rail)
    twice: Hom[[object], Report] = compose(layer, layer)(_rail)
    raw: Hom[[object], Report] = compose()(_rail)  # type: ignore[assignment]  # mypy Never-collapse on empty compose(); ty clean

    once_ids: frozenset[int] = getattr(once, "_assay_ids", frozenset())
    assert getattr(twice, "_assay_ids", frozenset()) == once_ids
    assert once_ids != getattr(raw, "_assay_ids", frozenset())
    assert assert_ok(twice("ignored")) == assert_ok(once("ignored"))


def test_compose_is_assemble_through_ok() -> None:
    """``compose`` projects ``assemble`` through the Ok rail for valid layer order."""
    log: Layer[[object], Report] = logged(event="rail", keys=_keys)
    trc: Layer[[object], Report] = traced(span="rail.span", attrs=_attrs)
    layers: list[Layer[[object], Report]] = [log, trc]
    woven_compose: Hom[[object], Report] = compose(*layers)(_rail)
    woven_assemble: Hom[[object], Report] = assert_ok(assemble(block.of_seq(layers), _rail))

    assert assert_ok(woven_compose("x")) == assert_ok(woven_assemble("x"))


def test_assemble_accepts_ascending_rejects_inversion() -> None:
    """Layer order is valid iff slots are non-descending; any regression yields ``Error(Inversion)``."""
    ident: Callable[[Hom[[object], Report]], Hom[[object], Report]] = lambda h: h  # noqa: E731
    cases: tuple[tuple[str, tuple[Slot, ...], bool], ...] = (
        ("checked-then-logged", (Slot.checked, Slot.logged), True),
        ("checked-then-traced", (Slot.checked, Slot.traced), True),
        ("ascending-full", (Slot.checked, Slot.logged, Slot.traced), True),
        ("logged-then-checked", (Slot.logged, Slot.checked), False),
        ("traced-then-logged", (Slot.traced, Slot.logged), False),
        ("traced-then-checked", (Slot.traced, Slot.checked), False),
    )
    for label, layers, accepted in cases:
        built: block.Block[Layer[[object], Report]] = block.of_seq([(s, ident) for s in layers])
        outcome: Result[Hom[[object], Report], Inversion] = assemble(built, _rail)
        match accepted:
            case True:
                woven = assert_ok(outcome)
                assert assert_ok(woven("x")) == _REPORT, label
            case False:
                inv = assert_error(outcome)
                assert inv.outer > inv.inner, label


def test_compose_raises_typeerror_carrying_inversion() -> None:
    """``compose`` surfaces a slot regression as ``TypeError`` whose arg is the originating ``Inversion``."""
    ident: Callable[[Hom[[object], Report]], Hom[[object], Report]] = lambda h: h  # noqa: E731
    log: Layer[[object], Report] = logged(event="x", keys=_keys)
    lower: Layer[[object], Report] = (Slot.checked, ident)
    with pytest.raises(TypeError, match=r"Slot\.logged") as raised:
        compose(log, lower)(_rail)

    assert isinstance(raised.value.args[0], Inversion)
    assert raised.value.args[0].outer is Slot.logged


# --- [CHECKED_CHECKED_CALL]


def test_checked_is_checked_slot_layer() -> None:
    """``checked`` produces a ``Layer`` pinned to ``Slot.checked`` carrying the beartype-weaving decorator."""
    slot, dec = checked()  # type: ignore[var-annotated]  # mypy collapses checked()'s free ParamSpec to Never; ty infers it
    assert slot is Slot.checked
    woven: Hom[[object], Report] = dec(_rail)
    assert assert_ok(woven("x")) == _REPORT


def test_checked_call_is_idempotent_under_repeat() -> None:
    """``checked_call`` weaves once: re-application via the ``_once`` guard returns the same woven identity."""

    def typed_rail(_n: int) -> Result[Report, Fault]:
        return Ok(_REPORT)

    identity(
        checked_call(typed_rail),
        lambda f: checked_call(f),  # noqa: PLW0108  # the lambda monomorphizes checked_call's ParamSpec so identity[T] unifies under ty
        eq=lambda a, b: getattr(a, "_assay_ids", frozenset()) == getattr(b, "_assay_ids", frozenset()) and a(0) == b(0),
    )


def test_checked_call_faults_on_shape_violation() -> None:
    """The beartype-woven rail enforces the annotated parameter constraint at call time.

    The validator is runtime-only to ty, so the negative literal proves ``checked_call`` actually weaves beartype.
    """

    def typed_rail(_n: Annotated[int, Is[lambda n: n > 0]]) -> Result[Report, Fault]:
        return Ok(_REPORT)

    woven = checked_call(typed_rail)

    assert assert_ok(woven(3)) == _REPORT
    with pytest.raises(BeartypeCallHintViolation):
        woven(-1)


# --- [LOGGED]


def test_logged_binds_keys_during_call_clears_after() -> None:
    """``logged`` binds the projected contextvars for the handler body and clears them on return."""
    seen: dict[str, object] = {}

    def handler(_x: object) -> Result[Report, Fault]:
        seen.update(get_contextvars())
        return Ok(_REPORT)

    layer: Layer[[object], Report] = logged(event="rail", keys=_keys)
    woven: Hom[[object], Report] = compose(layer)(handler)
    outcome = woven("payload")

    assert assert_ok(outcome) == _REPORT
    assert seen.get("run_id") == "r1"
    assert get_contextvars() == {}


def test_logged_emits_finish_event_per_rail(log_events: list[dict[str, object]]) -> None:
    """``logged`` emits exactly one ``<event>.finish`` log per woven call on both Ok and Error rails."""
    layer: Layer[[object], Report] = logged(event="rail", keys=_keys)
    ok_woven: Hom[[object], Report] = compose(layer)(_rail)
    err_woven: Hom[[object], Report] = compose(layer)(_faulting)

    _ = assert_ok(ok_woven("x"))
    _ = assert_error(err_woven("y"))

    finishes = tuple(e for e in log_events if e.get("event") == "rail.finish")
    assert len(finishes) == 2
    assert {e.get("log_level") for e in finishes} == {"info", "error"}


def test_logged_finish_severity_gate_info_below_failed(log_events: list[dict[str, object]]) -> None:
    """An Error rail whose Fault severity sits below FAILED (BUSY) finishes at info level with its own status.

    The severity gate keeps lease-busy exits out of alert noise while still emitting exactly one finish record.
    """
    layer: Layer[[object], Report] = logged(event="rail", keys=_keys)
    woven: Hom[[object], Report] = compose(layer)(_busy)

    _ = assert_error(woven("x"))

    finishes = tuple(e for e in log_events if e.get("event") == "rail.finish")
    assert len(finishes) == 1
    assert finishes[0].get("log_level") == "info"
    assert finishes[0].get("status") is RailStatus.BUSY


def test_logged_emits_faulted_finish_on_raised_then_reraises(log_events: list[dict[str, object]]) -> None:
    """A raised exception bypasses the Result rail: ``logged`` emits one FAULTED ``<event>.finish`` then re-propagates.

    Raised exceptions remain distinct from returned faults and must surface traceback evidence before re-raising.
    """
    layer: Layer[[object], Report] = logged(event="rail", keys=_keys)
    woven: Hom[[object], Report] = compose(layer)(_raising)

    with pytest.raises(RuntimeError, match="raised-through-rail"):
        _ = woven("x")

    finishes = tuple(e for e in log_events if e.get("event") == "rail.finish")
    assert len(finishes) == 1
    assert finishes[0].get("log_level") == "error"
    assert finishes[0].get("status") is RailStatus.FAULTED
    assert finishes[0].get("exc_info") is True


# --- [TRACED]


def test_traced_sync_records_one_span_with_status(otel_spans: InMemorySpanExporter) -> None:
    """A sync woven rail records exactly one span stamped with its OK status under the active provider."""
    layer: Layer[[object], Report] = traced(span="probe.span", attrs=_attrs)
    woven: Hom[[object], Report] = compose(layer)(_rail)
    outcome = woven("x")
    spans = otel_spans.get_finished_spans()

    assert assert_ok(outcome) == _REPORT
    assert tuple(s.name for s in spans) == ("probe.span",)
    assert spans[0].attributes is not None
    assert spans[0].attributes["assay.verb"] == "probe"
    assert spans[0].attributes["assay.status"] == str(_REPORT.status)


def test_traced_stamps_fault_status_and_adds_fault_event(otel_spans: InMemorySpanExporter) -> None:
    """An Error rail stamps the fault status on the span and records exactly one ``assay.fault`` event."""
    layer: Layer[[object], Report] = traced(span="probe.fault", attrs=_attrs)
    woven: Hom[[object], Report] = compose(layer)(_faulting)
    outcome = woven("x")
    spans = otel_spans.get_finished_spans()

    fault = assert_error(outcome)
    assert tuple(s.name for s in spans) == ("probe.fault",)
    assert spans[0].attributes is not None
    assert spans[0].attributes["assay.status"] == fault.status.value
    assert tuple(e.name for e in spans[0].events) == ("assay.fault",)


# --- [RING_PROCESSOR_RING_RECENT]


@given(level=st.sampled_from(("info", "warning", "error")), event=st.text(max_size=32))
def test_ring_processor_seeds_ring_and_passes_dict(level: str, event: str) -> None:
    """``ring_processor`` appends one ``<level>:<event>`` summary to the active ring and returns the dict."""
    ring: deque[str] = deque(maxlen=4)
    token = _RING.set(ring)
    try:
        payload: dict[str, object] = {"event": event, "level": level, "extra": 1}
        out = ring_processor(None, "method", payload)
        assert out is payload
        assert out["extra"] == 1
        assert tuple(ring) == (f"{level}:{event}",)
        assert ring_recent() == (f"{level}:{event}",)
    finally:
        _RING.reset(token)


def test_ring_processor_injects_trace_ids_under_active_span(otel_spans: InMemorySpanExporter) -> None:
    """An active recording span stamps ``trace_id``/``span_id``; absence leaves both keys out."""
    tracer = trace.get_tracer("assay.test")
    with tracer.start_as_current_span("op"):
        active = ring_processor(None, "info", {"event": "x", "level": "info"})
    inactive = ring_processor(None, "info", {"event": "y", "level": "info"})

    assert int(str(active["trace_id"]), 16) != 0
    assert len(str(active["span_id"])) == 16
    assert "trace_id" not in inactive
    assert "span_id" not in inactive
    _ = otel_spans


def test_ring_recent_empty_without_active_ring() -> None:
    """``ring_recent`` returns an empty tuple when no ring contextvar is bound in the current context."""
    assert ring_recent() == ()


# --- [COMPOSITION] ----------------------------------------------------------------------

for _subject, _law in (
    (Slot, "test_slot_ordering_is_monotonic"),
    (compose, "test_compose_double_checked_equals_single"),
    (assemble, "test_assemble_accepts_ascending_rejects_inversion"),
    (checked, "test_checked_is_checked_slot_layer"),
    (checked_call, "test_checked_call_is_idempotent_under_repeat"),
    (logged, "test_logged_binds_keys_during_call_clears_after"),
    (logged, "test_logged_finish_severity_gate_info_below_failed"),
    (logged, "test_logged_emits_faulted_finish_on_raised_then_reraises"),
    (traced, "test_traced_sync_records_one_span_with_status"),
    (ring_processor, "test_ring_processor_seeds_ring_and_passes_dict"),
    (ring_recent, "test_ring_recent_empty_without_active_ring"),
):
    register_law(_subject, _law, module=__name__)
