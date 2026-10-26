"""`Result` errors typed as `str`, `Exception`, or `BaseException`."""

lazy from expression import Result
lazy from mypy.nodes import Decorator, FuncDef, MypyFile, SymbolNode, TypeAlias, TypeInfo
lazy from mypy.types import CallableType, get_proper_type, Instance, ProperType, TupleType, Type, TypeAliasType
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

    def element(error: Type) -> ProperType:
        match get_proper_type(error):
            case TupleType(items=[first, *_]) | Instance(type=TypeInfo(fullname="builtins.tuple"), args=[first]):
                return get_proper_type(first)
            case other:
                return other

    def declared(definition: SymbolNode | None) -> tuple[tuple[TypeAlias | FuncDef, Type], ...]:
        match definition:
            case TypeAlias(target=returned) as alias:
                return ((alias, returned),)
            case (FuncDef(type=CallableType(ret_type=returned)) as func) | Decorator(func=FuncDef(type=CallableType(ret_type=returned)) as func) if not isinstance(returned, TypeAliasType):
                return ((func, returned),)
            case _:
                return ()

    errors.extend(
        ErrorResultFaultError.from_node(declaration, f"`{declaration.name}` declares `{untyped.type.name}` as `Result` error type, declare the package fault type")
        for _, symbol, _ in node.local_definitions()
        for declaration, returned in declared(symbol.node)
        if isinstance(result := get_proper_type(returned), Instance)
        and result.type.fullname == fullname(Result)
        and isinstance(untyped := element(result.args[-1]), Instance)
        and untyped.type.fullname in map(fullname, (str, Exception, BaseException))
    )
