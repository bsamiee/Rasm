"""Keyword arguments restating the literal default a same-module function or a `msgspec.Struct` field declares."""

from collections.abc import Iterator
from typing import override

import msgspec
from mypy.nodes import (
    ArgKind,
    AssignmentStmt,
    BytesExpr,
    CallExpr,
    Decorator,
    Expression,
    FloatExpr,
    FuncDef,
    IntExpr,
    MemberExpr,
    MypyFile,
    NameExpr,
    RefExpr,
    StrExpr,
    SymbolNode,
    TypeInfo,
    UnaryExpr,
    Var,
)
from mypy.types import Instance
from refurb.checks.common import get_mypy_type, is_equivalent, stringify
from refurb.error import Error
from refurb.visitor import TraverserVisitor

# --- [CONSTANTS] ------------------------------------------------------------------------

_STRUCT = f"{msgspec.Struct.__module__}.{msgspec.Struct.__qualname__}"
_FIELD = f"{msgspec.field.__module__}.{msgspec.field.__qualname__}"

# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorRestatedDefaultError(Error):
    """Keyword argument equal to the literal default its callee or `msgspec.Struct` field declares.

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

    prefix = "RASM"
    code = 102
    name = "restated-default"
    categories = ("function",)


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: MypyFile, errors: list[Error]) -> None:
    """Reports each keyword argument equal to the literal default of a function the module defines or of a `msgspec.Struct` field."""

    def literal(expression: Expression) -> bool:
        match expression:
            case IntExpr() | FloatExpr() | StrExpr() | BytesExpr() | NameExpr(fullname="builtins.True" | "builtins.False" | "builtins.None"):
                return True
            case UnaryExpr(op="-", expr=IntExpr() | FloatExpr()):
                return True
            case _:
                return False

    def declared(callee: Expression) -> SymbolNode | None:
        match callee, get_mypy_type(callee.expr) if isinstance(callee, MemberExpr) else None:
            case MemberExpr(name=name), Instance(type=owner) | (TypeInfo() as owner):
                return member.node if (member := owner.get(name)) is not None else None
            case RefExpr(node=symbol), _:
                return symbol
            case _:
                return None

    def assigned(info: TypeInfo) -> Iterator[tuple[str, Expression]]:
        for statement in info.defn.defs.body:
            match statement:
                case AssignmentStmt(lvalues=[NameExpr(name=name, node=Var(is_classvar=False))], rvalue=CallExpr(callee=RefExpr(fullname=callee)) as call) if callee == _FIELD:
                    yield name, next((value for argument, value in zip(call.arg_names, call.args) if argument == "default"), call)
                case AssignmentStmt(lvalues=[NameExpr(name=name, node=Var(is_classvar=False))], rvalue=value, type=annotation) if annotation is not None:
                    yield name, value
                case _:
                    pass

    def fields(info: TypeInfo) -> dict[str, Expression]:
        return {name: value for owner in reversed(info.mro) if owner.fullname != _STRUCT for name, value in assigned(owner)}

    class Defined(TraverserVisitor):
        def __init__(self) -> None:
            super().__init__()
            self.functions: set[FuncDef] = set()

        @override
        def visit_func_def(self, o: FuncDef) -> None:
            self.functions.add(o)
            super().visit_func_def(o)

    class Calls(TraverserVisitor):
        @override
        def visit_call_expr(self, o: CallExpr) -> None:
            match declared(o.callee):
                case (FuncDef() as function) | Decorator(func=FuncDef() as function) if function in defined.functions:
                    owner, defaults = function.name, {argument.variable.name: argument.initializer for argument in function.arguments if argument.initializer is not None}
                case TypeInfo() as info if info.has_base(_STRUCT):
                    owner, defaults = info.name, fields(info)
                case _:
                    owner, defaults = "", {}
            errors.extend(
                ErrorRestatedDefaultError.from_node(value, f"`{name}={stringify(value)}` restates the default `{owner}` declares, remove the argument")
                for name, value, kind in zip(o.arg_names, o.args, o.arg_kinds)
                if kind is ArgKind.ARG_NAMED and name is not None and (default := defaults.get(name)) is not None and literal(default) and is_equivalent(value, default)
            )
            super().visit_call_expr(o)

    defined = Defined()
    defined.accept(node)
    Calls().accept(node)
