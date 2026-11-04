"""Refurb replacements restricted to source forms preserving their results."""

from typing import override
lazy from collections import defaultdict
lazy from collections.abc import Callable
lazy from contextlib import nullcontext
lazy from io import BytesIO, StringIO
lazy from itertools import groupby

from refurb.checks.builtin import no_ignored_enumerate, no_slice_copy
from refurb.checks.readability import no_len_cmp
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
lazy from refurb.checks.builtin import writelines
lazy from refurb.checks.common import extract_binary_oper, get_mypy_type, is_same_type, stringify
lazy from refurb.checks.flow import no_with_assign
lazy from refurb.checks.iterable import implicit_readlines
lazy from refurb.checks.readability import no_copy_with_merge, no_or_default, no_temp_class_object, use_isinstance_bool
lazy from refurb.checks.secrets import simplify_token_function
lazy from refurb.checks.shlex import use_join
lazy from refurb.checks.string import no_multiline_lstrip, simplify_strip, startswith
lazy from refurb.checks.third_party.fastapi import simplify_query
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
        self._lengths = _Lengths(self.errors)
        self._defaults = default_check

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
        match check.__module__, node:
            case defaults.__name__, MypyFile():
                self._defaults(node, self.errors)
                return
            case startswith.__name__, OpExpr():
                match extract_binary_oper(node.op, node):
                    case (UnaryExpr(op="not", expr=first), UnaryExpr(op="not", expr=second)):
                        calls = first, second
                    case (first, second):
                        calls = first, second
                    case _:
                        return
                match calls:
                    case (
                        CallExpr(callee=MemberExpr(expr=receiver), args=[prefix], arg_kinds=[ArgKind.ARG_POS]),
                        CallExpr(callee=MemberExpr(expr=other), args=[suffix], arg_kinds=[ArgKind.ARG_POS]),
                    ) if (
                        _exact(receiver, str, bytes)
                        and all(map(_independent, (receiver, other, prefix)))
                        and (_primitive(suffix) or (isinstance(suffix, TupleExpr) and all(map(_primitive, suffix.items))))
                    ):
                        pass
                    case _:
                        return
            case no_len_cmp.__name__, AssertStmt() | ConditionalExpr() | DictionaryComprehension() | GeneratorExpr() | IfStmt() | MatchStmt() | WhileStmt():
                no_len_cmp.check_condition_like(self._lengths, node)
                return
            case no_with_assign.__name__, Block() | MypyFile():
                for allowed, statements in groupby(
                    node.body if isinstance(node, Block) else node.defs,
                    lambda statement: (
                        _independent(statement.rvalue)
                        if isinstance(statement, AssignmentStmt)
                        else (
                            not isinstance(statement, WithStmt)
                            or (
                                all(
                                    isinstance(manager, CallExpr)
                                    and isinstance(manager.callee, RefExpr)
                                    and manager.callee.fullname == fullname(nullcontext)
                                    and not any(map(ArgKind.is_star, manager.arg_kinds))
                                    and all(map(_independent, manager.args))
                                    for manager in statement.expr
                                )
                                and all(_inert(item) for item in statement.body.body)
                                and (owned := self.references.within.get(statement)) is not None
                                and all(count == 1 for count in owned.values())
                            )
                        )
                    ),
                ):
                    if allowed:
                        no_with_assign.check_stmts(list(statements), self.errors)
                return
            case writelines.__name__, WithStmt(
                expr=[manager],
                body=Block(body=[ForStmt(index=NameExpr(node=Var() as written, kind=kind), body=Block(body=[ExpressionStmt(expr=CallExpr(args=[argument], arg_kinds=[ArgKind.ARG_POS]))])) as loop]),
            ) if kind == LDEF and _primitive_sequence(loop.expr) and _independent(argument) and _exact(manager, StringIO, BytesIO):
                if self.references.counts[written] != self.references.within[loop][written]:
                    return
            case writelines.__name__, _:
                return
            case implicit_readlines.__name__, ForStmt(expr=CallExpr(callee=MemberExpr(expr=receiver, name="readlines"))):
                if not _exact(receiver, StringIO, BytesIO):
                    return
                if not all(_inert(statement) for statement in node.body.body):
                    return
            case implicit_readlines.__name__, _:
                return
            case no_or_default.__name__, OpExpr():
                match extract_binary_oper("or", node):
                    case (defaulted, _) if is_same_type(get_mypy_type(defaulted), bool):
                        pass
                    case (IntExpr(value=constant) | FloatExpr(value=constant) | ComplexExpr(value=constant) | StrExpr(value=constant) | BytesExpr(value=constant), _) if constant:
                        pass
                    case (UnaryExpr(op="+" | "-", expr=IntExpr(value=number) | FloatExpr(value=number) | ComplexExpr(value=number)), _) if number:
                        pass
                    case _:
                        return
            case no_ignored_enumerate.__name__, MypyFile():
                for loop in (owner for owner in self.references.within if isinstance(owner, ForStmt)):
                    self._enumerate(loop)
                return
            case no_ignored_enumerate.__name__, GeneratorExpr() | DictionaryComprehension():
                self._enumerate(node)
                return
            case no_ignored_enumerate.__name__, _:
                return
            case no_temp_class_object.__name__, CallExpr(callee=MemberExpr(name=method, expr=CallExpr(callee=NameExpr(node=TypeInfo() as owner), args=[]))):
                if (
                    (owner.metaclass_type is not None and owner.metaclass_type.type.fullname != fullname(type))
                    or owner.get("__del__") is not None
                    or any(isinstance(member, Decorator) and member.func.name == method and member.decorators for member in owner.defn.defs.body)
                    or not all(
                        (defined := owner.get_containing_type_info(member)) is not None and defined.fullname == fullname(object)
                        for member in (object.__init__.__name__, object.__new__.__name__, object.__getattribute__.__name__)
                    )
                ):
                    return
            case no_temp_class_object.__name__, _:
                return
            case no_slice_copy.__name__, MypyFile():
                _Copies(self.errors).accept(node)
                return
            case no_copy_with_merge.__name__, OpExpr(op="|", left=union_left, right=union_right):
                self.errors.extend(
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
                return
            case no_copy_with_merge.__name__, _:
                return
            case no_multiline_lstrip.__name__, CallExpr(callee=MemberExpr(expr=StrExpr(value=text), name="lstrip" | "rstrip" | "strip" as method), args=[] | [StrExpr(value="\n")] as passed):
                leading = text.startswith("\n") and len(text) > 1 and not text[1].isspace() and method != "rstrip"
                trailing = text.endswith("\n") and len(text) > 1 and not text[-2].isspace() and method != "lstrip"
                if _STRIP[method](text, "\n" if passed else None) != text[int(leading) : len(text) - int(trailing)]:
                    return
            case simplify_strip.__name__, CallExpr(
                callee=MemberExpr(expr=CallExpr(callee=MemberExpr(expr=receiver, name=first_method), args=[StrExpr(value=before)], arg_kinds=[ArgKind.ARG_POS]), name=second_method),
                args=[StrExpr(value=after)],
                arg_kinds=[ArgKind.ARG_POS],
            ) if _exact(receiver, str, bytes):
                before_chars, after_chars = frozenset(before), frozenset(after)
                if first_method == second_method and not (before_chars <= after_chars or after_chars <= before_chars):
                    return
            case simplify_strip.__name__, CallExpr(callee=MemberExpr(expr=CallExpr(callee=MemberExpr(expr=receiver)) as inner)):
                if not _exact(receiver, str, bytes) or any(kind is not ArgKind.ARG_POS for kind in (*inner.arg_kinds, *node.arg_kinds)):
                    return
            case simplify_token_function.__name__, IndexExpr():
                return
            case use_join.__name__, CallExpr(args=[(GeneratorExpr() as generator) | ListComprehension(generator=generator)]):
                match generator.left_expr, generator.condlists, generator.indices:
                    case CallExpr(args=[NameExpr(node=Var() as quoted)]), [[]], [NameExpr(node=Var() as index)] if quoted is index:
                        pass
                    case CallExpr(args=[NameExpr()]), [[]], _:
                        return
                    case _:
                        pass
            case use_isinstance_bool.__name__, ComparisonExpr(operands=[value, _]):
                if not is_same_type(get_mypy_type(value), bool):
                    return
            case use_isinstance_bool.__name__, OpExpr():
                match extract_binary_oper("or", node):
                    case (ComparisonExpr(operators=["is"], operands=[NameExpr(node=Var()), _]), ComparisonExpr(operators=["is"])):
                        pass
                    case (ComparisonExpr(operands=[NameExpr(node=Var()) as value, _]), _) if is_same_type(get_mypy_type(value), bool):
                        pass
                    case _:
                        return
            case simplify_query.__name__, FuncDef():
                if not (
                    arguments := [
                        argument
                        for argument in node.arguments
                        if isinstance(initializer := argument.initializer, CallExpr)
                        and isinstance(initializer.callee, NameExpr)
                        and initializer.callee.fullname == "fastapi.param_functions.Query"
                        and not any(map(ArgKind.is_star, initializer.arg_kinds))
                        and (annotation := argument.variable.type) is not None
                        and all(
                            isinstance(part := get_proper_type(item), NoneType | LiteralType)
                            or (isinstance(part, Instance) and (part.type.is_enum or any(part.type.has_base(owner) for owner in _SCALARS)))
                            for item in flatten_nested_unions([annotation], handle_recursive=False)
                        )
                    ]
                ):
                    return
                node = FuncDef(node.name, arguments, node.body)
            case _:
                pass
        super().run_check(node, check)

    def _enumerate(self, node: ForStmt | GeneratorExpr | DictionaryComprehension) -> None:
        match node:
            case ForStmt(index=target, expr=sequence) | GeneratorExpr(indices=[target], sequences=[sequence]) | DictionaryComprehension(indices=[target], sequences=[sequence]):
                pass
            case _:
                return
        match target, sequence:
            case TupleExpr(items=[NameExpr(node=Var() as index_var) as index, NameExpr(node=Var() as value_var) as value]), CallExpr(
                callee=NameExpr(fullname="builtins.enumerate"), args=[source], arg_kinds=[ArgKind.ARG_POS]
            ):
                if index.kind == LDEF and self.references.counts[index_var] == 1 and (not isinstance(node, ForStmt) or _primitive_sequence(source)):
                    self.errors.append(no_ignored_enumerate.ErrorInfo.from_node(index, f"Index is unused, use `for {stringify(value)} in {stringify(source)}` instead"))
                if value.kind == LDEF and self.references.counts[value_var] == 1 and _primitive_sequence(source):
                    self.errors.append(no_ignored_enumerate.ErrorInfo.from_node(value, f"Value is unused, use `for {stringify(index)} in range(len({stringify(source)}))` instead"))
            case _:
                pass


# --- [OPERATIONS] -----------------------------------------------------------------------


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


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("RewriteVisitor",)
