"""Traversal for the trees Rasm checks walk: an explicit worklist over operation trees in native preorder, and the template, type-form, and absent nodes refurb's traverser has no overload for, the absent key of a `{**row for row in rows}` comprehension among them."""

from types import NoneType
from typing import override

from mypy.nodes import Expression, OpExpr, TemplateStrExpr, TypeFormExpr
from refurb.visitor import TraverserVisitor
from refurb.visitor.traverser import accept

# --- [SERVICES] -------------------------------------------------------------------------


class Traverser(TraverserVisitor):
    """Native traversal with an explicit worklist for operation trees."""

    @override
    def visit_op_expr(self, o: OpExpr) -> None:
        pending: list[Expression] = [o]
        while pending:
            match pending.pop():
                case OpExpr(analyzed=analyzed, left=left, right=right):
                    pending.extend((*(() if analyzed is None else (analyzed,)), right, left))
                case expression:
                    self.accept(expression)


# --- [COMPOSITION] ----------------------------------------------------------------------


@accept.register
def _template(node: TemplateStrExpr, visitor: TraverserVisitor) -> None:
    for item in node.items:
        match item:
            case tuple():
                value, _, _, spec = item
                visitor.accept(value)
                if spec is not None:
                    visitor.accept(spec)
            case Expression():
                visitor.accept(item)


@accept.register
def _type_form(node: TypeFormExpr, visitor: TraverserVisitor) -> None:
    pass


@accept.register(NoneType)
def _absent(node: None, visitor: TraverserVisitor) -> None:
    pass


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Traverser"]
