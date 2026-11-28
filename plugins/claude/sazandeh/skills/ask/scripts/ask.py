#!/usr/bin/env -S uv run --script
# /// script
# requires-python = ">=3.15"
# dependencies = ["pydantic", "pydantic-settings", "ruamel.yaml", "msgspec", "cyclopts", "expression", "markdown-it-py", "mdit-py-plugins", "mdformat", "mdformat-gfm", "pygments", "regex", "wcwidth", "unidiff", "psutil"]
#
# [tool.ty.environment]
# python-platform = "darwin"
#
# [tool.uv]
# prerelease = "allow"
# ///
"""Builds the AskUserQuestion tool input for a spec of what the owner is asked."""

from itertools import chain
import sys
from typing import Annotated

from cyclopts import App, Parameter
from cyclopts.types import StdioPath
from expression import Error, Ok
from form import built, Host, Layout, loaded
import msgspec

# --- [COMPOSITION] ----------------------------------------------------------------------


def build(spec: Annotated[StdioPath, Parameter(allow_leading_hyphen=True)], /, *, plain: bool = False) -> int:
    """Print one AskUserQuestion input per line in calling order, and report lines on stderr.

    Args:
        spec: YAML or JSON spec, `-` for stdin.
        plain: Drop previews, for a re-ask after a preview dialog stopped taking keys.

    Returns:
        Exit status: 0 built, 65 refused spec, 66 unreadable spec or file, 69 failed git.
    """
    host = Host()
    layout = Layout.PLAIN if plain else host.layout
    match loaded(spec).bind(lambda asks: built(asks, host.box(layout))):
        case Ok((calls, notes)):
            sys.stdout.buffer.write(b"".join(msgspec.json.encode(call) + b"\n" for call, _ in calls))
            dropped = () if layout.previews else (f"previews: none, {'flag --plain' if plain else f'host {host.CLAUDE_CODE_ENTRYPOINT}'}",)
            rows = (*chain.from_iterable(rows for _, rows in calls), *notes, *dropped)
            if len(calls) != 1 or rows:
                sys.stderr.write("".join((f"ask: {len(calls)} call{'s' * (len(calls) != 1)}\n", *(f"  {row}\n" for row in rows))))
            return 0
        case Error(faults):
            ordered = sorted(dict.fromkeys(faults), key=lambda fault: fault.at[:1])
            sys.stderr.write("".join((f"ask: refused, {len(ordered)} fault{'s' * (len(ordered) > 1)}\n", *(fault.row for fault in ordered))))
            return ordered[0].reason.status


if __name__ == "__main__":
    app = App(help=__doc__, result_action="sys_exit", default_parameter=Parameter(negative=()), version_flags=())
    app.default(build)
    app()
