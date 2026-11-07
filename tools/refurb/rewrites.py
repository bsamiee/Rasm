"""Refurb replacements restricted to source forms preserving their results."""

from typing import override
lazy from collections import defaultdict
lazy from collections.abc import Callable
lazy from contextlib import nullcontext
lazy from io import BytesIO, StringIO
lazy from itertools import groupby

from refurb.checks.builtin import no_ignored_enumerate, no_slice_copy, writelines
from refurb.checks.flow import no_with_assign
from refurb.checks.iterable import implicit_readlines
from refurb.checks.readability import no_copy_with_merge, no_len_cmp, no_or_default, no_temp_class_object, use_isinstance_bool
from refurb.checks.secrets import simplify_token_function
from refurb.checks.shlex import use_join
from refurb.checks.string import no_multiline_lstrip, simplify_strip, startswith
from refurb.checks.third_party.fastapi import simplify_query
from refurb.visitor import RefurbVisitor
lazy from mypy.nodes import (
    ArgKind,
    AssertStmt,
    AssignmentStmt,
    Block,
    BytesExpr,
    CallExpr,
    ComparisonExpr,
    ComplexExpr,
    ConditionalExpr,
    Decorator,
    DictExpr,
    DictionaryComprehension,
    Expression,
    ExpressionStmt,
    FloatExpr,
    ForStmt,
    FuncDef,
    GeneratorExpr,
    IfStmt,
    IndexExpr,
    IntExpr,
    LDEF,
    ListComprehension,
    ListExpr,
    MatchStmt,
    MemberExpr,
    MypyFile,
    NameExpr,
    Node,
    OpExpr,
    PassStmt,
    RefExpr,
    SetComprehension,
    SetExpr,
    SliceExpr,
    Statement,
    StrExpr,
    TupleExpr,
    TypeApplication,
    TypeInfo,
    UnaryExpr,
    Var,
    WhileStmt,
    WithStmt,
)
lazy from mypy.types import flatten_nested_unions, get_proper_type, Instance, LiteralType, NoneType, Type
lazy from refurb.checks.common import extract_binary_oper, get_mypy_type, is_same_type, stringify
lazy from refurb.error import Error
lazy from refurb.settings import Settings
lazy from refurb.types import Check, Checks

from tools.refurb import defaults
from tools.refurb.findings import Declared, fullname
from tools.refurb.loops import operations, References, Traverser

# --- [CONSTANTS] ------------------------------------------------------------------------

_SCALARS = tuple(map(fullname, (str, bytes, int, float, complex)))
_SIZED = (str, bytes, bytearray, list, tuple, dict, set, frozenset, frozendict)
_STRIP = frozendict({member.__name__: member for member in (str.lstrip, str.rstrip, str.strip)})

# --- [SERVICES] -------------------------------------------------------------------------


class _Copies(no_slice_copy.SliceExprVisitor, Traverser):
    @override
    def visit_index_expr(self, node: IndexExpr) -> None:
        match node.index:
            case SliceExpr(begin_index=None, end_index=None, stride=None) if _exact(node.base, bytearray, list):
                receiver = stringify(node.base)
                self.errors.append(no_slice_copy.ErrorInfo.from_node(node, f"Replace `{receiver}[:]` with `{receiver}.copy()`"))
            case _:
                pass


class _Lengths(no_len_cmp.LenComparisonVisitor):
    @override
    def visit_op_expr(self, o: OpExpr) -> None:
        for expression in operations(o, ("and", "or")):
            if not isinstance(expression, OpExpr):
                self.accept(expression)

    @override
    def visit_call_expr(self, node: CallExpr) -> None:
        if _length_truth(node):
            super().visit_call_expr(node)

    @override
    def visit_comparison_expr(self, node: ComparisonExpr) -> None:
        match node.operands:
            case [CallExpr(callee=NameExpr(fullname="builtins.len")) as call, _]:
                allowed = _length_truth(call)
            case [receiver, _]:
                allowed = _exact(receiver, *_SIZED)
            case _:
                allowed = False
        if allowed:
            super().visit_comparison_expr(node)


class RewriteVisitor(RefurbVisitor, Traverser):
    """Native check execution retaining replacements proved by source values and bindings."""

    def __init__(self, checks: Checks, settings: Settings, *, default_check: Callable[[MypyFile, list[Error]], None] = defaults.check) -> None:
        """Registers one source reference pass before diagnostic traversal."""
        self.references = References()
        registered = defaultdict(list, checks)
        registered[MypyFile] = [self.references.check, *registered[MypyFile]]
        self._operations = registered.pop(OpExpr, [])
        self._comprehensions = registered.pop(DictionaryComprehension, [])
        super().__init__(registered, settings)
        self.lengths = _Lengths(self.errors)
        self.default_check = default_check

    @override
    def visit_op_expr(self, o: OpExpr) -> None:
        for expression in operations(o):
            if isinstance(expression, OpExpr):
                for check in self._operations:
                    self.run_check(expression, check)
            else:
                self.accept(expression)

    @override
    def visit_dictionary_comprehension(self, o: DictionaryComprehension) -> None:
        for check in self._comprehensions:
            self.run_check(o, check)
        super().visit_dictionary_comprehension(o)

    @override
    def run_check(self, node: Node, check: Check) -> None:
        if (forwarded := node if (run := _RUNS.get(check.__module__)) is None else run(self, node)) is not None:
            super().run_check(forwarded, check)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _defaults(visitor: RewriteVisitor, node: Node, /) -> Node | None:
    match node:
        case MypyFile():
            visitor.default_check(node, visitor.errors)
            return None
        case _:
            return node


def _startswith(_: RewriteVisitor, node: Node, /) -> Node | None:
    if not isinstance(node, OpExpr):
        return node
    match extract_binary_oper(node.op, node):
        case (UnaryExpr(op="not", expr=first), UnaryExpr(op="not", expr=second)) | (first, second):
            operands: tuple[Expression, ...] = (first, second)
        case _:
            operands = ()
    match operands:
        case (CallExpr(callee=MemberExpr(expr=receiver), args=[prefix], arg_kinds=[ArgKind.ARG_POS]), CallExpr(callee=MemberExpr(expr=other), args=[suffix], arg_kinds=[ArgKind.ARG_POS])) if (
            _exact(receiver, str, bytes) and all(map(_independent, (receiver, other, prefix))) and (_primitive(suffix) or (isinstance(suffix, TupleExpr) and all(map(_primitive, suffix.items))))
        ):
            return node
        case _:
            return None


def _length_conditions(visitor: RewriteVisitor, node: Node, /) -> Node | None:
    match node:
        case AssertStmt() | ConditionalExpr() | DictionaryComprehension() | GeneratorExpr() | IfStmt() | MatchStmt() | WhileStmt():
            no_len_cmp.check_condition_like(visitor.lengths, node)
            return None
        case _:
            return node


def _with_assign(visitor: RewriteVisitor, node: Node, /) -> Node | None:
    def allowed(statement: Statement) -> bool:
        match statement:
            case AssignmentStmt(rvalue=value):
                return _independent(value)
            case WithStmt(expr=managers, body=Block(body=body)):
                return (
                    all(
                        isinstance(manager, CallExpr)
                        and isinstance(manager.callee, RefExpr)
                        and manager.callee.fullname == fullname(nullcontext)
                        and not any(map(ArgKind.is_star, manager.arg_kinds))
                        and all(map(_independent, manager.args))
                        for manager in managers
                    )
                    and all(map(_inert, body))
                    and (owned := visitor.references.within.get(statement)) is not None
                    and all(count == 1 for count in owned.values())
                )
            case _:
                return True

    match node:
        case Block(body=statements) | MypyFile(defs=statements):
            for grouped in (list(run) for kept, run in groupby(statements, allowed) if kept):
                no_with_assign.check_stmts(grouped, visitor.errors)
            return None
        case _:
            return node


def _writelines(visitor: RewriteVisitor, node: Node, /) -> Node | None:
    match node:
        case WithStmt(
            expr=[manager],
            body=Block(body=[ForStmt(index=NameExpr(node=Var() as written, kind=kind), body=Block(body=[ExpressionStmt(expr=CallExpr(args=[argument], arg_kinds=[ArgKind.ARG_POS]))])) as loop]),
        ) if (
            kind == LDEF
            and _primitive_sequence(loop.expr)
            and _independent(argument)
            and _exact(manager, StringIO, BytesIO)
            and visitor.references.counts[written] == visitor.references.within[loop][written]
        ):
            return node
        case _:
            return None


def _readlines(_: RewriteVisitor, node: Node, /) -> Node | None:
    match node:
        case ForStmt(expr=CallExpr(callee=MemberExpr(expr=receiver, name="readlines")), body=Block(body=body)) if _exact(receiver, StringIO, BytesIO) and all(map(_inert, body)):
            return node
        case _:
            return None


def _or_default(_: RewriteVisitor, node: Node, /) -> Node | None:
    match node, (extract_binary_oper("or", node) if isinstance(node, OpExpr) else None):
        case OpExpr(), (defaulted, _) if is_same_type(get_mypy_type(defaulted), bool):
            return node
        case OpExpr(), (IntExpr(value=constant) | FloatExpr(value=constant) | ComplexExpr(value=constant) | StrExpr(value=constant) | BytesExpr(value=constant), _) if constant:
            return node
        case OpExpr(), (UnaryExpr(op="+" | "-", expr=IntExpr(value=number) | FloatExpr(value=number) | ComplexExpr(value=number)), _) if number:
            return node
        case OpExpr(), _:
            return None
        case _:
            return node


def _ignored_enumerate(visitor: RewriteVisitor, node: Node, /) -> Node | None:
    match node:
        case MypyFile():
            for loop in (owner for owner in visitor.references.within if isinstance(owner, ForStmt)):
                _enumerate(visitor, loop)
        case GeneratorExpr() | DictionaryComprehension():
            _enumerate(visitor, node)
        case _:
            pass
    return None


def _enumerate(visitor: RewriteVisitor, node: ForStmt | GeneratorExpr | DictionaryComprehension, /) -> None:
    match node:
        case ForStmt(index=target, expr=sequence) | GeneratorExpr(indices=[target], sequences=[sequence]) | DictionaryComprehension(indices=[target], sequences=[sequence]):
            pass
        case _:
            return
    match target, sequence:
        case TupleExpr(items=[NameExpr(node=Var() as index_var) as index, NameExpr(node=Var() as value_var) as value]), CallExpr(
            callee=NameExpr(fullname="builtins.enumerate"), args=[source], arg_kinds=[ArgKind.ARG_POS]
        ):
            if index.kind == LDEF and visitor.references.counts[index_var] == 1 and (not isinstance(node, ForStmt) or _primitive_sequence(source)):
                visitor.errors.append(no_ignored_enumerate.ErrorInfo.from_node(index, f"Index is unused, use `for {stringify(value)} in {stringify(source)}` instead"))
            if value.kind == LDEF and visitor.references.counts[value_var] == 1 and _primitive_sequence(source):
                visitor.errors.append(no_ignored_enumerate.ErrorInfo.from_node(value, f"Value is unused, use `for {stringify(index)} in range(len({stringify(source)}))` instead"))
        case _:
            pass


def _temporary_object(_: RewriteVisitor, node: Node, /) -> Node | None:
    match node:
        case CallExpr(callee=MemberExpr(name=method, expr=CallExpr(callee=NameExpr(node=TypeInfo() as owner), args=[]))) if not (
            (owner.metaclass_type is not None and owner.metaclass_type.type.fullname != fullname(type))
            or owner.get("__del__") is not None
            or any(isinstance(member, Decorator) and member.func.name == method and member.decorators for member in owner.defn.defs.body)
            or not all(
                (defined := owner.get_containing_type_info(member)) is not None and defined.fullname == fullname(object)
                for member in (object.__init__.__name__, object.__new__.__name__, object.__getattribute__.__name__)
            )
        ):
            return node
        case _:
            return None


def _slice_copies(visitor: RewriteVisitor, node: Node, /) -> Node | None:
    match node:
        case MypyFile():
            _Copies(visitor.errors).accept(node)
            return None
        case _:
            return node


def _merged_copies(visitor: RewriteVisitor, node: Node, /) -> Node | None:
    match node:
        case OpExpr(op="|", left=union_left, right=union_right):
            visitor.errors.extend(
                no_copy_with_merge.ErrorInfo.from_node(operand, f"Replace `{rendered}.copy()` with `{rendered}`")
                for operand in (union_left, union_right)
                if isinstance(operand, CallExpr)
                and isinstance(operand.callee, MemberExpr)
                and operand.callee.name == "copy"
                and not operand.args
                and _exact(merged_receiver := operand.callee.expr, dict, set)
                and (operand is union_right or _independent(union_right))
                for rendered in (stringify(merged_receiver),)
            )
        case _:
            pass
    return None


def _multiline_strip(_: RewriteVisitor, node: Node, /) -> Node | None:
    match node:
        case CallExpr(callee=MemberExpr(expr=StrExpr(value=text), name="lstrip" | "rstrip" | "strip" as method), args=[] | [StrExpr(value="\n")] as passed):
            leading = text.startswith("\n") and len(text) > 1 and not text[1].isspace() and method != "rstrip"
            trailing = text.endswith("\n") and len(text) > 1 and not text[-2].isspace() and method != "lstrip"
            return node if _STRIP[method](text, "\n" if passed else None) == text[int(leading) : len(text) - int(trailing)] else None
        case _:
            return node


def _chained_strip(_: RewriteVisitor, node: Node, /) -> Node | None:
    match node:
        case CallExpr(
            callee=MemberExpr(expr=CallExpr(callee=MemberExpr(expr=receiver, name=first_method), args=[StrExpr(value=before)], arg_kinds=[ArgKind.ARG_POS]), name=second_method),
            args=[StrExpr(value=after)],
            arg_kinds=[ArgKind.ARG_POS],
        ) if _exact(receiver, str, bytes):
            before_chars, after_chars = frozenset(before), frozenset(after)
            return None if first_method == second_method and not (before_chars <= after_chars or after_chars <= before_chars) else node
        case CallExpr(callee=MemberExpr(expr=CallExpr(callee=MemberExpr(expr=receiver)) as inner)):
            return node if _exact(receiver, str, bytes) and all(kind is ArgKind.ARG_POS for kind in (*inner.arg_kinds, *node.arg_kinds)) else None
        case _:
            return node


def _token_function(_: RewriteVisitor, node: Node, /) -> Node | None:
    match node:
        case IndexExpr():
            return None
        case _:
            return node


def _join(_: RewriteVisitor, node: Node, /) -> Node | None:
    match node:
        case CallExpr(args=[(GeneratorExpr() as generator) | ListComprehension(generator=generator)]):
            shape = generator.left_expr, generator.condlists, generator.indices
        case _:
            return node
    match shape:
        case CallExpr(args=[NameExpr(node=Var() as quoted)]), [[]], [NameExpr(node=Var() as index)] if quoted is index:
            return node
        case CallExpr(args=[NameExpr()]), [[]], _:
            return None
        case _:
            return node


def _isinstance_bool(_: RewriteVisitor, node: Node, /) -> Node | None:
    match node, (extract_binary_oper("or", node) if isinstance(node, OpExpr) else None):
        case ComparisonExpr(operands=[value, _]), _:
            return node if is_same_type(get_mypy_type(value), bool) else None
        case OpExpr(), (ComparisonExpr(operators=["is"], operands=[NameExpr(node=Var()), _]), ComparisonExpr(operators=["is"])):
            return node
        case OpExpr(), (ComparisonExpr(operands=[NameExpr(node=Var()) as value, _]), _) if is_same_type(get_mypy_type(value), bool):
            return node
        case OpExpr(), _:
            return None
        case _:
            return node


def _query(_: RewriteVisitor, node: Node, /) -> Node | None:
    match node:
        case FuncDef():
            arguments = [
                argument
                for argument in node.arguments
                if isinstance(initializer := argument.initializer, CallExpr)
                and isinstance(initializer.callee, NameExpr)
                and initializer.callee.fullname == "fastapi.param_functions.Query"
                and not any(map(ArgKind.is_star, initializer.arg_kinds))
                and (annotation := argument.variable.type) is not None
                and all(
                    isinstance(part := get_proper_type(item), NoneType | LiteralType) or (isinstance(part, Instance) and (part.type.is_enum or any(part.type.has_base(owner) for owner in _SCALARS)))
                    for item in flatten_nested_unions([annotation], handle_recursive=False)
                )
            ]
            return FuncDef(node.name, arguments, node.body) if arguments else None
        case _:
            return node


def _exact(expression: Expression, *owners: Declared) -> bool:
    match expression:
        case StrExpr():
            return str in owners
        case BytesExpr():
            return bytes in owners
        case ListExpr() | ListComprehension():
            return list in owners
        case TupleExpr():
            return tuple in owners
        case DictExpr() | DictionaryComprehension():
            return dict in owners
        case SetExpr() | SetComprehension():
            return set in owners
        case CallExpr(callee=RefExpr(node=TypeInfo(fullname=name)) | IndexExpr(analyzed=TypeApplication(expr=RefExpr(node=TypeInfo(fullname=name))))):
            return name in map(fullname, owners) and (name not in map(fullname, (str, bytes)) or not expression.args)
        case _:
            found = get_mypy_type(expression)
            proper = get_proper_type(found) if isinstance(found, Type) else None
            match proper.last_known_value if isinstance(proper, Instance) and proper.last_known_value is not None else proper:
                case LiteralType(fallback=Instance(type=info)) | Instance(type=TypeInfo(is_final=True) as info):
                    return info.fullname in map(fullname, owners)
                case _:
                    return False


def _independent(expression: Expression) -> bool:
    pending = [expression]
    while pending:
        match item := pending.pop():
            case NameExpr(node=Var(), kind=kind) if kind == LDEF:
                pass
            case ListExpr(items=items) | TupleExpr(items=items):
                pending.extend(items)
            case DictExpr(items=entries) if all(key is not None and _primitive(key) for key, _ in entries):
                pending.extend(value for _, value in entries)
            case SetExpr(items=members) if all(map(_primitive, members)):
                pass
            case _:
                if not _primitive(item):
                    return False
    return True


def _primitive(expression: Expression) -> bool:
    match expression:
        case IntExpr() | FloatExpr() | ComplexExpr() | StrExpr() | BytesExpr() | NameExpr(fullname="builtins.True" | "builtins.False" | "builtins.None"):
            return True
        case UnaryExpr(op="+" | "-", expr=IntExpr() | FloatExpr() | ComplexExpr()):
            return True
        case _:
            return False


def _primitive_sequence(expression: Expression) -> bool:
    match expression:
        case ListExpr(items=items) | TupleExpr(items=items) | SetExpr(items=items):
            return all(map(_primitive, items))
        case CallExpr(args=[]):
            return _exact(expression, str, bytes, list, tuple)
        case _:
            return _exact(expression, str, bytes)


def _length_truth(call: CallExpr) -> bool:
    match call:
        case CallExpr(callee=NameExpr(fullname="builtins.len"), args=[CallExpr(callee=NameExpr(fullname="builtins.list"), args=[_])]):
            return False
        case CallExpr(callee=NameExpr(fullname="builtins.len"), args=[receiver], arg_kinds=[ArgKind.ARG_POS]):
            return _exact(receiver, *_SIZED)
        case _:
            return False


def _inert(statement: Statement) -> bool:
    match statement:
        case PassStmt():
            return True
        case AssignmentStmt(lvalues=[NameExpr(node=Var())], rvalue=value) | ExpressionStmt(expr=value):
            return _independent(value)
        case _:
            return False


# --- [TABLES] ---------------------------------------------------------------------------

_RUNS: frozendict[str, Callable[[RewriteVisitor, Node], Node | None]] = frozendict({
    defaults.__name__: _defaults,
    startswith.__name__: _startswith,
    no_len_cmp.__name__: _length_conditions,
    no_with_assign.__name__: _with_assign,
    writelines.__name__: _writelines,
    implicit_readlines.__name__: _readlines,
    no_or_default.__name__: _or_default,
    no_ignored_enumerate.__name__: _ignored_enumerate,
    no_temp_class_object.__name__: _temporary_object,
    no_slice_copy.__name__: _slice_copies,
    no_copy_with_merge.__name__: _merged_copies,
    no_multiline_lstrip.__name__: _multiline_strip,
    simplify_strip.__name__: _chained_strip,
    simplify_token_function.__name__: _token_function,
    use_join.__name__: _join,
    use_isinstance_bool.__name__: _isinstance_bool,
    simplify_query.__name__: _query,
})

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("RewriteVisitor",)
