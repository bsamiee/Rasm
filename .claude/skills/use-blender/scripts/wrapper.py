# mypy: disable-error-code="attr-defined, union-attr"
# ty: ignore[unresolved-attribute, invalid-context-manager]
# ruff: file-ignore[mutable-class-default, private-member-access]
# /// script
# requires-python = ">=3.13"
# dependencies = ["attrs", "msgspec"]
# ///
"""PreToolUse hook wrapping agent code in this file's `agent_call`, which reports unfinished operators on every host, runs background calls in the file's window, and closes a live call with one grouped undo step."""

from collections.abc import Iterator
from contextlib import contextmanager
from pathlib import Path
import symtable
import sys
from typing import override, Protocol, TYPE_CHECKING

import attrs

if TYPE_CHECKING or "bpy" in sys.modules:
    import bpy
if TYPE_CHECKING or "bpy" not in sys.modules:
    import msgspec
if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import OperatorReturnItems

# --- [TYPES] ----------------------------------------------------------------------------


class BPyOpFunction(Protocol):
    """Operator function `bpy.ops.<module>.<name>` returns."""

    def __call__(self, *args: object, **kwargs: object) -> set[str]:
        """Return set of the operator, positional arguments the execution context and undo flag, keywords its properties."""
        ...

    def idname_py(self) -> str:
        """Operator name as `<module>.<name>`."""
        ...


# --- [MODELS] ---------------------------------------------------------------------------


@attrs.frozen
class Reported:
    """Operator function that prints `bpy.ops.<op> returned [...]` for a return set without `FINISHED`, reading every other attribute from the function."""

    function: BPyOpFunction

    def __getattr__(self, name: str) -> object:
        """Attribute of the operator function, `poll` and `get_rna_type` among them."""
        return getattr(self.function, name)

    def __call__(self, *args: object, **kwargs: object) -> set[str]:
        """Return set of the operator call, printed when the operator did not finish."""
        status = self.function(*args, **kwargs)
        if "FINISHED" not in status:
            sys.stdout.write(f"bpy.ops.{self.function.idname_py()} returned {sorted(status)}\n")
        return status


# --- [OPERATIONS] -----------------------------------------------------------------------


@contextmanager
def agent_call() -> Iterator[None]:
    """Scope of one agent call in Blender: `bpy.ops` lookups return `Reported` functions, background runs see the file's first window, and a live call ends with one grouped `Agent (<mode>)` undo step."""

    @contextmanager
    def undo_step() -> Iterator[None]:
        """Scope closed by the grouped step of the mode the code leaves, registering that mode's step operator on its first use."""
        try:
            yield
        finally:
            mode = bpy.context.mode
            name = f"agent_step_{mode.lower()}"

            class Step(bpy.types.Operator):
                """Undo step closing agent calls, consecutive calls in one mode sharing it."""

                bl_idname = f"mcp.{name}"
                bl_label = f"Agent ({bpy.types.UILayout.enum_item_name(bpy.context, 'mode', mode)})"
                bl_options = {"INTERNAL", "UNDO_GROUPED"}

                @override
                def execute(self, context: bpy.types.Context | None) -> "set[OperatorReturnItems]":
                    return {"FINISHED"}

            if bpy.types.Operator.bl_rna_get_subclass_py(f"MCP_OT_{name}") is None:
                bpy.utils.register_class(Step)
            undo = True
            create("mcp", name)("EXEC_DEFAULT", undo)

    create, window = bpy.ops._op_create_function, bpy.data.window_managers[0].windows[0]
    with bpy.context.temp_override(window=window, screen=window.screen) if bpy.app.background else undo_step():
        bpy.ops._op_create_function = lambda module, name: Reported(create(module, name))
        try:
            yield
        finally:
            bpy.ops._op_create_function = create


def wrap(code: str) -> str | None:
    """Agent code as a call under `agent_call` with `<agent>` line numbers as sent, `None` for code that fails to parse or binds `check_is_finished` at module level to defer its reply."""
    try:
        table = symtable.symtable(code, "<agent>", "exec")
    except SyntaxError:
        return None
    deferred = any(s.get_name() == "check_is_finished" and (s.is_assigned() or s.is_imported()) for s in table.get_symbols())
    source = str(Path(__file__).resolve())
    return None if deferred else f'with __import__("runpy").run_path({source!r})[{agent_call.__name__!r}]():\n    exec(compile({code!r}, "<agent>", "exec"), globals())\n'


# --- [COMPOSITION] ----------------------------------------------------------------------


def main() -> None:
    """Answer `updatedInput` with the wrapped code, nothing when the code runs as sent."""
    match msgspec.json.decode(sys.stdin.buffer.read()):
        case {"tool_input": {"code": str() as code} as tool_input} if (updated := wrap(code)) is not None:
            sys.stdout.buffer.write(msgspec.json.encode({"hookSpecificOutput": {"hookEventName": "PreToolUse", "updatedInput": {**tool_input, "code": updated}}}))


if __name__ == "__main__":
    main()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BPyOpFunction", "Reported", "agent_call", "main", "wrap"]
