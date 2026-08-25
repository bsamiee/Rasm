"""Assay health rail: catalog census, launcher/git probes with a token cache, and orphan-process hygiene.

Probe argv derives from catalog rows through ``launch()`` — the catalog stays the only command speller.
The registry's ``self-test`` verb folds this rail's evidence; verb binding stays in the registry.
"""

from pathlib import Path
import sys
import time
from typing import Final, TYPE_CHECKING
lazy import shutil

import msgspec
import psutil
import structlog

from assay.composition.catalog import launch, select, TOOLS
from assay.core.model import ArtifactKind, Check, Claim, Completed, Language, Match, Runner, ToolArgs, wire_encode
from assay.core.routing import Routed, Scope


if TYPE_CHECKING:
    from collections.abc import Mapping

    from expression import Result

    from assay.composition.settings import AssaySettings
    from assay.core.exec import Executor
    from assay.core.model import Fault, Tool


# --- [CONSTANTS] ------------------------------------------------------------------------

ORPHAN_MIN_AGE_S: Final = 900.0
_ORPHAN_PROCESS_TOKENS: Final[frozenset[str]] = frozenset(("python", "python3", "uv", "ty"))
_PROBE_CACHE_FILE: Final = "probe-cache.json"
_PROBE_CACHE_KEY: Final = "probe:%s"
_PROBE_ROUTED: Final[Routed] = Routed(language=Language.PYTHON, scope=Scope.CHANGED)
_PROBE_TTL: Final = 900.0
_UV_RUN_VALUE_OPTIONS: Final[frozenset[str]] = frozenset((
    "--directory",
    "--env-file",
    "--extra",
    "--group",
    "--index",
    "--index-url",
    "--project",
    "--python",
    "--with",
    "--with-editable",
    "--with-requirements",
))
_PROBE_LOCKED: Final[tuple[tuple[tuple[str, ...], str], ...]] = ((Runner.UV.prefix, "uv.lock"), (Runner.PNPM.prefix, "pnpm-lock.yaml"))

# --- [MODELS] ---------------------------------------------------------------------------


class _ProbeRow(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    token: str = ""
    ts: float = 0.0
    note: str = ""
    ok: bool = True


class _ProcessRow(msgspec.Struct, frozen=True, gc=False):
    pid: int
    age_s: float
    command: str


# --- [TABLES] ---------------------------------------------------------------------------

_PROBE_DECODER: Final[msgspec.json.Decoder[dict[str, _ProbeRow]]] = msgspec.json.Decoder(dict[str, _ProbeRow])
_TOOL_PROBE: Final[Tool] = next(t for t in TOOLS if t.name == "tool-probe")
_GIT_HEAD: Final[Check] = Check(tool=next(t for t in TOOLS if t.name == "git-head"))
_GIT_DIRTY: Final[Check] = Check(tool=next(t for t in TOOLS if t.name == "git-dirty"))

# --- [SERVICES] -------------------------------------------------------------------------

_LOG: Final = structlog.get_logger("assay.health")

# --- [OPERATIONS] -----------------------------------------------------------------------


def _probe(argv: tuple[str, ...]) -> Check:
    return Check(tool=_TOOL_PROBE, args=ToolArgs(argv=argv))


def _probe_argv(check: Check) -> tuple[str, ...]:
    return check.args.argv or check.tool.command


def _uv_run_program(argv: tuple[str, ...]) -> str | None:
    if argv[:2] != ("uv", "run"):
        return None
    index = 2
    while index < len(argv) and argv[index].startswith("-"):
        option = argv[index]
        if option == "--":
            index += 1
            break
        index += 2 if option in _UV_RUN_VALUE_OPTIONS else 1
    if index >= len(argv):
        return ""
    return "" if argv[index : index + 2] == ("python", "-m") and index + 2 < len(argv) else argv[index]


def _probe_token(argv: tuple[str, ...]) -> str | None:
    def mtime(path: str | None) -> int | None:
        try:
            return Path(path).stat().st_mtime_ns if path is not None else None
        except OSError:
            return None

    def lock_path(name: str) -> str | None:
        bases = Path(sys.executable).parents
        return next((str(base / name) for base in bases if (base / name).exists()), None)

    if argv[:2] == ("uv", "run"):
        program = _uv_run_program(argv) or ""
        resolved = shutil.which(program) or (sys.executable if not program else None)
        program_mtime, lock_mtime = mtime(resolved), mtime(lock_path("uv.lock"))
        return None if program_mtime is None and lock_mtime is None else f"{resolved}|{program_mtime}|uv.lock:{lock_mtime}"

    matched = next(((prefix, lock) for prefix, lock in _PROBE_LOCKED if argv[: len(prefix)] == prefix), None) if argv else None
    match matched:
        case (prefix, lock):
            program = argv[len(prefix)] if len(argv) > len(prefix) else ""
            resolved = shutil.which(program) or (sys.executable if not program else None)
            program_mtime, lock_mtime = mtime(resolved), mtime(lock_path(lock))
            return None if program_mtime is None and lock_mtime is None else f"{resolved}|{program_mtime}|{lock}:{lock_mtime}"
        case _ if argv:
            path = shutil.which(argv[0])
            return None if (m := mtime(path)) is None else f"{path}|{m}"
        case _:
            return None


def _cache_hit(cache: Mapping[str, _ProbeRow], argv: tuple[str, ...]) -> tuple[str, bool] | None:
    match (cache.get(_PROBE_CACHE_KEY % "\x00".join(argv)), _probe_token(argv)):
        case (_ProbeRow(token=stored, ts=ts, note=note, ok=ok), str() as token) if stored == token and 0 <= time.time() - ts < _PROBE_TTL:
            return note, ok
        case _:
            return None


def _probe_cache_load(settings: AssaySettings) -> dict[str, _ProbeRow]:
    try:
        return _PROBE_DECODER.decode(settings.store().read_bytes("cache", _PROBE_CACHE_FILE))
    except OSError, msgspec.MsgspecError:
        return {}


def _probe_cache_store(
    settings: AssaySettings, prior: Mapping[str, _ProbeRow], fresh: Mapping[tuple[str, ...], tuple[str, bool]], current: frozenset[tuple[str, ...]]
) -> None:
    fresh_rows = {
        _PROBE_CACHE_KEY % "\x00".join(argv): _ProbeRow(token=token, ts=time.time(), note=note, ok=ok)
        for argv, (note, ok) in fresh.items()
        if argv in current
        for token in (_probe_token(argv),)
        if token is not None
    }
    current_keys = frozenset(_PROBE_CACHE_KEY % "\x00".join(argv) for argv in current)
    retained = {key: row for key, row in prior.items() if key in current_keys and key not in fresh_rows}
    try:
        store = settings.store()
        store.write_bytes(wire_encode({**retained, **fresh_rows}), "cache", _PROBE_CACHE_FILE)
    except OSError as exc:
        _LOG.warning("probe_cache.store_failed", run_id=settings.run_id, error=str(exc)[:200])


def _tool_probes() -> tuple[tuple[str, Check], ...]:
    def keyed(tool: Tool) -> tuple[str, tuple[str, ...]]:
        match tool.runner:
            case Runner.DOTNET:
                return "dotnet", ("dotnet", "--version")
            case Runner.UV if uv := tool.uv_groups():
                return f"{tool.runner.value}:{tool.command[0]}:{','.join(uv)}", (*launch(tool), tool.command[0], "--version")
            case _:
                return f"{tool.runner.value}:{tool.command[0]}", (*launch(tool), tool.command[0], "--version")

    deduped = dict(keyed(t) for t in TOOLS if t.runner is not Runner.INPROC and "{" not in t.command[0])
    return tuple((argv[-2], _probe(argv)) for argv in deduped.values())


def _probe_note(label: str, result: Result[Completed, Fault]) -> tuple[str, bool]:
    match result.ok if result.is_ok() else None:
        case None:
            note = f"git: {label.removeprefix('git-')} unavailable" if label in {"git-head", "git-dirty"} else f"tool {label}: MISSING"
            return note, label in {"git-head", "git-dirty"}
        case Completed() as d if label == "git-head":
            return f"git: HEAD {d.stdout.decode(errors='replace').strip()[:40] or 'unknown'}", True
        case Completed() as d if label == "git-dirty":
            return f"git: {'dirty' if d.stdout.strip() else 'clean'}", True
        case Completed(returncode=0) as d:
            lines = d.stdout.decode(errors="replace").strip().splitlines()
            return f"tool {label}: {lines[0][:80] if lines else 'present'}", True
        case Completed() as d:
            lines = (d.stdout or d.stderr).decode(errors="replace").strip().splitlines()
            version = f": {lines[0][:80]}" if lines else ""
            return f"tool {label}: present (exit {d.returncode}){version}", True


def _tooling_process(cmdline: tuple[str, ...]) -> bool:
    return any(Path(part).name in _ORPHAN_PROCESS_TOKENS or Path(part).name.startswith("python") for part in cmdline)


def _orphan_process(proc: psutil.Process, root: Path, *, now: float) -> _ProcessRow | None:
    try:
        info = proc.info
        cmdline = tuple(str(part) for part in (info.get("cmdline") or ()))
        command = " ".join(cmdline) or str(info.get("name") or "")
        in_root = bool(info.get("cwd")) and Path(str(info["cwd"])).resolve().is_relative_to(root)
    except OSError, psutil.Error, TypeError, ValueError:
        return None
    age = max(now - float(info.get("create_time") or now), 0.0)
    match (int(info.get("ppid") or -1) == 1, in_root, age >= ORPHAN_MIN_AGE_S, _tooling_process(cmdline or (command,))):
        case (True, True, True, True):
            return _ProcessRow(pid=int(info.get("pid") or 0), age_s=age, command=command[:160])
        case _:
            return None


def _process_hygiene(settings: AssaySettings) -> tuple[tuple[Match, ...], tuple[str, ...]]:
    root, now = Path(str(settings.root)).resolve(), time.time()
    rows = tuple(
        filter(None, (_orphan_process(proc, root, now=now) for proc in psutil.process_iter(("pid", "ppid", "create_time", "name", "cmdline", "cwd"))))
    )
    if not rows:
        note = "process hygiene: no orphaned repo Python/UV/type-server processes"
        return (Match(id="process-hygiene", kind=ArtifactKind.PROCESS, text=note),), (note,)
    notes = tuple(f"process hygiene: orphan pid={row.pid} age={row.age_s / 60:.0f}m command={row.command}" for row in rows)
    return tuple(
        Match(id=f"process-hygiene-{row.pid}", kind=ArtifactKind.PROCESS, text=note, severity="failed") for row, note in zip(rows, notes, strict=True)
    ), notes


def census() -> bool:
    """Verify every catalog row selects back to itself under its own claim and language.

    Returns:
        True when the catalog is self-consistent; False signals a claim/language mismatch on a row.
    """
    return all(t in select(t.claim, t.language) for t in TOOLS)


def yak_ready() -> bool:
    """Report Rhino packaging readiness.

    Returns:
        True when a catalogued PACKAGE-claim yak row exists and ``yak`` resolves on PATH.
    """
    return any(t.name == "yak" and t.claim is Claim.PACKAGE for t in TOOLS) and shutil.which("yak") is not None


def probes(settings: AssaySettings, executor: Executor) -> tuple[tuple[Match, ...], tuple[str, ...]]:
    """Run tool/git health probes and orphan hygiene, warming from the token cache.

    Missing tools report as notes; tokenized launcher probes cache under path+mtime tokens, while git probes stay live.

    Returns:
        Health Match rows and their note projections, probes first, hygiene last.
    """
    pairs = (*_tool_probes(), ("git-head", _GIT_HEAD), ("git-dirty", _GIT_DIRTY))
    volatile = frozenset((_probe_argv(_GIT_HEAD), _probe_argv(_GIT_DIRTY)))
    cache = _probe_cache_load(settings)
    hits = {
        argv: hit for _, c in pairs for argv in (_probe_argv(c),) if argv not in volatile for hit in (_cache_hit(cache, argv),) if hit is not None
    }
    misses = tuple((label, c) for label, c in pairs if hits.get(_probe_argv(c)) is None)
    results = executor.fan(tuple(c for _, c in misses), settings=settings, scope=None, routed=_PROBE_ROUTED)
    fresh = {_probe_argv(c): _probe_note(label, r) for (label, c), r in zip(misses, results, strict=True)}
    current = frozenset(_probe_argv(c) for _, c in pairs if _probe_argv(c) not in volatile)
    _probe_cache_store(settings, cache, fresh, current)
    noted = tuple(hits.get(_probe_argv(c)) or fresh[_probe_argv(c)] for _, c in pairs)
    hygiene = _process_hygiene(settings)
    return (
        (
            *tuple(
                Match(id=label, kind=ArtifactKind.PROCESS, text=note, severity="failed" if not ok else None)
                for (label, _), (note, ok) in zip(pairs, noted, strict=True)
            ),
            *hygiene[0],
        ),
        (*tuple(note for note, _ in noted), *hygiene[1]),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ORPHAN_MIN_AGE_S", "census", "probes", "yak_ready"]
