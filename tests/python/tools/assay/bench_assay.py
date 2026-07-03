"""Row-driven benchmarks for representative assay hot paths."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from typing import TYPE_CHECKING

import msgspec
import pytest

from tests.python._testkit.bench import BenchCase, run_registry
from tests.python._testkit.strategies import resolve
from tests.python.tools.assay.kit import WIRE_ENCODER
from tools.assay.core.model import Claim, Completed, Envelope, envelope, Input, Language, Mode, RailStatus, receipt, Runner, Tool
from tools.assay.core.routing import place, Routed, Scope
from tools.assay.diagnostics import fold


if TYPE_CHECKING:
    from hypothesis import strategies as st

# --- [CONSTANTS] ------------------------------------------------------------------------

pytestmark = pytest.mark.benchmark

_FILES_TOOL: Tool = Tool(
    name="bench-files", runner=Runner.UV, command=("ruff", "check"), input=Input.FILES, language=Language.PYTHON, claim=Claim.CODE, mode=Mode.CHECK
)

_SETUP_ENCODER: msgspec.json.Encoder = msgspec.json.Encoder(order="deterministic")
_WIRE_DECODER: msgspec.json.Decoder[Envelope] = msgspec.json.Decoder(Envelope)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _envelope(size: int) -> Envelope:
    receipts = tuple(receipt(("t",), 0, status=RailStatus.OK) for _ in range(size))
    return envelope(fold(Claim.STATIC, "check", receipts), claim=Claim.STATIC, verb="check")


def _resolve_subject(payload: tuple[object, ...]) -> object:
    """Measure repeated ``type_info`` resolution without timing registry construction.

    Returns:
        Opaque value retained by the benchmark harness.
    """
    (size,) = payload
    assert isinstance(size, int)
    return tuple(msgspec.inspect.type_info(Completed) for _ in range(size))


def _place_subject(payload: tuple[object, ...]) -> object:
    import tempfile  # noqa: PLC0415

    from upath import UPath  # noqa: PLC0415

    from tools.assay.composition.settings import AssaySettings  # noqa: PLC0415

    (routed, tool) = payload
    assert isinstance(routed, Routed)
    assert isinstance(tool, Tool)
    with tempfile.TemporaryDirectory() as tmp:
        settings: AssaySettings = AssaySettings(root=UPath(tmp))
        return place(routed, tool, settings=settings)


# --- [TABLES] ---------------------------------------------------------------------------

_ROWS: tuple[BenchCase, ...] = (
    BenchCase(
        label="resolve-strategy-build", subject=_resolve_subject, workload=lambda size: (size,), sizes=(10, 100, 500), budget_ms=200.0, rounds=5
    ),
    BenchCase(
        label="model-fold",
        subject=lambda payload: fold(Claim.STATIC, "check", msgspec.convert(payload[0], tuple[Completed, ...])),
        workload=lambda size: (tuple(receipt(("tool",), 0, status=RailStatus.OK) for _ in range(size)),),
        sizes=(100, 1_000, 5_000),
        budget_ms=5.0,
        rounds=10,
    ),
    BenchCase(
        label="wire-encode",
        subject=lambda payload: WIRE_ENCODER.encode(payload[0]),
        workload=lambda size: (_envelope(size),),
        sizes=(10, 100, 500),
        budget_ms=10.0,
        rounds=20,
    ),
    BenchCase(
        label="wire-decode",
        subject=lambda payload: _WIRE_DECODER.decode(msgspec.convert(payload[0], bytes)),
        workload=lambda size: (_SETUP_ENCODER.encode(_envelope(size)),),
        sizes=(10, 100, 500),
        budget_ms=10.0,
        rounds=20,
    ),
    BenchCase(
        label="wire-roundtrip",
        subject=lambda payload: _WIRE_DECODER.decode(WIRE_ENCODER.encode(payload[0])),
        workload=lambda size: (_envelope(size),),
        sizes=(10, 100, 500),
        budget_ms=20.0,
        rounds=10,
    ),
    BenchCase(
        label="routing-place",
        subject=_place_subject,
        workload=lambda size: (
            Routed(language=Language.PYTHON, scope=Scope.CHANGED, files=tuple(f"pkg/mod_{i}.py" for i in range(size))),
            _FILES_TOOL,
        ),
        sizes=(10, 100, 500),
        budget_ms=50.0,
        rounds=5,
    ),
)

# --- [COMPOSITION] ----------------------------------------------------------------------

# Eager resolver warm-up keeps the first benchmark size from timing strategy registration.
_completed_st: st.SearchStrategy[Completed] = resolve(Completed)

bench_assay = run_registry(_ROWS)
