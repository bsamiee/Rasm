"""Reference counts for bindings a proposed loop or assignment removal changes."""

from typing import override
lazy from collections import Counter
lazy from collections.abc import Callable, Collection, Iterable, Iterator

from refurb.visitor import TraverserVisitor
lazy from mypy.nodes import AssignmentStmt, Block, DictionaryComprehension, Expression, ForStmt, NameExpr, Node, OpExpr, TupleExpr, Var, WithStmt
lazy from refurb.error import Error

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


class _References(Traverser):
    """Module references and counts inside each construct that binds a tracked variable."""

    def __init__(self) -> None:
        """Creates empty reference counts for the next source module."""
        super().__init__()
        self.counts: Counter[Var] = Counter()
        self.within: dict[ForStmt | WithStmt, dict[Var, int]] = {}

    def check(self, node: Node, _errors: list[Error]) -> None:
        """Counts each source module before its diagnostic callbacks run."""
        self.counts.clear()
        self.within.clear()
        self.accept(node)

    @override
    def visit_name_expr(self, o: NameExpr) -> None:
        if isinstance(o.node, Var):
            self.counts[o.node] += 1

    @override
    def visit_for_stmt(self, o: ForStmt) -> None:
        indices = o.index.items if isinstance(o.index, TupleExpr) else [o.index]
        self._observed(o, (index.node for index in indices if isinstance(index, NameExpr) and isinstance(index.node, Var)), super().visit_for_stmt)

    @override
    def visit_with_stmt(self, o: WithStmt) -> None:
        match o.body:
            case Block(body=[AssignmentStmt(lvalues=[NameExpr(node=Var() as variable)])]):
                self._observed(o, (variable,), super().visit_with_stmt)
            case _:
                super().visit_with_stmt(o)

    def _observed[T: ForStmt | WithStmt](self, node: T, variables: Iterable[Var], visit: Callable[[T], None]) -> None:
        before = {variable: self.counts[variable] for variable in variables}
        visit(node)
        self.within[node] = {variable: self.counts[variable] - count for variable, count in before.items()}


# --- [OPERATIONS] -----------------------------------------------------------------------


def _operations(root: OpExpr, operators: Collection[str] | None = None) -> Iterator[Expression]:
    """Yields operation trees in native preorder, descending through selected operators."""
    pending: list[Expression] = [root]
    while pending:
        expression = pending.pop()
        yield expression
        if isinstance(expression, OpExpr) and (operators is None or expression.op in operators):
            if expression.analyzed is not None:
                pending.append(expression.analyzed)
            pending.extend((expression.right, expression.left))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Traverser"]
