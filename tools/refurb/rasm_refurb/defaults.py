"""Keyword arguments restating the literal default a same-module function or a `msgspec.Struct` field declares."""

from functools import cache
from typing import override
lazy from collections import Counter, defaultdict

import msgspec
lazy from mypy.nodes import (
    AssignmentStmt,
    BytesExpr,
    CallExpr,
    Decorator,
    Expression,
    FloatExpr,
    FUNC_NO_INFO,
    FuncDef,
    get_func_def,
    IndexExpr,
    IntExpr,
    MemberExpr,
    MypyFile,
    NameExpr,
    OverloadedFuncDef,
    RefExpr,
    StrExpr,
    SymbolNode,
    TypeAlias,
    TypeApplication,
    TypeInfo,
    UnaryExpr,
    Var,
)
lazy from mypy.types import CallableType, FunctionLike, get_proper_type, Instance, Type
lazy from refurb.checks.common import get_mypy_type, is_equivalent, stringify
lazy from refurb.error import Error

from rasm_refurb._findings import fullname, RasmError
from rasm_refurb._loops import Traverser

# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorRestatedDefaultError(RasmError):
    """Keyword argument restating a literal default its callee declares in place of an omitted argument.

    Bad:

    ```
    def framed(text: str, *, width: int = 80) -> str: ...


    class Frame(msgspec.Struct, frozen=True, kw_only=True):
        width: int = 80


    framed(text, width=80)
    Frame(width=80)
    ```

    Good:

    ```
    framed(text)
    Frame()
    ```
    """

    code = 102
    categories = ("function",)


# --- [CONSTANTS] ------------------------------------------------------------------------

_STRUCT = fullname(msgspec.Struct)
_FIELD = fullname(msgspec.field)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _literal(expression: Expression, /) -> bool:
    match expression:
        case IntExpr() | FloatExpr() | StrExpr() | BytesExpr() | NameExpr(fullname="builtins.True" | "builtins.False" | "builtins.None") | UnaryExpr(op="-", expr=IntExpr() | FloatExpr()):
            return True
        case _:
            return False


def _declared(callee: Expression, assigned: Counter[Var], /) -> SymbolNode | None:
    """Declaration a call reaches: its referenced symbol, the member of the receiver type refurb's `get_mypy_type` resolves, or the function a callable variable holds where it is final or bound once, since refurb types a rebound variable by its first binding."""
    match callee:
        case IndexExpr(analyzed=TypeApplication(expr=constructor)):
            callee = constructor
        case _:
            pass
    found = get_mypy_type(callee.expr) if isinstance(callee, MemberExpr) and callee.node is None else None
    proper = get_proper_type(found) if isinstance(found, Type) else found
    match callee, proper.type_object() if isinstance(proper, FunctionLike) and proper.is_type_obj() else proper:
        case RefExpr(node=symbol), _ if symbol is not None:
            pass
        case MemberExpr(name=name), Instance(type=owner) | (TypeInfo() as owner):
            symbol = member.node if (member := owner.get(name)) is not None else None
        case _:
            return None
    match symbol:
        case Decorator(func=function) | OverloadedFuncDef(impl=Decorator(func=function) | (FuncDef() as function)):
            return function
        case Var() as variable if variable.is_final or assigned[variable] == 1:
            match get_proper_type(called) if isinstance(called := get_mypy_type(callee), Type) else None:
                case CallableType() as signature:
                    return signature.type_object() if signature.is_type_obj() else get_func_def(signature)
                case _:
                    return None
        case TypeAlias(target=target, python_3_12_type_alias=False):
            return proper.type if isinstance(proper := get_proper_type(target), Instance) else None
        case _:
            return symbol


@cache
def _fields(base: TypeInfo, /) -> dict[str, Expression]:
    """Default of each field a struct body declares, from a `msgspec.field` default or the assigned value."""
    held: dict[str, Expression] = {}
    for statement in base.defn.defs.body:
        match statement:
            case AssignmentStmt(lvalues=[NameExpr(name=name, node=Var(is_classvar=False))], rvalue=CallExpr(callee=RefExpr(fullname=callee)) as call) if callee == _FIELD:
                held[name] = next((value for argument, value in zip(call.arg_names, call.args) if argument == "default"), call)
            case AssignmentStmt(lvalues=[NameExpr(name=name, node=Var(is_classvar=False))], rvalue=value, type=annotation) if annotation is not None:
                held[name] = value
    return held


# --- [SERVICES] -------------------------------------------------------------------------


class _Walk(Traverser):
    """Undecorated functions of one module, its keyword calls, and the times each variable is bound."""

    def __init__(self) -> None:
        """Holds no function and no call yet."""
        super().__init__()
        self.functions: set[FuncDef] = set()
        self.calls: list[CallExpr] = []
        self.assigned: Counter[Var] = Counter()

    @override
    def visit_func_def(self, o: FuncDef) -> None:
        if not o.is_decorated:
            self.functions.add(o)
        super().visit_func_def(o)

    @override
    def visit_decorator(self, o: Decorator) -> None:
        if not o.decorators:
            self.functions.add(o.func)
        super().visit_decorator(o)

    @override
    def visit_call_expr(self, o: CallExpr) -> None:
        if any(o.arg_names):
            self.calls.append(o)
        super().visit_call_expr(o)

    @override
    def visit_assignment_stmt(self, o: AssignmentStmt) -> None:
        self.assigned.update(target.node for target in o.lvalues if isinstance(target, NameExpr) and isinstance(target.node, Var))
        super().visit_assignment_stmt(o)


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: MypyFile, errors: list[Error]) -> None:
    """Reports each keyword argument restating the literal default a final or same-module function or a `msgspec.Struct` field declares."""
    walk = _Walk()
    walk.accept(node)
    resolved: defaultdict[SymbolNode, list[CallExpr]] = defaultdict(list)
    for call in walk.calls:
        if (symbol := _declared(call.callee, walk.assigned)) is not None:
            resolved[symbol].append(call)
    for symbol, calls in resolved.items():
        match symbol:
            case FuncDef() as function if function in walk.functions and (function.info is FUNC_NO_INFO or function.is_final or function.info.is_final):
                owner = function.name
                held = {name: value for name, argument in zip(function.arg_names, function.arguments) if name is not None and (value := argument.initializer) is not None and _literal(value)}
            case TypeInfo() as info if any(base.fullname == _STRUCT and base.metaclass_type == info.metaclass_type for base in info.mro):
                owner = info.name
                held = {
                    name: value
                    for name in frozenset(name for call in calls for name in call.arg_names if name is not None)
                    if (value := next((found for base in info.mro if (found := _fields(base).get(name)) is not None), None)) is not None and _literal(value)
                }
            case _:
                continue
        errors.extend(
            ErrorRestatedDefaultError.from_node(value, f"`{name}={stringify(value)}` restates the default `{owner}` declares, omit `{name}`")
            for call in calls
            for name, value in zip(call.arg_names, call.args)
            if name is not None and (default := held.get(name)) is not None and is_equivalent(value, default)
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("ErrorRestatedDefaultError", "check")
