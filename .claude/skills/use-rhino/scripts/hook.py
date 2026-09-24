# /// script
# requires-python = ">=3.13"
# dependencies = ["msgspec", "packaging"]
# ///
"""PreToolUse hook that runs each `run_python` script inside the skill's runtime and refuses Rhino calls that reach IronPython or reload the shared interpreter."""

from importlib.metadata import distributions
from pathlib import Path
import re
import sys
import tomllib
from typing import Final

import msgspec
from packaging.requirements import Requirement
from packaging.utils import canonicalize_name

# --- [CONSTANTS] ------------------------------------------------------------------------

IRONPYTHON: Final = 'This call runs a .py file on IronPython 2.7; run the file through _-ScriptEditor _Run "<file>"'
RELOAD: Final = "python.reloadEngine reloads every caller's modules in place and keeps names a module dropped; the hook evicts the skill modules on each call"

# --- [MODELS] ---------------------------------------------------------------------------


class Input(msgspec.Struct, frozen=True):
    """`run_python` arguments."""

    script: str
    slot: str | None = None


class Command(msgspec.Struct, frozen=True):
    """`run_command` arguments."""

    command: str
    slot: str | None = None


class Script(msgspec.Struct, frozen=True, tag_field="tool_name", tag="mcp__rhino-mcp-platform__run_python"):
    """PreToolUse event of a `run_python` call."""

    tool_input: Input


class Macro(msgspec.Struct, frozen=True, tag_field="tool_name", tag="mcp__rhino-mcp-platform__run_command"):
    """PreToolUse event of a `run_command` call."""

    tool_input: Command


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


def decision(event: Script | Macro) -> dict[str, object] | None:
    """The hook's decision for the event: a refusal naming its reason, the wrapped `run_python` input, or none for a `run_command` passed unchanged."""
    match event:
        case Script(tool_input=Input(script=text)) | Macro(tool_input=Command(command=text)) if "runpythonscript" in text.casefold():
            return {"permissionDecision": "deny", "permissionDecisionReason": IRONPYTHON}
        case Script(tool_input=Input(script=text)) if "python.reloadengine" in text.casefold():
            return {"permissionDecision": "deny", "permissionDecisionReason": RELOAD}
        case Script(tool_input=tool_input):
            return {"updatedInput": msgspec.structs.replace(tool_input, script=wrap(tool_input.script))}
        case Macro():
            return None


# --- [COMPOSITION] ----------------------------------------------------------------------


def main() -> None:
    """Print the hook's output for the stdin event, nothing for a call it passes unchanged."""
    if (output := decision(msgspec.json.decode(sys.stdin.buffer.read(), type=Script | Macro))) is not None:
        sys.stdout.buffer.write(msgspec.json.encode({"hookSpecificOutput": {"hookEventName": "PreToolUse", **output}}))


if __name__ == "__main__":
    main()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Command", "Input", "Macro", "Script", "decision", "main", "wrap"]
