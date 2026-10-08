# /// script
# dependencies = ["msgspec"]
#
# [tool.ty.rules]
# all = "error"
# dynamic-function-decorator-return = "ignore"
# unsound-assignment = "ignore"
# unsound-return-statement = "ignore"
# ///
# ty: ignore[unresolved-attribute]
# mypy: disable-error-code=attr-defined
# ruff: file-ignore[boolean-positional-value-in-call, exec-builtin, mutable-class-default, private-member-access]
"""PreToolUse hook closing each `execute_blender_code` call with an undo step."""

from collections.abc import Callable
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
    """PreToolUse event of a `blender` or `mcp-for-blender` call."""

    hook_event_name: str
    tool_name: str
    tool_input: dict[str, object]


# --- [OPERATIONS] -----------------------------------------------------------------------


def step() -> None:
    """Push the undo step named after the current mode in a GUI process, consecutive calls in one mode sharing it."""
    if bpy.app.background:
        return
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


def deferred(check: Callable[[], object]) -> Callable[[], object]:
    """`check_is_finished` pushing the undo step once the check answers or raises."""

    def checked() -> object:
        try:
            answer = check()
        except Exception:
            step()
            raise
        if answer is not None:
            step()
        return answer

    return checked


def run(code: str, namespace: dict[str, object]) -> Callable[[], object] | None:
    """Execute the code in the namespace with each warning printed once per line on the call's stderr, then push the final mode's undo step, or return the `check_is_finished` the code defines pushing it after its last pass."""
    try:
        with warnings.catch_warnings(action="default", category=DeprecationWarning):
            warnings.showwarning = warnings._showwarning_orig
            exec(compile(code, "<agent>", "exec"), namespace)
    except BaseException:
        step()
        raise
    match namespace.get("check_is_finished"):
        case check if callable(check):
            return deferred(check)
        case _:
            step()
            return None


def wrap(code: str) -> str:
    """Source binding `check_is_finished` to `run` on the code in the caller's namespace, with every module of the scripts folder imported fresh and bytecode under the host's cache prefix."""
    script = Path(__file__).resolve()
    folder, names = str(script.parent), sorted(path.stem for path in script.parent.glob("*.py"))
    return (
        f"__import__('sys').pycache_prefix = {sys.pycache_prefix!r}\n"
        f"[__import__('sys').modules.pop(name, None) for name in {names!r}]\n"
        f"{folder!r} in __import__('sys').path or __import__('sys').path.insert(0, {folder!r})\n"
        f"check_is_finished = __import__({script.stem!r}).{run.__name__}({code!r}, globals())\n"
    )


def decision(event: Event) -> dict[str, object] | None:
    """Return the wrapped `execute_blender_code` input, or `None` for a call that runs as sent."""
    match event["tool_name"].rpartition("__")[2], event["tool_input"]:
        case "execute_blender_code", {"code": str() as code} as tool_input:
            return {"permissionDecision": "allow", "updatedInput": tool_input | {"code": wrap(code)}}
        case _:
            return None


# --- [COMPOSITION] ----------------------------------------------------------------------


def main() -> None:
    """Print the hook's output for the stdin event, nothing for a call that runs as sent."""
    event = msgspec.json.decode(sys.stdin.buffer.read(), type=Event)
    if (output := decision(event)) is not None:
        sys.stdout.buffer.write(msgspec.json.encode({"hookSpecificOutput": {"hookEventName": event["hook_event_name"], **output}}))


if __name__ == "__main__":
    main()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Event", "decision", "deferred", "main", "run", "step", "wrap"]
