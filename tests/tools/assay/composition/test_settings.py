"""Assay settings and artifact-store laws."""

from datetime import datetime, UTC
import inspect
from itertools import starmap
from pathlib import Path  # noqa: TC003  # pytest resolves fixture annotation at runtime
from typing import override, TYPE_CHECKING

import msgspec
from pydantic import ValidationError
from pydantic_settings import PydanticBaseSettingsSource
import pytest
from upath import UPath

from tests.tools.assay.conftest import make_history_envelope, WIRE_ENCODER  # testing internals is legitimate
from tools.assay.composition.settings import _mtime, ArtifactBackend, AssaySettings, Configuration  # noqa: PLC2701  # testing internals is legitimate
from tools.assay.core.model import Artifact, ArtifactKind


if TYPE_CHECKING:
    from pydantic.fields import FieldInfo

    from tests.tools.assay.conftest import AssayHarness
    from tools.assay.core.model import Report


class _DummySource(PydanticBaseSettingsSource):
    @override
    def get_field_value(self, field: FieldInfo, field_name: str) -> tuple[object, str, bool]:
        _ = (field, field_name)
        return None, "", False

    @override
    def __call__(self) -> dict[str, object]:
        return {}


def test_memory_store_round_trip_and_glob(assay_root: AssayHarness) -> None:
    """ArtifactStore validates segments and round-trips bytes over fsspec memory."""
    store = assay_root.settings.store(protocol="memory", root="round-trip-root")
    path = store.write_bytes(b"payload", "history", "run-1", "envelope.json")

    assert store.read_bytes("history", "run-1", "envelope.json") == b"payload"
    assert path in store.glob("history/*/envelope.json")


def test_settings_customise_sources_keeps_pydantic_hook_names_and_sources() -> None:
    """Pydantic calls settings_customise_sources by keyword, so hook names stay exact while dotenv/secrets are disabled."""
    params = tuple(inspect.signature(AssaySettings.settings_customise_sources).parameters)
    init = _DummySource(AssaySettings)
    env = _DummySource(AssaySettings)
    dotenv = _DummySource(AssaySettings)
    secrets = _DummySource(AssaySettings)

    assert params == ("settings_cls", "init_settings", "env_settings", "dotenv_settings", "file_secret_settings")
    assert AssaySettings.settings_customise_sources(AssaySettings, init, env, dotenv, secrets) == (init, env)


def test_store_override_revalidates_backend(assay_root: AssayHarness) -> None:
    """Store overrides re-enter ArtifactBackend validation instead of bypassing it with model_copy(update=...)."""
    with pytest.raises(ValidationError, match="protocol"):
        assay_root.settings.store(protocol="not valid")


def test_store_text_find_info_and_create_only(assay_root: AssayHarness) -> None:
    """ArtifactStore exposes text, metadata, recursive find, and create-only writes over fsspec."""
    store = assay_root.settings.store(protocol="memory", root="text-find-root")
    path = store.write_text("alpha\nbeta\n", "scope", "api", "surface.txt", create=True, transaction=True)

    assert store.read_text_path(path) == "alpha\nbeta\n"
    assert path in store.walk("scope", recursive=True)
    assert path in store.walk("scope", "api")
    assert store.size_path(path) == len("alpha\nbeta\n")
    with pytest.raises(FileExistsError):
        store.write_text("again", "scope", "api", "surface.txt", create=True)


def test_store_resolves_direct_and_latest_tokens(assay_root: AssayHarness) -> None:
    """ArtifactStore resolves show tokens through direct backend paths and metadata-ranked matches."""
    store = assay_root.settings.store(protocol="memory", root="resolve-token-root")
    older = store.write_text("old", "scope", "api", "run-a", "surface.txt")
    newer = store.write_text("new", "history", "run-b", "surface.txt")

    assert store.resolve_artifacts(older, "scope", "history") == (older,)
    assert store.resolve_artifacts("", "scope", "history") == ()
    assert store.resolve_artifacts("latest", "scope", "history", latest=True)[0] == older
    matches = store.resolve_artifacts("surface.txt", "scope", "history")
    assert set(matches) == {older, newer}


def test_settings_owned_artifact_backend_and_trace_env(assay_root: AssayHarness) -> None:
    """Artifact backend settings are independent of local root and trace context crosses SSH env projection."""
    settings = AssaySettings(root=UPath(assay_root.root), run_id="run-1", artifact_backend=ArtifactBackend(protocol="memory", root="custom-root"))
    env = settings.remote_env({"traceparent": "00-a-b-01", "baggage": "k=v", "UNSAFE": "x", "ASSAY_RUN_ID": "run-1"})

    assert settings.store().root == "custom-root"
    assert env == {"traceparent": "00-a-b-01", "baggage": "k=v", "ASSAY_RUN_ID": "run-1"}


def test_settings_configuration_projection_revalidates_without_computed_fields(assay_root: AssayHarness) -> None:
    """Configuration changes re-enter Pydantic validation without smuggling computed properties back as inputs."""
    debug = assay_root.settings.with_configuration(Configuration.DEBUG)

    assert debug.configuration is Configuration.DEBUG
    assert debug.root == assay_root.settings.root


def test_artifact_rejects_unsafe_path_segments(assay_root: AssayHarness) -> None:
    """Artifact paths accept run-safe segments and reject traversal."""
    assert assay_root.settings.artifact(ArtifactKind.CODE, "search", "run-1", "matches.txt").name == "matches.txt"

    for unsafe in ("..", "a/", "a//", "a\\b", "/abs", "\x00"):
        with pytest.raises(ValueError, match="unsafe artifact path segment"):
            assay_root.settings.artifact(ArtifactKind.CODE, unsafe, "matches.txt")


def test_store_backend_path_rejects_escape(assay_root: AssayHarness) -> None:
    """Backend-path reads/removes only accept paths under the store root."""
    store = assay_root.settings.store(protocol="memory", root="backend-path-root")
    path = store.write_bytes(b"payload", "history", "run-1", "envelope.json")

    assert store.read_path(path) == b"payload"
    with pytest.raises(ValueError, match="escaped store root"):
        store.read_path("other/history/run-1/envelope.json")


def test_settings_validate_run_id_exec_target_and_known_hosts(assay_root: AssayHarness) -> None:
    """Run ids and remote targets are strict while known-hosts can be explicitly disabled."""
    disabled = AssaySettings(root=UPath(assay_root.root), run_id="run-1", exec_target="ssh://user@example.com:22", exec_known_hosts="")

    assert AssaySettings(root=UPath(assay_root.root), run_id="run-1").local_root == UPath(assay_root.root)
    assert disabled.exec_known_hosts is None
    assert disabled.exec_target == "ssh://user@example.com:22"

    for bad in ("", "../bad", "bad/slash"):
        with pytest.raises(ValidationError, match="run_id"):
            AssaySettings(root=UPath(assay_root.root), run_id=bad)

    with pytest.raises(ValidationError, match="without path/query/fragment"):
        AssaySettings(root=UPath(assay_root.root), run_id="run-1", exec_target="ssh://host/path")


# --- [ARTIFACT_RANKING] ----------------------------------------------------------------


def test_resolve_artifacts_latest_respects_root_priority(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """`latest=True` returns the newest artifact from the FIRST non-empty root, even when a LATER root holds a newer file.

    Root priority is load-bearing: `api show latest` lists `scope/api` before the general scopes so API-specific
    artifacts win over newer generic ones (the within-root order is still mtime-descending via `_ranked_paths`).
    """
    store = assay_root.settings.store(protocol="memory", root="multi-root")
    first = store.write_text("first-root", "scope", "a.txt")
    store.write_text("newer-later-root", "history", "b.txt")

    # Stamp deterministic mtimes through the module rank key so the assertion never leans on wall-clock write order
    # (the memory backend caches its filesystem singleton, so wrapping store.fs would leak across tests).
    monkeypatch.setattr("tools.assay.composition.settings._mtime", lambda info: 2.0 if str(info.get("name", "")).endswith("b.txt") else 1.0)

    ranked = store.resolve_artifacts("latest", "scope", "history", latest=True)
    assert ranked[0] == first  # `scope` is the first non-empty root; history/b.txt is never reached despite being newer


@pytest.mark.parametrize(
    "info, expected",
    [
        ({"mtime": 42.0}, 42.0),
        ({"mtime": 7}, 7.0),
        ({"created": datetime(2026, 1, 1, tzinfo=UTC)}, datetime(2026, 1, 1, tzinfo=UTC).timestamp()),
        ({}, 0.0),
        ({"created": "not-a-time"}, 0.0),
    ],
)
def test_mtime_coerces_datetime_and_numeric(info: dict[str, object], expected: float) -> None:
    """`_mtime` coerces numeric mtime and a tz-aware `created` datetime (memory/info backend) to a POSIX float."""
    assert _mtime(info) == expected


# --- [HISTORY_RETENTION] ---------------------------------------------------------------


@pytest.mark.parametrize("backend", ["file", "memory"])
def test_artifact_store_history_law_matrix(backend: str, assay_root: AssayHarness) -> None:
    """write_history persists, load_history round-trips known/unknown ids, and retain_history prunes oldest first."""
    store = assay_root.settings.store() if backend == "file" else assay_root.settings.store(protocol="memory", root="hist")
    store.write_history("run-a", WIRE_ENCODER.encode(make_history_envelope("run-a")))
    store.write_history("run-b", WIRE_ENCODER.encode(make_history_envelope("run-b")))

    assert store.load_history("run-a") is not None
    assert store.load_history("nope") is None
    assert store.load_history("") is None

    store.retain_history(keep=1)

    assert store.load_history("run-a") is None
    assert store.load_history("run-b") is not None


@pytest.mark.parametrize("backend", ["file", "memory"])
def test_restore_full_report_falls_back_on_corrupt_artifact(backend: str, assay_root: AssayHarness) -> None:
    """A corrupt full-report artifact silently degrades restore_full_report to the compact envelope (OSError/MsgspecError)."""
    store = assay_root.settings.store() if backend == "file" else assay_root.settings.store(protocol="memory", root="restore")
    corrupt = store.write_bytes(b"{not a report", ArtifactKind.HISTORY.value, "run-x", "full.json")
    base = make_history_envelope("run-x")
    assert base.report is not None
    artifact = Artifact(id="full-report", kind=ArtifactKind.HISTORY, path=corrupt)
    report: Report = msgspec.structs.replace(base.report, artifacts=(artifact,))
    env = msgspec.structs.replace(base, report=report)

    assert store.restore_full_report(env) is env


# --- [ENV_INGESTION] -------------------------------------------------------------------


def test_dotenv_and_secrets_are_ignored(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    """Init values win and `.env`/secrets sources stay disabled because settings_customise_sources returns (init, env) only."""
    (tmp_path / "Workspace.slnx").write_text("", encoding="utf-8")
    (tmp_path / ".env").write_text("ASSAY_RUN_ID=from-dotenv\n", encoding="utf-8")
    monkeypatch.chdir(tmp_path)

    assert AssaySettings(root=UPath(tmp_path), run_id="explicit").run_id == "explicit"


@pytest.mark.parametrize(
    "env, expected",
    [
        ({"ASSAY_OTEL_ENDPOINT": "http://primary:4317"}, "http://primary:4317"),
        ({"OTEL_EXPORTER_OTLP_ENDPOINT": "http://generic:4317"}, "http://generic:4317"),
        ({"OTEL_EXPORTER_OTLP_TRACES_ENDPOINT": "http://traces:4317"}, "http://traces:4317"),
        ({"ASSAY_OTEL_ENDPOINT": "http://primary:4317", "OTEL_EXPORTER_OTLP_ENDPOINT": "http://generic:4317"}, "http://primary:4317"),
        ({"OTEL_EXPORTER_OTLP_ENDPOINT": "http://generic:4317", "OTEL_EXPORTER_OTLP_TRACES_ENDPOINT": "http://traces:4317"}, "http://generic:4317"),
    ],
)
def test_otel_endpoint_alias_precedence(env: dict[str, str], expected: str, assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """otel_endpoint resolves through AliasChoices in declaration order: ASSAY_OTEL_ENDPOINT before the generic then the traces OTLP env."""
    tuple(starmap(monkeypatch.setenv, env.items()))

    assert AssaySettings(root=UPath(assay_root.root), run_id="run-1").otel_endpoint == expected
