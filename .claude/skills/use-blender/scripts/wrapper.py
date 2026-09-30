# mypy: disable-error-code="attr-defined"
# ty: ignore[unresolved-attribute]
# ruff: file-ignore[boolean-positional-value-in-call, mutable-class-default, exec-builtin]
"""PreToolUse hook sending `execute_blender_code` and headless code through `run`, with the scripts folder imported fresh and one undo step closing each live call."""

from pathlib import Path
import sys
from typing import override, TYPE_CHECKING, TypedDict
import warnings

if TYPE_CHECKING or "bpy" in sys.modules:
    import bpy
if TYPE_CHECKING or "bpy" not in sys.modules:
    import msgspec
if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import OperatorReturnItems

# --- [MODELS] ---------------------------------------------------------------------------


class Event(TypedDict):
    """PreToolUse event of an `execute_blender_code` call, every tool argument a string."""

    hook_event_name: str
    tool_input: dict[str, str]


# --- [OPERATIONS] -----------------------------------------------------------------------


def step() -> None:
    """Push the undo step named after the current mode, consecutive calls in one mode sharing it."""
    mode = bpy.context.mode
    name = f"agent_step_{mode.lower()}"

    class Step(bpy.types.Operator):
        """Undo step closing a live call."""

        bl_idname = f"mcp.{name}"
        bl_label = f"Agent ({bpy.types.UILayout.enum_item_name(bpy.context, 'mode', mode)})"
        bl_options = {"INTERNAL", "UNDO_GROUPED"}

        @override
        def execute(self, context: bpy.types.Context) -> "set[OperatorReturnItems]":
            return {"FINISHED"}

    if name not in dir(bpy.ops.mcp):
        bpy.utils.register_class(Step)
    getattr(bpy.ops.mcp, name)("EXEC_DEFAULT", True)


def run(code: str, namespace: dict[str, object]) -> None:
    """Execute the code in the namespace with each deprecation printed once per line, pushing the final mode's undo step on return and on raise outside a background process."""
    try:
        with warnings.catch_warnings(action="default", category=DeprecationWarning):
            exec(compile(code, "<agent>", "exec"), namespace)
    finally:
        if not bpy.app.background:
            step()


def wrap(code: str) -> str:
    """Source calling `run` on the code in the caller's namespace with every module of the scripts folder imported fresh and bytecode written under the host's cache prefix."""
    script = Path(__file__).resolve()
    folder, names = str(script.parent), sorted(path.stem for path in script.parent.glob("*.py"))
    return (
        f"import importlib, sys\nsys.pycache_prefix = {sys.pycache_prefix!r}\n"
        f"for name in {names!r}:\n    sys.modules.pop(name, None)\n"
        f"if {folder!r} not in sys.path:\n    sys.path.insert(0, {folder!r})\n"
        f"importlib.import_module({script.stem!r}).{run.__name__}({code!r}, globals())\n"
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


def main() -> None:
    """Answer the stdin event with `updatedInput` holding its code wrapped."""
    event = msgspec.json.decode(sys.stdin.buffer.read(), type=Event)
    sys.stdout.buffer.write(msgspec.json.encode({"hookSpecificOutput": {"hookEventName": event["hook_event_name"], "updatedInput": event["tool_input"] | {"code": wrap(event["tool_input"]["code"])}}}))


if __name__ == "__main__":
    main()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Event", "main", "run", "step", "wrap"]
