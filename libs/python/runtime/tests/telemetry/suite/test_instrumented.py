"""Process-wide OpenTelemetry setup on entering `rasm_telemetry.process.instrumented`, copied from the opentelemetry-sdk file routing and tracer, OTLP/HTTP span exporter, splunk-opentelemetry configurator, and CPython incremental logging configuration tests the cases name."""

from collections.abc import Callable
from contextlib import AbstractContextManager
import itertools
import logging
import os
from pathlib import Path
import subprocess
import sys
from unittest import mock
from unittest.mock import MagicMock, Mock, patch

import msgspec
from opentelemetry import trace
from opentelemetry.configuration import ConfigurationError
from opentelemetry.environment_variables import OTEL_LOGS_EXPORTER, OTEL_METRICS_EXPORTER, OTEL_TRACES_EXPORTER
from opentelemetry.sdk.environment_variables import _OTEL_PYTHON_EXPORTER_OTLP_HTTP_TRACES_CREDENTIAL_PROVIDER, OTEL_CONFIG_FILE, OTEL_SDK_DISABLED
from opentelemetry.test.mock_test_classes import IterEntryPoint
import pytest

from rasm_telemetry import LogSettings
from rasm_telemetry.process import instrumented

# --- [TYPES] ----------------------------------------------------------------------------

type Configure = Callable[[], AbstractContextManager[Callable[[LogSettings], None]]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_OTLP_HTTP_TRACES = {OTEL_TRACES_EXPORTER: "otlp_proto_http", OTEL_METRICS_EXPORTER: "none", OTEL_LOGS_EXPORTER: "none"}

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def configure(tmp_path: Path) -> Configure:
    """Returns the call making the process scope over a log file, as rasm-cli enters it, standing where an exemplar calls its configurator's `_configure`."""
    return lambda: instrumented(tmp_path / "scope.log", service="rasm-telemetry-tests", version="0", settings=LogSettings(), scopes=())


@patch.dict("os.environ", {OTEL_CONFIG_FILE: "/tmp/otel.yaml"})
@patch("opentelemetry.sdk._configuration._initialize_components")
def test_env_var_set_routes_to_declarative_path(mock_init_components: MagicMock, configure: Configure) -> None:
    """`OTEL_CONFIG_FILE` loads the file and configures the SDK from it alone (`test_configurator_file_routing.py:47-59`)."""
    sentinel_config = object()
    with patch("opentelemetry.configuration.load_config_file", return_value=sentinel_config) as load_config_file, patch("opentelemetry.configuration.configure_sdk") as configure_sdk, configure():
        pass
    load_config_file.assert_called_once_with("/tmp/otel.yaml")
    configure_sdk.assert_called_once_with(sentinel_config)
    mock_init_components.assert_not_called()


@patch.dict("os.environ", {OTEL_CONFIG_FILE: "/tmp/otel.yaml"})
@patch.dict("sys.modules", {"opentelemetry.configuration": None}, clear=False)
@patch("opentelemetry.sdk._configuration._initialize_components")
def test_env_var_set_but_package_missing_raises(mock_init_components: MagicMock, configure: Configure) -> None:
    """`OTEL_CONFIG_FILE` without opentelemetry-configuration installed raises a `RuntimeError` naming that package (`test_configurator_file_routing.py:61-71`)."""
    with pytest.raises(RuntimeError, match="opentelemetry-configuration"), configure():
        pass
    mock_init_components.assert_not_called()


@patch.dict(os.environ, {_OTEL_PYTHON_EXPORTER_OTLP_HTTP_TRACES_CREDENTIAL_PROVIDER: "bad_credential", **_OTLP_HTTP_TRACES})
@patch("opentelemetry.exporter.otlp.proto.http._common.entry_points")
def test_entrypoint_wrong_type_raises(mock_entry_point: MagicMock, configure: Configure) -> None:
    """A credential provider entry point of the wrong type refuses the scope with a `RuntimeError` (`test_proto_span_exporter.py:221-231`)."""
    mock_entry_point.configure_mock(return_value=[IterEntryPoint("bad_credential", lambda: 1)])
    with pytest.raises(RuntimeError), configure():
        pass


@patch.dict(os.environ, {_OTEL_PYTHON_EXPORTER_OTLP_HTTP_TRACES_CREDENTIAL_PROVIDER: "missing", **_OTLP_HTTP_TRACES})
@patch("opentelemetry.exporter.otlp.proto.http._common.entry_points")
def test_entrypoint_missing_raises(mock_entry_point: MagicMock, configure: Configure) -> None:
    """A credential provider no entry point names refuses the scope with a `RuntimeError` (`test_proto_span_exporter.py:233-243`)."""
    mock_entry_point.configure_mock(return_value=[])
    with pytest.raises(RuntimeError), configure():
        pass


def test_environment_configuration_does_not_import_file_configuration() -> None:
    """Setup from the environment alone never imports the declarative configuration package (splunk `test_configurator.py:48-74`)."""
    env = {key: value for key, value in os.environ.items() if not key.startswith(("OTEL_", "SPLUNK_"))}
    env.update(OTEL_SERVICE_NAME="environment-test", OTEL_TRACES_EXPORTER="none", OTEL_METRICS_EXPORTER="none", OTEL_LOGS_EXPORTER="none", OTEL_PYTHON_DISABLED_INSTRUMENTATIONS="system_metrics")
    result = subprocess.run(
        [
            sys.executable,
            "-c",
            (
                "import sys; from rasm_telemetry import LogSettings; from rasm_telemetry.process import instrumented; "
                "instrumented(sys.stderr, service='environment-test', version='0', settings=LogSettings(), scopes=()).__enter__(); "
                "assert 'opentelemetry.configuration' not in sys.modules; "
                "assert 'jsonschema' not in sys.modules; assert 'rpds' not in sys.modules"
            ),
        ],
        env=env,
        capture_output=True,
        text=True,
        check=False,
    )
    assert result.returncode == 0, result.stderr


def test_invalid_file_fails_configuration(tmp_path: Path, monkeypatch: pytest.MonkeyPatch, configure: Configure) -> None:
    """A configuration file that is no valid document refuses the scope with `ConfigurationError` (splunk `test_configurator.py:77-82`)."""
    config_file = tmp_path / "config.yaml"
    config_file.write_text('file_format: "1.0"\ndisabled: [\n', encoding="utf-8")
    monkeypatch.setenv(OTEL_CONFIG_FILE, str(config_file))
    with pytest.raises(ConfigurationError), configure():
        pass


@mock.patch.dict("os.environ", {OTEL_SDK_DISABLED: "true"})
def test_get_tracer_with_sdk_disabled(configure: Configure) -> None:
    """Under `OTEL_SDK_DISABLED=true` the provider the scope leaves installed hands out no-op tracers (opentelemetry-sdk `tests/trace/test_trace.py:268-271`)."""
    with configure():
        tracer_provider = trace.get_tracer_provider()
        assert isinstance(tracer_provider.get_tracer(Mock()), trace.NoOpTracer)


def test_config_9_ok(tmp_path: Path) -> None:
    """The call the scope yields applies another settings record's level to a scope logger while the scope runs (CPython `Lib/test/test_logging.py:3576-3593`)."""
    output = tmp_path / "scope.log"
    next_message = map(str, itertools.count(1)).__next__

    def log_lines() -> list[tuple[object, object]]:
        try:
            written = output.read_bytes().splitlines()
        except FileNotFoundError:
            return []
        return [(record["severity_text"], record["body"]) for record in map(msgspec.json.Decoder(dict[str, object]).decode, written)]

    with (
        patch.dict(os.environ, {OTEL_TRACES_EXPORTER: "none", OTEL_METRICS_EXPORTER: "none", OTEL_LOGS_EXPORTER: "none"}, clear=True),
        instrumented(output, service="rasm-telemetry-tests", version="0", settings=LogSettings(level="warning"), scopes=("compiler.parser",)) as apply_config,
    ):
        logger = logging.getLogger("compiler.parser")
        logger.info(next_message())
        assert not log_lines()
        apply_config(LogSettings(level="warning"))
        logger.info(next_message())
        assert not log_lines()
        apply_config(LogSettings(level="info"))
        logger.info(next_message())
        assert log_lines() == [("INFO", "3")]
