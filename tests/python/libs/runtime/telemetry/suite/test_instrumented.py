"""Process-wide OpenTelemetry setup on entering `rasm_telemetry.process.instrumented`, copied from the opentelemetry-distro, opentelemetry-sdk file routing, OTLP/HTTP span exporter, and splunk-opentelemetry configurator tests the cases name."""

from collections.abc import Callable
from contextlib import AbstractContextManager
from importlib.metadata import PackageNotFoundError, version
import io
import os
from pathlib import Path
import subprocess
import sys
import types
from unittest import mock
from unittest.mock import MagicMock, Mock, patch

from opentelemetry import trace as trace_api
from opentelemetry.configuration import ConfigurationError
from opentelemetry.environment_variables import OTEL_LOGS_EXPORTER, OTEL_METRICS_EXPORTER, OTEL_TRACES_EXPORTER
from opentelemetry.sdk.environment_variables import _OTEL_PYTHON_EXPORTER_OTLP_HTTP_TRACES_CREDENTIAL_PROVIDER, OTEL_CONFIG_FILE, OTEL_EXPORTER_OTLP_PROTOCOL, OTEL_SDK_DISABLED
from opentelemetry.test.mock_test_classes import IterEntryPoint
import pytest

from rasm_telemetry import LogSettings
from rasm_telemetry.process import instrumented

# --- [CONSTANTS] ------------------------------------------------------------------------

_OTLP_HTTP_TRACES = {OTEL_TRACES_EXPORTER: "otlp_proto_http", OTEL_METRICS_EXPORTER: "none", OTEL_LOGS_EXPORTER: "none"}

# --- [MODELS] ---------------------------------------------------------------------------


class _FakeConfigurationModule(types.ModuleType):
    """Stub `opentelemetry.configuration` module, as the exemplar's class of this name (`test_configurator_file_routing.py:15-31`)."""

    configure_sdk: MagicMock
    load_config_file: MagicMock

    def __init__(self) -> None:
        """Names the module and stubs the two functions the routing calls."""
        super().__init__("opentelemetry.configuration")
        self.configure_sdk = MagicMock()
        self.load_config_file = MagicMock()


# --- [OPERATIONS] -----------------------------------------------------------------------


def _configure() -> AbstractContextManager[Callable[[LogSettings], None]]:
    """Returns the process scope read from `os.environ`, as rasm-cli enters it, standing where an exemplar calls its distro's or configurator's `configure`."""
    return instrumented(io.StringIO(), service="rasm-telemetry-tests", version="0", settings=LogSettings(), scopes=(), environ=os.environ)


def test_package_available() -> None:
    """The distribution is installed under its published name (`test_distro.py:21-25`)."""
    try:
        version("rasm-telemetry")
    except PackageNotFoundError:
        pytest.fail("rasm-telemetry not installed")


@mock.patch.dict("os.environ", {OTEL_SDK_DISABLED: "true"}, clear=True)
def test_default_configuration() -> None:
    """In an environment naming no exporter the scope selects OTLP over gRPC for traces and metrics (`test_distro.py:27-33`)."""
    with _configure():
        assert os.environ.get(OTEL_TRACES_EXPORTER) == "otlp"
        assert os.environ.get(OTEL_METRICS_EXPORTER) == "otlp"
        assert os.environ.get(OTEL_EXPORTER_OTLP_PROTOCOL) == "grpc"


@patch.dict("os.environ", {OTEL_CONFIG_FILE: "/tmp/otel.yaml"})
@patch("opentelemetry.sdk._configuration._initialize_components")
def test_env_var_set_routes_to_declarative_path(mock_init_components: MagicMock) -> None:
    """`OTEL_CONFIG_FILE` loads the file and configures the SDK from it alone (`test_configurator_file_routing.py:47-59`)."""
    fake = _FakeConfigurationModule()
    sentinel_config = object()
    fake.load_config_file.return_value = sentinel_config
    with patch.dict("sys.modules", {"opentelemetry.configuration": fake}), _configure():
        pass
    fake.load_config_file.assert_called_once_with("/tmp/otel.yaml")
    fake.configure_sdk.assert_called_once_with(sentinel_config)
    mock_init_components.assert_not_called()


@patch.dict("os.environ", {OTEL_CONFIG_FILE: "/tmp/otel.yaml"})
@patch.dict("sys.modules", {"opentelemetry.configuration": None}, clear=False)
@patch("opentelemetry.sdk._configuration._initialize_components")
def test_env_var_set_but_package_missing_raises(mock_init_components: MagicMock) -> None:
    """`OTEL_CONFIG_FILE` without opentelemetry-configuration installed raises a `RuntimeError` naming that package (`test_configurator_file_routing.py:61-71`)."""
    with pytest.raises(RuntimeError, match="opentelemetry-configuration"), _configure():
        pass
    mock_init_components.assert_not_called()


@patch.dict(os.environ, {_OTEL_PYTHON_EXPORTER_OTLP_HTTP_TRACES_CREDENTIAL_PROVIDER: "bad_credential", **_OTLP_HTTP_TRACES})
@patch("opentelemetry.exporter.otlp.proto.http._common.entry_points")
def test_entrypoint_wrong_type_raises(mock_entry_point: MagicMock) -> None:
    """A credential provider entry point of the wrong type refuses the scope with a `RuntimeError` (`test_proto_span_exporter.py:221-231`)."""
    mock_entry_point.configure_mock(return_value=[IterEntryPoint("bad_credential", lambda: 1)])
    with pytest.raises(RuntimeError), _configure():
        pass


@patch.dict(os.environ, {_OTEL_PYTHON_EXPORTER_OTLP_HTTP_TRACES_CREDENTIAL_PROVIDER: "missing", **_OTLP_HTTP_TRACES})
@patch("opentelemetry.exporter.otlp.proto.http._common.entry_points")
def test_entrypoint_missing_raises(mock_entry_point: MagicMock) -> None:
    """A credential provider no entry point names refuses the scope with a `RuntimeError` (`test_proto_span_exporter.py:233-243`)."""
    mock_entry_point.configure_mock(return_value=[])
    with pytest.raises(RuntimeError), _configure():
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
                "import os, sys; from rasm_telemetry import LogSettings; from rasm_telemetry.process import instrumented; "
                "instrumented(sys.stderr, service='environment-test', version='0', settings=LogSettings(), scopes=(), environ=os.environ).__enter__(); "
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


def test_invalid_file_fails_configuration(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    """A configuration file that is no valid document refuses the scope with `ConfigurationError` (splunk `test_configurator.py:77-82`)."""
    config_file = tmp_path / "config.yaml"
    config_file.write_text('file_format: "1.0"\ndisabled: [\n', encoding="utf-8")
    monkeypatch.setenv(OTEL_CONFIG_FILE, str(config_file))
    with pytest.raises(ConfigurationError), _configure():
        pass


@mock.patch.dict("os.environ", {OTEL_SDK_DISABLED: "true"})
def test_get_tracer_with_sdk_disabled() -> None:
    """Under `OTEL_SDK_DISABLED=true` the provider the scope leaves installed hands out no-op tracers (opentelemetry-sdk `tests/trace/test_trace.py:268-271`)."""
    with _configure():
        tracer_provider = trace_api.get_tracer_provider()
        assert isinstance(tracer_provider.get_tracer(Mock()), trace_api.NoOpTracer)
