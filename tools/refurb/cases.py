"""Partial case reads of expression `Result` and `Option` values outside a `match` on the value."""

from typing import override

from expression import Option, Result
from mypy.nodes import Expression, MatchStmt, MemberExpr, MypyFile, TupleExpr
from mypy.types import get_proper_type, Instance, Type
from refurb.checks.common import get_mypy_type, is_equivalent
from refurb.error import Error
from refurb.visitor import TraverserVisitor

# --- [CONSTANTS] ------------------------------------------------------------------------

CASES = {f"{union.__module__}.{union.__qualname__}": reads for union, reads in ((Result, frozenset({"ok", "error"})), (Option, frozenset({"some", "value"})))}

# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorPartialCaseReadError(Error):
    """Reading one case of a `Result` or `Option` raises on every other case.

    A `match` on the value binds each case in its pattern, a combinator (`map`, `bind`, `default_value`) reads it in one call:

    Bad:

    ```
    entry.row.value.record if entry.row.is_some() else fallback
    ```

    Good:

    ```
    entry.row.map(lambda row: row.record).default_value(fallback)
    ```
    """

    prefix = "RASM"
    code = 100
    name = "partial-case-read"
    categories = ("expression",)


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: MypyFile, errors: list[Error]) -> None:
    """Reports each case read whose receiver no enclosing `match` takes as its subject."""

    class Reads(TraverserVisitor):
        def __init__(self, subjects: tuple[Expression, ...]) -> None:
            super().__init__()
            self.subjects = subjects

        @override
        def visit_match_stmt(self, o: MatchStmt) -> None:
            self.accept(o.subject)
            arms = Reads((*self.subjects, *(o.subject.items if isinstance(o.subject, TupleExpr) else (o.subject,))))
            for part in (part for arm in zip(o.patterns, o.guards, o.bodies, strict=True) for part in arm if part is not None):
                arms.accept(part)

        @override
        def visit_member_expr(self, o: MemberExpr) -> None:
            receiver = get_mypy_type(o.expr)
            found = get_proper_type(receiver) if isinstance(receiver, Type) else None
            if isinstance(found, Instance) and o.name in CASES.get(found.type.fullname, frozenset()) and not any(is_equivalent(o.expr, subject) for subject in self.subjects):
                errors.append(ErrorPartialCaseReadError.from_node(o, f"`.{o.name}` reads one case of `{found.type.name}`, bind it in a `case` pattern or read it through a combinator"))
            super().visit_member_expr(o)

    Reads(()).accept(node)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ErrorPartialCaseReadError", "check"]
