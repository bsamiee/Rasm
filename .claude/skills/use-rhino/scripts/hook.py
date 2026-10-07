# /// script
# dependencies = ["msgspec"]
#
# [tool.ty.rules]
# all = "error"
# dynamic-function-decorator-return = "ignore"
# unsound-assignment = "ignore"
# unsound-return-statement = "ignore"
# ///
"""PreToolUse hook that runs each `run_python` script through `document.run` and refuses each router tool another call replaces."""

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


def replacement(tool: str) -> str | None:
    """Return the call that replaces a router tool, `None` for a tool the skill calls."""
    match tool:
        case "run_command":
            return "document.command(doc, macro, layer_path, ids) in run_python runs macros"
        case "run_csharp":
            return "run_python runs scripts through document.run"
        case "open_doc":
            return "open -g -b com.mcneel.rhinoceros.9 <file> opens, document.load imports"
        case "save_doc":
            return "document.save(doc, path) or document.export(doc, path, ids) in run_python"
        case "close_doc":
            return "document.close(doc) in run_python or close_slot closes documents"
        case "get_context" | "get_selection":
            return "document.describe(doc) in run_python reads state and selection"
        case "list_objects":
            return "document.find(doc, layer_path=, object_type=, name=) in run_python finds objects"
        case "get_viewport_image":
            return "document.capture(doc, name) in run_python draws views unchanged"
        case "zoom_to_object" | "zoom_to_layer" | "set_camera":
            return "document.show(doc, view=, zoom=, mode=) in run_python moves the user's view"
        case "set_layer_material":
            return "document.material and document.layer in run_python assign materials"
        case "g2_get_canvas_graph":
            return "canvas.graph(canvas.definition()) in run_python reads the canvas"
        case "g2_place_component" | "g2_place_slider" | "g2_connect" | "g2_connect_many" | "g2_apply_graph" | "g2_delete_component" | "g2_clear_canvas" | "g2_solve_canvas":
            return "canvas.build, wire, and assign in run_python edit task documents"
        case "ask_user":
            return "AskUserQuestion asks the user"
        case _:
            return None


def decision(event: Event) -> dict[str, object] | None:
    """Return the wrapped `run_python` input, a refusal naming the replacing call, or `None` for a call that runs as sent."""
    match event.tool_name.removeprefix("mcp__rhino-mcp-platform__"):
        case "run_python":
            tool_input = msgspec.json.decode(event.tool_input, type=Input)
            return {"permissionDecision": "allow", "updatedInput": msgspec.structs.replace(tool_input, script=wrap(tool_input.script))}
        case tool if (instead := replacement(tool)) is not None:
            return {"permissionDecision": "deny", "permissionDecisionReason": f"{tool} refused, {instead}"}
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

__all__ = ["Event", "Input", "decision", "main", "replacement", "wrap"]
