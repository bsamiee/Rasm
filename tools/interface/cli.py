"""Applies the interface to each application whose folder holds an `apply.py`, or to the named one.

Each outcome is written to `.artifacts/tools/interface/<app>.json` and printed, and the exit code is 0 when every application applied.
"""

import ast
from collections.abc import Awaitable, Callable
from functools import partial
from importlib import import_module
import os
from pathlib import Path
import subprocess
import sys
import traceback
from typing import Final

import anyio
import cyclopts
from host import Applied, Failed, Host, Outcome
import httpx
from mcp import McpError
import msgspec
import psutil

# --- [CONSTANTS] ------------------------------------------------------------------------

FOLDER: Final = Path(__file__).resolve().parent
ROOT: Final = FOLDER.parents[1]
ARTIFACTS: Final = ROOT / ".artifacts" / FOLDER.relative_to(ROOT)

# --- [MODELS] ---------------------------------------------------------------------------


class Configuration(msgspec.Struct, frozen=True, rename="camel"):
    """The `.mcp.json` rows, each application decoding its own."""

    mcp_servers: msgspec.Raw


# --- [OPERATIONS] -----------------------------------------------------------------------


async def outcome(name: str) -> Outcome:
    """One application's outcome written to its artifact, a process, transport, or decode failure failing it with the error."""
    apply: Callable[[Host], Awaitable[Outcome]] = import_module(f"{name}.apply").apply
    try:
        servers = msgspec.json.decode(await anyio.Path(ROOT, ".mcp.json").read_bytes(), type=Configuration).mcp_servers
        result = await apply(Host(name, FOLDER, ARTIFACTS, servers, os.environ))
    except* (OSError, subprocess.CalledProcessError, psutil.Error, msgspec.MsgspecError, McpError, httpx.HTTPError) as group:
        result = Failed(name, tuple(line.strip() for line in traceback.format_exception_only(group, show_group=True)))
    await anyio.Path(ARTIFACTS, f"{name}.json").write_bytes(msgspec.json.format(msgspec.json.encode(result), indent=1))
    return result


async def run(names: tuple[str, ...]) -> bool:
    """Print the outcome of every named application in order and return whether each applied."""
    outcomes = tuple([await outcome(name) for name in names])
    sys.stdout.buffer.write(msgspec.json.format(msgspec.json.encode(outcomes), indent=1) + b"\n")
    return all(isinstance(each, Applied) for each in outcomes)


# --- [COMPOSITION] ----------------------------------------------------------------------

APPLICATIONS: Final = tuple(sorted(FOLDER.glob("*/apply.py")))
app = cyclopts.App(help=__doc__)
app.default(partial(run, tuple(path.parent.name for path in APPLICATIONS)))
for path in APPLICATIONS:
    app.command(partial(run, (path.parent.name,)), name=path.parent.name, help=ast.get_docstring(ast.parse(path.read_text(encoding="utf-8"))))


def main() -> None:
    """Create the artifacts folder, then run the command line."""
    ARTIFACTS.mkdir(parents=True, exist_ok=True)
    app()


if __name__ == "__main__":
    main()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["app", "main"]
