# /// script
# requires-python = ">=3.13"
# dependencies = ["msgspec", "packaging"]
# ///
"""PreToolUse hook that runs each `run_python` script inside the skill's runtime and refuses the router tools and `RunScript` calls script entry points replace, IronPython runs, and interpreter reloads."""

from importlib.metadata import distributions
from pathlib import Path
import re
import sys
import tomllib
from typing import ClassVar

import msgspec
from packaging.requirements import Requirement
from packaging.utils import canonicalize_name

# --- [TYPES] ----------------------------------------------------------------------------

type Call = Script | Macro | CloseDoc | SaveDoc | OpenDoc

# --- [MODELS] ---------------------------------------------------------------------------


class Input(msgspec.Struct, frozen=True):
    """`run_python` arguments."""

    script: str
    slot: str | None = None


class Event(msgspec.Struct, frozen=True, tag_field="tool_name"):
    """PreToolUse event of a `rhino-mcp-platform` call, tagged by its tool name."""


class Script(Event, frozen=True, tag="mcp__rhino-mcp-platform__run_python"):
    """PreToolUse event of a `run_python` call."""

    tool_input: Input


class Macro(Event, frozen=True, tag="mcp__rhino-mcp-platform__run_command"):
    """PreToolUse event of a `run_command` call."""

    reason: ClassVar[str] = (
        "run_command returns the process command history and leaves an unanswered prompt waiting. "
        "document.command(doc, macro, layer_path, ids) in run_python returns the command's objects, results, and output"
    )


class CloseDoc(Event, frozen=True, tag="mcp__rhino-mcp-platform__close_doc"):
    """PreToolUse event of a `close_doc` call."""

    reason: ClassVar[str] = (
        "close_doc writes the document with a preview image and holds the UI thread past the router limit. close(doc) in run_python closes a titled document, close_slot a spawned one"
    )


class SaveDoc(Event, frozen=True, tag="mcp__rhino-mcp-platform__save_doc"):
    """PreToolUse event of a `save_doc` call."""

    reason: ClassVar[str] = "save_doc writes a copy and leaves the document's own path and edits unsaved. save(doc, path) in run_python saves the document and export(doc, path, ids) writes a copy"


class OpenDoc(Event, frozen=True, tag="mcp__rhino-mcp-platform__open_doc"):
    """PreToolUse event of an `open_doc` call."""

    reason: ClassVar[str] = (
        "open_doc imports into the slot's document. open -g -b com.mcneel.rhinoceros.9 <file> opens a file as its own document and load(doc, path, layer_path) imports under a layer"
    )


# --- [OPERATIONS] -----------------------------------------------------------------------


def wrap(source: str) -> str:
    """Return the script under hoisted RhinoCode directives, run with bytecode writes off, fresh skill modules, and captured output in one undo step its first comment line names."""
    directive = re.compile(r'#(?:!\s|\s*(?:(?i:r|requirements): |env: |venv: |r\s+")|\s+(?i:platform|async|flag):)')
    metadata = re.compile(r"(?m)^# /// script$\s((?:^#(?:| .*)$\s)+)^# ///$")
    folder = Path(__file__).resolve().parent
    modules = {
        module.stem: table for module in sorted(folder.glob("*.py")) if "requires-python" not in (table := tomllib.loads(re.sub(r"(?m)^# ?", "", "".join(metadata.findall(module.read_text())))))
    }
    installed = [{canonicalize_name(found.name) for found in distributions(path=[str(site)])} for site in Path.home().glob(".rhinocode/py3*/site-envs/default-*/lib/python3*/site-packages")]
    packages = [
        package for table in modules.values() for package in table.get("dependencies", ()) if not installed or any(canonicalize_name(Requirement(package).name) not in names for names in installed)
    ]
    lines = [line.strip() for line in source.splitlines()]
    hoisted = dict.fromkeys((f"# env: {folder}", *(f"# r: {package}" for package in packages), *filter(directive.match, lines)))
    label = next(filter(None, (line.lstrip("# ") for line in lines if line.startswith("#") and not directive.match(line))), "run_python")
    runner = """def _rhino_mcp_run(source, label, namespace, modules):
    import contextlib, io, linecache, scriptcontext, sys, traceback
    sys.dont_write_bytecode = True
    for name in modules:
        sys.modules.pop(name, None)
    doc, buffer, active = namespace["__rhino_doc__"], io.StringIO(), scriptcontext.doc
    linecache.cache["<run_python>"] = (len(source), None, source.splitlines(True), "<run_python>")
    scriptcontext.doc, record = doc, doc.BeginUndoRecord(label)
    try:
        with contextlib.redirect_stdout(buffer), contextlib.redirect_stderr(buffer):
            exec(compile(source, "<run_python>", "exec"), namespace)
    except BaseException as error:
        traceback.print_exception(error.with_traceback(error.__traceback__.tb_next), file=buffer)
    finally:
        scriptcontext.doc = active
        if record:
            doc.EndUndoRecord(record)
    print(buffer.getvalue(), end="")"""
    return "\n".join((*hoisted, runner, f"_rhino_mcp_run({source!r}, {f'MCP: {label}'!r}, globals(), {tuple(modules)!r})", ""))


def decision(event: Call) -> dict[str, object]:
    """Return a refusal naming its reason, or the wrapped `run_python` input for a script event."""
    match event:
        case Script(tool_input=Input(script=text)) if "runpythonscript" in text.casefold():
            reason = 'Script runs a .py file on IronPython 2.7. Run the file through _-ScriptEditor _Run "<file>"'
        case Script(tool_input=Input(script=text)) if "python.reloadengine" in text.casefold():
            reason = "python.reloadEngine reloads every caller's modules in place and keeps names a module dropped. The hook evicts the skill modules on each call"
        case Script(tool_input=Input(script=text)) if ".runscript(" in text.casefold():
            reason = (
                "RunScript on an inactive document spins Rhino's UI thread in a point prompt no call releases. "
                "document.command(doc, macro, layer_path, ids) runs a macro in the active document and refuses another"
            )
        case Script(tool_input=tool_input):
            return {"updatedInput": msgspec.structs.replace(tool_input, script=wrap(tool_input.script))}
        case refused:
            reason = refused.reason
    return {"permissionDecision": "deny", "permissionDecisionReason": reason}


# --- [COMPOSITION] ----------------------------------------------------------------------


def main() -> None:
    """Print the hook's output for the stdin event."""
    output = decision(msgspec.json.decode(sys.stdin.buffer.read(), type=Call))
    sys.stdout.buffer.write(msgspec.json.encode({"hookSpecificOutput": {"hookEventName": "PreToolUse", **output}}))


if __name__ == "__main__":
    main()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Call", "CloseDoc", "Event", "Input", "Macro", "OpenDoc", "SaveDoc", "Script", "decision", "main", "wrap"]
