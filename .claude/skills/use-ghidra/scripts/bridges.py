"""SessionStart and SessionEnd hook that stops each ghidra-cli bridge of an ended Claude Code session and runs `ghidra status` to clear pid, port, and lock files dead bridges left on each project no bridge holds."""

from collections.abc import Mapping
import math
import os
from pathlib import Path
import subprocess
import sys

import msgspec
import psutil

# --- [TYPES] ----------------------------------------------------------------------------

type Session = SessionStart | SessionEnd

# --- [CONSTANTS] ------------------------------------------------------------------------

HEADLESS = "ghidra.app.util.headless.AnalyzeHeadless"
BRIDGE = "GhidraCliBridge.java"

# --- [MODELS] ---------------------------------------------------------------------------


class Event(msgspec.Struct, frozen=True, tag_field="hook_event_name"):
    """Session event, tagged by event name."""


class SessionStart(Event, frozen=True, tag="SessionStart"):
    """Start of a Claude Code session."""


class SessionEnd(Event, frozen=True, tag="SessionEnd"):
    """End of the Claude Code session it names."""

    session_id: str


class Settings(msgspec.Struct, frozen=True, rename={"projects": "GHIDRA_PROJECT_DIR"}):
    """Folder `mise.toml` names for every Ghidra project."""

    projects: str


class Owner(msgspec.Struct, frozen=True, rename={"pid": "CLAUDE_PID", "session": "CLAUDE_CODE_SESSION_ID"}):
    """Claude Code process and session a bridge inherits in its environment."""

    pid: int
    session: str


class Bridge(msgspec.Struct, frozen=True):
    """Bridge JVM with project, owner, whether owner's process ended, and environment ghidra-cli started it under."""

    project: Path
    owner: Owner
    orphaned: bool
    environ: dict[str, str]

    def ended(self, event: Session) -> bool:
        """Return whether owner's process ended or `event` ends owner's session."""
        match event:
            case SessionEnd(session_id=session):
                return self.orphaned or self.owner.session == session
            case SessionStart():
                return self.orphaned


# --- [OPERATIONS] -----------------------------------------------------------------------


def bridges() -> tuple[Bridge, ...]:
    """Return each bridge JVM naming its owner, orphaned when no older process holds owner's pid, project from `AnalyzeHeadless` location and name arguments."""
    processes = tuple(psutil.process_iter(("cmdline", "environ", "create_time")))
    started = {process.pid: process.info["create_time"] for process in processes}
    return tuple(
        Bridge(Path(argv[at + 1], argv[at + 2]), owner, started.get(owner.pid, math.inf) > process.info["create_time"], environ)
        for process in processes
        if {HEADLESS, BRIDGE} <= set(argv := process.info["cmdline"] or []) and set(Owner.__struct_encode_fields__) <= (environ := process.info["environ"] or {}).keys()
        for owner in (msgspec.convert(environ, Owner, strict=False),)
        for at in (argv.index(HEADLESS),)
    )


def commands(event: Session, found: tuple[Bridge, ...], projects: tuple[Path, ...], environ: Mapping[str, str]) -> tuple[tuple[tuple[str, ...], Mapping[str, str]], ...]:
    """Return `ghidra stop` under each ended bridge's environment, then `ghidra status` under hook environment on each project no bridge holds."""
    held = {bridge.project for bridge in found}
    return (
        *((("ghidra", "--quiet", "stop", "--project", str(bridge.project)), bridge.environ) for bridge in found if bridge.ended(event)),
        *((("ghidra", "--quiet", "status", "--project", str(project)), environ) for project in projects if project not in held),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


def main() -> None:
    """Start each command in its own session, a bridge's save at `ghidra stop` outlasts the hook."""
    event = msgspec.json.decode(sys.stdin.buffer.read(), type=Session)
    settings = msgspec.convert(dict(os.environ), Settings)
    projects = tuple(path.with_suffix("") for path in Path(settings.projects).glob("*.gpr"))
    for command, environ in commands(event, bridges(), projects, os.environ):
        subprocess.Popen(command, env=environ, stdin=subprocess.DEVNULL, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, start_new_session=True)


if __name__ == "__main__":
    main()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BRIDGE", "HEADLESS", "Bridge", "Event", "Owner", "Session", "SessionEnd", "SessionStart", "Settings", "bridges", "commands", "main"]
