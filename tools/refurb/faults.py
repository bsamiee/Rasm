"""`Result` errors typed as `str`, `Exception`, or `BaseException`."""

lazy from collections import deque
lazy from functools import cache

lazy from expression import Result
lazy from mypy.nodes import Decorator, FuncDef, MypyFile, OverloadedFuncDef, SymbolNode, TypeAlias, TypeInfo
lazy from mypy.types import CallableType, get_proper_type, Instance, TupleType, Type, TypeAliasType, UnionType, UnpackType
lazy from refurb.error import Error

from tools.refurb.findings import fullname, RasmError

# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorResultFaultError(RasmError):
    """`Result` error typed as `str`, `Exception`, or `BaseException` in place of the package fault.

    Bad:

    ```
    def parsed(text: str) -> Result[int, str]: ...
    ```

    Good:

    ```
    def parsed(text: str) -> Result[int, Fault]: ...
    ```
    """

    code = 101
    categories = ("expression",)


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: MypyFile, errors: list[Error]) -> None:
    """Reports each `Result` error typed as `str`, `Exception`, or `BaseException` on an alias, function, or method the module defines."""
    result_name = fullname(Result)
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

    errors.extend(
        ErrorResultFaultError.from_node(declaration, f"`{declaration.name}` declares `{untyped.name}` as `Result` error type, declare the package fault type")
        for _, symbol, _ in node.local_definitions()
        for definition in ((*symbol.node.items, symbol.node.impl) if isinstance(symbol.node, OverloadedFuncDef) else (symbol.node,))
        for declaration, returned in declared(definition)
        for untyped in faults(returned)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("ErrorResultFaultError", "check")
