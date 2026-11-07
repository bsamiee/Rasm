"""`Result` errors typed as `str`, `Exception`, or `BaseException`, and `Option` or `Result` values iterated directly."""

lazy from collections import defaultdict, deque
lazy from collections.abc import Callable
lazy from functools import cache

from refurb.visitor import RefurbVisitor
lazy from expression import Option, Result
lazy from mypy.argmap import map_actuals_to_formals
lazy from mypy.erasetype import erase_typevars
lazy from mypy.maptype import map_instance_to_supertype
lazy from mypy.nodes import (
    ArgKind,
    AssignmentStmt,
    CallExpr,
    ComparisonExpr,
    Decorator,
    DictionaryComprehension,
    Expression,
    ForStmt,
    FuncDef,
    GeneratorExpr,
    ListExpr,
    MypyFile,
    Node,
    OverloadedFuncDef,
    StarExpr,
    SymbolNode,
    TupleExpr,
    TypeAlias,
    TypeInfo,
)
lazy from mypy.subtypes import is_subtype
lazy from mypy.types import AnyType, CallableType, flatten_nested_unions, get_proper_type, Instance, Overloaded, TupleType, Type, TypeAliasType, TypeOfAny, UnionType, UnpackType
lazy from refurb.checks.common import stringify
lazy from refurb.error import Error
lazy from refurb.settings import Settings

from rasm_refurb._findings import fullname, RasmError, Reads
from rasm_refurb._loops import Traverser

# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorMisusedExpressionError(RasmError):
    """`Result` error typed as `str`, `Exception`, or `BaseException` in place of the package fault, or an `Option` or `Result` iterated in place of a read through its members.

    Bad:

    ```
    def parsed(text: str) -> Result[int, str]: ...


    keywords = dict(zip(("icon_value",), value))
    ```

    Good:

    ```
    def parsed(text: str) -> Result[int, Fault]: ...


    keywords = dict(zip(("icon_value",), value.to_list()))
    ```
    """

    code = 101
    categories = ("expression",)


# --- [COMPOSITION] ----------------------------------------------------------------------


def configured(reads: Reads) -> Callable[[MypyFile, list[Error]], None]:
    """Returns a check that resolves iterated operands and callees through the build's expression types."""
    result_name = fullname(Result)
    wrapped = frozenset(map(fullname, (Option, Result)))
    rejected = frozenset(map(fullname, (str, Exception, BaseException)))

    @cache
    def faults(returned: Type) -> tuple[TypeInfo, ...]:
        match returned, get_proper_type(returned):
            case TypeAliasType(alias=TypeAlias(target=target)), resolved:
                owned = faults(get_proper_type(target))
                return tuple(info for info in faults(resolved) if info not in owned)
            case _, UnionType(items=items):
                return tuple(dict.fromkeys(*faults(item) for item in items))
            case _, Instance(type=owner, args=[_, error]) if owner.fullname == result_name:
                pending = deque((error,))
            case _:
                return ()
        seen: set[Type] = set()
        found: dict[TypeInfo, None] = {}
        while pending:
            if (current := pending.popleft()) in seen:
                continue
            seen.add(current)
            match get_proper_type(current):
                case TupleType(items=items) | UnionType(items=items):
                    pending.extend(items)
                case UnpackType(type=inner):
                    pending.append(inner)
                case Instance(type=TypeInfo(fullname="builtins.tuple"), args=arguments):
                    pending.extend(arguments)
                case Instance(type=info) if info.fullname in rejected:
                    found[info] = None
                case _:
                    pass
        return tuple(found)

    def declared(definition: SymbolNode | None) -> tuple[tuple[TypeAlias | FuncDef, Type], ...]:
        match definition:
            case TypeAlias(target=returned) as alias:
                return ((alias, returned),)
            case (FuncDef(type=CallableType(ret_type=returned)) as func) | Decorator(func=FuncDef(type=CallableType(ret_type=returned)) as func):
                match returned, func.is_coroutine, func.is_async_generator:
                    case Instance(args=[_, _, inner]), True, False:
                        return ((func, inner),)
                    case _:
                        return ((func, returned),)
            case _:
                return ()

    def held(found: Type | None) -> tuple[TypeInfo, ...]:
        return tuple(item.type for item in map(get_proper_type, flatten_nested_unions([found]) if found is not None else ()) if isinstance(item, Instance) and any(map(item.type.has_base, wrapped)))

    def iterating(info: TypeInfo) -> bool:
        return info.is_protocol and "__iter__" in info.protocol_members

    def element(found: Type | None) -> Type | None:
        match get_proper_type(found):
            case Instance(type=info) as instance:
                return next((map_instance_to_supertype(instance, base).args[0] for base in info.mro if iterating(base) and base.type_vars), None)
            case TupleType(partial_fallback=fallback):
                return element(fallback)
            case _:
                return None

    def consumed(call: CallExpr) -> tuple[tuple[Expression, Type | None], ...]:
        actuals = tuple(AnyType(TypeOfAny.special_form) if (found := reads.types.get(argument)) is None else found for argument in call.args)
        match get_proper_type(reads.types.get(call.callee)) if any(map(held, actuals)) else None:
            case Overloaded(items=items):
                signatures = items
            case CallableType() as signature:
                signatures = [signature]
            case _:
                signatures = []
        selected = next(
            (
                formals
                for signature in signatures
                for mapped in (map_actuals_to_formals(call.arg_kinds, call.arg_names, signature.arg_kinds, signature.arg_names, actuals.__getitem__),)
                for formals in (tuple((index, flatten_nested_unions([erase_typevars(formal)])) for indices, formal in zip(mapped, signature.arg_types) for index in indices),)
                if sorted(index for index, _ in formals) == list(range(len(actuals)))
                and all(indices or not kind.is_required() for indices, kind in zip(mapped, signature.arg_kinds))
                and all(any(is_subtype(actuals[index], member) for member in members) for index, members in formals)
            ),
            (),
        )
        return tuple(
            (call.args[index], actuals[index])
            for index, members in selected
            if all(isinstance(proper := get_proper_type(member), Instance) and iterating(proper.type) for member in members if is_subtype(actuals[index], member))
        )

    def unpacked(target: Expression, source: Expression | Type | None) -> tuple[tuple[Expression, Type | None], ...]:
        found = reads.types.get(source) if isinstance(source, Expression) else source
        match target:
            case TupleExpr(items=items) | ListExpr(items=items) if held(found):
                return ((source if isinstance(source, Expression) else target, found),)
            case TupleExpr(items=items) | ListExpr(items=items) if any(isinstance(item, StarExpr) for item in items):
                return ()
            case TupleExpr(items=items) | ListExpr(items=items):
                match source, get_proper_type(found):
                    case TupleExpr(items=values) | ListExpr(items=values), _ if len(values) == len(items):
                        return tuple(*unpacked(item, part) for item, part in zip(items, values))
                    case _, TupleType(items=parts) if len(parts) == len(items):
                        return tuple(*unpacked(item, part) for item, part in zip(items, parts))
                    case _:
                        return tuple(*unpacked(item, element(found)) for item in items)
            case _:
                return ()

    def iterated(node: Node) -> tuple[tuple[Expression, Type | None], ...]:
        match node:
            case StarExpr(expr=sequence):
                return ((sequence, reads.types.get(sequence)),)
            case ForStmt(index=target, expr=sequence, is_async=False):
                return ((sequence, found := reads.types.get(sequence)), *unpacked(target, element(found)))
            case GeneratorExpr(indices=targets, sequences=sequences, is_async=asynchronous) | DictionaryComprehension(indices=targets, sequences=sequences, is_async=asynchronous):
                return tuple(
                    *((sequence, found), *unpacked(target, element(found)))
                    for target, sequence, waited in zip(targets, sequences, asynchronous)
                    if not waited
                    for found in (reads.types.get(sequence),)
                )
            case AssignmentStmt(lvalues=targets, rvalue=value):
                return tuple(*unpacked(target, value) for target in targets)
            case ComparisonExpr(operators=operators, operands=[_, *rights]):
                return tuple((right, reads.types.get(right)) for operator, right in zip(operators, rights) if operator in {"in", "not in"})
            case CallExpr(args=arguments, arg_kinds=kinds):
                return (*((argument, reads.types.get(argument)) for argument, kind in zip(arguments, kinds) if kind is ArgKind.ARG_STAR), *consumed(node))
            case _:
                return ()

    class Walk(RefurbVisitor, Traverser):
        def __init__(self) -> None:
            super().__init__(defaultdict(list, {kind: [self.record] for kind in (ForStmt, GeneratorExpr, DictionaryComprehension, StarExpr, AssignmentStmt, ComparisonExpr, CallExpr)}), Settings())
            self.operands: dict[Expression, Type | None] = {}

        def record(self, node: Node, _errors: list[Error]) -> None:
            self.operands.update(iterated(node))

    def check(node: MypyFile, errors: list[Error]) -> None:
        walk = Walk()
        walk.accept(node)
        errors.extend((
            *(
                ErrorMisusedExpressionError.from_node(declaration, f"`{declaration.name}` declares `{untyped.name}` as `Result` error type, declare the package fault type")
                for _, symbol, _ in node.local_definitions()
                for definition in ((*symbol.node.items, symbol.node.impl) if isinstance(symbol.node, OverloadedFuncDef) else (symbol.node,))
                for declaration, returned in declared(definition)
                for untyped in faults(returned)
            ),
            *(
                ErrorMisusedExpressionError.from_node(
                    operand, f"`{stringify(operand)}` iterates `{info.name}`, which raises `EffectError` while it holds no value, read the value through its members or a pattern"
                )
                for operand, found in walk.operands.items()
                for info in held(found)[:1]
            ),
        ))

    return check


check = configured(Reads())

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("ErrorMisusedExpressionError", "check", "configured")
