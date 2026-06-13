"""Laws for tools.assay.composition.settings.

Scope: ArtifactBackend, ArtifactFileSystem, ArtifactScope, ArtifactStore, AssaySettings,
Configuration, LogFormat, mtime_from_info.

Law design:
- @pytest.mark.parametrize + register_law for case-table laws (enum sweeps, validation/projection matrices).
- @spec for single-property generated-instance laws; @given for the numeric coercion identities.
- Direct fixture laws (mem_store / assay_root) for store I/O and settings integration; tests.python._testkit.spec oracles
  (assert_some/assert_none/roundtrip/idempotent/validity_matrix) replace hand-rolled is_ok/equality bodies.
- One polymorphic _FsStub drives every walk/retain/info edge arm via constructor flags — no per-arm class.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import contextlib
from datetime import datetime, UTC
import operator
import tempfile
from typing import Final, override, Self, TYPE_CHECKING
import uuid  # noqa: TC003  # runtime import: open_store's annotation is evaluated eagerly (no `from __future__ import annotations`)

from dirty_equals import IsPartialDict, IsStr, IsTuple
import fsspec
from hypothesis import given, settings as hyp_settings, strategies as st, target
from hypothesis.stateful import Bundle, initialize, invariant, rule, RuleBasedStateMachine
import msgspec
from pydantic import ValidationError
import pytest
from upath import UPath

from tests.python._testkit.laws import register_law
from tests.python._testkit.spec import assert_none, assert_some, idempotent, model_based, roundtrip, validity_matrix
from tests.python.tools.assay.kit import (  # noqa: TC001  # runtime use: instantiated in fixture bodies, not annotation-only
    AssayHarness,
    make_history_envelope,
    WIRE_ENCODER,
)
from tools.assay.composition import settings as _settings_mod
from tools.assay.composition.settings import (
    ArtifactBackend,
    ArtifactFileSystem,
    ArtifactScope,
    ArtifactStore,
    AssaySettings,
    Configuration,
    LogFormat,
    mtime_from_info,
)
from tools.assay.core.model import Artifact, ArtifactKind, Claim


if TYPE_CHECKING:
    from collections.abc import Callable
    from enum import StrEnum
    from pathlib import Path

    from tools.assay.core.model import Envelope


# --- [CONSTANTS] ------------------------------------------------------------------------

_CPU_MIN: Final = 1
_CPU_MAX: Final = 256
_CPU_BELOW: Final = 0
_CPU_ABOVE: Final = 300

# --- [BOUNDARIES] -----------------------------------------------------------------------


class _StubWriter(contextlib.AbstractContextManager[object]):
    """Autocommit-deferred write handle: buffers bytes, committing to the backing dict only on clean __exit__.

    Mirrors fsspec LocalFileOpener(autocommit=False): a write that raises before clean exit leaves the target
    path untouched (no partial file), which is the write-atomicity contract _write_at relies on.
    """

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
                pass  # exception in the with-body: discard the buffer, leave the target path unwritten


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
        # Immediate run-dir children of `path`, derived from seeded keys; rows carry NO mtime so the
        # (mtime, run_id) rank collapses onto the lexicographic run-id tiebreaker (no monkeypatch).
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
                # Recursive prune: drop the dir key and every seeded child under `<path>/`.
                prefix = f"{path.rstrip('/')}/"
                for key in [p for p in self._data if p == path or p.startswith(prefix)]:
                    del self._data[key]
                return None
            case _:
                self._data.pop(path, None)
                return None

    @override
    def open(self, path: str, mode: str = "rb", *, autocommit: bool = True) -> contextlib.AbstractContextManager[object]:
        # autocommit-aware seam: _write_at writes through this handle; the in-memory dict is the commit target.
        return _StubWriter(self._data, path, fail=self._write_fails)

    transaction: contextlib.AbstractContextManager[object] = contextlib.nullcontext()


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [STR_ENUM_LAWS]


@pytest.mark.parametrize("enum_cls", [Configuration, LogFormat], ids=["configuration", "log_format"])
def test_str_enum_value_roundtrip(enum_cls: type[StrEnum]) -> None:
    """Every member is a str whose value is its str() form and round-trips Enum(str(member)) is member."""
    for member in enum_cls:
        assert isinstance(member, str)
        assert str(member) == member.value
        roundtrip(member, str, enum_cls, eq=operator.is_)


register_law(Configuration, "str_enum_value_roundtrip")
register_law(LogFormat, "str_enum_value_roundtrip")

# --- [MTIME_FROM_INFO_LAWS]


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


register_law(mtime_from_info, "mtime_from_info_projection")


@given(st.one_of(st.floats(min_value=0.0, max_value=1e12, allow_nan=False, allow_infinity=False), st.integers(min_value=0, max_value=2**53 + 2)))
@hyp_settings(max_examples=80)
def test_mtime_from_info_numeric_identity(v: float) -> None:
    """mtime_from_info returns float(v) for finite non-negative float/int mtime — exact within float()'s own promotion."""
    result = mtime_from_info({"mtime": v})
    assert isinstance(result, float)
    assert result == float(v)  # noqa: RUF069  # float() int→float promotion is deterministic; precision loss past 2^53 is the contract
    target(float(v), label="mtime_magnitude")  # biases Hypothesis toward 2^53+1, where int→float first loses precision


register_law(mtime_from_info, "mtime_from_info_numeric_identity")

# --- [ARTIFACT_BACKEND_LAWS]


def test_artifact_backend_defaults_to_file_protocol() -> None:
    """ArtifactBackend default protocol is 'file' with an empty root."""
    b = ArtifactBackend()
    assert b.protocol == "file"
    assert not b.root


register_law(ArtifactBackend, "artifact_backend_defaults_to_file_protocol")


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


register_law(ArtifactBackend, "artifact_backend_validation_matrix")


@pytest.mark.parametrize(
    "protocol, root, expect_part",
    [
        ("file", "", ".artifacts/assay"),  # file+empty -> store_root
        ("file", "rel/path", "rel/path"),  # file+relative -> joined to workspace root
        ("memory", "my-prefix", "my-prefix"),  # non-file -> root stripped
    ],
    ids=["file_empty", "file_relative", "non_file_root"],
)
def test_artifact_backend_target_dispatch(protocol: str, root: str, expect_part: str, assay_root: AssayHarness) -> None:
    """ArtifactBackend.target dispatches to the correct root path for each protocol/root pairing."""
    assert expect_part in ArtifactBackend(protocol=protocol, root=root).target(assay_root.settings)


def test_artifact_backend_target_file_absolute(assay_root: AssayHarness) -> None:
    """ArtifactBackend.target with an absolute file root returns that absolute path directly."""
    with tempfile.TemporaryDirectory() as td:
        assert ArtifactBackend(protocol="file", root=td).target(assay_root.settings) == td


register_law(ArtifactBackend, "artifact_backend_target_dispatch")
register_law(ArtifactBackend, "artifact_backend_target_file_absolute")


def test_artifact_backend_target_relative_file_root_joins_workspace(assay_root: AssayHarness) -> None:
    """A relative file root is anchored under settings.root, not returned bare.

    Falsifies the three mutations that drop or fail to match the `("file", root)` arm (arm deletion, and the
    `"file"` literal mutated to `"XXfileXX"` / `"FILE"`): each makes a relative file root fall through to the
    non-file `root.rstrip("/")` arm, returning the bare `"rel/path"` instead of the workspace-absolute join. The
    equality (not substring) assertion plus is_absolute pin the full join, which `"rel/path" in result` could not.
    """
    settings = assay_root.settings
    result = ArtifactBackend(protocol="file", root="rel/path").target(settings)
    assert result == str(settings.root / UPath("rel/path"))
    assert UPath(result).is_absolute()
    assert result != "rel/path"


register_law(ArtifactBackend, "artifact_backend_target_relative_file_root_joins_workspace")


@pytest.mark.parametrize(
    "root, expected",
    [("my-prefix/", "my-prefix"), ("my-prefix//", "my-prefix"), ("prefixX", "prefixX")],
    ids=["trailing_slash", "double_trailing_slash", "trailing_capital_x"],
)
def test_artifact_backend_target_non_file_root_rstrips_only_slashes(root: str, expected: str, assay_root: AssayHarness) -> None:
    """A non-file backend root is right-stripped of '/' exactly — never whitespace, left side, or other chars.

    Pins `root.rstrip("/")` against three survivors: `rstrip(None)` (strips whitespace, leaves trailing slash),
    `lstrip("/")` (strips the wrong end), and `rstrip("XX/XX")` (also strips a trailing 'X'). The trailing-slash
    rows kill the first two; the `prefixX` row kills the third because production keeps the 'X' while the mutant
    eats it.
    """
    assert ArtifactBackend(protocol="memory", root=root).target(assay_root.settings) == expected


register_law(ArtifactBackend, "artifact_backend_target_non_file_root_rstrips_only_slashes")

# --- [ASSAY_SETTINGS_LAWS]


def test_assay_settings_cpu_count_boundary(assay_root: AssayHarness) -> None:
    """cpu_count rejects values outside [1, 256] and accepts the inclusive endpoints exactly.

    A mutant dropping ge=1 would admit 0; a mutant dropping le=256 would admit 300; both must fail,
    while 1 and 256 must construct, pinning the bound to exactly [1, 256] rather than a looser variant.
    """

    def _build(cpu_count: int) -> AssaySettings:
        return AssaySettings(root=assay_root.settings.root, exec_target="", exec_known_hosts=None, cpu_count=cpu_count)

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


register_law(AssaySettings, "assay_settings_cpu_count_boundary")


def test_assay_settings_default_fields_present(assay_root: AssayHarness) -> None:
    """S2 defaults: non-empty probe_fixture_prefixes tuple[str], non-empty mutation_python, typed enum fields."""
    s = assay_root.settings
    assert s.probe_fixture_prefixes == IsTuple(length=(1, ...))
    assert all(isinstance(p, str) for p in s.probe_fixture_prefixes)
    assert s.mutation_python == IsStr(min_length=1)
    assert isinstance(s.configuration, Configuration)
    assert isinstance(s.log_format, LogFormat)


register_law(AssaySettings, "assay_settings_default_fields_present")


def test_assay_settings_computed_projections(assay_root: AssayHarness) -> None:
    """Computed solution / agent_context / python_tool_env project off root, run_id, and agent_task_id."""
    s = assay_root.settings
    assert s.solution == s.root / "Workspace.slnx"
    assert s.agent_context == IsPartialDict({"run.id": s.run_id, "agent.task.id": s.agent_task_id})
    env = s.python_tool_env
    assert env.keys() >= {"UV_CACHE_DIR", "HYPOTHESIS_STORAGE_DIRECTORY", "PYTEST_CACHE_DIR", "RUFF_CACHE_DIR", "MYPY_CACHE_DIR", "COVERAGE_FILE"}
    root_str = str(s.local_root)
    assert all(v.startswith(root_str) for v in env.values())


register_law(AssaySettings, "assay_settings_computed_projections")


def test_assay_settings_local_root_raises_for_non_file_protocol() -> None:
    """local_root raises when root uses a non-file protocol; construction itself still succeeds.

    A memory:// UPath has .protocol == 'memory'; _anchor folds to the cursor (no Workspace.slnx ancestor
    in the virtual FS), so the model builds and only local_root rejects the non-file protocol.
    """
    settings = AssaySettings(root=UPath("memory://bucket/scope"), exec_target="", exec_known_hosts=None)
    with pytest.raises(ValueError, match="file protocol"):
        _ = settings.local_root


register_law(AssaySettings, "assay_settings_local_root_raises_for_non_file_protocol")


def test_assay_settings_wire_safe_scrubs_surrogates(tmp_path: Path) -> None:
    """_wire_safe replaces lone surrogates in run_id / agent_task_id so the result is UTF-8 encodable."""
    (tmp_path / "Workspace.slnx").write_text("", encoding="utf-8")
    settings = AssaySettings(root=UPath(tmp_path), run_id="run-1", agent_task_id="run-\udcff", exec_target="", exec_known_hosts=None)
    settings.agent_task_id.encode("utf-8")


register_law(AssaySettings, "assay_settings_wire_safe_scrubs_surrogates")


@pytest.mark.parametrize(
    "exec_target, error_match",
    [("ssh://user@host:notaport", "non-numeric ssh port"), ("http://host/path", "without path/query/fragment")],
    ids=["non_numeric_port", "non_ssh_scheme"],
)
def test_assay_settings_exec_target_rejected(exec_target: str, error_match: str, tmp_path: Path) -> None:
    """exec_target rejects non-numeric ssh ports and non-ssh schemes with a diagnostic ValidationError."""
    (tmp_path / "Workspace.slnx").write_text("", encoding="utf-8")
    with pytest.raises(ValidationError, match=error_match):
        AssaySettings(root=UPath(tmp_path), exec_target=exec_target, exec_known_hosts=None)


def test_assay_settings_exec_target_valid_ssh_with_port(tmp_path: Path) -> None:
    """exec_target accepts ssh://[user@]host:port without path/query/fragment."""
    (tmp_path / "Workspace.slnx").write_text("", encoding="utf-8")
    s = AssaySettings(root=UPath(tmp_path), exec_target="ssh://user@host:22", exec_known_hosts=None)
    assert s.exec_target == "ssh://user@host:22"


register_law(AssaySettings, "assay_settings_exec_target_rejected")
register_law(AssaySettings, "assay_settings_exec_target_valid_ssh_with_port")

# --- [BOUNDARY_VALIDATOR_LAWS]


def test_exec_target_accepts_root_path_only() -> None:
    """_exec_target accepts only an empty or single-'/' path after the ssh host, rejecting any deeper path.

    Falsifies the path-set literal mutation `"" | "/"` -> `"" | "XX/XX"`: that mutant rejects a trailing-slash
    `ssh://host/` (urlsplit path == '/') which the contract must accept, while `ssh://host/sub` stays rejected so
    the accepted set is pinned to exactly {'', '/'} and never widens to arbitrary paths. The accepted rows also pin
    the round-trip return value, so an accept-as-reject regression cannot pass.
    """
    validity_matrix(
        (
            ("bare", "ssh://host", True),
            ("user_port", "ssh://user@host:22", True),
            ("root_slash", "ssh://host/", True),
            ("user_port_slash", "ssh://user@host:22/", True),
            ("non_root_path", "ssh://host/sub", False),
        ),
        _exec_target_ok,
    )
    assert _settings_mod._exec_target("ssh://host/") == "ssh://host/"


def _exec_target_ok(url: str) -> bool:
    try:
        _settings_mod._exec_target(url)
    except ValueError:
        return False
    return True


register_law(AssaySettings, "exec_target_accepts_root_path_only")


@pytest.mark.parametrize(
    "value, expected",
    [(None, None), ("", None), ("/abs/host_keys", "/abs/host_keys"), ("~", str(UPath("~").expanduser()))],
    ids=["none", "empty", "absolute", "tilde_expanded"],
)
def test_expand_or_none_projection(value: str | None, expected: str | None) -> None:
    """_expand_or_none folds None/'' to None and expands every other path through expanduser().

    Three mutations die here: dropping the `case _:` body (non-empty -> None) is killed by the absolute and tilde
    rows returning a real string; widening the `''` sentinel to a non-empty marker (empty -> expanded '.') is killed
    by the empty row asserting None; replacing the expansion body with `str(None)` (-> 'None') is killed by the
    absolute and tilde rows asserting the actual expanded path, and the tilde row pins that expanduser() truly runs.
    """
    assert _settings_mod._expand_or_none(value) == expected


register_law(AssaySettings, "expand_or_none_projection")


@pytest.mark.parametrize(
    "reported, expected",
    [(0, 8), (1, 1), (8, 8), (257, 256)],
    ids=["zero_falls_to_eight", "one_keeps_floor", "eight_identity", "over_cap_clamps_256"],
)
def test_default_cpu_count_clamp_table(reported: int, expected: int, monkeypatch: pytest.MonkeyPatch) -> None:
    """_default_cpu_count clamps a reported core count into [1, 256], falling back to 8 when the count is falsy.

    Pins the exact `min(256, max(1, count or 8))` arithmetic against four boundary mutations: 257-cap (over_cap row
    yields 256 not 257), floor-2 (one row yields 1 not 2), `or`->`and` (zero/over_cap rows diverge from 8/256), and
    fallback 8->9 (zero row yields 8 not 9). monkeypatch on os.process_cpu_count removes hardware non-determinism.
    """
    monkeypatch.setattr(_settings_mod.os, "process_cpu_count", lambda: reported)
    assert _settings_mod._default_cpu_count() == expected


register_law(AssaySettings, "default_cpu_count_clamp_table")


def test_assay_settings_remote_env_filters_and_injects_run_id(assay_root: AssayHarness) -> None:
    """remote_env injects ASSAY_RUN_ID, retains allowed trace context, strips unsafe keys, and forwards declared row env.

    The ``forward`` set carries deliberately-declared row-Tool env keys across the SSH boundary while the
    ambient allowlist keeps gating undeclared host env — an asymmetry fix for the local-vs-remote env merge.
    """
    source = {"traceparent": "00-a-b-01", "baggage": "k=v", "UNSAFE": "x", "ROW_DECLARED": "row", "ROW_UNDECLARED": "drop"}
    env = assay_root.settings.remote_env(source, forward=frozenset({"ROW_DECLARED"}))
    assert env == IsPartialDict({"ASSAY_RUN_ID": IsStr, "traceparent": "00-a-b-01", "baggage": "k=v", "ROW_DECLARED": "row"})
    assert "UNSAFE" not in env, "ambient non-allowlisted key leaked across SSH"
    assert "ROW_UNDECLARED" not in env, "an undeclared row key must not cross the boundary on the allowlist alone"
    assert "ROW_DECLARED" not in assay_root.settings.remote_env(source), "forward must be opt-in: the bare allowlist still gates row keys"


register_law(AssaySettings, "assay_settings_remote_env_filters_and_injects_run_id")


def test_assay_settings_with_configuration_roundtrip(assay_root: AssayHarness) -> None:
    """with_configuration rebinds via model_copy (no re-validation), switches configuration, and preserves all other fields."""
    debug = assay_root.settings.with_configuration(Configuration.DEBUG)
    release = debug.with_configuration(Configuration.RELEASE)
    assert (debug.configuration, release.configuration) == (Configuration.DEBUG, Configuration.RELEASE)
    assert debug.root == assay_root.settings.root
    assert release.run_id == assay_root.settings.run_id


register_law(AssaySettings, "assay_settings_with_configuration_roundtrip")


def test_assay_settings_artifact_projects_safe_path(assay_root: AssayHarness) -> None:
    """artifact() produces a path rooted under store_root/<kind>/<safe-segment...>."""
    path = str(assay_root.settings.artifact(ArtifactKind.CODE, "search", "run-1", "matches.txt"))
    assert ArtifactKind.CODE.value in path
    assert "matches.txt" in path


register_law(AssaySettings, "assay_settings_artifact_projects_safe_path")


@pytest.mark.parametrize(
    "part", ["..", "a/", "/abs", "a\\b", "\x00null", "a//b"], ids=["dotdot", "trailing_slash", "absolute", "backslash", "null", "double_slash"]
)
def test_assay_settings_artifact_rejects_unsafe_segments(part: str, assay_root: AssayHarness) -> None:
    """artifact() rejects traversal / unsafe path segments before they reach the backend."""
    with pytest.raises(ValueError, match="unsafe artifact path segment"):
        assay_root.settings.artifact(ArtifactKind.CODE, part, "matches.txt")


register_law(AssaySettings, "assay_settings_artifact_rejects_unsafe_segments")


def test_assay_settings_store_honors_protocol_and_forwards_opts(assay_root: AssayHarness) -> None:
    """store() builds the filesystem for the requested protocol and forwards backend opts verbatim.

    Two mutations die: `fsspec.filesystem(None, ...)` (would yield a LocalFileSystem for every protocol) is killed
    by the memory branch asserting a MemoryFileSystem and the absent file path; dropping `**opts` is killed by the
    `auto_mkdir=False` opt surviving onto the constructed LocalFileSystem (a dropped-opts mutant reverts to the
    cached default auto_mkdir=True).
    """
    from fsspec.implementations.local import LocalFileSystem  # noqa: PLC0415  # backend-class identity probe is the only fsspec-impl import here
    from fsspec.implementations.memory import MemoryFileSystem  # noqa: PLC0415  # see above

    mem = assay_root.settings.store(protocol="memory", root=f"store-proto/{assay_root.settings.run_id}")
    assert isinstance(mem.fs, MemoryFileSystem)
    mem.write_bytes(b"x", "probe.bin")
    from pathlib import Path as _Path  # noqa: PLC0415  # on-disk leak probe: a None-protocol mutant would land bytes on the real local FS

    assert not _Path(str(assay_root.settings.store_root / "probe.bin")).exists(), "memory store must not write to the local file backend"

    no_mkdir = assay_root.settings.store(protocol="file", auto_mkdir=False)
    assert isinstance(no_mkdir.fs, LocalFileSystem)
    assert no_mkdir.fs.auto_mkdir is False, "store must forward **opts (auto_mkdir) onto the constructed filesystem"


register_law(AssaySettings, "assay_settings_store_honors_protocol_and_forwards_opts")

# --- [ARTIFACT_SCOPE_LAWS]


@pytest.mark.parametrize("claim", list(Claim))
def test_artifact_scope_open_computes_path_lazily_per_claim(claim: Claim, assay_root: AssayHarness) -> None:
    """ArtifactScope.open computes a claim/run-id path and dotnet_flags WITHOUT materializing the directory.

    Lazy-ensure law: a mutant swapping claim values fails the segment check; a mutant restoring the eager
    store.ensure() materializes the run dir on the real file backend, failing the no-empty-dir assertion (writers
    makedirs(parent) on first write; dotnet creates its own --artifacts-path).
    """
    from pathlib import Path as _Path  # noqa: PLC0415  # local: filesystem-existence probe is the only on-disk check in this module

    scope = ArtifactScope.open(assay_root.settings, claim)
    assert isinstance(scope, ArtifactScope)
    assert claim.value in scope.path
    assert assay_root.settings.run_id in scope.path
    assert {"--artifacts-path", scope.path, "--disable-build-servers"} <= set(scope.dotnet_flags)
    assert not _Path(scope.path).exists(), "open() must not materialize the scope directory (no empty scope dirs)"


register_law(ArtifactScope, "artifact_scope_open_computes_path_lazily_per_claim")


def test_artifact_scope_ensure_materializes_through_store_boundary(assay_root: AssayHarness) -> None:
    """ArtifactScope.ensure() materializes the lazily-opened scope dir through ArtifactStore.ensure_path, returning its own path.

    open() is lazy; a rail that writes its own files into the scope calls ensure() once. ensure() routes the
    makedirs through the store boundary (ensure_path), never the raw fsspec handle, and is idempotent. The
    returned path equals scope.path and the directory now exists on the backend.
    Mutant caught: ensure() reaching scope.store.fs.makedirs directly (boundary bypass) or returning a path other
    than scope.path; ensure_path skipping backend_path validation (a path outside the store root would create dirs
    elsewhere on the backend).
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


register_law(ArtifactScope, "artifact_scope_ensure_materializes_through_store_boundary")
register_law(ArtifactStore, "artifact_scope_ensure_materializes_through_store_boundary")


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
    assert {"--artifacts-path", "--disable-build-servers"} <= set(scope.dotnet_flags)


register_law(ArtifactScope, "artifact_scope_build_configuration_variants")


def test_artifact_scope_dotnet_env_isolation(assay_root: AssayHarness) -> None:
    """ArtifactScope.dotnet_env provides DOTNET_CLI_HOME under the scope path and disables node reuse."""
    scope = ArtifactScope.open(assay_root.settings, Claim.STATIC)
    assert scope.dotnet_env == IsPartialDict({"DOTNET_CLI_HOME": IsStr(regex=rf"{scope.path}.*"), "MSBUILDDISABLENODEREUSE": "1"})


register_law(ArtifactScope, "artifact_scope_dotnet_env_isolation")


def test_artifact_scope_can_leave_build_servers_enabled(assay_root: AssayHarness) -> None:
    """Static build scopes can route artifacts without forcing build-server isolation."""
    scope = ArtifactScope.build(assay_root.settings, "static-closure", disable_build_servers=False)
    assert "--artifacts-path" in scope.dotnet_flags
    assert scope.path in scope.dotnet_flags
    assert "--disable-build-servers" not in scope.dotnet_flags
    assert scope.dotnet_env == IsPartialDict({"DOTNET_CLI_HOME": IsStr(regex=rf"{scope.path}.*")})
    assert "MSBUILDDISABLENODEREUSE" not in scope.dotnet_env


register_law(ArtifactScope, "can_leave_build_servers_enabled")

# --- [PROTOCOL]


def test_artifact_file_system_protocol_runtime_checkable(mem_store: ArtifactStore) -> None:
    """ArtifactFileSystem is @runtime_checkable: fsspec MemoryFileSystem satisfies it structurally."""
    assert isinstance(mem_store.fs, ArtifactFileSystem)


register_law(ArtifactFileSystem, "artifact_file_system_protocol_runtime_checkable")


def test_artifact_file_system_protocol_default_transaction_is_nullcontext() -> None:
    """A concrete filesystem inheriting the Protocol's default `transaction` receives a usable nullcontext.

    The default property body returns contextlib.nullcontext() so non-transactional backends compose with
    the `with self.fs.transaction` pattern in _write_at / write_many. _DefaultTxFs omits the override so the
    Protocol default — not a class-level attribute — is exercised.
    """

    class _DefaultTxFs(_FsStub):
        transaction = (
            ArtifactFileSystem.transaction
        )  # re-bind Protocol default so the property path is exercised, not _FsStub's class-level attribute

    ctx = _DefaultTxFs().transaction
    assert isinstance(ctx, contextlib.AbstractContextManager)
    with ctx:
        pass


register_law(ArtifactFileSystem, "artifact_file_system_protocol_default_transaction_is_nullcontext")

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


register_law(ArtifactStore, "artifact_store_write_bytes_identity")


def test_artifact_store_text_and_path_variants(mem_store: ArtifactStore) -> None:
    """Write_text/read_text round-trip, and write_bytes_path/write_text_path/read_text_path overwrite in place."""
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


register_law(ArtifactStore, "artifact_store_text_and_path_variants")


def test_artifact_store_glob_info_and_write_many(mem_store: ArtifactStore) -> None:
    """Glob discovers written paths, info/info_path expose file metadata, and write_many persists a batch in order."""
    mem_store.write_bytes(b"payload", "history", "run-1", "envelope.json")
    assert any("envelope.json" in m for m in mem_store.glob("history/*/envelope.json"))

    meta = mem_store.write_bytes(b"meta-check", "scope", "meta.txt")
    assert mem_store.info("scope", "meta.txt") == IsPartialDict({"type": "file"})
    assert mem_store.info_path(meta) == IsPartialDict({"type": "file"})

    paths = mem_store.write_many(((b"alpha", ("scope", "a.txt")), (b"beta", ("scope", "b.txt"))))
    assert len(paths) == 2
    assert (mem_store.read_bytes("scope", "a.txt"), mem_store.read_bytes("scope", "b.txt")) == (b"alpha", b"beta")


register_law(ArtifactStore, "artifact_store_glob_info_and_write_many")


def test_artifact_store_path_guards_reject(mem_store: ArtifactStore) -> None:
    """backend_path rejects an out-of-root path; write_bytes(create=True) rejects an existing target."""
    mem_store.write_bytes(b"payload", "history", "run-1", "envelope.json")
    with pytest.raises(ValueError, match="escaped store root"):
        mem_store.read_path("other/history/run-1/envelope.json")
    mem_store.write_bytes(b"first", "scope", "api", "once.txt")
    with pytest.raises(FileExistsError):
        mem_store.write_bytes(b"second", "scope", "api", "once.txt", create=True)


register_law(ArtifactStore, "artifact_store_path_guards_reject")


def test_artifact_store_write_at_atomic_no_partial_on_failure() -> None:
    """_write_at routes through fs.open(autocommit=False); a write that raises leaves no partial file at the target.

    The _StubWriter buffers and commits only on clean __exit__ (LocalFileOpener autocommit=False semantics). A
    write_fails stub raises inside the with-body, so the OSError propagates and exists() stays False — falsified by
    a pipe_file/eager-write that lands bytes before the failure surfaces.
    """
    failing = _FsStub(write_fails=True)
    store = ArtifactStore(fs=failing, root="atomic-root")
    with pytest.raises(OSError, match="injected write failure"):
        store.write_bytes(b"payload", "scope", "partial.bin")
    assert not store.exists("scope", "partial.bin"), "a failed write must not leave a partial file at the target path"


register_law(ArtifactStore, "artifact_store_write_at_atomic_no_partial_on_failure")


def test_artifact_store_write_at_commits_through_open_handle(mem_store: ArtifactStore) -> None:
    """A clean _write_at commits the full payload through the autocommit-deferred open handle (read-back identity).

    Pins that the open(autocommit=False)+write path is the live write route — falsified by a writer that no-ops or
    drops the buffer on commit.
    """
    path = mem_store.write_bytes(b"committed-via-open", "scope", "via-open.bin")
    assert mem_store.read_path(path) == b"committed-via-open"


register_law(ArtifactStore, "artifact_store_write_at_commits_through_open_handle")


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


register_law(ArtifactStore, "artifact_store_exists_size_remove_open_adopt")

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


register_law(ArtifactStore, "artifact_store_walk_detail_dict_keyed_branch")


def test_artifact_store_walk_absent_path_returns_empty_tuple(mem_store: ArtifactStore) -> None:
    """Walk catches FileNotFoundError on an absent path in both the find (recursive) and ls arms, returning ()."""
    assert mem_store.walk("missing-scope-entirely", recursive=True) == ()
    assert mem_store.walk("missing-scope-entirely", recursive=False) == ()


register_law(ArtifactStore, "artifact_store_walk_absent_path_returns_empty_tuple")


@pytest.mark.parametrize(
    "info_payload, reader",
    [({"name": "f", "type": "file", "mtime": "not-a-number"}, "mtime_path"), ({"name": "f", "type": "file", "size": "huge"}, "size_path")],
    ids=["mtime_non_numeric", "size_non_int"],
)
def test_artifact_store_info_path_non_numeric_fallback(info_payload: dict[str, object], reader: str) -> None:
    """mtime_path / size_path fall back to the falsy zero default (0.0 / 0) when backend metadata is non-numeric."""
    store = ArtifactStore(fs=_FsStub(info_payload=info_payload), root="info-root")
    assert not getattr(store, reader)("info-root/scope/f.txt")


register_law(ArtifactStore, "artifact_store_info_path_non_numeric_fallback")

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


register_law(ArtifactStore, "artifact_store_write_history_roundtrip_and_unknown")


@pytest.mark.parametrize("keep, survivors", [(1, 1), (0, 0)], ids=["keep_one", "keep_zero"])
def test_artifact_store_retain_history_prunes_oldest(keep: int, survivors: int, mem_store: ArtifactStore) -> None:
    """retain_history(keep=N) prunes oldest runs leaving exactly N survivors."""
    for i in range(3):
        mem_store.write_history(f"run-{i}", WIRE_ENCODER.encode(make_history_envelope(f"run-{i}")))
    mem_store.retain_history(keep=keep)
    present = [i for i in range(3) if mem_store.load_history(f"run-{i}") is not None]
    assert len(present) == survivors


register_law(ArtifactStore, "artifact_store_retain_history_prunes_oldest")


@pytest.mark.parametrize("keep, expected", [(2, ("scope-1", "scope-2")), (1, ("scope-2",)), (0, ())], ids=["keep_two", "keep_one", "keep_zero"])
def test_artifact_store_retain_scopes_prunes_oldest_per_claim(keep: int, expected: tuple[str, ...]) -> None:
    """retain_scopes(claim, keep) prunes the oldest per-claim run-dirs, keeping the newest keep (lexicographic-stable order).

    Each scope is a `<claim>/<run-id>/file` dir; the _FsStub omits mtime so the (mtime, run_id) rank collapses onto
    the lexicographic run-id tiebreaker, pinning a deterministic prune order. Falsified by pruning newest-first or
    pruning a different claim's root.
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


register_law(ArtifactStore, "artifact_store_retain_scopes_prunes_oldest_per_claim")


def test_artifact_store_sorted_history_ids_mtime_tie_lexicographic_fallback() -> None:
    """sorted_history_ids breaks an mtime tie (or omitted-mtime backend) by ascending lexicographic run id.

    The _FsStub omits mtime, so mtime_from_info folds every row to 0.0 and the (mtime, run_id) sort collapses onto
    the run-id tiebreaker. Falsified by dropping run_id from the sort key (tie order would be backend-arbitrary) —
    this replaces the monkeypatch-only coverage of the lexicographic fallback branch.
    """
    stub = _FsStub()
    store = ArtifactStore(fs=stub, root="tie-root")
    for run in ("run-c", "run-a", "run-b"):
        stub.seed(f"tie-root/{ArtifactKind.HISTORY.value}/{run}/envelope.json", b"{}")
    assert store.sorted_history_ids() == ("run-a", "run-b", "run-c")


register_law(ArtifactStore, "artifact_store_sorted_history_ids_mtime_tie_lexicographic_fallback")


def test_artifact_store_retain_history_already_removed_is_tolerated() -> None:
    """retain_history survives a FileNotFoundError mid-prune (concurrent removal race).

    The _FsStub(rm_raises=True) lets glob/find discover the seeded run but raises FileNotFoundError from
    rm, exercising the except-FileNotFoundError arm in retain_history.
    """
    racey_fs = _FsStub(rm_raises=True)
    store = ArtifactStore(fs=racey_fs, root="racey-root")
    racey_fs.seed("racey-root/history/run-race/envelope.json", WIRE_ENCODER.encode(make_history_envelope("run-race")))
    store.retain_history(keep=0)


register_law(ArtifactStore, "artifact_store_retain_history_already_removed_is_tolerated")


def test_artifact_store_sorted_history_ids_order(mem_store: ArtifactStore, monkeypatch: pytest.MonkeyPatch) -> None:
    """sorted_history_ids returns run ids oldest-first using (mtime, run_id) as the rank key."""
    for run_id in ("run-b", "run-a"):
        mem_store.write_history(run_id, WIRE_ENCODER.encode(make_history_envelope(run_id)))
    monkeypatch.setattr(_settings_mod, "mtime_from_info", lambda info: 1.0 if "run-a" in str(info.get("name", "")) else 2.0)
    ids = mem_store.sorted_history_ids()
    seeded = tuple(rid for rid in ids if rid in {"run-a", "run-b"})
    assert seeded == IsTuple("run-a", "run-b")  # mtime rank (1.0 < 2.0) orders run-a before run-b; both present, in order


register_law(ArtifactStore, "artifact_store_sorted_history_ids_order")


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


register_law(ArtifactStore, "artifact_store_full_report_restore_matrix")

# --- [STATEFUL_HISTORY_RBSM]


class HistoryRetentionStateMachine(RuleBasedStateMachine):
    """State machine for the ArtifactStore write_history / retain_history / load_history protocol.

    Oracles purely on the monotonic zero-padded counter (lexicographic == recency rank) to sidestep
    in-memory fsspec mtime-tie flakiness that the monkeypatch in test_sorted_history_ids_order works around.
    """

    runs: Bundle[str] = Bundle("runs")

    def __init__(self) -> None:
        """Initialise the monotonic write-order oracle; the store root is drawn in @initialize.

        The store identity is no longer minted from a process-global itertools.count(): the disjoint root
        comes from a hypothesis-drawn st.uuids() in @initialize, DataTree-recorded so it replays stably under
        derandomize. __init__ thus carries no process-global state — only the empty oracle sets.
        """
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
        # Postcondition at this rule boundary only; a subsequent write_run legitimately re-grows the store past keep.
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


register_law(ArtifactStore, "history_retention_state_machine")

# --- [ARTIFACT_STORE_RESOLVE_ARTIFACTS_LAWS]


def test_artifact_store_resolve_artifacts_empty_token_returns_empty(mem_store: ArtifactStore) -> None:
    """resolve_artifacts with an empty/whitespace-only token returns () without scanning the backend."""
    mem_store.write_bytes(b"x", "scope", "a.txt")
    assert mem_store.resolve_artifacts("", "scope") == ()
    assert mem_store.resolve_artifacts("  ", "scope") == ()


register_law(ArtifactStore, "artifact_store_resolve_artifacts_empty_token_returns_empty")


def test_artifact_store_resolve_artifacts_basename_substring_and_direct(mem_store: ArtifactStore) -> None:
    """resolve_artifacts matches a basename across roots, a path substring, and a direct existing backend path."""
    mem_store.write_bytes(b"a", "scope", "needle.txt")
    mem_store.write_bytes(b"b", "history", "run-1", "needle.txt")
    basename = mem_store.resolve_artifacts("needle.txt", "scope", "history")
    assert len(basename) == 2
    assert all("needle.txt" in m for m in basename)

    mem_store.write_bytes(b"c", "scope", "mydir", "file.txt")
    assert any("mydir" in m for m in mem_store.resolve_artifacts("mydir", "scope", "history"))

    direct = mem_store.write_bytes(b"direct", "scope", "direct.txt")
    assert mem_store.resolve_artifacts(direct, "scope") == (direct,)


register_law(ArtifactStore, "artifact_store_resolve_artifacts_basename_substring_and_direct")


def test_artifact_store_resolve_artifacts_latest_across_roots(mem_store: ArtifactStore, monkeypatch: pytest.MonkeyPatch) -> None:
    """resolve_artifacts latest=True returns the first non-empty root's newest file (root priority)."""
    mem_store.write_bytes(b"first", "scope", "a.txt")
    mem_store.write_bytes(b"newer", "history", "run-x", "b.txt")
    monkeypatch.setattr(_settings_mod, "mtime_from_info", lambda info: 2.0 if str(info.get("name", "")).endswith("b.txt") else 1.0)
    ranked = mem_store.resolve_artifacts("latest", "scope", "history", latest=True)
    assert len(ranked) >= 1
    assert "a.txt" in ranked[0]

    assert mem_store.resolve_artifacts("latest", "empty-scope", "empty-history", latest=True) == ()


register_law(ArtifactStore, "artifact_store_resolve_artifacts_latest_across_roots")

# --- [ARTIFACT_STORE_OPTION_LAWS]


def test_artifact_store_load_history_option_projection(mem_store: ArtifactStore) -> None:
    """load_history projects to Some(Envelope) for a stored run and None for an absent run (oracle laws)."""
    from expression import Option  # noqa: PLC0415  # oracle-only; top-level import would surface expression as a module-level dependency

    mem_store.write_history("run-opt", WIRE_ENCODER.encode(make_history_envelope("run-opt")))
    assert_some(Option.of_optional(mem_store.load_history("run-opt")))
    assert_none(Option.of_optional(mem_store.load_history("absent-run")))


register_law(ArtifactStore, "artifact_store_load_history_option_projection")


def test_artifact_store_write_bytes_idempotent_overwrite(mem_store: ArtifactStore) -> None:
    """Re-writing the same payload at a backend path is idempotent in read-back (overwrite semantics)."""
    idempotent(b"idem-payload", lambda p: (mem_store.write_bytes(p, "scope", "idem.bin"), mem_store.read_bytes("scope", "idem.bin"))[1])


register_law(ArtifactStore, "artifact_store_write_bytes_idempotent_overwrite")
