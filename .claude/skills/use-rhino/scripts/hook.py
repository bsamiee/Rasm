"""PreToolUse hook that runs each `run_python` script through `document.run` and refuses the router tools a script entry point replaces."""

from pathlib import Path
import sys
from typing import ClassVar

import msgspec

from interface.host import bootstrap

# --- [TYPES] ----------------------------------------------------------------------------

type Call = RunPython | RunCommand | CloseDoc | SaveDoc | OpenDoc

# --- [MODELS] ---------------------------------------------------------------------------


class Input(msgspec.Struct, frozen=True):
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

    reason: ClassVar[str] = (
        "run_command runs its macro in an inactive document, leaves an unanswered prompt holding every later command, and returns the process's command window text. "
        "document.command(doc, macro, layer_path, ids) in run_python cancels a prompt the macro leaves, refuses an inactive document, and returns the command's objects, results, and output"
    )


class CloseDoc(Event, frozen=True, tag="mcp__rhino-mcp-platform__close_doc"):
    """PreToolUse event of a `close_doc` call."""

    reason: ClassVar[str] = (
        "close_doc writes the document with a preview image and holds Rhino's UI thread past the router limit. document.close(doc) in run_python closes a titled document, close_slot a spawned one"
    )


class SaveDoc(Event, frozen=True, tag="mcp__rhino-mcp-platform__save_doc"):
    """PreToolUse event of a `save_doc` call."""

    reason: ClassVar[str] = (
        "save_doc writes a copy that leaves the document's path and edits unsaved. "
        "A copy onto the document's own file makes macOS reopen the document and drop later edits. "
        "document.save(doc, path) in run_python saves the document, document.export(doc, path, ids) writes a copy"
    )


class OpenDoc(Event, frozen=True, tag="mcp__rhino-mcp-platform__open_doc"):
    """PreToolUse event of an `open_doc` call."""

    reason: ClassVar[str] = (
        "open_doc imports into the slot's document and zooms every view to its extents. "
        "open -g -b com.mcneel.rhinoceros.9 <file> opens a file as its own document, document.load(doc, path, layer_path) in run_python imports under a layer"
    )


# --- [OPERATIONS] -----------------------------------------------------------------------


def wrap(source: str) -> str:
    """Return comment lines from the first 31 lines for RhinoCode directives, then a `document.run` of the script in an undo step its first comment line names."""
    comments = [(number, line) for number, line in enumerate(source.splitlines()) if line.lstrip().startswith("#")]
    label = next(filter(None, (line.strip().lstrip("# ") for _, line in comments)), "run_python")
    return "\n".join((*(line for number, line in comments if number < 31), bootstrap("document", t"run({source}, {f'MCP: {label}'}, globals())", Path(__file__).parent), ""))


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

__all__ = ["Call", "CloseDoc", "Event", "Input", "OpenDoc", "RunCommand", "RunPython", "SaveDoc", "decision", "main", "wrap"]
