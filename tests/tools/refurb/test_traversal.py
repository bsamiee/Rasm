"""Traversal every Rasm walk shares: operation trees deeper than Python's recursion limit in native preorder."""

from sys import getrecursionlimit
from typing import override

from mypy.nodes import NameExpr, OpExpr

from rasm.refurb import Traverser

# --- [SERVICES] -------------------------------------------------------------------------


class _Names(Traverser):
    """Names a traversal reaches, in visit order."""

    def __init__(self) -> None:
        """Holds no name yet."""
        super().__init__()
        self.reached: list[str] = []

    @override
    def visit_name_expr(self, o: NameExpr) -> None:
        self.reached.append(o.name)


# --- [OPERATIONS] -----------------------------------------------------------------------


def test_an_operation_tree_past_the_recursion_limit_is_walked_left_to_right() -> None:
    """A left-nested `+` chain twice the recursion limit deep, with a right-nested `|` pair at its root, reaches every operand once, left before right."""
    depth = getrecursionlimit() * 2
    tree = OpExpr("+", NameExpr("n0"), NameExpr("n1"))
    for index in range(2, depth):
        tree = OpExpr("+", tree, NameExpr(f"n{index}"))
    names = _Names()
    names.accept(OpExpr("|", tree, OpExpr("|", NameExpr("left"), NameExpr("right"))))
    assert names.reached == [*(f"n{index}" for index in range(depth)), "left", "right"]
