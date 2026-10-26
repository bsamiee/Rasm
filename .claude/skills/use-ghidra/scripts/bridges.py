"""SessionStart and SessionEnd hook stopping ghidra-cli bridges of ended sessions and clearing files dead bridges left on unheld projects."""

from collections.abc import Mapping
import os
from pathlib import Path
import subprocess
import sys

import msgspec
import psutil

# --- [TYPES] ----------------------------------------------------------------------------

type Session = SessionStart | SessionEnd

# --- [MODELS] ---------------------------------------------------------------------------


class Event(msgspec.Struct, frozen=True, tag_field="hook_event_name", tag=True):
    """Session event tagged by class name."""


class SessionStart(Event, frozen=True):
    """Claude Code session start."""


class SessionEnd(Event, frozen=True):
    """Claude Code session end."""

    session_id: str


class Settings(msgspec.Struct, frozen=True, rename={"projects": "GHIDRA_PROJECT_DIR"}):
    """Folder `mise.toml` names for every Ghidra project."""

    projects: str


class Owner(msgspec.Struct, frozen=True, rename={"pid": "CLAUDE_PID", "session_id": "CLAUDE_CODE_SESSION_ID"}):
    """Claude Code process and session bridges inherit through environment."""

    pid: int
    session_id: str


class Bridge(msgspec.Struct, frozen=True):
    """Bridge JVM with project, owner, owner liveness, and ghidra-cli launch environment."""

    project: Path
    owner: Owner
    orphaned: bool
    environ: frozendict[str, str]

    def ended(self, event: Session) -> bool:
        """Return whether owner's process ended or `event` ends owner's session."""
        match event:
            case SessionEnd(session_id=session_id):
                return self.orphaned or self.owner.session_id == session_id
            case SessionStart():
                return self.orphaned


# --- [OPERATIONS] -----------------------------------------------------------------------


def bridges() -> tuple[Bridge, ...]:
    """Return each bridge JVM naming its owner, orphaned when no older process holds owner PID, project from `AnalyzeHeadless` arguments."""
    headless = "ghidra.app.util.headless.AnalyzeHeadless"
    processes = tuple(psutil.process_iter(("cmdline", "environ", "create_time")))
    started = {process.pid: process.info["create_time"] for process in processes}
    return tuple(
        Bridge(
            Path(argv[(at := argv.index(headless)) + 1], argv[at + 2]),
            owner := msgspec.convert(environ, Owner, strict=False),
            owner.pid not in started or started[owner.pid] > process.info["create_time"],
            frozendict(environ),
        )
        for process in processes
        if {headless, "GhidraCliBridge.java"} <= set(argv := process.info["cmdline"] or [])
        and {field.encode_name for field in msgspec.structs.fields(Owner)} <= (environ := process.info["environ"] or {}).keys()
    )


def commands(event: Session, found: tuple[Bridge, ...], projects: tuple[Path, ...], environ: Mapping[str, str]) -> tuple[tuple[tuple[str, ...], Mapping[str, str]], ...]:
    """Return `ghidra stop` per ended bridge under its environment, then `ghidra status` per unheld project under hook environment."""
    held = {bridge.project for bridge in found}
    return (
        *((("ghidra", "--quiet", "stop", "--project", str(bridge.project)), bridge.environ) for bridge in found if bridge.ended(event)),
        *((("ghidra", "--quiet", "status", "--project", str(project)), environ) for project in projects if project not in held),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


def main() -> None:
    """Start each command in its own session, a bridge's save at `ghidra stop` outlasts the hook."""
    event = msgspec.json.decode(sys.stdin.buffer.read(), type=Session)
    projects = tuple(path.with_suffix("") for path in Path(msgspec.convert(os.environ, Settings).projects).glob("*.gpr"))
    for command, environ in commands(event, bridges(), projects, os.environ):
        subprocess.Popen(command, env=environ, stdin=subprocess.DEVNULL, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, start_new_session=True)


if __name__ == "__main__":
    main()
