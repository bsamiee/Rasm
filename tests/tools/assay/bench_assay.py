"""Benchmark suite for representative assay operations.

resolve strategy build, wire encode/decode, model fold, and routing place.
Each ``BenchSubject`` row encodes a real workload; ``bench_params`` generates the Cartesian
``(row, size)`` product so one parametrized function owns all benchmark invocations.
"""

# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------

from typing import TYPE_CHECKING

import msgspec
import pytest

from tests._bench import bench_params, BenchSubject, run_bench  # noqa: PLC2701
from tests._strategies import resolve  # noqa: PLC2701
from tests.tools.assay.conftest import WIRE_ENCODER
from tools.assay.core.model import Claim, Completed, Envelope, envelope, fold, Input, Language, Mode, receipt, Runner, Tool
from tools.assay.core.routing import place, Routed, Scope
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from hypothesis import strategies as st
    from pytest_benchmark.fixture import BenchmarkFixture


# --- [CONSTANTS] -----------------------------------------------------------------------

pytestmark = pytest.mark.benchmark

_FILES_TOOL: Tool = Tool(
    name="bench-files",
    runner=Runner.UV,
    command=("ruff", "check"),
    input=Input.FILES,
    language=Language.PYTHON,
    claim=Claim.CODE,
    mode=Mode.CHECK,
)

# Deterministic encoder for setup steps (mirrors WIRE_ENCODER policy; used inside workload builders).
_SETUP_ENCODER: msgspec.json.Encoder = msgspec.json.Encoder(order="deterministic")
_WIRE_DECODER: msgspec.json.Decoder[Envelope] = msgspec.json.Decoder(Envelope)

# Resolve strategy registered at module import; example() calls are the timed operation in resolve bench.
_completed_st: st.SearchStrategy[Completed] = resolve(Completed)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _resolve_subject(payload: tuple[object, ...]) -> object:
    """Walk the msgspec type-info graph for ``size`` field nodes of Completed.

    This measures the resolve() core path — ``msgspec.inspect.type_info`` field enumeration —
    which is the expensive step executed once per type during strategy registration. ``size``
    scales the number of repeated inspections (simulating resolver warm-up across N types).

    Returns:
        Tuple of StructType nodes, one per inspection call.
    """
    (size,) = payload
    assert isinstance(size, int)
    return tuple(msgspec.inspect.type_info(Completed) for _ in range(size))


def _fold_workload(size: int) -> tuple[object, ...]:
    """Build ``size`` OK receipts for the fold benchmark.

    Returns:
        One-element tuple wrapping the outcomes sequence.
    """
    outcomes: tuple[Completed, ...] = tuple(
        receipt(("tool",), 0, status=RailStatus.OK) for _ in range(size)
    )
    return (outcomes,)


def _fold_subject(payload: tuple[object, ...]) -> object:
    """Fold a sequence of Completed receipts into one Report.

    Returns:
        Folded Report carrying counts and status.
    """
    (raw_outcomes,) = payload
    # msgspec.convert narrows tuple[object,...] → tuple[Completed,...] with full runtime validation.
    outcomes: tuple[Completed, ...] = msgspec.convert(raw_outcomes, tuple[Completed, ...])
    return fold(Claim.STATIC, "check", outcomes)


def _encode_workload(size: int) -> tuple[object, ...]:
    """Build one Envelope wrapping a ``size``-receipt fold for the encode benchmark.

    Returns:
        One-element tuple carrying the pre-built Envelope.
    """
    outcomes: tuple[Completed, ...] = tuple(
        receipt(("t",), 0, status=RailStatus.OK) for _ in range(size)
    )
    env = envelope(fold(Claim.STATIC, "check", outcomes), claim=Claim.STATIC, verb="check")
    return (env,)


def _encode_subject(payload: tuple[object, ...]) -> object:
    """Encode a pre-built Envelope to deterministic JSON bytes.

    Returns:
        JSON bytes for the given Envelope.
    """
    (env,) = payload
    assert isinstance(env, Envelope)
    return WIRE_ENCODER.encode(env)


def _decode_workload(size: int) -> tuple[object, ...]:
    """Pre-encode one Envelope so the timed segment measures only msgspec JSON decode.

    Returns:
        One-element tuple carrying the pre-encoded bytes.
    """
    outcomes: tuple[Completed, ...] = tuple(
        receipt(("t",), 0, status=RailStatus.OK) for _ in range(size)
    )
    raw: bytes = _SETUP_ENCODER.encode(
        envelope(fold(Claim.STATIC, "check", outcomes), claim=Claim.STATIC, verb="check")
    )
    return (raw,)


def _decode_subject(payload: tuple[object, ...]) -> object:
    """Decode pre-encoded JSON bytes into a typed Envelope.

    Returns:
        Decoded Envelope from the given bytes.
    """
    (raw,) = payload
    assert isinstance(raw, bytes)
    return _WIRE_DECODER.decode(raw)


def _place_workload(size: int) -> tuple[object, ...]:
    """Build a Routed instance with ``size`` changed Python files for the place benchmark.

    Returns:
        Two-element tuple of (Routed, Tool) for the place subject.
    """
    files: tuple[str, ...] = tuple(f"pkg/mod_{i}.py" for i in range(size))
    routed = Routed(language=Language.PYTHON, scope=Scope.CHANGED, files=files)
    return (routed, _FILES_TOOL)


def _place_subject(payload: tuple[object, ...]) -> object:
    """Place routed files into command argument tails via a fresh isolated settings instance.

    Returns:
        Command argument tail groups for the tool input mode.
    """
    import tempfile  # noqa: PLC0415

    from upath import UPath  # noqa: PLC0415

    from tools.assay.composition.settings import AssaySettings  # noqa: PLC0415

    (routed, tool) = payload
    assert isinstance(routed, Routed)
    assert isinstance(tool, Tool)
    with tempfile.TemporaryDirectory() as tmp:
        settings: AssaySettings = AssaySettings(root=UPath(tmp), exec_target="", exec_known_hosts=None)
        return place(routed, tool, settings=settings)


def _roundtrip_workload(size: int) -> tuple[object, ...]:
    """Build a ``size``-receipt Envelope for the full encode+decode round-trip benchmark.

    Returns:
        One-element tuple carrying the pre-built Envelope.
    """
    outcomes: tuple[Completed, ...] = tuple(
        receipt(("t",), 0, status=RailStatus.OK) for _ in range(size)
    )
    env = envelope(fold(Claim.STATIC, "check", outcomes), claim=Claim.STATIC, verb="check")
    return (env,)


def _roundtrip_subject(payload: tuple[object, ...]) -> object:
    """Encode then decode an Envelope through the deterministic wire codec.

    Returns:
        Decoded Envelope equal to the input.
    """
    (env,) = payload
    assert isinstance(env, Envelope)
    raw = WIRE_ENCODER.encode(env)
    return _WIRE_DECODER.decode(raw)


# --- [MODELS] --------------------------------------------------------------------------

_ROWS: tuple[BenchSubject, ...] = (
    BenchSubject(
        label="resolve-strategy-build",
        subject=_resolve_subject,
        workload=lambda size: (size,),
        sizes=(10, 100, 500),
        budget_ms=200.0,
        rounds=5,
    ),
    BenchSubject(
        label="model-fold",
        subject=_fold_subject,
        workload=_fold_workload,
        sizes=(100, 1_000, 5_000),
        budget_ms=5.0,
        rounds=10,
    ),
    BenchSubject(
        label="wire-encode",
        subject=_encode_subject,
        workload=_encode_workload,
        sizes=(10, 100, 500),
        budget_ms=10.0,
        rounds=20,
    ),
    BenchSubject(
        label="wire-decode",
        subject=_decode_subject,
        workload=_decode_workload,
        sizes=(10, 100, 500),
        budget_ms=10.0,
        rounds=20,
    ),
    BenchSubject(
        label="wire-roundtrip",
        subject=_roundtrip_subject,
        workload=_roundtrip_workload,
        sizes=(10, 100, 500),
        budget_ms=20.0,
        rounds=10,
    ),
    BenchSubject(
        label="routing-place",
        subject=_place_subject,
        workload=_place_workload,
        sizes=(10, 100, 500),
        budget_ms=50.0,
        rounds=5,
    ),
)


# --- [COMPOSITION] ---------------------------------------------------------------------


@bench_params(_ROWS)
def bench_assay(benchmark: BenchmarkFixture, row: BenchSubject, size: int) -> None:
    """Drive the assay benchmark matrix: resolve, fold, encode, decode, roundtrip, and place."""
    run_bench(benchmark, row, size)
