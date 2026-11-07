"""Structlog log fields lost to event text, positional arguments, or a discarded logger."""

from inspect import getmembers, isfunction, signature
from typing import get_protocol_members
lazy from collections import deque

import structlog
from structlog.stdlib import AsyncBoundLogger
from structlog.typing import BindableLogger, FilteringBoundLogger
lazy from mypy.nodes import ArgKind, CallExpr, Expression, ExpressionStmt, FUNC_NO_INFO, FuncDef, get_func_def, ListExpr, MemberExpr, OpExpr, RefExpr, StarExpr, TupleExpr, Var
lazy from mypy.types import CallableType, find_unpack_in_list, get_proper_type, TupleType, Type, TypeVarTupleType, UnpackType
lazy from refurb.checks.common import get_mypy_type, is_subclass
lazy from refurb.error import Error

from rasm_refurb._findings import fullname, RasmError

# --- [CONSTANTS] ------------------------------------------------------------------------

_LOGGERS = frozenset(map(fullname, (FilteringBoundLogger, BindableLogger, structlog.BoundLoggerBase, AsyncBoundLogger)))
_FACTORIES = frozenset(map(fullname, (structlog.get_logger, structlog.stdlib.get_logger, structlog.wrap_logger)))
_BINDERS = frozenset(name for name in get_protocol_members(BindableLogger) if not name.startswith("_"))
_EVENTS = frozendict({name: index for name, member in getmembers(FilteringBoundLogger, isfunction) for index, parameter in enumerate(signature(member).parameters, start=-1) if parameter == "event"})

# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorLostLogFieldError(RasmError):
    """Structlog value in event text, a positional argument, or a discarded bound logger in place of a keyword field.

    Bad:

    ```
    logger.info(f"Queue sent {issued}")
    logger.info("Queue sent %s", issued)
    logger.bind(issued=issued)
    ```

    Good:

    ```
    logger.info("Queue sent", issued=issued)
    logger.bind(issued=issued).info("Queue sent")
    ```
    """

    code = 103
    categories = ("logging",)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _typed(expression: Expression, *owners: str) -> bool:
    return isinstance(found := get_mypy_type(expression), Type) and is_subclass(get_proper_type(found), *owners)


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: CallExpr | ExpressionStmt, errors: list[Error]) -> None:
    """Reports each structlog call that loses a log field."""

    def logger(expression: Expression) -> bool:
        while isinstance(expression, CallExpr):
            match expression.callee:
                case RefExpr(fullname=name) if name in _FACTORIES:
                    return True
                case RefExpr(node=Var(type=CallableType() as signature, is_final=True)) if (factory := get_func_def(signature)) is not None and factory.fullname in _FACTORIES:
                    return True
                case MemberExpr(name=name, expr=receiver) if name in _BINDERS:
                    expression = receiver
                case _:
                    break
        return _typed(expression, *_LOGGERS)

    match node:
        case CallExpr(callee=callee) | ExpressionStmt(expr=CallExpr(callee=callee)):
            pass
        case _:
            return
    match callee:
        case MemberExpr(name=method, expr=receiver) if method in (_BINDERS if isinstance(node, ExpressionStmt) else _EVENTS) and logger(receiver):
            pass
        case RefExpr(node=Var(type=CallableType(is_bound=True) as signature, is_final=True)):
            function = get_func_def(signature)
            if not (isinstance(function, FuncDef) and function.info is not FUNC_NO_INFO and any(base.fullname in _LOGGERS for base in function.info.mro)):
                return
            method = function.name
        case _:
            return
    if isinstance(node, ExpressionStmt):
        if method in _BINDERS:
            errors.append(ErrorLostLogFieldError.from_node(node, f"Statement discards the bound logger `{method}` returns, assign it and log through it"))
        return
    if method not in _EVENTS:
        return
    position = _EVENTS[method]

    def formatted(event: Expression) -> bool:
        match event:
            case OpExpr(op="%" | "+") | CallExpr(callee=RefExpr(fullname="builtins.str" | "builtins.repr" | "builtins.format")):
                return True
            case CallExpr(callee=MemberExpr(name="format" | "join", expr=receiver)):
                return _typed(receiver, fullname(str))
            case _:
                return False

    minimum = 0
    positioned: int | None = 0
    event: Expression | None = None
    keyword: Expression | None = None
    pending = deque(zip(node.args, node.arg_kinds, node.arg_names))
    while pending:
        argument, kind, argument_name = pending.popleft()
        found = get_mypy_type(argument) if kind is ArgKind.ARG_STAR and not isinstance(argument, TupleExpr | ListExpr) else None
        match argument, kind, argument_name, get_proper_type(found) if isinstance(found, Type) else None:
            case TupleExpr(items=items) | ListExpr(items=items), ArgKind.ARG_STAR, _, _:
                pending.extendleft((item.expr, ArgKind.ARG_STAR, None) if isinstance(item, StarExpr) else (item, ArgKind.ARG_POS, None) for item in reversed(items))
                continue
            case _, ArgKind.ARG_STAR, _, TupleType() as proper:
                count = proper.length() - sum(1 - (item.type.min_len if isinstance(item.type, TypeVarTupleType) else 0) for item in proper.items if isinstance(item, UnpackType))
                positioned = positioned if find_unpack_in_list(proper.items) is None else None
            case _, ArgKind.ARG_STAR, _, _:
                count, positioned = 0, None
            case _, ArgKind.ARG_POS, _, _:
                count = 1
            case _, ArgKind.ARG_NAMED, "event", _:
                keyword = argument
                continue
            case _:
                continue
        if positioned is not None and positioned <= position < positioned + count:
            event = argument
        minimum += count
        positioned = positioned + count if positioned is not None else None
    if (event := event if event is not None else keyword) is not None and formatted(event):
        errors.append(ErrorLostLogFieldError.from_node(event, f"`{method}` formats values into event text, pass each value as a keyword argument"))
    if minimum > position + 1:
        errors.append(ErrorLostLogFieldError.from_node(callee, f"`{method}` takes positional event arguments, pass each value as a keyword argument"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("ErrorLostLogFieldError", "check")
