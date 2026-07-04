"""Health rail laws: catalog-derived probes, token cache, orphan hygiene, census, and yak readiness."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import Callable  # noqa: TC003  # Callable annotation in parametrize parameter evaluated at collection time
from dataclasses import dataclass
from pathlib import Path
import time
from typing import TYPE_CHECKING

from expression import Error, Ok, Result  # noqa: TC002  # Result appears in parametrize annotations evaluated at collection time
import msgspec.structs
import pytest

from tests.python.tools.assay.kit import SeamExecutor
from tools.assay.composition.catalog import launch, TOOLS
from tools.assay.core.model import Completed, Fault, RailStatus, Runner
from tools.assay.rails import health as health_mod
from tools.assay.rails.health import census, ORPHAN_MIN_AGE_S, probes, yak_ready


if TYPE_CHECKING:
    from tests.python.tools.assay.kit import AssayHarness
    from tools.assay.core.model import Check


# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (census, probes, yak_ready)

# --- [HARNESS] --------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class _Proc:
    """Minimal psutil process double with a pre-populated ``info`` dict."""

    info: dict[str, object]


def _done(*, rc: int = 0, status: RailStatus = RailStatus.OK, stdout: bytes = b"") -> Completed:
    return msgspec.structs.replace(Completed(("probe",), rc, status=status), stdout=stdout)


def _completed(check: Check, *, rc: int = 0, status: RailStatus = RailStatus.OK, stdout: bytes = b"") -> Completed:
    return msgspec.structs.replace(Completed(health_mod._probe_argv(check), rc, status=status), stdout=stdout)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [ORPHAN_HYGIENE]


def test_orphan_min_age_s_threshold_boundary(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """The orphan age comparison is monotone across the ORPHAN_MIN_AGE_S boundary."""
    root = str(assay_root.root)
    base_info: dict[str, object] = {"ppid": 1, "create_time": 0.0, "name": "python3", "cmdline": ("python3", "-"), "cwd": root}
    monkeypatch.setattr(
        health_mod.__dict__["psutil"], "process_iter", lambda _attrs: (_Proc({**base_info, "pid": 1}), _Proc({**base_info, "pid": 2}))
    )

    def orphan_ids(now: float) -> frozenset[str]:
        monkeypatch.setattr(health_mod.__dict__["time"], "time", lambda: now)
        matches, _ = health_mod._process_hygiene(assay_root.settings)
        return frozenset(m.id for m in matches if m.severity == "failed")

    assert orphan_ids(ORPHAN_MIN_AGE_S + 1.0) >= orphan_ids(ORPHAN_MIN_AGE_S), "more orphans expected past threshold"


def test_process_hygiene_reports_old_repo_orphans(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Hygiene reports only old root-cwd tooling orphans, with pid/age/command pinned in the match text."""
    root = str(assay_root.root)
    rows = (
        _Proc({"pid": 123, "ppid": 1, "create_time": 100.0, "name": "python3", "cmdline": ("python3", "-"), "cwd": root}),
        _Proc({"pid": 456, "ppid": 99, "create_time": 100.0, "name": "python3", "cmdline": ("python3", "-"), "cwd": root}),
        _Proc({"pid": 789, "ppid": 1, "create_time": 100.0, "name": "python3", "cmdline": ("python3", "-"), "cwd": str(assay_root.root.parent)}),
        _Proc({"pid": 999, "ppid": 0, "create_time": 100.0, "name": "python3", "cmdline": ("python3", "-"), "cwd": root}),
    )
    monkeypatch.setattr(health_mod.__dict__["psutil"], "process_iter", lambda _attrs: rows)
    monkeypatch.setattr(health_mod.__dict__["time"], "time", lambda: 100.0 + ORPHAN_MIN_AGE_S + 1.0)
    matches, notes = health_mod._process_hygiene(assay_root.settings)

    failed = [m for m in matches if m.severity == "failed"]
    assert len(failed) == 1, f"expected exactly 1 orphan match, got {len(failed)}"
    assert "pid=123" in failed[0].text
    assert "age=15m" in failed[0].text
    assert "command=python3 -" in failed[0].text
    assert all("pid=456" not in n and "pid=789" not in n and "pid=999" not in n for n in notes)


def test_orphan_process_exception_returns_none() -> None:
    """_orphan_process returns None when proc.info raises OSError — a process vanishing mid-scan never crashes hygiene."""
    from unittest.mock import create_autospec, PropertyMock  # noqa: PLC0415

    import psutil as _ps  # noqa: PLC0415

    broken: _ps.Process = create_autospec(_ps.Process, instance=True)
    type(broken).info = PropertyMock(side_effect=OSError("process gone"))
    assert health_mod._orphan_process(broken, Path("/private/tmp"), now=10000.0) is None


# --- [PROBES]


def test_health_probe_cache_warm_path(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """probes() warms from cache while re-probing only volatile or token-unresolvable tools, all riding catalog rows."""
    calls: list[tuple[tuple[Check, ...], object]] = []

    def spy(checks: tuple[Check, ...], **kwargs: object) -> tuple[Result[Completed, Fault], ...]:
        calls.append((checks, kwargs["routed"]))
        return tuple(Ok(_completed(c)) for c in checks)

    probes(assay_root.settings, SeamExecutor(fan_fn=spy))
    probes(assay_root.settings, SeamExecutor(fan_fn=spy))
    (cold, cold_routed), (warm, warm_routed) = calls
    assert cold_routed is health_mod._PROBE_ROUTED
    assert warm_routed is health_mod._PROBE_ROUTED
    assert all(c.tool.name in {"tool-probe", "git-head", "git-dirty"} for c in cold), "a probe bypassed the catalog rows"
    cold_cmds = {health_mod._probe_argv(c) for c in cold}
    warm_cmds = {health_mod._probe_argv(c) for c in warm}
    assert ("dotnet", "--version") in cold_cmds
    volatile = {health_mod._probe_argv(health_mod._GIT_HEAD), health_mod._probe_argv(health_mod._GIT_DIRTY)}
    assert volatile <= cold_cmds
    assert volatile <= warm_cmds
    cacheable = {cmd for cmd in cold_cmds - volatile if health_mod._probe_token(cmd) is not None}
    assert cacheable, "expected at least one token-resolvable catalogued probe"
    assert warm_cmds.isdisjoint(cacheable)


def test_tool_probes_derive_from_launch() -> None:
    """Every launcher probe argv is launch(row)-prefixed and DOTNET rows collapse to one SDK probe — no second argv speller."""
    by_argv = {health_mod._probe_argv(check): label for label, check in health_mod._tool_probes()}
    probed_rows = tuple(t for t in TOOLS if t.runner not in {Runner.INPROC, Runner.DOTNET} and "{" not in t.command[0])
    for tool in probed_rows:
        assert (*launch(tool), tool.command[0], "--version") in by_argv, f"probe argv for {tool.name} not launch()-derived"
    assert ("dotnet", "--version") in by_argv, "DOTNET rows must collapse to one SDK probe"


def test_probe_cache_round_trip_and_retention(assay_root: AssayHarness) -> None:
    """Probe cache rows round-trip and retention keeps only catalogued keys."""
    argv = health_mod._probe_argv(health_mod._GIT_HEAD)
    health_mod._probe_cache_store(assay_root.settings, {}, {argv: ("note-x", False)}, frozenset((argv,)))
    loaded = health_mod._probe_cache_load(assay_root.settings)
    assert health_mod._cache_hit(loaded, argv) == ("note-x", False)

    key = health_mod._PROBE_CACHE_KEY % "\x00".join(argv)
    prior = {
        key: loaded[key],
        "probe:ghost": health_mod._ProbeRow(token="t", ts=time.time(), note="gone", ok=True),  # noqa: S106  # mtime token, not a secret
    }
    health_mod._probe_cache_store(assay_root.settings, prior, {}, frozenset((argv,)))
    assert set(health_mod._probe_cache_load(assay_root.settings)) == {key}


@pytest.mark.parametrize(
    "label, outcome, want_substrings, want_ok",
    [
        ("git-head", lambda: Ok(_done(stdout=b"abc1234\n")), ("HEAD", "abc1234"), True),
        ("git-dirty", lambda: Ok(_done(stdout=b" M file.py\n")), ("dirty",), True),
        ("git-dirty", lambda: Ok(_done(stdout=b"")), ("clean",), True),
        ("ruff", lambda: Ok(_done(rc=2, status=RailStatus.FAILED, stdout=b"ruff 0.8.0\n")), ("present (exit 2)", "ruff 0.8.0"), True),
        ("git-head", lambda: Error(Fault((), RailStatus.FAULTED, "err")), (), True),
        ("ruff", lambda: Error(Fault((), RailStatus.FAULTED, "err")), ("MISSING",), False),
    ],
    ids=["git-head-ok", "git-dirty", "git-clean", "tool-nonzero-exit", "git-missing-ok", "tool-missing-false"],
)
def test_probe_note_projects_result_to_note(
    label: str,
    outcome: Callable[[], Result[Completed, Fault]],
    want_substrings: tuple[str, ...],
    want_ok: bool,  # noqa: FBT001  # parametrize matrix field, not a call-site positional bool
) -> None:
    """_probe_note projects tool and git probe outcomes to note/health pairs keyed on the probe label."""
    note, ok = health_mod._probe_note(label, outcome())
    assert all(s in note for s in want_substrings)
    assert ok is want_ok


@pytest.mark.parametrize("fresh", [True, False], ids=["fresh", "stale"])
def test_cache_hit_respects_ttl(fresh: bool) -> None:  # noqa: FBT001  # parametrize-injected, not a call-site positional bool
    """_cache_hit returns fresh token hits and rejects stale rows."""
    argv = health_mod._probe_argv(health_mod._GIT_HEAD)
    token = health_mod._probe_token(argv)
    assert token is not None  # real git installed
    ts = time.time() if fresh else time.time() - health_mod._PROBE_TTL - 1
    cache = {health_mod._PROBE_CACHE_KEY % "\x00".join(argv): health_mod._ProbeRow(token=token, ts=ts, note="git: HEAD abc1234", ok=True)}
    assert health_mod._cache_hit(cache, argv) == (("git: HEAD abc1234", True) if fresh else None)


@pytest.mark.parametrize("argv", [(), ("__no_such_program_xyz__",)], ids=["empty-argv", "unresolvable"])
def test_probe_token_returns_none(argv: tuple[str, ...]) -> None:
    """_probe_token returns None for empty argv and unresolvable programs — never a stale cache key."""
    assert health_mod._probe_token(argv) is None


def test_probe_token_uv_locked_module_uses_module_boundary(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    """UV module probes cache against the Python executable plus uv.lock, never --locked."""
    python = tmp_path / ".venv" / "bin" / "python"
    python.parent.mkdir(parents=True)
    python.write_text("#!/bin/sh\n", encoding="utf-8")
    (tmp_path / "uv.lock").write_text("", encoding="utf-8")
    monkeypatch.setattr(health_mod.sys, "executable", str(python))

    token = health_mod._probe_token(("uv", "run", "--locked", "python", "-m", "tools.assay", "--version"))

    assert token is not None
    assert str(python) in token
    assert "--locked" not in token


def test_probe_token_uv_locked_group_uses_group_program(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    """UV group probes skip runner flags before resolving the probed executable."""
    mutmut = tmp_path / "mutmut"
    mutmut.write_text("#!/bin/sh\n", encoding="utf-8")
    monkeypatch.setattr("shutil.which", lambda program: str(mutmut) if program == "mutmut" else None)

    token = health_mod._probe_token(("uv", "run", "--locked", "--group", "mutation", "mutmut", "--version"))

    assert token is not None
    assert token.startswith(f"{mutmut}|")
    assert "--group" not in token


@pytest.mark.parametrize("seed", [None, b"{bad json"], ids=["missing", "corrupt"])
def test_probe_cache_load_folds_to_empty(assay_root: AssayHarness, seed: bytes | None) -> None:
    """_probe_cache_load returns {} for an absent cache file or corrupt bytes — both are clean misses."""
    match seed:
        case bytes() as raw:
            assay_root.settings.store().write_bytes(raw, "cache", "probe-cache.json")
        case None:
            pass
    assert health_mod._probe_cache_load(assay_root.settings) == {}


def test_probe_cache_store_oserror_is_swallowed(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_probe_cache_store swallows OSError from write_bytes — a failing write never crashes self_test."""

    def _bad_io(*_a: object, **_k: object) -> None:
        raise OSError("disk full")

    monkeypatch.setattr(type(assay_root.settings.store()), "write_bytes", _bad_io)
    health_mod._probe_cache_store(assay_root.settings, {}, {}, frozenset())


# --- [CENSUS_AND_YAK]


def test_census_true_for_catalog_and_false_when_selection_drops_rows(monkeypatch: pytest.MonkeyPatch) -> None:
    """census() is True over the real catalog and False when any row fails to select back."""
    assert census() is True
    monkeypatch.setattr(health_mod, "select", lambda *_a, **_k: ())
    assert census() is False


@pytest.mark.parametrize("resolved, ready", [("/opt/yak/yak", True), (None, False)], ids=["on-path", "missing"])
def test_yak_ready_tracks_path_resolution(resolved: str | None, ready: bool, monkeypatch: pytest.MonkeyPatch) -> None:  # noqa: FBT001  # parametrize matrix field
    """yak_ready() is True exactly when the catalogued PACKAGE yak row resolves on PATH."""
    import shutil  # noqa: PLC0415

    monkeypatch.setattr(shutil, "which", lambda name: resolved if name == "yak" else None)
    assert yak_ready() is ready
