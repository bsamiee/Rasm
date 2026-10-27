"""SessionStart and SessionEnd hook stopping ghidra-cli bridges of ended sessions and clearing files dead bridges left on unheld projects."""

lazy from collections.abc import Mapping
lazy import os
lazy from pathlib import Path
lazy import subprocess
lazy import sys

import msgspec
lazy from expression.extra.result import catch
lazy import psutil

# --- [TYPES] ----------------------------------------------------------------------------

type _Session = _SessionStart | _SessionEnd

# --- [MODELS] ---------------------------------------------------------------------------


class _Event(msgspec.Struct, frozen=True, tag_field="hook_event_name", tag=lambda name: name.removeprefix("_")):
    """Session event tagged by class name without its underscore."""


class _SessionStart(_Event, frozen=True):
    """Claude Code session start."""


class _SessionEnd(_Event, frozen=True):
    """Claude Code session end."""

    session_id: str


class _Settings(msgspec.Struct, frozen=True, rename={"projects": "GHIDRA_PROJECT_DIR"}):
    """Folder `mise.toml` names for every Ghidra project."""

    projects: str


class _Owner(msgspec.Struct, frozen=True, rename={"pid": "CLAUDE_PID", "session_id": "CLAUDE_CODE_SESSION_ID"}):
    """Claude Code process and session bridges inherit through environment."""

    pid: int
    session_id: str


class _Bridge(msgspec.Struct, frozen=True):
    """Bridge JVM with project, owner, owner liveness, and ghidra-cli launch environment."""

    project: Path
    owner: _Owner
    orphaned: bool
    environ: frozendict[str, str]

    def ended(self, event: _Session) -> bool:
        """Return whether owner's process ended or `event` ends owner's session."""
        match event:
            case _SessionEnd(session_id=session_id):
                return self.orphaned or self.owner.session_id == session_id
            case _SessionStart():
                return self.orphaned


# --- [OPERATIONS] -----------------------------------------------------------------------


def _bridges() -> tuple[_Bridge, ...]:
    """Return each bridge JVM naming its owner, orphaned when no older process holds owner PID, project from `AnalyzeHeadless` arguments."""
    headless = "ghidra.app.util.headless.AnalyzeHeadless"
    processes = tuple(psutil.process_iter(("cmdline", "environ", "create_time")))
    started = {process.pid: process.info["create_time"] for process in processes}
    return tuple(
        _Bridge(Path(argv[(at := argv.index(headless)) + 1], argv[at + 2]), owner, owner.pid not in started or started[owner.pid] > process.info["create_time"], frozendict(process.info["environ"]))
        for process in processes
        if {headless, "GhidraCliBridge.java"} <= set(argv := process.info["cmdline"] or [])
        for owner in catch(msgspec.convert, exception=msgspec.ValidationError)(process.info["environ"] or {}, _Owner, strict=False).to_option().to_list()
    )


def _commands(event: _Session, found: tuple[_Bridge, ...], projects: tuple[Path, ...], environ: Mapping[str, str]) -> tuple[tuple[tuple[str, ...], Mapping[str, str]], ...]:
    """Return `ghidra stop` per ended bridge under its environment, then `ghidra status` per unheld project under hook environment."""
    held = {bridge.project for bridge in found}
    return (
        *((("ghidra", "--quiet", "stop", "--project", str(bridge.project)), bridge.environ) for bridge in found if bridge.ended(event)),
        *((("ghidra", "--quiet", "status", "--project", str(project)), environ) for project in projects if project not in held),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


def main() -> None:
    """Start each command in its own session, a bridge's save at `ghidra stop` outlasts the hook."""
    event = msgspec.json.decode(sys.stdin.buffer.read(), type=_Session)
    projects = tuple(path.with_suffix("") for path in Path(msgspec.convert(os.environ, _Settings).projects).glob("*.gpr"))
    for command, environ in _commands(event, _bridges(), projects, os.environ):
        subprocess.Popen(command, env=environ, stdin=subprocess.DEVNULL, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, start_new_session=True)


if __name__ == "__main__":
    main()
