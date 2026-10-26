"""Attribute reads of one expression `Result` or `Option` case outside a `match` on the value."""

from typing import override, Protocol

from expression import Option, Result
from mypy.nodes import Expression, MatchStmt, MemberExpr, MypyFile, TupleExpr
from mypy.types import get_proper_type, Instance, NoneType, Type
from refurb.checks.common import get_mypy_type, is_equivalent, stringify
from refurb.error import Error
from refurb.visitor import TraverserVisitor

# --- [TYPES] ----------------------------------------------------------------------------


class _Named(Protocol):
    __name__: str


# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorPartialCaseReadError(Error):
    """Attribute read of one `Result` or `Option` case outside a `match` on the value.

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


# --- [OPERATIONS] -----------------------------------------------------------------------


def _listed(*readers: _Named) -> str:
    return f"{', '.join(f'`{reader.__name__}`' for reader in readers[:-1])}, or `{readers[-1].__name__}`"


# --- [TABLES] ---------------------------------------------------------------------------

_CASES = {
    f"{union.__module__}.{union.__qualname__}": {attribute: (case, readers[case]) for attribute, case in (*((field, field) for field in union.__match_args__ if field != "tag"), *properties)}
    for union, properties, readers in (
        (Result, (), {"ok": _listed(Result.map, Result.bind, Result.default_value), "error": _listed(Result.map_error, Result.or_else_with, Result.default_with)}),
        (Option, (("value", "some"),), {"some": _listed(Option.map, Option.bind, Option.default_value), "none": _listed(Option.default_value, Option.default_with, Option.or_else_with)}),
    )
}

# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: MypyFile, errors: list[Error]) -> None:
    """Reports each case read on a receiver no enclosing `match` takes as subject."""

    class Reads(TraverserVisitor):
        def __init__(self, subjects: tuple[Expression, ...]) -> None:
            super().__init__()
            self.subjects = subjects

        @override
        def visit_match_stmt(self, o: MatchStmt) -> None:
            self.accept(o.subject)
            arms = Reads((*self.subjects, *(o.subject.items if isinstance(o.subject, TupleExpr) else (o.subject,))))
            for part in (part for arm in zip(o.patterns, o.guards, o.bodies) for part in arm if part is not None):
                arms.accept(part)

        @override
        def visit_member_expr(self, o: MemberExpr) -> None:
            receiver = get_mypy_type(o.expr)
            found = get_proper_type(receiver) if isinstance(receiver, Type) else None
            if isinstance(found, Instance) and (read := _CASES.get(found.type.fullname, {}).get(o.name)) is not None and not any(is_equivalent(o.expr, subject) for subject in self.subjects):
                case, readers = read
                subject, field = stringify(o.expr), found.type.get(case)
                binding = "" if field is not None and isinstance(get_proper_type(field.type), NoneType) else f", {case}=<name>"
                errors.append(
                    ErrorPartialCaseReadError.from_node(
                        o, f'`{subject}.{o.name}` raises unless `{subject}` is the `{case}` case, match it with `case {found.type.name}(tag="{case}"{binding})` or read it through {readers}'
                    )
                )
            super().visit_member_expr(o)

    Reads(()).accept(node)
