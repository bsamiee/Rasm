"""Structlog log fields lost to event text, positional arguments, or a discarded logger."""

from typing import get_protocol_members

from mypy.nodes import ArgKind, CallExpr, Expression, ExpressionStmt, MemberExpr, OpExpr, RefExpr
from mypy.types import get_proper_type, Instance, Type
from refurb.checks.common import get_mypy_type, stringify
from refurb.error import Error
import structlog
from structlog.stdlib import AsyncBoundLogger
from structlog.typing import BindableLogger, FilteringBoundLogger

# --- [CONSTANTS] ------------------------------------------------------------------------

_LOGGERS = tuple(f"{owner.__module__}.{owner.__qualname__}" for owner in (FilteringBoundLogger, BindableLogger, structlog.BoundLoggerBase, AsyncBoundLogger))
_FACTORIES = frozenset(f"{factory.__module__}.{factory.__qualname__}" for factory in (structlog.get_logger, structlog.stdlib.get_logger, structlog.wrap_logger))
_BINDERS = frozenset(name for name in get_protocol_members(BindableLogger) if not name.startswith("_"))
_EVENTS = {name: 0 for level in ("debug", "info", "warning", "warn", "error", "err", "fatal", "exception", "critical", "msg") for name in (level, f"a{level}")} | {"log": 1, "alog": 1}

# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorLostLogFieldError(Error):
    """Structlog value formatted into event text, passed as a positional argument, or bound to a discarded logger.

    Bad:

    ```
    logger.info(f"Queue sent {issued}")
    logger.info("Queue sent %s", issued)
    logger.bind(issued=issued)
    ```

    Good:

    ```
    logger.info("Queue sent", issued=issued)
    ```
    """

    prefix = "RASM"
    code = 103
    name = "lost-log-field"
    categories = ("logging",)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _typed(expression: Expression, owner: str) -> bool:
    found = get_mypy_type(expression)
    return isinstance(found, Type) and isinstance(proper := get_proper_type(found), Instance) and proper.type.has_base(owner)


def _logger(expression: Expression) -> bool:
    match expression:
        case CallExpr(callee=RefExpr(fullname=name)) if name in _FACTORIES:
            return True
        case CallExpr(callee=MemberExpr(name=name, expr=receiver)) if name in _BINDERS:
            return _logger(receiver)
        case _:
            return any(_typed(expression, owner) for owner in _LOGGERS)


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: CallExpr | ExpressionStmt, errors: list[Error]) -> None:
    """Reports each structlog call that loses a log field."""

    def formatted(event: Expression) -> bool:
        match event:
            case OpExpr(op="%" | "+"):
                return True
            case CallExpr(callee=MemberExpr(name="format" | "join", expr=receiver)):
                return _typed(receiver, "builtins.str")
            case CallExpr(callee=RefExpr(fullname="builtins.str" | "builtins.repr" | "builtins.format")):
                return True
            case _:
                return False

    match node:
        case CallExpr(callee=MemberExpr(name=name, expr=receiver) as callee) if name in _EVENTS and _logger(receiver):
            positional = [argument for argument, kind in zip(node.args, node.arg_kinds) if kind in {ArgKind.ARG_POS, ArgKind.ARG_STAR}]
            if (
                event := positional[_EVENTS[name]]
                if len(positional) > _EVENTS[name]
                else next((argument for argument, argument_name in zip(node.args, node.arg_names) if argument_name == "event"), None)
            ) is not None and formatted(event):
                errors.append(ErrorLostLogFieldError.from_node(event, f"`{stringify(callee)}` formats values into its event, pass each as a keyword argument"))
            if len(positional) > _EVENTS[name] + 1:
                errors.append(ErrorLostLogFieldError.from_node(callee, f"`{stringify(callee)}` passes positional arguments into its event, pass each as a keyword argument"))
        case ExpressionStmt(expr=CallExpr(callee=MemberExpr(name=name, expr=receiver) as callee)) if name in _BINDERS and _logger(receiver):
            errors.append(ErrorLostLogFieldError.from_node(node, f"`{stringify(callee)}` returns a new logger the statement discards, assign it and log through it"))
        case _:
            pass
