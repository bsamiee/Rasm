"""Structlog log fields lost to event text, positional arguments, or a discarded logger."""

from inspect import getmembers, isfunction, signature
from typing import get_protocol_members

import structlog
from structlog.stdlib import AsyncBoundLogger
from structlog.typing import BindableLogger, FilteringBoundLogger
lazy from mypy.nodes import ArgKind, CallExpr, Expression, ExpressionStmt, MemberExpr, OpExpr, RefExpr
lazy from mypy.types import get_proper_type, Type
lazy from refurb.checks.common import get_mypy_type, is_subclass, stringify
lazy from refurb.error import Error

from tools.refurb.findings import fullname, RasmError

# --- [CONSTANTS] ------------------------------------------------------------------------

_LOGGERS = tuple(map(fullname, (FilteringBoundLogger, BindableLogger, structlog.BoundLoggerBase, AsyncBoundLogger)))
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


def _logger(expression: Expression) -> bool:
    match expression:
        case CallExpr(callee=RefExpr(fullname=name)) if name in _FACTORIES:
            return True
        case CallExpr(callee=MemberExpr(name=name, expr=receiver)) if name in _BINDERS:
            return _logger(receiver)
        case _:
            return _typed(expression, *_LOGGERS)


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: CallExpr | ExpressionStmt, errors: list[Error]) -> None:
    """Reports each structlog call that loses a log field."""

    def formatted(event: Expression) -> bool:
        match event:
            case OpExpr(op="%" | "+") | CallExpr(callee=RefExpr(fullname="builtins.str" | "builtins.repr" | "builtins.format")):
                return True
            case CallExpr(callee=MemberExpr(name="format" | "join", expr=receiver)):
                return _typed(receiver, fullname(str))
            case _:
                return False

    match node:
        case CallExpr(callee=MemberExpr(name=name, expr=receiver) as callee) if name in _EVENTS and _logger(receiver):
            passed = [argument for argument, kind in zip(node.args, node.arg_kinds) if kind in {ArgKind.ARG_POS, ArgKind.ARG_STAR}][_EVENTS[name] :]
            events = [*passed[:1], *(argument for argument, keyword in zip(node.args, node.arg_names) if keyword == "event")]
            errors.extend(
                ErrorLostLogFieldError.from_node(event, f"`{stringify(callee)}` formats values into event text, pass each value as a keyword argument") for event in events[:1] if formatted(event)
            )
            if passed[1:]:
                errors.append(ErrorLostLogFieldError.from_node(callee, f"`{stringify(callee)}` takes positional event arguments, pass each value as a keyword argument"))
        case ExpressionStmt(expr=CallExpr(callee=MemberExpr(name=name, expr=receiver) as callee)) if name in _BINDERS and _logger(receiver):
            errors.append(ErrorLostLogFieldError.from_node(node, f"Statement discards the bound logger `{stringify(callee)}` returns, assign it and log through it"))
        case _:
            pass
