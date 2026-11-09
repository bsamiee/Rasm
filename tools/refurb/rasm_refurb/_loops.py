"""Traversal for the trees Rasm checks walk: an explicit worklist over operation trees, and the template and type-form nodes refurb's traverser has no overload for."""

from typing import override
lazy from collections.abc import Iterator

from mypy.nodes import Expression, TemplateStrExpr, TypeFormExpr
from refurb.visitor import TraverserVisitor
from refurb.visitor.traverser import accept
lazy from mypy.nodes import DictionaryComprehension, OpExpr

# --- [SERVICES] -------------------------------------------------------------------------


class Traverser(TraverserVisitor):
    """Native traversal with an explicit worklist for operation trees."""

    @override
    def visit_op_expr(self, o: OpExpr) -> None:
        for expression in _operations(o):
            if not isinstance(expression, OpExpr):
                self.accept(expression)

    @override
    def visit_dictionary_comprehension(self, o: DictionaryComprehension) -> None:
        for index, sequence, conditions in zip(o.indices, o.sequences, o.condlists):
            self.accept(sequence)
            self.accept(index)
            for condition in conditions:
                self.accept(condition)
        if o.key is not None:
            self.accept(o.key)
        self.accept(o.value)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _operations(root: OpExpr) -> Iterator[Expression]:
    """Yields an operation tree in native preorder."""
    pending: list[Expression] = [root]
    while pending:
        expression = pending.pop()
        yield expression
        if isinstance(expression, OpExpr):
            if expression.analyzed is not None:
                pending.append(expression.analyzed)
            pending.extend((expression.right, expression.left))


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


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Traverser"]
