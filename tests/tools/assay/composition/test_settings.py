"""Assay settings and artifact-store laws."""

from typing import TYPE_CHECKING

from pydantic import ValidationError
import pytest
from upath import UPath

from tools.assay.composition.settings import ArtifactBackend, AssaySettings
from tools.assay.core.model import ArtifactKind


if TYPE_CHECKING:
    from tests.tools.assay.conftest import AssayHarness


def test_memory_store_round_trip_and_glob(assay_root: AssayHarness) -> None:
    """ArtifactStore validates segments and round-trips bytes over fsspec memory."""
    store = assay_root.settings.store(protocol="memory")
    path = store.write_bytes(b"payload", "history", "run-1", "envelope.json")

    assert store.read_bytes("history", "run-1", "envelope.json") == b"payload"
    assert path in store.glob("history/*/envelope.json")


def test_store_text_find_info_and_create_only(assay_root: AssayHarness) -> None:
    """ArtifactStore exposes text, metadata, recursive find, and create-only writes over fsspec."""
    store = assay_root.settings.store(protocol="memory")
    path = store.write_text("alpha\nbeta\n", "scope", "api", "surface.txt", create=True, transaction=True)

    assert store.read_text_path(path) == "alpha\nbeta\n"
    assert path in store.find("scope")
    assert path in store.list("scope", "api")
    assert store.size_path(path) == len("alpha\nbeta\n")
    with pytest.raises(FileExistsError):
        store.write_text("again", "scope", "api", "surface.txt", create=True)


def test_settings_owned_artifact_backend_and_trace_env(assay_root: AssayHarness) -> None:
    """Artifact backend settings are independent of local root and trace context crosses SSH env projection."""
    settings = AssaySettings(root=UPath(assay_root.root), run_id="run-1", artifact_backend=ArtifactBackend(protocol="memory", root="custom-root"))
    env = settings.remote_env({"traceparent": "00-a-b-01", "baggage": "k=v", "UNSAFE": "x", "ASSAY_RUN_ID": "run-1"})

    assert settings.store().root == "custom-root"
    assert env == {"traceparent": "00-a-b-01", "baggage": "k=v", "ASSAY_RUN_ID": "run-1"}


def test_artifact_rejects_unsafe_path_segments(assay_root: AssayHarness) -> None:
    """Artifact paths accept run-safe segments and reject traversal."""
    assert assay_root.settings.artifact(ArtifactKind.CODE, "search", "run-1", "matches.txt").name == "matches.txt"

    for unsafe in ("..", "a/", "a//", "a\\b", "/abs", "\x00"):
        with pytest.raises(ValueError, match="unsafe artifact path segment"):
            assay_root.settings.artifact(ArtifactKind.CODE, unsafe, "matches.txt")


def test_store_backend_path_rejects_escape(assay_root: AssayHarness) -> None:
    """Backend-path reads/removes only accept paths under the store root."""
    store = assay_root.settings.store(protocol="memory")
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
