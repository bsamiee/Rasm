# mypy: disable-error-code="attr-defined, union-attr"
# ty: ignore[unresolved-attribute, invalid-context-manager]
# ruff: file-ignore[boolean-positional-value-in-call, mutable-class-default, private-member-access]
# /// script
# requires-python = ">=3.13"
# dependencies = ["attrs", "msgspec"]
# ///
"""PreToolUse hook refusing agent code whose statements reach a Blender crash and wrapping the rest in this file's `agent_call`, which reports unfinished operators on every host, runs background calls in the file's window, and closes a live call with one grouped undo step."""

import ast
from collections.abc import Iterable, Iterator
from contextlib import contextmanager
from enum import auto, Enum, StrEnum
from functools import reduce
import operator
from pathlib import Path
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


class Fact(Enum):
    """Value a name holds on some path through its scope."""

    REGION_VIEW = auto()
    KEYCONFIG = auto()
    KEYCONFIG_PREFERENCES = auto()
    RELOADED_PREFERENCES = auto()
    BOUND = auto()
    REMOVED_ID = auto()


class Crash(StrEnum):
    """Statement sequence that segfaults Blender, its value the refusal naming the form that runs."""

    VIEW_UPDATE = (
        "`RegionView3D.update()` in a background process segfaults Blender in `GPU_matrix_ortho_set` with no GPU context. "
        "Read `view_rotation`, `view_location`, and `view_distance` in background code and run the update in the live session"
    )
    KEYCONFIG_RELOAD = (
        "A GUI write of `use_mouse_emulate_3_button` or `mouse_emulate_3_button_modifier` reloads the keyconfig and unregisters its `KeyConfigPreferences` type. "
        "A read through a `preferences` reference taken before the write segfaults Blender. Read `keyconfigs.active.preferences` after the write"
    )
    BATCH_REMOVE = (
        "`bpy.data.batch_remove` given an ID held from before an earlier `batch_remove`, `orphans_purge`, or `bpy.data.<collection>.remove` segfaults Blender "
        "in `pyrna_id_FromPyObject` once that removal freed it. Read the IDs from `bpy.data` after the removal or pass them all to one `batch_remove`"
    )
    WORKSPACE_SWITCH = (
        "`bpy.ops.workspace.duplicate()` switches to the copy on a later event-loop pass. A duplicate, delete, or `window.workspace` write before that pass "
        "segfaults Blender in `ED_workspace_change`. Yield from a `bpy.app.timers` generator until `window.workspace` is the copy"
    )
    REGION_HUD = (
        "A write of `show_region_hud` with a window in context segfaults Blender in `ED_area_init`. Leave the HUD at its stored state, `bpy.ops.screen.redo_last()` shows Adjust Last Operation"
    )


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


@attrs.frozen
class Flow:
    """Facts a scope's statements leave on some path to the current statement, a workspace duplicate pending in the event-loop pass, and each crash those paths reach."""

    background: bool
    facts: frozenset[tuple[Fact, str]] = frozenset()
    duplicating: bool = False
    crashes: frozenset[tuple[int, Crash]] = frozenset()

    def __or__(self, other: "Flow") -> "Flow":
        """Flow after either of two paths."""
        return Flow(self.background, self.facts | other.facts, self.duplicating or other.duplicating, self.crashes | other.crashes)

    def kinds(self, value: ast.expr | None) -> frozenset[Fact]:
        """Facts a value holds, those of an aliased name or those its attribute chain names."""
        match value, chain(value):
            case ast.Name(id=source), _:
                return frozenset(fact for fact, name in self.facts if name == source)
            case _, (*_, "region_3d"):
                return frozenset({Fact.REGION_VIEW})
            case _, (root, *path) if (Fact.KEYCONFIG, root) in self.facts or "keyconfigs" in path:
                return frozenset({Fact.KEYCONFIG_PREFERENCES if path[-1:] == ["preferences"] else Fact.KEYCONFIG})
            case _:
                return frozenset()

    def bind(self, name: str, value: ast.expr | None) -> "Flow":
        """Flow after the name takes the value, every fact of its earlier value dropped."""
        return attrs.evolve(self, facts=frozenset((fact, held) for fact, held in self.facts if held != name) | {(fact, name) for fact in self.kinds(value) | {Fact.BOUND}})

    def removing(self, names: Iterable[str]) -> "Flow":
        """Flow after a removal freed the IDs the names hold."""
        return attrs.evolve(self, facts=self.facts | {(Fact.REMOVED_ID, name) for name in names})

    def crash(self, node: ast.expr, crash: Crash) -> "Flow":
        """Flow with the crash the node's `<agent>` line reaches."""
        return attrs.evolve(self, crashes=self.crashes | {(node.lineno, crash)})

    def store(self, node: ast.expr, attr: str) -> "Flow":
        """Flow after a write of the attribute."""
        match attr:
            case "use_mouse_emulate_3_button" | "mouse_emulate_3_button_modifier" if not self.background:
                return attrs.evolve(self, facts=frozenset((Fact.RELOADED_PREFERENCES if fact is Fact.KEYCONFIG_PREFERENCES else fact, name) for fact, name in self.facts))
            case "workspace" if self.duplicating:
                return self.crash(node, Crash.WORKSPACE_SWITCH)
            case "show_region_hud":
                return self.crash(node, Crash.REGION_HUD)
            case _:
                return self

    def event(self, node: ast.AST) -> "Flow":
        """Flow after one evaluated node."""
        match node:
            case ast.Name(id=name, ctx=ast.Load()) if (Fact.RELOADED_PREFERENCES, name) in self.facts:
                return self.crash(node, Crash.KEYCONFIG_RELOAD)
            case ast.Attribute(attr=attr, ctx=ast.Store()) | ast.Call(func=ast.Name(id="setattr"), args=[_, ast.Constant(value=str() as attr), _]):
                return self.store(node, attr)
            case ast.Call(func=ast.Attribute(attr="update", value=owner)) if self.background and (Fact.REGION_VIEW in self.kinds(owner) or chain(owner)[-1:] == ("RegionView3D",)):
                return self.crash(node, Crash.VIEW_UPDATE)
            case ast.Call(func=ast.Attribute(attr="batch_remove")):
                held = passed(node)
                return (self.crash(node, Crash.BATCH_REMOVE) if any((Fact.REMOVED_ID, name) in self.facts for name in held) else self).removing(held)
            case ast.Call(func=ast.Attribute(attr="orphans_purge")):
                return self.removing(name for fact, name in self.facts if fact is Fact.BOUND)
            case ast.Call(func=ast.Attribute(attr="remove", value=ast.Attribute(value=owner))) if chain(owner)[-1:] == ("data",):
                return self.removing(passed(node))
            case ast.Call(func=func) if not self.background and (path := chain(func)[-3:]) in {("ops", "workspace", "duplicate"), ("ops", "workspace", "delete")}:
                return attrs.evolve(self.crash(node, Crash.WORKSPACE_SWITCH) if self.duplicating else self, duplicating=self.duplicating or path[-1] == "duplicate")
            case ast.Yield() | ast.YieldFrom():
                return attrs.evolve(self, duplicating=False)
            case ast.NamedExpr(target=ast.Name(id=name), value=value):
                return self.bind(name, value)
            case ast.Lambda(body=body):
                return attrs.evolve(self, crashes=self.crashes | Flow(self.background).visit(body).crashes)
            case _:
                return self

    def visit(self, node: ast.AST) -> "Flow":
        """Flow after every node of an expression in evaluation order, children before their parent and a lambda without its body."""
        return (self if isinstance(node, ast.Lambda) else reduce(Flow.visit, ast.iter_child_nodes(node), self)).event(node)

    def assign(self, target: ast.expr | None, value: ast.expr | None) -> "Flow":
        """Flow after the target takes the value, element by element for an unpacked tuple or list display."""
        match target, value:
            case ast.Name(id=name), _:
                return self.bind(name, value)
            case ((ast.Tuple(elts=targets) | ast.List(elts=targets)), (ast.Tuple(elts=values) | ast.List(elts=values))) if len(targets) == len(values):
                return reduce(lambda flow, pair: flow.assign(*pair), zip(targets, values, strict=True), self)
            case ((ast.Tuple(elts=targets) | ast.List(elts=targets)), _):
                return reduce(lambda flow, element: flow.assign(element, None), targets, self)
            case ast.Starred(value=inner), _:
                return self.assign(inner, None)
            case (ast.Attribute() | ast.Subscript()) as stored, _:
                return self.visit(stored)
            case _:
                return self

    def run(self, body: Iterable[ast.stmt]) -> "Flow":
        """Flow after the statements in order."""
        return reduce(Flow.step, body, self)

    def step(self, statement: ast.stmt) -> "Flow":
        """Flow after one statement, branches joined, loop bodies run zero, one, and two times, a `while` whose body yields ending past the pass it waits for, and nested scopes read from their own start."""
        match statement:
            case ast.FunctionDef(name=name, body=body) | ast.AsyncFunctionDef(name=name, body=body) | ast.ClassDef(name=name, body=body):
                return attrs.evolve(self.bind(name, None), crashes=self.crashes | Flow(self.background).run(body).crashes)
            case ast.If(test=test, body=body, orelse=orelse):
                entered = self.visit(test)
                return entered.run(body) | entered.run(orelse)
            case ast.For(target=target, iter=source, body=body, orelse=orelse) | ast.AsyncFor(target=target, iter=source, body=body, orelse=orelse):
                entered = self.visit(source)
                once = entered.assign(target, None).run(body)
                return (entered | once | once.assign(target, None).run(body)).run(orelse)
            case ast.While(test=test, body=body, orelse=orelse):
                entered = self.visit(test)
                once = entered.run(body).visit(test)
                waited = attrs.evolve(entered, duplicating=entered.duplicating and once.duplicating)
                return (waited | once | once.run(body).visit(test)).run(orelse)
            case ast.With(items=items, body=body) | ast.AsyncWith(items=items, body=body):
                return reduce(lambda flow, item: flow.visit(item.context_expr).assign(item.optional_vars, None), items, self).run(body)
            case ast.Try(body=body, handlers=handlers, orelse=orelse, finalbody=final) | ast.TryStar(body=body, handlers=handlers, orelse=orelse, finalbody=final):
                tried = self.run(body)
                return reduce(operator.or_, ((self | tried).run(handler.body) for handler in handlers), tried.run(orelse)).run(final)
            case ast.Match(subject=subject, cases=cases):
                entered = self.visit(subject)
                return reduce(operator.or_, (entered.run(case.body) for case in cases), entered)
            case ast.Assign(targets=targets, value=value):
                return reduce(lambda flow, target: flow.assign(target, value), targets, self.visit(value))
            case ast.AnnAssign(target=target, value=ast.expr() as value):
                return self.visit(value).assign(target, value)
            case ast.AugAssign(target=target, value=value):
                return self.visit(value).assign(target, None)
            case ast.Import(names=aliases) | ast.ImportFrom(names=aliases):
                return reduce(lambda flow, alias: flow.bind(alias.asname or alias.name.partition(".")[0], None), aliases, self)
            case _:
                return reduce(Flow.visit, ast.iter_child_nodes(statement), self)


# --- [ERRORS] ---------------------------------------------------------------------------


@attrs.frozen
class Refused:
    """Agent code refused before it runs, one reason per `<agent>` line reaching a crash."""

    reasons: tuple[str, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


def chain(node: ast.AST | None) -> tuple[str, ...]:
    """Root name and attribute names of an access chain through calls and subscripts, empty for a chain rooted in another expression."""
    match node:
        case ast.Name(id=name):
            return (name,)
        case ast.Attribute(value=value, attr=attr) if root := chain(value):
            return (*root, attr)
        case ast.Subscript(value=value) | ast.Call(func=value):
            return chain(value)
        case _:
            return ()


def passed(node: ast.AST) -> frozenset[str]:
    """Names whose values the node passes on as they are, a name read through an attribute or bound by a comprehension excluded."""
    match node:
        case ast.Name(id=name, ctx=ast.Load()):
            return frozenset({name})
        case ast.Attribute():
            return frozenset()
        case ast.ListComp(generators=generators) | ast.SetComp(generators=generators) | ast.GeneratorExp(generators=generators) | ast.DictComp(generators=generators):
            return frozenset().union(*map(passed, ast.iter_child_nodes(node))) - {name.id for each in generators for name in ast.walk(each.target) if isinstance(name, ast.Name)}
        case _:
            return frozenset().union(*map(passed, ast.iter_child_nodes(node)))


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
            create("mcp", name)("EXEC_DEFAULT", True)

    create, window = bpy.ops._op_create_function, bpy.data.window_managers[0].windows[0]
    with bpy.context.temp_override(window=window, screen=window.screen) if bpy.app.background else undo_step():
        bpy.ops._op_create_function = lambda module, name: Reported(create(module, name))
        try:
            yield
        finally:
            bpy.ops._op_create_function = create


def wrap(code: str, *, background: bool) -> str | Refused | None:
    """Agent code as a call under `agent_call` with `<agent>` line numbers as sent, `Refused` for code reaching a crash in a background or live process, `None` for code that fails to parse or binds `check_is_finished` at module level to defer its reply."""
    try:
        tree = ast.parse(code, "<agent>")
    except SyntaxError:
        return None
    flow = Flow(background).run(tree.body)
    match sorted(flow.crashes), (Fact.BOUND, "check_is_finished") in flow.facts:
        case [], False:
            return f'with __import__("runpy").run_path({str(Path(__file__).resolve())!r})[{agent_call.__name__!r}]():\n    exec(compile({code!r}, "<agent>", "exec"), globals())\n'
        case [], True:
            return None
        case crashes, _:
            return Refused(tuple(f"<agent> line {line}: {crash}" for line, crash in crashes))


# --- [COMPOSITION] ----------------------------------------------------------------------


def main() -> None:
    """Answer the stdin event of a live call with a refusal naming each crash or `updatedInput` holding the wrapped code, nothing when the code runs as sent."""

    def answer(output: dict[str, object]) -> None:
        sys.stdout.buffer.write(msgspec.json.encode({"hookSpecificOutput": {"hookEventName": "PreToolUse", **output}}))

    tool_input = msgspec.json.decode(sys.stdin.buffer.read())["tool_input"]
    match wrap(tool_input["code"], background=False):
        case Refused(reasons=reasons):
            answer({"permissionDecision": "deny", "permissionDecisionReason": "\n".join(reasons)})
        case str() as updated:
            answer({"updatedInput": {**tool_input, "code": updated}})
        case None:
            pass


if __name__ == "__main__":
    main()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BPyOpFunction", "Crash", "Fact", "Flow", "Refused", "Reported", "agent_call", "chain", "main", "passed", "wrap"]
