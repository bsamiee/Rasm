# /// script
# requires-python = ">=3.13"
# dependencies = ["packaging"]
# ///
"""PreToolUse hook over `run_python`: the script imports this directory's modules fresh, keeps its stdout and traceback, runs as one named undo step, and sees the slot's document through `scriptcontext.doc`."""

from importlib.metadata import distributions
import json
from pathlib import Path
import re
import sys
import tomllib

from packaging.requirements import Requirement
from packaging.utils import canonicalize_name

# --- [OPERATIONS] -----------------------------------------------------------------------


def wrap(source: str) -> str:
    """Return the script under RhinoCode directives for this directory and the packages Rhino's Python lacks, run by a function that names the undo step after the first comment line."""
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


# --- [COMPOSITION] ----------------------------------------------------------------------


def main() -> None:
    """Read the PreToolUse event on stdin and print the `tool_input` with its script wrapped."""
    tool_input = json.load(sys.stdin)["tool_input"]
    json.dump({"hookSpecificOutput": {"hookEventName": "PreToolUse", "updatedInput": {**tool_input, "script": wrap(tool_input["script"])}}}, sys.stdout)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["main", "wrap"]

if __name__ == "__main__":
    main()
