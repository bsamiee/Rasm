"""Runtime subscripts of classes that gained `__class_getitem__` in Python 3.14, which raise `TypeError` below it."""

import ctypes
import sqlite3
import types

from mypy.nodes import AssignmentStmt, ClassDef, IndexExpr, RefExpr
from refurb.error import Error
from refurb.settings import Settings

from rasm.refurb._findings import fullname, RasmError

# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorUnsubscriptableBuiltinError(RasmError):
    """Runtime subscript of a class with no `__class_getitem__` before Python 3.14, which raises `TypeError` when evaluated.

    Bad:

    ```
    View = memoryview[int]
    ```

    Good:

    ```
    type View = memoryview[int]
    ```
    """

    code = 109
    categories = ("typing",)


# --- [CONSTANTS] ------------------------------------------------------------------------

_UNSUBSCRIPTABLE = frozenset(map(fullname, (classmethod, memoryview, slice, staticmethod, types.UnionType, ctypes.py_object, sqlite3.Binary)))

# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: AssignmentStmt | ClassDef, errors: list[Error], settings: Settings) -> None:
    """Reports each assigned value or base class subscripting a class lacking `__class_getitem__` when refurb runs Python 3.13 or earlier."""
    errors.extend(
        ErrorUnsubscriptableBuiltinError.from_node(evaluated, f"`{name}` has no `__class_getitem__` before Python 3.14, declare a `type` alias or annotate it unsubscripted")
        for evaluated in ([node.rvalue] if isinstance(node, AssignmentStmt) else node.base_type_exprs)
        if isinstance(evaluated, IndexExpr) and isinstance(evaluated.base, RefExpr) and (name := evaluated.base.fullname) in _UNSUBSCRIPTABLE and settings.get_python_version() < (3, 14)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("ErrorUnsubscriptableBuiltinError", "check")
