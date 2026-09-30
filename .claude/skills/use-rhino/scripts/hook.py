# /// script
# dependencies = ["msgspec"]
# ///
"""PreToolUse hook that runs each `run_python` script through `document.run` and refuses the router tools a script entry point replaces."""

from pathlib import Path
import sys
from typing import ClassVar

import msgspec

# --- [TYPES] ----------------------------------------------------------------------------

type Call = RunPython | RunCommand | CloseDoc | SaveDoc | OpenDoc | GetViewportImage

# --- [MODELS] ---------------------------------------------------------------------------


class Input(msgspec.Struct, frozen=True, omit_defaults=True):
    """`run_python` arguments."""

    script: str
    slot: str | None = None


class Event(msgspec.Struct, frozen=True, tag_field="tool_name"):
    """PreToolUse event of a `rhino-mcp-platform` call, tagged by tool name."""


class RunPython(Event, frozen=True, tag="mcp__rhino-mcp-platform__run_python"):
    """PreToolUse event of a `run_python` call."""

    tool_input: Input


class RunCommand(Event, frozen=True, tag="mcp__rhino-mcp-platform__run_command"):
    """PreToolUse event of a `run_command` call."""

    reason: ClassVar[str] = "run_command refused, document.command(doc, macro, layer_path, ids) in run_python runs macros"


class CloseDoc(Event, frozen=True, tag="mcp__rhino-mcp-platform__close_doc"):
    """PreToolUse event of a `close_doc` call."""

    reason: ClassVar[str] = "close_doc refused, document.close(doc) in run_python or close_slot closes documents"


class SaveDoc(Event, frozen=True, tag="mcp__rhino-mcp-platform__save_doc"):
    """PreToolUse event of a `save_doc` call."""

    reason: ClassVar[str] = "save_doc refused, document.save(doc, path) or document.export(doc, path, ids) in run_python"


class OpenDoc(Event, frozen=True, tag="mcp__rhino-mcp-platform__open_doc"):
    """PreToolUse event of an `open_doc` call."""

    reason: ClassVar[str] = "open_doc refused, open -g -b com.mcneel.rhinoceros.9 <file> opens, document.load imports"


class GetViewportImage(Event, frozen=True, tag="mcp__rhino-mcp-platform__get_viewport_image"):
    """PreToolUse event of a `get_viewport_image` call."""

    reason: ClassVar[str] = "get_viewport_image refused, document.capture(doc, name) in run_python draws views unchanged"


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
        f"document.run({source!r}, {f'MCP: {label}'!r}, globals())",
        "",
    ))


def decision(event: Call) -> dict[str, object]:
    """Return a refusal naming its reason, or the wrapped `run_python` input for a script event."""
    match event:
        case RunPython(tool_input=tool_input):
            return {"updatedInput": msgspec.structs.replace(tool_input, script=wrap(tool_input.script))}
        case refused:
            return {"permissionDecision": "deny", "permissionDecisionReason": refused.reason}


# --- [COMPOSITION] ----------------------------------------------------------------------


def main() -> None:
    """Print the hook's output for the stdin event."""
    output = decision(msgspec.json.decode(sys.stdin.buffer.read(), type=Call))
    sys.stdout.buffer.write(msgspec.json.encode({"hookSpecificOutput": {"hookEventName": "PreToolUse", **output}}))


if __name__ == "__main__":
    main()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Call", "CloseDoc", "Event", "GetViewportImage", "Input", "OpenDoc", "RunCommand", "RunPython", "SaveDoc", "decision", "main", "wrap"]
