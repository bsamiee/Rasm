"""Settings law matrix for configuration, artifact scope, and store behavior."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import contextlib
from datetime import datetime, UTC
import os
from pathlib import Path
import tempfile
from typing import Final, override, Self, TYPE_CHECKING
import uuid  # noqa: TC003  # runtime: Hypothesis draws uuid.UUID values for the state-machine store identity

from dirty_equals import IsPartialDict, IsStr, IsTuple
import fsspec
from hypothesis import given, settings as hyp_settings, strategies as st, target
from hypothesis.stateful import Bundle, initialize, invariant, rule, RuleBasedStateMachine
import msgspec
from pydantic import ValidationError
import pytest
from upath import UPath
import zstandard

from tests.python._testkit.spec import assert_none, assert_some, idempotent, model_based, roundtrip, validity_matrix
from tests.python.tools.assay.kit import (  # noqa: TC001  # runtime use: instantiated in fixture bodies, not annotation-only
    AssayHarness,
    make_history_envelope,
    WIRE_ENCODER,
)
from tools.assay.composition import settings as _settings_mod, store as _store_mod
from tools.assay.composition.settings import (
    ArtifactBackend,
    AssaySettings,
    backend_capability,
    Configuration,
    Local,
    Offload,
    PullStrategy,
    remote_path,
    resolve_tilde,
    run_id_host_token,
    Ssh,
)
from tools.assay.composition.store import (
    ArtifactFileSystem,
    ArtifactScope,
    ArtifactStore,
    CS_ARTIFACT_ROOTS,
    DOTNET_BUILD_CLOSURE,
    mtime_from_info,
    prune_python_artifacts,
    PY_ARTIFACT_ROOTS,
    PY_COVERAGE_FILES,
    safe_segment,
    size_from_info,
    unframe,
)
from tools.assay.core.model import Artifact, ArtifactKind, Claim


if TYPE_CHECKING:
    from collections.abc import Callable

    from tools.assay.core.model import Envelope


# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (
    ArtifactBackend,
    ArtifactFileSystem,
    ArtifactScope,
    ArtifactStore,
    AssaySettings,
    Local,
    Offload,
    Ssh,
    backend_capability,
    mtime_from_info,
    prune_python_artifacts,
    remote_path,
    resolve_tilde,
    run_id_host_token,
    safe_segment,
    size_from_info,
    unframe,
)

_CPU_MIN: Final = 1
_CPU_MAX: Final = 256
_CPU_BELOW: Final = 0
_CPU_ABOVE: Final = 300

# --- [BOUNDARIES] -----------------------------------------------------------------------


class _StubWriter(contextlib.AbstractContextManager[object]):
    """Autocommit-deferred write handle that commits only on clean exit."""

    def __init__(self, data: dict[str, bytes], path: str, *, fail: bool = False) -> None:
        self._data = data
        self._path = path
        self._fail = fail
        self._buffer = bytearray()

    @override
    def __enter__(self) -> Self:
        return self

    def write(self, payload: bytes) -> int:
        match self._fail:
            case True:
                raise OSError("injected write failure")
            case _:
                self._buffer.extend(payload)
                return len(payload)

    @override
    def __exit__(self, *exc: object) -> None:
        match exc[0]:
            case None:
                self._data[self._path] = bytes(self._buffer)
            case _:
                pass  # Failed writes must leave the target path untouched.


class _FsStub(ArtifactFileSystem):  # noqa: PLR0904  # full structural protocol requires all 10 override surfaces
    """Parametrizable fsspec stub: keyed-dict find detail, configurable info, optional rm race."""

    def __init__(self, *, info_payload: dict[str, object] | None = None, rm_raises: bool = False, write_fails: bool = False) -> None:
        self._data: dict[str, bytes] = {}
        self._info = info_payload
        self._rm_raises = rm_raises
        self._write_fails = write_fails

    def seed(self, path: str, payload: bytes) -> None:
        """Seed a backend path so glob/find discover it (test setup, not part of the protocol)."""
        self._data[path] = payload

    @override
    def makedirs(self, path: str, *, exist_ok: bool = False) -> object:
        return None

    @override
    def glob(self, path: str) -> list[str]:
        prefix = path.rstrip("*").rstrip("/")
        return [p for p in self._data if p.startswith(prefix)]

    @override
    def ls(self, path: str, *, detail: bool = False) -> list[str] | list[dict[str, object]]:
        # Omitted mtime forces sorted-history tests onto the lexicographic run-id tiebreaker.
        prefix = f"{path.rstrip('/')}/"
        children = sorted({f"{prefix}{p[len(prefix) :].split('/', 1)[0]}" for p in self._data if p.startswith(prefix)})
        match detail:
            case True:
                rows: list[dict[str, object]] = [{"name": child, "type": "directory"} for child in children]
                return rows
            case _:
                return list(children)

    @override
    def find(self, path: str, *, detail: bool = False) -> list[str] | dict[str, dict[str, object]]:
        seeded = [p for p in self._data if p.startswith(path)]
        match (bool(seeded), detail):
            case (True, True):
                return {p: {"name": p, "type": "file"} for p in seeded}
            case (True, False):
                return seeded
            case (False, True):
                return {f"{path}/f.txt": {"name": f"{path}/f.txt", "type": "file", "size": 7}}
            case _:
                return [f"{path}/f.txt"]

    @override
    def info(self, path: str) -> dict[str, object]:
        return self._info if self._info is not None else {"name": path, "type": "file"}

    @override
    def exists(self, path: str) -> bool:
        return path in self._data or self._info is not None

    @override
    def cat_file(self, path: str) -> bytes:
        return self._data.get(path, b"")

    @override
    def rm(self, path: str, *, recursive: bool = False) -> object:
        match (self._rm_raises, recursive):
            case (True, _):
                raise FileNotFoundError(path)
            case (_, True):
                # Recursive pruning must remove both the dir key and its seeded children.
                prefix = f"{path.rstrip('/')}/"
                for key in [p for p in self._data if p == path or p.startswith(prefix)]:
                    del self._data[key]
                return None
            case _:
                self._data.pop(path, None)
                return None

    @override
    def open(self, path: str, mode: str = "rb", *, autocommit: bool = True) -> contextlib.AbstractContextManager[object]:
        # _write_at must route through the autocommit-aware handle, not mutate storage directly.
        return _StubWriter(self._data, path, fail=self._write_fails)

    transaction: contextlib.AbstractContextManager[object] = contextlib.nullcontext()


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [INFO_PROJECTION_LAWS]


@pytest.mark.parametrize(
    "info, expected",
    [
        ({"mtime": 42.0}, 42.0),
        ({"mtime": 7}, 7.0),
        ({"created": datetime(2026, 1, 1, tzinfo=UTC)}, datetime(2026, 1, 1, tzinfo=UTC).timestamp()),
        ({}, 0.0),
        ({"created": "not-a-time"}, 0.0),
        ({"mtime": None}, 0.0),
    ],
    ids=["float", "int_coerce", "datetime_created", "empty", "bad_created", "none_mtime"],
)
def test_mtime_from_info_projection(info: dict[str, object], expected: float) -> None:
    """mtime_from_info coerces numeric/datetime fsspec metadata to a POSIX float; falls back to 0.0."""
    assert mtime_from_info(info) == expected


@given(st.one_of(st.floats(min_value=0.0, max_value=1e12, allow_nan=False, allow_infinity=False), st.integers(min_value=0, max_value=2**53 + 2)))
@hyp_settings(max_examples=80)
def test_mtime_from_info_numeric_identity(v: float) -> None:
    """mtime_from_info returns float(v) for finite non-negative float/int mtime — exact within float()'s own promotion."""
    result = mtime_from_info({"mtime": v})
    assert isinstance(result, float)
    assert result == float(v)  # noqa: RUF069  # float() int→float promotion is deterministic; precision loss past 2^53 is the contract
    target(float(v), label="mtime_magnitude")  # biases Hypothesis toward 2^53+1, where int→float first loses precision


@pytest.mark.parametrize(
    "info, fallback, expected",
    [({"size": 4096}, 0, 4096), ({"size": 0}, 7, 0), ({}, 11, 11), ({"size": None}, 13, 13), ({"size": 3.5}, 17, 17)],
    ids=["int_size", "zero_is_genuine", "absent_falls_back", "none_falls_back", "non_int_falls_back"],
)
def test_size_from_info_projection(info: dict[str, object], fallback: int, expected: int) -> None:
    """size_from_info returns an int size when fsspec reports one, else the fallback for absent or non-int keys."""
    assert size_from_info(info, fallback=fallback) == expected


# --- [ARTIFACT_BACKEND_LAWS]


def test_artifact_backend_validation_matrix() -> None:
    """ArtifactBackend rejects non-file backends without a root and roots that traverse with '..'."""
    validity_matrix(
        (
            ("file_empty", ("file", ""), True),
            ("file_path", ("file", "some/path"), True),
            ("memory_ok", ("memory", "my-prefix"), True),
            ("memory_no_root", ("memory", ""), False),  # non-file backend requires non-empty root
            ("memory_traversal", ("memory", "../escape"), False),  # traversal rejected
            ("s3_no_root", ("s3", ""), False),  # non-file + empty root
        ),
        lambda pr: _backend_ok(*pr),
    )


def _backend_ok(protocol: str, root: str) -> bool:
    try:
        ArtifactBackend(protocol=protocol, root=root)
    except ValidationError:
        return False
    return True


@pytest.mark.parametrize(
    "protocol, root, expect_part",
    [
        ("file", "", ".artifacts/assay"),  # file+empty -> store_root
        ("memory", "my-prefix", "my-prefix"),  # non-file -> root stripped
        ("memory", "my-prefix/", "my-prefix"),  # rstrip: trailing slash only
        ("memory", "my-prefix//", "my-prefix"),  # rstrip: repeated slashes only
        ("memory", "prefixX", "prefixX"),  # rstrip must not eat the trailing X sentinel
    ],
    ids=["file_empty", "non_file_root", "trailing_slash", "double_trailing_slash", "trailing_capital_x"],
)
def test_artifact_backend_target_dispatch(protocol: str, root: str, expect_part: str, assay_root: AssayHarness) -> None:
    """ArtifactBackend.target dispatches per protocol/root; non-file roots strip only trailing slash characters."""
    result = ArtifactBackend(protocol=protocol, root=root).target(assay_root.settings)
    hit = expect_part in result if protocol == "file" else result == expect_part
    assert hit, f"target({protocol!r}, {root!r}) -> {result!r}, expected {expect_part!r}"


def test_artifact_backend_target_file_root_anchoring(assay_root: AssayHarness) -> None:
    """Absolute file roots pass through; relative file roots anchor under settings.root, never the non-file strip arm."""
    with tempfile.TemporaryDirectory() as td:
        assert ArtifactBackend(protocol="file", root=td).target(assay_root.settings) == td
    settings = assay_root.settings
    result = ArtifactBackend(protocol="file", root="rel/path").target(settings)
    assert result == str(settings.root / UPath("rel/path"))
    assert UPath(result).is_absolute()
    assert result != "rel/path"


# --- [ASSAY_SETTINGS_LAWS]


def test_assay_settings_cpu_count_boundary(assay_root: AssayHarness) -> None:
    """cpu_count rejects values outside [1, 256] and accepts the inclusive endpoints exactly.

    A mutant dropping ge=1 would admit 0; a mutant dropping le=256 would admit 300; both must fail,
    while 1 and 256 must construct, pinning the bound to exactly [1, 256] rather than a looser variant.
    """

    def _build(cpu_count: int) -> AssaySettings:
        return AssaySettings(root=assay_root.settings.root, exec_known_hosts=None, cpu_count=cpu_count)

    validity_matrix(
        (("below", _CPU_BELOW, False), ("above", _CPU_ABOVE, False), ("min", _CPU_MIN, True), ("max", _CPU_MAX, True)), lambda c: _cpu_ok(_build, c)
    )
    assert (_build(_CPU_MIN).cpu_count, _build(_CPU_MAX).cpu_count) == (_CPU_MIN, _CPU_MAX)


def _cpu_ok(build: Callable[[int], AssaySettings], cpu_count: int) -> bool:
    try:
        build(cpu_count)
    except ValidationError:
        return False
    return True


def test_assay_settings_computed_projections(assay_root: AssayHarness) -> None:
    """Computed solution / agent_context / python_tool_env project off root, run_id, and agent_task_id."""
    s = assay_root.settings
    assert s.solution == s.root / "Workspace.slnx"
    assert s.agent_context == IsPartialDict({"run.id": s.run_id, "agent.task.id": s.agent_task_id})
    assert s.mutation_python == IsStr(min_length=1)
    env = s.python_tool_env
    assert env.keys() >= {"UV_CACHE_DIR", "HYPOTHESIS_STORAGE_DIRECTORY", "PYTEST_CACHE_DIR", "RUFF_CACHE_DIR", "MYPY_CACHE_DIR", "COVERAGE_FILE"}
    root_str = str(s.local_root)
    assert all(v.startswith(root_str) for v in env.values())


def test_assay_settings_local_root_raises_for_non_file_protocol() -> None:
    """local_root raises when root uses a non-file protocol; construction itself still succeeds.

    A memory:// UPath has .protocol == 'memory'; _anchor folds to the cursor (no Workspace.slnx ancestor
    in the virtual FS), so the model builds and only local_root rejects the non-file protocol.
    """
    settings = AssaySettings(root=UPath("memory://bucket/scope"), exec_known_hosts=None)
    with pytest.raises(ValueError, match="file protocol"):
        _ = settings.local_root


def test_default_root_package_home_fallback(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    """A marker-less cwd falls back to the package-home workspace walk; a marker-bearing cwd wins outright."""
    monkeypatch.chdir(tmp_path)
    fallback = _settings_mod._default_root()
    assert (fallback / "Workspace.slnx").is_file(), "the fallback must land on the package's own workspace root"
    assert str(fallback) != str(UPath(tmp_path).resolve())
    (tmp_path / "Workspace.slnx").write_text("", encoding="utf-8")
    assert str(_settings_mod._default_root()) == str(UPath(tmp_path).resolve())


def test_assay_settings_wire_safe_scrubs_surrogates(tmp_path: Path) -> None:
    """_wire_safe replaces lone surrogates in run_id / agent_task_id so the result is UTF-8 encodable."""
    (tmp_path / "Workspace.slnx").write_text("", encoding="utf-8")
    settings = AssaySettings(root=UPath(tmp_path), run_id="run-1", agent_task_id="run-\udcff", exec_known_hosts=None)
    settings.agent_task_id.encode("utf-8")


def test_run_id_host_token_carves_the_per_host_prune_namespace(tmp_path: Path) -> None:
    """``run_id_host_token`` extracts the embedded host token so a remote prune sweeps one host's namespace only.

    The default ``<timestamp>-<token>-<pid>`` run id yields its blake2b host token, which ``host_run_token`` surfaces;
    a custom tokenless run id yields ``""`` so no remote prune ever claims another host's runs on a shared workroot.
    """
    (tmp_path / "Workspace.slnx").write_text("", encoding="utf-8")
    minted = _settings_mod._host_unique_run_id()
    assert run_id_host_token(minted) == _settings_mod._host_token(), "the minted run id must carry this host's blake2b token"
    assert AssaySettings(root=UPath(tmp_path), run_id=minted, exec_known_hosts=None).host_run_token == _settings_mod._host_token()
    assert not run_id_host_token("custom-run"), "a tokenless run id owns no host namespace"
    assert not AssaySettings(root=UPath(tmp_path), run_id="custom-run", exec_known_hosts=None).host_run_token


@pytest.mark.parametrize(
    "exec_target, error_match",
    [("ssh://user@host:notaport", "non-numeric ssh port"), ("http://host/path", "without path/query/fragment")],
    ids=["non_numeric_port", "non_ssh_scheme"],
)
def test_assay_settings_exec_target_rejected(exec_target: str, error_match: str, tmp_path: Path) -> None:
    """exec_target rejects non-numeric ssh ports and non-ssh schemes with a diagnostic ValidationError."""
    (tmp_path / "Workspace.slnx").write_text("", encoding="utf-8")
    with pytest.raises(ValidationError, match=error_match):
        AssaySettings.model_validate({"root": UPath(tmp_path), "exec_target": exec_target, "exec_known_hosts": None})


def test_assay_settings_exec_target_modal_union(tmp_path: Path) -> None:
    """exec_target admits ssh://[user@]host:port into an Ssh value object; an empty target admits the falsy Local case."""
    (tmp_path / "Workspace.slnx").write_text("", encoding="utf-8")
    s = AssaySettings.model_validate({"root": UPath(tmp_path), "exec_target": "ssh://user@host:22", "exec_known_hosts": None})
    assert isinstance(s.exec_target, Ssh)
    assert (s.exec_target.host, s.exec_target.port, s.exec_target.user, s.exec_target.url) == ("host", 22, "user", "ssh://user@host:22")
    local = AssaySettings.model_validate({"root": UPath(tmp_path), "exec_target": "", "exec_known_hosts": None})
    assert isinstance(local.exec_target, Local)
    assert (bool(local.exec_target), local.offload) == (False, None)


def test_assay_settings_exec_target_admits_raw_ssh_url_from_env(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    """``ASSAY_EXEC_TARGET=ssh://...`` admits a raw, unquoted env URL: NoDecode keeps the union field off pydantic-settings' json.loads.

    The env path is the live agent ergonomic — a bare ``ssh://host`` (no JSON quoting). Without ``NoDecode`` the ``Local | Ssh``
    union drives pydantic-settings to ``json.loads('ssh://...')``, raising ``SettingsError`` before the BeforeValidator runs.
    """
    (tmp_path / "Workspace.slnx").write_text("", encoding="utf-8")
    monkeypatch.setenv("ASSAY_ROOT", str(tmp_path))
    monkeypatch.setenv("ASSAY_EXEC_TARGET", "ssh://root@31.97.131.41")
    monkeypatch.setenv("ASSAY_EXEC_KNOWN_HOSTS", "")
    s = AssaySettings()
    assert isinstance(s.exec_target, Ssh)
    assert (s.exec_target.host, s.exec_target.user, s.exec_target.port) == ("31.97.131.41", "root", None)
    assert s.exec_target.url == "ssh://root@31.97.131.41"


@pytest.mark.parametrize(
    "backend, expected_protocol, expected_root, strategy",
    [
        (None, "sftp", "/srv/work/run-1/.artifacts/assay", PullStrategy.TRANSFER),
        ({"protocol": "s3", "root": "bucket/runs"}, "s3", "bucket/runs/run-1/.artifacts/assay", PullStrategy.SHARED),
    ],
    ids=["sftp_derived", "shared_cloud"],
)
def test_settings_offload_derivation_matrix(
    backend: dict[str, str] | None, expected_protocol: str, expected_root: str, strategy: PullStrategy, tmp_path: Path
) -> None:
    """A remote Ssh target derives one Offload binding pinned under the target's remote run dir.

    Host and backend cannot disagree — the backend is never a separate knob: sftp byte-transfer by default, the
    configured SHARED cloud store when admitted, with the pull strategy read off the capability table.
    """
    (tmp_path / "Workspace.slnx").write_text("", encoding="utf-8")
    payload: dict[str, object] = {
        "root": UPath(tmp_path),
        "exec_target": "ssh://host:22",
        "exec_known_hosts": None,
        "run_id": "run-1",
        "exec_workroot": "/srv/work",
        **({"artifact_backend": backend} if backend else {}),
    }
    offload = AssaySettings.model_validate(payload).offload
    assert offload is not None
    assert (offload.backend.protocol, offload.backend.root) == (expected_protocol, expected_root)
    assert offload.pull_strategy is strategy


@pytest.mark.parametrize(
    "protocol, reachable, admitted, strategy",
    [
        ("file", False, True, PullStrategy.NONE),  # local landing store: reachable=False, no pull
        ("sftp", True, True, PullStrategy.TRANSFER),  # byte-download remote-exec backend
        ("s3", True, True, PullStrategy.SHARED),  # admitted shared object store: agent reads the tool-written tree, zero byte transfer
        ("gs", True, True, PullStrategy.SHARED),
        ("gcs", True, True, PullStrategy.SHARED),
        ("http", False, False, PullStrategy.NONE),  # unknown protocol: unreachable, not admitted, pulls nothing
    ],
    ids=["file", "sftp", "s3", "gs", "gcs", "unknown"],
)
def test_backend_capability_table_owns_admission(protocol: str, reachable: bool, admitted: bool, strategy: PullStrategy) -> None:  # noqa: FBT001  # parametrize columns: pytest injects the expectation flags positionally by name
    """One capability table owns reachability, admission, and pull strategy per backend protocol.

    Re-admitting a cloud shared store is one row's admitted=True flip; the table never widens into a dispatch chain.
    """
    assert backend_capability(protocol) == (reachable, admitted, strategy)
    assert ArtifactBackend(protocol=protocol, root="x/y").capability == (reachable, admitted, strategy)


# --- [BOUNDARY_VALIDATOR_LAWS]


def test_ssh_parse_accepts_root_path_only() -> None:
    """Ssh.parse accepts only empty or root slash paths after the ssh host.

    Accepted rows pin the exact round-trip URL; deeper paths must remain rejected.
    """
    validity_matrix(
        (
            ("bare", "ssh://host", True),
            ("user_port", "ssh://user@host:22", True),
            ("root_slash", "ssh://host/", True),
            ("user_port_slash", "ssh://user@host:22/", True),
            ("non_root_path", "ssh://host/sub", False),
        ),
        _ssh_parse_ok,
    )
    assert Ssh.parse("ssh://host/").url == "ssh://host"


def _ssh_parse_ok(url: str) -> bool:
    try:
        Ssh.parse(url)
    except ValueError:
        return False
    return True


def test_ssh_resolve_home_rebinds_tilde_to_absolute_workroot() -> None:
    """``resolve_home`` rebinds a leading ``~`` workroot to the connection's absolute home; an absolute workroot is unchanged.

    The agent resolves the remote ``~`` once (``sftp.realpath('.')``) so the SFTP push and the login-shell ``cd`` share one
    absolute run dir — no literal ``~`` reaches SFTP. ``connect_kwargs`` defaults the port to 22 (asyncssh binds ``None`` as 0).
    """
    tilde = Ssh.parse("ssh://root@host")  # default workroot ~/.assay-work, no explicit port
    resolved = tilde.resolve_home("/root")
    assert resolved.remote_workroot("run-9") == "/root/.assay-work/run-9", "leading ~ must rebind to the absolute home"
    absolute = tilde.model_copy(update={"workroot": "/srv/work"})
    assert absolute.resolve_home("/root") is absolute, "an absolute workroot must resolve to itself (no copy)"
    tildes = (resolve_tilde("~", "/home/u"), resolve_tilde("~/x/y", "/home/u/"), resolve_tilde("/abs/path", "/home/u"))
    assert tildes == ("/home/u", "/home/u/x/y", "/abs/path"), "resolve_tilde rebinds ~/~/x and passes an absolute path through"
    assert tilde.connect_kwargs["port"] == 22, "a portless URL must default to 22 for asyncssh (None binds as port 0)"
    assert remote_path("/root") == "/root/.local/bin:/usr/local/dotnet:/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin"


def test_unframe_inflates_zstd_frame_and_passes_plain_bytes_through() -> None:
    """The store read boundary inflates a zstd-framed payload and passes a non-framed payload through unchanged.

    The frame magic discriminates compressed history frames from plain SARIF/coverage artifacts, so one boundary
    serves both kinds with no per-consumer codec knowledge.
    """
    payload = b'{"run": "envelope", "status": "ok"}' * 8
    framed = zstandard.ZstdCompressor(level=10).compress(payload)
    assert framed[:4] == zstandard.FRAME_HEADER, "a zstd frame must carry the magic prefix unframe sniffs"
    assert unframe(framed) == payload, "a framed payload must inflate to the original bytes"
    assert unframe(payload) == payload, "a non-framed payload (no magic prefix) must pass through unchanged"


@pytest.mark.parametrize(
    "value, expected",
    [(None, None), ("", None), ("/abs/host_keys", "/abs/host_keys"), ("~", str(UPath("~").expanduser()))],
    ids=["none", "empty", "absolute", "tilde_expanded"],
)
def test_expand_or_none_projection(value: str | None, expected: str | None) -> None:
    """_expand_or_none folds None/empty to None and expands every other path.

    Absolute and tilde rows pin the live expansion branch; the empty row pins the sentinel arm.
    """
    assert _settings_mod._expand_or_none(value) == expected


@pytest.mark.parametrize(
    "reported, expected",
    [(0, 8), (1, 1), (8, 8), (257, 256)],
    ids=["zero_falls_to_eight", "one_keeps_floor", "eight_identity", "over_cap_clamps_256"],
)
def test_default_cpu_count_clamp_table(reported: int, expected: int, monkeypatch: pytest.MonkeyPatch) -> None:
    """_default_cpu_count clamps to [1, 256] and falls back to 8 for falsy counts.

    The table pins cap, floor, fallback, and boolean-coalescing arithmetic without host CPU drift.
    """
    monkeypatch.setattr(_settings_mod.os, "process_cpu_count", lambda: reported)
    assert _settings_mod._default_cpu_count() == expected


def test_assay_settings_remote_env_filters_and_injects_run_id(assay_root: AssayHarness) -> None:
    """remote_env injects ASSAY_RUN_ID, retains allowed trace context, strips unsafe keys, and forwards declared row env.

    The ``forward`` set carries deliberately-declared row-Tool env keys across the SSH boundary while the
    ambient allowlist keeps gating undeclared host env — an asymmetry fix for the local-vs-remote env merge.
    """
    # The agent PATH in the source is the macOS PATH; remote_env must replace it with the injected Linux toolchain PATH, not forward it.
    source = {"traceparent": "00-a-b-01", "baggage": "k=v", "UNSAFE": "x", "ROW_DECLARED": "row", "ROW_UNDECLARED": "drop", "PATH": "/mac/path"}
    env = assay_root.settings.remote_env(source, home="/root", forward=frozenset({"ROW_DECLARED"}))
    assert env == IsPartialDict({"ASSAY_RUN_ID": IsStr, "traceparent": "00-a-b-01", "baggage": "k=v", "ROW_DECLARED": "row"})
    assert "UNSAFE" not in env, "ambient non-allowlisted key leaked across SSH"
    assert "ROW_UNDECLARED" not in env, "an undeclared row key must not cross the boundary on the allowlist alone"
    # The injected Linux toolchain PATH replaces the agent's local PATH and resolves ~ against the connection home for uv/dotnet reach.
    assert env["PATH"] == "/root/.local/bin:/usr/local/dotnet:/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin"
    assert "/mac/path" not in env["PATH"], "the agent's local macOS PATH must never cross to a Linux host"
    bare = assay_root.settings.remote_env(source, home="/root")
    assert "ROW_DECLARED" not in bare, "forward must be opt-in: the bare allowlist still gates row keys"


def test_assay_settings_with_configuration_roundtrip(assay_root: AssayHarness) -> None:
    """with_configuration rebinds via model_copy (no re-validation), switches configuration, and preserves all other fields."""
    debug = assay_root.settings.with_configuration(Configuration.DEBUG)
    release = debug.with_configuration(Configuration.RELEASE)
    assert (debug.configuration, release.configuration) == (Configuration.DEBUG, Configuration.RELEASE)
    assert debug.root == assay_root.settings.root
    assert release.run_id == assay_root.settings.run_id


def test_assay_settings_artifact_projects_safe_path(assay_root: AssayHarness) -> None:
    """artifact() produces a path rooted under store_root/<kind>/<safe-segment...>."""
    path = str(assay_root.settings.artifact(ArtifactKind.CODE, "search", "run-1", "matches.txt"))
    assert ArtifactKind.CODE.value in path
    assert "matches.txt" in path


@pytest.mark.parametrize(
    "part", ["..", "a/", "/abs", "a\\b", "\x00null", "a//b"], ids=["dotdot", "trailing_slash", "absolute", "backslash", "null", "double_slash"]
)
def test_assay_settings_artifact_rejects_unsafe_segments(part: str, assay_root: AssayHarness) -> None:
    """artifact() rejects traversal / unsafe path segments before they reach the backend."""
    with pytest.raises(ValueError, match="unsafe artifact path segment"):
        assay_root.settings.artifact(ArtifactKind.CODE, part, "matches.txt")


def test_assay_settings_store_honors_protocol_and_forwards_opts(assay_root: AssayHarness) -> None:
    """Store builds the requested filesystem and forwards backend opts.

    The memory branch rejects None-protocol fallback to local FS; auto_mkdir=False pins option forwarding onto the constructed backend.
    """
    from fsspec.implementations.local import LocalFileSystem  # noqa: PLC0415  # backend-class identity probe is the only fsspec-impl import here
    from fsspec.implementations.memory import MemoryFileSystem  # noqa: PLC0415  # paired backend identity probe

    mem = assay_root.settings.store(protocol="memory", root=f"store-proto/{assay_root.settings.run_id}")
    assert isinstance(mem.fs, MemoryFileSystem)
    mem.write_bytes(b"x", "probe.bin")
    from pathlib import Path as _Path  # noqa: PLC0415  # on-disk leak probe: a None-protocol mutant would land bytes on the real local FS

    assert not _Path(str(assay_root.settings.store_root / "probe.bin")).exists(), "memory store must not write to the local file backend"

    no_mkdir = assay_root.settings.store(protocol="file", auto_mkdir=False)
    assert isinstance(no_mkdir.fs, LocalFileSystem)
    assert no_mkdir.fs.auto_mkdir is False, "store must forward **opts (auto_mkdir) onto the constructed filesystem"


# --- [ARTIFACT_SCOPE_LAWS]


@pytest.mark.parametrize("claim", list(Claim))
def test_artifact_scope_open_computes_path_lazily_per_claim(claim: Claim, assay_root: AssayHarness) -> None:
    """ArtifactScope.open computes claim/run-id paths without materializing directories.

    Segment checks catch claim swaps; the no-empty-dir assertion catches eager store.ensure.
    """
    from pathlib import Path as _Path  # noqa: PLC0415  # local: filesystem-existence probe is the only on-disk check in this module

    scope = ArtifactScope.open(assay_root.settings, claim)
    assert isinstance(scope, ArtifactScope)
    assert claim.value in scope.path
    assert assay_root.settings.run_id in scope.path
    assert {"--artifacts-path", scope.path} <= set(scope.dotnet_flags)
    assert "--disable-build-servers" not in scope.dotnet_flags  # VBCSCompiler stays on; --artifacts-path is the isolation boundary
    assert not _Path(scope.path).exists(), "open() must not materialize the scope directory (no empty scope dirs)"


def test_artifact_scope_ensure_materializes_through_store_boundary(assay_root: AssayHarness) -> None:
    """ArtifactScope.ensure materializes lazy scopes through the ArtifactStore boundary.

    The returned path is scope.path, repeated calls are idempotent, and escaped backend paths are rejected before raw fsspec access.
    """
    from pathlib import Path as _Path  # noqa: PLC0415  # local: filesystem-existence probe

    scope = ArtifactScope.open(assay_root.settings, Claim.DOCS)
    assert not _Path(scope.path).exists(), "open() leaves the scope dir absent"
    created = scope.ensure()
    assert created == scope.path
    assert _Path(scope.path).exists(), "ensure() materializes the scope dir"
    assert scope.ensure() == scope.path, "ensure() is idempotent on an existing dir"
    with pytest.raises(ValueError, match="escaped store root"):
        scope.store.ensure_path("/outside/the/store/root")


@pytest.mark.parametrize(
    "config_arg, expect_in_path",
    [(None, "Release"), (Configuration.DEBUG, "Debug"), ("Custom", "Custom")],
    ids=["default", "enum_debug", "str_custom"],
)
def test_artifact_scope_build_configuration_variants(config_arg: Configuration | str | None, expect_in_path: str, assay_root: AssayHarness) -> None:
    """ArtifactScope.build embeds the correct configuration segment for None/enum/str inputs."""
    scope = ArtifactScope.build(assay_root.settings, "my-closure", config_arg)
    assert expect_in_path in scope.path
    assert "my-closure" in scope.path
    assert "--artifacts-path" in scope.dotnet_flags


def test_artifact_scope_dotnet_isolation_policy(assay_root: AssayHarness) -> None:
    """Scopes route artifacts via DOTNET_CLI_HOME + --artifacts-path; VBCSCompiler shared compilation stays on for open and build."""
    for scope in (ArtifactScope.open(assay_root.settings, Claim.STATIC), ArtifactScope.build(assay_root.settings, "static-closure")):
        assert scope.dotnet_env == IsPartialDict({"DOTNET_CLI_HOME": IsStr(regex=rf"{scope.path}.*")})
        assert "MSBUILDDISABLENODEREUSE" not in scope.dotnet_env
        assert "--disable-build-servers" not in scope.dotnet_flags
        assert scope.path in scope.dotnet_flags


# --- [PROTOCOL]


def test_artifact_file_system_protocol_contract(mem_store: ArtifactStore) -> None:
    """ArtifactFileSystem is @runtime_checkable and its default transaction is a usable nullcontext."""
    assert isinstance(mem_store.fs, ArtifactFileSystem)

    class _DefaultTxFs(_FsStub):
        # Re-bind the Protocol default so the property path is exercised, not _FsStub's class-level attribute.
        transaction = ArtifactFileSystem.transaction

    ctx = _DefaultTxFs().transaction
    assert isinstance(ctx, contextlib.AbstractContextManager)
    with ctx:
        pass


# --- [ARTIFACT_STORE_IO_LAWS]


@pytest.mark.parametrize(
    "payload",
    [b"", b"\x00\xff", b"utf8-text", b"\n" * 512, bytes(range(256)), b"round-trip-payload-\x00\xff"],
    ids=["empty", "binary_extremes", "text", "newlines", "full_byte_range", "history_payload"],
)
def test_artifact_store_write_bytes_identity(payload: bytes, mem_store: ArtifactStore) -> None:
    """write_bytes + read_bytes is an identity roundtrip over a representative payload case-table."""
    roundtrip(
        payload,
        lambda p: mem_store.write_bytes(p, "scope", f"data-{len(p)}.bin"),
        lambda _path: mem_store.read_bytes("scope", f"data-{len(payload)}.bin"),
    )


def test_artifact_store_io_lifecycle(mem_store: ArtifactStore, tmp_path: Path) -> None:
    """Text/path write variants round-trip and overwrite in place; glob/info/write_many/open_write/adopt/remove close the lifecycle."""
    roundtrip(
        "alpha\nbeta\ngamma\n",
        lambda t: mem_store.write_text(t, "scope", "api", "surface.txt"),
        lambda _p: mem_store.read_text("scope", "api", "surface.txt"),
    )
    path = mem_store.write_bytes(b"original", "scope", "overwrite.txt")
    mem_store.write_bytes_path(b"new-bytes", path)
    assert mem_store.read_bytes("scope", "overwrite.txt") == b"new-bytes"
    mem_store.write_text_path("new-text", path)
    assert mem_store.read_text("scope", "overwrite.txt") == "new-text"
    assert mem_store.read_text_path(path) == "new-text"

    mem_store.write_bytes(b"payload", "history", "run-1", "envelope.json")
    assert any("envelope.json" in m for m in mem_store.glob("history/*/envelope.json"))
    meta = mem_store.write_bytes(b"meta-check", "scope", "meta.txt")
    assert mem_store.info("scope", "meta.txt") == IsPartialDict({"type": "file"})
    assert mem_store.info_path(meta) == IsPartialDict({"type": "file"})
    paths = mem_store.write_many(((b"alpha", ("scope", "a.txt")), (b"beta", ("scope", "b.txt"))))
    assert len(paths) == 2
    assert (mem_store.read_bytes("scope", "a.txt"), mem_store.read_bytes("scope", "b.txt")) == (b"alpha", b"beta")


def test_artifact_store_exists_size_remove_open_adopt(mem_store: ArtifactStore, tmp_path: Path) -> None:
    """exists/exists_path/size_path agree on a written payload; open_write, adopt_file, remove_path round out the lifecycle."""
    payload = b"size-check-payload"
    backend = mem_store.write_bytes(payload, "scope", "check.txt")
    assert mem_store.exists("scope", "check.txt")
    assert mem_store.exists_path(backend)
    assert mem_store.size_path(backend) == len(payload)
    assert not mem_store.exists("scope", "missing.txt")
    stream_path, fh = mem_store.open_write("scope", "stream.bin")
    assert "stream.bin" in stream_path
    assert hasattr(fh, "write")
    local = tmp_path / "to-adopt.bin"
    local.write_bytes(b"adopted-content")
    mem_store.adopt_file(local, "artifacts", "copy.bin")
    assert mem_store.read_bytes("artifacts", "copy.bin") == b"adopted-content"
    mem_store.remove_path(backend)
    assert not mem_store.exists("scope", "check.txt")


def test_artifact_store_path_guards_reject(mem_store: ArtifactStore) -> None:
    """backend_path rejects an out-of-root path; write_bytes(create=True) rejects an existing target."""
    mem_store.write_bytes(b"payload", "history", "run-1", "envelope.json")
    with pytest.raises(ValueError, match="escaped store root"):
        mem_store.read_path("other/history/run-1/envelope.json")
    mem_store.write_bytes(b"first", "scope", "api", "once.txt")
    with pytest.raises(FileExistsError):
        mem_store.write_bytes(b"second", "scope", "api", "once.txt", create=True)


def test_artifact_store_write_at_atomic_commit_and_failure() -> None:
    """_write_at commits through the autocommit-deferred open handle and leaves no partial target on failure."""
    failing = _FsStub(write_fails=True)
    store = ArtifactStore(fs=failing, root="atomic-root")
    with pytest.raises(OSError, match="injected write failure"):
        store.write_bytes(b"payload", "scope", "partial.bin")
    assert not store.exists("scope", "partial.bin"), "a failed write must not leave a partial file at the target path"

    committing = ArtifactStore(fs=_FsStub(), root="atomic-root")
    path = committing.write_bytes(b"committed-via-open", "scope", "via-open.bin")
    assert committing.read_path(path) == b"committed-via-open"


# --- [ARTIFACT_STORE_WALK_LAWS]


def test_artifact_store_walk_detail_dict_keyed_branch() -> None:
    """walk(recursive=True, detail=True) projects a dict-keyed find result to sorted (path, info) rows."""
    rows = ArtifactStore(fs=_FsStub(), root="dict-root").walk("scope", recursive=True, detail=True)
    assert len(rows) == 1
    row = rows[0]
    assert isinstance(row, tuple)
    path, info = row
    assert "f.txt" in path
    assert info == IsPartialDict({"type": "file"})


def test_artifact_store_walk_absent_path_returns_empty_tuple(mem_store: ArtifactStore) -> None:
    """Walk catches FileNotFoundError on an absent path in both the find (recursive) and ls arms, returning ()."""
    assert mem_store.walk("missing-scope-entirely", recursive=True) == ()
    assert mem_store.walk("missing-scope-entirely", recursive=False) == ()


@pytest.mark.parametrize(
    "info_payload, reader",
    [({"name": "f", "type": "file", "mtime": "not-a-number"}, "mtime_path"), ({"name": "f", "type": "file", "size": "huge"}, "size_path")],
    ids=["mtime_non_numeric", "size_non_int"],
)
def test_artifact_store_info_path_non_numeric_fallback(info_payload: dict[str, object], reader: str) -> None:
    """mtime_path / size_path fall back to the falsy zero default (0.0 / 0) when backend metadata is non-numeric."""
    store = ArtifactStore(fs=_FsStub(info_payload=info_payload), root="info-root")
    assert not getattr(store, reader)("info-root/scope/f.txt")


# --- [ARTIFACT_STORE_HISTORY_LAWS]


def test_artifact_store_write_history_roundtrip_and_unknown(mem_store: ArtifactStore) -> None:
    """write_history + load_history restore the stored Envelope; unknown/empty/corrupt run ids load None."""
    env = make_history_envelope("run-hist-1")
    mem_store.write_history("run-hist-1", WIRE_ENCODER.encode(env))
    restored = mem_store.load_history("run-hist-1")
    assert restored is not None
    assert restored.run_id == env.run_id

    assert mem_store.load_history("does-not-exist") is None
    assert mem_store.load_history("") is None
    mem_store.write_bytes(b"{not valid json", ArtifactKind.HISTORY.value, "run-corrupt-env", "envelope.json")
    assert mem_store.load_history("run-corrupt-env") is None


@pytest.mark.parametrize("keep, survivors", [(1, 1), (0, 0)], ids=["keep_one", "keep_zero"])
def test_artifact_store_retain_history_prunes_oldest(keep: int, survivors: int, mem_store: ArtifactStore) -> None:
    """retain_history(keep=N) prunes oldest runs leaving exactly N survivors."""
    for i in range(3):
        mem_store.write_history(f"run-{i}", WIRE_ENCODER.encode(make_history_envelope(f"run-{i}")))
    mem_store.retain_history(keep=keep)
    present = [i for i in range(3) if mem_store.load_history(f"run-{i}") is not None]
    assert len(present) == survivors


@pytest.mark.parametrize("keep, expected", [(2, ("scope-1", "scope-2")), (1, ("scope-2",)), (0, ())], ids=["keep_two", "keep_one", "keep_zero"])
def test_artifact_store_retain_scopes_prunes_oldest_per_claim(keep: int, expected: tuple[str, ...]) -> None:
    """retain_scopes prunes oldest run dirs per claim using lexicographic fallback order.

    The stub omits mtime so ordering depends on run_id; the second claim root proves pruning is claim-local.
    """
    stub = _FsStub()
    store = ArtifactStore(fs=stub, root="scopes-root")
    for run in ("scope-0", "scope-1", "scope-2"):
        stub.seed(f"scopes-root/{Claim.STATIC.value}/{run}/artifact.bin", b"x")
    # A second claim root must be untouched: retain_scopes prunes one claim only.
    stub.seed(f"scopes-root/{Claim.CODE.value}/scope-0/artifact.bin", b"y")

    store.retain_scopes(Claim.STATIC, keep)

    survivors = tuple(run for run in ("scope-0", "scope-1", "scope-2") if store.exists(Claim.STATIC.value, run, "artifact.bin"))
    assert survivors == expected
    assert store.exists(Claim.CODE.value, "scope-0", "artifact.bin"), "retain_scopes must not prune a different claim's root"


def test_artifact_store_sorted_history_ids_mtime_tie_lexicographic_fallback() -> None:
    """sorted_history_ids breaks omitted-mtime ties by ascending run id.

    Dropping run_id from the sort key would make this stub's tie order backend-arbitrary.
    """
    stub = _FsStub()
    store = ArtifactStore(fs=stub, root="tie-root")
    for run in ("run-c", "run-a", "run-b"):
        stub.seed(f"tie-root/{ArtifactKind.HISTORY.value}/{run}/envelope.json", b"{}")
    assert store.sorted_history_ids() == ("run-a", "run-b", "run-c")


def test_artifact_store_retain_history_already_removed_is_tolerated() -> None:
    """retain_history tolerates FileNotFoundError during prune. The stub discovers a seeded run and raises only from rm, exercising the race arm."""
    racey_fs = _FsStub(rm_raises=True)
    store = ArtifactStore(fs=racey_fs, root="racey-root")
    racey_fs.seed("racey-root/history/run-race/envelope.json", WIRE_ENCODER.encode(make_history_envelope("run-race")))
    store.retain_history(keep=0)


def test_artifact_store_sorted_history_ids_order(mem_store: ArtifactStore, monkeypatch: pytest.MonkeyPatch) -> None:
    """sorted_history_ids returns run ids oldest-first using (mtime, run_id) as the rank key."""
    for run_id in ("run-b", "run-a"):
        mem_store.write_history(run_id, WIRE_ENCODER.encode(make_history_envelope(run_id)))
    monkeypatch.setattr(_store_mod, "mtime_from_info", lambda info: 1.0 if "run-a" in str(info.get("name", "")) else 2.0)
    ids = mem_store.sorted_history_ids()
    seeded = tuple(rid for rid in ids if rid in {"run-a", "run-b"})
    assert seeded == IsTuple("run-a", "run-b")  # mtime rank (1.0 < 2.0) orders run-a before run-b; both present, in order


def test_artifact_store_full_report_restore_matrix(mem_store: ArtifactStore) -> None:
    """restore_full_report: a present artifact restores the full Report; corrupt/absent/None-report degrade to original."""
    env = make_history_envelope("run-fr-1")
    assert env.report is not None
    report = env.report

    def _with_full_report(path: str | None) -> Envelope:
        artifacts = () if path is None else (Artifact(id="full-report", kind=ArtifactKind.HISTORY, path=path),)
        return msgspec.structs.replace(env, report=msgspec.structs.replace(report, artifacts=artifacts))

    bpath, nbytes = mem_store.write_full_report("run-fr-1", "full.json", report)
    assert nbytes > 0
    present = _with_full_report(bpath)
    restored = mem_store.restore_full_report(present)
    assert restored is not present
    assert restored.report is not None

    corrupt = _with_full_report(mem_store.write_bytes(b"{corrupt json", ArtifactKind.HISTORY.value, "run-fr-1", "corrupt.json"))
    assert mem_store.restore_full_report(corrupt) is corrupt

    no_art = _with_full_report(None)
    assert mem_store.restore_full_report(no_art) is no_art

    no_report = msgspec.structs.replace(env, report=None)
    assert mem_store.restore_full_report(no_report) is no_report


# --- [STATEFUL_HISTORY_RBSM]


class HistoryRetentionStateMachine(RuleBasedStateMachine):
    """State machine for history write, retain, and load protocol."""

    runs: Bundle[str] = Bundle("runs")

    def __init__(self) -> None:
        """Initialise process-local oracle state; @initialize owns the replay-stable store root."""
        super().__init__()
        self._store: ArtifactStore
        self._counter = 0  # monotonic write order == lexicographic run_id order == recency rank
        self._written: set[str] = set()

    @initialize(root_id=st.uuids())
    def open_store(self, root_id: uuid.UUID) -> None:
        """Bind a disjoint in-memory store under a hypothesis-drawn UUID root (replay-stable, fsspec-global-safe)."""
        self._store = ArtifactStore(fs=fsspec.filesystem("memory"), root=f"rbsm-store/{root_id}")

    @rule(target=runs)
    def write_run(self) -> str:
        self._counter += 1
        run_id = f"run-{self._counter:06d}"
        self._store.write_history(run_id, WIRE_ENCODER.encode(make_history_envelope(run_id)))
        self._written.add(run_id)
        return run_id

    @rule(keep=st.integers(min_value=0, max_value=5))
    def retain(self, keep: int) -> None:
        self._store.retain_history(keep=keep)
        sorted_written = sorted(self._written)
        pruned = set(sorted_written[: max(0, len(sorted_written) - keep)])
        self._written -= pruned
        # retain bounds only the current rule boundary; later writes may grow history again.
        assert len(self._store.sorted_history_ids()) <= keep, f"retain(keep={keep}) left {len(self._store.sorted_history_ids())} survivors"

    @invariant()
    def survivors_are_loadable(self) -> None:
        """Every id sorted_history_ids reports must load — no orphaned index entry listed without a payload."""
        ids = self._store.sorted_history_ids()
        orphans = tuple(rid for rid in ids if self._store.load_history(rid) is None)
        assert not orphans, f"orphaned index entries listed but load_history returned None: {orphans!r}"

    @invariant()
    def oracle_set_matches_reported_ids(self) -> None:
        """The set of loadable run ids must equal the oracle set derived from the monotonic write counter."""
        ids = set(self._store.sorted_history_ids())
        assert ids == self._written, f"oracle={self._written!r} reported={ids!r}"

    @override
    def teardown(self) -> None:
        """Purge the disjoint memory root to prevent cross-replay state leaks in the process-global fsspec memory filesystem."""
        self._store.remove_path(self._store.root, recursive=True) if self._store.exists_path(self._store.root) else None


def test_history_retention_state_machine() -> None:
    """Drive the ArtifactStore write/retain/load RBSM: survivors are always exactly the N most-recent runs."""
    model_based(HistoryRetentionStateMachine)


# --- [ARTIFACT_STORE_RESOLVE_ARTIFACTS_LAWS]


def test_artifact_store_resolve_artifacts_matrix(mem_store: ArtifactStore, monkeypatch: pytest.MonkeyPatch) -> None:
    """resolve_artifacts: empty tokens return (), basename matches across roots, substring and direct paths hit, latest ranks by root priority."""
    mem_store.write_bytes(b"a", "scope", "needle.txt")
    assert mem_store.resolve_artifacts("", "scope") == ()
    assert mem_store.resolve_artifacts("  ", "scope") == ()

    mem_store.write_bytes(b"b", "history", "run-1", "needle.txt")
    basename = mem_store.resolve_artifacts("needle.txt", "scope", "history")
    assert len(basename) == 2
    assert all("needle.txt" in m for m in basename)
    mem_store.write_bytes(b"c", "scope", "mydir", "file.txt")
    assert any("mydir" in m for m in mem_store.resolve_artifacts("mydir", "scope", "history"))
    direct = mem_store.write_bytes(b"direct", "scope", "direct.txt")
    assert mem_store.resolve_artifacts(direct, "scope") == (direct,)

    mem_store.write_bytes(b"first", "scope", "a.txt")
    mem_store.write_bytes(b"newer", "history", "run-x", "b.txt")
    # history's b.txt is globally newest; a.txt is merely scope's newest — ranked[0]=a.txt proves root priority beats recency.
    ranks = {"b.txt": 3.0, "a.txt": 2.0}
    monkeypatch.setattr(_store_mod, "mtime_from_info", lambda info: ranks.get(str(info.get("name", "")).rsplit("/", 1)[-1], 1.0))
    ranked = mem_store.resolve_artifacts("latest", "scope", "history", latest=True)
    assert len(ranked) >= 1
    assert "a.txt" in ranked[0], "the first non-empty root's newest file wins (root priority)"
    assert mem_store.resolve_artifacts("latest", "empty-scope", "empty-history", latest=True) == ()


# --- [ARTIFACT_STORE_OPTION_LAWS]


def test_artifact_store_load_history_option_projection(mem_store: ArtifactStore) -> None:
    """load_history projects to Some(Envelope) for a stored run and None for an absent run (oracle laws)."""
    from expression import Option  # noqa: PLC0415  # oracle-only; top-level import would surface expression as a module-level dependency

    mem_store.write_history("run-opt", WIRE_ENCODER.encode(make_history_envelope("run-opt")))
    assert_some(Option.of_optional(mem_store.load_history("run-opt")))
    assert_none(Option.of_optional(mem_store.load_history("absent-run")))
    idempotent(b"idem-payload", lambda p: (mem_store.write_bytes(p, "scope", "idem.bin"), mem_store.read_bytes("scope", "idem.bin"))[1])


# --- [ARTIFACT_PATH_POLICY]


def test_artifact_roots_route_all_heavy_lanes() -> None:
    """PY/CS artifact root tables own every heavy-lane output path; coverage files and the Stryker work tree derive from them.

    DOTNET_BUILD_CLOSURE stays one shared solution tree id — never per-claim or per-sha keys.
    """
    assert set(PY_ARTIFACT_ROOTS) == {"coverage", "benchmarks", "mutmut"}
    assert all(root.startswith(".artifacts/python/") for root in PY_ARTIFACT_ROOTS.values())
    assert set(PY_COVERAGE_FILES) == {"json", "xml", "lcov"}
    assert all(path == f"{PY_ARTIFACT_ROOTS['coverage']}/coverage.{fmt}" for fmt, path in PY_COVERAGE_FILES.items())
    assert set(CS_ARTIFACT_ROOTS) == {"stryker", "stryker-output", "trx"}
    assert all(root.startswith(".artifacts/csharp/") for root in CS_ARTIFACT_ROOTS.values())
    assert CS_ARTIFACT_ROOTS["stryker"].startswith(f"{CS_ARTIFACT_ROOTS['stryker-output']}/"), "work tree nests under the report root"
    assert DOTNET_BUILD_CLOSURE == "dotnet"
    assert "/" not in DOTNET_BUILD_CLOSURE


def test_prune_python_artifacts_bounds_benchmark_autosaves(assay_root: AssayHarness) -> None:
    """prune_python_artifacts keeps the newest ``keep`` benchmark autosaves; the self-overwriting coverage root is never child-pruned."""
    root = Path(str(assay_root.root))
    bench = root / PY_ARTIFACT_ROOTS["benchmarks"] / "machine"
    bench.mkdir(parents=True)
    for i in range(5):
        target_file = bench / f"{i:04d}_run.json"
        target_file.write_text("{}", encoding="utf-8")
        os.utime(target_file, (i, i))  # ascending mtime: 0000 oldest, 0004 newest
    coverage = root / PY_ARTIFACT_ROOTS["coverage"]
    coverage.mkdir(parents=True)
    (coverage / "coverage.json").write_text("{}", encoding="utf-8")
    prune_python_artifacts(root, keep=2)
    assert sorted(p.name for p in bench.glob("*.json")) == ["0003_run.json", "0004_run.json"]
    assert (coverage / "coverage.json").exists()


# --- [STORE_PATH_LAW]


@pytest.mark.parametrize("segment", ["run-1", "a.b-c_d", "envelope.json"], ids=["run-id", "dotted", "file"])
def test_safe_segment_admits_single_clean_pieces(segment: str) -> None:
    """safe_segment is the identity on single, relative, NUL-free path pieces."""
    assert safe_segment(segment) == segment


@pytest.mark.parametrize(
    "segment",
    ["../up", "a/b", "/abs", "", ".", "..", "nul\x00byte", "tail/"],
    ids=["traversal", "multi", "abs", "empty", "dot", "parent", "nul", "trailing-slash"],
)
def test_safe_segment_rejects_unsafe_pieces(segment: str) -> None:
    """safe_segment rejects absolute, empty, dotted, multi-piece, and NUL-bearing segments."""
    with pytest.raises(ValueError, match="unsafe artifact path segment"):
        _ = safe_segment(segment)
