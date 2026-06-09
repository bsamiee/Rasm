"""Shared pytest configuration for repo-local Python tests."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from datetime import datetime, UTC
import os
from pathlib import Path
import threading


ROOT = Path(__file__).resolve().parents[1]
REPO_ROOT = ROOT
_DEFAULT_HYPOTHESIS_HOME = ROOT / ".cache" / "hypothesis"
_hypothesis_storage_directory = os.environ.get("HYPOTHESIS_STORAGE_DIRECTORY")  # noqa: TID251  # pytest boundary: anchor Hypothesis before import
HYPOTHESIS_HOME = Path(_hypothesis_storage_directory) if _hypothesis_storage_directory else _DEFAULT_HYPOTHESIS_HOME
os.environ.setdefault("HYPOTHESIS_STORAGE_DIRECTORY", str(HYPOTHESIS_HOME))  # noqa: TID251  # pytest boundary: seed subprocess home before import

# Rank 18: opt-in observability artifact lane — must be set BEFORE hypothesis.internal.observability
# is first imported so the module-level callback registration fires on that import.
# Default off; zero overhead when unset. Activate with ASSAY_OBSERVABILITY=1.
if os.environ.get("ASSAY_OBSERVABILITY"):  # noqa: TID251  # pytest boundary: observability gate before hypothesis import
    os.environ.setdefault("HYPOTHESIS_EXPERIMENTAL_OBSERVABILITY", "1")  # noqa: TID251

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

    from structlog.types import EventDict


# --- [CONSTANTS] ------------------------------------------------------------------------
# Repo-local Hypothesis/cache anchors W3 reads directly (no fixture indirection over importable constants).
# Optional profiles: ``--hypothesis-profile=rasm-ci|rasm-stress|rasm-debug|rasm-adversarial|rasm-stateful|rasm-parity``;
# mutmut uses ``rasm-mutation`` via ``[tool.mutmut]`` pytest args.


HYPOTHESIS_EXAMPLES = HYPOTHESIS_HOME / "examples"
# Rank 16: BackgroundWriteDatabase keeps adversarial-profile (max_examples=2000) writes off the
# critical path. Behind RASM_HYPOTHESIS_GH_REPLAY, a ReadOnlyDatabase(GitHubArtifactDatabase(...))
# layer replays CI-found counterexamples locally via MultiplexedDatabase. rasm-mutation and
# rasm-parity profiles deliberately pass database=None and are unaffected.
# register_random: no private random.Random seam exists in the engines at this time; call it here
# (strong ref required — hypothesis weakrefs the instance) if one is introduced.
_local_db = BackgroundWriteDatabase(DirectoryBasedExampleDatabase(HYPOTHESIS_EXAMPLES))
_gh_replay = os.environ.get("RASM_HYPOTHESIS_GH_REPLAY")  # noqa: TID251  # test boundary: CI replay gate
_EXAMPLE_DB = (
    MultiplexedDatabase(_local_db, ReadOnlyDatabase(GitHubArtifactDatabase(*_gh_replay.split("/", 1))))
    if _gh_replay and "/" in _gh_replay
    else _local_db
)
_SUPPRESSIONS = (HealthCheck.too_slow, HealthCheck.data_too_large)
_log = structlog.get_logger(__name__)


# --- [COMPOSITION] ----------------------------------------------------------------------


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
    # Mutation runs: no shared example DB (cached examples must not mask mutants), derandomized for stable kill verdicts,
    # explicit+generate only (shrinking a survivor wastes the mutation budget; a single killing example suffices).
    "rasm-mutation",
    database=None,
    deadline=None,
    derandomize=True,
    max_examples=50,
    phases=(Phase.explicit, Phase.generate),
    suppress_health_check=_SUPPRESSIONS,
)
hyp_settings.register_profile(
    # Adversarial corpus: deep example budget to drive the hostile-input degradation laws into their corners.
    "rasm-adversarial",
    database=_EXAMPLE_DB,
    deadline=None,
    max_examples=2000,
    suppress_health_check=_SUPPRESSIONS,
)
hyp_settings.register_profile(
    # Stateful RBSMs (lease / tick / history): long step traces so interleavings shrink to a minimal counterexample.
    "rasm-stateful",
    database=_EXAMPLE_DB,
    deadline=None,
    stateful_step_count=200,
    suppress_health_check=_SUPPRESSIONS,
)
hyp_settings.register_profile(
    # A/B parity: derandomized (database=None is mandatory under derandomize) for byte-stable cross-tool comparison.
    "rasm-parity",
    database=None,
    deadline=None,
    derandomize=True,
    suppress_health_check=_SUPPRESSIONS,
)
# Rank 19 (future, no dep now): a rasm-symbolic profile (@settings(backend="crosshair")) over
# SMT-tractable pure _spec laws (arithmetic roundtrip / monotone / inverse) will be enabled when a
# cp315 crosshair-tool wheel ships. CrossHair integrates as a Hypothesis BACKEND via the
# hypothesis.internal.conjecture.engine AVAILABLE_PROVIDERS registry — zero engine change required,
# conftest-only. Gate behind a pytest marker to avoid accidental activation on normal runs.

# Rank 18: when ASSAY_OBSERVABILITY is set, redirect jsonlines observations to .artifacts/python/observed/
# alongside the coverage artifacts. The env var was set before the hypothesis import above so the
# module-level _deliver_to_file callback already registered; the custom callback below additionally
# writes the same payload to the repo artifacts tree for agent-readable evidence.
if os.environ.get("ASSAY_OBSERVABILITY"):  # noqa: TID251  # test boundary: observability artifact redirect
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

    Generic to every suite under this root (assay, quality, future py projects): ``pytest-socket``'s
    ``socket_enabled`` fixture lifts ``--disable-socket`` per test, but without this hook those tests
    are invisible to ``-m network`` selection. Hypothesis-backed tests gain ``property`` so ``-m property``
    selection stays total without per-test decorators.
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

    ``autouse=True``: every test records real spans without opt-in. Placed in root conftest so it runs
    before engine/aspect module-scope imports that bind the ProxyTracer — pytest guarantees root conftest
    fixtures execute before sub-conftest fixtures.

    Yields:
        The ``InMemorySpanExporter`` backed by the session-scoped ``TracerProvider``.
    """
    exp = InMemorySpanExporter()
    tp = TracerProvider()
    tp.add_span_processor(SimpleSpanProcessor(exp))
    otel_trace.set_tracer_provider(tp)
    yield exp
    tp.shutdown()


def _build_profiler_argv(artifact_dir: Path, secs: str) -> list[str]:
    """Construct the profiling.sampling attach argv and ensure the output directory exists.

    Args:
        artifact_dir: Directory under which the JSONL artifact is written.
        secs: Sampling duration as a string (passed verbatim to ``-d``).

    Returns:
        Argv list ready for subprocess execution.
    """
    artifact_dir.mkdir(parents=True, exist_ok=True)
    ts = datetime.now(tz=UTC).strftime("%Y%m%dT%H%M%S")
    artifact = artifact_dir / f"session-{ts}.jsonl"
    return ["python", "-m", "profiling.sampling", "attach", str(os.getpid()), "--jsonl", "-o", str(artifact), "-d", secs]


def _run_profiler(artifact_dir: Path, secs: str) -> None:
    """Execute the profiler subprocess inside ``anyio.run``.

    Any exception propagates to the daemon thread's unhandled-exception handler which
    writes to stderr — acceptable for observability-only infrastructure. The daemon
    thread cannot crash the session regardless of outcome.

    Args:
        artifact_dir: Directory for the JSONL output artifact.
        secs: Sampling duration forwarded to the subprocess.
    """

    async def _attach(argv: list[str]) -> None:
        async with await anyio.open_process(argv, stdout=None, stderr=None) as proc:
            await proc.wait()

    argv = _build_profiler_argv(artifact_dir, secs)
    anyio.run(_attach, argv)


@pytest.fixture(scope="session", autouse=True)
def _profiling_sampler(_otel_provider: InMemorySpanExporter) -> None:
    """Best-effort out-of-process CPU sampler for the test session PID.

    Activated only when ``ASSAY_PROFILE`` is non-empty in the environment. Spawns
    ``python -m profiling.sampling attach <pid> --jsonl -o <artifact> -d <secs>`` in a
    daemon thread so the subprocess does not block the session or its teardown.
    Default duration is 60 s; override with ``ASSAY_PROFILE_SECS``.

    WHY a daemon thread: ``anyio.run`` must not be called on the pytest main thread
    (which may already have an event loop in some plugins), and the subprocess is
    fire-and-forget — we never need its exit code or output.

    WHY exceptions absorbed in thread target: any failure (missing wheel, platform
    restriction, permission error) is logged via structlog to stderr and must never
    propagate out of the daemon thread into the session.
    """
    profile_flag = os.environ.get("ASSAY_PROFILE")  # noqa: TID251  # test boundary: env gate
    if not profile_flag:
        return
    secs = os.environ.get("ASSAY_PROFILE_SECS", "60")  # noqa: TID251  # test boundary: duration override
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
