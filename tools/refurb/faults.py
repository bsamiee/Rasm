"""`Result` errors typed as `str` or as faults of another first-party package."""

from expression import Result
from mypy.nodes import Block, ClassDef, Decorator, FuncDef, MypyFile, Statement, TypeAlias, TypeInfo
from mypy.types import CallableType, get_proper_type, Instance, TupleType, Type, TypeAliasType, UnionType, UnpackType
from refurb.error import Error

# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorResultFaultError(Error):
    """`Result` error typed as `str` or as faults of another first-party package.

    Bad:

    ```
    def graded(curve: Curve) -> ImagingResult[Curve]:
        return imaged(curve)


    def parsed(text: str) -> Result[int, str]: ...
    ```

    Good:

    ```
    def graded(curve: Curve) -> RenderingResult[Curve]:
        return imaged(curve).map_error(RenderingImagingFault.lifted)


    def parsed(text: str) -> Result[int, ParseFaults]: ...
    ```
    """

    prefix = "RASM"
    code = 101
    name = "result-fault"
    categories = ("expression",)


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: MypyFile, errors: list[Error]) -> None:
    """Reports each module-level function and method returning a `Result` with a `str` or foreign error."""
    root, package = node.fullname.partition(".")[0], node.fullname if node.is_package_init_file() else node.fullname.rpartition(".")[0] or node.fullname

    def defined(statement: Statement) -> tuple[FuncDef, ...]:
        match statement:
            case FuncDef():
                return (statement,)
            case Decorator(func=func):
                return (func,)
            case ClassDef(defs=Block(body=body)):
                return tuple(func for inner in body for func in defined(inner))
            case _:
                return ()

    def owners(error: Type) -> frozenset[str]:
        match error:
            case TypeAliasType(alias=TypeAlias(module=module), args=[]):
                return frozenset({module})
            case TypeAliasType(args=items) | UnionType(items=items) | TupleType(items=items) | Instance(type=TypeInfo(module_name="builtins"), args=items):
                return frozenset().union(*map(owners, items))
            case UnpackType(type=inner):
                return owners(inner)
            case Instance(type=TypeInfo(module_name=module)):
                return frozenset({module})
            case _:
                return frozenset()

    for func, error in (
        (func, result.args[1])
        for statement in node.defs
        for func in defined(statement)
        if isinstance(func.type, CallableType) and isinstance(result := get_proper_type(func.type.ret_type), Instance) and result.type.fullname == f"{Result.__module__}.{Result.__qualname__}"
    ):
        text = get_proper_type(error)
        foreign = sorted(owner for owner in owners(error) if owner.partition(".")[0] == root and not f"{node.fullname}.".startswith(f"{owner.rpartition('.')[0] or owner}."))
        if isinstance(text, Instance) and text.type.fullname == "builtins.str":
            errors.append(ErrorResultFaultError.from_node(func, f"`{func.name}` types its `Result` error as `str`, replace it with a fault `{package}` declares"))
        elif foreign:
            errors.append(
                ErrorResultFaultError.from_node(
                    func, f"`{func.name}` returns faults from {', '.join(f'`{owner}`' for owner in foreign)}, map them through `map_error` into a fault `{package}` declares"
                )
            )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ErrorResultFaultError", "check"]
