"""Keyword arguments restating the literal default a same-module function or a `msgspec.Struct` field declares."""

lazy from collections import defaultdict
lazy from collections.abc import Callable
lazy from functools import cache
lazy from typing import override

lazy import msgspec
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
lazy from mypy.types import CallableType, FunctionLike, get_proper_type, Instance
lazy from refurb.checks.common import is_equivalent, stringify
lazy from refurb.error import Error

from tools.refurb.findings import fullname, RasmError, Reads
from tools.refurb.loops import Traverser

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


# --- [COMPOSITION] ----------------------------------------------------------------------


def configured(reads: Reads) -> Callable[[MypyFile, list[Error]], None]:
    """Returns a check that resolves mutable callees through the build's expression types and reads each struct body through `reads`."""
    struct, field = fullname(msgspec.Struct), fullname(msgspec.field)

    def check(node: MypyFile, errors: list[Error]) -> None:
        def literal(expression: Expression) -> bool:
            match expression:
                case IntExpr() | FloatExpr() | StrExpr() | BytesExpr() | NameExpr(fullname="builtins.True" | "builtins.False" | "builtins.None") | UnaryExpr(op="-", expr=IntExpr() | FloatExpr()):
                    return True
                case _:
                    return False

        def declared(callee: Expression) -> SymbolNode | None:
            match callee:
                case IndexExpr(analyzed=TypeApplication(expr=constructor)):
                    callee = constructor
                case _:
                    pass
            proper = get_proper_type(reads.types.get(callee.expr)) if isinstance(callee, MemberExpr) and callee.node is None else None
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
                case Var():
                    match get_proper_type(reads.types.get(callee)):
                        case CallableType() as called:
                            return called.type_object() if called.is_type_obj() else get_func_def(called)
                        case _:
                            return None
                case TypeAlias(target=target, python_3_12_type_alias=False):
                    return proper.type if isinstance(proper := get_proper_type(target), Instance) else None
                case _:
                    return symbol

        @cache
        def fields(base: TypeInfo) -> dict[str, Expression]:
            held: dict[str, Expression] = {}
            for statement in reads.body(base):
                match statement:
                    case AssignmentStmt(lvalues=[NameExpr(name=name, node=Var(is_classvar=False))], rvalue=CallExpr(callee=RefExpr(fullname=callee)) as call) if callee == field:
                        held[name] = next((value for argument, value in zip(call.arg_names, call.args) if argument == "default"), call)
                    case AssignmentStmt(lvalues=[NameExpr(name=name, node=Var(is_classvar=False))], rvalue=value, type=annotation) if annotation is not None:
                        held[name] = value
            return held

        class Walk(Traverser):
            def __init__(self) -> None:
                super().__init__()
                self.functions: set[FuncDef] = set()
                self.calls: defaultdict[SymbolNode, list[CallExpr]] = defaultdict(list)

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
                if any(o.arg_names) and (symbol := declared(o.callee)) is not None:
                    self.calls[symbol].append(o)
                super().visit_call_expr(o)

        walk = Walk()
        walk.accept(node)
        for symbol, calls in walk.calls.items():
            match symbol:
                case FuncDef() as function if function in walk.functions and (function.info is FUNC_NO_INFO or function.is_final or function.info.is_final):
                    owner = function.name
                    held = {name: value for name, argument in zip(function.arg_names, function.arguments) if name is not None and (value := argument.initializer) is not None and literal(value)}
                case TypeInfo() as info if any(base.fullname == struct and base.metaclass_type == info.metaclass_type for base in info.mro):
                    owner = info.name
                    held = {
                        name: value
                        for name in frozenset(name for call in calls for name in call.arg_names if name is not None)
                        if (value := next((found for base in info.mro if (found := fields(base).get(name)) is not None), None)) is not None and literal(value)
                    }
                case _:
                    continue
            errors.extend(
                ErrorRestatedDefaultError.from_node(value, f"`{name}={stringify(value)}` restates the default `{owner}` declares, omit `{name}`")
                for call in calls
                for name, value in zip(call.arg_names, call.args)
                if name is not None and (default := held.get(name)) is not None and is_equivalent(value, default)
            )

    return check


check = configured(Reads())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("ErrorRestatedDefaultError", "check", "configured")
