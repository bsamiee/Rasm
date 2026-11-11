"""Instrumented process a case exports one span from, through the exporters its `OTEL_*` variables name."""

from collections.abc import Callable, Mapping
import functools
from inspect import cleandoc
from pathlib import Path
import subprocess
import sys

from opentelemetry.environment_variables import OTEL_LOGS_EXPORTER, OTEL_METRICS_EXPORTER
import pytest

# --- [OPERATIONS] -----------------------------------------------------------------------


def instrumented(folder: Path, environment: pytest.MonkeyPatch, variables: Mapping[str, str], /) -> None:
    """Runs a process that ends one span `a` inside the library's instrumented scope, under the given `OTEL_*` variables with metrics and logs unexported, to its exit."""
    for name, value in {OTEL_METRICS_EXPORTER: "none", OTEL_LOGS_EXPORTER: "none", **variables}.items():
        environment.setenv(name, value)
    program = cleandoc("""
        from datetime import timedelta
        import os
        from pathlib import Path
        import sys

        from opentelemetry import trace

        from rasm_telemetry import LogSettings
        from rasm_telemetry.process import instrumented

        with instrumented(Path(sys.argv[1]), service="suite", version="0.0.0", settings=LogSettings(), scopes=("suite",), environ=os.environ, release=timedelta(seconds=20)):
            trace.get_tracer("suite").start_span("a").end()
    """)
    subprocess.run((sys.executable, "-c", program, str(folder / "process.log")), check=True, timeout=25)


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def exported(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> Callable[[Mapping[str, str]], None]:
    """Returns the run of an instrumented process that exports one span `a` through the exporters its `OTEL_*` variables name."""
    return functools.partial(instrumented, tmp_path, monkeypatch)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = []
