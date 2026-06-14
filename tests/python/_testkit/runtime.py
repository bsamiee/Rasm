"""Pytest runtime plugin for Hypothesis, benchmark hooks, tracing, and optional diagnostics."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from datetime import datetime, UTC
import importlib
import os
from pathlib import Path
import threading


REPO_ROOT = Path(__file__).resolve().parents[3]
_DEFAULT_HYPOTHESIS_HOME = REPO_ROOT / ".cache" / "hypothesis"
_hypothesis_storage_directory = os.environ.get("HYPOTHESIS_STORAGE_DIRECTORY")  # noqa: TID251  # precedes hypothesis import; locks storage path
HYPOTHESIS_HOME = Path(_hypothesis_storage_directory) if _hypothesis_storage_directory else _DEFAULT_HYPOTHESIS_HOME
os.environ.setdefault("HYPOTHESIS_STORAGE_DIRECTORY", str(HYPOTHESIS_HOME))  # noqa: TID251  # precedes hypothesis import in subprocess workers

# Must precede any Hypothesis import; the observability callback is installed on first internal import.
if os.environ.get("TESTS_OBSERVABILITY"):  # noqa: TID251
    os.environ.setdefault("HYPOTHESIS_EXPERIMENTAL_OBSERVABILITY", "1")  # noqa: TID251  # must precede observability module import

from contextlib import contextmanager
from typing import TYPE_CHECKING

import anyio
from hypothesis import HealthCheck, is_hypothesis_test, Phase, settings as hyp_settings
from hypothesis.configuration import set_hypothesis_home_dir
from hypothesis.database import BackgroundWriteDatabase, DirectoryBasedExampleDatabase, GitHubArtifactDatabase, MultiplexedDatabase, ReadOnlyDatabase
from hypothesis.internal.observability import add_observability_callback  # no public re-export; hypothesis itself uses this path
from hypothesis.strategies._internal.utils import to_jsonable  # no public re-export; mirrors hypothesis._deliver_to_file's own import
import msgspec.json
from opentelemetry import trace as otel_trace
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import SimpleSpanProcessor
from opentelemetry.sdk.trace.export.in_memory_span_exporter import InMemorySpanExporter
import pytest
import structlog
from structlog.testing import capture_logs


if TYPE_CHECKING:
    from collections.abc import Generator
    from contextvars import ContextVar

    from hypothesis.database import ExampleDatabase
    from structlog.types import EventDict, Processor


# --- [CONSTANTS] ------------------------------------------------------------------------

# Public constant imported directly by suites that cannot use fixture indirection.
HYPOTHESIS_EXAMPLES = HYPOTHESIS_HOME / "examples"
_SUPPRESSIONS = (HealthCheck.too_slow, HealthCheck.data_too_large)

# Symbolic profile vocabulary for non-pytest consumers and model-based profile resolution.
PROFILE_DEFAULT = "rasm"
PROFILE_MUTATION = "rasm-mutation"
PROFILE_STATEFUL = "rasm-stateful"

# --- [SERVICES] -------------------------------------------------------------------------

_log = structlog.get_logger(__name__)

# --- [OPERATIONS] -----------------------------------------------------------------------


@contextmanager
def isolate[T](var: ContextVar[T], value: T) -> Generator[None]:
    """Pin a ``ContextVar`` binding for the enclosed block.

    Yields:
        None while ``var`` is bound to ``value``.
    """
    token = var.set(value)
    try:
        yield
    finally:
        var.reset(token)


def _build_profiler_argv(artifact_dir: Path, secs: str) -> list[str]:
    """Construct the ``profiling.sampling attach`` argv and ensure the artifact directory exists.

    Returns:
        Argv list ready for subprocess execution.
    """
    artifact_dir.mkdir(parents=True, exist_ok=True)
    ts = datetime.now(tz=UTC).strftime("%Y%m%dT%H%M%S")
    artifact = artifact_dir / f"session-{ts}.jsonl"
    return ["python", "-m", "profiling.sampling", "attach", str(os.getpid()), "--jsonl", "-o", str(artifact), "-d", secs]


def _run_profiler(artifact_dir: Path, secs: str) -> None:
    """Execute the profiler subprocess inside ``anyio.run`` off the pytest main thread."""

    async def _attach(argv: list[str]) -> None:
        async with await anyio.open_process(argv, stdout=None, stderr=None) as proc:
            await proc.wait()

    argv = _build_profiler_argv(artifact_dir, secs)
    anyio.run(_attach, argv)


# --- [COMPOSITION] ----------------------------------------------------------------------

_local_db = BackgroundWriteDatabase(DirectoryBasedExampleDatabase(HYPOTHESIS_EXAMPLES))
# Background writes stay off the critical path; optional GH replay is read-only.
_gh_replay = os.environ.get("RASM_HYPOTHESIS_GH_REPLAY")  # noqa: TID251  # CI replay gate
match _gh_replay.split("/", 1) if _gh_replay else []:
    case [owner, repo]:
        _EXAMPLE_DB: ExampleDatabase = MultiplexedDatabase(_local_db, ReadOnlyDatabase(GitHubArtifactDatabase(owner, repo)))
    case _:
        _EXAMPLE_DB = _local_db

set_hypothesis_home_dir(HYPOTHESIS_HOME)
hyp_settings.register_profile(PROFILE_DEFAULT, database=_EXAMPLE_DB, deadline=None, suppress_health_check=_SUPPRESSIONS)
hyp_settings.register_profile("rasm-ci", database=_EXAMPLE_DB, deadline=None, max_examples=200, suppress_health_check=_SUPPRESSIONS)
hyp_settings.register_profile(
    "rasm-stress",
    database=_EXAMPLE_DB,
    deadline=None,
    max_examples=1000,
    phases=(Phase.explicit, Phase.reuse, Phase.generate, Phase.target, Phase.shrink, Phase.explain),
    suppress_health_check=_SUPPRESSIONS,
)
hyp_settings.register_profile(
    "rasm-debug",
    database=_EXAMPLE_DB,
    deadline=None,
    max_examples=25,
    phases=(Phase.explicit, Phase.reuse, Phase.generate, Phase.explain),
    suppress_health_check=_SUPPRESSIONS,
)
hyp_settings.register_profile(
    # Mutation runs need stable, cache-free verdicts and short state traces to preserve kill-signal budget.
    PROFILE_MUTATION,
    database=None,
    deadline=None,
    derandomize=True,
    max_examples=50,
    stateful_step_count=20,
    phases=(Phase.explicit, Phase.generate),
    suppress_health_check=_SUPPRESSIONS,
)
hyp_settings.register_profile(
    # Hostile-input degradation laws need a deeper example budget.
    "rasm-adversarial",
    database=_EXAMPLE_DB,
    deadline=None,
    max_examples=2000,
    suppress_health_check=_SUPPRESSIONS,
)
hyp_settings.register_profile(
    # Stateful laws need long traces to minimize interleaving counterexamples.
    PROFILE_STATEFUL,
    database=_EXAMPLE_DB,
    deadline=None,
    stateful_step_count=200,
    suppress_health_check=_SUPPRESSIONS,
)
hyp_settings.register_profile(
    # Derandomized parity requires database=None for byte-stable cross-tool comparison.
    "rasm-parity",
    database=None,
    deadline=None,
    derandomize=True,
    suppress_health_check=_SUPPRESSIONS,
)
# Redirect Hypothesis observations to repo artifacts without replacing its built-in callback.
if os.environ.get("TESTS_OBSERVABILITY"):  # noqa: TID251
    _OBS_DIR = REPO_ROOT / ".artifacts" / "python" / "observed"

    def _deliver_to_artifacts(observation: object) -> None:
        kind = "testcases" if getattr(observation, "type", None) == "test_case" else "info"
        _OBS_DIR.mkdir(parents=True, exist_ok=True)
        artifact = _OBS_DIR / f"{datetime.now(tz=UTC).date().isoformat()}_{kind}.jsonl"
        with artifact.open(mode="a") as fh:
            fh.write(msgspec.json.encode(to_jsonable(observation, avoid_realization=False)).decode() + "\n")

    add_observability_callback(_deliver_to_artifacts, all_threads=True)


def pytest_configure(config: pytest.Config) -> None:
    """Register the bench module's regression hook when pytest-benchmark is loaded."""
    (
        config.pluginmanager.register(importlib.import_module("tests.python._testkit.bench"), "testkit-bench")
        if hasattr(config.pluginmanager.hook, "pytest_benchmark_update_json") and not config.pluginmanager.hasplugin("testkit-bench")
        else None
    )


def pytest_collection_modifyitems(items: list[pytest.Item]) -> None:
    """Auto-apply ``network`` and ``property`` markers from fixture and Hypothesis membership."""
    network = pytest.mark.network
    property_ = pytest.mark.property
    for item in items:
        if "socket_enabled" in getattr(item, "fixturenames", ()):
            item.add_marker(network, append=False)
        fn = getattr(item, "function", None)
        if fn is not None and is_hypothesis_test(fn):
            item.add_marker(property_, append=False)


@pytest.fixture(scope="session", autouse=True)
def _otel_provider() -> InMemorySpanExporter:
    """Attach a fresh exporter to the set-once process-level ``TracerProvider``.

    Returns:
        Session exporter receiving synchronously processed spans.
    """
    match otel_trace.get_tracer_provider():
        case TracerProvider() as live:
            tp = live
        case _:
            tp = TracerProvider()
            otel_trace.set_tracer_provider(tp)
    exp = InMemorySpanExporter()
    tp.add_span_processor(SimpleSpanProcessor(exp))
    return exp


@pytest.fixture(scope="session", autouse=True)
def _profiling_sampler(_otel_provider: InMemorySpanExporter) -> None:
    """Start the optional out-of-process CPU sampler for the test session PID."""
    profile_flag = os.environ.get("TESTS_PROFILE")  # noqa: TID251  # profiler activation gate
    if not profile_flag:
        return
    secs = os.environ.get("TESTS_PROFILE_SECS", "60")  # noqa: TID251  # sampling duration override
    artifact_dir = REPO_ROOT / ".artifacts" / "python" / "profile"

    def _spawn() -> None:
        _run_profiler(artifact_dir, secs)

    threading.Thread(target=_spawn, daemon=True, name="tests-profiler").start()


@pytest.fixture
def otel_spans(_otel_provider: InMemorySpanExporter) -> InMemorySpanExporter:
    """Clear and return the session exporter for one test.

    Returns:
        Exporter containing spans recorded after this fixture starts.
    """
    _otel_provider.clear()
    return _otel_provider


@pytest.fixture
def log_processors() -> tuple[Processor, ...]:
    """Default processor chain spliced before the ``log_events`` capture sink.

    Returns:
        Empty tuple unless a project conftest overrides the fixture.
    """
    return ()


@pytest.fixture
def log_events(log_processors: tuple[Processor, ...]) -> Generator[list[EventDict]]:
    """Capture structlog events as plain dictionaries for one test.

    Yields:
        Mutable list populated by ``capture_logs(processors=...)``.
    """
    with capture_logs(processors=log_processors) as events:
        yield events


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["REPO_ROOT", "HYPOTHESIS_HOME", "HYPOTHESIS_EXAMPLES", "PROFILE_DEFAULT", "PROFILE_MUTATION", "PROFILE_STATEFUL", "isolate"]
