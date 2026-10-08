# /// script
# dependencies = ["msgspec"]
#
# [tool.ty.rules]
# all = "error"
# dynamic-function-decorator-return = "ignore"
# unsound-assignment = "ignore"
# unsound-return-statement = "ignore"
# ///
"""PreToolUse hook that runs each `run_python` script through `document.run`."""

from pathlib import Path
import sys

import msgspec

# --- [MODELS] ---------------------------------------------------------------------------


class Input(msgspec.Struct, frozen=True, omit_defaults=True):
    """`run_python` arguments."""

    script: str
    slot: str | None = None


class Event(msgspec.Struct, frozen=True):
    """PreToolUse event of a `rhino-mcp-platform` call."""

    tool_name: str
    tool_input: msgspec.Raw


# --- [OPERATIONS] -----------------------------------------------------------------------


def wrap(source: str) -> str:
    """Return the script as a `document.run` call in an undo step its first comment line names, under the skill folder's `# env:` line and the comment lines of its first 30 lines, with skill modules imported fresh."""
    comments = [line for line in source.splitlines()[:30] if line.lstrip().startswith("#")]
    label = next(filter(None, (line.strip().lstrip("# ") for line in comments)), "run_python")
    folder = Path(__file__).resolve().parent
    return "\n".join((
        f"# env: {folder}",
        *comments,
        "import sys",
        f"sys.pycache_prefix = {sys.pycache_prefix!r}",
        f"for name in {tuple(path.stem for path in folder.glob('*.py'))!r}: sys.modules.pop(name, None)",
        "import document",
        f"document.run({source!r}, {f'MCP: {label}'!r}, __rhino_doc__, globals())",
        "",
    ))


def decision(event: Event) -> dict[str, object] | None:
    """Return the wrapped `run_python` input, or `None` for a call that runs as sent."""
    match event.tool_name.rpartition("__")[2]:
        case "run_python":
            tool_input = msgspec.json.decode(event.tool_input, type=Input)
            return {"permissionDecision": "allow", "updatedInput": msgspec.structs.replace(tool_input, script=wrap(tool_input.script))}
        case _:
            return None


# --- [COMPOSITION] ----------------------------------------------------------------------


def main() -> None:
    """Print the hook's output for the stdin event, nothing for a call that runs as sent."""
    if (output := decision(msgspec.json.decode(sys.stdin.buffer.read(), type=Event))) is not None:
        sys.stdout.buffer.write(msgspec.json.encode({"hookSpecificOutput": {"hookEventName": "PreToolUse", **output}}))


if __name__ == "__main__":
    main()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Event", "Input", "decision", "main", "wrap"]
