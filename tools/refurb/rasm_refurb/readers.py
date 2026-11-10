"""Package-internal module names and private class members whose readers, counted across every file of the member refurb builds, number one, and private-module exports no other file reads."""

import ast
from functools import cache
from importlib.util import resolve_name
from pathlib import Path
import tomllib
from typing import override
lazy from collections import Counter, defaultdict
lazy from collections.abc import Callable, Iterator

import msgspec
lazy from expression import Option
lazy from expression.extra.result import catch
lazy from mypy.nodes import (
    AssignmentStmt,
    BytesExpr,
    CallExpr,
    ClassDef,
    ComparisonExpr,
    ComplexExpr,
    ConditionalExpr,
    Decorator,
    EllipsisExpr,
    Expression,
    ExpressionStmt,
    FloatExpr,
    ForStmt,
    FuncDef,
    IfStmt,
    ImportFrom,
    IntExpr,
    LambdaExpr,
    ListExpr,
    MatchStmt,
    MemberExpr,
    MypyFile,
    NameExpr,
    Node,
    OpExpr,
    OverloadedFuncDef,
    RefExpr,
    ReturnStmt,
    Statement,
    StrExpr,
    TupleExpr,
    TypeInfo,
    UnaryExpr,
    WhileStmt,
)
lazy from mypy.types import get_proper_type, Instance, Type, TypeType, TypeVarType
lazy from mypy.util import correct_relative_import
lazy from refurb.checks.common import get_mypy_type
lazy from refurb.error import Error
lazy from refurb.settings import Settings

from rasm_refurb._findings import RasmError
from rasm_refurb._loops import Traverser

# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorNameReadersError(RasmError):
    """Package-internal module function or constant, or private class member, read by one function, method, or statement across the member or by none, a private name another file reads, or a private module's `__all__` name no other file reads.

    Bad:

    ```
    def _scaled(value: float) -> float:
        return value * 2.0


    def area(width: float, height: float) -> float:
        return _scaled(width) * height


    __all__ = ["area", "helper"]
    ```

    Good:

    ```
    def area(width: float, height: float) -> float:
        return width * 2.0 * height


    __all__ = ["area"]
    ```
    """

    code = 106
    categories = ("readability",)


# --- [MODELS] ---------------------------------------------------------------------------


class _Index(msgspec.Struct, frozen=True, kw_only=True):
    """Reads of every member file: the names each module loads, reader units per name with the nesting of each read, names read outside a call or as a comparison operand, the modules reading each name, and names a host, an entry, or an importable module publishes."""

    loads: frozendict[str, frozenset[str]]
    readers: frozendict[tuple[str, str], tuple[tuple[str, str, int], ...]]
    values: frozenset[tuple[str, str]]
    compared: frozenset[tuple[str, str]]
    importers: frozendict[tuple[str, str], frozenset[str]]
    public: frozenset[tuple[str, str]]
    members: frozendict[str, frozenset[str]]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _dotted(node: ast.expr, bound: dict[str, str], /) -> str | None:
    match node:
        case ast.Name(id=name):
            return bound.get(name)
        case ast.Attribute(value=value, attr=attribute):
            return None if (head := _dotted(value, bound)) is None else f"{head}.{attribute}"
        case _:
            return None


@cache
def _index(roots: tuple[Path, ...], /) -> _Index:
    """Walks every member file once per reading unit: resolved imports, reads with their nesting, private attribute reads, manifest entry points, `<module>:<name>` strings, and the `__all__` of each module an entry exports or a consumer imports by a public path."""
    paths = {
        ".".join(part for part in relative.with_suffix("").parts if part != "__init__"): relative
        for root in roots
        for path in root.rglob("*.py")
        for relative in (path.relative_to(Path.cwd()),)
        if not any(part.startswith((".", "__pycache__")) for part in relative.parts)
    }
    trees = {module: ast.parse(path.read_bytes()) for module, path in paths.items()}
    modules = frozenset(trees)
    readers: defaultdict[tuple[str, str], list[tuple[str, str, int]]] = defaultdict(list)
    importers: defaultdict[tuple[str, str], set[str]] = defaultdict(set)
    members: defaultdict[str, set[str]] = defaultdict(set)
    loads: defaultdict[str, set[str]] = defaultdict(set, {module: set() for module in modules})
    values: set[tuple[str, str]] = set()
    compared: set[tuple[str, str]] = set()
    units: list[tuple[str, str, ast.AST, bool]] = []
    exports: dict[str, tuple[str, ...]] = {}
    public: set[tuple[str, str]] = {
        (source, name)
        for root in roots
        for target in catch(Path.read_text, exception=OSError)(root / "pyproject.toml")
        .map(lambda text: tomllib.loads(text).get("project", {}))
        .map(lambda project: (*project.get("scripts", {}).values(), *(*group.values() for group in project.get("entry-points", {}).values())))
        .default_value(())
        for source, _, name in (target.partition(":"),)
    }
    for module, statement in ((module, statement) for module, tree in trees.items() for statement in tree.body):
        match statement:
            case ast.FunctionDef(name=name) | ast.AsyncFunctionDef(name=name):
                units.extend(((module, f"{module}.{name}", statement, False), *((module, f"{module}.{name}", decorator, True) for decorator in statement.decorator_list)))
            case ast.ClassDef(name=name, body=body):
                units.extend((
                    *((module, f"{module}.{name}", part, True) for part in (*statement.bases, *statement.keywords, *statement.decorator_list, *statement.type_params)),
                    *((module, f"{module}.{name}.{member.name}" if isinstance(member, ast.FunctionDef | ast.AsyncFunctionDef) else f"{module}:{member.lineno}", member, False) for member in body),
                ))
            case ast.If(test=ast.Compare(left=ast.Name(id="__name__"), comparators=[ast.Constant(value="__main__")])):
                units.append((module, "", statement, True))
            case ast.Assign(targets=[ast.Name(id="__all__")], value=ast.List(elts=items) | ast.Tuple(elts=items)):
                exports[module] = tuple(item.value for item in items if isinstance(item, ast.Constant) and isinstance(item.value, str))
            case _:
                units.append((module, f"{module}:{statement.lineno}", statement, False))

    def leveled(root: ast.AST, /) -> Iterator[tuple[ast.AST, int]]:
        """Yields each node under a reading unit with its nesting: one level per enclosing branch, loop, match, lambda, or local function, a chained `elif` or conditional alternative adding none."""
        pending: list[tuple[ast.AST, int, bool]] = [(root, 0, True)]
        while pending:
            node, outer, flat = pending.pop()
            level = outer + (not flat and isinstance(node, ast.If | ast.IfExp | ast.For | ast.AsyncFor | ast.While | ast.Match | ast.Lambda | ast.FunctionDef | ast.AsyncFunctionDef))
            yield node, level
            chained = node.orelse[0] if isinstance(node, ast.If) and len(node.orelse) == 1 else node.orelse if isinstance(node, ast.IfExp) else None
            pending.extend((child, level, child is chained and type(child) is type(node)) for child in ast.iter_child_nodes(node))

    walked = tuple((module, label, held, tuple(node for node, _ in found), {id(node): level for node, level in found}) for module, label, unit, held in units for found in (tuple(leveled(unit)),))
    sources = {
        id(found): resolve_name("." * found.level + (found.module or ""), module if paths[module].name == "__init__.py" else module.rpartition(".")[0])
        for module, _, _, nodes, _ in walked
        for found in nodes
        if isinstance(found, ast.ImportFrom)
    }
    bound = {
        module: {
            name: target
            for statement in tree.body
            if isinstance(statement, ast.Import | ast.ImportFrom)
            for alias in statement.names
            for name, target in (
                (
                    (Option.of_optional(alias.asname).default_value(alias.name), f"{sources[id(statement)]}.{alias.name}")
                    if isinstance(statement, ast.ImportFrom)
                    else (alias.asname, alias.name)
                    if alias.asname is not None
                    else (alias.name.partition(".")[0],) * 2
                ),
            )
        }
        for module, tree in trees.items()
    }
    public.update((module, name) for module, names in exports.items() if not any(part.startswith("_") for part in module.split(".")) for name in names)
    for module, label, held, nodes, levels in walked:
        imported = tuple((sources[id(found)], alias.name) for found in nodes if isinstance(found, ast.ImportFrom) and sources[id(found)] in modules for alias in found.names)
        exported = imported if paths[module].name == "__init__.py" else ()
        public.update((
            *exported,
            *((space, name) for space in map(".".join, exported) for name in exports.get(space, ())),
            *((split[0], split[2]) for found in nodes if isinstance(found, ast.Constant) and isinstance(found.value, str) if (split := found.value.partition(":"))[1] and split[0] in modules),
        ))
        loads[module].update(found.id for found in nodes if isinstance(found, ast.Name) and isinstance(found.ctx, ast.Load))
        for key in imported:
            importers[key].add(module)
        for attribute in (found.attr for found in nodes if isinstance(found, ast.Attribute) and found.attr.startswith("_") and not found.attr.endswith("_")):
            members[attribute].add(module)
        callees = frozenset(id(found.func) for found in nodes if isinstance(found, ast.Call))
        operands = frozenset(id(operand) for found in nodes if isinstance(found, ast.Compare) for operand in (found.left, *found.comparators))
        inner = frozenset(id(found.value) for found in nodes if isinstance(found, ast.Attribute))
        for found, key in (
            (found, key)
            for found in nodes
            if isinstance(found, ast.Name | ast.Attribute) and isinstance(found.ctx, ast.Load) and id(found) not in inner
            if (dotted := _dotted(found, bound[module])) is not None
            for parts in (dotted.split("."),)
            if (key := next(((prefix, parts[index]) for index in range(len(parts) - 1, 0, -1) if (prefix := ".".join(parts[:index])) in modules), None)) is not None
        ):
            importers[key].add(module)
            readers[key].append((module, label, levels[id(found)]))
            values.update((key,) if held or id(found) not in callees else ())
            compared.update((key,) if id(found) in operands else ())
            public.update(() if label else (key,))
    return _Index(
        loads=frozendict((module, frozenset(found)) for module, found in loads.items()),
        readers=frozendict((key, tuple(found)) for key, found in readers.items()),
        values=frozenset(values),
        compared=frozenset(compared),
        importers=frozendict((key, frozenset(found)) for key, found in importers.items()),
        public=frozenset(public),
        members=frozendict((name, frozenset(found)) for name, found in members.items()),
    )


def _rebuilt(expression: Expression, /) -> bool | None:
    """Whether each read of a value builds it again: `False` for a value the compiler folds or a member another name states, `True` for a lambda and for an operation or tuple over a member, `None` for a call or display built once."""
    match expression:
        case IntExpr() | FloatExpr() | ComplexExpr() | StrExpr() | BytesExpr() | EllipsisExpr() | NameExpr(fullname="builtins.True" | "builtins.False" | "builtins.None") | MemberExpr(expr=NameExpr()):
            return False
        case MemberExpr(expr=MemberExpr() as owner):
            return _rebuilt(owner)
        case LambdaExpr():
            return True
        case UnaryExpr(expr=operand):
            parts: tuple[Expression, ...] = (operand,)
        case OpExpr(left=left, right=right):
            parts = (left, right)
        case TupleExpr(items=items):
            parts = tuple(items)
        case _:
            return None
    costs = tuple(map(_rebuilt, parts))
    return None if None in costs else any(costs) or any(isinstance(part, MemberExpr) for part in parts)


def _inexact(expression: Expression, /) -> bool:
    match expression:
        case FloatExpr() | ComplexExpr():
            return True
        case UnaryExpr(expr=operand):
            return _inexact(operand)
        case _:
            return False


def _assigned(statement: Statement, /) -> tuple[NameExpr, Expression] | None:
    match statement:
        case AssignmentStmt(lvalues=[NameExpr() as target], rvalue=value, unanalyzed_type=None, is_alias_def=False):
            return target, value
        case _:
            return None


# --- [SERVICES] -------------------------------------------------------------------------


class _Walk(Traverser):
    """Names one reading unit reads with the nesting of each read, the callees of its calls, the operands of its comparisons, and the deepest nesting under its root."""

    def __init__(self, root: Node) -> None:
        """Holds no read yet, the root and each chained `elif` or conditional alternative adding no level."""
        super().__init__()
        self.found: list[tuple[NameExpr | MemberExpr, int]] = []
        self.callees: set[int] = set()
        self.operands: set[int] = set()
        self.flat: set[int] = {id(root.func if isinstance(root, Decorator) else root)}
        self.level = 0
        self.depth = 0

    def _nested[N: Node](self, o: N, visit: Callable[[N], None], /) -> None:
        step = id(o) not in self.flat
        self.level += step
        self.depth = max(self.depth, self.level)
        visit(o)
        self.level -= step

    @override
    def visit_if_stmt(self, o: IfStmt) -> None:
        chained = o.else_body.body if o.else_body is not None else []
        self.flat.update(id(held) for held in chained if len(chained) == 1 and isinstance(held, IfStmt))
        self._nested(o, super().visit_if_stmt)

    @override
    def visit_conditional_expr(self, o: ConditionalExpr) -> None:
        self.flat.update((id(o.else_expr),) if isinstance(o.else_expr, ConditionalExpr) else ())
        self._nested(o, super().visit_conditional_expr)

    @override
    def visit_for_stmt(self, o: ForStmt) -> None:
        self._nested(o, super().visit_for_stmt)

    @override
    def visit_while_stmt(self, o: WhileStmt) -> None:
        self._nested(o, super().visit_while_stmt)

    @override
    def visit_match_stmt(self, o: MatchStmt) -> None:
        self._nested(o, super().visit_match_stmt)

    @override
    def visit_lambda_expr(self, o: LambdaExpr) -> None:
        self._nested(o, super().visit_lambda_expr)

    @override
    def visit_func_def(self, o: FuncDef) -> None:
        self._nested(o, super().visit_func_def)

    @override
    def visit_comparison_expr(self, o: ComparisonExpr) -> None:
        self.operands.update(map(id, o.operands))
        super().visit_comparison_expr(o)

    @override
    def visit_call_expr(self, o: CallExpr) -> None:
        self.callees.add(id(o.callee))
        super().visit_call_expr(o)

    @override
    def visit_name_expr(self, o: NameExpr) -> None:
        if not o.is_new_def:
            self.found.append((o, self.level))
        super().visit_name_expr(o)

    @override
    def visit_member_expr(self, o: MemberExpr) -> None:
        self.found.append((o, self.level))
        super().visit_member_expr(o)


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: MypyFile, errors: list[Error], settings: Settings) -> None:
    """Reports a function one call reads, another candidate one unit reads, a private candidate nothing reads, another module's private name the file imports or reads through that module, and a private module's `__all__` name no other file reads.

    Candidates are module functions, constants no call builds, and private class members no base declares, each read under the scope mypy resolves for it: its module, or the class whose body declares a member read through an instance, a class, or `self`.
    A one-reader candidate is reported while the nesting of its read and the depth of its body sum to 3 or less, a body of more than one returned expression one level deeper as the local function it folds into.
    A name an entry exports, an `__all__` name of a module an entry exports or of a module with no private path part, and a constant each read builds again with a reader outside import time are no candidates.
    """
    index = _index(tuple(Path(found).resolve() for found in settings.files))
    module = node.fullname
    entry = node.is_package_init_file()
    named = tuple(found for statement in node.defs if (found := _assigned(statement)) is not None)
    units: list[tuple[str, Node, bool]] = []
    once = Counter(target.name for target, _ in named)
    declared: list[tuple[tuple[str, str], Node, bool, bool, bool]] = [
        ((module, target.name), target, False, _inexact(value), rebuilt)
        for target, value in named
        if once[target.name] == 1 and not target.name.startswith("__") and (rebuilt := _rebuilt(value)) is not None
    ]
    for statement in node.defs:
        match statement:
            case FuncDef(name=name) | OverloadedFuncDef(name=name):
                units.append((f"{module}.{name}", statement, False))
                declared.extend(() if isinstance(statement, OverloadedFuncDef) or name.startswith("__") else (((module, name), statement, True, False, False),))
            case Decorator(func=function, decorators=decorators):
                units.extend(((f"{module}.{function.name}", function, False), *((f"{module}.{function.name}", decorator, True) for decorator in decorators)))
            case ClassDef(defs=block, info=info):
                held = tuple(found for member in block.body if (found := _assigned(member)) is not None)
                single = Counter(target.name for target, _ in held)
                units.extend((
                    *((f"{module}.{statement.name}", part, True) for part in (*statement.base_type_exprs, *statement.decorators, *statement.keywords.values())),
                    *(
                        (f"{module}.{statement.name}.{member.name}" if isinstance(member, FuncDef | Decorator | OverloadedFuncDef) else f"{module}:{member.line}", member, False)
                        for member in block.body
                    ),
                ))
                declared.extend(
                    ((info.fullname, name), member, isinstance(member, FuncDef | Decorator), inexact, rebuilt)
                    for member, name, inexact, rebuilt in (
                        *((member, member.name, False, False) for member in block.body if isinstance(member, FuncDef)),
                        *(
                            (member, member.func.name, False, False)
                            for member in block.body
                            if isinstance(member, Decorator)
                            and all(isinstance(decorator, RefExpr) and decorator.fullname in {"builtins.staticmethod", "builtins.classmethod"} for decorator in member.decorators)
                        ),
                        *((target, target.name, _inexact(value), rebuilt) for target, value in held for rebuilt in (_rebuilt(value),) if single[target.name] == 1 and rebuilt is not None),
                    )
                    if not info.is_enum and name.startswith("_") and not name.endswith("_") and not info.names[name].plugin_generated and not any(name in base.names for base in info.mro[1:])
                )
            case IfStmt(expr=[ComparisonExpr(operands=[NameExpr(name="__name__"), StrExpr(value="__main__")])]):
                units.append(("", statement, True))
            case _:
                units.append((f"{module}:{statement.line}", statement, False))
    reads: defaultdict[tuple[str, str], list[tuple[str, int]]] = defaultdict(list)
    depths: defaultdict[str, int] = defaultdict(int)
    outside: dict[tuple[str, str], Node] = {
        (correct_relative_import(module, found.relative, found.id, entry)[0], name): found for found in node.imports if isinstance(found, ImportFrom) for name, _ in found.names
    }
    values: set[tuple[str, str]] = set()
    compared: set[tuple[str, str]] = set()
    for label, unit, held in units:
        walk = _Walk(unit)
        walk.accept(unit)
        body = unit.func if isinstance(unit, Decorator) else unit
        steps = tuple(held for held in body.body.body if not (isinstance(held, ExpressionStmt) and isinstance(held.expr, StrExpr))) if isinstance(body, FuncDef) else ()
        depths[label] = max(depths[label], walk.depth + (len(steps) > 1 or not all(isinstance(held, ReturnStmt) for held in steps)))
        for reference, level in walk.found:
            typed = get_mypy_type(reference.expr) if isinstance(reference, MemberExpr) and not reference.fullname else None
            proper = get_proper_type(typed) if isinstance(typed, Type) else typed
            match reference.fullname, proper:
                case str(name), _ if name:
                    key = (name.rpartition(".")[0], reference.name)
                case _, Instance(type=info) | TypeType(item=Instance(type=info)) | TypeVarType(upper_bound=Instance(type=info)) | TypeType(item=TypeVarType(upper_bound=Instance(type=info))) | (
                    TypeInfo() as info
                ):
                    key = (next((base.fullname for base in info.mro if reference.name in base.names), ""), reference.name)
                case _:
                    key = ("", reference.name)
            reads[key].append((label, level))
            outside.update(() if isinstance(reference, NameExpr) or key in outside else ((key, reference),))
            values.update((key,) if held or not label or id(reference) not in walk.callees else ())
            compared.update((key,) if id(reference) in walk.operands else ())
    errors.extend((
        *(
            ErrorNameReadersError.from_node(declaration, f"`{key[1]}` has one reader, move it into `{reader}`")
            for key, declaration, function, inexact, rebuilt in (() if entry else declared)
            if key not in index.public and not (function and (key in values or key in index.values)) and not (inexact and (key in compared or key in index.compared))
            if key[0] == module or not index.members.get(key[1], frozenset()) - {module}
            for sites in ((*reads[key], *((label, level) for owner, label, level in index.readers.get(key, ()) if owner != module)),)
            if len(found := frozenset(label for label, _ in sites)) == 1 and (len(sites) == 1 or not function) and "" not in found and f"{key[0]}.{key[1]}" not in found
            for reader in found
            if not rebuilt or ":" in reader
            if max(level for _, level in sites) + depths[f"{key[0]}.{key[1]}" if function else f"{module}:{declaration.line}"] <= 3
        ),
        *(
            ErrorNameReadersError.from_node(declaration, f"`{key[1]}` has no reader, delete it")
            for key, declaration, _, _, _ in (() if entry else declared)
            if key[1].startswith("_")
            and key not in index.public
            and key not in index.readers
            and (key[1] not in index.loads[module] if key[0] == module else not reads[key] and key[1] not in index.members)
        ),
        *(
            ErrorNameReadersError.from_node(found, f"`{name}` is read outside `{source}`, name it without `_` there and list it in its `__all__`")
            for (source, name), found in outside.items()
            if source != module and source in index.loads and name.startswith("_") and not name.startswith("__") and f"{source}.{name}" not in index.loads
        ),
        *(
            ErrorNameReadersError.from_node(item, f"`{item.value}` has no reader outside its file, remove it from `__all__` and prefix it with `_`")
            for statement in (() if entry or not any(part.startswith("_") for part in module.split(".")) else node.defs)
            if isinstance(statement, AssignmentStmt)
            and [target.name for target in statement.lvalues if isinstance(target, NameExpr)] == ["__all__"]
            and isinstance(statement.rvalue, ListExpr | TupleExpr)
            for item in statement.rvalue.items
            if isinstance(item, StrExpr) and (module, item.value) not in index.public and not index.importers.get((module, item.value), frozenset()) - {module}
        ),
    ))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("ErrorNameReadersError", "check")
