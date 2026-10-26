"""Keyword arguments restating the literal default a same-module function or a `msgspec.Struct` field declares."""

lazy from typing import override

import msgspec
lazy from mypy.nodes import (
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
    Statement,
    StrExpr,
    SymbolNode,
    TypeInfo,
    UnaryExpr,
    Var,
)
lazy from mypy.types import Instance
lazy from refurb.checks.common import get_mypy_type, is_equivalent, stringify
lazy from refurb.error import Error
lazy from refurb.visitor import TraverserVisitor

from tools.refurb.findings import fullname, RasmError

# --- [CONSTANTS] ------------------------------------------------------------------------

_STRUCT = fullname(msgspec.Struct)

# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorRestatedDefaultError(RasmError):
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

    code = 102
    categories = ("function",)


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: MypyFile, errors: list[Error]) -> None:
    """Reports each keyword argument equal to the literal default of a function the module defines or of a `msgspec.Struct` field."""

    def literal(expression: Expression) -> bool:
        match expression:
            case IntExpr() | FloatExpr() | StrExpr() | BytesExpr() | NameExpr(fullname="builtins.True" | "builtins.False" | "builtins.None") | UnaryExpr(op="-", expr=IntExpr() | FloatExpr()):
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

    def assigned(statement: Statement) -> tuple[tuple[str, Expression], ...]:
        match statement:
            case AssignmentStmt(lvalues=[NameExpr(name=name, node=Var(is_classvar=False))], rvalue=CallExpr(callee=RefExpr(fullname=callee)) as call) if callee == fullname(msgspec.field):
                return ((name, next((value for argument, value in zip(call.arg_names, call.args) if argument == "default"), call)),)
            case AssignmentStmt(lvalues=[NameExpr(name=name, node=Var(is_classvar=False))], rvalue=value, type=annotation) if annotation is not None:
                return ((name, value),)
            case _:
                return ()

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
                    owner, defaults = info.name, {name: value for base in reversed(info.mro) if base.fullname != _STRUCT for statement in base.defn.defs.body for name, value in assigned(statement)}
                case _:
                    owner, defaults = "", {}
            errors.extend(
                ErrorRestatedDefaultError.from_node(value, f"`{name}={stringify(value)}` restates the default `{owner}` declares, remove the argument")
                for name, value in zip(o.arg_names, o.args)
                if name is not None and (default := defaults.get(name)) is not None and literal(default) and is_equivalent(value, default)
            )
            super().visit_call_expr(o)

    defined = Defined()
    defined.accept(node)
    Calls().accept(node)
