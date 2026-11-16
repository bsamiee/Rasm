"""Iterables flattened through `itertools.chain` argument unpacking, or through `chain` and nested comprehensions where Python 3.15 comprehension unpacking states it."""

from itertools import chain

from mypy.nodes import ArgKind, CallExpr, DictionaryComprehension, GeneratorExpr, MemberExpr, NameExpr, RefExpr, TupleExpr
from refurb.error import Error
from refurb.settings import Settings

from rasm.refurb._findings import fullname, RasmError

# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorChainFlatteningError(RasmError):
    """Iterables unpacked into `itertools.chain` arguments, which reads the outer iterable eagerly, and on Python 3.15 iterables flattened through `chain` or a nested comprehension, where comprehension unpacking states the flattening.

    Bad:

    ```
    flat = list(chain.from_iterable(rows))
    merged = {key: value for row in rows for key, value in row.items()}
    ```

    Good:

    ```
    flat = [*row for row in rows]
    merged = {**row for row in rows}
    ```
    """

    code = 107
    categories = ("iterable",)


# --- [CONSTANTS] ------------------------------------------------------------------------

_CHAIN = fullname(chain)

# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: CallExpr | GeneratorExpr | DictionaryComprehension, errors: list[Error], settings: Settings) -> None:
    """Reports each `chain(*rows)` call, and on Python 3.15 or later each `chain.from_iterable` call and two-level comprehension yielding the inner item unchanged."""
    unpacking = settings.get_python_version() >= (3, 15)
    match node:
        case CallExpr(callee=RefExpr(fullname=name), arg_kinds=[ArgKind.ARG_STAR]) if name == _CHAIN and not unpacking:
            errors.append(ErrorChainFlatteningError.from_node(node, "Call `chain.from_iterable(<rows>)`, which reads the outer iterable lazily"))
        case _ if not unpacking:
            pass
        case CallExpr(callee=MemberExpr(name="from_iterable", expr=RefExpr(fullname=name))) | CallExpr(callee=RefExpr(fullname=name), arg_kinds=[ArgKind.ARG_STAR]) if name == _CHAIN:
            errors.append(ErrorChainFlatteningError.from_node(node, "Flatten through comprehension unpacking `[*row for row in rows]`"))
        case GeneratorExpr(left_expr=NameExpr(name=item), indices=[NameExpr(name=row), NameExpr(name=inner)], sequences=[_, NameExpr(name=source)], condlists=[[], []], is_async=[False, False]) if (
            item == inner and source == row
        ):
            errors.append(ErrorChainFlatteningError.from_node(node, "Flatten through comprehension unpacking `[*row for row in rows]`"))
        case DictionaryComprehension(
            key=NameExpr(name=key),
            value=NameExpr(name=value),
            indices=[NameExpr(name=row), TupleExpr(items=[NameExpr(name=inner_key), NameExpr(name=inner_value)])],
            sequences=[_, CallExpr(callee=MemberExpr(name="items", expr=NameExpr(name=source)), args=[])],
            condlists=[[], []],
            is_async=[False, False],
        ) if (key, value) == (inner_key, inner_value) and source == row:
            errors.append(ErrorChainFlatteningError.from_node(node, "Merge through dictionary unpacking `{**row for row in rows}`"))
        case _:
            pass


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("ErrorChainFlatteningError", "check")
