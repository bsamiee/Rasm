"""Keyword arguments restating the literal default a function of the same module declares."""

from typing import override

from mypy.nodes import ArgKind, BytesExpr, CallExpr, Decorator, Expression, FloatExpr, FuncDef, IntExpr, MemberExpr, MypyFile, NameExpr, StrExpr, SymbolNode, TypeInfo, UnaryExpr
from mypy.types import Instance
from refurb.checks.common import get_mypy_type, is_equivalent, stringify
from refurb.error import Error
from refurb.visitor import TraverserVisitor

# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorRestatedDefaultError(Error):
    """Keyword argument equal to the literal default its callee declares.

    Bad:

    ```
    def framed(text: str, *, width: int = 80) -> str: ...


    framed(text, width=80)
    ```

    Good:

    ```
    framed(text)
    ```
    """

    prefix = "RASM"
    code = 102
    name = "restated-default"
    categories = ("function",)


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: MypyFile, errors: list[Error]) -> None:
    """Reports each keyword argument equal to the literal default of a function or method the module defines."""

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
            case NameExpr(node=symbol), _:
                return symbol
            case MemberExpr(name=name), Instance(type=owner) | (TypeInfo() as owner):
                return member.node if (member := owner.get(name)) is not None else None
            case _:
                return None

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
                    defaults = {argument.variable.name: argument.initializer for argument in function.arguments if argument.initializer is not None and literal(argument.initializer)}
                    errors.extend(
                        ErrorRestatedDefaultError.from_node(value, f"`{name}={stringify(value)}` restates the default `{function.name}` declares, remove the argument")
                        for name, value, kind in zip(o.arg_names, o.args, o.arg_kinds)
                        if kind is ArgKind.ARG_NAMED and name is not None and (default := defaults.get(name)) is not None and is_equivalent(value, default)
                    )
                case _:
                    pass
            super().visit_call_expr(o)

    defined = Defined()
    defined.accept(node)
    Calls().accept(node)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ErrorRestatedDefaultError", "check"]
