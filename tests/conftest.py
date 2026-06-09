"""Shared pytest configuration for all repo-local Python test suites.

Registers hypothesis profiles (rasm, rasm-ci, rasm-stress, rasm-debug, rasm-mutation,
rasm-adversarial, rasm-stateful, rasm-parity), installs a session-scoped OpenTelemetry
TracerProvider backed by an InMemorySpanExporter, and wires optional observability and
CPU-profiler side channels controlled by ASSAY_OBSERVABILITY and ASSAY_PROFILE.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from datetime import datetime, UTC
import os
from pathlib import Path
import threading


ROOT = Path(__file__).resolve().parents[1]
REPO_ROOT = ROOT
_DEFAULT_HYPOTHESIS_HOME = ROOT / ".cache" / "hypothesis"
_hypothesis_storage_directory = os.environ.get("HYPOTHESIS_STORAGE_DIRECTORY")  # noqa: TID251  # precedes hypothesis import; locks storage path
HYPOTHESIS_HOME = Path(_hypothesis_storage_directory) if _hypothesis_storage_directory else _DEFAULT_HYPOTHESIS_HOME
os.environ.setdefault("HYPOTHESIS_STORAGE_DIRECTORY", str(HYPOTHESIS_HOME))  # noqa: TID251  # precedes hypothesis import in subprocess workers

# Must precede any hypothesis import; add_observability_callback fires on the first hypothesis.internal.observability import.
if os.environ.get("ASSAY_OBSERVABILITY"):  # noqa: TID251
    os.environ.setdefault("HYPOTHESIS_EXPERIMENTAL_OBSERVABILITY", "1")  # noqa: TID251  # must precede observability module import

from typing import TYPE_CHECKING

import anyio
from hypothesis import HealthCheck, is_hypothesis_test, Phase, settings as hyp_settings
from hypothesis.configuration import set_hypothesis_home_dir
from hypothesis.database import BackgroundWriteDatabase, DirectoryBasedExampleDatabase, GitHubArtifactDatabase, MultiplexedDatabase, ReadOnlyDatabase
from hypothesis.internal.observability import add_observability_callback  # no public re-export; hypothesis itself uses this path
from hypothesis.strategies._internal.utils import (  # noqa: PLC2701  # no public re-export; mirrors hypothesis._deliver_to_file's own import
    to_jsonable,
)
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

    from hypothesis.database import ExampleDatabase
    from structlog.types import EventDict


# --- [CONSTANTS] ------------------------------------------------------------------------

# HYPOTHESIS_EXAMPLES is a public constant: some test modules (e.g. W3) import it directly without fixture indirection.
HYPOTHESIS_EXAMPLES = HYPOTHESIS_HOME / "examples"
_SUPPRESSIONS = (HealthCheck.too_slow, HealthCheck.data_too_large)


# --- [SERVICES] -------------------------------------------------------------------------

_log = structlog.get_logger(__name__)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _build_profiler_argv(artifact_dir: Path, secs: str) -> list[str]:
    """Construct the profiling.sampling attach argv and ensure the artifact directory exists.

    Returns:
        Argv list ready for subprocess execution.
    """
    artifact_dir.mkdir(parents=True, exist_ok=True)
    ts = datetime.now(tz=UTC).strftime("%Y%m%dT%H%M%S")
    artifact = artifact_dir / f"session-{ts}.jsonl"
    return ["python", "-m", "profiling.sampling", "attach", str(os.getpid()), "--jsonl", "-o", str(artifact), "-d", secs]


def _run_profiler(artifact_dir: Path, secs: str) -> None:
    """Execute the profiler subprocess inside ``anyio.run`` on a daemon thread.

    Must not run on the pytest main thread: anyio.run would conflict with plugins
    that install their own event loop. Exceptions propagate to the thread's
    unhandled-exception handler (stderr) and never reach the session.
    """

    async def _attach(argv: list[str]) -> None:
        async with await anyio.open_process(argv, stdout=None, stderr=None) as proc:
            await proc.wait()

    argv = _build_profiler_argv(artifact_dir, secs)
    anyio.run(_attach, argv)


# --- [COMPOSITION] ----------------------------------------------------------------------


_local_db = BackgroundWriteDatabase(DirectoryBasedExampleDatabase(HYPOTHESIS_EXAMPLES))
# BackgroundWriteDatabase keeps writes off the critical path; RASM_HYPOTHESIS_GH_REPLAY layers a ReadOnlyDatabase
# over GitHubArtifactDatabase via MultiplexedDatabase to replay CI-found counterexamples locally.
_gh_replay = os.environ.get("RASM_HYPOTHESIS_GH_REPLAY")  # noqa: TID251  # CI replay gate
match _gh_replay.split("/", 1) if _gh_replay else []:
    case [owner, repo]:
        _EXAMPLE_DB: ExampleDatabase = MultiplexedDatabase(_local_db, ReadOnlyDatabase(GitHubArtifactDatabase(owner, repo)))
    case _:
        _EXAMPLE_DB = _local_db

set_hypothesis_home_dir(HYPOTHESIS_HOME)
hyp_settings.register_profile("rasm", database=_EXAMPLE_DB, deadline=None, suppress_health_check=_SUPPRESSIONS)
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
    # no DB: cached examples mask mutants; derandomized: stable kill verdicts; no shrink phase: wastes mutation budget
    "rasm-mutation",
    database=None,
    deadline=None,
    derandomize=True,
    max_examples=50,
    phases=(Phase.explicit, Phase.generate),
    suppress_health_check=_SUPPRESSIONS,
)
hyp_settings.register_profile(
    # deep budget drives hostile-input degradation laws to their boundary corners
    "rasm-adversarial",
    database=_EXAMPLE_DB,
    deadline=None,
    max_examples=2000,
    suppress_health_check=_SUPPRESSIONS,
)
hyp_settings.register_profile(
    # long step traces drive stateful RBSM interleavings to a minimal counterexample
    "rasm-stateful",
    database=_EXAMPLE_DB,
    deadline=None,
    stateful_step_count=200,
    suppress_health_check=_SUPPRESSIONS,
)
hyp_settings.register_profile(
    # derandomized for byte-stable cross-tool comparison; database=None is mandatory under derandomize
    "rasm-parity",
    database=None,
    deadline=None,
    derandomize=True,
    suppress_health_check=_SUPPRESSIONS,
)
# hypothesis registers its own _deliver_to_file callback on import; this second callback redirects observations to the repo artifacts tree.
if os.environ.get("ASSAY_OBSERVABILITY"):  # noqa: TID251
    _OBS_DIR = ROOT / ".artifacts" / "python" / "observed"

    def _deliver_to_artifacts(observation: object) -> None:
        kind = "testcases" if getattr(observation, "type", None) == "test_case" else "info"
        _OBS_DIR.mkdir(parents=True, exist_ok=True)
        artifact = _OBS_DIR / f"{datetime.now(tz=UTC).date().isoformat()}_{kind}.jsonl"
        with artifact.open(mode="a") as fh:
            fh.write(msgspec.json.encode(to_jsonable(observation, avoid_realization=False)).decode() + "\n")

    add_observability_callback(_deliver_to_artifacts, all_threads=True)


def pytest_collection_modifyitems(items: list[pytest.Item]) -> None:
    """Auto-apply ``network`` and ``property`` markers from fixture and Hypothesis membership.

    ``pytest-socket``'s ``socket_enabled`` fixture lifts ``--disable-socket`` per test, but
    without this hook those tests are invisible to ``-m network`` selection. Hypothesis-backed
    tests gain ``property`` so ``-m property`` selection is total without per-test decorators.
    """
    network = pytest.mark.network
    property_ = pytest.mark.property
    for item in items:
        if "socket_enabled" in getattr(item, "fixturenames", ()):
            item.add_marker(network, append=False)
        fn = getattr(item, "function", None)
        if fn is not None and is_hypothesis_test(fn):
            item.add_marker(property_, append=False)


# --- [EXPORTS] --------------------------------------------------------------------------


@pytest.fixture(scope="session", autouse=True)
def _otel_provider() -> Generator[InMemorySpanExporter]:
    """Install a real TracerProvider once per session; ``SimpleSpanProcessor`` ensures synchronous export.

    Placed in root conftest so it runs before engine/aspect module-scope imports that bind the
    ProxyTracer — pytest guarantees root conftest fixtures execute before sub-conftest fixtures.

    Yields:
        ``InMemorySpanExporter`` accumulating all spans for the session.
    """
    exp = InMemorySpanExporter()
    tp = TracerProvider()
    tp.add_span_processor(SimpleSpanProcessor(exp))
    otel_trace.set_tracer_provider(tp)
    yield exp
    tp.shutdown()


@pytest.fixture(scope="session", autouse=True)
def _profiling_sampler(_otel_provider: InMemorySpanExporter) -> None:
    """Best-effort out-of-process CPU sampler for the test session PID.

    Activated when ``ASSAY_PROFILE`` is non-empty. Runs in a daemon thread so
    ``anyio.run`` does not conflict with main-thread event loops. Any failure is
    absorbed in the thread target and never reaches the session.
    """
    profile_flag = os.environ.get("ASSAY_PROFILE")  # noqa: TID251  # profiler activation gate
    if not profile_flag:
        return
    secs = os.environ.get("ASSAY_PROFILE_SECS", "60")  # noqa: TID251  # sampling duration override
    artifact_dir = ROOT / ".artifacts" / "python" / "profile"

    def _spawn() -> None:
        _run_profiler(artifact_dir, secs)

    threading.Thread(target=_spawn, daemon=True, name="assay-profiler").start()


@pytest.fixture
def otel_spans(_otel_provider: InMemorySpanExporter) -> InMemorySpanExporter:
    """Clear and return the session exporter for one test.

    Returns:
        Exporter containing only spans recorded after this fixture starts.
    """
    _otel_provider.clear()
    return _otel_provider


@pytest.fixture
def log_events() -> Generator[list[EventDict]]:
    """Capture structlog events as plain dicts for one test via ``capture_logs()``.

    INCOMPATIBLE with ``ring_processor`` assertions in the same test: ``capture_logs`` replaces the
    full processor chain. Use ``_RING.set(deque(...))`` for ring-content assertions instead.

    Yields:
        Mutable list of captured log event dicts; each entry has ``{'event': str, 'log_level': str, ...}``.
    """
    with capture_logs() as events:
        yield events
