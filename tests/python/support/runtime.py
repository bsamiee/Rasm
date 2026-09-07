"""Pytest runtime plugin for Hypothesis, benchmark hooks, tracing, and optional diagnostics."""

# --- [IMPORTS] --------------------------------------------------------------------------

from datetime import datetime, UTC
import importlib
import os
from pathlib import Path
import sys
import threading

from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict

REPO_ROOT: Path = next(parent for parent in Path(__file__).resolve().parents if (parent / "uv.lock").is_file())


class Settings(BaseSettings):
    """Environment variables the plugin reads once at startup, before the hypothesis import that reads two of them."""

    model_config = SettingsConfigDict(case_sensitive=True)

    hypothesis_storage_directory: Path = Field(default=REPO_ROOT / ".cache" / "hypothesis", validation_alias="HYPOTHESIS_STORAGE_DIRECTORY")
    hypothesis_gh_replay: str = Field(default="", validation_alias="HYPOTHESIS_GH_REPLAY")
    tests_observability: bool = Field(default=False, validation_alias="TESTS_OBSERVABILITY")
    tests_profile: bool = Field(default=False, validation_alias="TESTS_PROFILE")
    tests_profile_secs: int = Field(default=60, validation_alias="TESTS_PROFILE_SECS")


_settings = Settings()
HYPOTHESIS_HOME = _settings.hypothesis_storage_directory
os.environ.setdefault("HYPOTHESIS_STORAGE_DIRECTORY", str(HYPOTHESIS_HOME))  # ast-grep-ignore: no-os-environ, written before the hypothesis import
if _settings.tests_observability:
    os.environ.setdefault("HYPOTHESIS_EXPERIMENTAL_OBSERVABILITY", "1")  # ast-grep-ignore: no-os-environ, written before the hypothesis import

from typing import TYPE_CHECKING

import anyio
from hypothesis import HealthCheck, is_hypothesis_test, Phase, settings as hyp_settings
from hypothesis.configuration import set_hypothesis_home_dir
from hypothesis.database import BackgroundWriteDatabase, DirectoryBasedExampleDatabase, GitHubArtifactDatabase, MultiplexedDatabase, ReadOnlyDatabase
from hypothesis.internal.observability import add_observability_callback
from hypothesis.strategies._internal.utils import to_jsonable
import msgspec.json
from opentelemetry import trace as otel_trace
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import SimpleSpanProcessor
from opentelemetry.sdk.trace.export.in_memory_span_exporter import InMemorySpanExporter
import pytest
import structlog
from structlog.testing import capture_logs

lazy from tests.python.support.properties import PROPERTY_RECORDS, PropertyRecord, record_coverage_declarations

if TYPE_CHECKING:
    from collections.abc import Generator

    from hypothesis.database import ExampleDatabase
    from structlog.types import EventDict, Processor


# --- [CONSTANTS] ------------------------------------------------------------------------

_SUPPRESSIONS = (HealthCheck.too_slow, HealthCheck.data_too_large, HealthCheck.filter_too_much)

PROFILE_DEFAULT = "default"
PROFILE_STATEFUL = "stateful"

# --- [SERVICES] -------------------------------------------------------------------------

_log = structlog.get_logger(__name__)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _run_profiler(artifact_dir: Path, secs: int) -> None:
    """Attach the stdlib sampling profiler to the session PID from a child process, off the pytest main thread."""
    artifact_dir.mkdir(parents=True, exist_ok=True)
    artifact = artifact_dir / f"session-{datetime.now(tz=UTC).strftime('%Y%m%dT%H%M%S')}.jsonl"
    argv = [sys.executable, "-m", "profiling.sampling", "attach", str(os.getpid()), "--jsonl", "-o", str(artifact), "-d", str(secs)]

    async def _attach() -> None:
        async with await anyio.open_process(argv, stdout=None, stderr=None) as proc:
            await proc.wait()

    anyio.run(_attach)


# --- [COMPOSITION] ----------------------------------------------------------------------

_local_db = BackgroundWriteDatabase(DirectoryBasedExampleDatabase(HYPOTHESIS_HOME / "examples"))
match _settings.hypothesis_gh_replay.split("/", 1):
    case [owner, repo]:
        _EXAMPLE_DB: ExampleDatabase = MultiplexedDatabase(_local_db, ReadOnlyDatabase(GitHubArtifactDatabase(owner, repo)))
    case _:
        _EXAMPLE_DB = _local_db

set_hypothesis_home_dir(HYPOTHESIS_HOME)
hyp_settings.register_profile(PROFILE_DEFAULT, database=_EXAMPLE_DB, deadline=None, suppress_health_check=_SUPPRESSIONS)
hyp_settings.register_profile("ci", database=_EXAMPLE_DB, deadline=None, max_examples=200, suppress_health_check=_SUPPRESSIONS)
hyp_settings.register_profile(
    "stress",
    database=_EXAMPLE_DB,
    deadline=None,
    max_examples=1000,
    phases=(Phase.explicit, Phase.reuse, Phase.generate, Phase.target, Phase.shrink, Phase.explain),
    suppress_health_check=_SUPPRESSIONS,
)
hyp_settings.register_profile(
    "debug",
    database=_EXAMPLE_DB,
    deadline=None,
    max_examples=25,
    phases=(Phase.explicit, Phase.reuse, Phase.generate, Phase.explain),
    suppress_health_check=_SUPPRESSIONS,
)
hyp_settings.register_profile("adversarial", database=_EXAMPLE_DB, deadline=None, max_examples=2000, suppress_health_check=_SUPPRESSIONS)
hyp_settings.register_profile(PROFILE_STATEFUL, database=_EXAMPLE_DB, deadline=None, stateful_step_count=200, suppress_health_check=_SUPPRESSIONS)
hyp_settings.register_profile("parity", database=None, deadline=None, derandomize=True, suppress_health_check=_SUPPRESSIONS)
if _settings.tests_observability:
    _OBSERVATIONS = REPO_ROOT / ".artifacts" / "python" / "hypothesis"

    def _write_observation(observation: object, _thread_id: int) -> None:
        kind = "testcases" if getattr(observation, "type", None) == "test_case" else "info"
        _OBSERVATIONS.mkdir(parents=True, exist_ok=True)
        artifact = _OBSERVATIONS / f"{datetime.now(tz=UTC).date().isoformat()}_{kind}.jsonl"
        with artifact.open(mode="a") as fh:
            fh.write(msgspec.json.encode(to_jsonable(observation, avoid_realization=False)).decode() + "\n")

    add_observability_callback(_write_observation, all_threads=True)


def pytest_configure(config: pytest.Config) -> None:
    """Register the bench module's regression hook when pytest-benchmark is loaded."""
    if hasattr(config.pluginmanager.hook, "pytest_benchmark_update_json") and not config.pluginmanager.hasplugin("test-support-bench"):
        config.pluginmanager.register(importlib.import_module("tests.python.support.bench"), "test-support-bench")


def pytest_collection_modifyitems(config: pytest.Config, items: list[pytest.Item]) -> None:
    """Apply network and property markers and record the property tests and each module's ``COVERS`` declarations once."""
    network = pytest.mark.network
    property_ = pytest.mark.property
    for item in items:
        if "socket_enabled" in getattr(item, "fixturenames", ()):
            item.add_marker(network, append=False)
        fn = getattr(item, "function", None)
        if fn is not None and is_hypothesis_test(fn):
            item.add_marker(property_, append=False)
    config.stash[PROPERTY_RECORDS] = (
        *(record for item in items for mark in item.iter_markers("property") if isinstance(record := mark.kwargs.get("record"), PropertyRecord)),
        *(record for module in dict.fromkeys(getattr(item, "module", None) for item in items) for record in record_coverage_declarations(module)),
    )


@pytest.fixture(scope="session")
def _otel_provider() -> InMemorySpanExporter:
    """Attach a fresh in-memory exporter to the set-once process-level ``TracerProvider``."""
    match otel_trace.get_tracer_provider():
        case TracerProvider() as live:
            tracer_provider = live
        case _:
            tracer_provider = TracerProvider()
            otel_trace.set_tracer_provider(tracer_provider)
    exporter = InMemorySpanExporter()
    tracer_provider.add_span_processor(SimpleSpanProcessor(exporter))
    return exporter


def pytest_sessionstart() -> None:
    """Start the optional out-of-process CPU sampler for the test session PID."""
    if _settings.tests_profile:
        artifact_dir = REPO_ROOT / ".artifacts" / "python" / "profile"
        threading.Thread(target=_run_profiler, args=(artifact_dir, _settings.tests_profile_secs), daemon=True, name="tests-profiler").start()


@pytest.fixture
def otel_spans(_otel_provider: InMemorySpanExporter) -> InMemorySpanExporter:
    """Clear and return the session exporter for a test."""
    _otel_provider.clear()
    return _otel_provider


@pytest.fixture(scope="session")
def energyplus() -> Path:
    """Point honeybee-energy at the EnergyPlus installation provision links under the tools directory and return that folder."""
    folder = REPO_ROOT / ".cache" / "tools" / "energyplus"
    if not folder.is_dir():
        pytest.skip("EnergyPlus is not provisioned, run nx run eng:provision")
    from honeybee_energy.config import folders  # ruff:ignore[import-outside-top-level] -- honeybee-energy imports slowly, tests that need it pay for it

    folders.energyplus_path = str(folder)
    return folder


@pytest.fixture
def log_processors() -> tuple[Processor, ...]:
    """Processors inserted before the ``log_events`` capture sink, empty unless a package conftest overrides it."""
    return ()


@pytest.fixture
def log_events(log_processors: tuple[Processor, ...]) -> Generator[list[EventDict]]:
    """Capture structlog events as plain dictionaries for a test."""
    with capture_logs(processors=log_processors) as events:
        yield events


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["REPO_ROOT", "HYPOTHESIS_HOME", "PROFILE_DEFAULT", "PROFILE_STATEFUL", "Settings"]
